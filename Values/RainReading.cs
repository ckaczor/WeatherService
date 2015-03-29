using System.Runtime.Serialization;

namespace WeatherService.Values
{
    [DataContract]
    public class RainReading : ReadingBase
    {
        public RainReading() : base(WeatherValueType.Rain)
        {
        }

        [DataMember]
        public double Millimeters
        {
            get { return Value; }
            set { Value = value; }
        }

        [DataMember]
        public double Inches
        {
            get { return Conversion.ConvertLength(Value, LengthUnit.Millimeters, LengthUnit.Inches); }
            set { Value = Conversion.ConvertLength(value, LengthUnit.Inches, LengthUnit.Millimeters); }
        }
    }
}