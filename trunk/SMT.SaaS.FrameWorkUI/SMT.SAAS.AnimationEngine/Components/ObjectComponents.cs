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
    public class ObjectComponents
    {
        /// <summary>
        /// 创建Point类型的关键帧动画
        /// </summary>
        /// <param name="Model">动画数据</param>
        /// <returns></returns>
        public static ObjectAnimationUsingKeyFrames BuildObjectKeyFramesAnimation(ObjectKeyFramesModel Model)
        {
            ObjectAnimationUsingKeyFrames _objectAnimation = new ObjectAnimationUsingKeyFrames();
            _objectAnimation.Duration = (Model.Duration == 0 ? Duration.Automatic : new Duration(TimeSpan.FromSeconds(Model.Duration)));
            _objectAnimation.AutoReverse = Model.AutoReverse;
            _objectAnimation.BeginTime = TimeSpan.FromSeconds(Model.BeginTime);
            _objectAnimation.FillBehavior = Model.FillBehavior;
            _objectAnimation.RepeatBehavior = Model.RepeatBehavior;
            _objectAnimation.SpeedRatio = Model.SpeedRatio;
            foreach (var item in Model.KeyFrames)
            {
                _objectAnimation.KeyFrames.Add(CreateColorKeyFrmas(item));
            }
            Storyboard.SetTarget(_objectAnimation, Model.Target);
            Storyboard.SetTargetProperty(_objectAnimation, new PropertyPath(Model.PropertyPath));
            return _objectAnimation;
        }
        private static ObjectKeyFrame CreateColorKeyFrmas(KeyFrames<Object> Model)
        {
            ObjectKeyFrame frame = null;
            switch (Model.Type)
            {
                case KeyFramesType.Spline: break;
                case KeyFramesType.Linear: break;
                case KeyFramesType.Easing: break;
                case KeyFramesType.Discrete: frame = new DiscreteObjectKeyFrame(); break;
                default: break;
            }
            frame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(Model.KeyTime));
            frame.Value = Model.Value;
            return frame;
        }
    }
}
