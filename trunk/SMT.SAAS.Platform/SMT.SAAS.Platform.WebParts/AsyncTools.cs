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
using System.Threading;

namespace SMT.SAAS.Platform.WebParts
{
    /// <summary>
    /// 检测上下文环境是否加载
    /// </summary>
    public class AsyncTools
    {
        private static bool bIsLoaded = false;
        SMT.SAAS.Platform.ViewModel.SplashScreen.SplashScreenViewModel _splashScreen = new ViewModel.SplashScreen.SplashScreenViewModel();
        public event EventHandler InitAsyncCompleted;

        /// <summary>
        /// 检测上下文环境是否加载
        /// </summary>
        private void Run()
        {
            if (ViewModel.Context.Managed != null)
            {
                if (ViewModel.Context.Managed.Catalog != null)
                {
                    if (ViewModel.Context.Managed.Catalog.Count > 0)
                    {
                        bIsLoaded = true;
                    }
                }
            }

            if (Application.Current.Resources["SYS_PostInfo"] == null)
            {
                InitOrg();
            }
            else
            {
                if (!bIsLoaded)
                {
                    InitMenu();
                }
                else
                {
                    //Deployment.Current.Dispatcher.BeginInvoke(delegate
                    //{
                    if (InitAsyncCompleted != null)
                    {
                        this.InitAsyncCompleted(this, EventArgs.Empty);
                    }
                    //}); 
                }
            }
        }

        /// <summary>
        /// 加载上下文环境
        /// </summary>
        public void InitMenu()
        {
            _splashScreen.InitCompleted += new EventHandler(_splashScreen_InitCompleted);
            _splashScreen.GetModules();
        }

        /// <summary>
        /// 加载上下文环境
        /// </summary>
        public void InitOrg()
        {
            _splashScreen.LoadCompanyInfo();
            _splashScreen.OnGetOrgCompleted += new EventHandler(_splashScreen_OnGetOrgCompleted);
        }

        void _splashScreen_OnGetOrgCompleted(object sender, EventArgs e)
        {
            if (!bIsLoaded)
            {
                InitMenu();
            }
            else
            {
                //Deployment.Current.Dispatcher.BeginInvoke(delegate
                //{
                if (InitAsyncCompleted != null)
                {
                    this.InitAsyncCompleted(this, EventArgs.Empty);
                }
                //}); 
            }
        }

        /// <summary>
        /// 加载上下文环境完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _splashScreen_InitCompleted(object sender, EventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                if (InitAsyncCompleted != null)
                {
                    this.InitAsyncCompleted(this, EventArgs.Empty);
                }
                bIsLoaded = true;
            }); 
        }

        public void BeginRun()
        {
            try
            {
                //ThreadPool.QueueUserWorkItem(delegate(object o)
                //    {
                        Run();
                    //});
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
