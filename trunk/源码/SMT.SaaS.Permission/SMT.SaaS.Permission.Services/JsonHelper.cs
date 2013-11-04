using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using SMT.SaaS.Permission.DAL;
using SMT.SaaS.Permission.DAL.views;

namespace SMT.SaaS.Permission.Services
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class UserMenuPermission
    {
        /// <summary>
        /// 用户角色菜单权限
        /// </summary>
        [DataMember]
        List<V_BllCommonUserPermission> userPermissons;
        /// <summary>
        /// 用户角色菜单自定义权限
        /// </summary>
        [DataMember]
        List<CustomerPermission> userCustomerPermission;
    }

    public class JsonHelper
    {
        /// <summary>
        /// 生成Json格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetJson<T>(T obj)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(obj.GetType());
            using (MemoryStream stream = new MemoryStream())
            {
                json.WriteObject(stream, obj);
                string szJson = Encoding.UTF8.GetString(stream.ToArray()); return szJson;
            }
        }
        /// <summary>
        /// 获取Json的Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="szJson"></param>
        /// <returns></returns>
        public static T ParseFromJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                return (T)serializer.ReadObject(ms);
            }



        }
    }
}