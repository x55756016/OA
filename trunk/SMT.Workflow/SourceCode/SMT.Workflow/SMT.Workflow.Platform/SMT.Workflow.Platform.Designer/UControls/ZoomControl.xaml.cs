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
using System.ComponentModel;

namespace SMT.Workflow.Platform.Designer.UControls
{
    public partial class ZoomControl : UserControl, INotifyPropertyChanged
    {
        // 定义
        private const double ButtonOpacity = 0.4;
        private const double ButtonOverOpacity = 0.8;
        private const double DownOpacity = 1.0;
        private bool dragging;
        private int increment;

        private static ZoomInterval[] intervals = new ZoomInterval[] 
        {
            new ZoomInterval(0.1, 1.0, 0x1a, 0x11), 
            new ZoomInterval(1.0, 1.5, 0x11, 12),
            new ZoomInterval(1.5, 3.0, 12, 9), 
            new ZoomInterval(3.0, 10.0, 9, 2), 
            new ZoomInterval(10.0, 20.0, 2, 0)
        };
        private bool mouseDownInButton;

        private const double ThumbOpacity = 0.8;
        private const double ThumbOverOpacity = 0.9;
        private Storyboard timer;
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomControl), new PropertyMetadata(new PropertyChangedCallback(ZoomControl.OnZoomChanged)));
        private EventHandler _ZoomToFit;
        private PropertyChangedEventHandler _PropertyChanged;

        #region 事件
        // 事件
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                this._PropertyChanged = (PropertyChangedEventHandler)Delegate.Combine(this._PropertyChanged, value);
            }
            remove
            {
                this._PropertyChanged = (PropertyChangedEventHandler)Delegate.Remove(this._PropertyChanged, value);
            }
        }

        public event EventHandler ZoomToFit
        {
            add
            {
                this._ZoomToFit = (EventHandler)Delegate.Combine(this._ZoomToFit, value);
            }
            remove
            {
                this._ZoomToFit = (EventHandler)Delegate.Remove(this._ZoomToFit, value);
            }
        }
        #endregion

        // 初始化
        public ZoomControl()
        {
            this.InitializeComponent();
            base.MouseEnter += new MouseEventHandler(ZoomControl_MouseEnter);
            base.MouseLeave += (new MouseEventHandler(ZoomControl_MouseLeave));
            this.button.MouseEnter += (new MouseEventHandler(button_MouseEnter));
            this.button.MouseLeave += (new MouseEventHandler(button_MouseLeave));
            this.button.MouseLeftButtonDown += (new MouseButtonEventHandler(button_MouseLeftButtonDown));
            this.button.MouseLeftButtonUp += (new MouseButtonEventHandler(button_MouseLeftButtonUp));
            this.sliderThumb.MouseEnter += (new MouseEventHandler(sliderThumb_MouseEnter));
            this.sliderThumb.MouseLeave += (new MouseEventHandler(sliderThumb_MouseLeave));
            this.sliderThumb.MouseLeftButtonDown += (new MouseButtonEventHandler(sliderThumb_MouseLeftButtonDown));
            this.sliderThumb.MouseMove += (new MouseEventHandler(sliderThumb_MouseMove));
            this.sliderThumb.MouseLeftButtonUp += (new MouseButtonEventHandler(sliderThumb_MouseLeftButtonUp));
            this.slider.MouseLeftButtonDown += (new MouseButtonEventHandler(Slider_MouseLeftButtonDown));
            this.slider.MouseLeftButtonUp += (new MouseButtonEventHandler(Slider_MouseLeftButtonUp));
            this.Zoom = 1.0;
        }

        #region 鼠标事件
        private void button_MouseEnter(object sender, MouseEventArgs e)
        {
            this.button.Opacity = 0.8;
        }

        private void button_MouseLeave(object sender, MouseEventArgs e)
        {
            this.button.Opacity = this.mouseDownInButton ? 0.8 : 0.4;
        }

        private void button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.button.Opacity = 1.0;
            this.mouseDownInButton = true;
            e.Handled = true;
        }

        private void button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ToggleZoom();
            this.button.Opacity = 0.8;
            this.mouseDownInButton = false;
            e.Handled = true;
        }
        #endregion

        #region 更改缩放值事件
        private void OnZoomChanged(double newZoom)
        {
            if (!this.dragging)
            {
                Grid.SetRow(this.sliderThumb, this.ZoomToRow(newZoom));
            }
            //if (newZoom < 3.0)
            //{
                this.text.Text = ((int)(newZoom*100)).ToString() + "%";
            //}
            //else
            //{
            //    this.text.Text = ((int)(newZoom / 1.0)).ToString() + "x";
            //}
            if (this._PropertyChanged != null)
            {
                this._PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Zoom"));
            }
        }

        private static void OnZoomChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((ZoomControl)obj).OnZoomChanged((double)args.NewValue);
        }

        private double RowToZoom(int row)
        {
            foreach (ZoomInterval interval in intervals)
            {
                if ((row <= interval.MinRow) && (row >= interval.MaxRow))
                {
                    return (interval.MinZoom + (((row - interval.MinRow) * (interval.MaxZoom - interval.MinZoom)) / ((double)(interval.MaxRow - interval.MinRow))));
                }
            }
            if (row > 0)
            {
                return intervals[0].MinZoom;
            }
            return intervals[intervals.Length - 1].MaxZoom;
        }

        private void Slider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.slider.CaptureMouse();
            int num = (int)((e.GetPosition(this.sliderBar).Y * this.slider.RowDefinitions.Count) / this.sliderBar.ActualHeight);
            int row = Grid.GetRow(this.sliderThumb);
            if ((num > row) && (row < intervals[0].MinRow))
            {
                this.ThumbRow++;
                this.StartTimer(1);
            }
            else if ((num < row) && (row > 0))
            {
                this.ThumbRow--;
                this.StartTimer(-1);
            }
        }

        private void Slider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.StopTimer();
            this.slider.ReleaseMouseCapture();
        }

        private void sliderThumb_MouseEnter(object sender, MouseEventArgs e)
        {
            this.sliderThumb.Opacity = 0.9;
        }

        private void sliderThumb_MouseLeave(object sender, MouseEventArgs e)
        {
            this.sliderThumb.Opacity = 0.8;
        }

        private void sliderThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.sliderThumb.CaptureMouse();
            this.sliderThumb.Opacity = 1.0;
            this.dragging = true;
            e.Handled = true;
        }

        private void sliderThumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.slider.Opacity = 0.9;
            this.dragging = false;
            this.sliderThumb.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void sliderThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.dragging)
            {
                int num = (int)((e.GetPosition(this.sliderBar).Y * this.slider.RowDefinitions.Count) / this.sliderBar.ActualHeight);
                this.ThumbRow = num;
            }
        }

        private void StartTimer(int increment)
        {
            this.StopTimer();
            if (((this.ThumbRow + increment) >= 0) && ((this.ThumbRow + increment) < this.slider.RowDefinitions.Count))
            {
                this.timer = new Storyboard();
                this.timer.Duration = new Duration(TimeSpan.FromMilliseconds(200.0));
                this.timer.Completed += new EventHandler(timer_Completed);
                this.increment = increment;
                this.timer.Begin();
            }
        }

        private void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer = null;
            }
        }

        private void timer_Completed(object sender, EventArgs e)
        {
            if (this.timer != null)
            {
                this.ThumbRow += this.increment;
                if (((this.ThumbRow + this.increment) >= 0) && ((this.ThumbRow + this.increment) < this.slider.RowDefinitions.Count))
                {
                    this.timer.Duration = new Duration(TimeSpan.FromMilliseconds(50.0));
                    this.timer.Begin();
                }
            }
        }

        private void ToggleZoom()
        {
            if (this.Zoom != 1.0)
            {
                this.Zoom = 1.0;
            }
            else if (this._ZoomToFit != null)
            {
                this._ZoomToFit.Invoke(this, EventArgs.Empty);
            }
        }

        private void ZoomControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.enter.Begin();
        }

        private void ZoomControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.leave.Begin();
        }

        private int ZoomToRow(double zoom)
        {
            foreach (ZoomInterval interval in intervals)
            {
                if ((zoom >= interval.MinZoom) && (zoom <= interval.MaxZoom))
                {
                    return (int)(interval.MinRow + (((zoom - interval.MinZoom) * (interval.MaxRow - interval.MinRow)) / (interval.MaxZoom - interval.MinZoom)));
                }
            }
            if (zoom < 1.0)
            {
                return intervals[0].MinRow;
            }
            return intervals[intervals.Length - 1].MaxRow;
        }
        #endregion

        // 属性
        private int ThumbRow
        {
            get
            {
                return Grid.GetRow(this.sliderThumb);
            }
            set
            {
                value = Math.Max(0, Math.Min(value, this.slider.RowDefinitions.Count - 1));
                Grid.SetRow(this.sliderThumb, value);
                this.Zoom = this.RowToZoom(value);
            }
        }

        public double Zoom
        {
            get
            {
                return (double)base.GetValue(ZoomProperty);
            }
            set
            {
                base.SetValue(ZoomProperty, value);
            }
        }

        // 类型定义
        internal class ZoomInterval
        {
            // Fields
            private int _MaxRow;
            private double _MaxZoom;
            private int _MinRow;
            private double _MinZoom;

            // 方法
            internal ZoomInterval(double minZoom, double maxZoom, int minRow, int maxRow)
            {
                this._MinZoom = minZoom;
                this._MaxZoom = maxZoom;
                this._MinRow = minRow;
                this._MaxRow = maxRow;
            }

            #region 属性
            // 属性
            internal int MaxRow
            {

                get
                {
                    return this._MaxRow;
                }

                set
                {
                    this._MaxRow = value;
                }
            }

            internal double MaxZoom
            {

                get
                {
                    return this._MaxZoom;
                }

                set
                {
                    this._MaxZoom = value;
                }
            }

            internal int MinRow
            {

                get
                {
                    return this._MinRow;
                }

                set
                {
                    this._MinRow = value;
                }
            }

            internal double MinZoom
            {

                get
                {
                    return this._MinZoom;
                }

                set
                {
                    this._MinZoom = value;
                }
            }
            #endregion
        }
    }
}
