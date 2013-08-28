using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMT.Workflow.Common.Model.FlowEngine;
using EngineDataModel;

namespace SMT.Workflow.Engine.Services
{
   
    [ServiceContract]
    public interface IPersonalRecordService
    {
        [OperationContract] 
        bool AddPersonalRecord(T_PF_PERSONALRECORD model);

        [OperationContract]
        bool AddPersonalRecords(List<T_PF_PERSONALRECORD> models);

        [OperationContract]
        bool UpdatePersonalRecord(T_PF_PERSONALRECORD model);

        [OperationContract]
        List<T_PF_PERSONALRECORD> GetPersonalRecord(int pageIndex, string strOrderBy, string checkstate, string filterString, string strCreateID,
            string BeginDate, string EndDate, ref int pageCount);

        [OperationContract]
        bool DeletePersonalRecord(string _personalrecordID);

        [OperationContract]
        T_PF_PERSONALRECORD GetPersonalRecordModelByID(string _personalrecordID);

        [OperationContract]
        T_PF_PERSONALRECORD GetPersonalRecordModelByModelID(string _sysType, string _modelCode, string _modelID, string _IsForward);

        [OperationContract]
        List<T_PF_PERSONALRECORD> GetPersonalRecordList(int pageIndex, string strOrderBy, string checkstate, string filterString,
             string strCreateID, string Isforward, string BeginDate, string EndDate, ref int pageCount);
        [OperationContract]
        List<T_PF_PERSONALRECORD> GetPersonalRecordListNew(int pageIndex, string strOrderBy, string checkstate, string filterString,
             string ownerid, string Isforward, string BeginDate, string EndDate, ref int pageCount);
    }
}
