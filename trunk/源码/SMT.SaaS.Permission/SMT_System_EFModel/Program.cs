using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_System_EFModel;
using SMT.Foundation.Core;
using System.Configuration;

namespace SMT_System_EFModel
{
    class Program
    {
        public static object ExecuteCustomerSql(string Sqlstring, ParameterCollection prameters)
        {
            string conn = ConfigurationManager.AppSettings["ConnectionString"].ToString();
            OracleDAO dao = new OracleDAO(conn);
            //prameters = new ParameterCollection();
            //prameters.Add("parme1", null);

            dao.ExecuteScalar(Sqlstring, System.Data.CommandType.Text);
            prameters = new ParameterCollection();
            dao.GetDataTable(Sqlstring, System.Data.CommandType.Text, prameters);

            return null;
        }

        static void Main(string[] args)
        {
            string strSql = @"select 
                                c.cname,
                                dd.departmentname,
                                pd.postname,
                                e.employeecname,
                                ei.entrydate,
                                pm.cardid,
                                ei.employeeentryid,
                                e.employeeid,
                                ep.employeepostid,
                                p.postid,
                                d.departmentid,
                                c.companyid
                                from t_hr_company c
                                inner join t_hr_department d on c.companyid =d.companyid
                                inner join t_hr_departmentdictionary dd on d.departmentdictionaryid=dd.departmentdictionaryid
                                inner join t_hr_post p on d.departmentid=p.departmentid
                                inner join t_hr_postdictionary pd on p.postdictionaryid=pd.postdictionaryid
                                inner join t_hr_employeepost ep on ep.postid=p.postid
                                inner join t_hr_employee e on e.employeeid=ep.employeeid
                                left join T_HR_EmployeeEntry ei on e.employeeid=ei.employeeid
                                left join T_HR_PensionMaster pm on e.employeeid=pm.employeeid
                                --where e.employeecname='初始化管理员'
                                order by c.cname";
            object obj = ExecuteCustomerSql(strSql, null);


            using (SMT_System_EFModelContext context = new SMT_System_EFModelContext())
            {
                T_SYS_ROLE role = new T_SYS_ROLE();

                //create
                //T_HR_DICTIONARY entUser = new T_HR_DICTIONARY();
                //entUser.DICTIONARYID = "user1";
                //try
                //{
                //    context.AddToTEST_USERS(entUser);
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{

                //}
                //read
                var ta = from a in context.T_SYS_USER
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
