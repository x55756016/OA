using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_EmployeeSurveyRequireDistribute
    {     
        public string RequireDistributeId { get; set; }
        public string RequireId { get; set; }


        public T_OA_REQUIREDISTRIBUTE requiredistributeEntity { get; set; }
        public IEnumerable<T_OA_DISTRIBUTEUSER> distributeuserList { get; set; }
        public IEnumerable<T_OA_DISTRIBUTEUSER> oldDistributeuserList { get; set; }

    }
}
