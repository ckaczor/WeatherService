using System;
using System.Linq;
using System.Runtime.Serialization;
using OneWireAPI;
using System.Collections.Generic;
using WeatherService.Data;
using WeatherService.Values;

namespace WeatherService.Devices
{
    #region Enumerations

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

    #endregion

    [DataContract]
    [KnownType(typeof(HumidityDevice))]
    [KnownType(typeof(PressureDevice))]
    [KnownType(typeof(RainDevice))]
    [KnownType(typeof(TemperatureDevice))]
    [KnownType(typeof(WindDirectionDevice))]
    [KnownType(typeof(WindSpeedDevice))]
    public class DeviceBase
    {
        #region Member variables

        protected int _deviceId;                                        // Device ID
        protected string _deviceAddress;                                // Device key
        protected Session _session;                                     // The root session
        protected owDevice _device;                                     // The one wire device
        protected DateTime _lastRead = DateTime.MinValue;               // When was the last refresh?
        protected int _refreshFrequency;                                // How often should we refresh?
        protected Dictionary<WeatherValueType, Value> _valueList;       // List of values		
        protected string _displayName;                                  // Device display name
        protected DeviceType _deviceType;                               // Type of device
        protected long _operationCount;                                 // Number of operations
        protected long _errorCount;                                     // Number of errors

        #endregion

        #region Constructor

        public DeviceBase(Session session, owDevice device, DeviceType deviceType)
        {
            // Initialize the value list
            _valueList = new Dictionary<WeatherValueType, Value>();

            // Store properties of the device
            _deviceAddress = device.ID.Name;
            _deviceType = deviceType;
            _session = session;
            _device = device;

            // Default the display name
            _displayName = device.ID.Name;

            // Default the read interval
            if (Type == DeviceType.WindDirection || Type == DeviceType.WindSpeed)
                _refreshFrequency = 1;
            else
                _refreshFrequency = 120;

            // Load device data
            bool bLoad = Load();

            // If we couldn't load data then save the default data
            if (!bLoad)
                Save();
        }

        #endregion

        #region Save and load

        internal bool Load()
        {
            try
            {
                // Get the device data from the database
                DeviceData deviceData = (from device in Database.DeviceTable where device.Address == _deviceAddress select device).First();

                // Load the device data
                _deviceId = deviceData.Id;
                _displayName = deviceData.Name;
                _refreshFrequency = deviceData.ReadInterval;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal bool Save()
        {
            // Get the device data from the database
            DeviceData deviceData = (from device in Database.DeviceTable where device.Address == _deviceAddress select device).First();

            // Save device data
            deviceData.Name = _displayName;
            deviceData.ReadInterval = _refreshFrequency;

            return true;
        }

        #endregion

        #region Internal cache refresh logic

        internal bool DoCacheRefresh()
        {
            switch (_refreshFrequency)
            {
                case -1:
                    // Do not refresh this device
                    return false;

                case 0:
                    // Refresh the device whenever possible
                    RefreshCache();

                    return true;

                default:
                    if (_lastRead == DateTime.MinValue)
                    {
                        // If we have never refreshed before then do it now
                        RefreshCache();

                        return true;
                    }

                    // Get the time since the last refresh
                    TimeSpan oTimeSpan = DateTime.Now - _lastRead;

                    // If it has been long enough then refresh the cache
                    if (oTimeSpan.TotalSeconds >= _refreshFrequency)
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
            _lastRead = DateTime.Now;
        }

        #endregion

        #region Public properties

        [DataMember]
        public int Id
        {
            get { return _deviceId; }
            set { _deviceId = value; }
        }

        [DataMember]
        public string Address
        {
            get { return _deviceAddress; }
            set { _deviceAddress = value; }
        }

        [DataMember]
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        internal owDevice OneWireDevice
        {
            get { return _device; }
        }

        [DataMember]
        public DateTime LastRead
        {
            get { return _lastRead; }
            set { _lastRead = value; }
        }

        [DataMember]
        public long Operations
        {
            get { return _operationCount; }
            set { _operationCount = value; }
        }

        [DataMember]
        public long Errors
        {
            get { return _errorCount; }
            set { _errorCount = value; }
        }

        [DataMember]
        public DeviceType Type
        {
            get { return _deviceType; }
            set { _deviceType = value; }
        }

        [DataMember]
        public int RefreshFrequency
        {
            get { return _refreshFrequency; }
            set { _refreshFrequency = value; }
        }

        [DataMember]
        public Dictionary<WeatherValueType, Value> Values
        {
            get { return _valueList; }
        }

        [DataMember]
        public List<WeatherValueType> SupportedValues
        {
            get { return _valueList.Keys.ToList(); }   
        }

        #endregion

        #region Public methods

        public Value GetValue(WeatherValueType valueType)
        {
            return _valueList[valueType];
        }

        #endregion
    }
}