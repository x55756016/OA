using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.IMServices.IMServiceWS;
using SMT.HRM.CustomModel;
using System.Data;
using SMT.HRM.BLL.Report;

namespace SMT.HRM.BLL
{
    public class LeftOfficeConfirmBLL : BaseBll<T_HR_LEFTOFFICECONFIRM>, IOperate
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
        public IQueryable<T_HR_LEFTOFFICECONFIRM> LeftOfficeConfirmPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            int index = queryParas.Count - 1;

            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEFTOFFICECONFIRM");
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
                SetFilterWithflow("CONFIRMID", "T_HR_LEFTOFFICECONFIRM", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }
            IQueryable<T_HR_LEFTOFFICECONFIRM> ents = dal.GetObjects<T_HR_LEFTOFFICECONFIRM>();

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_LEFTOFFICECONFIRM>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        
        /// <summary>
        /// 添加离职确认 
        /// </summary>
        /// <param name="entity"></param>
        public void LeftOfficeConfirmAdd(T_HR_LEFTOFFICECONFIRM entity)
        {
            try
            {
                T_HR_LEFTOFFICECONFIRM ent = new T_HR_LEFTOFFICECONFIRM();
                Utility.CloneEntity<T_HR_LEFTOFFICECONFIRM>(entity, ent);
                if (entity.T_HR_LEFTOFFICE != null)
                {
                    ent.T_HR_LEFTOFFICEReference.EntityKey =
                new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEFTOFFICE", "DIMISSIONID", entity.T_HR_LEFTOFFICE.DIMISSIONID);

                }
                //dal.Add(ent);
                Add(ent,ent.CREATEUSERID);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeConfirmAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 更新离职确认
        /// </summary>
        /// <param name="entity">离职申请记录实体</param>
        public void LeftOfficeConfirmUpdate(T_HR_LEFTOFFICECONFIRM entity)
        {
            try
            {
                //根据离职确认ID查询离职确认表
                T_HR_LEFTOFFICECONFIRM ent = (from c in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE").Include("T_HR_LEFTOFFICE.T_HR_EMPLOYEE").Include("T_HR_LEFTOFFICE.T_HR_EMPLOYEEPOST").Include("T_HR_LEFTOFFICE.T_HR_EMPLOYEEPOST.T_HR_POST")
                                              where c.CONFIRMID == entity.CONFIRMID
                                              select c).FirstOrDefault();

                if (ent != null)
                {
                    //更新离职确认状态
                    dal.UpdateCheckState("T_HR_LEFTOFFICECONFIRM", "CONFIRMID", entity.CONFIRMID, entity.CHECKSTATE);
                    SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeConfirmUpdate:" + "1231231");
                    #region 审核通过
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //根据员工岗位ID查询员工岗位表
                        var employeepost = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                           where ep.EMPLOYEEPOSTID == ent.EMPLOYEEPOSTID
                                           select ep;
                        if (employeepost.Count() > 0)
                        {
                            //根据员工ID查询员工基本信息表
                            var employee = (from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                            where c.EMPLOYEEID == ent.EMPLOYEEID
                                            select c).FirstOrDefault();
                            //是否代理:0非代理,1代理岗位
                            if (employeepost.FirstOrDefault().ISAGENCY == "0")
                            {
                                //更新员工状态为离职
                                //string strMsg = "";
                                //EmployeeBLL bll = new EmployeeBLL();
                                //T_HR_EMPLOYEE empoyee = ent.T_HR_LEFTOFFICE.T_HR_EMPLOYEE;
                                //empoyee.EMPLOYEESTATE = "2";
                                //bll.EmployeeUpdate(empoyee, ref strMsg);

                                #region 修改员工基本信息表                      

                                if (employee != null)
                                {
                                    //员工状态：0试用 1在职 2已离职 3离职中
                                    employee.EMPLOYEESTATE = "2";
                                    //更新员工基本信息表
                                    dal.UpdateFromContext(employee);
                                    dal.SaveContextChanges();
                                }
                                #endregion
                                #region 修改岗位状态
                                //根据员工ID查询员工岗位表
                                var employeeallposts = from epall in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                       where epall.EDITSTATE == "1" && epall.T_HR_EMPLOYEE.EMPLOYEEID == ent.EMPLOYEEID
                                                       select epall;
                                if (employeeallposts.Count() > 0)
                                {
                                    foreach (var item in employeeallposts)
                                    {
                                        //if (item.ISAGENCY == "0")
                                        //{
                                        //    item.EDITSTATE = "0";
                                        //    dal.UpdateFromContext(item);
                                        //}
                                        //else
                                        //{
                                        //    dal.DeleteFromContext(item);
                                        //}

                                        //编辑状态:0未生效，1生效中
                                        item.EDITSTATE = "0";
                                        //更新员工岗位表
                                        dal.UpdateFromContext(item);
                                    }
                                    dal.SaveContextChanges();
                                }
                                #endregion
                                #region 添加异动记录
                                //根据岗位ID连表查询岗位表和部门表
                                var tmpInfo = from c in dal.GetObjects<T_HR_POST>()
                                              join b in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals b.DEPARTMENTID
                                              where c.POSTID == employeepost.FirstOrDefault().T_HR_POST.POSTID
                                              select new
                                              {
                                                  c.POSTID,
                                                  b.DEPARTMENTID,
                                                  b.T_HR_COMPANY.COMPANYID

                                              };
                                //by luojie
                                EmployeePostBLL epostBll = new EmployeePostBLL();
                                T_HR_EMPLOYEEPOST emppost = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                             where ep.EMPLOYEEPOSTID == ent.EMPLOYEEPOSTID
                                                             select ep).FirstOrDefault();
                                emppost.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                                emppost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                                epostBll.EmployeePostUpdate(emppost);
                                EmployeePostChangeBLL epchangeBLL = new EmployeePostChangeBLL();
                                T_HR_EMPLOYEEPOSTCHANGE postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                                postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                                postChange.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                                postChange.T_HR_EMPLOYEE.EMPLOYEEID = employee.EMPLOYEEID;
                                postChange.EMPLOYEECODE = employee.EMPLOYEECODE;
                                postChange.EMPLOYEENAME = employee.EMPLOYEECNAME;
                                postChange.POSTCHANGEID = Guid.NewGuid().ToString();
                                postChange.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                                postChange.ISAGENCY = "0";
                                postChange.POSTCHANGCATEGORY = "3";

                                if (tmpInfo.Count() > 0)
                                {
                                    postChange.FROMCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                    postChange.FROMDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                    postChange.FROMPOSTID = tmpInfo.FirstOrDefault().POSTID;

                                    postChange.OWNERCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                    postChange.OWNERDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                    postChange.OWNERPOSTID = tmpInfo.FirstOrDefault().POSTID;

                                }
                                postChange.OWNERID = ent.EMPLOYEEID;

                                postChange.POSTCHANGREASON = ent.LEFTOFFICEREASON;
                                postChange.CHANGEDATE = ent.LEFTOFFICEDATE.ToString();
                                postChange.CREATEUSERID = ent.CREATEUSERID;
                                string Msg = string.Empty;
                                epchangeBLL.EmployeePostChangeAdd(postChange, ref Msg);
                                #endregion
                                #region 修改社保为无效
                                PensionMasterBLL penbll = new PensionMasterBLL();
                                T_HR_PENSIONMASTER pension = penbll.GetPensionMasterByEmployeeID(ent.EMPLOYEEID);
                                if (pension != null)
                                {
                                    pension.ISVALID = "0";
                                    penbll.PensionMasterUpdate(pension);
                                }
                                #endregion
                                #region 修改员工薪资档案
                                //-by luojie 20120910
                                try
                                {
                                    //根据employid查薪资档案。
                                    var salaryArchive = from sa in dal.GetObjects<T_HR_SALARYARCHIVE>()
                                                        where sa.EMPLOYEEID == entity.EMPLOYEEID && sa.CHECKSTATE == "2" && sa.EDITSTATE == "1"
                                                        select sa;
                                    SalaryArchiveBLL salaryArchiveBll = new SalaryArchiveBLL();
                                    if (salaryArchive != null)
                                    {
                                        foreach (var sa in salaryArchive.ToList())
                                        {
                                            sa.EDITSTATE = "0";// EditStates.UnActived.ToString();
                                            sa.UPDATEDATE = System.DateTime.Now;
                                            salaryArchiveBll.Update(sa);
                                            SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "LeftOfficeConfirmBll-LeftOfficeConfirmUpdate-薪资档案修改成功:" + sa.SALARYARCHIVEID);
                                        }
                                    }
                                    else
                                    {
                                        SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "LeftOfficeConfirmBll-LeftOfficeConfirmUpdate-没有可用薪资档案,未修改");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "LeftOfficeConfirmBll-LeftOfficeConfirmUpdate：" + ex.ToString());
                                }
                                #endregion
                                #region 合同终止
                                EmployeeContractBLL conbll = new EmployeeContractBLL();
                                T_HR_EMPLOYEECONTRACT contract = conbll.GetEmployeeContractByEmployeeID(ent.EMPLOYEEID);
                                if (contract != null)
                                {
                                    contract.ENDDATE = ent.LEFTOFFICEDATE;
                                    contract.EDITSTATE = "0";
                                    conbll.EmployeeContractUpdate(contract);
                                }
                                #endregion
                                #region 删除员工合同到期提醒的定时触发
                                List<T_HR_EMPLOYEECONTRACT> contractList = conbll.GetListEmpContractByEmpID(ent.EMPLOYEEID);
                                if (contractList != null && contractList.Count > 0)
                                {
                                    contractList.ForEach(it =>
                                        {
                                            Utility.DeleteTrigger("T_HR_EMPLOYEECONTRACT", it.EMPLOYEECONTACTID);
                                        });
                                }
                                #endregion
                                #region 入职信息设为无效
                                //根据员工ID查询员工入职表
                                var employeeEntrys = from c in dal.GetObjects<T_HR_EMPLOYEEENTRY>()
                                                     where c.T_HR_EMPLOYEE.EMPLOYEEID == ent.EMPLOYEEID && c.EDITSTATE != "2"
                                                     select c;
                                if (employeeEntrys.Count() > 0)
                                {
                                    foreach (var entEntry in employeeEntrys)
                                    {
                                        //编辑状态
                                        entEntry.EDITSTATE = "2";
                                        //更新员工入职表
                                        dal.UpdateFromContext(entEntry);
                                    }
                                    dal.SaveContextChanges();
                                }
                                #endregion
                                #region 人员离职后服务同步 weirui 2012-7-10
                                try
                                {
                                    T_HR_EMPLOYEECHANGEHISTORY employeeEntity = new T_HR_EMPLOYEECHANGEHISTORY();
                                    employeeEntity.RECORDID = Guid.NewGuid().ToString();
                                    //员工ID
                                    //employeeEntity.T_HR_EMPLOYEE.EMPLOYEEID = ent.EMPLOYEEID;
                                    employeeEntity.T_HR_EMPLOYEEReference.EntityKey =
                                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", ent.EMPLOYEEID);
                                    //员工姓名
                                    employeeEntity.EMPOLYEENAME = ent.EMPLOYEECNAME;
                                    //指纹编号
                                    employeeEntity.FINGERPRINTID = employee.FINGERPRINTID;
                                    //0.入职1.异动2.离职3.薪资级别变更4.签订合同
                                    employeeEntity.FORMTYPE = "2";
                                    //记录原始单据id（员工入职表ID）
                                    employeeEntity.FORMID = ent.CONFIRMID;
                                    //主岗位非主岗位
                                    employeeEntity.ISMASTERPOSTCHANGE = "0";
                                    //包括 异动类型及离职类型 0:1=异动类型：离职类型
                                    employeeEntity.CHANGETYPE = "1";

                                    //异动时间
                                    employeeEntity.CHANGETIME = DateTime.Now;
                                    //异动原因
                                    employeeEntity.CHANGEREASON = ent.LEFTOFFICEREASON;

                                    //异动前岗位id
                                    employeeEntity.OLDPOSTID = ent.EMPLOYEEPOSTID;
                                    //根据异动前岗位ID查找岗位表，查询岗位字典ID
                                    //var oldPostInfo = dal.GetObjects<T_HR_POST>().FirstOrDefault(s => s.POSTID == employeeEntity.OLDPOSTID);
                                    var oldPostInfo = (from c in dal.GetObjects<T_HR_POST>()
                                                       join m in dal.GetObjects<T_HR_POSTDICTIONARY>()
                                                       on c.T_HR_POSTDICTIONARY.POSTDICTIONARYID equals m.POSTDICTIONARYID
                                                       where c.POSTID == ent.OWNERPOSTID
                                                       select new
                                                       {
                                                           OLDPOSTNAME = m.POSTNAME
                                                       }).FirstOrDefault();


                                    if (oldPostInfo != null)
                                    {
                                        //根据岗位字典ID查询字典表，查询岗位名称
                                        //var oldPostDictionary = dal.GetObjects<T_HR_POSTDICTIONARY>().FirstOrDefault(s => s.POSTDICTIONARYID == oldPostInfo.T_HR_POSTDICTIONARY.POSTDICTIONARYID);
                                        //异动前岗位名称
                                        employeeEntity.OLDPOSTNAME = oldPostInfo.OLDPOSTNAME;
                                    }

                                    var employeealName = (from epall in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                          where epall.EDITSTATE == "1" && epall.T_HR_EMPLOYEE.EMPLOYEEID == ent.EMPLOYEEID
                                                          select epall).FirstOrDefault();
                                    if (employeealName != null)
                                    {
                                        //异动前岗位级别
                                        employeeEntity.OLDPOSTLEVEL = employeealName.POSTLEVEL.ToString();
                                        //异动前薪资级别
                                        employeeEntity.OLDSALARYLEVEL = employeealName.SALARYLEVEL.ToString();
                                    }

                                    //异动前部门id
                                    employeeEntity.OLDDEPARTMENTID = ent.OWNERDEPARTMENTID;
                                    //根据异动前部门ID查找部门表，查询部门字典ID
                                    //var oldDepartment = dal.GetObjects<T_HR_DEPARTMENT>().FirstOrDefault(s => s.DEPARTMENTID == employeeEntity.OLDDEPARTMENTID);
                                    var oldDepartment = (from c in dal.GetObjects<T_HR_DEPARTMENT>()
                                                         join m in dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>()
                                                         on c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID equals m.DEPARTMENTDICTIONARYID
                                                         where c.DEPARTMENTID == ent.OWNERDEPARTMENTID
                                                         select new
                                                         {
                                                             OLDDEPARTMENTNAME = m.DEPARTMENTNAME
                                                         }).FirstOrDefault();
                                    if (oldDepartment != null)
                                    {
                                        //根据部门字典ID查询部门字典表，查询部门名称
                                        //var oldDepartmentDictionary = dal.GetObjects<T_HR_DEPARTMENTDICTIONARY>().FirstOrDefault(s => s.DEPARTMENTDICTIONARYID == oldDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                                        //异动前部门名称
                                        employeeEntity.OLDDEPARTMENTNAME = oldDepartment.OLDDEPARTMENTNAME;
                                    }

                                    //异动前公司id
                                    employeeEntity.OLDCOMPANYID = ent.OWNERCOMPANYID;
                                    //根据异动前公司ID查找公司表，查询公司字典ID
                                    //var oldCompany = dal.GetObjects<T_HR_COMPANY>().FirstOrDefault(s => s.COMPANYID == employeeEntity.OLDCOMPANYID);
                                    var oldCompany = (from c in dal.GetObjects<T_HR_COMPANY>()
                                                      where c.COMPANYID == ent.OWNERCOMPANYID
                                                      select new
                                                      {
                                                          OLDCOMPANYNAMECH = c.CNAME,
                                                          OLDCOMPANYNAMEEN = c.ENAME
                                                      }).FirstOrDefault();
                                    if (oldCompany != null)
                                    {
                                        //根据公司字典ID查询公司字典表，查询公司名称
                                        //var oldCompanyHistory = dal.GetObjects<T_HR_COMPANYHISTORY>().FirstOrDefault(s => s.COMPANYID == oldCompany.COMPANYID);
                                        //异动前公司名称(中文)
                                        employeeEntity.OLDCOMPANYNAME = oldCompany.OLDCOMPANYNAMECH;
                                    }
                                    //备注
                                    employeeEntity.REMART = ent.REMARK;
                                    //创建时间
                                    employeeEntity.CREATEDATE = DateTime.Now;
                                    //所属员工ID
                                    employeeEntity.OWNERID = ent.OWNERID;
                                    //所属岗位ID
                                    employeeEntity.OWNERPOSTID = ent.OWNERPOSTID;
                                    //所属部门ID
                                    employeeEntity.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
                                    //所属公司ID
                                    employeeEntity.OWNERCOMPANYID = ent.OWNERCOMPANYID;
                                    dal.AddToContext(employeeEntity);
                                    dal.SaveContextChanges();
                                }
                                catch (Exception ex)
                                {
                                    SMT.Foundation.Log.Tracer.Debug("员工离职报表服务同步:" + ex.ToString());
                                }
                                #endregion
                                #region 禁用系统用户
                                SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient perclient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();
                                SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_USER user = perclient.GetUserByEmployeeID(ent.EMPLOYEEID);
                                if (user != null)
                                {
                                    user.STATE = "0";
                                    //perclient.SysUserInfoUpdate(user);
                                    //by luojie 改为不需要离职申请也直接离职
                                    //if (ent.T_HR_LEFTOFFICE != null && ent.T_HR_LEFTOFFICE.T_HR_EMPLOYEEPOST != null)
                                    if (ent.EMPLOYEEPOSTID != null)
                                    {
                                        //判断要离职的岗位是否为主职
                                        //T_HR_EMPLOYEEPOST中ISAGENCY"0"为主职，"1"为兼职
                                        var LeftEmployeeInfo = (from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                                where c.EMPLOYEEPOSTID == ent.EMPLOYEEPOSTID
                                                                select c).FirstOrDefault();
                                        bool IsMain = false;
                                        //if (ent.T_HR_LEFTOFFICE.T_HR_EMPLOYEEPOST.ISAGENCY == "0")
                                        if (LeftEmployeeInfo != null && LeftEmployeeInfo.ISAGENCY == "0")
                                        {
                                            IsMain = true;
                                        }

                                        //bool IsSuccess= perclient.SysUserInfoUpdateForEmployeeLeft(user, ent.T_HR_LEFTOFFICE.OWNERCOMPANYID,ent.T_HR_LEFTOFFICE.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID,IsMain);
                                        bool IsSuccess = perclient.SysUserInfoUpdateForEmployeeLeft(user, ent.OWNERCOMPANYID, ent.EMPLOYEEPOSTID, IsMain);
                                        string StrResult = "";
                                        if (IsSuccess)
                                        {
                                            StrResult = "成功";
                                        }
                                        else
                                        {
                                            StrResult = "失败";
                                        }
                                        SMT.Foundation.Log.Tracer.Debug("员工离职更新权限系统用户状态为" + System.DateTime.Now.ToString() + ":" + StrResult);
                                    }
                                    else
                                    {
                                        SMT.Foundation.Log.Tracer.Debug("员工离职对象为空" + System.DateTime.Now.ToString());
                                    }
                                }
                                #endregion

                            }
                            else
                            {
                                #region 修改岗位状态
                                //根据员工ID查询员工岗位表
                                var employeeallposts = from epall in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                       where epall.EDITSTATE == "1" && epall.EMPLOYEEPOSTID == ent.EMPLOYEEPOSTID
                                                       select epall;
                                if (employeeallposts.Count() > 0)
                                {
                                    foreach (var item in employeeallposts)
                                    {
                                        //if (item.ISAGENCY == "0")
                                        //{
                                        //    item.EDITSTATE = "0";
                                        //    dal.UpdateFromContext(item);
                                        //}
                                        //else
                                        //{
                                        //    dal.DeleteFromContext(item);
                                        //}

                                        //编辑状态:0未生效，1生效中
                                        item.EDITSTATE = "0";
                                        //更新员工岗位表
                                        dal.UpdateFromContext(item);
                                    }
                                    dal.SaveContextChanges();
                                }
                                #endregion
                                #region 添加异动记录
                                //根据岗位ID连表查询岗位表和部门表
                                var tmpInfo = from c in dal.GetObjects<T_HR_POST>()
                                              join b in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals b.DEPARTMENTID
                                              where c.POSTID == employeepost.FirstOrDefault().T_HR_POST.POSTID
                                              select new
                                              {
                                                  c.POSTID,
                                                  b.DEPARTMENTID,
                                                  b.T_HR_COMPANY.COMPANYID

                                              };
                                //by luojie
                                EmployeePostBLL epostBll = new EmployeePostBLL();
                                T_HR_EMPLOYEEPOST emppost = (from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                                             where ep.EMPLOYEEPOSTID == ent.EMPLOYEEPOSTID
                                                             select ep).FirstOrDefault();
                                emppost.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                                emppost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                                epostBll.EmployeePostUpdate(emppost);
                                EmployeePostChangeBLL epchangeBLL = new EmployeePostChangeBLL();
                                T_HR_EMPLOYEEPOSTCHANGE postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                                postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                                postChange.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                                postChange.T_HR_EMPLOYEE.EMPLOYEEID = employee.EMPLOYEEID;
                                postChange.EMPLOYEECODE = employee.EMPLOYEECODE;
                                postChange.EMPLOYEENAME = employee.EMPLOYEECNAME;
                                postChange.POSTCHANGEID = Guid.NewGuid().ToString();
                                postChange.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                                postChange.ISAGENCY = "0";
                                postChange.POSTCHANGCATEGORY = "3";

                                if (tmpInfo.Count() > 0)
                                {
                                    postChange.FROMCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                    postChange.FROMDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                    postChange.FROMPOSTID = tmpInfo.FirstOrDefault().POSTID;

                                    postChange.OWNERCOMPANYID = tmpInfo.FirstOrDefault().COMPANYID;
                                    postChange.OWNERDEPARTMENTID = tmpInfo.FirstOrDefault().DEPARTMENTID;
                                    postChange.OWNERPOSTID = tmpInfo.FirstOrDefault().POSTID;

                                }
                                postChange.OWNERID = ent.EMPLOYEEID;

                                postChange.POSTCHANGREASON = ent.LEFTOFFICEREASON;
                                postChange.CHANGEDATE = ent.LEFTOFFICEDATE.ToString();
                                postChange.CREATEUSERID = ent.CREATEUSERID;
                                string Msg = string.Empty;
                                epchangeBLL.EmployeePostChangeAdd(postChange, ref Msg);
                                #endregion
                            }
                        }


                    }
                    # endregion
                    Utility.CloneEntity<T_HR_LEFTOFFICECONFIRM>(entity, ent);
                    if (entity.T_HR_LEFTOFFICE != null)
                    {
                        ent.T_HR_LEFTOFFICEReference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_LEFTOFFICE", "DIMISSIONID", entity.T_HR_LEFTOFFICE.DIMISSIONID);
                    }
                    //dal.Update(ent);

                    Update(ent,ent.CREATEUSERID);
                }
            }
            catch (Exception ex)
            {

                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeConfirmUpdate:" + ex.Message);
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeConfirmUpdate:" + qualifiedEntitySetName);
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " LeftOfficeConfirmUpdate:" + ex.InnerException);
                throw ex;
            }
        }

        /// <summary>
        /// 删除离职确认记录
        /// </summary>
        /// <param name="dimissionIDs"></param>
        /// <returns></returns>
        public int LeftOfficeConfirmDelete(string[] IDs)
        {
            foreach (var id in IDs)
            {
                T_HR_LEFTOFFICECONFIRM ent = dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().FirstOrDefault(s => s.CONFIRMID == id);
                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    DeleteMyRecord(ent);
                }
            }
            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据离职确认ID获取信息
        /// </summary>
        /// <param name="dimissionID">离职信息ID</param>
        /// <returns></returns>
        public T_HR_LEFTOFFICECONFIRM GetLeftOfficeConfirmByID(string id)
        {
            return dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE").FirstOrDefault(s => s.CONFIRMID == id);
        }

        /// <summary>
        /// 根据员工ID获取审核通过的离职确认信息
        /// </summary>
        /// <param name="strEmployeeId">员工ID</param>
        /// <returns></returns>
        public T_HR_LEFTOFFICECONFIRM GetLeftOfficeConfirmByEmployeeId(string strEmployeeId)
        {
            string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
            return dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE").Include("T_HR_LEFTOFFICE.T_HR_EMPLOYEE").FirstOrDefault(s => s.EMPLOYEEID == strEmployeeId && s.CHECKSTATE == strCheckState);
        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var leftOfficeConfirm = (from c in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>()
                                         where c.CONFIRMID == EntityKeyValue
                                         select c).FirstOrDefault();
                if (leftOfficeConfirm != null)
                {
                    leftOfficeConfirm.CHECKSTATE = CheckState;
                    leftOfficeConfirm.UPDATEDATE = DateTime.Now;
                    SMT.Foundation.Log.Tracer.Debug("离职确认状态被调用"+EntityKeyValue);
                    LeftOfficeConfirmUpdate(leftOfficeConfirm);
                    #region 调用即时通讯接口
                    if (leftOfficeConfirm.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        SMT.Foundation.Log.Tracer.Debug("离职确认开始调用即时通讯接口");
                        DelDeparmentMember(leftOfficeConfirm);
                    }
                    #endregion
                    i = 1;

                }
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }


        #region 调用即时通讯接口
        private void DelDeparmentMember(T_HR_LEFTOFFICECONFIRM leftOffice)
        { 
            //用来记录提醒信息
            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯的接口，员工ID" + leftOffice.EMPLOYEEID);
            string StrMessage="";
            try
            {
                DataSyncServiceClient IMCient = new DataSyncServiceClient();
                var ents = from ent in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE")
                           where ent.CONFIRMID == leftOffice.CONFIRMID                           
                           select ent;
                if (ents.Count() > 0)
                {
                //    var Entoffice = from ent in dal.GetObjects<T_HR_LEFTOFFICE>().Include("T_HR_EMPLOYEEPOST")
                //                    where ent.DIMISSIONID == ents.FirstOrDefault().T_HR_LEFTOFFICE.DIMISSIONID
                //                    select ent;
                    //if (Entoffice.Count() > 0)
                    //{
                    var employeePosts = from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST")
                                            //where ent.EMPLOYEEPOSTID == Entoffice.FirstOrDefault().T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                                        where ent.EMPLOYEEPOSTID == ents.FirstOrDefault().EMPLOYEEPOSTID
                                            select ent;

                    if (employeePosts.Count() > 0)
                    {
                        var entpost=employeePosts.FirstOrDefault();

                        string postid = entpost.T_HR_POST.POSTID;
                        var entdeparts = from ent in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT").Include("T_HR_POSTDICTIONARY")
                                            where ent.POSTID == postid                                             
                                            select ent;
                            
                        if (entdeparts.Count() > 0)
                        {
                            StrMessage = "员工离职确认调用即时通讯时开始";
                            SMT.Foundation.Log.Tracer.Debug(StrMessage);
                            if (entdeparts.FirstOrDefault().T_HR_DEPARTMENT != null)
                            {
                                StrMessage = "员工离职确认调用即时通讯时部门不为空";
                                string StrPostID = entdeparts.FirstOrDefault().POSTID;
                                string StrPostName = "";//岗位名称
                                if (entdeparts.FirstOrDefault().T_HR_POSTDICTIONARY != null)
                                {
                                    StrPostName = entdeparts.FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
                                }
                                else
                                {
                                    StrMessage += "员工离职时获取岗位字典为空";
                                }
                                EmployeeType postType = EmployeeType.VicePost;
                                if (entpost.ISAGENCY == "0")//主岗位
                                {
                                    postType = EmployeeType.MainPost;
                                }
                                StrMessage = IMCient.EmployeeLeave(leftOffice.EMPLOYEEID, entdeparts.FirstOrDefault().T_HR_DEPARTMENT.DEPARTMENTID, StrPostID, postType);
                                
                            }
                            SMT.Foundation.Log.Tracer.Debug("员工离职确认调用即时通讯时返回结果为：" + StrMessage);
                        }
                        else
                        {
                            StrMessage = "员工离职确认调用即时通讯时获取岗位信息为空";
                            SMT.Foundation.Log.Tracer.Debug(StrMessage);
                        }
                    }
                    else
                    {
                        StrMessage = "员工离职确认调用即时通讯时获取员工岗位为空";
                        SMT.Foundation.Log.Tracer.Debug(StrMessage);
                    }
                    //}
                    //else
                    //{
                    //    StrMessage = "员工离职确认调用即时通讯时离职申请为空" ;
                    //    SMT.Foundation.Log.Tracer.Debug(StrMessage);
                    //}

                }
                else
                {
                    StrMessage = "员工离职确认调用即时通讯时获取离职确认信息为空";
                    SMT.Foundation.Log.Tracer.Debug(StrMessage);
                }

            }
            catch (Exception ex)
            {
                StrMessage = "员工离职确认调用即时通讯错误" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(StrMessage);
            }
        }
        #endregion


        #region 导出员工离职信息报表
        /// <summary>
        /// 导出员工离职报表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="IsType"></param>
        /// <param name="IsValue"></param>
        /// <returns></returns>
        //public byte[] ExportEmployeeLeftOfficeConfirmReports(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string IsType, string IsValue)
        //{
        //    try
        //    {
        //        IQueryable<V_EmployeeLeftOfficeInfos> employeeInfos = GetEmployeeLeftOfficeConfirmReports(pageIndex, pageSize, sort, filterString, paras, ref pageCount, userID,  IsType, IsValue);
        //        if (employeeInfos != null)
        //        {
        //            List<string> colName = new List<string>();
        //            colName.Add("机构/部门");
        //            colName.Add("姓名");
        //            colName.Add("性别");
        //            colName.Add("职务名称");
        //            colName.Add("身份证号码");
        //            colName.Add("离职日期");
        //            colName.Add("离职类型");
        //            colName.Add("离职原因概要");
        //            colName.Add("备注");
                    

        //            //var tmp = new SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient().GetSysDictionaryByCategoryList(new string[] { "EMPLOYEESTATE", "TOPEDUCATION", "NATION" });

                    

        //            StringBuilder sb = new StringBuilder();
        //            for (int i = 0; i < colName.Count; i++)
        //            {
        //                if (i == colName.Count / 2)
        //                {
        //                    sb.Append("神州通在线离职员工报表");
        //                }
        //                else
        //                {
        //                    sb.Append("");
        //                }
        //            }
        //            sb.Append("\r\n");//添加提示信息
        //            for (int i = 0; i < colName.Count; i++)
        //            {
        //                sb.Append(colName[i] + ",");
        //            }
        //            sb.Append("\r\n"); // 列头

        //            //内容
        //            foreach (var employeeinfo in employeeInfos)
        //            {
                        
                        
        //                sb.Append(employeeinfo.ORGANIZENAME + ",");
        //                sb.Append(employeeinfo.EMPLOYEECNAME + ",");
        //                sb.Append(employeeinfo.SEX + ",");
        //                sb.Append(employeeinfo.POSTNAME + ",");
        //                sb.Append(employeeinfo.IDNUMBER + ",");
                       
        //                sb.Append(employeeinfo.LEFTOFFICEDATE != null ? ((DateTime)employeeinfo.LEFTOFFICEDATE).ToShortDateString():""+ ",");
        //                sb.Append(employeeinfo.LEFTOFFICECATEGORY + ",");
        //                sb.Append(employeeinfo.LEFTOFFICEREASON + ",");                        
        //                sb.Append(employeeinfo.REMARK + ",");
        //                sb.Append("\r\n");
        //            }
        //            byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
        //            return result;

        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SMT.Foundation.Log.Tracer.Debug("ExportEmployeeLeftOfficeConfirmReports:" + ex.Message);
        //        return null;
        //    }
        //}


        

        

        #region 初始化列表头
        private DataTable TableToExportInit()
        {
            DataTable dt = new DataTable();

            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = "机构/部门";
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colCordED = new DataColumn();
            colCordED.ColumnName = "姓名";
            colCordED.DataType = typeof(string);
            dt.Columns.Add(colCordED);

            DataColumn colCordFD = new DataColumn();
            colCordFD.ColumnName = "性别";
            colCordFD.DataType = typeof(string);
            dt.Columns.Add(colCordFD);


            DataColumn colCordBank = new DataColumn();
            colCordBank.ColumnName = "职务名称";
            colCordBank.DataType = typeof(string);
            dt.Columns.Add(colCordBank);

            DataColumn colCordAddress = new DataColumn();
            colCordAddress.ColumnName = "身份证号码";
            colCordAddress.DataType = typeof(string);
            dt.Columns.Add(colCordAddress);

            DataColumn colCordComments = new DataColumn();
            colCordComments.ColumnName = "离职日期";
            colCordComments.DataType = typeof(string);
            dt.Columns.Add(colCordComments);

            DataColumn colCordYfxj = new DataColumn();
            colCordYfxj.ColumnName = "离职类型";
            colCordYfxj.DataType = typeof(string);
            dt.Columns.Add(colCordYfxj);

            DataColumn colReason = new DataColumn();
            colReason.ColumnName = "离职原因概要";
            colReason.DataType = typeof(string);
            dt.Columns.Add(colReason);

            DataColumn colRemark = new DataColumn();
            colRemark.ColumnName = "备注";
            colRemark.DataType = typeof(string);
            dt.Columns.Add(colRemark);

            return dt;
        }
        #endregion
        
        #endregion
    }
}
