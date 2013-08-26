/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：IShape.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/14 9:06:47   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.DesignerShape 
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

namespace SMT.Workflow.Platform.Designer.DesignerShape
{
    public interface IShape
    {
        /// <summary>
        /// 获取控件状态
        /// </summary>
        ElementState State { get; }

        /// <summary>
        /// 设置控件获得焦点状态
        /// </summary>
        void SetFocus();
        /// <summary>
        /// 设置控件失去焦点状态
        /// </summary>
        void SetUnFocus();
        /// <summary>
        /// 设置控件为待选
        /// </summary>
        void SetSelected();

        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="title">标题</param>
        void SetTitle(string title);
        /// <summary>
        /// 获取标题
        /// </summary>
        /// <returns>标题</returns>
        string GetTitle();
        /// <summary>
        /// 用颜色填充
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="opacity">透明度</param>
        void Fill(Color color, double opacity);
    }
}
