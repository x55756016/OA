/*活动目录操作组件 Beta 1.0 
 * 
 * 操作用户组（目录不允许出现名字相同的组）
 * 
 * 樊帆
 * 2005-4-27
 * 2005-5-27日更改
 * */


using System;
using System.DirectoryServices;

namespace InterActiveDirectory
{
	/// <summary>
	/// 组操作类的说明。
	/// </summary>
	public class AD_Group
	{
		#region 属性
		private  string ADPath=""; 
		private  string ADUser = "";
		private  string ADPassword = "";
		private  string ADServer="";

		private AD_Common Iadc;

		private AD_seacher Iads;

		private AD_Check Iadch;

		#endregion

		/// <summary>
		/// 构造函数
		/// </summary>
		public AD_Group()
		{
			AD_Parameter adp=new AD_Parameter();
			ADPath=adp.ADPath; 
			ADUser = adp.ADPassword;
			ADPassword = adp.ADPassword;
			ADServer=adp.ADServer;

			//创建基本对象
			Iadc=new AD_Common();
			Iads=new AD_seacher();
			Iadch=new AD_Check();
		}


		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="_ADPath"></param>
		/// <param name="_ADUser"></param>
		/// <param name="_ADPassword"></param>
		/// <param name="_ADServer"></param>
		public AD_Group(string _ADPath,string _ADUser,string _ADPassword,string _ADServer)
		{
			ADPath=_ADPath;
			ADUser=_ADUser;
			ADPassword=_ADPassword;
			ADServer=_ADServer;
			
			//创建基本对象
			Iadc=new AD_Common( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iads=new AD_seacher( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iadch=new AD_Check( _ADPath, _ADUser, _ADPassword, _ADServer);
		}


		/// <summary>
		/// 添加新组到指定的组织单元
		/// </summary>
		/// <param name="cn">用户组</param>
		/// <param name="ouPath">组织单元路径(格式:OU=sddsd,OU=sdsdsd,顺序，子倒父)</param>
		/// <param name="description">描述</param>
		/// <returns>bool</returns>
		public  int CreateGroupToUnit(string cn,string description,string path,out string errStr)
		{
			int result=0;
			errStr="";

			//创建指定路径的组织单元对象
			int i=0;int j=0;
			//string LDAPDomain ="/"+ouPath.ToString()+Iadc.GetLDAPDomain() ;

			//string LDAPDomain ="/"+ouPath.ToString()+ Iadc.GetLDAPDomain() ;

			//DirectoryEntry oDE= Iadc.GetDirectoryObject(LDAPDomain);
			
			DirectoryEntry oDE= Iadc.GetDirectoryObject(Iads.GetUnit(cn).ToString(),i,j);
			//DirectoryEntry oDE= Iadc.GetDirectoryObject(ouPath);
			
			

			DirectoryEntry oDEC=new DirectoryEntry();

			try
			{
				if(!Iadch.CheckGroup(cn))
				{
					oDEC=oDE.Children.Add("cn="+cn,"group");
                    //oDEC.Properties["grouptype"].Value = ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_GLOBAL_GROUP | ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_SECURITY_ENABLED ;
                    oDEC.Properties["sAMAccountName"].Value = cn;
					oDEC.Properties["description"].Value=description;
                     
                    oDEC.Properties["displayName"].Value = path;
					oDEC.CommitChanges();
					result=1;
				}
				else
				{
                    //移动组到正确的OU中
                    oDEC = Iads.GetGroupEntry(cn);
                    oDEC.Properties["displayName"].Value = path;
                    oDEC.CommitChanges();
                    oDEC.MoveTo(oDE);
                    
                    oDE.CommitChanges();
					result=2;
					errStr="目录已存在该组，不能重复添加";
				}
			}
			catch(Exception err)
			{
				result=0;
				errStr=err.ToString();
			}
			finally
			{
				oDE.Close();
				oDEC.Close();
			}
			
			return result;
		}



		/// <summary>
		/// 将组移动到组织单元上
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="parentcn"></param>
		public  int MoveGroupToUnit(string cn,string ou,string ouPath,out string errStr)
		{
			int result=0;
			errStr="";
			string LDAPDomain ="/"+ouPath.ToString()+ Iadc.GetLDAPDomain() ;
			LDAPDomain=ouPath;
			DirectoryEntry oDE=Iads.GetUnitEntry(ou,LDAPDomain.Substring(18));
			DirectoryEntry oDEC=Iads.GetGroupEntry(cn);

			if(!Iadch.CheckGroup(cn))return 2;

			try
			{
				oDEC.MoveTo(oDE);
				oDE.CommitChanges();
				result=1;
			}
			catch(Exception err)
			{
				result=0;
				errStr=err.ToString();
			}
			finally
			{
				oDEC.Close();
				oDE.Close();
			}

			return result;
			
		}
        /// <summary>
        /// 将组移动到组(chenl)
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="group"></param>
        /// <param name="ds"></param>
        public int MoveGroupToGroup(string cn, string group, out string errStr)
        {
            int result = 0;
            errStr = "";

            if (Iadch.CheckGroup(cn))
            {
                DirectoryEntry osubgroup = Iads.GetGroupEntry(cn);

                try
                {


                    DirectoryEntry oGroup = Iads.GetGroupEntry(group);
                    if (oGroup != null)
                    {
                        if (!oGroup.Properties["member"].Contains(osubgroup.Properties["distinguishedName"].Value))
                        {
                            oGroup.Properties["member"].Add(osubgroup.Properties["distinguishedName"].Value);
                            //					oUser.MoveTo(oGroup);
                            //					oUser.CommitChanges();
                            //					oGroup.CommitChanges();


                            oGroup.CommitChanges();
                            oGroup.Close();
                            result = 1;
                        }
                        else
                        {
                            errStr = "该用户已在本组内";
                            result = 3;
                        }
                    }

                }
                catch (Exception err)
                {
                    result = 0;
                    errStr = "系统错误";
                }
                finally
                {

                    osubgroup.Close();
                }
            }

            return result;

        }
		/// <summary>
		/// 将组移动到组织单元上
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="parentcn"></param>
		public  int MoveGroupToUnit2(string cn,string ou,out string errStr)
		{
			int result=0;
			errStr="";
//			string LDAPDomain ="/"+ouPath.ToString()+ Iadc.GetLDAPDomain() ;
//			LDAPDomain=ouPath;
			DirectoryEntry oDE=new DirectoryEntry();
			DirectoryEntry oDEC=new DirectoryEntry();

			if(!Iadch.CheckGroup(cn))return 2;

			try
			{
				oDE=Iadc.GetDirectoryObject(Iads.GetUnit(ou),1,2);
				oDEC=Iads.GetGroupEntry(cn);
				oDEC.MoveTo(oDE);
				oDE.CommitChanges();
				result=1;
			}
			catch(Exception err)
			{
				result=0;
				errStr=err.ToString();
			}
			finally
			{
				oDEC.Close();
				oDE.Close();
			}

			return result;
			
		}

		#region 注释掉的代码
//		/// <summary>
//		/// 创建组到组织单元
//		/// </summary>
//		/// <param name="cn">创建组的名称</param>
//		/// <param name="ou">目标组织单元名称</param>
//		/// <returns></returns>
//		public  DirectoryEntry  CreateNewGroupToUnit(string cn,string ou,string description)
//		{
//			string LDAPDomain =Iads.GetUnit(ou);
//			DirectoryEntry oDE= new DirectoryEntry(LDAPDomain,ADUser,ADPassword,AuthenticationTypes.Secure);
//			DirectoryEntry oDEC=new DirectoryEntry();
//			if(!Iadch.CheckGroup(cn))
//			{
//				oDEC=oDE.Children.Add("cn="+cn,"group");
//				oDEC.Properties["grouptype"].Value=ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_GLOBAL_GROUP| ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_SECURITY_ENABLED;
//				oDEC.Properties["sAMAccountName"].Value=cn;
//				oDEC.Properties["description"].Value=description;
//				oDEC.CommitChanges();
//			}
//			oDE.Close();
//			return oDEC;
//		}

		/// <summary>
		/// 移动组到组,不可移动一个组到另一个组
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="cnparent"></param>
		/// <returns></returns>
//		public  void MoveGroupToGroup(string cn,string cnparent)
//		{
//			DirectoryEntry oDEC=Iads.GetGroupEntry(cn);
//			DirectoryEntry oDE=Iads.GetGroupEntry(cnparent);
//			try
//			{
//				oDEC.MoveTo(oDE);
//			
//				//			oDE.Properties["member"].Add(oDEC.Properties["distinguishedName"].Value);
//				oDE.CommitChanges();
//			}
//			catch(Exception err)
//			{
//				string q="1";
//			}
//			finally
//			{
//				oDEC.Close();
//				oDE.Close();
//			}
//			
//		}

		#endregion

		/// <summary>
		/// 编辑组
		/// </summary>
		/// <param name="cn">编辑组的名称</param>
		/// <param name="reName">重命名名称</param>
		public  bool EditGroup(string cn,string reName,out string errStr)
		{
			bool result=false;
			errStr="";
			DirectoryEntry oDE= Iads.GetGroupEntry(cn);

			if(oDE==null)
			{
				errStr="在目录上找不到指定的组";
				return result;
			}

			try
			{
				oDE.Rename("CN="+reName);
				oDE.CommitChanges();
				result=true;
			}
			catch(Exception err)
			{
				result=false;
				errStr=err.ToString();
			}
			finally
			{
				oDE.Close();
			}
			return result;
		}



		/// <summary>
		/// 删除组
		/// </summary>
		/// <param name="cn">删除组的名称</param>
		public  int DeleteGroup(string cn,out string errStr)
		{
			int result=0;
			errStr="";
			DirectoryEntry oDE= Iads.GetGroupEntry(cn);

			if(oDE==null)
			{
				errStr="在目录上找不到指定的组";
				return 2;
			}

			try
			{
				oDE.DeleteTree();
				oDE.CommitChanges();
				result=1;
			}
			catch(Exception err)
			{
				result=0;
				errStr=err.ToString();
			}
			finally
			{
				oDE.Close();
			}
			return result;
		}
	}
}
