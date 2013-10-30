using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using System.Data.Objects;
using System.Configuration;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using System.Data.Objects.DataClasses;
using PermissionWS = SMT.SaaS.BLLCommonServices.PermissionWS;
using FlowServiceWS = SMT.SaaS.BLLCommonServices.FlowWFService;
using System.ServiceModel.Description;
using System.Reflection;
using System.Resources;
using BLLCommonServices = SMT.SaaS.BLLCommonServices;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.SaaS.BLLCommonServices.FlowWFService;
using SMT.SaaS.BLLCommonServices.BllCommonUserPermissionWS;
using SMT.Foundation.Log;
using SMT.HRM.CustomModel;
using SMT.SaaS.MobileXml;
using SMT.SaaS.BLLCommonServices.PublicInterfaceWS;

namespace SMT.HRM.BLL
{
    public class BaseBll<TEntity> : IDisposable where TEntity : class
    {
        public SubmitData AuditSubmitData { set; get; }
        public CommDal<TEntity> dal;
        public static string qualifiedEntitySetName = ConfigurationManager.AppSettings["DBContextName"] + ".";
        protected static PermissionWS.PermissionServiceClient PermClient;
        protected static BllCommonPermissionServicesClient BllPermClient;
        protected static ServiceClient flowSeriviceClient;
        protected static PublicServiceClient PublicInterfaceClient;
        
        public BaseBll()
        {
            if (!string.IsNullOrEmpty(CommonUserInfo.EmployeeID))
            {
                //dal.CurrentUserID = CommonUserInfo.EmployeeID;
            }
            if (PermClient == null)
            {
                PermClient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();
            }
            if (BllPermClient == null)
            {
                BllPermClient = new SMT.SaaS.BLLCommonServices.BllCommonUserPermissionWS.BllCommonPermissionServicesClient();
            }
            if (flowSeriviceClient==null)
            {
                flowSeriviceClient = new FlowServiceWS.ServiceClient();
            }
            //if (PublicInterfaceClient == null)
            //{
            //    PublicInterfaceClient = new PublicServiceClient();
            //}
            if (dal == null)
            {
                dal = new CommDal<TEntity>();
                dal.LogNewDal(typeof(TEntity).Name);

            }
        }

        public bool Add(TEntity entity)
        {
            try
            {
                int i = dal.Add(entity);
                if (i >= 1)
                {
                    SaveMyRecord(entity);
                    MvcCacheClear(entity,"Add");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="createuserID"></param>
        /// <returns></returns>
        public bool Add(TEntity entity, string createuserID)
        {
            try
            {
                int i = dal.Add(entity);
                
                if (i >= 1)
                {
                    SaveMyRecord(entity, createuserID);
                    MvcCacheClear(entity, "Add");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("添加数据出错："+ entity.GetType().Name);
                throw (ex);
            }
        }




        public bool Delete(TEntity entity)
        {
            try
            {
                dal.Delete(entity);
                DeleteMyRecord(entity);
                MvcCacheClear(entity, "Delete");
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        public int Update(TEntity entity)
        {
            int i = 0;
            try
            {
                i =dal.Update(entity);
                if (i > 0)
                {
                    SaveMyRecord(entity);
                    MvcCacheClear(entity, "Modify");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="createuserID"></param>
        public void Update(TEntity entity, string createuserID)
        {
            try
            {
                dal.Update(entity);
                SaveMyRecord(entity, createuserID);
                MvcCacheClear(entity, "Modify");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// 通知MVC缓存更新缓存的实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="action"></param>
        public void MvcCacheClear(TEntity entity, string action)
        {
            string strModelCode = entity.GetType().Name;
            BLLCommonServices.MVCCacheSV.EntityAction act;
            switch (action)
            {
                case "Add": act = BLLCommonServices.MVCCacheSV.EntityAction.Add; break;
                case "Modify": act = BLLCommonServices.MVCCacheSV.EntityAction.Modify; break;
                case "Delete": act = BLLCommonServices.MVCCacheSV.EntityAction.Delete; break;
                default: act = BLLCommonServices.MVCCacheSV.EntityAction.None; break;
            }
            if (entity is System.Data.Objects.DataClasses.EntityObject)
            {
                System.Data.Objects.DataClasses.EntityObject ent = entity as System.Data.Objects.DataClasses.EntityObject;
                string strFormId = entity.GetType().GetProperties().FirstOrDefault().GetValue(entity, null).ToString();
                if (strFormId != "" || strFormId != null)
                {
                    BLLCommonServices.Utility.MvcCacheClearAsync(strModelCode, strFormId, act);
                }
            }
        }

        /// <summary>
        /// 将指定的单据记录存储到我的单据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string SaveMyRecord(TEntity entity)
        {
            string strTemp = string.Empty;
            try
            {
                //SMT.Foundation.Log.Tracer.Debug("开始调用我的单据" + entity.GetType().Name);
                BLLCommonServices.Utility.SubmitMyRecord<TEntity>(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("调用我的单据出现错误"+ex.ToString());
                strTemp = ex.ToString();
            }
            return strTemp;
        }

        /// <summary>
        /// 将指定的单据记录存储到我的单据（将OWNERID转成CREATEUSERID）
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="createuserID">创建人ID</param>
        /// <returns></returns>
        public string SaveMyRecord(TEntity entity, string createuserID)
        {
            string strTemp = string.Empty;
            try
            {
                BLLCommonServices.Utility.SubmitMyRecord<TEntity>(entity, "0", createuserID);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("调用我的单据出现错误" + ex.ToString());
                strTemp = ex.ToString();
            }
            return strTemp;
        }

        /// <summary>
        /// 将指定的单据记录从我的单据中删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string DeleteMyRecord(TEntity entity)
        {
            string strTemp = string.Empty;
            try
            {
                BLLCommonServices.Utility.RemoveMyRecord<TEntity>(entity);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("删除我的单据出现错误" + ex.ToString());
                strTemp = ex.ToString();
            }
            return strTemp;
        }

        /// <summary>
        /// 关闭引擎异常消息提醒
        /// </summary>
        /// <param name="strCloseSignInIds">单据主键ID集合</param>
        /// <param name="strModelCode">模块实体名</param>
        /// <param name="strEmployeeId">接受消息的员工ID</param>
        public bool CloseAttendAbnormAlarmMsg(List<string> strCloseSignInIds, string strModelCode, string strEmployeeID)
        {
            try
            {
                SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient clientEngine
                    = new BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient();
                clientEngine.CloseDoTask(strCloseSignInIds.ToArray(), strModelCode, strEmployeeID);

                string strid=string.Empty;
                foreach(var q in strCloseSignInIds)
                {
                    strid = q + "," + strid;
                }
                Tracer.Debug("关闭引擎，接受参数：strModelCode：" + strModelCode + ", strEmployeeId:"
                   + strEmployeeID + " formids:" + strid);
                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("关闭引擎异常消息提醒发生错误，接受参数：strModelCode：" + strModelCode + ", strEmployeeId:"
                    + strEmployeeID + "。出错详细信息如下：" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 关闭引擎异常消息提醒
        /// </summary>
        /// <param name="strModelCode">模块实体名</param>
        /// <param name="strEmployeeId">接受消息的员工Id</param>
        private bool CloseAttendAbnormAlarmMsg(string strModelCode, string strEmployeeId)
        {
            try
            {
                SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient clientEngine
                    = new SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient();
                clientEngine.ModelMsgClose(strModelCode, strEmployeeId);

                Tracer.Debug("关闭引擎，接受参数：strModelCode：" + strModelCode + ", strEmployeeId:"
                   + strEmployeeId );
                return true;
            }
            catch (Exception ex)
            {
                Tracer.Debug("关闭引擎异常消息提醒发生错误，接受参数：strModelCode：" + strModelCode + ", strEmployeeId:"
                    + strEmployeeId + "。出错详细信息如下：" + ex.ToString());
                return false;
            }
        }

        public IQueryable<TEntity> GetTable()
        {
            //return ((ObjectQuery<TEntity>)dal.GetTable(typeof(TEntity).Name)).AsQueryable();
            return dal.GetTable<TEntity>();
        }

        public object CustomerQuery(string Sql)
        {
            return dal.CustomerQuery(Sql);

        }

        public void InitCustomResult(ref CustomResult result)
        {
            result = new CustomResult();
            result.ResultValue = 1;
        }

        public void SetCustomResult(ref CustomResult result, string message)
        {
            result.ResultValue = 0;
            result.ResultMessage = message;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<TEntity> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            #region 实现动态查询的两个例子
            //方式一:用Dynamic Linq 查询
            //string[] paras = new string[2];
            //paras[0] = "EmpCode3";
            //paras[1] = "EmpName5";
            //IQueryable<T_HR_EMPLOYEE> ents = dal.GetTable().Where("EMPLOYEECODE==@0 or EMPLOYEENAME==@1 ", paras).OrderBy(sort).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            //return ents;


            //方式二:用原始的DynamicQueryable查询
            //IQueryable<T_HR_EMPLOYEE> ents = dal.GetTable();
            //ParameterExpression para = Expression.Parameter(typeof(T_HR_EMPLOYEE), "emp");

            ////c.City=="London"
            //Expression left = Expression.Property(para, typeof(T_HR_EMPLOYEE).GetProperty("EMPLOYEECODE"));
            //Expression right = Expression.Constant("22222");
            //Expression filter = Expression.Equal(left, right);

            //Expression pred = Expression.Lambda(filter, para);
            ////Where(c=>c.City=="London")
            //Expression expr = Expression.Call(typeof(Queryable), "Where",
            //    new Type[] { typeof(T_HR_EMPLOYEE) },
            //    Expression.Constant(ents), pred);

            ////OrderBy(ContactName => ContactName)
            //MethodCallExpression orderByCallExpression = Expression.Call(
            //typeof(Queryable), "OrderBy",
            //new Type[] { typeof(T_HR_EMPLOYEE), typeof(string) },
            //expr,
            //Expression.Lambda(Expression.Property
            //(para, "EMPLOYEENAME"), para));

            ////生成动态查询
            //IQueryable<T_HR_EMPLOYEE> query = ents.AsQueryable()
            //    .Provider.CreateQuery<T_HR_EMPLOYEE>(orderByCallExpression);


            //int count = query.Count();
            //pageCount = count/pageSize;
            //int tmp = count % pageSize;
            //pageCount = pageCount + (tmp > 0 ? 1 : 0);

            //return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            #endregion

            IQueryable<TEntity> ents = dal.GetTable();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<TEntity>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        private int GetCustomPerms(string menuCode, T_HR_EMPLOYEE emp)
        {
            int perm = 99;
            //过滤自定义的权限
            if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
            {
                emp.T_HR_EMPLOYEEPOST.Load();
            }
            foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
            {
                if (!ep.T_HR_POSTReference.IsLoaded)
                    ep.T_HR_POSTReference.Load();
                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_ENTITYMENUCUSTOMPERM[] custPerms;
                //查看有没有岗位的特别权限
                custPerms = PermClient.GetCustomPostMenuPerms(menuCode, ep.T_HR_POST.POSTID);
                if (custPerms != null && custPerms.Count() > 0)
                    perm = Convert.ToInt32(AssignObjectType.Post);
                //查看有没有部门的特别权限
                custPerms = PermClient.GetCustomDepartMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                if (custPerms != null && custPerms.Count() > 0)
                    perm = Convert.ToInt32(AssignObjectType.Department);
                //查看有没有公司的特别权限
                custPerms = PermClient.GetCustomCompanyMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                if (custPerms != null && custPerms.Count() > 0)
                    perm = Convert.ToInt32(AssignObjectType.Company);
            }
            return perm;
        }
        public void SetOrganizationFilter(ref string filterString, ref List<object> queryParas, string employeeID, string entityName)
        {
            SetOrganizationFilter(ref filterString, ref queryParas, employeeID, entityName, "3");
        }

        List<string> LstDepartments = new List<string>();//部门ID集合
        public void SetOrganizationFilter(ref string filterString, ref List<object> queryParas, string employeeID, string entityName, string perm)
        {
            try
            {
                //注意，用户总是能看到自己创建的记录
                //获取正常的角色用户权限            

                int maxPerm = -1;
                //获取用户
                List<string> DepartmentPermissions = new List<string>();//具有权限的部门ID集合
                foreach (var op in PermClient.Endpoint.Contract.Operations)
                {
                    var dataContractBehavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)] as DataContractSerializerOperationBehavior;

                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = Int32.MaxValue; //int.MaxValue;
                    }
                }

                string OwnerCompanyIDs = "";
                string OwnerDepartmentIDs = "";
                string OwnerPositionIDs = "";

                //if (string.IsNullOrEmpty(filterString))
                //{
                //    filterString += " ( ";
                //}
                //else
                //{
                //    filterString += " and ( ";
                //}
                //bool HasPermsValue = true;//是否有传入的perm的权限。
                SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_USER user = PermClient.GetUserByEmployeeID(employeeID);
                SMT.SaaS.BLLCommonServices.BllCommonUserPermissionWS.V_BllCommonUserPermission[] perms = null;
                if (perm == "3")
                {
                    perms = BllPermClient.GetUserMenuPermsByUserPermissionBllCommon(entityName, user.SYSUSERID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);
                }
                else
                {
                    perms = BllPermClient.GetUserMenuPermsByUserPermissionBllCommonAddPermissionValue(entityName, user.SYSUSERID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs, perm);
                }
                #region 没权限的情况

                if ((perms == null || perms.Count() <= 0) && string.IsNullOrEmpty(OwnerCompanyIDs) && string.IsNullOrEmpty(OwnerDepartmentIDs) && string.IsNullOrEmpty(OwnerPositionIDs))
                {

                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);
                    filterString += " )";
                    return;
                }
                #endregion
                var employe = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                              where ent.EMPLOYEEID == user.EMPLOYEEID
                              select ent;
                T_HR_EMPLOYEE emp = employe.FirstOrDefault();
                if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
                {
                    emp.T_HR_EMPLOYEEPOST.Load();
                }

                string tmpfilter = string.Empty;

                if (perms != null && perms.Count() > 0)
                {
                    #region 代码修改
                    if (perms.Count() > 0)
                    {
                        //2012-5-9修改，如果不是查看的值传入则
                        //返回相应的权限值
                        string StrPerm = "";
                        if (perm != "3" && !string.IsNullOrEmpty(perm))
                        {
                            StrPerm = perm;
                        }
                        else
                        {
                            StrPerm = "3";
                        }
                        var tmpperms = perms.Where(p => p.PermissionDataRange == StrPerm).ToList();
                        //获取大部门的小部门
                        foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
                        {
                            if (!ep.T_HR_POSTReference.IsLoaded)
                            {
                                ep.T_HR_POSTReference.Load();
                            }
                            if (!ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded)
                            {
                                ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                            }
                            if (ep.T_HR_POST.T_HR_DEPARTMENT == null) continue;
                            string strDepartmentId = ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                            var ownerdeparts = from ent in LstDepartments
                                               where ent == strDepartmentId
                                               select ent;
                            if (ownerdeparts.Count() == 0)
                                LstDepartments.Add(strDepartmentId);
                        }

                        if (tmpperms.Count > 0)
                        {
                            if (emp.T_HR_EMPLOYEEPOST != null)
                            {
                                if (emp.T_HR_EMPLOYEEPOST.Count() > 0)
                                {
                                    //如果字符串为空则加上(  不为空 则为  and  (
                                    if (string.IsNullOrEmpty(filterString))
                                    {
                                        filterString += " ( ";
                                    }
                                    else
                                    {

                                        filterString += " and (";
                                    }
                                    int i = 0;

                                    foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
                                    {

                                        if (i > 0)
                                        {
                                            filterString += " OR ";
                                        }
                                        if(ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();
                                        var tmpents = tmpperms.Where(p => p.OwnerCompanyID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                                        if (tmpents.Count() > 0)
                                        {
                                            maxPerm = tmpents.Min(p => Convert.ToInt32(p.RoleMenuPermissionValue));
                                        }
                                        else
                                        {
                                            maxPerm = -1;
                                        }
                                        #region 公司级别
                                        var entcompanys = tmpperms.Where(p => p.OwnerCompanyID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Company))
                                        {
                                            if (entcompanys.Count() > 0)
                                            {
                                                filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();
                                                queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                                            }
                                        }
                                        #endregion

                                        #region 部门级别、大部门可以看到小部门的信息
                                        var entdepartments = tmpperms.Where(p => p.OwnerDepartmentID == ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);

                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Department))
                                        {

                                            LstDepartments.Clear();
                                            if (!ep.T_HR_POSTReference.IsLoaded)
                                            {
                                                ep.T_HR_POSTReference.Load();
                                            }
                                            if (!ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded)
                                            {
                                                ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();
                                            }
                                            LstDepartments.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                                            if (LstDepartments.Count() > 0)
                                            {
                                                DepartmentPermissions = GetCacheEmployeeForDepartmentIDsForPermission(LstDepartments.ToArray(), ep.T_HR_EMPLOYEE.EMPLOYEEID);
                                            }

                                            if (DepartmentPermissions.Count() > 0)
                                            {
                                                int kk = 0;
                                                foreach (var epdepartid in DepartmentPermissions)
                                                {
                                                    if (kk > 0)
                                                    {
                                                        filterString += " OR ";
                                                    }
                                                    filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();
                                                    queryParas.Add(epdepartid);
                                                    kk++;

                                                }

                                            }
                                        }
                                        #endregion

                                        #region 岗位级别信息

                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Post))
                                        {
                                            filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();
                                            queryParas.Add(ep.T_HR_POST.POSTID);
                                        }
                                        #endregion

                                        #region 查看个人信息
                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Employee) || maxPerm == -1)
                                        {
                                            filterString += " ( OWNERID==@" + queryParas.Count().ToString();
                                            queryParas.Add(employeeID);
                                            filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                                            queryParas.Add(employeeID);
                                        }
                                        #endregion

                                        i++;
                                    }

                                    //filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                                    //queryParas.Add(employeeID);
                                    //filterString += " OR CREATEUSERID==@" + queryParas.Count().ToString()+")";
                                    filterString += " OR CREATEUSERID==@" + queryParas.Count().ToString();
                                    queryParas.Add(employeeID);
                                }
                            }
                        }
                        else
                        {
                            //考虑没获取权限的情况下有自定义权限
                            //HasPermsValue = false;//没有权限
                            if (string.IsNullOrEmpty(filterString))
                            {
                                filterString += " ( ";
                            }
                            else
                            {

                                filterString += " and (";
                            }
                        }
                    }
                    #endregion

                }

                //看整个集团的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Organize))
                {
                    return;
                }


                //自定义权限存在公司集合
                if (!string.IsNullOrEmpty(OwnerCompanyIDs))
                {
                    //tmpfilter += " OR ";
                    tmpfilter += " (";
                    string[] ArrCompany = OwnerCompanyIDs.Split(',');
                    for (int i = 0; i < ArrCompany.Length; i++)
                    {

                        if (!string.IsNullOrEmpty(ArrCompany[i].Trim()))
                        {
                            if (i > 0) tmpfilter += " OR ";
                            tmpfilter += "OWNERCOMPANYID==@" + queryParas.Count().ToString();
                            queryParas.Add(ArrCompany[i]);
                        }
                    }
                    tmpfilter += ") ";
                }
                //看自定义权限中的部门信息
                if (!string.IsNullOrEmpty(OwnerDepartmentIDs))
                {
                    if (!string.IsNullOrEmpty(tmpfilter))
                    {
                        tmpfilter += " OR ";
                    }
                    tmpfilter += " (";
                    string[] ArrDepartment = OwnerDepartmentIDs.Split(',');
                    for (int i = 0; i < ArrDepartment.Length; i++)
                    {
                        if (i > 0) tmpfilter += " OR ";
                        tmpfilter += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();
                        queryParas.Add(ArrDepartment[i]);
                    }
                    tmpfilter += ") ";
                }
                //看自定义权限中岗位的信息
                if (!string.IsNullOrEmpty(OwnerPositionIDs))
                {
                    if (!string.IsNullOrEmpty(tmpfilter))
                    {
                        tmpfilter += " OR ";
                    }
                    tmpfilter += " (";
                    string[] ArrPosition = OwnerPositionIDs.Split(',');
                    for (int i = 0; i < ArrPosition.Length; i++)
                    {
                        if (i > 0) tmpfilter += " OR ";
                        tmpfilter += "OWNERPOSTID==@" + queryParas.Count().ToString();
                        queryParas.Add(ArrPosition[i]);
                    }
                    tmpfilter += ") ";
                }
                if (!string.IsNullOrEmpty(tmpfilter))
                {

                    if (filterString.Length >= 3 && filterString.Length >= 6)
                    {
                        filterString = filterString + " or (" + tmpfilter + ")";
                    }
                    else
                    {
                        filterString += " (" + tmpfilter + ")";
                    }
                }
                if ((perms != null && perms.Count() > 0) || string.IsNullOrEmpty(tmpfilter))
                {
                    filterString += " ) ";
                }
                //SMT.Foundation.Log.Tracer.Debug("权限条件为: " + filterString);
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString()+ex.StackTrace);
                throw (ex);
            }
        }



        private List<string> GetCacheEmployeeForDepartmentIDs(string[] strCompanys, string employeeID)
        {
            string keyString = "GetCacheEmployeeForDepartmentIDs" + employeeID;
            if (CacheManager.GetCache(keyString) == null)
            {
                string companyFilter = "";
                for (int i = 0; i < strCompanys.Count(); i++)
                {
                    if (string.IsNullOrEmpty(companyFilter))
                    {
                        companyFilter += " FATHERID==@" + i.ToString();
                    }
                    else
                    {
                        companyFilter += " or FATHERID==@" + i.ToString();
                    }
                }
                var list = dal.GetObjects<T_HR_COMPANY>().AsQueryable().Where(companyFilter, strCompanys);
                foreach (var item in list)
                {
                    LstDepartments.Add(item.COMPANYID);
                }
                if (LstDepartments.Count() == 0)
                {
                    for (int i = 0; i < strCompanys.Count(); i++)
                    {
                        LstDepartments.Add(strCompanys[i]);
                    }
                }
                CacheManager.AddCache(keyString, LstDepartments);
            }
            else
            {
                LstDepartments = (List<string>)CacheManager.GetCache(keyString);
            }
            return LstDepartments;
        }


        private List<string> GetCacheEmployeeForDepartmentIDsForPermission(string[] strDepartments, string employeeID)
        {

            string keyString = "GetCacheEmployeeForDepartmentIDsForPermission" + strDepartments[0] + employeeID;
            if (CacheManager.GetCache(keyString) == null)
            {
                string companyFilter = "";
                for (int i = 0; i < strDepartments.Count(); i++)
                {
                    if (string.IsNullOrEmpty(companyFilter))
                    {
                        companyFilter += " FATHERID==@" + i.ToString();
                    }
                    else
                    {
                        companyFilter += " or FATHERID==@" + i.ToString();
                    }
                }
                var list = dal.GetObjects<T_HR_DEPARTMENT>().AsQueryable().Where(companyFilter, strDepartments);

                foreach (var item in list)
                {
                    LstDepartments.Add(item.DEPARTMENTID);
                }
                if (LstDepartments.Count() == 0)
                {
                    for (int i = 0; i < strDepartments.Count(); i++)
                    {
                        LstDepartments.Add(strDepartments[i]);
                    }
                }
                CacheManager.AddCache(keyString, LstDepartments);
            }
            return LstDepartments;
        }


        /// <summary>
        /// 设置查询条件集合，加入流程控制部分的条件参数(主要用于审核人员查看当前指派给自己的待审核记录)
        /// </summary>
        /// <param name="strPrimaryKey">当前查询表单的主键</param>
        /// <param name="ModelCode">当前查询表单对应模块ID</param>
        /// <param name="strUserID">当前查询人的员工ID</param>
        /// <param name="strCheckState">审核状态</param>
        /// <param name="filterString">查询条件</param>
        /// <param name="queryParas">查询参数</param>
        public void SetFilterWithflow(string strPrimaryKey, string ModelCode, string strUserID, ref string strCheckState, ref string filterString, ref List<object> queryParas)
        {
            try
            {
                if (strCheckState == Convert.ToInt32(Common.CheckStates.All).ToString())
                {
                    strCheckState = string.Empty;
                    return;
                }

                if (strCheckState != Convert.ToInt32(Common.CheckStates.WaittingApproval).ToString())
                {
                    return;
                }

                strCheckState = Convert.ToInt32(Common.CheckStates.Approving).ToString();   //待审核的转化为审核中

                SMT.SaaS.BLLCommonServices.FlowWFService.ServiceClient clientFlow = new SMT.SaaS.BLLCommonServices.FlowWFService.ServiceClient();
                SMT.SaaS.BLLCommonServices.FlowWFService.FLOW_FLOWRECORDDETAIL_T[] flowList = clientFlow.GetFlowInfo("", "", strCheckState, "0", ModelCode, "", strUserID);

                if (flowList == null)
                {
                    return;
                }
                if (flowList.Length == 0)
                {
                    return;
                }
                StringBuilder strIds = new StringBuilder();
                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND ";
                }
                filterString += "( ";
                foreach (SMT.SaaS.BLLCommonServices.FlowWFService.FLOW_FLOWRECORDDETAIL_T item in flowList)
                {
                    filterString += " " + strPrimaryKey + "=@" + queryParas.Count() + " or";
                    queryParas.Add(item.FLOW_FLOWRECORDMASTER_T.FORMID);
                }
                filterString = filterString.Substring(0, filterString.Length - 2);
                filterString += " )";
                //int iIndex = 0;
                //if (!string.IsNullOrEmpty(strIds.ToString()))
                //{
                //    if (!string.IsNullOrEmpty(filterString))
                //    {
                //        filterString += " AND";
                //    }
                //    if (queryParas.Count() > 0)
                //    {
                //        iIndex = queryParas.Count();
                //    }
                //    filterString += " " + strPrimaryKey + ".Contains(@" + iIndex.ToString() + ")";
                //    queryParas.Add(strIds.ToString());
                //}
            }
            catch 
            {
            }
        }

        public static bool CheckUser(string strUserId, string strPwd)
        {
            bool bTemp = false;
            try
            {
                SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_USER entUserLogin = PermClient.UserLogin(strUserId, strPwd);
                if (entUserLogin != null)
                {
                    return true;
                }
            }
            catch
            {
                bTemp = false;
            }
            return bTemp;
        }

        #region 提交审核

        #region 每个业务层单据构造
        //public void SetFlowRecordEntity(Flow_FlowRecord_T entity, string POSTLEVEL)
        //{
        //    Dictionary<string, string> para = new Dictionary<string, string>();
        //    para.Add("POSTLEVEL", POSTLEVEL);
        //    entity.SystemCode = "HR";

        //    //Dictionary<string, string> para2 = new Dictionary<string, string>();
        //    //para2.Add("COMPANYTYPE", (cbxCompanyType.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCompanyType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
        //    //para2.Add("COUNTYTYPE", (cbxCountry.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCountry.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
        //    //para2.Add("COMPANYLEVEL", (cbxCompanyLevel.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCompanyLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
        //    //para2.Add("CITY", (cbxCity.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCity.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
        //    //para2.Add("T_HR_COMPANY2Reference", Company.T_HR_COMPANY2 == null ? "" : Company.T_HR_COMPANY2.COMPANYID + "#" + Company.T_HR_COMPANY2.CNAME);

        //    entity.SystemCode = "HR";
        //    string strXmlObjectSource = string.Empty;
        //    if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
        //        strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, Company);
        //    //strXmlObjectSource = Utility.ObjListToXml<T_HR_COMPANY>(Company, para, "HR");
        //    //  strXmlObjectSource = Utility.ObjListToXml<T_HR_COMPANY>(Company, para, "HR", para2, null);

        //    Utility.SetAuditEntity(entity, "T_HR_COMPANY", Company.COMPANYID, strXmlObjectSource);
        //}

        //private string GetXmlString(string StrSource, object Info,
        //    string POSTLEVEL
        //        ,string EMPLOYEENAME
        //            )
        //{
        //    //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
        //    decimal? stateValue = Convert.ToDecimal("1");
        //    string checkState = string.Empty;
        //    string checkStateDict
        //        = PermClient.GetDictionaryByCategoryArray(new string[]{"CHECKSTATE"}).Where(p => p.DICTIONARYVALUE == stateValue).FirstOrDefault().DICTIONARYNAME;
        //    checkState = checkStateDict == null ? "" : checkStateDict;


        //    decimal? postlevelValue = Convert.ToDecimal(POSTLEVEL);
        //    string postLevelName = string.Empty;
        //    string postLevelDict
        //         = PermClient.GetDictionaryByCategoryArray(new string[] { "CHECKSTATE" }).Where(p => p.DICTIONARYVALUE == stateValue).FirstOrDefault().DICTIONARYNAME;
        //    postLevelName = postLevelDict == null ? "" : postLevelDict;

        //    //decimal? payCatogryValue = Convert.ToDecimal(Info.PAYCATEGORY);
        //    //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY PayCatogryDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "PAYCATEGORY" && s.DICTIONARYVALUE == payCatogryValue).FirstOrDefault();

        //    decimal? overTimeValue = Convert.ToDecimal(Info.OVERTIMECATE);
           
        //    SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
        //    List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
        //    AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "CHECKSTATE", "1", checkState));
        //    AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "POSTLEVEL", POSTLEVEL, postLevelName));
        //    AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "EMPLOYEENAME", Info.EMPLOYEENAME, tbEmpName.Text));
        //    AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "OWNERCOMPANYID", Info.OWNERCOMPANYID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName));
        //    AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName));
        //    AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "OWNERPOSTID", Info.OWNERPOSTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName));
        //    //AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "PAYCATEGORY", Info.PAYCATEGORY, PayCatogryDict == null ? "" : PayCatogryDict.DICTIONARYNAME));
        //    AutoList.Add(basedata("T_HR_OUTAPPLYRECORD", "OVERTIMECATE", Info.OVERTIMECATE, overTimeDict == null ? "" : overTimeDict.DICTIONARYNAME));
        //    //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERCOMPANYID", approvalInfo.OWNERCOMPANYID, StrCompanyName));
        //    //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
        //    //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
        //    //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
        //    //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
        //    //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
        //    //    AutoList.Add(basedata("T_HR_LEFTOFFICE", "LEFTOFFICECATEGORY", LEFTOFFICECATEGORY != null ? LEFTOFFICECATEGORY.DICTIONARYVALUE.ToString() : "0", LEFTOFFICECATEGORY != null ? LEFTOFFICECATEGORY.DICTIONARYNAME : ""));
        //    string a = mx.TableToXml(Info, null, StrSource, AutoList);

        //    return a;
        //}

        public AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }
        #endregion


        public string GetBusinessObject(TEntity entity)
        {
            string BusinessObject = PublicInterfaceClient.GetBusinessObject("HR", entity.GetType().Name);
            return BusinessObject;
        }
        /// <summary>
        /// 提交审核
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="auditobj"></param>
        /// <param name="FormID"></param>
        /// <param name="ModelCode"></param>
        /// <returns></returns>
        public DataResult Audit(object Entity,object auditobj, string FormID,string ModelCode)
        {
            //if (EntityEditor is IAudit)
            //{
            //    ((IAudit)EntityEditor).SetFlowRecordEntity(AuditCtrl.AuditEntity);
            //}

            VirtualAudit auditEntity = auditobj as VirtualAudit;
            //flowSerivice.Open();
            
            string POSTLEVEL=string.Empty;
            string EMPLOYEENAME = string.Empty;
            //string xml = GetAuditXml(fbEntity);
            string StrSource=string.Empty;
            string xml = string.Empty;// GetXmlString(StrSource, Entity, POSTLEVEL, EMPLOYEENAME);
            //Tracer.Debug(xml);

            SubmitData AuditSubmitData = new SubmitData();
            AuditSubmitData.FormID = FormID;
            AuditSubmitData.ModelCode = ModelCode;
            AuditSubmitData.ApprovalUser = new SMT.SaaS.BLLCommonServices.FlowWFService.UserInfo
            {
                CompanyID = auditEntity.CREATECOMPANYID,
                CompanyName = auditEntity.CREATECOMPANYNAME,
                DepartmentID = auditEntity.CREATEDEPARTMENTID,
                DepartmentName = auditEntity.CREATEDEPARTMENTNAME,
                PostID = auditEntity.CREATEPOSTID,
                PostName = auditEntity.CREATEPOSTNAME,
                UserID = auditEntity.CREATEUSERID,
                UserName = auditEntity.CREATEUSERNAME
            };
            AuditSubmitData.ApprovalContent = auditEntity.Content;
            AuditSubmitData.NextStateCode = auditEntity.NextStateCode;
            AuditSubmitData.NextApprovalUser = new SMT.SaaS.BLLCommonServices.FlowWFService.UserInfo
            {
                UserID = auditEntity.OWNERID,
                UserName = auditEntity.OWNERNAME,
                CompanyID = auditEntity.OWNERCOMPANYID,
                CompanyName = auditEntity.OWNERCOMPANYNAME,
                DepartmentID = auditEntity.OWNERDEPARTMENTID,
                DepartmentName = auditEntity.OWNERDEPARTMENTNAME,
                PostID = auditEntity.OWNERPOSTID,
                PostName = auditEntity.OWNERPOSTNAME
            };
            AuditSubmitData.ApprovalResult = (ApprovalResult)auditEntity.Result;
            AuditSubmitData.FlowSelectType = (FlowSelectType)auditEntity.FlowSelectType;

            SubmitFlag AuditSubmitFlag = auditEntity.Op.ToUpper() == "ADD" ? SubmitFlag.New : SubmitFlag.Approval;
            AuditSubmitData.SubmitFlag = AuditSubmitFlag;
            AuditSubmitData.XML = xml;
            //#region 
            //FlowWFService.DataResult ar = new FlowWFService.DataResult();
            //ar.FlowResult = FlowWFService.FlowResult.FAIL;

            //return testResult;
            //#endregion

            DataResult ar = flowSeriviceClient.SubimtFlow(AuditSubmitData);

            if (ar.FlowResult == FlowResult.FAIL)
            {
                string msg = @"流程提交or审核失败. 参数: "
                      + string.Format("\r\n ApprovalUser.CompanyID: {0} ", AuditSubmitData.ApprovalUser.CompanyID)
                      + string.Format("\r\n ApprovalUser.DepartmentID: {0} ", AuditSubmitData.ApprovalUser.DepartmentID)
                      + string.Format("\r\n ApprovalUser.PostID: {0} ", AuditSubmitData.ApprovalUser.PostID)
                      + string.Format("\r\n ApprovalUser.UserName: {0} ", AuditSubmitData.ApprovalUser.UserName)
                      + string.Format("\r\n FormID: {0} ", AuditSubmitData.FormID)
                      + string.Format("\r\n ModelCode: {0} ", AuditSubmitData.ModelCode)
                      + string.Format("\r\n ApprovalContent: {0} ", AuditSubmitData.ApprovalContent)
                      + string.Format("\r\n NextStateCode: {0} ", AuditSubmitData.NextStateCode)
                      + string.Format("\r\n ApprovalResult: {0} ", AuditSubmitData.ApprovalResult)
                      + string.Format("\r\n FlowSelectType: {0} ", AuditSubmitData.FlowSelectType.ToString())
                      + string.Format("\r\n SubmitFlag: {0} ", AuditSubmitData.SubmitFlag.ToString())
                      + string.Format("\r\n NextApprovalUser.UserID: {0} ", AuditSubmitData.NextApprovalUser.UserName)
                      + string.Format("\r\n NextApprovalUser.UserName: {0} ", AuditSubmitData.NextApprovalUser.UserName)
                      + string.Format("\r\n ErrMessage: {0} ", ar.Err);

                Tracer.Debug(msg);
                //Tracer.Debug(xml);
            }
            return ar;
        }
        
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            dal.Dispose();
        }

        #endregion
    }


    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class VirtualAudit : VirtualEntityObject
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public int Result { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Content { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string ModelCode { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string FormID { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string GUID { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string NextStateCode { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string Op { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public int FlowSelectType { get; set; }

    }


    [global::System.Runtime.Serialization.DataContractAttribute(IsReference = true)]
    [global::System.Serializable()]
    public class VirtualEntityObject : System.Data.Objects.DataClasses.EntityObject
    {
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string ID { get; set; }
        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public virtual string Name { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATECOMPANYID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATECOMPANYNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEDEPARTMENTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEDEPARTMENTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEPOSTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEPOSTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEUSERID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string CREATEUSERNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERCOMPANYID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERCOMPANYNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERDEPARTMENTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERDEPARTMENTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERPOSTID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERPOSTNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string OWNERNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string UPDATEUSERID { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public string UPDATEUSERNAME { get; set; }

        [global::System.Runtime.Serialization.DataMemberAttribute()]
        public decimal EDITSTATES { get; set; }

    }
}
