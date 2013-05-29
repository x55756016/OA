using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

using SMT_FB_EFModel;
using System.Reflection;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Data;
using System.Linq.Dynamic;
using SMT.SaaS.BLLCommonServices;
using System.Configuration;
using SMT.Foundation.Core;

namespace SMT.FB.DAL
{
    public class BaseDAL : IDAL
    {
        private SMT_FB_EFModelContext _lbc = null;
        public SMT_FB_EFModelContext lbc
        {
            get
            {
                if (_lbc == null)
                {
                    _lbc =new SMT_FB_EFModelContext();
                }
                return _lbc;
                // return DBContext.GetObjectContext(this);
            }
        }
        private static string conn = ConfigurationManager.AppSettings["ConnectionString"].ToString();


        public BaseDAL()
        {
        }

        public BaseDAL(bool isSolo)
        {
            _lbc = new SMT_FB_EFModelContext();
        }

        private ObjectQuery<TEntity> GetQueryTable<TEntity>(QueryExpression queryExpression)
        {
            Func<ObjectQuery<TEntity>> GetInnerTables = () =>
            {
                if (queryExpression.InnerQueryExpression != null)
                {
                    return QueryTable<TEntity>(queryExpression.InnerQueryExpression) as ObjectQuery<TEntity>;
                }
                return null;
            };

            Func<ObjectQuery<TEntity>> GetTables = () =>
                {
                    ObjectQuery<TEntity> qTables = (this as IDAL).GetTable<TEntity>() as ObjectQuery<TEntity>;
                    qTables = qTables.SetOrganizationFilter(queryExpression);
                    return qTables;
                };

            if (queryExpression.InnerDataType == QueryExpression.InnerDataTypes.All)
            {

                return GetInnerTables();
            }
            else if (queryExpression.InnerDataType == QueryExpression.InnerDataTypes.Attached)
            {
                var innerData = GetInnerTables();
                if (innerData != null)
                {
                    return GetTables().Union(innerData);
                }
                else
                {
                    return GetTables();
                }

            }
            else if (queryExpression.InnerDataType == QueryExpression.InnerDataTypes.None)
            {
                return GetTables();
            }
            else
            {
                return null;
            }

        }

        public IQueryable<TEntity> QueryTable<TEntity>(QueryExpression queryExpression)
        {
            try
            {
                IQueryable<TEntity> result = null;

                var qTables = GetQueryTable<TEntity>(queryExpression);
                if (queryExpression.Include != null)
                {
                    for (int i = 0; i < queryExpression.Include.Length; i++)
                    {
                        qTables = qTables.Include(queryExpression.Include[i]);
                    }
                }
                Type EntityType = typeof(TEntity);

                Expression pred = QueryExpressionHelper.CovertToExpression(EntityType, queryExpression);
                if (pred == null)
                {
                    result = qTables;
                }
                else
                {
                    Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { EntityType }, Expression.Constant(qTables), pred);
                    //Expression.Call(typeof(Queryable), "OrderBy",null,
                    result = qTables.AsQueryable().Provider.CreateQuery<TEntity>(expr);

                }

                if (queryExpression.IsNoTracking)
                {
                    (result as ObjectQuery<TEntity>).MergeOption = MergeOption.NoTracking;
                }


                if (queryExpression.OrderByExpression != null)
                {
                    result = OrderByData(result, queryExpression.OrderByExpression);
                }
                if (queryExpression.Pager != null)
                {
                    result = FilterPage(result, queryExpression.Pager);
                }

                return result;

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public IQueryable<TEntity> OrderByData<TEntity>(IQueryable<TEntity> queryData, OrderByExpression orderByExpression)
        {

            string otype = orderByExpression.OrderByType == OrderByType.Asc ? "" : "desc";
            string orderStr = orderByExpression.PropertyName + " " + otype;
            return DynamicQueryable.OrderBy(queryData, orderStr);

            //IOrderedQueryable<TEntity> result = null;
            //bool bFirst = true;
            //var item = orderByExpression;
            //while (item != null)
            //{

            //    Func<TEntity, DateTime> selectorDT = entity =>
            //    {
            //        Type type = typeof(TEntity);
            //        return (DateTime)type.GetProperty(item.PropertyName).GetValue(entity, null);
            //    };

            //    Func<TEntity, string> selectorSTR = entity =>
            //    {
            //        Type type = typeof(TEntity);
            //        return Convert.ToString( type.GetProperty(item.PropertyName).GetValue(entity, null));
            //    };

            //    Func<TEntity, decimal> selectorDe = entity =>
            //    {
            //        Type type = typeof(TEntity);
            //        return Convert.ToDecimal(type.GetProperty(item.PropertyName).GetValue(entity, null));
            //    };

            //    dynamic selector = selectorSTR;
            //    if (item.PropertyType == typeof(DateTime).ToString())
            //    {
            //        selector = selectorDT;
            //    }
            //    else if (item.PropertyType == typeof(decimal).ToString())
            //    {
            //        selector = selectorDe;
            //    }

            //    if (bFirst)
            //    {
            //        if (item.OrderByType == OrderByType.Asc)
            //        {
            //            result = queryData.OrderBy(entity => selector(entity));
            //        }
            //        else
            //        {
            //            result = queryData.OrderByDescending(entity => selector(entity));
            //        }
            //        bFirst = false;
            //    }
            //    else
            //    {
            //        if (item.OrderByType == OrderByType.Asc)
            //        {
            //            result = result.ThenBy(entity => selector(entity));
            //        }
            //        else
            //        {
            //            result = result.ThenByDescending(entity => selector(entity));
            //        }
            //    }
            //    item = item.ThenOrderByExpression;
            //};

            //return result.AsQueryable();
        }

        public IQueryable<TEntity> FilterPage<TEntity>(IQueryable<TEntity> queryData, PageExpression pager)
        {
            int skipNum = (pager.PageIndex - 1) * pager.PageSize;
            int takeNum = pager.PreRowCount;
            int rowCount = queryData.Count();
            int pageCount = rowCount / pager.PageSize;

            if (rowCount % pager.PageSize > 0)
            {
                pageCount++;
            }
            pager.PageCount = pageCount;

            if (pageCount < pager.PageIndex)
            {
                pager.PageIndex = pageCount == 0 ? 1 : pageCount;
            }

            return queryData.ToList().AsQueryable().Skip(skipNum).Take(takeNum);
        }

        IQueryable<TEntity> IDAL.GetTable<TEntity>()
        {
            return lbc.CreateQuery<TEntity>("[" + typeof(TEntity).Name + "]");
        }


        public int Add(EntityObject entity)
        {
            try
            {
                if (entity.EntityKey == null)
                {
                    lbc.AddObject(entity.GetType().Name, entity);
                    return lbc.SaveChanges();
                }
                else
                {
                    // object existValue = null;
                    // lbc.TryGetObjectByKey(entity.EntityKey, out existValue);

                    if (entity.EntityKey.EntityKeyValues == null)
                    {
                        lbc.AddObject(entity.GetType().Name, entity);
                        return lbc.SaveChanges();
                    }

                    //  --2011-5-5 17：27屏蔽，解决出差报销提交产生费用报销单时的错误
                    //throw new Exception("entity exists in db");

                    return Update(entity);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("数据添加异常：" + ex.ToString());
                throw ex;
            }
        }

        public int Update(EntityObject entity)
        {
            try
            {
                if (entity.EntityKey != null)
                {
                    object existValue = null;
                    lbc.TryGetObjectByKey(entity.EntityKey, out existValue);

                    if (existValue != null)
                    {
                        this.GetEntityHalf(existValue as EntityObject);
                        lbc.ApplyCurrentValues(entity.GetType().Name, entity);
                        return lbc.SaveChanges();
                    }
                }
                throw new Exception("entity do not exist in db");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("数据更新异常：" + ex.ToString());
                throw ex;
            }
        }

        public int Delete(EntityObject entity)
        {
            try
            {
                if (entity.EntityKey != null)
                {
                    object existValue = null;
                    lbc.TryGetObjectByKey(entity.EntityKey, out existValue);
                    if (existValue != null)
                    {
                        lbc.DeleteObject(existValue);
                        return lbc.SaveChanges();
                    }
                }
                throw new Exception("entity do not exist in db");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("数据删除异常：" + ex.ToString());
                throw ex;
            }
        }

        public void Detach(EntityObject entity)
        {
            this.lbc.Detach(entity);
        }

        private System.Data.Common.DbTransaction tran = null;

        public void BeginTransaction()
        {
            tran = DBContext.BeginTransaction(this.lbc);
        }

        public void CommitTransaction()
        {
            if (tran != null)
            {
                DBContext.CommitTransaction(tran);
            }
        }

        public void RollbackTransaction()
        {
            if (tran != null)
            {
                DBContext.RollbackTransaction(tran);
            }
        }


        #region IDisposable 成员

        public void Dispose()
        {
            try
            {
                SMT.Foundation.Log.Tracer.Debug("FB BaseDAL.Dispose sucess");
                lbc.Dispose();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("FB BaseDAL.Dispose Err"+ex.ToString());
            }
        }

        #endregion


        #region IDAL 成员


        public EntityObject GetEntity(EntityKey key)
        {
            return this.lbc.GetObjectByKey(key) as EntityObject;
        }

        #endregion

        public EntityObject GetEntityHalf(EntityObject entity)
        {
            var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();
            foreach (IRelatedEnd re in rs)
            {
                if (re.GetType().BaseType != typeof(EntityReference))
                {
                    re.Load();
                }

            }
            return entity;
        }


        public void Attach(EntityObject entity)
        {
            try
            {
                if (entity.EntityState != EntityState.Detached)
                {
                    return;
                }
                if (entity.EntityKey == null)
                {
                    lbc.AddObject(entity.GetType().Name, entity);
                }
                else
                {
                    // object existValue = null;
                    // lbc.TryGetObjectByKey(entity.EntityKey, out existValue);

                    if (entity.EntityKey.EntityKeyValues == null)
                    {
                        lbc.AddObject(entity.GetType().Name, entity);
                    }
                    else
                    {
                        object existValue = null;
                        lbc.TryGetObjectByKey(entity.EntityKey, out existValue);

                        if (existValue != null)
                        {
                            this.GetEntityHalf(existValue as EntityObject);
                            lbc.ApplyCurrentValues(entity.GetType().Name, entity);
                        }
                    }

                    //  --2011-5-5 17：27屏蔽，解决出差报销提交产生费用报销单时的错误
                    //throw new Exception("entity exists in db");

                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("数据Attach异常：" + ex.ToString());
                throw ex;
            }
        }

        public void DeleteObject(EntityObject entity)
        {
            try
            {
                if (entity.EntityKey != null)
                {
                    object existValue = null;
                    if (entity.EntityState == EntityState.Detached)
                    {
                        lbc.TryGetObjectByKey(entity.EntityKey, out existValue);
                    }
                    else
                    {
                        existValue = entity;
                    }
                    
                    
                    if (existValue != null)
                    {
                        lbc.DeleteObject(existValue);
                    }
                }
                
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("实体删除异常：" + ex.ToString());
                throw ex;
            }
        }
        public int SaveChanges()
        {
            return this.lbc.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
        }


        public object ExecuteStoredProcedure(string Sqlstring, ParameterCollection prameters)
        {
            using (OracleDAO dao = new OracleDAO(conn))
            {
                return dao.ExecuteScalar("", System.Data.CommandType.StoredProcedure, prameters);
            }
        }

        public object ExecuteCustomerSql(string Sqlstring, ParameterCollection prameters)
        {
            using (OracleDAO dao = new OracleDAO(conn))
            {
                return dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text, prameters);
            }
        }

        public object ExecuteCustomerSql(string Sqlstring)
        {
            using (OracleDAO dao = new OracleDAO(conn))
            {
                object obj = dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);
                return obj;
            }
        }
    }


}
