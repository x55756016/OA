using System;
using System.Linq;
using System.Windows.Navigation;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysUserInfoPage : BasePage
    {
        private static PermissionServiceClient ServiceClient = new PermissionServiceClient();//龙康才新增
        //PermissionServiceClient ServiceClient;
        PersonnelServiceClient PersonClient;
        private string tmpUserID = "";
        private T_SYS_USER tmpUser = new T_SYS_USER();
        private ObservableCollection<string> StrAddStaffList = new ObservableCollection<string>();  //获取员工时的ID数组
        public SysUserInfoPage()
        {
            ServiceClient = new PermissionServiceClient();
            PersonClient = new PersonnelServiceClient();
            InitializeComponent();
            ServiceClient.GetUserByIDCompleted += new EventHandler<GetUserByIDCompletedEventArgs>(ServiceClient_GetUserByIDCompleted);
            PersonClient.GetEmployeeDetailByIDsCompleted += new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
        }

        void personclient_GetEmployeeDetailByIDsCompleted(object sender, GetEmployeeDetailByIDsCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    
                    
                    if (e.Result != null)
                    {
                        //vemployeeObj = e.Result.ToList();
                        //vemployeeObj.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEECNAME
                        //this.tblCompany.Text = e.Result.ToList().FirstOrDefault().EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                        //this.tblDepartment.Text = e.Result.ToList().FirstOrDefault().EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        //this.tblPosition.Text = e.Result.ToList().FirstOrDefault().EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;

                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        void ServiceClient_GetUserByIDCompleted(object sender, GetUserByIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    tmpUser = e.Result;
                    tmpUserID = tmpUser.SYSUSERID;
                    this.tblName.Text = tmpUser.EMPLOYEENAME;
                    this.tblUserName.Text = tmpUser.USERNAME;
                    this.tblremark.Text = tmpUser.REMARK;
                    this.tblcode.Text = tmpUser.EMPLOYEECODE;
                    StrAddStaffList.Add(tmpUser.EMPLOYEEID);
                    PersonClient.GetEmployeeDetailByIDsAsync(StrAddStaffList);
                }
            }
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            tmpUserID = this.NavigationContext.QueryString.Count()==0 ? "" : this.NavigationContext.QueryString["userid"].ToString();
            ServiceClient.GetUserByIDAsync(tmpUserID);         
        }
    }
}
