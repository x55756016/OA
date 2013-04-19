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
using System.Resources;
using System.Reflection;
using System.Threading;
using System.Globalization;

using SMT.SaaS.Globalization;


namespace SMT.SaaS.OA.UI
{
    //public class Common
    //{
    //    //public static readonly ResourceManager ResourceMgr = new ResourceManager("SMT.SaaS.OA.UI.Assets.Resources.OASystem", Assembly.GetExecutingAssembly());
    //    ////public static readonly ResourceManager ResourceMgr = new ResourceManager("SMT.SaaS.Globalization.Resource", Assembly.GetExecutingAssembly());

    //    private static CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;
    //    public static AppConfig CurrentConfig = new AppConfig();
    //    public static LoginUserInfo loginUserInfo = null;

    //    public static CultureInfo UiCulture
    //    {
    //        get { return uiCulture; }
    //        set { uiCulture = value; }
    //    }

    //    public static MainPage CurrentMainPage
    //    {
    //        get
    //        {
    //            Grid grid = Application.Current.RootVisual as Grid;
    //            if (grid != null && grid.Children.Count > 0)
    //            {
    //                return grid.Children[0] as MainPage;
    //            }
    //            else
    //                return null;
    //        }
    //    }

    //    public static Panel ParentLayoutRoot
    //    {
    //        get;
    //        set;
    //    }

    //    public static LoginUserInfo GetLoginUserInfo()
    //    {
    //        try
    //        {
    //            if (Common.CurrentConfig.CurrentUser.EmployeeInfo != null && CurrentConfig.CurrentUser.UserInfo != null)
    //            {
    //                LoginUserInfo employee = new LoginUserInfo(
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEEID,
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.POSTID,
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME,
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
    //                        Common.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME
    //                    );
    //                return employee;
    //            }
    //            return null;
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }            
    //    }

    //    public static SMT.SaaS.OA.UI.SmtOAServiceAdminWS.LoginUserInfo GetLoginUser()
    //    {
    //        SMT.SaaS.OA.UI.SmtOAServiceAdminWS.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAServiceAdminWS.LoginUserInfo();
    //        loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
    //        loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
    //        return loginUserInfo;
    //    }

    //    public static void Userlogin(LoginUserInfo loginuser)
    //    {
    //        if (loginuser == null)
    //        {
    //            //Utility.ShowCustomMessage(MessageTypes.Caution, "重新登录", "您还未登录，请重新登录");
    //            MessageBox.Show("您还没登录，请重新登录");
    //            //SMT.SaaS.OA.UI.Views.Login.Login login = new SMT.SaaS.OA.UI.Views.Login.Login();
    //            SMT.SaaS.OA.UI.MainPage page = new MainPage();
    //            page.ContentFrame.Navigate(new Uri("/Views/Login/Login", UriKind.Relative));
                                
    //        }
    //    }
    //}

    //public class LoginUserInfo
    //{
    //    public string userID { get; set; }                   //用户ID
    //    public string postID { get; set; }                   //岗位ID
    //    public string departmentID { get; set; }             //部门ID
    //    public string companyID { get; set; }                //公司ID
    //    public string userName { get; set; }                 //用户姓名
    //    public string postName { get; set; }                 //岗位名称
    //    public string departmentName { get; set; }           //部门名称
    //    public string companyName { get; set; }              //公司名称

    //    public LoginUserInfo(string userID, string postID, string departmentID, string companyID,
    //                    string userName, string postName, string departmentName, string companyName)                        
    //    {
    //        this.userID = userID;
    //        this.postID = postID;
    //        this.departmentID = departmentID;
    //        this.companyID = companyID;
    //        this.userName = userName;
    //        this.postName = postName;
    //        this.departmentName = departmentName;
    //        this.companyName = companyName;
    //    }
    //}

    



}
