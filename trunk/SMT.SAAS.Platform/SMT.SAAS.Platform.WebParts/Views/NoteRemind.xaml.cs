using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SMT.Saas.Tools.EngineWS;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class NoteRemind : UserControl, IWebpart
    {
        private DispatcherTimer _refdateTimer;
        private EngineWcfGlobalFunctionClient _client;
        private static string _state = "open";

        public NoteRemind()
        {
            InitializeComponent();
            Initialize();
        }

        public int Top
        {
            get;
            set;
        }

        public int RefDate
        {
            get;
            set;
        }

        public string Titel
        {
            get;
            set;
        }

        public void Initialize()
        {
            Top = 50;
            RefDate = 30;
            Titel = "消息提醒";
            _refdateTimer = new DispatcherTimer();
            _refdateTimer.Interval = new TimeSpan(0, 0, 60);
            _refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
            _client = new EngineWcfGlobalFunctionClient();
            this.Loaded += new RoutedEventHandler(WFEngineNotes_Loaded);
            _client.EngineNotesCompleted += new EventHandler<EngineNotesCompletedEventArgs>(_client_EngineNotesCompleted);

        }

        void _client_EngineNotesCompleted(object sender, EngineNotesCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        List<T_FLOW_ENGINENOTES> result = new List<T_FLOW_ENGINENOTES>();
                        foreach (var item in e.Result)
                        {
                            T_FLOW_ENGINENOTES temp = item;
                            temp.CREATEDATE = item.CREATEDATE + " " + item.CREATETIME;
                            result.Add(item);
                        }
                        Results.ItemsSource = null;
                        Results.ItemsSource = result;

                    }
                }
            }
            finally
            {
                _refdateTimer.Tick -= new EventHandler(_refdateTimer_Tick);
                _refdateTimer.Tick += new EventHandler(_refdateTimer_Tick);
                _refdateTimer.Start();
            }
        }

        void WFEngineNotes_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDate();
        }

        void _refdateTimer_Tick(object sender, EventArgs e)
        {
            _refdateTimer.Stop();
            LoadDate();
        }

        public void LoadDate()
        {
            _client.EngineNotesAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, _state, 10);
        }

        public void Cleanup()
        {
            _refdateTimer.Tick -= new EventHandler(_refdateTimer_Tick);
            _refdateTimer = null;
            _client = null;
        }


        public void Stop()
        {
            if (_refdateTimer != null)
                _refdateTimer.Stop();
        }

        public void Star()
        {
            if (_refdateTimer != null)
                _refdateTimer.Start();
        }
    }
}
