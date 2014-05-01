using WeatherService.Devices;
using WeatherService.Values;

namespace WeatherService.Common.Formatting
{
    public static class Format
    {
        #region Value unit conversion

        public static double ConvertValue(ReadingBase reading)
        {
            switch (reading.ValueType)
            {
                case WeatherValueType.Humidity:
                    return reading.Value;

                case WeatherValueType.Pressure:
                    return reading.Value;

                case WeatherValueType.Rain:
                    return Conversion.ConvertLength(reading.Value, LengthUnit.Millimeters, LengthUnit.Inches);

                case WeatherValueType.Temperature:
                    return Conversion.ConvertTemperature(reading.Value, TemperatureUnit.Celsius, TemperatureUnit.Fahrenheit);

                case WeatherValueType.WindDirection:
                    return reading.Value;

                case WeatherValueType.WindSpeed:
                    return reading.Value;

                default:
                    return reading.Value;
            }
        }

        #endregion

        #region Value string formatting

        public static string FormatValue(Value actualValue)
        {
            switch (actualValue.ValueType)
            {
                case WeatherValueType.Humidity:
                    return string.Format("{0:f2}", actualValue.Current.Value);
                case WeatherValueType.Pressure:
                    return string.Format("{0:f2}", actualValue.Current.Value);
                case WeatherValueType.Rain:
                    return string.Format("{0:f2}", Conversion.ConvertLength(actualValue.Total.Value, LengthUnit.Millimeters, LengthUnit.Inches));
                case WeatherValueType.Temperature:
                    return string.Format("{0:f2}", actualValue.Current.Value * 9 / 5 + 32);
                case WeatherValueType.WindDirection:
                    return string.Format("{0}", GetShortDirectionString((WindDirection)actualValue.Current.Value));
                case WeatherValueType.WindSpeed:
                    return string.Format("{0:f2}", actualValue.Current.Value);
                default:
                    return actualValue.Current.Value.ToString();
            }
        }

        #endregion

        #region Wind direction

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
                    return string.Empty;
            }
        }

        #endregion
    }
}