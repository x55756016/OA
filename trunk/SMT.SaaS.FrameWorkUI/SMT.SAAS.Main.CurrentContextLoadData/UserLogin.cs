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

using SMT.Saas.Tools.PermissionWS;
using System.Linq;
using System.Collections.Generic;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.LocalData;

namespace SMT.SAAS.Main.CurrentContextLoadData
{
    public class UserLogin
    {
        private PermissionServiceClient client = new PermissionServiceClient();
        private OrganizationServiceClient organClient = new OrganizationServiceClient();
        private PersonnelServiceClient personelClient = new PersonnelServiceClient();
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string UserPwd { get; set; }
        /// <summary>
        /// 登录结果 成功返回
        /// </summary>

        public bool LoginResult { get; set; }
        /// <summary>
        /// 权限表用户ID
        /// </summary>
        public string SysUserID { get; set; }
        /// <summary>
        /// 员工ID
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 登录结果 成功返回
        /// </summary>
        public T_SYS_USER User { get; set; }
        List<SMT.Saas.Tools.PermissionWS.V_UserPermissionUI> Permission { get; set; }
        public event EventHandler LoginedClick;
        public UserLogin()
        {

        }
        public UserLogin(string userName, string userPwd)
        {

            this.UserName = userName;
            this.UserPwd = userPwd;
            client.UserLoginCompleted += new EventHandler<UserLoginCompletedEventArgs>(client_UserLoginCompleted);
            client.GetUserPermissionByUserToUICompleted += new EventHandler<GetUserPermissionByUserToUICompletedEventArgs>(client_GetUserPermissionByUserToUICompleted);

            personelClient.GetEmployeeDetailViewByIDCompleted += new EventHandler<GetEmployeeDetailViewByIDCompletedEventArgs>(personelClient_GetEmployeeDetailViewByIDCompleted);
            //client.UserLoginAsync(userName,UserPwd);
            client.UserLoginAsync(UserName, Encrypt(UserPwd));
            Permission = new List<SMT.Saas.Tools.PermissionWS.V_UserPermissionUI>();
        }

        void client_GetUserPermissionByUserToUICompleted(object sender, GetUserPermissionByUserToUICompletedEventArgs e)
        {
            if (e.Result != null)
            {
                Permission = e.Result.ToList();
                personelClient.GetEmployeeDetailViewByIDAsync(User.EMPLOYEEID);
            }
        }
        void personelClient_GetEmployeeDetailViewByIDCompleted(object sender, GetEmployeeDetailViewByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {

                SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEDETAIL epDetail = e.Result;

                List<UserPost> userPosts = new List<UserPost>();
                if (epDetail.EMPLOYEEPOSTS != null)
                {
                    foreach (var postItem in epDetail.EMPLOYEEPOSTS)
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
                }
                bool IsManager = false;
                if (User.ISMANGER == 1)
                {
                    IsManager = true;
                }
                //CurrentLoginUserInfo = Common.GetLoginUserInfo(epDetail.EMPLOYEEID, epDetail.EMPLOYEENAME, epDetail.EMPLOYEECODE, epDetail.EMPLOYEESTATE, User.SYSUSERID, epDetail.OFFICEPHONE, epDetail.SEX, epDetail.EMPLOYEEPOSTS.ToList(), epDetail.WORKAGE, epDetail.PHOTO, IsManager,User.USERNAME,User.PASSWORD);
                if (epDetail.EMPLOYEEPOSTS != null)
                {
                    var postlist
                          = from ent in epDetail.EMPLOYEEPOSTS
                            select new SMT.SaaS.LocalData.V_EMPLOYEEPOSTBRIEF
                            {
                                EMPLOYEEPOSTID = ent.EMPLOYEEPOSTID,
                                POSTID = ent.POSTID,
                                PostName = ent.PostName,
                                DepartmentID = ent.DepartmentID,
                                DepartmentName = ent.DepartmentName,
                                CompanyID = ent.CompanyID,
                                CompanyName = ent.CompanyName,
                                ISAGENCY = ent.ISAGENCY,
                                POSTLEVEL = ent.POSTLEVEL
                            };
                    Common.CurrentLoginUserInfo = Common.GetLoginUserInfo(epDetail.EMPLOYEEID, epDetail.EMPLOYEENAME, epDetail.EMPLOYEECODE, epDetail.EMPLOYEESTATE, User.SYSUSERID, epDetail.OFFICEPHONE, epDetail.SEX, postlist.ToList(), epDetail.WORKAGE, epDetail.PHOTO, IsManager);
                }
                else
                {
                    Common.CurrentLoginUserInfo = Common.GetLoginUserInfo(epDetail.EMPLOYEEID, epDetail.EMPLOYEENAME, epDetail.EMPLOYEECODE, epDetail.EMPLOYEESTATE, User.SYSUSERID, epDetail.OFFICEPHONE, epDetail.SEX, null, epDetail.WORKAGE, epDetail.PHOTO, IsManager);
                }
                 //var q= from ent in Permission
                 //       select new SMT.SaaS.LocalData.V_UserPermissionUI
                 //       {
                 //           DataRange=ent.DataRange,
                 //           PermissionValue=ent.PermissionValue,  
                 //           EntityMenuID=ent.EntityMenuID,
                 //           MenuCode=ent.MenuCode
                 //       };

                 //List<SMT.SaaS.LocalData.V_UserPermissionUI> perlist = new List<SMT.SaaS.LocalData.V_UserPermissionUI>();
                Common.CurrentLoginUserInfo.PermissionInfoUI = new List<SMT.SaaS.LocalData.V_UserPermissionUI>();                     
                 foreach (var fent in Permission)
                 {
                     SMT.SaaS.LocalData.V_UserPermissionUI tps= new SMT.SaaS.LocalData.V_UserPermissionUI();
                     tps=Common.CloneObject<SMT.Saas.Tools.PermissionWS.V_UserPermissionUI, SMT.SaaS.LocalData.V_UserPermissionUI>(fent, tps);
                     Common.CurrentLoginUserInfo.PermissionInfoUI.Add(tps);
                 //    tps.DataRange = fent.DataRange;
                 //    tps.PermissionValue = fent.PermissionValue;
                 //    tps.EntityMenuID = fent.EntityMenuID;
                 //    tps.MenuCode = fent.MenuCode;
                 //    tps.CustomerPermission.EntityMenuId = fent.CustomerPermission.EntityMenuId;

                 //           foreach (var fq in fent.CustomerPermission.PermissionValue)
                 //           {
                 //               SMT.SaaS.LocalData.PermissionValue pv 
                 //                   =new  SMT.SaaS.LocalData.PermissionValue();

                 //               tps.Permission=fq.Permission;

                 //               List<SMT.SaaS.LocalData.OrgObject> orglist = new List<SMT.SaaS.LocalData.OrgObject>();
                 //               foreach (var org in fq.OrgObjects)
                 //               {
                 //                   SMT.SaaS.LocalData.OrgObject og = new SMT.SaaS.LocalData.OrgObject();
                 //                   og.OrgID = org.OrgID;
                 //                   og.OrgType = org.OrgType;
                 //               }
                 //               pv.OrgObjects=fq.OrgObjects;
                 //               tps.CustomerPermission.PermissionValue.Add(pv);
                 //           }

                 }
                //LoadCompanyInfo();

            }
            if (this.LoginedClick != null)
            {
                this.LoginedClick(this, null);
            }
        }
        void client_UserLoginCompleted(object sender, UserLoginCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                LoginResult = false;
                if (this.LoginedClick != null)
                {
                    this.LoginedClick(this, null);
                }
            }
            else
            {
                LoginResult = true;

                User = new T_SYS_USER();
                User = e.Result;
                client.GetUserPermissionByUserToUIAsync(User.SYSUSERID);
            }
            //if (this.LoginedClick != null)
            //{
            //    this.LoginedClick(this,null);
            //}
        }
        public T_SYS_USER GetUserInfo()
        {
            return User;
        }


        #region silverlight数据加密
        /**/
        /// <summary>
        /// AES算法加密数据
        /// </summary>
        /// <param name="input">加密前的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt(string input)
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
        #endregion

    }
}
