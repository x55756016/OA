/*
版权信息：SMT
作    者：向寒咏
日    期：2009-09-22
内容摘要： 数据访问工厂
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Collections;
using System.Threading;
using SMT.Foundation.Log;
using System.Data.Objects;

namespace SMT.Foundation.Core
{
    public class DALFacoty
    {
        private static string DataBaseType = ConfigurationManager.AppSettings["DataBaseType"];
        private static string DALAssemblyPath = ConfigurationManager.AppSettings["DALAssemblyPath"];
        public static string DBContextName = ConfigurationManager.AppSettings["DBContextName"];
        private static Mutex muxConsole = new Mutex();
        //创建一个线程安全的Hashtable
        public static Hashtable contenxtCache = Hashtable.Synchronized(new Hashtable());
        //private static IDAL dalInstance;
        private static object syncRoot=new object();
        private static int InstanceCount=0;

        private DALFacoty()
        {            
        }
        //LogManager loger = new LogManager();
        /// <summary>
        /// 根据数据库类型创建不同datacontext用于linq操作
        /// </summary>
        /// <param name="DataBaseType"></param>
        /// <returns></returns>
        public static IDAL CreateDataContext()
        {
            IDAL dalInstance=null;
            lock (syncRoot)
            {
                muxConsole.WaitOne();
                if (contenxtCache.Count > 0)
                {
                    try
                    {
                        //Tracer.Debug("There DataContext  " + contenxtCache.Count);                    
                        foreach (DictionaryEntry de in contenxtCache)
                        {
                            ObjectContext obj = de.Value as ObjectContext;
                            lock (obj)
                            {
                                if (obj.Connection.State == System.Data.ConnectionState.Closed)
                                {
                                    obj.Connection.Open();
                                }
                                InstanceCount = InstanceCount+1;
                                Tracer.Debug("Load DataContext "+InstanceCount.ToString()+" Times"+" From Memory");
                                dalInstance = de.Value as IDAL;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Tracer.Debug(ex.Message);
                    }
                }
                if (dalInstance == null)
                {
                    //强制回收,触发BaseDAL析构函数.
                    //GC.Collect();
                    string className = string.Empty;
                    switch (DataBaseType)
                    {
                        case "Oracle":
                            className = DALAssemblyPath + "." + DBContextName;
                            break;
                        case "SQLServer":
                            className = DALAssemblyPath + ".SqlEntityFrameworkContext";
                            break;
                        case "MySql":
                            className = DALAssemblyPath + ".MySqlEntityFrameworkContext";
                            break;
                        default:
                            className = DALAssemblyPath + "." + DBContextName;
                            break;
                    }
                    string threadName = Guid.NewGuid().ToString();
                    lock (syncRoot)
                    {
                        if(string.IsNullOrEmpty(Thread.CurrentThread.Name))Thread.CurrentThread.Name = threadName;
                    }
                    Assembly asm = Assembly.Load(DALAssemblyPath);
                    dalInstance = (IDAL)asm.CreateInstance(className);
                    contenxtCache.Add(threadName, dalInstance);
                    Tracer.Debug("Create DalInstance: " + threadName);

                }
                muxConsole.ReleaseMutex();
                return dalInstance;
            }
        }

        public static IDAL CreateDataContextEveryTime(string DalID)
        {
            string typeName = string.Empty;
            string dataBaseType = DataBaseType;
            if (dataBaseType != null)
            {
                if (!(dataBaseType == "Oracle"))
                {
                    if (dataBaseType == "SQLServer")
                    {
                        typeName = DALAssemblyPath + ".SqlEntityFrameworkContext";
                        goto Label_0092;
                    }
                    if (dataBaseType == "MySql")
                    {
                        typeName = DALAssemblyPath + ".MySqlEntityFrameworkContext";
                        goto Label_0092;
                    }
                }
                else
                {
                    typeName = DALAssemblyPath + "." + DBContextName;
                    goto Label_0092;
                }
            }
            typeName = DALAssemblyPath + "." + DBContextName;
        Label_0092:
            return (IDAL)Assembly.Load(DALAssemblyPath).CreateInstance(typeName);
        }

 

        public static void ClearCache(string threadName)
        {
            contenxtCache.Remove(threadName);
        }
        //private static IDAL CreateSQLDal()
        //{
        //    string className = DALAssemblyPath + ".DALDataContext";
        //    object dal = Assembly.Load(DALAssemblyPath).CreateInstance(className);
        //    return (IDAL)dal;
        //}


        /// <summary>
        /// 根据数据库类型创建不同datacontext用于linq操作(泛型类反射)
        /// </summary>
        /// <param name="DataBaseType"></param>
        /// <returns></returns>
        //public static IDAL<TEntity> CreateDataContext(string DataBaseType)
        //{
        //    string className = string.Empty;
        //    switch (DataBaseType)
        //    {
        //        case "Oracle":
        //            className = DALAssemblyPath + ".EntityFrameworkOracleContext";
        //            break;
        //        case "SQLServer":
        //            className = DALAssemblyPath + ".SqlEntityFrameworkContext";
        //            break;
        //        case "MySql":
        //            className = DALAssemblyPath + ".MySqlEntityFrameworkContext";
        //            break;
        //        default:
        //            className = DALAssemblyPath + ".EntityFrameworkOracleContext";
        //            break;
        //    }

        //    IDAL<TEntity> dalInstance = null;
        //    Assembly asm = Assembly.Load(DALAssemblyPath);
        //    //获取程序集中所有的类。 
        //    Type[] types = asm.GetTypes();
        //    //遍历类集合。 
        //    foreach (Type typeX in types)
        //    {
        //        //若该类型为泛型类。 
        //        if (typeX.IsGenericType)
        //        {
        //            //创建泛型类，其模版类以参数形式添加到该类型。 
        //            Type t = typeX.MakeGenericType(typeof(TEntity));
        //            //使用Activator类创建该类型的实例。 
        //            dalInstance = (IDAL<TEntity>)Activator.CreateInstance(t);
        //            //return dalInstance;
        //            break;
        //        }
        //    }
        //    //IDAL<TEntity> dalInstance= (IDAL<TEntity>)dal;
        //    return dalInstance;

        //}
    }

    public class SingleDALFacoty
    {
        private static string DataBaseType = ConfigurationManager.AppSettings["DataBaseType"];
        private static string DALAssemblyPath = ConfigurationManager.AppSettings["DALAssemblyPath"];
        private static string DBContextName = ConfigurationManager.AppSettings["DBContextName"];

        #region 单例模式

        private static readonly IDAL instance = CreateDataContext();

        private SingleDALFacoty() { }

        public static IDAL Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        //创建一个线程安全的Hashtable
        public static Hashtable contenxtCache = Hashtable.Synchronized(new Hashtable());
        //LogManager loger = new LogManager();
        /// <summary>
        /// 根据数据库类型创建不同datacontext用于linq操作
        /// </summary>
        /// <param name="DataBaseType"></param>
        /// <returns></returns>
        public static IDAL CreateDataContext()
        {
            IDAL dalInstance = null;
            string className = string.Empty;
            switch (DataBaseType)
            {
                case "Oracle":
                    className = DALAssemblyPath + "." + DBContextName;
                    break;
                case "SQLServer":
                    className = DALAssemblyPath + ".SqlEntityFrameworkContext";
                    break;
                case "MySql":
                    className = DALAssemblyPath + ".MySqlEntityFrameworkContext";
                    break;
                default:
                    className = DALAssemblyPath + "." + DBContextName;
                    break;
            }


            //threadName = Guid.NewGuid().ToString();
            Thread.CurrentThread.Name = Guid.NewGuid().ToString();
            Assembly asm = Assembly.Load(DALAssemblyPath);
            // Type dalType = Type.GetType(className);
            dalInstance = (IDAL)asm.CreateInstance(className);
            //contenxtCache.Add(threadName, dalInstance);
            Tracer.Debug("New DataContext" + "Thread.CurrentThread.Name");
            //强制回收,触发BaseDAL析构函数.
            //GC.Collect();               
            //System.Diagnostics.Trace.TraceInformation("2 threadName null,new dalInstance: threadName:");            
            //}
            return dalInstance;
        }

        public static void ClearCache(string threadName)
        {
            contenxtCache.Remove(threadName);
        }

    }
}


