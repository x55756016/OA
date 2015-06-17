using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TM.SaaS.CommonPermission
{
    public class PermissionUtility
    {


        public void SetOrganizationFilter(ref string filterString, ref List<object> queryParas, string employeeID, string entityName)
        {
            //注意，用户总是能看到自己创建的记录

            //获取正常的角色用户权限            
            try
            {
                int maxPerm = -1;

                List<string> CompanysPermissions = new List<string>();//具有权限的公司ID集合
                List<string> DepartmentPermissions = new List<string>();//具有权限的部门ID集合


                //获取用户
                GetEndpointSize();
                //获取权限管理中用户信息使用缓存
                //PermissionWS.T_SYS_USER user = PermClient.GetUserByEmployeeID(employeeID);
                #region 获取系统用户信息和用户信息(人事中的员工信息)

                CacheUser = GetUserCacheByEmployee(employeeID);
                //获取系统用户信息为null则只返回自己的数据
                if (CacheUser == null)
                {
                    Tracer.Debug("SetOrganizationFilter获取系统用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                    filterString = GetEmployeePermissions(filterString, queryParas, employeeID);
                    return;
                }


                //获取HR中 员工相关信息  使用缓存
                CachePerson = GetCacheEmployeeByID(employeeID);
                //如果获取员工信息为空，退出该函数并设置为默认为表单创建人或表单所属人 2011-7-21
                if (CachePerson == null)
                {
                    Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " AND ";

                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);
                    return;
                }
                #endregion
                //PermissionWS.V_Permission[] perms = PermClient.GetUserMenuPerms(entityName, user.SYSUSERID);
                //PermissionWS.V_UserPermission[] perms = PermClient.GetUserMenuPermsByUserPermission(entityName, CacheUser.SYSUSERID);
                string OwnerCompanyIDs = "";//公司ID字符窜由,连接起来
                string OwnerDepartmentIDs = "";//部门ID字符窜由,连接起来
                string OwnerPositionIDs = "";//岗位ID字符窜由,连接起来

                V_BllCommonUserPermission[] perms = BllPermClient.GetUserMenuPermsByUserPermissionBllCommon(entityName, CacheUser.SYSUSERID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs);

                bool hasPerms = true;
                bool hasCustomerPerms = true;//自定义权限

                #region 考虑传入的字符串的情况，不为空则加and  为空  加“("

                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND ";
                    filterString += " ( ";
                }
                else
                {
                    filterString += " ( ";
                }
                #endregion

                V_EMPLOYEEDETAIL emppost = PersonClient.GetEmployeePostBriefByEmployeeID(employeeID);
                if (emppost == null)
                {
                    Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                    filterString = GetEmployeePermissions(filterString, queryParas, employeeID);
                    return;
                }
                else
                {
                    if (emppost.EMPLOYEEPOSTS != null)
                    {
                        if (emppost.EMPLOYEEPOSTS.Count() == 0)
                        {
                            Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                            filterString = GetEmployeePermissions(filterString, queryParas, employeeID);
                            return;
                        }
                    }
                    else
                    {
                        Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                        filterString = GetEmployeePermissions(filterString, queryParas, employeeID);
                        return;
                    }
                }


                #region 判断权限的情况，没有权限则只返回自己创建或属于自己的单

                if ((perms == null || perms.Count() <= 0) && string.IsNullOrEmpty(OwnerCompanyIDs) && string.IsNullOrEmpty(OwnerDepartmentIDs) && string.IsNullOrEmpty(OwnerPositionIDs))
                {
                    hasPerms = false;
                    hasCustomerPerms = false;


                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);
                    filterString += ") )";
                    return;
                }
                else
                {
                    //有权限的情况下进行判断  否则查看自定义权限
                    if (perms.Count() > 0)
                    {
                        var tmpperms = perms.Where(p => p.PermissionDataRange == "3").ToList();
                        //获取大部门的小部门
                        //foreach (V_EMPLOYEEPOSTBRIEF ep in emppost.EMPLOYEEPOSTS)
                        //{
                        //    var ownerdeparts = from ent in LstDepartments
                        //                       where ent == ep.DepartmentID
                        //                       select ent;
                        //    if (ownerdeparts.Count() == 0)
                        //        LstDepartments.Add(ep.DepartmentID);
                        //}

                        if (tmpperms.Count > 0)
                        {
                            //maxPerm = tmpperms.Min(p => Convert.ToInt32(p.RoleMenuPermissionValue) );
                            if (emppost.EMPLOYEEPOSTS != null)
                            {
                                if (emppost.EMPLOYEEPOSTS.Count() > 0)
                                {


                                    filterString += " (";
                                    int i = 0;

                                    foreach (V_EMPLOYEEPOSTBRIEF ep in emppost.EMPLOYEEPOSTS)
                                    {

                                        if (i > 0)
                                        {
                                            filterString += " OR ";
                                        }
                                        var tmpents = tmpperms.Where(p => p.OwnerCompanyID == ep.CompanyID);
                                        if (tmpents.Count() > 0)
                                        {
                                            maxPerm = tmpents.Min(p => Convert.ToInt32(p.RoleMenuPermissionValue));
                                        }
                                        else
                                        {
                                            maxPerm = -1;
                                        }
                                        #region 公司级别
                                        var entcompanys = tmpperms.Where(p => p.OwnerCompanyID == ep.CompanyID);
                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Company))
                                        {
                                            if (entcompanys != null)
                                            {
                                                if (entcompanys.Count() > 0)
                                                {
                                                    filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();
                                                    queryParas.Add(ep.CompanyID);
                                                }
                                            }
                                        }
                                        #endregion

                                        #region 部门级别、大部门可以看到小部门的信息
                                        var entdepartments = tmpperms.Where(p => p.OwnerDepartmentID == ep.DepartmentID);

                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Department))
                                        {
                                            LstDepartments.Clear();
                                            LstDepartments.Add(ep.DepartmentID);
                                            if (LstDepartments.Count() > 0)
                                            {
                                                DepartmentPermissions = GetCacheEmployeeForDepartmentIDs(LstDepartments.ToArray(), CachePerson.EMPLOYEEID);
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
                                        //var entposts = tmpperms.Where(p => p.OwnerPostID == ep.POSTID);
                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Post))
                                        {
                                            //if (entposts != null)
                                            //{
                                            //    if (entposts.Count() > 0)
                                            //    {
                                            filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();
                                            queryParas.Add(ep.POSTID);
                                            //}
                                            //}
                                        }
                                        #endregion

                                        #region 查看个人信息
                                        if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Employee) || maxPerm == -1)
                                        {


                                            filterString += " ( OWNERID==@" + queryParas.Count().ToString();
                                            queryParas.Add(employeeID);

                                            filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                                            queryParas.Add(employeeID);
                                        }
                                        #endregion


                                        i++;
                                    }

                                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                                    queryParas.Add(employeeID);
                                    //filterString += " )";
                                }
                            }
                        }
                    }
                }
                #endregion

                //取员工岗位
                //V_EMPLOYEEPOST emppost = PersonClient.GetEmployeeDetailByID(CachePerson.EMPLOYEEID);

                //CachePerson.T_HR_EMPLOYEEPOST = emppost.EMPLOYEEPOSTS;


                //获取自定义权限  20100914注释  目前没使用自定义权限 
                //int custPerm = GetCustomPerms(entityName, CachePerson);
                //if (custPerm < maxPerm)
                //    maxPerm = custPerm;

                //看整个集团的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Organize))
                {
                    return;
                }

                #region 查看自定义权限
                //自定义权限存在公司集合
                //2011-6-24 注释掉的   自定义权限中的公司、部门已经取过滤了
                //2011-7-6 自定义权限的不取子公司 子部门
                if (!string.IsNullOrEmpty(OwnerCompanyIDs))
                {
                    filterString += " OR ";
                    filterString += " (";
                    string[] ArrCompany = OwnerCompanyIDs.Split(',');
                    for (int i = 0; i < ArrCompany.Length; i++)
                    {

                        if (!string.IsNullOrEmpty(ArrCompany[i].Trim()))
                        {
                            if (i > 0) filterString += " OR ";
                            filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();
                            queryParas.Add(ArrCompany[i]);
                        }
                    }
                    filterString += ") ";
                }
                //看自定义权限中的部门信息
                if (!string.IsNullOrEmpty(OwnerDepartmentIDs))
                {
                    filterString += " OR ";
                    filterString += " (";
                    string[] ArrDepartment = OwnerDepartmentIDs.Split(',');
                    for (int i = 0; i < ArrDepartment.Length; i++)
                    {
                        if (i > 0) filterString += " OR ";
                        filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();
                        queryParas.Add(ArrDepartment[i]);
                    }
                    filterString += ") ";
                }
                //看自定义权限中岗位的信息
                if (!string.IsNullOrEmpty(OwnerPositionIDs))
                {
                    filterString += " OR ";
                    filterString += " (";
                    string[] ArrPosition = OwnerPositionIDs.Split(',');
                    for (int i = 0; i < ArrPosition.Length; i++)
                    {
                        if (i > 0) filterString += " OR ";
                        filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();
                        queryParas.Add(ArrPosition[i]);
                    }
                    filterString += ") ";
                }
                #endregion

                filterString += " ) ";
                //Tracer.Debug("SetOrganizationFilter获取用户权限字符串:" + System.DateTime.Now.ToString() + ":" + filterString);
            }
            catch (Exception ex)
            {
                //有异常只返回自己创建的或属于自己的信息
                Tracer.Debug("SetOrganizationFilter获取用户权限信息出错:" + System.DateTime.Now.ToString() + ":" + ex.ToString());
                filterString = GetEmployeePermissions(filterString, queryParas, employeeID);

            }

        }

        private static void GetEndpointSize()
        {
            foreach (var op in PermClient.Endpoint.Contract.Operations)
            {
                var dataContractBehavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                    as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = Int32.MaxValue; //int.MaxValue;
                }
            }
        }

        private static string GetEmployeePermissions(string filterString, List<object> queryParas, string employeeID)
        {
            if (!string.IsNullOrEmpty(filterString))
                filterString += " AND ";

            filterString += " (( OWNERID==@" + queryParas.Count().ToString();
            queryParas.Add(employeeID);

            filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
            queryParas.Add(employeeID);
            //filterString += " ) )";
            return filterString;
        }



        /// <summary>
        /// 通过权限值获取数据范围
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="employeeID"></param>
        /// <param name="entityName"></param>
        /// <param name="PermissionValue"></param>
        /// <summary>
        /// 通过权限值获取数据范围 
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="employeeID"></param>
        /// <param name="entityName"></param>
        /// <param name="PermissionValue"></param>
        public void SetOrganizationFilterByPermissionValue(ref string filterString, ref List<object> queryParas, string employeeID, string entityName, string StrPermissionValue)
        {
            //注意，用户总是能看到自己创建的记录

            //获取正常的角色用户权限            
            try
            {
                int maxPerm = -1;

                List<string> CompanysPermissions = new List<string>();//具有权限的公司ID集合
                List<string> DepartmentPermissions = new List<string>();//具有权限的部门ID集合


                //获取用户
                foreach (var op in PermClient.Endpoint.Contract.Operations)
                {
                    var dataContractBehavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                        as DataContractSerializerOperationBehavior;
                    if (dataContractBehavior != null)
                    {
                        dataContractBehavior.MaxItemsInObjectGraph = Int32.MaxValue; //int.MaxValue;
                    }
                }
                //获取权限管理中用户信息使用缓存
                //PermissionWS.T_SYS_USER user = PermClient.GetUserByEmployeeID(employeeID);
                #region 获取系统用户信息和用户信息(人事中的员工信息)

                CacheUser = GetUserCacheByEmployee(employeeID);
                //获取系统用户信息为null则只返回自己的数据
                if (CacheUser == null)
                {
                    Tracer.Debug("SetOrganizationFilter获取系统用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " AND ";

                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);
                    return;
                }


                //获取HR中 员工相关信息  使用缓存
                CachePerson = GetCacheEmployeeByID(employeeID);
                //如果获取员工信息为空，退出该函数并设置为默认为表单创建人或表单所属人 2011-7-21
                if (CachePerson == null)
                {
                    Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " AND ";

                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);
                    return;
                }
                #endregion
                //PermissionWS.V_Permission[] perms = PermClient.GetUserMenuPerms(entityName, user.SYSUSERID);
                //PermissionWS.V_UserPermission[] perms = PermClient.GetUserMenuPermsByUserPermission(entityName, CacheUser.SYSUSERID);
                string OwnerCompanyIDs = "";//公司ID字符窜由,连接起来
                string OwnerDepartmentIDs = "";//部门ID字符窜由,连接起来
                string OwnerPositionIDs = "";//岗位ID字符窜由,连接起来

                BllCommonPermissionWS.V_BllCommonUserPermission[] perms = BllPermClient.GetUserMenuPermsByUserPermissionBllCommonAddPermissionValue(entityName, CacheUser.SYSUSERID, ref OwnerCompanyIDs, ref OwnerDepartmentIDs, ref OwnerPositionIDs, StrPermissionValue);

                bool hasPerms = true;
                bool hasCustomerPerms = true;//自定义权限

                #region 考虑传入的字符串的情况，不为空则加and  为空  加“("

                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND ";
                    filterString += " ( ";
                }
                else
                {
                    filterString += " ( ";
                }
                #endregion

                #region 判断权限的情况，没有权限则只返回自己创建或属于自己的单

                if ((perms == null || perms.Count() <= 0) && string.IsNullOrEmpty(OwnerCompanyIDs) && string.IsNullOrEmpty(OwnerDepartmentIDs) && string.IsNullOrEmpty(OwnerPositionIDs))
                {
                    hasPerms = false;
                    hasCustomerPerms = false;


                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);
                    return;
                }
                else
                {
                    //有权限的情况下进行判断  否则查看自定义权限
                    if (perms.Count() > 0)
                    {
                        var tmpperms = perms.Where(p => p.PermissionDataRange == "3").ToList();
                        if (tmpperms.Count > 0)
                            maxPerm = tmpperms.Min(p => Convert.ToInt32(p.RoleMenuPermissionValue));
                    }
                }
                #endregion

                //取员工岗位
                //V_EMPLOYEEPOST emppost = PersonClient.GetEmployeeDetailByID(CachePerson.EMPLOYEEID);
                V_EMPLOYEEDETAIL emppost = PersonClient.GetEmployeePostBriefByEmployeeID(employeeID);
                if (emppost == null)
                {
                    Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " AND ";

                    filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    queryParas.Add(employeeID);
                    return;
                }
                else
                {
                    if (emppost.EMPLOYEEPOSTS != null)
                    {
                        if (emppost.EMPLOYEEPOSTS.Count() == 0)
                        {
                            Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                            if (!string.IsNullOrEmpty(filterString))
                                filterString += " AND ";

                            filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                            queryParas.Add(employeeID);

                            filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                            queryParas.Add(employeeID);
                            return;
                        }
                    }
                    else
                    {
                        Tracer.Debug("SetOrganizationFilter获取用户信息出错:" + System.DateTime.Now.ToString() + ":" + employeeID);
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " AND ";

                        filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                        queryParas.Add(employeeID);

                        filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                        queryParas.Add(employeeID);
                        return;
                    }
                }
                //CachePerson.T_HR_EMPLOYEEPOST = emppost.EMPLOYEEPOSTS;


                //获取自定义权限  20100914注释  目前没使用自定义权限 
                //int custPerm = GetCustomPerms(entityName, CachePerson);
                //if (custPerm < maxPerm)
                //    maxPerm = custPerm;

                //看整个集团的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Organize))
                {
                    return;
                }

                #region 查看公司的权限，暂时没考虑有总公司的权限就可以查看子公司的信息的情况

                //看整个公司的

                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Company))
                {



                    if (emppost.EMPLOYEEPOSTS != null)
                    {
                        if (emppost.EMPLOYEEPOSTS.Count() > 0)
                        {
                            //如果字符串为空则加上(  不为空 则为  and  (
                            if (string.IsNullOrEmpty(filterString))
                            {
                                filterString += " ( ";
                            }

                            filterString += " (";
                            int i = 0;
                            foreach (V_EMPLOYEEPOSTBRIEF ep in emppost.EMPLOYEEPOSTS)
                            {

                                if (i > 0)
                                    filterString += " OR ";

                                filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();
                                queryParas.Add(ep.CompanyID);
                                i++;
                            }

                            filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                            queryParas.Add(employeeID);
                        }
                    }

                }


                #endregion

                #region 查看部门的权限，已考虑大部门权限可以查看子部门的信息

                //看部门的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Department))
                {
                    if (string.IsNullOrEmpty(filterString))
                    {
                        filterString += " ( ";
                    }
                    //添加部门信息,包括兼职的部门
                    foreach (V_EMPLOYEEPOSTBRIEF ep in emppost.EMPLOYEEPOSTS)
                    {
                        var ownerdeparts = from ent in LstDepartments
                                           where ent == ep.DepartmentID
                                           select ent;
                        if (ownerdeparts.Count() == 0)
                            LstDepartments.Add(ep.DepartmentID);
                    }

                    if (LstDepartments.Count() > 0)
                    {
                        DepartmentPermissions = GetCacheEmployeeForDepartmentIDs(LstDepartments.ToArray(), CachePerson.EMPLOYEEID);
                    }

                    if (DepartmentPermissions.Count() > 0)
                    {
                        filterString += " (";
                        int i = 0;

                        foreach (var ep in DepartmentPermissions)
                        {
                            if (i > 0)
                                filterString += " OR ";

                            filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();
                            queryParas.Add(ep);
                            i++;

                        }

                        //filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                        filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                        queryParas.Add(employeeID);
                    }
                    else
                    {
                        filterString += " CREATEUSERID==@" + queryParas.Count().ToString();
                        queryParas.Add(employeeID);
                    }
                }

                #endregion

                #region 查看岗位的权限

                //看岗位的
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Post))
                {
                    if (emppost.EMPLOYEEPOSTS != null)
                    {

                    }
                    if (string.IsNullOrEmpty(filterString))
                    {
                        filterString += " ( ";
                    }

                    filterString += " (";
                    int i = 0;

                    foreach (V_EMPLOYEEPOSTBRIEF ep in emppost.EMPLOYEEPOSTS)
                    {
                        if (i > 0)
                        {
                            filterString += " OR ";
                        }
                        filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();
                        queryParas.Add(ep.POSTID);
                        i++;

                    }
                    //filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);
                }
                #endregion

                #region 查看个人的权限

                //看员工
                if (Convert.ToInt32(maxPerm) == Convert.ToInt32(PermissionRange.Employee) || maxPerm == -1)
                {


                    filterString += " ( OWNERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);

                    filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString();
                    queryParas.Add(employeeID);
                }
                #endregion

                #region 查看自定义权限
                //自定义权限存在公司集合
                //2011-6-24 注释掉的   自定义权限中的公司、部门已经取过滤了
                //2011-7-6 自定义权限的不取子公司 子部门
                if (!string.IsNullOrEmpty(OwnerCompanyIDs))
                {
                    filterString += " OR ";
                    filterString += " (";
                    string[] ArrCompany = OwnerCompanyIDs.Split(',');
                    for (int i = 0; i < ArrCompany.Length; i++)
                    {

                        if (!string.IsNullOrEmpty(ArrCompany[i].Trim()))
                        {
                            if (i > 0) filterString += " OR ";
                            filterString += "OWNERCOMPANYID==@" + queryParas.Count().ToString();
                            queryParas.Add(ArrCompany[i]);
                        }
                    }
                    filterString += ") ";
                }
                //看自定义权限中的部门信息
                if (!string.IsNullOrEmpty(OwnerDepartmentIDs))
                {
                    filterString += " OR ";
                    filterString += " (";
                    string[] ArrDepartment = OwnerDepartmentIDs.Split(',');
                    for (int i = 0; i < ArrDepartment.Length; i++)
                    {
                        if (i > 0) filterString += " OR ";
                        filterString += "OWNERDEPARTMENTID==@" + queryParas.Count().ToString();
                        queryParas.Add(ArrDepartment[i]);
                    }
                    filterString += ") ";
                }
                //看自定义权限中岗位的信息
                if (!string.IsNullOrEmpty(OwnerPositionIDs))
                {
                    filterString += " OR ";
                    filterString += " (";
                    string[] ArrPosition = OwnerPositionIDs.Split(',');
                    for (int i = 0; i < ArrPosition.Length; i++)
                    {
                        if (i > 0) filterString += " OR ";
                        filterString += "OWNERPOSTID==@" + queryParas.Count().ToString();
                        queryParas.Add(ArrPosition[i]);
                    }
                    filterString += ") ";
                }
                #endregion

                filterString += " ) ";
                //Tracer.Debug("SetOrganizationFilter获取用户权限字符串:" + System.DateTime.Now.ToString() + ":" + filterString);
            }
            catch (Exception ex)
            {
                //有异常只返回自己创建的或属于自己的信息
                Tracer.Debug("SetOrganizationFilter获取用户权限信息出错:" + System.DateTime.Now.ToString() + ":" + ex.ToString());
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " AND ";

                filterString += " (( OWNERID==@" + queryParas.Count().ToString();
                queryParas.Add(employeeID);

                filterString += ") OR CREATEUSERID==@" + queryParas.Count().ToString() + ")";
                queryParas.Add(employeeID);

            }

        }


        public static List<object> SetOrganizationFilterToNewFrame(ref string filterstring, object[] paras, string employeeID, string menuCode, string sysCode)
        {
            List<object> queryParas1 = new List<object>();

            object[] aaa = BllPermClient.SetOrganizationFilterToNewFrame(ref filterstring, paras, employeeID, "T_OA_APPROVALINFO", "1");
            if (aaa != null)
            {
                queryParas1.AddRange(aaa);
            }
            return queryParas1;
        }
        /// <summary>
        /// 获取用户缓存
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEE GetCacheEmployeeByID(string employeeID)
        {
            string keyString = "CachePerson" + employeeID;
            if (CacheManager.Current[keyString] == null)
            {
                CachePerson = PersonClient.GetEmployeeByID(employeeID);
                CacheManager.Current.Insert(keyString, CachePerson, DateTime.Now.AddMinutes(15));
            }
            else
            {
                CachePerson = (T_HR_EMPLOYEE)CacheManager.Current[keyString];
            }
            return CachePerson;
        }

        /// <summary>
        /// 获取用户公司ID缓存
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public List<string> GetCacheEmployeeForCompanyIDs(string[] strCompanys, string employeeID)
        {
            string keyString = "GetCacheEmployeeForCompanyIDs" + employeeID;
            if (CacheManager.Current[keyString] == null)
            {
                LstCompanys = OrgClient.GetChildCompanyByCompanyID(strCompanys).ToList();
                CacheManager.Current.Insert(keyString, CachePerson, DateTime.Now.AddMinutes(15));
            }
            else
            {
                CachePerson = (T_HR_EMPLOYEE)CacheManager.Current[keyString];
            }
            return LstCompanys;
        }


        /// <summary>
        /// 获取用户部门ID缓存
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        private List<string> GetCacheEmployeeForDepartmentIDs(string[] strCompanys, string employeeID)
        {
            string keyString = "GetCacheEmployeeForDepartmentIDs" + employeeID;
            if (CacheManager.Current[keyString] == null)
            {
                LstDepartments = OrgClient.GetChildDepartmentBydepartmentID(strCompanys).ToList();
                if (LstDepartments.Count() == 0)
                {
                    for (int i = 0; i < strCompanys.Count(); i++)
                    {
                        LstDepartments.Add(strCompanys[i]);
                    }
                }
                CacheManager.Current.Insert(keyString, LstDepartments, DateTime.Now.AddMinutes(15));
            }
            else
            {
                LstDepartments = (List<string>)CacheManager.Current[keyString];
            }
            return LstDepartments;
        }
        /// <summary>
        /// 用户信息缓存
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        private T_SYS_USER GetUserCacheByEmployee(string employeeID)
        {

            string keyString = "CacheUser" + employeeID;
            if (CacheManager.Current[keyString] == null)
            {
                using (SysUserBLL bll = new SysUserBLL())
                {
                    CacheUser = bll.GetUserByEmployeeID(employeeID);
                    return user;
                }
                CacheManager.Current.Insert(keyString, CacheUser, DateTime.Now.AddMinutes(15));
            }
            else
            {

                CacheUser = (T_SYS_USER)CacheManager.Current[keyString];

            }
            return CacheUser;
        }

        private int GetCustomPerms(string menuCode, T_HR_EMPLOYEE emp)
        {
            int perm = 99;

            //过滤自定义的权限

            foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
            {

                PermissionWS.T_SYS_ENTITYMENUCUSTOMPERM[] custPerms;
                //查看有没有岗位的特别权限
                custPerms = PermClient.GetCustomPostMenuPerms(menuCode, ep.T_HR_POST.POSTID);
                if (custPerms != null && custPerms.Count() > 0)
                    perm = Convert.ToInt32(PermissionRange.Post);

                //查看有没有部门的特别权限
                custPerms = PermClient.GetCustomDepartMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                if (custPerms != null && custPerms.Count() > 0)
                    perm = Convert.ToInt32(PermissionRange.Department);

                //查看有没有公司的特别权限
                custPerms = PermClient.GetCustomCompanyMenuPerms(menuCode, ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                if (custPerms != null && custPerms.Count() > 0)
                    perm = Convert.ToInt32(PermissionRange.Company);
            }

            return perm;
        }

        ///// <summary>
        ///// 设置查询条件集合，加入流程控制部分的条件参数(主要用于审核人员查看当前指派给自己的待审核记录)
        ///// </summary>
        ///// <param name="strPrimaryKey">当前查询表单的主键</param>
        ///// <param name="ModelCode">当前查询表单对应模块ID</param>
        ///// <param name="strUserID">当前查询人的员工ID</param>
        ///// <param name="strCheckState">审核状态</param>
        ///// <param name="filterString">查询条件</param>
        ///// <param name="queryParas">查询参数</param>
        public void SetFilterWithflow(string strPrimaryKey, string ModelCode, string strUserID, ref string strCheckState, ref string filterString, ref List<object> queryParas)
        {
            try
            {
                FlowWFService.ServiceClient clientFlow = new FlowWFService.ServiceClient();
                string[] flowList = clientFlow.GetWaitingApprovalForm(ModelCode, strUserID);

                if (flowList == null)
                {
                    return;
                }

                if (flowList.Length == 0)
                {
                    return;
                }

                StringBuilder strIds = new StringBuilder();

                int iIndex = 0;


                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += " AND";
                }

                if (queryParas.Count() > 0)
                {
                    iIndex = queryParas.Count();
                }

                for (int i = 0; i < flowList.Count(); i++)
                {
                    string item = flowList[i];
                    if (item == null)
                    {
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    string strId = item;
                    if (i > 0)
                    {
                        strIds.Append(" OR ");
                    }

                    strIds.Append(strPrimaryKey + "=@" + iIndex.ToString());
                    queryParas.Add(strId);
                    iIndex++;
                }
                if (strIds.Length > 0)
                {
                    filterString += " (" + strIds.ToString() + " )";
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
        }


        /// <summary>
        /// 根据员工id获取员工组织架构信息，包括兼职岗位
        /// </summary>
        /// <param name="employeeid"></param>
        /// <returns></returns>
        public static V_EMPLOYEEPOST GetEmployeeOrgByid(string employeeid)
        {
            if (PersonClient == null)
            {
                PersonClient = new PersonnelServiceClient();
            }
            V_EMPLOYEEPOST ep = PersonClient.GetEmployeeDetailByID(employeeid);
            if (ep != null)
            {
                return ep;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取当前记录的描叙
        /// </summary>
        /// <param name="dtCreateDate"></param>
        /// <param name="strModelCode"></param>
        /// <param name="strModelDesp"></param>
        /// <returns></returns>
        private static string GetModelDescription(DateTime dtCreateDate, string strModelCode, string strModelDesp)
        {
            PermissionServiceClient clientPerm = new PermissionServiceClient();
            T_SYS_ENTITYMENU entMenu = clientPerm.GetSysMenuByEntityCode(strModelCode);

            string strTemp = string.Empty;
            if (entMenu != null)
            {
                strTemp = string.Format(strModelDesp, dtCreateDate.ToString("MM月dd日HH:mm").Trim(), entMenu.MENUNAME.Trim());
            }
            else
            {
                strTemp = string.Format(strModelDesp, dtCreateDate.ToString("MM月dd日HH:mm").Trim(), strModelCode.Trim());
            }

            return strTemp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SysType">系统类型</param>
        /// <param name="EntityType">实体类型</param>
        /// <param name="EntityKey">实体主键名</param>
        /// <param name="EntityId">主键ID</param>
        /// <param name="CheckState">审核状态</param>
        /// <returns></returns>
        public static bool InnerUpdateFormCheckState(string SysType, string EntityType, string EntityKey, string EntityId, CheckStates CheckState, ref string message, string strXmlParams)
        {
            bool IsResult = false;
            try
            {
                Tracer.Debug("系统名称：" + SysType);
                Tracer.Debug("手机版修改审核状态UpdateFormCheckState" + "实体名: " + EntityType);
                Tracer.Debug("表单ID名:" + EntityKey + "表单值：" + EntityId);
                Tracer.Debug("审核状态：" + ((int)CheckState).ToString());
                Tracer.Debug("XML：" + strXmlParams);
                switch (SysType)
                {
                    case "EDM":
                        Tracer.Debug("调用了进销存中的：" + EntityType);
                        EDMUpdateCheckStateWS.EDMUpdateCheckStateClient Client = new EDMUpdateCheckStateWS.EDMUpdateCheckStateClient();
                        int i = Client.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState).ToString());
                        //Tracer.Debug("手机版调用人力资源管理审核状态UpdateFormCheckState" + System.DateTime.Now.ToString());
                        if (i > 0)
                        {
                            IsResult = true;
                        }
                        break;
                }
                if (SysType == "HR")
                {
                    //OrgClient = new OrganizationWS.OrganizationServiceClient();
                    Tracer.Debug("调用了人力资源中的：" + EntityType);
                    HrUpdateCheckStateWS.HRUpdateCheckStateServicesClient hrClient = new HrUpdateCheckStateWS.HRUpdateCheckStateServicesClient();
                    int i = hrClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState).ToString());
                    //Tracer.Debug("手机版调用人力资源管理审核状态UpdateFormCheckState" + System.DateTime.Now.ToString());
                    if (i > 0)
                    {
                        IsResult = true;
                    }
                }
                if (SysType == "OA")
                {

                    //OrgClient = new OrganizationWS.OrganizationServiceClient();
                    Tracer.Debug("调用了办公系统中的：" + EntityType);
                    if (EntityType.ToUpper() == "T_OA_GIFTAPPLYMASTER")
                    {
                        GSCommonWS.GSCommonServicesClient gsClient = new GSCommonWS.GSCommonServicesClient();

                        var iresult = gsClient.UpdateCheckState(EntityType, EntityId, ((int)CheckState).ToString(), ref message);
                        if (iresult > 0)
                        {
                            IsResult = true;
                        }

                    }
                    else
                    {

                        OAUpdateCheckWS.OAUpdateCheckServicesClient oaClient = new OAUpdateCheckWS.OAUpdateCheckServicesClient();
                        int i = oaClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState).ToString());
                        //Tracer.Debug("手机版调用办公自动化审核状态UpdateFormCheckState" + System.DateTime.Now.ToString());
                        if (i > 0)
                        {
                            IsResult = true;
                        }
                    }


                }
                if (SysType == "FB")
                {
                    //日常管理的状态改动
                    if (EntityType == "T_FB_BORROWAPPLYMASTER" || EntityType == "T_FB_CHARGEAPPLYMASTER" || EntityType == "T_FB_REPAYAPPLYMASTER")
                    {
                        Tracer.Debug("调用了日常管理中的：" + EntityType);
                        FbDailyUpdateCheckStateWS.DailyUpdateCheckStateServiceClient fbaClient = new FbDailyUpdateCheckStateWS.DailyUpdateCheckStateServiceClient();
                        int i = fbaClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState).ToString());
                        if (i > 0)
                        {
                            IsResult = true;
                        }
                    }
                    else
                    {
                        string strMsg = string.Empty;
                        Tracer.Debug("调用了预算中的：" + EntityType);
                        FBServiceWS.FBServiceClient fbClient = new FBServiceWS.FBServiceClient();
                        int i = fbClient.UpdateCheckState(EntityType, EntityId, ((int)CheckState).ToString(), ref strMsg);
                        if (i > 0)
                        {
                            IsResult = true;
                        }

                    }
                }
                if (SysType == "RM")
                {
                    //OrgClient = new OrganizationWS.OrganizationServiceClient();
                    Tracer.Debug("调用了招聘系统中的：" + EntityType);
                    RMServicesClient rmClient = new RMServicesClient();
                    int i = rmClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState), strXmlParams);
                    if (i > 0)
                    {
                        IsResult = true;
                    }
                }

                if (SysType == "WP")
                {
                    WPServicesWS.WPServicesClient wpClient = new WPServicesClient();
                    Tracer.Debug("调用了工作计划中的：EntityType:" + EntityType + " EntityKey:" + EntityKey + "\r\n" + " EntityId:" + EntityId + " CheckState:" + CheckState + " URL:" + wpClient.Endpoint.Address);
                    Tracer.Debug("(int)CheckState):" + ((int)CheckState).ToString());
                    int i = wpClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState), strXmlParams, ref message);
                    Tracer.Debug("调用工作计划返回结果" + i.ToString() + "\r\n" + message);
                    if (i > 0)
                    {
                        IsResult = true;
                    }
                }
                if (SysType == "TM")
                {
                    //OrgClient = new OrganizationWS.OrganizationServiceClient();

                    TMServicesWS.TMServicesClient tmClient = new TMServicesClient();
                    Tracer.Debug("调用了培训系统中的：EntityType:" + EntityType + " EntityKey:" + EntityKey + "\r\n" + " EntityId:" + EntityId + " CheckState:" + CheckState + " URL:" + tmClient.Endpoint.Address);

                    int i = tmClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState), strXmlParams);
                    Tracer.Debug("调用培训系统返回结果" + i.ToString());
                    if (i > 0)
                    {
                        IsResult = true;
                    }
                }
                //考试系统
                if (SysType == "EM")
                {
                    EMServiceWS.EMServicesClient emClient = new EMServiceWS.EMServicesClient();
                    Tracer.Debug("调用了考试系统中的：EntityType:" + EntityType + " EntityKey:" + EntityKey + "\r\n" + " EntityId:" + EntityId + " CheckState:" + CheckState + " URL:" + emClient.Endpoint.Address + " strXmlParams:" + strXmlParams);

                    int i = emClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState), strXmlParams);
                    Tracer.Debug("调用考试系统返回结果" + i.ToString());
                    if (i > 0)
                    {
                        IsResult = true;
                    }
                }
                if (SysType == "VM")
                {
                    VMServiceWS.VMServicesClient vmClient = new VMServiceWS.VMServicesClient();
                    Tracer.Debug("调用了车辆系统中的：EntityType:" + EntityType + " EntityKey:" + EntityKey + "\r\n" + " EntityId:" + EntityId + " CheckState:" + CheckState + " URL:" + vmClient.Endpoint.Address + " strXmlParams:" + strXmlParams);

                    int i = vmClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState), strXmlParams);
                    Tracer.Debug("调用车辆系统返回结果" + i.ToString());
                    if (i > 0)
                    {
                        IsResult = true;
                    }
                }
                if (SysType == "TK")
                {

                    TKServicesWS.TKServicesClient tkClient = new TKServicesWS.TKServicesClient();
                    Tracer.Debug("调用了任务系统中的：EntityType:" + EntityType + " EntityKey:" + EntityKey + "\r\n" + " EntityId:" + EntityId + " CheckState:" + CheckState + " URL:" + tkClient.Endpoint.Address + " strXmlParams:" + strXmlParams);

                    int i = 0;// tkClient.UpdateCheckState(EntityType, EntityKey, EntityId, ((int)CheckState), strXmlParams, ref message);
                    Tracer.Debug("调用任务系统返回结果" + i.ToString() + "\r\n" + message);
                    if (i > 0)
                    {
                        IsResult = true;
                    }
                }
            }
            catch (BLLException bex)
            {
                Tracer.Debug("BLLException--手机版修改审核状态UpdateFormCheckState" + System.DateTime.Now.ToString() + " " + bex.ToString());
                throw bex;
            }
            catch (Exception ex)
            {
                Tracer.Debug("手机版修改审核状态UpdateFormCheckState" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
            return IsResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SysType">系统类型</param>
        /// <param name="EntityType">实体类型</param>
        /// <param name="EntityKey">实体主键名</param>
        /// <param name="EntityId">主键ID</param>
        /// <param name="CheckState">审核状态</param>
        /// <param name="message">业务服务的反馈消息</param>
        /// <returns></returns>
        public static bool UpdateFormCheckState(string SysType, string EntityType, string EntityKey, string EntityId, CheckStates CheckState, ref string message, string strXmlParams)
        {
            bool result = false;
            try
            {
                result = InnerUpdateFormCheckState(SysType, EntityType, EntityKey, EntityId, CheckState, ref message, strXmlParams);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                result = false;
                Tracer.Debug("流程引擎需要回滚，审核异常：模块代码：" + SysType + " 模块id：" + EntityId + " 审核状态:" + CheckState + " 错误消息：" + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 根据公司id获取公司名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetCompanNameByid(string id)
        {
            if (OrgClient == null)
            {
                OrgClient = new OrganizationWS.OrganizationServiceClient();
            }
            var company = OrgClient.GetCompanyById(id);
            if (company == null)
            {
                return string.Empty;
            }
            if (!string.IsNullOrEmpty(company.CNAME))
            {
                return company.CNAME;
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 根据部门id获取部门名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetDepartMentNameByid(string id)
        {
            if (OrgClient == null)
            {
                OrgClient = new OrganizationWS.OrganizationServiceClient();
            }
            var department = OrgClient.GetDepartmentById(id);
            if (department == null)
            {
                return string.Empty;
            }
            if (!string.IsNullOrEmpty(department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME))
            {
                return department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 根据字典类型及值获取字典名称
        /// </summary>
        /// <param name="dictionaryName"></param>
        /// <param name="dictionaryValue"></param>
        /// <returns></returns>
        public static string GetDictionaryNameByValue(string dictionaryName, decimal dictionaryValue)
        {
            return PermClient.GetDictionaryByCategoryArray(new string[] { dictionaryName }).Where(p => p.DICTIONARYVALUE == dictionaryValue).FirstOrDefault().DICTIONARYNAME;
        }
        public enum PermissionRange
        {
            /// <summary>
            /// 集团
            /// </summary>
            Organize,
            /// <summary>
            /// 公司
            /// </summary>
            Company,
            /// <summary>
            /// 部门
            /// </summary>
            Department,
            /// <summary>
            /// 岗位
            /// </summary>
            Post,
            /// <summary>
            /// 员工
            /// </summary>
            Employee

        }

        public enum Permissions
        {
            Add,
            Edit,
            Delete,
            Browse,
            Export,
            Report,
            Audit,
            Import,
        }



    }
}
