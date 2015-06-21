using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
namespace SMT
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DBProviderType
    {
        Oracle,
        SqlServer,
        OleDb,
        Odbc,
        MySql
    }
    public class DataBaseType
    {
        private static Hashtable hastableName = new Hashtable();
        private static string dataBaseTypeString;
        public static string DataBaseTypeString
        {
            get
            {
                if (string.IsNullOrEmpty(dataBaseTypeString))
                {
                    return DataBaseConfig.GetDataBaseType();
                }
                else
                {
                    return dataBaseTypeString;
                }
            }
            set
            {
                dataBaseTypeString = value;
            } 
        }
        /// <summary>
        /// 得到数据库类型
        /// </summary>
        /// <returns></returns>
        public static DBProviderType GetDBProviderType()
        {
            string providerType = DataBaseTypeString;
            DBProviderType dataProvider;
            switch (providerType.ToLower())
            {
                case "oracle":
                    dataProvider = DBProviderType.Oracle;
                    break;
                case "sqlserver":
                    dataProvider = DBProviderType.SqlServer;
                    break;
                case "oledb":
                    dataProvider = DBProviderType.OleDb;
                    break;
                case "odbc":
                    dataProvider = DBProviderType.Odbc;
                    break;
                case "mysql":
                    dataProvider = DBProviderType.MySql;
                    break;
                default:
                    return DBProviderType.Odbc;
            }
            return dataProvider;
        }
        public static Hashtable GetTableColumn(string tableName)
        {
            if (hastableName.Contains(tableName))
            {
                return (Hashtable)hastableName[tableName];
            }
            else
            {
                Hashtable hast = new Hashtable();
                string sql = "";
                //switch (DataBaseTypeString.ToLower())
                //{
                //    case "oracle":
                //        sql = "SELECT COLUMN_NAME AS NAME FROM ALL_TAB_COLUMNS WHERE TABLE_NAME = '" + tableName + "' ORDER BY NAME";
                //        break;
                //    case "sqlserver":
                //        sql = "select name from syscolumns where id=object_id('" + tableName + "')";
                //        break;
                //    default:
                //        return null;

                //}
                sql = "select * from " + tableName + " where 1=0";
                SMT.DataResult dresult = SMT.DataProvider.GetDataTable(sql);
                DataTable dt = dresult.DataTable;
                //if (dt != null)
                //{
                //    int n = dt.Rows.Count;
                //    for (int i = 0; i < n; i++)
                //    {
                //        string name = dt.Rows[i]["NAME"].ToString().ToUpper();
                //        if (!hast.Contains(name))
                //        {
                //            hast.Add(name, name);
                //        }
                //        //parmCache.Add(name, name);
                //    }
                //    hastableName.Add(tableName, hast);
                //}
                if (dt != null)
                {
                    int n = dt.Columns.Count;
                    for (int i = 0; i < n; i++)
                    {
                        string name = dt.Columns[i].ToString().ToUpper();
                        if (!hast.Contains(name))
                        {
                            hast.Add(name, name);
                        }
                        //parmCache.Add(name, name);
                    }
                    hastableName.Add(tableName, hast);
                }
                return hast;
            }
        }

    }   
}
