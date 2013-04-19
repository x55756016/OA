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
    public class DateValidator : ValidatorBase
    {
        public DateValidator() : base()
		{

		}

        public DateTime Date
        {
            get { return (DateTime)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }

        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register("IsDateTime", typeof(DateTime), typeof(DateValidator), new PropertyMetadata(DateTime.Now));

        protected override bool ValidateControl()
        {
            if (ElementToValidate is DatePicker)
            {
                DatePicker datePiker = ElementToValidate as DatePicker;
                try
                {
                    Convert.ToDateTime(datePiker.SelectedDate.ToString());
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
