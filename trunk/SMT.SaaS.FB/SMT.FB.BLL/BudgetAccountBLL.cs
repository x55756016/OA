using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_FB_EFModel;
using System.Data.Objects.DataClasses;
using SMT.FB.DAL;
using System.Data;
using System.Reflection;
using System.Linq.Expressions;
using PersonnelWS = SMT.SaaS.BLLCommonServices.PersonnelWS;
using OrganizationWS = SMT.SaaS.BLLCommonServices.OrganizationWS;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using FlowWFService = SMT.SaaS.BLLCommonServices.FlowWFService;

using System.Data.Objects;
using System.Xml.Linq;
using SMT.Foundation.Log;
using System.Configuration;

namespace SMT.FB.BLL
{
    public class BudgetAccountBBLException : FBBLLException
    {
        public BudgetAccountBBLException(string message)
            : base(message)
        {
        }
        public T_FB_BUDGETACCOUNT AccountItem { get; set; }

        public AccountErrorCode ErrorCode { get; set; }
    }
    public enum AccountErrorCode
    {
        None,
        OverMonthMoney,
        OverYearMoney,
        OverBorrowedMoney,
        OverLimitedMonthMoney,
        OverLimintedPersonMoney,
        OverMonthTransferMoney,

    }
    public class BudgetAccountBLL : SubjectBLL
    {



        private List<T_FB_BUDGETACCOUNT> _TempT = null;
        private List<T_FB_BUDGETACCOUNT> TempT
        {
            get
            {
                if (_TempT == null)
                {
                    _TempT = T_FB_BUDGETACCOUNTs.ToList();
                }
                return _TempT;
            }
        }
        private IQueryable<T_FB_BUDGETACCOUNT> _T_FB_BUDGETACCOUNTs;
        public IQueryable<T_FB_BUDGETACCOUNT> T_FB_BUDGETACCOUNTs
        {
            get
            {
                if (_T_FB_BUDGETACCOUNTs == null)
                {

                    _T_FB_BUDGETACCOUNTs = this.GetTable<T_FB_BUDGETACCOUNT>();
                    _T_FB_BUDGETACCOUNTs = (_T_FB_BUDGETACCOUNTs as ObjectQuery<T_FB_BUDGETACCOUNT>).Include(typeof(T_FB_SUBJECT).Name);

                }
                // _T_FB_BUDGETACCOUNTs.ToList();
                return _T_FB_BUDGETACCOUNTs;
            }
        }

        private IQueryable<T_FB_PERSONACCOUNT> _T_FB_PERSONACCOUNTs;
        public IQueryable<T_FB_PERSONACCOUNT> T_FB_PERSONACCOUNTs
        {
            get
            {
                if (_T_FB_PERSONACCOUNTs == null)
                {

                    _T_FB_PERSONACCOUNTs = this.GetTable<T_FB_PERSONACCOUNT>();
                }
                return _T_FB_PERSONACCOUNTs;
            }
        }





        public bool UpdateAccount(FBEntity fbEntity, CheckStates statesNew)
        {
            return UpdateAccount(fbEntity, statesNew, false);
        }
        public bool UpdateAccount(FBEntity fbEntity, CheckStates statesNew, bool isJustCheck)
        {
            bool tempValue = true;
            EntityObject entity = fbEntity.Entity;

            #region 年度（月度）预算审核通过后不修改预算总账, 待年度（月度）预算汇总后，方可生效
            List<Type> listType = new List<Type>();
            listType.Add(typeof(T_FB_COMPANYBUDGETAPPLYMASTER));    // 年度预算
            listType.Add(typeof(T_FB_COMPANYBUDGETMODMASTER));    // 年度预算变更
            listType.Add(typeof(T_FB_DEPTBUDGETAPPLYMASTER));       // 月度预算
            listType.Add(typeof(T_FB_DEPTBUDGETADDMASTER));
            listType.Add(typeof(T_FB_DEPTTRANSFERMASTER));
            Type entityType = entity.GetType();
            // 如果审核通过，并且是年度预算申请或者月度预算申请，不需要修改预算总账，但需要自动创建年/月度预算汇总申请单据
            if (statesNew == CheckStates.Approved && entityType == typeof(T_FB_COMPANYBUDGETAPPLYMASTER))
            {
                return UpdateCompanyBudgetApply(entity as T_FB_COMPANYBUDGETAPPLYMASTER, statesNew);

            }
            else if (statesNew == CheckStates.Approved && entityType == typeof(T_FB_DEPTBUDGETAPPLYMASTER))
            {
                return UpdateDeptBudgetApply(entity as T_FB_DEPTBUDGETAPPLYMASTER, statesNew);
            }
            //审核中的年度、月度预算都不处理, 汇总审核通过后才扣除
            // 审核中的月度增补不处处理，提交审核通过后才扣除
            else if (statesNew == CheckStates.Approving && listType.Contains(entityType))
            {
                return true;
            }


            // 如果是 年/月度预算汇总申请，并且是审核通过或审核不通过时，更新汇总的每个申请单(生效或不生效）并修改预算总账。
            if (statesNew == CheckStates.Approved || statesNew == CheckStates.UnApproved)
            {
                if (entity.GetType() == typeof(T_FB_DEPTBUDGETSUMMASTER))
                {
                    //满足以下条件返回ture：汇总审核不通过, 没有汇总设置或汇总设置的汇总单终审通过
                    if (GetCheckDeptBudgetSum(entity as T_FB_DEPTBUDGETSUMMASTER, statesNew))
                    {
                        return UpdateDeptBudgetSum(entity as T_FB_DEPTBUDGETSUMMASTER, statesNew);
                    }

                    //创建和更新汇总单
                    if (statesNew == CheckStates.Approved)
                    {
                        return CreateDeptBudgetSumSetMaster(fbEntity);
                    }
                }
                else if (entity.GetType() == typeof(T_FB_COMPANYBUDGETSUMMASTER))
                {

                    //满足以下条件返回ture：汇总审核不通过, 没有汇总设置或汇总设置的汇总单终审通过
                    if (GetCheckCompanyBudgetSum(entity as T_FB_COMPANYBUDGETSUMMASTER, statesNew))
                    {
                        return UpdateCompanyBudgetSum(entity as T_FB_COMPANYBUDGETSUMMASTER, statesNew);
                    }
                    //创建和更新汇总单
                    if (statesNew == CheckStates.Approved)
                    {
                        return CreateCompanyBudgetSumSetMaster(fbEntity);
                    }
                }
            }
            #endregion

            // 更新预算总账
            tempValue &= UpdateBudgetAccount(entity, statesNew, isJustCheck);

            // 更新个人往来总账
            tempValue &= UpdatePersonAccount(entity, statesNew, isJustCheck);

            return tempValue;
        }

        #region 2. 更新预算总账

        #region BudgetAccount

        public bool UpdateBudgetAccount(EntityObject entity, CheckStates statesNew, bool isJustCheck)
        {

            List<AccountItem> listAccountItem = GetAccountItem(entity, statesNew, AccountType.BudgetAccount);

            listAccountItem.ForEach(accountItem =>
            {
                FBEntityState state = FBEntityState.Unchanged;

                T_FB_BUDGETACCOUNT budgetAccountItem = FindBudgetAccout(accountItem);

                // 增加预算, 且终审通过
                if (accountItem.AccountOpertaion == AccountOpertaion.Add)
                {
                    if (statesNew == CheckStates.Approved) // 审核通过
                    {
                        budgetAccountItem.USABLEMONEY = budgetAccountItem.USABLEMONEY.Add(accountItem.moneyUsable);
                        budgetAccountItem.BUDGETMONEY = budgetAccountItem.BUDGETMONEY.Add(accountItem.moenyBudget);
                        budgetAccountItem.ACTUALMONEY = budgetAccountItem.ACTUALMONEY.Add(accountItem.moneyActual);
                        state = FBEntityState.Modified;
                    }
                }
                else if (accountItem.AccountOpertaion == AccountOpertaion.Subtract)// 减少预算费用
                {
                    if (statesNew == CheckStates.Approving) // 提交审核
                    {

                        if (accountItem.moneyUsable.Equal(0) == true) // 有些情况是不需要修改
                        {
                            return;
                        }
                        budgetAccountItem.USABLEMONEY = budgetAccountItem.USABLEMONEY.Add(0 - accountItem.moneyUsable);
                    }
                    else if (statesNew == CheckStates.Approved) // 审核通过
                    {
                        // if ( 
                        budgetAccountItem.BUDGETMONEY = budgetAccountItem.BUDGETMONEY.Subtract(accountItem.moenyBudget);
                        budgetAccountItem.ACTUALMONEY = budgetAccountItem.ACTUALMONEY.Subtract(accountItem.moneyActual);
                        budgetAccountItem.PAIEDMONEY = budgetAccountItem.PAIEDMONEY.Add(accountItem.moneyPaid);

                        // 补差价，提交审核时，扣减了可用金额 moneyUsable, 而实际审核批准的金额可能少于申请时扣减的可用金额
                        decimal? subMoney = accountItem.moneyUsable.Subtract(accountItem.moneyActual);
                        budgetAccountItem.USABLEMONEY = budgetAccountItem.USABLEMONEY.Add(subMoney);
                    }
                    else if (statesNew == CheckStates.UnApproved) // 审核不通过
                    {
                        if (accountItem.moneyUsable.Equal(0) == true) // 有些情况是不需要修改
                        {
                            return;
                        }
                        budgetAccountItem.USABLEMONEY = budgetAccountItem.USABLEMONEY.Add(accountItem.moneyUsable);
                    }
                    state = FBEntityState.Modified;
                }

                // 检查规则
                if (accountItem.CheckRule != null)
                {
                    bool bRulePass = accountItem.CheckRule(budgetAccountItem, statesNew);
                    if (!bRulePass)
                    {
                        return;
                    }
                }

                // 有修改过才更新到数据库中
                if (state == FBEntityState.Modified)
                {
                    //记录流水帐
                    var waterFlow = accountItem.CreateWaterFlow(budgetAccountItem);
                    waterFlow.TRIGGEREVENT = statesNew.ToString();

                    if (!isJustCheck)
                    {
                        this.Attach(budgetAccountItem);
                        this.Attach(waterFlow);
                    }
                }
            });
            if (!isJustCheck)
            {
                this.SaveChanges();
            }
            return true;
        }

        private T_FB_BUDGETACCOUNT FindBudgetAccout(AccountItem accountItem)
        {

            if (accountItem.T_FB_SUBJECT == null)
            {
                throw new Exception("数据有异常,　科目对象为空, 参数:"
                           + "\r\nBudgetYear: " + accountItem.BudgetYear
                           + "\r\nBudgetMonth: " + accountItem.BudgetMonth
                           + "\r\nOwnerCompanyID: " + accountItem.OwnerCompanyID
                           + "\r\nOwnerDepartmentID: " + accountItem.OwnerDepartmentID
                           + "\r\nAccountType: " + accountItem.AccountType.ToString()
                           + "\r\nOwnerID: " + accountItem.OwnerID
                           + "\r\nOwnerPostID: " + accountItem.OwnerPostID);

            }
            T_FB_BUDGETACCOUNT budgetAccountItem = null;

            var accountData = from item in T_FB_BUDGETACCOUNTs
                              where item.BUDGETYEAR == accountItem.BudgetYear &&
                                     item.BUDGETMONTH == accountItem.BudgetMonth &&
                                     item.OWNERCOMPANYID == accountItem.OwnerCompanyID &&
                                     item.OWNERDEPARTMENTID == accountItem.OwnerDepartmentID &&
                                     item.T_FB_SUBJECT.SUBJECTID == accountItem.T_FB_SUBJECT.SUBJECTID &&
                                     item.ACCOUNTOBJECTTYPE == (int)accountItem.AccountType
                              select item;

            if (accountItem.AccountType == AccountObjectType.Person || accountItem.AccountType == AccountObjectType.Personnext)
            {
                accountData = from item in accountData
                              where item.OWNERID == accountItem.OwnerID &&
                                item.OWNERPOSTID == accountItem.OwnerPostID
                              select item;
            }
            budgetAccountItem = accountData.FirstOrDefault();
            if (budgetAccountItem == null)
            {

                budgetAccountItem = new T_FB_BUDGETACCOUNT();

                //if (accountItem.T_FB_SUBJECT.EntityState != EntityState.Detached)
                //{
                //    this.Detach(accountItem.T_FB_SUBJECT);
                //}
                budgetAccountItem.T_FB_SUBJECT = accountItem.T_FB_SUBJECT;
                budgetAccountItem.BUDGETACCOUNTID = Guid.NewGuid().ToString();
                budgetAccountItem.ACCOUNTOBJECTTYPE = Convert.ToDecimal((int)accountItem.AccountType);
                budgetAccountItem.OWNERCOMPANYID = accountItem.OwnerCompanyID;
                budgetAccountItem.OWNERDEPARTMENTID = accountItem.OwnerDepartmentID;
                budgetAccountItem.OWNERID = accountItem.OwnerID;
                budgetAccountItem.OWNERPOSTID = accountItem.OwnerPostID;
                budgetAccountItem.CREATEDATE = DateTime.Now;
                budgetAccountItem.UPDATEDATE = DateTime.Now;
                budgetAccountItem.CREATEUSERID = SYSTEM_USER_ID;
                budgetAccountItem.UPDATEUSERID = SYSTEM_USER_ID;

                budgetAccountItem.BUDGETMONEY = 0;
                budgetAccountItem.ACTUALMONEY = 0;
                budgetAccountItem.USABLEMONEY = 0;
                budgetAccountItem.PAIEDMONEY = 0;

                budgetAccountItem.BUDGETYEAR = accountItem.BudgetYear;//  DateTime.Now.Year;
                budgetAccountItem.BUDGETMONTH = accountItem.BudgetMonth; // DateTime.Now.Month;

                // budgetAccountItem.T_FB_SUBJECTReference.EntityKey = accountItem.T_FB_SUBJECT.EntityKey;

            }
            return budgetAccountItem;
        }

        public List<AccountItem> GetAccountItem(EntityObject entity, CheckStates statesNew, AccountType accountType)
        {
            List<AccountItem> listResult = new List<AccountItem>();
            Type typeEntity = entity.GetType();

            string preMethodName = accountType == AccountType.BudgetAccount ? "Get" : "Fetch";
            MethodInfo method = this.GetType().GetMethod(preMethodName + typeEntity.Name);
            if (method != null)
            {
                object result = method.Invoke(this, new object[] { entity, statesNew });
                listResult = result as List<AccountItem>;
            }
            return listResult;
        }


        #endregion

        public bool GetCheckCompanyBudgetSum(T_FB_COMPANYBUDGETSUMMASTER entity, CheckStates statesNew)
        {
            try
            {
                //查询汇总设置表是否有记录
                FBEntity fbSumMasterSet = new FBEntity();
                QueryExpression qeDetail = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
                qeDetail.QueryType = "T_FB_SUMSETTINGSDETAIL";

                QueryExpression qeDetailEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                qeDetail.RelatedExpression = qeDetailEdits;
                T_FB_SUMSETTINGSDETAIL detailset = GetEntities<T_FB_SUMSETTINGSDETAIL>(qeDetail).FirstOrDefault();

                if (detailset != null)
                {
                    QueryExpression qeMaster = QueryExpression.Equal("SUMSETTINGSMASTERID", detailset.T_FB_SUMSETTINGSMASTERReference.EntityKey.EntityKeyValues[0].Value.ToString());
                    qeMaster.QueryType = "T_FB_SUMSETTINGSMASTER";

                    QueryExpression qeMasterEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                    qeMaster.RelatedExpression = qeMasterEdits;
                    T_FB_SUMSETTINGSMASTER masterset = GetEntities<T_FB_SUMSETTINGSMASTER>(qeMaster).FirstOrDefault();

                    //查询汇总单是否有记录
                    FBEntity SumMaster = new FBEntity();
                    string budgetYear = entity.BUDGETYEAR.ToString();
                    QueryExpression qe = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
                    qe.QueryType = "T_FB_COMPANYBUDGETSUMMASTER";

                    QueryExpression qeSumEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                    QueryExpression qeSumID = QueryExpression.Equal("SUMSETTINGSMASTERID", masterset.SUMSETTINGSMASTERID);
                    QueryExpression qeSumYear = QueryExpression.Equal("BUDGETYEAR", budgetYear);
                    QueryExpression qeSumLevel = QueryExpression.Equal("SUMLEVEL", "1");

                    qeSumYear.RelatedExpression = qeSumLevel;
                    qeSumID.RelatedExpression = qeSumYear;
                    qeSumEdits.RelatedExpression = qeSumID;
                    qe.RelatedExpression = qeSumEdits;

                    T_FB_COMPANYBUDGETSUMMASTER master = GetEntities<T_FB_COMPANYBUDGETSUMMASTER>(qe).FirstOrDefault();
                    if ((master != null && master.SUMSETTINGSMASTERID != null && statesNew == CheckStates.Approved) || statesNew == CheckStates.UnApproved)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SystemBLL.Debug("汇总单据创建异常：" + ex.ToString());
                return false;
            }
        }

        public bool GetCheckDeptBudgetSum(T_FB_DEPTBUDGETSUMMASTER entity, CheckStates statesNew)
        {
            try
            {
                //查询汇总设置表是否有记录
                FBEntity fbSumMasterSet = new FBEntity();
                QueryExpression qeDetail = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
                qeDetail.QueryType = "T_FB_SUMSETTINGSDETAIL";

                QueryExpression qeDetailEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                qeDetail.RelatedExpression = qeDetailEdits;

                T_FB_SUMSETTINGSDETAIL detailset = GetEntities<T_FB_SUMSETTINGSDETAIL>(qeDetail).FirstOrDefault();

                if (detailset != null)
                {
                    QueryExpression qeMaster = QueryExpression.Equal("SUMSETTINGSMASTERID", detailset.T_FB_SUMSETTINGSMASTERReference.EntityKey.EntityKeyValues[0].Value.ToString());
                    qeMaster.QueryType = "T_FB_SUMSETTINGSMASTER";

                    QueryExpression qeMasterEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                    qeMaster.RelatedExpression = qeMasterEdits;

                    T_FB_SUMSETTINGSMASTER masterset = GetEntities<T_FB_SUMSETTINGSMASTER>(qeMaster).FirstOrDefault();

                    //查询汇总单是否有记录
                    FBEntity SumMaster = new FBEntity();
                    string budgetMonth = entity.BUDGETARYMONTH.ToString("yyyy-MM-dd");
                    QueryExpression qe = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
                    qe.QueryType = "T_FB_DEPTBUDGETSUMMASTER";

                    QueryExpression qeSumEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                    QueryExpression qeSumID = QueryExpression.Equal("SUMSETTINGSMASTERID", masterset.SUMSETTINGSMASTERID);
                    QueryExpression qeSumMonth = QueryExpression.Equal("BUDGETARYMONTH", budgetMonth);
                    QueryExpression qeSumLevel = QueryExpression.Equal("SUMLEVEL", "1");

                    qeSumMonth.RelatedExpression = qeSumLevel;
                    qeSumID.RelatedExpression = qeSumMonth;
                    qeSumEdits.RelatedExpression = qeSumID;
                    qe.RelatedExpression = qeSumEdits;

                    T_FB_DEPTBUDGETSUMMASTER master = GetEntities<T_FB_DEPTBUDGETSUMMASTER>(qe).FirstOrDefault();
                    if ((master != null && master.SUMSETTINGSMASTERID != null && statesNew == CheckStates.Approved) || statesNew == CheckStates.UnApproved)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SystemBLL.Debug("汇总单据创建异常：" + ex.ToString());
                return false;
            }
        }


        public bool UpdateCompanyBudgetApply(T_FB_COMPANYBUDGETAPPLYMASTER entity, CheckStates statesNew)
        {

            if (statesNew == CheckStates.Approved)
            {
                CreateCompanyBudgetSumDetail(entity);
            }
            return true;
        }

        public bool UpdateDeptBudgetApply(T_FB_DEPTBUDGETAPPLYMASTER entity, CheckStates statesNew)
        {

            if (statesNew == CheckStates.Approved)
            {
                CreateDeptBudgetSumDetail(entity);
            }
            return true;
        }

        public bool UpdateDeptBudgetSum(T_FB_DEPTBUDGETSUMMASTER entity, CheckStates statesNew)
        {
            if (!entity.T_FB_DEPTBUDGETSUMDETAIL.IsLoaded) { entity.T_FB_DEPTBUDGETSUMDETAIL.Load(); }

            if (statesNew == CheckStates.UnApproved)
            {
                CheckDeptBudgetSumBySumSetting(entity, statesNew);
            }

            foreach (var item in entity.T_FB_DEPTBUDGETSUMDETAIL)
            {
                if (!item.T_FB_DEPTBUDGETAPPLYMASTERReference.IsLoaded)
                {
                    item.T_FB_DEPTBUDGETAPPLYMASTERReference.Load();
                }

                SumValid va = statesNew == CheckStates.Approved ? SumValid.Valid : SumValid.NotValid;
                string valid = ((int)va).ToString();
                item.T_FB_DEPTBUDGETAPPLYMASTER.ISVALID = valid;//未汇总
                BassBllSave(item.T_FB_DEPTBUDGETAPPLYMASTER, FBEntityState.Modified);
                UpdateBudgetAccount(item.T_FB_DEPTBUDGETAPPLYMASTER, statesNew, false);
            }

            return true;
        }

        /// <summary>
        /// 月度预算审核不通过时，检查是否存在汇总设置，如存在就更新汇总设置中所有机构的汇总单
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statesNew"></param>
        private void CheckDeptBudgetSumBySumSetting(T_FB_DEPTBUDGETSUMMASTER entity, CheckStates statesNew)
        {
            if (statesNew != CheckStates.UnApproved)
            {
                return;
            }

            FBEntity fbSumMasterSet = new FBEntity();
            QueryExpression qeDetail = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
            qeDetail.QueryType = "T_FB_SUMSETTINGSDETAIL";

            QueryExpression qeDetailEdits = QueryExpression.Equal(FieldName.EditStates, "1");
            qeDetail.RelatedExpression = qeDetailEdits;

            List<T_FB_SUMSETTINGSDETAIL> detailsets = GetEntities<T_FB_SUMSETTINGSDETAIL>(qeDetail);

            if (detailsets != null)
            {
                detailsets.ForEach(item =>
                {
                    FBEntity SumMaster = new FBEntity();
                    string budgetMonth = entity.BUDGETARYMONTH.ToString("yyyy-MM-dd");
                    QueryExpression qe = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
                    qe.QueryType = "T_FB_DEPTBUDGETSUMMASTER";

                    QueryExpression qeSumCheckStates = QueryExpression.Equal(FieldName.CheckStates, "2");
                    QueryExpression qeSumEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                    QueryExpression qeCompanyID = QueryExpression.Equal(FieldName.OwnerCompanyID, item.OWNERCOMPANYID);
                    QueryExpression qeSumMonth = QueryExpression.Equal("BUDGETARYMONTH", budgetMonth);

                    qeCompanyID.RelatedExpression = qeSumMonth;
                    qeSumEdits.RelatedExpression = qeCompanyID;
                    qeSumCheckStates.RelatedExpression = qeSumEdits;
                    qe.RelatedExpression = qeSumCheckStates;

                    List<T_FB_DEPTBUDGETSUMMASTER> deptmasters = GetEntities<T_FB_DEPTBUDGETSUMMASTER>(qe);

                    deptmasters.ForEach(master =>
                    {
                        master.CHECKSTATES = Convert.ToDecimal((int)statesNew);
                        BassBllSave(master, FBEntityState.Modified);
                    });
                });
            }
        }

        public bool UpdateCompanyBudgetSum(T_FB_COMPANYBUDGETSUMMASTER entity, CheckStates statesNew)
        {
            if (!entity.T_FB_COMPANYBUDGETSUMDETAIL.IsLoaded) { entity.T_FB_COMPANYBUDGETSUMDETAIL.Load(); }

            if (statesNew == CheckStates.UnApproved)
            {
                CheckCompanyBudgetSumBySumSetting(entity, statesNew);
            }

            entity.T_FB_COMPANYBUDGETSUMDETAIL.ToList().ForEach(item =>
            {
                item.T_FB_COMPANYBUDGETAPPLYMASTERReference.Load();
                SumValid va = statesNew == CheckStates.Approved ? SumValid.Valid : SumValid.NotValid;
                string valid = ((int)va).ToString();
                item.T_FB_COMPANYBUDGETAPPLYMASTER.ISVALID = valid;
                BassBllSave(item.T_FB_COMPANYBUDGETAPPLYMASTER, FBEntityState.Modified);
                UpdateBudgetAccount(item.T_FB_COMPANYBUDGETAPPLYMASTER, statesNew, false);
            });

            return true;
        }

        /// <summary>
        /// 月度预算审核不通过时，检查是否存在汇总设置，如存在就更新汇总设置中所有机构的汇总单
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statesNew"></param>
        private void CheckCompanyBudgetSumBySumSetting(T_FB_COMPANYBUDGETSUMMASTER entity, CheckStates statesNew)
        {
            if (statesNew != CheckStates.UnApproved)
            {
                return;
            }

            FBEntity fbSumMasterSet = new FBEntity();
            QueryExpression qeDetail = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
            qeDetail.QueryType = "T_FB_SUMSETTINGSDETAIL";

            QueryExpression qeDetailEdits = QueryExpression.Equal(FieldName.EditStates, "1");
            qeDetail.RelatedExpression = qeDetailEdits;

            List<T_FB_SUMSETTINGSDETAIL> detailsets = GetEntities<T_FB_SUMSETTINGSDETAIL>(qeDetail);

            if (detailsets != null)
            {
                detailsets.ForEach(item =>
                {
                    FBEntity SumMaster = new FBEntity();
                    string budgetYear = entity.BUDGETYEAR.ToString();
                    QueryExpression qe = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
                    qe.QueryType = "T_FB_COMPANYBUDGETSUMMASTER";

                    QueryExpression qeSumCheckStates = QueryExpression.Equal(FieldName.CheckStates, "2");
                    QueryExpression qeSumEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                    QueryExpression qeCompanyID = QueryExpression.Equal(FieldName.OwnerCompanyID, item.OWNERCOMPANYID);
                    QueryExpression qeSumMonth = QueryExpression.Equal("BUDGETYEAR", budgetYear);

                    qeCompanyID.RelatedExpression = qeSumMonth;
                    qeSumEdits.RelatedExpression = qeCompanyID;
                    qeSumCheckStates.RelatedExpression = qeSumEdits;
                    qe.RelatedExpression = qeSumCheckStates;

                    List<T_FB_COMPANYBUDGETSUMMASTER> compmasters = GetEntities<T_FB_COMPANYBUDGETSUMMASTER>(qe);

                    compmasters.ForEach(master =>
                    {
                        master.CHECKSTATES = Convert.ToDecimal((int)statesNew);
                        BassBllSave(master, FBEntityState.Modified);
                    });
                });
            }
        }



        #region 年度
        /// <summary>
        /// 公司预算
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_COMPANYBUDGETAPPLYMASTER(T_FB_COMPANYBUDGETAPPLYMASTER entity, CheckStates statesNew)
        {


            if (!entity.T_FB_COMPANYBUDGETAPPLYDETAIL.IsLoaded) { entity.T_FB_COMPANYBUDGETAPPLYDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_COMPANYBUDGETAPPLYDETAIL.ToList().CreateList(item =>
                {
                    if (!item.T_FB_SUBJECTReference.IsLoaded)
                    {
                        item.T_FB_SUBJECTReference.Load();
                    }
                    if (item.BUDGETMONEY == 0)
                    {
                        return null;
                    }
                    AccountItem accountItem = new AccountItem();
                    accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                    accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                    accountItem.moenyBudget = item.BUDGETMONEY;
                    accountItem.moneyUsable = item.BUDGETMONEY;
                    accountItem.moneyActual = item.BUDGETMONEY;
                    accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                    accountItem.OwnerPostID = entity.OWNERPOSTID;

                    accountItem.BudgetMonth = 1;
                    accountItem.BudgetYear = entity.BUDGETYEAR;

                    accountItem.AccountType = (entity.BUDGETYEAR == DateTime.Now.Year) ? AccountObjectType.Company : AccountObjectType.Companynext;
                    accountItem.AccountOpertaion = AccountOpertaion.Add;

                    // 流水帐
                    accountItem.MasterEntity = entity;
                    accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                    accountItem.orderDetailID = item.COMPANYBUDGETAPPLYDETAILID;

                    return accountItem;
                });
            return listResult;


        }

        /// <summary>
        /// 公司预算变更
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_COMPANYBUDGETMODMASTER(T_FB_COMPANYBUDGETMODMASTER entity, CheckStates statesNew)
        {
            if (!entity.T_FB_COMPANYBUDGETMODDETAIL.IsLoaded) { entity.T_FB_COMPANYBUDGETMODDETAIL.Load(); }

            List<AccountItem> listResult = entity.T_FB_COMPANYBUDGETMODDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                if (item.BUDGETMONEY == 0)
                {
                    return null;
                }

                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = item.OWNERDEPARTMENTID;
                accountItem.moenyBudget = item.BUDGETMONEY;
                accountItem.moneyUsable = item.BUDGETMONEY;
                accountItem.moneyActual = item.BUDGETMONEY;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerPostID = entity.OWNERPOSTID;

                accountItem.BudgetYear = entity.BUDGETYEAR;
                accountItem.BudgetMonth = 1;

                accountItem.AccountType = AccountObjectType.Company;
                accountItem.AccountOpertaion = AccountOpertaion.Add;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                accountItem.orderDetailID = item.COMPANYBUDGETMODDETAILID;

                return accountItem;
            });
            return listResult;
        }

        /// <summary>
        /// 公司预算变更
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_COMPANYTRANSFERMASTER(T_FB_COMPANYTRANSFERMASTER entity, CheckStates statesNew)
        {

            if (!entity.T_FB_COMPANYTRANSFERDETAIL.IsLoaded) { entity.T_FB_COMPANYTRANSFERDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_COMPANYTRANSFERDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }

                if (item.TRANSFERMONEY == 0)
                {
                    return null;
                }

                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = entity.TRANSFERFROM; // 调出部门
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerPostID = entity.OWNERPOSTID;
                accountItem.OwnerPostID = entity.OWNERID;
                accountItem.moenyBudget = item.TRANSFERMONEY;
                accountItem.moneyUsable = item.TRANSFERMONEY;
                accountItem.moneyActual = item.TRANSFERMONEY;

                accountItem.BudgetYear = entity.BUDGETYEAR;
                accountItem.BudgetMonth = 1;

                accountItem.AccountType = AccountObjectType.Company;
                accountItem.AccountOpertaion = AccountOpertaion.Subtract;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.TRANSFERMONEY;
                accountItem.orderDetailID = item.COMPANYTRANSFERDETAILID;

                return accountItem;
            });

            List<AccountItem> listResultTo = entity.T_FB_COMPANYTRANSFERDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                if (item.TRANSFERMONEY == 0)
                {
                    return null;
                }

                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = entity.TRANSFERTO;  // 调入部门
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerPostID = entity.OWNERPOSTID;
                accountItem.OwnerPostID = entity.OWNERID;
                accountItem.moenyBudget = item.TRANSFERMONEY;
                accountItem.moneyUsable = item.TRANSFERMONEY;
                accountItem.moneyActual = item.TRANSFERMONEY;

                accountItem.BudgetYear = entity.BUDGETYEAR;
                accountItem.BudgetMonth = 1;

                accountItem.AccountType = AccountObjectType.Company;
                accountItem.AccountOpertaion = AccountOpertaion.Add;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.TRANSFERMONEY;
                accountItem.orderDetailID = item.COMPANYTRANSFERDETAILID;

                return accountItem;
            });

            listResult.AddRange(listResultTo);
            return listResult;
        }

        #endregion

        #region 月度

        /// <summary>
        /// 部门预算
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_DEPTBUDGETAPPLYMASTER(T_FB_DEPTBUDGETAPPLYMASTER entity, CheckStates statesNew)
        {

            #region 部门月度预算数据
            if (!entity.T_FB_DEPTBUDGETAPPLYDETAIL.IsLoaded) { entity.T_FB_DEPTBUDGETAPPLYDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_DEPTBUDGETAPPLYDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                if (item.TOTALBUDGETMONEY.Equal(0))
                {
                    return null;
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;

                accountItem.moneyUsable = item.BUDGETMONEY; // 预算金额


                accountItem.moenyBudget = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY); // 个人+部门
                accountItem.moneyActual = item.BUDGETMONEY; // 审定的金额

                accountItem.BudgetYear = entity.BUDGETARYMONTH.Year;
                accountItem.BudgetMonth = entity.BUDGETARYMONTH.Month;

                accountItem.AccountType = ((entity.BUDGETARYMONTH.Year == DateTime.Now.Year) && (entity.BUDGETARYMONTH.Month == DateTime.Now.Month)) ?
                    AccountObjectType.Deaprtment : AccountObjectType.Deptnext;
                accountItem.AccountOpertaion = AccountOpertaion.Add;
                accountItem.CheckRule = CheckRule_DeptBudget;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                accountItem.orderDetailID = item.DEPTBUDGETAPPLYDETAILID;
                return accountItem;
            });
            #endregion

            #region 部门年度预算数据

            List<AccountItem> listResultCompany = entity.T_FB_DEPTBUDGETAPPLYDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                if (item.TOTALBUDGETMONEY.Equal(0))
                {
                    return null;
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;


                accountItem.moneyPaid = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY); // 个人+部门审定金额
                accountItem.moneyActual = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY); // 个人+部门审定金额

                #region 2011版
                //accountItem.moneyUsable = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY);
                #endregion
                #region 2012版
                // 因月度预算只在汇总审核通过后一起扣除可用额度，所以这里设置0额度.
                accountItem.moneyUsable = 0;
                #endregion
                accountItem.BudgetYear = entity.BUDGETARYMONTH.Year;
                accountItem.BudgetMonth = 1;

                accountItem.AccountType = (entity.BUDGETARYMONTH.Year == DateTime.Now.Year) ?
                    AccountObjectType.Company : AccountObjectType.Companynext;

                accountItem.AccountOpertaion = AccountOpertaion.Subtract;

                accountItem.CheckRule = CheckRule_DeptBudget;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                accountItem.orderDetailID = item.DEPTBUDGETAPPLYDETAILID;

                return accountItem;
            });

            #endregion

            #region 个人预算数据
            List<AccountItem> listPerson = new List<AccountItem>();
            entity.T_FB_DEPTBUDGETAPPLYDETAIL.ToList().ForEach(item =>
                {
                    if (!item.T_FB_PERSONBUDGETAPPLYDETAIL.IsLoaded)
                    {
                        item.T_FB_PERSONBUDGETAPPLYDETAIL.Load();
                    }
                    item.T_FB_PERSONBUDGETAPPLYDETAIL.ToList().ForEach(persondetail =>
                        {
                            if (!persondetail.T_FB_SUBJECTReference.IsLoaded) { persondetail.T_FB_SUBJECTReference.Load(); }
                            if (persondetail.BUDGETMONEY != 0)
                            {
                                AccountItem accountItem = new AccountItem();
                                accountItem.T_FB_SUBJECT = persondetail.T_FB_SUBJECT;
                                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                                accountItem.OwnerPostID = persondetail.OWNERPOSTID;
                                accountItem.OwnerID = persondetail.OWNERID;
                                accountItem.moneyPaid = persondetail.BUDGETMONEY;
                                accountItem.moneyUsable = persondetail.BUDGETMONEY;
                                accountItem.moneyActual = persondetail.BUDGETMONEY;
                                accountItem.moenyBudget = persondetail.BUDGETMONEY;


                                accountItem.BudgetYear = entity.BUDGETARYMONTH.Year;
                                accountItem.BudgetMonth = entity.BUDGETARYMONTH.Month;


                                accountItem.AccountType = ((entity.BUDGETARYMONTH.Year == DateTime.Now.Year) && (entity.BUDGETARYMONTH.Month == DateTime.Now.Month)) ?
                        AccountObjectType.Person : AccountObjectType.Personnext;

                                accountItem.AccountOpertaion = AccountOpertaion.Add;

                                accountItem.CheckRule = CheckRule_PersonBudget;

                                // 流水帐
                                accountItem.MasterEntity = entity;
                                accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                                accountItem.orderDetailID = item.DEPTBUDGETAPPLYDETAILID;

                                listPerson.Add(accountItem);
                            }


                        });
                });
            #endregion

            //beyond

            //listResult.AddRange(listPerson);        //临时使用
            listResultCompany.AddRange(listPerson);

            listResult.AddRange(listResultCompany);
            return listResult;
        }

        /// <summary>
        /// 部门预算
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_DEPTBUDGETADDMASTER(T_FB_DEPTBUDGETADDMASTER entity, CheckStates statesNew)
        {

            if (!entity.T_FB_DEPTBUDGETADDDETAIL.IsLoaded)
            {
                entity.T_FB_DEPTBUDGETADDDETAIL.Load();
            }
            #region 部门月度预算数据

            List<AccountItem> listResult = entity.T_FB_DEPTBUDGETADDDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }

                if (item.TOTALBUDGETMONEY.Equal(0))
                {
                    return null;
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;


                accountItem.moneyUsable = item.BUDGETMONEY;
                accountItem.moneyActual = item.BUDGETMONEY;
                accountItem.moenyBudget = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY); // 个人+部门



                // 增补最终与当前系统时间为准。
                //accountItem.BudgetYear = entity.BUDGETARYMONTH.Year;
                //accountItem.BudgetMonth = entity.BUDGETARYMONTH.Month;
                accountItem.BudgetYear = System.DateTime.Now.Year;
                accountItem.BudgetMonth = System.DateTime.Now.Month;

                accountItem.AccountType = AccountObjectType.Deaprtment;
                accountItem.AccountOpertaion = AccountOpertaion.Add;

                accountItem.CheckRule = CheckRule_DeptBudget;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                accountItem.orderDetailID = item.DEPTBUDGETADDDETAILID;

                return accountItem;
            });

            #endregion

            #region 部门年度预算数据
            List<AccountItem> listResultCompany = entity.T_FB_DEPTBUDGETADDDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                if (item.TOTALBUDGETMONEY.Equal(0))
                {
                    return null;
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;



                //beyond
                accountItem.moneyPaid = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY); // 个人+部门审定金额
                accountItem.moneyActual = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY); // 个人+部门审定金额

                #region 2011版
                // accountItem.moneyUsable = item.PERSONBUDGETMONEY.Add(item.BUDGETMONEY);
                #endregion
                #region 2012版
                // 因月度增补预算只在汇总审核通过后一起扣除可用额度，所以这里设置0额度.
                accountItem.moneyUsable = 0;
                #endregion
                // 增补以当前操作年份为准
                accountItem.BudgetYear = System.DateTime.Now.Year;
                accountItem.BudgetMonth = 1;

                accountItem.CheckRule = CheckRule_DeptBudget;
                accountItem.AccountType = AccountObjectType.Company;
                accountItem.AccountOpertaion = AccountOpertaion.Subtract;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                accountItem.orderDetailID = item.DEPTBUDGETADDDETAILID;

                return accountItem;
            });

            #endregion

            #region 个人预算数据
            List<AccountItem> listPerson = new List<AccountItem>();
            entity.T_FB_DEPTBUDGETADDDETAIL.ToList().ForEach(item =>
            {
                if (!item.T_FB_PERSONBUDGETADDDETAIL.IsLoaded)
                {
                    item.T_FB_PERSONBUDGETADDDETAIL.Load();
                }

                item.T_FB_PERSONBUDGETADDDETAIL.ToList().ForEach(persondetail =>
                {

                    if (!persondetail.T_FB_SUBJECTReference.IsLoaded) { persondetail.T_FB_SUBJECTReference.Load(); }
                    if (persondetail.BUDGETMONEY != 0)
                    {
                        AccountItem accountItem = new AccountItem();
                        accountItem.T_FB_SUBJECT = persondetail.T_FB_SUBJECT;
                        accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                        accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                        accountItem.OwnerPostID = persondetail.OWNERPOSTID;
                        accountItem.OwnerID = persondetail.OWNERID;
                        accountItem.moneyPaid = persondetail.BUDGETMONEY;
                        accountItem.moneyUsable = persondetail.BUDGETMONEY;
                        accountItem.moneyActual = persondetail.BUDGETMONEY;
                        accountItem.moenyBudget = persondetail.BUDGETMONEY;

                        // 增补最终与当前系统时间为准。
                        //accountItem.BudgetYear = entity.BUDGETARYMONTH.Year;
                        //accountItem.BudgetMonth = entity.BUDGETARYMONTH.Month;
                        accountItem.BudgetYear = System.DateTime.Now.Year;
                        accountItem.BudgetMonth = System.DateTime.Now.Month;


                        accountItem.AccountType = AccountObjectType.Person;
                        accountItem.AccountOpertaion = AccountOpertaion.Add;

                        accountItem.CheckRule = CheckRule_PersonBudget;

                        // 流水帐
                        accountItem.MasterEntity = entity;
                        accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                        accountItem.orderDetailID = item.DEPTBUDGETADDDETAILID;

                        listPerson.Add(accountItem);
                    }


                });
            });
            #endregion


            //beyond
            listResultCompany.AddRange(listPerson);

            listResult.AddRange(listResultCompany);
            //listResult.AddRange(listResultPerson);
            return listResult;
        }

        /// <summary>
        /// 部门预算变更
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_DEPTTRANSFERMASTER(T_FB_DEPTTRANSFERMASTER entity, CheckStates statesNew)
        {
            List<AccountItem> resultList = new List<AccountItem>();
            QueryExpression qe = QueryExpression.Equal("DEPTTRANSFERMASTERID", entity.DEPTTRANSFERMASTERID);
            qe.Include = new string[] { "T_FB_DEPTTRANSFERDETAIL.T_FB_PERSONTRANSFERDETAIL", "T_FB_DEPTTRANSFERDETAIL.T_FB_SUBJECT" };

            var newEntity = this.GetEntity<T_FB_DEPTTRANSFERMASTER>(qe);
            newEntity.T_FB_DEPTTRANSFERDETAIL.ForEach(item =>
                {
                    decimal? totalTransfermoney = 0;
                    item.T_FB_PERSONTRANSFERDETAIL.ForEach(itemPerson =>
                        {
                            if (itemPerson.BUDGETMONEY.Equal(0))
                            {
                                return;
                            }

                            // 个人调入
                            AccountItem accountItemPerson = new AccountItem();
                            #region 个人调入数据

                            accountItemPerson.T_FB_SUBJECT = item.T_FB_SUBJECT;
                            accountItemPerson.OwnerDepartmentID = newEntity.TRANSFERFROMDEPARTMENTID;
                            accountItemPerson.OwnerCompanyID = newEntity.TRANSFERFROMCOMPANYID;

                            accountItemPerson.OwnerPostID = itemPerson.OWNERPOSTID;
                            accountItemPerson.OwnerID = itemPerson.OWNERID;
                            accountItemPerson.AccountType = AccountObjectType.Person;

                            accountItemPerson.moneyUsable = itemPerson.BUDGETMONEY;
                            accountItemPerson.moneyActual = itemPerson.BUDGETMONEY;

                            accountItemPerson.BudgetYear = System.DateTime.Now.Year;
                            accountItemPerson.BudgetMonth = System.DateTime.Now.Month;

                            accountItemPerson.AccountOpertaion = AccountOpertaion.Add;

                            // 流水帐
                            accountItemPerson.MasterEntity = entity;
                            accountItemPerson.OPERATIONMONEY = itemPerson.BUDGETMONEY;
                            accountItemPerson.orderDetailID = itemPerson.PERSONTRANSFERDETAILID;

                            #endregion
                            // 累加调拨额度
                            totalTransfermoney = totalTransfermoney.Add(itemPerson.BUDGETMONEY);
                            resultList.Add(accountItemPerson);
                        });

                    if (totalTransfermoney.Equal(0))
                    {
                        return;
                    }
                    // 部门调出
                    AccountItem accountItemDept = new AccountItem();

                    #region 部门调出的数据

                    accountItemDept.T_FB_SUBJECT = item.T_FB_SUBJECT;
                    accountItemDept.OwnerDepartmentID = newEntity.TRANSFERFROMDEPARTMENTID;
                    accountItemDept.OwnerCompanyID = newEntity.TRANSFERFROMCOMPANYID;

                    accountItemDept.AccountType = AccountObjectType.Deaprtment;

                    accountItemDept.BudgetYear = System.DateTime.Now.Year;
                    accountItemDept.BudgetMonth = System.DateTime.Now.Month;

                    // 审核通过后，才扣出可用额度
                    accountItemDept.moneyUsable = 0;
                    accountItemDept.moneyActual = totalTransfermoney;

                    accountItemDept.AccountOpertaion = AccountOpertaion.Subtract;

                    accountItemDept.CheckRule = CheckRule_MonthTransfer;
                    // 流水帐
                    accountItemDept.MasterEntity = newEntity;
                    accountItemDept.OPERATIONMONEY = item.TRANSFERMONEY;
                    accountItemDept.orderDetailID = item.DEPTTRANSFERDETAILID;

                    #endregion

                    resultList.Add(accountItemDept);

                });

            return resultList;
        }

        public List<AccountItem> GetT_FB_PERSONMONEYASSIGNMASTER(T_FB_PERSONMONEYASSIGNMASTER entity, CheckStates statesNew)
        {
            if (!entity.T_FB_PERSONMONEYASSIGNDETAIL.IsLoaded) { entity.T_FB_PERSONMONEYASSIGNDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_PERSONMONEYASSIGNDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                if (item.BUDGETMONEY == 0)
                {
                    return null;
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;

                accountItem.OwnerDepartmentID = item.OWNERDEPARTMENTID;
                accountItem.OwnerCompanyID = item.OWNERCOMPANYID;
                accountItem.OwnerPostID = item.OWNERPOSTID;
                accountItem.OwnerID = item.OWNERID;

                accountItem.moneyUsable = item.BUDGETMONEY;
                accountItem.moneyActual = item.BUDGETMONEY;

                // 最终与当前系统时间为准。
                //accountItem.BudgetYear = entity.BUDGETARYMONTH.Year;
                //accountItem.BudgetMonth = entity.BUDGETARYMONTH.Month;
                accountItem.BudgetYear = System.DateTime.Now.Year;
                accountItem.BudgetMonth = System.DateTime.Now.Month;

                accountItem.AccountType = AccountObjectType.Person;
                accountItem.AccountOpertaion = AccountOpertaion.Add;

                // accountItem.CheckRule = 无需检验

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.BUDGETMONEY;
                accountItem.orderDetailID = item.PERSONBUDGETAPPLYDETAILID;

                return accountItem;
            });
            return listResult;
        }
        #endregion

        #region 个人(停用)
        /// <summary>
        /// 个人预算
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        //public List<AccountItem> GetT_FB_PERSONBUDGETAPPLYMASTER(T_FB_PERSONBUDGETAPPLYMASTER entity)
        //{
        //    entity.T_FB_PERSONBUDGETAPPLYDETAIL.Load();
        //    List<AccountItem> listResult = entity.T_FB_PERSONBUDGETAPPLYDETAIL.ToList().CreateList(item =>
        //    {
        //        item.T_FB_SUBJECTReference.Load();
        //        AccountItem accountItem = new AccountItem();
        //        accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
        //        accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
        //        accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
        //        accountItem.OwnerPostID = entity.OWNERPOSTID;
        //        accountItem.OwnerID = entity.OWNERID;

        //        accountItem.moenyBudget = item.BUDGETMONEY;
        //        accountItem.moneyUsable = item.BUDGETMONEY;
        //        accountItem.moneyActual = item.BUDGETMONEY;


        //        accountItem.AccountType = AccountObjectType.Person;
        //        accountItem.AccountOpertaion = AccountOpertaion.JustCheck;
        //        accountItem.CheckRule = CheckRule_PersonBudget;
        //        return accountItem;
        //    });
        //    return listResult;
        //}

        /// <summary>
        /// 个人预算
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        //public List<AccountItem> GetT_FB_PERSONBUDGETADDMASTER(T_FB_PERSONBUDGETADDMASTER entity)
        //{

        //    entity.T_FB_PERSONBUDGETADDDETAIL.Load();
        //    List<AccountItem> listResult = entity.T_FB_PERSONBUDGETADDDETAIL.ToList().CreateList(item =>
        //    {
        //        item.T_FB_SUBJECTReference.Load();
        //        AccountItem accountItem = new AccountItem();
        //        accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
        //        accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
        //        accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
        //        accountItem.OwnerPostID = entity.OWNERPOSTID;

        //        accountItem.moenyBudget = item.BUDGETMONEY;
        //        accountItem.moneyUsable = item.BUDGETMONEY;
        //        accountItem.moneyActual = item.BUDGETMONEY;

        //        accountItem.AccountType = AccountObjectType.Person;
        //        accountItem.AccountOpertaion = AccountOpertaion.JustCheck;
        //        accountItem.CheckRule = CheckRule_PersonBudget;
        //        return accountItem;
        //    });
        //    return listResult;
        //}

        #endregion 个人(停用)

        #region 报销, 借还款
        /// <summary>
        /// 费用报销
        ///    1. 带还款单
        ///         1.判断报销的金额是否大于还款的金额。
        ///    2. 不带还款单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_CHARGEAPPLYMASTER(T_FB_CHARGEAPPLYMASTER entity, CheckStates statesNew)
        {

            if (!entity.T_FB_CHARGEAPPLYDETAIL.IsLoaded) { entity.T_FB_CHARGEAPPLYDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_CHARGEAPPLYDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_BORROWAPPLYDETAILReference.IsLoaded) { item.T_FB_BORROWAPPLYDETAILReference.Load(); }

                decimal? moneyActual = item.CHARGEMONEY;
                decimal? moneyUsable = item.CHARGEMONEY;
                // 有借款单时， 可用金额和实际金额已被预先扣除，些处无需再次处理，但报销费用还需要加入总帐。
                if (item.T_FB_BORROWAPPLYDETAIL != null)
                {
                    if (item.T_FB_BORROWAPPLYDETAIL.UNREPAYMONEY.LessThan(item.CHARGEMONEY))
                    {
                        moneyUsable = item.CHARGEMONEY - item.T_FB_BORROWAPPLYDETAIL.UNREPAYMONEY.Value;
                        moneyActual = moneyUsable;
                    }
                    else
                    {
                        //item.REPAYMONEY = item.CHARGEMONEY;
                        moneyUsable = 0;
                        moneyActual = 0;
                    }
                }

                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                accountItem.OwnerPostID = entity.OWNERPOSTID;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerID = entity.OWNERID;

                accountItem.moneyPaid = item.CHARGEMONEY;
                // 2012年版　，在审核通过后扣除可用额度
                accountItem.moneyUsable = moneyUsable;
                accountItem.moneyActual = moneyActual;

                accountItem.BudgetYear = System.DateTime.Now.Year;
                accountItem.BudgetMonth = System.DateTime.Now.Month;

                if (item.CHARGETYPE.Value == decimal.Parse("1"))
                {
                    accountItem.AccountType = AccountObjectType.Person;

                }
                else
                {
                    accountItem.AccountType = AccountObjectType.Deaprtment;
                }
                accountItem.CheckRule = CheckRule_ChargeBudget;
                accountItem.AccountOpertaion = AccountOpertaion.Subtract;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.CHARGEMONEY;
                accountItem.orderDetailID = item.CHARGEAPPLYDETAILID;

                return accountItem;
            });
            return listResult;
        }

        /// <summary>
        /// 差旅报销
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_TRAVELEXPAPPLYMASTER(T_FB_TRAVELEXPAPPLYMASTER entity, CheckStates statesNew)
        {
            if (!entity.T_FB_TRAVELEXPAPPLYDETAIL.IsLoaded) { entity.T_FB_TRAVELEXPAPPLYDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_TRAVELEXPAPPLYDETAIL.ToList().CreateList(item =>
            {
                if (!item.T_FB_BORROWAPPLYDETAILReference.IsLoaded) { item.T_FB_BORROWAPPLYDETAILReference.Load(); }

                decimal? moneyActual = item.AUDITCHARGEMONEY;
                decimal? moneyUsable = item.TOTALCHARGE;
                // 有借款单时， 可用金额和实际金额已被预先扣除，些处无需再次处理，但报销费用还需要加入总帐。
                if (item.T_FB_BORROWAPPLYDETAIL != null)
                {
                    if (item.T_FB_BORROWAPPLYDETAIL.UNREPAYMONEY.LessThan(item.TOTALCHARGE))
                    {
                        moneyActual = item.TOTALCHARGE - item.T_FB_BORROWAPPLYDETAIL.UNREPAYMONEY.Value;
                        moneyUsable = moneyActual;
                    }
                    else
                    {
                        moneyActual = 0;
                        moneyUsable = 0;
                    }
                }

                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                accountItem.OwnerPostID = entity.OWNERPOSTID;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerID = entity.OWNERID;

                accountItem.moneyUsable = moneyUsable;
                accountItem.moneyActual = moneyActual;
                accountItem.moneyPaid = moneyActual;
                accountItem.BudgetYear = System.DateTime.Now.Year;
                accountItem.BudgetMonth = System.DateTime.Now.Month;


                if (item.CHARGETYPE.Value == (int)ChargeType.Person)
                {
                    accountItem.AccountType = AccountObjectType.Person;
                }
                else
                {
                    accountItem.AccountType = AccountObjectType.Deaprtment;
                }

                accountItem.AccountOpertaion = AccountOpertaion.Subtract;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.TOTALCHARGE;
                accountItem.orderDetailID = item.TRAVELEXPAPPLYDETAILID;

                return accountItem;
            });
            return listResult;
        }

        /// <summary>
        /// 借款申请
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_BORROWAPPLYMASTER(T_FB_BORROWAPPLYMASTER entity, CheckStates statesNew)
        {

            if (!entity.T_FB_BORROWAPPLYDETAIL.IsLoaded) { entity.T_FB_BORROWAPPLYDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_BORROWAPPLYDETAIL.ToList().CreateList(item =>
            {

                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                accountItem.OwnerPostID = entity.OWNERPOSTID;
                accountItem.OwnerID = entity.OWNERID;

                accountItem.moneyPaid = 0;
                accountItem.moneyUsable = item.BORROWMONEY;
                accountItem.moneyActual = item.BORROWMONEY;

                accountItem.BudgetYear = System.DateTime.Now.Year;
                accountItem.BudgetMonth = System.DateTime.Now.Month;

                //暂时定为部门预算
                accountItem.AccountType = item.CHARGETYPE == 1 ? AccountObjectType.Person : AccountObjectType.Deaprtment;

                accountItem.AccountOpertaion = AccountOpertaion.Subtract;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.BORROWMONEY;
                accountItem.orderDetailID = item.BORROWAPPLYDETAILID;
                return accountItem;
            });
            return listResult;
        }

        /// <summary>
        /// 还款申请
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<AccountItem> GetT_FB_REPAYAPPLYMASTER(T_FB_REPAYAPPLYMASTER entity, CheckStates statesNew)
        {

            if (!entity.T_FB_REPAYAPPLYDETAIL.IsLoaded) { entity.T_FB_REPAYAPPLYDETAIL.Load(); }
            List<AccountItem> listResult = entity.T_FB_REPAYAPPLYDETAIL.ToList().CreateList(item =>
            {

                if (!item.T_FB_SUBJECTReference.IsLoaded)
                {
                    item.T_FB_SUBJECTReference.Load();
                }
                AccountItem accountItem = new AccountItem();
                accountItem.T_FB_SUBJECT = item.T_FB_SUBJECT;
                accountItem.OwnerCompanyID = entity.OWNERCOMPANYID;
                accountItem.OwnerDepartmentID = entity.OWNERDEPARTMENTID;
                accountItem.OwnerPostID = entity.OWNERPOSTID;
                accountItem.OwnerID = entity.OWNERID;

                accountItem.moneyUsable = item.REPAYMONEY;
                accountItem.moneyActual = item.REPAYMONEY;


                accountItem.BudgetYear = System.DateTime.Now.Year;
                accountItem.BudgetMonth = System.DateTime.Now.Month;

                //暂时定为部门预算
                accountItem.AccountType = item.CHARGETYPE == 1 ? AccountObjectType.Person : AccountObjectType.Deaprtment;

                accountItem.AccountOpertaion = AccountOpertaion.Add;

                // 流水帐
                accountItem.MasterEntity = entity;
                accountItem.OPERATIONMONEY = item.REPAYMONEY;
                accountItem.orderDetailID = item.REPAYAPPLYDETAILID;
                return accountItem;
            });
            return listResult;
        }
        #endregion

        #region 科目设置检查
        /// <summary>
        /// 检查月预算总额是否超出上限
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool CheckRule_DeptBudget(T_FB_BUDGETACCOUNT account, CheckStates statesNew)
        {
            // 1. 查出相应的 部门科目记录+ 公司科目记录
            QueryExpression qeDept = QueryExpression.Equal(FieldName.OwnerDepartmentID, account.OWNERDEPARTMENTID);
            QueryExpression qeSubject = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", account.T_FB_SUBJECT.SUBJECTID);
            qeDept.RelatedExpression = qeSubject;

            List<T_FB_SUBJECTDEPTMENT> listSubjectDept = GetSubjectDepartment(qeDept);
            // 遍历相应部门科目记录，做校验
            listSubjectDept.ForEach(subjectDept =>
               {
                   AccountObjectType currentAType = (AccountObjectType)account.ACCOUNTOBJECTTYPE.Value;
                   if (currentAType == AccountObjectType.Company) // 年度预算总账
                   {
                       // 年度预算限制
                       if (subjectDept.T_FB_SUBJECTCOMPANY.ISYEARBUDGET.Equal(1))
                       {
                           if (account.USABLEMONEY < 0)
                           {
                               throw new BudgetAccountBBLException("科目 " + subjectDept.T_FB_SUBJECT.SUBJECTNAME + " 的预算额度超出年度预算可用额度")
                                   {
                                       AccountItem = account,
                                       ErrorCode = AccountErrorCode.OverYearMoney
                                   };
                           }
                       }
                   }
                   else if (currentAType == AccountObjectType.Deaprtment) // 月度预算总账
                   {
                       if (subjectDept.LIMITBUDGEMONEY.Equal(0)) // 为 0 或 null 时，不做最大预算的限制
                       {
                           return;
                       }
                       // 预算上限限制
                       if (subjectDept.LIMITBUDGEMONEY.LessThan(account.BUDGETMONEY))
                       {
                           throw new BudgetAccountBBLException("科目 " + subjectDept.T_FB_SUBJECT.SUBJECTNAME + " 的预算额度超出月预算限制")
                               {
                                   AccountItem = account,
                                   ErrorCode = AccountErrorCode.OverLimitedMonthMoney
                               };
                       }
                   }


               });
            return true;
        }

        /// <summary>
        /// 个人预算是否超出个人预算上限
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool CheckRule_PersonBudget(T_FB_BUDGETACCOUNT account, CheckStates statesNew)
        {
            QueryExpression qePost = QueryExpression.Equal(FieldName.OwnerPostID, account.OWNERPOSTID);

            // 1. 查出相应的 岗位科目记录 + 部门科目记录+ 公司科目记录
            List<T_FB_SUBJECTPOST> listSubjectPost = GetSubjectPost(qePost);

            // 2. 遍历相应岗位科目记录，做校验
            listSubjectPost.ForEach(subjectPost =>
                {
                    if (subjectPost.LIMITBUDGEMONEY.Equal(0)) // 为 0 或 null 时，不做最大预算的限制
                    {
                        return;
                    }
                    // 预算上限限制
                    if (subjectPost.LIMITBUDGEMONEY.LessThan(account.BUDGETMONEY))
                    {
                        throw new BudgetAccountBBLException("科目 " + subjectPost.T_FB_SUBJECT.SUBJECTNAME + " 的预算额度超出月预算限制")
                            {
                                AccountItem = account,
                                ErrorCode = AccountErrorCode.OverLimintedPersonMoney
                            };
                    }
                });
            return true;

        }

        /// <summary>
        /// 月度限制时,检查报销费用是否大于可以费用
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool CheckRule_ChargeBudget(T_FB_BUDGETACCOUNT account, CheckStates statesNew)
        {
            QueryExpression qeSubject = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", account.T_FB_SUBJECT.SUBJECTID);
            System.Collections.IList list = null;
            AccountObjectType currentAType = (AccountObjectType)account.ACCOUNTOBJECTTYPE.Value;
            if (currentAType == AccountObjectType.Deaprtment) // 月度预算总账
            {
                QueryExpression qeDept = QueryExpression.Equal(FieldName.OwnerDepartmentID, account.OWNERDEPARTMENTID);
                QueryExpression qeIsMonthLimit = QueryExpression.Equal("T_FB_SUBJECTCOMPANY.ISMONTHLIMIT", "1");
                qeDept.RelatedExpression = qeIsMonthLimit;
                qeIsMonthLimit.RelatedExpression = qeSubject;

                list = GetSubjectDepartment(qeDept);

            }
            else if (currentAType == AccountObjectType.Person) // 个人月度预算总账
            {
                QueryExpression qePost = QueryExpression.Equal(FieldName.OwnerPostID, account.OWNERPOSTID);
                QueryExpression qeIsMonthLimit = QueryExpression.Equal("T_FB_SUBJECTDEPTMENT.T_FB_SUBJECTCOMPANY.ISMONTHLIMIT", "1");
                qePost.RelatedExpression = qeIsMonthLimit;
                qeIsMonthLimit.RelatedExpression = qeSubject;
                //qePost.Include = new string[] {typeof(T_FB_SUBJECT).Name };                

                list = GetSubjectPost(qePost);
            }

            if (list.Count > 0)
            {
                if (account.USABLEMONEY.LessThan(0))
                {
                    QueryExpression qe = QueryExpression.Equal("SUBJECTID", account.T_FB_SUBJECT.SUBJECTID);
                    var q = qe.Query<T_FB_SUBJECT>(this).FirstOrDefault();


                    Tracer.Debug("记录时间：" + System.DateTime.Now.ToString());
                    Tracer.Debug("BUDGETACCOUNTID：" + account.BUDGETACCOUNTID);
                    //Tracer.Debug("报销科目：" + account.T_FB_SUBJECT.SUBJECTNAME);

                    Tracer.Debug("报销科目：" + account.T_FB_SUBJECT.SUBJECTID);
                    Tracer.Debug("可用结余USABLEMONEY：" + account.USABLEMONEY.ToString());
                    Tracer.Debug("PAIEDMONEY:" + account.PAIEDMONEY.ToString());
                    Tracer.Debug("BUDGETMONEY:" + account.BUDGETMONEY.ToString());
                    Tracer.Debug("ACTUALMONEY:" + account.ACTUALMONEY.ToString());
                    Tracer.Debug("报销年月" + account.BUDGETYEAR.ToString() + "-" + account.BUDGETMONTH.ToString());
                    Tracer.Debug("报销公司" + account.OWNERCOMPANYID.ToString());
                    Tracer.Debug("报销部门" + account.OWNERDEPARTMENTID.ToString());
                    Tracer.Debug("报销岗位" + account.OWNERPOSTID.ToString());
                    Tracer.Debug("报销人" + account.OWNERID.ToString());
                    Tracer.Debug("报销人" + account.OWNERID.ToString());
                    Tracer.Debug("报销类型：" + account.ACCOUNTOBJECTTYPE.ToString());
                    string strMessage = string.Empty;
                    if (account.ACCOUNTOBJECTTYPE.Value == 3)
                    {
                        strMessage = "科目：" + q.SUBJECTNAME + ",费用类型：个人费用，可用额度为:" + account.USABLEMONEY.Value.ToString();
                    }
                    else
                    {
                        strMessage = "科目：" + q.SUBJECTNAME + ",费用类型：公共部门费用，可用额度:" + account.USABLEMONEY.Value.ToString();
                    }
                    strMessage += ",您报销的额度超出此科目预算可用额度，请联系公司财务增加额度。";
                    Tracer.Debug(strMessage);
                    throw new BudgetAccountBBLException(strMessage)
                    {
                        AccountItem = account,
                        ErrorCode = AccountErrorCode.OverMonthMoney
                    };
                }
            }
            return true;
        }

        /// <summary>
        /// 月度限制时,检查报销费用是否大于可以费用
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool CheckRule_MonthTransfer(T_FB_BUDGETACCOUNT account, CheckStates statesNew)
        {
            QueryExpression qeSubject = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", account.T_FB_SUBJECT.SUBJECTID);
            System.Collections.IList list = null;
            AccountObjectType currentAType = (AccountObjectType)account.ACCOUNTOBJECTTYPE.Value;

            QueryExpression qeDept = QueryExpression.Equal(FieldName.OwnerDepartmentID, account.OWNERDEPARTMENTID);
            QueryExpression qeIsMonthLimit = QueryExpression.Equal("T_FB_SUBJECTCOMPANY.ISMONTHLIMIT", "1");
            qeDept.RelatedExpression = qeIsMonthLimit;
            qeIsMonthLimit.RelatedExpression = qeSubject;

            list = GetSubjectDepartment(qeDept);

            if (list.Count > 0)
            {
                if (account.USABLEMONEY.LessThan(0))
                {
                    throw new BudgetAccountBBLException("科目 " + account.T_FB_SUBJECT.SUBJECTNAME + " 的分配金额超出月度预算结余")
                    {
                        AccountItem = account,
                        ErrorCode = AccountErrorCode.OverMonthTransferMoney
                    };
                }
            }
            return true;
        }

        #endregion 科目设置检查

        #region 工资报销
        public List<AccountItem> GetT_FB_SALARYPAYLIST(T_FB_SALARYPAYLIST entity, CheckStates statesNew)
        {
            T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
            QueryExpression qe = QueryExpression.Equal("SUBJECTID", setting.SALARYSUBJECTID);
            T_FB_SUBJECT subject = this.GetEntity<T_FB_SUBJECT>(qe);
            AccountItem item = new AccountItem()
            {
                OwnerCompanyID = entity.OWNERCOMPANYID,
                OwnerDepartmentID = entity.OWNERDEPARTMENTID,
                T_FB_SUBJECT = subject,
                AccountOpertaion = AccountOpertaion.Subtract,
                AccountType = AccountObjectType.Deaprtment,
                BudgetMonth = System.DateTime.Now.Month,
                BudgetYear = System.DateTime.Now.Year,
                moneyActual = entity.PAYMONEY,
                moneyPaid = entity.PAYMONEY,
                moneyUsable = entity.PAYMONEY,

                // 流水帐
                MasterEntity = entity,
                OPERATIONMONEY = entity.PAYMONEY,
                orderDetailID = entity.SALARYPAYLISTID
            };
            return new List<AccountItem>() { item };



        }
        #endregion

        #endregion BudgetAccount

        #region 2.1 更新个人借还款总账

        #region 冲借款，还款，借款

        #region 2012版　, 不扣除可用额度，记账在T_FB_PERSONACCOUNT表
        /// <summary>
        /// 新版
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statesNew"></param>
        /// <returns></returns>
        public bool UpdatePersonAccount(EntityObject entity, CheckStates statesNew, bool isJustCheck)
        {
            // 审核通过的单据才生效
            if (statesNew != CheckStates.Approved)
            {
                return true;
            }

            var listAccount = FetchPersonAccounts(entity);
            listAccount.ForEach(item =>
                {
                    var pAccount = item.GetAccount();

                    if (item.AccountOpertaion == AccountOpertaion.Subtract)
                    {
                        pAccount.SIMPLEBORROWMONEY = pAccount.SIMPLEBORROWMONEY.Subtract(item.SIMPLEBORROWMONEY);
                        pAccount.SPECIALBORROWMONEY = pAccount.SPECIALBORROWMONEY.Subtract(item.SPECIALBORROWMONEY);
                        pAccount.BACKUPBORROWMONEY = pAccount.BACKUPBORROWMONEY.Subtract(item.BACKUPBORROWMONEY);
                    }
                    else
                    {
                        pAccount.SIMPLEBORROWMONEY = pAccount.SIMPLEBORROWMONEY.Add(item.SIMPLEBORROWMONEY);
                        pAccount.SPECIALBORROWMONEY = pAccount.SPECIALBORROWMONEY.Add(item.SPECIALBORROWMONEY);
                        pAccount.BACKUPBORROWMONEY = pAccount.BACKUPBORROWMONEY.Add(item.BACKUPBORROWMONEY);
                    }

                    // 检查规则
                    if (item.CheckRule != null)
                    {
                        bool bRulePass = item.CheckRule(pAccount);
                        if (!bRulePass)
                        {
                            return;
                        }
                    }

                    //记录流水帐
                    var waterFlow = item.CreateWaterFlow(pAccount);
                    waterFlow.TRIGGEREVENT = statesNew.ToString();

                    if (!isJustCheck)
                    {
                        this.Attach(pAccount);
                        this.Attach(waterFlow);
                        this.SaveChanges();
                    }
                });


            return true;
        }

        public List<PersonAccountItem> FetchPersonAccounts(EntityObject entity)
        {
            List<PersonAccountItem> listResult = new List<PersonAccountItem>();

            MethodInfo method = this.GetType().GetMethod("Fetch" + entity.GetType().Name);
            if (method != null)
            {
                object result = method.Invoke(this, new object[] { entity });
                listResult = result as List<PersonAccountItem>;
            }
            return listResult;
        }

        /// <summary>
        /// 个人费用报销，存在冲借款
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<PersonAccountItem> FetchT_FB_CHARGEAPPLYMASTER(T_FB_CHARGEAPPLYMASTER entity)
        {
            var resultList = new List<PersonAccountItem>();
            var result = new PersonAccountItem();

            T_FB_CHARGEAPPLYMASTER master = entity;

            if (!master.T_FB_CHARGEAPPLYREPAYDETAIL.IsLoaded) { master.T_FB_CHARGEAPPLYREPAYDETAIL.Load(); }

            if (master.T_FB_CHARGEAPPLYREPAYDETAIL.Count == 0)
            {
                return resultList;
            }

            result.OwnerID = master.OWNERID;
            result.OwnerCompanyID = master.OWNERCOMPANYID;
            result.OwnerDepartmentID = master.OWNERDEPARTMENTID;
            result.OwnerPostID = master.OWNERPOSTID;


            string detailID = "";
            decimal opmoney = 0;

            master.T_FB_CHARGEAPPLYREPAYDETAIL.ForEach(item =>
            {
                // 1现金还普通借款 2现金还备用金借款 3现金还专项借款
                detailID = item.CHARGEAPPLYREPAYDETAILID;
                opmoney = item.REPAYMONEY;
                if (item.REPAYTYPE == 1)
                {
                    result.SIMPLEBORROWMONEY = item.REPAYMONEY;
                }
                else if (item.REPAYTYPE == 2)
                {
                    result.BACKUPBORROWMONEY = item.REPAYMONEY;
                }
                else if (item.REPAYTYPE == 3)
                {
                    result.SPECIALBORROWMONEY = item.REPAYMONEY;
                }
            });

            result.orderDetailID = detailID;
            result.OPERATIONMONEY = opmoney;
            result.MasterEntity = master;
            result.AccountOpertaion = AccountOpertaion.Subtract;
            result.CheckRule = CheckPersonAccount;
            resultList.Add(result);

            return resultList;
        }

        /// <summary>
        ///　借款
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<PersonAccountItem> FetchT_FB_BORROWAPPLYMASTER(T_FB_BORROWAPPLYMASTER entity)
        {
            var resultList = new List<PersonAccountItem>();
            var result = new PersonAccountItem();

            T_FB_BORROWAPPLYMASTER master = entity;

            result.OwnerID = master.OWNERID;
            result.OwnerCompanyID = master.OWNERCOMPANYID;
            result.OwnerDepartmentID = master.OWNERDEPARTMENTID;
            result.OwnerPostID = master.OWNERPOSTID;


            // 1普通借款 2备用金借款 3专项借款
            string detailID = "";
            decimal opmoney = master.TOTALMONEY;
            if (master.REPAYTYPE == 1)
            {
                result.SIMPLEBORROWMONEY = master.TOTALMONEY;
            }
            else if (master.REPAYTYPE == 2)
            {
                result.BACKUPBORROWMONEY = master.TOTALMONEY;
            }
            else if (master.REPAYTYPE == 3)
            {
                result.SPECIALBORROWMONEY = master.TOTALMONEY;
            }

            result.orderDetailID = detailID;
            result.OPERATIONMONEY = opmoney;
            result.MasterEntity = master;
            result.AccountOpertaion = AccountOpertaion.Add;
            resultList.Add(result);

            return resultList;
        }

        /// <summary>
        /// 个人费用报销，存在冲借款
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<PersonAccountItem> FetchT_FB_REPAYAPPLYMASTER(T_FB_REPAYAPPLYMASTER entity)
        {
            var resultList = new List<PersonAccountItem>();
            var result = new PersonAccountItem();

            T_FB_REPAYAPPLYMASTER master = entity;

            if (!master.T_FB_REPAYAPPLYDETAIL.IsLoaded) { master.T_FB_REPAYAPPLYDETAIL.Load(); }

            if (master.T_FB_REPAYAPPLYDETAIL.Count == 0)
            {
                return resultList;
            }

            result.OwnerID = master.OWNERID;
            result.OwnerCompanyID = master.OWNERCOMPANYID;
            result.OwnerDepartmentID = master.OWNERDEPARTMENTID;
            result.OwnerPostID = master.OWNERPOSTID;


            string detailID = "";
            decimal opmoney = 0;

            master.T_FB_REPAYAPPLYDETAIL.ForEach(item =>
            {
                // 1现金还普通借款 2现金还备用金借款 3现金还专项借款
                detailID = item.REPAYAPPLYDETAILID;
                opmoney = item.REPAYMONEY.Value;
                if (item.REPAYTYPE == 1)
                {
                    result.SIMPLEBORROWMONEY = item.REPAYMONEY;
                }
                else if (item.REPAYTYPE == 2)
                {
                    result.BACKUPBORROWMONEY = item.REPAYMONEY;
                }
                else if (item.REPAYTYPE == 3)
                {
                    result.SPECIALBORROWMONEY = item.REPAYMONEY;
                }
            });

            result.orderDetailID = detailID;
            result.OPERATIONMONEY = opmoney;
            result.MasterEntity = master;
            result.AccountOpertaion = AccountOpertaion.Subtract;
            result.CheckRule = CheckPersonAccount;
            resultList.Add(result);

            return resultList;
        }

        #endregion
        #endregion 冲借款，还款，借款

        #region 个人往来数据检查
        public bool CheckPersonAccount(T_FB_PERSONACCOUNT item)
        {
            if (item.SIMPLEBORROWMONEY.LessThan(0)
                || item.SIMPLEBORROWMONEY.LessThan(0)
                || item.SIMPLEBORROWMONEY.LessThan(0))
            {
                throw new BudgetAccountBBLException(string.Format("还款金额超过了应还款金额。")) { ErrorCode = AccountErrorCode.OverBorrowedMoney };
            }
            return true;
        }
        #endregion

        #endregion

        #region 3. 预算结算
        public void CloseBudget(DateTime date)
        {

            T_FB_SYSTEMSETTINGS settig = SystemBLL.GetSetting(null);
            DateTime? dtOld = settig.LASTCHECKDATE;
            try
            {

                BeginTransaction();
                CloseBudgetPreMonth(date.AddMonths(-1));
                CloseBudgetPreYear(date.AddYears(-1));

                this.SaveChanges();
                // 将当前结账日期记录到系统表中．
                settig.LASTCHECKDATE = DateTime.Now.Date;
                SystemBLL.SaveSetting();


                CommitTransaction();
                //RollbackTransaction();
            }
            catch (Exception ex)
            {
                SystemBLL.Debug("月结产生错误，时间 " + DateTime.Now.ToString() + ex.ToString());
                settig.LASTCHECKDATE = dtOld;
                RollbackTransaction();
                throw ex;
            }
        }

        public void CloseBudgetPreYear(DateTime date)
        {
            QueryExpression qeBudgetYear = QueryExpression.Equal("BUDGETYEAR", date.Year.ToString());
            QueryExpression qeBudgetCom = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Company).ToString());

            qeBudgetYear.RelatedExpression = qeBudgetCom;
            qeBudgetYear.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;
            qeBudgetYear.Include = new string[] { typeof(T_FB_SUBJECT).Name };

            //IQueryable<T_FB_BUDGETACCOUNT> BUDGETACCOUNTs = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeBudgetYear);
            var BUDGETACCOUNTs = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeBudgetYear).ToList();

            // 写入预算结算记录
            BudgetCheck(BUDGETACCOUNTs, null);

            #region 找下一年记录
            qeBudgetYear = QueryExpression.Equal("BUDGETYEAR", date.AddYears(1).Year.ToString());
            qeBudgetCom = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Companynext).ToString());
            qeBudgetYear.RelatedExpression = qeBudgetCom;
            qeBudgetYear.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;
            qeBudgetYear.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            IQueryable<T_FB_BUDGETACCOUNT> BUDGETACCOUNTsNext = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeBudgetYear);
            #endregion



            if (BUDGETACCOUNTsNext.Count() == 0)
            {
                foreach (T_FB_BUDGETACCOUNT account in BUDGETACCOUNTs)
                {

                    account.BUDGETMONEY = 0;
                    account.ACTUALMONEY = 0;
                    account.PAIEDMONEY = 0;
                    account.USABLEMONEY = 0;
                    account.BUDGETMONTH = System.DateTime.Now.Month;
                    account.BUDGETYEAR = System.DateTime.Now.Year;
                    account.UPDATEDATE = System.DateTime.Now;
                    account.UPDATEUSERID = SYSTEM_USER_ID;

                    this.BassBllSave(account, FBEntityState.Modified);
                }
            }
            else
            {
                foreach (T_FB_BUDGETACCOUNT account in BUDGETACCOUNTs)
                {
                    //account.SetValue(FieldName.UpdateDate, System.DateTime.Now);
                    this.DeleteObject(account);
                    //this.Save(account, EntityState.Deleted);
                }

                foreach (T_FB_BUDGETACCOUNT account in BUDGETACCOUNTsNext)
                {

                    account.ACCOUNTOBJECTTYPE = (int)AccountObjectType.Company;
                    account.BUDGETMONTH = System.DateTime.Now.Month;
                    account.BUDGETYEAR = System.DateTime.Now.Year;
                    account.UPDATEDATE = System.DateTime.Now;
                    account.UPDATEUSERID = SYSTEM_USER_ID;
                    //this.Save(account, FBEntityState.Modified);
                    this.Attach(account);
                }

            }


        }

        /// <summary>
        /// 结算当前月份的预算总帐,
        /// 此处结算，没有考虑审核中的单据情况，默认没有这样的单据存在
        /// </summary>
        /// <param name="date">预算月份(当前时间的上一月)</param>
        public void CloseBudgetPreMonth(DateTime date)
        {
            QueryExpression qeBudgetYear = QueryExpression.Equal("BUDGETYEAR", date.Year.ToString());
            QueryExpression qeBudgetDate = QueryExpression.Equal("BUDGETMONTH", date.Month.ToString());

            QueryExpression qeBudgetCom = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Company).ToString());
            qeBudgetCom.Operation = QueryExpression.Operations.NotEqual;

            QueryExpression qeBudgetComnext = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Companynext).ToString());
            qeBudgetComnext.Operation = QueryExpression.Operations.NotEqual;
            qeBudgetCom.RelatedExpression = qeBudgetComnext;

            QueryExpression qeBudgetdeptnext = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Deptnext).ToString());
            qeBudgetdeptnext.Operation = QueryExpression.Operations.NotEqual;
            qeBudgetComnext.RelatedExpression = qeBudgetdeptnext;

            QueryExpression qeBudgetpersonnext = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Personnext).ToString());
            qeBudgetpersonnext.Operation = QueryExpression.Operations.NotEqual;
            qeBudgetdeptnext.RelatedExpression = qeBudgetpersonnext;



            qeBudgetDate.RelatedExpression = qeBudgetCom;
            qeBudgetYear.RelatedExpression = qeBudgetDate;
            qeBudgetYear.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;
            qeBudgetYear.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            // 上个月的预算
            IQueryable<T_FB_BUDGETACCOUNT> accountData = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeBudgetYear);

            DateTime datenext = date.AddMonths(1);
            qeBudgetYear = QueryExpression.Equal("BUDGETYEAR", datenext.Year.ToString());
            qeBudgetDate = QueryExpression.Equal("BUDGETMONTH", datenext.Month.ToString());

            qeBudgetCom = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Deptnext).ToString());
            QueryExpression qePerson = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Personnext).ToString());


            qeBudgetYear.RelatedExpression = qeBudgetDate;
            qeBudgetDate.RelatedExpression = qeBudgetCom;

            qeBudgetCom.RelatedExpression = qePerson;
            qeBudgetCom.RelatedType = QueryExpression.RelationType.Or;

            // 结算月的预算
            qeBudgetYear.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;
            //qeBudgetYear.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            IQueryable<T_FB_BUDGETACCOUNT> accountDatanext = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeBudgetYear);

            //当前已存的结算月的预算(个人和部门）
            var T_FB_BUDGETACCOUNTs = this.GetTable<T_FB_BUDGETACCOUNT>();

            var accountCurrent = (from item in T_FB_BUDGETACCOUNTs
                                  where item.BUDGETMONTH.Value == datenext.Month
                                  && item.BUDGETYEAR.Value == datenext.Year
                                  && (item.ACCOUNTOBJECTTYPE.Value == 2 || item.ACCOUNTOBJECTTYPE.Value == 3)
                                  select item);

            // qeBudgetYear
            IQueryable<T_FB_BUDGETACCOUNT> bYear = GetBudgetYear(date);
            // 写入预算结算记录
            var listAccountData = accountData.ToList();
            var listYear = bYear.ToList();
            BudgetCheck(listAccountData, listYear);

            // 刷新预算总帐
            //假定时间为2013年6月1号
            //accountData上月预算，即计算之前显示能用的费用（数据库里面数据为2013年5月的部门（2）个人（3）的预算费用）
            //accountDatanext为所做的预算（数据库里面数据为2013年6月的部门（21）个人（22）的预算费用）
            //accountCurrent感觉貌似没用，因为这个时候数据库里面应该没有结算时月份的有效的数据（数据库里面数据为2013年6月的部门（2）个人（3）的预算费用）
            RefechBudgetAccount(accountData, accountDatanext, accountCurrent);


        }
        /// <summary>
        /// 得到当年的年度预算
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public IQueryable<T_FB_BUDGETACCOUNT> GetBudgetYear(DateTime date)
        {
            QueryExpression qeBudgetYear = QueryExpression.Equal("BUDGETYEAR", date.Year.ToString());
            QueryExpression qeBudgetCom = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Company).ToString());

            qeBudgetYear.RelatedExpression = qeBudgetCom;
            qeBudgetYear.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;
            qeBudgetYear.Include = new string[] { typeof(T_FB_SUBJECT).Name };

            IQueryable<T_FB_BUDGETACCOUNT> BUDGETACCOUNTs = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeBudgetYear);
            return BUDGETACCOUNTs;
        }

        /// <summary>
        /// 结算当前月预算，写入预算结算记录
        /// 此处结算，没有考虑审核中的单据情况，默认没有这样的单据存在
        /// </summary>
        /// <param name="accountData"></param>
        private void BudgetCheck(List<T_FB_BUDGETACCOUNT> accountData, List<T_FB_BUDGETACCOUNT> bYear)
        {

            Func<string, string, T_FB_BUDGETACCOUNT> GetYearBudgetMoney = (deptID, subjectid) => { return null; };
            // 部门的。需要月结年度预算结余
            if (bYear != null)
            {
                GetYearBudgetMoney = (deptID, subjectid) =>
                    {
                        var yearB = bYear.FirstOrDefault(item =>
                            {
                                return item.OWNERDEPARTMENTID == deptID && item.T_FB_SUBJECT.SUBJECTID == subjectid;
                            });

                        if (yearB != null)
                        {
                            return yearB;
                        }
                        return null;
                    };
            }
            List<T_FB_BUDGETCHECK> listCheck = accountData.CreateList(account =>
            {
                T_FB_BUDGETCHECK check = new T_FB_BUDGETCHECK()
                {
                    ACCOUNTOBJECTTYPE = account.ACCOUNTOBJECTTYPE,
                    ACTUALMONEY = account.PAIEDMONEY, // 实际发生金额
                    BUDGETCHECKDATE = System.DateTime.Now,
                    BUDGETCHECKID = Guid.NewGuid().ToString(),
                    BUDGETMONEY = account.BUDGETMONEY,
                    BUDGETMONTH = account.BUDGETMONTH,
                    BUDGETYEAR = account.BUDGETYEAR,
                    CREATEDATE = System.DateTime.Now,
                    CREATEUSERID = SYSTEM_USER_ID,
                    OWNERCOMPANYID = account.OWNERCOMPANYID,
                    OWNERDEPARTMENTID = account.OWNERDEPARTMENTID,
                    OWNERID = account.OWNERID,
                    OWNERPOSTID = account.OWNERPOSTID,
                    UPDATEDATE = System.DateTime.Now,
                    UPDATEUSERID = SYSTEM_USER_ID,
                    USABLEMONEY = account.USABLEMONEY,
                    YEARBUDGETMONEY = 0,
                    YEARTOTALBUDGETMONEY = 0
                };
                var yearB = GetYearBudgetMoney(account.OWNERDEPARTMENTID, account.T_FB_SUBJECT.SUBJECTID);
                if (yearB != null)
                {
                    check.YEARBUDGETMONEY = yearB.ACTUALMONEY;
                    check.YEARTOTALBUDGETMONEY = yearB.BUDGETMONEY;
                }
                check.T_FB_SUBJECTReference.EntityKey = account.T_FB_SUBJECT.EntityKey;
                return check;
            });
            listCheck.ForEach(item =>
            {
                // this.Save(item, FBEntityState.Added);
                this.Attach(item);
            });
        }

        /// <summary>
        /// 刷新预算总帐（关键函数）假定时间为2013年6月1号
        /// </summary>
        /// <param name="accountData">上月预算，即计算之前显示能用的费用（数据库里面数据为2013年5月的部门（2）个人（3）的预算费用 ）</param>
        /// <param name="accountDatanext">所做的预算（数据库里面数据为2013年6月的部门（21）个人（22）的预算费用）</param>
        /// <param name="accountDataCurrent">感觉貌似没用，因为这个时候数据库里面应该没有结算时月份的有效的数据（数据库里面数据为2013年6月的部门（2）个人（3）的预算费用）</param>
        private void RefechBudgetAccount(IQueryable<T_FB_BUDGETACCOUNT> accountData, IQueryable<T_FB_BUDGETACCOUNT> accountDatanext, IQueryable<T_FB_BUDGETACCOUNT> accountDataCurrent)
        {
            try
            {
                var T_FB_SUBJECTCOMPANYs = this.GetTable<T_FB_SUBJECTCOMPANY>();
                var accountUpdates = from account in accountData
                                     join subjectCom in T_FB_SUBJECTCOMPANYs
                                     on new { account.T_FB_SUBJECT.SUBJECTID, account.OWNERCOMPANYID }
                                     equals
                                        new { subjectCom.T_FB_SUBJECT.SUBJECTID, subjectCom.OWNERCOMPANYID }
                                     select new { BudgetAccount = account, ControlType = subjectCom.CONTROLTYPE, SubjectID = account.T_FB_SUBJECT.SUBJECTID };

                var accountNexts = (from item in accountDatanext
                                    select new AccountCheckItem { T_FB_BUDGETACCOUNT = item, SUBJECTID = item.T_FB_SUBJECT.SUBJECTID });

                var accountCurrents = (from item in accountDataCurrent
                                       select new AccountCheckItem { T_FB_BUDGETACCOUNT = item, SUBJECTID = item.T_FB_SUBJECT.SUBJECTID });

                // 存放已处理的数据
                List<AccountCheckItem> listTemp = new List<AccountCheckItem>();

                #region 处理上个月的总帐数据，或累加下个月或清零
                var localListAccountNext = accountNexts.ToList();//月度预算
                var localaccountUpdates = accountUpdates.ToList();//上月预算
                var localListAccountCur = accountCurrents.ToList();//当前月预算

                WriteLog("开始预算结算,总记录：" + localaccountUpdates.Count);
                foreach (var accountUpdate in localaccountUpdates)
                {
                  
                    ControlType cType = (ControlType)(accountUpdate.ControlType.Value);
                    T_FB_BUDGETACCOUNT account = accountUpdate.BudgetAccount;
                    WriteLog("开始预算结算：科目id:" + accountUpdate.SubjectID + "预算控制类型（1 : 不那跨年使用; 2 不能跨月使用:  ; 3: 无限制 ; 4: 殊年结）：" + accountUpdate.ControlType.Value);
                    switch (cType)
                    {
                        case ControlType.LimitMonth: // 不跨月
                            account.USABLEMONEY = 0;
                            break;
                        case ControlType.LimitYear:  // 不跨年

                            // 如果是跨年了
                            if (System.DateTime.Now.Year > account.BUDGETYEAR)
                            {
                                account.USABLEMONEY = 0;
                                account.ACTUALMONEY = 0;
                            }
                            break;
                        case ControlType.Special:
                            // 跨年了，而且月份达到指定的月份
                            if ((System.DateTime.Now.Year > account.BUDGETYEAR) && (System.DateTime.Now.Month >= SYSTEM_SPECIAL_MONTH))
                            {
                                account.USABLEMONEY = 0;
                                account.ACTUALMONEY = 0;
                            }
                            break;
                    }
                    account.BUDGETMONEY = 0;
                    account.PAIEDMONEY = 0;

                    #region 存在下个月的预算 这里进行查找预算的时候，最好的应该是要根据预算科目，公司ID，部门ID，和岗位ID，因为不加这些条件，那么找出来的记录很有可能是另外一个公司部门的数据，分配到个人的也一样

                    //decimal atype = account.ACCOUNTOBJECTTYPE.Add(19).Value;
                    //var findItems = (from item in localListAccountNext
                    //                 where item.SUBJECTID == accountUpdate.SubjectID
                    //                 && item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE.Value == atype
                    //                 && ((item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Personnext && item.T_FB_BUDGETACCOUNT.OWNERID == account.OWNERID)
                    //                       || (item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Deptnext))
                    //                 select item);

                    decimal atype = account.ACCOUNTOBJECTTYPE.Add(19).Value;
                    string updateCompanyID = account.OWNERCOMPANYID;
                    string updateDeptID = account.OWNERDEPARTMENTID;
                    string updatePostID = account.OWNERPOSTID;

                    var findItems = (from item in localListAccountNext
                                     where item.SUBJECTID == accountUpdate.SubjectID//科目ID
                                     && item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE.Value == atype//预算类型（部门或个人）
                                     && item.T_FB_BUDGETACCOUNT.OWNERCOMPANYID == updateCompanyID//公司ID
                                     && ((item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Personnext && item.T_FB_BUDGETACCOUNT.OWNERID == account.OWNERID && item.T_FB_BUDGETACCOUNT.OWNERPOSTID == updatePostID)//个人预算时要判断岗位ID和个人ID
                                          || (item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Deptnext) && item.T_FB_BUDGETACCOUNT.OWNERDEPARTMENTID == updateDeptID)//部门预算要判断部门ID
                                     select item);

                    var accountNextItem = findItems.FirstOrDefault();
                    WriteLog("找到需要更新的预算总数量为（正确的应该最多一条数据）：" + findItems.Count());
                    //存在，则修改当前总账数目，不存在，则修改类型
                    if (accountNextItem != null)
                    {
                        var accountNext = accountNextItem.T_FB_BUDGETACCOUNT;
                        account.USABLEMONEY = account.USABLEMONEY + accountNext.USABLEMONEY;
                        account.BUDGETMONEY = accountNext.BUDGETMONEY;
                        account.PAIEDMONEY = accountNext.PAIEDMONEY;
                        account.ACTUALMONEY = account.ACTUALMONEY + accountNext.ACTUALMONEY;
                        this.DeleteObject(accountNext);
                        localListAccountNext.Remove(accountNextItem);
                    }
                    #endregion

                    account.BUDGETMONTH = System.DateTime.Now.Month;
                    account.BUDGETYEAR = System.DateTime.Now.Year;
                    account.UPDATEDATE = System.DateTime.Now;
                    account.UPDATEUSERID = SYSTEM_USER_ID;

                    //  this.Save(account, FBEntityState.Modified);
                    this.Attach(account);
                    listTemp.Add(new AccountCheckItem() { T_FB_BUDGETACCOUNT = account, SUBJECTID = accountUpdate.SubjectID });

                }
                #endregion

                #region 处理下个月的预算数据，变为当前月的标识，这里还存在的数据则为新增加的预算信息，如果上面更新上月预算查找到错误数据删掉了这里的新增的部门或个人预算，那么预算将没有新的预算费用
                foreach (var item in localListAccountNext)
                {
                    var accountNext = item.T_FB_BUDGETACCOUNT;
                    accountNext.ACCOUNTOBJECTTYPE = accountNext.ACCOUNTOBJECTTYPE.Add(-19);
                    accountNext.UPDATEDATE = System.DateTime.Now;
                    accountNext.UPDATEUSERID = SYSTEM_USER_ID;
                    // this.Save(accountNext, FBEntityState.Modified);
                    this.Attach(accountNext);
                    listTemp.Add(item);
                }
                #endregion

                #region 处理已存在当前月的数据，这里localListAccountCur为月结时的当月预算，一般没有值
                foreach (var accountUpdate in listTemp)
                {
                    //var accountCur = item.T_FB_BUDGETACCOUNT;
                    //var itemFind = listTemp.FirstOrDefault(itemTemp =>
                    //    {
                    //        itemTemp.
                    //    });

                    var account = accountUpdate.T_FB_BUDGETACCOUNT;
                    #region 存在下个月的预算
                    //var findItems = (from item in localListAccountCur
                    //                where item.SUBJECTID == accountUpdate.SUBJECTID
                    //                && item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE.Value == account.ACCOUNTOBJECTTYPE.Value
                    //                && ((item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person && item.T_FB_BUDGETACCOUNT.OWNERID == account.OWNERID)
                    //                      || (item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Deaprtment))
                    //                select item);

                    var findItems = (from item in localListAccountCur
                                     where item.SUBJECTID == accountUpdate.SUBJECTID
                                     && item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE.Value == account.ACCOUNTOBJECTTYPE.Value
                                     && item.T_FB_BUDGETACCOUNT.OWNERCOMPANYID == account.OWNERCOMPANYID
                                     && ((item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person && item.T_FB_BUDGETACCOUNT.OWNERID == account.OWNERID && item.T_FB_BUDGETACCOUNT.OWNERPOSTID == account.OWNERPOSTID)
                                           || (item.T_FB_BUDGETACCOUNT.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Deaprtment) && item.T_FB_BUDGETACCOUNT.OWNERDEPARTMENTID == account.OWNERDEPARTMENTID)
                                     select item);

                    var accountNextItem = findItems.FirstOrDefault();

                    //存在，则修改当前总账数目，不存在，则修改类型
                    if (accountNextItem != null)
                    {
                        var accountNext = accountNextItem.T_FB_BUDGETACCOUNT;
                        account.USABLEMONEY = account.USABLEMONEY + accountNext.USABLEMONEY;
                        account.BUDGETMONEY = accountNext.BUDGETMONEY;
                        account.PAIEDMONEY = accountNext.PAIEDMONEY;
                        account.ACTUALMONEY = account.ACTUALMONEY + accountNext.ACTUALMONEY;
                        this.DeleteObject(accountNext);
                        localListAccountCur.Remove(accountNextItem);
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                Tracer.Debug("月结产生错误，时间 " + DateTime.Now.ToString() + ex.Message);
            }

          
        }

        public void WriteLog(string msg)
        {
            try
            {
                string value = ConfigurationManager.AppSettings["DebugMode"];
                if (value == "true")
                {
                    Tracer.Debug(msg);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }

        }

        #endregion

        public class AccountCheckItem
        {
            public T_FB_BUDGETACCOUNT T_FB_BUDGETACCOUNT { get; set; }
            public string SUBJECTID { get; set; }
        }

        #region 4. 查询结算记录
        /// <summary>
        /// 获取结算数据，如果是当月的，则从总账中统计
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<T_FB_BUDGETCHECK> GetBudgetCheck(QueryExpression qe)
        {
            List<T_FB_BUDGETCHECK> listDeptCheck = new List<T_FB_BUDGETCHECK>();

            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID); // 部门ID
            QueryExpression qeCheckMonth = qe.GetQueryExpression("BUDGETARYMONTH"); // 结算月份
            DateTime dateCheck = DateTime.Parse(qeCheckMonth.PropertyValue);
            QueryExpression qeYear = QueryExpression.Equal("BUDGETYEAR", dateCheck.Year.ToString()); // 结算年份
            QueryExpression qeMonth = QueryExpression.Equal("BUDGETMONTH", dateCheck.Month.ToString()); // 结算年份
            qeDept.RelatedExpression = qeYear;
            qeYear.RelatedExpression = null;
            //   qeDept.Include = new string[] { typeof(T_FB_SUBJECT).Name };

            // 如果是查询当前月份的结算数据，就从预算总帐表中汇总
            if (dateCheck.Year == DateTime.Now.Year && dateCheck.Month == DateTime.Now.Month)
            {

                var baData = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeDept);

                var dataSubjectCompany = this.GetTable<T_FB_SUBJECTCOMPANY>();
                var dataYear = (from item in baData
                                where item.ACCOUNTOBJECTTYPE.Value == (int)AccountObjectType.Company
                                select item);
                var dataMonth = (from item in baData
                                 where item.ACCOUNTOBJECTTYPE.Value != (int)AccountObjectType.Company
                                && item.BUDGETMONTH.Value == dateCheck.Month
                                 select item);
                //var dataPerson = (from item in baData
                //                  where item.ACCOUNTOBJECTTYPE.Value =
                var dataCheck = from itemMonth in dataMonth
                                from itemYear in dataYear
                                from itemSubjectCompany in dataSubjectCompany
                                where itemMonth.T_FB_SUBJECT.SUBJECTID == itemYear.T_FB_SUBJECT.SUBJECTID
                                   && itemMonth.T_FB_SUBJECT != null
                                   && itemMonth.OWNERCOMPANYID == itemSubjectCompany.OWNERCOMPANYID
                                select new { itemMonth, itemYear.ACTUALMONEY, itemSubjectCompany.CONTROLTYPE };


                listDeptCheck = dataCheck.ToList().CreateList(itemData =>
                    {

                        T_FB_BUDGETCHECK temp = new T_FB_BUDGETCHECK();
                        T_FB_BUDGETACCOUNT item = itemData.itemMonth;
                        temp.USABLEMONEY = item.USABLEMONEY;
                        temp.ACTUALMONEY = item.PAIEDMONEY;
                        temp.BUDGETMONEY = item.BUDGETMONEY;
                        temp.ACCOUNTOBJECTTYPE = item.ACCOUNTOBJECTTYPE;
                        if (!item.T_FB_SUBJECTReference.IsLoaded)
                        {
                            item.T_FB_SUBJECTReference.Load();
                        }
                        temp.T_FB_SUBJECT = item.T_FB_SUBJECT;
                        temp.YEARBUDGETMONEY = itemData.ACTUALMONEY;
                        temp.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                        temp.OWNERCOMPANYID = item.OWNERCOMPANYID;
                        temp.OWNERID = item.OWNERID;
                        temp.OWNERPOSTID = item.OWNERPOSTID;


                        ControlType curType = (ControlType)Convert.ToInt32(itemData.CONTROLTYPE);
                        if (curType == ControlType.LimitMonth)
                        {
                            temp.USABLEMONEY = 0;
                        }
                        else if (curType == ControlType.LimitYear && !itemData.itemMonth.BUDGETYEAR.Equal(dateCheck.AddMonths(1).Year))
                        {
                            temp.USABLEMONEY = 0;
                        }


                        return temp;
                    });
            }
            else
            {
                qeYear.RelatedExpression = qeMonth;
                listDeptCheck = this.GetEntities<T_FB_BUDGETCHECK>(qeDept);


            }
            return listDeptCheck;
        }
        #endregion

        #region 5. 扣减工资预算额度
        public bool UpdateSalaryBudget(List<FBEntity> list)
        {
            try
            {
                BeginTransaction();
                list.ForEach(item =>
                    {
                        this.BassBllSave(item.Entity, item.FBEntityState);
                        this.UpdateAccount(item, CheckStates.Approved);

                    });
                CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                RollbackTransaction();
            }
            return false;

        }
        #endregion


        #region 2.	查询实体集合的操作方法

        #region 日常开销
        public List<FBEntity> QueryT_FB_CHARGEAPPLYDETAIL(QueryExpression qe)
        {
            QueryExpression qeBorrowID = qe.GetQueryExpression("T_FB_BORROWAPPLYMASTER.BORROWAPPLYMASTERID");
            string msg = string.Empty;
            List<T_FB_BUDGETACCOUNT> list = GetBUDGETACCOUNTPerson(qe, ref msg);

            if (qeBorrowID != null) // 从借款单出获取明细
            {
                return QueryT_FB_CHARGEAPPLYDETAIL2(qeBorrowID, list);
            }

            List<T_FB_CHARGEAPPLYDETAIL> listResult = new List<T_FB_CHARGEAPPLYDETAIL>();
            list.ForEach(p =>
            {
                T_FB_CHARGEAPPLYDETAIL t = new T_FB_CHARGEAPPLYDETAIL();
                t.CHARGEAPPLYDETAILID = Guid.NewGuid().ToString();
                t.T_FB_SUBJECT = p.T_FB_SUBJECT;

                t.USABLEMONEY = p.USABLEMONEY;
                t.CHARGEMONEY = 0;
                if (p.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person)
                {
                    t.CHARGETYPE = (int)ChargeType.Person;
                }
                else
                {
                    t.CHARGETYPE = (int)ChargeType.Commmon;
                }

                listResult.Add(t);
            });

            T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
            string tranverlSubjectid = setting.TRANVERLSUBJECTID;

            listResult.RemoveAll(item =>
            {
                return item.T_FB_SUBJECT.SUBJECTID == tranverlSubjectid;
            });

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
            });
            return listResult.ToFBEntityList<T_FB_CHARGEAPPLYDETAIL>();
        }

        public List<FBEntity> QueryT_FB_CHARGEAPPLYDETAIL2(QueryExpression qe, List<T_FB_BUDGETACCOUNT> listAccount)
        {
            qe.Include = new string[] { "T_FB_SUBJECT" };

            List<T_FB_BORROWAPPLYDETAIL> listBorrow = GetEntities<T_FB_BORROWAPPLYDETAIL>(qe);

            List<T_FB_CHARGEAPPLYDETAIL> listResult = listBorrow.CreateList(item =>
            {
                T_FB_CHARGEAPPLYDETAIL detail = new T_FB_CHARGEAPPLYDETAIL();
                detail.CHARGEAPPLYDETAILID = Guid.NewGuid().ToString();
                detail.T_FB_BORROWAPPLYDETAIL = item;
                detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                detail.CHARGETYPE = item.CHARGETYPE;
                detail.CHARGEMONEY = 0;
                T_FB_BUDGETACCOUNT accountPerson = listAccount.FirstOrDefault(account =>
                {
                    bool b1 = account.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID;
                    bool b2 = false;
                    if ((item.CHARGETYPE == 1 && account.ACCOUNTOBJECTTYPE == 3) || (item.CHARGETYPE == 2 && account.ACCOUNTOBJECTTYPE == 2))
                    {
                        b2 = true;
                    }
                    return b1 && b2;
                });
                if (accountPerson == null)
                {
                    detail.USABLEMONEY = item.UNREPAYMONEY;
                }
                else
                {
                    detail.USABLEMONEY = item.UNREPAYMONEY + accountPerson.USABLEMONEY;
                }
                return detail;
            });

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
            });

            return listResult.ToFBEntityList();
        }

        public List<FBEntity> QueryT_FB_BORROWAPPLYDETAIL(QueryExpression qe)
        {
            string msg = string.Empty;
            List<T_FB_BUDGETACCOUNT> list = GetBUDGETACCOUNTPerson(qe, ref msg);

            List<T_FB_BORROWAPPLYDETAIL> listResult = new List<T_FB_BORROWAPPLYDETAIL>();
            list.ForEach(p =>
            {
                T_FB_BORROWAPPLYDETAIL t = new T_FB_BORROWAPPLYDETAIL();
                t.T_FB_SUBJECT = p.T_FB_SUBJECT;
                t.BORROWAPPLYDETAILID = Guid.NewGuid().ToString();
                t.USABLEMONEY = p.USABLEMONEY;
                t.BORROWMONEY = 0;


                if (p.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person)
                {
                    t.CHARGETYPE = (int)ChargeType.Person;
                }
                else
                {
                    t.CHARGETYPE = (int)ChargeType.Commmon;
                }

                listResult.Add(t);

            });

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();

            });
            return listResult.ToFBEntityList<T_FB_BORROWAPPLYDETAIL>();
        }

        public List<FBEntity> QueryT_FB_REPAYAPPLYDETAIL(QueryExpression qe)
        {

            QueryExpression qeBorrowID = qe.GetQueryExpression("T_FB_BORROWAPPLYMASTER.BORROWAPPLYMASTERID");
            string msg = string.Empty;
            List<T_FB_BUDGETACCOUNT> list = GetBUDGETACCOUNTPerson(qe, ref msg);

            if (qeBorrowID != null) // 从借款单出获取明细
            {
                return QueryT_FB_REPAYAPPLYDETAIL2(qeBorrowID);
            }

            List<T_FB_REPAYAPPLYDETAIL> listResult = new List<T_FB_REPAYAPPLYDETAIL>();
            list.ForEach(p =>
            {
                T_FB_REPAYAPPLYDETAIL t = new T_FB_REPAYAPPLYDETAIL();

                t.T_FB_SUBJECT = p.T_FB_SUBJECT;
                t.REPAYAPPLYDETAILID = Guid.NewGuid().ToString();
                // t.BORROWMONEY = p.USABLEMONEY;
                t.REPAYMONEY = 0;
                listResult.Add(t);

                if (p.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person)
                {
                    t.CHARGETYPE = (int)ChargeType.Person;
                }
                else
                {
                    t.CHARGETYPE = (int)ChargeType.Commmon;
                }

                listResult.Add(t);
            });

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
            });


            return listResult.ToFBEntityList<T_FB_REPAYAPPLYDETAIL>();
        }

        public List<FBEntity> QueryT_FB_REPAYAPPLYDETAIL2(QueryExpression qe)
        {
            qe.Include = new string[] { "T_FB_SUBJECT" };

            List<T_FB_BORROWAPPLYDETAIL> listBorrow = GetEntities<T_FB_BORROWAPPLYDETAIL>(qe);

            List<T_FB_REPAYAPPLYDETAIL> listResult = listBorrow.CreateList(item =>
            {
                T_FB_REPAYAPPLYDETAIL detail = new T_FB_REPAYAPPLYDETAIL();
                detail.REPAYAPPLYDETAILID = Guid.NewGuid().ToString();
                detail.T_FB_BORROWAPPLYDETAIL = item;
                detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                detail.CHARGETYPE = item.CHARGETYPE;
                detail.REPAYMONEY = 0;
                // detail.BORROWMONEY = item.BORROWMONEY;
                return detail;
            });

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
            });

            return listResult.ToFBEntityList();
        }

        public List<FBEntity> QueryT_FB_TRAVELEXPAPPLYDETAIL(QueryExpression qe)
        {
            QueryExpression qeBorrowID = qe.GetQueryExpression("T_FB_BORROWAPPLYMASTER.BORROWAPPLYMASTERID");
            string msg = string.Empty;
            List<T_FB_BUDGETACCOUNT> list = GetBUDGETACCOUNTPerson(qe, ref msg);

            if (qeBorrowID != null) // 从借款单出获取明细
            {
                return QueryT_FB_TRAVELEXPAPPLYDETAIL2(qeBorrowID, list);
            }

            QueryExpression qeOwner = qe.GetQueryExpression(FieldName.OwnerID);
            List<FBEntity> listSystem = QueryT_FB_SYSTEMSETTINGS(null);

            FBEntity entitySystem = listSystem.FirstOrDefault();
            if (entitySystem == null)
            {
                return new List<FBEntity>();
            }
            string subjectID = (entitySystem.Entity as T_FB_SYSTEMSETTINGS).TRANVERLSUBJECTID;

            QueryExpression qe1 = QueryExpression.Equal("SUBJECTID", subjectID);
            qe1.QueryType = "T_FB_SUBJECT";


            T_FB_SUBJECT entity = GetEntity<T_FB_SUBJECT>(qe1);


            T_FB_BUDGETACCOUNT budgetAccountDept = list.FirstOrDefault(item =>
            {
                AccountObjectType aType = (AccountObjectType)(int)(item.ACCOUNTOBJECTTYPE.Value);
                return item.T_FB_SUBJECT.SUBJECTID == subjectID && aType == AccountObjectType.Deaprtment;
            });
            T_FB_BUDGETACCOUNT budgetAccountPerson = list.FirstOrDefault(item =>
            {
                AccountObjectType aType = (AccountObjectType)(int)(item.ACCOUNTOBJECTTYPE.Value);
                return item.T_FB_SUBJECT.SUBJECTID == subjectID && aType == AccountObjectType.Person;
            });

            List<T_FB_TRAVELEXPAPPLYDETAIL> listResult = new List<T_FB_TRAVELEXPAPPLYDETAIL>();

            if (budgetAccountDept != null && budgetAccountDept.USABLEMONEY.BiggerThan(0))
            {
                T_FB_TRAVELEXPAPPLYDETAIL t = new T_FB_TRAVELEXPAPPLYDETAIL();
                t.T_FB_SUBJECT = entity as T_FB_SUBJECT;
                t.USABLEMONEY = budgetAccountDept.USABLEMONEY;
                t.TRAVELEXPAPPLYDETAILID = Guid.NewGuid().ToString();
                t.MONTH = DateTime.Now.Month;
                t.DAY = DateTime.Now.Day;
                t.CREATEDATE = System.DateTime.Now;
                t.UPDATEDATE = t.CREATEDATE;
                t.CHARGETYPE = (int)ChargeType.Commmon;
                if (qeOwner != null)
                {
                    t.CREATEUSERID = qeOwner.PropertyValue;
                    t.UPDATEUSERID = qeOwner.PropertyValue;
                }
                listResult.Add(t);
            }

            if (budgetAccountPerson != null && budgetAccountPerson.USABLEMONEY.BiggerThan(0))
            {

                T_FB_TRAVELEXPAPPLYDETAIL t = new T_FB_TRAVELEXPAPPLYDETAIL();
                t.T_FB_SUBJECT = entity as T_FB_SUBJECT;
                t.USABLEMONEY = budgetAccountPerson.USABLEMONEY;
                t.TRAVELEXPAPPLYDETAILID = Guid.NewGuid().ToString();
                t.MONTH = DateTime.Now.Month;
                t.DAY = DateTime.Now.Day;
                t.CREATEDATE = System.DateTime.Now;
                t.UPDATEDATE = t.CREATEDATE;
                t.CHARGETYPE = (int)ChargeType.Person;
                if (qeOwner != null)
                {
                    t.CREATEUSERID = qeOwner.PropertyValue;
                    t.UPDATEUSERID = qeOwner.PropertyValue;
                }
                listResult.Add(t);
            }

            if (listResult.Count == 0)
            {
                T_FB_TRAVELEXPAPPLYDETAIL t = new T_FB_TRAVELEXPAPPLYDETAIL();
                t.T_FB_SUBJECT = entity as T_FB_SUBJECT;
                t.USABLEMONEY = 0;
                t.TRAVELEXPAPPLYDETAILID = Guid.NewGuid().ToString();
                t.MONTH = DateTime.Now.Month;
                t.DAY = DateTime.Now.Day;
                t.CREATEDATE = System.DateTime.Now;
                t.UPDATEDATE = t.CREATEDATE;
                t.CHARGETYPE = (int)ChargeType.Commmon;
                if (qeOwner != null)
                {
                    t.CREATEUSERID = qeOwner.PropertyValue;
                    t.UPDATEUSERID = qeOwner.PropertyValue;
                }
                listResult.Add(t);
            }

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
            });

            return listResult.ToFBEntityList<T_FB_TRAVELEXPAPPLYDETAIL>();


        }

        public List<FBEntity> QueryT_FB_TRAVELEXPAPPLYDETAIL2(QueryExpression qe, List<T_FB_BUDGETACCOUNT> listAccount)
        {
            List<FBEntity> listSystem = QueryT_FB_SYSTEMSETTINGS(null);
            FBEntity entitySystem = listSystem.FirstOrDefault();
            string subjectID = (entitySystem.Entity as T_FB_SYSTEMSETTINGS).TRANVERLSUBJECTID;

            QueryExpression qeTravel = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", subjectID);
            qeTravel.RelatedExpression = qe;
            qeTravel.Include = new string[] { "T_FB_SUBJECT" };

            List<T_FB_BORROWAPPLYDETAIL> listBorrow = GetEntities<T_FB_BORROWAPPLYDETAIL>(qeTravel);

            List<T_FB_TRAVELEXPAPPLYDETAIL> listResult = listBorrow.CreateList(item =>
            {
                T_FB_TRAVELEXPAPPLYDETAIL detail = new T_FB_TRAVELEXPAPPLYDETAIL();
                detail.TRAVELEXPAPPLYDETAILID = Guid.NewGuid().ToString();
                detail.T_FB_BORROWAPPLYDETAIL = item;
                detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                detail.CHARGETYPE = item.CHARGETYPE;
                detail.TOTALCHARGE = 0;
                T_FB_BUDGETACCOUNT accountPerson = listAccount.FirstOrDefault(account =>
                {
                    return account.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID;
                });

                if (accountPerson == null)
                {
                    detail.USABLEMONEY = item.UNREPAYMONEY;
                }
                else
                {
                    detail.USABLEMONEY = item.UNREPAYMONEY + accountPerson.USABLEMONEY;
                }
                return detail;
            });

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
            });

            return listResult.ToFBEntityList();
        }

        #endregion

        #region 年度
        /// <summary>
        /// 公司预算明细
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_COMPANYBUDGETAPPLYDETAIL(QueryExpression qe)
        {

            List<FBEntity> listResult = new List<FBEntity>();
            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID); // 部门ID

            qeDept.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            var listSubjectDept = GetSubjectDepartment(qeDept);


            QueryExpression qeYear = QueryExpression.Equal("ACCOUNTOBJECTTYPE", Convert.ToString((int)AccountObjectType.Company));

            var sumCurrent = (from itemCur in InnerGetEntities<T_FB_BUDGETACCOUNT>(qe)
                              group itemCur by new { itemCur.OWNERDEPARTMENTID, itemCur.T_FB_SUBJECT.SUBJECTID } into itemCurGroup
                              select new { itemCurGroup.Key.OWNERDEPARTMENTID, itemCurGroup.Key.SUBJECTID, SumMoney = itemCurGroup.Sum(item => item.PAIEDMONEY) }).ToList();

            var sumHistory = (from itemCur in InnerGetEntities<T_FB_BUDGETCHECK>(qe)
                              group itemCur by new { OWNERDEPARTMENTID = itemCur.OWNERDEPARTMENTID, SUBJECTID = itemCur.T_FB_SUBJECT.SUBJECTID } into itemCurGroup
                              select new { itemCurGroup.Key.OWNERDEPARTMENTID, itemCurGroup.Key.SUBJECTID, SumMoney = itemCurGroup.Sum(item => item.ACTUALMONEY) }).ToList();




            listSubjectDept.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
            });

            listSubjectDept.ForEach(subjectDept =>
            {
                T_FB_COMPANYBUDGETAPPLYDETAIL ComDetail = new T_FB_COMPANYBUDGETAPPLYDETAIL();
                ComDetail.T_FB_SUBJECT = subjectDept.T_FB_SUBJECT;
                ComDetail.AUDITBUDGETMONEY = 0;
                ComDetail.BUDGETMONEY = 0;
                ComDetail.COMPANYBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                //ComDetail.CREATEDATE = DateTime.Now;
                //ComDetail.CREATEUSERID=
                ComDetail.DISTRIBUTEDMONDEY = 0;
                ComDetail.BUDGETMONEY = 0;

                var u1 = sumCurrent.FirstOrDefault(item =>
                {
                    return item.SUBJECTID == subjectDept.T_FB_SUBJECT.SUBJECTID
                        && item.OWNERDEPARTMENTID == subjectDept.OWNERDEPARTMENTID;
                });

                var u2 = sumHistory.FirstOrDefault(item =>
                {
                    return item.SUBJECTID == subjectDept.T_FB_SUBJECT.SUBJECTID
                        && item.OWNERDEPARTMENTID == subjectDept.OWNERDEPARTMENTID;
                });

                if (u1 != null)
                {
                    ComDetail.LASTBUDGETMONEY += u1.SumMoney;
                }

                if (u2 != null)
                {
                    ComDetail.LASTBUDGETMONEY += u2.SumMoney;
                }

                FBEntity fbComDetail = ComDetail.ToFBEntity();
                listResult.Add(fbComDetail);
            });



            return listResult;

        }

        //public void SumCompanyCharge(QueryExpression qe)
        //{
        //    BaseBLL bll = new BaseBLL();
        //    QueryExpression qe = QueryExpression.Equal("BUDGETYEAR", budgetYear);
        //    qe.RelatedExpression = QueryExpression.Equal(FieldName.OwnerDepartmentID, deptID);

        //    var currentSum = bll.InnerGetEntities<T_FB_BUDGETACCOUNT>(qe);
        //    currentSum.GroupBy(item => 
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_COMPANYBUDGETMODDETAIL(QueryExpression qe)
        {

            List<FBEntity> listResult = new List<FBEntity>();
            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID); // 部门ID
            qeDept.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            List<T_FB_SUBJECTDEPTMENT> listSubjectDept = GetSubjectDepartment(qeDept);

            listSubjectDept.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
            });

            QueryExpression qeYear = QueryExpression.Equal("ACCOUNTOBJECTTYPE", Convert.ToString((int)AccountObjectType.Company));
            qeYear.RelatedExpression = qeDept;
            var currentAccount = InnerGetEntities<T_FB_BUDGETACCOUNT>(qeYear).ToList();

            listSubjectDept.ForEach(subjectDept =>
            {
                T_FB_COMPANYBUDGETMODDETAIL ComDetail = new T_FB_COMPANYBUDGETMODDETAIL();
                ComDetail.T_FB_SUBJECT = subjectDept.T_FB_SUBJECT;
                ComDetail.AUDITBUDGETMONEY = 0;
                ComDetail.BUDGETMONEY = 0;
                ComDetail.COMPANYBUDGETMODDETAILID = Guid.NewGuid().ToString();
                ComDetail.T_FB_SUBJECT = subjectDept.T_FB_SUBJECT;

                ComDetail.USABLEMONEY = 0;
                var find = currentAccount.FirstOrDefault(item =>
                {

                    return (item.T_FB_SUBJECT != null && item.T_FB_SUBJECT.SUBJECTID == subjectDept.T_FB_SUBJECT.SUBJECTID)
                        && item.OWNERDEPARTMENTID == subjectDept.OWNERDEPARTMENTID;
                });

                if (find != null)
                {
                    //ComDetail.USABLEMONEY = find.BUDGETMONEY;
                    ComDetail.USABLEMONEY = find.USABLEMONEY;
                }

                ComDetail.OWNERDEPARTMENTID = qeDept.PropertyValue;
                FBEntity fbComDetail = ComDetail.ToFBEntity();
                fbComDetail.FBEntityState = FBEntityState.Added;
                listResult.Add(fbComDetail);
            });
            return listResult;

        }

        /// <summary>
        /// 年度预算调拨
        /// </summary>
        /// <remarks>
        ///     1. 从当前预算总帐中找出相应可以于年度预算的金额 listBudgetAccount
        ///     2.从部门科目中找出所有可用科目 listSubjectCompany
        /// </remarks>
        /// <param name="qe"></param>
        /// <returns></returns>
        ///                        
        public List<FBEntity> QueryT_FB_COMPANYTRANSFERDETAIL(QueryExpression qe)
        {
            QueryExpression qeCompany = qe.GetQueryExpression(FieldName.OwnerCompanyID);
            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID);

            qeDept.Include = new string[] { "T_FB_SUBJECT" };
            // 1 从当前预算总帐中找出相应可以于年度预算的金额 listBudgetAccount
            List<T_FB_BUDGETACCOUNT> listBudgetAccount = GetBUDGETACCOUNT(qeDept, AccountObjectType.Company);

            listBudgetAccount = listBudgetAccount.OrderBy(item => item.T_FB_SUBJECT.SUBJECTCODE).ToList();



            List<T_FB_COMPANYTRANSFERDETAIL> listResult = listBudgetAccount.CreateList(item =>
            {
                T_FB_COMPANYTRANSFERDETAIL detail = new T_FB_COMPANYTRANSFERDETAIL();
                detail.COMPANYTRANSFERDETAILID = Guid.NewGuid().ToString();
                detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                detail.USABLEMONEY = item.USABLEMONEY;
                detail.TRANSFERMONEY = 0;
                return detail;
            });

            listResult.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
            });
            return listResult.ToFBEntityList();
        }
        #endregion

        #region 部门

        /// <summary>
        /// 部门预算明细
        ///     
        /// </summary>
        /// <remarks>
        ///     1. 汇总个人预算申请单明细 listPersonBudetDetail
        ///     2. 从部门科目中找出所有可用科目 listSubjectDept
        ///     3. 从预算结算中找出结算数据 listDeptCheck, 如果是预算月份尚未结算，则即时统计当前预算帐数据
        ///     4. 从当前预算总帐中找出相应可以于月度预算的金额 listBudgetAccount
        ///     5. 构成预算申请明细 listResult = ListPersonBudetDetail & ListSubjectDept & ListDeptCheck
        ///         1. 预算可用金额
        ///             1. 不受年度预算控制时，可用金额为decimal.max
        ///             ( A : 年度预算可用额度  , B: 月度预算最大额度)
        ///             2. 受年度预算控制时 可用金额 = 
        ///                 1. 如果 B ＝＝ 0 , 可用额度 = A
        ///                 1. 如果 B > 0, 再如果 B 小于 A 可用额度 B 否则取 A
        ///              
        /// </remarks>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_DEPTBUDGETAPPLYDETAIL(QueryExpression qe)
        {


            // 条件
            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID); // 部门ID
            QueryExpression qeBudgetMonth = qe.GetQueryExpression("BUDGETARYMONTH"); // 预算月份
            DateTime dateBudget = DateTime.Parse(qeBudgetMonth.PropertyValue);
            QueryExpression qeBudgetYear = QueryExpression.Equal("BUDGETYEAR", dateBudget.Year.ToString()); // 预算年份

            // 2 从部门科目中找出所有可用科目 listSubjectDept
            qeDept.RelatedExpression = null;
            List<T_FB_SUBJECTDEPTMENT> listSubjectDept = GetSubjectDepartment(qeDept);

            //移除活动经费科目
            string MoneyAssign = SystemBLL.GetSetting(null).MONEYASSIGNSUBJECTID;
            if (MoneyAssign != string.Empty)
            {
                T_FB_SUBJECTDEPTMENT itemmoney = listSubjectDept.Where(item => item.T_FB_SUBJECT.SUBJECTID == MoneyAssign).FirstOrDefault();
                if (itemmoney != null)
                    listSubjectDept.Remove(itemmoney);
            }

            // 4 从当前预算总帐中找出相应可以于月度预算的金额 listBudgetAccount
            qeDept.RelatedExpression = qeBudgetYear;
            var q1 = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Company).ToString());
            var q2 = QueryExpression.Equal(FieldName_AccountObjectType, ((int)AccountObjectType.Companynext).ToString());

            q1.RelatedExpression = q2;
            q1.RelatedType = QueryExpression.RelationType.Or;
            qeBudgetYear.RelatedExpression = q1;
            qeBudgetYear.RelatedType = QueryExpression.RelationType.And;
            qeDept.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            List<T_FB_BUDGETACCOUNT> listBudgetAccount = GetEntities<T_FB_BUDGETACCOUNT>(qeDept);

            // 3 从预算结算中找出结算数据 listDeptCheck, 如果是预算月份尚未结算，则即时统计当前预算帐数据
            DateTime dateCheck = dateBudget.AddMonths(-1);
            qeDept.RelatedExpression = QueryExpression.Equal("BUDGETARYMONTH", dateCheck.ToString("yyyy-MM-dd"));

            List<T_FB_BUDGETCHECK> listDeptCheck = GetBudgetCheck(qeDept);

            List<FBEntity> listResult = new List<FBEntity>();

            // 构造个人预算数据
            #region beyond

            List<T_FB_PERSONBUDGETAPPLYDETAIL> listPersonBudgetDetail = new List<T_FB_PERSONBUDGETAPPLYDETAIL>();
            listSubjectDept.ForEach(subjectDept =>
            {
                #region
                string subjectID = subjectDept.T_FB_SUBJECT.SUBJECTID;
                QueryExpression qa = QueryExpression.Equal("ISPERSON", "1");
                QueryExpression qb = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", subjectID);
                QueryExpression qc = QueryExpression.Equal("T_FB_SUBJECTDEPTMENT.OWNERDEPARTMENTID", qeDept.PropertyValue);

                qa.RelatedExpression = qb;
                qb.RelatedExpression = qc;
                qa.QueryType = typeof(T_FB_SUBJECTPOST).Name;

                //科目对应的岗位
                List<T_FB_SUBJECTPOST> listSubjectPost = GetSubjectPost(qa);

                listSubjectPost.ForEach(subjectpost =>
                {
                    #region
                    SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient psc = new SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();
                    SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEEPOST[] emps = psc.GetEmployeePostByPostID(subjectpost.OWNERPOSTID);
                    if (emps != null)
                    {
                        List<SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEEPOST> listEmp = emps.ToList<SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEEPOST>();
                        listEmp.ForEach(emppost =>
                        {
                            #region
                            T_FB_PERSONBUDGETAPPLYDETAIL persondetail = new T_FB_PERSONBUDGETAPPLYDETAIL();
                            persondetail.PERSONBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                            persondetail.BUDGETMONEY = 0;
                            persondetail.CREATEDATE = DateTime.Now;
                            persondetail.CREATEUSERID = "";
                            persondetail.CREATEUSERNAME = "";
                            persondetail.OWNERNAME = emppost.T_HR_EMPLOYEE.EMPLOYEECNAME;
                            persondetail.OWNERID = emppost.T_HR_EMPLOYEE.EMPLOYEEID;
                            persondetail.OWNERPOSTID = subjectpost.OWNERPOSTID;
                            if (emppost.T_HR_POST.T_HR_POSTDICTIONARY != null)
                            {
                                persondetail.OWNERPOSTNAME = emppost.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                            }

                            //persondetail.LIMITBUDGETMONEY 对应的是可用结余，统计的是当前预算之前时间里还剩余的月度预算；
                            //persondetail.USABLEMONEY 对应的是可用额度，即当前最大可下拨的预算；

                            persondetail.USABLEMONEY = Max_Charge;
                            if (subjectpost.LIMITBUDGEMONEY.BiggerThan(0))
                            {
                                persondetail.USABLEMONEY = subjectpost.LIMITBUDGEMONEY.Value;
                            }

                            // temp
                            persondetail.T_FB_SUBJECT = subjectpost.T_FB_SUBJECT;

                            // 获取个人的期初结余金额，个人预算明细的LIMITBUDGETMONEY用于存放此数据。
                            T_FB_BUDGETCHECK budgetCheckPerson = listDeptCheck.FirstOrDefault(item =>
                            {
                                return item.T_FB_SUBJECT.SUBJECTID == subjectID
                                    && item.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person
                                    && item.OWNERPOSTID == persondetail.OWNERPOSTID
                                    && item.OWNERID == persondetail.OWNERID;
                            });
                            if (budgetCheckPerson != null)
                            {
                                persondetail.LIMITBUDGETMONEY = budgetCheckPerson.USABLEMONEY;
                            }
                            else
                            {
                                persondetail.LIMITBUDGETMONEY = 0;//没有默认为0
                            }
                            listPersonBudgetDetail.Add(persondetail);
                            #endregion
                        });
                    }
                    #endregion
                });
                #endregion
            });
            #endregion

            listSubjectDept.ForEach(subjectDept =>
            {
                string subjectID = subjectDept.T_FB_SUBJECT.SUBJECTID;

                // 新建部门预算申请明细
                T_FB_DEPTBUDGETAPPLYDETAIL detail = new T_FB_DEPTBUDGETAPPLYDETAIL();
                detail.T_FB_SUBJECT = subjectDept.T_FB_SUBJECT;
                detail.AUDITBUDGETMONEY = 0;
                detail.DEPTBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                detail.BEGINNINGBUDGETBALANCE = 0;
                detail.BUDGETMONEY = 0;
                detail.LASTACTUALBUDGETMONEY = 0;
                detail.LASTBUDGEMONEY = 0;
                detail.PERSONBUDGETMONEY = 0;
                detail.YEARBUDGETBALANCE = 0;
                detail.TOTALBUDGETMONEY = 0;
                detail.USABLEMONEY = 0;
                FBEntity detailEntity = detail.ToFBEntity();
                // 查预算结算数据
                // 部门月度结算数据
                T_FB_BUDGETCHECK budgetCheckMonth = listDeptCheck.FirstOrDefault(item =>
                {
                    return item.T_FB_SUBJECT.SUBJECTID == subjectID && item.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Deaprtment;
                });

                if (budgetCheckMonth != null)
                {
                    detail.BEGINNINGBUDGETBALANCE = budgetCheckMonth.USABLEMONEY;
                    detail.LASTACTUALBUDGETMONEY = budgetCheckMonth.ACTUALMONEY;
                    detail.LASTBUDGEMONEY = budgetCheckMonth.BUDGETMONEY;
                    detail.YEARBUDGETBALANCE = budgetCheckMonth.YEARBUDGETMONEY;
                }


                // 加入个人预算申请明细
                List<T_FB_PERSONBUDGETAPPLYDETAIL> listPDetail = listPersonBudgetDetail.FindAll(item =>
                {

                    return item.T_FB_SUBJECT.SUBJECTID == subjectID;
                });





                if (listPDetail.Count > 0)
                {
                    decimal? personBudgetMoney = listPDetail.Sum(item =>
                    {
                        return item.BUDGETMONEY;
                    });

                    decimal? personBeginningMoney = listPDetail.Sum(item =>
                    {
                        return item.LIMITBUDGETMONEY;
                    });

                    detail.BEGINNINGBUDGETBALANCE = detail.BEGINNINGBUDGETBALANCE.Add(personBeginningMoney);
                    detail.PERSONBUDGETMONEY = personBudgetMoney;
                    detail.TOTALBUDGETMONEY = personBudgetMoney; // 总预算
                    detailEntity.AddFBEntities<T_FB_PERSONBUDGETAPPLYDETAIL>(listPDetail.ToFBEntityList());
                }

                // 加入可用金额
                T_FB_BUDGETACCOUNT budgetAccount = listBudgetAccount.FirstOrDefault(item =>
                {
                    return item.T_FB_SUBJECT.SUBJECTID == subjectID;
                });

                // 预算可用金额
                bool isYear = subjectDept.T_FB_SUBJECTCOMPANY.ISYEARBUDGET.Equal(1);
                // 不受年预算控制
                if (!isYear)
                {
                    detail.USABLEMONEY = Max_Budget;
                }
                else if (budgetAccount != null)
                {

                    detail.USABLEMONEY = budgetAccount.USABLEMONEY;
                }

                CompareResult r1 = subjectDept.LIMITBUDGEMONEY.Compare(0);

                //没有月度预算额度
                if (r1 == CompareResult.Bigger && subjectDept.LIMITBUDGEMONEY.LessThan(detail.USABLEMONEY))
                {
                    detail.USABLEMONEY = subjectDept.LIMITBUDGEMONEY;
                }

                // 加入结果集
                listResult.Add(detailEntity);
            });

            // 出除多余的关联
            listResult.ToEntityList<T_FB_DEPTBUDGETAPPLYDETAIL>().ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
                item.T_FB_SUBJECT.T_FB_BUDGETCHECK.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTDEPTMENT.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTPOST.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECT1.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECT2 = null;
            });

            return listResult;
        }

        /// <summary>
        /// 部门预算补增明细
        /// </summary>
        /// <remarks>
        ///     1.汇总个人预算申请单明细 listPersonAddDetail
        ///     2.从部门科目中找出所有可用科目 listSubjectDept
        ///     3 从当前预算总帐中找出相应可以于月度预算的金额 listBudgetAccount
        ///     4. 构成预算申请明细 listResult = listPersonAddDetail & ListSubjectDept & ListDeptCheck
        ///         1. 预算可用金额
        ///             1. 不受年度预算控制时，可用金额为decimal.max
        ///             ( A : 年度预算可用额度  , B: 月度预算最大额度, C: 已有的预算额度)
        ///             2. 受年度预算控制时 可用金额 = 
        ///                 1. 如果 B ＝＝ 0 , 可用额度 = A
        ///                 1. 如果 B > 0, 再如果 B-C 大 于 A，则取 A， 否则取 B-C
        /// </remarks>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_DEPTBUDGETADDDETAIL(QueryExpression qe)
        {
            // 已审核通过
            QueryExpression qeCheckPass = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID); // 部门ID

            // 1 汇总个人预算补增单明细 listPersonAddDetail
            qeCheckPass.RelatedExpression = qeDept;

            //List<T_FB_PERSONBUDGETADDMASTER> listPersonAdd = sBLL.GetEntities<T_FB_PERSONBUDGETADDMASTER>(qeCheckPass);
            List<T_FB_PERSONBUDGETADDDETAIL> listPersonAddDetail = new List<T_FB_PERSONBUDGETADDDETAIL>();
            //listPersonAdd.ForEach(item =>
            //{
            //    item.T_FB_PERSONBUDGETADDDETAIL.Load();
            //    listPersonAddDetail.AddRange(item.T_FB_PERSONBUDGETADDDETAIL);
            //});

            // 2 从部门科目中找出所有可用科目 listSubjectDept
            List<T_FB_SUBJECTDEPTMENT> listSubjectDept = GetSubjectDepartment(qeDept);

            //移除活动经费科目
            string MoneyAssign = SystemBLL.GetSetting(null).MONEYASSIGNSUBJECTID;
            if (MoneyAssign != string.Empty)
            {
                T_FB_SUBJECTDEPTMENT itemmoney = listSubjectDept.Where(item => item.T_FB_SUBJECT.SUBJECTID == MoneyAssign).FirstOrDefault();
                if (itemmoney != null)
                    listSubjectDept.Remove(itemmoney);
            }

            // 3 从当前预算总帐中找出相应可以于月度预算的金额 listBudgetAccount
            qeDept.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            List<T_FB_BUDGETACCOUNT> listBudgetAccount = GetEntities<T_FB_BUDGETACCOUNT>(qeDept);

            List<FBEntity> listResult = new List<FBEntity>();

            #region beyond
            SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient psc = new SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();
            listSubjectDept.ForEach(subjectDept =>
            {
                #region
                string subjectID = subjectDept.T_FB_SUBJECT.SUBJECTID;
                QueryExpression qa = QueryExpression.Equal("ISPERSON", "1");
                QueryExpression qb = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", subjectID);
                QueryExpression qc = QueryExpression.Equal("T_FB_SUBJECTDEPTMENT.OWNERDEPARTMENTID", qeDept.PropertyValue);

                qa.RelatedExpression = qb;
                qb.RelatedExpression = qc;
                qa.QueryType = typeof(T_FB_SUBJECTPOST).Name;
                //qa.Include = new ObservableCollection<string>() { typeof(T_FB_SUBJECT).Name };

                //科目对应的岗位
                List<T_FB_SUBJECTPOST> listSubjectPost = GetSubjectPost(qa);

                listSubjectPost.ForEach(subjectpost =>
                {
                    #region

                    SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEEPOST[] emps = psc.GetEmployeePostByPostID(subjectpost.OWNERPOSTID);

                    if (emps != null)
                    {
                        List<SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEEPOST> listEmp = emps.ToList<SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEEPOST>();
                        listEmp.ForEach(emppost =>
                        {
                            #region
                            T_FB_PERSONBUDGETADDDETAIL persondetail = new T_FB_PERSONBUDGETADDDETAIL();
                            persondetail.PERSONBUDGETADDDETAILID = Guid.NewGuid().ToString();
                            persondetail.BUDGETMONEY = 0;
                            persondetail.CREATEDATE = DateTime.Now;
                            persondetail.CREATEUSERID = "";
                            persondetail.CREATEUSERNAME = "";
                            persondetail.OWNERNAME = emppost.T_HR_EMPLOYEE.EMPLOYEECNAME;
                            persondetail.OWNERID = emppost.T_HR_EMPLOYEE.EMPLOYEEID;

                            persondetail.T_FB_SUBJECT = subjectpost.T_FB_SUBJECT;
                            persondetail.OWNERPOSTID = subjectpost.OWNERPOSTID;

                            if (emppost.T_HR_POST.T_HR_POSTDICTIONARY != null)
                            {
                                persondetail.OWNERPOSTNAME = emppost.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                            }


                            persondetail.USABLEMONEY = Max_Charge;
                            if (subjectpost.LIMITBUDGEMONEY.BiggerThan(0))
                            {
                                persondetail.USABLEMONEY = subjectpost.LIMITBUDGEMONEY.Value;
                            }

                            var accountPerson = listBudgetAccount.Where(item =>
                            {
                                return item.T_FB_SUBJECT.SUBJECTID == subjectID && item.ACCOUNTOBJECTTYPE.Equal((int)AccountObjectType.Person)
                                    && item.OWNERID == persondetail.OWNERID;
                            }).FirstOrDefault();
                            if (accountPerson != null)
                            {
                                persondetail.LIMITBUDGETMONEY = accountPerson.USABLEMONEY;
                            }
                            else
                            {
                                persondetail.LIMITBUDGETMONEY = 0;
                            }
                            //persondetail.T_FB_DEPTBUDGETAPPLYDETAIL = subjectDept;
                            //persondetail.T_FB_PERSONBUDGETAPPLYMASTER = new T_FB_PERSONBUDGETAPPLYMASTER
                            //{
                            //    OWNERPOSTID = subjectpost.SUBJECTPOSTID,
                            //    OWNERPOSTNAME = subjectpost.OWNERPOSTNAME,
                            //    OWNERID = emppost.T_HR_EMPLOYEE.EMPLOYEEID,
                            //    OWNERNAME = emppost.T_HR_EMPLOYEE.EMPLOYEECNAME,

                            //};


                            listPersonAddDetail.Add(persondetail);
                            #endregion
                        });
                    }


                    #endregion
                });
                #endregion
            });
            #endregion

            listSubjectDept.ForEach(subjectDept =>
            {
                string subjectID = subjectDept.T_FB_SUBJECT.SUBJECTID;
                //if (subjectID == "207745a9-c424-45b4-936a-fa1577deef00")
                //{
                //    string aa = "";
                //}
                T_FB_DEPTBUDGETADDDETAIL detail = new T_FB_DEPTBUDGETADDDETAIL();
                detail.T_FB_SUBJECT = subjectDept.T_FB_SUBJECT;
                detail.DEPTBUDGETADDDETAILID = Guid.NewGuid().ToString();
                detail.BUDGETMONEY = 0;
                detail.USABLEMONEY = 0;
                detail.TOTALBUDGETMONEY = 0;
                detail.PERSONBUDGETMONEY = 0;
                FBEntity detailEntity = detail.ToFBEntity();

                // 加入个人预算補增申请明细
                List<T_FB_PERSONBUDGETADDDETAIL> listPDetail = listPersonAddDetail.FindAll(item =>
                {
                    return item.T_FB_SUBJECT.SUBJECTID == subjectID;
                });
                if (listPDetail.Count > 0)
                {
                    decimal? personBudgetMoney = listPDetail.Sum(item =>
                    {
                        return item.BUDGETMONEY;
                    });

                    detail.PERSONBUDGETMONEY = personBudgetMoney;
                    detail.TOTALBUDGETMONEY = personBudgetMoney; // 总预算

                    detailEntity.AddFBEntities<T_FB_PERSONBUDGETADDDETAIL>(listPDetail.ToFBEntityList());
                }

                // 年度预算可用金额
                T_FB_BUDGETACCOUNT budgetAccountCompany = listBudgetAccount.FirstOrDefault(item =>
                {
                    return item.T_FB_SUBJECT.SUBJECTID == subjectID && item.ACCOUNTOBJECTTYPE.Equal((int)AccountObjectType.Company);
                });
                // 月度预算总账
                T_FB_BUDGETACCOUNT budgetAccountDept = listBudgetAccount.FirstOrDefault(item =>
                {
                    return item.T_FB_SUBJECT.SUBJECTID == subjectID && item.ACCOUNTOBJECTTYPE.Equal((int)AccountObjectType.Deaprtment);
                });
                // 个人预算总账                
                decimal? personBudgetMoneyAll = listBudgetAccount.Where(item =>
                {
                    return item.T_FB_SUBJECT.SUBJECTID == subjectID && item.ACCOUNTOBJECTTYPE.Equal((int)AccountObjectType.Person);
                }).Sum(itemS =>
                {
                    return itemS.USABLEMONEY;
                });

                // 预算可用金额
                decimal? YearUseable = 0;
                decimal? MonthBudget = 0;
                decimal? LimitBudget = 0;

                decimal? MonthSurplus = 0;//月度结余
                if (budgetAccountCompany != null)
                {
                    YearUseable = budgetAccountCompany.USABLEMONEY;
                }
                if (budgetAccountDept != null)
                {
                    MonthBudget = budgetAccountDept.BUDGETMONEY;
                    MonthSurplus = budgetAccountDept.USABLEMONEY;
                }

                MonthSurplus = MonthSurplus.Add(personBudgetMoneyAll);
                // 月度预算受年度预算控制
                bool isYear = subjectDept.T_FB_SUBJECTCOMPANY.ISYEARBUDGET.Equal(1);

                if (!isYear)
                {
                    YearUseable = Max_Budget;
                }


                //没有月度预算额度
                if (subjectDept.LIMITBUDGEMONEY.BiggerThan(0))
                {
                    LimitBudget = subjectDept.LIMITBUDGEMONEY - MonthBudget;
                    detail.USABLEMONEY = LimitBudget.BiggerThan(YearUseable) ? YearUseable : LimitBudget;
                }
                else
                {
                    detail.USABLEMONEY = YearUseable;
                }
                detail.AUDITBUDGETMONEY = MonthSurplus;
                // 加入结果集
                listResult.Add(detailEntity);
            });

            // 出除多余的关联
            listResult.ToEntityList<T_FB_DEPTBUDGETAPPLYDETAIL>().ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
                item.T_FB_SUBJECT.T_FB_BUDGETCHECK.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTDEPTMENT.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTPOST.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECT1.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECT2 = null;
            });
            return listResult;
        }


        ///// <summary>
        ///// 月度预算调拨
        ///// </summary>
        ///// <remarks>
        /////  1 从部门科目中找出所有可用科目 listSubjectDept
        /////  2 从当前预算总帐中找出相应可以于月度预算的金额 listBudgetAccount
        ///// </remarks>
        ///// <param name="qe"></param>
        ///// <returns></returns>
        //public List<FBEntity> QueryT_FB_DEPTTRANSFERDETAIL(QueryExpression qe)
        //{
        //    SubjectBLL bll = new SubjectBLL();

        //    qe.IsNoTracking = true;
        //    qe.IsUnCheckRight = true;
        //    qe.Include = new string[] { typeof(T_FB_SUBJECT).Name };
        //    var dataBudgetAccount = qe.Query<T_FB_BUDGETACCOUNT>();

        //    var data = (from item in dataBudgetAccount
        //                where item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Any(itemSC =>
        //                    itemSC.ISMONTHADJUST == 1 && itemSC.OWNERCOMPANYID == item.OWNERCOMPANYID
        //                    )
        //                select item);
        //    var listAccount = data.ToList();
        //    listAccount = listAccount.OrderBy(item => item.T_FB_SUBJECT.SUBJECTCODE).ToList();
        //    List<T_FB_DEPTTRANSFERDETAIL> listResult = listAccount.CreateList(item =>
        //    {
        //        T_FB_DEPTTRANSFERDETAIL detail = new T_FB_DEPTTRANSFERDETAIL();
        //        detail.DEPTTRANSFERDETAILID = Guid.NewGuid().ToString();
        //        detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
        //        detail.USABLEMONEY = item.USABLEMONEY;
        //        detail.TRANSFERMONEY = 0;
        //        return detail;
        //    });

        //    listResult.ForEach(item =>
        //    {
        //        item.T_FB_SUBJECT.T_FB_BUDGETACCOUNT.Clear();
        //        item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
        //    });
        //    return listResult.ToFBEntityList();
        //}
        #endregion

        #region 科目维护
        /// <summary>
        /// 查询科目与科目类型，不做数据权限控制
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_SUBJECTTYPE(QueryExpression qe)
        {
            QueryExpression qeAdd = QueryExpression.Equal(FieldName.EditStates, "1");
            qeAdd.QueryType = "T_FB_SUBJECTTYPE";
            qeAdd.RelatedExpression = qe;
            qeAdd.IsUnCheckRight = true; //不做权限控制
            qeAdd.Include = new string[] { typeof(T_FB_SUBJECT).Name };

            List<FBEntity> list = BaseGetEntities(qeAdd).ToFBEntityList();

            list.ForEach(item =>
            {
                T_FB_SUBJECTTYPE sType = item.Entity as T_FB_SUBJECTTYPE;
                List<T_FB_SUBJECT> listSubject = sType.T_FB_SUBJECT.ToList();

                listSubject.RemoveAll(entity =>
                {
                    return entity.EDITSTATES == 0;
                });

                item.AddFBEntities<T_FB_SUBJECT>(listSubject.ToFBEntityList());

                item.OrderDetailBy<T_FB_SUBJECT>(itemSubject => itemSubject.SUBJECTCODE);
            });
            return list;

        }

        /// <summary>
        /// 查询部门科目，不做数据权限控制
        /// 数据的控制，控制显示的部门
        /// // 1. 按权限过滤可以显示的部门 ListDept。
        ///     ２.　查出ListDept对应的subjectDepartment数据
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_SUBJECTDEPTMENT(QueryExpression queryExpression)
        {
            OrganizationBLL oBll = new OrganizationBLL();

            List<FBEntity> listDepartment = new List<FBEntity>();

            // 按访问人过滤 ？
            List<FBEntity> listCompany = oBll.GetCompany(queryExpression);

            listCompany.ForEach(fbCompany =>
            {
                VirtualCompany company = fbCompany.Entity as VirtualCompany;
                string companyID = company.ID;
                QueryExpression qeSubjectCompany = QueryExpression.Equal("OWNERCOMPANYID", companyID);
                qeSubjectCompany.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name;
                // qeSubjectCompany.Include = new string[] { "T_FB_SUBJECT" };

                List<T_FB_SUBJECTCOMPANY> listSubjectCompany = GetSubjectCompany(qeSubjectCompany);

                fbCompany.CollectionEntity[0].FBEntities.ForEach(fbDepartment =>
                {
                    VirtualDepartment department = fbDepartment.Entity as VirtualDepartment;
                    List<FBEntity> listsd = GetSubjectDepartment(department, listSubjectCompany);

                    fbDepartment.AddFBEntities<T_FB_SUBJECTDEPTMENT>(listsd);

                    fbDepartment.OrderDetailBy<T_FB_SUBJECTDEPTMENT>(item => item.T_FB_SUBJECT.SUBJECTCODE);

                    listDepartment.Add(fbDepartment);
                });

            });
            return listDepartment;

        }

        public List<FBEntity> QueryT_FB_SUBJECTCOMPANY(QueryExpression queryExpression)
        {
            OrganizationBLL oBll = new OrganizationBLL();
            List<VirtualCompany> listCompany = oBll.GetVirtualCompany(queryExpression);
            List<T_FB_SUBJECT> listSubject = GetSubject(null);
            List<FBEntity> listResult = new List<FBEntity>();
            listCompany.ForEach(company =>
            {
                List<FBEntity> listsc = GetSubjectCompany(company, listSubject);
                RelationManyEntity rme = new RelationManyEntity();
                rme.EntityType = "T_FB_SUBJECTCOMPANY";
                rme.FBEntities = listsc;
                FBEntity fbEntity = company.ToFBEntity();
                fbEntity.CollectionEntity.Add(rme);
                fbEntity.OrderDetailBy<T_FB_SUBJECTCOMPANY>(item => item.T_FB_SUBJECT.SUBJECTCODE);
                listResult.Add(fbEntity);
            });

            return listResult;

        }

        /// <summary>
        /// 公司科目设置
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_SUBJECTCOMPANYSET(QueryExpression queryExpression)
        {
            OrganizationBLL oBll = new OrganizationBLL();
            List<VirtualCompany> listCompany = oBll.GetVirtualCompany(queryExpression);
            List<T_FB_SUBJECT> listSubject = GetSubject(null);
            List<FBEntity> listResult = new List<FBEntity>();
            listCompany.ForEach(company =>
            {
                List<FBEntity> listsc = GetSubjectCompany(company, listSubject);
                RelationManyEntity rme = new RelationManyEntity();
                rme.EntityType = "T_FB_SUBJECTCOMPANY";
                rme.FBEntities = listsc;
                FBEntity fbEntity = company.ToFBEntity();
                fbEntity.CollectionEntity.Add(rme);
                fbEntity.OrderDetailBy<T_FB_SUBJECTCOMPANY>(item => item.T_FB_SUBJECT.SUBJECTCODE);
                listResult.Add(fbEntity);
            });

            return listResult;

        }


        /// <summary>
        /// 主查公司科目设置表 与公司科目分配表查询分开
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_SUBJECTCOMPANY_COMPANY(QueryExpression queryExpression)
        {
            OrganizationBLL oBll = new OrganizationBLL();
            List<VirtualCompany> listCompany = oBll.GetVirtualCompany(queryExpression);
            List<T_FB_SUBJECT> listSubject = GetSubject(null);
            List<FBEntity> listResult = new List<FBEntity>();
            listCompany.ForEach(company =>
            {
                List<FBEntity> listsc = GetSubjectCompany_Company(company, listSubject);
                RelationManyEntity rme = new RelationManyEntity();
                rme.EntityType = "T_FB_SUBJECTCOMPANY";
                rme.FBEntities = listsc;
                FBEntity fbEntity = company.ToFBEntity();
                fbEntity.CollectionEntity.Add(rme);
                fbEntity.OrderDetailBy<T_FB_SUBJECTCOMPANY>(item => item.T_FB_SUBJECT.SUBJECTCODE);
                listResult.Add(fbEntity);
            });

            return listResult;

        }


        public List<FBEntity> QueryVirtualCompany(QueryExpression qe)
        {
            //List<FBEntity> entTemps = new List<FBEntity>();
            //return entTemps;

            OrganizationBLL sBLL = new OrganizationBLL();
            return sBLL.GetCompany(qe);
        }
        #endregion


        #region 不用
        /// <summary>
        /// 个人预算明细
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_PERSONBUDGETAPPLYDETAIL(QueryExpression qe)
        {
            QueryExpression qeOwnerPostID = qe.GetQueryExpression(FieldName.OwnerPostID);

            qeOwnerPostID.RelatedExpression = null;
            QueryExpression qeIsPerson = QueryExpression.Equal("ISPERSON", "1");
            qeIsPerson.QueryType = typeof(T_FB_SUBJECTPOST).Name;
            qeIsPerson.RelatedExpression = qeOwnerPostID;

            List<T_FB_SUBJECTPOST> listSubjectPost = GetSubjectPost(qeIsPerson);

            List<T_FB_PERSONBUDGETAPPLYDETAIL> listResult = new List<T_FB_PERSONBUDGETAPPLYDETAIL>();

            listSubjectPost.ForEach(item =>
            {
                T_FB_PERSONBUDGETAPPLYDETAIL detail = new T_FB_PERSONBUDGETAPPLYDETAIL();

                detail.BUDGETMONEY = 0;
                detail.PERSONBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                listResult.Add(detail);
            });

            return listResult.ToFBEntityList();
        }
        /// <summary>
        /// 个人预算补增明细
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_PERSONBUDGETADDDETAIL(QueryExpression qe)
        {
            QueryExpression qeOwnerID = qe.GetQueryExpression(FieldName.OwnerID);
            QueryExpression qeOwnerDepartmentID = qe.GetQueryExpression(FieldName.OwnerDepartmentID);
            QueryExpression qeOwnerCompanyID = qe.GetQueryExpression(FieldName.OwnerCompanyID);
            QueryExpression qeOwnerPostID = qe.GetQueryExpression(FieldName.OwnerPostID);

            qeOwnerPostID.RelatedExpression = null;
            QueryExpression qeIsPerson = QueryExpression.Equal("ISPERSON", "1");
            qeIsPerson.QueryType = typeof(T_FB_SUBJECTPOST).Name;
            qeIsPerson.RelatedExpression = qeOwnerPostID;

            List<T_FB_SUBJECTPOST> listSubjectPost = GetSubjectPost(qeIsPerson);

            qeOwnerCompanyID.RelatedExpression = qeOwnerDepartmentID;
            qeOwnerDepartmentID.RelatedExpression = qeOwnerID;
            qeOwnerID.RelatedExpression = null;
            List<T_FB_BUDGETACCOUNT> listAccount = GetBUDGETACCOUNT(qeOwnerCompanyID, AccountObjectType.Person);

            List<T_FB_PERSONBUDGETADDDETAIL> listResult = new List<T_FB_PERSONBUDGETADDDETAIL>();
            listSubjectPost.ForEach(subjectPost =>
            {
                T_FB_BUDGETACCOUNT accountPerson = listAccount.FirstOrDefault(account =>
                {
                    return account.T_FB_SUBJECT.SUBJECTID == subjectPost.T_FB_SUBJECT.SUBJECTID;
                });

                T_FB_PERSONBUDGETADDDETAIL detail = new T_FB_PERSONBUDGETADDDETAIL();
                detail.T_FB_SUBJECT = subjectPost.T_FB_SUBJECT;
                detail.BUDGETMONEY = 0;
                detail.PERSONBUDGETADDDETAILID = Guid.NewGuid().ToString();
                //detail.SERIALNUMBER = listResult.Count + 1;
                detail.USABLEMONEY = 0;

                if (accountPerson != null)
                {
                    detail.USABLEMONEY = accountPerson.USABLEMONEY;
                }
                listResult.Add(detail);
            });
            return listResult.ToFBEntityList();
        }

        #endregion


        public List<FBEntity> QueryT_FB_PERSONMONEYASSIGNMASTERLatest(QueryExpression qe)
        {
            QueryExpression qeOwner = qe.GetQueryExpression(FieldName.OwnerID);
            QueryExpression qeAssign = qe.GetQueryExpression("ASSIGNCOMPANYID");

            QueryExpression qeCheckStates = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            qeCheckStates.RelatedType = QueryExpression.RelationType.And;

            qeOwner.RelatedExpression = qeCheckStates;
            qeAssign.RelatedExpression = qeOwner;
            qeAssign.QueryType = qe.QueryType;

            qeAssign.Include = new string[] { typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name };

            var masters = InnerGetEntities<T_FB_PERSONMONEYASSIGNMASTER>(qeAssign).OrderByDescending(item => item.BUDGETARYMONTH);
            var result = masters.ToList().FirstOrDefault();
            List<FBEntity> resultList = new List<FBEntity>();
            if (result != null)
            {
                FBEntity fbResult = result.ToFBEntity();

                //处理岗位信息一栏
                List<T_FB_PERSONMONEYASSIGNDETAIL> rlist = result.T_FB_PERSONMONEYASSIGNDETAIL.ToList();
                OrganizationBLL obll = new OrganizationBLL();
                rlist = obll.UpdatePostInfo(rlist);
                //按公司、部门排序
                rlist = rlist.OrderBy(c => c.OWNERCOMPANYID).ThenBy(c => c.OWNERDEPARTMENTID).ToList();


                fbResult.AddFBEntities<T_FB_PERSONMONEYASSIGNDETAIL>(rlist.ToFBEntityList());
                //  fbResult.AddFBEntities<T_FB_PERSONMONEYASSIGNDETAIL>(result.T_FB_PERSONMONEYASSIGNDETAIL.ToList().ToFBEntityList());
                resultList.Add(fbResult);
            }
            return resultList;

        }

        public List<FBEntity> QueryT_FB_PERSONMONEYASSIGNMASTERFormHR(QueryExpression qe)
        {
            QueryExpression qeOwner = qe.GetQueryExpression(FieldName.OwnerID);
            QueryExpression qeAssign = qe.GetQueryExpression("ASSIGNCOMPANYID");


            var masters = GetPersonMoneyAssign(qeAssign.PropertyValue, qeOwner.PropertyValue);

            List<FBEntity> resultList = new List<FBEntity>();
            if (masters != null)
            {
                FBEntity fbResult = masters.ToFBEntity();

                //处理岗位信息一栏
                List<T_FB_PERSONMONEYASSIGNDETAIL> rlist = masters.T_FB_PERSONMONEYASSIGNDETAIL.ToList();
                // rlist = obll.UpdatePostInfo(rlist);
                //按公司、部门排序
                rlist = rlist.OrderBy(c => c.OWNERCOMPANYID).ThenBy(c => c.OWNERDEPARTMENTID).ToList();

                fbResult.AddFBEntities<T_FB_PERSONMONEYASSIGNDETAIL>(rlist.ToFBEntityList());
                resultList.Add(fbResult);
            }

            return resultList;
        }


        /// <summary>
        /// 查询活动经费下拨
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public FBEntity GetEntityT_FB_PERSONMONEYASSIGNMASTER(QueryExpression qe)
        {
            FBEntity fbEntity = this.GetEntityDefault(qe);
            fbEntity.OrderDetailBy<T_FB_PERSONMONEYASSIGNDETAIL>(item => item.OWNERDEPARTMENTNAME);
            return fbEntity;
        }

        #endregion


        #region 5.	辅助查询方法
        /// <summary>
        /// 初始化程序所需数据
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryInitDataCore(QueryExpression qe)
        {
            FBEntity result = new FBEntity();
            var listCompany = result.GetRelationFBEntities("VirtualCompany");
            var listSetting = result.GetRelationFBEntities("T_FB_SYSTEMSETTINGS");

            qe.QueryType = typeof(VirtualCompany).Name;
            var listVC = QueryVirtualCompany(qe);
            listCompany.AddRange(listVC);

            qe.QueryType = typeof(T_FB_SYSTEMSETTINGS).Name;
            var listST = QueryT_FB_SYSTEMSETTINGS(qe);
            listSetting.AddRange(listST);
            return new List<FBEntity> { result };
        }

        /// <summary>
        /// 系统设置
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_SYSTEMSETTINGS(QueryExpression qe)
        {
            List<FBEntity> listResult = new List<FBEntity>();
            T_FB_SYSTEMSETTINGS entity = SystemBLL.GetSetting(qe);
            entity = entity.CopyEntity();
            if (entity != null)
            {
                entity.UPDATEDATE = System.DateTime.Now.Date;
                listResult.Add(entity.ToFBEntity());

            }
            return listResult;
        }
        #endregion




        #region 活动经费
        public T_FB_PERSONMONEYASSIGNMASTER CreatePersonMoneyAssignInfo(string ASSIGNCOMPANYID, string OWNERID)
        {
            T_FB_PERSONMONEYASSIGNMASTER master = GetPersonMoneyAssign(ASSIGNCOMPANYID, OWNERID);

            if (master == null)
            {
                return null;
            }

            if (master.T_FB_PERSONMONEYASSIGNDETAIL == null)
            {
                return null;
            }

            if (master.T_FB_PERSONMONEYASSIGNDETAIL.Count() == 0)
            {
                return null;
            }

            FBEntity fbEntity = new FBEntity();
            fbEntity.Entity = master;
            fbEntity.FBEntityState = FBEntityState.Added;
            SaveFBEntityDefault(fbEntity);

            string strCustomMsgBody = "您收到了[" + master.ASSIGNCOMPANYNAME + "]的活动经费下拨申请单，请及时处理！";

            EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
            userMsg.FormID = master.PERSONMONEYASSIGNMASTERID;
            userMsg.UserID = master.OWNERID;
            EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
            List[0] = userMsg;
            string submitName = master.OWNERNAME;
            Client.ApplicationMsgTriggerCustom(List, "FB", "T_FB_PERSONMONEYASSIGNMASTER", ObjListToXml(master, "FB", submitName), EngineWS.MsgType.Task, strCustomMsgBody);

            return master;
        }

        public T_FB_PERSONMONEYASSIGNMASTER GetPersonMoneyAssign(string ASSIGNCOMPANYID, string OWNERID)
        {
            PersonnelWS.PersonnelServiceClient pe = new PersonnelWS.PersonnelServiceClient();

            T_FB_PERSONMONEYASSIGNMASTER master = new T_FB_PERSONMONEYASSIGNMASTER();
            master.PERSONMONEYASSIGNMASTERID = Guid.NewGuid().ToString();

            PersonnelWS.V_EMPLOYEEPOST employee = pe.GetEmployeeDetailByID(OWNERID);

            OrganizationWS.OrganizationServiceClient orgClient = new OrganizationWS.OrganizationServiceClient();
            OrganizationWS.T_HR_COMPANY entCompany = orgClient.GetCompanyById(ASSIGNCOMPANYID);

            //如果下拨的公司不存在，则不会创建下拨单
            if (entCompany == null)
            {
                return null;
            }

            string budgetMonth = DateTime.Now.ToString("yyyy-MM") + "-1";

            QueryExpression qeAssignComp = QueryExpression.Equal("ASSIGNCOMPANYID", entCompany.COMPANYID);
            qeAssignComp.QueryType = "T_FB_PERSONMONEYASSIGNMASTER";

            QueryExpression qeOwner = QueryExpression.Equal(FieldName.OwnerID, OWNERID);
            QueryExpression qeBudgetMonth = QueryExpression.Equal("BUDGETARYMONTH", budgetMonth);
            QueryExpression qeCheckState = QueryExpression.NotEqual(FieldName.CheckStates, Convert.ToInt32(CheckStates.UnApproved).ToString());

            qeBudgetMonth.RelatedExpression = qeCheckState;
            qeOwner.RelatedExpression = qeBudgetMonth;
            qeAssignComp.RelatedExpression = qeOwner;

            List<T_FB_PERSONMONEYASSIGNMASTER> assignMasters = GetEntities<T_FB_PERSONMONEYASSIGNMASTER>(qeAssignComp);
            if (assignMasters != null)
            {
                if (assignMasters.Count() > 0)
                {
                    return null;
                }
            }

            T_FB_PERSONMONEYASSIGNMASTER entLastMonthmMaster = null;
            List<T_FB_PERSONMONEYASSIGNDETAIL> entLastMonthmdetails = new List<T_FB_PERSONMONEYASSIGNDETAIL>(); ;
            QueryExpression qelastAssignComp = QueryExpression.Equal("ASSIGNCOMPANYID", entCompany.COMPANYID);
            qelastAssignComp.QueryType = "T_FB_PERSONMONEYASSIGNMASTER";

            QueryExpression qelastOwner = QueryExpression.Equal(FieldName.OwnerID, OWNERID);
            QueryExpression qelastBudgetMonth = QueryExpression.Equal("BUDGETARYMONTH", DateTime.Now.AddMonths(-1).ToString("yyyy-MM") + "-1");
            QueryExpression qelastCheckState = QueryExpression.Equal(FieldName.CheckStates, Convert.ToInt32(CheckStates.Approved).ToString());

            qelastBudgetMonth.RelatedExpression = qelastCheckState;
            qelastOwner.RelatedExpression = qelastBudgetMonth;
            qelastAssignComp.RelatedExpression = qelastOwner;
            FBEntity fblastEntity = GetEntityDefault(qelastAssignComp);
            if (fblastEntity != null)
            {
                entLastMonthmMaster = fblastEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;

                if (entLastMonthmMaster != null)
                {
                    var existItems = fblastEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
                    foreach (var existItem in existItems)
                    {
                        T_FB_PERSONMONEYASSIGNDETAIL entLastMonthDetail = existItem.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                        entLastMonthmdetails.Add(entLastMonthDetail);
                    }
                }
            }

            master.OWNERID = OWNERID;
            master.OWNERNAME = employee.T_HR_EMPLOYEE.EMPLOYEECNAME;
            master.OWNERCOMPANYID = employee.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            master.OWNERCOMPANYNAME = employee.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
            master.OWNERDEPARTMENTID = employee.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
            master.OWNERDEPARTMENTNAME = employee.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            master.OWNERPOSTID = employee.EMPLOYEEPOSTS[0].T_HR_POST.POSTID;
            master.OWNERPOSTNAME = employee.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;

            master.ASSIGNCOMPANYID = ASSIGNCOMPANYID;
            master.ASSIGNCOMPANYNAME = ASSIGNCOMPANYID;
            if (entCompany != null)
            {
                master.ASSIGNCOMPANYNAME = entCompany.CNAME;
            }

            master.BUDGETARYMONTH = DateTime.Parse(budgetMonth);
            master.BUDGETMONEY = 0;
            master.EDITSTATES = 1;
            master.CHECKSTATES = 0;


            master.CREATECOMPANYID = "001";
            master.CREATECOMPANYNAME = "系统生成";
            master.CREATEDEPARTMENTID = "001";
            master.CREATEDEPARTMENTNAME = "系统生成";
            master.CREATEPOSTID = "001";
            master.CREATEPOSTNAME = "系统生成";
            master.CREATEUSERID = "001";
            master.CREATEUSERNAME = "系统生成";

            master.UPDATEUSERID = "001";
            master.UPDATEUSERNAME = "系统生成";
            master.CREATEDATE = DateTime.Now;
            master.UPDATEDATE = DateTime.Now;

            PersonnelWS.V_EMPLOYEEFUNDS[] vlistpostinfo = null;

            try
            {
                //通过HR接口获取被下拨人
                vlistpostinfo = pe.GetEmployeeFunds(ASSIGNCOMPANYID,OWNERID);
                if (vlistpostinfo == null)
                {
                    SystemBLL.Debug("调用HR服务 GetEmployeeFunds,返回结果为空，对应下拨公司的ID为： " + ASSIGNCOMPANYID);
                    return null;
                }

                if (vlistpostinfo.Count() == 0)
                {
                    SystemBLL.Debug("调用HR服务 GetEmployeeFunds,返回结果为空，对应下拨公司的ID为： " + ASSIGNCOMPANYID);
                    return null;
                }
            }
            catch (Exception ex)
            {
                SystemBLL.Debug("调用HR服务异常 GetEmployeeFunds " + ex.ToString());
            }

            if (vlistpostinfo != null && vlistpostinfo.Count() > 0)
            {
                QueryExpression qe = QueryExpression.Equal("SUBJECTID", "d5134466-c207-44f2-8a36-cf7b96d5851f");
                qe.QueryType = typeof(T_FB_SUBJECT).Name;
                T_FB_SUBJECT entSubject = GetEntity<T_FB_SUBJECT>(qe);
                if (entSubject == null)
                {
                    return null;
                }                

                foreach (var item in vlistpostinfo)
                {
                    T_FB_PERSONMONEYASSIGNDETAIL detail = new T_FB_PERSONMONEYASSIGNDETAIL();
                    detail.PERSONBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                    detail.T_FB_PERSONMONEYASSIGNMASTER = master;
                    detail.T_FB_SUBJECT = entSubject;
                    //detail.BUDGETMONEY = item.REALSUM;
                    detail.BUDGETMONEY = item.REALSUM;
                    detail.SUGGESTBUDGETMONEY = item.NEEDSUM;
                    detail.POSTINFO = item.ATTENDREMARK;

                    detail.OWNERID = item.EMPLOYEEID;
                    detail.OWNERNAME = item.EMPLOYECNAME;
                    detail.OWNERPOSTID = item.POSTID;
                    detail.OWNERPOSTNAME = item.POSTNAME;
                    detail.OWNERDEPARTMENTID = item.DEPARTMENTID;
                    detail.OWNERDEPARTMENTNAME = item.DEPARTMENTNAME;
                    detail.OWNERCOMPANYID = item.COMPANYID;
                    detail.OWNERCOMPANYNAME = item.COMPANYNAME;

                    detail.CREATEUSERID = "001";
                    detail.CREATEUSERNAME = "系统生成";
                    detail.CREATEDATE = DateTime.Now;

                    detail.UPDATEUSERID = "001";
                    detail.UPDATEUSERNAME = "系统生成";
                    detail.UPDATEDATE = DateTime.Now;

                    if (item.REALSUM != null)
                    {
                        master.BUDGETMONEY += item.REALSUM;
                    }
                }

                if (master.T_FB_PERSONMONEYASSIGNDETAIL != null)
                {
                    if (master.T_FB_PERSONMONEYASSIGNDETAIL.Count() > 0)
                    {
                        foreach (var entLastMonthDetail in entLastMonthmdetails)
                        {
                            var existitems = from n in master.T_FB_PERSONMONEYASSIGNDETAIL
                                             where n.OWNERID == entLastMonthDetail.OWNERID
                                             select n;


                            if (existitems.Count() > 0)
                            {
                                continue;
                            }

                            T_FB_PERSONMONEYASSIGNDETAIL detail = new T_FB_PERSONMONEYASSIGNDETAIL();
                            detail.PERSONBUDGETAPPLYDETAILID = Guid.NewGuid().ToString();
                            detail.T_FB_PERSONMONEYASSIGNMASTER = master;
                            detail.T_FB_SUBJECT = entSubject;
                            //detail.BUDGETMONEY = item.REALSUM;
                            detail.BUDGETMONEY = entLastMonthDetail.BUDGETMONEY;
                            detail.SUGGESTBUDGETMONEY = entLastMonthDetail.SUGGESTBUDGETMONEY;
                            detail.POSTINFO = entLastMonthDetail.POSTINFO;

                            detail.OWNERID = entLastMonthDetail.OWNERID;
                            detail.OWNERNAME = entLastMonthDetail.OWNERNAME;
                            detail.OWNERPOSTID = entLastMonthDetail.OWNERPOSTID;
                            detail.OWNERPOSTNAME = entLastMonthDetail.OWNERPOSTNAME;
                            detail.OWNERDEPARTMENTID = entLastMonthDetail.OWNERDEPARTMENTID;
                            detail.OWNERDEPARTMENTNAME = entLastMonthDetail.OWNERDEPARTMENTNAME;
                            detail.OWNERCOMPANYID = entLastMonthDetail.OWNERCOMPANYID;
                            detail.OWNERCOMPANYNAME = entLastMonthDetail.OWNERCOMPANYNAME;

                            detail.CREATEUSERID = "001";
                            detail.CREATEUSERNAME = "系统生成";
                            detail.CREATEDATE = DateTime.Now;

                            detail.UPDATEUSERID = "001";
                            detail.UPDATEUSERNAME = "系统生成";
                            detail.UPDATEDATE = DateTime.Now;

                            if (entLastMonthDetail.BUDGETMONEY != null)
                            {
                                master.BUDGETMONEY += entLastMonthDetail.BUDGETMONEY;
                            }
                        }
                    }
                }
            }

            return master;
        }


        /// <summary>
        /// 引擎需要的XML形式的实体字符串转化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, string SystemCode, string currentUserName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<System>");
            Type objtype = objectdata.GetType();
            sb.AppendLine("<Name>" + SystemCode + "</Name>");
            sb.AppendLine("<Object Name=\"Approval\" Description=\"\">");
            PropertyInfo[] propinfos = objtype.GetProperties();
            foreach (PropertyInfo propinfo in propinfos)
            {
                if (propinfo.Name.ToUpper() != "CHECKSTATE")
                {
                    sb.AppendLine("<Attribute Name=\"" + propinfo.Name + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + propinfo.GetValue(objectdata, null) + "\"/>");
                }
            }
            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + currentUserName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();
        }

        #endregion

        #region 保存方法
        public FBEntity SaveT_FB_EXTENSIONALORDER(FBEntity fbEntity)
        {

            string defaultOrderCode = SystemBLL.DEFAULTORDERCODE;
            FBEntityBLL bll = this;
            try
            {
                bll.BeginTransaction();

                // 如果是提交审核,需要通过外部单据生成内部单据
                T_FB_EXTENSIONALORDER entityExtensionalOrder = fbEntity.Entity as T_FB_EXTENSIONALORDER;
                CheckStates cStates = (CheckStates)((int)entityExtensionalOrder.CHECKSTATES);
                CheckStates cOldStates = CheckStates.UnSubmit;
                //cOldStates = GetOldStatesByExtension(entityExtensionalOrder);

                var oldExOrder = GetOldExOrder(entityExtensionalOrder);
                if (oldExOrder != null)
                {
                    entityExtensionalOrder.INNERORDERCODE = oldExOrder.INNERORDERCODE;
                    entityExtensionalOrder.INNERORDERID = oldExOrder.INNERORDERID;
                    entityExtensionalOrder.INNERORDERTYPE = oldExOrder.INNERORDERTYPE;
                    cOldStates = (CheckStates)((int)oldExOrder.CHECKSTATES);
                }
                string innerOrderID = entityExtensionalOrder.INNERORDERID;

                #region 单据状态判断
                if (cStates == CheckStates.UnSubmit && (cOldStates == CheckStates.Approving || cOldStates == CheckStates.Approved))
                {
                    throw new Exception("单据已在审核中或已审核通过，不能修改");
                }
                #endregion
                #region 处理 Extension 单据

                // 第一次保存
                if ((cStates == CheckStates.UnSubmit || fbEntity.FBEntityState == FBEntityState.ReSubmit) && string.IsNullOrEmpty(entityExtensionalOrder.INNERORDERID))
                {
                    // entityExtensionalOrder.INNERORDERID = Guid.NewGuid().ToString();

                    if (fbEntity.FBEntityState == FBEntityState.Unchanged)
                    {
                        fbEntity.FBEntityState = FBEntityState.Modified;
                    }
                    else if (fbEntity.FBEntityState == FBEntityState.ReSubmit)
                    {
                        fbEntity.FBEntityState = FBEntityState.Modified;
                    }
                }



                //当单据第一次被提交时，设置内部单据编号，根据不同的order type 生成相应的单据编号
                if (entityExtensionalOrder.INNERORDERCODE == defaultOrderCode)
                {
                    if (cStates == CheckStates.Approving && cOldStates == CheckStates.UnSubmit)
                    {
                        CreateInnerOrderCode(entityExtensionalOrder);

                    }
                }
                else if (string.IsNullOrEmpty(entityExtensionalOrder.INNERORDERCODE))
                {
                    entityExtensionalOrder.INNERORDERCODE = defaultOrderCode;
                }

                // 将对象数据转换为xml存于数据库中, 并清除对象数据。
                string payInfo = UnFormatExtensionOrder(fbEntity);
                List<FBEntity> listDetailBorrowTemp = fbEntity.GetRelationFBEntities(xmlOrderName);
                var listMaster = listDetailBorrowTemp.ToList();
                listDetailBorrowTemp.Clear();

                if (entityExtensionalOrder.T_FB_EXTENSIONALTYPE == null)
                {
                    var qeEType = QueryExpression.Equal("EXTENSIONALTYPECODE", "CCPX");
                    var etype = this.InnerGetEntities<T_FB_EXTENSIONALTYPE>(qeEType).FirstOrDefault();
                    entityExtensionalOrder.T_FB_EXTENSIONALTYPE = etype;
                }
                bll.InnerSave(fbEntity);
                // 查询最新的外部单据带上明细和明细的T_FB_SUBJECT

                QueryExpression qeQExOrder = QueryExpression.Equal("ORDERID", entityExtensionalOrder.ORDERID);
                qeQExOrder.QueryType = typeof(T_FB_EXTENSIONALORDER).Name;
                #endregion
                FBEntity fbEntityQuery = bll.FBEntityBLLGetFBEntity(qeQExOrder);
                entityExtensionalOrder = fbEntityQuery.Entity as T_FB_EXTENSIONALORDER;

                FBEntity fbEntityInnerOrder = null;
                switch (cStates)
                {
                    case CheckStates.UnSubmit:
                        {

                            if (cOldStates == CheckStates.UnApproved)
                            {

                            }
                            break;
                        }
                    case CheckStates.Approving:
                        {
                            if (entityExtensionalOrder.INNERORDERCODE == "<自动生成>")
                            {
                                CreateInnerOrderCode(entityExtensionalOrder);
                            }

                            if (cOldStates == CheckStates.UnSubmit || cOldStates == CheckStates.UnApproved)
                            {
                                #region　月结
                                bool isChecked = SystemBLL.IsChecked;
                                if (!isChecked)
                                {
                                    throw new Exception("本月尚未结算,无法提交!");
                                }
                                #endregion

                                // 生成内部单据的ID
                                entityExtensionalOrder.INNERORDERID = Guid.NewGuid().ToString();

                                //2012 version保存外部扩展单据（更新guid）
                                entityExtensionalOrder.CHECKSTATES = 1;

                                if (entityExtensionalOrder.T_FB_EXTENSIONORDERDETAIL == null) throw new Exception("外部单据没有子记录，无法保存");
                                entityExtensionalOrder.TOTALMONEY = entityExtensionalOrder.T_FB_EXTENSIONORDERDETAIL.Sum(c => c.APPLIEDMONEY);
                                bll.BassBllSave(entityExtensionalOrder, FBEntityState.Modified);

                                #region　生成内部单据
                                FBEntity fbEntityNew = null;
                                // 生成新的费用报销单
                                SetNewFBEntityByExtensionOrder(FBEntityState.Added, entityExtensionalOrder, payInfo, listMaster, ref fbEntityNew);

                                fbEntityNew.FBEntityState = FBEntityState.Added;
                                #endregion

                                fbEntityInnerOrder = fbEntityNew;
                            }

                        }
                        break;
                    case CheckStates.Approved:
                    case CheckStates.UnApproved:
                        {

                            #region 审核通过、不通过
                            QueryExpression qe = new QueryExpression();
                            qe.Operation = QueryExpression.Operations.Equal;
                            qe.PropertyValue = entityExtensionalOrder.INNERORDERID;

                            if (entityExtensionalOrder.APPLYTYPE.Equal(1))
                            {
                                qe.QueryType = typeof(T_FB_CHARGEAPPLYMASTER).Name;
                                qe.PropertyName = "CHARGEAPPLYMASTERID";
                            }
                            else
                            {
                                qe.QueryType = typeof(T_FB_BORROWAPPLYMASTER).Name;
                                qe.PropertyName = "BORROWAPPLYMASTERID";
                            }


                            FBEntity fbEntityOld = bll.FBEntityBLLGetFBEntity(qe);
                            if (fbEntityOld == null)
                            {
                                throw new Exception(string.Format("没有可用的内部单据可以操作，单据类型：{0}, 单据ID:{1}", qe.QueryType, qe.PropertyValue));
                            }
                            fbEntityOld.FBEntityState = FBEntityState.Modified;
                            //fbEntityOld.Entity.SetValue(FieldName.CheckStates, entityExtensionalOrder.GetValue("CHECKSTATES"));
                            #endregion

                            fbEntityInnerOrder = fbEntityOld;
                            break;
                        }

                }

                // 存在需要处理的内部单据，更新预算总帐，更新部门单据状态
                if (fbEntityInnerOrder != null)
                {
                    // if 事项审批 发起新流程
                    bool bIsOAApproval = false;
                    if (entityExtensionalOrder.T_FB_EXTENSIONALTYPE != null)
                    {
                        if (entityExtensionalOrder.T_FB_EXTENSIONALTYPE.EXTENSIONALTYPECODE == "SXSP")
                        {
                            bIsOAApproval = true;
                        }
                    }
                    using (AuditBLL abll = new AuditBLL())
                    {
                        // 判断是审核通过，并且是bIsOAApproval = true 时，提交预算流程
                        if (bIsOAApproval && cStates == CheckStates.Approved)
                        {
                            #region 事项审批流程设置
                            VirtualAudit va = new VirtualAudit();

                            //判断内部单据是借款还是报销，然后获取对应单据的数据填充到VirtualAudit
                            //APPLYTYPE:1为报销;2为借款
                            if (entityExtensionalOrder.APPLYTYPE == 1)
                            {
                                QueryExpression qeChargeApply = QueryExpression.Equal("CHARGEAPPLYMASTERID", entityExtensionalOrder.INNERORDERID);
                                qeChargeApply.QueryType = typeof(T_FB_CHARGEAPPLYMASTER).Name;
                                FBEntity fbChargeApply = QueryEntities(qeChargeApply).FirstOrDefault();
                                T_FB_CHARGEAPPLYMASTER entChargeApply = fbChargeApply.Entity as T_FB_CHARGEAPPLYMASTER;

                                if (entChargeApply != null)
                                {
                                    va.ModelCode = typeof(T_FB_CHARGEAPPLYMASTER).Name;
                                    va.FormID = entChargeApply.CHARGEAPPLYMASTERID;
                                    va.CREATECOMPANYID = entChargeApply.CREATECOMPANYID;
                                    va.CREATEDEPARTMENTID = entChargeApply.CREATEDEPARTMENTID;
                                    va.CREATEPOSTID = entChargeApply.CREATEPOSTID;
                                    va.CREATEUSERID = entChargeApply.CREATEUSERID;
                                    va.CREATEUSERNAME = entChargeApply.CREATEUSERNAME;
                                    va.Op = "ADD";
                                    va.UPDATEUSERID = entChargeApply.UPDATEUSERID;
                                    va.UPDATEUSERNAME = entChargeApply.UPDATEUSERNAME;
                                    va.Content = string.Empty;
                                    va.NextStateCode = "EndFlow";
                                    va.Result = 0;
                                    va.FlowSelectType = 1;

                                    var fbAudit = va.ToFBEntity();

                                    fbAudit.AddReferenceFBEntity<T_FB_CHARGEAPPLYMASTER>(fbEntityInnerOrder);

                                    abll.Audit(fbAudit);

                                }
                            }
                            else
                            {
                                QueryExpression qeBorrowApply = QueryExpression.Equal("BORROWAPPLYMASTERID", entityExtensionalOrder.INNERORDERID);
                                qeBorrowApply.QueryType = typeof(T_FB_BORROWAPPLYMASTER).Name;
                                FBEntity fbBorrowApply = QueryEntities(qeBorrowApply).FirstOrDefault();
                                T_FB_BORROWAPPLYMASTER entBorrowApply = fbBorrowApply.Entity as T_FB_BORROWAPPLYMASTER;

                                if (entBorrowApply != null)
                                {
                                    va.ModelCode = typeof(T_FB_BORROWAPPLYMASTER).Name;
                                    va.FormID = entBorrowApply.BORROWAPPLYMASTERID;
                                    va.CREATECOMPANYID = entBorrowApply.CREATECOMPANYID;
                                    va.CREATEDEPARTMENTID = entBorrowApply.CREATEDEPARTMENTID;
                                    va.CREATEPOSTID = entBorrowApply.CREATEPOSTID;
                                    va.CREATEUSERID = entBorrowApply.CREATEUSERID;
                                    va.CREATEUSERNAME = entBorrowApply.CREATEUSERNAME;
                                    va.Op = "ADD";
                                    va.UPDATEUSERID = entBorrowApply.UPDATEUSERID;
                                    va.UPDATEUSERNAME = entBorrowApply.UPDATEUSERNAME;
                                    va.NextStateCode = "EndFlow";
                                    va.Result = 0;
                                    va.FlowSelectType = 1;

                                    var fbAudit = va.ToFBEntity();

                                    fbAudit.AddReferenceFBEntity<T_FB_BORROWAPPLYMASTER>(fbEntityInnerOrder);

                                    abll.Audit(fbAudit);
                                }

                            }
                            #endregion
                        }
                        else if (!bIsOAApproval)
                        {
                            bll.InnerSave(fbEntityInnerOrder);
                            if (cStates != CheckStates.UnSubmit)
                            {
                                string msg = "";
                                AuditFBEntityWithoutFlow(fbEntityInnerOrder.Entity, cStates, ref msg);
                                if (msg != "")
                                {
                                    throw new FBBLLException(msg);
                                }
                                //BudgetAccountBLL bllAudit = new BudgetAccountBLL();
                                //bllAudit.UpdateAccount(fbEntityInnerOrder, cStates);
                            }
                        }
                    }
                }

                bll.CommitTransaction();
                return fbEntityQuery;
            }
            catch (Exception ex)
            {
                bll.RollbackTransaction();
                Tracer.Debug(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 生成内部单据编号
        /// </summary>
        /// <param name="entityExtensionalOrder"></param>
        private static void CreateInnerOrderCode(T_FB_EXTENSIONALORDER entityExtensionalOrder)
        {
            string innerOrderCode = string.Empty;
            string orderType = "CCBX";
            if (entityExtensionalOrder.T_FB_EXTENSIONALTYPEReference.EntityKey != null)
            {
                orderType = entityExtensionalOrder.T_FB_EXTENSIONALTYPE.EXTENSIONALTYPECODE;
            }

            // 出差申请单
            if (orderType == "CCBX")
            {
                T_FB_TRAVELEXPAPPLYMASTER master = new T_FB_TRAVELEXPAPPLYMASTER();
                SystemBLL.AddAutoOrderCode(master); // 自动生成编号                        
                innerOrderCode = master.TRAVELEXPAPPLYMASTERCODE;
            }
            else if (entityExtensionalOrder.APPLYTYPE.Equal(2))
            {
                T_FB_BORROWAPPLYMASTER master = new T_FB_BORROWAPPLYMASTER();
                SystemBLL.AddAutoOrderCode(master); // 自动生成编号                        
                innerOrderCode = master.BORROWAPPLYMASTERCODE;
            }
            else
            {
                T_FB_CHARGEAPPLYMASTER master = new T_FB_CHARGEAPPLYMASTER();
                SystemBLL.AddAutoOrderCode(master); // 自动生成编号                        
                innerOrderCode = master.CHARGEAPPLYMASTERCODE;
            }

            entityExtensionalOrder.INNERORDERCODE = innerOrderCode;
        }

        /// <summary>
        /// 获取临时单据的更新前的审核状态
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <returns></returns>
        private CheckStates GetOldStatesByExtension(T_FB_EXTENSIONALORDER entity)
        {
            CheckStates cStates = CheckStates.UnSubmit;
            if (entity == null)
            {
                throw new Exception("保存的数据不能为空");
            }

            if (entity.EntityKey == null)
            {
                return cStates;
            }
            FBEntityBLL bll = this;
            FBEntity fbOldEntity = GetFBEntityByEntityKey(entity.EntityKey);
            var master = fbOldEntity.Entity as T_FB_EXTENSIONALORDER;
            cStates = (CheckStates)((int)master.CHECKSTATES);
            return cStates;
        }

        private T_FB_EXTENSIONALORDER GetOldExOrder(T_FB_EXTENSIONALORDER entity)
        {
            if (entity == null)
            {
                throw new Exception("保存的数据不能为空");
            }

            QueryExpression qe = QueryExpression.Equal("EXTENSIONALORDERID", entity.EXTENSIONALORDERID);
            var result = qe.Query<T_FB_EXTENSIONALORDER>(this).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="entityExtensionalOrder"></param>
        /// <param name="fbEntityNew"></param>
        /// <returns></returns>
        private void SetNewFBEntityByExtensionOrder(FBEntityState setState, T_FB_EXTENSIONALORDER entityExtensionalOrder, string payInfo, List<FBEntity> masterList, ref FBEntity fbEntityNew)
        {
            if (entityExtensionalOrder.APPLYTYPE.Equal(1))
            {
                // 新增费用报销单
                T_FB_CHARGEAPPLYMASTER entity1 = new T_FB_CHARGEAPPLYMASTER();

                entity1.CHARGEAPPLYMASTERID = entityExtensionalOrder.INNERORDERID;

                entity1.BUDGETARYMONTH = System.DateTime.Now.Date;
                entity1.PAYTYPE = 1;
                entity1.CHARGEAPPLYMASTERCODE = entityExtensionalOrder.INNERORDERCODE;
                entity1.PAYMENTINFO = payInfo;
                entity1.REPAYMENT = 0;  //默认还款额为0，有冲借款明细单，则计算还款额
                fbEntityNew = entity1.ToFBEntity();



                QueryExpression qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, entityExtensionalOrder.OWNERCOMPANYID);


                QueryExpression qeDepartment = QueryExpression.Equal(FieldName.OwnerDepartmentID, entityExtensionalOrder.OWNERDEPARTMENTID);
                qeCompany.RelatedExpression = qeDepartment;

                QueryExpression qePost = QueryExpression.Equal(FieldName.OwnerPostID, entityExtensionalOrder.OWNERPOSTID);
                qeDepartment.RelatedExpression = qePost;

                QueryExpression qeOwner = QueryExpression.Equal(FieldName.OwnerID, entityExtensionalOrder.OWNERID);
                qePost.RelatedExpression = qeOwner;



                // 新增费用报销单明细
                List<FBEntity> listDetail = entityExtensionalOrder.T_FB_EXTENSIONORDERDETAIL.ToList().CreateList(item =>
                {
                    QueryExpression qe = new QueryExpression();
                    qe.RelatedExpression = qeCompany;
                    FBEntity entity = item.ToFBEntity();

                    ChangeExtenDetail(item.T_FB_SUBJECT.SUBJECTID, qe, ref entity);

                    T_FB_CHARGEAPPLYDETAIL detail = new T_FB_CHARGEAPPLYDETAIL();
                    detail.CHARGEAPPLYDETAILID = Guid.NewGuid().ToString();

                    detail.CHARGEMONEY = item.APPLIEDMONEY;
                    detail.CHARGETYPE = item.CHARGETYPE;
                    detail.CREATEDATE = DateTime.Now;
                    detail.CREATEUSERID = item.CREATEUSERID;
                    detail.REMARK = item.REMARK;
                    if (!item.T_FB_SUBJECTReference.IsLoaded)
                    {
                        item.T_FB_SUBJECTReference.Load();
                    }
                    detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                    detail.UPDATEUSERID = item.UPDATEUSERID;
                    detail.USABLEMONEY = item.USABLEMONEY;
                    detail.T_FB_CHARGEAPPLYMASTER = entity1;

                    FBEntity fbEntityDetail = detail.ToFBEntity();
                    fbEntityDetail.FBEntityState = FBEntityState.Added;
                    return fbEntityDetail;
                });
                fbEntityNew.AddFBEntities<T_FB_CHARGEAPPLYDETAIL>(listDetail);

                // 处理冲借款
                var masterFBEntity = masterList.FirstOrDefault();
                if (masterFBEntity != null)
                {
                    T_FB_CHARGEAPPLYMASTER master = masterFBEntity.Entity as T_FB_CHARGEAPPLYMASTER;
                    entity1.PAYTYPE = master.PAYTYPE;
                    List<FBEntity> listNewDetail = new List<FBEntity>();
                    decimal repayMoney = 0;
                    master.T_FB_CHARGEAPPLYREPAYDETAIL.ForEach(item =>
                    {
                        var detailBorrow = new T_FB_CHARGEAPPLYREPAYDETAIL();
                        detailBorrow.CHARGEAPPLYREPAYDETAILID = Guid.NewGuid().ToString();
                        detailBorrow.T_FB_CHARGEAPPLYMASTER = entity1;
                        detailBorrow.REPAYTYPE = item.REPAYTYPE;
                        detailBorrow.REMARK = item.REMARK;
                        detailBorrow.BORROWMONEY = item.BORROWMONEY;
                        detailBorrow.REPAYMONEY = item.REPAYMONEY;
                        detailBorrow.CREATEUSERID = entityExtensionalOrder.CREATEUSERID;
                        detailBorrow.CREATEDATE = System.DateTime.Now;
                        detailBorrow.UPDATEUSERID = entityExtensionalOrder.UPDATEUSERID;
                        detailBorrow.UPDATEDATE = System.DateTime.Now;
                        FBEntity fbEntityDetailBorrow = detailBorrow.ToFBEntity();
                        fbEntityDetailBorrow.FBEntityState = FBEntityState.Added;
                        listNewDetail.Add(fbEntityDetailBorrow);

                        repayMoney += detailBorrow.REPAYMONEY;
                    });

                    entity1.REPAYMENT = repayMoney;
                    fbEntityNew.AddFBEntities<T_FB_CHARGEAPPLYDETAIL>(listNewDetail);
                }
            }
            else
            {
                // 新增借款单
                T_FB_BORROWAPPLYMASTER entity2 = new T_FB_BORROWAPPLYMASTER();
                entity2.BORROWAPPLYMASTERID = entityExtensionalOrder.INNERORDERID;
                entity2.BORROWAPPLYMASTERCODE = entityExtensionalOrder.INNERORDERCODE;
                entity2.REPAYTYPE = 1;
                entity2.ISREPAIED = 0;
                entity2.PAYMENTINFO = payInfo;

                fbEntityNew = entity2.ToFBEntity();
                // 新增借款单明细
                List<FBEntity> listDetail = entityExtensionalOrder.T_FB_EXTENSIONORDERDETAIL.ToList().CreateList(item =>
                {
                    T_FB_BORROWAPPLYDETAIL detail = new T_FB_BORROWAPPLYDETAIL();
                    detail.BORROWAPPLYDETAILID = Guid.NewGuid().ToString();
                    detail.BORROWMONEY = item.APPLIEDMONEY;
                    detail.CHARGETYPE = item.CHARGETYPE;
                    detail.CREATEDATE = DateTime.Now;
                    detail.CREATEUSERID = item.CREATEUSERID;
                    detail.REMARK = item.REMARK;
                    detail.T_FB_SUBJECT = item.T_FB_SUBJECT;
                    detail.UPDATEUSERID = item.UPDATEUSERID;
                    detail.USABLEMONEY = item.USABLEMONEY;
                    detail.UNREPAYMONEY = item.APPLIEDMONEY;
                    detail.T_FB_BORROWAPPLYMASTER = entity2 as T_FB_BORROWAPPLYMASTER;
                    FBEntity fbEntityDetail = detail.ToFBEntity();
                    fbEntityDetail.FBEntityState = setState;
                    return fbEntityDetail;
                });
                fbEntityNew.AddFBEntities<T_FB_BORROWAPPLYDETAIL>(listDetail);
            }

            EntityObject newEntity = fbEntityNew.Entity;
            decimal dEditStates = 1;
            newEntity.SetValue("EDITSTATES", dEditStates);
            newEntity.SetValue("T_FB_EXTENSIONALORDER", entityExtensionalOrder);
            newEntity.SetValue("BANKACCOUT", entityExtensionalOrder.GetValue("BANKACCOUT"));
            newEntity.SetValue("BANK", entityExtensionalOrder.GetValue("BANK"));
            newEntity.SetValue("RECEIVER", entityExtensionalOrder.GetValue("RECEIVER"));
            newEntity.SetValue("PAYTARGET", entityExtensionalOrder.GetValue("PAYTARGET"));
            newEntity.SetValue("EDITSTATES", entityExtensionalOrder.GetValue("EDITSTATES"));
            newEntity.SetValue("TOTALMONEY", entityExtensionalOrder.GetValue("TOTALMONEY"));
            newEntity.SetValue("REMARK", entityExtensionalOrder.GetValue("REMARK"));

            newEntity.SetValue(FieldName.CreateCompanyID, entityExtensionalOrder.GetValue(FieldName.CreateCompanyID));

            newEntity.SetValue(FieldName.CreateDepartmentID, entityExtensionalOrder.GetValue(FieldName.CreateDepartmentID));
            newEntity.SetValue(FieldName.CreatePostID, entityExtensionalOrder.GetValue(FieldName.CreatePostID));
            newEntity.SetValue(FieldName.CreateUserID, entityExtensionalOrder.GetValue(FieldName.CreateUserID));

            newEntity.SetValue(FieldName.OwnerCompanyID, entityExtensionalOrder.GetValue(FieldName.OwnerCompanyID));
            newEntity.SetValue(FieldName.OwnerDepartmentID, entityExtensionalOrder.GetValue(FieldName.OwnerDepartmentID));
            newEntity.SetValue(FieldName.OwnerID, entityExtensionalOrder.GetValue(FieldName.OwnerID));
            newEntity.SetValue(FieldName.OwnerPostID, entityExtensionalOrder.GetValue(FieldName.OwnerPostID));

            newEntity.SetValue("CREATECOMPANYNAME", entityExtensionalOrder.GetValue("CREATECOMPANYNAME"));
            newEntity.SetValue("CREATEDEPARTMENTNAME", entityExtensionalOrder.GetValue("CREATEDEPARTMENTNAME"));
            newEntity.SetValue("CREATEPOSTNAME", entityExtensionalOrder.GetValue("CREATEPOSTNAME"));
            newEntity.SetValue("CREATEUSERNAME", entityExtensionalOrder.GetValue("CREATEUSERNAME"));

            newEntity.SetValue("OWNERCOMPANYNAME", entityExtensionalOrder.GetValue("OWNERCOMPANYNAME"));
            newEntity.SetValue("OWNERDEPARTMENTNAME", entityExtensionalOrder.GetValue("OWNERDEPARTMENTNAME"));
            newEntity.SetValue("OWNERNAME", entityExtensionalOrder.GetValue("OWNERNAME"));
            newEntity.SetValue("OWNERPOSTNAME", entityExtensionalOrder.GetValue("OWNERPOSTNAME"));

            newEntity.SetValue(FieldName.UpdateUserID, entityExtensionalOrder.GetValue(FieldName.UpdateUserID));
            fbEntityNew.FBEntityState = setState;
        }

        #endregion

        #region 审核
        /// <summary>
        /// 审核操作方法
        /// </summary>
        /// <param name="fBEntity"></param>
        /// <param name="cs"></param>
        public void AuditFBEntityWithoutFlow(EntityObject entity, CheckStates checkStates, ref string strMsg)
        {
            AuditResult result = new AuditResult();

            // 找单据ID
            string orderID = entity.GetOrderID();
            string entityType = entity.GetType().Name;
            if (LockOrder(orderID))
            {
                strMsg = "单据正在提交或审核中，不可重复操作！";
                return;
            }

            try
            {
                #region 找出DB里的实体审核状态
                CheckStates statesOld = CheckStates.UnSubmit;
                var dbEntity = entity.GetEntityInfo().Query(orderID, this);
                if (dbEntity != null)
                {
                    object checkStatesOld = dbEntity.GetValue(FieldName.CheckStates);
                    statesOld = (CheckStates)int.Parse(checkStatesOld.ToString());
                }
                #endregion

                if (checkStates == CheckStates.UnSubmit)
                {
                    strMsg = "此单据未提交审核";
                    return;
                }

                //if (statesOld == CheckStates.Approved || statesOld == CheckStates.UnApproved)
                //{
                //    strMsg = "单据已审核完毕，不可再次操作";
                //    return;
                //}

                //if (statesOld != CheckStates.UnSubmit && checkStates == CheckStates.Approving)
                //{
                //    strMsg = "单据已提交审核";
                //    return;
                //}


                #region 是否本月有结算

                bool isChecked = SystemBLL.IsChecked;
                // 没月结，只能处理报销。

                string[] EntityTypes = new string[] { typeof(T_FB_BORROWAPPLYMASTER).Name, 
                    typeof(T_FB_REPAYAPPLYMASTER).Name, typeof(T_FB_CHARGEAPPLYMASTER).Name, 
                    typeof(T_FB_TRAVELEXPAPPLYMASTER).Name};
                // 月结不可操作：1.日常报销类型的单据的审核或提交，2.所有单据的提交或重新提交
                if (!isChecked && (EntityTypes.Contains(entityType) || (checkStates == CheckStates.Approving)
                    || (checkStates == CheckStates.ReSubmit)))
                {
                    strMsg = "本月尚未结算,无法提交或审核!";
                    return;
                }

                #endregion

                #region 单据有效性判断

                if (!CheckAuditOrder(entity, statesOld, checkStates, ref strMsg))
                {
                    return;
                }
                #endregion


                try
                {
                    bool isJustCheck = true;
                    //在审核通过后才修改数据 2012版
                    if (checkStates == CheckStates.Approved || checkStates == CheckStates.UnApproved)
                    {
                        isJustCheck = false;
                    }
                    else if (entity is T_FB_CHARGEAPPLYMASTER && statesOld == CheckStates.UnSubmit && checkStates == CheckStates.Approving)
                    {
                        isJustCheck = false;
                    }

                    #region 更新预算总账

                    FBEntity fbEntitySave = entity.ToFBEntity();

                    fbEntitySave.Entity.SetValue(FieldName.CheckStates, Convert.ToDecimal((int)checkStates));
                    fbEntitySave.FBEntityState = FBEntityState.Modified;

                    // 提交时，才生成编号 2012版
                    //if (statesOld == CheckStates.UnSubmit && checkStates == CheckStates.Approving)
                    //{
                    //    if (entity is T_FB_CHARGEAPPLYMASTER)
                    //    {
                    //        T_FB_CHARGEAPPLYMASTER master = entity as T_FB_CHARGEAPPLYMASTER;
                    //        if (new string[] { ""," ", "   ", "    ", null, SystemBLL.DEFAULTORDERCODE }.Contains(master.CHARGEAPPLYMASTERCODE))
                    //        {
                    //            SystemBLL.AddAutoOrderCode(fbEntitySave.Entity); // 自动生成编号        
                    //        }
                    //    }
                    //    else
                    //    {
                    //        SystemBLL.AddAutoOrderCode(fbEntitySave.Entity); // 自动生成编号   
                    //    }
                    //}

                    // 保存业务表单
                    SaveResult sResult = FBCommSaveEntity(fbEntitySave);
                    if (sResult.Successful)
                    {
                        fbEntitySave = sResult.FBEntity;
                    }
                    else
                    {
                        //保存失败，抛出异常，触发回滚
                        throw new FBBLLException(sResult.Exception);
                    }

                    try
                    {
                        // 更新 预算总帐
                        UpdateAccount(fbEntitySave, checkStates, isJustCheck);
                    }
                    catch (BudgetAccountBBLException bbex)
                    {
                        string msg = bbex.Message;
                        switch (bbex.ErrorCode)
                        {
                            case AccountErrorCode.OverBorrowedMoney:
                                if (entityType == typeof(T_FB_REPAYAPPLYMASTER).Name)
                                {
                                    msg = "还款额度不能大于借款余额，请修改后再操作。";
                                }
                                else
                                {
                                    msg = "冲借款金额不能大于借款余额，请修改后再操作。";
                                }
                                break;
                            case AccountErrorCode.OverLimintedPersonMoney:
                                if (entityType == typeof(T_FB_DEPTBUDGETAPPLYMASTER).Name)
                                {
                                    msg = "月度预算金额不能大于部门月度预算最大额度限制，不能提交或审核通过。";
                                }
                                else if (entityType == typeof(T_FB_DEPTBUDGETADDMASTER).Name)
                                {

                                    msg = "月度预算增补金额不能大于部门月度预算最大额度限制，不能提交或审核通过。";
                                }
                                else
                                {
                                    msg = "月度预算汇总金额不能大于部门月度预算最大额度限制，不能提交或审核通过。";
                                }
                                break;
                            case AccountErrorCode.OverLimitedMonthMoney:
                                if (entityType == typeof(T_FB_DEPTBUDGETAPPLYMASTER).Name)
                                {
                                    msg = "个人月度预算金额不能大于个人月度预算最大额度限制，不能提交或审核通过。";
                                }
                                else if (entityType == typeof(T_FB_DEPTBUDGETADDMASTER).Name)
                                {

                                    msg = "个人月度预算增补金额不能大于个人月度预算最大额度限制，不能提交或审核通过。";
                                }
                                else
                                {
                                    msg = "个人月度预算汇总金额不能大于个人月度预算最大额度限制，不能提交或审核通过。";
                                }
                                break;
                            case AccountErrorCode.OverMonthMoney:
                                msg = bbex.Message;//"报销金额不能大于月度预算结余，请审核不通过。";
                                SystemBLL.Debug(msg);
                                break;
                            case AccountErrorCode.OverYearMoney:
                                if (entityType == typeof(T_FB_DEPTBUDGETAPPLYMASTER).Name)
                                {
                                    msg = "月度预算金额不能大于年度结余，请审核不通过。";
                                }
                                else if (entityType == typeof(T_FB_DEPTBUDGETADDMASTER).Name)
                                {
                                    msg = "月度预算增补金额不能大于年度结余，请审核不通过。";
                                }
                                else
                                {
                                    msg = "月度预算汇总金额不能大于年度结余，请审核不通过。";
                                }
                                break;
                            case AccountErrorCode.OverMonthTransferMoney:
                                msg = "分配金额不能大于月度结余，请审核不通过。";
                                break;
                        }

                        throw new FBBLLException(msg, bbex);
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    // 排除业务异常
                    //if (!typeof(FBBLLException).IsAssignableFrom(ex.GetType()))
                    //{
                    //    SystemBLL.Debug("审核异常：" + ex.ToString());
                    //}

                    SystemBLL.Debug("审核异常：" + ex.ToString());
                    strMsg = ex.Message;
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ReleaseOrder(orderID);
            }
        }

        #region 3.	审核操作方法

        private static bool LockOrder(string keyValue)
        {
            return LockHelper.LockOrder(keyValue);
        }
        private static bool ReleaseOrder(string keyValue)
        {
            return LockHelper.ReleaseOrder(keyValue);
        }



        #region 2011版, 暂时不用
        ///// <summary>
        ///// 审核操作方法
        ///// </summary>
        ///// <param name="fbEntity"></param>
        ///// <param name="checkStates">Approving:提交， Approved：审核通过, UnApproved:审核不通过</param>
        ///// <returns></returns>
        //public AuditResult AuditFBEntity(FBEntity fbEntity, CheckStates checkStates)
        //{

        //    AuditResult result = new AuditResult();

        //    EntityObject entity = fbEntity.ReferencedEntity[0].FBEntity.Entity;
        //    // 找单据ID
        //    string orderID = entity.GetOrderID();
        //    if (LockOrder(orderID))
        //    {
        //        result.Exception = "单据正在提交或审核中，不可重复操作！";
        //        return result;
        //    }

        //    try
        //    {
        //        #region 找出DB里的实体审核状态
        //        CheckStates statesOld = CheckStates.UnSubmit;
        //        var dbEntity = entity.GetEntityInfo().Query(orderID);
        //        if (dbEntity != null)
        //        {
        //            object checkStatesOld = dbEntity.GetValue(FieldName.CheckStates);
        //            statesOld = (CheckStates)int.Parse(checkStatesOld.ToString());
        //        }
        //        #endregion



        //        if ((statesOld == CheckStates.Approved || statesOld == CheckStates.UnApproved)
        //            && checkStates != CheckStates.ReSubmit)
        //        {
        //            result.Exception = "单据已审核完毕，不可再次操作";
        //            return result;
        //        }
        //        else if (statesOld == CheckStates.Approving && checkStates == CheckStates.Approving)
        //        {
        //            result.Exception = "单据已提交过，不可重复提交";
        //            return result;
        //        }

        //        #region 保存实体
        //        FBEntity fbEntitySave = null;
        //        // 先保存
        //        SaveResult sResult = Save(fbEntity.ReferencedEntity[0].FBEntity);
        //        if (sResult.Successful)
        //        {
        //            fbEntitySave = sResult.FBEntity;
        //        }
        //        else  // 保存不成功
        //        {
        //            result.Exception = sResult.Exception;
        //            result.Successful = false;
        //            return result;
        //        }
        //        result.FBEntity = fbEntitySave;
        //        #endregion

        //        #region 是否本月有结算

        //        bool isChecked = SystemBLL.IsChecked;
        //        // 没月结，只能处理报销。
        //        string entityType = entity.GetType().Name;
        //        string[] EntityTypes = new string[] { typeof(T_FB_BORROWAPPLYMASTER).Name, 
        //            typeof(T_FB_REPAYAPPLYMASTER).Name, typeof(T_FB_CHARGEAPPLYMASTER).Name, 
        //            typeof(T_FB_TRAVELEXPAPPLYMASTER).Name};
        //        // 月结不可操作：1.日常报销类型的单据的审核或提交，2.所有单据的提交或重新提交
        //        if (!isChecked && (EntityTypes.Contains(entityType) || (checkStates == CheckStates.Approving)
        //            || (checkStates == CheckStates.ReSubmit)))
        //        {
        //            result.Exception = "本月尚未结算,无法提交或审核!";
        //            return result;
        //        }

        //        #endregion

        //         #region 单据有效性判断
        //        string strMsg = "";
        //        if (!CheckAuditOrder(entity, statesOld, checkStates, ref strMsg))
        //        {
        //            result.Exception = strMsg;
        //            return result;
        //        }
        //         #endregion


        //        BudgetAccountBLL bll = new BudgetAccountBLL();
        //        AuditBLL auditBLL = new AuditBLL();

        //        try
        //        {

        //            BeginTransaction();
        //            auditBLL.Open();

        //            #region 审核
        //            // 更新 预算总帐
        //            //初次提交审核或是重新提交审核
        //            if ((checkStates == CheckStates.ReSubmit) || (checkStates == CheckStates.Approving))
        //            {
        //                checkStates = CheckStates.Approving;
        //                fbEntitySave.Entity.SetValue(FieldName.CheckStates, Convert.ToDecimal((int)checkStates));
        //                fbEntitySave.FBEntityState = FBEntityState.Modified;


        //                // 变更： 提交时，才生成编号
        //                if (statesOld == CheckStates.UnSubmit)
        //                {
        //                    SystemBLL.AddAutoOrderCode(fbEntitySave.Entity); // 自动生成编号
        //                }

        //                // 保存业务表单              
        //                sResult = Save(fbEntitySave);
        //                if (sResult.Successful)
        //                {
        //                    fbEntitySave = sResult.FBEntity;
        //                }
        //                bll.UpdateAccount(fbEntitySave, checkStates);

        //            }

        //            // 提交流程系统
        //            result.DataResult = auditBLL.Audit(fbEntity);

        //            if (result.DataResult.FlowResult == FlowWFService.FlowResult.END) // 流程结束
        //            {
        //                // 终审通过/不通过
        //                if (checkStates == CheckStates.Approved || checkStates == CheckStates.UnApproved)
        //                {
        //                    fbEntitySave.Entity.SetValue(FieldName.CheckStates, Convert.ToDecimal((int)checkStates));
        //                    fbEntitySave.FBEntityState = FBEntityState.Modified;



        //                    // 保存业务表单
        //                    sResult = Save(fbEntitySave);
        //                    if (sResult.Successful)
        //                    {
        //                        fbEntitySave = sResult.FBEntity;
        //                    }

        //                    // 更新 预算总帐
        //                    bll.UpdateAccount(fbEntitySave, checkStates);

        //                }
        //            }
        //            #endregion

        //            // 失败或出错，回滚
        //            if ((result.DataResult.FlowResult == FlowWFService.FlowResult.FAIL) || (result.DataResult.FlowResult == FlowWFService.FlowResult.MULTIUSER))
        //            {
        //                result.DataResult.Err = "流程异常： " + result.DataResult.Err;
        //                fbEntitySave.Entity.SetValue(FieldName.CheckStates, Convert.ToDecimal((int)statesOld));
        //                auditBLL.RollBack();
        //                RollbackTransaction();

        //            }
        //            else
        //            {
        //                result.FBEntity = sResult.FBEntity;
        //                auditBLL.Close();
        //                CommitTransaction();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            if (!typeof(FBBLLException).IsAssignableFrom(ex.GetType()))
        //            {
        //                SystemBLL.Debug("审核异常：" + ex.ToString());
        //            }

        //            result.Exception = ex.Message;
        //            auditBLL.RollBack();

        //            if (result.FBEntity != null)
        //            {
        //                result.FBEntity.Entity.SetValue(FieldName.CheckStates, Convert.ToDecimal((int)statesOld));
        //            }

        //            RollbackTransaction();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        ReleaseOrder(orderID);
        //    }
        //    return result;
        //}
        #endregion


        #region 2012版, 只做流程提交动作，流程再调业务系统处理数据加减等操作
        /// <summary>
        /// 审核操作方法
        /// </summary>
        /// <param name="fbEntity"></param>
        /// <param name="checkStates">Approving:提交， Approved：审核通过, UnApproved:审核不通过</param>
        /// <returns></returns>
        public AuditResult AuditFBEntity(FBEntity fbEntity, CheckStates checkStates)
        {

            AuditResult result = new AuditResult();

            try
            {


                EntityObject entity = fbEntity.ReferencedEntity[0].FBEntity.Entity;
                // 找单据ID
                string orderID = entity.GetOrderID();

                #region 找出DB里的实体审核状态
                CheckStates checkStatesOld = CheckStates.UnSubmit;
                var dbEntity = entity.GetEntityInfo().Query(orderID, this);
                if (dbEntity != null)
                {
                    object entityOld = dbEntity.GetValue(FieldName.CheckStates);
                    checkStatesOld = (CheckStates)int.Parse(entityOld.ToString());
                }
                #endregion



                if ((checkStatesOld == CheckStates.Approved || checkStatesOld == CheckStates.UnApproved)
                    && checkStates != CheckStates.ReSubmit)
                {
                    result.Exception = "单据已审核完毕，不可再次操作";
                    return result;
                }
                else if (checkStatesOld == CheckStates.Approving && checkStates == CheckStates.Approving)
                {
                    result.Exception = "单据已提交过，不可重复提交";
                    return result;
                }

                #region 特殊单据提交的单据的有效时间控制在上个月
                if (dbEntity != null)
                {
                    if (dbEntity.GetType().Name == "T_FB_DEPTBUDGETSUMMASTER" || dbEntity.GetType().Name == "T_FB_DEPTBUDGETAPPLYMASTER")
                    {
                        //T_FB_DEPTBUDGETSUMMASTER depSumEntity = (T_FB_DEPTBUDGETSUMMASTER)dbEntity;

                        object entityCreateDt = dbEntity.GetValue(FieldName.CreateDate);
                        DateTime dtCreateDt = (DateTime)entityCreateDt;

                        if (checkStates == CheckStates.Approving && checkStatesOld == CheckStates.UnSubmit)
                        {
                            int lastday = DateTime.DaysInMonth(dtCreateDt.Year, dtCreateDt.Month);
                            DateTime dtlastdate = new DateTime(dtCreateDt.Year, dtCreateDt.Month, lastday);

                            if (DateTime.Now.Date > dtlastdate)
                            {
                                result.Exception = "已超过汇总单最后提交期限: " + dtlastdate.ToString("yyyy-MM-dd");
                                return result;
                            }
                        }
                    }
                }
                // if ( currentCheckStates =
                #endregion

                #region 是否本月有结算

                bool isChecked = SystemBLL.IsChecked;
                // 没月结，只能处理报销。
                string entityType = entity.GetType().Name;
                string[] EntityTypes = new string[] { typeof(T_FB_BORROWAPPLYMASTER).Name, 
                    typeof(T_FB_REPAYAPPLYMASTER).Name, typeof(T_FB_CHARGEAPPLYMASTER).Name, 
                    typeof(T_FB_TRAVELEXPAPPLYMASTER).Name};
                // 月结不可操作：1.日常报销类型的单据的审核或提交，2.所有单据的提交或重新提交
                if (!isChecked && (EntityTypes.Contains(entityType) || (checkStates == CheckStates.Approving)
                    || (checkStates == CheckStates.ReSubmit)))
                {
                    result.Exception = "本月尚未结算,无法提交或审核!";
                    return result;
                }

                #endregion

                #region 单据有效性判断
                string strMsg = "";
                if (!CheckAuditOrder(entity, checkStatesOld, checkStates, ref strMsg))
                {
                    result.Exception = strMsg;
                    return result;
                }
                #endregion

                #region create order code
                // 提交时，才生成编号 2012版
                if (checkStatesOld == CheckStates.UnSubmit && checkStates == CheckStates.Approving)
                {
                    if (entity is T_FB_CHARGEAPPLYMASTER)
                    {
                        T_FB_CHARGEAPPLYMASTER master = entity as T_FB_CHARGEAPPLYMASTER;
                        if (new string[] { "", " ", "   ", "    ", null, SystemBLL.DEFAULTORDERCODE }.Contains(master.CHARGEAPPLYMASTERCODE))
                        {
                            SystemBLL.AddAutoOrderCode(fbEntity.ReferencedEntity[0].FBEntity.Entity); // 自动生成编号        
                        }
                    }
                    else
                    {
                        SystemBLL.AddAutoOrderCode(fbEntity.ReferencedEntity[0].FBEntity.Entity); // 自动生成编号   
                    }
                }

                #endregion

                #region 保存实体

                if (fbEntity.ReferencedEntity[0].FBEntity.FBEntityState == FBEntityState.Unchanged)
                {
                    fbEntity.ReferencedEntity[0].FBEntity.FBEntityState = FBEntityState.Modified;
                }

                FBEntity fbEntitySave = null;
                // 先保存
                SaveResult sResult = FBCommSaveEntity(fbEntity.ReferencedEntity[0].FBEntity);
                if (sResult.Successful)
                {
                    fbEntitySave = sResult.FBEntity;
                }
                else  // 保存不成功
                {
                    result.Exception = sResult.Exception;
                    result.Successful = false;
                    return result;
                }
                result.FBEntity = fbEntitySave;
                #endregion
                try
                {
                    CheckStates newAudiCheckStates = checkStatesOld;
                    #region 审核
                    // 更新 预算总帐
                    //初次提交审核或是重新提交审核
                    if ((checkStates == CheckStates.ReSubmit) || (checkStates == CheckStates.Approving))
                    {
                        newAudiCheckStates = CheckStates.Approving;
                    }

                    // 提交流程系统
                    using (AuditBLL bll = new AuditBLL())
                    {
                        result.DataResult = bll.Audit(fbEntity);
                    }
                    if (result.DataResult.FlowResult == FlowWFService.FlowResult.END) // 流程结束
                    {
                        // 终审通过/不通过
                        if (checkStates == CheckStates.Approved || checkStates == CheckStates.UnApproved)
                        {
                            newAudiCheckStates = checkStates;
                        }
                    }
                    #endregion

                    // 失败或出错，回滚
                    if ((result.DataResult.FlowResult == FlowWFService.FlowResult.FAIL) || (result.DataResult.FlowResult == FlowWFService.FlowResult.MULTIUSER))
                    {
                        result.DataResult.Err = "提交或审核失败： " + result.DataResult.Err;
                        newAudiCheckStates = checkStatesOld;
                    }

                    fbEntitySave.Entity.SetValue(FieldName.CheckStates, Convert.ToDecimal((int)newAudiCheckStates));
                    result.FBEntity = sResult.FBEntity;
                }
                catch (Exception ex)
                {
                    if (!typeof(FBBLLException).IsAssignableFrom(ex.GetType()))
                    {
                        SystemBLL.Debug("审核异常：" + ex.ToString());
                    }

                    result.Exception = ex.Message;


                    if (result.FBEntity != null)
                    {
                        result.FBEntity.Entity.SetValue(FieldName.CheckStates, Convert.ToDecimal((int)checkStatesOld));
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        #endregion
        public bool CheckAuditOrder(EntityObject entity, CheckStates oldStates, CheckStates newStates, ref string strMsg)
        {
            bool result = true;

            Type entityType = entity.GetType();


            if (entityType == typeof(T_FB_DEPTBUDGETADDMASTER))
            {
                #region T_FB_DEPTBUDGETADDMASTER 检查是否过期
                //去掉的原因：提交时不进行判断 2012-1-4
                //var entityNew = entity as T_FB_DEPTBUDGETADDMASTER;

                //DateTime BudgeMonth = entityNew.BUDGETARYMONTH;//预算年份、月份
                //DateTime DTNow = System.DateTime.Now;//当前系统时间                
                //DateTime OperDate = new DateTime();
                //DateTime DtUpdate = entityNew.UPDATEDATE;//修改的时间
                //if (entityNew.UPDATEDATE != null)
                //{
                //    //取当前月的最后一天 23:59:59
                //    OperDate = System.Convert.ToDateTime(entityNew.BUDGETARYMONTH.Year + "-" + entityNew.BUDGETARYMONTH.Month + "-" + entityNew.BUDGETARYMONTH.AddMonths(1).AddDays(-1).Day + " 23:59:59");
                //}
                //DateTime DTAdd = entityNew.CREATEDATE;//添加时间

                //if (newStates != CheckStates.UnApproved)
                //{
                //    if (DtUpdate.Year > BudgeMonth.Year)
                //    {
                //        strMsg = "年份已大于预算年份,无法提交或审核!";
                //        result = false;
                //    }
                //    if (DTNow > OperDate)
                //    {
                //        strMsg = "月份大于预算年月份,无法提交或审核!";
                //        result = false;
                //    }
                //}
                #endregion
            }
            else if (entityType == typeof(T_FB_DEPTBUDGETSUMMASTER))
            {
                T_FB_DEPTBUDGETSUMMASTER master = entity as T_FB_DEPTBUDGETSUMMASTER;

                #region T_FB_DEPTBUDGETAPPLYMASTER 提交的时间有效性判断
                if ((oldStates == CheckStates.ReSubmit || oldStates == CheckStates.UnApproved) && (newStates == CheckStates.Approving))
                {

                    if (System.DateTime.Now > master.BUDGETARYMONTH)
                    {
                        strMsg = string.Format("单据提交失败，单据的最大有效提交时间为: {0}", master.BUDGETARYMONTH.ToString("yyyy-MM-dd"));
                        result = false;
                    }

                }
                else if (newStates != CheckStates.UnApproved)
                {
                    var lastAuditDate = master.BUDGETARYMONTH.AddMonths(1);
                    if (System.DateTime.Now > lastAuditDate)
                    {
                        strMsg = string.Format("单据审核失败，单据的最大有效审核时间为{0}", lastAuditDate.ToString("yyyy-MM-dd"));
                        result = false;
                    }
                }
                #endregion
            }

            return result;
        }
        #endregion

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool FBcommonBllSaveList(List<FBEntity> fbEntityList)
        {
            try
            {

                return FBEntityBLLSaveList(fbEntityList);

                //FBEntityBLL bll = new FBEntityBLL();
                //return bll.Save(fbEntityList);
            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                // return FBEntity();
                throw ex;
            }
        }

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public SaveResult FBCommSaveEntity(FBEntity fbEntity)
        {

            SaveResult result = new SaveResult();
            try
            {
                result.FBEntity = base.SubjectBLLSave(fbEntity);
                result.Successful = true;
            }
            catch (FBBLLException ex)
            {
                result.Successful = false;
                result.Exception = ex.Message;
                SystemBLL.Debug(ex.ToString());

            }
            return result;
        }



        #endregion

    }
}
