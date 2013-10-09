using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.FB.BLL;
using System.Data.Objects.DataClasses;
using SMT_FB_EFModel;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Reflection;
using System.Collections;

using FlowWFService = SMT.SaaS.BLLCommonServices.FlowWFService;
using System.Xml.Linq;
using SMT.Foundation.Log;
using System.Data;
using SMT.FB.DAL;

namespace SMT.FB.BLL
{

    
    #region LockObject
    public class LockManager
    {

        private Object thisLock = new Object();
        private HashSet<string> OrderHandling = new HashSet<string>();

        public bool LockOrder(string keyValue)
        {

            bool result = true;
            lock (thisLock)
            {
                if (!OrderHandling.Contains(keyValue))
                {
                    OrderHandling.Add(keyValue);
                    result = false;
                }
            };
            return result;


        }
        public bool ReleaseOrder(string keyValue)
        {
            bool result = false;
            lock (thisLock)
            {
                if (OrderHandling.Contains(keyValue))
                {
                    OrderHandling.Remove(keyValue);
                    result = true;
                }
            };
            return result;
        }

    }
    #endregion

    #region 扩展方法
    public static class ListExtension
    {
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    action(item);
                }
            }
        }
    
    }
    public static class EntityInfoExtension
    {
        public static EntityObject Query(this EntityInfo entityInfo, string keyValue,BaseBLL bll)
        {
            var qe = QueryExpression.Equal(entityInfo.KeyName, keyValue);
            qe.QueryType = entityInfo.Type;
            var data = qe.Query(bll);
            return data.FirstOrDefault();
        }
    }
    
    public static class EntityObjectExtension
    {
        public static EntityInfo GetEntityInfo(this EntityObject entity)
        {
            string entityType = entity.GetType().Name;
            var entityInfo = FBCommonBLL.FBCommonEntityList.FirstOrDefault(item => item.Type == entityType);
            if (entityInfo == null)
            {
                // throw new Exception("找不到相应的单据结构信息, type: " + entityType);
                return null;
            }
            return entityInfo;
        }
        public static bool SetValue(this EntityObject entityObject, string propertyName, object value)
        {
            Type type = entityObject.GetType();
            PropertyInfo p = type.GetProperty(propertyName);
          
            if (p != null)
            {
                p.SetValue(entityObject, value, null);
                return true;
            }
            return false;

        }

        public static object GetValue(this EntityObject entityObject, string propertyName)
        {
            Type type = entityObject.GetType();
            PropertyInfo p = type.GetProperty(propertyName);

            if (p != null)
            {
                return p.GetValue(entityObject, null);
            }
            return null;

        }

        public static List<FBEntity> ToFBEntityList<T>(this List<T> list) where T : EntityObject
        {
            if (list == null)
            {
                return null;
            }
            List<FBEntity> listResult = new List<FBEntity>();
            list.ForEach(p =>
            {
                listResult.Add(p.ToFBEntity());
            });
            return listResult;
        }

        public static List<T> ToEntityList<T>(this List<FBEntity> list) where T : EntityObject
        {
            if (list == null)
            {
                return null;
            }
            List<T> listResult = new List<T>();
            list.ForEach(p =>
            {
                var r = p.Entity as T;
                if (r != null)
                {
                    listResult.Add(r);
                }
            });
            return listResult;
        }

        public static FBEntity ToFBEntity<T>(this T t) where T : EntityObject
        {
            if (t.GetType() == typeof(FBEntity))
            {
                return t as FBEntity;
            }
            FBEntity fbEntity = new FBEntity();
            fbEntity.Entity = t;
            fbEntity.CollectionEntity = new List<RelationManyEntity>();
            return fbEntity;   
        }

        public static List<T> AsObjectList<T>(this IList list)
        {
            List<T> listResult = new List<T>();
            if (list == null)
            {
                return listResult;
            }

            for (int i = 0; i < list.Count; i++)
            {
                T t = (T)list[i];
                if (t != null)
                {
                    listResult.Add(t);
                }
            }
            return listResult;
        }

        public static void CloneEntity<T>(T sourceObj, T targetObj) where T : class
        {
            Type a = sourceObj.GetType();
            PropertyInfo[] infos = a.GetProperties();
            foreach (PropertyInfo prop in infos)
            {
                //System.Data.Objects.DataClasses.
                if (prop.PropertyType.BaseType == typeof(EntityReference)
                    || prop.PropertyType.BaseType == typeof(RelatedEnd)
                    || prop.PropertyType == typeof(System.Data.EntityState)
                    || prop.PropertyType == typeof(System.Data.EntityKey)
                    || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                    continue;
                if (sourceObj is EntityObject)
                {
                    EntityObject ent = sourceObj as EntityObject;

                    if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
                    {
                        bool isKeyField = false;
                        foreach (var key in ent.EntityKey.EntityKeyValues)
                        {
                            if (key.Key == prop.Name)
                            {
                                isKeyField = true;
                                break;
                            }
                        }
                        if (isKeyField)
                            continue;
                    }
                }
                //prop.Name
                object value = prop.GetValue(sourceObj, null);
                try
                {
                    prop.SetValue(targetObj, value, null);
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }
        }
        public static List<TResult> CreateList<T, TResult>(this List<T> listSource, Func<T, TResult> func)
        {
            List<TResult> listResult = new List<TResult>();
            listSource.ForEach(item =>
            {
                TResult tr = func(item);
                if (tr != null)
                {
                    listResult.Add(tr);
                }
                
            });
            return listResult;
        }

        public static TEntity CopyEntity<TEntity>(this TEntity source)
            where TEntity : EntityObject, new()
        {
            TEntity target = new TEntity();
            List<PropertyInfo> listP = typeof(TEntity).GetProperties().ToList();
            List<string> BaseProperty = new List<string>{"EntityKey", "EntityState"};
            listP.ForEach(property =>
            {

                bool isCopyProperty = property.GetCustomAttributes(typeof(System.Runtime.Serialization.DataMemberAttribute), true).Count() > 0;
                bool isList = typeof(IListSource).IsAssignableFrom(property.PropertyType);
                bool isEntityReference = typeof(EntityReference).IsAssignableFrom(property.PropertyType);
                bool isBaseEntityProperty = BaseProperty.Contains(property.Name);
   
                if (isCopyProperty && !isList && !isEntityReference && !isBaseEntityProperty)
                {
                   
                    source.CopyTo<TEntity>(target, property.Name);
                }
            });
            return target;
        }

        public static void CopyTo<TEntity>(this TEntity sourceFBEntity, TEntity targetFBEntity, string propertyName)
            where TEntity : EntityObject
        {
            object value = sourceFBEntity.GetValue(propertyName);
            targetFBEntity.SetValue(propertyName, value);
        }

        /// <summary>
        /// 找出单据ID
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string GetOrderID(this EntityObject entity)
        {
            
            var entityInfo = entity.GetEntityInfo();
            if (entityInfo == null)
            {
                return null;
            }
            string keyValue = entity.GetValue(entityInfo.KeyName).ToString();
            return keyValue;
        }
    }

    public static class FBEntityExtesion
    {
        /// <summary>
        /// 根据 entityType , 返回对应的有关系的 FBEntity的集合, 
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="entityType">有关系的对象集合类型</param>
        /// <returns>如果不存在,将会创建相应的集合</returns>
        public static List<FBEntity> GetRelationFBEntities(this FBEntity fbEntity, string entityType)
        {
            List<FBEntity> listFBEntities = null;

            RelationManyEntity rmEntity = fbEntity.CollectionEntity.FirstOrDefault(item =>
            {
                return item.EntityType == entityType;
            });

            if (rmEntity == null)
            {
                rmEntity = new RelationManyEntity();
                rmEntity.EntityType = entityType;
                rmEntity.FBEntities = new List<FBEntity>();
                fbEntity.CollectionEntity.Add(rmEntity);
            }
            listFBEntities = rmEntity.FBEntities;
            return listFBEntities;
        }


        /// <summary>
        /// 添加 有关系的集合FBEntity
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="listNew"></param>
        public static void AddFBEntities<T>(this FBEntity fbEntity, IList<FBEntity> listNew)
        {
            string entityName = typeof(T).Name;

            List<FBEntity> list = fbEntity.GetRelationFBEntities(entityName);

            foreach (FBEntity item in listNew)
            {
                list.Add(item);
            }
        }

        public static void AddReferenceFBEntity<T>(this FBEntity fbEntity, FBEntity referenceFBEntity)
        {
            string entityName = typeof(T).Name;
            RelationOneEntity roe = new RelationOneEntity();
            roe.EntityType = entityName;
            roe.FBEntity = referenceFBEntity;
            fbEntity.ReferencedEntity.Add(roe);
        }


        public static void OrderDetailBy<T>(this FBEntity fbEntity, Func<T, object> keySelector) where T : EntityObject
        {
             var details = fbEntity.GetRelationFBEntities(typeof(T).Name);

             var result = details.OrderBy(item =>
             {
                 return keySelector(item.Entity as T);
             }).ToList();

            details.Clear();
            details.AddRange(result);
        }


        //public static void OrderDetailBy<Tkey>(this List<FBEntity> details, Func<EntityObject, Tkey> keySelector)
        //{
        //    // var details = fbEntity.GetRelationFBEntities(typeof(T).Name);

        //    var result = details.OrderBy(item =>
        //    {
        //        return keySelector(item.Entity);
        //    }).ToList();

        //    details.Clear();
        //    details.AddRange(result);
        //    // fbEntity.AddFBEntities<T>(result);
        //}
    }

    public static class QueryExpressionExtension
    {
        public static QueryExpression GetQueryExpression(this QueryExpression qe, string propertyName)
        {
            QueryExpression result = null;
            List<QueryExpression> listQE = qe.ToList();
            QueryExpression qeFind = listQE.FirstOrDefault(item =>
                {
                    return item.PropertyName == propertyName;
                });

            if (qeFind != null)
            {
                result = new QueryExpression
                {
                    PropertyValue = qeFind.PropertyValue,
                    PropertyName = qeFind.PropertyName
                };

            }
            return result;
        }

        public static QueryExpression And(this QueryExpression qe, string propertyName, string propertyValue)
        {
            QueryExpression qeTop = QueryExpression.Equal(propertyName, propertyValue);
            qeTop.RelatedExpression = qe;
            return qeTop;
        }

        public static IQueryable<TEntity> Query<TEntity>(this QueryExpression qe, BaseBLL bll) where TEntity : EntityObject
        {
            return bll.InnerGetEntities<TEntity>(qe);
        }

        public static List<EntityObject> Query(this QueryExpression qe, BaseBLL bll)
        {
            return bll.BaseGetEntities(qe);
        }

    }

    public static class NullableExtension
    {
        
         public static CompareResult Compare(this Nullable<decimal> a, Nullable<decimal> b)
         {
             if (a == null)
             {
                 a = new Nullable<Decimal>(0);
             }
             if ( b == null )
             {
                 b = new Nullable<Decimal>(0);
             }
             if (a.Value > b.Value)
             {
                 return CompareResult.Bigger;
             }
             else if (a.Value == b.Value)
             {
                 return CompareResult.Equal;
             }
             else
             {
                 return CompareResult.Less;
             }
         }

         public static bool Equal(this Nullable<decimal> a, Nullable<decimal> b)
         {
             return a.Compare(b) == CompareResult.Equal;
         }

         public static bool BiggerThan(this Nullable<decimal> a, Nullable<decimal> b)
         {
             return a.Compare(b) == CompareResult.Bigger;
         }

         public static bool LessThan(this Nullable<decimal> a, Nullable<decimal> b)
         {
             return a.Compare(b) == CompareResult.Less;
         }

         public static Nullable<decimal> Add(this Nullable<decimal> a, Nullable<decimal> b)
         {
             if (a == null)
             {
                 a = new Nullable<decimal>(0);
             }
             if (b == null)
             {
                 b = new Nullable<decimal>(0);
             }
             return new Nullable<decimal>(a.Value + b.Value);
         }

         public static Nullable<decimal> Subtract(this Nullable<decimal> a, Nullable<decimal> b)
         {
             if (a == null)
             {
                 a = new Nullable<decimal>(0);
             }
             if (b == null)
             {
                 b = new Nullable<decimal>(0);
             }
             return new Nullable<decimal>(a.Value - b.Value);
         }
    }


    public static class XmlHelper
    {

        public static XElement ToXml(this object obj)
        {
            if (obj == null)
            {
                return new XElement("Error", "Error:: 对象为nul! ");
            }

            List<object> listDone = new List<object>();

            return obj.ToXml(null, listDone);
        }

        /// <summary>
        /// 对单个节点的操作
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static XElement ToXml(this object obj, string xName, List<object> listDone)
        {
            try
            {
                if (obj == null)
                {
                    if (xName != null)
                    {
                        return new XElement(xName);
                    }
                    return null;
                }
                Type curType = obj.GetType();
                if (xName == null)
                {
                    xName = curType.Name;
                }
                XElement result = null;


                // 值类型
                if (curType.IsValueType || curType.IsEnum || curType == typeof(string))
                {
                    result = new XElement(xName);
                    result.Add(obj);
                }
                else
                {
                    IEnumerable ie = obj as IEnumerable;
                    // 集合类型
                    if (ie != null)
                    {
                        result = ie.ListToXml(xName, listDone);
                    }
                    else // 单个对象类型
                    {
                        result = obj.ObjectToXml(xName, listDone);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 集合对象的输出
        /// </summary>
        /// <param name="list"></param>
        /// <param name="xName"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static XElement ListToXml(this IEnumerable list, string xName, List<object> listDone)
        {
            var result = new XElement(xName);
            foreach (var item in list)
            {
                result.Add(item.ToXml(null, listDone));
            }
            return result;
        }
        /// <summary>
        /// 单个实例的输出
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="xName"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static XElement ObjectToXml(this object obj, string xName, List<object> listDone)
        {
            if (listDone.CheckAndAddToExist(obj))
            {
                return new XElement(xName,"Warn:: 对象已处理过! ");
            }

            //特殊处理
            var exResult = ExpectObjectToXml(obj, xName, listDone);
            if (exResult != null)
            {
                return exResult;
            }

            var result = new XElement(xName);
            Type type = obj.GetType();
            var ps = type.GetProperties(System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.GetProperty |
                System.Reflection.BindingFlags.SetProperty);
            foreach (var item in ps)
            {
                var itemValue = item.GetValue(obj, null);
                result.Add(itemValue.ToXml(item.Name, listDone));
            }
            return result;
        }
        /// <summary>
        /// 检查是否obj已存在listDone中，存在返回true,否则添加到listDone列表中，并返回false.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="listDone"></param>
        /// <returns></returns>
        private static bool CheckAndAddToExist(this List<object> listDone, object obj)
        {
            bool result = false;
            if (listDone.Contains(obj)) // 是否存在
            {
                
                result = true;
            }
            listDone.Add(obj);          // 添加列表,表达已处理过 
            return result;
        }

       
        private static XElement ExpectObjectToXml(this object obj, string xName, List<object> listDone)
        {
            Type type = obj.GetType();
            if (type == typeof(EntityKey))
            {
                return new XElement(xName, "Type: EntityKey, 暂不实现xml! ");
            }
            else if (type == typeof(Type))
            {
                return new XElement(xName, "Type : Type, 暂不实现xml! ");
            }
            else if (typeof(EntityReference).IsAssignableFrom(type))
            {
                return new XElement(xName, "Type: EntityReference, 暂不实现xml! ");
            }
            
            return null;
        }
    }
    #endregion

    #region BLL异常
    public class FBBLLException : Exception
    {
        public FBBLLException(string message)
            : base(message)
        {
        }
        public FBBLLException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    #endregion

    #region 实体
    /// <summary>
    /// 审核结果类
    /// </summary>
    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class AuditResult : SaveResult
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public FlowWFService.DataResult DataResult { get; set; }
    }

    /// <summary>
    /// 实体信息类
    /// </summary>
    public class EntityInfo
    {
        public string EntityCode { get; set; }
        public string Type { get; set; }
        public string KeyName { get; set; }
        public string CodeName { get; set; }
        public List<FieldInfo> FielInfos { get; set; }
    }
    /// <summary>
    /// 字段属性信息类
    /// </summary>
    public class FieldInfo
    {
        public string FieldName { get; set; }
        public string TypeName { get; set; }
    }
    #endregion
}
