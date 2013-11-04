using System;
using System.Security.Principal;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Configuration;

 

namespace Helper

{

	/// 

	/// �Ŀ¼�����ࡣ��װһϵ�лĿ¼������صķ�����

	/// 

	public sealed class ADHelper

	{

		/// 

		/// ����

		/// 

		private static string DomainName = "test";

		/// 

		/// LDAP ��ַ

		/// 

		private static string LDAPDomain = "DC=test,DC=com";

		/// 

		/// LDAP��·��

		/// 

		private static string ADPath = System.Configuration.ConfigurationSettings.AppSettings["ADPath"];

		/// 

		/// ��¼�ʺ�

		/// 

		private static string ADUser = System.Configuration.ConfigurationSettings.AppSettings["ADUser"];

		/// 

		/// ��¼����

		/// 

		private static string ADPassword = System.Configuration.ConfigurationSettings.AppSettings["ADPassword"];

		/// 

		/// ������ʵ��

		/// 

		private static IdentityImpersonation impersonate = new IdentityImpersonation(ADUser, ADPassword, DomainName);

 

		/// 

		/// �û���¼��֤���

		/// 

		public enum LoginResult

		{

			/// 

			/// ������¼

			/// 

			LOGIN_USER_OK = 0,

			/// 

			/// �û�������

			/// 

			LOGIN_USER_DOESNT_EXIST,

			/// 

			/// �û��ʺű�����

			/// 

			LOGIN_USER_ACCOUNT_INACTIVE,

			/// 

			/// �û����벻��ȷ

			/// 

			LOGIN_USER_PASSWORD_INCORRECT

		}

 

		/// 

		/// �û����Զ����־

		/// 

		public enum ADS_USER_FLAG_ENUM

		{

			/// 

			/// ��¼�ű���־�����ͨ�� ADSI LDAP ���ж���д����ʱ���ñ�־ʧЧ�����ͨ�� ADSI WINNT���ñ�־Ϊֻ����

			/// 

			ADS_UF_SCRIPT = 0X0001,

			/// 

			/// �û��ʺŽ��ñ�־

			/// 

			ADS_UF_ACCOUNTDISABLE = 0X0002,

			/// 

			/// ���ļ��б�־

			/// 

			ADS_UF_HOMEDIR_REQUIRED = 0X0008,

			/// 

			/// ���ڱ�־

			/// 

			ADS_UF_LOCKOUT = 0X0010,

			/// 

			/// �û����벻�Ǳ����

			/// 

			ADS_UF_PASSWD_NOTREQD = 0X0020,

			/// 

			/// ���벻�ܸ��ı�־

			/// 

			ADS_UF_PASSWD_CANT_CHANGE = 0X0040,

			/// 

			/// ʹ�ÿ���ļ��ܱ�������

			/// 

			ADS_UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0X0080,

			/// 

			/// �����ʺű�־

			/// 

			ADS_UF_TEMP_DUPLICATE_ACCOUNT = 0X0100,

			/// 

			/// ��ͨ�û���Ĭ���ʺ�����

			/// 

			ADS_UF_NORMAL_ACCOUNT = 0X0200,

			/// 

			/// ����������ʺű�־

			/// 

			ADS_UF_INTERDOMAIN_TRUST_ACCOUNT = 0X0800,

			/// 

			/// ����վ�����ʺű�־

			/// 

			ADS_UF_WORKSTATION_TRUST_ACCOUNT = 0x1000,

			/// 

			/// �����������ʺű�־

			/// 

			ADS_UF_SERVER_TRUST_ACCOUNT = 0X2000,

			/// 

			/// �����������ڱ�־

			/// 

			ADS_UF_DONT_EXPIRE_PASSWD = 0X10000,

			/// 

			/// MNS �ʺű�־

			/// 

			ADS_UF_MNS_LOGON_ACCOUNT = 0X20000,

			/// 

			/// ����ʽ��¼����ʹ�����ܿ�

			/// 

			ADS_UF_SMARTCARD_REQUIRED = 0X40000,

			/// 

			/// �����øñ�־ʱ�������ʺţ��û��������ʺţ���ͨ�� Kerberos ί������

			/// 

			ADS_UF_TRUSTED_FOR_DELEGATION = 0X80000,

			/// 

			/// �����øñ�־ʱ����ʹ�����ʺ���ͨ�� Kerberos ί�����εģ������ʺŲ��ܱ�ί��

			/// 

			ADS_UF_NOT_DELEGATED = 0X100000,

			/// 

			/// ���ʺ���Ҫ DES ��������

			/// 

			ADS_UF_USE_DES_KEY_ONLY = 0X200000,

			/// 

			/// ��Ҫ���� Kerberos Ԥ�����֤

			/// 

			ADS_UF_DONT_REQUIRE_PREAUTH = 0X4000000,

			/// 

			/// �û�������ڱ�־

			/// 

			ADS_UF_PASSWORD_EXPIRED = 0X800000,

			/// 

			/// �û��ʺſ�ί�б�־

			/// 

			ADS_UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0X1000000

		}

 

		public ADHelper()

		{

			//

		}

 

		#region GetDirectoryObject

 

		/// 

		/// ���DirectoryEntry����ʵ��,�Թ���Ա��½AD

		/// 

		/// 

		private static DirectoryEntry GetDirectoryObject()

		{

			DirectoryEntry entry = new DirectoryEntry(ADPath, ADUser, ADPassword, AuthenticationTypes.Secure);

			return entry;

		}

 

		/// 

		/// ����ָ���û�������������ӦDirectoryEntryʵ��

		/// 

		/// 

		/// 

		/// 

		private static DirectoryEntry GetDirectoryObject(string userName, string password)

		{

			DirectoryEntry entry = new DirectoryEntry(ADPath, userName, password, AuthenticationTypes.Secure);

			return entry;

		}

 

		/// 

		/// i.e. /CN=Users,DC=creditsights, DC=cyberelves, DC=Com

		/// 

		/// 

		/// 

		private static DirectoryEntry GetDirectoryObject(string domainReference)

		{

			DirectoryEntry entry = new DirectoryEntry(ADPath + domainReference, ADUser, ADPassword, AuthenticationTypes.Secure);

			return entry;

		}

 

		/// 

		/// �����UserName,Password������DirectoryEntry

		/// 

		/// 

		/// 

		/// 

		/// 

		private static DirectoryEntry GetDirectoryObject(string domainReference, string userName, string password)

		{

			DirectoryEntry entry = new DirectoryEntry(ADPath + domainReference, userName, password, AuthenticationTypes.Secure);

			return entry;

		}

 

		#endregion

 

		#region GetDirectoryEntry

 

		/// 

		/// �����û���������ȡ���û��� ����

		/// 

		/// �û���������

		/// ����ҵ����û����򷵻��û��� ���󣻷��򷵻� null

		public static DirectoryEntry GetDirectoryEntry(string commonName)

		{

			DirectoryEntry de = GetDirectoryObject();

			DirectorySearcher deSearch = new DirectorySearcher(de);

			deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(cn=" + commonName + "))";

			deSearch.SearchScope = SearchScope.Subtree;

 

			try

			{

				SearchResult result = deSearch.FindOne();

				de = new DirectoryEntry(result.Path);

				return de;

			}

			catch

			{

				return null;

			}

		}

 

		/// 

		/// �����û��������ƺ�����ȡ���û��� ����

		/// 

		/// �û���������

		/// �û�����

		/// ����ҵ����û����򷵻��û��� ���󣻷��򷵻� null

		public static DirectoryEntry GetDirectoryEntry(string commonName, string password)

		{

			DirectoryEntry de = GetDirectoryObject(commonName, password);

			DirectorySearcher deSearch = new DirectorySearcher(de);

			deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(cn=" + commonName + "))";

			deSearch.SearchScope = SearchScope.Subtree;

 

			try

			{

				SearchResult result = deSearch.FindOne();

				de = new DirectoryEntry(result.Path);

				return de;

			}

			catch

			{

				return null;

			}

		}

 

		/// 

		/// �����û��ʺų�ȡ���û��� ����

		/// 

		/// �û��ʺ���

		/// ����ҵ����û����򷵻��û��� ���󣻷��򷵻� null

		public static DirectoryEntry GetDirectoryEntryByAccount(string sAMAccountName)

		{

			DirectoryEntry de = GetDirectoryObject();

			DirectorySearcher deSearch = new DirectorySearcher(de);

			deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(sAMAccountName=" + sAMAccountName + "))";

			deSearch.SearchScope = SearchScope.Subtree;

 

			try

			{

				SearchResult result = deSearch.FindOne();

				de = new DirectoryEntry(result.Path);

				return de;

			}

			catch

			{

				return null;

			}

		}

 

		/// 

		/// �����û��ʺź�����ȡ���û��� ����

		/// 

		/// �û��ʺ���

		/// �û�����

		/// ����ҵ����û����򷵻��û��� ���󣻷��򷵻� null

		public static DirectoryEntry GetDirectoryEntryByAccount(string sAMAccountName, string password)

		{

			DirectoryEntry de = GetDirectoryEntryByAccount(sAMAccountName);

			if (de != null)

			{

				string commonName = de.Properties["cn"][0].ToString();

 

				if (GetDirectoryEntry(commonName, password) != null)

					return GetDirectoryEntry(commonName, password);

				else

					return null;

			}

			else

			{

				return null;

			}

		}

 

		/// 

		/// ��������ȡ���û���� ����

		/// 

		/// ����

		/// 

		public static DirectoryEntry GetDirectoryEntryOfGroup(string groupName)

		{

			DirectoryEntry de = GetDirectoryObject();

			DirectorySearcher deSearch = new DirectorySearcher(de);

			deSearch.Filter = "(&(objectClass=group)(cn=" + groupName + "))";

			deSearch.SearchScope = SearchScope.Subtree;

 

			try

			{

				SearchResult result = deSearch.FindOne();

				de = new DirectoryEntry(result.Path);

				return de;

			}

			catch

			{

				return null;

			}

		}

 

		#endregion

 

		#region GetProperty

 

		/// 

		/// ���ָ�� ָ����������Ӧ��ֵ

		/// 

		/// 

		/// ��������

		/// ����ֵ

		public static string GetProperty(DirectoryEntry de, string propertyName)

		{

			if(de.Properties.Contains(propertyName))

			{

				return de.Properties[propertyName][0].ToString() ;

			}

			else

			{

				return string.Empty;

			}

		}

 

		/// 

		/// ���ָ��������� ��ָ����������Ӧ��ֵ

		/// 

		/// 

		/// ��������

		/// ����ֵ

		public static string GetProperty(SearchResult searchResult, string propertyName)

		{

			if(searchResult.Properties.Contains(propertyName))

			{

				return searchResult.Properties[propertyName][0].ToString() ;

			}

			else

			{

				return string.Empty;

			}

		}

 

		#endregion

 

		/// 

		/// ����ָ�� ������ֵ

		/// 

		/// 

		/// ��������

		/// ����ֵ

		public static void SetProperty(DirectoryEntry de, string propertyName, string propertyValue)

		{

			if(propertyValue != string.Empty || propertyValue != "" || propertyValue != null)

			{

				if(de.Properties.Contains(propertyName))

				{

					de.Properties[propertyName][0] = propertyValue; 

				}

				else

				{

					de.Properties[propertyName].Add(propertyValue);

				}

			}

		}

 

		/// 

		/// �����µ��û�

		/// 

		/// DN λ�á����磺OU=����ƽ̨ �� CN=Users

		/// ��������

		/// �ʺ�

		/// ����

		/// 

		public static DirectoryEntry CreateNewUser(string ldapDN, string commonName, string sAMAccountName, string password)

		{

			DirectoryEntry entry = GetDirectoryObject();

			DirectoryEntry subEntry = entry.Children.Find(ldapDN);

			DirectoryEntry deUser = subEntry.Children.Add("CN=" + commonName, "user");

			deUser.Properties["sAMAccountName"].Value = sAMAccountName;

			deUser.CommitChanges();

			ADHelper.EnableUser(commonName);

			ADHelper.SetPassword(commonName, password);

			deUser.Close();

			return deUser;

		}

 

		/// 

		/// �����µ��û���Ĭ�ϴ����� Users ��Ԫ�¡�

		/// 

		/// ��������

		/// �ʺ�

		/// ����

		/// 

		public static DirectoryEntry CreateNewUser(string commonName, string sAMAccountName, string password)

		{

			return CreateNewUser("CN=Users", commonName, sAMAccountName, password);

		}

 

		/// 

		/// �ж�ָ���������Ƶ��û��Ƿ����

		/// 

		/// �û���������

		/// ������ڣ����� true�����򷵻� false

		public static bool IsUserExists(string commonName)

		{

			DirectoryEntry de = GetDirectoryObject();

			DirectorySearcher deSearch = new DirectorySearcher(de);

			deSearch.Filter = "(&(&(objectCategory=person)(objectClass=user))(cn=" + commonName + "))";       // LDAP ��ѯ��

			SearchResultCollection results = deSearch.FindAll();

 

			if (results.Count == 0)

				return false;

			else

				return true;

		}

 

		/// 

		/// �ж��û��ʺ��Ƿ񼤻�

		/// 

		/// �û��ʺ����Կ�����

		/// ����û��ʺ��Ѿ�������� true�����򷵻� false

		public static bool IsAccountActive(int userAccountControl)

		{

			int userAccountControl_Disabled = Convert.ToInt32(ADS_USER_FLAG_ENUM.ADS_UF_ACCOUNTDISABLE);

			int flagExists = userAccountControl & userAccountControl_Disabled;

 

			if (flagExists > 0)

				return false;

			else

				return true;

		}

 

		/// 

		/// �ж��û��������Ƿ��㹻�����������֤������¼

		/// 

		/// �û���������

		/// ����

		/// ���ܿ�������¼���򷵻� true�����򷵻� false

		public static LoginResult Login(string commonName, string password)

		{

			DirectoryEntry de = GetDirectoryEntry(commonName);

 

			if (de != null)

			{

				// �������ж��û�������ȷǰ�����ʺż������Խ����жϣ����򽫳����쳣��

				int userAccountControl = Convert.ToInt32(de.Properties["userAccountControl"][0]);

				de.Close();

 

				if (!IsAccountActive(userAccountControl))

					return LoginResult.LOGIN_USER_ACCOUNT_INACTIVE;

 

				if (GetDirectoryEntry(commonName, password) != null)

					return LoginResult.LOGIN_USER_OK;

				else

					return LoginResult.LOGIN_USER_PASSWORD_INCORRECT;

			}

			else

			{

				return LoginResult.LOGIN_USER_DOESNT_EXIST; 

			}

		}

 

		/// 

		/// �ж��û��ʺ��������Ƿ��㹻�����������֤������¼

		/// 

		/// �û��ʺ�

		/// ����

		/// ���ܿ�������¼���򷵻� true�����򷵻� false

		public static LoginResult LoginByAccount(string sAMAccountName, string password)

		{

			DirectoryEntry de = GetDirectoryEntryByAccount(sAMAccountName);

                   

			if (de != null)

			{

				// �������ж��û�������ȷǰ�����ʺż������Խ����жϣ����򽫳����쳣��

				int userAccountControl = Convert.ToInt32(de.Properties["userAccountControl"][0]);

				de.Close();

 

				if (!IsAccountActive(userAccountControl))

					return LoginResult.LOGIN_USER_ACCOUNT_INACTIVE;

 

				if (GetDirectoryEntryByAccount(sAMAccountName, password) != null)

					return LoginResult.LOGIN_USER_OK;

				else

					return LoginResult.LOGIN_USER_PASSWORD_INCORRECT;

			}

			else

			{

				return LoginResult.LOGIN_USER_DOESNT_EXIST; 

			}

		}

 

		/// 

		/// �����û����룬����Ա����ͨ�������޸�ָ���û������롣

		/// 

		/// �û���������

		/// �û�������

		public static void SetPassword(string commonName, string newPassword)

		{

			DirectoryEntry de = GetDirectoryEntry(commonName);

              

			// ģ�ⳬ������Ա���Դﵽ��Ȩ���޸��û�����

			impersonate.BeginImpersonate();

			de.Invoke("SetPassword", new object[]{newPassword});

			impersonate.StopImpersonate();

 

			de.Close();

		}

 

		/// 

		/// �����ʺ����룬����Ա����ͨ�������޸�ָ���ʺŵ����롣

		/// 

		/// �û��ʺ�

		/// �û�������

		public static void SetPasswordByAccount(string sAMAccountName, string newPassword)

		{

			DirectoryEntry de = GetDirectoryEntryByAccount(sAMAccountName);

 

			// ģ�ⳬ������Ա���Դﵽ��Ȩ���޸��û�����

			IdentityImpersonation impersonate = new IdentityImpersonation(ADUser, ADPassword, DomainName);

			impersonate.BeginImpersonate();

			de.Invoke("SetPassword", new object[]{newPassword});

			impersonate.StopImpersonate();

 

			de.Close();

		}

 

		/// 

		/// �޸��û�����

		/// 

		/// �û���������

		/// ������

		/// ������

		public static void ChangeUserPassword (string commonName, string oldPassword, string newPassword)

		{

			// to-do: ��Ҫ��������������

			DirectoryEntry oUser = GetDirectoryEntry(commonName);

			oUser.Invoke("ChangePassword", new Object[]{oldPassword, newPassword});

			oUser.Close();

		}

 

		/// 

		/// ����ָ���������Ƶ��û�

		/// 

		/// �û���������

		public static void EnableUser(string commonName)

		{

			EnableUser(GetDirectoryEntry(commonName));

		}

 

		/// 

		/// ����ָ�� ���û�

		/// 

		/// 

		public static void EnableUser(DirectoryEntry de)

		{

			impersonate.BeginImpersonate();

			de.Properties["userAccountControl"][0] = ADHelper.ADS_USER_FLAG_ENUM.ADS_UF_NORMAL_ACCOUNT | ADHelper.ADS_USER_FLAG_ENUM.ADS_UF_DONT_EXPIRE_PASSWD;

			de.CommitChanges();

			impersonate.StopImpersonate();

			de.Close();

		}

 

		/// 

		/// ����ָ���������Ƶ��û�

		/// 

		/// �û���������

		public static void DisableUser(string commonName)

		{

			DisableUser(GetDirectoryEntry(commonName));

		}

 

		/// 

		/// ����ָ�� ���û�

		/// 

		/// 

		public static void DisableUser(DirectoryEntry de)

		{

			impersonate.BeginImpersonate();

			de.Properties["userAccountControl"][0]=ADHelper.ADS_USER_FLAG_ENUM.ADS_UF_NORMAL_ACCOUNT | ADHelper.ADS_USER_FLAG_ENUM.ADS_UF_DONT_EXPIRE_PASSWD | ADHelper.ADS_USER_FLAG_ENUM.ADS_UF_ACCOUNTDISABLE;

			de.CommitChanges();

			impersonate.StopImpersonate();

			de.Close();

		}

 

		/// 

		/// ��ָ�����û���ӵ�ָ�������С�Ĭ��Ϊ Users �µ�����û���

		/// 

		/// �û���������

		/// ����

		public static void AddUserToGroup(string userCommonName, string groupName)

		{

			DirectoryEntry oGroup = GetDirectoryEntryOfGroup(groupName);

			DirectoryEntry oUser = GetDirectoryEntry(userCommonName);

              

			impersonate.BeginImpersonate();

			oGroup.Properties["member"].Add(oUser.Properties["distinguishedName"].Value);

			oGroup.CommitChanges();

			impersonate.StopImpersonate();

 

			oGroup.Close();

			oUser.Close();

		}

 

		/// 

		/// ���û���ָ�������Ƴ���Ĭ��Ϊ Users �µ�����û���

		/// 

		/// �û���������

		/// ����

		public static void RemoveUserFromGroup(string userCommonName, string groupName)

		{

			DirectoryEntry oGroup = GetDirectoryEntryOfGroup(groupName);

			DirectoryEntry oUser = GetDirectoryEntry(userCommonName);

              

			impersonate.BeginImpersonate();

			oGroup.Properties["member"].Remove(oUser.Properties["distinguishedName"].Value);

			oGroup.CommitChanges();

			impersonate.StopImpersonate();

 

			oGroup.Close();

			oUser.Close();

		}

 

	}

 

	/// 

	/// �û�ģ���ɫ�ࡣʵ���ڳ�����ڽ����û���ɫģ�⡣

	/// 

	public class IdentityImpersonation

	{

		[DllImport("advapi32.dll", SetLastError=true)]

		public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

 

		[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]

		public extern static bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

 

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]

		public extern static bool CloseHandle(IntPtr handle);

 

		// Ҫģ����û����û��������롢��(������)

		private String _sImperUsername;

		private String _sImperPassword;

		private String _sImperDomain;

		// ��¼ģ��������

		private WindowsImpersonationContext _imperContext;

		private IntPtr _adminToken;

		private IntPtr _dupeToken;

		// �Ƿ���ֹͣģ��

		private Boolean _bClosed;

 

		/// 

		/// ���캯��

		/// 

		/// ��Ҫģ����û����û���

		/// ��Ҫģ����û�������

		/// ��Ҫģ����û����ڵ���

		public IdentityImpersonation(String impersonationUsername, String impersonationPassword, String impersonationDomain) 

		{

			_sImperUsername = impersonationUsername;

			_sImperPassword = impersonationPassword;

			_sImperDomain = impersonationDomain;

 

			_adminToken = IntPtr.Zero;

			_dupeToken = IntPtr.Zero;

			_bClosed = true;

		}

 

		/// 

		/// ��������

		/// 

		~IdentityImpersonation() 

		{

			if(!_bClosed) 

			{

				StopImpersonate();

			}

		}

 

		/// 

		/// ��ʼ��ݽ�ɫģ�⡣

		/// 

		/// 

		public Boolean BeginImpersonate() 

		{

			Boolean bLogined = LogonUser(_sImperUsername, _sImperDomain, _sImperPassword, 2, 0, ref _adminToken);

                        

			if(!bLogined) 

			{

				return false;

			}

 

			Boolean bDuped = DuplicateToken(_adminToken, 2, ref _dupeToken);

 

			if(!bDuped) 

			{

				return false;

			}

 

			WindowsIdentity fakeId = new WindowsIdentity(_dupeToken);

			_imperContext = fakeId.Impersonate();

 

			_bClosed = false;

 

			return true;

		}

 

		/// 

		/// ֹͣ��ֽ�ɫģ�⡣

		/// 

		public void StopImpersonate() 

		{

			_imperContext.Undo();

			CloseHandle(_dupeToken);

			CloseHandle(_adminToken);

			_bClosed = true;

		}

	}

 

}


