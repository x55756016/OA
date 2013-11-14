<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestDo2.aspx.cs" Inherits="SMT.HRM.Services.TestDo2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
     <div style="width: 100%">
        <span style="width: 90px; text-align: left;">机构类型：</span>
        <asp:TextBox ID="txtFLOrgType" runat="server"></asp:TextBox>
        <br />
        <span style="width: 90px; text-align: left;">机构ID：</span>
        <asp:TextBox ID="txtFLOrgID" runat="server"></asp:TextBox>
        <br />
        <asp:Button ID="btnUpdateFreeLeaveRd" runat="server" Text="生成带薪假记录" 
             onclick="btnUpdateFreeLeaveRd_Click"/>
         <br />
         <br />
         <div id="DivTextBox"><asp:TextBox ID="TextBox1" runat="server" BackColor="Red"></asp:TextBox></div>
         
     </div>
    </div>
    <div>
    <p>
        <asp:Button ID="btnGetConfig" runat="server" Text="获取配置项" 
             onclick="btnGetConfig_Click"/>
         <asp:TextBox ID="TxtConfigName" runat="server" Width="285px"></asp:TextBox>
    </p>
    <asp:TextBox ID="txtResult" runat="server" Height="68px" Width="824px"></asp:TextBox>
    </div>
    <div>
    <p> <asp:Button ID="btnExcuteSql" runat="server" Text="Excute SQL" 
            onclick="btnExcuteSql_Click" /></p>
    <p><asp:TextBox ID="TxtSql" runat="server" Height="93px" Width="819px"></asp:TextBox></p>
    <p>        <asp:GridView ID="gridViewResult" runat="server">
        </asp:GridView>
    </p>
    </div>
    </form>
</body>
</html>
