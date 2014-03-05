using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class CommBll<TEntity>
    {
        /// <summary>
        /// 修改实体审核状态
        /// </summary>
        /// <param name="strEntityName">实体名</param>
        /// <param name="EntityKeyName">主键名</param>
        /// <param name="EntityKeyValue">主键值</param>
        /// <param name="CheckState">审核状态</param>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            Tracer.Debug("mmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm" );
            try
            {
                using (CommDaL<TEntity> dal = new CommDaL<TEntity>())
                {
                    int intResult = 0;
                    Tracer.Debug("进入了COMMONBLL，实体名：" + strEntityName + "，DateTime:" + System.DateTime.Now.ToString());
                    Tracer.Debug("实体ID名：" + EntityKeyName + "实体主键值：" + EntityKeyValue);
                    Tracer.Debug("审核的状态：" + CheckState);
                    switch (strEntityName)
                    {
                        case "T_OA_BUSINESSTRIP"://出差申请
                            using (TravelmanagementBLL TravelRequestBll = new TravelmanagementBLL())
                            {
                                intResult = TravelRequestBll.UpdateTravelRequestFromEngine(EntityKeyValue, CheckState);
                            }
                            break;
                        //case "T_OA_BUSINESSREPORT"://出差报告
                        //    MissionReportsBLL TravelReportBll = new MissionReportsBLL();
                        //    intResult = TravelReportBll.UpdateTravelReportFromEngine(EntityKeyValue, CheckState);
                        //    break;
                        case "T_OA_TRAVELREIMBURSEMENT"://出差报销
                            using (TravelReimbursementBLL TravelReimbursementBll = new TravelReimbursementBLL())
                            {
                                intResult = TravelReimbursementBll.UpdateTravelReimbursementFromEngine(EntityKeyValue, CheckState);
                            }
                            break;
                        case "T_OA_SENDDOC":
                            using (BumfCompanySendDocManagementBll sendDocBll = new BumfCompanySendDocManagementBll())
                            {
                                intResult = sendDocBll.UpdateCheckStateBumfEngine(EntityKeyValue, CheckState);
                            }
                            break;
                        default:
                            intResult = dal.UpdateCheckState(strEntityName, EntityKeyName, EntityKeyValue, CheckState);
                            break;
                    }
                    return intResult;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
