/*�Ŀ¼������� Beta 1.0 
 * ����
 * 2005-4-27
 * */

using System;
using System.Data;
using System.DirectoryServices;

namespace InterActiveDirectory
{
	/// <summary>
	/// AD_seacher ��ժҪ˵����
	/// </summary>
	public class AD_seacher
	{

		private  string ADPath=""; 
		private  string ADUser = "";
		private  string ADPassword = "";
		private  string ADServer="";

		private AD_Common Iadc;

		public AD_seacher()
		{
			AD_Parameter adp=new AD_Parameter();

			ADPath=adp.ADPath; 
			ADUser = adp.ADUser;
			ADPassword = adp.ADPassword;
			ADServer=adp.ADServer;

			Iadc=new AD_Common();
		}


		public AD_seacher(string _ADPath,string _ADUser,string _ADPassword,string _ADServer)
		{

			ADPath=_ADPath;
			ADUser=_ADUser;
			ADPassword=_ADPassword;
			ADServer=_ADServer;

			Iadc=new AD_Common( _ADPath, _ADUser, _ADPassword, _ADServer);
		}


		/// <summary>
		/// ������,����path
		/// </summary>
		/// <returns></returns>
		public  string GetGroup(string cn)
		{
			string condition="(&(objectClass=group)(cn="+cn+"))";
			SearchResult results= CommonWay(condition);
			return results.Path;
		}

		/// <summary>
		/// ������,����DirectoryEntry
		/// </summary>
		/// <returns></returns>
		public  DirectoryEntry GetGroupEntry(string cn)
		{
			string condition= "(&(objectClass=group)(cn="+cn+"))";
			DirectoryEntry oDE=CommonWayEntry(condition);
			return oDE;
		}

		/// <summary>
		/// ������֯��Ԫ,����path
		/// </summary>
		/// <returns></returns>
		public  string GetUnit(string ou)
		{
			string condition="(&(objectClass=organizationalunit)(ou="+ou+"))";
			SearchResult results= CommonWay(condition);
            
			return results.Path;
		}

		/// <summary>
		/// ����ָ��·������֯��Ԫ,����DirectoryEntry
		/// </summary>
		/// <param name="ou">��֯��Ԫ����</param>
		/// <param name="LDAPDomian">����Ԫ·��</param>
		/// <returns></returns>
		public  DirectoryEntry GetUnitEntry(string ou,string LDAPDomian)
		{
			string condition=  "(&(objectClass=organizationalunit)(ou="+ou+"))";

			DirectoryEntry oDE=CommonWayEntry(condition,LDAPDomian);

			return oDE;
		}

		/// <summary>
		/// �����û�,����path
		/// </summary>
		/// <returns></returns>
		public  string GetUser(string cn)
		{
			string strReturn="";
			//(&(&(objectCategory=person)(objectClass=user))(cn=*))
			//string condition= "(&(objectClass=user)(cn="+cn+"))";
			string condition="(&(&(objectCategory=person)(objectClass=user))(cn="+cn+"))";
			string results= CommonWay_Col(condition);//CommonWay(condition);
//			CommonWay_Col(condition);
//			strReturn=results.Path;
			return results;
		}

		/// <summary>
		/// �����û�,����DirectoryEntry
		/// </summary>
		/// <returns></returns>
		public  DirectoryEntry GetUserEntry(string cn)
		{
			string condition= "(&(objectClass=user)(cn="+cn+"))";
			DirectoryEntry oDE=CommonWayEntry_Col(condition);
			return oDE;
		}

		public  DataTable GetPeroritys(string cn)
		{
			DataTable dt=new DataTable();
			dt.Columns.Add(new DataColumn("name",typeof(string)));
			dt.Columns.Add(new DataColumn("value",typeof(string)));
			DataRow dr;
			DirectoryEntry oDE=GetGroupEntry(cn);
			for(int i=0;i<oDE.Properties.Count;i++)
			{
				dr=dt.NewRow();
				dr["name"]=oDE.Properties.PropertyNames.GetEnumerator().Current.ToString();
				dr["value"]=oDE.Properties.Values.GetEnumerator().Current.ToString();
				dt.Rows.Add(dr);
			}
			return dt;
		}


		public  SearchResult CommonWay(string condition)
		{
			string LDAPDomain ="/" + Iadc.GetLDAPDomain() ;
			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot =de;
			deSearch.Filter = condition;
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResult results= deSearch.FindOne();
			return results;
		}

		public  string CommonWay_Col(string condition)
		{
			 
			string LDAPDomain ="/" + Iadc.GetLDAPDomain() ;
			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot =de;
			deSearch.Filter = condition;
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResultCollection results= deSearch.FindAll();
			string [] res;
			string ret="";
			bool isCom=true;
			for (int i=0;i<results.Count;i++)
			{
				res=results[i].Path.Split(',');
				isCom=true;
				for(int j=0;j<res.Length;j++)
				{
					if(res[j]=="CN=Computers")
					{
                        isCom=false;
					}

				}
				if(isCom)
				{
					ret=results[i].Path;
				}
				
				
			}
			return ret;
		}

		/// <summary>
		/// ���������ͬ�Ľڵ������һ������
		/// </summary>
		/// <param name="condition"></param>
		/// <returns></returns>
		public  DirectoryEntry CommonWayEntry_Col(string condition)
		{
			string LDAPDomain ="/" + Iadc.GetLDAPDomain() ;
			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot =de;
			deSearch.Filter = condition;
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResultCollection results= deSearch.FindAll();
			int i=0;
			string [] res;
			string ret="";
			bool isCom=true;
			for (i=0;i<results.Count;i++)
			{
				res=results[i].Path.Split(',');
				isCom=true;
				for(int j=0;j<res.Length;j++)
				{
					if(res[j]=="CN=Computers")
					{
						isCom=false;
					}

				}
				if(isCom)
				{
					break;
				}
				
				
			}
			
			if(results !=null)
			{
				try
				{
					de= new DirectoryEntry(results[i].Path,ADUser,ADPassword,AuthenticationTypes.Secure);
				}
				catch{}
				return de;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// ���������ͬ�Ľڵ������һ������
		/// </summary>
		/// <param name="condition"></param>
		/// <returns></returns>
		public  DirectoryEntry CommonWayEntry(string condition)
		{
			string LDAPDomain ="/" + Iadc.GetLDAPDomain() ;
			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot =de;
			deSearch.Filter = condition;
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResult results= deSearch.FindOne();
			
			if(results !=null)
			{
				de= new DirectoryEntry(results.Path,ADUser,ADPassword,AuthenticationTypes.Secure);
				return de;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// ���������ͬ�Ľڵ������һ������
		/// </summary>
		/// <param name="condition"></param>
		/// <returns></returns>
		public  DirectoryEntry CommonWayEntry_ALL(string condition)
		{
			string LDAPDomain ="/" + Iadc.GetLDAPDomain() ;
			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot =de;
			deSearch.Filter = condition;
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResultCollection results= deSearch.FindAll();
			int i=0;
			string [] res;
			string ret="";
			bool isCom=true;
			for (i=0;i<results.Count;i++)
			{
				res=results[i].Path.Split(',');
				isCom=true;
				for(int j=0;j<res.Length;j++)
				{
					if(res[j]=="CN=User")
					{
						isCom=false;
					}

				}
				if(isCom)
				{
					break;
				}
				
				
			}
			
			if(results !=null)
			{
				try
				{
					de= new DirectoryEntry(results[i].Path,ADUser,ADPassword,AuthenticationTypes.Secure);
				}
				catch{}
				return de;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// ���ָ��·���Ľڵ�Ķ���
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="LDAPDomain">�ƶ�·��</param>
		/// <returns>DirectoryEntry</returns>
		public  DirectoryEntry CommonWayEntry(string condition,string LDAPDomain)
		{
			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);

			//���ָ��·���µĽڵ��·��
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot =de;
			deSearch.Filter = condition;
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResult results= deSearch.FindOne();
			
			if(results !=null)
			{
				de= new DirectoryEntry(results.Path,ADUser,ADPassword,AuthenticationTypes.Secure);
				return de;
			}
			else
			{
				return null;
			}
		}

		public  bool CommonWayBool(string condition)
		{
			string LDAPDomain ="/" + Iadc.GetLDAPDomain() ;
			bool result=false;
			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);
			DirectorySearcher deSearch = new DirectorySearcher();
			deSearch.SearchRoot =de;
			deSearch.Filter = condition;
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResult results= deSearch.FindOne();
			if(results!=null)
			{
				result = true;
			}
			return result;
		}

		/// <summary>
		/// ����ָ���Ľڵ����������������ͬ�Ķ���
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="LDAPDomain">���LDAP·��</param>
		/// <returns></returns>
		public  bool CommonWayBool(string condition,string LDAPDomain)
		{			
			bool result=false;

			DirectoryEntry de = Iadc.GetDirectoryObject(LDAPDomain);
			SearchResult results=null;

			///���ҽڵ��¼�
			DirectorySearcher deSearch = new DirectorySearcher();

			try
			{
				deSearch.SearchRoot =de;
				deSearch.Filter = condition;
				deSearch.SearchScope = SearchScope.OneLevel;

				results= deSearch.FindOne();
			}
			catch{}
			finally{}
			if(results!=null)
			{
				//�ڵ��´��ڸ�ͬ���ӽڵ�
				result = true;
			}

			return result;
		}
	}
}
