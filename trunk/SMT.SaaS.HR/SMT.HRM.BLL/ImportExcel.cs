using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Office.Interop.Excel;
using SMT_HRM_EFModel;
using SMT.HRM.DAL;
using SMT.HRM.CustomModel;
using System.Linq.Dynamic;
using System.Reflection;
using SMT.Foundation.Log;

namespace SMT.HRM.BLL
{
    public class ImportExcel
    {

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out   int ID);


        public string FileName { get; set; }
        public int WorkSheetIndex { get; set; }

        public T_HR_IMPORTSETMASTER ImportConfig { get; set; }
        public object EntityInstance { get; set; }
        public Dictionary<string, string> paras { get; set; }
        public ImportExcel(string fileName, int workSheetIndex, T_HR_IMPORTSETMASTER importConfig, object entity, Dictionary<string, string> paras)
        {
            FileName = fileName;
            WorkSheetIndex = workSheetIndex;
            ImportConfig = importConfig;

            EntityInstance = entity;
            this.paras = paras;
        }
        public bool StartImport()
        {
            //ThreadStart start = new ThreadStart(ReadExcelData);
            //Thread ImportThread = new Thread(start);
            //ImportThread.Start();
            //ReadExcelData();
            //return true;
            return ReadExcelData();
        }

        public bool ReadExcelData()
        {

            string path = FileName;

            //Application exc = null;
            //Workbooks workBooks = null;
            //Workbook workBook = null;
            //Worksheet workSheet = null;
            //Range r = null;

            // object oMissing = System.Reflection.Missing.Value;
            SMT.Foundation.Core.BaseDAL dal = new SMT.Foundation.Core.BaseDAL();
            try
            {

                dal.BeginTransaction();
                //exc = new Application();
                //exc.UserControl = true;
                //exc.Application.Workbooks.Open(path, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                //workBooks = exc.Workbooks;

                //workBook = workBooks[1];
                //workSheet = (Worksheet)workBook.Worksheets[WorkSheetIndex <= 0 ? 1 : WorkSheetIndex];

                //int colCount = workSheet.UsedRange.Columns.Count; //获得列数
                //int rowCount = workSheet.UsedRange.Rows.Count; //获得行数
                //Dictionary<string, string> dictIndexs = new Dictionary<string, string>();
                //dictIndexs.Add("A", "0");
                //dictIndexs.Add("B", "1");
                //dictIndexs.Add("C", "2");
                //dictIndexs.Add("D", "3");
                //dictIndexs.Add("E", "4");
                //dictIndexs.Add("F", "5");
                //dictIndexs.Add("G", "6");
                //dictIndexs.Add("H", "7");
                //dictIndexs.Add("I", "8");
                //dictIndexs.Add("J", "9");
                //dictIndexs.Add("K", "10");
                //dictIndexs.Add("L", "11");
                //dictIndexs.Add("M", "12");
                //dictIndexs.Add("N", "13");
                //dictIndexs.Add("O", "14");
                //dictIndexs.Add("P", "15");
                //dictIndexs.Add("Q", "16");
                //dictIndexs.Add("R", "17");
                //dictIndexs.Add("S", "18");
                //dictIndexs.Add("T", "19");
                //dictIndexs.Add("U", "20");
                //dictIndexs.Add("V", "21");
                //dictIndexs.Add("W", "22");
                //dictIndexs.Add("X", "23");
                //dictIndexs.Add("Y", "24");
                //dictIndexs.Add("Z", "25");
                Dictionary<string, Int32> dictIndexs = new Dictionary<string, Int32>();
                dictIndexs.Add("A", 0);
                dictIndexs.Add("B", 1);
                dictIndexs.Add("C", 2);
                dictIndexs.Add("D", 3);
                dictIndexs.Add("E", 4);
                dictIndexs.Add("F", 5);
                dictIndexs.Add("G", 6);
                dictIndexs.Add("H", 7);
                dictIndexs.Add("I", 8);
                dictIndexs.Add("J", 9);
                dictIndexs.Add("K", 10);
                dictIndexs.Add("L", 11);
                dictIndexs.Add("M", 12);
                dictIndexs.Add("N", 13);
                dictIndexs.Add("O", 14);
                dictIndexs.Add("P", 15);
                dictIndexs.Add("Q", 16);
                dictIndexs.Add("R", 17);
                dictIndexs.Add("S", 18);
                dictIndexs.Add("T", 19);
                dictIndexs.Add("U", 20);
                dictIndexs.Add("V", 21);
                dictIndexs.Add("W", 22);
                dictIndexs.Add("X", 23);
                dictIndexs.Add("Y", 24);
                dictIndexs.Add("Z", 25);
                //excel读取开始行号
                int beginRow = Convert.ToInt32(ImportConfig.STARTROW.GetValueOrDefault(2));
                //excel读取结束行号
                int endRow = Convert.ToInt32(ImportConfig.ENDROW.GetValueOrDefault(2));

                //if (endRow > rowCount)
                //    endRow = rowCount;
                int i = 1;
                System.Text.UTF8Encoding code = new UTF8Encoding();
                using (StreamReader sr = new StreamReader(path, Encoding.GetEncoding("gb2312")))
                {
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.

                    //while ((line = sr.ReadLine()) != null)
                    while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        string[] lineTmp = line.Split(',');
                        if (i >= beginRow && i <= endRow)
                        {
                            //创建一个实列                
                            Type type = EntityInstance.GetType();
                            object entity = Activator.CreateInstance(type);

                            //得到excel二维数组当前行中A列的值             
                            //r = (Range)workSheet.Cells[i, "A"];
                            //string value = r.Text.ToString().Trim();  

                            #region 实例化对像
                            foreach (T_HR_IMPORTSETDETAIL detail in ImportConfig.T_HR_IMPORTSETDETAIL)
                            {
                                if (!string.IsNullOrEmpty(detail.EXECELCOLUMN))
                                {
                                    //r = (Range)workSheet.Cells[i, detail.EXECELCOLUMN];
                                    ////r = (Range)workSheet.Cells[i, "D"];
                                    //string value = r.Text.ToString().Trim();
                                    int index = 0;
                                    //把列名拆成字符数组
                                    char[] tmps = detail.EXECELCOLUMN.ToUpper().ToCharArray();
                                    if (tmps.Length == 1)
                                    {
                                        //列名是一个字母 直接从字典查出列对应的索引
                                        index = dictIndexs[tmps[0].ToString()];
                                    }
                                    else
                                    {
                                        //列名是双字符  计算出列对应的索引
                                        index = (dictIndexs[tmps[0].ToString()] + 1) * 26 + (dictIndexs[tmps[1].ToString()]);
                                    }
                                    // int index = Convert.ToInt32(dictIndexs[detail.EXECELCOLUMN.ToUpper()]);
                                    string value = lineTmp[index].Trim();
                                    PropertyInfo prop = type.GetProperty(detail.ENTITYCOLUMNCODE);
                                    if (prop != null)
                                    {
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            if (prop.PropertyType.BaseType == typeof(System.ValueType))
                                            {
                                                if (prop.PropertyType.ToString().Contains(typeof(DateTime).ToString()))
                                                {
                                                    prop.SetValue(entity, Convert.ToDateTime(value), null);
                                                }
                                                else
                                                {
                                                    decimal tmpValue;
                                                    decimal.TryParse(value, out tmpValue);
                                                    prop.SetValue(entity, tmpValue, null);
                                                }

                                            }
                                            else
                                            {
                                                prop.SetValue(entity, value, null);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            PropertyInfo prop1 = type.GetProperty("PENSIONDETAILID");
                            prop1.SetValue(entity, Guid.NewGuid().ToString(), null);
                            PropertyInfo prop2 = type.GetProperty("OWNERCOMPANYID");
                            prop2.SetValue(entity, paras["OWNERCOMPANYID"], null);
                            PropertyInfo prop3 = type.GetProperty("OWNERDEPARTMENTID");
                            prop3.SetValue(entity, paras["OWNERDEPARTMENTID"], null);
                            PropertyInfo prop4 = type.GetProperty("OWNERPOSTID");
                            prop4.SetValue(entity, paras["OWNERPOSTID"], null);
                            PropertyInfo prop5 = type.GetProperty("OWNERID");
                            prop5.SetValue(entity, paras["OWNERID"], null);
                            PropertyInfo prop6 = type.GetProperty("CREATEUSERID");
                            prop6.SetValue(entity, paras["CREATEUSERID"], null);
                            PropertyInfo prop7 = type.GetProperty("PENSIONYEAR");
                            prop7.SetValue(entity, Convert.ToDecimal(paras["YEAR"]), null);
                            PropertyInfo prop8 = type.GetProperty("PENSIONMOTH");
                            prop8.SetValue(entity, Convert.ToDecimal(paras["MONTH"]), null);
                            PropertyInfo prop9 = type.GetProperty("CREATEDATE");
                            prop9.SetValue(entity, System.DateTime.Now, null);

                            T_HR_PENSIONDETAIL tmpDetail = entity as T_HR_PENSIONDETAIL;
                            string idnumber = string.Empty;
                            if (string.IsNullOrEmpty(tmpDetail.IDNUMBER))
                            {
                                idnumber = "Null0";
                            }
                            else
                            {
                                idnumber = tmpDetail.IDNUMBER.ToString();
                            }
                            //根据身份证号 查询员工  如果没有找到就不插入此条记录
                            var employee = from e in dal.GetObjects<T_HR_EMPLOYEE>()
                                           where e.IDNUMBER == idnumber && e.EDITSTATE == "1"
                                           select e;
                            if (employee.Count() > 0)
                            {
                                //删除旧的记录
                                var oldDetail = from c in dal.GetObjects<T_HR_PENSIONDETAIL>()
                                                where c.PENSIONMOTH == tmpDetail.PENSIONMOTH && c.PENSIONYEAR == tmpDetail.PENSIONYEAR && c.IDNUMBER == idnumber
                                                select c;
                                if (oldDetail.Count() > 0)
                                {
                                    dal.DeleteFromContext(oldDetail.FirstOrDefault());
                                }

                                T_HR_EMPLOYEE entCurEmp = employee.FirstOrDefault();
                                if (tmpDetail.EMPLOYEEID != entCurEmp.EMPLOYEEID)
                                {
                                    tmpDetail.EMPLOYEEID = entCurEmp.EMPLOYEEID;
                                    entity = tmpDetail;
                                }

                                //插入数据到数据库
                                dal.AddToContext(entity);
                                // dal.Add(entity);
                            }
                            else
                            {

                                Tracer.Debug("PensionImport:" + i.ToString() + "行，没有员工身份证为此号码:" + idnumber);
                            }
                        }
                        i++;
                    }
                }

                dal.SaveContextChanges();
                dal.CommitTransaction();
                CommDal<T_HR_PENSIONDETAIL> cdal = new CommDal<T_HR_PENSIONDETAIL>();
                //                string strSql = @" update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b where a.computerno
                //                                  = b.computerno),a.employeeid= (select employeeid from t_hr_pensionmaster b where a.computerno = b.computerno)
                //                                  ,a.CARDID= (select CARDID from t_hr_pensionmaster b where a.computerno = b.computerno)";
                //string strSql = @" update t_hr_pensiondetail a    set  a.employeeid= (select employeeid from t_hr_employee b where a.idnumber = b.IDNUMBER)";
                //string strSql = " update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b where a.computerno"
                //                +"= b.computerno and b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"'),a.employeeid= (select employeeid from t_hr_pensionmaster b where a.computerno = b.computerno   and b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
                //                +"  ,a.CARDID= (select CARDID from t_hr_pensionmaster b where a.computerno = b.computerno  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
                //                +"  ,a.OWNERID=(select employeeid from t_hr_pensionmaster b where a.computerno = b.computerno  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
                //                +"  ,a.OWNERPOSTID=(select c.OWNERPOSTID from t_hr_pensionmaster b ,t_hr_employee c where a.computerno = b.computerno and b.employeeid= c.employeeid +  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"') "
                //                +"  ,a.OWNERDEPARTMENTID=(select c.OWNERDEPARTMENTID from t_hr_pensionmaster b ,t_hr_employee c where a.computerno = b.computerno and b.employeeid= c.employeeid  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')"
                //                 +" ,a.OWNERCOMPANYID=(select c.OWNERCOMPANYID from t_hr_pensionmaster b ,t_hr_employee c where a.computerno = b.computerno and b.employeeid= c.employeeid  b.ownercompanyid='" + paras["OWNERCOMPANYID"]+"')" 
                //                 +" where a.PENSIONYEAR='" + paras["YEAR"] + "' and a.PENSIONMOTH='" + paras["MONTH"] + "'";
                //cdal.ExecuteCustomerSql(strSql);
                //strSql = "update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b where a.employeeid = b.employeeid),a.cardid =(select b.cardid from t_hr_pensionmaster b where a.employeeid = b.employeeid)";
                string strSql = " update t_hr_pensiondetail a    set  a.pensionmasterid = (select pensionmasterid from t_hr_pensionmaster b,t_hr_employee c where a.IDNUMBER= c.IDNUMBER and b.employeeid=c.employeeid and b.editstate=1)"
                               + "  ,a.employeeid= (select employeeid from t_hr_employee c where a.IDNUMBER = c.IDNUMBER  )"
                               + "  ,a.CARDID= (select CARDID from t_hr_pensionmaster b ,t_hr_employee c where a.IDNUMBER = c.IDNUMBER and b.employeeid=c.employeeid and b.editstate=1)"
                               + "  ,a.OWNERID=(select employeeid from t_hr_employee c where a.IDNUMBER = c.IDNUMBER )"
                               + "  ,a.OWNERPOSTID=(select c.OWNERPOSTID from t_hr_employee c where a.IDNUMBER= c.IDNUMBER ) "
                               + "  ,a.OWNERDEPARTMENTID=(select c.OWNERDEPARTMENTID from t_hr_employee c where a.IDNUMBER= c.IDNUMBER )"
                               + " ,a.OWNERCOMPANYID=(select c.OWNERCOMPANYID from t_hr_employee c where a.IDNUMBER = c.IDNUMBER  )"
                               + " where a.PENSIONYEAR='" + paras["YEAR"] + "' and a.PENSIONMOTH='" + paras["MONTH"] + "' and a.CREATEUSERID='" + paras["CREATEUSERID"] + "'";
                cdal.ExecuteCustomerSql(strSql);
                return true;
            }
            catch (Exception ex)
            {
                //TODO: 写成导入日志
                //throw ex;
                Tracer.Debug("My error" + ex.Message);
                dal.RollbackTransaction();
                return false;
            }
            finally
            {

                //关闭当前的excel进程
                //exc.Application.Workbooks.Close();
                //exc.Quit();
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(exc);
                //IntPtr t = new IntPtr(exc.Hwnd);
                //exc = null;
                //int kid = 0;
                //GetWindowThreadProcessId(t, out kid);
                //System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessById(kid);
                //p.Kill();
            }

        }



        public List<T_HR_PENSIONDETAIL> ReadExcelDataFirst()
        {
            string StrFile = FileName;
            List<T_HR_PENSIONDETAIL> ListDetails = new List<T_HR_PENSIONDETAIL>();
            
            try
            {
                SMT.Foundation.Core.BaseDAL dal = new SMT.Foundation.Core.BaseDAL();                         
                Dictionary<string, Int32> dictIndexs = new Dictionary<string, Int32>();

                ReadExcelColumn(dictIndexs);
                //excel读取开始行号
                int beginRow = Convert.ToInt32(ImportConfig.STARTROW.GetValueOrDefault(2));
                //excel读取结束行号
                int endRow = Convert.ToInt32(ImportConfig.ENDROW.GetValueOrDefault(2));

                
                int i = 1;
                System.Text.UTF8Encoding code = new UTF8Encoding();
                using (StreamReader sr = new StreamReader(StrFile, Encoding.GetEncoding("gb2312")))
                {
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.

                    //while ((line = sr.ReadLine()) != null)
                    while (!string.IsNullOrEmpty(line = sr.ReadLine()))
                    {
                        string[] lineTmp = line.Split(',');
                        if (i >= beginRow && i <= endRow)
                        {
                            //创建一个实列                
                            Type type = EntityInstance.GetType();
                            object entity = Activator.CreateInstance(type);

                            //得到excel二维数组当前行中A列的值             
                            //r = (Range)workSheet.Cells[i, "A"];
                            //string value = r.Text.ToString().Trim();  

                            #region 实例化对像
                            foreach (T_HR_IMPORTSETDETAIL detail in ImportConfig.T_HR_IMPORTSETDETAIL)
                            {
                                if (!string.IsNullOrEmpty(detail.EXECELCOLUMN))
                                {
                                    //r = (Range)workSheet.Cells[i, detail.EXECELCOLUMN];
                                    ////r = (Range)workSheet.Cells[i, "D"];
                                    //string value = r.Text.ToString().Trim();
                                    int index = 0;
                                    //把列名拆成字符数组
                                    char[] tmps = detail.EXECELCOLUMN.ToUpper().ToCharArray();
                                    if (tmps.Length == 1)
                                    {
                                        //列名是一个字母 直接从字典查出列对应的索引
                                        index = dictIndexs[tmps[0].ToString()];
                                    }
                                    else
                                    {
                                        //列名是双字符  计算出列对应的索引
                                        index = (dictIndexs[tmps[0].ToString()] + 1) * 26 + (dictIndexs[tmps[1].ToString()]);
                                    }
                                    // int index = Convert.ToInt32(dictIndexs[detail.EXECELCOLUMN.ToUpper()]);
                                    string value = lineTmp[index].Trim();
                                    PropertyInfo prop = type.GetProperty(detail.ENTITYCOLUMNCODE);
                                    if (prop != null)
                                    {
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            if (prop.PropertyType.BaseType == typeof(System.ValueType))
                                            {
                                                if (prop.PropertyType.ToString().Contains(typeof(DateTime).ToString()))
                                                {
                                                    prop.SetValue(entity, Convert.ToDateTime(value), null);
                                                }
                                                else
                                                {
                                                    decimal tmpValue;
                                                    decimal.TryParse(value, out tmpValue);
                                                    prop.SetValue(entity, tmpValue, null);
                                                }

                                            }
                                            else
                                            {
                                                prop.SetValue(entity, value, null);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            PropertyInfo prop1 = type.GetProperty("PENSIONDETAILID");
                            prop1.SetValue(entity, Guid.NewGuid().ToString(), null);
                            PropertyInfo prop2 = type.GetProperty("OWNERCOMPANYID");
                            prop2.SetValue(entity, paras["OWNERCOMPANYID"], null);
                            PropertyInfo prop3 = type.GetProperty("OWNERDEPARTMENTID");
                            prop3.SetValue(entity, paras["OWNERDEPARTMENTID"], null);
                            PropertyInfo prop4 = type.GetProperty("OWNERPOSTID");
                            prop4.SetValue(entity, paras["OWNERPOSTID"], null);
                            PropertyInfo prop5 = type.GetProperty("OWNERID");
                            prop5.SetValue(entity, paras["OWNERID"], null);
                            PropertyInfo prop6 = type.GetProperty("CREATEUSERID");
                            prop6.SetValue(entity, paras["CREATEUSERID"], null);
                            PropertyInfo prop7 = type.GetProperty("PENSIONYEAR");
                            prop7.SetValue(entity, Convert.ToDecimal(paras["YEAR"]), null);
                            PropertyInfo prop8 = type.GetProperty("PENSIONMOTH");
                            prop8.SetValue(entity, Convert.ToDecimal(paras["MONTH"]), null);
                            PropertyInfo prop9 = type.GetProperty("CREATEDATE");
                            prop9.SetValue(entity, System.DateTime.Now, null);

                            T_HR_PENSIONDETAIL tmpDetail = entity as T_HR_PENSIONDETAIL;
                            string idnumber = string.Empty;
                            if (string.IsNullOrEmpty(tmpDetail.IDNUMBER))
                            {
                                idnumber = "Null0";
                            }
                            else
                            {
                                idnumber = tmpDetail.IDNUMBER.ToString();
                            }
                            //查询员工身份证号和正式员工是否存在
                            var employee = from e in dal.GetObjects<T_HR_EMPLOYEE>()
                                           where e.IDNUMBER == idnumber && e.EDITSTATE == "1"
                                           select e;
                            if (employee.Count() > 0)
                            {                                
                                T_HR_EMPLOYEE entCurEmp = employee.FirstOrDefault();
                                if (tmpDetail.EMPLOYEEID != entCurEmp.EMPLOYEEID)
                                {
                                    tmpDetail.EMPLOYEEID = entCurEmp.EMPLOYEEID;                                    
                                }

                            }
                            else
                            {
                                tmpDetail.IDNUMBER = idnumber +"此身份证号无效或员工已离职";

                                //Tracer.Debug("PensionImport:" + i.ToString() + "行，没有员工身份证为此号码:" + idnumber);
                            }
                            ListDetails.Add(tmpDetail);                            

                        }
                        i++;
                    }
                }

                return ListDetails;
            }
            catch (Exception ex)
            {
                //TODO: 写成导入日志
                //throw ex;
                Tracer.Debug("My error" + ex.Message);
                
                return null;
            }
            
            

        }

        /// <summary>
        /// 设置列的序号
        /// </summary>
        /// <param name="dictIndexs"></param>
        private static void ReadExcelColumn(Dictionary<string, Int32> dictIndexs)
        {
            dictIndexs.Add("A", 0);
            dictIndexs.Add("B", 1);
            dictIndexs.Add("C", 2);
            dictIndexs.Add("D", 3);
            dictIndexs.Add("E", 4);
            dictIndexs.Add("F", 5);
            dictIndexs.Add("G", 6);
            dictIndexs.Add("H", 7);
            dictIndexs.Add("I", 8);
            dictIndexs.Add("J", 9);
            dictIndexs.Add("K", 10);
            dictIndexs.Add("L", 11);
            dictIndexs.Add("M", 12);
            dictIndexs.Add("N", 13);
            dictIndexs.Add("O", 14);
            dictIndexs.Add("P", 15);
            dictIndexs.Add("Q", 16);
            dictIndexs.Add("R", 17);
            dictIndexs.Add("S", 18);
            dictIndexs.Add("T", 19);
            dictIndexs.Add("U", 20);
            dictIndexs.Add("V", 21);
            dictIndexs.Add("W", 22);
            dictIndexs.Add("X", 23);
            dictIndexs.Add("Y", 24);
            dictIndexs.Add("Z", 25);
        }


    }
}
