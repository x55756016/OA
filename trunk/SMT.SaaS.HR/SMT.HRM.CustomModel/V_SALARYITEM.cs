using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_HRM_EFModel;
namespace SMT.HRM.CustomModel
{
    public class V_SALARYITEM
    {
        public T_HR_SALARYITEM T_HR_SALARYITEM { get; set; }
        public decimal? ORDERNUMBER { get; set; }
    }
}
