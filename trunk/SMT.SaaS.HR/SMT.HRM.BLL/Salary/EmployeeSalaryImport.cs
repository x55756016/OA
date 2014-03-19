
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
        public string ImportEmployeeMonthlySalary(string strPhysicalPath,string year,string month,string owerCompayId,ref string UImsg)
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
                    if (record == null)
                    {
                        msg = "请先生成员工薪资记录后再导入薪资项目," + " 导入的员工姓名：" + strEmployeeName + " 根据身份证号查到的员工姓名：" + employee.EMPLOYEECNAME
                            + " 薪资项目名：" + strSalaryItemName + " 薪资项目金额：" + strSalaryItemSUM;
                        UImsg += msg + System.Environment.NewLine;
                        Tracer.Debug(msg);
                        continue;
;
                    }
                    else
                    {
                        var SalaryRecord = (from a in dal.GetObjects<T_HR_EMPLOYEESALARYRECORD>()
                                            join b in dal.GetObjects<T_HR_EMPLOYEESALARYRECORDITEM>() on a.EMPLOYEESALARYRECORDID equals b.T_HR_EMPLOYEESALARYRECORD.EMPLOYEESALARYRECORDID
                                            join c in dal.GetObjects<T_HR_SALARYITEM>() on b.SALARYITEMID equals c.SALARYITEMID
                                            where a.EMPLOYEEID == employee.EMPLOYEEID
                                            && a.SALARYYEAR == year
                                            && a.SALARYMONTH == month
                                            && a.OWNERCOMPANYID == owerCompayId
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
                                    entsum.SUM = strSalaryItemSUM;
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
