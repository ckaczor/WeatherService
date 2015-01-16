using OneWireAPI;
using System.Runtime.Serialization;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class HumidityDevice : DeviceBase
    {
        private readonly Value _temperatureValue;
        private readonly Value _humidityValue;

        public HumidityDevice(Session session, Device device)
            : base(session, device, DeviceType.Humidity)
        {
            _temperatureValue = new Value(WeatherValueType.Temperature, this);
            _humidityValue = new Value(WeatherValueType.Humidity, this);

            Values.Add(WeatherValueType.Temperature, _temperatureValue);
            Values.Add(WeatherValueType.Humidity, _humidityValue);
        }

        internal override void RefreshCache()
        {
            _temperatureValue.SetValue(ReadTemperature());
            _humidityValue.SetValue(ReadHumidity());

            base.RefreshCache();
        }

        internal double ReadTemperature()
        {
            // Cast the device to its specific type
            var device = (DeviceFamily26) OneWireDevice;

            // Return the temperature from the device
            return device.GetTemperature();
        }

        internal double ReadHumidity()
        {
            // Cast the device to its specific type
            var device = (DeviceFamily26) OneWireDevice;

            // Get the supply voltage
            var supplyVoltage = device.GetSupplyVoltage();

            // Get the output voltage
            var outputVoltage = device.GetOutputVoltage();

            // Get the temperature
            var temperature = device.GetTemperature();

            // Calculate the humidity
            var humidity = (((outputVoltage / supplyVoltage) - 0.16F) / 0.0062F) / (1.0546F - 0.00216F * temperature);

            // Return the result
            return humidity;
        }
    }
}