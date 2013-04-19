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

namespace SMT.HRM.Services
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
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

   
    }

}
