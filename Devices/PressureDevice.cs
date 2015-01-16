using OneWireAPI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WeatherService.Values;

namespace WeatherService.Devices
{
    [DataContract]
    public class PressureDevice : DeviceBase
    {
        private readonly byte[] _resetSequence = { 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0 };              // Binary sequence to reset the device
        private readonly byte[] _readWord1Sequence = { 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0 };                                     // Binary sequence to read word 1
        private readonly byte[] _readWord2Sequence = { 1, 1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 0 };                                     // Binary sequence to read word 2
        private readonly byte[] _readWord3Sequence = { 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 0 };                                     // Binary sequence to read word 3
        private readonly byte[] _readWord4Sequence = { 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 0 };                                     // Binary sequence to read word 4
        private readonly byte[] _readPressureSequence = { 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0 };                                     // Binary sequence to read pressure
        private readonly byte[] _readTemperatureSequence = { 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0 };                                  // Binary sequence to read temperature

        private const byte ChannelAccessCommand = 0xF5;
        private const byte ConfigRead = 0xEC;
        private const byte ConfigWrite = 0x8C;
        private const byte ConfigPulseRead = 0xC8;

        private readonly DeviceFamily12 _writeDevice;           // Device for writing to the pressure sensor
        private readonly DeviceFamily12 _readDevice;            // Device for reading from the pressure sensor
        private readonly Value _temperatureValue;               // Last temperature (degrees C)
        private readonly Value _pressureValue;                  // Last pressure (mbar)

        private bool _readCalibration;                          // Have we read the calibration constants?

        private int _calibration1;                              // Calibration constant
        private int _calibration2;                              // Calibration constant
        private int _calibration3;                              // Calibration constant
        private int _calibration4;                              // Calibration constant
        private int _calibration5;                              // Calibration constant
        private int _calibration6;                              // Calibration constant

        public PressureDevice(Session session, Device firstDevice, Device secondDevice)
            : base(session, firstDevice, DeviceType.Pressure)
        {
            // Get both devices
            var device1 = (DeviceFamily12) firstDevice;
            var device2 = (DeviceFamily12) secondDevice;

            // Get the state of both devices
            var state1 = device1.ReadDevice();
            var state2 = device2.ReadDevice();

            // If both devices have the same power state then this isn't a proper pressure device
            if (device1.IsPowered(state1) == device2.IsPowered(state2))
            {
                // Throw an exception
                throw new Exception("Invalid TAI8570");
            }

            // The powered device is the write device - sort this out and remember which is which
            if (device1.IsPowered(state1))
            {
                _writeDevice = device1;
                _readDevice = device2;
            }
            else
            {
                _writeDevice = device2;
                _readDevice = device1;
            }

            _temperatureValue = new Value(WeatherValueType.Temperature, this);
            _pressureValue = new Value(WeatherValueType.Pressure, this);

            Values.Add(WeatherValueType.Temperature, _temperatureValue);
            Values.Add(WeatherValueType.Pressure, _pressureValue);
        }

        private void PrepPioForWrite()
        {
            var state = _writeDevice.ReadDevice();
            _writeDevice.SetLatchState(0, true, state);
            _writeDevice.SetLatchState(1, false, state);
            _writeDevice.WriteDevice(state);

            state = _readDevice.ReadDevice();
            _readDevice.SetLatchState(0, false, state);
            _readDevice.SetLatchState(1, false, state);
            _readDevice.WriteDevice(state);
        }

        private void PrepPioForRead()
        {
            var state = _readDevice.ReadDevice();
            _readDevice.SetLatchState(0, false, state);
            _readDevice.SetLatchState(1, false, state);
            _readDevice.WriteDevice(state);

            state = _writeDevice.ReadDevice();
            _writeDevice.SetLatchState(0, false, state);
            _writeDevice.SetLatchState(1, false, state);
            _writeDevice.WriteDevice(state);
        }

        private bool OpenPio(int pio)
        {
            var writeState = _writeDevice.ReadDevice();
            var readDevice = _readDevice.ReadDevice();

            _writeDevice.SetLatchState(pio, false, writeState);
            _readDevice.SetLatchState(pio, false, readDevice);

            _writeDevice.WriteDevice(writeState);
            _writeDevice.WriteDevice(readDevice);

            writeState = _writeDevice.ReadDevice();
            readDevice = _readDevice.ReadDevice();

            var result = (_writeDevice.GetLevel(pio, writeState) && _readDevice.GetLevel(pio, readDevice));

            return result;
        }

        private bool OpenPioA()
        {
            return OpenPio(0);
        }

        private bool OpenPioB()
        {
            return OpenPio(1);
        }

        private bool SetupForWrite()
        {
            var data = new byte[3];          // Data buffer to send over the network
            short dataCount = 0;                // How many bytes of data to send

            PrepPioForWrite();

            Adapter.Select(_writeDevice.Id);

            data[dataCount++] = ChannelAccessCommand;
            data[dataCount++] = ConfigWrite;
            data[dataCount++] = 0xFF;

            Adapter.SendBlock(data, dataCount);

            Adapter.ReadByte();

            return true;
        }

        private bool SetupForRead()
        {
            var data = new byte[3];          // Data buffer to send over the network
            short dataCount = 0;                // How many bytes of data to send

            PrepPioForRead();

            Adapter.Select(_readDevice.Id);

            data[dataCount++] = ChannelAccessCommand;
            data[dataCount++] = ConfigRead;
            data[dataCount++] = 0xFF;

            Adapter.SendBlock(data, dataCount);

            Adapter.ReadByte();

            return true;
        }

        private bool SetupForPulseRead()
        {
            var data = new byte[3];          // Data buffer to send over the network
            short dataCount = 0;                // How many bytes of data to send

            PrepPioForWrite();

            Adapter.Select(_readDevice.Id);

            data[dataCount++] = ChannelAccessCommand;
            data[dataCount++] = ConfigPulseRead;
            data[dataCount++] = 0xFF;

            Adapter.SendBlock(data, dataCount);

            Adapter.ReadByte();

            return true;
        }

        private bool WriteBitSequence(IEnumerable<byte> sequence)
        {
            if (!SetupForWrite())
                return false;

            foreach (var t in sequence)
            {
                SendBit(t != 0);
            }

            SendBit(false);

            return true;
        }

        private byte[] ReadBitSequence(IEnumerable<byte> sequence)
        {
            var result = new byte[16];

            if (!WriteBitSequence(sequence))
                return result;

            result = GetBits(16);
            OpenPioB();

            return result;
        }

        private static void SendBit(bool value)
        {
            if (value)
            {
                Adapter.SendBit(0);
                Adapter.SendBit(1);
                Adapter.SendBit(1);
                Adapter.SendBit(1);
                Adapter.SendBit(0);
                Adapter.SendBit(0);
            }
            else
            {
                Adapter.SendBit(0);
                Adapter.SendBit(0);
                Adapter.SendBit(1);
                Adapter.SendBit(0);
                Adapter.SendBit(0);
                Adapter.SendBit(0);
            }
        }

        private static bool ReadBit()
        {
            Adapter.ReadBit();			// Read PIO A #1
            Adapter.ReadBit();			// Read PIO B #1
            Adapter.ReadBit();			// Read PIO A #2
            Adapter.ReadBit();			// Read PIO B #2
            Adapter.ReadBit();			// Read PIO A #3
            Adapter.ReadBit();			// Read PIO B #3
            Adapter.ReadBit();			// Read PIO A #4
            var data = Adapter.ReadBit();

            var result = (data == 1);

            Adapter.SendBit(0); 			// Write PIO A #1
            Adapter.SendBit(1); 			// Write PIO B #1
            Adapter.SendBit(0); 			// Write PIO A #2
            Adapter.SendBit(1); 			// Write PIO B #2
            Adapter.SendBit(1); 			// Write PIO A #3
            Adapter.SendBit(1); 			// Write PIO B #3
            Adapter.SendBit(1); 			// Write PIO A #4
            Adapter.SendBit(1); 			// Write PIO B #4

            return result;
        }

        private bool Reset()
        {
            return WriteBitSequence(_resetSequence);
        }

        private bool CheckConversionStatus()
        {
            int index;

            if (!SetupForPulseRead())
                return false;

            for (index = 0; index < 100; index++)
                if (Adapter.SendBit(0) == 0)
                    break;

            return (index < 100);
        }

        private bool ReadCalibrationConstants()
        {
            int i;

            if (!Reset()) return false;

            var word1 = ReadBitSequence(_readWord1Sequence);

            var word2 = ReadBitSequence(_readWord2Sequence);

            var word3 = ReadBitSequence(_readWord3Sequence);

            var word4 = ReadBitSequence(_readWord4Sequence);

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
            var result = new byte[bitCount];

            if (!SetupForRead())
                return result;

            for (var index = 0; index < bitCount; index++)
            {
                if (ReadBit())
                    result[index] = 1;
                else
                    result[index] = 0;
            }

            return result;
        }

        private int ReadValue(IEnumerable<byte> sequence)
        {
            var result = 0;

            if (!Reset())
                return 0;

            if (!WriteBitSequence(sequence))
                return 0;

            if (!CheckConversionStatus())
                return 0;

            if (!OpenPioA())
                return 0;

            var data = GetBits(16);

            if (!OpenPioB())
                return 0;

            for (var index = 0; index < 16; index++)
            {
                result = (result << 1);

                if (data[index] == 1)
                    result = result + 1;
            }

            return result;
        }

        private bool ReadSensorData()
        {
            var pressure = ReadValue(_readPressureSequence);
            var temperature = ReadValue(_readTemperatureSequence);

            double calibrationTemperature = (8 * _calibration5) + 20224;
            var temperatureDifference = temperature - calibrationTemperature;
            _temperatureValue.SetValue(20 + ((temperatureDifference * (_calibration6 + 50)) / 10240));

            var offset = _calibration2 * 4 + (((_calibration4 - 512) * temperatureDifference) / 4096);
            var sensitivity = _calibration1 + ((_calibration3 * temperatureDifference) / 1024) + 24576;
            var actualPressure = ((sensitivity * (pressure - 7168)) / 16384) - offset;

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

        internal override void RefreshCache()
        {
            ReadDevice();

            base.RefreshCache();
        }

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