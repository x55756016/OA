using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;
using System.Data.Entity;
using SMT_HRM_EFModel;

namespace SMT.HRM.CustomModel
{
    public class V_ADJUSTLEAVE
    {
        public V_ADJUSTLEAVE() { }

        public T_HR_ADJUSTLEAVE T_HR_ADJUSTLEAVE { get; set; }
        public string VacationType { get; set; }
        public decimal? VacationDays { get; set; }
    }
}
