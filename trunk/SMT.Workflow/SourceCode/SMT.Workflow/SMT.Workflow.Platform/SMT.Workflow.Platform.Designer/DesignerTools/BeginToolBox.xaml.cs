/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：BeginToolBox.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 13:46:25   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerTools 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.Workflow.Platform.Designer.DesignerView;
using SMT.Workflow.Platform.Designer.DesignerControl;
using SMT.Workflow.Platform.Designer.DesignerShape;

namespace SMT.Workflow.Platform.Designer.DesignerTools
{
    public partial class BeginToolBox : UserControl, IToolBase
    {        

        public BeginToolBox()
        {
            InitializeComponent();
            //this.BeginTool.SetUnFocus();            
        }

        private IContainer _container;
        private Point _startLocation;  
        private Canvas _cnsParent;
        private double _CanvasLeft = 0;
        private double _CanvasWidth = 0;
        private double _CanvasTop = 0;
        private double _CanvasHeight = 0;
        public void InitializeMe(IContainer container, Point startLocation, Canvas cnsParent, double canvasLeft, double canvasTop)
        {
            this._container = container;
            this._startLocation = startLocation;
            this._cnsParent = cnsParent;
            this._CanvasLeft = canvasLeft;
            this._CanvasTop = canvasTop;
            this._CanvasWidth = cnsParent.Width;
            this._CanvasHeight = cnsParent.Height;
        }

        private double _ShadowX = 3;
        private double _ShadowY = 3;

        public ElementType Type
        {
            get { return ElementType.Begin; }
        }

        private void ShowShadow(Point point, PointType pointType)
        {
            if (pointType == PointType.Precision) //精确坐标点
            {
                Canvas.SetLeft(Shadow, point.X);
                Canvas.SetTop(Shadow, point.Y);
            }
            else  //偏移坐标点
            {
                Canvas.SetLeft(Shadow, point.X - this.x + this._ShadowX);
                Canvas.SetTop(Shadow, point.Y - this.y + this._ShadowX);
            }
        }

        private Point GetMe()
        {
            return new Point(Canvas.GetLeft(BeginTool) + this._ShadowX, Canvas.GetTop(BeginTool) + this._ShadowY);
        }

        private bool CheckPoint(Point mousePosition)
        {
            if (mousePosition.X - (originalPosition.X - this._startLocation.X) + this._container.SimpleShapeLeft < _CanvasLeft
                || mousePosition.X - (originalPosition.X - this._startLocation.X) + this._container.SimpleShapeLeft > _CanvasWidth) return false;
            if (mousePosition.Y - (originalPosition.Y - this._startLocation.Y) < _CanvasTop 
                || mousePosition.Y - (originalPosition.Y - this._startLocation.Y) > _CanvasHeight) return false;
            return true;
        }

        private void SetImageSourcce(FrameworkElement element, bool selected)
        {
            string strSource = "/SMT.Workflow.Platform.Designer;component/Images/{0}.jpg";

            if (selected)
                strSource = string.Format(strSource, "begin");
            else
                strSource = string.Format(strSource, "begin_noselect");

            ((System.Windows.Media.Imaging.BitmapImage)BeginTool.Source).UriSource = new Uri(strSource, UriKind.Relative);
        }

        private void Shape_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                SetImageSourcce(element, true);
                //((IShape)this.BeginTool).Fill(Colors.Blue, 0.8);
                element.Cursor = Cursors.Hand;
            }
        }

        private void Shape_MouseLeave(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove != true)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    SetImageSourcce(element, false);
                    //((IShape)this.BeginTool).Fill(Colors.Green, 1);
                    element.Cursor = Cursors.Arrow;
                }
            }
        }

        bool trackingPointMouseMove = false;
        bool pointHadActualMove = false;
        Point originalPosition, mousePosition;
        double x, y;
        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pointHadActualMove = false;
            trackingPointMouseMove = false;
            this._container.ResetZoom();
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                //((IShape)this.BeginTool).Fill(Colors.Blue, 0.8);

                mousePosition = e.GetPosition(this._cnsParent);
                //originalPosition = e.GetPosition(null);
                originalPosition = mousePosition;

                x = mousePosition.X - Canvas.GetLeft(BeginTool);
                y = mousePosition.Y - Canvas.GetTop(BeginTool);

                element.Cursor = Cursors.Hand;

                trackingPointMouseMove = true;
                element.CaptureMouse();

                e.Handled = true;
            }
        }

        private void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove)
            {
                if (mousePosition != e.GetPosition(this._cnsParent))
                {
                    Shadow.Visibility = Visibility.Visible;
                    mousePosition = e.GetPosition(this._cnsParent);
                    this.ShowShadow(mousePosition, PointType.Excursion);

                    if (CheckPoint(mousePosition) == true) pointHadActualMove = true;
                    else pointHadActualMove = false;
                }
            }
        }

        private void Shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (trackingPointMouseMove)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                SetImageSourcce(element, false);
                if (element != null)
                {
                    //((IShape)this.BeginTool).Fill(Colors.Green, 1);
                    element.Cursor = Cursors.Arrow;
                    element.ReleaseMouseCapture();
                }
                if (pointHadActualMove == true)
                {
                    //==modified by jason, 10/09/2011
                    //Point point = new Point(mousePosition.X - (originalPosition.X - this._startLocation.X) + this._container.SimpleShapeLeft,
                    //                        mousePosition.Y - (originalPosition.Y - this._startLocation.Y));
                    double _x = mousePosition.X - (originalPosition.X - this._startLocation.X) - 2 * BeginTool.Width;
                    double _y = mousePosition.Y - (originalPosition.Y - this._startLocation.Y) - BeginTool.Height / 2;

                    if (_x <= 0) _x = 5;
                    if (_y <= 0) _y = 5;

                    Point point = new Point(_x, _y);
                    PointCollection points = new PointCollection();
                    points.Add(point);
                    this._container.CreateElement(this.Type, points);

                    e.Handled = true;
                    pointHadActualMove = false;
                }

                if (mousePosition != originalPosition)
                {
                    Point point = GetMe();
                    this.ShowShadow(point, PointType.Precision);
                }
                trackingPointMouseMove = false;
            }
            Shadow.Visibility = Visibility.Collapsed;
        }
    }
}
