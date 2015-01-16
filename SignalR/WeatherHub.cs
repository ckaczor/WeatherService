using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using WeatherService.Devices;
using WeatherService.Remote;

namespace WeatherService.SignalR
{
    public class WeatherHub : Hub
    {
        public List<DeviceBase> GetDevices()
        {
            var devices = WeatherServiceCommon.GetDevices();

            //var json = JsonConvert.SerializeObject(devices);

            return devices;
        }
    }
}
