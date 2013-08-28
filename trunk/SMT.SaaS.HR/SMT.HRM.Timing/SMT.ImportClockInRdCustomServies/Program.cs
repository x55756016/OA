using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace SMT.ImportClockInRdCustomServies
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new ImportRdService() 
            };
            ServiceBase.Run(ServicesToRun);

            //以下为测试事启用，
            //测试时将工程改为控制台应用程序，启动对象设为SMT.ImportClockInRdCustomServies.Program
            //否则工程设为windows应用程序，启动对象设为空
            //ImportRdService sv = new ImportRdService();
            //sv.test();
            //Console.ReadLine();
        }
    }
}
