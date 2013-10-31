using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Objects.DataClasses;
using System.Collections;

namespace SMT.SaaS.Permission.BLL
{
    public interface ILookupEntity
    {
       // EntityObject[] GetLookupData(Dictionary<string, string> args);
        EntityObject[] GetLookupData(Dictionary<string, string> args,int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount);
    }
}
