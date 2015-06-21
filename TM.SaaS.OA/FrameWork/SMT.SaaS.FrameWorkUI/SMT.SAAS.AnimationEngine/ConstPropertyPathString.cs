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
    public class ConstPropertyPath
    {

        public const string UIELEMENT_WIDTH = "(UIElement.Width)";
        public const string UIELEMENT_HEIGHT = "(UIElement.Height)";
        public const string UIELEMENT_OPACITY = "(UIElement.Opacity)";
        public const string CANVAS_LEFT = "(Canvas.Left)";
        public const string CANVAS_TOP = "(Canvas.Top)";
        public const string CANVAS_ZINDEX = "(Canvas.ZIndex)";
        public const string SHAPE_FILL = "(Shape.Fill).(SolidColorBrush.Color)";
        public const string BORDER_BACKGROUND = "(Border.Background).(SolidColorBrush.Color)";


        #region PlaneProjection
        public const string UIELEMENT_PLANE_R0TATIONX = "(UIElement.Projection).(PlaneProjection.RotationX)";
        public const string UIELEMENT_PLANE_ROTATIONY = "(UIElement.Projection).(PlaneProjection.RotationY)";
        public const string UIELEMENT_PLANE_ROTATIONZ = "(UIElement.Projection).(PlaneProjection.RotationZ)";

        public const string UIELEMENT_PLANE_CENTERX = "(UIElement.Projection).(PlaneProjection.CenterOfRotationX)";
        public const string UIELEMENT_PLANE_CENTERY = "(UIElement.Projection).(PlaneProjection.CenterOfRotationY)";
        public const string UIELEMENT_PLANE_CENTERZ = "(UIElement.Projection).(PlaneProjection.CenterOfRotationZ)";

        public const string UIELEMENT_PLANE_GLOBALX = "(UIElement.Projection).(PlaneProjection.GlobalOffsetX)";
        public const string UIELEMENT_PLANE_GLOBALY = "(UIElement.Projection).(PlaneProjection.GlobalOffsetY)";
        public const string UIELEMENT_PLANE_GLOBALZ = "(UIElement.Projection).(PlaneProjection.GlobalOffsetZ)";

        public const string UIELEMENT_PLANE_LOCALX = "(UIElement.Projection).(PlaneProjection.LocalOffsetX)";
        public const string UIELEMENT_PLANE_LOCALY = "(UIElement.Projection).(PlaneProjection.LocalOffsetY)";
        public const string UIELEMENT_PLANE_LOCALZ = "(UIElement.Projection).(PlaneProjection.LocalOffsetZ)";
        //CenterOfRotationX 获取或设置所旋转对象的旋转中心 X 坐标。  
        //CenterOfRotationY 获取或设置所旋转对象的旋转中心 Y 坐标。 
        //CenterOfRotationZ 获取或设置所旋转对象的旋转中心 Z 坐标。 

        //GlobalOffsetX 获取或设置沿屏幕的 X 轴平移对象的距离。  
        //GlobalOffsetY 获取或设置沿屏幕的 Y 轴平移对象的距离。  
        //GlobalOffsetZ 获取或设置沿屏幕的 Z 轴平移对象的距离。

        //LocalOffsetX 获取或设置沿对象平面的 X 轴平移对象的距离。  
        //LocalOffsetY 获取或设置沿对象平面的 Y 轴平移对象的距离。 
        //LocalOffsetZ 获取或设置沿对象平面的 Z 轴平移对象的距离。

        //RotationX 获取或设置围绕旋转的 X 轴旋转对象的角度。 
        //RotationY 获取或设置围绕旋转的 Y 轴旋转对象的角度。 
        //RotationZ 获取或设置围绕旋转的 Z 轴旋转对象的角度。 

        #endregion
        #region TransformGroup
        public const string UIELEMENT_SCALETRS_SCALEX = "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)";
        public const string UIELEMENT_SCALETRS_SCALEY = "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)";
        public const string UIELEMENT_SCALETRS_CENTERX = "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.CenterX)";
        public const string UIELEMENT_SCALETRS_CENTERY = "(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.CenterY)";
        public const string UIELEMENT_SKEWTRS_ANGLEX = "(UIElement.RenderTransform).(TransformGroup.Children)[1].(SkewTransform.AngleX)";
        public const string UIELEMENT_SKEWTRS_ANGLEY = "(UIElement.RenderTransform).(TransformGroup.Children)[1].(SkewTransform.AngleY)";
        public const string UIELEMENT_SKEWTRS_CENTERX = "(UIElement.RenderTransform).(TransformGroup.Children)[1].(SkewTransform.CenterX)";
        public const string UIELEMENT_SKEWTRS_CENTERY = "(UIElement.RenderTransform).(TransformGroup.Children)[1].(SkewTransform.CenterY)";
        public const string UIELEMENT_ROTATETRS_ANGLE = "(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)";
        public const string UIELEMENT_ROTATETRS_CENTERX = "(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.CenterX)";
        public const string UIELEMENT_ROTATETRS_CENTERY = "(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.CenterY)";
        public const string UIELEMENT_TRANSLATETRS_X = "(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)";
        public const string UIELEMENT_TRANSLATETRS_Y = "(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.Y)";
        #endregion





    }
}
