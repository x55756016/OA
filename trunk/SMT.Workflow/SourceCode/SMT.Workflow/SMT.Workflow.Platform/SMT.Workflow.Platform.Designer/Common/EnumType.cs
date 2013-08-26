/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：EnumType.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/18 15:41:40   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Designer.Common 
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

namespace SMT.Workflow.Platform.Designer.Common
{
    /// <summary>
    /// 页面操作枚举
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 页面新增
        /// </summary>
        Add = 1,

        /// <summary>
        /// 页面修改
        /// </summary>
        Update = 2,

        /// <summary>
        /// 页面删除
        /// </summary>
        Delete = 3
    }
    /// <summary>
    /// 提示框信息信息类型
    /// </summary>
    public enum MessageTypes
    {
        Message,
        Caution,
        Error
    }
}


