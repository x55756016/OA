using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;
using System.Reflection;
//using SMT.SaaS.OA.BLL.DataSource;
using SMT.SaaS.BLLCommonServices.EngineConfigWS;

namespace SMT.SaaS.OA.BLL
{
    public class EngineDataSourceBll
    {
        public int StarEngine<T>(T oldDataObject, T newDataObject, string nextUserID)
        {
            string xmlString = null;
            string errorString = null;
            if (ObjListToXml(oldDataObject, newDataObject, ref xmlString, ref errorString, nextUserID) == -1)
            {
                return -1;//生成失败
            }
            EngineWcfGlobalFunctionClient wcfclient = new EngineWcfGlobalFunctionClient();
            
            //string aaa = wcfclient.test();
            //wcfclient.SaveTriggerData(xmlString);
            return 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldUser">旧对象</param>
        /// <param name="newUser"></param>
        /// <param name="xmlString"></param>
        /// <param name="errorString"></param>
        /// <returns></returns>
        public int ObjListToXml<T>(T oldDataObject, T newDataObject, ref string xmlString, ref string errorString, string nextUserId)
        {
            //if (oldDataObject.GetType().BaseType != typeof(EntityObject))
            //{
            //    errorString = "类不是规定的类型";
            //    return -1;
            //}
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            //sb.AppendLine("<table>");
            //sb.AppendLine("<ApplicationSystem> " + "EngineSystem_test" + " </ApplicationSystem>");
            //sb.AppendLine("<CompanyCode> " + "smt_online" + " </CompanyCode> ");
            //sb.AppendLine("<OperationUser> " + "steven" + " </OperationUser>");
            //string tableName = null;//表名
            //PropertyInfo[] propinfos = null;//属性
            //if (propinfos == null)
            //{
            //    Type objtype = oldDataObject.GetType();
            //    propinfos = objtype.GetProperties();
            //    tableName = objtype.Name;
            //}
            //sb.AppendLine("<TableName> " + tableName + " </TableName>");

            //EntityObject oldEntRef = oldDataObject as EntityObject;
            //List<string> keyList = new List<string>();
            //foreach (System.Data.EntityKeyMember key in oldEntRef.EntityKey.EntityKeyValues)
            //{
            //    keyList.Add(key.Key);
            //}
            //try
            //{
            //    foreach (PropertyInfo propinfo in propinfos)
            //    {
            //        if (keyList.Contains(propinfo.Name))
            //        {
            //            sb.AppendLine("<TableKey>");
            //            sb.Append("<TableKeyName>" + propinfo.Name + "</TableKeyName>");
            //            sb.Append("<TableKeyValue>" + GetString(propinfo.GetValue(oldDataObject, null)) + "</TableKeyValue>");
            //            sb.AppendLine("</TableKey>");

            //            sb.AppendLine("<FieldString>");
            //            sb.Append("<FieldName>" + propinfo.Name + "</FieldName>");
            //            sb.Append("<Field_old_value>" + GetString(propinfo.GetValue(oldDataObject, null)) + "</Field_old_value>");
            //            sb.Append("<Field_New_value>" + GetString(propinfo.GetValue(newDataObject, null)) + "</Field_New_value>");
            //            sb.AppendLine("</FieldString>");
            //        }
            //        else
            //        {
            //            if (propinfo.Name != "EntityState" && propinfo.Name != "EntityKey")
            //            {
            //                sb.AppendLine("<FieldString>");
            //                sb.Append("<FieldName>" + propinfo.Name + "</FieldName>");
            //                sb.Append("<Field_old_value>" + GetString(propinfo.GetValue(oldDataObject, null)) + "</Field_old_value>");
            //                sb.Append("<Field_New_value>" + GetString(propinfo.GetValue(newDataObject, null)) + "</Field_New_value>");
            //                sb.AppendLine("</FieldString>");
            //            }
            //            else
            //            {
            //                continue;
            //            }//nextUserId
            //        }
            //    }
            //    sb.AppendLine("<FieldString>");
            //    sb.Append("<FieldName>NextUserName</FieldName>");
            //    sb.Append("<Field_New_value>" + GetString(nextUserId) + "</Field_New_value>");
            //    sb.AppendLine("</FieldString>");
            //}
            //catch (Exception e)
            //{
            //    string cMessage = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Source + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.TargetSite + "]";
            //    //Trace.WriteLog(ZFILE_NAME, cMessage);
            //    //return ReturnValue;
            //}
            //sb.AppendLine("</table>");
            //xmlString = sb.ToString();
            return 1;
        }

        public string GetString(object value)
        {
            try
            {
                if (value == null)
                {
                    return "";
                }
                switch (value.GetType().ToString())
                {
                    case "System.String":
                        return (string)value;
                    case "System.Int64":
                        return ((long)value).ToString();
                    case "System.Decimal":
                        return ((decimal)value).ToString();
                    case "System.Int32":
                        return ((int)value).ToString();
                    case "System.Double":
                        return ((double)value).ToString();
                    case "System.Boolean":
                        return ((bool)value).ToString();
                    case "System.DateTime":
                        return ((DateTime)value).ToString();
                    default:
                        return "";
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
