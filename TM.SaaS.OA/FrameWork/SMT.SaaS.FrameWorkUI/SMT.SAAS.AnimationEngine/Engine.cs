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
using System.Collections.Generic;

namespace SMT.SAAS.AnimationEngine
{
    public class Engine
    {
        public static Storyboard CreateStoryboard(List<IModel> models)
        {
            return CreateStoryboard(models, null);
        }

        public static Storyboard CreateStoryboard(List<IModel> models, Storyboard sourceStoryboard)
        {
            Storyboard _storyboard;
            if (sourceStoryboard != null)
                _storyboard = sourceStoryboard;
            else
                _storyboard = new Storyboard();

            foreach (var item in models)
            {
                switch (item.ModelType)
                {
                    case IModelType.Double:_storyboard.Children.Add(Components.DoubleComponents.BuildDoubleAnimation(item as DoubleModel));
                        break;
                    case IModelType.DoubleKeyFrames:_storyboard.Children.Add(Components.DoubleComponents.BuildDoubleKeyFramesAnimation(item as DoubleKeyFramesModel));
                        break;
                    case IModelType.Color:_storyboard.Children.Add(Components.ColorComponents.BuildColorAnimation(item as ColorModel));
                        break;
                    case IModelType.ColorKeyFrames:_storyboard.Children.Add(Components.ColorComponents.BuildColorKeyFramesAnimation(item as ColorKeyFramesModel));
                        break;
                    case IModelType.Point:_storyboard.Children.Add(Components.PointComponents.BuildPointAnimation(item as PointModel));
                        break;
                    case IModelType.PointKeyFrames:_storyboard.Children.Add(Components.PointComponents.BuildPointKeyFramesAnimation(item as PointKeyFramesModel));
                        break;
                    case IModelType.ObjectKeyFrames:_storyboard.Children.Add(Components.ObjectComponents.BuildObjectKeyFramesAnimation(item as ObjectKeyFramesModel));
                        break;
                    default:
                        break;
                }
            }
            return _storyboard;
        }

    }
}
