using OneWireAPI;
using System.Diagnostics;
using System.Runtime.Serialization;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class WindSpeedDevice : DeviceBase
    {
        private readonly Value _speedValue;         // Cached speed value

        private long _lastTicks;                     // Last time we checked the counter
        private uint _lastCount;                    // The count at the last check

        public WindSpeedDevice(Session session, Device device)
            : base(session, device, DeviceType.WindSpeed)
        {
            _speedValue = new Value(WeatherValueType.WindSpeed, this);

            Values.Add(WeatherValueType.WindSpeed, _speedValue);
        }

        internal override void RefreshCache()
        {
            _speedValue.SetValue(ReadSpeed());

            base.RefreshCache();
        }

        internal double ReadSpeed()
        {
            // Get a reference to the device
            var counterDevice = (DeviceFamily1D) OneWireDevice;

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
            var currentCount = counterDevice.GetCounter(15);
            var currentTicks = Stopwatch.GetTimestamp();

            // Get the time difference in seconds
            var timeDifference = (double) (currentTicks - _lastTicks) / Stopwatch.Frequency;

            // Figure out how many revolutions per second in the last interval
            var revolutionsPerSecond = ((currentCount - _lastCount) / timeDifference) / 2D;

            // Store the current time and counter				
            _lastTicks = currentTicks;
            _lastCount = currentCount;

            // Convert the revolutions per second to wind speed
            return (revolutionsPerSecond * 2.453F);
        }
    }
}