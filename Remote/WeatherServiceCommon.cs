using System;
using System.Collections.Generic;
using System.Linq;
using WeatherService.Values;
using WeatherService.Devices;

namespace WeatherService.Remote
{
    internal static class WeatherServiceCommon
    {
        public static List<DeviceBase> GetDevices()
        {
            var deviceList = Program.Session.Devices.ToList();

            return deviceList;
        }

        public static ReadingBase GetLatestReading(string deviceAddress, WeatherValueType valueType)
        {
            DeviceBase deviceBase = Program.Session.Devices.Where(d => d.Address == deviceAddress).FirstOrDefault();

            if (deviceBase == null)
                return null;

            return deviceBase.GetValue(valueType).Current;
        }

        public static DeviceHistory GetDeviceHistory(string deviceAddress)
        {
            DeviceBase deviceBase = Program.Session.Devices.Where(d => d.Address == deviceAddress).FirstOrDefault();

            if (deviceBase == null)
                return null;

            DeviceHistory deviceHistory = new DeviceHistory();

            foreach (var valueEntry in deviceBase.Values)
            {
                deviceHistory[valueEntry.Key] = valueEntry.Value.History;
            }

            return deviceHistory;
        }

        public static Dictionary<DeviceBase, List<ReadingBase>> GetDeviceHistoryByValueType(WeatherValueType valueType)
        {
            var devices = Program.Session.Devices.Where(d => d.SupportedValues.Contains(valueType));

            Dictionary<DeviceBase, List<ReadingBase>> deviceHistoryList = new Dictionary<DeviceBase, List<ReadingBase>>();

            foreach (var device in devices)
            {
                deviceHistoryList[device] = device.GetValue(valueType).History;
            }

            return deviceHistoryList;
        }

        public static Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes)
        {
            var windSpeedHistory = new Dictionary<string, List<WindSpeedReading>>();

            var device = Program.Session.Devices.Where(d => d.SupportedValues.Contains(WeatherValueType.WindSpeed)).FirstOrDefault();

            if (device == null)
                return null;

            var values = device.GetValue(WeatherValueType.WindSpeed).History;

            TimeSpan interval = new TimeSpan(0, groupIntervalMinutes, 0);

            var groupList = values.GroupBy(reading => reading.ReadTime.Ticks / interval.Ticks).Select(d => new
            {
                ReadTime = new DateTime(d.Key * interval.Ticks),
                Readings = d.Select(r => r.Value)
            });

            windSpeedHistory["Average"] = groupList.Select(d => new WindSpeedReading(WeatherValueType.WindSpeed) { ReadTime = d.ReadTime, Value = d.Readings.Average() }).ToList();
            windSpeedHistory["Minimum"] = groupList.Select(d => new WindSpeedReading(WeatherValueType.WindSpeed) { ReadTime = d.ReadTime, Value = d.Readings.Min() }).ToList();
            windSpeedHistory["Maximum"] = groupList.Select(d => new WindSpeedReading(WeatherValueType.WindSpeed) { ReadTime = d.ReadTime, Value = d.Readings.Max() }).ToList();

            return windSpeedHistory;
        }

        public static Dictionary<string, int> GetWindDirectionHistory()
        {
            var device = Program.Session.Devices.Where(d => d.SupportedValues.Contains(WeatherValueType.WindDirection)).FirstOrDefault();

            if (device == null)
                return null;

            var history = device.GetValue(WeatherValueType.WindDirection).History
                .Cast<WindDirectionReading>()
                .Where(r => r.WindDirectionValue != WindDirection.Unknown)
                .OrderBy(r => r.Value);

            var grouped = history
                .GroupBy(r => r.WindDirectionString)
                .ToDictionary(r => r.Key, r => r.Count());

            return grouped;
        }
    }
}
