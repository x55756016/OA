/*活动目录操作组件 Beta 1.0 
 * 樊帆
 * 2005-4-27
 * */

using System;
using System.DirectoryServices;
using System.Text;

namespace InterActiveDirectory
{
	/// <summary>
	/// 公共类：包含公共方法和参数
	/// </summary>
	public class AD_Common
	{
		private  string ADPath=""; 
		private  string ADUser = "";
		private  string ADPassword = "";
		private  string ADServer="";


		/// <summary>
		/// 默认构造函数，初始化访问AD的参数
		/// </summary>
		public AD_Common()
		{
			AD_Parameter adp=new AD_Parameter();

			ADPath=adp.ADPath; 
			ADUser = adp.ADUser;
			ADPassword = adp.ADPassword;
			ADServer=adp.ADServer;
		}

		/// <summary>
		/// 构造函数的多态，初始化访问AD的参数
		/// </summary>
		/// <param name="_ADPath">AD路径</param>
		/// <param name="_ADUser">账号</param>
		/// <param name="_ADPassword">密码</param>
		/// <param name="_ADServer">AD服务器</param>
		public AD_Common(string _ADPath,string _ADUser,string _ADPassword,string _ADServer)
		{
			ADPath=_ADPath;
			ADUser=_ADUser;
			ADPassword=_ADPassword;
			ADServer=_ADServer;
			
		}

		public  void SetProperty(DirectoryEntry oDE, string PropertyName, string PropertyValue)
		{
			//check if the value is valid, otherwise dont update
			if(PropertyValue !=string.Empty)
			{
				//check if the property exists before adding it to the list
				if(oDE.Properties.Contains(PropertyName))
				{
					oDE.Properties[PropertyName][0]=PropertyValue; 
				}
				else
				{
					oDE.Properties[PropertyName].Add(PropertyValue);
				}
			}
		}

		public  void SetProperty(DirectoryEntry oDE, string PropertyName,byte[] PropertyValue)
		{
			if(oDE.Properties.Contains(PropertyName))
			{
				oDE.Properties[PropertyName][0]=PropertyValue; 
			}
			else
			{
				oDE.Properties[PropertyName].Add(PropertyValue);
			}
		}

		public  string GetLDAPDomain()
		{
			StringBuilder LDAPDomain = new StringBuilder();
			//string[] LDAPDC =ConfigurationSettings.AppSettings["ADServer"].Split('.');
			string[] LDAPDC = ADServer.Split('.');
			for(int i=0;i < LDAPDC.GetUpperBound(0)+1;i++)
			{
				LDAPDomain.Append("DC="+LDAPDC[i]);
				if(i <LDAPDC.GetUpperBound(0))
				{
					LDAPDomain.Append(",");
				}
			}
			return LDAPDomain.ToString();
		}

		public   DirectoryEntry GetDirectoryObject()
		{
			DirectoryEntry oDE;
			
			oDE = new DirectoryEntry(ADPath,ADUser,ADPassword,AuthenticationTypes.Secure);

			return oDE;
		}

		public  DirectoryEntry GetDirectoryObject(string DomainReference,int i,int j)
		{
			DirectoryEntry oDE =new DirectoryEntry();
			try
			{

				oDE = new DirectoryEntry(DomainReference,ADUser,ADPassword,AuthenticationTypes.ReadonlyServer);
			}
			catch(Exception ex)
			{
				ex.ToString();
			}

			return oDE;
		}



		public  DirectoryEntry GetDirectoryObject(string UserName, string Password)
		{
			DirectoryEntry oDE;
			
			oDE = new DirectoryEntry(ADPath,UserName,Password,AuthenticationTypes.Secure);

			return oDE;
		}
		
		public  DirectoryEntry GetDirectoryObject(string DomainReference)
		{
			DirectoryEntry oDE;
			
			oDE = new DirectoryEntry(ADPath + DomainReference,ADUser,ADPassword,AuthenticationTypes.Secure);

			return oDE;
		}

		/// <summary>
		/// 获取指定路径的DirectoryEntry对象
		/// </summary>
		/// <param name="DomainReference">全LDAP路径</param>
		/// <param name="i"></param>
		/// <returns></returns>
		public  DirectoryEntry GetDirectoryObject(string DomainReference,int i)
		{
			DirectoryEntry oDE;
			
			oDE = new DirectoryEntry(DomainReference,ADUser,ADPassword,AuthenticationTypes.Secure);

			return oDE;
		}
	}
}
