using System.Runtime.Serialization;
using OneWireAPI;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class HumidityDevice : DeviceBase
    {
        #region Member variables

        private readonly Value _temperatureValue;
        private readonly Value _humidityValue;

        #endregion

        #region Constructor

        public HumidityDevice(Session session, owDevice device) : base(session, device, DeviceType.Humidity)
        {
            _temperatureValue = new Value(WeatherValueType.Temperature, this);
            _humidityValue = new Value(WeatherValueType.Humidity, this);

            _valueList.Add(WeatherValueType.Temperature, _temperatureValue);
            _valueList.Add(WeatherValueType.Humidity, _humidityValue);
        }

        #endregion

        #region Internal methods

        internal override void RefreshCache()
        {
            _temperatureValue.SetValue(ReadTemperature());
            _humidityValue.SetValue(ReadHumidity());

            base.RefreshCache();
        }

        internal double ReadTemperature()
        {
            // Cast the device to its specific type
            owDeviceFamily26 oDevice = (owDeviceFamily26) _device;

            // Return the temperature from the device
            return oDevice.GetTemperature();
        }

        internal double ReadHumidity()
        {
            // Cast the device to its specific type
            owDeviceFamily26 oDevice = (owDeviceFamily26) _device;

            // Get the supply voltage
            double dSupplyVoltage = oDevice.GetSupplyVoltage();

            // Get the output voltage
            double dOutputVoltage = oDevice.GetOutputVoltage();

            // Get the temperature
            double dTemperature = oDevice.GetTemperature();

            // Calculate the humidity
            double dHumidity = (((dOutputVoltage / dSupplyVoltage) - 0.16F) / 0.0062F) / (1.0546F - 0.00216F * dTemperature);

            // Return the result
            return dHumidity;
        }

        #endregion
    }
}