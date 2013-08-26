/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：IContainer.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 11:00:25   
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
using System.Collections.Generic;
using SMT.Workflow.Platform.Designer.DesignerControl;

namespace SMT.Workflow.Platform.Designer.DesignerView
{
    public interface IContainer
    {

        List<UIElement> Elements { get; }
        List<UIElement> SelectedElements { get; }
        List<UIElement> NeedMoveUnFocusLines { get; }
        //List<UIElement> CopyedElements { get; }
        List<UIElement> GetActivitys();
        void NeedMoveUnFocusLineAdd(UIElement element);
        bool CtrlKeyIsPress { get; }
        void SetElementSelected(UIElement element);
        bool IsMultiSelected(UIElement element);

        UIElement GetElementByPoint(Point point);
        UIElement CreateElement(ElementType Type, PointCollection locations);
        double SimpleShapeLeft { get; }

        bool IsMouseSelecting { get; }

        void SetGridLines();

        void ResetZoom(); 

        //void ShowMessage(string message);
        //string ToXmlString();
        //void LoadFromXmlString(string xmlString);
    }
}
