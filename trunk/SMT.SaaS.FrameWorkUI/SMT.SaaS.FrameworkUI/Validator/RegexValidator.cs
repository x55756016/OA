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
    using System.Text.RegularExpressions;

    public class RegexValidator : ValidatorBase
    {
        public RegexValidator()
            : base()
        {
        }

        public Regex RegExpression
        {
            get { return (Regex)GetValue(RegExpressionProperty); }
            set { SetValue(RegExpressionProperty, value); }
        }

        //public bool isRequire { get; set; }

        // Using a DependencyProperty as the backing store for RegExpression.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RegExpressionProperty =
            DependencyProperty.Register("RegExpression", typeof(Regex), typeof(RegexValidator), new PropertyMetadata(null));

        public string Expression
        {
            get { return (string)GetValue(ExpressionProperty); }
            set { SetValue(ExpressionProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Expression.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpressionProperty =
            DependencyProperty.Register("Expression", typeof(string), typeof(RegexValidator), new PropertyMetadata(ExpressionPropertyChangedCallback));

        public static void ExpressionPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                d.SetValue(RegExpressionProperty, new Regex(e.NewValue.ToString()));
            }
            catch
            {
                d.SetValue(RegExpressionProperty, null);
            }
            
        }


        protected override bool ValidateControl()
        {
            if (ElementToValidate is TextBox && RegExpression != null)
            {
                TextBox box = ElementToValidate as TextBox;
                if (String.IsNullOrEmpty(box.Text)) return !IsRequired;
                return RegExpression.Match(box.Text).Success;
            }
            return true;
        }
    }
}