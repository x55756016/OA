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

namespace SMT.SAAS.Controls.Toolkit
{
    public delegate void DragEventHander(object sender, DragEventArgs args);
    public class DraggablePanel : AnimatedHeaderedContentControl
    {
        private static int currentZIndex = 1;
        private bool dragging;
        public static readonly DependencyProperty DraggingEnabledProperty = DependencyProperty.Register("DraggingEnabled", typeof(bool), typeof(DraggablePanel), new PropertyMetadata(true));
        private const string ElementGripBarElement = "GripBarElement";
        private Point lastDragPosition;

        public event DragEventHander DragFinished;
        public event DragEventHander DragMoved;
        public event DragEventHander DragStarted;
        public event EventHandler PanelFocused;

        // Methods
        private void GripBarElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((FrameworkElement)sender).CaptureMouse();
            this.LastDragPosition = e.GetPosition(sender as UIElement);
            this.dragging = true;
            if (this.DragStarted != null)
            {
                this.DragStarted(this, new DragEventArgs(0.0, 0.0, e));
            }
        }

        private void GripBarElement_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((FrameworkElement)sender).ReleaseMouseCapture();
            this.dragging = false;
            Point position = e.GetPosition(sender as UIElement);
            if (this.DragFinished != null)
            {
                this.DragFinished(this, new DragEventArgs(position.X - this.LastDragPosition.X, position.Y - this.LastDragPosition.Y, e));
            }
        }

        private void GripBarElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.dragging)
            {
                Point position = e.GetPosition(sender as UIElement);
                this.UpdatePosition(new Point((Canvas.GetLeft(this) + position.X) - this.LastDragPosition.X, (Canvas.GetTop(this) + position.Y) - this.LastDragPosition.Y));
                if (this.DragMoved != null)
                {
                    this.DragMoved(this, new DragEventArgs(position.X - this.LastDragPosition.X, position.Y - this.LastDragPosition.Y, e));
                }
                this.LastDragPosition = e.GetPosition(sender as UIElement);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FrameworkElement templateChild = base.GetTemplateChild("GripBarElement") as FrameworkElement;
            if (templateChild != null)
            {
                templateChild.MouseLeftButtonDown += new MouseButtonEventHandler(this.GripBarElement_MouseLeftButtonDown);
                templateChild.MouseMove += new MouseEventHandler(this.GripBarElement_MouseMove);
                templateChild.MouseLeftButtonUp += new MouseButtonEventHandler(this.GripBarElement_MouseLeftButtonUp);
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            int currentZIndex = CurrentZIndex;
            CurrentZIndex = currentZIndex + 1;
            Canvas.SetZIndex(this, currentZIndex);
            if (this.PanelFocused != null)
            {
                this.PanelFocused(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            int currentZIndex = CurrentZIndex;
            CurrentZIndex = currentZIndex + 1;
            Canvas.SetZIndex(this, currentZIndex);
            if (this.PanelFocused != null)
            {
                this.PanelFocused(this, EventArgs.Empty);
            }
        }

        public virtual void UpdatePosition(Point pos)
        {
            Canvas.SetLeft(this, Math.Max(0.0, pos.X));
            Canvas.SetTop(this, Math.Max(0.0, pos.Y));
        }

        public virtual void UpdateSize(double width, double height)
        {
            base.Width = Math.Max(base.MinWidth, width);
            base.Height = Math.Max(base.MinHeight, height);
        }

        internal static int CurrentZIndex
        {
            get
            {
                return currentZIndex;
            }
            set
            {
                currentZIndex = value;
            }
        }

        public bool DraggingEnabled
        {
            get
            {
                return (bool)base.GetValue(DraggingEnabledProperty);
            }
            set
            {
                base.SetValue(DraggingEnabledProperty, value);
            }
        }

        internal Point LastDragPosition
        {
            get
            {
                return this.lastDragPosition;
            }
            set
            {
                this.lastDragPosition = value;
            }
        }
    }
    public class DragEventArgs : EventArgs
    {
        // Methods
        public DragEventArgs()
        {
        }

        public DragEventArgs(double horizontalChange, double verticalChange, MouseEventArgs mouseEventArgs)
        {
            this.HorizontalChange = horizontalChange;
            this.VerticalChange = verticalChange;
            this.MouseEventArgs = mouseEventArgs;
        }

        // Properties
        public double HorizontalChange { get; set; }

        public MouseEventArgs MouseEventArgs { get; set; }

        public double VerticalChange { get; set; }

    }
}
