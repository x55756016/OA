using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using System.Data.Objects;
using SMT_OA_EFModel;
using System.Linq.Dynamic;
using System.Reflection;
using System.Data.Objects.DataClasses;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
//using SMT.SaaS.OA.BLL.PermissionWS;
//using SMT.SaaS.OA.BLL.PersonnelWS;
using System.Configuration;
using SMT.SaaS.BLLCommonServices.BllCommonUserPermissionWS;
using System.Reflection;
using System.Resources;

namespace SMT.SaaS.OA.BLL
{
    public class BaseBll<TEntity> : IDisposable where TEntity : class
    {
        public static string qualifiedEntitySetName = ConfigurationManager.AppSettings["DBContextName"] + ".";
        public  CommDaL<TEntity> dal;
        public static SMT.SaaS.BLLCommonServices.Utility UtilityClass;
        protected static PermissionServiceClient PermClient;
        protected static BllCommonPermissionServicesClient BllPermClient;
        public BaseBll()
        {
            //if (!string.IsNullOrEmpty(CommonUserInfo.EmployeeID))
            //{
            //    //dal.CurrentUserID = CommonUserInfo.EmployeeID;
            //}
            
            if (PermClient == null)
            {
                PermClient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient(); 
            }
            if (BllPermClient == null)
            {
                BllPermClient = new SMT.SaaS.BLLCommonServices.BllCommonUserPermissionWS.BllCommonPermissionServicesClient();
            }
            if (UtilityClass==null)
            {
               UtilityClass = new SMT.SaaS.BLLCommonServices.Utility();
               
            }
            if(dal==null)
            {
                dal = new CommDaL<TEntity>();
                //dal.LogNewDal(typeof(TEntity).Name);
            }
        }

        public bool Add(TEntity entity)
        {
            try
            {
                Utility.RefreshEntity(entity as System.Data.Objects.DataClasses.EntityObject);
                int i = dal.Add(entity);
                if (i >0 )
                {
                    BLLCommonServices.Utility.SubmitMyRecord<TEntity>(entity);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        public bool Delete(TEntity entity)
        {
            try
            {
                bool IsTrue = false;
                Utility.RefreshEntity(entity as System.Data.Objects.DataClasses.EntityObject);
                int i=dal.Delete(entity);
                if (i > 0)
                {
                    IsTrue = true;
                    BLLCommonServices.Utility.RemoveMyRecord<TEntity>(entity);
                }
                return IsTrue;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        public int Update(TEntity entity)
        {
            try
            {
                Utility.RefreshEntity(entity as System.Data.Objects.DataClasses.EntityObject);
                int i=dal.Update(entity);
                if(i >0) BLLCommonServices.Utility.SubmitMyRecord<TEntity>(entity);
                return i;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IQueryable<TEntity> GetTable()
        {
            return dal.GetTable();
        }

        public object CustomerQuery(string Sql)
        {
            return dal.CustomerQuery(Sql);
        }


        #region 事务处理

     
        public void BeginTransaction()
        {
            dal.BeginTransaction();
        }
        public void CommitTransaction()
        {
            dal.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            dal.RollbackTransaction();
        }

        #endregion

        #region 分页

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
        public IQueryable<TEntity> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string entityName)
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


            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, entityName);
            IQueryable<TEntity> ents = dal.GetTable().ToList().AsQueryable();
            //IQueryable<TEntity> ents = dal.GetTable();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            ents = Utility.Pager<TEntity>(ents, pageIndex, pageSize, ref pageCount);
            return ents;
        }


        ///// <summary>
        ///// 根据权限过滤
        ///// </summary>
        ///// <param name="filterString"></param>
        ///// <param name="queryParas"></param>
        ///// <param name="userID"></param>
        ///// <param name="entityName"></param>
        //protected void SetOrganizationFilter(ref string filterString, ref List<object> queryParas, string employeeID, string entityName)
        //{
        //    //获取用户
        //    T_SYS_USER user = permissionclient.GetUserByEmployeeID(employeeID);

        //    //EmployeeBLL bll = new EmployeeBLL();

        //    //T_HR_EMPLOYEE emp = bll.GetEmployeeByID(user.EMPLOYEEID);
        //    V_EMPLOYEEPOST vemp = personnelclient.GetEmployeeDetailByID(user.EMPLOYEEID);
        //    if (vemp == null) return;
        //    T_HR_EMPLOYEE emp = vemp.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE;
            

        //    //获取正常的角色用户权限  

        //    V_Permission[] perms = permissionclient.GetUserMenuPerms(entityName, user.SYSUSERID);
        //    if (perms == null)
        //        return;

        //    //获取查询的权限,值越小，权限越大
        //    int maxPerm = -1;
        //    var permlist = perms.Where(p => p.Permission.PERMISSIONVALUE == "3");
        //    if (permlist.Count() > 0)
        //    {
        //        maxPerm = permlist.Min(p => Convert.ToInt32(p.RoleMenuPermission.DATARANGE));
        //    }
        //    //perms.Where(p => p.Permission.PERMISSIONVALUE == "3").Min(p => Convert.ToInt32(p.RoleMenuPermission.DATARANGE));
        //    //var maxPerm = "2";



        //    //if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
        //    //{
        //    //    emp.T_HR_EMPLOYEEPOST.Load();
        //    //}

        //    //获取自定义权限
        //    int custPerm = GetCustomPerms(entityName, emp);
        //    if (custPerm < maxPerm)
        //        maxPerm = custPerm;

        //    //看整个公司的
        //    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Company))
        //    {
        //        if (!string.IsNullOrEmpty(filterString))
        //            filterString += " AND ";

        //        filterString += " (";
        //        int i = 0;
        //        foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
        //        {

        //            if (i > 0)
        //                filterString += " OR ";

        //            filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();


        //            //暂时先去掉
        //            //if (!ep.T_HR_POSTReference.IsLoaded)
        //            //    ep.T_HR_POSTReference.Load();

        //            //if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
        //            //    ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

        //            //if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
        //            //    ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

        //            queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);

        //            i++;
        //        }
        //        filterString += ") ";
        //    }


        //    //看部门的
        //    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Department))
        //    {
        //        if (!string.IsNullOrEmpty(filterString))
        //            filterString += " AND ";

        //        filterString += " (";
        //        int i = 0;
        //        foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
        //        {

        //            if (i > 0)
        //                filterString += " OR ";

        //            filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();

        //            if (!ep.T_HR_POSTReference.IsLoaded)
        //                ep.T_HR_POSTReference.Load();

        //            if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
        //                ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

        //            queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);

        //            i++;
        //        }
        //        filterString += ") ";
        //    }


        //    //看岗位的
        //    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Post))
        //    {
        //        if (!string.IsNullOrEmpty(filterString))
        //            filterString += " AND ";

        //        filterString += " (";
        //        int i = 0;
        //        foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
        //        {

        //            if (i > 0)
        //                filterString += " OR ";

        //            filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();

        //            if (!ep.T_HR_POSTReference.IsLoaded)
        //                ep.T_HR_POSTReference.Load();

        //            queryParas.Add(ep.T_HR_POST.POSTID);

        //            i++;
        //        }
        //        filterString += ") ";
        //    }

        //    //看员工
        //    if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Employee))
        //    {
        //        if (!string.IsNullOrEmpty(filterString))
        //            filterString += " AND ";

        //        filterString += "OWNERID==@" + queryParas.Count().ToString();
        //        queryParas.Add(employeeID);
        //    }

        //}

        //private int GetCustomPerms(string menuCode, T_HR_EMPLOYEE emp)
        //{
        //    int perm = 99;

        //    return perm;     //暂时未实现

        //    //过滤自定义的权限
        //    //emp =
        //    //if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
        //    //{
        //    //    emp.T_HR_EMPLOYEEPOST.Load();
        //    //}
        //    if (emp != null)
        //    {
        //        foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
        //        {
        //            if (!ep.T_HR_POSTReference.IsLoaded)
        //                ep.T_HR_POSTReference.Load();

        //            if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
        //                ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

        //            if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
        //                ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

        //            T_SYS_ENTITYMENUCUSTOMPERM[] custPerms;
        //            //查看有没有岗位的特别权限
        //            custPerms = permissionclient.GetCustomPostMenuPerms(menuCode, ep.T_HR_POST.POSTID);
        //            if (custPerms != null && custPerms.Count() > 0)
        //                perm = Convert.ToInt32(AssignObjectType.Post);

        //            //查看有没有部门的特别权限
        //            custPerms = permissionclient.GetCustomDepartMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
        //            if (custPerms != null && custPerms.Count() > 0)
        //                perm = Convert.ToInt32(AssignObjectType.Department);

        //            //查看有没有公司的特别权限
        //            custPerms = permissionclient.GetCustomCompanyMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
        //            if (custPerms != null && custPerms.Count() > 0)
        //                perm = Convert.ToInt32(AssignObjectType.Company);
        //        }
        //    }

        //    return perm;
        //}

        //private int GetCustomPerms(string menuCode, T_HR_EMPLOYEE emp)
        //{
        //    int perm = 99;

        //    //过滤自定义的权限
        //    if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
        //    {
        //        emp.T_HR_EMPLOYEEPOST.Load();
        //    }

        //    foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
        //    {
        //        if (!ep.T_HR_POSTReference.IsLoaded)
        //            ep.T_HR_POSTReference.Load();

        //        if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
        //            ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

        //        if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
        //            ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

        //        PermissionWS.T_SYS_ENTITYMENUCUSTOMPERM[] custPerms;
        //        //查看有没有岗位的特别权限
        //        custPerms = PermClient.GetCustomPostMenuPerms(menuCode, ep.T_HR_POST.POSTID);
        //        if (custPerms != null && custPerms.Count() > 0)
        //            perm = Convert.ToInt32(AssignObjectType.Post);

        //        //查看有没有部门的特别权限
        //        custPerms = PermClient.GetCustomDepartMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
        //        if (custPerms != null && custPerms.Count() > 0)
        //            perm = Convert.ToInt32(AssignObjectType.Department);

        //        //查看有没有公司的特别权限
        //        custPerms = PermClient.GetCustomCompanyMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
        //        if (custPerms != null && custPerms.Count() > 0)
        //            perm = Convert.ToInt32(AssignObjectType.Company);
        //    }

        //    return perm;
        //}

        #endregion

        #region 克隆实体1
        public static void CloneEntity<T>(T sourceObj, T targetObj) where T : class
        {
            Type a = sourceObj.GetType();
            PropertyInfo[] infos = a.GetProperties();
            foreach (PropertyInfo prop in infos)
            {
                //System.Data.Objects.DataClasses.
                if (prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityReference)
                    || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.RelatedEnd)
                    || prop.PropertyType == typeof(System.Data.EntityState)
                    || prop.PropertyType == typeof(System.Data.EntityKey))
                    continue;
                if (sourceObj is System.Data.Objects.DataClasses.EntityObject)
                {
                    System.Data.Objects.DataClasses.EntityObject ent = sourceObj as System.Data.Objects.DataClasses.EntityObject;

                    if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
                    {
                        bool isKeyField = false;
                        foreach (var key in ent.EntityKey.EntityKeyValues)
                        {
                            if (key.Key == prop.Name)
                            {
                                isKeyField = true;
                                break;
                            }
                        }
                        if (isKeyField)
                            continue;
                    }
                }
                //prop.Name
                object value = prop.GetValue(sourceObj, null);
                try
                {
                    prop.SetValue(targetObj, value, null);
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }
        }
        #endregion


        #region IDisposable 成员

        public void Dispose()
        {
            dal.Dispose();
            //GC.SuppressFinalize(this);
        }

        #endregion

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
                BLLCommonServices.Utility.SubmitMyRecord<TEntity>(entity);
            }
            catch (Exception ex)
            {
                strTemp = ex.InnerException.Message;
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
                strTemp = ex.InnerException.Message;
            }
            return strTemp;
        }

    }


    #region 克隆实体
    public static class Clones
    {
        public static T Clone<T>(this T source)
        {
            var dcs = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            using (var ms = new System.IO.MemoryStream())
            {
                dcs.WriteObject(ms, source);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)dcs.ReadObject(ms);
            }
        }
    }
    #endregion
}
