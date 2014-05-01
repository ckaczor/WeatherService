using System.Collections.Generic;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using WeatherService.Devices;
using WeatherService.Remote;
using WeatherService.Values;

namespace WeatherService
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WeatherService : IWeatherService
    {
        public List<DeviceBase> GetDevices()
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return WeatherServiceCommon.GetDevices();
        }

        public ReadingBase GetLatestReading(string deviceAddress, WeatherValueType valueType)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return WeatherServiceCommon.GetLatestReading(deviceAddress, valueType);
        }

        public DeviceHistory GetDeviceHistory(string deviceAddress)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return WeatherServiceCommon.GetDeviceHistory(deviceAddress);
        }

        public Dictionary<DeviceBase, List<ReadingBase>> GetDeviceHistoryByValueType(WeatherValueType valueType)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return WeatherServiceCommon.GetDeviceHistoryByValueType(valueType);
        }
        
        public Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes)
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return WeatherServiceCommon.GetWindSpeedHistory(groupIntervalMinutes);
        }

        public Dictionary<string, int> GetWindDirectionHistory()
        {
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            return WeatherServiceCommon.GetWindDirectionHistory();
        }
    }
}
