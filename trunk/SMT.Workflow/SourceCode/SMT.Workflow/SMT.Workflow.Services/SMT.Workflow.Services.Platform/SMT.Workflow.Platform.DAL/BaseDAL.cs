using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using SMT.Workflow.Common.DataAccess;
using System.Data;
using System.Reflection;
using System.Data.OracleClient;

namespace SMT.Workflow.Platform.DAL
{
    public class BaseDAL
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                object conntring = CacheProvider.GetCache("WorkFlowConnString2012");
                if (conntring != null)
                {
                    LogHelper.WriteLog("从 [缓存] 中读取数据库连接串:" + conntring.ToString());
                    return conntring.ToString();
                }
                else
                {
                    conntring = System.Configuration.ConfigurationManager.ConnectionStrings["WorkFlowConnString"].ConnectionString;
                    CacheProvider.Add("WorkFlowConnString2012", conntring);
                    LogHelper.WriteLog("从 [配置文件] 中读取数据库连接串:" + conntring.ToString());
                }
                return conntring.ToString();
            }
        }
        public IDAO dao = DALFacoty.Create(ConfigurationManager.ConnectionStrings["WorkFlowConnString"].ToString());
        
        /// <summary>
        /// DataTable 转换为List 集合
        /// </summary>
        /// <typeparam name="TResult">类型</typeparam>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public List<TResult> ToList<TResult>(DataTable dt) where TResult : class, new()
        {
            //创建一个属性的列表
            List<PropertyInfo> prlist = new List<PropertyInfo>();
            //获取TResult的类型实例  反射的入口
            Type t = typeof(TResult);
            //获得TResult 的所有的Public 属性 并找出TResult属性和DataTable的列名称相同的属性(PropertyInfo) 并加入到属性列表 
            Array.ForEach<PropertyInfo>(t.GetProperties(), p => { if (dt.Columns.IndexOf(p.Name) != -1) prlist.Add(p); });
            //创建返回的集合
            List<TResult> oblist = new List<TResult>();

            foreach (DataRow row in dt.Rows)
            {
                //创建TResult的实例
                TResult ob = new TResult();
                //找到对应的数据  并赋值
                prlist.ForEach(p => { if (row[p.Name] != DBNull.Value) p.SetValue(ob, row[p.Name], null); });
                //放入到返回的集合中.
                oblist.Add(ob);
            }
            return oblist;
        }
        /// <summary>
        /// 对IQueryable对像分页处理
        /// </summary>
        /// <typeparam name="T">需要分页对像类型</typeparam>
        /// <param name="ents">需要分页的IQueryable对像,必需是有排过序的</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页记录条数</param>
        /// <param name="pageCount">页数</param>
        /// <returns>IQueryable对像</returns>
        public static IQueryable<T> Pager<T>(IQueryable<T> ents, int pageIndex, int pageSize, ref int pageCount)
        {
            int count = ents.Count();
            pageCount = count / pageSize;
            int tmp = count % pageSize;

            pageCount = pageCount + (tmp > 0 ? 1 : 0);
            if (pageIndex > pageCount)
                pageIndex = 1;

            ents = ents.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return ents;
        }
        /// <summary>
        /// 如果value值为null则返回""字符串,否则返回value值。
        /// </summary>
        /// <param name="value">value值</param>
        /// <returns></returns>
        public object GetValue(object value)
        {
            return value == null ? DBNull.Value.ToString() : value;
        }
        public int ExecuteSql(string sql, OracleParameter[] pageparm)
        {
            int result = 0;
            try
            {
                dao.Open();
                result = dao.ExecuteNonQuery(sql, System.Data.CommandType.Text, pageparm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                dao.Close();
            }
            return result;
        }     
    }
}
