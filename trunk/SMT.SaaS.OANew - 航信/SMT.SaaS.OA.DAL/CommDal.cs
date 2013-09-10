using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Core;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Reflection;
using System.Configuration;
using SMT.Foundation.Log;
using SMT.SaaS.BLLCommonServices.BllCommonUserPermissionWS;

namespace SMT.SaaS.OA.DAL
{
    public class CommDaL<TEntity> : BaseDAL
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

        public object ExecuteCustomerSql(string Sqlstring, ParameterCollection prameters)
        {
            OracleDAO dao = new OracleDAO(conn);
            
            return dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text, prameters);

        }

        public object ExecuteCustomerSql(string Sqlstring)
        {
            OracleDAO dao = new OracleDAO(conn);
            object obj = dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);
            
            return obj;
        }
        /// <summary>
        /// 修改实体审核状态
        /// </summary>
        /// <param name="strEntityName">实体名</param>
        /// <param name="EntityKeyName">主键名</param>
        /// <param name="EntityKeyValue">主键值</param>
        /// <param name="CheckState">审核状态</param>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            int UpdateCont = 0;
            string qualifiedEntitySetName = ConfigurationManager.AppSettings["DBContextName"] + ".";
            Dictionary<object, object> Prameters = new Dictionary<object, object>();
            Prameters.Add("CHECKSTATE", CheckState);
            try
            {
                System.Data.EntityKey entityKey = new System.Data.EntityKey(qualifiedEntitySetName + strEntityName, EntityKeyName, EntityKeyValue);
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
                    UpdateCont=base.Update(obj);
                    if (UpdateCont > 0)
                    {
                        Tracer.Debug("手机版修改表单状态，表单名：" + strEntityName + "单据号：" + EntityKeyValue + System.DateTime.Now.ToString() );
                        BLLCommonServices.Utility.SubmitMyRecord<TEntity>(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("手机版修改表单状态，表单名：" + strEntityName + "单据号：" + EntityKeyValue +System.DateTime.Now.ToString() + " " + ex.ToString());
                //throw (ex);
            }
            return UpdateCont;
        }


    }
}
