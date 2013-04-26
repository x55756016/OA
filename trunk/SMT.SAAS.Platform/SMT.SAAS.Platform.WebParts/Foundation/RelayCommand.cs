using System;
using System.Windows.Input;

namespace SMT.SAAS.Platform.WebParts.ViewModels.Foundation
{
    public class RelayCommand : ICommand  
    {
        readonly Action _execute;
        readonly Func<bool> _canExecute;

        #region 构造函数

        /// <summary>
        /// 创建一个总是可以执行的命令
        /// </summary>
        /// <param name="execute">需要执行的逻辑.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// 创建一个新的命令
        /// </summary>
        /// <param name="execute">要处理的逻辑.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion  

        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            _execute();
        }

        #endregion
    }
}
