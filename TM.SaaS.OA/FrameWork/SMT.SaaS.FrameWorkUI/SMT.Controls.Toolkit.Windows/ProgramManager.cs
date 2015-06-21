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
using System.Collections.ObjectModel;
using System.Reflection;
using SMT.SAAS.Main;

namespace SMT.SAAS.Controls.Toolkit.Windows
{
    /// <summary>
    /// 任务管理器，核心类
    /// </summary>
    public class ProgramManager
    {
        public static bool ShowHideItem { get; set; }
        public static bool ShowModel = false;
        private static UserControl _wrapper;
        /// <summary>
        /// 主容器对象
        /// </summary>
        public static UserControl Wrapper
        {
            get
            {
                return _wrapper;
            }
            set
            {
                _wrapper = value;
            }
        }
        /// <summary>
        /// 根据Program信息加载应用程序
        /// </summary>
        /// <param name="program"></param>
        public static Window ShowProgram(Program program)
        {
            return ShowProgram(program, null);
        }
        public static Window ShowProgram(Program program, object param)
        {
            return ShowProgram(program, null, param);
        }
        public static Window ShowProgram(Program program, string asbName, object param, bool haveSource)
        {
            try
            {
                object content = SMT.SAAS.Utility.ApplicationHelper.CreateInstance(asbName, program.Type, program.InitParams);
                return ShowProgram(program.Title, program.TaskBarIconPath, program.ID, content, program.IsResizable, program.IsSysApp, param);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public static Window ShowProgram(Program program, Assembly source, object param)
        {
            object content = null;
            if (source == null)
            {
                //从当前程序集获取对象
                Type type = Type.GetType(program.Type);
                content = Activator.CreateInstance(type);
            }
            else
            {
                try
                {
                    content = SMT.SAAS.Utility.ApplicationHelper.CreateInstance(source, program.Type, program.InitParams);

                }
                catch (Exception ex)
                {
                    throw ex;
                    return null;
                }
                if (content == null)
                    return null;
            }

#if DEBUG
            //program.Title = program.Title + "--" + program.Type;
#endif
            return ShowProgram(program.Title, program.TaskBarIconPath, program.ID, content, program.IsResizable, program.IsSysApp, param);
        }
        public static Window ShowProgram(string Title, string IocPath, string windowID, object content, bool isResizeable, bool isSysApp, object param)
        {
            //当前应用程序窗口是否已经存在，如果存在则激活应用程序
            if (Windows.WindowsManager.WindowList.ContainsKey(windowID))
            {
                Windows.WindowsManager.ActiveWindow(Windows.WindowsManager.WindowList[windowID]);
                return null;
            }
            else
            {
                Windows.Window window = null;

                if (!isSysApp)
                    window = new Windows.Window(false, isResizeable);
                else
                    window = new Windows.Window(false, isResizeable);
                try
                {
                    window.scrollcontent.Content = content;
                }
                catch (Exception ex)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(ex.ToString());
                }
                window.Content = content;
                bool prload = true;
                //应用程序标题
                window.Caption = Title;
                if (IocPath == null || IocPath.Length == 0)
                    IocPath = "/SMT.SAAS.Controls.Toolkit.Windows;component/images/default.png";

                //应用程序任务栏以及窗口小图标地址
                window.IocPath = IocPath;
                window.ID = windowID;
                //初始化窗口位置/大小
                double width = !double.IsNaN(((Control)content).Width) ? ((Control)content).Width : ((Control)content).MinWidth;
                if ((width == 0.0) || double.IsNaN(width))
                {
                    width = 760.0;
                }
                double height = !double.IsNaN(((Control)content).Height) ? ((Control)content).Height : ((Control)content).MinHeight;
                if ((height == 0.0) || double.IsNaN(height))
                {
                    height = 550.0;
                }

                window.Height = height; window.Width = width;

                if (content is Control)
                {
                    //根据内容大小从新计算窗口大小 
                    ((Control)content).SizeChanged += (obj, args) =>
                        {
                            if (prload)
                            {
                                window.Height = args.NewSize.Height + 60;
                                window.Width = args.NewSize.Width + 30;
                                prload = false;
                            }
                        };
                }
                //加载应用程序窗口
                Windows.WindowsManager.ShowWindow(window, new Point(Wrapper.ActualWidth * 0.01, Wrapper.ActualHeight * 0.01), isResizeable, isSysApp, windowID);

                return window;
            }
        }
        /// <summary>
        /// MVC平台兼容silverlight弹出窗体特殊处理
        /// (窗体默认最大化)
        /// Author:傅意成
        /// Date:2012-07-12
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="IocPath"></param>
        /// <param name="windowID"></param>
        /// <param name="content"></param>
        /// <param name="isResizeable"></param>
        /// <param name="isSysApp"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Window ShowMvcProgram(string Title, string IocPath, string windowID, object content, bool isResizeable, bool isSysApp, object param)
        {
            //当前应用程序窗口是否已经存在，如果存在则激活应用程序
            if (Windows.WindowsManager.WindowList.ContainsKey(windowID))
            {
                Windows.WindowsManager.ActiveWindow(Windows.WindowsManager.WindowList[windowID]);
                return null;
            }
            else
            {
                Windows.Window window = null;
                if (!isSysApp)
                    window = new Windows.Window(false, isResizeable);
                else
                    window = new Windows.Window(false, isResizeable);
              
                window.scrollcontent.Content = content;
                window.Content = content;
                bool prload = true;
                //应用程序标题
                window.Caption = Title;
                if (IocPath == null || IocPath.Length == 0)
                    IocPath = "/SMT.SAAS.Controls.Toolkit.Windows;component/images/default.png";

                //应用程序任务栏以及窗口小图标地址
                window.IocPath = IocPath;
                window.ID = windowID;
                //初始化窗口位置/大小
                double width = !double.IsNaN(((Control)content).Width) ? ((Control)content).Width : ((Control)content).MinWidth;
                if ((width == 0.0) || double.IsNaN(width))
                {
                    width = 760.0;
                }
                double height = !double.IsNaN(((Control)content).Height) ? ((Control)content).Height : ((Control)content).MinHeight;
                if ((height == 0.0) || double.IsNaN(height))
                {
                    height = 550.0;
                }

                window.Height = height; window.Width = width;
                if (content is Control)
                {
                    //根据内容大小从新计算窗口大小 
                    ((Control)content).SizeChanged += (obj, args) =>
                    {
                        if (prload)
                        {
                            window.Height = args.NewSize.Height + 60;
                            window.Width = args.NewSize.Width + 30;
                            prload = false;
                        }
                    };
                }
                //加载应用程序窗口
                //Windows.WindowsManager.MaxWindow(window);
                Windows.WindowsManager.ShowMvcPlatWindow(window, new Point(Wrapper.ActualWidth * 0.01, Wrapper.ActualHeight * 0.01), isResizeable, isSysApp, windowID);
                return window;
            }
        }
    }
}
