/*�Ŀ¼������� Beta 1.0 
 * 
 * ������֯��Ԫ
 * 
 * ����
 * 2005-4-27
 * 2005-5-26�޸�ȫ·�����
 * */

using System;
using System.DirectoryServices;

namespace InterActiveDirectory
{
	/// <summary>
	/// AD_Unit ��ժҪ˵����
	/// </summary>
	public class AD_Unit
	{

		/// <summary>
		/// 
		/// </summary>
	    private AD_Common Iadc;

		private AD_seacher Iads;

		private AD_Check Iadch;

		/// <summary>
		/// 
		/// </summary>
		public AD_Unit()
		{			
			Iadc=new AD_Common();
			Iads=new AD_seacher();
			Iadch=new AD_Check();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_ADPath"></param>
		/// <param name="_ADUser"></param>
		/// <param name="_ADPassword"></param>
		/// <param name="_ADServer"></param>
		public AD_Unit(string _ADPath,string _ADUser,string _ADPassword,string _ADServer)
		{			
			Iadc=new AD_Common( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iads=new AD_seacher( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iadch=new AD_Check( _ADPath, _ADUser, _ADPassword, _ADServer);
		}

		/// <summary>
		///  �ڸ�Ŀ¼�´�����֯��Ԫ
		/// </summary>
		/// <param name="ou">��֯��Ԫ����</param>
		/// <returns></returns>
		public  bool CreateNewUnit(string ou,string ouName)
		{
			bool result=false;

			///��ô���DirectoryEntry �ĸ�����
			string LDAPDomain ="/"+ Iadc.GetLDAPDomain();
			DirectoryEntry oDE= Iadc.GetDirectoryObject(LDAPDomain);			
			DirectoryEntry oDEC=new DirectoryEntry();

			//У���Ƿ����ȫ��ͬ������֯��Ԫ
			if(!Iadch.CheckUnit(ou))
			{
				try
				{
					//��������֯��Ԫ
					oDEC=oDE.Children.Add("OU="+ou,"organizationalunit");
					oDEC.Properties["description"].Value=ouName;
					oDEC.Properties["l"].Value="1";
					oDEC.CommitChanges();
					result=true;
				}					 
				catch(Exception err)
				{
					result=false;
				}
				
			}			

			return result;
		}

		/// <summary>
		/// ��ָ������֯��Ԫ�´����µ���֯��Ԫ
		/// </summary>
		/// <param name="father_OU">����Ԫ·��:��ʽ��OU=father_OU_1,OU=father_OU_2,��</param>
		/// <param name="childer_OU">�ӵ�Ԫ</param>
		/// <returns></returns>
		public  int CreateNewUnit(string father_OU,string childer_OU,string childerName,out string errStr)
		{
			int result=0;	

			string LDAPDomain ="/"+father_OU.ToString()+ Iadc.GetLDAPDomain() ;

			DirectoryEntry oDE= Iadc.GetDirectoryObject(LDAPDomain);
			DirectoryEntry oDEC=new DirectoryEntry();

			if(!Iadch.CheckUnit(childer_OU,LDAPDomain))
			{
				try
				{
					oDEC=oDE.Children.Add("OU="+childer_OU,"organizationalunit");
					oDEC.Properties["description"].Value=childerName;
					oDEC.Properties["l"].Value="1";
					result=1;
					oDEC.CommitChanges();
				}
				catch(Exception err)
				{
					result=0;//δָ������
				}
				finally
				{
					
					oDEC.Close();
				}
				
			}
			else
			{
                result=2;//ָ������֯��Ԫ��
			}	
			errStr="";

			return result;
		}

		/// <summary>
		/// ��ָ������֯��Ԫ�´����µ���֯��Ԫ
		/// </summary>
		/// <param name="father_OU">����Ԫ·��:��ʽ��OU=father_OU_1,OU=father_OU_2,��</param>
		/// <param name="childer_OU">�ӵ�Ԫ</param>
		/// <returns></returns>
		public  int EditUnitName(string OU,string path,string newName,out string errStr)
		{
			int result=0;
			errStr="";			

			string LDAPDomain ="/"+path.ToString()+ Iadc.GetLDAPDomain() ;
			DirectoryEntry oDEC=Iads.GetUnitEntry(OU,LDAPDomain);
			

			if(!Iadch.CheckUnit(OU))return 2;
			
			try
			{
				oDEC.Properties["l"].Value="3";
				oDEC.Properties["description"].Value=newName;
				result=1;
				oDEC.CommitChanges();
			}
			catch
			{
				result=0;
				errStr="Err0001";//δָ������
			}
			finally
			{
				
				oDEC.Close();
			}			

			return result;
		}
		/// <summary>
		/// ��ָ������֯��Ԫ�´����µ���֯��Ԫ
		/// </summary>
		/// <param name="father_OU">����Ԫ·��:��ʽ��OU=father_OU_1,OU=father_OU_2,��</param>
		/// <param name="childer_OU">�ӵ�Ԫ</param>
		/// <returns></returns>
		public  int EditUnitName2(string OU,string path,string newName,string strOperateValue,out string errStr)
		{
			int result=0;
			errStr="";			

			//string LDAPDomain ="/"+path.ToString()+ Iadc.GetLDAPDomain() ;
			DirectoryEntry oDEC=Iadc.GetDirectoryObject(path,1,2);
			//DirectoryEntry oDEC=Iads.GetUnitEntry(OU,LDAPDomain);
			

			if(!Iadch.CheckUnit(OU))return 2;
			
			try
			{
                oDEC.Properties["l"].Value = strOperateValue;
				oDEC.Properties["description"].Value=newName;
				result=1;
				oDEC.CommitChanges();
			}
			catch
			{
				result=0;
				errStr="Err0001";//δָ������
			}
			finally
			{
				
				oDEC.Close();
			}			

			return result;
		}


		/// <summary>
		/// ��ָ������֯��Ԫ�´����µ���֯��Ԫ
		/// </summary>
		/// <param name="father_OU">����Ԫ·��:��ʽ��OU=father_OU_1,OU=father_OU_2,��</param>
		/// <param name="childer_OU">�ӵ�Ԫ</param>
		/// <returns></returns>
		public  int DeleteUnitOnState(string OU,string path,out string errStr)
		{
			int result=0;
			errStr="";			

			string LDAPDomain ="/"+path.ToString()+ Iadc.GetLDAPDomain() ;
			DirectoryEntry oDEC=Iads.GetUnitEntry(OU,LDAPDomain);			

			if(!Iadch.CheckUnit(OU))return 2;
			
			try
			{
				oDEC.Properties["l"].Value="9";				
				result=1;
				oDEC.CommitChanges();
			}
			catch
			{
				result=0;
				errStr="Err0001";//δָ������
			}
			finally
			{
				
				oDEC.Close();
			}			

			return result;
		}

		/// <summary>
		/// ��ָ������֯��Ԫ�´����µ���֯��Ԫ
		/// </summary>
		/// <param name="father_OU">����Ԫ·��:��ʽ��OU=father_OU_1,OU=father_OU_2,��</param>
		/// <param name="childer_OU">�ӵ�Ԫ</param>
		/// <returns></returns>
		public  int DeleteUnitOnState2(string OU,out string errStr)
		{
			int result=0;
			errStr="";			

//			string LDAPDomain ="/"+path.ToString()+ Iadc.GetLDAPDomain() ;
			//DirectoryEntry oDEC=Iads.GetUnitEntry(OU,LDAPDomain);	
			DirectoryEntry oDEC=new DirectoryEntry();
			oDEC=Iadc.GetDirectoryObject(Iads.GetUnit(OU),1,2);

			if(!Iadch.CheckUnit(OU))return 2;
			
			try
			{
				oDEC.Properties["l"].Value="9";				
				result=1;
				oDEC.CommitChanges();
			}
			catch
			{
				result=0;
				errStr="Err0001";//δָ������
			}
			finally
			{
				
				oDEC.Close();
			}			

			return result;
		}

		/// <summary>
		/// ��ָ������֯��Ԫ�ƶ���ָ����֯��Ԫ��
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="parentcn"></param>
		public  int MoveUnitToUnit(string ou,string childPath,string fatherOU,string fatherPath,out string errStr)
		{
			int result=0;
			errStr="";
			string LDAPDomain1 ="/"+fatherPath.ToString()+ Iadc.GetLDAPDomain() ;
			string LDAPDomain2 ="/"+childPath.ToString()+ Iadc.GetLDAPDomain() ;

			DirectoryEntry oDE=new DirectoryEntry();
			DirectoryEntry oDEC=new DirectoryEntry();

			if(!Iadch.CheckUnit(ou))return 2;

			try
			{
				oDEC=Iads.GetUnitEntry(ou,LDAPDomain2);
				
				oDEC.Properties["l"].Value="3";
				oDE=Iads.GetUnitEntry(fatherOU,LDAPDomain1);
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
		/// ��ָ������֯��Ԫ�ƶ���ָ����֯��Ԫ��
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="parentcn"></param>
		public  int MoveUnitToUnit2(string ou,string fatherOU,out string errStr)
		{
			int result=0;
			errStr="";
//			string LDAPDomain1 ="/"+fatherPath.ToString()+ Iadc.GetLDAPDomain() ;
//			string LDAPDomain2 ="/"+childPath.ToString()+ Iadc.GetLDAPDomain() ;

			DirectoryEntry oDE=new DirectoryEntry();
			DirectoryEntry oDEC=new DirectoryEntry();

			if(!Iadch.CheckUnit(ou))return 2;

			try
			{
				oDE=Iadc.GetDirectoryObject(Iads.GetUnit(fatherOU),1,2);
				oDEC=Iadc.GetDirectoryObject(Iads.GetUnit(ou),1,2);
				
				oDEC.Properties["l"].Value="3";
				
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
		/// ��������֯��Ԫ
		/// </summary>
		/// <param name="ou">�༭��֯��Ԫ������</param>
		/// <param name="reName">��֯��Ԫ����������</param>
		/// <param name="reName">������</param>
		/// <param name="father_OU">����Ԫ·��:��ʽ��OU=father_OU_1,OU=father_OU_2,��</param>
		/// <returns></returns>
		public  bool EditUnit(string ou,string reName,string newName,string father_OU,out string errStr)
		{
			bool result=false;
			errStr="";
			string LDAPDomain ="/"+father_OU.ToString()+ Iadc.GetLDAPDomain() ;

			DirectoryEntry oUnit= Iads.GetUnitEntry(ou,LDAPDomain);

			if(oUnit==null)
			{
				errStr="Err0003";
				return result;
			}

			try
			{
				oUnit.Rename("OU="+reName);
				oUnit.Properties["description"].Value=newName;
				oUnit.Properties["l"].Value="3";
				oUnit.CommitChanges();
				result=true;
			}
			catch(Exception err)
			{					
				errStr="Err0001";
				return result;
			}
			finally
			{
				oUnit.Close();
			}

			return result;
		}

		/// <summary>
		/// ɾ����֯��Ԫ
		/// </summary>
		/// <param name="ou">ɾ����֯��Ԫ������</param>
		public  bool DeleteUnit(string ou,string father_OU,out string errStr)
		{
			bool result=false;
			errStr="";
			string LDAPDomain ="/"+father_OU.ToString()+ Iadc.GetLDAPDomain() ;

			DirectoryEntry oUnit= Iads.GetUnitEntry(ou,LDAPDomain);

			if(oUnit==null)
			{
				errStr="Err0003";
				return result;
			}

			try
			{
				oUnit.DeleteTree();
				oUnit.CommitChanges();
				result=true;
			}
			catch
			{
				errStr="Err0001";
			}
			finally
			{
				oUnit.Close();
			}

            return result;
		}
	}
}
