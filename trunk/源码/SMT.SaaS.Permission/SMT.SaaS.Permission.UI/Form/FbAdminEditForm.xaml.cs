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
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Form
{
    public partial class FbAdminEditForm : UserControl, IEntityEditor
    {
        public FbAdminEditForm()
        {
            InitializeComponent();
        }
        PermissionServiceClient PermClient;
        V_EMPLOYEEVIEW empInfo = new V_EMPLOYEEVIEW();
        public FbAdminEditForm(V_EMPLOYEEVIEW vEmp)
        {
            InitializeComponent();
            empInfo = vEmp;
            InitParas();
        }

        string empId = string.Empty;//员工ID
        string postId = string.Empty;//岗位ID
        string deptId = string.Empty;//部门ID
        string corpId = string.Empty;//公司ID
        string corpName = string.Empty;//公司名称
        /// <summary>
        /// 初始化
        /// </summary>
        private void InitParas()
        {
            PermClient = new PermissionServiceClient();
            PermClient.UpdateFbAdminCompleted += new EventHandler<UpdateFbAdminCompletedEventArgs>(PermClient_UpdateFbAdminCompleted);
            if (empInfo != null)
            {
                string strInfo = empInfo.EMPLOYEECNAME + "-" + empInfo.POSTNAME + "-" + empInfo.DEPARTMENTNAME + "-" + empInfo.COMPANYNAME;
                this.tbName.Text = strInfo;
            }
        }

        void PermClient_UpdateFbAdminCompleted(object sender, UpdateFbAdminCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (string.IsNullOrWhiteSpace(e.Result))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "修改成功！");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(Convert.ToString(e.Error)));
            }
            RefreshUI(RefreshType);
        }

        /// <summary>
        /// 选择员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LookUp_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                     postId = post.ObjectID;
                    string postName = post.ObjectName;//岗位

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                     deptId = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                     corpId = corp.COMPANYID;
                     corpName = corp.CNAME;//公司

                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE emp = userInfo.ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    empId = emp.EMPLOYEEID;
                    T_HR_EMPLOYEEPOST empPost = emp.T_HR_EMPLOYEEPOST.Where(t => t.T_HR_POST.POSTID == postId).FirstOrDefault();
                    if (empPost == null)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "检测员工当前岗位异常，请重试",
                                                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }
                    //if (empPost.ISAGENCY == "1")
                    //{
                    //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "请选择员工主岗位",
                    //                                                     Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //    return;
                    //}
                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    lkEmployeeName.TxtLookUp.Text = StrEmployee;
                    lkEmployeeName.DataContext = emp;
                    ToolTipService.SetToolTip(lkEmployeeName.TxtLookUp, StrEmployee);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(empId) || string.IsNullOrWhiteSpace(postId) || string.IsNullOrWhiteSpace(deptId) || string.IsNullOrWhiteSpace(corpId))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("提示"), "获取更改的员工信息出错！");
                return;
            }
            T_SYS_FBADMIN fbadmin = new T_SYS_FBADMIN();
            fbadmin.FBADMINID = Guid.NewGuid().ToString();
            fbadmin.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbadmin.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            fbadmin.EMPLOYEEID = empId;
            fbadmin.EMPLOYEEPOSTID = postId;
            fbadmin.EMPLOYEEDEPARTMENTID = deptId;
            fbadmin.EMPLOYEECOMPANYID = corpId;
            fbadmin.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbadmin.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbadmin.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbadmin.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbadmin.ROLENAME = corpName + "预算配置员";
            fbadmin.ISCOMPANYADMIN = "1";
            fbadmin.ISSUPPERADMIN = "0";
            fbadmin.ADDUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            PermClient.UpdateFbAdminAsync(empInfo.EMPLOYEEID, empInfo.OWNERCOMPANYID,fbadmin);
        }
        #region IEntityEditor 成员

        public string GetTitle()
        {
            return "预算管理员配置";
        }

        public string GetStatus()
        {
            return "";
        }
        private RefreshedTypes RefreshType = RefreshedTypes.Close;
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "1":
                    RefreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        #endregion

    }
}
