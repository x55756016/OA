using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model.FlowEngine;


namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : ITimingTrigger
    {

        private TimingTriggerBLL timingbll = new TimingTriggerBLL();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strFilter"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_WF_TIMINGTRIGGERACTIVITY> GetTimingTriggerList(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {
                return timingbll.GetTimingTriggerList(pageSize, pageIndex, strFilter, strOrderBy, ref pageCount);
            }
            catch (Exception ex)
            {
                //记录错误日志
                return null;
            }
        }

        public string AddTimingActivity(T_WF_TIMINGTRIGGERACTIVITY entity)
        {
            try
            {
                timingbll.AddTimingActivity(entity);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string EditTimingActivity(T_WF_TIMINGTRIGGERACTIVITY entity)
        {
            try
            {
                timingbll.EditTimingActivity(entity);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteTimingActivity(string timingID)
        {
            try
            {
                timingbll.DeleteTimingActivity(timingID);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}