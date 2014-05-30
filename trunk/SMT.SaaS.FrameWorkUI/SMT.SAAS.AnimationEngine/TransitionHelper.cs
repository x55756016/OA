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
    public class TransitionHelper
    {
        /// <summary>
        /// 根据数值,返回TransformGroup对象
        /// </summary>
        public static TransformGroup GetTransformGroupForXYOffset(double X, double Y, double scaleX, double scaleY)
        {
            ScaleTransform scaleTrans = new ScaleTransform() { ScaleX = scaleX, ScaleY = scaleY };
            RotateTransform rotateTrans = new RotateTransform() { Angle = 0.00D };
            TranslateTransform translateTrans = new TranslateTransform() { X = X, Y = Y };
            SkewTransform skewTrans = new SkewTransform();

            TransformGroup transGroup = new TransformGroup();
            transGroup.Children.Add(scaleTrans);
            transGroup.Children.Add(skewTrans);
            transGroup.Children.Add(rotateTrans);
            transGroup.Children.Add(translateTrans);

            return transGroup;
        }
        ///// <summary>
        ///// 返回TransformGroup对象
        ///// </summary>
        //public static TransformGroup GetTransformGroup(DependencyObject Target)
        //{
        //    TransformGroup transGroup = new TransformGroup();
        //    transGroup.Children.Add(new ScaleTransform());
        //    transGroup.Children.Add(new SkewTransform());
        //    transGroup.Children.Add(new RotateTransform());
        //    transGroup.Children.Add(new TranslateTransform());

        //    return transGroup;
        //}
        /// <summary>
        /// 返回TransformGroup对象
        /// </summary>
        public static TransformGroup GetTransformGroup(DependencyObject Target)
        {
            TransformGroup  transGroup = new TransformGroup();
            if ((Target as UIElement).RenderTransform != null)
            {
                Transform transf = (Target as UIElement).RenderTransform;
                if (transf is ScaleTransform)
                {
                    transGroup.Children.Add(transf);
                    transGroup.Children.Add(new SkewTransform());
                    transGroup.Children.Add(new RotateTransform());
                    transGroup.Children.Add(new TranslateTransform());
                }
                else if (transf is SkewTransform)
                {
                    transGroup.Children.Add(new ScaleTransform());
                    transGroup.Children.Add(transf);
                    transGroup.Children.Add(new RotateTransform());
                    transGroup.Children.Add(new TranslateTransform());
                }
                else if (transf is RotateTransform)
                {
                    transGroup.Children.Add(new ScaleTransform());
                    transGroup.Children.Add(new SkewTransform());
                    transGroup.Children.Add(transf);
                    transGroup.Children.Add(new TranslateTransform());
                }
                else if (transf is TranslateTransform)
                {
                    transGroup.Children.Add(new ScaleTransform());
                    transGroup.Children.Add(new SkewTransform());
                    transGroup.Children.Add(new RotateTransform());
                    transGroup.Children.Add(transf);
                }
                else if (transf is TransformGroup)
                {
                    TransformGroup temp = transf as TransformGroup;

                    ScaleTransform scaleTrans=null; 
                    RotateTransform rotateTrans=null;
                    TranslateTransform translateTrans=null;
                    SkewTransform skewTrans=null;
                    foreach (var item in temp.Children)
                    {
                        if (item is ScaleTransform)
                            scaleTrans = item as ScaleTransform;
                        else if (item is RotateTransform)
                            rotateTrans = item as RotateTransform;
                        else if (item is TranslateTransform)
                            translateTrans = item as TranslateTransform;
                        else if (item is SkewTransform)
                            skewTrans = item as SkewTransform;
                    }
                    transGroup.Children.Add(scaleTrans == null ? new ScaleTransform() : scaleTrans);
                    transGroup.Children.Add(skewTrans == null ? new SkewTransform() : skewTrans);
                    transGroup.Children.Add(rotateTrans == null ? new RotateTransform() : rotateTrans);
                    transGroup.Children.Add(scaleTrans == null ? new ScaleTransform() : scaleTrans);
                    transGroup.Children.Add(translateTrans == null ? new TranslateTransform() : translateTrans);
                }
            }
            else
            {
                transGroup.Children.Add(new ScaleTransform());
                transGroup.Children.Add(new SkewTransform());
                transGroup.Children.Add(new RotateTransform());
                transGroup.Children.Add(new TranslateTransform());
            }
            return transGroup;
        }
    }
}
