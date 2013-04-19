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
	public class RequiredValidator : ValidatorBase
	{
        public RequiredValidator():base()
        {
            IsRequired = true;            
        }
 
		protected override bool ValidateControl()
		{
            return true;
           
		}
	}
}
