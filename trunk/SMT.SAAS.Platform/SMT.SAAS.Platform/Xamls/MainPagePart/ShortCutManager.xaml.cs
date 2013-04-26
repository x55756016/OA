using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SMT.SAAS.Platform.ViewModel.MainPage;
using System.Windows.Data;
using System.Windows.Threading;
using System.Diagnostics;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class ShortCutManager : UserControl
    {
        private ShortCutManagerViewModel _currentContext = null;
        private int _timeer = 5;
        private bool _isSubmit = true;
        private bool _islock = true;
        private DispatcherTimer _submitTimer;
        public event EventHandler<OnShortCutClickArgs> OnShortCutClick;
        public event EventHandler<System.Windows.Input.MouseButtonEventArgs> ShortCutMouseDown;
        public event EventHandler<System.Windows.Input.MouseEventArgs> ShortCutMouseMove;

        public ShortCutManager()
        {
            InitializeComponent();
            _submitTimer = new DispatcherTimer();
            _submitTimer.Interval = new TimeSpan(0, 0, _timeer);
            _submitTimer.Tick += new EventHandler(_submitTimer_Tick);
            _currentContext = new ShortCutManagerViewModel();

            this.DataContext = _currentContext;
        }

        private void _submitTimer_Tick(object sender, EventArgs e)
        {
            if (_isSubmit)
            {
                Debug.WriteLine("_isSubmit = true ");
                _currentContext.Submit();
                _submitTimer.Stop();
                _islock = false;
            }
            if (_islock)
            {
                _submitTimer.Start();
                _isSubmit = true;
            }
        }

        public void AddItem(ShortCutViewModel item)
        {
            Debug.WriteLine("AddItem  _isSubmit = false ");

            var existsitem = _currentContext.Item.FirstOrDefault(i => i.ModuleID == item.ModuleID);

            if (existsitem == null)
                _currentContext.Item.Add(item);

            _isSubmit = false;
            _islock = true;
            _submitTimer.Start();
        }

        public void RemoveItem(string shortCutID)
        {
            _currentContext.RemoveItem(shortCutID);
        }

        private void ShortCut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var shortcut = (sender as ShortCut).DataContext as ShortCutViewModel;

            if (OnShortCutClick != null)
                OnShortCutClick(sender, new OnShortCutClickArgs() { ModuleID = shortcut.ModuleID });
        }

        private void ShortCut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ShortCutMouseDown != null)
                ShortCutMouseDown(sender, e);
        }

        private void ShortCut_MouseMove(object sender, MouseEventArgs e)
        {
            if (ShortCutMouseMove != null)
                ShortCutMouseMove(sender, e);
        }
    }

    public class OnShortCutClickArgs : EventArgs
    {
        public string ModuleID { get; set; }
    }
}
