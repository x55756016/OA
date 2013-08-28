using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SMT.Workflow.Common.Model.FlowXml;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    /// <summary>
    /// 流程定义接口
    /// </summary>
    [ServiceContract]
    public interface IFlowXmlDefine
    {
        //读取Xml值
        [OperationContract]
        List<AppSystem> ListSystem();
        //读取Xml值
        [OperationContract]
        List<AppModel> AppModel(string ObjectFolder);
        //读取Xml值
        [OperationContract]
        List<AppModel> ListModel(List<string> ObjectFolder);
        //读取Xml值
        [OperationContract]
        List<TableColumn> ListTableColumn(string ObjectFolder, string strFileName);

        [OperationContract]
        Dictionary<List<AppFunc>, List<TableColumn>> ListFuncTableColumn(string ObjectFolder, string strFileName, ref string MsgLinkUrl);

        [OperationContract]
        List<WFBOAttribute> GetSystemBOAttributeList(string systemName, string objectName);

        //生成消息URL
        [OperationContract]
        List<AppFunc> ListSystemFunc(string ObjectFolder, string strFolderName, ref string MsgLinkUrl);

    }
}
