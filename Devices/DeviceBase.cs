using OneWireAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WeatherService.Data;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public enum DeviceType
    {
        [EnumMember]
        None,

        [EnumMember]
        Temperature,

        [EnumMember]
        Pressure,

        [EnumMember]
        Humidity,

        [EnumMember]
        WindDirection,

        [EnumMember]
        WindSpeed,

        [EnumMember]
        Rain
    }

    [DataContract]
    [KnownType(typeof(HumidityDevice))]
    [KnownType(typeof(PressureDevice))]
    [KnownType(typeof(RainDevice))]
    [KnownType(typeof(TemperatureDevice))]
    [KnownType(typeof(WindDirectionDevice))]
    [KnownType(typeof(WindSpeedDevice))]
    public class DeviceBase
    {
        protected Session Session;

        protected internal Device OneWireDevice { get; protected set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public DateTime LastRead { get; set; }

        [DataMember]
        public long Operations { get; set; }

        [DataMember]
        public long Errors { get; set; }

        [DataMember]
        public DeviceType Type { get; set; }

        [DataMember]
        public int RefreshFrequency { get; set; }

        [DataMember]
        public bool Indoor { get; set; }

        [DataMember]
        public Dictionary<WeatherValueType, Value> Values { get; protected set; }

        public DeviceBase(Session session, Device device, DeviceType deviceType)
        {
            LastRead = DateTime.MinValue;

            // Initialize the value list
            Values = new Dictionary<WeatherValueType, Value>();

            // Store properties of the device
            Address = device.Id.Name;
            Type = deviceType;
            Session = session;
            OneWireDevice = device;

            // Default the display name
            DisplayName = device.Id.Name;

            // Default the read interval
            RefreshFrequency = (Type == DeviceType.WindDirection || Type == DeviceType.WindSpeed) ? 1 : 120;

            // Load device data
            var load = Load();

            // If we couldn't load data then save the default data
            if (!load)
                Save();
        }

        internal bool Load()
        {
            try
            {
                using (var weatherData = new WeatherData())
                {
                    // Get the device data from the database
                    var deviceData = weatherData.Devices.FirstOrDefault(d => d.Address == Address);

                    if (deviceData == null)
                        return false;

                    // Load the device data
                    Id = deviceData.Id;
                    DisplayName = deviceData.Name;
                    RefreshFrequency = deviceData.ReadInterval;
                    Indoor = deviceData.Indoor;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal bool Save()
        {
            using (var weatherData = new WeatherData())
            {
                // Get the device data from the database
                var deviceData = weatherData.Devices.FirstOrDefault(d => d.Address == Address);

                if (deviceData == null)
                    return false;

                // Save device data
                deviceData.Name = DisplayName;
                deviceData.ReadInterval = RefreshFrequency;
                deviceData.Indoor = Indoor;

                weatherData.SaveChanges();
            }

            return true;
        }

        internal bool DoCacheRefresh()
        {
            switch (RefreshFrequency)
            {
                case -1:
                    // Do not refresh this device
                    return false;

                case 0:
                    // Refresh the device whenever possible
                    RefreshCache();

                    return true;

                default:
                    if (LastRead == DateTime.MinValue)
                    {
                        // If we have never refreshed before then do it now
                        RefreshCache();

                        return true;
                    }

                    // Get the time since the last refresh
                    var timeSpan = DateTime.Now - LastRead;

                    // If it has been long enough then refresh the cache
                    if (timeSpan.TotalSeconds >= RefreshFrequency)
                    {
                        RefreshCache();

                        return true;
                    }

                    return false;
            }
        }

        internal virtual void RefreshCache()
        {
            // Store the current time
            LastRead = DateTime.Now;
        }

        [DataMember]
        public List<WeatherValueType> SupportedValues
        {
            get { return Values.Keys.ToList(); }
        }

        public Value GetValue(WeatherValueType valueType)
        {
            return Values[valueType];
        }
    }
}