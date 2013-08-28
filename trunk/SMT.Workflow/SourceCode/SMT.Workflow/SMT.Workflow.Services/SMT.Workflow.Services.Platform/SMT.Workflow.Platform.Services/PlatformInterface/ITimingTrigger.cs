using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model.FlowEngine;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
      [ServiceContract]
    public interface ITimingTrigger
    {

          [OperationContract]
          List<T_WF_TIMINGTRIGGERACTIVITY> GetTimingTriggerList(int pageIndex, int pageSize, string strFilter, string strOrderBy, ref int pageCount);

          [OperationContract]
          string DeleteTimingActivity(string timingID);


          [OperationContract]
          string EditTimingActivity(T_WF_TIMINGTRIGGERACTIVITY entity);

           [OperationContract]
          string AddTimingActivity(T_WF_TIMINGTRIGGERACTIVITY entity);
    }
}