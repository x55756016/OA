/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：ContainerInterface.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/16 16:39:48   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerView 
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
using SMT.Workflow.Platform.Designer.DesignerControl;
using System.Collections.Generic;

namespace SMT.Workflow.Platform.Designer.DesignerView
{
    public partial class Container : UserControl, IContainer
    {
        #region 初始化变量区

        //流程ID
        private string _uniqueID;

        //流程名称
        private string _name;

        //容器中的所有元素集
        private List<UIElement> _elements;

        //选中的元素集
        private List<UIElement> _selectedElements;

        //上一个选中的元素
        private UIElement _lastSelectedElement;

        //需要移动的非焦点连线
        private List<UIElement> _needMoveUnFocusLines;

        #endregion
        /// <summary>
        /// 流程ID
        /// </summary>
        public string UniqueID
        {
            get
            {
                if (string.IsNullOrEmpty(_uniqueID)) _uniqueID = Guid.NewGuid().ToString().Replace("-", "");

                return _uniqueID;
            }
            set { _uniqueID = value; }
        }
        /// <summary>
        /// 流程名称
        /// </summary>
        public string Title
        {
            get { return _name; }
            set { _name = value; }
        }
        #region 实现接口属性、方法

        /// <summary>
        /// 获取所有活动元素
        /// </summary>
        /// <returns></returns>
        public List<UIElement> GetActivitys()
        {
            List<UIElement> activitys = new List<UIElement>();
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (((IControlBase)this.Elements[i]).Type == ElementType.Activity)
                {
                    activitys.Add(this.Elements[i]);
                }
            }

            return activitys;
        }

        /// <summary>
        /// Ctrl键是否按下
        /// </summary>
        public bool CtrlKeyIsPress
        {
            get
            {
                return (Keyboard.Modifiers == ModifierKeys.Control);
            }
        }

        /// <summary>
        /// 设置选中的元素, 控件里MouseLeftButtonDown调用此方法
        /// </summary>
        /// <param name="Element"></param>
        public void SetElementSelected(UIElement element)
        {
            if (CtrlKeyIsPress)
            {
                if (((IControlBase)element).State == ElementState.UnFocus) this.SelectedElementAdd(element);
                else this.SelectedElementRemove(element);

                this.ShowElementProperty();
            }
            else this.SetElementFocus(element);

        }

        /// <summary>
        /// 是否多选
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool IsMultiSelected(UIElement element)
        {
            if (((IControlBase)element).State == ElementState.UnFocus) return false;
            else if (this.SelectedElements.Count < 2) return false;
            return true;
        }

        /// <summary>
        /// 通过点的位置取当前元素, LineShape、LineTools里Hotspot_MouseMove调用此方法
        /// </summary>
        /// <param name="point">点对象</param>
        /// <returns></returns>
        public UIElement GetElementByPoint(Point point)
        {
            for (int i = 0; i < this.Elements.Count; i++)
            {
                switch (((IControlBase)this.Elements[i]).Type)
                {
                    case ElementType.Begin:
                        if (((IControlBase)this.Elements[i]).PointIsInside(point)) return this.Elements[i];
                        break;
                    case ElementType.Activity:
                    case ElementType.Condition:
                        if (((IControlBase)this.Elements[i]).PointIsInside(point)) return this.Elements[i];
                        break;
                    case ElementType.Finish:
                        if (((IControlBase)this.Elements[i]).PointIsInside(point)) return this.Elements[i];
                        break;
                }
            }
            return null;
        }

        public void NeedMoveUnFocusLineAdd(UIElement element)
        {
            if (!NeedMoveUnFocusLines.Contains(element))
            {
                NeedMoveUnFocusLines.Add(element);
            }
        }

        public bool IsMouseSelecting
        {
            get
            {
                return (this._selectArea != null);
            }
        }


        /// <summary>
        /// 容器左边距
        /// </summary>
        public double SimpleShapeLeft
        {
            get { return (this.spnlSimpleElementContainer.ActualWidth - this.cnsSimpleElementContainer.ActualWidth) / 2; }

        }


        /// <summary>
        /// 创建流程元素, 控件里当鼠标指针位于元素上并释放鼠标左键时事件调用此方法(ActivityTools里MouseLeftButtonUp方法)
        /// </summary>
        /// <param name="Type">元素类型</param>
        /// <param name="locations">位置</param>
        /// <returns></returns>
        public UIElement CreateElement(ElementType type, PointCollection locations)
        {
            UIElement element = this.ElementFactory(type);
            if (element != null)
            {
                ((IControlBase)element).ZIndex = this.Elements.Count;
                ((IControlBase)element).InitXY();
                if (type != ElementType.Line)
                {
                    ((IControlBase)element).ShowShadow(locations[0], PointType.Excursion, ((IControlBase)element));
                    ((IControlBase)element).ShowMe(locations[0], ((IControlBase)element));
                }
                else
                {
                    PointCollection points = ((LineControl)element).GetPloyline(locations[0], locations[1]);
                    ((LineControl)element).ShowShadow(points);
                    ((LineControl)element).ShowMe(points);
                    //判断线的两端是否有对象
                    this.SetLineLinkElement(this.Elements, element);
                }

                this.AddElement(element);
                this.SetElementFocus(element);
            }
            return element;
        }

        #endregion
    }
}
