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
    public class PointComponents
    {
        /// <summary>
        /// 创建Point值之间的线性动画
        /// </summary>
        public static PointAnimation BuildPointAnimation(PointModel Model)
        {
            PointAnimation _pointAnimation = new PointAnimation();
            _pointAnimation.From = Model.From;
            _pointAnimation.To = Model.To;
            _pointAnimation.Duration = new Duration(TimeSpan.FromSeconds(Model.Duration));
            _pointAnimation.AutoReverse = Model.AutoReverse;
            _pointAnimation.BeginTime = TimeSpan.FromSeconds(Model.BeginTime);
            _pointAnimation.By = Model.By;
            _pointAnimation.FillBehavior = Model.FillBehavior;
            _pointAnimation.RepeatBehavior = Model.RepeatBehavior;
            _pointAnimation.SpeedRatio = Model.SpeedRatio;

            if (Model.EasingFunction != null)
                _pointAnimation.EasingFunction = Model.EasingFunction;


            Storyboard.SetTarget(_pointAnimation, Model.Target);
            Storyboard.SetTargetProperty(_pointAnimation, new PropertyPath(Model.PropertyPath));
            return _pointAnimation;
        }
        /// <summary>
        /// 创建Point类型的关键帧动画
        /// </summary>
        /// <param name="Model">动画数据</param>
        /// <returns></returns>
        public static PointAnimationUsingKeyFrames BuildPointKeyFramesAnimation(PointKeyFramesModel Model)
        {
            PointAnimationUsingKeyFrames _pointAnimation = new PointAnimationUsingKeyFrames();
            _pointAnimation.Duration = (Model.Duration == 0 ? Duration.Automatic : new Duration(TimeSpan.FromSeconds(Model.Duration)));
            _pointAnimation.AutoReverse = Model.AutoReverse;
            _pointAnimation.BeginTime = TimeSpan.FromSeconds(Model.BeginTime);
            _pointAnimation.FillBehavior = Model.FillBehavior;
            _pointAnimation.RepeatBehavior = Model.RepeatBehavior;
            _pointAnimation.SpeedRatio = Model.SpeedRatio;
            foreach (var item in Model.KeyFrames)
            {
                _pointAnimation.KeyFrames.Add(CreatePointKeyFrmas(item));
            }
            Storyboard.SetTarget(_pointAnimation, Model.Target);
            Storyboard.SetTargetProperty(_pointAnimation, new PropertyPath(Model.PropertyPath));
            return _pointAnimation;
        }
        private static PointKeyFrame CreatePointKeyFrmas(KeyFrames<Point> Model)
        {
            PointKeyFrame frame = null;
            switch (Model.Type)
            {
                case KeyFramesType.Spline: frame = new SplinePointKeyFrame() { KeySpline = Model.Spline }; break;
                case KeyFramesType.Linear: frame = new LinearPointKeyFrame(); break;
                case KeyFramesType.Easing: frame = new EasingPointKeyFrame() { EasingFunction = Model.EasingFunction }; break;
                case KeyFramesType.Discrete: frame = new DiscretePointKeyFrame(); break;
                default: break;
            }
            frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(Model.KeyTime));
            frame.Value = Model.Value;
            return frame;
        }
    }
}
