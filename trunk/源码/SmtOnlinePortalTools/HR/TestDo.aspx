<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestDo.aspx.cs" Inherits="SMT.HRM.Services.TestDo" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        body
        {
            background: #b6b7bc;
            font-size: .80em;
            font-family: "Helvetica Neue" , "Lucida Grande" , "Segoe UI" , Arial, Helvetica, Verdana, sans-serif;
            margin: 0px;
            padding: 0px;
            color: #696969;
        }
        .page
        {
            width: 960px;
            background-color: #fff;
            margin: 20px auto 0px auto;
            border: 1px solid #496077;
        }
        
        .header
        {
            position: relative;
            margin: 0px;
            padding: 0px;
            background: #4b6c9e;
            width: 100%;
        }
        #menu, #footer
        {
            clear: both;
        }
        #left, #middle
        {
            float: left;
        }
        #left
        {
            width: 200px;
        }
        #footer
        {
            color: Red;
            text-align: center;
        }
        
        fieldset
        {
            margin: 1em 0px;
            padding: 1em;
            border: 1px solid #ccc;
        }
        
        fieldset p
        {
            margin: 2px 12px 10px 10px;
        }
        
        fieldset.login label, fieldset.register label, fieldset.changePassword label
        {
            display: block;
        }
        
        fieldset label.inline
        {
            display: inline;
        }
        #left
        {
            font-family: Verdana, Arial, Helvetica, sans-serif;
            font-size: 100%;
            padding: 0px;
            margin: 0px;
        }
        
        #left ul
        {
            list-style: none;
            margin: 0px;
            padding: 0px;
            border: none;
        }
        #left ul li
        {
            margin: 0px;
            padding: 0px;
        }
        #left ul li a
        {
            display: block;
            border-bottom: 1px dashed #C39C4E;
            padding: 5px 0px 2px 4px;
            text-decoration: none;
            color: #666666;
        }
        
        #left ul li a:hover, #left ul li a:focus
        {
            color: #000000;
            background-color: #eeeeee;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="page">
        <div class="header">
            <div class="title">
                <h1>
                    HR数据处理
                </h1>
            </div>
        </div>
        <asp:Panel ID="plLogin" runat="server">
            <div class="accountInfo">
                <fieldset class="login">
                    <legend>帐户信息</legend>
                    <p>
                        <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName">用户名:</asp:Label>
                        <asp:TextBox ID="UserName" runat="server" CssClass="textEntry"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                            CssClass="failureNotification" ErrorMessage="必须填写“用户名”。" ToolTip="必须填写“用户名”。"
                            ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                    </p>
                    <p>
                        <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password">密码:</asp:Label>
                        <asp:TextBox ID="Password" runat="server" CssClass="passwordEntry" TextMode="Password"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                            CssClass="failureNotification" ErrorMessage="必须填写“密码”。" ToolTip="必须填写“密码”。" ValidationGroup="LoginUserValidationGroup">*</asp:RequiredFieldValidator>
                    </p>
                </fieldset>
                <p class="submitButton">
                    <asp:Button ID="LoginButton" runat="server" Text="登录" ValidationGroup="LoginUserValidationGroup"
                        OnClick="LoginButton_Click" />
                </p>
            </div>
        </asp:Panel>
        <asp:Panel ID="plManage" runat="server">
            <div id="left">
                <ul>
                    <li>
                        <asp:LinkButton ID="lblCreateAtt" runat="server">考勤初始化</asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="lblUpdateAttRd" runat="server">检查请假及出差</asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="lblCheckAbnormal" runat="server">检查考勤异常</asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="lblClearAtt" runat="server">考勤清理</asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="lblUpdateFreeLeaveRd" runat="server">生成带薪假记录</asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="lblAssignCompany" runat="server">生成下拨经费</asp:LinkButton>
                    </li>
                    <li>
                        <asp:LinkButton ID="lblUpdateRecord" runat="server">更新审核记录</asp:LinkButton>
                    </li> 
                     <li>
                        <asp:LinkButton ID="lblGetFreeDays" runat="server">获取带薪假可用天数</asp:LinkButton>
                    </li>                    
                </ul>
            </div>
            <div id="middle">
                <div style="width: 100%">
                    <p>
                        <span style="width: 90px; text-align: left;">机构类型：</span>
                        <asp:TextBox ID="txtOrgType" runat="server" ToolTip="公司：1；员工：4"></asp:TextBox>
                    </p>
                    <p>
                        <span style="width: 90px; text-align: left;">机构ID：</span>
                        <asp:TextBox ID="txtOrgID" runat="server"></asp:TextBox>
                    </p>
                     <p>
                        <span style="width: 90px; text-align: left;">考勤月份：</span>
                        <asp:TextBox ID="txtCreateAttMonth" runat="server"></asp:TextBox>
                    </p>
                    <asp:Button ID="btnCreateAtt" runat="server" Text="考勤初始化" OnClick="btnCreateAtt_Click" />
                </div>
                <p>
                </p>
                <div>
                    <p>
                        <span style="width: 90px; text-align: left;">公司ID：</span>
                        <asp:TextBox ID="txtCompanyID" runat="server"></asp:TextBox>
                    </p>
                    <p>
                        <span style="width: 90px; text-align: left;">考勤月份：</span>
                        <asp:TextBox ID="txtCurMonth" runat="server"></asp:TextBox>
                    </p>
                    <asp:Button ID="btnUpdateAttRd" runat="server" Text="检查请假及出差" OnClick="btnUpdateAttRd_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <p>
                        <span style="width: 90px; text-align: left;">机构类型：</span>
                        <asp:TextBox ID="txtAbnormalOrgType" runat="server" ToolTip="公司：1；员工：4"></asp:TextBox>
                    </p>
                    <p>
                        <span style="width: 90px; text-align: left;">机构ID：</span>
                        <asp:TextBox ID="txtAbnormalOrgId" runat="server"></asp:TextBox>
                    </p>
                    <p>
                        <span style="width: 90px; text-align: left;">考勤月份：</span>
                        <asp:TextBox ID="txtPunchFrom" runat="server"></asp:TextBox><span style="width: 10px; text-align: left;">至</span><asp:TextBox ID="txtPunchTo" runat="server"></asp:TextBox>
                    </p>
                    <asp:Button ID="btnCheckAbnormal" runat="server" Text="检查考勤异常" OnClick="btnCheckAbnormal_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <span style="width: 90px; text-align: left;">机构类型：</span>
                    <asp:TextBox ID="txtClearAttOrgType" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">机构ID：</span>
                    <asp:TextBox ID="txtClearAttOrgID" runat="server"></asp:TextBox>
                    <br />
                    <asp:Button ID="btnClearAtt" runat="server" Text="考勤清理(作废）" 
                        OnClick="btnClearAtt_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <span style="width: 90px; text-align: left;">机构类型：</span>
                    <asp:TextBox ID="txtFLOrgType" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">机构ID：</span>
                    <asp:TextBox ID="txtFLOrgID" runat="server"></asp:TextBox>
                    <br />
                    <asp:Button ID="btnUpdateFreeLeaveRd" runat="server" Text="生成带薪假记录" OnClick="btnUpdateFreeLeaveRd_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <span style="width: 90px; text-align: left;">下拨公司ID：</span>
                    <asp:TextBox ID="txtAssignCompanyID" runat="server"></asp:TextBox>
                    <br />
                    <asp:Button ID="btnAssignCompany" runat="server" Text="生成下拨经费(作废）" 
                        OnClick="btnAssignCompany_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <p>
                        <span style="width: 90px; text-align: left;">机构类型：</span>
                        <asp:TextBox ID="txtPayOrgType" runat="server" ToolTip="公司：1；员工：4"></asp:TextBox>
                    </p>
                    <p>
                        <span style="width: 90px; text-align: left;">机构ID：</span>
                        <asp:TextBox ID="txtPayOrgID" runat="server"></asp:TextBox>
                    </p>
                    <p>
                        <span style="width: 90px; text-align: left;">发放月份：</span>
                        <asp:TextBox ID="txtPayMonth" runat="server"></asp:TextBox>
                    </p>
                    <asp:Button ID="btnCheckPayment" runat="server" Text="检查薪资发放(作废）" 
                        OnClick="btnCheckPayment_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <span style="width: 90px; text-align: left;">表名：</span><asp:TextBox ID="txtTableName"
                        runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">主键名：</span>
                    <asp:TextBox ID="txtTableKeyName" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">主键值：</span>
                    <asp:TextBox ID="txtFormID" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">审核状态：</span>
                    <asp:TextBox ID="txtCheckStates" runat="server"></asp:TextBox>
                    <br />
                    <asp:Button ID="btnUpdateRecord" runat="server" Text="更新审核记录" OnClick="btnUpdateRecord_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <asp:Button ID="btnInitAttendRdByEngine" runat="server" Text="触发引擎初始化考勤记录(作废）" ToolTip="触发后，每月1号临晨启动考勤初始化记录"
                        OnClick="btnInitAttendRdByEngine_Click" />
                    <br />
                    <br />
                    <asp:Button ID="btnInitFreeLeaveDaysByEngine" runat="server" Text="触发引擎初始化带薪假记录(作废）"
                        ToolTip="触发后，每月1号临晨启动考勤初始化记录" 
                        OnClick="btnInitFreeLeaveDaysByEngine_Click" />
                    <br />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <span style="width: 90px; text-align: left;">机构类型：</span>
                    <asp:TextBox ID="txtInserviceOrgType" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">机构ID：</span>
                    <asp:TextBox ID="txtInserviceOrgID" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">登录人ID：</span>
                    <asp:TextBox ID="txtInserviceOwnerID" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">截止时间：</span>
                    <asp:TextBox ID="txtInserviceDateTo" runat="server"></asp:TextBox>
                    <br />
                    <asp:Button ID="btnGetInserviceCount" runat="server" Text="检查在线人数(作废）" 
                        OnClick="btnGetInserviceCount_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <span style="width: 90px; text-align: left;">请假日期：</span>
                    <asp:TextBox ID="txtLeaveStartDate" runat="server"></asp:TextBox>
                    到
                    <asp:TextBox ID="txtLeaveEndDate" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">假期标准ID：</span>
                    <asp:TextBox ID="txtLeaveTypeSetID" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">登录人ID：</span>
                    <asp:TextBox ID="txtLeaveUserID" runat="server"></asp:TextBox>
                    <br />
                    <asp:Button ID="btnGetFreeDays" runat="server" Text="获取带薪假可用天数(作废）" 
                        OnClick="btnGetFreeDays_Click" />
                </div>
                <p>
                </p>
                <div style="width: 100%">
                    <span style="width: 90px; text-align: left;">请假时间：</span>
                    <asp:TextBox ID="txtLeaveFrom" runat="server"></asp:TextBox>
                    到
                    <asp:TextBox ID="txtLeaveTo" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">请假记录ID：</span>
                    <asp:TextBox ID="txtLeaveRdID" runat="server"></asp:TextBox>
                    <br />
                    <span style="width: 90px; text-align: left;">请假人ID：</span>
                    <asp:TextBox ID="txtLeaveEmployeeID" runat="server"></asp:TextBox>
                    <br />
                    <asp:Button ID="btnGetLeaveDays" runat="server" Text="计算请假时长(作废）" 
                        OnClick="btnGetLeaveDays_Click" />
                    <br />
                    <asp:Literal ID="litRealLeaveDays" runat="server"></asp:Literal>
                </div>
                <p>
                </p>
                <p>
                </p>
                <div>
                    <p>
                        <span style="width: 90px; text-align: left;">岗位ID：</span>
                        <asp:TextBox ID="txtLeaderPostID" runat="server"></asp:TextBox>
                    </p>
                    <p>
                        <span style="width: 90px; text-align: left;">上级类型：</span>
                        <asp:TextBox ID="txtLeaderType" runat="server"></asp:TextBox>
                    </p>
                    <asp:Button ID="btnCheckEmployeeLeader" runat="server" 
                        Text="检查流程获取上级员工信息是否正常(作废）" OnClick="btnCheckEmployeeLeader_Click" />
                </div>
            </div>
        </asp:Panel>
        <div id="footer">
            <asp:Literal ID="ltlMsg" runat="server"></asp:Literal>
        </div>
    </div>
    </form>
</body>
</html>
