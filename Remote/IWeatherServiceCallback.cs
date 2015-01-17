using System.ServiceModel;
using WeatherService.Devices;

namespace WeatherService.Remote
{
    public interface IWeatherServiceCallback : ICommunicationObject
    {
        [OperationContract(IsOneWay = true)]
        void OnDeviceUpdate(DeviceBase device);
    }
}
