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
using System.Collections.ObjectModel;
using System.Collections;

namespace SMT.SaaS.FrameworkUI.Validator
{
	public abstract class ValidatorService : DependencyObject
	{
		public static ValidatorBase GetValidator(DependencyObject obj)
		{
			return (ValidatorBase)obj.GetValue(ValidatorProperty);
		}

		public static void SetValidator(DependencyObject obj, ValidatorBase value)
		{
			obj.SetValue(ValidatorProperty, value);
		}
		//public static void SetValidators(DependencyObject obj, ValidatorBase value)
		//{
		//    obj.SetValue(ValidatorsProperty, new ValidatorBase(value));
		//}
		//public static void SetValidators(DependencyObject obj, ValidatorBase value)
		//{
		//    obj.SetValue(ValidatorsProperty, new ValidatorBase() { value });
		//}

		// Using a DependencyProperty as the backing store for Validator.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValidatorProperty =
			DependencyProperty.RegisterAttached("Validator", typeof(ValidatorBase), typeof(ValidatorService), new PropertyMetadata(ValidatorsPropertyChangedCallback));

		private static void ValidatorsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var v = e.NewValue as ValidatorBase;
			if (String.IsNullOrEmpty(v.ManagerName))
			{
				System.Diagnostics.Debug.WriteLine("Required Property 'ManagerName' required for Validator");
				throw new Exception("Required Property 'ManagerName' required for Validator");
			}

			v.Initialize(d as FrameworkElement);
		}
        public static System.Resources.ResourceManager _ResourceMgr;
        public static System.Resources.ResourceManager ResourceMgr
        {
            get
            {
                if (_ResourceMgr == null)
                {
                    _ResourceMgr = SMT.SaaS.Globalization.Localization.ResourceMgr;
                }
                return _ResourceMgr;
            }
            set
            {
                _ResourceMgr = value;
            }
        }
	}
}
