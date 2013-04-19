using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class AssessmentFormDetailBLL : BaseBll<T_HR_ASSESSMENTFORMDETAIL>
    {
        public List<T_HR_ASSESSMENTFORMDETAIL> GetAssessmentFormDetailByMasterID(string masterID)
        {
            var ent = dal.GetObjects<T_HR_ASSESSMENTFORMDETAIL>().Include("T_HR_CHECKPOINTSET").Include("T_HR_ASSESSMENTFORMMASTER").Where(s => s.T_HR_ASSESSMENTFORMMASTER.ASSESSMENTFORMMASTERID == masterID);
            return ent.Count() > 0 ? ent.ToList() : null;
        }
    }
}
