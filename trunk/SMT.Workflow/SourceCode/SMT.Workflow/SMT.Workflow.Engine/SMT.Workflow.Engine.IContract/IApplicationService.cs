/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：IApplicationService.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/23 14:59:00   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.IContract 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SMT.Workflow.Engine.IContract
{
    [ServiceContract]
    public interface IApplicationService
    {
        /// <summary>
        /// 一个参数无返回值
        /// </summary>
        /// <param name="param"></param>
        [OperationContract(IsOneWay = true)]
        void CallApplicationService(string strXml);

        /// <summary>
        /// 一个参数有返回值
        /// </summary>
        /// <param name="strXml"></param>
        /// <returns></returns>
        [OperationContract]
        string CallWaitAppService(string strXml);
    }
}
