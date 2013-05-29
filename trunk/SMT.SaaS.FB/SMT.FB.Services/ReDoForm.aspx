<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReDoForm.aspx.cs" Inherits="SMT.FB.Services.ReDoForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div>
        <asp:Button ID="Button2" runat="server" Text="统计单据" onclick="Button2_Click"  />
        <asp:Button ID="Button1" runat="server" Text="模拟审单" onclick="Button1_Click" />
        <asp:TextBox ID="tbStart" runat="server">2011-01-01</asp:TextBox>
        <asp:TextBox ID="tbEnd" runat="server">2011-02-01</asp:TextBox>
            <asp:Button ID="Button3" runat="server" onclick="Button3_Click" Text="月结" />
        </div>
        <div>
           <asp:TextBox ID="tbRemark" runat="server" MaxLength="3" TextMode="MultiLine" 
                Height="432px" Width="805px"></asp:TextBox>
        </div>
    
    </div>
    
    </form>
</body>
</html>
