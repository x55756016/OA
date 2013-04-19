using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL
{
    public class V_SystemNotice
    {
        public string FormId { get; set; }
        public string Formtype { get; set; }
        public string FormTitle { get; set; }
        public DateTime FormDate { get; set; }

        //public V_SystemNotice(string formid,string formtype,string formtitle,DateTime dt)
        //{
        //    this.FormId = formid;
        //    this.Formtype = formtype;
        //    this.FormTitle = formtitle;
        //    this.FormDate = dt;
        //}

    }
}
