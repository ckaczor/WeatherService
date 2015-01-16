using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using OneWireAPI;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class PressureDevice : DeviceBase
    {
        #region Constants

        private readonly byte[] _resetSequence = new byte[] { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0 };              // Binary sequence to reset the device
        private readonly byte[] _readWord1Sequence = new byte[] { 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0 };                                     // Binary sequence to read word 1
        private readonly byte[] _readWord2Sequence = new byte[] { 1, 1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 0 };                                     // Binary sequence to read word 2
        private readonly byte[] _readWord3Sequence = new byte[] { 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 0 };                                     // Binary sequence to read word 3
        private readonly byte[] _readWord4Sequence = new byte[] { 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0 };                                     // Binary sequence to read word 4
        private readonly byte[] _readPressureSequence = new byte[] { 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0 };                                     // Binary sequence to read pressure
        private readonly byte[] _readTemperatureSequence = new byte[] { 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0 };                                  // Binary sequence to read temperature

        private const byte ChannelAccessCommand = 0xF5;
        private const byte ConfigRead = 0xEC;
        private const byte ConfigWrite = 0x8C;
        private const byte ConfigPulseRead = 0xC8;

        #endregion

        #region Member variables

        private readonly owDeviceFamily12 _writeDevice;         // Device for writing to the pressure sensor
        private readonly owDeviceFamily12 _readDevice;          // Device for reading from the pressure sensor
        private readonly Value _temperatureValue;               // Last temperature (degrees C)
        private readonly Value _pressureValue;                  // Last pressure (mbar)

        private bool _readCalibration;                          // Have we read the calibration constants?
        
        private int _calibration1;                              // Calibration constant
        private int _calibration2;                              // Calibration constant
        private int _calibration3;                              // Calibration constant
        private int _calibration4;                              // Calibration constant
        private int _calibration5;                              // Calibration constant
        private int _calibration6;                              // Calibration constant

        #endregion

        #region Constructor

        public PressureDevice(Session session, owDevice firstDevice, owDevice secondDevice) : base(session, firstDevice, DeviceType.Pressure)
        {
            // Get both devices
            owDeviceFamily12 oDevice1 = (owDeviceFamily12) firstDevice;
            owDeviceFamily12 oDevice2 = (owDeviceFamily12) secondDevice;

            // Get the state of both devices
            byte[] baState1 = oDevice1.ReadDevice();
            byte[] baState2 = oDevice2.ReadDevice();

            // If both devices have the same power state then this isn't a proper pressure device
            if (oDevice1.IsPowered(baState1) == oDevice2.IsPowered(baState2))
            {
                // Throw an exception
                throw new Exception("Invalid TAI8570");
            }

            // The powered device is the write device - sort this out and remember which is which
            if (oDevice1.IsPowered(baState1))
            {
                _writeDevice = oDevice1;
                _readDevice = oDevice2;
            }
            else
            {
                _writeDevice = oDevice2;
                _readDevice = oDevice1;
            }

            _temperatureValue = new Value(WeatherValueType.Temperature, this);
            _pressureValue = new Value(WeatherValueType.Pressure, this);

            _valueList.Add(WeatherValueType.Temperature, _temperatureValue);
            _valueList.Add(WeatherValueType.Pressure, _pressureValue);
        }

        #endregion

        #region PIO methods

        private void PrepPioForWrite()
        {
            byte[] baState = _writeDevice.ReadDevice();
            _writeDevice.SetLatchState(0, true, baState);
            _writeDevice.SetLatchState(1, false, baState);
            _writeDevice.WriteDevice(baState);

            baState = _readDevice.ReadDevice();
            _readDevice.SetLatchState(0, false, baState);
            _readDevice.SetLatchState(1, false, baState);
            _readDevice.WriteDevice(baState);
        }

        private void PrepPioForRead()
        {
            byte[] baState = _readDevice.ReadDevice();
            _readDevice.SetLatchState(0, false, baState);
            _readDevice.SetLatchState(1, false, baState);
            _readDevice.WriteDevice(baState);

            baState = _writeDevice.ReadDevice();
            _writeDevice.SetLatchState(0, false, baState);
            _writeDevice.SetLatchState(1, false, baState);
            _writeDevice.WriteDevice(baState);
        }

        private bool OpenPio(int pio)
        {
            byte[] baWriteState = _writeDevice.ReadDevice();
            byte[] baReadState = _readDevice.ReadDevice();

            _writeDevice.SetLatchState(pio, false, baWriteState);
            _readDevice.SetLatchState(pio, false, baReadState);

            _writeDevice.WriteDevice(baWriteState);
            _writeDevice.WriteDevice(baReadState);

            baWriteState = _writeDevice.ReadDevice();
            baReadState = _readDevice.ReadDevice();

            bool bResult = (_writeDevice.GetLevel(pio, baWriteState) && _readDevice.GetLevel(pio, baReadState));

            return bResult;
        }

        private bool OpenPioA()
        {
            return OpenPio(0);
        }

        private bool OpenPioB()
        {
            return OpenPio(1);
        }

        #endregion

        #region Private methods

        private bool SetupForWrite()
        {
            byte[] data = new byte[3];          // Data buffer to send over the network
            short dataCount = 0;                // How many bytes of data to send

            PrepPioForWrite();

            owAdapter.Select(_writeDevice.Id);

            data[dataCount++] = ChannelAccessCommand;
            data[dataCount++] = ConfigWrite;
            data[dataCount++] = 0xFF;

            owAdapter.SendBlock(data, dataCount);

            owAdapter.ReadByte();

            return true;
        }

        private bool SetupForRead()
        {
            byte[] data = new byte[3];          // Data buffer to send over the network
            short dataCount = 0;                // How many bytes of data to send

            PrepPioForRead();

            owAdapter.Select(_readDevice.Id);

            data[dataCount++] = ChannelAccessCommand;
            data[dataCount++] = ConfigRead;
            data[dataCount++] = 0xFF;

            owAdapter.SendBlock(data, dataCount);

            owAdapter.ReadByte();

            return true;
        }

        private bool SetupForPulseRead()
        {
            byte[] data = new byte[3];          // Data buffer to send over the network
            short dataCount = 0;                // How many bytes of data to send

            PrepPioForWrite();

            owAdapter.Select(_readDevice.Id);

            data[dataCount++] = ChannelAccessCommand;
            data[dataCount++] = ConfigPulseRead;
            data[dataCount++] = 0xFF;

            owAdapter.SendBlock(data, dataCount);

            owAdapter.ReadByte();

            return true;
        }

        private bool WriteBitSequence(IEnumerable<byte> sequence)
        {
            bool result = false;

            if (SetupForWrite())
            {
                foreach (byte t in sequence)
                {
                    SendBit(t != 0);
                }

                SendBit(false);

                result = true;
            }

            return result;
        }

        private byte[] ReadBitSequence(IEnumerable<byte> sequence)
        {
            byte[] result = new byte[16];

            if (WriteBitSequence(sequence))
            {
                result = GetBits(16);
                OpenPioB();
            }

            return result;
        }

        private static void SendBit(bool value)
        {
            if (value)
            {
                owAdapter.SendBit(0);
                owAdapter.SendBit(1);
                owAdapter.SendBit(1);
                owAdapter.SendBit(1);
                owAdapter.SendBit(0);
                owAdapter.SendBit(0);
            }
            else
            {
                owAdapter.SendBit(0);
                owAdapter.SendBit(0);
                owAdapter.SendBit(1);
                owAdapter.SendBit(0);
                owAdapter.SendBit(0);
                owAdapter.SendBit(0);
            }
        }

        private static bool ReadBit()
        {
            owAdapter.ReadBit();			// Read PIO A #1
            owAdapter.ReadBit();			// Read PIO B #1
            owAdapter.ReadBit();			// Read PIO A #2
            owAdapter.ReadBit();			// Read PIO B #2
            owAdapter.ReadBit();			// Read PIO A #3
            owAdapter.ReadBit();			// Read PIO B #3
            owAdapter.ReadBit();			// Read PIO A #4
            short data = owAdapter.ReadBit();

            bool result = (data == 1);

            owAdapter.SendBit(0); 			// Write PIO A #1
            owAdapter.SendBit(1); 			// Write PIO B #1
            owAdapter.SendBit(0); 			// Write PIO A #2
            owAdapter.SendBit(1); 			// Write PIO B #2
            owAdapter.SendBit(1); 			// Write PIO A #3
            owAdapter.SendBit(1); 			// Write PIO B #3
            owAdapter.SendBit(1); 			// Write PIO A #4
            owAdapter.SendBit(1); 			// Write PIO B #4

            return result;
        }

        private bool Reset()
        {
            return WriteBitSequence(_resetSequence);
        }

        private bool CheckConversionStatus()
        {
            int i;

            if (!SetupForPulseRead()) 
                return false;

            for (i = 0; i < 100; i++)
                if (owAdapter.SendBit(0) == 0) 
                    break;

            return (i < 100);
        }

        private bool ReadCalibrationConstants()
        {
            int i;

            if (!Reset()) return false;

            byte[] word1 = ReadBitSequence(_readWord1Sequence);

            byte[] word2 = ReadBitSequence(_readWord2Sequence);

            byte[] word3 = ReadBitSequence(_readWord3Sequence);

            byte[] word4 = ReadBitSequence(_readWord4Sequence);

            _calibration1 = _calibration2 = _calibration3 = _calibration4 = _calibration5 = _calibration6 = 0;

            for (i = 0; i < 15; i++)
            {
                _calibration1 = (_calibration1 << 1);

                if (word1[i] == 1)
                    _calibration1 = _calibration1 + 1;
            }

            if (word1[15] == 1)
                _calibration5 = 1;

            for (i = 0; i < 10; i++)
            {
                _calibration5 = (_calibration5 << 1);

                if (word2[i] == 1)
                    _calibration5 = _calibration5 + 1;
            }

            for (i = 10; i < 16; i++)
            {
                _calibration6 = (_calibration6 << 1);

                if (word2[i] == 1)
                    _calibration6 = _calibration6 + 1;
            }

            for (i = 0; i < 10; i++)
            {
                _calibration4 = (_calibration4 << 1);

                if (word3[i] == 1)
                    _calibration4 = _calibration4 + 1;
            }

            for (i = 10; i < 16; i++)
            {
                _calibration2 = (_calibration2 << 1);

                if (word3[i] == 1)
                    _calibration2 = _calibration2 + 1;
            }

            for (i = 10; i < 16; i++)
            {
                _calibration2 = (_calibration2 << 1);

                if (word4[i] == 1)
                    _calibration2 = _calibration2 + 1;
            }

            for (i = 0; i < 10; i++)
            {
                _calibration3 = (_calibration3 << 1);

                if (word4[i] == 1)
                    _calibration3 = _calibration3 + 1;
            }

            return true;
        }

        private byte[] GetBits(byte bitCount)
        {
            byte[] result = new byte[bitCount];

            if (SetupForRead())
            {
                for (int i = 0; i < bitCount; i++)
                {
                    if (ReadBit())
                        result[i] = 1;
                    else
                        result[i] = 0;
                }
            }

            return result;
        }

        private int ReadValue(IEnumerable<byte> sequence)
        {
            int result = 0;

            if (!Reset()) 
                return 0;

            if (!WriteBitSequence(sequence)) 
                return 0;

            if (!CheckConversionStatus()) 
                return 0;

            if (!OpenPioA()) 
                return 0;

            byte[] data = GetBits(16);

            if (!OpenPioB()) 
                return 0;

            for (int i = 0; i < 16; i++)
            {
                result = (result << 1);

                if (data[i] == 1)
                    result = result + 1;
            }

            return result;
        }

        private bool ReadSensorData()
        {
            int pressure = ReadValue(_readPressureSequence);
            int temperature = ReadValue(_readTemperatureSequence);

            double calibrationTemperature = (8 * _calibration5) + 20224;
            double temperatureDifference = temperature - calibrationTemperature;
            _temperatureValue.SetValue(20 + ((temperatureDifference * (_calibration6 + 50)) / 10240));

            double offset = _calibration2 * 4 + (((_calibration4 - 512) * temperatureDifference) / 4096);
            double sensitivity = _calibration1 + ((_calibration3 * temperatureDifference) / 1024) + 24576;
            double actualPressure = ((sensitivity * (pressure - 7168)) / 16384) - offset;

            _pressureValue.SetValue((actualPressure / 32) + 250);

            return true;
        }

        private void ReadDevice()
        {
            if (!_readCalibration)
            {
                if (!ReadCalibrationConstants())
                    throw new Exception("Error reading calibration constants");

                _readCalibration = true;
            }

            if (!ReadSensorData())
                throw new Exception("Error reading Pressure and Temperature values");
        }

        #endregion

        #region Internal methods

        internal override void RefreshCache()
        {
            ReadDevice();

            base.RefreshCache();
        }

        #endregion

        #region Elevation code (not used yet)

        //private double m_dElevation = 0.0;				// Height above sea level (meters)

        //public void set_height_over_sea_level_meter(double height)
        //{
        //    m_dElevation = height;
        //}

        //public void set_height_over_sea_level_feet(double height)
        //{
        //    m_dElevation = height * 0.3048;
        //}

        //public double get_height_over_sea_level_meter()
        //{
        //    return m_dElevation;
        //}

        //public double get_height_over_sea_level_feet()
        //{
        //    return (3.281 * m_dElevation);
        //}

        //public double get_calc_height_over_sea_level_meter()
        //{
        //    return (288.15 / 0.0065) * (1 - Math.Pow((double) (getPressure_Pa() / 101325), (double) (0.0065 * (287.052 / 9.81))));
        //}

        #endregion
    }
}