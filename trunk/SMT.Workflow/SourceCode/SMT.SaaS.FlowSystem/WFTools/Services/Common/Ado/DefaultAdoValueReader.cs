using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace WFTools.Services.Common.Ado
{
	/// <summary>
    /// Default implementation of <see value="IAdoValueReader" /> that uses 
    /// the methods on the <see cref="IDataRecord" /> to retrieve values or
    /// direct casting of an <see cref="IDbCommand"or <see cref="IDbCommand" /> 
    /// to retieve values.
	/// </summary>
    public class DefaultAdoValueReader : IAdoValueReader
	{
        /// <summary>
        /// Obtain the ordinal of the specified column name.
        /// </summary>
        protected virtual Int32 GetOrdinal(IDataRecord dataRecord, String columnName)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            try
            {
                return dataRecord.GetOrdinal(columnName);
            }
            catch
            {
                throw new ArgumentNullException(RM.Get_Error_ColumnNotFound(columnName));
            }
        }

        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        public T GetEnum<T>(IDataRecord dataRecord, String columnName) where T : struct
        {
            return GetEnum<T>(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Enum value from a data record with a default value.
        /// </summary>
        public T GetEnum<T>(IDataRecord dataRecord, String columnName, T defaultValue) where T : struct
        {
            return GetEnum(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        public virtual T GetEnum<T>(IDataRecord dataRecord, Int32 ordinal) where T : struct
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new ArgumentException(RM.Get_Error_CannotConvertDBNullToEnum());
            else
            {
                Int32 enumValue = dataRecord.GetInt32(ordinal);
                if (Enum.IsDefined(typeof(T), enumValue))
                    return (T)Enum.Parse(typeof(T), enumValue.ToString(CultureInfo.InvariantCulture));
                else
                    throw new ArgumentException(RM.Get_Error_CannotConvertColumnToEnum());
            }
        }

        /// <summary>
        /// Retrieve an Enum value from a data record with a default value.
        /// </summary>
        public virtual T GetEnum<T>(IDataRecord dataRecord, Int32 ordinal, T defaultValue) where T : struct
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
            {
                Int32 enumValue = GetInt32(dataRecord, ordinal);
                if (Enum.IsDefined(typeof(T), enumValue))
                    return (T)Enum.Parse(typeof(T), enumValue.ToString(CultureInfo.InvariantCulture));
                else
                    return defaultValue;
            }
        }

        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        public T? GetNullableEnum<T>(IDataRecord dataRecord, String columnName) where T : struct
        {
            return GetNullableEnum<T>(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        public virtual T? GetNullableEnum<T>(IDataRecord dataRecord, Int32 ordinal) where T : struct
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
            {
                Int32 enumValue = GetInt32(dataRecord, ordinal);
                if (Enum.IsDefined(typeof(T), enumValue))
                    return (T)Enum.Parse(typeof(T), enumValue.ToString(CultureInfo.InvariantCulture));
                else
                    return null;
            }
        }

        /// <summary>
        /// Retrieve an Int32 value from a data record.
        /// </summary>
        public Int32 GetInt32(IDataRecord dataRecord, String columnName)
        {
            return GetInt32(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Int32 value from a data record with a default value.
        /// </summary>
        public Int32 GetInt32(IDataRecord dataRecord, String columnName, Int32 defaultValue)
        {
            return GetInt32(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve an Int32 value from a data record.
        /// </summary>
        public virtual Int32 GetInt32(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToInt32());
            else
                return dataRecord.GetInt32(ordinal);
        }

        /// <summary>
        /// Retrieve an Int32 value from a data record with a default value.
        /// </summary>
        public virtual Int32 GetInt32(IDataRecord dataRecord, Int32 ordinal, Int32 defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetInt32(ordinal);
        }

        /// <summary>
        /// Retrieve an Int32 value from a data record, or null if the specified value is not present.
        /// </summary>
        public Int32? GetNullableInt32(IDataRecord dataRecord, String columnName)
        {
            return GetNullableInt32(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Int32 value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Int32? GetNullableInt32(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetInt32(ordinal);
        }

        /// <summary>
        /// Retrieve an Int64 value from a data record.
        /// </summary>
        public Int64 GetInt64(IDataRecord dataRecord, String columnName)
        {
            return GetInt64(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Int64 value from a data record with a default value.
        /// </summary>
        public Int64 GetInt64(IDataRecord dataRecord, String columnName, Int64 defaultValue)
        {
            return GetInt64(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve an Int64 value from a data record.
        /// </summary>
        public virtual Int64 GetInt64(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToInt64());
            else
                return dataRecord.GetInt64(ordinal);
        }

        /// <summary>
        /// Retrieve an Int64 value from a data record with a default value.
        /// </summary>
        public virtual Int64 GetInt64(IDataRecord dataRecord, Int32 ordinal, Int64 defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetInt64(ordinal);
        }

        /// <summary>
        /// Retrieve an Int64 value from a data record, or null if the specified value is not present.
        /// </summary>
        public Int64? GetNullableInt64(IDataRecord dataRecord, String columnName)
        {
            return GetNullableInt64(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Int64 value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Int64? GetNullableInt64(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetInt64(ordinal);
        }

        /// <summary>
        /// Retrieve an Int16 value from a data record.
        /// </summary>
        public Int16 GetInt16(IDataRecord dataRecord, String columnName)
        {
            return GetInt16(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Int16 value from a data record with a default value.
        /// </summary>
        public Int16 GetInt16(IDataRecord dataRecord, String columnName, Int16 defaultValue)
        {
            return GetInt16(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve an Int16 value from a data record.
        /// </summary>
        public virtual Int16 GetInt16(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToInt16());
            else
                return dataRecord.GetInt16(ordinal);
        }

        /// <summary>
        /// Retrieve an Int16 value from a data record with a default value.
        /// </summary>
        public virtual Int16 GetInt16(IDataRecord dataRecord, Int32 ordinal, Int16 defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetInt16(ordinal);
        }

        /// <summary>
        /// Retrieve an Int16 value from a data record, or null if the specified value is not present.
        /// </summary>
        public Int16? GetNullableInt16(IDataRecord dataRecord, String columnName)
        {
            return GetNullableInt16(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve an Int16 value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Int16? GetNullableInt16(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetInt16(ordinal);
        }

        /// <summary>
        /// Retrieve a Byte from a data record.
        /// </summary>
        public Byte GetByte(IDataRecord dataRecord, String columnName)
        {
            return GetByte(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Byte from a data record with a default value.
        /// </summary>
        public Byte GetByte(IDataRecord dataRecord, String columnName, Byte defaultValue)
        {
            return GetByte(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Byte from a data record.
        /// </summary>
        public virtual Byte GetByte(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToByte());
            else
                return dataRecord.GetByte(ordinal);
        }

        /// <summary>
        /// Retrieve a Byte from a data record with a default value.
        /// </summary>
        public virtual Byte GetByte(IDataRecord dataRecord, Int32 ordinal, Byte defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetByte(ordinal);
        }


        /// <summary>
        /// Retrieve a Byte value from a data record, or null if the specified value is not present.
        /// </summary>
        public Byte? GetNullableByte(IDataRecord dataRecord, String columnName)
        {
            return GetNullableByte(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Byte value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Byte? GetNullableByte(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetByte(ordinal);
        }

        /// <summary>
        /// Retrieve a Single value from a data record.
        /// </summary>
        public Single GetSingle(IDataRecord dataRecord, String columnName)
        {
            return GetSingle(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Single value from a data record with a default value.
        /// </summary>
        public Single GetSingle(IDataRecord dataRecord, String columnName, Single defaultValue)
        {
            return GetSingle(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Single value from a data record.
        /// </summary>
        public virtual Single GetSingle(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToSingle());
            else
                return dataRecord.GetFloat(ordinal);
        }

        /// <summary>
        /// Retrieve a Single value from a data record with a default value.
        /// </summary>
        public virtual Single GetSingle(IDataRecord dataRecord, Int32 ordinal, Single defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetFloat(ordinal);
        }

        /// <summary>
        /// Retrieve a Single value from a data record, or null if the specified value is not present.
        /// </summary>
        public Single? GetNullableSingle(IDataRecord dataRecord, String columnName)
        {
            return GetNullableSingle(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Single value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Single? GetNullableSingle(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetFloat(ordinal);
        }

        /// <summary>
        /// Retrieve a Double value from a data record.
        /// </summary>
        public Double GetDouble(IDataRecord dataRecord, String columnName)
        {
            return GetDouble(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Double value from a data record with a default value.
        /// </summary>
        public Double GetDouble(IDataRecord dataRecord, String columnName, Double defaultValue)
        {
            return GetDouble(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Double value from a data record.
        /// </summary>
        public virtual Double GetDouble(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToDouble());
            else
                return dataRecord.GetDouble(ordinal);
        }

        /// <summary>
        /// Retrieve a Double value from a data record with a default value.
        /// </summary>
        public virtual Double GetDouble(IDataRecord dataRecord, Int32 ordinal, Double defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetDouble(ordinal);
        }

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        public Double? GetNullableDouble(IDataRecord dataRecord, String columnName)
        {
            return GetNullableDouble(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Double? GetNullableDouble(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetDouble(ordinal);
        }

	    /// <summary>
	    /// Retrieve an Object value from a data record.
	    /// </summary>
	    public object GetValue(IDataRecord dataRecord, String columnName)
	    {
	        return GetValue(dataRecord, GetOrdinal(dataRecord, columnName), null);
	    }

	    /// <summary>
        /// Retrieve an Object value from a data record with default value.
        /// </summary>
        public Object GetValue(IDataRecord dataRecord, String columnName, Object defaultValue)
        {
            return GetValue(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve an Object value from a data record.
        /// </summary>
	    public virtual object GetValue(IDataRecord dataRecord, Int32 ordinal)
	    {
	        return GetValue(dataRecord, ordinal, null);
	    }
        
	    /// <summary>
        /// Retrieve an Object value from a data record with default value.
        /// </summary>
        public virtual Object GetValue(IDataRecord dataRecord, Int32 ordinal, Object defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetValue(ordinal);
        }

        /// <summary>
        /// Retrieve a Guid value from a data record.
        /// </summary>
        public Guid GetGuid(IDataRecord dataRecord, String columnName)
        {
            return GetGuid(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Guid value from a data record with a default value.
        /// </summary>
        public Guid GetGuid(IDataRecord dataRecord, String columnName, Guid defaultValue)
        {
            return GetGuid(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Guid value from a data record.
        /// </summary>
        public virtual Guid GetGuid(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToGuid());
            else
                return dataRecord.GetGuid(ordinal);
        }

        /// <summary>
        /// Retrieve a Guid value from a data record with a default value.
        /// </summary>
        public virtual Guid GetGuid(IDataRecord dataRecord, Int32 ordinal, Guid defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetGuid(ordinal);
        }

        /// <summary>
        /// Retrieve a String value from a data record.
        /// </summary>
        public String GetString(IDataRecord dataRecord, String columnName)
        {
            return GetString(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a String value from a data record with a default value.
        /// </summary>
        public String GetString(IDataRecord dataRecord, String columnName, String defaultValue)
        {
            return GetString(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a String value from a data record.
        /// </summary>
        public virtual String GetString(IDataRecord dataRecord, Int32 ordinal)
        {
            return GetString(dataRecord, ordinal, null);
        }

        /// <summary>
        /// Retrieve a String value from a data record with a default value.
        /// </summary>
        public virtual String GetString(IDataRecord dataRecord, Int32 ordinal, String defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetString(ordinal);
        }

        /// <summary>
        /// Retrieve a Decimal value from a data record.
        /// </summary>
        public Decimal GetDecimal(IDataRecord dataRecord, String columnName)
        {
            return GetDecimal(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Decimal value from a data record with a default value.
        /// </summary>
        public Decimal GetDecimal(IDataRecord dataRecord, String columnName, Decimal defaultValue)
        {
            return GetDecimal(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Decimal value from a data record.
        /// </summary>
        public virtual Decimal GetDecimal(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToDecimal());
            else
                return dataRecord.GetDecimal(ordinal);
        }

        /// <summary>
        /// Retrieve a Decimal value from a data record with a default value.
        /// </summary>
        public virtual Decimal GetDecimal(IDataRecord dataRecord, Int32 ordinal, Decimal defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetDecimal(ordinal);
        }

        /// <summary>
        /// Retrieve a Decimal value from a data record, or null if the specified value is not present.
        /// </summary>
        public Decimal? GetNullableDecimal(IDataRecord dataRecord, String columnName)
        {
            return GetNullableDecimal(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Decimal value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Decimal? GetNullableDecimal(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");
            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetDecimal(ordinal);
        }

        /// <summary>
        /// Retrieve a Boolean value from a data record.
        /// </summary>
        public Boolean GetBoolean(IDataRecord dataRecord, String columnName)
        {
            return GetBoolean(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Boolean value from a data record with a default value.
        /// </summary>
        public Boolean GetBoolean(IDataRecord dataRecord, String columnName, Boolean defaultValue)
        {
            return GetBoolean(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Boolean value from a data record.
        /// </summary>
        public virtual Boolean GetBoolean(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToBoolean());
            else
                return dataRecord.GetBoolean(ordinal);
        }

        /// <summary>
        /// Retrieve a Boolean value from a data record with a default value.
        /// </summary>
        public virtual Boolean GetBoolean(IDataRecord dataRecord, Int32 ordinal, Boolean defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetBoolean(ordinal);
        }

        /// <summary>
        /// Retrieve a Bool value from a data record, or null if the specified value is not present.
        /// </summary>
        public Boolean? GetNullableBoolean(IDataRecord dataRecord, String columnName)
        {
            return GetNullableBoolean(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Bool value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual Boolean? GetNullableBoolean(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetBoolean(ordinal);
        }

        /// <summary>
        /// Retrieve a DateTime value from a data record.
        /// </summary>
        public DateTime GetDateTime(IDataRecord dataRecord, String columnName)
        {
            return GetDateTime(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a DateTime value from a data record with a default value.
        /// </summary>
        public DateTime GetDateTime(IDataRecord dataRecord, String columnName, DateTime defaultValue)
        {
            return GetDateTime(dataRecord, GetOrdinal(dataRecord, columnName), defaultValue);
        }

        /// <summary>
        /// Retrieve a DateTime value from a data record.
        /// </summary>
        public virtual DateTime GetDateTime(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToDateTime());
            else
                return dataRecord.GetDateTime(ordinal);
        }

        /// <summary>
        /// Retrieve a DateTime value from a data record with a default value.
        /// </summary>
        public virtual DateTime GetDateTime(IDataRecord dataRecord, Int32 ordinal, DateTime defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;
            else
                return dataRecord.GetDateTime(ordinal);
        }

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        public DateTime? GetNullableDateTime(IDataRecord dataRecord, String columnName)
        {
            return GetNullableDateTime(dataRecord, GetOrdinal(dataRecord, columnName));
        }

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        public virtual DateTime? GetNullableDateTime(IDataRecord dataRecord, Int32 ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return null;
            else
                return dataRecord.GetDateTime(ordinal);
        }

        /// <summary>
        /// Obtain the value of a parameter using its name.
        /// </summary>
        protected virtual Object GetValueFromCommand(IDbCommand dbCommand, String parameterName)
        {
            return ((IDataParameter)dbCommand.Parameters[parameterName]).Value;
        }

        /// <summary>
        /// Obtain the value of a parameter using its index.
        /// </summary>
        protected virtual Object GetValueFromCommand(IDbCommand dbCommand, Int32 parameterIndex)
        {
            return ((IDataParameter) dbCommand.Parameters[parameterIndex]).Value;
        }

	    /// <summary>
	    /// Retrieve an Enum value from an <see cref="IDbCommand" />.
	    /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
	    public T GetEnum<T>(IDbCommand dbCommand, String parameterName) where T : struct
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetEnum<T>(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Enum value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public T GetEnum<T>(IDbCommand dbCommand, String parameterName, T defaultValue) where T : struct
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetEnum<T>(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Enum value from an <see cref="IDbCommand" />.
	    /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
	    public T GetEnum<T>(IDbCommand dbCommand, Int32 parameterIndex) where T : struct
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetEnum<T>(GetValueFromCommand(dbCommand, parameterIndex));
        }

	    /// <summary>
	    /// Retrieve an Enum value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
	    public T GetEnum<T>(IDbCommand dbCommand, Int32 parameterIndex, T defaultValue) where T : struct
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetEnum(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
        }

        /// <summary>
        /// Retrieve an Enum value from a parameter value.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        protected virtual T GetEnum<T>(Object parameterValue) where T : struct
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new ArgumentException(RM.Get_Error_CannotConvertDBNullToEnum());

            Int32 enumValue = (Int32) parameterValue;
            if (Enum.IsDefined(typeof (T), enumValue))
                return (T) Enum.Parse(typeof (T), enumValue.ToString(CultureInfo.InvariantCulture));
            else
                throw new ArgumentException(RM.Get_Error_CannotConvertParameterToEnum());
        }

	    /// <summary>
        /// Retrieve an Enum value from a parameter value with a default value if null.
        /// </summary>
        protected virtual T GetEnum<T>(Object parameterValue, T defaultValue) where T : struct
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            Int32 enumValue = (Int32) parameterValue;
            if (Enum.IsDefined(typeof (T), enumValue))
                return (T) Enum.Parse(typeof (T), enumValue.ToString(CultureInfo.InvariantCulture));
            else
                throw new ArgumentException(RM.Get_Error_CannotConvertParameterToEnum());
        }

	    /// <summary>
	    /// Retrieve an Enum value from an <see cref="IDbCommand" />.
	    /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
	    public T? GetNullableEnum<T>(IDbCommand dbCommand, String parameterName) where T : struct
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableEnum<T>(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Enum value from an <see cref="IDbCommand" />.
	    /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
	    public T? GetNullableEnum<T>(IDbCommand dbCommand, Int32 parameterIndex) where T : struct
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetNullableEnum<T>(GetValueFromCommand(dbCommand, parameterIndex));
	    }

        /// <summary>
        /// Retrieve an Enum value from a parameter value.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        protected virtual T? GetNullableEnum<T>(Object parameterValue) where T : struct
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            Int32 enumValue = (Int32) parameterValue;
            if (Enum.IsDefined(typeof (T), enumValue))
                return (T) Enum.Parse(typeof (T), enumValue.ToString(CultureInfo.InvariantCulture));
            else
                throw new ArgumentException(RM.Get_Error_CannotConvertParameterToEnum());
        }

	    /// <summary>
	    /// Retrieve an Int32 value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Int32 GetInt32(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetInt32(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Int32 value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Int32 GetInt32(IDbCommand dbCommand, String parameterName, Int32 defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt32(GetValueFromCommand(dbCommand, parameterName), defaultValue);	        
	    }

	    /// <summary>
	    /// Retrieve an Int32 value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Int32 GetInt32(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt32(GetValueFromCommand(dbCommand, parameterIndex));	        
	    }

	    /// <summary>
	    /// Retrieve an Int32 value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Int32 GetInt32(IDbCommand dbCommand, Int32 parameterIndex, Int32 defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetInt32(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
	    }

        /// <summary>
        /// Retrieve an Int32 value from a parameter value.
        /// </summary>
        protected virtual Int32 GetInt32(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new ArgumentException(RM.Get_Error_CannotConvertDBNullToInt32());

            return Convert.ToInt32(parameterValue);
        }

        /// <summary>
        /// Retrieve an Int32 value from a parameter value with a default value if null.
        /// </summary>
        protected virtual Int32 GetInt32(Object parameterValue, Int32 defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToInt32(parameterValue);
        }

	    /// <summary>
	    /// Retrieve an Int32 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Int32? GetNullableInt32(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetNullableInt32(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Int32 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Int32? GetNullableInt32(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetNullableInt32(GetValueFromCommand(dbCommand, parameterIndex));
	    }

        /// <summary>
        /// Retrieve an Int32 value from a parameter value.
        /// </summary>
        protected virtual Int32? GetNullableInt32(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToInt32(parameterValue);
        }

	    /// <summary>
	    /// Retrieve an Int64 value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Int64 GetInt64(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt64(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Int64 value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Int64 GetInt64(IDbCommand dbCommand, String parameterName, Int64 defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt64(GetValueFromCommand(dbCommand, parameterName), defaultValue);
	    }

	    /// <summary>
	    /// Retrieve an Int64 value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Int64 GetInt64(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt64(GetValueFromCommand(dbCommand, parameterIndex));
        }

	    /// <summary>
	    /// Retrieve an Int64 value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Int64 GetInt64(IDbCommand dbCommand, Int32 parameterIndex, Int64 defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt64(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
	    }

        /// <summary>
        /// Retrieve an Int64 value from a parameter value.
        /// </summary>
        protected virtual Int64 GetInt64(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new ArgumentException(RM.Get_Error_CannotConvertDBNullToInt64());

            return Convert.ToInt64(parameterValue);
        }

        /// <summary>
        /// Retrieve an Int64 value from a parameter value using a default value if null.
        /// </summary>
        protected virtual Int64 GetInt64(Object parameterValue, Int64 defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToInt64(parameterValue);
        }

	    /// <summary>
	    /// Retrieve an Int64 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Int64? GetNullableInt64(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableInt64(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Int64 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Int64? GetNullableInt64(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableInt64(GetValueFromCommand(dbCommand, parameterIndex));
	    }

        /// <summary>
        /// Retrieve an Int64 value from a parameter value.
        /// </summary>
        protected virtual Int64? GetNullableInt64(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToInt64(parameterValue);
        }

	    /// <summary>
	    /// Retrieve an Int16 value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Int16 GetInt16(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetInt16(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Int16 value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Int16 GetInt16(IDbCommand dbCommand, String parameterName, Int16 defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt16(GetValueFromCommand(dbCommand, parameterName), defaultValue);
	    }

	    /// <summary>
	    /// Retrieve an Int16 value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Int16 GetInt16(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt16(GetValueFromCommand(dbCommand, parameterIndex));
        }

	    /// <summary>
	    /// Retrieve an Int16 value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Int16 GetInt16(IDbCommand dbCommand, Int32 parameterIndex, Int16 defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetInt16(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
        }

        /// <summary>
        /// Retrieve an Int16 value from a parameter value.
        /// </summary>
        protected virtual Int16 GetInt16(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new ArgumentException(RM.Get_Error_CannotConvertDBNullToInt16());

            return Convert.ToInt16(parameterValue);
        }

        /// <summary>
        /// Retrieve an Int16 value from a parameter value using a default if null.
        /// </summary>
        protected virtual Int16 GetInt16(Object parameterValue, Int16 defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToInt16(parameterValue);
        }

	    /// <summary>
	    /// Retrieve an Int16 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Int16? GetNullableInt16(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetNullableInt16(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve an Int16 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Int16? GetNullableInt16(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetNullableInt16(GetValueFromCommand(dbCommand, parameterIndex));
	    }

        /// <summary>
        /// Retrieve an Int16 value from a parameter value.
        /// </summary>
        protected virtual Int16? GetNullableInt16(object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToInt16(parameterValue);
        }

	    /// <summary>
	    /// Retrieve a Byte from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Byte GetByte(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetByte(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve a Byte from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Byte GetByte(IDbCommand dbCommand, String parameterName, Byte defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetByte(GetValueFromCommand(dbCommand, parameterName), defaultValue);
        }

	    /// <summary>
	    /// Retrieve a Byte from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Byte GetByte(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetByte(GetValueFromCommand(dbCommand, parameterIndex));
        }

	    /// <summary>
	    /// Retrieve a Byte from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Byte GetByte(IDbCommand dbCommand, Int32 parameterIndex, Byte defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetByte(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
	    }

        /// <summary>
        /// Retrieve an Byte value from a parameter value.
        /// </summary>
        protected virtual Byte GetByte(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new ArgumentException(RM.Get_Error_CannotConvertDBNullToByte());

            return Convert.ToByte(parameterValue);
        }

        /// <summary>
        /// Retrieve an Byte value from a parameter value using a default if null.
        /// </summary>
        protected virtual Byte GetByte(Object parameterValue, Byte defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToByte(parameterValue);
        }

	    /// <summary>
	    /// Retrieve a Byte value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Byte? GetNullableByte(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableByte(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve a Byte value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Byte? GetNullableByte(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableByte(GetValueFromCommand(dbCommand, parameterIndex));
	    }

        /// <summary>
        /// Retrieve a Byte value from parameter value.
        /// </summary>
        protected virtual Byte? GetNullableByte(object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToByte(parameterValue);
        }

	    /// <summary>
	    /// Retrieve a Single value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Single GetSingle(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetSingle(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve a Single value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Single GetSingle(IDbCommand dbCommand, String parameterName, Single defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetSingle(GetValueFromCommand(dbCommand, parameterName), defaultValue);
        }

	    /// <summary>
	    /// Retrieve a Single value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Single GetSingle(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetSingle(GetValueFromCommand(dbCommand, parameterIndex));
	    }

	    /// <summary>
	    /// Retrieve a Single value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Single GetSingle(IDbCommand dbCommand, Int32 parameterIndex, Single defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetSingle(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
	    }

        /// <summary>
        /// Retrieve an Single value from a parameter value.
        /// </summary>
        protected virtual Single GetSingle(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToSingle());

            return Convert.ToSingle(parameterValue);
        }

        /// <summary>
        /// Retrieve an Single value from a parameter value using a default if null.
        /// </summary>
        protected virtual Single GetSingle(Object parameterValue, Single defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToSingle(parameterValue);
        }

	    /// <summary>
	    /// Retrieve a Single value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Single? GetNullableSingle(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetNullableSingle(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve a Single value from an <see cref="IDbCommand" />, or null if the specified value is not present.
	    /// </summary>
	    public Single? GetNullableSingle(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableSingle(GetValueFromCommand(dbCommand, parameterIndex));
	    }

        /// <summary>
        /// Retrieve a Single value from a parameter value.
        /// </summary>
        protected virtual Single? GetNullableSingle(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToSingle(parameterValue);
        }

	    /// <summary>
	    /// Retrieve a Double value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Double GetDouble(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDouble(GetValueFromCommand(dbCommand, parameterName));
	    }

	    /// <summary>
	    /// Retrieve a Double value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Double GetDouble(IDbCommand dbCommand, String parameterName, Double defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDouble(GetValueFromCommand(dbCommand, parameterName), defaultValue);
	    }

	    /// <summary>
	    /// Retrieve a Double value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public Double GetDouble(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDouble(GetValueFromCommand(dbCommand, parameterIndex));
        }

	    /// <summary>
	    /// Retrieve a Double value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public Double GetDouble(IDbCommand dbCommand, Int32 parameterIndex, Double defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetDouble(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
	    }

        /// <summary>
        /// Retrieve an Double value from a parameter value.
        /// </summary>
        protected virtual Double GetDouble(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToDouble());

            return Convert.ToDouble(parameterValue);
        }

	    /// <summary>
        /// Retrieve an Double value from a parameter value using a default if null.
        /// </summary>
        protected virtual Double GetDouble(Object parameterValue, Double defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToDouble(parameterValue);
        }

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Double? GetNullableDouble(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableDouble(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Double? GetNullableDouble(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableDouble(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a Double value from a parameter value.
        /// </summary>
        protected virtual Double? GetNullableDouble(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToDouble(parameterValue);
        }

	    /// <summary>
	    /// Retrieve an Object value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public object GetValue(IDbCommand dbCommand, String parameterName)
	    {
            return GetValue(GetValueFromCommand(dbCommand, parameterName), null);
	    }

	    /// <summary>
	    /// Retrieve an Object value from an <see cref="IDbCommand" /> with default value.
	    /// </summary>
	    public Object GetValue(IDbCommand dbCommand, String parameterName, Object defaultValue)
	    {
            return GetValue(GetValueFromCommand(dbCommand, parameterName), defaultValue);
	    }

	    /// <summary>
	    /// Retrieve an Object value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public object GetValue(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            return GetValue(GetValueFromCommand(dbCommand, parameterIndex), null);
	    }

	    /// <summary>
	    /// Retrieve an Object value from an <see cref="IDbCommand" /> with default value.
	    /// </summary>
	    public Object GetValue(IDbCommand dbCommand, Int32 parameterIndex, Object defaultValue)
	    {
	        return GetValue(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
	    }

        /// <summary>
        /// Retrieve an Object value from a parameter value.
        /// </summary>
        protected virtual Object GetValue(Object parameterValue, Object defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return parameterValue;
        }

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />.
        /// </summary>
        public Guid GetGuid(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetGuid(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public Guid GetGuid(IDbCommand dbCommand, String parameterName, Guid defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetGuid(GetValueFromCommand(dbCommand, parameterName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />.
        /// </summary>
        public Guid GetGuid(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetGuid(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public Guid GetGuid(IDbCommand dbCommand, Int32 parameterIndex, Guid defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetGuid(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
        }

        /// <summary>
        /// Retrieve an Guid value from a parameter value.
        /// </summary>
        protected virtual Guid GetGuid(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToGuid());

            return (Guid)parameterValue;
        }

        /// <summary>
        /// Retrieve an Guid value from a parameter value using a default if null.
        /// </summary>
        protected virtual Guid GetGuid(Object parameterValue, Guid defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return (Guid)parameterValue;
        }

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Guid? GetNullableGuid(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableGuid(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Guid? GetNullableGuid(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableGuid(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a Guid value from a parameter value.
        /// </summary>
        protected virtual Guid? GetNullableGuid(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return (Guid)parameterValue;
        }

	    /// <summary>
	    /// Retrieve a String value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public String GetString(IDbCommand dbCommand, String parameterName)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

	        return GetString(GetValueFromCommand(dbCommand, parameterName), null);
	    }

	    /// <summary>
	    /// Retrieve a String value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public String GetString(IDbCommand dbCommand, String parameterName, String defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetString(GetValueFromCommand(dbCommand, parameterName), defaultValue);
	    }

	    /// <summary>
	    /// Retrieve a String value from an <see cref="IDbCommand" />.
	    /// </summary>
	    public String GetString(IDbCommand dbCommand, Int32 parameterIndex)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetString(GetValueFromCommand(dbCommand, parameterIndex), null);
	    }

	    /// <summary>
	    /// Retrieve a String value from an <see cref="IDbCommand" /> with a default value if null.
	    /// </summary>
	    public String GetString(IDbCommand dbCommand, Int32 parameterIndex, String defaultValue)
	    {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetString(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
	    }

        /// <summary>
        /// Retrieve a String from a parameter value using a default value if null. 
        /// </summary>
        protected virtual String GetString(Object parameterValue, String defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToString(parameterValue);
        }

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />.
        /// </summary>
        public Decimal GetDecimal(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDecimal(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public Decimal GetDecimal(IDbCommand dbCommand, String parameterName, Decimal defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDecimal(GetValueFromCommand(dbCommand, parameterName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />.
        /// </summary>
        public Decimal GetDecimal(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDecimal(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public Decimal GetDecimal(IDbCommand dbCommand, Int32 parameterIndex, Decimal defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDecimal(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
        }

        /// <summary>
        /// Retrieve an Decimal value from a parameter value.
        /// </summary>
        protected virtual Decimal GetDecimal(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToDecimal());

            return Convert.ToDecimal(parameterValue);
        }

        /// <summary>
        /// Retrieve an Decimal value from a parameter value using a default if null.
        /// </summary>
        protected virtual Decimal GetDecimal(Object parameterValue, Decimal defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToDecimal(parameterValue);
        }

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Decimal? GetNullableDecimal(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableDecimal(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Decimal? GetNullableDecimal(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableDecimal(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a Decimal value from a parameter value.
        /// </summary>
        protected virtual Decimal? GetNullableDecimal(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToDecimal(parameterValue);
        }

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" />.
        /// </summary>
        public Boolean GetBoolean(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetBoolean(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public Boolean GetBoolean(IDbCommand dbCommand, String parameterName, Boolean defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetBoolean(GetValueFromCommand(dbCommand, parameterName), defaultValue);
        }

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" />.
        /// </summary>
        public Boolean GetBoolean(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetBoolean(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public Boolean GetBoolean(IDbCommand dbCommand, Int32 parameterIndex, Boolean defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetBoolean(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
        }

        /// <summary>
        /// Retrieve an Boolean value from a parameter value.
        /// </summary>
        protected virtual Boolean GetBoolean(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToBoolean());

            return Convert.ToBoolean(parameterValue);
        }

        /// <summary>
        /// Retrieve an Boolean value from a parameter value using a default if null.
        /// </summary>
        protected virtual Boolean GetBoolean(Object parameterValue, Boolean defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToBoolean(parameterValue);
        }

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Boolean? GetNullableBoolean(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableBoolean(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public Boolean? GetNullableBoolean(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableBoolean(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a Boolean value from a parameter value.
        /// </summary>
        protected virtual Boolean? GetNullableBoolean(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToBoolean(parameterValue);
        }

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" />.
        /// </summary>
        public DateTime GetDateTime(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDateTime(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public DateTime GetDateTime(IDbCommand dbCommand, String parameterName, DateTime defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDateTime(GetValueFromCommand(dbCommand, parameterName), defaultValue);
        }

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" />.
        /// </summary>
        public DateTime GetDateTime(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDateTime(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        public DateTime GetDateTime(IDbCommand dbCommand, Int32 parameterIndex, DateTime defaultValue)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetDateTime(GetValueFromCommand(dbCommand, parameterIndex), defaultValue);
        }

        /// <summary>
        /// Retrieve an DateTime value from a parameter value.
        /// </summary>
        protected virtual DateTime GetDateTime(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToDateTime());

            return Convert.ToDateTime(parameterValue);
        }

        /// <summary>
        /// Retrieve an DateTime value from a parameter value using a default if null.
        /// </summary>
        protected virtual DateTime GetDateTime(Object parameterValue, DateTime defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            return Convert.ToDateTime(parameterValue);
        }

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public DateTime? GetNullableDateTime(IDbCommand dbCommand, String parameterName)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableDateTime(GetValueFromCommand(dbCommand, parameterName));
        }

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        public DateTime? GetNullableDateTime(IDbCommand dbCommand, Int32 parameterIndex)
        {
            if (dbCommand == null)
                throw new ArgumentNullException("dbCommand");

            return GetNullableDateTime(GetValueFromCommand(dbCommand, parameterIndex));
        }

        /// <summary>
        /// Retrieve a DateTime value from a parameter value.
        /// </summary>
        protected virtual DateTime? GetNullableDateTime(Object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            return Convert.ToDateTime(parameterValue);
        }
	}
}