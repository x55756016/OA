using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_HouseInfoIssuance
    {
        public T_OA_HOUSEINFOISSUANCE issuanceObj;
        private string guids;

        public string Guids
        {
            get { return guids; }
            set { guids = value; }
        }
    }
}
