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
    public class ApprovalTempletManagementBll : BaseBll<T_OA_APPROVALINFOTEMPLET>
    {

        public T_OA_APPROVALINFOTEMPLET Get_ApporvalTemplet(string id)
        {
            var m = from master in dal.GetObjects<T_OA_APPROVALINFOTEMPLET>() 
                    where master.APPROVALID == id select master;
            if (m.Count() > 0)
            {
                return m.ToList()[0];
            }
            else
            {
                return null;
            }

        }

        public T_OA_APPROVALINFOTEMPLET Get_ApporvalTempletByApporvalType(string id)
        {
            var m = from master in dal.GetObjects<T_OA_APPROVALINFOTEMPLET>()
                    where master.TYPEAPPROVAL == id
                    select master;
            if (m.Count() > 0)
            {
                return m.ToList()[0];
            }
            else
            {
                return null;
            }

        }

        public IQueryable<T_OA_APPROVALINFOTEMPLET> GetApprovalTempletList(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID, List<string> guidStringList, string checkState)
        {
            try
            {
                var q = from ent in dal.GetObjects<T_OA_APPROVALINFOTEMPLET>()
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
                        UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_APPROVALINFOTEMPLET");
                    }

                }
                //Tracer.Debug("事项审批中的数据filterstring"+filterString +"AAAAA"+System.DateTime.Now.ToString());
                if (!string.IsNullOrEmpty(filterString))
                {
                    q = q.Where(filterString, queryParas.ToArray());
                }
                q = q.OrderBy(sort);

                q = Utility.Pager<T_OA_APPROVALINFOTEMPLET>(q, pageIndex, pageSize, ref pageCount);

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
        public bool AddApprovalTemplet(T_OA_APPROVALINFOTEMPLET ApprovalInfo,ref string ApprovalCode)
        {
            var m = from master in dal.GetObjects<T_OA_APPROVALINFOTEMPLET>()
                    where master.TYPEAPPROVAL == ApprovalInfo.TYPEAPPROVAL
                    select master;
            if (m.Count() > 0)
            {
                ApprovalCode = "此类型的事项审批模板已存在，请勿重复添加";
                return false;
            }
            else
            {
                bool i = Add(ApprovalInfo);
                if (i == true)
                {
                    return true;
                }
                else
                {
                    ApprovalCode = "添加事项审批模板失败，请联系管理员";
                    Tracer.Debug(ApprovalCode);
                    return false;
                }
            }
        }

        public bool DeleteApprovalTemplet(T_OA_APPROVALINFOTEMPLET ApprovalInfo)
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

        public int UpdateApprovalTemplet(T_OA_APPROVALINFOTEMPLET ApprovalInfo)
        {
            int nRet = -1;
            try
            {
                
                var users = from ent in dal.GetObjects<T_OA_APPROVALINFOTEMPLET>()
                            where ent.APPROVALID == ApprovalInfo.APPROVALID
                            select ent;
                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    if (ApprovalInfo.CHECKSTATE == "0")
                    {
                        user.CONTENT = ApprovalInfo.CONTENT;
                        //user.ISCHARGE = ApprovalInfo.ISCHARGE;
                        //user.TEL = ApprovalInfo.TEL;
                        //user.CHARGEMONEY = ApprovalInfo.CHARGEMONEY;
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

 
    }
}