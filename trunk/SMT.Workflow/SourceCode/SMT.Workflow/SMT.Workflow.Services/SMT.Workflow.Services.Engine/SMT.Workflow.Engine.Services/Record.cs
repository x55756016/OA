/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Record.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/23 15:40:02   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Configuration;

namespace SMT.Workflow.Engine.Services
{
    public static class Record
    {
        private static string dirPath = ConfigurationManager.AppSettings["LogPath"].ToString();
        /// <summary>
        /// 创建目录(如果不存在就创建)
        /// </summary>
        /// <param name="dirPath">目录路径</param>
        public static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        } 
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public static bool WriteLogFunction(string content)
        {
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("================================================================================");
                sb.AppendLine("发生时间：" + DateTime.Now.ToString());
                sb.AppendLine("日志内容:" + content);
                string fileSavePath = dirPath + "/ServiceTest" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                using (FileStream aFile = new FileStream(fileSavePath, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(aFile, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}