using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;
using System.Reflection;
using System.Data;

namespace SMT.SaaS.Permission.BLL
{

    public class Utility
    {
        //public static EntityObject[] GetLookupData(EntityNames entity, Dictionary<string, string> args)
        //{
        //    EntityObject[] ents = null;
        //    Type type = Type.GetType("SMT.SaaS.Permission.BLL." + entity.ToString() + "BLL");

        //    if (type != null)
        //    {
        //        ILookupEntity bll = (ILookupEntity)Activator.CreateInstance(type);
        //        ents =  bll.GetLookupData(args);
        //    }
        //    return ents;
        //}
        public static EntityObject[] GetLookupData(EntityNames entity, Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            EntityObject[] ents = null;
            Type type = Type.GetType("SMT.SaaS.Permission.BLL." + entity.ToString() + "BLL");

            if (type != null)
            {
                ILookupEntity bll = (ILookupEntity)Activator.CreateInstance(type);
                ents = bll.GetLookupData( args,  pageIndex,  pageSize,  sort,  filterString,  paras, ref  pageCount);
            }
            return ents;
        }
        public static string SerializeObject(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer serializer =
                        new XmlSerializer(obj.GetType());
                serializer.Serialize(ms, obj);
                ms.Position = 0;

                using (StreamReader reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        public static T DeserializeObject<T>(string objString)
        {
            //List<Person> persons = DeserializeObject<List<Person>>( jsonString )
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                return (T)serializer.Deserialize(ms);
            }
        }
        public static object DeserializeObject(string objString, Type type)
        {
            //List<Person> persons = DeserializeObject<List<Person>>( jsonString )
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(type);

                return serializer.Deserialize(ms);
            }
        }

        public static void CloneEntity<T>(T sourceObj, T targetObj) where T : class
        {
            Type a = sourceObj.GetType();
            string aa = "";
            PropertyInfo[] infos = a.GetProperties();
            foreach (PropertyInfo prop in infos)
            {
                aa += prop.Name + "#";
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



        public static void RefreshEntity(EntityObject entity)
        {
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


        /// <summary>
        /// 对IQueryable对像分页处理
        /// </summary>
        /// <typeparam name="T">需要分页对像类型</typeparam>
        /// <param name="ents">需要分页的IQueryable对像,必需是有排过序的</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页记录条数</param>
        /// <param name="pageCount">页数</param>
        /// <returns>IQueryable对像</returns>
        public static IQueryable<T> Pager<T>(IQueryable<T> ents, int pageIndex, int pageSize, ref int pageCount)
        {
            int count = ents.Count();
            pageCount = count / pageSize;
            int tmp = count % pageSize;

            pageCount = pageCount + (tmp > 0 ? 1 : 0);
            if (pageIndex > pageCount)
                pageIndex = 1;
            //考虑往前翻页的情况
            if (pageIndex <=0)
                pageIndex = 1;
            ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return ents;
        }

        /// <summary>
        /// 分配对象类别
        /// </summary>
        public enum PermissionRange
        {
            /// <summary>
            /// 集团
            /// </summary>
            Organize,
            /// <summary>
            /// 公司
            /// </summary>
            Company,
            /// <summary>
            /// 部门
            /// </summary>
            Department,
            /// <summary>
            /// 岗位
            /// </summary>
            Post,
            /// <summary>
            /// 员工
            /// </summary>
            Employee

        }


        /// <summary>
        /// 即时通讯组织架构枚举类型
        /// </summary>
        public enum IMOrganize
        {
            //公司
            Company,
            //部门
            Deaprtment
        }

        

    }
}
