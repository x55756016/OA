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

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace SMT.ImportClockInRdCustomServies
{
    public partial class ImportRdService : ServiceBase
    {
        AttendanceServiceClient clientAtt;
        List<string> strImportIPs = new List<string>();
        List<string> strImportCompanys = new List<string>();
        List<string> strImportCheck = new List<string>();
        public zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
        int iPort = 4370;
        int iMachineNumber = 1;
        bool bIsNewDevice = false;
        private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ImportRdService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //服务启动
            this.timerImportRd.Enabled = true;
            Tracer.Debug(DateTime.Now.ToString() + "，启动服务成功");
        }

        protected override void OnStop()
        {
            //服务停止
            this.timerImportRd.Enabled = false;
            Tracer.Debug(DateTime.Now.ToString() + "，关闭服务成功");
        }

        /// <summary>
        /// 定时器运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerImportRd_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ImportRd();
        }

        private void ImportRd()
        {
            if (clientAtt==null) clientAtt = new AttendanceServiceClient();

            DateTime dtCur = DateTime.Now;
            string strElapsedHour = ConfigurationManager.AppSettings["ElapsedHour"].ToString();

            if (string.IsNullOrWhiteSpace(strElapsedHour))
            {
                Tracer.Debug(DateTime.Now.ToString() + "，导入打卡记录定时配置项(Key = ElapsedHour)未读取到");
                return;
            }

            if (dtCur.Hour != Convert.ToInt32(ConfigurationManager.AppSettings["ElapsedHour"]))
            {
                Tracer.Debug(DateTime.Now.ToString() + "，导入打卡记录未在指定时间内");
                return;
            }

            iPort = int.Parse(ConfigurationManager.AppSettings["clockPort"].ToString());

            Tracer.Debug(DateTime.Now.ToString() + "，开始导入打卡记录,设置的导入时间点为每天：" + strElapsedHour+" 点,导入的端口为："
                +iPort);

            #region 导入初始化


            string strIPs = ConfigurationManager.AppSettings["clockIp"].ToString();
            string strCompanyIDs = ConfigurationManager.AppSettings["companyID"].ToString();
            string[] ips = strIPs.Split(',');
            
            foreach (string str in ips)
            {
                strImportIPs.Add(str);
                Tracer.Debug(DateTime.Now.ToString() + "，导入的打卡机ip包括：" + str);
            }

            string[] companyIDs = strCompanyIDs.Split(',');
            foreach (string companyID in companyIDs)
            {
                strImportCompanys.Add(companyID);
                Tracer.Debug(DateTime.Now.ToString() + "，导入的公司ip包括：" + companyID);
            }


            #endregion

            try
            {
                foreach (string strCurIP in strImportIPs)
                {
                    bool bIsConnected = false;
                    string strNewDevices = ConfigurationManager.AppSettings["newDevice"].ToString();
                    bIsNewDevice = false;

                    bIsConnected = axCZKEM1.Connect_Net(strCurIP, iPort);
                    if (bIsConnected == true)
                    {
                        Tracer.Debug(DateTime.Now.ToString() + "，连接打卡机成功，打卡机IP为：" + strCurIP);
                        axCZKEM1.RegEvent(iMachineNumber, 65535);

                        if (!string.IsNullOrWhiteSpace(strNewDevices))
                        {
                            if (strNewDevices.Contains(strCurIP))
                            {
                                bIsNewDevice = true;
                            }
                        }

                        GetGeneralLogDataAndUpload(bIsConnected, strCurIP);
                    }
                    else
                    {
                        Tracer.Debug(DateTime.Now.ToString() + "，连接打卡机失败,请联系网管检查打卡机是否正常接入网络，打卡机IP为：" + strCurIP);
                    }
                }
                Tracer.Debug(DateTime.Now.ToString() + "，导入打卡记录全部完成");
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToString() + "，导入打卡记录发生异常，异常原因：" + ex.ToString());
            }
        }

        private void GetGeneralLogDataAndUpload(bool bIsConnected, string strCurIP)
        {
            try
            {
                if (bIsConnected == false)
                {
                    Tracer.Debug(DateTime.Now.ToString() + "，连接打卡机失败，打卡机IP为：" + strCurIP);
                    return;
                }

                DateTime dtFrom = new DateTime();
                DateTime dtTo = new DateTime();

                DateTime.TryParse(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), out dtFrom);
                //DateTime.TryParse(DateTime.Now.AddSeconds(-1).ToString(), out dtTo);
                dtTo = dtFrom.AddDays(1).AddSeconds(-1);

                int idwTMachineNumber = 0;
                int idwEnrollNumber = 0;
                int idwEMachineNumber = 0;
                int idwVerifyMode = 0;
                int idwInOutMode = 0;
                int idwYear = 0;
                int idwMonth = 0;
                int idwDay = 0;
                int idwHour = 0;
                int idwMinute = 0;


                List<T_HR_EMPLOYEECLOCKINRECORD> entTempList = new List<T_HR_EMPLOYEECLOCKINRECORD>();

                entTempList.Clear();

                axCZKEM1.EnableDevice(iMachineNumber, false);//disable the device
                if (axCZKEM1.ReadGeneralLogData(iMachineNumber))//read all the attendance records to the memory
                {
                    if (bIsNewDevice)
                    {
                        string sdwEnrollNumber = string.Empty;
                        int idwSecond = 0;
                        int idwWorkcode = 0;

                        while (axCZKEM1.SSR_GetGeneralLogData(iMachineNumber, out sdwEnrollNumber, out idwVerifyMode,
                               out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                        {
                            DateTime dtCurrent = new DateTime();
                            DateTime.TryParse(idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString(), out dtCurrent);
                            if (dtCurrent < dtFrom)
                            {
                                continue;
                            }

                            if (dtCurrent >= dtTo)
                            {
                                continue;
                            }

                            T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                            entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();
                            entTemp.FINGERPRINTID = sdwEnrollNumber;
                            entTemp.CLOCKID = idwWorkcode.ToString();
                            entTemp.PUNCHDATE = DateTime.Parse(dtCurrent.ToString("yyyy-MM-dd") + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":" + idwSecond.ToString());
                            entTemp.PUNCHTIME = idwHour.ToString() + ":" + idwMinute.ToString();
                            entTempList.Add(entTemp);
                        }
                    }
                    else
                    {
                        while (axCZKEM1.GetGeneralLogData(iMachineNumber, ref idwTMachineNumber, ref idwEnrollNumber,
                                ref idwEMachineNumber, ref idwVerifyMode, ref idwInOutMode, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute))//get records from the memory
                        {
                            DateTime dtCurrent = new DateTime();
                            DateTime.TryParse(idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString(), out dtCurrent);
                            if (dtCurrent < dtFrom)
                            {
                                continue;
                            }

                            if (dtCurrent >= dtTo)
                            {
                                continue;
                            }


                            T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                            entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();
                            entTemp.FINGERPRINTID = idwEnrollNumber.ToString();
                            entTemp.CLOCKID = idwTMachineNumber.ToString();
                            entTemp.PUNCHDATE = DateTime.Parse(dtCurrent.ToString("yyyy-MM-dd") + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":00");
                            entTemp.PUNCHTIME = idwHour.ToString() + ":" + idwMinute.ToString();
                            entTempList.Add(entTemp);
                        }
                    }
                }
                axCZKEM1.EnableDevice(iMachineNumber, true);//enable the device
                axCZKEM1.Disconnect();

                string strMsg = string.Empty;
                List<string> companyIds = new List<string>();

                companyIds = GetCompanyID(strCurIP);
                foreach (var strCompanyId in companyIds)
                {
                    clientAtt.ImportClockInRdListByWSRealTime(strCompanyId, entTempList.ToArray(), dtFrom, dtTo, strCurIP, ref strMsg);
                    Tracer.Debug(DateTime.Now.ToString() + "，导入打卡记录成功，打卡机IP为：" + strCurIP + "。导入的公司ID为：" + strCompanyId);
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(DateTime.Now.ToString() + "，导入打卡记录失败，打卡机IP为：" + strCurIP + "。失败原因为：" + ex.ToString());
            }
        }

        /// <summary>
        /// 获取公司ID
        /// </summary>
        /// <param name="strCurIP">打开机IP地址</param>
        /// <param name="strCompanyId">公司ID</param>
        private List<string> GetCompanyID(string strCurIP)
        {
            List<string> companyIds = new List<string>();
            try
            {
                if (strImportCompanys.Count() == 0)
                {
                    Tracer.Debug(DateTime.Now.ToString() + "，当前打卡机IP为：" + strCurIP + "无对应的公司，请检查配置项(Key = companyID)是否存在");
                    return companyIds;
                }

                foreach (string companyID in strImportCompanys)
                {
                    if (companyID.Contains(strCurIP))
                    {
                        string strCompanyId = companyID.Replace("(" + strCurIP + ")", string.Empty);
                        companyIds.Add(strCompanyId);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.Debug(ex.ToString());
            }
            return companyIds;
        }
    }

    public class CompanyImportFlag
    {
        public DateTime Importdate;
        public string Companyid = string.Empty;
        public string CompanyName = string.Empty;
        bool IsImported = false;

    }
}
