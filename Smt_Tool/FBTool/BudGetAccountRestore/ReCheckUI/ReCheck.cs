using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DAL;
using System.IO;
using SMT_FB_EFModel;
using SMT.Foundation.Log;
using System.Threading;

namespace ReCheckUI
{
    public partial class ReCheckForm : Form
    {
        BudgetAccountDAL dal = new BudgetAccountDAL();
        /// <summary>
        /// 预算类型
        /// </summary>
        private int intAccountType = 0;
        /// <summary>
        /// 总账表数据
        /// </summary>
        //DataTable dtAcount = new DataTable();
        public ReCheckForm()
        {
            InitializeComponent();
        }

        private void btnReCheck_Click(object sender, EventArgs e)
        {

            //重新结算所有月份科目
            txtMsg.Text = "结算开始......";
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(this.ResetAllFBMoney));

            btnReCheck.Enabled = false;
            thread.Start();

          
        }

        /// <summary>
        /// //更新结算表
        /// </summary>
        /// <param name="entlist"></param>
        private void UpdataFBAcountRecord(List<FBAcountCheck> entlist)
        {
            //更新总账表
            DAL.AcountCheckTempTableAdapters.BUDGETCHECK_TEMPTableAdapter checkado = new DAL.AcountCheckTempTableAdapters.BUDGETCHECK_TEMPTableAdapter();
            AcountCheckTemp.BUDGETCHECK_TEMPDataTable dataTable = new AcountCheckTemp.BUDGETCHECK_TEMPDataTable();
            foreach (FBAcountCheck item in entlist)
            {
                AcountCheckTemp.BUDGETCHECK_TEMPRow row = dataTable.NewBUDGETCHECK_TEMPRow();
                row.BUDGETYEAR = item.year;
                row.BUDGETMONTH = item.month;
                row.BUDGETCHECKID = System.Guid.NewGuid().ToString();
                row.BUDGETCHECKDATE = DateTime.Parse(item.year.ToString() + "-" + (item.month + 1).ToString() + "-1");
                row.ACCOUNTOBJECTTYPE = decimal.Parse(item.AcountType);
                row.BUDGETMONEY = item.Bugedmoney;
                row.ACTUALMONEY = item.PaidMoney;
                row.USABLEMONEY = item.UsableMoney;
                row.SUBJECTID = item.SUBJECTID;
                row.OWNERCOMPANYID = item.OWNERCOMPANYID;
                row.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                row.OWNERPOSTID = string.Empty;
                row.OWNERID = string.Empty;
                row.YEARBUDGETMONEY = 0;
                row.YEARTOTALBUDGETMONEY = 0;
                row.CREATEUSERID = "001";
                row.CREATEDATE = row.BUDGETCHECKDATE;
                row.CREATEUSERID = "001";
                row.UPDATEDATE = row.BUDGETCHECKDATE;

                dataTable.Rows.Add(row);
            }
            dataGridView1.DataSource = dataTable;
        }


        private void ResetAllFBMoney()
        {
            //按月循环 从1月开始  取出1月结算表存在的预算科目，循环处理（取出总预算-总使用的额度），更新1月结算表额度，
            //再循环处理到2月 取出2月结算表存在的预算科目 循环处理（取出总预算-总使用的额度），更新2月结算表额度
            //.......循环处理至当前月的前一月结算表的记录
            //最后更新当前月的总账表科目额度
            decimal dYear = 2011;
            StringBuilder strMsg = new StringBuilder();
            #region 处理部门
            //查出所有部门的预算额度（预算+增补）
            SendMessage("获取所有部门的预算额度（预算+增补）");
            DataTable DepartmentAllMoney = dal.DtBudGetAllDataSource(new DateTime(2011, 1, 1), DateTime.Now, ref strMsg);
            SendMessage("获取完毕：共获取到" + DepartmentAllMoney.Rows.Count.ToString() + "条数据");
            //按月结算
            SendMessage("开始按月结算");
            for (int i = 0; i < DateTime.Now.Month; i++)
            {

                decimal dMonth = i + 1;
                string strTemp = string.Empty;
                DateTime startTime = DateTime.Now;
                //取出当前月的所有科目
                DataRow[] dt = DepartmentAllMoney.Select(@"BUDGETARYMONTH ='" + new DateTime(2011, int.Parse(dMonth.ToString()), 1).ToString() + "'");
                SendMessage("结算"+dMonth.ToString()+"月"+dt.Count().ToString()+"条记录");
                if (dMonth < DateTime.Now.Month)
                {
                    //更新预算结算表科目额度
                    dal.ResetDepartMentMoney(dYear, dMonth, ref strTemp, dt);
                    strMsg.Append(strTemp + "\r\n");
                }
                if (dMonth == DateTime.Now.Month)
                {
                    //更新预算总账表科目额度
                    //dal.ResetDeptBudgetAccount(dYear, dMonth, ref strTemp, dt);
                    strMsg.Append(strTemp + "\r\n");
                }
                DateTime endTime = DateTime.Now;
                string message=dMonth.ToString() + "月一共结算出" + dal.DicdepartmentCheck.Count().ToString() 
                    + "条记录 耗时："+(endTime-startTime).Seconds.ToString()+"秒";
                SendMessage(message);
            }
           

           
            #endregion

            List<FBAcountCheck> entlist = dal.DicdepartmentCheck;
            //更新结算表
            UpdataFBAcountRecord(entlist);
            SendMessage("处理完毕！");
        }

        private void SendMessage(string strMsg)
        {
            this.BeginInvoke(new System.Threading.ThreadStart(delegate()
            {
                txtMsg.Text =txtMsg.Text+ System.Environment.NewLine + strMsg;
            }));
        }


        #region 重新调整总账金额
        private void btnGetCheckRd_Click(object sender, EventArgs e)
        {
            Thread d = new Thread(StarCheck);
            d.Start();
            //StarCheck();
        }

        private void StarCheck()
{
    try { 
            Tracer.Debug("结算开始");
            using (SMT_FB_EFModelContext context = new SMT_FB_EFModelContext())
            {
                DateTime thisYear = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                SetLog("开始获取总账数据，请稍等......");
                List<T_FB_BUDGETACCOUNT> allItem = new List<T_FB_BUDGETACCOUNT>();
                var q = from ent in context.T_FB_BUDGETACCOUNT.Include("T_FB_SUBJECT")
                         where ent.BUDGETYEAR>=DateTime.Now.Year
                         && ent.T_FB_SUBJECT.SUBJECTNAME!="活动经费"
                         && ent.T_FB_SUBJECT.SUBJECTNAME != "部门经费"
                        orderby ent.OWNERCOMPANYID, ent.OWNERDEPARTMENTID, ent.OWNERPOSTID
                        select ent;
                allItem=q.ToList();

                SetLog("获取总账数据完毕,开始获取年度预算");
                //年度预算
                List<T_FB_COMPANYBUDGETAPPLYMASTER> T_FB_COMPANYBUDGETAPPLYMASTERList = 
                    (from ent in context.T_FB_COMPANYBUDGETAPPLYMASTER
                        where ent.BUDGETYEAR>=DateTime.Now.Year
                        && ent.CHECKSTATES==2
                        && ent.EDITSTATES==1
                        select ent).ToList();
                List<T_FB_COMPANYBUDGETAPPLYDETAIL> T_FB_COMPANYBUDGETAPPLYDETAILList =
                    (from ent in context.T_FB_COMPANYBUDGETAPPLYDETAIL.Include("T_FB_COMPANYBUDGETAPPLYMASTER").Include("T_FB_SUBJECT")
                        where ent.T_FB_COMPANYBUDGETAPPLYMASTER.BUDGETYEAR>=DateTime.Now.Year
                        && ent.T_FB_COMPANYBUDGETAPPLYMASTER.CHECKSTATES==2
                        && ent.T_FB_COMPANYBUDGETAPPLYMASTER.EDITSTATES==1
                        select ent).ToList();
                SetLog("获取年度预算完毕，开始获取年底增补");
                //年度增补
                List<T_FB_COMPANYBUDGETMODMASTER> T_FB_COMPANYBUDGETMODMASTERList =
                    (from ent in context.T_FB_COMPANYBUDGETMODMASTER
                     where ent.BUDGETYEAR >= DateTime.Now.Year
                     && ent.CHECKSTATES==2
                     select ent).ToList();
                List<T_FB_COMPANYBUDGETMODDETAIL> T_FB_COMPANYBUDGETMODDETAILList =
                    (from ent in context.T_FB_COMPANYBUDGETMODDETAIL.Include("T_FB_COMPANYBUDGETMODMASTER").Include("T_FB_SUBJECT")
                        where ent.T_FB_COMPANYBUDGETMODMASTER.BUDGETYEAR>= DateTime.Now.Year
                        && ent.T_FB_COMPANYBUDGETMODMASTER.CHECKSTATES==2
                     select ent).ToList();
                SetLog("获取年度增补数据完毕，开始获取月度预算");
                //月度预算
                List<T_FB_DEPTBUDGETAPPLYMASTER> T_FB_DEPTBUDGETAPPLYMASTERList =
                    (from ent in context.T_FB_DEPTBUDGETAPPLYMASTER
                     where ent.BUDGETARYMONTH >= thisYear
                     && ent.CHECKSTATES==2
                     && ent.EDITSTATES==1
                     select ent).ToList();
                List<T_FB_DEPTBUDGETAPPLYDETAIL> T_FB_DEPTBUDGETAPPLYDETAILList =
                    (from ent in context.T_FB_DEPTBUDGETAPPLYDETAIL.Include("T_FB_DEPTBUDGETAPPLYMASTER").Include("T_FB_SUBJECT")
                     where ent.T_FB_DEPTBUDGETAPPLYMASTER.BUDGETARYMONTH >= thisYear
                     && ent.T_FB_DEPTBUDGETAPPLYMASTER.CHECKSTATES==2
                     && ent.T_FB_DEPTBUDGETAPPLYMASTER.EDITSTATES==1
                     select ent).ToList();
                SetLog("获取月度预算完毕，开始获取月度增补");
                //月度增补
                List<T_FB_DEPTBUDGETADDMASTER> T_FB_DEPTBUDGETADDMASTERList =
                    (from ent in context.T_FB_DEPTBUDGETADDMASTER
                     where ent.BUDGETARYMONTH >= thisYear
                     && ent.CHECKSTATES==2
                     select ent).ToList();
                List<T_FB_DEPTBUDGETADDDETAIL> T_FB_DEPTBUDGETADDDETAILList 
                    = (from ent in context.T_FB_DEPTBUDGETADDDETAIL.Include("T_FB_DEPTBUDGETADDMASTER").Include("T_FB_SUBJECT")
                       where ent.T_FB_DEPTBUDGETADDMASTER.BUDGETARYMONTH >= thisYear
                       && ent.T_FB_DEPTBUDGETADDMASTER.CHECKSTATES==2
                     select ent).ToList();
                SetLog("获取月度增补完毕，开始获取个人预算申请及增补");
                List<T_FB_PERSONBUDGETAPPLYDETAIL> T_FB_PERSONBUDGETAPPLYDETAILList =
                    (from ent in context.T_FB_PERSONBUDGETAPPLYDETAIL.Include("T_FB_DEPTBUDGETAPPLYDETAIL").Include("T_FB_SUBJECT")
                     where ent.T_FB_DEPTBUDGETAPPLYDETAIL.T_FB_DEPTBUDGETAPPLYMASTER.BUDGETARYMONTH >= thisYear
                     && ent.T_FB_DEPTBUDGETAPPLYDETAIL.T_FB_DEPTBUDGETAPPLYMASTER.CHECKSTATES==2
                     && ent.T_FB_DEPTBUDGETAPPLYDETAIL.T_FB_DEPTBUDGETAPPLYMASTER.EDITSTATES==1
                     select ent).ToList();
                List<T_FB_PERSONBUDGETADDDETAIL> T_FB_PERSONBUDGETADDDETAILList =
                    (from ent in context.T_FB_PERSONBUDGETADDDETAIL.Include("T_FB_DEPTBUDGETADDDETAIL").Include("T_FB_SUBJECT")
                     where ent.T_FB_DEPTBUDGETADDDETAIL.T_FB_DEPTBUDGETADDMASTER.BUDGETARYMONTH >= thisYear
                     && ent.T_FB_DEPTBUDGETADDDETAIL.T_FB_DEPTBUDGETADDMASTER.CHECKSTATES==2
                     select ent).ToList();
                SetLog("获取个人预算申请及增补完毕，开始获取费用报销");
                //个人费用报销
                List<T_FB_CHARGEAPPLYMASTER> T_FB_CHARGEAPPLYMASTERList = 
                    (from ent in context.T_FB_CHARGEAPPLYMASTER
                     where ent.BUDGETARYMONTH >= thisYear
                     && ent.CHECKSTATES==2
                     select ent).ToList();
                List<T_FB_CHARGEAPPLYDETAIL> T_FB_CHARGEAPPLYDETAILList = 
                    (from ent in context.T_FB_CHARGEAPPLYDETAIL.Include("T_FB_CHARGEAPPLYMASTER").Include("T_FB_SUBJECT")
                     where ent.T_FB_CHARGEAPPLYMASTER.BUDGETARYMONTH >= thisYear
                     && ent.T_FB_CHARGEAPPLYMASTER.CHECKSTATES==2
                     select ent).ToList();
                SetLog("获取费用报销完毕，开始处理......");
                int allCount = allItem.Count;
                int i = 0;
                foreach (var item in allItem)
                {
                    i++;
                    if (item.T_FB_SUBJECT.SUBJECTID=="08c1d9c6-2396-43c3-99f9-227e4a7eb417")// == "部门经费")
                    {
                        continue;
                    }
                    if (item.T_FB_SUBJECT.SUBJECTID == "d5134466-c207-44f2-8a36-cf7b96d5851f")//活动经费
                    {
                        continue;
                    }

                    if (item.OWNERDEPARTMENTID == "7f663615-56a0-46ed-96a3-0e9a79d2cb51"
                        && item.T_FB_SUBJECT.SUBJECTID == "00161652-e3bf-4e9f-9a57-a9e1ff8cef74")
                    {

                    }
                    string id=   item.T_FB_SUBJECT.SUBJECTID;
                    decimal A=0;//年度预算
                    decimal B=0;//年度增补
                    decimal C=0;//部门月度预算=公共+个人
                    decimal D=0;//部门月度增补=公共+个人
                    decimal C1 = 0;//月度预算-部门公共
                    decimal D1 = 0;//月度增补-部门公共
                    decimal C2 = 0;//月度预算-个人
                    decimal D2 = 0;//月度增补-个人
                    decimal E = 0;//个人费用报销已终审-部门公共
                    decimal F = 0;//个人费用报销审核中-部门公共
                    decimal G = 0;//个人费用报销已终审-个人
                    decimal H = 0;//个人费用报销审核中-个人
                    GetABCD(T_FB_COMPANYBUDGETAPPLYMASTERList,
                        T_FB_COMPANYBUDGETAPPLYDETAILList,
                        T_FB_COMPANYBUDGETMODMASTERList,
                        T_FB_COMPANYBUDGETMODDETAILList,
                        T_FB_DEPTBUDGETAPPLYMASTERList,
                        T_FB_DEPTBUDGETAPPLYDETAILList,
                        T_FB_DEPTBUDGETADDMASTERList,
                        T_FB_DEPTBUDGETADDDETAILList,
                        T_FB_CHARGEAPPLYMASTERList,
                        T_FB_CHARGEAPPLYDETAILList,
                        T_FB_PERSONBUDGETAPPLYDETAILList,
                        T_FB_PERSONBUDGETADDDETAILList,
                        item, 
                        ref A, ref B, ref C, 
                        ref C1, ref D1,
                        ref C2, ref D2, 
                        ref D, ref E, 
                        ref F, ref G, 
                        ref H);
                    decimal BUDGETMONEY = item.BUDGETMONEY.Value;
                    decimal USABLEMONEY = item.USABLEMONEY.Value;
                    decimal ACTUALMONEY = item.ACTUALMONEY.Value;
                    decimal PAIEDMONEY = item.PAIEDMONEY.Value;
                    switch(item.ACCOUNTOBJECTTYPE.Value.ToString())
                    {
                        case "1"://部门年度预算
                            item.BUDGETMONEY = A+B;//预算金额
                            item.USABLEMONEY = A+B-C-D;//可用额度
                            item.ACTUALMONEY = A+B-C-D;//实际额度
                            item.PAIEDMONEY = C+D;//已用额度
                            break;
                        case "2"://部门月度预算--公共科目
                            item.BUDGETMONEY = C1+D1;//预算金额
                            item.USABLEMONEY = C1+D1-E-F;//可用额度
                            item.ACTUALMONEY = C1+D1-E;//实际额度
                            item.PAIEDMONEY = E;//已用额度
                            break;
                        case "3"://部门月度预算--个人月度预算
                            item.BUDGETMONEY = C2+D2;//预算金额
                            item.USABLEMONEY = C2+D2-G-H;//可用额度
                            item.ACTUALMONEY = C2+D2-G;//实际额度
                            item.PAIEDMONEY = G;//已用额度
                            break;
                    }
                    string msg = string.Empty;
                    msg = "总 " + allCount + " 第 " + i + " 条:";
                    if (BUDGETMONEY != item.BUDGETMONEY.Value)
                    {
                         msg += " BUDGETMONEY 不对，公司：" + item.OWNERCOMPANYID
                            //+ " 部门：" + item.OWNERDEPARTMENTID
                            //+ " 岗位：" + item.OWNERPOSTID
                            //+ " 科目id：" + item.T_FB_SUBJECT.SUBJECTID
                            +" 总账id"+item.BUDGETACCOUNTID
                            + " 预算类型：" + item.ACCOUNTOBJECTTYPE.Value.ToString()
                            + " 科目：" + item.T_FB_SUBJECT.SUBJECTNAME
                            + " 修改前 BUDGETMONEY:" + BUDGETMONEY
                            + " 修改后 BUDGETMONEY    ->    " + item.BUDGETMONEY.Value
                            + " A:" + A
                            + " B:" + B
                            + " C:" + C
                            + " D:" + D
                            + " C1:" + C1
                            + " D1:" + D1
                            + " C2:" + C2
                            + " D2:" + D2
                            + " E:" + E
                            + " F:" + F
                            + " G:" + G
                            + " H:" + H;
                    }
                    if (USABLEMONEY != item.USABLEMONEY.Value)
                    {
                        msg += " USABLEMONEY 不对，公司：" + item.OWNERCOMPANYID
                              //+ " 部门：" + item.OWNERDEPARTMENTID
                              //+ " 岗位：" + item.OWNERPOSTID
                              //+ " 科目id：" + item.T_FB_SUBJECT.SUBJECTID
                              + " 总账id" + item.BUDGETACCOUNTID
                              + " 预算类型：" + item.ACCOUNTOBJECTTYPE.Value.ToString()
                              + " 科目：" + item.T_FB_SUBJECT.SUBJECTNAME
                              + " 修改前 USABLEMONEY:" + USABLEMONEY
                              + " 修改后 USABLEMONEY    ->    " + item.USABLEMONEY.Value
                              + " A:" + A
                            + " B:" + B
                            + " C:" + C
                            + " D:" + D
                            + " C1:" + C1
                            + " D1:" + D1
                            + " C2:" + C2
                            + " D2:" + D2
                            + " E:" + E
                            + " F:" + F
                            + " G:" + G
                            + " H:" + H;
                    }
                    if (ACTUALMONEY != item.ACTUALMONEY.Value)
                    {
                        msg += " ACTUALMONEY 不对，公司：" + item.OWNERCOMPANYID
                              //+ " 部门：" + item.OWNERDEPARTMENTID
                              //+ " 岗位：" + item.OWNERPOSTID
                              //+ " 科目id：" + item.T_FB_SUBJECT.SUBJECTID
                              + " 总账id" + item.BUDGETACCOUNTID
                              + " 预算类型：" + item.ACCOUNTOBJECTTYPE.Value.ToString()
                              + " 科目：" + item.T_FB_SUBJECT.SUBJECTNAME
                              + " 修改前 ACTUALMONEY:" + ACTUALMONEY
                              + " 修改后 ACTUALMONEY    ->    " + item.ACTUALMONEY.Value
                               + " A:" + A
                            + " B:" + B
                            + " C:" + C
                            + " D:" + D
                            + " C1:" + C1
                            + " D1:" + D1
                            + " C2:" + C2
                            + " D2:" + D2
                            + " E:" + E
                            + " F:" + F
                            + " G:" + G
                            + " H:" + H;
                    }
                    if (PAIEDMONEY != item.PAIEDMONEY.Value)
                    {
                       msg +=" PAIEDMONEY 不对，公司：" + item.OWNERCOMPANYID
                                //+ " 部门id：" + item.OWNERDEPARTMENTID
                                //+ " 岗位id：" + item.OWNERPOSTID
                                //+ " 科目id："+item.T_FB_SUBJECT.SUBJECTID
                                + " 总账id" + item.BUDGETACCOUNTID
                                + " 预算类型：" + item.ACCOUNTOBJECTTYPE.Value.ToString()
                                + " 科目：" + item.T_FB_SUBJECT.SUBJECTNAME
                                + " 修改前 PAIEDMONEY:" + PAIEDMONEY
                                + " 修改后 PAIEDMONEY    ->    " + item.PAIEDMONEY.Value
                                 + " A:" + A
                            + " B:" + B
                            + " C:" + C
                            + " D:" + D
                            + " C1:" + C1
                            + " D1:" + D1
                            + " C2:" + C2
                            + " D2:" + D2
                            + " E:" + E
                            + " F:" + F
                            + " G:" + G
                            + " H:" + H;
                    }
                    SetLog(msg);
                }

                MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("确定要更新异常的数据吗?", "确认", messButton);
                if (dr == DialogResult.OK)//如果点击“确定”按钮
                {
                    int j = context.SaveChanges();
                    MessageBox.Show("处理完毕,共处理了: " + j + " 条数据");
                }
                i = 0;
            }
        
            MessageBox.Show("处理完毕！");
        
    }catch(Exception ex)
    {
        MessageBox.Show("异常："+ex.ToString());
        SetLog(ex.ToString());
    }
}

        private void GetABCD( List<T_FB_COMPANYBUDGETAPPLYMASTER> T_FB_COMPANYBUDGETAPPLYMASTERList,
       
            List<T_FB_COMPANYBUDGETAPPLYDETAIL> T_FB_COMPANYBUDGETAPPLYDETAILList,
             List<T_FB_COMPANYBUDGETMODMASTER> T_FB_COMPANYBUDGETMODMASTERList ,
            List<T_FB_COMPANYBUDGETMODDETAIL> T_FB_COMPANYBUDGETMODDETAILList,
            List<T_FB_DEPTBUDGETAPPLYMASTER> T_FB_DEPTBUDGETAPPLYMASTERList,

            List<T_FB_DEPTBUDGETAPPLYDETAIL> T_FB_DEPTBUDGETAPPLYDETAILList ,
             List<T_FB_DEPTBUDGETADDMASTER> T_FB_DEPTBUDGETADDMASTERList,
            List<T_FB_DEPTBUDGETADDDETAIL> T_FB_DEPTBUDGETADDDETAILList,
             List<T_FB_CHARGEAPPLYMASTER> T_FB_CHARGEAPPLYMASTERList,

            List<T_FB_CHARGEAPPLYDETAIL> T_FB_CHARGEAPPLYDETAILList,

            List<T_FB_PERSONBUDGETAPPLYDETAIL> T_FB_PERSONBUDGETAPPLYDETAILList,
            List<T_FB_PERSONBUDGETADDDETAIL> T_FB_PERSONBUDGETADDDETAILList,

            T_FB_BUDGETACCOUNT item,
            ref decimal A,ref decimal B,
            ref decimal C, 
            ref decimal C1, ref decimal D1,
            ref decimal C2, ref decimal D2, 
            ref decimal D, ref decimal E, 
            ref decimal F, ref decimal H, 
            ref decimal G)
        {
            
            #region//年度预算A
            var YearMoney = from a in T_FB_COMPANYBUDGETAPPLYMASTERList
                            join b in T_FB_COMPANYBUDGETAPPLYDETAILList
                         on a.COMPANYBUDGETAPPLYMASTERID equals b.T_FB_COMPANYBUDGETAPPLYMASTER.COMPANYBUDGETAPPLYMASTERID
                         where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                         && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                         && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                         && a.BUDGETYEAR == item.BUDGETYEAR
                         && a.CHECKSTATES==2
                         select b.BUDGETMONEY;
                if (YearMoney.Count() > 0)
                {
                    foreach (var va in YearMoney)
                    {
                        A = A + va;
                    }
                }
            #endregion

                #region//年度增补B
                var YearAddMoney = from a in T_FB_COMPANYBUDGETMODMASTERList
                                   join b in T_FB_COMPANYBUDGETMODDETAILList
                         on a.COMPANYBUDGETMODMASTERID equals b.T_FB_COMPANYBUDGETMODMASTER.COMPANYBUDGETMODMASTERID
                         where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                         && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                         && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                         && a.BUDGETYEAR == item.BUDGETYEAR
                         && a.CHECKSTATES == 2
                         select b.BUDGETMONEY;
                if (YearAddMoney.Count() > 0)
                {
                    foreach (var va in YearAddMoney)
                    {
                        B = B + va;
                    }
                }
                #endregion

                #region//月度预算部门C
                var MonthMoeny = from a in T_FB_DEPTBUDGETAPPLYMASTERList
                                 join b in T_FB_DEPTBUDGETAPPLYDETAILList
                         on a.DEPTBUDGETAPPLYMASTERID equals b.T_FB_DEPTBUDGETAPPLYMASTER.DEPTBUDGETAPPLYMASTERID
                         where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                         && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                         && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                         && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                         && a.CHECKSTATES == 2
                         select b.TOTALBUDGETMONEY;
                if (MonthMoeny.Count() > 0)
                {
                    foreach (var va in MonthMoeny)
                    {
                        C = C + va.Value;
                    }
                }
                #endregion

                #region//月度预算增补部门D
                var MonthAddMoeny = from a in T_FB_DEPTBUDGETADDMASTERList
                                    join b in T_FB_DEPTBUDGETADDDETAILList
                         on a.DEPTBUDGETADDMASTERID equals b.T_FB_DEPTBUDGETADDMASTER.DEPTBUDGETADDMASTERID
                         where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                         && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                         && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                         && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                         && a.CHECKSTATES == 2
                         select b.TOTALBUDGETMONEY;
                if (MonthAddMoeny.Count() > 0)
                {
                    foreach (var va in MonthAddMoeny)
                    {
                        D = D + va.Value;
                    }
                }
                #endregion

                #region//月度预算-部门公共C1
                var DepartmentCommonMonthMoeny = from a in T_FB_DEPTBUDGETAPPLYMASTERList
                                                 join b in T_FB_DEPTBUDGETAPPLYDETAILList
                                 on a.DEPTBUDGETAPPLYMASTERID equals b.T_FB_DEPTBUDGETAPPLYMASTER.DEPTBUDGETAPPLYMASTERID
                                 where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                 && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                 && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                                 && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                 && a.CHECKSTATES == 2
                                 select b.BUDGETMONEY;
                if (DepartmentCommonMonthMoeny.Count() > 0)
                {
                    foreach (var va in DepartmentCommonMonthMoeny)
                    {
                        C1 = C1 + va;
                    }
                }
                #endregion

                #region//月度预算增补-部门公共D1
                var DepartmentMonthAddMoeny = from a in T_FB_DEPTBUDGETADDMASTERList
                                              join b in T_FB_DEPTBUDGETADDDETAILList
                                    on a.DEPTBUDGETADDMASTERID equals b.T_FB_DEPTBUDGETADDMASTER.DEPTBUDGETADDMASTERID
                                    where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                    && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                    && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                                    && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                    && a.CHECKSTATES == 2
                                    select b.BUDGETMONEY;
                if (DepartmentMonthAddMoeny.Count() > 0)
                {
                    foreach (var va in DepartmentMonthAddMoeny)
                    {
                        D1 = D1 + va;
                    }
                }
                #endregion

                #region//月度预算-个人C2
                var PersonCommonMonthMoeny = from a in T_FB_DEPTBUDGETAPPLYMASTERList
                                             join b in T_FB_DEPTBUDGETAPPLYDETAILList
                                                 on a.DEPTBUDGETAPPLYMASTERID equals b.T_FB_DEPTBUDGETAPPLYMASTER.DEPTBUDGETAPPLYMASTERID
                                             join c in T_FB_PERSONBUDGETAPPLYDETAILList
                                                 on b.DEPTBUDGETAPPLYDETAILID equals c.T_FB_DEPTBUDGETAPPLYDETAIL.DEPTBUDGETAPPLYDETAILID
                                                 where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                                 && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                                 && c.OWNERPOSTID == item.OWNERPOSTID
                                                 && c.OWNERID == item.OWNERID
                                                 && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                                 && a.CHECKSTATES == 2
                                                 select c.BUDGETMONEY;
                if (PersonCommonMonthMoeny.Count() > 0)
                {
                    foreach (var va in PersonCommonMonthMoeny)
                    {
                        C2 = C2 + va.Value;
                    }
                }
                #endregion

                #region//月度预算增补-个人D2
                var PersonMonthAddMoeny = from a in T_FB_DEPTBUDGETADDMASTERList
                                          join b in T_FB_DEPTBUDGETADDDETAILList
                                              on a.DEPTBUDGETADDMASTERID equals b.T_FB_DEPTBUDGETADDMASTER.DEPTBUDGETADDMASTERID
                                          join c in T_FB_PERSONBUDGETADDDETAILList
                                              on b.DEPTBUDGETADDDETAILID equals c.T_FB_DEPTBUDGETADDDETAIL.DEPTBUDGETADDDETAILID
                                              where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                              && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                              && c.OWNERPOSTID == item.OWNERPOSTID
                                              && c.OWNERID == item.OWNERID
                                              && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                              && a.CHECKSTATES == 2
                                              select c.BUDGETMONEY;
                if (PersonMonthAddMoeny.Count() > 0)
                {
                    foreach (var va in PersonMonthAddMoeny)
                    {
                        D2 = D2 + va.Value;
                    }
                }
                #endregion

                #region//个人费用部门科目报销已终审E
                if(T_FB_CHARGEAPPLYDETAILList!=null)
                { 
                    var ChargeMoenyChecked = from a in T_FB_CHARGEAPPLYMASTERList
                                             join b in T_FB_CHARGEAPPLYDETAILList
                                        on a.CHARGEAPPLYMASTERID equals b.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID
                                        where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                        && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                        && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                                        && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                        && a.CHECKSTATES == 2
                                        && b.CHARGETYPE==2//部门
                                        select b.CHARGEMONEY;
                    if (ChargeMoenyChecked.Count() > 0)
                    {
                        foreach (var va in ChargeMoenyChecked)
                        {
                            E = E + va;
                        }
                    }
                }
                #endregion

                #region//个人费用部门科目报销终审中F
                if (T_FB_CHARGEAPPLYDETAILList != null)
                {
                    var ChargeingMoeny = from a in T_FB_CHARGEAPPLYMASTERList
                                         join b in T_FB_CHARGEAPPLYDETAILList
                                      on a.CHARGEAPPLYMASTERID equals b.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID
                                         where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                         && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                         && a.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID
                                         && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                         && a.CHECKSTATES == 1
                                         && b.CHARGETYPE == 2//部门
                                         select b.CHARGEMONEY;
                    if (ChargeingMoeny.Count() > 0)
                    {
                        foreach (var va in ChargeingMoeny)
                        {
                            F = F + va;
                        }
                    }
                }
                #endregion

                #region//个人费用个人科目报销已终审G
                if (T_FB_CHARGEAPPLYDETAILList != null)
                {
                    var ChargeMoenyPersonChecked = from a in T_FB_CHARGEAPPLYMASTERList
                                                   join b in T_FB_CHARGEAPPLYDETAILList
                                             on a.CHARGEAPPLYMASTERID equals b.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID
                                                   where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                                   && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                                   && a.OWNERPOSTID == item.OWNERPOSTID
                                                   && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                                   && a.CHECKSTATES == 2
                                                   && b.CHARGETYPE == 1//个人
                                                   select b.CHARGEMONEY;
                    if (ChargeMoenyPersonChecked.Count() > 0)
                    {
                        foreach (var va in ChargeMoenyPersonChecked)
                        {
                            G = G + va;
                        }
                    }
                }
                #endregion

                #region//个人费用个人科目报销终审中H
                if (T_FB_CHARGEAPPLYDETAILList != null)
                {
                    var ChargeingPersonMoeny = from a in T_FB_CHARGEAPPLYMASTERList
                                               join b in T_FB_CHARGEAPPLYDETAILList
                                         on a.CHARGEAPPLYMASTERID equals b.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID
                                               where a.OWNERCOMPANYID == item.OWNERCOMPANYID
                                               && b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                               && a.OWNERPOSTID == item.OWNERPOSTID
                                               && a.BUDGETARYMONTH.Year == item.BUDGETYEAR
                                               && a.CHECKSTATES == 1
                                               && b.CHARGETYPE == 1//个人
                                               select b.CHARGEMONEY;
                    if (ChargeingPersonMoeny.Count() > 0)
                    {
                        foreach (var va in ChargeingPersonMoeny)
                        {
                            H = H + va;
                        }
                    }
                }
                #endregion
        }

        #endregion
        

        #region 跨线程调用UI控件显示消息
        delegate void DelShow(String Msg); //代理
        //将对控件的操作写到一个函数中 
        public void SetLog(String para)
        {
            if (!txtMsg.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                txtMsg.Text = DateTime.Now.ToLongTimeString() + " " + para + System.Environment.NewLine + txtMsg.Text;
            }
            else //非创建线程，用代理进行操作
            {
                DelShow ds = new DelShow(SetLog);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }
        #endregion



        AcountCheckTemp.ERRORACCOUT_DOUBLEDataTable dt = new AcountCheckTemp.ERRORACCOUT_DOUBLEDataTable();
        private void btnSelectDoubleAcount_Click(object sender, EventArgs e)
        {
            DAL.AcountCheckTempTableAdapters.ERRORACCOUT_DOUBLETableAdapter ad = new DAL.AcountCheckTempTableAdapters.ERRORACCOUT_DOUBLETableAdapter();
            ad.Fill(dt);
            dataGridView2.DataSource = dt;
        }


        #region 合并重复科目
        private void BtnAcountIn_Click(object sender, EventArgs e)
        {
            DAL.AcountCheckTempTableAdapters.ZZZ_DEPTTableAdapter depAD = new DAL.AcountCheckTempTableAdapters.ZZZ_DEPTTableAdapter();
            DAL.AcountCheckTemp.ZZZ_DEPTDataTable depDT = new AcountCheckTemp.ZZZ_DEPTDataTable();
            if (dt == null || dt.Rows.Count <= 0)
            {
                MessageBox.Show("无任何需要处理的数据");
                return;
            }
            foreach (AcountCheckTemp.ERRORACCOUT_DOUBLERow dr in dt.Rows)
            {
                try
                {
                    switch (dr.ACCOUNTOBJECTTYPE.ToString())
                    {
                        case "1"://公司
                            break;
                        case "2"://部门
                            MergeDuplicateDepartFees(dr);
                            break;
                        case "3"://个人
                            MergeDuplicatePersonFees(dr);
                            break;
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    continue;
                }
            }

            MessageBox.Show("处理完成！");
        }

        private void MergeDuplicateDepartFees(AcountCheckTemp.ERRORACCOUT_DOUBLERow dr)
        {
            //查出流水中的实际额度，可用额度
            //实际额度
            string strActual = @"select sum(zzz.BUDGETMONEY) from ZZZ_DEPT  zzz where zzz.OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                        "' and zzz.SUBJECTID='" + dr.SUBJECTID + "' and zzz.OWNERDEPARTMENTID='" + dr.OWNERDEPARTMENTID + "'"
                        + "and zzz.CHECKSTATESNAME='审核通过'";
            object Actualobj = dal.ExecuteCustomerSql(strActual);
            //可用额度
            string strUsable = @"select sum(zzz.BUDGETMONEY) from ZZZ_DEPT  zzz where zzz.OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                        "' and zzz.SUBJECTID='" + dr.SUBJECTID + "' and zzz.OWNERDEPARTMENTID='" + dr.OWNERDEPARTMENTID + "'"
                        + "and zzz.CHECKSTATESNAME <> '审核中或未汇总'";
            object Usableobj = dal.ExecuteCustomerSql(strUsable);

            decimal Actualmoney = 0;
            if (Actualobj != null)
            {
                Actualmoney = decimal.Parse(Actualobj.ToString());
            }
            decimal Usablemoney = 0;
            if (Usableobj != null)
            {
                Usablemoney = decimal.Parse(Usableobj.ToString());
            }
            DataRow[] rows = dt.Select("ACCOUNTOBJECTTYPE=2 and OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                        "' and SUBJECTID='" + dr.SUBJECTID + "' and OWNERDEPARTMENTID='" + dr.OWNERDEPARTMENTID + "'");
            if (rows.Count() > 1)
            {
                string updateid = rows[0]["BUDGETACCOUNTID"].ToString();
                string strUpdate = "update t_fb_budgetaccount t set t.actualmoney=" + Actualmoney
                    + ", t.usablemoney=" + Usablemoney + " where t.budgetaccountid='" + updateid + "'";
                object falg = dal.ExecuteCustomerSql(strUpdate);
                foreach (DataRow drdel in rows)
                {
                    if (drdel["BUDGETACCOUNTID"] == System.DBNull.Value)
                    {
                        continue;
                    }

                    if (drdel["BUDGETACCOUNTID"].ToString() != updateid)
                    {
                        string strdel = "delete t_fb_budgetaccount t where t.budgetaccountid='" + drdel["BUDGETACCOUNTID"].ToString() + "'";
                        object falgdel = dal.ExecuteCustomerSql(strdel);
                    }
                }
            }
        }


        private void MergeDuplicatePersonFees(AcountCheckTemp.ERRORACCOUT_DOUBLERow dr)
        {
            //查出流水中的实际额度，可用额度
            //实际额度
            string strActual = @"select sum(zzz.BUDGETMONEY) from zzz_person  zzz where zzz.OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                        "' and zzz.SUBJECTID='" + dr.SUBJECTID + "' and zzz.OWNERID='" + dr.OWNERID + "'"
                        + "and zzz.CHECKSTATESNAME='审核通过'";
            object Actualobj = dal.ExecuteCustomerSql(strActual);



            //可用额度
            string strUsable = @"select sum(zzz.BUDGETMONEY) from zzz_person  zzz where zzz.OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                        "' and zzz.SUBJECTID='" + dr.SUBJECTID + "' and zzz.OWNERID='" + dr.OWNERID + "'"
                        + "or ( zzz.OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                       "' and zzz.SUBJECTID='" + dr.SUBJECTID + "' and zzz.OWNERID='" + dr.OWNERID + "'"
                        + "and zzz.CHECKSTATESNAME <> '审核中'"
                        + "and zzz.ORDERTYPENAME = '还款单') ";
            object Usableobj = dal.ExecuteCustomerSql(strUsable);

            //取流水最新数据
            string strFreshRecord = @"select ep.POSTID,d.DEPARTMENTID,c.COMPANYID from  smthrm.t_hr_employeepost ep  
                                        inner join smthrm.t_hr_post p on ep.postid = p.postid
                                         inner join smthrm.t_hr_department d on p.departmentid = d.departmentid
                                         inner join smthrm.t_hr_company c on d.companyid = c.companyid where ep.employeeid='" + dr.OWNERID + "'"
                                        +@"and  ep.editstate=1  and ep.checkstate=2 and ep.isagency=0";

            DataTable dtFreshRecord = dal.GetDataTable(strFreshRecord);
            


            decimal Actualmoney = 0;
            if (Actualobj != null)
            {
                Actualmoney = decimal.Parse(Actualobj.ToString());
            }
            decimal Usablemoney = 0;
            if (Usableobj != null)
            {
                Usablemoney = decimal.Parse(Usableobj.ToString());
            }
            DataRow[] rows = dt.Select("ACCOUNTOBJECTTYPE=3 and OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                        "' and SUBJECTID='" + dr.SUBJECTID + "' and OWNERID='" + dr.OWNERID + "'");
            if (rows.Count() > 1)
            {
                string updateid = rows[0]["BUDGETACCOUNTID"].ToString();
                
                string strUpdate = "update t_fb_budgetaccount t set t.actualmoney=" + Actualmoney
                    + ", t.usablemoney=" + Usablemoney + " where t.budgetaccountid='" + updateid + "'";
                object falg = dal.ExecuteCustomerSql(strUpdate);

                //更新为最新的公司，部门，岗位
                if (dtFreshRecord.Rows.Count > 0)
                {
                    string strUpdateFreshRow = "update t_fb_budgetaccount t set t.OWNERDEPARTMENTID=" + dtFreshRecord.Rows[0]["DEPARTMENTID"].ToString()
                        + ", t.OWNERPOSTID=" + dtFreshRecord.Rows[0]["POSTID"].ToString()
                   + ", t.OWNERCOMPANYID=" + dtFreshRecord.Rows[0]["COMPANYID"].ToString() + " where t.budgetaccountid='" + updateid + "'";
                    object falgs = dal.ExecuteCustomerSql(strUpdate);
                }

                foreach (DataRow drdel in rows)
                {
                    if (drdel["BUDGETACCOUNTID"] == System.DBNull.Value)
                    {
                        continue;
                    }

                    if (drdel["BUDGETACCOUNTID"].ToString() != updateid)
                    {
                        string strdel = "delete t_fb_budgetaccount t where t.budgetaccountid='" + drdel["BUDGETACCOUNTID"].ToString() + "'";
                        object falgdel = dal.ExecuteCustomerSql(strdel);
                    }
                }
            }
        }

        #endregion

        AcountCheckTemp.T_FB_BUDGETACCOUNTDataTable accErrTable = new AcountCheckTemp.T_FB_BUDGETACCOUNTDataTable();
        private void btnGetActualmoney_Click(object sender, EventArgs e)
        {
            DAL.AcountCheckTempTableAdapters.T_FB_BUDGETACCOUNTTableAdapter ad = new DAL.AcountCheckTempTableAdapters.T_FB_BUDGETACCOUNTTableAdapter();
            AcountCheckTemp.T_FB_BUDGETACCOUNTDataTable accTable = new AcountCheckTemp.T_FB_BUDGETACCOUNTDataTable();

            ad.Fill(accTable);

            List<AcountCheckTemp.T_FB_BUDGETACCOUNTRow> rowTemps = accTable.Where(t => t.USABLEMONEY > t.ACTUALMONEY).ToList();

            if (rowTemps.Count == 0)
            {
                MessageBox.Show("无记录");
                return;
            }

            foreach (AcountCheckTemp.T_FB_BUDGETACCOUNTRow item in rowTemps)
            {
                AcountCheckTemp.T_FB_BUDGETACCOUNTRow rowTemp = accErrTable.NewT_FB_BUDGETACCOUNTRow();
                rowTemp.BUDGETACCOUNTID = item.BUDGETACCOUNTID;
                rowTemp.BUDGETYEAR = item.BUDGETYEAR;
                rowTemp.BUDGETMONTH = item.BUDGETMONTH;
                rowTemp.ACCOUNTOBJECTTYPE = item.ACCOUNTOBJECTTYPE;

                rowTemp.SUBJECTID = item.SUBJECTID;
                rowTemp.BUDGETMONEY = item.BUDGETMONEY;
                rowTemp.USABLEMONEY = item.USABLEMONEY;
                rowTemp.ACTUALMONEY = item.ACTUALMONEY;
                rowTemp.PAIEDMONEY = item.PAIEDMONEY;
                rowTemp.OWNERCOMPANYID = item.OWNERCOMPANYID;
                rowTemp.OWNERDEPARTMENTID = item.OWNERDEPARTMENTID;
                rowTemp.OWNERPOSTID = item.OWNERPOSTID;
                rowTemp.OWNERID = item.OWNERID;

                rowTemp.CREATEUSERID = item.CREATEUSERID;
                rowTemp.CREATEDATE = item.CREATEDATE;
                rowTemp.UPDATEUSERID = item.UPDATEUSERID;
                rowTemp.UPDATEDATE = item.UPDATEDATE;

                accErrTable.Rows.Add(rowTemp);
            }

            dataGridView3.DataSource = accErrTable;
        }

        private void btnChangeActualmoney_Click(object sender, EventArgs e)
        {
            DAL.AcountCheckTempTableAdapters.ZZZ_DEPTTableAdapter depAD = new DAL.AcountCheckTempTableAdapters.ZZZ_DEPTTableAdapter();
            DAL.AcountCheckTemp.ZZZ_DEPTDataTable depDT = new AcountCheckTemp.ZZZ_DEPTDataTable();

            if (accErrTable == null || accErrTable.Rows.Count <= 0)
            {
                MessageBox.Show("无任何需要处理的数据");
                return;
            }
            foreach (AcountCheckTemp.T_FB_BUDGETACCOUNTRow dr in accErrTable.Rows)
            {
                switch (dr.ACCOUNTOBJECTTYPE.ToString())
                {
                    case "1"://公司
                        break;
                    case "2"://部门
                        //查出流水
                        //实际额度
                        string str = @"select sum(zzz.BUDGETMONEY) from ZZZ_DEPT  zzz where zzz.OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                                    "' and zzz.SUBJECTID='" + dr.SUBJECTID + "' and zzz.OWNERDEPARTMENTID='" + dr.OWNERDEPARTMENTID + "'"
                                    + "and zzz.CHECKSTATESNAME='审核通过'";
                        object Actualobj = dal.ExecuteCustomerSql(str);
                        //可用额度
                        string str2 = @"select sum(zzz.BUDGETMONEY) from ZZZ_DEPT  zzz where zzz.OWNERCOMPANYID='" + dr.OWNERCOMPANYID +
                                    "' and zzz.SUBJECTID='" + dr.SUBJECTID + "' and zzz.OWNERDEPARTMENTID='" + dr.OWNERDEPARTMENTID + "'"
                                    + "and zzz.CHECKSTATESNAME <> '审核中或未汇总'";
                        object Usableobj = dal.ExecuteCustomerSql(str);

                        decimal Actualmoney = 0;
                        if (Actualobj != null)
                        {
                            decimal.TryParse(Actualobj.ToString(), out Actualmoney);
                        }
                        decimal Usablemoney = 0;
                        if (Usableobj != null)
                        {
                            decimal.TryParse(Usableobj.ToString(), out Usablemoney);
                        }

                        string updateid = dr.BUDGETACCOUNTID;
                        string strUpdate = "update budgetaccount_temp20111021 t set t.actualmoney=" + Actualmoney
                            + ", t.usablemoney=" + Usablemoney + " where t.budgetaccountid='" + updateid + "'";
                        object falg = dal.ExecuteCustomerSql(strUpdate);

                        break;
                    case "3"://个人
                        break;
                }
            }

            MessageBox.Show("数据完毕");

        }

        private void btnSelectErrData_Click(object sender, EventArgs e)
        {
            AcountCheckTemp.T_FB_BUDGETACCOUNTDataTable dtAcount = new AcountCheckTemp.T_FB_BUDGETACCOUNTDataTable();
            DAL.AcountCheckTempTableAdapters.T_FB_BUDGETACCOUNTTableAdapter Adacount = new DAL.AcountCheckTempTableAdapters.T_FB_BUDGETACCOUNTTableAdapter();
            Adacount.Fill(dtAcount);
            //AcountCheckTemp.ACCOUNTDDataTable dtAcount = new AcountCheckTemp.ACCOUNTDDataTable();
            //if (dtAcount.Rows.Count <= 0)
            //{
            //    DAL.AcountCheckTempTableAdapters.ACCOUNTDTableAdapter AdacountAd = new DAL.AcountCheckTempTableAdapters.ACCOUNTDTableAdapter();
            //    AdacountAd.Fill(dtAcount);
            //}
//         
            
            string strCompanys = string.Empty; 
            for (int i = 0; i < dataGridCompany.Rows.Count; i++)
            {
                if (dataGridCompany.Rows[i].Cells["CheckColumn"].Value != null)
                {
                    if ((bool)dataGridCompany.Rows[i].Cells["CheckColumn"].Value)
                    {
                        strCompanys += "'" + dataGridCompany.Rows[i].Cells[2].Value.ToString() + "'"+ ",";
                    }
                }
            }
            string strFilter = "USABLEMONEY < 0";
            if(!string.IsNullOrEmpty(strCompanys))
            {
                strCompanys=strCompanys.Substring(0, strCompanys.Length - 1);
                strFilter += "and ACCOUNTOBJECTTYPE = "+intAccountType+" and OWNERCOMPANYID in(" + strCompanys + ")";
            }
            //DataRow[] rows= dtAcount.Select(strFilter);
            dataGridErrAcData.DataSource = dtAcount.Select(strFilter);
        }

        private void btnCompany_Click(object sender, EventArgs e)
        {
            try
            {
                int i = cmbAcountType.SelectedIndex;

                if (i <= 0) return;
                this.intAccountType = i;
                DataTable dt = new DataTable();
                string str = "select c.cname,c.companyid from smthrm.t_hr_company c  order by c.cname ";
                dt = (DataTable)dal.GetDataTable(str);
                dataGridCompany.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }






        private void btnExport_Click(object sender, EventArgs e)
        {
            DataTable outputdt = new DataTable();
            for (int rowindex = 0; rowindex < dataGridErrAcData.Rows.Count; rowindex++)
            {
                string strLiuShui = string.Empty;
                try
                {
                    switch (intAccountType)
                    {
                        case 1:
                            strLiuShui = @"公司级别 暂无";
                            break;
                        case 2://部门
                            strLiuShui = @"select 
                                    t.SUBJECTNAME 科目名称,
                                    t.CHARGETYPENAME 费用类型,
                                    t.BUDGETARYMONTH 预算年月,
                                    t.ORDERTYPENAME 单据类型,
                                    t.UPDATEDATE 终审日期,
                                    t.USABLEMONEY 可用额度,
                                    t.BUDGETMONEY 操作金额,
                                    t.CHECKSTATESNAME 审核状态,
                                    t.OWNERNAME 单据所属人,
                                    t.OWNERPOSTNAME 所属人岗位,
                                    t.OWNERDEPARTMENTNAME 所属部门,
                                    t.OWNERCOMPANYNAME 所属公司,
                                    t.ORDERCODE 单据编号,
                                    t.CREATEDATE 创建日期 from zzz_dept t
                                where t.OWNERCOMPANYID ='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERCOMPANYID"].Value.ToString()
                                     + "' and t.OWNERDEPARTMENTID='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERDEPARTMENTID"].Value.ToString()
                                     + "' and t.SUBJECTID='" + dataGridErrAcData.Rows[rowindex].Cells["SUBJECTID"].Value.ToString() + "'";
                            break;
                        case 3://个人
                            strLiuShui = @"select 
                                    t.SUBJECTNAME 科目名称,
                                    t.CHARGETYPENAME 费用类型,
                                    t.BUDGETARYMONTH 预算年月,
                                    t.ORDERTYPENAME 单据类型,
                                    t.UPDATEDATE 终审日期,
                                    t.USABLEMONEY 可用额度,
                                    t.BUDGETMONEY 操作金额,
                                    t.CHECKSTATESNAME 审核状态,
                                    t.OWNERNAME 单据所属人,
                                    t.OWNERPOSTNAME 所属人岗位,
                                    t.OWNERDEPARTMENTNAME 所属部门,
                                    t.OWNERCOMPANYNAME 所属公司,
                                    t.ORDERCODE 单据编号,
                                    t.CREATEDATE 创建日期 from ZZZ_PERSON t
                                where t.OWNERCOMPANYID ='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERCOMPANYID"].Value.ToString()
                                  + "' and t.OWNERID='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERID"].Value.ToString()
                                  + "' and t.SUBJECTID='" + dataGridErrAcData.Rows[rowindex].Cells["SUBJECTID"].Value.ToString() + "'";
                            break;
                    }
                    outputdt = (DataTable)dal.GetDataTable(strLiuShui);
                    dataGridSequence.DataSource = outputdt;
                    //设置文件名
                    if (outputdt.Rows.Count == 0) continue;
                    string title = outputdt.Rows[0]["所属公司"].ToString()
                        + outputdt.Rows[0]["所属部门"].ToString()
                        + outputdt.Rows[0]["科目名称"].ToString() + "流水.xls";
                    byte[] byExport = Utility.OutFileStream(title, outputdt);
                    if (byExport == null)
                    {
                        return;
                    }
                    string strPath = @"d:\OutPutFile\" + title;
                    using (FileStream fs = new FileStream(strPath, FileMode.OpenOrCreate))
                    {
                        fs.Write(byExport, 0, byExport.Length);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            MessageBox.Show("处理完毕");
        }

        #region DataGrid事件
        private void dataGridErrAcData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowindex = e.RowIndex;
            ReFreshLiuShui(rowindex);

        }

        private void ReFreshLiuShui(int rowindex)
        {
            try
            {
                DataTable dt = new DataTable();
                string strLiuShui = string.Empty;
                if (intAccountType <= 0) return;
                switch (intAccountType)
                {
                    case 1:
                        strLiuShui = @"公司级别 暂无";
                        break;
                    case 2://部门
                        strLiuShui = @"select * from zzz_dept t
                                where t.OWNERCOMPANYID ='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERCOMPANYID"].Value.ToString()
                                 + "' and t.OWNERDEPARTMENTID='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERDEPARTMENTID"].Value.ToString()
                                 + "' and t.SUBJECTID='" + dataGridErrAcData.Rows[rowindex].Cells["SUBJECTID"].Value.ToString() + "'"
                                 + " order by t.Updatedate";
                        break;
                    case 3://个人
                        strLiuShui = @"select * from zzz_person t
                                where t.OWNERCOMPANYID ='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERCOMPANYID"].Value.ToString()
                              + "' and t.OWNERID='" + dataGridErrAcData.Rows[rowindex].Cells["OWNERID"].Value.ToString()
                              + "' and t.SUBJECTID='" + dataGridErrAcData.Rows[rowindex].Cells["SUBJECTID"].Value.ToString() + "'"
                              + " order by t.Updatedate";
                        break;
                }
                dt = (DataTable)dal.GetDataTable(strLiuShui);
                dataGridSequence.DataSource = dt;
                decimal money = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ORDERTYPENAME"].ToString() == "还款单"
                        && dr["CHECKSTATESNAME"].ToString() == "审核中")
                    {

                    }
                    else if (dr["CHECKSTATESNAME"].ToString() == "审核中或未汇总")
                    {
                    }
                    else
                    {
                        money += decimal.Parse(dr["BUDGETMONEY"].ToString());
                    }
                }
                txtMoneyReslut.Text = money.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void dataGridSequence_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int rowindex = e.RowIndex;
                int selectIndex = 0;


                switch (intAccountType)
                {
                    case 1:
                        break;
                    case 2://部门
                        for (int i = 0; i < dataGridErrAcData.Rows.Count; i++)
                        {
                            if (dataGridErrAcData.Rows[i].Cells["OWNERCOMPANYID"].Value.ToString() == dataGridSequence.Rows[rowindex].Cells["OWNERCOMPANYID"].Value.ToString()
                                && dataGridErrAcData.Rows[i].Cells["OWNERDEPARTMENTID"].Value.ToString() == dataGridSequence.Rows[rowindex].Cells["OWNERDEPARTMENTID"].Value.ToString()
                                && dataGridErrAcData.Rows[i].Cells["SUBJECTID"].Value.ToString() == dataGridSequence.Rows[rowindex].Cells["SUBJECTID"].Value.ToString())
                            {
                                selectIndex = i;
                                break;
                            }
                        }
                        break;
                    case 3://个人
                        for (int i = 0; i < dataGridErrAcData.Rows.Count; i++)
                        {
                            if (dataGridErrAcData.Rows[i].Cells["OWNERCOMPANYID"].Value.ToString() == dataGridSequence.Rows[rowindex].Cells["OWNERCOMPANYID"].Value.ToString()
                                && dataGridErrAcData.Rows[i].Cells["OWNERID"].Value.ToString() == dataGridSequence.Rows[rowindex].Cells["OWNERID"].Value.ToString()
                                && dataGridErrAcData.Rows[i].Cells["SUBJECTID"].Value.ToString() == dataGridSequence.Rows[rowindex].Cells["SUBJECTID"].Value.ToString())
                            {
                                selectIndex = i;
                                break;
                            }
                        }
                        break;
                }


             
                if (selectIndex != 0)
                {
                    this.dataGridErrAcData.CurrentCell = this.dataGridErrAcData.Rows[selectIndex].Cells[8];
                    //dataGridErrAcData.Rows[selectIndex].Selected = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        private void cmbAcountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            intAccountType = cmbAcountType.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //取流水最新数据
            string strFresh = @"select * from updatebudget_person";
            DataTable dt = dal.GetDataTable(strFresh);


            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    string updateid = dr["BUDGETACCOUNTID"].ToString();

                    string strFreshRecord = @"select ep.POSTID,d.DEPARTMENTID,c.COMPANYID from  smthrm.t_hr_employeepost ep  
                                        inner join smthrm.t_hr_post p on ep.postid = p.postid
                                         inner join smthrm.t_hr_department d on p.departmentid = d.departmentid
                                         inner join smthrm.t_hr_company c on d.companyid = c.companyid where ep.employeeid='" + dr["OWNERID"] + "'"
                                           + @"and  ep.editstate=1  and ep.checkstate=2 and ep.isagency=0";
                    DataTable dtFreshRecord = dal.GetDataTable(strFreshRecord);

                    string strUpdateFreshRow = "update t_fb_budgetaccount t set t.OWNERDEPARTMENTID='" + dtFreshRecord.Rows[0]["DEPARTMENTID"].ToString()
                          + "', t.OWNERPOSTID='" + dtFreshRecord.Rows[0]["POSTID"].ToString()
                     + "', t.OWNERCOMPANYID='" + dtFreshRecord.Rows[0]["COMPANYID"].ToString() + "' where t.budgetaccountid='" + updateid + "'";
                    object falgs = dal.ExecuteCustomerSql(strUpdateFreshRow);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.ToString());
                    string strMsg = "公司id："+dr["OWNERCOMPANYID"].ToString() +"员工id："+ dr["OWNERID"].ToString();
                    txtMsgUdOrg.Text +=strMsg+System.Environment.NewLine;
                    continue;
                }
            }
            
        }

        private void dataGridErrAcData_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int rowindex = e.RowIndex;
                ReFreshLiuShui(rowindex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btbSelect_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = new DataTable();
                string strLiuShui = string.Empty;
                if (intAccountType <= 0) return;
                switch (intAccountType)
                {
                    case 1:
                        strLiuShui = @"公司级别 暂无";
                        break;
                    case 2://部门
                        strLiuShui = @"select * from zzz_dept t
                                where t.OWNERCOMPANYNAME ='" +txtCompany.Text
                                 + "' and t.OWNERDEPARTMENTNAME='" +txtDepartment.Text
                                 + "' and t.SUBJECTNAME='" +txtSubject.Text 
                                 + "' order by t.Updatedate";
                        break;
                    case 3://个人
                        strLiuShui = @"select * from zzz_person t
                                where t.OWNERCOMPANYNAME ='" + txtCompany.Text
                              + "' and t.OWNERNAME='" + txtPerson.Text
                              + "' and t.SUBJECTNAME='" + txtSubject.Text 
                              + "' order by t.Updatedate";
                        break;
                }
                dt = (DataTable)dal.GetDataTable(strLiuShui);
                dataGridSequence.DataSource = dt;
                decimal money = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["ORDERTYPENAME"].ToString() == "还款单"
                        && dr["CHECKSTATESNAME"].ToString() == "审核中")
                    {

                    }
                    else if (dr["CHECKSTATESNAME"].ToString() == "审核中或未汇总")
                    {
                    }
                    else
                    {
                        money += decimal.Parse(dr["BUDGETMONEY"].ToString());
                    }
                }
                txtMoneyReslut.Text = money.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            Tracer.Debug(txtMsg.Text);
        }

        #region 活动经费调整
        private void btnCheckJF_Click(object sender, EventArgs e)
        {
            jinFeiId = "d5134466-c207-44f2-8a36-cf7b96d5851f";//活动经费
            Thread d = new Thread(starChechHDJF);
            d.Start();
           
        }

        public string jinFeiId = string.Empty;

        private void btnBMJF_Click(object sender, EventArgs e)
        {
            ReCheck_BMJF bmjf = new ReCheck_BMJF();
            bmjf.DepartMentId = "12bf769a-06d1-474d-a005-8f56f84a3cbe";
            bmjf.SubjectId="08c1d9c6-2396-43c3-99f9-227e4a7eb417";//部门经费
            bmjf.checkBMJF();
        }
        public bool ShowCheckProgreLog = true;

        /// <summary>
        /// 检查活动经费流水，调整额度
        /// </summary>
        private void starChechHDJF()
        {
            try
            {
                Tracer.Debug("结算开始");
                using (SMT_FB_EFModelContext context = new SMT_FB_EFModelContext())
                {
                    DateTime thisYear = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    SetLog("开始获取总账数据，请稍等......");
                    List<T_FB_BUDGETACCOUNT> allItem = new List<T_FB_BUDGETACCOUNT>();
                    List<T_FB_PERSONMONEYASSIGNMASTER> T_FB_PERSONMONEYASSIGNMASTERList = new List<T_FB_PERSONMONEYASSIGNMASTER>();
                    List<T_FB_PERSONMONEYASSIGNDETAIL> T_FB_PERSONMONEYASSIGNDETAILList = new List<T_FB_PERSONMONEYASSIGNDETAIL>();
                    List<T_FB_CHARGEAPPLYMASTER> T_FB_CHARGEAPPLYMASTERList = new List<T_FB_CHARGEAPPLYMASTER>();
                    List<T_FB_CHARGEAPPLYDETAIL> T_FB_CHARGEAPPLYDETAILList = new List<T_FB_CHARGEAPPLYDETAIL>();
                    if (allItem.Count < 1)
                    {

                        List<string> employeeALl = (from ent in context.T_FB_BUDGETACCOUNT.Include("T_FB_SUBJECT")
                                                    where ent.T_FB_SUBJECT.SUBJECTID == "d5134466-c207-44f2-8a36-cf7b96d5851f"//活动经费
                                                    && ent.ACCOUNTOBJECTTYPE == 3
                                                    select ent.OWNERID).Distinct().ToList();

                        if (!string.IsNullOrEmpty(txtEmployee.Text))
                        {
                            employeeALl.Clear();
                            employeeALl.Add(txtEmployee.Text);

                        }
                        int allCount = employeeALl.Count;
                        int i = 0;
                        string msg = string.Empty;

                        //经费下拨额度
                        T_FB_PERSONMONEYASSIGNMASTERList
                            = (from ent in context.T_FB_PERSONMONEYASSIGNMASTER.Include("T_FB_PERSONMONEYASSIGNDETAIL")
                               where ent.CHECKSTATES == 2
                               select ent).ToList();

                        foreach (var employeeid in employeeALl)
                        {
                            i++;
                            var q = from ent in context.T_FB_BUDGETACCOUNT.Include("T_FB_SUBJECT")
                                    where ent.T_FB_SUBJECT.SUBJECTID == "d5134466-c207-44f2-8a36-cf7b96d5851f"//活动经费
                                    && ent.ACCOUNTOBJECTTYPE == 3
                                    && ent.OWNERID == employeeid
                                    select ent;
                            allItem = q.ToList();
                            if (allItem.Count > 1)
                            {
                                //MessageBox.Show("活动经费总账大于1，请手动处理，可能是异动导致");
                                foreach (var itm in allItem)
                                {
                                    SetLog("活动经费总账多条：id:" + itm.BUDGETACCOUNTID+" ownerid="+itm.OWNERID+" ownerPostId="+itm.OWNERPOSTID);
                                }
                                continue;
                            }
                            SetLog("获取总账数据完毕,开始获取下拨经费");
                            T_FB_PERSONMONEYASSIGNDETAILList
                                = (from ent in context.T_FB_PERSONMONEYASSIGNDETAIL.Include("T_FB_PERSONMONEYASSIGNMASTER").Include("T_FB_SUBJECT")
                                   where ent.T_FB_PERSONMONEYASSIGNMASTER.CHECKSTATES == 2
                                   && ent.OWNERID == employeeid
                                   select ent).ToList();
                            SetLog("获取个人预算申请及增补完毕，开始获取费用报销");
                            //个人费用报销
                            T_FB_CHARGEAPPLYMASTERList =
                                (from ent in context.T_FB_CHARGEAPPLYMASTER
                                 join b in context.T_FB_CHARGEAPPLYDETAIL on ent.CHARGEAPPLYMASTERID equals b.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID
                                 where ent.CHECKSTATES == 2 && b.T_FB_SUBJECT.SUBJECTID == "d5134466-c207-44f2-8a36-cf7b96d5851f"
                                 && ent.OWNERID == employeeid
                                 select ent).ToList();
                            T_FB_CHARGEAPPLYDETAILList =
                                (from ent in context.T_FB_CHARGEAPPLYDETAIL.Include("T_FB_CHARGEAPPLYMASTER").Include("T_FB_SUBJECT")
                                 join b in context.T_FB_CHARGEAPPLYMASTER on ent.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID equals b.CHARGEAPPLYMASTERID
                                 where ent.T_FB_SUBJECT.SUBJECTID == "d5134466-c207-44f2-8a36-cf7b96d5851f" && b.CHECKSTATES == 2
                                 && ent.T_FB_CHARGEAPPLYMASTER.OWNERID == employeeid
                                 select ent).ToList();

                            SetLog("获取费用报销完毕，开始处理......");
                           var item = allItem.FirstOrDefault();
                            
                                string id = item.T_FB_SUBJECT.SUBJECTID;
                                decimal A = 0;//年度预算
                                decimal B = 0;//年度增补
                                decimal C = 0;//部门月度预算=公共+个人
                                decimal D = 0;//部门月度增补=公共+个人
                                decimal C1 = 0;//月度预算-部门公共
                                decimal D1 = 0;//月度增补-部门公共
                                decimal C2 = 0;//月度预算-个人
                                decimal D2 = 0;//月度增补-个人
                                decimal E = 0;//个人费用报销已终审-部门公共
                                decimal F = 0;//个人费用报销审核中-部门公共
                                decimal G = 0;//个人费用报销已终审-个人
                                decimal H = 0;//个人费用报销审核中-个人
                                string ownerName = string.Empty;
                                GetJFABCD(T_FB_PERSONMONEYASSIGNMASTERList,
                                    T_FB_PERSONMONEYASSIGNDETAILList,
                                    T_FB_CHARGEAPPLYMASTERList,
                                    T_FB_CHARGEAPPLYDETAILList,
                                    item,
                                    ref A, ref B, ref C,
                                    ref C1, ref D1,
                                    ref C2, ref D2,
                                    ref D, ref E,
                                    ref F, ref G,
                                    ref H,
                                    ref ownerName);
                                msg = "总 " + allCount + " 第 " + i + " 条:";

                                decimal USABLEMONEY = 0;
                                if (item.USABLEMONEY != null)
                                {
                                    USABLEMONEY = item.USABLEMONEY.Value;
                                }
                                decimal ACTUALMONEY = 0;
                                if (item.ACTUALMONEY != null)
                                {
                                    ACTUALMONEY = item.ACTUALMONEY.Value;
                                }
                                //decimal PAIEDMONEY = item.PAIEDMONEY.Value;
                                switch (item.ACCOUNTOBJECTTYPE.Value.ToString())
                                {
                                    case "3"://部门月度预算--个人月度预算
                                        //item.BUDGETMONEY = C2 + D2;//预算金额
                                        item.USABLEMONEY = C2 - E - F;//可用额度
                                        item.ACTUALMONEY = C2 - E;//实际额度
                                        //item.PAIEDMONEY = G;//已用额度
                                        break;
                                }


                                if (item.USABLEMONEY == null
                                    || USABLEMONEY != item.USABLEMONEY.Value)
                                {
                                    if (item.USABLEMONEY == null)
                                    {
                                        msg += " 总账id" + item.BUDGETACCOUNTID + " USABLEMONEY==Null";
                                    }
                                    if (USABLEMONEY != item.USABLEMONEY.Value)
                                    {
                                        
                                        msg += " 总账id" + item.BUDGETACCOUNTID + " USABLEMONEY 不对，员工：" + ownerName
                                              + " 科目：" + item.T_FB_SUBJECT.SUBJECTNAME
                                              + " 修改前 USABLEMONEY:" + USABLEMONEY
                                              + " 修改后 USABLEMONEY    ->    " + item.USABLEMONEY.Value
                                            + " C2:" + C2
                                            + " E:" + E
                                            + " F:" + F;
                                        if (USABLEMONEY > item.USABLEMONEY.Value)
                                        {
                                            msg +=System.Environment.NewLine+ "总账金额大于实际金额，可能导致报超，请重点关注---------------------------------->";
                                        }

                                    }
                                }
                                //if (item.ACTUALMONEY==null || ACTUALMONEY != item.ACTUALMONEY.Value)
                                //{
                                //    if (item.ACTUALMONEY == null)
                                //    {
                                //        msg += " 总账id" + item.BUDGETACCOUNTID + " ACTUALMONEY==Null";
                                //    }
                                //    if (ACTUALMONEY != item.ACTUALMONEY.Value)
                                //    {
                                //        msg += " 总账id" + item.BUDGETACCOUNTID +" ACTUALMONEY 不对，员工：" + ownerName
                                //              + " 预算类型：" + item.ACCOUNTOBJECTTYPE.Value.ToString()
                                //              + " 科目：" + item.T_FB_SUBJECT.SUBJECTNAME
                                //              + " 修改前 ACTUALMONEY:" + ACTUALMONEY
                                //              + " 修改后 ACTUALMONEY    ->    " + item.ACTUALMONEY.Value
                                //            + " C2:" + C2
                                //            + " E:" + E
                                //            + " F:" + F;
                                //    }
                                //}   
                                SetLog(msg); 
                        }
                        i = 0;
                        if (msg.Contains("不对"))
                        {
                            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
                            DialogResult dr = MessageBox.Show("确定要更新异常的数据吗?", "确认", messButton);
                            if (dr == DialogResult.OK)//如果点击“确定”按钮
                            {
                                int j = context.SaveChanges();
                                MessageBox.Show("处理完毕,共处理了: " + j + " 条数据");
                            }
                        }
                        SetLog("处理完毕！");
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("异常：" + ex.ToString());
                SetLog(ex.ToString());
            }
        }

        private void GetJFABCD(
            List<T_FB_PERSONMONEYASSIGNMASTER> T_FB_PERSONMONEYASSIGNMASTERList,
            List<T_FB_PERSONMONEYASSIGNDETAIL> T_FB_PERSONMONEYASSIGNDETAILList,

            List<T_FB_CHARGEAPPLYMASTER> T_FB_CHARGEAPPLYMASTERList,
           List<T_FB_CHARGEAPPLYDETAIL> T_FB_CHARGEAPPLYDETAILList,

           T_FB_BUDGETACCOUNT item,
           ref decimal A, ref decimal B,
           ref decimal C,
           ref decimal C1, ref decimal D1,
           ref decimal C2, ref decimal D2,
           ref decimal D, ref decimal E,
           ref decimal F, ref decimal H,
           ref decimal G,
            ref string OwnerName)
        {

            #region "活动经费"
            if (item.T_FB_SUBJECT.SUBJECTID == "d5134466-c207-44f2-8a36-cf7b96d5851f")
            {
                var YearMoney = (from a in T_FB_PERSONMONEYASSIGNMASTERList
                                join b in T_FB_PERSONMONEYASSIGNDETAILList
                             on a.PERSONMONEYASSIGNMASTERID equals b.T_FB_PERSONMONEYASSIGNMASTER.PERSONMONEYASSIGNMASTERID
                                where b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                && b.OWNERID == item.OWNERID
                                && a.CHECKSTATES == 2
                                orderby a.UPDATEDATE
                                select new
                                {
                                    a.PERSONMONEYASSIGNMASTERCODE,
                                    b.PERSONBUDGETAPPLYDETAILID,
                                    b.BUDGETMONEY,
                                    a.UPDATEDATE,
                                    OwnerName=b.OWNERCOMPANYNAME+"-"+b.OWNERNAME
                                }).ToList().Distinct();
                if (YearMoney.Count() > 0)
                {
                    foreach (var va in YearMoney)
                    {
                        OwnerName = va.OwnerName;
                        if (OwnerName.Contains("田少林"))
                        {

                        }
                        C2 = C2 + va.BUDGETMONEY.Value;
                        if(ShowCheckProgreLog)
                        {
                            SetLog("单号:"+va.PERSONMONEYASSIGNMASTERCODE+",员工：" + va.OwnerName + "日期：" + va.UPDATEDATE + " 下拨经费额度：," + va.BUDGETMONEY);
                        }
                    }
                }

            }
            #endregion


            #region//个人费用部门科目报销已终审E
            if (T_FB_CHARGEAPPLYDETAILList != null)
            {
                var ChargeMoenyChecked = (from a in T_FB_CHARGEAPPLYMASTERList
                                         join b in T_FB_CHARGEAPPLYDETAILList
                                    on a.CHARGEAPPLYMASTERID equals b.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID
                                         where b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                         && a.OWNERID == item.OWNERID
                                         && a.CHECKSTATES == 2
                                         orderby a.UPDATEDATE
                                         select new {a.CHARGEAPPLYMASTERCODE,a.OWNERNAME,a.UPDATEDATE, b.CHARGEAPPLYDETAILID, b.CHARGEMONEY }).ToList().Distinct();
                if (ChargeMoenyChecked.Count() > 0)
                {
                    foreach (var va in ChargeMoenyChecked)
                    {
                        E = E + va.CHARGEMONEY;
                        if (ShowCheckProgreLog)
                        {
                            SetLog("单号:" + va.CHARGEAPPLYMASTERCODE + ",员工：" + va.OWNERNAME + "日期：" + va.UPDATEDATE + " 报销活动经费额度：,-" + va.CHARGEMONEY);
                        }
                    }
                }
            }
            #endregion

            #region//个人费用部门科目报销终审中F
            if (T_FB_CHARGEAPPLYDETAILList != null)
            {
                var ChargeingMoeny = (from a in T_FB_CHARGEAPPLYMASTERList
                                     join b in T_FB_CHARGEAPPLYDETAILList
                                  on a.CHARGEAPPLYMASTERID equals b.T_FB_CHARGEAPPLYMASTER.CHARGEAPPLYMASTERID
                                     where b.T_FB_SUBJECT.SUBJECTID == item.T_FB_SUBJECT.SUBJECTID
                                     && a.OWNERID == item.OWNERID
                                     && a.CHECKSTATES == 1
                                     orderby a.UPDATEDATE
                                     select  new {a.CHARGEAPPLYMASTERCODE,a.OWNERNAME,a.UPDATEDATE, b.CHARGEAPPLYDETAILID, b.CHARGEMONEY }).ToList().Distinct();
                if (ChargeingMoeny.Count() > 0)
                {
                    foreach (var va in ChargeingMoeny)
                    {
                        F = F + va.CHARGEMONEY;
                        if (ShowCheckProgreLog)
                        {
                            SetLog("单号:" + va.CHARGEAPPLYMASTERCODE + ",员工：" + va.OWNERNAME + "日期：" + va.UPDATEDATE + " 报销中活动经费额度：,-" + va.CHARGEMONEY);
                        }
                    }
                }
            }
            #endregion
        }

        #endregion

        private void btnGetJF_Click(object sender, EventArgs e)
        {
            string sql=@"select employeeid from smthrm.t_hr_employee 
                        where employeecname='"+txtEmployeeName.Text+"'";
            DataTable dt = dal.GetDataTable(sql) as DataTable;
            if(dt.Rows.Count>0)
            {
                txtEmployee.Text = dt.Rows[0][0].ToString();
            }
        }

        private void btnYUsuan_Click(object sender, EventArgs e)
        {
            FBTrans tran = new FBTrans();
            tran.start();
        }
    }
}
