using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using WeatherService.Devices;
using WeatherService.Remote;
using WeatherService.Values;

namespace WeatherService.SignalR
{
    public class WeatherHub : Hub
    {
        public List<DeviceBase> GetDevices()
        {
            return WeatherServiceCommon.GetDevices();
        }

        public ReadingBase GetLatestReading(string deviceAddress, WeatherValueType valueType)
        {
            return WeatherServiceCommon.GetLatestReading(deviceAddress, valueType);
        }

        public DeviceHistory GetDeviceHistory(string deviceAddress)
        {
            return WeatherServiceCommon.GetDeviceHistory(deviceAddress);
        }

        public Dictionary<DeviceBase, List<ReadingBase>> GetDeviceHistoryByValueType(WeatherValueType valueType)
        {
            return WeatherServiceCommon.GetDeviceHistoryByValueType(valueType);
        }

        public Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes)
        {
            return WeatherServiceCommon.GetWindSpeedHistory(groupIntervalMinutes);
        }

        public Dictionary<string, int> GetWindDirectionHistory()
        {
            return WeatherServiceCommon.GetWindDirectionHistory();
        }
    }
}
