using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using System.Text;
using SMT_FB_EFModel;
using SMT.FB.BLL;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Xml.Linq;
using SMT.Foundation.Log;
using SMT.SaaS.BLLCommonServices.PersonnelWS;

namespace SMT.FB.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceKnownType("GetKnownTypes", typeof(Helper))]
    public class FBService : FBServiceBase
    {
        public FBService() : base()
        {
            
        }

        #region 1.	用于HR财务工资预算的方法

        private void TryGetQueryExpression(string propertyName, string propertyValue, ref QueryExpression parentQueryExpression)
        {
            if (!string.IsNullOrEmpty(propertyValue))
            {
                QueryExpression qe = QueryExpression.Equal(propertyName, propertyValue);
                parentQueryExpression.RelatedExpression = qe;
                parentQueryExpression = qe;
            }
        }

        /// <summary>
        /// 获取预算总帐
        /// </summary>
        /// <param name="sbjectID"></param>
        /// <param name="companyID"></param>
        /// <param name="departmentID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected List<T_FB_BUDGETACCOUNT> FetchBudgetAccount(string sbjectID, string companyID, string departmentID, BudgetAccountBLL.AccountObjectType type)
        {
            QueryExpression qeTop = new QueryExpression();
            QueryExpression qeFirst = qeTop;
            TryGetQueryExpression("T_FB_SUBJECT.SUBJECTID", sbjectID, ref qeTop);
            TryGetQueryExpression("OWNERCOMPANYID", companyID, ref qeTop);
            TryGetQueryExpression("OWNERDEPARTMENTID", departmentID, ref qeTop);
            // TryGetQueryExpression("ACCOUNTOBJECTTYPE", ((int)type).ToString(), ref qeTop);
            if (qeFirst.RelatedExpression == null)
            {
                return new List<T_FB_BUDGETACCOUNT>();
            }
            qeFirst = qeFirst.RelatedExpression;
            qeFirst.QueryType = typeof(T_FB_BUDGETACCOUNT).Name;

            BudgetAccountBLL bll = new BudgetAccountBLL();
            List<T_FB_BUDGETACCOUNT> listResult = bll.FetchBUDGETACCOUNT(qeFirst, type);            

            return listResult;
        }

        /// <summary>
        /// 获取月度工资预算
        /// </summary>
        /// <param name="companyID">公司Guid</param>
        /// <param name="departmentID">部门Guid</param>
        /// <returns>USABLEMONEY : 当前可用额度， 
        ///          BUDGETMONEY : 当前月预算额度,
        ///          OWNERCOMPANYID : 当前公司, 
        ///          OWNERDEPARTMENTID : 当前部门
        ///          BUDGETMONTH : 当前月份
        ///          BUDGETYEAR : 当前年份
        ///          </returns>
        [OperationContract]
        public List<T_FB_BUDGETACCOUNT> FetchSalaryBudget(string companyID, string departmentID)
        {

            string subjectID = string.Empty;
            T_FB_SYSTEMSETTINGS setting = SystemBLL.GetSetting(null);
            if (setting != null)
            {

                subjectID = setting.SALARYSUBJECTID;
            }
            if (string.IsNullOrEmpty(subjectID))
            {
                return new List<T_FB_BUDGETACCOUNT>();
            }

            return FetchBudgetAccount(subjectID, companyID, departmentID, BudgetAccountBLL.AccountObjectType.Deaprtment);


        }

        /// <summary>
        /// 扣除工资预算
        /// </summary>
        /// <param name="xml">按部门汇总的工资列表</param>
        /// <returns></returns>
        [OperationContract]
        public bool UpdateSalaryBudget(string xml)
        {

            //string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            //XElement xe = XElement.Load(path + "\\SalaryBudget.xml");
            //xml = xe.ToString();

            XElement doc = XElement.Parse(xml);
            int payYear = int.Parse(doc.Attribute("Year").Value);
            int payMonth = int.Parse(doc.Attribute("Month").Value);
            var comX = doc.Elements("Company");
            List<T_FB_SALARYPAYLIST> listSave = new List<T_FB_SALARYPAYLIST>();
            foreach (var com in comX)
            {
                string comID = com.Attribute("CompanyID").Value;
                var deptPayList = from item in com.Elements("Department")
                                  select new T_FB_SALARYPAYLIST
                                  {
                                      PAYYEAR = payYear,
                                      PAYMONTH = payMonth,
                                      SALARYPAYLISTID = Guid.NewGuid().ToString(),
                                      OWNERCOMPANYID = comID,
                                      OWNERDEPARTMENTID = item.Attribute("DepartmentID").Value,
                                      PAYMONEY = decimal.Parse(item.Attribute("Salary").Value),
                                      CREATECOMPANYID = "001",
                                      CREATEDEPARTMENTID = "001",
                                      CREATEPOSTID = "001",
                                      CREATEUSERID = "001",
                                      UPDATEUSERID = "001",
                                      CREATEDATE = System.DateTime.Now,
                                      UPDATEDATE = System.DateTime.Now
                                  };
                listSave.AddRange(deptPayList);
            }
            List<FBEntity> listResult = listSave.ToFBEntityList();
            listResult.ForEach(item =>
                {
                    item.FBEntityState = FBEntityState.Added;

                });
            BudgetAccountBLL bll = new BudgetAccountBLL();

            return bll.UpdateSalaryBudget(listResult);
        }

        /// <summary>
        /// 返回需要按工资扣款的借款人
        /// </summary>
        /// <param name="corpID">公司ID</param>
        /// <param name="departmentID">部门ID</param>
        /// <returns>
        /// 需要扣款的借款人列表
        ///    EmployeeID : 员工ID
        ///    UsableSalary : 可用于扣款的工资额度， 默认为0
        ///    Debt : 欠款额度, 默认为0。
        /// </returns>
        [OperationContract]
        public List<DebtInfo> GetBorrowers(string companyID, string departmentID)
        {
            return new List<DebtInfo>();
        }

        /// <summary>
        /// 工资扣借款
        /// </summary>
        /// <param name="listDebt">
        /// 需要扣款的借款人列表
        ///    EmployeeID : 员工ID
        ///    UsableSalary : 可用于扣款的工资额度
        /// </param>
        /// <param name="isRepay">
        ///  true: 真正扣款
        ///  false : 只列出需要扣的款
        /// </param>
        /// <returns>
        /// 需要扣款的借款人列表
        ///    EmployeeID : 员工ID
        ///    Debt : 需要扣款额度
        /// </returns>
        [OperationContract]
        public List<DebtInfo> RepayBySalary(List<DebtInfo> listDebt, RepayType repayType)
        {
            List<DebtInfo> result = new List<DebtInfo>();
            //System.Linq.Expressions.Expression.con
            listDebt.ForEach(item =>
                {
                    QueryExpression qeOwnerID = QueryExpression.Equal(FieldName.OwnerID, item.EmployeeID);
                    QueryExpression qeCheckStates = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
                    QueryExpression qeISREPAIED = QueryExpression.Equal("ISREPAIED", "0");
                    QueryExpression qeOverDate = QueryExpression.Equal("PLANREPAYDATE", DateTime.Now.Date.ToString("yyyy-MM-dd"));
                    qeOverDate.Operation = QueryExpression.Operations.LessThan;
                    qeOwnerID.RelatedExpression = qeCheckStates;
                    qeCheckStates.RelatedExpression = qeISREPAIED;
                    qeISREPAIED.RelatedExpression = qeOverDate;
                    List<DebtInfo> debtInfos = GetDebtInfo(qeOwnerID);

                    debtInfos.ForEach(dept =>
                        {

                        });
                });


            return new List<DebtInfo>();
        }
        public enum RepayType
        {
            Plan, Pass, Canel
        }
        /// <summary>
        /// 列出离职人员借款清单
        /// </summary>
        /// <param name="employeeID">员工ID</param>
        /// <returns>
        /// 离职人员借款清单     
        ///    EmployeeID : 员工ID
        ///    OrderType : 单据类型
        ///    OrderCode : 单据编号
        ///    Debt : 借款数
        /// </returns>
        [OperationContract]
        public List<DebtInfo> GetLeavingUser(string employeeID)
        {

            QueryExpression qeOwnerID = QueryExpression.Equal(FieldName.OwnerID, employeeID);
            QueryExpression qeCheckStates = QueryExpression.Equal(FieldName.CheckStates, ((int)CheckStates.Approved).ToString());
            QueryExpression qeISREPAIED = QueryExpression.Equal("ISREPAIED", "0");

            qeOwnerID.RelatedExpression = qeCheckStates;
            qeCheckStates.RelatedExpression = qeISREPAIED;
            return GetDebtInfo(qeOwnerID);
        }

        private List<DebtInfo> GetDebtInfo(QueryExpression qe)
        {
            using (FBCommonBLL bll = new FBCommonBLL())
            {
                List<DebtInfo> result = new List<DebtInfo>();
                qe.Include = new string[] { typeof(T_FB_BORROWAPPLYDETAIL).Name };
                qe.OrderBy = new string[] { "PLANREPAYDATE" };

                List<T_FB_BORROWAPPLYMASTER> list = bll.GetEntities<T_FB_BORROWAPPLYMASTER>(qe);

                list = list.OrderBy(item => item.PLANREPAYDATE).ToList();
                result = list.CreateList(item =>
                {
                    DebtInfo debtInfo = new DebtInfo();
                    decimal? deblt = item.T_FB_BORROWAPPLYDETAIL.Sum(detail =>
                    {
                        return detail.UNREPAYMONEY;
                    });
                    debtInfo.Debt = deblt == null ? 0 : deblt.Value;
                    debtInfo.EmployeeID = item.OWNERID;
                    debtInfo.OrderType = "T_FB_BORROWAPPLYMASTER";
                    debtInfo.OrderCode = item.BORROWAPPLYMASTERCODE;
                    return debtInfo;
                });
                return result;
            }
        }
        #endregion

        #region 2.	用于预算控件的方法

        /// <summary>
        /// 批量删除外部单据
        /// </summary>
        /// <param name="orderIDs">单据ID集合</param>
        /// <returns>不能删除的单据ID集合</returns>
        [OperationContract]
        public List<string> RemoveExtensionOrder(List<string> orderIDs)
        {
            using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
            {
                List<string> listResult = new List<string>();
                fbCommonBLL.BeginTransaction();
                QueryExpression qe = new QueryExpression();
                qe.Operation = QueryExpression.Operations.Equal;
                qe.PropertyName = "ORDERID";

                orderIDs.ForEach(item =>
                {
                    qe.PropertyValue = item;
                    List<T_FB_EXTENSIONALORDER> list = fbCommonBLL.GetEntities<T_FB_EXTENSIONALORDER>(qe);
                    list.ForEach(entity =>
                    {
                        // 只能删除未提交的单据
                        if (((int)entity.CHECKSTATES) != (int)CheckStates.UnSubmit)
                        {
                            listResult.Add(item);
                        }
                        else
                        {
                            fbCommonBLL.Remove(entity);
                        }
                    });
                });
                return listResult;
            }
        }
        [OperationContract]
        public SaveResult Save(FBEntity fbEntity)
        {
            SaveResult result = new SaveResult();
            try
            {
                using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
                {
                    result = fbCommonBLL.FBCommSaveEntity(fbEntity);
                }
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.Exception = ex.Message;
            }
            return result;
        }

        [OperationContract(Name = "SaveEntity")]
        public SaveResult Save(SaveEntity saveEntity)
        {
            using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
            {
                SaveResult result = new SaveResult();
                try
                {
                    result = fbCommonBLL.Save(saveEntity);
                }
                catch (Exception ex)
                {
                    result.Successful = false;
                    result.Exception = ex.Message;
                    Tracer.Debug(ex.ToString());

                }
                return result;
            }
        }

        [OperationContract]
        public List<FBEntity> QueryFBEntities(QueryExpression queryExpression)
        {
            using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
            {
                List<string> listOrder = new List<string>();
                listOrder.Add(typeof(T_FB_EXTENSIONORDERDETAIL).Name);
                listOrder.Add(typeof(T_FB_EXTENSIONALORDER).Name);
                listOrder.Add(typeof(T_FB_EXTENSIONALTYPE).Name);
                if (!listOrder.Contains(queryExpression.QueryType))
                {
                    return new List<FBEntity>();
                }
                queryExpression.IsUnCheckRight = true;
                List<FBEntity> listDetail = fbCommonBLL.QueryFBEntities(queryExpression);
                return listDetail;
            }
        }


        [OperationContract]
        public List<FBEntity> GetFBEntities(QueryExpression qp)
        {
            using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
            {
                return fbCommonBLL.GetFBEntities(qp);
            }
        }

        #endregion

        #region 3.	月结方法
        /// <summary>
        /// 预算结算
        /// </summary>
        [OperationContract]
        public void CloseBudget()
        {
            DateTime currentDate = DateTime.Now;
            BudgetAccountBLL bll = new BudgetAccountBLL();
            bll.CloseBudget(currentDate);
            Tracer.Debug(string.Format("在{0} 预算月结完毕!", System.DateTime.Now.ToString("yyyy-MM-dd")));
        }
        #endregion

        #region 4.      用于移动版单据审核
        /// <summary>
        /// 根据实体名，实体的主键ID及审核状态，更新指定实体的记录
        /// </summary>
        /// <param name="strModelCode">实体名</param>
        /// <param name="orderID">实体的主键ID</param>
        /// <param name="strCheckStates">审核状态</param>
        [OperationContract]
        public int UpdateCheckState(string strModelCode, string orderID, string strCheckStates, ref string strMsg)
        {
            int i = -1;
            try
            {
                SystemBLL.Debug("UpdateCheckState方法已被调用，参数：strModelCode: " + strModelCode + ", orderID: " + orderID + ", strCheckStates: " + strCheckStates);

                // begin 用于出差报销、事项审批的手机提交
                if (strModelCode == "Travel")
                {
                    var tempResult = UpdateExtensionOrder(strModelCode, orderID, strCheckStates, ref strMsg);
                    return (tempResult == null) ? -1 : 1;

                }
                // end 
                FBCommonService fbCommonService = new FBCommonService();
                List<EntityInfo> EntityInfoList = fbCommonService.GetEntityInfoList();
                if (EntityInfoList == null)
                {
                    strMsg = "预算服务初始化异常，请重试。";
                    return -1;
                }

                if (EntityInfoList.Count() == 0)
                {
                    strMsg = "预算服务初始化异常，请重试。";
                    return -1;
                }

                string strTypeName = "";
                string strKeyName = "";
                CheckStates cs = CheckStates.UnSubmit;
                switch (strCheckStates)
                {
                    case "1":
                        cs = CheckStates.Approving;
                        break;
                    case "2":
                        cs = CheckStates.Approved;
                        break;
                    case "3":
                        cs = CheckStates.UnApproved;
                        break;
                    default:
                        break;
                }

                var entityInfo = EntityInfoList.Where(t => t.Type == strModelCode).FirstOrDefault();
                strTypeName = entityInfo.Type;
                strKeyName = entityInfo.KeyName;
                /////add 2012.12.12
                /////传入报销月份为时间去和当前时间判断，如果不在同一年
                /////说明该报销单是跨年的，则不能进行审核操作，即当年的报销单只能在当年进行报销
                //if (dNewCheckStates == FBAEnums.CheckStates.Approved || dNewCheckStates == FBAEnums.CheckStates.Approving)
                //{
                //    if (IsOverYear(entity.BUDGETARYMONTH))
                //    {
                //        strMsg = "报销单跨年后只能终审不通过(财务规定)";
                //        Tracer.Debug(strMsg);
                //        return;
                //    }
                //}
                using (FBCommonBLL bllCommon = new FBCommonBLL())
                {
                    bllCommon.BeginTransaction();
                    SystemBLL.Debug("BeginTransaction "+ strModelCode + " 的单据[" + orderID + "]"); 
                    try
                    {
                        QueryExpression qe = QueryExpression.Equal(strKeyName, orderID);
                        qe.QueryType = strTypeName;

                        var data = qe.Query(bllCommon);
                        var order = data.FirstOrDefault();
                        if (order == null)
                        {
                            strMsg = "没有可操作的数据";
                            return -1;
                        }

                        bllCommon.AuditFBEntityWithoutFlow(order, cs, ref strMsg);
                        i = 1;
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            bllCommon.CommitTransaction();
                            SystemBLL.Debug("CommitTransaction " + strModelCode + " 的单据[" + orderID + "]");
                        }
                        else
                        {
                            bllCommon.RollbackTransaction();
                            SystemBLL.Debug("RollbackTransaction 审核" + strModelCode + "的单据[" + orderID + "]失败，提示消息为：" + strMsg);
                        
                        }
                     
                    }
                    catch (Exception ex)
                    {
                        bllCommon.RollbackTransaction();
                        SystemBLL.Debug("RollbackTransaction 审核" + strModelCode + "的单据[" + orderID + "]失败，提示消息为：" + strMsg);
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                strMsg = "单据审核异常，请联系管理员";
                throw ex;
            }
            
            // 把消息通过异常机制返回
            if (!string.IsNullOrWhiteSpace(strMsg))
            {
                
                SystemBLL.Debug("审核" + strModelCode + "的单据[" + orderID + "]失败，提示消息为：" + strMsg);
                throw new Exception(strMsg);
            }

            return i;
        }

        /// <summary>
        /// 扩展单据手机审单专用
        /// </summary>
        /// <param name="strModelCode"></param>
        /// <param name="orderID"></param>
        /// <param name="strCheckStates"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        [OperationContract]
        public T_FB_EXTENSIONALORDER UpdateExtensionOrder(string strModelCode, string orderID, string strCheckStates, ref string strMsg)
        {
            using (FBCommonBLL fbCommonBLL = new FBCommonBLL())
            {
                try
                {
                    SystemBLL.Debug("UpdateExtensionOrder方法已被调用，参数：strModelCode: " + strModelCode + ", orderID: " + orderID + ", strCheckStates: " + strCheckStates);

                    QueryExpression queryExpression = QueryExpression.Equal("ORDERID", orderID);

                    if (strModelCode == "Travel")
                    {
                        QueryExpression tempQE = QueryExpression.Equal("TravelSubject", "1");

                        tempQE.RelatedExpression = QueryExpression.Equal("EXTENSIONALTYPECODE", "CCPX");
                        queryExpression.RelatedExpression = tempQE;
                    }
                    //查出保存的外部单据
                    queryExpression.QueryType = typeof(T_FB_EXTENSIONALORDER).Name;
                    queryExpression.IsNoTracking = true;
                    List<FBEntity> listDetail = fbCommonBLL.QueryFBEntities(queryExpression);
                    //如果存在外部单据
                    if (listDetail.Count > 0)
                    {
                        var saveFBEntity = listDetail[0];
                        saveFBEntity.Entity.SetValue("CHECKSTATES", Decimal.Parse(strCheckStates));
                        saveFBEntity.FBEntityState = FBEntityState.Modified;
                        //var temp = fbCommonBLL.FBCommSaveEntity(listDetail[0]);
                        SaveResult temp = new SaveResult();
                        try
                        {
                            temp.FBEntity = fbCommonBLL.SaveT_FB_EXTENSIONALORDER(listDetail[0]);
                            temp.Successful = true;
                        }
                        catch(Exception ex)
                        {
                            temp.Exception = ex.Message;
                            temp.Successful = false;
                        }
                         
                        if (temp.Successful)
                        {
                            return temp.FBEntity.Entity as T_FB_EXTENSIONALORDER;
                        }
                        else
                        {
                            strMsg = temp.Exception;
                        }
                    }
                    else
                    {
                        strMsg = "没有可操作的数据";
                    }
                }
                catch (Exception ex)
                {
                    strMsg = ex.Message;
                    if (!(ex is FBBLLException))
                    {
                        // strMsg = "单据审核异常，请联系管理员";
                        Tracer.Debug("审核" + strModelCode + "的单据[" + orderID + "]出现异常，错误消息为：" + ex.ToString());
                    }
                }
                return null;
            }
        }
        #endregion

        #region 5.      系统生成活动经费并生成下拨活动经费的待办任务
        [OperationContract]
        public void CreatePersonMoneyAssignInfo(string ASSIGNCOMPANYID, string OWNERID)
        {
            using (BudgetAccountBLL obll = new BudgetAccountBLL())
            {
                obll.CreatePersonMoneyAssignInfo(ASSIGNCOMPANYID, OWNERID);
                Tracer.Warn(string.Format("在{0} 系统生成活动经费：公司ID{1},个人ID{2}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ASSIGNCOMPANYID, OWNERID));
            }
        }
        #endregion

        #region 6.  HR人事调动后修改活动经费
        [OperationContract]
        public bool HRPersonPostChanged(T_HR_EMPLOYEEPOSTCHANGE personChange, ref string message)
        {
            try
            {
                using (FBCommonBLL bll = new FBCommonBLL())
                {
                    var setting = SystemBLL.GetSetting(null);

                    var qePostID = QueryExpression.Equal(FieldName.OwnerPostID, personChange.FROMPOSTID);
                    var qeOwnerID = QueryExpression.Equal(FieldName.OwnerID, personChange.T_HR_EMPLOYEE.EMPLOYEEID);
                    var qeSubjectID = QueryExpression.Equal("T_FB_SUBJECT.SUBJECTID", setting.MONEYASSIGNSUBJECTID);
                    qePostID.RelatedExpression = qeOwnerID;
                    qeOwnerID.RelatedExpression = qeSubjectID;


                    var entity = bll.GetEntity<T_FB_BUDGETACCOUNT>(qePostID);

                    if (entity != null)
                    {
                        entity.OWNERCOMPANYID = personChange.TOCOMPANYID;
                        entity.OWNERDEPARTMENTID = personChange.TODEPARTMENTID;
                        entity.OWNERPOSTID = personChange.TOPOSTID;
                        entity.OWNERID = personChange.T_HR_EMPLOYEE.EMPLOYEEID;

                        bll.BassBllSave(entity, FBEntityState.Modified);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "修改异动后的预算活动经费数据异常!";
                Tracer.Debug(message + " 异常信息： " + ex.ToString());
                return false;
            }


            
        }
        #endregion

    }

}
