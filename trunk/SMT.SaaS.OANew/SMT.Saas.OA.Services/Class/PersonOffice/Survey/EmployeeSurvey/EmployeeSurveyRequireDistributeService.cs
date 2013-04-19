using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.SaaS.OA.DAL.Views;
using SMT.SaaS.OA.BLL;
using System.ServiceModel;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {
        #region 新增
        /// <summary>
        /// 新增员工调查发布申请和分发范围
        /// </summary>
        [OperationContract]
        public bool AddRequireDistribute(V_EmployeeSurveyRequireDistribute AddDistribute)
        {
            try
            {
                if (AddDistribute != null)
                {
                    using (EmployeeSurveyRequireDistributeBll distributeBll = new EmployeeSurveyRequireDistributeBll())
                    {
                        return distributeBll.AddRequireDistribute(AddDistribute);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查发布EmployeeSurveyRequireDistributeService-AddRequireDistribute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        #endregion

        #region 修改
        /// <summary>
        /// 
        /// </summary>
        [OperationContract]
        public bool UpdRequireDistribute(V_EmployeeSurveyRequireDistribute UpdDistribute)
        {
            try
            {
                if (UpdDistribute != null)
                {
                    using (EmployeeSurveyRequireDistributeBll distributeBll = new EmployeeSurveyRequireDistributeBll())
                    {
                        return distributeBll.UpdRequireDistribute(UpdDistribute);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("员工调查发布EmployeeSurveyRequireDistributeService-UpdRequireDistribute" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }

        }
        #endregion

        #region 查询
        /// <summary>
        /// 员工调查发布申请子页面加载取数据
        /// </summary>
        [OperationContract]
        public V_EmployeeSurveyRequireDistribute GetDistributeData(string distributeId)
        {
                if (!string.IsNullOrEmpty(distributeId))
                {
                    using (EmployeeSurveyRequireDistributeBll distributeBll = new EmployeeSurveyRequireDistributeBll())
                    {
                        return distributeBll.GetDistributeData(distributeId);
                    }
                }
                return null;
            }

        #endregion

        #region 查询结果
        #endregion
    }
}