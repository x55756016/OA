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

namespace SMT.SaaS.FrameworkUI
{
    public class UIRefreshedEventArgs : EventArgs
    {
        public RefreshedTypes RefreshedType { get; set; }
        public string RefreshedArgs { get; set; }
    }
    public enum RefreshedTypes
    { 
        /// <summary>
        ///    GenerateLeftMenu(); GenerateToolBar();   GenerateEntityInfoCtr(); ReloadData();
        /// </summary>
        All,
        /// <summary>
        /// 左边菜单
        /// </summary>
        LeftMenu,
        /// <summary>
        /// 头部工具栏
        /// </summary>
        ToolBar,
        /// <summary>
        /// 窗体标题
        /// </summary>
        EntityInfo,
        /// <summary>
        /// 关闭窗体
        /// </summary>
        Close,
        /// <summary>
        /// 关闭窗口并触发重新加载数据的事件
        /// </summary>
        CloseAndReloadData,
        /// <summary>
        /// 刷新底部审核信息
        /// </summary>
        AuditInfo,
        /// <summary>
        /// 显示进度条
        /// </summary>
        ProgressBar,
        /// <summary>
        /// 隐藏进度条
        /// </summary>
        HideProgressBar,
        /// <summary>
        /// 显示进度条
        /// </summary>
        ShowProgressBar,
        /// <summary>
        /// 上传控件
        /// </summary>
        UploadBar,
        /// <summary>
        /// 隐藏审核信息栏
        /// </summary>
        HideAudit,
        /// <summary>
        /// 显示审核信息栏
        /// </summary>
        ShowAudit
    }
}
