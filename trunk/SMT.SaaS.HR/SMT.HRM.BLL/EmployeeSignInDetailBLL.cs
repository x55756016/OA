using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class EmployeeSignInDetailBLL : BaseBll<T_HR_EMPLOYEESIGNINDETAIL>
    {
        public EmployeeSignInDetailBLL()
        { }

        #region 获取数据

        public T_HR_EMPLOYEESIGNINDETAIL GetEmployeeSignInDetailByAbnormRecordID(string strAbnormRecordId)
        {
            var ents = from d in dal.GetObjects().Include("T_HR_EMPLOYEESIGNINRECORD").Include("T_HR_EMPLOYEEABNORMRECORD")
                       where d.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID == strAbnormRecordId
                       select d;

            if (ents.Count() < 1)
            {
                return null;
            }

            return ents.FirstOrDefault();
        }

        /// <summary>
        /// 根据签卡明细关联的异常记录主键索引及签卡主表的审核
        /// 状态来获取单一的明细(仅在签卡主表的审核状态为审核通过时能确定唯一记录，其他状态下使用会出现误差)
        /// </summary>
        /// <param name="strAbnormRecordId">关联的异常记录主键索引</param>
        /// <param name="strCheckState">签卡主表的审核状态</param>
        /// <returns></returns>
        public T_HR_EMPLOYEESIGNINDETAIL GetEmployeeSignInDetailByAbnormRecordIDAndCheckState(string strAbnormRecordId, string strCheckState)
        {
            var ents = from d in dal.GetObjects().Include("T_HR_EMPLOYEESIGNINRECORD").Include("T_HR_EMPLOYEEABNORMRECORD")
                       where d.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID == strAbnormRecordId && d.T_HR_EMPLOYEESIGNINRECORD.CHECKSTATE == strCheckState
                       select d;

            if (ents.Count() < 1)
            {
                return null;
            }

            return ents.FirstOrDefault();
        }

        /// <summary>
        /// 根据员工签卡记录ID获取签卡的异常信息记录
        /// </summary>
        /// <param name="signinID">签卡记录ID</param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEESIGNINDETAIL> GetEmployeeSignInDetailBySigninID(string signinID)
        {
            var ents = from d in dal.GetObjects().Include("T_HR_EMPLOYEESIGNINRECORD").Include("T_HR_EMPLOYEEABNORMRECORD")
                       select d;

            return ents.Where(s => s.T_HR_EMPLOYEESIGNINRECORD.SIGNINID == signinID).OrderBy("ABNORMALDATE");
        }        

        /// <summary>
        /// 根据条件，获取员工异常记录信息
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strEmployeeID">异常记录对应关联的员工序号</param>
        /// <param name="strSignInID">签卡ID(参数为空，则取未签卡的异常；不为空，则取对应已签卡的异常)</param>
        /// <param name="strCurDateMonth">当前日期(年-月)</param>
        /// <param name="strSortKey">排序字段</param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEESIGNINDETAIL> GetAllEmployeeSignInDetailRdListByMultSearch(string strOwnerID, string strEmployeeID, string strSignInID,
            string strCurDateMonth, string strSortKey)
        {
            EmployeeSignInDetailDAL dalEmployeeSignInDetail = new EmployeeSignInDetailDAL();

            StringBuilder strfilter = new StringBuilder();
            List<object> objArgs = new List<object>();
            string strOrderBy = string.Empty;
            int iIndex = 0;

            if (!string.IsNullOrEmpty(strEmployeeID))
            {
                strfilter.Append(" T_HR_EMPLOYEESIGNINRECORD.EMPLOYEEID == @0");
                objArgs.Add(strEmployeeID);
            }

            if (!string.IsNullOrEmpty(strSignInID))
            {
                if (!string.IsNullOrEmpty(strfilter.ToString()))
                {
                    strfilter.Append(" AND ");
                }

                if (objArgs.Count() > 0)
                {
                    iIndex = objArgs.Count();
                }

                strfilter.Append(" T_HR_EMPLOYEESIGNINRECORD.SIGNINID == @" + iIndex.ToString());
                objArgs.Add(strSignInID);
            }

            if (!string.IsNullOrEmpty(strSortKey))
            {
                strOrderBy = strSortKey;
            }
            else
            {
                strOrderBy = " ABNORMALDATE ";
            }

            string filterString = strfilter.ToString();

            SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEESIGNINDETAIL");

            var q = dalEmployeeSignInDetail.GetEmployeeSignInDetailRdListByMultSearch(strOrderBy, strCurDateMonth, filterString, objArgs.ToArray());
            return q;
        }

        /// <summary>
        /// 根据条件，获取员工异常记录信息,并进行分页
        /// </summary>
        /// <param name="strOwnerID">权限控制，当前记录所有者的员工序号</param>
        /// <param name="strEmployeeID">异常记录对应关联的员工序号</param>
        /// <param name="strSignInID">签卡ID(参数为空，则取未签卡的异常；不为空，则取对应已签卡的异常)</param>
        /// <param name="strCurDateMonth">当前日期(年-月)</param>
        /// <param name="strSortKey">排序字段</param>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">总页数</param>
        /// <returns>员工异常记录信息</returns>
        public IQueryable<T_HR_EMPLOYEESIGNINDETAIL> GetEmployeeSignInDetailRdListByMultSearch(string strOwnerID, string strEmployeeID, string strSignInID,
            string strCurDateMonth, string strSortKey, int pageIndex, int pageSize, ref int pageCount)
        {
            var q = GetAllEmployeeSignInDetailRdListByMultSearch(strOwnerID, strEmployeeID, strSignInID, strCurDateMonth, strSortKey);

            return Utility.Pager<T_HR_EMPLOYEESIGNINDETAIL>(q, pageIndex, pageSize, ref pageCount);
        }
        #endregion

        #region 操作

        /// <summary>
        /// 新增员工异常记录信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string AddEmployeeSignInDetail(T_HR_EMPLOYEESIGNINDETAIL entTemp)
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

                strFilter.Append(" T_HR_EMPLOYEESIGNINRECORD.SIGNINID == @0");
                strFilter.Append(" && T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID == @1");

                objArgs.Add(entTemp.T_HR_EMPLOYEESIGNINRECORD.SIGNINID);
                objArgs.Add(entTemp.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID);

                string strCurDate = entTemp.ABNORMALDATE.Value.ToShortDateString();

                EmployeeSignInDetailDAL dalEmployeeSignInDetail = new EmployeeSignInDetailDAL();
                flag = dalEmployeeSignInDetail.IsExistsRd(strCurDate, strFilter.ToString(), objArgs.ToArray());

                if (flag)
                {
                    return "{ALREADYEXISTSRECORD}";
                }

                dalEmployeeSignInDetail.Add(entTemp);

                strMsg = "{SAVESUCCESSED}";

            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 修改员工异常记录信息
        /// </summary>
        /// <param name="entTemp"></param>
        /// <returns></returns>
        public string ModifyEmployeeSignInDetail(T_HR_EMPLOYEESIGNINDETAIL entTemp)
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

                strFilter.Append(" SIGNINDETAILID == @0");

                objArgs.Add(entTemp.SIGNINDETAILID);

                EmployeeSignInDetailDAL dalEmployeeSignInDetail = new EmployeeSignInDetailDAL();
                flag = dalEmployeeSignInDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_EMPLOYEESIGNINDETAIL entUpdate = dalEmployeeSignInDetail.GetEmployeeSignInDetailRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                Utility.CloneEntity(entTemp, entUpdate);

                dalEmployeeSignInDetail.Update(entUpdate);
                strMsg = "{SAVESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据主键索引，删除员工考勤异常信息(注：暂定为物理删除)
        /// </summary>
        /// <param name="strVacationId">主键索引</param>
        /// <returns></returns>
        public string RemoveEmployeeSignInDetail(string strSignInDetailId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strSignInDetailId))
                {
                    return "{REQUIREDFIELDS}";
                }

                bool flag = false;
                StringBuilder strFilter = new StringBuilder();
                List<string> objArgs = new List<string>();

                strFilter.Append(" SIGNINDETAILID == @0");

                objArgs.Add(strSignInDetailId);

                EmployeeSignInDetailDAL dalEmployeeSignInDetail = new EmployeeSignInDetailDAL();
                flag = dalEmployeeSignInDetail.IsExistsRd(strFilter.ToString(), objArgs.ToArray());

                if (!flag)
                {
                    return "{NOTFOUND}";
                }

                T_HR_EMPLOYEESIGNINDETAIL entDel = dalEmployeeSignInDetail.GetEmployeeSignInDetailRdByMultSearch(strFilter.ToString(), objArgs.ToArray());
                dalEmployeeSignInDetail.Delete(entDel);

                strMsg = "{DELETESUCCESSED}";
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 根据签卡Id，删除指定签卡记录下的明细
        /// </summary>
        /// <param name="strSignInId">签卡Id</param>
        /// <returns></returns>
        public string RemoveSignInDetailsBySignInId(string strSignInId)
        {
            string strMsg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(strSignInId))
                {
                    return "{REQUIREDFIELDS}";
                }

                IQueryable<T_HR_EMPLOYEESIGNINDETAIL> entDetails = GetEmployeeSignInDetailBySigninID(strSignInId);

                if (entDetails == null)
                {
                    return string.Empty;
                }

                if (entDetails.Count() == 0)
                {
                    return string.Empty;
                }

                foreach (T_HR_EMPLOYEESIGNINDETAIL item in entDetails)
                {
                    dal.DeleteFromContext(item);
                }

                dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;
        }

        /// <summary>
        /// 批量添加签卡明细
        /// </summary>
        /// <param name="entDetails"></param>
        public string AddEmployeeSignInDetails(List<T_HR_EMPLOYEESIGNINDETAIL> entDetails)
        {
            string strMsg = string.Empty;
            try
            {
                if (entDetails == null)
                {
                    return string.Empty;
                }

                if (entDetails.Count() == 0)
                {
                    return string.Empty;
                }

                foreach (T_HR_EMPLOYEESIGNINDETAIL item in entDetails)
                {
                    T_HR_EMPLOYEESIGNINDETAIL entTemp = new T_HR_EMPLOYEESIGNINDETAIL();
                    Utility.CloneEntity(item, entTemp);

                    if (entTemp.EntityKey != null)
                    {
                        entTemp.EntityKey = null;
                    }

                    if (entTemp.SIGNINDETAILID == null)
                    {
                        entTemp.SIGNINDETAILID = System.Guid.NewGuid().ToString().ToUpper();
                    }

                    if (item.T_HR_EMPLOYEEABNORMRECORD != null)
                    {
                        entTemp.T_HR_EMPLOYEEABNORMRECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_EMPLOYEEABNORMRECORD", "ABNORMRECORDID", item.T_HR_EMPLOYEEABNORMRECORD.ABNORMRECORDID);
                    }

                    if (item.T_HR_EMPLOYEESIGNINRECORD != null)
                    {
                        entTemp.T_HR_EMPLOYEESIGNINRECORDReference.EntityKey = new System.Data.EntityKey("SMT_HRM_EFModelContext.T_HR_EMPLOYEESIGNINRECORD", "SIGNINID", item.T_HR_EMPLOYEESIGNINRECORD.SIGNINID);
                    }

                    dal.AddToContext(entTemp);
                }

                dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                strMsg = ex.Message.ToString();
            }

            return strMsg;

        }

        #endregion       
    
        
    }
}
