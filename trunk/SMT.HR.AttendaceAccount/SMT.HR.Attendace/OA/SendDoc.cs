using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using AttendaceAccount;
using AttendaceAccount.AttendWS;
using AttendaceAccount.OACommonOfficeWS;
using System.Collections.ObjectModel;

namespace SmtPortalSetUp
{
    public partial class SendDocForm : Form
    {

        private DataTable dtEmployee = new DataTable();
        public SendDocForm()
        {
            InitializeComponent();
            GlobalParameters.sendDocForm = this;
            this.TxtSendDocid.Text = GlobalParameters.employeeid;
            txtCompanyid.Text = GlobalParameters.employeeMasterCompanyid;
            txtStartDate.Text = GlobalParameters.StartDate;
            //txtEndDate.Text = Employee.EndDate;
            txtDocName.Text = GlobalParameters.employeeName;
        }

        delegate void DelShow(String Msg); //代理
        //将对控件的操作写到一个函数中 
        public void ShowMessage(String para)
        {
            if (!txtMessagebox.InvokeRequired) //不需要唤醒，就是创建控件的线程
            //如果是创建控件的线程，直接正常操作 
            {
                txtMessagebox.Text = DateTime.Now.ToLongTimeString() +" "+ para + System.Environment.NewLine + txtMessagebox.Text;
            }
            else //非创建线程，用代理进行操作
            {
                DelShow ds = new DelShow(ShowMessage);
                //唤醒主线程，可以传递参数，也可以为null，即不传参数
                Invoke(ds, new object[] { para });
            }
        }

        private void txtMessagebox_DoubleClick(object sender, EventArgs e)
        {
            txtMessagebox.SelectAll();

        }


        private void GetCompanyDoc()
        {
            string sql = @"select t.senddocid,t.senddoctitle,t.* 
                            from smtoa.t_oa_senddoc t
                            where t.senddoctitle like '%" + txtDocName.Text+"%'";
            OracleHelp.Connect();
            DataTable dt = OracleHelp.getTable(sql);
            if (dt != null)
            {

                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows.Count > 1)
                    {
                        MessageBox.Show("存在多条记录，请检查查询条件");
                        return;
                    }
                    TxtSendDocid.Text = dt.Rows[0]["senddocid"].ToString();
                    txtDocName.Text = dt.Rows[0]["senddoctitle"].ToString();
                }

                string sqldoc = @"select c.cname,c.companyid,d.* 
                        from smtoa.T_OA_DISTRIBUTEUSER d
                        inner join smthrm.t_hr_company c on d.viewer=c.companyid
                        where d.formid='" + TxtSendDocid.Text + "'";

                DataTable dtdoc = OracleHelp.getTable(sqldoc);
                if (dtdoc.Rows.Count > 0)
                {
                    dataGridSendDoc.DataSource = dtdoc;
                }
                else
                {
                    MessageBox.Show("没找到公文发布范围。");
                }

                OracleHelp.close();
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：" + dt.Rows.Count.ToString() + "条数据";
            }
            else
            {
                txtMessagebox.Text += System.Environment.NewLine + "查询完成，共：0条数据";
            }
        }

        private void btnGetEmployeeid_Click(object sender, EventArgs e)
        {
//            string sql = @"select t.employeeid,t.employeecname from smthrm.t_hr_employee t
//                    where t.employeecname='"+txtDocName.Text+"'";
//            OracleHelp.Connect();
//            DataTable dt = OracleHelp.getTable(sql);
//            dataGridSendDoc.DataSource = dt;
//            OracleHelp.close();
//            TxtSendDocid.Text = dt.Rows[0][0].ToString();
        }



        private void btnPrevious_Click(object sender, EventArgs e)
        {
            Form_Second form = GlobalParameters.fromSecond;
            form.Show();
            this.Hide();
        }

        private void AttendEmploeeBalance_Load(object sender, EventArgs e)
        {
            //GetEmployeeBlance();
        }


        private void btnGenerateSalary_Click(object sender, EventArgs e)
        {
            //SalaryBalanceForm form = GlobalParameters.salaryBalanceForm;
            //if (form == null) form = new SalaryBalanceForm();
            //form.Show();
            //this.Hide();
        }

        private void AttendEmploeeBalance_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void dataGridSendDoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = dataGridSendDoc.Columns[e.ColumnIndex];

                if (column is DataGridViewButtonColumn)
                {
                    if (column.Name == "ColumnSendDoc")
                    {
                        //这里可以编写你需要的任意关于按钮事件的操作~
                    

                      string selectValue = dataGridSendDoc.Rows[e.RowIndex].Cells["ColumnSelect"].EditedFormattedValue.ToString();
                         if (selectValue == "True")
                         {
                             string strCompanyid = dataGridSendDoc.Rows[e.RowIndex].Cells["companyid"].EditedFormattedValue.ToString();
                             SendDocByCompanyid(strCompanyid);
                         }
                    }
                }
            }
        }
        private void SendDocByCompanyid(string companyid)
        {
            Dictionary<string, string> dicCompany=new Dictionary<string,string>();
            string strSunCompany = @"select c.companyid,c.cname from smthrm.t_hr_company c
            where c.fatherid='" + companyid + "'";
            OracleHelp.Connect();
            DataTable dtsun = OracleHelp.getTable(strSunCompany);
            if (dtsun != null)
            {
                if (dtsun.Rows.Count > 0)
                {
                    for (int i = 0; i < dtsun.Rows.Count; i++)
                    {
                        dicCompany.Add(dtsun.Rows[i]["cname"].ToString(), dtsun.Rows[i]["companyid"].ToString());
                        string strGrandSunCompany = @"select c.companyid,c.cname from smthrm.t_hr_company c
                            where c.fatherid='" + dtsun.Rows[i]["companyid"].ToString() + "'";

                        DataTable dtGrandSun = OracleHelp.getTable(strGrandSunCompany);
                        if (dtGrandSun != null)
                        {
                            if (dtGrandSun.Rows.Count > 0)
                            {
                                for (int j = 0; j < dtGrandSun.Rows.Count; j++)
                                {
                                    dicCompany.Add(dtGrandSun.Rows[j]["cname"].ToString(), dtGrandSun.Rows[j]["companyid"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            OracleHelp.close();
            if (string.IsNullOrEmpty(TxtSendDocid.Text))
            {
                MessageBox.Show("请先查询公文id");
                return;
            }
            string msg = string.Empty;
            ObservableCollection<T_OA_DISTRIBUTEUSER> distributeAddList = new ObservableCollection<T_OA_DISTRIBUTEUSER>() ;
            SmtOACommonOfficeClient DocDistrbuteClient = new SmtOACommonOfficeClient();
            ObservableCollection<string> CompanyIDsList = new ObservableCollection<string>(); 
            ObservableCollection<string> StrDepartmentIDsList = new ObservableCollection<string>();  //获取部门ID
            ObservableCollection<string> StrPositionIDsList = new ObservableCollection<string>();
            ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID


            if (dicCompany.Count > 0)
            {
                foreach (var q in dicCompany)
                {
                    //if (q.Key == "杭州新神州通货物运输有限公司") continue;
                    ShowMessage("发布公文给这个公司："+q.Key+" 公司id："+q.Value);
                    CompanyIDsList.Add(q.Value);

                    T_OA_DISTRIBUTEUSER distributeTmp = new T_OA_DISTRIBUTEUSER();
                    distributeTmp.DISTRIBUTEUSERID = Guid.NewGuid().ToString();
                    distributeTmp.MODELNAME = "CompanySendDoc";
                    distributeTmp.FORMID = TxtSendDocid.Text;
                    distributeTmp.VIEWTYPE = "0";//公司
                    distributeTmp.VIEWER = q.Value;

                    distributeTmp.CREATEDATE = DateTime.Now;
                    distributeTmp.CREATEUSERID =GlobalParameters.employeeid;
                    distributeTmp.CREATEUSERNAME = GlobalParameters.employeeName;
                    distributeTmp.CREATEPOSTID = GlobalParameters.employeeMasterPostid;
                    distributeTmp.CREATEDEPARTMENTID = GlobalParameters.employeeMasterDepartmentid;
                    distributeTmp.CREATECOMPANYID = GlobalParameters.employeeMasterCompanyid;
                    distributeTmp.OWNERID =GlobalParameters.employeeid;
                    distributeTmp.OWNERNAME = GlobalParameters.employeeName;
                    distributeTmp.OWNERPOSTID = GlobalParameters.employeeMasterPostid;
                    distributeTmp.OWNERDEPARTMENTID = GlobalParameters.employeeMasterDepartmentid;
                    distributeTmp.OWNERCOMPANYID = GlobalParameters.employeeMasterCompanyid;
                    distributeAddList.Add(distributeTmp);
                }
                ShowMessage("发布公文给所有公司完毕，共发送：" + dicCompany.Count+ " 条数据");
            }
            V_BumfCompanySendDoc tmpSenddoc=DocDistrbuteClient.GetBumfDocInfo(TxtSendDocid.Text);
            if (tmpSenddoc.OACompanySendDoc == null)
            {
                MessageBox.Show("通过id获取的公文为空，请检查后再试");
                return;
            }
            T_OA_SENDDOC sendDoc = tmpSenddoc.OACompanySendDoc;
            sendDoc.T_OA_SENDDOCTYPE = null;
            sendDoc.T_OA_SENDDOCTYPEReference = null;

            DocDistrbuteClient.BatchAddCompanyDocDistrbuteForNew(distributeAddList.ToArray(), CompanyIDsList.ToArray(), StrDepartmentIDsList.ToArray(), StrPositionIDsList.ToArray(), StrStaffList.ToArray(), sendDoc);
            //MessageBox.Show("发布公文成功！");
        }

        private void btnSearchDoc_Click(object sender, EventArgs e)
        {
            GetCompanyDoc();
        }

        private void btnSendAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridSendDoc.RowCount; i++)
            {
                string selectValue = dataGridSendDoc.Rows[i].Cells["ColumnSelect"].EditedFormattedValue.ToString();
                //if (selectValue == "True")
                //{
                    string strCompanyid = dataGridSendDoc.Rows[i].Cells["companyid"].EditedFormattedValue.ToString();
                    SendDocByCompanyid(strCompanyid);
                //}
            }
            MessageBox.Show("发送完毕");
        }

    }

    
}
