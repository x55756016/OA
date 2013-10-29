using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Data;

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

        /// <summary>
        /// 导出员工签卡明细
        /// </summary>
        /// <param name="signinID">签卡单ID</param>
        /// <returns></returns>
        public byte[] ExportEmployeeSignIn(string signinID)
        {
            try
            {
                EmployeeSignInRecordBLL bll=new EmployeeSignInRecordBLL();
                T_HR_EMPLOYEESIGNINRECORD record = bll.GetEmployeeSigninRecordByID(signinID);
                string empName = string.Empty;
                if (record != null)
                {
                    empName = record.EMPLOYEENAME;
                }
                var ent = this.GetEmployeeSignInDetailBySigninID(signinID);
                byte[] result;
                DataTable dt = TableToExportInit();
                if (ent!=null && ent.Any())
                {
                    DataTable dttoExport = GetDataConversion(dt, ent);
                    result = Utility.OutFileStream(empName + Utility.GetResourceStr(" 异常签卡明细"), dttoExport);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeeSignIn导出员工签卡信息:" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 数据组装
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="entSignIn"></param>
        /// <returns></returns>
        private DataTable GetDataConversion(DataTable dt, IQueryable<T_HR_EMPLOYEESIGNINDETAIL> entSignIn)
        {
            dt.Rows.Clear();
            foreach (var item in entSignIn)
            {
                try
                {
                    var dic = new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "ABNORMCATEGORY", "ATTENDPERIOD", "REASONCATEGORY" });//获取字典值
                    // nationDict = tmp.Where(s => s.DICTIONCATEGORY == "NATION" && s.DICTIONARYVALUE == nationValue).FirstOrDefault();
                    DataRow row = dt.NewRow();
                    #region 每行数据
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        switch (i)
                        {
                            case 0: row[i] = item.ABNORMALDATE.Value.ToString("yyyy-MM-dd"); break;//异常日期
                            case 1:
                                decimal? abCategory = Convert.ToDecimal(item.ABNORMCATEGORY);
                                var dicAbCategory = dic.Where(s => s.DICTIONCATEGORY == "ABNORMCATEGORY" && s.DICTIONARYVALUE == abCategory).FirstOrDefault();
                                if (dicAbCategory != null)
                                {
                                    row[i] = dicAbCategory.DICTIONARYNAME; ;//异常类型
                                }
                                break;
                            case 2:
                                decimal? abOd = Convert.ToDecimal(item.ATTENDPERIOD);
                                var dicAbOd = dic.Where(s => s.DICTIONCATEGORY == "ATTENDPERIOD" && s.DICTIONARYVALUE == abOd).FirstOrDefault();
                                if (dicAbOd != null)
                                {
                                    row[i] = dicAbOd.DICTIONARYNAME;//异常时间段
                                }
                                break;
                            case 3: row[i] = item.ABNORMALTIME; break;//异常时长（分钟）
                            case 4:
                                decimal? reCategory = Convert.ToDecimal(item.REASONCATEGORY);
                                var dicReCategory = dic.Where(s => s.DICTIONCATEGORY == "REASONCATEGORY" && s.DICTIONARYVALUE == reCategory).FirstOrDefault();
                                if (dicReCategory != null)
                                {
                                    row[i] = dicReCategory.DICTIONARYNAME; ;//异常原因类型
                                }
                                break;
                            case 5: row[i] = item.DETAILREASON; break;//异常原因
                        }
                    }
                    dt.Rows.Add(row);
                    #endregion
                }
                catch (Exception ex)
                {
                    SMT.Foundation.Log.Tracer.Debug("ExportEmployeeSignIn导出员工签卡信息组装DataTable时出错:" + ex.Message);
                    return null;
                }

            }
            return dt;
        }

        /// <summary>
        /// 定义表头
        /// </summary>
        /// <returns></returns>
        private DataTable TableToExportInit()
        {
            DataTable dt = new DataTable();
            #region 定义表头名称
            DataColumn col1 = new DataColumn();
            col1.ColumnName = "异常时间";
            col1.DataType = typeof(string);
            dt.Columns.Add(col1);

            DataColumn col2 = new DataColumn();
            col2.ColumnName = "异常类型";
            col2.DataType = typeof(string);
            dt.Columns.Add(col2);

            DataColumn col3 = new DataColumn();
            col3.ColumnName = "异常时间段";
            col3.DataType = typeof(string);
            dt.Columns.Add(col3);

            DataColumn col4 = new DataColumn();
            col4.ColumnName = "异常时长(分钟)";
            col4.DataType = typeof(decimal);
            dt.Columns.Add(col4);

            DataColumn col5 = new DataColumn();
            col5.ColumnName = "异常原因类型";
            col5.DataType = typeof(string);
            dt.Columns.Add(col5);

            DataColumn col6 = new DataColumn();
            col6.ColumnName = Utility.GetResourceStr("异常原因");
            col6.DataType = typeof(string);
            dt.Columns.Add(col6);
            #endregion
            return dt;
        }
        #endregion       
    
        
    }
}
