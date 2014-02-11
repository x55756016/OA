using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using SMT.HRM.IMServices.IMServiceWS;
using SMT.SaaS.BLLCommonServices.PermissionWS;
namespace SMT.HRM.BLL
{
    public class LeftOfficeBLL : BaseBll<T_HR_LEFTOFFICE>, IOperate
    {
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
        public IQueryable<T_HR_LEFTOFFICE> LeftOfficePaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            // int index = queryParas.Count - 1;
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEFTOFFICE");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(CheckState);
                }
            }
            else
            {
                SetFilterWithflow("DIMISSIONID", "T_HR_LEFTOFFICE", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }
            IQueryable<T_HR_LEFTOFFICE> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_LEFTOFFICE>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }


        /// <summary>
        /// 导出离职申请
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="dtStart"></param>
        /// <param name="dtEnd"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public byte[] ExportLeftOfficeViews(string filterString, IList<object> paras, DateTime dtStart, DateTime dtEnd, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            // int index = queryParas.Count - 1;
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEFTOFFICE");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(CheckState);
                }
            }
            else
            {
                SetFilterWithflow("DIMISSIONID", "T_HR_LEFTOFFICE", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }

            IQueryable<V_LEFTOFFICEVIEW> ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST.T_HR_POST")
                                                join v in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on c.T_HR_EMPLOYEE.EMPLOYEEID equals v.T_HR_EMPLOYEE.EMPLOYEEID
                                                join b in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on c.DIMISSIONID equals b.T_HR_LEFTOFFICE.DIMISSIONID into temp
                                                from d in temp.DefaultIfEmpty()
                                                select new V_LEFTOFFICEVIEW
                                                {
                                                    DIMISSIONID = c.DIMISSIONID,
                                                    EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                                    EMPLOYEECNAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                    EMPLOYEECODE = c.T_HR_EMPLOYEE.EMPLOYEECODE,
                                                    LEFTOFFICECATEGORY = c.LEFTOFFICECATEGORY,
                                                    LEFTOFFICEDATE = c.LEFTOFFICEDATE,
                                                    APPLYDATE = c.APPLYDATE,
                                                    CHECKSTATE = c.CHECKSTATE,
                                                    CREATEUSERID = c.CREATEUSERID,
                                                    OWNERCOMPANYID = c.OWNERCOMPANYID,
                                                    OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                                    OWNERPOSTID = c.OWNERPOSTID,
                                                    OWNERID = c.OWNERID,
                                                    ISCONFIRMED = d == null ? "-1" : d.CHECKSTATE,
                                                    REMARK = c.REMARK,
                                                    LEFTOFFICEREASON = c.LEFTOFFICEREASON,
                                                    EMPLOYEEPOSTID = c.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID,
                                                    POSTID = c.T_HR_EMPLOYEEPOST.T_HR_POST.POSTID,
                                                    ENTRYDATE = v.ENTRYDATE
                                                };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            if (dtStart != DateTime.MinValue)
            {
                ents = ents.Where(t => t.LEFTOFFICEDATE >= dtStart);
            }
            if (dtEnd != DateTime.MinValue)
            {
                ents = ents.Where(t => t.LEFTOFFICEDATE <= dtEnd);
            }
            ents = ents.OrderByDescending(t => t.LEFTOFFICEDATE);
            return OutEmployeeAttendStream("", "", ents.ToList());
        }

        private byte[] OutEmployeeAttendStream(string CompanyName, string Strdate, List<V_LEFTOFFICEVIEW> EmployeeInfos)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Utility.GetHeader().ToString());
            stringBuilder.Append(this.GetEmployeeAttendBody(CompanyName, Strdate, EmployeeInfos).ToString());
            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        private StringBuilder GetEmployeeAttendBody(string CompanyName, string Strdate, List<V_LEFTOFFICEVIEW> Collects)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("<body>\n\r");
            stringBuilder.Append("<table ID=\"Table0\" BORDER=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            stringBuilder.Append("<tr>");
            stringBuilder.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            stringBuilder.Append("<tr>");
            stringBuilder.Append("<td align=center class=\"title\" >员工编号</td>");
            stringBuilder.Append("<td align=center class=\"title\" >员工姓名</td>");
            stringBuilder.Append("<td align=center class=\"title\" >离职类型</td>");
            stringBuilder.Append("<td align=center class=\"title\" >入职时间</td>");
            stringBuilder.Append("<td align=center class=\"title\" >离职时间</td>");
            stringBuilder.Append("<td align=center class=\"title\" >申请时间</td>");
            stringBuilder.Append("<td align=center class=\"title\" >审批状态</td>");
            stringBuilder.Append("<td align=center class=\"title\" >是否确认</td>");
            stringBuilder.Append("</tr>");
            if (Collects.Count<V_LEFTOFFICEVIEW>() > 0)
            {
                T_SYS_DICTIONARY[] sysDictionaryByCategoryList = new PermissionServiceClient().GetSysDictionaryByCategoryList(new string[]
		        {
			        "LEFTOFFICECATEGORY", 
			        "CHECKSTATE"
		        });
                int i;
                for (i = 0; i < Collects.Count; i++)
                {
                    stringBuilder.Append("<tr>");
                    stringBuilder.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEECODE + "</td>");
                    stringBuilder.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEECNAME + "</td>");
                    stringBuilder.Append("<td class=\"x1282\">" + sysDictionaryByCategoryList.Where(t => t.DICTIONARYVALUE == Convert.ToInt32(Collects[i].LEFTOFFICECATEGORY) && t.DICTIONCATEGORY == "LEFTOFFICECATEGORY").FirstOrDefault().DICTIONARYNAME + "</td>");
                    stringBuilder.Append("<td class=\"x1282\">" + Collects[i].ENTRYDATE.Value.ToString("yyyy-MM-dd") + "</td>");
                    stringBuilder.Append("<td class=\"x1282\">" + Collects[i].LEFTOFFICEDATE.Value.ToString("yyyy-MM-dd") + "</td>");
                    stringBuilder.Append("<td class=\"x1282\">" + Collects[i].APPLYDATE.Value.ToString("yyyy-MM-dd") + "</td>");
                    stringBuilder.Append("<td class=\"x1282\">" + sysDictionaryByCategoryList.Where(t => t.DICTIONARYVALUE == decimal.Parse(Collects[i].CHECKSTATE) && t.DICTIONCATEGORY == "CHECKSTATE").FirstOrDefault().DICTIONARYNAME + "</td>");

                    int num = Convert.ToInt32(Collects[i].ISCONFIRMED);
                    string str1 = string.Empty;
                    if (num < 0)
                    {
                        str1 = "未确认";
                    }
                    else
                    {
                        if (num >= 0 && num < 2)
                        {
                            str1 = "确认中";
                        }
                        else
                        {
                            if (num == 2)
                            {
                                str1 = "已确认";
                            }
                            else
                            {
                                if (num == 3)
                                {
                                    str1 = "确认未通过";
                                }
                            }
                        }
                    }
                    stringBuilder.Append("<td class=\"x1282\">" + str1 + "</td>");
                    stringBuilder.Append("</tr>");
                }
            }
            stringBuilder.Append("</table>");
            stringBuilder.Append("</body></html>");
            return stringBuilder;
        }

        /// <summary>
        /// 获取离职申请视图
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public IQueryable<V_LEFTOFFICEVIEW> LeftOfficeViewsPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, DateTime dtStart, DateTime dtEnd, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            // int index = queryParas.Count - 1;
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEFTOFFICE");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    filterString += "CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(CheckState);
                }
            }
            else
            {
                SetFilterWithflow("DIMISSIONID", "T_HR_LEFTOFFICE", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }
            IQueryable<V_LEFTOFFICEVIEW> ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST.T_HR_POST")
                                                join v in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on c.T_HR_EMPLOYEE.EMPLOYEEID equals v.T_HR_EMPLOYEE.EMPLOYEEID 
                                                join b in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on c.DIMISSIONID equals b.T_HR_LEFTOFFICE.DIMISSIONID into temp
                                                from d in temp.DefaultIfEmpty()
                                                select new V_LEFTOFFICEVIEW
                                                {
                                                    DIMISSIONID = c.DIMISSIONID,
                                                    EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                                    EMPLOYEECNAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                    EMPLOYEECODE = c.T_HR_EMPLOYEE.EMPLOYEECODE,
                                                    LEFTOFFICECATEGORY = c.LEFTOFFICECATEGORY,
                                                    LEFTOFFICEDATE = c.LEFTOFFICEDATE,
                                                    APPLYDATE = c.APPLYDATE,
                                                    CHECKSTATE = c.CHECKSTATE,
                                                    CREATEUSERID = c.CREATEUSERID,
                                                    OWNERCOMPANYID = c.OWNERCOMPANYID,
                                                    OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                                    OWNERPOSTID = c.T_HR_EMPLOYEEPOST.T_HR_POST.POSTID,
                                                    OWNERID = c.OWNERID,
                                                    ISCONFIRMED = d == null ? "-1" : d.CHECKSTATE,
                                                    REMARK = c.REMARK,
                                                    LEFTOFFICEREASON = c.LEFTOFFICEREASON,
                                                    EMPLOYEEPOSTID = c.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID,
                                                    POSTID = c.T_HR_EMPLOYEEPOST.T_HR_POST.POSTID,
                                                    ENTRYDATE = v.ENTRYDATE
                                                };

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            if (dtStart != DateTime.MinValue)
            {
                ents = ents.Where(t => t.LEFTOFFICEDATE >= dtStart);
            }
            if (dtEnd != DateTime.MinValue)
            {
                ents = ents.Where(t => t.LEFTOFFICEDATE <= dtEnd);
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_LEFTOFFICEVIEW>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 添加离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        public void LeftOfficeAdd(T_HR_LEFTOFFICE entity, ref string strMsg)
        {
            try
            {
                var tmp = from c in dal.GetObjects()
                          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1" || c.CHECKSTATE == "2")
                          && c.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID == entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                          select c;
                if (tmp.Count() > 0)
                {
                    // throw new Exception("LEFTOFFICESUBMITTED");
                    strMsg = "LEFTOFFICESUBMITTED";
                    return;
                }
                T_HR_LEFTOFFICE ent = new T_HR_LEFTOFFICE();
                Utility.CloneEntity<T_HR_LEFTOFFICE>(entity, ent);
                ent.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                if (entity.T_HR_EMPLOYEEPOST != null)
                {
                    ent.T_HR_EMPLOYEEPOSTReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOST", "EMPLOYEEPOSTID", entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID);
                }
                //dal.Add(ent);
                //xiedx
                //2012-8-27
                bool i = Add(ent, ent.CREATEUSERID);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新离职申请记录
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        public void LeftOfficeUpdate(T_HR_LEFTOFFICE entity, ref string strMsg)
        {
            try
            {
                SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient perclient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();
                string employeeId = entity.T_HR_EMPLOYEE.EMPLOYEEID;
                var t_sys_user = perclient.GetUserByEmployeeID(employeeId);

                var tmp = from c in dal.GetObjects()
                          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1")
                          && c.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID == entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID && c.DIMISSIONID != entity.DIMISSIONID
                          select c;
                if (tmp.Count() > 0)
                {
                    //throw new Exception("LEFTOFFICESUBMITTED");
                    strMsg = "LEFTOFFICESUBMITTED";
                    return;
                }
                T_HR_LEFTOFFICE ent = dal.GetTable().FirstOrDefault(s => s.DIMISSIONID == entity.DIMISSIONID);
                if (ent != null)
                {
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //如果是代理岗位  就将代理岗设为无效   并添加异动记录

                        EmployeePostBLL epbll = new EmployeePostBLL();
                        T_HR_EMPLOYEEPOST epost = dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").FirstOrDefault(ep => ep.EMPLOYEEPOSTID == entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID);
                        if (epost != null && epost.ISAGENCY == "1")
                        {
                            epost.EDITSTATE = "0";
                            epbll.EmployeePostUpdate(epost);
                            //删除岗位
                            #region 添加异动记录
                            var tmpInfo = from c in dal.GetObjects<T_HR_POST>()
                                          join b in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals b.DEPARTMENTID
                                          where c.POSTID == epost.T_HR_POST.POSTID
                                          select new
                                          {
                                              c.POSTID,
                                              b.DEPARTMENTID,
                                              b.T_HR_COMPANY.COMPANYID

                                          };
                            EmployeePostChangeBLL epchangeBLL = new EmployeePostChangeBLL();
                            T_HR_EMPLOYEEPOSTCHANGE postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                            postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                            postChange.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                            postChange.T_HR_EMPLOYEE.EMPLOYEEID = entity.T_HR_EMPLOYEE.EMPLOYEEID;
                            postChange.EMPLOYEECODE = entity.T_HR_EMPLOYEE.EMPLOYEECODE;
                            postChange.EMPLOYEENAME = entity.T_HR_EMPLOYEE.EMPLOYEECNAME;
                            postChange.POSTCHANGEID = Guid.NewGuid().ToString();
                            postChange.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                            postChange.ISAGENCY = "1";
                            if (tmpInfo.Count() > 0)
                            {
                                postChange.FROMCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                postChange.FROMDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                postChange.FROMPOSTID = tmpInfo.FirstOrDefault().POSTID;

                                postChange.OWNERCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                postChange.OWNERDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                postChange.OWNERPOSTID = tmpInfo.FirstOrDefault().POSTID;
                            }
                            postChange.OWNERID = entity.T_HR_EMPLOYEE.EMPLOYEEID;
                            postChange.POSTCHANGREASON = entity.LEFTOFFICEREASON;
                            postChange.CHANGEDATE = entity.LEFTOFFICEDATE.ToString();
                            postChange.CREATEUSERID = entity.CREATEUSERID;
                            postChange.POSTCHANGCATEGORY = "3";
                            string Msg = string.Empty;
                            epchangeBLL.EmployeePostChangeAdd(postChange, ref Msg);
                            #endregion
                            //通知及时通讯
                            DelImstantMember(entity.T_HR_EMPLOYEE.EMPLOYEEID, epost.T_HR_POST.POSTID);

                        }
                        else
                        {
                            //员工状态修改为离职中 
                            string tmpstr = "";
                            var employeetmps = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                               where c.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                                               select c;
                            if (employeetmps.Count() > 0)
                            {
                                EmployeeBLL bll = new EmployeeBLL();
                                var employeetmp = employeetmps.FirstOrDefault();
                                if (employeetmp.EMPLOYEESTATE != "2")//已离职，如果已经离职则不要再改为离职中
                                {
                                    employeetmp.EMPLOYEESTATE = "3";//离职中
                                }
                                bll.EmployeeUpdate(employeetmp, ref tmpstr);
                            }
                        }

                        #region 员工离职通知流程管理员进行修改相应的流程
                        try
                        {
                            SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " 员工离职审核通过通知流程管理员进行修改相应的流程");
                            StringBuilder sb = new StringBuilder();
                            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                            sb.Append("<Root>");
                            //查出用户的所有角色
                            var roleUserList = perclient.GetSysUserRoleByUser(t_sys_user.SYSUSERID).ToList();
                            if (roleUserList != null && roleUserList.Any())
                            {
                                bool hasRole = false;
                                sb.Append("<Roles>");
                                foreach (var roleUser in roleUserList)
                                {
                                    //查出改用户所在的角色，还有没有其它用户，如果没有则调流程
                                    string roleId = roleUser.T_SYS_ROLE.ROLEID;
                                    var roleUserIncludeSource = perclient.GetSysUserByRole(roleId);
                                    if (roleUserIncludeSource == null)
                                    {
                                        hasRole = true;
                                        sb.Append("<Role RoleID=\"" + roleUser.T_SYS_ROLE.ROLEID + "\" RoleName=\"" + roleUser.T_SYS_ROLE.ROLENAME + "\" />");
                                    }
                                    else
                                    {
                                        var roleUserInclude = roleUserIncludeSource.Where(t => t.EMPLOYEEID != employeeId).ToList();
                                        if (roleUserInclude.Count == 0)
                                        {
                                            hasRole = true;
                                            sb.Append("<Role RoleID=\"" + roleUser.T_SYS_ROLE.ROLEID + "\" RoleName=\"" + roleUser.T_SYS_ROLE.ROLENAME + "\" />");
                                        }
                                    }
                                }
                                sb.Append("</Roles>");
                                //如果存在角色
                                if (hasRole)
                                {
                                    var empInfo = from c in dal.GetObjects<T_HR_POST>()
                                                  join b in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals b.DEPARTMENTID
                                                  join d in dal.GetObjects<T_HR_COMPANY>() on b.T_HR_COMPANY.COMPANYID equals d.COMPANYID
                                                  where c.POSTID == entity.T_HR_EMPLOYEE.OWNERPOSTID
                                                  select new
                                                  {
                                                      c.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                                      b.T_HR_COMPANY.BRIEFNAME,
                                                      c.T_HR_POSTDICTIONARY.POSTNAME
                                                  };
                                    string companyName = "";
                                    string deptName = "";
                                    string postName = "";
                                    if (empInfo != null)
                                    {
                                        companyName = empInfo.FirstOrDefault().BRIEFNAME;
                                        deptName = empInfo.FirstOrDefault().DEPARTMENTNAME;
                                        postName = empInfo.FirstOrDefault().POSTNAME;
                                    }
                                    sb.Append(" <User UserID=\"" + entity.T_HR_EMPLOYEE.EMPLOYEEID + "\" UserName=\"" + entity.T_HR_EMPLOYEE.EMPLOYEECNAME + "\" CompanyID=\"" + entity.T_HR_EMPLOYEE.OWNERCOMPANYID + "\" CompanyName=\"" + companyName + "\" DeparmentID=\"" + entity.T_HR_EMPLOYEE.OWNERDEPARTMENTID + "\" DeparmentName=\"" + deptName + "\" PostID=\"" + entity.T_HR_EMPLOYEE.OWNERPOSTID + "\" PostName=\"" + postName + "\" />");

                                    bool hasManagerEmail = false;
                                    var flowManagers = perclient.GetFlowManagers(new string[] { entity.T_HR_EMPLOYEE.OWNERCOMPANYID });
                                    if (flowManagers != null)
                                    {
                                        sb.Append("<Admins>");
                                        foreach (var mangers in flowManagers)
                                        {
                                            string email = "";
                                            var employee = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                                           where c.EMPLOYEEID == mangers.EMPLOYEEID
                                                           select c;
                                            if (employee != null)
                                            {
                                                email = employee.FirstOrDefault().EMAIL;
                                            }
                                            if (!string.IsNullOrEmpty(email))
                                            {
                                                hasManagerEmail = true;
                                                sb.Append("<Admin ID=\"" + mangers.EMPLOYEEID + "\" Name=\"" + mangers.EMPLOYEENAME + "\" Email=\"" + email + "\" />");
                                            }
                                        }
                                        sb.Append("</Admins>");
                                    }
                                    sb.Append("</Root>");
                                    if (hasManagerEmail)
                                    {
                                        SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "调用CheckFlowByRole:" + sb.ToString());
                                        SMT.SaaS.BLLCommonServices.WFPlatformWS.OutInterfaceClient outClient = new SaaS.BLLCommonServices.WFPlatformWS.OutInterfaceClient();
                                        outClient.CheckFlowByRole(sb.ToString());
                                    }
                                    else
                                    {
                                        SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "流程管理员没有设置邮箱，不调用CheckFlowByRole");
                                    }
                                }
                                else
                                {
                                    SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "当前用户id:（" + employeeId + "）所在角色还有用户，不调用CheckFlowByRole");
                                }
                            }
                            else
                            {
                                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "没有找到用户的角色");
                            }
                        }
                        catch (Exception ex)
                        {
                            SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "调用员工离职审核通过通知流程管理员进行修改相应的流程异常：" + ex.Message.ToString());
                        }
                        #endregion
                    }
                    Utility.CloneEntity<T_HR_LEFTOFFICE>(entity, ent);
                    if (entity.T_HR_EMPLOYEE != null)
                    {
                        ent.T_HR_EMPLOYEEReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    }
                    if (entity.T_HR_EMPLOYEEPOST != null)
                    {
                        ent.T_HR_EMPLOYEEPOSTReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEEPOST", "EMPLOYEEPOSTID", entity.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID);
                    }
                    //dal.Update(ent);
                    Update(ent, ent.CREATEUSERID);
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeUpdate:" + ex.Message);
                throw ex;
            }
        }


        #region 调用即时通讯接口
        private void DelImstantMember(string EMPLOYEEID, string StrPostID)
        {
            //用来记录提醒信息
            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯的接口，员工ID" + EMPLOYEEID);
            string StrMessage = "";
            try
            {
                DataSyncServiceClient IMCient = new DataSyncServiceClient();
                var q = from ent in dal.GetObjects<T_HR_POST>()
                        where ent.POSTID == StrPostID
                        select ent.T_HR_DEPARTMENT.DEPARTMENTID;

                if (q.Count() > 0)
                {
                    StrMessage = IMCient.EmployeeLeave(EMPLOYEEID, q.FirstOrDefault(), StrPostID, EmployeeType.VicePost);
                }
                else
                {
                    StrMessage = "员工离职调用即时通讯时部门为空";
                }
                SMT.Foundation.Log.Tracer.Debug("员工离职确认调用即时通讯时返回结果为：" + StrMessage);
            }
            catch (Exception ex)
            {
                StrMessage = "员工离职确认调用即时通讯错误" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(StrMessage);
            }
        }
        #endregion
        /// <summary>
        /// 删除离职申请记录
        /// </summary>
        /// <param name="dimissionIDs"></param>
        /// <returns></returns>
        public int LeftOfficeDelete(string[] dimissionIDs)
        {
            try
            {
                foreach (var id in dimissionIDs)
                {
                    T_HR_LEFTOFFICE ent = dal.GetObjects().FirstOrDefault(s => s.DIMISSIONID == id);
                    if (ent != null)
                    {
                        dal.DeleteFromContext(ent);
                        try
                        {
                            if (dal.SaveContextChanges() > 0)
                            {
                                DeleteMyRecord(ent);
                            }
                        }
                        catch
                        {
                            return 0;
                        }
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("LeftOfficeDelete Error:" + ex.ToString());
                return -1;
            }
        }
        /// <summary>
        /// 根据离职申请记录ID获取信息
        /// </summary>
        /// <param name="dimissionID">离职信息ID</param>
        /// <returns></returns>
        public T_HR_LEFTOFFICE GetLeftOfficeByID(string dimissionID)
        {
            return dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST").Include("T_HR_EMPLOYEEPOST.T_HR_POST").FirstOrDefault(s => s.DIMISSIONID == dimissionID);
        }

        /// <summary>
        /// 根据员工ID和岗位ID获取信息
        /// </summary>
        /// <param name="EmployeeID">员工ID</param>
        /// <param name="PostID">岗位ID</param>
        /// <returns></returns>
        public T_HR_LEFTOFFICE GetLeftOfficeByEmployeeIDAndPostID(string EmployeeID, string PostID)
        {
            var ents = (from ent in dal.GetObjects().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST").Include("T_HR_EMPLOYEEPOST.T_HR_POST")
                        where ent.T_HR_EMPLOYEE.EMPLOYEEID == EmployeeID
                        && ent.T_HR_EMPLOYEEPOST.T_HR_POST.POSTID == PostID
                        && ent.CHECKSTATE == "2"
                        select ent).FirstOrDefault();

            return ents;

        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var leftOffice = (from c in dal.GetObjects<T_HR_LEFTOFFICE>().Include("T_HR_EMPLOYEE").Include("T_HR_EMPLOYEEPOST")
                                  where c.DIMISSIONID == EntityKeyValue
                                  select c).FirstOrDefault();
                if (leftOffice != null)
                {
                    leftOffice.CHECKSTATE = CheckState;
                    leftOffice.UPDATEDATE = DateTime.Now;
                    LeftOfficeUpdate(leftOffice, ref strMsg);
                    if (string.IsNullOrEmpty(strMsg))
                    {
                        i = 1;
                    }
                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }
    }
}
