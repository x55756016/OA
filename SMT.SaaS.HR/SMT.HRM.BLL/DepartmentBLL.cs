﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Dynamic;
using System.Data.Objects;
using SMT.Foundation.Log;
using SMT.HRM.CustomModel;
using SMT.HRM.IMServices.IMServiceWS;//即时通讯接口
namespace SMT.HRM.BLL
{
    public class DepartmentBLL : BaseBll<T_HR_DEPARTMENT>, ILookupEntity, IOperate
    {
        /// <summary>
        /// 根据部门ID，获取该部门的全称（如果该ID对应的部门为子部门，则将父部门的名称也加上）
        /// </summary>
        /// <param name="p"></param>
        /// <param name="strFullDeptName"></param>
        public void GetFullDepartmentNameByID(string departID, ref string strFullDeptName)
        {
            try
            {
                var ents = dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Where(c => c.DEPARTMENTID == departID);
                if (ents == null)
                {
                    return;
                }

                if (ents.FirstOrDefault() == null)
                {
                    return;
                }

                if(ents.FirstOrDefault().T_HR_DEPARTMENTDICTIONARY == null)
                {
                    return;
                }


                if (string.IsNullOrWhiteSpace(ents.FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(strFullDeptName))
                {
                    strFullDeptName = ents.FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                }
                else
                {
                    strFullDeptName = ents.FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + "-" + strFullDeptName;  
                }
                
                GetFullDepartmentNameByID(ents.FirstOrDefault().FATHERID, ref strFullDeptName);
            }
            catch (Exception ex)
            {
                Utility.SaveLog("调用函数GetFullDepartmentNameByID出错，参数departID：" + departID + ", 出错信息：" + ex.ToString());
            }
        }

        /// <summary>
        /// 获取全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActived(string userID)
        {
            IQueryable<T_HR_DEPARTMENT> ents = GetDepartmentActived(userID, "3", "T_HR_DEPARTMENT");
            return ents;
        }

        /// <summary>
        /// 获取全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActived(string userID, string perm, string entity)
        {
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();

            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, entity, perm);
                SetDepartmentFilter(ref filterString, ref paras, userID);
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

            IQueryable<T_HR_DEPARTMENT> ents = dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY");
            //dal.GetObjects().MergeOption = MergeOption.NoTracking;
            //dal.GetObjects()DICTIONARY.MergeOption = MergeOption.NoTracking;
            //DataContext.T_HR_COMPANY.MergeOption = MergeOption.NoTracking;

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }

        /// <summary>
        /// 获取当前员工ID隶属于公司的所有部门
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="queryParas"></param>
        /// <param name="employeeID"></param>
        private void SetDepartmentFilter(ref string filterString, ref List<object> queryParas, string employeeID)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return;
            }
            List<string> departmentIDs = new List<string>();
            EmployeeBLL bll = new EmployeeBLL();
            T_HR_EMPLOYEE emp = bll.GetEmployeeByID(employeeID);
            if (!emp.T_HR_EMPLOYEEPOST.IsLoaded)
            {
                emp.T_HR_EMPLOYEEPOST.Load();
            }
            foreach (T_HR_EMPLOYEEPOST ep in emp.T_HR_EMPLOYEEPOST)
            {
                //不是有效岗位，跳过
                if (ep.EDITSTATE != "1")
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(filterString))
                    filterString += " OR ";

                if (!ep.T_HR_POSTReference.IsLoaded)
                    ep.T_HR_POSTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENTReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENTReference.Load();

                if (ep.T_HR_POST != null && ep.T_HR_POST.T_HR_DEPARTMENT != null && ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.IsLoaded == false)
                    ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANYReference.Load();

                filterString += "DEPARTMENTID==@" + queryParas.Count().ToString();
                queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);

                departmentIDs.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                //下级公司
                //SetInferiorCompany(ep, ref filterString, ref queryParas);  
                //SetInferiorCompany(ep.T_HR_POST.T_HR_DEPARTMENT, ref filterString, ref queryParas);  
            }
            #region
            //foreach (T_HR_DEPARTMENT dep in departments)
            //{
            //    bool hasFather = true;
            //    T_HR_DEPARTMENT deptemp = dep;
            //    while (hasFather)
            //    {
            //        if (!string.IsNullOrEmpty(deptemp.FATHERID))
            //        {
            //            var tmp = from c in dal.GetObjects<T_HR_DEPARTMENT>()
            //                      where c.DEPARTMENTID == deptemp.FATHERID
            //                      select c;
            //            if (tmp.Count() > 0)
            //            {
            //                if (!string.IsNullOrEmpty(filterString))
            //                    filterString += " OR ";

            //                filterString += " DEPARTMENTID==@" + queryParas.Count().ToString();
            //                queryParas.Add(tmp.FirstOrDefault().DEPARTMENTID);
            //                deptemp = tmp.FirstOrDefault();
            //            }
            //            else
            //            {
            //                hasFather = false;
            //            }
            //        }

            //    }
            //}
            #endregion
            /// 获取员工所在部门的所有上级机构中的部门
            foreach (string depID in departmentIDs)
            {
                Dictionary<string, string> dictIDs = GetFatherByDepartmentID(depID);
                foreach (KeyValuePair<string, string> item in dictIDs)
                {
                    if (item.Value == "1")
                    {
                        if (!string.IsNullOrEmpty(filterString))
                            filterString += " OR ";
                        filterString += " DEPARTMENTID==@" + queryParas.Count().ToString();
                        queryParas.Add(item.Key);
                    }
                }

            }
        }

        //private void SetInferiorCompany(T_HR_DEPARTMENT ep, ref string filterString, ref List<object> queryParas)
        //{
        //    var tempEnt = dal.GetObjects().Where(s => s.DEPARTMENTID == ep.DEPARTMENTID);
        //    if (tempEnt != null)
        //    {
        //        foreach (var ent in tempEnt)
        //        {
        //            if (!string.IsNullOrEmpty(filterString))
        //                filterString += " OR ";

        //            filterString += "DEPARTMENTID==@" + queryParas.Count().ToString();
        //            queryParas.Add(ep.DEPARTMENTID);
        //            SetInferiorCompany(ent, ref filterString, ref queryParas);
        //        }
        //    }
        //}
        private void SetInferiorCompany(T_HR_EMPLOYEEPOST ep, ref string filterString, ref List<object> queryParas)
        {
            var tempEnt = dal.GetObjects<T_HR_COMPANY>().Where(s => s.T_HR_COMPANY2.COMPANYID == ep.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
            if (tempEnt != null)
            {
                foreach (var ent in tempEnt)
                {
                    if (!string.IsNullOrEmpty(filterString))
                        filterString += " OR ";

                    filterString += "DEPARTMENTID==@" + queryParas.Count().ToString();
                    queryParas.Add(ep.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                    SetInferiorCompany(ep, ref filterString, ref queryParas);
                }
            }
        }


        /// <summary>
        /// 获取除审核状态不通过和编辑状态为删除全部可用的部门信息
        /// </summary>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentAll(string userID)
        {
            string state = ((int)EditStates.Deleted).ToString();
            string canceledEditState = ((int)EditStates.Canceled).ToString(); //by xiedx 已撤销的也不能看。。
            string checkState = ((int)CheckStates.UnApproved).ToString();
            //var ents = from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
            //           where a.EDITSTATE == state && a.CHECKSTATE == auditState
            //           select a;
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_DEPARTMENT");
                SetDepartmentFilter(ref filterString, ref paras, userID);
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

            IQueryable<T_HR_DEPARTMENT> ents = dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            return ents;
        }

        /// <summary>
        /// 获取指定公司全部可用的部门信息
        /// </summary>
        /// 函数创建人：吴鹏
        /// 函数创建时间：2010年1月23日, 16:32:22
        /// <param name="companyID">公司ID</param>
        /// <returns>可用部门信息列表</returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActivedByCompanyID(string companyID)
        {
            string strFilter = string.Empty;
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();
            List<string> strArgs = new List<string>();

            if (!string.IsNullOrEmpty(companyID))
            {
                strFilter = "T_HR_COMPANY.COMPANYID == @0";
                strArgs.Add(companyID);
            }
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " EDITSTATE==@" + strArgs.Count;
            strArgs.Add(state);
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " CHECKSTATE==@" + strArgs.Count;
            strArgs.Add(checkState);

            var q = from d in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                    select d;

            return q.Where(strFilter, strArgs.ToArray());
        }

        /// <summary>
        ///     获取带公司信息的部门ID -by luojie
        /// </summary>
        /// <param name="companyIds">字符串的公司ID，以‘，’分隔各个ID</param>
        /// <returns></returns>
        public List<V_DEPARTMENTSWITHCOMPANY> GetDepartmentByCompanyIDs(string companyIds)
        {
            try
            {
                if (!string.IsNullOrEmpty(companyIds))
                {
                    //获取部门与公司信息的linq语句
                    var QListDep = from c in dal.GetObjects<T_HR_COMPANY>()
                                   join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.COMPANYID equals d.T_HR_COMPANY.COMPANYID
                                   where c.COMPANYID.Contains(companyIds) && d.EDITSTATE == "1"
                                   select new V_DEPARTMENTSWITHCOMPANY
                                   {
                                       DEPARTMENTID = d.DEPARTMENTID,
                                       COMPANYID = c.COMPANYID,
                                       COMPANYNAME = c.CNAME,
                                       BRIEFNAME = c.BRIEFNAME
                                   };

                    return QListDep.ToList();
                }
            }
            catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "-DepartmentBll:GetDepartmentByCompanyIDs:" + ex.ToString());
            }      
            return null;
        }

        /// <summary>
        /// 根据公司ID和用户ID获取部门
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentActivedByCompanyIDAndUserID(string companyID, string userID)
        {
            string strFilter = string.Empty;
            string state = ((int)EditStates.Actived).ToString();
            string checkState = ((int)CheckStates.Approved).ToString();
            List<object> strArgs = new List<object>();

            if (!string.IsNullOrEmpty(companyID))
            {
                strFilter = "T_HR_COMPANY.COMPANYID == @0";
                strArgs.Add(companyID);
            }
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " EDITSTATE==@" + strArgs.Count;
            strArgs.Add(state);
            if (!string.IsNullOrEmpty(strFilter))
            {
                strFilter += " and ";
            }
            strFilter += " CHECKSTATE==@" + strArgs.Count;
            strArgs.Add(checkState);
            SetOrganizationFilter(ref strFilter, ref strArgs, userID, "T_HR_DEPARTMENT");

            var q = from d in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                    select d;

            return q.Where(strFilter, strArgs.ToArray());
        }
        /// <summary>
        /// 用于实体Grid中显示数据的分页查询,获取所有的部门信息
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_DEPARTMENT> DepartmentPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string strCheckState)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string tempString = "";
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                if (!string.IsNullOrEmpty(userID))
                {
                    SetOrganizationFilter(ref tempString, ref queryParas, userID, "T_HR_DEPARTMENT");
                    SetDepartmentFilter(ref tempString, ref queryParas, userID);
                }


                if (!string.IsNullOrEmpty(tempString))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        tempString = filterString + " and (" + tempString + ")";
                    }
                }
                else
                {
                    tempString = filterString;
                }
            }
            else
            {
                SetFilterWithflow("DEPARTMENTID", "T_HR_DEPARTMENT", userID, ref strCheckState, ref tempString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }

            IQueryable<T_HR_DEPARTMENT> ents = dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY");
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

            ents = Utility.Pager<T_HR_DEPARTMENT>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// 根据部门ID获取公司信息
        /// </summary>
        /// <param name="companyID">部门ID</param>
        /// <returns>返回部门信息</returns>
        public T_HR_DEPARTMENT GetDepartmentById(string depID)
        {
            var ents = from ent in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                       where ent.DEPARTMENTID == depID
                       select ent;

            return ents.Count()>0?ents.FirstOrDefault():null;
        }
        public List<T_HR_DEPARTMENT> GetDepartmentByCompanyId(string comapnyID)
        {
            var ents = from ent in dal.GetTable()
                       where ent.T_HR_COMPANY.COMPANYID == comapnyID
                       select ent;
            return ents.ToList();
        }
        /// <summary>
        /// 添加公司部门
        /// </summary>
        /// <param name="entity">公司部门实例</param>
        public void DepartmentAdd(T_HR_DEPARTMENT entity, ref string strMsg)
        {
            try
            {
                string checkState = Convert.ToInt32(CheckStates.UnApproved).ToString();
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_COMPANY.COMPANYID == entity.T_HR_COMPANY.COMPANYID
                && s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE == entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE && s.CHECKSTATE != checkState 
                && s.FATHERID == entity.FATHERID);
                if (temp != null)
                {
                    // throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                T_HR_DEPARTMENT ent = new T_HR_DEPARTMENT();
                Utility.CloneEntity<T_HR_DEPARTMENT>(entity, ent);
                ent.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                ent.T_HR_COMPANYReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY.COMPANYID);
                ent.CREATEDATE = System.DateTime.Now;
                // dal.Add(ent);
                Add(ent);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentAdd:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 变更公司部门
        /// </summary>
        /// <param name="entity">公司部门实例</param>
        public void DepartmentUpdate(T_HR_DEPARTMENT entity, ref string strMsg)
        {
            try
            {
                //string checkState = Convert.ToInt32(CheckStates.UnApproved).ToString();
                //string editState = Convert.ToInt32(EditStates.Canceled).ToString();

                string checkState = Convert.ToInt32(CheckStates.Approved).ToString();
                string editState = Convert.ToInt32(EditStates.Actived).ToString();
                var temp = dal.GetObjects().FirstOrDefault(s => s.T_HR_COMPANY.COMPANYID == entity.T_HR_COMPANY.COMPANYID
                && s.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE == entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTCODE
                && s.DEPARTMENTID != entity.DEPARTMENTID && s.CHECKSTATE == checkState && s.EDITSTATE == editState 
                && s.FATHERID == entity.FATHERID);//还要判断上级机构是否一样，这可能有问题
                if (temp != null)
                {
                    //throw new Exception("Repetition");
                    strMsg = "Repetition";
                    return;
                }
                //var ents = from ent in dal.GetObjects()
                //           where ent.DEPARTMENTID == entity.DEPARTMENTID
                //           select ent;
                //if (ents.Count() > 0)
                ////{
                //    var ent = ents.FirstOrDefault();
                //    Utility.CloneEntity<T_HR_DEPARTMENT>(entity, ent);
                entity.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENT", "DEPARTMENTID", entity.DEPARTMENTID);
                if (entity.T_HR_DEPARTMENTDICTIONARY != null)
                {
                    entity.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                    entity.T_HR_DEPARTMENTDICTIONARY.EntityKey =
                     new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                }
                if (entity.T_HR_COMPANY != null)
                {
                    entity.T_HR_COMPANYReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY.COMPANYID);
                    entity.T_HR_COMPANY.EntityKey = new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_COMPANY", "COMPANYID", entity.T_HR_COMPANY.COMPANYID);
                }
                // dal.Update(entity);
                entity.UPDATEDATE = System.DateTime.Now;
                Update(entity);
                //如果审核状态为审核通过则添加部门历史
                if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    DepartmentHistoryBLL dhbll = new DepartmentHistoryBLL();
                    T_HR_DEPARTMENTHISTORY departmentHis = new T_HR_DEPARTMENTHISTORY();
                    departmentHis.RECORDSID = Guid.NewGuid().ToString();
                    departmentHis.T_HR_DEPARTMENTDICTIONARY = new T_HR_DEPARTMENTDICTIONARY();
                    departmentHis.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID = entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID;
                    departmentHis.DEPARTMENTID = entity.DEPARTMENTID;
                    departmentHis.DEPARTMENTFUNCTION = entity.DEPARTMENTFUNCTION;
                    departmentHis.EDITSTATE = entity.EDITSTATE;
                    departmentHis.COMPANYID = entity.T_HR_COMPANY.COMPANYID;
                    departmentHis.CREATEUSERID = entity.CREATEUSERID;
                    departmentHis.CREATEDATE = entity.CREATEDATE;
                    departmentHis.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                    departmentHis.OWNERDEPARTMENTID = entity.OWNERDEPARTMENTID;
                    departmentHis.OWNERID = entity.OWNERID;
                    departmentHis.OWNERPOSTID = entity.OWNERPOSTID;
                    departmentHis.UPDATEDATE = entity.UPDATEDATE;
                    departmentHis.UPDATEUSERID = entity.UPDATEUSERID;
                    //if (entity.T_HR_DEPARTMENTDICTIONARY != null)
                    //{
                    //    departmentHis.T_HR_DEPARTMENTDICTIONARYReference.EntityKey =
                    // new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_DEPARTMENTDICTIONARY", "DEPARTMENTDICTIONARYID", entity.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID);
                    //}
                    departmentHis.REUSEDATE = DateTime.Now;
                    if (entity.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        departmentHis.CANCELDATE = DateTime.Now;
                    }
                    dhbll.DepartmentHistoryAdd(departmentHis);
                    // DataContext.AddObject("T_HR_DEPARTMENTHISTORY", departmentHis);
                    //  }
                    new CompanyBLL().EditVersion("部门");

                    
                    //WorkflowUtility.CallWorkflow("部门变更审核工作流", ent);
                }

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentUpdate:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 根据部门ID查找部门信息
        /// </summary>
        /// <param name="companyID">部门ID</param>
        /// <returns>返回部门实例</returns>
        public int GetDepartCount(string companyID)
        {
            string stateActived = Convert.ToInt32(EditStates.Actived).ToString();
            string statePendingCanceled = Convert.ToInt32(EditStates.PendingCanceled).ToString();
            var ents = from o in dal.GetObjects()
                       where o.T_HR_COMPANY.COMPANYID == companyID && (o.EDITSTATE == stateActived || o.EDITSTATE == statePendingCanceled)
                       select o;

            return ents.Count();
        }
        /// <summary>
        /// 部门下是否有岗位
        /// </summary>
        /// <param name="departID">部门ID</param>
        /// <returns>是否成功找到岗位</returns>
        public bool IsExistChilds(string departID)
        {
            //是否分配了岗位
            PostBLL tmpBll = new PostBLL();
            int count = tmpBll.GetPositionCount(departID);
            if (count > 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="id">部门ID</param>
        public void DepartmentDelete(string id, ref string strMsg)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.DEPARTMENTID == id
                               select ent);
                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    if (IsExistChilds(id))
                    {
                        //throw new Exception("此部门已关联岗位，不能删除！");
                        strMsg = "DEPARTMENTHASPOST";
                        return;
                    }
                    // dal.Delete(entity);
                    Delete(entity);
                    new CompanyBLL().EditVersion("部门");
                }
                else
                {
                    // throw new Exception("没有找到对应实体！");
                    strMsg = "NOTFOUND";


                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentDelete:" + ex.Message);

            }

        }
        /// <summary>
        /// 撤消部门
        /// </summary>
        /// <param name="sourceEntity">部门实体</param>
        public void DepartmentCancel(T_HR_DEPARTMENT sourceEntity, ref string strMsg)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.DEPARTMENTID == sourceEntity.DEPARTMENTID
                               select ent);

                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    if (IsExistChilds(sourceEntity.DEPARTMENTID))
                    {
                        // throw new Exception("此部门已关联岗位，不能撤消！");
                        strMsg = "DEPARTMENTHASPOST";
                        return;
                    }
                    entity.EDITSTATE = sourceEntity.EDITSTATE;
                    entity.CHECKSTATE = sourceEntity.CHECKSTATE;
                    entity.UPDATEUSERID = sourceEntity.UPDATEUSERID;
                    entity.UPDATEDATE = System.DateTime.Now;
                    // dal.Update(entity);
                    Update(entity);
                    #region 调用即时通讯接口
                    //SMT.Foundation.Log.Tracer.Debug("取消部门开始调用即时通讯接口:"+ entity.DEPARTMENTID);
                    //AddDepartmentInfoToIM(entity);
                    #endregion
                    new CompanyBLL().EditVersion("部门");
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
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentCancel:" + ex.Message);

            }
        }

        //public EntityObject[] GetLookupData(Dictionary<string, string> args)
        //{

        //    IQueryable<T_HR_DEPARTMENT> ents = from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
        //                                       select a;


        //    return ents.Count() > 0 ? ents.ToArray() : null;
        //}

        /// <summary>
        /// 根据部门ID获取所有上级
        /// </summary>
        /// <param name="companyID"></param>
        /// <param name="userID"></param>
        /// <returns>value=0:公司，value=1：部门</returns>
        public Dictionary<string, string> GetFatherByDepartmentID(string departmentID)
        {
            T_HR_DEPARTMENT department = new T_HR_DEPARTMENT();
            T_HR_COMPANY company = new T_HR_COMPANY();
            Dictionary<string, string> fathers = new Dictionary<string, string>();
            string fatherType = "0";
            string fatherID = "";
            bool hasFather = false;
            department = (from c in dal.GetObjects<T_HR_DEPARTMENT>()
                          where c.DEPARTMENTID == departmentID
                          select c).FirstOrDefault();

            if (department != null)
            {
                if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                {
                    fatherType = department.FATHERTYPE;
                    fatherID = department.FATHERID;
                    hasFather = true;
                }
                else
                {
                    hasFather = false;
                }
            }

            while (hasFather)
            {
                if (fatherType == "1" && !string.IsNullOrEmpty(fatherID))
                {
                    department = (from de in dal.GetObjects<T_HR_DEPARTMENT>()
                                  where de.DEPARTMENTID == fatherID
                                  select de).FirstOrDefault();
                    if (department != null)
                    {
                        fathers.Add(department.DEPARTMENTID, "1");
                        if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                        {
                            fatherID = department.FATHERID;
                            fatherType = department.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }
                }
                else if (fatherType == "0" && !string.IsNullOrEmpty(fatherID))
                {
                    company = (from com in dal.GetObjects<T_HR_COMPANY>()
                               where com.COMPANYID == fatherID
                               select com).FirstOrDefault();

                    if (company != null)
                    {
                        fathers.Add(company.COMPANYID, "0");
                        if (!string.IsNullOrEmpty(company.FATHERTYPE) && !string.IsNullOrEmpty(company.FATHERID))
                        {
                            fatherID = company.FATHERID;
                            fatherType = company.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }

                }
                else
                {
                    hasFather = false;
                }

            }
            return fathers;
        }

        public EntityObject[] GetLookupData(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_DEPARTMENT");
                SetDepartmentFilter(ref filterString, ref queryParas, userID);
            }


            IQueryable<T_HR_DEPARTMENT> ents =
                from a in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                select a;

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_DEPARTMENT>(ents, pageIndex, pageSize, ref pageCount);

            return ents.Count() > 0 ? ents.ToArray() : null;
        }
        /// <summary>
        /// 修改部门排序号
        /// </summary>
        /// <param name="depart"></param>
        /// <param name="strMsg"></param>
        public void DepartmentIndexUpdate(T_HR_DEPARTMENT depart, ref string strMsg)
        {
            try
            {
                var entitys = (from ent in dal.GetObjects()
                               where ent.DEPARTMENTID == depart.DEPARTMENTID
                               select ent);

                if (entitys.Count() > 0)
                {
                    var entity = entitys.FirstOrDefault();
                    entity.SORTINDEX = depart.SORTINDEX;
                    dal.Update(entity);
                    new CompanyBLL().EditVersion("部门");
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
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " DepartmentIndexUpdate:" + ex.Message);
                throw (ex);
            }
        }
        /// <summary>
        /// 获取指定时间后更新的部门
        /// </summary>
        /// <param name="endDate"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public IQueryable<T_HR_DEPARTMENT> GetDepartmentWithSpecifiedTime(string startDate)
        {
            DateTime start;
            bool flag;
            flag = DateTime.TryParse(startDate, out start);
            if (flag)
            {

                IQueryable<T_HR_DEPARTMENT> ents = from c in dal.GetObjects().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
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
        /// 获取部门视图
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_DEPARTMENT> GetAllDepartmentView(string userID)
        {
            #region 按权限过滤
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_DEPARTMENT", "3");
                SetDepartmentFilter(ref filterString, ref paras, userID);
            }
            #endregion
            IQueryable<T_HR_DEPARTMENT> ents = from c in dal.GetObjects()
                                               select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            var departmentViews = from c in ents
                                  select new V_DEPARTMENT
                                 {
                                     DEPARTMENTID = c.DEPARTMENTID,
                                     DEPARTMENTNAME = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                     DEPARTMENTDICTIONARYID = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID,
                                     CHECKSTATE = c.CHECKSTATE,
                                     EDITSTATE = c.EDITSTATE,
                                     DEPARTMENTBOSSHEAD = c.DEPARTMENTBOSSHEAD,
                                     FATHERID = c.FATHERID,
                                     FATHERTYPE = c.FATHERTYPE,
                                     COMPANYID = c.T_HR_COMPANY.COMPANYID,
                                     CNAME = c.T_HR_COMPANY.CNAME,
                                     SORTINDEX = c.SORTINDEX
                                 };


            return departmentViews;
        }

        /// <summary>
        /// 获取指定时间后更新的部门视图
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_DEPARTMENT> GetDepartmentViewByDateAndUser(string startDate, string userID)
        {
            #region 按权限过滤
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, "T_HR_DEPARTMENT", "3");
                SetDepartmentFilter(ref filterString, ref paras, userID);
            }
            #endregion
            IQueryable<T_HR_DEPARTMENT> ents = from c in dal.GetObjects()
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
            var departmentViews = from c in ents
                                  where c.UPDATEDATE >= start
                                  select new V_DEPARTMENT
                                  {
                                      DEPARTMENTID = c.DEPARTMENTID,
                                      DEPARTMENTNAME = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                      DEPARTMENTDICTIONARYID = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID,
                                      CHECKSTATE = c.CHECKSTATE,
                                      EDITSTATE = c.EDITSTATE,
                                      DEPARTMENTBOSSHEAD = c.DEPARTMENTBOSSHEAD,
                                      FATHERID = c.FATHERID,
                                      FATHERTYPE = c.FATHERTYPE,
                                      COMPANYID = c.T_HR_COMPANY.COMPANYID,
                                      CNAME = c.T_HR_COMPANY.CNAME,
                                      SORTINDEX = c.SORTINDEX
                                  };

            return departmentViews;
        }

        /// <summary>
        /// 根据实体权限获取部门视图
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="perm"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IQueryable<V_DEPARTMENT> GetDepartmentView(string userID, string perm, string entity)
        {
            #region 按权限过滤
            if (string.IsNullOrEmpty(perm))
            {
                perm = "3";
            }
            if (string.IsNullOrEmpty(entity))
            {
                entity = "T_HR_DEPARTMENT";
            }
            List<object> paras = new List<object>();
            string filterString = "";

            if (!string.IsNullOrEmpty(userID))
            {
                SetOrganizationFilter(ref filterString, ref paras, userID, entity, perm);
                SetDepartmentFilter(ref filterString, ref paras, userID);
            }
            #endregion
            IQueryable<T_HR_DEPARTMENT> ents = from c in dal.GetObjects()
                                               select c;
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
            }
            var departmentViews = from c in ents
                                  select new V_DEPARTMENT
                                  {
                                      DEPARTMENTID = c.DEPARTMENTID,
                                      DEPARTMENTNAME = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                                      DEPARTMENTDICTIONARYID = c.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTDICTIONARYID,
                                      CHECKSTATE = c.CHECKSTATE,
                                      EDITSTATE = c.EDITSTATE,
                                      DEPARTMENTBOSSHEAD = c.DEPARTMENTBOSSHEAD,
                                      FATHERID = c.FATHERID,
                                      FATHERTYPE = c.FATHERTYPE,
                                      COMPANYID = c.T_HR_COMPANY.COMPANYID,
                                      CNAME = c.T_HR_COMPANY.CNAME,
                                      SORTINDEX = c.SORTINDEX
                                  };


            return departmentViews;
        }

        /// <summary>
        /// 根据部门ID获取所有子部门ID
        /// </summary>
        /// <param name="companyID"></param>
        /// <returns></returns>
        public List<string> GetChildDepartmentBydepartmentID(List<string> paraDepartmentIDs)
        {
            List<string> departmentIDs = new List<string>();
            if (paraDepartmentIDs == null || paraDepartmentIDs.Count == 0)
            {
                return departmentIDs;
            }
            var ents = from c in dal.GetObjects<T_HR_DEPARTMENT>()
                       where c.CHECKSTATE == "2"
                       select c;
            if (ents.Count() <= 0)
            {
                return departmentIDs;
            }
            List<T_HR_DEPARTMENT> departmentLists = ents.ToList();
            foreach (var departmentID in paraDepartmentIDs)
            {
                if (departmentIDs.Contains(departmentID))
                {
                    continue;
                }
                var departments = departmentLists.Where(s => s.FATHERID == departmentID && s.FATHERTYPE == "1");
                if (departments.Count() <= 0)
                {
                    continue;
                }

                GetDepartmentchild(ref departmentIDs, departments.ToList(), departmentLists);
            }
            return departmentIDs;
        }

        public void GetDepartmentchild(ref List<string> departmentIDs, List<T_HR_DEPARTMENT> departments, List<T_HR_DEPARTMENT> departmentLists)
        {
            if (departments.Count == 0)
            {
                return;
            }
            foreach (var ent in departments)
            {
                if (departmentIDs.Contains(ent.DEPARTMENTID))
                {
                    continue;
                }
                departmentIDs.Add(ent.DEPARTMENTID);
                var departs = departmentLists.Where(s => s.FATHERID == ent.DEPARTMENTID && s.FATHERTYPE == "1");
                if (departs.Count() <= 0)
                {
                    continue;
                }
                GetDepartmentchild(ref departmentIDs, departments.ToList(), departmentLists);
            }

        }
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                string strMsg = string.Empty;
                var department = (from c in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY").Include("T_HR_COMPANY")
                                  where c.DEPARTMENTID == EntityKeyValue
                                  select c).FirstOrDefault();
                if (department != null)
                {
                    department.CHECKSTATE = CheckState;
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        if (department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            department.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                        }
                        else
                        {
                            department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                        }
                    }
                    if (CheckState == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        if (department.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            EntityObject departmentHis =new T_HR_DEPARTMENTHISTORY();

                            departmentHis= (from c in dal.GetObjects<T_HR_DEPARTMENTHISTORY>()
                                  where c.DEPARTMENTID == department.CREATEDEPARTMENTID
                                  orderby c.UPDATEDATE descending
                                  select c ).FirstOrDefault();



                            Utility.CloneEntity(departmentHis, department);

                            department.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                            department.CHECKSTATE = "2";
                        }
                        else
                        {
                            department.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        }
                    }
                    DepartmentUpdate(department, ref strMsg);
                    #region 添加即时通讯的接口调用
                    if (department.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        SMT.Foundation.Log.Tracer.Debug("修改部门开始调用即时通讯接口:" + department.DEPARTMENTID);
                        AddDepartmentInfoToIM(department);
                    }
                    #endregion
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



        #region 即时通讯接口调用
        private void AddDepartmentInfoToIM(T_HR_DEPARTMENT Department)
        {
            string StrMessage = "";
            SMT.Foundation.Log.Tracer.Debug("开始调用即时通讯服务AddDepartmentInfoToIM：" + StrMessage);
            try
            {
                DataSyncServiceClient Client = new DataSyncServiceClient();
                string StrType = "0";//类型 1 公司  0 部门
                string StrValid = "";//是否可用  0 不可用 1 可用 
                string StrFatherId = "";//上级部门ID
                string StrDepartmentName = "";//部门名称
                if (Department.EDITSTATE == "1")
                {
                    StrValid = "1";
                }
                else
                {
                    StrValid = "0";
                }
                if (!string.IsNullOrEmpty(Department.FATHERID))
                {
                    StrFatherId = Department.FATHERID;
                }
                else
                {
                    StrFatherId = "0";
                }
                if (Department.T_HR_DEPARTMENTDICTIONARY != null)
                {
                    StrDepartmentName = Department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                }
                SMT.Foundation.Log.Tracer.Debug("更新即时通讯接口AddOrUpdateImDepartment的结果StrValid：" + StrValid);
                StrMessage = Client.AddOrUpdateCompanyDepartment(Department.DEPARTMENTID, StrDepartmentName, StrFatherId, StrType, StrValid,"0");
                SMT.Foundation.Log.Tracer.Debug("更新即时通讯接口AddOrUpdateImDepartment的结果：" + StrMessage);
            }
            catch (Exception ex)
            {
                StrMessage = ex.ToString() + "部门ID:" + Department.DEPARTMENTID ;
                SMT.Foundation.Log.Tracer.Debug("即时通讯服务错误AddOrUpdateImDepartment：" + StrMessage);
            }
        }
        #endregion

    }
}