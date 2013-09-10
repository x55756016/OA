using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.BLL
{

    #region 会议室时间变更

    public class MeetingRoomTimeChangeManagementBll:BaseBll<T_OA_MEETINGROOMTIMECHANGE>
    {
        
        //添加会议室申请内容
        public string AddMeetingRoomTimeChangeInfo(T_OA_MEETINGROOMTIMECHANGE RoomTimeChangeInfo)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.T_OA_MEETINGROOMAPP.MEETINGROOMAPPID == RoomTimeChangeInfo.T_OA_MEETINGROOMAPP.MEETINGROOMAPPID
                    && s.STARTTIME == RoomTimeChangeInfo.STARTTIME && s.ENDTIME == RoomTimeChangeInfo.ENDTIME && s.CREATEUSERID == RoomTimeChangeInfo.CREATEUSERID);
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                 
                }
                else
                {
                    Utility.RefreshEntity(RoomTimeChangeInfo);
                    
                    int i = dal.Add(RoomTimeChangeInfo);

                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;
                
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


         //查询是否添加了同一条记录
        public bool GetMeetingTimeChangeByAppIDStartOrEndOrCreateUser(string StrAppId,DateTime DtStart,DateTime DtEnd,string StrCreateUser)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects<T_OA_MEETINGROOMTIMECHANGE>()
                        //where ent.MEETINGROOMAPPID == StrAppId && ent.STARTTIME == DtStart && ent.ENDTIME == DtEnd && ent.CREATEUSERID == StrCreateUser
                        where ent.T_OA_MEETINGROOMAPP.MEETINGROOMAPPID == StrAppId && ent.STARTTIME == DtStart && ent.ENDTIME == DtEnd && ent.CREATEUSERID == StrCreateUser
                        orderby ent.MEETINGROOMTIMECHANGEID
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
                return false;
                throw (ex);
            }
            //return null;
        }


    }

    #endregion

    #region 会议时间变更

    public class MeetingInfoTimeChangeManagementBll : BaseBll<T_OA_MEETINGTIMECHANGE>
    {
        
        //添加会议室申请内容
        public string AddMeetingInfoTimeChangeInfo(T_OA_MEETINGTIMECHANGE MeetingTimeChangeInfo)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.MEETINGTIMECHANGEID == MeetingTimeChangeInfo.MEETINGTIMECHANGEID
                    && s.STARTTIME == MeetingTimeChangeInfo.STARTTIME && s.ENDTIME == MeetingTimeChangeInfo.ENDTIME && s.CREATEUSERID == MeetingTimeChangeInfo.CREATEUSERID
                    && s.REASON == MeetingTimeChangeInfo.REASON
                    );
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！                 
                }
                else
                {
                    Utility.RefreshEntity(MeetingTimeChangeInfo);

                    int i = dal.Add(MeetingTimeChangeInfo);

                    if (!(i > 0))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;

                //int i = RoomTimeDal.Add(MeetingTimeChangeInfo);
                //if (i == 1)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
                
            }

        }


        //查询是否添加了同一条记录
        public bool GetMeetingInfoTimeChangeByAppIDStartOrEndOrCreateUser(string StrMeetingInfoID, DateTime DtStart, DateTime DtEnd, string StrCreateUser)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetTable()
                        //where ent.MEETINGINFOID == StrMeetingInfoID && ent.STARTTIME == DtStart && ent.ENDTIME == DtEnd && ent.CREATEUSERID == StrCreateUser
                        where ent.MEETINGTIMECHANGEID == StrMeetingInfoID && ent.STARTTIME == DtStart && ent.ENDTIME == DtEnd && ent.CREATEUSERID == StrCreateUser
                        orderby ent.MEETINGTIMECHANGEID
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
                return false;
                throw (ex);
            }
            //return null;
        }


    }

    #endregion

    #region 会议上传材料

    public class MeetingContentManagementBll : BaseBll<T_OA_MEETINGCONTENT>
    {
        //MeetingContentDal ContentDal = new MeetingContentDal();


        //添加会议室申请内容
        public string AddMeetingContentInfo(T_OA_MEETINGCONTENT MeetingContentInfo)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.MEETINGINFOID == MeetingContentInfo.MEETINGINFOID
                    && s.MEETINGUSERID == MeetingContentInfo.MEETINGUSERID
                    );
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！
                    //throw new Exception("Repetition");
                }
                else
                {

                    //int i = dal.Add(MeetingContentInfo);
                    //dal.DataContext.AddObject("T_OA_MEETINGCONTENT", MeetingContentInfo);
                    //int i = dal.DataContext.SaveChanges();
                    bool Isbool=base.Add(MeetingContentInfo);

                    if (!(Isbool))
                    {
                        StrReturn = "SAVEFAILED";//保存失败
                    }
                }
                return StrReturn;

                
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        //查询是否添加了同一条记录
        public bool GetMeetingConetentInfoByMeetingInfoIDAndUserID(string StrMeetingInfoID,string UserID)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGINFOID == StrMeetingInfoID && ent.MEETINGUSERID == UserID
                        orderby ent.MEETINGCONTENTID
                        select ent;
                if (q.Count() > 0)
                {
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        //删除记录
        public bool DeleteMeetingContentInfo(string MeetingContentInfoID)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.MEETINGCONTENTID == MeetingContentInfoID
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
                return false;
                throw (ex);
            }
        }


        //获取单条会员上传内容
        public T_OA_MEETINGCONTENT GetMeetingContentByContentId(string ContentId)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_MEETINGCONTENT>()
                        where ent.MEETINGCONTENTID == ContentId
                        orderby ent.MEETINGCONTENTID
                        select ent;
                if (q.Count() > 0)
                {
                    return q.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }


        //修改会员上传材料信息
        public void UpdateMeetingContentInfo(T_OA_MEETINGCONTENT ContentInfo)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.MEETINGCONTENTID == ContentInfo.MEETINGCONTENTID
                            select ent;

                if (users.Count() > 0)
                {

                    var user = users.FirstOrDefault();
                    
                    user.UPDATEDATE = ContentInfo.UPDATEDATE;
                    user.UPDATEUSERID = ContentInfo.UPDATEUSERID;
                    user.UPDATEUSERNAME = ContentInfo.UPDATEUSERNAME;
                    user.CONTENT = ContentInfo.CONTENT;

                    dal.Update(user);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //获取某次会议的所有与会人员信息
        public List<T_OA_MEETINGCONTENT> GetMeetingContentInfosByMeetingInfoID(string MeetingInfoID)
        {
            try
            {
                var query = from p in dal.GetObjects()
                            where p.MEETINGINFOID == MeetingInfoID
                            orderby p.CREATEDATE descending
                            select p;
                return query.ToList<T_OA_MEETINGCONTENT>();
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }


        




    }

    #endregion



}
