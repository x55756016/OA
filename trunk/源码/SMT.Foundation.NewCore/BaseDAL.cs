/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： 数据访问基类
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using System.Configuration;

using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Threading;
using System.Data;
using System.Data.EntityClient;

namespace SMT.Foundation.Core
{

    public class BaseDAL :IDisposable 
    {
        private IDAL lbc;
        public string CurrentUserID { get; set; }

        public BaseDAL()
        {
            //单例模式
            lbc = DALFacoty.CreateDataContext();
        }

        /// <summary>
        /// 添加对象到数据库中
        /// </summary>
        /// <param name="obj">需要处理的对象</param>
        /// <returns></returns>       
        public int Add(object obj)
        {
            return lbc.Add(obj);
        }
        /// <summary>
        /// 更新对象至数据库中
        /// </summary>
        /// <param name="obj">需要处理的对象</param>
        /// <returns></returns>
        public int Update(object obj)
        {
            UpdateFromContext(obj);
            return lbc.Update(obj);
        }
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="obj">需要处理的对象</param>
        /// <returns></returns>
        public int Delete(object obj)
        {
            RefreshEntity(obj);
            return lbc.Delete(obj);
        }

        public IQueryable<TEntity> GetTable<TEntity>()
        {
            return lbc.GetObjects<TEntity>();
        }

        public ObjectQuery<TEntity> GetObjects<TEntity>()
        {
            return lbc.GetObjects<TEntity>();
        }

        public object GetObjectByID<TEntity>(string ObjectKeyName, string ObjectKeyValue)
        {
            string EntityName = typeof(TEntity).Name;
            EntityName = DALFacoty.DBContextName + EntityName;
            System.Data.EntityKey entityKey = new System.Data.EntityKey(EntityName, ObjectKeyName, ObjectKeyValue);
            object entity = lbc.GetObjectByEntityKey(entityKey);
            return entity;
        }

        public object GetObjectByEntityKey(EntityKey entityKey)
        {
            object entity = lbc.GetObjectByEntityKey(entityKey);
            RefreshEntity(entity);
            return entity;
        }

        /// <summary>
        /// 实体上下文中添加对象（未保存到数据库中，请调用SaveContextChanges方法）
        /// </summary>
        /// <param name="obj"></param>
        public void AddToContext(object obj)
        {
            lbc.GetDataContext().AddObject(obj.GetType().Name, obj);
        }
        /// <summary>
        /// 更新实体上下文中的对象（未保存到数据库中,请调用SaveContextChanges方法）
        /// </summary>
        /// <param name="obj"></param>
        public void UpdateFromContext(object obj)
        {
            EntityObject Entobj = (EntityObject)obj;
            object existValue = null;

            string strEntityKeyName = string.Empty;
            string strEntityKeyvalue = string.Empty;
            if (Entobj.EntityKey == null)
            {
                foreach (var prop in obj.GetType().GetProperties())
                {
                    var attr = prop.GetCustomAttributes(typeof(System.Data.Objects.DataClasses.EdmScalarPropertyAttribute), false).FirstOrDefault()
                        as System.Data.Objects.DataClasses.EdmScalarPropertyAttribute;
                    if (attr != null && attr.EntityKeyProperty)
                    {
                        strEntityKeyName = prop.Name;
                        strEntityKeyvalue = prop.GetValue(obj, null).ToString();
                        break;
                    }
                }
                string EntityName = obj.GetType().Name;
                EntityName = DALFacoty.DBContextName + EntityName;
                System.Data.EntityKey entityKey = new System.Data.EntityKey(EntityName, strEntityKeyName, strEntityKeyvalue);
                lbc.GetDataContext().TryGetObjectByKey(entityKey, out existValue);
            }
            else
            {
                lbc.GetDataContext().TryGetObjectByKey(Entobj.EntityKey, out existValue);
            }
            lbc.GetDataContext().ApplyPropertyChanges(Entobj.GetType().Name, obj);
            //外键赋值
            foreach (IRelatedEnd relatedEnd in ((IEntityWithRelationships)Entobj).RelationshipManager.GetAllRelatedEnds())
            {
                foreach (IEntityWithKey relatedItem in relatedEnd)
                {
                    object reference = null;
                    lbc.GetDataContext().TryGetObjectByKey(relatedItem.EntityKey, out reference);
                    if (Entobj.GetType().Name == relatedEnd.TargetRoleName)//自引用的实体
                    {
                        (existValue as EntityObject).GetType().GetProperty(relatedEnd.TargetRoleName + "2").SetValue(existValue, reference, null);
                    }
                    else
                    {
                        //如果是主表，跳出
                        if ((existValue as EntityObject).GetType().GetProperty(relatedEnd.TargetRoleName).PropertyType.Name == "EntityCollection`1")
                        {
                            continue;
                        }
                        else
                        {
                            //更新实体的外键
                            (existValue as EntityObject).GetType().GetProperty(relatedEnd.TargetRoleName).SetValue(existValue, reference, null);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 从实体上下文中删除对象（未保存到数据库,请调用SaveContextChanges方法）
        /// </summary>
        /// <param name="obj"></param>
        public void DeleteFromContext(object obj)
        {
            //RefreshEntity(obj);
            EntityObject Entobj = (EntityObject)obj;
            object existValue = null;
            lbc.GetDataContext().TryGetObjectByKey(Entobj.EntityKey, out existValue);
            lbc.GetDataContext().DeleteObject(existValue);
        }
        /// <summary>
        /// 将实体上下文中的对象变动保存至数据库中
        /// </summary>
        /// <returns>保存变动的对象数量</returns>
        public int SaveContextChanges()
        {
            int i = 0;
            i = lbc.GetDataContext().SaveChanges();
            return i;
        }

        public object CustomerQuery(string Sqlstring)
        {
            //string con = "name = EntityFrameworkOracleContext";
            using (EntityConnection econn = new EntityConnection(lbc.GetDataContext().MetadataWorkspace, lbc.GetDataContext().Connection))
            {
                econn.Open();
                EntityCommand ecmd = new EntityCommand(Sqlstring, econn);

                return ecmd.ExecuteScalar();
            }
        }

        private ObjectContext GetDataContext()
        {
            return lbc.GetDataContext();
        }

        /// <summary>
        /// 开始事务处理
        /// </summary>
        public void BeginTransaction()
        {
            if (lbc.GetDataContext().Connection.State == ConnectionState.Closed)
            {
                lbc.GetDataContext().Connection.Open();
            }
            lbc.BeginTransaction();
        }
        /// <summary>
        /// 确认事务处理
        /// </summary>
        public void CommitTransaction()
        {
            lbc.CommitTransaction();
        }
        /// <summary>
        /// 回滚事务处理
        /// </summary>
        public void RollbackTransaction()
        {
            lbc.RollbackTransaction();
        }

        ~BaseDAL()
        {
        }


        #region IDisposable 成员

        public void Dispose()
        {
            //throw new NotImplementedException();
            //DALFacoty.ClearCache(this.currentThreadName);
            //DALFacoty.contenxtCache.Remove(this.currentThreadName);
            lbc.GetDataContext().Dispose();
            this.Dispose();
        }

        #endregion

        public static void RefreshEntity(Object entObj)
        {
            EntityObject entity = (EntityObject)entObj;
            var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();

            foreach (IRelatedEnd re in rs)
            {
                List<EntityObject> list = new List<EntityObject>();
                foreach (var item in re)
                {
                    list.Add(item as EntityObject);
                }
                list.ForEach(p =>
                {
                    if (re.GetType().BaseType == typeof(EntityReference))
                    {
                        EntityKey eKey = p.EntityKey;
                        if (eKey != null)
                        {
                            (re as EntityReference).EntityKey = eKey;
                            re.Remove(p);
                        }
                    }

                });
            }
        }
    }
}
