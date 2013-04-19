using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using SMT.Foundation.Log; 



namespace SMT.SaaS.OA.BLL
{


    #region 会议管理

    public class MeetingManagementBll:BaseBll<T_OA_MEETINGINFO>
    {

        
        

         //添加会议室内容
        /// <summary>
        /// 添加会议信息
        /// </summary>
        /// <param name="MeetingInfoT">会议信息实体</param>
        /// <param name="StaffObj">参会员工</param>
        /// <param name="ContentObj">会议内容</param>
        /// <param name="MessageObj">会议消息</param>
        /// <returns></returns>
        public string AddMeetingInfo(T_OA_MEETINGINFO MeetingInfoT, List<T_OA_MEETINGSTAFF> StaffObj, List<T_OA_MEETINGCONTENT> ContentObj, T_OA_MEETINGMESSAGE MessageObj)
        {
            try
            {
                BeginTransaction();
                //添加与会人员
                MeetingInfoT.CREATEDATE = DateTime.Now;
                MessageObj.CREATEDATE = DateTime.Now;
                StaffObj.ForEach(item =>
                    {
                        MeetingInfoT.T_OA_MEETINGSTAFF.Add(item);
                        Utility.RefreshEntity(item);
                    });
                //添加会议通知
                MessageObj.T_OA_MEETINGINFO = MeetingInfoT;
                
                Utility.RefreshEntity(MessageObj);

                this.Add(MeetingInfoT);
                //添加会议内容
                ContentObj.ForEach(item =>
                    {
                        //DataContext.AddObject(typeof(T_OA_MEETINGCONTENT).Name, item);
                        //base.Add(item);
                        dal.Add(item);

                    });
                //DataContext.SaveChanges();
                CommitTransaction();
                return "";
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Tracer.Debug("会议信息MeetingManagementBll-AddMeetingInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "ERROR";
            }
        }
        

        /// <summary>
        /// 事务开始
        /// </summary>
        public void BeginTransaction()
        {
            dal.BeginTransaction();
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            dal.CommitTransaction();
        }
        /// <summary>
        /// 事务回滚
        /// </summary>
        public void RollbackTransaction()
        {
            dal.RollbackTransaction();
        }
        /// <summary>
        /// 删除会议信息  这个需要完善  [删除会议信息时，一并将会议通知、参会人员、会议内容一起删除，否则引起数据冗余] 2011-7-25
        /// </summary>
        /// <param name="MeetingInfoID">会议信息ID</param>
        /// <returns>成功为真  错误为假</returns>
        public bool DeleteMeetingInfo(string MeetingInfoID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects<T_OA_MEETINGINFO>()
                               where ent.MEETINGINFOID == MeetingInfoID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-DeleteMeetingInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }
        /// <summary>
        /// 更新会议信息
        /// </summary>
        /// <param name="MeetingInfoT"></param>
        /// <param name="StaffObj"></param>
        /// <param name="ContentObj"></param>
        /// <param name="MessageObj"></param>
        /// <returns></returns>
        public int UpdateMeetingInfo(T_OA_MEETINGINFO MeetingInfoT, List<T_OA_MEETINGSTAFF> StaffObj, List<T_OA_MEETINGCONTENT> ContentObj, T_OA_MEETINGMESSAGE MessageObj)
        {
            try
            {
                BeginTransaction();
                int result = 0;
                string OldCheckState="0";
                MeetingInfoT.UPDATEDATE = DateTime.Now;
                MeetingStaffManagementBll staffbll = new MeetingStaffManagementBll();
                List<string> employeid = new List<string>();
                var users = from ent in dal.GetObjects()
                            where ent.MEETINGINFOID == MeetingInfoT.MEETINGINFOID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    OldCheckState = user.CHECKSTATE;
                    if (MeetingInfoT.EntityKey == null)
                    {
                        MeetingInfoT.EntityKey = user.EntityKey;
                    }

                    //T_OA_MEETINGINFO entity = dal.GetObjectByEntityKey(MeetingInfoT.EntityKey) as T_OA_MEETINGINFO;
                    //entity.T_OA_MEETINGTYPE = dal.GetObjectByEntityKey(MeetingInfoT.T_OA_MEETINGTYPE.EntityKey) as T_OA_MEETINGTYPE;
                    //entity.T_OA_MEETINGROOM = dal.GetObjectByEntityKey(MeetingInfoT.T_OA_MEETINGROOM.EntityKey) as T_OA_MEETINGROOM;
                    //Utility.CloneEntity(MeetingInfoT, user);
                    Utility.RefreshEntity(MeetingInfoT);
                    int i = Update(MeetingInfoT);
                    if (i > 0)
                    {
                        if (OldCheckState == "0" && MeetingInfoT.CHECKSTATE !="0")
                            return i;//如果不是第一次提交则不修改其它数据  只对未提交的数据进行修改
                    }
                    else
                    {
                        result = -1;
                    }


                }
                List<T_OA_MEETINGSTAFF> liststaff = new List<T_OA_MEETINGSTAFF>();
                liststaff = staffbll.GetMeetingStaffInfosByMeetingInfoID(MeetingInfoT.MEETINGINFOID);//获取与会人员的信息

                StaffObj.ForEach(item =>
                {
                    employeid.Add(item.MEETINGUSERID);

                });
                //先删除
                DeleteMeetingContentByMeetingInfoID(MeetingInfoT.MEETINGINFOID);
                liststaff.ForEach(item =>
                {
                    //var q = from a in 
                    int bb = employeid.IndexOf(item.MEETINGUSERID);

                    T_OA_MEETINGSTAFF singlestaff = new T_OA_MEETINGSTAFF();
                    singlestaff = staffbll.GetMeetingStaffByMeetingInfoAndUserIDSecond(item.MEETINGINFOID, item.MEETINGUSERID);
                    bool istrue = staffbll.DeleteMeetingStaffInfo(singlestaff.MEETINGSTAFFID);
                    if (!istrue)
                    {
                        result = -1;
                    }


                });


                StaffObj.ForEach(itemStaff =>
                {


                    if (itemStaff.T_OA_MEETINGINFO != null)
                    {
                        if (itemStaff.T_OA_MEETINGINFO.EntityKey == null)
                            itemStaff.T_OA_MEETINGINFO.EntityKey = users.FirstOrDefault().EntityKey;
                    }
                    Utility.RefreshEntity(itemStaff);
                    dal.AddToContext(itemStaff);


                });
                dal.SaveContextChanges();
                if (result != 0)
                    return -1;

                //MessageObj.T_OA_MEETINGINFO = MeetingInfoT;

                if (MessageObj.T_OA_MEETINGINFO != null)
                    if (MessageObj.T_OA_MEETINGINFO.EntityKey == null)
                        MessageObj.T_OA_MEETINGINFO.EntityKey = users.FirstOrDefault().EntityKey;
                Utility.RefreshEntity(MessageObj);
                dal.Add(MessageObj);

                ContentObj.ForEach(item =>
                {
                    Utility.RefreshEntity(item);
                    //DataContext.ApplyPropertyChanges(typeof(T_OA_MEETINGCONTENT).Name, item);
                    //dal.Add(item);
                    dal.AddToContext(item);
                });
                dal.SaveContextChanges();
                //string Record = SaveMyRecord(MeetingInfoT);
                //if (!string.IsNullOrEmpty(Record))
                //{
                //    result = -1;
                //}
                if (MeetingInfoT.CHECKSTATE == "2") //审核通过 判断会议是选择了什么类型 是否需要定时发起
                {
                    if (MeetingInfoT.T_OA_MEETINGTYPE.ISAUTO == "1")
                    {
                        List<object> objArds = new List<object>();
                        T_OA_LICENSEUSER record = new T_OA_LICENSEUSER();
                        objArds.Add(record.LICENSEUSERID);
                        objArds.Add("OA");
                        objArds.Add("hireAppObj.HIREAPPID");
                        objArds.Add(record.LICENSEUSERID);
                        objArds.Add(DateTime.Now.AddDays((int)MeetingInfoT.T_OA_MEETINGTYPE.REMINDDAY).ToString("yyyy/MM/d"));
                        objArds.Add(DateTime.Now.ToString("HH:mm"));
                        objArds.Add("Day");
                        objArds.Add("");
                        //objArds.Add(ent.LICENSENAME + "证照归还时间：" + entity.ENDDATE.ToString("yyyy-MM-dd") + ",归还");
                        objArds.Add("");
                        objArds.Add("");
                        objArds.Add(Utility.strEngineFuncWSSite);
                        objArds.Add("EventTriggerProcess");
                        objArds.Add("<Para FuncName=\"UpdateEmployeeWorkAgeByID\" Name=\"LICENSEUSERID\" Value=\"" + record.LICENSEUSERID + "\"></Para>");
                        objArds.Add("Г");
                        objArds.Add("CustomBinding");

                        Utility.SendEngineEventTriggerData(objArds);
                    }
                }
                //DataContext.SaveChanges();
                //Utility.RefreshEntity(MeetingInfoT);
                if (result == 0)
                {

                    CommitTransaction();


                }
                else
                {

                    RollbackTransaction();
                }
                return result;
                //dal.Update(MeetingInfoT);
                //CommitTransaction();
                //return 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Tracer.Debug("会议信息MeetingManagementBll-UpdateMeetingInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// 更新会议状态并添加取消会议通知
        /// </summary>
        /// <param name="MeetingInfoT"></param>
        /// <param name="StaffObj"></param>
        /// <param name="ContentObj"></param>
        /// <param name="MessageObj"></param>
        /// <returns></returns>
        public int UpdateMeetingNoticeInfo(T_OA_MEETINGINFO MeetingInfoT, T_OA_MEETINGMESSAGE MessageObj)
        {
            try
            {
                BeginTransaction();
                //DataContext.ApplyPropertyChanges(MeetingInfoT.GetType().Name, MeetingInfoT);
                Utility.RefreshEntity(MeetingInfoT);
                this.Update(MeetingInfoT);
                MessageObj.T_OA_MEETINGINFO = MeetingInfoT;
                Utility.RefreshEntity(MessageObj);
                
                dal.Add(MessageObj);
                //DataContext.SaveChanges();
                CommitTransaction();
                return 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Tracer.Debug("会议信息MeetingManagementBll-UpdateMeetingNoticeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
            }
        }
        /// <summary>
        /// 修改会议信息
        /// </summary>
        /// <param name="MeetingInfoT"></param>
        /// <returns></returns>
        public int UpdateMeetingInfo(T_OA_MEETINGINFO MeetingInfoT)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.MEETINGINFOID == MeetingInfoT.MEETINGINFOID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    MeetingInfoT.EntityKey = user.EntityKey;
                    Utility.CloneEntity(MeetingInfoT, user);
                    int i = dal.Update(user);
                    if (i > 0)
                    {
                        return i;
                    }
                    else
                    {
                        return -1;
                        //result = -1;
                    }
                    

                }

                //T_OA_MEETINGINFO tmpobj = DataContext.GetObjectByKey(MeetingInfoT.EntityKey) as T_OA_MEETINGINFO;
                //tmpobj.T_OA_MEETINGROOM = DataContext.GetObjectByKey(MeetingInfoT.T_OA_MEETINGROOM.EntityKey) as T_OA_MEETINGROOM;
                //tmpobj.T_OA_MEETINGTYPE = DataContext.GetObjectByKey(MeetingInfoT.T_OA_MEETINGTYPE.EntityKey) as T_OA_MEETINGTYPE;
                //DataContext.ApplyPropertyChanges(MeetingInfoT.EntityKey.EntitySetName, MeetingInfoT);
                //int i = DataContext.SaveChanges();
                //if (i > 0)
                //{
                //    return i;
                //}
                //else
                //{
                //    return -1;
                //}
                return 0;
                
                //return 0;
            }
            catch(Exception ex) 
            {
                Tracer.Debug("会议信息MeetingManagementBll-UpdateMeetingNoticeInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                
            }
        }



        public T_OA_MEETINGINFO GetMeetingInfoById(string MeetingId)
        {
            try
            {
                var q = from ent in dal.GetObjects().Include("T_OA_MEETINGROOM").Include("T_OA_MEETINGTYPE")
                        where ent.MEETINGINFOID == MeetingId
                        
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GetMeetingInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }
        //批量删除密级信息
        public bool BatchDeleteMeetingInfo(string[] ArrMeetingInfoIDs)
        {
            try
            {

                foreach (string id in ArrMeetingInfoIDs)
                {
                    //删除通知
                    var message = from m in dal.GetObjects<T_OA_MEETINGMESSAGE>().Include("T_OA_MEETINGINFO")
                                  where m.T_OA_MEETINGINFO.MEETINGINFOID == id
                                  select m;
                    if (message.Count() > 0)
                    {
                        foreach (var m in message)
                        {
                            dal.DeleteFromContext(m);                            
                        }
                    }
                    //删除与会人员
                    var meetingstaff = from staff in dal.GetObjects<T_OA_MEETINGSTAFF>().Include("T_OA_MEETINGINFO")
                                       where staff.T_OA_MEETINGINFO.MEETINGINFOID == id
                                       select staff;
                    if (meetingstaff.Count() > 0)
                    {
                        foreach (var n in meetingstaff)
                        {
                            dal.DeleteFromContext(n);
                        }
                    }

                    //删除会议内容
                    var content = from c in dal.GetObjects<T_OA_MEETINGCONTENT>()
                                  where c.MEETINGINFOID == id
                                  select c;
                    if (content.Count() > 0)
                    {
                        foreach (var con in content)
                        {
                            dal.DeleteFromContext(con);
                        }
                    }
                    //DataContext.SaveChanges();
                    var ents = from e in dal.GetObjects()
                               where e.MEETINGINFOID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }

                int i = dal.SaveContextChanges();
                

                return i > 0 ? true : false;
                
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-BatchDeleteMeetingInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        

        /// <summary>
        /// 获取所有的会议信息
        /// </summary>
        /// 
        /// <returns></returns>       
        public IQueryable<V_MeetingInfo> GetMeetingInfos(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {


                //SMT_OA_EFModel.SMT_OA_EFModelContext context = dal.lbc.GetDataContext() as SMT_OA_EFModel.SMT_OA_EFModelContext;
                
                //context.T_OA_MEETINGINFO.MergeOption = System.Data.Objects.MergeOption.NoTracking;
                    
                var ents = (from a in dal.GetObjects()
                            join b in dal.GetObjects<T_OA_MEETINGTYPE>() on a.T_OA_MEETINGTYPE.MEETINGTYPE equals b.MEETINGTYPE
                            join c in dal.GetObjects<T_OA_MEETINGROOM>() on a.T_OA_MEETINGROOM.MEETINGROOMNAME equals c.MEETINGROOMNAME
                                
                                
                            select new V_MeetingInfo 
                            { 
                                meetinginfo = a,meetingtype=b,meetingroom =c,
                                OWNERCOMPANYID = a.OWNERCOMPANYID,
                                OWNERID = a.OWNERID,
                                OWNERPOSTID = a.OWNERPOSTID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                CREATEUSERID = a.CREATEUSERID
                            });
                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {
                            
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.meetinginfo.MEETINGINFOID equals l.FormID
                                select new V_MeetingInfo
                                { 
                                    meetinginfo = a.meetinginfo,meetingtype=a.meetingtype,
                                    meetingroom=a.meetingroom, flowApp = l ,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });
                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.meetinginfo.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_MEETINGINFO");
                    if (queryParas.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(filterString))
                        {
                            ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                        }
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_MeetingInfo>(ents, pageIndex, pageSize, ref pageCount);
                    return ents;
                }
                return null;
                    
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GetMeetingInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        /// <summary>
        /// 获取1条会议通知信息 通过webpart界面中传递1个会议通知参数 
        /// </summary>
        /// 
        /// <returns></returns>       
        public V_MeetingInfo GetMeetingNoticeByNoticeID(string NoticeID)
        {
            try
            {


                
                var ents = (
                    from m in dal.GetObjects<T_OA_MEETINGMESSAGE>()
                    join n in
                        (
                            from a in dal.GetObjects()
                            join b in dal.GetObjects<T_OA_MEETINGTYPE>() on a.T_OA_MEETINGTYPE.MEETINGTYPE equals b.MEETINGTYPE
                            join c in dal.GetObjects<T_OA_MEETINGROOM>() on a.T_OA_MEETINGROOM.MEETINGROOMNAME equals c.MEETINGROOMNAME
                            select new { a, b, c }
                        )
                    on m.T_OA_MEETINGINFO.MEETINGINFOID equals n.a.MEETINGINFOID
                    where m.MEETINGMESSAGEID == NoticeID
                    select new V_MeetingInfo { meetingmessage=m,meetinginfo = n.a, meetingtype = n.b, meetingroom = n.c });

                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
                return null;

                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GetMeetingNoticeByNoticeID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        ///// <summary>int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo
        ///// 获取某一员工所参加的会议
        ///// </summary>
        ///// <param name="userID">用户ID</param>
        ///// <returns></returns>
        //public IQueryable<V_MyMeetingInfosManagement> GeMeetingMembertMeetingInfos(string userID)
        //{
        //    try
        //    {

        //        SMT_OA_EFModel.SMT_OA_EFModelContext context = dal.lbc.GetDataContext() as SMT_OA_EFModel.SMT_OA_EFModelContext;

        //        if (context != null)
        //        {
        //            context.T_OA_MEETINGINFO.MergeOption = System.Data.Objects.MergeOption.NoTracking;
        //            context.T_OA_MEETINGCONTENT.MergeOption = System.Data.Objects.MergeOption.NoTracking;
        //            context.T_OA_MEETINGSTAFF.MergeOption = System.Data.Objects.MergeOption.NoTracking;
        //            var ents = from a in context.T_OA_MEETINGINFO.Include("T_OA_MEETINGTYPE").Include("T_OA_MEETINGROOM")
        //                       join l in context.T_OA_MEETINGCONTENT on a.MEETINGINFOID equals l.MEETINGINFOID
        //                       join s in context.T_OA_MEETINGSTAFF on a.MEETINGINFOID equals s.MEETINGINFOID
        //                       join m in context.T_OA_MEETINGTYPE on a.T_OA_MEETINGTYPE.MEETINGTYPE equals m.MEETINGTYPE
        //                       join n in context.T_OA_MEETINGROOM on a.T_OA_MEETINGROOM.MEETINGROOMNAME equals n.MEETINGROOMNAME
        //                       where l.MEETINGUSERID == userID && s.MEETINGUSERID == userID
        //                       orderby a.CREATEDATE descending
        //                       select new V_MyMeetingInfosManagement
        //                       { 
        //                           OAMeetingInfoT = a, OAMeetingContentT = l, 
        //                           OAMeetingStaffT = s, meetingtype = m, meetingroom = n
        //                       };


        //            if (ents.Count() > 0)
        //            {
        //                return ents;
        //            }
        //        }


        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //        throw(ex);
        //    }
        //}

        /// <summary>int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string checkState, LoginUserInfo loginUserInfo
        /// 获取某一员工所参加的会议 增加了翻页  查询功能 2010-8-3
        /// </summary>
        /// <param name="userID">用户ID</param>        
        /// <returns></returns>
        public IQueryable<V_MyMeetingInfosManagement> GeMeetingMembertMeetingInfos(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            try
            {

                
                var ents = from a in dal.GetObjects().Include("T_OA_MEETINGTYPE").Include("T_OA_MEETINGROOM")
                            join l in dal.GetObjects<T_OA_MEETINGCONTENT>() on a.MEETINGINFOID equals l.MEETINGINFOID
                           join s in dal.GetObjects<T_OA_MEETINGSTAFF>() on a.MEETINGINFOID equals s.MEETINGINFOID
                            join m in dal.GetObjects<T_OA_MEETINGTYPE>() on a.T_OA_MEETINGTYPE.MEETINGTYPE equals m.MEETINGTYPE
                            join n in dal.GetObjects<T_OA_MEETINGROOM>() on a.T_OA_MEETINGROOM.MEETINGROOMNAME equals n.MEETINGROOMNAME
                            where l.MEETINGUSERID == userID && s.MEETINGUSERID == userID && a.CHECKSTATE=="2"
                            orderby a.CREATEDATE descending
                            select new V_MyMeetingInfosManagement
                            {
                                OAMeetingInfoT = a,
                                OAMeetingContentT = l,
                                OAMeetingStaffT = s,
                                meetingtype = m,
                                meetingroom = n
                            };

                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                    
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<V_MyMeetingInfosManagement>(ents, pageIndex, pageSize, ref pageCount);
                if (ents.Count() > 0)
                {
                    return ents;
                }
                return null;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GeMeetingMembertMeetingInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }



        /// <summary>
        /// 获取某一员工主持的会议
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public IQueryable<V_MyMeetingInfosManagement> GeMemberEmceeMeetingInfos(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            try
            {

                //SMT_OA_EFModel.SMT_OA_EFModelContext context = dal.lbc.GetDataContext() as SMT_OA_EFModel.SMT_OA_EFModelContext;

                
                var ents = from a in dal.GetObjects().Include("T_OA_MEETINGTYPE").Include("T_OA_MEETINGROOM")
                           join m in dal.GetObjects<T_OA_MEETINGTYPE>() on a.T_OA_MEETINGTYPE.MEETINGTYPE equals m.MEETINGTYPE
                           join n in dal.GetObjects<T_OA_MEETINGROOM>() on a.T_OA_MEETINGROOM.MEETINGROOMNAME equals n.MEETINGROOMNAME
                            where a.HOSTID == userID && a.CHECKSTATE == "2"
                            orderby a.CREATEDATE descending
                            select new V_MyMeetingInfosManagement { OAMeetingInfoT = a, meetingtype = m, meetingroom = n };
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);

                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<V_MyMeetingInfosManagement>(ents, pageIndex, pageSize, ref pageCount);
                    
                if (ents.Count() > 0)
                {
                    return ents;
                }
                


                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GeMemberEmceeMeetingInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }


        //判断是否有存在的会议类型
        //TypeName 会议类型
        public bool GetMeetingInfoByAdd(string StrTitle,string StrMeetingType,string StrRoom,string StrCreatUser,DateTime stardt,DateTime enddt)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_MEETINGINFO>()
                        //where ent.MEETINGTYPE == StrMeetingType && ent.MEETINGTITLE == StrTitle && ent.MEETINGROOMNAME == StrRoom && ent.CREATEUSERID == StrCreatUser && ent.STARTTIME == stardt && ent.ENDTIME == enddt
                        where ent.T_OA_MEETINGTYPE.MEETINGTYPEID == StrMeetingType && ent.MEETINGTITLE == StrTitle && ent.T_OA_MEETINGROOM.MEETINGROOMID == StrRoom && ent.CREATEUSERID == StrCreatUser && ent.STARTTIME == stardt && ent.ENDTIME == enddt

                        select ent;
                if (q.Count() > 0)
                {
                    //return q.FirstOrDefault();
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GetMeetingInfoByAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }

            //return null;
        }



        //获取查询的会议信息
        public IQueryable<T_OA_MEETINGINFO> GetMeetingInfosListByTitleTimeSearch(string StrTitle,string StrType,string StrDepartment,DateTime DtStart,DateTime DtEnd,string StrContent,string StrCheckState)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_MEETINGINFO>()

                        select ent;

                if (!string.IsNullOrEmpty(StrCheckState))
                {
                    q = q.Where(s => s.CHECKSTATE == StrCheckState);
                }
                if (!string.IsNullOrEmpty(StrTitle))
                {
                    q = q.Where(s => StrTitle.Contains(s.MEETINGTITLE));
                }
                //if (!string.IsNullOrEmpty(StrContent))
                //{
                //    q = q.Where(s => StrContent.Contains(s.CONTENT));
                //}
                if (!string.IsNullOrEmpty(StrType))
                {
                    //q = q.Where(s => StrType.Contains(s.MEETINGTYPE));
                    q = q.Where(s => StrType.Contains(s.T_OA_MEETINGTYPE.MEETINGTYPEID));

                }
                if (!string.IsNullOrEmpty(StrDepartment))
                {
                    q = q.Where(s => StrDepartment.Contains(s.DEPARTNAME));
                }
                //int aa = DtStart.CompareTo(DtEnd);
                if (DtStart != null && DtEnd != null && (DtStart.CompareTo(System.Convert.ToDateTime("0001-1-1 0:00:00")) > 0))
                {
                    q = q.Where(s => DtStart < s.STARTTIME);
                    q = q.Where(s => DtEnd > s.ENDTIME);
                }
                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GetMeetingInfosListByTitleTimeSearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }



        public int UpdateMeetingMessageInfo(T_OA_MEETINGMESSAGE MeetingInfoT)
        {
            try
            {

                T_OA_MEETINGMESSAGE tmpobj = dal.GetObjectByEntityKey(MeetingInfoT.EntityKey) as T_OA_MEETINGMESSAGE;
                tmpobj.T_OA_MEETINGINFO = dal.GetObjectByEntityKey(MeetingInfoT.T_OA_MEETINGINFO.EntityKey) as T_OA_MEETINGINFO;
                
                //DataContext.ApplyPropertyChanges(MeetingInfoT.EntityKey.EntitySetName, MeetingInfoT);
                //dal.get
                int i = dal.Update(MeetingInfoT);
                if (i > 0)
                {
                    return i;
                }
                else
                {
                    return -1;
                }

                
                
            }
            catch(Exception ex) 
            {
                Tracer.Debug("会议信息MeetingManagementBll-UpdateMeetingMessageInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                
            }
        }
        /// <summary>
        /// 获取会议信息的消息
        /// </summary>
        /// <param name="MessageID"></param>
        /// <returns></returns>
        public T_OA_MEETINGMESSAGE GetMeetingMessageByID(string MessageID)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_MEETINGMESSAGE>()
                        //where ent.MEETINGMESSAGEID == MessageID
                        where ent.T_OA_MEETINGINFO.MEETINGINFOID == MessageID
                        orderby ent.CREATEDATE
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-GetMeetingMessageByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        /// <summary>
        /// 删除会议内容信息
        /// </summary>
        /// <param name="MeetingInfoID">会议ID</param>
        /// <returns></returns>
        public bool DeleteMeetingContentByMeetingInfoID(string MeetingInfoID)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_MEETINGCONTENT>()                        
                        where ent.MEETINGINFOID == MeetingInfoID                        
                        select ent;
                if (q.Count() > 0)
                {
                    foreach (var a in q)
                    {
                        Utility.RefreshEntity(a);
                        dal.DeleteFromContext(a);
                    }
                    int i = dal.SaveContextChanges();
                    return i > 0 ? true : false;
                }
                return true;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议信息MeetingManagementBll-DeleteMeetingContentByMeetingInfoID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
            
        }



    }


    #endregion

    #region 会议室
    public class MeetingRoomBll : BaseBll<T_OA_MEETINGROOM>
    {
        
        
        //添加会议室内容
        /// <summary>
        /// 添加会议室信息
        /// </summary>
        /// <param name="RoomInfo"></param>
        /// <returns></returns>
        public string AddRoomInfo(T_OA_MEETINGROOM RoomInfo)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.MEETINGROOMNAME == RoomInfo.MEETINGROOMNAME);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！
                    //throw new Exception("Repetition");
                }
                else
                {

                    int i = dal.Add(RoomInfo);


                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;

                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-AddRoomInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "ERROR";
            }

        }


        /// <summary>
        /// 根据会议室ID删除会议室信息
        /// </summary>
        /// <param name="RoomID"></param>
        /// <returns></returns>
        public bool DeleteRoomInfo(string RoomID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.MEETINGROOMID == RoomID
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    dal.Delete(entity);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-DeleteRoomInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }
        /// <summary>
        /// 修改会议室信息
        /// </summary>
        /// <param name="RoomInfo"></param>
        public void UpdateRoomInfo(T_OA_MEETINGROOM RoomInfo)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.MEETINGROOMID == RoomInfo.MEETINGROOMID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    RoomInfo.EntityKey = user.EntityKey;
                    Utility.CloneEntity(RoomInfo, user);
                    dal.Update(user);
                }


                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-UpdateRoomInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return;
            }
        }

        /// <summary>
        /// 获取会议室信息
        /// </summary>
        /// <param name="RoomID"></param>
        /// <returns></returns>
        public IQueryable<T_OA_MEETINGROOM> GetMeetingRoomListByID(string RoomID)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.CREATEUSERID == RoomID
                        select ent;


                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRoomListByID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }


        //判断是否有存在的会议室名
        //RoomName 会议室名  CompanyID公司ID
        /// <summary>
        /// 根据公司ID 会议室名是否有会议室
        /// </summary>
        /// <param name="RoomName">会议室</param>
        /// <param name="CompanyID">公司ID</param>
        /// <returns></returns>
        public bool GetMeetingRoomByRoomName(string RoomName,string CompanyID)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGROOMNAME == RoomName && ent.COMPANYID == CompanyID
                        orderby ent.MEETINGROOMID
                        select ent;
                if (q.Count() > 0)
                {

                    IsExist = true;
                }

                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRoomByRoomName" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }
        /// <summary>
        /// 获取一条会议室信息
        /// </summary>
        /// <param name="RoomId">会议室ID</param>
        /// <returns></returns>
        public T_OA_MEETINGROOM GetMeetingRoomById(string RoomId)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGROOMID == RoomId
                        orderby ent.MEETINGROOMID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRoomById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        //获取所有的会议室信息
        /// <summary>
        /// 获取所有会议室信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_MEETINGROOM> GetMeetingRooms()
        {
            try
            {
                var query = from p in dal.GetObjects()
                            orderby p.CREATEDATE descending
                            select p;
                return query.ToList<T_OA_MEETINGROOM>();
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRooms" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }


        //获取查询的会议类型信息
        /// <summary>
        /// 根据条件过滤  会议室信息
        /// </summary>
        /// <param name="MeetingName"></param>
        /// <param name="strMemo"></param>
        /// <returns></returns>
        public IQueryable<T_OA_MEETINGROOM> GetMeetingRoomInfosListBySearch(string MeetingName,string strMemo)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        select ent;

                if (!string.IsNullOrEmpty(MeetingName))
                {
                    q = q.Where(s => MeetingName.Contains(s.MEETINGROOMNAME));
                }
                if (!string.IsNullOrEmpty(strMemo))
                {
                    q = q.Where(s => strMemo.Contains(s.REMARK));
                }
                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRoomInfosListBySearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        //获取公司的会议室信息  2010-5-17 
        /// <summary>
        /// 获取公司的会议室信息
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_OA_MEETINGROOM> GetMeetingRoomTreeInfosListByCompanyID(string userID)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        select ent;

                
                string filterString = "";
                List<object> queryParas = new List<object>();
                //queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAMEETINGROOM");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                
                q = q.OrderByDescending(s => s.CREATEDATE);
                if (q.Count() > 0)
                {
                    return q;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRoomTreeInfosListByCompanyID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }

        /// <summary>
        /// 获取会议室信息
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_OA_MEETINGROOM> GetMeetingRoomNameInfos()
        {
            try
            {
                var query = from p in dal.GetObjects()
                            orderby p.CREATEDATE descending
                            select p;

                return query;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRoomNameInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }


        //批量删除会议室信息
        /// <summary>
        /// 批量删除会议室信息
        /// </summary>
        /// <param name="ArrMeetingRoomIDs">会议室ID集合</param>
        /// <returns></returns>
        public string BatchDeleteMeetingRoomInfos(string[] ArrMeetingRoomIDs)
        {
            try
            {
                string StrReturn = "";
                foreach (string id in ArrMeetingRoomIDs)
                {


                    var ents = from e in dal.GetObjects()
                               where e.MEETINGROOMID == id
                               select e;

                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        var roomapp = from p in dal.GetObjects<T_OA_MEETINGROOMAPP>().Include("T_OA_MEETINGROOM")
                                      where p.T_OA_MEETINGROOM.MEETINGROOMNAME == ents.FirstOrDefault().MEETINGROOMNAME
                                      select p;
                        var meeting = from n in dal.GetObjects<T_OA_MEETINGINFO>().Include("T_OA_MEETINGROOM")
                                      where n.T_OA_MEETINGROOM.MEETINGROOMNAME == ents.FirstOrDefault().MEETINGROOMNAME
                                      select n;
                        if (roomapp.Count() > 0 || meeting.Count() > 0)
                        {
                            if (roomapp.Count() > 0 && meeting.Count() > 0)
                            {
                                StrReturn = "DELETEROOMAPPANDMEETINGINFO";
                            }
                            else
                            {
                                if (roomapp.Count() > 0)
                                {
                                    StrReturn = "PLEASEFIRSTDELETEROOMAPP";
                                }
                                else
                                {
                                    StrReturn = "PLEASEFIRSTDELETEMEETINGINFO";
                                }
                            }

                            break;
                        }
                        else
                        {
                            dal.DeleteFromContext(ent);
                        }

                        
                    }

                }
                int i = dal.SaveContextChanges();
                return i > 0 ? "" : "ERROR";

            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-BatchDeleteMeetingRoomInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "";
                
            }
        }

        /// <summary>
        /// 获取会议室信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_OA_MEETINGROOM> GetMeetingRoomInfosList(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            try
            {

                var ents = from ent in dal.GetObjects()

                           select ent;
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "OAMEETINGROOM");
                if (queryParas.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                }
                ents = ents.OrderBy(sort);
                ents = Utility.Pager<T_OA_MEETINGROOM>(ents, pageIndex, pageSize, ref pageCount);
                return ents;

            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室信息MeetingRoomBll-GetMeetingRoomInfosList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }
    }

    #endregion
       

}
