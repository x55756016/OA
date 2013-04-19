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
using System.Collections.Generic;
using System.Linq;

namespace SMT.SaaS.FrameworkUI.Validator
{
	public class ValidatorManager : FrameworkElement
	{
		public class ManagerList
		{
			public FrameworkElement Element { get; set; }
			public ValidatorBase Validator { get; set; }
		}

		List<ManagerList> items = new List<ManagerList>();

        public ValidatorManager()
        {
            //defaults
			Indicator = new DefaultIndicator();
            this.InvalidBorder = new SolidColorBrush(Colors.Red);
            this.InvalidBorderThickness = new Thickness(1);
            
        }

		public void Register(FrameworkElement elementToValidate, ValidatorBase v)
		{
			items.Add(new ManagerList() { Element = elementToValidate, Validator = v });
		}

        /// <summary>
        /// Validate all validators within the group
        /// </summary>
        /// <returns></returns>
		public List<ValidatorBase> ValidateAll()
		{
			List<ValidatorBase> errors = new List<ValidatorBase>();
			
			foreach (var item in items)
			{
				if (!item.Validator.Validate(true))
				{
					errors.Add(item.Validator);
                    break;//遇到错误就返回，这样就不会全部标红了ljx 2011-2-12
				}
			}
			return errors;
		}

		public static string GetErrorString(IEnumerable<string> list, string delimiter)
		{
			string s = "";
			foreach (var item in list)
			{
				s += item + delimiter;
			}
			return s.TrimEnd(delimiter.ToCharArray());
		}

		public static string GetErrorString(IEnumerable<ValidatorBase> list, string delimiter)
		{
			return GetErrorString(
				list.Select(n => n.ErrorMessage).ToList(), delimiter);
		}

		public IIndicator Indicator { get; set; }

        public Brush InvalidBackground
        {
            get;
            set;
        }

        public Brush InvalidBorder
        {
            get;
            set;
        }

        public Thickness InvalidBorderThickness
        {
            get;
            set;
        }
        
	}
}
