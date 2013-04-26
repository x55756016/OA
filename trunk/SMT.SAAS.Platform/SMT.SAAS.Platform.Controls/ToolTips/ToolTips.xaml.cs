//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: ToolTips提示控件
// 完成日期：2011-05-30 
// 版    本：V1.0 
// 作    者：王玲
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SMT.SAAS.Platform.Controls.ToolTips
{
    public partial class ToolTips : UserControl
    {
        public ToolTips()
        {
            InitializeComponent();

            //事件注册
            ellipse.MouseEnter += new MouseEventHandler(ellipse_MouseEnter);//ellipse鼠标触发点
            canvasMessage.MouseLeave += new MouseEventHandler(canvasMessage_MouseLeave);
            sbMsg.Completed += new EventHandler(sbMsg_Completed);
            Initialize();
            //添加提示-集合
            ToolManager.ListTips().Add(this);
        }

        /// <summary>
        /// 默认隐藏面板
        /// </summary>
        private void Initialize()
        {
            this.haloFrame.Visibility = Visibility.Collapsed;
            this.tbMessage.Visibility = Visibility.Collapsed;
            this.canvasMessage.Visibility = Visibility.Collapsed;
            base.SetValue(Canvas.ZIndexProperty, 0);  
        }

        #region 事件处理
        void sbMsg_Completed(object sender, EventArgs e)
        {
            double? nullable = this.aniOpacityMsgBackground.To;
            if ((nullable.GetValueOrDefault() <= 0.0) || !nullable.HasValue)
            {
                this.tbMessage.Visibility = Visibility.Collapsed; ;
                this.canvasMessage.Visibility = Visibility.Collapsed;
                this.popup.IsOpen = false;
            }
        }

        void canvasMessage_MouseLeave(object sender, MouseEventArgs e)
        {
            this.HideMessage();
        }

        void ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
             var p = GetTextWidthHeight(this.Message, tbMessage);
             /// <summary>
             /// 设置显示框布局位置
             /// </summary>
             switch (TipPosition)
                {
                    case Position.LeftTop:
                        this.helpGrid.Margin = new Thickness(2.0, 4.0, 24, 4.0);
                        this.borderMessage.SetValue(Canvas.LeftProperty, -(p.X+9));
                        this.borderMessage.SetValue(Canvas.TopProperty,-(p.Y-14));
                        this.Icon1.SetValue(Canvas.LeftProperty, 4.0);
                        break;

                    case Position.LeftBottom:
                        this.helpGrid.Margin = new Thickness(2.0, 4.0, 28, 4.0);
                        this.borderMessage.SetValue(Canvas.LeftProperty, -(p.X + 13));
                        this.Icon1.SetValue(Canvas.LeftProperty, 4.0);
                        break;

                    case Position.RightTop:
                        this.helpGrid.Margin = new Thickness(22.0, 4.0, 4.0, 4.0);
                        this.borderMessage.SetValue(Canvas.TopProperty,-(p.Y-14));
                        break;

                    case Position.RightBottom:
                        this.helpGrid.Margin = new Thickness(22.0, 4.0, 4.0, 4.0);
                        break;
                 default:
                        this.helpGrid.Margin = new Thickness(22.0, 4.0, 4.0, 4.0);
                        break;
                }
            this.ShowMessage();
        }

        /// <summary>
        /// 根据控件加载内容得出所需宽高
        /// </summary>
        /// <param name="text">内容文本</param>
        /// <param name="sourceText">加载控件</param>
        /// <returns>控件的坐标</returns>
        private Point GetTextWidthHeight(string text, TextBlock sourceText)
        {
            TextBlock tbtemp = new TextBlock();
            tbtemp.Text = text;
            tbtemp.Style = sourceText.Style;
            tbtemp.FontFamily = sourceText.FontFamily;
            tbtemp.FontSize = sourceText.FontSize;
            tbtemp.FontSource = sourceText.FontSource;
            tbtemp.FontStretch = sourceText.FontStretch;
            tbtemp.FontStyle = sourceText.FontStyle;
            tbtemp.FontWeight = sourceText.FontWeight;
            tbtemp.LineHeight = sourceText.LineHeight;
            tbtemp.TextWrapping = sourceText.TextWrapping;
            tbtemp.TextAlignment = sourceText.TextAlignment;
            tbtemp.LineStackingStrategy = sourceText.LineStackingStrategy;
           
            Point xy = new Point(tbtemp.ActualWidth, tbtemp.ActualHeight);
            return xy;
        }
        #endregion

        #region 附属属性
        /// <summary>
        /// 组名
        /// </summary>
        public static readonly DependencyProperty GroupNameProperty =
           DependencyProperty.Register(
          "GroupName",
          typeof(string),
          typeof(ToolTips),
          new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnGroupNamePropertyChanged)));

        /// <summary>
        /// 显示信息
        /// </summary>
        public static DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message",
            typeof(string),
            typeof(ToolTips), 
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnMessagePropertyChanged)));

        /// <summary>
        /// 是否显示
        /// </summary>
        public static DependencyProperty IsVisibilityProperty = DependencyProperty.Register(
           "IsVisibility",
           typeof(bool),
           typeof(ToolTips),
           new PropertyMetadata(true, new PropertyChangedCallback(OnIsVisibilityPropertyChanged)));

        public static DependencyProperty TipPositionProperty = DependencyProperty.Register(
            "TipPosition",
            typeof(Position),
            typeof(ToolTips),
            new PropertyMetadata(Position.RightTop, new PropertyChangedCallback(OnTipPositionPropertyChanged)));

        private static void OnTipPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnIsVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTips tip = d as ToolTips;
            if (tip != null)
            {
                bool str = (bool)e.NewValue;
                //根据传值判断是否显示该提示控件
                if (!str)
                {
                    tip.LayoutRoot.Visibility = Visibility.Collapsed;
                    tip.LayoutRoot.Opacity = 0.0;
                }
                else
                {
                    tip.LayoutRoot.Visibility = Visibility.Visible;
                    tip.LayoutRoot.Opacity = 1.0;
                }
            }  
        }

        private static void OnGroupNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTips tip = d as ToolTips;
            if (tip != null)
            {
                string str = (e.NewValue == null) ? string.Empty : e.NewValue.ToString();
                tip.tbgroup.Text = str;
            }
        }

        public static void OnMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolTips tip = d as ToolTips;
            if (tip != null)
            {
                string str = (e.NewValue == null) ? string.Empty : e.NewValue.ToString();
                tip.tbMessage.Text = str;
            }
        }
        #endregion

        /// <summary>
        /// 显示提示信息面板
        /// </summary>
        private void ShowMessage()
        {
            this.popup.IsOpen = true;
            base.SetValue(Canvas.ZIndexProperty, 1);
            this.tbMessage.Visibility = Visibility.Visible;
            this.canvasMessage.Visibility = Visibility.Visible;
            this.aniOpacityMsgBackground.To = 0.9;
            this.aniOpacityBlackLine.To = 0.0;
            this.sbMsg.Begin();
        }

        /// <summary>
        /// 隐藏信息面板
        /// </summary>
        private void HideMessage()
        {
            base.SetValue(Canvas.ZIndexProperty, 0);
            this.aniOpacityMsgBackground.To = 0.0;
            this.aniOpacityBlackLine.To = 1.0;
            this.sbMsg.Begin();
        }

        #region Property
        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message
        {
            get
            {
                return (string)base.GetValue(MessageProperty);
            }
            set
            {
                base.SetValue(MessageProperty, value);
            }
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsVisibility
        {
            get
            {
                return (bool)base.GetValue(IsVisibilityProperty);
            }
            set
            {
                base.SetValue(IsVisibilityProperty, value);
            }
        }

        /// <summary>
        /// 组名称
        /// </summary>
        public string GroupIndex
        {
            get
            {
                return (string)GetValue(GroupNameProperty);
            }
            set
            {
                SetValue(GroupNameProperty, value);
            }
        }

        /// <summary>
        /// 提示控件坐在的方位
        /// </summary>
        public Position TipPosition
        {
            get
            {
                return (Position)GetValue(TipPositionProperty);
            }
            set
            {
                SetValue(TipPositionProperty, value);
            }
        }
        #endregion
    }

    public enum Position
    {
        LeftTop,
        RightTop,
        RightBottom,
        LeftBottom
    }
}
