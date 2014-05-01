using System.ServiceModel;
using WeatherService.Devices;

namespace WeatherService
{
    public interface IWeatherServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnDeviceUpdate(DeviceBase device);
    }
}
