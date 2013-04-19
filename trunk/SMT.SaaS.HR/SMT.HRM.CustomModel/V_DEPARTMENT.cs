using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.CustomModel
{
    public class V_DEPARTMENT
    {
        public string DEPARTMENTID { get; set; }
        public string DEPARTMENTNAME { get; set; }
        public string DEPARTMENTDICTIONARYID { get; set; }
        public string CHECKSTATE { get; set; }
        public string EDITSTATE { get; set; }
        public string DEPARTMENTBOSSHEAD { get; set; }
        public string FATHERID { get; set; }
        public string FATHERTYPE { get; set; }
        public string COMPANYID { get; set; }
        public string CNAME { get; set; }
        public decimal? SORTINDEX { get; set; }
    }
}
