using SMT.SAAS.Controls.Toolkit.Windows;
using System.Reflection;

///<summary>
///系统窗口
///</summary>
namespace System.Windows.Controls
{
    /// <summary>
    /// 窗口类
    /// 此类封装了对SMT.SAAS.Controls.Toolkit.Windows中窗口的调用.
    /// <remarks>
    /// 使用窗口前需要设置Parent与Wrapper属性.
    /// Parent:属性为窗口的父容器.容器类型为Canvas
    /// Wrapper:属性为Parent所在的对象
    /// </remarks>
    /// </summary>
    public class Window : UserControl
    {
        private SMT.SAAS.Controls.Toolkit.Windows.Window _window;
        public static readonly DependencyProperty TitleContentProperty =
           DependencyProperty.Register("TitleContent", typeof(object), typeof(Window), new PropertyMetadata("Title"));
        /// <summary>
        /// 窗口标题.
        /// 用于显示在窗口左上角的提示性文字
        /// </summary>
        public object TitleContent
        {
            get
            {
                return (object)this.GetValue(Window.TitleContentProperty);
            }
            set
            {
                this.SetValue(Window.TitleContentProperty, value);
            }
        }
        /// <summary>
        /// 父窗口对象.当窗口打开后可以使用此窗口对窗口进行控制
        /// <example>
        /// 控制窗口大小,让其以1024*1024大小显示
        /// <code>
        ///    Size windowSize = new Size() { Width = 1024, Height = 1024 };
        ///    ParentWindow.Width = windowSize.Width;
        ///    ParentWindow.Height = windowSize.Height;
        /// </code>
        /// </example>
        /// </summary>
        public SMT.SAAS.Controls.Toolkit.Windows.Window ParentWindow
        {
            get { return _window.IsNotNull() ? _window : null; }
        }
        /// <summary>
        /// 窗口所在父容器.
        /// </summary>
        public static Canvas Parent
        {
            private get { return WindowsManager.Desktop; }
            set
            {
                WindowsManager.Desktop = value;
            }
        }
        /// <summary>
        /// 是否显示窗口标题
        /// </summary>
        public static bool IsShowtitle { get; set; }
        /// <summary>
        /// 窗口任务栏
        /// </summary>
        public static StackPanel TaskBar
        {
            private get { return WindowsManager.TaskBar; }
            set
            {
                WindowsManager.TaskBar = value;

            }
        }
        /// <summary>
        /// 窗口父容器所在的对象
        /// </summary>
        public static UserControl Wrapper
        {
            get { return ProgramManager.Wrapper; }
            set { ProgramManager.Wrapper = value; }
        }
        /// <summary>
        /// 显示对话框
        /// </summary>
        public void ShowDialog()
        {
            Show();
        }
        /// <summary>
        /// 窗口内容
        /// </summary>
        public UIElement Content
        {
            get { return base.Content; }
            set { base.Content = value; }
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        public void Show()
        {

            this._window = ProgramManager.ShowProgram(TitleContent.ToString(), string.Empty, Guid.NewGuid().ToString(), this, true, false, null);
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="GUID">
        /// 窗口标识.
        /// 可根据此ID限制此窗口只能弹出一个
        /// </param>
        public void Show(string GUID)
        {
            this._window = ProgramManager.ShowProgram(TitleContent.ToString(), string.Empty, GUID, this, true, false, null);
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <typeparam name="TResult">窗口返回的结果的类型</typeparam>
        /// <param name="windowmodel">窗口模式,为<see cref="DialogMode"/></param>类型
        /// <param name="parent">窗口父容器</param>
        /// <param name="result">此参数未使用</param>
        /// <param name="close">此参数未使用</param>
        public void Show<TResult>(DialogMode windowmodel, FrameworkElement parent, TResult result, Action<TResult> close)
        {
            if (parent.IsNull())
            {
                this.Show<TResult>(windowmodel, parent, result, close, false);
            }
            else
            {
                this.Show<TResult>(windowmodel, this, result, close, false);

            }         
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <typeparam name="TResult">窗口返回的结果的类型</typeparam>
        /// <param name="windowmodel">窗口模式,为<see cref="DialogMode"/></param>类型
        /// <param name="parent">窗口父容器</param>
        /// <param name="result">此参数未使用</param>
        /// <param name="close">此参数未使用</param>
        /// <param name="isResizable">窗口是否可拖拽大小</param>
        public void Show<TResult>(DialogMode windowmodel, FrameworkElement parent, TResult result, Action<TResult> close, bool isResizable)
        {
            //this._window = ProgramManager.ShowProgram(TitleContent.ToString(), "", Guid.NewGuid().ToString(), this, isResizable, false, null);
            this.Show<TResult>(windowmodel, this, result, close, true,Guid.NewGuid().ToString());
        }
       
        

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <typeparam name="TResult">窗口返回的结果的类型</typeparam>
        /// <param name="windowmodel">窗口模式,为<see cref="DialogMode"/></param>类型
        /// <param name="parent">窗口父容器</param>
        /// <param name="result">此参数未使用</param>
        /// <param name="close">此参数未使用</param>
        /// <param name="isResizable">窗口是否可拖拽大小</param>
        /// <param name="GUID">
        /// 窗口标识.
        /// 可根据此ID限制此窗口只能弹出一个
        /// </param>
        public void Show<TResult>(DialogMode windowmodel, FrameworkElement parent, TResult result, Action<TResult> close, bool isResizable, string GUID)
        {
            this._window = ProgramManager.ShowProgram(TitleContent.ToString(), string.Empty, GUID, this, isResizable, true, null);
        }

        public void Show<TResult>(DialogMode windowmodel, FrameworkElement parent, TResult result, Action<TResult> close, bool isResizable, bool isSysApp, string GUID)
        {
            this._window = ProgramManager.ShowProgram(TitleContent.ToString(), string.Empty, GUID, this, isResizable, isSysApp, null);
        }

        /// <summary>
        /// 显示窗口
        /// 兼容MVC 平台弹出窗口
        /// Author:傅意成
        /// Date:2012-07-12
        /// </summary>
        public void ShowMvcPlat<TResult>(DialogMode windowmodel, FrameworkElement parent, TResult result, Action<TResult> close)
        {
            this._window = ProgramManager.ShowMvcProgram(TitleContent.ToString(), string.Empty, Guid.NewGuid().ToString(),this,false,true, null);
        }

        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        public void Close()
        {
            if (_window.IsNotNull())
                _window.Close();
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="windowmodel">窗口模式,为<see cref="DialogMode"/></param>类型
        /// <param name="program">窗口关联数据.为<see cref="Program"/>类型</param>
        /// <remarks>
        /// 使用静态函数调用窗口,将不能使用ParentWindow
        /// </remarks>
        public static void Show(DialogMode windowModel, Program program)
        {
            ProgramManager.ShowProgram(program);
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="program">窗口关联数据.为<see cref="Program"/>类型</param>
        /// <param name="source">窗口内容所属程序集</param>
        /// <param name="param">窗口参数</param>
        /// <remarks>
        /// 使用静态函数调用窗口,将不能使用ParentWindow
        /// </remarks>
        public static void Show(Program program, Assembly source, object param)
        {
            ProgramManager.ShowProgram(program, source, param);

        }

        public static void Show(Program program, string assemblyName, object param)
        {
            ProgramManager.ShowProgram(program, assemblyName, param,true);

        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="windowmodel">窗口模式,为<see cref="DialogMode"/></param>类型
        /// <param name="program">窗口关联数据.为<see cref="Program"/>类型</param>
        /// <param name="source">窗口内容所属程序集</param>
        /// <param name="param">窗口参数</param>
        /// <remarks>
        /// 使用静态函数调用窗口,将不能使用ParentWindow
        /// </remarks>
        public static void Show(DialogMode windowModel, Program program, Assembly source, object param)
        {
            ProgramManager.ShowProgram(program, source, param);
        }
        /// <summary>
        /// 打开窗口
        /// </summary>
        /// <param name="Title">窗口标题</param>
        /// <param name="IocPath">窗口图标</param>
        /// <param name="windowID"> 
        /// 窗口标识.
        /// 可根据此ID限制此窗口只能弹出一个</param>
        /// <param name="isResizable">是否可控制窗口大小</param>
        /// <param name="isSysApp">是否为系统窗口</param>
        /// <param name="content">窗口内容</param>
        /// <param name="param">窗口参数</param>
        /// <remarks>
        /// 使用静态函数调用窗口,将不能使用ParentWindow
        /// </remarks>
        public static void Show(string Title, string IocPath, string windowID, bool isResizable, bool isSysApp, object content, object param)
        {
            ProgramManager.ShowProgram(Title, IocPath, windowID, content, isResizable, isSysApp, param);
        }
    }
    #region 枚举 窗口模式
    /// <summary>
    /// 对话框模式
    /// </summary>
    public enum DialogMode
    {
        /// <summary>
        /// 默认模式
        /// </summary>
        Default,
        /// <summary>
        /// 模式对话框，当前父窗口
        /// </summary>
        Modal,
        /// <summary>
        /// 应用程序模式对话框，当前应用程序
        /// </summary>
        ApplicationModal
    }
    #endregion

    #region 枚举 窗口消失扩展
    /// <summary>
    /// 窗口自动消失
    /// </summary>
    public enum AutoDisappear
    {
        /// <summary>
        /// 消失
        /// </summary>
        Yes,
        /// <summary>
        ///停留，不消失 
        /// </summary>
        No,
        /// <summary>
        /// 原使用方式
        /// </summary>
        Normal
    }
    #endregion
}
