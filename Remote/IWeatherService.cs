using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using WeatherService.Devices;
using WeatherService.Remote;
using WeatherService.Values;

namespace WeatherService
{
    [ServiceContract]
    public interface IWeatherService
    {
        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        List<DeviceBase> GetDevices();

        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ReadingBase GetLatestReading(string deviceAddress, WeatherValueType valueType);

        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        DeviceHistory GetDeviceHistory(string deviceAddress);

        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        Dictionary<DeviceBase, List<ReadingBase>> GetDeviceHistoryByValueType(WeatherValueType valueType);

        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes);

        [WebGet(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        Dictionary<string, int> GetWindDirectionHistory();
    }
}
