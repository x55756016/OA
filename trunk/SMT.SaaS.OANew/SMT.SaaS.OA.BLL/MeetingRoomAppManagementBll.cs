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
    //会议室申请信息
    public class MeetingRoomAppManagementBll : BaseBll<T_OA_MEETINGROOMAPP>
    {
        

        //添加会议室申请内容
        public string AddMeetingRoomAppInfo(T_OA_MEETINGROOMAPP RoomAppInfo)
        {
            try
            {
                string StrReturn = "";
                //var tempEnt = DataContext.T_OA_MEETINGROOMAPP.FirstOrDefault(s => s.T_OA_MEETINGROOM.MEETINGROOMNAME == RoomAppInfo.T_OA_MEETINGROOM.MEETINGROOMNAME
                //    && s.STARTTIME == RoomAppInfo.STARTTIME && s.ENDTIME == RoomAppInfo.ENDTIME && s.DEPARTNAME == RoomAppInfo.DEPARTNAME);

                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.T_OA_MEETINGROOM.MEETINGROOMNAME == RoomAppInfo.T_OA_MEETINGROOM.MEETINGROOMNAME
                && s.CHECKSTATE == "2" 
                && s.STARTTIME < RoomAppInfo.ENDTIME
                && s.ENDTIME > RoomAppInfo.STARTTIME);

                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                 
                }
                else
                {   
                    //Utility.RefreshEntity(RoomAppInfo);
                    T_OA_MEETINGROOM ent = (from e in dal.GetObjects<T_OA_MEETINGROOM>()
                                            where e.MEETINGROOMID == RoomAppInfo.T_OA_MEETINGROOM.MEETINGROOMID
                                            select e
                                                ).FirstOrDefault();
                    RoomAppInfo.T_OA_MEETINGROOM = ent;
                    RoomAppInfo.CREATEDATE = DateTime.Now;
                    
                    base.Add(RoomAppInfo);
                    int i = 1;
                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;

                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-AddMeetingRoomAppInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }

        }



        public bool DeleteMeetingRoomAppInfo(string MeetingRoomAppID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects<T_OA_MEETINGROOMAPP>()
                               where ent.MEETINGROOMAPPID == MeetingRoomAppID
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
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-DeleteMeetingRoomAppInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //批量删除会议类型信息
        public bool BatchDeleteMeetingRoomAppInfo(string[] ArrRoomAppIDs)
        {
            try
            {
                foreach (string id in ArrRoomAppIDs)
                {
                    var ents = from e in dal.GetObjects()
                               where e.MEETINGROOMAPPID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                    var RoomChanges = from room in dal.GetObjects<T_OA_MEETINGROOMTIMECHANGE>().Include("T_OA_MEETINGROOMAPP")
                                      where room.T_OA_MEETINGROOMAPP.MEETINGROOMAPPID == ent.MEETINGROOMAPPID
                                      select room;
                    if (ent != null)
                    {
                        //DataContext.DeleteObject(ent);
                        if (RoomChanges != null && RoomChanges.Count() >0)
                        {
                            foreach (var n in RoomChanges)
                            {
                                dal.DeleteFromContext(n);
                            }
                            dal.SaveContextChanges();//删除会议室申请时间变更
                        }
                            
                        dal.DeleteFromContext(ent);
                    }
                }
                return dal.SaveContextChanges() > 0 ? true : false;
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-BatchDeleteMeetingRoomAppInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }



        public string UpdateMeetingRoomAppInfo(T_OA_MEETINGROOMAPP MeetingRoomAppInfo)
        {
            string StrReturn = "";
            try
            {

                // 查看会议室是否被占用
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.T_OA_MEETINGROOM.MEETINGROOMNAME == MeetingRoomAppInfo.T_OA_MEETINGROOM.MEETINGROOMNAME
               && s.CHECKSTATE == "2"
               && s.STARTTIME < MeetingRoomAppInfo.ENDTIME
               && s.ENDTIME > MeetingRoomAppInfo.STARTTIME);

                if (tempEnt != null)
                {
                    StrReturn = "REPETITION";  //被占用  
                }
                else
                {
                    var users = from ent in dal.GetTable()
                                where ent.MEETINGROOMAPPID == MeetingRoomAppInfo.MEETINGROOMAPPID
                                select ent;

                    if (users.Count() > 0)
                    {
                        MeetingRoomAppInfo.UPDATEDATE = DateTime.Now;
                        
                        var user = users.FirstOrDefault();
                        MeetingRoomAppInfo.CREATEDATE = user.CREATEDATE;
                        if(MeetingRoomAppInfo.EntityKey == null)
                            MeetingRoomAppInfo.EntityKey = user.EntityKey;
                        //Utility.CloneEntity(MeetingRoomAppInfo, user);
                        
                        int i = Update(MeetingRoomAppInfo);
                        if (!(i > 0))
                        {
                            StrReturn = "SAVEFAILED"; // 修改失败
                        }
                    }
                }
                return StrReturn;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-UpdateMeetingRoomAppInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return "";
            }
        }


        


        //判断是否有相同的会议室申请记录
        //TypeName 会议类型
        public bool GetMeetingRoomAppInfoByRoomStartEnd(string RoomName,string Deaprtment,DateTime Start,DateTime End,string createUser)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_MEETINGROOMAPP>()
                        //where ent.MEETINGROOMNAME == RoomName && ent.DEPARTNAME == Deaprtment && ent.STARTTIME == Start && ent.ENDTIME == End && ent.CREATEUSERID == createUser
                        where ent.T_OA_MEETINGROOM.MEETINGROOMID == RoomName && ent.DEPARTNAME == Deaprtment && ent.STARTTIME == Start && ent.ENDTIME == End && ent.CREATEUSERID == createUser
                        orderby ent.MEETINGROOMAPPID
                        select ent;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-GetMeetingRoomAppInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //判断会议室是否被占用(是否有相同的会议室申请记录)
        public bool GetMeetingRoomJustUsingByRoomStartEnd(string RoomName,DateTime Start, DateTime End)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_MEETINGROOMAPP>()
                        //where ent.MEETINGROOMNAME == RoomName && ent.CHECKSTATE == "1" && ent.STARTTIME == Start && ent.ENDTIME == End 
                        where ent.T_OA_MEETINGROOM.MEETINGROOMID == RoomName && ent.CHECKSTATE == "1" && ent.STARTTIME == Start && ent.ENDTIME == End
                        orderby ent.MEETINGROOMAPPID
                        select ent;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-GetMeetingRoomJustUsingByRoomStartEnd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }

        //获取单条会议室申请记录信息
        public T_OA_MEETINGROOMAPP GetMeetingRoomAppInfoById(string RoomAppId)
        {
            try
            {
                var q = from ent in dal.GetObjects().Include("T_OA_MEETINGROOM")
                        where ent.MEETINGROOMAPPID == RoomAppId
                        
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-GetMeetingRoomAppInfoById" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
              
            }
        }

        //获取所有的会议类型信息
        public List<T_OA_MEETINGROOMAPP> GetMeetingRoomAppInfos(string StrCheckState)
        {
            try
            {
                var query = from p in dal.GetObjects<T_OA_MEETINGROOMAPP>()
                            where p.CHECKSTATE == StrCheckState
                            orderby p.CREATEDATE descending
                            select p;
                return query.ToList<T_OA_MEETINGROOMAPP>();
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-GetMeetingRoomAppInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
             
            }

        }
        //获取某一时间段的会议申请信息
        public IQueryable<T_OA_MEETINGROOMAPP> GetMeetingRoomAppInfos(DateTime start, DateTime end, string StrCheckState)
        {
            try
            {
                //取出审核通过且将开始的会议时间在已存在的会议时间之间 即：开始大于开始时间小于等于结束时间
                var query = from p in dal.GetObjects<T_OA_MEETINGROOMAPP>().Include("T_OA_MEETINGROOM")
                            where p.CHECKSTATE == StrCheckState && p.STARTTIME >= start && p.ENDTIME >= start
                            orderby p.CREATEDATE descending
                            select p;
                return query;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-GetMeetingRoomAppInfos" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
               
            }

        }
        public IQueryable<V_MeetingRoomApp> GetMeetingRoomAppInfosByFlow(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {


                //SMT_OA_EFModel.SMT_OA_EFModelContext context = dal.lbc.GetDataContext() as SMT_OA_EFModel.SMT_OA_EFModelContext;
                

                var ents = from a in dal.GetObjects()
                           join B in dal.GetObjects<T_OA_MEETINGROOM>() on a.T_OA_MEETINGROOM.MEETINGROOMNAME equals B.MEETINGROOMNAME
                            select new V_MeetingRoomApp
                            { 
                                roomapp = a, room = B,
                                OWNERCOMPANYID = a.OWNERCOMPANYID,
                                OWNERID = a.OWNERID,
                                OWNERPOSTID = a.OWNERPOSTID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                CREATEUSERID = a.CREATEUSERID
                            };
                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {

                        ents = (from a in ents.ToList().AsQueryable()
                                    
                                join l in flowInfoList on a.roomapp.MEETINGROOMAPPID equals l.FormID
                                select new V_MeetingRoomApp
                                { 
                                    roomapp = a.roomapp,room=a.room, flowApp = l ,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });
                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => s.roomapp.CHECKSTATE == checkState  );
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_MEETINGROOMAPP");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                        
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_MeetingRoomApp>(ents, pageIndex, pageSize, ref pageCount);
                    return ents;
                }

                
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-GetMeetingRoomAppInfosByFlow" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        

        //获取查询的会议类型信息
        public IQueryable<T_OA_MEETINGROOMAPP> GetMeetingRoomAppListBySearch(string MeetingRoom, DateTime DtStart, DateTime DtEnd, string department, string checkstate, string createUser)
        {

            try
            {

                var q = from ent in dal.GetObjects<T_OA_MEETINGROOMAPP>()
                        select ent;
                if (!string.IsNullOrEmpty(MeetingRoom))
                {
                    //q = q.Where(s => MeetingRoom.Contains(s.MEETINGROOMNAME));
                    q = q.Where(s => MeetingRoom.Contains(s.T_OA_MEETINGROOM.MEETINGROOMID));
                }
                if (!string.IsNullOrEmpty(department))
                {
                    q = q.Where(s => department.Contains(s.DEPARTNAME));
                }
                if (DtStart != null)
                {
                    q = q.Where(s => DtStart < s.STARTTIME);
                }
                if (DtEnd != null)
                {
                    q = q.Where(s => DtEnd > s.ENDTIME);
                }
                if (!string.IsNullOrEmpty(checkstate))
                {
                    q = q.Where(s => s.CHECKSTATE == checkstate);
                }
                if (!string.IsNullOrEmpty(createUser))
                {
                    q = q.Where(s => s.CREATEUSERID == createUser);
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
                Tracer.Debug("会议室申请信息MeetingRoomAppManagementBll-GetMeetingRoomAppListBySearch" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                
            }
        }


        
    }
}
