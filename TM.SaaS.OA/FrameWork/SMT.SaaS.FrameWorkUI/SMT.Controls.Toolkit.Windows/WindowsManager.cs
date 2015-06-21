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
using System.Windows.Browser;
using System.Linq;
using System.Collections.Generic;
using SMT.SAAS.AnimationEngine;
namespace SMT.SAAS.Controls.Toolkit.Windows
{
    /// <summary>
    /// 窗口管理器，核心类
    /// </summary>
    public class WindowsManager
    {
        static WindowsManager()
        {
            desktop = null;
            taskBar = null;
        }
        public WindowsManager()
        {
        }
        #region WindowManager属性
        /// <summary>
        /// 桌面窗口集合Key=ID
        /// </summary>
        public static Dictionary<string, string> WindowList = new Dictionary<string, string>();
        /// <summary>
        /// 桌面容器
        /// </summary>
        private static Canvas desktop = null;
        /// <summary>
        /// 任务栏
        /// </summary>
        private static StackPanel taskBar = null;
        /// <summary>
        /// 桌面容器
        /// </summary>
        public static Canvas Desktop
        {
            get
            {
                return desktop;
            }
            set
            {
                desktop = value;
            }
        }
        /// <summary>
        /// 任务栏
        /// </summary>
        public static StackPanel TaskBar
        {
            get
            {
                return taskBar;
            }
            set
            {
                taskBar = value;
            }
        }

        /// <summary>
        /// 统计桌面窗口
        /// </summary>
        private static int countItem = 0;
        #endregion
        #region Window处理方法、激活窗口、获取最大Zindex、添加任务栏项
        public static void ClearAllWindows()
        {
            desktop.Children.Clear();
        }
        /// <summary>
        /// 根据应用程序窗口信息添加对于的人物栏按钮
        /// </summary>
        /// <param name="w"></param>
        private static void AddTaskBarItem(Window w)
        {
            if (taskBar != null)
            {
                double parentWidth = taskBar.ActualWidth;
                TaskBarItem item = new TaskBarItem();

                item.Name = "tb" + w.Name;
                item.Caption = w.Caption;
                item.IocPath = w.IocPath;
                item.InitWidth = 86;
                item.Clicked += new EventHandler(WindowsManager.taskBarItem_Clicked);
                taskBar.Children.Add(item);

                #region 控制任务栏按钮的缩放

                int itemcount = taskBar.Children.Count;
                if (itemcount > 1)
                {
                    ((TaskBarItem)taskBar.Children[itemcount - 2]).InitWidth = ((TaskBarItem)taskBar.Children[itemcount - 2]).ActualWidth;
                }

                double countItemWidth = 0;
                foreach (FrameworkElement child in taskBar.Children)
                {
                    countItemWidth += child.ActualWidth;
                }
                countItemWidth += item.InitWidth;
                if (countItemWidth >= parentWidth - (taskBar.Children.Count * 2))
                {
                    double itemWidth = (parentWidth - (taskBar.Children.Count)) / taskBar.Children.Count;
                    foreach (FrameworkElement child in taskBar.Children)
                    {
                        child.Width = itemWidth;
                    }
                }

                #endregion
            }
        }
        /// <summary>
        /// 激活窗口
        /// </summary>
        private static void ArrangeActiveWindow()
        {
            ArrangeActiveWindow(true);
        }
        public static void ArrangeActiveWindow(bool showIFrame)
        {
            Window activeWindow = null;
            activeWindow = GetActiveWindow(activeWindow);
        }
        private static Window GetActiveWindow(Window activeWindow)
        {

            int num = 0;
            foreach (Window window in desktop.Children.Where<UIElement>(delegate(UIElement i)
            {
                return (i is Window) && (((Window)i).Visibility == Visibility.Visible);
            }))
            {
                int zIndex = Canvas.GetZIndex(window);
                if (zIndex > num)
                {
                    num = zIndex;

                    activeWindow = window;
                }
            }
            return activeWindow;
        }
        /// <summary>
        /// 激活窗口
        /// </summary>
        /// <param name="WindowName"></param>
        public static void ActiveWindow(string WindowName)
        {
            ///根据窗口名称获取窗口在任务栏按钮的对象以及窗口对象
            TaskBarItem taskBarItem = (TaskBarItem)taskBar.Children.Where<UIElement>(delegate(UIElement i)
            {
                return ((i is TaskBarItem) && (((TaskBarItem)i).Name == "tb" + WindowName));
            }).FirstOrDefault<UIElement>();
            Window window = (Window)desktop.Children.Where<UIElement>(delegate(UIElement i)
            {
                return ((i is Window) && (((Window)i).Name == WindowName));
            }).FirstOrDefault<UIElement>();
            ///判断窗口是否为NULL
            if (window != null)
            {
                //如果窗口视图为不显示则将其从任务栏位置显示出来
                if (window.Visibility == Visibility.Collapsed)
                {
                    if (!window.IsHideWindow)
                    {
                        ///设置窗口显示，并使用动画将其呈现
                        window.Visibility = Visibility.Visible;
                        GeneralTransform gentransform = taskBarItem.TransformToVisual(desktop);
                        Point point = gentransform.Transform(new Point(0, 0));
                        Point to = new Point(Canvas.GetLeft(window), Canvas.GetTop(window));

                        Storyboard PrepareWindow = CommonAnimation.PrepareWindow
                               (window, window.TimeOffset, point, window.TaskBarPoint, new Size(2, 2), window.HideWindowSize, 0, 1, true);
                        PrepareWindow.Begin();
                        PrepareWindow.Completed += (ob, arg) => { window.IsHideWindow = true; };
                    }
                }
                else
                {
                    Canvas.SetZIndex(window, Window.currentZIndex++);
                }
                window.Focus();
            }

        }
        /// <summary>
        /// 获取顶层窗口的Zindex
        /// </summary>
        /// <returns></returns>
        public static int GetMaxZindex()
        {
            Window activeWindow = null;
            activeWindow = GetActiveWindow(activeWindow);
            if (activeWindow != null)
            {
                return Canvas.GetZIndex(activeWindow);
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 显示桌面，将desktop中的所有应用程序窗口视图设置为不可见
        /// </summary>
        public static void ShowDesktop()
        {
            foreach (Window window in desktop.Children.Where<UIElement>(delegate(UIElement i)
            {
                return (i is Window) && (((Window)i).Visibility == Visibility.Visible);
            }))
            {
                window.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
        #region 添加/显示窗口
        public static Window ShowModal(string caption, string text)
        {
            return null;
        }
        /// <summary>
        /// 显示一个模式对话框，不可拖动，不可修改大小
        /// </summary>
        public static Window ShowModal(FrameworkElement content, string caption, Point point, Size size, string IocPath)
        {
            Window element = new Window(false, false);

            element.Caption = caption;
            element.IocPath = IocPath;
            element.scrollcontent.Content = content;//设置模式对话框里面要提示的内容或应用程序
            element.ResizeEnabled = false;
            element.DraggingEnabled = false;
            element.Name = Guid.NewGuid().ToString();
            element.MaxHeight = desktop.ActualHeight;
            element.MaxWidth = desktop.ActualWidth;
            element.Width = size.Width;
            element.Height = size.Height;
            Canvas.SetLeft(element, point.X);
            Canvas.SetTop(element, point.Y);
            desktop.Children.Add(element);

            //Point pos = new Point(point.X, point.Y);
            //启动窗口动画
            //OpenWin(pos);
            return element;
        }
        public static Window GetMessageWindow(FrameworkElement content, string caption, Point point, Size size, string IocPath)
        {
            Window element = new Window(false, false);

            element.Caption = caption;
            element.IocPath = IocPath;
            element.scrollcontent.Content = content;//设置模式对话框里面要提示的内容或应用程序
            element.ResizeEnabled = false;
            element.DraggingEnabled = false;
            element.Name = Guid.NewGuid().ToString();
            element.MaxHeight = desktop.ActualHeight;
            element.MaxWidth = desktop.ActualWidth;
            element.Width = size.Width;
            element.Height = size.Height;
            Canvas.SetLeft(element, point.X);
            Canvas.SetTop(element, point.Y);

            return element;
        }
       
        public static void ShowWindow(Window window, Point location)
        {
            ShowWindow(window, location, false, false);
        }
      
        public static Window ShowWindow(FrameworkElement content, string caption, Point location)
        {
            Window window = new Window();
            window.Caption = caption;
            window.scrollcontent.Content = content;
            ShowWindow(window, location);
            return window;
        }
        public static void ShowWindow(Window window, Point location, bool isResizable, bool isSysApp)
        {
            ShowWindow(window, location, false, false,Guid.NewGuid().ToString());
        }
        public static void ShowWindow(Window window, Point location, bool isResizable, bool isSysApp,string windowID)
        {
            try
            {
                if (desktop.Children.Count < 5)
                {
                    //为应用程序窗口注册事件
                    window.Name = Guid.NewGuid().ToString();
                    window.Closed += new EventHandler(WindowsManager.window_Closed);
                    window.Maximized += new EventHandler(WindowsManager.window_Maximized);
                    window.Normalized += new EventHandler(WindowsManager.window_Normalized);
                    window.Minimized += new EventHandler(WindowsManager.window_Minimized);
                    window.Dragged += new EventHandler(WindowsManager.window_Dragged);
                    window.SetZIndex += new EventHandler(WindowsManager.window_SetZIndex);
                    window.MaxHeight = desktop.ActualHeight;
                    window.MaxWidth = desktop.ActualWidth;
                    window.ResizeEnabled = isResizable;
                    

                    //修正窗口显示位置
                    double left = location.X + countItem * 25;
                    double top = location.Y;// +countItem * 25;
                    //if (top >= desktop.ActualHeight - window.Height - 10)
                    //    countItem = -1;
                    if (left >= desktop.ActualWidth - window.Width - 30)
                        countItem = -1;
                    //控制位置
                    Canvas.SetLeft(window, left);
                    Canvas.SetTop(window, top);

                    desktop.Children.Add(window);

                    //应用程序窗口层次
                    countItem++;
                    Point pos = new Point(left, top);
                    //启动窗口动画
                    OpenWin(pos);

                    //为应用程序添加任务栏按钮
                    //if (isSysApp)
                    //    AddTaskBarItem(window);

                    Windows.WindowsManager.WindowList.Add(windowID, window.Name);
                }
                else
                {
                    MessageBox.Show("对不起,窗口不能打开超过5个，请关闭其他窗口！");
                }

            }
            catch (Exception e)
            {
                //window = null;
                MessageBox.Show(e.Message);
                // throw e;
            }
        }
        /// <summary>
        /// 显示窗口
        /// 兼容MVC 平台弹出窗口
        /// Author:傅意成
        /// Date:2012-07-12
        /// <param name="window"></param>
        /// <param name="location"></param>
        /// <param name="isResizable"></param>
        /// <param name="isSysApp"></param>
        /// <param name="windowID"></param>
        public static void ShowMvcPlatWindow(Window window, Point location, bool isResizable, bool isSysApp, string windowID)
        {
            try
            {
                if (desktop.Children.Count < 5)
                {
                    //为应用程序窗口注册事件
                    window.Name = Guid.NewGuid().ToString();
                    window.Closed += new EventHandler(WindowsManager.window_Closed);
                    window.Maximized += new EventHandler(WindowsManager.window_Maximized);
                    window.Normalized += new EventHandler(WindowsManager.window_Normalized);
                    window.Minimized += new EventHandler(WindowsManager.window_Minimized);
                    window.Dragged += new EventHandler(WindowsManager.window_Dragged);
                    window.SetZIndex += new EventHandler(WindowsManager.window_SetZIndex);
                    window.MaxHeight = desktop.ActualHeight;
                    window.MaxWidth = desktop.ActualWidth;
                    window.ResizeEnabled = isResizable;
                    window.closeButton.Visibility = Visibility.Collapsed;
                    window.AllMaximized = true;
                    //修正窗口显示位置
                    double left = location.X + countItem * 25;
                    double top = location.Y;// +countItem * 25;
                    //if (top >= desktop.ActualHeight - window.Height - 10)
                    //    countItem = -1;
                    if (left >= desktop.ActualWidth - window.Width - 30)
                        countItem = -1;
                    //控制位置
                    Canvas.SetLeft(window, left);
                    Canvas.SetTop(window, top);

                    desktop.Children.Add(window);

                    //应用程序窗口层次
                    countItem++;
                    Point pos = new Point(left, top);
                    //启动窗口动画
                    OpenWin(pos);

                    //为应用程序添加任务栏按钮
                    //if (isSysApp)
                    //    AddTaskBarItem(window);

                    Windows.WindowsManager.WindowList.Add(windowID, window.Name);
                }
                else
                {
                    MessageBox.Show("对不起,窗口不能打开超过5个，请关闭其他窗口！");
                }

            }
            catch (Exception e)
            {
                //window = null;
                MessageBox.Show(e.Message);
                // throw e;
            }
        }

        /// <summary>
        /// 窗口打开动画
        /// </summary>
        static void OpenWin(Point pos)
        {
            //switch (DataTransform.UserWinOpenAnim)
            //{
            //    case 0:
            //        DataTransform.MainPage.endX.Value = pos.X;
            //        DataTransform.MainPage.endY.Value = pos.Y;
            //        DataTransform.MainPage.OpenWindow.Begin();
            //        break;
            //    case 1:
            //        DataTransform.MainPage.endX_1.Value = pos.X;
            //        DataTransform.MainPage.endY_1.Value = pos.Y;
            //        DataTransform.MainPage.OpenWindow_Plus_1.Begin();
            //        break;
            //    case 2:
            //        DataTransform.MainPage.endX_2.Value = pos.X;
            //        DataTransform.MainPage.endY_2.Value = pos.Y;
            //        DataTransform.MainPage.OpenWindow_Plus_2.Begin();
            //        break;
            //}
        }
        #endregion
        #region 窗口/任务栏事件方法。窗口最大化、最小化、拖动、设置Zindex
        private static void taskBarItem_Clicked(object sender, EventArgs e)
        {
            TaskBarItem taskBarItem = (TaskBarItem)sender;

            Window window = (Window)desktop.Children.Where<UIElement>(delegate(UIElement i)
            {
                return ((i is Window) && (((Window)i).Name == taskBarItem.Name.Substring(2)));
            }).FirstOrDefault<UIElement>();
            if (window != null)
            {
                if (window.Visibility == Visibility.Collapsed)
                {
                    if (!window.IsHideWindow)
                    {
                        window.Visibility = Visibility.Visible;
                        GeneralTransform gentransform = taskBarItem.TransformToVisual(desktop);
                        Point point = gentransform.Transform(new Point(0, 0));
                        Point to = new Point(Canvas.GetLeft(window), Canvas.GetTop(window));

                        Storyboard PrepareWindow = CommonAnimation.PrepareWindow
                               (window, window.TimeOffset, point, window.TaskBarPoint, new Size(2, 2), window.HideWindowSize, 0, 1, true);
                        PrepareWindow.Begin();
                        PrepareWindow.Completed += (ob, arg) => { window.IsHideWindow = true; };
                    }
                }
                else
                {
                    if (window.IsHideWindow)
                    {
                        GeneralTransform gentransform = taskBarItem.TransformToVisual(desktop);
                        Point point = gentransform.Transform(new Point(0, 0));
                        window.TaskBarPoint = new Point(Canvas.GetLeft(window), Canvas.GetTop(window));
                        window.HideWindowSize = new Size(window.Width, window.Height);


                        Storyboard PrepareWindow = CommonAnimation.PrepareWindow
                              (window, window.TimeOffset, window.TaskBarPoint, point, window.HideWindowSize, new Size(2, 2), 1, 0, true);
                        PrepareWindow.Begin();
                        PrepareWindow.Completed += (o, arg) =>
                        { window.Visibility = Visibility.Collapsed; window.IsHideWindow = false; };
                        ArrangeActiveWindow();
                    }
                }
                window.Focus();
            }
        }
        private static void window_Closed(object sender, EventArgs e)
        {
            Func<UIElement, bool> predicate = null;
            Window w = (Window)sender;
            w.HideWindow.Begin();
            WindowList.Remove(w.ID);
            desktop.Children.Remove(w);


            if (desktop.Children.Count == 0)
                countItem = 0;

            if (taskBar != null)
            {
                if (predicate == null)
                {
                    predicate = delegate(UIElement i)
                    {
                        return (i is TaskBarItem) && (((TaskBarItem)i).Name == ("tb" + w.Name));
                    };
                }
                TaskBarItem item = (TaskBarItem)taskBar.Children.Where<UIElement>(predicate).FirstOrDefault<UIElement>();
                if (item != null)
                {
                    int count = taskBar.Children.Count;
                    taskBar.Children.Remove(item);

                    double parentWidth = taskBar.ActualWidth - taskBar.Children.Count - 78;
                    double itemWidth = parentWidth / taskBar.Children.Count;

                    if (itemWidth < item.InitWidth)
                    {
                        foreach (FrameworkElement child in taskBar.Children)
                        {
                            child.Width = itemWidth;
                        }
                    }
                    else if (itemWidth > item.InitWidth)
                    {
                        foreach (FrameworkElement child in taskBar.Children)
                        {
                            child.Width = ((TaskBarItem)child).InitWidth;
                        }
                    }
                }
            }
            ArrangeActiveWindow();
            GC.Collect();
        }
        private static void window_Dragged(object sender, EventArgs e)
        {
            Window window = (Window)sender;
        }
        private static void window_Maximized(object sender, EventArgs e)
        {
            MaxWindow(sender);
        }

        public static void MaxWindow(object sender)
        {
            Window element = (Window)sender;
            Point from = new Point(Canvas.GetLeft(element), Canvas.GetTop(element));
            Size fromSize = new Size(element.Width, element.Height);
            Size toSize = new Size(desktop.ActualWidth, desktop.ActualHeight);
            Storyboard PrepareWindow = CommonAnimation.PrepareWindow
                   (element, element.TimeOffset, from, new Point(0, 0), fromSize, toSize, 0, 1, false);
            PrepareWindow.Begin();
        }
        private static void window_Minimized(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            TaskBarItem item = (TaskBarItem)taskBar.Children.Where<UIElement>(delegate(UIElement i)
            {
                return ((i is TaskBarItem) && (((TaskBarItem)i).Name == "tb" + window.Name));
            }).FirstOrDefault<UIElement>();
            if (window.IsHideWindow)
            {
                GeneralTransform gentransform = item.TransformToVisual(desktop);
                Point point = gentransform.Transform(new Point(0, 0));
                window.TaskBarPoint = new Point(Canvas.GetLeft(window), Canvas.GetTop(window));
                window.HideWindowSize = new Size(window.Width, window.Height);

                Storyboard PrepareWindow = CommonAnimation.PrepareWindow
                      (window, window.TimeOffset, window.TaskBarPoint, point, window.HideWindowSize, new Size(2, 2), 1, 0, true);
                PrepareWindow.Begin();
                PrepareWindow.Completed += (o, arg) =>
                { window.Visibility = Visibility.Collapsed; window.IsHideWindow = false; };
                ArrangeActiveWindow();
            }

        }
        private static void window_Normalized(object sender, EventArgs e)
        {

        }
        private static void window_SetZIndex(object sender, EventArgs e)
        {
            Func<UIElement, bool> predicate = null;
            Window w = (Window)sender;
            if (w.Visibility == Visibility.Visible)
            {
                if (predicate == null)
                {
                    predicate = delegate(UIElement i)
                    {
                        return (((i is Window) && (((Window)i).Visibility == Visibility.Visible)) && (((Window)i).Caption == "Internet Explorer")) && (((Window)i).Name != w.Name);
                    };
                }
            }
        }
        #endregion
    }
}
