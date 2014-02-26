using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
//添加即时通讯的服务引用
using SMT.HRM.IMServices.IMServiceWS;
using SMT.SaaS.BLLCommonServices.PermissionWS;
using SMT.SaaS.BLLCommonServices.MailService;
using SMT.HRM.BLL.Report;
using SMT.SaaS.SmtOlineEn;
using Enyim.Caching;

using System.Data;
using System.Data.Objects;
using System.Collections.ObjectModel;
namespace SMT.HRM.BLL
{
    public class EmployeeEntryBLL : BaseBll<T_HR_EMPLOYEEENTRY>, IOperate
    {
        /// <summary>
        /// 添加员工入职信息
        /// </summary>
        /// <param name="entity">员工入职实体</param>
        /// <param name="ent">员工岗位实体</param>
        public void EmployeeEntryAdd(T_HR_EMPLOYEEENTRY entity, T_HR_EMPLOYEEPOST ent)
        {
            try
            {
                T_HR_EMPLOYEEENTRY employeeEntry = new T_HR_EMPLOYEEENTRY();
                Utility.CloneEntity<T_HR_EMPLOYEEENTRY>(entity, employeeEntry);
                employeeEntry.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                // DataContext.AddObject("T_HR_EMPLOYEEENTRY", employeeEntry);
                // dal.AddToContext(employeeEntry);
                // 把之前的入职记录设为无效
                var oldEntrys = from oldentry in dal.GetObjects()
                                where oldentry.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                                select oldentry;
                if (oldEntrys.Count() > 0)
                {
                    foreach (var item in oldEntrys)
                    {
                        item.EDITSTATE = "2";
                        dal.UpdateFromContext(item);
                    }
                    dal.SaveContextChanges();
                }
                Add(employeeEntry);
                //入职时新增相应的岗位记录
                T_HR_EMPLOYEEPOST employeePost = new T_HR_EMPLOYEEPOST();
                Utility.CloneEntity<T_HR_EMPLOYEEPOST>(ent, employeePost);
                employeePost.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                employeePost.T_HR_POSTReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", ent.T_HR_POST.POSTID);
                //DataContext.AddObject("T_HR_EMPLOYEEPOST", employeePost);
                dal.AddToContext(employeePost);
                //DataContext.SaveChanges();
                dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeEntryAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 生成员工编码的前五位
        /// </summary>
        /// <returns></returns>
        string CreateCode(DateTime EntryDate, string companyID)
        {
            string employeeCode = string.Empty;
            #region 生成员工编码前五位
            int year = EntryDate.Year;
            int month = EntryDate.Month;
            int day = EntryDate.Day;
            if (month <= 9)
            {
                if (day <= 9)
                {
                    employeeCode += "2";
                }
                else if (day <= 19)
                {
                    employeeCode += "3";
                }
                else if (day <= 29)
                {
                    employeeCode += "4";
                }
                else if (day <= 31)
                {
                    employeeCode += "5";
                }
                employeeCode += month.ToString();
            }
            else
            {
                if (day <= 9)
                {
                    employeeCode += "6";
                }
                else if (day <= 19)
                {
                    employeeCode += "7";
                }
                else if (day <= 29)
                {
                    employeeCode += "8";
                }
                else if (day <= 31)
                {
                    employeeCode += "9";
                }
                employeeCode += (1 + month % 10).ToString();
            }
            if (day <= 9)
            {
                employeeCode += day.ToString();
            }
            else
            {
                employeeCode += ((day / 10 + day % 10) % 10).ToString();
            }
            employeeCode += ((year - 1995) / 10).ToString() + ((year - 1995) % 10).ToString();
            #endregion
            #region 后三位
            bool flag = true;
            Random r = new Random();
            int i = 0;
            List<int> j = new List<int>();
            j.Add(0);
            while (flag)
            {
                i = r.Next(1, 1000);
                if (!j.Contains(i))
                {
                    j.Add(i);
                    //employeeCode += (i / 100).ToString() + ((i / 10) % 10).ToString() + (i % 10).ToString();
                    employeeCode += i.ToString();
                    var codes = from n in dal.GetObjects<T_HR_EMPLOYEE>()
                                join m in dal.GetObjects<T_HR_EMPLOYEEPOST>() on n.EMPLOYEEID equals m.T_HR_EMPLOYEE.EMPLOYEEID
                                where (n.EMPLOYEECODE == employeeCode) && m.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == companyID
                                select n;
                    if (codes.Count() == 0)
                    {
                        flag = false;
                    }
                }
            }
            #endregion
            return employeeCode;
        }
        public string EmployeeEntryAdd(T_HR_EMPLOYEE employee, T_HR_EMPLOYEEENTRY employeeEntry, T_HR_EMPLOYEEPOST employeePost)
        {
            string strMsg = string.Empty;
            try
            {

                #region 添加员工档案

                employee.EMPLOYEECODE = CreateCode(Convert.ToDateTime(employeeEntry.ENTRYDATE), employee.OWNERCOMPANYID);   //生成员工编码
                if (employee.EDITSTATE != "1")//1为生效,如果传入的值为生效则保持原始值
                {
                    employee.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                }
                dal.BeginTransaction();
                //var tempEnt = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                //              join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                //              where (c.EMPLOYEECODE == employee.EMPLOYEECODE || c.IDNUMBER == employee.IDNUMBER) && b.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == employee.OWNERCOMPANYID
                //              select c;
                var tempEnt = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                              where c.IDNUMBER == employee.IDNUMBER
                              select c;
                if (tempEnt.Count() > 0)
                {
                    try
                    {
                        var temp = tempEnt.FirstOrDefault();
                        Utility.CloneEntity<T_HR_EMPLOYEE>(employee, temp);
                        dal.UpdateFromContext(temp);
                    }
                    catch (Exception ex)
                    {
                        dal.RollbackTransaction();
                        SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeUpate:" + ex.Message);
                        if (ex.InnerException != null)
                        {
                            SMT.Foundation.Log.Tracer.Debug(ex.InnerException.Message);
                        }

                    }
                }
                else
                {
                    T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
                    Utility.CloneEntity<T_HR_EMPLOYEE>(employee, temp);
                    if (employee.T_HR_RESUME != null)
                    {
                        temp.T_HR_RESUMEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_RESUME", "RESUMEID", employee.T_HR_RESUME.RESUMEID);
                    }
                    dal.AddToContext(temp);
                    //  dal.SaveContextChanges();
                }

                #endregion
                #region 入职信息
                T_HR_EMPLOYEEENTRY Entry = new T_HR_EMPLOYEEENTRY();
                Utility.CloneEntity<T_HR_EMPLOYEEENTRY>(employeeEntry, Entry);
                Entry.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", employeeEntry.T_HR_EMPLOYEE.EMPLOYEEID);
                // 把之前的入职记录设为无效
                var oldEntrys = from oldentry in dal.GetObjects()
                                where oldentry.T_HR_EMPLOYEE.EMPLOYEEID == employeeEntry.T_HR_EMPLOYEE.EMPLOYEEID
                                select oldentry;
                if (oldEntrys.Count() > 0)
                {
                    foreach (var item in oldEntrys)
                    {
                        item.EDITSTATE = "2";
                        dal.UpdateFromContext(item);
                    }
                    //dal.SaveContextChanges();
                }
                //Add(employeeEntry);
               // dal.AddToContext(Entry);
                Add(Entry,Entry.CREATEUSERID);

                //入职时新增相应的岗位记录
                T_HR_EMPLOYEEPOST Post = new T_HR_EMPLOYEEPOST();
                Utility.CloneEntity<T_HR_EMPLOYEEPOST>(employeePost, Post);
                Post.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", employee.EMPLOYEEID);
                Post.T_HR_POSTReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", employeePost.T_HR_POST.POSTID);
              
                dal.AddToContext(Post);
               
                int i = dal.SaveContextChanges();
                dal.CommitTransaction();
                if (i > 0)
                {
                    strMsg = "SAVED";
                }
                #endregion
            }

            catch (Exception ex)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeEntryAdd:" + ex.Message + "----" + ex.InnerException.Message);
                throw ex;
            }

            return strMsg;
        }

        /// <summary>
        /// 更新员工入职信息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="postent"></param>
        public void EmployeeEntryUpdate(T_HR_EMPLOYEEENTRY entity, T_HR_EMPLOYEEPOST postent)
        {
            try
            {
                var employeeEntry = dal.GetObjects().Include("T_HR_EMPLOYEE").FirstOrDefault(s => s.EMPLOYEEENTRYID == entity.EMPLOYEEENTRYID);
                if (employeeEntry != null)
                {
                    Utility.CloneEntity<T_HR_EMPLOYEEENTRY>(entity, employeeEntry);
                    if (entity.T_HR_EMPLOYEE != null)
                    {
                        employeeEntry.T_HR_EMPLOYEEReference.EntityKey =
                                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    }
                }
               // dal.UpdateFromContext(employeeEntry);
                Update(employeeEntry, employeeEntry.CREATEUSERID);
                //SaveMyRecord(employeeEntry);

                // 修改入职，也修改此员工相关的岗位信息
                var employeePost = dal.GetObjects<T_HR_EMPLOYEEPOST>().FirstOrDefault(s => s.EMPLOYEEPOSTID == postent.EMPLOYEEPOSTID);
                if (employeePost != null)
                {
                    Utility.CloneEntity<T_HR_EMPLOYEEPOST>(postent, employeePost);
                    if (entity.T_HR_EMPLOYEE != null)
                    {
                        employeePost.T_HR_EMPLOYEEReference.EntityKey =
                                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    }
                    if (postent.T_HR_POST != null)
                    {
                        employeePost.T_HR_POST = new T_HR_POST();
                        employeePost.T_HR_POST.EntityKey =
                       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", postent.T_HR_POST.POSTID);
                        employeePost.T_HR_POSTReference.EntityKey =
                         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_POST", "POSTID", postent.T_HR_POST.POSTID);
                    }

                    dal.UpdateFromContext(employeePost);
                    dal.SaveContextChanges();
                }
                //EmployeePostBLL epb = new EmployeePostBLL();
                //epb.EmployeePostUpdate(postent);

                //获取员工公司
                string companyID = (from c in dal.GetObjects<T_HR_POST>()
                                    join b in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals b.DEPARTMENTID
                                    where c.POSTID == postent.T_HR_POST.POSTID
                                    select b.T_HR_COMPANY.COMPANYID).FirstOrDefault();


                //修改员工状态
                string strMsg = "";
                EmployeeBLL employeeBll = new EmployeeBLL();
                
                employeeBll.EmployeeUpdate(entity.T_HR_EMPLOYEE, companyID, ref strMsg);

                // DataContext.SaveChanges();
                dal.SaveContextChanges();

                //以下工作应该由流程引擎发起
                //employeeEntryAction(entity, employeeEntry);
                SMT.Foundation.Log.Tracer.Debug("员工入职审核状态："+employeeEntry.CHECKSTATE );
                if (employeeEntry.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    sendMail(entity.T_HR_EMPLOYEE);//发邮件给运维组
                    AttendanceSolutionAsignBLL attBll = new AttendanceSolutionAsignBLL();
                    attBll.AsignAttendanceSolutionForSingleEmployee(entity.T_HR_EMPLOYEE);//创建考勤方案
                    //调用即时通讯的接口  
                    string StrMessages = "员工姓名：" + entity.T_HR_EMPLOYEE.EMPLOYEECNAME + "员工ID：" + entity.T_HR_EMPLOYEE.EMPLOYEEID;
                    SMT.Foundation.Log.Tracer.Debug("员工入职开始调用即时通讯接口" +StrMessages);
                    AddImInstantMessageForEntry(entity.T_HR_EMPLOYEE, postent);
                }
                

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeEntryUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 发送邮件给运维组
        /// </summary>
        private void sendMail(T_HR_EMPLOYEE employee)
        {
            try
            {
                SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient Client = new SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient();
                //获取运维组的员工
                var employees = from c in dal.GetObjects<T_HR_EMPLOYEE>()
                                where c.OWNERPOSTID == "b7c4f515-a0b8-42ff-a103-c178170a51f7" && c.EMPLOYEESTATE != "2"
                                select c;
                //获取员工组织架构信息
                var employeeInfo = from a in dal.GetObjects<T_HR_COMPANY>()
                                   join b in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY") on a.COMPANYID equals b.T_HR_COMPANY.COMPANYID
                                   join c in dal.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY") on b.DEPARTMENTID equals c.T_HR_DEPARTMENT.DEPARTMENTID
                                   where c.POSTID == employee.OWNERCOMPANYID
                                   select new
                                   {
                                       ComanyName = a.CNAME,
                                       DepartmentName = b.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                       PostName = c.T_HR_POSTDICTIONARY.POSTNAME
                                   };
                string strOrgInfo = string.Empty;
                if (employeeInfo.Count() > 0)
                {
                    var tmp = employeeInfo.FirstOrDefault();
                    strOrgInfo = tmp.ComanyName + tmp.DepartmentName + tmp.PostName;
                }
                if (employees.Count() > 0)
                {
                    SMT.SaaS.BLLCommonServices.EngineConfigWS.MailParams[] paras = new SaaS.BLLCommonServices.EngineConfigWS.MailParams[employees.Count()];
                    int i = 0;
                    foreach (T_HR_EMPLOYEE ent in employees)
                    {
                        T_SYS_USER userInfo = GetUserInfo(employee.EMPLOYEEID);
                        if (userInfo!=null && !string.IsNullOrEmpty(userInfo.USERNAME))
                        {
                            SMT.SaaS.SmtOlineEn.SmtOlineDES des = new SmtOlineDES();
                            string TruePassword = des.getValue(userInfo.PASSWORD);
                            SMT.SaaS.BLLCommonServices.EngineConfigWS.MailParams para = new SaaS.BLLCommonServices.EngineConfigWS.MailParams();
                            para.MailContent = strOrgInfo + ":" + employee.EMPLOYEECNAME + " 已经入职，请创建邮箱。用户名:" + userInfo.USERNAME + "  密码：" + TruePassword;
                            SMT.Foundation.Log.Tracer.Debug("建邮箱语句：" + para.MailContent);
                            para.MailTitle = "请为" + employee.EMPLOYEECNAME + "创建邮箱";
                            para.ReceiveUserMail = ent.EMAIL;
                            paras[i] = para;
                            i++;
                        }
                        else
                        {
                            SMT.Foundation.Log.Tracer.Debug("EmployeeEntry-sendMail:缺少用户名");
                        }
                    }
                    Client.SendMail(paras);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeEntry-sendMail:" + ex.Message);
            }

        }

        /// <summary>
        /// 用于获取权限系统的用户信息
        /// </summary>
        /// <param name="EmployeeId">HR系统T_HR_EMPLOYEE的EMPLOYEEID</param>
        /// <returns>登录系统所用的用户信息</returns>
        private T_SYS_USER GetUserInfo(string EmployeeId)
        {
            try
            {  
                using (PermissionServiceClient psc = new PermissionServiceClient())
                {
                    string UserName = string.Empty;//记录返回的ID
                    T_SYS_USER userEnt = null;//获取User实体

                    if (!string.IsNullOrEmpty(EmployeeId))
                    {
                        userEnt = psc.GetUserByEmployeeID(EmployeeId);
                    }
                    return userEnt;
                }
            }
            catch (Exception ex)
            {
                Foundation.Log.Tracer.Debug("获取sysuserId出错："+ex.ToString());
                return null;
            }
        }

        private void employeeEntryAction(T_HR_EMPLOYEEENTRY entity, T_HR_EMPLOYEEENTRY employeeEntry)
        {
            if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
            {
                //薪资档案流程
                SalaryArchiveBLL salaryBll = new SalaryArchiveBLL();
                salaryBll.CreateEmployeeArchiveByEmployee(entity.T_HR_EMPLOYEE, true);
                //考勤方案
                AttendanceSolutionAsignBLL attBll = new AttendanceSolutionAsignBLL();
                attBll.AsignAttendanceSolutionForSingleEmployee(entity.T_HR_EMPLOYEE);
                //员工社保
                PensionMasterBLL penBll = new PensionMasterBLL();
                penBll.PensionMasterByEmployeeAdd(entity.T_HR_EMPLOYEE);
            }
        }

        /// <summary>
        /// 删除员工入职信息
        /// </summary>
        /// <param name="employeeEntryID">入职信息ID</param>
        /// <returns></returns>
        public int EmployeeEntryDelete(string[] employeeEntryIDs)
        {
            int isPoat = 1;
            try
            {

                dal.BeginTransaction();
                foreach (var employeeEntryID in employeeEntryIDs)
                {
                    #region  原来删除入职单时要删除员工档案，员工岗位和员工社保，但是删除时还是会有外键关联，好像还有员工简历，现在只删除员工入职单
                    //var entitys = from ent in dal.GetObjects().Include("T_HR_EMPLOYEE")
                    //              where ent.EMPLOYEEENTRYID == employeeEntryID
                    //              select ent;
                    //var entity = entitys.Count() > 0 ? entitys.FirstOrDefault() : null;

                    ////2012-8-13
                    ////判断员工信息情况可添加
                    //var ents = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                    //           where ep.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                    //           select ep;
                    ////2012-8-17
                    ////目前考虑了职位，合同，员工信息和社保信息
                    ////这里也不是很清楚，因为员工离职后数据库里员工信息表还是存在那个数据
                    //if (entity.T_HR_EMPLOYEE.T_HR_EMPLOYEEPOST.Count() > 0 || entity.T_HR_EMPLOYEE.T_HR_EMPLOYEECONTRACT.Count() > 0 || entity.T_HR_EMPLOYEE.T_HR_PENSIONMASTER.Count() > 0 || (entitys.Count()>0 && entity.T_HR_EMPLOYEE.EMPLOYEESTATE != "4"))
                    //{                     
                    //        isPoat = 0;
                    //}
                    //else
                    //{ 
                    //     if (entity != null)
                    //     {

                    //         // 删除员工信息
                    //         var employee = dal.GetObjects<T_HR_EMPLOYEE>().FirstOrDefault(s => s.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    //         if (employee != null)
                    //         {
                    //             dal.DeleteFromContext(employee);
                    //         }
                    //         //删除相关联的岗位
                    //         var temp = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                    //                    where ep.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                    //                    select ep;
                    //         if (temp.Count() > 0)
                    //         {
                    //             foreach (var ent in ents)
                    //             {
                    //                 dal.DeleteFromContext(ent);
                    //             }
                    //         }
                    //         dal.DeleteFromContext(entity);
                    //         DeleteMyRecord(entity);


                    //         ///TODO:ADD系统用户删除
                    //         //     SaaS.BLLCommonServices.PermissionWS.T_SYS_USER user = PermClient.GetUserByEmployeeID(entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    //         //PermClient.sysuserd

                    //         //2012-8-13
                    //         //删除社保信息
                    //         var pension = dal.GetObjects<T_HR_PENSIONMASTER>().FirstOrDefault(s => s.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    //         if (pension != null)
                    //         {
                    //             dal.DeleteFromContext(pension);
                    //         }
                    //     }
                    //     isPoat = dal.SaveContextChanges();
                    //}
                    ////2012-8-17
                    ////就是说现在只能第一次未提交才能删除，因为其他信息在
                    ////审核通过之后会生成很多员工信息，怕被恶意用户删除
                    ////而且根据entity获取的信息，有些信息获取不全
                    //if (entity.T_HR_EMPLOYEE.EMPLOYEESTATE == "4")
                    //{
                    //    if (entity != null)
                    //    {
                    //        //删除相关联的岗位
                    //        var temp = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                    //                   where ep.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID
                    //                   select ep;
                    //        if (temp.Count() > 0)
                    //        {
                    //            foreach (var ent in ents)
                    //            {
                    //                dal.DeleteFromContext(ent);
                    //            }
                    //        }
                    //        dal.DeleteFromContext(entity);
                    //        DeleteMyRecord(entity);



                    //        // 删除员工信息
                    //        var employee = dal.GetObjects<T_HR_EMPLOYEE>().FirstOrDefault(s => s.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    //        if (employee != null)
                    //        {
                    //            dal.DeleteFromContext(employee);
                    //        }
                    //        ///TODO:ADD系统用户删除
                    //        //     SaaS.BLLCommonServices.PermissionWS.T_SYS_USER user = PermClient.GetUserByEmployeeID(entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    //        //PermClient.sysuserd

                    //        //2012-8-13
                    //        //删除社保信息
                    //        var pension = dal.GetObjects<T_HR_PENSIONMASTER>().FirstOrDefault(s => s.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    //        if (pension != null)
                    //        {
                    //            dal.DeleteFromContext(pension);
                    //        }
                    //    }
                    //    isPoat = dal.SaveContextChanges();
                    //}
                    //else
                    //{
                    //    isPoat = 0;
                    //}
                    #endregion
                    var ent = (from en in dal.GetObjects()
                               where en.EMPLOYEEENTRYID == employeeEntryID
                               select en).FirstOrDefault();
                    if (ent != null)
                    {
                        dal.Delete(ent);
                        DeleteMyRecord(ent); //人员入职删除待办
                    }
                }

                dal.CommitTransaction();
                return isPoat;
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeEntryDelete:" + ex.Message);
                throw (ex);
            }
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
        //public IQueryable<T_HR_EMPLOYEEENTRY> EmployeeEntryPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID)
        //{
        //    List<object> queryParas = new List<object>();
        //    queryParas.AddRange(paras);

        //    SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEENTRY");

        //    IQueryable<T_HR_EMPLOYEEENTRY> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");
        //    switch (sType)
        //    {
        //        case "Company":
        //            ents = from o in dal.GetObjects().Include("T_HR_EMPLOYEE")
        //                   join ep in DataContext.T_HR_EMPLOYEEPOST on o.T_HR_EMPLOYEE.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in DataContext.T_HR_POST on ep.T_HR_POST.POSTID equals p.POSTID
        //                   join d in DataContext.T_HR_DEPARTMENT on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
        //                   join c in DataContext.T_HR_COMPANY on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
        //                   where c.COMPANYID == sValue
        //                   select o;
        //            break;
        //        case "Department":
        //            ents = from o in dal.GetObjects().Include("T_HR_EMPLOYEE")
        //                   join ep in DataContext.T_HR_EMPLOYEEPOST on o.T_HR_EMPLOYEE.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in DataContext.T_HR_POST on ep.T_HR_POST.POSTID equals p.POSTID
        //                   join d in DataContext.T_HR_DEPARTMENT on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
        //                   where d.DEPARTMENTID == sValue
        //                   select o;
        //            break;
        //        case "Post":
        //            ents = from o in dal.GetObjects().Include("T_HR_EMPLOYEE")
        //                   join ep in DataContext.T_HR_EMPLOYEEPOST on o.T_HR_EMPLOYEE.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in DataContext.T_HR_POST on ep.T_HR_POST.POSTID equals p.POSTID
        //                   where p.POSTID == sValue
        //                   select o;
        //            break;
        //    }
        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        ents = ents.Where(filterString, queryParas.ToArray());
        //    }
        //    ents = ents.OrderBy(sort);

        //    ents = Utility.Pager<T_HR_EMPLOYEEENTRY>(ents, pageIndex, pageSize, ref pageCount);

        //    return ents;
        //}
        public List<V_EMPLOYEEENTRY> EmployeeEntryPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID, string CheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEENTRY");

                //if (!string.IsNullOrEmpty(CheckState))
                //{
                //    if (!string.IsNullOrEmpty(filterString))
                //    {
                //        filterString += " and ";
                //    }
                //    filterString += "CHECKSTATE == @" + queryParas.Count();
                //    queryParas.Add(CheckState);
                //}
            }
            else
            {
                SetFilterWithflow("EMPLOYEEENTRYID", "T_HR_EMPLOYEEENTRY", userID, ref CheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }
            IQueryable<V_EMPLOYEEENTRY> ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEE")
                                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEPOSTID equals ep.EMPLOYEEPOSTID
                                               where ep.ISAGENCY == "0"
                                               select new V_EMPLOYEEENTRY
                                               {
                                                   EMPLOYEEENTRYID = c.EMPLOYEEENTRYID,
                                                   EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                                   EMPLOYEECODE = c.T_HR_EMPLOYEE.EMPLOYEECODE,
                                                   EMPLOYEECNAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                                   IDNUMBER = c.T_HR_EMPLOYEE.IDNUMBER,
                                                   ENTRYDATE = c.ENTRYDATE,
                                                   ONPOSTDATE = c.ONPOSTDATE,
                                                   CHECKSTATE = c.CHECKSTATE,
                                                   OWNERCOMPANYID = c.OWNERCOMPANYID,
                                                   OWNERID = c.OWNERID,
                                                   OWNERPOSTID = c.OWNERPOSTID,
                                                   OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                                                   CREATEUSERID = c.CREATEUSERID
                                               };
            switch (sType)
            {
                case "Company":
                    ents = from o in dal.GetObjects().Include("T_HR_EMPLOYEE")
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEPOSTID equals ep.EMPLOYEEPOSTID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                           where c.COMPANYID == sValue && ep.ISAGENCY == "0"
                           select new V_EMPLOYEEENTRY
                           {
                               EMPLOYEEENTRYID = o.EMPLOYEEENTRYID,
                               EMPLOYEEID = o.T_HR_EMPLOYEE.EMPLOYEEID,
                               EMPLOYEECODE = o.T_HR_EMPLOYEE.EMPLOYEECODE,
                               EMPLOYEECNAME = o.T_HR_EMPLOYEE.EMPLOYEECNAME,
                               IDNUMBER = o.T_HR_EMPLOYEE.IDNUMBER,
                               ENTRYDATE = o.ENTRYDATE,
                               ONPOSTDATE = o.ONPOSTDATE,
                               CHECKSTATE = o.CHECKSTATE,
                               OWNERCOMPANYID = o.OWNERCOMPANYID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               CREATEUSERID = o.CREATEUSERID
                           };
                    break;
                case "Department":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEPOSTID equals ep.EMPLOYEEPOSTID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           where d.DEPARTMENTID == sValue && ep.ISAGENCY == "0"
                           select new V_EMPLOYEEENTRY
                           {
                               EMPLOYEEENTRYID = o.EMPLOYEEENTRYID,
                               EMPLOYEEID = o.T_HR_EMPLOYEE.EMPLOYEEID,
                               EMPLOYEECODE = o.T_HR_EMPLOYEE.EMPLOYEECODE,
                               EMPLOYEECNAME = o.T_HR_EMPLOYEE.EMPLOYEECNAME,
                               IDNUMBER = o.T_HR_EMPLOYEE.IDNUMBER,
                               ENTRYDATE = o.ENTRYDATE,
                               ONPOSTDATE = o.ONPOSTDATE,
                               CHECKSTATE = o.CHECKSTATE,
                               OWNERCOMPANYID = o.OWNERCOMPANYID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               CREATEUSERID = o.CREATEUSERID
                           };
                    break;
                case "Post":
                    ents = from o in dal.GetObjects()
                           join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.EMPLOYEEPOSTID equals ep.EMPLOYEEPOSTID
                           join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                           where p.POSTID == sValue && ep.ISAGENCY == "0"
                           select new V_EMPLOYEEENTRY
                           {
                               EMPLOYEEENTRYID = o.EMPLOYEEENTRYID,
                               EMPLOYEEID = o.T_HR_EMPLOYEE.EMPLOYEEID,
                               EMPLOYEECODE = o.T_HR_EMPLOYEE.EMPLOYEECODE,
                               EMPLOYEECNAME = o.T_HR_EMPLOYEE.EMPLOYEECNAME,
                               IDNUMBER = o.T_HR_EMPLOYEE.IDNUMBER,
                               ENTRYDATE = o.ENTRYDATE,
                               ONPOSTDATE = o.ONPOSTDATE,
                               CHECKSTATE = o.CHECKSTATE,
                               OWNERCOMPANYID = o.OWNERCOMPANYID,
                               OWNERID = o.OWNERID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               CREATEUSERID = o.CREATEUSERID
                           };
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            if (!string.IsNullOrEmpty(CheckState))
            {
                ents = ents.Where(s => s.CHECKSTATE == CheckState);
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<V_EMPLOYEEENTRY>(ents, pageIndex, pageSize, ref pageCount);
            return ents.Count() > 0 ? ents.ToList() : null;
        }

        #region back
        //public List<V_EMPLOYEEENTRY> EmployeeEntryPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string sType, string sValue, string userID, string CheckState)
        //{
        //    List<object> queryParas = new List<object>();
        //    queryParas.AddRange(paras);
        //    if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
        //    {
        //        SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEENTRY");

        //        if (!string.IsNullOrEmpty(CheckState))
        //        {
        //            if (!string.IsNullOrEmpty(filterString))
        //            {
        //                filterString += " and ";
        //            }
        //            filterString += "CHECKSTATE == @" + queryParas.Count();
        //            queryParas.Add(CheckState);
        //        }
        //    }
        //    else
        //    {
        //        SetFilterWithflow("EMPLOYEEENTRYID", "T_HR_EMPLOYEEENTRY", userID, ref CheckState, ref filterString, ref queryParas);
        //        if (queryParas.Count() == paras.Count)
        //        {
        //            return null;
        //        }
        //    }
        //    IQueryable<V_EMPLOYEEENTRY> ents = from c in dal.GetObjects().Include("T_HR_EMPLOYEE")
        //                                       join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.T_HR_EMPLOYEE.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                                       where ep.ISAGENCY == "0" && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
        //                                       select new V_EMPLOYEEENTRY
        //                                       {
        //                                           EMPLOYEEENTRYID = c.EMPLOYEEENTRYID,
        //                                           EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
        //                                           EMPLOYEECODE = c.T_HR_EMPLOYEE.EMPLOYEECODE,
        //                                           EMPLOYEECNAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
        //                                           IDNUMBER = c.T_HR_EMPLOYEE.IDNUMBER,
        //                                           ENTRYDATE = c.ENTRYDATE,
        //                                           ONPOSTDATE = c.ONPOSTDATE,
        //                                           CHECKSTATE = c.CHECKSTATE,
        //                                           OWNERCOMPANYID = c.OWNERCOMPANYID,
        //                                           OWNERID = c.OWNERID,
        //                                           OWNERPOSTID = c.OWNERPOSTID,
        //                                           OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
        //                                           CREATEUSERID = c.CREATEUSERID
        //                                       };
        //    switch (sType)
        //    {
        //        case "Company":
        //            ents = from o in dal.GetObjects().Include("T_HR_EMPLOYEE")
        //                   join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.T_HR_EMPLOYEE.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
        //                   join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
        //                   join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
        //                   where c.COMPANYID == sValue && ep.ISAGENCY == "0" && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
        //                   select new V_EMPLOYEEENTRY
        //                   {
        //                       EMPLOYEEENTRYID = o.EMPLOYEEENTRYID,
        //                       EMPLOYEEID = o.T_HR_EMPLOYEE.EMPLOYEEID,
        //                       EMPLOYEECODE = o.T_HR_EMPLOYEE.EMPLOYEECODE,
        //                       EMPLOYEECNAME = o.T_HR_EMPLOYEE.EMPLOYEECNAME,
        //                       IDNUMBER = o.T_HR_EMPLOYEE.IDNUMBER,
        //                       ENTRYDATE = o.ENTRYDATE,
        //                       ONPOSTDATE = o.ONPOSTDATE,
        //                       CHECKSTATE = o.CHECKSTATE,
        //                       OWNERCOMPANYID = o.OWNERCOMPANYID,
        //                       OWNERID = o.OWNERID,
        //                       OWNERPOSTID = o.OWNERPOSTID,
        //                       OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
        //                       CREATEUSERID = o.CREATEUSERID
        //                   };
        //            break;
        //        case "Department":
        //            ents = from o in dal.GetObjects().Include("T_HR_EMPLOYEE")
        //                   join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.T_HR_EMPLOYEE.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
        //                   join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
        //                   where d.DEPARTMENTID == sValue && ep.ISAGENCY == "0" && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
        //                   select new V_EMPLOYEEENTRY
        //                   {
        //                       EMPLOYEEENTRYID = o.EMPLOYEEENTRYID,
        //                       EMPLOYEEID = o.T_HR_EMPLOYEE.EMPLOYEEID,
        //                       EMPLOYEECODE = o.T_HR_EMPLOYEE.EMPLOYEECODE,
        //                       EMPLOYEECNAME = o.T_HR_EMPLOYEE.EMPLOYEECNAME,
        //                       IDNUMBER = o.T_HR_EMPLOYEE.IDNUMBER,
        //                       ENTRYDATE = o.ENTRYDATE,
        //                       ONPOSTDATE = o.ONPOSTDATE,
        //                       CHECKSTATE = o.CHECKSTATE,
        //                       OWNERCOMPANYID = o.OWNERCOMPANYID,
        //                       OWNERID = o.OWNERID,
        //                       OWNERPOSTID = o.OWNERPOSTID,
        //                       OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
        //                       CREATEUSERID = o.CREATEUSERID
        //                   };
        //            break;
        //        case "Post":
        //            ents = from o in dal.GetObjects().Include("T_HR_EMPLOYEE")
        //                   join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on o.T_HR_EMPLOYEE.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
        //                   join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
        //                   where p.POSTID == sValue && ep.ISAGENCY == "0" && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
        //                   select new V_EMPLOYEEENTRY
        //                   {
        //                       EMPLOYEEENTRYID = o.EMPLOYEEENTRYID,
        //                       EMPLOYEEID = o.T_HR_EMPLOYEE.EMPLOYEEID,
        //                       EMPLOYEECODE = o.T_HR_EMPLOYEE.EMPLOYEECODE,
        //                       EMPLOYEECNAME = o.T_HR_EMPLOYEE.EMPLOYEECNAME,
        //                       IDNUMBER = o.T_HR_EMPLOYEE.IDNUMBER,
        //                       ENTRYDATE = o.ENTRYDATE,
        //                       ONPOSTDATE = o.ONPOSTDATE,
        //                       CHECKSTATE = o.CHECKSTATE,
        //                       OWNERCOMPANYID = o.OWNERCOMPANYID,
        //                       OWNERID = o.OWNERID,
        //                       OWNERPOSTID = o.OWNERPOSTID,
        //                       OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
        //                       CREATEUSERID = o.CREATEUSERID
        //                   };
        //            break;
        //    }
        //    if (!string.IsNullOrEmpty(filterString))
        //    {
        //        ents = ents.Where(filterString, queryParas.ToArray());
        //    }
        //    ents = ents.OrderBy(sort);

        //    ents = Utility.Pager<V_EMPLOYEEENTRY>(ents, pageIndex, pageSize, ref pageCount);
        //    return ents.ToList();
        //}
        #endregion
        /// <summary>
        /// 根据入职ID获取员工入职的信息
        /// </summary>
        /// <param name="employeeEntryID">员工入职ID</param>
        /// <returns>员工入职信息</returns>
        public T_HR_EMPLOYEEENTRY GetEmployeeEntryByID(string employeeEntryID)
        {
            try
            {
                var ents = (from t in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")
                            where t.EMPLOYEEENTRYID == employeeEntryID
                            select t).FirstOrDefault();
                string strlog = null;
                strlog += "\nEMPLOYEEPOSTID"+ents.EMPLOYEEPOSTID.ToString();
                strlog += "\nEMPLOYEEENTRYID" + ents.EMPLOYEEENTRYID.ToString();
                strlog += "\nEMPLOYEEID" + ents.T_HR_EMPLOYEE.EMPLOYEEID;
                strlog += "\nEMPLOYEECNAME"+ents.T_HR_EMPLOYEE.EMPLOYEECNAME;
                strlog += "\nEMPLOYEESEX" + ents.T_HR_EMPLOYEE.SEX;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString()+"EmployeeBLL-GetEmployeeEntryByID:成功-- "+strlog);
                return ents;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString()+" EmployeeBLL-GetEmployeeEntryByID:"+ex.ToString());
            }
            return null;

        }

        /// <summary>
        /// 根据员工ID获取员工入职信息
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEEENTRY GetEmployeeEntryByEmployeeID(string employeeID)
        {
            var q =from ent in  dal.GetObjects().Include("T_HR_EMPLOYEE")
                    where ent.T_HR_EMPLOYEE.EMPLOYEEID==employeeID && ent.CHECKSTATE=="2"
                    orderby ent.ENTRYDATE descending
                    select ent;
            //时间排序尚未实现故需先tolist
            var q2=q.ToList().OrderByDescending(c => c.ENTRYDATE);
            return q2.FirstOrDefault();
        }

        /// <summary>
        /// 根据员工ID获取员工入职信息 weirui 修改 添加上公司+员工ID一起查询  重载
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEEENTRY> GetEmployeeEntryByEmployeeIDAndCOMPANYID(string employeeID, string COMPANYID)
        {
            //return dal.GetObjects().Include("T_HR_EMPLOYEE").(s => (s.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && s.T_HR_EMPLOYEE.OWNERCOMPANYID == COMPANYID));

            //var Entity = from c in q
            //             orderby c.CREATEDATE
            //             select c;
            //return "";

            var EmployeentryEntity = from ent in dal.GetObjects()
                                     join entry in dal.GetTable<T_HR_EMPLOYEE>()
                                     on ent.T_HR_EMPLOYEE.EMPLOYEEID equals entry.EMPLOYEEID
                                     where ent.OWNERCOMPANYID == COMPANYID
                                     && ent.T_HR_EMPLOYEE.EMPLOYEEID == employeeID
                                     orderby ent.CREATEDATE descending
                                     select ent;
            
            if (EmployeentryEntity.Count() > 0)
            {
                //var q = EmployeentryEntity.FirstOrDefault();
                return EmployeentryEntity.ToList();
            }
            return null;
        }

        /// <summary>
        /// 修改入职信息为无效
        /// </summary>
        /// <param name="employeeID"></param>
        public void EmployeeEntryEnd(string employeeID)
        {
            var ents = from c in dal.GetObjects()
                       where c.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && c.EDITSTATE != "2"
                       select c;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    ent.EDITSTATE = "2";
                    dal.UpdateFromContext(ent);
                }
                dal.SaveContextChanges();
            }
        }

        /// <summary>
        /// 终审通过调用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                dal.BeginTransaction();
                //根据入职表ID查询到入职信息
                var tmp = (from c in dal.GetObjects<T_HR_EMPLOYEEENTRY>().Include("T_HR_EMPLOYEE")

                           where c.EMPLOYEEENTRYID == EntityKeyValue
                           select new
                           {
                               EMPLOYEEENTRY = c,
                               EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                               EMPLOYEE = c.T_HR_EMPLOYEE,
                               EMPLOYEEPOSTID = c.EMPLOYEEPOSTID

                           }).FirstOrDefault();
                if (tmp != null)
                {
                    //入职信息
                    var employeeEntry = tmp.EMPLOYEEENTRY;
                    employeeEntry.T_HR_EMPLOYEE = tmp.EMPLOYEE;
                    //员工ID
                    string employeeID = tmp.EMPLOYEEID;
                    //入职审核状态
                    employeeEntry.CHECKSTATE = CheckState;
                    //如果入职审核状态为通过
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //那么编辑状态
                        employeeEntry.EDITSTATE = "1";
                    }
                    //修改时间为当前时间
                    employeeEntry.UPDATEDATE = DateTime.Now;
                    //更新入职表
                    dal.UpdateFromContext(employeeEntry);

                    //审核未通过执行的业务
                    if (CheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        #region 更新岗位信息
                        var employeePosts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                            join en in dal.GetObjects() on ep.EMPLOYEEPOSTID equals en.EMPLOYEEPOSTID
                                            where ep.T_HR_EMPLOYEE.EMPLOYEEID == tmp.EMPLOYEEID && ep.EMPLOYEEPOSTID == tmp.EMPLOYEEPOSTID
                                            select ep;
                        Foundation.Log.Tracer.Debug("入职审核未通过修改员工岗位信息" + tmp.EMPLOYEEID + " 的岗位id为 " + tmp.EMPLOYEEPOSTID + ",记录数为" + employeePosts.Count());
                        var employeePost = employeePosts.FirstOrDefault();
                        if (employeePost != null)
                        {
                            employeePost.CHECKSTATE = CheckState;
                            //更新岗位信息
                            dal.UpdateFromContext(employeePost);
                        }
                        #endregion
                    }

                    //审核通过 执行的业务
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        #region
                        #region 更新员工状态
                        //根据员工ID查询员工基本信息表
                        var employee = dal.GetObjects<T_HR_EMPLOYEE>().FirstOrDefault(s => s.EMPLOYEEID == employeeID);
                        if (employee != null)
                        {
                            //员工状态：0试用 1在职 2已离职 3离职中
                            //如果试用期为零，则让Employee表的EmployeeState为1(在职)，免去试用期 by luojie
                            if (employeeEntry.PROBATIONPERIOD == 0 || employeeEntry.PROBATIONPERIOD == null) employee.EMPLOYEESTATE = "1";
                            else employee.EMPLOYEESTATE = "0";
                            //编辑状态
                            employee.EDITSTATE = "1";
                            //获取员工用户信息，有则把邮箱信息等传进去
                            T_SYS_USER userInfo = GetUserInfo(employee.EMPLOYEEID);
                            if (userInfo != null && !string.IsNullOrEmpty(userInfo.USERNAME))
                            {
                                employee.EMAIL = userInfo.USERNAME + "@sinomaster.com";
                                employee.OTHERCOMMUNICATE = userInfo.USERNAME;
                            }
                            //更新员工基本信息表
                            dal.UpdateFromContext(employee);
                        }
                        #endregion

                        #region 更新岗位信息
                        //根据员工岗位ID查询员工岗位表
                        //var employeePosts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                        //                    join en in dal.GetObjects() on ep.EMPLOYEEPOSTID equals en.EMPLOYEEPOSTID
                        //                    select ep;

                        var employeePosts = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                            join en in dal.GetObjects() on ep.EMPLOYEEPOSTID equals en.EMPLOYEEPOSTID
                                            where ep.T_HR_EMPLOYEE.EMPLOYEEID == tmp.EMPLOYEEID && ep.EMPLOYEEPOSTID == tmp.EMPLOYEEPOSTID
                                            select ep;
                        Foundation.Log.Tracer.Debug("入职 " + tmp.EMPLOYEEID + " 的岗位id为 " + tmp.EMPLOYEEPOSTID + ",记录数为" + employeePosts.Count());
                        var employeePost = employeePosts.FirstOrDefault();
                        if (employeePost != null)
                        {
                            //若已有生效的主岗位 则此岗位转为兼职岗位
                            bool isRepeat = CheckRepeatAgencyPost(tmp.EMPLOYEEID);
                            if (isRepeat)
                            {
                                employeePost.ISAGENCY = "1";
                            }
                            //审核状态
                            employeePost.CHECKSTATE = CheckState;
                            //编辑状态:0未生效，1生效中
                            employeePost.EDITSTATE = "1";
                            //更新岗位信息
                            dal.UpdateFromContext(employeePost);
                        }
                        #endregion

                        #region 添加异动信息
                        T_HR_EMPLOYEEPOSTCHANGE postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                        postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                        postChange.POSTCHANGEID = Guid.NewGuid().ToString();
                        postChange.EMPLOYEENAME = employee.EMPLOYEECNAME;
                        postChange.EMPLOYEECODE = employee.EMPLOYEECODE;
                        postChange.T_HR_EMPLOYEEReference.EntityKey =
                                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", employeeID);
                        postChange.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                        postChange.TOCOMPANYID = employeeEntry.OWNERCOMPANYID;
                        postChange.TODEPARTMENTID = employeeEntry.OWNERDEPARTMENTID;
                        postChange.TOPOSTID = employeeEntry.OWNERPOSTID;
                        postChange.ISAGENCY = "0";
                        postChange.OWNERID = employee.EMPLOYEEID;
                        postChange.OWNERPOSTID = postChange.TOPOSTID;
                        postChange.OWNERDEPARTMENTID = postChange.TODEPARTMENTID;
                        postChange.OWNERCOMPANYID = postChange.TOCOMPANYID;
                        postChange.POSTCHANGCATEGORY = "2";
                        postChange.EMPLOYEEPOSTID = employeePost.EMPLOYEEPOSTID;
                        postChange.POSTCHANGREASON = Utility.GetResourceStr("EMPLOYEEENTRY");
                        postChange.CHANGEDATE = employeeEntry.ENTRYDATE.ToString();
                        postChange.CREATEUSERID = employeeEntry.CREATEUSERID;
                        postChange.CREATECOMPANYID = employeeEntry.CREATECOMPANYID;
                        postChange.CREATEDEPARTMENTID = employeeEntry.CREATEDEPARTMENTID;
                        postChange.CREATEPOSTID = employeeEntry.CREATEPOSTID;
                        dal.AddToContext(postChange);
                        #endregion

                        #region 员工入职报表服务同步 weirui 2012-7-6
                        try
                        {
                            T_HR_EMPLOYEECHANGEHISTORY employeeEntity = new T_HR_EMPLOYEECHANGEHISTORY();
                            employeeEntity.RECORDID = Guid.NewGuid().ToString();
                            //员工ID
                            //employeeEntity.T_HR_EMPLOYEE.EMPLOYEEID = employee.EMPLOYEEID;
                            employeeEntity.T_HR_EMPLOYEEReference.EntityKey =
                                              new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", employeeID);
                            //员工姓名
                            employeeEntity.EMPOLYEENAME = employee.EMPLOYEECNAME;
                            //指纹编号
                            employeeEntity.FINGERPRINTID = employee.FINGERPRINTID;
                            //0.入职1.异动2.离职3.薪资级别变更4.签订合同
                            employeeEntity.FORMTYPE = "0";
                            //记录原始单据id（员工入职表ID）
                            employeeEntity.FORMID = employeeEntry.EMPLOYEEENTRYID;
                            //主岗位非主岗位
                            employeeEntity.ISMASTERPOSTCHANGE = employeePost.ISAGENCY;

                            //备注
                            employeeEntity.REMART = employeeEntry.REMARK;
                            //创建时间
                            employeeEntity.CREATEDATE = DateTime.Now;
                            //所属员工ID
                            employeeEntity.OWNERID = employeeEntry.OWNERID;
                            //所属岗位ID
                            employeeEntity.OWNERPOSTID = employee.OWNERPOSTID;
                            //所属部门ID
                            employeeEntity.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                            //所属公司ID
                            employeeEntity.OWNERCOMPANYID = employee.OWNERCOMPANYID;
                            dal.AddToContext(employeeEntity);

                        }
                        catch (Exception ex)
                        {
                            SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + "员工入职报表服务同步:" + ex.ToString());
                        }
                        #endregion

                        #region 启用系统用户
                        SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient perclient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();
                        SMT.SaaS.BLLCommonServices.PermissionWS.T_SYS_USER user = perclient.GetUserByEmployeeID(employee.EMPLOYEEID);
                        if (user != null)
                        {
                            user.STATE = "1";
                            bool flag = PermClient.SysUserInfoUpdate(user);
                            if (flag)
                            {
                                this.AddDefaultRole(user,employee,employeePost);//添加员工默认角色
                            }
                        }

                        #endregion
                        i = dal.SaveContextChanges();
                        dal.CommitTransaction();
                        #region 转正提醒
                        SMT.Foundation.Log.Tracer.Debug("----调用员工转正提醒-----");
                        EmployeeCheckBLL checkBll = new EmployeeCheckBLL();
                        checkBll.GetEmployeeCheckEngineXml(employeeEntry);
                        #endregion
                        sendMail(employee);//发邮件给运维组
                        AttendanceSolutionAsignBLL attBll = new AttendanceSolutionAsignBLL();
                        attBll.AsignAttendanceSolutionForSingleEmployee(employee);//创建考勤方案
                        #region 调用即时通讯接口
                        //调用即时通讯的接口  
                        string StrMessages = "员工姓名：" + employee.EMPLOYEECNAME + "员工ID：" + employee.EMPLOYEEID;
                        SMT.Foundation.Log.Tracer.Debug("员工入职开始调用即时通讯接口" + StrMessages);
                        AddImInstantMessageForEntry(employee, employeePost);
                        #endregion
                        #endregion
                    }
                    else
                    {
                        i = dal.SaveContextChanges();
                        dal.CommitTransaction();
                    }
                    if (i > 0)
                    {
                        SMT.Foundation.Log.Tracer.Debug("员工入职开始调用我的单据服务");
                        SaveMyRecord(employeeEntry);
                    }
                }
                
                return i;
            }
            catch (Exception e)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug("员工入职出现错误，FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }

        /// <summary>
        /// 员工入职添加默认角色
        /// </summary>
        /// <param name="user"></param>
        /// <param name="employee"></param>
        /// <param name="employeePost"></param>
        private void AddDefaultRole(T_SYS_USER user,T_HR_EMPLOYEE employee,T_HR_EMPLOYEEPOST employeePost)
        {
            try
            {
                SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient perclient = new SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient();
                string companyName = (from e in dal.GetObjects<T_HR_COMPANY>()
                                      where e.COMPANYID == employee.OWNERCOMPANYID
                                      select e.CNAME).FirstOrDefault();
                string comID = employee.OWNERCOMPANYID, deptID = employee.OWNERDEPARTMENTID, postID = employee.OWNERPOSTID;
                string employeeID = employee.EMPLOYEEID;
                string employeePostID = employeePost.EMPLOYEEPOSTID;
                bool flag = perclient.EmployeeEntryAddDefaultRole(user, comID, companyName, deptID, postID,employeeID, employeePostID);
                if (flag)
                {
                    SMT.Foundation.Log.Tracer.Debug("员工入职添加默认角色成功，员工ID:" + employeeID);
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("员工入职添加默认角色失败，员工ID:" + employeeID);
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工入职添加默认角色出现错误" +ex.Message+"，员工ID:"+employee.EMPLOYEEID);
            }
        }

        #region 实现即时通讯接口
        public void AddImInstantMessageForEntry(T_HR_EMPLOYEE employee,T_HR_EMPLOYEEPOST employeepost)
        {
            //用来记录提醒信息
            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯的接口，员工ID" + employee.EMPLOYEEID);
            string StrMessage="";
            try
            {
                DataSyncServiceClient IMCient = new DataSyncServiceClient();
                PermissionServiceClient Client = new PermissionServiceClient();
                T_SYS_DICTIONARY dict = new T_SYS_DICTIONARY();
                T_SYS_USER userInfo = GetUserInfo(employee.EMPLOYEEID);
                #region 定义变量               
                
                string StrSex = "";//性别
                string StrBirthday = "";//生日
                string StrCompanyName = "";//公司名
                //string StrJob = "";//职业
                //string StrEducation = "";//学历
                //string StrNickName = ""; //昵称
                //string StrPWd = "";//密码
                //string StrEmailIsShow = "";//邮箱是否公开
                //string StrProvince = "";//省
                //string StrRegion = "";//地区
                string StrPostName = "";//岗位名称
                string StrPostID = "";//岗位ID
                string StrUserID = employee.EMPLOYEEID;//员工ID
                string StrUserName = userInfo.USERNAME;//改为取系统用户名 而不是 employee.EMPLOYEEENAME;//用户名                
                string StrEmployeeName =employee.EMPLOYEECNAME;//员工姓名                
                string StrTel=employee.OFFICEPHONE;//办公电话
                string StrMobile =employee.MOBILE;//手机号码
                string StrEmail = employee.EMAIL;//邮箱;//邮箱
                using (CompanyBLL cmpBLL=new CompanyBLL())
                {
                    bool isTopSmt = cmpBLL.IsTopCompanySmt(employee.OWNERCOMPANYID);
                    if (isTopSmt)
                    {
                        StrEmail = userInfo.USERNAME + "@sinomaster.com";
                    }
                }
                string StrStreet =employee.CURRENTADDRESS;//街道地址
                string StrPostCode =employee.FAMILYZIPCODE;//邮编
                string StrDeptId = "";
                #endregion
                //获取省份  TOPEDUCATION
                //StrProvince = Client.GetSysDictionaryByCategoryAndValue("PROVINCE", employee.PROVINCE);
                //最高学历
                //StrEducation = Client.GetSysDictionaryByCategoryAndValue("TOPEDUCATION", employee.TOPEDUCATION);
                if (employee.SEX == "0")
                {
                    StrSex = "女";
                }
                if (employee.SEX == "1")
                {
                    StrSex = "男";
                }
                if (employee.BIRTHDAY != null)
                {
                    StrBirthday = System.Convert.ToDateTime( employee.BIRTHDAY).ToShortDateString();
                }

                var ents = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                           where c.EMPLOYEEPOSTID == employeepost.EMPLOYEEPOSTID
                           select new V_EMPLOYEEPOSTBRIEF
                           {
                               EMPLOYEEPOSTID = c.EMPLOYEEPOSTID,
                               POSTID = c.T_HR_POST.POSTID,
                               PostName = c.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME,
                               DepartmentID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                               DepartmentName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                               CompanyID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID,
                               CompanyName = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                               POSTLEVEL = c.POSTLEVEL,
                               ISAGENCY = c.ISAGENCY
                           };
                if (ents.Count() > 0)
                {
                    StrCompanyName = ents.FirstOrDefault().CompanyName;
                    StrPostName = ents.FirstOrDefault().PostName;
                    StrPostID = ents.FirstOrDefault().POSTID;
                    StrDeptId = ents.FirstOrDefault().DepartmentID;
                }
                ImUserDataObject EntObj = new ImUserDataObject();
                EntObj.DeptId = StrDeptId;
                EntObj.Email = StrEmail;
                EntObj.CompanyName = StrCompanyName;
                EntObj.Gender = StrSex;
                EntObj.Mobile = StrMobile;
                EntObj.Telephone = StrTel;
                EntObj.UserId = StrUserID;
                EntObj.UserName = StrUserName;
                EntObj.NickName = StrEmployeeName;
                EntObj.PostId = StrPostID;
                EntObj.PostName = StrPostName;
                EntObj.Address = StrStreet;
                EntObj.Sort = "0";
                StrMessage = IMCient.EmployeeEntry(EntObj);

                //已入职的员工（即有审核通过的入职记录一项以上），则解冻邮箱，而非创建
                var listEntry = GetAllEmployeeEntryByEmployeeid(employee.EMPLOYEEID);
                if (listEntry != null && listEntry.Count() > 0)
                {
                    UnFreezeMail(employee);
                }
                //入职创建邮箱后再修改密码
                PermissionServiceClient psc = new PermissionServiceClient();      
                psc.SysUserInfoUpdate(userInfo);

                
                
                //StrMessage = IMCient.AddOrUpdateImUser(StrUserID, StrUserName, StrPWd,
                //    StrEmployeeName, StrSex, StrBirthday, StrCompanyName, StrJob, StrEducation,
                //    StrTel, StrMobile, StrEmail, StrEmailIsShow, StrProvince, StrRegion,
                //    StrStreet, StrPostCode, StrPostName);
                //SMT.Foundation.Log.Tracer.Debug("ents.FirstOrDefault().DepartmentID：" + ents.FirstOrDefault().DepartmentID);
                //StrMessage +="员工入职入职调部门："+ IMCient.AddOrUpdateDepartmentMember(StrUserID, ents.FirstOrDefault().DepartmentID,StrPostID,StrPostName);
                //SMT.Foundation.Log.Tracer.Debug("StrUserID：" + StrUserID);
                //SMT.Foundation.Log.Tracer.Debug("StrUserName：" + StrUserName);
                //SMT.Foundation.Log.Tracer.Debug("StrPWd：" + StrPWd);
                //SMT.Foundation.Log.Tracer.Debug("StrNickName：" + StrNickName);
                //SMT.Foundation.Log.Tracer.Debug("StrSex：" + StrSex);
                //SMT.Foundation.Log.Tracer.Debug("StrBirthday：" + StrBirthday);
                //SMT.Foundation.Log.Tracer.Debug("StrCompanyName：" + StrCompanyName);
                //SMT.Foundation.Log.Tracer.Debug("StrJob：" + StrJob);
                //SMT.Foundation.Log.Tracer.Debug("StrEducation：" + StrEducation);
                //SMT.Foundation.Log.Tracer.Debug("StrTel：" + StrTel);
                //SMT.Foundation.Log.Tracer.Debug("StrMobile：" + StrMobile);
                //SMT.Foundation.Log.Tracer.Debug("StrEmail：" + StrEmail);
                //SMT.Foundation.Log.Tracer.Debug("StrEmailIsShow：" + StrEmailIsShow);
                //SMT.Foundation.Log.Tracer.Debug("StrProvince：" + StrProvince);
                //SMT.Foundation.Log.Tracer.Debug("StrRegion：" + StrRegion);
                //SMT.Foundation.Log.Tracer.Debug("StrStreet：" + StrStreet);
                //SMT.Foundation.Log.Tracer.Debug("StrPostCode：" + StrPostCode);
                //SMT.Foundation.Log.Tracer.Debug("StrPostName：" + StrPostName);

                SMT.Foundation.Log.Tracer.Debug("调用即时通讯接口EmployeeEntry结果，" + StrMessage);
            }
            catch (Exception ex)
            {
                StrMessage = "员工ID"+employee.EMPLOYEEID + "员工姓名："+employee.EMPLOYEECNAME; 
                SMT.Foundation.Log.Tracer.Debug("AddImInstantMessage产生错误，" + ex.ToString() + StrMessage);
            }
        }

        /// <summary>
        /// 解冻用户的邮箱（用于用户有过入职经历的情况）
        /// </summary>
        /// <param name="employee"></param>
        private void UnFreezeMail(T_HR_EMPLOYEE employee)
        {
            string logger=string.Empty;
            try
            {
                using (MailServiceClient msClient = new MailServiceClient())
                {
                    logger += "开始解冻邮箱：";
                    if (employee != null)
                    {
                        msClient.Freeze(employee.EMPLOYEEID, "unfreeze");
                        logger += "调用Freeze成功-";
                    }
                    else
                    {
                        logger += "员工实体为空-";
                    }
                }
            }
            catch
            {
                logger += "UnFreezeMail出错";
            }
            finally
            {
                SMT.Foundation.Log.Tracer.Debug(logger);
            }
        }
        #endregion

        private bool CheckRepeatAgencyPost(string employeeId)
        {
            bool isNoRepat=false;
            try
            {
                //已有审核通过的在用的岗位，也把这个保存的岗位存为兼职的
                var entPost = from ep in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                              where ep.T_HR_EMPLOYEE.EMPLOYEEID == employeeId
                                 && ep.CHECKSTATE == "2" && ep.EDITSTATE == "1"
                              select ep;

                if (entPost.Count() > 0)
                {
                    isNoRepat = true;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeEntryBLL-AvoidRepeatAgencyPost:" + ex.ToString());
                return false;
            }

            return isNoRepat;
        }

        /// <summary>
        /// 通过员工id获取所有审核通过的员工入职单的列表
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        private List<T_HR_EMPLOYEEENTRY> GetAllEmployeeEntryByEmployeeid(string employeeID)
        {
            List<T_HR_EMPLOYEEENTRY> listEntry = new List<T_HR_EMPLOYEEENTRY>();
            string logger = string.Empty;
            try
            {
                var entryEnt = from e in dal.GetObjects()
                               where e.T_HR_EMPLOYEE.EMPLOYEEID == employeeID && e.CHECKSTATE=="2"
                               select e;
                if (entryEnt.Count() > 1)
                {
                    logger = string.Format("获取ID为{0}的入职单{1}个。", employeeID, entryEnt.Count());
                    listEntry = entryEnt.ToList();
                }
                else
                {
                    logger = string.Format("获取ID为{0}的入职单获取失败", employeeID);
                    listEntry = null;
                }
            }
            catch
            {
                logger = string.Format("获取ID为{0}的入职单获取错误", employeeID);
            }
            finally
            {
                SMT.Foundation.Log.Tracer.Debug(logger);
            }
            return listEntry;
        }

        #region 批量导入员工入职信息

        /// <summary>
        /// 批量添加员工入职信息
        /// </summary>
        /// <param name="listEmpEntry"></param>
        /// <param name="companyID"></param>
        /// <param name="strMsg">错误信息等</param>
        /// <returns></returns>
        public bool AddBatchEmployeeEntry(List<V_EmployeeEntryInfo> listEmpEntry, string companyID, ref string strMsg)
        {
            try
            {
                #region 变量定义及赋值
                bool flag = true;
                string tempStr = string.Empty;
                var company = (from e in dal.GetObjects<T_HR_COMPANY>()
                               where e.COMPANYID == companyID
                               select e).FirstOrDefault();
                string companyName = company.CNAME;
                #endregion
                string strRecord = string.Empty;//记录正确或错误人名
                listEmpEntry.ForEach(it =>
                    {
                        string strTemp = string.Empty, ePostID=string.Empty;
                        it.EmployeeID = Guid.NewGuid().ToString();
                        AddEmployeeInfo(it, ref strTemp, ref ePostID);//添加员工入职信息,员工档案信息，员工岗位信息
                        if (string.IsNullOrWhiteSpace(strTemp))//没有错误才进行下一步
                        {
                            AddUser(it, ref strTemp);//添加系统用户信息
                            AddEemployeeContact(it, ref strTemp);//员工合同
                            AddEmployeeSalay(it, ePostID, ref strTemp);//员工薪资
                            AddEmployeePension(it, ref strTemp);//员工社保
                        }
                            //记录日志
                        SMT.Foundation.Log.Tracer.Debug(" AddBatchEmployeeEntry批量添加部门岗位信息日志记录:员工ＩＤ和姓名为："+ it.EmployeeID+it.EmployeeName+ "输出信息为（ 空为正确）："+strTemp);
                        if (string.IsNullOrWhiteSpace(strTemp))
                        {
                            strRecord += it.EmployeeName+"添加成功\n";
                        }
                        else
                        {
                            strRecord += it.EmployeeName + "添加失败\n";
                            flag = false;
                        }
                    });
                strMsg += strRecord;
                return flag;
            }
            catch (Exception ex)
            {
                strMsg += ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(" AddBatchEmployeeEntry批量添加部门岗位信息出错:" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 添加员工社保档案
        /// </summary>
        /// <param name="empInfo"></param>
        /// <param name="strMsg"></param>
        private void AddEmployeePension(V_EmployeeEntryInfo empInfo, ref string strMsg)
        {
            try
            {
                T_HR_PENSIONMASTER entity = new T_HR_PENSIONMASTER();
                entity.PENSIONMASTERID = Guid.NewGuid().ToString();
                entity.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                entity.T_HR_EMPLOYEE.EMPLOYEEID = empInfo.EmployeeID;
                entity.CARDID = string.Empty;
                entity.COMPUTERNO = string.Empty;
                entity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                entity.EDITSTATE = ((int)EditStates.UnActived).ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = empInfo.EmployeeID;
                entity.OWNERPOSTID = empInfo.PostID;
                entity.OWNERDEPARTMENTID = empInfo.DepartmentID;
                entity.OWNERCOMPANYID = empInfo.CompamyID;
                entity.CREATEUSERID = empInfo.OwnerID;
                entity.CREATEPOSTID = empInfo.OwnerPostID;
                entity.CREATEDEPARTMENTID = empInfo.OwnerDepartmentID;
                entity.CREATECOMPANYID = empInfo.OwnerCompanyID;
                PensionMasterBLL pmBll = new PensionMasterBLL();
                pmBll.PensionMasterAdd(entity, ref strMsg);
            }
            catch (Exception ex)
            {
                strMsg += ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(" AddEemployeeContact批量导入员工入职信息-添加员工社保档案错误:" + ex.ToString());
            }
        }

        /// <summary>
        /// 添加一个空的薪资档案
        /// </summary>
        /// <param name="empInfo"></param>
        /// <param name="ePostID"></param>
        /// <param name="strMsg"></param>
        private void AddEmployeeSalay(V_EmployeeEntryInfo empInfo, string ePostID, ref string strMsg)
        {
            try
            {
                T_HR_SALARYARCHIVE entity = new T_HR_SALARYARCHIVE();
                entity.SALARYARCHIVEID = Guid.NewGuid().ToString();
                entity.EMPLOYEEID = empInfo.EmployeeID;
                entity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                entity.EDITSTATE = ((int)EditStates.UnActived).ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = empInfo.EmployeeID;
                entity.OWNERPOSTID = empInfo.PostID;
                entity.OWNERDEPARTMENTID = empInfo.DepartmentID;
                entity.OWNERCOMPANYID = empInfo.CompamyID;
                entity.CREATEUSERID = empInfo.OwnerID;
                entity.CREATEPOSTID = empInfo.OwnerPostID;
                entity.CREATEDEPARTMENTID = empInfo.OwnerDepartmentID;
                entity.CREATECOMPANYID = empInfo.OwnerCompanyID;
                entity.EMPLOYEENAME = empInfo.EmployeeName;
                entity.POSTLEVEL = Convert.ToDecimal(empInfo.PostLevel);
                entity.EMPLOYEEPOSTID = ePostID;
                SalaryArchiveBLL saBll = new SalaryArchiveBLL();
                saBll.SalaryArchiveAdd(entity);
            }
            catch (Exception ex)
            {
                strMsg += ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(" AddEemployeeContact批量导入员工入职信息-添加薪资档案出错:" + ex.ToString());
            }
        }

        /// <summary>
        /// 添加合同，创建一条空的合同
        /// </summary>
        /// <param name="empInfo"></param>
        /// <param name="strMsg"></param>
        private void AddEemployeeContact(V_EmployeeEntryInfo empInfo, ref string strMsg)
        {
            try
            {
                T_HR_EMPLOYEECONTRACT entity = new T_HR_EMPLOYEECONTRACT();
                entity.EMPLOYEECONTACTID = Guid.NewGuid().ToString();
                entity.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                entity.T_HR_EMPLOYEE.EMPLOYEEID = empInfo.EmployeeID;
                SMT.Foundation.Log.Tracer.Debug("合同员工ID:" + entity.T_HR_EMPLOYEE.EMPLOYEEID);
                entity.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                entity.EDITSTATE = ((int)EditStates.UnActived).ToString();
                entity.CREATEDATE = DateTime.Now;
                entity.OWNERID = empInfo.EmployeeID;
                entity.OWNERPOSTID = empInfo.PostID;
                entity.OWNERDEPARTMENTID = empInfo.DepartmentID;
                entity.OWNERCOMPANYID = empInfo.CompamyID;
                entity.CREATEUSERID = empInfo.OwnerID;
                entity.CREATEPOSTID = empInfo.OwnerPostID;
                entity.CREATEDEPARTMENTID = empInfo.OwnerDepartmentID;
                entity.CREATECOMPANYID = empInfo.OwnerCompanyID;
                EmployeeContractBLL ecBll = new EmployeeContractBLL();
                ecBll.EmployeeContractAdd(entity, ref strMsg);
            }
            catch (Exception ex)
            {
                strMsg += ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(" AddEemployeeContact批量导入员工入职信息-添加合同失败:" + ex.ToString());
            }
        }

        /// <summary>
        /// 添加系统用户
        /// </summary>
        /// <param name="empInfo"></param>
        /// <param name="strMsg"></param>
        private void AddUser(V_EmployeeEntryInfo empInfo,ref string strMsg)
        {
            try
            {
                T_SYS_USER sysUser = new T_SYS_USER();
                sysUser.SYSUSERID = Guid.NewGuid().ToString();
                sysUser.STATE = "1";//启用
                sysUser.CREATEDATE = DateTime.Now;
                sysUser.CREATEUSER = empInfo.OwnerID;
                sysUser.USERNAME = empInfo.UserName;
                sysUser.EMPLOYEEID = empInfo.EmployeeID;
                sysUser.EMPLOYEENAME = empInfo.EmployeeName;
                sysUser.OWNERID = empInfo.EmployeeID;
                sysUser.OWNERPOSTID = empInfo.PostID;
                sysUser.OWNERDEPARTMENTID = empInfo.DepartmentID;
                sysUser.OWNERCOMPANYID = empInfo.CompamyID;
                sysUser.PASSWORD = empInfo.PassWord;
                PermissionServiceClient perclient = new PermissionServiceClient();
                perclient.SysUserInfoAddORUpdate(sysUser, ref strMsg);
            }
            catch (Exception ex)
            {
                strMsg += ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddUser:批量导入员工入职信息-添加系统用户出错" + ex.Message);
            }
        }

        /// <summary>
        /// 添加员工入职信息,员工档案信息，员工岗位信息，薪资密码
        /// </summary>
        /// <param name="empInfo"></param>
        ///  <param name="strMsg"></param>
        /// <param name="ePostID"></param>
        public void AddEmployeeInfo(V_EmployeeEntryInfo empInfo, ref string strMsg, ref string ePostID)
        {
            try
            {
                #region 员工档案
                T_HR_EMPLOYEE emp = new T_HR_EMPLOYEE();
                emp.EMPLOYEEID = empInfo.EmployeeID;
                emp.EMPLOYEECNAME = empInfo.EmployeeName;
                emp.EMPLOYEEENAME = empInfo.UserName;
                emp.SEX = Convert.ToString(empInfo.SexDic);
                emp.IDNUMBER = empInfo.IdNumber;
                emp.HEIGHT = empInfo.Height;
                emp.BANKID = empInfo.Bank;
                emp.BANKCARDNUMBER = empInfo.BankCardNumber;
                emp.REGRESIDENCE = empInfo.RegResidence;
                emp.EMAIL = empInfo.Email;
                emp.MOBILE = empInfo.Mobile;
                emp.CURRENTADDRESS = empInfo.CurrentAddress;
                emp.FAMILYADDRESS = empInfo.FamilyAddress;
                emp.FINGERPRINTID = empInfo.FingerPrintID;
                emp.BIRTHDAY = Convert.ToDateTime(empInfo.Birthday);
                emp.EMPLOYEESTATE = "1";//在职
                emp.EDITSTATE = "1";//生效
                emp.OWNERID = empInfo.EmployeeID;
                emp.OWNERPOSTID = empInfo.PostID;
                emp.OWNERDEPARTMENTID = empInfo.DepartmentID;
                emp.OWNERCOMPANYID = empInfo.CompamyID;
                emp.CREATEDATE = DateTime.Now;
                emp.CREATEUSERID = empInfo.OwnerID;
                emp.CREATEPOSTID = empInfo.OwnerPostID;
                emp.CREATEDEPARTMENTID = empInfo.OwnerDepartmentID;
                emp.CREATECOMPANYID = empInfo.OwnerCompanyID;
                emp.SOCIALSERVICEYEAR = Convert.ToDateTime(empInfo.SocialServiceYear).ToString("yyyy-MM-dd");
                #endregion
                #region 员工岗位
                T_HR_EMPLOYEEPOST ePost = new T_HR_EMPLOYEEPOST();
                ePostID = Guid.NewGuid().ToString();
                ePost.EMPLOYEEPOSTID = ePostID;
                ePost.ISAGENCY = "0";//主岗位
                ePost.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                ePost.T_HR_EMPLOYEE.EMPLOYEEID = empInfo.EmployeeID;
                ePost.T_HR_POST = new T_HR_POST();
                ePost.T_HR_POST.POSTID = empInfo.PostID;
                ePost.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();//审核通过
                ePost.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();//生效
                ePost.CREATEUSERID = empInfo.OwnerID;
                ePost.CREATEDATE = DateTime.Now;
                ePost.POSTLEVEL = Convert.ToDecimal(empInfo.PostLevel);
                #endregion
                #region 员工入职
                T_HR_EMPLOYEEENTRY eEntry = new T_HR_EMPLOYEEENTRY();
                eEntry.EMPLOYEEENTRYID = Guid.NewGuid().ToString();
                eEntry.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                eEntry.T_HR_EMPLOYEE.EMPLOYEEID = empInfo.EmployeeID;
                eEntry.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();//审核通过
                eEntry.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();//生效
                eEntry.ENTRYDATE = Convert.ToDateTime(empInfo.EntryDate);
                eEntry.ONPOSTDATE = Convert.ToDateTime(empInfo.OnPostDate);
                eEntry.PROBATIONPERIOD = 0;//没有试用期
                eEntry.OWNERID = empInfo.EmployeeID;
                eEntry.OWNERPOSTID = empInfo.PostID;
                eEntry.OWNERDEPARTMENTID = empInfo.DepartmentID;
                eEntry.OWNERCOMPANYID = empInfo.CompamyID;
                eEntry.CREATEDATE = DateTime.Now;
                eEntry.CREATEUSERID = empInfo.OwnerID;
                eEntry.CREATEPOSTID = empInfo.OwnerPostID;
                eEntry.CREATEDEPARTMENTID = empInfo.OwnerDepartmentID;
                eEntry.CREATECOMPANYID = empInfo.OwnerCompanyID;
                eEntry.EMPLOYEEPOSTID = ePostID;
                #endregion
                string strRes = this.EmployeeEntryAdd(emp, eEntry, ePost);//原来的方法
                if (strRes== "SAVED")//保存成功则添加薪资密码
                {
                    SalaryLoginBLL bll = new SalaryLoginBLL();
                    bll.AddSalaryPassword(emp.EMPLOYEEID, emp.EMPLOYEECNAME, empInfo.PassWord);
                }
                #region 添加异动信息
                T_HR_EMPLOYEEPOSTCHANGE postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                postChange = new T_HR_EMPLOYEEPOSTCHANGE();
                postChange.POSTCHANGEID = Guid.NewGuid().ToString();
                postChange.EMPLOYEENAME = emp.EMPLOYEECNAME;
                postChange.EMPLOYEECODE = emp.EMPLOYEECODE;
                postChange.T_HR_EMPLOYEEReference.EntityKey =
                                   new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", emp.EMPLOYEEID);
                postChange.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                postChange.TOCOMPANYID = eEntry.OWNERCOMPANYID;
                postChange.TODEPARTMENTID = eEntry.OWNERDEPARTMENTID;
                postChange.TOPOSTID = eEntry.OWNERPOSTID;
                postChange.ISAGENCY = "0";
                postChange.OWNERID = emp.EMPLOYEEID;
                postChange.OWNERPOSTID = postChange.TOPOSTID;
                postChange.OWNERDEPARTMENTID = postChange.TODEPARTMENTID;
                postChange.OWNERCOMPANYID = postChange.TOCOMPANYID;
                postChange.POSTCHANGCATEGORY = "2";
                postChange.EMPLOYEEPOSTID = ePost.EMPLOYEEPOSTID;
                postChange.POSTCHANGREASON = Utility.GetResourceStr("EMPLOYEEENTRY");
                postChange.CHANGEDATE = eEntry.ENTRYDATE.ToString();
                postChange.CREATEUSERID = eEntry.CREATEUSERID;
                postChange.CREATECOMPANYID = eEntry.CREATECOMPANYID;
                postChange.CREATEDEPARTMENTID = eEntry.CREATEDEPARTMENTID;
                postChange.CREATEPOSTID = eEntry.CREATEPOSTID;
                dal.AddToContext(postChange);
                #endregion
            }
            catch (Exception ex)
            {
                strMsg += ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " AddEmployeeEntry:批量导入员工入职信息-添加员工入职信息错误" + ex.Message);
            }
        }

        /// <summary>
        /// 获取员工入职批量导入信息
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="companyID"></param>
        /// <param name="empInfoDic"></param>
        /// <returns></returns>
        public List<V_EmployeeEntryInfo> ImportEmployeeEntry(string strPath, string companyID, Dictionary<string, string> empInfoDic)
        {
            try
            {
                #region 获取Excel数据并转换成V_EmployeeEntryInfo类型
                DataTable dt = new DataTable();
                dt = Utility.GetDataFromFile(strPath, 1, 1);//都第一列第一行数据，以验证模板
                var chEnt = from o in dt.AsEnumerable()
                            select new
                            {
                                rowName = o["col0"].ToString().Trim()
                            };
                if (chEnt == null ||  chEnt.Count() <= 0 || chEnt.FirstOrDefault().rowName != "员工姓名")
                {
                    return null;
                }
                dt = Utility.GetDataFromFile(strPath, 18, 2);
                var ent = from o in dt.AsEnumerable()
                          select new V_EmployeeEntryInfo
                          {
                              EmployeeName = o["col0"].ToString().Trim(),
                              IdNumber = o["col1"].ToString().Trim(),
                              Sex = o["col2"].ToString().Trim(),
                              CompanyName = o["col3"].ToString().Trim(),
                              DepartmentName = o["col4"].ToString().Trim(),
                              PostName = o["col5"].ToString().Trim(),
                              PostLevel = o["col6"].ToString().Trim(),
                              EntryDate = o["col7"].ToString().Trim(),
                              OnPostDate = o["col8"].ToString().Trim(),
                              SocialServiceYear = o["col9"].ToString().Trim(),
                              Birthday = o["col10"].ToString().Trim(),
                              Height = o["col11"].ToString().Trim(),
                              FingerPrintID = o["col12"].ToString().Trim(),
                              Bank = o["col13"].ToString().Trim(),
                              BankCardNumber = o["col14"].ToString().Trim(),
                              RegResidence = o["col15"].ToString().Trim(),
                              FamilyAddress = o["col16"].ToString().Trim(),
                              CurrentAddress = o["col17"].ToString().Trim(),
                              Mobile = o["col18"].ToString().Trim(),
                              OwnerID = empInfoDic["ownerID"],
                              OwnerPostID = empInfoDic["ownerPostID"],
                              OwnerDepartmentID = empInfoDic["ownerDepartmentID"],
                              OwnerCompanyID = empInfoDic["ownerCompanyID"]
                          };
                #endregion

                if (ent != null && ent.Count() > 0)
                {
                    List<V_EmployeeEntryInfo> listEmpInfo = ent.ToList();
                    var company = (from e in dal.GetObjects<T_HR_COMPANY>()
                                   where e.COMPANYID == companyID
                                   select e).FirstOrDefault();
                    string companyName = company.CNAME;
                    //遍历进行身份证验证，账号密码邮箱在前台进行生成
                    foreach (var item in listEmpInfo)
                    {
                        string strMsg = string.Empty;
                        ValidInfo(item, ref strMsg);//必填项验证
                        #region 公司判断
                        if (item.CompanyName != companyName)
                        {
                            strMsg += "模板公司与所选公司不一致";
                        }
                        item.CompamyID = companyID;
                        #endregion
                        #region 部门岗位判断，并返回部门ID和岗位ID
                        string deptID=string.Empty,postID=string.Empty;
                        bool isdepo = GetEmloyeePostInfo(item, ref strMsg, ref deptID,ref postID);
                        if (isdepo)
                        {
                            item.DepartmentID = deptID;
                            item.PostID = postID;
                        }
                        else
                        {
                            item.DepartmentName += "没有找到该部门";
                            item.PostName += "没有找到该岗位";
                        }
                        #endregion
                        #region 岗位编制判断
                        PostBLL pBll = new PostBLL();
                        if (!string.IsNullOrWhiteSpace(postID))
                        {
                            int num = pBll.GetPostNumber(postID);
                            if (num<=0)
                            {
                                item.PostName += "该岗位人员编制已满";
                                strMsg += "入职岗位人员编制已满";
                            }
                        }
                        
                        #endregion
                        #region 身份证判断
                        bool isExist = ValidEmployeeIdNumber(item.IdNumber, ref strMsg);
                        if (!isExist)
                        {
                            item.IdNumber = "身份证不合法" + item.IdNumber;
                        }
                        #endregion
                        #region 产生密码
                        string strCarID = item.IdNumber;
                        if (strCarID.Length > 6)
                        {
                           string strPassWord = "smt" + strCarID.Substring(strCarID.Length - 6);
                           item.PassWord = Utility.Encrypt(strPassWord);
                        }
                        else
                        {
                            string strPassWord = "smt" + strCarID;
                            item.PassWord = Utility.Encrypt(strPassWord);
                        }
                        #endregion
                        item.ErrorMsg += strMsg;
                    }
                    return listEmpInfo;
                }
                return null;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(" ImportEmployeeEntry获取员工入职批量导入信息Excel数据错误:" + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 获取员工部门ID，岗位ID等信息
        /// </summary>
        /// <param name="empInfo"></param>
        /// <param name="strMsg"></param>
        /// <param name="deptID"></param>
        /// <param name="postID"></param>
        /// <returns></returns>
        private bool GetEmloyeePostInfo(V_EmployeeEntryInfo empInfo, ref string strMsg, ref string deptID, ref string postID)
        {
            try
            {
                bool flag = true;
                var entDep = (from e in dal.GetObjects<T_HR_DEPARTMENT>()
                              where e.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME == empInfo.DepartmentName
                              && e.T_HR_COMPANY.COMPANYID == empInfo.CompamyID
                              select e).FirstOrDefault();
                if (entDep == null)
                {
                    strMsg += "没有找到员工入职的部门";
                    flag = false;
                }
                if (entDep != null)
                {
                    var entPost = (from e in dal.GetObjects<T_HR_POST>()
                                   where e.T_HR_POSTDICTIONARY.POSTNAME == empInfo.PostName
                                   && e.COMPANYID == empInfo.CompamyID
                                   && e.T_HR_DEPARTMENT.DEPARTMENTID == entDep.DEPARTMENTID
                                   select e).FirstOrDefault();
                    if (entPost == null)
                    {
                        strMsg += "没有找到员工入职的岗位";
                        flag = false;
                    }
                    else
                    {
                        deptID = entDep.DEPARTMENTID;
                        postID = entPost.POSTID;
                    }
                }
                return flag;
            }
            catch (Exception ex)
            {
                strMsg += "查找员工入职的部门和岗位出错";
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetEmloyeePostInfo:批量导入员工入职信息-获取员工部门ID，岗位ID等信息" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 验证用户名是否存在，存在则返回一个新的
        /// </summary>
        /// <param name="listEmpInfo"></param>
        /// <returns></returns>
        public List<V_EmployeeEntryInfo> ValidUserNameIsExist(List<V_EmployeeEntryInfo> listEmpInfo)
        {
            try
            {
                listEmpInfo.ForEach(it =>
                    {
                        SMT.SaaS.BLLCommonServices.PermissionWS.PermissionServiceClient perClient = new PermissionServiceClient();
                        it.UserName = perClient.GetUserNameIsExistNameAddOne(it.UserName, "123abc");//这里员工ID用123abc代替，因为前面已经用员工身份证进行过判断，所以用户表里面的数据不会重复
                    });
                return listEmpInfo;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(" ValidUserNameIsExist验证用户名是否存在错误:" + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 验证数据必填项
        /// </summary>
        /// <param name="info"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private bool ValidInfo(V_EmployeeEntryInfo info, ref string strMsg)
        {
            bool flag = true;
            if (string.IsNullOrWhiteSpace(info.EmployeeName) || string.IsNullOrWhiteSpace(info.IdNumber) || string.IsNullOrWhiteSpace(info.Sex))
            {
                flag = false;
            }
            if (string.IsNullOrWhiteSpace(info.CompanyName)||string.IsNullOrWhiteSpace(info.DepartmentName) || string.IsNullOrWhiteSpace(info.PostName) || string.IsNullOrWhiteSpace(info.PostLevel))
            {
                flag = false;
            }
            if (info.EntryDate==null||info.OnPostDate==null||info.SocialServiceYear==null)
            {
                flag = false;
            }
            if (!flag)
            {
                strMsg += "请检查必填项是否输入有效值";
            }
            return flag;
        }
        /// <summary>
        /// 根据身份证号验证员工是否有入职信息
        /// </summary>
        /// <param name="IdNumber"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private bool ValidEmployeeIdNumber(string IdNumber, ref string strMsg)
        {
            //检查身份证
            string blackMessage = "";
            string[] leaveMessage = new string[2];
            EmployeeBLL bll = new EmployeeBLL();
            bool flag = bll.EmployeeIsEntry(IdNumber.Trim().ToUpper(), ref blackMessage, ref leaveMessage);
            if (!false)
            {
                if (!string.IsNullOrWhiteSpace( leaveMessage[0]))
                {
                    leaveMessage[0] = "该身份证员工已经在职";
                }
                if (!string.IsNullOrWhiteSpace( leaveMessage[1]))
                {
                    leaveMessage[1] = "该身份证员工离职时间小于指定的时间";
                }
                strMsg += blackMessage + leaveMessage[0] + leaveMessage[1];
            }
            return flag;
        }
        #endregion
    }
}
