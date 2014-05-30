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
    public class BaseModel
    {
        private bool _AutoReverse = false;
        private double _BeginTime = 0;
        private double _SpeedRatio = 1;
        private RepeatBehavior _RepeatBehavior = new RepeatBehavior(1);
        private FillBehavior _FillBehavior = System.Windows.Media.Animation.FillBehavior.HoldEnd;
        private double _Duration = 0;

        /// <summary>
        /// 获取或设置动画将要控制的对象
        /// </summary>
        public DependencyObject Target { get; set; }
        /// <summary>
        /// 获取或设置动画将要控制的对象属性
        /// </summary>
        public string PropertyPath { get; set; }
        /// <summary>
        /// 获取或设置一个值，该值指示时间线在完成向前迭代后是否按相反的顺序播放。
        /// </summary>
        public bool AutoReverse { get { return _AutoReverse; } set { _AutoReverse = value; } }
        /// <summary>
        /// 获取或设置此 Timeline 将要开始的时间
        /// </summary>
        public double BeginTime { get { return _BeginTime; } set { _BeginTime = value; } }
        /// <summary>
        /// 获取或设置此 Timeline 的时间相对于其父级的前进速率。
        /// </summary>
        public double SpeedRatio { get { return _SpeedRatio; } set { _SpeedRatio = value; } }
        /// <summary>
        /// 获取或设置此时间线的重复行为
        /// </summary>
        public RepeatBehavior RepeatBehavior { get { return _RepeatBehavior; } set { _RepeatBehavior = value; } }
        /// <summary>
        /// 获取或设置一个值，该值指定动画在活动周期结束后的行为方式。
        /// </summary>
        public FillBehavior FillBehavior { get { return _FillBehavior; } set { _FillBehavior = value; } }
        /// <summary>
        /// 获取或设置此时间线播放的时间长度，而不是计数重复
        /// </summary>
        public double Duration { get { return _Duration; } set { _Duration = value; } }
       

    }
}
