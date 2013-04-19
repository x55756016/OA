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
using System.Text.RegularExpressions;

namespace SMT.SaaS.FrameworkUI.Validator
{
	public class PhoneValidator : RegexValidator
	{
		public PhoneValidator():base()
		{
			this.Expression = @"^(1?)(\D*)([0-9]{3})(\D*)([0-9]{3})(\D*)([0-9]{4})$";
		}
		
		public bool  ApplyFormat
		{
			get { return (bool )GetValue(ApplyFormatProperty); }
			set { SetValue(ApplyFormatProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ApplyFormat.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ApplyFormatProperty =
			DependencyProperty.Register("ApplyFormat", typeof(bool ), typeof(PhoneValidator), new PropertyMetadata(null));

        protected override void ElementToValidate_LostFocus(object sender, RoutedEventArgs e)
        {
            bool valid = this.Validate(true);

            if (valid && ApplyFormat)
            {
                //format phone number
                string formatted = DoFormat((ElementToValidate as TextBox).Text);
                
                if ((ElementToValidate as TextBox).Text != formatted)
                {
                    (ElementToValidate as TextBox).Text = formatted;
                }
            }
        }

        #region public static string StripFormat(string s)
        /// <summary>
        /// Gets the Digets in the string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string StripFormat(string s)
        {
            if (s == null)
                return null;

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"\D");
            return r.Replace(s, "");
        }
        #endregion

        #region public static string DoFormat(string s)
        /// <summary>
        /// Strips the number format from a string the applys
        /// the (AREACODE) PREFIX-NUMBER Format to a 10 diget number
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string DoFormat(string s)
        {
            string news = StripFormat(s);

            if (news.Length == 10)
            {
                return AddFormat(news);
            }
            else
            {
                return s;
            }
        }
        #endregion

        #region public static string AddFormat(string s)
        /// <summary>
        /// Adds the (AREACODE) PREFIX-NUMBER Format to a 10 digit number
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string AddFormat(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }

            s = StripFormat(s);
            // If the length isn't 10, then it's not in the right format.
            if (s.Length != 10)
            {
                return s;
            }
            try
            {
                return ("(" + s.Substring(0, 3) + ") " + s.Substring(3, 3) + "-" + s.Substring(6, 4));
            }
            catch
            {
                return s;
            }
        }
        #endregion		
	}
}
