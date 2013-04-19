using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.HRM.CustomModel;

namespace SMT.HRM.BLL
{
    /// <summary>
    /// 人事报表
    /// </summary>
    public class ReportsBLL : BaseBll<T_HR_EMPLOYEE>
    {

        public List<V_EmployeeBasicInfo> EmployeesBasicInfoForReport(List<string> companyids, string strOwnerID, string strOrderBy, string filterString, IList<object> paras)
        {
            List<V_EmployeeBasicInfo> LstBasicInfos = new List<V_EmployeeBasicInfo>();
            try
            {
                List<object> objArgs = new List<object>();
                objArgs.AddRange(paras);
                if (!string.IsNullOrEmpty(strOwnerID))
                {
                    SetOrganizationFilter(ref filterString, ref objArgs, strOwnerID, "T_HR_EMPLOYEE");
                }

                if (string.IsNullOrEmpty(strOrderBy))
                {
                    strOrderBy = "p.ownercompanyid";
                }

                ReportsDAL dalReport = new ReportsDAL();
                DataTable dtRes = new DataTable();
                dtRes = dalReport.GetDataForExportEmployeesBasicInfo(strOrderBy, filterString, objArgs.ToArray());


                var q = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                        join o in dal.GetObjects<T_HR_EMPLOYEE>() on ent.T_HR_EMPLOYEE.EMPLOYEEID equals o.EMPLOYEEID
                        join echck in dal.GetObjects<T_HR_EMPLOYEECHECK>() on o.EMPLOYEEID equals echck.T_HR_EMPLOYEE.EMPLOYEEID
                        select new
                        {
                            ent

                        };



                List<string> StrEductions = new List<string>();
                StrEductions.Add("TOPEDUCATION");//最高学历
                StrEductions.Add("PROVINCE");//籍贯
                SMT.SaaS.BLLCommonServices.PermissionWS.V_Dictionary[] ListDicts = PermClient.GetDictionaryByCategoryArray(StrEductions.ToArray());
                //获取员工记录
                var Employees = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                where companyids.Contains(ent.OWNERCOMPANYID)
                                select ent;
                //获取公司信息
                var Companys = from ent in dal.GetObjects<T_HR_COMPANY>().Include("T_HR_COMPANY2")
                               select ent;
                //获取转正记录
                var Checks = from ent in dal.GetObjects<T_HR_EMPLOYEECHECK>().Include("T_HR_EMPLOYEE")
                             where companyids.Contains(ent.OWNERCOMPANYID)
                             select ent;
                //简历信息
                var Resumes = from ent in dal.GetObjects<T_HR_RESUME>()
                              select ent;
                //合同信息
                var Contracts = from ent in dal.GetObjects<T_HR_EMPLOYEECONTRACT>().Include("T_HR_EMPLOYEE")

                                select ent;


                foreach (DataRow row in dtRes.Rows)
                {
                    V_EmployeeBasicInfo report = new V_EmployeeBasicInfo();
                    string StrCompanyid = row["OWNERCOMPANYID"].ToString();
                    var SingleCompany = Companys.Where(p => p.COMPANYID == StrCompanyid);
                    if (SingleCompany.Count() > 0)
                    {
                        if (SingleCompany.FirstOrDefault().T_HR_COMPANY2 != null)
                        {
                            report.ORGANIZENAME = SingleCompany.FirstOrDefault().T_HR_COMPANY2.BRIEFNAME;
                        }
                        else
                        {
                            report.ORGANIZENAME = "神州通集团";
                        }
                    }
                    string StrEmployeeid = row["EMPLOYEEID"].ToString();

                    report.EMPLOYEEID = StrEmployeeid;

                    //report.ORGANIZENAME = row[""].ToString();

                    string StrEntryType = row["FORMTYPE"].ToString();
                    if (StrEntryType == "0" || StrEntryType == "1")//入职
                    {
                        DateTime DtNow = System.DateTime.Now;//当前系统时间
                        if (!string.IsNullOrEmpty(row["CHANGETIME"].ToString()))
                        {

                            DateTime DtEntry = System.Convert.ToDateTime(row["CHANGETIME"].ToString());
                            report.ENTRYDATE = DtEntry;
                            if (DtNow.Year == DtEntry.Year && DtNow.Month == DtEntry.Month)
                            {
                                report.REMARK += "新入职";
                            }

                            DateTime Nowdate = System.DateTime.Now;
                            int workAge = ((Nowdate.Year - DtEntry.Year) * 12 + (Nowdate.Month - DtEntry.Month - 1)) / 12;
                            if (Nowdate.Day >= DtEntry.Day)
                            {
                                workAge += 1;
                            }
                            report.WORKAGE = workAge;
                        }
                    }
                    if (Employees.Count() > 0)
                    {
                        var EntEmployee = Employees.Where(p => p.EMPLOYEEID == StrEmployeeid);

                        if (EntEmployee.Count() > 0)
                        {
                            T_HR_EMPLOYEE EmployeeObj = EntEmployee.FirstOrDefault();
                            string StrNumerID = EmployeeObj.IDNUMBER;
                            report.IDNUMBER = StrNumerID;
                            report.SEX = EmployeeObj.SEX == "1" ? "男" : "女";
                            report.ISMARRY = EmployeeObj.MARRIAGE == "1" ? "已婚" : "未婚";
                            report.EMPLOYEECNAME = EmployeeObj.EMPLOYEECNAME;

                            report.COMPANYNAME = row["NEXTCOMPANYNAME"].ToString();
                            report.DEPARTMENTNAME = row["NEXTDEPARTMENTNAME"].ToString();
                            report.POSTNAME = row["NEXTPOSTNAME"].ToString();
                            report.BIRTHDAY = EmployeeObj.BIRTHDAY;
                            report.MOBILE = EmployeeObj.MOBILE;
                            report.URGENCYCONTACT = EmployeeObj.URGENCYCONTACT;
                            report.URGENCYPERSON = EmployeeObj.URGENCYPERSON;
                            report.INTERESTCONTENT = EmployeeObj.INTERESTCONTENT;
                            report.FAMILYADDRESS = EmployeeObj.FAMILYADDRESS;
                            report.CREATEUSERID = EmployeeObj.CREATEUSERID;
                            report.OWNERID = EmployeeObj.OWNERID;
                            report.OWNERCOMPANYID = EmployeeObj.OWNERCOMPANYID;
                            report.OWNERDEPARTMENTID = EmployeeObj.OWNERDEPARTMENTID;
                            report.OWNERPOSTID = EmployeeObj.OWNERPOSTID;
                            report.REMARK = EmployeeObj.REMARK;
                            report.FIRSTCONTRACTENDDATE = null;
                            report.FIRSTCONTRACTDEADLINE = "";
                            report.SECONDCONTRACTENDDATE = null;
                            report.SECONDCONTRACTDEADLINE = "";
                            report.THIRDCONTRACTENDDATE = null;
                            report.THIIRDCONTRACTDEADLINE = "";
                            //report.SEX = EntEmployee.FirstOrDefault()
                            if (!string.IsNullOrEmpty(EmployeeObj.PROVINCE))
                            {
                                decimal IntPro = Convert.ToDecimal(EmployeeObj.PROVINCE);
                                var Educations = from ent in ListDicts
                                                 where ent.DICTIONCATEGORY == "PROVINCE"
                                                 && ent.DICTIONARYVALUE == IntPro
                                                 select ent;
                                if (Educations.Count() > 0)
                                {
                                    report.PROVINCE = Educations.FirstOrDefault().DICTIONARYNAME;
                                }
                            }
                            if (!string.IsNullOrEmpty(EmployeeObj.TOPEDUCATION))
                            {
                                decimal IntPro = Convert.ToDecimal(EntEmployee.FirstOrDefault().TOPEDUCATION);
                                var Educations = from ent in ListDicts
                                                 where ent.DICTIONCATEGORY == "TOPEDUCATION"
                                                 && ent.DICTIONARYVALUE == IntPro
                                                 select ent;
                                if (Educations.Count() > 0)
                                {
                                    report.TOPEDUCATION = Educations.FirstOrDefault().DICTIONARYNAME;
                                }
                            }
                            string companyid = EmployeeObj.OWNERCOMPANYID;

                            var EmployeeChecks = Checks.Where(p => p.OWNERCOMPANYID == companyid);

                            if (EmployeeChecks.Count() > 0)
                            {
                                if (EmployeeChecks.FirstOrDefault().BEREGULARDATE != null)
                                {
                                    report.BEREGULARDATE = EmployeeChecks.FirstOrDefault().BEREGULARDATE;
                                }
                            }

                            var EmployeeResumes = from ent in dal.GetObjects<T_HR_RESUME>()
                                                  where ent.IDCARDNUMBER == StrNumerID
                                                  select ent;
                            if (EmployeeResumes.Count() > 0)
                            {
                                report.SPECIALTY = EmployeeResumes.FirstOrDefault().SPECIALTY;
                                report.GRADUATESCHOOL = EmployeeResumes.FirstOrDefault().GRADUATESCHOOL;
                                //item.GRADUATEDATE = EmployeeResumes.FirstOrDefault().
                            }
                            #region 员工合同
                            var EmployeeContracts = Contracts.Where(p => p.T_HR_EMPLOYEE.EMPLOYEEID == StrEmployeeid && p.OWNERCOMPANYID == companyid && p.CHECKSTATE == "2");
                            if (EmployeeContracts.Count() > 0)
                            {
                                if (EmployeeContracts.Count() == 1)
                                {
                                    if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                                    {
                                        report.FIRSTCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                                        report.FIRSTCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();
                                        report.SECONDCONTRACTDEADLINE = "";
                                        report.SECONDCONTRACTENDDATE = null;
                                        report.THIIRDCONTRACTDEADLINE = "";
                                        report.THIRDCONTRACTENDDATE = null;
                                    }
                                }
                                int k = 0;
                                if (EmployeeContracts.Count() == 2)
                                {
                                    EmployeeContracts.ToList().ForEach(itemcontract =>
                                    {
                                        k++;
                                        if (k == 1)
                                        {
                                            if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                                            {
                                                report.FIRSTCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                                                report.FIRSTCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                                            }
                                        }
                                        if (k == 2)
                                        {
                                            if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                                            {
                                                report.SECONDCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                                                report.SECONDCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();
                                                report.THIIRDCONTRACTDEADLINE = "";
                                                report.THIRDCONTRACTENDDATE = null;
                                            }
                                        }
                                    });

                                }
                                if (EmployeeContracts.Count() > 2)
                                {
                                    EmployeeContracts.ToList().ForEach(itemcontract =>
                                    {
                                        k++;
                                        if (k == 1)
                                        {
                                            if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                                            {
                                                report.FIRSTCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                                                report.FIRSTCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                                            }
                                        }
                                        if (k == 2)
                                        {
                                            if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                                            {
                                                report.SECONDCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                                                report.SECONDCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                                            }
                                        }
                                        if (k == 3)
                                        {
                                            if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                                            {
                                                report.THIRDCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                                                report.THIIRDCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                                            }
                                        }
                                    });
                                }
                            }

                            #endregion

                            LstBasicInfos.Add(report);

                        }
                    }



                    //item.ENTRYDATE = Entrys.FirstOrDefault().ENTRYDATE;


                }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工报表出现错误-EmployeesBasicInfoForReport：" + ex.ToString());
            }
            return LstBasicInfos;
        }


        public byte[] ExportEmployeesBasicInfoForReport(List<string> companyids, string strOwnerID, string strOrderBy, string filterString, IList<object> paras)
        {

            List<V_EmployeeBasicInfo> LstBasicInfos = new List<V_EmployeeBasicInfo>();
            LstBasicInfos = EmployeesBasicInfoForReport(companyids, strOwnerID, strOrderBy, filterString, paras);
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            ReportsBLL bll = new ReportsBLL();
            byte[] by = bll.ExportGetEmployeeBasicInfosToReportBll(LstBasicInfos);

            return by;
        }
        #region 员工汇总报表


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<V_EmployeeCollect> GetEmployeesCollectReports(ref List<V_EMPLOYEEDATADETAIL> EmployeeDatas, string sort, string filterString, IList<object> paras, string userID, string CompanyId, DateTime DtStart,DateTime DtEnd)
        {
            List<V_EmployeeCollect> ListColls = new List<V_EmployeeCollect>();
            try
            {
                V_EmployeeCollect CompanyCollect = new V_EmployeeCollect();
                var entCompanys = from ent in dal.GetObjects<T_HR_COMPANY>()
                                  where ent.T_HR_COMPANY2.COMPANYID == CompanyId
                                  select ent;
                //是上级公司，获取其下的子公司
                if (entCompanys.Count() > 0)
                {
                    CompanyCollect.ISTOP = "1";//为顶级公司
                    CompanyCollect.ORANIZATIONID = entCompanys.FirstOrDefault().COMPANYID;
                    CompanyCollect.ORGANIZATIONNAME = entCompanys.FirstOrDefault().BRIEFNAME;
                    ListColls.Add(CompanyCollect);
                    for (int i = 0; i < entCompanys.Count(); i++)
                    {
                        var ChildCompanys = from ent in entCompanys
                                            where ent.T_HR_COMPANY2.COMPANYID == entCompanys.ToList()[i].COMPANYID
                                            && ent.FATHERTYPE == "0"
                                            select ent;
                        if (ChildCompanys.Count() > 0)
                        {
                            GetChildOrganzationColls(ref ListColls, ref EmployeeDatas, "0", entCompanys.ToList()[i].COMPANYID, ChildCompanys.ToList(), null,DtStart,DtEnd);
                        }
                    }

                }
                else
                {
                    var SelfCompanys = from ent in dal.GetObjects<T_HR_COMPANY>()
                                       where ent.COMPANYID == CompanyId
                                       select ent;
                    if (SelfCompanys.Count() > 0)
                    {
                        CompanyCollect.ISTOP = "1";//公司名字
                        CompanyCollect.ORANIZATIONID = SelfCompanys.FirstOrDefault().COMPANYID;
                        CompanyCollect.ORGANIZATIONNAME = SelfCompanys.FirstOrDefault().BRIEFNAME;
                        ListColls.Add(CompanyCollect);


                    }
                    //不存在子公司,则取部门
                    var entDepartments = from ent in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_COMPANY")
                                         where ent.T_HR_COMPANY.COMPANYID == CompanyId
                                         select ent;
                    if (entDepartments.Count() > 0)
                    {
                        for (int i = 0; i < entDepartments.Count(); i++)
                        {
                            T_HR_DEPARTMENT Depart = new T_HR_DEPARTMENT();
                            Depart = entDepartments.ToList()[i];
                            var SingleDeparts = from ent in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY")
                                                where ent.DEPARTMENTID == Depart.DEPARTMENTID
                                                select ent;
                            if (SingleDeparts.Count() > 0)
                            {
                                var ChildDeparts = from ent in entDepartments
                                                   where ent.FATHERID == SingleDeparts.FirstOrDefault().DEPARTMENTID

                                                   select ent;
                                if (ChildDeparts.Count() > 0)
                                {
                                    GetChildOrganzationColls(ref ListColls, ref EmployeeDatas, "1", entDepartments.ToList()[i].DEPARTMENTID, null, ChildDeparts.ToList(), DtStart, DtEnd);
                                }
                                else
                                {
                                    GetEmployeeCollectByDepartment(ref EmployeeDatas, ref ListColls, SingleDeparts, DtStart, DtEnd);

                                }
                            }


                        }
                    }

                }
                //var ents = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                //           select new V_EmployeeCollect
                //           {
                //               //ORGANIZATIONNAME = "",

                //           }
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工汇总出现错误：" + ex.ToString());
            }
            return ListColls;
        }
        /// <summary>
        /// 获取部门下的员工统计信息
        /// </summary>
        /// <param name="EmployeeDatas"></param>
        /// <param name="ListColls"></param>
        /// <param name="SingleDeparts"></param>
        private void GetEmployeeCollectByDepartment(ref List<V_EMPLOYEEDATADETAIL> EmployeeDatas, ref List<V_EmployeeCollect> ListColls, IQueryable<T_HR_DEPARTMENT> SingleDeparts,DateTime DtStart,DateTime DtEnd)
        {
            
            //离职人数
            if (SingleDeparts.Count() > 0)
            {
                string Deparmentid = SingleDeparts.FirstOrDefault().DEPARTMENTID;
                var LeftOffices = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < DtStart
                                  && ent.OWNERDEPARTMENTID == SingleDeparts.FirstOrDefault().DEPARTMENTID 
                                  && ent.FORMTYPE == "2"
                                  select ent;
                //核心干部人数
                var CadreNumers = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < DtStart
                                  && ent.OWNERDEPARTMENTID == SingleDeparts.FirstOrDefault().DEPARTMENTID
                                  && ent.FORMTYPE != "2"
                                  select ent;
                //在职人数
                var OnPostionEnts = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                    join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                    join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on o.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID

                                    where k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                    && p.OWNERDEPARTMENTID == SingleDeparts.FirstOrDefault().DEPARTMENTID
                                    && p.FORMTYPE != "2" && p.CHANGETIME < DtEnd
                                    //&& m.OWNERCOMPANYID == o.OWNERCOMPANYID && m.CHECKSTATE =="2"
                                    select p;


                var NewEntry = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                               join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                               join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on o.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID

                               where k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                               && k.OWNERDEPARTMENTID == SingleDeparts.FirstOrDefault().DEPARTMENTID
                               && p.CHANGETIME >= DtStart
                               && p.FORMTYPE == "0" && p.CHANGETIME < DtEnd
                               select p;
                //某月离职总人数
                var LeftOfficeTotal = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                      join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                      join k in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE") on o.EMPLOYEEID equals k.EMPLOYEEID

                                      where k.T_HR_LEFTOFFICE.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                      && k.OWNERDEPARTMENTID == SingleDeparts.FirstOrDefault().DEPARTMENTID
                                      && p.CHANGETIME >= DtStart
                                      && p.FORMTYPE == "2" && p.CHANGETIME < DtEnd
                                      select p;
                
                //干部离职人数
                //var ManageLefts = LeftOfficeTotal.Where(p=>p.NEXTPOSTLEVEL );


                V_EmployeeCollect Collect = new V_EmployeeCollect();
                Collect.ORANIZATIONID = Deparmentid;
                Collect.ORGANIZATIONNAME = SingleDeparts.FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                //var Fronts = onPositon.Where(p=>p.NEXTPOSTLEVEL);
                Collect.FRONTNUMERS = 10;
                //var Backs = onPositon.Where(p=>p.NEXTPOSTLEVEL);
                Collect.BACKNUMERS = 10;
                //在职人数=总人数-本月前离职的人数
                if (OnPostionEnts.Count() > 0 && LeftOffices.Count() > 0)
                {
                    Collect.TOTALNUMERS = OnPostionEnts.Count() - LeftOffices.Count();
                }
                ListColls.Add(Collect);

                V_EMPLOYEEDATADETAIL EmployeeNew = new V_EMPLOYEEDATADETAIL();
                EmployeeNew.COLLECTTYPE = EmployeeCollsType.NewEntry.ToString();//新入职
                V_EmployeeCollect Entry = new V_EmployeeCollect();
                Entry.ORANIZATIONID = Deparmentid;
                //前台、后台待定
                Entry.FRONTNUMERS = 10;
                Entry.BACKNUMERS = 10;
                Entry.TOTALNUMERS = NewEntry.Distinct().Count();

                EmployeeNew.LISTEMPLOYEESCOUNT.Add(Entry);
                EmployeeDatas.Add(EmployeeNew);

                V_EMPLOYEEDATADETAIL ONPosition = new V_EMPLOYEEDATADETAIL();
                ONPosition.COLLECTTYPE = EmployeeCollsType.ONPosition.ToString();//在职总人数
                ONPosition.LISTEMPLOYEESCOUNT.Add(Collect);
                EmployeeDatas.Add(ONPosition);

                V_EMPLOYEEDATADETAIL CadreLeftOffice = new V_EMPLOYEEDATADETAIL();
                CadreLeftOffice.COLLECTTYPE = EmployeeCollsType.CadreLeftOffice.ToString();//核心干部离职人数
                V_EmployeeCollect Hexin = new V_EmployeeCollect();
                Hexin.ORANIZATIONID = Deparmentid;
                Hexin.FRONTNUMERS = 3;
                Hexin.BACKNUMERS = 3;
                //var hexinOnpostion = OnPostionEnts.Where(p=>p.NEXTPOSTLEVEL);
                //var hexinLeft = LeftOffices.where();
                Hexin.TOTALNUMERS = 6;
                CadreLeftOffice.LISTEMPLOYEESCOUNT.Add(Collect);
                EmployeeDatas.Add(CadreLeftOffice);


                V_EMPLOYEEDATADETAIL LeftOfficeDetail = new V_EMPLOYEEDATADETAIL();
                LeftOfficeDetail.COLLECTTYPE = EmployeeCollsType.LeftOfficeTotal.ToString();//离职总人数
                V_EmployeeCollect LeftTotal = new V_EmployeeCollect();
                LeftTotal.ORANIZATIONID = Deparmentid;
                //定义好了前台后台
                LeftTotal.FRONTNUMERS = 10;
                LeftTotal.BACKNUMERS = 4;
                if (LeftOfficeTotal.Count() > 0)
                {
                    LeftTotal.TOTALNUMERS = LeftOfficeTotal.Distinct().Count();
                }
                LeftOfficeDetail.LISTEMPLOYEESCOUNT.Add(LeftTotal);
                EmployeeDatas.Add(LeftOfficeDetail);

                V_EMPLOYEEDATADETAIL CadreLeftOfficeRate = new V_EMPLOYEEDATADETAIL();
                CadreLeftOfficeRate.COLLECTTYPE = EmployeeCollsType.CadreLeftOfficeRate.ToString();//核心干部离职总人数
                V_EmployeeCollect CadreRate = new V_EmployeeCollect();
                CadreRate.ORANIZATIONID = Deparmentid;
                CadreRate.FRONTNUMERS = 2;
                CadreRate.BACKNUMERS = 3;
                CadreRate.TOTALNUMERS = 5;
                CadreLeftOfficeRate.LISTEMPLOYEESCOUNT.Add(CadreRate);
                EmployeeDatas.Add(CadreLeftOfficeRate);

                V_EMPLOYEEDATADETAIL LeftOfficeTotalRate = new V_EMPLOYEEDATADETAIL();
                LeftOfficeTotalRate.COLLECTTYPE = EmployeeCollsType.LeftOfficeTotalRate.ToString();//总离职率
                V_EmployeeCollect TotalLeftRate = new V_EmployeeCollect();
                TotalLeftRate.ORANIZATIONID = Deparmentid;
                TotalLeftRate.FRONTNUMERS = 2;
                TotalLeftRate.BACKNUMERS = 3;
                TotalLeftRate.TOTALNUMERS = 5;
                LeftOfficeTotalRate.LISTEMPLOYEESCOUNT.Add(TotalLeftRate);
                EmployeeDatas.Add(LeftOfficeTotalRate);


            }

        }

        public void GetChildOrganzationColls(ref List<V_EmployeeCollect> ListColls, ref List<V_EMPLOYEEDATADETAIL> ListDatas, string IsType, string OrganzationID, List<T_HR_COMPANY> ListCompanys, List<T_HR_DEPARTMENT> ListDeparts,DateTime DtStart,DateTime DtEnd)
        {
            if (IsType == "0")
            {
                var FatherCompany = from ent in dal.GetObjects<T_HR_COMPANY>()
                                    where ent.COMPANYID == OrganzationID
                                    select ent;
                List<string> ListStrCompanyIDs = new List<string>();
                ListCompanys.ForEach(item=>{
                    ListStrCompanyIDs.Add(item.COMPANYID);
                });
                //离职人数
                var LeftOffices = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < DtStart
                                  && ListStrCompanyIDs.Contains(ent.OWNERCOMPANYID)
                                  && ent.FORMTYPE == "2"
                                  select ent;
                //核心干部人数
                var CadreNumers = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < DtStart
                                  && ListStrCompanyIDs.Contains(ent.OWNERCOMPANYID)
                                  && ent.FORMTYPE == "2"
                                  select ent;
                //在职人数
                var OnPostionEnts = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                     join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                     join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on o.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID

                                     where k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                     && ListStrCompanyIDs.Contains(o.OWNERCOMPANYID)
                                     && p.FORMTYPE != "2" && k.ENTRYDATE < DtEnd
                                     //&& m.OWNERCOMPANYID == o.OWNERCOMPANYID && m.CHECKSTATE =="2"
                                     select p;
                for (int i = 0; i < ListCompanys.Count(); i++)
                {
                    T_HR_COMPANY company = new T_HR_COMPANY();
                    company = ListCompanys[i];
                    var ChildCompanys = from ent in ListCompanys
                                        where ent.T_HR_COMPANY2.COMPANYID == ListCompanys[i].COMPANYID
                                        && ent.FATHERTYPE == "0"
                                        select ent;
                    if (ChildCompanys.Count() > 0)
                    {
                        GetChildOrganzationColls(ref ListColls, ref ListDatas, "0", ChildCompanys.ToList()[i].COMPANYID, ChildCompanys.ToList(), null,DtStart,DtEnd);
                    }
                    else
                    {
                        var NewEntry = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                       join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                       join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on o.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID

                                       where k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                       && k.OWNERCOMPANYID == company.COMPANYID
                                       && p.CHANGETIME >= DtStart
                                       && p.FORMTYPE == "0" && p.CHANGETIME < DtEnd
                                       select p;
                        //某月离职总人数
                        var LeftOfficeTotal = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                          join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                          join k in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE") on o.EMPLOYEEID equals k.EMPLOYEEID

                                          where k.T_HR_LEFTOFFICE.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                          && k.OWNERCOMPANYID == company.COMPANYID
                                          && p.CHANGETIME >= DtStart
                                          && p.FORMTYPE == "0" && p.CHANGETIME < DtEnd
                                          select p;
                        var onPositon = OnPostionEnts.Where(p=>p.OWNERCOMPANYID == company.COMPANYID).Distinct();
                        var LeftOffice = LeftOffices.Where(p => p.OWNERCOMPANYID == company.COMPANYID).Distinct();
                        //干部离职人数
                        //var ManageLefts = LeftOfficeTotal.Where(p=>p.NEXTPOSTLEVEL );

                        
                        V_EmployeeCollect Collect = new V_EmployeeCollect();
                        Collect.ORANIZATIONID = company.COMPANYID;
                        Collect.ORGANIZATIONNAME = FatherCompany.FirstOrDefault().BRIEFNAME + "/" + company.BRIEFNAME;
                        //var Fronts = onPositon.Where(p=>p.NEXTPOSTLEVEL);
                        Collect.FRONTNUMERS = 10;
                        //var Backs = onPositon.Where(p=>p.NEXTPOSTLEVEL);
                        Collect.BACKNUMERS = 10;
                        //在职人数=总人数-本月前离职的人数
                        Collect.TOTALNUMERS = onPositon.Count() - LeftOffice.Count();
                        ListColls.Add(Collect);

                        V_EMPLOYEEDATADETAIL EmployeeNew = new V_EMPLOYEEDATADETAIL();
                        EmployeeNew.COLLECTTYPE = EmployeeCollsType.NewEntry.ToString();//新入职
                        V_EmployeeCollect Entry = new V_EmployeeCollect();
                        Entry.ORANIZATIONID = company.COMPANYID;
                        //前台、后台待定
                        Entry.FRONTNUMERS = 10;
                        Entry.BACKNUMERS = 10;
                        Entry.TOTALNUMERS = NewEntry.Distinct().Count();

                        EmployeeNew.LISTEMPLOYEESCOUNT.Add(Entry);
                        ListDatas.Add(EmployeeNew);

                        V_EMPLOYEEDATADETAIL ONPosition = new V_EMPLOYEEDATADETAIL();
                        ONPosition.COLLECTTYPE = EmployeeCollsType.ONPosition.ToString();//在职总人数
                        ONPosition.LISTEMPLOYEESCOUNT.Add(Collect);
                        ListDatas.Add(ONPosition);

                        V_EMPLOYEEDATADETAIL CadreLeftOffice = new V_EMPLOYEEDATADETAIL();
                        CadreLeftOffice.COLLECTTYPE = EmployeeCollsType.CadreLeftOffice.ToString();//核心干部离职人数
                        V_EmployeeCollect Hexin = new V_EmployeeCollect();
                        Hexin.ORANIZATIONID = company.COMPANYID; 
                        Hexin.FRONTNUMERS = 3;
                        Hexin.BACKNUMERS = 3;
                        //var hexinOnpostion = OnPostionEnts.Where(p=>p.NEXTPOSTLEVEL);
                        //var hexinLeft = LeftOffices.where();
                        Hexin.TOTALNUMERS = 6;
                        CadreLeftOffice.LISTEMPLOYEESCOUNT.Add(Collect);
                        ListDatas.Add(CadreLeftOffice);


                        V_EMPLOYEEDATADETAIL LeftOfficeDetail = new V_EMPLOYEEDATADETAIL();
                        LeftOfficeDetail.COLLECTTYPE = EmployeeCollsType.LeftOfficeTotal.ToString();//离职总人数
                        V_EmployeeCollect LeftTotal = new V_EmployeeCollect();
                        LeftTotal.ORANIZATIONID = company.COMPANYID;
                        //定义好了前台后台
                        LeftTotal.FRONTNUMERS = 10;
                        LeftTotal.BACKNUMERS = 4;
                        if (LeftOfficeTotal.Count() > 0)
                        {
                            LeftTotal.TOTALNUMERS = LeftOfficeTotal.Distinct().Count();
                        }
                        LeftOfficeDetail.LISTEMPLOYEESCOUNT.Add(LeftTotal);
                        ListDatas.Add(LeftOfficeDetail);

                        V_EMPLOYEEDATADETAIL CadreLeftOfficeRate = new V_EMPLOYEEDATADETAIL();
                        CadreLeftOfficeRate.COLLECTTYPE = EmployeeCollsType.CadreLeftOfficeRate.ToString();//核心干部离职总人数
                        V_EmployeeCollect CadreRate = new V_EmployeeCollect();
                        CadreRate.ORANIZATIONID = company.COMPANYID;
                        CadreRate.FRONTNUMERS = 2;
                        CadreRate.BACKNUMERS = 3;
                        CadreRate.TOTALNUMERS = 5;
                        CadreLeftOfficeRate.LISTEMPLOYEESCOUNT.Add(CadreRate);
                        ListDatas.Add(CadreLeftOfficeRate);

                        V_EMPLOYEEDATADETAIL LeftOfficeTotalRate = new V_EMPLOYEEDATADETAIL();
                        LeftOfficeTotalRate.COLLECTTYPE = EmployeeCollsType.LeftOfficeTotalRate.ToString();//总离职率
                        V_EmployeeCollect TotalLeftRate = new V_EmployeeCollect();
                        TotalLeftRate.ORANIZATIONID = company.COMPANYID; 
                        TotalLeftRate.FRONTNUMERS = 2;
                        TotalLeftRate.BACKNUMERS = 3;
                        TotalLeftRate.TOTALNUMERS = 5;
                        LeftOfficeTotalRate.LISTEMPLOYEESCOUNT.Add(TotalLeftRate);
                        ListDatas.Add(LeftOfficeTotalRate);

                    }
                }


            }
            if (IsType == "1")
            {
                var FatherDepartment = from ent in dal.GetObjects<T_HR_DEPARTMENT>().Include("T_HR_DEPARTMENTDICTIONARY")

                                       where ent.DEPARTMENTID == OrganzationID && ent.CHECKSTATE == "2"
                                       && ent.EDITSTATE == "1"
                                       select ent;
                List<string> ListStrDepartmentIDs = new List<string>();
                ListDeparts.ForEach(item =>
                {
                    ListStrDepartmentIDs.Add(item.DEPARTMENTID);
                });
                //离职人数
                var LeftOffices = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < DtStart
                                  && ListStrDepartmentIDs.Contains(ent.OWNERDEPARTMENTID)
                                  && ent.FORMTYPE == "2"
                                  select ent;
                //核心干部人数
                var CadreNumers = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < DtStart
                                  && ListStrDepartmentIDs.Contains(ent.OWNERDEPARTMENTID)
                                  && ent.FORMTYPE == "2"
                                  select ent;
                //在职人数
                var OnPostionEnts = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                    join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                    join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on o.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID

                                    where k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                    && ListStrDepartmentIDs.Contains(o.OWNERDEPARTMENTID)
                                    && p.FORMTYPE != "2" && k.ENTRYDATE < DtEnd
                                    //&& m.OWNERCOMPANYID == o.OWNERCOMPANYID && m.CHECKSTATE =="2"
                                    select p;
                for (int i = 0; i < ListDeparts.Count(); i++)
                {
                    T_HR_DEPARTMENT dept = new T_HR_DEPARTMENT();
                    dept = ListDeparts[i];
                    var ChildDeparts = from ent in ListDeparts
                                       where ent.FATHERID == ListDeparts[i].DEPARTMENTID

                                       select ent;
                    if (ChildDeparts.Count() > 0)
                    {
                        GetChildOrganzationColls(ref ListColls, ref ListDatas, "1", ChildDeparts.ToList()[i].DEPARTMENTID, null, ChildDeparts.ToList(),DtStart,DtEnd);
                    }
                    else
                    {

                        var NewEntry = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                       join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                       join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on o.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID

                                       where k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                       && k.OWNERDEPARTMENTID == dept.DEPARTMENTID
                                       && p.CHANGETIME >= DtStart
                                       && p.FORMTYPE == "0" && p.CHANGETIME < DtEnd
                                       select p;
                        //某月离职总人数
                        var LeftOfficeTotal = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                                              join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                                              join k in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE") on o.EMPLOYEEID equals k.EMPLOYEEID

                                              where k.T_HR_LEFTOFFICE.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                              && k.OWNERDEPARTMENTID == dept.DEPARTMENTID
                                              && p.CHANGETIME >= DtStart
                                              && p.FORMTYPE == "0" && p.CHANGETIME < DtEnd
                                              select p;
                        var onPositon = OnPostionEnts.Where(p => p.OWNERDEPARTMENTID == dept.DEPARTMENTID).Distinct();
                        var LeftOffice = LeftOffices.Where(p => p.OWNERDEPARTMENTID == dept.DEPARTMENTID).Distinct();
                        //干部离职人数
                        //var ManageLefts = LeftOfficeTotal.Where(p=>p.NEXTPOSTLEVEL );


                        V_EmployeeCollect Collect = new V_EmployeeCollect();
                        Collect.ORANIZATIONID = dept.DEPARTMENTID;
                        Collect.ORGANIZATIONNAME = FatherDepartment.FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + "/" + dept.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        //var Fronts = onPositon.Where(p=>p.NEXTPOSTLEVEL);
                        Collect.FRONTNUMERS = 10;
                        //var Backs = onPositon.Where(p=>p.NEXTPOSTLEVEL);
                        Collect.BACKNUMERS = 10;
                        //在职人数=总人数-本月前离职的人数
                        Collect.TOTALNUMERS = onPositon.Count() - LeftOffice.Count();
                        ListColls.Add(Collect);

                        V_EMPLOYEEDATADETAIL EmployeeNew = new V_EMPLOYEEDATADETAIL();
                        EmployeeNew.COLLECTTYPE = EmployeeCollsType.NewEntry.ToString();//新入职
                        V_EmployeeCollect Entry = new V_EmployeeCollect();
                        Entry.ORANIZATIONID = dept.DEPARTMENTID;
                        //前台、后台待定
                        Entry.FRONTNUMERS = 10;
                        Entry.BACKNUMERS = 10;
                        Entry.TOTALNUMERS = NewEntry.Distinct().Count();

                        EmployeeNew.LISTEMPLOYEESCOUNT.Add(Entry);
                        ListDatas.Add(EmployeeNew);

                        V_EMPLOYEEDATADETAIL ONPosition = new V_EMPLOYEEDATADETAIL();
                        ONPosition.COLLECTTYPE = EmployeeCollsType.ONPosition.ToString();//在职总人数
                        ONPosition.LISTEMPLOYEESCOUNT.Add(Collect);
                        ListDatas.Add(ONPosition);

                        V_EMPLOYEEDATADETAIL CadreLeftOffice = new V_EMPLOYEEDATADETAIL();
                        CadreLeftOffice.COLLECTTYPE = EmployeeCollsType.CadreLeftOffice.ToString();//核心干部离职人数
                        V_EmployeeCollect Hexin = new V_EmployeeCollect();
                        Hexin.ORANIZATIONID = dept.DEPARTMENTID;
                        Hexin.FRONTNUMERS = 3;
                        Hexin.BACKNUMERS = 3;
                        //var hexinOnpostion = OnPostionEnts.Where(p=>p.NEXTPOSTLEVEL);
                        //var hexinLeft = LeftOffices.where();
                        Hexin.TOTALNUMERS = 6;
                        CadreLeftOffice.LISTEMPLOYEESCOUNT.Add(Collect);
                        ListDatas.Add(CadreLeftOffice);


                        V_EMPLOYEEDATADETAIL LeftOfficeDetail = new V_EMPLOYEEDATADETAIL();
                        LeftOfficeDetail.COLLECTTYPE = EmployeeCollsType.LeftOfficeTotal.ToString();//离职总人数
                        V_EmployeeCollect LeftTotal = new V_EmployeeCollect();
                        LeftTotal.ORANIZATIONID = dept.DEPARTMENTID;
                        //定义好了前台后台
                        LeftTotal.FRONTNUMERS = 10;
                        LeftTotal.BACKNUMERS = 4;
                        if (LeftOfficeTotal.Count() > 0)
                        {
                            LeftTotal.TOTALNUMERS = LeftOfficeTotal.Distinct().Count();
                        }
                        LeftOfficeDetail.LISTEMPLOYEESCOUNT.Add(LeftTotal);
                        ListDatas.Add(LeftOfficeDetail);

                        V_EMPLOYEEDATADETAIL CadreLeftOfficeRate = new V_EMPLOYEEDATADETAIL();
                        CadreLeftOfficeRate.COLLECTTYPE = EmployeeCollsType.CadreLeftOfficeRate.ToString();//核心干部离职总人数
                        V_EmployeeCollect CadreRate = new V_EmployeeCollect();
                        CadreRate.ORANIZATIONID = dept.DEPARTMENTID;
                        CadreRate.FRONTNUMERS = 2;
                        CadreRate.BACKNUMERS = 3;
                        CadreRate.TOTALNUMERS = 5;
                        CadreLeftOfficeRate.LISTEMPLOYEESCOUNT.Add(CadreRate);
                        ListDatas.Add(CadreLeftOfficeRate);

                        V_EMPLOYEEDATADETAIL LeftOfficeTotalRate = new V_EMPLOYEEDATADETAIL();
                        LeftOfficeTotalRate.COLLECTTYPE = EmployeeCollsType.LeftOfficeTotalRate.ToString();//总离职率
                        V_EmployeeCollect TotalLeftRate = new V_EmployeeCollect();
                        TotalLeftRate.ORANIZATIONID = dept.DEPARTMENTID;
                        TotalLeftRate.FRONTNUMERS = 2;
                        TotalLeftRate.BACKNUMERS = 3;
                        TotalLeftRate.TOTALNUMERS = 5;
                        LeftOfficeTotalRate.LISTEMPLOYEESCOUNT.Add(TotalLeftRate);
                        ListDatas.Add(LeftOfficeTotalRate);


                    }
                }
            }
        }

        public byte[] ExportEmployeesCollectReports(string sort, string filterString, IList<object> paras, string userID, string CompanyId, DateTime DtStart,DateTime DtEnd)
        {

            byte[] result = null;
            try
            {

                List<V_EMPLOYEEDATADETAIL> CollectDetails = new List<V_EMPLOYEEDATADETAIL>();
                List<V_EmployeeCollect> employeeInfos = GetEmployeesCollectReports(ref CollectDetails, sort, filterString, paras, userID, CompanyId,DtStart,DtEnd);

                string CompanyName = "";
                string Strdate = "";


                result = OutEmployeeCollectStream(CompanyName, Strdate, employeeInfos, CollectDetails);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeesCollectReports出现错误:" + ex.Message);

            }
            return result;


        }

        /// <summary>
        /// 将数据转为指定html格式，并以流的形式返回，以便导出成指定格式的文件
        /// </summary>
        /// <param name="title"></param>
        /// <param name="dt"></param>
        public static byte[] OutEmployeeCollectStream(string CompanyName, string Strdate, List<V_EmployeeCollect> Collects, List<V_EMPLOYEEDATADETAIL> CollectDetails)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetBodyForCollects(CompanyName, Strdate, Collects, CollectDetails).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }

        /// <summary>
        /// 员工统计
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static StringBuilder GetBodyForCollects(string CompanyName, string Strdate, List<V_EmployeeCollect> Collects, List<V_EMPLOYEEDATADETAIL> CollectDetails)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" border=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\">" + CompanyName + "产业单位" + Strdate + "员工人数汇总表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td>截止时间：</td>");
            s.Append("<td align=left class=\"title\">" + Strdate + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");

            #region 产业单位人数统计


            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td rowspan=\"2\"></td>");
            s.Append("<td align=center class=\"title\">机构名称/部门</td>");
            for (int i = 0; i < Collects.Count; i++)
            {
                s.Append("<td class=\"x1282\" colspan=\"3\">" + Collects[i].ORGANIZATIONNAME + "</td>");
            }
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">人员类别</td>");
            for (int i = 0; i < Collects.Count; i++)
            {
                s.Append("<td align=center class=\"title\">前台</td>");
                s.Append("<td align=center class=\"title\">后台</td>");
                s.Append("<td align=center class=\"title\">合计</td>");
            }
            s.Append("</tr>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">在职人员数量汇总</td>");
            s.Append("<td>" + Strdate + "</td>");
            for (int i = 0; i < Collects.Count; i++)
            {
                s.Append("<td class=\"x1282\">" + Collects[i].FRONTNUMERS + "</td>");
                s.Append("<td class=\"x1282\">" + Collects[i].BACKNUMERS + "</td>");
                s.Append("<td class=\"x1282\">" + Collects[i].TOTALNUMERS + "</td>");
            }
            s.Append("</tr>");
            s.Append("</table>\n\r");
            #endregion
            s.Append("<table ID=\"Table0\" border=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");
            s.Append("<td  align=center class=\"title\">以下为本月数据明细</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");

            #region 本月员工数据明细
            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td rowspan=\"4\">本月人数明细</td>");
            s.Append("<td align=center class=\"title\">新入职人数</td>");
            var NewEntrys = from ent in CollectDetails
                            where ent.COLLECTTYPE == System.Convert.ToInt32(EmployeeCollsType.NewEntry).ToString()
                            select ent;

            if (NewEntrys.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    for (int j = 0; j < NewEntrys.ToList().Count(); j++)
                    {
                        V_EMPLOYEEDATADETAIL detail = NewEntrys.ToList()[j];
                        if (Collects[i].ORANIZATIONID == detail.LISTEMPLOYEESCOUNT.FirstOrDefault().ORANIZATIONID)
                        {
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().FRONTNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().BACKNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().TOTALNUMERS + "</td>");
                        }
                    }

                }

            }
            s.Append("</tr>");


            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">在职总人数</td>");
            var ONPosition = from ent in CollectDetails
                             where ent.COLLECTTYPE == System.Convert.ToInt32(EmployeeCollsType.ONPosition).ToString()
                             select ent;
            if (ONPosition.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    for (int j = 0; j < ONPosition.ToList().Count(); j++)
                    {
                        V_EMPLOYEEDATADETAIL detail = ONPosition.ToList()[j];
                        if (Collects[i].ORANIZATIONID == detail.LISTEMPLOYEESCOUNT.FirstOrDefault().ORANIZATIONID)
                        {
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().FRONTNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().BACKNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().TOTALNUMERS + "</td>");
                        }
                    }

                }

            }
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">核心干部离职人数</td>");
            var CadreLeftOffice = from ent in CollectDetails
                                  where ent.COLLECTTYPE == System.Convert.ToInt32(EmployeeCollsType.CadreLeftOffice).ToString()
                                  select ent;
            if (CadreLeftOffice.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    for (int j = 0; j < CadreLeftOffice.ToList().Count(); j++)
                    {
                        V_EMPLOYEEDATADETAIL detail = CadreLeftOffice.ToList()[j];
                        if (Collects[i].ORANIZATIONID == detail.LISTEMPLOYEESCOUNT.FirstOrDefault().ORANIZATIONID)
                        {
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().FRONTNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().BACKNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().TOTALNUMERS + "</td>");
                        }
                    }

                }

            }
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">离职总人数</td>");
            var LeftOfficeTotal = from ent in CollectDetails
                                  where ent.COLLECTTYPE == System.Convert.ToInt32(EmployeeCollsType.LeftOfficeTotal).ToString()
                                  select ent;
            if (LeftOfficeTotal.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    for (int j = 0; j < LeftOfficeTotal.ToList().Count(); j++)
                    {
                        V_EMPLOYEEDATADETAIL detail = LeftOfficeTotal.ToList()[j];
                        if (Collects[i].ORANIZATIONID == detail.LISTEMPLOYEESCOUNT.FirstOrDefault().ORANIZATIONID)
                        {
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().FRONTNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().BACKNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().TOTALNUMERS + "</td>");
                        }
                    }

                }

            }
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td rowspan=\"2\">离职率</td>");
            s.Append("<td align=center class=\"title\">核心干部离职率</td>");
            var CadreLeftOfficeRate = from ent in CollectDetails
                                      where ent.COLLECTTYPE == System.Convert.ToInt32(EmployeeCollsType.CadreLeftOfficeRate).ToString()
                                      select ent;
            if (CadreLeftOfficeRate.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    for (int j = 0; j < CadreLeftOfficeRate.ToList().Count(); j++)
                    {
                        V_EMPLOYEEDATADETAIL detail = CadreLeftOfficeRate.ToList()[j];
                        if (Collects[i].ORANIZATIONID == detail.LISTEMPLOYEESCOUNT.FirstOrDefault().ORANIZATIONID)
                        {
                            s.Append("<td >" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().FRONTNUMERS + "</td>");
                            s.Append("<td >" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().BACKNUMERS + "</td>");
                            s.Append("<td >" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().TOTALNUMERS + "</td>");
                        }
                    }

                }

            }
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">总离职率</td>");
            var LeftOfficeTotalRate = from ent in CollectDetails
                                      where ent.COLLECTTYPE == System.Convert.ToInt32(EmployeeCollsType.CadreLeftOffice).ToString()
                                      select ent;
            if (LeftOfficeTotalRate.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    for (int j = 0; j < LeftOfficeTotalRate.ToList().Count(); j++)
                    {
                        V_EMPLOYEEDATADETAIL detail = LeftOfficeTotalRate.ToList()[j];
                        if (Collects[i].ORANIZATIONID == detail.LISTEMPLOYEESCOUNT.FirstOrDefault().ORANIZATIONID)
                        {
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().FRONTNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().BACKNUMERS + "</td>");
                            s.Append("<td class=\"x1282\">" + detail.LISTEMPLOYEESCOUNT.FirstOrDefault().TOTALNUMERS + "</td>");
                        }
                    }

                }


            }
            s.Append("</tr>");


            s.Append("</table>");
            #endregion

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
        /// <summary>
        /// 员工汇总类型
        /// </summary>
        public enum EmployeeCollsType
        {
            /// <summary>
            /// 新入职员工
            /// </summary>
            NewEntry,
            /// <summary>
            /// 在职总人数
            /// </summary>
            ONPosition,
            /// <summary>
            /// 核心干部离职人数
            /// </summary>
            CadreLeftOffice,
            /// <summary>
            /// 离职总人数
            /// </summary>
            LeftOfficeTotal,
            /// <summary>
            /// 核心干部离职率
            /// </summary>
            CadreLeftOfficeRate,
            /// <summary>
            /// 总离职率
            /// </summary>
            LeftOfficeTotalRate


        }

        #endregion

        #region 员工结构统计报表


        public byte[] ExportEmployeesTructReports(List<string> companyids, string sort, string filterString, IList<object> paras, string userID, string CompanyId, DateTime DtStart,DateTime DtEnd)
        {

            byte[] result = null;
            try
            {

                List<V_EmployeeTructReports> CollectDetails = new List<V_EmployeeTructReports>();
                List<V_EmployeeTructReports> employeeInfos = GetEmployeesTructReports(companyids, sort, filterString, paras, userID, CompanyId, DtStart,DtEnd);

                string CompanyName = "";
                string StrDate = "";

                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == companyids.FirstOrDefault()
                           select ent;
                
                if (ents.Count() > 0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = DtStart.Year.ToString() + "年" + DtStart.Month.ToString() + "月";
                result = OutEmployeeTructDataStream(CompanyName, StrDate, employeeInfos);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeesCollectReports出现错误:" + ex.Message);

            }
            return result;


        }

        public List<V_EmployeeTructReports> GetEmployeesTructReports(List<string> CompanyIDs, string sort,  string filterString, IList<object> paras, string userID, string CompanyId, DateTime DtStart, DateTime DtEnd)
        {
            List<V_EmployeeTructReports> ListReports = new List<V_EmployeeTructReports>();
            try
            {
                var employees = from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                where CompanyIDs.Contains(ent.OWNERCOMPANYID)
                                select ent;
                
                var LeftOffices = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < DtStart
                                  && CompanyIDs.Contains(ent.OWNERCOMPANYID)
                                  && ent.FORMTYPE == "2"
                                  select ent;
                List<string> LeftEmployees = new List<string>();
                if (LeftOffices != null)
                {
                    LeftOffices.ToList().ForEach(item => {
                        LeftEmployees.Add(item.T_HR_EMPLOYEE.EMPLOYEEID);
                    });
                }

                var diretemployees2 = from o in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                     join p in dal.GetObjects<T_HR_EMPLOYEE>() on o.T_HR_EMPLOYEE.EMPLOYEEID equals p.EMPLOYEEID
                                     join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on p.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID
                                     where k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                                     && CompanyIDs.Contains(o.OWNERCOMPANYID)
                                     && !LeftEmployees.Contains(p.EMPLOYEEID)
                                     && (o.FORMTYPE == "0" || o.FORMTYPE == "1") && k.ENTRYDATE < DtEnd
                                     orderby o.CHANGETIME descending
                                     select o;
                List<string> aa = new List<string>();
                diretemployees2.ToList().ForEach(item =>
                {
                    aa.Add(item.RECORDID);
                });
                var diretemployees = from o in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                     where aa.Contains(o.RECORDID)
                                     select o;
                List<T_HR_EMPLOYEECHANGEHISTORY> OnPostionsEmployee = new List<T_HR_EMPLOYEECHANGEHISTORY>();
                
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                filterString = filterString.Replace("CREATEUSERID","OWNERID");
                if (!string.IsNullOrEmpty(filterString))
                {
                    diretemployees = diretemployees.Where(filterString, queryParas.ToArray());
                }
                diretemployees.ToList().ForEach(item =>
                {
                    var entemployee = from ent in OnPostionsEmployee
                                      where ent.OWNERID == item.OWNERID
                                      && ent.NEXTCOMPANYID == item.NEXTCOMPANYID
                                      && ent.NEXTDEPARTMENTID == item.NEXTDEPARTMENTID
                                      && ent.NEXTPOSTID == item.NEXTPOSTID
                                      orderby ent.CHANGETIME descending
                                      select ent;

                    if (entemployee.Count() == 0)
                    {
                        OnPostionsEmployee.Add(item);
                    }

                    
                });
                if (CompanyIDs.Count() > 0)
                {
                    for (int i = 0; i < CompanyIDs.Count(); i++)
                    {
                        V_EmployeeTructReports report = new V_EmployeeTructReports();
                        string StrCompanyId = CompanyIDs[i];
                        var ents = diretemployees.Where(p => p.OWNERCOMPANYID == StrCompanyId);
                        if (ents != null)
                        {                            
                            report.ONPOSITIONCOUNT = OnPostionsEmployee.Count();
                        }
                        //结婚
                        var Marred = from ent in OnPostionsEmployee                                     
                                     where ent.T_HR_EMPLOYEE.MARRIAGE == "1"
                                     select ent;
                                         
                        //未婚
                        var NoMarr = from ent in  OnPostionsEmployee                                     
                                     where ent.T_HR_EMPLOYEE.MARRIAGE == "0"
                                     select ent;
                                     
                        //var Marred = ents.Where(p => p.MARRIAGE == "1");
                        //var NoMarr = ents.Where(p => p.MARRIAGE == "0");
                        if (Marred != null)
                        {                            
                            report.MARRIEDCOUNT = Marred.Count();//已婚人数
                        }
                        if (NoMarr != null)
                        {                            
                            report.NOMARRYCOUNT = NoMarr.Count();//未婚人数
                        }
                        var SexMans = from ent in OnPostionsEmployee                                     
                                      where ent.T_HR_EMPLOYEE.SEX == "1"
                                      select ent;
                            //OnPostionsEmployee.Where(p => p.T_HR_EMPLOYEE != null && p.T_HR_EMPLOYEE.SEX == "1");
                        var SexWomen = from ent in OnPostionsEmployee                                       
                                       where ent.T_HR_EMPLOYEE.SEX == "0"
                                       select ent;
                        
                        if (SexMans != null)
                        {
                            report.MANCOUNT = SexMans.Count();
                        }
                        if (SexWomen != null)
                        {
                            report.WOMANCOUNT = SexWomen.Count();
                        }
                        var edudazhuan = from ent in OnPostionsEmployee                                         
                                         where ent.T_HR_EMPLOYEE.TOPEDUCATION == "2" || ent.T_HR_EMPLOYEE.TOPEDUCATION =="3"
                                         select ent;
                            
                        var edubenke = from ent in OnPostionsEmployee                                       
                                       where ent.T_HR_EMPLOYEE.TOPEDUCATION == "4"
                                       select ent;
                            
                        var edushuoshi = from ent in OnPostionsEmployee                                         
                                         where ent.T_HR_EMPLOYEE.TOPEDUCATION == "5"
                                         select ent;
                            
                        var eduboshi = from ent in OnPostionsEmployee                                       
                                       where ent.T_HR_EMPLOYEE.TOPEDUCATION == "7"
                                       select ent;
                           
                        report.EDUCATIONASSOCIATECOUNT = edudazhuan != null ? edudazhuan.Count() : 0;
                        report.EDUCATIONUNDERGRADUTECOUNT = edubenke != null ? edubenke.Count() : 0;
                        report.EDUCATIONMASTERCOUNT = edushuoshi != null ? edushuoshi.Count() : 0;
                        report.EDUCATIONDOCTORCOUNT = eduboshi != null ? eduboshi.Count() : 0;

                        #region 统计男女人数
                        DateTime Now = DateTime.Now;
                        
                        
                        OnPostionsEmployee.ToList().ForEach(item =>
                        {
                            //var SingleEmployee = employees.Where(p=>p.EMPLOYEEID == item.OWNERID);
                            //item.T_HR_EMPLOYEE = SingleEmployee.FirstOrDefault();
                            ////DateTime BirDate = (DateTime)SingleEmployee.FirstOrDefault().BIRTHDAY;
                            if (item.T_HR_EMPLOYEE != null)
                            {
                                
                                DateTime BirDate = (DateTime)item.T_HR_EMPLOYEE.BIRTHDAY;
                                int IntAge = Now.Year - BirDate.Year;
                                DateTime BirDateNow = DateTime.Parse(string.Format("{0}-{1}-{2} ", BirDate.Year + IntAge, BirDate.Month, BirDate.Day));
                                DateTime NowDate = DateTime.Parse(string.Format("{0}-{1}-{2} ", Now.Year, Now.Month, Now.Day));
                                if (BirDateNow >= NowDate)
                                    IntAge--;

                                //if (SingleEmployee.FirstOrDefault().SEX == "1")
                                if (item.T_HR_EMPLOYEE.SEX == "1")
                                {
                                    if (IntAge <= 25)
                                    {
                                        report.MANAGETWENTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge <= 35 && IntAge > 25)
                                    {
                                        report.MANAGETWENTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge <= 45 && IntAge > 35)
                                    {
                                        report.MANAGEFORTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge <= 55 && IntAge > 45)
                                    {
                                        report.MANAGEFORTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge > 55)
                                    {
                                        report.MANAGEOVERFIFTYFIVECOUNT += 1;
                                    }
                                }
                                else
                                {
                                    if (IntAge <= 25)
                                    {
                                        report.WOMANAGETWENTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge <= 35 && IntAge > 25)
                                    {
                                        report.WOMANAGETHIRTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge <= 45 && IntAge > 35)
                                    {
                                        report.WOMANAGEFORTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge <= 55 && IntAge > 45)
                                    {
                                        report.WOMANAGEFIFTYFIVECOUNT += 1;
                                    }
                                    else if (IntAge > 55)
                                    {
                                        report.WOMANAGEOVERFIFTYFIVECOUNT += 1;
                                    }
                                }

                                int WorkAge = (int)item.T_HR_EMPLOYEE.WORKINGAGE;
                                if (WorkAge > 11)
                                {
                                    report.WORKAGEOVERTEN += 1;
                                }
                                else if (WorkAge >= 10)
                                {
                                    report.WORKAGETEN += 1;
                                }
                                else if (WorkAge >= 8)
                                {
                                    report.WORKAGEEIGHT += 1;
                                }
                                else if (WorkAge >= 5)
                                {
                                    report.WORKAGEFIVE += 1;
                                }
                                else if (WorkAge >= 3)
                                {
                                    report.WORKAGETHREE += 1;
                                }
                                else if (WorkAge <= 1)
                                {
                                    report.WORKAGEONE += 1;
                                }
                            }

                            #region 统计司龄
                            

                            #endregion


                        });
                        #endregion
                        //管理层需要再确定
                        report.EMPLOYEECOUNT = 12;
                        report.MIDDLEMANAGERCOUNT = 13;
                        report.ADVANCEMANAGERCOUNT = 14;
                        report.LEADERSHIPCOUNT = 2;
                        report.REMARK = "";

                        ListReports.Add(report);



                    }


                }


            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工结构报表出错：ReportsBLL-GetEmployeesTructReports：" + ex.ToString());
            }
            return ListReports;
        }

        /// <summary>
        /// 将数据转为指定html格式，并以流的形式返回，以便导出成指定格式的文件
        /// </summary>
        /// <param name="title"></param>
        /// <param name="dt"></param>
        public static byte[] OutEmployeeTructDataStream(string CompanyName, string Strdate, List<V_EmployeeTructReports> Collects)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeeTructBody(CompanyName, Strdate, Collects).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }

        /// <summary>
        /// 获取公司的员工结构报表
        /// </summary>
        /// <param name="CompanyName">公司名称</param>
        /// <param name="Strdate">时间</param>
        /// <param name="Collects">数据集合</param>
        /// <returns>返回字符串</returns>
        public static StringBuilder GetEmployeeTructBody(string CompanyName, string Strdate, List<V_EmployeeTructReports> Collects)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" border=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\" colspan=\"30\">" + CompanyName + "产业单位" + Strdate + "员工结构统计</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">截止时间：</td>");
            s.Append("<td  align=left class=\"title\" colspan=\"29\">" + Strdate + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");

            #region 产业单位人数统计


            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" rowspan=\"3\">机构名称/部门</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"3\">在编人数小计</td>");
            s.Append("<td align=center class=\"title\" colspan=\"2\">婚姻状况</td>");
            s.Append("<td align=center class=\"title\" colspan=\"4\">学历</td>");
            s.Append("<td align=center class=\"title\" colspan=\"10\">年龄结构</td>");
            s.Append("<td align=center class=\"title\" colspan=\"6\">司龄</td>");
            s.Append("<td align=center class=\"title\" colspan=\"5\">管理层</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"3\">备注</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">男性</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">女性</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">大专及以下</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">本科</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">硕士</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">博士</td>");
            s.Append("<td align=center class=\"title\" colspan=\"2\">25岁以下</td>");
            s.Append("<td align=center class=\"title\" colspan=\"2\">26-35岁</td>");
            s.Append("<td align=center class=\"title\" colspan=\"2\">36-45岁</td>");
            s.Append("<td align=center class=\"title\" colspan=\"2\">46-55岁</td>");
            s.Append("<td align=center class=\"title\" colspan=\"2\">55岁以上</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">1年及1年以内</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">1-3年（含3年）</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">3-5年（含5年）</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">5-8年（含8年）</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">8-10年（不含10年）</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">10年以上</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">员工层</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">基层管理者<br/>(主管级（含）以上，经理级以下）</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">中级管理层<br/>（经理级（含）以上，助理总监级以下）</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">高级管理层<br/>（助理总监级（含）以上，副总经理级以下）</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">领导层<br/>（副总经理级（含）以上）</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">男性</td>");
            s.Append("<td align=center class=\"title\">女性</td>");
            s.Append("<td align=center class=\"title\">男性</td>");
            s.Append("<td align=center class=\"title\">女性</td>");
            s.Append("<td align=center class=\"title\">男性</td>");
            s.Append("<td align=center class=\"title\">女性</td>");
            s.Append("<td align=center class=\"title\">男性</td>");
            s.Append("<td align=center class=\"title\">女性</td>");
            s.Append("<td align=center class=\"title\">男性</td>");
            s.Append("<td align=center class=\"title\">女性</td>");
            s.Append("</tr>");

            s.Append("<tr>");

            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    s.Append("<td class=\"x1282\">" + Collects[i].ORGANIZATIONNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ONPOSITIONCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MARRIEDCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].NOMARRYCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MANCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WOMANCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EDUCATIONASSOCIATECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EDUCATIONUNDERGRADUTECOUNT + "</td>");                    
                    s.Append("<td class=\"x1282\">" + Collects[i].EDUCATIONMASTERCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MANAGETHIRTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WOMANAGETHIRTYFIVECOUNT + "</td>");                    
                    s.Append("<td class=\"x1282\">" + Collects[i].WOMANAGETHIRTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MANAGEFORTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WOMANAGEFORTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MANAGEFIFTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WOMANAGEFIFTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MANAGEOVERFIFTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WOMANAGEOVERFIFTYFIVECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WORKAGEONE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WORKAGETHREE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WORKAGEFIVE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WORKAGEEIGHT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WORKAGETEN + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].WORKAGEOVERTEN + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEECOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PRIMARYMANAGERCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MIDDLEMANAGERCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ADVANCEMANAGERCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LEADERSHIPCOUNT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].REMARK + "</td>");
                }
            }
            s.Append("</tr>");


            s.Append("</table>");
            #endregion

            s.Append("</body></html>");
            return s;
        }
        #endregion


        #region 员工基本信息表
        /// <summary>
        /// 员工基本信息报表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public IQueryable<V_EmployeeBasicInfo> GetEmployeeBasicInfosByCompany(List<string> CompanyIDs, string sort, string filterString, IList<object> paras, string userID, string IsType,DateTime Start,DateTime End)
        {
            List<V_EmployeeBasicInfo> LstBasicInfos = new List<V_EmployeeBasicInfo>();
            IQueryable<V_EmployeeBasicInfo> IQueryEnts = null;
            string StrMessage = "";//记录信息
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                SMT.Foundation.Log.Tracer.Debug("开始获取员工报表信息");
                //获取截至时间内的所有不是离职的员工
                //var Entrys = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("")
                //             where ent.CHANGETIME < End
                //             && CompanyIDs.Contains(ent.OWNERCOMPANYID)
                //             && ent.CHANGETYPE !="2"
                //             select ent;
                //获取该月份前离职的员工
                var LeftOffices = from ent in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>().Include("T_HR_EMPLOYEE")
                                  where ent.CHANGETIME < Start
                                  && CompanyIDs.Contains(ent.OWNERCOMPANYID)
                                  && ent.FORMTYPE == "2"
                                  select ent; 

                var ents = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                           join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                           join k in dal.GetObjects<T_HR_EMPLOYEEENTRY>() on o.EMPLOYEEID equals k.T_HR_EMPLOYEE.EMPLOYEEID
                           join m in dal.GetObjects<T_HR_EMPLOYEECHECK>() on o.EMPLOYEEID equals m.T_HR_EMPLOYEE.EMPLOYEEID into temp
                           from t1 in temp.DefaultIfEmpty()

                           join resume in dal.GetObjects<T_HR_RESUME>() on o.T_HR_RESUME.RESUMEID equals resume.RESUMEID into Resumetemp
                           from t2 in Resumetemp.DefaultIfEmpty()


                           //join contact in dal.GetObjects<T_HR_EMPLOYEECONTRACT>() on o.EMPLOYEEID equals contact.T_HR_EMPLOYEE.EMPLOYEEID into contactTemp
                           //from t3 in contactTemp.DefaultIfEmpty()



                           where //o.EDITSTATE == "1" && o.EMPLOYEESTATE != "2"
                           
                            k.OWNERCOMPANYID == o.OWNERCOMPANYID && k.CHECKSTATE == "2"
                           && CompanyIDs.Contains(p.NEXTCOMPANYID)
                           && p.FORMTYPE !="2" && k.ENTRYDATE < End
                           //&& m.OWNERCOMPANYID == o.OWNERCOMPANYID && m.CHECKSTATE =="2"
                           select new V_EmployeeBasicInfo
                           {
                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               //ORGANIZENAME = p.NEXTCOMPANYNAME + "/" + p.NEXTDEPARTMENTNAME,
                               COMPANYNAME = p.NEXTCOMPANYNAME,
                               DEPARTMENTNAME = p.NEXTDEPARTMENTNAME,
                               SEX = o.SEX == "0" ? "女" : "男",
                               ISMARRY = o.MARRIAGE == "0" ? "未婚" : "已婚",
                               IDNUMBER = o.IDNUMBER,
                               BIRTHDAY = o.BIRTHDAY,
                               POSTNAME = p.NEXTPOSTNAME,
                               ENTRYDATE = k.ENTRYDATE,//入职时间
                               CHANGETIME = p.CHANGETIME,//异动时间
                               BEREGULARDATE = t1.BEREGULARDATE,//转正时间
                               WORKAGE = (decimal)o.WORKINGAGE,//工龄
                               TOPEDUCATION = o.TOPEDUCATION,
                               SPECIALTY = t2.SPECIALTY,//所学专业
                               GRADUATESCHOOL = t2.GRADUATESCHOOL,//毕业院校
                               GRADUATEDATE = DateTime.Now,//毕业时间-----------------需要在简历表中增加字段
                               PROVINCE = o.PROVINCE,
                               REGRESIDENCE = t2.REGRESIDENCE,//户口所在地
                               URGENCYPERSON = o.URGENCYPERSON,
                               URGENCYCONTACT = o.URGENCYCONTACT,
                               MOBILE = o.MOBILE,
                               INTERESTCONTENT = o.INTERESTCONTENT,//兴趣/爱好
                               FAMILYADDRESS = o.FAMILYADDRESS,
                               CREATEUSERID = o.CREATEUSERID,
                               OWNERCOMPANYID = o.OWNERCOMPANYID,
                               OWNERPOSTCOMPANYID = p.OWNERCOMPANYID,
                               OWNERPOSTDEPARTMENT = p.OWNERDEPARTMENTID,
                               OWNERPOSTID = o.OWNERPOSTID,
                               OWNERID = o.OWNERID,
                               OWNERDEPARTMENTID = o.OWNERDEPARTMENTID,
                               //CONTRACTID=null,//合同id
                               FIRSTCONTRACTENDDATE = null,//第一次合同终止时间
                               FIRSTCONTRACTDEADLINE = "",//第一次合同期限
                               SOCIALSERVICEYEAR=o.SOCIALSERVICEYEAR,
                               SECONDCONTRACTENDDATE = null,
                               SECONDCONTRACTDEADLINE = "",//第二次
                               THIRDCONTRACTENDDATE = null,
                               THIIRDCONTRACTDEADLINE = "",//第三次
                               REMARK = o.REMARK,
                               CREATEDATE = p.CREATEDATE,
                               OLDCOMPANYID = p.OLDCOMPANYID,
                               OLDDEPARTMENTID = p.OLDDEPARTMENTID,
                               NEXTCOMPANYID = p.NEXTCOMPANYID,
                               NEXTDEPARTMENTID = p.NEXTDEPARTMENTID
                                                              
                           };

                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEE");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }


                SMT.Foundation.Log.Tracer.Debug("filterString结果为：" + filterString);
                SMT.Foundation.Log.Tracer.Debug("ents 记录数为：" + ents.Count());


                //字典转换
                List<string> StrEductions = new List<string>();
                StrEductions.Add("TOPEDUCATION");//最高学历
                StrEductions.Add("PROVINCE");//籍贯
                SMT.SaaS.BLLCommonServices.PermissionWS.V_Dictionary[] ListDicts = PermClient.GetDictionaryByCategoryArray(StrEductions.ToArray());
                //获取公司信息
                var Companys = from ent in dal.GetObjects<T_HR_COMPANY>().Include("T_HR_COMPANY2")
                               select ent;

                if (ents.Count() > 0)
                {
                    //获取入职信息、学历信息、合同信息
                    ents.ToList().ForEach(item =>
                    {
                        var SingleCompany = Companys.Where(p => p.COMPANYID == item.OWNERCOMPANYID);
                        if (SingleCompany.Count() > 0)
                        {
                            
                            if (SingleCompany.FirstOrDefault().T_HR_COMPANY2 != null)
                            {
                                item.ORGANIZENAME = SingleCompany.FirstOrDefault().T_HR_COMPANY2.BRIEFNAME;
                            }
                            else
                            {
                                item.ORGANIZENAME = "神州通集团";
                            }
                            SMT.Foundation.Log.Tracer.Debug("SingleCompany 记录数为：" + item.ORGANIZENAME);
                        }

                        if (item.OLDCOMPANYID == SingleCompany.FirstOrDefault().COMPANYID)
                        { 
                            //item.COMPANYNAME = 
                        }
                        if (!string.IsNullOrEmpty(item.PROVINCE))
                        {
                            decimal IntPro = Convert.ToDecimal(item.PROVINCE);
                            var Educations = from ent in ListDicts
                                             where ent.DICTIONCATEGORY == "PROVINCE"
                                             && ent.DICTIONARYVALUE == IntPro
                                             select ent;
                            if (Educations.Count() > 0)
                            {
                                item.PROVINCE = Educations.FirstOrDefault().DICTIONARYNAME;
                            }
                            SMT.Foundation.Log.Tracer.Debug("SingleCompany 记录数为：" + item.PROVINCE);
                        }
                        if (!string.IsNullOrEmpty(item.TOPEDUCATION))
                        {
                            decimal IntPro = Convert.ToDecimal(item.TOPEDUCATION);
                            var Educations = from ent in ListDicts
                                             where ent.DICTIONCATEGORY == "TOPEDUCATION"
                                             && ent.DICTIONARYVALUE == IntPro
                                             select ent;
                            if (Educations.Count() > 0)
                            {
                                item.TOPEDUCATION = Educations.FirstOrDefault().DICTIONARYNAME;
                            }
                            SMT.Foundation.Log.Tracer.Debug("SingleCompany 记录数为：" + item.TOPEDUCATION);
                        }
                        //#region 员工合同
                        //var EmployeeContracts = from ent in ents
                        //                        where ent.EMPLOYEEID ==item.EMPLOYEEID
                        //                        && 
                        //                        select ent;

                        //if (EmployeeContracts.Count() > 0)
                        //{
                        //    if (EmployeeContracts.Count() == 1)
                        //    {
                        //        if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                        //        {
                        //            item.FIRSTCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                        //            item.FIRSTCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();
                        //            item.SECONDCONTRACTDEADLINE = "";
                        //            item.SECONDCONTRACTENDDATE = null;
                        //            item.THIIRDCONTRACTDEADLINE = "";
                        //            item.THIRDCONTRACTENDDATE = null;
                        //        }
                        //    }
                        //    int k = 0;
                        //    if (EmployeeContracts.Count() == 2)
                        //    {
                        //        EmployeeContracts.ToList().ForEach(itemcontract =>
                        //        {
                        //            k++;
                        //            if (k == 1)
                        //            {
                        //                if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                        //                {
                        //                    item.FIRSTCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                        //                    item.FIRSTCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                        //                }
                        //            }
                        //            if (k == 2)
                        //            {
                        //                if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                        //                {
                        //                    item.SECONDCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                        //                    item.SECONDCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();
                        //                    item.THIIRDCONTRACTDEADLINE = "";
                        //                    item.THIRDCONTRACTENDDATE = null;
                        //                }
                        //            }
                        //        });

                        //    }
                        //    if (EmployeeContracts.Count() > 2)
                        //    {
                        //        EmployeeContracts.ToList().ForEach(itemcontract =>
                        //        {
                        //            k++;
                        //            if (k == 1)
                        //            {
                        //                if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                        //                {
                        //                    item.FIRSTCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                        //                    item.FIRSTCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                        //                }
                        //            }
                        //            if (k == 2)
                        //            {
                        //                if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                        //                {
                        //                    item.SECONDCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                        //                    item.SECONDCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                        //                }
                        //            }
                        //            if (k == 3)
                        //            {
                        //                if (EmployeeContracts.FirstOrDefault().ENDDATE != null)
                        //                {
                        //                    item.THIRDCONTRACTENDDATE = EmployeeContracts.FirstOrDefault().ENDDATE;
                        //                    item.THIIRDCONTRACTDEADLINE = EmployeeContracts.FirstOrDefault().CONTACTPERIOD.ToString();

                        //                }
                        //            }
                        //        });
                        //    }
                        //}

                        //#endregion
                        var entBAsic = from ent in LstBasicInfos
                                       where ent.EMPLOYEEID == item.EMPLOYEEID
                                       && ent.OWNERCOMPANYID == item.OWNERCOMPANYID
                                       && ent.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                                       && ent.OWNERPOSTID == item.OWNERPOSTID
                                       && CompanyIDs.Contains(ent.OWNERCOMPANYID)
                                       select ent;
                        if (entBAsic.Count() == 0)
                        {
                            SMT.Foundation.Log.Tracer.Debug("entBAsic 记录数为：");
                            var LeftEmployee = LeftOffices.Where(p=>p.T_HR_EMPLOYEE.EMPLOYEEID == item.EMPLOYEEID);
                            if (LeftEmployee.Count() == 0)
                            {
                                //不是离职人员则添加
                                LstBasicInfos.Add(item);
                                SMT.Foundation.Log.Tracer.Debug("LstBasicInfos添加了记录 记录数为：" + LstBasicInfos.Count());
                            }
                        }


                    }

                        );
                }
                IQueryEnts = LstBasicInfos.AsQueryable();
                IQueryEnts = IQueryEnts.OrderBy(sort);
                SMT.Foundation.Log.Tracer.Debug("员工信息报表记录："+IQueryEnts.Count().ToString());
                //IQueryEnts = Utility.Pager<V_EmployeeBasicInfo>(IQueryEnts, pageIndex, pageSize, ref pageCount);

            }
            catch (Exception ex)
            {
                StrMessage = "获取员工基本报表信息出错EmployeeBll-GetEmployeeBasicInfosByCompany" + ex.ToString();
                SMT.Foundation.Log.Tracer.Debug(StrMessage);
            }
            return IQueryEnts != null ? IQueryEnts.Distinct() : null;
        }

        /// <summary>
        /// 导出员工基本信息报表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤字符串</param>
        /// <param name="paras">参数</param>
        /// <param name="pageCount">页数</param>
        /// <param name="userID">用户ID</param>
        /// <param name="IsType">类型  company department</param>
        /// <param name="IsValue">值</param>
        /// <returns></returns>
        public byte[] ExportGetEmployeeBasicInfosByCompany(List<string> companyids,string sort, string filterString, IList<object> paras, string userID, string IsType, DateTime start,DateTime end)
        {

            byte[] result = null;
            try
            {
                //DataTable dt = TableToExportInit();
                List<V_EmployeeBasicInfo> entlist = new List<V_EmployeeBasicInfo>();
                IQueryable<V_EmployeeBasicInfo> employeeInfos =  GetEmployeeBasicInfosByCompany(companyids, sort, filterString, paras, userID, IsType,start,end);
                if (employeeInfos.Count() > 0)
                {
                    entlist = employeeInfos.ToList();
                    //填充社保缴交时间
                    foreach (var c in entlist)
                    {
                        var pensionMaster = GetPensionMasetByEmployeeID(c.OWNERID);
                        if (pensionMaster != null)
                        {
                            c.SOCIALSERVICEYEAR = pensionMaster.SOCIALSERVICEYEAR;
                        }
                    }
                }
                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == companyids.FirstOrDefault()
                           select ent;
                string CompanyName = "";
                string StrDate = "";
                if (ents.Count() > 0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = start.Year.ToString() + "年" + start.Month.ToString() + "月";

                //DataTable dttoExport = GetDataConversion(dt, entlist);

                result = OutEmployeeBasicInfosStream(CompanyName, StrDate, entlist);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工报表信息出错ExportGetEmployeeBasicInfosByCompany:" + ex.Message);

            }
            return result;


        }

        /// <summary>
        /// 获取社保实体
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        private T_HR_PENSIONMASTER GetPensionMasetByEmployeeID(string employeeID)
        {
            T_HR_PENSIONMASTER returnEnt=new T_HR_PENSIONMASTER();
            try
            {
                var pensionEnt = from p in dal.GetObjects<T_HR_PENSIONMASTER>()
                                 where p.OWNERID == employeeID && p.SOCIALSERVICEYEAR!=null
                                 select p;
                if (pensionEnt != null && pensionEnt.Count() > 0)
                {
                    returnEnt = pensionEnt.FirstOrDefault();
                }
                else
                {
                    returnEnt = null;
                }
            }
            catch
            {
                return null;
            }
            return returnEnt;
        }

        public byte[] ExportGetEmployeeBasicInfosByCompany(List<V_EmployeeBasicInfo> ListInfos, DateTime Dt, List<string> companyids)
        {

            byte[] result = null;
            try
            {
                
                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == companyids.FirstOrDefault()
                           select ent;
                string CompanyName = "";
                string StrDate ="";
                if(ents.Count() >0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = Dt.Year.ToString() +"年"+Dt.Month.ToString()+"月";
                //DataTable dttoExport = GetDataConversion(dt, ListInfos);
                if (ListInfos.Count() > 0)
                {
                    //填充社保缴交时间
                    foreach (var c in ListInfos)
                    {
                        var pensionMaster = GetPensionMasetByEmployeeID(c.OWNERID);
                        if (pensionMaster != null)
                        {
                            c.SOCIALSERVICEYEAR = pensionMaster.SOCIALSERVICEYEAR;
                        }
                    }
                }
                result = OutEmployeeBasicInfosStream(CompanyName,StrDate,ListInfos);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工报表信息出错3个参数：ExportGetEmployeeBasicInfosByCompany:" + ex.Message);

            }
            return result;


        }

        public static byte[] OutEmployeeBasicInfosStream(string CompanyName, string Strdate, List<V_EmployeeBasicInfo> EmployeeInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeeBasicInfosBody(CompanyName, Strdate, EmployeeInfos).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }

        /// <summary>
        /// 获取公司的员工结构报表
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="Strdate"></param>
        /// <param name="Collects"></param>
        /// <returns></returns>
        public static StringBuilder GetEmployeeBasicInfosBody(string CompanyName, string Strdate, List<V_EmployeeBasicInfo> Collects)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" border=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\" colspan=\"20\">" + CompanyName + "产业单位" + Strdate + "员工基本信息表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">截止时间：</td>");
            s.Append("<td  align=left class=\"title\" colspan=\"19\">" + Strdate + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");

            #region 产业单位人数统计


            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">序号</td>");
            s.Append("<td align=center class=\"title\" colspan=\"9\">个人主要信息</td>");
            s.Append("<td align=center class=\"title\" colspan=\"4\">个人岗位信息</td>");
            s.Append("<td align=center class=\"title\" colspan=\"4\">个人教育信息</td>");
            s.Append("<td align=center class=\"title\" colspan=\"8\">个人其它信息</td>");
            s.Append("<td align=center class=\"title\" colspan=\"6\">个人合同类</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">备注</td>");
            
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >总公司</td>");
            s.Append("<td align=center class=\"title\" >公司名称</td>");
            s.Append("<td align=center class=\"title\" >部门名称</td>");
            s.Append("<td align=center class=\"title\" >姓名</td>");
            s.Append("<td align=center class=\"title\" >性别</td>");
            s.Append("<td align=center class=\"title\" >出生日期</td>");
            s.Append("<td align=center class=\"title\" >婚姻状况</td>");
            s.Append("<td align=center class=\"title\" >身份证号</td>");
            s.Append("<td align=center class=\"title\" >社保缴交</td>");//luojie 2013/1/25添加
            s.Append("<td align=center class=\"title\" >岗位</td>");
            s.Append("<td align=center class=\"title\" >入职时间</td>");
            s.Append("<td align=center class=\"title\" >转正时间</td>");
            s.Append("<td align=center class=\"title\" >司龄</td>");
            s.Append("<td align=center class=\"title\" >学历</td>");
            s.Append("<td align=center class=\"title\" >毕业时间</td>");
            s.Append("<td align=center class=\"title\" >所学专业</td>");
            s.Append("<td align=center class=\"title\" >毕业院校</td>");
            s.Append("<td align=center class=\"title\" >户籍</td>");
            s.Append("<td align=center class=\"title\" >户籍地址</td>");
            s.Append("<td align=center class=\"title\" >现居住地</td>");
            s.Append("<td align=center class=\"title\" >本人联系电话</td>");
            s.Append("<td align=center class=\"title\" >紧急联系人</td>");
            s.Append("<td align=center class=\"title\" >紧急联系人联系方式</td>");
            s.Append("<td align=center class=\"title\" >特长/兴趣爱好</td>");
            s.Append("<td align=center class=\"title\" >第一次次合同期限</td>");
            s.Append("<td align=center class=\"title\" >第一次合同终止时间</td>");
            s.Append("<td align=center class=\"title\" >第二次合同期限</td>");
            s.Append("<td align=center class=\"title\" >第二次合同终止时间</td>");
            s.Append("<td align=center class=\"title\" >第三次合同期限</td>");
            s.Append("<td align=center class=\"title\" >第三次合同终止时间</td>");
            s.Append("<td align=center class=\"title\" >备注</td>");
            
            s.Append("</tr>");


            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + (i+1).ToString() + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ORGANIZENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].COMPANYNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].DEPARTMENTNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEECNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SEX + "</td>");
                    if (Collects[i].BIRTHDAY != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].BIRTHDAY).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    s.Append("<td class=\"x1282\">" + Collects[i].ISMARRY + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].IDNUMBER + "</td>");
                    //社保缴交时间
                    if (Collects[i].SOCIALSERVICEYEAR != null)
                    {
                        s.Append("<td class=\"x1282\">" + Collects[i].SOCIALSERVICEYEAR + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    s.Append("<td class=\"x1282\">" + Collects[i].POSTNAME + "</td>");
                    if (Collects[i].ENTRYDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].ENTRYDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }

                    if (Collects[i].BEREGULARDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].BEREGULARDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    
                    s.Append("<td class=\"x1282\">" + Collects[i].WORKAGE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].TOPEDUCATION + "</td>");
                    if (Collects[i].GRADUATEDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].GRADUATEDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    
                    s.Append("<td class=\"x1282\">" + Collects[i].SPECIALTY + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].GRADUATESCHOOL + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].PROVINCE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].REGRESIDENCE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].CURRENTADDRESS + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].MOBILE + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].URGENCYPERSON + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].URGENCYCONTACT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].INTERESTCONTENT + "</td>");
                    
                    s.Append("<td class=\"x1282\">" + Collects[i].FIRSTCONTRACTDEADLINE + "</td>");
                    if (Collects[i].FIRSTCONTRACTENDDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].FIRSTCONTRACTENDDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    s.Append("<td class=\"x1282\">" + Collects[i].SECONDCONTRACTDEADLINE + "</td>");
                    if (Collects[i].SECONDCONTRACTENDDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].SECONDCONTRACTENDDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    
                    s.Append("<td class=\"x1282\">" + Collects[i].THIIRDCONTRACTDEADLINE + "</td>");
                    if (Collects[i].THIRDCONTRACTENDDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].THIRDCONTRACTENDDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                                      
                    s.Append("<td class=\"x1282\">" + Collects[i].REMARK + "</td>");
                    s.Append("</tr>");
                }
            }
            


            s.Append("</table>");
            #endregion

            s.Append("</body></html>");
            return s;
        }
        /// <summary>
        /// 供reportbll调用
        /// </summary>
        /// <param name="entlist"></param>
        /// <returns></returns>
        public byte[] ExportGetEmployeeBasicInfosToReportBll(List<V_EmployeeBasicInfo> entlist)
        {

            byte[] result = null;
            try
            {
                DataTable dt = TableToExportInit();

                DataTable dttoExport = GetDataConversion(dt, entlist);

                result = Utility.OutFileStream("导出员工信息表", dttoExport);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportGetEmployeeBasicInfosToReportBll:" + ex.Message);

            }
            return result;


        }





        private DataTable TableToExportInit()
        {
            DataTable dt = new DataTable();

            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = "机构/部门";
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colCordED = new DataColumn();
            colCordED.ColumnName = "姓名";
            colCordED.DataType = typeof(string);
            dt.Columns.Add(colCordED);

            DataColumn colCordFD = new DataColumn();
            colCordFD.ColumnName = "性别";
            colCordFD.DataType = typeof(string);
            dt.Columns.Add(colCordFD);

            DataColumn colBirth = new DataColumn();
            colBirth.ColumnName = "出生日期";
            colBirth.DataType = typeof(string);
            dt.Columns.Add(colBirth);

            DataColumn colMarry = new DataColumn();
            colMarry.ColumnName = "婚姻状况";
            colMarry.DataType = typeof(string);
            dt.Columns.Add(colMarry);

            DataColumn colCard = new DataColumn();
            colCard.ColumnName = "身份证号码";
            colCard.DataType = typeof(string);
            dt.Columns.Add(colCard);

            DataColumn colPost = new DataColumn();
            colPost.ColumnName = "岗位/职务";
            colPost.DataType = typeof(string);
            dt.Columns.Add(colPost);

            DataColumn colEntryDate = new DataColumn();
            colEntryDate.ColumnName = "入职时间";
            colEntryDate.DataType = typeof(string);
            dt.Columns.Add(colEntryDate);

            DataColumn colCheckDate = new DataColumn();
            colCheckDate.ColumnName = "转正时间";
            colCheckDate.DataType = typeof(string);
            dt.Columns.Add(colCheckDate);

            DataColumn colWorkAge = new DataColumn();
            colWorkAge.ColumnName = "司龄";
            colWorkAge.DataType = typeof(string);
            dt.Columns.Add(colWorkAge);

            DataColumn colEduc = new DataColumn();
            colEduc.ColumnName = "学历";
            colEduc.DataType = typeof(string);
            dt.Columns.Add(colEduc);

            DataColumn colGradeDate = new DataColumn();
            colGradeDate.ColumnName = "毕业时间";
            colGradeDate.DataType = typeof(string);
            dt.Columns.Add(colGradeDate);

            DataColumn colSpecial = new DataColumn();
            colSpecial.ColumnName = "所学专业";
            colSpecial.DataType = typeof(string);
            dt.Columns.Add(colSpecial);

            DataColumn colSchool = new DataColumn();
            colSchool.ColumnName = "毕业院校";
            colSchool.DataType = typeof(string);
            dt.Columns.Add(colSchool);

            DataColumn colProvince = new DataColumn();
            colProvince.ColumnName = "籍贯";
            colProvince.DataType = typeof(string);
            dt.Columns.Add(colProvince);

            DataColumn colFamily = new DataColumn();
            colFamily.ColumnName = "户籍地址";
            colFamily.DataType = typeof(string);
            dt.Columns.Add(colFamily);

            DataColumn colAddress = new DataColumn();
            colAddress.ColumnName = "现居住地";
            colAddress.DataType = typeof(string);
            dt.Columns.Add(colAddress);

            DataColumn colLinkerTel = new DataColumn();
            colLinkerTel.ColumnName = "本人联系电话";
            colLinkerTel.DataType = typeof(string);
            dt.Columns.Add(colLinkerTel);

            DataColumn colLinker = new DataColumn();
            colLinker.ColumnName = "紧急联系人";
            colLinker.DataType = typeof(string);
            dt.Columns.Add(colLinker);

            DataColumn colMethod = new DataColumn();
            colMethod.ColumnName = "紧急联系人联系方式";
            colMethod.DataType = typeof(string);
            dt.Columns.Add(colMethod);

            DataColumn colLove = new DataColumn();
            colLove.ColumnName = "特长/兴趣爱好";
            colLove.DataType = typeof(string);
            dt.Columns.Add(colLove);

            DataColumn colContraceOne = new DataColumn();
            colContraceOne.ColumnName = "第一次合同期限";
            colContraceOne.DataType = typeof(string);
            dt.Columns.Add(colContraceOne);

            DataColumn colContractOneEnd = new DataColumn();
            colContractOneEnd.ColumnName = "第一次合同终止时间";
            colContractOneEnd.DataType = typeof(string);
            dt.Columns.Add(colContractOneEnd);

            DataColumn colContraceSec = new DataColumn();
            colContraceSec.ColumnName = "第二次合同期限";
            colContraceSec.DataType = typeof(string);
            dt.Columns.Add(colContraceSec);

            DataColumn colContractOneSec = new DataColumn();
            colContractOneSec.ColumnName = "第二次合同终止时间";
            colContractOneSec.DataType = typeof(string);
            dt.Columns.Add(colContractOneSec);

            DataColumn colContraceThird = new DataColumn();
            colContraceThird.ColumnName = "第三次合同期限";
            colContraceThird.DataType = typeof(string);
            dt.Columns.Add(colContraceThird);

            DataColumn colContractThirdEnd = new DataColumn();
            colContractThirdEnd.ColumnName = "第三次合同终止时间";
            colContractThirdEnd.DataType = typeof(string);
            dt.Columns.Add(colContractThirdEnd);

            DataColumn colTrain = new DataColumn();
            colTrain.ColumnName = "培训协议终止时间";
            colTrain.DataType = typeof(string);
            dt.Columns.Add(colTrain);

            DataColumn colRemark = new DataColumn();
            colRemark.ColumnName = "备注";
            colRemark.DataType = typeof(string);
            dt.Columns.Add(colRemark);

            return dt;
        }

        private DataTable GetDataConversion(DataTable dt, List<V_EmployeeBasicInfo> entlist)
        {
            dt.Rows.Clear();

            for (int i = 0; i < entlist.Count; i++)
            {
                DataRow row = dt.NewRow();
                for (int n = 0; n < dt.Columns.Count; n++)
                {
                    switch (n)
                    {
                        case 0:
                            row[n] = entlist[i].ORGANIZENAME;
                            break;
                        case 1:
                            row[n] = entlist[i].EMPLOYEECNAME;
                            break;
                        case 2:
                            row[n] = entlist[i].SEX;
                            break;
                        case 3:
                            row[n] = entlist[i].BIRTHDAY;
                            break;
                        case 4:
                            row[n] = entlist[i].ISMARRY;
                            break;
                        case 5:
                            row[n] = entlist[i].IDNUMBER;
                            break;
                        case 6:
                            row[n] = entlist[i].POSTNAME;
                            break;
                        case 7:
                            row[n] = entlist[i].ENTRYDATE;
                            break;
                        case 8:
                            row[n] = entlist[i].BEREGULARDATE;
                            break;
                        case 9:
                            row[n] = entlist[i].WORKAGE;
                            break;
                        case 10:
                            row[n] = entlist[i].TOPEDUCATION;
                            break;
                        case 11:
                            row[n] = entlist[i].GRADUATEDATE;
                            break;
                        case 12:
                            row[n] = entlist[i].SPECIALTY;
                            break;
                        case 13:
                            row[n] = entlist[i].GRADUATESCHOOL;
                            break;
                        case 14:
                            row[n] = entlist[i].PROVINCE;
                            break;
                        case 15:
                            row[n] = entlist[i].REGRESIDENCE;
                            break;
                        case 16:
                            row[n] = entlist[i].FAMILYADDRESS;
                            break;
                        case 17:
                            row[n] = entlist[i].MOBILE;
                            break;
                        case 18:
                            row[n] = entlist[i].URGENCYPERSON;
                            break;
                        case 19:
                            row[n] = entlist[i].URGENCYCONTACT;
                            break;
                        case 20:
                            row[n] = entlist[i].INTERESTCONTENT;
                            break;
                        case 21:
                            row[n] = entlist[i].FIRSTCONTRACTDEADLINE;
                            break;
                        case 22:
                            row[n] = entlist[i].FIRSTCONTRACTENDDATE;
                            break;
                        case 23:
                            row[n] = entlist[i].SECONDCONTRACTDEADLINE;
                            break;
                        case 24:
                            row[n] = entlist[i].SECONDCONTRACTENDDATE;
                            break;
                        case 25:
                            row[n] = entlist[i].THIIRDCONTRACTDEADLINE;
                            break;
                        case 26:
                            row[n] = entlist[i].THIRDCONTRACTENDDATE;
                            break;
                        case 27:
                            row[n] = entlist[i].LEARNERSHIPENDDATE;
                            break;
                        case 28:
                            row[n] = entlist[i].REMARK;
                            break;

                    }
                }
                dt.Rows.Add(row);

            }

            return dt;
        }

        #endregion

        #region 员工离职统计
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
        public IQueryable<V_EmployeeLeftOfficeInfos> GetEmployeeLeftOfficeConfirmReports(List<string> companyids,string sort, string filterString, IList<object> paras, string userID, string IsType, DateTime dtStart,DateTime DtEnd)
        {
            List<V_EmployeeLeftOfficeInfos> LstBasicInfos = new List<V_EmployeeLeftOfficeInfos>();
            IQueryable<V_EmployeeLeftOfficeInfos> IQueryEnts = null;
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                var Lstents = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                           join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID
                           join a in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE") on o.EMPLOYEEID equals a.EMPLOYEEID
                           join b in dal.GetObjects<T_HR_LEFTOFFICE>().Include("T_HR_EMPLOYEEPOST") on a.T_HR_LEFTOFFICE.DIMISSIONID equals b.DIMISSIONID 

                           where p.CHANGETIME >= dtStart
                           && p.CHANGETIME < DtEnd
                           && companyids.Contains(o.OWNERCOMPANYID)
                           && p.FORMTYPE == "2" 
                           select new V_EmployeeLeftOfficeInfos
                           {
                               CONFIRMID = p.FORMID,
                               EMPLOYEEID = o.EMPLOYEEID,
                               ORGANIZENAME = "",
                               COMPANYNAME = p.NEXTCOMPANYNAME,
                               DEPARTMENTNAME = p.NEXTDEPARTMENTNAME,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               SEX = o.SEX == "0" ? "女" : "男",
                               IDNUMBER = o.IDNUMBER,
                               POSTNAME = p.NEXTPOSTNAME,
                               LEFTOFFICECATEGORY = b.LEFTOFFICECATEGORY,
                               LEFTOFFICEREASON = b.LEFTOFFICEREASON,
                               LEFTOFFICEDATE = b.LEFTOFFICEDATE,
                               APPLYDATE = b.APPLYDATE,
                               REMARK = b.REMARK,
                               CHECKSTATE = a.CHECKSTATE,
                               OWNERID = a.OWNERID,
                               OWNERPOSTID = a.OWNERPOSTID,
                               OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                               OWNERCOMPANYID = a.OWNERCOMPANYID,
                               CREATEUSERID = a.CREATEUSERID,
                               EMPLOYEEPOSTID = b.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                           }; 

                //var Lstents = from a in dal.GetObjects<T_HR_LEFTOFFICECONFIRM>().Include("T_HR_LEFTOFFICE")
                //              join b in dal.GetObjects<T_HR_LEFTOFFICE>().Include("T_HR_EMPLOYEEPOST") on a.T_HR_LEFTOFFICE.DIMISSIONID equals b.DIMISSIONID
                //              join c in dal.GetObjects<T_HR_EMPLOYEE>() on a.EMPLOYEEID equals c.EMPLOYEEID
                //              select new V_EmployeeLeftOfficeInfos
                //              {
                //                  CONFIRMID = a.CONFIRMID,
                //                  EMPLOYEEID = a.EMPLOYEEID,
                //                  ORGANIZENAME = a.OWNERCOMPANYID,
                //                  EMPLOYEECNAME = c.EMPLOYEECNAME,
                //                  SEX = c.SEX == "0" ? "女" : "男",
                //                  IDNUMBER = c.IDNUMBER,
                //                  POSTNAME = "",
                //                  LEFTOFFICECATEGORY = b.LEFTOFFICECATEGORY,
                //                  LEFTOFFICEREASON = b.LEFTOFFICEREASON,
                //                  LEFTOFFICEDATE = b.LEFTOFFICEDATE,
                //                  APPLYDATE = b.APPLYDATE,
                //                  REMARK = b.REMARK,
                //                  CHECKSTATE = a.CHECKSTATE,
                //                  OWNERID = a.OWNERID,
                //                  OWNERPOSTID = a.OWNERPOSTID,
                //                  OWNERDEPARTMENTID = a.OWNERDEPARTMENTID,
                //                  OWNERCOMPANYID = a.OWNERCOMPANYID,
                //                  CREATEUSERID = a.CREATEUSERID,
                //                  EMPLOYEEPOSTID = b.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID
                //              };
                

                if (!string.IsNullOrWhiteSpace(userID))
                {
                    SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_LEFTOFFICECONFIRM");
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    Lstents = Lstents.Where(filterString, queryParas.ToArray());
                }

                List<string> StrEductions = new List<string>();
                StrEductions.Add("LEFTOFFICECATEGORY");//离职类型

                SMT.SaaS.BLLCommonServices.PermissionWS.V_Dictionary[] ListDicts = PermClient.GetDictionaryByCategoryArray(StrEductions.ToArray());
                Lstents = Lstents.OrderBy(sort);
                //获取公司信息
                var Companys = from ent in dal.GetObjects<T_HR_COMPANY>().Include("T_HR_COMPANY2")
                               select ent;
                if (Lstents.Count() > 0)
                {
                    Lstents.ToList().ForEach(
                        item =>
                        {
                            var SingleCompany = Companys.Where(p => p.COMPANYID == item.OWNERCOMPANYID);
                            if (SingleCompany.Count() > 0)
                            {
                                if (SingleCompany.FirstOrDefault().T_HR_COMPANY2 != null)
                                {
                                    item.ORGANIZENAME = SingleCompany.FirstOrDefault().T_HR_COMPANY2.BRIEFNAME;
                                }
                                else
                                {
                                    item.ORGANIZENAME = "神州通集团";
                                }
                            }

                            
                            if (!string.IsNullOrEmpty(item.LEFTOFFICECATEGORY))
                            {
                                decimal IntPro = Convert.ToDecimal(item.LEFTOFFICECATEGORY);
                                var Educations = from ent in ListDicts
                                                 where ent.DICTIONCATEGORY == "LEFTOFFICECATEGORY"
                                                 && ent.DICTIONARYVALUE == IntPro
                                                 select ent;
                                if (Educations.Count() > 0)
                                {
                                    item.LEFTOFFICECATEGORY = Educations.FirstOrDefault().DICTIONARYNAME;
                                }
                            }

                            LstBasicInfos.Add(item);

                        }

                        );
                }

                IQueryEnts = LstBasicInfos.AsQueryable();
                IQueryEnts = IQueryEnts.OrderBy(sort);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取员工离职报表错误" + ex.ToString());
            }
            return IQueryEnts;
        }

        public byte[] ExportEmployeeLeftOfficeConfirmReports(List<string> companyids, string sort, string filterString, IList<object> paras, string userID, string IsType, DateTime DtStart, DateTime DtEnd)
        {
            byte[] result = null;
            try
            {
                DataTable dt = TableToExportInit();
                List<V_EmployeeLeftOfficeInfos> entlist = new List<V_EmployeeLeftOfficeInfos>();
                IQueryable<V_EmployeeLeftOfficeInfos> employeeInfos = GetEmployeeLeftOfficeConfirmReports(companyids, sort, filterString, paras, userID, IsType, DtStart, DtEnd);
                if (employeeInfos.Count() > 0)
                {
                    entlist = employeeInfos.ToList();
                }
                DataTable dttoExport = GetDataConversion(dt, entlist);

                result = Utility.OutFileStream("导出员工离职表", dttoExport);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeeLeftOfficeConfirmReports:" + ex.Message);

            }
            return result;
        }

        public byte[] ExportEmployeeLeftOfficeConfirmReports(List<string> companyids, List<V_EmployeeLeftOfficeInfos> entlist, DateTime DtStart)
        {
            byte[] result = null;
            try
            {
                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == companyids.FirstOrDefault()
                           select ent;
                string CompanyName = "";
                string StrDate = "";
                if (ents.Count() > 0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = DtStart.Year.ToString() + "年" + DtStart.Month.ToString() + "月";



                result = OutEmployeeLeftOfficeStream(CompanyName, StrDate, entlist);

                //DataTable dt = TableToExportInit();
                
                //DataTable dttoExport = GetDataConversion(dt, entlist);

                //result = Utility.OutFileStream("导出员工离职表", dttoExport);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeeLeftOfficeConfirmReports:" + ex.Message);

            }
            return result;
        }
        /// <summary>
        /// 以二进制形式导出员工离职报表
        /// </summary>
        /// <param name="CompanyName">公司名</param>
        /// <param name="Strdate">时间</param>
        /// <param name="EmployeeInfos">数据集合</param>
        /// <returns>返回二进制流数据</returns>
        public static byte[] OutEmployeeLeftOfficeStream(string CompanyName, string Strdate, List<V_EmployeeLeftOfficeInfos> EmployeeInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeeLeftOfficeBody(CompanyName, Strdate, EmployeeInfos).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }


        /// <summary>
        /// 获取公司的员工异动报表模版
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="Strdate"></param>
        /// <param name="Collects"></param>
        /// <returns></returns>
        public static StringBuilder GetEmployeeLeftOfficeBody(string CompanyName, string Strdate, List<V_EmployeeLeftOfficeInfos> Collects)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" BORDER=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\" colspan=\"12\">" + CompanyName + "产业单位" + Strdate + "员工离职统计表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">截止时间：</td>");
            s.Append("<td  align=left class=\"title\" colspan=\"11\">" + Strdate + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");


            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >序号</td>");
            s.Append("<td align=center class=\"title\" >总公司</td>");
            s.Append("<td align=center class=\"title\" >公司</td>");
            s.Append("<td align=center class=\"title\" >部门</td>");
            s.Append("<td align=center class=\"title\" >姓名</td>");
            s.Append("<td align=center class=\"title\" >性别</td>");
            s.Append("<td align=center class=\"title\" >职务名称</td>");
            s.Append("<td align=center class=\"title\" >身份证号</td>");
            s.Append("<td align=center class=\"title\" >离职日期</td>");
            s.Append("<td align=center class=\"title\" >离职类型</td>");
            s.Append("<td align=center class=\"title\" >离职原因概要</td>");
            
            s.Append("<td align=center class=\"title\">备注</td>");
            s.Append("</tr>");

            


            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + (i + 1).ToString() + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ORGANIZENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].COMPANYNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].DEPARTMENTNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEECNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SEX + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].POSTNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].IDNUMBER + "</td>");
                    if (Collects[i].LEFTOFFICEDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].LEFTOFFICEDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    s.Append("<td class=\"x1282\">" + Collects[i].LEFTOFFICECATEGORY + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].LEFTOFFICEREASON + "</td>");  
                    s.Append("<td class=\"x1282\">" + Collects[i].REMARK + "</td>");
                    s.Append("</tr>");
                }
            }



            s.Append("</table>");

            s.Append("</body></html>");
            return s;
        }




        #region 转换为datatable
        private DataTable GetDataConversion(DataTable dt, List<V_EmployeeLeftOfficeInfos> entlist)
        {
            dt.Rows.Clear();

            for (int i = 0; i < entlist.Count; i++)
            {
                DataRow row = dt.NewRow();
                for (int n = 0; n < dt.Columns.Count; n++)
                {
                    switch (n)
                    {

                        case 0:
                            row[n] = entlist[i].ORGANIZENAME;
                            break;
                        case 1:
                            row[n] = entlist[i].EMPLOYEECNAME;
                            break;
                        case 2:
                            row[n] = entlist[i].SEX;
                            break;
                        case 3:
                            row[n] = entlist[i].POSTNAME;
                            break;
                        case 4:
                            row[n] = entlist[i].IDNUMBER;
                            break;
                        case 5:
                            row[n] = entlist[i].LEFTOFFICEDATE;
                            break;

                        case 6:
                            row[n] = entlist[i].LEFTOFFICECATEGORY;
                            break;
                        case 7:
                            row[n] = entlist[i].LEFTOFFICEREASON;
                            break;
                        case 8:
                            row[n] = entlist[i].REMARK;
                            break;
                    }
                }

                dt.Rows.Add(row);

            }

            return dt;
        }
        #endregion
        #endregion

        #region 员工异动
        #region 员工异动数据
        /// <summary>
        /// 员工岗位异动报表
        /// </summary>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="sort">排序字段</param>
        /// <param name="filterString">过滤条件</param>
        /// <param name="paras">过滤条件中的参数值</param>
        /// <param name="pageCount">返回总页数</param>
        /// <returns>查询结果集</returns>
        public IQueryable<V_EmployeeChangeInfos> EmployeePostReportInfos(List<string> companyids, string sort, string filterString, IList<object> paras, string userID,DateTime DtStart,DateTime DtEnd)
        {
            List<V_EmployeeChangeInfos> LstBasicInfos = new List<V_EmployeeChangeInfos>();
            
            IQueryable<V_EmployeeChangeInfos> IQueryEnts = null;
            
            try
            {
                List<object> queryParas = new List<object>();
                queryParas.AddRange(paras);
                
                var ents = from o in dal.GetObjects<T_HR_EMPLOYEE>()
                           join p in dal.GetObjects<T_HR_EMPLOYEECHANGEHISTORY>() on o.EMPLOYEEID equals p.T_HR_EMPLOYEE.EMPLOYEEID


                           where p.CHANGETIME >= DtStart
                           && p.CHANGETIME < DtEnd
                           && companyids.Contains(o.OWNERCOMPANYID)
                           && (p.FORMTYPE == "1" || p.FORMTYPE == "3")
                           select new V_EmployeeChangeInfos
                           {

                               EMPLOYEEID = o.EMPLOYEEID,
                               EMPLOYEECNAME = o.EMPLOYEECNAME,
                               IDNUMBER = o.IDNUMBER,
                               ORGANIZENAME = "",
                               COMPANYNAME = p.NEXTCOMPANYNAME,
                               DEPARTMENTNAME = p.NEXTDEPARTMENTNAME,
                               POSTNAME = p.NEXTPOSTNAME,
                               SEX = o.SEX == "0" ? "女" : "男",
                               POSTCHANGCATEGORY = p.CHANGETYPE,
                               REMARK = p.CHANGEREASON,
                               INTTOSALARYLEVEL = p.NEXTSALARYLEVEL,
                               INTFROMSALARYLEVEL = p.OLDSALARYLEVEL,
                               FROMCOMPANY = p.OLDCOMPANYNAME,
                               FROMDEPARTMENT = p.OLDDEPARTMENTNAME,
                               FROMPOST = p.OLDPOSTNAME,
                               TOCOMPANY = p.NEXTCOMPANYNAME,
                               TODEPARTMENT = p.NEXTDEPARTMENTNAME,
                               TOPOST = p.NEXTPOSTNAME,
                               STRCHANGEDATE = "",
                               CHANGEDATE = p.CHANGETIME,
                               
                               CREATEUSERID = p.OWNERID,
                               OWNERCOMPANYID = p.OWNERCOMPANYID,
                               OWNERDEPARTMENTID = p.OWNERDEPARTMENTID,
                               OWNERID = p.OWNERID,
                               OWNERPOSTID = p.OWNERPOSTID
                           };
                SetOrganizationFilter(ref filterString, ref queryParas, userID, "T_HR_EMPLOYEEPOSTCHANGE");
                if (!string.IsNullOrEmpty(filterString))
                {
                    ents = ents.Where(filterString, queryParas.ToArray());
                }
                
                List<string> StrEductions = new List<string>();
                StrEductions.Add("SALARYLEVEL");//工资级别
                StrEductions.Add("POSTCHANGCATEGORY");//异动类型
                SMT.SaaS.BLLCommonServices.PermissionWS.V_Dictionary[] ListDicts = PermClient.GetDictionaryByCategoryArray(StrEductions.ToArray());
                //获取公司信息
                var Companys = from ent in dal.GetObjects<T_HR_COMPANY>().Include("T_HR_COMPANY2")
                               select ent;

                if (ents.Count() > 0)
                {
                    ents.ToList().ForEach(item => {
                        //获取上级公司名称
                        var SingleCompany = Companys.Where(p => p.COMPANYID == item.OWNERCOMPANYID);
                        if (SingleCompany.Count() > 0)
                        {
                            if (SingleCompany.FirstOrDefault().T_HR_COMPANY2 != null)
                            {
                                item.ORGANIZENAME = SingleCompany.FirstOrDefault().T_HR_COMPANY2.BRIEFNAME;
                            }
                            else
                            {
                                item.ORGANIZENAME = "神州通集团";
                            }
                        }

                        
                         //获取工资等级   
                        if (!string.IsNullOrEmpty(item.INTFROMSALARYLEVEL))
                        {
                            decimal IntFromLevel = System.Convert.ToDecimal(item.INTFROMSALARYLEVEL);
                            var Educations = from ent in ListDicts
                                                where ent.DICTIONCATEGORY == "SALARYLEVEL"
                                                && ent.DICTIONARYVALUE == IntFromLevel
                                                select ent;
                            if (Educations.Count() > 0)
                            {
                                item.FROMSALARYLEVEL = Educations.FirstOrDefault().DICTIONARYNAME;
                            }
                        }
                        //异动后工资等级
                        if (!string.IsNullOrEmpty(item.INTTOSALARYLEVEL))
                        {
                            decimal IntToLevel = System.Convert.ToDecimal(item.INTTOSALARYLEVEL);
                            var Educations = from ent in ListDicts
                                                where ent.DICTIONCATEGORY == "SALARYLEVEL"
                                                && ent.DICTIONARYVALUE == IntToLevel
                                                select ent;
                            if (Educations.Count() > 0)
                            {
                                item.TOSALARYLEVEL = Educations.FirstOrDefault().DICTIONARYNAME;
                            }
                        }
                        //异动类型
                        if (!string.IsNullOrEmpty(item.POSTCHANGCATEGORY))
                        {
                            decimal IntPro = Convert.ToDecimal(item.POSTCHANGCATEGORY);
                            var Educations = from ent in ListDicts
                                                where ent.DICTIONCATEGORY == "POSTCHANGCATEGORY"
                                                && ent.DICTIONARYVALUE == IntPro
                                                select ent;
                            if (Educations.Count() > 0)
                            {
                                item.POSTCHANGCATEGORY = Educations.FirstOrDefault().DICTIONARYNAME;
                            }
                        }

                        LstBasicInfos.Add(item);
                            
                        
                    });
                }
                
                
                IQueryEnts = LstBasicInfos.AsQueryable();
                
                IQueryEnts = IQueryEnts.OrderBy(sort);
                

                
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("获取员工离职报表错误" + ex.ToString());
            }
            return IQueryEnts;
        }
        #endregion

        #region 导出员工异动报表
        /// <summary>
        /// 导出员工异动报表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        /// <param name="filterString"></param>
        /// <param name="paras"></param>
        /// <param name="pageCount"></param>
        /// <param name="userID"></param>
        /// <param name="CheckState"></param>
        /// <returns></returns>
        public byte[] ExportEmployeePostReportInfos(List<string> companyids, string sort, string filterString, IList<object> paras, string userID, DateTime DtStart,DateTime DtEnd)
        {
            byte[] result = null;
            try
            {
                
                List<V_EmployeeChangeInfos> entlist = new List<V_EmployeeChangeInfos>();
                IQueryable<V_EmployeeChangeInfos> employeeInfos = EmployeePostReportInfos(companyids, sort, filterString, paras, userID, DtStart,DtEnd);
                if (employeeInfos.Count() > 0)
                {
                    entlist = employeeInfos.ToList();
                }
                

                
                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == companyids.FirstOrDefault()
                           select ent;
                string CompanyName = "";
                string StrDate = "";
                if (ents.Count() > 0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = DtStart.Year.ToString() + "年" + DtStart.Month.ToString() + "月";



                result = OutEmployeePostChangeStream(CompanyName, StrDate, entlist);


            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("ExportEmployeePostReportInfos:" + ex.Message);

            }
            return result;
            
        }
        /// <summary>
        /// 导出员工异动报表
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="Strdate"></param>
        /// <param name="EmployeeInfos"></param>
        /// <returns></returns>
        public static byte[] OutEmployeePostChangeStream(string CompanyName, string Strdate, List<V_EmployeeChangeInfos> EmployeeInfos)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.GetHeader().ToString());
            sb.Append(GetEmployeePostChangeBody(CompanyName, Strdate, EmployeeInfos).ToString());
            byte[] by = Encoding.UTF8.GetBytes(sb.ToString());

            return by;
        }

        /// <summary>
        /// 获取公司的员工异动报表模版
        /// </summary>
        /// <param name="CompanyName"></param>
        /// <param name="Strdate"></param>
        /// <param name="Collects"></param>
        /// <returns></returns>
        public static StringBuilder GetEmployeePostChangeBody(string CompanyName, string Strdate, List<V_EmployeeChangeInfos> Collects)
        {
            StringBuilder s = new StringBuilder();

            s.Append("<body>\n\r");
            s.Append("<table ID=\"Table0\" border=1 CELLSPACING=1 CELLPADDING=3 width=100% align=center>\n\r");
            s.Append("<tr>");

            s.Append("<td  align=center class=\"title\" colspan=\"16\">" + CompanyName + "产业单位" + Strdate + "员工异动信息表</td>");
            s.Append("</tr>\n\r");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\">截止时间：</td>");
            s.Append("<td  align=left class=\"title\" colspan=\"15\">" + Strdate + "</td>");
            s.Append("</tr>\n\r");
            s.Append("</table>\n\r");

            
            s.Append("<table border=1 cellspacing=0 CELLPADDING=3 width=100% align=center>");
            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">序号</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">总公司</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">公司</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">部门</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">姓名</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">性别</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">身份证号</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">异动日期</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">异动类型</td>");
            s.Append("<td align=center class=\"title\" colspan=\"3\">异动前</td>");
            s.Append("<td align=center class=\"title\" colspan=\"3\">异动后</td>");
            s.Append("<td align=center class=\"title\" rowspan=\"2\">备注</td>");
            s.Append("</tr>");

            s.Append("<tr>");
            s.Append("<td align=center class=\"title\" >部门</td>");
            s.Append("<td align=center class=\"title\" >岗位名称</td>");
            s.Append("<td align=center class=\"title\" >岗位工资</td>");
            s.Append("<td align=center class=\"title\" >部门</td>");
            s.Append("<td align=center class=\"title\" >岗位名称</td>");
            s.Append("<td align=center class=\"title\" >岗位工资</td>");            
            s.Append("</tr>");


            if (Collects.Count() > 0)
            {
                for (int i = 0; i < Collects.Count; i++)
                {
                    s.Append("<tr>");
                    s.Append("<td class=\"x1282\">" + (i + 1).ToString() + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].ORGANIZENAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].COMPANYNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].DEPARTMENTNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].EMPLOYEECNAME + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].SEX + "</td>");                    
                    s.Append("<td class=\"x1282\">" + Collects[i].IDNUMBER + "</td>");
                    if (Collects[i].CHANGEDATE != null)
                    {
                        s.Append("<td class=\"x1282\">" + ((DateTime)Collects[i].CHANGEDATE).ToString("yyyy-MM-dd") + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    s.Append("<td class=\"x1282\">" + Collects[i].POSTCHANGCATEGORY + "</td>");
                                        
                    s.Append("<td class=\"x1282\">" + Collects[i].FROMDEPARTMENT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].FROMPOST + "</td>");
                    if (Collects[i].INTFROMSALARYLEVEL != null)
                    {
                        s.Append("<td class=\"x1282\">" + Collects[i].INTFROMSALARYLEVEL.ToString() + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    s.Append("<td class=\"x1282\">" + Collects[i].TODEPARTMENT + "</td>");
                    s.Append("<td class=\"x1282\">" + Collects[i].TOPOST + "</td>");
                    if (Collects[i].TOSALARYLEVEL != null)
                    {
                        s.Append("<td class=\"x1282\">" + Collects[i].TOSALARYLEVEL.ToString() + "</td>");
                    }
                    else
                    {
                        s.Append("<td class=\"x1282\"></td>");
                    }
                    
                    s.Append("<td class=\"x1282\">" + Collects[i].REMARK + "</td>");
                    s.Append("</tr>");
                }
            }



            s.Append("</table>");
         
            s.Append("</body></html>");
            return s;
        }



        public byte[] ExportEmployeePostChangeNoQueryReport(List<string> companyids, List<V_EmployeeChangeInfos> ListChanges, DateTime Dt)
        {
            byte[] result = null;
            try
            {
                
                var ents = from ent in dal.GetObjects<T_HR_COMPANY>()
                           where ent.COMPANYID == companyids.FirstOrDefault()
                           select ent;
                string CompanyName = "";
                string StrDate ="";
                if(ents.Count() >0)
                {
                    CompanyName = ents.FirstOrDefault().BRIEFNAME;
                }
                StrDate = Dt.Year.ToString() +"年"+Dt.Month.ToString()+"月";

                result = OutEmployeePostChangeStream(CompanyName,StrDate,ListChanges);

            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug("员工异动报表出现错误-ExportEmployeePostChangeNoQueryReport:" + ex.Message);

            }
            return result;

        }

        

        
        #region 转换为datatable
        private DataTable GetDataConversionForPostChange(DataTable dt, List<V_EmployeeChangeInfos> entlist)
        {
            dt.Rows.Clear();

            for (int i = 0; i < entlist.Count; i++)
            {
                DataRow row = dt.NewRow();
                for (int n = 0; n < dt.Columns.Count; n++)
                {
                    switch (n)
                    {                        
                        case 0:
                            row[n] = entlist[i].ORGANIZENAME;
                            break;
                        case 1:
                            row[n] = entlist[i].EMPLOYEECNAME;
                            break;
                        case 2:
                            row[n] = entlist[i].SEX;
                            break;
                        case 3:
                            row[n] = entlist[i].IDNUMBER;                            
                            break;
                        case 4:
                            row[n] = entlist[i].CHANGEDATE;
                            break;
                        case 5:
                            row[n] = entlist[i].POSTCHANGCATEGORY;
                            break;
                        case 6:
                            row[n] = entlist[i].FROMDEPARTMENT;
                            break;
                        case 7:
                            row[n] = entlist[i].FROMPOST;
                            break;
                        case 8:
                            row[n] = entlist[i].FROMSALARYLEVEL;
                            break;
                        case 9:
                            row[n] = entlist[i].TODEPARTMENT;
                            break;
                        case 10:
                            row[n] = entlist[i].TOPOST;
                            break;
                        case 11:
                            row[n] = entlist[i].TOSALARYLEVEL;
                            break;
                        case 12:
                            row[n] = entlist[i].REMARK;
                            break;
                    }
                }
                dt.Rows.Add(row);

            }

            return dt;
        }
        #endregion

        /// <summary>
        /// 表头
        /// </summary>
        /// <returns></returns>
        private DataTable TableToExportInitForPostChange()
        {
            DataTable dt = new DataTable();
            
            DataColumn colCordSD = new DataColumn();
            colCordSD.ColumnName = "机构/部门";
            colCordSD.DataType = typeof(string);
            dt.Columns.Add(colCordSD);

            DataColumn colCordED = new DataColumn();
            colCordED.ColumnName = "姓名";
            colCordED.DataType = typeof(string);
            dt.Columns.Add(colCordED);

            DataColumn colCordFD = new DataColumn();
            colCordFD.ColumnName = "性别";
            colCordFD.DataType = typeof(string);
            dt.Columns.Add(colCordFD);

            DataColumn colCordAddress = new DataColumn();
            colCordAddress.ColumnName = "身份证号码";
            colCordAddress.DataType = typeof(string);
            dt.Columns.Add(colCordAddress);

            DataColumn colCordBank = new DataColumn();
            colCordBank.ColumnName = "异动日期";
            colCordBank.DataType = typeof(string);
            dt.Columns.Add(colCordBank);

            DataColumn colCordYfxj = new DataColumn();
            colCordYfxj.ColumnName = "异动类型";
            colCordYfxj.DataType = typeof(string);
            dt.Columns.Add(colCordYfxj);            

            DataColumn colCordComments = new DataColumn();
            colCordComments.ColumnName = "异动前所在部门";
            colCordComments.DataType = typeof(string);
            dt.Columns.Add(colCordComments);            

            DataColumn colReason = new DataColumn();
            colReason.ColumnName = "异动前职务名称";
            colReason.DataType = typeof(string);
            dt.Columns.Add(colReason);

            DataColumn colBeforeSal = new DataColumn();
            colBeforeSal.ColumnName = "异动前岗位工资";
            colBeforeSal.DataType = typeof(string);
            dt.Columns.Add(colBeforeSal);

            DataColumn colBackDep = new DataColumn();
            colBackDep.ColumnName = "异动后所在部门";
            colBackDep.DataType = typeof(string);
            dt.Columns.Add(colBackDep);

            DataColumn colBackPost = new DataColumn();
            colBackPost.ColumnName = "异动后职务名称";
            colBackPost.DataType = typeof(string);
            dt.Columns.Add(colBackPost);
            
            DataColumn colBackSal = new DataColumn();
            colBackSal.ColumnName = "异动后岗位工资";
            colBackSal.DataType = typeof(string);
            dt.Columns.Add(colBackSal);

            DataColumn colRemark = new DataColumn();
            colRemark.ColumnName = "备注";
            colRemark.DataType = typeof(string);
            dt.Columns.Add(colRemark);

            return dt;
        }

        #endregion

        #endregion


    }
}
