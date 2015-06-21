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
    public class DropShadowTextBlock : Control
    {
        public static readonly DependencyProperty DropShadowAngleProperty = DependencyProperty.Register("DropShadowAngle", typeof(double), typeof(DropShadowTextBlock), new PropertyMetadata(new PropertyChangedCallback(DropShadowTextBlock.DropShadowAngle_Changed)));
        private SolidColorBrush dropShadowBrush;
        public static readonly DependencyProperty DropShadowColorProperty = DependencyProperty.Register("DropShadowColor", typeof(Color), typeof(DropShadowTextBlock), new PropertyMetadata(new PropertyChangedCallback(DropShadowTextBlock.DropShadowColor_Changed)));
        public static readonly DependencyProperty DropShadowDistanceProperty = DependencyProperty.Register("DropShadowDistance", typeof(double), typeof(DropShadowTextBlock), new PropertyMetadata(new PropertyChangedCallback(DropShadowTextBlock.DropShadowDistance_Changed)));
        public static readonly DependencyProperty DropShadowOpacityProperty = DependencyProperty.Register("DropShadowOpacity", typeof(double), typeof(DropShadowTextBlock), new PropertyMetadata(new PropertyChangedCallback(DropShadowTextBlock.DropShadowOpacity_Changed)));
        private TranslateTransform dropShadowTranslate;
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(DropShadowTextBlock), null);
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DropShadowTextBlock), null);
        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(DropShadowTextBlock), null);

        public DropShadowTextBlock()
        {
            base.DefaultStyleKey = typeof(DropShadowTextBlock);
        }

        private static void DropShadowAngle_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((DropShadowTextBlock)dependencyObject).UpdateDropShadowPosition();
        }

        private static void DropShadowColor_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((DropShadowTextBlock)dependencyObject).UpdateDropShadowBrush();
        }

        private static void DropShadowDistance_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((DropShadowTextBlock)dependencyObject).UpdateDropShadowPosition();
        }

        private static void DropShadowOpacity_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((DropShadowTextBlock)dependencyObject).UpdateDropShadowBrush();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.dropShadowTranslate = base.GetTemplateChild("PART_DropShadowTranslate") as TranslateTransform;
            this.dropShadowBrush = base.GetTemplateChild("PART_DropShadowBrush") as SolidColorBrush;
            this.UpdateDropShadowPosition();
            this.UpdateDropShadowBrush();
        }

        internal void UpdateDropShadowBrush()
        {
            if (this.dropShadowBrush != null)
            {
                this.dropShadowBrush.Color = this.DropShadowColor;
                this.dropShadowBrush.Opacity = this.DropShadowOpacity;
            }
        }

        internal void UpdateDropShadowPosition()
        {
            if (this.dropShadowTranslate != null)
            {
                Point offset = MathHelper.GetOffset(this.DropShadowAngle, this.DropShadowDistance);
                this.dropShadowTranslate.X = offset.X;
                this.dropShadowTranslate.Y = offset.Y;
            }
        }

        [Description("The drop shadow angle."), Category("Appearance")]
        public double DropShadowAngle
        {
            get
            {
                return (double)base.GetValue(DropShadowAngleProperty);
            }
            set
            {
                base.SetValue(DropShadowAngleProperty, value);
            }
        }

        [Description("The drop shadow color."), Category("Appearance")]
        public Color DropShadowColor
        {
            get
            {
                return (Color)base.GetValue(DropShadowColorProperty);
            }
            set
            {
                base.SetValue(DropShadowColorProperty, value);
            }
        }

        [Category("Appearance"), Description("The drop shadow distance.")]
        public double DropShadowDistance
        {
            get
            {
                return (double)base.GetValue(DropShadowDistanceProperty);
            }
            set
            {
                base.SetValue(DropShadowDistanceProperty, value);
            }
        }

        [Category("Appearance"), Description("The drop shadow opacity.")]
        public double DropShadowOpacity
        {
            get
            {
                return (double)base.GetValue(DropShadowOpacityProperty);
            }
            set
            {
                base.SetValue(DropShadowOpacityProperty, value);
            }
        }

        [Description("The text content."), Category("Common Properties")]
        public string Text
        {
            get
            {
                return (string)base.GetValue(TextProperty);
            }
            set
            {
                base.SetValue(TextProperty, value);
            }
        }

        [Description("The text decorations."), Category("Common Properties")]
        public TextDecorationCollection TextDecorations
        {
            get
            {
                return (TextDecorationCollection)base.GetValue(TextDecorationsProperty);
            }
            set
            {
                base.SetValue(TextDecorationsProperty, value);
            }
        }

        [Description("Whether the text wraps."), Category("Common Properties")]
        public TextWrapping TextWrapping
        {
            get
            {
                return (TextWrapping)base.GetValue(TextWrappingProperty);
            }
            set
            {
                base.SetValue(TextWrappingProperty, value);
            }
        }
    }
}
