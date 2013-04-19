/*
 * 文件名：EncryptionBLL.cs
 * 作  用：加密算法
 * 创建人： 喻建华
 * 创建时间：2010年9月20日, 15:37:12
 * 修改人：
 * 修改时间：
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;

namespace SMT.HRM.BLL
{
    class EncryptionBLL
    {
        
    }

    public class DES
    {
        /// <summary>
        /// 进行加密，注意：这里的密钥Key必须能转为8个byte，即输入密钥为8半角个字符或者4个全角字符或者4个汉字的字符串
        /// </summary>
        /// <param name="originalData">被加密的原始数据</param>
        /// <returns>返回字符串类型结果</returns>
        public static string DESEncrypt(string originalData)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(originalData);
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                //以下两句可以使输入密钥为中文或英文文本
                des.Key = System.Text.Encoding.Default.GetBytes(GetKeys());
                des.IV = System.Text.Encoding.Default.GetBytes(GetKeys());
                ICryptoTransform desencrypt = des.CreateEncryptor();
                byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
                return BitConverter.ToString(result);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                ex.Message.ToString();
                return null;
            }

        }

        /// <summary>
        /// 进行解密，注意：这里的密钥Key必须能转为8个byte，即输入密钥为8半角个字符或者4个全角字符或者4个汉字的字符串
        /// </summary>
        /// <param name="hashedData">被解密的数据</param>
        /// <returns>返回字符串类型结果</returns>
        public static string DESDecrypt(string hashedData)
        {
            try
            {
                string[] sInput = hashedData.Split("-".ToCharArray());
                byte[] data = new byte[sInput.Length];
                for (int i = 0; i < sInput.Length; i++)
                {
                    data[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
                }
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                //以下两句可以使输入密钥为中文或英文文本
                des.Key = System.Text.Encoding.Default.GetBytes(GetKeys());
                des.IV = System.Text.Encoding.Default.GetBytes(GetKeys());
                ICryptoTransform desencrypt = des.CreateDecryptor();
                byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
                return Encoding.UTF8.GetString(result);
            }
            catch (Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
                ex.Message.ToString();
                return null;
            }

        }

        /// <summary>
        /// 待定方法    //可能从数据库里获取密钥结果
        /// 获取密钥Key必须能转为8个byte，即输入密钥为8半角个字符或者4个全角字符或者4个汉字的字符串  
        /// </summary>
        /// <returns>返回字符串类型结果</returns>
        public static string GetKeys()
        {
            return "methodke";
        }
    }

    public class AES
    {
        //密钥向量    
        private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

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
                byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组 
                //设置密钥及密钥向量
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
            }catch(Exception ex)
            {
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
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
                SMT.Foundation.Log.Tracer.Debug(ex.Message);
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

        /// <summary>
        /// AES算法加密数据(不变密)
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

    }
}
