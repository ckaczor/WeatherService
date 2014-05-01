using System.Runtime.Serialization;

namespace WeatherService.Values
{
    [DataContract]
    public class WindSpeedReading : ReadingBase
    {
        public WindSpeedReading(WeatherValueType valueType) : base(valueType)
        {
        }
    }
}
