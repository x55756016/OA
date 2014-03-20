using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using System.Management;
using System.Configuration;
using System.Web;
using log4net;
using System.Reflection;
using SMT.ImportClockInRdCustomServies.AttendanceWS;
using System.Net;
using SMT.Foundation.Log;
using SMT.HRM.ImportAttRecordWinSV;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SMT.ImportClockInRdCustomServies
{
    public partial class ImportRdService : ServiceBase
    {
        public void ImportRdHuNanHangXing()
        {
            DateTime dtCur = DateTime.Now;          
            if (dtCur.Hour != Convert.ToInt32(ConfigurationManager.AppSettings["ElapsedHour"]))
            {
                Tracer.Debug(DateTime.Now.ToString() + "，导入打卡记录未在指定时间内");
                return;
            }
            Tracer.Debug(DateTime.Now.ToString() + "，开始导入打卡记录,设置的导入时间点为每天：" + strElapsedHour + " 点,导入的端口为："
                + iPort);
            if (clientAtt == null) clientAtt = new AttendanceServiceClient();
            List<T_HR_EMPLOYEECLOCKINRECORD> entTempList = new List<T_HR_EMPLOYEECLOCKINRECORD>();

            DateTime dtFrom = new DateTime();
            DateTime dtTo = new DateTime();

            DateTime.TryParse(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), out dtFrom);
            dtTo = dtFrom.AddDays(1).AddSeconds(-1);
            entTempList.Clear();
            using (hrEntities db = new hrEntities())
            {

                var data = from ent in db.kqjl
                           where ent.checktime >= dtFrom
                           && ent.checktime<dtTo
                           select ent;
                foreach (var item in data)
                {
                    T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                    entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();
                    entTemp.FINGERPRINTID = item.badgenumber;
                    entTemp.CLOCKID = item.machine_sn.ToString();
                    entTemp.PUNCHDATE = item.checktime;
                    entTemp.PUNCHTIME = item.checktime.Value.Hour.ToString() + ":" + item.checktime.Value.Minute.ToString();
                    entTempList.Add(entTemp);
                }


            }
            try
            {
                Tracer.Debug("航信获取打卡记录成功，下载记录数：" + entTempList.Count());
                string strMsg = string.Empty;
                string[] companyIds = ConfigurationManager.AppSettings["HangXingcompanyID"].Split(',');
                foreach (var strCompanyId in companyIds)
                {
                    if (TestMode == "true")
                    {
                        foreach (var ent in entTempList)
                        {
                            Tracer.Debug("员工指纹编码：" + ent.FINGERPRINTID + " 打卡时间：" + ent.PUNCHDATE + ":" + ent.PUNCHTIME);
                        }
                    }
                    else
                    {
                        string strCurIP = "数据库读取";
                        
                        clientAtt.ImportClockInRdListByWSRealTime(strCompanyId, entTempList.ToArray(), dtFrom, dtTo, strCurIP, ref strMsg);
                    }
                    Tracer.Debug("导入打卡记录成功，导入的公司ID为：" + strCompanyId);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug("航信导入打卡记录失败，失败原因为：" + ex.ToString());
            }

        }
    }
}
