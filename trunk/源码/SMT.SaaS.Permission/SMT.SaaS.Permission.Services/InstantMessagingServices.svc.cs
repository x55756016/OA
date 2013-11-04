/*
 * 文件名：InstantMessagingServices.svc
 * 作  用：即时通讯使用的接口
 * 创建人：刘建兴
 * 创建时间：2011-11-16 15:00
 * 主要包括四个接口 
 * 1 获取所有组织架构
 * 2 登录信息
 * 3 获取员工信息
 * 4 修改员工信息 
 * 
 * 
 *  
 */
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using SMT.SaaS.Permission.BLL;
using SMT.Foundation.Log;
using SMT_System_EFModel;
using System.Collections.Generic;

namespace SMT.SaaS.Permission.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class InstantMessagingServices
    {
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            return;
        }

        #region 获取员工信息
        
        
        /// <summary>
        /// 通过员工登录帐号返回员工信息，格式如下
        /// </summary>
        /// <param name="LoginAccount">登录帐号</param>
        /// <returns>XML格式的字符串</returns>
        /// 
        ///成功 
        ///<Employee EmployeeId="" EmployeeName="" LoginAccount="" Sex="" DepartMentName=" " 
        ///Age="" PostName="" Email=" " Address=" " AddCode="" Mobile="" Tel="" Nation=" " Province=" " City=" " Remark=" " />
        ///</BizRegReq>
        ///失败：
        ///<BizRegReq>
        ///<Error ErrorMsg=" " />
        ///</BizRegReq>
        [OperationContract]
        public string GetEmployeeInfo(string loginAccount)
        {
            using (InstantMessageBll bll = new InstantMessageBll())
            {
                string StrReturn = bll.GetEmployeeInfo(loginAccount);
                return StrReturn;
            }
            
        }
        #endregion

        #region 修改员工信息        
        
        /// <summary>
        /// 修改员工信息 
        /// 先根据帐号获取员工信息，然后再根据员工ID获取员工信息传入接口修改信息
        /// </summary>
        /// <param name="employeeInfo">实体</param>
        /// <returns></returns>
        /// 成功则返回
        /// <BizRegReq>
        /// <Employee EmployeeId="" EmployeeName="" LoginAccount="" Sex="" DepartMentName=" " 
        /// Age="" PostName="" Email=" " Address=" " AddCode="" Mobile="" Tel="" Nation=" " Province=" " City=" " Remark=" " />
        /// </BizRegReq>
        /// 失败返回
        /// <BizRegReq>
        /// <Error ErrorMsg=" " />
        /// </BizRegReq>
        [OperationContract]
        public string UpdateEmployeeInfo(SMT.SaaS.Permission.DAL.views.EmployeeInfo employeeInfo)
        {
            using (InstantMessageBll bll = new InstantMessageBll())
            {
                string StrReturn = bll.UpdateEmployeeInfo(employeeInfo);
                return StrReturn;
            }
        }
        #endregion

        #region 员工登录
        /// <summary>
        /// 员工登录信息
        /// </summary>
        /// <param name="loginAccount">登录帐号</param>
        /// <param name="password">登录密码</param>
        /// <returns>返回XML字符串</returns>
        /// 成功：
        /// <BizRegReq>
        /// <Employee EmployeeId="" EmployeeName="" LoginAccount="" Sex="" DepartMentName=" " 
        /// Age="" PostName="" Email=" " Address=" " AddCode="" Mobile="" Tel="" Nation=" " Province=" " City=" " Remark=" " />
        /// </BizRegReq>
        /// 失败：
        /// <BizRegReq>
        /// <Error ErrorMsg=" " /> 
        /// </BizRegReq>
        [OperationContract]
        public string EmployeeLogin(string loginAccount, string password)
        {
            DateTime dtstart = DateTime.Now;
            try
            {
                using (InstantMessageBll bll = new InstantMessageBll())
                {
                    T_SYS_USER plist = bll.GetUsers(loginAccount);
                    //if (WCFCache.Current[keyString] == null)
                    //{
                    //    plist = bll.GetUsers(loginAccount);
                    //    WCFCache.Current.Insert(keyString, plist, DateTime.Now.AddMinutes(150));
                    //}
                    //else
                    //{
                    //    plist = (T_SYS_USER)WCFCache.Current[keyString];
                    //}
                    if (plist == null)
                    {
                        SMT.Foundation.Log.Tracer.Debug("即时通讯获取用户信息为空");
                        return "用户信息为空";
                    }
                    string StrReturn = bll.EmployeeLogin(loginAccount, password, plist);
                    return StrReturn;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("InstantMessagingServices-EmployeeLogin登录出错:" + ex.ToString());
                return "EmployeeLogin登录失败";
            }
            finally
            {
                DateTime dtend = DateTime.Now;
                TimeSpan ts = dtend - dtstart;
                SMT.Foundation.Log.Tracer.Debug(loginAccount + "即时通讯开始登录：" + dtstart.ToString()
                    + " 即时通讯结束登录: " + dtend.ToString() + " 耗时：" + ts.Seconds.ToString() + "秒!" + ts.Milliseconds.ToString() + "毫秒");

            }
        }
        #endregion

        #region 获取所有组织架构（已挪至HR）
        /// <summary>
        /// 获取组织架构：公司、部门和所有在职员工
        /// </summary>
        /// <param name="companyNum">公司数量</param>
        /// <param name="departNum">部门数量</param>
        /// <param name="employeeNum">员工数量</param>
        /// <returns></returns>
        /// 成功则返回
        /// <BizRegReq >
        /// <CompanyList>
        /// <Company CompanyID="" CompanyName="" ParentID="" />
        /// </CompanyList>
        /// <DepartmentList >
        /// <Department DeptID="" DepartName="" CompanyID="" ParentID="" />
        /// </DepartmentList>
        ///  <EmployeeList>
        ///  <Employee EmployeeID=" " EmployeeName=" " LoginAccount=" " Moblie="" DeptID="" PostName=" "/>
        ///  </EmployeeList>
        ///  </BizRegReq >
        ///  如果数据一致则返回为空
        //[OperationContract]
        //public string GetAllOrganization(int companyNum, int departNum, int employeeNum)
        //{
        //    Tracer.Debug("调用了GetAllOrganization"+System.DateTime.Now);
        //    using (InstantMessageBll bll = new InstantMessageBll())
        //    {
        //        Tracer.Debug("调用了GetAllOrganization Using里面" + System.DateTime.Now);
        //        string StrReturn = bll.GetAllOrganization(companyNum,departNum,employeeNum);
        //        return StrReturn;
        //    }
        //}
        #endregion
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
    }
}
