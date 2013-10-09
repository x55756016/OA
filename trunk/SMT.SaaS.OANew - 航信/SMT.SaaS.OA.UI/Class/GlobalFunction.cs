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

using SMT.SaaS.OA.UI.EngineDataSource;
using System.Windows.Data;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.OA.UI.Class
{
    public class GlobalFunction
    {
        public static int CheckIsDecimal(string checkString)
        {
            if (string.IsNullOrEmpty(checkString.Trim()))
            {
                return -1;
            }
            foreach (char c in checkString.Trim())
            {
                if (char.IsNumber(c) || c == '.')
                {
                }
                else
                {
                    return -1;
                }
            }
            return 1;
        }

        public static int CheckIsDateTime(string checkString)
        {
            if (string.IsNullOrEmpty(checkString.Trim()))
            {
                return -1;
            }
            try
            {
                Convert.ToDateTime(checkString);
            }
            catch
            {
                return -1;
            }
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
                //Trace.WriteLog(ZFILE_NAME, cMessage);
                //return ReturnValue;
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

        public static void TextBoxInputDecimal(object sender, KeyEventArgs e)
        {
            int keyCode = e.PlatformKeyCode;
            if (keyCode < 48 || keyCode > 105 || keyCode > 57 && keyCode < 96 && keyCode != 190)
            {
                ((TextBox)sender).Text = "";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("IsDouble"));
            }
            if (!string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                if (!char.IsNumber(((TextBox)sender).Text, 0))
                {
                    ((TextBox)sender).Text = "";
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("ISNotAllowed"));
                }
            }
        }

        public static void TextBoxInputDecimal(object sender, KeyEventArgs e,string EntityName)
        {
            int keyCode = e.PlatformKeyCode;
            if (keyCode < 48 || keyCode > 105 || keyCode > 57 && keyCode < 96 && keyCode != 190)
            {
                ((TextBox)sender).Text = "";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("IsDouble", EntityName));
            }
            if (!string.IsNullOrEmpty(((TextBox)sender).Text))
            {
                if (!char.IsNumber(((TextBox)sender).Text, 0))
                {
                    ((TextBox)sender).Text = "";
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("ISNotAllowed"));
                }
            }
        }

        public static void TextBoxInputInt(object sender, KeyEventArgs e)
        {
            int keyCode = e.PlatformKeyCode;
            if (keyCode < 48 || keyCode > 105 || keyCode > 57 && keyCode < 96)
            {
                ((TextBox)sender).Text = "";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("IsDouble"));
            }
        }

        public static void TextBoxInputInt(object sender, KeyEventArgs e,string EntityName)
        {
            int keyCode = e.PlatformKeyCode;
            if (keyCode < 48 || keyCode > 105 || keyCode > 57 && keyCode < 96)
            {
                ((TextBox)sender).Text = "";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("IsDouble", EntityName));
            }
        }

        //public static void TextBoxInputOneNumber(object sender)
        //{
        //    string inputText = ((TextBox)sender).Text;
        //    if (inputText.Length != 1)
        //    {
        //        ((TextBox)sender).Text = "";
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(""), Utility.GetResourceStr("IsOneNumber"));
        //    }
        //}

        public static void TextBox_GotFocus(object sender, RoutedEventArgs e, int nWidth, int nHeight)
        {
            ((TextBox)sender).Width = nWidth;
            ((TextBox)sender).Height = nHeight;
        }

        public static void TextBox_LostFocus(object sender, RoutedEventArgs e, int nWidth, int nHeight)
        {
            ((TextBox)sender).Width = nWidth;
            ((TextBox)sender).Height = nHeight;
        }

        public static string GetCompanyNameByID(string companyID)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> dictc = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
            if (dictc == null)
                return "";
            var ent = (from q in dictc
                       where q.COMPANYID == companyID
                       select q.CNAME).FirstOrDefault();
            return ent == null ? "" : ent;
        }

        public static string GetDepartmentNameByID(string departmentID)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> dictc = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
            if (dictc == null)
            {
                return null;
            }
            var objc = from a in dictc
                       where a.DEPARTMENTID == departmentID
                       select a.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            return objc.Count() > 0 ? objc.FirstOrDefault() : null;
        }

        public static string GetPostNameByID(string postID)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> dictc = Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            if (dictc == null)
                return "";
            var ent = (from q in dictc
                       where q.POSTID == postID
                       select q.T_HR_POSTDICTIONARY.POSTNAME).FirstOrDefault();
            return ent == null ? "" : ent;
        }

        //public static string GetDepartmentNameByID(string departmentID)
        //{
        //    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> dictc = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
        //    if (dictc == null)
        //    {
        //        return null;
        //    }
        //    var objc = from a in dictc
        //               where a.DEPARTMENTID == departmentID
        //               select a.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //    return objc.Count() > 0 ? objc.FirstOrDefault() : null;
        //}

        public static SMT.SaaS.FrameworkUI.CheckStates GetCheckStateByValue(string value)
        {
            int a = Convert.ToInt32(value);

            SMT.SaaS.FrameworkUI.CheckStates state = (SMT.SaaS.FrameworkUI.CheckStates)a;

            //string name = System.Enum.GetName(typeof(SMT.SaaS.FrameworkUI.CheckStates), int.Parse(value));
            //switch(name)
            //{
            //    case "UnSubmit":
            //        return SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;                   
            //    case "Approved":
            //        return SMT.SaaS.FrameworkUI.CheckStates.Approved;
            //    case "Approving":
            //        return SMT.SaaS.FrameworkUI.CheckStates.Approving;
            //    case "UnApproved":
            //        return SMT.SaaS.FrameworkUI.CheckStates.UnApproved;
            //    case "WaittingApproval":
            //        return SMT.SaaS.FrameworkUI.CheckStates.Approving;
            //}
            return state;
            //int a = Convert.ToInt32
        }

        public static bool IsSaveAndClose(RefreshedTypes type)
        {
            return type == RefreshedTypes.CloseAndReloadData;
        }
    }
}