/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：zhangyh   
	 * 创建日期：2011/10/21 9:25:36   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer 
	 * 描　　述： 属性接口
	 * 模块名称：工作流设计器
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

namespace SMT.Workflow.Platform.Designer.ActivityProperty
{
    public interface IProperty
    {
        /// <summary>
        /// 显示属性窗口
        /// </summary>
        /// <param name="element"></param>
        void ShowPropertyWindow(UIElement element);

        /// <summary>
        /// 加载属性
        /// </summary>
        void LoadProperty();
    }
}