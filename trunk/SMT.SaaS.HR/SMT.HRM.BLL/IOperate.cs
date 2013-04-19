using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMT.HRM.BLL
{
    public interface IOperate:IDisposable
    {
        int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState);
    }
}
