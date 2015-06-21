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
using System.Collections.Generic;

namespace SMT.SAAS.AnimationEngine
{
    /// <summary>
    /// 公共动画类,适应于所有的FrameworkElement类型的类
    /// </summary>
    public class CommonAnimation
    {
        
        /// <summary>
        /// 控制窗口位置、大小、透明度动画
        /// </summary>
        public static Storyboard PrepareWindow(FrameworkElement element, double seconds, Point fromLocation, Point toLocation, Size fromSize, Size toSize, double fromOpacity, double toOpacity, bool isUseOpacity)
        {
            //DoubleModel[] animationList = new DoubleModel[] {
            //new DoubleModel(){Target=element,From=fromLocation.X,To=toLocation.X,PropertyPath=ConstPropertyPathString.CANVS_LEFT},
            //new DoubleModel(){Target=element,From=fromLocation.Y,To=toLocation.Y,PropertyPath=ConstPropertyPathString.CANVS_TOP},
            //new DoubleModel(){Target=element,From=fromSize.Width,To=toSize.Width,PropertyPath=ConstPropertyPathString.CONTROL_WIDTH},
            //new DoubleModel(){Target=element,From=fromSize.Height,To=toSize.Height,PropertyPath=ConstPropertyPathString.CONTROL_HEIGHT}
            //};
            Storyboard storyboard = new Storyboard();
            DoubleAnimation _left = Components.DoubleComponents.GetDoubleAnimation(ConstPropertyPath.CANVAS_LEFT, seconds, fromLocation.X, toLocation.X, null);
            DoubleAnimation _top = Components.DoubleComponents.GetDoubleAnimation(ConstPropertyPath.CANVAS_TOP, seconds, fromLocation.Y, toLocation.Y, null);
            DoubleAnimation _width = Components.DoubleComponents.GetDoubleAnimation(ConstPropertyPath.UIELEMENT_WIDTH, seconds, fromSize.Width, toSize.Width, null);
            DoubleAnimation _height = Components.DoubleComponents.GetDoubleAnimation(ConstPropertyPath.UIELEMENT_HEIGHT, seconds, fromSize.Height, toSize.Height, null);

            Storyboard.SetTarget(_left, element);
            Storyboard.SetTarget(_top, element);
            Storyboard.SetTarget(_width, element);
            Storyboard.SetTarget(_height, element);

            storyboard.Children.Add(_left);
            storyboard.Children.Add(_top);
            storyboard.Children.Add(_width);
            storyboard.Children.Add(_height);

            if (isUseOpacity)
            {
                DoubleAnimation _opacity = Components.DoubleComponents.GetDoubleAnimation(ConstPropertyPath.UIELEMENT_OPACITY, seconds, fromOpacity, toOpacity, null);
                storyboard.Children.Add(_opacity);
                Storyboard.SetTarget(_opacity, element);
            }

             

            return storyboard;
        }
        /// <summary>
        /// 由小到大显示Element
        /// </summary>
        /// <param name="Element">需要应用Storyboard的对象</param>
        /// <param name="seconds">动画处理时间</param>
        /// <returns>已经根据Element封装好的动画,返回的Storyboard直接使用Begin函数即可</returns>
        public static Storyboard PrepareGrowStoryBoard(FrameworkElement Element, double seconds)
        {
            PlaneProjection _planePrj = new PlaneProjection();
            Element.Projection = _planePrj;
            Storyboard _storyboard = new Storyboard();
            TransformGroup transGroup = new TransformGroup();
            ScaleTransform scaletrans = new ScaleTransform();
            transGroup.Children.Add(scaletrans);
            scaletrans.CenterX = 0.5;
            scaletrans.CenterY = 0.5;
            Element.RenderTransform = transGroup;
            Element.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation doubleX = new DoubleAnimation();
            doubleX.From = 0;
            doubleX.To = 1;
            doubleX.FillBehavior = FillBehavior.HoldEnd;
            _storyboard.Children.Add(doubleX);

            DoubleAnimation doubleY = new DoubleAnimation();
            doubleY.From = 0;
            doubleY.To = 1;
            doubleY.FillBehavior = FillBehavior.HoldEnd;
            _storyboard.Children.Add(doubleY);
            //调用封装透明度和旋转X轴方法
            AddDoubleAnimation(Element, seconds, _storyboard);

            Storyboard.SetTarget(doubleX, Element);
            Storyboard.SetTargetProperty(doubleX,
               new PropertyPath("(UIElement.RenderTransform).(transGroup.Children)[0].(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(doubleY, Element);
            Storyboard.SetTargetProperty(doubleY,
               new PropertyPath("(UIElement.RenderTransform).(transGroup.Children)[0].(ScaleTransform.ScaleY)"));
            _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
            return _storyboard;
        }
        /// <summary>
        /// 由（屏幕）下至上显示Element
        /// </summary>
        /// <param name="Element">需要应用Storyboard的对象</param>
        /// <param name="seconds">动画处理时间</param>
        /// <returns>已经根据Element封装好的动画,返回的Storyboard直接使用Begin函数即可</returns>
        public static Storyboard PrepareSlideUpStoryBoard(FrameworkElement Element, double seconds)
        {
            PlaneProjection _planePrj = new PlaneProjection();
            Element.Projection = _planePrj;
            Storyboard _storyboard = new Storyboard();
            TransformGroup transGroup = new TransformGroup();
            TranslateTransform translate = new TranslateTransform();
            transGroup.Children.Add(translate);
            Element.RenderTransform = transGroup;
            Element.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation doubleY = new DoubleAnimation();
            doubleY.From = Element.ActualHeight;
            doubleY.To = 0;
            doubleY.FillBehavior = FillBehavior.HoldEnd;
            doubleY.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            _storyboard.Children.Add(doubleY);

            AddDoubleAnimation(Element, seconds, _storyboard);

            Storyboard.SetTarget(doubleY, Element);
            Storyboard.SetTargetProperty(doubleY,
               new PropertyPath("(UIElement.RenderTransform).(transGroup.Children)[0].(TranslateTransform.Y)"));

            _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
            return _storyboard;
        }
        /// <summary>
        /// 由（屏幕）上至下显示Element
        /// </summary>
        /// <param name="Element">需要应用Storyboard的对象</param>
        /// <param name="seconds">动画处理时间</param>
        /// <returns>已经根据Element封装好的动画,返回的Storyboard直接使用Begin函数即可</returns>
        public static Storyboard PrepareSlideDownStoryBoard(FrameworkElement Element, double seconds)
        {
            PlaneProjection _planePrj = new PlaneProjection();
            Element.Projection = _planePrj;
            Storyboard _storyboard = new Storyboard();
            TransformGroup transGroup = new TransformGroup();
            TranslateTransform translate = new TranslateTransform();
            transGroup.Children.Add(translate);
            Element.RenderTransform = transGroup;
            Element.RenderTransformOrigin = new Point(0.5, 0.5);
            DoubleAnimation doubleY = new DoubleAnimation();
            doubleY.From = -Element.ActualHeight;
            doubleY.To = 0;
            doubleY.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            doubleY.FillBehavior = FillBehavior.HoldEnd;
            _storyboard.Children.Add(doubleY);
            //调用封装透明度和旋转X轴方法
            AddDoubleAnimation(Element, seconds, _storyboard);
            Storyboard.SetTarget(doubleY, Element);
            Storyboard.SetTargetProperty(doubleY,
               new PropertyPath("(UIElement.RenderTransform).(transGroup.Children)[0].(TranslateTransform.Y)"));

            _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
            return _storyboard;
        }
        /// <summary>
        /// 由（屏幕）左至右显示Element
        /// </summary>
        /// <param name="Element">目标对象,你的Element</param>
        /// <param name="seconds">动画处理时间</param>
        /// <returns>已经根据Element封装好的动画,返回的Storyboard直接使用Begin函数即可</returns>
        public static Storyboard PrepareSlideInFromLeftStoryBoard(FrameworkElement Element, double seconds)
        {
            PlaneProjection _planePrj = new PlaneProjection();
            Element.Projection = _planePrj;
            Storyboard _storyboard = new Storyboard();
            TransformGroup transGroup = new TransformGroup();
            TranslateTransform translate = new TranslateTransform();
            transGroup.Children.Add(translate);
            Element.RenderTransform = transGroup;
            Element.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation doubleX = new DoubleAnimation();
            doubleX.From = -Element.ActualWidth;
            doubleX.To = 0;
            doubleX.FillBehavior = FillBehavior.HoldEnd;
            doubleX.Duration = new Duration(TimeSpan.FromSeconds(seconds));
            _storyboard.Children.Add(doubleX);

            AddDoubleAnimation(Element, seconds, _storyboard);

            Storyboard.SetTarget(doubleX, Element);
            Storyboard.SetTargetProperty(doubleX,
               new PropertyPath("(UIElement.RenderTransform).(transGroup.Children)[0].(TranslateTransform.X)"));
            _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
            return _storyboard;
        }
        /// <summary>
        /// 为Storyboard加入处理Opacity/RotationX的DoubleAnimation
        /// </summary>
        private static void AddDoubleAnimation(FrameworkElement Element, double seconds, Storyboard _storyboard)
        {
            DoubleAnimation _opactityDA = Components.DoubleComponents.GetDoubleAnimation(ConstPropertyPath.UIELEMENT_OPACITY, seconds, 0, 1, null);
            DoubleAnimation _rotatoinXDA = Components.DoubleComponents.GetDoubleAnimation(ConstPropertyPath.UIELEMENT_PLANE_R0TATIONX, seconds, 90, 0, null);
            Storyboard.SetTarget(_opactityDA, Element);
            _storyboard.Children.Add(_opactityDA);
            Storyboard.SetTarget(_rotatoinXDA, Element);
            _storyboard.Children.Add(_rotatoinXDA);
        }



        //    /// <summary>
        ///// 由（屏幕）右至左显示Element
        ///// </summary>
        ///// <param name="Element">目标对象,你的Element</param>
        ///// <param name="seconds">动画处理时间</param>
        ///// <returns>已经根据Element封装好的动画,返回的Storyboard直接使用Begin函数即可</returns>
        //public static Storyboard PrepareSlideInFromRightStoryBoard(FrameworkElement Element, double seconds, double width)
        //{
        //    // Element.CacheMode = new BitmapCache();
        //    Storyboard _storyboard = new Storyboard();
        //    TransformGroup transGroup = new TransformGroup();
        //    TranslateTransform translate = new TranslateTransform();
        //    transGroup.Children.Add(translate);
        //    Element.RenderTransform = transGroup;
        //    Element.RenderTransformOrigin = new Point(0.5, 0.5);

        //    DoubleAnimation doubleX = new DoubleAnimation();
        //    doubleX.From = width;
        //    doubleX.To = 0;
        //    doubleX.FillBehavior = FillBehavior.HoldEnd;
        //    doubleX.Duration = new Duration(TimeSpan.FromSeconds(seconds));

        //    _storyboard.Children.Add(doubleX);

        //    Storyboard.SetTarget(doubleX, Element);
        //    Storyboard.SetTargetProperty(doubleX,
        //       new PropertyPath("(UIElement.RenderTransform).(transGroup.Children)[0].(TranslateTransform.X)"));

        //    _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
        //    return _storyboard;
        //}   
        ///// <summary>
        ///// 设置Element沿X轴旋转的角度
        ///// </summary>
        ///// <param name="Element">需要应用Storyboard的对象</param>
        ///// <param name="seconds">动画时间</param>
        ///// <param name="from">起始角度</param>
        ///// <param name="to">结束角度</param>
        ///// <returns></returns>
        //public static Storyboard PrepareRotationXStoryBoard(FrameworkElement Element, double seconds, double from, double to)
        //{
        //    PlaneProjection _planePrj = new PlaneProjection();
        //    Element.Projection = _planePrj;
        //    Storyboard _storyboard = new Storyboard();
        //    DoubleAnimation _rotationXDA = Components.DoubleComponents.GetDoubleAnimation(PropertyPathString.CONTROL_ROTATIONX, seconds, from, to, null);
        //    Storyboard.SetTarget(_rotationXDA, Element);
        //    _storyboard.Children.Add(_rotationXDA);
        //    _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
        //    return _storyboard;
        //}
        ///// <summary>
        ///// 设置Element沿Y轴旋转的角度
        ///// </summary>
        ///// <param name="Element">需要应用Storyboard的对象</param>
        ///// <param name="seconds">动画时间</param>
        ///// <param name="from">起始角度</param>
        ///// <param name="to">结束角度</param>
        ///// <returns></returns>
        //public static Storyboard PrepareRotationYStoryBoard(FrameworkElement Element, double seconds, double from, double to)
        //{
        //    PlaneProjection prj = new PlaneProjection();
        //    Element.Projection = prj;
        //    Storyboard _storyboard = new Storyboard();
        //    DoubleAnimation _rotationYDA = Components.DoubleComponents.GetDoubleAnimation(PropertyPathString.CONTROL_ROTATIONY, seconds, from, to, null);
        //    Storyboard.SetTarget(_rotationYDA, Element);
        //    _storyboard.Children.Add(_rotationYDA);
        //    _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
        //    return _storyboard;
        //}
        ///// <summary>
        ///// 控制Element透明度动画
        ///// </summary>
        ///// <param name="Element">需要应用Storyboard的对象</param>
        ///// <param name="seconds">动画处理时间</param>
        ///// <returns>已经根据Element封装好的动画,返回的Storyboard直接使用Begin函数即可</returns>
        //public static Storyboard PrepareOpacityStoryBoard(FrameworkElement Element, double seconds, double from, double to)
        //{
        //    Storyboard _storyboard = new Storyboard();
        //    DoubleAnimation _dopactity = Components.DoubleComponents.GetDoubleAnimation(PropertyPathString.CONTROL_OPACITY, seconds, from, to, null);
        //    Storyboard.SetTarget(_dopactity, Element);
        //    _storyboard.Duration = new Duration(TimeSpan.FromSeconds(seconds));
        //    _storyboard.Children.Add(_dopactity);
        //    _storyboard.Completed += (s, e) => { _storyboard.Stop(); };
        //    return _storyboard;
        //}

        private Storyboard sb()
        {
            List<IModel> models = new List<IModel>();
           // models.Add
          //  Engine


            return null;
        }
    }
}
