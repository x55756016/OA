using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using System.IO;
using System.Text;

using SMT.Saas.Tools.PermissionWS;
using System.Collections.Generic;
using System.Linq;

using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.Globalization;
using System.Security.Cryptography;
using System.Reflection;
using System.Windows.Media.Imaging;
using CurrentContext = SMT.SAAS.Main.CurrentContext;
using FluxJpeg.Core;
using FluxJpeg.Core.Encoder;
using System.Xml.Linq;
using System.Xml;
using System.Resources;
using SMT.Saas.Tools.OrganizationWS;
using System.ServiceModel;
using System.ServiceModel.Channels;


namespace SMT.SaaS.FrameworkUI.Common
{
    public class Utility
    {
        private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        public static string GetMessageResource(string message)
        {

            string rslt = SMT.SaaS.Globalization.Localization.MessageResourceMgr.GetString(message, SMT.SaaS.Globalization.Localization.UiCulture);
            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }
        public static string GetResourceStr(string name)
        {
            return Localization.GetString(name);
        }
        public static string GetResourceStr(string message, string parameter)
        {
            string rslt = SMT.SaaS.Globalization.Localization.GetString(message, parameter);

            return string.IsNullOrEmpty(rslt) ? message : rslt;
        }

        /// <summary>
        /// 获取Engine XMLConfig资源信息
        /// </summary>
        /// <param name="strFormName"></param>
        /// <returns></returns>
        public static string GetFormConfigResourceValue(string strkey)
        {
            string rslt = SMT.SaaS.Globalization.Localization.GetFormConfigString(strkey);

            return string.IsNullOrEmpty(rslt) ? strkey : rslt;
        }

        /// <summary>
        /// TreeViewItem myItem =(TreeViewItem)(treeOrganization.ItemContainerGenerator.ContainerFromItem(item));
        ///CheckBox cbx = FindChildControl<CheckBox>(myItem);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindChildControl<T>(DependencyObject obj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return child as T;
                else
                {
                    T childOfChild = FindChildControl<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        /// <summary>
        /// 通过控件名在父控件中查询控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="ctrName">子控件名</param>
        /// <returns></returns>
        public static T FindChildControl<T>(DependencyObject obj, string ctrName)
          where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {

                    object value = child.GetValue(Control.NameProperty);
                    if (value != null && value.ToString() == ctrName)
                    {
                        return child as T;
                    }
                    else
                    {
                        T childOfChild = FindChildControl<T>(child, ctrName);
                        if (childOfChild != null)
                            return childOfChild;
                    }
                }
                else
                {
                    T childOfChild = FindChildControl<T>(child, ctrName);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 显示DataGrid上面通用按钮
        /// </summary>
        /// <param name="toolBar">所属工具条</param>
        /// <param name="entityName">表名称</param>
        /// <param name="displayAuditButton">是示有审核按钮</param>
        public static void DisplayGridToolBarButton(FormToolBar toolBar, string entityName, bool displayAuditButton)
        {
            //查看
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Browse) < 0)
            {
                //MessageBox.Show(Utility.GetResourceStr("NOPERMISSION"));
                //Uri uri = new Uri("/Home", UriKind.Relative);

                ////取当前主页
                //Grid grid = Application.Current.RootVisual as Grid;
                //if (grid != null && grid.Children.Count > 0)
                //{
                //    MainPage page = grid.Children[0] as MainPage;
                //    if (page != null)
                //    {
                //        page.NavigateTo(uri);
                //    }
                //}

            }
            //添加
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Add) < 0)
            {
                toolBar.btnNew.Visibility = Visibility.Collapsed;
                toolBar.retNew.Visibility = Visibility.Collapsed;
            }
            //修改
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Edit) < 0)
            {
                toolBar.btnEdit.Visibility = Visibility.Collapsed;
                toolBar.retEdit.Visibility = Visibility.Collapsed;
            }
            //删除
            if (PermissionHelper.GetPermissionValue(entityName, Permissions.Delete) < 0)
            {
                toolBar.btnDelete.Visibility = Visibility.Collapsed;
            }
            //重新提交
            //if (PermissionHelper.GetPermissionValue(entityName, Permissions.ReSubmit) < 0)
            //{
            //    toolBar.btnReSubmit.Visibility = Visibility.Collapsed;
            //}

            if (displayAuditButton)
            {
                //审核
                if (PermissionHelper.GetPermissionValue(entityName, Permissions.Audit) < 0)
                {
                    toolBar.btnAudit.Visibility = Visibility.Collapsed;
                    toolBar.retAudit.Visibility = Visibility.Collapsed;

                }
            }
            else
            {
                toolBar.btnAudit.Visibility = Visibility.Collapsed;
                toolBar.retAudit.Visibility = Visibility.Collapsed;

                toolBar.stpCheckState.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 通过选择的审核过滤条件显示相应的GridToolBar功能按钮
        /// </summary>
        /// <param name="iCheckState">审核的状态</param>
        /// <param name="toolBar">GridToolBar名</param>
        /// <param name="entityName">实体名称</param>
        public static void SetToolBarButtonByCheckState(int iCheckState, FormToolBar toolBar, string entityName)
        {
            //修改
            toolBar.btnEdit.Visibility = Visibility.Visible;
            toolBar.retEdit.Visibility = Visibility.Visible;
            //删除
            toolBar.btnDelete.Visibility = Visibility.Visible;
            toolBar.retEdit.Visibility = Visibility.Visible;
            //审核
            toolBar.btnAudit.Visibility = Visibility.Collapsed;
            toolBar.retAudit.Visibility = Visibility.Collapsed;
            //重新提交
            toolBar.ButtonReSubmit.Visibility = Visibility.Visible;
            toolBar.ButtonReSubmit.Visibility = Visibility.Visible;

            DisplayGridToolBarButton(toolBar, entityName, true);
            switch (iCheckState)
            {
                case (int)CheckStates.All:
                    //重新提交
                    toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    break;
                case (int)CheckStates.Approved:
                    //修改
                    toolBar.btnEdit.Visibility = Visibility.Collapsed;
                    toolBar.retEdit.Visibility = Visibility.Collapsed;
                    //删除
                    toolBar.btnDelete.Visibility = Visibility.Collapsed;
                    toolBar.retDelete.Visibility = Visibility.Collapsed;
                    //审核
                    toolBar.btnAudit.Visibility = Visibility.Collapsed;
                    toolBar.retAudit.Visibility = Visibility.Collapsed;
                    break;
                case (int)CheckStates.Approving:
                    //修改
                    toolBar.btnEdit.Visibility = Visibility.Collapsed;
                    toolBar.retEdit.Visibility = Visibility.Collapsed;
                    //删除
                    toolBar.btnDelete.Visibility = Visibility.Collapsed;
                    toolBar.retDelete.Visibility = Visibility.Collapsed;
                    //重新提交
                    toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    break;
                case (int)CheckStates.Delete:
                    break;
                case (int)CheckStates.UnApproved:
                    //修改
                    toolBar.btnEdit.Visibility = Visibility.Collapsed;
                    toolBar.retEdit.Visibility = Visibility.Collapsed;
                    //审核
                    toolBar.btnAudit.Visibility = Visibility.Collapsed;
                    toolBar.retAudit.Visibility = Visibility.Collapsed;
                    break;
                case (int)CheckStates.UnSubmit:
                    //重新提交
                    toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    break;
                case (int)CheckStates.WaittingApproval:
                    //修改
                    toolBar.btnEdit.Visibility = Visibility.Collapsed;
                    toolBar.retEdit.Visibility = Visibility.Collapsed;
                    //删除
                    toolBar.btnDelete.Visibility = Visibility.Collapsed;
                    toolBar.retEdit.Visibility = Visibility.Collapsed;
                    //重新提交
                    toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    //toolBar.ButtonReSubmit.Visibility = Visibility.Collapsed;
                    //审核
                    toolBar.btnAudit.Visibility = Visibility.Visible;
                    toolBar.retAudit.Visibility = Visibility.Visible;
                    break;

            }
        }


        #region  ---数据解密---

        /// <summary>   
        /// AES加密算法   
        /// </summary>   
        /// <param name="plainText">明文字符串</param>     
        /// <returns>返回加密后的密文字节数组</returns>   
        public static string AESEncrypt(string plainText)
        {
            //分组加密算法   
            string result = string.Empty;
            AesManaged aesmanaged = new AesManaged();
            try
            {
                SymmetricAlgorithm des = aesmanaged as SymmetricAlgorithm;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);
                des.Key = Encoding.UTF8.GetBytes(GetKey());
                des.IV = _key1;
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                byte[] cipherBytes = ms.ToArray();
                cs.Close();
                ms.Close();
                result = Convert.ToBase64String(cipherBytes);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }

        /// <summary>   
        /// AES解密   
        /// </summary>   
        /// <param name="hashedData">密文字节</param>     
        /// <returns>返回解密后的字符串</returns>    
        public static string AESDecrypt(string hashedData)
        {
            //分组加密算法 
            string result = string.Empty;
            SymmetricAlgorithm des;
            System.Security.Cryptography.AesManaged aesmanaged = new System.Security.Cryptography.AesManaged();
            try
            {
                des = aesmanaged as SymmetricAlgorithm;
                des.Key = Encoding.UTF8.GetBytes(GetKey());
                des.IV = _key1;
                byte[] cipherText = Convert.FromBase64String(hashedData);
                byte[] decryptBytes = new byte[cipherText.Length];
                MemoryStream ms = new MemoryStream(cipherText);
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
                cs.Read(decryptBytes, 0, decryptBytes.Length);
                cs.Close();
                ms.Close();
                result = System.Text.Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return string.IsNullOrEmpty(result) ? string.Empty : result;
        }

        /// <summary>
        /// 待定方法    //可能从数据库里获取密钥结果
        /// 获取密钥Key  密钥,128位 
        /// </summary>
        /// <returns>返回字符串类型结果</returns>
        public static string GetKey()
        {
            return "yujianhuareshgrt";
        }

        #endregion

        #region  ---字典值转换---
        /// <summary>
        /// 字典值转换
        /// </summary>
        /// <param name="typename">所属类型</param>
        /// <param name="entry">被转换的值</param>
        /// <returns>返回字典转换后的结果</returns>
        public static string GetConvertResources(string typename, string entry)
        {
            if (string.IsNullOrEmpty(entry)) return string.Empty;
            decimal temp = Convert.ToDecimal(entry);
            var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                       where a.DICTIONCATEGORY == typename && a.DICTIONARYVALUE == temp
                       select new
                       {
                           DICTIONARYNAME = a.DICTIONARYNAME
                       };
            if (ents.Count() > 0)
            {
                return ents.FirstOrDefault().DICTIONARYNAME;
            }
            return string.Empty;
        }
        #endregion

        public static string GetCheckState(CheckStates checkState)
        {
            return ((int)checkState).ToString();
        }


        #region silverlight数据加密
        /**/
        /// <summary>
        /// AES算法加密数据
        /// </summary>
        /// <param name="input">加密前的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt(string input)
        {
            // 盐值
            string saltValue = "saltValue";
            // 密码值
            string pwdValue = "pwdValue";

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(input);
            byte[] salt = System.Text.UTF8Encoding.UTF8.GetBytes(saltValue);

            // AesManaged - 高级加密标准(AES) 对称算法的管理类
            System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged();

            // Rfc2898DeriveBytes - 通过使用基于 HMACSHA1 的伪随机数生成器，实现基于密码的密钥派生功能 (PBKDF2 - 一种基于密码的密钥派生函数)
            // 通过 密码 和 salt 派生密钥
            System.Security.Cryptography.Rfc2898DeriveBytes rfc = new System.Security.Cryptography.Rfc2898DeriveBytes(pwdValue, salt);

            /**/
            /*
         * AesManaged.BlockSize - 加密操作的块大小（单位：bit）
         * AesManaged.LegalBlockSizes - 对称算法支持的块大小（单位：bit）
         * AesManaged.KeySize - 对称算法的密钥大小（单位：bit）
         * AesManaged.LegalKeySizes - 对称算法支持的密钥大小（单位：bit）
         * AesManaged.Key - 对称算法的密钥
         * AesManaged.IV - 对称算法的密钥大小
         * Rfc2898DeriveBytes.GetBytes(int 需要生成的伪随机密钥字节数) - 生成密钥
         */

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称加密器对象
            System.Security.Cryptography.ICryptoTransform encryptTransform = aes.CreateEncryptor();

            // 加密后的输出流
            System.IO.MemoryStream encryptStream = new System.IO.MemoryStream();

            // 将加密后的目标流（encryptStream）与加密转换（encryptTransform）相连接
            System.Security.Cryptography.CryptoStream encryptor = new System.Security.Cryptography.CryptoStream
                (encryptStream, encryptTransform, System.Security.Cryptography.CryptoStreamMode.Write);

            // 将一个字节序列写入当前 CryptoStream （完成加密的过程）
            encryptor.Write(data, 0, data.Length);
            encryptor.Close();

            // 将加密后所得到的流转换成字节数组，再用Base64编码将其转换为字符串
            string encryptedString = Convert.ToBase64String(encryptStream.ToArray());

            return encryptedString;
        }
        #endregion

        #region silverlight密码解密
        /**/
        /// <summary>
        /// AES算法解密数据
        /// </summary>
        /// <param name="input">加密后的字符串</param>
        /// <returns>加密前的字符串</returns>
        public static string Decrypt(string input)
        {
            // 盐值（与加密时设置的值一致）
            string saltValue = "saltValue";
            // 密码值（与加密时设置的值一致）
            string pwdValue = "pwdValue";

            byte[] encryptBytes = Convert.FromBase64String(input);
            byte[] salt = Encoding.UTF8.GetBytes(saltValue);

            System.Security.Cryptography.AesManaged aes = new System.Security.Cryptography.AesManaged();

            System.Security.Cryptography.Rfc2898DeriveBytes rfc = new System.Security.Cryptography.Rfc2898DeriveBytes(pwdValue, salt);

            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // 用当前的 Key 属性和初始化向量 IV 创建对称解密器对象
            System.Security.Cryptography.ICryptoTransform decryptTransform = aes.CreateDecryptor();

            // 解密后的输出流
            MemoryStream decryptStream = new MemoryStream();

            // 将解密后的目标流（decryptStream）与解密转换（decryptTransform）相连接
            System.Security.Cryptography.CryptoStream decryptor = new System.Security.Cryptography.CryptoStream(
                decryptStream, decryptTransform, System.Security.Cryptography.CryptoStreamMode.Write);

            // 将一个字节序列写入当前 CryptoStream （完成解密的过程）
            decryptor.Write(encryptBytes, 0, encryptBytes.Length);
            decryptor.Close();

            // 将解密后所得到的流转换为字符串
            byte[] decryptBytes = decryptStream.ToArray();
            string decryptedString = UTF8Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);

            return decryptedString;
        }

        #endregion

        /// <summary>
        /// 门户创建待审批的单据（通过引擎）
        /// </summary>
        /// <param name="AssemblyPath">业务系统Dll路径</param>
        /// <param name="FormID">业务表单ID</param>
        /// <param name="FormName">业务表单对应的Form名称</param>
        public static void CreateFormFromEngine(string assemblyName, string FormID, string FormName)
        {

            //Assembly asm = Assembly.Load("");
            // Type dalType = Type.GetType(className);
            //Type t = Type.GetType(FormName);//"SMT.HRM.UI.Form.Personnel.PensionMasterForm"

            Assembly asm = Assembly.Load(assemblyName);
            Type t = Type.GetType(FormName);
            //            //使用Activator类创建该类型的实例。 
            //            dalInstance = (IDAL<TEntity>)Activator.CreateInstance(t);
            //Assembly asm = Assembly.GetExecutingAssembly();
            //UserControl FormInstance = (UserControl)asm.CreateInstance("SMT.HRM.UI.Form.Personnel.PensionMasterForm");            
            Object[] parameters = new Object[2];    // 定义构造函数需要的参数
            parameters[0] = FormTypes.Edit;
            parameters[1] = FormID;// "5d572f2d-c0e4-49ca-960e-6bd45bfb97a9";
            //obj[0]= new object[]{FormTypes,"55"};

            //object dalInstance = asm.CreateInstance(FormName);
            //asm.CreateInstance("Reflection4.Calculator", true, BindingFlags.Default, null, parameters, null, null);
            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                //entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(entBrowser_ReloadDataEvent);
                entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
        }


        #region "加密"
        //        //方法  
        ////加密方法  
        //public    string  Encrypt(string  pToEncrypt,  string  sKey)  
        //{  
        //           DESCryptoServiceProvider  des  =  new  DESCryptoServiceProvider();  
        //           //把字符串放到byte数组中  
        //                 //原来使用的UTF8编码，我改成Unicode编码了，不行  
        //           byte[]  inputByteArray  =  Encoding.Default.GetBytes(pToEncrypt);  
        //           //byte[]  inputByteArray=Encoding.Unicode.GetBytes(pToEncrypt);  

        //           //建立加密对象的密钥和偏移量  
        //           //原文使用ASCIIEncoding.ASCII方法的GetBytes方法  
        //           //使得输入密码必须输入英文文本  
        //           des.Key  =  ASCIIEncoding.ASCII.GetBytes(sKey);  
        //           des.IV  =  ASCIIEncoding.ASCII.GetBytes(sKey);  
        //           MemoryStream  ms  =  new  MemoryStream();  
        //           CryptoStream  cs  =  new  CryptoStream(ms,  des.CreateEncryptor(),CryptoStreamMode.Write);  
        //           //Write  the  byte  array  into  the  crypto  stream  
        //           //(It  will  end  up  in  the  memory  stream)  
        //           cs.Write(inputByteArray,  0,  inputByteArray.Length);  
        //           cs.FlushFinalBlock();  
        //           //Get  the  data  back  from  the  memory  stream,  and  into  a  string  
        //           StringBuilder  ret  =  new  StringBuilder();  
        //           foreach(byte  b  in  ms.ToArray())  
        //                       {  
        //                       //Format  as  hex  
        //                       ret.AppendFormat("{0:X2}",  b);  
        //                       }  
        //           ret.ToString();  
        //           return  ret.ToString();  
        //}  

        ////解密方法  
        //public    string  Decrypt(string  pToDecrypt,  string  sKey)  
        //{  
        //           DESCryptoServiceProvider  des  =  new  DESCryptoServiceProvider();  

        //           //Put  the  input  string  into  the  byte  array  
        //           byte[]  inputByteArray  =  new  byte[pToDecrypt.Length  /  2];  
        //           for(int  x  =  0;  x  <  pToDecrypt.Length  /  2;  x++)  
        //           {  
        //                     int  i  =  (Convert.ToInt32(pToDecrypt.Substring(x  *  2,  2),  16));  
        //               inputByteArray[x]  =  (byte)i;  
        //           }  

        //           //建立加密对象的密钥和偏移量，此值重要，不能修改  
        //           des.Key  =  ASCIIEncoding.ASCII.GetBytes(sKey);  
        //           des.IV  =  ASCIIEncoding.ASCII.GetBytes(sKey);  
        //           MemoryStream  ms  =  new  MemoryStream();  
        //           CryptoStream  cs  =  new  CryptoStream(ms,  des.CreateDecryptor(),CryptoStreamMode.Write);  
        //           //Flush  the  data  through  the  crypto  stream  into  the  memory  stream  
        //           cs.Write(inputByteArray,  0,  inputByteArray.Length);  
        //           cs.FlushFinalBlock();  

        //           //Get  the  decrypted  data  back  from  the  memory  stream  
        //           //建立StringBuild对象，CreateDecrypt使用的是流对象，必须把解密后的文本变成流对象  
        //           StringBuilder  ret  =  new  StringBuilder();  

        //           return  System.Text.Encoding.Default.GetString(ms.ToArray());  
        //}  


        //    #region DES加密字符串 
        ///**//// <summary> 
        ///// 加密字符串 
        ///// 注意:密钥必须为８位 
        ///// </summary> 
        ///// <param name="strText">字符串</param> 
        ///// <param name="encryptKey">密钥</param> 


        //public void DesEncrypt() 
        //{ 
        //byte[] byKey=null; 
        //byte[] IV= {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF}; 
        //try 
        //{ 
        //byKey = System.Text.Encoding.UTF8.GetBytes(this.encryptKey.Substring(0,8)); 
        //DESCryptoServiceProvider des = new DESCryptoServiceProvider(); 
        //byte[] inputByteArray = Encoding.UTF8.GetBytes(this.inputString); 
        //MemoryStream ms = new MemoryStream(); 
        //CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write) ; 
        //cs.Write(inputByteArray, 0, inputByteArray.Length); 
        //cs.FlushFinalBlock(); 
        //this.outString=Convert.ToBase64String(ms.ToArray()); 
        //} 
        //catch(System.Exception error) 
        //{ 
        //this.noteMessage=error.Message; 
        //} 
        //} 
        //#endregion 

        //    #region DES解密字符串 
        ///**//// <summary> 
        ///// 解密字符串 
        ///// </summary> 
        ///// <param name="this.inputString">加了密的字符串</param> 
        ///// <param name="decryptKey">密钥</param>  
        //public void DesDecrypt() 
        //{ 
        //byte[] byKey = null; 
        //byte[] IV= {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF}; 
        //byte[] inputByteArray = new Byte[this.inputString.Length]; 
        //try 
        //{ 
        //byKey = System.Text.Encoding.UTF8.GetBytes(decryptKey.Substring(0,8)); 
        //DESCryptoServiceProvider des = new DESCryptoServiceProvider(); 
        //inputByteArray = Convert.FromBase64String(this.inputString); 
        //MemoryStream ms = new MemoryStream(); 
        //CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write); 
        //cs.Write(inputByteArray, 0, inputByteArray.Length); 
        //cs.FlushFinalBlock(); 
        //System.Text.Encoding encoding = new System.Text.UTF8Encoding(); 
        //this.outString=encoding.GetString(ms.ToArray()); 
        //} 
        //catch(System.Exception error) 
        //{ 
        //this.noteMessage=error.Message; 
        //} 
        //} 
        //#endregion 

        //    #region DES加密文件 
        ///**//// <summary> 
        ///// DES加密文件 
        ///// </summary> 
        ///// <param name="this.inputFilePath">源文件路径</param> 



        ///// <param name="this.outFilePath">输出文件路径</param> 
        ///// <param name="encryptKey">密钥</param> 
        //public void FileDesEncrypt() 
        //{ 
        //byte[] byKey=null; 
        //byte[] IV= {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF}; 
        //try 
        //{ 
        //byKey = System.Text.Encoding.UTF8.GetBytes(this.encryptKey.Substring(0,8)); 
        //FileStream fin = new FileStream(this.inputFilePath, FileMode.Open, FileAccess.Read); 
        //FileStream fout = new FileStream(this.outFilePath, FileMode.OpenOrCreate, FileAccess.Write); 
        //fout.SetLength(0); 
        ////Create variables to help with read and write. 
        //byte[] bin = new byte[100]; //This is intermediate storage for the encryption. 
        //long rdlen = 0; //This is the total number of bytes written. 
        //long totlen = fin.Length; //This is the total length of the input file. 
        //int len; //This is the number of bytes to be written at a time. 
        //DES des = new DESCryptoServiceProvider(); 
        //CryptoStream encStream = new CryptoStream(fout, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write); 



        ////Read from the input file, then encrypt and write to the output file. 
        //while(rdlen < totlen) 
        //{ 
        //len = fin.Read(bin, 0, 100); 
        //encStream.Write(bin, 0, len); 
        //rdlen = rdlen + len; 
        //} 


        //encStream.Close(); 
        //fout.Close(); 
        //fin.Close(); 


        //} 
        //catch(System.Exception error) 
        //{ 
        //this.noteMessage=error.Message.ToString(); 




        //} 
        //} 
        //#endregion 

        //    #region DES解密文件 
        //    /**//// <summary> 
        //    /// 解密文件 
        //    /// </summary> 
        //    /// <param name="this.inputFilePath">加密了的文件路径</param> 
        //    /// <param name="this.outFilePath">输出文件路径</param> 
        //    /// <param name="decryptKey">密钥</param> 
        //    public void FileDesDecrypt() 
        //    { 
        //    byte[] byKey = null; 
        //    byte[] IV= {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF}; 
        //    try 
        //    { 
        //    byKey = System.Text.Encoding.UTF8.GetBytes(decryptKey.Substring(0,8)); 
        //    FileStream fin = new FileStream(this.inputFilePath, FileMode.Open, FileAccess.Read); 
        //    FileStream fout = new FileStream(this.outFilePath, FileMode.OpenOrCreate, FileAccess.Write); 
        //    fout.SetLength(0); 
        //    //Create variables to help with read and write. 
        //    byte[] bin = new byte[100]; //This is intermediate storage for the encryption. 
        //    long rdlen = 0; //This is the total number of bytes written.  
        //    long totlen = fin.Length; //This is the total length of the input file. 
        //    int len; //This is the number of bytes to be written at a time. 
        //    DES des = new DESCryptoServiceProvider(); 
        //    CryptoStream encStream = new CryptoStream(fout, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);  


        //    //Read from the input file, then encrypt and write to the output file. 
        //    while(rdlen < totlen) 
        //    { 
        //    len = fin.Read(bin, 0, 100); 
        //    encStream.Write(bin, 0, len); 
        //    rdlen = rdlen + len; 
        //    } 

        //    encStream.Close(); 
        //    fout.Close(); 
        //    fin.Close(); 
        //    } 
        //    catch(System.Exception error) 
        //    { 
        //    this.noteMessage=error.Message.ToString(); 
        //    } 
        //    } 
        //    #endregion 

        //    #region MD5 
        //    /**//// <summary> 
        //    /// MD5 Encrypt 
        //    /// </summary> 
        //    /// <param name="strText">text</param> 
        //    /// <returns>md5 Encrypt string</returns> 
        //    public void MD5Encrypt() 
        //    { 
        //    MD5 md5 = new MD5CryptoServiceProvider(); 
        //    byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(this.inputString)); 
        //    this.outString=System.Text.Encoding.Default.GetString(result); 
        //    } 
        //    #endregion 
        #endregion


        /// <summary>
        /// 引擎需要的XML形式的实体字符串转化 by 罗旋
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectdata"></param>
        /// <param name="SystemCode"></param>
        /// <returns></returns>
        public static string ObjListToXml<T>(T objectdata, string SystemCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<System>");
            Type objtype = objectdata.GetType();
            sb.AppendLine("<Name>" + SystemCode + "</Name>");
            sb.AppendLine("<Object Name=\"Approval\" Description=\"\">");
            PropertyInfo[] propinfos = objtype.GetProperties();
            foreach (PropertyInfo propinfo in propinfos)
            {
                if (propinfo.Name.ToUpper() != "CHECKSTATE")
                {
                    sb.AppendLine("<Attribute Name=\"" + propinfo.Name + "\" Description=\"" + "" + "\" DataType=\"" + "" + "\" DataValue=\"" + propinfo.GetValue(objectdata, null) + "\"/>");
                }
            }
            //sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentConfig.CurrentUser.UserInfo.EMPLOYEENAME + "\"/>");
            //替换为:
            //sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName + "\"/>");
            //修改原因：
            //Common.CurrentConfig.CurrentUser.UserInfo已过期，被Common.CurrentLoginUserInfo代替
            //GaoYan 2010-12-15

            sb.AppendLine("<Attribute Name=\"" + "CURRENTEMPLOYEENAME" + "\" Description=\"" + "提交者" + "\" DataType=\"" + "" + "\" DataValue=\"" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName + "\"/>");
            sb.AppendLine("</Object>");
            sb.AppendLine("</System>");
            return sb.ToString();

        }

        #region  城市值转换
        public static string GetCityName(string cityvalue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CITY" && a.DICTIONARYVALUE == Convert.ToDecimal(cityvalue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region 提交表单时将所选的用户信息传给流程
        public static void SetAuditEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity, string modelcode, string formid, string strXmlObjectSource, Dictionary<string, string> paras)
        {
            entity.ModelCode = modelcode;//"archivesLending";T_HR_COMPANY
            entity.FormID = formid; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
            entity.CreateCompanyID = paras["CreateCompanyID"];
            entity.CreateDepartmentID = paras["CreateDepartmentID"];
            entity.CreatePostID = paras["CreatePostID"];
            entity.CreateUserID = paras["CreateUserID"];

            entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.XmlObject = strXmlObjectSource;
        }

        public static void SetAuditEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity, string modelcode, string formid, string strXmlObjectSource)
        {

            entity.ModelCode = modelcode;//"archivesLending";T_HR_COMPANY
            entity.FormID = formid; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
            entity.CreateCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;// "7cd6c0a4-9735-476a-9184-103b962d3383";
            entity.CreateDepartmentID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entity.CreatePostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entity.CreateUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entity.XmlObject = strXmlObjectSource;
        }
        #endregion

        #region 获取富文本值 转化二进制数组方法
        /// <summary>
        /// 获取富文本框的值-向寒咏
        /// </summary>
        /// <param name="box">富文本框</param>
        /// <returns>返回byte[]类型的值</returns>
        public static byte[] GetRichTextBoxContext(RichTextBox box)
        {
            long bytelength = 0;
            object obj = box.DataContext;
            BlockCollection bco = box.Blocks;

            using (MemoryStream ms = new MemoryStream())
            {
                string StrBlocks = string.Empty;
                byte[] byall = null;
                byte[] bylist = null;
                byte[] start = new byte[1024];
                bool isPic = false;
                int s = 0;
                for (int i = 0; i < box.Blocks.Count; i++)
                {
                    Paragraph aPara = (Paragraph)box.Blocks[i];

                    byte[] bytes = null;
                    for (int j = 0; j < aPara.Inlines.Count; j++)
                    {


                        if (byall != null)
                        {
                            bylist = byall;
                        }
                        TextElement element = (TextElement)aPara.Inlines[j];

                        if (element.GetType().Equals(typeof(Run)))
                        {
                            StringBuilder buder = new StringBuilder();
                            Run aRun = (Run)element;
                            buder.Append("あ");
                            buder.Append(aRun.FontSize.ToString());
                            buder.Append("卐");
                            buder.Append(aRun.FontFamily.ToString());
                            buder.Append("卐");
                            SolidColorBrush brush = (SolidColorBrush)aRun.Foreground;
                            Color cl = brush.Color;
                            string clstr = cl.ToString();
                            buder.Append(clstr);
                            buder.Append("卐");
                            buder.Append(aRun.FontWeight.ToString());
                            buder.Append("卐");
                            buder.Append(aRun.FontStyle.ToString());
                            buder.Append("卐");
                            buder.Append(aRun.FontStretch.ToString());
                            buder.Append("卐");
                            if (aRun.TextDecorations != null)
                                buder.Append(TextDecorations.Underline.ToString());

                            buder.Append("あ");

                            StrBlocks += aRun.Text;
                            bytes = Encoding.UTF8.GetBytes(buder.ToString() + StrBlocks);
                            StrBlocks = "";
                            bytelength += bytes.Length;
                            isPic = false;
                        }
                        if (element.GetType().Equals(typeof(Hyperlink)))
                        {
                            Hyperlink hyperlink = (Hyperlink)element;
                            string hyurl = hyperlink.NavigateUri.ToString();
                            bytes = Encoding.UTF8.GetBytes(hyurl);
                            bytelength += bytes.Length;

                        }
                        if (element.GetType().Equals(typeof(InlineUIContainer)))
                        {
                            s++;
                            InlineUIContainer inline = (InlineUIContainer)element;
                            BitmapImage bi = (BitmapImage)((System.Windows.Controls.Image)inline.Child).Source;
                            WriteableBitmap wb = new WriteableBitmap(bi);//将Image对象转换为WriteableBitmap
                            bytes = Convert.FromBase64String(GetBase64Image(wb));//得到byte数组

                            //if (StrBlocks == "\n")
                            //{
                            //    bytelength += 1;
                            //}
                            byte[] stabt = BitConverter.GetBytes(bytelength);
                            if (s == 1)
                            {
                                stabt.CopyTo(start, 0);
                            }
                            else
                            {
                                stabt.CopyTo(start, 8 * s);
                            }
                            bytelength += bytes.Length;
                            byte[] end = BitConverter.GetBytes(bytelength);
                            if (s == 1)
                            {
                                end.CopyTo(start, 8);
                            }
                            else
                            {
                                s++;
                                end.CopyTo(start, 8 * s);
                            }
                            StrBlocks = "";
                            isPic = true;
                        }
                        byall = new byte[bytelength + 1024];
                        if (bylist != null)
                        {
                            if (isPic)
                            {
                                bylist.CopyTo(byall, 0);
                                start.CopyTo(byall, 0);
                            }
                            else
                            {
                                bylist.CopyTo(byall, 0);
                            }
                        }
                        if (j == 0)
                        {
                            start.CopyTo(byall, 0);
                            if (bylist == null)
                            {
                                bytes.CopyTo(byall, 1024);
                            }
                            else
                            {
                                bytes.CopyTo(byall, bylist.Length);
                            }
                        }
                        else
                        {
                            bytes.CopyTo(byall, bylist.Length);
                        }

                    }
                    StrBlocks = "\n";
                }
                return byall;
            }
        }
        #endregion

        #region 转化流文件操作

        /// <summary>
        /// 将WriteableBitmap转化为base64位字符串  
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static string GetBase64Image(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int bands = 3;
            byte[][,] raster = new byte[bands][,];

            for (int i = 0; i < bands; i++)
            {
                raster[i] = new byte[width, height];
            }

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int pixel = bitmap.Pixels[width * row + column];
                    raster[0][column, row] = (byte)(pixel >> 16);
                    raster[1][column, row] = (byte)(pixel >> 8);
                    raster[2][column, row] = (byte)pixel;
                }
            }

            ColorModel model = new ColorModel { colorspace = ColorSpace.RGB };
            FluxJpeg.Core.Image img = new FluxJpeg.Core.Image(model, raster);
            MemoryStream stream = new MemoryStream();
            JpegEncoder encoder = new JpegEncoder(img, 100, stream);
            encoder.Encode();

            stream.Seek(0, SeekOrigin.Begin);
            byte[] binaryData = new Byte[stream.Length];
            long bytesRead = stream.Read(binaryData, 0, (int)stream.Length);

            string base64String =
                    System.Convert.ToBase64String(binaryData,
                                                  0,
                                                  binaryData.Length);

            return base64String;

        }

        /// <summary>
        /// 将BitmapImag转化为二进制流
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
            byte[] byteArray = null;

            try
            {
                Stream sMarket = null;

                if (sMarket != null && sMarket.Length > 0)
                {
                    //很重要，因为Position经常位于Stream的末尾，导致下面读取到的长度为0。 
                    sMarket.Position = 0;

                    using (BinaryReader br = new BinaryReader(sMarket))
                    {
                        byteArray = br.ReadBytes((int)sMarket.Length);
                    }
                }
            }
            catch
            {
                //other exception handling 
            }

            return byteArray;
        }

        /// 将Image对象转化成二进制流 
        ///  </summary> 
        ///  <param name="image"> </param> 
        ///  <returns> </returns> 
        public byte[] ImageToByteArray(System.Windows.Controls.Image image)
        {
            //实例化流 
            MemoryStream imageStream = new MemoryStream();
            //将图片的实例保存到流中           

            //保存流的二进制数组 
            byte[] imageContent = new Byte[imageStream.Length];

            imageStream.Position = 0;
            //将流泻如数组中 
            imageStream.Read(imageContent, 0, (int)imageStream.Length);

            return imageStream.ToArray();

        }

        /// <summary>
        /// 将byte[]转化为整形数组
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int[] Byte64toLong(byte[] start)
        {
            byte[] bylong = new byte[8];
            int[] val = new int[start.Length / 8];

            try
            {
                //  Array arr = new Array[start.Length / 8];
                for (int i = 0; i < start.Length / 8; i++)
                {
                    Array.Copy(start, 8 * i, bylong, 0, 8);
                    long a = BitConverter.ToInt64(bylong, 0);

                    if (i == 1)
                    {
                        if (a == 0)
                        {
                            val = null;
                            break;
                        }
                    }
                    if (i > 0)
                    {
                        if (a == 0)
                        {
                            break;
                        }
                    }
                    val[i] = (int)a;
                }
            }
            catch (Exception )
            {
                val = null;
            }


            return val;

        }

        /// <summary>
        /// 图片控件加载图片方法
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static System.Windows.Controls.Image ByteToImage(byte[] image, double width, double height)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Stretch = Stretch.Uniform;

            BitmapImage bi = new BitmapImage();
            try
            {
                bi.SetSource(new MemoryStream(image));
                img.Source = bi;
                img.Tag = bi.UriSource.ToString();
                img.Width = bi.PixelWidth;//图片宽度
                img.Height = bi.PixelHeight;//图片高度
            }
            catch
            {
                return null;
            }

            return img;
        }

        private static void ReturnFocus(RichTextBox box)
        {
            if (box != null)
                box.Focus();
        }

        #endregion

        #region 富文本取值 将Byte数组 转化为文字和图片
        /// <summary>
        /// 富文本框赋值-向寒咏
        /// </summary>
        /// <param name="box">富文本框</param>
        /// <param name="RichBoxData">保存的值-Byte[]类型</param>
        public static void SetRichTextBoxData(RichTextBox box, byte[] RichBoxData)
        {
            box.Blocks.Clear();
            if (RichBoxData == null || RichBoxData.Length == 0)
            {
                return;
            }
            byte[] start = new byte[1024];   //图片定位数组
            if (RichBoxData.Length > 1024)
                Array.Copy(RichBoxData, 0, start, 0, 1024);

            long starleng = 0;     //开始截取的位置
            long endleng = 0;     //结束截取的位置
            long endy = 0;         //获取上次结束的位置
            bool isendtext = false;  //不存在图片
            int[] lengst = Byte64toLong(start);  //转化定位数组
            if (RichBoxData.Length > 1024)
            {
                for (int x = 1024; x < RichBoxData.Length; x++)   //内容进行循环   位置从开始匹配
                {
                    if (lengst != null)
                    {
                        endy = endleng;
                        for (int a = 0; a < lengst.Length; a++)   //定位数组进行循环
                        {
                            if (a % 2 == 0)
                            {
                                starleng = long.Parse(lengst[a].ToString());  //获取开始出现图片位置
                                continue;
                            }
                            else
                            {
                                endleng = long.Parse(lengst[a].ToString());   //获取结束图片位置

                                if (endleng == 0)
                                {
                                    isendtext = true;
                                }
                            }
                            if (starleng == x - 1024 && isendtext == false)                      //当前位置是图片位置 进行处理
                            {
                                byte[] imageByte = new byte[endleng - starleng];
                                Array.Copy(RichBoxData, (int)starleng + 1024, imageByte, 0, (int)(endleng - starleng));
                                SetImageBindBox(box, imageByte);  //控件绑定
                                x = (int)endleng + 1024;
                                if (x == RichBoxData.Length - 2)
                                {
                                    x++;
                                }

                            }  // 处理文字信息
                            else
                            {
                                byte[] textByte = null;
                                if (starleng == 0 && isendtext == true)          //所有都是文字处理方式
                                {
                                    textByte = new byte[RichBoxData.Length - x];
                                    if (RichBoxData.Length == x)
                                    {
                                        break;
                                    }
                                    Array.Copy(RichBoxData, x, textByte, 0, (int)(RichBoxData.Length - x));
                                }

                                if (starleng > x - 1024)    // 当前位置小于开始图片位置
                                {
                                    textByte = new byte[starleng - x + 1024];
                                    Array.Copy(RichBoxData, x, textByte, 0, (int)(starleng - x + 1024));
                                }

                                SetTextBindBox(box, textByte, 1);
                                //using (MemoryStream stream = new MemoryStream(textByte))
                                //{
                                //    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                                //    {
                                //        string Xaml = reader.ReadToEnd();
                                //        Run myRun = new Run();
                                //        myRun.Text = Xaml;
                                //        box.Selection.Insert(myRun);

                                //    }

                                //}
                                if (starleng == 0)          //所有都是文字处理方式  没有图片时跳出循环
                                {
                                    x = RichBoxData.Length - 1;
                                    break;
                                }
                                if (starleng > x - 1024)    //当当前位置
                                {
                                    x = 1024 + (int)starleng;
                                    byte[] imageByte = new byte[endleng - starleng];
                                    Array.Copy(RichBoxData, x, imageByte, 0, (int)(endleng - starleng));
                                    SetImageBindBox(box, imageByte);  //控件绑定
                                    x = (int)endleng + 1024;
                                    if (x == RichBoxData.Length - 2)
                                    {
                                        x++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        #region 判断没有图片的时候
                        if (starleng == 0 && endleng == 0)
                        {
                            byte[] strByte = new byte[RichBoxData.Length - 1024];
                            Array.Copy(RichBoxData, 1024, strByte, 0, RichBoxData.Length - 1024);
                            SetTextBindBox(box, strByte, 1);

                            break;
                        }
                        #endregion
                    }

                }
            }
            else
            {
                //SetTextBindBox(box, RichBoxData,1);
                using (MemoryStream stream = new MemoryStream(RichBoxData))
                {
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        string Xaml = reader.ReadToEnd();
                        Run myRun = new Run();
                        myRun.Text = Xaml;
                        box.Selection.Insert(myRun);

                    }

                }
            }
            //box.SelectAll();
            //box.Selection.ApplyPropertyValue(Run.FontSizeProperty, "12");
            TextPointer startPointer = box.ContentStart.GetNextInsertionPosition(LogicalDirection.Forward);
            TextPointer MyTP1 = startPointer.GetPositionAtOffset(1, LogicalDirection.Forward);
            box.Selection.Select(startPointer, MyTP1);
            box.Focus();

        }
        #endregion

        #region 富文本绑定 图片和文本
        /// <summary>
        /// 富文本绑定图片数组
        /// </summary>
        /// <param name="box"></param>
        /// <param name="imageByte"></param>
        public static void SetImageBindBox(RichTextBox box, byte[] imageByte)
        {
            InlineUIContainer container = new InlineUIContainer();
            container.Child = ByteToImage(imageByte, 200, 150);
            box.Selection.Insert(container);
        }
        /// <summary>
        /// 富文本框绑定 文本数组
        /// </summary>
        /// <param name="box"></param>
        /// <param name="RichBoxData"></param>
        public static void SetTextBindBox(RichTextBox box, byte[] RichBoxData, int type)
        {

            using (MemoryStream stream = new MemoryStream(RichBoxData))
            {
                //DataContractSerializer serializer = new DataContractSerializer(typeof(string));
                using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    //string Xaml = (string)serializer.ReadObject(stream);
                    string Xaml = reader.ReadToEnd();
                    //txtContent.GetRichTextbox().Xaml = Xaml;
                    // box.Blocks.Clear();
                    Paragraph myPara = new Paragraph();

                    string[] arr = null;
                    try
                    {
                        arr = Xaml.Split('あ');
                        for (int i = 1; i < arr.Length; i += 2)
                        {
                            Run myRun = new Run();
                            string but = arr[i].ToString();
                            string[] lad = but.Split('卐');
                            myRun.FontSize = double.Parse(lad[0].ToString());
                            FontFamily fot = new FontFamily(lad[1].ToString());
                            string color = lad[2].ToString();
                            myRun.FontFamily = fot;
                            Color cl = Color.FromArgb(byte.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                                                      byte.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                                                      byte.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                                                      byte.Parse(color.Substring(7, 2), System.Globalization.NumberStyles.HexNumber));
                            SolidColorBrush brush = new SolidColorBrush(cl);
                            myRun.Foreground = brush;
                            if (lad[3].ToString() == "Normal")
                            {
                                myRun.FontWeight = FontWeights.Normal;
                            }
                            else
                            {
                                myRun.FontWeight = FontWeights.Bold;
                            }
                            if (lad[4].ToString() == "Normal")
                            {
                                myRun.FontStyle = FontStyles.Normal;
                            }
                            else
                            {
                                myRun.FontStyle = FontStyles.Italic;
                            }
                            FontStretch a = new FontStretch();
                            myRun.FontStretch = a;
                            if (lad[6].ToString() == "Underline")
                                myRun.TextDecorations = TextDecorations.Underline;
                            myRun.Text = arr[i + 1].ToString();

                            if (type == 1)
                            {
                                myPara.Inlines.Add(myRun);

                            }
                            else
                            {
                                box.Selection.Insert(myRun);
                            }
                        }
                        //    myRun.TextDecorations = TextDecorations.;
                    }
                    catch (Exception )
                    {
                        Run myRuns = new Run();
                        myRuns.Text = Xaml;
                        myPara.Inlines.Add(myRuns);
                        box.Blocks.Add(myPara);
                    }

                    box.Blocks.Add(myPara);

                    // Paragraph myPara = new Paragraph();


                }

            }
        }
        #endregion

        /// <summary>
        /// 将字符串赋到富文本框中
        /// </summary>
        /// <param name="box">富文本框</param>
        /// <param name="Str">需要放入富文本框中的字符串</param>
        public static void SetRichTextBoxDataByString(RichTextBox box, string Str)
        {
            Paragraph pg = new Paragraph();
            pg.Inlines.Add(Str);
            box.Blocks.Add(pg);
        }

        /// <summary>
        /// 验证控件是否合法，不合法弹出提示
        /// </summary>
        /// <param name="Group1"></param>
        /// <returns></returns>
        public static bool CheckDataIsValid(SMT.SaaS.FrameworkUI.Validator.ValidatorManager Group1)
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    //ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage), Utility.GetResourceStr("CONFIRMBUTTON"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(h.ErrorMessage),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }
            return true;
        }

        public static void ShowCustomMessage(MessageTypes messageType, string title, string message)
        {
            // ErrorWindow ewin = new ErrorWindow(title, message);
            // ewin.Show();
            ComfirmWindow.ConfirmationBox(title, message, Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        public static bool ToolBarButtonByOperationControl(object obj, OperationType Operation, string UserId)
        {
            string menuCode = obj.GetType().Name;
            
            int OperationValue = Convert.ToInt32(Operation);//操作方式
            int PermValue = -1;
            Type objName = obj.GetType();
            if (CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI != null)
            {
                var objs = from o in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                           where o.PermissionValue == Convert.ToInt32(OperationValue).ToString()
                           && o.MenuCode == menuCode
                           select o;
                if (objs != null && objs.Count() > 0)
                {
                    PermValue = objs.Min(p => Convert.ToInt32(p.DataRange));
                }

                PropertyInfo[] infos = objName.GetProperties();
                foreach (PropertyInfo prop in infos)
                {

                    switch (PermValue)
                    {
                        case 1:
                            if (prop.Name == "OWNERCOMPANYID")
                            {
                                //公司
                                if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID)
                                {
                                    return true;
                                }
                            }
                            break;
                        case 2:
                            if (prop.Name == "OWNERDEPARTMENTID")
                            {
                                //部门
                                if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID)
                                {
                                    return true;
                                }
                            }
                            break;
                        case 3:
                            if (prop.Name == "OWNERPOSTID")
                            {
                                //岗位
                                if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID)
                                {
                                    return true;
                                }
                            }
                            break;
                        case 4:
                            if (prop.Name == "OWNERID")
                            {
                                //个人
                                if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
                                {
                                    return true;
                                }
                            }
                            break;
                    }
                }

                //自定义权限
                foreach (var objcustomer in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI)
                {
                    var Menus = objcustomer.CustomerPermission;
                    if (Menus == null) continue;

                    if (Menus.PermissionValue == null)
                    {
                        continue;
                    }

                    if (Menus.PermissionValue.Count() == 0)
                    {
                        continue;
                    }

                    foreach (var Perms in Menus.PermissionValue)
                    {
                        if (Perms.OrgObjects == null)
                        {
                            continue;
                        }

                        if (Perms.OrgObjects.Count() == 0)
                        {
                            continue;
                        }

                        foreach (var OrgIns in Perms.OrgObjects)
                        {
                            switch (OrgIns.OrgType)
                            {

                                //公司
                                case "0":
                                    if (OrgIns.OrgID == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID)
                                    {
                                        return true;
                                    }
                                    break;
                                case "1"://部门
                                    if (OrgIns.OrgID == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID)
                                    {
                                        return true;
                                    }

                                    break;
                                case "2"://岗位
                                    if (OrgIns.OrgID == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID)
                                    {
                                        return true;
                                    }
                                    break;
                            }
                        }
                    }
                }

            }
            return false;
        }

        /// <summary>
        /// 操作权限的判断
        /// </summary>
        /// <param name="obj">实体名</param>
        /// <param name="StrMenuCode">menucode</param>
        /// <param name="Operation">动作</param>
        /// <param name="UserId">用户ID</param>
        /// <returns></returns>
        public static bool ToolBarButtonOperationPermission(object obj,string StrMenuCode, OperationType Operation, string UserId)
        {
            bool IsReturn = false;
            string menuCode = obj.GetType().Name;
            menuCode = menuCode == StrMenuCode ? menuCode : StrMenuCode;//如果实体名称和传递的参数strmenucode是否一致
            int OperationValue = Convert.ToInt32(Operation);//操作方式
            int PermValue = -1;
            Type objName = obj.GetType();
            if (CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI != null)
            {
                var objs = from o in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                           where o.PermissionValue == Convert.ToInt32(OperationValue).ToString()
                           && o.MenuCode == menuCode
                           select o;
                if (objs != null && objs.Count() > 0)
                {
                    PermValue = objs.Min(p => Convert.ToInt32(p.DataRange));
                }

                PropertyInfo[] infos = objName.GetProperties();
                foreach (PropertyInfo prop in infos)
                {
                    if (prop.GetValue(obj, null) == null)
                    {
                        if (prop.Name == "OWNERCOMPANYID")
                        {
                            //string bb = "";
                        }
                        continue;
                    }
                    switch (PermValue)
                    {
                        case 1:
                            if (prop.Name == "OWNERCOMPANYID")
                            {
                                //公司
                                string companyids = "";
                                if (CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Count > 0)
                                { 
                                    for(int i =0;i< CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Count;i++)
                                    {
                                        companyids += CurrentContext.Common.CurrentLoginUserInfo.UserPosts[i].CompanyID + ",";
                                    }
                                }
                                if (companyids.IndexOf(prop.GetValue(obj, null).ToString()) > -1)
                                {
                                    IsReturn = true;
                                }
                                //if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID)
                                //{
                                //    //return true;
                                //    IsReturn = true;
                                //}
                            }
                            break;
                        case 2:
                            if (prop.Name == "OWNERDEPARTMENTID")
                            {
                                //部门
                                string departmentids = "";
                                if (CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Count > 0)
                                {
                                    for (int i = 0; i < CurrentContext.Common.CurrentLoginUserInfo.UserPosts.Count; i++)
                                    {
                                        departmentids += CurrentContext.Common.CurrentLoginUserInfo.UserPosts[i].DepartmentID + ",";
                                    }
                                }
                                if (departmentids.IndexOf(prop.GetValue(obj, null).ToString()) > -1)
                                {
                                    IsReturn = true;
                                }
                                //if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID)
                                //{
                                //    IsReturn = true;
                                //}
                            }
                            break;
                        case 3:
                            if (prop.Name == "OWNERPOSTID")
                            {
                                //岗位
                                if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID)
                                {
                                    IsReturn = true;
                                }
                            }
                            break;
                        case 4:
                            if (prop.Name == "OWNERID")
                            {
                                //个人
                                
                                if (prop.GetValue(obj, null).ToString() == CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
                                {
                                    IsReturn = true;
                                }
                                
                            }
                            break;
                    }
                }
                return IsReturn;//先不考虑自定义权限的问题

                var Customersobjs1 = from o in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                                    where  o.MenuCode == menuCode
                                    select o;
                var Customersobjs = from o in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI
                           where o.PermissionValue == Convert.ToInt32(OperationValue).ToString()
                           && o.MenuCode == menuCode
                           select o;
                if (Customersobjs.Count() > 0)
                { 

                }
                //自定义权限
                foreach (var objcustomer in CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI)
                {
                    var Menus = objcustomer.CustomerPermission;
                    if (Menus == null) continue;

                    if (Menus.PermissionValue == null)
                    {
                        continue;
                    }

                    if (Menus.PermissionValue.Count() == 0)
                    {
                        continue;
                    }

                    foreach (var Perms in Menus.PermissionValue)
                    {
                        if (Perms.OrgObjects == null)
                        {
                            continue;
                        }

                        if (Perms.OrgObjects.Count() == 0)
                        {
                            continue;
                        }
                        //公司自定义权限
                        var CompanyPerms = from ent in Perms.OrgObjects
                                           where ent.OrgType == "0" &&  CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID.Contains(ent.OrgID)
                                           select ent;
                        if(CompanyPerms.Count() >0)
                            IsReturn = true;
                        //部门自定义权限
                        var DepartmentPerms = from ent in Perms.OrgObjects
                                           where ent.OrgType == "1" && CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID.Contains(ent.OrgID)
                                           select ent;
                        if (DepartmentPerms.Count() > 0)
                            IsReturn = true;
                        //岗位自定义权限
                        var PositionsPerms = from ent in Perms.OrgObjects
                                           where ent.OrgType == "2" && CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID.Contains(ent.OrgID)
                                           select ent;
                        if (PositionsPerms.Count() > 0)
                            IsReturn = true;

                        //foreach (var OrgIns in Perms.OrgObjects)
                        //{
                        //    switch (OrgIns.OrgType)
                        //    {

                        //        //公司
                        //        case "0":
                        //            if (OrgIns.OrgID == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID)
                        //            {
                        //                return true;
                        //            }
                        //            break;
                        //        case "1"://部门
                        //            if (OrgIns.OrgID == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID)
                        //            {
                        //                return true;
                        //            }

                        //            break;
                        //        case "2"://岗位
                        //            if (OrgIns.OrgID == CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID)
                        //            {
                        //                return true;
                        //            }
                        //            break;
                        //    }
                        //}
                    }
                }

            }
            //return false;
            return IsReturn;
        }
        #region 龙康才新增,通过事项类型ID,找到流程路径

        static Dictionary<string, string> lineList = new Dictionary<string, string>();

        /// <summary>
        /// 通过事项类型获取流程路径
        /// </summary>
        /// <param name="layoutXml"></param>
        /// <param name="typeNumber">事项类型值数字</param>
        /// <param name="typeDictionary">事项类型值字典</param>
        /// <returns></returns>
        public static string GetFlowPathByTypeApprovalValue(string layoutXml, string typeNumber, Dictionary<string, string> typeDictionary)
        {
            lineList.Clear();//先清空
            if (lineList.Count < 1)
            {
                GetFlowTypeApproval(layoutXml);
            }
            #region
            #region 事项类型名称 代替 事项类型 数字
            Dictionary<string, string> newList = new Dictionary<string, string>();//把代替后的重新加入
            //开始--[一级事项类型>=461]-->财务经理-在线---->副总经理---->助理总裁---->副总裁（刘小强）---->总裁---->结束；
            foreach (KeyValuePair<string, string> line in lineList)
            {
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("\\[(.*?)\\]", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                System.Text.RegularExpressions.MatchCollection matchs = reg.Matches(line.Value);
                if (matchs.Count > 0)
                {
                    #region 有事项类型
                    for (int i = 0; i < matchs.Count; i++)
                    {
                        string charStr = "";
                        #region 比较字符
                        if (matchs[i].Groups[1].Value.IndexOf(">") > 0)
                        {
                            charStr = ">";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf("<") > 0)
                        {
                            charStr = "<";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf("==") > 0)
                        {
                            charStr = "==";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf(">=") > 0)
                        {
                            charStr = ">=";
                        }
                        if (matchs[i].Groups[1].Value.IndexOf("<=") > 0)
                        {
                            charStr = "<=";
                        }

                        #endregion
                        if (charStr != "")
                        {
                            //matchs[i].Groups[1].Value  =  一级事项类型==39
                            string name = System.Text.RegularExpressions.Regex.Split(matchs[i].Groups[1].Value, charStr)[1];
                            string typeName = name;
                            if (typeDictionary.ContainsKey(typeName))
                            {
                                typeName = typeDictionary[typeName];//事项类型名称
                            }
                            string typeAllName = matchs[i].Groups[1].Value.Replace(name, typeName);// 一级事项类型==集团合同
                            if (!newList.ContainsKey(line.Key))
                            {
                                newList.Add(line.Key, line.Value.Replace(matchs[i].Groups[1].Value, typeAllName));
                            }
                            else
                            {
                                newList[line.Key] = newList[line.Key].Replace(matchs[i].Groups[1].Value, typeAllName);
                            }
                        }
                        else
                        {
                            newList.Add(line.Key, line.Value);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region 没有事项类型
                    if (!newList.ContainsKey(line.Key))
                    {
                        newList.Add(line.Key, line.Value);
                    }
                    else
                    {
                        newList[line.Key] = newList[line.Key];
                    }
                    #endregion
                }
            }
            #endregion
            #endregion 获取
            if (newList.Count > 0)
            {
                if (newList.ContainsKey("错误分支"))
                {
                    MessageBox.Show("发现可能进入死循环的错误分支:" + newList["错误分支"].ToString());
                }
                if (newList.ContainsKey(typeNumber))
                {
                    return newList[typeNumber].ToString();
                }
                else
                {
                    return newList["Default"].ToString();
                }
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 通过layoutXml查找所有路径,但如果发现有死循环的错误分支即终止返回,之前查找到的分支
        /// </summary>
        /// <param name="layoutXml"></param>
        /// <returns></returns>
        private static List<string> GetFlowTypeApproval(string layoutXml)
        {
            try
            {
                List<string> list = new List<string>();
                Byte[] b = System.Text.UTF8Encoding.UTF8.GetBytes(layoutXml);
                XElement xElement = XElement.Load(System.Xml.XmlReader.Create(new MemoryStream(b)));
                var lines = from item in xElement.Descendants("Rule")
                            where item.Attribute("StrStartActive").Value == "StartFlow"
                            select item;
                foreach (var line in lines)
                {
                    string conditions = "", conditionsValue = "", Operate = "";
                    if (line.Element("Conditions") != null)
                    {
                        conditions = "[" + line.Element("Conditions").Element("Condition").Attribute("Description").Value + "";
                        Operate = "" + line.Element("Conditions").Element("Condition").Attribute("Operate").Value + "";
                        conditionsValue = "" + line.Element("Conditions").Element("Condition").Attribute("CompareValue").Value + "]";
                    }
                    var Element = (from item in xElement.Descendants("Activity")
                                   where item.Attribute("Name").Value == line.Attribute("StrEndActive").Value
                                   select item).FirstOrDefault();
                    string path = "开始--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value;
                    list = GetActivityPath(xElement, Element.Attribute(XName.Get("Name")).Value, path, list);
                    //if (list[0].ToString() == "开始-->流程设计格式不正确")
                    //{
                    //    return list;
                    //}
                }
                #region 获取事项类型
                foreach (var li in list)
                {
                    if (li.Contains("事项类型") || li.Contains("审批类型"))
                    {
                        #region 事项类型, 审批类型
                        try
                        {
                            //开始--[一级事项类型==127]-->分支机构财务负责人5---->分支机构负责人5---->商务部副经理---->营销总监---->营运中心办公室助理总监---->财务中心办公室总监---->副总经理5---->法务主管---->总经理5---->结束；
                            //开始---->直接上级---->部门负责人--[事项审批类型==35]-->财务经理-在线---->律师---->副总经理--[一级事项类型==42]-->人力资源部负责人---->副总裁（刘小强）---->总裁---->结束；"
                            //开始--[一级事项类型>=461]-->财务经理-在线---->副总经理---->助理总裁---->副总裁（刘小强）---->总裁---->结束；

                            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("\\[(.*?)\\]", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            System.Text.RegularExpressions.MatchCollection matchs = reg.Matches(li);
                            for (int i = 0; i < matchs.Count; ++i)
                            {
                                string charStr = "";
                                #region 比较字符
                                if (matchs[i].Groups[1].Value.IndexOf(">") > 0)
                                {
                                    charStr = ">";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("<") > 0)
                                {
                                    charStr = "<";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("==") > 0)
                                {
                                    charStr = "==";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf(">=") > 0)
                                {
                                    charStr = ">=";
                                }
                                if (matchs[i].Groups[1].Value.IndexOf("<=") > 0)
                                {
                                    charStr = "<=";
                                }

                                #endregion
                                if (charStr != "")
                                {
                                    string typeValue = System.Text.RegularExpressions.Regex.Split(matchs[i].Groups[1].Value, charStr)[1];
                                    if (lineList.ContainsKey(typeValue))
                                    {
                                        lineList[typeValue] = lineList[typeValue] + "\r\n" + li;
                                    }
                                    else
                                    {
                                        lineList.Add(typeValue, li);
                                    }
                                }
                            }
                        }
                        catch
                        { }
                        #endregion
                    }
                    else
                    {
                        if (lineList.ContainsKey("Default"))
                        {
                            lineList["Default"] = lineList["Default"] + "\r\n" + li;
                        }
                        else
                        {
                            lineList.Add("Default", li);
                        }
                    }
                }
                #endregion
                return list;
            }
            catch (Exception ex)
            {
                MessageBox.Show("流条件设置有误:" + ex.Message);
                return null;
            }
        }

        private static List<string> GetActivityPath(XElement xElement, string ActivityID, string path, List<string> list)
        {
            string snap = path;//foreach中循环的变量
            var lines = from item in xElement.Descendants("Rule")
                        where item.Attribute("StrStartActive").Value == ActivityID
                        select item;
            foreach (var line in lines)
            {
                string conditions = "", conditionsValue = "", Operate = "";
                if (line.Element("Conditions") != null)
                {
                    conditions = "[" + line.Element("Conditions").Element("Condition").Attribute("Description").Value + "";
                    Operate = "" + line.Element("Conditions").Element("Condition").Attribute("Operate").Value + "";
                    conditionsValue = "" + line.Element("Conditions").Element("Condition").Attribute("CompareValue").Value + "]";
                }
                var Element = (from item in xElement.Descendants("Activity")
                               where item.Attribute("Name").Value == line.Attribute("StrEndActive").Value
                               select item).FirstOrDefault();
                if (line.Attribute("StrEndActive").Value == "EndFlow")
                {
                    if (list.Count() > 0 && list.Count() == 2 && list[0].ToString() == "开始-->流程设计格式不正确")
                    {
                        return list;
                    }
                    else
                    {
                        list.Add(snap + "---->结束；");
                    }
                    continue;
                }
                else
                {

                    if (snap.IndexOf("-->" + Element.Attribute(XName.Get("Remark")).Value + "--") < 1)
                    {
                        if (lines.Count() > 1)
                        {
                            path = snap + "--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value;
                        }
                        else
                        {

                            path += "--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value;
                        }
                        GetActivityPath(xElement, Element.Attribute(XName.Get("Name")).Value, path, list);
                    }
                    else
                    {
                        #region 获取事项类型
                        //foreach (var li in list)
                        //{
                        //    if (li.Split('>')[0].Contains("事项类型"))
                        //    {
                        //        try
                        //        {
                        //            string name = li.Replace(")-->", "|").Split('|')[0].Replace("==", "|").Split('|')[1];
                        //            if (lineList.ContainsKey(name))
                        //            {
                        //                lineList[name] = lineList[name] + "\r\n" + li;
                        //            }
                        //            else
                        //            {
                        //                lineList.Add(name, li);
                        //            }
                        //        }
                        //        catch
                        //        { }
                        //    }

                        //}
                        #endregion
                        //  list.Clear();
                        //  list.Add("开始-->流程设计格式不正确");
                        list.Add("错误分支：" + snap + "--" + conditions + Operate + conditionsValue + "-->" + Element.Attribute(XName.Get("Remark")).Value + "");
                        return list;
                    }
                }
            }
            return list;
        }

        #endregion
        #region 查询流程审核顺序

        #region 从XML中获取值
        private string GetValue(string xml, string system, string Object, string node)
        {
            try
            {
                if (xml == null || xml == "")
                    return null;

                XmlReader XmlReader;
                StringReader bb = new StringReader(xml);
                XmlReader = XmlReader.Create(bb);

                XElement xEle = XElement.Load(XmlReader);
                var DataVale = from c in xEle.Descendants("Attribute")
                               where c.Attribute("Name").Value == node
                               select c.Attribute("DataValue").Value;


                #region 匹配系统，对象查询值 --取消匹配系统和对象，直接取值
              

                #endregion

                return DataVale.ToList().Count > 0 ? DataVale.ToList().FirstOrDefault().ToString() : null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GetString(string xml, string system, string Object, string node)
        {

            return GetValue(xml, system, Object, node);
        }

        private decimal? GetDecimal(string xml, string system, string Object, string node)
        {
            decimal? tmpDecimal;
            string tmpData = GetValue(xml, system, Object, node);
            if (tmpData == "")
                tmpData = "0";
            tmpDecimal = null;
            return tmpData == null ? tmpDecimal : Convert.ToDecimal(tmpData);
        }

        private DateTime GetDate(string xml, string system, string Object, string node)
        {

            return Convert.ToDateTime(GetValue(xml, system, Object, node));
        }


        #endregion

        #region 通过状态代码查询状态说明

        private string GetFlowRemark(XDocument doc, string Parm)
        {
            try
            {


                if (Parm == "StartFlow")
                    return "开始";
                if (Parm == "EndFlow")
                    return "结束";
                var Remark = from c in doc.Root.Elements("Activitys").Elements("Activity")
                             where c.Attribute("Name").Value == Parm
                             select c.Attribute("Remark").Value;
                return Remark.First().ToString();
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        #endregion
       
        #region 获取流程列表
        public static Dictionary<int, string> GetFlowListResult(string Layout, string XML)
        {
            try
            {
                XmlReader XmlReader;
                StringReader bb = new StringReader(Layout);
                XmlReader = XmlReader.Create(bb);

                XDocument custs = XDocument.Load(XmlReader);
           

                Utility FlowOpt = new Utility();
                Dictionary<int, string> Dic = FlowOpt.GetFlowList(0, custs, "StartFlow", "EndFlow", XML);
                if (Dic != null && Dic.Count > 0)
                    return Dic.OrderBy(c => c.Key).ToDictionary(k => k.Key, v => v.Value);
                return null;

            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        private Dictionary<int, string> GetFlowList(int OrderNo, XDocument doc, string Start, string End, string XML)
        {
            try
            {


                if (Start == "EndFlow")
                {
                    Dictionary<int, string> Dic = new Dictionary<int, string>();
                    Dic.Add(OrderNo, GetFlowRemark(doc, Start));
                    return Dic;
                }
                var rules = from c in doc.Root.Elements("Rules").Elements("Rule")
                            where c.Attribute("StrStartActive").Value == Start
                            select c;
                string tmp = "";



                if (rules.Count() <= 0)
                {
                    return null;
                }
                else if (rules.Count() == 1)
                {
                    tmp = rules.ElementAt(0).Attribute("StrEndActive").Value;
                }
                else
                {
                    // FlowData FlowDataOpt = new FlowData();
                    bool isComValue = true;
                    foreach (var item in rules)
                    {

                        if ((item.Element("Conditions") != null))
                        {
                            //todo 处理有条件时的比较

                            if (item.Element("Conditions").Elements("Condition").Count() > 0)
                            {
                                var e = from f in item.Element("Conditions").Descendants("Condition")
                                        select f;

                                foreach (var tmp2 in e)
                                {
                                    switch (tmp2.Attribute("Operate").Value)
                                    {
                                        case "==":
                                            {
                                                if (tmp2.Attribute("CompareValue") != null)
                                                {
                                                    if (tmp2.Attribute("CompareValue").Value != GetString(XML, "", "", tmp2.Attribute("CompAttr").Value))
                                                        isComValue = false;
                                                    else
                                                        isComValue = true;
                                                }
                                                break;
                                            }
                                        case ">":
                                            {
                                                if (Convert.ToDecimal(tmp2.Attribute("CompareValue").Value) >= GetDecimal(XML, "", "", tmp2.Attribute("CompAttr").Value))
                                                    isComValue = false;
                                                else
                                                    isComValue = true;
                                                break;
                                            }
                                        case ">=":
                                            {
                                                if (Convert.ToDecimal(tmp2.Attribute("CompareValue").Value) > GetDecimal(XML, "", "", tmp2.Attribute("CompAttr").Value))
                                                    isComValue = false;
                                                else
                                                    isComValue = true;
                                                break;
                                            }
                                        case "<":
                                            {
                                                if (Convert.ToDecimal(tmp2.Attribute("CompareValue").Value) <= GetDecimal(XML, "", "", tmp2.Attribute("CompAttr").Value))
                                                    isComValue = false;
                                                else
                                                    isComValue = true;
                                                break;
                                            }
                                        case "<=":
                                            {
                                                if (Convert.ToDecimal(tmp2.Attribute("CompareValue").Value) < GetDecimal(XML, "", "", tmp2.Attribute("CompAttr").Value))
                                                    isComValue = false;
                                                else
                                                    isComValue = true;
                                                break;
                                            }
                                        case "<>":
                                            {
                                                if (tmp2.Attribute("CompareValue").Value == GetString(XML, "", "", tmp2.Attribute("CompAttr").Value))
                                                    isComValue = false;
                                                else
                                                    isComValue = true;
                                                break;

                                            }
                                        default:
                                            {

                                                break;
                                            }
                                    }
                                }

                                if (isComValue) //符合条件下一状态值，并退了循环
                                {
                                    tmp = item.Attribute("StrEndActive").Value;
                                    break;
                                }
                            }

                        }
                        else
                            tmp = item.Attribute("StrEndActive").Value; //默认下一状态值
                    }


                }
                //Dictionary<string, int> Dic = new Dictionary<string, int>();
                //Dic.Add(tmp,0);
                Dictionary<int, string> tmpDic = GetFlowList(OrderNo + 1, doc, tmp, End, XML);
                if (tmpDic != null)
                {
                    tmpDic.Add(OrderNo, GetFlowRemark(doc, Start));
                }

                return tmpDic;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        #endregion
        #endregion

        /// <summary>
        /// 返回字符串信息，如果为空或null则为“”
        /// </summary>
        /// <param name="StrValue"></param>
        /// <returns></returns>
        public static string GetEntityFiledValue(string StrValue)
        {
            if (string.IsNullOrEmpty(StrValue))
            {
                return "";
            }
            return StrValue;
        }
        /// <summary>
        /// 预算调用OA的单据信息 ljx2011-3-16
        /// </summary>
        /// <param name="MenuCode"></param>
        /// <param name="FormId"></param>
        public static void ShowOAFormByFB(string MenuCode, string FormId)
        {
            if (string.IsNullOrEmpty(MenuCode) || string.IsNullOrEmpty(FormId))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "参数传入错误，不能为空",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            string FormName = "";
            switch (MenuCode)
            {
                case "OAPERSONAPPROVAL"://报批件
                    FormName = "SMT.SaaS.OA.UI.UserControls.ApprovalForm_aud";
                    break;
                case "T_OA_TRAVELREIMBURSEMENT"://出差报销
                    FormName = "SMT.SaaS.OA.UI.UserControls.TravelReimbursementControl";
                    break;
                case "T_OA_BUSINESSTRIP"://出差申请
                    FormName = "SMT.SaaS.OA.UI.Views.Travelmanagement.TravelapplicationChildWindows";
                    break;
                case "T_OA_BUSINESSREPORT"://出差报告
                    FormName = "SMT.SaaS.OA.UI.Views.Travelmanagement.MissionReportsChildWindows";
                    break;
            }

            FormTypes CurrentAction = FormTypes.Browse;
            

            Type t = Type.GetType(FormName);

            Object[] parameters = new Object[2];    // 定义构造函数需要的参数
            parameters[0] = CurrentAction;
            parameters[1] = FormId;// 

            object form = Activator.CreateInstance(t, parameters);
            if (form != null)
            {
                EntityBrowser entBrowser = new EntityBrowser(form);
                entBrowser.FormType = CurrentAction;
                if (FormName == "SMT.SaaS.OA.UI.Views.Travelmanagement.MissionReportsChildWindows")
                {
                    entBrowser.ParentWindow.Height = 900;
                    entBrowser.ParentWindow.Width = 1145;
                }
                else if (FormName == "SMT.SaaS.OA.UI.Views.Travelmanagement.TravelapplicationChildWindows")
                {
                    entBrowser.ParentWindow.Height = 900;
                    entBrowser.ParentWindow.Width = 1060;
                }
                else if (FormName == "SMT.SaaS.OA.UI.UserControls.TravelReimbursementControl")
                {
                    entBrowser.ParentWindow.Height = 900;
                    entBrowser.ParentWindow.Width = 1180;
                }
                else
                {
                    entBrowser.ParentWindow.Height = 900;
                    entBrowser.ParentWindow.Width = 900;
                }
                entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

                
            }

            
            
        }
        /// <summary>
        /// 隐藏grid中的某一行
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="IntIndex"></param>
        public static void HiddenGridRow(Grid dg, int IntIndex)
        {
            dg.RowDefinitions[IntIndex].Height = new GridLength(0);
        }



        #region 把令牌添加到消息头中

        /// <summary>
        /// 把令牌添加到消息头中
        /// </summary>
        /// <param name="cs">WCF通道</param>
        /// <param name="LoginToken">令牌</param>
        public static void DoChannel(IClientChannel cs, string LoginToken)
        {
            OperationContextScope scope = new OperationContextScope(cs);
            var myNamespace = "http://portal.smt-online.net";

            // 注意Header的名字中不能出现空格，因为要作为Xml节点名。  

            var Token = MessageHeader.CreateHeader("Token", myNamespace, LoginToken);

            OperationContext.Current.OutgoingMessageHeaders.Add(Token);

        }

        #endregion

        public static void CbxItemBinder(ComboBox cbx, string category, string defalutValue)
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var ents = dicts.Where(s => s.DICTIONCATEGORY == category).OrderBy(s => s.DICTIONARYVALUE);
            List<T_SYS_DICTIONARY> tempDicts = ents.ToList();

            if (ents == null)
            {
                return;
            }

            if (ents.Count() == 0)
            {
                return;
            }

            T_SYS_DICTIONARY nuldict = new T_SYS_DICTIONARY();
            nuldict.DICTIONARYNAME = Utility.GetResourceStr("ALL");
            nuldict.DICTIONARYVALUE = 5;
            tempDicts.Insert(0, nuldict);

            cbx.ItemsSource = tempDicts;
            cbx.DisplayMemberPath = "DICTIONARYNAME";
            if (defalutValue != "")
            {
                foreach (var item in cbx.Items)
                {
                    T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                    if (dict != null)
                    {
                        if (dict.DICTIONARYVALUE.ToString() == defalutValue)
                        {
                            cbx.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        public static void ShowMessageBox(string actionString, bool isFlowFlag, bool isSuccess)
        {
            string successString = "FAILED";
            if (isSuccess)
            {
                successString = "SUCCESSED";
            }
            if (isFlowFlag)
            {
                actionString = "SUBMIT";
            }
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr(successString), Utility.GetResourceStr(actionString + successString, ""));
        }

        #region  业务对象操作

        object BusinessObject;
        public void GetBusinessObject(object sender, string SystemCode, string BusinessObjectName)
        {
            BusinessObject = sender;
            SMT.Saas.Tools.PublicInterfaceWS.PublicServiceClient PublicInterface = new SMT.Saas.Tools.PublicInterfaceWS.PublicServiceClient();
            PublicInterface.GetBusinessObjectAsync(SystemCode, BusinessObjectName);
            PublicInterface.GetBusinessObjectCompleted += new EventHandler<Saas.Tools.PublicInterfaceWS.GetBusinessObjectCompletedEventArgs>(PublicInterface_GetBusinessObjectCompleted);  
        }

         void PublicInterface_GetBusinessObjectCompleted(object sender, Saas.Tools.PublicInterfaceWS.GetBusinessObjectCompletedEventArgs e)
        {
            var cc = e.Result;
            if (BusinessObject is IBusinessObject && BusinessObject != null)
            {
                ((IBusinessObject)BusinessObject).GetBusinessObjectCompleted(e);
            }
           
          
            //throw new NotImplementedException();
        }
        #endregion
    }

}
