using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AttendaceAccount;
using System.Diagnostics;

namespace SmtPortalSetUp
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_First());
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            GlobalParameters.fromFisrt.Dispose();
            GlobalParameters.fromSecond.Dispose();
            GlobalParameters.fromEmployee.Dispose();
            GlobalParameters.fromEmployeeBalance.Dispose();
            GlobalParameters.fromCompany.Dispose();
            GlobalParameters.salaryBalanceForm.Dispose();

            Application.Exit();
            //using System.Diagnostics;//记得加入此引用
            Process.GetCurrentProcess().Kill();             
        
        }
       
    }
}
