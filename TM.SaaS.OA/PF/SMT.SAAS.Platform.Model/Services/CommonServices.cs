using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.LocalData.Tables;
using SMT.SaaS.LocalData.ViewModel;

namespace SMT.SAAS.Platform.Model.Services
{
    /// <summary>
    /// 提供公共服务的支持。比如：权限服务。
    /// </summary>
    public class CommonServices
    {
        //public event EventHandler<GetEntityListEventArgs<Model.UserMenu>> OnGetUserMenuCompleted;
        //public event EventHandler<GetEntityEventArgs<Model.UserLogin>> OnGetUserInfoCompleted;
        public event EventHandler OnGetMenuPermissionCompleted;
        public event EventHandler<ExecuteNoQueryEventArgs> OnUpdateUserInfoCompleted;

        public event EventHandler<ExecuteNoQueryEventArgs> OnGetUserCustomerPermissionCompleted;

        private PermissionServiceClient _client = new PermissionServiceClient();
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient _toolsClient = null;
        public static bool HasNewsPublish = false;

        public CommonServices()
        {
            _toolsClient = new Saas.Tools.PermissionWS.PermissionServiceClient();
            //_client.GetSysLeftMenuFilterPermissionToNewFrameCompleted += new System.EventHandler<GetSysLeftMenuFilterPermissionToNewFrameCompletedEventArgs>(_client_GetSysLeftMenuFilterPermissionToNewFrameCompleted);
            //_toolsClient.GetEntityPermissionByUserCompleted += new EventHandler<Saas.Tools.PermissionWS.GetEntityPermissionByUserCompletedEventArgs>(_toolsClient_GetEntityPermissionByUserCompleted);
            //_client.SysUserInfoUpdateCompleted += new EventHandler<SysUserInfoUpdateCompletedEventArgs>(_client_SysUserInfoUpdateCompleted);
            //_client.SysUserInfoUpdateByUserIdandUsernameCompleted += new EventHandler<SysUserInfoUpdateByUserIdandUsernameCompletedEventArgs>(_client_SysUserInfoUpdateByUserIdandUsernameCompleted);
            _toolsClient.GetCustomerPermissionByUserIDAndEntityCodeCompleted += new EventHandler<Saas.Tools.PermissionWS.GetCustomerPermissionByUserIDAndEntityCodeCompletedEventArgs>(_toolsClient_GetCustomerPermissionByUserIDAndEntityCodeCompleted);
        }

        #region 用户菜单与菜单权限
  

        public void GetCustomPermission(string sysUserID, string menuID)
        {
            _toolsClient.GetCustomerPermissionByUserIDAndEntityCodeAsync(sysUserID, menuID);
        }

        void _toolsClient_GetCustomerPermissionByUserIDAndEntityCodeCompleted(object sender, Saas.Tools.PermissionWS.GetCustomerPermissionByUserIDAndEntityCodeCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                var v = e.Result;
                HasNewsPublish = true;
            }
            if (OnGetUserCustomerPermissionCompleted != null)
            {
                OnGetUserCustomerPermissionCompleted(this,null);
            }
        }


        #endregion
    }
}
