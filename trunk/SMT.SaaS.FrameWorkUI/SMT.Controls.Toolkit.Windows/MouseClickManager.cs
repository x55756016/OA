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

namespace SMT.SAAS.Controls.Toolkit.Windows
{
    public class MouseClickManager
    {
        #region Fields
        private bool _clicked;
        private int _timeout;
        private UIElement _UIElement;
        #endregion
        #region Events
        public event MouseButtonEventHandler Click;
        public event MouseButtonEventHandler DoubleClick;
        #endregion
        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler click = this.Click;
            if (click != null)
            {
                if(e!=null)
                    this.UIElement.Dispatcher.BeginInvoke(click, new object[] { sender, e });
            }
        }
        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MouseButtonEventHandler doubleClick = this.DoubleClick;
            if (doubleClick != null)
            {
                    doubleClick(sender, e);
            }
        }

        public UIElement UIElement
        {
            get
            {
                return this._UIElement;
            }
            set
            {
                this._UIElement = value;
            }
        }
        private bool Clicked
        {
            get
            {
                return this._clicked;
            }
            set
            {
                this._clicked = value;
            }
        }
        public int Timeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                this._timeout = value;
            }
        }

        public void HandleClick(object sender, MouseButtonEventArgs e)
        {
            lock (this)
            {
                if (this.Clicked)
                {
                    this.Clicked = false;
                    this.OnDoubleClick(sender, e);
                }
                else
                {
                    this.Clicked = true;
                    ParameterizedThreadStart start = new ParameterizedThreadStart(this.ResetThread);
                    new Thread(start).Start(e);
                }
            }
        }
        private void ResetThread(object state)
        {
            Thread.Sleep(this.Timeout);
            lock (this)
            {
                if (this.Clicked)
                {
                    this.Clicked = false;
                    this.OnClick(this, (MouseButtonEventArgs)state);
                }
            }
        }
        public MouseClickManager(UIElement uiElement, int timeout)
        {
            this.Clicked = false;
            this.UIElement = uiElement;
            this.Timeout = timeout;
        }


    }
}
