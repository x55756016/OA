/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：LineShape.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 9:27:05   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerShape 
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
using SMT.Workflow.Platform.Designer.DesignerControl;
using SMT.Workflow.Platform.Designer.DesignerView;

namespace SMT.Workflow.Platform.Designer.DesignerShape
{
    public partial class LineShape : UserControl, IShape
    {
        public LineShape()
        {
            InitializeComponent();
            PointCollection points = new PointCollection();
            points.Add(new Point(5, 5));
            points.Add(new Point(65, 5));
            plShadow.Points = points;
            ShowMe(points);
            SetFocus();
        }

        public void SetRange(double canvasLeft, double canvasTop, double canvasWidth, double canvasHeight)
        {
            this._CanvasLeft = canvasLeft;
            this._CanvasTop = canvasTop;
            this._CanvasWidth = canvasWidth;
            this._CanvasHeight = canvasHeight;
        }

        private UIElement _UIContainter = null;
        public UIElement UIContainer
        {
            get { return this._UIContainter; }
            set { this._UIContainter = value; }
        }

        private double _CanvasLeft = 0;
        private double _CanvasWidth = 0;
        private double _CanvasTop = 0;
        private double _CanvasHeight = 0;

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

        private enum LineMoveType { None = 0, Begin, Center, End, Line };
        private LineMoveType _MoveType = LineMoveType.Begin;

        private ElementState _State = ElementState.UnFocus;
        /// <summary>
        /// 获取焦点状态
        /// </summary>
        public ElementState State
        {
            get { return _State; }
        }

        private IContainer _container;
        public IContainer Container
        {
            get
            {
                return _container;
            }
            set
            {
                _container = value;
            }
        }

        private LineControl _lineControl;
        public LineControl lineControl
        {
            get { return this._lineControl; }
            set { this._lineControl = value; }
        }

        public void SetFocus()
        {
            if (this._State != ElementState.Focus)
            {
                SetHotspotStyle(Colors.Yellow, 1.0);
                ellipseBegin.Visibility = Visibility.Visible;
                rectangleCenter.Visibility = Visibility.Visible;
                ellipseEnd.Visibility = Visibility.Visible;
                //直线状态不显示中间点
                //if (IsStraightLine() == true) rectangleCenter.Visibility = Visibility.Collapsed;

                this._State = ElementState.Focus;
            }
        }
        public void SetUnFocus()
        {
            if (this._State != ElementState.UnFocus)
            {
                SetHotspotStyle(Colors.Blue, 0.8);
                ellipseBegin.Visibility = Visibility.Collapsed;
                rectangleCenter.Visibility = Visibility.Collapsed;
                ellipseEnd.Visibility = Visibility.Collapsed;

                this._State = ElementState.UnFocus;
            }
        }
        public void SetSelected()
        {
            if (this._State != ElementState.Selected)
            {
                SetHotspotStyle(Colors.Red, 1.0);
                ellipseBegin.Visibility = Visibility.Visible;
                rectangleCenter.Visibility = Visibility.Visible;
                ellipseEnd.Visibility = Visibility.Visible;

                this._State = ElementState.Selected;
            }
        }

        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            this.txtbActivityTitle.Text = title;
        }

        /// <summary>
        /// 获取标题
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return this.txtbActivityTitle.Text;
        }


        public void Fill(Color color, double opacity)
        {
            this.lineShape.Stroke = new SolidColorBrush(color);
            this.lineShape.Opacity = opacity;
            this.arrowhead.Fill = new SolidColorBrush(color);
            this.arrowhead.Opacity = 1.0;
        }

        /// <summary>
        /// 设置热点的颜色
        /// </summary>
        /// <param name="color"></param>
        private void SetHotspotStyle(Color color, double opacity)
        {
            ellipseBegin.Fill = new SolidColorBrush(color);
            ellipseBegin.Opacity = opacity;
            rectangleCenter.Opacity = opacity;
            ellipseEnd.Fill = new SolidColorBrush(color);
            ellipseEnd.Opacity = opacity;
        }

        private Point previousPoint;
        public void SetPreviousPoint(Point mousePoint)
        {
            previousPoint = mousePoint;
        }

        public void ShowShadow(Point mousePoint, PointType pointType)
        {
            if (pointType == PointType.Excursion)
            {
                plShadow.Points = this.GetPloyline(plShadow.Points, previousPoint, mousePoint);
                previousPoint = mousePoint;
            }
            else
            {
                plShadow.Points = GetMe();
            }
        }

        public void ShowMe()
        {
            ShowMe(plShadow.Points);
        }

        public void ShowShadow(Point pBegin, Point pEnd)
        {
            plShadow.Points = this.GetPloyline(pBegin, pEnd);
        }

        public void ShowShadow(PointCollection points)
        {
            plShadow.Points = points;
        }

        public void ShowMe(Point pBegin, Point pEnd)
        {
            plShadow.Points = this.GetPloyline(pBegin, pEnd);
            ShowMe(plShadow.Points);
        }

        public bool IsInside(Point point, double x, double y)
        {
            Point pBegin = plShadow.Points[0];
            Point pEnd = plShadow.Points[1];

            //if (this.GetLineShape() == LineType.Z)
            //{
            //    if (((pEnd.X >= point.X && pBegin.X <= point.X + x) || (pBegin.X >= point.X && pEnd.X <= point.X + x))
            //        && ((pEnd.Y > point.Y && pBegin.Y < point.Y + y) || (pBegin.Y > point.Y && pEnd.Y < point.Y + y)))
            //    {
            //        return true;
            //    }
            //}
            //else
            //{
            //    if (((pEnd.Y >= point.Y && pBegin.Y <= point.Y + y) || (pBegin.Y >= point.Y && pEnd.Y <= point.Y + y))
            //        && ((pEnd.X > point.X && pBegin.X < point.X + x) || (pBegin.X > point.X && pEnd.X < point.X + x)))
            //    {
            //        return true;
            //    }
            //}
            return false;
        }

        //显示控件
        public void ShowMe(PointCollection points)
        {
            Point pBegin, pCenter, pEnd;
            pBegin = points[0];
            pEnd = points[1];
            pCenter = new Point((pBegin.X < pEnd.X ? pBegin.X : pEnd.X) + Math.Abs(pBegin.X - pEnd.X) / 2, (pBegin.Y < pEnd.Y ? pBegin.Y : pEnd.Y) + Math.Abs(pBegin.Y - pEnd.Y) / 2);

            //设置起始圆点的坐标
            Canvas.SetLeft(ellipseBegin, pBegin.X - ellipseBegin.Width / 2);
            Canvas.SetTop(ellipseBegin, pBegin.Y - ellipseBegin.Height / 2);
            //设置的坐标
            lineShape.X1 = pBegin.X; lineShape.Y1 = pBegin.Y;
            lineShape.X2 = pEnd.X; lineShape.Y2 = pEnd.Y;
            //设置的坐标
            lineShapeHide.X1 = pBegin.X; lineShapeHide.Y1 = pBegin.Y;
            lineShapeHide.X2 = pEnd.X; lineShapeHide.Y2 = pEnd.Y;
            //线的标题坐标
            Canvas.SetLeft(txtbActivityTitle, pCenter.X);
            Canvas.SetTop(txtbActivityTitle, pCenter.Y);

            //设置中间菱形点的坐标
            Canvas.SetLeft(rectangleCenter, pCenter.X);
            Canvas.SetTop(rectangleCenter, pCenter.Y);
            ////调整高度要减去凌形的半径
            Canvas.SetTop(rectangleCenter, Canvas.GetTop(rectangleCenter) - rectangleCenter.Height * Math.Sin(45) + 1);

            //设置结束圆点的坐标            
            Canvas.SetLeft(ellipseEnd, pEnd.X - ellipseEnd.Width / 2);
            Canvas.SetTop(ellipseEnd, pEnd.Y - ellipseEnd.Height / 2);
            //设置箭头的坐标
            Canvas.SetLeft(arrowhead, pEnd.X);
            Canvas.SetTop(arrowhead, pEnd.Y);         
            arrowhead.SetAngleByPoint(pBegin, pEnd);
            //else arrowhead.SetAngleByPoint(pCenterC, pEnd);
        }

        /// <summary>
        /// 获取当前线的三点坐标
        /// </summary>
        /// <returns></returns>
        public PointCollection GetMe()
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

        /// <summary>
        /// 返回两点的三点坐标
        /// </summary>
        /// <param name="pBegin"></param>
        /// <param name="pEnd"></param>
        /// <returns></returns>
        public PointCollection GetPloyline(Point pBegin, Point pEnd)
        {
            PointCollection points = new PointCollection();
            points.Add(pBegin);
            points.Add(pEnd);
            return points;
        }
        private PointCollection GetPloyline(Point pCenter)
        {
            PointCollection points = new PointCollection();
            Point pStart = new Point(Canvas.GetLeft(ellipseBegin) + ellipseBegin.Width / 2, Canvas.GetTop(ellipseBegin) + ellipseBegin.Height / 2);

            Point pEnd = new Point(Canvas.GetLeft(ellipseEnd) + ellipseEnd.Width / 2, Canvas.GetTop(ellipseEnd) + ellipseEnd.Height / 2);

            points.Add(pStart);
            points.Add(pEnd);
            return points;
        }
        public PointCollection GetPloyline(PointCollection points, Point pOld, Point pNew)
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
        public bool CheckPoints(PointCollection points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].X < _CanvasLeft || points[i].X > _CanvasWidth) return false;
                if (points[i].Y < _CanvasTop || points[i].Y > _CanvasHeight) return false;
            }
            return true;
        }



        private void Hotspot_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.Cursor = Cursors.Hand;
            }
        }

        private void Hotspot_MouseLeave(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove != true)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    element.Cursor = Cursors.Arrow;
                }
            }
        }

        bool trackingPointMouseMove = false;
        bool pointHadActualMove = false;
        Point mousePosition;
        private void Hotspot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Container.IsMultiSelected(this.lineControl)) return;

            pointHadActualMove = false;
            trackingPointMouseMove = false;

            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                _MoveType = LineMoveType.None;
                if (element.Name == ellipseBegin.Name)
                {
                    _MoveType = LineMoveType.Begin;
                    element.Cursor = Cursors.Hand;
                }
                else if (element.Name == rectangleCenter.Name)
                {
                    _MoveType = LineMoveType.None;
                    element.Cursor = Cursors.Hand;
                }
                else if (element.Name == ellipseEnd.Name)
                {
                    _MoveType = LineMoveType.End;
                    element.Cursor = Cursors.Hand;
                }
                else if (element.Name == lineShape.Name ||
                    element.Name == arrowhead.Name)
                {
                    _MoveType = LineMoveType.Line;
                    mousePosition = e.GetPosition(this.UIContainer);
                    element.Cursor = Cursors.Hand;
                }

                if (_MoveType != LineMoveType.None)
                {
                    trackingPointMouseMove = true;
                    element.CaptureMouse();
                }
            }
        }

        bool linkElement = false;
        private void Hotspot_MouseMove(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove)
            {
                Point pStart, pEnd;
                switch (_MoveType)
                {
                    case LineMoveType.Begin:
                        pStart = new Point(e.GetPosition(this.UIContainer).X, e.GetPosition(this.UIContainer).Y);
                        pEnd = new Point(Canvas.GetLeft(ellipseEnd) + ellipseEnd.Width / 2,
                                         Canvas.GetTop(ellipseEnd) + ellipseEnd.Height / 2);

                        plShadow.Points = GetPloyline(pStart, pEnd);
                        break;
                    case LineMoveType.End:
                        pStart = new Point(Canvas.GetLeft(ellipseBegin) + ellipseBegin.Width / 2,
                                           Canvas.GetTop(ellipseBegin) + ellipseBegin.Height / 2);
                        pEnd = new Point(e.GetPosition(this.UIContainer).X, e.GetPosition(this.UIContainer).Y);

                        plShadow.Points = GetPloyline(pStart, pEnd);
                        break;
                    case LineMoveType.Line:
                        if (mousePosition != e.GetPosition(this.UIContainer))
                        {
                            plShadow.Points = GetPloyline(plShadow.Points, mousePosition, e.GetPosition(this.UIContainer));
                            mousePosition = e.GetPosition(this.UIContainer);
                        }
                        break;
                }

                if (CheckPoints(plShadow.Points) == true) pointHadActualMove = true;
                else pointHadActualMove = false;

                if (pointHadActualMove && this._MoveType != LineMoveType.Center && this._MoveType != LineMoveType.None)
                {
                    linkElement = true;
                    if (this.BeginElement != null)
                    {
                        ((IControlBase)this.BeginElement).BeginLinesRemove(this.lineControl);
                        ((IControlBase)this.BeginElement).SetUnFocus();
                    }
                    this.BeginElement = this.Container.GetElementByPoint(plShadow.Points[0]);
                    if (this.BeginElement != null && !IsLineBetweenActivity() && LineBeginFinish())
                    {
                        if (((IControlBase)this.BeginElement).BeginLinesAdd(this.lineControl) == false) this.BeginElement = null;
                        else ((IControlBase)this.BeginElement).SetSelected();
                    }

                    if (this.EndElement != null)
                    {
                        ((IControlBase)this.EndElement).EndLinesRemove(this.lineControl);
                        ((IControlBase)this.EndElement).SetUnFocus();
                    }
                    this.EndElement = this.Container.GetElementByPoint(plShadow.Points[1]);
                    if (this.EndElement != null && !IsLineBetweenActivity() && LineBeginFinish())
                    {
                        if (((IControlBase)this.EndElement).EndLinesAdd(this.lineControl) == false) this.EndElement = null;
                        else ((IControlBase)this.EndElement).SetSelected();
                    }
                }
                else
                {
                    linkElement = false;
                }
            }
        }

        private void Hotspot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (trackingPointMouseMove)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    element.Cursor = Cursors.Arrow;
                    element.ReleaseMouseCapture();
                }
                if (pointHadActualMove == true)
                {
                    if (linkElement == true && (this.BeginElement != null || this.BeginElement != null))
                    {
                        Point pBegin, pEnd;
                        if (this.BeginElement != null)
                        {
                            this.BeginElementHotspot = ((IControlBase)this.BeginElement).GetNearHotspot(plShadow.Points[0]);
                            pBegin = ((IControlBase)this.BeginElement).GetHotspot(this.BeginElementHotspot);
                            ((IControlBase)this.BeginElement).SetUnFocus();
                        }
                        else pBegin = plShadow.Points[0];
                        if (this.EndElement != null)
                        {
                            this.EndElementHotspot = ((IControlBase)this.EndElement).GetNearHotspot(plShadow.Points[1]);
                            pEnd = ((IControlBase)this.EndElement).GetHotspot(this.EndElementHotspot);
                            ((IControlBase)this.EndElement).SetUnFocus();
                        }
                        else pEnd = plShadow.Points[1];

                        plShadow.Points = this.GetPloyline(pBegin, pEnd);
                    }

                    ShowMe(plShadow.Points);
                    e.Handled = true;

                    pointHadActualMove = false;
                }
                else
                {
                    plShadow.Points = GetMe();
                }
                trackingPointMouseMove = false;
            }
        }

        /// <summary>
        /// 开始线不能直接结束
        /// </summary>
        /// <returns></returns>
        private bool LineBeginFinish()
        {
            if (this.BeginElement == null || this.EndElement == null) return true;
            if (((IControlBase)this.BeginElement).Type == ElementType.Begin && ((IControlBase)this.EndElement).Type == ElementType.Finish)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 检查连个Activity之间是否存在连线
        /// </summary>
        /// <returns>bool - true : 存在, false : 不存在</returns>
        private bool IsLineBetweenActivity()
        {
            bool isExists = false;

            if (this.BeginElement == null || this.EndElement == null) return false;

            var lines = from item in this.Container.Elements
                        where ((IControlBase)item).Type == ElementType.Line && ((IControlBase)item) != this.lineControl
                        select item;

            foreach (LineControl line in lines)
            {
                if (line.BeginElement == this.BeginElement && line.EndElement == this.EndElement)
                {
                    isExists = true;
                    break;
                }
                if (line.BeginElement == this.EndElement && line.EndElement == this.BeginElement)
                {
                    isExists = true;
                    break;
                }
            }

            return isExists;
        }

        private void Arrowhead_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Container.IsMultiSelected(this.lineControl)) return;

            pointHadActualMove = false;
            trackingPointMouseMove = false;

            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                _MoveType = LineMoveType.None;
                _MoveType = LineMoveType.Line;
                mousePosition = e.GetPosition(this.UIContainer);
                element.Cursor = Cursors.Hand;

                if (_MoveType != LineMoveType.None)
                {
                    trackingPointMouseMove = true;
                    element.CaptureMouse();
                }
            }
        }

        private void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.Cursor = Cursors.Hand;
            }
        }

        private void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove != true)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    element.Cursor = Cursors.Arrow;
                }
            }
        }
    }
}
