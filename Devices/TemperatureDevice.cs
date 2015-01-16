using OneWireAPI;
using System.Runtime.Serialization;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class TemperatureDevice : DeviceBase
    {
        private readonly Value _temperatureValue;

        public TemperatureDevice(Session session, Device device)
            : base(session, device, DeviceType.Temperature)
        {
            // Create the new value object
            _temperatureValue = new Value(WeatherValueType.Temperature, this);

            Values.Add(WeatherValueType.Temperature, _temperatureValue);
        }

        internal override void RefreshCache()
        {
            // Read the current temperature
            _temperatureValue.SetValue(ReadTemperature());

            base.RefreshCache();
        }

        internal double ReadTemperature()
        {
            // Cast the device to its specific type
            var temperatureDevice = (DeviceFamily10) OneWireDevice;

            // Return the temperature from the device
            return temperatureDevice.GetTemperature();
        }
    }
}