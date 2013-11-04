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
	/// AD_Users ��ժҪ˵����
	/// </summary>
	public class AD_Users
	{

		private AD_Common Iadc;

		private AD_seacher Iads;

		private AD_Check Iadch;

		public AD_Users()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
			Iadc=new AD_Common();
			Iads=new AD_seacher();
			Iadch=new AD_Check();
		}

		public AD_Users(string _ADPath,string _ADUser,string _ADPassword,string _ADServer)
		{
			Iadc=new AD_Common( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iads=new AD_seacher( _ADPath, _ADUser, _ADPassword, _ADServer);
			Iadch=new AD_Check( _ADPath, _ADUser, _ADPassword, _ADServer);
		}

//		/// <summary>
//		/// �������˺ŵ�ָ��������
//		/// </summary>
//		/// <param name="cn"></param>
//		/// <returns></returns>
        public bool CreateNewUserToGroup(string cn, string name, string group, out string errStr)   // modi  zl 4.26
        {
            bool result = false;


            DirectoryEntry AD = new DirectoryEntry("LDAP://" + "DC=sinomaster.com,DC=zxkj");
            DirectoryEntry NewUser = AD.Children.Add("TestUser1", "user"); //�ʺ�
            NewUser.Invoke("SetPassword", new object[] { "#12345Abc" }); // ����
            NewUser.Invoke("Put", new object[] { "Description", "Test User from .NET" });
            NewUser.CommitChanges();
            DirectoryEntry grp;

            grp = AD.Children.Find("Guests", "group");
            if (grp != null) 
            { 
                grp.Invoke("Add", new object[] { NewUser.Path.ToString() }); 
            }

            ////////////////////



            //DirectoryEntry AD = Iads.GetGroupEntry(group);
            //DirectoryEntry NewUser = AD.Children.Add("TestUser1", "user"); //�ʺ�
            //NewUser.Invoke("SetPassword", new object[] { "1234" }); // ����
            //NewUser.Invoke("Put", new object[] { "Description", "Test User from .NET" });
            //NewUser.CommitChanges();



            DirectoryEntry oDE = Iads.GetGroupEntry(group);
            errStr = "";

            

            DirectoryEntry oDEC = new DirectoryEntry();

            try
            {

                if (!Iadch.CheckUser(cn))
                {
                    oDEC = oDE.Children.Add("CN=" + cn, "user");
                    oDEC.Properties["sAMAccountName"].Value = cn;
                    oDEC.Properties["description"].Value = name;
                    oDEC.Properties["userAccountControl"].Value = ActiveDs.ADS_USER_FLAG.ADS_UF_NORMAL_ACCOUNT;

                    oDEC.CommitChanges();
                    result = true;
                }
                else
                {
                    errStr = "Ŀ¼���Ѵ���ͬ�����˺�";
                    result = false;
                }
            }
            catch (Exception err)
            {
                errStr = err.ToString();
                result = false;
            }
            finally
            {
                oDE.Close();
            }
            return result;


        }

		

		/// <summary>
		/// �����ʺ���Ϣ
		/// </summary>
		/// <param name="cn">�˺�</param>
		/// <param name="dt">givenName��,sn��,displayName��ʾ����,description��ʾ˵��,telephoneNumber,mail</param>
		/// <returns></returns>
		public  bool SetGeneral(string cn,DataTable dt,byte state)
		{
			bool result=false;
			string xx="";
			DirectoryEntry usr=Iads.GetUserEntry(cn);
			try
			{
				
				//����
				//�������Ա�,����,ѧ��,��������,�ֻ����룬�绰���룬��ͥ�绰,ͨѶ��ַ,��ͥ��ַ
				//usr.Properties["description"].Value = InitValue(dt.Rows[0]["NumName"]);//����

				usr.Properties["sn"].Value =  InitValue(cn);//Ա���ʺ�		
 				usr.Properties["displayName"].Value=InitValue(dt.Rows[0]["NumName"]);//����
				usr.Properties["mail"].Value = InitValue(dt.Rows[0]["Email"]);//�������� 
				usr.Properties["mobile"].Value =  InitValue(dt.Rows[0]["Mobile"]);//�ֻ����� 	
				usr.Properties["extensionAttribute1"].Value = "1111";//�ж�ϵͳ�����Ƿ�ɹ�
 				usr.Properties["extensionAttribute3"].Value =  InitValue(dt.Rows[0]["Sex"]);//�Ա�
 				usr.Properties["extensionAttribute4"].Value = InitValue(dt.Rows[0]["Nation"]); //����
 				usr.Properties["extensionAttribute5"].Value =  InitValue(dt.Rows[0]["EduLevel"]);//ѧ��
 				usr.Properties["extensionAttribute6"].Value =  InitValue(dt.Rows[0]["HomeTel"]);//��ͥ�绰
 				usr.Properties["extensionAttribute7"].Value =  InitValue(dt.Rows[0]["ContactAddress"]);//ͨѶ��ַ
 				usr.Properties["extensionAttribute8"].Value =  InitValue(dt.Rows[0]["FamilyAddress"]);//��ͥ��ַ	
 				usr.Properties["extensionAttribute9"].Value =  InitValue(dt.Rows[0]["CreateOptions"]);//��ͨѡ��	
                usr.Properties["extensionAttribute10"].Value = InitValue(dt.Rows[0]["extention10"]);//�Ż���Ҫ��Ϣ
				usr.Properties["extensionAttribute11"].Value =  InitValue(dt.Rows[0]["NumId"]);//	Ա������
				usr.Properties["extensionAttribute12"].Value =  InitValue(dt.Rows[0]["CardId"]);//���֤��	
				usr.Properties["extensionAttribute13"].Value =  InitValue(dt.Rows[0]["NumState"]);//Ա��״̬	
				usr.Properties["extensionAttribute14"].Value =  InitValue(dt.Rows[0]["IsMarry"]);//�Ƿ��ѻ�	
				usr.Properties["extensionAttribute15"].Value =  InitValue(dt.Rows[0]["Birthday"]);//����	

                usr.Properties["pager"].Value = InitValue(dt.Rows[0]["IsTopMan"]);//�Ƿ��ܲ���Ա(Ѱ����)

				usr.Properties["department"].Value =  InitValue(dt.Rows[0]["OrgName"]);//����	
				usr.Properties["company"].Value =  InitValue(dt.Rows[0]["CorpName"]);//��˾����	
				usr.Properties["title"].Value =  InitValue(dt.Rows[0]["PostName"]);//ְλ										
				usr.Properties["wWWHomePage"].Value = InitValue(dt.Rows[0]["DynamicPwd"]);//��̬���뿨		
				
				usr.Properties["l"].Value =state.ToString(); //==4

				usr.CommitChanges();
				result=true;
			}
			catch
			{
				result=false;
			}
			finally
			{				
				usr.Close();
			}
			return result;
		}
        /// <summary>
        /// ��ʼ����������
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool SetGeneral(string cn, string  value)
        {
            bool result = false;
            string xx = "";
            DirectoryEntry usr = Iads.GetUserEntry(cn);
            try
            {



                usr.Properties["extensionAttribute10"].Value = InitValue(value);//�Ƿ��ܲ���Ա(Ѱ����)

                

                //usr.Properties["l"].Value = state.ToString(); //==4

                usr.CommitChanges();
                result = true;
            }
            catch
            {
                result = false;
            }
            finally
            {
                usr.Close();
            }
            return result;
        }
		/// <summary>
		/// �趨�Ŀ¼�û���״̬
		/// </summary>
		/// <param name="cn">�ʺ�</param>
		/// <param name="state">״ֵ̬</param>
		/// <returns></returns>
		public bool SetState(string cn,byte state)
		{
			bool result=false;
			string xx="";
			DirectoryEntry usr=Iads.GetUserEntry(cn);
			try
			{
				usr.Properties["l"].Value =state.ToString(); 
				usr.CommitChanges();
				result=true;
			}
			catch
			{
				result=false;
			}
			finally
			{				
				usr.Close();
			}
			return result;
		}



		public void Set_Sign_User_Card(string cn,out string ErrResult)
		{
			try
			{
				

				int i=0;
				int j=0;
				ErrResult="";
				DirectoryEntry usr=Iadc.GetDirectoryObject(Iads.GetUser(cn),i,j);
				//�绰ҳ
				string system_sign=Convert.ToString(usr.Properties["extensionAttribute9"].Value);
				if(system_sign.Length==6)
				{
					string a=system_sign.Substring(0,2);
					string b=system_sign.Substring(3,3);
					usr.Properties["extensionAttribute9"].Value=a+"3"+b;
					usr.CommitChanges();
					usr.Close();
					ErrResult="";
				}
				
			}
			catch(Exception ex){ErrResult=ex.ToString();}
		}

		


		private string InitValue(object dtValue)
		{
			if(Convert.IsDBNull(dtValue))
				return "0";
			else
			{
				if(dtValue.ToString().Trim()=="")
					return "0";
				else
					return dtValue.ToString();
			}
		}


		/// <summary>
		/// ���õ�λ��Ϣ
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="dr">dr["title"],dr["department"],dr["company"]</param>
		public  void SetWorkDep(string cn,DataRow dr)
		{
			try
			{
				DirectoryEntry usr=Iads.GetUserEntry(cn);
				//��λҳ
				usr.Properties["title"].Value=dr["employeeType"].ToString().Trim();
				usr.Properties["department"].Value=dr["department"].ToString().Trim();
				usr.Properties["company"].Value=dr["company"].ToString().Trim();
				usr.Properties["l"].Value="4";
				usr.CommitChanges();
				usr.Close();
			}
			catch
			{}
		}

		/// <summary>
		/// �����û��绰
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="dr">dr["homePhone"],dr["mobile"]</param>
		public  void SetTel(string cn,DataRow dr)
		{
			try
			{
				DirectoryEntry usr=Iads.GetUserEntry(cn);
				//�绰ҳ
				if(dr["homePhone"].ToString().Trim()==""&&dr["homePhone"].ToString().Trim()==null)
				{
					usr.Properties["homePhone"].Value="1111-1111";
				}
				else
				{
					usr.Properties["homePhone"].Value=dr["homePhone"].ToString().Trim();
				}
				if(dr["mobile"].ToString().Trim()==""||dr["mobile"].ToString().Trim()==null)
				{
					usr.Properties["mobile"].Value="13500000000";
				}
				else
				{
					usr.Properties["mobile"].Value=dr["mobile"].ToString().Trim();
				}
				usr.Properties["l"].Value="4";
				usr.CommitChanges();
				usr.Close();
			}
			catch{}
		}
		public void Set_Sign(string cn, out string ErrResult)
		{
			try
			{
				int i=0;

				DirectoryEntry usr=Iadc.GetDirectoryObject(Iads.GetUnit(cn),i);
				//�绰ҳ
				
				usr.Properties["l"].Value="0";
				usr.CommitChanges();
				usr.Close();
				ErrResult="";
			}
			catch(Exception ex){ErrResult=ex.ToString();}
		}
		public void Set_Sign_User(string cn,out string ErrResult)
		{
			try
			{
				

				DirectoryEntry usr=Iads.GetUserEntry(cn);
				//�绰ҳ
				
				usr.Properties["l"].Value="0";
				usr.CommitChanges();
				usr.Close();
				ErrResult="";
			}
			catch(Exception ex){ErrResult=ex.ToString();}
		}
		/// <summary>
		/// �����ʼ���ַ
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="dr">�ʼ���ַ</param>
		public  bool SetAccount(string cn,string  mail)
		{
			bool result=false;
			DirectoryEntry usr=Iads.GetUserEntry(cn);

			try
			{				
				//�˺�ҳ
				if(mail.ToString().Trim()==""||mail.ToString().Trim()==null)
				{
					usr.Properties["userPrincipalName"].Value =cn+"@aisidi.com";
				}
				else
				{
					usr.Properties["userPrincipalName"].Value = mail.ToString().Trim();
				}
				usr.Properties["l"].Value="4";
				usr.CommitChanges();
				result=true;
			}
			catch
			{
				result=false;
			}
			finally
			{
				usr.Close();
			}
			return result;
		}

		/// <summary>
		/// �����û���ַ
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="dr">��ַ</param>
		public  void SetAddress(string cn,DataRow dr)
		{
			try
			{
				DirectoryEntry usr=Iads.GetUserEntry(cn);
				//��ַҳ
				if(dr["postalAddress"].ToString().Trim()==""||dr["postalAddress"].ToString().Trim()==null)
				{
					usr.Properties["streetAddress"].Value ="����";
				}
				else
				{
					usr.Properties["streetAddress"].Value = dr["postalAddress"].ToString().Trim();
				}
				usr.Properties["l"].Value="4";
				usr.CommitChanges();
				usr.Close();
			}
			catch{}
		}

		/// <summary>
		/// ��������
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="dr">���ܵ�����</param>
		public  void SetEncryptPassword(string cn,string encryptPassword,string oldPassword,string orgPwd, out string errStr)
		{			
			errStr="";
			//��������
			string password=Encryption.DesDecrypt(encryptPassword.ToString());
			SetUserPasswordAdmin(cn,password,oldPassword,orgPwd,out errStr)	;
			
		}

		/// <summary>
		/// ��������
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="password">δ���ܵ�����</param>
		public  void SetPassword(string cn,string password,string oldPassword,string xx,out string errStr)
		{			
			errStr="";
            SetUserPasswordAdmin(cn, password, oldPassword, xx, out errStr);
		}
		/// <summary>
		/// ���û��ƶ�����(chenl)
		/// </summary>
		/// <param name="cn"></param>
		/// <param name="group"></param>
		/// <param name="ds"></param>
		public  int MoveUserToGroup(string cn ,string group,out string errStr)
		{
			int result=0;
			errStr="";

			if(!Iadch.CheckUser(cn))
			{
				errStr="���û����ڱ�����";
				return 2;
			}
			DirectoryEntry oUser=Iads.GetUserEntry(cn);
			
			try
			{
				
				
				DirectoryEntry oGroup=Iads.GetGroupEntry(group);
				if(oGroup!=null)
				{
					if(!oGroup.Properties["member"].Contains(oUser.Properties["distinguishedName"].Value))
					{
						oGroup.Properties["member"].Add(oUser.Properties["distinguishedName"].Value);
						//					oUser.MoveTo(oGroup);
						//					oUser.CommitChanges();
						//					oGroup.CommitChanges();
						//oUser.Properties["l"].Value="4";
						//oUser.CommitChanges();
						oGroup.CommitChanges();
						result=1;
					}
					else
					{
						errStr="���û����ڱ�����";
						result=3;					
					}
				}
				else
				{
					AD_Group adgroup=new AD_Group();
					adgroup.CreateGroupToUnit(group,"","",out errStr);
					oGroup=Iads.GetGroupEntry(group);
					if(!oGroup.Properties["member"].Contains(oUser.Properties["distinguishedName"].Value))
					{
						oGroup.Properties["member"].Add(oUser.Properties["distinguishedName"].Value);
						oUser.Properties["l"].Value="4";
						oUser.CommitChanges();
						oGroup.CommitChanges();
						result=1;
					}
					else
					{
						errStr="���û����ڱ�����";
						result=3;					
					}
				}
			}
			catch(Exception err)
			{
				result=0;
				errStr="ϵͳ����";
			}
			finally
			{
				oUser.Close();
			}

			return result;
			
		}
        /// <summary>
        /// ���û��Ƴ���(chenl)
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="group"></param>
        /// <param name="ds"></param>
        public int DeleteUserToGroup(string cn, string group, out string errStr)
        {
            int result = 0;
            errStr = "";

             
            DirectoryEntry oUser = Iads.GetUserEntry(cn);

            try
            {


                DirectoryEntry oGroup = Iads.GetGroupEntry(group);
                if (oGroup != null)
                {
                    if (oGroup.Properties["member"].Contains(oUser.Properties["distinguishedName"].Value))
                    {
                        oGroup.Properties["member"].Remove(oUser.Properties["distinguishedName"].Value);
                        //					oUser.MoveTo(oGroup);
                        //					oUser.CommitChanges();
                        //					oGroup.CommitChanges();
                        //oUser.Properties["l"].Value="4";
                        //oUser.CommitChanges();
                        oGroup.CommitChanges();
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
                oUser.Close();
            }

            return result;

        }

		/// <summary>
		/// ���û��ƶ�������
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="ou">����</param>
		public  int MoveUserToUnit(string cn ,string ou,string father_OU,out string errStr)
		{
			int result=0;
			errStr="";
			string LDAPDomain ="/"+father_OU.ToString()+ Iadc.GetLDAPDomain() ;

			DirectoryEntry oUnit=new DirectoryEntry();
			DirectoryEntry oUser=new DirectoryEntry();

			if(!Iadch.CheckUnit(ou))
			{
				errStr="δ�ҵ�ָ���Ļ���/����";
				return 3;
			}

			if(!Iadch.CheckUser(cn))
			{
				errStr="δ�ҵ�ָ�����û�";
				return 2;
			}

			try
			{
				oUnit=Iads.GetUnitEntry(ou,LDAPDomain);
				oUser=Iads.GetUserEntry(cn);
				if(!oUnit.Properties["member"].Contains(oUser.Properties["distinguishedName"].Value))
				{				
					oUser.Properties["l"].Value="4";
					
					oUser.MoveTo(oUnit);
					oUser.CommitChanges();
					oUnit.CommitChanges();
					result=1;
				}
			}
			catch(Exception err)
			{
				result=0;
			}
			finally
			{
				oUser.Close();
			}
			return result;
		}
		/// <summary>
		/// ���û�����������
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="ou">����</param>
//		public  int CreateNewUserToUnit_new(string cn ,string name,string ou,string ou_path,out string errStr)
//		{
//			int result=0;
//			errStr="";
//			string LDAPDomain ="/"+ou_path.ToString()+ Iadc.GetLDAPDomain() ;
//
//			DirectoryEntry oDEC=new DirectoryEntry();
//
//			if(Iadch.CheckUser(cn))
//			{
//				errStr="�Ѵ����û�";
//				return 3;
//			}
//
//			DirectoryEntry oUnit;
//			try
//			{
//				oUnit=Iadc.GetDirectoryObject(LDAPDomain);
//				
//			}
//			catch
//			{}
//			finally
//			{}
//
//
//		}


		/// <summary>
		/// ���û�����������
		/// </summary>
		/// <param name="cn">UserName</param>
		/// <param name="ou">����</param>
		public  int CreateNewUserToUnit(string cn ,string name,string ou,string ou_path,out string errStr)
		{
			int result=0;
			errStr="";
			string LDAPDomain ="/"+ou_path.ToString()+ Iadc.GetLDAPDomain() ;

			DirectoryEntry oDEC=new DirectoryEntry();

		



			
//			if(!Iadch.CheckUnit(ou,LDAPDomain))
//			{
//				errStr="δ�ҵ�ָ���Ļ���/����";
//				return 2;
//			}

			if(Iadch.CheckUser(cn))
			{
				errStr="�Ѵ����û�";
				return 3;
			}

			DirectoryEntry oUnit=Iadc.GetDirectoryObject(LDAPDomain);

			try
			{
				oDEC=oUnit.Children.Add("CN="+cn,"user");
				oDEC.Properties["sAMAccountName"].Value=cn;
				oDEC.Properties["description"].Value=name;
				oDEC.Properties["l"].Value="2";
//				oDEC.Properties["userAccountControl"].Value = ActiveDs.ADS_USER_FLAG.ADS_UF_NORMAL_ACCOUNT;
//				
				oDEC.CommitChanges();
				result=1;
				
			}
			catch(Exception err)
			{
				errStr=err.ToString();
				result=0;
			}
			finally
			{
				oUnit.Close();
				oDEC.Close();
			}
//			DisableUserAccount(cn,out errStr);
			EnableUserAccount(cn,out errStr);
			return result;
		}
		
		



	

		/// <summary>
		/// ɾ���û�
		/// </summary>
		/// <param name="cn"></param>
		public  bool DeleteUser(string cn,out string errStr)
		{
			errStr="";
			bool result=false;

			DirectoryEntry oUser=Iads.GetUserEntry(cn);

			if(oUser==null)
			{
                errStr="�Ҳ���ָ���û�";
				return false;
			}
			
			try
			{
				oUser.DeleteTree();
				oUser.CommitChanges();
				result=true;
			}
			catch
			{
				errStr="ϵͳ����";
				result=false;
			}
			finally
			{
				oUser.Close();
			}
			return result;
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="oDE"></param>
		/// <param name="Password"></param>
		private  bool SetPsw(DirectoryEntry oDE, string Password,out string errStr)
		{
			errStr="";
			bool result=false;
			try
			{

				//oDE.Invoke("SetPassword",new Object[]{Password});
				//oDE.Invoke("setPassword", Password ); 

				

				oDE.Invoke("SetPassword", new Object[]{Password});


				oDE.CommitChanges();
				oDE.Close();
				result=true;
			}
			catch(Exception err)
			{
				errStr=err.ToString();
				result=false;
			}

			return result;
			

		}

		/// <summary>
		/// �趨�˺�����
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="newpassword"></param>
		public  void SetUserPassword(string UserName,string NewPassword,string oldPassword,string orgPassword,out string errStr)
		{
			errStr="";
			string[] strArrary=new string[4];
			string xx="";
            ChangePwdService.ChangePWD svr1 = new InterActiveDirectory.ChangePwdService.ChangePWD();
			
            //if(!svr1.Exchange_PassWord_erp(UserName,NewPassword,oldPassword,out xx))
            //{
            //    errStr=xx+"�޸�ERP���뱨��!\\n";
            //    strArrary[0]="1";
            //}
            //else
            //    strArrary[0]="0";

			if(!svr1.Exchange_PassWord_hls(UserName,NewPassword,oldPassword,out xx))
			{
				errStr=xx+"�޸�LIS���뱨��!\\n";
				strArrary[1]="1";
			}
			else
				strArrary[1]="0";
            kmoapassword.CHTWebService cht = new InterActiveDirectory.kmoapassword.CHTWebService();
            if (!cht.Edit_PassWord(UserName, NewPassword, oldPassword, out xx))
			{
				errStr=xx+"�޸�KMOA���뱨��!\\n";
				strArrary[2]="1";
			}
			else
				strArrary[2]="0";
			if(svr1.Exchange_PassWord_RTX(UserName,orgPassword,oldPassword)!="")
			{
				errStr=xx+"�޸�RTX���뱨��!\\n";	
				strArrary[3]="0";
			}
			else
				strArrary[3]="1";


			try
			{
				ADVB.AdUser advb=new ADVB.AdUser(); 
				DirectoryEntry oUser= Iads.GetUserEntry(UserName);
                AdOperate.AD_ExchangePassWord adService = new InterActiveDirectory.AdOperate.AD_ExchangePassWord();
				string er="";
                int t = adService.SetExchangePassword(UserName, orgPassword, out er)?1:0;


                //DirectoryEntry oUser = Iads.GetUserEntry(UserName);
                //SetPsw(oUser, orgPassword, out errStr);
                //oUser.Close();
				
	
				oUser.Properties["l"].Value="4";
				//oUser.Properties["physicalDeliveryOfficeName"].Value=orgPassword;		

                /********Editor:cengyp 2008��11��3��********/
                oUser.Properties["physicalDeliveryOfficeName"].Value = Encryption.DesEncrypt(orgPassword);
				oUser.Properties["extensionAttribute1"].Value="1111";		
				oUser.CommitChanges();
				oUser.Close();
			}
			catch(Exception err)
			{
				errStr=errStr+"-------"+err+"  �޸ĻĿ¼���뱨��\\n";
			}	
			
		}

//		/// <summary>
//		/// 
//		/// </summary>
//		/// <param name="oDE"></param>
//		/// <param name="Password"></param>
//		private  bool SetPsw(DirectoryEntry oDE, string Password,out string errStr)
//		{
//			errStr="";
//			bool result=false;
//			try
//			{
//				//oDE.Invoke("SetPassword",new Object[]{Password});
//				oDE.Properties["extensionAttribute10"].Value=Password;
//				oDE.Properties["l"].Value="4";
//				oDE.CommitChanges();
//				result=true;
//			}
//			catch(Exception err)
//			{
//				errStr=err.ToString();
//				result=false;
//			}
//
//			return result;
//			
//
//		}
		/// <summary>
		/// �趨�˺���Ч
		/// </summary>
		/// <param name="UserName"></param>
		public  bool EnableUserAccount(string UserName,out string errStr)
		{
			errStr="";
			return EnableUserAccount2(Iads.GetUserEntry(UserName),out errStr);
		}

		/// <summary>
		/// ��ȡ�˺ŵ�״̬
		/// </summary>
		/// <returns></returns>
		public string GetUserEnable(string UserName)
		{
			DirectoryEntry ode=Iads.GetUserEntry(UserName);
			return ode.Properties["userAccountControl"][0].ToString();
//			return AD_Enum.ADAccountOptions.UF_NORMAL_ACCOUNT.ToString();
		}


		/// <summary>
		/// enable the account by resetting all the account options excluding the disable flag
		/// </summary>
		/// <param name="oDE"></param>
		private  bool EnableUserAccount2(DirectoryEntry oDE,out string errStr)
		{
			bool result=false;
            errStr="";

			if(oDE==null)
			{
				errStr="�Ҳ���ָ�����˺�";
				return false;
			}

			try
			{
				
				oDE.Properties["userAccountControl"][0]=AD_Enum.ADAccountOptions.UF_NORMAL_ACCOUNT;
				oDE.Properties["l"].Value="4";
				oDE.CommitChanges();
				result=true;
			}
			catch(Exception err)
			{
				result=false;
				errStr="ϵͳ����";
			}
			finally
			{
				oDE.Close();
			}

			return result;
		}

		/// <summary>
		/// �趨�˺���Ч
		/// </summary>
		/// <param name="Username"></param>
		public  bool DisableUserAccount(string UserID,out string errStr)
		{
			errStr="";
			return DisableUserAccount2(Iads.GetUserEntry(UserID),out errStr);
		}

		/// <summary>
		/// Enable the user account based on the DirectoryEntry object passed to it
		/// disable the account by resetting all the default properties
		/// </summary>
		/// <param name="oDE"></param>
		private  bool DisableUserAccount2(DirectoryEntry oDE,out string errStr)
		{
			bool result=false;
			errStr="";

			if(oDE==null)
			{
                errStr="δ�ҵ�ָ�����˺�";
				return false;
			}

			try
			{
				oDE.Properties["userAccountControl"][0]=AD_Enum.ADAccountOptions.UF_ACCOUNTDISABLE;
				oDE.Properties["l"].Value="4";
				oDE.CommitChanges();
				result=false;
			}
			catch
			{
				errStr="ϵͳ����";
				result=false;
			}
			finally
			{
				oDE.Close();
			}

			return result;
		}




		/// <summary>
		/// �Թ���Ա����ݸ����û�������
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="NewPassword"></param>
		public  void SetUserPasswordAdmin (string UserName, string NewPassword ,string oldPassword,string orgPassword,out string errStr)
		{
			errStr="";
			string[] strArrary=new string[4];
			string xx="";
            ChangePwdService.ChangePWD svr1 = new InterActiveDirectory.ChangePwdService.ChangePWD();
		
            if (svr1.Exchange_PassWord_RTX(UserName, orgPassword, oldPassword) != "")
            {
                
                errStr = xx + "�޸�RTX���뱨��!\\n";
                strArrary[0] = "0";
            }
            else
            {
                strArrary[0] = "1";
            }

			try
			{
				ADVB.AdUser advb=new ADVB.AdUser(); 
				DirectoryEntry oUser= Iads.GetUserEntry(UserName);
//				advb.SetUserPasswd(oUser.Path,"123123",orgPassword);	

                AdOperate.AD_ExchangePassWord adService = new InterActiveDirectory.AdOperate.AD_ExchangePassWord();
				string er="";
                int t = adService.SetExchangePassword(UserName, orgPassword, out er)?1:0;
				//int t=adService.SetPassword_adsi(UserName,orgPassword);
//	
//				DirectoryEntry oUser= Iads.GetUserEntry(UserName);
//				SetPsw(oUser,orgPassword,out errStr);
//				oUser.Close();

				oUser.Properties["l"].Value="4";
				//oUser.Properties["initials"].Value=orgPassword;		
				oUser.Properties["extensionAttribute1"].Value="1111";		//strArrary[0]+strArrary[2]+strArrary[1]
				oUser.CommitChanges();
				oUser.Close();
			}
			catch(Exception er)
			{
				errStr=errStr+"�޸ĻĿ¼���뱨��\\n"+er.Message;
			}		
		
		}

		/// <summary>
		/// �����ʺ���Ч
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="strMsg"></param>
		/// <returns></returns>
		public void NewSetUserEnable(string userId,out string strMsg)
		{
			strMsg="";
            ExchangeServices.ExchangeServices objService = new InterActiveDirectory.ExchangeServices.ExchangeServices();
            objService.SetUserEnable(userId, out strMsg);
            //AdOperate.AD_ExchangePassWord adService = new InterActiveDirectory.AdOperate.AD_ExchangePassWord();
            //adService.SetExchangePassword(userId, false, out strMsg);
		}

		public int AddUserMailBox(string userID,string emaliSuffix,string pwd)
		{
			ADVB.AdUser  adUser=new ADVB.AdUser();
			ExchangeServices.ExchangeServices exs=new InterActiveDirectory.ExchangeServices.ExchangeServices();
			DirectoryEntry oUser= Iads.GetUserEntry(userID);
			try
			{
				oUser.Properties["l"].Value="4";
				oUser.CommitChanges();
			}
			catch{}
			return  exs.AddUserMail(Iads.GetUser(userID),userID,emaliSuffix,pwd)?1:0;
		}

		public string GetUserMail(string UserName)
		{
			DirectoryEntry oUser= Iads.GetUserEntry(UserName);
			string mail="";			
			try
			{
				mail= oUser.Properties["mail"].Value.ToString();
				
			}
			catch{}
            return mail;

		}
        public string GetUserextensionAttribute1(string UserName)
        {
            DirectoryEntry oUser = Iads.GetUserEntry(UserName);
            string extensionAttribute9 = "";
            try
            {
                extensionAttribute9 = oUser.Properties["extensionAttribute9"].Value.ToString();

            }
            catch { }
            return extensionAttribute9;

        }

//		public int AddUserMailBox_C(string userID,string emaliSuffix,string pwd)
//		{
////			CDO.PersonClass pcUser
////
////			CDOEXM.IMailboxStore MailBox;
////
////			//�������
////			MailBox=(IMailboxStore)pcUser.GetInterface("IMailboxStore");
////			MailBox.CreateMailbox(strHomeMDBURL);
////
////			pcUser.Email="SMTP:" + loginname + "@" + p_strDomainName;
////			pcUser.Fields["mailnickname"].Value="mailnickname" + loginname;
////			pcUser.Fields.Update();
////			pcUser.DataSource.Save();
//
//
//		}
		
	}
}
