using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services.Common.Ado
{
    /// <summary>
    /// Interface used for retrieving data from an <see cref="IDataRecord" />
    /// or <see cref="IDbCommand" />.
    /// </summary>
    public interface IAdoValueReader
    {
        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDataRecord dataRecord, String columnName) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from a data record with a default value if null.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDataRecord dataRecord, String columnName, T defaultValue) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDataRecord dataRecord, Int32 ordinal) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from a data record with a default value if null.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDataRecord dataRecord, Int32 ordinal, T defaultValue) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T? GetNullableEnum<T>(IDataRecord dataRecord, String columnName) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from a data record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T? GetNullableEnum<T>(IDataRecord dataRecord, Int32 ordinal) where T : struct;

        /// <summary>
        /// Retrieve an Int32 value from a data record.
        /// </summary>
        Int32 GetInt32(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve an Int32 value from a data record with a default value if null.
        /// </summary>
        Int32 GetInt32(IDataRecord dataRecord, String columnName, Int32 defaultValue);

        /// <summary>
        /// Retrieve an Int32 value from a data record.
        /// </summary>
        Int32 GetInt32(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Int32 value from a data record with a default value if null.
        /// </summary>
        Int32 GetInt32(IDataRecord dataRecord, Int32 ordinal, Int32 defaultValue);

        /// <summary>
        /// Retrieve an Int32 value from a data record, or null if the specified value is not present.
        /// </summary>
        Int32? GetNullableInt32(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve an Int32 value from a data record, or null if the specified value is not present.
        /// </summary>
        Int32? GetNullableInt32(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Int64 value from a data record.
        /// </summary>
        Int64 GetInt64(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve an Int64 value from a data record with a default value if null.
        /// </summary>
        Int64 GetInt64(IDataRecord dataRecord, String columnName, Int64 defaultValue);

        /// <summary>
        /// Retrieve an Int64 value from a data record.
        /// </summary>
        Int64 GetInt64(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Int64 value from a data record with a default value if null.
        /// </summary>
        Int64 GetInt64(IDataRecord dataRecord, Int32 ordinal, Int64 defaultValue);

        /// <summary>
        /// Retrieve an Int64 value from a data record, or null if the specified value is not present.
        /// </summary>
        Int64? GetNullableInt64(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve an Int64 value from a data record, or null if the specified value is not present.
        /// </summary>
        Int64? GetNullableInt64(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Int16 value from a data record.
        /// </summary>
        Int16 GetInt16(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve an Int16 value from a data record with a default value if null.
        /// </summary>
        Int16 GetInt16(IDataRecord dataRecord, String columnName, Int16 defaultValue);

        /// <summary>
        /// Retrieve an Int16 value from a data record.
        /// </summary>
        Int16 GetInt16(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Int16 value from a data record with a default value if null.
        /// </summary>
        Int16 GetInt16(IDataRecord dataRecord, Int32 ordinal, Int16 defaultValue);

        /// <summary>
        /// Retrieve an Int16 value from a data record, or null if the specified value is not present.
        /// </summary>
        Int16? GetNullableInt16(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve an Int16 value from a data record, or null if the specified value is not present.
        /// </summary>
        Int16? GetNullableInt16(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Byte from a data record.
        /// </summary>
        Byte GetByte(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Byte from a data record with a default value if null.
        /// </summary>
        Byte GetByte(IDataRecord dataRecord, String columnName, Byte defaultValue);

        /// <summary>
        /// Retrieve a Byte from a data record.
        /// </summary>
        Byte GetByte(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Byte from a data record with a default value if null.
        /// </summary>
        Byte GetByte(IDataRecord dataRecord, Int32 ordinal, Byte defaultValue);

        /// <summary>
        /// Retrieve a Byte value from a data record, or null if the specified value is not present.
        /// </summary>
        Byte? GetNullableByte(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Byte value from a data record, or null if the specified value is not present.
        /// </summary>
        Byte? GetNullableByte(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Single value from a data record.
        /// </summary>
        Single GetSingle(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Single value from a data record with a default value if null.
        /// </summary>
        Single GetSingle(IDataRecord dataRecord, String columnName, Single defaultValue);

        /// <summary>
        /// Retrieve a Single value from a data record.
        /// </summary>
        Single GetSingle(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Single value from a data record with a default value if null.
        /// </summary>
        Single GetSingle(IDataRecord dataRecord, Int32 ordinal, Single defaultValue);

        /// <summary>
        /// Retrieve a Single value from a data record, or null if the specified value is not present.
        /// </summary>
        Single? GetNullableSingle(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Single value from a data record, or null if the specified value is not present.
        /// </summary>
        Single? GetNullableSingle(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Double value from a data record.
        /// </summary>
        Double GetDouble(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Double value from a data record with a default value if null.
        /// </summary>
        Double GetDouble(IDataRecord dataRecord, String columnName, Double defaultValue);

        /// <summary>
        /// Retrieve a Double value from a data record.
        /// </summary>
        Double GetDouble(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Double value from a data record with a default value if null.
        /// </summary>
        Double GetDouble(IDataRecord dataRecord, Int32 ordinal, Double defaultValue);

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        Double? GetNullableDouble(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        Double? GetNullableDouble(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Object value from a data record.
        /// </summary>
        Object GetValue(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve an Object value from a data record with default value.
        /// </summary>
        Object GetValue(IDataRecord dataRecord, String columnName, Object defaultValue);

        /// <summary>
        /// Retrieve an Object value from a data record.
        /// </summary>
        Object GetValue(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Object value from a data record with default value.
        /// </summary>
        Object GetValue(IDataRecord dataRecord, Int32 ordinal, Object defaultValue);

        /// <summary>
        /// Retrieve a Guid value from a data record.
        /// </summary>
        Guid GetGuid(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Guid value from a data record with a default value if null.
        /// </summary>
        Guid GetGuid(IDataRecord dataRecord, String columnName, Guid defaultValue);

        /// <summary>
        /// Retrieve a Guid value from a data record.
        /// </summary>
        Guid GetGuid(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Guid value from a data record with a default value if null.
        /// </summary>
        Guid GetGuid(IDataRecord dataRecord, Int32 ordinal, Guid defaultValue);

        /// <summary>
        /// Retrieve a String value from a data record.
        /// </summary>
        String GetString(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a String value from a data record with a default value if null.
        /// </summary>
        String GetString(IDataRecord dataRecord, String columnName, String defaultValue);

        /// <summary>
        /// Retrieve a String value from a data record.
        /// </summary>
        String GetString(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a String value from a data record with a default value if null.
        /// </summary>
        String GetString(IDataRecord dataRecord, Int32 ordinal, String defaultValue);

        /// <summary>
        /// Retrieve a Decimal value from a data record.
        /// </summary>
        Decimal GetDecimal(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Decimal value from a data record with a default value if null.
        /// </summary>
        Decimal GetDecimal(IDataRecord dataRecord, String columnName, Decimal defaultValue);

        /// <summary>
        /// Retrieve a Decimal value from a data record.
        /// </summary>
        Decimal GetDecimal(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Decimal value from a data record with a default value if null.
        /// </summary>
        Decimal GetDecimal(IDataRecord dataRecord, Int32 ordinal, Decimal defaultValue);

        /// <summary>
        /// Retrieve a Decimal value from a data record, or null if the specified value is not present.
        /// </summary>
        Decimal? GetNullableDecimal(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Decimal value from a data record, or null if the specified value is not present.
        /// </summary>
        Decimal? GetNullableDecimal(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Boolean value from a data record.
        /// </summary>
        Boolean GetBoolean(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Boolean value from a data record with a default value if null.
        /// </summary>
        Boolean GetBoolean(IDataRecord dataRecord, String columnName, Boolean defaultValue);

        /// <summary>
        /// Retrieve a Boolean value from a data record.
        /// </summary>
        Boolean GetBoolean(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a Boolean value from a data record with a default value if null.
        /// </summary>
        Boolean GetBoolean(IDataRecord dataRecord, Int32 ordinal, Boolean defaultValue);

        /// <summary>
        /// Retrieve a Bool value from a data record, or null if the specified value is not present.
        /// </summary>
        Boolean? GetNullableBoolean(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Bool value from a data record, or null if the specified value is not present.
        /// </summary>
        Boolean? GetNullableBoolean(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a DateTime value from a data record.
        /// </summary>
        DateTime GetDateTime(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a DateTime value from a data record with a default value if null.
        /// </summary>
        DateTime GetDateTime(IDataRecord dataRecord, String columnName, DateTime defaultValue);

        /// <summary>
        /// Retrieve a DateTime value from a data record.
        /// </summary>
        DateTime GetDateTime(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve a DateTime value from a data record with a default value if null.
        /// </summary>
        DateTime GetDateTime(IDataRecord dataRecord, Int32 ordinal, DateTime defaultValue);

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        DateTime? GetNullableDateTime(IDataRecord dataRecord, String columnName);

        /// <summary>
        /// Retrieve a Double value from a data record, or null if the specified value is not present.
        /// </summary>
        DateTime? GetNullableDateTime(IDataRecord dataRecord, Int32 ordinal);

        /// <summary>
        /// Retrieve an Enum value from an <see cref="IDbCommand" />.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDbCommand dbCommand, String parameterName) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDbCommand dbCommand, String parameterName, T defaultValue) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from an <see cref="IDbCommand" />.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDbCommand dbCommand, Int32 parameterIndex) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T GetEnum<T>(IDbCommand dbCommand, Int32 parameterIndex, T defaultValue) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from an <see cref="IDbCommand" />.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T? GetNullableEnum<T>(IDbCommand dbCommand, String parameterName) where T : struct;

        /// <summary>
        /// Retrieve an Enum value from an <see cref="IDbCommand" />.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T? GetNullableEnum<T>(IDbCommand dbCommand, Int32 parameterIndex) where T : struct;

        /// <summary>
        /// Retrieve an Int32 value from an <see cref="IDbCommand" />.
        /// </summary>
        Int32 GetInt32(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve an Int32 value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Int32 GetInt32(IDbCommand dbCommand, String parameterName, Int32 defaultValue);

        /// <summary>
        /// Retrieve an Int32 value from an <see cref="IDbCommand" />.
        /// </summary>
        Int32 GetInt32(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve an Int32 value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Int32 GetInt32(IDbCommand dbCommand, Int32 parameterIndex, Int32 defaultValue);

        /// <summary>
        /// Retrieve an Int32 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Int32? GetNullableInt32(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve an Int32 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Int32? GetNullableInt32(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve an Int64 value from an <see cref="IDbCommand" />.
        /// </summary>
        Int64 GetInt64(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve an Int64 value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Int64 GetInt64(IDbCommand dbCommand, String parameterName, Int64 defaultValue);

        /// <summary>
        /// Retrieve an Int64 value from an <see cref="IDbCommand" />.
        /// </summary>
        Int64 GetInt64(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve an Int64 value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Int64 GetInt64(IDbCommand dbCommand, Int32 parameterIndex, Int64 defaultValue);

        /// <summary>
        /// Retrieve an Int64 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Int64? GetNullableInt64(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve an Int64 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Int64? GetNullableInt64(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve an Int16 value from an <see cref="IDbCommand" />.
        /// </summary>
        Int16 GetInt16(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve an Int16 value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Int16 GetInt16(IDbCommand dbCommand, String parameterName, Int16 defaultValue);

        /// <summary>
        /// Retrieve an Int16 value from an <see cref="IDbCommand" />.
        /// </summary>
        Int16 GetInt16(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve an Int16 value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Int16 GetInt16(IDbCommand dbCommand, Int32 parameterIndex, Int16 defaultValue);

        /// <summary>
        /// Retrieve an Int16 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Int16? GetNullableInt16(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve an Int16 value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Int16? GetNullableInt16(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Byte from an <see cref="IDbCommand" />.
        /// </summary>
        Byte GetByte(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Byte from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Byte GetByte(IDbCommand dbCommand, String parameterName, Byte defaultValue);

        /// <summary>
        /// Retrieve a Byte from an <see cref="IDbCommand" />.
        /// </summary>
        Byte GetByte(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Byte from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Byte GetByte(IDbCommand dbCommand, Int32 parameterIndex, Byte defaultValue);

        /// <summary>
        /// Retrieve a Byte value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Byte? GetNullableByte(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Byte value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Byte? GetNullableByte(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Single value from an <see cref="IDbCommand" />.
        /// </summary>
        Single GetSingle(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Single value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Single GetSingle(IDbCommand dbCommand, String parameterName, Single defaultValue);

        /// <summary>
        /// Retrieve a Single value from an <see cref="IDbCommand" />.
        /// </summary>
        Single GetSingle(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Single value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Single GetSingle(IDbCommand dbCommand, Int32 parameterIndex, Single defaultValue);

        /// <summary>
        /// Retrieve a Single value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Single? GetNullableSingle(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Single value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Single? GetNullableSingle(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />.
        /// </summary>
        Double GetDouble(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Double GetDouble(IDbCommand dbCommand, String parameterName, Double defaultValue);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />.
        /// </summary>
        Double GetDouble(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Double GetDouble(IDbCommand dbCommand, Int32 parameterIndex, Double defaultValue);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Double? GetNullableDouble(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Double? GetNullableDouble(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve an Object value from an <see cref="IDbCommand" />.
        /// </summary>
        Object GetValue(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve an Object value from an <see cref="IDbCommand" /> with default value.
        /// </summary>
        Object GetValue(IDbCommand dbCommand, String parameterName, Object defaultValue);

        /// <summary>
        /// Retrieve an Object value from an <see cref="IDbCommand" />.
        /// </summary>
        Object GetValue(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve an Object value from an <see cref="IDbCommand" /> with default value.
        /// </summary>
        Object GetValue(IDbCommand dbCommand, Int32 parameterIndex, Object defaultValue);

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />.
        /// </summary>
        Guid GetGuid(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Guid GetGuid(IDbCommand dbCommand, String parameterName, Guid defaultValue);

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />.
        /// </summary>
        Guid GetGuid(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Guid GetGuid(IDbCommand dbCommand, Int32 parameterIndex, Guid defaultValue);

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />.
        /// </summary>
        Guid? GetNullableGuid(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />.
        /// </summary>
        Guid? GetNullableGuid(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a String value from an <see cref="IDbCommand" />.
        /// </summary>
        String GetString(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a String value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        String GetString(IDbCommand dbCommand, String parameterName, String defaultValue);

        /// <summary>
        /// Retrieve a String value from an <see cref="IDbCommand" />.
        /// </summary>
        String GetString(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a String value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        String GetString(IDbCommand dbCommand, Int32 parameterIndex, String defaultValue);

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />.
        /// </summary>
        Decimal GetDecimal(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Decimal GetDecimal(IDbCommand dbCommand, String parameterName, Decimal defaultValue);

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />.
        /// </summary>
        Decimal GetDecimal(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Decimal GetDecimal(IDbCommand dbCommand, Int32 parameterIndex, Decimal defaultValue);

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Decimal? GetNullableDecimal(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Decimal value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Decimal? GetNullableDecimal(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" />.
        /// </summary>
        Boolean GetBoolean(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Boolean GetBoolean(IDbCommand dbCommand, String parameterName, Boolean defaultValue);

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" />.
        /// </summary>
        Boolean GetBoolean(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a Boolean value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        Boolean GetBoolean(IDbCommand dbCommand, Int32 parameterIndex, Boolean defaultValue);

        /// <summary>
        /// Retrieve a Bool value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Boolean? GetNullableBoolean(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Bool value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        Boolean? GetNullableBoolean(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" />.
        /// </summary>
        DateTime GetDateTime(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        DateTime GetDateTime(IDbCommand dbCommand, String parameterName, DateTime defaultValue);

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" />.
        /// </summary>
        DateTime GetDateTime(IDbCommand dbCommand, Int32 parameterIndex);

        /// <summary>
        /// Retrieve a DateTime value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        DateTime GetDateTime(IDbCommand dbCommand, Int32 parameterIndex, DateTime defaultValue);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        DateTime? GetNullableDateTime(IDbCommand dbCommand, String parameterName);

        /// <summary>
        /// Retrieve a Double value from an <see cref="IDbCommand" />, or null if the specified value is not present.
        /// </summary>
        DateTime? GetNullableDateTime(IDbCommand dbCommand, Int32 parameterIndex);
    }
}