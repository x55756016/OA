using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.FB.DAL;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using SMT_FB_EFModel;
using System.Data;
using System.Reflection;
using System.Collections;

namespace SMT.FB.BLL
{

    public class BaseBLL : IDisposable
    {
        private IDAL _baseDal;
        public IDAL baseDal
        {
            get
            {
                if (_baseDal == null)
                {
                    _baseDal = new BaseDAL();
                }
                return _baseDal;
            }
        }

        public BaseBLL()
        {

        }

        internal BaseBLL(bool isSolo)
        {
            _baseDal = new BaseDAL(isSolo);
        }

        ~BaseBLL()
        {
            this.baseDal.Dispose();
        }

        #region 1.	查询实体对象


        public EntityObject GetEntity(EntityKey key)
        {
            return baseDal.GetEntity(key);
        }

        public EntityObject GetEntity(QueryExpression queryExpression)
        {
            List<EntityObject> list = BaseGetEntities(queryExpression);
            return list.FirstOrDefault();
        }

        public T GetEntity<T>(QueryExpression queryExpression)
        {
            return InnerGetEntities<T>(queryExpression).FirstOrDefault();
        }

        public List<T> GetEntities<T>(QueryExpression queryExpression)
        {
            if (queryExpression == null)
            {
                return new List<T>();
            }

            return InnerGetEntities<T>(queryExpression).ToList();
        }



        public IQueryable<T> InnerGetEntities<T>(QueryExpression queryExpression)
        {
            if (queryExpression.QueryType != typeof(T).Name)
            {
                queryExpression.QueryType = typeof(T).Name;
            }
            return baseDal.QueryTable<T>(queryExpression);
        }

        public List<EntityObject> BaseGetEntities(QueryExpression queryExpression)
        {

            try
            {
                if (queryExpression.Pager != null && queryExpression.OrderByExpression == null)
                {
                    queryExpression.OrderByExpression = new OrderByExpression
                    {
                        OrderByType = OrderByType.Dsc,
                        PropertyType = typeof(System.DateTime).ToString(),
                        PropertyName = FieldName.UpdateDate
                    };
                }
                Type gType = Type.GetType("SMT_FB_EFModel." + queryExpression.QueryType + ",SMT_FB_EFModel");
                MethodInfo myMethod = baseDal.GetType().GetMethods().First(m => m.Name.Equals("QueryTable") && m.IsGenericMethod);

                object result = myMethod.MakeGenericMethod(gType).Invoke(baseDal, new object[] { queryExpression });

                IEnumerable listResult = result as IEnumerable;
                List<EntityObject> list = new List<EntityObject>();

                foreach (var entity in listResult)
                {
                    // GetEntityHalf(entity as EntityObject);
                    list.Add(entity as EntityObject);
                }

                return list;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }

        }


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

        public IQueryable<TEntity> GetTable<TEntity>()
        {
            IQueryable<TEntity> result = (baseDal as IDAL).GetTable<TEntity>();
            return result;
        }
        #endregion

        #region 2.	实体对象的增，删，修操作

        /// <summary>
        /// 逻辑删除，修改EDITSTATES 为 0
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Delete(EntityObject entity)
        {
            try
            {
                EntityObject odel = (EntityObject)baseDal.GetEntity(entity.EntityKey);
                odel.SetValue("EDITSTATES", decimal.Parse("0"));
                int i = baseDal.Update(odel);
                return i > 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public bool Add(EntityObject entity)
        {
            try
            {
                Type type = entity.GetType();

                DateTime dtNow = DateTime.Now;
                entity.SetValue(FieldName.CreateDate, dtNow);
                entity.SetValue(FieldName.UpdateDate, dtNow);

                //SystemBLL.AddAutoOrderCode(entity); // 自动生成编号
                int i = baseDal.Add(entity);
                SaveMyRecord(entity);
                return i > 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public bool Update(EntityObject entity)
        {
            try
            {

                Type type = entity.GetType();
                entity.SetValue(FieldName.UpdateDate, DateTime.Now);
                int i = baseDal.Update(entity);
                SaveMyRecord(entity);
                return i > 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void UpdateState(EntityObject entity)
        {
            try
            {
                int i = baseDal.Update(entity);
                SaveMyRecord(entity);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public bool Remove(EntityObject entity)
        {

            entity = this.GetEntity(entity.EntityKey);
            List<EntityObject> list = GetAllSubEntities(entity);
            list.ForEach(item =>
            {
                InnerRemove(item);
            });
            return true;

            #region 暂时不用
            //object checkStates = entity.GetValue("CHECKSTATES");

            //// 如果是草稿单，级联删除所以被参照表记录
            //if (checkStates != null && Convert.ToInt32(checkStates) == 0)
            //{
            //    entity = this.GetEntity(entity.EntityKey);
            //    List<EntityObject> list = new List<EntityObject>();
            //    var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();
            //    foreach (IRelatedEnd re in rs)
            //    {
            //        if (re.GetType().BaseType != typeof(EntityReference))
            //        {
            //            re.Load();
            //            foreach (var item in re)
            //            {
            //                list.Add(item as EntityObject);
            //            }
            //        }
            //    }
            //    list.Add(entity);
            //    list.ForEach(item =>
            //    {
            //        InnerRemove(item);
            //    });
            //    return true;
            //}
            //else if (checkStates != null && (int)checkStates != 0)
            //{
            //    return this.Delete(entity);
            //}
            //else
            //{
            //    return InnerRemove(entity);
            //}
            #endregion
        }

        /// <summary>
        /// 从 ObjectContenct中去掉实体,就是把实体的EntityState 改为Datched
        /// </summary>
        /// <param name="entity"></param>
        public void Detach(EntityObject entity)
        {
            baseDal.Detach(entity);
        }

        private List<EntityObject> GetAllSubEntities(EntityObject entity)
        {
            entity = this.GetEntity(entity.EntityKey);
            List<EntityObject> list = new List<EntityObject>();
            var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();
            foreach (IRelatedEnd re in rs)
            {
                if (re.GetType().BaseType != typeof(EntityReference))
                {
                    re.Load();
                    foreach (var item in re)
                    {
                        List<EntityObject> listSub = GetAllSubEntities(item as EntityObject);
                        list.AddRange(listSub);
                    }
                }
            }
            list.Add(entity);
            return list;
        }

        public bool InnerRemove(EntityObject entity)
        {
            try
            {
                int i = baseDal.Delete(entity);
                DeleteMyRecord(entity);
                return i > 0;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void BassBllSave(EntityObject entity, FBEntityState entityState)
        {
            try
            {
                if (typeof(VirtualEntityObject).IsAssignableFrom(entity.GetType()))
                {
                    return;
                }

                // 可能是某个entity的子集,已被添加.
                if (entity.EntityState == EntityState.Unchanged)
                {
                    return;
                }
                string type = entity.GetType().Name;
                switch (entityState)
                {
                    case FBEntityState.Added:
                        Add(entity);
                        break;
                    case FBEntityState.Modified:
                        Update(entity);
                        break;
                    case FBEntityState.Deleted:
                        Delete(entity);
                        break;
                    case FBEntityState.Detached:
                        Remove(entity);
                        break;
                }
                
                //SystemBLL.Debug(() =>
                //    {
                //        return string.Format("\r\n\r\n操作类型: {0} \r\n操作数据:\r\n{1}\r\n\r\n", entityState.ToString(), entity.ToXml());
                //    }
                //    );
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("BaseBLL.Save,异常错误: " + ex.Message);
                sb.AppendLine("单据所有人: " + Convert.ToString(entity.GetValue("OWNERNAME")));
                sb.AppendLine("单据所有人岗位: " + Convert.ToString(entity.GetValue("OWNERPOSTNAME")));
                sb.AppendLine("单据所有人部门: " + Convert.ToString(entity.GetValue("OWNERDEPARTMENTNAME")));
                sb.AppendLine("单据所有人公司: " + Convert.ToString(entity.GetValue("OWNERCOMPANYNAME")));
                sb.AppendLine("单据类型: " + entity.GetType().Name);
                sb.AppendLine("操作类型: " + entityState.ToString());

                throw new Exception(sb.ToString(), ex);
            }
                
        }

        

        public void RefreshData(EntityObject entity)
        {
            var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();
            if (rs.Count() == 0) return;

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
                        if ((eKey != null) && (p.EntityState == EntityState.Detached) && eKey.EntityKeyValues != null)
                        {
                            (re as EntityReference).EntityKey = eKey;
                            re.Remove(p);
                        }
                    }

                });
            }
        }
        #endregion

        #region 3.	业务的事务方法
        public void BeginTransaction()
        {
            baseDal.BeginTransaction();
        }
        public void CommitTransaction()
        {
            baseDal.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            baseDal.RollbackTransaction();
        }

        #endregion

        #region 4.	将须审核的单据记录索引在"我的单据"中存储或者删除
        /// <summary>
        /// 将指定的单据记录存储到我的单据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string SaveMyRecord(EntityObject entity)
        {
            string strTemp = string.Empty;
            try
            {
                if (entityTypeList.Count() == 0)
                {
                    InitEntityTypeList();
                }

                if (!entityTypeList.Contains(entity.GetType().Name))
                {
                    return strTemp;
                }

                if (entity.GetType().Name == typeof(T_FB_CHARGEAPPLYMASTER).Name)
                {
                    T_FB_CHARGEAPPLYMASTER entTemp = entity as T_FB_CHARGEAPPLYMASTER;
                    if (entTemp.CHARGEAPPLYMASTERCODE.ToUpper().StartsWith("CLBX"))
                    {
                        return strTemp; 
                    }
                }

                SMT.SaaS.BLLCommonServices.Utility.SubmitMyRecord<EntityObject>(entity);
            }
            catch (Exception ex)
            {
                strTemp = ex.ToString();
            }
            return strTemp;
        }

        /// <summary>
        /// 将指定的单据记录从我的单据中删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string DeleteMyRecord(EntityObject entity)
        {
            string strTemp = string.Empty;
            try
            {
                if (entityTypeList.Count() == 0)
                {
                    InitEntityTypeList();
                }

                if (!entityTypeList.Contains(entity.GetType().Name))
                {
                    return strTemp;
                }

                string orderid = entity.GetOrderID();
                var eInfo = entity.GetEntityInfo();
                SMT.SaaS.BLLCommonServices.Utility.RemoveMyRecord("FB", eInfo.Type, orderid);
                // SMT.SaaS.BLLCommonServices.Utility.RemoveMyRecord<EntityObject>(entity);
            }
            catch (Exception ex)
            {
                strTemp = ex.ToString();
            }
            return strTemp;
        }

        private void InitEntityTypeList()
        {
            entityTypeList.Add(typeof(T_FB_COMPANYBUDGETSUMMASTER).Name);
            entityTypeList.Add(typeof(T_FB_COMPANYBUDGETMODMASTER).Name);
            entityTypeList.Add(typeof(T_FB_COMPANYTRANSFERMASTER).Name);

            entityTypeList.Add(typeof(T_FB_DEPTBUDGETSUMMASTER).Name);
            entityTypeList.Add(typeof(T_FB_DEPTBUDGETADDMASTER).Name);
            entityTypeList.Add(typeof(T_FB_DEPTTRANSFERMASTER).Name);

            entityTypeList.Add(typeof(T_FB_PERSONMONEYASSIGNMASTER).Name);

            entityTypeList.Add(typeof(T_FB_BORROWAPPLYMASTER).Name);
            entityTypeList.Add(typeof(T_FB_REPAYAPPLYMASTER).Name);

            entityTypeList.Add(typeof(T_FB_CHARGEAPPLYMASTER).Name);
            entityTypeList.Add(typeof(T_FB_TRAVELEXPAPPLYMASTER).Name);

            entityTypeList.Add(typeof(T_FB_COMPANYBUDGETAPPLYMASTER).Name);
            entityTypeList.Add(typeof(T_FB_DEPTBUDGETAPPLYMASTER).Name);
        }
        List<string> entityTypeList = new List<string>();
        #endregion

        public void Attach(EntityObject entity)
        {
            this.baseDal.Attach(entity);
        }

        public void DeleteObject(EntityObject entity)
        {
            this.baseDal.DeleteObject(entity);
        }


        public int SaveChanges()
        {
            return this.baseDal.SaveChanges();
        }

        public void Dispose()
        {
            this.baseDal.Dispose();
            this._baseDal = null;
        }
    }

}
