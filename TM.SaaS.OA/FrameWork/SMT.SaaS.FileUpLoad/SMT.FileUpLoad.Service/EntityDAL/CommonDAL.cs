using System;
using System.Linq;
using System.Data.EntityClient;
using System.Data.Objects;

using SMT.Foundation.Core;

namespace SMT.SAAS.MP.DAL
{
    public class CommonDAL<TEntity> : BaseDAL
    {
        //public object CustomerQuery(string Sqlstring)
        //{
        //    using (EntityConnection econn = new EntityConnection(lbc.GetDataContext().MetadataWorkspace, lbc.GetDataContext().Connection))
        //    {
        //        econn.Open();
        //        EntityCommand ecmd = new EntityCommand(Sqlstring, econn);

        //        return ecmd.ExecuteScalar();
        //    }
        //}
        //public TM_SaaS_OA_EFModel.SMT_FILEUPLOAD_EFModelContext DataContext
        //{
        //    get
        //    {
        //        return GetDataContext() as TM_SaaS_OA_EFModel.SMT_FILEUPLOAD_EFModelContext;
        //    }
        //}

        

        public ObjectQuery<TEntity> GetObjects()
        {
            return base.GetObjects<TEntity>();
        }

        #region 事务处理

        //private ObjectContext edm = null;
        //private System.Data.Common.DbTransaction tran = null;

        //public void BeginTransaction()
        //{
        //    //edm = dal.GetDataContext();
        //    edm = this.GetDataContext();
        //    if (edm.Connection.State == System.Data.ConnectionState.Closed)
        //    {
        //        edm.Connection.Open();
        //    }
        //    tran = edm.Connection.BeginTransaction();
        //}

        //public void CommitTransaction()
        //{
        //    tran.Commit();
        //}

        //public void RollbackTransaction()
        //{
        //    tran.Rollback();
        //}

        #endregion
        #region 基础操作

        /// <summary>
        /// 新增单个<see cref="TTEntity"/>
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>是否新增成功.
        /// 0：失败
        /// 1：成功
        /// </returns>
        public int Add(TEntity entity)
        {
            try
            {
                return base.Add(entity);
            }
            catch (Exception ex)
            {
                return -1;
                throw (ex);
            }
        }
        /// <summary>
        /// 删除单个<see cref="TTEntity"/>
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>是否新增成功.
        /// 0：失败
        /// 1：成功
        /// </returns>
        public int Delete(TEntity entity)
        {
            try
            {
                return base.Delete(entity);
            }
            catch (Exception ex)
            {
                return -1;
                throw (ex);
            }
        }
        /// <summary>
        /// 更新单个<see cref="TTEntity"/>
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>是否新增成功.
        /// 0：失败
        /// 1：成功
        /// </returns>
        public int Update(TEntity entity)
        {
            try
            {
                return base.Update(entity);
            }
            catch (Exception ex)
            {
                return -1;
                throw (ex);
            }
        }
        /// <summary>
        /// 查询所有<see cref="TTEntity"/>
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns>是否新增成功.
        /// 0：失败
        /// 1：成功
        /// </returns>
        public IQueryable<TEntity> GetTable()
        {
            return base.GetTable<TEntity>();
        } 
        #endregion
    }
}
