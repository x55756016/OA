/*�Ŀ¼������� Beta 1.0 
 * 
 * �����û��飨Ŀ¼���������������ͬ���飩
 * 
 * ����
 * 2005-4-27
 * 2005-5-27�ո���
 * */


using System;
using System.DirectoryServices;

namespace InterActiveDirectory
{
	/// <summary>
	/// ��������˵����
	/// </summary>
	public class AD_Group
	{
		#region ����
		private  string ADPath=""; 
		private  string ADUser = "";
		private  string ADPassword = "";
		private  string ADServer="";

		private AD_Common Iadc;

		private AD_seacher Iads;

		private AD_Check Iadch;

		#endregion

		/// <summary>
		/// ���캯��
		/// </summary>
		public AD_Group()
		{
			AD_Parameter adp=new AD_Parameter();
			ADPath=adp.ADPath; 
			ADUser = adp.ADPassword;
			ADPassword = adp.ADPassword;
			ADServer=adp.ADServer;

			//������������
			Iadc=new AD_Common();
			Iads=new AD_seacher();
			Iadch=new AD_Check();
		}


		/// <summary>
		/// ���캯��
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
			
			//������������
			Iadc=new AD_Common( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iads=new AD_seacher( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iadch=new AD_Check( _ADPath, _ADUser, _ADPassword, _ADServer);
		}


		/// <summary>
		/// ������鵽ָ������֯��Ԫ
		/// </summary>
		/// <param name="cn">�û���</param>
		/// <param name="ouPath">��֯��Ԫ·��(��ʽ:OU=sddsd,OU=sdsdsd,˳���ӵ���)</param>
		/// <param name="description">����</param>
		/// <returns>bool</returns>
		public  int CreateGroupToUnit(string cn,string description,string path,out string errStr)
		{
			int result=0;
			errStr="";

			//����ָ��·������֯��Ԫ����
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
                    //�ƶ��鵽��ȷ��OU��
                    oDEC = Iads.GetGroupEntry(cn);
                    oDEC.Properties["displayName"].Value = path;
                    oDEC.CommitChanges();
                    oDEC.MoveTo(oDE);
                    
                    oDE.CommitChanges();
					result=2;
					errStr="Ŀ¼�Ѵ��ڸ��飬�����ظ����";
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
		/// �����ƶ�����֯��Ԫ��
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
        /// �����ƶ�����(chenl)
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
                            errStr = "���û����ڱ�����";
                            result = 3;
                        }
                    }

                }
                catch (Exception err)
                {
                    result = 0;
                    errStr = "ϵͳ����";
                }
                finally
                {

                    osubgroup.Close();
                }
            }

            return result;

        }
		/// <summary>
		/// �����ƶ�����֯��Ԫ��
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

		#region ע�͵��Ĵ���
//		/// <summary>
//		/// �����鵽��֯��Ԫ
//		/// </summary>
//		/// <param name="cn">�����������</param>
//		/// <param name="ou">Ŀ����֯��Ԫ����</param>
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
		/// �ƶ��鵽��,�����ƶ�һ���鵽��һ����
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
		/// �༭��
		/// </summary>
		/// <param name="cn">�༭�������</param>
		/// <param name="reName">����������</param>
		public  bool EditGroup(string cn,string reName,out string errStr)
		{
			bool result=false;
			errStr="";
			DirectoryEntry oDE= Iads.GetGroupEntry(cn);

			if(oDE==null)
			{
				errStr="��Ŀ¼���Ҳ���ָ������";
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
		/// ɾ����
		/// </summary>
		/// <param name="cn">ɾ���������</param>
		public  int DeleteGroup(string cn,out string errStr)
		{
			int result=0;
			errStr="";
			DirectoryEntry oDE= Iads.GetGroupEntry(cn);

			if(oDE==null)
			{
				errStr="��Ŀ¼���Ҳ���ָ������";
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
