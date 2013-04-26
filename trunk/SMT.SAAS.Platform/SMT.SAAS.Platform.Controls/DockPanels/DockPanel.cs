//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: DockPanel停靠缩放面板控件
// 完成日期：2011-04-06 
// 版    本：V1.0 
// 作    者：高雁
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace SMT.SAAS.Platform.Controls.DockPanels
{
    [TemplatePart(Name = "DockButtonLeft", Type = typeof(ToggleButton)),
     TemplatePart(Name = "NormalButtonLeft", Type = typeof(ToggleButton)),
     TemplatePart(Name = "ContentBorderLeft", Type = typeof(Border)),
     TemplatePart(Name = "ContentLeft", Type = typeof(ContentPresenter)),

     TemplatePart(Name = "DockButtonRight", Type = typeof(ToggleButton)),
     TemplatePart(Name = "NormalButtonRight", Type = typeof(ToggleButton)),
     TemplatePart(Name = "ContentBorderRight", Type = typeof(Border)),
     TemplatePart(Name = "ContentRight", Type = typeof(ContentPresenter)),

     TemplatePart(Name = "TemplateLeft", Type = typeof(Grid)),
     TemplatePart(Name = "TemplateRight", Type = typeof(Grid)),

     TemplatePart(Name = "DockPanelLeft", Type = typeof(Border)),
     TemplatePart(Name = "DockPanelRight", Type = typeof(Border))]

    public class DockPanel : Control
    {
        public static readonly DependencyProperty DockContentProperty = DependencyProperty.Register("DockContent", typeof(object), typeof(DockPanel), null);
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(DockPanel), null);
        public static readonly DependencyProperty IsDockProperty = DependencyProperty.Register("IsDock", typeof(bool), typeof(DockPanel), null);
        public static readonly DependencyProperty DockPanelPlacementProperty =
            DependencyProperty.Register("DockPanelPlacement", typeof(Dock), typeof(DockPanel), new PropertyMetadata(new PropertyChangedCallback(DockPanel.OnDockPanelPlacementPropertyChanged)));
        private Dock _DockPanelPlacement = Dock.Left;


        #region 属性
        private static void OnDockPanelPlacementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DockPanel node = d as DockPanel;
            node._DockPanelPlacement = (Dock)e.NewValue;
        }
        /// <summary>
        /// 内容编辑区
        /// </summary>
        public object DockContent
        {
            get
            {
                return GetValue(DockContentProperty);
            }
            set
            {
                SetValue(DockContentProperty, value);
            }
        }
        /// <summary>
        /// Docking圆角
        /// </summary>
        public CornerRadius CornerRadius
        {
            get
            {
                return (CornerRadius)GetValue(CornerRadiusProperty);
            }
            set
            {
                SetValue(CornerRadiusProperty, value);
            }
        }

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsDock
        {
            get
            {
                return (bool)GetValue(IsDockProperty);
            }
            set
            {
                SetValue(IsDockProperty, value);
                ChangeVisualState(true);
            }
        }

        public Dock DockPanelPlacement
        {
            get
            { 
                return (Dock)GetValue(DockPanelPlacementProperty);
            }
            set 
            {
                SetValue(DockPanelPlacementProperty, value);
            }
        }
        #endregion

        #region 属性定义 
        private ToggleButton _dockButtonLeft;
        private ToggleButton _normalButtonLeft;

        private ToggleButton _dockButtonRight;
        private ToggleButton _normalButtonRight;

        private Border _contentBorderLeft;
        private Border _contentBorderRight;
        private Border _DockPanelLeft;

        private Storyboard _dockStoryboard;
        private Storyboard _normalStoryboard;
        private Storyboard _dockStoryboard2;
        private Storyboard _normalStoryboard2;
        private double _width;

        private Grid _TemplateLeft;
        private Grid _TemplateRight;

        private ContentPresenter _ContentLeft;
        private ContentPresenter _ContentRight;

        public event EventHandler<ClickEventArgs> CompleteClick;
        #endregion

        public DockPanel()
        {
            DefaultStyleKey = typeof(DockPanel);
            _DockPanelPlacement = Dock.Left;
            this.SizeChanged += new SizeChangedEventHandler(DockPanel_SizeChanged);
        }

        void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SizeChanged -= new SizeChangedEventHandler(DockPanel_SizeChanged);
        }

        #region 重写
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _dockButtonLeft = base.GetTemplateChild("DockButtonLeft") as ToggleButton;
            _dockButtonRight = base.GetTemplateChild("DockButtonRight") as ToggleButton;
           
            _normalButtonLeft = base.GetTemplateChild("NormalButtonLeft") as ToggleButton;
            _normalButtonRight = base.GetTemplateChild("NormalButtonRight") as ToggleButton;

            _contentBorderLeft = base.GetTemplateChild("ContentBorderLeft") as Border;
            _contentBorderRight = base.GetTemplateChild("ContentBorderRight") as Border;
            _DockPanelLeft = base.GetTemplateChild("LeftDockPanel") as Border;

            _TemplateLeft = base.GetTemplateChild("TemplateLeft") as Grid;
            _TemplateRight = base.GetTemplateChild("TemplateRight") as Grid;

            _ContentRight = base.GetTemplateChild("ContentRight") as ContentPresenter;
            _ContentLeft = base.GetTemplateChild("ContentLeft") as ContentPresenter;

            if (_dockButtonLeft != null)
                _dockButtonLeft.Click += dockButton_Click;

            if (_normalButtonLeft != null)
                _normalButtonLeft.Click += dockButton_Click;

            if (_dockButtonRight != null)
                _dockButtonRight.Click += dockButton_Click;

            if (_normalButtonRight != null)
                _normalButtonRight.Click += dockButton_Click;

            if (_DockPanelPlacement == Dock.Left)
            {
                _TemplateLeft.Visibility = Visibility.Visible;
                _TemplateRight.Visibility = Visibility.Collapsed;
            }
            else
            {
                _TemplateRight.Visibility = Visibility.Visible;
                _TemplateLeft.Visibility = Visibility.Collapsed;
            }

            if (!_width.IsNaN())
            {
                _width = this.Width;
            }
            else
            {
                _width = 250;
            }

            this.InitPart();
            this.ChangeVisualState(false);
        }
        #endregion

        #region 事件支持
        private void dockButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsDock = !this.IsDock;
            ChangeVisualState(true);
        }

        /// <summary>
        /// 更改面板区域状态
        /// </summary>
        /// <param name="useTransitions"></param>
        private void ChangeVisualState(bool useTransitions)
        {
            if (!this.IsDock)
            {
                Normal();
            }
            else
            {
                DPDock();
            }
        }

        /// <summary>
        /// 初始动画-停靠-隐藏
        /// </summary>
        private void InitPart()
        {
            if (_DockPanelPlacement == Dock.Left)
            {
                _dockStoryboard = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.Duration = new Duration(new TimeSpan(0, 0, 0));
                doubleAnimation.From = 0;
                doubleAnimation.To = _width * -1;
                Storyboard.SetTarget(doubleAnimation, _contentBorderLeft);
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(Canvas.Left)"));
                _dockStoryboard.Children.Add(doubleAnimation);


                _normalStoryboard = new Storyboard();
                DoubleAnimation doubleAnimation2 = new DoubleAnimation();
                doubleAnimation2.Duration = new Duration(new TimeSpan(0, 0, 0));
                doubleAnimation2.From = _width * -1;
                doubleAnimation2.To = 0;
                Storyboard.SetTarget(doubleAnimation2, _contentBorderLeft);
                Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(Canvas.Left)"));
                _normalStoryboard.Children.Add(doubleAnimation2);
                Canvas.SetLeft(_dockButtonLeft, _width);
            }
            else
            {
                _dockStoryboard2 = new Storyboard();
                DoubleAnimation doubleAnimation_r = new DoubleAnimation();
                doubleAnimation_r.Duration = new Duration(new TimeSpan(0, 0, 0));
                doubleAnimation_r.From = _width * 1;
                doubleAnimation_r.To = 0;
                Storyboard.SetTarget(doubleAnimation_r, _contentBorderRight);
                Storyboard.SetTargetProperty(doubleAnimation_r, new PropertyPath("(Canvas.Left)"));
                _dockStoryboard2.Children.Add(doubleAnimation_r);


                _normalStoryboard2 = new Storyboard();
                DoubleAnimation doubleAnimation2_r = new DoubleAnimation();
                doubleAnimation2_r.Duration = new Duration(new TimeSpan(0, 0, 0));
                doubleAnimation2_r.From = 0;
                doubleAnimation2_r.To = _width * 1;
                Storyboard.SetTarget(doubleAnimation2_r, _contentBorderRight);
                Storyboard.SetTargetProperty(doubleAnimation2_r, new PropertyPath("(Canvas.Left)"));
                _normalStoryboard2.Children.Add(doubleAnimation2_r);
                Canvas.SetLeft(_dockButtonLeft, _width);
             }
        }

        /// <summary>
        /// 模式-停靠 只保留透明区域中的按钮
        /// </summary>
        private void DPDock()
        {
            if (_DockPanelPlacement == Dock.Left)
            {
                _dockStoryboard.Begin();
                _dockStoryboard.Completed += (obj, args) =>
                {
                    _TemplateLeft.Visibility = Visibility.Visible;
                    _normalButtonLeft.Visibility = Visibility.Visible;
                    _dockButtonLeft.Visibility = Visibility.Collapsed;
                    _contentBorderLeft.Visibility = Visibility.Collapsed;
                };
            }
            else
            {
                _dockStoryboard2.Begin();
                _dockStoryboard2.Completed += (obj, args) =>
                {
                    _TemplateRight.Visibility = Visibility.Visible;
                    _normalButtonRight.Visibility = Visibility.Visible;
                    _dockButtonRight.Visibility = Visibility.Collapsed;
                    _contentBorderRight.Visibility = Visibility.Collapsed;
                };
            }

            this.Width = 10;
            if (CompleteClick != null)
                CompleteClick(this, new ClickEventArgs()
                {
                    islock = true
                });
        }

        /// <summary>
        /// 默认模式-显示
        /// </summary>
        private void Normal()
        {
            if (_DockPanelPlacement == Dock.Left)
            {
                _normalStoryboard.Begin();
                _normalStoryboard.Completed += (obj, args) =>
                {
                    _TemplateLeft.Visibility = Visibility.Visible;
                    _normalButtonLeft.Visibility = Visibility.Collapsed;
                    _dockButtonLeft.Visibility = Visibility.Visible;
                    _contentBorderLeft.Visibility = Visibility.Visible;
                };
            }
            else
            {
                _normalStoryboard2.Begin();
                _normalStoryboard2.Completed += (obj, args) =>
                {
                    _TemplateRight.Visibility = Visibility.Visible;
                    _normalButtonRight.Visibility = Visibility.Collapsed;
                    _dockButtonRight.Visibility = Visibility.Visible;
                    _contentBorderRight.Visibility = Visibility.Visible;
                };
            }

             this.Width = _width;

             if (CompleteClick != null)
                 CompleteClick(this, new ClickEventArgs()
              {
                  islock = false
              });
        }
        #endregion
    }
    public class ClickEventArgs : EventArgs
    {
        public bool islock { get; set; }
    }
}
