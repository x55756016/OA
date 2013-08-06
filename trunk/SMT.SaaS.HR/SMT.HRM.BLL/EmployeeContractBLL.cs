using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
namespace SMT.HRM.BLL
{
    public class EmployeeContractBLL : BaseBll<T_HR_EMPLOYEECONTRACT>, IOperate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="sType"></param>
        /// <param name="sValue"></param>
        /// <param name="strCheckState"></param>
        /// <param name="ownerID"></param>
        /// <returns></returns>
        public IQueryable<T_HR_EMPLOYEECONTRACT> EmployeeContractsPaging(int pageIndex, int pageSize, string sort, string filterString,
            IList<object> paras, ref int pageCount, string sType, string sValue, string strCheckState, string ownerID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            List<object> objArgs = new List<object>();
            objArgs.Add(paras);
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                SetOrganizationFilter(ref filterString, ref queryParas, ownerID, "T_HR_EMPLOYEECONTRACT");


                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " AND";
                    }

                    filterString += " CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
            }
            else
            {
                SetFilterWithflow("EMPLOYEECONTACTID", "T_HR_EMPLOYEECONTRACT", ownerID, ref strCheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }

            IQueryable<T_HR_EMPLOYEECONTRACT> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");
            IQueryable<T_HR_EMPLOYEE> entEmps = dal.GetObjects<T_HR_EMPLOYEE>();

            if (!string.IsNullOrWhiteSpace(sType) && !string.IsNullOrWhiteSpace(sValue))
            {
                switch (sType)
                {
                    case "Company":
                        ents = ents.Where(t => t.OWNERCOMPANYID == sValue);
                        break;
                    case "Department":
                        ents = ents.Where(t => t.OWNERDEPARTMENTID == sValue);
                        break;
                    case "Post":
                        ents = ents.Where(t => t.OWNERPOSTID == sValue);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEECONTRACT>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 用于实体Grid中显示数据的分页查询
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<T_HR_EMPLOYEECONTRACT> EmployeeContractPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckState, string userID)
        {

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            List<object> objArgs = new List<object>();
            objArgs.Add(paras);
            if (strCheckState != Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEECONTRACT");


                if (!string.IsNullOrEmpty(strCheckState))
                {
                    if (!string.IsNullOrEmpty(filterString))
                    {
                        filterString += " AND";
                    }

                    filterString += " CHECKSTATE == @" + queryParas.Count();
                    queryParas.Add(strCheckState);
                }
            }
            else
            {
                SetFilterWithflow("EMPLOYEECONTACTID", "T_HR_EMPLOYEECONTRACT", userID, ref strCheckState, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }

            }
            IQueryable<T_HR_EMPLOYEECONTRACT> ents = dal.GetObjects().Include("T_HR_EMPLOYEE");

            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_EMPLOYEECONTRACT>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        /// <summary>
        /// 添加劳动合同
        /// </summary>
        /// <param name="entity">劳动合同实体</param>
        public void EmployeeContractAdd(T_HR_EMPLOYEECONTRACT entity, ref string strMsg)
        {
            try
            {
                //查指定公司指定员工的合同（不包含因离职导致的终止合同）
                var contractTmp = from c in dal.GetObjects()
                                  where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.ENDDATE == null && c.OWNERCOMPANYID == entity.OWNERCOMPANYID
                                  select c;
                if (contractTmp.Count() > 0)
                {
                    foreach (var entItem in contractTmp)
                    {
                        //有未提交和审核中的合同 不能新建
                        if (entItem.CHECKSTATE == "0" || entItem.CHECKSTATE == "1")
                        {
                            strMsg = "EXIST";
                            return;
                        }
                        //else if (entItem.CHECKSTATE == "2")
                        //{
                        //    //审核通过 未到期的合同
                        //    DateTime dt;
                        //    bool flag = DateTime.TryParse(entItem.TODATE, out dt);
                        //    if (flag)
                        //    {
                        //        if(dt.Date<=System.DateTime.Now.Date)
                        //    }
                        //}
                    }

                }
                T_HR_EMPLOYEECONTRACT ent = new T_HR_EMPLOYEECONTRACT();
                Utility.CloneEntity<T_HR_EMPLOYEECONTRACT>(entity, ent);
                ent.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                //dal.Add(ent);
                SMT.Foundation.Log.Tracer.Debug("开始添加员工合同信息，员工" + entity.T_HR_EMPLOYEE.EMPLOYEEID);
                if (!Add(ent, ent.CREATEUSERID))
                {
                    strMsg = "ERROR";
                    return;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeContractAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 修改劳动合同记录
        /// </summary>
        /// <param name="entity">劳动合同实体</param>
        public void EmployeeContractUpdate(T_HR_EMPLOYEECONTRACT entity)
        {
            try
            {
                //var contractTmp = from c in dal.GetObjects()
                //                  where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && c.CHECKSTATE != "3" && c.EMPLOYEECONTACTID != entity.EMPLOYEECONTACTID
                //                  select c;
                //if (contractTmp.Count() > 0)
                //{
                //    throw new Exception("EXIST");
                //}
                // T_HR_EMPLOYEECONTRACT ent = new T_HR_EMPLOYEECONTRACT();
                #region
                //var ents = from c in dal.GetObjects()
                //           where c.EMPLOYEECONTACTID == entity.EMPLOYEECONTACTID
                //           select c;
                //if (ents != null)
                //{
                //    var ent = ents.FirstOrDefault();

                //    Utility.CloneEntity<T_HR_EMPLOYEECONTRACT>(entity, ent);
                //    if (entity.T_HR_EMPLOYEE != null)
                //    {
                //        ent.T_HR_EMPLOYEEReference.EntityKey =
                //            new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                //    }
                //    dal.Update(ent);
                //}
                #endregion

                entity.EntityKey =
                         new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEECONTRACT", "EMPLOYEECONTACTID", entity.EMPLOYEECONTACTID);
                if (entity.T_HR_EMPLOYEE != null)
                {
                    entity.T_HR_EMPLOYEEReference.EntityKey =
                        new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    entity.T_HR_EMPLOYEE.EntityKey =
                       new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                //dal.Update(entity);
                Update(entity,entity.CREATEUSERID);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeContractUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 可删除劳动合同组
        /// </summary>
        /// <param name="employeeCheckIDs">劳动合同ID组</param>
        /// <returns></returns>
        public int EmployeeContractDelete(string[] employeeContractIDs)
        {
            foreach (var id in employeeContractIDs)
            {
                T_HR_EMPLOYEECONTRACT ent = dal.GetObjects().FirstOrDefault(s => s.EMPLOYEECONTACTID == id);
                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    DeleteMyRecord(ent);
                }
            }
            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据ID查询劳动合同信息
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEECONTRACT GetEmployeeContractByID(string strID)
        {
            return dal.GetObjects().Include("T_HR_EMPLOYEE").FirstOrDefault(s => s.EMPLOYEECONTACTID == strID);
        }

        public T_HR_EMPLOYEECONTRACT GetEmployeeContractByEmployeeID(string employeeID)
        {
            var ent = from e in dal.GetObjects().Include("T_HR_EMPLOYEE")
                      where e.T_HR_EMPLOYEE.EMPLOYEEID == employeeID 
                      && e.CHECKSTATE=="2"//审核通过
                      orderby e.CREATEDATE descending
                      select e;
            return ent != null ? ent.ToList().FirstOrDefault() : null;//根据时间排序，得出最新的一条员工合同,这里toList后才能取到第一条数据，可能是数据还在数据库里面
          //  return dal.GetObjects().Include("T_HR_EMPLOYEE").FirstOrDefault(s => s.T_HR_EMPLOYEE.EMPLOYEEID == employeeID);
        }

        /// <summary>
        /// 员工合同到期提醒
        /// </summary>
        /// <param name="employeeCheck"></param>
        public void EmployeeContractAlarm(T_HR_EMPLOYEECONTRACT entity)
        {

            string submitName = "";
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                       where a.EMPLOYEEID == entity.OWNERID
                       select a;
            EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
            userMsg.FormID = entity.EMPLOYEECONTACTID;
            userMsg.UserID = entity.CREATEUSERID;
            EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
            List[0] = userMsg;
            if (ents.Count() > 0) submitName = ents.FirstOrDefault().EMPLOYEECNAME;
            SMT.Foundation.Log.Tracer.Debug("合同到期开始调用ApplicationMsgTrigger。ID:"+entity.EMPLOYEECONTACTID);
            Client.ApplicationMsgTrigger(List, "HR", "T_HR_EMPLOYEECONTRACT", Utility.ObjListToXml(entity, "HR", submitName), EngineWS.MsgType.Task);

        }
        /// <summary>
        /// 员工合同到期提醒xml
        /// </summary>
        /// <param name="entTemp"></param>
        public void GetEmployeeContractEngineXml(T_HR_EMPLOYEECONTRACT entTemp)
        {
            DateTime dtStart = System.DateTime.Now;
            string strStartTime = "10:00";
            int alarmDay = 0;
            if (entTemp.ALARMDAY != null)
            {
                alarmDay = Convert.ToInt32(entTemp.ALARMDAY);
            }
            else
            {
                alarmDay = 10;//默认为提前10天
            }
            if (entTemp.TODATE != null)
            {
                dtStart = Convert.ToDateTime(entTemp.TODATE).AddDays(-alarmDay);
            }
            //dtStart = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, remindDate);
            List<object> objArds = new List<object>();
            objArds.Add(entTemp.OWNERCOMPANYID);
            objArds.Add("HR");
            objArds.Add("T_HR_EMPLOYEECONTRACT");
            objArds.Add(entTemp.EMPLOYEECONTACTID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("");
            objArds.Add("");
            objArds.Add(entTemp.T_HR_EMPLOYEE.EMPLOYEECNAME + " 合同到期提醒");
            objArds.Add("");
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"EmployeeContractRemind\" Name=\"EMPLOYEECONTACTID\" Value=\"" + entTemp.EMPLOYEECONTACTID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("CustomBinding");
            SMT.Foundation.Log.Tracer.Debug("调用引擎信息，合同ID：" + entTemp.EMPLOYEECONTACTID);
            SMT.Foundation.Log.Tracer.Debug("调用引擎信息，员工的合同：" + entTemp.T_HR_EMPLOYEE.EMPLOYEECNAME);
            SMT.Foundation.Log.Tracer.Debug("开始调用引擎的默认消息");
            
            Utility.SendEngineEventTriggerData(objArds);
        }
        /// <summary>
        /// 更新合同状态
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            string strLogger = "进入EmployeeContractBLL:\r\n";
            strLogger += "strEntityName:" + strEntityName+"\r\n";
            strLogger += "EntityKeyName:" + EntityKeyName + "\r\n";
            strLogger += "EntityKeyValue:" + EntityKeyValue + "\r\n";
            strLogger += "CheckState:" + CheckState + "\r\n";
            SMT.Foundation.Log.Tracer.Debug(strLogger);
            try
            {
                int i = 0;
                var contract = (from c in dal.GetObjects<T_HR_EMPLOYEECONTRACT>().Include("T_HR_EMPLOYEE")
                                where c.EMPLOYEECONTACTID == EntityKeyValue 
                                select c).FirstOrDefault();
                if (contract != null)
                {
                    contract.CHECKSTATE = CheckState;
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        contract.EDITSTATE = "1";
                    }
                    contract.UPDATEDATE = DateTime.Now;
                    dal.UpdateFromContext(contract);
                    i = dal.SaveContextChanges();
                    if (i > 0)
                    {
                        //NoEndDate为0为固定合同（0非永久，1永久），进入提醒
                        //EndDate不为null时有效，进入提醒
                        Update(contract, contract.CREATEUSERID);
                        if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString()
                            && contract.NOENDDATE != "1"
                            && contract.TODATE !=null)
                        {
                            SMT.Foundation.Log.Tracer.Debug("tingxing");
                            GetEmployeeContractEngineXml(contract);
                        }
                    }
                }
                strLogger += "返回的信息i为" + i + "\r\n";
                SMT.Foundation.Log.Tracer.Debug(strLogger);
                return i;
            }
            catch (Exception e)
            {
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
            
        }

        /// <summary>
        ///  考勤异常提醒xml
        /// </summary>
        /// <param name="employeeCheck"></param>
        //public void GetEmployeeContractEngineXml(T_HR_EMPLOYEECONTRACT entTemp)
        //{
        //    DateTime dtStart = System.DateTime.Now;
        //    if (entTemp.ENDDATE != null)
        //    {
        //        dtStart = (DateTime)entTemp.ENDDATE;
                
        //        if (entTemp.CONTACTPERIOD != null)
        //        {
        //            dtStart = dtStart.AddDays((double)entTemp.CONTACTPERIOD);
        //        }
        //    }
        //    List<object> objArds = new List<object>();
        //    objArds.Add(entTemp.OWNERCOMPANYID);
        //    objArds.Add("HR");
        //    objArds.Add("T_HR_EMPLOYEECONTRACT");
        //    objArds.Add(entTemp.T_HR_EMPLOYEE.EMPLOYEEID);
        //    objArds.Add(dtStart.ToString("yyyy/MM/d"));
        //    objArds.Add(dtStart.ToString("HH:mm"));
        //    objArds.Add("");
        //    objArds.Add("");
        //    objArds.Add(entTemp.T_HR_EMPLOYEE.EMPLOYEECNAME+"的合同即将到期，请处理");
        //    objArds.Add("");
        //    objArds.Add(Utility.strEngineFuncWSSite);
        //    objArds.Add("EventTriggerProcess");
        //    objArds.Add("<Para Name=\"EmployeeContractRemind\" Name=\"EMPLOYEECONTACTID\" Value=\"" + entTemp.EMPLOYEECONTACTID + "\"></Para>");
        //    objArds.Add("Г");
        //    objArds.Add("basicHttpBinding");

        //    Utility.SendEngineEventTriggerData(objArds);
        //}

    }
}
