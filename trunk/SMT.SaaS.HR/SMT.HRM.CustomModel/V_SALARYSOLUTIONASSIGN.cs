using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_SALARYSOLUTIONASSIGN
    {
        public V_SALARYSOLUTIONASSIGN()
        { 
        }
        public string AssignObjectName { get; set; }

        public SMT_HRM_EFModel.T_HR_SALARYSOLUTIONASSIGN SalarySolutionAssign { get; set; }
    }
}
