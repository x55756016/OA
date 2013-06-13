using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Core;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Configuration.Assemblies;
using System.Configuration;

namespace SMT.SaaS.PublicInterface.DAL
{
    public class CommDAL<TEntity> : BaseDAL
    {
        private static string conn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
        public IQueryable<TEntity> GetTable()
        {
            return base.GetTable<TEntity>();
        }
        public ObjectQuery<TEntity> GetObjects()
        {
            return base.GetObjects<TEntity>();
        }
        public object ExecuteCustomerSql(string Sqlstring)
        {
            OracleDAO dao = new OracleDAO(conn);
            object obj = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text);
            //ParameterCollection prameters = new ParameterCollection();
            //object obj1 = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text, prameters);
            return obj;
        }
    }
}
