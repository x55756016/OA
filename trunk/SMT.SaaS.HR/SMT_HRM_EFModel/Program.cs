using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_HRM_EFModel;
using System.Data.EntityClient;
using System.Configuration;
using System.Data.Objects;
using System.Reflection;

namespace SMT_HRM_EFModel
{
    class Program
    {   

        static void Main(string[] args)
        {


//            using (EntityConnection econn = new EntityConnection(ConfigurationManager.ConnectionStrings["SMT_HRM_EFModelContext"].ToString()))
//            {

//                string Sqlstring = @"SMT_HRM_EFMODELCONTEXT.T_HR_COMPANY SET T_HR_COMPANY.CHECKSTATE = '1' 
//                WHERE T_HR_COMPANY.COMPANYID='ED73A65F-18BC-44A8-80A8-9621C019C574'";
//                EntityCommand ecmd = new EntityCommand(Sqlstring, econn);
//                econn.Open();

//                object a = ecmd.ExecuteScalar();
//            }



            using (SMT_HRM_EFModelContext context = new SMT_HRM_EFModelContext())
            {
                
                //var q=from ent in context.T_HR_EMPLOYEEOUTAPPLIECRECORD
                //System.Data.Common.DbConnection cn = context.Connection;
                //System.Data.Common.DbCommand dbcd = cn.CreateCommand();
                //dbcd.CommandText = "seleclt * from T_HR_COMPANY";
                //cn.Open();
                //object obj= dbcd.ExecuteScalar();
                //cn.Close();

                Dictionary<object, object> prameter = new Dictionary<object, object>();
                prameter.Add("CHECKSTATE", "4");
                //context.UpdateCheckState("SMT_HRM_EFModelContext.T_HR_COMPANY", "COMPANYID","4d9ed044-e0c3-4dfb-9a84-a4d91f9affdc",prameter);
                
                
                //create
                //T_HR_COMPANY entUser = new T_HR_COMPANY();
                //entUser.COMPANYID = Guid.NewGuid().ToString();
                //try
                //{
                //    context.AddObject(((object)entUser).GetType().Name, entUser);
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{
                //}
                //SMT_HRM_EFModelContext context2 = context.lbc;
                //read
                var ta = from a in context.T_HR_COMPANY
                        select a;
                 ta.ToList();
                
                 //foreach (var d in ta)
                 //{
                 //    Console.WriteLine(d.NICKNAME+" "+d.PASSWORD+" "+ d.USERNAME+" "+d.PASSWORD+" "+d.EMAIL);
                 //}
                 
                Console.ReadLine();

                ////delete
                //var deluser = (from a in context.TEST_USERS
                //               where a.USERNAME.Contains("entityFramwork2")
                //               select a).FirstOrDefault();
                //context.DeleteObject(deluser);
                //context.SaveChanges();

                //update
                 //var updateEnt = (from ent in context.TEST_USERS
                 //                where ent.USERNAME.Equals("entityFramwork2")
                 //                select ent).FirstOrDefault();
                 //updateEnt.PASSWORD = "entityFramwork22";
                 //context.SaveChanges();
                          
            }



        }
    }
}
