/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Container.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/15 10:21:56   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerView 
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
using SMT.Workflow.Platform.Designer.Utils;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer.DesignerView
{
    public partial class Container : UserControl, IContainer
    {
        public Container()
        {
            InitializeComponent();
            //this.SetGridLines();               //画线
            this.InitialSimpleElement();   //初始化简单元素的设置   

            //注册缩放控件的事件
            _zoomcontrol.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_zoomcontrol_PropertyChanged);
            //this.SetGridLines();            //画线       
            //注册服务
            RegisterServices();  
        }

        #region 变量


        //初始放大的值
        private double _initialZoom = 1.0;

        /// <summary>
        /// 网格线容器
        /// </summary>
        //private Canvas _gridLinesContainer;
        //public Canvas GridLinesContainer
        //{
        //    get
        //    {
        //        if (_gridLinesContainer == null)
        //        {

        //            Canvas cnsGridLinesContainer = new Canvas();               
        //            cnsGridLinesContainer.Name = "cnsGridLinesContainer";
        //            TargetCanvas.Children.Add(cnsGridLinesContainer);
        //            _gridLinesContainer = cnsGridLinesContainer;

        //        }
        //        return _gridLinesContainer;
        //    }
        //}

        private List<UIElement> _Elements;
        public List<UIElement> Elements
        {
            get
            {
                if (this._Elements == null)
                {
                    this._Elements = new List<UIElement>();
                }
                return this._Elements;
            }
        }

        private List<UIElement> _SelectedElements;
        public List<UIElement> SelectedElements
        {
            get
            {
                if (this._SelectedElements == null)
                    _SelectedElements = new List<UIElement>();
                return _SelectedElements;

            }
        }

        private List<UIElement> _NeedMoveUnFocusLines;
        public List<UIElement> NeedMoveUnFocusLines
        {
            get
            {
                if (this._NeedMoveUnFocusLines == null)
                    this._NeedMoveUnFocusLines = new List<UIElement>();
                return this._NeedMoveUnFocusLines;
            }
        }

        private Rectangle _selectArea;
     /// <summary>
        /// 拷贝的元素集
     /// </summary>
        private List<UIElement> _copyedElements;
        #endregion

        #region 画布中的划线

        /// <summary>
        /// 设置网格线
        /// </summary>
        public void SetGridLines()
        {
            //GridLinesContainer.Children.Clear();
            //double thickness = 0.2d;
            //DoubleCollection strokeDashArray = new DoubleCollection() { 20.0, 5.0 };
            //this.SetGridLines(0, 0, 40, Colors.Black, thickness, strokeDashArray);

            //DoubleCollection strokeDashArray2 = new DoubleCollection() { 10.0, 5.0 };
            //this.SetGridLines(20, 20, 40, Colors.Gray, thickness, strokeDashArray2);
        }
        private void SetGridLines(double left, double top, double stepLength, Color color, double thickness, DoubleCollection strokeDashArray)
        {
            //SolidColorBrush brush = new SolidColorBrush();
            //brush.Color = color;

            //double width = cnsDesignerContainer.Width;
            //double height = cnsDesignerContainer.Height;

            //double x, y;
            //x = left;
            //y = 0;
            //while (x < width + left)
            //{
            //    Line line = new Line();
            //    line.X1 = x;
            //    line.Y1 = y;
            //    line.X2 = x;
            //    line.Y2 = y + height;

            //    line.Stroke = brush;
            //    line.StrokeThickness = thickness;
            //    line.StrokeDashArray.Clear();
            //    for (int i = 0; i < strokeDashArray.Count; i++) line.StrokeDashArray.Add(strokeDashArray[i]);
            //    line.Stretch = Stretch.None;
            //    GridLinesContainer.Children.Add(line);
            //    x += stepLength;
            //}

            //x = 0;
            //y = top;
            //while (y < height + top)
            //{
            //    Line line = new Line();
            //    line.X1 = x;
            //    line.Y1 = y;
            //    line.X2 = x + width;
            //    line.Y2 = y;

            //    line.Stroke = brush;
            //    line.Stretch = Stretch.None;
            //    line.StrokeThickness = thickness;
            //    line.StrokeDashArray.Clear();
            //    for (int i = 0; i < strokeDashArray.Count; i++) line.StrokeDashArray.Add(strokeDashArray[i]);
            //    GridLinesContainer.Children.Add(line);
            //    y += stepLength;
            //}
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 初始化元素位置
        /// </summary>
        private void InitialSimpleElement()
        {
            SMTStart.InitializeMe(this, new Point(25, 20), this.cnsDesignerContainer, 0, 0);
            SMTActivity.InitializeMe(this, new Point(25, 40), this.cnsDesignerContainer, 0, 0);
            SMTLine.InitializeMe(this, new Point(25, 60), this.cnsDesignerContainer, 0, 0);
            SMTFinish.InitializeMe(this, new Point(25, 110), this.cnsDesignerContainer, 0, 0);
        }
        #endregion

        #region 私有方法 

        /// <summary>
        /// 记录ZIndex，SelectedElementAdd子方法
        /// </summary>
        private void ReorderZindex()
        {
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (((IControlBase)this.Elements[i]).Type == ElementType.Line)
                {
                    if (!this.SelectedElements.Contains(this.Elements[i]))
                        ((IControlBase)this.Elements[i]).ZIndex = i; //this.WF11Elements.Count * 2 + i;
                    else ((IControlBase)this.Elements[i]).ZIndex = this.Elements.Count + i;
                }
                else
                {
                    if (!this.SelectedElements.Contains(this.Elements[i]))
                    {
                        ((IControlBase)this.Elements[i]).ZIndex = i;
                    }
                    else
                    {
                        ((IControlBase)this.Elements[i]).ZIndex = this.Elements.Count + i;
                    }
                }
            }
        }
        /// <summary>
        /// 清除容器上的元素
        /// </summary>
        private void CleareContainer()
        {
            //this.CopyedWF11Elements.Clear();  
            this.NeedMoveUnFocusLines.Clear();
            this.SelectedElementRemoveAll();
            this.Elements.Clear();
            cnsDesignerContainer.Children.Clear();
            //this._gridLinesContainer = null;
            this.SetGridLines();
        }

     
        private void SetElementFocus(UIElement element)
        {
            this.SelectedElementRemoveAll();
            this.SelectedElementAdd(element);
            this.ShowElementProperty();
            this.Focus();
        }


        private UIElement ElementFactory(ElementType type)
        {
            //SetZoom(1.0);//置放大或者缩小为原点           
            UIElement element = null;
            double left = 0, right = 0;
            switch (type)
            {
                case ElementType.Begin:
                    {
                        element = new BeginControl(left, right, cnsDesignerContainer.Width, cnsDesignerContainer.Height);
                        break;
                    }
                case ElementType.Activity:
                    {
                        element = new SMT.Workflow.Platform.Designer.DesignerControl.ActivityControl(left, right, cnsDesignerContainer.Width, cnsDesignerContainer.Height);
                        ((DesignerControl.ActivityControl)element).Title = "新建活动" + this.GetActivieyIdentityNo();
                        break;
                    }            
                case ElementType.Line:
                    {
                        element = new LineControl(left, right, cnsDesignerContainer.Width, cnsDesignerContainer.Height);
                        ((DesignerControl.LineControl)element).Title = "新建连线" + this.GetLineIdentityNo();
                        break;
                    }
                case ElementType.Finish:
                    {
                        element = new FinishControl(left, right, cnsDesignerContainer.Width, cnsDesignerContainer.Height);
                        break;
                    }
            }
            return element;
        }

        /// <summary>
        /// 移动没有焦点的连线
        /// </summary>
        private void MoveUnFocusLinesShadow()
        {
            Point pBegin, pEnd;
            for (int i = 0; i < this.NeedMoveUnFocusLines.Count; i++)
            {
                pBegin = ((IControlBase)((LineControl)this.NeedMoveUnFocusLines[i]).BeginElement).GetShadowHotspot(((LineControl)this.NeedMoveUnFocusLines[i]).BeginElementHotspot);
                pEnd = ((IControlBase)((LineControl)this.NeedMoveUnFocusLines[i]).EndElement).GetShadowHotspot(((LineControl)this.NeedMoveUnFocusLines[i]).EndElementHotspot);
                ((LineControl)this.NeedMoveUnFocusLines[i]).ShowShadow(pBegin, pEnd);
            }
        }

        /// <summary>
        /// 移动未获得焦点的连线
        /// </summary>
        private void MoveUnFocusLines()
        {
            Point pBegin, pEnd;
            for (int i = 0; i < this.NeedMoveUnFocusLines.Count; i++)
            {
                pBegin = ((IControlBase)((LineControl)this.NeedMoveUnFocusLines[i]).BeginElement).GetHotspot(((LineControl)this.NeedMoveUnFocusLines[i]).BeginElementHotspot);
                pEnd = ((IControlBase)((LineControl)this.NeedMoveUnFocusLines[i]).EndElement).GetHotspot(((LineControl)this.NeedMoveUnFocusLines[i]).EndElementHotspot);
                ((LineControl)this.NeedMoveUnFocusLines[i]).ShowMe(pBegin, pEnd);
            }
        }

  
        #region 元素创建、删除、添加、选中处理
        /// <summary>
        /// 创建元素
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="strLeft"></param>
        /// <param name="strTop"></param>
        /// <param name="zIndex"></param>
        /// <returns></returns>
        private UIElement CreateElement(ElementType type, string name, string strLeft, string strTop, int zIndex)
        {           
            UIElement element = this.ElementFactory(type);
            Point location = new Point();
            double xy = 0;
            if (element != null)
            {
                ((LineControl)element).Title = name;
                ((IControlBase)element).ZIndex = zIndex;
                ((IControlBase)element).InitXY();
                if (type != ElementType.Line)
                {
                    double.TryParse(strLeft, out xy);
                    location.X = xy;
                    double.TryParse(strTop, out xy);
                    location.Y = xy;
                    ((IControlBase)element).ShowShadow(location, PointType.Excursion, ((IControlBase)element));
                    ((IControlBase)element).ShowMe(location, ((IControlBase)element));
                }
                else
                {
                    string[] xx = strLeft.Split(',');
                    string[] yy = strTop.Split(',');
                    PointCollection points = new PointCollection();
                    for (int i = 0; i < xx.Length; i++)
                    {
                        double.TryParse(xx[i], out xy);
                        location.X = xy;
                        double.TryParse(yy[i], out xy);
                        location.Y = xy;
                        points.Add(location);
                    }
                    ((LineControl)element).ShowShadow(points);
                    ((LineControl)element).ShowMe(points);
                }

                this.AddElement(element);
                this.SetElementFocus(element);
            }
            return element;
        }
      /// <summary>
        ///  创建元素
      /// </summary>
        /// <param name="UniqueID">已元素的UniqueID</param>
        /// <param name="name">已元素的名称（title）</param>
      /// <param name="Type"></param>
      /// <param name="strLeft"></param>
      /// <param name="strTop"></param>
      /// <param name="zIndex"></param>
      /// <returns></returns>
        private UIElement CreateElement(string UniqueID,string name,ElementType Type, string strLeft, string strTop, int zIndex)
        {           
            UIElement SMTElement = this.ElementFactory(Type);
            Point location = new Point();
            double xy = 0;
            if (SMTElement != null)
            {
                if (!string.IsNullOrEmpty(UniqueID))
                {//加载已有活动使用(龙康才)
                    ((IControlBase)SMTElement).UniqueID = UniqueID;
                    ((IControlBase)SMTElement).Title = name;
                }
                ((IControlBase)SMTElement).ZIndex = zIndex;
                ((IControlBase)SMTElement).InitXY();
                if (Type != ElementType.Line)
                {
                    double.TryParse(strLeft, out xy);
                    location.X = xy;
                    double.TryParse(strTop, out xy);
                    location.Y = xy;
                    ((IControlBase)SMTElement).ShowShadow(location, PointType.Excursion, ((IControlBase)SMTElement));
                    ((IControlBase)SMTElement).ShowMe(location, ((IControlBase)SMTElement));
                }
                else
                {
                    string[] xx = strLeft.Split(',');
                    string[] yy = strTop.Split(',');
                    PointCollection points = new PointCollection();
                    for (int i = 0; i < xx.Length; i++)
                    {
                        double.TryParse(xx[i], out xy);
                        location.X = xy;
                        double.TryParse(yy[i], out xy);
                        location.Y = xy;
                        points.Add(location);
                    }
                    if (!string.IsNullOrEmpty(UniqueID))
                    {//加载已有连线使用(龙康才)
                        ((LineControl)SMTElement).UniqueID = UniqueID;
                    }
                    ((LineControl)SMTElement).ShowShadow(points);
                    ((LineControl)SMTElement).ShowMe(points);
                }

                this.AddElement(SMTElement);
                this.SetElementFocus(SMTElement);
            }
            return SMTElement;
        }
    
       /// <summary>
       /// 向画布中添加元素
       /// </summary>
       /// <param name="element"></param>
        private void AddElement(UIElement element)
        {
            if (IsExistsElement(element))//判断开始，结束在画布里面只有一个
            {
                return;
            }
            if (!cnsDesignerContainer.Children.Contains(element))
            {
                cnsDesignerContainer.Children.Add(element);
                ((IControlBase)element).Container = this;
            }
            if (!Elements.Contains(element))
                Elements.Add(element);
        }
        /// <summary>
        /// 判定当前列表中是否存在当前元素
        /// </summary>
        /// <param name="SMTElement"></param>
        /// <returns></returns>
        private bool IsExistsElement(UIElement SMTElement)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                if ((((IControlBase)SMTElement).Type == ElementType.Begin && ((IControlBase)this.Elements[i]).Type == ElementType.Begin))
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "容器中已存在开始控件!", "确定");
                    return true;
                }

                if ((((IControlBase)SMTElement).Type == ElementType.Finish && ((IControlBase)this.Elements[i]).Type == ElementType.Finish))
                {
                    ComfirmWindow.ConfirmationBox("提示信息", "容器中已存在结束控件!", "确定");
                    return true;
                }                
            }

            return false;
        }
        /// <summary>
        /// 选中所有元素
        /// </summary>
        private void SelectAllElement()
        {
            for (int i = 0; i < this.Elements.Count; i++)
            {
                this.SelectedElementAdd(this.Elements[i]);
            }
        }

        /// <summary>
        /// 添加选中元素
        /// </summary>
        /// <param name="element"></param>
        private void SelectedElementAdd(UIElement element)
        {
            if (!SelectedElements.Contains(element))
            {
                SelectedElements.Add(element);
                this.ReorderZindex();
                ((IControlBase)element).SetFocus();
            }
        }

        /// <summary>
        /// 删除选中元素
        /// </summary>
        /// <param name="element"></param>
        private void SelectedElementRemove(UIElement element)
        {
            if (SelectedElements.Contains(element))
            {
                SelectedElements.Remove(element);
                ((IControlBase)element).SetUnFocus();
            }
        }

        /// <summary>
        /// 删除所有元素
        /// </summary>
        private void SelectedElementRemoveAll()
        {
            if (SelectedElements == null || SelectedElements.Count == 0) return;
            for (int i = SelectedElements.Count - 1; i >= 0; i--)
            {
                ((IControlBase)SelectedElements[i]).SetUnFocus();
                SelectedElements.RemoveAt(i);
            }
        }

        /// <summary>
        /// 取得内部的元素
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void GetInsideElement(Point point, double x, double y)
        {
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (((IControlBase)this.Elements[i]).IsInside(point, x, y)) this.SelectedElementAdd(this.Elements[i]);
            }
        }


        /// <summary>
        /// 从容器中删除选中的元素
        /// </summary>
        private void DeleteSelectedElements()
        {
            if (this.SelectedElements == null || this.SelectedElements.Count == 0) return;
            for (int i = SelectedElements.Count - 1; i >= 0; i--)
            {
                if (((IControlBase)this.SelectedElements[i]).Type != ElementType.Line)
                {
                    for (int j = 0; j < ((IControlBase)this.SelectedElements[i]).BeginLines.Count; j++)
                    {
                        ((LineControl)((IControlBase)this.SelectedElements[i]).BeginLines[j]).BeginElement = null;
                    }
                    for (int j = 0; j < ((IControlBase)this.SelectedElements[i]).EndLines.Count; j++)
                    {
                        ((LineControl)((IControlBase)this.SelectedElements[i]).EndLines[j]).EndElement = null;
                    }
                }
                else
                {
                    if (((LineControl)this.SelectedElements[i]).BeginElement != null)
                    {
                        ((IControlBase)((LineControl)this.SelectedElements[i]).BeginElement).BeginLinesRemove(this.SelectedElements[i]);
                        ((LineControl)this.SelectedElements[i]).BeginElement = null;
                    }
                    if (((LineControl)this.SelectedElements[i]).EndElement != null)
                    {
                        ((IControlBase)((LineControl)this.SelectedElements[i]).EndElement).EndLinesRemove(this.SelectedElements[i]);
                        ((LineControl)this.SelectedElements[i]).EndElement = null;
                    }
                }
                this.cnsDesignerContainer.Children.Remove(this.SelectedElements[i]);
                this.Elements.Remove(this.SelectedElements[i]);
                this.SelectedElements.RemoveAt(i);
            }
            //this.CopyedElements.Clear();
            this.ShowElementProperty();
        }
        #endregion

        #region 元素移动处理

        /// <summary>
        /// 移动元素
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void MoveShadow(double x, double y)
        {
            if (!CheckMove(x, y)) return;
            for (int i = 0; i < this.SelectedElements.Count; i++)
            {
                ((IControlBase)this.SelectedElements[i]).MoveShadow(x, y);
            }
            this.MoveUnFocusLinesShadow();
        }

        /// <summary>
        /// 检查是否移动
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool CheckMove(double x, double y)
        {
            bool CanMove = true;
            Point point;
            for (int i = 0; i < this.SelectedElements.Count; i++)
            {
                point = ((IControlBase)this.SelectedElements[i]).GetShadow();
                point.X += x; point.Y += y;
                ((IControlBase)this.SelectedElements[i]).InitXY();
                CanMove = ((IControlBase)this.SelectedElements[i]).CheckPoint(point);
                if (!CanMove) break;
            }
            return CanMove;
        }

        /// <summary>
        /// 移动元素
        /// </summary>
        private void Move()
        {
            for (int i = 0; i < this.SelectedElements.Count; i++)
            {
                ((IControlBase)this.SelectedElements[i]).Move();
            }
            this.MoveUnFocusLines();
            this.NeedMoveUnFocusLines.Clear();
        }
        #endregion
       
        #region 连线处理
        /// <summary>
        /// 设置连线连接的元素
        /// </summary>
        /// <param name="Elements"></param>
        /// <param name="Line"></param>
        private void SetLineLinkElement(List<UIElement> elements, UIElement line)
        {
            if (((IControlBase)line).Type != ElementType.Line) return;

            ((LineControl)line).ShapeLine.BeginElement = this.GetElementByPoint(elements, ((LineControl)line).GetBegin());
            if (((LineControl)line).ShapeLine.BeginElement != null)
            {
                ((LineControl)line).ShapeLine.BeginElementHotspot = ((IControlBase)((LineControl)line).ShapeLine.BeginElement).GetNearHotspot(((LineControl)line).GetBegin());
                ((IControlBase)((LineControl)line).ShapeLine.BeginElement).BeginLinesAdd(((LineControl)line).ShapeLine.lineControl);
            }
            System.Diagnostics.Debug.WriteLine(this.GetElementByPoint(((LineControl)line).GetEnd()));
            ((LineControl)line).ShapeLine.EndElement = this.GetElementByPoint(elements, ((LineControl)line).GetEnd());
            System.Diagnostics.Debug.WriteLine(((LineControl)line).ShapeLine.EndElement);
            if (((LineControl)line).ShapeLine.EndElement != null)
            {
                ((LineControl)line).ShapeLine.EndElementHotspot = ((IControlBase)((LineControl)line).ShapeLine.EndElement).GetNearHotspot(((LineControl)line).GetEnd());
                ((IControlBase)((LineControl)line).ShapeLine.EndElement).EndLinesAdd(((LineControl)line).ShapeLine.lineControl);
            }
        }

        private UIElement GetElementByPoint(List<UIElement> elements, Point point)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                switch (((IControlBase)elements[i]).Type)
                {
                    case ElementType.Begin:
                        if (((IControlBase)elements[i]).PointIsInside(point)) return elements[i];
                        break;
                    case ElementType.Activity:
                    case ElementType.Condition:
                        if (((IControlBase)elements[i]).PointIsInside(point)) return elements[i];
                        break;
                    case ElementType.Finish:
                        if (((IControlBase)elements[i]).PointIsInside(point)) return elements[i];
                        break;
                }
            }
            return null;
        }
        #endregion
     
      

        /// <summary>
        /// 控制显示各个活动的右侧属性
        /// </summary>   
        private void ShowElementProperty()
        {
            if (SelectedElements.Count == 0)
            {
                ucFlowSetting.ShowProperty(null);                    
            }
            else
            {
                if (SelectedElements.Count != 1) return;

                UIElement element = this.SelectedElements[0];
                
                if (((IControlBase)element).Type == ElementType.Activity)
                {
                    ucFlowSetting.activityProperty.ActivityObjectData = GetActivityObjectData(((IControlBase)element).UniqueID);
                }
                if (((IControlBase)element).Type == ElementType.Line)
                {
                    ucFlowSetting.lineProperty.LineObjectData = GetLineObjectData(((IControlBase)element).UniqueID);
                }
                ucFlowSetting.ShowProperty(element);
            }       
        }       

        private int GetActivieyIdentityNo()
        {
            int iNo = 0;
            List<int> lstNo = new List<int>();
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (((IControlBase)this.Elements[i]).Type == ElementType.Activity)
                {
                    if (((IControlBase)this.Elements[i]).Title.IndexOf("新建活动") != -1)
                    {
                        Int32.TryParse(((IControlBase)this.Elements[i]).Title.Substring(("新建活动").Length), out iNo);
                        if (iNo != 0) lstNo.Add(iNo);
                    }
                }
            }
            return DesignerUtils.GetIdentityNo(lstNo);
        }

        /// <summary>
        /// 取得容器中线段数
        /// </summary>
        /// <returns></returns>
        private int GetLineIdentityNo()
        {
            int iNo = 0;
            List<int> lstNo = new List<int>();
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (((IControlBase)this.Elements[i]).Type == ElementType.Line)
                {
                    if (((IControlBase)this.Elements[i]).Title.IndexOf("新建连线") != -1)
                    {
                        Int32.TryParse(((IControlBase)this.Elements[i]).Title.Substring(("新建连线").Length), out iNo);
                        if (iNo != 0) lstNo.Add(iNo);
                    }
                }
            }
            return DesignerUtils.GetIdentityNo(lstNo);
        }
       
        #endregion 私有方法

        #region 画布控件事件

        /// <summary>
        /// 设置放大缩小比例
        /// </summary>
        /// <param name="zoomFactor"></param>
        private void SetZoom(double zoomFactor)
        {
            if (this.zoomTransform != null)
            {
                this.zoomTransform.ScaleX = zoomFactor;
                this.zoomTransform.ScaleY = zoomFactor;
                //this.cnsDesignerContainer.Width = this.cnsDesignerContainer.ActualWidth * this.zoomTransform.ScaleX;
                //this.cnsDesignerContainer.Height = this.cnsDesignerContainer.ActualHeight * this.zoomTransform.ScaleY;
            }
            else
            {
                this._initialZoom = zoomFactor;
            }
        }
        /// <summary>
        /// 还原放大缩小控件
        /// </summary>
        public void ResetZoom()
        {
            _zoomcontrol.Zoom = 1.0;
            SetZoom(1.0);
        }
        /// <summary>
        /// 缩放事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _zoomcontrol_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetZoom(_zoomcontrol.Zoom);           
        }

        bool trackingPointMouseMove = false;
        bool pointHadActualMove = false;
        bool startSelectArea = false;
        Point originalPosition, mousePosition;
        private void DesignerContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pointHadActualMove = false;
            trackingPointMouseMove = false;

            mousePosition = e.GetPosition(this.cnsDesignerContainer);
            originalPosition = e.GetPosition(this.cnsDesignerContainer);

            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element == null) return;
            if (element.Name != this.cnsDesignerContainer.Name)
            {
                for (int i = 0; i < this.SelectedElements.Count; i++)
                {
                    ((IControlBase)this.SelectedElements[i]).SetXY(mousePosition);
                }

                element.Cursor = Cursors.Hand;

                trackingPointMouseMove = true;
            }
            else
            {
                this.SelectedElementRemoveAll();
                startSelectArea = true;
                this.Focus();
            }
            element.CaptureMouse();
            e.Handled = true;
        }

        private void DesignerContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (trackingPointMouseMove)
            {
                if (mousePosition != e.GetPosition(this.cnsDesignerContainer))
                {
                    mousePosition = e.GetPosition(this.cnsDesignerContainer);
                    pointHadActualMove = true;
                    for (int i = 0; i < this.SelectedElements.Count; i++)
                    {
                        ((IControlBase)this.SelectedElements[i]).ShowShadow(mousePosition, PointType.Excursion, this);
                        this.MoveUnFocusLinesShadow();
                        if (((IControlBase)this.SelectedElements[i]).CheckPoint(mousePosition) == false) pointHadActualMove = false;
                    }
                }
            }
            else if (startSelectArea)
            {
                mousePosition = e.GetPosition(this.cnsDesignerContainer);
                if (this._selectArea == null)
                {
                    this._selectArea = new Rectangle();

                    SolidColorBrush brush = new SolidColorBrush();
                    brush.Color = Color.FromArgb(255, 234, 213, 2);
                    this._selectArea.Fill = brush;
                    this._selectArea.Opacity = 0.2;

                    brush = new SolidColorBrush();
                    brush.Color = Color.FromArgb(255, 0, 0, 0);
                    this._selectArea.Stroke = brush;
                    this._selectArea.StrokeMiterLimit = 2.0;

                    cnsDesignerContainer.Children.Add(this._selectArea);
                }
                if (mousePosition.X >= originalPosition.X)
                {
                    if (mousePosition.Y >= originalPosition.Y)
                    {
                        this._selectArea.SetValue(Canvas.TopProperty, originalPosition.Y);
                        this._selectArea.SetValue(Canvas.LeftProperty, originalPosition.X);
                    }
                    else
                    {
                        this._selectArea.SetValue(Canvas.TopProperty, mousePosition.Y);
                        this._selectArea.SetValue(Canvas.LeftProperty, originalPosition.X);
                    }
                }
                else
                {
                    if (mousePosition.Y >= originalPosition.Y)
                    {
                        this._selectArea.SetValue(Canvas.TopProperty, originalPosition.Y);
                        this._selectArea.SetValue(Canvas.LeftProperty, mousePosition.X);
                    }
                    else
                    {
                        this._selectArea.SetValue(Canvas.TopProperty, mousePosition.Y);
                        this._selectArea.SetValue(Canvas.LeftProperty, mousePosition.X);
                    }
                }

                this._selectArea.Width = Math.Abs(mousePosition.X - originalPosition.X);
                this._selectArea.Height = Math.Abs(mousePosition.Y - originalPosition.Y);
            }
        }

        private void DesignerContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element != null)
            {
                element.Cursor = Cursors.Arrow;
                element.ReleaseMouseCapture();
            }
            if (trackingPointMouseMove)
            {
                if (pointHadActualMove)
                {
                    for (int i = 0; i < this.SelectedElements.Count; i++)
                    {
                        ((IControlBase)this.SelectedElements[i]).ShowMe(mousePosition, this);
                    }
                    this.MoveUnFocusLines();
                    pointHadActualMove = false;
                }
                else
                {
                    if (mousePosition != originalPosition)
                    {
                        for (int i = 0; i < this.SelectedElements.Count; i++)
                        {
                            Point point = ((IControlBase)this.SelectedElements[i]).GetMe();
                            ((IControlBase)this.SelectedElements[i]).ShowShadow(point, PointType.Precision, this);
                        }
                        this.MoveUnFocusLines();
                    }
                }
                this.NeedMoveUnFocusLines.Clear();
                trackingPointMouseMove = false;
            }
            else if (startSelectArea)
            {
                if (this._selectArea != null)
                {
                    Point point = new Point(Canvas.GetLeft(this._selectArea), Canvas.GetTop(this._selectArea));
                    this.GetInsideElement(point, this._selectArea.Width, this._selectArea.Height);
                    this.cnsDesignerContainer.Children.Remove(this._selectArea);
                    this._selectArea = null;
                }
                startSelectArea = false;
            }
            this.ShowElementProperty();
            e.Handled = true;
        }


       

        #endregion 控件事件

        #region 方向键盘移动
        /// <summary>
        /// 移动步长
        /// </summary>
        private int MoveStepLenght
        {
            get
            {
                if (CtrlKeyIsPress)
                    return 5;
                return 1;
            }
        }
        /// <summary>
        /// 向左移动
        /// </summary>
        private void MoveShadowLeft()
        {
            MoveShadow(-this.MoveStepLenght, 0);
        }
        /// <summary>
        /// 向上移动
        /// </summary>
        private void MoveShadowUp()
        {
            MoveShadow(0, -this.MoveStepLenght);
        }
        /// <summary>
        /// 向右移动
        /// </summary>
        private void MoveShadowRight()
        {
            MoveShadow(this.MoveStepLenght, 0);
        }

        /// <summary>
        /// 向下移动
        /// </summary>
        private void MoveShadowDown()
        {
            MoveShadow(0, this.MoveStepLenght);
        }
        #endregion

        #region 键盘事件

        /// <summary>
        /// 鼠标按下时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    this.MoveShadowUp();
                    break;
                case Key.Down:
                    this.MoveShadowDown();
                    break;
                case Key.Left:
                    this.MoveShadowLeft();
                    break;
                case Key.Right:
                    this.MoveShadowRight();
                    break;
                case Key.A:
                    if (this.CtrlKeyIsPress) this.SelectAllElement();
                    break;
                //case Key.C:
                //    if (this.CtrlKeyIsPress) this.CopySelectedWF11Elements();
                //    break;
            }
        }
        /// <summary>
        /// 鼠标按下并放开时触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    this.DeleteSelectedElements();
                    break;
                #region 2012/3/13
                //case Key.Up:
                //    this.MoveShadowUp();
                //    break;
                //case Key.Down:
                //    this.MoveShadowDown();
                //    break;
                //case Key.Left:
                //    this.MoveShadowLeft();
                //    break;
                //case Key.Right:
                //    this.MoveShadowRight();
                //    break;
                #endregion
                #region 2012/3/13
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                    this.Move();
                    break;
                #endregion 
                //case Key.V:
                //    if (this.CtrlKeyIsPress) this.PlasterCopyedWF11Elements();
                //    break;
            }
        }
        #endregion

        /// <summary>
        /// 被拷贝的元素集
        /// </summary>
        public List<UIElement> CopyedElements
        {
            get
            {
                if (_copyedElements == null) _copyedElements = new List<UIElement>();

                return _copyedElements;
            }
        }
    }
}
