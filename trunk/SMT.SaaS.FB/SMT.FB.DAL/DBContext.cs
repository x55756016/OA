using System;
using System.Collections.Generic;

using System.Data.EntityClient;
using SMT_FB_EFModel;
using System.Threading;
using System.Data.Common;
using System.Linq;

namespace SMT.FB.DAL
{
    public class DBContext
    {
        //static DBContext()
        //{
        //    contenxtCache = new Dictionary<string, SMT_FB_EFModelContext>();
        //    dictionaryThread = new Dictionary<string, List<object>>();
        //    dictionaryThread_Te = new Dictionary<object, string>();
        //    transactionList = new List<string>();
        //}
        ////public static Dictionary<string, SMT_FB_EFModelContext> contenxtCache;
        //public static Dictionary<string, List<object>> dictionaryThread;
        //public static Dictionary<object, string> dictionaryThread_Te;
        //public static List<string> transactionList;
        //private static object lockObj = new object();

        //public static SMT_FB_EFModelContext CreateObjectContext()
        //{
        //    return new SMT_FB_EFModelContext();
        //}
        //public static SMT_FB_EFModelContext GetObjectContext(object obj)
        //{
        //    SMT_FB_EFModelContext dbinstance = new SMT_FB_EFModelContext();
        //    //string threadName = Thread.CurrentThread.Name;
        //    //if (threadName!= null &&  contenxtCache.ContainsKey(threadName))
        //    //{
        //    //    dbinstance = contenxtCache[threadName];
        //    //}
        //    //else
        //    //{
        //    //    if (threadName == null)
        //    //    {
        //    //        threadName = Guid.NewGuid().ToString();
        //    //        Thread.CurrentThread.Name = threadName;
        //    //    }
        //    //    dbinstance = new SMT_FB_EFModelContext();
        //    //    dbinstance.CommandTimeout = 180;
        //    //    //db.Log = Console.Out;
        //    //    //dbinstance.Log = Console.Out;
        //    //    //dbinstance.ObjectTrackingEnabled = false;
        //    //    contenxtCache.Add(threadName, dbinstance);
                
        //    //}
        //    //RegisterObj(obj);
        //    return dbinstance;
        //}

        //public static SMT_FB_EFModelContext GetObjectContext()
        //{
        //    //string threadName = Thread.CurrentThread.Name;
        //    //if (threadName != null)
        //    //{
        //    //    return contenxtCache[threadName];
        //    //}
        //    //else
        //    //{
        //        return new SMT_FB_EFModelContext();
        //    //}
        //}

        //private static void RegisterObj(object obj)
        //{
        //    string threadName = Thread.CurrentThread.Name;
        //    List<object> list = null;
        //    if (dictionaryThread.ContainsKey(threadName))
        //    {
        //        list = dictionaryThread[threadName];
        //    }
        //    else
        //    {
        //        list = new List<object>();
        //        dictionaryThread.Add(threadName, list);
        //    }
        //    list.Add(obj);
        //}

        //public static void UnregisterObj(object obj)
        //{
        //   //  SMT.Foundation.Log.Tracer.Warn(System.DateTime.Now.ToString("yyyy-MM-dd") + " : " + "释放ObjectContext");
        //    string threadName = Thread.CurrentThread.Name;
        //    if (threadName == null || !dictionaryThread.ContainsKey(threadName))
        //    {
        //        return;
        //    }

        //    List<object> list = dictionaryThread[threadName];
        //    list.Remove(obj);
        //    if (list.Count == 0)
        //    {
        //        dictionaryThread.Remove(threadName);
        //        SMT_FB_EFModelContext tempDC = contenxtCache[threadName];
        //        tempDC.Dispose();
        //        GC.SuppressFinalize(tempDC);
        //        tempDC = null;
        //        contenxtCache.Remove(threadName);
        //        Thread.CurrentThread.Name = null;
        //        SMT.Foundation.Log.Tracer.Warn("UnregisterObj, 释放:" + threadName);
        //    }

        //}

        //public static string ManualRegister()
        //{
        //    lock (lockObj)
        //    {
        //        string threadName = Thread.CurrentThread.Name;
        //        if (string.IsNullOrEmpty(threadName))
        //        {
        //            threadName = Guid.NewGuid().ToString();
        //            Thread.CurrentThread.Name = threadName;
        //        }

        //        if (!contenxtCache.ContainsKey(threadName))
        //        {
        //            SMT_FB_EFModelContext dbinstance = new SMT_FB_EFModelContext();
        //            dbinstance.CommandTimeout = 180;
        //            contenxtCache.Add(threadName, dbinstance);
        //        }
        //        return threadName;
        //    }
            
        //}

        //public static void ManualUnRegister(string threadName)
        //{
        //    lock (lockObj)
        //    {
        //        if (contenxtCache.ContainsKey(threadName))
        //        {
        //            SMT_FB_EFModelContext tempDC = contenxtCache[threadName];
        //            //if (tempDC.Connection.State == System.Data.ConnectionState.Open)
        //            //{
        //            //    tempDC.Connection.Close();
        //            //}
                    
        //            contenxtCache.Remove(threadName);
        //            GC.SuppressFinalize(tempDC);
                    
        //        }
        //    }
        //}

        #region 事务中不嵌套事务
        public static DbTransaction BeginTransaction(SMT_FB_EFModelContext mc)
        {
            //string threadName = Thread.CurrentThread.Name;
            //if (transactionList.Contains(threadName))
            //{
            //    return null;
            //}
            //transactionList.Add(threadName);
            if (mc.Connection.State == System.Data.ConnectionState.Closed)
            {
                mc.Connection.Open();
            }
            DbTransaction dbt = mc.Connection.BeginTransaction();
            return dbt;
        }
        public static void CommitTransaction(DbTransaction dbt)
        {
            if (dbt == null)
            {
                return;
            }
            dbt.Commit();
            //string threadName = Thread.CurrentThread.Name;
            //if (threadName != null)
            //{
            //    transactionList.Remove(threadName);
            //}
        }
        public static void RollbackTransaction(DbTransaction dbt)
        {
            if (dbt == null)
            {
                return;
            }
            if (dbt.Connection.State == System.Data.ConnectionState.Closed)
            {
                return;
            }
            dbt.Rollback();
            //if (transactionList.Contains(threadName))
            //{
            //    dbt.Rollback();
            //    transactionList.Remove(threadName);
            //}
        }
        #endregion
    }
}
