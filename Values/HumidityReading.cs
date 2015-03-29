using System.Runtime.Serialization;

namespace WeatherService.Values
{
    [DataContract]
    public class HumidityReading : ReadingBase
    {
        public HumidityReading() : base(WeatherValueType.Humidity)
        {
        }
    }
}
