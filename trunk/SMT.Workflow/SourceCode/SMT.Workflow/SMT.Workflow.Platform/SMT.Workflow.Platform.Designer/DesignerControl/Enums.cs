/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Enums.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 9:08:22   
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

namespace SMT.Workflow.Platform.Designer.DesignerControl
{
    /// <summary>
    /// 热点类型:Left , Top, Right, Bottom
    /// </summary>
    public enum HotspotType
    {
        Left = 0,
        Top,
        Right,
        Bottom
    }

    /// <summary>
    ///坐标类型
    /// </summary>
    public enum PointType
    {
        /// <summary>
        /// 精确坐标点 
        /// </summary>
        Precision = 0,
        /// <summary>
        /// 偏移坐标点
        /// </summary>
        Excursion
    }

    /// <summary>
    /// 活动节点类型
    /// </summary>
    public enum ElementType
    {
        /// <summary>
        /// 开始节点
        /// </summary>
        Begin = 0,
        /// <summary>
        /// 活动节点
        /// </summary>
        Activity,
        /// <summary>
        /// 条件节点
        /// </summary>
        Condition,
        /// <summary>
        /// 连线
        /// </summary>
        Line,
        /// <summary>
        /// 结束节点
        /// </summary>
        Finish
    }

    /// <summary>
    ///焦点状态
    /// </summary>
    public enum ElementState
    {
        /// <summary>
        /// 得到焦点
        /// </summary>
        Focus = 0,
        /// <summary>
        /// 失去焦点
        /// </summary>
        UnFocus,
        /// <summary>
        /// 选择焦点
        /// </summary>
        Selected
    }
}
