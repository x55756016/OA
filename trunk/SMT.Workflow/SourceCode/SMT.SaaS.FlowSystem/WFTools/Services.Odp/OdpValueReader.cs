using System;
using System.Data;
using WFTools.Services.Common.Ado;

namespace WFTools.Services.Odp
{
    /// <summary>
    /// ODP.NET specific implementation of <see cref="IAdoValueReader" />.
    /// </summary>
    /// <remarks>
    /// This implementation modifies the GUID retrieval functionality
    /// to use a string instead.
    /// </remarks>
    public class OdpValueReader : DefaultAdoValueReader
    {
        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" />.
        /// </summary>
        protected override Guid GetGuid(object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToGuid());

            try
            {
                return new Guid(base.GetString(parameterValue, null));
            }
            catch
            {
                throw new InvalidCastException(RM.Get_Error_CannotConvertParameterToEnum());
            }
        }

        /// <summary>
        /// Retrieve a Guid value from an <see cref="IDbCommand" /> with a default value if null.
        /// </summary>
        protected override Guid GetGuid(object parameterValue, Guid defaultValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return defaultValue;

            try
            {
                return new Guid(base.GetString(parameterValue, null));
            }
            catch (ArgumentNullException)
            {
                return defaultValue;
            }
            catch (FormatException)
            {
                return defaultValue;
            }
            catch (OverflowException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retrieve a Guid value from a data record.
        /// </summary>
        public override Guid GetGuid(IDataRecord dataRecord, int ordinal)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                throw new InvalidCastException(RM.Get_Error_CannotConvertDBNullToGuid());
            
            try
            {
                return new Guid(base.GetString(dataRecord, ordinal));
            }
            catch
            {
                throw new InvalidCastException(RM.Get_Error_CannotConvertColumnToEnum());
            }
        }

        /// <summary>
        /// Retrieve a Guid value from a data record with a default value.
        /// </summary>
        public override Guid GetGuid(IDataRecord dataRecord, int ordinal, Guid defaultValue)
        {
            if (dataRecord == null)
                throw new ArgumentNullException("dataRecord");

            if (dataRecord.IsDBNull(ordinal))
                return defaultValue;

            try
            {
                return new Guid(base.GetString(dataRecord, ordinal));
            }
            catch (ArgumentNullException)
            {
                return defaultValue;
            }
            catch (FormatException)
            {
                return defaultValue;
            }
            catch (OverflowException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Retrieve a Guid value from a parameter value.
        /// </summary>
        protected override Guid? GetNullableGuid(object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
                return null;

            try
            {
                return new Guid(base.GetString(parameterValue, null));
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }
    }
}