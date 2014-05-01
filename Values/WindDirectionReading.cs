using System.Runtime.Serialization;
using WeatherService.Common.Formatting;
using WeatherService.Devices;

namespace WeatherService.Values
{
    [DataContract]
    public class WindDirectionReading : ReadingBase
    {
        public WindDirectionReading(WeatherValueType valueType) : base(valueType)
        {
        }

        [DataMember]
        public WindDirection WindDirectionValue
        {
            get { return (WindDirection) Value; }
            set { Value = (double) value; }
        }

        [DataMember]
        public string WindDirectionString
        {
            get { return Format.GetShortDirectionString(WindDirectionValue); }
            set { }
        }
    }
}