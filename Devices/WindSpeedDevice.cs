using System.Diagnostics;
using System.Runtime.Serialization;
using OneWireAPI;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class WindSpeedDevice : DeviceBase
    {
        #region Member variables 

        private readonly Value _speedValue;         // Cached speed value

        private long _lastTicks;                     // Last time we checked the counter
        private uint _lastCount;                    // The count at the last check

        #endregion

        #region Constructor

        public WindSpeedDevice(Session session, owDevice device) : base(session, device, DeviceType.WindSpeed)
        {
            _speedValue = new Value(WeatherValueType.WindSpeed, this);

            _valueList.Add(WeatherValueType.WindSpeed, _speedValue);
        }

        #endregion

        #region Internal methods

        internal override void RefreshCache()
        {
            _speedValue.SetValue(ReadSpeed());

            base.RefreshCache();
        }

        internal double ReadSpeed()
        {
            // Get a reference to the device
            owDeviceFamily1D counterDevice = (owDeviceFamily1D) _device;

            // Special case if we have never read before
            if (_lastTicks == 0)
            {
                // Initialize the last data to the data now
                _lastTicks = Stopwatch.GetTimestamp();
                _lastCount = counterDevice.GetCounter(15);

                // Wait for a second
                System.Threading.Thread.Sleep(1000);
            }

            // Get the current counter and time
            uint currentCount = counterDevice.GetCounter(15);
            long currentTicks = Stopwatch.GetTimestamp();

            // Get the time difference in seconds
            double timeDifference = (double) (currentTicks - _lastTicks) / Stopwatch.Frequency;

            // Figure out how many revolutions per second in the last interval
            double revolutionsPerSecond = ((currentCount - _lastCount) / timeDifference) / 2D;

            // Store the current time and counter				
            _lastTicks = currentTicks;
            _lastCount = currentCount;

            // Convert the revolutions per second to wind speed
            return (revolutionsPerSecond * 2.453F);
        }

        #endregion
    }
}