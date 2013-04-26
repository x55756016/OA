using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Collections;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: ViewModel基类
//           1.实现 INotifyPropertyChanged接口，用于向客户端（通常是执行绑定的客户端）发出某一属性值已更改的通知。
//           2.实现 INotifyDataErrorInfo接口，定义数据实体类可以实现以提供自定义异步验证支持的成员。
//           3.封装调用ValidationScope，实现VM的属性验证。
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.ViewModel.Foundation
{
    /// <summary>
    /// ViewModel基类
    /// 1.实现 INotifyPropertyChanged接口，用于向客户端（通常是执行绑定的客户端）发出某一属性值已更改的通知。
    /// 2.实现 INotifyDataErrorInfo接口，定义数据实体类可以实现以提供自定义异步验证支持的成员。
    /// 3.封装调用ValidationScope，实现VM的属性验证。
    /// </summary>
    public class BasicViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region INotifyPropertyChanged接口的支持
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 注册属性的的PropertyChanged事件
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        /// 验证属性是否存在
        /// </summary>
        protected void VerifyPropertyName(string propertyName)
        {
            //if (String.IsNullOrEmpty(propertyName))
            //    return;

            if (this.GetType().GetProperty(propertyName) == null)
            {
                string msg = "Invalid property name: " + propertyName;
                throw new ArgumentException(msg);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler pceh = PropertyChanged;
            if (pceh != null)
            {
                pceh(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual bool SetValue<T>(ref T target, T value, string propertyName)
        {
            if (Object.Equals(target, value))
            {
                return false;
            }

            Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = propertyName });
            target = value;
            OnPropertyChanged(propertyName);

            return true;
        }
        #endregion

        #region ValidationScope的支持，托管触发验证
        private readonly ValidationScope _validationScope1 = new ValidationScope();
        public ValidationScope ValidationScope
        {
            get { return _validationScope1; }
        }
        public bool IsValidation
        {
            get
            {
                ValidationScope.ValidateScope();
                return ValidationScope.IsValid() && HasErrors;
            }
        }
        #endregion

        #region INotifyDataErrorInfo接口的支持，提供异步验证支持。
        //错误列表
        private Dictionary<string, List<ValidationErrorInfo>> _errors = new Dictionary<string, List<ValidationErrorInfo>>();

        /// <summary>
        /// 当属性或整个对象的验证错误已经更改时发生。
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// 获取指定属性或整个对象的验证错误。
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>错误验证列表</returns>
        public IEnumerable GetErrors(string propertyName)
        {
            if (!_errors.ContainsKey(propertyName))
                return _errors.Values;

            return _errors[propertyName];
        }

        /// <summary>
        /// 获取一个指示该对象是否有验证错误的值。
        /// </summary>
        public bool HasErrors
        {
            get { return this._errors.Count == 0; }
        }

        /// <summary>
        /// 触发给定名称的属性验证
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyErrorsChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// 根据属性名称，为其添加一个异步错误。
        /// </summary>
        /// <param name="propertyName">
        /// 属性名称。
        /// </param>
        /// <param name="errorInfo">
        /// 错误信息。
        /// </param>
        protected void AddError(string propertyName, ValidationErrorInfo errorInfo)
        {
            //判断当前错误是否已经存在，若存在则删除此错误信息。
            ExistError(propertyName, errorInfo.ErrorCode);

            if (!_errors.ContainsKey(propertyName))
                _errors.Add(propertyName, new List<ValidationErrorInfo>());

            _errors[propertyName].Add(errorInfo);

            NotifyErrorsChanged(propertyName);
        }

        protected void RemoveError(string propertyName, int errorCode)
        {
            if (_errors.ContainsKey(propertyName))
            {
                ExistError(propertyName, errorCode);

                NotifyErrorsChanged(propertyName);
            }
        }

        /// <summary>
        ///  根据属性名称，判断当前错误是否已经存在，若存在则删除此错误信息。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="errorCode"></param>
        private void ExistError(string propertyName, int errorCode)
        {
            if (_errors.ContainsKey(propertyName))
            {
                var errorToRemove = _errors[propertyName].SingleOrDefault(error => error.ErrorCode == errorCode);

                if (errorToRemove != null)
                {
                    _errors[propertyName].Remove(errorToRemove);

                    if (_errors[propertyName].Count == 0)
                        _errors.Remove(propertyName);
                }
            }
        }

        #endregion
    }
}
