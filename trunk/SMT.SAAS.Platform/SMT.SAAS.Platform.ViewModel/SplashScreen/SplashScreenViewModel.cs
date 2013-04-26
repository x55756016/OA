using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.LocalData.ViewModel;
using SMT.SAAS.Platform.Core.Modularity;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.LocalData.Tables;

namespace SMT.SAAS.Platform.ViewModel.SplashScreen
{
    /// <summary>
    /// 平台首页-页面样式控制，子系统及其菜单读取，
    /// 权限更新检查
    /// </summary>
    public class SplashScreenViewModel : Foundation.BasicViewModel
    {
        /// <summary>
        /// 实例化本地存储
        /// </summary>
        public static IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        /// <summary>
        /// 平台服务类实例化
        /// </summary>
        private Model.Services.ModuleServices moduleServices = new Model.Services.ModuleServices();
        /// <summary>
        /// 部门实体集临时变量
        /// </summary>
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        /// <summary>
        /// 岗位实体集临时变量
        /// </summary>
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPosts;
        /// <summary>
        /// 公司实体集临时变量
        /// </summary>
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;

        /// <summary>
        /// 组织架构加载完成事件
        /// </summary>
        public event EventHandler OnGetOrgCompleted;

        /// <summary>
        /// 权限接口实例化
        /// </summary>
        private PermissionServiceClient permClient = new PermissionServiceClient();

        /// <summary>
        /// 构造函数
        /// </summary>
        public SplashScreenViewModel()
        {
            moduleServices.OnGetModulesCompleted += new EventHandler<Model.GetEntityListEventArgs<Model.ModuleInfo>>(_services_OnGetModulesCompleted);
            RegisterServers();
        }

        #region 成员列表
        /// <summary>
        /// 预加载标志符（私有变量）
        /// </summary>
        private bool bIsStart = true;
        /// <summary>
        /// 预加载标志符
        /// </summary>
        public bool IsStart
        {
            get
            {
                return bIsStart;
            }
            set
            {
                SetValue(ref bIsStart, value, "IsStart");
                if (value)
                {
                    GetModules();
                }
            }
        }

        /// <summary>
        /// 平台加载消息提示
        /// </summary>
        private string strMessage = "正在初始化系统......";

        /// <summary>
        /// 平台加载消息提示
        /// </summary>
        public string Message
        {
            get
            {
                return strMessage;
            }
            set
            {
                SetValue(ref strMessage, value, "Message");
            }
        }

        #endregion

        /// <summary>
        /// 系统初始化完成事件
        /// </summary>
        public event EventHandler InitCompleted;

        /// <summary>
        /// 系统初始化异常事件
        /// </summary>
        public event EventHandler InitFailied;

        /// <summary>
        /// 检查权限是否需要更新
        /// </summary>
        public void CheckPerm()
        {
            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
            {
                return;
            }

            permClient.GetLatestTimeOfPermissionCompleted += new EventHandler<GetLatestTimeOfPermissionCompletedEventArgs>(permClient_GetLatestTimeOfPermissionCompleted);
            permClient.GetLatestTimeOfPermissionAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        /// <summary>
        /// 权限检查更新完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void permClient_GetLatestTimeOfPermissionCompleted(object sender, GetLatestTimeOfPermissionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                if (V_PermissionCheckVM.IsExists(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID) == false)
                {
                    SavePermCheckByLocal(e.Result);
                }
                else
                {
                    GetPermCheckByLocal(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, e.Result);
                }
            }
        }

        /// <summary>
        /// 检查是否需要更新本地权限更新检查实体数据
        /// </summary>
        /// <param name="strEmployeeID">员工ID</param>
        /// <param name="entPermissionUpdateState">权限检查实体（本地存储）</param>
        private void GetPermCheckByLocal(string strEmployeeID, V_PermissionUpdateState entPermissionUpdateState)
        {
            V_PermissionCheck vLocalPermissionCheck = V_PermissionCheckVM.Get_V_PermissionCheck(strEmployeeID);

            bool bIsUpdate = false;
            foreach (var item in entPermissionUpdateState.Timer)
            {
                switch (item.Key.ToUpper())
                {
                    case "T_SYS_USERROLE":
                        if (item.Value != null)
                        {
                            if (vLocalPermissionCheck.UserRoleDate != item.Value.Value)
                            {
                                vLocalPermissionCheck.UserRoleDate = item.Value.Value;
                                bIsUpdate = true;
                            }
                        }
                        break;
                    case "T_SYS_ROLEENTITYMENU":
                        if (item.Value != null)
                        {
                            if (vLocalPermissionCheck.RoleEntityMenuDate != item.Value.Value)
                            {
                                vLocalPermissionCheck.RoleEntityMenuDate = item.Value.Value;
                                bIsUpdate = true;
                            }
                        }
                        break;
                    case "T_SYS_ROLEMENUPERMISSION":
                        if (item.Value != null)
                        {
                            if (vLocalPermissionCheck.RoleMenuPermissionDate != item.Value.Value)
                            {
                                vLocalPermissionCheck.RoleMenuPermissionDate = item.Value.Value;
                                bIsUpdate = true;
                            }
                        }
                        break;
                    case "T_SYS_ENTITYMENUCUSTOMPERM":
                        if (item.Value != null)
                        {
                            if (vLocalPermissionCheck.EntityMenuCustompermDate != item.Value.Value)
                            {
                                vLocalPermissionCheck.EntityMenuCustompermDate = item.Value.Value;
                                bIsUpdate = true;
                            }
                        }
                        break;
                }
            }

            SMT.SAAS.Main.CurrentContext.AppContext.IsPermUpdate = bIsUpdate;
            if (bIsUpdate)
            {
                V_PermissionCheckVM.SaveV_PermissionCheck(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, vLocalPermissionCheck);
            }
        }

        /// <summary>
        /// 保存权限更新检查实体数据到本地数据库
        /// </summary>
        /// <param name="entPermissionUpdateState"></param>
        private static void SavePermCheckByLocal(V_PermissionUpdateState entPermissionUpdateState)
        {
            V_PermissionCheck entPermissionCheck = new V_PermissionCheck();
            entPermissionCheck.EmployeeID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            foreach (var item in entPermissionUpdateState.Timer)
            {
                DateTime dtPermDate = new DateTime();

                switch (item.Key.ToUpper())
                {
                    case "T_SYS_USERROLE":
                        entPermissionCheck.UserRoleDate = dtPermDate;
                        if (item.Value != null)
                        {
                            entPermissionCheck.UserRoleDate = item.Value.Value;
                        }
                        break;
                    case "T_SYS_ROLEENTITYMENU":
                        entPermissionCheck.UserRoleDate = dtPermDate;
                        if (item.Value != null)
                        {
                            entPermissionCheck.RoleEntityMenuDate = item.Value.Value;
                        }
                        break;
                    case "T_SYS_ROLEMENUPERMISSION":
                        entPermissionCheck.UserRoleDate = dtPermDate;
                        if (item.Value != null)
                        {
                            entPermissionCheck.RoleMenuPermissionDate = item.Value.Value;
                        }
                        break;
                    case "T_SYS_ENTITYMENUCUSTOMPERM":
                        entPermissionCheck.UserRoleDate = dtPermDate;
                        if (item.Value != null)
                        {
                            entPermissionCheck.EntityMenuCustompermDate = item.Value.Value;
                        }
                        break;
                }
            }

            V_PermissionCheckVM.SaveV_PermissionCheck(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, entPermissionCheck);
            SMT.SAAS.Main.CurrentContext.AppContext.IsPermUpdate = true;
        }

        /// <summary>
        /// 获取分系统及其菜单
        /// </summary>
        public void GetModules()
        {
            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo != null)
            {
                //if (V_ModuleInfoVM.IsExists(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID) == false)
                //{
                //    _services.OnGetModulesCompleted += new EventHandler<Model.GetEntityListEventArgs<Model.ModuleInfo>>(_services_OnGetModulesCompleted);
                //    _services.GetModuleCatalogByUser(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
                //}
                if (!Application.Current.Resources.Contains("AllModule"))
                {
                    //_services.OnGetModulesCompleted += new EventHandler<Model.GetEntityListEventArgs<Model.ModuleInfo>>(_services_OnGetModulesCompleted);
                    moduleServices.GetModuleCatalogByUser(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
                }
                else
                {
                    //权限检查发现有变更时，权限需要重新从服务器获取
                    if (SMT.SAAS.Main.CurrentContext.AppContext.IsPermUpdate)
                    {
                        Application.Current.Resources.Remove("AllModule");
                        //_services.OnGetModulesCompleted += new EventHandler<Model.GetEntityListEventArgs<Model.ModuleInfo>>(_services_OnGetModulesCompleted);
                        moduleServices.GetModuleCatalogByUser(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
                        return;
                    }

                    List<ModuleInfo> moduleinfo = Application.Current.Resources["AllModule"] as List<ModuleInfo>;
                    InitMainContext(moduleinfo);
                    if (this.InitCompleted != null)
                    {
                        this.InitCompleted(this, EventArgs.Empty);
                    }
                    //GetModulesByLocal(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
                }
            }
        }

        /// <summary>
        /// 获取分系统及其菜单完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _services_OnGetModulesCompleted(object sender, Model.GetEntityListEventArgs<Model.ModuleInfo> e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    List<ModuleInfo> moduleinfo = new List<ModuleInfo>();
                    foreach (var item in e.Result)
                    {
                        if (item.ModuleName != null)
                        {
                            ModuleInfo info = item.CloneObject<ModuleInfo>(new ModuleInfo());
                            info.InitializationMode = item.UseState == "1" ? InitializationMode.OnDemand : InitializationMode.OnDemand;
                            info.Ref = string.Empty;
                            info.IsOnWeb = false;
                            info.ModuleType = item.UseState == "1" ? string.Empty : item.ModuleType;
                            moduleinfo.Add(info);
                        }
                    }
                    InitMainContext(moduleinfo);
                    if (!Application.Current.Resources.Contains("AllModule"))
                    {
                        Application.Current.Resources.Add("AllModule", moduleinfo);
                    }
                    //SaveModuleByLocal(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, moduleinfo);
                    if (this.InitCompleted != null)
                    {
                        this.InitCompleted(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 保存分系统及其菜单完成事件到本地数据库
        /// </summary>
        /// <param name="strUserID">用户ID</param>
        /// <param name="listModuleInfos">菜单实体集</param>
        private void SaveModuleByLocal(string strUserID, List<ModuleInfo> listModuleInfos)
        {
            List<V_ModuleInfo> vLocalModules = new List<V_ModuleInfo>();
            List<V_ModuleInfo_DependsOn> vLocalModuleDependsOns = new List<V_ModuleInfo_DependsOn>();
            List<V_ModuleInfo_Params> vLocalModuleParams = new List<V_ModuleInfo_Params>();

            foreach (var item in listModuleInfos)
            {
                if (item.ModuleName == null)
                {
                    continue;
                }

                V_ModuleInfo info = item.CloneObject<V_ModuleInfo>(new V_ModuleInfo());
                info.UserModuleID = System.Guid.NewGuid().ToString();
                info.UserID = strUserID;
                vLocalModules.Add(info);

                if (item.DependsOn != null)
                {
                    if (item.DependsOn.Count > 0)
                    {
                        foreach (var p in item.DependsOn)
                        {
                            V_ModuleInfo_DependsOn dependsOn = new V_ModuleInfo_DependsOn();
                            dependsOn.UserModuleID = info.UserModuleID;
                            dependsOn.UserID = strUserID;
                            dependsOn.ModuleID = item.ModuleID;
                            dependsOn.ModuleName = p;

                            vLocalModuleDependsOns.Add(dependsOn);
                        }
                    }
                }

                if (item.InitParams != null)
                {
                    if (item.InitParams.Count > 0)
                    {
                        foreach (var d in item.InitParams)
                        {
                            V_ModuleInfo_Params param = new V_ModuleInfo_Params();
                            param.UserModuleID = info.UserModuleID;
                            param.UserID = strUserID;
                            param.ModuleID = item.ModuleID;
                            param.ParamKey = d.Key;
                            param.ParamValue = d.Value;

                            vLocalModuleParams.Add(param);
                        }
                    }
                }
            }

            V_ModuleInfoVM.SaveV_ModuleInfo(strUserID, vLocalModules);
            V_ModuleInfo_DependsOnVM.SaveV_ModuleInfo_DependsOn(strUserID, vLocalModuleDependsOns);
            V_ModuleInfo_ParamsVM.SaveV_ModuleInfo_Params(strUserID, vLocalModuleParams);
        }

        /// <summary>
        /// 获取分系统及其菜单完成事件到本地数据库
        /// </summary>
        /// <param name="strUserID"></param>
        public void GetModulesByLocal(string strUserID)
        {
            List<ModuleInfo> moduleinfo = new List<ModuleInfo>();

            List<V_ModuleInfo> vLocalModules = V_ModuleInfoVM.GetAllV_ModuleInfo(strUserID);
            List<V_ModuleInfo_DependsOn> vLocalModuleDependsOns = V_ModuleInfo_DependsOnVM.GetAllV_ModuleInfo_DependsOn(strUserID);
            List<V_ModuleInfo_Params> vLocalModuleParams = V_ModuleInfo_ParamsVM.GetAllV_ModuleInfo_Params(strUserID);

            foreach (var item in vLocalModules)
            {
                if (item.ModuleName != null)
                {
                    ModuleInfo info = item.CloneObject<ModuleInfo>(new ModuleInfo());
                    info.InitializationMode = item.UseState == "1" ? InitializationMode.OnDemand : InitializationMode.OnDemand;
                    info.Ref = string.Empty;
                    info.IsOnWeb = false;
                    info.ModuleType = item.UseState == "1" ? string.Empty : item.ModuleType;

                    List<string> listDepends = new List<string>();
                    foreach (var p in vLocalModuleDependsOns)
                    {
                        if (p.UserModuleID != item.UserModuleID)
                        {
                            continue;
                        }

                        info.DependsOn.Add(p.ModuleName);
                    }

                    foreach (var d in vLocalModuleParams)
                    {
                        if (info.InitParams == null)
                        {
                            info.InitParams = new Dictionary<string, string>();
                        }

                        if (d.UserModuleID != item.UserModuleID)
                        {
                            continue;
                        }

                        info.InitParams.Add(d.ParamKey, d.ParamValue);
                    }

                    moduleinfo.Add(info);
                }
            }

            InitMainContext(moduleinfo);
            if (this.InitCompleted != null)
            {
                this.InitCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 初始化平台上下文环境，并加载样式文件
        /// </summary>
        /// <param name="moduleinfo"></param>
        public void Run(List<ModuleInfo> moduleinfo)
        {
            try
            {
                InitMainContext(moduleinfo);

                Message = "正在初始化系统皮肤......";

                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(Message);
                InitTheme();
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("初始系统皮肤完毕");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 初始化平台上下文环境
        /// </summary>
        /// <param name="moduleinfo"></param>
        private static void InitMainContext(List<ModuleInfo> moduleinfo)
        {
            if (Context.Managed == null)
            {
                Context.Managed = new Managed();
            }

            Context.Managed.Catalog = moduleinfo;

            Context.Managed.Run(true);
        }

        /// <summary>
        /// 加载登陆人的员工信息（内容已删改）
        /// </summary>
        private void GetUserInfo()
        {
            //Model.Services.SystemInit init = new Model.Services.SystemInit();
            //init.OnInitCompleted += new EventHandler(init_OnInitCompleted);
            Message = "正在初始化基础数据......";
            //string isAdminflag=SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.IsManager==true?"1":"0";
            //init.StartInit(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, isAdminflag, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.LoginRecordID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName);
            //if (Application.Current.Resources["CurrentUserID"] == null)
            //{
            //    if (Application.Current.Resources.Contains("CurrentUserID"))
            //        Application.Current.Resources.Remove("CurrentUserID");

            //    Application.Current.Resources.Add("CurrentUserID", SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            //}
        }

        /// <summary>
        /// 加载组织架构
        /// </summary>
        private void LoadedOrganization()
        {
        }

        /// <summary>
        /// 加载子系统及菜单数据完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Managed_OnLoadModuleCompleted(object sender, LoadModuleEventArgs e)
        {
        }

        /// <summary>
        /// 初始化用户定义的主题文件
        /// </summary>
        public void InitTheme()
        {
            string themeName;
            if (AppSettings.Contains("ThemeName"))
            {
                themeName = AppSettings["ThemeName"].ToString();
                AppSettings.Save();
            }
            else
            {
                themeName = "ShinyBlue";
                AppSettings.Add("ThemeName", themeName);
                AppSettings.Save();
            }

            Application.Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary newStyle3 = new ResourceDictionary();
            Uri uri = new Uri("/SMT.SAAS.Themes;component/" + themeName + ".xaml", UriKind.Relative);
            newStyle3.Source = uri;
            Application.Current.Resources.MergedDictionaries.Add(newStyle3);

            ResourceDictionary newStyle_toolkit = new ResourceDictionary();
            Uri uri_toolkit = new Uri("/SMT.SAAS.Themes;component/ToolKitResource.xaml", UriKind.Relative);
            newStyle_toolkit.Source = uri_toolkit;
            Application.Current.Resources.MergedDictionaries.Add(newStyle_toolkit);

            ResourceDictionary newStyle_template = new ResourceDictionary();
            Uri uri_template = new Uri("/SMT.SAAS.Themes;component/ControlTemplate.xaml", UriKind.Relative);
            newStyle_template.Source = uri_template;
            Application.Current.Resources.MergedDictionaries.Add(newStyle_template);

            ResourceDictionary platformStyle = new ResourceDictionary();
            Uri uri_platform = new Uri("/SMT.SAAS.Platform;component/Themes/CommonThemes.xaml", UriKind.Relative);
            platformStyle.Source = uri_platform;
            Application.Current.Resources.MergedDictionaries.Add(platformStyle);

            if (!Application.Current.Resources.Contains("GridHeaderConverter"))
            {
                Application.Current.Resources.Add("GridHeaderConverter", new SMT.SaaS.Globalization.GridHeaderConverter());
            }

            if (!Application.Current.Resources.Contains("ResourceConveter"))
            {
                Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            }
        }

        #region 组织架构加载
        /// <summary>
        /// 组织架构服务
        /// </summary>
        private OrganizationServiceClient organClient = null;

        /// <summary>
        /// 部门视图实体集临时变量
        /// </summary>
        private List<SMT.Saas.Tools.OrganizationWS.V_DEPARTMENT> allDepartmentsView;

        /// <summary>
        /// 岗位视图实体集临时变量
        /// </summary>
        private List<SMT.Saas.Tools.OrganizationWS.V_POST> allPostsView;

        /// <summary>
        /// 公司视图实体集临时变量
        /// </summary>
        private List<SMT.Saas.Tools.OrganizationWS.V_COMPANY> allCompanysView;

        #region 应用程序资源字典集合KEY
        //public const string RES_DICTIONARY_KEY = "SYS_DICTIONARY";

        /// <summary>
        /// 公司数据的内存缓存的键名
        /// </summary>
        public const string RESCOMPANYINFOKEY = "SYS_CompanyInfo";

        /// <summary>
        /// 部门数据的内存缓存的键名
        /// </summary>
        public const string RESDEPARTMENTINFOKEY = "SYS_DepartmentInfo";

        /// <summary>
        /// 岗位数据的内存缓存的键名
        /// </summary>
        public const string RESPOSTINFOKEY = "SYS_PostInfo";
        //public const string RES_ENTITYMENU_KEY = "SYS_EntityMenus";
        #endregion

        /// <summary>
        /// 注册服务
        /// </summary>
        private void RegisterServers()
        {
            organClient = new OrganizationServiceClient();
        }

        /// <summary>
        /// 加载公司数据
        /// </summary>
        public void LoadCompanyInfo()
        {
            organClient.GetALLCompanyViewCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        allCompanysView = e.Result.ToList();
                        ReFillDataToCompany();
                    }
                }
            };

            if (!Application.Current.Resources.Contains("SYS_CompanyInfo"))
            {
                organClient.GetALLCompanyViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                LoadDepartmentInfo();
            }
            //if (V_CompanyInfoVM.IsExists(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID) == false)
            //{
            //    organClient.GetALLCompanyViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            //}
            //else
            //{
            //    GetCompanysByLocal(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
            //}
        }

        /// <summary>
        /// 将公司信息集合转换成标准结构的公司数据集合(V_COMPANY => T_HR_COMPANY),
        /// 并将转换后的数据集合存到内存中，然后再加载部门数据
        /// </summary>
        private void ReFillDataToCompany()
        {
            allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
            var ents = allCompanysView.OrderBy(c => c.FATHERID);
            //把公司的视图集合转换为公司实体集合 
            foreach (var ent in ents)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                company.COMPANYID = ent.COMPANYID;
                company.CNAME = ent.CNAME;
                company.ENAME = ent.ENAME;
                if (!string.IsNullOrEmpty(ent.BRIEFNAME))
                {
                    company.BRIEFNAME = ent.BRIEFNAME;
                }
                else
                {
                    company.BRIEFNAME = ent.CNAME;
                }
                company.COMPANRYCODE = ent.COMPANRYCODE;
                company.SORTINDEX = ent.SORTINDEX;
                if (!string.IsNullOrEmpty(ent.FATHERCOMPANYID))
                {
                    company.T_HR_COMPANY2 = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                    company.T_HR_COMPANY2.COMPANYID = ent.FATHERCOMPANYID;
                    //modify by 安凯航 2011年9月5日
                    //在父公司ID为空时,不做处理
                    V_COMPANY v_company = allCompanysView.Where(s => s.COMPANYID == ent.FATHERCOMPANYID).FirstOrDefault();
                    if (v_company != null)
                    {
                        company.T_HR_COMPANY2.CNAME = v_company.CNAME;
                    }
                }
                //end modify;
                company.FATHERID = ent.FATHERID;
                company.FATHERTYPE = ent.FATHERTYPE;
                company.CHECKSTATE = ent.CHECKSTATE;
                company.EDITSTATE = ent.EDITSTATE;
                allCompanys.Add(company);
            }
            //把公司实体集合存入缓存
            AddToResourceDictionary<List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>>(RESCOMPANYINFOKEY, allCompanys);
            LoadDepartmentInfo();
        }

        /// <summary>
        /// 加载部门数据
        /// </summary>
        private void LoadDepartmentInfo()
        {
            organClient.GetAllDepartmentViewCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        allDepartmentsView = e.Result.ToList();
                        ReFillDataToDepartment();
                    }
                }
            };

            if (!Application.Current.Resources.Contains("SYS_DepartmentInfo"))
            {
                organClient.GetAllDepartmentViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                LoadPostInfo();
            }
            //if (V_DepartmentInfoVM.IsExists(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID) == false)
            //{
            //    organClient.GetAllDepartmentViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            //}
            //else
            //{
            //    GetDepartmentsByLocal(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
            //}
        }

        /// <summary>
        /// 将部门信息集合转换成标准结构的部门数据集合(V_DEPARTMENT => T_HR_DEPARTMENT),
        /// 并将转换后的数据集合存到内存中，然后再加载岗位数据
        /// </summary>
        private void ReFillDataToDepartment()
        {
            allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
            var ents = allDepartmentsView.OrderBy(c => c.FATHERID);
            //把部门视图集合转换成部门实体集合
            foreach (var ent in ents)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT dep = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                dep.DEPARTMENTID = ent.DEPARTMENTID;
                dep.FATHERID = ent.FATHERID;
                dep.FATHERTYPE = ent.FATHERTYPE;
                dep.T_HR_DEPARTMENTDICTIONARY = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENTDICTIONARY();
                dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = ent.DEPARTMENTDICTIONARYID;
                dep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME = ent.DEPARTMENTNAME;
                dep.T_HR_COMPANY = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
                dep.T_HR_COMPANY = allCompanys.Where(s => s.COMPANYID == ent.COMPANYID).FirstOrDefault();

                dep.DEPARTMENTBOSSHEAD = ent.DEPARTMENTBOSSHEAD;
                dep.SORTINDEX = ent.SORTINDEX;
                dep.CHECKSTATE = ent.CHECKSTATE;
                dep.EDITSTATE = ent.EDITSTATE;
                allDepartments.Add(dep);
            }
            //把部门实体集合存入缓存和独立存储
            AddToResourceDictionary<List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>>(RESDEPARTMENTINFOKEY, allDepartments);
            LoadPostInfo();
        }

        /// <summary>
        /// 加载岗位
        /// </summary>
        private void LoadPostInfo()
        {
            organClient.GetAllPostViewCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        allPostsView = e.Result.ToList();
                        ReFillDataToPost();
                    }
                }
            };

            if (!Application.Current.Resources.Contains("SYS_PostInfo"))
            {
                organClient.GetAllPostViewAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        /// <summary>
        /// 将岗位信息集合转换成标准结构的部岗位数据集合(V_DEPARTMENT => T_HR_DEPARTMENT),
        /// 并将转换后的数据集合存到内存中，然后再加载岗位数据
        /// </summary>
        private void ReFillDataToPost()
        {
            allPosts = new List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>();
            //把岗位视图集合转换为岗位实体集合
            foreach (var ent in allPostsView)
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST pt = new SMT.Saas.Tools.OrganizationWS.T_HR_POST();
                pt.POSTID = ent.POSTID;
                pt.FATHERPOSTID = ent.FATHERPOSTID;
                pt.CHECKSTATE = ent.CHECKSTATE;
                pt.EDITSTATE = ent.EDITSTATE;

                pt.T_HR_POSTDICTIONARY = new SMT.Saas.Tools.OrganizationWS.T_HR_POSTDICTIONARY();
                pt.T_HR_POSTDICTIONARY.POSTDICTIONARYID = Guid.NewGuid().ToString();
                pt.T_HR_POSTDICTIONARY.POSTNAME = ent.POSTNAME;

                pt.T_HR_DEPARTMENT = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                pt.T_HR_DEPARTMENT = allDepartments.Where(s => s.DEPARTMENTID == ent.DEPARTMENTID).FirstOrDefault();

                allPosts.Add(pt);
            }
            //把岗位实体集合存入缓存和独立存储
            AddToResourceDictionary<List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>>(RESPOSTINFOKEY, allPosts);
            if (OnGetOrgCompleted != null)
            {
                OnGetOrgCompleted(this, null);
            }
            // personelClient.GetEmployeeDetailViewByIDAsync(_UserEmployeeID);

            //第三阶段优化
            //SaveOrganizationByLocal(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
        }

        /// <summary>
        /// 从本地数据库读取登陆人所能查看到的公司信息，而后加载到缓存中
        /// </summary>
        /// <param name="strUserID"></param>
        public void GetCompanysByLocal(string strUserID)
        {
            if (Application.Current.Resources["SYS_CompanyInfo"] != null && Application.Current.Resources["SYS_DepartmentInfo"] != null && Application.Current.Resources["SYS_PostInfo"] != null)
            {
                return;
            }

            List<SMT.Saas.Tools.OrganizationWS.V_COMPANY> companyinfo = new List<SMT.Saas.Tools.OrganizationWS.V_COMPANY>();

            List<V_CompanyInfo> vLocalCompanys = V_CompanyInfoVM.GetAllV_CompanyInfo(strUserID);

            foreach (var item in vLocalCompanys)
            {
                if (string.IsNullOrWhiteSpace(item.COMPANYID))
                {
                    continue;
                }

                SMT.Saas.Tools.OrganizationWS.V_COMPANY info = item.CloneObject<SMT.Saas.Tools.OrganizationWS.V_COMPANY>(new SMT.Saas.Tools.OrganizationWS.V_COMPANY());

                companyinfo.Add(info);
            }

            allCompanysView = companyinfo;
            LoadCompanyInfo();
        }

        /// <summary>
        /// 从本地数据库读取登陆人所能查看到的部门信息，而后加载到缓存中
        /// </summary>
        /// <param name="strUserID"></param>
        public void GetDepartmentsByLocal(string strUserID)
        {
            List<SMT.Saas.Tools.OrganizationWS.V_DEPARTMENT> departmentinfos = new List<SMT.Saas.Tools.OrganizationWS.V_DEPARTMENT>();

            List<V_DepartmentInfo> vLocalCompanys = V_DepartmentInfoVM.GetAllV_DepartmentInfo(strUserID);

            foreach (var item in vLocalCompanys)
            {
                if (string.IsNullOrWhiteSpace(item.DEPARTMENTID))
                {
                    continue;
                }

                SMT.Saas.Tools.OrganizationWS.V_DEPARTMENT info = item.CloneObject<SMT.Saas.Tools.OrganizationWS.V_DEPARTMENT>(new SMT.Saas.Tools.OrganizationWS.V_DEPARTMENT());

                departmentinfos.Add(info);
            }

            allDepartmentsView = departmentinfos;
        }

        /// <summary>
        /// 从本地数据库读取登陆人所能查看到的岗位信息，而后加载到缓存中
        /// </summary>
        /// <param name="strUserID"></param>
        public void GetPostsByLocal(string strUserID)
        {
            List<SMT.Saas.Tools.OrganizationWS.V_POST> postinfos = new List<SMT.Saas.Tools.OrganizationWS.V_POST>();

            List<V_PostInfo> vLocalPosts = V_PostInfoVM.GetAllV_PostInfo(strUserID);

            foreach (var item in vLocalPosts)
            {
                if (string.IsNullOrWhiteSpace(item.POSTID))
                {
                    continue;
                }

                SMT.Saas.Tools.OrganizationWS.V_POST info = item.CloneObject<SMT.Saas.Tools.OrganizationWS.V_POST>(new SMT.Saas.Tools.OrganizationWS.V_POST());

                postinfos.Add(info);
            }

            allPostsView = postinfos;
        }

        /// <summary>
        /// 保存组织架构到本地数据库
        /// </summary>
        /// <param name="strUserID"></param>
        private void SaveOrganizationByLocal(string strUserID)
        {
            List<V_CompanyInfo> vLocalCompanys = new List<V_CompanyInfo>();
            List<V_DepartmentInfo> vLocalDepartments = new List<V_DepartmentInfo>();
            List<V_PostInfo> vLocalPosts = new List<V_PostInfo>();

            foreach (var item in allCompanysView)
            {
                if (string.IsNullOrWhiteSpace(item.COMPANYID))
                {
                    continue;
                }

                V_CompanyInfo info = item.CloneObject<V_CompanyInfo>(new V_CompanyInfo());
                info.UserModuleID = System.Guid.NewGuid().ToString();
                info.UserID = strUserID;
                vLocalCompanys.Add(info);

                if (allDepartmentsView != null)
                {
                    if (allDepartmentsView.Count() > 0)
                    {
                        foreach (var p in allDepartmentsView)
                        {
                            V_DepartmentInfo departmentInfo = item.CloneObject<V_DepartmentInfo>(new V_DepartmentInfo());
                            departmentInfo.UserModuleID = info.UserModuleID;
                            departmentInfo.UserID = strUserID;

                            vLocalDepartments.Add(departmentInfo);
                        }
                    }
                }

                if (allPostsView != null)
                {
                    if (allPostsView.Count() > 0)
                    {
                        foreach (var d in allPostsView)
                        {
                            V_PostInfo postInfo = item.CloneObject<V_PostInfo>(new V_PostInfo());
                            postInfo.UserModuleID = info.UserModuleID;
                            postInfo.UserID = strUserID;

                            vLocalPosts.Add(postInfo);
                        }
                    }
                }
            }

            V_CompanyInfoVM.SaveV_CompanyInfo(strUserID, vLocalCompanys);
            V_DepartmentInfoVM.SaveV_DepartmentInfo(strUserID, vLocalDepartments);
            V_PostInfoVM.SaveV_PostInfo(strUserID, vLocalPosts);
        }

        /// <summary>
        /// 保存记录到内存中
        /// </summary>
        /// <typeparam name="T">待存储记录</typeparam>
        /// <param name="key">字典键</param>
        /// <param name="value">字典值</param>
        private static void AddToResourceDictionary<T>(string key, T value)
        {
            try
            {
                if (Application.Current.Resources[key] == null)
                {
                    if (value != null)
                    {
                        if (Application.Current.Resources.Contains(key))
                        {
                            Application.Current.Resources.Remove(key);
                        }

                        Application.Current.Resources.Add(key, value);
                    }
                }
            }
            catch (Exception ex)
            {
                string strmsg = "将资源添加到内存时发生错误，原因：" + ex.ToString();
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);
            }
        }
        #endregion 组织架构加载
    }
}
