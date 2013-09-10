using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Text;

namespace SMT.SaaS.OA.Services
{
    public class SerializerHelper
    {
        public T XmlToContractObject<T>(string xml) where T : class
        {
            MemoryStream memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(xml));
            using (
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memoryStream, Encoding.Unicode,
                       new XmlDictionaryReaderQuotas(), null))
            {
                DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
                return dataContractSerializer.ReadObject(reader) as T;
            }
        }
        public string ContractObjectToXml<T>(T obj) where T : class
        {
            DataContractSerializer dataContractSerializer = new DataContractSerializer(obj.GetType());

            String text;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractSerializer.WriteObject(memoryStream, obj);
                byte[] data = new byte[memoryStream.Length];
                Array.Copy(memoryStream.GetBuffer(), data, data.Length);

                text = Encoding.UTF8.GetString(data);
            }
            return text;
        }

        public static object XmlToContractObject(string xml, Type type)
        {
            MemoryStream memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(xml));
            using (
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memoryStream, Encoding.Unicode,
                       new XmlDictionaryReaderQuotas(), null))
            {
                DataContractSerializer dataContractSerializer = new DataContractSerializer(type);
                return dataContractSerializer.ReadObject(reader);
            }
        }
        public static string ContractObjectToXml(object obj)
        {
            DataContractSerializer dataContractSerializer = new DataContractSerializer(obj.GetType());

            String text;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractSerializer.WriteObject(memoryStream, obj);
                byte[] data = new byte[memoryStream.Length];
                Array.Copy(memoryStream.GetBuffer(), data, data.Length);

                text = Encoding.UTF8.GetString(data);
            }
            return text;
        }
    }
}
