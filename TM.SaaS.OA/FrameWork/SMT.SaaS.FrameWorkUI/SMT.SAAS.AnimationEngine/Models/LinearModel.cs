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

namespace SMT.SAAS.AnimationEngine.Model
{
    public class LinearModel<T> : BaseModel
    {
        private T _From;
        private T _To;
        private T _By;
        /// <summary>
        /// 获取或设置动画的起始值。
        /// </summary>
        public T From { get { return _From; } set { _From = value; } }
        /// <summary>
        /// 获取或设置动画的结束值。 
        /// </summary>
        public T To { get { return _To; } set { _To = value; } }

        /// <summary>
        /// 获取或设置动画更改其起始值时所依据的总量
        /// </summary>
        public T By { get { return _By; } set { _By = value; } }
        /// <summary>
        /// 获取或设置应用于此动画的缓动函数
        /// </summary>
        public IEasingFunction EasingFunction { get; set; }
    }
}
