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
using System.Data.Objects;

namespace SMT.HRM.BLL
{
    public class PerformanceRecordBll : BaseBll<T_HR_PERFORMANCERECORD>
    {
        public bool CheckRecordIsSummarize(string recordId)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_HR_PERFORMANCEDETAIL>()
                           where a.T_HR_KPIRECORD.KPIRECORDID == recordId
                           
                           select a;
                if (ents.Count() > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
            return false;
        }
    }
}
