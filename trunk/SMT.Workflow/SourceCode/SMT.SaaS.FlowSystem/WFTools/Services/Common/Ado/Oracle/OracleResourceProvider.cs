using System;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;

namespace WFTools.Services.Common.Ado.Oracle
{
    /// <summary>
    /// Oracle specific implementation of <see cref="IAdoResourceProvider" />.
    /// </summary>
    /// <remarks>
    /// This implementation modifies the parameter value/type population
    /// so that ref cursors and GUIDs are correctly populated.
    /// </remarks>
    public class OracleResourceProvider : DefaultAdoResourceProvider
    {
        /// <summary>
        /// Indicates that this provider supports cursors.
        /// </summary>
        protected override bool SupportCursors
        {
            get { return true; }
        }

        /// <summary>
        /// Populate the specified <see cref="DbParameter" /> with its value.
        /// </summary>
        /// <remarks>
        /// This implementation makes sure that GUIDs are passed as strings.
        /// </remarks>
        /// <param name="dbParameter"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override void PopulateValue(DbParameter dbParameter, object value)
        {
            if (value is Guid)
                dbParameter.Value = value.ToString();
            else
                base.PopulateValue(dbParameter, value);
        }

        /// <summary>
        /// Populate the specified <see cref="DbParameter" /> with its type.
        /// </summary>
        /// <remarks>
        /// This implementation makes sure that GUIDs are passed as strings
        /// and ref cursors are added.
        /// </remarks>
        /// <param name="dbParameter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
            else if (type == AdoDbType.Cursor)
                oracleParameter.OracleType = OracleType.Cursor;
            else if (type == AdoDbType.Text)
                oracleParameter.OracleType = OracleType.NClob;
            else
                base.PopulateType(dbParameter, type);
        }
    }
}