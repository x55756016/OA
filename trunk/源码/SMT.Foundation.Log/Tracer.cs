using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
namespace SMT.Foundation.Log
{
    public class Tracer
    {
        /// <summary>
        /// 将对象序列化保存至文件中
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="FileName">如果文件名为空则为保存的对象的类型名</param>
        public static void Serializer(object obj,string FileName)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                FileName = obj.GetType().Name;
            }
            XmlSerializer ser = new XmlSerializer(obj.GetType());

            string logpath = LogConfig.Instance.ErrorLogPath;
            string filepath = string.Empty;
            if (string.IsNullOrEmpty(logpath))
            {
                filepath = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                filepath = logpath;
            }
            filepath += @"/ObjectXML/"+DateTime.Now.ToString("yyyy-MM-dd")+"/";
            if (!Directory.Exists(filepath))    //如果文件夹不存在
            {
                DirectoryInfo dirinfo = System.IO.Directory.CreateDirectory(filepath);//创建文件夹
            }
            filepath += DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + FileName + ".XML";
            FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None, 2048, true);                
            try
            {
               ser.Serialize(fs, obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
            //byte[] fileContent = new byte[fs.Length];
            //fs.Write(fileContent, 0, (int)fs.Length);
            //try
            //{
            //    AutoResetEvent manualEvent = new AutoResetEvent(false);
            //    IAsyncResult asyncResult = fs.BeginWrite(fileContent, 0, fileContent.Length,
            //                                            new AsyncCallback(EndWriteCallback),
            //                                            new WriteState(fs, manualEvent));
            //    manualEvent.WaitOne(3000, false);
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    fs.Close();
            //}
                        
        }

        // 异步写
        private static void EndWriteCallback(IAsyncResult asyncResult)
        {
            WriteState stateInfo = (WriteState)asyncResult.AsyncState;
            int workerThreads;
            int portThreads;
            try
            {
                ThreadPool.GetAvailableThreads(out workerThreads, out portThreads);
                stateInfo.fStream.EndWrite(asyncResult);
            }
            finally
            {
                stateInfo.autoEvent.Set();
            }
        }


        static AutoResetEvent myWriteResetEvent = new AutoResetEvent(false);

        public static void Debug(string message)
        {
            try
            {
                // traceLevel.Sort();
                if (checkTracer("Debug"))
                {
                    TXTLogger txtLogger = new TXTLogger();
                    txtLogger.Write(message, level.Debug, myWriteResetEvent);
                }
            }
            catch
            {

            }
        }
        public static void Info(string message)
        {
            try
            {
                //   traceLevel.Sort();
                if (checkTracer("Info"))
                {
                    TXTLogger txtLogger = new TXTLogger();
                    txtLogger.Write(message, level.Info, myWriteResetEvent);
                }
            }
            catch
            {
            }
        }
        public static void Warn(string message)
        {
            try
            {
                //    traceLevel.Sort();
                if (checkTracer("Warn"))
                {
                    TXTLogger txtLogger = new TXTLogger();
                    txtLogger.Write(message, level.Warn, myWriteResetEvent);
                }
            }
            catch
            {
            }
        }
        public static void Error(string message)
        {
            try
            {
                //    traceLevel.Sort();
                if (checkTracer("Error"))
                {
                    TXTLogger txtLogger = new TXTLogger();
                    txtLogger.Write(message, level.Error, myWriteResetEvent);
                }
            }
            catch
            {
            }
        }
        public static void Fatal(string message)
        {
            try
            {
                //  traceLevel.Sort();
                if (checkTracer("Fatal"))
                {
                    TXTLogger txtLogger = new TXTLogger();
                    txtLogger.Write(message, level.Fatal, myWriteResetEvent);
                }
            }
            catch
            {
            }
        }
        private static bool checkTracer(string level)
        {
            List<string> traceLevel = new List<string>(LogConfig.Instance.TraceLevel);
            bool result;
            traceLevel.Sort();
            if (traceLevel.BinarySearch(level) >= 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
