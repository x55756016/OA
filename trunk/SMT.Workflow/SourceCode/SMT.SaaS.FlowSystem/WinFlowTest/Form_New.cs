using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OracleClient;
using System.Xml;
using System.IO;
using WinFlowTest.FlowService;

namespace WinFlowTest
{
    public partial class Form_New : Form
    {
        public Form_New()
        {
            InitializeComponent();
            Open();           
        }
        public FlowService.SubmitData GetSubmitData()
        {
            FlowService.SubmitData submitData = new FlowService.SubmitData();
            submitData.FormID = "81501679-14f3-4ec7-a1f9-107276bf80ac";
            submitData.ModelCode = "T_OA_APPROVALINFO";
            submitData.ApprovalUser = new UserInfo();
            submitData.ApprovalUser.CompanyID = "cea137a0-3706-4267-8baa-a847536f1f48";
            submitData.ApprovalUser.DepartmentID = "c18d20e3-9f94-4b4c-ac24-b8256132e7d2";
            submitData.ApprovalUser.PostID = "d798ead2-559b-488c-ae76-b7640afff8e7";
            submitData.ApprovalUser.UserID = "8f4df3c7-1e41-488d-acb7-42c3dd773200";
            submitData.ApprovalUser.UserName = "英雄";
            submitData.NextStateCode = "EndFlow";
            submitData.NextApprovalUser = new UserInfo();
            submitData.NextApprovalUser.CompanyID = "";
            submitData.NextApprovalUser.DepartmentID = "";
            submitData.NextApprovalUser.PostID = "";
            submitData.NextApprovalUser.UserID = "";
            submitData.NextApprovalUser.UserName = "";
            submitData.SubmitFlag = SubmitFlag.Approval;
            submitData.FlowType = FlowType.Approval;
            submitData.ApprovalResult = ApprovalResult.Pass;
            submitData.ApprovalContent = "";
            submitData.SumbitCompanyID = "cea137a0-3706-4267-8baa-a847536f1f48";
            submitData.SumbitDeparmentID = "c18d20e3-9f94-4b4c-ac24-b8256132e7d2";
            submitData.SumbitPostID = "d798ead2-559b-488c-ae76-b7640afff8e7";
            submitData.SumbitUserID = "8f4df3c7-1e41-488d-acb7-42c3dd773200";
            submitData.SumbitUserName = "英雄";
            //submitData.SumbitCompanyID = "939b5bc5-6c17-45e8-9456-b09b406d9d5b";
            //submitData.SumbitDeparmentID = "486dacee-049d-4a1a-81e8-d0ff67a2958d";
            //submitData.SumbitPostID = "b9f4708e-ba77-4425-b11a-447aa7b4d6cc";
            //submitData.SumbitUserID = "c2c046f0-f7a2-4377-8ee8-2e747f1eff60";
            //submitData.SumbitUserName = "李琳琳";

            StringBuilder sb = new StringBuilder();
            

            //submitData.XML = sb.ToString();
           
            return submitData;
        }
        private void btn_Submit_Click(object sender, EventArgs e)
        {
            string name = checkPassOrNopass.Checked ? "确定审核不通过吗?" : "确定审核通过吗?";
            if (cbo_FlowCheck.Checked)
            {
                name = "确定新增吗?"; 
            }
            if (MessageBox.Show(name, "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //FlowService.ServiceClient test = new FlowService.ServiceClient();
                //var s=test.SubimtFlow(GetSubmitData());
                //MessageBox.Show(s.Err);
                //return;
                #region 提交
                FlowService.ServiceClient aa = new FlowService.ServiceClient();
               
                FlowService.SubmitData SubmitData = new FlowService.SubmitData();
                SubmitData.FlowSelectType = FlowService.FlowSelectType.FixedFlow;
                SubmitData.FormID = txtFormID.Text;
                SubmitData.ModelCode = txtModelID.Text;
                SubmitData.ApprovalUser = new FlowService.UserInfo();
                SubmitData.ApprovalUser.CompanyID = txtCompanyID.Text;

                SubmitData.ApprovalUser.DepartmentID = txtDepartID.Text;
                SubmitData.ApprovalUser.PostID = txtPostID.Text;
                SubmitData.ApprovalUser.UserID = txtUserID.Text;
                SubmitData.ApprovalUser.UserName = txtUserName.Text;
                //SubmitData.ApprovalContent = txtContent.Text.Trim();
                SubmitData.NextStateCode = "";

                SubmitData.NextApprovalUser = new FlowService.UserInfo();
                SubmitData.NextApprovalUser.CompanyID = "";
                SubmitData.NextApprovalUser.DepartmentID = "";
                SubmitData.NextApprovalUser.PostID = "";
                SubmitData.NextApprovalUser.UserID = "";
                SubmitData.NextApprovalUser.UserName = "";
                //  SubmitData.SubmitFlag = FlowService.SubmitFlag.New;
                SubmitData.SubmitFlag = cbo_FlowCheck.Checked ? FlowService.SubmitFlag.New : FlowService.SubmitFlag.Approval;

                //string str = txtXML.Text;
                //string strOut = " ";
                //for (int i = 0; i < str.Length; i += 6)
                //{
                //    strOut += (char)Convert.ToInt32(str.Substring(i + 2, 4), 16);
                //}

                SubmitData.XML = txtXML.Text;// String.Format(@txtXML.Text);
                SubmitData.FlowType = FlowService.FlowType.Approval;

                SubmitData.ApprovalResult = checkPassOrNopass.Checked ? FlowService.ApprovalResult.NoPass : FlowService.ApprovalResult.Pass;
                SubmitData.ApprovalContent = txtContent.Text.Trim();
                if (checkPassOrNopass.Checked && txtContent.Text.Trim() == "审核通过")
                {
                    MessageBox.Show("审核不通过时,审批意见不能是<审核通过>");
                    return;
                }
                #region 代码
                
                #endregion
                FlowService.DataResult cc;
                try
                {
                    cc = aa.SubimtFlow(SubmitData);
                    if (cc.FlowResult == FlowService.FlowResult.MULTIUSER)
                    {
                        SubmitData.NextApprovalUser = new FlowService.UserInfo();
                        SubmitData.NextApprovalUser.CompanyID = cc.UserInfo[1].CompanyID;
                        SubmitData.NextApprovalUser.DepartmentID = cc.UserInfo[1].DepartmentID;
                        SubmitData.NextApprovalUser.PostID = cc.UserInfo[1].PostID;
                        SubmitData.NextApprovalUser.UserID = cc.UserInfo[1].UserID;
                        SubmitData.NextApprovalUser.UserName = cc.UserInfo[1].UserName;
                        SubmitData.NextStateCode = cc.AppState;
                        cc = aa.SubimtFlow(SubmitData);
                    }
                    if (cc.Err != "")
                        MessageBox.Show(cc.Err + "!请重新获取审核信息,看审核是否正确!");
                    else
                    {
                        if (cc.UserInfo.Count() > 0)
                        {
                            MessageBox.Show(cc.FlowResult.ToString() + "下一处理人:" + cc.UserInfo[0].UserName);
                        }
                        else
                        {
                            MessageBox.Show(cc.FlowResult.ToString() + "!请重新获取审核信息,看审核是否正确!");
                        }
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
                #endregion
            }
           
        }
        PermissionService.PermissionServiceClient client = new PermissionService.PermissionServiceClient();
        PersonnelServiceWS.PersonnelServiceClient pclient = new PersonnelServiceWS.PersonnelServiceClient();
        #region 查找角色
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
               // WcfPermissionService.PermissionServiceClient client = new WcfPermissionService.PermissionServiceClient();
                PermissionService.FlowUserInfo[] User = client.GetFlowUserInfoByRoleID(txtRule.Text); //检索本状态（角色）对应用户
                //WcfPermissionService.T_SYS_USER[] role = client.GetSysRoleInfos(txtRule.Text); //检索本状态（角色）对应用户
                if (User != null && User.Length > 0)
                {
                    string name = "";
                    for (int i = 0; i < User.Length; i++)
                    {
                        name += "公司ID   = " + User[i].CompayID + "\r\n";
                        name += "部门ID   = " + User[i].DepartmentID + "\r\n";
                        name += "岗位ID   = " + User[i].PostID + "\r\n";
                        name += "员工ID   = " + User[i].UserID + "\r\n";

                        name += "公司名称 = " + User[i].CompayName + "\r\n";
                        name += "部门名称 = " + User[i].DepartmentName + "\r\n";                        
                        name += "岗位名称 = " + User[i].PostName + "\r\n";                       
                        name += "员工姓名 = " + User[i].EmployeeName + "\r\n";
                        name += "\r\n";
                        foreach (var role in User[i].Roles)
                        {
                            name += "角色ID   = " + role.ROLEID + "\r\n";
                            name += "角色名称 = " + role.ROLENAME + "\r\n";
                        }

                        name += "==================================================================================\r\n";

                    }
                    textBox1.Text = name;
                }
                else
                {
                    textBox1.Text = "没有找到相关信息!"; 
                }
            }          

            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        #endregion
        #region 查询HR信息
        private void button2_Click(object sender, EventArgs e)
        {


            try
            {
                PermissionService.FlowUserInfo[] User =null;
             
                if (radioButton1.Checked || radioButton2.Checked)
                {
                    User = client.GetSuperiorByPostID(txtRule.Text);  
                }
                else
                {
                    User = client.GetDepartmentHeadByDepartmentID(txtRule.Text); 
                }
               
                if (User != null && User.Length > 0)
                {
                    string name = "";
                    for (int i = 0; i < User.Length; i++)
                    {
                        name += "公司ID   = " + User[i].CompayID + "\r\n";
                        name += "部门ID   = " + User[i].DepartmentID + "\r\n";
                        name += "岗位ID   = " + User[i].PostID + "\r\n";
                        name += "员工ID   = " + User[i].UserID + "\r\n";

                        name += "公司名称 = " + User[i].CompayName + "\r\n";
                        name += "部门名称 = " + User[i].DepartmentName + "\r\n";                        
                        name += "岗位名称 = " + User[i].PostName + "\r\n";                       
                        name += "员工姓名 = " + User[i].EmployeeName + "\r\n";
                        name += "\r\n";
                        foreach (var role in User[i].Roles)
                        {
                            name += "角色ID   = " + role.ROLEID + "\r\n";
                            name += "角色名称 = " + role.ROLENAME + "\r\n";
                        }

                        name += "==================================================================================\r\n";

                    }
                    textBox1.Text = name;
                }
                else
                {
                    textBox1.Text = "没有找到相关信息!"; 
                }
            }

            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
        }
        #endregion
        #region 查询流程数据
        private void button3_Click(object sender, EventArgs e)
        {
           
            string sql = "select * from flow_flowdefine_t t  where t.flowcode in (select flowcode from flow_flowrecordmaster_t t where t.formid = '" + txtRule.Text.Trim() + "')";
            string sql2 = "select MODELCODE,DESCRIPTION  from flow_modeldefine_t t where t.modelcode in(select modelcode from flow_modelflowrelation_t t where t.flowcode in (select flowcode from flow_flowrecordmaster_t t where formid='" + txtRule.Text.Trim() + "'))";
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            OracleCommand cmd = con.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql.ToString();           
            OracleDataReader dr = cmd.ExecuteReader();
            cmd.CommandText = sql2.ToString();
            OracleDataReader dr2 = cmd.ExecuteReader();
            if (dr.Read())
            {
                label20.Text = "流程名称：" + dr["DESCRIPTION"].ToString() + ";    创建人：" + dr["CREATEUSERNAME"].ToString();
                textBox1.Text = FormatXml(dr["LAYOUT"].ToString());//代码
                
            }
            else
            {
                label20.Text = "流程名称：";
                textBox1.Text = "";//代码
            }
            if (dr2.Read())
            {
                label20.Text += "\r\n关联模块：" + dr2["DESCRIPTION"].ToString() + "(" + dr2["MODELCODE"].ToString() + ")";
            }
            else
            {
                label20.Text = "流程名称：";
                textBox1.Text = "没有找到相关信息!"; 
            }
            dr.Close();
            dr2.Close();
            #region 是否是自选流程
            if (textBox1.Text.Trim() == "")
            {
                string sql3 = "select * from flow_flowrecordmaster_t t where t.formid = '" + txtRule.Text.Trim() + "'";
                string sql4 = "select MODELCODE,DESCRIPTION  from flow_modeldefine_t t where t.modelcode in(select modelcode from flow_flowrecordmaster_t t where formid='" + txtRule.Text.Trim() + "')";
                //cmd = con.CreateCommand();
                cmd.CommandText = sql3.ToString();
                OracleDataReader dr3 = cmd.ExecuteReader();
                if (dr3.Read())
                {
                    label20.Text = "流程名称：自选流程;创建人：" + dr3["CREATEUSERNAME"].ToString();                   
                    cmd.CommandText = sql4.ToString();
                    OracleDataReader dr4 = cmd.ExecuteReader();
                    if (dr4.Read())
                    {
                        label20.Text += "\r\n关联模块：" + dr4["DESCRIPTION"].ToString() + "(" + dr4["MODELCODE"].ToString() + ")";
                    }
                    else
                    {
                        label20.Text = "流程名称：";
                    }
                    dr4.Close();
                }
                else
                {
                    label20.Text = "流程名称：";
                }
                dr3.Close();
                
            }
            #endregion
            Close();
            //string FlowConnection = System.Configuration.ConfigurationManager.ConnectionStrings["FlowConnection"].ConnectionString;
            //using (OracleConnection con = new OracleConnection(FlowConnection))
            //{
            //    if (con.State != ConnectionState.Open)
            //    {
            //        con.Open();
            //    }
            //    OracleCommand cmd = con.CreateCommand();
            //    cmd.CommandText = sql.ToString();
            //    cmd.CommandType = CommandType.Text;
            //    OracleDataReader dr = cmd.ExecuteReader();
            //    if (dr.Read())
            //    {
            //        textBox1.Text = FormatXml(dr["LAYOUT"].ToString());//代码
            //    }
            //    else
            //    {
            //        textBox1.Text = "";//代码
            //    }
            //    dr.Close();
            //};
            //FlowService.ServiceClient aa = new FlowService.ServiceClient();
            //aa.Endpoint.Address = new System.ServiceModel.EndpointAddress(txtAddress.Text);

            //var ss = aa.GetFlowInfo(txtFormID.Text, "", "", "", "", "", "");

            //var qq = from a in ss 
            //         group a by a.FLOW_FLOWRECORDMASTER_T
            //             into g
            //             orderby g.Key.CREATEDATE
            //             select g.Key;

        }
        #endregion
        private string FormatXml(string sUnformattedXml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter xtw = null;
            try
            {
                xtw = new XmlTextWriter(sw);

                xtw.Formatting = Formatting.Indented;
                xtw.Indentation = 1;
                xtw.IndentChar = '\t';

                xd.WriteTo(xtw);
            }
            finally
            {
                if (xtw != null)
                    xtw.Close();
            }
            return sb.ToString();
        }
        //获取审核信息
        private void button4_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (chkDetail.Checked)
                {
                    #region
                    StringBuilder sb = new StringBuilder();
                    //sb.AppendLine("--单据状态（1审批中，2审批通过，3审批不通过）");
                    //sb.AppendLine("--审核状态（1审核中，2审核通过，3审核不通过）");
                    //sb.AppendLine("select A.modelcode,A.checkstate as 单据状态,B.* from flow_flowrecordmaster_t A right join ");
                    //sb.AppendLine("(");
                    //sb.AppendLine("select t.editcompanyid,t.editdepartmentid,t.editpostid,t.edituserid, t.checkstate as 审核状态,t.flag,t.editusername,t.flowrecordmasterid from flow_flowrecorddetail_t t where ");
                    //sb.AppendLine("t.flowrecordmasterid in(select flowrecordmasterid from flow_flowrecordmaster_t t where t.formid = '" + textFormid.Text.Trim() + "') and t.checkstate='2' and t.flag='0' order by t.createdate desc ");
                    //sb.AppendLine(") B ON A.flowrecordmasterid=B.flowrecordmasterid");
                    #region  流程
                    sb.AppendLine("select A.modelcode as 模块名称,A.checkstate as 单据状态,B.* from flow_flowrecordmaster_t A right join ");
                    sb.AppendLine("(");
                    sb.AppendLine("select t.editcompanyid as 公司ID,t.editdepartmentid AS 部门ID,t.editpostid AS 岗位ID,t.edituserid AS 当前审核人ID, t.checkstate as 审核状态,t.flag as 处理状态,t.editusername AS 当前审核人,t.flowrecordmasterid from flow_flowrecorddetail_t t where ");
                    sb.AppendLine("t.flowrecordmasterid in(select flowrecordmasterid from flow_flowrecordmaster_t t where t.formid = '" + textFormid.Text.Trim() + "')  order by t.createdate desc ");
                    sb.AppendLine(") B ON A.flowrecordmasterid=B.flowrecordmasterid");
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }
                    OracleCommand cmd = con.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    cmd.CommandType = CommandType.Text;
                    OracleDataAdapter oda = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    oda.Fill(dt);

                    bool bol = false;//是否存在未处理信息
                    if (dt.Rows.Count > 0)
                    {
                        dataGridView1.DataSource = dt;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["审核状态"].ToString().ToLower() == "2" && dt.Rows[i]["处理状态"].ToString().ToLower() == "0")
                            {
                                bol = true;
                                txtCompanyID.Text = dt.Rows[i]["公司ID"].ToString();//公司ID
                                txtDepartID.Text = dt.Rows[i]["部门ID"].ToString();//部门ID
                                txtPostID.Text = dt.Rows[i]["岗位ID"].ToString();//岗位ID
                                txtUserID.Text = dt.Rows[i]["当前审核人ID"].ToString();//用户ID
                                txtUserName.Text = dt.Rows[i]["当前审核人"].ToString();//用户名
                                txtFormID.Text = textFormid.Text.Trim();//表单ID:
                                txtModelID.Text = dt.Rows[i]["模块名称"].ToString();//模板代码  
                            }
                        }
                        if (!bol)
                        {
                            txtCompanyID.Text = "";//公司ID
                            txtDepartID.Text = "";//部门ID
                            txtPostID.Text = "";//岗位ID
                            txtUserID.Text = "";//用户ID
                            txtUserName.Text = "";//用户名
                            txtFormID.Text = "";//表单ID:
                            txtModelID.Text = "";//模板代码
                        }
                    }
                    else
                    {
                        dataGridView1.DataSource = null;
                        txtCompanyID.Text = "";//公司ID
                        txtDepartID.Text = "";//部门ID
                        txtPostID.Text = "";//岗位ID
                        txtUserID.Text = "";//用户ID
                        txtUserName.Text = "";//用户名
                        txtFormID.Text = "";//表单ID:
                        txtModelID.Text = "";//模板代码
                    }
                    #endregion
                    #region 引擎
                    if (con2.State != ConnectionState.Open)
                    {
                        con2.Open();
                    }

                    //xml
                    //string sql = " select t.* from t_flow_enginemsglist t where t.ordernodecode like '%" + txtFormID.Text.Trim() + "' and t.receiveuser='" + txtUserID.Text + "' ORDER BY t.createtime desc";
                    //string sql = " select t.MESSAGEBODY AS 待办消息,t.receiveuser AS 待办人ID,t.receivedate AS 日期,t.receivetime AS 时间,t.messagestatus AS 待办状态,t.modelcode AS 模块ID,t.modelname AS 模块名称，t.appxml,t.flowxml,t.ORDERNODECODE as ID  from t_flow_enginemsglist t where t.ordernodecode like '%" + textFormid.Text.Trim() + "'  ORDER BY t.createtime desc";
                    string sql = "select t.MESSAGEBODY AS 待办消息,t.receiveuserid AS 待办人ID,t.createdatetime AS 时间,t.dotaskstatus AS 待办状态,t.modelcode AS 模块ID,t.modelname AS 模块名称,t.appxml,t.flowxml,t.orderid as ID  from t_wf_dotask t where t.orderid like '%" + textFormid.Text.Trim() + "'  ORDER BY t.createdatetime desc";
                    OracleCommand cmd2 = con2.CreateCommand();
                    cmd2.CommandText = sql;
                    cmd2.CommandType = CommandType.Text;

                    OracleDataAdapter oda2 = new OracleDataAdapter(cmd2);
                    DataTable dt2 = new DataTable();
                    oda2.Fill(dt2);
                    dataGridView2.DataSource = dt2;
                    bool bol2 = false;
                    if (dt2.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            if (dt2.Rows[i]["待办人ID"].ToString().ToLower() == txtUserID.Text)
                            {
                                bol2 = true;
                                txtXML.Text = dt2.Rows[i]["APPXML"].ToString();
                            }
                        }
                        if (!bol2)
                        {
                            txtXML.Text = "";
                        }
                    }
                    else
                    {
                        dataGridView2.DataSource = null;
                        txtXML.Text = "";
                    }
                    #endregion

                    #endregion
                }
                else
                {
                    #region 流程

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("--单据状态（1审批中，2审批通过，3审批不通过）");
                    sb.AppendLine("--审核状态（1审核中，2审核通过，3审核不通过）");
                    sb.AppendLine("select A.modelcode,A.checkstate as 单据状态,B.* from flow_flowrecordmaster_t A right join ");
                    sb.AppendLine("(");
                    sb.AppendLine("select t.editcompanyid,t.editdepartmentid,t.editpostid,t.edituserid, t.checkstate as 审核状态,t.flag,t.editusername,t.flowrecordmasterid from flow_flowrecorddetail_t t where ");
                    sb.AppendLine("t.flowrecordmasterid in(select flowrecordmasterid from flow_flowrecordmaster_t t where t.formid = '" + textFormid.Text.Trim() + "') and t.checkstate='2' and t.flag='0' order by t.createdate desc ");
                    sb.AppendLine(") B ON A.flowrecordmasterid=B.flowrecordmasterid");

                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }
                    OracleCommand cmd = con.CreateCommand();
                    cmd.CommandText = sb.ToString();
                    cmd.CommandType = CommandType.Text;
                    OracleDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {

                        txtCompanyID.Text = dr["EDITCOMPANYID"].ToString();//公司ID
                        txtDepartID.Text = dr["EDITDEPARTMENTID"].ToString();//部门ID
                        txtPostID.Text = dr["EDITPOSTID"].ToString();//岗位ID
                        txtUserID.Text = dr["EDITUSERID"].ToString();//用户ID
                        txtUserName.Text = dr["EDITUSERNAME"].ToString();//用户名
                        txtFormID.Text = textFormid.Text.Trim();//表单ID:
                        txtModelID.Text = dr["MODELCODE"].ToString();//模板代码
                    }
                    else
                    {
                        txtCompanyID.Text = "";//公司ID
                        txtDepartID.Text = "";//部门ID
                        txtPostID.Text = "";//岗位ID
                        txtUserID.Text = "";//用户ID
                        txtUserName.Text = "";//用户名
                        txtFormID.Text = "";//表单ID:
                        txtModelID.Text = "";//模板代码
                    }
                    dr.Close();
                    #endregion
                    #region 引擎
                    if (con2.State != ConnectionState.Open)
                    {
                        con2.Open();
                    }
                    //xml
                    string sql = " select t.APPXML from t_wf_dotask t where t.orderid like '%" + txtFormID.Text.Trim() + "' and t.orderuserid='" + txtUserID.Text + "' ORDER BY t.createdatetime desc";

                    cmd = con2.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    OracleDataReader dr2 = cmd.ExecuteReader();
                    if (dr2.Read())
                    {
                        txtXML.Text = dr2["APPXML"].ToString();
                    }
                    else
                    {
                        txtXML.Text = "";
                    }
                    dr2.Close();
                    #endregion
                }
                Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }
        OracleConnection con = new OracleConnection();
        OracleConnection con2 = new OracleConnection();
        private void Open()
        {
              string FlowConnection = System.Configuration.ConfigurationManager.ConnectionStrings["newFlowConnection"].ConnectionString;
              string EngineConnection = System.Configuration.ConfigurationManager.ConnectionStrings["newEngineConnection"].ConnectionString;
              con = new OracleConnection(FlowConnection);
              con2 = new OracleConnection(EngineConnection);
              if (con.State != ConnectionState.Open)
              {
                  con.Open();
              }
              if (con2.State != ConnectionState.Open)
              {
                  con2.Open();
              } 
        }
        private void Close()
        {
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            if (con2.State == ConnectionState.Open)
            {
                con2.Close();
            }  
        }
    }
}
