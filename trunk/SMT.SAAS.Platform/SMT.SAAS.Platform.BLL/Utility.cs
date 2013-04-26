using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Data;
using SMT.Foundation.Log;

namespace SMT.SAAS.Platform.BLL
{
    //public static class Utility
    //{
    //    /// <summary>
    //    /// 异常日志公共方法
    //    /// </summary>
    //    public static void WriterErrorLog(string typeName, string methodName, Exception exception)
    //    {
    //        StringBuilder message = new StringBuilder();
    //        message.AppendLine("--------------------------------异******常--------------------------------");
    //        message.Append("异常文件名   :  ");
    //        message.Append(typeName);
    //        message.AppendLine();
    //        message.Append("异常方法名 :  ");
    //        message.Append(methodName);
    //        message.AppendLine();
    //        message.Append("异常消息    :  ");
    //        message.AppendLine();
    //        message.Append("\t");
    //        message.Append(exception.Message);

    //        if (exception.InnerException != null)
    //        {
    //            message.AppendLine();
    //            message.Append("内部异常消息        :  ");
    //            message.AppendLine();
    //            message.Append("\t");
    //            message.Append(exception.InnerException.Message);
    //        }
    //        message.AppendLine();
    //        message.Append("异常产生时间  :");
    //        Tracer.Debug(message.ToString());
    //    }

    //    public static string SerializeObject(object obj)
    //    {
    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //            XmlSerializer serializer =
    //                    new XmlSerializer(obj.GetType());
    //            serializer.Serialize(ms, obj);
    //            ms.Position = 0;

    //            using (StreamReader reader = new StreamReader(ms))
    //            {
    //                return reader.ReadToEnd();
    //            }
    //        }
    //    }
    //    public static T DeserializeObject<T>(string objString)
    //    {
    //        //List<Person> persons = DeserializeObject<List<Person>>( jsonString )
    //        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString)))
    //        {
    //            XmlSerializer serializer = new XmlSerializer(typeof(T));

    //            return (T)serializer.Deserialize(ms);
    //        }
    //    }
    //    public static object DeserializeObject(string objString, Type type)
    //    {
    //        //List<Person> persons = DeserializeObject<List<Person>>( jsonString )
    //        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString)))
    //        {
    //            XmlSerializer serializer = new XmlSerializer(type);

    //            return serializer.Deserialize(ms);
    //        }
    //    }
    //    /// <summary>
    //    /// 对IQueryable对像分页处理
    //    /// </summary>
    //    /// <typeparam name="T">需要分页对像类型</typeparam>
    //    /// <param name="ents">需要分页的IQueryable对像,必需是有排过序的</param>
    //    /// <param name="pageIndex">当前页</param>
    //    /// <param name="pageSize">每页记录条数</param>
    //    /// <param name="pageCount">页数</param>
    //    /// <returns>IQueryable对像</returns>
    //    public static IQueryable<T> Pager<T>(IQueryable<T> ents, int pageIndex, int pageSize, ref int pageCount)
    //    {
    //        int count = ents.Count();
    //        pageCount = count / pageSize;
    //        int tmp = count % pageSize;

    //        pageCount = pageCount + (tmp > 0 ? 1 : 0);
    //        if (pageIndex > pageCount)
    //            pageIndex = 1;

    //        ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
    //        return ents;
    //    }

    //    /// <summary>
    //    /// 对IQueryable对像分页处理
    //    /// </summary>
    //    /// <typeparam name="T">需要分页对像类型</typeparam>
    //    /// <param name="ents">需要分页的IQueryable对像,必需是有排过序的</param>
    //    /// <param name="pageIndex">当前页</param>
    //    /// <param name="pageSize">每页记录条数</param>
    //    /// <param name="pageCount">页数</param>
    //    /// <returns>IQueryable对像</returns>
    //    public static List<T> Pager<T>(List<T> ents, int pageIndex, int pageSize, ref int pageCount)
    //    {
    //        int count = ents.Count();
    //        pageCount = count / pageSize;
    //        int tmp = count % pageSize;

    //        pageCount = pageCount + (tmp > 0 ? 1 : 0);
    //        if (pageIndex > pageCount)
    //            pageIndex = 1;

    //        ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
    //        return ents;
    //    }

    //    public static void CloneEntity<T>(T sourceObj, T targetObj) where T : class
    //    {
    //        Type a = sourceObj.GetType();
    //        PropertyInfo[] infos = a.GetProperties();
    //        foreach (PropertyInfo prop in infos)
    //        {
    //            //System.Data.Objects.DataClasses.
    //            if (prop.PropertyType.BaseType == typeof(EntityReference)
    //                || prop.PropertyType.BaseType == typeof(RelatedEnd)
    //                || prop.PropertyType == typeof(System.Data.EntityState)
    //                || prop.PropertyType == typeof(System.Data.EntityKey)
    //                || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
    //                continue;
    //            if (sourceObj is EntityObject)
    //            {
    //                EntityObject ent = sourceObj as EntityObject;

    //                if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
    //                {
    //                    bool isKeyField = false;
    //                    foreach (var key in ent.EntityKey.EntityKeyValues)
    //                    {
    //                        if (key.Key == prop.Name)
    //                        {
    //                            isKeyField = true;
    //                            break;
    //                        }
    //                    }
    //                    if (isKeyField)
    //                        continue;
    //                }
    //            }
    //            //prop.Name
    //            object value = prop.GetValue(sourceObj, null);
    //            try
    //            {
    //                prop.SetValue(targetObj, value, null);
    //            }
    //            catch (Exception ex)
    //            {
    //                string e = ex.Message;
    //            }
    //        }
    //    }
    //    public static void RefreshEntity(EntityObject entity)
    //    {
    //        var rs = (entity as IEntityWithRelationships).RelationshipManager.GetAllRelatedEnds();

    //        foreach (IRelatedEnd re in rs)
    //        {
    //            List<EntityObject> list = new List<EntityObject>();
    //            foreach (var item in re)
    //            {
    //                list.Add(item as EntityObject);
    //            }
    //            list.ForEach(p =>
    //            {
    //                if (re.GetType().BaseType == typeof(EntityReference))
    //                {
    //                    EntityKey eKey = p.EntityKey;
    //                    if (eKey != null)
    //                    {
    //                        (re as EntityReference).EntityKey = eKey;
    //                        re.Remove(p);
    //                    }
    //                }

    //            });
    //        }
    //    }
    //}
}
