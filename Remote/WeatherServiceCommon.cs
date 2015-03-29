using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using WeatherService.Data;
using WeatherService.Values;
using WeatherService.Devices;

namespace WeatherService.Remote
{
    internal static class WeatherServiceCommon
    {
        private static List<ReadingBase> LoadHistory(WeatherValueType valueType, int deviceId, DateTimeOffset start, DateTimeOffset end)
        {
            var history = new List<ReadingBase>();

            for (var year = start.Year; year <= end.Year; year++)
            {
                using (var archiveData = new WeatherArchiveData(year))
                {
                    var readings = archiveData.Readings;

                    var yearlyHistory = readings.Where(r => r.DeviceId == deviceId && r.Type == (int) valueType && r.ReadTime >= start && r.ReadTime <= end).ToList();

                    history.AddRange(yearlyHistory.Select(r => ReadingBase.CreateReading(valueType, r.ReadTime.DateTime, r.Value)));
                }
            }

            return history;
        }

        public static List<DeviceBase> GetDevices()
        {
            var deviceList = Program.Session.Devices.ToList();

            return deviceList;
        }

        public static Dictionary<DeviceBase, List<ReadingBase>> GetGenericHistory(WeatherValueType valueType, DateTimeOffset start, DateTimeOffset end)
        {
            var devices = Program.Session.Devices.Where(d => d.SupportedValues.Contains(valueType));

            var deviceHistoryList = new Dictionary<DeviceBase, List<ReadingBase>>();            

            foreach (var device in devices)
            {
                deviceHistoryList[device] = LoadHistory(valueType, device.Id, start, end);
            }

            return deviceHistoryList;
        }

        public static Dictionary<string, List<WindSpeedReading>> GetWindSpeedHistory(int groupIntervalMinutes, DateTimeOffset start, DateTimeOffset end)
        {
            var windSpeedHistory = new Dictionary<string, List<WindSpeedReading>>();

            var device = Program.Session.Devices.FirstOrDefault(d => d.SupportedValues.Contains(WeatherValueType.WindSpeed));

            if (device == null)
                return null;

            var values = LoadHistory(WeatherValueType.WindSpeed, device.Id, start, end);

            var interval = new TimeSpan(0, groupIntervalMinutes, 0);

            var groupList = values.GroupBy(reading => reading.ReadTime.Ticks / interval.Ticks).Select(d => new
            {
                ReadTime = new DateTime(d.Key * interval.Ticks),
                Readings = d.Select(r => r.Value)
            }).ToList();

            windSpeedHistory["Average"] = groupList.Select(d => new WindSpeedReading() { ReadTime = d.ReadTime, Value = d.Readings.Average() }).ToList();
            windSpeedHistory["Minimum"] = groupList.Select(d => new WindSpeedReading() { ReadTime = d.ReadTime, Value = d.Readings.Min() }).ToList();
            windSpeedHistory["Maximum"] = groupList.Select(d => new WindSpeedReading() { ReadTime = d.ReadTime, Value = d.Readings.Max() }).ToList();

            return windSpeedHistory;
        }

        public static Dictionary<string, int> GetWindDirectionHistory(DateTimeOffset start, DateTimeOffset end)
        {
            var device = Program.Session.Devices.FirstOrDefault(d => d.SupportedValues.Contains(WeatherValueType.WindDirection));

            if (device == null)
                return null;

            var values = LoadHistory(WeatherValueType.WindDirection, device.Id, start, end);

            var history = values
                .Cast<WindDirectionReading>()
                .Where(r => r.WindDirectionValue != WindDirection.Unknown)
                .OrderBy(r => r.Value);

            var grouped = history
                .GroupBy(r => r.WindDirectionString)
                .ToDictionary(r => r.Key, r => r.Count());

            return grouped;
        }

        public static Dictionary<string, List<ReadingBase>> GetDailySummary(WeatherValueType valueType, int deviceId, DateTime startDate, DateTime endDate)
        {
            var summaryList = new Dictionary<string, List<ReadingBase>>();

            summaryList["Average"] = new List<ReadingBase>();
            summaryList["Minimum"] = new List<ReadingBase>();
            summaryList["Maximum"] = new List<ReadingBase>();

            for (var year = startDate.Year; year <= endDate.Year; year++)
            {
                using (var archiveData = new WeatherArchiveData(year))
                {
                    var groupList = archiveData.Readings
                        .Where(r => r.ReadTime >= startDate &&
                                    r.ReadTime <= endDate &&
                                    r.DeviceId == deviceId &&
                                    r.Type == (int) valueType)
                        .GroupBy(r => DbFunctions.TruncateTime(r.ReadTime))
                        .Select(g => new
                        {
                            ReadTime = g.Key,
                            Readings = g.Select(r => r.Value)
                        }).ToList();

                    summaryList["Average"].AddRange(groupList.Select(d => ReadingBase.CreateReading(valueType, d.ReadTime.Value.DateTime, d.Readings.Average())));
                    summaryList["Minimum"].AddRange(groupList.Select(d => ReadingBase.CreateReading(valueType, d.ReadTime.Value.DateTime, d.Readings.Min())));
                    summaryList["Maximum"].AddRange(groupList.Select(d => ReadingBase.CreateReading(valueType, d.ReadTime.Value.DateTime, d.Readings.Max())));
                }
            }

            return summaryList;
        }
    }
}
