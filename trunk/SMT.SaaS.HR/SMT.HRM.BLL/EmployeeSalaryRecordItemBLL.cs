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
using System.Security.Cryptography;
namespace SMT.HRM.BLL
{
    public class EmployeeSalaryRecordItemBLL : BaseBll<T_HR_EMPLOYEESALARYRECORDITEM>
    {

        /// <summary>
        /// 新增薪资记录薪资项
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public void AddEmployeeSalaryRecordItem(T_HR_EMPLOYEESALARYRECORDITEM entity)
        {
            try
            {
                if (entity == null)
                {
                    return;
                }
                var ents = from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                           where a.EMPLOYEESALARYRECORDID == entity.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID
                           select a;
                if (ents.Count() <= 0)
                {
                    return;
                }

                T_HR_EMPLOYEESALARYRECORDITEM ent = new T_HR_EMPLOYEESALARYRECORDITEM();
                Utility.CloneEntity<T_HR_EMPLOYEESALARYRECORDITEM>(entity, ent);
                ent.T_HR_EMPLOYEESALARYRECORDReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEESALARYRECORD", "EMPLOYEESALARYRECORDID", entity.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID);

                Utility.RefreshEntity(ent);

                dal.Add(ent);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                ex.Message.ToString();
            }

            return;
        }

        /// <summary>
        /// 薪资记录薪资项
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T_HR_EMPLOYEESALARYRECORDITEM GetEmployeeSalaryRecordItem(T_HR_EMPLOYEESALARYRECORDITEM entity)
        {
            try
            {
                if (entity == null)
                {
                    return null;
                }
                //var ents = from a in DataContext.T_HR_EMPLOYEESALARYRECORD
                //           where a.EMPLOYEESALARYRECORDID == entity.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID
                //           select a;
                //if (ents.Count() <= 0)
                //{
                //    return null;
                //}

                T_HR_EMPLOYEESALARYRECORDITEM ent = new T_HR_EMPLOYEESALARYRECORDITEM();
                Utility.CloneEntity<T_HR_EMPLOYEESALARYRECORDITEM>(entity, ent);
                ent.T_HR_EMPLOYEESALARYRECORDReference.EntityKey =
                    new System.Data.EntityKey(qualifiedEntitySetName + "T_HR_EMPLOYEESALARYRECORD", "EMPLOYEESALARYRECORDID", entity.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID);

                Utility.RefreshEntity(ent);

                return ent;

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                ex.Message.ToString();
            }

            return null;
        }

        /// <summary>
        /// 根据薪资记录ID查询实体
        /// </summary>
        /// <param name="SalaryRecordID">薪资记录ID</param>
        /// <returns>返回薪资记录薪资项实体结果集</returns>
        public List<V_EMPLOYEESALARYRECORDITEM> GetEmployeeSalaryRecordItemByID(string SalaryRecordID)
        {
            var ents = from a in dal.GetTable()
                       join b in dal.GetObjects<T_HR_SALARYITEM>() on a.SALARYITEMID equals b.SALARYITEMID
                       where a.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == SalaryRecordID
                       orderby a.ORDERNUMBER
                       select new V_EMPLOYEESALARYRECORDITEM
                       {
                           SALARYITEMNAME = b.SALARYITEMNAME,
                           CALCULATEFORMULA = a.CALCULATEFORMULA,
                           SALARYRECORDITEMID = a.SALARYRECORDITEMID,
                           SALARYITEMID = a.SALARYITEMID,
                           CREATEDATE = a.CREATEDATE,
                           ORDERNUMBER = a.ORDERNUMBER,
                           REMARK = a.REMARK,
                           SUM = a.SUM
                       };
            var entAddsums = from c in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                             join b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>() on c.EMPLOYEEID equals b.EMPLOYEEID
                             where c.DEALYEAR == b.SALARYYEAR && c.DEALMONTH == b.SALARYMONTH && b.EMPLOYEESALARYRECORDID == SalaryRecordID && c.CHECKSTATE == "2"
                             select c;
            string remark = string.Empty;
            V_EMPLOYEESALARYRECORDITEM addsumItem = new V_EMPLOYEESALARYRECORDITEM();
            addsumItem.SALARYITEMNAME = "备注";

            addsumItem.SALARYRECORDITEMID = Guid.NewGuid().ToString();

            addsumItem.ORDERNUMBER = 100;
            if (entAddsums.Count() > 0)
            {
                foreach (var addsum in entAddsums)
                {
                    remark += addsum.REMARK;
                }

                addsumItem.SUM = AES.AESEncrypt(remark);
            }

            List<V_EMPLOYEESALARYRECORDITEM> listItems;
            if (ents.Count() > 0)
            {
                listItems = ents.ToList();
                listItems.Add(addsumItem);
            }
            else
            {
                listItems = null;
            }
            return listItems;
        }

        /// <summary>
        /// 根据薪资记录薪资项ID查询实体
        /// </summary>
        /// <param name="SalaryItemSetID">薪资记录薪资项ID</param>
        /// <returns>返回薪资记录薪资项实体</returns>
        public T_HR_EMPLOYEESALARYRECORDITEM GetSalaryItemByID(string SalaryItemSetID)
        {
            var ents = from a in dal.GetTable()
                       where a.SALARYITEMID == SalaryItemSetID
                       select a;
            return ents.Count() > 0 ? ents.FirstOrDefault() : null;
        }

        /// <summary>
        /// 删除薪资记录薪资项记录
        /// </summary>
        /// <param name="EmployeeSalaryRecordID">薪资记录ID</param>
        /// <returns></returns>
        public int EmployeeSalaryRecordItemDelete(string EmployeeSalaryRecordID)
        {
            var ents = from e in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                       where e.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID == EmployeeSalaryRecordID
                       select e;
            if (ents.Count() > 0)
            {
                foreach (var ent in ents)
                {
                    if (ent != null) dal.DeleteFromContext(ent);
                }
                return dal.SaveContextChanges();
            }
            return 0;
        }

        /// <summary>
        /// 删除薪资记录薪资项记录，可同时删除多行记录
        /// </summary>
        /// <param name="EmployeeSalaryRecordItemIDs">薪资记录薪资项ID数组</param>
        /// <returns></returns>
        public int EmployeeSalaryRecordItemDeletes(string[] EmployeeSalaryRecordItemIDs)
        {
            foreach (string id in EmployeeSalaryRecordItemIDs)
            {
                var ents = from e in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                           where e.SALARYRECORDITEMID == id
                           select e;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }

        public List<V_MonthDeductionTax> GetMonthDeductionTaxs(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, bool IsPageing)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");
            decimal banlanceYear = Convert.ToDecimal(queryParas[0]);
            decimal banlanceMonth = Convert.ToDecimal(queryParas[1]);
            var ents = from c in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                       join f in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>() on c.EMPLOYEEID equals f.EMPLOYEEID
                       join e in dal.GetObjects<T_HR_EMPLOYEE>() on c.EMPLOYEEID equals e.EMPLOYEEID
                       // join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                       join b in dal.GetObjects<T_HR_POST>() on f.OWNERPOSTID equals b.POSTID
                       join d in dal.GetObjects<T_HR_COMPANY>() on c.OWNERCOMPANYID equals d.COMPANYID
                       where f.BALANCEYEAR == banlanceYear && f.BALANCEMONTH == banlanceMonth
                       select new SalryRecordView
                       {
                           orgName = d.CNAME,
                           CNAME = b.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                           DepartmentName = b.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           PostName = b.T_HR_POSTDICTIONARY.POSTNAME,
                           EmployeeName = e.EMPLOYEECNAME,
                           IDNumber = e.IDNUMBER,
                           OWNERID = c.OWNERID,
                           OWNERPOSTID = c.OWNERPOSTID,
                           OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = c.OWNERCOMPANYID,
                           SalaryRecordID = c.EMPLOYEESALARYRECORDID,
                           salaryMonth = c.SALARYMONTH,
                           salaryYear = c.SALARYYEAR,
                           CREATEUSERID = c.CREATEUSERID,
                           SalaryItems = c.T_HR_EMPLOYEESALARYRECORDITEM
                       };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            if (IsPageing)
            {
                ents = Utility.Pager<SalryRecordView>(ents, pageIndex, pageSize, ref pageCount);
            }
            #region
            //StringBuilder recordIDs = new StringBuilder();
            ////   int flag = 0;
            //foreach (var ent in ents)
            //{
            //    recordIDs.Append(ent.SalaryRecordID + ",");
            //    //      flag++;
            //    //      if (flag >= 20)
            //    //          break;
            //}
            //string ids = recordIDs.ToString();
            //var items = from n in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
            //            join m in dal.GetObjects<T_HR_SALARYITEM>() on n.SALARYITEMID equals m.SALARYITEMID
            //            select new
            //            {
            //                itemName = m.SALARYITEMNAME,
            //                itemContont = n

            //            };
            //filterString = " itemContont.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID.Contains(@0)";
            //List<object> p = new List<object>();
            //p.Add(ids);
            //items = items.Where(filterString, p.ToArray());
            #endregion
            List<V_MonthDeductionTax> VmonthDeductiontaxs = new List<V_MonthDeductionTax>();
            foreach (var ent in ents)
            {
                V_MonthDeductionTax tmp = new V_MonthDeductionTax();
                tmp.PayCompany = ent.orgName;
                tmp.Organization = ent.CNAME + " - " + ent.DepartmentName + " - " + ent.PostName;
                tmp.IdNumber = ent.IDNumber + "\t";
                tmp.EmployeeName = ent.EmployeeName;
                var salaryItems = ent.SalaryItems.OrderBy(t => t.ORDERNUMBER);//根据排序号排序
                foreach (var item in salaryItems)
                {
                    int order = Convert.ToInt32(item.ORDERNUMBER.Value);
                    switch (order)//对于排序为对于薪资项
                    {
                        case 21: tmp.Sum = item == null ? string.Empty : item.SUM; break;//计税工资
                        case 22: tmp.TaxesBasic = item == null ? string.Empty : item.SUM; break;//扣税基数
                        case 23: tmp.Balance = item == null ? string.Empty : item.SUM; break;//差额
                        case 24: tmp.TaxesRate = item == null ? string.Empty : item.SUM; break;//税率
                        case 25: tmp.CalculateDeduct = item == null ? string.Empty : item.SUM; break; //速算扣除数
                        case 26: tmp.Personalincometax = item == null ? string.Empty : item.SUM; break;//个人所得税
                        default: break;
                    }
                }
                #region 旧方法
                //// var CalculateDeduct = ent.SalaryItems.Where(s => s.SALARYITEMID == "68d52d34-805e-4873-bb21-544746ec5d1e").FirstOrDefault();//速算扣除数
                //var CalculateDeduct = (from si in ent.SalaryItems
                //                       join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                //                       where s.SALARYITEMNAME == "速算扣除数"
                //                       select si).FirstOrDefault();
                //tmp.CalculateDeduct = CalculateDeduct == null ? string.Empty : CalculateDeduct.SUM;

                ////var banlance = ent.SalaryItems.Where(s => s.SALARYITEMID == "d62310ea-f5ec-4393-862c-da3500722689").FirstOrDefault();//差额
                //var banlance = (from si in ent.SalaryItems
                //                       join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                //                where s.SALARYITEMNAME == "差额"
                //                       select si).FirstOrDefault();
                //tmp.Balance = banlance == null ? string.Empty : banlance.SUM;

                ////var Personalincometax = ent.SalaryItems.Where(s => s.SALARYITEMID == "a3162966-cb70-497a-a972-180337d8fffd").FirstOrDefault();//个人所得税
                //var Personalincometax = (from si in ent.SalaryItems
                //                join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                //                         where s.SALARYITEMNAME == "个人所得税"
                //                select si).FirstOrDefault();
                //tmp.Personalincometax = Personalincometax == null ? string.Empty : Personalincometax.SUM;

                ////var Sum = ent.SalaryItems.Where(s => s.SALARYITEMID == "9a42a345-7d5f-439f-9820-9cdbbcbec871").FirstOrDefault();//计税工资
                //var Sum = (from si in ent.SalaryItems
                //                         join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                //           where s.SALARYITEMNAME == "计税工资"
                //                         select si).FirstOrDefault();
                //tmp.Sum = Sum == null ? string.Empty : Sum.SUM;

                ////var TaxesBasic = ent.SalaryItems.Where(s => s.SALARYITEMID == "039e58e4-7877-49f0-8a1b-3250634f785c").FirstOrDefault();//扣税基数
                //var TaxesBasic = (from si in ent.SalaryItems
                //           join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                //                  where s.SALARYITEMNAME == "扣税基数"
                //           select si).FirstOrDefault();
                //tmp.TaxesBasic = TaxesBasic == null ? string.Empty : TaxesBasic.SUM;

                ////var TaxesRate = ent.SalaryItems.Where(s => s.SALARYITEMID == "5c142665-ecda-489c-8e7d-95a39c2144cf").FirstOrDefault();//税率
                //var TaxesRate = (from si in ent.SalaryItems
                //                  join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                //                 where s.SALARYITEMNAME == "税率"
                //                  select si).FirstOrDefault();
                //tmp.TaxesRate = TaxesRate == null ? string.Empty : TaxesRate.SUM;
                #endregion
                VmonthDeductiontaxs.Add(tmp);
            }
            return VmonthDeductiontaxs;
        }
        public List<V_EmployeeDeductionMoney> GetEmployeeDeductionMoney(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");
            decimal banlanceYear = Convert.ToDecimal(year);
            decimal banlanceMonth = Convert.ToDecimal(month);
            var ents = from c in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                      // join f in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>() on c.EMPLOYEEID equals f.EMPLOYEEID
                       join e in dal.GetObjects<T_HR_EMPLOYEE>() on c.EMPLOYEEID equals e.EMPLOYEEID
                       // join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                       join b in dal.GetObjects<T_HR_POST>() on e.OWNERPOSTID equals b.POSTID
                       join d in dal.GetObjects<T_HR_COMPANY>() on c.OWNERCOMPANYID equals d.COMPANYID
                       //where f.BALANCEYEAR == banlanceYear && f.BALANCEMONTH == banlanceMonth
                       where c.SALARYYEAR == year && c.SALARYMONTH == month
                       select new SalryRecordView
                       {
                           orgName = d.CNAME,
                           CNAME = b.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                           DepartmentName = b.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           PostName = b.T_HR_POSTDICTIONARY.POSTNAME,
                           EmployeeName = e.EMPLOYEECNAME,
                           IDNumber = e.IDNUMBER,
                           OWNERID = c.OWNERID,
                           OWNERPOSTID = c.OWNERPOSTID,
                           OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = c.OWNERCOMPANYID,
                           SalaryRecordID = c.EMPLOYEESALARYRECORDID,
                           salaryMonth = c.SALARYMONTH,
                           salaryYear = c.SALARYYEAR,
                           CREATEUSERID = c.CREATEUSERID,
                           EmployeeID = c.EMPLOYEEID,
                           SalaryItems = c.T_HR_EMPLOYEESALARYRECORDITEM
                       };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            if (IsPageing)
            {
                ents = Utility.Pager<SalryRecordView>(ents, pageIndex, pageSize, ref pageCount);
            }
            #region
            //StringBuilder recordIDs = new StringBuilder();
            //StringBuilder employeeIDsBuild = new StringBuilder();
            //int flag = 0;
            //foreach (var ent in ents)
            //{
            //    recordIDs.Append(ent.SalaryRecordID + ",");
            //    employeeIDsBuild.Append(ent.EmployeeID + ",");
            //    flag++;
            //    if (flag >= 10)
            //        break;
            //}
            //string ids = recordIDs.ToString();
            //string employeeIDs = employeeIDsBuild.ToString();
            //var items = from n in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
            //            join m in dal.GetObjects<T_HR_SALARYITEM>() on n.SALARYITEMID equals m.SALARYITEMID
            //            select new
            //            {
            //                itemName = m.SALARYITEMNAME,
            //                itemContont = n

            //            };
            //filterString = " itemContont.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID.Contains(@0)";
            //List<object> p = new List<object>();
            //p.Add(ids);
            //items = items.Where(filterString, p.ToArray());

            //filterString = "  EMPLOLYEEID.Contains(@0)";
            //p.Clear();
            //p.Add(employeeIDs);
            #endregion
            var addsums = from f in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                          where f.CHECKSTATE == "2" && f.DEALMONTH == month && f.DEALYEAR == year
                          select f;
            List<V_EmployeeDeductionMoney> V_EmployeeDeductionMoneys = new List<V_EmployeeDeductionMoney>();
            foreach (var ent in ents)
            {
                V_EmployeeDeductionMoney tmp = new V_EmployeeDeductionMoney();
                tmp.PayCompany = ent.orgName;
                tmp.Organization = ent.CNAME + " - " + ent.DepartmentName + " - " + ent.PostName;
                tmp.IdNumber = ent.IDNumber + "\t";
                tmp.EmployeeName = ent.EmployeeName;

                //var OtherAddDeduction = ent.SalaryItems.Where(s => s.SALARYITEMID == "2c8c93e5-c870-470d-9339-007e191232ed").FirstOrDefault();//其它加扣款
                var OtherAddDeduction = (from si in ent.SalaryItems
                                       join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                                         where s.SALARYITEMNAME == "其它加扣款"
                                       select si).FirstOrDefault();
                tmp.OtherAddDeduction = OtherAddDeduction == null ? string.Empty : OtherAddDeduction.SUM;

                //var OtherSubjoin = ent.SalaryItems.Where(s => s.SALARYITEMID == "c356bcf0-add3-40fe-b4f6-2e30e86daebe").FirstOrDefault();//其它代扣款
                var OtherSubjoin = (from si in ent.SalaryItems
                                       join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                                    where s.SALARYITEMNAME == "其它代扣款"
                                       select si).FirstOrDefault();
                tmp.OtherSubjoin = OtherSubjoin == null ? string.Empty : OtherSubjoin.SUM;

                //var MantissaDeduct = ent.SalaryItems.Where(s => s.SALARYITEMID == "c8bf6ab9-6578-4fff-8d69-6ec274754de5").FirstOrDefault();//尾数扣款
                var MantissaDeduct = (from si in ent.SalaryItems
                                    join s in dal.GetObjects<T_HR_SALARYITEM>() on si.SALARYITEMID equals s.SALARYITEMID
                                      where s.SALARYITEMNAME == "尾数扣款"
                                    select si).FirstOrDefault();
                tmp.MantissaDeduct = MantissaDeduct == null ? string.Empty : MantissaDeduct.SUM;

                var Remarks = addsums.Where(s => s.EMPLOYEEID == ent.EmployeeID);
                if (Remarks.Count() > 0)
                {
                    foreach (var re in Remarks)
                    {
                        tmp.Remark += re.PROJECTNAME + ":" + re.PROJECTMONEY + "；";
                    }
                }


                V_EmployeeDeductionMoneys.Add(tmp);
            }
            return V_EmployeeDeductionMoneys;
        }
        public List<V_SalarySummary> GetSalarySummary(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);
            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEESALARYRECORD");

            var ents = from c in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                     //  join f in dal.GetObjects<T_HR_ATTENDMONTHLYBALANCE>() on c.EMPLOYEEID equals f.EMPLOYEEID
                       join e in dal.GetObjects<T_HR_EMPLOYEE>() on c.EMPLOYEEID equals e.EMPLOYEEID
                       // join b in dal.GetObjects<T_HR_EMPLOYEEPOST>() on c.EMPLOYEEID equals b.T_HR_EMPLOYEE.EMPLOYEEID
                       join b in dal.GetObjects<T_HR_POST>() on e.OWNERPOSTID equals b.POSTID//员工个人档案里面部门和岗位
                       join d in dal.GetObjects<T_HR_COMPANY>() on c.OWNERCOMPANYID equals d.COMPANYID
                      // where f.BALANCEYEAR == banlanceYear && f.BALANCEMONTH == banlanceMonth && c.ATTENDANCEUNUSUALTIMES == f.OWNERCOMPANYID
                      // where c.SALARYYEAR == year && c.SALARYMONTH == month && c.ATTENDANCEUNUSUALTIMES == f.OWNERCOMPANYID
                       //where f.BALANCEYEAR == banlanceYear && f.BALANCEMONTH == banlanceMonth //去掉比较公司条件，这里还是要根据考勤的年月，薪资年月查找数据会很多
                       //where c.SALARYYEAR == year && c.SALARYMONTH == month
                       select new SalryRecordView
                       {
                           orgName = d.CNAME,
                           CNAME = b.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME,
                           DepartmentName = b.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME,
                           PostName = b.T_HR_POSTDICTIONARY.POSTNAME,
                           EmployeeName = e.EMPLOYEECNAME,
                           IDNumber = e.IDNUMBER,
                           PostLevel = b.POSTLEVEL,
                           SalaryLevle = b.SALARYLEVEL,
                           OWNERID = c.OWNERID,
                           OWNERPOSTID = c.OWNERPOSTID,
                           OWNERDEPARTMENTID = c.OWNERDEPARTMENTID,
                           OWNERCOMPANYID = c.OWNERCOMPANYID,
                           SalaryRecordID = c.EMPLOYEESALARYRECORDID,
                           salaryMonth = c.SALARYMONTH,
                           salaryYear = c.SALARYYEAR,
                           CREATEUSERID = c.CREATEUSERID,
                           EmployeeID = c.EMPLOYEEID,
                           BanKID = e.BANKCARDNUMBER,
                           BankName = e.BANKID,
                           SalaryItems = c.T_HR_EMPLOYEESALARYRECORDITEM,
                           ActuallyPay = c.ACTUALLYPAY,
                       };
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);
            if (IsPageing)
            {
                ents = Utility.Pager<SalryRecordView>(ents, pageIndex, pageSize, ref pageCount);
            }
            //var addsums = from f in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
            //              where f.CHECKSTATE == "2" && f.DEALMONTH == month && f.DEALYEAR == year
            //              select f;
            var addsums = from f in dal.GetObjects<T_HR_EMPLOYEEADDSUM>()
                          where f.CHECKSTATE == "2" 
                          select f;
            //addsums = addsums.Where(tempFilter, tempParas.ToArray());//过滤加扣款信息，条件有查询的起止年月或姓名
            List<V_SalarySummary> V_SalarySum = new List<V_SalarySummary>();
            foreach (var ent in ents)
            {
                V_SalarySummary tmp = new V_SalarySummary();
                tmp.PayCompany = ent.orgName;
                tmp.Organization = ent.CNAME + " - " + ent.DepartmentName + " - " + ent.PostName;
                tmp.CompanyName = ent.CNAME;
                tmp.DeptName = ent.DepartmentName;
                tmp.PostName = ent.PostName;
                tmp.BankID = ent.BanKID;
                tmp.BankName = ent.BankName;
                tmp.EmployeeName = ent.EmployeeName;
                tmp.IDNumber = ent.IDNumber;
                tmp.SalaryDate = ent.salaryYear + "年" + ent.salaryMonth + "月";
                tmp.ActuallyPay = ent.ActuallyPay;
                var salaryTtem = ent.SalaryItems.FirstOrDefault();
                if (salaryTtem != null)
                {
                    var salaryArchive = (from c in dal.GetObjects<T_HR_SALARYARCHIVE>()
                                         where c.SALARYARCHIVEID == salaryTtem.SALARYARCHIVEID
                                         select c).FirstOrDefault();

                    if (salaryArchive != null)
                    {
                        //if (ent.PostLevel != null && ent.SalaryLevle != null)
                        //{
                        byte[] array = new byte[1];
                        array[0] = (byte)(65 + Convert.ToInt32(salaryArchive.POSTLEVEL));
                        tmp.PostCode = Encoding.ASCII.GetString(array).ToString();
                        tmp.PostCode += "-" + salaryArchive.SALARYLEVEL.ToString();
                        //   }
                    }
                }
                var salaryItems = ent.SalaryItems.OrderBy(t => t.ORDERNUMBER);//根据排序号排序
                foreach (var item in salaryItems)
                {
                    int order = 1;
                    if (item.ORDERNUMBER != null)
                    {
                        order = Convert.ToInt32(item.ORDERNUMBER.Value);
                    }
                    switch (order)//排序为对应薪资项
                    {
                        case 1: tmp.SubTotal = item == null ? string.Empty : item.SUM; break;//应发小计
                        case 2: break;
                        case 3: tmp.BasicSalary = item == null ? string.Empty : item.SUM; break;//基本工资
                        case 4: tmp.PostSalary = item == null ? string.Empty : item.SUM; break;//岗位工资
                        case 5: tmp.SecurityAllowance = item == null ? string.Empty : item.SUM; break;//保密津贴
                        case 6: tmp.HousingAllowance = item == null ? string.Empty : item.SUM; break;//住房补贴
                        case 7: tmp.AreadifAllowance = item == null ? string.Empty : item.SUM; break;//地区差异补贴
                        case 8: tmp.FoodAllowance = item == null ? string.Empty : item.SUM; break;//餐费补贴
                        case 9: tmp.FixIncomeSum = item == null ? string.Empty : item.SUM; break;//固定收入合计
                        case 10: tmp.AbsenceDays = item == null ? string.Empty : item.SUM; break;//缺勤天数
                        case 11: tmp.OvertimeSum = item == null ? string.Empty : item.SUM; break;//加班费
                        case 12: tmp.DutyAllowance = item == null ? string.Empty : item.SUM; break;//值班津贴
                        case 13: tmp.WorkingSalary = item == null ? string.Empty : item.SUM; break;//出勤工资
                        case 14: tmp.HousingDeduction = item == null ? string.Empty : item.SUM; break;//住房公积金扣款
                        case 15: tmp.PersonalsiCost = item == null ? string.Empty : item.SUM; break;//个人社保负担
                        case 16: tmp.PretaxSubTotal = item == null ? string.Empty : item.SUM; break;//税前应发合计
                        case 17: tmp.VacationDeduct = item == null ? string.Empty : item.SUM; break;//假期其它扣款
                        case 18: tmp.OtherAddDeduction = item == null ? string.Empty : item.SUM; break;//其它加扣款
                        case 19: tmp.PerformancerewardRecord = item == null ? string.Empty : item.SUM; break;//绩效奖金
                        case 20: tmp.TaxCoefficient = item == null ? string.Empty : item.SUM; break;//纳税系数
                        case 21: tmp.Sum = item == null ? string.Empty : item.SUM; break;//计税工资
                        case 22: tmp.TaxesBasic = item == null ? string.Empty : item.SUM; break;//扣税基数
                        case 23: tmp.Balance = item == null ? string.Empty : item.SUM; break;//差额
                        case 24: tmp.TaxesRate = item == null ? string.Empty : item.SUM; break;//税率
                        case 25: tmp.CalculateDeduct = item == null ? string.Empty : item.SUM; break; //速算扣除数
                        case 26: tmp.Personalincometax = item == null ? string.Empty : item.SUM; break;//个人所得税
                        case 27: tmp.AttendanceUnusualDeduct = item == null ? string.Empty : item.SUM; break;//考勤异常扣款
                        case 28: tmp.OtherSubjoin = item == null ? string.Empty : item.SUM; break;//其它代扣款
                        case 29: tmp.MantissaDeduct = item == null ? string.Empty : item.SUM; break;//尾数扣款
                        case 30: tmp.DeductTotal = item == null ? string.Empty : item.SUM; break;//扣款合计
                        default:break;
                    }
                }
                #region 旧方法
               // // var VacationDeduct = ent.SalaryItems.Where(s => s.SALARYITEMID == "7a4a0abf-24a0-481d-ade1-c52d4a0c56cf").FirstOrDefault();//假期其它扣款
               // var VacationDeduct = (from si in ent.SalaryItems
               //                       join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                       where s.SALARYITEMNAME == "假期其它扣款"
               //                       select si).FirstOrDefault();

               // tmp.VacationDeduct = VacationDeduct == null ? string.Empty : VacationDeduct.SUM;

               // //var BasicSalary = ent.SalaryItems.Where(s => s.SALARYITEMID == "ea67192b-a34a-42b9-b56c-253c9f8e0f35").FirstOrDefault();//基本工资
               // var BasicSalary = (from si in ent.SalaryItems
               //                    join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                    where s.SALARYITEMNAME == "基本工资"
               //                    select si).FirstOrDefault();
               // tmp.BasicSalary = BasicSalary == null ? string.Empty : BasicSalary.SUM;

               // //var SecurityAllowance = ent.SalaryItems.Where(s => s.SALARYITEMID == "4b83c88a-2b60-43f0-bf22-9a88a78204d0").FirstOrDefault();//保密津贴
               // var SecurityAllowance = (from si in ent.SalaryItems
               //                          join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                          where s.SALARYITEMNAME == "保密津贴"
               //                          select si).FirstOrDefault();
               // tmp.SecurityAllowance = SecurityAllowance == null ? string.Empty : SecurityAllowance.SUM;

               // //var HousingAllowance = ent.SalaryItems.Where(s => s.SALARYITEMID == "f4ad7117-1d2f-4515-b216-0586d2fd8664").FirstOrDefault();//住房补贴
               // var HousingAllowance = (from si in ent.SalaryItems
               //                         join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                         where s.SALARYITEMNAME == "住房补贴"
               //                          select si).FirstOrDefault();
               // tmp.HousingAllowance = HousingAllowance == null ? string.Empty : HousingAllowance.SUM;

               // //var WorkingSalary = ent.SalaryItems.Where(s => s.SALARYITEMID == "916ddaea-a612-40b8-908e-cd28c1a57137").FirstOrDefault();//出勤工资
               // var WorkingSalary = (from si in  ent.SalaryItems
               //                      join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                      where s.SALARYITEMNAME == "出勤工资"
               //                         select si).FirstOrDefault();
               // tmp.WorkingSalary = WorkingSalary == null ? string.Empty : WorkingSalary.SUM;

               // //var TaxCoefficient = ent.SalaryItems.Where(s => s.SALARYITEMID == "8540c3f1-b448-4eec-adf7-83bff08550bd").FirstOrDefault();//纳税系数
               // var TaxCoefficient = (from si in ent.SalaryItems
               //                       join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                       where s.SALARYITEMNAME == "纳税系数"
               //                      select si).FirstOrDefault();
               // tmp.TaxCoefficient = TaxCoefficient == null ? string.Empty : TaxCoefficient.SUM;

               //// var Sum = ent.SalaryItems.Where(s => s.SALARYITEMID == "9a42a345-7d5f-439f-9820-9cdbbcbec871").FirstOrDefault();//计税工资
               // var Sum = (from si in ent.SalaryItems
               //            join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //            where s.SALARYITEMNAME == "计税工资"
               //                       select si).FirstOrDefault();
               // tmp.Sum = Sum == null ? string.Empty : Sum.SUM;

               // //var AreadifAllowance = ent.SalaryItems.Where(s => s.SALARYITEMID == "58d9c878-150f-4d14-b1a9-d7f07f8527c1").FirstOrDefault();//地区差异补贴
               // var AreadifAllowance = (from si in ent.SalaryItems
               //                         join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                         where s.SALARYITEMNAME == "地区差异补贴"
               //                       select si).FirstOrDefault();
               // tmp.AreadifAllowance = AreadifAllowance == null ? string.Empty : AreadifAllowance.SUM;

               // //var TaxesBasic = ent.SalaryItems.Where(s => s.SALARYITEMID == "039e58e4-7877-49f0-8a1b-3250634f785c").FirstOrDefault();//扣税基数
               // var TaxesBasic = (from si in ent.SalaryItems
               //                   join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                   where s.SALARYITEMNAME == "扣税基数"
               //                       select si).FirstOrDefault();
               // tmp.TaxesBasic = TaxesBasic == null ? string.Empty : TaxesBasic.SUM;

               // //var PersonalsiCost = ent.SalaryItems.Where(s => s.SALARYITEMID == "a1448582-f354-43db-a2a9-1f138b98d2d2").FirstOrDefault();//个人社保负担
               // var PersonalsiCost = (from si in ent.SalaryItems
               //                       join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                       where s.SALARYITEMNAME == "个人社保负担"
               //                       select si).FirstOrDefault();
               // tmp.PersonalsiCost = PersonalsiCost == null ? string.Empty : PersonalsiCost.SUM;

               // //var PostSalary = ent.SalaryItems.Where(s => s.SALARYITEMID == "fba9f626-ab50-41e6-b479-2649cd7e20d4").FirstOrDefault();//岗位工资
               // var PostSalary = (from si in ent.SalaryItems
               //                   join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                   where s.SALARYITEMNAME == "岗位工资"
               //                       select si).FirstOrDefault();
               // tmp.PostSalary = PostSalary == null ? string.Empty : PostSalary.SUM;

               // //var HousingDeduction = ent.SalaryItems.Where(s => s.SALARYITEMID == "bee94c6a-e860-4939-8572-fff587486392").FirstOrDefault();//住房公积金扣款
               // var HousingDeduction = (from si in ent.SalaryItems
               //                         join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                         where s.SALARYITEMNAME == "住房公积金扣款"
               //                       select si).FirstOrDefault();
               // tmp.HousingDeduction = HousingDeduction == null ? string.Empty : HousingDeduction.SUM;

               // //var TaxesRate = ent.SalaryItems.Where(s => s.SALARYITEMID == "5c142665-ecda-489c-8e7d-95a39c2144cf").FirstOrDefault();//税率
               // var TaxesRate = (from si in ent.SalaryItems
               //                  join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                  where s.SALARYITEMNAME == "税率"
               //                       select si).FirstOrDefault();
               // tmp.TaxesRate = TaxesRate == null ? string.Empty : TaxesRate.SUM;

               // //var FoodAllowance = ent.SalaryItems.Where(s => s.SALARYITEMID == "86926078-d4c4-4fb6-a9c2-1910843a9309").FirstOrDefault();//餐费补贴
               // var FoodAllowance = (from si in ent.SalaryItems
               //                      join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                      where s.SALARYITEMNAME == "餐费补贴"
               //                       select si).FirstOrDefault();
               // tmp.FoodAllowance = FoodAllowance == null ? string.Empty : FoodAllowance.SUM;

               // //var Personalincometax = ent.SalaryItems.Where(s => s.SALARYITEMID == "a3162966-cb70-497a-a972-180337d8fffd").FirstOrDefault();//个人所得税
               // var Personalincometax = (from si in ent.SalaryItems
               //                          join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                          where s.SALARYITEMNAME == "个人所得税"
               //                       select si).FirstOrDefault();
               // tmp.Personalincometax = Personalincometax == null ? string.Empty : Personalincometax.SUM;

               // //var AbsenceDays = ent.SalaryItems.Where(s => s.SALARYITEMID == "857b5020-0abb-4a7b-b087-812450c5c0e9").FirstOrDefault();//缺勤天数
               // var AbsenceDays = (from si in ent.SalaryItems
               //                    join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                    where s.SALARYITEMNAME == "缺勤天数"
               //                       select si).FirstOrDefault();
               // tmp.AbsenceDays = AbsenceDays == null ? string.Empty : AbsenceDays.SUM;

               // //var OvertimeSum = ent.SalaryItems.Where(s => s.SALARYITEMID == "23993b15-5e3e-4780-8273-a367b143b046").FirstOrDefault();//加班费
               // var OvertimeSum = (from si in ent.SalaryItems
               //                    join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                    where s.SALARYITEMNAME == "加班费"
               //                       select si).FirstOrDefault();
               // tmp.OvertimeSum = OvertimeSum == null ? string.Empty : OvertimeSum.SUM;

               // //var OtherSubjoin = ent.SalaryItems.Where(s => s.SALARYITEMID == "c356bcf0-add3-40fe-b4f6-2e30e86daebe").FirstOrDefault();//其它代扣款
               // var OtherSubjoin = (from si in ent.SalaryItems
               //                     join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                     where s.SALARYITEMNAME == "其它代扣款"
               //                       select si).FirstOrDefault();
               // tmp.OtherSubjoin = OtherSubjoin == null ? string.Empty : OtherSubjoin.SUM;

               // //var FixIncomeSum = ent.SalaryItems.Where(s => s.SALARYITEMID == "7692ec13-0785-432d-abd8-aa26b993216c").FirstOrDefault();//固定收入合计
               // var FixIncomeSum = (from si in ent.SalaryItems
               //                     join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                     where s.SALARYITEMNAME == "固定收入合计"
               //                       select si).FirstOrDefault();
               // tmp.FixIncomeSum = FixIncomeSum == null ? string.Empty : FixIncomeSum.SUM;

               // //var AttendanceUnusualDeduct = ent.SalaryItems.Where(s => s.SALARYITEMID == "c6cfc5c4-8d5b-4290-aa8d-2af3858502d1").FirstOrDefault();//考勤异常扣款
               // var AttendanceUnusualDeduct = (from si in ent.SalaryItems
               //                                join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                                where s.SALARYITEMNAME == "考勤异常扣款"
               //                       select si).FirstOrDefault();
               // tmp.AttendanceUnusualDeduct = AttendanceUnusualDeduct == null ? string.Empty : AttendanceUnusualDeduct.SUM;

               // //var DutyAllowance = ent.SalaryItems.Where(s => s.SALARYITEMID == "99e04010-9e4b-4853-9b4c-616846865bea").FirstOrDefault();//值班津贴
               // var DutyAllowance = (from si in ent.SalaryItems
               //                      join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                      where s.SALARYITEMNAME == "值班津贴"
               //                       select si).FirstOrDefault();
               // tmp.DutyAllowance = DutyAllowance == null ? string.Empty : DutyAllowance.SUM;

               // //var OtherAddDeduction = ent.SalaryItems.Where(s => s.SALARYITEMID == "2c8c93e5-c870-470d-9339-007e191232ed").FirstOrDefault();//其它加扣款
               // var OtherAddDeduction = (from si in ent.SalaryItems
               //                          join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                          where s.SALARYITEMNAME == "其它加扣款"
               //                       select si).FirstOrDefault();
               // tmp.OtherAddDeduction = OtherAddDeduction == null ? string.Empty : OtherAddDeduction.SUM;

               // //var PretaxSubTotal = ent.SalaryItems.Where(s => s.SALARYITEMID == "0c473d24-fa15-431a-994f-40523d5fe09a").FirstOrDefault();//税前应发合计
               // var PretaxSubTotal = (from si in ent.SalaryItems
               //                       join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                       where s.SALARYITEMNAME == "税前应发合计"
               //                       select si).FirstOrDefault();
               // tmp.PretaxSubTotal = PretaxSubTotal == null ? string.Empty : PretaxSubTotal.SUM;

               // //var SubTotal = ent.SalaryItems.Where(s => s.SALARYITEMID == "bc8a3557-c931-4ec5-984f-42a01b8cf75b").FirstOrDefault();//应发小计
               // var SubTotal = (from si in ent.SalaryItems
               //                 join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                 where s.SALARYITEMNAME == "应发小计"
               //                       select si).FirstOrDefault();
               // tmp.SubTotal = SubTotal == null ? string.Empty : SubTotal.SUM;

               // //var Balance = ent.SalaryItems.Where(s => s.SALARYITEMID == "d62310ea-f5ec-4393-862c-da3500722689").FirstOrDefault();//差额
               // var Balance = (from si in ent.SalaryItems
               //                join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                where s.SALARYITEMNAME == "差额"
               //                       select si).FirstOrDefault();
               // tmp.Balance = Balance == null ? string.Empty : Balance.SUM;

               // //var CalculateDeduct = ent.SalaryItems.Where(s => s.SALARYITEMID == "68d52d34-805e-4873-bb21-544746ec5d1e").FirstOrDefault();//速算扣除数
               // var CalculateDeduct = (from si in ent.SalaryItems
               //                        join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                        where s.SALARYITEMNAME == "速算扣除数"
               //                       select si).FirstOrDefault();
               // tmp.CalculateDeduct = CalculateDeduct == null ? string.Empty : CalculateDeduct.SUM;

               // //var MantissaDeduct = ent.SalaryItems.Where(s => s.SALARYITEMID == "c8bf6ab9-6578-4fff-8d69-6ec274754de5").FirstOrDefault();//尾数扣款
               // var MantissaDeduct = (from si in ent.SalaryItems
               //                       join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                       where s.SALARYITEMNAME == "尾数扣款"
               //                       select si).FirstOrDefault();
               // tmp.MantissaDeduct = MantissaDeduct == null ? string.Empty : MantissaDeduct.SUM;

               // //var DeductTotal = ent.SalaryItems.Where(s => s.SALARYITEMID == "e7e58f22-eae0-4026-970d-c8c70a9faee9").FirstOrDefault();//扣款合计
               // var DeductTotal = (from si in ent.SalaryItems
               //                    join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                    where s.SALARYITEMNAME == "扣款合计"
               //                       select si).FirstOrDefault();
               // tmp.DeductTotal = DeductTotal == null ? string.Empty : DeductTotal.SUM;

               //// var PerformancerewardRecord = ent.SalaryItems.Where(s => s.SALARYITEMID == "421cc017-b203-4cfa-9396-f617663107eb").FirstOrDefault();//绩效奖金
               // var PerformancerewardRecord = (from si in ent.SalaryItems
               //                                join s in items on si.SALARYITEMID equals s.SALARYITEMID
               //                                where s.SALARYITEMNAME == "绩效奖金"
               //                       select si).FirstOrDefault();
               // tmp.PerformancerewardRecord = PerformancerewardRecord == null ? string.Empty : PerformancerewardRecord.SUM;
                #endregion
                var deducts = addsums.Where(s => s.EMPLOYEEID == ent.EmployeeID && s.DEALMONTH == ent.salaryMonth && s.DEALYEAR == ent.salaryYear);//扣款备注
                if (deducts.Count() > 0)
                {
                    foreach (var re in deducts)
                    {
                        tmp.DeductRemark += re.PROJECTNAME + ":" + re.PROJECTMONEY + "；";
                    }
                }

                V_SalarySum.Add(tmp);
            }
            V_SalarySum = V_SalarySum.OrderBy(t => t.CompanyName).ThenBy(t => t.DeptName).ThenBy(t => t.PostName).ToList();
            return V_SalarySum;
        }

        public byte[] ExportSalarySummary(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            try
            {
                List<V_SalarySummary> V_SalarySum = GetSalarySummary(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, year, month, IsPageing);

                if (V_SalarySum.Any())
                {
                    List<string> colName = new List<string>();
                    colName.Add("银行帐号");
                    colName.Add("开户行");
                    colName.Add("发薪机构");
                   // colName.Add("行政单位");
                    colName.Add("公司");
                    colName.Add("部门");
                    colName.Add("岗位");
                    colName.Add("员工姓名");
                    colName.Add("身份证号");
                    colName.Add("职级代码");
                    colName.Add("发放月份");
                    colName.Add("实发工资");
                    colName.Add("应发小计");
                    colName.Add("基本工资");
                    colName.Add("岗位工资");
                    colName.Add("保密津贴");
                    colName.Add("住房补贴");
                    colName.Add("地区差异补贴");
                    colName.Add("餐费补贴");
                    colName.Add("固定收入合计");
                    colName.Add("缺勤天数");
                    colName.Add("加班费");
                    colName.Add("值班津贴");
                    colName.Add("出勤工资");
                    colName.Add("住房公积金扣款");
                    colName.Add("个人社保负担");
                    colName.Add("税前应发合计");
                    colName.Add("假期其它扣款");
                    colName.Add("其它加扣款");
                    colName.Add("绩效奖金");
                    colName.Add("纳税系数");
                    colName.Add("计税工资");
                    colName.Add("扣税基数");
                    colName.Add("差额");
                    colName.Add("税率");
                    colName.Add("速算扣除数");
                    colName.Add("个人所得税");
                    colName.Add("考勤异常扣款");
                    colName.Add("其它代扣款");
                    colName.Add("尾数扣款");
                    colName.Add("扣款合计");
                    colName.Add("备注");

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < colName.Count; i++)
                    {
                        sb.Append(colName[i] + ",");
                    }
                    sb.Append("\r\n"); // 列头
                    //合计
                    decimal totalActuallyPay = 0;
                    decimal totalSubTotal = 0;
                    decimal totalBasicSalary = 0;
                    decimal totalPostSalary = 0;
                    decimal totalSecurityAllowance = 0;
                    decimal totalHousingAllowance = 0;
                    decimal totalAreadifAllowance = 0;
                    decimal totalFoodAllowance = 0;
                    decimal totalFixIncomeSum = 0;
                    decimal totalAbsenceDays = 0;
                    decimal totalOvertimeSum = 0;
                    decimal totalDutyAllowance = 0;
                    decimal totalWorkingSalary = 0;
                    decimal totalHousingDeduction = 0;
                    decimal totalPersonalsiCost = 0;
                    decimal totalPretaxSubTotal = 0;
                    decimal totalVacationDeduct = 0;
                    decimal totalOtherAddDeduction = 0;
                    decimal totalPerformancerewardRecord = 0;
                    // decimal totalTaxCoefficient = 0;
                    decimal totalSum = 0;
                    //decimal totalTaxesBasic = 0;
                    decimal totalBalance = 0;
                    decimal totalCalculateDeduct = 0;
                    decimal totalPersonalincometax = 0;
                    decimal totalAttendanceUnusualDeduct = 0;
                    decimal totalOtherSubjoin = 0;
                    decimal totalMantissaDeduct = 0;
                    decimal totalDeductTotal = 0;

                    //内容
                    foreach (var ent in V_SalarySum)
                    {
                        sb.Append(ent.BankID + ",");
                        sb.Append(ent.BankName + ",");
                        sb.Append(ent.PayCompany + ",");
                        //string[] org = ent.Organization.Split('-');
                        //sb.Append(org[0] + ",");
                        //sb.Append(org[1] + ",");
                        //sb.Append(org[2] + ",");
                        sb.Append(ent.CompanyName + ",");
                        sb.Append(ent.DeptName + ",");
                        sb.Append(ent.PostName + ",");
                        //sb.Append(ent.Organization + ",");
                        sb.Append(ent.EmployeeName + ",");
                        //sb.Append(ent.IDNumber + ",");
                        sb.Append(SpaceStringSeperator(ent.IDNumber) + ",");
                        sb.Append(ent.PostCode + ",");
                        sb.Append(ent.SalaryDate + ",");

                        string strActuallyPay = AESDecrypt(ent.ActuallyPay);
                        decimal ActuallyPay = 0;
                        decimal.TryParse(strActuallyPay, out ActuallyPay);
                        totalActuallyPay += ActuallyPay;
                        sb.Append(strActuallyPay + ",");

                        string strSubTotal = AESDecrypt(ent.SubTotal);
                        decimal SubTotal = 0;
                        decimal.TryParse(strSubTotal, out SubTotal);
                        totalSubTotal += SubTotal;
                        sb.Append(strSubTotal + ",");


                        string strBasicSalary = AESDecrypt(ent.BasicSalary);
                        decimal BasicSalary = 0;
                        decimal.TryParse(strBasicSalary, out BasicSalary);
                        totalBasicSalary += BasicSalary;
                        sb.Append(strBasicSalary + ",");

                        string strPostSalary = AESDecrypt(ent.PostSalary);
                        decimal PostSalary = 0;
                        decimal.TryParse(strPostSalary, out PostSalary);
                        totalPostSalary += PostSalary;
                        sb.Append(strPostSalary + ",");

                        string strSecurityAllowance = AESDecrypt(ent.SecurityAllowance);
                        decimal SecurityAllowance = 0;
                        decimal.TryParse(strSecurityAllowance, out SecurityAllowance);
                        totalSecurityAllowance += SecurityAllowance;
                        sb.Append(strSecurityAllowance + ",");

                        string strHousingAllowance = AESDecrypt(ent.HousingAllowance);
                        decimal HousingAllowance = 0;
                        decimal.TryParse(strHousingAllowance, out HousingAllowance);
                        totalHousingAllowance += HousingAllowance;
                        sb.Append(strHousingAllowance + ",");

                        string strAreadifAllowance = AESDecrypt(ent.AreadifAllowance);
                        decimal AreadifAllowance = 0;
                        decimal.TryParse(strAreadifAllowance, out AreadifAllowance);
                        totalAreadifAllowance += AreadifAllowance;
                        sb.Append(strAreadifAllowance + ",");

                        string strFoodAllowance = AESDecrypt(ent.FoodAllowance);
                        decimal FoodAllowance = 0;
                        decimal.TryParse(strFoodAllowance, out FoodAllowance);
                        totalFoodAllowance += FoodAllowance;
                        sb.Append(strFoodAllowance + ",");

                        string strFixIncomeSum = AESDecrypt(ent.FixIncomeSum);
                        decimal FixIncomeSum = 0;
                        decimal.TryParse(strFixIncomeSum, out FixIncomeSum);
                        totalFixIncomeSum += FixIncomeSum;
                        sb.Append(strFixIncomeSum + ",");

                        string strAbsenceDays = AESDecrypt(ent.AbsenceDays);
                        decimal AbsenceDays = 0;
                        decimal.TryParse(strAbsenceDays, out AbsenceDays);
                        totalAbsenceDays += AbsenceDays;
                        sb.Append(strAbsenceDays + ",");

                        string strOvertimeSum = AESDecrypt(ent.OvertimeSum);
                        decimal OvertimeSum = 0;
                        decimal.TryParse(strOvertimeSum, out OvertimeSum);
                        totalOvertimeSum += OvertimeSum;
                        sb.Append(strOvertimeSum + ",");

                        string strDutyAllowance = AESDecrypt(ent.DutyAllowance);
                        decimal DutyAllowance = 0;
                        decimal.TryParse(strDutyAllowance, out DutyAllowance);
                        totalDutyAllowance += DutyAllowance;
                        sb.Append(strDutyAllowance + ",");

                        string strWorkingSalary = AESDecrypt(ent.WorkingSalary);
                        decimal WorkingSalary = 0;
                        decimal.TryParse(strWorkingSalary, out WorkingSalary);
                        totalWorkingSalary += WorkingSalary;
                        sb.Append(strWorkingSalary + ",");

                        string strHousingDeduction = AESDecrypt(ent.HousingDeduction);
                        decimal HousingDeduction = 0;
                        decimal.TryParse(strHousingDeduction, out HousingDeduction);
                        totalHousingDeduction += HousingDeduction;
                        sb.Append(strHousingDeduction + ",");

                        string strPersonalsiCost = AESDecrypt(ent.PersonalsiCost);
                        decimal PersonalsiCost = 0;
                        decimal.TryParse(strPersonalsiCost, out PersonalsiCost);
                        totalPersonalsiCost += PersonalsiCost;
                        sb.Append(strPersonalsiCost + ",");

                        string strPretaxSubTotal = AESDecrypt(ent.PretaxSubTotal);
                        decimal PretaxSubTotal = 0;
                        decimal.TryParse(strPretaxSubTotal, out PretaxSubTotal);
                        totalPretaxSubTotal += PretaxSubTotal;
                        sb.Append(strPretaxSubTotal + ",");

                        string strVacationDeduct = AESDecrypt(ent.VacationDeduct);
                        decimal VacationDeduct = 0;
                        decimal.TryParse(strVacationDeduct, out VacationDeduct);
                        totalVacationDeduct += VacationDeduct;
                        sb.Append(strVacationDeduct + ",");

                        string strOtherAddDeduction = AESDecrypt(ent.OtherAddDeduction);
                        decimal OtherAddDeduction = 0;
                        decimal.TryParse(strOtherAddDeduction, out OtherAddDeduction);
                        totalOtherAddDeduction += OtherAddDeduction;
                        sb.Append(strOtherAddDeduction + ",");

                        string strPerformancerewardRecord = AESDecrypt(ent.PerformancerewardRecord);
                        decimal PerformancerewardRecord = 0;
                        decimal.TryParse(strPerformancerewardRecord, out PerformancerewardRecord);
                        totalPerformancerewardRecord += PerformancerewardRecord;
                        sb.Append(strPerformancerewardRecord + ",");

                        string strTaxCoefficient = AESDecrypt(ent.TaxCoefficient);
                        //decimal TaxCoefficient = 0;
                        //decimal.TryParse(strTaxCoefficient, out TaxCoefficient);
                        //totalTaxCoefficient += TaxCoefficient;
                        sb.Append(strTaxCoefficient + ",");

                        string strSum = AESDecrypt(ent.Sum);
                        decimal Sum = 0;
                        decimal.TryParse(strSum, out Sum);
                        totalSum += Sum;
                        sb.Append(strSum + ",");

                        string strTaxesBasic = AESDecrypt(ent.TaxesBasic);
                        //decimal TaxesBasic = 0;
                        //decimal.TryParse(strTaxesBasic, out TaxesBasic);
                        //totalTaxesBasic += TaxesBasic;
                        sb.Append(strTaxesBasic + ",");

                        string strBalance = AESDecrypt(ent.Balance);
                        decimal Balance = 0;
                        decimal.TryParse(strBalance, out Balance);
                        totalBalance += Balance;
                        sb.Append(strBalance + ",");

                        sb.Append(AESDecrypt(ent.TaxesRate) + ",");

                        string strCalculateDeduct = AESDecrypt(ent.CalculateDeduct);
                        decimal CalculateDeduct = 0;
                        decimal.TryParse(strCalculateDeduct, out CalculateDeduct);
                        totalCalculateDeduct += CalculateDeduct;
                        sb.Append(strCalculateDeduct + ",");

                        string strPersonalincometax = AESDecrypt(ent.Personalincometax);
                        decimal Personalincometax = 0;
                        decimal.TryParse(strPersonalincometax, out Personalincometax);
                        totalPersonalincometax += Personalincometax;
                        sb.Append(strPersonalincometax + ",");

                        string strAttendanceUnusualDeduct = AESDecrypt(ent.AttendanceUnusualDeduct);
                        decimal AttendanceUnusualDeduct = 0;
                        decimal.TryParse(strAttendanceUnusualDeduct, out AttendanceUnusualDeduct);
                        totalAttendanceUnusualDeduct += AttendanceUnusualDeduct;
                        sb.Append(strAttendanceUnusualDeduct + ",");

                        string strOtherSubjoin = AESDecrypt(ent.OtherSubjoin);
                        decimal OtherSubjoin = 0;
                        decimal.TryParse(strOtherSubjoin, out OtherSubjoin);
                        totalOtherSubjoin += OtherSubjoin;
                        sb.Append(strOtherSubjoin + ",");

                        string strMantissaDeduct = AESDecrypt(ent.MantissaDeduct);
                        decimal MantissaDeduct = 0;
                        decimal.TryParse(strMantissaDeduct, out MantissaDeduct);
                        totalMantissaDeduct += MantissaDeduct;
                        sb.Append(strMantissaDeduct + ",");

                        string strDeductTotal = AESDecrypt(ent.DeductTotal);
                        decimal DeductTotal = 0;
                        decimal.TryParse(strDeductTotal, out DeductTotal);
                        totalDeductTotal += DeductTotal;
                        sb.Append(strDeductTotal + ",");

                        if (!string.IsNullOrWhiteSpace(ent.DeductRemark))
                        {
                            sb.Append(ent.DeductRemark.Replace("\r", "").Replace("\n", "").Trim() + ",");
                        }
                        else
                        {
                            sb.Append("" + ","); 
                        }


                        sb.Append("\r\n");
                    }
                    //合计
                    sb.Append("合计" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append(totalActuallyPay.ToString() + ",");
                    sb.Append(totalSubTotal.ToString() + ",");
                    sb.Append(totalBasicSalary.ToString() + ",");
                    sb.Append(totalPostSalary.ToString() + ",");
                    sb.Append(totalSecurityAllowance.ToString() + ",");
                    sb.Append(totalHousingAllowance.ToString() + ",");
                    sb.Append(totalAreadifAllowance.ToString() + ",");
                    sb.Append(totalFoodAllowance.ToString() + ",");
                    sb.Append(totalFixIncomeSum.ToString() + ",");
                    sb.Append(totalAbsenceDays.ToString() + ",");
                    sb.Append(totalOvertimeSum.ToString() + ",");
                    sb.Append(totalDutyAllowance.ToString() + ",");
                    sb.Append(totalWorkingSalary.ToString() + ",");
                    sb.Append(totalHousingDeduction.ToString() + ",");
                    sb.Append(totalPersonalsiCost.ToString() + ",");
                    sb.Append(totalPretaxSubTotal.ToString() + ",");
                    sb.Append(totalVacationDeduct.ToString() + ",");
                    sb.Append(totalOtherAddDeduction.ToString() + ",");
                    sb.Append(totalPerformancerewardRecord.ToString() + ",");
                    sb.Append("" + ",");
                    sb.Append(totalSum.ToString() + ",");
                    sb.Append("" + ",");
                    sb.Append(totalBalance.ToString() + ",");
                    sb.Append("" + ",");
                    sb.Append(totalCalculateDeduct.ToString() + ",");
                    sb.Append(totalPersonalincometax.ToString() + ",");
                    sb.Append(totalAttendanceUnusualDeduct.ToString() + ",");
                    sb.Append(totalOtherSubjoin.ToString() + ",");
                    sb.Append(totalMantissaDeduct.ToString() + ",");
                    sb.Append(totalDeductTotal.ToString() + ",");
                    sb.Append("" + ",");
                    sb.Append("\r\n");

                    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                    return result;

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportSalarySummary:" + ex.Message);
                return null;
            }
        }

        private string SpaceStringSeperator(string str)
        {
            int position = 6, increseNum = 8;
            while (position < str.Length)
            {
                str=str.Insert(position, " ");
                position += increseNum+1;
            }
            return str;
        }

        public byte[] ExportEmployeeDeductionMoney(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, string year, string month, bool IsPageing)
        {
            try
            {
                List<V_EmployeeDeductionMoney> V_EmployeeDeductionMoneys = GetEmployeeDeductionMoney(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, year, month, IsPageing);

                if (V_EmployeeDeductionMoneys.Count > 0)
                {
                    List<string> colName = new List<string>();
                    colName.Add("发薪机构");
                    colName.Add("行政单位");
                    colName.Add("身份证号");
                    colName.Add("员工姓名");
                    colName.Add("扣借款");
                    colName.Add("其它加扣款");
                    colName.Add("其它代扣款");
                    colName.Add("尾数扣款");
                    colName.Add("备注");

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < colName.Count; i++)
                    {
                        sb.Append(colName[i] + ",");
                    }
                    sb.Append("\r\n"); // 列头

                    //扣税明细汇总
                    decimal DeductionBorrow = 0;
                    decimal OtherAddDeduction = 0;
                    decimal OtherSubjoin = 0;
                    decimal MantissaDeduct = 0;

                    decimal totalDeductionBorrow = 0;
                    decimal totalOtherAddDeduction = 0;
                    decimal totalOtherSubjoin = 0;
                    decimal totalMantissaDeduct = 0;

                    string strDeductionBorrow;
                    string strOtherAddDeduction;
                    string strOtherSubjoin;
                    string strMantissaDeduct;

                    //内容
                    foreach (var ent in V_EmployeeDeductionMoneys)
                    {
                        strDeductionBorrow = AESDecrypt(ent.DeductionBorrow);
                        strOtherAddDeduction = AESDecrypt(ent.OtherAddDeduction);
                        strOtherSubjoin = AESDecrypt(ent.OtherSubjoin);
                        strMantissaDeduct = AESDecrypt(ent.MantissaDeduct);

                        decimal.TryParse(strDeductionBorrow, out DeductionBorrow);
                        decimal.TryParse(strOtherAddDeduction, out OtherAddDeduction);
                        decimal.TryParse(strOtherSubjoin, out OtherSubjoin);
                        decimal.TryParse(strMantissaDeduct, out MantissaDeduct);

                        totalDeductionBorrow += DeductionBorrow;
                        totalOtherAddDeduction += OtherAddDeduction;
                        totalOtherSubjoin += OtherSubjoin;
                        totalMantissaDeduct += MantissaDeduct;

                        sb.Append(ent.PayCompany + ",");
                        sb.Append(ent.Organization + ",");
                        sb.Append(ent.IdNumber + ",");
                        sb.Append(ent.EmployeeName + ",");
                        sb.Append(strDeductionBorrow + ",");
                        sb.Append(strOtherAddDeduction + ",");
                        sb.Append(strOtherSubjoin + ",");
                        sb.Append(strMantissaDeduct + ",");                        

                        if (!string.IsNullOrWhiteSpace(ent.Remark))
                        {
                            sb.Append(ent.Remark.Replace("\r", "").Replace("\n", "").Trim() + ",");
                        }
                        else
                        {
                            sb.Append("" + ",");
                        }

                        sb.Append("\r\n");
                    }

                    //添加合计
                    sb.Append("合计" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append(totalDeductionBorrow + ",");
                    sb.Append(totalOtherAddDeduction + ",");
                    sb.Append(totalOtherSubjoin + ",");
                    sb.Append(totalMantissaDeduct + ",");
                    sb.Append("" + ",");
                    sb.Append("\r\n");

                    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                    return result;

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeeDeductionMoney:" + ex.Message);
                return null;
            }
        }

        public byte[] ExportMonthDeductionTaxs(int pageIndex, int pageSize, string sort, string filterString, IList<object> paras, ref int pageCount, string userID, bool IsPageing)
        {
            try
            {
                List<V_MonthDeductionTax> VmonthDeductiontaxs = GetMonthDeductionTaxs(pageIndex, pageSize, sort, filterString, paras, ref  pageCount, userID, IsPageing);

                if (VmonthDeductiontaxs.Count > 0)
                {
                    List<string> colName = new List<string>();
                    colName.Add("发薪机构");
                    colName.Add("行政单位");
                    colName.Add("身份证号");
                    colName.Add("员工姓名");
                    colName.Add("计税合计");
                    colName.Add("差额");
                    colName.Add("扣税基数");
                    colName.Add("税率");
                    colName.Add("速算扣除数");
                    colName.Add("个人所得税");

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < colName.Count; i++)
                    {
                        sb.Append(colName[i] + ",");
                    }
                    sb.Append("\r\n"); // 列头

                    //添加合计
                    decimal Sum = 0;
                    decimal Balance = 0;
                    decimal CalculateDeduct = 0;
                    decimal Personalincometax = 0;

                    decimal totalSum = 0;
                    decimal totalBalance = 0;
                    decimal totalCalculateDeduct = 0;
                    decimal totalPersonalincometax = 0;

                    string strSum;
                    string strBalance;
                    string strCalculateDeduct;
                    string strPersonalincometax;
                    //内容
                    foreach (var ent in VmonthDeductiontaxs)
                    {
                        strSum = AESDecrypt(ent.Sum);
                        strBalance = AESDecrypt(ent.Balance);
                        strCalculateDeduct = AESDecrypt(ent.CalculateDeduct);
                        strPersonalincometax = AESDecrypt(ent.Personalincometax);

                        decimal.TryParse(strSum, out Sum);
                        decimal.TryParse(strBalance, out Balance);
                        decimal.TryParse(strCalculateDeduct, out CalculateDeduct);
                        decimal.TryParse(strPersonalincometax, out Personalincometax);

                        totalSum += Sum;
                        totalBalance += Balance;
                        totalCalculateDeduct += CalculateDeduct;
                        totalPersonalincometax += Personalincometax;

                        sb.Append(ent.PayCompany + ",");
                        sb.Append(ent.Organization + ",");
                        sb.Append(ent.IdNumber + ",");
                        sb.Append(ent.EmployeeName + ",");
                        sb.Append(strSum + ",");
                        sb.Append(strBalance + ",");
                        sb.Append(AESDecrypt(ent.TaxesBasic) + ",");
                        sb.Append(AESDecrypt(ent.TaxesRate) + ",");
                        sb.Append(strCalculateDeduct + ",");
                        sb.Append(strPersonalincometax + ",");
                        sb.Append("\r\n");
                    }
                    //合计
                    sb.Append("合计" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append(totalSum.ToString() + ",");
                    sb.Append(totalBalance.ToString() + ",");
                    sb.Append("" + ",");
                    sb.Append("" + ",");
                    sb.Append(totalCalculateDeduct.ToString() + ",");
                    sb.Append(totalPersonalincometax + ",");
                    sb.Append("\r\n");
                    byte[] result = Encoding.GetEncoding("GB2312").GetBytes(sb.ToString());
                    return result;

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportMonthDeductionTaxs:" + ex.Message);
                return null;
            }
        }
        public class SalryRecordView
        {
            public string orgName { get; set; }
            public string CNAME { get; set; }
            public string DepartmentName { get; set; }
            public string PostName { get; set; }
            public string EmployeeName { get; set; }
            public string IDNumber { get; set; }
            public string OWNERID { get; set; }
            public string OWNERPOSTID { get; set; }
            public string OWNERDEPARTMENTID { get; set; }
            public string OWNERCOMPANYID { get; set; }
            public string SalaryRecordID { get; set; }
            public string salaryMonth { get; set; }
            public string salaryYear { get; set; }
            public string CREATEUSERID { get; set; }
            public string EmployeeID { get; set; }
            public decimal? PostLevel { get; set; }
            public decimal? SalaryLevle { get; set; }
            public string BanKID { get; set; }
            public string BankName { get; set; }
            public string ActuallyPay { get; set; }
            public EntityCollection<T_HR_EMPLOYEESALARYRECORDITEM> SalaryItems { get; set; }
        }
        private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        /// <summary>   
        /// AES解密   
        /// </summary>   
        /// <param name="hashedData">密文字节</param>     
        /// <returns>返回解密后的字符串</returns>    
        public static string AESDecrypt(string hashedData)
        {
            //分组加密算法 
            string result = string.Empty;
            SymmetricAlgorithm des;
            System.Security.Cryptography.AesManaged aesmanaged = new System.Security.Cryptography.AesManaged();
            try
            {
                des = aesmanaged as SymmetricAlgorithm;
                des.Key = Encoding.UTF8.GetBytes("yujianhuareshgrt");
                des.IV = _key1;
                byte[] cipherText = Convert.FromBase64String(hashedData);
                byte[] decryptBytes = new byte[cipherText.Length];
                MemoryStream ms = new MemoryStream(cipherText);
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
                cs.Read(decryptBytes, 0, decryptBytes.Length);
                cs.Close();
                ms.Close();
                result = System.Text.Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }
    }
}
