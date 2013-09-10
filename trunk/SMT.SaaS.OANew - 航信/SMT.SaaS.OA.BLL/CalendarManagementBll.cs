using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
namespace SMT.SaaS.OA.BLL
{
    public class CalendarManagementBll : BaseBll<T_OA_CALENDAR>
    {
        /// <summary>
        /// 获取日程安排管理信息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_OA_CALENDAR> GetCalendarListByUserID(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            try
            {
                var q = from ent in dal.GetTable()

                        select ent;
                List<object> queryParas = new List<object>();
                if (paras != null)
                {
                    queryParas.AddRange(paras);
                }
                UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_CALENDAR");
                if (!string.IsNullOrEmpty(filterString))
                {
                    q = q.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                }
                q = q.OrderBy(sort);
                q = Utility.Pager<T_OA_CALENDAR>(q, pageIndex, pageSize, ref pageCount);
                if (q.Count() > 0)
                {
                    return q;
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("日程管理CalendarManagementBll-GetCalendarListByUserID" + System.DateTime.Now.ToString() + " " + ex.ToString());
                
            }
            return null;
        }
        /// <summary>
        /// 通过权限过滤日程管理
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_OA_CALENDAR> GetCalendarList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_CALENDAR>()
                        select ent;
                
                List<object> queryParas = new List<object>();
                if (paras != null)
                {
                    queryParas.AddRange(paras);
                }
                string bb = filterString;
                
                if (!(filterString.IndexOf("OWNERID") > -1))
                {
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_CALENDAR");
                }

                

                if (!string.IsNullOrEmpty(filterString))
                {
                    q = q.Where(filterString, queryParas.ToArray());
                }
                q = q.OrderBy(sort);

                q = Utility.Pager<T_OA_CALENDAR>(q, pageIndex, pageSize, ref pageCount);

                if (q.Count() > 0)
                {
                    return q;
                }
                
            }
            catch (Exception ex)
            {
                Tracer.Debug("日程管理CalendarManagementBll-GetCalendarList" + System.DateTime.Now.ToString() + " " + ex.ToString());
            }

            return null;
        }
        /// <summary>
        /// 添加日程管理信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool AddCalendarInfo(T_OA_CALENDAR entity)
        {
            try
            {
                int i = dal.Add(entity);
                if (i == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("日程管理CalendarManagementBll-AddCalendarInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        /// <summary>
        /// 删除日程管理信息
        /// </summary>
        /// <param name="calendarGuid">日程管理ID</param>
        /// <returns></returns>
        public bool DeleteCalendarInfo(string calendarGuid)
        {
            try
            {
                var entitys = (from ent in dal.GetTable()
                               where ent.CALENDARID == calendarGuid
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
                Tracer.Debug("日程管理CalendarManagementBll-DeleteCalendarInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }
        /// <summary>
        /// 修改日程管理信息
        /// </summary>
        /// <param name="entity">日程管理实体</param>
        /// <returns></returns>
        public int UpdateCalendarInfo(T_OA_CALENDAR entity)
        {
            try
            {
                var users = from ent in dal.GetTable()
                            where ent.CALENDARID == entity.CALENDARID
                            select ent;
                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.TITLE = entity.TITLE;
                    user.CONTENT = entity.CONTENT;
                    user.REMINDERRMODEL = entity.REMINDERRMODEL;
                    user.REPARTREMINDER = entity.REPARTREMINDER;
                    user.UPDATEDATE = entity.UPDATEDATE;
                    user.UPDATEUSERID = entity.UPDATEUSERID;
                    user.PLANTIME = entity.PLANTIME;
                    return dal.Update(user);
                }
                return -1;
            }
            catch (Exception ex)
            {
                Tracer.Debug("日程管理CalendarManagementBll-UpdateCalendarInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return -1;
                
            }
        }
    }
}