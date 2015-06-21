using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

namespace SMT.SAAS.Controls.Toolkit
{
    public class ClippingBorder : ContentControl
    {
        // Fields
        private Border border;
        private RectangleGeometry bottomLeftClip;
        private ContentControl bottomLeftContentControl;
        private RectangleGeometry bottomRightClip;
        private ContentControl bottomRightContentControl;
        public static readonly DependencyProperty ClipContentProperty = DependencyProperty.Register("ClipContent", typeof(bool), typeof(ClippingBorder), new PropertyMetadata(new PropertyChangedCallback(ClippingBorder.ClipContent_Changed)));
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ClippingBorder), new PropertyMetadata(new PropertyChangedCallback(ClippingBorder.CornerRadius_Changed)));
        private RectangleGeometry topLeftClip;
        private ContentControl topLeftContentControl;
        private RectangleGeometry topRightClip;
        private ContentControl topRightContentControl;

        // Methods
        public ClippingBorder()
        {
            base.DefaultStyleKey = typeof(ClippingBorder);
            base.SizeChanged += new SizeChangedEventHandler(this.ClippingBorder_SizeChanged);
        }

        private static void ClipContent_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((ClippingBorder)dependencyObject).UpdateClipContent((bool)eventArgs.NewValue);
        }

        private void ClippingBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ClipContent)
            {
                this.UpdateClipSize(e.NewSize);
            }
        }

        private void ContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ClipContent)
            {
                this.UpdateClipSize(new Size(base.ActualWidth, base.ActualHeight));
            }
        }

        private static void CornerRadius_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((ClippingBorder)dependencyObject).UpdateCornerRadius((CornerRadius)eventArgs.NewValue);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.border = base.GetTemplateChild("PART_Border") as Border;
            this.topLeftContentControl = base.GetTemplateChild("PART_TopLeftContentControl") as ContentControl;
            this.topRightContentControl = base.GetTemplateChild("PART_TopRightContentControl") as ContentControl;
            this.bottomRightContentControl = base.GetTemplateChild("PART_BottomRightContentControl") as ContentControl;
            this.bottomLeftContentControl = base.GetTemplateChild("PART_BottomLeftContentControl") as ContentControl;
            if (this.topLeftContentControl != null)
            {
                this.topLeftContentControl.SizeChanged += new SizeChangedEventHandler(this.ContentControl_SizeChanged);
            }
            this.topLeftClip = base.GetTemplateChild("PART_TopLeftClip") as RectangleGeometry;
            this.topRightClip = base.GetTemplateChild("PART_TopRightClip") as RectangleGeometry;
            this.bottomRightClip = base.GetTemplateChild("PART_BottomRightClip") as RectangleGeometry;
            this.bottomLeftClip = base.GetTemplateChild("PART_BottomLeftClip") as RectangleGeometry;
            this.UpdateClipContent(this.ClipContent);
            this.UpdateCornerRadius(this.CornerRadius);
        }

        internal void UpdateClipContent(bool clipContent)
        {
            if (clipContent)
            {
                if (this.topLeftContentControl != null)
                {
                    this.topLeftContentControl.Clip = this.topLeftClip;
                }
                if (this.topRightContentControl != null)
                {
                    this.topRightContentControl.Clip = this.topRightClip;
                }
                if (this.bottomRightContentControl != null)
                {
                    this.bottomRightContentControl.Clip = this.bottomRightClip;
                }
                if (this.bottomLeftContentControl != null)
                {
                    this.bottomLeftContentControl.Clip = this.bottomLeftClip;
                }
                this.UpdateClipSize(new Size(base.ActualWidth, base.ActualHeight));
            }
            else
            {
                if (this.topLeftContentControl != null)
                {
                    this.topLeftContentControl.Clip = null;
                }
                if (this.topRightContentControl != null)
                {
                    this.topRightContentControl.Clip = null;
                }
                if (this.bottomRightContentControl != null)
                {
                    this.bottomRightContentControl.Clip = null;
                }
                if (this.bottomLeftContentControl != null)
                {
                    this.bottomLeftContentControl.Clip = null;
                }
            }
        }

        private void UpdateClipSize(Size size)
        {
            if ((size.Width > 0.0) || (size.Height > 0.0))
            {
                double num = Math.Max((double)0.0, (double)((size.Width - base.BorderThickness.Left) - base.BorderThickness.Right));
                double num2 = Math.Max((double)0.0, (double)((size.Height - base.BorderThickness.Top) - base.BorderThickness.Bottom));
                if (this.topLeftClip != null)
                {
                    this.topLeftClip.Rect = new Rect(0.0, 0.0, num + (this.CornerRadius.TopLeft * 2.0), num2 + (this.CornerRadius.TopLeft * 2.0));
                }
                if (this.topRightClip != null)
                {
                    this.topRightClip.Rect = new Rect(0.0 - this.CornerRadius.TopRight, 0.0, num + this.CornerRadius.TopRight, num2 + this.CornerRadius.TopRight);
                }
                if (this.bottomRightClip != null)
                {
                    this.bottomRightClip.Rect = new Rect(0.0 - this.CornerRadius.BottomRight, 0.0 - this.CornerRadius.BottomRight, num + this.CornerRadius.BottomRight, num2 + this.CornerRadius.BottomRight);
                }
                if (this.bottomLeftClip != null)
                {
                    this.bottomLeftClip.Rect = new Rect(0.0, 0.0 - this.CornerRadius.BottomLeft, num + this.CornerRadius.BottomLeft, num2 + this.CornerRadius.BottomLeft);
                }
            }
        }

        internal void UpdateCornerRadius(CornerRadius newCornerRadius)
        {
            if (this.border != null)
            {
                this.border.CornerRadius = newCornerRadius;
            }
            if (this.topLeftClip != null)
            {
                this.topLeftClip.RadiusX = this.topLeftClip.RadiusY = newCornerRadius.TopLeft - (Math.Min(base.BorderThickness.Left, base.BorderThickness.Top) / 2.0);
            }
            if (this.topRightClip != null)
            {
                this.topRightClip.RadiusX = this.topRightClip.RadiusY = newCornerRadius.TopRight - (Math.Min(base.BorderThickness.Top, base.BorderThickness.Right) / 2.0);
            }
            if (this.bottomRightClip != null)
            {
                this.bottomRightClip.RadiusX = this.bottomRightClip.RadiusY = newCornerRadius.BottomRight - (Math.Min(base.BorderThickness.Right, base.BorderThickness.Bottom) / 2.0);
            }
            if (this.bottomLeftClip != null)
            {
                this.bottomLeftClip.RadiusX = this.bottomLeftClip.RadiusY = newCornerRadius.BottomLeft - (Math.Min(base.BorderThickness.Bottom, base.BorderThickness.Left) / 2.0);
            }
            this.UpdateClipSize(new Size(base.ActualWidth, base.ActualHeight));
        }

        // Properties
        [Category("Appearance"), Description("Sets whether the content is clipped or not.")]
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
            }
        }
    }
}
