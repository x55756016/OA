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
    public sealed class SterlingService : IApplicationService, IApplicationLifetimeAware, IDisposable
    {
        private SterlingEngine _engine;
        public static SterlingService Current { get; private set; }
        public ISterlingDatabaseInstance Database { get; private set; }
        private SterlingDefaultLogger _logger;
        public void StartService(ApplicationServiceContext context)
        {
            if (DesignerProperties.IsInDesignTool) return;
            _engine = new SterlingEngine();
            Current = this;
        }

        public void StopService()
        { return; }

        public void Starting()
        {
            if (DesignerProperties.IsInDesignTool) return;
             if (Debugger.IsAttached)
        {
          _logger = new SterlingDefaultLogger(SterlingLogLevel.Verbose);
        }

            _engine.Activate();

            Database = _engine.SterlingDatabase.RegisterDatabase<SMTLacalDB>();            
        }

        public void Started()
        { return; }

        public void Exiting()
        {
            if (DesignerProperties.IsInDesignTool) return;
            if (Debugger.IsAttached && _logger != null)
            {
                _logger.Detach();
            }
        }

        public void Exited()
        {
            Dispose();
            _engine = null;
            return;
        }

        public void Dispose()
        {
            if (_engine != null)
            {
                _engine.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }

}
