using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using SMT.SaaS.LocalData;

namespace SMT.SAAS.Main.CurrentContext
{
    public class Common
    {
        private static CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;
        public static AppConfig CurrentConfig = new AppConfig();
        public static LoginUserInfo CurrentLoginUserInfo = null;
        public static string HostIP = string.Empty;
        public static string HostAddress = string.Empty;
        public static int PageSize = 15;
        public static CultureInfo UiCulture
        {
            get { return uiCulture; }
            set { uiCulture = value; }
        }
        public static List<string> CacheMenuPermissionList = new List<string>();
        public static UserControl CurrentMainPage
        {
            get
            {
                Grid grid = Application.Current.RootVisual as Grid;
                if (grid != null && grid.Children.Count > 0)
                {
                    return grid.Children[0] as UserControl;
                }
                else
                    return null;
            }
        }
        public static List<T_SYS_ENTITYMENU> EntityMenu { get; set; }

        public static Panel ParentLayoutRoot
        {
            get;
            set;
        }

        public static LoginUserInfo GetLoginUserInfo(string userID, string userName, string employeeCode, string employeeState, string sysUserID,
            string mobilePhone,string officePhone, string sex, List<V_EMPLOYEEPOSTBRIEF> posts, int workAge, byte[] photo, bool isManager)
        {
            try
            {
                #region 获取用户岗位详细信息

                List<UserPost> userPosts = new List<UserPost>();

                foreach (var postItem in posts)
                {
                    UserPost userPost = new UserPost();
                    userPost.EmployeePostID = postItem.EMPLOYEEPOSTID;
                    userPost.IsAgency = postItem.ISAGENCY == "0" ? true : false;
                    userPost.PostLevel = postItem.POSTLEVEL;
                    userPost.PostID = postItem.POSTID;
                    userPost.CompanyID = postItem.CompanyID;
                    userPost.CompanyName = postItem.CompanyName;
                    userPost.DepartmentID = postItem.DepartmentID;
                    userPost.DepartmentName = postItem.DepartmentName;
                    userPost.PostID = postItem.POSTID;
                    userPost.PostName = postItem.PostName;
                    userPosts.Add(userPost);
                }
                #endregion
                LoginUserInfo loginUserInfo = new LoginUserInfo()
                  {
                      EmployeeID = userID,
                      EmployeeName = userName,
                      EmployeeCode = employeeCode,
                      EmployeeState = employeeState,
                      SysUserID = sysUserID,
                      Photo = photo,
                      SexID = sex,
                      WorkingAge = workAge,
                      OfficeTelphone = officePhone,
                      MobileTelphone=mobilePhone,
                      Telphone =officePhone==null?mobilePhone:mobilePhone + "/" + officePhone,
                      IsManager = isManager,
                      UserPosts = userPosts
                  };
                return loginUserInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据跟定的两个已知对象TSource，TTarget
        /// 将TSource对象中的数据克隆到TTarget中。
        /// TSource与TTarget可以为不同类型，其仅会拷贝具有相同名称和数据类型的属性值。
        /// </summary>
        /// <typeparam name="TSource">数据源类型</typeparam>
        /// <typeparam name="TTarget">数据目标类型</typeparam>
        /// <param name="Source">数据源</param>
        /// <param name="Target">数据目标</param>
        /// <returns>已包含赋值数据的结果</returns>
        public static TTarget CloneObject<TSource, TTarget>(TSource Source, TTarget Target)
        {
            try
            {
                if (Source == null)
                    throw new ArgumentNullException("Source");
                if (Target == null)
                    throw new ArgumentNullException("Target");

                PropertyInfo[] sourcePropertys = Source.GetType().GetProperties();
                PropertyInfo[] targetPropertys = Target.GetType().GetProperties();
                foreach (PropertyInfo sourcePro in sourcePropertys)
                {
                    var targetPro = targetPropertys.Where<PropertyInfo>(delegate(PropertyInfo proInfo)
                    {
                        return proInfo.Name.ToLower() == sourcePro.Name.ToLower() &&
                               proInfo.PropertyType == sourcePro.PropertyType;
                    }).FirstOrDefault<PropertyInfo>();

                    if (targetPro != null)
                        targetPro.SetValue(Target, sourcePro.GetValue(Source, null), null);
                }
                return Target;
            }
            catch (Exception ex)
            {
                throw new Exception("对象克隆异常", ex);
            }
        }
    }
}