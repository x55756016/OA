/*活动目录操作组件 Beta 1.0 
 * 樊帆
 * 2005-4-27
 * */



using System;
using System.DirectoryServices;


namespace InterActiveDirectory
{
	/// <summary>
	/// AD_LoginCheck 的摘要说明。
	/// </summary>
	public class AD_LoginCheck
	{
		private  string ADPath=""; 
		private  string ADUser = "";
		private  string ADPassword = "";
		private  string ADServer="";

		private AD_Common Iadc;

		private AD_seacher Iads;

		private AD_Check Iadch;

		public AD_LoginCheck()
		{
			AD_Parameter adp=new AD_Parameter();

			ADPath=adp.ADPath; 
			ADUser = adp.ADUser;
			ADPassword = adp.ADPassword;
			ADServer=adp.ADServer;

			//
			// TODO: 在此处添加构造函数逻辑
			//
			Iadc=new AD_Common();
			Iads=new AD_seacher();
			Iadch=new AD_Check();
		}

		public AD_LoginCheck(string _ADPath,string _ADUser,string _ADPassword,string _ADServer)
		{
			ADPath=_ADPath;
			ADUser=_ADUser;
			ADPassword=_ADPassword;
			ADServer=_ADServer;
			//
			// TODO: 在此处添加构造函数逻辑
			//
			Iadc=new AD_Common( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iads=new AD_seacher( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iadch=new AD_Check( _ADPath, _ADUser, _ADPassword, _ADServer);
		}

		/// <summary>
		/// 验证用户登陆,检查账号\密码\账号状态
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <returns></returns>
		public  AD_Enum.LoginResult  Login(string UserName, string Password)
		{
			//如果是系统管理员或公司管理员，不验证活动目录
//			if(System.Web.HttpContext.Current.Session["LoginType"]!=null
//				&& (System.Web.HttpContext.Current.Session["LoginType"].ToString()=="1" ||
//				System.Web.HttpContext.Current.Session["LoginType"].ToString()=="2"))
//				return AD_Enum.LoginResult.LOGIN_OK;

			if(IsUserValid(UserName,Password))
			{
				DirectoryEntry de = Iads.GetUserEntry(UserName);
				if(de !=null)
				{
					int userAccountControl = Convert.ToInt32(de.Properties["userAccountControl"][0]);
					de.Close();
					if(!IsAccountActive(userAccountControl))
					{
						return AD_Enum.LoginResult.LOGIN_USER_ACCOUNT_INACTIVE;
					}
					else if(IsPWDExipred(userAccountControl))
					{
						return AD_Enum.LoginResult.LOGIN_USER_PWD_EXPIRED;
					}
					else
					{
						return AD_Enum.LoginResult.LOGIN_OK;
					}
				}
				else
				{
					return AD_Enum.LoginResult.LOGIN_USER_DOESNT_EXIST; 
				}
			}
			else
			{
				return AD_Enum.LoginResult.LOGIN_USER_DOESNT_EXIST; 
			}
		}

		public  AD_Enum.LoginResult  Login(string UserName)
		{
			//如果是系统管理员或公司管理员，不验证活动目录
//			if(System.Web.HttpContext.Current.Session["LoginType"]!=null
//				&& (System.Web.HttpContext.Current.Session["LoginType"].ToString()=="1" ||
//				System.Web.HttpContext.Current.Session["LoginType"].ToString()=="2"))
//				return AD_Enum.LoginResult.LOGIN_OK;


			DirectoryEntry de = Iads.GetUserEntry(UserName);
			if(de !=null)
			{
				int userAccountControl = Convert.ToInt32(de.Properties["userAccountControl"][0]);
				de.Close();
				if(!IsAccountActive(userAccountControl))
				{
					return AD_Enum.LoginResult.LOGIN_USER_ACCOUNT_INACTIVE;
				}
				else if(IsPWDExipred(userAccountControl))
				{
					return AD_Enum.LoginResult.LOGIN_USER_PWD_EXPIRED;
				}
			
				return AD_Enum.LoginResult.LOGIN_OK;
			
			}
			else
			{
				return AD_Enum.LoginResult.LOGIN_USER_DOESNT_EXIST; 
			}
		}

		public  DirectoryEntry UserExists(string UserName, string Password)
		{
			DirectoryEntry de = Iadc.GetDirectoryObject();
			DirectorySearcher deSearch = new DirectorySearcher();

			deSearch.SearchRoot =de;
			deSearch.Filter = "((objectClass=user)(cn=" + UserName + ")(userPassword=" + Password + "))";
			deSearch.SearchScope = SearchScope.Subtree;
			SearchResult results= deSearch.FindOne();

			de= new DirectoryEntry(results.Path,ADUser,ADPassword,AuthenticationTypes.Secure);
			return de;
		}

		/// <summary>
		/// 验证账号是否有效
		/// </summary>
		/// <param name="userAccountControl"></param>
		/// <returns></returns>
		public  bool IsAccountActive(int userAccountControl)
		{
			int userAccountControl_Disabled= Convert.ToInt32(AD_Enum.ADAccountOptions.UF_ACCOUNTDISABLE);
			
			int flagExists = userAccountControl & userAccountControl_Disabled;
			//bool userAccountControl & (int)ADS_USER_FLAG_ENUM.ADS_UF_DONT_EXPIRE_PASSWD) != 0
			//UserAccountControl & (int)ADS_USER_FLAG_ENUM.ADS_UF_DONT_EXPIRE_PASSWD) != 0;
			//userAccountControl & (int)0X10000!=0
			//int i=userAccountControl & (int)0X10000;

			if(flagExists >0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// 帐号密码是否过期
		/// </summary>
		/// <param name="userAccountControl"></param>
		/// <returns></returns>
		public  bool IsPWDExipred(int userAccountControl)
		{
			int i=userAccountControl & (int)0X10000;
			if(i==0)
				return true;
			else
				return false;		
		}

		/// <summary>
		/// 验证用户名和密码是否有效
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		/// <returns></returns>
		public  bool IsUserValid (string UserName, string Password)
		{
			try
			{
				DirectoryEntry deUser = GetUser(UserName,Password);
				deUser.Close();
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// 获取账号对象,并验证密码
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public  DirectoryEntry GetUser(string UserName, string Password)
		{
			SearchResult results=null;
			DirectoryEntry de = Iadc.GetDirectoryObject(UserName,Password);
			DirectorySearcher deSearch = new DirectorySearcher();

			//筛选
			deSearch.SearchRoot =de;
			deSearch.Filter = "(&(objectClass=user)(cn=" + UserName + "))";
			deSearch.SearchScope = SearchScope.Subtree;
			try
			{
				results= deSearch.FindOne();
			}
			catch{}
			finally{}


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
		
	}
}
