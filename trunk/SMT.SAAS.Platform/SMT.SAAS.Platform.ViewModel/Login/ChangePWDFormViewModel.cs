
using System.Windows.Input;
using System.ComponentModel.DataAnnotations;
using SMT.SAAS.Platform.ViewModel.Foundation;
using System;

namespace SMT.SAAS.Platform.ViewModel.Login
{
    public class ChangePWDFormViewModel : Foundation.BasicViewModel
    {
        Model.Services.CommonServices _Services;
        private const int UserInfoERROCODE = 200;
        private const int PasswordERROCODE = 201;
        public event EventHandler<OnUpdateUserInfoArges> OnUpdateUserInfoCompleted;

        #region UserChangePwd属性
        private string _UserName;
        private string _OldPwd;
        private string _NewPwd1;
        private string _NewPwd2;
        private bool _IsLock = false;
        private string _empUserID = string.Empty;


        public bool IsLock
        {
            get { return _IsLock; }
            set
            {
                SetValue(ref _IsLock, value, "IsLock");
            }
        }

        /// <summary>
        /// 即更改密码的用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空!")]
        public string UserName
        {
            get { return _UserName; }
            set
            {
                SetValue(ref _UserName, value, "UserName");

                if (!string.IsNullOrEmpty(OldPwd))
                    CheckUserInfo();
            }
        }

        /// <summary>
        /// 旧密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空!")]
        public string OldPwd
        {
            get { return _OldPwd; }
            set
            {
                SetValue(ref _OldPwd, value, "OldPwd");
                CheckUserInfo();
            }
        }

        /// <summary>
        /// 新密码  )：^[a-zA-Z][a-zA-Z0-9_]{8,15}$
        /// </summary>
        [Required(ErrorMessage = "新密码不能为空!")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "密码长度为8-15个字符!")]
        [RegularExpression(@"^[0-9a-zA-Z_]{8,15}$", ErrorMessage = "长度在8-15个字符,密码组合为数字、字母、下划线!")]

        public string NewPwd1
        {
            get { return _NewPwd1; }
            set
            {
                SetValue(ref _NewPwd1, value, "NewPwd1");
                if (!string.IsNullOrEmpty(NewPwd2))
                    ValidationNewPassword();
            }
        }

        /// <summary>
        /// 确认新密码
        /// </summary>
        [Required(ErrorMessage = "确认密码不能为空!")]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "密码长度为8-15个字符!")]
        [RegularExpression(@"^[0-9a-zA-Z_]{8,15}$", ErrorMessage = "长度在8-15个字符,密码组合为数字、字母、下划线!")]
        public string NewPwd2
        {
            get { return _NewPwd2; }
            set
            {
                SetValue(ref _NewPwd2, value, "NewPwd2");
                if (!string.IsNullOrEmpty(NewPwd1))
                    ValidationNewPassword();
            }
        }
        #endregion

        public ChangePWDFormViewModel()
        {
            _Services = new Model.Services.CommonServices();
            _Services.OnGetUserInfoCompleted += new System.EventHandler<Model.GetEntityEventArgs<Model.UserLogin>>(_Services_OnGetUserInfoCompleted);
            _Services.OnUpdateUserInfoCompleted += new System.EventHandler<Model.ExecuteNoQueryEventArgs>(_Services_OnUpdateUserInfoCompleted);
        }


        private void ValidationUserInfo(bool issubmit)
        {
            var propertyName1 = "UserName";

            if (!issubmit)
            {
                AddError(propertyName1,
                         new ValidationErrorInfo()
                         {
                             ErrorCode = UserInfoERROCODE,
                             ErrorMessage = string.Format("用户名或密码输入不正确，请检测并重新输入！")
                         });
            }
            else
            {
                RemoveError(propertyName1, UserInfoERROCODE);
            }
        }

        private void ValidationNewPassword()
        {
            var propertyName1 = "NewPwd1";

            if (NewPwd1 != NewPwd2)
            {
                //IsLock = false;
                AddError(propertyName1,
                         new ValidationErrorInfo()
                         {
                             ErrorCode = PasswordERROCODE,
                             ErrorMessage = string.Format("新密码与重复密码密码输入不一致！")
                         });
            }
            else
            {
                // IsLock = true;
                RemoveError(propertyName1, UserInfoERROCODE);
            }
        }

        void _Services_OnUpdateUserInfoCompleted(object sender, Model.ExecuteNoQueryEventArgs e)
        {
            
            if (OnUpdateUserInfoCompleted != null)
                OnUpdateUserInfoCompleted(this, new OnUpdateUserInfoArges() { Error=e.Error,Result=e.Result });
        }

        void _Services_OnGetUserInfoCompleted(object sender, Model.GetEntityEventArgs<Model.UserLogin> e)
        {
            bool valUser = false;
            if (e.Result != null)
            {
                if (!string.IsNullOrEmpty(e.Result.UserName))
                {
                    if (e.Result.UserPassword == _Services.Encrypt(OldPwd))
                    {
                        _empUserID = e.Result.EmployeeID;
                        valUser = true;
                    }
                }
            }
            IsLock = valUser;
            ValidationUserInfo(valUser);
        }

        private void CheckUserInfo()
        {
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(OldPwd))
                _Services.GetUserInfo(UserName);
        }

        /// <summary>
        /// 提交修改密码
        /// </summary>
        public ICommand Submit
        {
            get { return new Foundation.RelayCommand(Save); }
        }

        /// <summary>
        /// 调用服务端，并验证是否通过
        /// </summary>
        private void Save()
        {
            if (IsValidation)
            {
                _Services.UpdateUserInfo(_empUserID, UserName, NewPwd1);
            }
        }
    }

    public class OnUpdateUserInfoArges : EventArgs
    {
        public bool Result { get; set; }
        public Exception Error { get; set; }
    }
}
