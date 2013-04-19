using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
using SMT.SaaS.OA.BLL;
using SMT_OA_EFModel;

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
    }
}
