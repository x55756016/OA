using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.OracleClient;
using SMT.Workflow.Common.DataAccess;
using System.Data;
using SMT.SaaS.BLLCommonServices.PermissionWS;

namespace SMT.FlowWFService.NewFlow
{
    /// <summary>
    /// 当前提交,流程所属人用户信息
    /// </summary>
    public class FlowUser
    {
        
        public FlowUser(string companyid, string userid, OracleConnection con, string modelCode)
        {
            string names = "";
            try
            {
                ModelInfo modelinfo = new ModelInfo();
                FlowUserInfo[] Users = { };
                #region ModelInfo加入缓存
                object modelinfoObj= CacheProvider.GetCache(modelCode);
                if (modelinfoObj != null)
                {
                    modelinfo = (ModelInfo)modelinfoObj;
                    LogHelper.WriteLog("从缓存中获取 ModelInfo");
                }
                else
                {
                    LogHelper.WriteLog("从数据库获取　通过模块代码查询系统代码 开始 modelCode=" + modelCode);
                    
                    modelinfo = FlowBLL.GetSysCodeByModelCode(con, modelCode);//对数据库操作
                    if (modelinfo != null)
                    {
                        CacheProvider.RemoveCache(modelCode);
                        CacheProvider.Add(modelCode, modelinfo);
                    }
                    else
                    {
                        LogHelper.WriteLog("从数据库获取　通过模块代码查询系统代码 结果为空"); 
                    }
                    LogHelper.WriteLog("从数据库获取　通过模块代码查询系统代码 结束 modelCode=" + modelCode); 
                }
                #endregion                
                this.ModelCode = modelCode;
                if (modelinfo != null)
                {
                    this.ModelName = modelinfo.ModelName;
                    this.SysCode = modelinfo.SysCode;
                }
                #region FlowUserInfo加入缓存
                object FlowUserInfoObj = CacheProvider.GetCache(userid);
                if (FlowUserInfoObj != null)
                {
                    Users = (FlowUserInfo[])FlowUserInfoObj;
                    LogHelper.WriteLog("从缓存中获取 FlowUserInfo");
                }
                else
                {
                    LogHelper.WriteLog("创建服务 PermissionServiceClient 开始");
                    PermissionServiceClient WcfPermissionService = new PermissionServiceClient();
                    LogHelper.WriteLog("从数据库获取 FlowUserInfo 开始 userid=" + userid);
                    Users = WcfPermissionService.GetFlowUserByUserID(userid);//新的接口
                    if (Users != null)
                    {
                        CacheProvider.RemoveCache(userid);
                        CacheProvider.Add(userid, Users);
                    }
                    LogHelper.WriteLog("从数据库获取 FlowUserInfo  结束 userid=" + userid); 
                }
                #endregion
               
                foreach (var user in Users)
                {
                    if (user.CompayID == companyid)
                    {
                        names += "公司ID=" + user.CompayID+"\r\n";
                        names += "部门ID="  + user.DepartmentID+"\r\n";
                        names += "岗位ID="  + user.PostID+"\r\n";
                        names += "用户ID="  + user.UserID+"\r\n";

                        names += "公司名称="  + user.CompayName+"\r\n";
                        names += "部门名称="  +  user.DepartmentName+"\r\n";
                        names += "岗位名称=" +  user.PostName+"\r\n";
                        names += "用户名称=" + user.EmployeeName+"\r\n";

                        #region 用户基本信息
                        this.CompayID = user.CompayID;
                        this.DepartmentID = user.DepartmentID;
                        this.PostID = user.PostID;
                        this.UserID = user.UserID;

                        this.CompayName = user.CompayName;
                        this.DepartmentName = user.DepartmentName;
                        this.PostName = user.PostName;
                        this.UserName = user.EmployeeName;
                        this.Roles = new List<T_SYS_ROLE>();
                        foreach (var role in user.Roles)
                        {
                            if (role != null)
                            {
                                names += "角色ID=" + role.ROLEID + "\r\n";
                                names += "角色名称=" + role.ROLENAME + "\r\n\r\n";
                            }
                            
                            this.Roles.Add(role);
                        }
                        #endregion
                    }
                }
                LogHelper.WriteLog("流程单据所属人身份:\r\n" + names); 
            }
            catch (Exception e)
            {
                ErrorMsg += "获取当前提交,审核单据时的用户信息出错:names=" + names + "异常信息:\r\n" + e.ToString()+"\r\n";
                LogHelper.WriteLog("获取当前提交,审核单据时的用户信息出错:names=" + names + "异常信息:\r\n" + e.ToString()); 
            }
 
        }
        public FlowUser()
        {
            
        }
        #region 属性
        /// <summary>
        /// 只是用来记录程序的跟踪信息,没有影响流程和业务
        /// </summary>
        public  string TrackingMessage{ get; set; }
        /// <summary>
        /// FormID(guid)
        /// </summary>
        public string FormID { get; set; }
        /// <summary>
        /// 当前单据绑定的流程代码(guid)
        /// </summary>
        public string FlowCode { get; set; }
        /// <summary>
        /// 当前单据绑定的流程名称
        /// </summary>
        public string FlowName { get; set; }
        /// <summary>
        /// 用户姓名ID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 所属公司ID
        /// </summary>
        public string CompayID { get; set; }
        /// <summary>
        /// 所属公司名称
        /// </summary>
        public string CompayName { get; set; }
        /// <summary>
        /// 所属部门ID
        /// </summary>
        public string DepartmentID { get; set; }
        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 所属岗位ID
        /// </summary>
        public string PostID { get; set; }
        /// <summary>
        /// 所属岗位名称
        /// </summary>
        public string PostName { get; set; }
        /// <summary>
        /// 工作流实例ID
        /// </summary>
        public DataSet InstanceState { get; set; }
        /// <summary>
        /// 所包含的角色：一个人在同一家公司可以有多个角色
        /// </summary>
        public List<T_SYS_ROLE> Roles { get; set; }
        /// <summary>
        /// 是否是所在的部门的负责人
        /// </summary>
        public string IsHead { get; set; }
        /// <summary>
        /// 是否是所在的岗位的直接上级
        /// </summary>
        public string IsSuperior { get; set; }
        /// <summary>
        /// 提交或审核时模块代码
        /// </summary>
        public string ModelCode { get; set; }
        /// <summary>
        /// 提交或审核时模块名称
        /// </summary>
        public string ModelName { get; set; }
        /// <summary>
        /// 模块所属的系统代码
        /// </summary>
        public string SysCode { get; set; }
        /// <summary>
        /// 当前审核人审核完后,下一个审核人的ID
        /// </summary>
        public string NextEditUserID { get; set; }
        /// <summary>
        /// 当前审核人审核完后,下一个审核人的姓名
        /// </summary>
        public string NextEditUserName { get; set; }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        public string ErrorMsg { get; set; }
        
        #endregion
        /// <summary>
        /// 如果当前审核人的所属的公司ID等参数公司ID,则返回所在的公司名称,否则返回""
        /// </summary>
        /// <param name="companyID">参数公司ID</param>
        /// <returns></returns>
        public string GetCompanyName(string companyID)
        {
            if (companyID == this.CompayID)
            {
                return this.CompayName;
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 如果当前审核人的所属的部门ID等参数公司ID,则返回所在的部门名称,否则返回""
        /// </summary>
        /// <param name="companyID">参数部门ID</param>
        /// <returns></returns>
        public string GetDepartmentName(string departmentID)
        {
            if (departmentID == this.DepartmentID)
            {
                return this.DepartmentName;
            }
            else
            {
                return "";
            }
        }
    }
}
