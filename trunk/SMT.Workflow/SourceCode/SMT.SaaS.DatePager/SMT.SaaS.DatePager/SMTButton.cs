using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SMT.SaaS.DatePager
{
    public class SMTButton : Button
    {

        public SMTButton()
            : base()
        {
            this.DefaultStyleKey = typeof(SMTButton);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public static readonly DependencyProperty normalImageProperty = DependencyProperty.Register("NormalImage", typeof(ImageSource), typeof(SMTButton), null);
        public ImageSource NormalImage
        {
            get { return (ImageSource)GetValue(normalImageProperty); }
            set { SetValue(normalImageProperty, value); }
        }

        public static readonly DependencyProperty hoverImageProperty = DependencyProperty.Register("HoverImage", typeof(ImageSource), typeof(SMTButton), null);
        public ImageSource HoverImage
        {
            get { return (ImageSource)GetValue(normalImageProperty); }
            set { SetValue(hoverImageProperty, value); }
        }

        public static readonly DependencyProperty clickImageProperty = DependencyProperty.Register("ClickImage", typeof(ImageSource), typeof(SMTButton), null);
        public ImageSource ClickImage
        {
            get { return (ImageSource)GetValue(normalImageProperty); }
            set { SetValue(clickImageProperty, value); }
        }

    }
}
