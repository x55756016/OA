using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace SMT.SaaS.OA.BLL
{
    public interface ILookupEntity
    {
        //int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState,string userID
        //EntityObject[] GetLookupData(Dictionary<string, string> args);
        EntityObject[] GetLookupData(Dictionary<string, string> args, int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID);
    }
}
