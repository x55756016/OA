using System;

using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using SMT.SaaS.Permission.DAL;
using SMT_System_EFModel;
using SMT.SaaS.Permission.BLL.PersonnelWS;
using System.Data.Objects;
using SMT.Foundation.Log;
using SMT.SaaS.BLLCommonServices;


namespace SMT.SaaS.Permission.BLL
{
    public class BaseBll<TEntity>: IDisposable where TEntity : class
    {

        public CommDAL<TEntity> dal ;
        //protected PermissionWS.PermissionServiceClient permissionclient = new PermissionWS.PermissionServiceClient();
        public static PersonnelWS.PersonnelServiceClient personnelclient;
        public static SMT.SaaS.BLLCommonServices.OrganizationWS.OrganizationServiceClient Orgclinet;
        public BaseBll()
        {
            if (dal == null)
            {
                dal = new CommDAL<TEntity>();
            }
            if(personnelclient ==null)
            {
                personnelclient=new PersonnelWS.PersonnelServiceClient();                
            }
            if (Orgclinet == null)
            {
                Orgclinet = new BLLCommonServices.OrganizationWS.OrganizationServiceClient();
            }        
        }

        public bool Add(TEntity entity)
        {
            try
            {
                int i = dal.Add(entity);
                if (i >0)
                {
                    try
                    {
                        MvcCacheClear(entity, "Add");
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug("PermissionBLL.Add"+ex.ToString());
                    }
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

        public bool Delete(TEntity entity)
        {
            try
            {
                dal.Delete(entity);
                try
                {
                    MvcCacheClear(entity, "Delete");
                }
                catch (Exception ex)
                {
                    Tracer.Debug("PermissionBLL.Delete" + ex.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw (ex);
            }
        }

        public bool Update(TEntity entity)
        {
            try
            {
                Utility.RefreshEntity(entity as EntityObject);
                int i=dal.Update(entity);
                try
                {
                    MvcCacheClear(entity, "Modify");
                }
                catch (Exception ex)
                {
                    Tracer.Debug("PermissionBLL.Modify" + ex.ToString());
                }
                return i > 0 ? true : false;
                
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
        public ObjectQuery<TEntity> GetObjects()
        {
            return dal.GetObjects();
        }

        public object CustomerQuery(string Sql)
        {
            return dal.CustomerQuery(Sql);
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
        /// 根据权限过滤
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="userID"></param>
        /// <param name="entityName"></param>
        protected void SetOrganizationFilter(ref string filterString, ref System.Collections.Generic.List<object> queryParas, string employeeID, string entityName)
        {
            //获取用户
            SysUserBLL UserBll = new SysUserBLL();
            T_SYS_USER user = UserBll.GetUserByEmployeeID(employeeID);
            
            V_EMPLOYEEPOST vemp = personnelclient.GetEmployeeDetailByID(user.EMPLOYEEID);
            if (vemp == null) return;
            T_HR_EMPLOYEE emp = vemp.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE;


            //获取正常的角色用户权限  
            
            IQueryable<V_Permission> plist = UserBll.GetUserMenuPerms(entityName, user.SYSUSERID);

            IQueryable<V_Permission> perms =  plist;
            if (perms == null)
                return;

            //获取查询的权限,值越小，权限越大
            int maxPerm = -1;
            var permlist = perms.Where(p => p.Permission.PERMISSIONVALUE == "3");
            if (permlist.Count() > 0)
            {
                maxPerm = permlist.Min(p => Convert.ToInt32(p.RoleMenuPermission.DATARANGE));
            }
            //perms.Where(p => p.Permission.PERMISSIONVALUE == "3").Min(p => Convert.ToInt32(p.RoleMenuPermission.DATARANGE));
            //var maxPerm = "2";



            //if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
            //{
            //    emp.T_HR_EMPLOYEEPOST.Load();
            //}

            //获取自定义权限
            int custPerm = GetCustomPerms(entityName, emp);
            if (custPerm < maxPerm)
                maxPerm = custPerm;

            //看整个公司的
            if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Company))
            {
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " AND ";

                filterString += " (";
                int i = 0;
                foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
                {

                    if (i > 0)
                        filterString += " OR ";

                    filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();


                    //暂时先去掉
                    //if (!ep.T_HR_POSTReference.IsLoaded)
                    //    ep.T_HR_POSTReference.Load();

                    //if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                    //    ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                    //if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
                    //    ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

                    queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);

                    i++;
                }
                filterString += ") ";
            }


            //看部门的
            if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Department))
            {
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " AND ";

                filterString += " (";
                int i = 0;
                foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
                {

                    if (i > 0)
                        filterString += " OR ";

                    filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();

                    if (!ep.T_HR_POSTReference.IsLoaded)
                        ep.T_HR_POSTReference.Load();

                    if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                        ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                    queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);

                    i++;
                }
                filterString += ") ";
            }


            //看岗位的
            if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Post))
            {
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " AND ";

                filterString += " (";
                int i = 0;
                foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
                {

                    if (i > 0)
                        filterString += " OR ";

                    filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();

                    if (!ep.T_HR_POSTReference.IsLoaded)
                        ep.T_HR_POSTReference.Load();

                    queryParas.Add(ep.T_HR_POST.POSTID);

                    i++;
                }
                filterString += ") ";
            }

            //看员工
            if (Convert.ToInt32(maxPerm) == Convert.ToInt32(AssignObjectType.Employee))
            {
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " AND ";

                filterString += "OWNERID==@" + queryParas.Count().ToString();
                queryParas.Add(employeeID);
            }

        }

        private int GetCustomPerms(string menuCode, T_HR_EMPLOYEE emp)
        {
            int perm = 99;

            return perm;     //暂时未实现

            //过滤自定义的权限
            //emp =
            //if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
            //{
            //    emp.T_HR_EMPLOYEEPOST.Load();
            //}
            if (emp != null)
            {
                foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
                {
                    if (!ep.T_HR_POSTReference.IsLoaded)
                        ep.T_HR_POSTReference.Load();

                    if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                        ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                    if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
                        ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

                    IQueryable<T_SYS_ENTITYMENUCUSTOMPERM> custPerms;
                    //查看有没有岗位的特别权限
                    EntityMenuCustomPermBLL bll = new EntityMenuCustomPermBLL();
                    
                    
                    custPerms = bll.GetCustomPostMenuPerms(menuCode, ep.T_HR_POST.POSTID);
                    if (custPerms != null && custPerms.Count() > 0)
                        perm = Convert.ToInt32(AssignObjectType.Post);

                    //查看有没有部门的特别权限
                    custPerms = bll.GetCustomDepartMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (custPerms != null && custPerms.Count() > 0)
                        perm = Convert.ToInt32(AssignObjectType.Department);

                    //查看有没有公司的特别权限
                    custPerms = bll.GetCustomCompanyMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                    if (custPerms != null && custPerms.Count() > 0)
                        perm = Convert.ToInt32(AssignObjectType.Company);
                }
            }

            return perm;
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

        public enum Permissions
        {
            Add,// 0 
            Edit,// 1
            Delete,//2
            Browse,// 3
            Export,
            Report,
            Audit,
            Import,
        }
        

        
        #region IDisposable 成员

        public void Dispose()
        {
            dal.Dispose();
        }

        #endregion
        
    }
}
