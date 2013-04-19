using System;
using System.Net;
using System.Threading;
using System.Globalization;

namespace SMT.HRM.BLL
{
    public class CommonUserInfo
    {

        private static CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;
        public static AppConfig CurrentConfig = new AppConfig();
        public static LoginUserInfo loginUserInfo = null;
        public static string IPAddress = string.Empty;
        public static int PageSize = 15;
        public static string EmployeeID = string.Empty;
        public static CultureInfo UiCulture
        {
            get { return uiCulture; }
            set { uiCulture = value; }
        }

        //public static UserControl CurrentMainPage
        //{
        //    get
        //    {
        //        Grid grid = Application.Current.RootVisual as Grid;
        //        if (grid != null && grid.Children.Count > 0)
        //        {
        //            return grid.Children[0] as UserControl;
        //        }
        //        else
        //            return null;
        //    }
        //}

        //public static Panel ParentLayoutRoot
        //{
        //    get;
        //    set;
        //}

        public static LoginUserInfo GetLoginUserInfo()
        {
            try
            {
                if (CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo != null && CurrentConfig.CurrentUser.UserInfo != null)
                {
                    LoginUserInfo employee = new LoginUserInfo(
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEEID,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.POSTID,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                            CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.OFFICEPHONE,
                             CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.PHOTO,
                              CommonUserInfo.CurrentConfig.CurrentUser.EmployeeInfo.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.SEX,
                              CommonUserInfo.CurrentConfig.CurrentUser.UserInfo.SYSUSERID

                        );
                    return employee;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}
