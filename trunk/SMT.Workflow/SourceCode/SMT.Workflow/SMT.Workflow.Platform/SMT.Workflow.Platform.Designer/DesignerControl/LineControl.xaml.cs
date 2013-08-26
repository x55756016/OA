/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：LineControl.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 11:03:27   
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
    public partial class LineControl : UserControl, IControlBase
    {
        public LineControl()
        {
            InitializeComponent();
            this.ShapeLine.UIContainer = this.cnContainer;
            this.ShapeLine.lineControl = this;    
        }

        public LineControl(double canvasLeft, double canvasTop, double canvasWidth, double canvasHeight)
            :this()
        {
            this._CanvasLeft = canvasLeft;
            this._CanvasTop = canvasTop;
            this._CanvasWidth = canvasWidth;
            this._CanvasHeight = canvasHeight;

            this.ShapeLine.SetRange(canvasLeft, canvasTop, canvasWidth, canvasHeight);            
        }

        private double _CanvasLeft = 0;
        private double _CanvasWidth = 0;
        private double _CanvasTop = 0;
        private double _CanvasHeight = 0;


        public ElementType Type
        {
            get { return ElementType.Line; }
        }

        public ElementState State
        {
            get { return ShapeLine.State; }
        }

        public UIElement BeginElement
        {
            get { return ShapeLine.BeginElement; }
            set { ShapeLine.BeginElement = value; }
        }
        public HotspotType BeginElementHotspot
        {
            get { return ShapeLine.BeginElementHotspot; }
        }

        public UIElement EndElement
        {
            get { return ShapeLine.EndElement; }
            set { ShapeLine.EndElement = value; }
        }
        public HotspotType EndElementHotspot
        {
            get { return ShapeLine.EndElementHotspot; }
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
            get { return 0; }
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

        public IContainer Container
        {
            get
            {
                return ShapeLine.Container;
            }
            set
            {
                ShapeLine.Container = value;
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
            ShapeLine.SetFocus();
            ShapeLine.Opacity = 1.0;
        }        

        public void SetUnFocus()
        {
            ShapeLine.SetUnFocus();
            ShapeLine.Opacity = 0.7;
        }

        public void SetSelected()
        {
            ShapeLine.SetSelected();
            ShapeLine.Opacity = 1.0;
        }

        public void InitXY()
        {
            //
        }

        public void SetXY(Point mousePoint)
        {
            ShapeLine.SetPreviousPoint(mousePoint);
        }

        public void ShowShadow(Point point, PointType pointType, object sender)
        {
            if ((this.BeginElement == null && this.EndElement == null)
                || (this.BeginElement != null && this.EndElement != null && this.State == ElementState.Focus))
            {
                ShapeLine.ShowShadow(point, pointType);
            }
        }

        public void ShowShadow(Point pBegin, Point pEnd)
        {
            ShapeLine.ShowShadow(pBegin, pEnd);
        }

        public void ShowShadow(PointCollection points)
        {
            ShapeLine.ShowShadow(points);
        }

        public Point GetShadow()
        {
            return new Point(0, 0);
        }

        public void ShowMe(Point mousePoint, object sender)
        {
            ShapeLine.ShowMe();
        }

        public void ShowMe(Point pBegin, Point pEnd)
        {
            ShapeLine.ShowMe(pBegin, pEnd);
        }

        public void ShowMe(PointCollection points)
        {
            ShapeLine.ShowMe(points);
        }

        public Point GetMe()
        {
            return new Point(0, 0);
        }

        public bool CheckPoint(Point mousePosition)
        {
            return ShapeLine.CheckPoints(ShapeLine.plShadow.Points);
        }

        public bool IsInside(Point point, double x, double y)
        {
            return ShapeLine.IsInside(point, x, y);
        }

        public Point Location
        {
            get
            {
                return ShapeLine.plShadow.Points[0];
            }
        }

        public Point GetHotspot(HotspotType hotspotType)
        {
            Point point = new Point(0, 0);
            switch (hotspotType)
            {
                case HotspotType.Left:
                    point.X = ShapeLine.plShadow.Points[0].X;
                    point.Y = ShapeLine.plShadow.Points[0].Y;
                    break;
                case HotspotType.Top:
                    point.X = ShapeLine.plShadow.Points[0].X;
                    point.Y = ShapeLine.plShadow.Points[0].Y;
                    break;
                case HotspotType.Right:
                    point.X = ShapeLine.plShadow.Points[1].X;
                    point.Y = ShapeLine.plShadow.Points[1].Y;
                    break;
                case HotspotType.Bottom:
                    point.X = ShapeLine.plShadow.Points[1].X;
                    point.Y = ShapeLine.plShadow.Points[1].Y;
                    break;
            }
            return point;
        }

        public Point GetShadowHotspot(HotspotType hotspotType)
        {
            return GetHotspot(hotspotType);
        }

        public HotspotType GetNearHotspot(Point point)
        {
            HotspotType hotspotType = HotspotType.Left;
            return hotspotType;
        }        

        public bool PointIsInside(Point point)
        {
            bool isInside = false;
          
            return isInside;
        }

        public Point GetBegin()
        {
            return ShapeLine.plShadow.Points[0];
        }

        public Point GetEnd()
        {
            return ShapeLine.plShadow.Points[1];
        }

        public PointCollection GetPloyline(Point pBegin, Point pEnd)
        {
            return this.ShapeLine.GetPloyline(pBegin, pEnd);
        }

        public void MoveShadow(double x, double y)
        {
            if ((this.BeginElement == null && this.EndElement == null)
                || (this.BeginElement != null && this.EndElement != null && this.State == ElementState.Focus))
            {
                Point pOld = this.GetBegin();
                Point pNew=this.GetBegin();
                pNew.X += x; pNew.Y += y;
                PointCollection points = ShapeLine.GetPloyline(ShapeLine.plShadow.Points, pOld, pNew);
                if (ShapeLine.CheckPoints(points))
                {
                    ShapeLine.plShadow.Points = points;
                }
            }
        }

        public void Move()
        {
            if ((this.BeginElement == null && this.EndElement == null)
                || (this.BeginElement != null && this.EndElement != null && this.State == ElementState.Focus))
            {
                ShapeLine.ShowMe();
            }
        }

        public string Title
        {
            get
            {
                return ShapeLine.GetTitle();
            }
            set
            {
                ShapeLine.SetTitle(value);
            }
        }


        public int ZIndex
        {
            get { return (int)this.GetValue(Canvas.ZIndexProperty); }
            set { this.SetValue(Canvas.ZIndexProperty, value); }
        }

     
      

        private void SetMeSelected()
        {
            Container.SetElementSelected((UIElement)this);
        }

        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.Container.IsMultiSelected(this) && !this.Container.CtrlKeyIsPress) return;

            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                this.SetMeSelected();

                e.Handled = true;
            }
        }

        private void Shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           
        }


        public string ToXmlString()
        {
            string Left = string.Empty;
            string Top = string.Empty;
            for (int i = 0; i < ShapeLine.plShadow.Points.Count; i++)
            {
                Left += ShapeLine.plShadow.Points[i].X.ToString() + ",";
                Top += ShapeLine.plShadow.Points[i].Y.ToString() + ",";
            }
            Left = Left.Trim(',');
            Top = Top.Trim(',');

            System.Text.StringBuilder xml = new System.Text.StringBuilder();
            xml.Append(@"        <SMTElement ");
            xml.Append(@" UniqueID=""" + this.UniqueID + @"""");
            xml.Append(@" Title=""" + this.Title + @"""");
            xml.Append(@" ElementType=""" + this.Type + @"""");
            xml.Append(@" Left=""" + Left + @"""");
            xml.Append(@" Top=""" + Top + @"""");
            xml.Append(@" ZIndex=""" + this.ZIndex + @""">");
            xml.Append(Environment.NewLine);
            xml.Append(@"        </SMTElement>");

            return xml.ToString();
        }
       
    }
}
