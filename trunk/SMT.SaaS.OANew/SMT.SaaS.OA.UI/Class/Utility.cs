using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;
using System.Collections.ObjectModel;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.FlowDesignerWS;
using SMT.SAAS.Controls.Toolkit.Windows;
using System.Text.RegularExpressions;
using SMT.SAAS.ClientUtility;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI
{
    public class Utility
    {
        /// <summary>
        /// 加载字典
        /// Added by 安凯航 2011年7月15日
        /// </summary>
        static Utility()
        {
            DictionaryManager dm = new DictionaryManager();
            List<string> dicts = new List<string> 
            {
                "POSTLEVEL", 
                "CONSERVANAME",
                "MAINTENANCENAME",
                "FEESTYPE",

                "RECORDTYPE",
                "CHECKSTATE",
                "HIRERENTTYPE",
                "ORDERMEALSTATE",
                "LENDSTATE",
                "POSTLEVEL",
            };
            dm.LoadDictionary(dicts);
            dm.OnDictionaryLoadCompleted += (o, e) =>
            {
                return;
            };
        }

        public static string GetCheckState(CheckStates checkState)
        {
            return ((int)checkState).ToString();
        }
        /// <summary>
        /// 克隆对像
        /// </summary>
        /// <param name="source">要被克隆的对像</param>
        /// <returns>克隆的新对像</returns>
        public static object CloneObject(object source)
        {
            if (source == null)
            {
                return null;
            }

            string str = SerializeObject(source);
            object tmpObj = DeserializeObject(str, source.GetType());
            return tmpObj;
        }


        /// <summary>
        /// 序列化对像
        /// </summary>
        /// <param name="obj">要被序列化的对像</param>
        /// <returns>序列化字符串</returns>
        public static string SerializeObject(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(ms, obj);
                ms.Position = 0;

                using (StreamReader reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static object DeserializeObject(string objString, Type type)
        {

            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(type);

                return serializer.Deserialize(ms);
            }
        }
        public static object XmlToContractObject(string xml, Type type)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {
                DataContractSerializer dataContractSerializer = new DataContractSerializer(type);
                return dataContractSerializer.ReadObject(ms);
            }
        }

        /// <summary>
        ///CheckBox CBAll = sender as CheckBox;
        ///var grid = Utility.FindParentControl<DataGrid>(CBAll);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T FindParentControl<T>(DependencyObject item) where T : class
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                T parentGrid = parent as T;
                return (parentGrid != null) ? parentGrid : FindParentControl<T>(parent);
            }
            return null;
        }


        public static object FindParentControl(DependencyObject item)
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);

                return (parent != null) ? parent : FindParentControl(parent);
            }
            return null;
        }
        /// <summary>
        /// TreeViewItem myItem =(TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));
        ///CheckBox cbx = FindChildControl<CheckBox>(myItem);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindChildControl<T>(DependencyObject obj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return child as T;
                else
                {
                    T childOfChild = FindChildControl<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }


        /// <summary>
        /// TreeViewItem myItem =(TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));
        ///CheckBox cbx = FindChildControl<CheckBox>(myItem);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindChildControlToIsEnable<T>(DependencyObject obj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    
                }
                else
                {
                    T childOfChild = FindChildControlToIsEnable<T>(child);
                    if (childOfChild != null)
                    {

                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 审核或查看时,页面所有控件为关闭不可修改状态
        /// </summary>
        /// <param name="control">父控件</param>
        public static void  FindChildControlIsEnable(DependencyObject control) 
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(control); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                if ((child != null) && (child is Control))
                {
                    ((Control)child).IsEnabled = false;
                }
                else
                {
                    FindChildControlIsEnable(child);
                }
            }
           
        }

        public static T FindChildControl<T>(DependencyObject obj, string ctrName)
          where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {

                    object value = child.GetValue(Control.NameProperty);
                    if (value != null && value.ToString() == ctrName)
                    {
                        return child as T;
                    }
                    else
                    {
                        T childOfChild = FindChildControl<T>(child, ctrName);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
                else
                {
                    T childOfChild = FindChildControl<T>(child, ctrName);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public static void CbxItemBinder(ComboBox cbx, string category, string defalutValue)
        {
            if (cbx == null)
                return;
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var ents = dicts.Where(s => s.DICTIONCATEGORY == category).OrderBy(s => s.DICTIONARYVALUE);
            List<T_SYS_DICTIONARY> tempDicts = ents.ToList();

            if (ents == null)
            {
                return;
            }

            if (ents.Count() == 0)
            {
                return;
            }

            T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
            nuldict.DICTIONARYNAME = Utility.GetResourceStr("ALL");
            nuldict.DICTIONARYVALUE = 5;
            tempDicts.Insert(0, nuldict);

            cbx.ItemsSource = tempDicts;
            cbx.DisplayMemberPath = "DICTIONARYNAME";
            if (defalutValue != "")
            {
                foreach (var item in cbx.Items)
                {
                    T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DICTIONARYVALUE.ToString() == defalutValue)
                        {
                            cbx.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        public static void CbxItemBinders(ComboBox cbx, string category, string defalutValue)
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            if (dicts != null)
            {
                var ents = dicts.Where(s => s.DICTIONCATEGORY == category).OrderBy(s => s.DICTIONARYVALUE);
                cbx.ItemsSource = ents.ToList();
                cbx.DisplayMemberPath = "DICTIONARYNAME";
                if (defalutValue != "")
                {
                    foreach (var item in cbx.Items)
                    {
                        T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                        if (dict != null)
                        {
                            if (dict.DICTIONARYVALUE.ToString() == defalutValue)
                            {
                                cbx.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    if (cbx.SelectedItem == null && (category != "COUNTYTYPE" && category != "CITY"))
                    {
                        //考虑不选的情况  -1
                        //cbx.SelectedIndex = 0;
                        cbx.SelectedIndex = Convert.ToInt32(defalutValue);
                    }
                }
                else
                {
                    cbx.SelectedIndex = 0;
                }
            }

        }

        /// <summary>
        /// 根据父级ID绑定字典值
        /// </summary>
        /// <param name="cbx">要绑定的ComboBox</param>
        /// <param name="category">类型</param>
        /// <param name="defalutValue">默认值</param>
        /// <param name="strFatherID">父级ID</param>
        public static void CbxItemBinder(ComboBox cbx, string category, string defalutValue, string strFatherID)
        {

            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var ents = dicts.Where(s => s.DICTIONCATEGORY == category && (s.T_SYS_DICTIONARY2 != null && s.T_SYS_DICTIONARY2.DICTIONARYID == strFatherID)).OrderBy(s => s.DICTIONARYVALUE);
            cbx.ItemsSource = ents.ToList();
            cbx.DisplayMemberPath = "DICTIONARYNAME";
            if (defalutValue != "")
            {
                foreach (var item in cbx.Items)
                {
                    T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DICTIONARYVALUE.ToString() == defalutValue)
                        {
                            cbx.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }
        public static void BindComboBox(ComboBox cbx, string displayMember, int defalutIndex)
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var ents = dicts.Where(s => s.DICTIONCATEGORY == displayMember).OrderBy(s => s.DICTIONARYVALUE);
            cbx.ItemsSource = ents.ToList();
            cbx.DisplayMemberPath = "DICTIONARYNAME";
            cbx.SelectedIndex = defalutIndex;
        }

        public static void SetComboboxSelectByValue(ComboBox cbx, string defalutValue, int defalutIndex)
        {
            if (cbx.Items == null) return;
            foreach (var item in cbx.Items)
            {
                T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                if (dict != null)
                {
                    if (dict.DICTIONARYVALUE.ToString() == defalutValue)
                    {
                        cbx.SelectedItem = item;
                        return;
                    }
                }
            }
            cbx.SelectedIndex = defalutIndex;
        }

        public static void SetComboboxSelectByText(ComboBox cbx, string selectText, int defalutIndex)
        {
            if (cbx.Items == null) return;
            foreach (var item in cbx.Items)
            {
                T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                if (dict != null)
                {
                    if (dict.DICTIONARYNAME.ToString() == selectText)
                    {
                        cbx.SelectedItem = item;
                        return;
                    }
                }
            }
            cbx.SelectedIndex = defalutIndex;
        }


        public static string GetDictionaryValue(string category, string value)
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var ents = dicts.Where(s => s.DICTIONCATEGORY == category && s.DICTIONARYVALUE == decimal.Parse(value));
            return ents.Count() > 0 ? ents.ToList()[0].DICTIONARYNAME : value;
        }

        public static string GetCbxSelectItemValue(ComboBox cbx)
        {
            if (cbx.SelectedItem == null) return "";
            T_SYS_DICTIONARY item = cbx.SelectedItem as T_SYS_DICTIONARY;
            return item.DICTIONARYVALUE.ToString();
        }

        public static string GetCbxSelectItemText(ComboBox cbx)
        {
            if (cbx.SelectedItem == null) return "";
            T_SYS_DICTIONARY item = cbx.SelectedItem as T_SYS_DICTIONARY;
            return item.DICTIONARYNAME.ToString();
        }

        /// <summary>
        /// 显示错误提示框信息
        /// </summary>
        /// <param name="messageType">信息类型</param>
        /// <param name="title">标题</param>
        /// <param name="message">错误内容</param>
        public static void ShowCustomMessage(MessageTypes messageType, string title, string message)
        {
            string _text = "";
            if (messageType == MessageTypes.Error)
            {
                MessageWindow.Show<string>(title, message, MessageIcon.Error, result => _text = result, "Default", Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else if (messageType == MessageTypes.Caution)
            {
                MessageWindow.Show<string>(title, message, MessageIcon.Exclamation, result => _text = result, "Default", Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                MessageWindow.Show<string>(title, message, MessageIcon.Information, result => _text = result, "Default", Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        public static string GetResourceStr(string message)
        {
            string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(message, SMT.SaaS.Globalization.Localization.UiCulture);
            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }

        public static string GetResourceStr(string message, string parameter)
        {
            string rslt = SMT.SaaS.Globalization.Localization.GetString(message, parameter);

            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }

        public static string GetCompanyName(string companyID)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> dicts = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
            var ents = dicts.Where(s => s.COMPANYID == companyID);
            return ents.Count() > 0 ? ents.ToList()[0].CNAME : companyID;
        }

        public static string GetGetCompanyID(string companyName)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> dicts = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
            var ents = dicts.Where(s => s.CNAME == companyName);
            return ents.Count() > 0 ? ents.FirstOrDefault().COMPANYID : companyName;
        }

        public static string GetDepartmentName(string departmentID)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> dicts = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
            var ents = dicts.Where(s => s.DEPARTMENTID == departmentID);
            return ents.Count() > 0 ? ents.ToList()[0].T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME : departmentID;
        }

        public static string GetDepartmentID(string departmentName)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> dicts = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
            var ents = dicts.Where(s => s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME == departmentName);
            return ents.Count() > 0 ? ents.FirstOrDefault().DEPARTMENTID : departmentName;
        }

        public static string GetPostName(string postID)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> dicts = Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            var ents = dicts.Where(s => s.POSTID == postID);
            return ents.Count() > 0 ? ents.ToList()[0].T_HR_POSTDICTIONARY.POSTNAME : postID;
        }

        public static string GetPostID(string postName)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> dicts = Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            var ents = dicts.Where(s => s.T_HR_POSTDICTIONARY.POSTNAME == postName);
            return ents.Count() > 0 ? ents.FirstOrDefault().POSTID : postName;
        }
        public static string GetMododuelName(string ModeCode)
        {
            List<SMT.Saas.Tools.FlowDesignerWS.FLOW_MODELDEFINE_T> dictc = Application.Current.Resources["FLOW_MODELDEFINE_T"] as List<SMT.Saas.Tools.FlowDesignerWS.FLOW_MODELDEFINE_T>;
            if (dictc == null)
                return ModeCode;
            var objc = from a in dictc
                       where a.MODELCODE == ModeCode
                       select a.DESCRIPTION;
            return objc.Count() > 0 ? objc.FirstOrDefault() : ModeCode;
        }
        public static string GetPostLevle(string postLevle)
        {
            string strDictioncategory = "POSTLEVEL";
            decimal dPostLevle = 0;
            decimal.TryParse(postLevle, out dPostLevle);
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var ents = dicts.Where(s => s.DICTIONCATEGORY == strDictioncategory && s.DICTIONARYVALUE == dPostLevle);
            return ents.Count() > 0 ? ents.ToList()[0].DICTIONARYNAME : postLevle;
        }
        /// <summary>
        /// 获取发布员工的姓名 2010-7-8 
        /// </summary>
        /// <param name="StrEmployeeID"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDistrbuteUserName(string StrEmployeeID, List<V_EMPLOYEEPOST> obj)
        {
            if (obj.Count > 0)
            {
                var ents = obj.Where(s => s.T_HR_EMPLOYEE.EMPLOYEEID == StrEmployeeID);
                return ents.Count() > 0 ? ents.ToList()[0].T_HR_EMPLOYEE.EMPLOYEECNAME : "";
            }
            return "";



        }
        //public static void GetLoginInfo<T>(T obj) where T : class
        //{
        //    Type a = Common.CurrentLoginUserInfo.GetType();
        //    PropertyInfo[] infos = a.GetProperties();
        //    foreach (PropertyInfo prop in infos)
        //    {
        //        object value = prop.GetValue(Common.CurrentLoginUserInfo, null);
        //        try
        //        {
        //            prop.SetValue(obj, value.ToString(), null);
        //        }
        //        catch (Exception ex)
        //        {
        //            string e = ex.Message;
        //        }
        //    }
        //}

        public static void GetLoginUserInfo(object obj)
        {
            PropertyInfo[] objectProperty = obj.GetType().GetProperties();
            PropertyInfo[] sourceProperty = Common.CurrentLoginUserInfo.GetType().GetProperties();
            foreach (var h in objectProperty)
            {
                foreach (var j in sourceProperty)
                {
                    if (h.Name == j.Name)
                    {
                        h.SetValue(obj, j.GetValue(Common.CurrentLoginUserInfo, null), null);
                    }
                }
            }
        }


        ///// <summary>
        ///// 显示DataGrid上面通用按钮
        ///// </summary>
        ///// <param name="toolBar">所属工具条</param>
        ///// <param name="entityName">表名称</param>
        ///// <param name="displayAuditButton">是示有审核按钮</param>
        //public static void DisplayGridToolBarButton(FormToolBar toolBar, string entityName, bool displayAuditButton)
        //{
        //    //查看 去掉了=，因为集团的范围为0  2010-5-25 liujx
        //    if (PermissionHelper.GetPermissionValue(entityName, Permissions.Browse) < 0)
        //    {
        //        //MessageBox.Show(Utility.GetResourceStr("NOPERMISSION"));
        //        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSION"));
        //        Uri uri = new Uri("/Home", UriKind.Relative);

        //        //取当前主页
        //        (App.MainPage as MainPage).ContentFrame.Navigate(uri);


        //    }
        //    //添加
        //    if (PermissionHelper.GetPermissionValue(entityName, Permissions.Add) < 0)
        //    {
        //        toolBar.btnNew.Visibility = Visibility.Collapsed;
        //        toolBar.retNew.Visibility = Visibility.Collapsed;
        //    }
        //    //修改
        //    if (PermissionHelper.GetPermissionValue(entityName, Permissions.Edit) < 0)
        //    {
        //        toolBar.btnEdit.Visibility = Visibility.Collapsed;
        //        toolBar.retEdit.Visibility = Visibility.Collapsed;
        //    }
        //    //删除
        //    if (PermissionHelper.GetPermissionValue(entityName, Permissions.Delete) < 0)
        //    {
        //        toolBar.btnDelete.Visibility = Visibility.Collapsed;
        //    }

        //    if (displayAuditButton)
        //    {
        //        //审核
        //        if (PermissionHelper.GetPermissionValue(entityName, Permissions.Audit) < 0)
        //        {
        //            toolBar.btnAudit.Visibility = Visibility.Collapsed;
        //            toolBar.retAudit.Visibility = Visibility.Collapsed;

        //        }
        //    }
        //    else
        //    {
        //        toolBar.btnAudit.Visibility = Visibility.Collapsed;
        //        toolBar.retAudit.Visibility = Visibility.Collapsed;


        //        toolBar.stpCheckState.Visibility = Visibility.Collapsed;
        //    }
        //}


        /// <summary>
        /// 显示DataGrid上面通用按钮
        /// </summary>
        /// <param name="toolBar">所属工具条</param>
        /// <param name="entityName">表名称</param>
        /// <param name="displayAuditButton">是示有审核按钮</param>
        public static void DisplayGridToolBarButton(FormToolBar toolBar, string entityName, bool displayAuditButton)
        {
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Browse) < 0)
            {
                MessageBox.Show(Utility.GetResourceStr("NOPERMISSION"));
                Uri uri = new Uri("/Home", UriKind.Relative);

                //取当前主页
                //Grid grid = Application.Current.RootVisual as Grid;
                SMT.SaaS.OA.UI.App.EntryPointPage MainPage = Application.Current.RootVisual as SMT.SaaS.OA.UI.App.EntryPointPage;
                Grid grid = MainPage.Content as Grid;

                if (grid != null && grid.Children.Count > 0)
                {
                    MainPage page = grid.Children[0] as MainPage;
                    if (page != null)
                    {
                        page.NavigateTo(uri);
                    }
                }
                return;

            }
            //添加
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Add) < 0)
            {
                toolBar.btnNew.Visibility = Visibility.Collapsed;
                toolBar.retNew.Visibility = Visibility.Collapsed;
            }
            //修改
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Edit) < 0)
            {
                toolBar.btnEdit.Visibility = Visibility.Collapsed;
                toolBar.retEdit.Visibility = Visibility.Collapsed;
            }
            //删除
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Delete) < 0)
            {
                toolBar.btnDelete.Visibility = Visibility.Collapsed;
            }

          

            if (displayAuditButton)
            {
                //审核
                if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Audit) < 0)
                {
                    toolBar.btnAudit.Visibility = Visibility.Collapsed;
                    toolBar.retAudit.Visibility = Visibility.Collapsed;
                    //toolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                toolBar.btnAudit.Visibility = Visibility.Collapsed;
                toolBar.retAudit.Visibility = Visibility.Collapsed;
                //toolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
                //toolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
                toolBar.stpCheckState.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 显示DataGrid上面通用按钮
        /// </summary>
        /// <param name="toolBar">所属工具条</param>
        /// <param name="entityName">表名称</param>
        /// <param name="displayAuditButton">是示有审核按钮</param>
        public static void DisplayGridToolBarButtonUI(FormToolBar toolBar, string entityName, bool displayAuditButton)
        {
            //查看 去掉了=，因为集团的范围为0  2010-5-25 liujx
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Browse) < 0)
            {
                MessageBox.Show(Utility.GetResourceStr("NOPERMISSION"));
                Uri uri = new Uri("/Home", UriKind.Relative);

                //取当前主页
                //Grid grid = Application.Current.RootVisual as Grid;
                SMT.SaaS.OA.UI.App.EntryPointPage MainPage = Application.Current.RootVisual as SMT.SaaS.OA.UI.App.EntryPointPage;
                Grid grid = MainPage.Content as Grid;

                if (grid != null && grid.Children.Count > 0)
                {
                    MainPage page = grid.Children[0] as MainPage;
                    if (page != null)
                    {
                        page.NavigateTo(uri);
                    }
                }
                return;

            }
            //添加
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Add) < 0)
            {
                toolBar.btnNew.Visibility = Visibility.Collapsed;
                toolBar.retNew.Visibility = Visibility.Collapsed;
            }
            //修改
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Edit) < 0)
            {
                toolBar.btnEdit.Visibility = Visibility.Collapsed;
                toolBar.retEdit.Visibility = Visibility.Collapsed;
            }
            //删除
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Delete) < 0)
            {
                toolBar.btnDelete.Visibility = Visibility.Collapsed;
            }

            if (displayAuditButton)
            {
                //审核
                if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Audit) < 0)
                {
                    toolBar.btnAudit.Visibility = Visibility.Collapsed;
                    toolBar.retAudit.Visibility = Visibility.Collapsed;
                    //toolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                toolBar.btnAudit.Visibility = Visibility.Collapsed;
                toolBar.retAudit.Visibility = Visibility.Collapsed;
                //toolBar.btnAduitNoTPass.Visibility = Visibility.Collapsed;
                //toolBar.btnSumbitAudit.Visibility = Visibility.Collapsed;
                toolBar.stpCheckState.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// 生成Form上的保存，保存并关闭按钮
        /// </summary>
        /// <returns>按钮列表</returns>
        public static List<ToolbarItem> CreateFormSaveButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = GetResourceStr("SAVE"),// "保存",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = GetResourceStr("SAVEANDCLOSE"),//"保存与关闭"
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            return items;
        }

        /// <summary>
        /// 生成Form上的保存，保存并关闭按钮
        /// </summary>
        /// <returns>按钮列表</returns>
        public static List<ToolbarItem> CreateFormSaveButton(string menucode, string userid, string postid, string departid, string comapnyid)
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            //如果有修改权限才允许保存
            if (PermissionHelper.GetPermissionValue(menucode, Permissions.Edit, userid, postid, departid, comapnyid) >= 0)
            {
                items = CreateFormSaveButton();
            }
            return items;
        }

        /// <summary>
        /// 生成Form上的编辑，保存并关闭按钮
        /// </summary>
        /// <returns>按钮列表2</returns>
        public static List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = GetResourceStr("SAVE"),// "保存",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = GetResourceStr("SAVEANDCLOSE"),//"保存与关闭"
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "2",
                Title = GetResourceStr("SUBMITAUDIT"),//"提交审核"
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            };

            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "3",
                Title = GetResourceStr("AUDIT"),//"审核"
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            };

            items.Add(item);
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "4",
                Title = GetResourceStr("AUDITNOTPASS"),//"审核不通过"
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png"
            };

            items.Add(item);

            return items;
        }

        /// <summary>
        /// 生成Form上的编辑，保存并关闭按钮
        /// </summary>
        /// <returns>按钮列表</returns>
        public static List<ToolbarItem> CreateFormEditButton(string menucode, string userid, string postid, string departid, string comapnyid)
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            //如果有修改权限才允许保存
            if (PermissionHelper.GetPermissionValue(menucode, Permissions.Edit, userid, postid, departid, comapnyid) >= 0)
            {
                items = CreateFormSaveButton();
            }
            return items;
        }


        //public static void BindComboBox(ComboBox cbx, List<T_SYS_DICTIONARY> dicts, string category, string defaultValue)
        //{
        //    var objs = from d in dicts
        //               where d.DICTIONCATEGORY == category
        //               select d;
        //    List<T_SYS_DICTIONARY> tmpDicts = objs.ToList();

        //    T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
        //    nuldict.DICTIONARYNAME = GetResourceStr("NULL");
        //    nuldict.DICTIONARYVALUE = "";
        //    tmpDicts.Insert(0, nuldict);

        //    cbx.ItemsSource = tmpDicts;
        //    cbx.DisplayMemberPath = "DICTIONARYNAME";


        //    foreach (var item in cbx.Items)
        //    {
        //        T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
        //        if (dict != null)
        //        {
        //            if (dict.DICTIONARYVALUE == defaultValue)
        //            {
        //                cbx.SelectedItem = item;
        //                break;
        //            }
        //        }
        //    }

        //}
        //public static void BindComboBox(ComboBox cbx, List<T_SYS_DICTIONARY> dicts,string defaultValue)
        //{

        //    cbx.ItemsSource = dicts;
        //    cbx.DisplayMemberPath = "DICTIONARYNAME";

        //    T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
        //    nuldict.DICTIONARYNAME = GetResourceStr("NULL");
        //    nuldict.DICTIONARYVALUE = "";

        //    dicts.Insert(0, nuldict);

        //    foreach (var item in cbx.Items)
        //    {
        //        T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
        //        if (dict != null)
        //        {
        //            if (dict.DICTIONARYVALUE == defaultValue)
        //            {
        //                cbx.SelectedItem = item;
        //                break;
        //            }
        //        }
        //    }

        //}

        public static void ShowMessageBox(string actionString, bool isFlowFlag, bool isSuccess)
        {
            string successString = "FAILED";
            if (isSuccess)
            {
                successString = "SUCCESSED";
            }
            if (isFlowFlag)
            {
                actionString = "SUBMIT";
            }
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(successString), Utility.GetResourceStr(actionString + successString, ""));
        }

        private static string FormID;
        private static string FormName;
        private static string FormType;

        /// <summary>
        /// 门户打开单据方法，通过引擎传入的ApplicationURL 
        /// </summary>
        /// <param name="StrFormID">ApplicationOrder</param>
        /// <param name="StrFormName">PageParameter</param>
        /// <param name="StrFormType">Edit</param>
        public static void CreateFormFromEngine(string StrFormID, string StrFormName, string StrFormType)
        {
            //<?xml version="1.0" encoding="utf-8" ?><System><AssemblyName>SMT.SaaS.OA.UI</AssemblyName>
            //<PublicClass>SMT.SaaS.OA.UI.Utility</PublicClass>
            //<ProcessName>CreateFormFromEngine</ProcessName>
            //<PageParameter>SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm</PageParameter>
            //<ApplicationOrder>84c9919b-3265-4e79-bf9a-f4afe4b8f0fe</ApplicationOrder>
            //<FormTypes>Edit</FormTypes></System>";
            FormID = StrFormID;
            FormName = StrFormName;
            FormType = StrFormType;

            ChecResource();
            FormTypes CurrentAction = FormTypes.Audit;
            if (!string.IsNullOrEmpty(FormType))
            {
                switch (FormType.ToUpper())
                {
                    case "AUDIT":
                        CurrentAction = FormTypes.Audit;
                        break;
                    case "ADD":
                        CurrentAction = FormTypes.New;
                        break;
                    case "EDIT":
                        CurrentAction = FormTypes.Edit;
                        break;
                    case "VIEW":
                        CurrentAction = FormTypes.Browse;
                        break;
                    case "RESUBMIT":
                        CurrentAction = FormTypes.Resubmit;
                        break;

                }
            }

            Type t = Type.GetType(FormName);
            Object[] parameters = new Object[2];    // 定义构造函数需要的参数
            parameters[0] = CurrentAction;
            parameters[1] = FormID;

            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                entBrowser.FormType = CurrentAction;
                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm"
                    && CurrentAction!=FormTypes.Resubmit)
                {
                    entBrowser.EntityBrowseToolBar.MaxHeight = 0;
                }
                entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, FormID);
                //MessageBox.Show(entBrowser.FormType.ToString()+"  "+ FormName+" id:"+FormID);
                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                {
                    WindowsManager.MaxWindow(entBrowser.ParentWindow);                  
                }
                else
                {
                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 900;
                }
            }
        }


        /// <summary>
        /// 门户创建待审批的单据（通过引擎）IsKpi==true时 2010-6-24
        /// </summary>
        /// <param name="StrFormID">formid</param>
        /// <param name="StrFormName">formname</param>
        /// <param name="StrFormType">formtype</param>
        /// <param name="parent"></param>
        public static void CreateFormFromEngine(string StrFormID, string StrFormName, string StrFormType, Border parent)
        {
            try
            {
                FormID = StrFormID;
                FormName = StrFormName;
                FormType = StrFormType;

                ChecResource();
                FormTypes CurrentAction = FormTypes.Audit;
                if (!string.IsNullOrEmpty(FormType))
                {
                    switch (FormType.ToUpper())
                    {
                        case "AUDIT":
                            CurrentAction = FormTypes.Audit;
                            break;
                        case "ADD":
                            CurrentAction = FormTypes.New;
                            break;
                        case "EDIT":
                            CurrentAction = FormTypes.Edit;
                            break;
                        case "VIEW":
                            CurrentAction = FormTypes.Browse;
                            break;
                        case "RESUBMIT":
                            CurrentAction = FormTypes.Resubmit;
                            break;

                    }
                }

                Type t = Type.GetType(FormName);
                Object[] parameters = new Object[2];// 定义构造函数需要的参数
                parameters[0] = CurrentAction;
                parameters[1] = FormID;
                
                object form = Activator.CreateInstance(t, parameters);
                
                if (form != null)
                {
                    EntityBrowser entBrowser = new EntityBrowser(form);
                    if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                    {
                        entBrowser.EntityBrowseToolBar.MaxHeight = 0;
                    }
                    if (entBrowser != null)
                    {
                        entBrowser.FormType = CurrentAction;
                        if (parent != null)
                        {
                            parent.Child = entBrowser;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }


        /// <summary>
        /// 门户创建待审批的单据（通过引擎） 2012-8-3
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="FormName"></param>
        public static void CreateFormFromMvcPlat(string StrFormID, string StrFormName, string StrFormType)
        {
            FormID = StrFormID;
            FormName = StrFormName;
            FormType = StrFormType;

            ChecResource();
            FormTypes CurrentAction = FormTypes.Audit;
            if (!string.IsNullOrEmpty(FormType))
            {
                switch (FormType.ToUpper())
                {
                    case "AUDIT":
                        CurrentAction = FormTypes.Audit;
                        break;
                    case "ADD":
                        CurrentAction = FormTypes.New;
                        break;
                    case "EDIT":
                        CurrentAction = FormTypes.Edit;
                        break;
                    case "VIEW":
                        CurrentAction = FormTypes.Browse;
                        break;
                    case "RESUBMIT":
                        CurrentAction = FormTypes.Resubmit;
                        break;

                }
            }

            Type t = Type.GetType(FormName);
            Object[] parameters = new Object[2];    // 定义构造函数需要的参数
            parameters[0] = CurrentAction;
            parameters[1] = FormID;

            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                entBrowser.FormType = CurrentAction;
                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm"
                    && CurrentAction != FormTypes.Resubmit)
                {
                    entBrowser.EntityBrowseToolBar.MaxHeight = 0;
                }
                
                entBrowser.ShowMvcPlat<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

                //entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, FormID);
                //MessageBox.Show(entBrowser.FormType.ToString()+"  "+ FormName+" id:"+FormID);
                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                {
                    WindowsManager.MaxWindow(entBrowser.ParentWindow);
                }
                else
                {
                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 900;
                }
            }
        }


        /// <summary>
        /// MVC工作计划创建出差报销
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="FormName"></param>
        public static void CreateFormFromMVC(string StrFormID, string StrFormName, string StrFormType)
        {
            //<?xml version="1.0" encoding="utf-8" ?><System><AssemblyName>SMT.SaaS.OA.UI</AssemblyName>
            //<PublicClass>SMT.SaaS.OA.UI.Utility</PublicClass>
            //<ProcessName>CreateFormFromEngine</ProcessName>
            //<PageParameter>SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm</PageParameter>
            //<ApplicationOrder>84c9919b-3265-4e79-bf9a-f4afe4b8f0fe</ApplicationOrder>
            //<FormTypes>Edit</FormTypes></System>";
            FormID = StrFormID;
            FormName = StrFormName;
            FormType = StrFormType;

            ChecResource();
            FormTypes CurrentAction = FormTypes.Audit;
            if (!string.IsNullOrEmpty(FormType))
            {
                switch (FormType.ToUpper())
                {
                    case "AUDIT":
                        CurrentAction = FormTypes.Audit;
                        break;
                    case "ADD":
                        CurrentAction = FormTypes.New;
                        break;
                    case "EDIT":
                        CurrentAction = FormTypes.Edit;
                        break;
                    case "VIEW":
                        CurrentAction = FormTypes.Browse;
                        break;
                    case "RESUBMIT":
                        CurrentAction = FormTypes.Resubmit;
                        break;

                }
            }

            Type t = Type.GetType(FormName);
            Object[] parameters = new Object[3];    // 定义构造函数需要的参数
            parameters[0] = CurrentAction;
            parameters[1] = FormID;
            parameters[2] = "FromMVC";

            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                entBrowser.FormType = CurrentAction;
                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm"
                    && CurrentAction != FormTypes.Resubmit)
                {
                    entBrowser.EntityBrowseToolBar.MaxHeight = 0;
                }
                entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, FormID);
                //MessageBox.Show(entBrowser.FormType.ToString()+"  "+ FormName+" id:"+FormID);
                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                {
                    WindowsManager.MaxWindow(entBrowser.ParentWindow);
                }
                else
                {
                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 900;
                }
            }
        }


        private static void ChecResource()
        {
            if (Application.Current.Resources["FLOW_MODELDEFINE_T"] == null)
            {
                Utility.LoadDictss();
            }
            if (!Application.Current.Resources.Contains("CustomDateConverter"))
            {
                Application.Current.Resources.Add("CustomDateConverter", new SMT.SaaS.OA.UI.CustomDateConverter());
            }
            if (!Application.Current.Resources.Contains("ResourceConveter"))
            {
                Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            }
            if (!Application.Current.Resources.Contains("DictionaryConverter"))
            {
                Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.OA.UI.DictionaryConverter());
            }
            if (!Application.Current.Resources.Contains("StateConvert"))
            {
                Application.Current.Resources.Add("StateConvert", new SMT.SaaS.OA.UI.CheckStateConverter());
            }
            if (!Application.Current.Resources.Contains("ModuleNameConverter"))
            {
                Application.Current.Resources.Add("ModuleNameConverter", new SMT.SaaS.OA.UI.ModuleNameConverter());
            }
        }


        /// <summary>
        /// 加载权限
        /// </summary>
        /// <param name="FormName"></param>
        private static void CheckPermission(string FormName)
        {
            PermissionServiceClient clientPerm = new PermissionServiceClient();

            clientPerm.GetUserPermissionByUserToUICompleted += new EventHandler<GetUserPermissionByUserToUICompletedEventArgs>(clientPerm_GetUserPermissionByUserToUICompleted);

            string strUserId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID;
            //ApplicationCache.CacheMenuList
            clientPerm.GetUserPermissionByUserToUIAsync(strUserId);

        }
        static void clientPerm_GetUserPermissionByUserToUICompleted(object sender, GetUserPermissionByUserToUICompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }
                List<V_UserPermissionUI> entPermList = e.Result.ToList();

                foreach (var fent in entPermList)
                {
                    //SMT.SAAS.Main.CurrentContext.Entity.V_UserPermissionUI tps = new SMT.SAAS.Main.CurrentContext.Entity.V_UserPermissionUI();
                    SMT.SaaS.LocalData.V_UserPermissionUI tps = new LocalData.V_UserPermissionUI();
                    tps = Common.CloneObject<V_UserPermissionUI, SMT.SaaS.LocalData.V_UserPermissionUI>(fent, tps);
                    Common.CurrentLoginUserInfo.PermissionInfoUI.Add(tps);
                }
                ShowForm();
            }
        }
        /// <summary>
        /// 打开Form表单
        /// </summary>
        private static void ShowForm()
        {
            FormTypes CurrentAction = FormTypes.Audit;
            if (!string.IsNullOrEmpty(FormType))
            {
                switch (FormType.ToUpper())
                {
                    case "AUDIT":
                        CurrentAction = FormTypes.Audit;
                        break;
                    case "ADD":
                        CurrentAction = FormTypes.New;
                        break;
                    case "EDIT":
                        CurrentAction = FormTypes.Edit;
                        break;
                    case "VIEW":
                        CurrentAction = FormTypes.Browse;
                        break;
                }
            }

            Type t = Type.GetType(FormName);
            // SMT.SaaS.OA.UI.UserControls..BusinessApplicationsForm
            Object[] parameters = new Object[2];    // 定义构造函数需要的参数
            parameters[0] = CurrentAction;
            parameters[1] = FormID;// "5d572f2d-c0e4-49ca-960e-6bd45bfb97a9";

            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                entBrowser.FormType = CurrentAction;
                //entBrowser.TitleContent = "出差申请";
                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                {
                    entBrowser.EntityBrowseToolBar.MaxHeight = 0;
                }
                entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, FormID);

                if (FormName == "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm")
                {
                    WindowsManager.MaxWindow(entBrowser.ParentWindow);
                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 1145;
                }
                else
                {
                    entBrowser.ParentWindow.Height = 900;
                    entBrowser.ParentWindow.Width = 900;
                }
            }
        }

        /// <summary>
        /// 获取发布对象的 ID  2010-7-28
        /// </summary>
        /// <param name="issuanceExtOrgObj"></param>
        /// <returns></returns>
        public static string ReturnIssuranceObjID(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj)
        {
            string StrReturn = "";
            switch (issuanceExtOrgObj.ObjectInstance.GetType().Name)
            {
                case "T_HR_EMPLOYEE"://发布类型为员工
                    SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE tmpEmployee = new SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE();
                    tmpEmployee = (SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE)issuanceExtOrgObj.ObjectInstance;
                    StrReturn = tmpEmployee.EMPLOYEEID;
                    break;
                case "T_HR_POST"://发布类型为 岗位，目前暂时不考虑 
                    break;
                case "T_HR_DEPARTMENT"://发布类型为部门
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDepartment = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                    tmpDepartment = (SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)issuanceExtOrgObj.ObjectInstance;
                    StrReturn = tmpDepartment.DEPARTMENTID;
                    break;
                case "T_HR_COMPANY"://发布类型为公司
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpCompany = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                    tmpCompany = (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)issuanceExtOrgObj.ObjectInstance;
                    StrReturn = tmpCompany.COMPANYID;

                    break;

            }
            return StrReturn;
        }
        public static void SetAuditEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity, string modelcode, string formid, string strXmlObjectSource)
        {

            entity.ModelCode = modelcode;//"archivesLending";T_HR_COMPANY
            entity.FormID = formid; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
            entity.CreateCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;// "7cd6c0a4-9735-476a-9184-103b962d3383";
            entity.CreateDepartmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entity.CreatePostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entity.CreateUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.XmlObject = strXmlObjectSource;
        }

        public static void SetAuditEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity, string modelcode, string formid)
        {

            entity.ModelCode = modelcode;//"archivesLending";T_HR_COMPANY
            entity.FormID = formid; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
            entity.CreateCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entity.CreateDepartmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entity.CreatePostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entity.CreateUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            //entity.XmlObject = strXmlObjectSource;
        }
        /// <summary>
        /// 代提单时设置流程实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modelcode"></param>
        /// <param name="formid"></param>
        /// <param name="strXmlObjectSource"></param>
        /// <param name="paras">被代提单的员工岗位，部门，公司ID集合</param>
        public static void SetAuditEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity, string modelcode, string formid, string strXmlObjectSource, Dictionary<string, string> paras)
        {
            entity.ModelCode = modelcode;//"archivesLending";T_HR_COMPANY
            entity.FormID = formid; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
            entity.CreateCompanyID = paras["CreateCompanyID"];
            entity.CreateDepartmentID = paras["CreateDepartmentID"];
            entity.CreatePostID = paras["CreatePostID"];
            entity.CreateUserID = paras["CreateUserID"];

            entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.XmlObject = strXmlObjectSource;
        }

        /// <summary>
        /// 引擎需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, string SystemCode)
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
            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();

        }

        public static string ObjListToXmlForTravel<T>(T objectdata, string SystemCode, Dictionary<string, string> parameters)
        {
            if (parameters == null)
                return "";
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
            foreach (var q in parameters)
            {
                sb.AppendLine("<Attribute Name=\"" + q.Key + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + q.Value + "\"/>");
            }
            string BorrowMoney = "0";
            if (parameters.GetObjValue("BorrowMoney") != null)
            {
                BorrowMoney = parameters.GetObjValue("BorrowMoney").ToString();
            }

            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
        }

        /// <summary>
        /// 引擎需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="Prameters">当需要传递不在当前实体中的属性值时使用</param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode)
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
            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
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

        public static void LoadDictss()
        {
            ServiceClient FlowDesigner = new ServiceClient();
            FlowDesigner.GetModelNameInfosComboxCompleted += (o, e) =>
            {
                List<FLOW_MODELDEFINE_T> dicts = new List<FLOW_MODELDEFINE_T>();
                dicts = e.Result == null ? null : e.Result.ToList();
                if (Application.Current.Resources["FLOW_MODELDEFINE_T"] == null)
                {
                    Application.Current.Resources.Add("FLOW_MODELDEFINE_T", dicts);
                }
            };
            //TODO: 获取模块
            FlowDesigner.GetModelNameInfosComboxAsync();
        }

        public static void DataRowAddRowNo(object sender, DataGridRowEventArgs e)
        {
            DataGrid dgEmployeeSurvey = sender as DataGrid;
            int index = e.Row.GetIndex();
            var cell = dgEmployeeSurvey.Columns[1].GetCellContent(e.Row) as TextBlock;
            if (cell != null)
            {
                cell.HorizontalAlignment = HorizontalAlignment.Center;
                cell.VerticalAlignment = VerticalAlignment.Center;
                cell.Text = (index + 1).ToString();
            }
        }

        /// <summary>
        /// 返回字符串信息，如果为空或null则为“”
        /// </summary>
        /// <param name="StrValue"></param>
        /// <returns></returns>
        public static string GetEntityFiledValue(string StrValue)
        {
            if (string.IsNullOrEmpty(StrValue))
            {
                return "";
            }
            return StrValue;
        }
        /// <summary>
        /// 判断输入的是否是数字 采用正则表达式
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsInt(string source)
        {

            Regex regex = new Regex(@"^[0-9]+(.[0-9]{1,2})?$");
            if (regex.Match(source).Success)
            {
                return true;
            }
            return false;
        }

        #region 按照移动版要求生成XML

        /// <summary>
        /// 流程最新元数据需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="Prameters">当需要传递不在当前实体中的属性值时使用</param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode, Dictionary<string, string> DictionaryPerpertys, string StrTitle)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<System>");
            Type objtype = objectdata.GetType();
            sb.AppendLine("<Name>" + SystemCode + "</Name>");
            sb.AppendLine("<Object Name=\"" + objtype.Name + "\" Description=\"" + StrTitle + "\">");
            PropertyInfo[] propinfos = objtype.GetProperties();
            foreach (PropertyInfo propinfo in propinfos)
            {
                if (propinfo.Name.ToUpper() != "CHECKSTATE")
                {

                    if (DictionaryPerpertys.ContainsKey(propinfo.Name))
                    {
                        string keyValue = string.Empty;
                        DictionaryPerpertys.TryGetValue(propinfo.Name, out keyValue);
                        if (propinfo.PropertyType.BaseType == typeof(SMT.SaaS.OA.UI.SmtOACommonOfficeService.EntityReference))
                        {
                            //判断里面是否包含分割字符#
                            if (keyValue.IndexOf('#') > -1)
                            {
                                string[] StrArray = keyValue.Split('#');
                                sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                    + "\" LableResourceID=\"" + propinfo.Name
                                    + "\" Description=\"" + GetResourceStr(StrArray[0])
                                    + "\" DataType=\"" + ""
                                    + "\" DataValue=\"" + StrArray[0]
                                    + "\" DataText=\"" + StrArray[1]
                                    + "\"/>");
                            }
                            else
                            {
                                //如果没有分隔符#则2个一致
                                sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                    + "\" LableResourceID=\"" + propinfo.Name
                                    + "\" Description=\"" + ""
                                    + "\" DataType=\"" + ""
                                    + "\" DataValue=\"" + keyValue
                                    + "\" DataText=\"" + keyValue
                                    + "\"/>");
                            }

                        }
                        else
                        {

                            //DictionaryPerpertys.TryGetValue(propinfo.Name, out keyValue);
                            sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                + "\" LableResourceID=\"" + propinfo.Name
                                + "\" Description=\"" + GetResourceStr(propinfo.Name)
                                + "\" DataType=\"" + ""
                                + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)
                                + "\" DataText=\"" + keyValue
                                + "\"/>");
                        }
                    }
                    else
                    {
                        sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                            + "\" LableResourceID=\"" + propinfo.Name
                            + "\" Description=\"" + GetResourceStr(propinfo.Name)
                            + "\" DataType=\"" + ""
                            + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)
                            + "\" DataText=\"" + ""
                            + "\"/>");
                    }
                }
            }
            foreach (var q in Prameters)
            {
                sb.AppendLine("<Attribute Name=\"" + q.Key
                       + "\"LableResourceID=\"" + q.Key
                       + "\" Description=\"" + ""
                       + "\" DataType=\"" + ""
                       + "\" DataValue=\"" + q.Value
                       + "\" DataText=\"" + ""
                       + "\"/>");
            }

            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\" DataText=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
        }




        /// <summary>
        /// 流程最新元数据需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="Prameters">当需要传递不在当前实体中的属性值时使用</param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode, Dictionary<string, string> DictionaryPerpertys, Dictionary<List<object>, Dictionary<string, string>> DictionaryPerpertys2)
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

                    if (DictionaryPerpertys.ContainsKey(propinfo.Name))
                    {
                        string keyValue = string.Empty;
                        DictionaryPerpertys.TryGetValue(propinfo.Name, out keyValue);
                        if (propinfo.PropertyType.BaseType == typeof(SMT.SaaS.OA.UI.SmtOACommonOfficeService.EntityReference))
                        {
                            //判断里面是否包含分割字符#
                            if (keyValue.IndexOf('#') > -1)
                            {
                                string[] StrArray = keyValue.Split('#');
                                sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                    + "\" LableResourceID=\"" + propinfo.Name
                                    + "\" Description=\"" + ""
                                    + "\" DataType=\"" + ""
                                    + "\" DataValue=\"" + StrArray[0]
                                    + "\" DataText=\"" + StrArray[1]
                                    + "\"/>");
                            }
                            else
                            {
                                //如果没有分隔符#则2个一致
                                sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                    + "\" LableResourceID=\"" + propinfo.Name
                                    + "\" Description=\"" + ""
                                    + "\" DataType=\"" + ""
                                    + "\" DataValue=\"" + keyValue
                                    + "\" DataText=\"" + keyValue
                                    + "\"/>");
                            }

                        }
                        else
                        {

                            //DictionaryPerpertys.TryGetValue(propinfo.Name, out keyValue);
                            sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                + "\" LableResourceID=\"" + propinfo.Name
                                + "\" Description=\"" + GetResourceStr(propinfo.Name)
                                + "\" DataType=\"" + ""
                                + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)
                                + "\" DataText=\"" + keyValue
                                + "\"/>");
                        }
                    }
                    else
                    {
                        sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                            + "\" LableResourceID=\"" + propinfo.Name
                            + "\" Description=\"" + GetResourceStr(propinfo.Name)
                            + "\" DataType=\"" + ""
                            + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)
                            + "\" DataText=\"" + ""
                            + "\"/>");
                    }
                }
            }
            foreach (var q in Prameters)
            {
                sb.AppendLine("<Attribute Name=\"" + q.Key
                       + "\"LableResourceID=\"" + q.Key
                       + "\" Description=\"" + ""
                       + "\" DataType=\"" + ""
                       + "\" DataValue=\"" + q.Value
                       + "\" DataText=\"" + ""
                       + "\"/>");
            }

            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\" DataText=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\"/>");
            if (DictionaryPerpertys2.Count > 0)
            {
                sb.AppendLine("<ObjectList Name=\"ApprovalDetailList\" LableResourceID=\"ApprovalDetailList\" Description\"测试单明细\" DataText=\"\">");
                DictionaryPerpertys2.ForEach(item =>
                {

                });
            }
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
        }




        #endregion



        #region 按照移动版要求生成XML--有子表的情况

        /// <summary>
        /// 流程最新元数据需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="Prameters">当需要传递不在当前实体中的属性值时使用</param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string NewObjListToXmlForChildren<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode, Dictionary<string, string> DictionaryPerpertys, List<object> Childobj)
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

                    if (DictionaryPerpertys.ContainsKey(propinfo.Name))
                    {
                        string keyValue = string.Empty;
                        DictionaryPerpertys.TryGetValue(propinfo.Name, out keyValue);
                        if (propinfo.PropertyType.BaseType == typeof(SMT.SaaS.OA.UI.SmtOACommonOfficeService.EntityReference))
                        {
                            //判断里面是否包含分割字符#
                            if (keyValue.IndexOf('#') > -1)
                            {
                                string[] StrArray = keyValue.Split('#');
                                sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                    + "\" LableResourceID=\"" + propinfo.Name
                                    + "\" Description=\"" + ""
                                    + "\" DataType=\"" + ""
                                    + "\" DataValue=\"" + StrArray[0]
                                    + "\" DataText=\"" + StrArray[1]
                                    + "\"/>");
                            }
                            else
                            {
                                //如果没有分隔符#则2个一致
                                sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                    + "\" LableResourceID=\"" + propinfo.Name
                                    + "\" Description=\"" + ""
                                    + "\" DataType=\"" + ""
                                    + "\" DataValue=\"" + keyValue
                                    + "\" DataText=\"" + keyValue
                                    + "\"/>");
                            }

                        }
                        else
                        {

                            //DictionaryPerpertys.TryGetValue(propinfo.Name, out keyValue);
                            sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                                + "\" LableResourceID=\"" + propinfo.Name
                                + "\" Description=\"" + GetResourceStr(propinfo.Name)
                                + "\" DataType=\"" + ""
                                + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)
                                + "\" DataText=\"" + keyValue
                                + "\"/>");
                        }
                    }
                    else
                    {
                        sb.AppendLine("<Attribute Name=\"" + propinfo.Name
                            + "\" LableResourceID=\"" + propinfo.Name
                            + "\" Description=\"" + GetResourceStr(propinfo.Name)
                            + "\" DataType=\"" + ""
                            + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)
                            + "\" DataText=\"" + ""
                            + "\"/>");
                    }
                }
            }
            foreach (var q in Prameters)
            {
                sb.AppendLine("<Attribute Name=\"" + q.Key
                       + "\"LableResourceID=\"" + q.Key
                       + "\" Description=\"" + ""
                       + "\" DataType=\"" + ""
                       + "\" DataValue=\"" + q.Value
                       + "\" DataText=\"" + ""
                       + "\"/>");
            }

            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
        }


        #endregion

        /// <summary>
        /// 填充传入的实体的公共数据
        /// </summary>
        public static void CreateUserInfo<TEntity>(TEntity entity) where TEntity:class
        {
            SMT.SaaS.LocalData.LoginUserInfo userInfo = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo;

            entity.SetObjValue("OWNERCOMPANYID", userInfo.UserPosts[0].CompanyID);
            entity.SetObjValue("OWNERDEPARTMENTID", userInfo.UserPosts[0].DepartmentID);
            entity.SetObjValue("OWNERPOSTID", userInfo.UserPosts[0].PostID);
            entity.SetObjValue("OWNERID", userInfo.EmployeeID);

            entity.SetObjValue("CREATECOMPANYID", userInfo.UserPosts[0].CompanyID);
            entity.SetObjValue("CREATEDEPARTMENTID", userInfo.UserPosts[0].DepartmentID);
            entity.SetObjValue("CREATEPOSTID", userInfo.UserPosts[0].PostID);

            entity.SetObjValue("CREATEUSERID", userInfo.EmployeeID);
            //entity.SetObjValue("UPDATEUSERID", userInfo.EmployeeID);

            entity.SetObjValue("CREATEDATE", DateTime.Now.ToString("yyyy-MM-dd"));
            //entity.SetObjValue("UPDATEDATE", DateTime.Now.ToString("yyyy-MM-dd"));

            entity.SetObjValue("CREATEUSERNAME", userInfo.EmployeeName);
            //entity.SetObjValue("UPDATEUSERNAME", userInfo.EmployeeName);
            entity.SetObjValue("OWNERNAME", userInfo.EmployeeName);

            entity.SetObjValue("CREATEDEPARTMENTNAME", userInfo.UserPosts[0].DepartmentName);
            entity.SetObjValue("CREATECOMPANYNAME", userInfo.UserPosts[0].CompanyName);
            entity.SetObjValue("CREATEPOSTNAME", userInfo.UserPosts[0].PostName);

            entity.SetObjValue("OWNERDEPARTMENTNAME", userInfo.UserPosts[0].DepartmentName);
            entity.SetObjValue("OWNERCOMPANYNAME", userInfo.UserPosts[0].CompanyName);
            entity.SetObjValue("OWNERPOSTNAME", userInfo.UserPosts[0].PostName);

        }

        /// <summary>
        /// 寻找相应名称的父控件
        /// </summary>
        public static T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)parent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// 寻找相应名称子控件的集合
        /// </summary>
        public static List<T> GetChildObjects<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, ""));
            }
            return childList;
        }

        #region 新上传控件
        /// <summary>
        /// 新上传控件调用
        /// </summary>
        /// <param name="strModelCode">模块编码，一般为表名</param>
        /// <param name="strApplicationID">表单ID</param>
        /// <param name="action">动作</param>
        /// <param name="control">上传控件</param>
        public static void InitFileLoad(string strModelCode, string strApplicationID, FormTypes action, FileUpLoad.FileControl control)
        {
            InitFileLoad(strModelCode, strApplicationID, action, control, true);
        }
        /// <summary>
        /// 新上传控件调用
        /// </summary>
        /// <param name="strModelCode">模块编码，一般为表名</param>
        /// <param name="strApplicationID">表单ID</param>
        /// <param name="action">动作</param>
        /// <param name="control">上传控件</param>
        /// <param name="AllowDelete">是否允许删除</param>
        public static void InitFileLoad(string strModelCode, string strApplicationID, FormTypes action, FileUpLoad.FileControl control, bool AllowDelete)
        {
            SMT.FileUpLoad.Classes.UserConfig uc = new SMT.FileUpLoad.Classes.UserConfig();
            uc.CompanyCode = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            uc.SystemCode = "OA";
            uc.ModelCode = strModelCode;
            uc.UserID = Common.CurrentLoginUserInfo.EmployeeID;
            uc.ApplicationID = strApplicationID;
            uc.NotShowThumbailChckBox = true;
            if (action == FormTypes.Browse || action == FormTypes.Audit)
            {
                uc.NotShowUploadButton = true;
                uc.NotShowDeleteButton = true;
                uc.NotAllowDelete = true;
            }
            if (!AllowDelete)
            {
                uc.NotShowDeleteButton = true;
            }
            uc.Multiselect = true;
            uc.Filter = "所有文件 (*.*)|*.*";
            //uc.Filter = "图片文件(*.jpg,*.gif,*.bmp)|*.jpg;*.gif;*.bmp";
            uc.MaxConcurrentUploads = 5;
            uc.MaxSize = "20.MB";
            uc.CreateName = Common.CurrentLoginUserInfo.EmployeeName;
            uc.PageSize = 20;
            control.Init(uc);
        }

        public static void InitFileLoad(FormTypes action, FileUpLoad.FileControl control, string StrApplicationId, bool AllowDelete)
        {
            SMT.FileUpLoad.Classes.UserConfig uc = new SMT.FileUpLoad.Classes.UserConfig();
            uc.CompanyCode = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            uc.SystemCode = "OA";
            uc.IsLimitCompanyCode = false;
            uc.UserID = Common.CurrentLoginUserInfo.EmployeeID;
            uc.ApplicationID = StrApplicationId;

            uc.NotShowThumbailChckBox = true;
            if (action == FormTypes.Browse || action == FormTypes.Audit)
            {
                uc.NotShowUploadButton = true;
                uc.NotShowDeleteButton = true;
                uc.NotAllowDelete = true;
            }
            if (!AllowDelete)
            {
                uc.NotShowDeleteButton = true;
            }
            uc.Multiselect = true;
            uc.Filter = "所有文件 (*.*)|*.*";
            //uc.Filter = "图片文件(*.jpg,*.gif,*.bmp)|*.jpg;*.gif;*.bmp";
            uc.MaxConcurrentUploads = 5;
            uc.MaxSize = "20.MB";
            //uc.CreateName = Common.CurrentLoginUserInfo.EmployeeName;
            uc.PageSize = 20;
            control.Init(uc);
        }

        #endregion

        /// <summary>
        /// 在平台日志框上写日志
        /// </summary>
        /// <param name="message"></param>
        public static void SetLog(string message)
        {
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(message);
        }
        /// <summary>
        /// 在平台上写日志并显示日志框
        /// </summary>
        /// <param name="message"></param>
        public static void SetLogAndShowLog(string message)
        {
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(message);
            SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
        }

    }


}



