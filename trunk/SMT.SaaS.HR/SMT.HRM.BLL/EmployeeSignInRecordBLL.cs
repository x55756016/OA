using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    public class EmployeeSignInRecordBLL : BaseBll<T_HR_EMPLOYEESIGNINRECORD>, IOperate
    {
        /// <summary>
        /// 根据ID获取签卡记录信息
        /// </summary>
        /// <param name="strid">记录信息ID</param>
        /// <returns></returns>
        public T_HR_EMPLOYEESIGNINRECORD GetEmployeeSigninRecordByID(string strid)
        {
            return dal.GetObjects().FirstOrDefault(s => s.SIGNINID == strid);
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的公司信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>        
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strOwnerID">登录的员工ID</param>
        /// <param name="recorderDate">浏览时间</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEESIGNINRECORD> EmployeeSignInRecordPaging(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string recorderDate)
        {

            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }

                SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_EMPLOYEESIGNINRECORD");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("SIGNINID", "T_HR_EMPLOYEESIGNINRECORD", strOwnerID, ref strCheckState, ref filterString, ref paras);
                if (string.Compare(strCheckfilter, filterString) == 0)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(strCheckState))
            {
                int iIndex = 0;
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                if (paras.Count() > 0)
                {
                    iIndex = paras.Count();
                }

                filterString += " CHECKSTATE == @" + iIndex.ToString();
                paras.Add(strCheckState);
            }

            IQueryable<T_HR_EMPLOYEESIGNINRECORD> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            if (!string.IsNullOrEmpty(recorderDate))
            {
                DateTime tmpDate = Convert.ToDateTime(recorderDate);
                ents = ents.Where(p => p.SIGNINTIME.Value.Year == tmpDate.Year && p.SIGNINTIME.Value.Month == tmpDate.Month);
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEESIGNINRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的公司信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>        
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strOwnerID">登录的员工ID</param>
        /// <param name="recorderDate">浏览时间</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEESIGNINRECORD> EmployeeSignInRecordPagingIncludeDetail(string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string recorderDate)
        {

            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }

                SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_EMPLOYEESIGNINRECORD");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("SIGNINID", "T_HR_EMPLOYEESIGNINRECORD", strOwnerID, ref strCheckState, ref filterString, ref paras);
                if (string.Compare(strCheckfilter, filterString) == 0)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(strCheckState))
            {
                int iIndex = 0;
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                if (paras.Count() > 0)
                {
                    iIndex = paras.Count();
                }

                filterString += " CHECKSTATE == @" + iIndex.ToString();
                paras.Add(strCheckState);
            }

            IQueryable<T_HR_EMPLOYEESIGNINRECORD> ents = dal.GetObjects().Include("T_HR_EMPLOYEESIGNINDETAIL") ;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            if (!string.IsNullOrEmpty(recorderDate))
            {
                DateTime tmpDate = Convert.ToDateTime(recorderDate);
                ents = ents.Where(p => p.SIGNINTIME.Value.Year == tmpDate.Year && p.SIGNINTIME.Value.Month == tmpDate.Month);
            }
            ents = ents.OrderBy(sort);

            //ents = Utility.Pager<T_HR_EMPLOYEESIGNINRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
       
        
        
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的公司信息
        /// (视图V_EMPLOYEESIGNINRECORD 带有部门名称)
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>        
        /// <param name="strCheckState">审核状态</param>
        /// <param name="strOwnerID">登录的员工ID</param>
        /// <param name="recorderDate">浏览时间</param>
        /// <returns>查询结果集</returns>
        public IQueryable<V_EMPLOYEESIGNINRECORD> EmployeeSignInRecordPagingByView(int pageIndex, int pageSize, string sort, string filterString, List<object> paras, ref int pageCount, string strCheckState, string strOwnerID, string recorderDate)
        {

            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                if (strCheckState == Convert.ToInt32(CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                }

                SetOrganizationFilter(ref filterString, ref paras, strOwnerID, "T_HR_EMPLOYEESIGNINRECORD");
            }
            else
            {
                string strCheckfilter = string.Copy(filterString);
                SetFilterWithflow("SIGNINID", "T_HR_EMPLOYEESIGNINRECORD", strOwnerID, ref strCheckState, ref filterString, ref paras);
                if (string.Compare(strCheckfilter, filterString) == 0)
                {
                    return null;
                }
            }

            if (!string.IsNullOrEmpty(strCheckState))
            {
                int iIndex = 0;
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                if (paras.Count() > 0)
                {
                    iIndex = paras.Count();
                }

                filterString += " CHECKSTATE == @" + iIndex.ToString();
                paras.Add(strCheckState);
            }

            IQueryable<T_HR_EMPLOYEESIGNINRECORD> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            if (!string.IsNullOrEmpty(recorderDate))
            {
                DateTime tmpDate = Convert.ToDateTime(recorderDate);
                ents = ents.Where(p => p.SIGNINTIME.Value.Year == tmpDate.Year && p.SIGNINTIME.Value.Month == tmpDate.Month);
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEESIGNINRECORD>(ents, pageIndex, pageSize, ref pageCount);
            var entr = from e in ents
                       join vDepartment in dal.GetObjects<T_HR_DEPARTMENT>() on e.OWNERDEPARTMENTID equals vDepartment.DEPARTMENTID
                       select new V_EMPLOYEESIGNINRECORD
                       {
                           CHECKSTATE = e.CHECKSTATE,
                           CREATECOMPANYID = e.CREATECOMPANYID,
                           CREATEDATE = e.CREATEDATE,
                           CREATEDEPARTMENTID = e.CREATECOMPANYID,
                           CREATEPOSTID = e.CREATEPOSTID,
                           CREATEUSERID = e.CREATEUSERID,
                           DEPTNAME = vDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           EMPLOYEECODE = e.EMPLOYEECODE,
                           EMPLOYEEID = e.EMPLOYEEID,
                           EMPLOYEENAME = e.EMPLOYEENAME,
                           OWNERCOMPANYID = e.OWNERCOMPANYID,
                           OWNERDEPARTMENTID = e.OWNERDEPARTMENTID,
                           OWNERID = e.OWNERID,
                           OWNERPOSTID = e.OWNERPOSTID,
                           REMARK = e.REMARK,
                           SIGNINCATEGORY = e.SIGNINCATEGORY,
                           SIGNINID = e.SIGNINID,
                           SIGNINTIME = e.SIGNINTIME

                       };
            return entr;
        }



        /// <summary>
        /// 添加签卡记录信息
        /// </summary>
        /// <param name="entity">签卡记录实体</param>
        /// <param name="entityList">异常信息实体</param>
        public string EmployeeSignInRecordAdd(T_HR_EMPLOYEESIGNINRECORD entity, List<T_HR_EMPLOYEESIGNINDETAIL> entityList)
        {
            string strMsg = string.Empty;

            try
            {
                T_HR_EMPLOYEESIGNINRECORD entSignInRd = new T_HR_EMPLOYEESIGNINRECORD();
                Utility.CloneEntity(entity, entSignInRd);
                dal.AddToContext(entSignInRd);
                dal.SaveContextChanges();

                foreach (T_HR_EMPLOYEESIGNINDETAIL entDetail in entityList)
                {
                    T_HR_EMPLOYEESIGNINDETAIL entTemp = new T_HR_EMPLOYEESIGNINDETAIL();
                    Utility.CloneEntity(entDetail, entTemp);
                    if (entDetail.T_HR_EMPLOYEEABNORMRECORD != null)
                    {
                        entTemp.T_HR_EMPLOYEEABNORMRECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_EMPLOYEEABNORMRECORD", "ABNORMRECORDID", entDetail.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID);
                    }
                    if (entDetail.T_HR_EMPLOYEESIGNINRECORD != null)
                    {
                        entTemp.T_HR_EMPLOYEESIGNINRECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_EMPLOYEESIGNINRECORD", "SIGNINID", entDetail.T_HR_EMPLOYEESIGNINRECORD.SIGNINID);
                    }

                    dal.AddToContext(entTemp);
                }

                dal.SaveContextChanges();
                strMsg = "{SAVESUCCESSED}";
                SaveMyRecord(entSignInRd);
            }
            catch (Exception ex)
            {
                strMsg = ex.Message; ;
            }

            return strMsg;
        }
        /// <summary>
        /// 修改签卡记录信息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityList"></param>
        public void EmployeeSigninRecordUpdate(T_HR_EMPLOYEESIGNINRECORD entTemp, List<T_HR_EMPLOYEESIGNINDETAIL> entityList)
        {
            try
            {
                if (entTemp == null)
                {
                    return;
                }

                EmployeeSignInDetailBLL bllDetail = new EmployeeSignInDetailBLL();
                bllDetail.RemoveSignInDetailsBySignInId(entTemp.SIGNINID);
                bllDetail.AddEmployeeSignInDetails(entityList);

                if (entTemp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    dal.UpdateFromContext(entTemp);
                    dal.SaveContextChanges();
                    SaveMyRecord(entTemp);
                }
                else if (entTemp.CHECKSTATE == Convert.ToInt32(CheckStates.Approving).ToString())//待办任务提交审核时使用
                {
                    EmployeeSigninRecordAudit(entTemp.SIGNINID, entTemp.CHECKSTATE);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 签卡记录审核
        /// </summary>
        /// <param name="strSignInID"></param>
        /// <param name="strCheckState"></param>
        public string EmployeeSigninRecordAudit(string strSignInID, string strCheckState)
        {
            string strMsg = string.Empty;
            string singinstate = "0";
            try
            {
                if (string.IsNullOrEmpty(strSignInID) || string.IsNullOrEmpty(strCheckState))
                {
                    return "{NOTFOUND}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" SIGNINID == @0");

                objArgs.Add(strSignInID);

                EmployeeSigninRecordDAL dalSigninRecord = new EmployeeSigninRecordDAL();
                flag = dalSigninRecord.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_EMPLOYEESIGNINRECORD entAudit = dalSigninRecord.GetSigninRecordByMultSearch(strFilter.ToString(), objArgs.ToArray());

                //已审核通过的记录禁止再次提交审核
                if (entAudit.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    return "{REPEATAUDITERROR}";
                }

                //审核状态变为审核中或者审核通过时，生成对应的员工考勤记录(应用的员工范围，视应用对象而定)
                if (strCheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    singinstate = "2";
                }
                else if (strCheckState == Convert.ToInt32(CheckStates.Approving).ToString())
                {
                    singinstate = "3";
                }
                else if (strCheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    singinstate = "1";
                }

                if (Convert.ToInt32(singinstate) == 0)
                {
                    return "{NOTFOUND}";
                }

                EmployeeSignInDetailBLL bllDetail = new EmployeeSignInDetailBLL();
                IQueryable<T_HR_EMPLOYEESIGNINDETAIL> entDetails = bllDetail.GetEmployeeSignInDetailBySigninID(strSignInID);
                foreach (T_HR_EMPLOYEESIGNINDETAIL item in entDetails)
                {
                    AbnormRecordBLL bllAbnormRecord = new AbnormRecordBLL();
                    T_HR_EMPLOYEEABNORMRECORD entAbnormRecord = item.T_HR_EMPLOYEEABNORMRECORD;
                    entAbnormRecord.SINGINSTATE = singinstate;//(Convert.ToInt32(Common.IsChecked.Yes) + 1).ToString();
                    bllAbnormRecord.ModifyAbnormRecord(entAbnormRecord);
                }

                entAudit.CHECKSTATE = strCheckState;
                dalSigninRecord.Update(entAudit);

                SaveMyRecord(entAudit);
                strMsg = "{SAVESUCCESSED}";

                if (entAudit.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() && entAudit.CHECKSTATE != strCheckState)
                {
                    List<T_HR_EMPLOYEEABNORMRECORD> entABnormRecords = entDetails.Select(c => c.T_HR_EMPLOYEEABNORMRECORD).ToList();
                    ClearNoSignInRecord("T_HR_EMPLOYEESIGNINRECORD", entAudit.EMPLOYEEID, entABnormRecords);
                }

            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }

            return strMsg;
        }



        /// <summary>
        /// 删除签卡记录组
        /// </summary>
        /// <param name="leaveRecordIDs">签卡记录ID组</param>
        /// <returns>返回受影响的行数</returns>
        public int EmployeeSigninRecordDelete(string[] strSignInIDs)
        {
            if (strSignInIDs == null)
            {
                return 0;
            }

            foreach (var id in strSignInIDs)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                var q = from d in dal.GetObjects<T_HR_EMPLOYEESIGNINDETAIL>().Include("T_HR_EMPLOYEESIGNINRECORD").Include("T_HR_EMPLOYEEABNORMRECORD")
                        where d.T_HR_EMPLOYEESIGNINRECORD.SIGNINID == id
                        select d;

                foreach (T_HR_EMPLOYEESIGNINDETAIL entDetail in q)
                {
                    dal.Delete(entDetail);
                }

                var entity = from a in dal.GetObjects()
                             where a.SIGNINID == id
                             select a;

                if (entity.Count() == 0)
                {
                    continue;
                }

                T_HR_EMPLOYEESIGNINRECORD entSignInRd = entity.FirstOrDefault();

                dal.Delete(entSignInRd);
                DeleteMyRecord(entSignInRd);

                List<string> strCloseSignInIds = new List<string>();
                strCloseSignInIds.Add(id);
                CloseAttendAbnormAlarmMsg(strCloseSignInIds, "T_HR_EMPLOYEESIGNINRECORD", entSignInRd.EMPLOYEEID);
            }
            return 1;
        }

        /// <summary>
        /// 删除签卡记录及明细，关闭消息提醒
        /// </summary>
        /// <param name="strModelCode">模块实体代码</param>
        /// <param name="strEmployeeId">签卡人员工ID</param>
        /// <param name="entAbnormRecords">异常记录</param>
        public void ClearNoSignInRecord(string strModelCode, string strEmployeeId, List<T_HR_EMPLOYEEABNORMRECORD> entAbnormRecords)
        {
            if (entAbnormRecords == null)
            {
                return;
            }

            if (entAbnormRecords.Count() == 0)
            {
                return;
            }

            List<string> strSignInIds = new List<string>();
            List<string> strCloseSignInIds = new List<string>();

            foreach (T_HR_EMPLOYEEABNORMRECORD entAbnormRecord in entAbnormRecords)
            {
                var q = from d in dal.GetObjects<T_HR_EMPLOYEESIGNINDETAIL>().Include("T_HR_EMPLOYEESIGNINRECORD").Include("T_HR_EMPLOYEEABNORMRECORD")
                        where d.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID == entAbnormRecord.ABNORMRECORDID
                        && d.T_HR_EMPLOYEESIGNINRECORD.CHECKSTATE == "0"
                        select d;

                if (q.Count() == 0)
                {
                    continue;
                }

                foreach (T_HR_EMPLOYEESIGNINDETAIL entDetail in q)
                {
                    string strSignInId = entDetail.T_HR_EMPLOYEESIGNINRECORD.SIGNINID;
                    if (strSignInIds.Contains(strSignInId) == false)
                    {
                        strSignInIds.Add(strSignInId);
                    }

                    dal.Delete(entDetail);
                }
            }

            bool isClose = false;
            foreach (string strSignInId in strSignInIds)
            {
                var entity = from a in dal.GetObjects()
                             where a.SIGNINID == strSignInId
                             select a;

                if (entity.Count() == 0)
                {
                    continue;
                }

                T_HR_EMPLOYEESIGNINRECORD entSignInRd = entity.FirstOrDefault();

                var q = from d in dal.GetObjects<T_HR_EMPLOYEESIGNINDETAIL>().Include("T_HR_EMPLOYEESIGNINRECORD").Include("T_HR_EMPLOYEEABNORMRECORD")
                        where d.T_HR_EMPLOYEESIGNINRECORD.SIGNINID == entSignInRd.SIGNINID
                        select d;

                if (q.Count() == 0)
                {
                    strCloseSignInIds.Add(strSignInId);
                    dal.Delete(entSignInRd);
                    DeleteMyRecord(entSignInRd);
                    isClose = true;
                }
            }

            if (isClose)
            {
                CloseAttendAbnormAlarmMsg(strCloseSignInIds, strModelCode, strEmployeeId);
            }
        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                strMsg = EmployeeSigninRecordAudit(EntityKeyValue, CheckState);
                if (strMsg == "{SAVESUCCESSED}")
                {
                    i = 1;
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
    }
}
