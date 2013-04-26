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
        public event EventHandler<GetEntityListEventArgs<Model.UserMenu>> OnGetUserMenuCompleted;
        public event EventHandler<GetEntityEventArgs<Model.UserLogin>> OnGetUserInfoCompleted;
        public event EventHandler OnGetMenuPermissionCompleted;
        public event EventHandler<ExecuteNoQueryEventArgs> OnUpdateUserInfoCompleted;

        public event EventHandler<ExecuteNoQueryEventArgs> OnGetUserCustomerPermissionCompleted;

        private PermissionServiceClient _client = Client.BasicServices.PermissionClient;
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient _toolsClient = null;
        public static bool HasNewsPublish = false;

        public CommonServices()
        {
            _toolsClient = new Saas.Tools.PermissionWS.PermissionServiceClient();
            _client.GetSysLeftMenuFilterPermissionToNewFrameCompleted += new System.EventHandler<GetSysLeftMenuFilterPermissionToNewFrameCompletedEventArgs>(_client_GetSysLeftMenuFilterPermissionToNewFrameCompleted);
            _toolsClient.GetEntityPermissionByUserCompleted += new EventHandler<Saas.Tools.PermissionWS.GetEntityPermissionByUserCompletedEventArgs>(_toolsClient_GetEntityPermissionByUserCompleted);
            _client.GetUserInfoCompleted += new EventHandler<GetUserInfoCompletedEventArgs>(_client_GetUserInfoCompleted);
            _client.SysUserInfoUpdateCompleted += new EventHandler<SysUserInfoUpdateCompletedEventArgs>(_client_SysUserInfoUpdateCompleted);
            _client.SysUserInfoUpdateByUserIdandUsernameCompleted += new EventHandler<SysUserInfoUpdateByUserIdandUsernameCompletedEventArgs>(_client_SysUserInfoUpdateByUserIdandUsernameCompleted);
            _toolsClient.GetCustomerPermissionByUserIDAndEntityCodeCompleted += new EventHandler<Saas.Tools.PermissionWS.GetCustomerPermissionByUserIDAndEntityCodeCompletedEventArgs>(_toolsClient_GetCustomerPermissionByUserIDAndEntityCodeCompleted);
        }

        #region 用户菜单与菜单权限
        public void GetUserMenu(string sysUserID)
        {
            _client.GetSysLeftMenuFilterPermissionToNewFrameAsync(sysUserID);
        }

        public void GetUserMenuPermission(string sysUserID, string menuid)
        {
            _toolsClient.GetEntityPermissionByUserAsync(sysUserID, menuid);
        }

        void _client_GetSysLeftMenuFilterPermissionToNewFrameCompleted(object sender, GetSysLeftMenuFilterPermissionToNewFrameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    ObservableCollection<Model.UserMenu> result = new ObservableCollection<UserMenu>();
                    foreach (var item in e.Result)
                    {
                        Model.UserMenu v = item.CloneObject<Model.UserMenu>(new Model.UserMenu());
                        result.Add(v);
                    }

                    if (OnGetUserMenuCompleted != null)
                        OnGetUserMenuCompleted(this, new GetEntityListEventArgs<Model.UserMenu>(result, e.Error));
                }

            }
        }

        void _toolsClient_GetEntityPermissionByUserCompleted(object sender, Saas.Tools.PermissionWS.GetEntityPermissionByUserCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    //if (Common.CurrentLoginUserInfo.PermissionInfoUI == null)
                    //{
                    //    Common.CurrentLoginUserInfo.PermissionInfoUI = new List<SMT.SaaS.LocalData.V_UserPermissionUI>();
                    //}

                    //if(Common.CurrentLoginUserInfo.PermissionInfoUI.Count == 0)
                    //{
                    //    foreach (var fent in e.Result.ToList())
                    //    {
                    //        SMT.SaaS.LocalData.V_UserPermissionUI tps = new SMT.SaaS.LocalData.V_UserPermissionUI();
                    //        tps = Common.CloneObject<SMT.Saas.Tools.PermissionWS.V_UserPermissionUI, SMT.SaaS.LocalData.V_UserPermissionUI>(fent, tps);
                    //        Common.CurrentLoginUserInfo.PermissionInfoUI.Add(tps);
                    //    }
                    //}

                    SavePermissionByLocal(e.Result);

                    if (OnGetMenuPermissionCompleted != null)
                    {
                        OnGetMenuPermissionCompleted(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// 保存权限到本地数据库
        /// </summary>
        private void SavePermissionByLocal(ObservableCollection<SMT.Saas.Tools.PermissionWS.V_UserPermissionUI> list)
        {
            if (list == null)
            {
                return;
            }

            List<V_UserPermUILocal> localUserPermUIs = new List<V_UserPermUILocal>();
            List<V_CustomerPermission> localCusPerms = new List<V_CustomerPermission>();
            List<V_PermissionValue> localPermValues = new List<V_PermissionValue>();
            List<V_OrgObject> localOrgObjs = new List<V_OrgObject>();

            string strEmployeeID = Common.CurrentLoginUserInfo.EmployeeID;
            foreach (var item in list)
            {
                if (item.MenuCode == null)
                {
                    continue;
                }

                V_UserPermUILocal info = item.CloneObject<V_UserPermUILocal>(new V_UserPermUILocal());
                info.UserModuleID = System.Guid.NewGuid().ToString();
                info.EmployeeID = Common.CurrentLoginUserInfo.EmployeeID;
                localUserPermUIs.Add(info);

                //如果自定义权限为空，就不用再继续向下轮询
                if (item.CustomerPermission == null)
                {
                    continue;
                }

                V_CustomerPermission cusPerm = item.CustomerPermission.CloneObject<V_CustomerPermission>(new V_CustomerPermission());
                cusPerm.UserModuleID = System.Guid.NewGuid().ToString();
                cusPerm.EmployeeID = strEmployeeID;
                cusPerm.PermissionUIID = info.UserModuleID;
                localCusPerms.Add(cusPerm);

                if (item.CustomerPermission.PermissionValue == null)
                {
                    continue;
                }

                if (item.CustomerPermission.PermissionValue.Count == 0)
                {
                    continue;
                }

                foreach (var p in item.CustomerPermission.PermissionValue)
                {
                    V_PermissionValue permValue = p.CloneObject<V_PermissionValue>(new V_PermissionValue());
                    permValue.UserModuleID = info.UserModuleID;
                    permValue.EmployeeID = strEmployeeID;
                    permValue.CusPermID = cusPerm.UserModuleID;

                    if (p.OrgObjects != null)
                    {
                        if (p.OrgObjects.Count > 0)
                        {
                            foreach (var d in p.OrgObjects)
                            {
                                V_OrgObject orgObj = d.CloneObject<V_OrgObject>(new V_OrgObject());
                                orgObj.UserModuleID = info.UserModuleID;
                                orgObj.EmployeeID = strEmployeeID;
                                orgObj.PermValueID = permValue.UserModuleID;

                                localOrgObjs.Add(orgObj);
                            }
                        }
                    }

                    localPermValues.Add(permValue);
                }                
            }

            V_UserPermUILocalVM.SaveV_UserPermUILocal(strEmployeeID, localUserPermUIs);
            V_CustomerPermissionVM.SaveV_CustomerPermission(strEmployeeID, localCusPerms);
            V_PermissionValueVM.SaveV_PermissionValue(strEmployeeID, localPermValues);
            V_OrgObjectVM.SaveV_OrgObject(strEmployeeID, localOrgObjs);
        }

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

        #region 验证用户与修改密码

        public void GetUserInfo(string userName)
        {
            _client.GetUserInfoAsync(userName);
        }

        public void UpdateUserInfo(string userID, string userName, string userPassword)
        {
            //T_SYS_USER userInfo = new T_SYS_USER()
            //{
            //    SYSUSERID=sysUserID,
            //    USERNAME = userName,
            //    PASSWORD = Encrypt(userPassword),
            //    UPDATEDATE = System.DateTime.Now
            //};

            _client.SysUserInfoUpdateByUserIdandUsernameAsync(userID, userName, Encrypt(userPassword));
        }

        void _client_GetUserInfoCompleted(object sender, GetUserInfoCompletedEventArgs e)
        {

            Model.UserLogin userLogin = new UserLogin();
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    userLogin.SysUserID = e.Result.SYSUSERID;
                    userLogin.EmployeeID = e.Result.EMPLOYEEID;
                    userLogin.UserName = e.Result.USERNAME;
                    userLogin.UserPassword = e.Result.PASSWORD;
                }
            }

            if (OnGetUserInfoCompleted != null)
                OnGetUserInfoCompleted(this, new GetEntityEventArgs<Model.UserLogin>(userLogin, e.Error));

        }
        void _client_SysUserInfoUpdateByUserIdandUsernameCompleted(object sender, SysUserInfoUpdateByUserIdandUsernameCompletedEventArgs e)
        {
            bool result = false;
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    result = e.Result;
                }
                if (OnUpdateUserInfoCompleted != null)
                    OnUpdateUserInfoCompleted(this, new ExecuteNoQueryEventArgs(result, e.Error));
            }
        }
        void _client_SysUserInfoUpdateCompleted(object sender, SysUserInfoUpdateCompletedEventArgs e)
        {

        }

        #endregion

        public string Encrypt(string input)
        {
            // 盐值
            string saltValue = "saltValue";
            // 密码值
            string pwdValue = "pwdValue";

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(input);
            byte[] salt = System.Text.UTF8Encoding.UTF8.GetBytes(saltValue);

            // AesManaged - 高级加密标准(AES) 对称算法的管理类
            System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged();

            // Rfc2898DeriveBytes - 通过使用基于 HMACSHA1 的伪随机数生成器，实现基于密码的密钥派生功能 (PBKDF2 - 一种基于密码的密钥派生函数)
            // 通过 密码 和 salt 派生密钥
            System.Security.Cryptography.Rfc2898DeriveBytes rfc = new System.Security.Cryptography.Rfc2898DeriveBytes(pwdValue, salt);

            /**/
            /*
         * AesManaged.BlockSize - 加密操作的块大小（单位：bit）
         * AesManaged.LegalBlockSizes - 对称算法支持的块大小（单位：bit）
         * AesManaged.KeySize - 对称算法的密钥大小（单位：bit）
         * AesManaged.LegalKeySizes - 对称算法支持的密钥大小（单位：bit）
         * AesManaged.Key - 对称算法的密钥
         * AesManaged.IV - 对称算法的密钥大小
         * Rfc2898DeriveBytes.GetBytes(int 需要生成的伪随机密钥字节数) - 生成密钥
         */

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称加密器对象
            System.Security.Cryptography.ICryptoTransform encryptTransform = aes.CreateEncryptor();

            // 加密后的输出流
            System.IO.MemoryStream encryptStream = new System.IO.MemoryStream();

            // 将加密后的目标流（encryptStream）与加密转换（encryptTransform）相连接
            System.Security.Cryptography.CryptoStream encryptor = new System.Security.Cryptography.CryptoStream
                (encryptStream, encryptTransform, System.Security.Cryptography.CryptoStreamMode.Write);

            // 将一个字节序列写入当前 CryptoStream （完成加密的过程）
            encryptor.Write(data, 0, data.Length);
            encryptor.Close();

            // 将加密后所得到的流转换成字节数组，再用Base64编码将其转换为字符串
            string encryptedString = Convert.ToBase64String(encryptStream.ToArray());

            return encryptedString;
        }
    }
}
