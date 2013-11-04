using System;

namespace InterActiveDirectory
{
	public class AD_Parameter
	{
		private  string aDPath=""; 
		private  string aDUser = "";
		private  string aDPassword = "";
		private  string aDServer="";

		public AD_Parameter()
		{
//			aDPath="LDAP://172.18.5.5";
//		aDUser="administrator";
//		aDPassword="123";
//			aDServer="test.com";

			aDPath="LDAP://172.16.1.16";
		 	aDUser="ctuser";
            aDPassword = "chinact789";
			 aDServer="sinomaster.com";
            
		}

		public  string ADPath
		{
			set{ADPath=aDPath;}
			get{return aDPath;}
		}
		public  string ADUser
		{
			set{ADUser=aDUser;}
			get{return aDUser;}
		}
		public  string ADPassword
		{
			set{ADPassword=aDPassword;}
			get{return aDPassword;}
		}
		public  string ADServer
		{
			set{ADServer=aDServer;}
			get{return aDServer;}
		}
		
	}





	/// <summary>
	/// ϵͳʹ�õ�ö������ ��ժҪ˵����
	/// </summary>
	public class AD_Enum
	{
		public AD_Enum()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}

		public enum ErrorString
		{
//			Err0001="δָ���Ĵ���",
//			Err0002="��ָ���Ľڵ��·����������ڵ�ͬ���Ķ���",
//			Err0003="δ�ҵ�ָ���Ķ���",
		}

		public enum LoginResult
		{
			LOGIN_OK=0,
			LOGIN_USER_DOESNT_EXIST,
			LOGIN_USER_ACCOUNT_INACTIVE,
			LOGIN_USER_PWD_EXPIRED
		}

		public enum ADAccountOptions
		{
			UF_TEMP_DUPLICATE_ACCOUNT = 0x0100,
			UF_NORMAL_ACCOUNT =0X0200,
			UF_INTERDOMAIN_TRUST_ACCOUNT =0x0800,
			UF_WORKSTATION_TRUST_ACCOUNT = 0x1000,
			UF_SERVER_TRUST_ACCOUNT =0x2000,
			UF_DONT_EXPIRE_PASSWD=0x10000,
			UF_SCRIPT =0x0001,
			UF_ACCOUNTDISABLE=0x0002,
			UF_HOMEDIR_REQUIRED =0x0008,
			UF_LOCKOUT=0x0010,
			UF_PASSWD_NOTREQD=0x0020,
			UF_PASSWD_CANT_CHANGE=0x0040,
			UF_ACCOUNT_LOCKOUT=0X0010,
			UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED=0X0080,
		}	
	}
}
