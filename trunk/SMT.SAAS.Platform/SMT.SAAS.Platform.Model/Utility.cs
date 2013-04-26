using System;
using System.Reflection;
using System.Linq;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 工具类
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform
{
    public static class Utility
    {
        /// <summary>
        /// 根据跟定的两个已知对象TSource，TTarget
        /// 将TSource对象中与TTarget
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="Source"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        public static TTarget CloneObject<TSource, TTarget>(TSource Source, TTarget Target)
        {
            try
            {
                if (Source == null)
                    throw new ArgumentException("Source");
                if (Target == null)
                    throw new ArgumentException("Target");

                PropertyInfo[] sourcePropertys = Source.GetType().GetProperties();
                PropertyInfo[] targetPropertys = Target.GetType().GetProperties();
                foreach (PropertyInfo sourcePro in sourcePropertys)
                {
                    var targetPro = targetPropertys.Where<PropertyInfo>(delegate(PropertyInfo proInfo)
                    {
                        return proInfo.Name.ToLower() == sourcePro.Name.ToLower() &&
                            proInfo.PropertyType == sourcePro.PropertyType;
                    }).FirstOrDefault<PropertyInfo>();
                    if (targetPro != null)
                        targetPro.SetValue(Target, sourcePro.GetValue(Source, null), null);
                }
                return Target;
            }
            catch (Exception ex)
            {
                throw new Exception("对象克隆异常.", ex);
            }
        }

        /// <summary>
        /// 根据跟定的两个已知对象TSource，TTarget
        /// 将TSource对象中的数据克隆到TTarget中。
        /// TSource与TTarget可以为不同类型，其仅会拷贝具有相同名称和数据类型的属性值。
        /// </summary>
        /// <typeparam name="TSource">数据源类型</typeparam>
        /// <typeparam name="TTarget">数据目标类型</typeparam>
        /// <param name="Source">数据源</param>
        /// <param name="Target">数据目标</param>
        /// <returns>已包含赋值数据的结果</returns>
        public static TTarget CloneObject<TTarget>(this object Source, TTarget Target)
        {
            try
            {
                if (Source == null)
                    throw new ArgumentNullException("Source");
                if (Target == null)
                    throw new ArgumentNullException("Target");

                PropertyInfo[] sourcePropertys = Source.GetType().GetProperties();
                PropertyInfo[] targetPropertys = Target.GetType().GetProperties();
                foreach (PropertyInfo sourcePro in sourcePropertys)
                {
                    var targetPro = targetPropertys.Where<PropertyInfo>(delegate(PropertyInfo proInfo)
                    {
                        return proInfo.Name.ToLower() == sourcePro.Name.ToLower() &&
                            proInfo.PropertyType == sourcePro.PropertyType;
                    }).FirstOrDefault<PropertyInfo>();
                    if (targetPro != null)
                        targetPro.SetValue(Target, sourcePro.GetValue(Source, null), null);
                }
                return Target;
            }
            catch (Exception ex)
            {
                throw new Exception("对象克隆异常", ex);
            }
        }

        /// <summary>
        /// 根据给定的字符串对其进行加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
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

        public static Model.UserLogin UserLogin = new Model.UserLogin(); 
    }

}
