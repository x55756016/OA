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

namespace SMT.SAAS.AnimationEngine
{
    public enum KeyFramesType
    {
        /// <summary>
        /// 样条曲线式关键帧：将在值之间产生由 KeySpline 属性确定的可变转换
        /// </summary>
        Spline,
        /// <summary>
        /// 线性关键帧：能够在不同值之间创建平滑的线性内插
        /// </summary>
        Linear,
        /// <summary>
        /// 缓动函数关键帧：可以使用给定的缓动函数来进行变换运行
        /// </summary>
        Easing,
        /// <summary>
        /// 离散关键帧将在值之间产生突然"跳跃"(无内插算法)
        /// </summary>
        Discrete
    }
}
