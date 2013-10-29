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
using Wintellect.Sterling;
using System.ComponentModel;
using System.Diagnostics;
using SMT.SaaS.LocalData.Database;

namespace SMT.SaaS.LocalData
{
    /// <summary>
    /// 本地数据库服务类
    /// </summary>
    public sealed class SterlingService : IApplicationService, IApplicationLifetimeAware, IDisposable
    {
        /// <summary>
        /// 本地数据库引擎
        /// </summary>
        private SterlingEngine sengine;

        /// <summary>
        /// 本地数据库服务
        /// </summary>
        public static SterlingService Current { get; private set; }

        /// <summary>
        /// 本地数据库接口
        /// </summary>
        public ISterlingDatabaseInstance Database { get; private set; }

        /// <summary>
        /// 本地数据库日志类
        /// </summary>
        private SterlingDefaultLogger sdlogger;

        /// <summary>
        /// 启动本地数据库服务
        /// </summary>
        /// <param name="context">应用程序池</param>
        public void StartService(ApplicationServiceContext context)
        {
            if (DesignerProperties.IsInDesignTool)
            { 
                return; 
            }
            sengine = new SterlingEngine();
            Current = this;
        }

        /// <summary>
        /// 服务停止
        /// </summary>
        public void StopService()
        { 
            return; 
        }

        /// <summary>
        /// 服务启动
        /// </summary>
        public void Starting()
        {
            if (DesignerProperties.IsInDesignTool)
            {
                return;
            }
            if (Debugger.IsAttached)
            {
                sdlogger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
            }

            sengine.Activate();

            Database = sengine.SterlingDatabase.RegisterDatabase<SMTLacalDB>();
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Started()
        { 
            return; 
        }

        /// <summary>
        /// 注销
        /// </summary>
        public void Exiting()
        {
            if (DesignerProperties.IsInDesignTool)
            {
                return;
            }
            if (Debugger.IsAttached && sdlogger != null)
            {
                sdlogger.Detach();
            }
        }

        /// <summary>
        /// 注销完成
        /// </summary>
        public void Exited()
        {
            Dispose();
            sengine = null;
            return;
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Dispose()
        {
            if (sengine != null)
            {
                sengine.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
