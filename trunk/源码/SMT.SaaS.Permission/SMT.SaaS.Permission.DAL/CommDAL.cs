using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Core;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Configuration.Assemblies;
using System.Configuration;
using SMT.SaaS.BLLCommonServices;

namespace SMT.SaaS.Permission.DAL
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
            //return base.GetTable<TEntity>();
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

        #region 重写增删查改
        /// <summary>
        /// 增加实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns><0则出错</returns>
        public int Add(TEntity entity)
        {
            int result = 0;
            try
            {
                result = base.Add(entity);
                if (result > 0)
                {
                    base.SaveContextChanges();
                    MvcCacheClear(entity, "Add");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("PermissionBLL.Add" + ex.ToString());
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 增加实体(AddToContext)
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns><0则出错</returns>
        public int AddToContext(object obj)
        {
            int result = 1;
            try
            {
                base.AddToContext(obj);
                MvcCacheClear((TEntity)(obj), "Add");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("PermissionBLL.AddToContext" + ex.ToString());
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 修改实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns><0则出错</returns>
        public int Update(TEntity entity)
        {
            int result = 0;
            try
            {
                result = base.Update(entity);
                if (result > 0)
                {
                    base.SaveContextChanges();
                    MvcCacheClear(entity, "Modify");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("PermissionBLL.Modify" + ex.ToString());
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 修改实体(UpdateFromContext版本)
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns><0则出错</returns>
        public int UpdateFromContext(Object obj)
        {
            int result = 1;
            try
            {
                base.UpdateFromContext(obj);
                MvcCacheClear((TEntity)(obj), "Modify");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("PermissionBLL.UpdateFromContext" + ex.ToString());
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns><0则出错</returns>
        public int Delete(TEntity entity)
        {
            int result = 0;
            try
            {
                result = base.Delete(entity);
                if (result > 0)
                {
                    base.SaveContextChanges();
                    MvcCacheClear(entity, "Delete");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("PermissionBLL.Delete" + ex.ToString());
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns><0则出错</returns>
        public int DeleteFromContext(object obj)
        {
            int result = 1;
            try
            {
                base.DeleteFromContext(obj);
                MvcCacheClear((TEntity)obj, "Delete");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("PermissionBLL.Delete" + ex.ToString());
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 通知MVC缓存更新缓存的实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        public void MvcCacheClear(TEntity entity, string action)
        {
            string strModelCode = entity.GetType().Name;
            BLLCommonServices.MVCCacheSV.EntityAction act;
            switch (action)
            {
                case "Add": act = BLLCommonServices.MVCCacheSV.EntityAction.Add; break;
                case "Modify": act = BLLCommonServices.MVCCacheSV.EntityAction.Modify; break;
                case "Delete": act = BLLCommonServices.MVCCacheSV.EntityAction.Delete; break;
                default: act = BLLCommonServices.MVCCacheSV.EntityAction.None; break;
            }
            if (entity is System.Data.Objects.DataClasses.EntityObject)
            {
                System.Data.Objects.DataClasses.EntityObject ent = entity as System.Data.Objects.DataClasses.EntityObject;
                string strFormId = entity.GetType().GetProperties().FirstOrDefault().GetValue(entity, null).ToString();
                if (strFormId != "" || strFormId != null)
                {
                    BLLCommonServices.Utility.MvcCacheClearAsync(strModelCode, strFormId, act);
                }
            }
        }
        #endregion
    }
}
