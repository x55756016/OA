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
    public class EmployeeCheckBLL : BaseBll<T_HR_EMPLOYEECHECK>, IOperate
    {
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
        public IQueryable<T_HR_EMPLOYEECHECK> EmployeeCheckPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string strCheckstate, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            if (strCheckstate != Convert.ToInt32(CheckStates.WaittingApproval).ToString())// 如果不是待审核 不取流程数据，是待审核就只查流程中待审核数据
            {
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEECHECK");



                if (strCheckstate != Convert.ToInt32(CheckStates.All).ToString())
                {
                    if (queryParas.Count() > 0)
                    {
                        filterString += " AND ";
                    }

                    filterString += "CHECKSTATE==@" + queryParas.Count().ToString();
                    queryParas.Add(strCheckstate);
                }
            }
            else
            {
                SetFilterWithflow("BEREGULARID", "T_HR_EMPLOYEECHECK", userID, ref strCheckstate, ref filterString, ref queryParas);
                if (queryParas.Count() == paras.Count)
                {
                    return null;
                }
            }

            IQueryable<T_HR_EMPLOYEECHECK> ents = dal.GetObjects().Include("T_HR_EMPLOYEE").Where(v=>v.T_HR_EMPLOYEE.EMPLOYEECODE!=null);
            
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            
            ents = Utility.Pager<T_HR_EMPLOYEECHECK>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }
        /// <summary>
        /// Add by luojie
        /// EmployeeCheckPaging获取转正信息时缺少获取code，这个是补充的方法
        /// </summary>
        /// <param name="employeeCheck"></param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEECHECK> EmployeecheckWithEmployeecode(IQueryable<T_HR_EMPLOYEECHECK> employeeCheck)
        {
            try
            {
                List<T_HR_EMPLOYEECHECK> rslCheck=null;
                if (employeeCheck != null)
                {
                    rslCheck = employeeCheck.ToList();
                    foreach (var rc in rslCheck)
                    {
                        var employeeCode = from ec in dal.GetObjects<T_HR_EMPLOYEE>()
                                           where ec.EMPLOYEEID == rc.T_HR_EMPLOYEE.EMPLOYEEID
                                           select ec.EMPLOYEECODE;
                        if(employeeCode != null)
                            rc.EMPLOYEECODE = employeeCode.FirstOrDefault() ;
                    }
                }
                return rslCheck==null?null:rslCheck;
            }
            catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("EmployeeCheckBLL-EmployeeWithEmployeecode:"+ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// 添加转正审核信息
        /// </summary>
        /// <param name="entity">转正审核实体</param>
        public void EmployeeCheckAdd(T_HR_EMPLOYEECHECK entity, ref string strMsg)
        {
            try
            {
                var tmp = from c in dal.GetObjects()
                          where c.T_HR_EMPLOYEE.EMPLOYEEID == entity.T_HR_EMPLOYEE.EMPLOYEEID && (c.CHECKSTATE == "0" || c.CHECKSTATE == "1")
                          select c;
                if (tmp.Count() > 0)
                {
                    // throw new Exception("EMPLOYEECHECKSUBMITTED");
                    strMsg = "EMPLOYEECHECKSUBMITTED";
                    return;
                }
                T_HR_EMPLOYEECHECK ent = new T_HR_EMPLOYEECHECK();
                Utility.CloneEntity<T_HR_EMPLOYEECHECK>(entity, ent);
                ent.T_HR_EMPLOYEEReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);

               //dal.Add(ent);
                Add(ent,ent.CREATEUSERID);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeCheckAdd:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 修改转正审核信息
        /// </summary>
        /// <param name="entity">转正审核实体</param>
        public void EmployeeCheckUpdate(T_HR_EMPLOYEECHECK entity)
        {
            try
            {
                T_HR_EMPLOYEECHECK ent = dal.GetObjects().Include("T_HR_EMPLOYEE").FirstOrDefault(s => s.BEREGULARID == entity.BEREGULARID);
                Utility.SaveTriggerData<T_HR_EMPLOYEECHECK>(ent, entity);
                if (ent != null)
                {
                    if (entity.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //EmployeeBLL bll = new EmployeeBLL();
                        //T_HR_EMPLOYEE empoyee = ent.T_HR_EMPLOYEE;
                        //empoyee.EMPLOYEESTATE = "1";
                        //bll.EmployeeUpdate(empoyee, ref strMsg);
                        var employee = dal.GetObjects<T_HR_EMPLOYEE>().FirstOrDefault(c => c.EMPLOYEEID == ent.T_HR_EMPLOYEE.EMPLOYEEID);
                        if (employee != null)
                        {
                            employee.EMPLOYEESTATE = "1";
                            dal.UpdateFromContext(employee);
                            dal.SaveContextChanges();
                        }
                    }
                    Utility.CloneEntity<T_HR_EMPLOYEECHECK>(entity, ent);
                    if (entity.T_HR_EMPLOYEE != null)
                    {
                        ent.T_HR_EMPLOYEEReference.EntityKey =
                                           new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", entity.T_HR_EMPLOYEE.EMPLOYEEID);
                    }
                    //dal.Update(ent);
                    Update(ent,ent.CREATEUSERID);

                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " EmployeeCheckUpdate:" + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// 可删除转正审核组
        /// </summary>
        /// <param name="employeeCheckIDs">转正审核ID组</param>
        /// <returns></returns>
        public int EmployeeCheckDelete(string[] employeeCheckIDs)
        {
            foreach (var id in employeeCheckIDs)
            {
                T_HR_EMPLOYEECHECK ent = dal.GetObjects().FirstOrDefault(s => s.BEREGULARID == id);
                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                    DeleteMyRecord(ent);
                }
            }
            return dal.SaveContextChanges();
        }
        /// <summary>
        /// 根据转正审核ID获取信息
        /// </summary>
        /// <param name="employeeCheckID">转正审核ID</param>
        /// <returns>转正审核实体</returns>
        public T_HR_EMPLOYEECHECK GetEmployeeCheckByID(string employeeCheckID)
        {
            return dal.GetObjects().Include("T_HR_EMPLOYEE").FirstOrDefault(s => s.BEREGULARID == employeeCheckID);
        }
        /// <summary>
        /// 指定转正提醒日期
        /// </summary>
        /// <param name="employeeCheck"></param>
        public void EmployeeCheckAlarm(T_HR_EMPLOYEECHECK employeeCheck)
        {

            string submitName = "";
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEE>()
                       where a.EMPLOYEEID == employeeCheck.OWNERID
                       select a;
            EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
            EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
            userMsg.FormID = employeeCheck.BEREGULARID;
            userMsg.UserID = employeeCheck.CREATEUSERID;
            EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
            List[0] = userMsg;
            if (ents.Count() > 0) submitName = ents.FirstOrDefault().EMPLOYEECNAME;
            Client.ApplicationMsgTrigger(List, "HR", "T_HR_EMPLOYEECHECK", Utility.ObjListToXml(employeeCheck, "HR", submitName), EngineWS.MsgType.Msg);
            Client.ApplicationMsgTrigger(List, "HR", "T_HR_EMPLOYEECHECK", Utility.ObjListToXml(employeeCheck, "HR", submitName), EngineWS.MsgType.Task);

        }
        /// <summary>
        ///  指定转正提醒日期xml
        /// </summary>
        /// <param name="employeeCheck"></param>
        public void GetEmployeeCheckEngineXml(T_HR_EMPLOYEEENTRY entTemp)
        {
            DateTime dtStart = System.DateTime.Now;
            if (entTemp.ENTRYDATE != null)
            {
                dtStart = (DateTime)entTemp.ENTRYDATE;
            }
            string strStartTime = "10:00";
            int AddDays = 0;
            if (entTemp.PROBATIONPERIOD != null)
            {
                AddDays = (int)entTemp.PROBATIONPERIOD;
                AddDays = AddDays - 5;//提前5天提醒
            }
            dtStart = dtStart.AddDays(AddDays);
            //dtStart = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, remindDate);
            List<object> objArds = new List<object>();
            objArds.Add(entTemp.OWNERCOMPANYID);
            objArds.Add("HR");
            objArds.Add("T_HR_EMPLOYEECHECK");
            objArds.Add(entTemp.T_HR_EMPLOYEE.EMPLOYEEID);
            objArds.Add(dtStart.ToString("yyyy/MM/d"));
            objArds.Add(strStartTime);
            objArds.Add("");
            objArds.Add("");
            objArds.Add(entTemp.T_HR_EMPLOYEE.EMPLOYEECNAME + "试用期即将完成！");
            objArds.Add("");
            objArds.Add(Utility.strEngineFuncWSSite);
            objArds.Add("EventTriggerProcess");
            objArds.Add("<Para FuncName=\"EmployeeCheckRemind\" Name=\"BEREGULARID\" Value=\"" + entTemp.EMPLOYEEENTRYID + "\"></Para>");
            objArds.Add("Г");
            objArds.Add("basicHttpBinding");

            Utility.SendEngineEventTriggerData(objArds);
        }

        /// <summary>
        /// 服务引擎调用
        /// </summary>
        /// <param name="strEntityName"></param>
        /// <param name="EntityKeyName"></param>
        /// <param name="EntityKeyValue"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public int UpdateCheckState(string strEntityName, string EntityKeyName, string EntityKeyValue, string CheckState)
        {
            try
            {
                int i = 0;
                dal.BeginTransaction();
                //根据转正表ID查询员工转正表信息
                var employeeCheck = (from c in dal.GetObjects<T_HR_EMPLOYEECHECK>().Include("T_HR_EMPLOYEE")
                                     where c.BEREGULARID == EntityKeyValue
                                     select c).FirstOrDefault();
                if (employeeCheck != null)
                {
                    //转正审核状态
                    employeeCheck.CHECKSTATE = CheckState;
                    //审核通过
                    var employee = dal.GetObjects<T_HR_EMPLOYEE>().FirstOrDefault(c => c.EMPLOYEEID == employeeCheck.T_HR_EMPLOYEE.EMPLOYEEID);
                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //根据员工ID查询员工表
                        if (employee != null)
                        {
                            //更改员工状态：0试用 1在职 2已离职 3离职中
                            employee.EMPLOYEESTATE = "1";
                            //更新员工表
                            dal.UpdateFromContext(employee);
                        }
                    }
                    
                    //更新员工转正表更改的时间，去当前修改时间
                    employeeCheck.UPDATEDATE = DateTime.Now;
                    //修改员工转正表
                    dal.UpdateFromContext(employeeCheck);

                    if (CheckState == Convert.ToInt32(CheckStates.Approved).ToString())
	                {
                        #region 员工转正服务同步 weirui 2012-7-10
                        T_HR_EMPLOYEECHANGEHISTORY employeeEntity = new T_HR_EMPLOYEECHANGEHISTORY();
                        employeeEntity.RECORDID = Guid.NewGuid().ToString();
                        //员工ID
                        //employeeEntity.T_HR_EMPLOYEE.EMPLOYEEID = employeeCheck.T_HR_EMPLOYEE.EMPLOYEEID;
                        employeeEntity.T_HR_EMPLOYEEReference.EntityKey =
                                             new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEE", "EMPLOYEEID", employeeCheck.T_HR_EMPLOYEE.EMPLOYEEID);
                        //员工姓名
                        employeeEntity.EMPOLYEENAME = employeeCheck.EMPLOYEENAME;
                        //指纹编号
                        employeeEntity.FINGERPRINTID = employee.FINGERPRINTID;
                        //0.入职1.异动2.离职3.薪资级别变更4.转正
                        employeeEntity.FORMTYPE = "4";
                        //记录原始单据id（员工入职表ID）
                        employeeEntity.FORMID = employeeCheck.BEREGULARID;
                        //根据员工ID查询员工岗位表
                        var employeePost = dal.GetObjects<T_HR_EMPLOYEEPOST>().FirstOrDefault(s => s.T_HR_EMPLOYEE.EMPLOYEEID == employeeCheck.T_HR_EMPLOYEE.EMPLOYEEID);
                        //var employeePost =from c in dal.GetObjects<T_HR_EMPLOYEEPOST>
                        //主岗位非主岗位
                        if (employeePost != null)
                        {
                            employeeEntity.ISMASTERPOSTCHANGE = employeePost.ISAGENCY;
                        }
                        //备注
                        employeeEntity.REMART = employeeCheck.REMARK;
                        //创建时间
                        employeeEntity.CREATEDATE = DateTime.Now;
                        //所属员工ID
                        employeeEntity.OWNERID = employeeCheck.OWNERID;
                        //所属岗位ID
                        employeeEntity.OWNERPOSTID = employeeCheck.OWNERPOSTID;
                        //所属部门ID
                        employeeEntity.OWNERDEPARTMENTID = employeeCheck.OWNERDEPARTMENTID;
                        //所属公司ID
                        employeeEntity.OWNERCOMPANYID = employeeCheck.OWNERCOMPANYID;
                        dal.AddToContext(employeeEntity);
                        #endregion
                    } 
                    
                    
                    i = dal.SaveContextChanges();
                }
                dal.CommitTransaction();
                return i;
            }
            catch (Exception e)
            {
                dal.RollbackTransaction();
                SMT.Foundation.Log.Tracer.Debug("FormID:" + EntityKeyValue + " UpdateCheckState:" + e.Message);
                return 0;
            }
        }


        
    }
}
