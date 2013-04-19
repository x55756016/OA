using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_POINTSET
    {
        public V_POINTSET() { }
        public string CheckPointSetID { get; set; }
        public string CheckProjectID { get; set; }
        public string CheckEmployeeType { get; set; }
        public string CheckPoint { get; set; }
        public string CheckPointDes { get; set; }
        public decimal? CheckPointScore { get; set; }
        public List<SMT_HRM_EFModel.T_HR_CHECKPOINTLEVELSET> LeavelList {get;set;}
        public decimal FirstScore { get; set; }
        public decimal SecondScore { get; set; }
    }
}
