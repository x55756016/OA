/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：FlowCategory.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/9 10:03:06   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.Services.PlatformServices 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.Workflow.Platform.Services.PlatformInterface;
using SMT.Workflow.Platform.BLL;
using SMT.Workflow.Common.Model;

namespace SMT.Workflow.Platform.Services
{
    public partial class PlatformServices : IFlowCategory
    {
        FlowCategoryBLL flowcategory = new FlowCategoryBLL();

        /// <summary>
        /// 新增流程类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int AddFlowCategory(FLOW_FLOWCATEGORY entity)
        {
            return flowcategory.AddFlowCategory(entity);
        }

        /// <summary>
        /// 判断是否存在类型名称
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public int GetExistsFlowCategory(string Name)
        {
            return flowcategory.GetExistsFlowCategory(Name);
        }

        /// <summary>
        /// 修改流程类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int UpdateFlowCategory(FLOW_FLOWCATEGORY entity)
        {
            return flowcategory.UpdateFlowCategory(entity);
        }

         /// <summary>
        /// 删除流程类型
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        public int DeleteFlowCategory(string CategoryID)
        {
            return flowcategory.DeleteFlowCategory(CategoryID);
        }
    }
}