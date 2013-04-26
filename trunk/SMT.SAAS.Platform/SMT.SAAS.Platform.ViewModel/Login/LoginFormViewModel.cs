using System;
using System.Windows.Input;
using System.ComponentModel.DataAnnotations;
using SMT.SAAS.Platform.Model;
using SMT.SAAS.Platform.Model.Services;
using SMT.SAAS.Platform.ViewModel.Foundation;
using System.IO.IsolatedStorage;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.ViewModel.Login
{
    /// <summary>
    /// 封装用户登录界面的功能。
    /// </summary>
    public class LoginFormViewModel : Foundation.BasicViewModel
    {
        public event EventHandler OnLoginCompleted;
        private LoginUser _loginUser;
        private const int ValCodeERROCODE = 100;
        private const int ValCodeNULLERROCODE = 101;
        private const int ValCodeUSERNAME = 102;
        private const int ValCodePASSWORD = 103;

        /// <summary>
        /// 独立存储
        /// </summary>
        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        /// <summary>
        /// 存储最后一次访问的用户名KEY
        /// </summary>
        private const string USERKEY = "USERNAME";

        public LoginFormViewModel()
        {

            if (AppSettings.Contains(USERKEY))
                UserName = AppSettings[USERKEY].ToString();
            _loginUser = new LoginUser();
            _loginUser.UserLoginCompleted += new EventHandler<GetEntityEventArgs<UserLogin>>(_loginUser_UserLoginCompleted);
            _loginUser.UserLoginFaild += new EventHandler(_loginUser_UserLoginFaild);
        }

        void _loginUser_UserLoginFaild(object sender, EventArgs e)
        {
            UnLock = true;
            ValidationUser(false);
        }

        /// <summary>
        /// 已经作废，登录在新的工程里
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _loginUser_UserLoginCompleted(object sender, GetEntityEventArgs<UserLogin> e)
        {
            UnLock = true;
            var result = e.Result;

            if (!AppSettings.Contains(USERKEY))
                AppSettings.Add(USERKEY, result.UserName);
            else
                AppSettings[USERKEY] = result.UserName;

            //SMT.SAAS.Main.CurrentContext.Common = e.Result;
            if (this.OnLoginCompleted != null)
            {
                OnLoginCompleted(this, EventArgs.Empty);
            }
        }

        #region 成员列表
        private string _userName;
        private string _userPassword;
        private string _currValCode = "";
        private string _userValCode;

        private bool _unlock=true;

        public bool UnLock
        {
            get { return _unlock; }
            set
            {
                SetValue(ref _unlock, value, "UnLock");
                //ValidationUser(true);
            }
        }

        /// <summary>
        /// 登录的用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空!")]
        public string UserName
        {
            get { return _userName; }
            set
            {
                SetValue(ref _userName, value, "UserName");
                //ValidationUser(true);
            }
        }


        /// <summary>
        /// 登录的用户密码
        ///         [StringLength(15, MinimumLength = 8, ErrorMessage = "密码长度不正确,为8-15位!")]
        /// </summary>
        [Required(ErrorMessage = "密码不能为空!")]
        public string UserPassword
        {
            get { return _userPassword; }
            set
            {
                SetValue(ref _userPassword, value, "UserPassword");
                //  ValidationUser(true);
            }
        }

        /// <summary>
        /// 当前验证码
        /// </summary>
       //[Required(ErrorMessage = "验证码不能为空!")]
        public string CurrValCode
        {
            get { return _currValCode; }
            set
            {
                SetValue(ref _currValCode, value, "CurrValCode");
               // ValidationCode();
            }
        }

        /// <summary>
        /// 用户输入验证码
        /// </summary>
        public string UserValCode
        {
            get { return _userValCode; }
            set
            {
                SetValue(ref _userValCode, value, "UserValCode");
               
               // ValidationCode();
            }
        }

        private void ValidationCodeIsEmpty()
        {
            var propertyName = "UserValCode";

            if (string.IsNullOrEmpty(UserValCode))
            {
                AddError(propertyName,
                     new ValidationErrorInfo()
                     {
                         ErrorCode = ValCodeNULLERROCODE,
                         ErrorMessage = string.Format("验证码不能为空, 请输入：{0}！ ", CurrValCode.ToLower())
                     });
            }
            else
            {
                RemoveError(propertyName, ValCodeNULLERROCODE);
            }
        }

        private void ValidationUser(bool issubmit)
        {
            var propertyName1 = "UserName";
            //  var propertyName2 = "UserPassword";
            if (!issubmit)
            {
                AddError(propertyName1,
                         new ValidationErrorInfo()
                         {
                             ErrorCode = ValCodeUSERNAME,
                             ErrorMessage = string.Format("用户名或密码不正确,请重新输入！")
                         });
                //AddError(propertyName2,
                //       new ValidationErrorInfo()
                //       {
                //           ErrorCode = ValCodePASSWORD,
                //           ErrorMessage = string.Format("用户名或密码不正确,请重新输入！")
                //       });
            }
            else
            {
                RemoveError(propertyName1, ValCodeUSERNAME);
                // RemoveError(propertyName2, ValCodePASSWORD);
            }

        }

        private void ValidationCode()
        {
            if (string.IsNullOrEmpty(UserValCode))
                return;

            var propertyName = "UserValCode";
            RemoveError(propertyName, ValCodeNULLERROCODE);
            if (UserValCode.ToLower() != CurrValCode.ToLower())
            {
                AddError(propertyName,
                    new ValidationErrorInfo()
                    {
                        ErrorCode = ValCodeERROCODE,
                        ErrorMessage = string.Format("输入的验证码不匹配, {0} 不为 {1}！ ", UserValCode, CurrValCode.ToLower())
                    });
            }
            else
            {
                RemoveError(propertyName, ValCodeERROCODE);
            }
        }

        #endregion

        public ICommand Submit
        {
            get { return new Foundation.RelayCommand(SubmitLogin); }
        }

        public void SubmitLogin()
        {
            UnLock = false;
            //ValidationCodeIsEmpty();

            // ValidationCode();

            ValidationUser(true);

            if (base.IsValidation)
            {
                _loginUser.UserLogin(this.UserName.Trim(), this.UserPassword);
            }
            else
            {
                UnLock = true;
            }
        }
    }
}
