/*�Ŀ¼������� Beta 1.0 
 * ����
 * 2005-4-27
 * */

using System;
using System.DirectoryServices;
using System.Text;

namespace InterActiveDirectory
{
	/// <summary>
	/// �����ࣺ�������������Ͳ���
	/// </summary>
	public class AD_Common
	{
		private  string ADPath=""; 
		private  string ADUser = "";
		private  string ADPassword = "";
		private  string ADServer="";


		/// <summary>
		/// Ĭ�Ϲ��캯������ʼ������AD�Ĳ���
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
		/// ���캯���Ķ�̬����ʼ������AD�Ĳ���
		/// </summary>
		/// <param name="_ADPath">AD·��</param>
		/// <param name="_ADUser">�˺�</param>
		/// <param name="_ADPassword">����</param>
		/// <param name="_ADServer">AD������</param>
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
		/// ��ȡָ��·����DirectoryEntry����
		/// </summary>
		/// <param name="DomainReference">ȫLDAP·��</param>
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
