using System;

namespace SMT.SAAS.Platform.Model.Interface
{
    public interface ILoginUser
    {

        event EventHandler<GetEntityEventArgs<UserLogin>> UserLoginCompleted;
        event EventHandler<ExecuteNoQueryEventArgs> UserLogOutCompleted;

        void UserLogin(string userName,string userPwd);
        void GetSystemCodes(string sysUserID, string result);
        void UserLogOut(string userID,string loginRecordID);
    }
}
