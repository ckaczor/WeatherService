using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WeatherService.Data;
using WeatherService.Values;
using WeatherService.Devices;
using WeatherService.Reporting;

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
            var deviceBase = Program.Session.Devices.FirstOrDefault(d => d.Address == deviceAddress);

            if (deviceBase == null)
                return null;

            return deviceBase.GetValue(valueType).Current;
        }

        public static DeviceHistory GetDeviceHistory(string deviceAddress)
        {
            var deviceBase = Program.Session.Devices.FirstOrDefault(d => d.Address == deviceAddress);

            if (deviceBase == null)
                return null;

            var deviceHistory = new DeviceHistory();

            foreach (var valueEntry in deviceBase.Values)
            {
                deviceHistory[valueEntry.Key] = valueEntry.Value.History;
            }

            return deviceHistory;
        }

        public static Dictionary<DeviceBase, List<ReadingBase>> GetDeviceHistoryByValueType(WeatherValueType valueType)
        {
            var devices = Program.Session.Devices.Where(d => d.SupportedValues.Contains(valueType));

            var deviceHistoryList = new Dictionary<DeviceBase, List<ReadingBase>>();

            foreach (var device in devices)
            {
                deviceHistoryList[device] = device.GetValue(valueType).History;
            }

            return deviceHistoryList;
        }

        public static Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes)
        {
            var windSpeedHistory = new Dictionary<string, List<WindSpeedReading>>();

            var device = Program.Session.Devices.FirstOrDefault(d => d.SupportedValues.Contains(WeatherValueType.WindSpeed));

            if (device == null)
                return null;

            var values = device.GetValue(WeatherValueType.WindSpeed).History;

            var interval = new TimeSpan(0, groupIntervalMinutes, 0);

            var groupList = values.GroupBy(reading => reading.ReadTime.Ticks / interval.Ticks).Select(d => new
            {
                ReadTime = new DateTime(d.Key * interval.Ticks),
                Readings = d.Select(r => r.Value)
            }).ToList();

            windSpeedHistory["Average"] = groupList.Select(d => new WindSpeedReading(WeatherValueType.WindSpeed) { ReadTime = d.ReadTime, Value = d.Readings.Average() }).ToList();
            windSpeedHistory["Minimum"] = groupList.Select(d => new WindSpeedReading(WeatherValueType.WindSpeed) { ReadTime = d.ReadTime, Value = d.Readings.Min() }).ToList();
            windSpeedHistory["Maximum"] = groupList.Select(d => new WindSpeedReading(WeatherValueType.WindSpeed) { ReadTime = d.ReadTime, Value = d.Readings.Max() }).ToList();

            return windSpeedHistory;
        }

        public static Dictionary<string, int> GetWindDirectionHistory()
        {
            var device = Program.Session.Devices.FirstOrDefault(d => d.SupportedValues.Contains(WeatherValueType.WindDirection));

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

        public static List<DailySummary> GetDailySummary(int deviceId, int valueType, DateTime startDate, DateTime endDate)
        {
            var summaryList = new List<DailySummary>();

            for (var year = startDate.Year; year <= endDate.Year; year++)
            {
                using (var archiveData = new WeatherArchiveData(year))
                {
                    var groupedReadings = archiveData.Readings
                        .Where(r => r.ReadTime >= startDate &&
                                    r.ReadTime <= endDate &&
                                    r.DeviceId == deviceId &&
                                    r.Type == valueType)
                        .GroupBy(r => DbFunctions.TruncateTime(r.ReadTime))
                        .Select(r => new DailySummary
                        {
                            Date = r.Key,
                            Count = r.Count(),
                            Minimum = r.Min(v => v.Value),
                            Maximum = r.Max(v => v.Value),
                            Average = r.Average(v => v.Value)
                        })
                        .OrderBy(d => d.Date);

                    summaryList.AddRange(groupedReadings);
                }
            }

            return summaryList;
        }
    }
}
