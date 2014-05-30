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
    public class KeyFrames<T>
    {
        /// <summary>
        /// 关键帧触发的时间点
        /// </summary>
        public double KeyTime { get; set; }
        /// <summary>
        /// 关键帧到达的值
        /// </summary>
        public T Value { get; set; }
        /// <summary>
        /// 关键帧类型
        /// </summary>
        public KeyFramesType Type { get; set; }
        /// <summary>
        /// 运行曲线,只有在关键帧为Spline类型时此属性方可启用
        /// </summary>
        public KeySpline Spline { get; set; }
        /// <summary>
        /// 缓动函数,只有在关键帧类型为Easing类型时此属性方可启用
        /// </summary>
        public IEasingFunction EasingFunction { get; set; }

    }
    
}
