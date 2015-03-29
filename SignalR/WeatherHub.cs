using System;
using System.Linq;
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

        public List<KeyValuePair<DeviceBase, List<ReadingBase>>> GetGenericHistory(WeatherValueType valueType, DateTimeOffset start, DateTimeOffset end)
        {
            return WeatherServiceCommon.GetGenericHistory(valueType, start, end).ToList();
        }

        public List<KeyValuePair<string, List<WindSpeedReading>>> GetWindSpeedHistory(int groupIntervalMinutes, DateTimeOffset start, DateTimeOffset end)
        {
            return WeatherServiceCommon.GetWindSpeedHistory(groupIntervalMinutes, start, end).ToList();
        }

        public List<KeyValuePair<string, int>> GetWindDirectionHistory(DateTimeOffset start, DateTimeOffset end)
        {
            return WeatherServiceCommon.GetWindDirectionHistory(start, end).ToList();
        }

        public List<KeyValuePair<string, List<ReadingBase>>> GetDailySummary(WeatherValueType valueType, int deviceId, DateTime startDate, DateTime endDate)
        {
            return WeatherServiceCommon.GetDailySummary(valueType, deviceId, startDate, endDate).ToList();
        }
    }
}
