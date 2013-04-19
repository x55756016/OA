using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Core;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Reflection;
using System.Configuration;
using System.Data;
using SMT.Foundation.Log;

namespace SMT.HRM.DAL
{
    public class CommDal<TEntity> : BaseDAL
    {
        private static string conn = ConfigurationManager.AppSettings["ConnectionString"].ToString();


        public object ExecuteStoredProcedure(string Sqlstring, ParameterCollection prameters)
        {
            OracleDAO dao = new OracleDAO(conn);
            //prameters = new ParameterCollection();
            //prameters.Add("parme1", null);
            return dao.ExecuteScalar("", System.Data.CommandType.StoredProcedure, prameters);
        }

        public object ExecuteCustomerSql(string Sqlstring, ParameterCollection prameters)
        {
            try
            {
                OracleDAO dao = new OracleDAO(conn);
                //prameters = new ParameterCollection();
                //prameters.Add("parme1", null);
                return dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text, prameters);

                //return object obj=dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);
                //prameters = new ParameterCollection();
                //object obj1 = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text, prameters);
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                return null;

            }
        }

        public object ExecuteCustomerSql(string Sqlstring)
        {
            try
            {
                OracleDAO dao = new OracleDAO(conn);
                object obj = dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);
                //ParameterCollection prameters = new ParameterCollection();
                //object obj1 = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text, prameters);
                return obj;
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                return null;

            }
        }

        public DataTable GetDataTableByCustomerSql(string Sqlstring)
        {
            OracleDAO dao = new OracleDAO(conn);
            DataTable dt = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text);
            return dt;
        }

        public List<TEntity> GetListEntitiesWithPagingBySql<TEntity>(string strSql, int pageIndex, int pageSize)
            where TEntity : class
        {
            OracleDAO dao = new OracleDAO(conn);
            return dao.GetListEntitiesWithPaging<TEntity>(strSql, pageIndex, pageSize);
        }

        //public bool Delete(TEntity entity)
        //{
        //    try
        //    {
        //        base.Delete(entity);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //        throw (ex);
        //    }
        //}

        /// <summary>
        /// 修改实体审核状态
        /// </summary>
        /// <param name="strEntityName">实体名</param>
        /// <param name="EntityKeyName">主键名</param>
        /// <param name="EntityKeyValue">主键值</param>
        /// <param name="CheckState">审核状态</param>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            int IntReturn = 0;
            string qualifiedEntitySetName = ConfigurationManager.AppSettings["DBContextName"] + ".";
            Dictionary<object, object> Prameters = new Dictionary<object, object>();
            Prameters.Add("CHECKSTATE", CheckState);
            try
            {
                System.Data.EntityKey entityKey = new System.Data.EntityKey(qualifiedEntitySetName+strEntityName, EntityKeyName, EntityKeyValue);
                object obj = GetObjectByEntityKey(entityKey);
                if (obj != null)
                {
                    Type a = obj.GetType();

                    PropertyInfo[] infos = a.GetProperties();
                    foreach (PropertyInfo prop in infos)
                    {
                        if (Prameters.ContainsKey(prop.Name))
                        {
                            prop.SetValue(obj, Prameters[prop.Name], null);
                        }
                    }
                   IntReturn = base.Update(obj);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("手机审单调用了" + strEntityName + "表单ID为：" + EntityKeyValue + "错误信息为："+ System.DateTime.Now.ToString() + ex.ToString());
                throw (ex);
            }
            return IntReturn;
        }



        public IQueryable<TEntity> GetTable()
        {
            return base.GetTable<TEntity>();
        }

        public ObjectQuery<TEntity> GetObjects()
        {
            return base.GetObjects<TEntity>();
        }

    }


}
