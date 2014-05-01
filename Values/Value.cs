using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Linq;
using System.Runtime.Serialization;
using WeatherService.Data;
using WeatherService.Devices;

namespace WeatherService.Values
{
    [DataContract]
    public enum WeatherValueType
    {
        [EnumMember]
        Temperature,

        [EnumMember]
        Pressure,

        [EnumMember]
        Humidity,

        [EnumMember]
        WindSpeed,

        [EnumMember]
        WindDirection,

        [EnumMember]
        Rain
    }

    /// <summary>
    /// Stores information for a particular device value
    /// </summary>
    [DataContract]
    public class Value
    {
        #region Constants

        private const int MaximumHours = 24;

        #endregion

        #region Member variables

        private readonly DeviceBase _ownerDevice;                   // Owner device

        #endregion

        #region Constructor

        public Value(WeatherValueType valueType, DeviceBase ownerDevice)
        {
            MaximumHistoryHours = MaximumHours;

            // Remember information we were given
            ValueType = valueType;
            _ownerDevice = ownerDevice;

            // Create the readings
            Current = ReadingBase.CreateReading(ValueType);
            Maximum = ReadingBase.CreateReading(ValueType);
            Minimum = ReadingBase.CreateReading(ValueType);
            Average = ReadingBase.CreateReading(ValueType);

            Total = valueType == WeatherValueType.Rain ? ReadingBase.CreateReading(ValueType) : new ReadingBase(valueType);

            History = new List<ReadingBase>();

            // Figure out the minimum time we want to load into history
            DateTime minimumTime = DateTime.Now.AddHours(-MaximumHours);

            // Build a list for this device using the right value type and limit to the maximum number of hours
            var readingList = from reading in Database.ReadingTable 
                              where reading.DeviceId == ownerDevice.Id && reading.Type == valueType && reading.ReadTime >= minimumTime 
                              select reading;

            // Loop over all readings and reload them into the history
            foreach (ReadingData readingData in readingList)
            {
                // Get the value from the reading
                double dValue = readingData.Value;

                // Get the timestamp from the reading
                DateTime dtTimestamp = readingData.ReadTime;

                // Set the value into the history
                SetValue(dValue, dtTimestamp, false);
            }
        }

        #endregion

        #region Private methods

        private void AddHistory(double value, DateTime timeStamp)
        {
            // Create the reading
            ReadingBase readingBase = ReadingBase.CreateReading(ValueType);
            readingBase.Value = value;
            readingBase.ReadTime = timeStamp;

            // Add the current value to the history
            History.Add(readingBase);

            // Get the current head of the history list
            ReadingBase oHead = History[0];

            // Get the difference in time between the now and the reading time
            TimeSpan oTimeSpan = DateTime.Now - oHead.ReadTime;

            // If the span is over the maximum time then remove it
            if (oTimeSpan.TotalHours > MaximumHours)
            {
                // Remove this sample
                Readings--;

                // Remove this value from the total
                Total.Value -= oHead.Value;

                // Remove the reading
                History.RemoveAt(0);
            }
        }

        private void CheckMinimumValue(double value, DateTime timeStamp)
        {
            // Figure out if we want a zero value
            bool bNoZero = (ValueType == WeatherValueType.WindSpeed || ValueType == WeatherValueType.Rain);

            // Figure out how old the current minimum is
            TimeSpan oTimeSpan = DateTime.Now - Minimum.ReadTime;

            // If the minimum is too old then we need a new minimum
            if (oTimeSpan.TotalHours > MaximumHours)
            {
                List<ReadingBase> readings = bNoZero ? History.Where(r => r.Value > 0).ToList() : History;

                ReadingBase newMin = readings.Min();

                // If we got a minimum value then set it
                if (newMin != null)
                {
                    // Set the data into the value
                    Minimum.SetValue(newMin.Value, newMin.ReadTime);
                }
            }

            // If we have no minimum or the value is over the current minimum then reset the minimum
            if ((Minimum.ReadTime == DateTime.MinValue) || (value < Minimum.Value) || (Minimum.Value.Equals(0) && bNoZero))
            {
                bool bSetMinimum;				// Is this value the new minimum?
                
                if (bNoZero)
                    bSetMinimum = (value > 0) || (Minimum.ReadTime == DateTime.MinValue);
                else
                    bSetMinimum = true;

                if (bSetMinimum)
                    Minimum.SetValue(value, timeStamp);
            }
        }

        private void CheckMaximumValue(double value, DateTime timeStamp)
        {
            // Figure out how old the current maximum is
            TimeSpan oTimeSpan = DateTime.Now - Maximum.ReadTime;

            // If the maximum is too old then we need a new maximum
            if (oTimeSpan.TotalHours > MaximumHours)
            {
                // Get the new maximum
                var newMax = History.Max();

                // Set the maximum
                Maximum.SetValue(newMax.Value, newMax.ReadTime);
            }

            // If we have no maximum or the value is over the current maximum then reset the maximum
            if ((Maximum.ReadTime == DateTime.MinValue) || (value >= Maximum.Value))
            {
                Maximum.SetValue(value, timeStamp);
            }
        }

        #endregion

        #region Internal Methods

        internal void SetValue(double value)
        {
            // Set the value with the current time as the timestamp
            SetValue(value, DateTime.Now);
        }

        internal void SetValue(double value, DateTime timeStamp)
        {
            // Set the value with the current time as the timestamp - save the state
            SetValue(value, DateTime.Now, true);
        }

        internal void SetValue(double value, DateTime timeStamp, bool save)
        {
            // Set the current value
            Current.SetValue(value, timeStamp);

            // Add another sample
            Readings++;

            // Add the current value to the total
            Total.Value += value;

            // Add the value to the history
            AddHistory(value, timeStamp);

            if (save)
            {
                // Save the reading
                /*
                ReadingData readingData = new ReadingData
                                              {
                                                  DeviceId = _ownerDevice.Id,
                                                  Value = value,
                                                  Type = _valueType,
                                                  ReadTime = timeStamp
                                              };
                Database.ReadingTable.InsertOnSubmit(readingData);
                Database.SaveChanges();
                */

                string query =
                    string.Format(
                        "INSERT INTO Reading (DeviceID, Type, Value, ReadTime) VALUES ({0}, {1:d}, {2}, '{3}')",
                        _ownerDevice.Id, ValueType, value, timeStamp);
                SqlCeCommand command = new SqlCeCommand(query, Database.Connection);
                command.ExecuteScalar();

                // Update the minimum value
                CheckMinimumValue(value, timeStamp);

                // Update the maximum value
                CheckMaximumValue(value, timeStamp);
            }

            // Calculate the new average value
            Average.SetValue(Total.Value / Readings);
        }

        #endregion

        #region Properties

        [DataMember]
        public ReadingBase Current { get; set; }

        [DataMember]
        public ReadingBase Maximum { get; set; }

        [DataMember]
        public ReadingBase Minimum { get; set; }

        [DataMember]
        public ReadingBase Average { get; set; }

        [DataMember]
        public ReadingBase Total { get; set; }

        [DataMember]
        public long Readings { get; set; }

        [DataMember]
        public WeatherValueType ValueType { get; set; }

        [DataMember]
        public int MaximumHistoryHours { get; set; }

        public List<ReadingBase> History { get; set; }

        #endregion
    }
}
