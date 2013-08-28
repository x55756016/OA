using System;
using System.Data;
using System.Data.Common;
using Oracle.DataAccess.Client;
using WFTools.Services.Common.Ado;

namespace WFTools.Services.Odp
{
    /// <summary>
    /// ODP.NET specific implementation of <see cref="IAdoResourceProvider" />.
    /// </summary>
    /// <remarks>
    /// This implementation modifies the parameter value/type population
    /// so that ref cursors and GUIDs are correctly populated.
    /// </remarks>
    public class OdpResourceProvider : DefaultAdoResourceProvider
    {
        /// <summary>
        /// Indicates that this provider supports cursors.
        /// </summary>
        protected override bool SupportCursors
        {
            get { return true; }
        }

        /// <summary>
        /// Replaces any existing 'enlist' parameter in the connection string
        /// with a value indicating that manual enlist is necessary.
        /// </summary>
        /// <remarks>
        /// ODP.NET supports 3 values for 'enlist'; 'true', 'false' and 'dynamic'.
        ///  'dynamic' effectively works the same as 'false' in System.Data.OracleClient.
        /// </remarks>
        protected override void ReplaceEnlistInConnectionString(DbConnectionStringBuilder dbConnectionStringBuilder)
        {
            if (dbConnectionStringBuilder.ContainsKey("enlist"))
                dbConnectionStringBuilder.Remove("enlist");

            dbConnectionStringBuilder.Add("enlist", "dynamic");
        }

        /// <summary>
        /// Populate the specified <see cref="DbParameter" /> with its value.
        /// </summary>
        /// <remarks>
        /// This implementation makes sure that GUIDs are passed as strings and
        /// boolean values are passed as Int16 values.
        /// </remarks>
        protected override void PopulateValue(DbParameter dbParameter, object value)
        {
            if (value is Guid)
                dbParameter.Value = value.ToString();
            else if (value is Boolean)
                dbParameter.Value = ((Boolean) value) ? 1 : 0;
            else
                base.PopulateValue(dbParameter, value);
        }

        /// <summary>
        /// Populate the specified <see cref="DbParameter" /> with its type.
        /// </summary>
        /// <remarks>
        /// This implementation makes sure that GUIDs are passed as strings, 
        /// boolean values are passed as Int16 and ref cursors are added.
        /// </remarks>
        protected override void PopulateType(DbParameter dbParameter, AdoDbType type)
        {
            OracleParameter oracleParameter = dbParameter as OracleParameter;
            if (oracleParameter == null)
                throw new ArgumentException(RM.Get_Error_OracleParameterExpected());

            if (type == AdoDbType.Guid)
            {
                oracleParameter.DbType = DbType.String;
                oracleParameter.Size = 36;
            }
            else if (type == AdoDbType.Boolean)
            {
                oracleParameter.DbType = DbType.Int16;
                oracleParameter.Scale = 1;
            }
            else if (type == AdoDbType.Cursor)
                oracleParameter.OracleDbType = OracleDbType.RefCursor;
            else
                base.PopulateType(dbParameter, type);
        }
    }
}