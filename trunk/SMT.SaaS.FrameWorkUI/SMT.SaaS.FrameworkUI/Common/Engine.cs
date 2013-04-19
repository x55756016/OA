using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Text;
using System.Reflection;
namespace SMT.SaaS.FrameworkUI.Common
{
    public class Engine
    {
        public static int ObjListToXml<T>(object oldDataObject, object newDataObject, ref string xmlString, ref string errorString, string nextUserId, string companyID, string currentUserID, string nFlowNode, string oFlowNode)
        {
            if (oldDataObject.GetType().BaseType.FullName.Substring(oldDataObject.GetType().BaseType.FullName.LastIndexOf('.') + 1) != "EntityObject")
            {
                errorString = "类不是规定的类型";
                return -1;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<table>");
            sb.AppendLine("<ApplicationSystem> " + "smtoasystem" + " </ApplicationSystem>");
            sb.AppendLine("<CompanyCode> " + companyID + " </CompanyCode> ");
            sb.AppendLine("<OperationUser> " + currentUserID + " </OperationUser>");
            string tableName = null;//表名
            PropertyInfo[] propinfos = null;//属性
            if (propinfos == null)
            {
                Type objtype = oldDataObject.GetType();
                propinfos = objtype.GetProperties();
                tableName = objtype.Name;
            }
            sb.AppendLine("<TableName> " + tableName + " </TableName>");
            PropertyInfo objectProperty = oldDataObject.GetType().GetProperty("EntityKey");
            object pKey = objectProperty.GetValue(oldDataObject, null);//主键
            object keyValue = pKey.GetType().GetProperty("EntityKeyValues").GetValue(pKey, null);
            System.Collections.ObjectModel.ObservableCollection<T> keyMember = keyValue as System.Collections.ObjectModel.ObservableCollection<T>;
            foreach (T t in keyMember)
            {
                string keyName = t.GetType().GetProperty("Key").GetValue(t, null).ToString();
                sb.AppendLine("<TableKey>");
                sb.Append("<TableKeyName>" + keyName + "</TableKeyName>");
                sb.Append("<TableKeyValue>" + GetString(oldDataObject.GetType().GetProperty(keyName).GetValue(oldDataObject, null)) + "</TableKeyValue>");
                sb.AppendLine("</TableKey>");
            }
            try
            {
                foreach (PropertyInfo propinfo in propinfos)
                {
                    if (propinfo.Name != "EntityState" && propinfo.Name != "EntityKey")
                    {
                        sb.AppendLine("<FieldString>");
                        sb.Append("<FieldName>" + propinfo.Name + "</FieldName>");
                        sb.Append("<Field_old_value>" + GetString(propinfo.GetValue(oldDataObject, null)) + "</Field_old_value>");
                        sb.Append("<Field_New_value>" + GetString(propinfo.GetValue(newDataObject, null)) + "</Field_New_value>");
                        sb.AppendLine("</FieldString>");
                    }
                    else
                    {
                        continue;
                    }
                }
                sb.AppendLine("<FieldString>");
                sb.Append("<FieldName>UserDefineNextUserCode</FieldName>");
                sb.Append("<Field_old_value>" + GetString(nextUserId) + "</Field_old_value>");
                sb.Append("<Field_New_value>" + GetString(nextUserId) + "</Field_New_value>");
                sb.AppendLine("</FieldString>");
                sb.AppendLine("<FieldString>");
                sb.Append("<FieldName>FlowNodeCode</FieldName>");
                sb.Append("<Field_old_value>" + oFlowNode + "</Field_old_value>");
                sb.Append("<Field_New_value>" + nFlowNode + "</Field_New_value>");
                sb.AppendLine("</FieldString>");
            }
            catch (Exception e)
            {
                string cMessage = "<HR>Message=[" + e.Message + "]" + "<HR>Source=[" + e.Data + "]<HR>StackTrace=[" + e.StackTrace + "]<HR>TargetSite=[" + e.InnerException + "]";
            }
            sb.AppendLine("</table>");
            xmlString = sb.ToString();
            return 1;
        }

        public static string GetString(object value)
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