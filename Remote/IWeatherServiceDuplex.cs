using System;
using System.Collections.Generic;
using System.ServiceModel;
using WeatherService.Devices;
using WeatherService.Values;

namespace WeatherService.Remote
{
    [ServiceContract(CallbackContract = typeof(IWeatherServiceCallback))]
    public interface IWeatherServiceDuplex
    {
        [OperationContract]
        List<DeviceBase> GetDevices();

        [OperationContract]
        bool Subscribe();

        [OperationContract]
        bool Unsubscribe();

        [OperationContract]
        Dictionary<DeviceBase, List<ReadingBase>> GetGenericHistory(WeatherValueType valueType, DateTimeOffset start, DateTimeOffset end);

        [OperationContract]
        Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes, DateTimeOffset start, DateTimeOffset end);

        [OperationContract]
        Dictionary<string, int> GetWindDirectionHistory(DateTimeOffset start, DateTimeOffset end);
    }
}
