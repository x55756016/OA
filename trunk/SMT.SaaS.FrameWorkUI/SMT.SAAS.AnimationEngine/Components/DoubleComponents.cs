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
using SMT.SAAS.AnimationEngine.Model;

namespace SMT.SAAS.AnimationEngine.Components
{
    /// <summary>
    /// 封装返回处理UIElement的DoubleAnimation
    /// </summary>
    internal class DoubleComponents
    {
        /// <summary>
        /// 返回根据ProtertyPath封装过的DoubleAnimation
        /// </summary>
        /// <param name="PropertyPath">属性路径</param>
        /// <param name="seconds">动画时间</param>
        /// <param name="from">起始值</param>
        /// <param name="to">结束值</param>
        /// <param name="EasingFun">缓动函数</param>
        /// <returns>已组装过的DoubleAnimation</returns>
        public static DoubleAnimation GetDoubleAnimation(string PropertyPath, double seconds, double from, double to, EasingFunctionBase EasingFun)
        {
            DoubleAnimation _doubleAnimation = new DoubleAnimation();
            _doubleAnimation.From = from;
            _doubleAnimation.To = to;
            _doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            if (EasingFun != null)
                _doubleAnimation.EasingFunction = EasingFun;
            Storyboard.SetTargetProperty(_doubleAnimation, new PropertyPath(PropertyPath));
            return _doubleAnimation;
        }

        /// <summary>
        /// 创建Double值之间的线性动画
        /// </summary>
        public static DoubleAnimation BuildDoubleAnimation(DoubleModel Model)
        {

            (Model.Target as UIElement).RenderTransform=TransitionHelper.GetTransformGroup(Model.Target);

            DoubleAnimation _doubleAnimation = new DoubleAnimation();
            _doubleAnimation.From = Model.From;
            _doubleAnimation.To = Model.To;
            _doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(Model.Duration));
            _doubleAnimation.AutoReverse = Model.AutoReverse;
            _doubleAnimation.BeginTime = TimeSpan.FromSeconds(Model.BeginTime);
            _doubleAnimation.By = Model.By;
            _doubleAnimation.FillBehavior = Model.FillBehavior;
            _doubleAnimation.RepeatBehavior = Model.RepeatBehavior;
            _doubleAnimation.SpeedRatio = Model.SpeedRatio;

            if (Model.EasingFunction != null)
                _doubleAnimation.EasingFunction = Model.EasingFunction;


            Storyboard.SetTarget(_doubleAnimation, Model.Target);
            Storyboard.SetTargetProperty(_doubleAnimation, new PropertyPath(Model.PropertyPath));
            return _doubleAnimation;
        }
        public static DoubleAnimationUsingKeyFrames BuildDoubleKeyFramesAnimation(DoubleKeyFramesModel Model)
        {
           // (Model.Target as UIElement).RenderTransform = TransitionHelper.GetTransformGroup(Model.Target);
            DoubleAnimationUsingKeyFrames _doubleAnimation = new DoubleAnimationUsingKeyFrames();
            _doubleAnimation.Duration = (Model.Duration == 0 ? Duration.Automatic : new Duration(TimeSpan.FromSeconds(Model.Duration)));
            _doubleAnimation.AutoReverse = Model.AutoReverse;
            _doubleAnimation.BeginTime = TimeSpan.FromSeconds(Model.BeginTime);
            _doubleAnimation.FillBehavior = Model.FillBehavior;
            _doubleAnimation.RepeatBehavior = Model.RepeatBehavior;
            _doubleAnimation.SpeedRatio = Model.SpeedRatio;
            foreach (var item in Model.KeyFrames)
            {
                _doubleAnimation.KeyFrames.Add(CreateDoubleKeyFrmas(item));
            }
            Storyboard.SetTarget(_doubleAnimation, Model.Target);
            Storyboard.SetTargetProperty(_doubleAnimation, new PropertyPath(Model.PropertyPath));
            return _doubleAnimation;
        }
        private static DoubleKeyFrame CreateDoubleKeyFrmas(KeyFrames<double> Model)
        {
            DoubleKeyFrame frame = null;
            switch (Model.Type)
            {
                case KeyFramesType.Spline: frame = new SplineDoubleKeyFrame() { KeySpline = Model.Spline }; break;
                case KeyFramesType.Linear: frame = new LinearDoubleKeyFrame(); break;
                case KeyFramesType.Easing: frame = new EasingDoubleKeyFrame() { EasingFunction = Model.EasingFunction }; break;
                case KeyFramesType.Discrete: frame = new DiscreteDoubleKeyFrame(); break;
                default: break;
            }
            frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(Model.KeyTime)); 
            frame.Value = Model.Value;
            return frame;
        }
    }
}

