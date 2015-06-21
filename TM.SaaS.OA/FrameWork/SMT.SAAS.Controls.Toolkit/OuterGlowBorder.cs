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
    public class OuterGlowBorder : ContentControl
    {
        // Fields
        public static readonly DependencyProperty ClipContentProperty = DependencyProperty.Register("ClipContent", typeof(bool), typeof(OuterGlowBorder), null);
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(OuterGlowBorder), null);
        private Border outerGlowBorder;
        public static readonly DependencyProperty OuterGlowColorProperty = DependencyProperty.Register("OuterGlowColor", typeof(Color), typeof(OuterGlowBorder), new PropertyMetadata(new PropertyChangedCallback(OuterGlowBorder.OuterGlowColor_Changed)));
        public static readonly DependencyProperty OuterGlowOpacityProperty = DependencyProperty.Register("OuterGlowOpacity", typeof(double), typeof(OuterGlowBorder), null);
        public static readonly DependencyProperty OuterGlowSizeProperty = DependencyProperty.Register("OuterGlowSize", typeof(double), typeof(OuterGlowBorder), null);
        public static readonly DependencyProperty ShadowCornerRadiusProperty = DependencyProperty.Register("ShadowCornerRadius", typeof(CornerRadius), typeof(OuterGlowBorder), null);
        private GradientStop shadowHorizontal1;
        private GradientStop shadowHorizontal2;
        private GradientStop shadowOuterStop1;
        private GradientStop shadowOuterStop2;
        private GradientStop shadowVertical1;
        private GradientStop shadowVertical2;

        // Methods
        public OuterGlowBorder()
        {
            base.DefaultStyleKey = typeof(OuterGlowBorder);
            base.SizeChanged += new SizeChangedEventHandler(this.OuterGlowContentControl_SizeChanged);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.shadowOuterStop1 = (GradientStop)base.GetTemplateChild("PART_ShadowOuterStop1");
            this.shadowOuterStop2 = (GradientStop)base.GetTemplateChild("PART_ShadowOuterStop2");
            this.shadowVertical1 = (GradientStop)base.GetTemplateChild("PART_ShadowVertical1");
            this.shadowVertical2 = (GradientStop)base.GetTemplateChild("PART_ShadowVertical2");
            this.shadowHorizontal1 = (GradientStop)base.GetTemplateChild("PART_ShadowHorizontal1");
            this.shadowHorizontal2 = (GradientStop)base.GetTemplateChild("PART_ShadowHorizontal2");
            this.outerGlowBorder = (Border)base.GetTemplateChild("PART_OuterGlowBorder");
            this.UpdateGlowSize(this.OuterGlowSize);
            this.UpdateGlowColor(this.OuterGlowColor);
        }

        private static void OuterGlowColor_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((OuterGlowBorder)dependencyObject).UpdateGlowColor((Color)eventArgs.NewValue);
        }

        private void OuterGlowContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateStops(e.NewSize);
        }

        internal void UpdateGlowColor(Color color)
        {
            if (this.shadowVertical1 != null)
            {
                this.shadowVertical1.Color = color;
            }
            if (this.shadowVertical2 != null)
            {
                this.shadowVertical2.Color = color;
            }
            if (this.shadowOuterStop1 != null)
            {
                this.shadowOuterStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
            }
            if (this.shadowOuterStop2 != null)
            {
                this.shadowOuterStop2.Color = Color.FromArgb(0, color.R, color.G, color.B);
            }
        }

        internal void UpdateGlowSize(double size)
        {
            if (this.outerGlowBorder != null)
            {
                this.outerGlowBorder.Margin = new Thickness(-Math.Abs(size));
            }
        }

        private void UpdateStops(Size size)
        {
            if ((size.Width > 0.0) && (size.Height > 0.0))
            {
                if (this.shadowHorizontal1 != null)
                {
                    this.shadowHorizontal1.Offset = Math.Abs(this.OuterGlowSize) / ((size.Width + Math.Abs(this.OuterGlowSize)) + Math.Abs(this.OuterGlowSize));
                }
                if (this.shadowHorizontal2 != null)
                {
                    this.shadowHorizontal2.Offset = 1.0 - (Math.Abs(this.OuterGlowSize) / ((size.Width + Math.Abs(this.OuterGlowSize)) + Math.Abs(this.OuterGlowSize)));
                }
                if (this.shadowVertical1 != null)
                {
                    this.shadowVertical1.Offset = Math.Abs(this.OuterGlowSize) / ((size.Height + Math.Abs(this.OuterGlowSize)) + Math.Abs(this.OuterGlowSize));
                }
                if (this.shadowVertical2 != null)
                {
                    this.shadowVertical2.Offset = 1.0 - (Math.Abs(this.OuterGlowSize) / ((size.Height + Math.Abs(this.OuterGlowSize)) + Math.Abs(this.OuterGlowSize)));
                }
            }
        }

        // Properties
        [Description("Sets whether the content is clipped or not."), Category("Appearance")]
        public bool ClipContent
        {
            get
            {
                return (bool)base.GetValue(ClipContentProperty);
            }
            set
            {
                base.SetValue(ClipContentProperty, value);
            }
        }

        [Description("Sets the corner radius on the border."), Category("Appearance")]
        public CornerRadius CornerRadius
        {
            get
            {
                return (CornerRadius)base.GetValue(CornerRadiusProperty);
            }
            set
            {
                base.SetValue(CornerRadiusProperty, value);
                CornerRadius radius = new CornerRadius(Math.Abs((double)(value.TopLeft * 1.5)), Math.Abs((double)(value.TopRight * 1.5)), Math.Abs((double)(value.BottomRight * 1.5)), Math.Abs((double)(value.BottomLeft * 1.5)));
                base.SetValue(ShadowCornerRadiusProperty, radius);
            }
        }

        [Category("Appearance"), Description("The outer glow color.")]
        public Color OuterGlowColor
        {
            get
            {
                return (Color)base.GetValue(OuterGlowColorProperty);
            }
            set
            {
                base.SetValue(OuterGlowColorProperty, value);
            }
        }

        [Description("The outer glow opacity."), Category("Appearance")]
        public double OuterGlowOpacity
        {
            get
            {
                return (double)base.GetValue(OuterGlowOpacityProperty);
            }
            set
            {
                base.SetValue(OuterGlowOpacityProperty, value);
            }
        }

        [Category("Appearance"), Description("The outer glow size.")]
        public double OuterGlowSize
        {
            get
            {
                return (double)base.GetValue(OuterGlowSizeProperty);
            }
            set
            {
                base.SetValue(OuterGlowSizeProperty, value);
                this.UpdateGlowSize(this.OuterGlowSize);
                this.UpdateStops(new Size(base.ActualWidth, base.ActualHeight));
            }
        }
    }
}
