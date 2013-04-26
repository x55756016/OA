
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SAAS.Platform.Model.Services
{
    /// <summary>
    /// 常量字符串
    /// </summary>
    public static class ConstStrings
    {
        #region 独立存储文件名称
        public const string ISO_DICTIONARY_FILENAME = "DICTIONARY.OBJ";
        public const string ISO_COMPANYINFO_FILENAME = "COMPANYINFO.OBJ";
        public const string ISO_DEPARTMENTINFO_FILENAME = "DEPARTMENTINFO.OBJ";
        public const string ISO_POSTINFO_FILENAME = "POSTINFO.OBJ";
        public const string ISO_ENTITYMENU_FILENAME = "ENTITYMENU.OBJ";
        #endregion

        #region 应用程序资源字典集合KEY
        public const string RES_DICTIONARY_KEY = "SYS_DICTIONARY";
        public const string RES_COMPANYINFO_KEY = "SYS_CompanyInfo";
        public const string RES_DEPARTMENTINFO_KEY = "SYS_DepartmentInfo";
        public const string RES_POSTINFO_KEY = "SYS_PostInfo";
        public const string RES_ENTITYMENU_KEY = "SYS_EntityMenus";
        #endregion

    }
    public class SystemInit
    {
        public event EventHandler OnInitCompleted;
        public event EventHandler OnInitFailed;
        private CommonServices _commServices=null;

        public SystemInit()
        {
            RegisterServers();
            _commServices = new CommonServices();
        }

        //已经作废
        public void StartInit(string employeeID, string sysuserid, string isadmin, string loginRecord, string userName)
        {
            _UserEmployeeID = employeeID;
            _SysUserID = sysuserid;
            _IsAdmin = isadmin;
            _LoginRecord = loginRecord;
            _UserToken = userName;

            LoadCompanyInfo();
        }

        #region 私有成员 Private Member

        //组织架构服务
        private OrganizationServiceClient organClient = null;
        //人力资源用户服务
        private PersonnelServiceClient personelClient = null;
        //当前登录用户信息
        private T_SYS_USER _userModel = null;
        //当前登录用户信息
        private string _UserEmployeeID = string.Empty;
        private string _SysUserID = string.Empty;
        private string _IsAdmin = string.Empty;
        private string _LoginRecord = string.Empty;
        private string _UserToken = string.Empty;
        //当前登录用户职位信息
        private List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST> _userPostList = null;
        //当前最大职位
        #endregion

        #region 私有方法 Private Method
        private void RegisterServers()
        {
            organClient = new OrganizationServiceClient();
            personelClient = new PersonnelServiceClient();
            _userPostList = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEEPOST>();
            personelClient.GetEmployeeDetailViewByIDCompleted += new EventHandler<GetEmployeeDetailViewByIDCompletedEventArgs>(personelClient_GetEmployeeDetailViewByIDCompleted);

        }


        //存组织架构
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPosts;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;

        private void LoadCompanyInfo()
        {
            organClient.GetALLCompanyViewCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        List<SMT.Saas.Tools.OrganizationWS.V_COMPANY> entTemps = e.Result.ToList();
                        allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                        var ents = entTemps.OrderBy(c => c.FATHERID);
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
                                V_COMPANY v_company = entTemps.Where(s => s.COMPANYID == ent.FATHERCOMPANYID).FirstOrDefault();
                                if (v_company != null)
                                {
                                    company.T_HR_COMPANY2.CNAME = v_company.CNAME;
                                }
                                else
                                {
                                    //throw new Exception("v_company");
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
                        AddToResourceDictionary<List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>>(ConstStrings.RES_COMPANYINFO_KEY, allCompanys);
                        LoadDepartmentInfo();
                    }
                }

            };

            organClient.GetALLCompanyViewAsync(_UserEmployeeID);

        }
        private void LoadDepartmentInfo()
        {
            organClient.GetAllDepartmentViewCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        List<SMT.Saas.Tools.OrganizationWS.V_DEPARTMENT> entTemps = e.Result.ToList();
                        allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                        var ents = entTemps.OrderBy(c => c.FATHERID);
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
                        AddToResourceDictionary<List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>>(ConstStrings.RES_DEPARTMENTINFO_KEY, allDepartments);
                        LoadPostInfo();
                    }
                }
            };
            organClient.GetAllDepartmentViewAsync(_UserEmployeeID);
        }
        private void LoadPostInfo()
        {

            organClient.GetAllPostViewCompleted += (o, e) =>
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {

                        List<SMT.Saas.Tools.OrganizationWS.V_POST> vpostList = e.Result.ToList();
                        allPosts = new List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>();
                        //把岗位视图集合转换为岗位实体集合
                        foreach (var ent in vpostList)
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
                        AddToResourceDictionary<List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>>(ConstStrings.RES_POSTINFO_KEY, allPosts);
                        // personelClient.GetEmployeeDetailViewByIDAsync(_UserEmployeeID);
                    }
                    //personelClient.GetEmployeeDetailViewByIDAsync(_UserEmployeeID);

                    //默认获取以下，用户是否有发新闻的权限

                    //已经作废
                    _commServices.GetCustomPermission(_SysUserID, "NEWSPUBLISH");
                }
                else
                {
                    //errorPanel.Visibility = Visibility.Visible;
                    //loadingfoPanel.Visibility = Visibility.Collapsed;
                    //ExceptionManager.SendException(Resource.MSG_100014_WCF_ERROR, "PF", "LoadPostInfo", "1000014", e.Error);
                }
            };
            organClient.GetAllPostViewAsync(_UserEmployeeID);
        }

        private static void AddToResourceDictionary<T>(string key, T value)
        {
            try
            {
                if (Application.Current.Resources[key] == null)
                {
                    if (value != null)
                    {
                        if (Application.Current.Resources.Contains(key))
                            Application.Current.Resources.Remove(key);

                        Application.Current.Resources.Add(key, value);
                    }
                }
            }
            catch (Exception ex)
            {
                //ExceptionManager.SendException(Resource.MSG_100015_ISO_ERROR, "PF", "Convent", "1000015", ex);
            }
        }

        void personelClient_GetEmployeeDetailViewByIDCompleted(object sender, GetEmployeeDetailViewByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    V_EMPLOYEEDETAIL result = e.Result;
                    bool isAdmin = _IsAdmin == "1" ? true : false;
                    if (result.EMPLOYEEPOSTS == null)
                    {
                        result.EMPLOYEEPOSTS = new System.Collections.ObjectModel.ObservableCollection<V_EMPLOYEEPOSTBRIEF>();
                    }

                    //Common.CurrentLoginUserInfo = Common.GetLoginUserInfo(
                    //    _UserEmployeeID, result.EMPLOYEENAME,
                    //    result.EMPLOYEECODE, result.EMPLOYEESTATE,
                    //    _SysUserID, result.OFFICEPHONE,
                    //    result.SEX, result.EMPLOYEEPOSTS.ToList(), result.WORKAGE, result.PHOTO, isAdmin);
                    //Common.CurrentLoginUserInfo.LoginRecordID = _LoginRecord;
                    //Common.CurrentLoginUserInfo.UserName = Utility.UserLogin.UserName;
                    //Common.CurrentLoginUserInfo.UserPwd = Utility.UserLogin.UserPassword;
                    //AppConfig._CurrentStyleCode = 1;

                    //if (Application.Current.Resources["CurrentUserID"] == null)
                    //{
                    //    if (Application.Current.Resources.Contains("CurrentUserID"))
                    //        Application.Current.Resources.Remove("CurrentUserID");

                    //    Application.Current.Resources.Add("CurrentUserID", _UserEmployeeID);
                    //}
                  
                    if (this.OnInitCompleted != null)
                        this.OnInitCompleted(this, e);

                    //    //结束系统初始化过程
                    //    if (_Actived)
                    //    {
                    //        if (InitSystemCompleted.IsNotNull())
                    //        {
                    //            //触发系统初始化完成事件  
                    //            InitSystemCompleted(this, EventArgs.Empty);
                    //            if (!CurrentContext.SysInit)
                    //                CurrentContext.SysInit = true;

                    //            txbLodingInfo.Text = "系统初始化完毕!";
                    //        }
                    //        //关闭系统初始化开关
                    //        _Actived = false;
                    //    }
                }
            }
            else
            {

                if (this.OnInitFailed != null)
                    this.OnInitFailed(this, e);
                //errorPanel.Visibility = Visibility.Visible;
                //loadingfoPanel.Visibility = Visibility.Collapsed;
                //ExceptionManager.SendException(Resource.MSG_100014_WCF_ERROR, "PF", "LoadPostInfo", "1000014", e.Error);
            }

        }

        #endregion
    }
}

