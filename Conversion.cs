namespace WeatherService
{
    #region Enumerations

    public enum TemperatureUnit
    {
        Fahrenheit,
        Celsius
    }

    public enum LengthUnit
    {
        Inches,
        Millimeters
    }

    public enum PressureUnit
    {
        HectoPascal,
        MilliBar,
        InchesMercury,
        MillimeterMercury
    }

    #endregion

    public static class Conversion
    {
        #region Temperature

        public static double ConvertTemperature(double value, TemperatureUnit fromUnit, TemperatureUnit toUnit)
        {
            if (fromUnit == TemperatureUnit.Fahrenheit && toUnit == TemperatureUnit.Celsius)
            {
                return (value - 32) / 1.8F;
            }

            if (fromUnit == TemperatureUnit.Celsius && toUnit == TemperatureUnit.Fahrenheit)
            {
                return value * 1.8F + 32;
            }

            return value;
        }

        #endregion

        #region Length

        public static double ConvertLength(double value, LengthUnit fromUnit, LengthUnit toUnit)
        {
            double baseValue;

            switch (fromUnit)
            {
                case LengthUnit.Millimeters:
                    baseValue = value;
                    break;

                case LengthUnit.Inches:
                    baseValue = value / 0.0393700787F;
                    break;

                default:
                    baseValue = value;
                    break;
            }

            switch (toUnit)
            {
                case LengthUnit.Millimeters:
                    return baseValue;

                case LengthUnit.Inches:
                    return baseValue * 0.0393700787F;

                default:
                    return baseValue;
            }
        }

        #endregion

        #region Pressure

        public static double ConvertPressure(double value, PressureUnit fromUnit, PressureUnit toUnit)
        {
            double baseValue;

            switch (fromUnit)
            {
                case PressureUnit.HectoPascal:
                    baseValue = value;
                    break;

                case PressureUnit.MilliBar:
                    baseValue = value;
                    break;

                case PressureUnit.InchesMercury:
                    baseValue = value / 0.02952999;
                    break;

                case PressureUnit.MillimeterMercury:
                    baseValue = value / 0.7500617;
                    break;

                default:
                    baseValue = value;
                    break;
            }

            switch (toUnit)
            {
                case PressureUnit.HectoPascal:
                    return baseValue;

                case PressureUnit.MilliBar:
                    return baseValue;

                case PressureUnit.InchesMercury:
                    return baseValue * 0.02952999;

                case PressureUnit.MillimeterMercury:
                    return baseValue * 0.7500617;

                default:
                    return baseValue;
            }
        }

        #endregion
    }
}