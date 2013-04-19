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


namespace SMT.SaaS.FrameworkUI.Validator
{
	public class RangeValidator : ValidatorBase
	{
		public RangeValidator():base()
		{
		}
		
		public double Min
		{
			get { return (double)GetValue(MinProperty); }
			set { SetValue(MinProperty, value); }
		}
		// Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinProperty =
			DependencyProperty.Register("Min", typeof(double), typeof(RangeValidator), new PropertyMetadata(double.MinValue));
		
		public double Max
		{
			get { return (double)GetValue(MaxProperty); }
			set { SetValue(MaxProperty, value); }
		}
		// Using a DependencyProperty as the backing store for Max.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaxProperty =
			DependencyProperty.Register("Max", typeof(double), typeof(RangeValidator), new PropertyMetadata(double.MaxValue));
		
		public TextBoxFilterType Filter
		{
			get { return (TextBoxFilterType)GetValue(FilterProperty); }
			set { SetValue(FilterProperty, value); }
		}
		// Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FilterProperty =
			DependencyProperty.Register("Filter", typeof(TextBoxFilterType), typeof(RangeValidator), new PropertyMetadata(TextBoxFilterType.Decimal));

		public override void ActivateValidationRoutine()
		{
			base.ActivateValidationRoutine();

			TextBoxFilterService.SetFilter(ElementToValidate, Filter);
		}
		
		protected override bool ValidateControl()
		{
			if (ElementToValidate is TextBox)
			{
				string txt = (ElementToValidate as TextBox).Text;
				if(String.IsNullOrEmpty(txt)) return true;

				double d;
				if (!double.TryParse((ElementToValidate as TextBox).Text, out d))
				{
					return false;
				}
				return d >= Min && d <= Max;
			}
			return true;
		}
	}
}
