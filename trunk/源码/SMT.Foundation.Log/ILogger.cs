using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SMT.Foundation.Log
{
    interface ILogger
    {
        void Write(ErrorLog message);

        //DataSet RetrieveErrorLogs(DateTime dtfrom, DateTime dtto, string strUserID);

        //ErrorLog RetrieveErrorLogById(Guid ErrorLogID);

        //void DeleteErrorLog(Guid ErrorLogID);
    }
}
