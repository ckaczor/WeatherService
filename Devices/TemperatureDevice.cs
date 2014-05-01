using System.Runtime.Serialization;
using OneWireAPI;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class TemperatureDevice : DeviceBase
    {
        #region Member variables
			
        private readonly Value _temperatureValue;       // Cached temperature

        #endregion

        #region Constructor

        public TemperatureDevice(Session session, owDevice device) : base(session, device, DeviceType.Temperature)
        {
            // Create the new value object
            _temperatureValue = new Value(WeatherValueType.Temperature, this);

            _valueList.Add(WeatherValueType.Temperature, _temperatureValue);
        }

        #endregion

        #region Internal methods

        internal override void RefreshCache()
        {
            // Read the current temperature
            _temperatureValue.SetValue(ReadTemperature());

            base.RefreshCache();
        }

        internal double ReadTemperature()
        {
            // Cast the device to its specific type
            owDeviceFamily10 temperatureDevice = (owDeviceFamily10) _device;

            // Return the temperature from the device
            return temperatureDevice.GetTemperature();
        }

        #endregion
    }
}