using System.Runtime.Serialization;

namespace WeatherService.Values
{
    [DataContract]
    public class TemperatureReading : ReadingBase
    {
        public TemperatureReading() : base(WeatherValueType.Temperature)
        {
        }

        [DataMember]
        public double DegreesC
        {
            get { return Value; }
            set { Value = value; }
        }

        [DataMember]
        public double DegreesF
        {
            get { return Conversion.ConvertTemperature(Value, TemperatureUnit.Celsius, TemperatureUnit.Fahrenheit); }
            set { Value = Conversion.ConvertTemperature(value, TemperatureUnit.Fahrenheit, TemperatureUnit.Celsius); }
        }
    }
}
