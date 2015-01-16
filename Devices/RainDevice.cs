using OneWireAPI;
using System.Runtime.Serialization;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class RainDevice : DeviceBase
    {
        private readonly Value _recentValue;

        private readonly uint _clearedCount;        // Counter at least clear

        private uint _lastCount;                    // Counter from last check
        private uint _startupCount;                 // Counter at startup
        private bool _initialized;

        public RainDevice(Session session, Device device)
            : base(session, device, DeviceType.Rain)
        {
            // TODO - Read the count as of the last counter clearing
            _clearedCount = 150;

            // Get a reference to the device
            var counter = (DeviceFamily1D) OneWireDevice;

            // Get the current counter
            _lastCount = counter.GetCounter(15);

            // Setup the rain value
            _recentValue = new Value(WeatherValueType.Rain, this);
            Values.Add(WeatherValueType.Rain, _recentValue);
        }

        internal override void RefreshCache()
        {
            var recentRain = ReadRecentRainfall();

            _recentValue.SetValue(recentRain);

            base.RefreshCache();
        }

        internal double ReadSessionRainfall()
        {
            // Get a reference to the device
            var counter = (DeviceFamily1D) OneWireDevice;

            // If we haven't initialized then do it now
            if (!_initialized)
            {
                // Get the current counter
                _startupCount = counter.GetCounter(15);

                _initialized = true;
            }

            // Get the current counter
            var currentCount = counter.GetCounter(15);

            // Get the amount of rain since the last check
            return (currentCount - _startupCount) * 0.2;
        }

        internal double ReadRecentRainfall()
        {
            // Get a reference to the device
            var counter = (DeviceFamily1D) OneWireDevice;

            // Get the current counter
            var currentCount = counter.GetCounter(15);

            // Get the amount of rain since the last check
            var rainValue = (currentCount - _lastCount) * 0.2;

            // Store the last counter
            _lastCount = currentCount;

            return rainValue;
        }

        internal double ReadTotalRainfall()
        {
            // Get a reference to the device
            var counter = (DeviceFamily1D) OneWireDevice;

            // Get the current counter
            var currentCount = counter.GetCounter(15);

            // Get the amount of rain since the last check
            double rainValue = (currentCount - _clearedCount) * 0.2F;

            // Store the last counter
            _lastCount = currentCount;

            return rainValue;
        }
    }
}