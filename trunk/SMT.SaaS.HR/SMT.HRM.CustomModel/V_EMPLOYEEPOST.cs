using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;
using System.Data.Entity;
using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
    public class V_EMPLOYEEPOST : EntityObject
    {
        public T_HR_EMPLOYEE T_HR_EMPLOYEE { get; set; }
        public List<T_HR_EMPLOYEEPOST> EMPLOYEEPOSTS { get; set; }
    }
}
