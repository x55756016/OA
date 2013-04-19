
/*
 * 文件名：SchedulingTemplateDetailBLL.cs
 * 作  用：排班模板明细 业务逻辑类
 * 创建人：吴鹏
 * 创建时间：2010-3-10 19:26:57
 * 修改人：
 * 修改时间：
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;

using SMT_HRM_EFModel;
using SMT.HRM.DAL;

namespace SMT.HRM.BLL
{
    public class SchedulingTemplateDetailBLL: BaseBll<T_HR_SCHEDULINGTEMPLATEDETAIL>
    {
        public SchedulingTemplateDetailBLL()
        { }

        #region 获取数据

        /// <summary>
        /// 获取排班模板明细信息
        /// </summary>
        /// <param name="strSchedulingTemplateDetailId">主键索引</param>
        /// <returns></returns>
        public T_HR_SCHEDULINGTEMPLATEDETAIL GetSchedulingTemplateDetailByID(string strTemplateDetailId)
        {
            if (string.IsNullOrEmpty(strTemplateDetailId))
            {
                return null;
            }

            SchedulingTemplateDetailDAL dalSchedulingTemplateDetail = new SchedulingTemplateDetailDAL();
            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();

            if (!string.IsNullOrEmpty(strTemplateDetailId))
            {
                strfilter.Append(" TEMPLATEDETAILID == @0");
                objArgs.Add(strTemplateDetailId);
            }

            T_HR_SCHEDULINGTEMPLATEDETAIL entRd = dalSchedulingTemplateDetail.GetSchedulingTemplateDetailRdByMultSearch(strfilter.ToString(), objArgs.ToArray());
            return entRd;
        }        

        /// <summary>
        /// 根据条件，获取排班模板明细信息
        /// </summary>        
        /// <param name="strTemplateMasterId">排班模板主表主键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns></returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> GetAllSchedulingTemplateDetailRdListByMultSearch(string strTemplateMasterId, string strSortKey)
        {
            SchedulingTemplateDetailDAL dalSchedulingTemplateDetail = new SchedulingTemplateDetailDAL();

            StringBuilder strfilter = new StringBuilder();
            List<string> objArgs = new List<string>();
            string strOrderBy = string.Empty;

            if (!string.IsNullOrEmpty(strTemplateMasterId))
            {
                strfilter.Append(" T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID == @0");
                objArgs.Add(strTemplateMasterId);
            }
            
            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " TEMPLATEDETAILID ";
            }

            var q = dalSchedulingTemplateDetail.GetSchedulingTemplateDetailRdListByMultSearch(strOrderBy, strfilter.ToString(), objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取排班模板明细信息,并进行分页
        /// </summary>
        /// <param name="strTemplateName">排班模板名称</param>
        /// <param name="strTemplateMasterId">排班模板主表主键索引</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>返回排班模板明细信息</returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> GetSchedulingTemplateDetailRdListByMultSearch(string strTemplateMasterId, string strSortKey, 
            int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllSchedulingTemplateDetailRdListByMultSearch(strTemplateMasterId, strSortKey);

            return Utility.Pager<T_HR_SCHEDULINGTEMPLATEDETAIL>(q, pageIndex, pageSize, ref pageCount);
        }

        /// <summary>
        /// 根据考勤方案的主键索引，获取考勤班次信息
        /// </summary>
        /// <param name="strAttendanceSolutionId">考勤方案的主键索引</param>
        /// <returns>返回考勤班次信息</returns>
        public IQueryable<T_HR_SCHEDULINGTEMPLATEDETAIL> GetTemplateDetailRdListByAttendanceSolutionId(string strAttendanceSolutionId)
        {
            SchedulingTemplateDetailDAL dalSchedulingTemplateDetail = new SchedulingTemplateDetailDAL();
            var q = dalSchedulingTemplateDetail.GetTemplateDetailRdListByAttendanceSolutionId(strAttendanceSolutionId);
            return q;
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增排班模板明细信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddTemplateDetail(T_HR_SCHEDULINGTEMPLATEDETAIL entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID == @0");
                strFilter.Append(" && T_HR_SHIFTDEFINE.SHIFTDEFINEID == @1");
                strFilter.Append(" && SCHEDULINGDATE == @2");

                objArgs.Add(entTemp.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID);
                objArgs.Add(entTemp.T_HR_SHIFTDEFINE.SHIFTDEFINEID);
                objArgs.Add(entTemp.SCHEDULINGDATE);

                SchedulingTemplateDetailDAL dalSchedulingTemplateDetail = new SchedulingTemplateDetailDAL();
                flag = dalSchedulingTemplateDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return string.Empty;
                }

                T_HR_SCHEDULINGTEMPLATEDETAIL ent = new T_HR_SCHEDULINGTEMPLATEDETAIL();
                Utility.CloneEntity<T_HR_SCHEDULINGTEMPLATEDETAIL>(entTemp, ent);
                ent.T_HR_SCHEDULINGTEMPLATEMASTERReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SCHEDULINGTEMPLATEMASTER", "TEMPLATEMASTERID", entTemp.T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID);
                ent.T_HR_SHIFTDEFINEReference.EntityKey =
                    new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_SHIFTDEFINE", "SHIFTDEFINEID", entTemp.T_HR_SHIFTDEFINE.SHIFTDEFINEID);

                Utility.RefreshEntity(ent);

                dalSchedulingTemplateDetail.Add(ent);                

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改排班模板明细信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyTemplateDetail(T_HR_SCHEDULINGTEMPLATEDETAIL entTemp)
        {
            string strMsg = string.Empty;
            try
            {
                if (entTemp == null)
                {
                    return "{REQUIREDFIELDS}";
                }


                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" TEMPLATEDETAILID == @0");

                objArgs.Add(entTemp.TEMPLATEDETAILID);

                SchedulingTemplateDetailDAL dalSchedulingTemplateDetail = new SchedulingTemplateDetailDAL();
                flag = dalSchedulingTemplateDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_SCHEDULINGTEMPLATEDETAIL entUpdate = dalSchedulingTemplateDetail.GetSchedulingTemplateDetailRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.RefreshEntity(entTemp);
                entUpdate.SCHEDULINGDATE = entTemp.SCHEDULINGDATE;
                entUpdate.REMARK = entTemp.REMARK;
                entUpdate.UPDATEDATE = entTemp.UPDATEDATE;
                entUpdate.UPDATEUSERID = entTemp.UPDATEUSERID;
                entUpdate.CREATEDATE = entTemp.CREATEDATE;
                entUpdate.CREATEUSERID = entTemp.CREATEUSERID;

                dalSchedulingTemplateDetail.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除T_HR_SCHEDULINGTEMPLATEDETAIL信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strTemplateDetailId">主键索引</param>
        /// <returns></returns>
        public string DeleteTemplateDetail(string strTemplateDetailId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strTemplateDetailId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" TEMPLATEDETAILID == @0");

                objArgs.Add(strTemplateDetailId);

                SchedulingTemplateDetailDAL dalSchedulingTemplateDetail = new SchedulingTemplateDetailDAL();
                flag = dalSchedulingTemplateDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_SCHEDULINGTEMPLATEDETAIL entDel = dalSchedulingTemplateDetail.GetSchedulingTemplateDetailRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalSchedulingTemplateDetail.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据排班模板主表主键索引，删除关联的排班模板明细记录
        /// </summary>
        /// <param name="strTemplateMasterID">排班模板主表主键索引</param>
        /// <returns>返回处理后的消息</returns>
        public string DeleteByTemplateMasterID(string strTemplateMasterID)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strTemplateMasterID))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" T_HR_SCHEDULINGTEMPLATEMASTER.TEMPLATEMASTERID == @0");

                objArgs.Add(strTemplateMasterID);

                SchedulingTemplateDetailDAL dalSchedulingTemplateDetail = new SchedulingTemplateDetailDAL();
                flag = dalSchedulingTemplateDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                string strOrderBy = " TEMPLATEDETAILID ";
                var q = dalSchedulingTemplateDetail.GetSchedulingTemplateDetailRdListByMultSearch(strOrderBy, strFilter.ToString(), objArgs.ToArray());

                if (q == null)
                {
                    return strMsg;
                }

                if (q.Count() == 0)
                {
                    return strMsg; 
                }
                
                foreach (T_HR_SCHEDULINGTEMPLATEDETAIL item in q)
                {                    
                    dalSchedulingTemplateDetail.Delete(item);
                }

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 对指定的排班设置主记录添加其相关的子记录
        /// </summary>
        /// <param name="strTemplateMasterID">排班模板主表主键索引</param>
        /// <param name="entTemps">排班模板主表相关子记录集</param>
        /// <returns>返回处理后的消息</returns>
        public string AddDetailForTemplateMaster(string strTemplateMasterID, List<T_HR_SCHEDULINGTEMPLATEDETAIL> entTemps)
        {
            string strMsg = string.Empty;
            try
            {
                DeleteByTemplateMasterID(strTemplateMasterID);

                if (entTemps.Count > 0)
                {
                    foreach (T_HR_SCHEDULINGTEMPLATEDETAIL item in entTemps)
                    {
                        if (item.EntityKey != null)
                        {
                            item.EntityKey = null;  //清除EntityKey不为null的情况
                        }
                        AddTemplateDetail(item);
                    }
                }                

                strMsg = "{SAVESUCCESSED}";
            }
            catch(Exception ex)
            {
                strMsg = ex.Message.ToString();
            }
            return strMsg;
        }
        #endregion

    }
}