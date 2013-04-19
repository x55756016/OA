using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.DAL.Views
{
    public class V_HouseHireList
    {
        public T_OA_HOUSEINFOISSUANCE houseIssueObj;//房屋发布
        public T_OA_HOUSEINFO houseInfoObj;
        public T_OA_HOUSELIST houselistObj;
        public T_OA_DISTRIBUTEUSER distrbuteObj;//发布对象
        public string houseInfo { get; set; }
        public string OWNERID { get; set; }
        public string OWNERCOMPANYID { get; set; }
        public string OWNERDEPARTMENTID { get; set; }
        public string OWNERPOSTID { get; set; }
        public string CREATEUSERID { get; set; }
    }
}
