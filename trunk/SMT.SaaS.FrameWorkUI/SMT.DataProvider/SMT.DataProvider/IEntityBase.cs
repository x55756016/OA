using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Data;

namespace SMT
{
    [ServiceContract]
    interface IEntityBase
    {
        [OperationContract]
        EntityResult AddByTransaction(IDbCommand idbconn);
        [OperationContract]
        EntityResult UpdateByTransaction(IDbCommand idbconn);
        [OperationContract]
        EntityResult DeleteByTransaction(IDbCommand idbconn);
        [OperationContract]
        EntityResult Delete();
        [OperationContract]
        EntityResult Delete(string childTableName, string primaryKeyName);
        [OperationContract]
        EntityResult Update();
        [OperationContract]
        EntityResult Add();
        [OperationContract]
        EntityResult Exists();
    }
}
