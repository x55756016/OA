using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.Objects.DataClasses;


namespace SMT_OA_EFModel
{
    class Program
    {
        static void Main(string[] args)
        {

            using (SMT_OA_EFModelContext context = new SMT_OA_EFModelContext())
            {
              
                ////create
                //TEST_USERS entUser = new TEST_USERS();
                //entUser.USERNAME = "user1";
                //try
                //{
                //    context.AddToTEST_USERS(entUser);
                //    context.SaveChanges();
                //}
                //catch (Exception ex)
                //{

                //}
                //read
                //Flow_FlowRecord_T[] ss = context.get;


                //var ta = from a in context.T_OA_WELFARE
                //         select a;
                //ta.ToList();

                //T_OA_MEETINGTYPE t = (from a in context.T_OA_MEETINGTYPE
                //        where a.MEETINGTYPEID == "9fb6c8d2-340c-434e-b914-f9364de53603"
                //        select a).FirstOrDefault();

             //   T_OA_MEETINGTEMPLATE meetingtemplate = new T_OA_MEETINGTEMPLATE();
                //ee.MEETINGTEMPLATEID = Guid.NewGuid().ToString();


                        //meetingtemplate.MEETINGTEMPLATEID = System.Guid.NewGuid().ToString();
                        //meetingtemplate.TEMPLATENAME = "abc";

                        //meetingtemplate.T_OA_MEETINGTYPE = new T_OA_MEETINGTYPE();
                        //meetingtemplate.T_OA_MEETINGTYPE = t;
                        ////meetingtemplate.T_OA_MEETINGTYPEReference.EntityKey = new System.Data.EntityKey();

                        ////meetingtemplate.T_OA_MEETINGTYPEReference.EntityKey = new System.Data.EntityKey("SMT_OA_EFModelContext.T_OA_MEETINGTYPE", "MEETINGTYPEID", t.MEETINGTYPEID);

                        //meetingtemplate.CONTENT = "aa";

                        //meetingtemplate.OWNERCOMPANYID = "Common.loginUserInfo.companyID";
                        //meetingtemplate.OWNERDEPARTMENTID = "Common.loginUserInfo.companyID";
                        //meetingtemplate.OWNERID = "Common.loginUserInfo.companyID";
                        //meetingtemplate.OWNERNAME = "Common.loginUserInfo.companyID";
                        //meetingtemplate.OWNERPOSTID = "Common.loginUserInfo.companyID";
                        //meetingtemplate.CREATECOMPANYID ="Common.loginUserInfo.companyID";
                        //meetingtemplate.CREATEDEPARTMENTID = "Common.loginUserInfo.companyID";
                        //meetingtemplate.CREATEUSERNAME = "Common.loginUserInfo.companyID";
                        //meetingtemplate.CREATEPOSTID = "Common.loginUserInfo.companyID";
                        //meetingtemplate.CREATEUSERID = "Common.loginUserInfo.companyID";
                        //meetingtemplate.UPDATEUSERNAME = "";

                        //meetingtemplate.UPDATEDATE = null;
                        //meetingtemplate.UPDATEUSERID = "";
                        //meetingtemplate.CREATEDATE = System.DateTime.Now;

                        //context.AddObject("T_OA_MEETINGTEMPLATE", meetingtemplate);
                        //context.SaveChanges();


                //update 

                var t1 = from a in context.T_OA_VEHICLEDISPATCH.Include("T_OA_VEHICLEDISPATCHDETAIL.T_OA_VEHICLEUSEAPP")
                         where a.VEHICLEDISPATCHID == "9684e198-30a3-4918-bcfb-bc97759c7f2c"
                                              select a;



                        //T_OA_MEETINGTEMPLATE updateEntity = (from a in context.T_OA_MEETINGTEMPLATE
                        //                                     where a.MEETINGTEMPLATEID == "ce5f0774-4978-4e60-aaec-1247aa5ac1e7"
                        //                                     select a).FirstOrDefault();
                        //updateEntity.T_OA_MEETINGTYPE = t1;
                        //context.SaveChanges();

                //ee.T_OA_MEETINGTYPEReference.en

                //foreach (var d in ta)
                //{
                //    Console.WriteLine(d.CreateUserId + " " + d.EditUserId + " ");
                //}
                 
                Console.ReadLine();

                //delete
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


        public static void CloneEntity<T>(T sourceObj, T targetObj) where T : class
        {
            Type a = sourceObj.GetType();
            PropertyInfo[] infos = a.GetProperties();
            foreach (PropertyInfo prop in infos)
            {
                //System.Data.Objects.DataClasses.
                if (prop.PropertyType.BaseType == typeof(EntityReference)
                    || prop.PropertyType.BaseType == typeof(RelatedEnd)
                    || prop.PropertyType == typeof(System.Data.EntityState)
                    || prop.PropertyType == typeof(System.Data.EntityKey)
                    || prop.PropertyType.BaseType == typeof(System.Data.Objects.DataClasses.EntityObject))
                    continue;
                if (sourceObj is EntityObject)
                {
                    EntityObject ent = sourceObj as EntityObject;

                    if (ent != null && ent.EntityKey != null && ent.EntityKey.EntityKeyValues != null && ent.EntityKey.EntityKeyValues.Count() > 0)
                    {
                        bool isKeyField = false;
                        foreach (var key in ent.EntityKey.EntityKeyValues)
                        {
                            if (key.Key == prop.Name)
                            {
                                isKeyField = true;
                                break;
                            }
                        }
                        if (isKeyField)
                            continue;
                    }
                }
                //prop.Name
                object value = prop.GetValue(sourceObj, null);
                try
                {
                    prop.SetValue(targetObj, value, null);
                }
                catch (Exception ex)
                {
                    string e = ex.Message;
                }
            }
        }        
    }
}
