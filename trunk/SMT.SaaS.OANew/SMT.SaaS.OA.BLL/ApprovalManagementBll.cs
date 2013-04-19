using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT.SaaS.OA.DAL.Views;
using SMT_OA_EFModel;
using System.Data.Objects;
using SMT.Foundation.Log;
using System.Net;
using System.Linq.Dynamic;


namespace SMT.SaaS.OA.BLL
{
    public class ApprovalManagementBll : BaseBll<T_OA_APPROVALINFO>
    {
        
        public T_OA_APPROVALINFO Get_Apporval(string id)
        {
            var m = from master in dal.GetObjects<T_OA_APPROVALINFO>() where master.APPROVALID == id select master;
            if (m.Count() > 0)
            {
                return m.ToList()[0];
            }
            else
            {
                return null;
            }

        }
        public IQueryable<T_OA_APPROVALINFO> GetApprovalList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID, List<string> guidStringList, string checkState)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_APPROVALINFO>()
                        select ent;
                if (checkState == "4")//审批人
                {
                    if (guidStringList != null)
                    {
                        q = from ent in q
                            where guidStringList.Contains(ent.APPROVALID)
                            select ent;
                        //q = q.ToList().Where(x => guidStringList.Contains(x.APPROVALID)).AsQueryable();
                    }
                }
                else//创建人
                {
                    //q = q.Where(ent => ent.CREATEUSERID == userID);
                    if (checkState != "5")
                    {
                        q = q.Where(ent => ent.CHECKSTATE == checkState);
                    }
                }
                List<object> queryParas = new List<object>();
                if (paras != null)
                {
                    queryParas.AddRange(paras);
                }
                string bb = filterString;
                if (guidStringList == null)
                {
                    if (!(filterString.IndexOf("CREATEUSERID") > -1))
                    {
                        UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_APPROVALINFO");
                    }

                }
                //Tracer.Debug("事项审批中的数据filterstring"+filterString +"AAAAA"+System.DateTime.Now.ToString());
                if (!string.IsNullOrEmpty(filterString))
                {
                    q = q.Where(filterString, queryParas.ToArray());
                }
                q = q.OrderBy(sort);

                q = Utility.Pager<T_OA_APPROVALINFO>(q, pageIndex, pageSize, ref pageCount);

                if (q.Count() > 0)
                {
                    return q;
                }

                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("事项审批GetApprovalList" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
           
        }
        ///// <summary>
        ///// 获取文件名
        ///// </summary>
        ///// <param name="approvalID"></param>
        ///// <returns></returns>
        //public List<SMT.SaaS.BLLCommonServices.OAPersonalWS.T_OA_FILEUPLOAD> GetApprovalFile(string approvalID)
        //{
        //    var q = from ent in dal.GetObjects<SMT.SaaS.BLLCommonServices.OAPersonalWS.T_OA_FILEUPLOAD>()
        //            where ent.FORMID == approvalID
        //            select ent;
        //    return q.Count() > 0 ? q.ToList() : null;
        //}
        ///// <summary>
        ///// 删除多条附件记录
        ///// </summary>
        ///// <param name="ids">附件id</param>
        ///// <returns></returns>
        //public int DelFiles(string[] ids)
        //{
        //    try
        //    {
        //        foreach (string id in ids)
        //        {
        //            var ents = from e in dal.GetObjects<T_OA_FILEUPLOAD>()
        //                       where e.FILEUPLOADID == id
        //                       select e;
        //            var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
        //            if (ent != null)
        //                dal.DeleteFromContext(ent);
        //        }
        //        int i = dal.SaveContextChanges();
        //        return i;
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("事项审批ApprovalManagementBll-DelFiles" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //        return 0;
                
        //    }
        //}
        ///// <summary>
        ///// 删除多条附件记录
        ///// </summary>
        ///// <param name="ids">附件父ID</param>
        ///// <returns></returns>
        //public bool DelFiles_Parent(string[] ids)
        //{
        //    try
        //    {
        //        foreach (string id in ids)
        //        {
        //            var ents = from e in dal.GetObjects<T_OA_FILEUPLOAD>()
        //                       where e.FORMID == id
        //                       select e;
        //            var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
        //            if (ent != null)
        //                dal.DeleteFromContext(ent);
        //        }
        //        int i = dal.SaveContextChanges();
        //        return i > 0 ? true : false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("事项审批ApprovalManagementBll-DelFiles_Parent" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //        return false;
                
        //    }
        //}
        public bool AddApproval(T_OA_APPROVALINFO ApprovalInfo,ref string ApprovalCode)
        {
            try
            {
                string appCodeList = GetMaxApprovalCode(ApprovalInfo.APPROVALCODE);
                if (appCodeList == null)
                {
                    ApprovalInfo.APPROVALCODE = ApprovalInfo.APPROVALCODE + "0001";
                }
                else
                {
                    ApprovalInfo.APPROVALCODE = ApprovalInfo.APPROVALCODE.Substring(0, 11) + (Convert.ToInt32(appCodeList.Substring(11)) + 1).ToString().PadLeft(4,'0');
                    
                }
                bool i = Add(ApprovalInfo);
                if (i == true)
                {
                    ApprovalCode = ApprovalInfo.APPROVALCODE;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("事项审批ApprovalManagementBll-AddApproval" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
            }
        }

        ///// <summary>
        /////   添加报批数据 与 上传文件的关联
        ///// </summary>
        ///// <param name="ApprovalInfo"></param>
        ///// <param name="fileInfo"></param>
        ///// <returns></returns>
        //public string AddApprovalUploadFile(T_OA_APPROVALINFO ApprovalInfo, List<T_OA_FILEUPLOAD> lst)
        //{
        //    try
        //    {
        //        string StrReturn = "";
        //        string appCodeList = GetMaxApprovalCode(ApprovalInfo.APPROVALCODE);
        //        if (appCodeList == null)
        //            ApprovalInfo.APPROVALCODE = ApprovalInfo.APPROVALCODE + "0001";
        //        else
        //            ApprovalInfo.APPROVALCODE = ApprovalInfo.APPROVALCODE.Substring(0, 10) + (Convert.ToInt32(appCodeList.Substring(11)) + 1).ToString();

        //        bool i = Add(ApprovalInfo);
        //        if (i == true)
        //        {
        //            if (lst != null && lst.Count > 0)
        //            {
        //                foreach (T_OA_FILEUPLOAD info in lst)
        //                {
        //                    info.FORMID = ApprovalInfo.APPROVALID;
        //                    //dal.Add(info);
        //                    dal.AddToContext(info);
        //                }
        //                int k = dal.SaveContextChanges();
        //                StrReturn = k > 0 ? "" : "uploadfile error";
        //            }
        //        }
        //        else
        //            StrReturn = "approval error";
        //        return StrReturn;
        //    }
        //    catch (Exception ex)
        //    {
        //        Tracer.Debug("事项审批ApprovalManagementBll-AddApprovalUploadFile" + System.DateTime.Now.ToString() + " " + ex.ToString());
        //        return ex.Message;
                
        //    }
        //}

        public bool DeleteApproval(T_OA_APPROVALINFO ApprovalInfo)
        {
            try
            {
                
                var entitys = (from ent in dal.GetTable()
                               where ent.APPROVALID == ApprovalInfo.APPROVALID
                               select ent);
                if (entitys.Count() > 0)
                {
                    
                    var entity = entitys.FirstOrDefault();
                    dal.BeginTransaction();
                    if (base.Delete(entity))
                    {
                        PublicInterfaceWS.PublicServiceClient publicClient = new PublicInterfaceWS.PublicServiceClient();
                        if (!publicClient.DeleteContent(ApprovalInfo.APPROVALID))
                        {
                            dal.RollbackTransaction();
                            return false;
                        }
                        dal.CommitTransaction();
                        return true;
                    }
                    else
                    {
                        dal.RollbackTransaction();
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                dal.RollbackTransaction();
                Tracer.Debug("事项审批ApprovalManagementBll-DeleteApproval" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                
            }
        }
        public int UpdateApproval(T_OA_APPROVALINFO ApprovalInfo)
        {
            int nRet = -1;
            try
            {
                
                var users = from ent in dal.GetObjects<T_OA_APPROVALINFO>()
                            where ent.APPROVALID == ApprovalInfo.APPROVALID
                            select ent;
                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    if (ApprovalInfo.CHECKSTATE == "0")
                    {
                        user.CONTENT = ApprovalInfo.CONTENT;
                        user.ISCHARGE = ApprovalInfo.ISCHARGE;
                        user.TEL = ApprovalInfo.TEL;
                        user.CHARGEMONEY = ApprovalInfo.CHARGEMONEY;
                        user.APPROVALTITLE = ApprovalInfo.APPROVALTITLE;
                        user.TYPEAPPROVAL = ApprovalInfo.TYPEAPPROVAL;
                        user.TYPEAPPROVALONE = ApprovalInfo.TYPEAPPROVALONE;
                        user.TYPEAPPROVALTWO = ApprovalInfo.TYPEAPPROVALTWO;
                        user.TYPEAPPROVALTHREE = ApprovalInfo.TYPEAPPROVALTHREE;
                    }
                    
                    user.UPDATEUSERID = ApprovalInfo.UPDATEUSERID;                    
                    user.CHECKSTATE = ApprovalInfo.CHECKSTATE;                    
                    user.UPDATEDATE = DateTime.Now;
                    nRet = Update(user);
                }
                nRet = 0;
            }
            catch (Exception ex)
            {
                Tracer.Debug("事项审批ApprovalManagementBll-UpdateApproval" + System.DateTime.Now.ToString() + " " + ex.ToString());
                nRet = -1;
                
            }
            return nRet;
        }

        private string GetMaxApprovalCode(string appCodeHeader)
        {
            //将传过来的编号重新设置，以服务器端为依据
            string StrYear = System.DateTime.Now.Year.ToString();
            string StrMonth = System.DateTime.Now.Month.ToString().PadLeft(2,'0');
            
            string StrDay = System.DateTime.Now.Day.ToString().PadLeft(2,'0');
            
            appCodeHeader = "BPJ" + StrYear + StrMonth + StrDay;
            
            var q = from ent in dal.GetObjects<T_OA_APPROVALINFO>()
                    where ent.APPROVALCODE.Substring(0, 11) == appCodeHeader
                    select ent.APPROVALCODE;
            if (q.Count() > 0)
            {                
                return q.Max();
            }
            return null;
        }
        /// <summary>
        /// 添加事项审批类型的设置
        /// </summary>
        /// <param name="ListSet"></param>
        /// <param name="lstcompany"></param>
        /// <param name="lstdepartment"></param>
        /// <returns></returns>
        public string AddApprovalSet(List<T_OA_APPROVALTYPESET> ListSet,List<string> lstcompany,List<string> lstdepartment)
        {
            string StrReturn = "";
            int k = 0;
            try
            {
                //公司
                var companyents = from ent in dal.GetObjects<T_OA_APPROVALTYPESET>()
                                  where ent.ORGANIZATIONTYPE == "0" && lstcompany.Contains(ent.ORGANIZATIONID)
                                  select ent;
                var departmentents = from ent in dal.GetObjects<T_OA_APPROVALTYPESET>()
                                     where ent.ORGANIZATIONTYPE == "1" && lstdepartment.Contains(ent.ORGANIZATIONID)
                                     select ent;
                dal.BeginTransaction();
                if (companyents.Count() > 0)
                {
                    companyents.ToList().ForEach(
                        item =>
                        {
                            dal.DeleteFromContext(item);
                        }
                        );
                }
                if (departmentents.Count() > 0)
                {
                    departmentents.ToList().ForEach(item => {
                        dal.DeleteFromContext(item);
                    });
                }
                if (departmentents.Count() > 0 || companyents.Count() > 0)
                {
                     k = dal.SaveContextChanges();
                }

                if (ListSet.Count > 0)
                {
                    ListSet.ForEach(
                        item =>
                            {
                                item.CREATEDATE = System.DateTime.Now;
                                //事项审批类型不为空 怎添加
                                if (!string.IsNullOrEmpty(item.TYPEAPPROVAL))
                                    dal.AddToContext(item);
                            }
                        );
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        dal.CommitTransaction();
                    }
                    else
                    {
                        dal.RollbackTransaction();
                        StrReturn = "ERROR";
                    }
                }
                else
                {
                    if (k > 0)
                    {
                        dal.CommitTransaction();
                    }
                    else
                    {
                        dal.RollbackTransaction();
                        StrReturn = "ERROR";
                    } 
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("事项审批ApprovalManagementBll-AddApprovalSet" + System.DateTime.Now.ToString() + " " + ex.ToString());
                StrReturn = "ERROR";
                dal.RollbackTransaction();
            }
            return StrReturn;
        }

        /// <summary>
        /// 返回组织架构的事项审批集合
        /// </summary>
        /// <param name="orgid"></param>
        /// <param name="orgtype"></param>
        /// <returns></returns>
        public List<T_OA_APPROVALTYPESET> GetApprovalSetByOrgType(string orgid, string orgtype)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_OA_APPROVALTYPESET>()
                           where ent.ORGANIZATIONID == orgid && ent.ORGANIZATIONTYPE == orgtype
                           select ent;
                return ents.Count() > 0 ? ents.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("事项审批ApprovalManagementBll-GetApprovalSetByOrgType" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }

        public List<T_OA_APPROVALTYPESET> GetApprovalByCompanyandDepartmentids(List<string> companyids, List<string> departmentids)
        {
            try
            {
                var ents = from ent in dal.GetObjects<T_OA_APPROVALTYPESET>()
                           where (companyids.Contains(ent.ORGANIZATIONID) && ent.ORGANIZATIONTYPE == "0") || (departmentids.Contains(ent.ORGANIZATIONID) && ent.ORGANIZATIONTYPE=="1")
                           select ent;
                return ents.Count() > 0 ? ents.ToList() : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("事项审批ApprovalManagementBll-GetApprovalByCompanyandDepartmentids" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 根据公司ID 和部门ID获取  事项审批的类型
        /// </summary>
        /// <param name="companyid"></param>
        /// <param name="departmentid"></param>
        /// <returns></returns>
        public List<string> GetApprovalTypeByCompanyandDepartmentid(string companyid, string  departmentid)
        {
            try
            {
                //公司设置
                var ents = from ent in dal.GetObjects<T_OA_APPROVALTYPESET>()
                           where ent.ORGANIZATIONID == companyid && ent.ORGANIZATIONTYPE == "0"
                           select ent;
                //部门设置
                var entdepartments = from ent in dal.GetObjects<T_OA_APPROVALTYPESET>()
                                     where ent.ORGANIZATIONID == departmentid && ent.ORGANIZATIONTYPE == "1"
                                     select ent;
                List<string> ApprovalTypes = new List<string>();
                if (entdepartments.Count() == 0)
                {
                    if (ents.Count() > 0)
                    {
                        ents.ToList().ForEach(
                            item =>
                            {
                                ApprovalTypes.Add(item.TYPEAPPROVAL);
                            }
                            );
                    }
                }
                else
                {
                    entdepartments.ToList().ForEach(
                            item =>
                            {
                                ApprovalTypes.Add(item.TYPEAPPROVAL);
                            }
                            );
                }
                return ApprovalTypes.Count() > 0 ? ApprovalTypes : null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("事项审批ApprovalManagementBll-GetApprovalTypeByCompanyandDepartmentid" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 通过公司名称获取公司下的部门 -by luojie
        /// </summary>
        /// <param name="strCompanies">字符串形式的公司名称，以‘，’作为间隔</param>
        /// <returns>V_DepartmentWinthCompany列表,有部门id，公司id，公司名字及其简称</returns>
        public List<V_DepartmentWithCompany> GetApprovalTypesByCompanyIDs(string strCompanies)
        {
            List<V_DepartmentWithCompany> ApprovalTypeList = new List<V_DepartmentWithCompany>();
            try
            {
                if (!string.IsNullOrEmpty(strCompanies))
                {
                    //获取附带公司信息的部门
                    SMT.SaaS.BLLCommonServices.OrganizationWS.OrganizationServiceClient oscClient = new BLLCommonServices.OrganizationWS.OrganizationServiceClient();
                    var departments = oscClient.GetDepartmentByCompanyIDs(strCompanies);

                    if (departments != null)
                    {
                        //查询所有的事项审批类型
                        var ApprovalTypeSet = from at in dal.GetObjects<T_OA_APPROVALTYPESET>()
                                              select at;

                        //获取公司的事项审批类型
                        var CompanyList = from cl in departments
                                          group cl by new { cl.COMPANYID,cl.COMPANYNAME,cl.BRIEFNAME } into clg
                                          select new { clg.Key,clg };

                        foreach (var cmp in CompanyList)
                        {
                            var approvalType = (from at in ApprovalTypeSet
                                               where at.ORGANIZATIONID == cmp.Key.COMPANYID && at.ORGANIZATIONTYPE == "0"
                                               select at).ToList();
                            if (approvalType != null)
                            {
                                foreach(var at in approvalType)
                                {
                                V_DepartmentWithCompany temp = new V_DepartmentWithCompany();
                                temp.COMPANYID = cmp.Key.COMPANYID;
                                temp.COMPANYNAME = cmp.Key.COMPANYNAME;
                                temp.BRIEFNAME = cmp.Key.BRIEFNAME;
                                temp.DEPARTMENTID = null;
                                temp.APPROVALTYPEVALUE = at.TYPEAPPROVAL;
                                ApprovalTypeList.Add(temp);
                                }
                            }
                        }

                        //获取部门的事项审批类型
                        foreach (var dep in departments)
                        {
                            var approvalType = (from at in ApprovalTypeSet
                                               where at.ORGANIZATIONID == dep.DEPARTMENTID && at.ORGANIZATIONTYPE == "1"
                                               select at).ToList();
                            if (approvalType != null)
                            {
                                foreach (var at in approvalType)
                                {
                                    V_DepartmentWithCompany temp = new V_DepartmentWithCompany();
                                    temp.COMPANYID = dep.COMPANYID;
                                    temp.COMPANYNAME = dep.COMPANYNAME;
                                    temp.BRIEFNAME = dep.BRIEFNAME;
                                    temp.DEPARTMENTID = dep.DEPARTMENTID;
                                    if (approvalType != null)
                                    {
                                        temp.APPROVALTYPEVALUE = at.TYPEAPPROVAL;
                                        ApprovalTypeList.Add(temp);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + "-ApprovalManagementBll-GetApprovalTypesByCompanyIDs:" + ex.ToString());
            }
            return ApprovalTypeList == null ? null : ApprovalTypeList;
        }
 
    }
}