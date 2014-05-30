using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Collections;
namespace SMT
{
    #region 实体对象数据上下文
    /// <summary>
    ///  实体对象数据上下文
    /// </summary>
    public class DataContext
    {
        /// <summary>
        ///  执行返回的结果
        /// </summary>
        public EntityResult Result;
        private Type type;
        private List<object> entityList;//装载实体列表
        private object entity;
       /// <summary>
       /// 构造函数
       /// </summary>
        public DataContext()
        {
            Result = new EntityResult();
        }
        /// <summary>
        /// 查找父表实体
        /// </summary>
        /// <typeparam name="TEntity">父表实体类</typeparam>
        /// <returns></returns>
        public DataContext Parent<TEntity>() where TEntity : new()
        {          
            string error = "";
            type = entity.GetType();
            object t = new TEntity();
            Type ptype = t.GetType();
            string table = t.GetType().Name;
            #region 单个实体
            PropertyInfo pi = type.GetProperty(table);//主表中的明细对象的 [属性名称] 一定要跟明细表的 [类名] 一样
            if (pi != null)
            {
                if (pi.PropertyType.IsClass)
                {
                    if (pi.PropertyType.Name == table)
                    {
                        //try
                        //{
                        string foreignkey = entity.GetType().GetProperty("ForeignKey").GetValue(entity, null).ToString();//外键字段名称
                        string foreignkeyname = entity.GetType().GetProperty("ForeignKeyName").GetValue(entity, null).ToString();//从表储存外键值字段名称
                                            
                            try
                            {
                                string value = type.GetProperty(foreignkeyname).GetValue(entity, null).ToString();//外键字段值
                                string sql2 = "select * from " + table + " where " + foreignkey + "='" + value + "'";
                                t = GetChildClass(t, sql2);
                                pi.SetValue(entity, t, null);
                            }
                            catch (Exception e)
                            {
                                error = pi.PropertyType.Name + ":" + e.Message;
                                Result.Error += error + ";";
                            }
                    }
                }
            }
            #endregion    
            return this;
        }
        /// <summary>
        /// 查找父表实体
        /// </summary>
        /// <typeparam name="TEntity">父表实体类</typeparam>
        /// <param name="childColumn">所要显示的字段，如uid,usernam</param>
        /// <returns></returns>
        public DataContext Parent<TEntity>(string childColumn) where TEntity : new()
        {
            string error = "";
            type = entity.GetType();
            object t = new TEntity();
            Type ptype = t.GetType();
            string table = t.GetType().Name;
            #region 单个实体
            PropertyInfo pi = type.GetProperty(table);//主表中的明细对象的 [属性名称] 一定要跟明细表的 [类名] 一样
            if (pi != null)
            {
                if (pi.PropertyType.IsClass)
                {
                    if (pi.PropertyType.Name == table)
                    {
                        //try
                        //{
                        string foreignkey = entity.GetType().GetProperty("ForeignKey").GetValue(entity, null).ToString();//外键字段名称
                        string foreignkeyname = entity.GetType().GetProperty("ForeignKeyName").GetValue(entity, null).ToString();//从表储存外键值字段名称

                        try
                        {
                            string value = type.GetProperty(foreignkeyname).GetValue(entity, null).ToString();//外键字段值
                            string sql2 = "select " + childColumn + " from " + table + " where " + foreignkey + "='" + value + "'";
                            t = GetChildClass(t, sql2);
                            pi.SetValue(entity, t, null);
                        }
                        catch (Exception e)
                        {
                            error = pi.PropertyType.Name + ":" + e.Message;
                            Result.Error += error + ";";
                        }
                    }
                }
            }
            #endregion
            return this;
        }
        /// <summary>
        /// 包含明细表
        /// </summary>
        /// <typeparam name="TEntity">明细表类</typeparam>
        /// <returns></returns>
        public DataContext Inclued<TEntity>() where TEntity : new()
        {
            if (entityList != null)
            {
                IncludeList<TEntity>();
                return this;
            }
            string error = "";
            type = entity.GetType();
            object t = new TEntity();
            string table = t.GetType().Name;
            string datatableObjectName = null;            
             #region 单个实体
             PropertyInfo pi = type.GetProperty(table);//主表中的明细对象的 [属性名称] 一定要跟明细表的 [类名] 一样
             if (pi != null)
             {
                 if (pi.PropertyType.IsClass)
                 {
                     if (pi.PropertyType.Name == table)
                     {
                         //try
                         //{
                         string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                         string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                         if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                         {
                             try
                             {
                                 string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                 string sql2 = "select * from " + table + " where " + foreignkeyname + "='" + value + "'";
                                 t = GetChildClass(t, sql2);
                                 pi.SetValue(entity, t, null);
                             }
                             catch (Exception e)
                             {
                                 error = pi.PropertyType.Name + ":" + e.Message;
                                 Result.Error += error + ";";
                             }

                         }
                         //}
                         //catch (Exception e)
                         //{
                         //    error = e.Message;
                         //    Result.Error += error+";";
                         //}
                     }
                 }
             }
             #endregion
             #region 多个实体
             foreach (PropertyInfo p in type.GetProperties())
             {
                 if (p.PropertyType.IsClass && p.PropertyType.UnderlyingSystemType.Name == "List`1")
                 {
                     string A = p.ToString();//System.Collections.Generic.List`1[SMT.Test.SMT_TEST_DETAIL] SMT_TEST_DETAIL2
                     string ptablename = A.Substring(0, A.IndexOf(']')).Substring(A.LastIndexOf('.') + 1);
                     if (table == ptablename)
                     {
                         try
                         {
                             #region
                             List<TEntity> list = new List<TEntity>();
                             string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                             string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                             if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                             {
                                 string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                 string sql2 = "select * from " + table + " where " + foreignkeyname + "='" + value + "'";
                                 DataTable dtt = SMT.DataProvider.GetDataTable(sql2).DataTable;
                                 for (int i = 0; i < dtt.Rows.Count; i++)
                                 {
                                     t = new TEntity();
                                     foreach (PropertyInfo p2 in t.GetType().GetProperties())
                                     {
                                         if (dtt.Columns.Contains(p2.Name))
                                         {
                                             p2.SetValue(t, SMTProperty.SetPropertyValue(p2, dtt.Rows[i][p2.Name]), null);
                                             try
                                             {
                                                 p2.SetValue(t, SMTProperty.SetPropertyValue(p2, dtt.Rows[i][p2.Name]), null);
                                             }
                                             catch (Exception e)
                                             {
                                                 error = p2.Name + ":" + e.Message;
                                                 Result.Error += error + ";";
                                             }
                                         }
                                     }
                                     list.Add((TEntity)t);
                                 }
                                 p.SetValue(entity, list, null);
                             }
                             #endregion
                         }
                         catch (Exception e)
                         {
                             error = e.Message;
                             Result.Error += error + ";";
                         }
                     }
                 }
             }

             #endregion
             #region DatatTable
             foreach (PropertyInfo p in type.GetProperties())
             {
                 if (p.PropertyType.Name.ToLower() == "datatable")
                 {
                     datatableObjectName = p.Name;
                     if (datatableObjectName.Substring(0,datatableObjectName.LastIndexOf('_') )== table)
                     {
                         PropertyInfo dp = type.GetProperty(datatableObjectName);
                         if (dp.PropertyType.IsClass)
                         {
                             try
                             {
                                 string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                                 string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称
                                 if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                                 {
                                     try
                                     {
                                         string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                         string sqld = "select * from " + table + " where " + foreignkeyname + "='" + value + "'";
                                         dp.SetValue(entity, SMT.DataProvider.GetDataTable(sqld).DataTable, null);
                                     }
                                     catch (Exception e)
                                     {
                                         error = p.PropertyType.Name + ":" + e.Message;
                                         Result.Error += error + ";";
                                     }
                                 }
                             }
                             catch (Exception e)
                             {
                                 error = e.Message;
                                 Result.Error += error + ";";
                             }
                         }
                     }
                 }
             }
             #endregion
            return this;
        }
        /// <summary>
        /// 包含明细表
        /// </summary>
        /// <typeparam name="TEntity">明细表类</typeparam>
        /// <param name="childColumn">所要显示的字段，如uid,username</param>
        /// <returns></returns>
        public DataContext Inclued<TEntity>(string childColumn) where TEntity : new()
        {
            if (entityList != null)
            {
                IncludeList<TEntity>();
                return this;
            }
            string error = "";
            type = entity.GetType();
            object t = new TEntity();
            string table = t.GetType().Name;
            string datatableObjectName = null;
            #region 单个实体
            PropertyInfo pi = type.GetProperty(table);//主表中的明细对象的 [属性名称] 一定要跟明细表的 [类名] 一样
            if (pi != null)
            {
                if (pi.PropertyType.IsClass)
                {
                    if (pi.PropertyType.Name == table)
                    {
                        try
                        {
                            string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                            string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                            if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                            {
                                string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                string sql2 = "select " + childColumn + " from " + table + " where " + foreignkeyname + "='" + value + "'";
                                t = GetChildClass(t, sql2);
                                pi.SetValue(entity, t, null);
                            }
                        }
                        catch (Exception e)
                        {
                            error = e.Message;
                            Result.Error += error + ";";
                        }
                    }
                }
            }
            #endregion
            #region 多个实体
            foreach (PropertyInfo p in type.GetProperties())
            {
                if (p.PropertyType.IsClass && p.PropertyType.UnderlyingSystemType.Name == "List`1")
                {
                    string A = p.ToString();//System.Collections.Generic.List`1[SMT.Test.SMT_TEST_DETAIL] SMT_TEST_DETAIL2
                    string ptablename = A.Substring(0, A.IndexOf(']')).Substring(A.LastIndexOf('.') + 1);
                    if (table == ptablename)
                    {
                        try
                        {
                            #region
                            List<TEntity> list = new List<TEntity>();
                            string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                            string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                            if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                            {
                                string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                string sql2 = "select " + childColumn + " from " + table + " where " + foreignkeyname + "='" + value + "'";
                                DataTable dtt = SMT.DataProvider.GetDataTable(sql2).DataTable;
                                for (int i = 0; i < dtt.Rows.Count; i++)
                                {
                                    t = new TEntity();
                                    foreach (PropertyInfo p2 in t.GetType().GetProperties())
                                    {
                                        if (dtt.Columns.Contains(p2.Name))
                                        {
                                            p2.SetValue(t, SMTProperty.SetPropertyValue(p2, dtt.Rows[i][p2.Name]), null);
                                        }
                                    }
                                    list.Add((TEntity)t);
                                }
                                p.SetValue(entity, list, null);
                            }
                            #endregion
                        }
                        catch (Exception e)
                        {
                            error = e.Message;
                            Result.Error += error + ";";
                        }
                    }
                }
            }

            #endregion
            #region DatatTable
            foreach (PropertyInfo p in type.GetProperties())
            {
                if (p.PropertyType.Name.ToLower() == "datatable")
                {

                    datatableObjectName = p.Name;
                    if (datatableObjectName.Substring(0, datatableObjectName.LastIndexOf('_')) == table)
                    {

                        PropertyInfo dp = type.GetProperty(datatableObjectName);
                        if (dp.PropertyType.IsClass)
                        {
                            try
                            {
                                string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                                string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称
                                if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                                {
                                    string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                    string sqld = "select " + childColumn + " from " + table + " where " + foreignkeyname + "='" + value + "'";
                                    dp.SetValue(entity, SMT.DataProvider.GetDataTable(sqld).DataTable, null);
                                }
                            }
                            catch (Exception e)
                            {
                                error = e.Message;
                                Result.Error += error + ";";
                            }
                        }//endif
                    }
                }
            }
          
            #endregion
            return this;
        }
        private void IncludeList<TEntity>() where TEntity : new()
        {
            string error = "";
            //type = entity.GetType();
            object t = new TEntity();
            string table = t.GetType().Name;
            string datatableObjectName = null;
            foreach (object en in entityList)
            {
                type = en.GetType();
                #region 单个实体
                PropertyInfo pi = type.GetProperty(table);//主表中的明细对象的 [属性名称] 一定要跟明细表的 [类名] 一样
                if (pi != null)
                {

                    if (pi.PropertyType.IsClass)
                    {
                        if (pi.PropertyType.Name == table)
                        {
                            //try
                            //{
                            string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                            string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                            if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                            {
                                try
                                {
                                    string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                    string sql2 = "select * from " + table + " where " + foreignkeyname + "='" + value + "'";
                                    t = GetChildClass(t, sql2);
                                    pi.SetValue(entity, t, null);
                                }
                                catch (Exception e)
                                {
                                    error = pi.PropertyType.Name + ":" + e.Message;
                                    Result.Error += error + ";";
                                }

                            }
                            //}
                            //catch (Exception e)
                            //{
                            //    error = e.Message;
                            //    Result.Error += error+";";
                            //}
                        }
                    }
                }
                #endregion
                #region 多个实体
                foreach (PropertyInfo p in type.GetProperties())
                {
                    if (p.PropertyType.IsClass && p.PropertyType.UnderlyingSystemType.Name == "List`1")
                    {
                        string A = p.ToString();//System.Collections.Generic.List`1[SMT.Test.SMT_TEST_DETAIL] SMT_TEST_DETAIL2
                        string ptablename = A.Substring(0, A.IndexOf(']')).Substring(A.LastIndexOf('.') + 1);
                        if (table == ptablename)
                        {
                            try
                            {
                                #region
                                List<TEntity> list = new List<TEntity>();
                                string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                                string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                                if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                                {
                                    string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                    string sql2 = "select * from " + table + " where " + foreignkeyname + "='" + value + "'";
                                    DataTable dtt = SMT.DataProvider.GetDataTable(sql2).DataTable;
                                    for (int i = 0; i < dtt.Rows.Count; i++)
                                    {
                                        t = new TEntity();
                                        foreach (PropertyInfo p2 in t.GetType().GetProperties())
                                        {
                                            if (dtt.Columns.Contains(p2.Name))
                                            {
                                                p2.SetValue(t, SMTProperty.SetPropertyValue(p2, dtt.Rows[i][p2.Name]), null);
                                                try
                                                {
                                                    p2.SetValue(t, SMTProperty.SetPropertyValue(p2, dtt.Rows[i][p2.Name]), null);
                                                }
                                                catch (Exception e)
                                                {
                                                    error = p2.Name + ":" + e.Message;
                                                    Result.Error += error + ";";
                                                }
                                            }
                                        }
                                        list.Add((TEntity)t);
                                    }
                                    p.SetValue(entity, list, null);
                                }
                                #endregion
                            }
                            catch (Exception e)
                            {
                                error = e.Message;
                                Result.Error += error + ";";
                            }
                        }
                    }
                }

                #endregion
                #region DatatTable
                foreach (PropertyInfo p in type.GetProperties())
                {
                    if (p.PropertyType.Name.ToLower() == "datatable")
                    {
                        datatableObjectName = p.Name;
                        if (datatableObjectName.Substring(0, datatableObjectName.LastIndexOf('_')) == table)
                        {
                            
                            PropertyInfo dp = type.GetProperty(datatableObjectName);
                            if (dp.PropertyType.IsClass)
                            {
                                try
                                {
                                    string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                                    string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称
                                    if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                                    {
                                        try
                                        {
                                            string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                            string sqld = "select * from " + table + " where " + foreignkeyname + "='" + value + "'";
                                            dp.SetValue(entity, SMT.DataProvider.GetDataTable(sqld).DataTable, null);
                                        }
                                        catch (Exception e)
                                        {
                                            error = p.PropertyType.Name + ":" + e.Message;
                                            Result.Error += error + ";";
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    error = e.Message;
                                    Result.Error += error + ";";
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
        private void IncludeList<TEntity>(string childColumn) where TEntity : new()
        {
            string error = "";
            //type = entity.GetType();
            object t = new TEntity();
            string table = t.GetType().Name;
            string datatableObjectName = null;
            foreach (object en in entityList)
            {
                type = en.GetType();
                #region 单个实体
                PropertyInfo pi = type.GetProperty(table);//主表中的明细对象的 [属性名称] 一定要跟明细表的 [类名] 一样
                if (pi != null)
                {
                    if (pi.PropertyType.IsClass)
                    {
                        if (pi.PropertyType.Name == table)
                        {
                            //try
                            //{
                            string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                            string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                            if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                            {
                                try
                                {
                                    string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                    string sql2 = "select " + childColumn + " from " + table + " where " + foreignkeyname + "='" + value + "'";
                                    t = GetChildClass(t, sql2);
                                    pi.SetValue(entity, t, null);
                                }
                                catch (Exception e)
                                {
                                    error = pi.PropertyType.Name + ":" + e.Message;
                                    Result.Error += error + ";";
                                }

                            }
                            //}
                            //catch (Exception e)
                            //{
                            //    error = e.Message;
                            //    Result.Error += error+";";
                            //}
                        }
                    }
                }
                #endregion
                #region 多个实体
                foreach (PropertyInfo p in type.GetProperties())
                {
                    if (p.PropertyType.IsClass && p.PropertyType.UnderlyingSystemType.Name == "List`1")
                    {
                        string A = p.ToString();//System.Collections.Generic.List`1[SMT.Test.SMT_TEST_DETAIL] SMT_TEST_DETAIL2
                        string ptablename = A.Substring(0, A.IndexOf(']')).Substring(A.LastIndexOf('.') + 1);
                        if (table == ptablename)
                        {
                            try
                            {
                                #region
                                List<TEntity> list = new List<TEntity>();
                                string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                                string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称

                                if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                                {
                                    string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                    string sql2 = "select " + childColumn + " from " + table + " where " + foreignkeyname + "='" + value + "'";
                                    DataTable dtt = SMT.DataProvider.GetDataTable(sql2).DataTable;
                                    for (int i = 0; i < dtt.Rows.Count; i++)
                                    {
                                        t = new TEntity();
                                        foreach (PropertyInfo p2 in t.GetType().GetProperties())
                                        {
                                            if (dtt.Columns.Contains(p2.Name))
                                            {
                                                p2.SetValue(t, SMTProperty.SetPropertyValue(p2, dtt.Rows[i][p2.Name]), null);
                                                try
                                                {
                                                    p2.SetValue(t, SMTProperty.SetPropertyValue(p2, dtt.Rows[i][p2.Name]), null);
                                                }
                                                catch (Exception e)
                                                {
                                                    error = p2.Name + ":" + e.Message;
                                                    Result.Error += error + ";";
                                                }
                                            }
                                        }
                                        list.Add((TEntity)t);
                                    }
                                    p.SetValue(entity, list, null);
                                }
                                #endregion
                            }
                            catch (Exception e)
                            {
                                error = e.Message;
                                Result.Error += error + ";";
                            }
                        }
                    }
                }

                #endregion
                #region DatatTable
                foreach (PropertyInfo p in type.GetProperties())
                {
                    if (p.PropertyType.Name.ToLower() == "datatable")
                    {   
                        datatableObjectName = p.Name;
                        if (datatableObjectName.Substring(0, datatableObjectName.LastIndexOf('_')) == table)
                        {
                            PropertyInfo dp = type.GetProperty(datatableObjectName);
                            if (dp.PropertyType.IsClass)
                            {
                                try
                                {
                                    string foreignkey = t.GetType().GetProperty("ForeignKey").GetValue(t, null).ToString();//外键字段名称
                                    string foreignkeyname = t.GetType().GetProperty("ForeignKeyName").GetValue(t, null).ToString();//从表储存外键值字段名称
                                    if (type.GetProperty(foreignkey).GetValue(entity, null) != null)
                                    {
                                        try
                                        {
                                            string value = type.GetProperty(foreignkey).GetValue(entity, null).ToString();//外键字段值
                                            string sqld = "select " + childColumn + " from " + table + " where " + foreignkeyname + "='" + value + "'";
                                            dp.SetValue(entity, SMT.DataProvider.GetDataTable(sqld).DataTable, null);
                                        }
                                        catch (Exception e)
                                        {
                                            error = p.PropertyType.Name + ":" + e.Message;
                                            Result.Error += error + ";";
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    error = e.Message;
                                    Result.Error += error + ";";
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// 通过SQL语得到一个实体
        /// </summary>
        /// <typeparam name="TEntity">实体类</typeparam>
        /// <param name="t">实体类对象</param>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public DataContext GetEntity<TEntity>(TEntity t, string sql)// where TEntity : class
        {
            string error = "";
            entity = t;       
            Result = GetEntityResult(SMT.DataProvider.GetDataReader(sql));
            IDataReader dt = Result.IDataReader;
            if (dt == null)
            {
                return this;
            }
            Type type = t.GetType();
            if (dt.Read())
            {
                Result.RecordCount = 1;
                Hashtable has = new Hashtable();
                for (int i = 0; i < dt.FieldCount; i++)
                {
                    has.Add(dt.GetName(i).ToLower(), dt.GetValue(i));
                }
              
                try
                {
                    foreach (PropertyInfo p in type.GetProperties())
                    {
                        if (has.Contains(p.Name.ToLower()))
                        {
                            error = p.Name + "=" + has[p.Name.ToLower()];
                            p.SetValue(t, SMTProperty.SetPropertyValue(p, has[p.Name.ToLower()]), null);
                        }
                    }
                }
                catch (Exception e)
                {
                    error = e.Message;
                    Result.Error += error + ";";
                }
            }
            dt.Close();
            entityList = null;//单个实体与实体列表不以冲突，所以清空
            return this;
        }
        /// <summary>
        /// 通过SQL语得到一个实体列表
        /// </summary>
        /// <typeparam name="TEntity">主表实体类</typeparam>
        /// <param name="list">List对象</param>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        public DataContext GetEntity<TEntity>(List<TEntity> list,string sql) where TEntity:new()
        {
            entityList = new List<object>();
            string error = "";
            entity = new TEntity();
            Type type = entity.GetType();
            Result = GetEntityResult(SMT.DataProvider.GetDataTable(sql));
            DataTable dt = Result.DataTable;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    entity = new TEntity();
                    foreach (PropertyInfo p in type.GetProperties())
                    {
                        if (dt.Columns.Contains(p.Name))
                        {
                            p.SetValue(entity, SMTProperty.SetPropertyValue(p, dt.Rows[i][p.Name]), null);
                        }
                    }
                    list.Add((TEntity)entity);
                    entityList.Add((TEntity)entity);
                }
                catch (Exception e)
                {
                    error = e.Message;
                    Result.Error += error + ";";
                }
            }
           
       return this;
       
        }
        /// <summary>
        /// 分页绑定
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="list">实体集合对像</param>
        /// <param name="sql">SQL语句</param>
        /// <param name="index">页索引</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns></returns>
        public DataContext GetEntityByPaging<TEntity>(List<TEntity> list, string sql, int index, int pageSize, out int pageCount, out int rowCount) where TEntity : new()
        {
            entityList = new List<object>();
            string error = "";
            entity = new TEntity();
            Type type = entity.GetType();
            Result = GetEntityResult(SMT.SMTPaging.BindPaging(sql,index,pageSize,out pageCount,out rowCount));
            DataTable dt = Result.DataTable;
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        entity = new TEntity();
                        foreach (PropertyInfo p in type.GetProperties())
                        {
                            if (dt.Columns.Contains(p.Name))
                            {
                                p.SetValue(entity, SMTProperty.SetPropertyValue(p, dt.Rows[i][p.Name]), null);
                            }
                        }
                        list.Add((TEntity)entity);
                        entityList.Add((TEntity)entity);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        Result.Error += error + ";";
                    }
                }
            }
            return this;

        }
        /// <summary>
        /// 分页绑定
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="list">实体集合对像</param>     
        /// <param name="index">页索引</param>
        /// <param name="pageSize">每页显示记录数</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns></returns>
        public DataContext GetEntityByPaging<TEntity>(List<TEntity> list, int index, int pageSize, out int pageCount, out int rowCount) where TEntity : new()
        {
            entityList = new List<object>();
            string error = "";
            entity = new TEntity();
            Type type = entity.GetType();
            string sql = "SELECT * FROM "+type.Name;
            Result = GetEntityResult(SMT.SMTPaging.BindPaging(sql, index, pageSize, out pageCount, out rowCount));
            DataTable dt = Result.DataTable;
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        entity = new TEntity();
                        foreach (PropertyInfo p in type.GetProperties())
                        {
                            if (dt.Columns.Contains(p.Name))
                            {
                                p.SetValue(entity, SMTProperty.SetPropertyValue(p, dt.Rows[i][p.Name]), null);
                            }
                        }
                        list.Add((TEntity)entity);
                        entityList.Add((TEntity)entity);
                    }
                    catch (Exception e)
                    {
                        error = e.Message;
                        Result.Error += error + ";";
                    }
                }
            }
            return this;

        }
        #region 返回T

        private object GetChildClass(object t, string sql)
        {
            string error = "";
            Result = GetEntityResult(SMT.DataProvider.GetDataReader(sql));
            IDataReader dr = Result.IDataReader;
            if (dr == null)
            {
                return t;
            }
            Type type = t.GetType();
            if (dr.Read())
            {
                try
                {
                    Hashtable has = new Hashtable();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        has.Add(dr.GetName(i).ToLower(), dr.GetValue(i));
                    }
                    foreach (PropertyInfo p in type.GetProperties())
                    {
                        if (has.Contains(p.Name.ToLower()))
                        {
                            p.SetValue(t, SMTProperty.SetPropertyValue(p, has[p.Name.ToLower()]), null);
                        }
                    }
                }
                catch (Exception e)
                {
                    error = e.Message;
                    Result.Error += error + ";";
                }
            }
            dr.Close();
            return t;
        }      
        #endregion
        private EntityResult GetEntityResult(SMT.DataResult dresult)
        {
            //EntityResult Result = new EntityResult();
            Result.DataSet = dresult.DataSet;
            Result.DataTable = dresult.DataTable;
            if (Result.Sql == null || Result.Sql.IndexOf(dresult.Sql) < 0)
            {
                Result.Sql += (string.IsNullOrEmpty(dresult.Sql)) ? "" : dresult.Sql + "       ";
            }
            //Result.Sql += (string.IsNullOrEmpty(dresult.Sql)) ? "" : dresult.Sql + "       ";
            Result.IDataReader = dresult.IDataReader;
            Result.IDbDataParameter = dresult.IDbDataParameter;
            Result.RecordCount += dresult.RecordCount;
            Result.Error += (string.IsNullOrEmpty(dresult.Error)) ? "" : dresult.Error + "       "; 
            return Result;

        }
    }
    #endregion  
}
