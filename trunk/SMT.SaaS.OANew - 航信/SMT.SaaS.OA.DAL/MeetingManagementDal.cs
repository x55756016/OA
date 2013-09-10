using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_OA_EFModel;
using SMT.Foundation.Core;
using System.Data.Objects;

namespace SMT.SaaS.OA.DAL
{
    //会议信息
    public class MeetingManagementDal: CommDaL<T_OA_MEETINGINFO>
    {
    }
    //会议室实例
    public class MeetingRoomDal : CommDaL<T_OA_MEETINGROOM>
    { 
    }

    //会议类型
    public class MeetingTypeDal : CommDaL<T_OA_MEETINGTYPE>
    { 
    }

    //会议模板
    public class MeetingTemplateDal : CommDaL<T_OA_MEETINGTEMPLATE>
    { 
    }
    //与会人员
    public class MeetingStaffDal : CommDaL<T_OA_MEETINGSTAFF>
    { 

    }

    //会议室申请信息
    public class MeetingRoomAppDal : CommDaL<T_OA_MEETINGROOMAPP>
    { 

    }

    //会议变更信息表
    public class MeetingTimeChangeDal : CommDaL<T_OA_MEETINGTIMECHANGE>
    {

    }
    //会议室使用时间
    public class MeetingRoomTimeChangeDal : CommDaL<T_OA_MEETINGROOMTIMECHANGE>
    {

    }
    //会议上传材料
    public class MeetingContentDal : CommDaL<T_OA_MEETINGCONTENT>
    {

    }



}
