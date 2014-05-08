using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AttendaceAccount.AttendWS;
using System.Configuration;
using System.IO;
using System.Net;
using SmtPortalSetUp;

namespace AttendaceAccount
{
    public partial class AttendLogFormHanWang : Form
    {
        AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        public zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();
        private bool bIsConnected = false;  //检测是否连接
        private int iMachineNumber = 1;     //默认指纹机编号
        List<T_HR_EMPLOYEECLOCKINRECORD> entTempList = new List<T_HR_EMPLOYEECLOCKINRECORD>();
        private string strCompanyId = string.Empty;
        List<string> strImportIPs = new List<string>();
        List<string> strImportCompanys = new List<string>();
        bool bIsNewDevice = false;

        public AttendLogFormHanWang()
        {
            InitializeComponent();
            string strIPs = ConfigurationManager.AppSettings["clockIp"].ToString();
            string strNewIPs = ConfigurationManager.AppSettings["newDevice"].ToString();
            string strCompanyIDs = ConfigurationManager.AppSettings["companyID"].ToString();

            dpDateFrom.Value = DateTime.Parse(GlobalParameters.StartDate);
            dpDateTo.Value = DateTime.Parse(GlobalParameters.EndDate);

            string[] ips = strIPs.Split(',');
            string[] ipsNew = strNewIPs.Split(',');
            foreach (string str in ips)
            {
                strImportIPs.Add(str);
            }
            foreach (string str in ipsNew)
            {
                strImportIPs.Add(str);
            }

            string[] companyIDs = strCompanyIDs.Split(',');
            foreach (string companyID in companyIDs)
            {
                strImportCompanys.Add(companyID);
            }

            if (strImportIPs.Count() > 0)
            {
                txtIP.Text = strImportIPs.FirstOrDefault();
            }

        }

        /// <summary>
        /// 连接考勤机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string strNewDevices = ConfigurationManager.AppSettings["newDevice"].ToString();
            if (string.IsNullOrWhiteSpace(txtIP.Text) || string.IsNullOrWhiteSpace(txtPort.Text))
            {
                MessageBox.Show("必须输入IP地址及其端口!", "Error");
                return;
            }

            if (!strImportIPs.Contains(txtIP.Text))
            {
                MessageBox.Show("输入IP地址未录入到配置项中，请修改配置文件中对应的配置项，防止上传记录失败!", "Error");
                return; 
            }

            int idwErrorCode = 0;

            Cursor = Cursors.WaitCursor;
            if (btnConnect.Text == "断开连接")
            {
                axCZKEM1.Disconnect();
                bIsConnected = false;
                btnConnect.Text = "连接";
                lblState.Text = "连接状态:未连接";
                Cursor = Cursors.Default;
                return;
            }

            bIsConnected = axCZKEM1.Connect_Net(txtIP.Text, Convert.ToInt32(txtPort.Text));
            if (bIsConnected == true)
            {
                btnConnect.Text = "断开连接";
                btnConnect.Refresh();
                lblState.Text = "连接状态:连接";
                iMachineNumber = 1;//In fact,when you are using the tcp/ip communication,this parameter will be ignored,that is any integer will all right.Here we use 1.
                axCZKEM1.RegEvent(iMachineNumber, 65535);//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)

                if (!string.IsNullOrWhiteSpace(strNewDevices))
                {
                    if (strNewDevices.Contains(txtIP.Text))
                    {
                        bIsNewDevice = true;
                    }
                }
            }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                MessageBox.Show("无法连接到考勤机,错误代码：" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// 下载打卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetGeneralLogData_Click(object sender, EventArgs e)
        {
            DateTime dtFrom = new DateTime();
            DateTime dtTo = new DateTime();


            DateTime.TryParse(dpDateFrom.Value.ToString("yyyy-MM-dd"), out dtFrom);
            DateTime.TryParse(dpDateTo.Value.AddDays(1).ToString("yyyy-MM-dd"), out dtTo);

            if (bIsConnected == false)
            {
                MessageBox.Show("请先连接考勤机以便下载打卡记录。", "Error");
                return;
            }

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

            int idwErrorCode = 0;
            int iGLCount = 0;
            int iIndex = 0;

            Cursor = Cursors.WaitCursor;
            lvLogs.Items.Clear();
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

                        //根据员工指纹编号，取指定员工的打卡记录
                        if (!string.IsNullOrWhiteSpace(txtFingerPrintID.Text))
                        {
                            if (!txtFingerPrintID.Text.Contains(sdwEnrollNumber))
                            {
                                continue;
                            }
                        }

                        iGLCount++;
                        lvLogs.Items.Add(iGLCount.ToString());
                        lvLogs.Items[iIndex].SubItems.Add(sdwEnrollNumber);
                        switch (idwVerifyMode)
                        {
                            case 0:
                                lvLogs.Items[iIndex].SubItems.Add("密码验证");
                                break;
                            case 1:
                                lvLogs.Items[iIndex].SubItems.Add("指纹验证");
                                break;
                            case 2:
                                lvLogs.Items[iIndex].SubItems.Add("刷卡验证");
                                break;
                            default:
                                lvLogs.Items[iIndex].SubItems.Add("-");
                                break;
                        }
                        lvLogs.Items[iIndex].SubItems.Add(idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString() + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":" + idwSecond.ToString());
                        iIndex++;

                        T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                        entTemp.CLOCKINRECORDID = System.Guid.NewGuid().ToString().ToUpper();
                        entTemp.FINGERPRINTID = sdwEnrollNumber.ToString();
                        entTemp.CLOCKID = idwTMachineNumber.ToString();
                        entTemp.PUNCHDATE = DateTime.Parse(dtCurrent.ToString("yyyy-MM-dd") + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":00");
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

                        //根据员工指纹编号，取指定员工的打卡记录
                        if (!string.IsNullOrWhiteSpace(txtFingerPrintID.Text))
                        {
                            if (!txtFingerPrintID.Text.Contains(idwEnrollNumber.ToString()))
                            {
                                continue;
                            }
                        }

                        iGLCount++;
                        lvLogs.Items.Add(iGLCount.ToString());
                        lvLogs.Items[iIndex].SubItems.Add(idwEnrollNumber.ToString());
                        switch (idwVerifyMode)
                        {
                            case 0:
                                lvLogs.Items[iIndex].SubItems.Add("密码验证");
                                break;
                            case 1:
                                lvLogs.Items[iIndex].SubItems.Add("指纹验证");
                                break;
                            case 2:
                                lvLogs.Items[iIndex].SubItems.Add("刷卡验证");
                                break;
                            default:
                                lvLogs.Items[iIndex].SubItems.Add("-");
                                break;
                        }
                        lvLogs.Items[iIndex].SubItems.Add(idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString() + " " + idwHour.ToString() + ":" + idwMinute.ToString());
                        iIndex++;

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
            else
            {
                Cursor = Cursors.Default;
                axCZKEM1.GetLastError(ref idwErrorCode);

                if (idwErrorCode != 0)
                {
                    MessageBox.Show("下载打卡记录失败,错误代码: " + idwErrorCode.ToString(), "Error");
                }
                else
                {
                    MessageBox.Show("指定时间段内无打卡记录!", "Error");
                }
            }
            axCZKEM1.EnableDevice(iMachineNumber, true);//enable the device
            Cursor = Cursors.Default;
        }

        /// <summary>
        /// 上传记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUploadGeneralLogData_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtIP.Text.Trim()))
                {
                    MessageBox.Show("打卡机IP未输入，无法上传，请联系管理员！", "Error");
                    return;
                }

                if (strImportIPs.Count() == 0 || strImportCompanys.Count() == 0)
                {
                    MessageBox.Show("配置文件中涉及打卡机IP及导入机构ID的配置项关键信息丢失，无法上传，请检查配置文件！", "Error");
                    return;
                }

                if (entTempList.Count() == 0)
                {
                    MessageBox.Show("当前无下载记录，请先从打卡机下载打卡记录。", "Error");
                    return;
                }

                DateTime dtFrom = new DateTime();
                DateTime dtTo = new DateTime();

                DateTime.TryParse(dpDateFrom.Value.ToString("yyyy-MM-dd"), out dtFrom);
                DateTime.TryParse(dpDateTo.Value.ToString("yyyy-MM-dd"), out dtTo);
                dtTo = dtTo.AddDays(1).AddSeconds(-1);

                string strMsg = string.Empty;
                string strClientIP = string.Empty;
                strClientIP = txtIP.Text.Trim() + ",本次导入的客户机IP如下：" + GetClientLocalIPAddress() + GetClientInternetIPAddress();
                GetCompanyID(txtIP.Text.Trim(), ref strCompanyId);

                if (string.IsNullOrWhiteSpace(strCompanyId))
                {
                    MessageBox.Show("当前打卡机IP无对应的导入机构，无法上传，请检查配置文件！", "Error");
                    return;
                }

                clientAtt.ImportClockInRdListByWSRealTime(strCompanyId, entTempList.ToArray(), dtFrom, dtTo, strClientIP, ref strMsg);
                MessageBox.Show("打卡记录上传完毕!", "Message");
            }
            catch (Exception ex)
            {
                MessageBox.Show("上传失败，错误信息：" + ex.Message.ToString(), "Error");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportExcel(lvLogs);
        }

        public void ExportExcel(ListView lv)
        {
            if (lv.Items == null) return;

            string saveFileName = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "xls";
            saveDialog.Filter = "Excel文件|*.xls";
            saveDialog.FileName = DateTime.Now.ToString("yyyy-MM-dd");
            saveDialog.ShowDialog();
            saveFileName = saveDialog.FileName;
            if (saveFileName.IndexOf(":") < 0)
                return;
            //这里直接删除，因为saveDialog已经做了文件是否存在的判断
            if (File.Exists(saveFileName))
            {
                File.Delete(saveFileName);
            }

            if (saveDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Stream myStream = saveDialog.OpenFile();
            StreamWriter sw = new StreamWriter(myStream, System.Text.Encoding.GetEncoding(-0));
            string strColumnTitle = string.Empty;
            try
            {
                //写入列标题
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    if (i > 0)
                    {
                        strColumnTitle += "\t";
                    }

                    strColumnTitle += lv.Columns[i].Text;
                }

                sw.WriteLine(strColumnTitle);

                //写入列内容
                for (int j = 0; j < lv.Items.Count; j++)
                {
                    string strColumnValue = string.Empty;
                    for (int n = 0; n < lv.Columns.Count; n++)
                    {
                        if (n > 0)
                        {
                            strColumnValue += "\t";
                        }

                        strColumnValue += lv.Items[j].SubItems[n].Text;
                    }

                    sw.WriteLine(strColumnValue);
                }

                sw.Close();
                myStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                sw.Close();
                myStream.Close();
            }
        }

        /// <summary>
        /// 获取公司ID
        /// </summary>
        /// <param name="strCurIP">打开机IP地址</param>
        /// <param name="strCompanyId">公司ID</param>
        private void GetCompanyID(string strCurIP, ref string strCompanyId)
        {
            if (strImportCompanys.Count() == 0)
            {
                return;
            }

            foreach (string companyID in strImportCompanys)
            {
                if (companyID.Contains(strCurIP))
                {
                    strCompanyId = companyID.Replace("(" + strCurIP + ")", string.Empty);
                    break;
                }
            }
        }


        /// <summary>
        /// 获取客户端内网IP地址
        /// </summary>
        /// <returns></returns>
        private static string GetClientLocalIPAddress()
        {
            string localIP = null;
            try
            {
                IPHostEntry ipHost = System.Net.Dns.GetHostEntry(Dns.GetHostName());// Dns.Resolve(Dns.GetHostName()); ;
                IPAddress ipaddress = ipHost.AddressList[0];
                localIP = ipaddress.ToString();
                return "内网IP地址：" + localIP;
            }
            catch
            {
                return "内网IP地址：unknown";
            }
        }

        /// <summary>
        /// 获得客户端外网IP地址
        /// </summary>
        /// <returns>IP地址</returns>
        public static string GetClientInternetIPAddress()
        {
            string internetAddress = "";
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    internetAddress = webClient.DownloadString("http://www.coridc.com/ip");//从外部网页获得IP地址
                    //判断IP是否合法
                    if (!System.Text.RegularExpressions.Regex.IsMatch(internetAddress, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
                    {
                        internetAddress = webClient.DownloadString("http://fw.qq.com/ipaddress");//从腾讯提供的API中获得IP地址
                    }
                }
                return "外网IP地址：" + internetAddress;
            }
            catch
            {
                return "外网IP地址：unknown";
            }
        }

        private void btnGetEmployeeId_Click(object sender, EventArgs e)
        {
            string sql = @"select e.employeeename,e.employeeid
                          from smthrm.t_hr_employee e
                         where  
                         e.employeecname like '%" + txtEmployeeName.Text + @"%'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt.Rows.Count == 1)
            {
                txtEmployeeName.Text = dt.Rows[0]["employeeename"].ToString();
                txtEmployeeId.Text = dt.Rows[0]["employeeid"].ToString();
            }
            else
            {
                MessageBox.Show("员工有同名！");
            }
            OracleHelp.close();
        }




        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Form_Second form = GlobalParameters.fromSecond;
            form.Show();
            this.Hide();
        }

        #region 再次上传打卡记录
        List<T_HR_EMPLOYEECLOCKINRECORD> entTempListAgain = new List<T_HR_EMPLOYEECLOCKINRECORD>();
        private void btnGetAttPrintRecord_Click(object sender, EventArgs e)
        {
            #region 打卡记录
            //打卡记录时间格式带小时分钟，需特殊处理
            string sql = @"select * 
                    from smthrm.t_hr_employeeclockinrecord c
                    where c.employeeid='" + txtEmployeeId.Text + @"'
                    and c.punchdate>=to_date('" + GlobalParameters.StartDate + @"','yyyy-mm-dd')
                    and c.punchdate<=to_date('" + GlobalParameters.EndDate + @"','yyyy-mm-dd')
                    order by c.punchdate";
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {
                this.dtAttRecord.DataSource = dt;
                foreach (DataRow dr in dt.Rows)
                {
                    T_HR_EMPLOYEECLOCKINRECORD entTemp = new T_HR_EMPLOYEECLOCKINRECORD();
                    entTemp.CLOCKINRECORDID = dr["CLOCKINRECORDID"].ToString();
                    entTemp.FINGERPRINTID = dr["FINGERPRINTID"].ToString();
                    entTemp.CLOCKID = dr["CLOCKID"].ToString();
                    entTemp.PUNCHDATE =DateTime.Parse(dr["PUNCHDATE"].ToString());
                    entTemp.PUNCHTIME = dr["PUNCHTIME"].ToString();
                    entTempListAgain.Add(entTemp);
                }
                OracleHelp.close();
                //txtMessagebox.Text = "查询员工打卡记录完成，共：" + dt.Rows.Count.ToString() + "条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            else
            {
                //txtMessagebox.Text = "查询员工打卡记录完成，共：0条数据" + System.Environment.NewLine + txtMessagebox.Text;
            }
            #endregion
        }
        private void btnUploadAgain_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtIP.Text.Trim()))
                {
                    MessageBox.Show("打卡机IP未输入，无法上传，请联系管理员！", "Error");
                    return;
                }

                if (strImportIPs.Count() == 0 || strImportCompanys.Count() == 0)
                {
                    MessageBox.Show("配置文件中涉及打卡机IP及导入机构ID的配置项关键信息丢失，无法上传，请检查配置文件！", "Error");
                    return;
                }

                if (entTempListAgain.Count() == 0)
                {
                    MessageBox.Show("当前无下载记录，请先从打卡机下载打卡记录。", "Error");
                    return;
                }

                DateTime dtFrom = new DateTime();
                DateTime dtTo = new DateTime();

                DateTime.TryParse(dpDateFrom.Value.ToString("yyyy-MM-dd"), out dtFrom);
                DateTime.TryParse(dpDateTo.Value.ToString("yyyy-MM-dd"), out dtTo);
                dtTo = dtTo.AddDays(1).AddSeconds(-1);

                string strMsg = string.Empty;
                string strClientIP = string.Empty;
                strClientIP = txtIP.Text.Trim() + ",本次导入的客户机IP如下：" + GetClientLocalIPAddress() + GetClientInternetIPAddress();
                GetCompanyID(txtIP.Text.Trim(), ref strCompanyId);

                if (string.IsNullOrWhiteSpace(strCompanyId))
                {
                    MessageBox.Show("当前打卡机IP无对应的导入机构，无法上传，请检查配置文件！", "Error");
                    return;
                }

                clientAtt.ImportClockInRdListByWSRealTime(strCompanyId, entTempListAgain.ToArray(), dtFrom, dtTo, strClientIP, ref strMsg);
                MessageBox.Show("打卡记录上传完毕!", "Message");
            }
            catch (Exception ex)
            {
                MessageBox.Show("上传失败，错误信息：" + ex.Message.ToString(), "Error");
            }
        }

        #endregion
    }
}
