using System;
using System.Runtime.Serialization;
using WeatherService.Devices;

namespace WeatherService.Values
{
    [DataContract]
    public class WindDirectionReading : ReadingBase
    {
        public static string GetShortDirectionString(WindDirection actualValue)
        {
            switch (actualValue)
            {
                case WindDirection.North:
                    return "N";
                case WindDirection.NorthNorthEast:
                    return "NNE";
                case WindDirection.NorthEast:
                    return "NE";
                case WindDirection.EastNorthEast:
                    return "ENE";
                case WindDirection.East:
                    return "E";
                case WindDirection.EastSouthEast:
                    return "ESE";
                case WindDirection.SouthEast:
                    return "SE";
                case WindDirection.SouthSouthEast:
                    return "SSE";
                case WindDirection.South:
                    return "S";
                case WindDirection.SouthSouthWest:
                    return "SSW";
                case WindDirection.SouthWest:
                    return "SW";
                case WindDirection.WestSouthWest:
                    return "WSW";
                case WindDirection.West:
                    return "W";
                case WindDirection.WestNorthWest:
                    return "WNW";
                case WindDirection.NorthWest:
                    return "NW";
                case WindDirection.NorthNorthWest:
                    return "NNW";
                default:
                    return String.Empty;
            }
        }

        public WindDirectionReading()
            : base(WeatherValueType.WindDirection)
        {
        }

        [DataMember]
        public WindDirection WindDirectionValue
        {
            get { return (WindDirection)Value; }
            set { Value = (double)value; }
        }

        [DataMember]
        public string WindDirectionString
        {
            get { return GetShortDirectionString(WindDirectionValue); }
            set { }
        }
    }
}