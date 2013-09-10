using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
using SMT.SaaS.OA.DAL.Views;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region 新增
        /// <summary>
        /// 新增员工调查申请
        /// </summary>
        [OperationContract]
        public bool AddRequire(V_EmployeeSurveyApp addApp)
        {

            if (!string.IsNullOrEmpty(addApp.MasterId))
            {
                using (EmployeeSurveyAppBll addBll = new EmployeeSurveyAppBll())
                {
                    return addBll.AddRequire(addApp);
                }
            }
            return false;
        }
        #endregion

        #region 修改
        /// <summary>
        /// 修改员工调查申请
        /// </summary>
        [OperationContract]
        public bool UpdRequire(V_EmployeeSurveyApp updApp)
        {
            if (!(string.IsNullOrEmpty(updApp.MasterId)))
            {
                using (EmployeeSurveyAppBll updBll = new EmployeeSurveyAppBll())
                {
                    return updBll.UpdRequire(updApp);
                }
            }
            return false;
        }
        #endregion

        #region 查询
        /// <summary>
        /// 员工调查申请子页面加载时查询
        /// </summary>
        [OperationContract]
        public V_EmployeeSurveyApp GetRequireData(string requireKey)
        {
            using (EmployeeSurveyAppBll getBll = new EmployeeSurveyAppBll())
            {
                V_EmployeeSurveyApp data = getBll.GetRequireData(requireKey);
                return data!=null ? data : null;
            }
        }

        /// <summary>
      /// 选择员工调查申请时查询，并排序分页
      /// </summary>
       [OperationContract]
        public List<V_EmployeeSurveyApp> GetEmployeeSurveyAppByCheckstateAndDate(int pageCount, int pageIndex, int pageSize, string checkstate, DateTime[] datetimes)
        {
            if (datetimes.Count() > 1)
            {
                IQueryable<V_EmployeeSurveyApp> returnList;
                using (EmployeeSurveyAppBll getBll = new EmployeeSurveyAppBll())
                {
                    
                    returnList = getBll.GetEmployeeSurveyAppByCheckstateAndDate(pageCount, pageIndex, pageSize, checkstate, datetimes);
                    return returnList.Count() > 0 ? returnList.ToList() : null;
                }
            }
            return null;
        }

        #endregion
    }
}