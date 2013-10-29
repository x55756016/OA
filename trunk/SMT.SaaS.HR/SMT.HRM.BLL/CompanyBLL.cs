using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
using SMT.HRM.CustomModel;
using SMT.HRM.IMServices.IMServiceWS;//即时通讯服务
namespace SMT.HRM.BLL
{
    public class CompanyBLL : BaseBll<T_HR_COMPANY>, ILookupEntity, IOperate
    {
        /// <summary>
        /// 根据公司ID获取公司
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public T_HR_COMPANY GetCompanyByID(string strCompanyID)
        {
            IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                            where c.COMPANYID == strCompanyID
                                            select c;

            T_HR_COMPANY ent = new T_HR_COMPANY();
            if (ents.Count() > 0)
            {
                ent = ents.FirstOrDefault();
            }
            else
            {
                ent = null;
            }

            return ent;
        }

        /// <summary>
        /// 根据部门ID获取该公司所属的公司
        /// </summary>
        /// <param name="strDepartmentID"></param>
        /// <returns></returns>
        public T_HR_COMPANY GetCompanyByDepartmentID(string strDepartmentID)
        {
            IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                            join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.COMPANYID equals d.T_HR_COMPANY.COMPANYID
                                            where d.DEPARTMENTID == strDepartmentID
                                            select c;

            T_HR_COMPANY ent = new T_HR_COMPANY();
            if (ents.Count() > 0)
            {
                ent = ents.FirstOrDefault();
            }
            else
            {
                ent = null;
            }

            return ent;
        }

        /// <summary>
        /// 根据岗位ID获取该岗位所属的公司
        /// </summary>
        /// <param name="strPostID"></param>
        /// <returns></returns>
        public T_HR_COMPANY GetCompanyByPostID(string strPostID)
        {
            IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                            join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.COMPANYID equals d.T_HR_COMPANY.COMPANYID
                                            join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on d.DEPARTMENTID equals p.T_HR_DEPARTMENT.DEPARTMENTID
                                            where p.POSTID == strPostID
                                            select c;

            T_HR_COMPANY ent = new T_HR_COMPANY();
            if (ents.Count() > 0)
            {
                ent = ents.FirstOrDefault();
            }
            else
            {
                ent = null;
            }

            return ent;
        }

        public T_HR_COMPANY GetCompanyByEmployeeID(string strEmployeeID)
        {
            IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                            join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.COMPANYID equals d.T_HR_COMPANY.COMPANYID
                                            join p in dal.GetObjects<T_HR_POST>().Include("T_HR_DEPARTMENT") on d.DEPARTMENTID equals p.T_HR_DEPARTMENT.DEPARTMENTID
                                            join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST") on p.POSTID equals ep.T_HR_POST.POSTID
                                            join e in dal.GetObjects<T_HR_EMPLOYEE>() on ep.T_HR_EMPLOYEE.EMPLOYEEID equals e.EMPLOYEEID
                                            where e.EMPLOYEEID == strEmployeeID
                                            select c;

            T_HR_COMPANY ent = new T_HR_COMPANY();
            if (ents.Count() > 0)
            {
                ent = ents.FirstOrDefault();
            }
            else
            {
                ent = null;
            }

            return ent;
        }

        /// <summary>
        /// 获取全部可用的公司信息
        /// </summary>
        /// <returns>公司列表</returns>
        public IQueryable<T_HR_COMPANY> GetCompanyActived(string userID)
        {
            IQueryable<T_HR_COMPANY> ents = GetCompanyActived(userID, "3", "T_HR_COMPANY");
            return ents;
        }
        /// <summary>
        /// 获取全部可用的公司信息
        /// </summary>
        /// <returns>公司列表</returns>
        public IQueryable<T_HR_COMPANY> GetCompanyActived(string userID, string perm, string entity)
        {
            try
            {
                string state = ((int)EditStates.Actived).ToString();
                string checkState = ((int)CheckStates.Approved).ToString();
                //var ents = from a in dal.GetObjects()
                //           where a.EDITSTATE == state && a.CHECKSTATE == checkState
                //       select a;
                List<object> paras = new List<object>();
                string filterString = "";

                if (!string.IsNullOrEmpty(userID))
                {
                    SetOrganizationFilter(ref filterString, ref paras, userID, entity, perm);
                    SetCompanyFilter(ref filterString, ref paras, userID);
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString = "(" + filterString + ")";
                    filterString += " and EDITSTATE==@" + paras.Count;
                    paras.Add(state);
                }
                else
                {
                    filterString = "EDITSTATE==@" + paras.Count;
                    paras.Add(state);
                }

                filterString += " and CHECKSTATE==@" + paras.Count;
                paras.Add(checkState);

                IQueryable<T_HR_COMPANY> ents = dal.GetObjects();
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, paras.ToArray());
                }
                if (string.IsNullOrEmpty(userID))
                {
                    List<T_HR_COMPANY> cl = new List<T_HR_COMPANY>();

                    foreach (var q in ents)
                    {
                        T_HR_COMPANY c = new T_HR_COMPANY();
                        c.COMPANYID = q.COMPANYID;
                       // c.CNAME = q.CNAME;
                        c.CNAME = q.BRIEFNAME == null ? q.CNAME : q.BRIEFNAME;
                        c.FATHERID = q.FATHERID;
                        c.FATHERTYPE = q.FATHERTYPE;
                        cl.Add(c);

                    }
                    return cl.AsQueryable();
                }
                foreach (var item in ents)
                {
                   item.CNAME = item.BRIEFNAME == null ? item.CNAME : item.BRIEFNAME;
                }
                return ents;
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " GetCompanyActived获取公司信息错误:" + ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 获取除审核状态不通过和编辑状态为删除全部可用的公司信息
        /// </summary>
        /// <returns>公司列表</returns>
        public IQueryable<T_HR_COMPANY> GetCompanyAll(string userID)
        {
            string state = ((int)EditStates.Deleted).ToString();
            string canceledEditState = ((int)EditStates.Canceled).ToString(); //by xiedx 已撤销的也不能看。。
            string checkState = ((int)CheckStates.UnApproved).ToString();
            //var ents = from a in dal.GetObjects()
            //           where a.EDITSTATE == state && a.CHECKSTATE == checkState
            //       select a;
            string filterString = "";
            List<object> paras = new List<object>();

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_COMPANY");
                SetCompanyFilter(ref filterString, ref paras, userID);
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = "(" + filterString + ")";
                filterString += " and EDITSTATE<>@" + paras.Count;
                paras.Add(state);
                filterString += " and EDITSTATE<>@" + paras.Count;
                paras.Add(canceledEditState);
                
            }
            else
            {
                filterString = "EDITSTATE<>@" + paras.Count;
                paras.Add(state);
                filterString += " and EDITSTATE<>@" + paras.Count;
                paras.Add(canceledEditState);
            }

            filterString += " and CHECKSTATE<>@" + paras.Count;
            paras.Add(checkState);

            IQueryable<T_HR_COMPANY> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }

        /// <summary>
        /// 对指定公司更新所属员工的企业工龄
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public void GetCompanyForOutEngineXml(T_HR_COMPANY entTemp)
        {
            List<object> objArds = new List<object>();
            objArds.Add(entTemp.COMPANYID);
            objArds.Add("HR");
            objArds.Add("Company");
            objArds.Add(entTemp.COMPANYID);
            objArds.Add(DateTime.Now.ToString("yyyy/MM/d"));
            objArds.Add(DateTime.Now.ToString("HH:mm"));
            objArds.Add("Day");
            objArds.Add("");
            objArds.Add(entTemp.CNAME + "公司于" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",开始检测当前公司所属员工的企业工龄");
            objArds.Add("");
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"UpdateEmployeeWorkAgeByID\" Name=\"COMPANYID\" Value=\"" + entTemp.COMPANYID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("CustomBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        /// <summary>
        /// 查看本公司的所有公司信息
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="EmployeeID"></param>
        private void SetCompanyFilter(ref string filterString, ref List<object> queryParas, string employeeID)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return;
            }
            var employe = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                          where ent.EMPLOYEEID == employeeID
                          select ent;
            T_HR_EMPLOYEE emp = employe.FirstOrDefault();

            if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
            {
                emp.T_HR_EMPLOYEEPOST.Load();
            }
            DepartmentBLL depBll = new DepartmentBLL();
            foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
            {
                //不是有效岗位，跳过
                if (ep.EDITSTATE != "1")
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " OR ";

                filterString += "COMPANYID==@" + queryParas.Count().ToString();

                if (!ep.T_HR_POSTReference.IsLoaded)
                    ep.T_HR_POSTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

                queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                //查看是否有下级公司
                SetInferiorCompany(ep, ref filterString, ref queryParas);
                //所在部门的所有上级公司
                Dictionary<string, string> dictIDs = depBll.GetFatherByDepartmentID(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                foreach (KeyValuePair<string, string> item in dictIDs)
                {
                    if (item.Value == "0")
                    {
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " OR ";
                        filterString += " COMPANYID==@" + queryParas.Count().ToString();
                        queryParas.Add(item.Key);
                    }
                }


            }
        }

        private void SetInferiorCompany(T_HR_EMPLOYEEPOST ep, ref string filterString, ref List<object> queryParas)
        {
            var tempEnt = dal.GetObjects().Where(s => s.T_HR_COMPANY2.COMPANYID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
            getAllCompany(ref filterString, ref queryParas, tempEnt);
        }

        private void getAllCompany(ref string filterString, ref List<object> queryParas, IQueryable<T_HR_COMPANY> tempEnt)
        {
            if (tempEnt != null)
            {
                foreach (var ent in tempEnt)
                {
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " OR ";

                    filterString += "COMPANYID==@" + queryParas.Count().ToString();
                    queryParas.Add(ent.COMPANYID);
                    var tempChildEnt = dal.GetObjects().Where(s => s.T_HR_COMPANY2.COMPANYID == ent.COMPANYID);
                    if (tempChildEnt.Count() > 0)
                    {
                        getAllCompany(ref filterString, ref queryParas, tempChildEnt);
                    }
                }
            }
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的公司信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_COMPANY> CompanyPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string strCheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string tempString = "";
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                if (!string.IsNullOrEmpty(userID))
                {
                    SetOrganizationFilter(ref tempString, ref queryParas, userID, "T_HR_COMPANY");
                    SetCompanyFilter(ref tempString, ref queryParas, userID);
                }



                if (!string.IsNullOrEmpty(tempString))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        tempString = "(" + tempString + ")";
                        tempString += " and " + filterString;
                    }
                }
                else
                {
                    tempString = filterString;
                }

            }
            else
            {
                SetFilterWithflow("COMPANYID", "T_HR_COMPANY", userID, ref strCheckState, ref tempString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }

            IQueryable<T_HR_COMPANY> ents = dal.GetObjects();
            if (!string.IsNullOrEmpty(tempString))
            {
                ents = ents.Where(tempString, queryParas.ToArray());
            }
            //BUG修改人:喻建华  时间:2010-06-19 修改原因:审核过滤错误
            if (!string.IsNullOrEmpty(strCheckState))
            {
                ents = ents.Where(m => m.CHECKSTATE == strCheckState);
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_COMPANY>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 根据公司ID获取公司信息
        /// </summary>
        /// <param name="companyID">公司ID</param>
        /// <returns>返回公司信息</returns>
        public T_HR_COMPANY GetCompanyById(string companyID)
        {
            var ents = from ent in dal.GetObjects().Include("T_HR_COMPANY2")
                       where ent.COMPANYID == companyID
                       select ent;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 根据公司ID集合获取公司
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<T_HR_COMPANY> GetCompanyByIds(string[] ids)
        {
            string filterString = "";

            IList<object> queryParas = new List<object>();

            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " OR ";

                filterString += " COMPANYID==@" + queryParas.Count().ToString() + " ";
                queryParas.Add(id);
            }

            var ents = dal.GetObjects().Where(filterString, queryParas.ToArray());

            return ents.Count() > 0 ? ents.ToList() : null;
        }
        /// <summary>
        /// 添加公司
        /// </summary>
        /// <param name="entity">公司实例</param>
        public void CompanyAdd(T_HR_COMPANY entity, ref string strMsg)
        {
            try
            {
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.COMPANRYCODE == entity.COMPANRYCODE
                || s.CNAME == entity.CNAME);
                if (tempEnt != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                T_HR_COMPANY ent = new T_HR_COMPANY();
                Utility.CloneEntity<T_HR_COMPANY>(entity, ent);

                //如果父公司为空，就不赋值
                if (entity.T_HR_COMPANY2 != null)
                {
                    ent.T_HR_COMPANY2Reference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY2.COMPANYID);
                }
                // dal.Add(ent);
                ent.CREATEDATE = System.DateTime.Now;
                Add(ent);
                //  WorkflowUtility.CallWorkflow("公司申请审核工作流", ent);
            }
            catch (Exception ex)
            {
                // strMsg = ex.Message;
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CompanyAdd:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 变更公司
        /// </summary>
        /// <param name="entity">公司实例</param>
        public void CompanyUpdate(T_HR_COMPANY entity, ref string strMsg)
        {
            try
            {
                #region
                //var temp = dal.GetObjects().FirstOrDefault(s => (s.COMPANRYCODE == entity.COMPANRYCODE
                //   || s.CNAME == entity.CNAME) && s.COMPANYID != entity.COMPANYID);
                //if (temp != null)
                //{
                //    // throw new Exception("Repetition");
                //    strMsg = "Repetition";
                //    return;
                //}
                //var ents = from ent in dal.GetObjects()
                //           where ent.COMPANYID == entity.COMPANYID
                //           select ent;
                //if (ents.Count() > 0)
                //{
                //    var ent = ents.FirstOrDefault();
                //    Utility.CloneEntity<T_HR_COMPANY>(entity, ent);
                //    //如果父公司为空，就不赋值
                //    if (entity.T_HR_COMPANY2 != null)
                //    {
                //        ent.T_HR_COMPANY2Reference.EntityKey =
                //            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY2.COMPANYID);
                //    }
                //    dal.Update(ent);
                //    //如果审核状态为审核通过则添加公司历史
                //    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                //    {
                //        CompanyHistoryBLL chbll = new CompanyHistoryBLL();
                //        T_HR_COMPANYHISTORY companyHis = new T_HR_COMPANYHISTORY();
                //        companyHis.RECORDSID = Guid.NewGuid().ToString();
                //        //companyHis = Utility.CloneObject<T_HR_COMPANYHISTORY>(entity);
                //        companyHis.COMPANYCATEGORY = entity.COMPANYCATEGORY;
                //        companyHis.COMPANRYCODE = entity.COMPANRYCODE;
                //        companyHis.COMPANYLEVEL = entity.COMPANYLEVEL;
                //        companyHis.COMPANYID = entity.COMPANYID;
                //        companyHis.CNAME = entity.CNAME;
                //        companyHis.ENAME = entity.ENAME;
                //        companyHis.T_HR_COMPANY = entity.T_HR_COMPANY2;
                //        companyHis.LEGALPERSON = entity.LEGALPERSON;
                //        companyHis.LINKMAN = entity.LINKMAN;
                //        companyHis.TELNUMBER = entity.TELNUMBER;
                //        companyHis.ADDRESS = entity.ADDRESS;
                //        companyHis.LEGALPERSONID = entity.LEGALPERSONID;
                //        companyHis.BUSSINESSLICENCENO = entity.BUSSINESSLICENCENO;
                //        companyHis.BUSSINESSAREA = entity.BUSSINESSAREA;
                //        companyHis.ACCOUNTCODE = entity.ACCOUNTCODE;
                //        companyHis.BANKID = entity.BANKID;
                //        companyHis.EMAIL = entity.EMAIL;
                //        companyHis.ZIPCODE = entity.ZIPCODE;
                //        companyHis.FAXNUMBER = entity.FAXNUMBER;
                //        companyHis.CREATEDATE = entity.CREATEDATE;
                //        companyHis.CREATEUSERID = entity.CREATEUSERID;
                //        companyHis.UPDATEDATE = DateTime.Now;
                //        companyHis.UPDATEUSERID = entity.UPDATEUSERID;
                //        companyHis.REUSEDATE = DateTime.Now;
                //        companyHis.T_HR_COMPANYReference.EntityKey = null;
                //        companyHis.T_HR_COMPANY = null;

                //        companyHis.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                //        companyHis.OWNERDEPARTMENTID = entity.OWNERDEPARTMENTID;
                //        companyHis.OWNERID = entity.OWNERDEPARTMENTID;
                //        companyHis.OWNERPOSTID = entity.OWNERPOSTID;

                //        if (entity.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                //        {
                //            companyHis.CANCELDATE = DateTime.Now;
                //        }
                //        // DataContext.AddObject("T_HR_COMPANYHISTORY", companyHis);
                //        chbll.CompanyHistoryAdd(companyHis);
                //        try
                //        {
                //            GetCompanyForOutEngineXml(entity);//向引擎推送计算员工企业工龄的接口契约
                //        }
                //        catch (Exception ex)
                //        {
                //            Tracer.Debug(ex.Message);
                //            throw (ex);
                //        }
                //    }
                //    //  DataContext.SaveChanges();
                //    //WorkflowUtility.CallWorkflow("公司审批审核工作流", ent);
                //}
                #endregion
                var temp = dal.GetObjects().FirstOrDefault(s => (s.COMPANRYCODE == entity.COMPANRYCODE
                  || s.CNAME == entity.CNAME) && s.COMPANYID != entity.COMPANYID);
                if (temp != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }

                entity.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.COMPANYID);
                //如果父公司为空，就不赋值
                if (entity.T_HR_COMPANY2 != null)
                {
                    entity.T_HR_COMPANY2Reference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY2.COMPANYID);
                    entity.T_HR_COMPANY2.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY2.COMPANYID);
                }
                //  dal.Update(entity);
                entity.UPDATEDATE = System.DateTime.Now;
                Update(entity);
                //如果审核状态为审核通过则添加公司历史
                #region 如果审核状态为审核通过则添加公司历史
                if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    CompanyHistoryBLL chbll = new CompanyHistoryBLL();
                    T_HR_COMPANYHISTORY companyHis = new T_HR_COMPANYHISTORY();
                    companyHis.RECORDSID = Guid.NewGuid().ToString();
                    //companyHis = Utility.CloneObject<T_HR_COMPANYHISTORY>(entity);
                    companyHis.COMPANYCATEGORY = entity.COMPANYCATEGORY;
                    companyHis.COMPANRYCODE = entity.COMPANRYCODE;
                    companyHis.COMPANYLEVEL = entity.COMPANYLEVEL;
                    companyHis.COMPANYID = entity.COMPANYID;
                    companyHis.CNAME = entity.CNAME;
                    companyHis.ENAME = entity.ENAME;
                    companyHis.T_HR_COMPANY = entity.T_HR_COMPANY2;
                    companyHis.LEGALPERSON = entity.LEGALPERSON;
                    companyHis.LINKMAN = entity.LINKMAN;
                    companyHis.TELNUMBER = entity.TELNUMBER;
                    companyHis.ADDRESS = entity.ADDRESS;
                    companyHis.LEGALPERSONID = entity.LEGALPERSONID;
                    companyHis.BUSSINESSLICENCENO = entity.BUSSINESSLICENCENO;
                    companyHis.BUSSINESSAREA = entity.BUSSINESSAREA;
                    companyHis.ACCOUNTCODE = entity.ACCOUNTCODE;
                    companyHis.BANKID = entity.BANKID;
                    companyHis.EMAIL = entity.EMAIL;
                    companyHis.ZIPCODE = entity.ZIPCODE;
                    companyHis.FAXNUMBER = entity.FAXNUMBER;
                    companyHis.CREATEDATE = entity.CREATEDATE;
                    companyHis.CREATEUSERID = entity.CREATEUSERID;
                    companyHis.UPDATEDATE = DateTime.Now;
                    companyHis.UPDATEUSERID = entity.UPDATEUSERID;
                    companyHis.REUSEDATE = DateTime.Now;
                    companyHis.T_HR_COMPANYReference.EntityKey = null;
                    companyHis.T_HR_COMPANY = null;

                    companyHis.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                    companyHis.OWNERDEPARTMENTID = entity.OWNERDEPARTMENTID;
                    companyHis.OWNERID = entity.OWNERID;
                    companyHis.OWNERPOSTID = entity.OWNERPOSTID;

                    if (entity.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        companyHis.CANCELDATE = DateTime.Now;
                    }
                    // DataContext.AddObject("T_HR_COMPANYHISTORY", companyHis);
                    chbll.CompanyHistoryAdd(companyHis);
                    EditVersion("公司");
                #endregion

                    #region 添加即时通讯接口
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //SMT.Foundation.Log.Tracer.Debug("引擎开始调用即时通讯接口，添加公司");
                        //this.AddCompanyInfoToIM(entity);
                    }
                    #endregion
                    try
                    {
                        GetCompanyForOutEngineXml(entity);//向引擎推送计算员工企业工龄的接口契约
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug(ex.Message);
                        throw (ex);
                    }

                    //  DataContext.SaveChanges();
                    //WorkflowUtility.CallWorkflow("公司审批审核工作流", ent);
                }

            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                Tracer.Debug(ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 撤销公司
        /// </summary>
        /// <param name="entity">公司实例</param>
        /// <returns>是否成功撤销</returns>
        public bool CompanyCancel(T_HR_COMPANY entity, ref string strMsg)
        {
            try
            {
                DepartmentBLL departBll = new DepartmentBLL();

                if (GetChildOrgCount(entity.COMPANYID) > 0)
                {
                    // throw new Exception("当前公司有下级公司，不能撤消！");
                    strMsg = "HASCHILDCOMPANY";
                    return false;
                }
                else
                {
                    if (departBll.GetDepartCount(entity.COMPANYID) > 0)
                    {
                        // throw new Exception("当前公司下有部门，不能撤消！");
                        strMsg = "HASCHILDDEPARTMENT";
                        return false;
                    }
                    //var ent = entity;
                    //ent.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                    //ent.UPDATEDATE = entity.UPDATEDATE;
                    //ent.UPDATEUSERID = entity.UPDATEUSERID;
                    //dal.Update(ent);

                    var ents = from q in dal.GetTable()
                               where q.COMPANYID == entity.COMPANYID
                               select q;
                    if (ents.Count() > 0)
                    {
                        var ent = ents.FirstOrDefault();
                        ent.EDITSTATE = entity.EDITSTATE;
                        ent.CHECKSTATE = entity.CHECKSTATE;
                        ent.UPDATEDATE = System.DateTime.Now;
                        ent.UPDATEUSERID = entity.UPDATEUSERID;
                        // dal.Update(ent);
                        Update(ent);
                        EditVersion("公司");
                        //WorkflowUtility.CallWorkflow("公司撤消审批工作流", entity);
                        //#region 取消公司时调用即时通讯接口
                        //string StrMessage = "取消公司开始调用即时通讯接口公司名称：";
                        //StrMessage += ent.CNAME + "公司ID：" + ent.COMPANYID;
                        //SMT.Foundation.Log.Tracer.Debug(StrMessage);
                        //AddCompanyInfoToIM(ent);
                        //#endregion
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CompanyCancel:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 删除公司
        /// </summary>
        /// <param name="entity">公司ID</param>
        /// <returns></returns>
        public void CompanyDelete(string id, ref string strMsg)
        {
            try
            {
                string[] ids = id.Split(',');
                foreach (var idItem in ids)
                {
                    if (!string.IsNullOrEmpty(idItem))
                    {
                        var entitys = (from ent in dal.GetTable()
                                       where ent.COMPANYID == id
                                       select ent);
                        if (entitys.Count() > 0)
                        {
                            var entity = entitys.FirstOrDefault();
                            DepartmentBLL departBll = new DepartmentBLL();
                            if (GetChildOrgCount(entity.COMPANYID) > 0)
                            {
                                //  throw new Exception("当前公司有下级公司，不能删除！");
                                strMsg += entity.CNAME + "有下级公司，不能删除！\n";
                                // return;
                            }
                            else
                            {
                                if (departBll.GetDepartCount(entity.COMPANYID) > 0)
                                {
                                    // throw new Exception("当前公司下有部门，不能删除！");
                                    strMsg += entity.CNAME + "有部门，不能删除！\n";
                                    // return;
                                }
                                else
                                {
                                    //  dal.Delete(entity);
                                    Delete(entity);

                                }
                            }
                        }

                    }

                }
                EditVersion("公司");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CompanyDelete:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 判断选择的父公司，是否为当前添加(修改)的公司的子公司
        /// </summary>
        /// <param name="companyID">当前添加(修改)公司的ID</param>
        /// <param name="parentCompanyID">选择父公司的ID</param>
        /// <returns>是否为当前公司的子公司</returns>
        public bool IsChildCompany(string companyID, string parentCompanyID)
        {
            var ents = from a in dal.GetObjects()
                       where a.T_HR_COMPANY2.COMPANYID == companyID
                       select a;
            if (ents != null && ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    if (ent.COMPANYID == parentCompanyID)
                    {
                        return false;
                    }
                    else
                    {
                        if (!IsChildCompany(ent.COMPANYID, parentCompanyID))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 根据员工ID获取员工所有岗位的公司ID（包括父级公司）
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>公司string集合</returns>
        public List<string> GetAllParentsCompamy(string employeeID)
        {
            try
            {
                EmployeeBLL bll = new EmployeeBLL();
                List<V_EMPLOYEEPOSTBRIEF> listBrief = bll.GetEmployeeDetailView(employeeID).EMPLOYEEPOSTS;
                List<string> listCompanyID = new List<string>();
                if (listBrief != null && listBrief.Count > 0)
                {
                    listBrief.ForEach(it =>
                        {
                            var ents = from a in dal.GetObjects()
                                       where a.COMPANYID == it.CompanyID
                                       select a;
                            if (ents != null && ents.Count() > 0)
                            {
                                ents.ToList().ForEach(item =>
                                    {
                                        List<string> tempList = new List<string>();
                                        this.GetParentsCompanyID(item.COMPANYID,item.FATHERTYPE, ref tempList);
                                        listCompanyID.AddRange(tempList);
                                    });
                            }
                        });
                }
                listCompanyID = listCompanyID.Distinct().ToList();
                return listCompanyID;
            }
            catch (Exception ex)
            {
                Tracer.Debug("活动员工所有公司 GetAllParentsCompamy出错" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 递归获取某个公司或部门机构的所有父级公司
        /// </summary>
        /// <param name="orgID">机构ID</param>
        /// <param name="type">机构类型（0公司，1部门）</param>
        /// <param name="listCompanyID"></param>
        /// <returns></returns>
        public List<string> GetParentsCompanyID(string orgID,  string type, ref List<string> listCompanyID)
        {
            try
            {
                if (type == "0")
                {
                    listCompanyID.Add(orgID);
                    var entCom = from a in dal.GetObjects()
                                 where a.COMPANYID == orgID
                                 select a;
                    if (entCom != null && entCom.Count() > 0)
                    {
                        T_HR_COMPANY company = entCom.FirstOrDefault();
                        if (!string.IsNullOrEmpty(company.FATHERID))
                        {
                            this.GetParentsCompanyID(company.FATHERID, company.FATHERTYPE, ref listCompanyID);
                        }
                    }
                }
                else if (type == "1")
                {
                    var entDep = from a in dal.GetObjects<T_HR_DEPARTMENT>()
                                 where a.DEPARTMENTID == orgID
                                 select a;
                    if (entDep != null && entDep.Count() > 0)
                    {
                        T_HR_DEPARTMENT dep = entDep.FirstOrDefault();
                        if (!string.IsNullOrEmpty(dep.FATHERID))
                        {
                            this.GetParentsCompanyID(dep.FATHERID, dep.FATHERTYPE, ref listCompanyID);
                        }
                    }
                }
                else//没有上级部门
                {
                    listCompanyID.Add(orgID);
                }
                return listCompanyID;
            }
            catch (Exception ex)
            {
                Tracer.Debug("活动员工所有公司 GetAllParentsCompamy出错" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return listCompanyID;
            }
        }
       
        /// <summary>
        /// 当前公司有字公司的数量
        /// </summary>
        /// <param name="companyID">父公司ID</param>
        /// <returns>返回公司数量</returns>
        private int GetChildOrgCount(string companyID)
        {
            var ents = from o in dal.GetObjects()
                       where o.T_HR_COMPANY2.COMPANYID == companyID && o.EDITSTATE == "1"
                       select o;

            return ents.Count();
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{            
        //    string state = ((int)EditStates.Actived).ToString();
        //    string checkState = ((int)CheckStates.Approved).ToString();
        //    IQueryable<T_HR_COMPANY> ents = from a in dal.GetObjects()
        //               where a.CHECKSTATE == checkState && a.EDITSTATE == state
        //               select a;


        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_COMPANY");
                SetCompanyFilter(ref filterString, ref queryParas, userID);
            }

            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();

            IQueryable<T_HR_COMPANY> ents = from a in dal.GetObjects()
                                            where a.CHECKSTATE == checkState && a.EDITSTATE == state
                                            select a;

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_COMPANY>(ents, pageIndex, pageSize, ref pageCount);

            return ents.Count() > 0 ? ents.ToArray() : null;
        }

        /// <summary>
        /// 读取资源版本
        /// </summary>
        /// <returns></returns>
        public string ReadVersion()
        {
            try
            {
                string fileName = System.Web.HttpContext.Current.Server.MapPath("OrgResourceVersion\\OrgResourceVersion.xml");
                System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                byte[] version = new byte[fileStream.Length];
                fileStream.Read(version, 0, version.Length);
                fileStream.Close();
                string ver = System.Text.Encoding.UTF8.GetString(version);
                return ver;
            }
            catch (Exception ex)
            {
                Tracer.Debug("COMPANYBLL-ReadVersion" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return string.Empty;
            }

        }
        /// <summary>
        ///修改版本号
        /// </summary>
        public void EditVersion(string ResourceName)
        {
            try
            {
                string version = "0";
                string fileName = System.Web.HttpContext.Current.Server.MapPath("OrgResourceVersion\\OrgResourceVersion.xml");
                System.Xml.Linq.XDocument sourceFile = System.Xml.Linq.XDocument.Load(fileName);
                var ent = from xml in sourceFile.Root.Elements("Resource")
                          where xml.Attribute("Name").Value == ResourceName
                          select xml;
                if (ent.Count() > 0)
                {
                    var tmp = ent.FirstOrDefault();
                    if (tmp.Attribute("Version").Value != null)
                    {
                        version = tmp.Attribute("Version").Value;
                    }
                    double ver;
                    double.TryParse(version, out ver);
                    tmp.Attribute("Version").Value = (ver + 1).ToString();
                    tmp.Attribute("EditDate").Value = System.DateTime.Now.ToString();
                    sourceFile.Save(fileName);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("COMPANYBLL-EditVersion " + ResourceName + System.DateTime.Now.ToString() + " " + ex.ToString());
            }

        }
        /// <summary>
        /// 修改公司排序号
        /// </summary>
        /// <param name="company"></param>
        /// <param name="strMsg"></param>
        public void CompanyIndexUpdate(T_HR_COMPANY company, ref string strMsg)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.COMPANYID == company.COMPANYID
                               select ent);

                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    entity.SORTINDEX = company.SORTINDEX;
                    dal.Update(entity);
                    new CompanyBLL().EditVersion("公司");
                    //  WorkflowUtility.CallWorkflow("申请撤消工作流", entity);
                }
                else
                {
                    // throw new Exception("没有找到对应实体！");
                    strMsg = "NOTFOUND";
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " CompanyIndexUpdate:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 修改公司排序号
        /// </summary>
        /// <param name="company"></param>
        /// <param name="strMsg"></param>
        public void OrgChange()
        {
            try
            {
                SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient FlowEngine = new SMT.SaaS.BLLCommonServices.EngineConfigWS.EngineWcfGlobalFunctionClient();
                SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[] msgs = new SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg[1];
                SMT.SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg msg = new SaaS.BLLCommonServices.EngineConfigWS.CustomUserMsg();
                msg.UserID = "d32ad3d3-bd42-4552-874c-484f595e4286";
                msg.FormID = Guid.NewGuid().ToString();

                msgs[0] = msg;
                FlowEngine.ApplicationNotesTrigger(msgs, "HR", "变更提醒");
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " OrgChange:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 获取指定时间后更新的公司
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public IQueryable<T_HR_COMPANY> GetCompanyWithSpecifiedTime(string startDate)
        {
            DateTime start;
            bool flag;
            flag = DateTime.TryParse(startDate, out start);
            if (flag)
            {
                IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                                where c.UPDATEDATE >= start || c.CREATEDATE >= start
                                                select c;
                return ents;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取公司视图
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_COMPANY> GetALLCompanyView(string userID)
        {
            #region 根据权限过滤数据
            List<object> paras = new List<object>();
            string filterString = "";
            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_COMPANY", "3");
                SetCompanyFilter(ref filterString, ref paras, userID);
            }
            #endregion
            IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                            select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            var comapnyViews = from c in ents
                               select new V_COMPANY
                               {
                                   COMPANYID = c.COMPANYID,
                                   CNAME = c.BRIEFNAME == null ? c.CNAME : c.BRIEFNAME,
                                   ENAME = c.ENAME,
                                   COMPANRYCODE = c.COMPANRYCODE,
                                   BRIEFNAME = c.BRIEFNAME,
                                   FATHERID = c.FATHERID,
                                   FATHERTYPE = c.FATHERTYPE,
                                   FATHERCOMPANYID = c.T_HR_COMPANY2.COMPANYID,
                                   SORTINDEX = c.SORTINDEX,
                                   CHECKSTATE = c.CHECKSTATE,
                                   EDITSTATE = c.EDITSTATE,
                                   COMPANYTYPE = c.COMPANYTYPE
                               };

            return comapnyViews;

        }
        /// <summary>
        /// 获取指定时间后更新的是公司视图
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_COMPANY> GetCompanyViewByDateAndUser(string startDate, string userID)
        {
            #region 根据权限过滤数据
            List<object> paras = new List<object>();
            string filterString = "";
            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_COMPANY", "3");
                SetCompanyFilter(ref filterString, ref paras, userID);
            }
            #endregion
            IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                            select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }

            DateTime start;
            bool flag;
            flag = DateTime.TryParse(startDate, out start);
            if (!flag)
                return null;
            var comapnyViews = from c in ents
                               where c.UPDATEDATE >= start
                               select new V_COMPANY
                               {
                                   COMPANYID = c.COMPANYID,
                                   CNAME = c.BRIEFNAME == null ? c.CNAME : c.BRIEFNAME,
                                   ENAME = c.ENAME,
                                   COMPANRYCODE = c.COMPANRYCODE,
                                   BRIEFNAME = c.BRIEFNAME,
                                   FATHERID = c.FATHERID,
                                   FATHERTYPE = c.FATHERTYPE,
                                   FATHERCOMPANYID = c.T_HR_COMPANY2.COMPANYID,
                                   SORTINDEX = c.SORTINDEX,
                                   CHECKSTATE = c.CHECKSTATE,
                                   EDITSTATE = c.EDITSTATE,
                                   COMPANYTYPE = c.COMPANYTYPE
                               };

            return comapnyViews;

        }

        /// <summary>
        /// 根据实体权限获取公司视图
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="perm"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IQueryable<V_COMPANY> GetCompanyView(string userID, string perm, string entity)
        {
            #region 根据权限过滤数据
            if (string.IsNullOrEmpty(perm))
            {
                perm = "3";
            }
            if (string.IsNullOrEmpty(entity))
            {
                entity = "T_HR_COMPANY";
            }
            List<object> paras = new List<object>();
            string filterString = "";
            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, entity, perm);
                SetCompanyFilter(ref filterString, ref paras, userID);
            }
            #endregion
            IQueryable<T_HR_COMPANY> ents = from c in dal.GetObjects()
                                            select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            var comapnyViews = from c in ents
                               select new V_COMPANY
                               {
                                   COMPANYID = c.COMPANYID,
                                   CNAME = c.BRIEFNAME == null ? c.CNAME : c.BRIEFNAME,
                                   ENAME = c.ENAME,
                                   COMPANRYCODE = c.COMPANRYCODE,
                                   BRIEFNAME = c.BRIEFNAME,
                                   FATHERID = c.FATHERID,
                                   FATHERTYPE = c.FATHERTYPE,
                                   FATHERCOMPANYID = c.T_HR_COMPANY2.COMPANYID,
                                   SORTINDEX = c.SORTINDEX,
                                   CHECKSTATE = c.CHECKSTATE,
                                   EDITSTATE = c.EDITSTATE,
                                   COMPANYTYPE = c.COMPANYTYPE
                               };

            return comapnyViews;

        }

        /// <summary>
        /// 根据公司ID获取所有子公司
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        public List<string> GetChildCompanyByCompanyID(List<string> ParaCompanyIDs)
        {

            List<string> companyIDs = new List<string>();
            if (ParaCompanyIDs == null || ParaCompanyIDs.Count == 0)
            {
                return companyIDs;
            }
            var ents = from c in dal.GetObjects<T_HR_COMPANY>()
                       where c.EDITSTATE == "1"
                       select c;
            if (ents.Count() <= 0)
            {
                return companyIDs;
            }
            List<T_HR_COMPANY> companyLists = ents.ToList();
            foreach (var companyID in ParaCompanyIDs)
            {
                if (companyIDs.Contains(companyID))
                {
                    continue;
                }
                var companys = companyLists.Where(s => s.FATHERID == companyID && s.FATHERTYPE == "0");
                if (companys.Count() <= 0)
                {
                    continue;
                }

                GetCompanychild(ref companyIDs, companys.ToList(), companyLists);
            }
            return companyIDs;
        }

        public void GetCompanychild(ref List<string> companyids, List<T_HR_COMPANY> companys, List<T_HR_COMPANY> companyLists)
        {
            if (companys.Count == 0)
            {
                return;
            }
            foreach (var ent in companys)
            {
                if (companyids.Contains(ent.COMPANYID))
                {
                    continue;
                }
                companyids.Add(ent.COMPANYID);
                var coms = companyLists.Where(s => s.FATHERID == ent.COMPANYID && s.FATHERTYPE == "0");
                if (coms.Count() <= 0)
                {
                    continue;
                }
                GetCompanychild(ref companyids, coms.ToList(), companyLists);
            }

        }


        /// <summary>
        /// 根据用户权限分步获取组织架构
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="perm">权限类型（查看，新建，修改）</param>
        /// <param name="entity">实体</param>
        /// <param name="flag">实体类型（公司，部门，岗位）</param>
        /// <param name="orgID">实体ID</param>
        /// <returns></returns>
        public List<string> GetOrgnazationsBystep(string userID, string perm, string entity, string flag, string orgID)
        {
            #region 根据权限过滤数据
            if (string.IsNullOrEmpty(perm))
            {
                perm = "3";
            }
            if (string.IsNullOrEmpty(entity))
            {
                if (string.IsNullOrEmpty(flag) || flag == "COMPANY")
                {
                    entity = "T_HR_COMPANY";
                }
                if (flag == "POST")
                {
                    entity = "T_HR_POST";
                }
                if (flag == "DEPARTMENT")
                {
                    entity = "T_HR_DEPARTMENT";
                }
            }

            List<string> orgList = new List<string>();
            #endregion


            // 查公司的下级
            if (string.IsNullOrEmpty(flag) || flag == "COMPANY")
            {
                var ents = GetCompanyActived(userID, perm, entity);
                #region 顶级的公司
                if (string.IsNullOrEmpty(flag))
                {
                    var TopCompany = from c in ents
                                     where c.FATHERID == null
                                     orderby c.SORTINDEX
                                     select c;
                    if (TopCompany.Count() > 0)
                    {
                        foreach (var ent in TopCompany)
                        {
                            var childCompanys = from c in ents
                                                where c.FATHERID == ent.COMPANYID
                                                select c;
                            var childDepartments = from b in dal.GetObjects<T_HR_DEPARTMENT>()
                                                   where b.FATHERID == ent.COMPANYID
                                                   select b;
                            int haschilds = 0;
                            if (childCompanys.Count() > 0 || childDepartments.Count() > 0)
                            {
                                haschilds = 1;
                            }
                            string org = "COMPANYЁ" + ent.COMPANYID + "Ё" + (string.IsNullOrEmpty(ent.BRIEFNAME) ? ent.CNAME : ent.BRIEFNAME) + "Ё" + haschilds.ToString();
                            orgList.Add(org);
                        }
                        return orgList;
                    }
                    else
                    {
                        return null;
                    }
                }

                #endregion
                #region 公司下级
                if (flag == "COMPANY")
                {
                    ents = ents.Where(s => s.FATHERID == orgID);
                    foreach (var ent in ents)
                    {
                        var childCompanys = from c in ents
                                            where c.FATHERID == ent.COMPANYID
                                            select c;
                        var childDepartments = from b in dal.GetObjects<T_HR_DEPARTMENT>()
                                               where b.FATHERID == ent.COMPANYID && b.EDITSTATE == "1"
                                               select b;
                        int haschilds = 0;
                        if (childCompanys.Count() > 0 || childDepartments.Count() > 0)
                        {
                            haschilds = 1;
                        }
                        string org = "COMPANYЁ" + ent.COMPANYID + "Ё" + (string.IsNullOrEmpty(ent.BRIEFNAME) ? ent.CNAME : ent.BRIEFNAME) + "Ё" + haschilds.ToString();
                        orgList.Add(org);
                    }
                    var departments = from c in dal.GetObjects<T_HR_DEPARTMENT>()
                                      where c.FATHERID == orgID && c.EDITSTATE == "1"
                                      orderby c.SORTINDEX
                                      select new
                                      {
                                          DEPARTMENTID = c.DEPARTMENTID,
                                          DEPARTMENTNAME = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME

                                      };
                    foreach (var dep in departments)
                    {
                        var childCompanys = from c in ents
                                            where c.FATHERID == dep.DEPARTMENTID
                                            select c;
                        var childDepartments = from b in dal.GetObjects<T_HR_DEPARTMENT>()
                                               where b.FATHERID == dep.DEPARTMENTID && b.EDITSTATE == "1"
                                               select b;
                        var childPosts = from p in dal.GetObjects<T_HR_POST>()
                                         where p.T_HR_DEPARTMENT.DEPARTMENTID == dep.DEPARTMENTID && p.EDITSTATE == "1"
                                         select p;
                        int haschilds = 0;
                        if (childCompanys.Count() > 0 || childDepartments.Count() > 0 || childPosts.Count() > 0)
                        {
                            haschilds = 1;
                        }
                        string org = "DEPARTMENTЁ" + dep.DEPARTMENTID + "Ё" + dep.DEPARTMENTNAME + "Ё" + haschilds.ToString();
                        orgList.Add(org);
                    }
                    return orgList.Count > 0 ? orgList : null;
                }
                #endregion
            }
            #region 部门的下级
            if (flag == "DEPARTMENT")
            {
                //下级公司
                var companys = GetCompanyActived(userID, perm, entity);
                companys = companys.Where(s => s.FATHERID == orgID);
                foreach (var com in companys)
                {
                    var childCompanys = from c in companys
                                        where c.FATHERID == com.COMPANYID
                                        select c;
                    var childDepartments = from b in dal.GetObjects<T_HR_DEPARTMENT>()
                                           where b.FATHERID == com.COMPANYID && b.EDITSTATE == "1"
                                           select b;
                    int haschilds = 0;
                    if (childCompanys.Count() > 0 || childDepartments.Count() > 0)
                    {
                        haschilds = 1;
                    }
                    string org = "COMPANYЁ" + com.COMPANYID + "Ё" + (string.IsNullOrEmpty(com.BRIEFNAME) ? com.CNAME : com.BRIEFNAME) + "Ё" + haschilds;
                    orgList.Add(org);
                }
                //下级部门
                var departments = from c in dal.GetObjects<T_HR_DEPARTMENT>()
                                  where c.FATHERID == orgID && c.EDITSTATE == "1"
                                  orderby c.SORTINDEX
                                  select new
                                  {
                                      DEPARTMENTID = c.DEPARTMENTID,
                                      DEPARTMENTNAME = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                                  };
                foreach (var dep in departments)
                {
                    var childCompanys = from c in companys
                                        where c.FATHERID == dep.DEPARTMENTID
                                        select c;
                    var childDepartments = from b in dal.GetObjects<T_HR_DEPARTMENT>()
                                           where b.FATHERID == dep.DEPARTMENTID && b.EDITSTATE == "1"
                                           select b;
                    var childPosts = from p in dal.GetObjects<T_HR_POST>()
                                     where p.T_HR_DEPARTMENT.DEPARTMENTID == dep.DEPARTMENTID && p.EDITSTATE == "1"
                                     select p;
                    int haschilds = 0;
                    if (childCompanys.Count() > 0 || childDepartments.Count() > 0 || childPosts.Count() > 0)
                    {
                        haschilds = 1;
                    }
                    string org = "DEPARTMENTЁ" + dep.DEPARTMENTID + "Ё" + dep.DEPARTMENTNAME + "Ё" + haschilds.ToString();
                    orgList.Add(org);
                }
                //部门下的岗位
                var posts = from c in dal.GetObjects<T_HR_POST>()
                            where c.T_HR_DEPARTMENT.DEPARTMENTID == orgID && c.EDITSTATE == "1"
                            select new
                            {
                                POSTID = c.POSTID,
                                POSTNAME = c.T_HR_POSTDICTIONARY.POSTNAME
                            };
                foreach (var post in posts)
                {
                    var employees = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                    where c.T_HR_POST.POSTID == post.POSTID && c.EDITSTATE == "1"
                                    select c;
                    int haschilds = 0;
                    if (employees.Count() > 0)
                    {
                        haschilds = 1;
                    }
                    string org = "POSTЁ" + post.POSTID + "Ё" + post.POSTNAME + "Ё" + haschilds.ToString(); ;
                    orgList.Add(org);
                }
            }
            #endregion
            #region  岗位下的员工
            if (flag == "POST")
            {
                var employees = from c in dal.GetObjects<T_HR_EMPLOYEEPOST>()
                                where c.EDITSTATE == "1" && c.T_HR_POST.POSTID == orgID
                                select new
                                {
                                    EMPLOYEENAME = c.T_HR_EMPLOYEE.EMPLOYEECNAME,
                                    EMPLOYEEID = c.T_HR_EMPLOYEE.EMPLOYEEID,
                                    POSTLEVEL = c.POSTLEVEL,
                                    POSTID = c.T_HR_POST.POSTID,
                                    DEPARTMENTID = c.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID,
                                    COMPANYID = c.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID

                                };
                foreach (var employee in employees)
                {
                    //类型  员工ID  姓名 岗位级别  岗位Id 部门Id  公司Id
                    string org = "EMPLOYEEЁ" + employee.EMPLOYEEID + "Ё" + employee.EMPLOYEENAME + "Ё" + employee.POSTLEVEL + "Ё" + employee.POSTID + "Ё" + employee.DEPARTMENTID + "Ё" + employee.COMPANYID;
                    orgList.Add(org);
                }
            }
            #endregion


            return orgList;

        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var company = (from c in dal.GetObjects<T_HR_COMPANY>().Include("T_HR_COMPANY2")
                               where c.COMPANYID == EntityKeyValue
                               select c).FirstOrDefault();
                if (company != null)
                {

                    if (company.T_HR_COMPANY2 != null)
                    {
                        string companyid = company.T_HR_COMPANY2.COMPANYID;
                        company.T_HR_COMPANY2 = new T_HR_COMPANY();
                        company.T_HR_COMPANY2.COMPANYID = companyid;
                    }
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        if (company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            company.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                        }
                        else
                        {
                            company.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                        }
                    }
                    if (CheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        if (company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            company.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                        }
                        else
                        {
                            company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        }
                    }
                    company.CHECKSTATE = CheckState;
                    CompanyUpdate(company, ref strMsg);
                    if (company.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        SMT.Foundation.Log.Tracer.Debug("引擎开始调用即时通讯接口，添加公司");
                        this.AddCompanyInfoToIM(company);
                    }
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


        #region 调用即时通讯接口
        /// <summary>
        /// 添加公司信息至即时通讯接口
        /// </summary>
        /// <param name="Company"></param>
        private void AddCompanyInfoToIM(T_HR_COMPANY Company)
        {
            string StrMessage = "";
            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯服务AddCompanyInfoToIM：" + StrMessage);
            try
            {
                DataSyncServiceClient Client = new DataSyncServiceClient();
                string StrType = "1";//类型 1 公司  0 部门
                string StrValid = "";//是否可用  0 不可用 1 可用 
                string StrFatherId = "";//上级公司ID
                if (Company.EDITSTATE == "1")
                {
                    StrValid = "1";
                }
                else
                {
                    StrValid = "0";
                }
                if (Company.T_HR_COMPANY2 != null)
                {
                    StrFatherId = Company.T_HR_COMPANY2.COMPANYID;
                }
                else
                {
                    StrFatherId = "0";
                }

                StrMessage = Client.AddOrUpdateCompanyDepartment(Company.COMPANYID, Company.CNAME, StrFatherId, StrType, StrValid,"0");
                SMT.Foundation.Log.Tracer.Debug("更新即时通讯接口AddOrUpdateImDepartment的结果：" + StrMessage);
            }
            catch (Exception ex)
            {
                StrMessage = ex.ToString() + Company.COMPANYID + "公司名称：" + Company.CNAME;
                SMT.Foundation.Log.Tracer.Debug("即时通讯服务错误AddCompanyInfoToIM：" + StrMessage);
            }
        }
        #endregion

        /// <summary>
        /// 获取最高层的ID，如神州通集团
        /// </summary>
        /// <param name="companyid"></param>
        /// <returns></returns>
        private string GetTopCompany(string companyid)
        {
            try
            {
                //记录结果用
                string topCompanyid = string.Empty;
                var entComp = from c in dal.GetObjects()
                              where c.COMPANYID == companyid
                              select c;
                if (entComp != null && entComp.Count()>0)
                {
                    T_HR_COMPANY curCompany = entComp.FirstOrDefault();
                    //当父公司id为空时返回结果，否则递归获取
                    if (string.IsNullOrEmpty(curCompany.FATHERID))
                    {
                        topCompanyid = curCompany.COMPANYID;
                    }
                    else
                    {
                        topCompanyid = GetTopCompany(curCompany.FATHERID);
                    }
                }
                return topCompanyid;
            }
            catch (Exception ex)
            {
                Tracer.Debug("CompanyBll-GetTopCompany:"+ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 验证某公司的根公司是否是神州通集团
        /// </summary>
        /// <param name="companyid"></param>
        /// <returns></returns>
        public bool IsTopCompanySmt(string companyid)
        {
            try
            {
                string topCompanyId = GetTopCompany(companyid);
                //神州通集团公司ID：
                if (topCompanyId == "427eb67d-35b4-47a9-9609-baf5355d2ed5") return true;
                else return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("CompanyBll-IsTopCompanySmt:"+ex.ToString());
                return false ;
            }
        }
    }
}
