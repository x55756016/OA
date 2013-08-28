using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : IMessageBodyDefine
    {

        private MessageBodyDefineBLL messagebll = new MessageBodyDefineBLL();

        /// <summary>
        /// 获取默认消息的分页列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="strFilter"></param>
        /// <param name="strOrderBy"></param>
        /// <param name="pageCount"></param>
        /// <returns></returns>
        public List<T_WF_MESSAGEBODYDEFINE> GetDefaultMessgeList(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {
                return messagebll.GetDefaultMessgeList(pageSize, pageIndex, strFilter, strOrderBy, ref pageCount);
            }
            catch (Exception ex)
            {
                //记录错误日志
                return null;
            }
        }

        public string AddDefaultMessge(T_WF_MESSAGEBODYDEFINE entity)
        {
            try
            {
                if (messagebll.ExistsDefaultMessage(entity))
                {
                    messagebll.AddDefaultMessage(entity);
                    return "1";
                }
                else
                {
                    return "2";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string EditDefaultMessge(T_WF_MESSAGEBODYDEFINE entity)
        {
            try
            {
                if (messagebll.ExistsDefaultMessage(entity))
                {
                    messagebll.EditDefaultMessage(entity);
                    return "1";
                }
                else
                {
                    return "2";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteDefaultMessge(string DeleteID)
        {
            try
            {
                messagebll.DeleteDefaultMessage(DeleteID);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}