using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_FB_EFModel;
using System.Data;

namespace SMT.FB.BLL
{
    public class BudgetSumBLL :FBEntityBLL
    {
        public const string SYSTEM_USER_ID = "001";
        public const string FieldName_BudgetYear = "BUDGETYEAR";
        public const string FieldName_BudgetMonth = "BUDGETARYMONTH";

        #region 1.	创建年度预算汇总主表和明细
        public void CreateCompanyBudgetSumDetail(T_FB_COMPANYBUDGETAPPLYMASTER entity)
        {
            T_FB_COMPANYBUDGETSUMDETAIL detail = new T_FB_COMPANYBUDGETSUMDETAIL();

            detail.COMPANYBUDGETSUMDETAILID = Guid.NewGuid().ToString();
            detail.CREATEUSERID = SYSTEM_USER_ID;
            detail.CREATEDATE = System.DateTime.Now;
            detail.UPDATEUSERID = SYSTEM_USER_ID;
            detail.UPDATEDATE = System.DateTime.Now;
            detail.T_FB_COMPANYBUDGETAPPLYMASTER = entity;
            FBEntity fbDetail = detail.ToFBEntity();
            fbDetail.FBEntityState = FBEntityState.Added;

            FBEntity fbSumMaster = new FBEntity();
            fbSumMaster = GetCompanyBudgetSum(entity);           
            detail.T_FB_COMPANYBUDGETSUMMASTER = fbSumMaster.Entity as T_FB_COMPANYBUDGETSUMMASTER;
            fbSumMaster.AddFBEntities<T_FB_COMPANYBUDGETSUMDETAIL>(new List<FBEntity> { fbDetail });
            this.InnerSave(fbSumMaster);
        }
        public FBEntity GetCompanyBudgetSum(T_FB_COMPANYBUDGETAPPLYMASTER entity)
        {
            FBEntity result = null;
            decimal? budgetYear = entity.BUDGETYEAR;
            QueryExpression qe = QueryExpression.Equal(FieldName_BudgetYear, budgetYear.Value.ToString());
            QueryExpression qeStates = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.UnSubmit).ToString());
            QueryExpression qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
            qe.RelatedExpression = qeStates;
            qeStates.RelatedExpression = qeCompany;

            T_FB_COMPANYBUDGETSUMMASTER sumMaster = this.InnerGetEntities<T_FB_COMPANYBUDGETSUMMASTER>(qe).FirstOrDefault();
            if (sumMaster == null)
            {
                sumMaster = new T_FB_COMPANYBUDGETSUMMASTER();
                sumMaster.COMPANYBUDGETSUMMASTERID = Guid.NewGuid().ToString();
                sumMaster.COMPANYBUDGETSUMMASTERCODE = "自动生成";
                sumMaster.CREATEUSERID = SYSTEM_USER_ID;
                sumMaster.CREATEUSERNAME = "系统生成";
                sumMaster.CREATEDATE = System.DateTime.Now;
                sumMaster.UPDATEUSERID = SYSTEM_USER_ID;
                sumMaster.UPDATEDATE = System.DateTime.Now;

                sumMaster.CREATECOMPANYID = SYSTEM_USER_ID;
                sumMaster.CREATECOMPANYNAME = "系统生成";
                sumMaster.CREATEDEPARTMENTID = SYSTEM_USER_ID;
                sumMaster.CREATEDEPARTMENTNAME = "系统生成";
                sumMaster.CREATEPOSTID = SYSTEM_USER_ID;
                sumMaster.CREATEPOSTNAME = "系统生成";              

                sumMaster.BUDGETYEAR = budgetYear;
                sumMaster.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                sumMaster.OWNERCOMPANYNAME = entity.OWNERCOMPANYNAME;

                sumMaster.OWNERDEPARTMENTID = SYSTEM_USER_ID;
                sumMaster.OWNERDEPARTMENTNAME = "系统生成";       

                sumMaster.OWNERPOSTID = SYSTEM_USER_ID;
                sumMaster.OWNERPOSTNAME = "系统生成";

                sumMaster.OWNERID = SYSTEM_USER_ID;
                sumMaster.OWNERNAME = "系统生成";

                sumMaster.SUMLEVEL = 0; //0：代表当前汇总单不走自定义汇总流程；1：代表当前汇总单走自定义汇总流程

                sumMaster.CHECKSTATES = (int)CheckStates.UnSubmit;
                sumMaster.EDITSTATES = (int)EditStates.Actived;
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Added;
            }
            else
            {
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Modified;
            }
            return result;
        }

        //自定义汇总
        public bool CreateCompanyBudgetSumSetMaster(FBEntity fbMaster)
        {
            bool bRes = false;
            T_FB_COMPANYBUDGETSUMMASTER Master = fbMaster.Entity as T_FB_COMPANYBUDGETSUMMASTER;
            foreach (var entity in Master.T_FB_COMPANYBUDGETSUMDETAIL)
            {
                T_FB_COMPANYBUDGETSUMDETAIL detail = new T_FB_COMPANYBUDGETSUMDETAIL();

                detail.COMPANYBUDGETSUMDETAILID = Guid.NewGuid().ToString();
                detail.CREATEUSERID = SYSTEM_USER_ID;
                detail.CREATEDATE = System.DateTime.Now;
                detail.UPDATEUSERID = SYSTEM_USER_ID;
                detail.UPDATEDATE = System.DateTime.Now;
                detail.T_FB_COMPANYBUDGETAPPLYMASTER = entity.T_FB_COMPANYBUDGETAPPLYMASTER;
                FBEntity fbDetail = detail.ToFBEntity();
                fbDetail.FBEntityState = FBEntityState.Added;

                List<FBEntity> fbSumMasterlist = new List<FBEntity>();

                //查找汇总节点设置 有则新增汇总记录
                FBEntity fbSumMaster = new FBEntity();
                QueryExpression qeDetail = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.T_FB_COMPANYBUDGETAPPLYMASTER.OWNERCOMPANYID).And(FieldName.EditStates, "1");
                qeDetail.QueryType = "T_FB_SUMSETTINGSDETAIL";
                T_FB_SUMSETTINGSDETAIL detailset = GetEntities<T_FB_SUMSETTINGSDETAIL>(qeDetail).FirstOrDefault();
                T_FB_SUMSETTINGSMASTER masterset = null;
                if (detailset != null)
                {
                    QueryExpression qeMaster = QueryExpression.Equal("SUMSETTINGSMASTERID", detailset.T_FB_SUMSETTINGSMASTERReference.EntityKey.EntityKeyValues[0].Value.ToString()).And(FieldName.EditStates, "1");
                    qeMaster.QueryType = "T_FB_SUMSETTINGSMASTER";
                    masterset = GetEntities<T_FB_SUMSETTINGSMASTER>(qeMaster).FirstOrDefault();
                    if (masterset != null)
                    {
                        fbSumMaster = GetCompanyBudgetSumSet(entity, masterset);
                        detail.T_FB_COMPANYBUDGETSUMMASTER = fbSumMaster.Entity as T_FB_COMPANYBUDGETSUMMASTER;
                        fbSumMaster.AddFBEntities<T_FB_COMPANYBUDGETSUMDETAIL>(new List<FBEntity> { fbDetail });
                        fbSumMasterlist.Add(fbSumMaster);
                        Master.PARENTID = detail.T_FB_COMPANYBUDGETSUMMASTER.COMPANYBUDGETSUMMASTERID;
                        Master.SUMSETTINGSMASTERID = masterset.SUMSETTINGSMASTERID;
                        Master.SUMLEVEL = 1; //0：代表当前汇总单不走自定义汇总流程；1：代表当前汇总单走自定义汇总流程
                        fbMaster.Entity = Master;
                        fbMaster.FBEntityState = FBEntityState.Modified;
                        fbSumMasterlist.Add(fbMaster);
                        this.FBEntityBLLSaveListNoTrans(fbSumMasterlist);
                       // this.InnerSave(fbSumMaster);
                    }
                }
            }

            bRes = true;
            return bRes;
          
        }

        public FBEntity GetCompanyBudgetSumSet(T_FB_COMPANYBUDGETSUMDETAIL entity, T_FB_SUMSETTINGSMASTER masterset)
        {
            FBEntity result = null;
            decimal? budgetYear = entity.T_FB_COMPANYBUDGETAPPLYMASTER.BUDGETYEAR;
            QueryExpression qe = QueryExpression.Equal(FieldName_BudgetYear, budgetYear.Value.ToString());
            QueryExpression qeStates = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.UnSubmit).ToString());
            QueryExpression qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, masterset.OWNERCOMPANYID);
           // QueryExpression qeSumsettings = QueryExpression.Equal("SUMSETTINGSMASTERID", masterset.SUMSETTINGSMASTERID).And("PARENTID", entity.T_FB_COMPANYBUDGETSUMMASTER.PARENTID);
            qe.RelatedExpression = qeStates;
            qeStates.RelatedExpression = qeCompany;
           // qeCompany.RelatedExpression = qeSumsettings;

            T_FB_COMPANYBUDGETSUMMASTER sumMaster = this.InnerGetEntities<T_FB_COMPANYBUDGETSUMMASTER>(qe).FirstOrDefault();
            if (sumMaster == null)
            {
                sumMaster = new T_FB_COMPANYBUDGETSUMMASTER();
                sumMaster.COMPANYBUDGETSUMMASTERID = Guid.NewGuid().ToString();
                sumMaster.COMPANYBUDGETSUMMASTERCODE = "自动生成";
                sumMaster.CREATEUSERID = SYSTEM_USER_ID;
                sumMaster.CREATEUSERNAME = "系统生成";
                sumMaster.CREATEDATE = System.DateTime.Now;
                sumMaster.UPDATEUSERID = SYSTEM_USER_ID;
                sumMaster.UPDATEDATE = System.DateTime.Now;

                sumMaster.CREATECOMPANYID = SYSTEM_USER_ID;
                sumMaster.CREATECOMPANYNAME = "系统生成";
                sumMaster.CREATEDEPARTMENTID = SYSTEM_USER_ID;
                sumMaster.CREATEDEPARTMENTNAME = "系统生成";
                sumMaster.CREATEPOSTID = SYSTEM_USER_ID;
                sumMaster.CREATEPOSTNAME = "系统生成";

                sumMaster.BUDGETYEAR = budgetYear;
                sumMaster.OWNERCOMPANYID = masterset.OWNERCOMPANYID;
                sumMaster.OWNERCOMPANYNAME = masterset.OWNERCOMPANYNAME;

                sumMaster.OWNERDEPARTMENTID = masterset.OWNERDEPARTMENTID;
                sumMaster.OWNERDEPARTMENTNAME = masterset.OWNERDEPARTMENTNAME;

                sumMaster.OWNERPOSTID = masterset.OWNERPOSTID;
                sumMaster.OWNERPOSTNAME = masterset.OWNERPOSTNAME;

                sumMaster.OWNERID = masterset.OWNERID; ;
                sumMaster.OWNERNAME = masterset.OWNERNAME;

                sumMaster.SUMSETTINGSMASTERID = masterset.SUMSETTINGSMASTERID;
                sumMaster.SUMLEVEL = 1; //0：代表当前汇总单不走自定义汇总流程；1：代表当前汇总单走自定义汇总流程

                sumMaster.CHECKSTATES = (int)CheckStates.UnSubmit;
                sumMaster.EDITSTATES = (int)EditStates.Actived;
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Added;
            }
            else
            {
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Modified;
            }
            return result;
        }
        #endregion 

        #region 2.	创建月度预算汇总主表和明细
        public void CreateDeptBudgetSumDetail(T_FB_DEPTBUDGETAPPLYMASTER entity)
        {
            T_FB_DEPTBUDGETSUMDETAIL detail = new T_FB_DEPTBUDGETSUMDETAIL();

            detail.DEPTBUDGETSUMDETAILID = Guid.NewGuid().ToString();
            detail.CREATEUSERID = SYSTEM_USER_ID;
            detail.CREATEDATE = System.DateTime.Now;
            detail.UPDATEUSERID = SYSTEM_USER_ID;
            detail.UPDATEDATE = System.DateTime.Now;
            detail.T_FB_DEPTBUDGETAPPLYMASTER = entity;
            FBEntity fbDetail = detail.ToFBEntity();
            fbDetail.FBEntityState = FBEntityState.Added;

            FBEntity fbSumMaster = new FBEntity();
            fbSumMaster = GetDeptBudgetSum(entity);           
            detail.T_FB_DEPTBUDGETSUMMASTER = fbSumMaster.Entity as T_FB_DEPTBUDGETSUMMASTER;
            fbSumMaster.AddFBEntities<T_FB_DEPTBUDGETSUMDETAIL>(new List<FBEntity> { fbDetail });
            this.InnerSave(fbSumMaster);
        }
        public FBEntity GetDeptBudgetSum(T_FB_DEPTBUDGETAPPLYMASTER entity)
        {
            FBEntity result = null;
            DateTime budgetMonth = entity.BUDGETARYMONTH;
            QueryExpression qe = QueryExpression.Equal(FieldName_BudgetMonth, budgetMonth.ToString("yyyy-MM-dd"));
            QueryExpression qeStates = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.UnSubmit).ToString());
            QueryExpression qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
            qe.RelatedExpression = qeStates;
            qeStates.RelatedExpression = qeCompany;

            T_FB_DEPTBUDGETSUMMASTER sumMaster = this.InnerGetEntities<T_FB_DEPTBUDGETSUMMASTER>(qe).FirstOrDefault();
            if (sumMaster == null)
            {
                sumMaster = new T_FB_DEPTBUDGETSUMMASTER();
                sumMaster.DEPTBUDGETSUMMASTERID = Guid.NewGuid().ToString();
                sumMaster.DEPTBUDGETSUMMASTERCODE = "自动生成";
                sumMaster.CREATEUSERID = SYSTEM_USER_ID;
                sumMaster.CREATEUSERNAME = "系统生成";
                sumMaster.CREATEDATE = System.DateTime.Now;
                sumMaster.UPDATEUSERID = SYSTEM_USER_ID;
                sumMaster.UPDATEDATE = System.DateTime.Now;
                sumMaster.BUDGETARYMONTH = budgetMonth;

                sumMaster.CREATECOMPANYID = SYSTEM_USER_ID;
                sumMaster.CREATECOMPANYNAME = "系统生成";
                sumMaster.CREATEDEPARTMENTID = SYSTEM_USER_ID;
                sumMaster.CREATEDEPARTMENTNAME = "系统生成";
                sumMaster.CREATEPOSTID = SYSTEM_USER_ID;
                sumMaster.CREATEPOSTNAME = "系统生成";         

                sumMaster.OWNERCOMPANYID = entity.OWNERCOMPANYID;
                sumMaster.OWNERCOMPANYNAME = entity.OWNERCOMPANYNAME;

                sumMaster.OWNERDEPARTMENTID = SYSTEM_USER_ID;
                sumMaster.OWNERDEPARTMENTNAME = "系统生成";

                sumMaster.OWNERPOSTID = SYSTEM_USER_ID;
                sumMaster.OWNERPOSTNAME = "系统生成";

                sumMaster.OWNERID = SYSTEM_USER_ID;
                sumMaster.OWNERNAME = "系统生成";

                sumMaster.SUMLEVEL = 0; //0：代表当前汇总单不走自定义汇总流程；1：代表当前汇总单走自定义汇总流程

                sumMaster.CHECKSTATES = (int)CheckStates.UnSubmit;
                sumMaster.EDITSTATES = (int)EditStates.Actived;
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Added;
            }
            else
            {
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Modified;
            }
            return result;
        }

        //自定义汇总
        public bool CreateDeptBudgetSumSetMaster(FBEntity fbMaster)
        {
            bool bRes = false;
            T_FB_DEPTBUDGETSUMMASTER Master = fbMaster.Entity as T_FB_DEPTBUDGETSUMMASTER;
            foreach (var entity in Master.T_FB_DEPTBUDGETSUMDETAIL)
            {
                T_FB_DEPTBUDGETSUMDETAIL detail = new T_FB_DEPTBUDGETSUMDETAIL();

                detail.DEPTBUDGETSUMDETAILID = Guid.NewGuid().ToString();
                detail.CREATEUSERID = SYSTEM_USER_ID;
                detail.CREATEDATE = System.DateTime.Now;
                detail.UPDATEUSERID = SYSTEM_USER_ID;
                detail.UPDATEDATE = System.DateTime.Now;
                detail.T_FB_DEPTBUDGETAPPLYMASTER = entity.T_FB_DEPTBUDGETAPPLYMASTER;
                FBEntity fbDetail = detail.ToFBEntity();
                fbDetail.FBEntityState = FBEntityState.Added;

                List<FBEntity> fbSumMasterlist = new List<FBEntity>();

                //查找汇总节点设置 有则新增汇总记录
                FBEntity fbSumMaster = new FBEntity();
                QueryExpression qeDetail = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.T_FB_DEPTBUDGETAPPLYMASTER.OWNERCOMPANYID);
                qeDetail.QueryType = "T_FB_SUMSETTINGSDETAIL";
                
                QueryExpression qeDetailEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                qeDetail.RelatedExpression = qeDetailEdits;

                T_FB_SUMSETTINGSDETAIL detailset = GetEntities<T_FB_SUMSETTINGSDETAIL>(qeDetail).FirstOrDefault();
                T_FB_SUMSETTINGSMASTER masterset = null;
                if (detailset != null)
                {
                    QueryExpression qeMaster = QueryExpression.Equal("SUMSETTINGSMASTERID", detailset.T_FB_SUMSETTINGSMASTERReference.EntityKey.EntityKeyValues[0].Value.ToString());
                    qeMaster.QueryType = "T_FB_SUMSETTINGSMASTER";

                    QueryExpression qeMasterEdits = QueryExpression.Equal(FieldName.EditStates, "1");
                    qeMaster.RelatedExpression = qeMasterEdits;

                    masterset = GetEntities<T_FB_SUMSETTINGSMASTER>(qeMaster).FirstOrDefault();
                    if (masterset != null)
                    {
                        fbSumMaster = GetDeptBudgetSumSet(entity, masterset);

                        detail.T_FB_DEPTBUDGETSUMMASTER = fbSumMaster.Entity as T_FB_DEPTBUDGETSUMMASTER;
                        fbSumMaster.AddFBEntities<T_FB_DEPTBUDGETSUMDETAIL>(new List<FBEntity> { fbDetail });
                        fbSumMasterlist.Add(fbSumMaster);
                        Master.PARENTID = detail.T_FB_DEPTBUDGETSUMMASTER.DEPTBUDGETSUMMASTERID;
                        Master.SUMSETTINGSMASTERID = masterset.SUMSETTINGSMASTERID;
                        Master.SUMLEVEL = 1; //0：代表当前汇总单不走自定义汇总流程；1：代表当前汇总单走自定义汇总流程
                        fbMaster.Entity = Master;
                        fbMaster.FBEntityState = FBEntityState.Modified;                       
                        fbSumMasterlist.Add(fbMaster);
                        this.FBEntityBLLSaveListNoTrans(fbSumMasterlist);
                      //  this.InnerSave(fbSumMaster);
                    }
                }
            }

            bRes = true;
            return bRes;
        }

        public FBEntity GetDeptBudgetSumSet(T_FB_DEPTBUDGETSUMDETAIL entity, T_FB_SUMSETTINGSMASTER masterset)
        {
            FBEntity result = null;
            DateTime budgetMonth = entity.T_FB_DEPTBUDGETAPPLYMASTER.BUDGETARYMONTH;
            QueryExpression qe = QueryExpression.Equal(FieldName_BudgetMonth, budgetMonth.ToString("yyyy-MM-dd"));
            QueryExpression qeStates = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.UnSubmit).ToString());
            QueryExpression qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, masterset.OWNERCOMPANYID);
           // QueryExpression qeSumsettings = QueryExpression.Equal("SUMSETTINGSMASTERID", masterset.SUMSETTINGSMASTERID).And("PARENTID", entity.T_FB_DEPTBUDGETSUMMASTER.PARENTID);
            qe.RelatedExpression = qeStates;
            qeStates.RelatedExpression = qeCompany;
           // qeCompany.RelatedExpression = qeSumsettings;

            T_FB_DEPTBUDGETSUMMASTER sumMaster = this.InnerGetEntities<T_FB_DEPTBUDGETSUMMASTER>(qe).FirstOrDefault();
            if (sumMaster == null)
            {
                sumMaster = new T_FB_DEPTBUDGETSUMMASTER();
                sumMaster.DEPTBUDGETSUMMASTERID = Guid.NewGuid().ToString();
                sumMaster.DEPTBUDGETSUMMASTERCODE = "自动生成";
                sumMaster.CREATEUSERID = SYSTEM_USER_ID;
                sumMaster.CREATEUSERNAME = "系统生成";
                sumMaster.CREATEDATE = System.DateTime.Now;
                sumMaster.UPDATEUSERID = SYSTEM_USER_ID;
                sumMaster.UPDATEDATE = System.DateTime.Now;
                sumMaster.BUDGETARYMONTH = budgetMonth;

                sumMaster.CREATECOMPANYID = SYSTEM_USER_ID;
                sumMaster.CREATECOMPANYNAME = "系统生成";
                sumMaster.CREATEDEPARTMENTID = SYSTEM_USER_ID;
                sumMaster.CREATEDEPARTMENTNAME = "系统生成";
                sumMaster.CREATEPOSTID = SYSTEM_USER_ID;
                sumMaster.CREATEPOSTNAME = "系统生成";

                sumMaster.OWNERCOMPANYID = masterset.OWNERCOMPANYID;
                sumMaster.OWNERCOMPANYNAME = masterset.OWNERCOMPANYNAME;

                sumMaster.OWNERDEPARTMENTID = masterset.OWNERDEPARTMENTID;
                sumMaster.OWNERDEPARTMENTNAME = masterset.OWNERDEPARTMENTNAME;

                sumMaster.OWNERPOSTID = masterset.OWNERPOSTID;
                sumMaster.OWNERPOSTNAME = masterset.OWNERPOSTNAME;

                sumMaster.OWNERID = masterset.OWNERID;
                sumMaster.OWNERNAME = masterset.OWNERNAME;

                sumMaster.SUMSETTINGSMASTERID = masterset.SUMSETTINGSMASTERID;
                sumMaster.SUMLEVEL = 1; //0：代表当前汇总单不走自定义汇总流程；1：代表当前汇总单走自定义汇总流程
               
                sumMaster.CHECKSTATES = (int)CheckStates.UnSubmit;
                sumMaster.EDITSTATES = (int)EditStates.Actived;
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Added;
            }
            else
            {
                result = sumMaster.ToFBEntity();
                result.FBEntityState = FBEntityState.Modified;
            }
            return result;
        }
        #endregion



    }
}
