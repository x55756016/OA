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
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.Generic;
using System.Linq;


namespace SMT.SaaS.Permission.UI.UserControls
{
    public partial class UserRoleApplyForm : UserControl, IClient, IEntityEditor, IAudit
    {

        #region 初始化所属参数
        
        string OwnerCompanyid = "";//所属公司ID
        string OwnerDepartmentid = "";//所属部门ID        
        string Ownerid = "";//所属员工ID
        string postLevel = "";//岗位级别
        #endregion

        #region 初始化申请人信息
        private void InitApplyerInfo()
        {
            Ownerid = Common.CurrentLoginUserInfo.EmployeeID;
            string StrName = Common.CurrentLoginUserInfo.EmployeeID + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            //txtOwnerName.Text = StrName;
            //ToolTipService.SetToolTip(txtOwnerName,StrName);
        }
        #endregion

        #region 申请人

        private void btnLookUpPartyb_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    string depName = "";
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位
                    postLevel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault().POSTLEVEL.ToString();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门
                    depName = dept.ObjectName;//部门

                    OwnerDepartmentid = deptid;

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司
                    OwnerCompanyid = corpid;

                    Ownerid = userInfo.ObjectID;
                    roleInfo.OWNERCOMPANYID = corpid;
                    roleInfo.OWNERDEPARTMENTID = deptid;
                    roleInfo.OWNERID = userInfo.ObjectID;
                    roleInfo.CREATEUSERNAME = userInfo.ObjectName;
                    roleInfo.OWNERPOSTID = postid;
                    //txtOwnerName.Text = userInfo.ObjectName;



                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    //txtOwnerName.Text = StrEmployee;
                    //txtTel.Text = userInfo.te
                    //ToolTipService.SetToolTip(txtOwnerName, StrEmployee);

                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }
        #endregion

        #region 获取员工信息

        void personnelClient_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    postLevel = e.Result.EMPLOYEEPOSTS[0].POSTLEVEL.ToString();
                    string PostName = "";
                    string DepartmentName = "";
                    string CompanyName = "";
                    string StrName = "";
                    PostName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    DepartmentName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    StrName = e.Result.T_HR_EMPLOYEE.EMPLOYEECNAME + "-" + PostName + "-" + DepartmentName + "-" + CompanyName;
                    //txtOwnerName.Text = StrName;
                    //ToolTipService.SetToolTip(txtOwnerName, StrName);
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
        }
        #endregion

    }
}
