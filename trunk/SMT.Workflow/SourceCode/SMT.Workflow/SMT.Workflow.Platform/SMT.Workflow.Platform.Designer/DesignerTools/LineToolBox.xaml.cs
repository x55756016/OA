/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：LineToolBox.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 14:21:11   
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

namespace SMT.Workflow.Platform.Designer.DesignerTools
{
    public partial class LineToolBox : UserControl, IToolBase
    {
        public LineToolBox()
        {
            InitializeComponent();
            PointCollection points = new PointCollection();
            points.Add(new Point(5, 5));          
            points.Add(new Point(65, 5));
            this.plShadow.Points = points;
            this.ShowMe(points);
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

        private double _ShadowX = -56;
        private double _ShadowY = 3;


        public ElementType Type
        {
            get { return ElementType.Line; }
        }

        private void Fill(Color color, double opacity)
        {
            this.lineTool.Stroke = new SolidColorBrush(color);
            this.lineTool.Opacity = opacity;
            this.arrowhead.Fill = new SolidColorBrush(color);
            this.arrowhead.Opacity = 1.0;
        }

        private PointCollection GetPloyline(PointCollection points, Point pOld, Point pNew)
        {
            double x = pNew.X - pOld.X;
            double y = pNew.Y - pOld.Y;

            PointCollection newPoints = new PointCollection();
            for (int i = 0; i < points.Count; i++)
            {
                newPoints.Add(new Point(points[i].X + x, points[i].Y + y));
            }
            return newPoints;
        }

        private bool CheckPoints(PointCollection points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].X < _CanvasLeft || points[i].X > _CanvasWidth) return false;
                if (points[i].Y < _CanvasTop || points[i].Y > _CanvasHeight) return false;
            }
            return true;
        }

        //private bool IsStraightLine()
        //{
        //    return (lineLeft.X1 == lineRight.X2 || lineLeft.Y1 == lineRight.Y2);
        //}

        private PointCollection GetMe()
        {
            PointCollection points = new PointCollection();
            Point pStart = new Point();          
            Point pEnd = new Point();

            pStart.X = Canvas.GetLeft(ellipseBegin) + ellipseBegin.Width / 2;
            pStart.Y = Canvas.GetTop(ellipseBegin) + ellipseBegin.Height / 2;
            pEnd.X = Canvas.GetLeft(ellipseEnd) + ellipseEnd.Width / 2;
            pEnd.Y = Canvas.GetTop(ellipseEnd) + ellipseEnd.Height / 2;        
            points.Add(pStart);       
            points.Add(pEnd);
            return points;
        }

        private void ShowShadow(PointCollection points)
        {
            plShadow.Points = points;
        }

        private void ShowMe(PointCollection points)
        {
            Point pBegin, pCenter,pEnd;
            pBegin = points[0];
            pEnd = points[1];
            pCenter = new Point(Math.Abs(pBegin.X - pEnd.X) / 2 + pBegin.X, Math.Abs(pBegin.Y - pEnd.Y) / 2 + pBegin.Y);
     
            //设置起始圆点的坐标
            Canvas.SetLeft(ellipseBegin, pBegin.X - ellipseBegin.Width / 2);
            Canvas.SetTop(ellipseBegin, pBegin.Y - ellipseBegin.Height / 2);
            //设置的坐标
            lineTool.X1 = pBegin.X; lineTool.Y1 = pBegin.Y;
            lineTool.X2 = pEnd.X; lineTool.Y2 = pEnd.Y;

            //设置中间菱形点的坐标
            Canvas.SetLeft(rectangleCenter, pCenter.X);
            Canvas.SetTop(rectangleCenter, pCenter.Y);
            ////调整高度要减去凌形的半径
            Canvas.SetTop(rectangleCenter, Canvas.GetTop(rectangleCenter) - rectangleCenter.Height * Math.Sin(45) + 1);
            //设置右边线的坐标
            //设置结束圆点的坐标            
            Canvas.SetLeft(ellipseEnd, pEnd.X - ellipseEnd.Width / 2);
            Canvas.SetTop(ellipseEnd, pEnd.Y - ellipseEnd.Height / 2);
            //设置箭头的坐标
            Canvas.SetLeft(arrowhead, pEnd.X);
            Canvas.SetTop(arrowhead, pEnd.Y);
            //if (pEnd == pCenterC) arrowhead.SetAngleByPoint(pCenterB, pCenterC);
            //else arrowhead.SetAngleByPoint(pCenterC, pEnd);
        }

        private UIElement _beginElement = null;
        public UIElement BeginElement
        {
            get { return this._beginElement; }
            set { this._beginElement = value; }
        }
        private HotspotType _beginElementHotspot;
        public HotspotType BeginElementHotspot
        {
            get { return this._beginElementHotspot; }
            set { this._beginElementHotspot = value; }
        }

        private UIElement _endElement = null;
        public UIElement EndElement
        {
            get { return this._endElement; }
            set { this._endElement = value; }
        }
        private HotspotType _endElementHotspot;
        public HotspotType EndElementHotspot
        {
            get { return this._endElementHotspot; }
            set { this._endElementHotspot = value; }
        }
        private void SetImageSourcce(FrameworkElement element, bool selected)
        {
            string strSource = "/SMT.Workflow.Platform.Designer;component/Images/{0}.jpg";

            if (selected)
                strSource = string.Format(strSource, "line");
            else
                strSource = string.Format(strSource, "line_noselect");

            ((System.Windows.Media.Imaging.BitmapImage)imgShape.Source).UriSource = new Uri(strSource, UriKind.Relative);
        }
        private void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            SetImageSourcce(element, true);

            if (element != null)
            {
                this.Fill(Colors.Blue, 0.8);
                element.Cursor = Cursors.Hand;
            }
        }

        private void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove != true)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                SetImageSourcce(element, false);
                if (element != null)
                {
                    this.Fill(Colors.Green, 0.8);
                    element.Cursor = Cursors.Arrow;
                }
            }
        }

        bool trackingPointMouseMove = false;
        bool pointHadActualMove = false;
        Point originalPosition, mousePosition;
        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pointHadActualMove = false;
            trackingPointMouseMove = false;
            this._container.ResetZoom();
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                this.Fill(Colors.Blue, 0.8);
                mousePosition = e.GetPosition(this._cnsParent);
                originalPosition = e.GetPosition(null);
                //originalPosition = mousePosition;
                element.Cursor = Cursors.Hand;

                trackingPointMouseMove = true;
                element.CaptureMouse();

                e.Handled = true;
            }
        }

        private void Line_MouseMove(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove)
            {
                if (mousePosition != e.GetPosition(this._cnsParent))
                {
                    plShadow.Points = GetPloyline(plShadow.Points, mousePosition, e.GetPosition(this._cnsParent));
                    mousePosition = e.GetPosition(this._cnsParent);
                    plShadow.Visibility = Visibility.Visible;
                }

                PointCollection points = new PointCollection();
                for (int i = 0; i < plShadow.Points.Count; i++)
                {
                    points.Add(new Point(plShadow.Points[i].X + this._ShadowX + this._startLocation.X + this._container.SimpleShapeLeft,
                                         plShadow.Points[i].Y + this._ShadowY + this._startLocation.Y));
                    //Point point = new Point(plShadow.Points[i].X + this._startLocation.X - 2 * imgShape.Width,
                    //                     plShadow.Points[i].Y + this._startLocation.Y - imgShape.Height / 2);
                    //points.Add(point);
                }
                if (CheckPoints(points) == true) pointHadActualMove = true;
                else pointHadActualMove = false;

                if (this.BeginElement != null)
                {
                    ((IControlBase)this.BeginElement).SetUnFocus();
                }
                this.BeginElement = this._container.GetElementByPoint(points[0]);
                if (this.BeginElement != null)
                {
                    if (((IControlBase)this.BeginElement).BeginLines.Count < ((IControlBase)this.BeginElement).MaxBeginLines) ((IControlBase)this.BeginElement).SetSelected();
                    else this.BeginElement = null;
                }

                if (this.EndElement != null)
                {
                    ((IControlBase)this.EndElement).SetUnFocus();
                }
                this.EndElement = this._container.GetElementByPoint(points[1]);
                if (this.EndElement != null)
                {
                    if (((IControlBase)this.EndElement).EndLines.Count < ((IControlBase)this.EndElement).MaxEndLines) ((IControlBase)this.EndElement).SetSelected();
                    else this.EndElement = null;
                }
            }
        }

        private void Line_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (trackingPointMouseMove)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                SetImageSourcce(element, false);
                if (element != null)
                {
                    this.Fill(Colors.Green, 1);
                    element.Cursor = Cursors.Arrow;
                    element.ReleaseMouseCapture();
                }

                if (pointHadActualMove == true)
                {
                    plShadow.Points = GetPloyline(plShadow.Points, mousePosition, e.GetPosition(this._cnsParent));
                    PointCollection points = new PointCollection();
                    for (int i = 0; i < plShadow.Points.Count; i++)
                    {
                        points.Add(new Point(plShadow.Points[i].X + this._ShadowX + this._startLocation.X + this._container.SimpleShapeLeft,
                                             plShadow.Points[i].Y + this._ShadowY + this._startLocation.Y));

                    }

                    if (this.BeginElement != null || this.EndElement != null)
                    {
                        Point pBegin, pEnd;
                        PointCollection points2 = new PointCollection();
                        if (this.BeginElement != null)
                        {
                            this.BeginElementHotspot = ((IControlBase)this.BeginElement).GetNearHotspot(points[0]);
                            pBegin = ((IControlBase)this.BeginElement).GetHotspot(this.BeginElementHotspot);
                            ((IControlBase)this.BeginElement).SetUnFocus();
                            this.BeginElement = null;
                        }
                        else pBegin = points[0];
                        if (this.EndElement != null)
                        {
                            this.EndElementHotspot = ((IControlBase)this.EndElement).GetNearHotspot(points[1]);
                            pEnd = ((IControlBase)this.EndElement).GetHotspot(this.EndElementHotspot);
                            ((IControlBase)this.EndElement).SetUnFocus();
                            this.EndElement = null;
                        }
                        else pEnd = points[1];

                        points2.Add(pBegin);
                        points2.Add(points[1]);
                        points2.Add(pEnd);
                        points.Clear();
                        for (int i = 0; i < points2.Count; i++) points.Add(points2[i]);
                    }
                    this._container.CreateElement(this.Type, points);

                    e.Handled = true;
                    pointHadActualMove = false;
                }

                if (mousePosition != originalPosition)
                {
                    PointCollection points = this.GetMe();
                    this.ShowShadow(points);
                }
                trackingPointMouseMove = false;
            }
            plShadow.Visibility = Visibility.Collapsed;
        }
    }
}
