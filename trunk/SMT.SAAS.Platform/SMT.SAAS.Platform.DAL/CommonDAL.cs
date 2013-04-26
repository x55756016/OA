using System;
using System.Linq;
using SMT.Foundation.Core;
using System.Data.Objects;
using System.Reflection;
using System.Collections.Generic;
using System.Configuration;
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供基于Oracle数据库的WEBPART信息访问实现
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.OracleDAL
{
    /// <summary>
    /// 封装访问数据库的的公共方法
    /// </summary>
    /// <typeparam name="TEntity">映射出的数据表类型</typeparam>
    internal class CommonDAL<TEntity> : BaseDAL
    {
        //数据库连接字符串，需要在AppConfig配置文件中进行配置
        private static string _conn = string.Empty;
        private OracleDAO _dao = null;
        /// <summary>
        /// 创建一个<see cref="CommonDAL"/>的新实例。
        /// </summary>
        public CommonDAL()
        {
            _conn = ConfigurationManager.AppSettings["ConnectionString"].ToString();

            if (string.IsNullOrEmpty(_conn))
                _dao = new OracleDAO(_conn);
        }

        /// <summary>
        /// 新增一条数据到数据库中
        /// </summary>
        /// <param name="entity">封装对应名称的数据表结构</param>
        /// <returns>是否新增成功</returns>
        public bool Add(TEntity entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");
                int i = base.Add(entity);
                return i >= 1 ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception("新增数据产生异常", ex);
            }
        }

        /// <summary>
        /// 修改数据库中的一条数据
        /// </summary>
        /// <param name="entity">封装对应名称的数据表结构</param>
        /// <returns>是否修改成功</returns>
        public bool Update(TEntity entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");
                int i = base.Update(entity);
                return i >= 1 ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception("修改数据产生异常", ex);
            }
        }

        /// <summary>
        /// 删除数据库中的一条数据
        /// </summary>
        /// <param name="entity">封装对应名称的数据表结构</param>
        /// <returns>是否删除成功</returns>
        public bool Delete(TEntity entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");
                int i = base.Delete(entity);
                return i >= 1 ? true : false;
            }
            catch (Exception ex)
            {
                throw new Exception("删除数据产生异常", ex);
            }
        }


        /// <summary>
        /// 获取整个表数据.慎用
        /// </summary>
        /// <returns>返回的结果集</returns>
        public IQueryable<TEntity> GetTable()
        {
            return base.GetTable<TEntity>();
        }

        public ObjectQuery<TEntity> GetObjects()
        {
            return base.GetObjects<TEntity>();
        }

        public object ExecuteStoredProcedure(string Sqlstring, ParameterCollection prameters)
        {

            //prameters = new ParameterCollection();
            //prameters.Add("parme1", null);
            return _dao.ExecuteScalar("", System.Data.CommandType.StoredProcedure, prameters);
        }

        public object ExecuteCustomerSql(string Sqlstring, ParameterCollection prameters)
        {
            OracleDAO dao = new OracleDAO(_conn);
            //prameters = new ParameterCollection();
            //prameters.Add("parme1", null);
            return dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text, prameters);

            //return object obj=dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);
            //prameters = new ParameterCollection();
            //object obj1 = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text, prameters);
        }

        public object ExecuteCustomerSql(string Sqlstring)
        {
            OracleDAO dao = new OracleDAO(_conn);
            object obj = dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);

            //ParameterCollection prameters = new ParameterCollection();
            //object obj1 = dao.GetDataTable(Sqlstring, System.Data.CommandType.Text, prameters);
            return obj;
        }


    }
}
