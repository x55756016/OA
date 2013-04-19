using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.CustomModel.Reports;
using SMT_HRM_EFModel;

namespace SMT.HRM.BLL.Sqlscript
{
    /// <summary>
    /// 考勤汇总一览
    /// </summary>
    public class AbnormRecordBllTe : BaseBll<T_HR_EMPLOYEEABNORMRECORD>
    {
        #region  在线考勤汇总一览表 weirui 2012-12-13 add
        /// <summary>
        /// 根据员工ID和公司ID查询考勤记录表 weirui 2012-12-6 add
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>考勤记录表</returns>
        public List<AbnormalAttendanceeEntity> ListEMPLOYEEABNORMRECORD(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            List<AbnormalAttendanceeEntity> listEMPLOYEE = new List<AbnormalAttendanceeEntity>();

            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);
            try
            {
                //根据

                //根据公司ID查询此公司的员工信息 目标集合
                var companyInfo = from t in dal.GetObjects<T_HR_EMPLOYEE>()
                                  where t.EMPLOYEESTATE != "2"
                                  && t.OWNERCOMPANYID == ownerCompanyId
                                  select t;
                //表示选择人员查询,数据过滤
                if (!string.IsNullOrEmpty(ownerId))
                {
                    companyInfo = from t in companyInfo
                                  where t.EMPLOYEEID == ownerId
                                  select t;
                    //companyInfo.Where(c => c.EMPLOYEEID == ownerId);
                }

                //查询异常考勤记录表
                var q = from t in dal.GetObjects<T_HR_EMPLOYEEABNORMRECORD>()
                        join p in dal.GetObjects<T_HR_ATTENDANCERECORD>()
                        on t.T_HR_ATTENDANCERECORD.ATTENDANCERECORDID equals p.ATTENDANCERECORDID
                        where t.OWNERCOMPANYID == ownerCompanyId
                        && t.ABNORMALDATE >= startDates
                        && t.ABNORMALDATE <= endDates
                        select t;
                //已目标值为基础 左连接
                var enployeeInfo = from t in companyInfo
                                   join y in q
                                   on t.EMPLOYEEID equals y.OWNERID into EMPLOYEE
                                   from n in EMPLOYEE.DefaultIfEmpty()
                                   select new AbnormalAttendanceeEntity
                                   {
                                       //员工名字
                                       cname = t.EMPLOYEECNAME,
                                       //员工ID
                                       EMPLOYEEID = t.EMPLOYEEID,
                                       //异常类型
                                       ABNORMCATEGORY = n.ABNORMCATEGORY,
                                       //异常时长
                                       ABNORMALTIME = n.ABNORMALTIME
                                   };

                if (enployeeInfo.Count() > 0)
                {
                    return enployeeInfo.ToList();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 根据公司IDOr员工ID查找员工请假记录
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>请假记录</returns>
        public List<AbnormalAttendanceeEntity> GetEmployeeLeaverecord(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);

            //根据公司ID查询假期类型表，得到事假2、年休假4、病假3、调休假1 id
            //var y=from t in dal.GetObjects<T_HR_LEAVETYPESET>()LEAVETYPESETID
            var o = from t in dal.GetObjects<T_HR_EMPLOYEELEAVERECORD>()
                    join p in dal.GetObjects<T_HR_LEAVETYPESET>().Where(c => c.LEAVETYPEVALUE == "2" || c.LEAVETYPEVALUE == "4"
                        || c.LEAVETYPEVALUE == "3" || c.LEAVETYPEVALUE == "1")
                    on t.T_HR_LEAVETYPESET.LEAVETYPESETID equals p.LEAVETYPESETID
                    where t.OWNERCOMPANYID == ownerCompanyId
                    && t.STARTDATETIME >= startDates
                    && t.ENDDATETIME <= endDates
                    && t.CHECKSTATE == "2"
                    select new AbnormalAttendanceeEntity
                    {
                        //员工名字
                        cname = t.EMPLOYEENAME,
                        //员工ID
                        EMPLOYEEID = t.EMPLOYEEID,
                        //请假类型
                        LeaverecordStyple = p.LEAVETYPEVALUE,
                        //请假时长
                        LeaverecordTime = t.TOTALHOURS
                    };
            //表示选择人员查询,数据过滤
            if (!string.IsNullOrEmpty(ownerId))
            {
                o.Where(c => c.EMPLOYEEID == ownerId);
            }

            #region 删除
            ////员工的 事假2、年休假4、病假3、调休假1计算
            //var str = (from t in o
            //        group t by
            //        new
            //        {
            //            t.EMPLOYEEID,
            //            t.cname
            //        }
            //            into g
            //            select new AbnormalAttendanceeEntity
            //            {
            //                //员工ID
            //                EMPLOYEEID = g.Key.EMPLOYEEID,
            //                //员工姓名
            //                cname = g.Key.cname,

            //                //请事假时长
            //                LeaveHour = g.Where(c => c.LeaverecordStyple == "2").Sum(c => c.LeaverecordTime),
            //                //请年假时长
            //                AnnualLeave = g.Where(c => c.LeaverecordStyple == "4").Sum(c => c.LeaverecordTime),
            //                //请病假时长
            //                SickLeave = g.Where(c => c.LeaverecordStyple == "3").Sum(c => c.LeaverecordTime),
            //                //请调休假时长
            //                OffHour = g.Where(c => c.LeaverecordStyple == "1").Sum(c => c.LeaverecordTime),
            //                AdjustableVacation=0
            //            }).ToList();
            //在时间范围内可修假天数
            //var v = from t in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
            //        where t.EFFICDATE >= startDates
            //        && t.TERMINATEDATE <= endDates
            //        && t.VACATIONTYPE=="1"
            //        && (t.EMPLOYEEID == ownerId || t.OWNERCOMPANYID == ownerCompanyId)
            //        select new AbnormalAttendanceeEntity
            //        {
            //            //员工名字
            //            cname = t.EMPLOYEENAME,
            //            //员工ID
            //            EMPLOYEEID = t.EMPLOYEEID,
            //            //请假天数
            //            AdjustableDay =t.DAYS

            //        };

            //var str2 = (from t in v
            //           group t by
            //           new
            //           {
            //               t.EMPLOYEEID,
            //               t.cname
            //           } into g
            //           select new AbnormalAttendanceeEntity
            //           {
            //               //员工ID
            //               EMPLOYEEID = g.Key.EMPLOYEEID,
            //               //员工姓名
            //               cname = g.Key.cname,
            //               //可休假天数
            //               AdjustableDay=g.Sum(c=>c.AdjustableDay)
            //           }).ToList();

            //if (str2.Count()>0 && str.Count()>0)
            //{
            //    foreach (var item in str2)
            //    {
            //        foreach (var item2 in str)
            //        {
            //            if (item.EMPLOYEEID == item2.EMPLOYEEID)
            //            {
            //                item2.AdjustableVacation = decimal.Parse((7.5 * double.Parse(item.AdjustableDay.ToString())
            //                    - double.Parse(item2.OffHour.ToString())).ToString());
            //            }
            //        }
            //    }
            //}

            //str2.ToList();
            #endregion

            if (o.Count() > 0)
            {
                return o.ToList();
            }
            return null;
        }

        /// <summary>
        /// 根据公司IDOr员工ID查找员工可以调休的请假天数(掉休假)
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>请假天数(掉休假)</returns>
        public List<AbnormalAttendanceeEntity> GetAdjustableVacation(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);
            var v = from t in dal.GetObjects<T_HR_EMPLOYEELEVELDAYCOUNT>()
                    where t.EFFICDATE >= startDates
                    && t.TERMINATEDATE <= endDates
                    && t.VACATIONTYPE == "1"
                    && (t.EMPLOYEEID == ownerId || t.OWNERCOMPANYID == ownerCompanyId)
                    select new AbnormalAttendanceeEntity
                    {
                        //员工名字
                        cname = t.EMPLOYEENAME,
                        //员工ID
                        EMPLOYEEID = t.EMPLOYEEID,
                        //请假天数
                        AdjustableDay = t.DAYS
                    };

            //表示选择人员查询,数据过滤
            if (!string.IsNullOrEmpty(ownerId))
            {
                v.Where(c => c.EMPLOYEEID == ownerId);
            }

            if (v.Count() > 0)
            {
                return v.ToList();
            }
            return null;
        }

        /// <summary>
        /// 得到每天最后的打卡时间
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>最后的打卡时间</returns>
        public List<AbnormalAttendanceeEntity> GetLasterClockInRecord(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            DateTime startDates = DateTime.Parse(startDate);
            DateTime endDates = DateTime.Parse(endDate);

            var v = from t in dal.GetObjects<T_HR_EMPLOYEECLOCKINRECORD>()
                    where t.PUNCHDATE >= startDates
                    && t.PUNCHDATE <= endDates
                    && t.OWNERCOMPANYID == ownerCompanyId
                    group t by
                    new
                    {
                        EMPLOYEENAME = t.EMPLOYEENAME,
                        EMPLOYEEID = t.EMPLOYEEID,
                        //PUNCHDATE=new DateTime(t.PUNCHDATE.Value.Year,t.PUNCHDATE.Value.Month,t.PUNCHDATE.Value.Day)
                        YEAR = t.PUNCHDATE.Value.Year,
                        Month = t.PUNCHDATE.Value.Month,
                        Day = t.PUNCHDATE.Value.Day
                    } 
                    into g
                    select new AbnormalAttendanceeEntity
                    {
                        cname = g.Key.EMPLOYEENAME,
                        EMPLOYEEID = g.Key.EMPLOYEEID,
                        Punchdate = g.Max(c => c.PUNCHDATE)
                    };

            //表示选择人员查询,数据过滤
            if (!string.IsNullOrEmpty(ownerId))
            {
                v.Where(c => c.EMPLOYEEID == ownerId);
            }

            if (v.Count() > 0)
            {
                return v.ToList();
            }
            return null;
        }
        /// <summary>
        /// 考勤汇总导出
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>考勤汇总</returns>
        public List<AbnormalAttendanceeEntity> ExportEmployees(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity = new List<AbnormalAttendanceeEntity>();

            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity2 = new List<AbnormalAttendanceeEntity>();

            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity3 = new List<AbnormalAttendanceeEntity>();

            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity4 = new List<AbnormalAttendanceeEntity>();
            //请假记录
            abnormalAttendanceeEntity = GetEmployeeLeaverecord(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity != null)
            {
                var v = from t in abnormalAttendanceeEntity
                        group t by
                        new
                        {
                            t.EMPLOYEEID,
                            t.cname
                        }
                            into g
                            select new AbnormalAttendanceeEntity
                            {
                                //员工ID
                                EMPLOYEEID = g.Key.EMPLOYEEID,
                                //员工姓名
                                cname = g.Key.cname,

                                //请事假时长
                                LeaveHour = g.Where(c => c.LeaverecordStyple == "2").Sum(c => c.LeaverecordTime),
                                //请年假时长
                                AnnualLeave = g.Where(c => c.LeaverecordStyple == "4").Sum(c => c.LeaverecordTime),
                                //请病假时长
                                SickLeave = g.Where(c => c.LeaverecordStyple == "3").Sum(c => c.LeaverecordTime),
                                //请调休假时长
                                OffHour = g.Where(c => c.LeaverecordStyple == "1").Sum(c => c.LeaverecordTime)
                            };
                abnormalAttendanceeEntity = v.ToList();
            }
            //超额工时
            abnormalAttendanceeEntity4 = GetLasterClockInRecord(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity4 != null)
            {
                var v = from t in abnormalAttendanceeEntity4
                        group t by new
                        {
                            t.cname,
                            t.EMPLOYEEID
                        } 
                        into g
                        select new AbnormalAttendanceeEntity
                        {
                            //员工ID
                            EMPLOYEEID = g.Key.EMPLOYEEID,
                            //员工姓名
                            cname = g.Key.cname,
                            ExcessHoursTotal = g.Where(c => c.Punchdate.Value.Hour > 6).Sum(c => c.Punchdate.Value.Hour)
                            + (g.Where(c => c.Punchdate.Value.Hour > 6).Sum(c => c.Punchdate.Value.Minute) / 60)
                        };
                abnormalAttendanceeEntity4 = v.ToList();
            }
            //可以调休的请假天数
            abnormalAttendanceeEntity3 = GetAdjustableVacation(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity3 != null)
            {
                var v = from t in abnormalAttendanceeEntity3
                        group t by
                        new
                        {
                            t.EMPLOYEEID,
                            t.cname
                        } 
                        into g
                        select new AbnormalAttendanceeEntity
                        {
                            //员工ID
                            EMPLOYEEID = g.Key.EMPLOYEEID,
                            //员工姓名
                            cname = g.Key.cname,
                            //可调休假
                            AdjustableDay = g.Sum(c => c.AdjustableDay)
                        };
                abnormalAttendanceeEntity3 = v.ToList();
            }

            //考勤
            abnormalAttendanceeEntity2 = ListEMPLOYEEABNORMRECORD(ownerId, ownerCompanyId, startDate, endDate);
            if (abnormalAttendanceeEntity2 != null)
            {
                var v = from t in abnormalAttendanceeEntity2
                        group t by new
                        {
                            t.EMPLOYEEID,
                            t.cname
                        } 
                        into g
                        select new AbnormalAttendanceeEntity
                        {
                            //名字
                            cname = g.Key.cname,
                            //员工ID
                            EMPLOYEEID = g.Key.EMPLOYEEID,
                            //迟到/早退次数
                            outTimes = g.Where(c => c.ABNORMCATEGORY == "1").Count(),
                            //迟到/早退合计小时
                            outMinutes = g.Where(c => c.ABNORMCATEGORY == "1").Sum(c => c.ABNORMALTIME),
                            //缺勤次数
                            DrainTimeNumber = g.Where(c => c.ABNORMCATEGORY == "3").Count(),
                            //超额工时合计
                            ExcessHoursTotal = 0,
                            //可调休假
                            AdjustableVacation = 0,
                            //事假
                            LeaveHour = 0,
                            //年休假
                            AnnualLeave = 0,
                            //病假
                            SickLeave = 0,
                            //调休假
                            OffHour = 0
                        };
                abnormalAttendanceeEntity2 = v.ToList();
                if (abnormalAttendanceeEntity != null)
                {
                    foreach (var item in abnormalAttendanceeEntity)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID == item2.EMPLOYEEID)
                            {
                                item2.LeaveHour = item.LeaveHour;
                                item2.AnnualLeave = item.AnnualLeave;
                                item2.SickLeave = item.SickLeave;
                                item2.OffHour = item.OffHour;
                            }
                        }
                    }
                }
                if (abnormalAttendanceeEntity3 != null)
                {
                    foreach (var item in abnormalAttendanceeEntity3)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID == item2.EMPLOYEEID)
                            {
                                double dou = Double.Parse(item.AdjustableDay.ToString()) * 7.5
                                    - Double.Parse(item2.OffHour.ToString());
                                item2.AdjustableVacation = decimal.Parse(dou.ToString());
                            }
                        }
                    }
                }
                if (abnormalAttendanceeEntity4 != null)
                {
                    foreach (var item in abnormalAttendanceeEntity4)
                    {
                        foreach (var item2 in abnormalAttendanceeEntity2)
                        {
                            if (item.EMPLOYEEID == item2.EMPLOYEEID)
                            {
                                item2.ExcessHoursTotal = item.ExcessHoursTotal;
                            }
                        }
                    }
                }
            }
            return abnormalAttendanceeEntity2;
        }

        /// <summary>
        /// 数据转换 导出考勤
        /// </summary>
        /// <param name="ownerId">员工ID</param>
        /// <param name="ownerCompanyId">公司ID</param>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <returns>数据转换 导出考勤</returns>
        public byte[] ExportEmployeesIntime(string ownerId, string ownerCompanyId, string startDate, string endDate)
        {
            List<AbnormalAttendanceeEntity> abnormalAttendanceeEntity = ExportEmployees(ownerId, ownerCompanyId, startDate, endDate);

            try
            {
                if (abnormalAttendanceeEntity.Count() > 0)
                {
                    List<string> colName = new List<string>();
                    colName.Add("员工姓名");
                    colName.Add("迟到/早退总次数");
                    colName.Add("迟到/早退合计（分钟)");
                    colName.Add("漏打卡次数");
                    colName.Add("超额工时合计（小时）");
                    colName.Add("可调休假（小时）");
                    colName.Add("事假（小时）");
                    colName.Add("年休假（小时）");
                    colName.Add("病假（小时）");
                    colName.Add("调休假（小时）");
                    //var tmp = new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "EMPLOYEESTATE", "TOPEDUCATION", "NATION" });

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < colName.Count; i++)
                    {
                        sb.Append(colName[i] + ",");
                    }
                    sb.Append("\r\n"); // 列头

                    //内容
                    foreach (var employeeinfo in abnormalAttendanceeEntity)
                    {
                        sb.Append(employeeinfo.cname + ",");
                        sb.Append(employeeinfo.outTimes + ",");
                        sb.Append(employeeinfo.outMinutes + ",");
                        sb.Append(employeeinfo.DrainTimeNumber + ",");
                        sb.Append(employeeinfo.ExcessHoursTotal + ",");
                        sb.Append(employeeinfo.AdjustableVacation + ",");
                        sb.Append(employeeinfo.LeaveHour + ",");
                        sb.Append(employeeinfo.AnnualLeave + ",");
                        sb.Append(employeeinfo.SickLeave + ",");
                        sb.Append(employeeinfo.OffHour + ",");
                        sb.Append("\r\n");
                    }
                    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeesIntime:" + ex.Message);
                return null;
            }
        }
        #endregion
    }
}
