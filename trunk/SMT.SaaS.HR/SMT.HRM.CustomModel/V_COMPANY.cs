using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_COMPANY
    {
        public string COMPANYID { get; set; }
        public string CNAME { get; set; }
        public string COMPANRYCODE { get; set; }
        public string ENAME { get; set; }
        public string FATHERID { get; set; }
        public string FATHERTYPE { get; set; }
        public string FATHERCOMPANYID { get; set; }
        public decimal? SORTINDEX { get; set; }
        public string BRIEFNAME { get; set; }
        public string CHECKSTATE { get; set; }
        public string EDITSTATE { get; set; }
        public string COMPANYTYPE { get; set; }
    }
}
