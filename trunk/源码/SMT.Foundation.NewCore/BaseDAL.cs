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
using System.Reflection;
using SMT.Foundation.Log;

namespace SMT.Foundation.Core
{

    public class BaseDAL :IDisposable 
    {
        private IDAL lbc;
        public string DalID;
        public string CurrentUserID { get; set; }
        private string entityName;

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
            EntityObject NewEntity = (EntityObject)obj;
            object OldEntity = null;
            string keyName = string.Empty;
            string keyValue = string.Empty;
            if (NewEntity.EntityKey != null)
            {
                if (!this.IsGetObjectByKey(NewEntity.EntityKey, ref OldEntity))
                {
                    this.GetObjectInOtherway(NewEntity.EntityKey, ref OldEntity);
                }
            }
            else
            {
                foreach (PropertyInfo info in NewEntity.GetType().GetProperties())
                {
                    EdmScalarPropertyAttribute attribute = info.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).FirstOrDefault<object>() as EdmScalarPropertyAttribute;
                    if ((attribute != null) && attribute.EntityKeyProperty)
                    {
                        keyName = info.Name;
                        keyValue = info.GetValue(NewEntity, null).ToString();
                        break;
                    }
                }
                string name = obj.GetType().Name;
                EntityKey key = new EntityKey(DALFacoty.DBContextName + "." + name, keyName, keyValue);
                this.lbc.GetDataContext().TryGetObjectByKey(key, out OldEntity);
            }
            //将修改的普通属性值（不包括外键）设置到当前上下文实体上
            this.lbc.GetDataContext().ApplyPropertyChanges(NewEntity.GetType().Name, NewEntity);
            //将修改的外键值设置到当前上下文实体上
            foreach (IRelatedEnd newEntityRefrence in ((IEntityWithRelationships)NewEntity).RelationshipManager.GetAllRelatedEnds())
            {
                //if (!newEntityRefrence.IsLoaded) newEntityRefrence.Load();load后值会被修改为数据库里的原值
                //EntityReference r = newEntityRefrence.EntityKey;
                //foreach (IEntityWithKey entityR in newEntityRefrence)
                //{   
                EntityReference ef = null;
                try
                {
                    //EntityReference，EntityReferenceConllection继承IRelatedEnd
                    //如果是外键，为EntityReference，如果是主表则为EntityReferenceConllection
                    ef = newEntityRefrence as EntityReference;
                }
                catch (Exception ex)
                {
                }
                if (ef != null)
                {
                    object objRefNew = null;
                    this.lbc.GetDataContext().TryGetObjectByKey(ef.EntityKey, out objRefNew);
                    if (NewEntity.GetType().Name == newEntityRefrence.TargetRoleName)
                    {
                        (OldEntity as EntityObject).GetType().GetProperty(newEntityRefrence.TargetRoleName + "2").SetValue(OldEntity, objRefNew, null);
                    }
                    else
                    {
                        if ((OldEntity as EntityObject).GetType().GetProperty(newEntityRefrence.TargetRoleName).PropertyType.Name == "EntityCollection`1")
                        {
                            continue;
                        }
                        (OldEntity as EntityObject).GetType().GetProperty(newEntityRefrence.TargetRoleName).SetValue(OldEntity, objRefNew, null);
                    }
                }
            }
        }

        private bool IsGetObjectByKey(EntityKey entityKey, ref object existValue)
        {
            try
            {
                return this.lbc.GetDataContext().TryGetObjectByKey(entityKey, out existValue);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void GetObjectInOtherway(EntityKey entityKey, ref object existValue)
        {
            try
            {
                existValue = this.lbc.GetObjectByEntityKey(entityKey);
            }
            catch (Exception exception)
            {
                Tracer.Debug(exception.InnerException.Message);
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
        public void LogNewDal(string dalName)
        {
            this.entityName = dalName;
        }

        #region IDisposable 成员

        public void Dispose()
        {
            //throw new NotImplementedException();
            //DALFacoty.ClearCache(this.currentThreadName);
            //DALFacoty.contenxtCache.Remove(this.currentThreadName);
            ((ObjectContext)this.lbc).Dispose();
        }

        #endregion

        public static void RefreshEntity(object entObj)
        {
            EntityObject obj2 = (EntityObject)entObj;
            IEnumerable<IRelatedEnd> allRelatedEnds = ((IEntityWithRelationships)obj2).RelationshipManager.GetAllRelatedEnds();
            using (IEnumerator<IRelatedEnd> enumerator = allRelatedEnds.GetEnumerator())
            {
                Action<EntityObject> action = null;
                IRelatedEnd re;
                while (enumerator.MoveNext())
                {
                    re = enumerator.Current;
                    List<EntityObject> list = new List<EntityObject>();
                    foreach (object obj3 in re)
                    {
                        list.Add(obj3 as EntityObject);
                    }
                    if (action == null)
                    {
                        action = delegate(EntityObject p)
                        {
                            if (re.GetType().BaseType == typeof(EntityReference))
                            {
                                EntityKey entityKey = p.EntityKey;
                                if (entityKey != null)
                                {
                                    (re as EntityReference).EntityKey = entityKey;
                                    re.Remove(p);
                                }
                            }
                        };
                    }
                    list.ForEach(action);
                }
            }
        }
    }
}
