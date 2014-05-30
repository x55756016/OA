using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
namespace SMT
{
    /// <summary>
    /// 执行返回的结果
    /// </summary>
      
    public class EntityResult
    {
        /// <summary>
        /// 返回SQL语句
        /// </summary>
         
          public string Sql { get; set; }
        /// <summary>
        /// 影响的记录数
        /// </summary>
         
          public int RecordCount { get; set; }
        /// <summary>
        /// DataTable
        /// </summary>
         
          public DataTable DataTable { get; set; }
        /// <summary>
        /// DataSet
        /// </summary>
         
          public DataSet DataSet { get; set; }
        /// <summary>
        /// IDataReader
        /// </summary>
        
          public IDataReader IDataReader { get; set; }
        /// <summary>
        /// IDbDataParameter
        /// </summary>
         
          public IDbDataParameter[] IDbDataParameter { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        
          public string Error { get; set; }
    }
    /// <summary>
    /// 实体的基础类
    /// </summary>
    [DataContract]
    public partial class EntityBase// : IEntityBase
    {
        #region 固定属性设置
        /// <summary>
        ///主关键字段值(唯一值，用来修改、删除)
        /// </summary>
        
        public virtual string PrimaryKeyValue { get; set; }
        /// <summary>
        /// 主关键字段名称(唯一值，用来修改、删除)
        /// </summary>
        
        public virtual string PrimaryKeyName { get; set; }
        /// <summary>
        /// 外键字段名称(对应主表的[取值]字段名称)
        /// </summary>
        
        public virtual string ForeignKey { get; set; }
        /// <summary>
        /// 外键字段名称(对应从表的[存值]字段名称)
        /// </summary>
        
        public virtual string ForeignKeyName { get; set; }
        #endregion
      //  private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());
        EntityBase eb;
        public Hashtable has = new Hashtable();
        public EntityBase()
        {
           // has = DataBaseType.GetTableColumn(this.GetType().Name);
           
          //  string sql = "SELECT COLUMN_NAME FROM ALL_TAB_COLUMNS WHERE TABLE_NAME = '" + this.GetType().Name.ToUpper()+ "'";
          //SMT.DataResult dresult= SMT.DataProvider.GetDataTable(sql);
          //DataTable dt = dresult.DataTable;
          //  if (dt != null)
          //  {
          //      int n = dt.Rows.Count;
          //      for (int i = 0; i < n; i++)
          //      {
          //          string name = dt.Rows[i]["COLUMN_NAME"].ToString().ToUpper();
          //          has.Add(name, name);
          //          //parmCache.Add(name, name);
          //      }
          //  }
 
        }
        #region 事务处理
        //static  IDbConnection idbconn;
        ///// <summary>
        ///// 开始事务
        ///// </summary>
        //public   void BeginTransaction()
        //{
        //   idbconn= SMT.DataProvider.BeginTransaction();          
        //}       
        ///// <summary>
        ///// 提交事务
        ///// </summary>
        //public  bool CommitTransaction()
        //{
        //   return SMT.DataProvider.CommitTransaction();   
        //}
        #region 事务增加\更新\删除
        /// <summary>
        /// 增加(通过事务,必须在BeginTransaction()和 CommitTransaction()二个方法中间)
        /// </summary>
        
        public EntityResult AddByTransaction(IDbCommand idbconn)
        {
            
            eb = this;
            Type type = this.GetType();
            string name = "(";
            string value = "(";
            string name2 = "(";
            string value2 = "(";
            if (has == null)
            {
                has = DataBaseType.GetTableColumn(this.GetType().Name);
            }
            SMT.DataProvider.CreateParameters(has.Count);
            int i = 0;
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                if (has.Contains(p.Name.ToUpper()))
                {
                    SMT.DataProvider.AddParameters(i, p.Name.ToUpper(), p.GetValue(eb, null));
                    name += p.Name.ToUpper() + ",";
                    value += ":" + p.Name.ToUpper() + ",";
                    name2 += p.Name.ToUpper() + ",";
                    value2 += "'" + p.GetValue(eb, null) + "',";

                    i++;
                }
            }
            string Sql = "INSERT INTO  " + this.GetType().Name + " " + name2.TrimEnd(',') + ")VALUES" + value2.TrimEnd(',') + ")";
            string exeSql = "INSERT INTO  " + this.GetType().Name + " " + name.TrimEnd(',') + ")VALUES" + value.TrimEnd(',') + ")";           
            EntityResult result = GetEntityResult(SMT.DataProvider.ExecuteSQLByTransaction(idbconn, exeSql));
            result.Sql = Sql;
            return result;
        }
        /// <summary>
        /// 更改(通过事务,必须在BeginTransaction()和 CommitTransaction()二个方法中间)
        /// </summary>
       
        public EntityResult UpdateByTransaction(IDbCommand idbconn)
        {
            #region 参数化
            eb = this;
            string sql = "";
            Type type = this.GetType();
            string name = "";
            if (has == null)
            {
                has = DataBaseType.GetTableColumn(this.GetType().Name);
            }
            SMT.DataProvider.CreateParameters(has.Count);
            int i = 0;
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                //Console.WriteLine(p.Name + "=" + p.GetValue(eb, null));
                //this.GetType().GetProperties()[3].Name
                if (has.Contains(p.Name.ToUpper()))
                {

                    if (PrimaryKeyName.ToUpper() != p.Name.ToUpper())
                    {
                        SMT.DataProvider.AddParameters(i, p.Name.ToUpper(), p.GetValue(eb, null));
                        sql += p.Name.ToUpper() + "='" + p.GetValue(eb, null) + "',";
                        name += p.Name.ToUpper() + "=:" + p.Name.ToUpper() + ",";
                        i++;

                    }
                }
            }
            sql = "UPDATE " + this.GetType().Name + " SET " + sql.TrimEnd(',') + " WHERE  " + PrimaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
            SMT.DataProvider.AddParameters(i, PrimaryKeyName.ToUpper(), PrimaryKeyValue);
            string exeSql = "UPDATE " + this.GetType().Name + " SET " + name.TrimEnd(',') + " WHERE  " + PrimaryKeyName.ToUpper() + "=:" + PrimaryKeyName.ToUpper() + "";
                     
            EntityResult result = GetEntityResult(SMT.DataProvider.ExecuteSQLByTransaction(idbconn, exeSql));
            result.Sql = sql;
            return result;
            #endregion
        }
        /// <summary>
        /// 删除(通过事务,必须在BeginTransaction()和 CommitTransaction()二个方法中间)
        /// </summary>
       
        public EntityResult DeleteByTransaction(IDbCommand idbconn)
        {
            string sql = "DELETE FROM " + this.GetType().Name + " WHERE " + PrimaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
            //SMT.DataProvider.ExecuteSQLByTransaction(idbconn, sql);
            EntityResult result = GetEntityResult(SMT.DataProvider.ExecuteSQLByTransaction(idbconn, sql));
            result.Sql = sql;
            return result;
        }
        #endregion
        #endregion
        private EntityResult GetEntityResult(SMT.DataResult dresult)
        {
            EntityResult result = new EntityResult();
            result.DataSet = dresult.DataSet;
            result.DataTable = dresult.DataTable;
            result.Sql = dresult.Sql;
            result.IDataReader = dresult.IDataReader;
            result.IDbDataParameter = dresult.IDbDataParameter;
            result.RecordCount = dresult.RecordCount;
            result.Error = dresult.Error;
            return result;

        }
      
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        
        public EntityResult Delete()
        {
           
            string sql = "DELETE FROM " + this.GetType().Name + " WHERE " + PrimaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
          
            EntityResult result =GetEntityResult(SMT.DataProvider.ExecuteSQL(sql));
            int n = result.RecordCount;
            if (n > 0)
            {
                eb = null;               
                result.RecordCount = n;
            }
            else
            {              
                result.RecordCount =0; 
            }
            return result;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="childTableName">从表名称如是多个从表，以逗号隔开</param>
        /// <param name="primaryKeyName">主健字段名称</param>
        /// <returns></returns>
       
        public EntityResult Delete(string childTableName, string primaryKeyName)
        {
            List<string> sqllist=new List<string>();
           
            if (childTableName.IndexOf(',') > 0)
            {
                string[] sqls=childTableName.Split(',');
                for (int i = 0; i < sqls.Length; i++)
                {
                    string childsql = "DELETE FROM " + sqls[i] + " WHERE " + primaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
                    sqllist.Add(childsql);
                }
            }
            else
            {
                string childsql = "DELETE FROM " + childTableName + " WHERE " + primaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
                sqllist.Add(childsql);
            }
            string mainsql = "DELETE FROM " + this.GetType().Name + " WHERE " + PrimaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
            sqllist.Add(mainsql); 
            EntityResult result = GetEntityResult(SMT.DataProvider.ExecuteByTransaction(sqllist));
            int n = result.RecordCount;
            if (n > 0)
            {
                eb = null;
                result.RecordCount = n;
            }
            else
            {
                result.RecordCount = 0;
            }
            return result;
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        
        public EntityResult Update()
        {
            #region sql语句           
            //Result result = new Result();
            //eb = this;
            //string sql="";          
            //Type type=this.GetType();
            //foreach (PropertyInfo p in this.GetType().GetProperties())
            //     {
            //         //Console.WriteLine(p.Name + "=" + p.GetValue(eb, null));
            //         //this.GetType().GetProperties()[3].Name
            //         if (has.Contains(p.Name.ToUpper()))
            //         {
            //             if (PrimaryKeyName.ToUpper() != p.Name.ToUpper())
            //             {
            //                 sql += p.Name.ToUpper() + "='" + p.GetValue(eb, null) + "',";
            //             }
            //         }
            //     }
            //string exeSql="UPDATE "+this.GetType().Name+" SET "+sql.TrimEnd(',')+" WHERE  "+ PrimaryKeyName.ToUpper() + "='" + PrimaryKey + "'";
            //result.Sql = exeSql;
            //int n = SMT.DataProvider.ExecuteSQL(exeSql);
            //if (n > 0)
            //{
            //    result.RecordCount = n;
            //}
            //else
            //{
            //    result.RecordCount = 0;
            //}
            //return result;
            #endregion
            #region 参数化
          
            eb = this;
            string sql = "";
            Type type = this.GetType();
            string name = "";
            if (has == null)
            {
                has = DataBaseType.GetTableColumn(this.GetType().Name);
            }
            SMT.DataProvider.CreateParameters(has.Count);
            int i = 0;
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                //Console.WriteLine(p.Name + "=" + p.GetValue(eb, null));
                //this.GetType().GetProperties()[3].Name
                if (has.Contains(p.Name.ToUpper()))
                {

                    if (PrimaryKeyName.ToUpper() != p.Name.ToUpper())
                    {
                        SMT.DataProvider.AddParameters(i, p.Name.ToUpper(), p.GetValue(eb, null));
                        sql += p.Name.ToUpper() + "='" + p.GetValue(eb, null) + "',";
                        name += p.Name.ToUpper() + "=:" + p.Name.ToUpper() + ",";
                        i++;

                    }
                }
            }
            sql = "UPDATE " + this.GetType().Name + " SET " + sql.TrimEnd(',') + " WHERE  " + PrimaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
            SMT.DataProvider.AddParameters(i, PrimaryKeyName.ToUpper(), PrimaryKeyValue);
            string exeSql = "UPDATE " + this.GetType().Name + " SET " + name.TrimEnd(',') + " WHERE  " + PrimaryKeyName.ToUpper() + "=:" + PrimaryKeyName.ToUpper() + ""; 
         
            EntityResult result = GetEntityResult(SMT.DataProvider.ExecuteSQL(exeSql));
            result.Sql = sql;
            int n = result.RecordCount;
            if (n > 0)
            {
                result.RecordCount = n;
            }
            else
            {
                result.RecordCount = 0;
            }
            return result;
            #endregion
        }
        /// <summary>
        /// 增加
        /// </summary>
        /// <returns></returns>
       
        public EntityResult Add()
        {
            eb = this;
          
            Type type = this.GetType();
            //string name = "(";
            //string value = "(";
            //foreach (PropertyInfo p in this.GetType().GetProperties())
            //{
            //    //Console.WriteLine(p.Name + "=" + p.GetValue(eb, null));
            //    //this.GetType().GetProperties()[3].Name
            //    if (has.Contains(p.Name.ToUpper()))
            //    {
            //        name += p.Name.ToUpper() + ",";
            //        value +="'"+ p.GetValue(eb, null) +"',";                    
            //    }
            //}
            string name = "(";
            string value = "(";
            string name2 = "(";
            string value2 = "(";
            if (has == null || has.Count==0)
            {
                has = DataBaseType.GetTableColumn(this.GetType().Name);
            }
            SMT.DataProvider.CreateParameters(has.Count);
            int i = 0;
         
            foreach (PropertyInfo p in this.GetType().GetProperties())
            {
                if (has.Contains(p.Name.ToUpper()))
                {
                  
                    SMT.DataProvider.AddParameters(i, p.Name.ToUpper(), p.GetValue(eb, null));
                    name += p.Name.ToUpper() + ",";
                    value += ":" + p.Name.ToUpper() + ",";
                    name2 += p.Name.ToUpper() + ",";
                    value2 += "'" + p.GetValue(eb, null) + "',";                 
                    i++;
                }
            }
            if (has.Count != i)
            {//数据表的字段与实体定义的字段属表有不一样的,重新分配
                SMT.DataProvider.ClearParameters();
             
                 name = "(";
                 value = "(";
                 name2 = "(";
                 value2 = "(";
                SMT.DataProvider.CreateParameters(i);
                i = 0;
                foreach (PropertyInfo p in this.GetType().GetProperties())
                {
                    if (has.Contains(p.Name.ToUpper()))
                    {
                        
                        SMT.DataProvider.AddParameters(i, p.Name.ToUpper(), p.GetValue(eb, null));
                        name += p.Name.ToUpper() + ",";
                        value += ":" + p.Name.ToUpper() + ",";
                        name2 += p.Name.ToUpper() + ",";
                        value2 += "'" + p.GetValue(eb, null) + "',";
                        i++;
                    }
                } 
            }
            string Sql = "INSERT INTO  " + this.GetType().Name + " " + name2.TrimEnd(',') + ")VALUES" + value2.TrimEnd(',') + ")";
            string exeSql = "INSERT INTO  " + this.GetType().Name + " " + name.TrimEnd(',') + ")VALUES" + value.TrimEnd(',') + ")";
            EntityResult result = GetEntityResult(SMT.DataProvider.ExecuteSQL(exeSql));
            result.Sql = Sql;
            int n = result.RecordCount;
           
            if (n> 0)
            {
                result.RecordCount = n;
            }
            else
            {
                result.RecordCount = 0;
            }
            return result;
        }
        /// <summary>
        /// 是否存在记录
        /// </summary>
        /// <returns></returns>
       
        public EntityResult Exists()
        {
            string sql = "SELECT * FROM " + this.GetType().Name + " WHERE " + PrimaryKeyName.ToUpper() + "='" + PrimaryKeyValue + "'";
            EntityResult result = GetEntityResult(SMT.DataProvider.ExistsRecord(sql));
            int n = result.RecordCount;
            if (n > 0)
            {
                eb = null;
                result.RecordCount = n;
            }
            else
            {
                result.RecordCount = 0;
            }
            return result;
        }
    }
}
