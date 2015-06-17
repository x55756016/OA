using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.DAL;
using TM_SaaS_OA_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using System.Data;
using System.Reflection;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public class EmployeeBalancePostBLL : BaseBll<T_HR_EMPLOYEESALARYPOSTASIGN>, IOperate
    {
        /// <summary>
        /// 添加员工薪资岗位变更
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="details"></param>
        /// <param name="flag">1：第一次保存 2：第2次保存</param>
        /// <returns></returns>
        public string BalancePostADD(T_HR_EMPLOYEESALARYPOSTASIGN obj, List<T_HR_BALANCEPOSTDETAIL> details, string flag, ref string strResult)
        {
            string strReturn = string.Empty;
            try
            {
                obj.CREATEDATE = DateTime.Now;
                obj.EDITSTATE = "0";
                obj.EMPLOYEECOUNT = details.Count();
                string str = string.Empty;
                
                str = CheckEmployeeBalancePost(obj, details,ref strResult);
                if (flag == "1")
                {                    
                    if (!string.IsNullOrEmpty(str))
                    {
                        return str;
                    }
                }
                obj.NOTESCONTENT = strResult;      
                bool i =Add(obj);
                if (i )
                {
                    SMT.Foundation.Log.Tracer.Debug("BalancePostADD,主记录添加成功");
                }
                else
                {
                    strReturn = "添加薪资岗位变更申请失败";
                    return strReturn;
                }
                bool blRetun =BalancePostsLotsofADD(details);
                if (blRetun)
                {
                    SMT.Foundation.Log.Tracer.Debug("BalancePostADD,明细添加成功");
                }
                else
                {
                    strReturn = "添加明细失败";
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("BalancePostADD:" + ex.Message);
                throw ex;
            }
            return strReturn;
        }

        /// <summary>
        /// 检查是有其它员工没有分配薪资结算岗位
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public string CheckEmployeeBalancePost(T_HR_EMPLOYEESALARYPOSTASIGN obj, List<T_HR_BALANCEPOSTDETAIL> details,ref string strCheck)
        {
            string str = string.Empty;
            try
            {
                //岗位ID
                List<string> listPosts = new List<string>();
                foreach (var ent in details)
                {
                    listPosts.Add(ent.EMPLOYEEPOSTID);
                }
                //生效的薪资结算或考勤结算变更
                var oldPosts = from ent in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                               where listPosts.Contains(ent.EMPLOYEEPOSTID) && ent.EDITSTATE =="1"
                               select ent;
                List<string> listOldAsigns = new List<string>();
                foreach (var ent in oldPosts)
                {
                    if (!listOldAsigns.Contains(ent.BALANCEPOSTASIGNID))
                    {
                        listOldAsigns.Add(ent.BALANCEPOSTASIGNID);
                    }
                }
                var oldAsings = from ent in dal.GetObjects<T_HR_EMPLOYEESALARYPOSTASIGN>()
                                where ent.EDITSTATE == "1" && listOldAsigns.Contains(ent.EMPLOYEESALARYPOSTASIGNID)
                                select ent;
                if (oldPosts.Count() > 0)
                {
                    foreach (var ent in oldPosts)
                    {
                        var existNew = from a in details
                                       where a.EMPLOYEEPOSTID == ent.EMPLOYEEPOSTID
                                       select a;
                        if (existNew.Count() > 0)
                        {
                            T_HR_BALANCEPOSTDETAIL newDetail = existNew.FirstOrDefault();
                            var existOldAsign = from b in oldAsings
                                                where b.EMPLOYEESALARYPOSTASIGNID == ent.BALANCEPOSTASIGNID
                                                select b;
                            if (existOldAsign.Count() > 0)
                            {
                                T_HR_EMPLOYEESALARYPOSTASIGN oldPostAsign = existOldAsign.FirstOrDefault();
                                if (ent.SALARYSET == "1")
                                {
                                    if (newDetail.SALARYSET == "1")
                                    {
                                        if (oldPostAsign.BALANCEPOSTNAME != obj.BALANCEPOSTNAME)
                                        {
                                            strCheck += "岗位：" + ent.EMPLOYEEPOSTNAME + "原来薪资结算岗位为：" + oldPostAsign.BALANCEPOSTNAME + ",现变更为："+ obj.BALANCEPOSTNAME+"。";
                                        }
                                    }
                                    else
                                    {                                        
                                        strCheck += "岗位：" + ent.EMPLOYEEPOSTNAME + "原来薪资结算岗位为：" + oldPostAsign.BALANCEPOSTNAME + ",现变更为不设置。";                                        
                                    }
                                }
                                else
                                {
                                    if (newDetail.SALARYSET == "1")
                                    {                                        
                                        strCheck += "岗位：" + ent.EMPLOYEEPOSTNAME + "原来薪资结算岗位没有设置，现变更为：" + obj.BALANCEPOSTNAME + "。";                                        
                                    }                                    
                                }

                                if (ent.ATTENDANCESET == "1")
                                {
                                    if (newDetail.SALARYSET == "1")
                                    {
                                        if (oldPostAsign.BALANCEPOSTNAME != obj.BALANCEPOSTNAME)
                                        {
                                            strCheck += "岗位：" + ent.EMPLOYEEPOSTNAME + "原来考勤结算岗位为：" + oldPostAsign.BALANCEPOSTNAME + ",现变更为：" + obj.BALANCEPOSTNAME + "。";
                                        }
                                    }
                                    else
                                    {
                                        strCheck += "岗位：" + ent.EMPLOYEEPOSTNAME + "原来考勤结算岗位为：" + oldPostAsign.BALANCEPOSTNAME + ",现变更为不设置。";
                                    }
                                }
                                else
                                {
                                    if (newDetail.SALARYSET == "1")
                                    {
                                        strCheck += "岗位：" + ent.EMPLOYEEPOSTNAME + "原来考勤结算岗位没有设置，现变更为：" + obj.BALANCEPOSTNAME + "。";
                                    }
                                }
                                
                                //strCheck += ent.EMPLOYEEPOSTNAME + "原来结算岗位为：" + oldPostAsign.BALANCEPOSTNAME + "。现变更为：" + obj.BALANCEPOSTNAME+"。";
                            }
                        }
                    }
                }
                #region 注释掉以前的员工判断，现在改成了结算岗位
                
                
                //var entEmployees = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                //                   where (ent.EMPLOYEESTATE == "0" || ent.EMPLOYEESTATE == "1" || ent.EMPLOYEESTATE == "3")
                //                   && ent.OWNERCOMPANYID == obj.OWNERCOMPANYID
                //                   select ent;                
                //List<string> noIds = new List<string>();
                //List<string> DetailIds = new List<string>();
                //if (entEmployees.Count() > 0)
                //{
                //    foreach (var ent in entEmployees)
                //    {
                //        noIds.Add(ent.EMPLOYEEID);
                //    }
                //    foreach (var ent in details)
                //    {
                //        DetailIds.Add(ent.EMPLOYEEID);
                //    }
                //    if (noIds.Count() > 0)
                //    {
                //        //获取已存在的记录
                //        var entArchives = from ent in dal.GetObjects<T_HR_SALARYARCHIVE>()
                //                          where  DetailIds.Contains(ent.EMPLOYEEID) && ent.CHECKSTATE =="2"
                //                          select ent;
                //        if (entArchives.Count() > 0)
                //        {
                //            List<string> listPosts = new List<string>();
                //            foreach (var ent in entArchives)
                //            {
                //                listPosts.Add(ent.BALANCEPOSTID);                                
                //            }
                //            var entPosts = from e in dal.GetObjects<T_HR_POST>().Include("T_HR_POSTDICTIONARY")
                //                           where listPosts.Contains(e.POSTID)
                //                           select e;
                //            foreach (var ent in DetailIds)
                //            {
                //                var single = entArchives.Where(s => s.EMPLOYEEID == ent);
                //                if (single != null)
                //                {
                //                    var employeeArchive = single.OrderByDescending(s=>s.CREATEDATE).FirstOrDefault();
                //                    if (employeeArchive != null)
                //                    {
                //                        string postName = string.Empty;
                //                        if (employeeArchive.BALANCEPOSTID != null)
                //                        {
                //                            T_HR_POST post = entPosts.Where(s => s.POSTID == employeeArchive.BALANCEPOSTID).FirstOrDefault();
                //                            if (post != null)
                //                            {
                //                                if (post.T_HR_POSTDICTIONARY != null)
                //                                {
                //                                    postName = post.T_HR_POSTDICTIONARY.POSTNAME;
                //                                }
                //                            }
                //                            string strEmployee = employeeArchive.EMPLOYEENAME + "-原来的结算岗位为：" + postName;
                //                            if (strCheck.IndexOf(strEmployee) == -1)
                //                            {
                //                                strCheck += strEmployee + ",";
                //                            }
                //                        }
                //                    }
                //                }

                //            }
                //            if (!string.IsNullOrEmpty(strCheck))
                //            {
                //                if (strCheck.IndexOf(",") > -1)
                //                {
                //                    if (strCheck.LastIndexOf(",") == strCheck.Length - 1)
                //                    {
                //                        strCheck = strCheck.Substring(0, strCheck.Length - 1);
                //                    }
                //                }
                //                //strCheck = strCheck.Substring(0, strCheck.Length-1);
                //            }
                //        }
                //        List<string> noChanges = new List<string>();
                //        foreach (var ent in noIds)
                //        {
                //            var noEmployees = from ent1 in details
                //                              where ent1.EMPLOYEEID == ent
                //                              select ent1;
                //            if (noEmployees.Count() == 0)
                //            {
                //                noChanges.Add(ent); 
                //            }
                //        }
                //        //没在岗位变更的员工
                //        var entDetails = from ent in entEmployees
                //                         where noChanges.Contains(ent.EMPLOYEEID)
                //                         select ent;
                //        ////存在且生效的数据
                //        var entExists = from ent in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                //                        where noChanges.Contains(ent.EMPLOYEEID)
                //                        && ent.EDITSTATE == "1"
                //                        select ent;

                //        if (entDetails.Count() > 0)
                //        {
                //            foreach (var ent in entDetails)
                //            {
                //                var exist = from a in entExists
                //                            where a.EMPLOYEEID == ent.EMPLOYEEID
                //                            select a;
                //                if (exist.Count() == 0)
                //                {
                //                    if (str.IndexOf(ent.EMPLOYEECNAME) == -1)
                //                    {
                //                        str += ent.EMPLOYEECNAME + ",";
                //                    }
                //                }
                //            }
                //            if (!string.IsNullOrEmpty(str))
                //            {
                //                if (str.IndexOf(",") > -1)
                //                {
                //                    if (str.LastIndexOf(",") == str.Length - 1)
                //                    {
                //                        str = str.Substring(0, str.Length - 1);
                //                    }
                //                }
                //                if (!string.IsNullOrEmpty(str))
                //                {
                //                    str += "。薪资结算岗位没有变更，是否保存";
                //                }
                //            }
                //        }
                //    } 
                //}
                #endregion
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("CheckEmployeeBalancePost:" + ex.Message);
                str = "error";
            }
            return str;
        }
        /// <summary>
        /// 批量添加员工薪资结算岗位
        /// </summary>
        /// <param name="obj"> 薪资结算岗位实例集合</param>
        public bool BalancePostsLotsofADD(List<T_HR_BALANCEPOSTDETAIL> objs)
        {
            try
            {
                foreach (T_HR_BALANCEPOSTDETAIL obj in objs)
                {
                    obj.CREATEDATE = DateTime.Now;
                    obj.EDITSTATE = "0";
                    dal.AddToContext(obj);                     
                }
                return dal.SaveContextChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("BalancePostsLotsofADD:" + ex.Message);
                throw ex;
            }
        }



        /// <summary>
        /// 批量添加员工薪资结算岗位
        /// </summary>
        /// <param name="obj"> 薪资结算岗位实例集合</param>
        public bool BalancePostsLotsofUpdate(T_HR_EMPLOYEESALARYPOSTASIGN asign,List<T_HR_BALANCEPOSTDETAIL> objs)
        {
            bool isReturn = true;
            try
            {
                var ents = from ent in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                           where ent.BALANCEPOSTASIGNID == asign.EMPLOYEESALARYPOSTASIGNID
                           select ent;
                //UpdateCheckState("T_HR_EMPLOYEESALARYPOSTASIGN", "EMPLOYEESALARYPOSTASIGNID", asign.EMPLOYEESALARYPOSTASIGNID, "1");
                int addCount = 0;
                foreach (T_HR_BALANCEPOSTDETAIL obj in objs)
                {
                    var exist = from ent in ents
                                where ent.BALANCEPOSTDETAIL == obj.BALANCEPOSTDETAIL
                                select ent;
                    if (exist.Count() == 0)
                    {
                        obj.CREATEDATE = DateTime.Now;
                        obj.EDITSTATE = "0";
                        dal.AddToContext(obj);
                        addCount++;
                    }
                    else
                    {
                        //修改
                        T_HR_BALANCEPOSTDETAIL old = exist.FirstOrDefault();
                        Utility.CloneEntity<T_HR_BALANCEPOSTDETAIL>(obj, old);
                        old.UPDATEDATE = DateTime.Now;
                        dal.UpdateFromContext(old);
                        addCount++;
                    }
                }
                //删除没传递过来的数据
                int delCount = 0;
                foreach (T_HR_BALANCEPOSTDETAIL obj in ents)
                {
                    var exist = from ent in objs
                                where ent.BALANCEPOSTDETAIL == obj.BALANCEPOSTDETAIL
                                select ent;
                    if (exist.Count() == 0)
                    {
                        obj.CREATEDATE = DateTime.Now;
                        dal.DeleteFromContext(obj);
                        delCount++;
                    }
                }

                if (addCount > 0 || delCount >0)
                {
                    return dal.SaveContextChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("BalancePostsLotsofADD:" + ex.Message);
                throw ex;
            }
            return isReturn;
        }

        /// <summary>
        /// 更新员工薪资岗位变更
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objs"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public string BalancePostUpdate(T_HR_EMPLOYEESALARYPOSTASIGN obj, List<T_HR_BALANCEPOSTDETAIL> objs,string flag,ref string strResult)
        {
            string strReturn = string.Empty;
            try
            {
                var ent = from a in dal.GetObjects()
                          where a.EMPLOYEESALARYPOSTASIGNID == obj.EMPLOYEESALARYPOSTASIGNID
                          select a;
                string str = string.Empty;                
                str = CheckEmployeeBalancePost(obj, objs, ref strResult);
                if (flag == "1")
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        return str;
                    }                    
                }                
                if (ent.Count() > 0)
                {
                    T_HR_EMPLOYEESALARYPOSTASIGN tmpEnt = ent.FirstOrDefault();
                    obj.UPDATEDATE = DateTime.Now;
                    Utility.CloneEntity<T_HR_EMPLOYEESALARYPOSTASIGN>(obj, tmpEnt);
                    tmpEnt.EMPLOYEECOUNT = objs.Count();
                    tmpEnt.NOTESCONTENT = strResult;
                    int i =Update(tmpEnt);
                    if (i == 0)
                    {
                        strReturn = "更新失败";
                    }
                    bool blResult = BalancePostsLotsofUpdate(obj, objs);
                    if (!blResult)
                    {
                        strReturn = "更新明细失败";
                    }
                }
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug(e.Message);
                e.Message.ToString();
                //throw e;
            }
            return strReturn;
        }
        /// <summary>
        /// 删除员工薪资结算岗位
        /// </summary>
        /// <param name="IDs">薪资结算岗位ID</param>
        /// <returns></returns>
        public int BalancePostDelete(string[] IDs)
        {
            int i = 0;
            try
            {
                
                var entDetails = from ent in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                                 where IDs.Contains(ent.BALANCEPOSTASIGNID)
                                 select ent;
                if (entDetails.Count() > 0)
                {
                    foreach (var ent in entDetails)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }
                foreach (string id in IDs)
                {
                    var ents = from e in dal.GetObjects<T_HR_EMPLOYEESALARYPOSTASIGN>()
                               where e.EMPLOYEESALARYPOSTASIGNID == id
                               select e;
                    var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                    if (ent != null)
                    {
                        //dal.DeleteFromContext(ent);                        
                        Delete(ent);
                    }
                    //TODO:删除项目所包含的明细
                }
                i = dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeAddSumDelete:" + ex.Message);
            }

            return i;
        }

        /// <summary>
        /// 删除薪资岗位变更信息
        /// </summary>
        /// <param name="IDs"></param>
        /// <returns></returns>
        public int BalancePostDetailDelete(string[] IDs)
        {
            int i = 0;
            try
            {

                var entDetails = from ent in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                                 where IDs.Contains(ent.BALANCEPOSTASIGNID)
                                 select ent;
                if (entDetails.Count() > 0)
                {
                    foreach (var ent in entDetails)
                    {
                        dal.DeleteFromContext(ent);
                    }
                }                
                i = dal.SaveContextChanges();
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("BalancePostDetailDelete:" + ex.Message);
            }
            return i;
        }
        
        /// <summary>
        /// 根据ID获取员工薪资结算岗位
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEESALARYPOSTASIGN GetBalancePostByID(string ID)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYPOSTASIGN>()
                       where a.EMPLOYEESALARYPOSTASIGNID == ID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 获取员工薪资岗位变更信息
        /// </summary>
        /// <param name="asignID">变更ID</param>
        /// <param name="asign">变更信息</param>
        /// <returns>返回明细信息集合</returns>
        public List<T_HR_BALANCEPOSTDETAIL> GetBalancePostsByBalanceID(string asignID, ref T_HR_EMPLOYEESALARYPOSTASIGN asign)
        {
            List<T_HR_BALANCEPOSTDETAIL> details = new List<T_HR_BALANCEPOSTDETAIL>();
            try
            {
                //int b = UpdateCheckState("","","b91bf0f0-9b22-465f-a2ad-59fc392129cf","2");
                var ents = from ent in dal.GetObjects<T_HR_EMPLOYEESALARYPOSTASIGN>()
                           where ent.EMPLOYEESALARYPOSTASIGNID == asignID
                           select ent;
                if (ents.Count() > 0)
                {
                    asign = ents.FirstOrDefault();
                }
                var listDetails = from ent in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                                  where ent.BALANCEPOSTASIGNID == asignID
                                  select ent;
                if (listDetails.Count() > 0)
                {
                    details = listDetails.ToList();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("GetBalancePostsByBalanceID:" + ex.Message); 
            }
            return details; 
        }
        /// <summary>
        /// 员工薪资结算岗位视图
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public V_EmployeeAddsumView GetEmployeeAddSumViewByID(string ID)
        {
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                       join b in dal.GetObjects<T_HR_COMPANY>() on a.OWNERCOMPANYID equals b.COMPANYID
                       join c in dal.GetObjects<T_HR_DEPARTMENT>() on a.OWNERDEPARTMENTID equals c.DEPARTMENTID
                       join d in dal.GetObjects<T_HR_POST>() on a.OWNERPOSTID equals d.POSTID
                       where a.ADDSUMID == ID
                       select new V_EmployeeAddsumView
                       {
                           ADDSUMID = a.ADDSUMID,
                           EMPLOYEECODE = a.EMPLOYEECODE,
                           EMPLOYEENAME = a.EMPLOYEENAME,
                           PROJECTNAME = a.PROJECTNAME,
                           PROJECTCODE = a.PROJECTCODE,
                           PROJECTMONEY = a.PROJECTMONEY,
                           SYSTEMTYPE = a.SYSTEMTYPE,
                           DEALYEAR = a.DEALYEAR,
                           DEALMONTH = a.DEALMONTH,
                           CHECKSTATE = a.CHECKSTATE,
                           REMARK = a.REMARK,
                           OWNERID = a.OWNERID,
                           OWNERPOSTID = a.OWNERPOSTID,
                           OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = a.OWNERCOMPANYID,
                           CREATEPOSTID = a.CREATEPOSTID,
                           CREATEDEPARTMENTID = a.CREATEDEPARTMENTID,
                           CREATECOMPANYID = a.CREATECOMPANYID,
                           CREATEDATE = a.CREATEDATE,
                           CREATEUSERID = a.CREATEUSERID,
                           UPDATEUSERID = a.UPDATEUSERID,
                           UPDATEDATE = a.UPDATEDATE,
                           EMPLOYEEID = a.EMPLOYEEID,
                           MONTHLYBATCHID = a.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID,
                           CompanyName = b.CNAME,
                           PostName = d.T_HR_POSTDICTIONARY.POSTNAME,
                           DepartmentName = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                       };
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
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
        public new IQueryable<T_HR_EMPLOYEESALARYPOSTASIGN> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {

            IQueryable<T_HR_EMPLOYEESALARYPOSTASIGN> ents = dal.GetTable(); ;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEESALARYPOSTASIGN>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 带有权限的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页 </param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件的参数 </param>
        /// <param name="pageCount">总页数</param>
        /// <param name="userID">用户ID</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEESALARYPOSTASIGN> QueryWithPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string employeeName,string postID, string userID, string CheckState, int orgtype, string orgid)
        {
            bool isAudit = false;//因为批量审核和查询条件都使用该方法，所以批量审核时加一个标识，在这里进行区分
            if (!string.IsNullOrWhiteSpace(filterString) && filterString == "audit")
            {
                filterString = "";
                isAudit = true;
            }
            string year = string.Empty;
            string filter2 = "";
            List<string> queryParasDt = new List<string>();
            DateTime dtNull = Convert.ToDateTime("0001-1-1 0:00:00");

            List<object> queryParas = new List<object>();

            List<T_HR_EMPLOYEESALARYPOSTASIGN> ent = new List<T_HR_EMPLOYEESALARYPOSTASIGN>();
            //IQueryable<T_HR_EMPLOYEESALARYPOSTASIGN> ents = GetBalancePostFilter(orgtype, orgid);
            IQueryable<T_HR_EMPLOYEESALARYPOSTASIGN> ents = dal.GetObjects();
            //var en = ents.GroupBy(y => y.EMPLOYEESALARYPOSTASIGNID).Select(g => new { group = g.Key, groupcontent = g });
            //foreach (var v in en)
            //{
            //    ent.Add(v.groupcontent.FirstOrDefault());
            //}
            //ents = ent.AsQueryable();

            queryParas.AddRange(paras);
            if (CheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYPOSTASIGN");

                if (!string.IsNullOrEmpty(CheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " and ";
                    }
                    if (isAudit && CheckState == Convert.ToString(Convert.ToInt32(CheckStates.All)))//表示为批量审核，这是也加载审核未通过的数据
                    {
                        CheckState = Convert.ToString(Convert.ToInt32(CheckStates.UnSubmit));
                        filterString += "(CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(CheckState);
                        filterString += " or CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(Convert.ToInt32(CheckStates.UnApproved).ToString());//并且未提交的或者审核未通过的，把审核未通过的也算在批量审核内
                        filterString += ")";
                    }
                    else
                    {
                        filterString += "CHECKSTATE == @" + queryParas.Count();
                        queryParas.Add(CheckState);
                    }
                }
            }
            else
            {
                SetFilterWithflow("EMPLOYEESALARYPOSTASIGNID", "T_HR_EMPLOYEESALARYPOSTASIGN", userID, ref CheckState, ref  filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }

            //IQueryable<T_HR_EMPLOYEEADDSUM> ents = DataContext.T_HR_EMPLOYEEADDSUM;
            if (ents == null)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(employeeName))
            {
                var entDetails = from ent1 in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                                 //where ent1.EMPLOYEENAME.Contains(employeeName)
                                 where employeeName.Contains(ent1.EMPLOYEENAME)
                                 select ent1;
                List<string> listAsigns = new List<string>();
                if (entDetails.Count() > 0)
                {
                    foreach (var aa in entDetails)
                    {
                        listAsigns.Add(aa.BALANCEPOSTASIGNID);
                    }
                }
                if (listAsigns.Count() > 0)
                {
                    ents = ents.Where(s=>listAsigns.Contains(s.EMPLOYEESALARYPOSTASIGNID));
                }
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            int t = 0; 
            ents = Utility.Pager<T_HR_EMPLOYEESALARYPOSTASIGN>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }


        /// <summary>
        /// 员工薪资结算岗位过滤
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEESALARYPOSTASIGN> GetBalancePostFilter(int orgtype, string orgid)
        {
            IQueryable<T_HR_BALANCEPOSTDETAIL> ents = dal.GetObjects<T_HR_BALANCEPOSTDETAIL>();
            switch (orgtype)
            {
                case 0:
                    ents = from a in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                           where a.EMPLOYEECOMPANYID == orgid
                           select a;
                    break;
                case 1:
                    ents = from a in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                           where a.EMPLOYEEDEPARTMENTID == orgid 
                           select a;
                    break;
                case 2:
                    ents = from a in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                           where a.EMPLOYEEPOSTID == orgid
                           select a;
                    break;
            }
            List<string> asignIDs = new List<string>();
            IQueryable<T_HR_EMPLOYEESALARYPOSTASIGN> returnEnts = dal.GetTable();
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    asignIDs.Add(ent.BALANCEPOSTASIGNID);
                }
                returnEnts = returnEnts.Where(s => asignIDs.Contains(s.EMPLOYEESALARYPOSTASIGNID));
            }
            else
            {
                returnEnts = null;
            }
            return returnEnts;
        }

        /// <summary>
        /// 员工过滤
        /// </summary>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEE> GetEmployeeFilter(int orgtype, string orgid, DateTime starttime, DateTime endtime)
        {
            IQueryable<T_HR_EMPLOYEE> ents = dal.GetObjects<T_HR_EMPLOYEE>();
            switch (orgtype)
            {
                case -1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           where a.EMPLOYEESTATE == "2"
                           select a;
                    break;
                case 0:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join e in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.T_HR_COMPANY.COMPANYID == orgid && e.CHECKSTATE == "2" && e.STOPPAYMENTDATE >= starttime && e.STOPPAYMENTDATE <= endtime
                           select a;
                    break;
                case 1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           join e in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.DEPARTMENTID == orgid && e.CHECKSTATE == "2" && e.STOPPAYMENTDATE >= starttime && e.STOPPAYMENTDATE <= endtime
                           select a;
                    break;
                case 2:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join e in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>() on a.EMPLOYEEID equals e.EMPLOYEEID
                           where c.POSTID == orgid && e.CHECKSTATE == "2" && e.STOPPAYMENTDATE >= starttime && e.STOPPAYMENTDATE <= endtime
                           select a;
                    break;
            }
            return ents;
        }

        /// <summary>
        /// 更新状态，终审通过后修改t_hr_salaryarchive
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
                SMT.Foundation.Log.Tracer.Debug("employeeBalancePostBLL");
                var balancePost = (from c in dal.GetObjects<T_HR_EMPLOYEESALARYPOSTASIGN>()
                              where c.EMPLOYEESALARYPOSTASIGNID == EntityKeyValue
                              select c).FirstOrDefault();
                if (balancePost != null)
                {
                    balancePost.CHECKSTATE = CheckState;
                    balancePost.UPDATEDATE = DateTime.Now;
                    dal.UpdateFromContext(balancePost);
                    i = dal.SaveContextChanges();
                    //终审通过后才改动
                    if (CheckState == "2")
                    {
                        UpdateEmployeeSalaryInfo(balancePost);
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

        public void UpdateEmployeeSalaryInfo(T_HR_EMPLOYEESALARYPOSTASIGN asign)
        {
            try
            {
                var balancePost = from c in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                                  where c.BALANCEPOSTASIGNID == asign.EMPLOYEESALARYPOSTASIGNID
                                  //&& c.SALARYSET =="1"
                                  select c;
                var entPosts = from p in dal.GetObjects<T_HR_POST>()
                               where p.POSTID == asign.BALANCEPOSTID
                               select p;
                string payCompanyId = string.Empty;
                if (entPosts.Count() > 0)
                {
                    T_HR_POST post = entPosts.FirstOrDefault();
                    if (post != null)
                    {
                        payCompanyId = post.OWNERCOMPANYID;
                    }
                    else
                    {
                        SMT.Foundation.Log.Tracer.Debug("UpdateEmployeeSalaryInfo获取岗位信息为空：");
                    }
                }
                else
                {
                    SMT.Foundation.Log.Tracer.Debug("UpdateEmployeeSalaryInfo获取岗位信息为空22222"); 
                }
                List<string> employeeIDs = new List<string>();
                //岗位ID集合
                List<string> SalarypostIDs = new List<string>();
                List<string> AttendancepostIDs = new List<string>();
                if (balancePost.Count() > 0)
                {
                    
                    foreach (var ent in balancePost)
                    {
                        employeeIDs.Add(ent.EMPLOYEEID);
                        if (ent.SALARYSET == "1")
                        {
                            //只有薪资岗位设置了才修改员工薪资档案中的结算岗位信息
                            SalarypostIDs.Add(ent.EMPLOYEEPOSTID);
                        }
                        if (ent.ATTENDANCESET == "1")
                        {
                            //考勤结算对应的岗位ID
                            AttendancepostIDs.Add(ent.EMPLOYEEPOSTID);
                        }
                        ent.EDITSTATE = "1";
                        dal.UpdateFromContext(ent);
                    }
                    #region 将原来的记录设置为失效

                    var balancePost1 = from c in dal.GetObjects<T_HR_BALANCEPOSTDETAIL>()
                                       //where employeeIDs.Contains(c.EMPLOYEEID)
                                       where SalarypostIDs.Contains(c.EMPLOYEEPOSTID) 
                                       || AttendancepostIDs.Contains(c.EMPLOYEEPOSTID)
                                       //&& c.SALARYSET == "1"
                                       select c;
                    //foreach (var id in employeeIDs)
                    //{
                    //    var entemployees = from ent in balancePost1
                    //                       where ent.EMPLOYEEID == id
                    //                       && ent.EDITSTATE == "1"
                    //                       orderby ent.CREATEDATE descending
                    //                       select ent;
                    //    if (entemployees.Count() > 0)
                    //    {
                    //        T_HR_BALANCEPOSTDETAIL updateDetail = entemployees.FirstOrDefault();
                    //        updateDetail.EDITSTATE = "0";
                    //        dal.UpdateFromContext(updateDetail);
                    //        SMT.Foundation.Log.Tracer.Debug(updateDetail.EMPLOYEENAME + updateDetail.BALANCEPOSTDETAIL + "已修改");
                    //    }
                    //}
                    foreach (var id in SalarypostIDs)
                    {
                        var entemployees = from ent in balancePost1
                                           where ent.EMPLOYEEPOSTID == id
                                           && ent.EDITSTATE == "1"
                                           orderby ent.CREATEDATE descending
                                           select ent;
                        if (entemployees.Count() > 0)
                        {
                            T_HR_BALANCEPOSTDETAIL updateDetail = entemployees.FirstOrDefault();
                            updateDetail.EDITSTATE = "0";
                            dal.UpdateFromContext(updateDetail);
                            SMT.Foundation.Log.Tracer.Debug(updateDetail.EMPLOYEENAME + updateDetail.BALANCEPOSTDETAIL + "已修改");
                        }
                    }
                    //考勤设置对应的岗位ID
                    foreach (var id in AttendancepostIDs)
                    {
                        var entemployees = from ent in balancePost1
                                           where ent.EMPLOYEEPOSTID == id
                                           && ent.EDITSTATE == "1"
                                           orderby ent.CREATEDATE descending
                                           select ent;
                        if (entemployees.Count() > 0)
                        {
                            T_HR_BALANCEPOSTDETAIL updateDetail = entemployees.FirstOrDefault();
                            updateDetail.EDITSTATE = "0";
                            dal.UpdateFromContext(updateDetail);
                            SMT.Foundation.Log.Tracer.Debug(updateDetail.EMPLOYEENAME + updateDetail.BALANCEPOSTDETAIL + "已修改");
                        }
                    }
                    #endregion
                }
                //var Salarys = from ent in dal.GetObjects<T_HR_SALARYARCHIVE>()
                //              where employeeIDs.Contains(ent.EMPLOYEEID)
                //              && ent.CHECKSTATE == "2"
                //              orderby ent.CREATEDATE descending
                //              select ent;
                //有薪资岗位设置才进行员工岗位变更操作
                if (SalarypostIDs.Count() > 0)
                {
                    var entEmployeePosts = from ent in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_POST").Include("T_HR_EMPLOYEE")
                                           where SalarypostIDs.Contains(ent.T_HR_POST.POSTID)
                                           && ent.CHECKSTATE =="2" && ent.ISAGENCY =="0"
                                           && ent.EDITSTATE =="1"
                                           select ent;
                    List<string> postEmployeeIDs = new List<string>();
                    foreach (var a in entEmployeePosts)
                    {
                        postEmployeeIDs.Add(a.T_HR_EMPLOYEE.EMPLOYEEID);
                    }

                    //var Salarys = from ent in dal.GetObjects<T_HR_SALARYARCHIVE>()
                    //              where SalarypostIDs.Contains(ent.EMPLOYEEPOSTID)
                    //              && ent.CHECKSTATE == "2"
                    //              orderby ent.CREATEDATE descending
                    //              select ent;
                    var Salarys = from ent in dal.GetObjects<T_HR_SALARYARCHIVE>()
                                  where postEmployeeIDs.Contains(ent.EMPLOYEEID)
                                  && ent.CHECKSTATE == "2"
                                  orderby ent.CREATEDATE descending
                                  select ent;
                    List<T_HR_SALARYARCHIVE> updateSalary = new List<T_HR_SALARYARCHIVE>();
                    if (Salarys.Count() > 0)
                    {
                        foreach (var ent in Salarys)
                        {
                            var entFirst = from a in Salarys
                                           where a.EMPLOYEEID == ent.EMPLOYEEID
                                           orderby a.CREATEDATE descending
                                           select a;
                            if (entFirst.Count() > 0)
                            {
                                T_HR_SALARYARCHIVE archive = entFirst.FirstOrDefault();
                                if (updateSalary.Count() == 0)
                                {
                                    updateSalary.Add(archive);
                                }
                                else
                                {
                                    var exists = from b in updateSalary
                                                 where b.SALARYARCHIVEID == archive.SALARYARCHIVEID
                                                 select b;
                                    if (exists.Count() == 0)
                                    {
                                        updateSalary.Add(archive);
                                    }
                                }
                            }
                        }
                    }
                    if (updateSalary.Count() > 0)
                    {
                        foreach (var ent in updateSalary)
                        {
                            SMT.Foundation.Log.Tracer.Debug(ent.EMPLOYEENAME + "原来结算岗位为：" + ent.BALANCEPOSTNAME + "岗位ID:" + ent.BALANCEPOSTID);
                            ent.BALANCEPOSTID = asign.BALANCEPOSTID;
                            ent.BALANCEPOSTNAME = asign.BALANCEPOSTNAME;
                            if (!string.IsNullOrEmpty(payCompanyId))
                            {
                                ent.PAYCOMPANY = payCompanyId;
                            }
                            ent.OWNERCOMPANYID = asign.OWNERCOMPANYID;
                            dal.UpdateFromContext(ent);
                            SMT.Foundation.Log.Tracer.Debug(ent.EMPLOYEENAME + "修改为：" + ent.BALANCEPOSTNAME);
                        }
                    }
                }
                int i = dal.SaveContextChanges();


            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("UpdateEmployeeSalaryInfo执行时出错："+ ex.ToString()); 
            }
        
        }

        
    }
}
