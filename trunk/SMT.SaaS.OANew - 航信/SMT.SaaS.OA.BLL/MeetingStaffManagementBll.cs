using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;

namespace SMT.SaaS.OA.BLL
{
    public class MeetingStaffManagementBll:BaseBll<T_OA_MEETINGSTAFF>
    {
        
        //添加与会人员信息
        public string AddMeetingStaffInfo(T_OA_MEETINGSTAFF StaffInfo)
        {
            try
            {
                string StrReturn = "";
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.MEETINGINFOID == StaffInfo.T_OA_MEETINGINFO.MEETINGINFOID
                    && s.MEETINGUSERID == StaffInfo.MEETINGUSERID 
                    );
                if (tempEnt != null)
                {
                    StrReturn = "REPETITION"; //{0}已存在，保存失败！
                    //throw new Exception("Repetition");
                }
                else
                {
                    var ent = (from a in dal.GetObjects<T_OA_MEETINGINFO>()
                               where a.MEETINGINFOID == StaffInfo.T_OA_MEETINGINFO.MEETINGINFOID
                               select a
                                   ).FirstOrDefault();
                    StaffInfo.T_OA_MEETINGINFO = ent;
                    int i = dal.Add(StaffInfo);


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

        //判断是否有相同与会人员记录
        
        public bool GetMeetingStaffByMeetingInfoAndUserID(string StrMeetingInfoID,string UserID)
        {
            try
            {
                bool IsExist = false;
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGINFOID == StrMeetingInfoID && ent.MEETINGUSERID == UserID
                        orderby ent.MEETINGSTAFFID
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

        public T_OA_MEETINGSTAFF GetMeetingStaffByMeetingInfoAndUserIDSecond(string StrMeetingInfoID, string UserID)
        {
            try
            {
                
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGINFOID == StrMeetingInfoID && ent.MEETINGUSERID == UserID
                        orderby ent.MEETINGSTAFFID
                        select ent;
                return q.Count() > 0 ? q.FirstOrDefault() : null;
                
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }


        


        public bool DeleteMeetingStaffInfo(string StaffID)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.MEETINGSTAFFID == StaffID
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



        public void UpdateMeetingStaffInfo(T_OA_MEETINGSTAFF StaffInfo)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.MEETINGSTAFFID == StaffInfo.MEETINGSTAFFID
                            select ent;

                if (users.Count() > 0)
                {

                    var user = users.FirstOrDefault();                
                    user.FILENAME = StaffInfo.FILENAME;
                    user.MEETINGSTAFFID = StaffInfo.MEETINGSTAFFID;
                    user.CONFIRMFLAG = StaffInfo.CONFIRMFLAG;
                    user.UPDATEDATE = StaffInfo.UPDATEDATE;
                    user.UPDATEUSERID = StaffInfo.UPDATEUSERID;
                    user.UPDATEUSERNAME = StaffInfo.UPDATEUSERNAME;
                    user.ISOK = StaffInfo.ISOK;
                    dal.Update(user);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        //获取单条与会人员信息
        public T_OA_MEETINGSTAFF GetMeetingStaffInfoById(string StaffId)
        {
            try
            {
                var q = from ent in dal.GetObjects()
                        where ent.MEETINGSTAFFID == StaffId
                        orderby ent.MEETINGSTAFFID
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

        //获取所有的与会人员信息
        public List<T_OA_MEETINGSTAFF> GetMeetingStaffInfos()
        {
            try
            {
                var query = from p in dal.GetObjects()
                            orderby p.CREATEDATE descending
                            select p;
                return query.ToList<T_OA_MEETINGSTAFF>();
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }

        }



        //获取某次会议的所有与会人员信息
        public List<T_OA_MEETINGSTAFF> GetMeetingStaffInfosByMeetingInfoID(string MeetingInfoID)
        {
            try
            {
                var query = from p in dal.GetObjects()
                            where p.MEETINGINFOID == MeetingInfoID
                            orderby p.CREATEDATE descending
                            select p;
                if (query.Count() > 0)
                {
                    return query.ToList<T_OA_MEETINGSTAFF>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }

        }
        //获取某会议的确认参会人员上传的内容和附件
        public IQueryable<V_MyMeetingInfosManagement> GetMeetingStaffInfosByMeetingInfoIDEmcee(string StrFlag,string StrIsOk,string MeetingInfoID)
        {
            try
            {
                var query = from p in dal.GetObjects()
                            join n in dal.GetObjects <T_OA_MEETINGCONTENT>() on p.MEETINGINFOID equals n.MEETINGINFOID
                            where p.MEETINGINFOID == MeetingInfoID && n.MEETINGINFOID == MeetingInfoID && p.MEETINGUSERID == n.MEETINGUSERID && p.CONFIRMFLAG == "1"

                            orderby p.CREATEDATE descending
                            select new V_MyMeetingInfosManagement { OAMeetingStaffT = p, OAMeetingContentT = n };
                if (!string.IsNullOrEmpty(StrFlag))
                {
                    query = query.Where(a => a.OAMeetingStaffT.CONFIRMFLAG == StrFlag);
                }
                if (!string.IsNullOrEmpty(StrIsOk))
                {
                    query = query.Where(a => a.OAMeetingStaffT.ISOK == StrIsOk);
                }
                return query;
            }
            catch (Exception ex)
            {
                return null;
                throw (ex);
            }
        }





    }
}
