using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class SumPerformanceRecordBll : BaseBll<T_HR_SUMPERFORMANCERECORD>
    {

        public IQueryable<T_HR_SUMPERFORMANCERECORD> GetSumPerformancePaging(int pageIndex, int pageSize, string sort, string filterString, 
            string[] paras, ref int pageCount, string sType, string sValue, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_KPIRECORD");
            //员工入职审核通过再显示。

            ///TOADD:员工编辑状态为生效  EDITSTATE==Convert.ToInt32(EditStates.Actived).ToString();
            ///

            IQueryable<T_HR_SUMPERFORMANCERECORD> ents = dal.GetObjects();
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects()
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on o.CREATECOMPANYID equals d.DEPARTMENTID
                           join c in dal.GetObjects <T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue
                           select o;
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join d in dal.GetObjects <T_HR_DEPARTMENT>() on o.CREATECOMPANYID equals d.DEPARTMENTID
                           where d.DEPARTMENTID == sValue
                           select o;
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_SUMPERFORMANCERECORD>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }

        public IQueryable<T_HR_PERFORMANCERECORD> GetPerformanceAllBySumID(string sumID)
        {
            var q = from ent in dal.GetObjects <T_HR_PERFORMANCERECORD>()
                    where ent.T_HR_SUMPERFORMANCERECORD.SUMID == sumID
                    select ent;
            return q;
        }

        public T_HR_PERFORMANCERECORD GetPerformanceRecordByID(string recordID)
        {
            try
            {
                var ents = from a in dal.GetObjects <T_HR_PERFORMANCERECORD>()
                           where a.PERFORMANCEID == recordID
                           select a;
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetPerformanceRecordByID:" + ex.Message);
                throw ex;
            }
            return null;
        }
    }
}
