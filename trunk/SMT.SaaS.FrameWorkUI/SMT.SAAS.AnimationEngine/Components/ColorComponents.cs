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
    public class ColorComponents
    {
        /// <summary>
        /// 创建Color值之间的线性动画
        /// </summary>
        public static ColorAnimation BuildColorAnimation(ColorModel Model)
        {
            ColorAnimation _colorAnimation = new ColorAnimation();
            _colorAnimation.From = Model.From;
            _colorAnimation.To = Model.To;
            _colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(Model.Duration));
            _colorAnimation.AutoReverse = Model.AutoReverse;
            _colorAnimation.BeginTime = TimeSpan.FromSeconds(Model.BeginTime);
            _colorAnimation.By = Model.By;
            _colorAnimation.FillBehavior = Model.FillBehavior;
            _colorAnimation.RepeatBehavior = Model.RepeatBehavior;
            _colorAnimation.SpeedRatio = Model.SpeedRatio;

            if (Model.EasingFunction != null)
                _colorAnimation.EasingFunction = Model.EasingFunction;


            Storyboard.SetTarget(_colorAnimation, Model.Target);
            Storyboard.SetTargetProperty(_colorAnimation, new PropertyPath(Model.PropertyPath));
            return _colorAnimation;
        }
        /// <summary>
        /// 创建Color类型的关键帧动画
        /// </summary>
        /// <param name="Model">动画数据</param>
        /// <returns></returns>
        public static ColorAnimationUsingKeyFrames BuildColorKeyFramesAnimation(ColorKeyFramesModel Model)
        {
            ColorAnimationUsingKeyFrames _colorAnimation = new ColorAnimationUsingKeyFrames();
            _colorAnimation.Duration = (Model.Duration==0? Duration.Automatic:new Duration(TimeSpan.FromSeconds(Model.Duration)));  
            _colorAnimation.AutoReverse = Model.AutoReverse;
            _colorAnimation.BeginTime = TimeSpan.FromSeconds(Model.BeginTime);
            _colorAnimation.FillBehavior = Model.FillBehavior;
            _colorAnimation.RepeatBehavior = Model.RepeatBehavior;
            _colorAnimation.SpeedRatio = Model.SpeedRatio;
            foreach (var item in Model.KeyFrames)
            {
                _colorAnimation.KeyFrames.Add(CreateColorKeyFrmas(item));
            }
            Storyboard.SetTarget(_colorAnimation, Model.Target);
            Storyboard.SetTargetProperty(_colorAnimation, new PropertyPath(Model.PropertyPath));
            return _colorAnimation;
        }
        private static ColorKeyFrame CreateColorKeyFrmas(KeyFrames<Color> Model)
        {
            ColorKeyFrame frame = null;
            switch (Model.Type)
            {
                case KeyFramesType.Spline: frame = new SplineColorKeyFrame() { KeySpline = Model.Spline }; break;
                case KeyFramesType.Linear: frame = new LinearColorKeyFrame(); break;
                case KeyFramesType.Easing: frame = new EasingColorKeyFrame() { EasingFunction = Model.EasingFunction }; break;
                case KeyFramesType.Discrete: frame = new DiscreteColorKeyFrame(); break;
                default: break;
            }
            frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(Model.KeyTime));
            frame.Value = Model.Value;
            return frame;
        }
    }
}
