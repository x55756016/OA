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
using System.Windows.Controls.Primitives;

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public abstract class Dialog
    {
        private Point _location;
        private bool _isShowing;
        private Popup _popup;
        private Grid _grid;
        private Canvas _canvas;
        private FrameworkElement _content;

        public void Show(Point location)
        {
            if (_isShowing)
                throw new InvalidOperationException();

            _isShowing = true;
            _location = location;
            EnsurePopup();
            _popup.IsOpen = true;
        }

        public void Close()
        {
            _isShowing = false;

            if (_popup != null)
            {
                _popup.IsOpen = false;
            }
        }

        protected abstract FrameworkElement GetContent();

        protected virtual void OnClickOutside() { }

        private void EnsurePopup()
        {
            if (_popup != null)
                return;

            _popup = new Popup();
            _grid = new Grid();

            _popup.Child = _grid;

            _canvas = new Canvas();

            _canvas.MouseLeftButtonDown += (sender, args) => { OnClickOutside(); };
            _canvas.MouseRightButtonDown += (sender, args) => { args.Handled = true; OnClickOutside(); };

            _canvas.Background = new SolidColorBrush(Colors.Transparent);

            _grid.Children.Add(_canvas);

            _content = GetContent();

            _content.HorizontalAlignment = HorizontalAlignment.Left;
            _content.VerticalAlignment = VerticalAlignment.Top;
            _content.Margin = new Thickness(_location.X, _location.Y, 0, 0);


            _grid.Children.Add(_content);

            UpdateSize();
        }

        private void OnPluginSizeChanged(object sender, EventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            _grid.Width = Application.Current.Host.Content.ActualWidth;
            _grid.Height = Application.Current.Host.Content.ActualHeight;

            if (_canvas != null)
            {
                _canvas.Width = _grid.Width;
                _canvas.Height = _grid.Height;
            }
        }
    }
}