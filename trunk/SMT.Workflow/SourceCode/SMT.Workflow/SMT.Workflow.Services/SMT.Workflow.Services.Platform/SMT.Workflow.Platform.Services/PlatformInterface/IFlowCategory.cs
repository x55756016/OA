/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：IFlowCategory.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/9 10:01:47   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Services.PlatformInterface 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using SMT.Workflow.Common.Model;

namespace SMT.Workflow.Platform.Services.PlatformInterface
{
    [ServiceContract]
    public interface IFlowCategory
    {
        [OperationContract]
        int AddFlowCategory(FLOW_FLOWCATEGORY entity);

        [OperationContract]
        int UpdateFlowCategory(FLOW_FLOWCATEGORY entity);

        [OperationContract]
        int GetExistsFlowCategory(string Name);
        [OperationContract]
        int DeleteFlowCategory(string CategoryID);
    }
}