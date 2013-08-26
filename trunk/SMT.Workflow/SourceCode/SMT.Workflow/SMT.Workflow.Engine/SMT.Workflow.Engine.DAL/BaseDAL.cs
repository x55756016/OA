/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：BaseDAL.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/11/23 14:25:49   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.DAL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Workflow.Common.DataAccess;
using System.Data;
using System.Configuration;
using System.Reflection;

namespace SMT.Workflow.Engine.DAL
{
    /// <summary>
    /// 基类
    /// </summary>
    public class BaseDAL
    {

        /// <summary>
        /// 链接
        /// </summary>
        public IDAO dao = DALFacoty.Create(ConfigurationManager.ConnectionStrings["WorkFlowConnString"].ToString());

        /// <summary>
        /// DataTable 转换为List 集合
        /// </summary>
        /// <typeparam name="TResult">类型</typeparam>
        /// <param name="dt">DataTable</param>
        /// <returns>List</returns>
        public List<TResult> ToList<TResult>(DataTable dt) where TResult : class, new()
        {
            //创建一个属性的列表
            List<PropertyInfo> prlist = new List<PropertyInfo>();
            //获取TResult的类型实例  反射的入口
            Type t = typeof(TResult);
            //获得TResult 的所有的Public 属性 并找出TResult属性和DataTable的列名称相同的属性(PropertyInfo) 并加入到属性列表 
            Array.ForEach<PropertyInfo>(
                t.GetProperties(), 
                p => 
                {
                    if (dt.Columns.IndexOf(p.Name) != -1)
                    {
                        prlist.Add(p); 
                    }
                });
            //创建返回的集合
            List<TResult> oblist = new List<TResult>();

            foreach (DataRow row in dt.Rows)
            {
                //创建TResult的实例
                TResult ob = new TResult();
                //找到对应的数据  并赋值
                prlist.ForEach(p => 
                {
                    if (row[p.Name] != DBNull.Value)
                    {
                        p.SetValue(ob, row[p.Name], null); 
                    }
                });
                //放入到返回的集合中.
                oblist.Add(ob);
            }
            return oblist;
        }
    }
}
