/// <summary>
/// Log No.： 1
/// Modify Desc： ItemsControl绑定多选CheckBox
/// Modifier： 冉龙军
/// Modify Date： 2010-09-15
/// </summary>
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

using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

using SMT.Saas.Tools.PermissionWS;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Controls.Theming;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.FlowWFService;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Linq;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI
{
    public class Utility
    {

        private static PermissionServiceClient clientPerm = null;
        /// <summary>
        /// 平台类型0 silverlight平台，1 aspMVC平台
        /// </summary>
        private static PortalType portaltype { get; set; }
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
        /// 克隆对像
        /// </summary>
        /// <param name="source">要被克隆的对像</param>
        /// <returns>克隆的新对像</returns>
        public static T CloneObject<T>(object source) where T : class
        {
            if (source == null)
            {
                return null;
            }

            string str = SerializeObject(source);
            T tmpObj = DeserializeObject<T>(str);
            return tmpObj;
        }

        /// <summary>
        /// 拷贝对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(T source)
        {
            var dcs = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            using (var ms = new System.IO.MemoryStream())
            {
                dcs.WriteObject(ms, source);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)dcs.ReadObject(ms);
            }
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

        public static void Log(string msg)
        {
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(msg);
        }

        public static object DeserializeObject(string objString, Type type)
        {

            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(type);

                return serializer.Deserialize(ms);
            }
        }
        public static T DeserializeObject<T>(string objString) where T : class
        {

            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                return serializer.Deserialize(ms) as T;
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
        /// 确认信息
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public static void ShowCustomMessage(MessageTypes messageType, string title, string message)
        {
            ComfirmWindow.ConfirmationBox(title, message, Utility.GetResourceStr("CONFIRMBUTTON"));
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
        public static void BindComboBox(ComboBox cbx, List<T_SYS_DICTIONARY> dicts, string category, string defaultValue)
        {
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == category
                       select d;
            List<T_SYS_DICTIONARY> tmpDicts = objs.ToList();

            T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
            nuldict.DICTIONARYNAME = GetResourceStr("NULL");
            nuldict.DICTIONARYVALUE = 0;
            tmpDicts.Insert(0, nuldict);

            cbx.ItemsSource = tmpDicts;
            cbx.DisplayMemberPath = "DICTIONARYNAME";


            foreach (var item in cbx.Items)
            {
                T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                if (dict != null)
                {
                    if (dict.DICTIONARYVALUE.ToString() == defaultValue)
                    {
                        cbx.SelectedItem = item;
                        break;
                    }
                }
            }

        }
        public static void BindComboBox(ComboBox cbx, List<T_SYS_DICTIONARY> dicts, string defaultValue)
        {

            cbx.ItemsSource = dicts;
            cbx.DisplayMemberPath = "DICTIONARYNAME";

            T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
            nuldict.DICTIONARYNAME = GetResourceStr("NULL");
            nuldict.DICTIONARYVALUE = 0;

            dicts.Insert(0, nuldict);

            foreach (var item in cbx.Items)
            {
                T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                if (dict != null)
                {
                    if (dict.DICTIONARYVALUE.ToString() == defaultValue)
                    {
                        cbx.SelectedItem = item;
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// 绑定多选CheckBox
        /// </summary>
        public static void BindCheckBoxList(List<CheckBoxModel> entlist, ListBox chkList)
        {
            if (entlist == null)
            {
                return;
            }

            if (entlist.Count() == 0)
            {
                return;
            }

            if (chkList.ItemsSource != null)
            {
                chkList.ItemsSource = null;
            }

            chkList.ItemsSource = entlist;
        }
        // 1s 冉龙军
        /// <summary>
        /// 绑定多选CheckBox
        /// </summary>
        public static void BindItemContainerList(List<CheckBoxModel> entlist, ItemsControl itemList)
        {
            if (entlist == null)
            {
                return;
            }

            if (entlist.Count() == 0)
            {
                return;
            }

            if (itemList.ItemsSource != null)
            {
                itemList.ItemsSource = null;
            }

            itemList.ItemsSource = entlist;
        }
        // 1e
        #region 检查资源是否加载
        /// <summary>
        /// 检查转换器资源是否加载
        /// </summary>
        public static void CheckResourceConverter()
        {
            if (Application.Current.Resources["ResourceWrapper"] == null)
            {
                Application.Current.Resources.Add("ResourceWrapper", new SMT.HRM.UI.ResourceWrapper());
            }

            if (Application.Current.Resources["DictionaryConverter"] == null)
            {
                Application.Current.Resources.Add("DictionaryConverter", new SMT.HRM.UI.DictionaryConverter());
            }

            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new SMT.HRM.UI.CustomDateConverter());
            }

            if (Application.Current.Resources["CheckConverter"] == null)
            {
                Application.Current.Resources.Add("CheckConverter", new SMT.HRM.UI.CheckConverter());
            }
        }
        #endregion

        /// <summary>
        /// 根据Url参数变量返回参数值
        /// </summary>
        /// <param name="sourceUrl">Url地址</param>
        /// <param name="targetParam">参数变量</param>
        /// <returns>返回参数值</returns>
        public static string GetUrlParamenter(string sourceUrl, string targetParam)
        {
            if (sourceUrl == "" || targetParam == "")
            {
                return sourceUrl;
            }
            sourceUrl = sourceUrl.Remove(0, sourceUrl.IndexOf(targetParam));
            int startPosition = targetParam.Length + 1;
            int entPosition = 0;
            if (sourceUrl.IndexOf('&') > 0)
            {
                entPosition = sourceUrl.IndexOf('&') - startPosition;
            }
            else
            {
                entPosition = sourceUrl.Length - startPosition;
            }
            return sourceUrl.Substring(startPosition, entPosition);
        }
        /// <summary>
        /// 绑定字典值
        /// </summary>
        /// <param name="cbx">要绑定的ComboBox</param>
        /// <param name="category">类型</param>
        /// <param name="defalutValue">默认值</param>
        public static void CbxItemBinder(ComboBox cbx, string category, string defalutValue)
        {
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

        /// <summary>
        /// 显示DataGrid上面通用按钮
        /// </summary>
        /// <param name="toolBar">所属工具条</param>
        /// <param name="entityName">表名称</param>
        /// <param name="displayAuditButton">是示有审核按钮</param>
        public static void DisplayGridToolBarButton(FormToolBar toolBar, string entityName, bool displayAuditButton)
        {
            //查看
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(entityName, SMT.SaaS.FrameworkUI.Common.Permissions.Browse) < 0)
            {
                // MessageBox.Show(Utility.GetResourceStr("NOPERMISSION"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOPERMISSION"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                Uri uri = new Uri("/Home", UriKind.Relative);

                //取当前主页
                //Grid grid = Application.Current.RootVisual as Grid;
                SMT.HRM.UI.App.EntryPointPage MainPage = Application.Current.RootVisual as SMT.HRM.UI.App.EntryPointPage;
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
        /// 通过选择的审核过滤条件显示相应的GridToolBar功能按钮
        /// </summary>
        /// <param name="iCheckState">审核的状态</param>
        /// <param name="toolBar">GridToolBar名</param>
        /// <param name="entityName">实体名称</param>
        public static void SetToolBarButtonByCheckState(int iCheckState, FormToolBar toolBar, string entityName)
        {
            SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(iCheckState, toolBar, entityName);
            ////修改
            //toolBar.btnEdit.Visibility = Visibility.Visible;
            //toolBar.retEdit.Visibility = Visibility.Visible;
            ////删除
            //toolBar.btnDelete.Visibility = Visibility.Visible;
            //toolBar.retEdit.Visibility = Visibility.Visible;
            ////审核
            //toolBar.btnAudit.Visibility = Visibility.Visible;
            //toolBar.retEdit.Visibility = Visibility.Visible;

            //DisplayGridToolBarButton(toolBar, entityName, true);
            //switch (iCheckState)
            //{
            //    case (int)CheckStates.All:
            //        break;
            //    case (int)CheckStates.Approved:
            //        //修改
            //        toolBar.btnEdit.Visibility = Visibility.Collapsed;
            //        toolBar.retEdit.Visibility = Visibility.Collapsed;
            //        //删除
            //        toolBar.btnDelete.Visibility = Visibility.Collapsed;
            //        toolBar.retDelete.Visibility = Visibility.Collapsed;
            //        //审核
            //        toolBar.btnAudit.Visibility = Visibility.Collapsed;
            //        toolBar.retAudit.Visibility = Visibility.Collapsed;
            //        break;
            //    case (int)CheckStates.Approving:
            //        //修改
            //        toolBar.btnEdit.Visibility = Visibility.Collapsed;
            //        toolBar.retEdit.Visibility = Visibility.Collapsed;
            //        //删除
            //        toolBar.btnDelete.Visibility = Visibility.Collapsed;
            //        toolBar.retDelete.Visibility = Visibility.Collapsed;
            //        break;
            //    case (int)CheckStates.Delete:
            //        break;
            //    case (int)CheckStates.UnApproved:
            //        //修改
            //        toolBar.btnEdit.Visibility = Visibility.Collapsed;
            //        toolBar.retEdit.Visibility = Visibility.Collapsed;
            //        //删除
            //        toolBar.btnDelete.Visibility = Visibility.Collapsed;
            //        toolBar.retDelete.Visibility = Visibility.Collapsed;
            //        break;
            //    case (int)CheckStates.UnSubmit:
            //        break;
            //    case (int)CheckStates.WaittingApproval:
            //        //修改
            //        toolBar.btnEdit.Visibility = Visibility.Collapsed;
            //        toolBar.retEdit.Visibility = Visibility.Collapsed;
            //        //删除
            //        toolBar.btnDelete.Visibility = Visibility.Collapsed;
            //        toolBar.retEdit.Visibility = Visibility.Collapsed;
            //        break;

            //}
        }


        public static string GetCheckState(CheckStates checkState)
        {
            return ((int)checkState).ToString();
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
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(menucode, SMT.SaaS.FrameworkUI.Common.Permissions.Edit, userid, postid, departid, comapnyid) >= 0)
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
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "2",
            //    Title = GetResourceStr("SUBMITAUDIT"),//"提交审核"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "3",
            //    Title = GetResourceStr("AUDIT"),//"审核"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "4",
            //    Title = GetResourceStr("AUDITNOTPASS"),//"审核不通过"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png"
            //};

            //items.Add(item);

            return items;
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
        /// <summary>
        /// 代提单时设置流程实体请使用此方法
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modelcode"></param>
        /// <param name="formid"></param>
        /// <param name="strXmlObjectSource"></param>
        /// <param name="Ownerid">被代提单的员工Id</param>
        public static void SetAuditEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity, string modelcode, string formid, string strXmlObjectSource, string Ownerid)
        {
            entity.ModelCode = modelcode;//"archivesLending";T_HR_COMPANY
            entity.FormID = formid; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
            entity.CreateCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;// "7cd6c0a4-9735-476a-9184-103b962d3383";
            entity.CreateDepartmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entity.CreatePostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entity.CreateUserID = Ownerid;

            entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.XmlObject = strXmlObjectSource;
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

            if (paras.Keys.Contains("CreateUserName"))
            {
                entity.CreateUserName = paras["CreateUserName"];
            }
            else
            {
                entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            }
            entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.XmlObject = strXmlObjectSource;
        }
        public static void SetAuditEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity, string modelcode, string formid)
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
            //entity.XmlObject = strXmlObjectSource;
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
        /// <param name="Prameters">当需要传递不在当前实体中的属性值时使用</param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXmlForSalaryRecord<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode)
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
                //if (propinfo.Name.ToUpper() == "ACTUALLYPAY")
                //{
                //    sb.AppendLine("<Attribute Name=\"" + propinfo.Name + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + "xxx" + "\"/>");
                //}
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

        #region 按照移动版要求生成XML

        public static string ObjListToXml<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode, Dictionary<string, string> DictionaryPerpertys, List<object> listobjs)
        {
            return "";
        }

        /// <summary>

        /// 流程最新元数据需要的XML形式的实体字符串转化

        /// </summary>

        /// <typeparam name="T"></typeparam>

        /// <param name="objectdata"></param>

        /// <param name="Prameters">当需要传递不在当前实体中的属性值时使用</param>

        /// <param name="SystemCode"></param>

        /// <returns></returns>

        public static string ObjListToXml<T>(T objectdata, Dictionary<string, string> Prameters, string SystemCode, Dictionary<string, string> DictionaryPerpertys, string strKeyName, string strKeyValue)
        {

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

            sb.AppendLine("<System>");

            Type objtype = objectdata.GetType();

            sb.AppendLine("<Name>" + SystemCode + "</Name>");

            sb.AppendLine("<Object Name=\"" + objtype.Name + "\" Description=\"\" Key=\"" + strKeyName + "\" id=\"" + strKeyValue + "\">");

            PropertyInfo[] propinfos = objtype.GetProperties();

            foreach (PropertyInfo propinfo in propinfos)
            {

                if (propinfo.Name.ToUpper() == "CHECKSTATE")
                {
                    continue;
                }

                if (propinfo.Name.ToUpper() == "ENTITYKEY")
                {
                    continue;
                }


                if (!DictionaryPerpertys.ContainsKey(propinfo.Name))
                {
                    sb.AppendLine("<Attribute Name=\"" + propinfo.Name

                        + "\" LableResourceID=\"" + propinfo.Name

                        + "\" Description=\"" + GetResourceStr(propinfo.Name)

                        + "\" DataType=\"" + ""

                        + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)

                        + "\" DataText=\"" + ""

                        + "\"/>");
                    continue;
                }

                string keyValue = string.Empty;
                DictionaryPerpertys.TryGetValue(propinfo.Name, out keyValue);

                if (propinfo.PropertyType.BaseType == typeof(SMT.Saas.Tools.AttendanceWS.EntityReference)
                    || propinfo.PropertyType.BaseType == typeof(SMT.Saas.Tools.OrganizationWS.EntityReference)
                    || propinfo.PropertyType.BaseType == typeof(SMT.Saas.Tools.PerformanceWS.EntityReference)
                    || propinfo.PropertyType.BaseType == typeof(SMT.Saas.Tools.PersonnelWS.EntityReference)
                    || propinfo.PropertyType.BaseType == typeof(SMT.Saas.Tools.SalaryWS.EntityReference))
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
                    sb.AppendLine("<Attribute Name=\"" + propinfo.Name

                        + "\" LableResourceID=\"" + propinfo.Name

                        + "\" Description=\"" + GetResourceStr(propinfo.Name)

                        + "\" DataType=\"" + ""

                        + "\" DataValue=\"" + propinfo.GetValue(objectdata, null)

                        + "\" DataText=\"" + keyValue

                        + "\"/>");

                }
            }

            foreach (var q in Prameters)
            {

                sb.AppendLine("<Attribute Name=\"" + q.Key

                       + "\" LableResourceID=\"" + q.Key

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
        /// 生成Form上的编辑，保存并关闭按钮
        /// </summary>
        /// <returns>按钮列表</returns>
        public static List<ToolbarItem> CreateFormEditButton(string menucode, string userid, string postid, string departid, string comapnyid)
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            //如果有修改权限才允许保存
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue(menucode, SMT.SaaS.FrameworkUI.Common.Permissions.Edit, userid, postid, departid, comapnyid) >= 0)
            {
                items = CreateFormSaveButton();
            }
            return items;
        }

        public static void UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue,
            SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult result)
        {

            //Company.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            string state = "";
            switch (result)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }

            OrganizationWS.OrganizationServiceClient client = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            client.UpdateCheckStateCompleted += (o, e) =>
            {
                if (e.Error != null && e.Error.Message != "")
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));

                }
                else
                {
                    //造成多余的提示
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", strEntityName));
                }
            };

            client.UpdateCheckStateAsync(strEntityName, EntityKeyName, EntityKeyValue, state);
        }

        /// <summary>
        /// 设置查询条件集合，加入流程控制部分的条件参数(主要用于审核人员查看当前指派给自己的待审核记录)
        /// </summary>
        /// <param name="strPrimaryKey">当前查询表单的主键</param>
        /// <param name="strQueryValue">当前查询参数</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="filterString">查询条件</param>
        /// <param name="queryParas">查询参数</param>
        public static void SetFilterWithflow(string strPrimaryKey, string strQueryValue, ref string strCheckState, ref string filter, ref ObservableCollection<object> paras)
        {
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                return;
            }

            int iIndex = 0;

            if (string.IsNullOrEmpty(strQueryValue))
            {
                return;
            }

            strCheckState = Convert.ToInt32(CheckStates.Approving).ToString();   //待审核的转化为审核中

            if (!string.IsNullOrEmpty(filter))
            {
                filter += " AND";
            }

            if (paras.Count() > 0)
            {
                iIndex = paras.Count();
            }

            filter += " " + strPrimaryKey + ".Contains(@" + iIndex.ToString() + ")";
            paras.Add(strQueryValue);
        }

        ///// <summary>
        ///// 门户创建待审批的单据（通过引擎）
        ///// </summary>
        ///// <param name="FormID">FormID</param>
        ///// <param name="FormName">FormName</param>
        //public static void CreateFormFromEngine(string FormID, string FormName)
        //{

        //    //Assembly asm = Assembly.Load("");
        //    // Type dalType = Type.GetType(className);
        //    Type t = Type.GetType(FormName);//"SMT.HRM.UI.Form.Personnel.PensionMasterForm"
        //    //            //使用Activator类创建该类型的实例。 
        //    //            dalInstance = (IDAL<TEntity>)Activator.CreateInstance(t);
        //    //Assembly asm = Assembly.GetExecutingAssembly();
        //    //UserControl FormInstance = (UserControl)asm.CreateInstance("SMT.HRM.UI.Form.Personnel.PensionMasterForm");            
        //    Object[] parameters = new Object[2];    // 定义构造函数需要的参数
        //    parameters[0] = FormTypes.Edit;
        //    parameters[1] = FormID;// "5d572f2d-c0e4-49ca-960e-6bd45bfb97a9";
        //    //obj[0]= new object[]{FormTypes,"55"};

        //    object form = Activator.CreateInstance(t, parameters);
        //    if (form != null)
        //    {
        //        EntityBrowser entBrowser = new EntityBrowser(form);
        //        //entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
        //        entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //    }
        //}

        #region 弹出审核表单

        private static string FormID;
        private static string FormName;
        private static string FormType;
        private static Border bParent;

        /// <summary>
        /// 门户创建待审批的单据（通过引擎）
        /// </summary>
        /// <param name="AssemblyPath">业务系统Dll路径</param>
        /// <param name="FormID">业务表单ID</param>
        /// <param name="FormName">业务表单对应的Form名称</param>
        public static void CreateFormFromEngine(string strFormID, string strFormName, string strFormType)
        {
            FormID = strFormID;
            FormName = strFormName;
            FormType = strFormType;
            portaltype = PortalType.Silverlight;
            try
            {
                CheckResourceConverter();
                CheckPermission(FormName, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 门户创建待审批的单据（通过引擎）（weirui 2012/8/3 新平台MVC框架兼容旧协同办公平台）
        /// </summary>
        /// <param name="AssemblyPath">业务系统Dll路径</param>
        /// <param name="FormID">业务表单ID</param>
        /// <param name="FormName">业务表单对应的Form名称</param>
        public static void CreateFormFromMvcPlat(string strFormID, string strFormName, string strFormType)
        {
            FormID = strFormID;
            FormName = strFormName;
            FormType = strFormType;
            portaltype = PortalType.AspMVC;

            try
            {
                CheckResourceConverter();
                CheckPermission(FormName, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 门户创建待审批的单据（通过引擎）
        /// </summary>
        /// <param name="AssemblyPath">业务系统Dll路径</param>
        /// <param name="FormID">业务表单ID</param>
        /// <param name="FormName">业务表单对应的Form名称</param>
        public static void CreateFormFromEngine(string strFormID, string strFormName, string strFormType, Border parent)
        {
            FormID = strFormID;
            FormName = strFormName;
            FormType = strFormType;
            bParent = parent;
            portaltype = PortalType.Silverlight;

            try
            {
                CheckResourceConverter();
                CheckPermission(FormName, "0");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 门户创建待KPI（通过引擎）
        /// </summary>
        /// <param name="XmlParemeter">创建form的参数</param>
        public static void CreateFormFromEngine(string FormID, string FormName, string FormType, string XmlParemeter)
        {
            // 1s 冉龙军
            //string statecode = "";
            // 1e
            try
            {
                CheckResourceConverter();

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

                Object[] parameters = new Object[2];    // 定义构造函数需要的参数
                parameters[0] = CurrentAction;
                parameters[1] = FormID;// "5d572f2d-c0e4-49ca-960e-6bd45bfb97a9";
                // 1s 冉龙军
                //parameters[2] = statecode;
                // 1e
                object form = Activator.CreateInstance(t, parameters);
                if (form != null)
                {
                    EntityBrowser entBrowser = new EntityBrowser(form);
                    //引擎解析参数：XmlParemeter（方法来自于平台）
                    string TmpFieldValueString = XmlParemeter;
                    string[] valuerownode = TmpFieldValueString.Split('Ё');
                    for (int j = 0; j < valuerownode.Length; j++)
                    {
                        if (valuerownode[j] != "")
                        {
                            string[] valuecolnode = valuerownode[j].Split('|');
                            if (valuecolnode[0] == "FlowStateCode")
                            {
                                entBrowser.FlowStepCode = valuecolnode[1];
                            }
                            if (valuecolnode[0] == "RemindGuid")
                            {
                                entBrowser.RemindGuid = valuecolnode[1];
                            }
                            if (valuecolnode[0] == "IsKpi")
                            {
                                entBrowser.IsKpi = valuecolnode[1];
                            }
                            if (valuecolnode[0] == "MESSAGEID")
                            {
                                entBrowser.MessgeID = valuecolnode[1];
                            }
                        }
                    }

                    entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 900;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 打开Form表单
        /// </summary>
        /// <param name="FormKind">待办任务 0；我的单据 1；</param>
        private static void ShowForm(string FormKind)
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
                    case "RESUBMIT":
                        CurrentAction = FormTypes.Resubmit;
                        break;
                }
            }

            Type t = Type.GetType(FormName);

            Object[] parameters = new Object[2];    // 定义构造函数需要的参数
            parameters[0] = CurrentAction;
            parameters[1] = FormID;// "5d572f2d-c0e4-49ca-960e-6bd45bfb97a9";

            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                entBrowser.FormType = CurrentAction;

                if (FormKind == "0")
                {
                    if (bParent != null)
                    {
                        bParent.Child = entBrowser;
                        if (CurrentAction == FormTypes.Edit)
                        {
                            UICache.CreateCache("CurrentActionInfo", "FormTypes.Edit");//打开待办如果为编辑就存入缓存，因为未提交的单据打开时编辑状态
                        }
                    }
                }
                else if (FormKind == "1")
                {
                    switch (portaltype)
                    {
                        case PortalType.Silverlight:
                            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, FormID);
                            if (entBrowser.ParentWindow != null)
                            {
                                entBrowser.ParentWindow.Height = 900;
                                entBrowser.ParentWindow.Width = 900;
                            }
                            break;
                        case PortalType.AspMVC:
                            entBrowser.ShowMvcPlat<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                            break;
                        default:
                            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, FormID);
                            entBrowser.ParentWindow.Height = 900;
                            entBrowser.ParentWindow.Width = 900;
                            break;
                    }


                }

            }
        }

        /// <summary>
        /// 加载权限
        /// </summary>
        /// <param name="FormName"></param>
        /// <param name="FormKind">待办任务 0；我的单据 1；</param>
        private static void CheckPermission(string FormName, string FormKind)
        {
            string strUserId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID;

            Log("打开Form，FormName：" + FormName + " FormKind" + FormKind
                + " ,获取权限:strUserId" + strUserId
                + " FormKind:" + FormKind);
            if (clientPerm == null) clientPerm = new PermissionServiceClient();
            //  clientPerm.GetEntityPermissionByUserCompleted += new EventHandler<GetEntityPermissionByUserCompletedEventArgs>(clientPerm_GetEntityPermissionByUserCompleted);
            clientPerm.GetUserPermissionByUserToUICompleted += new EventHandler<GetUserPermissionByUserToUICompletedEventArgs>(clientPerm_GetUserPermissionByUserToUICompleted);

            //ApplicationCache.CacheMenuList
            clientPerm.GetUserPermissionByUserToUIAsync(strUserId, FormKind);

            //switch (FormName)
            //{
            //    case "SMT.HRM.UI.Form.Attendance.SignInRdForm":
            //        clientPerm.GetEntityPermissionByUserAsync(strUserId, "T_HR_EMPLOYEESIGNINRECORD");
            //        break;
            //    default:
            //        clientPerm.GetUserPermissionByUserToUIAsync(strUserId);
            //        break;
            //}
        }

        static void clientPerm_GetUserPermissionByUserToUICompleted(object sender, GetUserPermissionByUserToUICompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //if (e.Result == null)
                //{
                //    return;
                //}
                try
                {
                    List<V_UserPermissionUI> entPermList = e.Result.ToList();
                    if (Common.CurrentLoginUserInfo.PermissionInfoUI == null)
                    {
                        Common.CurrentLoginUserInfo.PermissionInfoUI = new List<SaaS.LocalData.V_UserPermissionUI>();
                        SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("HR CurrentLoginUserInfo.PermissionInfoUI 为空，New了一个新的PermissionInfoUI");
                    }
                    if (entPermList != null && entPermList.Any())
                    {
                        foreach (var fent in entPermList)
                        {
                            SMT.SaaS.LocalData.V_UserPermissionUI tps = new SMT.SaaS.LocalData.V_UserPermissionUI();
                            tps = Common.CloneObject<V_UserPermissionUI, SMT.SaaS.LocalData.V_UserPermissionUI>(fent, tps);
                            Common.CurrentLoginUserInfo.PermissionInfoUI.Add(tps);
                        }
                    }

                    if (SMT.SAAS.Main.CurrentContext.AppContext.AppHost != null)
                    {
                        SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("HR ShowForm" + e.UserState.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }

                ShowForm(Convert.ToString(e.UserState));
                
            }
        }

        //static void clientPerm_GetEntityPermissionByUserCompleted(object sender, GetEntityPermissionByUserCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result == null)
        //        {
        //            string strUserId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID;
        //            PermissionServiceClient clientPerm = new PermissionServiceClient();
        //            clientPerm.GetUserPermissionByUserToUIAsync(strUserId);
        //            clientPerm.GetUserPermissionByUserToUICompleted += new EventHandler<GetUserPermissionByUserToUICompletedEventArgs>(clientPerm_GetUserPermissionByUserToUICompleted);
        //            return;
        //        }
        //        List<V_UserPermissionUI> entPermList = e.Result.ToList();
        //        SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI = entPermList;
        //        ShowForm();
        //    }
        //}

        #endregion

        /// <summary>
        /// 获取HRM所有实体名
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllEntityName()
        {
            string strResourceName = "SMT.HRM.UI.EntitysXML.SMT_HRM_EFModel.xml";

            //WebClient client = new WebClient(); 
            //client.DownloadStringAsync(new Uri(HtmlPage.Document.DocumentUri, "projects.xml")); 
            //client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted); 
            //} Stream stream = Assembly.LoadFrom("SMT.HRM.UI.dll").GetManifestResourceStream("SMT.HRM.UI.EntitysXML.SMT_HRM_EFModel.xml");            
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(strResourceName);
            //MessageBox.Show(Assembly.GetExecutingAssembly().FullName);
            XElement xele = XElement.Load(stream);

            //XElement xele = XElement.Load("EntitysXML/SMT_HRM_EFModel.xml");
            var qNames = from ent in xele.Elements()
                         select ent;

            var q = from ent in qNames
                    select ent.Attribute("Name").Value;


            return q.ToList();
        }

        /// <summary>
        /// 获取HRM所有实体名
        /// </summary>
        /// <returns></returns>
        public static List<SMT.SaaS.LocalData.T_SYS_ENTITYMENU> GetSystemEntity()
        {
            //WebClient client = new WebClient(); 
            //client.DownloadStringAsync(new Uri(HtmlPage.Document.DocumentUri, "projects.xml")); 
            //client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted); 
            //}             
            List<SMT.SaaS.LocalData.T_SYS_ENTITYMENU> listP = new List<SMT.SaaS.LocalData.T_SYS_ENTITYMENU>();

            var q = from ent in GetAllEntityName()
                    select ent;

            string entityNames = string.Empty;

            foreach (var entName in q)
            {
                var entityMenu = from ent in SMT.SAAS.Main.CurrentContext.Common.EntityMenu
                                 where ent.ENTITYCODE == entName
                                 select ent;
                if (entityMenu.FirstOrDefault() != null)
                {
                    listP.Add(entityMenu.FirstOrDefault());
                    entityNames += "," + entityMenu.FirstOrDefault().ENTITYNAME;
                }
            }
            return listP;
        }

        /// <summary>
        /// 通过指定的实体获取此实体的所有属性
        /// </summary>
        /// <param name="EntityName">实体名称</param>
        /// <returns></returns>
        public static List<EntityProPerty> GetEntityPropertyByName(string EntityName)
        {
            List<EntityProPerty> listP = new List<EntityProPerty>();

            //XElement xele = XElement.Load("EntitysXML/SMT_HRM_EFModel.xml");
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SMT.HRM.UI.EntitysXML.SMT_HRM_EFModel.xml");
            XElement xele = XElement.Load(stream);
            var qNames = from ent in xele.Elements()
                         select ent;

            var qEntity = from ent in qNames
                          where ent.Attribute("Name").Value == EntityName
                          select ent;

            var qElement = from c in qEntity.Descendants()
                           select c;

            var qProperty = from ent in qElement
                            where ent.Name.LocalName == "Property"
                            select ent;

            var qNavigationProperty = from ent in qElement
                                      where ent.Name.LocalName == "NavigationProperty"
                                      select ent;
            foreach (var ent in qNavigationProperty)
            {
                var qcolumn = from a in qNames
                              where a.Attribute("Name").Value == ent.FirstAttribute.Value
                              select a;
                var qcolumnEle = from b in qcolumn.Descendants()
                                 select b;
                var qcolumnPro = from d in qcolumnEle
                                 where d.Name.LocalName == "Property"
                                 select d;
                if (qcolumnPro.Count() > 0)
                {
                    var q = qcolumnPro.FirstOrDefault();
                    EntityProPerty firstcolumn = new EntityProPerty();
                    firstcolumn.ProPertyName = GetResourceStr(q.Attribute("Name").Value);
                    firstcolumn.ProPertyCode = q.Attribute("Name").Value;
                    firstcolumn.PropertyType = q.Attribute("Type").Value;
                    if (firstcolumn.PropertyType == "String")
                    {
                        firstcolumn.ProPertyLenth = q.Attribute("MaxLength").Value;
                    }
                    else if (firstcolumn.PropertyType == "Decimal")
                    {
                        firstcolumn.ProPertyLenth = q.Attribute("Precision").Value;
                    }
                    else if (firstcolumn.PropertyType == "DateTime")
                    {
                        firstcolumn.ProPertyLenth = "";
                    }
                    listP.Add(firstcolumn);
                }
            }

            foreach (var ent in qProperty)
            {
                EntityProPerty proPerty = new EntityProPerty();
                proPerty.ProPertyName = GetResourceStr(ent.Attribute("Name").Value);
                proPerty.ProPertyCode = ent.Attribute("Name").Value;
                proPerty.PropertyType = ent.Attribute("Type").Value;
                if (proPerty.PropertyType == "String")
                {
                    proPerty.ProPertyLenth = ent.Attribute("MaxLength").Value;
                }
                else if (proPerty.PropertyType == "Decimal")
                {
                    proPerty.ProPertyLenth = ent.Attribute("Precision").Value;
                }
                else if (proPerty.PropertyType == "DateTime")
                {
                    proPerty.ProPertyLenth = "";
                }

                listP.Add(proPerty);
            }

            return listP;
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
            uc.SystemCode = "HR";
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
            uc.SystemCode = "HR";
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

        #region 模拟平台在待办任务中点击新建按钮事件
        /// <summary>
        /// 模拟平台在待办任务中点击新建按钮事件
        /// </summary>
        /// <param name="FormType"></param>
        /// <param name="InitParams"></param>
        public static void OpenNewTask(string FormType, Dictionary<string, string> InitParams)
        {
            Type moduleType = null;

            object instance = null;
            try
            {
                moduleType = Type.GetType(FormType);
                instance = Activator.CreateInstance(moduleType);
                if (InitParams != null && instance != null)
                {
                    foreach (var item in InitParams)
                    {
                        PropertyInfo property = instance.GetType().GetProperty(item.Key);
                        property.SetValue(instance, item.Value, null);
                    }
                }
                if (moduleType != null && instance != null)
                {
                    SMT.SaaS.FrameworkUI.EntityBrowser browser = new SaaS.FrameworkUI.EntityBrowser(instance);

                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("新建任务打开异常,请查看系统日志！");
                //Logging.Logger.Current.Log("10000", "Platform", "新建任务", "新建任务打开异常", ex, Logging.Category.Exception, Logging.Priority.High);
            }
        }

        #endregion

    }


    public class EntityProPerty
    {
        private string proPertyCode;

        public string ProPertyCode
        {
            get { return proPertyCode; }
            set { proPertyCode = value; }
        }

        private string proPertyName;

        public string ProPertyName
        {
            get { return proPertyName; }
            set { proPertyName = value; }
        }
        private string propertyType;

        public string PropertyType
        {
            get { return propertyType; }
            set { propertyType = value; }
        }
        private string proPertyLenth;

        public string ProPertyLenth
        {
            get { return proPertyLenth; }
            set { proPertyLenth = value; }
        }



    }

    public enum PortalType
    {
        Silverlight,
        AspMVC
    }
}
