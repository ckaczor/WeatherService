using System.Runtime.Serialization;

namespace WeatherService.Values
{
    [DataContract]
    public class PressureReading : ReadingBase
    {
        public PressureReading(WeatherValueType valueType) : base(valueType)
        {
        }
    }
}
