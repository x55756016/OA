using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_ATTENDANCESOLUTIONASIGN
    {
        public string ATTENDANCESOLUTIONASIGNID { get; set; }
        public string ATTENDANCESOLUTIONNAME { get; set; }
        public string ASSIGNEDOBJECTTYPE { get; set; }
        public string ASSIGNEDOBJECTID { get; set; }
        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }
        public string CHECKSTATE { get; set; }
    }
}
