using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Data.Objects.DataClasses;
using System.Collections;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using System.IO;
using System.Data;
using System.Data.OleDb;
using SMT.HRM.BLL.Common;
using System.Text;
using EngineWS = SMT.SaaS.BLLCommonServices.EngineConfigWS;
using SMT.Foundation.Log;
namespace SMT.HRM.BLL
{
    public class PaymentBLL : BaseBll<V_PAYMENT>
    {
        private BaseBll<T_HR_EMPLOYEESALARYRECORD> dall = new BaseBll<T_HR_EMPLOYEESALARYRECORD>();
        protected SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient FBSclient = new SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceClient();
        /// <summary>
        /// 更新薪资发放实体
        /// </summary>
        /// <param name="entity">薪资发放实体</param>
        /// <returns></returns>
        public void PaymentUpdate(T_HR_EMPLOYEESALARYRECORD entity)
        {
            try
            {
                var ents = from a in dall.GetTable()
                           where a.EMPLOYEESALARYRECORDID == entity.EMPLOYEESALARYRECORDID
                           select a;
                if (ents.Count() > 0)
                {
                    List<T_HR_EMPLOYEESALARYRECORD> list = new List<T_HR_EMPLOYEESALARYRECORD>();
                    var ent = ents.FirstOrDefault();
                    ent.PAIDDATE = entity.PAIDDATE;
                    ent.PAIDBY = entity.PAIDBY;
                    ent.PAIDTYPE = entity.PAIDTYPE;
                    ent.PAYCONFIRM = entity.PAYCONFIRM;
                    list.Add(entity);
                    if (SalaryBudgetDeduct(list, entity.SALARYYEAR, entity.SALARYMONTH))
                    {
                        dall.Update(ent);
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }

        ///// <summary>
        ///// 薪资发放发引擎消息
        ///// </summary>
        ///// <param name="salarysolution"></param>
        //public void PayEngineMsg(List<T_HR_EMPLOYEESALARYRECORD> employeesalaryList)
        //{
        //    try
        //    {
        //        EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
        //        foreach (T_HR_EMPLOYEESALARYRECORD employeesalary in employeesalaryList)
        //        {
        //            try
        //            {
        //                string submitName = string.Empty;
        //                EngineWS.CustomUserMsg[] List = new EngineWS.CustomUserMsg[1];
        //                EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
        //                userMsg.FormID = Guid.NewGuid().ToString();
        //                userMsg.UserID = employeesalary.EMPLOYEEID;
        //                List[0] = userMsg;
        //                string appXML = Utility.ObjListToXml(employeesalary, "HR", submitName);
        //                if(!string.IsNullOrEmpty(appXML))
        //                {
        //                    Client.ApplicationMsgTrigger(List, "HR", "T_HR_EMPLOYEESALARYRECORD", appXML,
        //                    EngineWS.MsgType.Msg);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Utility.SaveLog("向员工" + employeesalary.EMPLOYEENAME + "发送薪资发放确认提醒的消息及邮件失败！错误信息为：" + ex.ToString()); 
        //                continue;
        //            }
        //            Utility.SaveLog("向员工" + employeesalary.EMPLOYEENAME + "发送薪资发放确认提醒的消息及邮件成功！");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.SaveLog("向员工发送薪资发放确认提醒的消息及邮件失败！错误信息为：" + ex.ToString()); 
        //    }
        //}
        /// <summary>
        /// 薪资发放发引擎消息
        /// </summary>
        /// <param name="salarysolution"></param>
        public void PayEngineMsg(List<T_HR_EMPLOYEESALARYRECORD> employeesalaryList)
        {
            try
            {
                EngineWS.EngineWcfGlobalFunctionClient Client = new EngineWS.EngineWcfGlobalFunctionClient();
                List<EngineWS.CustomUserMsg> listUserMsg = new List<EngineWS.CustomUserMsg>();
                foreach (var item in employeesalaryList)
                {
                    EngineWS.CustomUserMsg userMsg = new EngineWS.CustomUserMsg();
                    userMsg.UserID = item.EMPLOYEEID + "|" + Guid.NewGuid().ToString();
                    userMsg.FormID = Utility.ObjListToXml(item, "HR", string.Empty);
                    listUserMsg.Add(userMsg);
                }
                string strMsg = Client.SendTaskMessage(listUserMsg.ToArray(), "HR", "T_HR_EMPLOYEESALARYRECORD");//批量发送消息
                if (strMsg == "1")
                {
                    Utility.SaveLog("发送薪资发放确认提醒的消息及邮件成功！");
                }
                else
                {
                    Utility.SaveLog("发送薪资发放确认提醒的消息及邮件失败！");
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("向员工发送薪资发放确认提醒的消息及邮件失败！错误信息为：" + ex.ToString());
            }
        }

        /// <summary>
        /// 薪资发放确认
        /// </summary>
        /// <param name="entitys">薪资发放实体集</param>
        /// <returns></returns>
        public void PaymentConfirmUpdate(List<T_HR_EMPLOYEESALARYRECORD> entitys)
        {
            try
            {
                List<T_HR_EMPLOYEESALARYRECORD> list = new List<T_HR_EMPLOYEESALARYRECORD>();
                foreach (T_HR_EMPLOYEESALARYRECORD en in entitys)
                {
                    var ents = from a in dall.GetTable()
                               where a.EMPLOYEESALARYRECORDID == en.EMPLOYEESALARYRECORDID
                               select a;
                    if (ents.Count() > 0)
                    {
                        var ent = ents.FirstOrDefault();
                        ent.PAIDDATE = en.PAIDDATE;
                        ent.PAIDBY = en.PAIDBY;
                        //ent.PAIDTYPE = en.PAIDTYPE;
                        ent.PAYCONFIRM = en.PAYCONFIRM;
                        list.Add(ent);

                        dall.Update(ent);
                        EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                        string sql = "UPDATE T_HR_EMPLOYEESALARYRECORD t SET t.PAYCONFIRM = '" + ent.PAYCONFIRM + "'";
                        sql += " ,t.PAIDDATE = TO_DATE('" + ent.PAIDDATE + "', 'YYYY-MM-DD HH24:MI:SS'),t.PAIDBY = '" + ent.PAIDBY + "'";
                        sql += " WHERE t.EMPLOYEESALARYRECORDID = '" + en.EMPLOYEESALARYRECORDID + "'";
                        bll.ExecuteSql(sql, "T_HR_EMPLOYEESALARYRECORD");
                    }
                }
                try
                {
                    PayEngineMsg(entitys);
                    SalaryBudgetDeduct(list, entitys[0].SALARYYEAR, entitys[0].SALARYMONTH);
                }
                catch (Exception e)
                {
                    Utility.SaveLog("触发引擎消息发生异常，异常消息为：" + e.ToString());
                }
            }
            catch (Exception ex)
            {
                Utility.SaveLog("薪资发放确认时发生异常，异常消息为：" + ex.ToString());
                throw ex;
            }
        }

        public void BankPaymentUpdate(V_PAYMENT entity)
        {
            try
            {
                var ents = from a in dall.GetTable()
                           where a.EMPLOYEESALARYRECORDID == entity.EMPLOYEESALARYRECORDID
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    ent.PAIDDATE = entity.PAIDDATE;
                    ent.PAIDBY = entity.PAIDBY;
                    ent.PAIDTYPE = entity.PAYTYPE;
                    ent.PAYCONFIRM = Convert.ToInt32(PaymentState.PAYMENTING).ToString();
                    //dall.Update(ent);
                    EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                    string sql = "UPDATE T_HR_EMPLOYEESALARYRECORD t SET t.PAYCONFIRM = '" + ent.PAYCONFIRM + "'";
                    sql += " ,t.PAIDDATE = TO_DATE('" + ent.PAIDDATE + "', 'YYYY-MM-DD HH24:MI:SS'),t.PAIDBY = '" + ent.PAIDBY + "',t.PAIDTYPE = '" + ent.PAIDTYPE + "'";
                    sql += " WHERE t.EMPLOYEESALARYRECORDID = '" + ent.EMPLOYEESALARYRECORDID + "'";
                    bll.ExecuteSql(sql, "T_HR_EMPLOYEESALARYRECORD");
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 获取薪资发放实体(一条)
        /// </summary>
        /// <param name="employeeid">员工ID</param>
        /// <returns>返回薪资发放实体(一条)</returns>
        public T_HR_EMPLOYEESALARYRECORD GetSalaryRecordOne(string employeeid, string year, string month)
        {
            try
            {
                var ents = from a in dall.GetTable()
                           where a.EMPLOYEEID == employeeid
                           select a;
                ents = ents.Where(m => m.SALARYYEAR == year);
                ents = ents.Where(m => m.SALARYMONTH == month);
                if (ents.Count() > 0)
                {
                    return ents.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                throw ex;
            }
            return null;
        }

        /// <summary>
        /// 获取薪资发放实体
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>返回薪资发放实体</returns>
        public IQueryable<V_PAYMENT> GetPaymentPaging(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string year, string month)
        {

            List<V_PAYMENT> ent = new List<V_PAYMENT>();
            IQueryable<V_PAYMENT> ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                         join b in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                                         join c in dal.GetObjects<T_HR_SALARYSTANDARD>() on b.T_HR_SALARYSTANDARD.SALARYSTANDARDID equals c.SALARYSTANDARDID
                                         join d in dal.GetObjects<T_HR_SALARYSOLUTION>() on b.SALARYSOLUTIONID equals d.SALARYSOLUTIONID
                                         join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                                         select new V_PAYMENT
                                         {
                                             ACTUALLYPAY = a.ACTUALLYPAY,
                                             ATTENDANCEUNUSUALDEDUCT = a.ATTENDANCEUNUSUALDEDUCT,
                                             BANKACCOUNTNO = d.BANKACCOUNTNO,
                                             BANKCARDNUMBER = e.BANKCARDNUMBER,
                                             BANKNAME = d.BANKNAME,
                                             BASICSALARY = a.BASICSALARY,
                                             BLANKID = e.BANKID,
                                             EMPLOYEECODE = a.EMPLOYEECODE,
                                             EMPLOYEEID = a.EMPLOYEEID,
                                             EMPLOYEENAME = a.EMPLOYEENAME,
                                             EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                                             PAYTYPE = a.PAIDTYPE,
                                             PERFORMANCESUM = a.PERFORMANCESUM,
                                             RECORDCREATEDATE = a.CREATEDATE,
                                             ARCHIVECREATEDATE = b.CREATEDATE,
                                             CHECKSTATE = a.CHECKSTATE,
                                             PAYCONFIRM = a.PAYCONFIRM,
                                             PAIDBY = a.PAIDBY,
                                             SALARYYEAR = a.SALARYYEAR,
                                             SALARYMONTH = a.SALARYMONTH,
                                             PAIDDATE = a.PAIDDATE
                                         };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
                ents = ents.Where(m => m.SALARYYEAR == year);
                ents = ents.Where(m => m.SALARYMONTH == month);
            }
            ents = ents.OrderBy(sort);
            var en = ents.GroupBy(y => y.EMPLOYEEID).Select(g => new { group = g.Key, groupcontent = g });
            foreach (var v in en)
            {
                ent.Add(v.groupcontent.FirstOrDefault());
            }
            ents = ent.AsQueryable();
            ents = Utility.Pager<V_PAYMENT>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        public IQueryable<T_HR_EMPLOYEESALARYRECORD> GetPaymentPagings(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string year, string month, int orgtype, string orgid, string userid)
        {
            List<object> queryParas = new List<object>();
            List<T_HR_EMPLOYEESALARYRECORD> ent = new List<T_HR_EMPLOYEESALARYRECORD>();
            IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = GetResultset(orgtype, orgid);

            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_EMPLOYEESALARYRECORD");

            if (!string.IsNullOrEmpty(filterString))
            {
                //ents = ents.Where(filterString, paras.ToArray());
                ents = ents.Where(filterString, queryParas.ToArray());
                ents = ents.Where(m => m.SALARYYEAR == year);
                ents = ents.Where(m => m.SALARYMONTH == month);
            }
            ents = ents.OrderBy(sort);
            var en = ents.GroupBy(y => y.EMPLOYEEID).Select(g => new { group = g.Key, groupcontent = g });
            foreach (var v in en)
            {
                ent.Add(v.groupcontent.FirstOrDefault());
            }
            ents = ent.AsQueryable();
            ents = Utility.Pager<T_HR_EMPLOYEESALARYRECORD>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        public IQueryable<T_HR_EMPLOYEESALARYRECORD> GetResultset(int orgtype, string orgid)
        {
            IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>();
            switch (orgtype)
            {
                case 0:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.T_HR_COMPANY.COMPANYID == orgid && b.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                           select a;
                    break;
                case 1:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           join d in dal.GetObjects<T_HR_DEPARTMENT>() on c.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where d.DEPARTMENTID == orgid && b.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                           select a;
                    break;
                case 2:
                    ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on a.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                           join c in dal.GetObjects<T_HR_POST>() on b.T_HR_POST.POSTID equals c.POSTID
                           //join e in dal.GetObjects.T_HR_SALARYARCHIVE on a.EMPLOYEEID equals e.EMPLOYEEID
                           where c.POSTID == orgid && b.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                           select a;
                    break;
            }
            return ents;
        }

        /// <summary>
        /// 获取薪资发放实体
        /// </summary>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="orgtype">orgtype{ 0为公司  1为部门 2 为岗位 }</param>
        /// <returns>返回薪资发放实体</returns>
        public IQueryable<V_PAYMENT> GetPayment(string sort, string filterString, IList<object> paras, string year, string month, int orgtype, string orgid)
        {
            IQueryable<V_PAYMENT> ents;
            List<V_PAYMENT> ent = new List<V_PAYMENT>();
            IQueryable<V_PAYMENT> ents1 = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                          join b in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                                          //join c in dal.GetObjects.T_HR_SALARYSTANDARD on b.T_HR_SALARYSTANDARD.SALARYSTANDARDID equals c.SALARYSTANDARDID                                         
                                          join d in dal.GetObjects<T_HR_SALARYSOLUTION>() on b.SALARYSOLUTIONID equals d.SALARYSOLUTIONID
                                          join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                                          join f in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals f.T_HR_EMPLOYEE.EMPLOYEEID
                                          join g in dal.GetObjects<T_HR_POST>() on f.T_HR_POST.POSTID equals g.POSTID
                                          join h in dal.GetObjects<T_HR_DEPARTMENT>() on g.T_HR_DEPARTMENT.DEPARTMENTID equals h.DEPARTMENTID
                                          where h.T_HR_COMPANY.COMPANYID == orgid && f.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                                          select new V_PAYMENT
                                          {
                                              ACTUALLYPAY = a.ACTUALLYPAY,
                                              ATTENDANCEUNUSUALDEDUCT = a.ATTENDANCEUNUSUALDEDUCT,
                                              BANKACCOUNTNO = d.BANKACCOUNTNO,
                                              BANKCARDNUMBER = e.BANKCARDNUMBER,
                                              BANKNAME = d.BANKNAME,
                                              BASICSALARY = a.BASICSALARY,
                                              BLANKID = e.BANKID,
                                              EMPLOYEECODE = a.EMPLOYEECODE,
                                              EMPLOYEEID = a.EMPLOYEEID,
                                              EMPLOYEENAME = a.EMPLOYEENAME,
                                              EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                                              PAYTYPE = a.PAIDTYPE,
                                              PERFORMANCESUM = a.PERFORMANCESUM,
                                              RECORDCREATEDATE = a.CREATEDATE,
                                              ARCHIVECREATEDATE = b.CREATEDATE,
                                              CHECKSTATE = a.CHECKSTATE,
                                              PAYCONFIRM = a.PAYCONFIRM,
                                              PAIDBY = a.PAIDBY,
                                              SALARYYEAR = a.SALARYYEAR,
                                              SALARYMONTH = a.SALARYMONTH,
                                              PAIDDATE = a.PAIDDATE
                                          };

            IQueryable<V_PAYMENT> ents2 = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                          join b in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                                          //join c in dal.GetObjects.T_HR_SALARYSTANDARD on b.T_HR_SALARYSTANDARD.SALARYSTANDARDID equals c.SALARYSTANDARDID
                                          join d in dal.GetObjects<T_HR_SALARYSOLUTION>() on b.SALARYSOLUTIONID equals d.SALARYSOLUTIONID
                                          join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                                          join f in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals f.T_HR_EMPLOYEE.EMPLOYEEID
                                          join g in dal.GetObjects<T_HR_POST>() on f.T_HR_POST.POSTID equals g.POSTID
                                          join h in dal.GetObjects<T_HR_DEPARTMENT>() on g.T_HR_DEPARTMENT.DEPARTMENTID equals h.DEPARTMENTID
                                          where h.DEPARTMENTID == orgid && f.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                                          select new V_PAYMENT
                                          {
                                              ACTUALLYPAY = a.ACTUALLYPAY,
                                              ATTENDANCEUNUSUALDEDUCT = a.ATTENDANCEUNUSUALDEDUCT,
                                              BANKACCOUNTNO = d.BANKACCOUNTNO,
                                              BANKCARDNUMBER = e.BANKCARDNUMBER,
                                              BANKNAME = d.BANKNAME,
                                              BASICSALARY = a.BASICSALARY,
                                              BLANKID = e.BANKID,
                                              EMPLOYEECODE = a.EMPLOYEECODE,
                                              EMPLOYEEID = a.EMPLOYEEID,
                                              EMPLOYEENAME = a.EMPLOYEENAME,
                                              EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                                              PAYTYPE = a.PAIDTYPE,
                                              PERFORMANCESUM = a.PERFORMANCESUM,
                                              RECORDCREATEDATE = a.CREATEDATE,
                                              ARCHIVECREATEDATE = b.CREATEDATE,
                                              CHECKSTATE = a.CHECKSTATE,
                                              PAYCONFIRM = a.PAYCONFIRM,
                                              PAIDBY = a.PAIDBY,
                                              SALARYYEAR = a.SALARYYEAR,
                                              SALARYMONTH = a.SALARYMONTH,
                                              PAIDDATE = a.PAIDDATE
                                          };

            IQueryable<V_PAYMENT> ents3 = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                          join b in dal.GetObjects<T_HR_SALARYARCHIVE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                                          //join c in dal.GetObjects.T_HR_SALARYSTANDARD on b.T_HR_SALARYSTANDARD.SALARYSTANDARDID equals c.SALARYSTANDARDID
                                          join d in dal.GetObjects<T_HR_SALARYSOLUTION>() on b.SALARYSOLUTIONID equals d.SALARYSOLUTIONID
                                          join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                                          join f in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals f.T_HR_EMPLOYEE.EMPLOYEEID
                                          join g in dal.GetObjects<T_HR_POST>() on f.T_HR_POST.POSTID equals g.POSTID
                                          where f.T_HR_POST.POSTID == orgid && f.ISAGENCY == "0"  //&& b.EDITSTATE == "1"
                                          select new V_PAYMENT
                                          {
                                              ACTUALLYPAY = a.ACTUALLYPAY,
                                              ATTENDANCEUNUSUALDEDUCT = a.ATTENDANCEUNUSUALDEDUCT,
                                              BANKACCOUNTNO = d.BANKACCOUNTNO,
                                              BANKCARDNUMBER = e.BANKCARDNUMBER,
                                              BANKNAME = d.BANKNAME,
                                              BASICSALARY = a.BASICSALARY,
                                              BLANKID = e.BANKID,
                                              EMPLOYEECODE = a.EMPLOYEECODE,
                                              EMPLOYEEID = a.EMPLOYEEID,
                                              EMPLOYEENAME = a.EMPLOYEENAME,
                                              EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                                              PAYTYPE = a.PAIDTYPE,
                                              PERFORMANCESUM = a.PERFORMANCESUM,
                                              RECORDCREATEDATE = a.CREATEDATE,
                                              ARCHIVECREATEDATE = b.CREATEDATE,
                                              CHECKSTATE = a.CHECKSTATE,
                                              PAYCONFIRM = a.PAYCONFIRM,
                                              PAIDBY = a.PAIDBY,
                                              SALARYYEAR = a.SALARYYEAR,
                                              SALARYMONTH = a.SALARYMONTH,
                                              PAIDDATE = a.PAIDDATE
                                          };

            ents = ents1;
            switch (orgtype)
            {
                case 0:
                    ents = ents1;
                    break;
                case 1:
                    ents = ents2;
                    break;
                case 2:
                    ents = ents3;
                    break;
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, paras.ToArray());
                ents = ents.Where(m => m.SALARYYEAR == year);
                ents = ents.Where(m => m.SALARYMONTH == month);
            }
            ents = ents.OrderBy(sort);
            var en = ents.GroupBy(y => y.EMPLOYEEID).Select(g => new { group = g.Key, groupcontent = g });
            foreach (var v in en)
            {
                ent.Add(v.groupcontent.FirstOrDefault());
            }
            ents = ent.AsQueryable();

            return ents;
        }

        /// <summary>
        /// 导出EXCEL(完整数据)
        /// </summary>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <returns>返回</returns>
        public byte[] ExportExcelAll(string sort, string filterString, IList<object> paras, string year, string month, int orgtype, string orgid)
        {


            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string userid = "";
            if (queryParas != null)
            {
                int i = queryParas.Count;
                userid = queryParas[i - 1].ToString();
                queryParas.RemoveAt(i - 1);
                SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_EMPLOYEESALARYRECORD");
            }
            else
            {
                return null;
            }
            string strMsg = string.Empty;
            string ptstr = Convert.ToInt32(PaidType.BANKSUBSTITUTING).ToString();
            byte[] result;
            DataTable dt;
            var tmpData = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                          join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                          where a.SALARYYEAR == year && a.SALARYMONTH == month && a.PAIDTYPE == ptstr && a.OWNERCOMPANYID == orgid
                          select new
                          {
                              ACTUALLYPAY = a.ACTUALLYPAY,
                              ATTENDANCEUNUSUALDEDUCT = a.ATTENDANCEUNUSUALDEDUCT,
                              //BANKACCOUNTNO = d.BANKACCOUNTNO,
                              BANKCARDNUMBER = e.BANKCARDNUMBER,
                              //BANKNAME = d.BANKNAME,
                              BASICSALARY = a.BASICSALARY,
                              BLANKID = e.BANKID,
                              EMPLOYEECODE = a.EMPLOYEECODE,
                              EMPLOYEEID = a.EMPLOYEEID,
                              EMPLOYEENAME = a.EMPLOYEENAME,
                              EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                              PAYTYPE = a.PAIDTYPE,
                              PERFORMANCESUM = a.PERFORMANCESUM,
                              RECORDCREATEDATE = a.CREATEDATE,
                              // ARCHIVECREATEDATE = b.CREATEDATE,
                              CHECKSTATE = a.CHECKSTATE,
                              PAYCONFIRM = a.PAYCONFIRM,
                              PAIDBY = a.PAIDBY,
                              SALARYYEAR = a.SALARYYEAR,
                              SALARYMONTH = a.SALARYMONTH,
                              PAIDDATE = a.PAIDDATE,
                              OWNERCOMPANYID = a.OWNERCOMPANYID,
                              OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                              OWNERPOSTID = a.OWNERPOSTID,
                              OWNERID = a.OWNERID,
                              CREATEUSERID = a.CREATEUSERID,
                              EMPLOYEESALARYRECORDITEMS = a.T_HR_EMPLOYEESALARYRECORDITEM
                          };

            tmpData = tmpData.Where(filterString, queryParas.ToArray());

            long totalCount = tmpData.Count();

            if (totalCount > 2147483647)
            {
                strMsg = "OVERMAXEXPORTSIZE";
                return null;
            }

            if (totalCount == 0)
            {
                strMsg = "NOEXPORTDATA";
                return null;
            }


            #region 生成导出表的结构
            dt = new DataTable();
            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = Utility.GetResourceStr("BANKCARDNUMBER");
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colCordED = new DataColumn();
            colCordED.ColumnName = Utility.GetResourceStr("EMPLOYEENAME");
            colCordED.DataType = typeof(string);
            dt.Columns.Add(colCordED);

            DataColumn colCordBank = new DataColumn();
            colCordBank.ColumnName = Utility.GetResourceStr("开户行");
            colCordBank.DataType = typeof(string);
            dt.Columns.Add(colCordBank);

            //DataColumn colCordAddress = new DataColumn();
            //colCordAddress.ColumnName = Utility.GetResourceStr("开户地");
            //colCordAddress.DataType = typeof(string);
            //dt.Columns.Add(colCordAddress);

            DataColumn colCordLevel = new DataColumn();
            colCordLevel.ColumnName = Utility.GetResourceStr("职级代码");
            colCordLevel.DataType = typeof(string);
            dt.Columns.Add(colCordLevel);

            DataColumn colCordComments = new DataColumn();
            colCordComments.ColumnName = Utility.GetResourceStr("注释");
            colCordComments.DataType = typeof(string);
            dt.Columns.Add(colCordComments);

            DataColumn colCordFD = new DataColumn();
            colCordFD.ColumnName = Utility.GetResourceStr("ACTUALLYPAY");
            colCordFD.DataType = typeof(string);
            dt.Columns.Add(colCordFD);

            var recordModel = tmpData.FirstOrDefault();//取出一个薪资记录的薪资项集合做为模版
            var recordItems = from a in recordModel.EMPLOYEESALARYRECORDITEMS
                              orderby a.ORDERNUMBER
                              select a;

            //薪资项目的集合
            var salaryItems = from c in dal.GetObjects<T_HR_SALARYITEM>()
                              select c;
            if (recordItems.Count() > 0)
            {
                foreach (var recordItem in recordItems)
                {
                    var e = salaryItems.Where(s => s.SALARYITEMID == recordItem.SALARYITEMID).FirstOrDefault();
                    DataColumn colCordTemp = new DataColumn();
                    colCordTemp.ColumnName = Utility.GetResourceStr(e != null ? e.SALARYITEMNAME : string.Empty);
                    colCordTemp.DataType = typeof(string);
                    dt.Columns.Add(colCordTemp);
                }
            }
            #endregion

            #region 生成表数据
            dt.Rows.Clear();
            decimal[] totalRecord = new decimal[80];
            foreach (var tmp in tmpData)
            {
                DataRow row = dt.NewRow();
                var recordItem = tmp.EMPLOYEESALARYRECORDITEMS.FirstOrDefault();//取出一个薪资项

                var ents = tmp.EMPLOYEESALARYRECORDITEMS.OrderBy(s => s.ORDERNUMBER);//对薪资记录的薪资项排序

                var salaryArchie = (from c in dal.GetObjects<T_HR_SALARYARCHIVE>()  //查询岗位级别和薪资级别
                                    where c.SALARYARCHIVEID == recordItem.SALARYARCHIVEID
                                    select new
                                    {
                                        postLevel = c.POSTLEVEL,
                                        salaryLevel = c.SALARYLEVEL
                                    }).FirstOrDefault();

                string POSTLEVELCODE = string.Empty;

                try
                {
                    if (salaryArchie != null)
                    {
                        byte[] array = new byte[1];
                        array[0] = (byte)(65 + Convert.ToInt32(salaryArchie.postLevel));
                        POSTLEVELCODE = Encoding.ASCII.GetString(array).ToString();
                        POSTLEVELCODE += "-" + salaryArchie.salaryLevel.ToString();
                    }
                }
                catch { }

                row[0] = tmp.BANKCARDNUMBER;
                row[1] = tmp.EMPLOYEENAME;
                row[2] = tmp.BLANKID;
                row[3] = POSTLEVELCODE;
                row[4] = tmp.SALARYYEAR + "年 " + tmp.SALARYMONTH + "月 " + "工资发放";
                row[5] = AES.AESDecrypt(tmp.ACTUALLYPAY);
                if (ents.Count() > 0)
                {
                    int j = 6;
                    foreach (var ent in ents)
                    {
                        decimal tempsum = string.IsNullOrEmpty(ent.SUM) ? 0 : Convert.ToDecimal(AES.AESDecrypt(ent.SUM));
                        totalRecord[j] = totalRecord[j] + tempsum;
                        row[j] = tempsum.ToString();
                        j++;
                    }
                    dt.Rows.Add(row);
                }
            }
            #endregion
            result = Utility.OutFileStream(Utility.GetResourceStr("EMPLOYEESALARYRECORD"), dt);
            return result;

        }

        private DataTable GetAllDataConversion(DataTable dt, List<V_PAYMENT> entlist, List<V_EMPLOYEESALARYRECORD> levels)
        {
            dt.Rows.Clear();
            decimal[] totalRecord = new decimal[80];
            for (int i = 0; i < entlist.Count; i++)
            {
                DataRow row = dt.NewRow();
                string temp = entlist[i].EMPLOYEESALARYRECORDID;
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                           where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == temp    //&& a.SALARYITEMID == "bc8a3557-c931-4ec5-984f-42a01b8cf75b"
                           orderby a.ORDERNUMBER
                           select a;
                V_EMPLOYEESALARYRECORD lev = new V_EMPLOYEESALARYRECORD();
                try
                {
                    lev = levels.Where(h => h.EMPLOYEEID == entlist[i].EMPLOYEEID).FirstOrDefault();
                    byte[] array = new byte[1];
                    array[0] = (byte)(65 + Convert.ToInt32(lev.POSTLEVEL));
                    lev.POSTLEVELCODE = Encoding.ASCII.GetString(array).ToString();
                    lev.POSTLEVELCODE += "-" + lev.SALARYLEVEL.ToString();
                    if (lev.POSTLEVEL == null || lev.SALARYLEVEL == null)
                        lev.POSTLEVELCODE = string.Empty;
                }
                catch { }

                row[0] = entlist[i].BANKCARDNUMBER;
                row[1] = entlist[i].EMPLOYEENAME;
                row[2] = entlist[i].BLANKID;
                row[3] = lev != null ? lev.POSTLEVELCODE : string.Empty;
                row[4] = entlist[i].SALARYYEAR + "年 " + entlist[i].SALARYMONTH + "月 " + "工资发放";
                row[5] = entlist[i].ACTUALLYPAY;
                if (ents.Count() > 0)
                {
                    int j = 6;
                    foreach (var ent in ents)
                    {
                        decimal tempsum = string.IsNullOrEmpty(ent.SUM) ? 0 : Convert.ToDecimal(AES.AESDecrypt(ent.SUM));
                        totalRecord[j] = totalRecord[j] + tempsum;
                        row[j] = tempsum.ToString();
                        j++;
                    }
                    dt.Rows.Add(row);
                }

            }
            if (dt.Rows.Count > 0 && totalRecord.Count() > 0)
            {
                DataRow row = dt.NewRow();
                row[0] = row[1] = row[2] = row[3] = row[4] = row[5] = "---";
                row[0] = Utility.GetResourceStr("TOTAL");
                row[5] = 0;
                foreach (var el in entlist)
                {
                    row[5] = Convert.ToDecimal(row[5]) + (string.IsNullOrEmpty(el.ACTUALLYPAY) ? 0 : Convert.ToDecimal(el.ACTUALLYPAY));
                }
                for (int t = 6; t < totalRecord.Count(); t++)
                {
                    row[t] = totalRecord[t];
                    if (t >= dt.Columns.Count - 1) break;
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        private DataTable TableToAllExportInit(V_PAYMENT ent)
        {
            DataTable dt = new DataTable();

            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = Utility.GetResourceStr("BANKCARDNUMBER");
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colCordED = new DataColumn();
            colCordED.ColumnName = Utility.GetResourceStr("EMPLOYEENAME");
            colCordED.DataType = typeof(string);
            dt.Columns.Add(colCordED);

            DataColumn colCordBank = new DataColumn();
            colCordBank.ColumnName = Utility.GetResourceStr("开户行");
            colCordBank.DataType = typeof(string);
            dt.Columns.Add(colCordBank);

            //DataColumn colCordAddress = new DataColumn();
            //colCordAddress.ColumnName = Utility.GetResourceStr("开户地");
            //colCordAddress.DataType = typeof(string);
            //dt.Columns.Add(colCordAddress);

            DataColumn colCordLevel = new DataColumn();
            colCordLevel.ColumnName = Utility.GetResourceStr("职级代码");
            colCordLevel.DataType = typeof(string);
            dt.Columns.Add(colCordLevel);

            DataColumn colCordComments = new DataColumn();
            colCordComments.ColumnName = Utility.GetResourceStr("注释");
            colCordComments.DataType = typeof(string);
            dt.Columns.Add(colCordComments);

            DataColumn colCordFD = new DataColumn();
            colCordFD.ColumnName = Utility.GetResourceStr("ACTUALLYPAY");
            colCordFD.DataType = typeof(string);
            dt.Columns.Add(colCordFD);

            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                       where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == ent.EMPLOYEESALARYRECORDID
                       orderby a.ORDERNUMBER
                       select a;
            if (ents.Count() > 0)
            {
                foreach (var en in ents)
                {
                    var e = from b in dal.GetObjects<T_HR_SALARYITEM>()
                            where b.SALARYITEMID == en.SALARYITEMID
                            select b.SALARYITEMNAME;
                    DataColumn colCordTemp = new DataColumn();
                    colCordTemp.ColumnName = Utility.GetResourceStr(e.Count() > 0 ? e.FirstOrDefault() : string.Empty);
                    colCordTemp.DataType = typeof(string);
                    dt.Columns.Add(colCordTemp);
                }
            }

            return dt;
        }


        /// <summary>
        /// 导出EXCEL（员工薪资发放-银行发放 导出调用的方法）
        /// </summary>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <returns>返回</returns>
        public byte[] ExportExcel(string sort, string filterString, IList<object> paras, string year, string month, int orgtype, string orgid)
        {

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string userid = "";
            if (queryParas != null)
            {
                int i = queryParas.Count;
                userid = queryParas[i - 1].ToString();
                queryParas.RemoveAt(i - 1);
                SetOrganizationFilter(ref filterString, ref queryParas, userid, "T_HR_EMPLOYEESALARYRECORD");
            }
            else
            {
                return null;
            }
            string strMsg = string.Empty;
            string ptstr = Convert.ToInt32(PaidType.BANKSUBSTITUTING).ToString();
            byte[] result;
            DataTable dt = TableToExportInit();
            List<V_PAYMENT> entlist = new List<V_PAYMENT>();
            var tmpData = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                          join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                          where a.SALARYYEAR == year && a.SALARYMONTH == month && a.PAIDTYPE == ptstr && a.OWNERCOMPANYID == orgid
                          select new
                          {
                              ACTUALLYPAY = a.ACTUALLYPAY,
                              ATTENDANCEUNUSUALDEDUCT = a.ATTENDANCEUNUSUALDEDUCT,
                              //BANKACCOUNTNO = d.BANKACCOUNTNO,
                              BANKCARDNUMBER = e.BANKCARDNUMBER,
                              //BANKNAME = d.BANKNAME,
                              BASICSALARY = a.BASICSALARY,
                              BLANKID = e.BANKID,
                              EMPLOYEECODE = a.EMPLOYEECODE,
                              EMPLOYEEID = a.EMPLOYEEID,
                              EMPLOYEENAME = a.EMPLOYEENAME,
                              EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                              PAYTYPE = a.PAIDTYPE,
                              PERFORMANCESUM = a.PERFORMANCESUM,
                              RECORDCREATEDATE = a.CREATEDATE,
                              // ARCHIVECREATEDATE = b.CREATEDATE,
                              CHECKSTATE = a.CHECKSTATE,
                              PAYCONFIRM = a.PAYCONFIRM,
                              PAIDBY = a.PAIDBY,
                              SALARYYEAR = a.SALARYYEAR,
                              SALARYMONTH = a.SALARYMONTH,
                              PAIDDATE = a.PAIDDATE,
                              OWNERCOMPANYID = a.OWNERCOMPANYID,
                              OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                              OWNERPOSTID = a.OWNERPOSTID,
                              OWNERID = a.OWNERID,
                              CREATEUSERID = a.CREATEUSERID,
                          };

            tmpData = tmpData.Where(filterString, queryParas.ToArray());

            if (tmpData.Count() > 0)
            {
                foreach (var item in tmpData)
                {
                    //by luojie 用于在身份证后面加上用户的身份证号
                    //string IdNumber = (from emp in dal.GetObjects<T_HR_EMPLOYEE>()
                    //                   where emp.EMPLOYEEID == item.EMPLOYEEID
                    //                   select emp).FirstOrDefault().IDNUMBER.ToString();

                    V_PAYMENT pay = new V_PAYMENT();
                    pay.ACTUALLYPAY = item.ACTUALLYPAY;
                    pay.BLANKID = item.BLANKID;
                    pay.SALARYMONTH = item.SALARYMONTH;
                    pay.BANKCARDNUMBER = item.BANKCARDNUMBER;
                    pay.SALARYYEAR = item.SALARYYEAR;
                    pay.EMPLOYEENAME = item.EMPLOYEENAME;
                    pay.EMPLOYEESALARYRECORDID = item.EMPLOYEESALARYRECORDID;
                    pay.PAYCONFIRM = item.PAYCONFIRM;
                    pay.PAIDDATE = item.PAIDDATE;
                    pay.PAIDBY = item.PAIDBY;
                    pay.PAYTYPE = item.PAYTYPE;
                    entlist.Add(pay);
                }
            }
            else
            {
                return null;
            }

            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
            for (int i = 0; i < entlist.Count; i++)
            {
                string temp = entlist[i].EMPLOYEESALARYRECORDID;
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                           where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == temp && a.SALARYITEMID == "bc8a3557-c931-4ec5-984f-42a01b8cf75b"
                           select a;
                entlist[i].ACTUALLYPAY = AES.AESDecrypt(entlist[i].ACTUALLYPAY);
                if (ents.Count() > 0)
                {
                    entlist[i].BASICSALARY = Math.Round(bll.Alternative(Convert.ToDecimal(AES.AESDecrypt(ents.FirstOrDefault().SUM)) * Convert.ToDecimal(0.87), 0), 0);
                }
                else entlist[i].BASICSALARY = 0;
            }
            long totalCount = entlist.Count;

            if (totalCount > 2147483647)
            {
                strMsg = "OVERMAXEXPORTSIZE";
                return null;
            }

            if (totalCount == 0)
            {
                strMsg = "NOEXPORTDATA";
                return null;
            }

            DataTable dttoExport = GetDataConversion(dt, entlist);

            result = Utility.OutFileStream(Utility.GetResourceStr("EMPLOYEESALARYRECORD"), dttoExport);
            if (result.Count() > 0)
            {
                foreach (var en in entlist) BankPaymentUpdate(en);
            }
            return result;

        }

        private DataTable GetDataConversion(DataTable dt, List<V_PAYMENT> entlist)
        {
            dt.Rows.Clear();

            for (int i = 0; i < entlist.Count; i++)
            {
                DataRow row = dt.NewRow();
                for (int n = 0; n < dt.Columns.Count; n++)
                {
                    switch (n)
                    {
                        //case 0:
                        //    row[n] = entlist[i].EMPLOYEESALARYRECORDID;
                        //    break;
                        //case 1:
                        //    row[n] = entlist[i].BLANKID;
                        //    break;
                        //case 3:
                        //    row[n] = entlist[i].SALARYYEAR;
                        //    break;
                        //case 4:
                        //    row[n] = entlist[i].SALARYMONTH;
                        //    break;
                        case 0:
                            row[n] = entlist[i].BANKCARDNUMBER;
                            break;
                        case 1:
                            row[n] = entlist[i].EMPLOYEENAME;
                            break;
                        case 2:
                            row[n] = GetDecimalValue(entlist[i].ACTUALLYPAY);
                            break;
                        case 3:
                            row[n] = entlist[i].BLANKID;
                            break;
                        case 4:
                            row[n] = string.Empty;
                            break;
                        case 5:
                            row[n] = entlist[i].SALARYYEAR + "年 " + entlist[i].SALARYMONTH + "月 " + "工资发放";
                            break;

                        //case 6:
                        //    row[n] = entlist[i].BASICSALARY;//临时
                        //    break;
                    }
                }

                dt.Rows.Add(row);

            }

            return dt;
        }

        private decimal GetDecimalValue(string strText)
        {
            decimal dRes = 0;
            decimal.TryParse(strText, out dRes);
            return dRes;
        }

        private DataTable TableToExportInit()
        {
            DataTable dt = new DataTable();

            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = Utility.GetResourceStr("BANKCARDNUMBER");
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colCordED = new DataColumn();
            colCordED.ColumnName = Utility.GetResourceStr("EMPLOYEENAME");
            colCordED.DataType = typeof(string);
            dt.Columns.Add(colCordED);

            DataColumn colCordFD = new DataColumn();
            colCordFD.ColumnName = Utility.GetResourceStr("ACTUALLYPAY");
            colCordFD.DataType = typeof(decimal);
            dt.Columns.Add(colCordFD);

            //DataColumn colCordYD = new DataColumn();
            //colCordYD.ColumnName = Utility.GetResourceStr("SALARYYEAR");
            //colCordYD.DataType = typeof(string);
            //dt.Columns.Add(colCordYD);

            //DataColumn colCordMD = new DataColumn();
            //colCordMD.ColumnName = Utility.GetResourceStr("SALARYMONTH");
            //colCordMD.DataType = typeof(string);
            //dt.Columns.Add(colCordMD);

            DataColumn colCordBank = new DataColumn();
            colCordBank.ColumnName = Utility.GetResourceStr("开户行");
            colCordBank.DataType = typeof(string);
            dt.Columns.Add(colCordBank);

            DataColumn colCordAddress = new DataColumn();
            colCordAddress.ColumnName = Utility.GetResourceStr("开户地");
            colCordAddress.DataType = typeof(string);
            dt.Columns.Add(colCordAddress);

            DataColumn colCordComments = new DataColumn();
            colCordComments.ColumnName = Utility.GetResourceStr("注释");
            colCordComments.DataType = typeof(string);
            dt.Columns.Add(colCordComments);


            //临时
            //DataColumn colCordYfxj = new DataColumn();
            //colCordYfxj.ColumnName = Utility.GetResourceStr("应发小计*0.87");
            //colCordYfxj.DataType = typeof(decimal);
            //dt.Columns.Add(colCordYfxj);

            return dt;
        }

        /// <summary>
        /// 读取EXCEL文件
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <param name="paySign">发放成功的标志</param>
        /// <param name="columnnum">发放成功的标志的列</param>
        /// <returns></returns>
        public List<T_HR_EMPLOYEESALARYRECORD> ReadExcel(string filepath, out int failcount, out int successcount, string year, string month, string paySign) //返回实体
        {//, int columnnum
            List<T_HR_EMPLOYEESALARYRECORD> recordlist = new List<T_HR_EMPLOYEESALARYRECORD>();
            List<T_HR_EMPLOYEESALARYRECORD> recordresult = new List<T_HR_EMPLOYEESALARYRECORD>();//返回实体
            string strCon = " Provider = Microsoft.Jet.OLEDB.4.0 ; Data Source = ";
            strCon += filepath + ";Extended Properties=Excel 8.0";
            OleDbConnection myConn = new OleDbConnection(strCon);
            string strCom = " SELECT * FROM [Sheet1$] ";
            myConn.Open();
            OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
            DataSet myDataSet = new DataSet();
            myCommand.Fill(myDataSet);
            myConn.Close();

            //myDataSet.Tables[0].Rows[0][3].ToString();//发放标志
            //myDataSet.Tables[0].Rows[0][4].ToString();//失败原因

            int recordcount = myDataSet.Tables[0].Rows.Count;//记录总数
            int recordfail = 0;
            int recordOk = 0;
            foreach (DataRow dr in myDataSet.Tables[0].Rows)
            {
                string eid = dr[0].ToString();
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           join b in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals b.EMPLOYEEID
                           where a.EMPLOYEEID == b.EMPLOYEEID && b.BANKCARDNUMBER == eid && a.PAYCONFIRM != "2"
                           where a.SALARYYEAR == year && a.SALARYMONTH == month && a.CHECKSTATE == "2"
                           select a;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();
                    if (dr[3].ToString() == paySign)
                    {
                        ent.PAYCONFIRM = "2";
                        recordOk++;
                    }
                    else
                    {
                        ent.PAYCONFIRM = "0";
                        ent.REMARK = dr[4].ToString();
                        recordfail++;
                    }
                    try
                    {
                        EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                        string sql = "UPDATE T_HR_EMPLOYEESALARYRECORD t SET t.PAYCONFIRM = '" + ent.PAYCONFIRM + "'";
                        sql += " WHERE t.EMPLOYEESALARYRECORDID = '" + ent.EMPLOYEESALARYRECORDID + "'";
                        bll.ExecuteSql(sql, "T_HR_EMPLOYEESALARYRECORD");
                        //dall.Update(ent);
                        recordlist.Add(ent);
                    }
                    catch (Exception ex)
                    {
                        SMT.Foundation.Log.Tracer.Debug(ex.Message);
                    }
                }
            }
            if (recordlist.Count > 0)
            {
                List<T_HR_EMPLOYEESALARYRECORD> recordsucess = recordlist.Where(m => m.PAYCONFIRM == "2").ToList();//执行预算扣除
                SalaryBudgetDeduct(recordsucess, year, month);
                //执行真实还款
                //ActuallyRepayment(recordsucess, year, month);   

                //if (!SalaryBudgetDeduct(recordsucess, year, month))
                //{
                //    failcount = 0;
                //    successcount = 0;
                //    return null;
                //}
            }
            failcount = recordfail;//记录失败总数
            successcount = recordOk;//记录成功总数
            var en = recordlist.GroupBy(y => y.PAYCONFIRM).Select(g => new { group = g.Key, groupcontent = g });
            foreach (var v in en)
            {
                foreach (var vm in v.groupcontent) recordresult.Add(vm);
            }
            return recordresult;
        }


        //public void PayConfirm(string filepath, out int failcount, out int successcount, string year, string month, string paySign) //返回实体
        //{
        //    //myDataSet.Tables[0].Rows[0][3].ToString();//发放标志
        //    //myDataSet.Tables[0].Rows[0][4].ToString();//失败原因

        //    int recordcount = 0;//记录总数
        //    int recordfail = 0;
        //    int recordOk = 0;

        //    SMT.Foundation.Core.BaseDAL dal = new SMT.Foundation.Core.BaseDAL();
        //    try
        //    {

        //        dal.BeginTransaction();

        //        int i = 1;
        //        System.Text.UTF8Encoding code = new UTF8Encoding();
        //        using (StreamReader sr = new StreamReader(filepath, Encoding.GetEncoding("gb2312")))
        //        {
        //            string line;
        //            // Read and display lines from the file until the end of 
        //            // the file is reached.

        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                string[] lineTmp = line.Split(',');
        //                if (i >= beginRow && i <= endRow)
        //                {
        //                    //创建一个实列                
        //                    Type type = EntityInstance.GetType();
        //                    object entity = Activator.CreateInstance(type);

        //                    //得到excel二维数组当前行中A列的值             
        //                    //r = (Range)workSheet.Cells[i, "A"];
        //                    //string value = r.Text.ToString().Trim();  

        //                    #region 实例化对像
        //                    foreach (T_HR_IMPORTSETDETAIL detail in ImportConfig.T_HR_IMPORTSETDETAIL)
        //                    {
        //                        if (!string.IsNullOrEmpty(detail.EXECELCOLUMN))
        //                        {
        //                            //r = (Range)workSheet.Cells[i, detail.EXECELCOLUMN];
        //                            ////r = (Range)workSheet.Cells[i, "D"];
        //                            //string value = r.Text.ToString().Trim();
        //                            int index = 0;
        //                            //把列名拆成字符数组
        //                            char[] tmps = detail.EXECELCOLUMN.ToUpper().ToCharArray();
        //                            if (tmps.Length == 1)
        //                            {
        //                                //列名是一个字母 直接从字典查出列对应的索引
        //                                index = dictIndexs[tmps[0].ToString()];
        //                            }
        //                            else
        //                            {
        //                                //列名是双字符  计算出列对应的索引
        //                                index = (dictIndexs[tmps[0].ToString()] + 1) * 26 + (dictIndexs[tmps[1].ToString()]);
        //                            }
        //                            // int index = Convert.ToInt32(dictIndexs[detail.EXECELCOLUMN.ToUpper()]);
        //                            string value = lineTmp[index].Trim();
        //                            PropertyInfo prop = type.GetProperty(detail.ENTITYCOLUMNCODE);
        //                            if (prop != null)
        //                            {
        //                                if (!string.IsNullOrEmpty(value))
        //                                {
        //                                    if (prop.PropertyType.BaseType == typeof(System.ValueType))
        //                                    {
        //                                        if (prop.PropertyType.ToString().Contains(typeof(DateTime).ToString()))
        //                                        {
        //                                            prop.SetValue(entity, Convert.ToDateTime(value), null);
        //                                        }
        //                                        else
        //                                        {
        //                                            decimal tmpValue;
        //                                            decimal.TryParse(value, out tmpValue);
        //                                            prop.SetValue(entity, tmpValue, null);
        //                                        }

        //                                    }
        //                                    else
        //                                    {
        //                                        prop.SetValue(entity, value, null);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    #endregion
        //                    PropertyInfo prop1 = type.GetProperty("PENSIONDETAILID");
        //                    prop1.SetValue(entity, Guid.NewGuid().ToString(), null);
        //                    PropertyInfo prop2 = type.GetProperty("OWNERCOMPANYID");
        //                    prop2.SetValue(entity, paras["OWNERCOMPANYID"], null);
        //                    PropertyInfo prop3 = type.GetProperty("OWNERDEPARTMENTID");
        //                    prop3.SetValue(entity, paras["OWNERDEPARTMENTID"], null);
        //                    PropertyInfo prop4 = type.GetProperty("OWNERPOSTID");
        //                    prop4.SetValue(entity, paras["OWNERPOSTID"], null);
        //                    PropertyInfo prop5 = type.GetProperty("OWNERID");
        //                    prop5.SetValue(entity, paras["OWNERID"], null);
        //                    PropertyInfo prop6 = type.GetProperty("CREATEUSERID");
        //                    prop6.SetValue(entity, paras["CREATEUSERID"], null);
        //                    PropertyInfo prop7 = type.GetProperty("PENSIONYEAR");
        //                    prop7.SetValue(entity, Convert.ToDecimal(paras["YEAR"]), null);
        //                    PropertyInfo prop8 = type.GetProperty("PENSIONMOTH");
        //                    prop8.SetValue(entity, Convert.ToDecimal(paras["MONTH"]), null);
        //                    PropertyInfo prop9 = type.GetProperty("CREATEDATE");
        //                    prop9.SetValue(entity, System.DateTime.Now, null);

        //                    T_HR_PENSIONDETAIL tmpDetail = entity as T_HR_PENSIONDETAIL;
        //                    string idnumber = string.Empty;
        //                    if (string.IsNullOrEmpty(tmpDetail.IDNUMBER))
        //                    {
        //                        idnumber = "Null0";
        //                    }
        //                    else
        //                    {
        //                        idnumber = tmpDetail.IDNUMBER.ToString();
        //                    }
        //                    //根据身份证号 查询员工  如果没有找到就不插入此条记录
        //                    var employee = from e in dal.GetObjects<T_HR_EMPLOYEE>()
        //                                   where e.IDNUMBER == idnumber
        //                                   select e;
        //                    if (employee.Count() > 0)
        //                    {
        //                        //删除旧的记录
        //                        var oldDetail = from c in dal.GetObjects<T_HR_PENSIONDETAIL>()
        //                                        where c.PENSIONMOTH == tmpDetail.PENSIONMOTH && c.PENSIONYEAR == tmpDetail.PENSIONYEAR && c.IDNUMBER == idnumber
        //                                        select c;
        //                        if (oldDetail.Count() > 0)
        //                        {
        //                            dal.DeleteFromContext(oldDetail.FirstOrDefault());
        //                        }

        //                        //插入数据到数据库
        //                        dal.AddToContext(entity);
        //                        // dal.Add(entity);
        //                    }
        //                    else
        //                    {

        //                        Tracer.Debug("PensionImport:" + i.ToString() + "行，没有员工身份证为此号码:" + idnumber);
        //                    }
        //                }
        //                i++;
        //            }
        //        }

        //        dal.SaveContextChanges();
        //        dal.CommitTransaction();
        //        CommDal<T_HR_PENSIONDETAIL> cdal = new CommDal<T_HR_PENSIONDETAIL>();
        //        //                string strSql = @" update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b where a.computerno
        //        //                                  = b.computerno),a.employeeid= (select employeeid from t_hr_pensionmaster b where a.computerno = b.computerno)
        //        //                                  ,a.CARDID= (select CARDID from t_hr_pensionmaster b where a.computerno = b.computerno)";
        //        //string strSql = @" update t_hr_pensiondetail a    set  a.employeeid= (select employeeid from t_hr_employee b where a.idnumber = b.IDNUMBER)";
        //        //string strSql = " update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b where a.computerno"
        //        //                +"= b.computerno and b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"'),a.employeeid= (select employeeid from t_hr_pensionmaster b where a.computerno = b.computerno   and b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
        //        //                +"  ,a.CARDID= (select CARDID from t_hr_pensionmaster b where a.computerno = b.computerno  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
        //        //                +"  ,a.OWNERID=(select employeeid from t_hr_pensionmaster b where a.computerno = b.computerno  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
        //        //                +"  ,a.OWNERPOSTID=(select c.OWNERPOSTID from t_hr_pensionmaster b ,t_hr_employee c where a.computerno = b.computerno and b.employeeid= c.employeeid +  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"') "
        //        //                +"  ,a.OWNERDEPARTMENTID=(select c.OWNERDEPARTMENTID from t_hr_pensionmaster b ,t_hr_employee c where a.computerno = b.computerno and b.employeeid= c.employeeid  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
        //        //                 +" ,a.OWNERCOMPANYID=(select c.OWNERCOMPANYID from t_hr_pensionmaster b ,t_hr_employee c where a.computerno = b.computerno and b.employeeid= c.employeeid  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')" 
        //        //                 +" where a.PENSIONYEAR='" + paras["YEAR"] + "' and a.PENSIONMOTH='" + paras["MONTH"] + "'";
        //        //cdal.ExecuteCustomerSql(strSql);
        //        //strSql = "update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b where a.employeeid = b.employeeid),a.cardid =(select b.cardid from t_hr_pensionmaster b where a.employeeid = b.employeeid)";
        //        string strSql = " update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b,t_hr_employee c where a.IDNUMBER= c.IDNUMBER and b.employeeid=c.employeeid and b.editstate=1)"
        //                       + "  ,a.employeeid= (select employeeid from t_hr_employee c where a.IDNUMBER = c.IDNUMBER  )"
        //                       + "  ,a.CARDID= (select CARDID from t_hr_pensionmaster b ,t_hr_employee c where a.IDNUMBER = c.IDNUMBER and b.employeeid=c.employeeid and b.editstate=1)"
        //                       + "  ,a.OWNERID=(select employeeid from t_hr_employee c where a.IDNUMBER = c.IDNUMBER )"
        //                       + "  ,a.OWNERPOSTID=(select c.OWNERPOSTID from t_hr_employee c where a.IDNUMBER= c.IDNUMBER ) "
        //                       + "  ,a.OWNERDEPARTMENTID=(select c.OWNERDEPARTMENTID from t_hr_employee c where a.IDNUMBER= c.IDNUMBER )"
        //                       + " ,a.OWNERCOMPANYID=(select c.OWNERCOMPANYID from t_hr_employee c where a.IDNUMBER = c.IDNUMBER  )"
        //                       + " where a.PENSIONYEAR='" + paras["YEAR"] + "' and a.PENSIONMOTH='" + paras["MONTH"] + "'";
        //        cdal.ExecuteCustomerSql(strSql);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        //TODO: 写成导入日志
        //        //throw ex;
        //        Tracer.Debug("My error" + ex.Message);
        //        dal.RollbackTransaction();
        //        return false;
        //    }
        //    finally
        //    {


        //    }

        //    if (recordlist.Count > 0)
        //    {
        //        List<T_HR_EMPLOYEESALARYRECORD> recordsucess = recordlist.Where(m => m.PAYCONFIRM == "2").ToList();//执行预算扣除
        //        SalaryBudgetDeduct(recordsucess, year, month);
        //        //执行真实还款
        //        //ActuallyRepayment(recordsucess, year, month);   

        //        //if (!SalaryBudgetDeduct(recordsucess, year, month))
        //        //{
        //        //    failcount = 0;
        //        //    successcount = 0;
        //        //    return null;
        //        //}
        //    }
        //    failcount = recordfail;//记录失败总数
        //    successcount = recordOk;//记录成功总数
        //    var en = recordlist.GroupBy(y => y.PAYCONFIRM).Select(g => new { group = g.Key, groupcontent = g });
        //    foreach (var v in en)
        //    {
        //        foreach (var vm in v.groupcontent) recordresult.Add(vm);
        //    }
        //    return recordresult;
        //}

        /// <summary>
        /// 执行真实还款
        /// </summary>
        /// <param name="list">发放记录列表</param>
        /// <returns></returns>
        public void ActuallyRepayment(List<T_HR_EMPLOYEESALARYRECORD> list, string year, string month)
        {
            int i = 0;
            SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[] listDebt = new SMT.SaaS.BLLCommonServices.FBServiceWS.DebtInfo[list.Count];
            foreach (T_HR_EMPLOYEESALARYRECORD li in list)
            {
                EmployeeAddSumBLL addbll = new EmployeeAddSumBLL();
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                           where a.EMPLOYEEID == li.EMPLOYEEID && a.PROJECTCODE == "-3" && a.DEALYEAR == year && a.DEALMONTH == month
                           select a;
                if (ents.Count() > 0)
                {
                    listDebt[i].OrderID = ents.FirstOrDefault().ADDSUMID;
                }
                i++;
            }
            //FB操作真实还款
            if (i > 0)
                FBSclient.RepayBySalary(listDebt, SMT.SaaS.BLLCommonServices.FBServiceWS.FBServiceRepayType.Pass);
        }

        /// <summary>
        /// 执行预算扣除
        /// </summary>
        /// <param name="list">发放记录列表</param>
        /// <returns></returns>
        public bool SalaryBudgetDeduct(List<T_HR_EMPLOYEESALARYRECORD> list, string year, string month)
        {
            string filter = string.Empty;
            List<object> queryParas = new List<object>();
            string xml = @"<?xml version='1.0' encoding='utf-8' ?>";
            var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                       join f in dal.GetObjects<T_HR_EMPLOYEEPOST>() on e.EMPLOYEEID equals f.T_HR_EMPLOYEE.EMPLOYEEID
                       join g in dal.GetObjects<T_HR_POST>() on f.T_HR_POST.POSTID equals g.POSTID
                       join h in dal.GetObjects<T_HR_DEPARTMENT>() on g.T_HR_DEPARTMENT.DEPARTMENTID equals h.DEPARTMENTID
                       where a.SALARYYEAR == year && a.SALARYMONTH == month
                       select new
                       {
                           COMPANYID = h.T_HR_COMPANY.COMPANYID,
                           DEPARTMENTID = h.DEPARTMENTID,
                           EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                           ACTUALLYPAY = a.ACTUALLYPAY
                       };

            if (ents.Count() > 0)
            {
                foreach (var li in list)
                {
                    if (!string.IsNullOrEmpty(filter)) filter += "OR";
                    filter += " EMPLOYEESALARYRECORDID == @" + queryParas.Count();
                    queryParas.Add(li.EMPLOYEESALARYRECORDID);
                }
                ents = ents.Where(filter, queryParas.ToArray());
                //ents = ents.Where(k => esid.Contains(k.EMPLOYEESALARYRECORDID));
                //ents = from t in ents where esid.Any(p => p == t.EMPLOYEESALARYRECORDID) select t;
                var ent = ents.GroupBy(x => x.COMPANYID).Select(gs => new { groups = gs.Key, groupcontents = gs });
                xml += "<SalaryBudget Year='" + year + "' Month='" + month + "'>";
                foreach (var t in ent)
                {
                    var en = t.groupcontents.GroupBy(y => y.DEPARTMENTID).Select(g => new { group = g.Key, groupcontent = g });
                    xml += "<Company CompanyID='" + ents.FirstOrDefault().COMPANYID + "'>";
                    foreach (var v in en)
                    {
                        decimal counts = 0;
                        foreach (var vm in v.groupcontent)
                        {
                            counts += Convert.ToDecimal(vm.ACTUALLYPAY == null ? "0" : AES.AESDecrypt(vm.ACTUALLYPAY));
                        }
                        xml += "<Department DepartmentID='" + v.groupcontent.FirstOrDefault().DEPARTMENTID + "' Salary='" + counts + "' />";
                    }
                }
                xml += "</Company></SalaryBudget>";
                return FBSclient.UpdateSalaryBudget(xml);
            }
            return false;
        }

        public byte[] ExportSalaryExcel(string sort, string filterString, IList<object> paras, string userID)
        {

            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            string monthBatchID = string.Empty;
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");

            string strMsg = string.Empty;
            string ptstr = Convert.ToInt32(PaidType.BANKSUBSTITUTING).ToString();
            byte[] result;
            DataTable dt = TableToExportInit();
            List<V_PAYMENT> entlist = new List<V_PAYMENT>();
            var tmpData = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                          join e in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals e.EMPLOYEEID
                          where a.PAYCONFIRM == "0" && a.CHECKSTATE == "2"
                          select new
                          {
                              ACTUALLYPAY = a.ACTUALLYPAY,
                              ATTENDANCEUNUSUALDEDUCT = a.ATTENDANCEUNUSUALDEDUCT,
                              //BANKACCOUNTNO = d.BANKACCOUNTNO,
                              BANKCARDNUMBER = e.BANKCARDNUMBER,
                              //BANKNAME = d.BANKNAME,
                              BASICSALARY = a.BASICSALARY,
                              BLANKID = e.BANKID,
                              EMPLOYEECODE = a.EMPLOYEECODE,
                              EMPLOYEEID = a.EMPLOYEEID,
                              EMPLOYEENAME = a.EMPLOYEENAME,
                              EMPLOYEESALARYRECORDID = a.EMPLOYEESALARYRECORDID,
                              PAYTYPE = a.PAIDTYPE,
                              PERFORMANCESUM = a.PERFORMANCESUM,
                              RECORDCREATEDATE = a.CREATEDATE,
                              // ARCHIVECREATEDATE = b.CREATEDATE,
                              CHECKSTATE = a.CHECKSTATE,
                              PAYCONFIRM = a.PAYCONFIRM,
                              PAIDBY = a.PAIDBY,
                              SALARYYEAR = a.SALARYYEAR,
                              SALARYMONTH = a.SALARYMONTH,
                              PAIDDATE = a.PAIDDATE,
                              OWNERCOMPANYID = a.OWNERCOMPANYID,
                              OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                              OWNERPOSTID = a.OWNERPOSTID,
                              OWNERID = a.OWNERID,
                              CREATEUSERID = a.CREATEUSERID,
                              MONTHLYBATCHID = a.T_HR_SALARYRECORDBATCH.MONTHLYBATCHID
                          };

            tmpData = tmpData.Where(filterString, queryParas.ToArray());

            if (tmpData.Count() > 0)
            {

                foreach (var item in tmpData)
                {
                    V_PAYMENT pay = new V_PAYMENT();
                    pay.ACTUALLYPAY = item.ACTUALLYPAY;
                    pay.BLANKID = item.BLANKID;
                    pay.SALARYMONTH = item.SALARYMONTH;
                    pay.BANKCARDNUMBER = item.BANKCARDNUMBER;
                    pay.SALARYYEAR = item.SALARYYEAR;
                    pay.EMPLOYEENAME = item.EMPLOYEENAME;
                    pay.EMPLOYEESALARYRECORDID = item.EMPLOYEESALARYRECORDID;
                    pay.PAYCONFIRM = item.PAYCONFIRM;
                    pay.PAIDDATE = item.PAIDDATE;
                    pay.PAIDBY = item.PAIDBY;
                    pay.PAYTYPE = item.PAYTYPE;
                    monthBatchID = item.MONTHLYBATCHID;
                    entlist.Add(pay);
                }
            }
            else
            {
                return null;
            }

            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
            for (int i = 0; i < entlist.Count; i++)
            {
                string temp = entlist[i].EMPLOYEESALARYRECORDID;
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                           where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == temp && a.SALARYITEMID == "bc8a3557-c931-4ec5-984f-42a01b8cf75b"
                           select a;
                entlist[i].ACTUALLYPAY = AES.AESDecrypt(entlist[i].ACTUALLYPAY);
                if (ents.Count() > 0)
                {
                    entlist[i].BASICSALARY = Math.Round(bll.Alternative(Convert.ToDecimal(AES.AESDecrypt(ents.FirstOrDefault().SUM)) * Convert.ToDecimal(0.87), 0), 0);
                }
                else entlist[i].BASICSALARY = 0;
            }
            long totalCount = entlist.Count;

            if (totalCount > 2147483647)
            {
                strMsg = "OVERMAXEXPORTSIZE";
                return null;
            }

            if (totalCount == 0)
            {
                strMsg = "NOEXPORTDATA";
                return null;
            }

            DataTable dttoExport = GetDataConversion(dt, entlist);

            result = Utility.OutFileStream(Utility.GetResourceStr("EMPLOYEESALARYRECORD"), dttoExport);
            if (result.Count() > 0)
            {
                foreach (var en in entlist)
                {
                    BankPaymentUpdate(en);
                }

                var batch = (from c in dal.GetObjects<T_HR_SALARYRECORDBATCH>()
                             where c.MONTHLYBATCHID == monthBatchID
                             select c).FirstOrDefault();
                if (batch != null)
                {
                    try
                    {
                        batch.EDITSTATE = "1";//已经导出过 不能再导出
                        dal.UpdateFromContext(batch);
                        dal.SaveContextChanges();
                    }
                    catch (Exception ex)
                    {
                        SMT.Foundation.Log.Tracer.Debug(ex.Message);
                    }
                }
            }
            return result;

        }

        public void PayRemindByOrgID(string strOrgType, string strOrgID, DateTime dtCurDate, ref string strMsg)
        {
            try
            {
                string strCheckState = Convert.ToInt32(Common.CheckStates.Approved).ToString();
                string strPayconfirm = Convert.ToInt32(Common.PaymentState.ALREADYPAYMENT).ToString();
                string strPayYear = dtCurDate.Year.ToString();
                string strPayMonth = dtCurDate.Month.ToString();


                IQueryable<T_HR_EMPLOYEESALARYRECORD> ents = from s in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                                             where s.CHECKSTATE == strCheckState && s.PAYCONFIRM == strPayconfirm
                                                             && s.SALARYYEAR == strPayYear && s.SALARYMONTH == strPayMonth
                                                             select s;

                switch (strOrgType)
                {
                    case "1":
                        ents = from s in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on s.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                               join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                               join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                               join c in dal.GetObjects<T_HR_COMPANY>() on d.T_HR_COMPANY.COMPANYID equals c.COMPANYID
                               where c.COMPANYID == strOrgID && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                               && s.CHECKSTATE == strCheckState && s.PAYCONFIRM == strPayconfirm && s.SALARYYEAR == strPayYear
                               && s.SALARYMONTH == strPayMonth
                               select s;
                        break;
                    case "2":
                        ents = from s in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on s.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                               join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                               join d in dal.GetObjects<T_HR_DEPARTMENT>() on p.T_HR_DEPARTMENT.DEPARTMENTID equals d.DEPARTMENTID
                               where d.DEPARTMENTID == strOrgID && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                               && s.CHECKSTATE == strCheckState && s.PAYCONFIRM == strPayconfirm && s.SALARYYEAR == strPayYear
                               && s.SALARYMONTH == strPayMonth
                               select s;
                        break;
                    case "3":
                        ents = from s in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                               join ep in dal.GetObjects<T_HR_EMPLOYEEPOST>() on s.EMPLOYEEID equals ep.T_HR_EMPLOYEE.EMPLOYEEID
                               join p in dal.GetObjects<T_HR_POST>() on ep.T_HR_POST.POSTID equals p.POSTID
                               where p.POSTID == strOrgID && (ep.EDITSTATE == "1" || ep.EDITSTATE == "0" && ep.CHECKSTATE == "0")
                               && s.CHECKSTATE == strCheckState && s.PAYCONFIRM == strPayconfirm && s.SALARYYEAR == strPayYear
                               && s.SALARYMONTH == strPayMonth
                               select s;
                        break;
                    case "4":
                        ents = from s in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                               where s.CHECKSTATE == strCheckState && s.PAYCONFIRM == strPayconfirm
                                && s.SALARYYEAR == strPayYear && s.SALARYMONTH == strPayMonth && strOrgID.Contains(s.EMPLOYEEID)
                               select s;
                        break;
                }

                if (ents == null)
                {
                    strMsg = "执行PayRemindByOrgID失败，原因：当前参数：strOrgType=" + strOrgType + ", strOrgID=" + strOrgID;
                    return;
                }

                List<T_HR_EMPLOYEESALARYRECORD> entPays = ents.ToList();

                if (entPays == null)
                {
                    strMsg = "执行PayRemindByOrgID失败，原因：根据当前参数：strOrgType=" + strOrgType + ", strOrgID=" + strOrgID
                        + "获取不到已完成发放确认的薪资数据";
                    return;
                }

                if (entPays.Count() == 0)
                {
                    strMsg = "执行PayRemindByOrgID失败，原因：当前参数：strOrgType=" + strOrgType + ", strOrgID=" + strOrgID
                        + "获取不到已完成发放确认的薪资数据";
                    return;
                }

                PayEngineMsg(entPays);

                strMsg = "薪资发放提醒执行成功。";
            }
            catch (Exception ex)
            {
                strMsg = "执行PayRemindByOrgID失败，原因：当前参数：strOrgType=" + strOrgType + ", strOrgID=" + strOrgID
                                       + "获取不到已完成发放确认的薪资数据,执行此函数时发生异常，异常消息为：" + ex.ToString();
            }
        }
    }
}
