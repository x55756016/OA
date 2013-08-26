using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;




namespace SMT.Workflow.Engine.TimingService
{
    public partial class EnginService : Form
    {
        private System.Timers.Timer SendMessageTimer = null;//定时发送消息
        private System.Timers.Timer TimingTriggerTimer = null;//定时触发     
        private delegate void SendTimer();//代理
        private delegate void AddTimer();//代理

        public EnginService()
        {
            InitializeComponent();
            btnStop.Enabled = false;
            SendMessageTimer = new System.Timers.Timer();
            SendMessageTimer.Interval = SMT.Workflow.Engine.BLL.Config.MailRtxTimer * 1000;
            SendMessageTimer.Enabled = false;
            SendMessageTimer.Elapsed += new System.Timers.ElapsedEventHandler(SendMessageTimer_Elapsed);

            TimingTriggerTimer = new System.Timers.Timer();
            TimingTriggerTimer.Interval = SMT.Workflow.Engine.BLL.Config.TriggerTimer * 1000;
            TimingTriggerTimer.Enabled = false;
            TimingTriggerTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimingTriggerTimer_Elapsed);
            this.label1.Text = "定时引擎触发时间:" + SMT.Workflow.Engine.BLL.Config.TriggerTimer.ToString()+"秒";
            this.label2.Text = "邮件和RTX触发时间:" + SMT.Workflow.Engine.BLL.Config.MailRtxTimer.ToString() + "秒";

        }

        void TimingTriggerTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            SMT.Workflow.Engine.BLL.TimingTrigger.TimingTriggerActivity();//定时触发（包含短信发送）
            //System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            //this.label1.Text = "定时引擎触发时间:" + DateTime.Now.ToString();
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Start_Click(object sender, EventArgs e)
        {

            SMT.Workflow.Engine.BLL.TimingTrigger.TimingTriggerActivity();//定时
            //SMT.Workflow.Engine.BLL.SendMailRTX.SendDoTask();//待办发送邮件与RTX          
            //SendMessageTimer.Enabled = true;
            //TimingTriggerTimer.Enabled = true;
            //Start.Enabled = false;
            //btnStop.Enabled = true;
            //SMT.Workflow.Engine.BLL.TimingTrigger.TimingTriggerActivity();//定时触发（包含短信发送）
        }

        void SendMessageTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
           
            //SMT.Workflow.Engine.BLL.SendMailRTX.SendDoTask();//待办发送邮件与RTX   
            //System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            //this.label2.Text = "邮件和RTX触发时间:" + DateTime.Now.ToString();
        }

        
        /// <summary>
        /// 停止服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            SendMessageTimer.Enabled = false;
            TimingTriggerTimer.Enabled = false;
            Start.Enabled = true;
            btnStop.Enabled = false;
        }


    }
}
