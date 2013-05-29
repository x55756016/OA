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
using SMT.SaaS.FrameworkUI.Validator;
using SMT.SaaS.FrameworkUI.Common;

namespace SMT.FB.UI.Common
{
    public static class ValidationExtension
    {

        public static RequiredValidator SetRequiredValidation(this FrameworkElement uiElement, ValidatorManager validatorManager, string propertyDisplayName)
         {
             RequiredValidator rv = new RequiredValidator();
             rv.ErrorMessage = "REQUIRED";
             rv.ErrorMessageParameter = propertyDisplayName;
             rv.Manager = validatorManager;
             rv.ManagerName = validatorManager.Name;
             uiElement.SetValue(ValidatorService.ValidatorProperty, rv);

             return rv;
         }
    }

    //public interface IValidator
    //{
    //}
    //public interface IValidatorManager
    //{
    //    public List<IValidator> ValidateAll();
    //}
}
