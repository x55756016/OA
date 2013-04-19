/*
 * 文件名：CompanyBLL.cs
 * 作  用：
 * 创建人：
 * 创建时间：2010-2-26 14:19:12
 * 修改人：向寒咏
 * 修改说明：增加缓存
 * 修改时间：2010-7-7 14:19:12
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;

namespace SMT.HRM.BLL
{
    public class CompanyBLLBak : BaseBll<T_HR_COMPANY>, ILookupEntity
    {
        private List<T_HR_COMPANY> listCOMPANY;

        public List<T_HR_COMPANY> ListCOMPANY
        {
            get
            {

                List<T_HR_COMPANY> lsdic;
                if (CacheManager.GetCache("T_HR_COMPANY") != null)
                {
                    lsdic = (List<T_HR_COMPANY>)CacheManager.GetCache("T_HR_COMPANY");
                }
                else
                {

                    var ents = from a in dal.GetObjects().Include("T_HR_COMPANY2")
                               select a;

                    lsdic = ents.ToList();
                    CacheManager.AddCache("T_HR_COMPANY", lsdic);
                }

                return lsdic.Count() > 0 ? lsdic : null;
            }


            set { listCOMPANY = value; }
        }

        /// <summary>
        /// 根据公司ID获取公司
        /// </summary>
        /// <param name="strCompanyID"></param>
        /// <returns></returns>
        public T_HR_COMPANY GetCompanyByID(string strCompanyID)
        {
            IQueryable<T_HR_COMPANY> ents = from c in ListCOMPANY.AsQueryable()
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
            IQueryable<T_HR_COMPANY> ents = from c in ListCOMPANY.AsQueryable()                                       
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
            IQueryable<T_HR_COMPANY> ents = from c in ListCOMPANY.AsQueryable()
                                            join d in dal.GetObjects <T_HR_DEPARTMENT>() on c.COMPANYID equals d.T_HR_COMPANY.COMPANYID
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
            IQueryable<T_HR_COMPANY> ents = from c in ListCOMPANY.AsQueryable()
                                            join d in dal.GetObjects <T_HR_DEPARTMENT>() on c.COMPANYID equals d.T_HR_COMPANY.COMPANYID
                                            join p in dal.GetObjects <T_HR_POST>().Include("T_HR_DEPARTMENT") on d.DEPARTMENTID equals p.T_HR_DEPARTMENT.DEPARTMENTID
                                            join ep in dal.GetObjects <T_HR_EMPLOYEEPOST>().Include("T_HR_EMPLOYEE").Include("T_HR_POST") on p.POSTID equals ep.T_HR_POST.POSTID
                                            join e in dal.GetObjects <T_HR_EMPLOYEE>() on ep.T_HR_EMPLOYEE.EMPLOYEEID equals e.EMPLOYEEID
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

            IQueryable<T_HR_COMPANY> ents = ListCOMPANY.AsQueryable();
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }
        /// <summary>
        /// 获取除审核状态不通过和编辑状态为删除全部可用的公司信息
        /// </summary>
        /// <returns>公司列表</returns>
        public IQueryable<T_HR_COMPANY> GetCompanyAll(string userID)
        {
            string state = ((int)EditStates.Deleted).ToString();
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
            }
            else
            {
                filterString = "EDITSTATE<>@" + paras.Count;
                paras.Add(state);
            }

            filterString += " and CHECKSTATE<>@" + paras.Count;
            paras.Add(checkState);

            IQueryable<T_HR_COMPANY> ents = ListCOMPANY.AsQueryable();
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
            EmployeeBLL bll = new EmployeeBLL();
            T_HR_EMPLOYEE emp = bll.GetEmployeeByID(employeeID);
            if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
            {
                emp.T_HR_EMPLOYEEPOST.Load();
            }

            foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
            {
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
            }
        }

        private void SetInferiorCompany(T_HR_EMPLOYEEPOST ep, ref string filterString, ref List<object> queryParas)
        {
            var tempEnt = dal.GetObjects().Where(s => s.T_HR_COMPANY2.COMPANYID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
            //from ent in ListCOMPANY.
            //where ent.T_HR_COMPANY2.COMPANYID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID
            //select ent;
            tempEnt.Count();
                            // 
            getAllCompany(ref filterString, ref queryParas, tempEnt.ToList());
        }

        private void getAllCompany(ref string filterString, ref List<object> queryParas, List<T_HR_COMPANY> tempEnt)
        {
            if (tempEnt != null)
            {
                foreach (var ent in tempEnt)
                {
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " OR ";

                    filterString += "COMPANYID==@" + queryParas.Count().ToString();
                    queryParas.Add(ent.COMPANYID);
                    var tempChildEnt = dal.GetObjects().Where(s => s.T_HR_COMPANY2.COMPANYID == ent.COMPANYID).ToList();
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

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref tempString, ref queryParas, userID, "T_HR_COMPANY");
                SetCompanyFilter(ref tempString, ref queryParas, userID);
            }


            IQueryable<T_HR_COMPANY> ents = ListCOMPANY.AsQueryable();
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

            SetFilterWithflow("COMPANYID", "T_HR_COMPANY", userID, ref strCheckState, ref tempString, ref queryParas);

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
            var ents = from ent in ListCOMPANY.AsQueryable()
                       where ent.COMPANYID == companyID
                       select ent;

            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        /// <summary>
        /// 添加公司
        /// </summary>
        /// <param name="entity">公司实例</param>
        public void CompanyAdd(T_HR_COMPANY entity)
        {
            try
            {
                var tempEnt = dal.GetObjects().FirstOrDefault(s => s.COMPANRYCODE == entity.COMPANRYCODE
                || s.CNAME == entity.CNAME);
                if (tempEnt != null)
                {
                    throw new Exception("Repetition");
                }
                T_HR_COMPANY ent = new T_HR_COMPANY();
                Utility.CloneEntity<T_HR_COMPANY>(entity, ent);

                //如果父公司为空，就不赋值
                if (entity.T_HR_COMPANY2 != null)
                {
                    ent.T_HR_COMPANY2Reference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY2.COMPANYID);
                }
                dal.Add(ent);

                WorkflowUtility.CallWorkflow("公司申请审核工作流", ent);
                CacheManager.RemoveCache("T_HR_COMPANY");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// 变更公司
        /// </summary>
        /// <param name="entity">公司实例</param>
        public void CompanyUpdate(T_HR_COMPANY entity)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => (s.COMPANRYCODE == entity.COMPANRYCODE
                   || s.CNAME == entity.CNAME) && s.COMPANYID != entity.COMPANYID);
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }
                var ents = from ent in dal.GetObjects()
                           where ent.COMPANYID == entity.COMPANYID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    Utility.CloneEntity<T_HR_COMPANY>(entity, ent);
                    //如果父公司为空，就不赋值
                    if (entity.T_HR_COMPANY2 != null)
                    {
                        ent.T_HR_COMPANY2Reference.EntityKey =
                            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY2.COMPANYID);
                    }
                    //如果审核状态为审核通过则添加公司历史
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
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
                        companyHis.OWNERID = entity.OWNERDEPARTMENTID;
                        companyHis.OWNERPOSTID = entity.OWNERPOSTID;

                        if (entity.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            companyHis.CANCELDATE = DateTime.Now;
                        }
                        //DataContext.AddObject("T_HR_COMPANYHISTORY", companyHis);
                        dal.AddToContext(companyHis);
                        GetCompanyForOutEngineXml(entity);//向引擎推送计算员工企业工龄的接口契约
                    }
                    dal.SaveContextChanges();
                    CacheManager.RemoveCache("T_HR_COMPANY");
                    //WorkflowUtility.CallWorkflow("公司审批审核工作流", ent);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// 撤销公司
        /// </summary>
        /// <param name="entity">公司实例</param>
        /// <returns>是否成功撤销</returns>
        public bool CompanyCancel(T_HR_COMPANY entity)
        {
            try
            {
                DepartmentBLL departBll = new DepartmentBLL();

                if (GetChildOrgCount(entity.COMPANYID) > 0)
                {
                    throw new Exception("当前公司有下级公司，不能撤消！");
                }
                else
                {
                    if (departBll.GetDepartCount(entity.COMPANYID) > 0)
                    {
                        throw new Exception("当前公司下有部门，不能撤消！");
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
                        ent.UPDATEDATE = entity.UPDATEDATE;
                        ent.UPDATEUSERID = entity.UPDATEUSERID;
                        dal.Update(ent);
                        CacheManager.RemoveCache("T_HR_COMPANY");
                        //WorkflowUtility.CallWorkflow("公司撤消审批工作流", entity);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        /// <summary>
        /// 删除公司
        /// </summary>
        /// <param name="entity">公司ID</param>
        /// <returns></returns>
        public void CompanyDelete(string id)
        {
            try
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
                        throw new Exception("当前公司有下级公司，不能删除！");
                    }
                    else
                    {
                        if (departBll.GetDepartCount(entity.COMPANYID) > 0)
                        {
                            throw new Exception("当前公司下有部门，不能删除！");
                        }
                        else
                        {
                            dal.Delete(entity);
                            CacheManager.RemoveCache("T_HR_COMPANY");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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
            if (ents.Count() > 0)
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
        /// 当前公司有字公司的数量
        /// </summary>
        /// <param name="companyID">父公司ID</param>
        /// <returns>返回公司数量</returns>
        private int GetChildOrgCount(string companyID)
        {
            var ents = from o in listCOMPANY.AsQueryable()
                       where o.T_HR_COMPANY2.COMPANYID == companyID
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

            IQueryable<T_HR_COMPANY> ents = from a in listCOMPANY.AsQueryable()
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
    }
}
