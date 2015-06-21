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
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace SMT.SAAS.Controls.Toolkit.Windows
{
    /// <summary>
    /// 封装主界面需要加载的子程序信息,核心类
    /// </summary>
    public class Program
    {
        public Program()
        {
            this.InitParams = new Dictionary<string, object>();
        }
        public Program(string type, string title, string iconPath, bool isResizable, string taskBarIocPath)
        {
            this.Type = type;
            this.Title = title;
            this.IconPath = iconPath;
            this.IsResizable = isResizable;
            this.TaskBarIconPath = taskBarIocPath;
        }
        public Program(string type, string title, string iconPath, string taskBarIocPath, bool isResizable, string uri, bool isSysApp, ObservableCollection<Program> items) :
            this(type, title, iconPath, isResizable, taskBarIocPath)
        {
            this.Uri = uri; this.IsSysApp = isSysApp; this.Item = items;
        }
        /// <summary>
        /// 程序/文件夹标识
        /// </summary>
        public string ID
        {
            get;
            set;
        }
        /// <summary>
        /// 父级ID
        /// </summary>
        public string ParentID { get; set; }
        /// <summary>
        /// 桌面图标路径
        /// </summary>
        public string IconPath
        {
            get;
            set;
        }
        /// <summary>
        /// 任务栏图标路径
        /// </summary>
        public string TaskBarIconPath
        {
            get;
            set;
        }
        /// <summary>
        /// 应用程序窗口大小是否可控
        /// </summary>
        public bool IsResizable
        {
            get;
            set;
        }
        /// <summary>
        /// 在桌面以及任务栏显示的标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// 应用程序类型
        /// eg:Windows.ItemWindow..用于添加到Window之中
        /// </summary>
        public string Type
        {
            get;
            set;
        }
        public string AssemblyName{get;set;}
        /// <summary>
        /// 要打开的网页的地址
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// 应用程序类型
        /// </summary>
        public ProgramType ProgramType { get; set; }
        /// <summary>
        /// 识别是否有子程序,如果没有单击图标后直接打开指定的网址
        /// </summary>
        public bool IsSysApp { get; set; }
        /// <summary>
        /// 是否为系统应用
        /// </summary>
        public bool IsSysNeed { get; set; }
        /// <summary>
        /// 文件/类别下的子系统
        /// </summary>
        public ObservableCollection<Program> Item { get; set; }
        public Dictionary<string, object> InitParams { get; set; }
    }
    /// <summary>
    /// 枚举,应用程序类型
    /// </summary>
    public enum ProgramType
    { 
        /// <summary>
        /// 应用程序,可打开连接
        /// </summary>
        Program,
        /// <summary>
        /// 文件夹,为用户自定义
        /// </summary>
        Folder,
        /// <summary>
        /// 系统文件夹,包含系统应用程序
        /// </summary>
        SysFolder
    }
}
