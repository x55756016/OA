using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
using SMT.SaaS.OA.BLL;
using SMT_OA_EFModel;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AgentServices
    {
        [OperationContract]
        public T_HR_EMPLOYEE GetQueryAgent(string UserId, string ModCode)
        {
            ProxySettingsBLL psbll = new ProxySettingsBLL();
            PersonnelServiceClient psc = new PersonnelServiceClient();

            if (psbll.IsExistAGENTForUser(UserId))
            {
                T_OA_AGENTSET AgentList = psbll.GetQueryAgent(UserId, ModCode);

                T_HR_EMPLOYEE employee = null;

                if (AgentList != null)
                {
                    T_HR_EMPLOYEEPOST[] tmps = psc.GetEmployeePostByPostID(AgentList.AGENTPOSTID);
                    if (tmps != null && tmps.Count() > 0)
                    {
                        employee = tmps[0].T_HR_EMPLOYEE;
                    }
                    else
                    {
                        T_HR_EMPLOYEE[] tmp = psc.GetEmployeeLeader(UserId, 0);
                        if (tmp != null && tmp.Count() > 0)
                        {
                            employee = tmp[0];//返回该岗位员工的直接上级
                        }
                    }
                }
                else
                {
                    T_HR_EMPLOYEE[] tmp = psc.GetEmployeeLeader(UserId, 0);
                    if (tmp != null && tmp.Count() > 0)
                    {
                        employee = tmp[0];//返回该岗位员工的直接上级
                    }
                }
                return employee;
            }
            return null;
        }

        [OperationContract]
        public string UpdateEntityXML(string Formid, string OldString, string ReplaceString)
        {
            try
            {
                TravelReimbursementBLL bll=new TravelReimbursementBLL();
                ReplaceString = (from ent in bll.dal.GetObjects()
                                 where ent.TRAVELREIMBURSEMENTID == Formid
                                select ent.NOBUDGETCLAIMS).FirstOrDefault();
                if (string.IsNullOrEmpty(ReplaceString))
                {
                    Tracer.Debug("出差报销提交审核替换元数据单号，获取的单号为空：" + ReplaceString);
                    return "";
                }
                //更新元数据里的报销单号
                SMT.SaaS.BLLCommonServices.FlowWFService.ServiceClient client =
                new SaaS.BLLCommonServices.FlowWFService.ServiceClient();
                Tracer.Debug("开始调用元数据获取接口：FlowWFService.GetMetadataByFormid(" + Formid + ")");
                string xml = string.Empty;
                xml = client.GetMetadataByFormid(Formid);
                Tracer.Debug("获取到的元数据：" + xml);
                xml = xml.Replace("自动生成", ReplaceString);
                Tracer.Debug("替换单号后的XML:" + xml);
                bool flag = client.UpdateMetadataByFormid(Formid, xml);
                if (flag)
                {
                    Tracer.Debug("出差报销元数据替换单号成功：" + ReplaceString);
                    return "";
                }
                else
                {
                    Tracer.Debug("出差报销元数据替换单号UpdateMetadataByFormid返回false：Formid：" + Formid
                        + OldString
                        + ReplaceString);
                    return "";
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                return "";
            }

        }
    }
}
