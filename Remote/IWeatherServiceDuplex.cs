using System.Collections.Generic;
using System.ServiceModel;
using WeatherService.Devices;
using WeatherService.Remote;
using WeatherService.Values;

namespace WeatherService
{
    [ServiceContract(CallbackContract = typeof(IWeatherServiceCallback))]
    public interface IWeatherServiceDuplex
    {
        [OperationContract]
        List<DeviceBase> GetDevices();

        [OperationContract]
        ReadingBase GetLatestReading(string deviceAddress, WeatherValueType valueType);

        [OperationContract]
        bool Subscribe();

        [OperationContract]
        bool Unsubscribe();

        [OperationContract]
        DeviceHistory GetDeviceHistory(string deviceAddress);

        [OperationContract]
        Dictionary<DeviceBase, List<ReadingBase>> GetDeviceHistoryByValueType(WeatherValueType valueType);

        [OperationContract]
        Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes);

        [OperationContract]
        Dictionary<string, int> GetWindDirectionHistory();
    }
}
