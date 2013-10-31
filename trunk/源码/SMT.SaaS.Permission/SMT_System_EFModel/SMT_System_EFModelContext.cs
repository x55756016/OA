using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using SMT.Foundation.Core;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data;
using SMT.Foundation.Log;

namespace SMT_System_EFModel
{
    public partial class SMT_System_EFModelContext:  IDAL
    {
        #region IDAL 增删改成员
        public SMT_System_EFModelContext lbc
        {
            get
            {
                return this;
            }
        }

        int IDAL.Add(object obj)
        {
            try
            {                
                lbc.AddObject(obj.GetType().Name, obj);
                return lbc.SaveChanges();
            }
            catch (System.Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " Add Err:" + ex.InnerException.Message);
                lbc.DeleteObject(obj);
                return 0;
            }
        }

        int IDAL.Update(object obj)
        {
            int i = 0;
            //lbc.AttachTo(typeof(TEntity).Name, obj);
            //EntityObject Entobj = (EntityObject)obj;
            try
            {
              i= lbc.SaveChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " Update Err:" + ex.InnerException.Message);
                lbc.Refresh(RefreshMode.StoreWins, obj);
                throw ex;                
            }
            return i;
        }

        int IDAL.Delete(object obj)
        {
            EntityObject entity = (EntityObject)obj;
            int i=0;
            try
            {
                lbc.TryGetObjectByKey(entity.EntityKey, out obj);
                lbc.DeleteObject(obj);
                i = lbc.SaveChanges();
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " Delete Err:" + ex.InnerException.Message);
                lbc.Refresh(RefreshMode.StoreWins, obj);
                throw ex;     
            }
            return i;
        }

        IQueryable<TEntity> IDAL.GetTable<TEntity>()
        {
            ObjectQuery<TEntity> objs = lbc.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]");
            objs.MergeOption = MergeOption.NoTracking;
            return objs;
        }

        ObjectQuery<TEntity> IDAL.GetObjects<TEntity>()
        {
            ObjectQuery<TEntity> objs = lbc.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]");
            objs.MergeOption = MergeOption.NoTracking;
            return objs;
        }

        public object GetObjectByEntityKey(EntityKey entityKey)
        {
            object obj = null;
            try
            {
                lbc.TryGetObjectByKey(entityKey, out obj);
                return obj;
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToShortTimeString() + " GetObjectByEntityKey Err:" + ex.Message);
                
                throw;
            }
        }

        int IDAL.SaveChanges()
        {
            return this.SaveChanges();
        }

        ObjectContext IDAL.GetDataContext()
        {
            return lbc;
        }

        #endregion

        #region 事务处理IDAL 成员

        private System.Data.Common.DbTransaction tran = null;

        /// <summary>
        /// 开始事务处理
        /// </summary>
        void IDAL.BeginTransaction()
        {
            ObjectContext edm = this;
            if (edm.Connection.State == System.Data.ConnectionState.Closed)
            {
                edm.Connection.Open();
            }
            tran = edm.Connection.BeginTransaction();
        }
        /// <summary>
        /// 确认事务处理
        /// </summary>
        void IDAL.CommitTransaction()
        {
            tran.Commit();
        }
        /// <summary>
        /// 回滚事务处理
        /// </summary>
        void IDAL.RollbackTransaction()
        {
            tran.Rollback();
        }
        #endregion
    }
}
