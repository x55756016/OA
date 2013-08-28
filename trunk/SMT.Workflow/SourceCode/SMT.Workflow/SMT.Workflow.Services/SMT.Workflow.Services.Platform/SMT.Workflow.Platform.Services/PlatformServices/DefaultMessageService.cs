using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model;

namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : IDefaultMessage
    {
        DefaultMessageBLL bll = new DefaultMessageBLL();

        public List<T_WF_DEFAULTMESSAGE> GetMessage(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount)
        {
            try
            {
                return bll.GetDefaultMessageList(pageSize, pageIndex, strFilter, strOrderBy, ref pageCount);
            }
            catch (Exception ex)
            {
                //记录错误日志
                return null;
            }
        }

        public string InitMessage()
        {
            try
            {
                 bll.InitDefaultMessage();
                 return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string AddMessage(T_WF_DEFAULTMESSAGE entity)
        {
            try
            {
                if (bll.GetBool(entity))
                {
                    bll.AddDefaultMessage(entity);
                }
                else
                {
                    return "2";
                }            
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
       
        public string EditMessage(T_WF_DEFAULTMESSAGE entity)
        {
            try
            {
                if (bll.GetBool(entity))
                {
                    bll.EditDefaultMessage(entity);
                }
                else
                {
                    return "2";
                }
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
         public string  DeleteMessage(string MessageID)
        {
            try
            {
                bll.DeleteDefaultMessage(MessageID);
                return "1";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}