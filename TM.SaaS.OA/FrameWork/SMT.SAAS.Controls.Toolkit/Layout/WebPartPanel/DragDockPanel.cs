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
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace SMT.SAAS.Controls.Toolkit
{
    public class DragDockPanel : DraggablePanel
    {
        private const string ElementMaximizeToggleButton = "MaximizeToggleButton";
        private bool ignoreUnCheckedEvent;
        private int panelIndex;
        private PanelState panelState;
        public event EventHandler Maximized;
        public event EventHandler Minimized;
        public event EventHandler Restored;
        public DragDockPanel()
        {
            base.DefaultStyleKey = typeof(DragDockPanel);

        }

        private void MaximizeToggle_Checked(object sender, RoutedEventArgs e)
        {
            int currentZIndex = DraggablePanel.CurrentZIndex;
            DraggablePanel.CurrentZIndex = currentZIndex + 1;
            Canvas.SetZIndex(this, currentZIndex);
            this.ignoreUnCheckedEvent = false;
            if (this.Maximized != null)
            {
                this.Maximized(this, e);
            }
        }

        private void MaximizeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!this.ignoreUnCheckedEvent)
            {
                this.PanelState = PanelState.Restored;
                if (this.Restored != null)
                {
                    this.Restored(this, e);
                }
            }
            else
            {
                this.ignoreUnCheckedEvent = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ToggleButton templateChild = base.GetTemplateChild("MaximizeToggleButton") as ToggleButton;
            if (templateChild != null)
            {
                templateChild.Checked += new RoutedEventHandler(this.MaximizeToggle_Checked);
                templateChild.Unchecked += new RoutedEventHandler(this.MaximizeToggle_Unchecked);
                if (this.PanelState == PanelState.Restored)
                {
                    this.ignoreUnCheckedEvent = true;
                    templateChild.IsChecked = false;
                }
                else if (this.PanelState == PanelState.Maximized)
                {
                    templateChild.IsChecked = true;
                }
            }
        }

        public override void UpdatePosition(Point pos)
        {
            Canvas.SetLeft(this, pos.X);
            Canvas.SetTop(this, pos.Y);
        }

        public int PanelIndex
        {
            get
            {
                return this.panelIndex;
            }
            set
            {
                this.panelIndex = value;
            }
        }

        [Description("设置面板是否是最大化."), Category("面板属性")]
        public PanelState PanelState
        {
            get
            {
                return this.panelState;
            }
            set
            {
                this.panelState = value;
                ToggleButton templateChild = base.GetTemplateChild("MaximizeToggleButton") as ToggleButton;
                if (this.panelState == PanelState.Restored)
                {
                    this.ignoreUnCheckedEvent = true;
                    if (templateChild != null)
                    {
                        templateChild.IsChecked = false;
                    }
                }
                else if (this.panelState == PanelState.Maximized)
                {
                    if (templateChild != null)
                    {
                        templateChild.IsChecked = true;
                    }
                }
                else if ((this.panelState == PanelState.Minimized) && (this.Minimized != null))
                {
                    this.Minimized(this, EventArgs.Empty);
                }
            }
        }
    }

}
