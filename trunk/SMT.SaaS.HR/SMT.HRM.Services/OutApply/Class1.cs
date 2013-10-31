using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using SMT.HRM.BLL;
using SMT_HRM_EFModel;

namespace SMT.HRM.Services
{
    public partial class OutAppliecrecordService
    {
        #region T_HR_OUTAPPLYCONFIRM 员工外出确认
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
        public List<T_HR_OUTAPPLYCONFIRM> EmployeeOutApplyConfrimPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID)
        {
            using (OutApplyConfirmBLL bll = new OutApplyConfirmBLL())
            {
                var ents = bll.EmployeeOutApplyConfirmPaging(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, strCheckState, strOwnerID);

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
        public T_HR_OUTAPPLYCONFIRM GetOutApplyConfrimByID(string strOverTimeRecordId)
        {
            using (OutApplyConfirmBLL bllOverTimeRecord = new OutApplyConfirmBLL())
            {
                return bllOverTimeRecord.GetOutApplyConfirmByID(strOverTimeRecordId);
            }
        }

        /// <summary>
        /// 新增员工加班信息
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string AddOutApplyConfrim(T_HR_OUTAPPLYCONFIRM entOTRd)
        {
            using (OutApplyConfirmBLL bllOverTimeRecord = new OutApplyConfirmBLL())
            {
                return bllOverTimeRecord.AddOutApplyConfirm(entOTRd);
            }
        }

        /// <summary>
        /// 修改员工加班信息
        /// </summary>
        /// <param name="entOTRd"></param>
        /// <returns></returns>
        [OperationContract]
        public string UpdateOutApplyConfrim(T_HR_OUTAPPLYCONFIRM entOTRd)
        {
            using (OutApplyConfirmBLL bllOverTimeRecord = new OutApplyConfirmBLL())
            {
                if (bllOverTimeRecord.UpdateOutApplyConfirm(entOTRd) == 1)
                    return "OK";
                else return "Fail";
            }
        }

        /// <summary>
        /// 删除员工加班信息(注：仅在未提交状态下，方可进行物理删除)
        /// </summary>
        /// <param name="strOverTimeRecordId">主键索引</param>
        /// <returns></returns>
        [OperationContract]
        public bool DeleteOutApplyConfrim(string[] strOverTimeRecordId)
        {
            using (OutApplyConfirmBLL bll = new OutApplyConfirmBLL())
            {
                int rslt = bll.DeleteOutApplyConfirm(strOverTimeRecordId);
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
        public string AuditOutApplyConfrim(string strOverTimeRecordID, string strCheckState)
        {
            using (OutApplyConfirmBLL bllOverTimeRecord = new OutApplyConfirmBLL())
            {
                string rslt = bllOverTimeRecord.AuditOutApplyConfirm(strOverTimeRecordID, strCheckState);
                return rslt;
            }
        }
        #endregion


    }
}