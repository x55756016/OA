/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：FinishControl.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/15 14:57:40   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerControl 
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

namespace SMT.Workflow.Platform.Designer.DesignerControl
{
    public partial class FinishControl : UserControl, IControlBase
    {
        public FinishControl()
        {
            InitializeComponent();
        }

        public FinishControl(double canvasLeft, double canvasTop, double canvasWidth, double canvasHeight)
            :this()
        {
            this._CanvasLeft = canvasLeft;
            this._CanvasTop = canvasTop;
            this._CanvasWidth = canvasWidth;
            this._CanvasHeight = canvasHeight;
        }

        private double _CanvasLeft = 0;
        private double _CanvasWidth = 0;
        private double _CanvasTop = 0;
        private double _CanvasHeight = 0;


        private double _ShadowX = 4;
        private double _ShadowY = 4;

        public ElementType Type
        {
            get { return ElementType.Finish; }
        }

        public ElementState State
        {
            get { return ShapeFinish.State; }
        }

        public int MaxBeginLines
        {
            get { return 0; }
        }
        private List<UIElement> _beginLines;
        public List<UIElement> BeginLines
        {
            get
            {
                if (this._beginLines == null)
                {
                    this._beginLines = new List<UIElement>();
                }
                return this._beginLines;
            }
        }
        public bool BeginLinesAdd(UIElement element)
        {
            if (BeginLines.Count < this.MaxBeginLines && !BeginLines.Contains(element) && !EndLines.Contains(element))
            {
                BeginLines.Add(element);
                return true;
            }
            return false;
        }
        public void BeginLinesRemove(UIElement element)
        {
            if (BeginLines.Contains(element))
            {
                BeginLines.Remove(element);
            }
        }

        public int MaxEndLines
        {
            get { return 80; }
        }
        private List<UIElement> _endLines;
        public List<UIElement> EndLines
        {
            get
            {
                if (this._endLines == null)
                {
                    this._endLines = new List<UIElement>();
                }
                return this._endLines;
            }
        }
        public bool EndLinesAdd(UIElement element)
        {
            if (EndLines.Count < this.MaxEndLines && !BeginLines.Contains(element) && !EndLines.Contains(element))
            {
                EndLines.Add(element);
                return true;
            }
            return false;
        }
        public void EndLinesRemove(UIElement element)
        {
            if (EndLines.Contains(element))
            {
                EndLines.Remove(element);
            }
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

        private string _uniqueID;
        public string UniqueID
        {
            get
            {
                if (string.IsNullOrEmpty(this._uniqueID))
                {
                    this._uniqueID = Guid.NewGuid().ToString().Replace("-", "");
                }
                return this._uniqueID;
            }
            set { this._uniqueID = value; }
        }

        public void SetFocus()
        {
            ShapeFinish.SetFocus();
        }

        public void SetUnFocus()
        {
            ShapeFinish.SetUnFocus();
        }

        public void SetSelected()
        {
            ShapeFinish.SetSelected();
        }

        public void InitXY()
        {
            this.x = 0;
            this.y = 0;
        }

        public void SetXY(Point mousePoint)
        {
            x = mousePoint.X - Canvas.GetLeft(ShapeFinish);
            y = mousePoint.Y - Canvas.GetTop(ShapeFinish);
        }

        public void ShowShadow(Point point, PointType wf11PointType, object sender)
        {
            if (wf11PointType == PointType.Precision) //精确坐标点
            {
                Canvas.SetLeft(Shadow, point.X);
                Canvas.SetTop(Shadow, point.Y);
            }
            else  //偏移坐标点
            {
                Canvas.SetLeft(Shadow, point.X - this.x + this._ShadowX);
                Canvas.SetTop(Shadow, point.Y - this.y + this._ShadowX);
            }
            this.ShowLineShadow(sender);
        }

        public Point GetShadow()
        {
            return new Point(Canvas.GetLeft(Shadow) - this._ShadowX, Canvas.GetTop(Shadow) - this._ShadowY);
        }

        public void ShowMe(Point mousePoint, object sender)
        {
            Canvas.SetLeft(ShapeFinish, mousePoint.X - x);
            Canvas.SetTop(ShapeFinish, mousePoint.Y - y);
            this.ShowLine(sender);
        }

        public Point GetMe()
        {
            return new Point(Canvas.GetLeft(ShapeFinish) + this._ShadowX, Canvas.GetTop(ShapeFinish) + this._ShadowY);
        }

        public bool CheckPoint(Point mousePosition)
        {
            if (mousePosition.X - this.x < _CanvasLeft || mousePosition.X - this.x > _CanvasWidth) return false;
            if (mousePosition.Y - this.y < _CanvasTop || mousePosition.Y - this.y > _CanvasHeight) return false;
            return true;
        }

        public bool IsInside(Point point, double x, double y)
        {
            Point pMe = new Point();
            pMe.X = Canvas.GetLeft(ShapeFinish);
            pMe.Y = Canvas.GetTop(ShapeFinish);
            Point pLeftTop = new Point(pMe.X, pMe.Y);
            Point pRightTop = new Point(pMe.X + ShapeFinish.Width, pMe.Y);
            Point pLeftBottom = new Point(pMe.X, pMe.Y + ShapeFinish.Height);
            Point pRightBottom = new Point(pMe.X + ShapeFinish.Width, pMe.Y + ShapeFinish.Height);

            if ((pRightBottom.X >= point.X && pRightBottom.Y >= point.Y)      //检查选中区域的左上角
                && (pLeftBottom.X <= point.X + x && pLeftBottom.Y >= point.Y) //检查选中区域的右上角
                && (pRightTop.X >= point.X && pRightTop.Y <= point.Y + y)     //检查选中区域的左下角
                && (pLeftTop.X <= point.X + x && pLeftTop.Y <= point.Y + y))  //检查选中区域的右下角
            {
                return true;
            }
            return false;
        }

        public Point Location
        {
            get
            {
                return new Point(Canvas.GetLeft(ShapeFinish), Canvas.GetTop(ShapeFinish));
            }
        }

        public Point GetHotspot(HotspotType hotspotType)
        {
            Point point = new Point(0, 0);
            switch (hotspotType)
            {
                case HotspotType.Left:
                    point.X = (double)Canvas.GetLeft(ShapeFinish) + ShapeFinish.HotspotLeft.Width / 2;
                    point.Y = (double)Canvas.GetTop(ShapeFinish) + ShapeFinish.Height / 2;
                    break;
                case HotspotType.Top:
                    point.X = (double)Canvas.GetLeft(ShapeFinish) + ShapeFinish.Width / 2;
                    point.Y = (double)Canvas.GetTop(ShapeFinish) + ShapeFinish.HotspotLeft.Height / 2;
                    break;
                case HotspotType.Right:
                    point.X = (double)Canvas.GetLeft(ShapeFinish) + ShapeFinish.Width - ShapeFinish.HotspotLeft.Width / 2;
                    point.Y = (double)Canvas.GetTop(ShapeFinish) + ShapeFinish.Height / 2;
                    break;
                case HotspotType.Bottom:
                    point.X = (double)Canvas.GetLeft(ShapeFinish) + ShapeFinish.Width / 2;
                    point.Y = (double)Canvas.GetTop(ShapeFinish) + ShapeFinish.Height - ShapeFinish.HotspotLeft.Height / 2;
                    break;
            }
            return point;
        }

        public Point GetShadowHotspot(HotspotType hotspotType)
        {
            Point point = new Point(0, 0);
            switch (hotspotType)
            {
                case HotspotType.Left:
                    point.X = (double)Canvas.GetLeft(Shadow);
                    point.Y = (double)Canvas.GetTop(Shadow) + Shadow.Height / 2;
                    break;
                case HotspotType.Top:
                    point.X = (double)Canvas.GetLeft(Shadow) + Shadow.Width / 2;
                    point.Y = (double)Canvas.GetTop(Shadow);
                    break;
                case HotspotType.Right:
                    point.X = (double)Canvas.GetLeft(Shadow) + Shadow.Width;
                    point.Y = (double)Canvas.GetTop(Shadow) + Shadow.Height / 2;
                    break;
                case HotspotType.Bottom:
                    point.X = (double)Canvas.GetLeft(Shadow) + Shadow.Width / 2;
                    point.Y = (double)Canvas.GetTop(Shadow) + Shadow.Height;
                    break;
            }
            return point;
        }

        public HotspotType GetNearHotspot(Point point)
        {
            PointCollection points = new PointCollection();
            points.Add(this.GetHotspot(HotspotType.Left));
            points.Add(this.GetHotspot(HotspotType.Top));
            points.Add(this.GetHotspot(HotspotType.Right));
            points.Add(this.GetHotspot(HotspotType.Bottom));

            HotspotType hotspotType = HotspotType.Left;
            int idx = 0;
            double minValue = Math.Abs(point.X - points[0].X) + Math.Abs(point.Y - points[0].Y);
            for (int i = 1; i < points.Count; i++)
            {
                if (minValue > (Math.Abs(point.X - points[i].X) + Math.Abs(point.Y - points[i].Y)))
                {
                    idx = i;
                    minValue = Math.Abs(point.X - points[i].X) + Math.Abs(point.Y - points[i].Y);
                }
            }
            switch (idx)
            {
                case 0:
                    hotspotType = HotspotType.Left;
                    break;
                case 1:
                    hotspotType = HotspotType.Top;
                    break;
                case 2:
                    hotspotType = HotspotType.Right;
                    break;
                case 3:
                    hotspotType = HotspotType.Bottom;
                    break;
            }
            return hotspotType;
        }

        public bool PointIsInside(Point point)
        {
            bool isInside = false;

            double thisWidth = ShapeFinish.Width;
            double thisHeight = ShapeFinish.Height;

            if (Location.X < point.X && point.X < Location.X + thisWidth
                && Location.Y < point.Y && point.Y < Location.Y + thisHeight)
            {
                isInside = true;
            }
            return isInside;
        }

        public void MoveShadow(double x, double y)
        {
            Point point = new Point();
            point.X = Canvas.GetLeft(Shadow) + x;
            point.Y = Canvas.GetTop(Shadow) + y;
            Canvas.SetLeft(Shadow, point.X);
            Canvas.SetTop(Shadow, point.Y);
            this.ShowLineShadow(null);
        }

        public void Move()
        {
            Point point = this.GetShadow();
            Canvas.SetLeft(ShapeFinish, point.X);
            Canvas.SetTop(ShapeFinish, point.Y);
            this.ShowLine(null);
        }

        public string Title
        {
            get
            {
                return ShapeFinish.GetTitle();
            }
            set
            {
                ShapeFinish.SetTitle(value);
            }
        }

        public int ZIndex
        {
            get { return (int)this.GetValue(Canvas.ZIndexProperty); }
            set { this.SetValue(Canvas.ZIndexProperty, value); }
        }

        //public string ToXmlString()
        //{
        //    System.Text.StringBuilder xml = new System.Text.StringBuilder();
        //    xml.Append(@"        <Element ");
        //    xml.Append(@" UniqueID=""" + this.UniqueID + @"""");
        //    xml.Append(@" Title=""" + this.Title + @"""");
        //    xml.Append(@" ElementType=""" + this.Type + @"""");
        //    xml.Append(@" Left=""" + this.Location.X + @"""");
        //    xml.Append(@" Top=""" + this.Location.Y + @"""");
        //    xml.Append(@" ZIndex=""" + this.ZIndex + @""">");
        //    xml.Append(Environment.NewLine);
        //    xml.Append(@"        </Element>");

        //    return xml.ToString();
        //}

        //public UIElement Clone()
        //{
        //    UIElement element = new Finish(this._CanvasLeft, this._CanvasTop, this._CanvasWidth, this._CanvasHeight);
        //    ((IElement)element).Container = this.Container;
        //    ((IElement)element).InitXY();
        //    Point point = this.GetMe();
        //    point.X += 10; point.Y += 10;
        //    ((IElement)element).ShowShadow(point, PointType.Precision, element);
        //    point = this.GetShadow();
        //    point.X += 10; point.Y += 10;
        //    ((IElement)element).ShowMe(point, element);

        //    return element;
        //}

        private void SetMeSelected()
        {
            Container.SetElementSelected((UIElement)this);
        }

        private void ShowLineShadow(object sender)
        {
            Point pBegin, pEnd;
            if (sender == this)  //当前组件处理
            {
                if (this.BeginLines.Count != 0)
                {
                    for (int i = 0; i < this.BeginLines.Count; i++)
                    {
                        pBegin = this.GetShadowHotspot(((LineControl)this.BeginLines[i]).BeginElementHotspot);
                        pEnd = ((LineControl)this.BeginLines[i]).GetEnd();
                        ((LineControl)this.BeginLines[i]).ShowShadow(pBegin, pEnd);
                    }
                }
                if (this.EndLines.Count != 0)
                {
                    for (int i = 0; i < this.EndLines.Count; i++)
                    {
                        pBegin = ((LineControl)this.EndLines[i]).GetBegin();
                        pEnd = this.GetShadowHotspot(((LineControl)this.EndLines[i]).EndElementHotspot);
                        ((LineControl)this.EndLines[i]).ShowShadow(pBegin, pEnd);
                    }
                }
            }
            else //传递给容器处理
            {
                if (this.BeginLines.Count != 0)
                {
                    for (int i = 0; i < this.BeginLines.Count; i++)
                    {
                        if (((LineControl)this.BeginLines[i]).EndElement == null)
                        {
                            pBegin = this.GetShadowHotspot(((LineControl)this.BeginLines[i]).BeginElementHotspot);
                            pEnd = ((LineControl)this.BeginLines[i]).GetEnd();
                            ((LineControl)this.BeginLines[i]).ShowShadow(pBegin, pEnd);
                        }
                        else
                        {
                            if (((IControlBase)this.BeginLines[i]).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.BeginLines[i]).BeginElement).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.BeginLines[i]).EndElement).State != ElementState.Focus)
                            {
                                this.Container.NeedMoveUnFocusLineAdd(this.BeginLines[i]);
                            }
                        }
                    }
                }
                if (this.EndLines.Count != 0)
                {
                    for (int i = 0; i < this.EndLines.Count; i++)
                    {
                        if (((LineControl)this.EndLines[i]).BeginElement == null)
                        {
                            pBegin = ((LineControl)this.EndLines[i]).GetBegin();
                            pEnd = this.GetShadowHotspot(((LineControl)this.EndLines[i]).EndElementHotspot);
                            ((LineControl)this.EndLines[i]).ShowShadow(pBegin, pEnd);
                        }
                        else
                        {
                            if (((IControlBase)this.EndLines[i]).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.EndLines[i]).BeginElement).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.EndLines[i]).EndElement).State != ElementState.Focus)
                            {
                                this.Container.NeedMoveUnFocusLineAdd(this.EndLines[i]);
                            }
                        }
                    }
                }
            }
        }

        private void ShowLine(object sender)
        {
            Point pBegin, pEnd;
            if (sender == this)  //当前组件处理
            {
                if (this.BeginLines.Count != 0)
                {
                    for (int i = 0; i < this.BeginLines.Count; i++)
                    {
                        pBegin = this.GetHotspot(((LineControl)this.BeginLines[i]).BeginElementHotspot);
                        pEnd = ((LineControl)this.BeginLines[i]).GetEnd();
                        ((LineControl)this.BeginLines[i]).ShowMe(pBegin, pEnd);
                    }
                }
                if (this.EndLines.Count != 0)
                {
                    for (int i = 0; i < this.EndLines.Count; i++)
                    {
                        pBegin = ((LineControl)this.EndLines[i]).GetBegin();
                        pEnd = this.GetHotspot(((LineControl)this.EndLines[i]).EndElementHotspot);
                        ((LineControl)this.EndLines[i]).ShowMe(pBegin, pEnd);
                    }
                }
            }
            else //传递给容器处理
            {
                if (this.BeginLines.Count != 0)
                {
                    for (int i = 0; i < this.BeginLines.Count; i++)
                    {
                        if (((LineControl)this.BeginLines[i]).EndElement == null)
                        {
                            pBegin = this.GetHotspot(((LineControl)this.BeginLines[i]).BeginElementHotspot);
                            pEnd = ((LineControl)this.BeginLines[i]).GetEnd();
                            ((LineControl)this.BeginLines[i]).ShowMe(pBegin, pEnd);
                        }
                        else
                        {
                            if (((IControlBase)this.BeginLines[i]).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.BeginLines[i]).BeginElement).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.BeginLines[i]).EndElement).State != ElementState.Focus)
                            {
                                this.Container.NeedMoveUnFocusLineAdd(this.BeginLines[i]);
                            }
                        }
                    }
                }
                if (this.EndLines.Count != 0)
                {
                    for (int i = 0; i < this.EndLines.Count; i++)
                    {
                        if (((LineControl)this.EndLines[i]).BeginElement == null)
                        {
                            pBegin = ((LineControl)this.EndLines[i]).GetBegin();
                            pEnd = this.GetHotspot(((LineControl)this.EndLines[i]).EndElementHotspot);
                            ((LineControl)this.EndLines[i]).ShowMe(pBegin, pEnd);
                        }
                        else
                        {
                            if (((IControlBase)this.EndLines[i]).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.EndLines[i]).BeginElement).State != ElementState.Focus
                                || ((IControlBase)((LineControl)this.EndLines[i]).EndElement).State != ElementState.Focus)
                            {
                                this.Container.NeedMoveUnFocusLineAdd(this.EndLines[i]);
                            }
                        }
                    }
                }
            }
        }

        private void Shape_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
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

            if (this.Container.IsMultiSelected(this) && !this.Container.CtrlKeyIsPress) return;

            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                this.SetMeSelected();

                mousePosition = e.GetPosition(cnContainer);
                originalPosition = e.GetPosition(cnContainer);

                x = mousePosition.X - Canvas.GetLeft(ShapeFinish);
                y = mousePosition.Y - Canvas.GetTop(ShapeFinish);

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
                if (mousePosition != e.GetPosition(cnContainer))
                {
                    mousePosition = e.GetPosition(cnContainer);
                    this.ShowShadow(mousePosition, PointType.Excursion, this);

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
                if (element != null)
                {
                    element.Cursor = Cursors.Arrow;
                    element.ReleaseMouseCapture();
                }
                if (pointHadActualMove == true)
                {
                    ShowMe(mousePosition, this);
                    e.Handled = true;
                    pointHadActualMove = false;
                }
                else
                {
                    if (mousePosition != originalPosition)
                    {
                        Point point = GetMe();
                        this.ShowShadow(point, PointType.Precision, this);
                    }
                }
                trackingPointMouseMove = false;
            }
        }


        public string ToXmlString()
        {
            System.Text.StringBuilder xml = new System.Text.StringBuilder();
            xml.Append(@"        <SMTElement ");
            xml.Append(@" UniqueID=""" + this.UniqueID + @"""");
            xml.Append(@" Title=""" + this.Title + @"""");
            xml.Append(@" ElementType=""" + this.Type + @"""");
            xml.Append(@" Left=""" + this.Location.X + @"""");
            xml.Append(@" Top=""" + this.Location.Y + @"""");
            xml.Append(@" ZIndex=""" + this.ZIndex + @""">");
            xml.Append(Environment.NewLine);
            xml.Append(@"        </SMTElement>");

            return xml.ToString();
        }
    }
}
