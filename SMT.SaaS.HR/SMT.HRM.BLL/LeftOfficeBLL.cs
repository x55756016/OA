﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using SMT.HRM.IMServices.IMServiceWS;
namespace SMT.HRM.BLL
{
    public class LeftOfficeBLL : BaseBll<T_HR_LEFTOFFICE>, IOperate
    {
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_LEFTOFFICE> LeftOfficePaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            // int index = queryParas.Count - 1;
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEFTOFFICE");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(CheckState);
                }
            }
            else
            {
                SetFilterWithflow("DIMISSIONID", "T_HR_LEFTOFFICE", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }
            IQueryable<T_HR_LEFTOFFICE> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_LEFTOFFICE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }


        /// <summary>
        /// 获取离职申请视图
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public IQueryable<V_LEFTOFFICEVIEW> LeftOfficeViewsPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            // int index = queryParas.Count - 1;
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEFTOFFICE");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(CheckState);
                }
            }
            else
            {
                SetFilterWithflow("DIMISSIONID", "T_HR_LEFTOFFICE", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }
            IQueryable<V_LEFTOFFICEVIEW> ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST.T_HR_POST")
                                                join b in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on c.DIMISSIONID equals b.T_HR_LEFTOFFICE.DIMISSIONID into temp
                                                from d in temp.DefaultIfEmpty()
                                                select new V_LEFTOFFICEVIEW
                                                {
                                                    DIMISSIONID = c.DIMISSIONID,
                                                    EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                                    EMPLOYEECNAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                    EMPLOYEECODE = c.T_HR_EMPLOYEE.EMPLOYEECODE,
                                                    LEFTOFFICECATEGORY = c.LEFTOFFICECATEGORY,
                                                    LEFTOFFICEDATE = c.LEFTOFFICEDATE,
                                                    APPLYDATE = c.APPLYDATE,
                                                    CHECKSTATE = c.CHECKSTATE,
                                                    CREATEUSERID = c.CREATEUSERID,
                                                    OWNERCOMPANYID = c.OWNERCOMPANYID,
                                                    OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                                    OWNERPOSTID = c.OWNERPOSTID,
                                                    OWNERID = c.OWNERID,
                                                    ISCONFIRMED = d == null ? "-1" : d.CHECKSTATE,
                                                    REMARK = c.REMARK,
                                                    LEFTOFFICEREASON = c.LEFTOFFICEREASON,
                                                    EMPLOYEEPOSTID = c.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID,
                                                    POSTID = c.T_HR_EMPLOYEEPOST.T_HR_POST.POSTID

                                                };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_LEFTOFFICEVIEW>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 添加离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        public void LeftOfficeAdd(T_HR_LEFTOFFICE entity, ref string strMsg)
        {
            try
            {
                var tmp = from c in dal.GetObjects()
                          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1" || c.CHECKSTATE == "2")
                          && c.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID == entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                          select c;
                if (tmp.Count() > 0)
                {
                    // throw new Exception("LEFTOFFICESUBMITTED");
                    strMsg = "LEFTOFFICESUBMITTED";
                    return;
                }
                T_HR_LEFTOFFICE ent = new T_HR_LEFTOFFICE();
                Utility.CloneEntity<T_HR_LEFTOFFICE>(entity, ent);
                ent.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                if (entity.T_HR_EMPLOYEEPOST != null)
                {
                    ent.T_HR_EMPLOYEEPOSTReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOST", "EMPLOYEEPOSTID", entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID);
                }
                //dal.Add(ent);
                //xiedx
                //2012-8-27
                bool i = Add(ent,ent.CREATEUSERID);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        public void LeftOfficeUpdate(T_HR_LEFTOFFICE entity, ref string strMsg)
        {
            try
            {
                var tmp = from c in dal.GetObjects()
                          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1")
                          && c.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID == entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID && c.DIMISSIONID != entity.DIMISSIONID
                          select c;
                if (tmp.Count() > 0)
                {
                    //throw new Exception("LEFTOFFICESUBMITTED");
                    strMsg = "LEFTOFFICESUBMITTED";
                    return;
                }
                T_HR_LEFTOFFICE ent = dal.GetTable().FirstOrDefault(s => s.DIMISSIONID == entity.DIMISSIONID);
                if (ent != null)
                {
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //如果是代理岗位  就将代理岗设为无效   并添加异动记录

                        EmployeePostBLL epbll = new EmployeePostBLL();
                        T_HR_EMPLOYEEPOST epost = dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").FirstOrDefault(ep => ep.EMPLOYEEPOSTID == entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID);
                        if (epost != null && epost.ISAGENCY == "1")
                        {
                            epost.EDITSTATE = "0";
                            epbll.EmployeePostUpdate(epost);
                            //删除岗位
                            #region 添加异动记录
                            var tmpInfo = from c in dal.GetObjects<T_HR_POST>()
                                          join b in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals b.DEPARTMENTID
                                          where c.POSTID == epost.T_HR_POST.POSTID
                                          select new
                                          {
                                              c.POSTID,
                                              b.DEPARTMENTID,
                                              b.T_HR_COMPANY.COMPANYID

                                          };
                            EmployeePostChangeBLL epchangeBLL = new EmployeePostChangeBLL();
                            T_HR_EMPLOYEEPOSTCHANGE postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                            postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                            postChange.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                            postChange.T_HR_EMPLOYEE.EMPLOYEEID = entity.T_HR_EMPLOYEE.EMPLOYEEID;
                            postChange.EMPLOYEECODE = entity.T_HR_EMPLOYEE.EMPLOYEECODE;
                            postChange.EMPLOYEENAME = entity.T_HR_EMPLOYEE.EMPLOYEECNAME;
                            postChange.POSTCHANGEID = Guid.NewGuid().ToString();
                            postChange.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                            postChange.ISAGENCY = "1";
                            if (tmpInfo.Count() > 0)
                            {
                                postChange.FROMCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                postChange.FROMDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                postChange.FROMPOSTID = tmpInfo.FirstOrDefault().POSTID;

                                postChange.OWNERCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                postChange.OWNERDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                postChange.OWNERPOSTID = tmpInfo.FirstOrDefault().POSTID;
                            }
                            postChange.OWNERID = entity.T_HR_EMPLOYEE.EMPLOYEEID;
                            postChange.POSTCHANGREASON = entity.LEFTOFFICEREASON;
                            postChange.CHANGEDATE = entity.LEFTOFFICEDATE.ToString();
                            postChange.CREATEUSERID = entity.CREATEUSERID;
                            postChange.POSTCHANGCATEGORY = "3";
                            string Msg = string.Empty;
                            epchangeBLL.EmployeePostChangeAdd(postChange, ref Msg);
                            #endregion
                            //通知及时通讯
                            DelImstantMember(entity.T_HR_EMPLOYEE.EMPLOYEEID, epost.T_HR_POST.POSTID); 
                            
                        }
                        else
                        {
                            //员工状态修改为离职中 
                            string tmpstr = "";
                            var employeetmps = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                               where c.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                                               select c;
                            if (employeetmps.Count() > 0)
                            {
                                EmployeeBLL bll = new EmployeeBLL();
                                var employeetmp = employeetmps.FirstOrDefault();
                                employeetmp.EMPLOYEESTATE = "3";
                                bll.EmployeeUpdate(employeetmp, ref tmpstr);
                            }

                        }
                    }
                    Utility.CloneEntity<T_HR_LEFTOFFICE>(entity, ent);
                    if (entity.T_HR_EMPLOYEE != null)
                    {
                        ent.T_HR_EMPLOYEEReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    }
                    if (entity.T_HR_EMPLOYEEPOST != null)
                    {
                        ent.T_HR_EMPLOYEEPOSTReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOST", "EMPLOYEEPOSTID", entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID);
                    }
                    //dal.Update(ent);
                    Update(ent,ent.CREATEPOSTID);
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeUpdate:" + ex.Message);
                throw ex;
            }
        }


        #region 调用即时通讯接口
        private void DelImstantMember(string EMPLOYEEID, string StrPostID)
        {
            //用来记录提醒信息
            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯的接口，员工ID" + EMPLOYEEID);
            string StrMessage = "";
            try
            {
                DataSyncServiceClient IMCient = new DataSyncServiceClient();
                var q = from ent in dal.GetObjects<T_HR_POST>()
                        where ent.POSTID == StrPostID
                        select ent.T_HR_DEPARTMENT.DEPARTMENTID;

                if (q.Count() > 0)
                {
                    StrMessage = IMCient.EmployeeLeave(EMPLOYEEID, q.FirstOrDefault(), StrPostID, EmployeeType.VicePost);
                }
                else
                {
                    StrMessage = "员工离职调用即时通讯时部门为空";
                }
                SMT.Foundation.Log.Tracer.Debug("员工离职确认调用即时通讯时返回结果为：" + StrMessage);
            }
            catch (Exception ex)
            {
                StrMessage = "员工离职确认调用即时通讯错误" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(StrMessage);
            }
        }
        #endregion
        /// <summary>
        /// 删除离职申请记录
        /// </summary>
        /// <param name="dimissionIDs"></param>
        /// <returns></returns>
        public int LeftOfficeDelete(string[] dimissionIDs)
        {
            try
            {
                foreach (var id in dimissionIDs)
                {
                    T_HR_LEFTOFFICE ent = dal.GetObjects().FirstOrDefault(s => s.DIMISSIONID == id);
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                        try
                        {
                            if (dal.SaveContextChanges() > 0)
                            {
                                DeleteMyRecord(ent);
                            }
                        }
                        catch
                        {
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("LeftOfficeDelete Error:" + ex.ToString());
                return -1;
            }
        }
        /// <summary>
        /// 根据离职申请记录ID获取信息
        /// </summary>
        /// <param name="dimissionID">离职信息ID</param>
        /// <returns></returns>
        public T_HR_LEFTOFFICE GetLeftOfficeByID(string dimissionID)
        {
            return dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST").Include("T_HR_EMPLOYEEPOST.T_HR_POST").FirstOrDefault(s => s.DIMISSIONID == dimissionID);
        }

        /// <summary>
        /// 根据员工ID和岗位ID获取信息
        /// </summary>
        /// <param name="EmployeeID">员工ID</param>
        /// <param name="PostID">岗位ID</param>
        /// <returns></returns>
        public T_HR_LEFTOFFICE GetLeftOfficeByEmployeeIDAndPostID(string EmployeeID,string PostID)
        {
            var ents = (from ent in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST").Include("T_HR_EMPLOYEEPOST.T_HR_POST")
                       where ent.T_HR_EMPLOYEE.EMPLOYEEID == EmployeeID
                       && ent.T_HR_EMPLOYEEPOST.T_HR_POST.POSTID == PostID
                       && ent.CHECKSTATE =="2" 
                       select ent).FirstOrDefault();

            return ents;
            
        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var leftOffice = (from c in dal.GetObjects<T_HR_LEFTOFFICE>().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST")
                                  where c.DIMISSIONID == EntityKeyValue
                                  select c).FirstOrDefault();
                if (leftOffice != null)
                {
                    leftOffice.CHECKSTATE = CheckState;
                    leftOffice.UPDATEDATE = DateTime.Now;
                    LeftOfficeUpdate(leftOffice, ref strMsg);
                    if (string.IsNullOrEmpty(strMsg))
                    {
                        i = 1;
                    }
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