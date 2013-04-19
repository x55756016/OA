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
    public class LengthValidator : ValidatorBase
	{
		public LengthValidator() :base()
		{
		}

		public int MinLength
		{
			get { return (int)GetValue(MinLengthProperty); }
			set { SetValue(MinLengthProperty, value); }
		}
		// Using a DependencyProperty as the backing store for MinLength.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinLengthProperty =
			DependencyProperty.Register("MinLength", typeof(int), typeof(LengthValidator), new PropertyMetadata(Int32.MinValue));
		
		public int MaxLength
		{
			get { return (int)GetValue(MaxLengthProperty); }
			set { SetValue(MaxLengthProperty, value); }
            
		}
		// Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaxLengthProperty =
			DependencyProperty.Register("MaxLength", typeof(int), typeof(LengthValidator), new PropertyMetadata(Int32.MaxValue));


		protected override bool ValidateControl()
		{
            if ( ElementToValidate is TextBox)
			{
				TextBox box = ElementToValidate as TextBox;
                return box.Text.Length >= MinLength && box.Text.Length <= MaxLength;
			}
            return true;
		}
	}
}
