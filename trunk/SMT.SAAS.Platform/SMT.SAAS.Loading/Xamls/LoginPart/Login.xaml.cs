using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Xml.Linq;
using SMT.SAAS.Main.CurrentContext;
using SMT.SAAS.Platform.EmployeeInfoWS;
using SMT.SAAS.Platform.MainUIWS;
using SMT.SaaS.LocalData;

namespace SMT.SAAS.Platform.Xamls.LoginPart
{
    /// <summary>
    /// Silverlight登录入口
    /// </summary>
    public partial class Login : UserControl
    {
        /// <summary>
        /// 本地缓存的文件夹名称
        /// </summary>
        private string strApplicationPath = "SmtPortal";

        /// <summary>
        /// 登录验证接口
        /// </summary>
        private MainUIServicesClient sysloginClinet;

        /// <summary>
        /// 员工信息接口
        /// </summary>
        private EmployeeInfoServiceClient employeeInfoClient;
        
        /// <summary>
        /// 用户实体
        /// </summary>
        private SMT.SAAS.Platform.EmployeeInfoWS.V_UserLogin sysUser;

        /// <summary>
        /// 员工信息实体
        /// </summary>
        private V_EMPLOYEEDETAIL employee;

        /// <summary>
        /// 本地缓存键名
        /// </summary>
        private const string USERKEY = "USERNAME";

        /// <summary>
        /// 本地版本文件路径
        /// </summary>
        private string dllVersionFilePath = string.Empty;

        /// <summary>
        /// 需要下载的所有xap,zip包集合
        /// </summary>
        private List<string> needDownDllNames = new List<string>();

        /// <summary>
        /// 当前正在下载的xap,zip包名称
        /// </summary>
        private string downloadDllName;

        /// <summary>
        /// 版本文件下载接口
        /// </summary>
        private WebClient webcDllVersion;

        /// <summary>
        /// 系统公共组件下载接口
        /// </summary>
        private WebClient wDownloadDllClinet;

        /// <summary>
        /// 首次登录标识
        /// </summary>
        private bool isFirstUser = true;

        /// <summary>
        /// 加载登录后首页的外观基类
        /// </summary>
        private UIElement uMainPage;

        /// <summary>
        /// 加载登录后首页的程序集
        /// </summary>
        private Assembly asmMain = null;

        /// <summary>
        /// 检测是否已开始登录
        /// </summary>
        private bool isloading = false;

        #region 全局时间，用来记录操作时间
        /// <summary>
        /// 全局时间（开始）
        /// </summary>
        DateTime dtstart = DateTime.Now;

        /// <summary>
        /// 全局时间（截止）
        /// </summary>
        DateTime dtend = DateTime.Now;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public Login()
        {
            InitializeComponent();
            RegisterServices();
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        private void RegisterServices()
        {
            HtmlPage.RegisterScriptableObject("MvcToSl", this);
            spAddSpace.Visibility = System.Windows.Visibility.Collapsed;
            sysloginClinet = new MainUIServicesClient();
            sysloginClinet.GetUserInfobyIDCompleted += new EventHandler<GetUserInfobyIDCompletedEventArgs>(sysloginClinet_GetUserInfobyIDCompleted);

            employeeInfoClient = new EmployeeInfoServiceClient();
            employeeInfoClient.getEmployeeInfobyLoginCompleted += new EventHandler<getEmployeeInfobyLoginCompletedEventArgs>(EmployeeInfoClient_getEmployeeInfobyLoginCompleted); ;
            webcDllVersion = new WebClient();
            webcDllVersion.OpenReadCompleted += new OpenReadCompletedEventHandler(webcDllVersion_OpenReadCompleted);

            wDownloadDllClinet = new WebClient();
            wDownloadDllClinet.OpenReadCompleted += new OpenReadCompletedEventHandler(webcDownloadDll_OpenReadCompleted);
            wDownloadDllClinet.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadDllClinet_DownloadProgressChanged);

            dllVersionFilePath = strApplicationPath + @"/" + "DllVersion.xml";
            employee = new V_EMPLOYEEDETAIL();
        }

        /// <summary>
        /// 增加系统存储空间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddSpace_Click(object sender, RoutedEventArgs e)
        {
            CheckLoginUser();
        }

        #region mvc平台登录，打开菜单，待办任务，新闻，公文

        /// <summary>
        /// mvc自动登录，兼打开指定的菜单，待办任务，新闻或我的单据记录
        /// </summary>
        /// <param name="strmoduleid">菜单ID</param>
        /// <param name="stropttype">打开记录的类型</param>
        /// <param name="strmessageid">待办任务的记录ID</param>
        /// <param name="strconfig">待办任务对应表单的xml配置信息</param>
        /// <param name="strUid">员工的帐号ID(用户登录验证)</param>
        /// <param name="strUserName">员工的帐号（用于本地缓存的存储）</param>
        [ScriptableMember]
        public void StartSilverlight(string strmoduleid, string stropttype, string strmessageid, string strconfig, string strUid, string strUserName)
        {
            ibLogin.Visibility = System.Windows.Visibility.Collapsed;
            spAddSpace.Visibility = System.Windows.Visibility.Visible;
            btnAddSpace.IsEnabled = true;
            if (string.IsNullOrWhiteSpace(strUid) || string.IsNullOrWhiteSpace(stropttype) || string.IsNullOrWhiteSpace(strUserName))
            {
                txtUserMsg.Text = "关键信息未获取到，操作被阻止！";
                return;
            }


            //txtUserMsg.Text += "[" + strUid + ";" + stropttype + ";" + strUserName + "]";

            if (Application.Current.Resources["CurrentSysUserID"] != null)
            {
                Application.Current.Resources.Remove("CurrentSysUserID");
            }

            Application.Current.Resources.Add("CurrentSysUserID", strUid);

            if (Application.Current.Resources["username"] != null)
            {
                Application.Current.Resources.Remove("username");
            }
            Application.Current.Resources.Add("username", strUserName);

            //标明第一次打开
            if (Application.Current.Resources["isFirstOpen"] != null)
            {
                Application.Current.Resources.Remove("isFirstOpen");
            }
            Application.Current.Resources.Add("isFirstOpen", true);

            List<string> MvcSourcelist = new List<string>();
            MvcSourcelist.Add(strmoduleid);
            MvcSourcelist.Add(stropttype);
            MvcSourcelist.Add(strmessageid);
            MvcSourcelist.Add(strconfig);

            if (Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                Application.Current.Resources.Remove("MvcOpenRecordSource");
            }

            Application.Current.Resources.Add("MvcOpenRecordSource", MvcSourcelist);
            foreach (var q in MvcSourcelist)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("MvcOpenRecordSource:" + q);
            }
            //SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            if (IosManager.CheckeSpace())
            {
                CheckLoginUser();
            }
        }

        ///// <summary>
        ///// mvc打开指定的菜单，待办任务，新闻或我的单据记录
        ///// </summary>
        ///// <param name="strModuleid">模块ID</param>
        ///// <param name="strOptType">单据类型</param>
        ///// <param name="strMessageid">单据ID</param>
        ///// <param name="strConfig">单据配置信息</param>
        //[ScriptableMember]
        //public void ExchangeModule(string strModuleid, string strOptType, string strMessageid, string strConfig)
        //{
        //    try
        //    {
        //        SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("strModuleid:" + strModuleid
        //            +System.Environment.NewLine+ "strOptType:" + strOptType
        //             + System.Environment.NewLine + "strMessageid:" + strMessageid
        //              + System.Environment.NewLine + "strConfig:" + strConfig);
              
        //        SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();

        //        if (!IosManager.CheckeSpace())
        //        {
        //            MessageBox.Show("未增加独立存储空间，请关闭当前页面，重新登录！");
        //            ibLogin.Visibility = System.Windows.Visibility.Collapsed;
        //            spAddSpace.Visibility = System.Windows.Visibility.Visible;
        //            return;
        //        }

        //        if (Application.Current.Resources["CurrentSysUserID"] == null)
        //        {
        //            MessageBox.Show("用户登录信息异常，不能执行当前操作！");
        //            return;
        //        }

        //        if (!SMT.SAAS.Main.CurrentContext.AppContext.IsLoadingCompleted)
        //        {
        //            MessageBox.Show("系统加载未完成，不能执行当前操作！");
        //            return;
        //        }

        //        Type t = uMainPage.GetType();

        //        MethodInfo m = t.GetMethod("OpenModuleWithMVC");
        //        SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("开始调用："+t.Name+" OpenModuleWithMVC:");
              
        //        m.Invoke(uMainPage, new object[4] { strModuleid, strOptType, strMessageid, strConfig });
        //    }
        //    catch (Exception ex)
        //    {
        //        SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
        //        SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
        //    }
        //}

        /// <summary>
        ///  检查登录
        /// </summary>
        private void CheckLoginUser()
        {
            try
            {
                if (!IosManager.CheckeSpace())
                {
                    try
                    {
                        if (!IosManager.AddSpace())
                        {
                            MessageBox.Show("请增加独立存储空间，否则系统无法运行");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("增加独立存储空间出错，原因如下：" + ex.ToString());
                    }
                }

                if (!IosManager.ExistsFile(strApplicationPath))
                {
                    IosManager.CreatePath(strApplicationPath);
                    isFirstUser = true;
                }

                btnAddSpace.Visibility = System.Windows.Visibility.Collapsed;

                string strUid = string.Empty, strUserName = string.Empty;

                if (Application.Current.Resources["CurrentSysUserID"] != null)
                {
                    strUid = Application.Current.Resources["CurrentSysUserID"].ToString();
                }

                if (Application.Current.Resources["username"] != null)
                {
                    strUserName = Application.Current.Resources["username"].ToString();                
                }

                if (string.IsNullOrWhiteSpace(strUid) || string.IsNullOrWhiteSpace(strUserName))
                {
                    txtUserMsg.Text = "警告！用户信息异常，不能执行当前的操作请求。";
                    if (string.IsNullOrWhiteSpace(strUid))
                    {
                        AppContext.SystemMessage("传入的用户CurrentSysUserID为空");
                        AppContext.ShowSystemMessageText();
                    }
                    if (string.IsNullOrWhiteSpace(strUserName))
                    {
                        AppContext.SystemMessage("传入的用户username为空");
                        AppContext.ShowSystemMessageText();
                    }
                    return;
                }

                if (!App.AppSettings.Contains(USERKEY))
                {
                    App.AppSettings.Add(USERKEY, strUserName);
                    isFirstUser = true;
                }
                else
                {
                    App.AppSettings[USERKEY] = strUserName;
                }

                sysloginClinet.GetUserInfobyIDAsync(strUid);
                txtUserMsg.Text = "系统正在加载，请稍等......";
            }
            catch (Exception ex)
            {
                btnAddSpace.Visibility = System.Windows.Visibility.Visible;
                txtUserMsg.Text = "系统异常，请联系管理员";
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
        }

        /// <summary>
        /// 根据SysUserID，验证用户是否允许登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sysloginClinet_GetUserInfobyIDCompleted(object sender, GetUserInfobyIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(e.Error.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
            else
            {
                if (e.Result == null)
                {
                    txtUserMsg.Text = "警告！用户异常，不能执行当前请求。";
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("警告！用户不存在，不能执行当前请求。");
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                    return;
                }
                else
                {
                    isloading = true;
                    sysUser = new EmployeeInfoWS.V_UserLogin();
                    sysUser.EMPLOYEEID = e.Result.EMPLOYEEID;
                    sysUser.ISMANAGER = e.Result.ISMANAGER;
                    sysUser.SYSUSERID = e.Result.SYSUSERID;

                    //登录成功，获取员工信息
                    employeeInfoClient.getEmployeeInfobyLoginAsync(sysUser.EMPLOYEEID);

                    SMT.SaaS.LocalData.Tables.V_UserLogin us = new SaaS.LocalData.Tables.V_UserLogin();
                    us.UserName = sysUser.SYSUSERID;
                    us.EMPLOYEEID = sysUser.EMPLOYEEID;
                    us.ISMANAGER = sysUser.ISMANAGER;
                    us.SYSUSERID = sysUser.SYSUSERID;
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo = new LoginUserInfo();
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID = us.EMPLOYEEID;
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID = us.SYSUSERID;
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.IsManager = us.ISMANAGER == "1" ? true : false;
                }
            }
        }

        /// <summary>
        /// 获取员工信息事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EmployeeInfoClient_getEmployeeInfobyLoginCompleted(object sender, getEmployeeInfobyLoginCompletedEventArgs e)
        {
            dtend = DateTime.Now;
            string strmsg = "登录系统完成耗时： " + (dtend - dtstart).Milliseconds.ToString() + " 毫秒";
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);

            if (e.Error != null && !string.IsNullOrWhiteSpace(e.Error.Message))
            {
                txtUserMsg.Text = "获取员工信息错误,请联系管理员";
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(e.Error.Message); MessageBox.Show(e.Error.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
            else
            {
                if (e.Result != null)
                {
                    employee = e.Result;
                    employee.sysuser = sysUser;
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("已获取到employee信息");
                    txtUserMsg.Text = "获取员工信息成功，开始获取系统更新，请稍等......";
                    bool isAdmin = employee.sysuser.ISMANAGER == "1" ? true : false;
                    var postlist
                        = from ent in employee.EMPLOYEEPOSTS
                          orderby ent.ISAGENCY
                          select new SMT.SaaS.LocalData.V_EMPLOYEEPOSTBRIEF
                          {
                              EMPLOYEEPOSTID = ent.EMPLOYEEPOSTID,
                              POSTID = ent.POSTID,
                              PostName = ent.PostName,
                              DepartmentID = ent.DepartmentID,
                              DepartmentName = ent.DepartmentName,
                              CompanyID = ent.CompanyID,
                              CompanyName = ent.CompanyName,
                              ISAGENCY = ent.ISAGENCY,
                              POSTLEVEL = ent.POSTLEVEL
                          };

                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo = SMT.SAAS.Main.CurrentContext.Common.GetLoginUserInfo(
                            employee.EMPLOYEEID, employee.EMPLOYEENAME,
                            employee.EMPLOYEECODE, employee.EMPLOYEESTATE,
                            employee.sysuser.SYSUSERID, employee.OFFICEPHONE,
                            employee.SEX, postlist.ToList(),
                            employee.WORKAGE, employee.PHOTO, isAdmin);
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI = new List<V_UserPermissionUI>();
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName = Application.Current.Resources["username"].ToString();

                    if (isFirstUser == true)
                    {
                        txtUserMsg.Text = "员工信息验证通过，开始获取系统更新，请稍等......";
                        dllVersionUpdataCheck();
                    }
                    else
                    {
                        uMainPage = asmMain.CreateInstance("SMT.SAAS.Platform.Xamls.MVCMainPage") as UIElement;
                        AppContext.AppHost.SetRootVisual(uMainPage);
                    }
                }
                else
                {
                    txtUserMsg.Text = "获取员工信息出错，请联系管理员";
                    return;
                }
            }
        }
        #endregion
        
        #region 平台组件下载

        /// <summary>
        /// 组件下载更新检查
        /// </summary>
        private void dllVersionUpdataCheck()
        {
            string path = @"http://" + SMT.SAAS.Main.CurrentContext.Common.HostIP.ToString() + @"/ClientBin/DllVersion.xml?dt=" + DateTime.Now.Millisecond;
            webcDllVersion.OpenReadAsync(new Uri(path, UriKind.Absolute));
        }

        /// <summary>
        /// 组件下载完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webcDllVersion_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("下载版本文件出错，请联系管理员" + e.Error.ToString());
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                    return;
                }
                if (e.Result.Length < 1)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("获取服务器更新列表为空，请联系管理员");
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                    txtUserMsg.Text = "获取服务器更新列表出错，请联系管理员";
                    return;
                }
                txtUserMsg.Text = "获取服务器更新列表成功，正在获取更新......";
                //将读取服务器下载的Dll Version XML            
                StreamResourceInfo sXapSri = new StreamResourceInfo(e.Result, "text/xml");
                Stream manifestStream = sXapSri.Stream;
                string strVersionXmlServer = new StreamReader(manifestStream).ReadToEnd();
                //Linq to xml   manifestStream
                XElement dllVersionXElementServer = XDocument.Parse(strVersionXmlServer).Root;
                var dllVersionXElementlistServer = (from ent in dllVersionXElementServer.Elements().Elements()
                                                    select ent).ToList();

                //本地存在dllversion.xml文件
                if (IosManager.ExistsFile(dllVersionFilePath))
                {
                    //比较版本并下载新版本的dll及zip文件
                    IsolatedStorageFileStream xmlIsoStreamLocal = IosManager.GetFileStream(dllVersionFilePath);
                    StreamResourceInfo sXapSrilocal = new StreamResourceInfo(xmlIsoStreamLocal, "text/xml");
                    Stream versionXmlStreamLocal = sXapSrilocal.Stream;
                    string versionXmlLocal = new StreamReader(versionXmlStreamLocal).ReadToEnd();
                    xmlIsoStreamLocal.Close();
                    if (string.IsNullOrEmpty(versionXmlLocal))
                    {
                        needDownDllNames = (from ent in dllVersionXElementlistServer
                                            select ent.Attribute("Source").Value).ToList();

                        DownLoadDll(needDownDllNames);
                        return;
                    }
                    XElement deploymentRoot = XDocument.Parse(versionXmlLocal).Root;
                    var dllVersionglistlocal = (from assemblyParts in deploymentRoot.Elements().Elements()
                                                select assemblyParts).ToList();

                    var needUpdataDlls = from a in dllVersionXElementlistServer
                                         join b in dllVersionglistlocal
                                         on a.Attribute("Source").Value equals b.Attribute("Source").Value
                                         where a.Attribute("version").Value != b.Attribute("version").Value
                                         select a;
                    if (needUpdataDlls.Count() > 0)
                    {
                        txtUserMsg.Text = "系统检查到更新，正在更新本地程序，请稍等......";
                        needDownDllNames = (from ent in needUpdataDlls
                                            select ent.Attribute("Source").Value).ToList();

                        DownLoadDll(needDownDllNames);
                    }
                    else
                    {
                        //从本地加载系统
                        LoadData(strApplicationPath + @"/SMT.SAAS.Platform.xap");
                    }
                }
                else
                {
                    //从服务器下载所有包
                    needDownDllNames = (from ent in dllVersionXElementlistServer
                                        select ent.Attribute("Source").Value).ToList();

                    DownLoadDll(needDownDllNames);
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("更新系统出错，请联系管理员：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                txtUserMsg.Text = "系统更新错误，请联系管理员";
            }
            finally
            {
                //存储versxml到本地
                byte[] streambyte = new byte[e.Result.Length];
                e.Result.Seek(0, SeekOrigin.Begin);
                e.Result.Read(streambyte, 0, streambyte.Length);
                e.Result.Close();
                IosManager.CreateFile(strApplicationPath, "DllVersion.xml", streambyte);
            }
        }

        /// <summary>
        /// 下载组件
        /// </summary>
        /// <param name="dllXElements">待下载组件集合</param>
        private void DownLoadDll(List<string> dllXElements)
        {

            if (dllXElements.Count < 1)
            {
                //从本地加载系统
                LoadData(strApplicationPath + @"/SMT.SAAS.Platform.xap");
                return;
            }
            downloadDllName = dllXElements.FirstOrDefault();
            //txtUserMsg.Text = "正在获取更新,请稍等......";
            string path = @"http://" + SMT.SAAS.Main.CurrentContext.Common.HostIP.ToString() + @"/ClientBin/" + downloadDllName + "?dt=" + DateTime.Now.Millisecond;
            wDownloadDllClinet.OpenReadAsync(new Uri(path, UriKind.Absolute));
        }

        /// <summary>
        /// 组件下载进程完成事件
        /// </summary>
        /// <param name="sender">触发对象</param>
        /// <param name="e">返回结果</param>
        void DownloadDllClinet_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int iProgressValue = e.ProgressPercentage;
            txtUserMsg.Text = "正在下载更新：" + downloadDllName + " " + iProgressValue + "%";
        }

        /// <summary>
        /// 组件下载完成事件
        /// </summary>
        /// <param name="sender">触发对象</param>
        /// <param name="e">返回结果</param>
        void webcDownloadDll_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("服务器下载" + downloadDllName + "出错，请联系管理员" + e.Error.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
            try
            {
                #region 将下载的dll保存至本地
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("服务器下载" + downloadDllName + "完成");
                byte[] streambyte = new byte[e.Result.Length];
                e.Result.Read(streambyte, 0, streambyte.Length);
                e.Result.Close();
                IosManager.CreateFile(strApplicationPath, downloadDllName, streambyte);

                #endregion

                needDownDllNames.Remove(downloadDllName);
                List<string> newDlllist = needDownDllNames;
                DownLoadDll(newDlllist);
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("下载更新出错：" + downloadDllName + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                txtUserMsg.Text = "系统更新错误，请联系管理员";
            }
        }

        #endregion

        #region "webpart Xap包加载"

        /// <summary>
        /// 加载数据包
        /// </summary>
        /// <param name="strXapName">xap包名称</param>
        private void LoadData(string strXapName)
        {
            string sDllSourceName = string.Empty;
            AssemblyPart asmPart = null;
            List<XElement> deploymentParts = new List<XElement>();
            try
            {

                #region "直接加载xap包中的dll"
                dtstart = DateTime.Now;
                using (var store = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
                {
                    //打开本地xap包流
                    IsolatedStorageFileStream sXapfileStream = store.OpenFile(strXapName, FileMode.Open, FileAccess.Read);
                    if (sXapfileStream.Length == 0)
                    {
                        //IosManager.DeletFile(ApplicationPath, "SMT.SAAS.Platform.Main.xap");
                        List<string> dllNames = new List<string>();
                        dllNames.Add("SMT.SAAS.Platform.xap");
                        DownLoadDll(dllNames);
                    }

                    #region Original Code
                    StreamResourceInfo srXapSri = new StreamResourceInfo(sXapfileStream, "application/binary");
                    Stream manifestStream = Application.GetResourceStream(srXapSri, new Uri("AppManifest.xaml", UriKind.Relative)).Stream;

                    string appManifest = new StreamReader(manifestStream).ReadToEnd();
                    manifestStream.Close();
                    //Linq to xml   
                    XElement deploymentRoot = XDocument.Parse(appManifest).Root;
                    deploymentParts = (from assemblyParts in deploymentRoot.Elements().Elements()
                                       select assemblyParts).ToList();
                    //检测所有包是否在本地，不在，就从服务器上下载
                    bool canStart = true;
                    List<string> dllDelete = new List<string>();
                    foreach (XElement xElement in deploymentParts)
                    {
                        if (xElement.Attribute("Source").Value.Contains("zip")
                            && !IosManager.ExistsFile(strApplicationPath + @"/" + xElement.Attribute("Source").Value))
                        {
                            dllDelete.Add(xElement.Attribute("Source").Value);
                            canStart = false;
                        }
                    }
                    if (!canStart)
                    {
                        DownLoadDll(dllDelete);
                        return;
                    }

                    StreamResourceInfo streamInfo;
                    //Assembly assemblyViewModel = null;
                    string message = string.Empty;
                    foreach (XElement xElement in deploymentParts)
                    {
                        try
                        {
                            sDllSourceName = xElement.Attribute("Source").Value;
                            dtstart = DateTime.Now;
                            //setLoadmingMessage( "正在加载：" + DllSourceName);
                            if (!sDllSourceName.Contains("zip"))
                            {
                                //直接加载dll
                                asmPart = new AssemblyPart();
                                asmPart.Source = sDllSourceName;
                                streamInfo = Application.GetResourceStream(new StreamResourceInfo(sXapfileStream, "application/binary"), new Uri(sDllSourceName, UriKind.Relative));

                                if (sDllSourceName == "SMT.SAAS.Platform.dll")
                                {
                                    asmMain = asmPart.Load(streamInfo.Stream);
                                }
                                else
                                {
                                    var a = asmPart.Load(streamInfo.Stream);
                                    message = message + a.FullName + System.Environment.NewLine + "从DLL文件中直接加载程序集： "+a.FullName;
                                }
                                streamInfo.Stream.Close();
                            }
                            else
                            {
                                //加载zip包                               
                                //setLoadmingMessage("正在加载：" + DllSourceName);
                                if (sDllSourceName.Contains("zip"))
                                {
                                    //打开本地zip包流                
                                    IsolatedStorageFileStream zipfileStream = IosManager.GetFileStream(strApplicationPath + @"/" + sDllSourceName);
                                    streamInfo = Application.GetResourceStream(new StreamResourceInfo(zipfileStream, "application/binary"), new Uri(sDllSourceName.Replace("zip", "dll"), UriKind.Relative));
                                    asmPart = new AssemblyPart();
                                    asmPart.Source = sDllSourceName.Replace("zip", "dll");
                                    var a = asmPart.Load(streamInfo.Stream);
                                    streamInfo.Stream.Close();
                                    message = message + a.FullName + System.Environment.NewLine + "从Zip文件中加载程序集： " + a.FullName;
                                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("从Zip文件中加载程序集： " + a.FullName);

                                }
                            }
                            dtend = DateTime.Now;
                            string strmsg = "加载成功：" + sDllSourceName + " 加载耗时： " + (dtend - dtstart).Milliseconds.ToString() + " 毫秒";
                            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);
                        }
                        catch (Exception ex)
                        {
                            string strmsg = "加载失败：" + sDllSourceName + " 错误信息： " + ex.ToString();
                            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);
                            SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                            setLoadmingMessage("系统加载出错，请联系管理员");
                            return;
                        }
                    }
                    message = string.Empty; ;
                    #endregion
                }
                #endregion

                setLoadmingMessage("系统检测更新完毕,正在打开单据，请稍后...");
                
                //return;               
                if (isFirstUser == true)
                {
                    //MainPage = asmMain.CreateInstance("SMT.SAAS.Platform.Xamls.MainPage") as UIElement;
                    uMainPage = asmMain.CreateInstance("SMT.SAAS.Platform.Xamls.MVCMainPage") as UIElement;
                    if (uMainPage == null)
                    {
                        MessageBox.Show("系统加载错误，请清空silverlight缓存后再试，或联系管理员");
                        setLoadmingMessage("系统加载错误，请清空silverlight缓存后再试，或联系管理员");
                        return;
                    }
                    AppContext.AppHost.SetRootVisual(uMainPage);
                }                
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(sDllSourceName + " 加载系统出错：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
        }

        /// <summary>
        /// 设置登录消息
        /// </summary>
        /// <param name="message">待传递消息</param>
        private void setLoadmingMessage(string message)
        {
            this.Dispatcher.BeginInvoke(
                           delegate
                           {
                               txtUserMsg.Text = message;
                           });
        }
        #endregion
    }
}
