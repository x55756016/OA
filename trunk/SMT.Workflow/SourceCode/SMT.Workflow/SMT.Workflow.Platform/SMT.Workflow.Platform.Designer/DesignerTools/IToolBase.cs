/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：IToolBase.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 13:51:32   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerTools 
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
using SMT.Workflow.Platform.Designer.DesignerView;

namespace SMT.Workflow.Platform.Designer.DesignerTools
{
    public interface IToolBase
    {
        /// <summary>
        /// 简单流程元素的类型
        /// </summary>
        ElementType Type { get; }
        /// <summary>
        /// 初始化简单流程元素
        /// </summary>
        /// <param name="container">父控件</param>
        /// <param name="startLocation">简单流程元素的开始位置</param>
        /// <param name="cnsParent">简单流程元素的父Canvas</param>
        /// <param name="canvasLeft">简单流程元素父Canvas在container中的Left</param>
        /// <param name="canvasTop">简单流程元素父Canvas在container中的Top</param>
        void InitializeMe(IContainer container, Point startLocation, Canvas cnsParent, double canvasLeft, double canvasTop);

    }
}
