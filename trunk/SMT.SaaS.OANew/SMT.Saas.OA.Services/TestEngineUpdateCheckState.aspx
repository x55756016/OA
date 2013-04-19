<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestEngineUpdateCheckState.aspx.cs" Inherits="SMT.SaaS.OA.Services.TestEngineUpdateCheckState" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        SystemCode<asp:TextBox ID="txtSystemcode" runat="server" Width="207px"></asp:TextBox>
        <br />
    
        EntityType<asp:TextBox ID="txtEntityType" runat="server" Width="209px"></asp:TextBox>
        <br />
        EntityKey<asp:TextBox ID="txtEntityKey" runat="server" Width="221px"></asp:TextBox>
        <br />
        EntityId<asp:TextBox ID="txtEntityId" runat="server" Width="260px"></asp:TextBox>
        <br />
        <br />
        <br />
        <asp:Button ID="btnSubmit" runat="server" onclick="btnSubmit_Click" Text="提交" />
        <br />
        <br />
        <br />
        <asp:TextBox ID="txtMessage" runat="server" Height="140px" TextMode="MultiLine" 
            Width="767px"></asp:TextBox>
    
    </div>
    <p>
    公司ID<asp:TextBox ID="txtCompanyID" runat="server" Width="207px"></asp:TextBox>
        <br />
        出差月份<asp:TextBox ID="txtTravelMonth" runat="server" Width="209px"></asp:TextBox>
        <br />
        <asp:Button ID="btnSyncAttend" runat="server" onclick="btnSyncAttend_Click" Text="同步考勤" />
        <br />
        <span style="color:Red;"><asp:Literal ID="ltlMsg" runat="server"></asp:Literal></span>
    </p>
    </form>
</body>
</html>
