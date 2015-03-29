using System;
using System.Runtime.Serialization;
using WeatherService.Data;
using WeatherService.Devices;

namespace WeatherService.Values
{
    [DataContract]
    public enum WeatherValueType
    {
        [EnumMember]
        Temperature,

        [EnumMember]
        Pressure,

        [EnumMember]
        Humidity,

        [EnumMember]
        WindSpeed,

        [EnumMember]
        WindDirection,

        [EnumMember]
        Rain
    }

    /// <summary>
    /// Stores information for a particular device value
    /// </summary>
    [DataContract]
    public class Value
    {
        #region Member variables

        private readonly DeviceBase _ownerDevice;                   // Owner device

        #endregion

        #region Constructor

        public Value(WeatherValueType valueType, DeviceBase ownerDevice)
        {
            // Remember information we were given
            ValueType = valueType;
            _ownerDevice = ownerDevice;

            // Create the readings
            Current = ReadingBase.CreateReading(ValueType);
        }

        #endregion

        #region Internal Methods

        internal void SetValue(double value)
        {
            // Set the value with the current time as the timestamp
            SetValue(value, DateTime.Now);
        }

        internal void SetValue(double value, DateTime timeStamp)
        {
            // Set the value with the current time as the timestamp - save the state
            SetValue(value, DateTime.Now, true);
        }

        internal void SetValue(double value, DateTime timeStamp, bool save)
        {
            // Set the current value
            Current.SetValue(value, timeStamp);

            if (save)
            {
                // Save the reading
                using (var weatherArchiveData = new WeatherArchiveData(timeStamp.Year))
                {
                    var reading = new ReadingData
                    {
                        DeviceId = _ownerDevice.Id,
                        Type = (int) ValueType,
                        Value = value,
                        ReadTime = timeStamp
                    };

                    weatherArchiveData.Readings.Add(reading);
                    weatherArchiveData.SaveChanges();
                }
            }
        }

        #endregion

        #region Properties

        [DataMember]
        public ReadingBase Current { get; set; }

        [DataMember]
        public WeatherValueType ValueType { get; set; }

        #endregion
    }
}
