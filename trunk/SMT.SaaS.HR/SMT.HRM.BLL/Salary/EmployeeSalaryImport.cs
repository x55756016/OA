
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
        public List<T_HR_EMPLOYEESALARYRECORD> ImportEmployeeMonthlySalary(string strPhysicalPath)
        {
            Microsoft.VisualBasic.FileIO.TextFieldParser TF = new Microsoft.VisualBasic.FileIO.TextFieldParser(strPhysicalPath, Encoding.GetEncoding("GB2312"));
            List<T_HR_EMPLOYEESALARYRECORD> balanceList = new List<T_HR_EMPLOYEESALARYRECORD>();
            TF.Delimiters = new string[] { "," }; //设置分隔符
            string[] strLine;
            while (!TF.EndOfData)
            {
                try
                {
                    strLine = TF.ReadFields();
                    string strFingerPrintId = strLine[0];

                    T_HR_EMPLOYEESALARYRECORD entTemp = new T_HR_EMPLOYEESALARYRECORD();
                    entTemp.EMPLOYEECODE = strLine[0];
                    entTemp.EMPLOYEENAME = strLine[1];
                    //entTemp.NEEDATTENDDAYS = GetDecimalValue(strLine[2]);
                    //entTemp.REALATTENDDAYS = GetDecimalValue(strLine[3]);
                    //entTemp.FORGETCARDTIMES = GetDecimalValue(strLine[4]);
                    //entTemp.LATEDAYS = GetDecimalValue(strLine[5]);
                    //entTemp.LATEMINUTES = GetDecimalValue(strLine[6]);
                    //entTemp.LEAVEEARLYDAYS = GetDecimalValue(strLine[7]);
                    //entTemp.ABSENTDAYS = GetDecimalValue(strLine[8]);
                    //entTemp.ABSENTMINUTES = GetDecimalValue(strLine[9]);
                    //entTemp.ANNUALLEVELDAYS = GetDecimalValue(strLine[10]);
                    //entTemp.LEAVEUSEDDAYS = GetDecimalValue(strLine[11]);
                    //entTemp.AFFAIRLEAVEDAYS = GetDecimalValue(strLine[12]);
                    //entTemp.SICKLEAVEDAYS = GetDecimalValue(strLine[13]);
                    //entTemp.MARRYDAYS = GetDecimalValue(strLine[14]);
                    //entTemp.MATERNITYLEAVEDAYS = GetDecimalValue(strLine[15]);
                    //entTemp.NURSESDAYS = GetDecimalValue(strLine[16]);
                    //entTemp.TRIPDAYS = GetDecimalValue(strLine[17]);
                    //entTemp.INJURYLEAVEDAYS = GetDecimalValue(strLine[18]);
                    //entTemp.PRENATALCARELEAVEDAYS = GetDecimalValue(strLine[19]);
                    //entTemp.FUNERALLEAVEDAYS = GetDecimalValue(strLine[20]);
                    //entTemp.EVECTIONTIME = GetDecimalValue(strLine[21]);
                    //entTemp.OUTAPPLYTIME = GetDecimalValue(strLine[22]);

                    balanceList.Add(entTemp);
                }
                catch (Exception ex)
                {
                    Utility.SaveLog(ex.ToString());
                }
            }
            TF.Close();
            return balanceList;
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
