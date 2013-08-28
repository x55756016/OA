/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©   2011    
	 * 文件名：Log.cs  
	 * 创建者： 亢晓方
	 * 创建日期：2011/12/19 16:54:30   
	 * CLR版本： 4.0.30319.239  
	 * 命名空间：SMT.Workflow.Engine.Services.DAL 
	 * 模块名称：
	 * 描　　述： 	 
* ---------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace SMT.Workflow.Engine.Services.DAL
{
    public class Log
    {
        #region 日志
        //private static string currentDir = Environment.CurrentDirectory;
        //private static string hostDir = System.AppDomain.CurrentDomain.BaseDirectory + "Log\\";
        private static string hostDir = ConfigurationManager.AppSettings["LogPath"].ToString();

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
        /// 创建文件(如果不存在就创建)
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void CreateFile(string filePath)
        {
            int index = filePath.LastIndexOf("\\");
            string dir = filePath.Substring(0, index);
            string dirPath = dir;
            if (!File.Exists(filePath))
            {
                CreateDirectory(dirPath);
                File.Create(filePath);
            }
        }

        #region 写日志
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="content">内容</param>
        /// <returns></returns>
        public static bool WriteLog(string content)
        {

            string dirPath = hostDir;
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
                string fileSavePath = dirPath + "/Engine" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
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
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="obj">当前发生的对象（如：类对象this）</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="content">内容</param>
        /// <param name="e">Exception</param>
        /// <returns></returns>
        public static bool WriteLog(object obj, string methodName, string content, Exception e)
        {

            string dirPath = hostDir;
            if (!Directory.Exists(dirPath))
            {
                CreateDirectory(dirPath);

            }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("================================================================================");
                sb.AppendLine("时间:" + DateTime.Now.ToString());
                sb.AppendLine("DLL:" + obj.GetType().Module.Name);
                sb.AppendLine("类名:" + obj.GetType().FullName);
                sb.AppendLine("方法:" + methodName);
                sb.AppendLine("内容:" + content);
                if (e != null)
                {
                    sb.AppendLine("异常:" + (e.InnerException != null ? e.InnerException.Message : e.Message));
                }
                string fileSavePath = dirPath + "/Engine" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
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
        #endregion
        #endregion
    }
}
