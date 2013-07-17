using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.AuditControl;

using OrganizationWS = SMT.Saas.Tools.OrganizationWS;

using Common = SMT.SAAS.Main.CurrentContext.Common;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeEntryAddForm : BaseForm, IClient
    {
        #region 初始化
        public FormTypes FormType { get; set; }
        SMTLoading loadbar = new SMTLoading();
        public T_HR_EMPLOYEEPOST EmployeePost { get; set; }//岗位信息 
        int employeeType = 0;
        public T_HR_EMPLOYEE Employee { get; set; }//员工信息
        private string companyID { get; set; }
        public T_SYS_USER SysUser { set; get; } //系统用户
        private T_HR_EMPLOYEEENTRY employeeEntry;//入职信息
        public T_HR_EMPLOYEEENTRY EmployeeEntry
        {
            get { return employeeEntry; }
            set
            {
                employeeEntry = value;
                this.DataContext = value;
            }
        }
        public bool IsEntryBefore { get; set; } //是否是离职再入职
        public string ComputerNo { get; set; }
        public string PensionCardID { get; set; }
        public string SocialServiceYear { get; set; }
        public EmployeeInfoForm eminfo;
        public bool canSave = true;

        PersonnelServiceClient client;
        PermissionServiceClient perclient;
        OrganizationWS.OrganizationServiceClient orclient;
        SMT.Saas.Tools.SalaryWS.SalaryServiceClient salaryCient;
        public EmployeeEntryAddForm(T_HR_EMPLOYEE ent, int employeeType)
        {
            InitializeComponent();

            Employee = ent;
            this.employeeType = employeeType;

            this.Loaded += (sender, args) =>
            {
                InitParas();
            };

        }

        private void InitParas()
        {
            PARENT.Children.Remove(loadbar);
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            client = new PersonnelServiceClient();

            client.EmployeeEntryAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeEntryAddCompleted);
            client.EmployeeAddCompleted += new EventHandler<EmployeeAddCompletedEventArgs>(client_EmployeeAddCompleted);
            client.EmployeeUpdateCompleted += new EventHandler<EmployeeUpdateCompletedEventArgs>(client_EmployeeUpdateCompleted);
            client.AddEmployeeEntryCompleted += new EventHandler<AddEmployeeEntryCompletedEventArgs>(client_AddEmployeeEntryCompleted);
            perclient = new PermissionServiceClient();
            perclient.SysUserInfoAddORUpdateCompleted += new EventHandler<SysUserInfoAddORUpdateCompletedEventArgs>(perclient_SysUserInfoAddORUpdateCompleted);
            orclient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            orclient.GetPostNumberCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostNumberCompletedEventArgs>(orclient_GetPostNumberCompleted);

            salaryCient = new Saas.Tools.SalaryWS.SalaryServiceClient();
            salaryCient.AddSalaryPasswordCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(salaryCient_AddSalaryPasswordCompleted);
            //员工入职
            EmployeeEntry = new T_HR_EMPLOYEEENTRY();
            //EmployeeEntry.T_HR_EMPLOYEE = Employee;
            EmployeeEntry.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
            EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID = Employee.EMPLOYEEID;
            EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECNAME = Employee.EMPLOYEECNAME;
            EmployeeEntry.EMPLOYEEENTRYID = Guid.NewGuid().ToString();
            EmployeeEntry.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            EmployeeEntry.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            EmployeeEntry.CREATEDATE = DateTime.Now;
            EmployeeEntry.ENTRYDATE = DateTime.Now;
            EmployeeEntry.ONPOSTDATE = DateTime.Now;
            EmployeeEntry.EDITSTATE = "0";

            EmployeeEntry.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            EmployeeEntry.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            EmployeeEntry.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            EmployeeEntry.OWNERID = Employee.EMPLOYEEID;

            //员工岗位
            EmployeePost = new T_HR_EMPLOYEEPOST();
            EmployeePost.EMPLOYEEPOSTID = Guid.NewGuid().ToString();
            //EmployeePost.T_HR_EMPLOYEE = Employee;
            EmployeePost.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
            EmployeePost.T_HR_EMPLOYEE.EMPLOYEEID = Employee.EMPLOYEEID;
            EmployeePost.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            EmployeePost.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            EmployeePost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString(); //岗位未审核通过 无效
            EmployeePost.CREATEDATE = DateTime.Now;
            EmployeePost.ISAGENCY = "0"; //非代理岗位

            EmployeeEntry.EMPLOYEEPOSTID = EmployeePost.EMPLOYEEPOSTID;

            //系统用户
            SysUser = new T_SYS_USER();
            SysUser.SYSUSERID = Guid.NewGuid().ToString();
            SysUser.STATE = "0";
            txtUser.Text = Employee.EMPLOYEEENAME.Trim();
            string strCarID = Employee.IDNUMBER;
            if (strCarID.Length > 6)
            {
                txtPwd.Password = "smt"+strCarID.Substring(strCarID.Length - 6);
            }
            else
            {
                txtPwd.Password = "smt"+strCarID;
            }
            txtPwd.IsEnabled = false;
            SysUser.CREATEUSER = Common.CurrentLoginUserInfo.EmployeeID;
            SysUser.CREATEDATE = DateTime.Now;
        }





        void salaryCient_AddSalaryPasswordCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                loadbar.Stop();
                OnUIRefreshed();
            }
            this.showEmployeeEntryForm();
        }


        #endregion

        #region  完成事件
        /// <summary>
        /// 添加系统用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void perclient_SysUserInfoAddORUpdateCompleted(object sender, SysUserInfoAddORUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();

            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("USERNAMEREPETION"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("USERNAMEREPETION"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    loadbar.Stop();
                }
                else
                {
                    // 员工档案
                    //string strMsg = "";
                    //if (employeeType == 0)
                    //{
                    //    Employee.EMPLOYEECODE = CreateCode();
                    //    client.EmployeeAddAsync(Employee, companyID, strMsg);
                    //}
                    //else
                    //{
                    //    client.EmployeeUpdateAsync(Employee, companyID, strMsg);
                    //}
                    //Employee.EMPLOYEEENAME = e.strMsg;
                    Employee.IDNUMBER = Employee.IDNUMBER.ToUpper();
                    client.AddEmployeeEntryAsync(Employee, EmployeeEntry, EmployeePost);

                    //添加域账户
                    //string[] orgnames = lkPost.TxtLookUp.Text.Split(new char[] { '-' });
                    //System.Collections.ObjectModel.ObservableCollection<string> orgnamesOrdered = new System.Collections.ObjectModel.ObservableCollection<string>();
                    //for (int i = orgnames.Length - 2; i > 0; i--)
                    //{
                    //    orgnamesOrdered.Add(orgnames[i].Trim());
                    //}
                    //Dictionary<string, string> paras = new Dictionary<string, string>();
                    //paras.Add("displayName", Employee.EMPLOYEECNAME);
                    //paras.Add("description", Employee.EMPLOYEECNAME);
                    //perclient.CreateNewUserAsync(orgnamesOrdered, SysUser.USERNAME, SysUser.USERNAME, SysUser.PASSWORD, paras);
                }

            }
        }
        /// <summary>
        ///  入职
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AddEmployeeEntryCompleted(object sender, AddEmployeeEntryCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
                //删除系统用户信息 未实现
            }
            else
            {
                if (e.Result == "SAVED")
                {
                    //添加薪资密码
                    salaryCient.AddSalaryPasswordAsync(Employee.EMPLOYEEID, Employee.EMPLOYEECNAME, SysUser.PASSWORD);
                    //添加社保档案 不在这里添加社保档案，员工入职审核通过后会发出社保档案
                    //string strMsg = string.Empty;
                    //T_HR_PENSIONMASTER pension = new T_HR_PENSIONMASTER();
                    //pension.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                    //pension.T_HR_EMPLOYEE.EMPLOYEEID = Employee.EMPLOYEEID;
                    //pension.PENSIONMASTERID = Guid.NewGuid().ToString();
                    //pension.COMPUTERNO = ComputerNo;
                    //pension.CARDID = PensionCardID;
                    //pension.SOCIALSERVICEYEAR = SocialServiceYear;
                    //pension.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    //pension.OWNERCOMPANYID = Employee.OWNERCOMPANYID;
                    //pension.OWNERDEPARTMENTID = Employee.OWNERDEPARTMENTID;
                    //pension.OWNERPOSTID = Employee.OWNERPOSTID;
                    //pension.OWNERID = Employee.OWNERID;
                    //pension.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                    //pension.EDITSTATE = ((int)EditStates.UnActived).ToString();
                    //client.PensionMasterAddAsync(pension, strMsg);
                    eminfo.saveResume();
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    loadbar.Stop();
                    OnUIRefreshed();
                }

            }
        }

        /// <summary>
        /// 新增员工档案 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeAddCompleted(object sender, EmployeeAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    loadbar.Stop();
                    return;
                }

                EmployeePost.POSTLEVEL = (cbxPostLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE;
                //添加员工入职和岗位信息
                client.EmployeeEntryAddAsync(EmployeeEntry, EmployeePost);
                salaryCient.AddSalaryPasswordAsync(Employee.EMPLOYEEID, Employee.EMPLOYEECNAME, SysUser.PASSWORD);
            }
        }

        /// <summary>
        /// 修改员工信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeUpdateCompleted(object sender, EmployeeUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    loadbar.Stop();
                    return;
                }
                if (IsEntryBefore == true)
                {
                    EmployeeEntry.REMARK = "离职未满6个月";
                }
                EmployeePost.POSTLEVEL = (cbxPostLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE;
                //添加员工入职和岗位信息
                client.EmployeeEntryAddAsync(EmployeeEntry, EmployeePost);
            }
        }

        /// <summary>
        /// 新增员工入职 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeEntryAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                loadbar.Stop();
            }
            else
            {

                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("EMPLOYEEENTRY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                loadbar.Stop();
                OnUIRefreshed();

            }
        }

        /// <summary>
        /// 获取岗位空缺
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orclient_GetPostNumberCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostNumberCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                lkPost.DataContext = null;
                lkPost.TxtLookUp.Text = string.Empty;
                //txtCompanyName.Text = string.Empty;
                //txtDepartment.Text = string.Empty;
                cbxPostLevel.SelectedItem = null;
            }
            else
            {
                if (e.Result <= 0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("POSTNUMBERFULL", "POST"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("POSTNUMBERFULL", "POST"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    //lkPost.TxtLookUp.Text = string.Empty;
                    lkPost.DataContext = null;
                    lkPost.TxtLookUp.Text = string.Empty;
                    //txtCompanyName.Text = string.Empty;
                    //txtDepartment.Text = string.Empty;
                    cbxPostLevel.SelectedItem = null;
                }

            }
        }
        #endregion

        #region 保存
        public void Save()
        {


            loadbar.Start();
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                loadbar.Stop();
                return;
            }
            if (string.IsNullOrEmpty(lkPost.TxtLookUp.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POST"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                loadbar.Stop();
                return;
            }
            if (cbxPostLevel.SelectedItem == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                loadbar.Stop();
                return;
            }
            if (string.IsNullOrEmpty(dpEntryDate.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENTRYDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "ENTRYDATE"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                loadbar.Stop();
                return;
            }
            if (string.IsNullOrEmpty(dpOnPostDate.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ONPOSTDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "ONPOSTDATE"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                loadbar.Stop();
                return;
            }

            if (txtUser.Text == "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "USERNAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "USERNAME"),
      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                loadbar.Stop();
                return;
            }
            if (!string.IsNullOrEmpty(txtRemarks.Text))
            {
                if (!string.IsNullOrEmpty(EmployeeEntry.REMARK))
                {
                    EmployeeEntry.REMARK += "Ё" + txtRemarks.Text;
                }
                else
                {
                    EmployeeEntry.REMARK = txtRemarks.Text;
                }

            }
            SysUser.USERNAME = txtUser.Text.Trim();
            //SysUser.PASSWORD = SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(txtPwd.Password.Trim());
            //改密码的时候作判断
            //  SysUser.PASSWORD = txtPwd.Password.Trim();
            SysUser.REMARK = txtMark.Text;
            SysUser.EMPLOYEEID = Employee.EMPLOYEEID;
            SysUser.EMPLOYEECODE = Employee.EMPLOYEECODE;
            SysUser.EMPLOYEENAME = Employee.EMPLOYEECNAME;
            SysUser.OWNERID = Employee.EMPLOYEEID;
            SysUser.ISMANGER = 0;
            SysUser.OWNERPOSTID = Employee.OWNERPOSTID;
            SysUser.OWNERDEPARTMENTID = Employee.OWNERDEPARTMENTID;
            SysUser.OWNERCOMPANYID = Employee.OWNERCOMPANYID;
            EmployeePost.POSTLEVEL = (cbxPostLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE;
            #region
            // Employee.EMPLOYEECODE = CreateCode();
            //检查系统用户名是否重复
            //if (employeeType == 0)
            //{
            //    perclient.IsExistSysUserInfoAsync(SysUser.USERNAME);
            //}
            //else
            //{
            //    string strMsg = "";
            //    EmployeeEntry.REMARK = "离职未满6个月";
            //    perclient.SysUserInfoAddORUpdateAsync(SysUser, strMsg);
            //}

            #endregion
            //添加系统用户名   
            string strMsg = "";
            if (txtPwd.Password != null && CheckPwd(txtPwd.Password.Trim()))
            {
                SysUser.PASSWORD = SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(txtPwd.Password.Trim());
                perclient.SysUserInfoAddORUpdateAsync(SysUser, strMsg);
                canSave = true;
            }
            else
            {
                canSave = false;
                loadbar.Stop();
            }

        }

        /// <summary>
        /// 打开员工入职表单EmployeeEntryForm
        /// </summary>
        private void showEmployeeEntryForm()
        {
            try
            {
                EmployeeEntryForm form = new EmployeeEntryForm(FormTypes.Edit, EmployeeEntry.EMPLOYEEENTRYID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.MinHeight = 300;
                browser.FormType = FormTypes.Edit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            catch
            {
                //不用处理
            }
        }

        /// <summary>
        /// added by luojie
        /// 用与验证密码的合法性 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool CheckPwd(string password)
        {
            bool legalPwd = true;
            string rstMessage = string.Empty;
            if (password != null)
            {
                if (password.Length < 8)
                {
                    legalPwd = false;
                    rstMessage = "密码不能小于8位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                if (password.Length > 15)
                {
                    legalPwd = false;
                    rstMessage = "密码不能大于15位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                string ptnNum = @"\D[0-9]+";
                string ptnWord = @"[0-9][a-z_A-Z]+";
                Match matchNum = Regex.Match(password, ptnNum);
                Match matchWord = Regex.Match(password, ptnWord);
                if (!matchNum.Success && !matchWord.Success)
                {
                    legalPwd = false;
                    rstMessage = "密码必须是8-15位的英文与数字组合";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码必须是中英文结合的");
                }
                if (!string.IsNullOrWhiteSpace(rstMessage)) Utility.ShowCustomMessage(MessageTypes.Error, "错误", rstMessage);
            }
            else
            {
                legalPwd = false;
            }
            return legalPwd;
        }
        #endregion
        #region
        public delegate void refreshGridView();
        public event refreshGridView OnUIRefreshed;

        public void RefreshUI()
        {
            if (OnUIRefreshed != null)
            {
                OnUIRefreshed();
            }
        }

        #endregion

        #region 选择岗位
        private void lkPost_FindClick(object sender, EventArgs e)
        {
            //OrganizationLookupForm lookup = new OrganizationLookupForm();
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                OrganizationWS.T_HR_POST ent = lookup.SelectedObj[0].ObjectInstance as OrganizationWS.T_HR_POST;

                if (ent != null)
                {
                    HandlePostChanged(ent);
                    orclient.GetPostNumberAsync(ent.POSTID);

                }
            };

            lookup.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void HandlePostChanged(OrganizationWS.T_HR_POST ent)
        {
            lkPost.DataContext = ent;
            T_HR_POST temp = new T_HR_POST();
            temp.POSTID = ent.POSTID;
            EmployeePost.T_HR_POST = temp;
            companyID = ent.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            //txtCompanyName.Text = ent.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
            //txtDepartment.Text = ent.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT fartherdep = ent.T_HR_DEPARTMENT;
            string orgname = ent.T_HR_POSTDICTIONARY.POSTNAME + GetFullOrgName(fartherdep);
            lkPost.TxtLookUp.Text = orgname;
            SysUser.OWNERCOMPANYID = ent.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            SysUser.OWNERDEPARTMENTID = ent.T_HR_DEPARTMENT.DEPARTMENTID;
            SysUser.OWNERPOSTID = ent.POSTID;

            Employee.OWNERCOMPANYID = companyID;
            Employee.OWNERDEPARTMENTID = ent.T_HR_DEPARTMENT.DEPARTMENTID;
            Employee.OWNERPOSTID = ent.POSTID;
            Employee.OWNERID = Employee.EMPLOYEEID;

            EmployeeEntry.OWNERCOMPANYID = companyID;
            EmployeeEntry.OWNERDEPARTMENTID = ent.T_HR_DEPARTMENT.DEPARTMENTID;
            EmployeeEntry.OWNERPOSTID = ent.POSTID;

            foreach (T_SYS_DICTIONARY item in cbxPostLevel.Items)
            {
                if (item.DICTIONARYVALUE == ent.POSTLEVEL)
                {
                    cbxPostLevel.SelectedItem = item;
                    break;
                }
            }
        }

        public string GetFullOrgName(SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT dep)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = dep;
            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
            string orgName = string.Empty;
            string fatherType = "0";
            string fatherID = "";
            bool hasFather = false;

            if (department != null)
            {
                orgName += " - " + department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                {
                    fatherType = department.FATHERTYPE;
                    fatherID = department.FATHERID;
                    hasFather = true;
                }
                else
                {
                    hasFather = false;
                }
            }

            while (hasFather)
            {
                if (fatherType == "1" && !string.IsNullOrEmpty(fatherID))
                {
                    department = (from de in allDepartments
                                  where de.DEPARTMENTID == fatherID
                                  select de).FirstOrDefault();
                    if (department != null)
                    {
                        orgName += " - " + department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                        {
                            fatherID = department.FATHERID;
                            fatherType = department.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }
                }
                else if (fatherType == "0" && !string.IsNullOrEmpty(fatherID))
                {
                    company = (from com in allCompanys
                               where com.COMPANYID == fatherID
                               select com).FirstOrDefault();

                    if (company != null)
                    {
                        orgName += " - " + company.CNAME;
                        if (!string.IsNullOrEmpty(company.FATHERTYPE) && !string.IsNullOrEmpty(company.FATHERID))
                        {
                            fatherID = company.FATHERID;
                            fatherType = company.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }

                }
                else
                {
                    hasFather = false;
                }

            }
            return orgName;
        }
        #endregion
        #region 生成员工编码的前五位
        /// <summary>
        /// 生成员工编码的前五位
        /// </summary>
        /// <returns></returns>
        string CreateCode()
        {
            string employeeCode = string.Empty;
            DateTime EntryDate = Convert.ToDateTime(EmployeeEntry.ENTRYDATE);
            int year = EntryDate.Year;
            int month = EntryDate.Month;
            int day = EntryDate.Day;
            if (month <= 9)
            {
                if (day <= 9)
                {
                    employeeCode += "2";
                }
                else if (day <= 19)
                {
                    employeeCode += "3";
                }
                else if (day <= 29)
                {
                    employeeCode += "4";
                }
                else if (day <= 31)
                {
                    employeeCode += "5";
                }
                employeeCode += month.ToString();
            }
            else
            {
                if (day <= 9)
                {
                    employeeCode += "6";
                }
                else if (day <= 19)
                {
                    employeeCode += "7";
                }
                else if (day <= 29)
                {
                    employeeCode += "8";
                }
                else if (day <= 31)
                {
                    employeeCode += "9";
                }
                employeeCode += (1 + month % 10).ToString();
            }
            if (day <= 9)
            {
                employeeCode += day.ToString();
            }
            else
            {
                employeeCode += ((day / 10 + day % 10) % 10).ToString();
            }
            employeeCode += ((year - 1995) / 10).ToString() + ((year - 1995) % 10).ToString();
            return employeeCode;
        }
        #endregion
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            perclient.DoClose();
            orclient.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
