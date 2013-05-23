/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-23

** 修改人：刘锦

** 修改时间：2011-09-02

** 描述：

**    主要用于出差申请的业务逻辑处理

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
using SMT.SaaS.BLLCommonServices.FBServiceWS;
using SMT.SaaS.BLLCommonServices.OAUpdateCheckWS;

namespace SMT.SaaS.OA.BLL
{
    public class TravelmanagementBLL : BaseBll<T_OA_BUSINESSTRIP>
    {
        #region 获取所有的出差申请
        /// <summary>
        /// 获取所有的出差申请
        /// </summary>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_BUSINESSTRIP> GetTravelmanagement()
        {
            TravelmanagementDAL Travelmanagement = new TravelmanagementDAL();
            var entity = from p in Travelmanagement.GetTable()
                         orderby p.CREATEDATE descending
                         select p;
            return entity.Count() > 0 ? entity : null;
        }
        #endregion

        #region 根据申请ID获取出差申请
        /// <summary>
        /// 根据申请ID获取出差申请
        /// </summary>
        /// <param name="contractApprovalID">申请ID</param>
        /// <returns>返回结果</returns>
        public T_OA_BUSINESSTRIP GetTravelmanagementById(string TravelmanagementID)
        {
            TravelmanagementID = GetRealBusinesstripId(TravelmanagementID);

            if (string.IsNullOrWhiteSpace(TravelmanagementID))
            {
                return null;
            }

            var entsM = from a in dal.GetObjects<T_OA_BUSINESSTRIP>()
                        where a.BUSINESSTRIPID == TravelmanagementID

                        select a;
            var entsd = from a in dal.GetObjects<T_OA_BUSINESSTRIPDETAIL>()
                        where a.T_OA_BUSINESSTRIP.BUSINESSTRIPID == TravelmanagementID
                        orderby a.STARTDATE
                        select a;
            if (entsM.Count() > 0)
            {
                var q = entsM.FirstOrDefault();
                if (entsd.Count() > 0)
                {
                    var e = entsd.ToList().OrderBy(c => c.STARTDATE);
                    foreach (var a in e)
                    {
                        q.T_OA_BUSINESSTRIPDETAIL.Add(a);
                    }

                }
                return q;
            }
            return null;
        }
        public T_OA_BUSINESSTRIP GetTravelmanagementBysId()
        {
            var entity = from p in dal.GetObjects()
                         join b in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>() on p.BUSINESSTRIPID equals b.T_OA_BUSINESSTRIP.BUSINESSTRIPID
                         where p.BUSINESSTRIPID == b.T_OA_BUSINESSTRIP.BUSINESSTRIPID
                         select p;
            return entity.Count() > 0 ? entity.FirstOrDefault() : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T_OA_BUSINESSTRIP GetTravelmanagementBysId(string tripid)
        {
            var entity = from p in dal.GetObjects()
                         join b in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>() on p.BUSINESSTRIPID equals b.T_OA_BUSINESSTRIP.BUSINESSTRIPID
                         where p.BUSINESSTRIPID == b.T_OA_BUSINESSTRIP.BUSINESSTRIPID && p.BUSINESSTRIPID == tripid
                         select p;
            return entity.Count() > 0 ? entity.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据出差申请ID查询出报告ID、报销ID
        /// </summary>
        /// <param name="businesstripId">申请ID</param>
        /// <returns></returns>
        public V_Travelmanagement GetAccordingToBusinesstripIdCheck(string businesstripId)
        {
            try
            {
                businesstripId = GetRealBusinesstripId(businesstripId);

                if (string.IsNullOrWhiteSpace(businesstripId))
                {
                    return null;
                }

                var ents = (from p in dal.GetObjects<T_OA_BUSINESSTRIP>()
                            join a in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>() on p.BUSINESSTRIPID equals a.T_OA_BUSINESSTRIP.BUSINESSTRIPID
                            into a_join
                            from x in a_join.DefaultIfEmpty()
                            where p.BUSINESSTRIPID == businesstripId
                            select new V_Travelmanagement
                            {
                                TraveAppCheckState = p.CHECKSTATE,
                                TrId = x == null ? "空" : x.TRAVELREIMBURSEMENTID,
                                TrCheckState = x.CHECKSTATE,
                                NoClaims = x == null ? string.Empty : x.NOBUDGETCLAIMS,
                                Travelmanagement = p,
                            });

                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 根据传入的参数，查询到真正的出差申请的ID(我的单据中用到)
        /// </summary>
        /// <param name="businesstripId">申请ID</param>
        /// <returns></returns>
        private string GetRealBusinesstripId(string businesstripId)
        {
            string strRes = string.Empty;
            //优先检查输入ID是否在出差申请中存在对应记录
            var check1 = from c1 in dal.GetObjects<T_OA_BUSINESSTRIP>()
                         where c1.BUSINESSTRIPID == businesstripId
                         select c1;

            //如果存在，即返回当前查询到的ID
            if (check1 != null)
            {
                if (check1.FirstOrDefault() != null)
                {
                    strRes = check1.FirstOrDefault().BUSINESSTRIPID;
                }
            }

            if (!string.IsNullOrEmpty(strRes))
            {
                return strRes;
            }

            //如果不存在出差报告记录，就到出差报销中查询该ID是否存在对应的出差报销记录，并连接查询出关联的出差申请
            var check3 = from c3 in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Include("T_OA_BUSINESSTRIP")
                         where c3.TRAVELREIMBURSEMENTID == businesstripId
                         select c3;

            //如果存在出差报销记录，就返回其关联的出差申请ID
            if (check3 != null)
            {
                if (check3.FirstOrDefault() != null)
                {
                    strRes = check3.FirstOrDefault().T_OA_BUSINESSTRIP.BUSINESSTRIPID;
                }
            }
            if (!string.IsNullOrEmpty(strRes))
            {
                return strRes;
            }
            return strRes;
        }
        #endregion

        #region 添加申请
        /// <summary>
        /// 添加申请
        /// </summary>
        /// <param name="ContractApproval">申请名称</param>
        /// <returns></returns>
        public bool TravelmanagementAdd(T_OA_BUSINESSTRIP AddTravelmanagement, List<T_OA_BUSINESSTRIPDETAIL> TraveDetail)
        {
            try
            {
                AddTravelmanagement.CREATEDATE = DateTime.Now;
                Utility.RefreshEntity(AddTravelmanagement);
                foreach (var detail in TraveDetail)
                {
                    AddTravelmanagement.T_OA_BUSINESSTRIPDETAIL.Add(detail);
                    Utility.RefreshEntity(detail);
                }
                bool travelAdd = Add(AddTravelmanagement);
                if (travelAdd == true)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差申请TravelmanagementBLL-TravelmanagementAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return false;
        }
        #endregion

        #region 修改申请(自己用)
        /// <summary>
        /// 修改申请
        /// </summary>
        /// <param name="contraApproval">申请名称</param>
        /// <returns></returns>
        public bool UpdateTravelmanagement(T_OA_BUSINESSTRIP TravelmanagementUpdate, List<T_OA_BUSINESSTRIPDETAIL> TraveDetail, string FormType)
        {
            bool returnStr = true;
            dal.BeginTransaction();
            try
            {
                var entity = dal.GetObjects<T_OA_BUSINESSTRIP>().Where(s => s.BUSINESSTRIPID == TravelmanagementUpdate.BUSINESSTRIPID).FirstOrDefault();//出差申请主表

                if (entity == null)
                {
                    return false;
                }
                //避免谜一样的提交后再次发生的保存事件 在原单据已审核的情况下不再把
                if ((FormType == "Edit" || FormType == "New" || FormType == "Resubmit") && entity.CHECKSTATE!="1")
                {
                    //更新整个实体及明细
                    TravelmanagementUpdate.UPDATEDATE = DateTime.Now;
                    Utility.CloneEntity(TravelmanagementUpdate, entity);
                    entity.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_BUSINESSTRIP", "BUSINESSTRIPID", entity.BUSINESSTRIPID);
                    int i = Update(entity);
                    if (i > 0)
                    {
                        returnStr = true;
                    }
                    else
                    {
                        returnStr = false;
                    }

                    int iResult = dal.SaveContextChanges();

                    //先删除T_OA_BUSINESSTRIPDETAIL
                    var ent = dal.GetObjects<T_OA_BUSINESSTRIPDETAIL>().Where(s => s.T_OA_BUSINESSTRIP.BUSINESSTRIPID == entity.BUSINESSTRIPID);//出差申请子表

                    if (ent != null)
                    {
                        foreach (var deleteDetails in ent)
                        {
                            dal.DeleteFromContext(deleteDetails);
                        }
                        dal.SaveContextChanges();
                    }
                    //再插入T_OA_BUSINESSTRIPDETAIL
                    foreach (var updateDetails in TraveDetail)
                    {
                        T_OA_BUSINESSTRIPDETAIL detail = new T_OA_BUSINESSTRIPDETAIL();
                        Utility.CloneEntity(updateDetails, detail);
                        detail.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                        detail.T_OA_BUSINESSTRIPReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_BUSINESSTRIP", "BUSINESSTRIPID", entity.BUSINESSTRIPID);
                        dal.AddToContext(detail);
                        int iResulto = dal.SaveContextChanges();
                        if (iResulto > 0)
                        {
                            returnStr = true;
                        }
                        else
                        {
                            returnStr = false;
                        }
                    }
                }
                dal.CommitTransaction();
                Tracer.Debug("更新出差申请审核状态成功：id" + TravelmanagementUpdate.BUSINESSTRIPID + "!!!!!!!!!!!!!!!!!!!!!!!!" + "审核状态:" + TravelmanagementUpdate.CHECKSTATE);
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("出差申请TravelmanagementBLL-UpdateTravelmanagement" + TravelmanagementUpdate.BUSINESSTRIPID + " " + ex.ToString());
                return false;
            }
            return returnStr;
        }
        #endregion

        #region 通过引擎更新表单状态
        /// <summary>
        /// 通过引擎更新表单状态
        /// </summary>
        /// <param name="BorrowID"></param>
        /// <param name="StrCheckState"></param>
        /// <returns></returns>
        public int UpdateTravelRequestFromEngine(string BorrowID, string StrCheckState)
        {
            dal.BeginTransaction();
            Tracer.Debug("引擎开始更新出差申请状态：出差申请id：" + BorrowID + " 审核状态：" + StrCheckState);
            try
            {
                T_OA_BUSINESSTRIP Master = new T_OA_BUSINESSTRIP();
                Master = GetTravelmanagementById(BorrowID);
                Master.CHECKSTATE = StrCheckState;
                int i = Update(Master);
                if (i >0)
                {
                    dal.CommitTransaction();
                    Tracer.Debug("引擎更新出差报销状态成功：出差报销id：" + BorrowID + " 审核状态：" + StrCheckState);
                    if (Master.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {                                               
                        #region 开始调用HR中考勤的数据
                        Tracer.Debug("出差申请开始调用HR中考勤的接口");
                        try
                        {
                            InsertAttenceRecord(Master);
                        }
                        catch (Exception ex)
                        {
                            Tracer.Debug("出差申请调用HR中考勤的接口出现错误");
                        }
                        #endregion
                    }
                    Tracer.Debug("引擎更新出差申请状态成功：出差申请id：" + BorrowID + " 审核状态：" + StrCheckState);
                }
                else
                {
                    Tracer.Debug("引擎更新出差申请状态失败：出差申请id：" + BorrowID + " 审核状态：" + StrCheckState);
                    dal.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("出差申请TravelmanagementBLL-GetTravelRequestMobile" + BorrowID + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw ex;
            }
            return 1;
        }

        public void InsertAttenceRecord(T_OA_BUSINESSTRIP travel)
        {
            string StrMessage = "";
            List<AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD> Records = new List<AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD>();
            try
            {
                //不是审核通过状态直接退出
                if (travel.CHECKSTATE != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    StrMessage = "出差申请调用考勤数据的状态为" + travel.CHECKSTATE;
                    SMT.Foundation.Log.Tracer.Debug(StrMessage);
                    return;
                }

                List<AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD> ListRecord
                    = new List<AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD>();

                var Details = from ent in dal.GetObjects<T_OA_BUSINESSTRIPDETAIL>().Include("T_OA_BUSINESSTRIP")
                              where ent.T_OA_BUSINESSTRIP.BUSINESSTRIPID == travel.BUSINESSTRIPID
                              select ent;
                if (Details.Count() > 0)
                {
                    List<T_OA_BUSINESSTRIPDETAIL> ListDetals = Details.ToList().OrderBy(c => c.STARTDATE).ToList();
                    //List<T_OA_BUSINESSTRIPDETAIL> ListDetals = Details.ToList();
                    T_OA_BUSINESSTRIPDETAIL item = new T_OA_BUSINESSTRIPDETAIL();
                    item = ListDetals[0];

                    AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD Record = new AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
                    Record.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
                    Record.CHECKSTATE = "2";

                    Record.DESTINATION = item.DESTCITY;//出差目的地
                    Record.EMPLOYEECODE = "";
                    Record.EMPLOYEEID = travel.OWNERID;
                    Record.EMPLOYEENAME = travel.OWNERNAME;

                    Record.STARTDATE = ((DateTime)item.STARTDATE).Date;
                    Record.STARTTIME = ((DateTime)item.STARTDATE).ToShortTimeString();
                    Record.EVECTIONREASON = travel.CONTENT;//出差原因
                    Record.EVECTIONRECORDCATEGORY = "";//出差类型
                    Record.OWNERCOMPANYID = travel.OWNERCOMPANYID;
                    Record.OWNERDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    Record.OWNERPOSTID = travel.OWNERPOSTID;
                    Record.OWNERID = travel.OWNERID;

                    Record.CREATECOMPANYID = travel.OWNERCOMPANYID;
                    Record.CREATEDATE = System.DateTime.Now;
                    Record.CREATEDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    Record.CREATEPOSTID = travel.OWNERPOSTID;
                    Record.CREATEUSERID = travel.OWNERID;
                    Record.REMARK = "出差申请同步考勤生成";

                    Record.SUBSIDYTYPE = "1";//出差类型
                    Record.SUBSIDYVALUE = travel.CHARGEMONEY;//补助金额
                    Record.TOTALDAYS = string.IsNullOrEmpty(item.BUSINESSDAYS) ? 0 : Convert.ToDecimal(item.BUSINESSDAYS);//

                    Record.UPDATEDATE = DateTime.Now;
                    Record.UPDATEUSERID = travel.UPDATEUSERID;
                    //StrMessage = "出差申请开始调用考勤数据" + ListRecord.Count();
                    //如果出差明细大于1
                    if (ListDetals.Count() > 1)
                    {
                        Record.STARTDATE = ListDetals[0].STARTDATE.Value;//需要长日期格式消除考勤异常
                        Record.STARTTIME = ((DateTime)ListDetals[0].STARTDATE).ToShortTimeString();

                        Record.ENDDATE = ListDetals[ListDetals.Count() - 1].ENDDATE.Value;//需要长日期格式消除考勤异常
                        Record.ENDTIME = ((DateTime)ListDetals[ListDetals.Count() - 1].ENDDATE).ToShortTimeString();
                        Record.DESTINATION = item.DESTCITY;//出差目的地
                        ListRecord.Add(Record);
                    }
                    else
                    {
                        Record.STARTDATE = item.STARTDATE.Value;//需要长日期格式消除考勤异常
                        Record.STARTTIME = ((DateTime)item.STARTDATE).ToShortTimeString();

                        Record.ENDDATE = item.ENDDATE.Value;//需要长日期格式消除考勤异常
                        Record.ENDTIME = ((DateTime)item.ENDDATE).ToShortTimeString();
                        Record.DESTINATION = item.DESTCITY;//出差目的地

                        ListRecord.Add(Record);
                    }

                    ///修改接口的使用方式
                    try
                    {
                        if (ListRecord.Count() > 0)
                        {
                            SMT.SaaS.OA.BLL.AttendanceWS.AttendanceServiceClient Client
                                = new AttendanceWS.AttendanceServiceClient();

                            Client.AddEmployeeEvectionRdList(ListRecord.ToArray());

                            StrMessage =travel.OWNERNAME+ " 的出差申请同步考勤数据成功，出差申请开始时间："
                                        + ListRecord[0].STARTDATE.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                        + " 结束时间:" + ListRecord[0].ENDDATE.Value.ToString("yyyy-MM-dd HH:mm:ss");

                            SMT.Foundation.Log.Tracer.Debug(StrMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        SMT.Foundation.Log.Tracer.Debug(travel.OWNERNAME + " 的出差申请同步考勤数据出错::" + ex.Message.ToString());
                    }

                }

            }
            catch (Exception ex)
            {
                StrMessage = travel.OWNERNAME + " 的出差申请生成考勤数据出错" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(StrMessage);
            }
        }

        /// <summary>
        /// 根据公司ID，查找出差申请记录，同步考勤记录
        /// </summary>
        /// <param name="strCompanyID"></param>
        public void SyncAttenceRecordForCompany(string strCompanyID, DateTime dtTravel, ref string strMsg)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strCompanyID))
                {
                    strMsg = "同步考勤记录失败！请输入公司的ID";
                    return;
                }

                DateTime dtCheck = new DateTime();
                if (dtCheck >= dtTravel)
                {
                    strMsg = "同步考勤记录失败！出差月份请填写（例：2012-7）";
                    return;
                }

                string strCheckState = Convert.ToInt32(CheckStates.Approved).ToString();
                DateTime dtTravelFrom = DateTime.Parse(dtTravel.ToString("yyyy-MM") + "-1");
                DateTime dtTravelTo = dtTravelFrom.AddMonths(1).AddSeconds(-1);

                var ents = from n in dal.GetObjects()
                           where n.OWNERCOMPANYID == strCompanyID && n.CHECKSTATE == strCheckState && n.STARTDATE >= dtTravelFrom && n.STARTDATE <= dtTravelTo
                           select n;

                if (ents == null)
                {
                    strMsg = "同步考勤记录失败！该公司无出差申请记录";
                    return;
                }

                if (ents.Count() == 0)
                {
                    strMsg = "同步考勤记录失败！该公司无出差申请记录";
                    return;
                }

                foreach (T_OA_BUSINESSTRIP travel in ents)
                {
                    InsertAttenceRecord(travel);
                }

                strMsg = "同步考勤记录完成！";
            }
            catch (Exception ex)
            {
                strMsg = "同步考勤记录失败！出错了：" + ex.ToString();
                Tracer.Debug("调用SyncAttenceRecordForCompany函数出错。" + strMsg);
            }
        }
        #endregion

        #region 删除申请
        /// <summary>
        /// 删除申请
        /// </summary>
        /// <param name="contraApprovalID">申请ID</param>
        /// <returns></returns>
        public bool DeleteTravelmanagement(string[] TravelmanagementID, ref bool FBControl)
        {
            try
            {
                var entity = from ent in dal.GetObjects<T_OA_BUSINESSTRIP>().ToList()
                             where TravelmanagementID.Contains(ent.BUSINESSTRIPID)
                             select ent;

                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        //删除T_OA_BUSINESSTRIPDETAIL
                        var ent = from p in dal.GetObjects<T_OA_BUSINESSTRIPDETAIL>()
                                  where h.BUSINESSTRIPID == p.T_OA_BUSINESSTRIP.BUSINESSTRIPID
                                  select p;
                        foreach (var k in ent)
                        {
                            dal.DeleteFromContext(k);
                        }
                        int m = dal.SaveContextChanges();
                        //删除T_OA_BUSINESSTRIP
                        if (m > 0)
                        {
                            dal.DeleteFromContext(h);
                            base.DeleteMyRecord(h);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                FBServiceClient FBClient = new FBServiceClient();
                string[] StrFBMessage = FBClient.RemoveExtensionOrder(TravelmanagementID);
                if (StrFBMessage.Count() > 0)
                {
                    FBControl = false;
                }
                int iResult = dal.SaveContextChanges();
                if (iResult > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差申请TravelmanagementBLL-DeleteTravelmanagement" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return false;
        }
        #endregion


        #region 查询出差明细
        public List<T_OA_BUSINESSTRIPDETAIL> GetBusinesstripDetail(string buipId)
        {
            var entity = (from p in dal.GetObjects<T_OA_BUSINESSTRIPDETAIL>().Include("T_OA_BUSINESSTRIP")
                          join b in dal.GetObjects<T_OA_BUSINESSTRIP>() on p.T_OA_BUSINESSTRIP.BUSINESSTRIPID equals b.BUSINESSTRIPID
                          where p.T_OA_BUSINESSTRIP.BUSINESSTRIPID == buipId
                          orderby p.STARTDATE
                          select p).ToList();
            return entity;
        }
        #endregion

        #region 检查是否存在该出差申请
        /// <summary>
        /// 检查是否存在该申请
        /// </summary>
        /// <param name="ContractApproval">出差事由</param>
        /// <param name="ContractApprovalID">申请编号</param>
        /// <returns></returns>
        public bool IsExistContractTravelmanagement(string ownerid, string TravelmanagementID)
        {
            TravelmanagementDAL Travelmanagement = new TravelmanagementDAL();
            bool IsExist = false;
            var q = from cnt in Travelmanagement.GetTable()
                    where cnt.OWNERID == ownerid && cnt.BUSINESSTRIPID == TravelmanagementID
                    orderby cnt.CREATEDATE
                    select cnt;
            if (q.Count() > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }
        #endregion

        #region

        public List<string> GetTravelmanagementRoomNameInfos()
        {
            TravelmanagementDAL Travelmanagement = new TravelmanagementDAL();
            var query = from p in Travelmanagement.GetTable()
                        orderby p.CREATEDATE descending
                        select p.OWNERID;

            return query.ToList<string>();
        }

        #endregion

        #region 获取申请信息

        /// <summary>
        /// 获取申请信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_BUSINESSTRIP> GetTravelmanagementRooms()
        {
            TravelmanagementDAL Travelmanagement = new TravelmanagementDAL();
            var query = from p in Travelmanagement.GetTable()
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_BUSINESSTRIP>();

        }

        #endregion

        #region 根据条件查询申请信息
        /// <summary>
        /// 根据条件查询申请信息
        /// </summary>
        /// <param name="ownerid">出差人</param>
        /// <param name="area">区域</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<V_Travelmanagement> GetTravelmanagementRoomInfosListBySearch(string ownerid, string DepCity, string DestCity, string startTime, string endTime)
        {
            //try
            //{


            //    var q = from p in Travelmanagement.GetTable().ToList()
            //            select new V_Travelmanagement { Travelmanagement = p };

            //    if (!string.IsNullOrEmpty(ownerid))
            //    {
            //        q = q.Where(s => ownerid.Contains(s.Travelmanagement.OWNERID));
            //    }
            //    if (!string.IsNullOrEmpty(DepCity))
            //    {
            //        q = q.Where(s => DepCity.Contains(s.Travelmanagement.DEPCITY));
            //    }
            //    if (!string.IsNullOrEmpty(DestCity))
            //    {
            //        q = q.Where(s => DestCity.Contains(s.Travelmanagement.DESTCITY));
            //    }
            //    if (q.Count() > 0)
            //    {
            //        return q.ToList<V_Travelmanagement>();
            //    }
            //    return null;

            //}
            //catch (Exception ex)
            //{
            //    return null;
            //}
            return null;
        }
        #endregion

        #region 获取用户申请记录
        /// <summary>
        /// 获取用户申请记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<V_Travelmanagement> GetTravelmanagementInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                var ents = (from p in dal.GetObjects<T_OA_BUSINESSTRIP>()
                            join a in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>() on p.BUSINESSTRIPID equals a.T_OA_BUSINESSTRIP.BUSINESSTRIPID
                            into a_join
                            from x in a_join.DefaultIfEmpty()

                            select new V_Travelmanagement
                            {
                                TrId = x == null ? "空" : x.TRAVELREIMBURSEMENTID,
                                TrCheckState = x == null ? string.Empty : x.CHECKSTATE,//出差报销的审核状态
                                TraveAppCheckState = p.CHECKSTATE,//出差申请的审核状态
                                NoClaims = x == null ? string.Empty : x.NOBUDGETCLAIMS,
                                Tdetail = x == null ? 0 : x.T_OA_REIMBURSEMENTDETAIL.Count,
                                Travelmanagement = p,
                                OWNERCOMPANYID = p.OWNERCOMPANYID,
                                OWNERID = p.OWNERID,
                                OWNERPOSTID = p.OWNERPOSTID,
                                OWNERDEPARTMENTID = p.OWNERDEPARTMENTID,
                                CREATEUSERID = p.CREATEUSERID
                            });

                if (ents.Count() > 0)
                {
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    string ids = string.Empty;
                    if (flowInfoList == null)
                    {

                        UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_BUSINESSTRIP");

                        if (!string.IsNullOrEmpty(checkState))
                        {
                            if (!string.IsNullOrEmpty(filterString))
                            {
                                filterString += " and ";
                                filterString += "( TraveAppCheckState ==@" + queryParas.Count();
                                queryParas.Add(checkState);
                                filterString += " or TrCheckState ==@" + queryParas.Count() + ")";
                                queryParas.Add(checkState);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < flowInfoList.Count; i++)
                        {
                            ids += flowInfoList[i].FormID;
                            if (i != flowInfoList.Count - 1)
                            {
                                ids += ",";
                            }
                        }
                        ids = ids.Substring(0, ids.Length - 1);//去掉最后的,
                    }
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.Where(filterString, queryParas.ToArray());
                    }
                    if (ids.Length > 0)
                    {
                        string[] ArrAuditIds = ids.Split(',');
                        if (ArrAuditIds != null)
                        {
                            if (ArrAuditIds.Count() > 0)
                            {
                                ents = ents.Where(p => ArrAuditIds.Contains(p.Travelmanagement.BUSINESSTRIPID) || ArrAuditIds.Contains(p.TrId));
                            }
                        }
                    }
                    ents = ents.OrderBy(sort).ToList().AsQueryable();
                    ents = Utility.Pager<V_Travelmanagement>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);

                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差申请TravelmanagementBLL-GetTravelmanagementInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 出差申请是否能被查看
        /// <summary>
        /// 查询出差申请是否能被查看
        /// </summary>
        /// <param name="archivesID"></param>
        /// <returns></returns>
        public bool IsContractCanBrowser(string archivesID)
        {
            //var entity = from q in dal.GetObjects<T_OA_BUSINESSTRIP>()
            //             where q.BUSINESSTRIPID == archivesID && q.ENDDATE != q.STARTDATE
            //             && q.CHECKSTATE == "1"
            //             select q;
            //if (entity.Count() > 0)
            //{
            //    return true;
            //}
            return false;
        }
        #endregion

        //#region 获取出差申请记录(写出差报告时选出差申请用)
        ///// <summary>
        ///// 获取出差申请记录(将已做报告及已报销的数据过滤)
        ///// </summary>
        ///// <param name="userID">用户ID</param>
        ///// <returns></returns>
        //public List<V_Travelmanagement> GetCheckTravelmanagement(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        //{
        //    try
        //    {
        //        List<V_Travelmanagement> listaa = new List<V_Travelmanagement>();

        //        var q = from t in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Include("T_OA_BUSINESSREPORT").Include("T_OA_BUSINESSREPORT.T_OA_BUSINESSTRIP")
        //                where t.CHECKSTATE == "2"
        //                select t.T_OA_BUSINESSREPORT.T_OA_BUSINESSTRIP;

        //        var en = dal.GetObjects<T_OA_BUSINESSTRIP>().Where(t => t.CHECKSTATE == "2").Except(q);

        //        var ents = from c in en
        //                   select new V_Travelmanagement
        //                   {
        //                       Travelmanagement = c,
        //                       OWNERCOMPANYID = c.OWNERCOMPANYID,
        //                       OWNERID = c.OWNERID,
        //                       OWNERPOSTID = c.OWNERPOSTID,
        //                       OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
        //                       CREATEUSERID = c.CREATEUSERID
        //                   };
        //        if (ents.Count() > 0)
        //        {
        //            if (flowInfoList != null)
        //            {
        //                ents = (from a in ents.ToList().AsQueryable()
        //                        join l in flowInfoList on a.Travelmanagement.BUSINESSTRIPID equals l.FormID
        //                        select new V_Travelmanagement
        //                        {
        //                            Travelmanagement = a.Travelmanagement,
        //                            OWNERCOMPANYID = a.OWNERCOMPANYID,
        //                            OWNERID = a.OWNERID,
        //                            OWNERPOSTID = a.OWNERPOSTID,
        //                            OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
        //                            CREATEUSERID = a.CREATEUSERID
        //                        });
        //            }
        //            if (!string.IsNullOrEmpty(checkState))
        //            {
        //                ents = ents.Where(s => checkState == s.Travelmanagement.CHECKSTATE);
        //            }
        //            List<object> queryParas = new List<object>();
        //            queryParas.AddRange(paras);
        //            if (flowInfoList == null)
        //            {
        //                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_BUSINESSTRIP");
        //            }
        //            if (!string.IsNullOrEmpty(filterString))
        //            {
        //                ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
        //            }
        //            ents = ents.OrderBy(sort).ToList().AsQueryable();
        //            ents = Utility.Pager<V_Travelmanagement>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);

        //            foreach (var ent in ents)
        //            {
        //                string StartCity = "";//出发城市
        //                string EndCity = "";//目标城市
        //                string StartTime = "";//开始时间
        //                string EndTime = "";//结束时间
        //                var detailinfos = from info in dal.GetObjects<T_OA_BUSINESSTRIPDETAIL>().Include("T_OA_BUSINESSTRIP")
        //                                  where info.T_OA_BUSINESSTRIP.BUSINESSTRIPID == ent.Travelmanagement.BUSINESSTRIPID
        //                                  select info;
        //                detailinfos = detailinfos.OrderBy(p => p.STARTDATE);
        //                if (detailinfos.Count() > 1)//存在多条出差详情信息
        //                {
        //                    StartCity = detailinfos.ToList().FirstOrDefault().DEPCITY.ToString();
        //                    EndCity = detailinfos.ToList().LastOrDefault().DESTCITY.ToString();
        //                    StartTime = detailinfos.ToList().FirstOrDefault().STARTDATE.ToString();
        //                    EndTime = detailinfos.ToList().LastOrDefault().ENDDATE.ToString();

        //                    if (StartCity == EndCity)//如果刚开始的出发城市和最后一条记录的目的城市  则取最后一条记录的出发城市
        //                    {
        //                        EndCity = detailinfos.ToList().LastOrDefault().DEPCITY.ToString();
        //                    }
        //                }
        //                if (detailinfos.Count() == 1)//只存在一条记录的情况
        //                {
        //                    StartCity = detailinfos.ToList().FirstOrDefault().DEPCITY.ToString();
        //                    EndCity = detailinfos.ToList().LastOrDefault().DESTCITY.ToString();
        //                    StartTime = detailinfos.ToList().FirstOrDefault().STARTDATE.ToString();
        //                    EndTime = detailinfos.ToList().LastOrDefault().ENDDATE.ToString();
        //                }
        //                ent.Cicty = StartCity;
        //                ent.Cictys = EndCity;
        //                ent.Startdate = StartTime;
        //                ent.Endtime = EndTime;
        //                listaa.Add(ent);
        //            }
        //            ents = listaa.AsQueryable();
        //            return ents.ToList();
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("出差申请TravelmanagementBLL-GetCheckTravelmanagement" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //        return null;
        //    }
        //}
        //#endregion

        #region 获取用户是否有未处理出差申请和出差报销
        /// <summary>
        /// Added   :罗捷
        /// Date    :2012/11/1
        /// For     :获取某用户的未完成的出差报销和出差申请
        /// </summary>
        /// <param name="employeeid">用户id</param>
        /// <returns>“出差申请”或“出差报销”：相应formID</returns>
        public Dictionary<string,string> GetUnderwayTravelmanagement(string employeeid)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(employeeid))
                {
                    Dictionary<string, string> DicTrip = new Dictionary<string, string>();
                    //获取未完成的出差申请
                    var UnderwayBtripIds = from b in dal.GetObjects<T_OA_BUSINESSTRIP>()
                                           where b.OWNERID == employeeid
                                                 && (b.CHECKSTATE == "0" || b.CHECKSTATE == "1")
                                           select b.BUSINESSTRIPID;
                    int countAdd = 0;
                    foreach (var ub in UnderwayBtripIds)
                    {
                        DicTrip.Add("T_OA_BUSINESSTRIP"+countAdd.ToString(), ub);
                        countAdd++;
                    }

                    //获取未完成的出差报销
                    var UnderwayReimbursement = from r in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>()
                                                join b in dal.GetObjects<T_OA_BUSINESSTRIP>() on r.T_OA_BUSINESSTRIP.BUSINESSTRIPID equals b.BUSINESSTRIPID
                                                where b.CHECKSTATE=="2" && (r.CHECKSTATE=="0" || r.CHECKSTATE=="1") && r.OWNERID==employeeid
                                                select r.TRAVELREIMBURSEMENTID;
                    countAdd = 0;
                    foreach (var ur in UnderwayReimbursement)
                    {
                        DicTrip.Add("T_OA_TRAVELREIMBURSEMENT"+countAdd.ToString(), ur);
                        countAdd++;
                    }

                    //返回Dictionary<"出差申请","formid">格式信息
                    if (DicTrip.Count() > 0)
                    {
                        return DicTrip;
                    }
                    else
                    {
                        string logger = "TravelmanagementBll-GetUnderwayTravelmanagement:"+employeeid;
                        logger += "没有未解决的出差信息";
                        Tracer.Debug(logger);
                    }
                }
                else
                {
                    string logger = "TravelmanagementBll-GetUnderwayTravelmanagement:" + employeeid;
                    logger += "未获取用户的employeeid";
                    Tracer.Debug(logger);
                }
            }
            catch (Exception ex)
            {
                string logger = "TravelmanagementBll-GetUnderwayTravelmanagement:"+employeeid +" => " + ex.ToString();
                Tracer.Debug(logger);
            }
            return null;
        }
        #endregion
    }
}
