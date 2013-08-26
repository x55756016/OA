/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：TriangleArrow.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 9:23:26   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerShape 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/

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

namespace SMT.Workflow.Platform.Designer.DesignerShape
{
    public class TriangleArrow : System.Windows.Controls.Canvas
    {
        int arrowLenght = 10;
        int arrowAngle = 20;

        /// <summary>
        /// 箭头的长度
        /// </summary>
        public int ArrowLenght
        {
            get
            {
                return arrowLenght;
            }
            set
            {
                arrowLenght = value;
            }
        }
        /// <summary>
        /// 箭头的与直线的夹角
        /// </summary>
        public int ArrowAngle
        {
            get
            {
                return arrowAngle;
            }
            set
            {
                arrowAngle = value;
            }
        }
        Polygon polygonArrow;
        public int ZIndex
        {
            get
            {
                return (int)this.GetValue(Canvas.ZIndexProperty);
            }
            set
            {
                this.SetValue(Canvas.ZIndexProperty, value);
            }
        }
        public new double Opacity
        {
            get
            {
                return polygonArrow.Opacity;
            }
            set
            {
                polygonArrow.Opacity = value;

            }
        }
        public Brush Fill
        {
            get
            {
                return polygonArrow.Fill;
            }
            set
            {
                polygonArrow.Fill = value;
            }
        }

        public Brush Stroke
        {
            get
            {
                return polygonArrow.Stroke;
            }
            set
            {
                polygonArrow.Stroke = value;
            }
        }
        public double StrokeThickness
        {
            set
            {
                polygonArrow.StrokeThickness = value;
            }
            get
            {
                return polygonArrow.StrokeThickness;
            }
        }
        void SetAngleByDegree(double degreeLeft, double degreeRight)
        {
            polygonArrow.Points.Clear();
            polygonArrow.Points.Add(new Point(0, 0));
            double angleSi = Math.PI * degreeLeft / 180.0;
            double x = System.Math.Sin(Math.PI * degreeLeft / 180.0);
            double y = System.Math.Sin(Math.PI * (90 - degreeLeft) / 180.0);

            x = -ArrowLenght * x;
            y = -ArrowLenght * y;
            polygonArrow.Points.Add(new Point(x, y));
            x = System.Math.Sin(Math.PI * degreeRight / 180.0);
            y = System.Math.Sin(Math.PI * (90 - degreeRight) / 180.0);
            x = ArrowLenght * x;
            y = -ArrowLenght * y;
            polygonArrow.Points.Add(new Point(x, y));
        }
        /// <summary>
        /// 根据直线的起始点和结束点的坐标设置箭头的旋转角度
        /// </summary>
        /// <param name="beginPoint"></param>
        /// <param name="endPoint"></param>
        public void SetAngleByPoint(Point beginPoint, Point endPoint)
        {

            double x = endPoint.X - beginPoint.X;
            double y = endPoint.Y - beginPoint.Y;
            double angle = 0;
            if (y == 0)
            {
                if (x > 0)
                    angle = -90;
                else
                    angle = 90;

            }
            else
                angle = System.Math.Atan(x / y) * 180 / Math.PI;


            if (endPoint.Y <= beginPoint.Y)
                SetAngleByDegree((ArrowAngle + angle) - 180, (ArrowAngle - angle) - 180);
            else
                SetAngleByDegree(ArrowAngle + angle, ArrowAngle - angle);


        }
        public TriangleArrow()
        {
            polygonArrow = new Polygon();
            this.Children.Add(polygonArrow);

            SetAngleByPoint(new Point(0, 0), new Point(15, 0));
        }

    }
}
