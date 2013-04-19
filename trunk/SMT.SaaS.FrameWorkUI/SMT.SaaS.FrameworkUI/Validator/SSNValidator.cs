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
	public class SSNValidator : RegexValidator
	{
		public SSNValidator()
			: base()
		{
			this.Expression = @"^([0-9]{3})(\D*)([0-9]{2})(\D*)([0-9]{4})$";
		}	
	}
}
