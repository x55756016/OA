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


using System.Collections.Generic;
using System.Linq;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Reflection;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Controls.Toolkit.Windows;//提示窗体
using SMT.Saas.Tools.FlowDesignerWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI
{
    public class Utility
    {

        private static PortalType EnumPort = PortalType.Silverlight;//定义平台的类型
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
 
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(objString)))
            {
                XmlSerializer serializer = new XmlSerializer(type);

                return serializer.Deserialize(ms);
            }
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
        
                
        public static object XmlToContractObject(string xml, Type type)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {
                DataContractSerializer dataContractSerializer = new DataContractSerializer(type);
                return dataContractSerializer.ReadObject(ms);
            }
        }

        public static string GetResourceStr(string message)
        {
            string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(message, SMT.SaaS.Globalization.Localization.UiCulture);
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


        public static void ShowCustomMessage(MessageTypes messageType, string title, string message)
        {
            // ErrorWindow ewin = new ErrorWindow(title, message);
            // ewin.Show(); 

            ComfirmWindow.ConfirmationBox(title, message, Utility.GetResourceStr("CONFIRMBUTTON"));

        }

        

        public static string GetResourceStr(string message, string parameter)
        {
            //string rslt = SMT.SaaS.Globalization.Localization.ResourceMgr.GetString(message, SMT.SaaS.Globalization.Localization.UiCulture);
            string rslt = SMT.SaaS.Globalization.Localization.GetString(message, parameter);
            return string.IsNullOrEmpty(rslt) ? message : rslt;

            //string rslt = SMT.SaaS.Globalization.Localization.GetString(message, parameter);

            //return string.IsNullOrEmpty(rslt) ? message : rslt;
        }



        

        /// <summary>
        /// 生成Form上的保存，保存并关闭按钮
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public static List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = GetResourceStr("SAVE"),// "保存",
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = GetResourceStr("SAVEANDCLOSE"),//"保存与关闭"
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
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
        public static string GetCheckState(CheckStates checkState)
        {
            return ((int)checkState).ToString();
        }

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
                SMT.SaaS.Permission.UI.App.EntryPointPage MainPage = Application.Current.RootVisual as SMT.SaaS.Permission.UI.App.EntryPointPage;
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

        #region 平台调用窗口
        
       
        private static string FormID;
        private static string FormName;
        private static string FormType;

        /// <summary>
        /// 门户创建待审批的单据（通过引擎） 2011-06-07
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="FormName"></param>
        public static void CreateFormFromEngine(string StrFormID, string StrFormName, string StrFormType)
        {
            FormID = StrFormID;
            FormName = StrFormName;
            FormType = StrFormType;
            
            ChecResource();
            CheckPermission(FormName);

        }

        /// <summary>
        /// 门户创建待审批的单据（通过引擎） 2012-08-03
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="FormName"></param>
        public static void CreateFormFromMvcPlat(string StrFormID, string StrFormName, string StrFormType)
        {
            FormID = StrFormID;
            FormName = StrFormName;
            FormType = StrFormType;
            EnumPort = PortalType.AspMVC;
            ChecResource();
            CheckPermission(FormName);

        }


        private static void ChecResource()
        {
            if (Application.Current.Resources["FLOW_MODELDEFINE_T"] == null)
            {
                Utility.LoadDictss();
            }
            if (!Application.Current.Resources.Contains("CustomDateConverter"))
            {
                Application.Current.Resources.Add("CustomDateConverter", new SMT.SaaS.Permission.UI.CustomDateConverter());
            }
            if (!Application.Current.Resources.Contains("ResourceConveter"))
            {
                Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            }
            if (!Application.Current.Resources.Contains("DictionaryConverter"))
            {
                Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.Permission.UI.DictionaryConverter());
            }
            if (!Application.Current.Resources.Contains("StateConvert"))
            {
                Application.Current.Resources.Add("StateConvert", new SMT.SaaS.Permission.UI.CheckStateConverter());
            }
            if (!Application.Current.Resources.Contains("ModuleNameConverter"))
            {
                Application.Current.Resources.Add("ModuleNameConverter", new SMT.SaaS.Permission.UI.ModuleNameConverter());
            }
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

            Object[] parameters = new Object[2];    // 定义构造函数需要的参数
            parameters[0] = CurrentAction;
            parameters[1] = FormID;// "5d572f2d-c0e4-49ca-960e-6bd45bfb97a9";

            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                entBrowser.FormType = CurrentAction;
                if (EnumPort == PortalType.Silverlight)
                {
                    entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true, FormID);
                }
                else
                {
                    entBrowser.ShowMvcPlat<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                }
                if (FormName == "SMT.SaaS.Permission.UI.Views.Travelmanagement.MissionReportsChildWindows")
                {
                    WindowsManager.MaxWindow(entBrowser.ParentWindow);
                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 1145;
                }
                else if (FormName == "SMT.SaaS.Permission.UI.Views.Travelmanagement.TravelapplicationChildWindows")
                {

                    WindowsManager.MaxWindow(entBrowser.ParentWindow);
                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 1060;
                }
                else if (FormName == "SMT.SaaS.Permission.UI.UserControls.TravelReimbursementControl")
                {
                    WindowsManager.MaxWindow(entBrowser.ParentWindow);
                    //entBrowser.ParentWindow.Height = 900;
                    //entBrowser.ParentWindow.Width = 1180;
                }
                else
                {
                    entBrowser.ParentWindow.Height = 900;
                    entBrowser.ParentWindow.Width = 900;
                }
            }
        }

        #endregion

    }

}
