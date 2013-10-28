using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using SMT.HRM.BLL;

namespace SMT.HRM.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class HRAjaxSv
    {
        
        #region 免打卡人员设置
 


        #endregion
        // 要使用 HTTP GET，请添加 [WebGet] 特性。(默认 ResponseFormat 为 WebMessageFormat.Json)
        // 要创建返回 XML 的操作，
        //     请添加 [WebGet(ResponseFormat=WebMessageFormat.Xml)]，
        //     并在操作正文中包括以下行:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        [OperationContract]
        public void DoWork()
        {
            // 在此处添加操作实现
            return;
        }

        List<UserInfo> userList = new List<UserInfo>
        {
            new UserInfo {Name = "赵二", Address = "北京", Age = 2, IsMember = true},
            new UserInfo {Name = "张三", Address = "北京", Age = 3, IsMember = false},
            new UserInfo {Name = "李四", Address = "北京", Age = 4, IsMember = true},
            new UserInfo {Name = "王五", Address = "北京", Age = 5, IsMember = false},
        };

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetUser")]
        public UserInfo GetUser()
        {
            return new UserInfo() { Name = "张三", Address = "北京", Age = 3, IsMember = true };
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetUserListByAge/{age}")]
        public UserInfo GetUserListByAge(string age)
        {
            return (from u in userList
                    where u.Age == int.Parse(age)
                    select new UserInfo
                    {
                        Age = u.Age,
                        Name = u.Name,
                        IsMember = u.IsMember,
                        Address = u.Address
                    }).SingleOrDefault();
        }

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "GetUserList")]
        public List<UserInfo> GetUserList()
        {
            return userList;
        }


        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            List<Product> products = new List<Product>(){
                    new Product(){Name="苹果",Price=5.5},
                    new Product(){Name="橘子",Price=2.5},
        new Product(){Name="干柿子",Price=16.00}
                    };
            ProductList productlist = new ProductList();
            productlist.GetProducts = products;
            context.Response.Write(new JavaScriptSerializer().Serialize(productlist));
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        // 在此处添加更多操作并使用 [OperationContract] 标记它们
    }

    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class ProductList
    {
        public List<Product> GetProducts { get; set; }
    }

    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public int Age { get; set; }
        [DataMember]
        public bool IsMember { get; set; }

    }
}
