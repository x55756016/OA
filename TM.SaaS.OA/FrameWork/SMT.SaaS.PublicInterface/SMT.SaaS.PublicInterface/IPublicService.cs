using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Activation;
using SMT.SaaS.PublicInterface.BLL;

namespace SMT.SaaS.PublicInterface
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPublicService" in both code and config file together.
    [ServiceContract]
    public interface IPublicService
    {
        /// <summary>
        /// 获取业务对象
        /// </summary>
        /// <param name="SystemCode">系统代码</param>
        /// <param name="BusinessObjectName">业务对象名</param>
        /// <returns>业务对象</returns>
        [OperationContract]
        string GetBusinessObject(string SystemCode, string BusinessObjectName);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>内容</returns>
        [OperationContract]
        byte[] GetContent(string FORMID);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>内容</returns>
        [OperationContract]
        string GetContentFormatImgToUrl(string FORMID);

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>bool</returns>
        [OperationContract]
        bool DeleteContent(string FORMID);

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="content">内容</param>
        /// <param name="UPDATEUSERID">用户ID</param>
        /// <param name="UPDATEUSERNAME">用户名</param>
        /// <returns>bool</returns>
        [OperationContract]
        bool UpdateContent(string FormID, byte[] content, UserInfo userinfo);


        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="FormID">表单ID</param>
        /// <param name="content">内容</param>
        /// <param name="CompanyID">公司ID</param>
        /// <param name="SystemCode">系统代码</param>
        /// <param name="ModelName">模块代码</param>
        /// <param name="userinfo">操作用户信息</param>
        /// <returns>成功:true,失败:false</returns>
        [OperationContract]
        bool AddContent(string FormID, byte[] content, string CompanyID, string SystemCode, string ModelName, UserInfo userinfo);

        /// <summary>
        /// 数据是否存在
        /// </summary>
        /// <param name="FORMID">表单ID</param>
        /// <returns>bool</returns>
        [OperationContract]
        bool IsExits(string FORMID);
    }
}
