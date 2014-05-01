using System.Runtime.Serialization;

namespace WeatherService.Values
{
    [DataContract]
    public class HumidityReading : ReadingBase
    {
        public HumidityReading(WeatherValueType valueType) : base(valueType)
        {
        }
    }
}
