
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
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public class EmployeeSalaryImportBLL : BaseBll<T_HR_EMPLOYEESALARYRECORD>
    {
        /// <summary>
        /// 从Excel读取数据
        /// </summary>
        /// <param name="strPhysicalPath"></param>
        /// <returns></returns>
       public string ImportEmployeeMonthlySalary(GenerateUserInfo GenerateUser,string strPhysicalPath, string year, string month, string owerCompayId, ref string UImsg)
        {
            string msg = string.Empty;
            Microsoft.VisualBasic.FileIO.TextFieldParser TF = new Microsoft.VisualBasic.FileIO.TextFieldParser(strPhysicalPath, Encoding.GetEncoding("GB2312"));
          
            TF.Delimiters = new string[] { "," }; //设置分隔符
            string[] strLine;
            while (!TF.EndOfData)
            {
                try
                {
                    strLine = TF.ReadFields();
                    string strIDNUMBER = strLine[0];//身份证号码
                    string strEmployeeName = strLine[1];//员工姓名
                    string strSalaryItemName = strLine[2];//员工薪资项目名
                    string strSalaryItemSUM = strLine[3];//员工薪资项目金额

                    var employee = (from ent in dal.GetObjects<T_HR_EMPLOYEE>()
                                        where ent.IDNUMBER == strIDNUMBER
                                        select ent).FirstOrDefault();
                    if (employee == null)
                    {
                        msg = "薪资项目导入，根据身份证未找到相关员工档案,身份证号码：" + strIDNUMBER;
                        UImsg += msg + System.Environment.NewLine;
                        Tracer.Debug(msg);
                        continue;
                    }
                    else
                    {
                        Tracer.Debug("薪资项目导入，根据身份证找到相关员工档案,身份证号码：" + strIDNUMBER
                            + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                            + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM);
                    }
                    T_HR_EMPLOYEESALARYRECORD record = (from r in dal.GetTable()
                                                        where r.EMPLOYEEID == employee.EMPLOYEEID
                                                        && r.SALARYMONTH == month && r.SALARYYEAR == year
                                                        && r.OWNERCOMPANYID == owerCompayId
                                                        select r).FirstOrDefault();
                    #region 新建薪资项目
                    if (record == null)
                    {
                        try
                        {
                            Tracer.Debug("导入员工薪资项目，未生成员工月薪，开始生成，员工姓名：" + employee.EMPLOYEECNAME + " " + year + "年" + month + "月"
                                + "的月薪，record == null" + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                            + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM);
                            EmployeeSalaryRecordBLL bll = new EmployeeSalaryRecordBLL();
                            var salaryAchive = bll.GetEmployeeAcitiveSalaryArchive(employee.EMPLOYEEID, GenerateUser.GeneratePostId, GenerateUser.GenerateCompanyId, int.Parse(year), int.Parse(month));
                            record = new T_HR_EMPLOYEESALARYRECORD();
                            record.EMPLOYEESALARYRECORDID = Guid.NewGuid().ToString();
                            record.EMPLOYEEID = employee.EMPLOYEEID;
                            record.CREATEUSERID = GenerateUser.GenerateUserId;
                            record.CREATEPOSTID = GenerateUser.GeneratePostId;
                            //  record.CREATEPOSTID = archive.CREATEPOSTID;
                            record.CREATEDEPARTMENTID = GenerateUser.GenerateDepartmentId;
                            record.CREATECOMPANYID = GenerateUser.GenerateCompanyId;

                            record.OWNERID = employee.EMPLOYEEID; ;
                            record.OWNERPOSTID = employee.OWNERPOSTID;
                            record.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                            record.OWNERCOMPANYID = employee.OWNERCOMPANYID;

                            record.ATTENDANCEUNUSUALTIMES = employee.OWNERCOMPANYID;//记录薪资人的所属公司
                            record.ATTENDANCEUNUSUALTIME = employee.OWNERPOSTID;//记录薪资人的所属岗位
                            record.ABSENTTIMES = salaryAchive.SALARYARCHIVEID;//记录生成薪资使用的薪资档案

                            record.PAIDTYPE = bll.GetPaidType(employee.EMPLOYEEID);  //发放方式
                            record.REMARK = "薪资项目导入生成";
                            record.PAIDTYPE = bll.GetPaidType(employee.EMPLOYEEID);
                            record.EMPLOYEECODE = salaryAchive.EMPLOYEECODE;
                            record.EMPLOYEENAME = salaryAchive.EMPLOYEENAME;
                            record.PAYCONFIRM = "0";
                            record.CHECKSTATE = "0";
                            record.CREATEDATE = System.DateTime.Now;
                            record.SALARYYEAR = year;
                            record.SALARYMONTH = month;
                            record.T_HR_EMPLOYEESALARYRECORDITEM.Clear(); 
                            int i = dal.Add(record);
                            if (i == 1)
                            {
                                T_HR_EMPLOYEESALARYRECORDITEM SalaryItem = new T_HR_EMPLOYEESALARYRECORDITEM();
                                SalaryItem.SALARYRECORDITEMID = Guid.NewGuid().ToString();
                                SalaryItem.T_HR_EMPLOYEESALARYRECORD = record;
                                var item = (from ent in dal.GetObjects<T_HR_SALARYARCHIVEITEM>()
                                            join entb in dal.GetObjects<T_HR_SALARYITEM>()
                                           on ent.SALARYITEMID equals entb.SALARYITEMID
                                            where entb.SALARYITEMNAME == strSalaryItemName
                                            && ent.T_HR_SALARYARCHIVE.SALARYARCHIVEID == salaryAchive.SALARYARCHIVEID
                                            select ent).FirstOrDefault();

                                if (item != null)
                                {
                                    SalaryItem.SALARYITEMID = item.SALARYITEMID;
                                    SalaryItem.SALARYARCHIVEID = salaryAchive.SALARYARCHIVEID;
                                    SalaryItem.SALARYSTANDARDID = item.SALARYSTANDARDID;
                                    SalaryItem.ORDERNUMBER = item.ORDERNUMBER;
                                    SalaryItem.SUM = AES.AESEncrypt(strSalaryItemSUM);
                                    SalaryItem.REMARK = "文件导入";
                                    SalaryItem.CREATEDATE = System.DateTime.Now;
                                    int iadd = dal.Add(record);

                                    if (iadd >= 1)
                                    {
                                        msg = "薪资项目导入成功，受影响的条数：" + i + " 身份证号码：" + strIDNUMBER
                                               + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                               + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                        UImsg += msg + System.Environment.NewLine;
                                        Tracer.Debug(msg);
                                    }
                                    else
                                    {
                                        msg = "薪资项目导入失败，受影响的条数：" + i + " 身份证号码：" + strIDNUMBER
                                               + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                               + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                        UImsg += msg + System.Environment.NewLine;
                                        Tracer.Debug(msg);
                                    }

                                }
                                else
                                {
                                    msg = "薪资项目导入在公司薪资项目或薪资档案中未找到相关薪资项目！" + " 身份证号码：" + strIDNUMBER
                                               + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                               + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                    UImsg += msg + System.Environment.NewLine;
                                    Tracer.Debug(msg);
                                }
                            }
                            else
                            {
                                msg = "薪资项目导入添加员工薪资记录失败！" + " 身份证号码：" + strIDNUMBER
                                               + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                               + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                UImsg += msg + System.Environment.NewLine;
                                Tracer.Debug(msg);
                            }

                        }
                        catch (Exception ex)
                        {
                            msg = "薪资项目导入添加员工薪资记录失败！" +ex.ToString()+ " 身份证号码：" + strIDNUMBER
                                              + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                              + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                            UImsg += msg + System.Environment.NewLine;
                            Tracer.Debug(msg);
                        }
;
                    }
                    #endregion
                    #region 更新薪资项目
                    else
                    {
                        var SalaryRecord = (from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                            join b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>() on a.EMPLOYEESALARYRECORDID equals b.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID
                                            join c in dal.GetObjects<T_HR_SALARYITEM>() on b.SALARYITEMID equals c.SALARYITEMID
                                            where a.EMPLOYEEID == employee.EMPLOYEEID
                                            && a.SALARYYEAR == year
                                            && a.SALARYMONTH == month
                                            && a.OWNERCOMPANYID == owerCompayId
                                            && a.CHECKSTATE=="0"
                                            select new
                                            {
                                                b.SALARYRECORDITEMID,
                                                c.SALARYITEMNAME,
                                                b.SUM
                                            });

                        foreach (var item in SalaryRecord)
                        {
                            if (item.SALARYITEMNAME == strSalaryItemName)
                            {
                                var q = from ent in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>()
                                        where ent.SALARYRECORDITEMID == item.SALARYRECORDITEMID                                        
                                        select ent;
                                var entsum = q.FirstOrDefault();
                                if (entsum != null)
                                {
                                    entsum.SUM = AES.AESEncrypt(strSalaryItemSUM);
                                    int i = dal.Update(entsum);
                                    if (i >= 1)
                                    {
                                        msg = "薪资项目导入成功，受影响的条数：" + i + " 身份证号码：" + strIDNUMBER
                                               + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                               + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                        UImsg += msg + System.Environment.NewLine;
                                        Tracer.Debug(msg);
                                    }
                                    else
                                    {
                                        msg = "薪资项目导入失败，受影响的条数：" + i + " 身份证号码：" + strIDNUMBER
                                               + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                               + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                        UImsg += msg + System.Environment.NewLine;
                                        Tracer.Debug(msg);
                                        continue;
                                    }
                                }
                                else
                                {
                                    msg = "薪资项目导入失败，未找到薪资项目," + " 身份证号码：" + strIDNUMBER
                                            + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                            + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                    UImsg += msg + System.Environment.NewLine;
                                    Tracer.Debug(msg);
                                    continue;
                                }
                            }
                            else
                            {
                                var q = from ent in SalaryRecord
                                        where ent.SALARYITEMNAME == strSalaryItemName
                                        select ent;
                                if (q.Count() == 0)
                                {
                                    msg = "薪资项目导入失败，请检查薪资项目名称是否正确， 身份证号码：" + strIDNUMBER
                                                   + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                                                   + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                                    UImsg += msg + System.Environment.NewLine;
                                    Tracer.Debug(msg);
                                    continue;
                                }
                            }
                        }

                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    UImsg += "导入薪资项目异常：" + ex + System.Environment.NewLine;
                    Utility.SaveLog(ex.ToString());
                    continue;
                }
            }
            TF.Close();
            return UImsg;
        }

        /// <summary>
        /// 转化字符串为浮点数,且最多保留小数点后两位
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private decimal GetDecimalValue(string p)
        {
            decimal dRes = 0;
            decimal.TryParse(p, out dRes);
            dRes = decimal.Round(dRes, 2);
            return dRes;
        }
    }
}
