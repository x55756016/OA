using System;
using System.Collections.Generic;

namespace System.Windows.Controls
{
    [TemplatePart(Name = WindowsContainer.WindowsContainerGridName, Type = typeof(Grid))]
    [TemplatePart(Name = WindowsContainer.ModalContainerGridName, Type = typeof(Grid))]
    [TemplatePart(Name = WindowsContainer.ContentControlName, Type = typeof(ContentControl))]
    public class WindowsContainer : ContentControl
    {
        # region Declarations

        private bool _IsTemplateLoaded;

        private Window _Modal;

        private const string WindowsContainerGridName = "WindowsContainerGrid";
        private Grid _WindowsContainerGrid;

        private const string ModalContainerGridName = "ModalContainerGrid";
        private Grid _ModalContainerGrid;

        private const string ContentControlName = "ContentControl";
        private ContentControl _ContentControl;

        # endregion

        # region Constructor

        public WindowsContainer()
        {
            this.DefaultStyleKey = typeof(WindowsContainer);
        }

        # endregion

        # region PreTemplateWindows

        private List<Window> _PreTemplateWindows;

        public List<Window> PreTemplateWindows
        {
            get
            {
                if (this._PreTemplateWindows.IsNull())
                {
                    this._PreTemplateWindows = new List<Window>();
                }
                return this._PreTemplateWindows;
            }
        }

        # endregion

        # region OnApplyTemplate

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._WindowsContainerGrid = this.GetTemplateChild(WindowsContainer.WindowsContainerGridName) as Grid;
            this._ModalContainerGrid = this.GetTemplateChild(WindowsContainer.ModalContainerGridName) as Grid;
            this._ContentControl = this.GetTemplateChild(WindowsContainer.ContentControlName) as ContentControl;

            this._PreTemplateWindows.TryDo(p =>
            {
                foreach (var window in p)
                {
                    this._WindowsContainerGrid.TryDo(c => c.Children.Add(window.InternalContainerGrid));
                }
            });

            this._PreTemplateWindows = null;

            this._Modal.TryDo(m =>
            {
                this._ModalContainerGrid.TryDo(c => c.Children.Add(m.InternalContainerGrid));
                this._ContentControl.TryDo(c => c.IsEnabled = false);
            });

            this._IsTemplateLoaded = true;
        }

        # endregion

        # region ShowWindow

        public void ShowWindow(Window window)
        {
            if (window.DialogMode == DialogMode.ApplicationModal)
            {
                window.Show();
            }
            else if (window.DialogMode == DialogMode.Modal)
            {
                this.ShowDialog(window);
            }
            else
            {
                if (!this._IsTemplateLoaded)
                {
                    this.PreTemplateWindows.Add(window);
                }
                else
                {
                    this._WindowsContainerGrid.TryDo(c => c.Children.Add(window.InternalContainerGrid));
                }
            }
        }
        public void ShowDialog(Window window)
        {
            window.DialogMode = DialogMode.Modal;

            if (this._Modal.IsNotNull())
            {
                throw new InvalidOperationException(
                    "There is already a modal Window been shown in this container.");
            }

            this._Modal = window;

            if (this._IsTemplateLoaded)
            {
                this._ModalContainerGrid.TryDo(c => c.Children.Add(window.InternalContainerGrid));
                this._ContentControl.TryDo(c => c.IsEnabled = false);
            }
        }

        # endregion

        # region CloseWindow

        public void CloseWindow(Window window)
        {
            if (window.DialogMode == DialogMode.ApplicationModal)
            {
                window.Close();
            }
            else if (window.DialogMode == DialogMode.Modal)
            {
                var modal = this._Modal;
                if (modal.IsNull() && modal != window)
                {
                    throw new InvalidOperationException(
                        "This Window is not been shown in this container.");
                }
                this.CloseDialog();
            }
            else
            {
                this._WindowsContainerGrid.TryDo(c => c.Children.Remove(window.InternalContainerGrid));
            }
        }
        public void CloseDialog()
        {
            if (this._Modal.IsNull())
            {
                throw new InvalidOperationException(
                    "There are no current modal Window been shown in this container.");
            }

            this._ModalContainerGrid.TryDo(c => c.Children.Clear());
            this._ContentControl.TryDo(c => c.IsEnabled = true);
            this._Modal = null;
        }

        # endregion
    }
}