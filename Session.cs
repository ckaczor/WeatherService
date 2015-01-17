using Common.Debug;
using Common.Extensions;
using Microsoft.AspNet.SignalR;
using OneWireAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WeatherService.Devices;
using WeatherService.Remote;
using WeatherService.SignalR;

namespace WeatherService
{
    public class Session
    {
        #region Member variables

        private Thread _thread;
        private IHubContext _hubContext;

        private static volatile bool _terminateThread;        

        #endregion

        #region Events
      
        private void FireDeviceRefreshed(DeviceBase device)
        {
            WeatherServiceDuplex.FireNotification(device);

            _hubContext.Clients.All.DeviceRefreshed(device);
        }

        #endregion

        #region Constructor

        public Session()
        {
            // Initialize the database
            // DB CHANGE - Database.Initialize(Settings.Default.DatabaseFile);

            // Create the list of devices
            Devices = new List<DeviceBase>();

            // Create a new session
            OneWireSession = new OneWireAPI.Session();
        }

        #endregion

        #region Private methods

        private void ThreadProcedure()
        {
            while (!_terminateThread)
            {
                //FireRefreshStarted();

                OneWireSession.Network.Initialize();

                foreach (DeviceBase device in Devices)
                {
                    try
                    {
                        DeviceOperations++;

                        // Refresh the cached data for this device
                        bool refreshed = device.DoCacheRefresh();

                        // Fire event if device was actually refreshed
                        if (refreshed)
                        {
                            FireDeviceRefreshed(device);
                            device.Operations++;
                        }
                    }
                    catch (OneWireException exception)
                    {
                        DeviceErrors++;
                        device.Errors++;

                        // TODO - Error event
                        Tracer.WriteLine(String.Format("{0} - Error in device {1}: {2}", DateTime.Now, exception.DeviceId.Name, exception.Message));
                    }
                    catch (Exception exception)
                    {
                        // TODO - Error event
                        Tracer.WriteLine(String.Format("{0} - Unknown error: {1}", DateTime.Now, exception.Message));

                        // Rethrow the exception
                        throw;
                    }

                    if (_terminateThread)
                        break;
                }

                // Fire the refresh completed event
                //FireRefreshCompleted();

                // Sleep the thread for a second - we don't need a hard loop
                Thread.Sleep(Settings.Default.PollingInterval);
            }
        }

        private void AddWeatherDevice(Device device)
        {
            // TODO - Handle device mapping for multiple devices that use the same family code

            switch (device.Family)
            {
                case 0x12:
                    TMEX.FileEntry fileEntry = new TMEX.FileEntry();        // Entry to describe a file
                    byte[] fileData = new byte[8];                          // File data
                    Identifier deviceID;                                  // Identifier for the other device

                    // Select this device
                    Adapter.Select(device.Id);

                    // Setup to try to open the pressure sensor file
                    fileEntry.Name = System.Text.Encoding.ASCII.GetBytes("8570");
                    fileEntry.Extension = 0;

                    // Try to open the file
                    short fileHandle = TMEX.TMOpenFile(OneWireSession.SessionHandle, OneWireSession.StateBuffer, ref fileEntry);

                    // If a file was found then try to read it
                    if (fileHandle >= 0)
                    {
                        // Read the file to get the ID of the other device
                        TMEX.TMReadFile(OneWireSession.SessionHandle, OneWireSession.StateBuffer, fileHandle, fileData, (short) fileData.Length);

                        // Close the file
                        TMEX.TMCloseFile(OneWireSession.SessionHandle, OneWireSession.StateBuffer, fileHandle);

                        // Create an ID so we can get the string name
                        deviceID = new Identifier(fileData);

                        // Find the other device
                        Device otherDevice = (Device) OneWireSession.Network.Devices[deviceID.Name];

                        // Create a new pressure device and it to the the list
                        Devices.Add(new PressureDevice(this, device, otherDevice));
                    }

                    break;

                case 0x10:
                    // Create a new temperature device and add it to the list
                    Devices.Add(new TemperatureDevice(this, device));
                    break;

                case 0x1D:
                    if (device.Id.Name == "4000000004A4081D")
                    {
                        // Create a new rain device and add it to the list
                        Devices.Add(new RainDevice(this, device));
                    }
                    else
                    {
                        // Create a new wind speed device and add it to the list
                        Devices.Add(new WindSpeedDevice(this, device));
                    }

                    break;

                case 0x20:
                    // Create a new wind direction device and add it to the list
                    Devices.Add(new WindDirectionDevice(this, device));
                    break;

                case 0x26:
                    // Create a new humidity device and add it to the list
                    Devices.Add(new HumidityDevice(this, device));
                    break;
            }

        }

        private void LoadWeatherDevices()
        {
            // Add each 1-wire device as a weather device
            OneWireSession.Network.Devices.Values.ToList().ForEach(d => AddWeatherDevice(d));
        }

        #endregion

        #region Public methods

        public void Initialize()
        {
            // Create the SignalR hub context
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<WeatherHub>();

            // Try 5 times to aquire a session
            for (int i = 0; i < 5; i++)
            {
                // Get the session going
                bool result = OneWireSession.Acquire();

                // If successful then break out
                if (result)
                    break;

                // Wait for a little bit
                Thread.Sleep(1000);
            }

            // Hook events for new devices
            OneWireSession.Network.DeviceAdded += HandleDeviceAdded;

            // Load the list of weather devices
            LoadWeatherDevices();

            // Fire the initialized event
            //FireInitialized();
        }      

        private void HandleDeviceAdded(Device device)
        {
            AddWeatherDevice(device);
        }

        public void Terminate()
        {
            // Fire the terminated event
            //FireTerminated();

            // Make sure the refresh thread has stopped
            StopRefresh();

            // Release the session
            if (OneWireSession != null)
                OneWireSession.Release();

            OneWireSession = null;
        }

        public void StartRefresh()
        {
            // Create the cache refresh thread
            _thread = new Thread(ThreadProcedure);

            // Start the thread
            _thread.Start();
        }

        public void StopRefresh()
        {
            if (_thread == null)
                return;

            // Terminate the thread on the next check
            _terminateThread = true;

            // Wait for the thread to terminate
            _thread.Join();

            _thread = null;
        }

        #endregion

        #region Properties

        public List<DeviceBase> Devices { get; private set; }

        public long DeviceOperations { get; private set; }

        public long DeviceErrors { get; private set; }

        internal OneWireAPI.Session OneWireSession { get; private set; }

        #endregion
    }
}
