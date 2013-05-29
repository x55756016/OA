using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_FB_EFModel;
using System.Data.Objects.DataClasses;
using System.Reflection;
using SMT.FB.DAL;
using System.Linq.Expressions;
using System.Collections;
using System.Xml.Linq;

namespace SMT.FB.BLL
{
    public class QueryEntityBLL : BudgetAccountBLL
    {
        #region 4.	汇总查询方法
        public List<FBEntity> QueryAuditOrder(QueryExpression qe)
        {
            using (AuditBLL bll = new AuditBLL())
            {
                return bll.GetAuditedFBEntity(qe);
            }
        }

        /// <summary>
        /// 可用额度汇总
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryQueryBudgetAccount(QueryExpression qe)
        {
            // qe.VisitModuleCode = typeof(T_FB_BUDGETACCOUNT).Name;
            qe.Include = new string[] { "T_FB_SUBJECT" };

            qe.QueryType = "T_FB_BUDGETACCOUNT";
            if (qe.RightType != "QueryBudgetAccount")
            {
                qe.RightType = "QueryBudgetAccount";
            }

            List<T_FB_BUDGETACCOUNT> listBudgetAccount = GetEntities<T_FB_BUDGETACCOUNT>(qe);

            List<T_FB_BUDGETACCOUNT> listBAPerson = listBudgetAccount.FindAll(item =>
            {
                return !string.IsNullOrEmpty(item.OWNERID);
            });
            List<string> listUserIDs = listBAPerson.CreateList(item =>
            {
                return item.OWNERID;
            });
            OrganizationBLL orgbll = new OrganizationBLL();
            List<VirtualUser> listUser = orgbll.GetVirtualUser(listUserIDs);
            List<VirtualCompany> listCompany = orgbll.GetVirtualCompany(qe);
            List<VirtualDepartment> listDepartment = orgbll.GetVirtualDepartment(qe);
            List<VirtualPost> listPost = orgbll.GetVirtualPost(qe);


            listBudgetAccount.ForEach(item =>
                {
                    VirtualCompany vc = listCompany.FirstOrDefault(item2 =>
                        {
                            return item2.ID == item.OWNERCOMPANYID;
                        });
                    item.OWNERCOMPANYID = vc == null ? "" : vc.Name;

                    VirtualDepartment vd = listDepartment.FirstOrDefault(item2 =>
                        {
                            return item2.ID == item.OWNERDEPARTMENTID;
                        });
                    item.OWNERDEPARTMENTID = vd == null ? "" : vd.Name;

                    VirtualPost vp = listPost.FirstOrDefault(item2 =>
                        {
                            return item2.ID == item.OWNERPOSTID;
                        });
                    item.OWNERPOSTID = vp == null ? "" : vp.Name;

                    VirtualUser vu = listUser.FirstOrDefault(item2 =>
                    {
                        return item2.ID == item.OWNERID;
                    });
                    item.OWNERID = vu == null ? "" : vu.Name;

                });
            listBudgetAccount = listBudgetAccount.OrderByDescending(item => item.BUDGETYEAR)
                .ThenByDescending(item => item.BUDGETMONTH)
                .ThenBy(item => item.ACCOUNTOBJECTTYPE)
                .ThenBy(item => item.OWNERCOMPANYID)
                .ThenBy(item => item.OWNERDEPARTMENTID)
                .ThenBy(item => item.OWNERPOSTID)
                .ThenBy(item => item.OWNERID).ToList();
            return listBudgetAccount.ToFBEntityList();

        }
        #endregion



    }


}

