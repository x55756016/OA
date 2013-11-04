/*�Ŀ¼������� Beta 1.0 
 * ����
 * 2005-4-27
 * */


using System;

namespace InterActiveDirectory
{
	/// <summary>
	/// AD_Check ��ժҪ˵����
	/// </summary>
	public class AD_Check
	{

		private AD_seacher Iads;

		public AD_Check()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
			Iads=new AD_seacher();

		}

		public AD_Check(string _ADPath,string _ADUser,string _ADPassword,string _ADServer)
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
			Iads=new AD_seacher( _ADPath, _ADUser, _ADPassword, _ADServer);
		}


		/// <summary>
		/// ��֤���Ƿ����
		/// </summary>
		/// <returns></returns>
		public  bool CheckGroup(string cn)
		{
			string condition="(&(objectClass=group)(cn="+cn+"))";
			bool result=Iads.CommonWayBool(condition);
			return result;
		}

		/// <summary>
		/// ��֤��֯��Ԫ�Ƿ����
		/// </summary>
		/// <returns></returns>
		public  bool CheckUnit(string ou)
		{
			string condition= "(&(objectClass=organizationalunit)(ou="+ou+"))";

			bool result=Iads.CommonWayBool(condition);

			return result;
		}

		/// <summary>
		/// ��ָ֤�����ӽڵ�����֯��Ԫ�Ƿ����
		/// </summary>
		/// <param name="ou"></param>
		/// <param name="LDAPDomian"></param>
		/// <returns></returns>
		public  bool CheckUnit(string ou,string LDAPDomian)
		{

			string condition= "(&(objectClass=organizationalunit)(ou="+ou+"))";

			bool result=Iads.CommonWayBool(condition,LDAPDomian);

			return result;
		}

//		/// <summary>
//		/// ��֤�˺��Ƿ����
//		/// </summary>
//		/// <returns></returns>
//		public  bool CheckUser(string userName)
//		{
//			//deSearch.Filter="(&(&(objectCategory=person)(objectClass=user))(cn=*))";
//			string condition="(&(objectClass=user)(cn="+userName+"))";
//			bool result=Iads.CommonWayBool(condition);
//			return result;
//		}


		public  bool CheckUser(string userName)
		{
			string condition="(&(&(objectCategory=person)(objectClass=user))(cn="+userName+"))";			
			bool result=Iads.CommonWayBool(condition);
			return result;
		}




		/// <summary>
		/// ��֤�˺��Ƿ����
		/// </summary>
		/// <returns></returns>
		public  bool CheckUser(string userName,string father_OU)
		{
			AD_Common Iadc=new AD_Common();
			string LDAPDomain ="/"+father_OU.ToString()+ Iadc.GetLDAPDomain() ;
			string condition="(&(objectClass=user)(cn="+userName+"))";
			bool result=Iads.CommonWayBool(condition,LDAPDomain);
			return result;
		}
	}
}
