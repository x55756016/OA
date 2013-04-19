using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using SMT_OA_EFModel;
using SMT.SaaS.OA.BLL;
namespace SMT.SaaS.OA.Services
{
    public partial class SmtOAPersonOffice
    {

        [OperationContract]
        public List<T_OA_CALENDAR> GetCalendarListByUserID(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            using (CalendarManagementBll calendarBll = new CalendarManagementBll())
            {            
                IQueryable<T_OA_CALENDAR> calendarList = calendarBll.GetCalendarListByUserID(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID);
                if (calendarList == null)
                {
                    return null;
                }
                else
                {
                    return calendarList.ToList();
                }
            }
        }

        [OperationContract]
        public int DelCalendarList(List<T_OA_CALENDAR> infoList)
        {
            using (CalendarManagementBll calendarBll = new CalendarManagementBll())
            {                
                foreach (T_OA_CALENDAR obj in infoList)
                {
                    if (!calendarBll.DeleteCalendarInfo(obj.CALENDARID))
                    {
                        return -1;
                    }
                }
                return 1;
            }
        }

        [OperationContract]
        public int AddCalendar(T_OA_CALENDAR obj)
        {
            using (CalendarManagementBll calendarBll = new CalendarManagementBll())
            {                
                bool sucess = calendarBll.AddCalendarInfo(obj);
                if (sucess == false)
                {
                    return -1;
                }
                return 1;
            }
        }

        [OperationContract]
        public int DelCalendar(T_OA_CALENDAR obj)
        {
            using (CalendarManagementBll calendarBll = new CalendarManagementBll())
            {
                
                if (calendarBll.DeleteCalendarInfo(obj.CALENDARID))
                {
                    return 1;
                }
                return -1;
            }
        }
        [OperationContract]
        public int UpdateCalendar(T_OA_CALENDAR obj)
        {
            using (CalendarManagementBll calendarBll = new CalendarManagementBll())
            {            
                return calendarBll.UpdateCalendarInfo(obj);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">页数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">条件字符串</param>
        /// <param name="paras">参数值</param>
        /// <param name="pageCount">页总数</param>        
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [OperationContract]        
        public List<T_OA_CALENDAR> GetApporvalList22(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, LoginUserInfo loginUserInfo)
        {

            using (CalendarManagementBll calendarBll = new CalendarManagementBll())
            {

                IQueryable<T_OA_CALENDAR> approvalList = calendarBll.GetCalendarList(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, loginUserInfo.userID);
                if (approvalList == null)
                {
                    return null;
                }
                else
                {
                    return approvalList.ToList();
                }                
            }
        }
    }
}
