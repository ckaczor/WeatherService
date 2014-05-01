using System.Collections.Generic;
using System.ServiceModel;
using WeatherService.Devices;
using WeatherService.Remote;
using WeatherService.Values;

namespace WeatherService
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class WeatherServiceDuplex : IWeatherServiceDuplex
    {
        private static readonly List<IWeatherServiceCallback> subscribers = new List<IWeatherServiceCallback>();

        public List<DeviceBase> GetDevices()
        {
            return WeatherServiceCommon.GetDevices();
        }

        public ReadingBase GetLatestReading(string deviceAddress, WeatherValueType valueType)
        {
            return WeatherServiceCommon.GetLatestReading(deviceAddress, valueType);
        }

        public DeviceHistory GetDeviceHistory(string deviceAddress)
        {
            return WeatherServiceCommon.GetDeviceHistory(deviceAddress);
        }

        public Dictionary<DeviceBase, List<ReadingBase>> GetDeviceHistoryByValueType(WeatherValueType valueType)
        {
            return WeatherServiceCommon.GetDeviceHistoryByValueType(valueType);
        }

        public Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes)
        {
            return WeatherServiceCommon.GetWindSpeedHistory(groupIntervalMinutes);
        }

        public Dictionary<string, int> GetWindDirectionHistory()
        {
            return WeatherServiceCommon.GetWindDirectionHistory();
        }

        public bool Subscribe()
        {
            try
            {
                IWeatherServiceCallback callback = OperationContext.Current.GetCallbackChannel<IWeatherServiceCallback>();

                if (!subscribers.Contains(callback))
                    subscribers.Add(callback);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Unsubscribe()
        {
            try
            {
                IWeatherServiceCallback callback = OperationContext.Current.GetCallbackChannel<IWeatherServiceCallback>();

                if (subscribers.Contains(callback))
                    subscribers.Remove(callback);

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void FireNotification(DeviceBase device)
        {
            // Create a list of callbacks that need to be removed
            var removeList = new List<IWeatherServiceCallback>();

            // Loop over each subscriber
            foreach (var callback in subscribers)
            {
                // If the callback connection isn't open...
                if ((callback as ICommunicationObject).State != CommunicationState.Opened)
                {
                    // ...add it to the remove list and continue
                    removeList.Add(callback);
                    continue;
                }

                try
                {
                    // Make the callback
                    callback.OnDeviceUpdate(device);
                }
                catch
                {
                    // The callback failed - add it to the remove list
                    removeList.Add(callback);
                }

            }

            // Remove all callbacks in the remove list
            removeList.ForEach(o => subscribers.Remove(o));
        }
    }
}
