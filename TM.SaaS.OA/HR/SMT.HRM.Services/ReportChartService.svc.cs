using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.HRM.CustomModel.Reports;
using System.Collections.Generic;
using SMT.HRM.BLL.Report;
using TM_SaaS_OA_EFModel;
namespace SMT.HRM.Services.Report
{
    [ServiceContract(Namespace = "")]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ReportChartService
    {
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            return;
        }

        // 在此处添加更多操作并使用 [OperationContract] 标记它们
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
        [OperationContract]
        public List<EmployeeMarriage> NewGetAllEmployeeentryInfo(string strCompanyid, DateTime StartDate, DateTime EndDate)
        {
            //var ents =object
            try
            {
                using (ChartReportsBll bllOvertimeReward = new ChartReportsBll())
                {
                    var ents = bllOvertimeReward.NewGetAllEmployeeentryInfo(strCompanyid, StartDate, EndDate);
                    if (ents != null)
                    {
                        return ents;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [OperationContract]
        public List<PieEmployeece> GetPieEmployeeceInfo(string Companyid, string Style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                using (ChartReportsBll bllOvertimeReward = new ChartReportsBll())
                {
                    var ents = bllOvertimeReward.GetPieEmployeeceInfo(Companyid,Style,StartDate, EndDate, filterString, paras, userID);
                    if (ents!=null)
                    {
                        return ents;
                    }
                    else
                    {
                        return null;
                    } 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [OperationContract]
        public List<PieEmployeece> GetSexPieEmployeeceInfo(string Companyid, string style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                using(ChartReportsBll bllOvertimeReward = new ChartReportsBll())
	            {
		            var ents = bllOvertimeReward.GetSexPieEmployeeceInfo(Companyid,style, StartDate, EndDate, filterString, paras, userID);
                    if (ents!=null)
                    {
                        return ents;
                    }
                    else
                    {
                        return null;
                    } 
	            }
                           
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [OperationContract]
        public List<PieEmployeece> GetEducationPieEmployeeceInfo(string Companyid, string style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                using (ChartReportsBll bllOvertimeReward=new ChartReportsBll())
                {
                    var ents = bllOvertimeReward.GetEducationPieEmployeeceInfo(Companyid,style, StartDate, EndDate, filterString, paras, userID);
                    if (ents!=null)
                    {
                        return ents.Count()>0 ? ents : null;
                    }
                    else
                    {
                        return null;
                    } 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }    
        }

        [OperationContract]
        public List<PieEmployeece> GetAgePieEmployeeceInfo(string Companyid, string Style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                using (ChartReportsBll bllOvertimeReward=new ChartReportsBll())
                {
                    var ents = bllOvertimeReward.GetAgePieEmployeeceInfo(Companyid,Style, StartDate, EndDate, filterString, paras, userID);
                    if (ents !=null)
                    {
                        return ents.Count() > 0 ? ents : null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [OperationContract]
        public List<PieEmployeece> GetLengthServicePie(string Companyid, string Style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                using (ChartReportsBll bllOvertimeReward=new ChartReportsBll())
                {
                    var ents = bllOvertimeReward.GetLengthServicePie(Companyid,Style,StartDate, EndDate, filterString, paras, userID);
                    if (ents !=null)
                    {
                        return ents.Count() > 0 ? ents : null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }

        [OperationContract]
        public List<PieEmployeece> GetAllCompanyInfo(string Companyid, string Style, string StartDate, string EndDate, string filterString, IList<object> paras, string userID)
        {
            try
            {
                using (ChartReportsBll bllOvertimeReward=new ChartReportsBll())
                {
                    var ents = bllOvertimeReward.GetAllCompanyInfo(Companyid,Style,StartDate, EndDate, filterString, paras, userID);
                    if (ents !=null)
                    {
                        return ents.Count() > 0 ? ents : null;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
        }
    }
}
