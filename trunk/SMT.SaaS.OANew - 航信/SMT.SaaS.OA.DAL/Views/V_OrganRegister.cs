using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_OrganRegister
    {
        public T_OA_ORGANIZATION organ;
        private string guids;

        public string Guids
        {
            get { return guids; }
            set { guids = value; }
        }
    }
}
