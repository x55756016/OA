using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_LicenseBorrow 
    {
        public T_OA_LICENSEUSER licenseUser;
        private string licenseName;
        private string guids;

        public string LicenseName
        {
            get { return licenseName; }
            set { licenseName = value; }
        }
       
        public string Guids
        {
            get { return guids; }
            set { guids = value; }
        }
    }
}
