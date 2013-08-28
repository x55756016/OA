/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：FlowCategoryBLL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/9 9:58:35   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Platform.BLL 
	 * 模块名称：
	 * 描　　述： 	 流程类型
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Platform.DAL;
using SMT.Workflow.Common.Model;

namespace SMT.Workflow.Platform.BLL
{
    public class FlowCategoryBLL
    {
        FlowCategoryDAL dal = new FlowCategoryDAL();

        /// <summary>
        /// 新增流程类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int AddFlowCategory(FLOW_FLOWCATEGORY entity)
        {
            return dal.AddFlowCategory(entity);
        }
        /// <summary>
        /// 判断是否存在类型名称
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public int GetExistsFlowCategory(string Name)
        {
            return dal.GetExistsFlowCategory(Name);
        }
        /// <summary>
        /// 修改流程类型
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int UpdateFlowCategory(FLOW_FLOWCATEGORY entity)
        {
            return dal.UpdateFlowCategory(entity);
        }

        /// <summary>
        /// 删除流程类型
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        public int DeleteFlowCategory(string CategoryID)
        {
            return dal.DeleteFlowCategory(CategoryID);
        }
    }
}
