using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.HRM.UI
{
    public class ExportToCSV
    {
         #region 001----将DataSet转换成CSV文件
        //public static void Export2CSV(DataGrid dtGrid, IEnumerable ItemsSource,string fileName)
        //{ 
        //    string csvStr = ConverDataSet2CSV(dtGrid,ItemsSource);
        //    if(csvStr=="") return;
        //    FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
        //    //将string转换成byte[]
        //    byte[] csvArray = System.Text.Encoding.UTF8.GetBytes(csvStr.ToCharArray(), 0, csvStr.Length - 1);
        //    fs.Write(csvArray,0,csvStr.Length - 1);
        //    fs.Close();
        //    fs = null;
        //}

        ///// <summary>
        ///// 将指定的数据集中指定的表转换成CSV字符串
        ///// </summary>
        ///// <param name="dataGrid"></param>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //private static string ConverDataSet2CSV(DataGrid dataGrid, IEnumerable ItemsSource)
        //{
        //    //首先判断数据集中是否包含指定的表
        //    if (dataGrid == null || dataGrid.ItemsSource==null)
        //    {
        //        MessageBox.Show("指定的数据集为空或不包含要写出的数据表！");
        //        return "";
        //    }
        //    string csvStr = "";
        //    //下面写出数据
        //    //DataTable tb = dataGrid.Tables[tableName];
        //    //写表名
        //    //csvStr += tb.TableName + "\n";
        //    //第一步：写出列名
          
        //    foreach (var column in dataGrid.Columns)
        //    {
        //        if(column is DataGridTextColumn)
        //        {
        //            DataGridTextColumn item=column as DataGridTextColumn;
        //            csvStr += "\""+column.Header.ToString() +"\"" +",";
        //        }
        //    }
        //    //去掉最后一个","
        //    csvStr = csvStr.Remove(csvStr.LastIndexOf(","), 1);
        //    csvStr += "\n";
            
        //    //第二步：写出数据
        //    foreach (var row in ItemsSource)
        //    {
        //        if(row is DateTime)
        //        {
        //            CustomDateConverter converter=new CustomDateConverter();
        //            converter.Convert(row,"DATE");
        //        }
        //        foreach (DataColumn column in tb.Columns)
        //        {
        //            csvStr += "\"" + row[column].ToString() + "\"" + ",";
        //        }
        //        csvStr = csvStr.Remove(csvStr.LastIndexOf(","), 1);
        //        csvStr += "\n";
        //    }
        //    return csvStr;
        //}

        #endregion


        /// <summary>        
        /// 导出DataGrid数据到Excel        
        /// </summary>        
        /// <param name="withHeaders">是否需要表头</param>        
        /// <param name="grid">DataGrid</param>        
        /// <returns>Excel内容字符串</returns>        
        public static string ExportDataGrid(bool withHeaders, DataGrid grid)
        {
            System.Reflection.PropertyInfo propInfo;
            System.Windows.Data.Binding binding;
            var strBuilder = new System.Text.StringBuilder();
            DictionaryConverter dicConverter = new DictionaryConverter();
            var source = (grid.ItemsSource as System.Collections.IList);
            if (source == null) return "";
            var headers = new List<string>();
            grid.Columns.ForEach(col =>
            {
                if (col is DataGridBoundColumn)
                {
                    string strHeader = ConvertDic(col.Header.ToString());
                    headers.Add(FormatCsvField(strHeader));
                }
            });
            strBuilder.Append(String.Join(",", headers.ToArray())).Append("\r\n");
            foreach (Object data in source)
            {
                var csvRow = new List<string>();
                foreach (DataGridColumn col in grid.Columns)
                {
                    try
                    {
                        if (col is DataGridBoundColumn)
                        {
                            binding = (col as DataGridBoundColumn).Binding;
                            string colPath = binding.Path.Path;
                            string[] arr = colPath.Split('.');
                            string dicCategory = Convert.ToString(binding.ConverterParameter);//如有绑定字典值，则为字典类别
                            propInfo = data.GetType().GetProperty(colPath);
                            object ob = data;
                            if (arr.Length > 1)
                            {
                                ob = data.GetObjValue(arr[0]);
                                propInfo = data.GetObjValue(arr[0]).GetType().GetProperty(arr[1]);
                            }
                            if (propInfo != null)
                            {
                                object obj = propInfo.GetValue(ob, null) == null ? null : propInfo.GetValue(ob, null).ToString();
                                obj = dicConverter.Convert(obj, null, dicCategory, null);
                                string value = Convert.ToString(obj);
                                csvRow.Add(FormatCsvField(value));
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                strBuilder.Append(String.Join(",", csvRow.ToArray())).Append("\r\n");
            }
            return strBuilder.ToString();
        }

        #region 导出CSV
        /// <summary>        
        /// CSV格式化        
        /// </summary>        
        /// <param name="data">数据</param>        
        /// <returns>格式化数据</returns>        
        private static string FormatCsvField(string data)
        {
            return String.Format("\"{0}\"", data.Replace("\"", "\"\"\"").Replace("\n", "").Replace("\r", ""));
        }

        /// <summary>
        /// 根据绑定字典值转换成相应名称(可加判断扩展)
        /// </summary>
        /// <param name="strDic">字典值</param>
        /// <returns>转换后名称</returns>
        private static string ConvertDic(string strDic)
        {
            string strName=string.Empty;
            try
            {
                strName = Utility.GetResourceStr(strDic);
            }
            catch (Exception)
            {
                 strName=strDic;
            }
            return strName;
        }

         /// <summary>       
        /// /// 导出DataGrid数据到Excel        
        /// </summary>        
        /// <param name="withHeaders">是否需要表头</param>  
        /// /// <param name="grid">DataGrid</param>      
        /// /// <param name="dataBind"></param>        
        /// <returns>Excel内容字符串</returns>      
        public static string ExportDataGrid(bool withHeaders, DataGrid grid, bool dataBind)
        {
            var strBuilder = new System.Text.StringBuilder();
            var source = (grid.ItemsSource as System.Collections.IList);
            if (source == null) return "";
            var headers = new List<string>();
            grid.Columns.ForEach(col =>
            {
                if (col is DataGridTemplateColumn)
                {
                    headers.Add(col.Header != null ? FormatCsvField(col.Header.ToString()) : string.Empty);
                }
            });
            strBuilder.Append(String.Join(",", headers.ToArray())).Append("\r\n");
            foreach (Object data in source)
            {
                var csvRow = new List<string>();
                foreach (DataGridColumn col in grid.Columns)
                {
                    if (col is DataGridTemplateColumn)
                    {
                        FrameworkElement cellContent = col.GetCellContent(data);
                        TextBlock block;
                        if (cellContent.GetType() == typeof(Grid))
                        {
                            block = cellContent.FindName("TempTextblock") as TextBlock;
                        }
                        else
                        {
                            block = cellContent as TextBlock;
                        }
                        if (block != null)
                        {
                            csvRow.Add(FormatCsvField(block.Text));
                        }
                    }
                }
                strBuilder.Append(String.Join(",", csvRow.ToArray())).Append("\r\n");
            }
            return strBuilder.ToString();
        }

        /// <summary>        
        /// 导出DataGrid数据到Excel为CVS文件     
        /// 使用utf8编码 中文是乱码 改用Unicode编码 
        /// </summary>       
        /// <param name="withHeaders">是否带列头</param>
        /// <param name="grid">DataGrid</param>     
        public static void ExportDataGridSaveAs(DataGrid grid)
        {
            string data = ExportDataGrid(true, grid);
            var sfd = new SaveFileDialog
            {
                DefaultExt = "csv",
                Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*",
                FilterIndex = 1
            };
            if (sfd.ShowDialog() == true)
            {
                using (Stream stream = sfd.OpenFile())
                {
                    using (var writer = new StreamWriter(stream, System.Text.Encoding.Unicode))
                    {
                        data = data.Replace(",", "\t");
                        writer.Write(data);
                        writer.Close();
                    }
                    stream.Close();
                    MessageBox.Show("导出成功！");
                }
            }
        }

        #endregion
    
    }
}
