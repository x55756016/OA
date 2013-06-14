using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.FB.DAL;
using SMT_FB_EFModel;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Xml.Linq;
using SMT.Foundation.Log;

namespace SMT.FB.BLL
{
    public class SubjectBLL : BudgetSumBLL
    {
        public string xmlOrderName = "T_FB_CHARGEAPPLYMASTER";
        public const decimal Max_Charge = 999999999999;
        public const decimal Max_Budget = 9999999999999;

        #region enum
        public const string FieldName_BudgetYear = "BUDGETYEAR";
        public const string FieldName_AccountObjectType = "ACCOUNTOBJECTTYPE";
        public const string SYSTEM_USER_ID = "001";
        public const int SYSTEM_SPECIAL_MONTH = 4;
        public enum AccountObjectType
        {
            Company = 1,
            Deaprtment,
            Person,
            Simple = 11,
            Backup,
            Special,
            /// <summary>
            /// 非当前记录
            /// </summary>
            Companynext = 20,
            /// <summary>
            /// 非当前记录
            /// </summary>
            Deptnext,
            /// <summary>
            /// 非当前记录
            /// </summary>
            Personnext

        }

        public enum AccountType
        {
            BudgetAccount, PersonAccount
        }
        /// <summary>
        /// 1: 还款申请单 ; 1: 费用申请单 ; 2:差旅报销申请单 ; 
        /// </summary>
        public enum OrderType
        {
            ReplayOrder = 1, ChargeOrder, TravelOrder
        }

        public enum AuditType
        {
            /// <summary>
            /// 无
            /// </summary>
            None,

            /// <summary>
            /// 提交
            /// </summary>
            Submit,

            /// <summary>
            /// 审核通过
            /// </summary>
            AuditPass,

            /// <summary>
            /// 审核不通过
            /// </summary>
            AuditFail
        }

        public enum AccountOpertaion
        {
            Add, Subtract, JustCheck
        }

        public enum ControlType
        {
            //1 : 不能跨月使用; 2 : 不那跨年使用 ; 3: 无限制 ; 4: 殊年结
            LimitYear = 1, LimitMonth, Unlimit, Special
        }
        #endregion

        #region 实体Item

        public class PersonAccountItem
        {
            public AccountOpertaion AccountOpertaion { get; set; }
            public string OwnerID { get; set; }
            public string OwnerDepartmentID { get; set; }
            public string OwnerCompanyID { get; set; }
            public string OwnerPostID { get; set; }
            public decimal? SIMPLEBORROWMONEY { get; set; }
            public decimal? SPECIALBORROWMONEY { get; set; }
            public decimal? BACKUPBORROWMONEY { get; set; }

            //　流水帐
            public decimal? OPERATIONMONEY { get; set; }
            public string orderDetailID { get; set; }
            public EntityObject MasterEntity { get; set; }

            public T_FB_WFPERSONACCOUNT CreateWaterFlow(T_FB_PERSONACCOUNT personAccount)
            {
                T_FB_WFPERSONACCOUNT wf = new T_FB_WFPERSONACCOUNT();
                wf.WFPERSONACCOUNTID = Guid.NewGuid().ToString();

                EntityInfo entityInfo = MasterEntity.GetEntityInfo();

                if (entityInfo != null)
                {
                    string orderID = Convert.ToString(MasterEntity.GetValue(entityInfo.KeyName));
                    string orderCode = Convert.ToString(MasterEntity.GetValue(entityInfo.CodeName));


                    wf.ORDERID = orderID;
                    wf.ORDERCODE = orderCode;
                    wf.ORDERTYPE = entityInfo.Type;
                }

                wf.ORDERDETAILID = orderDetailID;
                wf.OPERATIONMONEY = OPERATIONMONEY;
                wf.TRIGGERBY = "系统";
                wf.CREATEUSERID = "系统";
                wf.UPDATEUSERID = "系统";
                wf.CREATEDATE = System.DateTime.Now;
                wf.UPDATEDATE = System.DateTime.Now;


                wf.PERSONACCOUNTID = personAccount.PERSONACCOUNTID;
                wf.BORROWMONEY = personAccount.BORROWMONEY;
                wf.NEXTREPAYDATE = personAccount.NEXTREPAYDATE;
                wf.SPECIALBORROWMONEY = personAccount.SPECIALBORROWMONEY;
                wf.SIMPLEBORROWMONEY = personAccount.SIMPLEBORROWMONEY;
                wf.BACKUPBORROWMONEY = personAccount.BACKUPBORROWMONEY;
                wf.OWNERID = personAccount.OWNERID;
                wf.OWNERPOSTID = personAccount.OWNERPOSTID;
                wf.OWNERDEPARTMENTID = personAccount.OWNERDEPARTMENTID;
                wf.OWNERCOMPANYID = personAccount.OWNERCOMPANYID;
                wf.REMARK = personAccount.REMARK;
                wf.CREATECOMPANYID = personAccount.CREATECOMPANYID;
                wf.CREATEDEPARTMENTID = personAccount.CREATEDEPARTMENTID;
                wf.CREATEPOSTID = personAccount.CREATEPOSTID;

                return wf;
            }

            public T_FB_PERSONACCOUNT GetAccount()
            {
                using (BaseBLL bll = new BaseBLL())
                {
                    var master = this.MasterEntity;
                    QueryExpression qeOwnerID = QueryExpression.Equal(FieldName.OwnerID, OwnerID);
                    QueryExpression qeOwnerCompanyID = QueryExpression.Equal(FieldName.OwnerCompanyID, OwnerCompanyID);
                    qeOwnerID.RelatedExpression = qeOwnerCompanyID;
                    var personAccountList = qeOwnerID.Query<T_FB_PERSONACCOUNT>(bll);
                    var pAccount = personAccountList.FirstOrDefault();

                    if (pAccount == null)
                    {
                        throw new BudgetAccountBBLException(string.Format("找不到个人往来记录,OwnerID: {0}, OwnerCompanyID: {1}", OwnerID, OwnerCompanyID));
                    }
                    return pAccount;
                }
            }

            public Func<T_FB_PERSONACCOUNT, bool> CheckRule { get; set; }
        }

        public class AccountItem
        {
            public AccountItem()
            {
                moenyBudget = 0;
                moneyUsable = 0;
                moneyActual = 0;
                moneyPaid = 0;
            }
            public AccountOpertaion AccountOpertaion { get; set; }
            public AccountObjectType AccountType { get; set; }
            public T_FB_SUBJECT T_FB_SUBJECT { get; set; }
            public string OwnerID { get; set; }
            public string OwnerDepartmentID { get; set; }
            public string OwnerCompanyID { get; set; }
            public string OwnerPostID { get; set; }
            public decimal? moenyBudget { get; set; }
            public decimal? moneyUsable { get; set; }
            public decimal? moneyActual { get; set; }
            public decimal? moneyPaid { get; set; }

            public decimal? BudgetYear { get; set; }
            public decimal? BudgetMonth { get; set; }

            public Func<T_FB_BUDGETACCOUNT, CheckStates, bool> CheckRule { get; set; }

            #region 流水帐

            //　流水帐
            public decimal? OPERATIONMONEY { get; set; }
            public string orderDetailID { get; set; }
            public EntityObject MasterEntity { get; set; }

            public T_FB_WFBUDGETACCOUNT CreateWaterFlow(T_FB_BUDGETACCOUNT budgetAccount)
            {
                T_FB_WFBUDGETACCOUNT wf = new T_FB_WFBUDGETACCOUNT();

                EntityInfo entityInfo = MasterEntity.GetEntityInfo();

                if (entityInfo != null)
                {
                    string orderID = Convert.ToString(MasterEntity.GetValue(entityInfo.KeyName));
                    string orderCode = Convert.ToString(MasterEntity.GetValue(entityInfo.CodeName));


                    wf.ORDERID = orderID;
                    wf.ORDERCODE = orderCode;
                    wf.ORDERTYPE = entityInfo.Type;
                }

                wf.WFBUDGETACCOUNTID = Guid.NewGuid().ToString();
                wf.CREATEUSERID = "系统";
                wf.UPDATEUSERID = "系统";
                wf.CREATEDATE = System.DateTime.Now;
                wf.UPDATEDATE = System.DateTime.Now;

                wf.ORDERDETAILID = this.orderDetailID;
                wf.OPERATIONMONEY = this.OPERATIONMONEY;

                wf.BUDGETACCOUNTID = budgetAccount.BUDGETACCOUNTID;
                wf.ACCOUNTOBJECTTYPE = budgetAccount.ACCOUNTOBJECTTYPE;
                wf.BUDGETYEAR = budgetAccount.BUDGETYEAR;
                wf.BUDGETMONTH = budgetAccount.BUDGETMONTH;
                wf.OWNERCOMPANYID = budgetAccount.OWNERCOMPANYID;
                wf.OWNERDEPARTMENTID = budgetAccount.OWNERDEPARTMENTID;
                wf.OWNERID = budgetAccount.OWNERID;
                wf.OWNERPOSTID = budgetAccount.OWNERPOSTID;
                wf.SUBJECTID = this.T_FB_SUBJECT.SUBJECTID;
                wf.BUDGETMONEY = budgetAccount.BUDGETMONEY;
                wf.USABLEMONEY = budgetAccount.USABLEMONEY;
                wf.ACTUALMONEY = budgetAccount.ACTUALMONEY;
                wf.PAIEDMONEY = budgetAccount.PAIEDMONEY;


                return wf;
            }

            #endregion
        }

        #endregion

        #region 科目操作
        public List<T_FB_SUBJECT> GetSubject(QueryExpression queryExpression)
        {
            QueryExpression qe = new QueryExpression();
            qe.QueryType = "T_FB_SUBJECT";
            qe.PropertyName = "EDITSTATES";
            qe.PropertyValue = "1";
            qe.Operation = QueryExpression.Operations.Equal;
            qe.RelatedType = QueryExpression.RelationType.And;
            qe.RelatedExpression = queryExpression;
            qe.Include = new string[] { typeof(T_FB_SUBJECTTYPE).Name };
            //qe.IsCheckRight = false; 
            List<EntityObject> listSubject = this.BaseGetEntities(qe);
            List<T_FB_SUBJECT> listResult = listSubject.AsObjectList<T_FB_SUBJECT>();

            return listResult;

        }
        public List<T_FB_SUBJECTDEPTMENT> GetSubjectDepartment(QueryExpression queryExpression)
        {
            return InnerGetSubjectDepartment(queryExpression).ToList();
        }
        public IQueryable<T_FB_SUBJECTDEPTMENT> InnerGetSubjectDepartment(QueryExpression queryExpression)
        {
            QueryExpression qe = QueryExpression.Equal("ACTIVED", "1");
            
            QueryExpression qestates = QueryExpression.Equal("T_FB_SUBJECT.EDITSTATES", "1");
            qe.QueryType = typeof(T_FB_SUBJECTDEPTMENT).Name;            
            qe.RelatedExpression = qestates;
            qestates.RelatedExpression = queryExpression;

            //移除活动经费科目
            string MoneyAssign = SystemBLL.GetSetting(null).MONEYASSIGNSUBJECTID;
            var qeNonMoneyAssign = QueryExpression.NotEqual("T_FB_SUBJECT.SUBJECTID", MoneyAssign);
            qeNonMoneyAssign.RelatedExpression = qe;
            qe = qeNonMoneyAssign;

            qe.Include = new string[] { typeof(T_FB_SUBJECTCOMPANY).Name, typeof(T_FB_SUBJECT).Name };
            var listResult = this.InnerGetEntities<T_FB_SUBJECTDEPTMENT>(qe);
            listResult = listResult.OrderBy(item => item.T_FB_SUBJECT.SUBJECTCODE);
            return listResult;
        }

        public List<T_FB_SUBJECTCOMPANY> GetSubjectCompany(QueryExpression queryExpression)
        {
            QueryExpression qe = QueryExpression.Equal("ACTIVED", "1");
            QueryExpression qestates = QueryExpression.Equal("T_FB_SUBJECT.EDITSTATES", "1");
            qe.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name;
            qe.RelatedExpression = qestates;
            qestates.RelatedExpression = queryExpression;

            qe.Include = new string[] { "T_FB_SUBJECT.T_FB_SUBJECTTYPE" };
         
                List<EntityObject> listSubject = this.BaseGetEntities(qe);
                List<T_FB_SUBJECTCOMPANY> listResult = listSubject.AsObjectList<T_FB_SUBJECTCOMPANY>();

                listResult = listResult.OrderBy(item => item.T_FB_SUBJECT.SUBJECTCODE).ToList();

                return listResult;
            
        }

        public List<T_FB_SUBJECTPOST> GetSubjectPost(QueryExpression queryExpression)
        {
            QueryExpression qe = QueryExpression.Equal("ACTIVED", "1");
            QueryExpression qestates = QueryExpression.Equal("T_FB_SUBJECT.EDITSTATES", "1");
            qe.QueryType = typeof(T_FB_SUBJECTPOST).Name;
            qe.RelatedExpression = qestates;
            qestates.RelatedExpression = queryExpression;
            
            qe.Include = new string[] { "T_FB_SUBJECTDEPTMENT.T_FB_SUBJECTCOMPANY", typeof(T_FB_SUBJECT).Name };

            List<T_FB_SUBJECTPOST> listResult = this.GetEntities < T_FB_SUBJECTPOST>(qe);

            return listResult;
        }

        public List<FBEntity> GetSubjectCompany(VirtualCompany virtualCompany, List<T_FB_SUBJECT> listSubject)
        {

            string companyID = virtualCompany.ID;
            QueryExpression qeSubjectCompany = new QueryExpression();
            qeSubjectCompany.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name;
            qeSubjectCompany.PropertyName = "OWNERCOMPANYID";
            qeSubjectCompany.PropertyValue = companyID;
            qeSubjectCompany.Operation = QueryExpression.Operations.Equal;

            List<T_FB_SUBJECTCOMPANY> listSubjectCompany = this.GetEntities < T_FB_SUBJECTCOMPANY>(qeSubjectCompany);
                        
            // 去除subject 的父subject.否则会出现异常.
            listSubject.ForEach(item =>
            {
                item.T_FB_SUBJECT2 = null;                
            });
            List<T_FB_SUBJECTCOMPANY> listResult = new List<T_FB_SUBJECTCOMPANY>();

            listSubject.ForEach(subject =>
                {
                    
                   T_FB_SUBJECTCOMPANY curSC = listSubjectCompany.FirstOrDefault(sc =>
                        {
                            return sc.T_FB_SUBJECT.SUBJECTID == subject.SUBJECTID;
                        });
                   if (curSC == null)
                   {
                       curSC = new T_FB_SUBJECTCOMPANY();
                       curSC.T_FB_SUBJECT = subject;
                       curSC.SUBJECTCOMPANYID = Guid.NewGuid().ToString();
                       curSC.ACTIVED = 0;
                       curSC.ISMONTHADJUST = 0;
                       curSC.ISMONTHLIMIT = 1;
                       curSC.ISPERSON = 0;
                       curSC.ISYEARBUDGET = 1;
                       curSC.CONTROLTYPE = 1;

                       curSC.OWNERCOMPANYID = companyID;
                       curSC.OWNERDEPARTMENTID = QueryEntityBLL.SYSTEM_USER_ID;
                       curSC.OWNERPOSTID = QueryEntityBLL.SYSTEM_USER_ID;
                       curSC.OWNERID = QueryEntityBLL.SYSTEM_USER_ID;
                       curSC.EDITSTATES = 0;

                       if (subject.SUBJECTID == SystemBLL.GetSetting(null).MONEYASSIGNSUBJECTID)
                       {
                          //curSC.ACTIVED = 1;
                           curSC.ISPERSON = 1;
                           curSC.CONTROLTYPE = 3;
                       }
                   }
                   listResult.Add(curSC);
                });
           
            return listResult.ToFBEntityList();
        }

        /// <summary>
        ///  主查公司科目设置表 与公司科目分配表查询分开
        /// </summary>
        /// <param name="virtualCompany"></param>
        /// <param name="listSubject"></param>
        /// <returns></returns>
        public List<FBEntity> GetSubjectCompany_Company(VirtualCompany virtualCompany, List<T_FB_SUBJECT> listSubject)
        {
            string companyID = virtualCompany.ID;
            QueryExpression qeSubjectCompany = new QueryExpression();
            qeSubjectCompany.QueryType = typeof(T_FB_SUBJECTCOMPANY).Name;
            qeSubjectCompany.PropertyName = "OWNERCOMPANYID";
            qeSubjectCompany.PropertyValue = companyID;
            qeSubjectCompany.Operation = QueryExpression.Operations.Equal;

            List<T_FB_SUBJECTCOMPANY> listSubjectCompany = this.GetEntities<T_FB_SUBJECTCOMPANY>(qeSubjectCompany);
            
            return listSubjectCompany.ToFBEntityList();
        }


        public List<FBEntity> GetSubjectDepartment(VirtualDepartment virtualDepartment, List<T_FB_SUBJECTCOMPANY> listSubjectCompany)
        {
            string departmentID = virtualDepartment.ID;
            QueryExpression qeSubjectDepartment = new QueryExpression();
            qeSubjectDepartment.QueryType = typeof(T_FB_SUBJECTDEPTMENT).Name;
            qeSubjectDepartment.PropertyName = "OWNERDEPARTMENTID";
            qeSubjectDepartment.PropertyValue = departmentID;
            qeSubjectDepartment.Operation = QueryExpression.Operations.Equal;
            List<FBEntity> listOfSubjectDeparttment = GetFBEntities(qeSubjectDepartment);

            List<FBEntity> listResult = new List<FBEntity>();

            listSubjectCompany.ForEach(subjectCompany =>
            {

                FBEntity curFB = listOfSubjectDeparttment.FirstOrDefault(sd =>
                {
                    T_FB_SUBJECTDEPTMENT tempsd = sd.Entity as T_FB_SUBJECTDEPTMENT;
                    return tempsd.T_FB_SUBJECTCOMPANY.SUBJECTCOMPANYID == subjectCompany.SUBJECTCOMPANYID;
                });
                if (curFB == null)
                {
                    T_FB_SUBJECTDEPTMENT curSD = new T_FB_SUBJECTDEPTMENT();
                    curSD.T_FB_SUBJECTCOMPANY = subjectCompany;
                    curSD.T_FB_SUBJECT = subjectCompany.T_FB_SUBJECT;
                    curSD.SUBJECTDEPTMENTID = Guid.NewGuid().ToString();
                    curSD.ACTIVED = 0;
                    curSD.ISPERSON = 0;
                    curSD.EDITSTATES = 1;
                    curSD.LIMITBUDGEMONEY = 0;
                    curSD.OWNERDEPARTMENTID = departmentID;
                    curSD.OWNERCOMPANYID = subjectCompany.OWNERCOMPANYID;
                    curSD.OWNERPOSTID = QueryEntityBLL.SYSTEM_USER_ID;
                    curSD.OWNERID = QueryEntityBLL.SYSTEM_USER_ID;
                    curFB = curSD.ToFBEntity();

                    if (subjectCompany.T_FB_SUBJECT.SUBJECTID == SystemBLL.GetSetting(null).MONEYASSIGNSUBJECTID)
                    {
                        curSD.ACTIVED = 1;
                        curSD.ISPERSON = 1;
                        curFB.FBEntityState = FBEntityState.Modified;
                    }
                }
                listResult.Add(curFB);
            });

            return listResult;
        }

        #endregion


        #region 1.	查询单实体与及实体的所有参照实体的操作方法

        public FBEntity GetFBEntityByEntityKey(System.Data.EntityKey entityKey)
        {
            QueryExpression qe = QueryExpression.Equal(entityKey.EntityKeyValues[0].Key, entityKey.EntityKeyValues[0].Value.ToString());
            qe.QueryType = entityKey.EntitySetName;
            return GetFBEntityByExpression(qe);
        }

        /// <summary>
        /// 入口方法
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public FBEntity GetFBEntityByExpression(QueryExpression qe)
        {
            string returnType = qe.QueryType;
            Type type = this.GetType();
            MethodInfo method = type.GetMethod("GetEntity" + returnType);
            if (method != null)
            {
                object result = method.Invoke(this, new object[] { qe });
                return result as FBEntity;
            }
            else
            {
                return this.GetEntityDefault(qe);
            }
        }

        public FBEntity GetEntityDefault(QueryExpression qe)
        {
            return FBEntityBLLGetFBEntity(qe);
        }


        public FBEntity GetEntityT_FB_DEPTBUDGETSUMMASTER(QueryExpression qe)
        {
            //qe.Include = new string[] { "T_FB_DEPTBUDGETSUMDETAIL.T_FB_DEPTBUDGETAPPLYMASTER.T_FB_DEPTBUDGETAPPLYDETAIL" };
            FBEntity fbEntity = this.GetEntityDefault(qe);
            var master = fbEntity.Entity as T_FB_DEPTBUDGETSUMMASTER;
            decimal? totalMoney = 0;
            master.T_FB_DEPTBUDGETSUMDETAIL.ToList().ForEach(item =>
            {

                totalMoney = totalMoney.Add(item.T_FB_DEPTBUDGETAPPLYMASTER.BUDGETMONEY);
                var details = item.T_FB_DEPTBUDGETAPPLYMASTER.T_FB_DEPTBUDGETAPPLYDETAIL;
                details.Load();
                details.ToList().ForEach(detail =>
                {
                    detail.T_FB_SUBJECTReference.Load();
                });

            });
            master.BUDGETMONEY = totalMoney;
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_COMPANYBUDGETSUMMASTER(QueryExpression qe)
        {

            FBEntity fbEntity = this.GetEntityDefault(qe);

            var master = fbEntity.Entity as T_FB_COMPANYBUDGETSUMMASTER;
            decimal? totalMoney = 0;
            master.T_FB_COMPANYBUDGETSUMDETAIL.ToList().ForEach(item =>
            {
                totalMoney = totalMoney.Add(item.T_FB_COMPANYBUDGETAPPLYMASTER.BUDGETMONEY);

                var details = item.T_FB_COMPANYBUDGETAPPLYMASTER.T_FB_COMPANYBUDGETAPPLYDETAIL;
                details.Load();
                details.ToList().ForEach(detail =>
                {
                    detail.T_FB_SUBJECTReference.Load();
                });
            });
            master.BUDGETMONEY = totalMoney;
            return fbEntity;
        }


        public FBEntity GetEntityT_FB_COMPANYBUDGETAPPLYMASTER(QueryExpression qe)
        {
            FBEntity fbEntity = this.GetEntityDefault(qe);
            fbEntity.OrderDetailBy<T_FB_COMPANYBUDGETAPPLYDETAIL>(item => item.T_FB_SUBJECT.SUBJECTCODE);
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_COMPANYBUDGETMODMASTER(QueryExpression qe)
        {

            FBEntity fbEntity = this.GetEntityDefault(qe);
            fbEntity.OrderDetailBy<T_FB_COMPANYBUDGETMODDETAIL>(item => item.T_FB_SUBJECT.SUBJECTCODE);
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_COMPANYTRANSFERMASTER(QueryExpression qe)
        {

            FBEntity fbEntity = this.GetEntityDefault(qe);
            fbEntity.OrderDetailBy<T_FB_COMPANYTRANSFERDETAIL>(item => item.T_FB_SUBJECT.SUBJECTCODE);
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_DEPTBUDGETAPPLYMASTER(QueryExpression qe)
        {
            FBEntity fbEntity = this.GetEntityDefault(qe);
            fbEntity.OrderDetailBy<T_FB_DEPTBUDGETAPPLYDETAIL>(item => item.T_FB_SUBJECT.SUBJECTCODE);
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_DEPTBUDGETADDMASTER(QueryExpression qe)
        {

            FBEntity fbEntity = this.GetEntityDefault(qe);
            fbEntity.OrderDetailBy<T_FB_DEPTBUDGETADDDETAIL>(item => item.T_FB_SUBJECT.SUBJECTCODE);
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_DEPTTRANSFERMASTER(QueryExpression qe)
        {

            FBEntity fbEntity = this.GetEntityDefault(qe);
            T_FB_DEPTTRANSFERMASTER master = fbEntity.Entity as T_FB_DEPTTRANSFERMASTER;
            if (master.CHECKSTATES.Equals(0))
            {
                #region 未提交时,需要实时的数据
                var existItems = fbEntity.GetRelationFBEntities(typeof(T_FB_DEPTTRANSFERDETAIL).Name);



                // 调出单位类型
                QueryExpression qeTransferType = QueryExpression.Equal("ACCOUNTOBJECTTYPE", master.TRANSFERFROMTYPE.ToString());

                // 预算单位
                QueryExpression qeFrom = QueryExpression.Equal("TRANSFERFROM", master.TRANSFERFROM);
                qeFrom.RelatedExpression = qeTransferType;
                if (master.TRANSFERFROMTYPE.Equal(3)) // 个人
                {
                    qeFrom.PropertyName = FieldName.OwnerID;

                    // 岗位
                    QueryExpression qePost = QueryExpression.Equal(FieldName.OwnerPostID, master.TRANSFERFROMPOSTID);
                    qeFrom.RelatedExpression = qePost;
                }
                else
                {
                    qeFrom.PropertyName = FieldName.OwnerDepartmentID;
                }

                // 预算年份
                QueryExpression qeYear = QueryExpression.Equal("BUDGETYEAR", master.BUDGETARYMONTH.Year.ToString());
                qeYear.RelatedExpression = qeFrom;
                // 预算月份         
                QueryExpression qeMonth = QueryExpression.Equal("BUDGETMONTH", master.BUDGETARYMONTH.Month.ToString());
                qeMonth.RelatedExpression = qeYear;
                qeMonth.QueryType = typeof(T_FB_DEPTTRANSFERDETAIL).Name;
                var referenceItems = QueryT_FB_DEPTTRANSFERDETAIL(qeMonth);

                RefreshData(existItems, referenceItems,
                    (newItem, oldItem) =>
                    {
                        return (newItem.Entity as T_FB_DEPTTRANSFERDETAIL).T_FB_SUBJECT.SUBJECTID == (oldItem.Entity as T_FB_DEPTTRANSFERDETAIL).T_FB_SUBJECT.SUBJECTID;
                    },
                    (newItem, oldItem) =>
                    {
                        if (oldItem != null && newItem != null)
                        {
                            (newItem.Entity as T_FB_DEPTTRANSFERDETAIL).USABLEMONEY = (oldItem.Entity as T_FB_DEPTTRANSFERDETAIL).USABLEMONEY;
                        }
                        else if (oldItem == null && newItem != null)
                        {
                            (newItem.Entity as T_FB_DEPTTRANSFERDETAIL).USABLEMONEY = 0;
                        }
                        else if (oldItem != null && newItem == null)
                        {
                            existItems.Add(oldItem);
                            oldItem.FBEntityState = FBEntityState.Added;
                        }

                    });
                #endregion
            }
            fbEntity.OrderDetailBy<T_FB_DEPTTRANSFERDETAIL>(item => item.T_FB_SUBJECT.SUBJECTCODE);
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_CHARGEAPPLYMASTER(QueryExpression qe)
        {
            qe.Include = new string[] { "T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE" };
            FBEntity fbEntity = this.GetEntityDefault(qe);
            return fbEntity;
        }

        public FBEntity GetEntityT_FB_BORROWAPPLYMASTER(QueryExpression qe)
        {
            qe.Include = new string[] { "T_FB_EXTENSIONALORDER.T_FB_EXTENSIONALTYPE" };
            FBEntity fbEntity = this.GetEntityDefault(qe);
            return fbEntity;
        }

        #region GetEntities
        public List<FBEntity> GetEntities(QueryExpression qe)
        {
            string returnType = qe.QueryType;
            Type type = this.GetType();
            MethodInfo method = type.GetMethod("Get" + returnType);
            if (method != null)
            {
                object result = method.Invoke(this, new object[] { qe });
                return result as List<FBEntity>;
            }
            else
            {
                return this.GetDefaultEntities(qe);
            }
        }

        public List<FBEntity> GetDefaultEntities(QueryExpression qe)
        {
            return FBEntityBllGetFBEntities(qe);
        }

        #endregion
        #endregion

     

        #region Save FBEntityList

        public bool SaveEntityBLLSaveList(List<FBEntity> fbEntityList)
        {
            if (fbEntityList.Count == 0)
            {
                return true;
            }
            string returnType = fbEntityList[0].Entity.GetType().Name;
            Type type = this.GetType();
            MethodInfo method = type.GetMethod("SaveList" + returnType);
            if (method != null)
            {
                try
                {

                    object result = method.Invoke(this, new object[] { fbEntityList });
                    return (bool)result;
                }
                catch (Exception ex)
                {

                    throw ex.InnerException;
                }
            }
            else
            {
                return SaveFbEntityList(fbEntityList);
            }

        }

        public bool SaveFbEntityList(List<FBEntity> fbEntityList)
        {
            return SaveEntityBLLSaveList(fbEntityList);
        }

        public bool SaveListT_FB_SUMSETTINGSMASTER(List<FBEntity> fbEntityList)
        {
            fbEntityList.ForEach(item =>
            {
                T_FB_SUMSETTINGSMASTER Master = item.Entity as T_FB_SUMSETTINGSMASTER;
                if (Master.EDITSTATES == 0)
                {
                    QueryExpression qeID = QueryExpression.Equal("T_FB_SUMSETTINGSMASTER.SUMSETTINGSMASTERID", Master.SUMSETTINGSMASTERID);

                    qeID.QueryType = "T_FB_SUMSETTINGSDETAIL";
                    var result = GetFBEntities(qeID);
                    if (result != null)
                    {
                        List<FBEntity> fbEntity = result;
                        fbEntity.ForEach(p =>
                        {
                            //QueryExpression qeCompany = QueryExpression.Equal("SUMSETTINGSMASTERID", Master.SUMSETTINGSMASTERID);
                            //qeCompany.QueryType = "T_FB_COMPANYBUDGETSUMMASTER";
                            //var v = GetFBEntity(qeCompany);
                            //QueryExpression qeDept = QueryExpression.Equal("SUMSETTINGSMASTERID", Master.SUMSETTINGSMASTERID);
                            //qeDept.QueryType = "T_FB_DEPTBUDGETSUMMASTER";
                            //var q = GetFBEntity(qeCompany);

                            //if (v != null||q!=null)
                            //{
                            //    throw new FBBLLException("以下公司已经有汇总使用，不能删除！");
                            //}

                            p.FBEntityState = FBEntityState.Modified;
                            T_FB_SUMSETTINGSDETAIL detail = p.Entity as T_FB_SUMSETTINGSDETAIL;
                            detail.EDITSTATES = 0;
                            SaveEntityBLLSaveList(fbEntity);
                        });
                    }
                }
            });

            return SaveEntityBLLSaveList(fbEntityList);
        }

        /// <summary>
        /// 保存公司科目维护
        ///   级联的去除不可用的部门科目和岗位科目
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_SUBJECTCOMPANY(List<FBEntity> fbEntityList)
        {
            QueryExpression qeSCom = new QueryExpression();
            QueryExpression qeTop = qeSCom;
            string StrCompanyID = "";//公司ID
            bool IsExistPlus = false;
            // 找出没有设置年度预算而后又允许年度预算的
            List<T_FB_SUBJECTCOMPANY> inActivedlist = fbEntityList.CreateList(item =>
            {
                T_FB_SUBJECTCOMPANY entity = item.Entity as T_FB_SUBJECTCOMPANY;
                if (string.IsNullOrEmpty(StrCompanyID))
                {
                    StrCompanyID = entity.OWNERCOMPANYID;
                    QueryExpression qe = QueryExpression.Equal("SUBJECTCOMPANYID", entity.SUBJECTCOMPANYID);
                    var baData = this.InnerGetEntities<T_FB_SUBJECTCOMPANY>(qe);
                    if (baData.Count() > 0)
                    {
                        T_FB_SUBJECTCOMPANY OldSub = new T_FB_SUBJECTCOMPANY();
                        OldSub = baData.FirstOrDefault();
                        if (OldSub.ISYEARBUDGET == 0)
                        {
                            if (entity.ISYEARBUDGET == 1)
                            {
                                QueryExpression qeAccount = QueryExpression.Equal("OWNERCOMPANYID", entity.OWNERCOMPANYID);
                                QueryExpression qeAccount1 = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", entity.T_FB_SUBJECT != null ? entity.T_FB_SUBJECT.SUBJECTID : entity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString());
                                QueryExpression qeAccount2 = QueryExpression.Equal("ACCOUNTOBJECTTYPE", "1");
                                QueryExpression qeAccount3 = new QueryExpression();
                                qeAccount3.PropertyName = "USABLEMONEY";
                                qeAccount3.PropertyValue = "0";
                                qeAccount3.Operation = QueryExpression.Operations.LessThanOrEqual;
                                qeAccount3.Operation = QueryExpression.Operations.LessThan;//是否有问题
                                qeAccount.RelatedType = QueryExpression.RelationType.And;
                                qeAccount1.RelatedType = QueryExpression.RelationType.And;
                                qeAccount2.RelatedType = QueryExpression.RelationType.And;
                                qeAccount3.RelatedType = QueryExpression.RelationType.And;

                                qeAccount.RelatedExpression = qeAccount1;
                                qeAccount2.RelatedExpression = qeAccount1;
                                qeAccount3.RelatedExpression = qeAccount2;
                                qeAccount3.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;


                                //var baDataAccount = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeAccount);
                                //if(baDataAccount.Count() >0)
                                //{
                                //    //IsExistPlus= true;
                                //}
                            }
                        }
                    }
                }
                //return entity.ACTIVED != 1 ? entity : null;

                return entity;
            });
            if (IsExistPlus)
            {
                return IsExistPlus;
            }

            //var baData = this.InnerGetEntities<T_FB_SUBJECTCOMPANY>(qeDept);
            // 查出公司科目相关的部门科目及岗位科目
            inActivedlist.ForEach(item =>
            {
                qeTop.RelatedExpression = QueryExpression.Equal("T_FB_SUBJECTCOMPANY.SUBJECTCOMPANYID", item.SUBJECTCOMPANYID);
                qeTop.RelatedType = QueryExpression.RelationType.Or;
                qeTop = qeTop.RelatedExpression;
            });
            // 将部门科目及岗位科目置为不可用
            if (qeSCom.RelatedExpression != null)
            {
                qeSCom = qeSCom.RelatedExpression;
                qeSCom.Include = new string[] { "T_FB_SUBJECTPOST" };
                List<T_FB_SUBJECTDEPTMENT> inActiveDataList = GetEntities<T_FB_SUBJECTDEPTMENT>(qeSCom.RelatedExpression);
                inActiveDataList.ForEach(item =>
                {
                    item.ACTIVED = 0;
                    item.T_FB_SUBJECTPOST.ToList().ForEach(itemPost =>
                    {
                        itemPost.ACTIVED = 0;
                    });
                });
            }

            if (fbEntityList.Count > 0)
            {
                //记录公司部门科目设置修改流水
                SaveListT_FB_WFSUBJECTSETTING(fbEntityList, "1");
            }
            return SaveEntityBLLSaveList(fbEntityList);
        }


        /// <summary>
        ///   部门科目记录修改流水
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_SUBJECTDEPTMENT(List<FBEntity> fbEntityList)
        {
            if (fbEntityList.Count > 0)
            {
                //记录公司部门科目设置修改流水
                SaveListT_FB_WFSUBJECTSETTING(fbEntityList, "2");

                //修改部门启用时，同时更新岗位启用。
                fbEntityList.ForEach(item =>
                {
                    T_FB_SUBJECTDEPTMENT entity = item.Entity as T_FB_SUBJECTDEPTMENT;
                    if (entity != null && entity.ACTIVED == 0)
                    {
                        List<FBEntity> EntityListPost = new List<FBEntity>();
                        QueryExpression qe = QueryExpression.Equal("T_FB_SUBJECTDEPTMENT.SUBJECTDEPTMENTID", entity.SUBJECTDEPTMENTID);

                        List<T_FB_SUBJECTPOST> PostList = GetEntities<T_FB_SUBJECTPOST>(qe);
                        PostList.ForEach(p =>
                        {
                            FBEntity a = new FBEntity();
                            a.FBEntityState = FBEntityState.Modified;

                            p.ACTIVED = 0;//1 : 可用 ; 0 : 不可用

                            a.Entity = p;
                            a.EntityKey = null;
                            EntityListPost.Add(a);
                        });
                        SaveEntityBLLSaveList(EntityListPost);
                    }
                });
            }
            return SaveEntityBLLSaveList(fbEntityList);
        }

        /// <summary>
        ///   部门科目记录修改流水
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_SUBJECTPOST(List<FBEntity> fbEntityList)
        {
            if (fbEntityList.Count > 0)
            {
                //记录修改流水
                SaveListT_FB_WFSUBJECTSETTING(fbEntityList, "3");

                // 写死活动经费科目 可用；
                //string MoneyAssign = SystemBLL.etityT_FB_SYSTEMSETTINGS.MONEYASSIGNSUBJECTID;
                //fbEntityList.ForEach(item =>
                //{
                //    T_FB_SUBJECTPOST entity = item.Entity as T_FB_SUBJECTPOST;
                //    string strSubjectID = entity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                //    if (strSubjectID == MoneyAssign)
                //    {
                //        FBEntity a = new FBEntity();
                //        a.FBEntityState = FBEntityState.Modified;

                //        entity.ACTIVED = 1;//1 : 可用 ; 0 : 不可用

                //        a.Entity = entity;
                //        a.EntityKey = null;
                //        item = a;

                //        return;
                //    }
                //});
            }
            return SaveEntityBLLSaveList(fbEntityList);
        }


        /// <summary>        
        ///   保存科目设置流水
        /// </summary>
        /// <param name="fbEntityList"></param>
        /// <returns></returns>
        public bool SaveListT_FB_WFSUBJECTSETTING(List<FBEntity> fbEntityList, string strfig)
        {
            List<FBEntity> inActivedlist = fbEntityList.CreateList(item =>
            {
                T_FB_WFSUBJECTSETTING fbEntity = new T_FB_WFSUBJECTSETTING();

                if (strfig == "1")
                {
                    T_FB_SUBJECTCOMPANY SubjectEntity = item.Entity as T_FB_SUBJECTCOMPANY;

                    fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                    fbEntity.SUBJECTID = SubjectEntity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                    fbEntity.ACTIVED = SubjectEntity.ACTIVED;
                    fbEntity.ISMONTHADJUST = SubjectEntity.ISMONTHADJUST;
                    fbEntity.ISMONTHLIMIT = SubjectEntity.ISMONTHLIMIT;
                    fbEntity.ISPERSON = SubjectEntity.ISPERSON;
                    fbEntity.ISYEARBUDGET = SubjectEntity.ISYEARBUDGET;
                    fbEntity.CONTROLTYPE = SubjectEntity.CONTROLTYPE;
                    fbEntity.OWNERCOMPANYID = SubjectEntity.OWNERCOMPANYID;
                    fbEntity.OWNERCOMPANYNAME = SubjectEntity.OWNERCOMPANYNAME;
                    fbEntity.OWNERDEPARTMENTID = SubjectEntity.OWNERDEPARTMENTID;
                    fbEntity.OWNERDEPARTMENTNAME = SubjectEntity.OWNERDEPARTMENTNAME;
                    fbEntity.OWNERPOSTID = SubjectEntity.OWNERPOSTID;
                    fbEntity.OWNERPOSTNAME = SubjectEntity.OWNERPOSTNAME;
                    fbEntity.CREATEUSERID = SubjectEntity.CREATEUSERID;
                    fbEntity.UPDATEUSERID = SubjectEntity.UPDATEUSERID;
                    fbEntity.UPDATEDATE = DateTime.Now;
                    fbEntity.CREATEDATE = DateTime.Now;
                    fbEntity.ORDERTYPE = strfig;//1 公司 2部门 3岗位
                }
                else if (strfig == "2")
                {
                    T_FB_SUBJECTDEPTMENT SubjectEntity = item.Entity as T_FB_SUBJECTDEPTMENT;
                    if (SubjectEntity == null)
                    {
                        T_FB_SUBJECTPOST SubjectEntity1 = item.Entity as T_FB_SUBJECTPOST;

                        fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                        fbEntity.SUBJECTID = SubjectEntity1.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                        fbEntity.ACTIVED = SubjectEntity1.ACTIVED;
                        fbEntity.LIMITBUDGEMONEY = SubjectEntity1.LIMITBUDGEMONEY;
                        fbEntity.OWNERCOMPANYID = SubjectEntity1.OWNERCOMPANYID;
                        fbEntity.OWNERCOMPANYNAME = SubjectEntity1.OWNERCOMPANYNAME;
                        fbEntity.OWNERDEPARTMENTID = SubjectEntity1.OWNERDEPARTMENTID;
                        fbEntity.OWNERDEPARTMENTNAME = SubjectEntity1.OWNERDEPARTMENTNAME;
                        fbEntity.OWNERPOSTID = SubjectEntity1.OWNERPOSTID;
                        fbEntity.OWNERPOSTNAME = SubjectEntity1.OWNERPOSTNAME;
                        fbEntity.CREATEUSERID = SubjectEntity1.CREATEUSERID;
                        fbEntity.UPDATEUSERID = SubjectEntity1.UPDATEUSERID;
                        fbEntity.UPDATEDATE = DateTime.Now;
                        fbEntity.CREATEDATE = DateTime.Now;
                        fbEntity.ORDERTYPE = "3";//1 公司 2部门 3岗位
                    }
                    else
                    {
                        fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                        fbEntity.SUBJECTID = SubjectEntity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                        fbEntity.ACTIVED = SubjectEntity.ACTIVED;
                        fbEntity.LIMITBUDGEMONEY = SubjectEntity.LIMITBUDGEMONEY;
                        fbEntity.OWNERCOMPANYID = SubjectEntity.OWNERCOMPANYID;
                        fbEntity.OWNERCOMPANYNAME = SubjectEntity.OWNERCOMPANYNAME;
                        fbEntity.OWNERDEPARTMENTID = SubjectEntity.OWNERDEPARTMENTID;
                        fbEntity.OWNERDEPARTMENTNAME = SubjectEntity.OWNERDEPARTMENTNAME;
                        fbEntity.OWNERPOSTID = SubjectEntity.OWNERPOSTID;
                        fbEntity.OWNERPOSTNAME = SubjectEntity.OWNERPOSTNAME;
                        fbEntity.CREATEUSERID = SubjectEntity.CREATEUSERID;
                        fbEntity.UPDATEUSERID = SubjectEntity.UPDATEUSERID;
                        fbEntity.UPDATEDATE = DateTime.Now;
                        fbEntity.CREATEDATE = DateTime.Now;
                        fbEntity.ORDERTYPE = strfig;//1 公司 2部门 3岗位
                    }
                }
                else if (strfig == "3")
                {
                    T_FB_SUBJECTPOST SubjectEntity = item.Entity as T_FB_SUBJECTPOST;

                    fbEntity.WFSUBJECTSETTINGID = Guid.NewGuid().ToString();
                    fbEntity.SUBJECTID = SubjectEntity.T_FB_SUBJECTReference.EntityKey.EntityKeyValues[0].Value.ToString();
                    fbEntity.ACTIVED = SubjectEntity.ACTIVED;
                    fbEntity.LIMITBUDGEMONEY = SubjectEntity.LIMITBUDGEMONEY;
                    fbEntity.OWNERCOMPANYID = SubjectEntity.OWNERCOMPANYID;
                    fbEntity.OWNERCOMPANYNAME = SubjectEntity.OWNERCOMPANYNAME;
                    fbEntity.OWNERDEPARTMENTID = SubjectEntity.OWNERDEPARTMENTID;
                    fbEntity.OWNERDEPARTMENTNAME = SubjectEntity.OWNERDEPARTMENTNAME;
                    fbEntity.OWNERPOSTID = SubjectEntity.OWNERPOSTID;
                    fbEntity.OWNERPOSTNAME = SubjectEntity.OWNERPOSTNAME;
                    fbEntity.CREATEUSERID = SubjectEntity.CREATEUSERID;
                    fbEntity.UPDATEUSERID = SubjectEntity.UPDATEUSERID;
                    fbEntity.UPDATEDATE = DateTime.Now;
                    fbEntity.CREATEDATE = DateTime.Now;
                    fbEntity.ORDERTYPE = strfig;//1 公司 2部门 3岗位
                }
                FBEntity a = new FBEntity();
                a.Entity = fbEntity;
                a.FBEntityState = FBEntityState.Added;
                a.EntityKey = null;
                return a;
            });
            return SaveEntityBLLSaveList(inActivedlist);
        }
        #endregion

        #region Save SaveEntity
        public FBEntity SaveEntityBLLSave(SaveEntity saveEntity)
        {
            FBEntity fbEntity = saveEntity.FBEntity;
            FBEntity result = this.SubjectBLLSave(fbEntity);
            if (saveEntity.QueryExpression != null)
            {
                result = QueryEntities(saveEntity.QueryExpression).FirstOrDefault();
            }
            return result;
        }
        #endregion


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <remarks>
        ///    qe 中有一公共参数
        ///     IsGetFullData
        ///     true : 所有数据，明细表数据和可供选择的数据
        ///     false: 公获取明细表数据
        /// </remarks>
        /// <param name="qe">参数</param>
        /// <returns></returns>
        public List<FBEntity> QueryEntities(QueryExpression qe)
        {   
            string returnType = qe.QueryType;
            QueryExpression qeStates = qe.GetQueryExpression(FieldName.CheckStates);
            if (qeStates != null)
            {
                using (AuditBLL bll = new AuditBLL())
                {
                    // 待我审核的单据
                    if (qe.PropertyValue == ((int)CheckStates.WaittingApproval).ToString())
                    {
                        QueryExpression qeAuditedBy = QueryExpression.Equal(AuditBLL.NAME_AUDITEDBY, qe.VisitUserID);
                        qeAuditedBy.Pager = qe.Pager;
                        qeAuditedBy.OrderBy = qe.OrderBy;
                        qeAuditedBy.RelatedExpression = qe.RelatedExpression;   //待审核单据的查询条件需要带上UI层传递过来的除CheckState以外其他的条件
                        return bll.GetAuditedFBEntity(qeAuditedBy, returnType);
                    }
                    else if (string.IsNullOrEmpty(qe.PropertyValue)) // 所有单据
                    {
                        QueryExpression tempQE = null;
                        // 找待审核的单据
                        try
                        {
                            QueryExpression qeAuditedBy = QueryExpression.Equal(AuditBLL.NAME_AUDITEDBY, qe.VisitUserID);
                            List<QueryExpression> listAuditQE = bll.GetFlowQueryExpression(qeAuditedBy, returnType);
                            listAuditQE.ForEach(item =>
                            {
                                item.RelatedExpression = tempQE;
                                item.RelatedType = QueryExpression.RelationType.Or;
                                tempQE = item;
                            });
                        }
                        catch (Exception ex)
                        {
                            SystemBLL.Debug("找待审核的单据报错"+ex.ToString());
                        }
                        // 找所有状态的单据
                        QueryExpression qeEmpty = qe.RelatedExpression;
                        if (qeEmpty == null)
                        {
                            qeEmpty = new QueryExpression();
                        }
                        qeEmpty.IsUnCheckRight = false;
                        qeEmpty.QueryType = qe.QueryType;
                        qeEmpty.VisitAction = qe.VisitAction;
                        qeEmpty.VisitModuleCode = qe.VisitModuleCode;
                        qeEmpty.VisitUserID = qe.VisitUserID;
                        qeEmpty.Pager = qe.Pager;
                        if (tempQE != null)
                        {
                            qeEmpty.InnerQueryExpression = tempQE;
                            qeEmpty.InnerDataType = QueryExpression.InnerDataTypes.Attached;
                        }
                        List<FBEntity> listAll = QueryEntitiesByType(qeEmpty, returnType);
                        //// 找出相同的单据
                        //listAudit.ForEach(item =>
                        //{
                        //    var sameItem = listAll.FirstOrDefault(itemAll =>
                        //    {
                        //        return itemAll.Entity.EntityKey == item.Entity.EntityKey;
                        //    });
                        //    if (sameItem == null)
                        //    {
                        //        listAll.Add(item);
                        //    }
                        //});
                        return listAll;
                    }
                }
            }
            return QueryEntitiesByType(qe, returnType);
        }

        public List<FBEntity> QueryEntitiesByType(QueryExpression qe, string returnType)
        {
          
            Type type = this.GetType();
            MethodInfo method = type.GetMethod("Query" + returnType);
            if (method != null)
            {
                try
                {
                    if (returnType == "T_FB_EXTENSIONALORDER")
                    {
                         List<FBEntity> result = QueryT_FB_EXTENSIONALORDER(qe);
                         return result;
                    }
                    else
                    {
                        object result = method.Invoke(this, new object[] { qe });
                        return result as List<FBEntity>;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                return this.QueryDefault(qe);
            }
        }

        public List<FBEntity> QueryDefault(QueryExpression qe)
        {
            List<EntityObject> list = BaseGetEntities(qe);
            return list.ToFBEntityList();

        }

        public List<FBEntity> QueryT_FB_SUMSETTINGSMASTER(QueryExpression qe)
        {
            QueryExpression qeAdd = QueryExpression.Equal(FieldName.EditStates, "1");
            qeAdd.QueryType = "T_FB_SUMSETTINGSMASTER";
            qeAdd.RelatedExpression = qe;

            List<FBEntity> list = BaseGetEntities(qeAdd).ToFBEntityList();

            return list;

        }



        /// <summary>
        /// 2012年，只显示可用于个人预算的科目
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_DEPTTRANSFERDETAIL(QueryExpression qe)
        {
            List<FBEntity> listResult = new List<FBEntity>();

            // 17号修改的，未完成
            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID); // 部门ID
            string ownerDepartmentID = qeDept.PropertyValue;

            string moneyAssignSubjectID = SystemBLL.GetSetting(null).MONEYASSIGNSUBJECTID;

            var tableT_FB_SUBJECTPOST = GetTable<T_FB_SUBJECTPOST>();
            var tableT_FB_SUBJECTDEPTMENT = GetTable<T_FB_SUBJECTDEPTMENT>();
            var tableT_FB_BUDGETACCOUNT = GetTable<T_FB_BUDGETACCOUNT>();
            var tableT_FB_SUBJECT = GetTable<T_FB_SUBJECT>();

            var ListSubjectPost = from itemSPost in tableT_FB_SUBJECTPOST
                                  from itemSDept in tableT_FB_SUBJECTDEPTMENT
                                  where itemSDept.SUBJECTDEPTMENTID == itemSPost.T_FB_SUBJECTDEPTMENT.SUBJECTDEPTMENTID
                                  && itemSDept.ISPERSON.Value == 1
                                  && itemSPost.ISPERSON.Value == 1
                                  && itemSDept.OWNERDEPARTMENTID == ownerDepartmentID
                                  && itemSDept.T_FB_SUBJECT.SUBJECTID != moneyAssignSubjectID
                                  select new { itemSPost.T_FB_SUBJECT.SUBJECTID, itemSPost.OWNERPOSTID };


            var ListAccount = from itemAccount in tableT_FB_BUDGETACCOUNT
                              from itemSubject in tableT_FB_SUBJECT
                              where itemAccount.T_FB_SUBJECT.SUBJECTID == itemSubject.SUBJECTID
                              && itemSubject.T_FB_SUBJECTDEPTMENT.Any(itemD => itemD.OWNERDEPARTMENTID == ownerDepartmentID && itemD.ISPERSON.Value == 1)
                              && itemAccount.OWNERDEPARTMENTID == ownerDepartmentID
                              && (itemAccount.ACCOUNTOBJECTTYPE.Value == 2 || itemAccount.ACCOUNTOBJECTTYPE.Value == 3)
                              && itemAccount.T_FB_SUBJECT.SUBJECTID != moneyAssignSubjectID
                              select new { itemAccount, itemSubject };

            var accountList = ListAccount.ToList();

            var accountListDept = accountList.Where(item => item.itemAccount.ACCOUNTOBJECTTYPE.Value == 2).ToList();
            var accountListPerson = accountList.Where(item => item.itemAccount.ACCOUNTOBJECTTYPE.Value == 3).ToList();
            var subjectPostList = ListSubjectPost.ToList();
            var employeerList = new List<SaaS.BLLCommonServices.PersonnelWS.T_HR_EMPLOYEEPOST>();

            #region 人员信息
            SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient psc = new SaaS.BLLCommonServices.PersonnelWS.PersonnelServiceClient();
            var postIDs = ListSubjectPost.Select(item => item.OWNERPOSTID).Distinct().ToArray();

            postIDs.ForEach(item =>
            {
                var emps = psc.GetEmployeePostByPostID(item);
                if (emps != null)
                {
                    employeerList.AddRange(emps);
                }
            });
            #endregion

            accountListDept.ForEach(item =>
            {
                #region content

                #region 部门分派明细
                string subjectID = item.itemSubject.SUBJECTID;

                T_FB_DEPTTRANSFERDETAIL detail = new T_FB_DEPTTRANSFERDETAIL();
                detail.T_FB_SUBJECT = item.itemSubject;
                detail.DEPTTRANSFERDETAILID = Guid.NewGuid().ToString();
                detail.TRANSFERMONEY = 0;
                detail.USABLEMONEY = 0;
                detail.AUDITTRANSFERMONEY = 0;

                detail.USABLEMONEY = item.itemAccount.USABLEMONEY;          // 可用额度
                detail.AUDITTRANSFERMONEY = item.itemAccount.USABLEMONEY; // 可用结余


                var listPostIDs = subjectPostList.FindAll(itemSP => itemSP.SUBJECTID == subjectID).Select(itemItem => itemItem.OWNERPOSTID).ToList();

                #endregion
                // 加入个人预算補增申请明细
                #region 个人分派明细

                var listEmplyeers = employeerList.FindAll(itemE => listPostIDs.Contains(itemE.T_HR_POST.POSTID));

                List<T_FB_PERSONTRANSFERDETAIL> listPDetail = listEmplyeers.CreateList(itemE =>
                {
                    T_FB_PERSONTRANSFERDETAIL detailPerson = new T_FB_PERSONTRANSFERDETAIL();
                    detailPerson.PERSONTRANSFERDETAILID = Guid.NewGuid().ToString();
                    detailPerson.OWNERID = itemE.T_HR_EMPLOYEE.OWNERID;
                    detailPerson.OWNERNAME = itemE.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    detailPerson.OWNERPOSTID = itemE.T_HR_POST.POSTID;
                    detailPerson.OWNERPOSTNAME = itemE.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    detailPerson.T_FB_SUBJECT = item.itemSubject;
                    detailPerson.USABLEMONEY = BudgetAccountBLL.Max_Budget; // 此字段目前不用
                    detailPerson.T_FB_DEPTTRANSFERDETAIL = detail;

                    detailPerson.CREATEDATE = DateTime.Now;
                    detailPerson.UPDATEDATE = DateTime.Now;
                    detailPerson.CREATEUSERID = "";
                    detailPerson.UPDATEUSERID = "";
                    detailPerson.CREATEUSERNAME = "";
                    detailPerson.UPDATEUSERNAME = "";

                    var accountPerson = accountListPerson.Where(itemAP =>
                    {
                        return itemAP.itemSubject.SUBJECTID == subjectID && itemAP.itemAccount.ACCOUNTOBJECTTYPE.Equal((int)BudgetAccountBLL.AccountObjectType.Person)
                            && itemAP.itemAccount.OWNERID == detailPerson.OWNERID && itemAP.itemAccount.OWNERPOSTID == detailPerson.OWNERPOSTID;
                    }).FirstOrDefault();
                    if (accountPerson != null)
                    {
                        detailPerson.LIMITBUDGETMONEY = accountPerson.itemAccount.USABLEMONEY;
                    }
                    else
                    {
                        detailPerson.LIMITBUDGETMONEY = 0;
                    }

                    return detailPerson;
                });

                FBEntity detailEntity = detail.ToFBEntity();
                detailEntity.AddFBEntities<T_FB_PERSONTRANSFERDETAIL>(listPDetail.ToFBEntityList());
                #endregion 个人分派明细
                #endregion  content

                listResult.Add(detailEntity);
            });

            // 出除多余的关联
            listResult.ToEntityList<T_FB_DEPTTRANSFERDETAIL>().ForEach(item =>
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

            // end 17号修改的，未完成           

        }

        #region 刷新数据
        public void RefreshData<T>(List<T> existItems, List<T> referencetems, Func<T, T, bool> actionFind, Action<T, T> action) where T : EntityObject
        {

            existItems.ToList().ForEach(newItem =>
            {
                var itemFind = referencetems.FirstOrDefault(item =>
                {
                    return actionFind(newItem, item);
                });
                if (itemFind == null)
                {
                    action(newItem, null);
                }
            });

            referencetems.ToList().ForEach(item =>
            {
                var itemFind = existItems.FirstOrDefault(newItem =>
                {
                    return actionFind(newItem, item);
                });
                action(itemFind, item);
            });
        }

        #endregion

        #region Save FBEntity

        public FBEntity SubjectBllInnerSave(FBEntity fbEntity)
        {
            //暂时不特殊处理
            //if (fbEntity.FBEntityState == FBEntityState.ReSubmit)
            //{
            //    return ReSubmit(fbEntity);
            //}
            string returnType = fbEntity.Entity.GetType().Name;
            Type type = this.GetType();
            MethodInfo method = type.GetMethod("Save" + returnType);
            
            if (method != null)
            {
                try
                {
                    object result = method.Invoke(this, new object[] { fbEntity });
                    return result as FBEntity;
                }
                catch (Exception ex)
                {

                    throw ex.InnerException;
                }
            }
            else
            {
                return SaveFBEntityDefault(fbEntity);
            }
        }

        public FBEntity SubjectBLLSave(FBEntity fbEntity)
        {
            string orderid = "";
            try
            {
                orderid = fbEntity.Entity.GetOrderID();
                //if (orderid != null && LockHelper.LockOrder(orderid))
                //{
                //    throw new FBBLLException("单据已锁定，不能操作!");
                //}
                return SubjectBllInnerSave(fbEntity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                LockHelper.ReleaseOrder(orderid);
            }
        }

        public FBEntity SaveFBEntityDefault(FBEntity fbEntity)
        {
            if (base.FBEntityBllSave(fbEntity))
            {
                return GetFBEntityByEntityKey(fbEntity.Entity.EntityKey);
            }
            return null;
        }

        public FBEntity SaveT_FB_SUMSETTINGSMASTER(FBEntity fbEntity)
        {
            T_FB_SUMSETTINGSMASTER entity = fbEntity.Entity as T_FB_SUMSETTINGSMASTER;
            entity.T_FB_SUMSETTINGSDETAIL.ToList().ForEach(item =>
            {
                QueryExpression qeCompanyID = QueryExpression.Equal(FieldName.OwnerCompanyID, item.OWNERCOMPANYID);
                QueryExpression qeStates = QueryExpression.Equal("T_FB_SUMSETTINGSMASTER.EDITSTATES", "1");
                QueryExpression qeMasterID = QueryExpression.NotEqual("T_FB_SUMSETTINGSMASTER.SUMSETTINGSMASTERID", item.T_FB_SUMSETTINGSMASTER.SUMSETTINGSMASTERID);

                qeCompanyID.QueryType = "T_FB_SUMSETTINGSDETAIL";
                qeCompanyID.RelatedExpression = qeStates;
                qeStates.RelatedExpression = qeMasterID;

                var result = GetFBEntityByExpression(qeCompanyID);
                if (result != null)
                {
                    throw new FBBLLException("以下公司已经有汇总: " + item.OWNERCOMPANYNAME);
                }
            });
            return SaveFBEntityDefault(fbEntity);
        }

        public FBEntity SaveT_FB_DEPTBUDGETSUMMASTER(FBEntity fbEntity)
        {

            T_FB_DEPTBUDGETSUMMASTER entity = fbEntity.Entity as T_FB_DEPTBUDGETSUMMASTER;
   
            #region 审核中的单据
            QueryExpression qeCompanyID = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.CREATECOMPANYID);
            QueryExpression qeMonth = QueryExpression.Equal("BUDGETARYMONTH", entity.BUDGETARYMONTH.ToString("yyyy-MM-dd"));
            QueryExpression qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approving).ToString());

            qeCompanyID.RelatedExpression = qeMonth;
            qeMonth.RelatedExpression = qeStatesApproving;

            var result2 = InnerGetEntities<T_FB_DEPTBUDGETSUMMASTER>(qeCompanyID);
            if (result2.Count() > 0)
            {
                string departs = " ";
                result2.ToList().ForEach(item => { departs = departs.Trim() + "、" + item.OWNERDEPARTMENTNAME; });
                throw new FBBLLException("该月度还有以下部门的月度预算处于审核中: " + departs.Substring(1));
            }
            #endregion
            return SaveFBEntityDefault(fbEntity);
        }

        public FBEntity SaveT_FB_COMPANYBUDGETSUMMASTER(FBEntity fbEntity)
        {

            T_FB_COMPANYBUDGETSUMMASTER entity = fbEntity.Entity as T_FB_COMPANYBUDGETSUMMASTER;

            QueryExpression qeCompanyID = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.CREATECOMPANYID);
            QueryExpression qeYear = QueryExpression.Equal("BUDGETYEAR", entity.BUDGETYEAR.ToString());
            QueryExpression qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approving).ToString());

            qeCompanyID.RelatedExpression = qeYear;
            qeYear.RelatedExpression = qeStatesApproving;

            var result2 = InnerGetEntities<T_FB_COMPANYBUDGETAPPLYMASTER>(qeCompanyID);
            if (result2.Count() > 0)
            {
                string departs = " ";
                result2.ToList().ForEach(item => { departs = departs.Trim() + "、" + item.OWNERDEPARTMENTNAME; });
                throw new FBBLLException("该年度还有以下部门的年度预算处于审核中: " + departs.Substring(1));
            }
            return SaveFBEntityDefault(fbEntity);
        }

        public FBEntity SaveT_FB_DEPTBUDGETAPPLYMASTER(FBEntity fbEntity)
        {

            T_FB_DEPTBUDGETAPPLYMASTER entity = fbEntity.Entity as T_FB_DEPTBUDGETAPPLYMASTER;
            entity.BUDGETARYMONTH = new DateTime(entity.BUDGETARYMONTH.Year, entity.BUDGETARYMONTH.Month, 1);
            DateTime bDate = entity.BUDGETARYMONTH;


            #region 审核中
            QueryExpression qeID = QueryExpression.Equal("DEPTBUDGETAPPLYMASTERID", entity.DEPTBUDGETAPPLYMASTERID);
            qeID.Operation = QueryExpression.Operations.NotEqual;
            QueryExpression qeBUDGETARYMONTH = QueryExpression.Equal("BUDGETARYMONTH", bDate.ToString("yyyy-MM-dd"));
            qeID.RelatedExpression = qeBUDGETARYMONTH;
            QueryExpression qeDept = QueryExpression.Equal(FieldName.OwnerDepartmentID, entity.OWNERDEPARTMENTID);
            qeBUDGETARYMONTH.RelatedExpression = qeDept;
            QueryExpression qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approving).ToString());
            qeDept.RelatedExpression = qeStatesApproving;

            var result = InnerGetEntities<T_FB_DEPTBUDGETAPPLYMASTER>(qeID);
            if (result.Count() > 0)
            {
                throw new FBBLLException("该部门月度预算正在审核中");
            }
            #endregion

            #region 审核通过
            qeID = QueryExpression.Equal("DEPTBUDGETAPPLYMASTERID", entity.DEPTBUDGETAPPLYMASTERID);
            qeID.Operation = QueryExpression.Operations.NotEqual;
            qeBUDGETARYMONTH = QueryExpression.Equal("BUDGETARYMONTH", bDate.ToString("yyyy-MM-dd"));
            qeID.RelatedExpression = qeBUDGETARYMONTH;
            qeDept = QueryExpression.Equal(FieldName.OwnerDepartmentID, entity.OWNERDEPARTMENTID);
            qeBUDGETARYMONTH.RelatedExpression = qeDept;
            qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            qeDept.RelatedExpression = qeStatesApproving;
            QueryExpression qeEDITSTATES = QueryExpression.Equal("ISVALID", "0");
            qeStatesApproving.RelatedExpression = qeEDITSTATES;

            result = InnerGetEntities<T_FB_DEPTBUDGETAPPLYMASTER>(qeID);
            if (result.Count() > 0)
            {
                throw new FBBLLException("该部门月度预算已审核通过");
            }
            #endregion

            #region 审核通过
            qeID = QueryExpression.Equal("DEPTBUDGETAPPLYMASTERID", entity.DEPTBUDGETAPPLYMASTERID);
            qeID.Operation = QueryExpression.Operations.NotEqual;
            qeBUDGETARYMONTH = QueryExpression.Equal("BUDGETARYMONTH", bDate.ToString("yyyy-MM-dd"));
            qeID.RelatedExpression = qeBUDGETARYMONTH;
            qeDept = QueryExpression.Equal(FieldName.OwnerDepartmentID, entity.OWNERDEPARTMENTID);
            qeBUDGETARYMONTH.RelatedExpression = qeDept;
            //qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            //qeDept.RelatedExpression = qeStatesApproving;
            qeEDITSTATES = QueryExpression.Equal("ISVALID", "1");
            qeDept.RelatedExpression = qeEDITSTATES;

            result = InnerGetEntities<T_FB_DEPTBUDGETAPPLYMASTER>(qeID);
            if (result.Count() > 0)
            {
                throw new FBBLLException("该部门月度预算已生效");
            }
            #endregion

            #region 月度汇总
            QueryExpression qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
            qeBUDGETARYMONTH = QueryExpression.Equal("BUDGETARYMONTH", bDate.ToString("yyyy-MM-dd"));
            QueryExpression qeStatesApproved = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            QueryExpression qeSumLevel = QueryExpression.Equal("SUMLEVEL", "0");
            qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approving).ToString());
            qeStatesApproved.RelatedExpression = qeStatesApproving;
            qeStatesApproved.RelatedType = QueryExpression.RelationType.Or;
            qeSumLevel.RelatedExpression = qeStatesApproved;
            qeBUDGETARYMONTH.RelatedExpression = qeSumLevel;
            qeCompany.RelatedExpression = qeBUDGETARYMONTH;

            var result2 = InnerGetEntities<T_FB_DEPTBUDGETSUMMASTER>(qeCompany);
            if (result2.Count() > 0 && entity.CHECKSTATES != (int)CheckStates.UnApproved)
            {
                throw new FBBLLException("该月度已做过月度汇总或正在审核中");
            }
            #endregion



            return SaveFBEntityDefault(fbEntity);
        }

        public FBEntity SaveT_FB_DEPTBUDGETADDMASTER(FBEntity fbEntity)
        {

            T_FB_DEPTBUDGETADDMASTER entity = fbEntity.Entity as T_FB_DEPTBUDGETADDMASTER;
            entity.BUDGETARYMONTH = new DateTime(entity.BUDGETARYMONTH.Year, entity.BUDGETARYMONTH.Month, 1);
            if (entity.BUDGETCHARGE <= 0)
            {
                throw new Exception("费用总预算必须大于0");
            }
            //DateTime bDate = entity.BUDGETARYMONTH;

            //QueryExpression qeDept = QueryExpression.Equal(FieldName.OwnerDepartmentID, entity.OWNERDEPARTMENTID);
            //QueryExpression qe = QueryExpression.Equal("BUDGETARYMONTH", bDate.ToString("yyyy-MM-dd"));
            //QueryExpression qeStatesApproved = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());

            //qeDept.RelatedExpression = qe;
            //qe.RelatedExpression = qeStatesApproved;


            //var result = InnerGetEntities<T_FB_DEPTBUDGETAPPLYMASTER>(qeDept);
            //if (result.Count() == 0)
            //{
            //    throw new FBBLLException("该部门尚未做过月度预算");
            //}

            return SaveFBEntityDefault(fbEntity);
        }

        public FBEntity SaveT_FB_COMPANYBUDGETAPPLYMASTER(FBEntity fbEntity)
        {

            #region 是否存在审核中
            T_FB_COMPANYBUDGETAPPLYMASTER entity = fbEntity.Entity as T_FB_COMPANYBUDGETAPPLYMASTER;
            //entity.BUDGETYEAR = DateTime.Now.Year;
            QueryExpression qeID = QueryExpression.Equal("COMPANYBUDGETAPPLYMASTERID", entity.COMPANYBUDGETAPPLYMASTERID);
            qeID.Operation = QueryExpression.Operations.NotEqual;
            QueryExpression qeBUDGETYEAR = QueryExpression.Equal("BUDGETYEAR", entity.BUDGETYEAR.ToString());
            qeID.RelatedExpression = qeBUDGETYEAR;
            QueryExpression qeCom = QueryExpression.Equal("OWNERDEPARTMENTID", entity.OWNERDEPARTMENTID);
            qeBUDGETYEAR.RelatedExpression = qeCom;
            QueryExpression qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approving).ToString());
            qeCom.RelatedExpression = qeStatesApproving;

            var result = InnerGetEntities<T_FB_COMPANYBUDGETAPPLYMASTER>(qeID);
            if (result.Count() > 0)
            {
                throw new FBBLLException("该部门年度预算正在审核中");
            }
            #endregion

            #region 是否存在审核通过
            qeID = QueryExpression.Equal("COMPANYBUDGETAPPLYMASTERID", entity.COMPANYBUDGETAPPLYMASTERID);
            qeID.Operation = QueryExpression.Operations.NotEqual;
            qeBUDGETYEAR = QueryExpression.Equal("BUDGETYEAR", entity.BUDGETYEAR.ToString());
            qeID.RelatedExpression = qeBUDGETYEAR;
            qeCom = QueryExpression.Equal("OWNERDEPARTMENTID", entity.OWNERDEPARTMENTID);
            qeBUDGETYEAR.RelatedExpression = qeCom;
            qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            qeCom.RelatedExpression = qeStatesApproving;
            QueryExpression qeEDITSTATES = QueryExpression.Equal("ISVALID", "0");
            qeStatesApproving.RelatedExpression = qeEDITSTATES;
            result = InnerGetEntities<T_FB_COMPANYBUDGETAPPLYMASTER>(qeID);

            if (result.Count() > 0)
            {
                throw new FBBLLException("该部门年度预算已通过审核");
            }

            #endregion

            #region 是否存在生效
            qeID = QueryExpression.Equal("COMPANYBUDGETAPPLYMASTERID", entity.COMPANYBUDGETAPPLYMASTERID);
            qeID.Operation = QueryExpression.Operations.NotEqual;
            qeBUDGETYEAR = QueryExpression.Equal("BUDGETYEAR", entity.BUDGETYEAR.ToString());
            qeID.RelatedExpression = qeBUDGETYEAR;
            qeCom = QueryExpression.Equal("OWNERDEPARTMENTID", entity.OWNERDEPARTMENTID);
            qeBUDGETYEAR.RelatedExpression = qeCom;
            //qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            //qeCom.RelatedExpression = qeStatesApproving;
            qeEDITSTATES = QueryExpression.Equal("ISVALID", "1");
            qeCom.RelatedExpression = qeEDITSTATES;
            result = InnerGetEntities<T_FB_COMPANYBUDGETAPPLYMASTER>(qeID);
            if (result.Count() > 0)
            {
                throw new FBBLLException("该部门年度预算已生效");
            }

            #endregion

            #region 月度汇总
            QueryExpression qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
            qeBUDGETYEAR = QueryExpression.Equal("BUDGETYEAR", entity.BUDGETYEAR.ToString());
            QueryExpression qeStatesApproved = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            qeStatesApproving = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approving).ToString());
            qeStatesApproved.RelatedExpression = qeStatesApproving;
            qeStatesApproved.RelatedType = QueryExpression.RelationType.Or;
            qeBUDGETYEAR.RelatedExpression = qeStatesApproved;
            qeCompany.RelatedExpression = qeBUDGETYEAR;

            var result2 = InnerGetEntities<T_FB_COMPANYBUDGETSUMMASTER>(qeCompany);
            if (result2.Count() > 0)
            {
                throw new FBBLLException("该年度已做过年度汇总或正在审核中");
            }
            #endregion

            return SaveFBEntityDefault(fbEntity);
        }

        public FBEntity SaveT_FB_COMPANYBUDGETMODMASTER(FBEntity fbEntity)
        {

            //T_FB_COMPANYBUDGETMODMASTER entity = fbEntity.Entity as T_FB_COMPANYBUDGETMODMASTER;
            //entity.BUDGETYEAR = DateTime.Now.Year;
            //QueryExpression qeCom = QueryExpression.Equal(FieldName.OwnerCompanyID, entity.OWNERCOMPANYID);
            //QueryExpression qe = QueryExpression.Equal("BUDGETYEAR", entity.BUDGETYEAR.ToString());
            //QueryExpression qeStatesApproved = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            //QueryExpression qeEDITSTATES = QueryExpression.Equal("ISVALID", "1");
            //qeCom.RelatedExpression = qe;
            //qe.RelatedExpression = qeStatesApproved;
            //qeStatesApproved.RelatedExpression = qeEDITSTATES;
            //var result = InnerGetEntities<T_FB_COMPANYBUDGETAPPLYMASTER>(qeCom);
            //if (result.Count() == 0)
            //{
            //    throw new FBBLLException("该公司尚未做过年度预算");
            //}
            return SaveFBEntityDefault(fbEntity);
        }

        public FBEntity SaveT_FB_SYSTEMSETTINGS(FBEntity fbEntity)
        {
            try
            {


                bool isUpdateCheckDate = true;
                DateTime checkDateNew = Convert.ToDateTime(fbEntity.Entity.GetValue("CHECKDATE"));

                if (fbEntity.FBEntityState == FBEntityState.Modified)
                {
                    T_FB_SYSTEMSETTINGS setting = GetEntity(fbEntity.Entity.EntityKey) as T_FB_SYSTEMSETTINGS;
                    DateTime checkDateOld = setting.CHECKDATE.Value;

                    isUpdateCheckDate = !checkDateNew.Equals(checkDateOld);
                }

                if (isUpdateCheckDate)
                {
                    bool isSuccessful = EngineX.ConfigCheckDate(checkDateNew);
                    if (!isSuccessful)
                    {
                        throw new FBBLLException("设置自动预算结算失败。");
                    }
                }
                FBEntity result = this.SaveFBEntityDefault(fbEntity);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //public FBEntity SBSaveT_FB_EXTENSIONALORDER(FBEntity fbEntity)
        //{
        //    return SaveT_FB_EXTENSIONALORDER(fbEntity);
        //}

        public FBEntity SaveT_FB_PERSONMONEYASSIGNMASTER(FBEntity fbEntity)
        {
            T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
            var qe = QueryExpression.Equal("SUBJECTID", setting.MONEYASSIGNSUBJECTID);
            qe.IsNoTracking = true;

            var subject = qe.Query<T_FB_SUBJECT>(this).FirstOrDefault();
            if (subject == null)
            {
                throw new Exception("未找到对应的活动经费，保存失败！");
            }

            T_FB_PERSONMONEYASSIGNMASTER master = fbEntity.Entity as T_FB_PERSONMONEYASSIGNMASTER;
            if (fbEntity.FBEntityState == FBEntityState.Added)
            {
                //T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
                //var subject = QueryExpression.Equal("SUBJECTID", setting.MONEYASSIGNSUBJECTID).Query<T_FB_SUBJECT>().FirstOrDefault();

                master.T_FB_PERSONMONEYASSIGNDETAIL.ToList().ForEach(item =>
                {
                    item.T_FB_SUBJECT = subject;

                });
            }



            var listDetail = fbEntity.GetRelationFBEntities(typeof(T_FB_PERSONMONEYASSIGNDETAIL).Name);
            listDetail.ForEach(item =>
            {
                T_FB_PERSONMONEYASSIGNDETAIL entdetail = item.Entity as T_FB_PERSONMONEYASSIGNDETAIL;
                if (item.FBEntityState == FBEntityState.Added && subject != null)
                {
                    entdetail.T_FB_SUBJECT = subject;
                }
            });
            return this.SaveFBEntityDefault(fbEntity);

        }
        #endregion

        #region 1.	静态变量
        /// <summary>
        /// 实体集合
        /// </summary>
        public static List<EntityInfo> FBCommonEntityList { get; set; }
        /// <summary>
        /// 对外的预算服务地址
        /// </summary>
        public static string FBServiceUrl { get; set; }
       

        public static LockManager LockHelper = new LockManager();
        #endregion

        #region 4.	实体查询方法
        /// <summary>
        /// 查询实体,并带上实体的参照对象(包换父对象和子实体集)
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        public FBEntity GetFBEntity(QueryExpression queryExpression)
        {
            try
            {
                //if (queryExpression.VisitAction == ((int)SMT.SaaS.BLLCommonServices.Utility.Permissions.Audit).ToString())
                //{
                //    queryExpression.IsUnCheckRight = true;
                //}
                queryExpression.IsUnCheckRight = true;
                return GetFBEntityByExpression(queryExpression);
            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 查询实体,并带上实体的参照对象(包换父对象和子实体集)
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        public List<FBEntity> GetFBEntities(QueryExpression queryExpression)
        {

            try
            {
                if (queryExpression.VisitAction == ((int)SMT.SaaS.BLLCommonServices.Utility.Permissions.Audit).ToString())
                {
                    queryExpression.IsUnCheckRight = true;
                }

                return GetEntities(queryExpression);

            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// 1. 查询实体, 不包换子实体集
        /// 2. 一些特殊的数据实体查询
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        public QueryResult QueryData(QueryExpression queryExpression)
        {

            List<FBEntity> resultData = this.QueryFBEntities(queryExpression);
            QueryResult result = new QueryResult();
            result.Result = resultData;
            result.Pager = queryExpression.Pager;
            return result;
        }

        /// <summary>
        /// 1. 查询实体, 不包换子实体集
        /// 2. 一些特殊的数据实体查询
        /// </summary>
        /// <param name="queryExpression"></param>
        /// <returns></returns>
        public List<FBEntity> QueryFBEntities(QueryExpression queryExpression)
        {

            try
            {
                if (queryExpression.VisitAction == ((int)SMT.SaaS.BLLCommonServices.Utility.Permissions.Audit).ToString())
                {
                    queryExpression.IsUnCheckRight = true;
                }

                return QueryEntities(queryExpression);

            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                throw ex;
            }
        }

        public List<EntityObject> QueryTable(QueryExpression queryExpression)
        {
            try
            {
                return BaseGetEntities(queryExpression);

            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
                throw ex;
            }
        }
        #endregion

        #region "出差报销"


        #region 3.	扩展单据的查询方法
        /// <summary>
        /// 查扩展单的可用科目
        /// </summary>
        /// <remarks>
        ///     1. 从当前预算总帐中找出可用的科目 listBudgetAccount
        /// </remarks>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_EXTENSIONORDERDETAIL(QueryExpression qe)
        {
            var result = InnerQueryT_FB_EXTENSIONORDERDETAIL(qe);
            QueryExpression qeTravel = qe.GetQueryExpression("OrderTypes");

            if (qeTravel == null)
            {
                T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
                string tranverlSubjectid = setting.TRANVERLSUBJECTID;

                result.RemoveAll(item =>
                {
                    return (item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_SUBJECT.SUBJECTID == tranverlSubjectid;
                });
            }

            return result;
        }

        public List<FBEntity> InnerQueryT_FB_EXTENSIONORDERDETAIL(QueryExpression qe)
        {
            List<FBEntity> result = new List<FBEntity>();
            string msg = string.Empty;
            List<T_FB_BUDGETACCOUNT> listBudgetAccount = GetBUDGETACCOUNTPerson(qe,ref msg);

            var ListPerson = listBudgetAccount.FindAll(item =>
            {
                return ((AccountObjectType)item.ACCOUNTOBJECTTYPE) == AccountObjectType.Person;
            });

            var ListDept = listBudgetAccount.FindAll(item =>
            {
                return ((AccountObjectType)item.ACCOUNTOBJECTTYPE) == AccountObjectType.Deaprtment;
            });

            var listFind = ListDept.FindAll(item =>
            {
                return ListPerson.Exists(itemPerson => itemPerson.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID);
            });
            listFind.ForEach(item =>
            {
                listBudgetAccount.Remove(item);
            });
            listBudgetAccount.ForEach(p =>
            {
                T_FB_EXTENSIONORDERDETAIL t = new T_FB_EXTENSIONORDERDETAIL();
                t.EXTENSIONORDERDETAILID = Guid.NewGuid().ToString();
                var sub = p.T_FB_SUBJECT;


                t.T_FB_SUBJECT = sub;

                t.USABLEMONEY = p.USABLEMONEY;
                t.APPLIEDMONEY = 0;

                if (p.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person)
                {
                    t.CHARGETYPE = (int)ChargeType.Person;
                }
                else
                {
                    t.CHARGETYPE = (int)ChargeType.Commmon;
                }


                var temp = t.ToFBEntity();
                temp.FBEntityState = FBEntityState.Added;
                result.Add(temp);
            });


            return result;
        }
        /// <summary>
        /// 通过OrderID获取扩展单据
        /// 1. 如果没找到就当是新建扩展单据
        /// 2. 如果存在条件TravelSubject, 则认为有一条默认的业务差旅明细存在。
        ///     业务差旅费分为个人的，有部门。
        ///     如果有个人的预算，且可用额度大于0, 则用个人的预算，如果个人额度不够，则不能提交。
        ///     如果没有个人的预算或个人的预算额度为０，就用部门的预算。部门预算不够，不能提交。
        ///     
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<FBEntity> QueryT_FB_EXTENSIONALORDER(QueryExpression qe)
        {
            QueryExpression qeOrderID = qe.GetQueryExpression("ORDERID");
            QueryExpression qeTravelSubject = qe.GetQueryExpression("TravelSubject");
            QueryExpression qeExOrderTyp = qe.GetQueryExpression("EXTENSIONALTYPECODE");
            //获取部门ID
            QueryExpression qeOrderDepartment = qe.GetQueryExpression("OWNERDEPARTMENTID");
            QueryExpression qeOrderOwnerId = qe.GetQueryExpression("OWNERID");

            qeOrderID.IsUnCheckRight = true;
            qeOrderID.QueryType = typeof(T_FB_EXTENSIONALORDER).Name;
            qeOrderID.Include = new string[] { typeof(T_FB_EXTENSIONORDERDETAIL).Name, typeof(T_FB_EXTENSIONALTYPE).Name, "T_FB_EXTENSIONORDERDETAIL.T_FB_SUBJECT" };
            qeOrderID.IsNoTracking = true;
            List<FBEntity> listResult = this.QueryDefault(qeOrderID);

            if (listResult.Count == 0)
            {
                T_FB_EXTENSIONALORDER order = new T_FB_EXTENSIONALORDER();
                order.EXTENSIONALORDERID = Guid.NewGuid().ToString();
                order.ORDERID = qeOrderID.PropertyValue;
                order.OWNERID = qeOrderOwnerId.PropertyValue;
                order.OWNERDEPARTMENTID = qeOrderDepartment.PropertyValue;
                order.APPLYTYPE = 1;
                order.PAYTARGET = 1;
                var fbE = order.ToFBEntity();
                fbE.FBEntityState = FBEntityState.Added;
                listResult.Add(fbE);
            }


            FBEntity orderFB = listResult[0];
            List<FBEntity> details = orderFB.GetRelationFBEntities(typeof(T_FB_EXTENSIONORDERDETAIL).Name);
            List<FBEntity> detailsSpecial = orderFB.GetRelationFBEntities("T_FB_EXTENSIONORDERDETAIL_Travel");
            List<FBEntity> detailsBorrow = orderFB.GetRelationFBEntities(xmlOrderName);
            if (orderFB.FBEntityState != FBEntityState.Added)
            {
                T_FB_EXTENSIONALORDER orderEx = orderFB.Entity as T_FB_EXTENSIONALORDER;
                // orderEx.T_FB_EXTENSIONORDERDETAIL.Load();
                List<FBEntity> listDetail = orderEx.T_FB_EXTENSIONORDERDETAIL.ToList().ToFBEntityList();
                details.AddRange(listDetail);

                // 将存在字段PAYMENTINFO的xml转化为对象
                var master = FormatExtensionOrder(orderFB);
                if (master == null)
                {
                    // 借款项
                    List<FBEntity> listBorrowDetail = InnerQueryBorrowInfo(qe);
                    detailsBorrow.AddRange(listBorrowDetail);
                }
            }
            else
            {
                // 借款项
                List<FBEntity> listBorrowDetail = InnerQueryBorrowInfo(qe);
                detailsBorrow.AddRange(listBorrowDetail);
            }

            if (qeTravelSubject != null)
            {
                string subjectID = SystemBLL.GetSetting(null).TRANVERLSUBJECTID;
                if (orderFB.FBEntityState == FBEntityState.Added)
                {
                    details = InnerQueryT_FB_EXTENSIONORDERDETAIL(qe);
                }

                List<FBEntity> specialDetails = null;
                specialDetails = details.FindAll(item =>
                {
                    return (item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_SUBJECT.SUBJECTID == subjectID;
                });

                specialDetails.ForEach(item =>
                {
                    details.Remove(item);
                });


                // 存在既有个人预算又有部门预算
                if (specialDetails.Count() > 1)
                {

                    var personS = detailsSpecial.FirstOrDefault(item =>
                    {
                        return (item.Entity as T_FB_EXTENSIONORDERDETAIL).CHARGETYPE.Equal((int)ChargeType.Person);
                    });
                    if (personS != null)
                    {
                        detailsSpecial.Add(personS);
                    }
                    else
                    {
                        detailsSpecial.Add(specialDetails.FirstOrDefault());
                    }

                }
                else
                {
                    //detailsSpecial.AddRange(specialDetails);
                    if (specialDetails.Count() > 0)
                    {
                        detailsSpecial.AddRange(specialDetails);
                    }
                    else
                    {
                        T_FB_EXTENSIONORDERDETAIL detail = new T_FB_EXTENSIONORDERDETAIL();
                        detail.EXTENSIONORDERDETAILID = Guid.NewGuid().ToString();

                        detail.CHARGETYPE = 3;
                        detail.CREATEDATE = DateTime.Now;
                        //新建一张T_FB_EXTENSIONALORDER单，OWNERID会为NULL
                        detail.CREATEUSERID = qeOrderID.PropertyValue;
                        //根据科目ID获取科目
                        QueryExpression qeSubject = new QueryExpression();
                        qeSubject.PropertyName = "SUBJECTID";
                        //业务差旅费的ID
                        qeSubject.PropertyValue = "00161652-e3bf-4e9f-9a57-a9e1ff8cef74";
                        qeSubject.QueryType = typeof(T_FB_SUBJECT).Name;
                        List<FBEntity> Subjects = this.QueryDefault(qeSubject);
                        if (Subjects.Count() > 0)
                        {
                            detail.T_FB_SUBJECT = Subjects.FirstOrDefault().Entity as T_FB_SUBJECT;
                        }
                        detail.REMARK = "系统添加";
                        detail.UPDATEUSERID = qeOrderID.PropertyValue;
                        detail.USABLEMONEY = 0;
                        FBEntity fbEntityDetail = detail.ToFBEntity();
                        detailsSpecial.Add(fbEntityDetail);

                    }
                }

                detailsSpecial.ForEach(item =>
                {
                    item.Entity.SetValue("T_FB_EXTENSIONALORDER", orderFB.Entity);
                });

            }
            detailsSpecial.ForEach(item =>
            {
                string StrOwnerid = (orderFB.Entity as T_FB_EXTENSIONALORDER).OWNERID;
                string StrDepartmentid = (orderFB.Entity as T_FB_EXTENSIONALORDER).OWNERDEPARTMENTID;
                T_FB_EXTENSIONORDERDETAIL detail = item.Entity as T_FB_EXTENSIONORDERDETAIL;

                QueryExpression qeOwner = qe.GetQueryExpression(FieldName.OwnerID);
                QueryExpression qePost = qe.GetQueryExpression(FieldName.OwnerPostID);
                QueryExpression qeCompany = qe.GetQueryExpression(FieldName.OwnerCompanyID);
                QueryExpression qeDepartment = qe.GetQueryExpression(FieldName.OwnerDepartmentID);
                if (qeCompany == null)
                {
                    qeCompany = QueryExpression.Equal(FieldName.OwnerCompanyID, detail.T_FB_EXTENSIONALORDER.OWNERCOMPANYID);
                    qe.RelatedExpression = qeCompany;
                }
                if (qeDepartment == null)
                {
                    qeDepartment = QueryExpression.Equal(FieldName.OwnerDepartmentID, detail.T_FB_EXTENSIONALORDER.OWNERDEPARTMENTID);
                    qeCompany.RelatedExpression = qeDepartment;
                }
                if (qePost == null)
                {
                    qePost = QueryExpression.Equal(FieldName.OwnerPostID, detail.T_FB_EXTENSIONALORDER.OWNERPOSTID);
                    qeDepartment.RelatedExpression = qePost;
                }
                if (qeOwner == null)
                {
                    qeOwner = QueryExpression.Equal(FieldName.OwnerID, detail.T_FB_EXTENSIONALORDER.OWNERID);
                    qePost.RelatedExpression = qeOwner;
                }




                var RealAccount = this.ChangeExtenDetail(detail.T_FB_SUBJECT.SUBJECTID, qe, ref item);


                //(item.Entity as T_FB_EXTENSIONORDERDETAIL).USABLEMONEY = this.ChangeExtenDetail((item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_EXTENSIONALORDER.OWNERID, (item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_SUBJECT.SUBJECTID, (item.Entity as T_FB_EXTENSIONORDERDETAIL).T_FB_EXTENSIONALORDER.OWNERDEPARTMENTID, item.Entity as T_FB_EXTENSIONORDERDETAIL);
            });
            //ChangeExtenDetail();

            if (qeExOrderTyp != null)
            {
                List<FBEntity> extOrderTypes = orderFB.GetRelationFBEntities(typeof(T_FB_EXTENSIONALTYPE).Name);

                qeExOrderTyp.QueryType = typeof(T_FB_EXTENSIONALTYPE).Name;
                List<FBEntity> extTemps = QueryDefault(qeExOrderTyp);
                extOrderTypes.AddRange(extTemps);
            }

            return listResult;
        }

        public List<FBEntity> InnerQueryBorrowInfo(QueryExpression qe)
        {
            T_FB_CHARGEAPPLYMASTER master = GetEmptyMaster();

            List<FBEntity> resultList = new List<FBEntity>();
            var qeOwnerID = qe.GetQueryExpression(FieldName.OwnerID);
            var qeOwnerCompanyID = qe.GetQueryExpression(FieldName.OwnerCompanyID);

            if (qeOwnerCompanyID == null || qeOwnerID == null)
            {
                return resultList;
            }

            qeOwnerID.RelatedExpression = qeOwnerCompanyID;
            var borrowInfos = qeOwnerID.Query<T_FB_PERSONACCOUNT>(this);


            var itemBorrowInfo = borrowInfos.FirstOrDefault();
            if (itemBorrowInfo != null)
            {
                Func<decimal, decimal?, T_FB_CHARGEAPPLYREPAYDETAIL> CreateDetail = (repayType, money) =>
                {
                    T_FB_CHARGEAPPLYREPAYDETAIL detail = new T_FB_CHARGEAPPLYREPAYDETAIL();
                    detail.CHARGEAPPLYREPAYDETAILID = Guid.NewGuid().ToString();
                    detail.BORROWMONEY = money;
                    detail.CREATEDATE = DateTime.Now;
                    detail.CREATEUSERID = SYSTEM_USER_ID;
                    detail.UPDATEDATE = DateTime.Now;
                    detail.UPDATEUSERID = SYSTEM_USER_ID;
                    detail.REPAYTYPE = repayType;
                    detail.REPAYMONEY = 0;
                    return detail;
                };

                // 1现金还普通借款 2现金还备用金借款 3现金还专项借款
                if (!itemBorrowInfo.SIMPLEBORROWMONEY.Equal(0))
                {
                    master.T_FB_CHARGEAPPLYREPAYDETAIL.Add(CreateDetail(1, itemBorrowInfo.SIMPLEBORROWMONEY));
                }

                if (!itemBorrowInfo.BACKUPBORROWMONEY.Equal(0))
                {
                    master.T_FB_CHARGEAPPLYREPAYDETAIL.Add(CreateDetail(2, itemBorrowInfo.BACKUPBORROWMONEY));
                }

                if (!itemBorrowInfo.SPECIALBORROWMONEY.Equal(0))
                {
                    master.T_FB_CHARGEAPPLYREPAYDETAIL.Add(CreateDetail(3, itemBorrowInfo.SPECIALBORROWMONEY));
                }
            };

            resultList.Add(master.ToFBEntity());
            return resultList;
        }


        /// <summary>
        /// 扩展单据增加了冲还款记录，这些数据记录在Order 的 PAYMENTINFO　属性中（xml格式），在使用时，需要将其他转化为对象数据
        /// </summary>
        /// <param name="fbEntity"></param>
        public T_FB_CHARGEAPPLYMASTER FormatExtensionOrder(FBEntity fbEntity)
        {
            T_FB_CHARGEAPPLYMASTER result = null;
            T_FB_EXTENSIONALORDER order = fbEntity.Entity as T_FB_EXTENSIONALORDER;
            string xmlValue = order.PAYMENTINFO;
            try
            {
                XElement xElement = XElement.Parse(xmlValue);

                string paymentInfo = xElement.Element("PAYMENTINFO").Value;

                var masterXml = xElement.Element(xmlOrderName);
                List<T_FB_CHARGEAPPLYREPAYDETAIL> listDetail = new List<T_FB_CHARGEAPPLYREPAYDETAIL>();
                if (masterXml != null)
                {
                    T_FB_CHARGEAPPLYMASTER master = GetEmptyMaster();

                    master.PAYTYPE = Convert.ToInt32(masterXml.Element("PAYTYPE").Value);
                    result = master;
                    masterXml.Elements("T_FB_CHARGEAPPLYREPAYDETAIL").ForEach(item =>
                    {
                        T_FB_CHARGEAPPLYREPAYDETAIL detail = new T_FB_CHARGEAPPLYREPAYDETAIL();

                        detail.CHARGEAPPLYREPAYDETAILID = item.Element("CHARGEAPPLYREPAYDETAILID").Value;
                        detail.REPAYTYPE = Convert.ToInt32(item.Element("REPAYTYPE").Value);
                        detail.BORROWMONEY = Convert.ToDecimal(item.Element("BORROWMONEY").Value);
                        detail.REPAYMONEY = Convert.ToDecimal(item.Element("REPAYMONEY").Value);
                        detail.REMARK = item.Element("REMARK").Value;
                        detail.CREATEUSERID = item.Element("CREATEUSERID").Value;
                        detail.CREATEDATE = Convert.ToDateTime(item.Element("CREATEDATE").Value);
                        detail.UPDATEUSERID = item.Element("UPDATEUSERID").Value;
                        detail.UPDATEDATE = Convert.ToDateTime(item.Element("UPDATEDATE").Value);
                        master.T_FB_CHARGEAPPLYREPAYDETAIL.Add(detail);
                    });
                }

                order.PAYMENTINFO = paymentInfo;

                var listD = fbEntity.GetRelationFBEntities(xmlOrderName);
                if (listD.Count > 0) listD.Add(result.ToFBEntity());
            }
            catch (Exception ex)
            {
                SystemBLL.Debug(string.Format("调用FormatOrder方法异常, \r\n xml:{0} \r\n 异常： {1}", xmlValue, ex.ToString()));
            }

            return result;
        }
        /// <summary>
        /// 扩展单据增加了冲还款记录，这些数据记录在Order 的 PAYMENTINFO　属性中（xml格式），在保存时，需要将其对象数据转化为xml
        /// </summary>
        /// <param name="fbEntity"></param>
        public string UnFormatExtensionOrder(FBEntity fbEntity)
        {
            string paymentInfo = string.Empty;
            try
            {
                T_FB_EXTENSIONALORDER order = fbEntity.Entity as T_FB_EXTENSIONALORDER;

                XElement xElement = new XElement("T_FB_EXTENSIONALORDER");
                paymentInfo = order.PAYMENTINFO;
                xElement.Add(new XElement("PAYMENTINFO", paymentInfo));



                var listD = fbEntity.GetRelationFBEntities(xmlOrderName);
                var masterFBEntity = listD.FirstOrDefault();
                if (masterFBEntity != null)
                {
                    var masterXml = new XElement(xmlOrderName);

                    T_FB_CHARGEAPPLYMASTER master = masterFBEntity.Entity as T_FB_CHARGEAPPLYMASTER;
                    masterXml.Add(new XElement("PAYTYPE", master.PAYTYPE));

                    master.T_FB_CHARGEAPPLYREPAYDETAIL.ForEach(detail =>
                    {
                        var detailXml = new XElement("T_FB_CHARGEAPPLYREPAYDETAIL",
                            new XElement("CHARGEAPPLYREPAYDETAILID", detail.CHARGEAPPLYREPAYDETAILID),
                            new XElement("REPAYTYPE", detail.REPAYTYPE),
                            new XElement("BORROWMONEY", detail.BORROWMONEY),
                            new XElement("REPAYMONEY", detail.REPAYMONEY),
                            new XElement("REMARK", detail.REMARK),
                            new XElement("CREATEUSERID", detail.CREATEUSERID),
                            new XElement("CREATEDATE", detail.CREATEDATE),
                            new XElement("UPDATEUSERID", detail.UPDATEUSERID),
                            new XElement("UPDATEDATE", detail.UPDATEDATE));

                        masterXml.Add(detailXml);
                    });

                    xElement.Add(masterXml);
                }

                order.PAYMENTINFO = xElement.ToString();
            }
            catch (Exception ex)
            {
                SystemBLL.Debug(ex.ToString());
            }
            return paymentInfo;
        }

        private T_FB_CHARGEAPPLYMASTER GetEmptyMaster()
        {
            T_FB_CHARGEAPPLYMASTER master = new T_FB_CHARGEAPPLYMASTER();
            master.CHARGEAPPLYMASTERID = Guid.NewGuid().ToString();
            master.CHARGEAPPLYMASTERCODE = "001";

            master.PAYTYPE = 1;

            master.OWNERCOMPANYID = "001";
            master.OWNERDEPARTMENTID = "001";
            master.OWNERPOSTID = "001";
            master.OWNERID = "001";

            master.CREATEUSERID = "001";
            master.CREATEPOSTID = "001";
            master.CREATEDEPARTMENTID = "001";
            master.CREATECOMPANYID = "001";


            master.EDITSTATES = 1;
            master.CHECKSTATES = 0;

            master.BUDGETARYMONTH = System.DateTime.Now;
            master.CREATEDATE = System.DateTime.Now;
            master.UPDATEDATE = System.DateTime.Now;

            master.TOTALMONEY = 0;
            master.UPDATEUSERID = "001";

            return master;
        }


        /// <summary>
        /// 改变扩展表中可用额度
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="subjectId"></param>
        /// <param name="ownerDepartmentid"></param>
        /// <param name="extDetailObj"></param>
        /// <returns></returns>
        public decimal ChangeExtenDetail(string subjectId, QueryExpression qe, ref FBEntity fbentity)
        {
            List<T_FB_BUDGETACCOUNT> ltBudgetAccount = new List<T_FB_BUDGETACCOUNT>();
            string msg = string.Empty;
            ltBudgetAccount = GetBUDGETACCOUNTPerson(qe,ref msg);
            if (ltBudgetAccount.Count > 0)
            {
                var q = from ent in ltBudgetAccount
                        where ent.T_FB_SUBJECT.SUBJECTID == subjectId
                        select ent;
                if (q.Count() > 0)
                {
                    fbentity.Entity.SetValue("USABLEMONEY", q.FirstOrDefault().USABLEMONEY.Value);
                    if (q.FirstOrDefault().ACCOUNTOBJECTTYPE.Value == 3)//个人
                    {
                        Decimal value = 1;
                        fbentity.Entity.SetValue("CHARGETYPE", value);
                        SystemBLL.Debug("改变 " + fbentity.Entity.GetType().Name + " CHARGETYPE值：" + value + "科目id：" + subjectId);
                    }
                    else
                    {
                        Decimal value = 2;
                        fbentity.Entity.SetValue("CHARGETYPE", value);//部门公共
                        SystemBLL.Debug("改变 " + fbentity.Entity.GetType().Name + " CHARGETYPE值：" + value + "科目id：" + subjectId);
                    }
                    return q.FirstOrDefault().USABLEMONEY.Value;
                }
                else
                {
                    SystemBLL.Debug("ChangeExtenDetail " + qe.ToXml());
                    throw new FBBLLException("无可用的业务差旅费报销费用"+msg);
                }
            }
            else
            {
                SystemBLL.Debug("ChangeExtenDetail " + qe.ToXml());
                //00161652-e3bf-4e9f-9a57-a9e1ff8cef74业务差旅费  科目ID
                if (subjectId == "00161652-e3bf-4e9f-9a57-a9e1ff8cef74")
                {
                    throw new FBBLLException("无可用的业务差旅费报销科目，请联系财务管理员确认是否已启用该科目且有预算费用" + msg);
                }
                else
                {
                    throw new FBBLLException("无可用的报销科目" + msg);
                }
            }
            //decimal? deResult = 0;
            //QueryExpression qE = new QueryExpression();
            //qE.QueryType = "T_FB_BUDGETACCOUNT";

            ////报销类型为个人
            //IQueryable<T_FB_BUDGETACCOUNT> listBudgetAccount = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qE);
            //if (extDetailObj.CHARGETYPE == ((int)ChargeType.Person))
            //{
            //    var items = from ent in listBudgetAccount
            //                where ent.T_FB_SUBJECT.SUBJECTID == subjectId
            //                && ent.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person
            //                && ent.OWNERID == ownerId
            //                select ent;
            //    if (items.Count() > 0)
            //    {
            //        deResult = items.Max(p => p.USABLEMONEY);
            //    }
            //    else
            //    {
            //        deResult = extDetailObj.USABLEMONEY;
            //    }
            //}
            //else
            //{
            //    var q = from ent in baseDal.GetTable<T_FB_SUBJECTCOMPANY>()
            //            join department in baseDal.GetTable<T_FB_SUBJECTDEPTMENT>() 
            //            on ent.SUBJECTCOMPANYID equals department.T_FB_SUBJECTCOMPANY.SUBJECTCOMPANYID
            //            where ent.OWNERDEPARTMENTID == ownerDepartmentid
            //            && ent.T_FB_SUBJECT.SUBJECTID == subjectId
            //            && department.ACTIVED==1
            //            select ent;
            //    if (q.Count() > 0)
            //    {
            //        if (q.FirstOrDefault().ISMONTHLIMIT.Value == 0)
            //        {
            //            deResult = Max_Charge;
            //            return deResult;
            //        }
            //    }
            //    var itemDeparts = from ent in listBudgetAccount
            //                      where ent.OWNERDEPARTMENTID == ownerDepartmentid
            //                      && ent.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Deaprtment
            //                      && ent.T_FB_SUBJECT.SUBJECTID == subjectId
            //                      select ent;

            //    if (itemDeparts.Count() > 0)
            //    {
            //        deResult = itemDeparts.Max(p => p.USABLEMONEY);
            //    }
            //    else
            //    {
            //        deResult = extDetailObj.USABLEMONEY;
            //    }
            //}
            //return deResult;

        }

        #endregion

        #endregion

        #region 1. 查看个人或部门可用预算额度
        /// <summary>
        /// 查询预算总账
        /// </summary>
        /// <param name="qe"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<T_FB_BUDGETACCOUNT> GetBUDGETACCOUNT(QueryExpression qe, AccountObjectType type)
        {
            QueryExpression qeType = QueryExpression.Equal(FieldName_AccountObjectType, Convert.ToString((int)type));
            qeType.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;
            qeType.RelatedExpression = qe;
            //qeType.Include = qe.Include;
            qeType.Include = new string[] { typeof(T_FB_SUBJECT).Name };

            // this.GetTable<T_FB_BUDGETACCOUNT>();
            List<T_FB_BUDGETACCOUNT> result = this.GetEntities<T_FB_BUDGETACCOUNT>(qeType);
            result = result.OrderBy(item => item.T_FB_SUBJECT.SUBJECTCODE).ToList();
            return result;
        }

        public List<T_FB_BUDGETACCOUNT> FetchBUDGETACCOUNT(QueryExpression qe, AccountObjectType type)
        {
            List<T_FB_BUDGETACCOUNT> listResult = GetBUDGETACCOUNT(qe, type);
            List<T_FB_BUDGETACCOUNT> result = new List<T_FB_BUDGETACCOUNT>();

            QueryExpression qeDept = qe.GetQueryExpression(FieldName.OwnerDepartmentID);
            qeDept.Include = new string[] { typeof(T_FB_SUBJECT).Name, typeof(T_FB_SUBJECTCOMPANY).Name };
            IQueryable<T_FB_SUBJECTDEPTMENT> qSubjectDept = this.InnerGetEntities<T_FB_SUBJECTDEPTMENT>(qeDept);

            var views = from item in listResult
                        join itemCom in qSubjectDept
                        on new { item.T_FB_SUBJECT.SUBJECTID, item.OWNERCOMPANYID } equals new { itemCom.T_FB_SUBJECT.SUBJECTID, itemCom.OWNERCOMPANYID }
                        select new { item, itemCom, item.T_FB_SUBJECT };

            foreach (var view in views)
            {
                if (!view.itemCom.T_FB_SUBJECTCOMPANY.ISMONTHLIMIT.Equal(1))
                {
                    view.item.USABLEMONEY = Max_Charge;
                }
                view.item.T_FB_SUBJECT = view.T_FB_SUBJECT;
                result.Add(view.item);
            }

            return result;
        }

        /// <summary>
        /// 查询与个人有关的所有可用预算（个人和部门公共部门)
        /// </summary>
        /// <param name="qe"></param>
        /// <returns></returns>
        public List<T_FB_BUDGETACCOUNT> GetBUDGETACCOUNTPerson(QueryExpression qe,ref string msg)
        {
            QueryExpression qeOwner = qe.GetQueryExpression(FieldName.OwnerID);
            QueryExpression qePost = qe.GetQueryExpression(FieldName.OwnerPostID);
            QueryExpression qeDepartment = qe.GetQueryExpression(FieldName.OwnerDepartmentID);
            QueryExpression qeCompany = qe.GetQueryExpression(FieldName.OwnerCompanyID);

            QueryExpression qeOwerDepartment = QueryExpression.Equal("OWNERDEPARTMENTID", qeDepartment.PropertyValue);

            QueryExpression qeActived = QueryExpression.Equal("ACTIVED", "1");
            qeActived.RelatedExpression = qeOwerDepartment;
            qePost.RelatedExpression = qeActived;
            QueryExpression qeInner = new QueryExpression();
            qePost.IsNoTracking = true;
            qeInner.IsNoTracking = true;
            qeInner.Include = new string[] { typeof(T_FB_SUBJECT).Name };
            IQueryable<T_FB_SUBJECTPOST> qSubjectPost = this.InnerGetEntities<T_FB_SUBJECTPOST>(qePost);

            IQueryable<T_FB_BUDGETACCOUNT> qAccount = this.InnerGetEntities<T_FB_BUDGETACCOUNT>(qeInner);


            // 个人预算
            var personResult = (from item in qAccount
                                where item.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Person && item.OWNERID == qeOwner.PropertyValue
                                && item.OWNERPOSTID == qePost.PropertyValue
                                select item);
            // 部门的预算
            var departResult = (from item2 in qAccount
                                join item3 in qSubjectPost
                                on new { item2.T_FB_SUBJECT.SUBJECTID, item2.OWNERDEPARTMENTID } equals new { item3.T_FB_SUBJECT.SUBJECTID, item3.OWNERDEPARTMENTID }
                                where item2.ACCOUNTOBJECTTYPE == (int)AccountObjectType.Deaprtment
                                select item2);
            // 汇总
            var resultTemp = personResult.Union(departResult);

            // 在设置中可以用的科目，但在总账中没有记录的预算, 且是无报销额度限制的
            var newPostSubject = from item in qSubjectPost
                                 where item.T_FB_SUBJECTDEPTMENT.T_FB_SUBJECTCOMPANY.ISMONTHLIMIT == 0
                                    && !(
                                     from item2 in resultTemp
                                     select item2.T_FB_SUBJECT.SUBJECTID).Contains(item.T_FB_SUBJECT.SUBJECTID)
                                 select new { item.T_FB_SUBJECT, item.T_FB_SUBJECTDEPTMENT.T_FB_SUBJECTCOMPANY.ISMONTHLIMIT };
            List<T_FB_BUDGETACCOUNT> result = new List<T_FB_BUDGETACCOUNT>();

            // 如果科目报销不受月度预算控制时，可用额度为999999.
            var views = from item in resultTemp
                        join itemCom in this.GetTable<T_FB_SUBJECTCOMPANY>()

                        on new { item.T_FB_SUBJECT.SUBJECTID, item.OWNERCOMPANYID } equals new { itemCom.T_FB_SUBJECT.SUBJECTID, itemCom.OWNERCOMPANYID }
                        select new { item, itemCom, item.T_FB_SUBJECT };

            foreach (var view in resultTemp)
            {

                try
                {
                    var itemCompanySet = from item in this.GetTable<T_FB_SUBJECTCOMPANY>()
                                         where item.T_FB_SUBJECT.SUBJECTID == view.T_FB_SUBJECT.SUBJECTID
                                             && item.OWNERCOMPANYID == view.OWNERCOMPANYID
                                         select item;
                    //去除不受控
                    if (itemCompanySet.Count() > 0)
                    {
                        if (!itemCompanySet.FirstOrDefault().ISMONTHLIMIT.Equal(1))
                        {
                            view.USABLEMONEY = Max_Charge;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tracer.Debug("GetBUDGETACCOUNTPerson查询公司科目设置异常：" + ex.ToString());
                }


                var q = from ent in qSubjectPost
                        where ent.T_FB_SUBJECT.SUBJECTID == view.T_FB_SUBJECT.SUBJECTID
                        && ent.OWNERPOSTID == qePost.PropertyValue
                        && ent.OWNERDEPARTMENTID == view.OWNERDEPARTMENTID
                        select ent;
                if (q.Count() > 0)
                {
                    if (q.FirstOrDefault().ISPERSON == null)//默认分配到部门
                    {
                        if (!string.IsNullOrEmpty(qeDepartment.PropertyValue))
                        {
                            if (view.OWNERDEPARTMENTID != qeDepartment.PropertyValue) continue;
                        }
                        if (view.ACCOUNTOBJECTTYPE.Value == 3) continue;
                    }
                    else
                    {
                        if (q.FirstOrDefault().ISPERSON.Value == 1)//分配到个人
                        {
                            //如果在总账中没有找到个人分配的额度，那么使用公共的额度
                            var b = from ent in views
                                    where ent.T_FB_SUBJECT.SUBJECTID == view.T_FB_SUBJECT.SUBJECTID
                                    && ent.item.OWNERPOSTID == qePost.PropertyValue
                                    && ent.item.OWNERDEPARTMENTID == view.OWNERDEPARTMENTID
                                    && ent.item.ACCOUNTOBJECTTYPE.Value == 3
                                    select ent;
                            if (b.Count() > 0)//如果存在个人分配的额度，把部门的额度过滤掉
                            {
                                if (view.ACCOUNTOBJECTTYPE.Value == 2) continue;
                            }
                        }
                        else//使用公共费用
                        {
                            if (!string.IsNullOrEmpty(qeDepartment.PropertyValue))
                            {
                                if (view.OWNERDEPARTMENTID != qeDepartment.PropertyValue) continue;
                            }
                            if (view.ACCOUNTOBJECTTYPE.Value == 3) continue;
                        }
                    }
                
                    //判断使用个人还是公共费用
                    
                    if (view.T_FB_SUBJECT.SUBJECTID == "00161652-e3bf-4e9f-9a57-a9e1ff8cef74")//业务差旅费
                    {
                        if (q.FirstOrDefault().ISPERSON == null)//默认分配到部门
                        {
                            msg = "业务差旅费使用公共费用";
                        }
                        else
                        {
                            if (q.FirstOrDefault().ISPERSON.Value == 1)//分配到个人
                            {
                                msg = "业务差旅费使用个人费用";
                            }
                            else
                            {
                                msg = "业务差旅费使用公共费用";
                            }
                        }
                    }
                    view.T_FB_SUBJECT = view.T_FB_SUBJECT;
                    result.Add(view);
                }
                else
                {
                    if (view.T_FB_SUBJECT.SUBJECTID == "00161652-e3bf-4e9f-9a57-a9e1ff8cef74")
                    {
                        msg = "没有找到岗位科目设置中的业务差旅费设置项,请联系管理员";
                    }
                }
            }
            newPostSubject.ToList().ForEach(item =>
            {
                result.Add(
                    new T_FB_BUDGETACCOUNT
                    {
                        BUDGETACCOUNTID = Guid.NewGuid().ToString(),
                        T_FB_SUBJECT = item.T_FB_SUBJECT,
                        ACCOUNTOBJECTTYPE = (int)AccountObjectType.Deaprtment,
                        USABLEMONEY = item.ISMONTHLIMIT.Equal(0) ? Max_Charge : 0,
                        ACTUALMONEY = 0,
                        BUDGETYEAR = DateTime.Now.Year,
                        BUDGETMONTH = DateTime.Now.Month
                    }
                );
            });

            result.ForEach(item =>
            {
                item.T_FB_SUBJECT.T_FB_SUBJECTPOST.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTCOMPANY.Clear();
                item.T_FB_SUBJECT.T_FB_SUBJECTDEPTMENT.Clear();
            });
            result = result.OrderBy(item => item.T_FB_SUBJECT.SUBJECTCODE).ToList();

            return result;
        }
        #endregion

    }
}
