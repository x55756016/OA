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

namespace SMT.SAAS.Controls.Toolkit
{
    public class HeaderedContentControl : ContentControl
    {
        // Fields
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(HeaderedContentControl), new PropertyMetadata(new PropertyChangedCallback(HeaderedContentControl.OnHeaderPropertyChanged)));
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(HeaderedContentControl), new PropertyMetadata(new PropertyChangedCallback(HeaderedContentControl.OnHeaderTemplatePropertyChanged)));

        // Methods
        public HeaderedContentControl()
        {
            base.DefaultStyleKey = typeof(HeaderedContentControl);
        }

        protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
        {
        }

        private static void OnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeaderedContentControl)d).OnHeaderChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
        {
        }

        private static void OnHeaderTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HeaderedContentControl)d).OnHeaderTemplateChanged((DataTemplate)e.OldValue, (DataTemplate)e.NewValue);
        }

        // Properties
        [Category("公共属性"), Description("获取或设置显示的标题对象.")]
        public object Header
        {
            get
            {
                return base.GetValue(HeaderProperty);
            }
            set
            {
                base.SetValue(HeaderProperty, value);
            }
        }

        [Category("公共属性"), Description("获取或设置控件头部要显示的模版.")]
        public DataTemplate HeaderTemplate
        {
            get
            {
                return (DataTemplate)base.GetValue(HeaderTemplateProperty);
            }
            set
            {
                base.SetValue(HeaderTemplateProperty, value);
            }
        }
    }
}
