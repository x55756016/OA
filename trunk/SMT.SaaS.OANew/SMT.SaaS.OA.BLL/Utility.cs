using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml.Serialization;
using SMT.SaaS.OA.DAL.Views;

using System.Data.Objects.DataClasses;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using System.Xml;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Mapping;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using System.IO;
using System.Linq.Expressions;

namespace SMT.SaaS.OA.BLL
{
    public class Utility
    {

        public static string strEngineFuncWSSite = ConfigurationManager.AppSettings["EngineFuncWSSite"];

        public static object GetLookupData(EntityNames entity, Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            object ents = null;
            Type type = Type.GetType("SMT.SaaS.OA.BLL." + entity.ToString() + "Bll");

            if (type != null)
            {
                ILookupEntity bll = (ILookupEntity)Activator.CreateInstance(type);
                ents = bll.GetLookupData(args, pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID);
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
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                return (T)serializer.Deserialize(ms);
            }
        }
        public static object DeserializeObject(string objString, Type type)
        {
            //List<Person> persons = DeserializeObject<List<Person>>( jsonString )
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(type);

                return serializer.Deserialize(ms);
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

            ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            //ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return ents;
        }



        /// <summary>
        /// 对IQueryable对像分页处理---手机分页
        /// </summary>
        /// <typeparam name="T">需要分页对像类型</typeparam>
        /// <param name="ents">需要分页的IQueryable对像,必需是有排过序的</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页记录条数</param>
        /// <param name="pageCount">页数</param>
        /// <returns>IQueryable对像</returns>
        public static IQueryable<T> PagerMobile<T>(IQueryable<T> ents, int pageIndex, int pageSize, ref int pageCount,ref int count)
        {
            count = ents.Count();
            pageCount = count / pageSize;
            int tmp = count % pageSize;

            pageCount = pageCount + (tmp > 0 ? 1 : 0);
            if (pageIndex > pageCount)
                pageIndex = 1;

            ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            //ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return ents;
        }

        public　static　IQueryable<T>　DataSorting<T>(IQueryable<T>　source,　string　sortExpression,　string　sortDirection)
        {
            try
            {
                string sortingDir = string.Empty;
                if (sortDirection.ToUpper().Trim() == "ASC")
                    sortingDir = "OrderBy";
                else if (sortDirection.ToUpper().Trim() == "DESC")
                    sortingDir = "OrderByDescending";
                //sortExpression = "OWNERID";
                ParameterExpression param = Expression.Parameter(typeof(T), sortExpression);
                PropertyInfo pi = typeof(T).GetProperty(sortExpression);
                Type[] types = new Type[2];
                types[0] = typeof(T);
                types[1] = pi.PropertyType;
                Expression expr = Expression.Call(typeof(Queryable), sortingDir, types, source.Expression, Expression.Lambda(Expression.Property(param, sortExpression), param));
                IQueryable<T> query = source.AsQueryable().Provider.CreateQuery<T>(expr);
                return query;
            }
            catch (Exception ex)
            {
                string aa = ex.ToString();
                return null;
            }
           
　　　　　　　　　　　　　
　　　　　　　　　　　　　
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

        public static void SendEngineEventTriggerData(IList<object> paras)
        {
            StringBuilder strRes = new StringBuilder();

            strRes.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            strRes.Append("<System>");
            strRes.Append("<CompanyCode>" + paras[0].ToString() + "</CompanyCode>");
            strRes.Append("<SystemCode>" + paras[1].ToString() + "</SystemCode>");
            strRes.Append("<ModelCode>" + paras[2].ToString() + "</ModelCode>");
            strRes.Append("<ApplicationOrderCode>" + paras[3].ToString() + "</ApplicationOrderCode>");
            strRes.Append("<TaskStartDate>" + paras[4].ToString() + "</TaskStartDate>");
            strRes.Append("<TaskStartTime>" + paras[5].ToString() + "</TaskStartTime>");
            strRes.Append("<ProcessCycle>" + paras[6].ToString() + "</ProcessCycle>");
            strRes.Append("<ReceiveUser>" + paras[7].ToString() + "</ReceiveUser>");
            strRes.Append("<MessageBody>" + paras[8].ToString() + "</MessageBody>");
            strRes.Append("<MsgLinkUrl>" + paras[9].ToString() + "</MsgLinkUrl>");
            strRes.Append("<ProcessWcfUrl>" + paras[10].ToString() + "</ProcessWcfUrl>");
            strRes.Append("<WcfFuncName>" + paras[11].ToString() + "</WcfFuncName>");
            strRes.Append("<WcfFuncParamter>" + paras[12].ToString() + "</WcfFuncParamter>");
            strRes.Append("<WcfParamSplitChar>" + paras[13].ToString() + "</WcfParamSplitChar>");
            strRes.Append("<WcfBinding>" + paras[14].ToString() + "</WcfBinding>");
            strRes.Append("</System>");

            //return strRes.ToString();
            EngineWS.EngineWcfGlobalFunctionClient EngineClient = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineClient.SaveEventData(strRes.ToString());
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
        /// 引擎需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="Prameters">当需要传递不在当前实体中的属性值时使用(出差自动发起流程调用)</param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode, string currentUserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<System>");
            Type objtype = objectdata.GetType();
            sb.AppendLine("<Name>" + SystemCode + "</Name>");
            sb.AppendLine("<Object Name=\"" + objtype.Name + "\" Description=\"" + "\">");
            PropertyInfo[] propinfos = objtype.GetProperties();
            foreach (PropertyInfo propinfo in propinfos)
            {
                if (propinfo.Name.ToUpper() != "CHECKSTATE")
                {
                    sb.AppendLine("<Attribute Name=\"" + propinfo.Name + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + propinfo.GetValue(objectdata, null) + "\"/>");
                }
            }
            foreach (var q in Prameters)
            {
                sb.AppendLine("<Attribute Name=\"" + q.Key + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + q.Value + "\"/>");
            }
            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + currentUserName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
        }

        public static string ObjListToXml<T>(T objectdata, string SystemCode, string currentUserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<System>");
            Type objtype = objectdata.GetType();
            sb.AppendLine("<Name>" + SystemCode + "</Name>");
            sb.AppendLine("<Object Name=\"Approval\" Description=\"\">");
            PropertyInfo[] propinfos = objtype.GetProperties();
            foreach (PropertyInfo propinfo in propinfos)
            {
                if (propinfo.Name.ToUpper() != "CHECKSTATE")
                {
                    sb.AppendLine("<Attribute Name=\"" + propinfo.Name + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + propinfo.GetValue(objectdata, null) + "\"/>");
                }
            }
            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + currentUserName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();

        }

        /// <summary>
        /// 增加实体entitykey
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="keyName">主键名称</param>
        /// <param name="ID">主键值</param>
        /// <returns>返回entitykey</returns>
        public static EntityKey AddEntityKey(string entityName, string keyName, string ID)
        {
            if (!(string.IsNullOrEmpty(entityName) && string.IsNullOrEmpty(keyName) && string.IsNullOrEmpty(ID)))
            {
                string qualifiedEntitySetName = ConfigurationManager.AppSettings["DBContextName"] + ".";
                return new EntityKey(qualifiedEntitySetName + entityName,
keyName, ID);
            }
            return null;
        }

    }
}
