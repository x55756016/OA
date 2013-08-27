using System;
using SMT.SAAS.Platform.Client;
using SMT.SAAS.Platform.Client.UserLoginWS;
using System.Collections.ObjectModel;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Model.Services
{
    /// <summary>
    /// 用户服务管理。
    /// 封装对权限服务的接口调用。
    /// </summary>
    public class LoginUser : Interface.ILoginUser
    {
        private MainUIServicesClient _client = new MainUIServicesClient();
        private string result = string.Empty;
        private UserLogin _userLogin;

        #region 事件

        public event EventHandler<ExecuteNoQueryEventArgs> UserLogOutCompleted;
        public event EventHandler<GetEntityEventArgs<UserLogin>> UserLoginCompleted;
        public event EventHandler UserLoginFaild;
        #endregion


        public LoginUser()
        {
            _userLogin = new UserLogin();
            _client.UserLoginCompleted += new EventHandler<UserLoginCompletedEventArgs>(_client_UserLoginCompleted);
            _client.UserLoginOutCompleted += new EventHandler<UserLoginOutCompletedEventArgs>(_client_UserLoginOutCompleted);
            _client.GetSystemTypeByUserIDCompleted += new EventHandler<GetSystemTypeByUserIDCompletedEventArgs>(_client_GetSystemTypeByUserIDCompleted);

        }

        public void UserLogin(string userName, string userPwd)
        {
            _userLogin.UserName = userName;
            _userLogin.UserPassword = Utility.Encrypt(userPwd);
            
            _client.UserLoginAsync(_userLogin.UserName, _userLogin.UserPassword);
        }

        public void GetSystemCodes(string sysUserID, string result)
        {
            _client.GetSystemTypeByUserIDAsync(sysUserID, result);
        }

        public void UserLogOut(string userID, string loginRecordID)
        {
            _client.UserLoginOutAsync(userID, loginRecordID);
        }

        #region 服务端完成事件

        private void _client_UserLoginCompleted(object sender, UserLoginCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    _userLogin.EmployeeID = e.Result.EMPLOYEEID;
                    _userLogin.IsManager = e.Result.ISMANAGER;
                    _userLogin.LoginRecordID = e.Result.LOGINRECORDID;
                    _userLogin.SysUserID = e.Result.SYSUSERID;

                    this.GetSystemCodes(_userLogin.SysUserID, result);
                }
                else 
                {
                    if (UserLoginCompleted != null)
                    {
                        UserLoginFaild(this, EventArgs.Empty);
                    }
                }

            }
        }

        private void _client_GetSystemTypeByUserIDCompleted(object sender, GetSystemTypeByUserIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.StrResult.Length <= 0)
                {
                    #region 获取用户具有的系统编号
                    string[] codelist = e.Result.Split(',');//"1,2,3,4"

                    _userLogin.SystemCode = codelist;
                    Utility.UserLogin = _userLogin;
                    #endregion
                    if (UserLoginCompleted != null)
                    {
                        UserLoginCompleted(this, new GetEntityEventArgs<UserLogin>(_userLogin, e.Error));
                    }
                }
            }
           
        }

        private void _client_UserLoginOutCompleted(object sender, UserLoginOutCompletedEventArgs e)
        {

        }
        #endregion
    }
}
