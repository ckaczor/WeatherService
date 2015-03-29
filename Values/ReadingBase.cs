using System;
using System.Runtime.Serialization;

namespace WeatherService.Values
{
    /// <summary>
    /// Stores a single value reading from a device
    /// </summary>
    [DataContract]
    [KnownType(typeof(TemperatureReading))]
    [KnownType(typeof(WindDirectionReading))]
    [KnownType(typeof(WindSpeedReading))]
    [KnownType(typeof(RainReading))]
    [KnownType(typeof(PressureReading))]
    [KnownType(typeof(HumidityReading))]
    public class ReadingBase : IComparable<ReadingBase>
    {
        #region Constructor

        public ReadingBase(WeatherValueType valueType)
        {
            ValueType = valueType;
        }

        #endregion

        #region Internal methods

        internal void SetValue(double value)
        {
            Value = value;
            ReadTime = DateTime.Now;
        }

        internal void SetValue(double value, DateTime readTime)
        {
            Value = value;
            ReadTime = readTime;
        }

        #endregion

        #region Properties

        [DataMember]
        public WeatherValueType ValueType { get; set; }

        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public DateTime ReadTime { get; set; }

        #endregion

        #region Comparison

        public int CompareTo(ReadingBase other)
        {
            double otherValue = other.Value;
            double thisValue = Value;

            if (otherValue > thisValue)
                return -1;

            if (otherValue.Equals(thisValue))
                return 0;

            return 1;
        }

        #endregion

        #region Creation

        public static ReadingBase CreateReading(WeatherValueType valueType)
        {
            switch (valueType)
            {
                case WeatherValueType.Temperature:
                    return new TemperatureReading();
                case WeatherValueType.Pressure:
                    return new PressureReading();
                case WeatherValueType.Humidity:
                    return new HumidityReading();
                case WeatherValueType.WindSpeed:
                    return new WindSpeedReading();
                case WeatherValueType.WindDirection:
                    return new WindDirectionReading();
                case WeatherValueType.Rain:
                    return new RainReading();
                default:
                    throw new ArgumentOutOfRangeException("valueType");
            }
        }

        public static ReadingBase CreateReading(WeatherValueType valueType, DateTime readTime, double value)
        {
            var reading = CreateReading(valueType);

            reading.ReadTime = readTime;
            reading.Value = value;

            return reading;
        }

        #endregion
    }
}
