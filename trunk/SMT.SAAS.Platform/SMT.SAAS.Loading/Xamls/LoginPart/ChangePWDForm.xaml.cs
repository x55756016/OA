//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 修改密码界面
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using SMT.SAAS.Platform.PermissionWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SAAS.Platform.Xamls.LoginPart
{
    public partial class ChangePWDForm : Grid
    {
        PermissionServiceClient _toolsClient;
        MainUIWS.MainUIServicesClient lodingClinet;
        private ItemBox parentWindow;
        public ChangePWDForm()
        {
            InitializeComponent();
            lodingClinet = new MainUIWS.MainUIServicesClient();
            lodingClinet.SystemLoginCompleted += new System.EventHandler<MainUIWS.SystemLoginCompletedEventArgs>(lodingClinet_SystemLoginCompleted);
            _toolsClient = new PermissionWS.PermissionServiceClient();
            _toolsClient.SysUserInfoUpdateByUserIdandUsernameCompleted += new System.EventHandler<PermissionWS.SysUserInfoUpdateByUserIdandUsernameCompletedEventArgs>(_toolsClient_SysUserInfoUpdateByUserIdandUsernameCompleted);


            if (App.AppSettings.Contains("USERNAME"))
                txbUserName.Text = App.AppSettings["USERNAME"].ToString();

        }

      

     


        #region 修改密码

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radButton1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txbUserName.Text))
            {
                MessageBox.Show("用户名不能为空");
                return;
            }
            if (string.IsNullOrEmpty(txbUserPassword.Password))
            {
                MessageBox.Show("原密码不能为空");
                return;
            }
            if(!CheckPwd(txbNewPwd.Password))
            {
                return ;
            }
            if (txbNewPwd.Password != txbNewPwd2.Password)
            {
                MessageBox.Show("确认密码跟新密码不一致！请输入一致的新密码。");
                return;
            }
            radButton1.IsEnabled = false;
            string UserPwdMD5 = MD5.GetMd5String(this.txbUserPassword.Password);
            lodingClinet.SystemLoginAsync(txbUserName.Text, UserPwdMD5);
        }

        

        void lodingClinet_SystemLoginCompleted(object sender, MainUIWS.SystemLoginCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("原用户名密码错误！");
                radButton1.IsEnabled = true;
            }
            else
            {
                if (e.Result == null)
                {
                    MessageBox.Show("原用户名密码错误！");
                    radButton1.IsEnabled = true;
                }
                else
                {
                    _toolsClient.SysUserInfoUpdateByUserIdandUsernameAsync(e.Result.EMPLOYEEID, txbUserName.Text, Utility.Encrypt(txbNewPwd2.Password));
                }
            }
        }
    

        void _toolsClient_SysUserInfoUpdateByUserIdandUsernameCompleted(object sender, PermissionWS.SysUserInfoUpdateByUserIdandUsernameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                MessageBox.Show("用户密码修改成功！");
            }
            else
            {
                radButton1.IsEnabled = true;
                MessageBox.Show("用户密码修改失败！请联系管理员");
            }
        }

        /// <summary>
        /// added by luojie
        /// 用与验证密码的合法性 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool CheckPwd(string password)
        {
            bool legalPwd = true;//返回的结果
            string rstMessage = string.Empty;//提示语
            if (password != null)
            {
                if (password.Length < 8)
                {
                    legalPwd = false;
                    rstMessage = "新密码不能小于8位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                if (password.Length > 15)
                {
                    legalPwd = false;
                    rstMessage = "新密码不能大于15位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                string ptnNum = @"\D[0-9]+";//字母开头后面必须跟数字
                string ptnWord = @"[0-9][a-z_A-Z]+";//数字开头后面必须跟字母
                Match matchNum = Regex.Match(password, ptnNum);
                Match matchWord = Regex.Match(password, ptnWord);
                if (!matchNum.Success && !matchWord.Success)
                {
                    legalPwd = false;
                    rstMessage = "新密码必须是8-15位的英文与数字组合";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码必须是中英文结合的");
                }
                if (!string.IsNullOrWhiteSpace(rstMessage)) MessageBox.Show(rstMessage);
            }
            else
            {
                legalPwd = false;
            }
            return legalPwd;
        }
        #endregion

        

        #region 返回登录界面
        /// <summary>
        /// 返回登录界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.parentWindow.NavigateToLogin();
        }
        #endregion

        public void SetparentWindow(ItemBox login)
        {
            this.parentWindow = login;
        }
    }
}
