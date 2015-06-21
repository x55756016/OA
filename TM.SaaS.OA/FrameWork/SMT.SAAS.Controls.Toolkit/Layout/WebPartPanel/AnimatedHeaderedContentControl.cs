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
    public class AnimatedHeaderedContentControl : HeaderedContentControl
    {
        private bool positionAnimating;
        private Storyboard positionAnimation;
        private TimeSpan positionAnimationTimespan = new TimeSpan(0, 0, 0, 0, 500);
        private SplineDoubleKeyFrame positionAnimationXKeyFrame;
        private SplineDoubleKeyFrame positionAnimationYKeyFrame;
        private bool sizeAnimating;
        private Storyboard sizeAnimation = new Storyboard();
        private SplineDoubleKeyFrame sizeAnimationHeightKeyFrame;
        private TimeSpan sizeAnimationTimespan = new TimeSpan(0, 0, 0, 0, 500);
        private SplineDoubleKeyFrame sizeAnimationWidthKeyFrame;

        public AnimatedHeaderedContentControl()
        {
            DoubleAnimationUsingKeyFrames timeline = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(timeline, this);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("(FrameworkElement.Width)", new object[0]));
            this.sizeAnimationWidthKeyFrame = new SplineDoubleKeyFrame();
            this.sizeAnimationWidthKeyFrame.KeySpline = new KeySpline { ControlPoint1 = new Point(0.528, 0.0), ControlPoint2 = new Point(0.142, 0.847) };
            this.sizeAnimationWidthKeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500.0));
            this.sizeAnimationWidthKeyFrame.Value = 0.0;
            timeline.KeyFrames.Add(this.sizeAnimationWidthKeyFrame);
            DoubleAnimationUsingKeyFrames frames2 = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(frames2, this);
            Storyboard.SetTargetProperty(frames2, new PropertyPath("(FrameworkElement.Height)", new object[0]));
            this.sizeAnimationHeightKeyFrame = new SplineDoubleKeyFrame();
            this.sizeAnimationHeightKeyFrame.KeySpline = new KeySpline { ControlPoint1 = new Point(0.528, 0.0), ControlPoint2 = new Point(0.142, 0.847) };
            this.sizeAnimationHeightKeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500.0));
            this.sizeAnimationHeightKeyFrame.Value = 0.0;
            frames2.KeyFrames.Add(this.sizeAnimationHeightKeyFrame);
            this.sizeAnimation.Children.Add(timeline);
            this.sizeAnimation.Children.Add(frames2);
            this.sizeAnimation.Completed += new EventHandler(this.SizeAnimation_Completed);
            this.positionAnimation = new Storyboard();
            DoubleAnimationUsingKeyFrames frames3 = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(frames3, this);
            Storyboard.SetTargetProperty(frames3, new PropertyPath("(Canvas.Left)", new object[0]));
            this.positionAnimationXKeyFrame = new SplineDoubleKeyFrame();
            this.positionAnimationXKeyFrame.KeySpline = new KeySpline { ControlPoint1 = new Point(0.528, 0.0), ControlPoint2 = new Point(0.142, 0.847) };
            this.positionAnimationXKeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500.0));
            this.positionAnimationXKeyFrame.Value = 0.0;
            frames3.KeyFrames.Add(this.positionAnimationXKeyFrame);
            DoubleAnimationUsingKeyFrames frames4 = new DoubleAnimationUsingKeyFrames();
            Storyboard.SetTarget(frames4, this);
            Storyboard.SetTargetProperty(frames4, new PropertyPath("(Canvas.Top)", new object[0]));
            this.positionAnimationYKeyFrame = new SplineDoubleKeyFrame();
            this.positionAnimationYKeyFrame.KeySpline = new KeySpline { ControlPoint1 = new Point(0.528, 0.0), ControlPoint2 = new Point(0.142, 0.847) };
            this.positionAnimationYKeyFrame.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500.0));
            this.positionAnimationYKeyFrame.Value = 0.0;
            frames4.KeyFrames.Add(this.positionAnimationYKeyFrame);
            this.positionAnimation.Children.Add(frames3);
            this.positionAnimation.Children.Add(frames4);
            this.positionAnimation.Completed += new EventHandler(this.PositionAnimation_Completed);
        }

        public void AnimatePosition(double x, double y)
        {
            if (this.positionAnimating)
            {
                this.positionAnimation.Pause();
            }
            if (base.Parent != null)
            {
                this.positionAnimating = true;
                this.positionAnimationXKeyFrame.Value = x;
                this.positionAnimationYKeyFrame.Value = y;
                this.positionAnimation.Begin();
            }
        }

        public void AnimateSize(double width, double height)
        {
            if (this.sizeAnimating)
            {
                this.sizeAnimation.Pause();
            }
            if (base.Parent != null)
            {
                base.Width = base.ActualWidth;
                base.Height = base.ActualHeight;
                this.sizeAnimating = true;
                this.sizeAnimationWidthKeyFrame.Value = width;
                this.sizeAnimationHeightKeyFrame.Value = height;
                this.sizeAnimation.Begin();
            }
        }

        private void PositionAnimation_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(this, this.positionAnimationXKeyFrame.Value);
            Canvas.SetTop(this, this.positionAnimationYKeyFrame.Value);
        }

        private void SizeAnimation_Completed(object sender, EventArgs e)
        {
            base.Width = this.sizeAnimationWidthKeyFrame.Value;
            base.Height = this.sizeAnimationHeightKeyFrame.Value;
        }

        [Description("位置变换动画的持续时间."), Category("动画属性")]
        public TimeSpan PositionAnimationDuration
        {
            get
            {
                return this.positionAnimationTimespan;
            }
            set
            {
                this.positionAnimationTimespan = value;
                if (this.positionAnimationXKeyFrame != null)
                {
                    this.positionAnimationXKeyFrame.KeyTime = KeyTime.FromTimeSpan(this.positionAnimationTimespan);
                }
                if (this.positionAnimationYKeyFrame != null)
                {
                    this.positionAnimationYKeyFrame.KeyTime = KeyTime.FromTimeSpan(this.positionAnimationTimespan);
                }
            }
        }

        [Description("大小变换动画的持续时间."), Category("动画属性")]
        public TimeSpan SizeAnimationDuration
        {
            get
            {
                return this.sizeAnimationTimespan;
            }
            set
            {
                this.sizeAnimationTimespan = value;
                if (this.sizeAnimationWidthKeyFrame != null)
                {
                    this.sizeAnimationWidthKeyFrame.KeyTime = KeyTime.FromTimeSpan(this.sizeAnimationTimespan);
                }
                if (this.sizeAnimationHeightKeyFrame != null)
                {
                    this.sizeAnimationHeightKeyFrame.KeyTime = KeyTime.FromTimeSpan(this.sizeAnimationTimespan);
                }
            }
        }
    }
}
