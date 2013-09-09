using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using SMT.HRM.BLL;
using SMT_HRM_EFModel;

namespace SMT.HRM.Services
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“OutAppliecrecordService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 OutAppliecrecordService.svc 或 OutAppliecrecordService.svc.cs，然后开始调试。
    [ServiceContract(Namespace = "")]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OutAppliecrecordService 
    {
        #region T_HR_EMPLOYEEOUTAPPLIECRECORD 员工加班记录服务
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的出差记录信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        [OperationContract]
        public List<T_HR_EMPLOYEEOUTAPPLIECRECORD> EmployeeOverTimeRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
        {
            using (OutAppliecationBLL bllOverTimeRecord = new OutAppliecationBLL())
            {
                var ents = bllOverTimeRecord.EmployeeOverTimeRecordPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID);

                if (ents == null)
                {
                    return null;
                }

                return ents.ToList();
            }
        }

        /// <summary>
        /// 根据主键索引，获取员工加班信息
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public T_HR_EMPLOYEEOUTAPPLIECRECORD GetOverTimeRdByID(string strOverTimeRecordId)
        {
            using (OutAppliecationBLL bllOverTimeRecord = new OutAppliecationBLL())
            {
                return bllOverTimeRecord.GetOverTimeRdByID(strOverTimeRecordId);
            }
        }

        /// <summary>
        /// 新增员工加班信息
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddOverTimeRd(T_HR_EMPLOYEEOUTAPPLIECRECORD entOTRd)
        {
            using (OutAppliecationBLL bllOverTimeRecord = new OutAppliecationBLL())
            {
                return bllOverTimeRecord.OverTimeRecordAdd(entOTRd);
            }
        }

        /// <summary>
        /// 修改员工加班信息
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string ModifyOverTimeRd(T_HR_EMPLOYEEOUTAPPLIECRECORD entOTRd)
        {
            using (OutAppliecationBLL bllOverTimeRecord = new OutAppliecationBLL())
            {
                return bllOverTimeRecord.ModifyOverTimeRd(entOTRd);
            }
        }

        /// <summary>
        /// 删除员工加班信息(注：仅在未提交状态下，方可进行物理删除)
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public bool RemoveOverTimeRd(string[] strOverTimeRecordId)
        {
            using (OutAppliecationBLL bllOverTimeRecord = new OutAppliecationBLL())
            {
                int rslt = bllOverTimeRecord.DeleteOverTimeRd(strOverTimeRecordId);
                return (rslt > 0);
            }
        }

        /// <summary>
        /// 审核员工加班信息
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <param name="strCheckState">审核状态</param>
        /// <returns></returns>
        [OperationContract]
        public string AuditOverTimeRd(string strOverTimeRecordID, string strCheckState)
        {
            using (OutAppliecationBLL bllOverTimeRecord = new OutAppliecationBLL())
            {
                string rslt = bllOverTimeRecord.AuditOverTimeRd(strOverTimeRecordID, strCheckState);
                return rslt;
            }
        }
        #endregion
    }
}
