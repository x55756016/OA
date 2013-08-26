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

namespace SMT.Workflow.Platform.Designer.UControls
{
    [TemplatePart(Name = "BtnImg", Type = typeof(Image)),
     TemplatePart(Name = "BtnName", Type = typeof(TextBlock))]

    public class ImgButton : Button
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ImgButton), new PropertyMetadata(new PropertyChangedCallback(ImgButton.OnIconChanged)));

        public ImgButton()
        {
            base.DefaultStyleKey = typeof(ImgButton);
        }

        public override void OnApplyTemplate()
        {
            this._icon = base.GetTemplateChild("Icon") as Image;
            this._icon.Source = this.Icon;
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImgButton node = d as ImgButton;
            node.Icon = (ImageSource)e.NewValue;
        }

        private Image _icon;

        public ImageSource Icon
        {
            get
            {
                return (ImageSource)base.GetValue(IconProperty);
            }
            set
            {
                if (value != null)
                {
                    base.SetValue(IconProperty, value);
                    if (this._icon != null)
                    {
                        this._icon.Source = value;
                    }
                }
            }
        }
    }
}
