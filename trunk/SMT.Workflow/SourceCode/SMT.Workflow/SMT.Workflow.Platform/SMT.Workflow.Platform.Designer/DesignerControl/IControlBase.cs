/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：ElementControl.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 10:57:31   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerControl 
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
using SMT.Workflow.Platform.Designer.DesignerView;

namespace SMT.Workflow.Platform.Designer.DesignerControl
{
    public interface IControlBase
    {
        IContainer Container { get; set; }

        string Title { get; set; }
        ElementState State { get; }
        ElementType Type { get; }
        int MaxBeginLines { get; }
        List<UIElement> BeginLines { get; }
        List<UIElement> EndLines { get; }
        int MaxEndLines { get; }
        //string UniqueID { get; set; }
        //string Title { get; set; }
        int ZIndex { get; set; }

        bool BeginLinesAdd(UIElement element);
        void BeginLinesRemove(UIElement element);

        bool EndLinesAdd(UIElement element);
        void EndLinesRemove(UIElement element);

        void SetFocus();
        void SetUnFocus();
        void SetSelected();
        void InitXY();
        void SetXY(Point mousePoint);
        void ShowShadow(Point point, PointType PointType, object sender);
        Point GetShadow();
        void ShowMe(Point mousePoint, object sender);
        Point GetMe();
        bool CheckPoint(Point mousePoint);
        bool IsInside(Point point, double x, double y);

        Point Location { get; }
        Point GetHotspot(HotspotType HotspotType);
        Point GetShadowHotspot(HotspotType HotspotType);
        HotspotType GetNearHotspot(Point point);
        bool PointIsInside(Point point);

        void MoveShadow(double x, double y);
        void Move();
        #region  xml导入、导出
        string UniqueID { get; set; }
        string ToXmlString();
        #endregion
    }
}
