using System.Runtime.Serialization;
using OneWireAPI;
using WeatherService.Values;

namespace WeatherService.Devices
{
    #region Enumerations

    [DataContract]
    public enum WindDirection
    {
        [EnumMember]
        North,
        
        [EnumMember]
        NorthNorthEast,

        [EnumMember]
        NorthEast,

        [EnumMember]
        EastNorthEast,

        [EnumMember]
        East,

        [EnumMember]
        EastSouthEast,

        [EnumMember]
        SouthEast,

        [EnumMember]
        SouthSouthEast,

        [EnumMember]
        South,

        [EnumMember]
        SouthSouthWest,

        [EnumMember]
        SouthWest,

        [EnumMember]
        WestSouthWest,

        [EnumMember]
        West,

        [EnumMember]
        WestNorthWest,

        [EnumMember]
        NorthWest,

        [EnumMember]
        NorthNorthWest,

        [EnumMember]
        Unknown = -1
    }

    #endregion

    [DataContract]
    public class WindDirectionDevice : DeviceBase
    {
        #region Constants

        private const double WindowOffset = 0.7;

        private readonly double[,] _lookupTable = {	{4.66, 4.66, 2.38, 4.66},			// 0
                                            	    {4.66, 3.18, 3.20, 4.64},			// 1
                                            	    {4.66, 2.38, 4.66, 4.66},			// 2
                                            	    {3.20, 3.20, 4.66, 4.64},			// 3
                                            	    {2.38, 4.66, 4.66, 4.66},			// 4
                                            	    {2.36, 4.62, 4.60, 0.06},			// 5
                                            	    {4.64, 4.64, 4.64, 0.06},			// 6
                                            	    {4.60, 4.60, 0.06, 0.06},			// 7
                                            	    {4.64, 4.64, 0.06, 4.64},			// 8
                                            	    {4.62, 0.06, 0.06, 4.60},			// 9
                                            	    {4.64, 0.06, 4.64, 4.64},			// 10
                                            	    {0.06, 0.06, 4.60, 4.60},			// 11
                                            	    {0.06, 4.64, 4.64, 4.64},			// 12
                                            	    {0.06, 4.62, 4.62, 2.34},			// 13
                                            	    {4.66, 4.66, 4.66, 2.38},			// 14
                                            	    {4.66, 4.66, 3.18, 3.18}	};		// 15

        #endregion

        #region Member variables

        private readonly Value _directionValue;             // Cached direction value

        private bool _initialized;                          // Has the device been initialized

        #endregion

        #region Constructor

        public WindDirectionDevice(Session session, owDevice device)
            : base(session, device, DeviceType.WindDirection)
        {
            // Create the value
            _directionValue = new Value(WeatherValueType.WindDirection, this);

            _valueList.Add(WeatherValueType.WindDirection, _directionValue);
        }

        #endregion

        #region Internal methods

        internal override void RefreshCache()
        {
            _directionValue.SetValue((double)ReadDirection());

            base.RefreshCache();
        }

        internal WindDirection ReadDirection()
        {
            int direction = -1;         // Decoded direction

            // Cast the device as the specific device
            owDeviceFamily20 voltage = (owDeviceFamily20)_device;

            // If we haven't initialized the device we need to do it now
            if (!_initialized)
            {
                // Initialize the device
                voltage.Initialize();

                // Remember that we've done this
                _initialized = true;
            }

            // Get the array of voltages from the device
            double[] voltages = voltage.GetVoltages();

            // Loop over the lookup table to find the direction that maps to the voltages
            for (int i = 0; i < 16; i++)
            {
                if (((voltages[0] <= _lookupTable[i, 0] + WindowOffset) && (voltages[0] >= _lookupTable[i, 0] - WindowOffset)) &&
                    ((voltages[1] <= _lookupTable[i, 1] + WindowOffset) && (voltages[1] >= _lookupTable[i, 1] - WindowOffset)) &&
                    ((voltages[2] <= _lookupTable[i, 2] + WindowOffset) && (voltages[2] >= _lookupTable[i, 2] - WindowOffset)) &&
                    ((voltages[3] <= _lookupTable[i, 3] + WindowOffset) && (voltages[3] >= _lookupTable[i, 3] - WindowOffset)))
                {
                    direction = i;
                    break;
                }
            }

            // Return the direction
            return (WindDirection)direction;
        }

        #endregion
    }
}