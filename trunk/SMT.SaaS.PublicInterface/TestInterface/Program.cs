using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {


                PublicServiceWS.PublicServiceClient Content = new PublicServiceWS.PublicServiceClient();

               string str= Content.GetContentFormatImgToUrl("c63e4df4-58ee-435e-84cb-12c983a26c78");

                //Content.AddContent("Test3", System.Text.Encoding.Default.GetBytes("sdfsfgwg"), "a", "Test", "Test", new PublicServiceWS.UserInfo{ COMPANYID="1",DEPARTMENTID="2",POSTID="3",USERID="4",USERNAME="5"});
                //var ss = Content.GetContent("Test3");

         //   InterfaceWS.BusinessObjectServiceClient BO = new InterfaceWS.BusinessObjectServiceClient();
            //string tmpXML = Content.GetBusinessObject("oa", "MeetingInfo");
            //Console.Write(tmpXML);
               Console.Write(str);
              
            }
            catch (Exception e)
            {

                throw e;
            }
            Console.ReadLine();
            
        }
    }
}
