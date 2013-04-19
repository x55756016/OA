using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_PROJECTPOINT
    {
        public V_PROJECTPOINT() { }
        public string CheckProject { get; set; }
        public decimal? CheckProjectScore { get; set; }
        public List<V_POINTSET> PointList { get; set; }
    }
}
