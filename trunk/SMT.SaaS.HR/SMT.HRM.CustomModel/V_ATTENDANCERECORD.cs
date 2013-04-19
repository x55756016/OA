using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_ATTENDANCERECORD
    {
        public V_ATTENDANCERECORD() { }
        public string STARTTIME { get; set; }
        public double STARTVALUE { get; set; }
        public string ENDTIME { get; set; }
        public double ENDVALUE { get; set; }
    }
}
