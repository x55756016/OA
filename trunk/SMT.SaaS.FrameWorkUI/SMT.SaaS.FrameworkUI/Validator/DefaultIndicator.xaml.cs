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

namespace SMT.SaaS.FrameworkUI.Validator
{
    public partial class DefaultIndicator : UserControl, IIndicator
    {
		public DefaultIndicator()
        {
            InitializeComponent();

            _Border.Width = this.Width;
			_Border.Height = this.Height;


			this.Loaded += new RoutedEventHandler(Indicator1_Loaded);
            //this.Opacity = 0;
			
        }

        void Indicator1_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateLayout();
            this.ApplyTemplate();
        }

        public void ShowIndicator(ValidatorBase validator)
        {
			try
			{
				//TODO: Refactor Indicator insertion to UserControl
				//There is an issue with adding the Indicator to the LayoutRoot and where it's positioned.
				//This way works for now, but should be refactored.

                //Modified by Reo, 需要显示到被验证控件的父元素
                //if (this.Parent == null)
                //{
                //    Panel g = (Panel)validator.UserControl.FindName("LayoutRoot");
                //    g.Children.Add(this);
					

                //    if (validator.ElementToValidate.Parent == g)
                //    {
                //        if (g is Grid)
                //        {
                //            var col = Grid.GetColumn(validator.ElementToValidate);
                //            var row = Grid.GetRow(validator.ElementToValidate);

                //            Grid.SetColumn(this, col);
                //            Grid.SetRow(this, row);
                //        }
                //        PositionIndicatorToRoot(validator);
                //    }
                //    else
                //    {
                //        PositionIndicator(validator);
                //    }
                //}

                //暂时不显示
                if (this.Parent == null)
                {

                    Panel g = (Panel)FindParentControl<Grid>(validator.ElementToValidate);
                    g.Children.Add(this);


                    if (validator.ElementToValidate.Parent == g)
                    {
                        if (g is Grid)
                        { 
                            var col = Grid.GetColumn(validator.ElementToValidate);
                            var row = Grid.GetRow(validator.ElementToValidate);

                            Grid.SetColumn(this, col);
                            Grid.SetRow(this, row);
                            Grid.SetColumnSpan(this, Grid.GetColumnSpan(validator.ElementToValidate));
                            Grid.SetRowSpan(this, Grid.GetRowSpan(validator.ElementToValidate));
                        }
                        PositionIndicatorToRoot(validator);
                    }
                    else
                    {
                        PositionIndicator(validator);
                    }
                }

				if (!string.IsNullOrEmpty(validator.ErrorMessage))
				{
					ToolTipService.SetToolTip(this, validator.ErrorMessage);
				}
				validator.UserControl.UpdateLayout();


				HideIndicatorStory.Stop();
				ShowIndicatorStory.Begin();
			}
			catch(Exception e) {
				var s = e;
			}
           // return;
        }
        public  T FindParentControl<T>(DependencyObject item) where T : class
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                T parentGrid = parent as T;
                return (parentGrid != null) ? parentGrid : FindParentControl<T>(parent);
            }
            return null;
        }
        public void HideIndicator()
		{
            ShowIndicatorStory.Stop();
            HideIndicatorStory.Begin();
		}

        private void PositionIndicatorToRoot(ValidatorBase validator)
        {
            //double controlTop = (validator.ElementToValidate.ActualHeight / 2) - (this.Height / 2);
            double controlTop = (validator.ElementToValidate.ActualHeight / 2);
            double controlLeft = validator.ElementToValidate.ActualWidth + 8;
            this.HorizontalAlignment = HorizontalAlignment.Right;
            this.Margin = new Thickness(0, 0, 0, 0);
            
        }

		private void PositionIndicator(ValidatorBase validator)
		{
			GeneralTransform gt = validator.ElementToValidate.TransformToVisual(validator.UserControl);
			Point offset = gt.Transform(new Point(0, 0));
 
			double controlTop = offset.Y + (validator.ElementToValidate.ActualHeight / 2) - (this.Height / 2);

			double controlLeft = offset.X + validator.ElementToValidate.ActualWidth + 3;
            this.HorizontalAlignment = HorizontalAlignment.Right;
            this.Margin = new Thickness(0, 0, 0, 0);
           
		}
    }
}
