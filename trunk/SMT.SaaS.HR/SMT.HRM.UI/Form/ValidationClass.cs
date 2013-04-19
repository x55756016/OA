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
using System.ComponentModel.DataAnnotations;

namespace SMT.HRM.UI.Form
{
    public partial class ValidationClass
    {       
        private string USERNAMEField;


        [System.ComponentModel.DataAnnotations.Display(Name = "First Name", Description = "Enter First Name")]
        [System.Runtime.Serialization.DataMemberAttribute()]
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "First name can't be empty!")]
        //[System.ComponentModel.DataAnnotations.StringLength(0)]
        public string USERNAME
        {
            get
            {
                return this.USERNAMEField;
            }
            set
            {
                ValidationContext context  = new ValidationContext(this, null, null) { MemberName = "USERNAME" };
                Validator.ValidateProperty(value, context);

                //if (string.IsNullOrEmpty(value))throw new System.Exception("不能为空");
                if ((object.ReferenceEquals(this.USERNAMEField, value) != true))
                {
                    this.USERNAMEField = value;
                }
            }
        }
    }
}
