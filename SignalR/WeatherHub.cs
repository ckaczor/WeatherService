using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;
using WeatherService.Devices;
using WeatherService.Remote;
using WeatherService.Reporting;
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

        public List<KeyValuePair<DeviceBase, List<ReadingBase>>> GetDeviceHistoryByValueType(WeatherValueType valueType)
        {
            return WeatherServiceCommon.GetDeviceHistoryByValueType(valueType).ToList();
        }

        public List<KeyValuePair<string, List<WindSpeedReading>>> GetWindSpeedHistory(int groupIntervalMinutes)
        {
            return WeatherServiceCommon.GetWindSpeedHistory(groupIntervalMinutes).ToList();
        }

        public List<KeyValuePair<string, int>> GetWindDirectionHistory()
        {
            return WeatherServiceCommon.GetWindDirectionHistory().ToList();
        }

        public List<DailySummary> GetDailySummary(int deviceId, int valueType, DateTime startDate, DateTime endDate)
        {
            return WeatherServiceCommon.GetDailySummary(deviceId, valueType, startDate, endDate);
        }
    }
}
