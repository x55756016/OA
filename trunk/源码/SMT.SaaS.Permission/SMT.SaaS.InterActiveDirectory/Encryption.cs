using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InterActiveDirectory
{
	/// <summary>
	/// Encryption 的摘要说明。
	/// </summary>
	public class Encryption
	{
		private static byte[] DES_IV = new byte[]{0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF};
		private static byte[] DES_KEY = new byte[]{0x12, 0x34, 0x56, 0x78, 0x87, 0x65, 0x43, 0x21};

		/// <summary>
		/// 加密
		/// </summary>
		/// <param name="strText">待加密内容</param>
		/// <returns>加密后内容</returns>
		public static string EncryptData(string strText)
		{
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();
			byte[] inputByteArray = Encoding.ASCII.GetBytes(strText);
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(DES_KEY, DES_IV), CryptoStreamMode.Write);
			cs.Write(inputByteArray, 0, inputByteArray.Length);
			cs.FlushFinalBlock();
			return Convert.ToBase64String(ms.ToArray());	
		}

		/// <summary>
		/// 解密
		/// </summary>
		/// <param name="strText">待解密内容</param>
		/// <returns>解密后内容</returns>
		public static string DecryptData(string strText)
		{
			byte[] inputByteArray = new Byte[strText.Length];
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();
			inputByteArray = Convert.FromBase64String(strText);
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(DES_KEY, DES_IV), CryptoStreamMode.Write);
			cs.Write(inputByteArray, 0, inputByteArray.Length);
			cs.FlushFinalBlock();
			return Encoding.ASCII.GetString(ms.ToArray());
		}

		/// <summary>
		/// 加密
		/// </summary>
		/// <param name="strText">待加密内容</param>
		/// <returns>加密后内容</returns>
		public static string DesEncrypt(string oStr) 
		{
			string nStr="";
			byte[] KEY_64 = new byte[8] {2,3,4,5,6,7,8,9};
			byte[] IV_64 = new byte[8] {12,13,14,15,16,17,1,19};
		
			if(oStr.Length>0)
			{
				DESCryptoServiceProvider cryptoProvider=new DESCryptoServiceProvider();
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms,cryptoProvider.CreateEncryptor(KEY_64,IV_64),CryptoStreamMode.Write);
				byte[] d = System.Text.Encoding.Unicode.GetBytes(oStr);
				cs.Write(d, 0, d.Length);
				cs.FlushFinalBlock();
				byte[] buffer = ms.ToArray();
				nStr=Convert.ToBase64String(buffer,0,(int) ms.Length);   
				cs.Close();
				ms.Close();
			}
			return nStr;
		}

		/// <summary>
		/// 解密
		/// </summary>
		/// <param name="strText">待解密内容</param>
		/// <returns>解密后内容</returns>
		public static  string DesDecrypt(string oStr) 
		{
			string nStr="";
			byte[] KEY_64 = new byte[8] {2,3,4,5,6,7,8,9};
			byte[] IV_64 = new byte[8] {12,13,14,15,16,17,1,19};
			if(oStr.Length>0)
			{
				try
				{
					DESCryptoServiceProvider cryptoProvider=new DESCryptoServiceProvider();
					byte[] buffer=Convert.FromBase64String(oStr); 
					MemoryStream ms = new MemoryStream();
					CryptoStream cs = new CryptoStream(ms,cryptoProvider.CreateDecryptor(KEY_64, IV_64),CryptoStreamMode.Write); 
					cs.Write(buffer, 0, buffer.Length);
					cs.FlushFinalBlock();
					buffer = ms.ToArray();
					nStr = System.Text.Encoding.Unicode.GetString(buffer);
					cs.Close();
					ms.Close();
				}
				catch{nStr=oStr;}
			}
			return nStr;
		}
	}
}
