using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_HouseHirer
    {
        public T_OA_HIREAPP houseObj;
        public T_OA_HOUSEINFO houseInfoObj;
        public string houseInfo { get; set; }
    }
}
