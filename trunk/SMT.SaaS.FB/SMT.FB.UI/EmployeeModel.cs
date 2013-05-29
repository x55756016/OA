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

namespace SMT.FB.UI
{
    public class EmployeeModel
    {
        private string _name;
        [Display(Name = "名字", Description = "必填字段")]
        [Required(ErrorMessage = "名字必填")]
        public string Name
        {
            get { return _name; }
            set
            {
                /*
                 * Validator.ValidateProperty() - 用于决定指定的属性是否通过了验证（根据属性的 DataAnnotations 的 Attribute 做判断）。以及当其没有通过验证时，抛出异常
                 */
                Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = "Name" });
                _name = value;
            }
        }

        private double _salary;
        [Display(Name = "薪水", Description = "薪水介于 0 - 10000 之间")]
        [Range(0, 10000)]
        public double Salary
        {
            get { return _salary; }
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = "Salary" });
                _salary = value;
            }
        }

        public DateTime DateOfBirth { get; set; }
    }
}
