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

namespace SMT.SaaS.FrameworkUI.WaterMarkTextBox
{
    /// <summary>
    /// 带水印的TextBox，使用时不能给该控件使用新的style样式，不然水印不会出来，因为水印通过默认样式去加载
    /// 使用方法:命名空间  xmlns:wmt="clr-namespace:SMT.SaaS.FrameworkUI.WaterMarkTextBox;assembly=SMT.SaaS.FrameworkUI"
     ///<wmt:WaterMarkTextBox >
     ///       <wmt:WaterMarkTextBox.Watermark>
    ///           <StackPanel Orientation="Horizontal" Opacity="0.4">
     ///               <TextBlock  Text="需要提示的水印信息"/>
     ///           </StackPanel>
     ///       </wmt:WaterMarkTextBox.Watermark>
     ///   </wmt:WaterMarkTextBox>
    /// </summary>
    public class WaterMarkTextBox : TextBox
    {
        public object Watermark
        {
            get { return base.GetValue(WatermarkProperty) as object; }
            set { base.SetValue(WatermarkProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark", typeof(object), typeof(WaterMarkTextBox), new PropertyMetadata(null));
        public WaterMarkTextBox()
        {
            this.DefaultStyleKey = typeof(WaterMarkTextBox);
            TextChanged += new TextChangedEventHandler(WaterMarkTextBox_TextChanged);
        }

        /// <summary>
        /// 文本框有长度大于0有值则隐藏水印，这里有个问题就是水印信息在查看的时候还会存在，
        /// 所以在查看数据时要把水印内容置空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WaterMarkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            try
            {
                //FindChildControl找不到隐藏的控件，父控件从隐藏到显示，但是子控件用FindChildControl还是找不到，奇怪
                TextBlock textBlock = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<TextBlock>(tb);
                //StackPanel sp = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<StackPanel>(tb);
                if (tb.Text.Length > 0)
                {
                    if (textBlock != null)
                    {
                        textBlock.Visibility = Visibility.Collapsed;
                    }
                    System.Windows.VisualStateManager.GoToState(tb, "Hidden", false);
                }
                else
                {
                    if (textBlock != null)
                    {
                        textBlock.Visibility = Visibility.Visible;
                    }
                    System.Windows.VisualStateManager.GoToState(tb, "Shown", false);
                }
            }
            catch
            {
                ///
            }
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
