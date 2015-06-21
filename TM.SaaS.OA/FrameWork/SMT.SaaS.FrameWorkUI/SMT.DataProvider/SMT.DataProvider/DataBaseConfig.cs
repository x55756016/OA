using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Data;

namespace SMT
{
    class DataBaseConfig
    {
        private static Hashtable hast = new Hashtable();
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {           
            if (!hast.Contains("longkcgetidbconnection"))
            {
                string connectionstring = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                hast.Add("longkcgetidbconnection", connectionstring);
                return connectionstring;
            }
            else
            {
                return hast["longkcgetidbconnection"].ToString();
            }
            //return ConfigurationManager.AppSettings["ConnectionString"].ToString();
        }
        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <returns></returns>
        public static string GetDataBaseType()
        {
            if (!hast.Contains("longkcgetdatabasetype"))
            {
                string connectionstring = ConfigurationManager.AppSettings["DataBaseType"].ToString();
                hast.Add("longkcgetdatabasetype", connectionstring);
                return connectionstring;
            }
            else
            {
                return hast["longkcgetdatabasetype"].ToString();
            }
           // return ConfigurationManager.AppSettings["DataBaseType"].ToString();
        }
      
    }
}
