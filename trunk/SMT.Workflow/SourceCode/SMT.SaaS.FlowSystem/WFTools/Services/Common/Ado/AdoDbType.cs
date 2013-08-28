using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services.Common.Ado
{
    /// <summary>
    /// Copy of <see cref="DbType" /> that adds support for cursors.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", 
        Justification = "Mirroring ADO DbType enumeration.")]
    public enum AdoDbType
    {
        AnsiString = 0,
        Binary = 1,
        Boolean = 3,
        Byte = 2,
        Currency = 4,
        Date = 5,
        DateTime = 6,
        Decimal = 7,
        Double = 8,
        Guid = 9,
        Int16 = 10,
        Int32 = 11,
        Int64 = 12,
        Object = 13,
        SByte = 14,
        Single = 15,
        String = 16,
        Time = 17,
        UInt16 = 18,
        UInt32 = 19,
        UInt64 = 20,
        VarNumeric = 21,
        AnsiStringFixedLength = 22,
        StringFixedLength = 23,
        Cursor = 24,
        Xml = 25,
        Text = 26,
    }
}