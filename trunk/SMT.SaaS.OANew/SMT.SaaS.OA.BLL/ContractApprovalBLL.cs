/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-01-23

** 修改人：刘锦

** 修改时间：2010-07-23

** 描述：

**    主要用于合同申请的业务逻辑处理

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.SaaS.OA.DAL;
using SMT_OA_EFModel;

using SMT.SaaS.OA.DAL.Views;
using System.Linq.Dynamic;
using SMT.SaaS.BLLCommonServices.PersonnelWS;
using SMT.Foundation.Log;

namespace SMT.SaaS.OA.BLL
{
    public class ContractApprovalBLL : BaseBll<T_OA_CONTRACTAPP>
    {
        #region 获取所有的申请信息
        /// <summary>
        /// 获取所有的申请信息
        /// </summary>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_CONTRACTAPP> GetContractApplications()
        {
            ApplicationsForContracts afcs = new ApplicationsForContracts();
            var entity = from p in afcs.GetTable()
                         orderby p.CREATEDATE descending
                         select p;
            return entity.Count() > 0 ? entity : null;
        }
        #endregion

        #region 根据申请ID获取申请信息
        /// <summary>
        /// 根据申请ID获取申请信息
        /// </summary>
        /// <param name="contractApprovalID">申请ID</param>
        /// <returns>返回结果</returns>
        public IQueryable<T_OA_CONTRACTAPP> GetApprovalById(string contractApprovalID)
        {
            var ents = from a in dal.GetObjects<T_OA_CONTRACTAPP>()
                       where a.CONTRACTAPPID == contractApprovalID
                       select a;
            if (ents.Count() > 0)
            {
                return ents;
            }
            return null;
        }
        public V_ContractApplications GetContractApprovalById(string contractApprovalID)
        {
            var ents = (from a in dal.GetObjects<T_OA_CONTRACTAPP>()
                        join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on a.CONTRACTTYPEID equals b.CONTRACTTYPEID
                        where a.CONTRACTAPPID == contractApprovalID
                        select new V_ContractApplications { contractApp = a, contractType = b.CONTRACTTYPE });
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        #endregion

        #region 根据打印ID查询合同打印
        public V_ContractPrint GetContractPrintingById(string contractPrintinglID)
        {
            var ents = from a in dal.GetObjects<T_OA_CONTRACTPRINT>().Include("T_OA_CONTRACTAPP")
                       join c in dal.GetObjects<T_OA_CONTRACTAPP>() on a.T_OA_CONTRACTAPP.CONTRACTAPPID equals c.CONTRACTAPPID
                       join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on c.CONTRACTTYPEID equals b.CONTRACTTYPEID
                       select new V_ContractPrint { contractPrint = a, contractApp = new V_ContractApplications { contractApp = c, contractType = b.CONTRACTTYPE } };
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }
        #endregion

        #region 添加合同申请
        /// <summary>
        /// 新增合同申请
        /// </summary>
        /// <param name="ContractApproval">申请名称</param>
        /// <returns></returns>
        public bool ContractApprovalAdd(T_OA_CONTRACTAPP ContractApproval)
        {
            try
            {
                ContractApproval.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                bool contract = Add(ContractApproval);
                if (contract == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-ContractApprovalAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 添加合同打印
        /// <summary>
        /// 添加合同打印
        /// </summary>
        /// <param name="ContractPrintingInfo">打印实体</param>
        /// <returns></returns>
        public bool ContractPrintingAdd(T_OA_CONTRACTPRINT ContractPrintingInfo)
        {
            try
            {
                T_OA_CONTRACTAPP Printing = (from a in dal.GetObjects<T_OA_CONTRACTAPP>()
                                             where a.CONTRACTAPPID == ContractPrintingInfo.T_OA_CONTRACTAPP.CONTRACTAPPID
                                             select a).FirstOrDefault();

                ContractPrintingInfo.T_OA_CONTRACTAPP = Printing;
                ContractPrintingInfo.CREATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());

                int printAdd = dal.Add(ContractPrintingInfo);
                if (printAdd > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同打印ContractApprovalBLL-ContractPrintingAdd" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 上传附件
        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="contraTemplate">打印上传附件实体</param>
        /// <returns></returns>
        public bool UpdateContractPrinting(T_OA_CONTRACTPRINT contractPrinting)
        {
            bool result = false;
            try
            {
                var users = from ent in dal.GetObjects<T_OA_CONTRACTPRINT>()
                            where ent.CONTRACTPRINTID == contractPrinting.CONTRACTPRINTID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    user.SIGNDATE = contractPrinting.SIGNDATE;//签订时间
                    user.ISUPLOAD = contractPrinting.ISUPLOAD;//是否已上传
                    user.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());//修改时间
                    user.UPDATEUSERID = contractPrinting.UPDATEUSERID;//修改人ID
                    user.UPDATEUSERNAME = contractPrinting.UPDATEUSERNAME;//修改人姓名

                    int i = dal.Update(user);

                    if (i == 1)
                    {
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-UpdateContractPrinting" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 查询合同打印记录
        /// <summary>
        /// 查询合同打印记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public List<V_ContractPrint> InquiryContractPrintingRecord(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);

                var ents = from a in dal.GetObjects<T_OA_CONTRACTPRINT>().Include("T_OA_CONTRACTAPP")
                           join c in dal.GetObjects<T_OA_CONTRACTAPP>() on a.T_OA_CONTRACTAPP.CONTRACTAPPID equals c.CONTRACTAPPID
                           join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on c.CONTRACTTYPEID equals b.CONTRACTTYPEID
                           select new V_ContractPrint { contractPrint = a, contractApp = new V_ContractApplications { contractApp = c, contractType = b.CONTRACTTYPE } };

                if (ents.Count() > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_ContractPrint>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-InquiryContractPrintingRecord" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }
        #endregion

        #region 查询合同已打印上传的记录
        /// <summary>
        /// 查询合同已打印上传的记录
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="flowInfoList"></param>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public List<V_ContractPrint> GetInquiryContractPrintingRecordInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount)
        {
            try
            {
                var ents = from a in dal.GetObjects<T_OA_CONTRACTPRINT>().Include("T_OA_CONTRACTAPP")
                           join c in dal.GetObjects<T_OA_CONTRACTAPP>() on a.T_OA_CONTRACTAPP.CONTRACTAPPID equals c.CONTRACTAPPID
                           join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on a.T_OA_CONTRACTAPP.CONTRACTTYPEID equals b.CONTRACTTYPEID
                           where a.NUM > 0 && a.ISUPLOAD == "1"
                           select new V_ContractPrint { contractPrint = a, contractApp = new V_ContractApplications { contractApp = c, contractType = b.CONTRACTTYPE } };

                if (ents.ToList().Count > 0)
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, paras.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_ContractPrint>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-GetInquiryContractPrintingRecordInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
                throw (ex);
            }
        }
        #endregion

        #region 修改申请信息
        /// <summary>
        /// 修改申请信息
        /// </summary>
        /// <param name="contraApproval">申请名称</param>
        /// <returns></returns>
        public bool UpdateContraApproval(T_OA_CONTRACTAPP contraApproval)
        {
            bool result = false;
            try
            {
                ApplicationsForContracts afcs = new ApplicationsForContracts();
                var users = from ent in dal.GetObjects<T_OA_CONTRACTAPP>()
                            where ent.CONTRACTAPPID == contraApproval.CONTRACTAPPID
                            select ent;

                if (users.Count() > 0)
                {
                    var user = users.FirstOrDefault();
                    contraApproval.UPDATEDATE = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    user.CONTRACTFLAG = contraApproval.CONTRACTFLAG;//合同标志
                    user.CONTRACTTITLE = contraApproval.CONTRACTTITLE;//标题
                    user.CHECKSTATE = contraApproval.CHECKSTATE;//审批状态
                    user.CONTENT = contraApproval.CONTENT;//正文
                    user.CONTRACTTYPEID = contraApproval.CONTRACTTYPEID;//类型ID
                    user.CONTRACTLEVEL = contraApproval.CONTRACTLEVEL;//级别
                    user.CONTRACTCODE = contraApproval.CONTRACTCODE;//合同编号
                    user.UPDATEUSERID = contraApproval.UPDATEUSERID;//修改人ID
                    user.UPDATEUSERNAME = contraApproval.UPDATEUSERNAME;//修改人姓名
                    user.PARTYA = contraApproval.PARTYA;//甲方
                    user.PARTYB = contraApproval.PARTYB;//乙方
                    user.STARTDATE = contraApproval.STARTDATE;//开始时间
                    user.ENDDATE = contraApproval.ENDDATE;//结束时间
                    user.EXPIRATIONREMINDER = contraApproval.EXPIRATIONREMINDER;//到期提醒天数
                    int i = Update(user);

                    if (i > 0)
                    {
                        if (contraApproval.CHECKSTATE == "2")
                        {
                            List<object> objArds = new List<object>();
                            DateTime Days = Convert.ToDateTime(contraApproval.ENDDATE).AddDays(-Convert.ToInt32(contraApproval.EXPIRATIONREMINDER));
                            string NewsLinks = " <AssemblyName>SMT.SaaS.OA.UI</AssemblyName>" + "<PublicClass>SMT.SaaS.OA.UI.Utility</PublicClass>" +
                                               "<ProcessName>CreateFormFromEngine</ProcessName>" +
                                               "<PageParameter>SMT.SaaS.OA.UI.Views.ContractManagement.ApplicationsForContractsPages</PageParameter>" +
                                               "<ApplicationOrder>" + contraApproval.CONTRACTAPPID + "</ApplicationOrder>" +
                                               "<FormTypes>Audit</FormTypes>  ";
                            objArds.Add(contraApproval.OWNERCOMPANYID);
                            objArds.Add("OA");
                            objArds.Add("Contrat");
                            objArds.Add(contraApproval.CONTRACTAPPID);
                            objArds.Add(Days.ToString("yyyy/MM/d"));
                            objArds.Add("09:00");
                            objArds.Add("Day");
                            objArds.Add(contraApproval.PARTYA);
                            objArds.Add(contraApproval.PARTYB + "的合同于" + Convert.ToDateTime(contraApproval.ENDDATE).ToString("yyyy-MM-dd") + ",到期");
                            objArds.Add(NewsLinks);
                            objArds.Add("");
                            objArds.Add("");
                            objArds.Add("");
                            objArds.Add("Г");
                            objArds.Add("");

                            Utility.SendEngineEventTriggerData(objArds);
                        }
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-UpdateContraApproval" + System.DateTime.Now.ToString() + " " + ex.ToString());
                throw (ex);
            }
        }
        #endregion

        #region 删除申请信息
        /// <summary>
        /// 删除申请信息
        /// </summary>
        /// <param name="contraApprovalID">合同申请ID</param>
        /// <returns></returns>
        public bool DeleteContraApproval(string[] contraApprovalID)
        {
            try
            {
                ApplicationsForContracts afcs = new ApplicationsForContracts();
                var entitys = from ent in afcs.GetTable().ToList()
                              where contraApprovalID.Contains(ent.CONTRACTAPPID)
                              select ent;

                if (entitys.Count() > 0)
                {
                    foreach (var obj in entitys)
                    {
                        Delete(obj);
                    }
                    int i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-DeleteContraApproval" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return false;
                throw (ex);
            }
        }
        #endregion

        #region 检查是否存在该合同申请
        /// <summary>
        /// 检查是否存在该申请合同
        /// </summary>
        /// <param name="contractNumber">合同编号</param>
        /// <param name="contractTitle">合同标题</param>
        /// <param name="contractType">合同类型</param>
        /// <param name="contractLelve">合同级别</param>
        /// <returns></returns>
        public bool IsExistContractApproval(string contractNumber, string contractTitle, string contractType, string contractLelve)
        {
            ApplicationsForContracts afcs = new ApplicationsForContracts();
            bool IsExist = false;
            var q = from cnt in afcs.GetTable()
                    where cnt.CONTRACTCODE == contractNumber && cnt.CONTRACTTITLE == contractTitle &&
                    cnt.CONTRACTTYPEID == contractType && cnt.CONTRACTLEVEL == contractLelve
                    orderby cnt.CREATEUSERID
                    select cnt;
            if (q.Count() > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }
        #endregion

        #region

        public List<string> GetApprovalRoomNameInfos()
        {
            ApplicationsForContracts afcs = new ApplicationsForContracts();
            var query = from p in afcs.GetTable()
                        orderby p.CREATEDATE descending
                        select p.CONTRACTTITLE;

            return query.ToList<string>();
        }

        #endregion

        #region 获取申请信息

        /// <summary>
        /// 获取申请信息
        /// </summary>
        /// <returns></returns>
        public List<T_OA_CONTRACTAPP> GetApprovalRooms()
        {
            ApplicationsForContracts afcs = new ApplicationsForContracts();
            var query = from p in afcs.GetTable()
                        orderby p.CREATEDATE descending
                        select p;
            return query.ToList<T_OA_CONTRACTAPP>();

        }

        #endregion

        #region 根据条件查询合同的申请信息
        /// <summary>
        /// 根据条件查询合同的申请信息
        /// </summary>
        /// <param name="ContractApprovalName">标题</param>
        /// <param name="strID">合同ID</param>
        /// <param name="strContractLevel">级别</param>
        /// <param name="strContractLogo">申请标志</param>
        /// <returns></returns>
        public List<V_ContractApplications> GetApprovalRoomInfosListBySearch(string ContractApprovalName, string strID, string strContractLevel, string strContractLogo)
        {
            try
            {
                ApplicationsForContracts afcs = new ApplicationsForContracts();
                var q = from p in afcs.GetTable().ToList()
                        select new V_ContractApplications { contractApp = p };

                if (!string.IsNullOrEmpty(ContractApprovalName))
                {
                    q = q.Where(s => ContractApprovalName.Contains(s.contractApp.CONTRACTTITLE));
                }
                if (!string.IsNullOrEmpty(strID))
                {
                    q = q.Where(s => strID.Contains(s.contractApp.CONTRACTCODE));
                }
                if (!string.IsNullOrEmpty(strContractLevel))
                {
                    q = q.Where(s => strContractLevel.Contains(s.contractApp.CONTRACTLEVEL));
                }
                if (!string.IsNullOrEmpty(strContractLogo))
                {
                    q = q.Where(s => strContractLogo.Contains(s.contractApp.CONTRACTFLAG));
                }
                if (q.Count() > 0)
                {
                    return q.ToList<V_ContractApplications>();
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region 获取用户申请记录
        /// <summary>
        /// 获取用户申请记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<V_ContractApplications> GetApprovalInfo(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState, string userID)
        {
            try
            {
                var ents = (from a in dal.GetObjects<T_OA_CONTRACTAPP>()
                            join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on a.CONTRACTTYPEID equals b.CONTRACTTYPEID
                            select new V_ContractApplications
                            {
                                contractApp = a,
                                contractType = b.CONTRACTTYPE,
                                OWNERCOMPANYID = a.OWNERCOMPANYID,
                                OWNERID = a.OWNERID,
                                OWNERPOSTID = a.OWNERPOSTID,
                                OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                CREATEUSERID = a.CREATEUSERID
                            });
                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.contractApp.CONTRACTAPPID equals l.FormID
                                select new V_ContractApplications
                                {
                                    contractApp = a.contractApp,
                                    contractType = a.contractType,
                                    OWNERCOMPANYID = a.OWNERCOMPANYID,
                                    OWNERID = a.OWNERID,
                                    OWNERPOSTID = a.OWNERPOSTID,
                                    OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                                    CREATEUSERID = a.CREATEUSERID
                                });
                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.contractApp.CHECKSTATE);
                    }
                    List<object> queryParas = new List<object>();
                    queryParas.AddRange(paras);
                    UtilityClass.SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_OA_CONTRACTAPP");
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, queryParas.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_ContractApplications>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-GetApprovalInfo" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 获取用户申请记录(合同打印)
        /// <summary>
        /// 获取用户申请记录
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<V_ContractApplications> GetApprovalInfoPrinting(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, List<V_FlowAPP> flowInfoList, string checkState)
        {

            try
            {
                var ents = (from a in dal.GetObjects<T_OA_CONTRACTAPP>()
                            join b in dal.GetObjects<T_OA_CONTRACTTYPE>() on a.CONTRACTTYPEID equals b.CONTRACTTYPEID
                            select new V_ContractApplications
                            {
                                contractApp = a,
                                contractType = b.CONTRACTTYPE
                            });
                if (ents.Count() > 0)
                {
                    if (flowInfoList != null)
                    {
                        ents = (from a in ents.ToList().AsQueryable()
                                join l in flowInfoList on a.contractApp.CONTRACTAPPID equals l.FormID
                                select new V_ContractApplications
                                {
                                    contractApp = a.contractApp,
                                    contractType = a.contractType
                                });
                    }
                    if (!string.IsNullOrEmpty(checkState))
                    {
                        ents = ents.Where(s => checkState == s.contractApp.CHECKSTATE);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        ents = ents.ToList().AsQueryable().Where(filterString, paras.ToArray());
                    }
                    ents = ents.OrderBy(sort);
                    ents = Utility.Pager<V_ContractApplications>(ents, pageIndex, pageSize, ref pageCount);
                    return ents.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Tracer.Debug("合同申请ContractApprovalBLL-GetApprovalInfoPrinting" + System.DateTime.Now.ToString() + " " + ex.ToString());
                return null;
            }
        }
        #endregion

        #region 合同是否能被查看
        /// <summary>
        /// 查询合同是否能被查看
        /// </summary>
        /// <param name="archivesID"></param>
        /// <returns></returns>
        public bool IsContractCanBrowser(string archivesID)
        {
            var entity = from q in dal.GetObjects<T_OA_CONTRACTAPP>()
                         where q.CONTRACTAPPID == archivesID && q.ENDDATE != q.STARTDATE
                         && q.CHECKSTATE == "1"
                         select q;
            if (entity.Count() > 0)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
