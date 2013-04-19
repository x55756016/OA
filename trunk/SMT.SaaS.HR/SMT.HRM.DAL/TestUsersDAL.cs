using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT.Foundation.Core;
using System.Data.Objects;
using System.Data.EntityClient;
using System.Data;

namespace SMT.HRM.DAL
{
    public class TESTUSERSDAL : BaseDAL
    {
        //public object CustomerQuery(string Sqlstring)
        //{
        //    //return  lbc.GetDataContext().CreateQuery<TEST_USERS>(Sqlstring);

        //    //string con = "name = EntityFrameworkOracleContext";
        //    using (EntityConnection econn = new EntityConnection(lbc.GetDataContext().MetadataWorkspace, lbc.GetDataContext().Connection))
        //    {
        //        econn.Open();
        //        EntityCommand ecmd = new EntityCommand(Sqlstring, econn);

        //        return ecmd.ExecuteScalar();
        //    }
        //    // 自字典查询第2种方式
        //    //using (var edm = lbc.GetDataContext())
        //    //{

        //    //    string esql = "select value c from NorthwindEntities.Customers as c ";

        //    //    ObjectQuery<TEST_USERS> query1 = edm.CreateQuery<TEST_USERS>(esql);

        //    //    //使用ObjectParameter的写法 
        //    //    query1 = query1.Where("it.CustomerId=@customerid");
        //    //    query1.Parameters.Add(new ObjectParameter("customerid", "ALFKI"));

        //    //    //也可以这样写
        //    //    //ObjectQuery<Customers> query2 = edm.Customers.Where("it.CustomerID='ALFKI'");
        //    //    foreach (var c in query1)
        //    //        Console.WriteLine(c.USERNAME);
        //    //    //显示查询执行的SQL语句
        //    //    Console.WriteLine(query1.ToTraceString());
        //    //}
        //}


        //public void CustomerSelect()
        //{            
        //    using (EntityConnection econn = new EntityConnection(lbc.GetDataContext().MetadataWorkspace, lbc.GetDataContext().Connection))
        //    {
        //        string esql = "select * from dnt_users where bday > '1953'";
        //        econn.Open();
        //        EntityCommand ecmd = new EntityCommand(esql, econn);

        //        EntityDataReader ereader = ecmd.ExecuteReader(CommandBehavior.SequentialAccess);
        //        if (ereader.Read())
        //        {
        //            Console.WriteLine(ereader["CustomerID"]);
        //        }
        //        ecmd.ExecuteNonQuery();
        //        ecmd.ExecuteScalar();

        //    }

        //    // 自字典查询第2种方式
        //    //using (var edm = lbc.GetDataContext())
        //    //{

        //    //    string esql = "select value c from NorthwindEntities.Customers as c ";

        //    //    ObjectQuery<TEST_USERS> query1 = edm.CreateQuery<TEST_USERS>(esql);

        //    //    //使用ObjectParameter的写法 
        //    //    query1 = query1.Where("it.CustomerId=@customerid");
        //    //    query1.Parameters.Add(new ObjectParameter("customerid", "ALFKI"));

        //    //    //也可以这样写
        //    //    //ObjectQuery<Customers> query2 = edm.Customers.Where("it.CustomerID='ALFKI'");
        //    //    foreach (var c in query1)
        //    //        Console.WriteLine(c.USERNAME);
        //    //    //显示查询执行的SQL语句
        //    //    Console.WriteLine(query1.ToTraceString());
        //    //}
        //}

        //public void selectWithPrameter()
        //{
        //    using (EntityConnection econn = new EntityConnection(lbc.GetDataContext().MetadataWorkspace, lbc.GetDataContext().Connection))
        //    {
        //        string esql = "Select value c from NorthwindEntities.Customers as c order by c.CustomerID skip @start limit @end";
        //        econn.Open();
        //        EntityCommand ecmd = new EntityCommand(esql, econn);
        //        EntityParameter p1 = new EntityParameter("start", DbType.Int32);
        //        p1.Value = 0;
        //        EntityParameter p2 = new EntityParameter("end", DbType.Int32);
        //        p2.Value = 10;
        //        ecmd.Parameters.Add(p1);
        //        ecmd.Parameters.Add(p2);
        //        EntityDataReader ereader = ecmd.ExecuteReader(CommandBehavior.SequentialAccess);
        //        while (ereader.Read())
        //        {
        //            Console.WriteLine(ereader["CustomerID"]);
        //        }
        //        Console.WriteLine(ecmd.ToTraceString());
        //    }
        //}

        public void EntityConnect()
        {
            EntityConnectionStringBuilder esb = new EntityConnectionStringBuilder();
            esb.Provider = "System.Data.SqlClient";
            esb.Metadata = @"res://*/NorthWind.csdl|res://*/NorthWind.ssdl|res://*/NorthWind.msl";
            esb.ProviderConnectionString = @"Data Source=.SQLEXPRESS;Initial Catalog=Northwind;Integrated Security=True;MultipleActiveResultSets=True";
            EntityConnection econn = new EntityConnection(esb.ConnectionString);//创建连接
        }

        
    }
}
