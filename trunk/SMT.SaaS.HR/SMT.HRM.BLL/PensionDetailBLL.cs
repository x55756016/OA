using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMT.HRM.DAL;
using SMT_HRM_EFModel;
using System.Linq.Dynamic;
using SMT.HRM.CustomModel;
using System.Data.OleDb;
using System.Data;

namespace SMT.HRM.BLL
{
    public class PensionDetailBLL : BaseBll<T_HR_PENSIONDETAIL>
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
        public IQueryable<T_HR_PENSIONDETAIL> PensionDetailPaging(int pageIndex, int pageSize, string sort, string filterString, object[] paras, ref int pageCount, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_PENSIONDETAIL");
            IQueryable<T_HR_PENSIONDETAIL> ents = dal.GetObjects().Include("T_HR_PENSIONMASTER");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            ents = Utility.Pager<T_HR_PENSIONDETAIL>(ents, pageIndex, pageSize, ref pageCount);

            return ents;
        }

        public IQueryable<T_HR_PENSIONDETAIL> PensionDetailForReport(string sort, string filterString, IList<object> paras, string userID)
        {
            List<object> queryParas = new List<object>();
            queryParas.AddRange(paras);

            SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_PENSIONDETAIL");
            IQueryable<T_HR_PENSIONDETAIL> ents = dal.GetObjects().Include("T_HR_PENSIONMASTER");
            if (!string.IsNullOrEmpty(filterString))
            {
                ents = ents.Where(filterString, queryParas.ToArray());
            }
            ents = ents.OrderBy(sort);

            
            return ents;
        }
        #region 导出社保明细报表
        
        
        public byte[] ExportPensionDetailReport(string sort, string filterString, IList<object> paras, string userID, string CompanyId, string StrYear, string StrMonth)
        {

            byte[] result = null;
            try
            {

                List<T_HR_PENSIONDETAIL> CollectDetails = new List<T_HR_PENSIONDETAIL>();
                IQueryable<T_HR_PENSIONDETAIL> employeeInfos = PensionDetailForReport(sort,  filterString, paras,  userID);

                string CompanyName = "";
                string StrDate = "";

                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == CompanyId
                           select ent;

                if (ents.Count() > 0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = StrYear + "年" + StrMonth + "月";
                if (employeeInfos.Count() > 0)
                {
                    CollectDetails = employeeInfos.ToList();
                }
                result = OutEmployeePensionDetailsStream(CompanyName, StrDate, CollectDetails);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeesCollectReports出现错误:" + ex.Message);

            }
            return result;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="Strdate"></param>
        /// <param name="EmployeeInfos"></param>
        /// <returns></returns>
        public static byte[] OutEmployeePensionDetailsStream(string CompanyName, string Strdate, List<T_HR_PENSIONDETAIL> EmployeeInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeePensionDetailsBody(CompanyName, Strdate, EmployeeInfos).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }
        /// <summary>
        /// 画模版
        /// </summary>
        /// <param name="CompanyName">公司名</param>
        /// <param name="Strdate">年月</param>
        /// <param name="Collects">数据集合</param>
        /// <returns></returns>
        public static StringBuilder GetEmployeePensionDetailsBody(string CompanyName, string Strdate, List<T_HR_PENSIONDETAIL> Collects)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" BORDER=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\" colspan=\"12\">" + CompanyName + "产业单位" + Strdate + "员工社保缴交记录表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">截止时间：</td>");
            s.Append("<td  align=left class=\"title\" colspan=\"11\">" + Strdate + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");


            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >序号</td>");
            s.Append("<td align=center class=\"title\" >社保年份</td>");
            s.Append("<td align=center class=\"title\" >社保月份</td>");
            s.Append("<td align=center class=\"title\" >员工姓名</td>");
            s.Append("<td align=center class=\"title\" >证件号码</td>");
            s.Append("<td align=center class=\"title\" >社保卡号</td>");
            s.Append("<td align=center class=\"title\" >社保电脑号</td>");
            s.Append("<td align=center class=\"title\" >缴交基数</td>");
            s.Append("<td align=center class=\"title\" >交缴合计</td>");
            s.Append("<td align=center class=\"title\" >最近交缴时间</td>");

            s.Append("<td align=center class=\"title\" >个人交缴合计</td>");
            s.Append("<td align=center class=\"title\" >单位交缴合计</td>");
            s.Append("<td align=center class=\"title\" >养老保险个人缴</td>");
            s.Append("<td align=center class=\"title\" >养老保险单位缴</td>");
            s.Append("<td align=center class=\"title\" >住房公积金单位缴</td>");
            s.Append("<td align=center class=\"title\" >医疗保险个人缴</td>");
            s.Append("<td align=center class=\"title\" >医疗保险单位缴</td>");
            s.Append("<td align=center class=\"title\" >工伤保险单位缴</td>");
            s.Append("<td align=center class=\"title\" >失业保险单位缴</td>");
            s.Append("<td align=center class=\"title\" >生育保险单位缴</td>");

            s.Append("<td align=center class=\"title\" >养老保险个人比例</td>");
            s.Append("<td align=center class=\"title\" >养老保险单位比例</td>");
            s.Append("<td align=center class=\"title\" >住房公积金单位比例</td>");
            s.Append("<td align=center class=\"title\" >住房公积金个人缴</td>");
            s.Append("<td align=center class=\"title\" >住房公积金个人比例</td>");
            s.Append("<td align=center class=\"title\" >医疗保险个人比例</td>");
            s.Append("<td align=center class=\"title\" >医疗保险单位比例</td>");
            s.Append("<td align=center class=\"title\" >工伤保险单位比例</td>");
            s.Append("<td align=center class=\"title\" >工伤保险个人缴</td>");
            s.Append("<td align=center class=\"title\" >工伤保险个人比例</td>");

            s.Append("<td align=center class=\"title\" >失业保险单位比例</td>");
            s.Append("<td align=center class=\"title\" >失业保险个人缴</td>");
            s.Append("<td align=center class=\"title\" >失业保险个比例</td>");
            s.Append("<td align=center class=\"title\" >生育保险单位比例</td>");
            s.Append("<td align=center class=\"title\" >生育保险个人缴</td>");
            s.Append("<td align=center class=\"title\" >生育保险个人比例</td>");
            s.Append("<td align=center class=\"title\" >养老缴交基数</td>");
            s.Append("<td align=center class=\"title\" >工伤缴交基数</td>");
            s.Append("<td align=center class=\"title\" >医疗缴交基数</td>");
            s.Append("<td align=center class=\"title\" >生育缴交基数</td>");

            s.Append("<td align=center class=\"title\" >失业缴交基数</td>");
            s.Append("<td align=center class=\"title\" >住房公积金缴交基数</td>");
            s.Append("<td align=center class=\"title\" >老干部单位缴</td>");
            s.Append("<td align=center class=\"title\" >老干部单位比例</td>");
            s.Append("<td align=center class=\"title\" >大额补助单位缴</td>");
            s.Append("<td align=center class=\"title\" >大额补助单位比例</td>");
            s.Append("<td align=center class=\"title\" >大额补助个人缴</td>");
            s.Append("<td align=center class=\"title\" >大额补助个人比例</td>");
            s.Append("<td align=center class=\"title\" >本地户口</td>");
            s.Append("<td align=center class=\"title\" >付款日期</td>");
            
            
            s.Append("</tr>");




            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + (i + 1).ToString() + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PENSIONYEAR + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PENSIONMOTH + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].IDNUMBER + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].CARDID + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].COMPUTERNO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PAYBASE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].TOTALCOST + "</td>");
                    //if (Collects[i]. != null)
                    //{
                    //    s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].LASTDATE).ToString("yyyy-MM-dd") + "</td>");
                    //}
                    //else
                    //{
                    //上次缴交字段为空
                    s.Append("<td class=\"x1282\"></td>");
                    //}
                    s.Append("<td class=\"x1282\">" + Collects[i].TOTALPERSONCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].TOTALCOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PENSIONPERSONCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PENSIONCOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].HOUSINGFUNDCOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MEDICAREPERSONCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MEDICARECOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].INJURYINSURANCECOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].UNEMPLOYEDINSURANCECOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].BIRTHINSURANCECOMPANYCOST + "</td>");

                    s.Append("<td class=\"x1282\">" + Collects[i].PENSIONPERSONRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PENSIONCOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].HOUSINGFUNDCOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].HOUSINGFUNDPERSONCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].HOUSINGFUNDPERSONRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MEDICAREPERSONRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MEDICARECOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].INJURYINSURANCECOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].INJURYINSURANCEPERSONCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].INJURYINSURANCEPERSONRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].UNEMPLOYEDCOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].UNEMPLOYEDPERSON + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].UNEMPLOYEDPERSONRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].BIRTHCOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].BIRTHPERSONCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].BIRTHPERSONRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PENSIONBASE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].INJURYBASE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MEDICAREBASE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].BIRTHBASE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].UNEMPLOYEDBASE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].HOUSINGFUNDBASE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].CADRECOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].CADRECOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SUBSIDYCOMPANYCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SUBSIDYCOMPANYRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SUBSIDYPERSONCOST + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SUBSIDYPERSONRATIO + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ISLOCAL + "</td>");
                    if (Collects[i].PAYDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].PAYDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    
                    s.Append("</tr>");
                }
            }



            s.Append("</table>");

            s.Append("</body></html>");
            return s;
        }
        #endregion
        /// <summary>
        /// 删除社保记录
        /// </summary>
        /// <param name="pensionDetailIDs">社保记录ID组</param>
        /// <returns></returns>
        public int PensionDetailDelete(string[] pensionDetailIDs)
        {
            foreach (string id in pensionDetailIDs)
            {
                var ents = from a in dal.GetObjects()
                           where a.PENSIONDETAILID == id
                           select a;
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;
                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
            }
            return dal.SaveContextChanges();
        }

        private void PensionDetailAdd(List<T_HR_PENSIONDETAIL> entity)
        {
            foreach (var ent in entity)
            {
                T_HR_PENSIONDETAIL tempEnt = new T_HR_PENSIONDETAIL();
                Utility.CloneEntity(ent, tempEnt);
                //DataContext.AddObject("T_HR_PENSIONDETAIL", tempEnt);
                dal.AddToContext(tempEnt);
            }
            //DataContext.SaveChanges();
            dal.SaveContextChanges();
        }

        /// <summary>
        /// 导入Excel的员工社保卡信息
        /// </summary>
        /// <param name="strPath">当前上传的Excel文件路径</param>
        /// <param name="strMsg">处理消息</param>
        //public void ImportPensionByImportExcel(string strPath, string employeeID, ref string strMsg)
        //{
        //    ImportSetMasterBLL bll = new ImportSetMasterBLL();
        //    T_HR_IMPORTSETMASTER master = bll.GetImportSetMasterByEntityCode("T_HR_PENSIONDETAIL", employeeID);
        //    ImportExcel penBll = new ImportExcel(strPath, 0, master, new T_HR_PENSIONDETAIL());
        //    penBll.StartImport();
        //}
        public void ImportPensionByImportExcel(string strPath, Dictionary<string, string> paras, ref string strMsg)
        {
            try
            {
                ImportSetMasterBLL bll = new ImportSetMasterBLL();
                T_HR_IMPORTSETMASTER master = bll.GetImportSetMasterByEntityCode("T_HR_PENSIONDETAIL", paras["CITY"], paras["OWNERCOMPANYID"]);
                if (master != null)
                {
                    T_HR_PENSIONDETAIL dt = new T_HR_PENSIONDETAIL();
                    ImportExcel penBll = new ImportExcel(strPath, 0, master, new T_HR_PENSIONDETAIL(), paras);
                    bool sucess = penBll.StartImport();
                    if (sucess)
                    {
                        strMsg = "true";
                    }
                    else
                    {
                        strMsg = "false";
                    }
                }
                else
                {
                    strMsg = "NOTFOUND";
                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ImportPensionByImportExcel:" + ex.Message);
            }
        }

        /// <summary>
        /// 导入员工社保记录并返回显示
        /// </summary>
        /// <param name="strPath">路径</param>
        /// <param name="paras">参数字典</param>
        /// <param name="strMsg">返回的参数</param>
        public List<T_HR_PENSIONDETAIL> ImportPensionByImportExcelForShow(string strPath, Dictionary<string, string> paras, ref string strMsg)
        {
            List<T_HR_PENSIONDETAIL> ListResult = new List<T_HR_PENSIONDETAIL>();
            try
            {
                
                ImportSetMasterBLL bll = new ImportSetMasterBLL();
                T_HR_IMPORTSETMASTER master = bll.GetImportSetMasterByEntityCode("T_HR_PENSIONDETAIL", paras["CITY"], paras["OWNERCOMPANYID"]);
                if (master != null)
                {
                    
                    T_HR_PENSIONDETAIL dt = new T_HR_PENSIONDETAIL();
                    ImportExcel penBll = new ImportExcel(strPath, 0, master, new T_HR_PENSIONDETAIL(), paras);
                    ListResult = penBll.ReadExcelDataFirst();
                    
                }
                
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ImportPensionByImportExcel:" + ex.Message);
                
            }
            return ListResult;
        }

        #region 批量导入员工社保
        
        /// <summary>
        /// 批量添加员工社保
        /// </summary>
        /// <param name="listPension">社保集合</param>
        /// <param name="StrCity">城市</param>
        /// <param name="StrCompanyid">公司ID</param>
        /// <param name="StrYear">年</param>
        /// <param name="StrMonth">月</param>
        /// <param name="StrCreateUserId">创建用户</param>
        /// <param name="StrMsg">返回的消息，用来存放不合格记录的身份证号</param>
        /// <returns></returns>
        public bool BatchAddPensionDetail(List<T_HR_PENSIONDETAIL> listPension, Dictionary<string, string> paras, ref string StrMsg)
        {
            bool IsResult = false;
            SMT.Foundation.Log.Tracer.Debug("开始导入员工社保。");
            try
            {
                dal.BeginTransaction();
                ImportSetMasterBLL bll = new ImportSetMasterBLL();
                T_HR_IMPORTSETMASTER master = bll.GetImportSetMasterByEntityCode("T_HR_PENSIONDETAIL",paras["CITY"],paras["OWNERCOMPANYID"]);
                if (master != null)
                {
                    //用来记录身份证信息不存在的数量
                    int i = 0;
                    foreach (var ent in listPension)
                    {
                        var employee = from e in dal.GetObjects<T_HR_EMPLOYEE>()
                                       where e.IDNUMBER == ent.IDNUMBER && e.EDITSTATE == "1"
                                       select e;
                        if (employee.Count() > 0)
                        {
                            //删除旧的记录
                            var oldDetail = from c in dal.GetObjects<T_HR_PENSIONDETAIL>()
                                            where c.PENSIONMOTH == ent.PENSIONMOTH && c.PENSIONYEAR == ent.PENSIONYEAR && c.IDNUMBER == ent.IDNUMBER
                                            select c;
                            if (oldDetail.Count() > 0)
                            {
                                dal.DeleteFromContext(oldDetail.FirstOrDefault());
                            }

                            T_HR_EMPLOYEE entCurEmp = employee.FirstOrDefault();
                            if (ent.EMPLOYEEID != entCurEmp.EMPLOYEEID)
                            {
                                ent.EMPLOYEEID = entCurEmp.EMPLOYEEID;
                            }
                            //插入数据到数据库
                            dal.AddToContext(ent);
                        }
                        else
                        {
                            i++;
                            SMT.Foundation.Log.Tracer.Debug("PensionImport:" + i.ToString() + "行，没有员工身份证为此号码:" + ent.IDNUMBER);
                            StrMsg += "身份证号为：" + ent.IDNUMBER + "没有找到员工记录\n";
                        }

                    }
                    if (i > 0)
                    {
                        StrMsg += "共有" + i.ToString() + " 位员工社保导入错误。";
                    }

                    int k = dal.SaveContextChanges();
                    if (k > 0)
                    {
                        
                        dal.CommitTransaction();
                        IsResult = true;
                        CommDal<T_HR_PENSIONDETAIL> cdal = new CommDal<T_HR_PENSIONDETAIL>();

                        string strSql = " update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b,t_hr_employee c where a.IDNUMBER= c.IDNUMBER and b.employeeid=c.employeeid and b.editstate=1)"
                                       + "  ,a.employeeid= (select employeeid from t_hr_employee c where a.IDNUMBER = c.IDNUMBER  )"
                                       + "  ,a.CARDID= (select CARDID from t_hr_pensionmaster b ,t_hr_employee c where a.IDNUMBER = c.IDNUMBER and b.employeeid=c.employeeid and b.editstate=1)"
                                       + "  ,a.OWNERID=(select employeeid from t_hr_employee c where a.IDNUMBER = c.IDNUMBER )"
                                       + "  ,a.OWNERPOSTID=(select c.OWNERPOSTID from t_hr_employee c where a.IDNUMBER= c.IDNUMBER ) "
                                       + "  ,a.OWNERDEPARTMENTID=(select c.OWNERDEPARTMENTID from t_hr_employee c where a.IDNUMBER= c.IDNUMBER )"
                                       + " ,a.OWNERCOMPANYID=(select c.OWNERCOMPANYID from t_hr_employee c where a.IDNUMBER = c.IDNUMBER  )"
                                       + " where a.PENSIONYEAR='" + paras["YEAR"] + "' and a.PENSIONMOTH='" + paras["MONTH"] + "' and a.CREATEUSERID='" + paras["CREATEUSERID"] + "'";
                        //修改社保的所属人信息
                        cdal.ExecuteCustomerSql(strSql);
                    }
                    else
                    {
                        dal.RollbackTransaction();
                    }

                }
                else
                {
                    StrMsg = "社保主表记录不存在。";
                }

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(System.DateTime.Now.ToString() + " ImportPensionByImportExcel:" + ex.Message);
                dal.RollbackTransaction();
            }
            return IsResult;
        }


        #endregion


        #region 导出社保、薪酬

        public V_EmployeePensionReports GetEmployeePensionReports(string sort, string filterString, IList<object> paras, string userID,string CompanyID,DateTime Dt)
        {
            V_EmployeePensionReports Reports = new V_EmployeePensionReports();
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);

                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_PENSIONDETAIL");
                decimal IntYear = Dt.Year;
                decimal IntMonth = Dt.Month;
                IQueryable<T_HR_PENSIONDETAIL> ents = from ent in dal.GetObjects().Include("T_HR_PENSIONMASTER")
                                                      where ent.PENSIONMOTH == IntMonth
                                                      && ent.PENSIONYEAR == IntYear
                                                      && ent.OWNERCOMPANYID == CompanyID
                                                      
                                                      select ent;

                                                      
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                
                var entCompany = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == CompanyID
                           select ent;
                if (entCompany.Count() > 0)
                {
                    Reports.COMPANYNAME = entCompany.FirstOrDefault().BRIEFNAME;
                }
                string StrYear =  Dt.Year.ToString();
                string StrMonth = Dt.Month.ToString();
                Reports.ENDDATE = StrYear + "年" + StrMonth + "月";
                //养老总数
                //var CompanyPensions = ents.Sum(p => p.PENSIONCOMPANYCOST );
                //个人养老保险总数
                //var PersonPensions = ents.Sum(p=>p.PENSIONPERSONCOST);
                //养老保险人数
                //var PensionsNums = ents.Where(p=> p.PENSIONCOMPANYCOST >0 ).Count();
                Reports.FULLMEDICALNUMERS = 0;
                Reports.FULLLOSTNUMERS = 0;
                Reports.FULLCPFNUMERS = 0;
                Reports.FULLPENSIONNUMERS = 0;
                Reports.FULLINJURYNUMERS = 0;
                Reports.FULLBEARNUMERS = 0;

                Reports.FULLCOMPANYPENSIONNUMERS = 0;
                Reports.FULLOWNERPENSIONNUMERS = 0;//养老

                Reports.FULLCOMPANYCPFNUMERS = 0;
                Reports.FULLOWNERCPFNUMERS = 0;//公积金

                Reports.FULLCOMPANYMEDICALNUMERS = 0;
                Reports.FULLOWNERMEDICALNUMERS = 0;//医疗

                Reports.FULLCOMPANYINJURYNUMERS = 0;
                Reports.FULLOWNERINJURYNUMERS = 0;//工伤

                Reports.FULLCOMPANYLOSTNUMERS = 0;
                Reports.FULLOWNERLOSTNUMERS = 0;//失业

                Reports.FULLCOMPANYBEARNUMERS = 0;
                Reports.FULLOWNERBEARNUMERS = 0;//生育
                Reports.TOTOALFIXINCOMEMONEY = 0;
                #region 非合同工
                Reports.TEMPMEDICALNUMERS = 0;
                Reports.TEMPLOSTNUMERS = 0;
                Reports.TEMPCPFNUMERS = 0;
                Reports.TEMPPENSIONNUMERS = 0;
                Reports.TEMPINJURYNUMERS = 0;
                Reports.TEMPBEARNUMERS = 0;
                Reports.TEMPCOMPANYPENSIONNUMERS = 0;//养老保险
                Reports.TEMPOWNERPENSIONNUMERS = 0;//个人缴交养老保险

                Reports.TEMPCOMPANYCPFNUMERS = 0;//公司公积金
                Reports.TEMPOWNERCPFNUMERS = 0;//个人缴公积金

                Reports.TEMPCOMPANYMEDICALNUMERS = 0;//单位缴交医疗金
                Reports.TEMPOWNERMEDICALNUMERS = 0;//个人缴医疗金

                Reports.TEMPCOMPANYINJURYNUMERS = 0;//公司缴交工伤险
                Reports.TEMPOWNERINJURYNUMERS = 0;//公司缴交工伤险

                Reports.TEMPCOMPANYLOSTNUMERS = 0;//公司缴交失业险
                Reports.TEMPOWNERLOSTNUMERS = 0;//个人缴交失业险

                Reports.TEMPCOMPANYBEARNUMERS = 0;//公司缴交生育险
                Reports.TEMPOWNERBEARNUMERS = 0;//个人缴交生育险
                #endregion
                Reports.FULLNUMERSTOTAL = 0;
                Reports.TEMPNUMERS = 0;
                Reports.FULLCOMPANYTOTAL = 0;
                Reports.TEMPCOMPANYTOTAL = 0;
                Reports.FULLOWNERTOTAL = 0;
                Reports.TEMPOWNERTOTAL = 0;
                Reports.TOTOALFIXINCOMEMONEY = 0;

                if (ents.Count() > 0)
                { 
                    //ents = from contract in dal.GetObjects<T_HR_EMPLOYEECONTRACT>
                    
                    ents.ToList().ForEach(item => {
                        var employees = from ent in dal.GetObjects<T_HR_EMPLOYEECONTRACT>().Include("T_HR_EMPLOYEE")
                                        where ent.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID
                                        select ent;
                        
                            
                        //获取该员工当月的工资中固定收入
                        var Salary = from ent in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                     where ent.EMPLOYEEID == item.EMPLOYEEID
                                     && ent.SALARYYEAR == StrYear && ent.SALARYMONTH == StrMonth
                                     && ent.CHECKSTATE == "2"
                                     select ent;
                        
                        if (employees.Count() > 0)
                        {
                            Reports.FULLMEDICALNUMERS += 1;
                            Reports.FULLLOSTNUMERS += 1;
                            Reports.FULLCPFNUMERS += 1;
                            Reports.FULLPENSIONNUMERS += 1;
                            Reports.FULLINJURYNUMERS += 1;
                            Reports.FULLBEARNUMERS += 1;
                            if (item.PENSIONCOMPANYCOST != null)
                            {
                                Reports.FULLCOMPANYPENSIONNUMERS += item.PENSIONCOMPANYCOST;//养老保险
                            }
                            if (item.PENSIONPERSONCOST != null)
                            {
                                Reports.FULLOWNERPENSIONNUMERS += item.PENSIONPERSONCOST;//个人缴交养老保险
                            }
                            if (item.HOUSINGFUNDCOMPANYCOST != null)
                            {
                                Reports.FULLCOMPANYCPFNUMERS += item.HOUSINGFUNDCOMPANYCOST;//公司公积金
                            }
                            if (item.HOUSINGFUNDPERSONCOST != null)
                            {
                                Reports.FULLOWNERCPFNUMERS += item.HOUSINGFUNDPERSONCOST;//个人缴公积金
                            }
                            if (item.MEDICARECOMPANYCOST != null)
                            {
                                Reports.FULLCOMPANYMEDICALNUMERS += item.MEDICARECOMPANYCOST;//单位缴交医疗金
                            }
                            if (item.MEDICAREPERSONCOST != null)
                            {
                                Reports.FULLOWNERMEDICALNUMERS += item.MEDICAREPERSONCOST;//个人缴医疗金
                            }
                            if (item.INJURYINSURANCECOMPANYCOST != null)
                            {
                                Reports.FULLCOMPANYINJURYNUMERS += item.INJURYINSURANCECOMPANYCOST;//公司缴交工伤险
                            }
                            if (item.INJURYINSURANCEPERSONCOST != null)
                            {
                                Reports.FULLOWNERINJURYNUMERS += item.INJURYINSURANCEPERSONCOST;//公司缴交工伤险
                            }
                            if (item.UNEMPLOYEDINSURANCECOMPANYCOST != null)
                            {
                                Reports.FULLCOMPANYLOSTNUMERS += item.UNEMPLOYEDINSURANCECOMPANYCOST;//公司缴交失业险
                            }
                            if (item.UNEMPLOYEDPERSON != null)
                            {
                                Reports.FULLOWNERLOSTNUMERS += item.UNEMPLOYEDPERSON;//个人缴交失业险
                            }
                            if (item.BIRTHINSURANCECOMPANYCOST != null)
                            {
                                Reports.FULLCOMPANYBEARNUMERS += item.BIRTHINSURANCECOMPANYCOST;//公司缴交生育险
                            }
                            if (item.BIRTHPERSONCOST != null)
                            {
                                Reports.FULLOWNERBEARNUMERS += item.BIRTHPERSONCOST;//个人缴交生育险
                            }
                            if (Salary.Count() > 0)
                            {
                                Reports.TOTOALFIXINCOMEMONEY += Salary.FirstOrDefault().FIXEDINCOMESUM;

                            }
                        }
                        else
                        {
                            Reports.TEMPMEDICALNUMERS += 1;
                            Reports.TEMPLOSTNUMERS += 1;
                            Reports.TEMPCPFNUMERS += 1;
                            Reports.TEMPPENSIONNUMERS += 1;
                            Reports.TEMPINJURYNUMERS += 1;
                            Reports.TEMPBEARNUMERS += 1;

                            Reports.TEMPCOMPANYPENSIONNUMERS += item.PENSIONCOMPANYCOST;//养老保险
                            Reports.TEMPOWNERPENSIONNUMERS += item.PENSIONPERSONCOST;//个人缴交养老保险

                            Reports.TEMPCOMPANYCPFNUMERS += item.HOUSINGFUNDCOMPANYCOST;//公司公积金
                            Reports.TEMPOWNERCPFNUMERS += item.HOUSINGFUNDPERSONCOST;//个人缴公积金

                            Reports.TEMPCOMPANYMEDICALNUMERS += item.MEDICARECOMPANYCOST;//单位缴交医疗金
                            Reports.TEMPOWNERMEDICALNUMERS += item.MEDICAREPERSONCOST;//个人缴医疗金

                            Reports.TEMPCOMPANYINJURYNUMERS += item.INJURYINSURANCECOMPANYCOST;//公司缴交工伤险
                            Reports.TEMPOWNERINJURYNUMERS += item.INJURYINSURANCEPERSONCOST;//公司缴交工伤险

                            Reports.TEMPCOMPANYLOSTNUMERS += item.UNEMPLOYEDINSURANCECOMPANYCOST;//公司缴交失业险
                            Reports.TEMPOWNERLOSTNUMERS += item.UNEMPLOYEDPERSON;//个人缴交失业险

                            Reports.TEMPCOMPANYBEARNUMERS += item.BIRTHINSURANCECOMPANYCOST;//公司缴交生育险
                            Reports.TEMPOWNERBEARNUMERS += item.BIRTHPERSONCOST;//个人缴交生育险

                            if (Salary.Count() > 0)
                            {
                                Reports.TEMPTOTOALFIXINCOMEMONEY += Salary.FirstOrDefault().FIXEDINCOMESUM;

                            }
                        }
                    });
                    Reports.FULLNUMERSTOTAL = Reports.FULLMEDICALNUMERS + Reports.FULLINJURYNUMERS + Reports.FULLPENSIONNUMERS + Reports.FULLBEARNUMERS + Reports.FULLLOSTNUMERS + Reports.FULLCPFNUMERS;
                    Reports.TEMPNUMERS = Reports.TEMPMEDICALNUMERS + Reports.TEMPINJURYNUMERS + Reports.TEMPPENSIONNUMERS + Reports.TEMPBEARNUMERS + Reports.TEMPLOSTNUMERS + Reports.TEMPCPFNUMERS;
                    Reports.FULLCOMPANYTOTAL = Reports.FULLCOMPANYMEDICALNUMERS + Reports.FULLCOMPANYLOSTNUMERS + Reports.FULLCOMPANYINJURYNUMERS + Reports.FULLCOMPANYPENSIONNUMERS + Reports.FULLCOMPANYCPFNUMERS + Reports.FULLCOMPANYBEARNUMERS;
                    Reports.TEMPCOMPANYTOTAL = Reports.TEMPCOMPANYMEDICALNUMERS + Reports.TEMPCOMPANYLOSTNUMERS + Reports.TEMPCOMPANYINJURYNUMERS + Reports.TEMPCOMPANYPENSIONNUMERS + Reports.TEMPCOMPANYCPFNUMERS + Reports.TEMPCOMPANYBEARNUMERS;
                    Reports.FULLOWNERTOTAL = Reports.FULLOWNERMEDICALNUMERS + Reports.FULLOWNERPENSIONNUMERS + Reports.FULLOWNERLOSTNUMERS + Reports.FULLOWNERINJURYNUMERS + Reports.FULLOWNERCPFNUMERS + Reports.FULLOWNERBEARNUMERS;
                    Reports.TEMPOWNERTOTAL = Reports.TEMPOWNERMEDICALNUMERS + Reports.TEMPOWNERPENSIONNUMERS + Reports.TEMPOWNERLOSTNUMERS + Reports.TEMPOWNERINJURYNUMERS + Reports.TEMPOWNERCPFNUMERS + Reports.TEMPOWNERBEARNUMERS;

                    
                    Reports.TOTOALFIXINCOMEMONEY = Reports.FULLCOMPANYTOTAL + Reports.FULLOWNERTOTAL;
                    //Reports.TOTALPERFORMANCEMONEY = 

                }

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("PensionDetailBLL-GetEmployeePensionReports导出员工薪酬社保报表出错" + ex.ToString());
            }

            return Reports;
        }
        public byte[] ExportEmployeePensionReports(string sort, string filterString, IList<object> paras, string userID, string CompanyID,DateTime Dt)
        {

            byte[] result = null;
            try
            {
                V_EmployeePensionReports Reports = new V_EmployeePensionReports();                
                Reports = GetEmployeePensionReports( sort, filterString, paras, userID,CompanyID,Dt);               
                
                int cols = 7;
                result = OutPensionFileStream(cols, Reports);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("导出员工薪酬社保报表出错ExportEmployeePensionReports:" + ex.Message);

            }
            return result;


        }

        /// <summary>
        /// 将数据转为指定html格式，并以流的形式返回，以便导出成指定格式的文件
        /// </summary>
        /// <param name="title"></param>
        /// <param name="dt"></param>
        public static byte[] OutPensionFileStream(int cols, V_EmployeePensionReports dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetBody(dt,cols).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }
        /// <summary>
        /// 获取社保、薪酬的EXCEL
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static StringBuilder GetBody(V_EmployeePensionReports Reports, int cols)
        {
            StringBuilder s = new StringBuilder();
            int SecCols = cols - 1;
            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" border=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td colspan=\"" + cols + "\" align=center class=\"title\">" + Reports.COMPANYNAME + "产业单位" + Reports.ENDDATE + "薪酬、保险缴交明细表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td>截止时间：</td>");
            s.Append("<td colspan=\"" + SecCols + "\" align=center class=\"title\">" + Reports.ENDDATE + "</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td>产业单位：</td>");
            s.Append("<td colspan=\"" + SecCols + "\" align=center class=\"title\">" + Reports.COMPANYNAME + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");

            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\"  rowspan=\"2\" colspan=\"2\">保险类别</td>");
            s.Append("<td  align=center class=\"title\" rowspan=\"2\" >是否参保</td>");
            s.Append("<td align=center class=\"title\"  rowspan=\"2\">参保人数（人）</td>");
            s.Append("<td align=center class=\"title\" >单位缴纳</td>");
            s.Append("<td align=center class=\"title\" >个人缴纳</td>");
            s.Append("<td colspan=\"2\" align=center class=\"title\" >薪酬总额</td>");
                        
            s.Append("</tr>");
            s.Append("<tr>");
            //s.Append("<td ></td>");
            //s.Append("<td ></td>");
            //s.Append("<td ></td>");
            //s.Append("<td></td>");
            s.Append("<td align=center class=\"title\" >缴费金额合计(元)</td>");
            s.Append("<td align=center class=\"title\" >缴费金额合计(元)</td>");
            s.Append("<td align=center class=\"title\" >固定收入合计</td>");
            s.Append("<td align=center class=\"title\" >应发绩效工资合计</td>");
            s.Append("</tr>");
            #region 合同制员工            
            

            s.Append("<tr>");
            s.Append("<td rowspan=\"7\" class=\"x1282\">合同制员工</td>");
            s.Append("<td class=\"x1282\">养老</td>");
            s.Append("<td class=\"x1282\">是</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLPENSIONNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCOMPANYPENSIONNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLOWNERPENSIONNUMERS + "</td>");
            s.Append("<td class=\"x1282\" rowspan=\"6\">" + Reports.TOTOALFIXINCOMEMONEY + "</td>");
            s.Append("<td rowspan=\"6\">" + Reports.TOTALPERFORMANCEMONEY + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">工伤</td>");
            s.Append("<td class=\"x1282\">是</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLINJURYNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCOMPANYINJURYNUMERS + "</td>");
            s.Append("<td>" + Reports.FULLOWNERINJURYNUMERS + "</td>");
            
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">生育</td>");
            s.Append("<td class=\"x1282\">是</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLBEARNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCOMPANYBEARNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLOWNERBEARNUMERS + "</td>");
            
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">医疗</td>");
            s.Append("<td class=\"x1282\">是</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLMEDICALNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCOMPANYMEDICALNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLOWNERMEDICALNUMERS + "</td>");                     
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">失业</td>");
            s.Append("<td class=\"x1282\">是</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLLOSTNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCOMPANYLOSTNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLOWNERLOSTNUMERS + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">住房公积金</td>");
            s.Append("<td class=\"x1282\">是</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCPFNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCOMPANYCPFNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLOWNERCPFNUMERS + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">小计</td>");
            s.Append("<td class=\"x1282\">是</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLNUMERSTOTAL + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLCOMPANYTOTAL + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.FULLOWNERTOTAL + "</td>");
            s.Append("<td colspan=\"2\"></td>");
            s.Append("</tr>");
            
            #endregion

            #region 非合同制
            s.Append("<tr>");
            s.Append("<td rowspan=\"7\" class=\"x1282\">非合同制员工(指临时工/挂靠员工)</td>");
            s.Append("<td class=\"x1282\">养老</td>");
            s.Append("<td class=\"x1282\">/</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPPENSIONNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCOMPANYPENSIONNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPOWNERPENSIONNUMERS + "</td>");
            s.Append("<td class=\"x1282\" rowspan=\"6\">" + Reports.TEMPTOTOALFIXINCOMEMONEY + "</td>");
            s.Append("<td class=\"x1282\" rowspan=\"6\">" + Reports.TEMPTOTALPERFORMANCEMONEY + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">工伤</td>");
            s.Append("<td class=\"x1282\">/</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPINJURYNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCOMPANYINJURYNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPOWNERINJURYNUMERS + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">生育</td>");
            s.Append("<td class=\"x1282\">/</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPBEARNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCOMPANYBEARNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPOWNERBEARNUMERS + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">医疗</td>");
            s.Append("<td class=\"x1282\">/</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPMEDICALNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCOMPANYMEDICALNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPOWNERMEDICALNUMERS + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">失业</td>");
            s.Append("<td class=\"x1282\">/</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPLOSTNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCOMPANYLOSTNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPOWNERLOSTNUMERS + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">住房公积金</td>");
            s.Append("<td class=\"x1282\">/</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCPFNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCOMPANYCPFNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPOWNERCPFNUMERS + "</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td class=\"x1282\">总计</td>");
            s.Append("<td class=\"x1282\">/</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPNUMERS + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPCOMPANYTOTAL + "</td>");
            s.Append("<td class=\"x1282\">" + Reports.TEMPOWNERTOTAL + "</td>");
            //s.Append("<td  class=\"x1282\" colspan=\"2\"></td>");
            s.Append("</tr>");
            #endregion
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    s.Append("<tr>");
            //    for (int j = 0; j < dt.Columns.Count; j++)
            //    {
            //        s.Append("<td class='" + GetCss(dt.Rows[i][j].ToString(), dt.Columns[j].DataType.Name) + "'>" + dt.Rows[i][j].ToString() + "</td>");
            //    }
            //    s.Append("</tr>");
            //}
            s.Append("</table>");
            s.Append("</body></html>");
            return s;
        }

        /// <summary>
        /// 获取单元格样式
        /// </summary>
        /// <param name="str"></param>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static string GetCss(string str, string typename)
        {
            string tempStr = "x1282";
            if (!str.Equals("") && ("Int32,Decimal,Double".IndexOf(typename) >= 0))
            {

                int m = 0;
                if (str.LastIndexOf(".") >= 0)
                    m = str.Length - str.LastIndexOf('.') - 1;
                if (m >= 0) tempStr = "x" + m;
            }
            return tempStr;
        }
        #endregion

    }
}
