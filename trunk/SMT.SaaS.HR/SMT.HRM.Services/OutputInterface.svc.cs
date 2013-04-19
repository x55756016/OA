using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT.HRM.BLL;
using SMT.HRM.CustomModel;
using System.ServiceModel.Activation;

namespace SMT.HRM.Services
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“OutputInterface”。
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OutputInterface 
    {
        [OperationContract]
        public void DoWork()
        {
        }

        /// <summary>
        /// 获取指定时间内指定绩效级别的员工信息
        /// </summary>
        /// <param name="pageIndex">当前页数，(必填)</param>
        /// <param name="pageSize">每页个数，(必填)</param>
        /// <param name="sort">排序字段，可为空</param>
        /// <param name="filterString">过滤条件，没有请填空</param>
        /// <param name="paras">过滤条件值，没有请填空</param>
        /// <param name="Count">总个数,(必填)</param>
        /// <param name="pageCount">总页数,(必填)</param>
        /// <param name="sType">机构类型：公司“Company”，部门“Department",岗位：”Post“(必填)</param>
        /// <param name="sValue">机构id，对应的公司，部门，岗位id(必填)</param>
        /// <param name="userID">当前操作用户id(必填)</param>
        /// <param name="startTime">开始时间(必填)</param>
        /// <param name="endTime">结束时间(必填)</param>
        /// <param name="sartlevel">绩效等级起始值(必填)</param>
        /// <param name="endlevel">绩效等级结束值(必填)</param>
        /// <returns></returns>
        [OperationContract]
        public List<V_EMPLOYEEVIEW> GetPerformenceEmployee(int pageIndex, int pageSize, string sort, string filterString, string[] paras, ref int Count, ref int pageCount, string sType, string sValue, string userID, string startTime, string endTime, int sartlevel, int endlevel)
        {
            using (SumPerformanceBll bll = new SumPerformanceBll())
            {

                var pents = bll.GetPerformenceEmployee(pageIndex, pageSize, sort, filterString, paras, ref Count, ref pageCount, sType, sValue, userID, startTime, endTime, sartlevel, endlevel);
                return pents;

            }
        }
    }
}
