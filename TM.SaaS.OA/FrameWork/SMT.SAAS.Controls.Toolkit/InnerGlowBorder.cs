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
    public class InnerGlowBorder : ContentControl
    {
        // Fields
        private Border bottomGlow;
        private GradientStop bottomGlowStop0;
        private GradientStop bottomGlowStop1;
        public static readonly DependencyProperty ClipContentProperty = DependencyProperty.Register("ClipContent", typeof(bool), typeof(InnerGlowBorder), null);
        public static readonly DependencyProperty ContentZIndexProperty = DependencyProperty.Register("ContentZIndex", typeof(int), typeof(InnerGlowBorder), null);
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(InnerGlowBorder), null);
        public static readonly DependencyProperty InnerGlowColorProperty = DependencyProperty.Register("InnerGlowColor", typeof(Color), typeof(InnerGlowBorder), new PropertyMetadata(new PropertyChangedCallback(InnerGlowBorder.InnerGlowColor_Changed)));
        public static readonly DependencyProperty InnerGlowOpacityProperty = DependencyProperty.Register("InnerGlowOpacity", typeof(double), typeof(InnerGlowBorder), null);
        public static readonly DependencyProperty InnerGlowSizeProperty = DependencyProperty.Register("InnerGlowSize", typeof(Thickness), typeof(InnerGlowBorder), new PropertyMetadata(new PropertyChangedCallback(InnerGlowBorder.InnerGlowSize_Changed)));
        private Border leftGlow;
        private GradientStop leftGlowStop0;
        private GradientStop leftGlowStop1;
        private Border rightGlow;
        private GradientStop rightGlowStop0;
        private GradientStop rightGlowStop1;
        private Border topGlow;
        private GradientStop topGlowStop0;
        private GradientStop topGlowStop1;

        // Methods
        public InnerGlowBorder()
        {
            base.DefaultStyleKey = typeof(InnerGlowBorder);
        }

        private static void InnerGlowColor_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((InnerGlowBorder)dependencyObject).UpdateGlowColor((Color)eventArgs.NewValue);
        }

        private static void InnerGlowSize_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((InnerGlowBorder)dependencyObject).UpdateGlowSize((Thickness)eventArgs.NewValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.leftGlow = base.GetTemplateChild("PART_LeftGlow") as Border;
            this.topGlow = base.GetTemplateChild("PART_TopGlow") as Border;
            this.rightGlow = base.GetTemplateChild("PART_RightGlow") as Border;
            this.bottomGlow = base.GetTemplateChild("PART_BottomGlow") as Border;
            this.leftGlowStop0 = base.GetTemplateChild("PART_LeftGlowStop0") as GradientStop;
            this.leftGlowStop1 = base.GetTemplateChild("PART_LeftGlowStop1") as GradientStop;
            this.topGlowStop0 = base.GetTemplateChild("PART_TopGlowStop0") as GradientStop;
            this.topGlowStop1 = base.GetTemplateChild("PART_TopGlowStop1") as GradientStop;
            this.rightGlowStop0 = base.GetTemplateChild("PART_RightGlowStop0") as GradientStop;
            this.rightGlowStop1 = base.GetTemplateChild("PART_RightGlowStop1") as GradientStop;
            this.bottomGlowStop0 = base.GetTemplateChild("PART_BottomGlowStop0") as GradientStop;
            this.bottomGlowStop1 = base.GetTemplateChild("PART_BottomGlowStop1") as GradientStop;
            this.UpdateGlowColor(this.InnerGlowColor);
            this.UpdateGlowSize(this.InnerGlowSize);
        }

        internal void UpdateGlowColor(Color color)
        {
            if (this.leftGlowStop0 != null)
            {
                this.leftGlowStop0.Color = color;
            }
            if (this.leftGlowStop1 != null)
            {
                this.leftGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
            }
            if (this.topGlowStop0 != null)
            {
                this.topGlowStop0.Color = color;
            }
            if (this.topGlowStop1 != null)
            {
                this.topGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
            }
            if (this.rightGlowStop0 != null)
            {
                this.rightGlowStop0.Color = color;
            }
            if (this.rightGlowStop1 != null)
            {
                this.rightGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
            }
            if (this.bottomGlowStop0 != null)
            {
                this.bottomGlowStop0.Color = color;
            }
            if (this.bottomGlowStop1 != null)
            {
                this.bottomGlowStop1.Color = Color.FromArgb(0, color.R, color.G, color.B);
            }
        }

        internal void UpdateGlowSize(Thickness newGlowSize)
        {
            if (this.leftGlow != null)
            {
                this.leftGlow.Width = Math.Abs(newGlowSize.Left);
            }
            if (this.topGlow != null)
            {
                this.topGlow.Height = Math.Abs(newGlowSize.Top);
            }
            if (this.rightGlow != null)
            {
                this.rightGlow.Width = Math.Abs(newGlowSize.Right);
            }
            if (this.bottomGlow != null)
            {
                this.bottomGlow.Height = Math.Abs(newGlowSize.Bottom);
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

        [Category("Appearance"), Description("Set 0 for behind the shadow, 1 for in front.")]
        public int ContentZIndex
        {
            get
            {
                return (int)base.GetValue(ContentZIndexProperty);
            }
            set
            {
                base.SetValue(ContentZIndexProperty, value);
            }
        }

        [Category("Appearance"), Description("Sets the corner radius on the border.")]
        public CornerRadius CornerRadius
        {
            get
            {
                return (CornerRadius)base.GetValue(CornerRadiusProperty);
            }
            set
            {
                base.SetValue(CornerRadiusProperty, value);
            }
        }

        [Description("The inner glow color."), Category("Appearance")]
        public Color InnerGlowColor
        {
            get
            {
                return (Color)base.GetValue(InnerGlowColorProperty);
            }
            set
            {
                base.SetValue(InnerGlowColorProperty, value);
            }
        }

        [Category("Appearance"), Description("The inner glow opacity.")]
        public double InnerGlowOpacity
        {
            get
            {
                return (double)base.GetValue(InnerGlowOpacityProperty);
            }
            set
            {
                base.SetValue(InnerGlowOpacityProperty, value);
            }
        }

        [Description("The inner glow size."), Category("Appearance")]
        public Thickness InnerGlowSize
        {
            get
            {
                return (Thickness)base.GetValue(InnerGlowSizeProperty);
            }
            set
            {
                base.SetValue(InnerGlowSizeProperty, value);
                this.UpdateGlowSize(value);
            }
        }
    }
}
