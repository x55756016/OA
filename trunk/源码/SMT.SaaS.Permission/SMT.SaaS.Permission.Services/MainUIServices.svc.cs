using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;

using SMT.SaaS.Permission.BLL;
using SMT_System_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using SMT.SaaS.Permission.DAL;
using SMT.SaaS.Permission.DAL.views;
using SMT.Foundation.Log;
using System.Collections.Generic;
using System.Text;

namespace SMT.SaaS.Permission.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class MainUIServices
    {
        /// <summary>
        /// 系统用户登录(手机版在使用此登录接口)
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="Pwd">用户密码：加密后的密码</param>
        /// <returns>根据平台需求返回对应的实体</returns>
        [OperationContract]
        public V_UserLogin UserLogin(string userName, string Pwd)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
                return bll.AddUserLoginHis(userName, Pwd, Ip);
            }
            //SysUserBLL bll = new SysUserBLL();
            
            //    string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
            //    return bll.GetUserInfoByLogin(userName, Pwd, Ip);
            
        }

        /// <summary>
        /// 系统用户登录
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="Pwd">用户密码：加密后的密码</param>
        /// <returns>根据平台需求返回对应的实体</returns>
        [OperationContract]
        public V_UserLogin SystemLogin(string userName, string Pwd)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                return bll.systemUserLogin(userName, Pwd);
            }
        }

        /// <summary>
        /// 登录成功后创建令牌
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        [OperationContract]
        public V_UserLogin UserLoginToTaken(string userName, string Pwd,ref string StrTaken)
        {
            //using (SysUserBLL bll = new SysUserBLL())
            //{
            //    string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
            //    return bll.GetUserInfoByLogin(userName, Pwd, Ip);
            //}
            SysUserBLL bll = new SysUserBLL();

            string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
            return bll.GetUserInfoByLoginToTaken(userName, Pwd, Ip, StrTaken);

        }
        /// <summary>
        /// 根据登录用户获取  获取其所拥有的系统 返回格式为 0,1,2
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        [OperationContract]
        public string GetSystemTypeByUserID(string UserID, ref string StrResult)
        {

            //using (SysUserRoleBLL RoleBll = new SysUserRoleBLL())
            //{
            //    return RoleBll.GetSystemTypeByUserID(UserID, ref StrResult);
            //}
            SysUserRoleBLL RoleBll = new SysUserRoleBLL();
            
                return RoleBll.GetSystemTypeByUserID(UserID, ref StrResult);
            
        }

        /// <summary>
        /// 用户注销
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="LoginRecordId"></param>
        /// <returns></returns>
        [OperationContract]
        public bool UserLoginOut(string UserID, string LoginRecordId)
        {
            //using (SysUserBLL bll = new SysUserBLL())
            //{
            //    string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
            //    return bll.LoginOut(UserID, LoginRecordId, Ip);
            //}
            SysUserBLL bll = new SysUserBLL();
            
                string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
                return bll.LoginOut(UserID, LoginRecordId, Ip);
            
        }
        // 在此处添加更多操作并使用 [OperationContract] 标记它们

        /// <summary>
        /// 通过sysuserid获取员工信息，若为null则此用户不存在
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [OperationContract]
        public V_UserLogin GetUserInfobyID(string userId)
        {
            using (SysUserBLL bll = new SysUserBLL())
            {
                string Ip = System.Web.HttpContext.Current.Request.UserHostAddress.ToString();
                var userInfo = bll.GetUserLoginInfo(userId,Ip); ;
                if (userInfo != null)
                {
                    V_UserLogin vUserInfo = new V_UserLogin()
                    {
                        SYSUSERID = userInfo.SYSUSERID,
                        EMPLOYEEID = userInfo.EMPLOYEEID,
                        ISMANAGER = userInfo.ISMANGER.ToString()
                    };
                    return vUserInfo;
                }
                else
                {
                    return null;
                }

            }
        }
    }
}
