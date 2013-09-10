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

namespace SMT.SaaS.OA.BLL
{
    public class TravelReimbursementBLL : BaseBll<T_OA_TRAVELREIMBURSEMENT>
    {
        private TravelReimbursementDAL trdal;

        #region 获取所有的出差报销
        /// <summary>
        /// 获取所有的出差报销
        /// </summary>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_TRAVELREIMBURSEMENT> GetTravelReimbursement()
        {
            trdal = new TravelReimbursementDAL();
            var entity = from p in trdal.GetTable()
                         orderby p.CREATEDATE descending
                         select p;
            return entity.Count() > 0 ? entity : null;
        }
        #endregion

        #region 根据报销ID获取出差报销
        /// <summary>
        /// 根据报销ID获取出差报销
        /// </summary>
        /// <param name="TravelReimbursementID">报销ID</param>
        /// <returns>返回结果</returns>
        public T_OA_TRAVELREIMBURSEMENT GetTravelReimbursementById(string TravelReimbursementID)
        {
            //var ents = from a in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Include("T_OA_REIMBURSEMENTDETAIL")
            //           join b in dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>() on a.TRAVELREIMBURSEMENTID equals b.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID
            //           where a.TRAVELREIMBURSEMENTID == TravelReimbursementID     
            //           orderby b.STARTDATE
            //           select a;
            //return ents.Count() > 0 ? ents.FirstOrDefault() : null;

            var entsM = from a in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>()
                        where a.TRAVELREIMBURSEMENTID == TravelReimbursementID

                        select a;
            var entsd = from a in dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>()
                        where a.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID == TravelReimbursementID
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
                        q.T_OA_REIMBURSEMENTDETAIL.Add(a);
                    }

                }
                return q;
            }
            return null;

        }
        #endregion

        #region 添加报销
        /// <summary>
        /// 添加报销
        /// </summary>
        /// <param name="AddTravelReimbursement">报销实体</param>
        /// <param name="portDetail">报销明细</param>
        /// <returns></returns>
        public bool TravelReimbursementAdd(T_OA_TRAVELREIMBURSEMENT AddTravelReimbursement, List<T_OA_REIMBURSEMENTDETAIL> portDetail)
        {
            try
            {
                //先添加主表数据
                dal.BeginTransaction();
                string businesstripId = AddTravelReimbursement.T_OA_BUSINESSTRIP.BUSINESSTRIPID;
                Tracer.Debug("添加出差申请ID为" + AddTravelReimbursement.T_OA_BUSINESSTRIP.BUSINESSTRIPID + "的出差报销" + AddTravelReimbursement.TRAVELREIMBURSEMENTID);
                AddTravelReimbursement.CREATEDATE = DateTime.Now;
                AddTravelReimbursement.UPDATEDATE = DateTime.Now;
                Utility.RefreshEntity(AddTravelReimbursement);

                bool isExist = CheckTravelReimbursementByBusinesstrip(businesstripId);
                bool travelAdd = false;
                if (!isExist)
                {
                    travelAdd = Add(AddTravelReimbursement);
                    //再添加子表数据
                    foreach (var addDetails in portDetail)
                    {
                        addDetails.T_OA_TRAVELREIMBURSEMENTReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELREIMBURSEMENT", "TRAVELREIMBURSEMENTID", AddTravelReimbursement.TRAVELREIMBURSEMENTID);
                        dal.AddToContext(addDetails);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        dal.CommitTransaction();
                        return true;
                    }
                    else
                    {
                        dal.RollbackTransaction();
                        return false;
                    }
                }
                else
                {
                    Tracer.Debug("添加失败，已存在出差申请" + businesstripId + "的出差报销");
                }
                return false;
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("出差报销TravelReimbursementBLL-TravelReimbursementAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }
        #endregion

        public void DeleteTheSameTravelreimbursement(string busntpid)
        {
            try
            {
                using (trdal = new TravelReimbursementDAL())
                {
                    //此方法用于检测是否有自动生成的出差报销，因此去checkstate为0，即未提交的
                    var entity = from t in trdal.GetObjects()
                                 where t.T_OA_BUSINESSTRIP.BUSINESSTRIPID == busntpid && t.CHECKSTATE == "0"
                                 select t;
                    if (entity.Count() > 1)
                    {
                        var entList = entity.ToList();
                        int countList = entList.Count() - 1;
                        for (int i = 0; i < countList; i++)
                        {
                            string trid = entList[i].TRAVELREIMBURSEMENTID;
                            var ent = trdal.GetObjects<T_OA_REIMBURSEMENTDETAIL>().Include("T_OA_TRAVELREIMBURSEMENT")
                                      .Where(p=>p.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID == trid);

                            if (ent!=null)
                            {
                                foreach (var td in ent)
                                {
                                    dal.DeleteFromContext(td) ;
                                }
                            }

                            Tracer.Debug("delete " + trid);
                            if (entList[i] != null)
                            {
                                if(entList[i]!=null)
                                    base.DeleteMyRecord(entList[i]);
                                if(entList[i]!=null)
                                    dal.DeleteFromContext(entList[i]);
                            }                            
                        }
                        dal.SaveContextChanges();
                    }
                    else
                    {
                        Tracer.Debug("引擎添加出差报销成功，且只有一条记录。");
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-DeleteTheSameTravelReimbursement" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }
        }

        #region 更新报销单号
        public bool UpdateNoClaims(T_OA_TRAVELREIMBURSEMENT TravelNoClaims, List<T_OA_REIMBURSEMENTDETAIL> portDetail, string FormType)
        {
            bool returnStr = true;
            try
            {
                var entity = dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Where(s => s.TRAVELREIMBURSEMENTID == TravelNoClaims.TRAVELREIMBURSEMENTID).FirstOrDefault();//出差报销主表

                if (entity == null)
                {
                    return false;
                }

                if (FormType == "Edit" || FormType == "New" || FormType == "Resubmit")
                {
                    TravelNoClaims.UPDATEDATE = DateTime.Now;
                    Utility.CloneEntity(TravelNoClaims, entity);
                    entity.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELREIMBURSEMENT", "TRAVELREIMBURSEMENTID", entity.TRAVELREIMBURSEMENTID);
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

                    //先删除T_OA_REIMBURSEMENTDETAIL
                    var ent = dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>().Where(s => s.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID == entity.TRAVELREIMBURSEMENTID);//出差报销子表

                    if (ent != null)
                    {
                        foreach (var deleteDetails in ent)
                        {
                            dal.DeleteFromContext(deleteDetails);
                        }
                        dal.SaveContextChanges();
                    }
                    //再插入T_OA_REIMBURSEMENTDETAIL
                    foreach (var updateDetails in portDetail)
                    {
                        T_OA_REIMBURSEMENTDETAIL detail = new T_OA_REIMBURSEMENTDETAIL();
                        Utility.CloneEntity(updateDetails, detail);
                        detail.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();
                        detail.T_OA_TRAVELREIMBURSEMENTReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELREIMBURSEMENT", "TRAVELREIMBURSEMENTID", entity.TRAVELREIMBURSEMENTID);
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
                else if (entity.CHECKSTATE == ((int)CheckStates.Approving).ToString())//更新审核状态、时间
                {
                    //只更新状态
                    entity.NOBUDGETCLAIMS = TravelNoClaims.NOBUDGETCLAIMS;
                    entity.UPDATEDATE = DateTime.Now;
                    int i = Update(entity);
                    if (i > 0)
                    {
                        returnStr = true;
                    }
                    else
                    {
                        returnStr = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-UpdateNoClaims" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
            return returnStr;
        }
        #endregion

        #region 修改报销
        /// <summary>
        /// 修改报销
        /// </summary>
        /// <param name="TravelReimbursementUpdate">报销实体</param>
        /// <param name="portDetail">报销明细</param>
        /// <returns></returns>
        public bool UpdateTravelReimbursement(T_OA_TRAVELREIMBURSEMENT TravelReimbursementUpdate, List<T_OA_REIMBURSEMENTDETAIL> portDetail, string FormType)
        {
            bool returnStr = true;
            dal.BeginTransaction();
            try
            {
                trdal = new TravelReimbursementDAL();
                var entity = dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Where(s => s.TRAVELREIMBURSEMENTID == TravelReimbursementUpdate.TRAVELREIMBURSEMENTID).FirstOrDefault();//出差报销主表

                if (entity == null)
                {
                    return false;
                }

                if (FormType == "Edit" || FormType == "New" || FormType == "Resubmit")
                {
                    TravelReimbursementUpdate.UPDATEDATE = DateTime.Now;
                    Utility.CloneEntity(TravelReimbursementUpdate, entity);
                    entity.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELREIMBURSEMENT", "TRAVELREIMBURSEMENTID", entity.TRAVELREIMBURSEMENTID);
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

                    //先删除T_OA_REIMBURSEMENTDETAIL
                    var ent = dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>().Where(s => s.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID == entity.TRAVELREIMBURSEMENTID);//出差报销子表

                    if (ent != null)
                    {
                        foreach (var deleteDetails in ent)
                        {
                            dal.DeleteFromContext(deleteDetails);
                        }
                        dal.SaveContextChanges();
                    }
                    //再插入T_OA_REIMBURSEMENTDETAIL
                    foreach (var updateDetails in portDetail)
                    {
                        T_OA_REIMBURSEMENTDETAIL detail = new T_OA_REIMBURSEMENTDETAIL();
                        Utility.CloneEntity(updateDetails, detail);
                        detail.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();
                        detail.T_OA_TRAVELREIMBURSEMENTReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELREIMBURSEMENT", "TRAVELREIMBURSEMENTID", entity.TRAVELREIMBURSEMENTID);
                        dal.AddToContext(detail);
                    }
                    int iResulto = dal.SaveContextChanges();
                    if (iResulto > 0)
                    {
                        dal.CommitTransaction();
                        returnStr = true;
                    }
                    else
                    {
                        dal.RollbackTransaction();
                        returnStr = false;
                    }
                }
                else if (entity.CHECKSTATE == ((int)CheckStates.Approving).ToString())//更新审核状态、时间
                {
                    //只更新状态
                    //entity.CHECKSTATE = TravelReimbursementUpdate.CHECKSTATE;
                    entity.UPDATEDATE = DateTime.Now;
                    int i = Update(entity);
                    if (i > 0)
                    {
                        dal.CommitTransaction();
                        returnStr = true;
                    }
                    else
                    {
                        dal.RollbackTransaction();
                        returnStr = false;
                    }
                }
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("出差报销TravelReimbursementBLL-UpdateTravelReimbursement" + System.DateTime.Now.ToString() + " " + ex.ToString());
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
        public int UpdateTravelReimbursementFromEngine(string BorrowID, string StrCheckState)
        {
            dal.BeginTransaction();
          
            string oldCheckState = string.Empty;
            try
            { 
                T_OA_TRAVELREIMBURSEMENT Master = new T_OA_TRAVELREIMBURSEMENT();
                FBServiceClient FBClient = new FBServiceClient();
                string strMsg = "";//string.Empty;
                Master = (from ent in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>()
                          where ent.TRAVELREIMBURSEMENTID == BorrowID
                          select ent).FirstOrDefault();//GetTravelReimbursementById(BorrowID);
                oldCheckState = Master.CHECKSTATE;//保留旧的审核状态
               
                Master.CHECKSTATE = StrCheckState;

                Master.UPDATEDATE = DateTime.Now;
                Master.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_TRAVELREIMBURSEMENT", "TRAVELREIMBURSEMENTID", Master.TRAVELREIMBURSEMENTID);
               

                Tracer.Debug("事务开始 出差报销引擎开始更新预算关联单据：出差报销id：" + BorrowID + " 审核状态：" + StrCheckState);
                var fbResult = FBClient.UpdateExtensionOrder("Travel", BorrowID, StrCheckState, ref strMsg);//手机审单时通过后台修改个人费用报销中对应的报销单据状态

                if (string.IsNullOrEmpty(strMsg))//预算没有错误才执行改变表单状态的操作
                {
                    if (fbResult == null)
                    {
                        dal.RollbackTransaction();
                        Tracer.Debug("事务中 出差报销引擎开始更新预算关联单据失败:"
                            + "no FB.Result was returned");
                        throw new Exception("no FB.Result was returned");
                    }
                    else
                    {
                        Tracer.Debug("事务中 出差报销引擎开始更新预算关联单据成功，预算生成单号:"
                            + fbResult.INNERORDERCODE + " 审核状态：" + StrCheckState);
                    }

                    if (oldCheckState == "0" && StrCheckState == "1")
                    {
                        string fbOrderCode = fbResult.INNERORDERCODE;
                        Master.NOBUDGETCLAIMS = fbOrderCode;
                    }

                    int i = Update(Master);
                    if (i > 0)
                    {
                        Tracer.Debug("事务中 引擎更新出差报销状态成功：出差报销id：" + BorrowID + " 审核状态：" + StrCheckState);

                        if (Master.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                        {
                            #region 开始调用HR中考勤的数据
                            Tracer.Debug("事务中 引擎开始更新出差报销状态：出差报销id：" + BorrowID + " 审核状态：" + StrCheckState);
                            try
                            {
                                InsertAttenceRecord(Master);
                            }
                            catch (Exception ex)
                            {
                                Tracer.Debug("事务回滚 调用HR中考勤的接口出现错误:" + ex.ToString());
                                dal.RollbackTransaction();
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        Tracer.Debug("事务回滚 引擎更新出差报销单号失败,受影响的记录数小于1：出差报销id："
                            + BorrowID + " 审核状态：" + StrCheckState);
                        dal.RollbackTransaction();
                        throw new Exception(strMsg);
                    }



                }
                else
                {
                    Tracer.Debug("事务回滚 引擎更新出差报销状态失败：" + BorrowID
                        + System.DateTime.Now.ToString() + " " + " 出差报销引擎开始更新预算关联单据返回结果为空");
                    dal.RollbackTransaction();
                    throw new Exception(strMsg);
                }
                Tracer.Debug("事务确认成功：引擎更新出差报销，同步预算单，考勤记录成功！");
                dal.CommitTransaction();
                //更新元数据单号
                //var BUSINESSTRIPID = (from ent in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Include("T_OA_BUSINESSTRIP")
                //                     where ent.TRAVELREIMBURSEMENTID == Master.TRAVELREIMBURSEMENTID
                //                     select ent).First().T_OA_BUSINESSTRIP.BUSINESSTRIPID;

                //UpdateEntityXML(Master.TRAVELREIMBURSEMENTID
                //    , "自动生成", Master.NOBUDGETCLAIMS);
                //Tracer.Debug("出差更新元数据中出差单号成功！");
            }
            catch (Exception ex)
            {
                Tracer.Debug("事务回滚 引擎更新出差报销状态失败：" + BorrowID + System.DateTime.Now.ToString() + " " + ex.ToString());
                dal.RollbackTransaction();
                throw ex;
            }
            return 1;
        }

        public string UpdateEntityXML(string Formid, string OldString, string ReplaceString)
        {
            try
            {
                TravelReimbursementBLL bll = new TravelReimbursementBLL();
                ReplaceString = (from ent in bll.dal.GetObjects()
                                 select ent.NOBUDGETCLAIMS).FirstOrDefault();
                if (string.IsNullOrEmpty(ReplaceString))
                {
                    Tracer.Debug("出差报销提交审核替换元数据单号，获取的单号为空：" + ReplaceString);
                    return "";
                }
                //更新元数据里的报销单号
                SMT.SaaS.BLLCommonServices.FlowWFService.ServiceClient client =
                new SaaS.BLLCommonServices.FlowWFService.ServiceClient();
                Tracer.Debug("开始调用元数据获取接口：FlowWFService.GetMetadataByFormid(" + Formid + ")");
                string xml = string.Empty;
                xml = client.GetMetadataByFormid(Formid);
                Tracer.Debug("获取到的元数据：" + xml);
                xml = xml.Replace("自动生成", ReplaceString);
                Tracer.Debug("替换单号后的XML:" + xml);
                bool flag = client.UpdateMetadataByFormid(Formid, xml);
                if (flag)
                {
                    Tracer.Debug("出差报销元数据替换单号成功：" + ReplaceString);
                    return "";
                }
                else
                {
                    Tracer.Debug("出差报销元数据替换单号UpdateMetadataByFormid返回false：Formid：" + Formid
                        + OldString
                        + ReplaceString);
                    return "";
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
                return "";
            }

        }

        public void InsertAttenceRecord(T_OA_TRAVELREIMBURSEMENT travel)
        {
            string StrMessage = "";
            try
            {
                //不是审核通过状态直接退出
                if (travel.CHECKSTATE != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    StrMessage = "出差报销调用考勤数据的状态为" + travel.CHECKSTATE;
                    SMT.Foundation.Log.Tracer.Debug(StrMessage);
                    return;
                }

                List<AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD> ListRecord 
                    = new List<AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD>();
                
                var Details = from ent in dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>().Include("T_OA_TRAVELREIMBURSEMENT")
                              where ent.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID == travel.TRAVELREIMBURSEMENTID                              
                              select ent;
                if (Details.Count() > 0)
                {
                    List<T_OA_REIMBURSEMENTDETAIL> ListDetals = Details.ToList().OrderBy(c => c.STARTDATE).ToList();                   
                    T_OA_REIMBURSEMENTDETAIL item = new T_OA_REIMBURSEMENTDETAIL();
                    item = ListDetals[0];

                    AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD Record = new AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
                    Record.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
                    Record.CHECKSTATE = "2";
                    Record.EMPLOYEEID = travel.OWNERID;
                    Record.EMPLOYEENAME = travel.CLAIMSWERENAME;
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
                    Record.REMARK = "出差报销自动产生";
                    Record.SUBSIDYTYPE = "1";//出差类型
                    Record.SUBSIDYVALUE = travel.THETOTALCOST;//补助金额
                    /// Modified by 罗捷
                    /// 作用：传来的item.BUSINESSDAYS数据为“17天”，导致Convert.ToDecimal方法异常
                    string BUSINESSDAYS = CheckBusinessday(item.T_OA_TRAVELREIMBURSEMENT.COMPUTINGTIME);
                    Record.TOTALDAYS = string.IsNullOrEmpty(BUSINESSDAYS) ? 0 : Convert.ToDecimal(BUSINESSDAYS);//
                    Record.T_HR_EMPLOYEEEVECTIONREPORT = null;//
                    Record.UPDATEDATE = DateTime.Now;
                    Record.UPDATEUSERID = travel.OWNERID;
                    Record.EMPLOYEECODE = "";

                    //如果出差明细大于1
                    if (ListDetals.Count() > 1)
                    {
                        Record.STARTDATE = ListDetals[0].STARTDATE.Value;
                        Record.STARTTIME = ((DateTime)ListDetals[0].STARTDATE).ToShortTimeString();
                        Record.ENDDATE = ListDetals[ListDetals.Count()-1].ENDDATE.Value;
                        Record.ENDTIME = ((DateTime)ListDetals[ListDetals.Count() - 1].ENDDATE).ToShortTimeString();
                        Record.DESTINATION = item.DESTCITY;//出差目的地
                        ListRecord.Add(Record);
                    }
                    else
                    {
                        Record.STARTDATE = item.STARTDATE.Value;
                        Record.STARTTIME = ((DateTime)item.STARTDATE).ToShortTimeString();
                       
                        Record.ENDDATE = item.ENDDATE.Value;
                        Record.ENDTIME = ((DateTime)item.ENDDATE).ToShortTimeString();
                        Record.DESTINATION = item.DESTCITY;//出差目的地
                        
                        ListRecord.Add(Record);
                    }

                    #region 旧逻辑
                    //for (int i = 0; i < ListDetals.Count(); i++)
                    //{
                    //    T_OA_REIMBURSEMENTDETAIL item = new T_OA_REIMBURSEMENTDETAIL();
                    //    item = ListDetals[i];

                    //    #region 不是私事的情况
                    //    //ListDetals.Count() - 2  为最后一条出差记录的时间计算
                    //    if (item.PRIVATEAFFAIR == "0" && i < ListDetals.Count() - 1)
                    //    {
                    //        SMT.Foundation.Log.Tracer.Debug(i.ToString());
                    //        AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD Record = new AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
                    //        Record.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
                    //        Record.CHECKSTATE = "2";

                    //        Record.DESTINATION = item.DESTCITY;//出差目的地
                    //        Record.EMPLOYEECODE = "";
                    //        Record.EMPLOYEEID = travel.OWNERID;
                    //        Record.EMPLOYEENAME = travel.CLAIMSWERENAME;
                    //        //正常情况下结束时间为目的地的开始时间
                    //        if (i < ListDetals.Count() - 2)
                    //        {
                    //            Record.ENDDATE = ((DateTime)ListDetals[i + 1].STARTDATE).Date;
                    //            Record.ENDTIME = ((DateTime)ListDetals[i + 1].STARTDATE).ToShortTimeString();
                    //        }
                    //        else
                    //        {
                    //            Record.ENDDATE = ((DateTime)ListDetals[i].ENDDATE).Date;
                    //            Record.ENDTIME = ((DateTime)ListDetals[i].ENDDATE).ToShortTimeString();
                    //        }
                    //        Record.STARTDATE = ((DateTime)item.STARTDATE).Date;
                    //        Record.STARTTIME = ((DateTime)item.STARTDATE).ToShortTimeString();
                    //        Record.EVECTIONREASON = travel.CONTENT;//出差原因
                    //        Record.EVECTIONRECORDCATEGORY = "";//出差类型
                    //        Record.OWNERCOMPANYID = travel.OWNERCOMPANYID;
                    //        Record.OWNERDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    //        Record.OWNERPOSTID = travel.OWNERPOSTID;
                    //        Record.OWNERID = travel.OWNERID;
                    //        Record.CREATECOMPANYID = travel.OWNERCOMPANYID;
                    //        Record.CREATEDATE = System.DateTime.Now;
                    //        Record.CREATEDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    //        Record.CREATEPOSTID = travel.OWNERPOSTID;
                    //        Record.CREATEUSERID = travel.OWNERID;
                    //        Record.REMARK = "出差报销自动产生";

                    //        Record.SUBSIDYTYPE = "1";//出差类型
                    //        Record.SUBSIDYVALUE = travel.THETOTALCOST;//补助金额
                    //        /// Modified by 罗捷
                    //        /// 作用：传来的item.BUSINESSDAYS数据为“17天”，导致Convert.ToDecimal方法异常
                    //        item.BUSINESSDAYS = CheckBusinessday(item.BUSINESSDAYS);
                    //        Record.TOTALDAYS = string.IsNullOrEmpty(item.BUSINESSDAYS) ? 0 : Convert.ToDecimal(item.BUSINESSDAYS);//
                    //        Record.T_HR_EMPLOYEEEVECTIONREPORT = null;//
                    //        Record.UPDATEDATE = DateTime.Now;
                    //        Record.UPDATEUSERID = travel.OWNERID;
                    //        ListRecord.Add(Record);
                    //    }
                    //    else
                    //    {
                    //        if (i == ListDetals.Count() - 2)
                    //        {
                    //            AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD Record = new AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
                    //            Record.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
                    //            Record.CHECKSTATE = "2";

                    //            Record.DESTINATION = item.DESTCITY;//出差目的地
                    //            Record.EMPLOYEECODE = "";
                    //            Record.EMPLOYEEID = travel.OWNERID;
                    //            Record.EMPLOYEENAME = travel.CLAIMSWERENAME;
                    //            //正常情况下结束时间为目的地的开始时间

                    //            Record.ENDDATE = ((DateTime)ListDetals[i].ENDDATE).Date;
                    //            Record.ENDTIME = ((DateTime)ListDetals[i].ENDDATE).ToShortTimeString();
                    //            //上次出差记录为非私事
                    //            if (ListDetals[i - 1].PRIVATEAFFAIR == "0")
                    //            {
                    //                Record.STARTDATE = ((DateTime)ListDetals[i - 1].STARTDATE).Date;
                    //                Record.STARTTIME = ((DateTime)ListDetals[i - 1].STARTDATE).ToShortTimeString();
                    //            }
                    //            else
                    //            {
                    //                Record.STARTDATE = ((DateTime)item.STARTDATE).Date;
                    //                Record.STARTTIME = ((DateTime)item.STARTDATE).ToShortTimeString();
                    //            }
                    //            Record.EVECTIONREASON = travel.CONTENT;//出差原因
                    //            Record.EVECTIONRECORDCATEGORY = "";//出差类型
                    //            Record.OWNERCOMPANYID = travel.OWNERCOMPANYID;
                    //            Record.OWNERDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    //            Record.OWNERPOSTID = travel.OWNERPOSTID;
                    //            Record.OWNERID = travel.OWNERID;
                    //            Record.CREATECOMPANYID = travel.OWNERCOMPANYID;
                    //            Record.CREATEDATE = System.DateTime.Now;
                    //            Record.CREATEDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    //            Record.CREATEPOSTID = travel.OWNERPOSTID;
                    //            Record.CREATEUSERID = travel.OWNERID;
                    //            Record.REMARK = "出差报销自动产生";

                    //            Record.SUBSIDYTYPE = "1";//出差类型
                    //            Record.SUBSIDYVALUE = travel.THETOTALCOST;//补助金额
                    //            /// Modified by 罗捷
                    //            /// 作用：传来的item.BUSINESSDAYS数据为“17天”，导致Convert.ToDecimal方法异常
                    //            item.BUSINESSDAYS = CheckBusinessday(item.BUSINESSDAYS);
                    //            Record.TOTALDAYS = string.IsNullOrEmpty(item.BUSINESSDAYS) ? 0 : Convert.ToDecimal(item.BUSINESSDAYS);//
                    //            Record.T_HR_EMPLOYEEEVECTIONREPORT = null;//
                    //            Record.UPDATEDATE = DateTime.Now;
                    //            Record.UPDATEUSERID = travel.OWNERID;
                    //            ListRecord.Add(Record);
                    //        }
                    //        else
                    //        {
                    //            SMT.Foundation.Log.Tracer.Debug(i.ToString());
                    //            AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD Record = new AttendanceWS.T_HR_EMPLOYEEEVECTIONRECORD();
                    //            Record.EVECTIONRECORDID = System.Guid.NewGuid().ToString();
                    //            Record.CHECKSTATE = "2";

                    //            Record.DESTINATION = item.DESTCITY;//出差目的地
                    //            Record.EMPLOYEECODE = "";
                    //            Record.EMPLOYEEID = travel.OWNERID;
                    //            Record.EMPLOYEENAME = travel.CLAIMSWERENAME;
                    //            //正常情况下结束时间为目的地的开始时间
                    //            if (i < ListDetals.Count() - 2)
                    //            {
                    //                Record.ENDDATE = ((DateTime)ListDetals[i + 1].STARTDATE).Date;
                    //                Record.ENDTIME = ((DateTime)ListDetals[i + 1].STARTDATE).ToShortTimeString();
                    //            }
                    //            else
                    //            {
                    //                Record.ENDDATE = ((DateTime)ListDetals[i].ENDDATE).Date;
                    //                Record.ENDTIME = ((DateTime)ListDetals[i].ENDDATE).ToShortTimeString();
                    //            }
                    //            Record.STARTDATE = ((DateTime)item.STARTDATE).Date;
                    //            Record.STARTTIME = ((DateTime)item.STARTDATE).ToShortTimeString();
                    //            Record.EVECTIONREASON = travel.CONTENT;//出差原因
                    //            Record.EVECTIONRECORDCATEGORY = "";//出差类型
                    //            Record.OWNERCOMPANYID = travel.OWNERCOMPANYID;
                    //            Record.OWNERDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    //            Record.OWNERPOSTID = travel.OWNERPOSTID;
                    //            Record.OWNERID = travel.OWNERID;
                    //            Record.CREATECOMPANYID = travel.OWNERCOMPANYID;
                    //            Record.CREATEDATE = System.DateTime.Now;
                    //            Record.CREATEDEPARTMENTID = travel.OWNERDEPARTMENTID;
                    //            Record.CREATEPOSTID = travel.OWNERPOSTID;
                    //            Record.CREATEUSERID = travel.OWNERID;
                    //            Record.REMARK = "出差报销自动产生";

                    //            Record.SUBSIDYTYPE = "1";//出差类型
                    //            Record.SUBSIDYVALUE = travel.THETOTALCOST;//补助金额
                    //            /// Modified by 罗捷
                    //            /// 作用：传来的item.BUSINESSDAYS数据为“17天”，导致Convert.ToDecimal方法异常
                    //            item.BUSINESSDAYS = CheckBusinessday(item.BUSINESSDAYS);
                    //            Record.TOTALDAYS = string.IsNullOrEmpty(item.BUSINESSDAYS) ? 0 : Convert.ToDecimal(item.BUSINESSDAYS);//
                    //            Record.T_HR_EMPLOYEEEVECTIONREPORT = null;//
                    //            Record.UPDATEDATE = DateTime.Now;
                    //            Record.UPDATEUSERID = travel.OWNERID;
                    //            ListRecord.Add(Record);
                    //        }
                    //    }
                    //    #endregion


                    //}
#endregion

                }
                try
                {
                    if (ListRecord.Count() > 0)
                    {
                        SMT.SaaS.OA.BLL.AttendanceWS.AttendanceServiceClient Client
                            = new AttendanceWS.AttendanceServiceClient();                       
                        Client.AddEmployeeEvectionRdList(ListRecord.ToArray());

                        StrMessage = travel.CLAIMSWERENAME +" 的出差报销同步考勤数据成功，出差报销开始时间："
                            + ListRecord[0].STARTDATE.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            + " 结束时间:" + ListRecord[0].ENDDATE.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        SMT.Foundation.Log.Tracer.Debug(StrMessage);
                    }
                }
                catch (Exception ex)
                {
                    StrMessage = travel.CLAIMSWERENAME +" 的出差报销同步考勤数据出错:" + ListRecord.Count() + ex.Message.ToString();
                    SMT.Foundation.Log.Tracer.Debug(StrMessage);
                }
            }
            catch (Exception ex)
            {
                StrMessage = travel.CLAIMSWERENAME+" 的出差报销生成考勤数据出错" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(StrMessage);
            }
        }

        /// <summary>
        ///     Modified by 罗捷
        ///     BUSINESSDAY可能含有天字，在这里切割掉
        /// </summary>
        /// <param name="BUSINESSDAYS"></param>
        private string CheckBusinessday(string BUSINESSDAYS)
        {
            if (!string.IsNullOrEmpty(BUSINESSDAYS))
                BUSINESSDAYS = BUSINESSDAYS.Split('天')[0];
            return BUSINESSDAYS;
        }

        #endregion

        #region 查询报销明细
        public List<T_OA_REIMBURSEMENTDETAIL> GetTravelReimbursementDetail(string detailId)
        {
            trdal = new TravelReimbursementDAL();
            var entity = (from p in dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>()
                          join b in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Include("T_OA_BUSINESSTRIP") on p.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID equals b.TRAVELREIMBURSEMENTID
                          where p.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID == detailId
                          orderby p.STARTDATE
                          select p).ToList();
            return entity;
        }
        #endregion

        #region 删除报销
        /// <summary>
        /// 删除报销
        /// </summary>
        /// <param name="TravelReimbursementID">报销ID</param>
        /// <param name="FBControl">报销费用</param>
        /// <returns></returns>
        public bool DeleteTravelReimbursement(string[] TravelReimbursementID, ref bool FBControl)
        {
            try
            {
                trdal = new TravelReimbursementDAL();
                var entity = from ent in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().ToList()
                             where TravelReimbursementID.Contains(ent.TRAVELREIMBURSEMENTID)
                             select ent;

                if (entity.Count() > 0)
                {
                    foreach (var h in entity)
                    {
                        //删除T_OA_REIMBURSEMENTDETAIL
                        var ent = from p in dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>()
                                  where h.TRAVELREIMBURSEMENTID == p.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID
                                  select p;
                        foreach (var k in ent)
                        {
                            dal.DeleteFromContext(k);
                        }
                        //删除T_OA_TRAVELREIMBURSEMENT
                        dal.DeleteFromContext(h);
                        base.DeleteMyRecord(h);
                    }

                    FBServiceClient FBClient = new FBServiceClient();
                    string[] StrFBMessage = FBClient.RemoveExtensionOrder(TravelReimbursementID);
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
                else
                {
                    Tracer.Debug("出差报销TravelReimbursementBLL-DeleteTravelReimbursement:未找到未提交的报销单，仅能删除未提交单据");
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-DeleteTravelReimbursement" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return false;
        }
        #endregion

        #region 通过出差申请id获取出差报销id
        /// <summary>
        /// 删除报销
        /// </summary>
        /// <param name="TravelReimbursementID">报销ID</param>
        /// <param name="FBControl">报销费用</param>
        /// <returns></returns>
        public List<string> GetReimbursementIDsByBusinesstripId(string[] TravelmanagementID)
        {
            try
            {
                if (TravelmanagementID != null)
                {
                    var entity = from tm in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>()
                                 where TravelmanagementID.Contains(tm.T_OA_BUSINESSTRIP.BUSINESSTRIPID) && tm.CHECKSTATE=="0"
                                 select tm.TRAVELREIMBURSEMENTID;
                    if (entity.Count() > 0) return entity.ToList();
                }
                else
                {
                    Tracer.Debug("出差报销TravelReimbursementBLL-GetReimbursementIDsByBusinesstripId" + System.DateTime.Now.ToString() + " " + "TravelmanagementID为空");
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-GetReimbursementIDsByBusinesstripId" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return null;
        }
        #endregion

        #region 检查是否存在该合同报销
        /// <summary>
        /// 检查是否存在该报销内容
        /// </summary>
        /// <param name="ownerid">报销人</param>
        /// <param name="TravelReimbursementID">报销编号</param>
        /// <returns></returns>
        public bool IsExistContractTravelReimbursement(string ownerid, string TravelReimbursementID)
        {
            try
            {
                trdal = new TravelReimbursementDAL();
                bool IsExist = false;
                var q = from cnt in trdal.GetTable()
                        where cnt.CLAIMSWERE == ownerid && cnt.TRAVELREIMBURSEMENTID == TravelReimbursementID
                        orderby cnt.CREATEDATE
                        select cnt;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-IsExistContractTravelReimbursement" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 通过出差申请ID检测是否已存在自动生成出差报销
        /// </summary>
        /// <param name="businesstripId">出差申请ID</param>
        /// <returns>true为存在，false为不存在</returns>
        public bool CheckTravelReimbursementByBusinesstrip(string businesstripId)
        {
            bool IsExist = false;
            try
            { 
                using (trdal = new TravelReimbursementDAL())
                {
                    //此方法用于检测是否有自动生成的出差报销，因此去checkstate为0，即未提交的
                    var entity = from t in trdal.GetObjects()
                                 where t.T_OA_BUSINESSTRIP.BUSINESSTRIPID == businesstripId
                                 select t;
                    if (entity.Count() > 0)
                        IsExist = true;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-IsExistTravelReimbursement" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
            return IsExist;
        }

        /// <summary>
        /// 检查是否存在该报销内容
        /// add ljx 检查出差申请中是否有了出差报销2011
        /// </summary>
        /// <param name="ownerid">报销人</param>
        /// <param name="TravelReimbursementID">报销编号</param>
        /// <returns></returns>
        public bool IsExistTravelReimbursementBySportid(string ownerid, string sportid)
        {
            try
            {
                trdal = new TravelReimbursementDAL();
                bool IsExist = false;
                var q = from cnt in trdal.GetObjects<T_OA_TRAVELREIMBURSEMENT>().Include("T_OA_BUSINESSTRIP")
                        where cnt.CLAIMSWERE == ownerid && cnt.T_OA_BUSINESSTRIP.BUSINESSTRIPID == sportid
                        orderby cnt.CREATEDATE
                        select cnt;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-IsExistContractTravelReimbursement" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        #endregion

        #region

        public List<string> GetTravelReimbursementRoomNameInfos()
        {
            trdal = new TravelReimbursementDAL();
            var query = from p in trdal.GetTable()
                        orderby p.CREATEDATE descending
                        select p.CLAIMSWERE;

            return query.ToList<string>();
        }

        #endregion

        #region 获取报销信息
        /// <summary>
        /// 获取报销信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_TRAVELREIMBURSEMENT> GetTravelReimbursementRooms()
        {
            trdal = new TravelReimbursementDAL();
            var query = from p in trdal.GetTable()
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_TRAVELREIMBURSEMENT>();

        }
        #endregion

        #region 获取用户报销记录
        /// <summary>
        /// 获取用户报销记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<V_TravelReimbursement> GetTravelReimbursementInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                List<V_TravelReimbursement> listaa = new List<V_TravelReimbursement>();
                trdal = new TravelReimbursementDAL();
                var ents = from p in dal.GetObjects<T_OA_TRAVELREIMBURSEMENT>()
                           where p.OWNERNAME != null && p.CHECKSTATE != null
                           && p.OWNERCOMPANYID != null && p.OWNERDEPARTMENTID != null
                           select new V_TravelReimbursement
                           {
                               TravelReimbursement = p,
                               OWNERCOMPANYID = p.OWNERCOMPANYID,
                               //OWNERID = p.OWNERID,
                               OWNERPOSTID = p.OWNERPOSTID,
                               OWNERDEPARTMENTID = p.OWNERDEPARTMENTID,
                               CREATEUSERID = p.CREATEUSERID
                           };
                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.TravelReimbursement.TRAVELREIMBURSEMENTID equals l.FormID
                                select new V_TravelReimbursement
                                {
                                    TravelReimbursement = a.TravelReimbursement,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });

                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.TravelReimbursement.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    if (flowInfoList == null)
                    {
                        UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_TRAVELREIMBURSEMENT");
                    }
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.AsQueryable().OrderBy(sort);
                    ents = Utility.Pager<V_TravelReimbursement>(ents.AsQueryable(), pageIndex, pageSize, ref pageCount);

                    foreach (var ent in ents)
                    {
                        string StartCity = "";//出发城市
                        string EndCity = "";//目标城市
                        string StartTime = "";//开始时间
                        string EndTime = "";//结束时间
                        var detailinfos = from info in dal.GetObjects<T_OA_REIMBURSEMENTDETAIL>().Include("T_OA_TRAVELREIMBURSEMENT")
                                          where info.T_OA_TRAVELREIMBURSEMENT.TRAVELREIMBURSEMENTID == ent.TravelReimbursement.TRAVELREIMBURSEMENTID
                                          select info;
                        detailinfos = detailinfos.OrderBy(p => p.STARTDATE);
                        if (detailinfos.Count() > 1)//存在多条出差详情信息
                        {
                            StartCity = detailinfos.ToList().FirstOrDefault().DEPCITY.ToString();
                            EndCity = detailinfos.ToList().LastOrDefault().DESTCITY.ToString();
                            StartTime = detailinfos.ToList().FirstOrDefault().STARTDATE.ToString();
                            EndTime = detailinfos.ToList().LastOrDefault().ENDDATE.ToString();

                            if (StartCity == EndCity)//如果刚开始的出发城市和最后一条记录的目的城市  则取最后一条记录的出发城市
                            {
                                EndCity = detailinfos.ToList().LastOrDefault().DEPCITY.ToString();
                            }
                        }
                        if (detailinfos.Count() == 1)//只存在一条记录的情况
                        {
                            StartCity = detailinfos.ToList().FirstOrDefault().DEPCITY.ToString();
                            EndCity = detailinfos.ToList().LastOrDefault().DESTCITY.ToString();
                            StartTime = detailinfos.ToList().FirstOrDefault().STARTDATE.ToString();
                            EndTime = detailinfos.ToList().LastOrDefault().ENDDATE.ToString();
                        }
                        ent.Cicty = StartCity;
                        ent.Cictys = EndCity;
                        ent.Startdate = StartTime;
                        ent.Endtime = EndTime;
                        listaa.Add(ent);
                    }
                    ents = listaa.AsQueryable();
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("出差报销TravelReimbursementBLL-GetTravelReimbursementInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion
    }
}
