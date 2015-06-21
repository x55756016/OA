using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.SaaS.BLLCommonServices;
using SMT.HRM.CustomModel;
using SMT.Foundation.Log;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.ServiceModel.Web;
using SMT.HRM.BLL;
using System.Configuration;

namespace SMT.HRM.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EmployeeInfoService
    {
        /// <summary>
        /// 登录验证，并返回员工信息
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="UserPassword"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [OperationContract]
        public V_EMPLOYEEDETAIL getEmployeeInfobyLogin(string employeeid)
        {
            try
            {
                using (EmployeeBLL bll = new EmployeeBLL())
                {
                    V_EMPLOYEEDETAIL employee =  bll.GetEmployeeDetailView(employeeid);
                if (employee == null)
                {
                    Tracer.Debug("通过员工id获取到员工详细信息为空，员工id：" + employeeid);
                    return null;
                }
                return employee;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("通过员工id获取到员工详细信息出错，员工id：" + employeeid+ex.ToString());
                throw ex;
            }
        }

        private static string strConfig = string.Empty;
        /// <summary>
        /// 前台UI日志记录
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="LogMsg"></param>
        /// <returns></returns>
        [OperationContract]
        public void RecordUILog(string employee,string LogMsg)
        {
            try
            {
                if (string.IsNullOrEmpty(strConfig))
                {
                    strConfig = ConfigurationManager.AppSettings["LogErrorToFile"].ToString();
                }
                if (strConfig == "Yes")
                {
                    Tracer.Debug("员工：" + employee + " 前台发生的日志：" + LogMsg);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("通过员工id获取到员工详细信息出错，员工id：" + employee + ex.ToString());
                //throw ex;
            }
        }

   
    }

}
