<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test2.aspx.cs" Inherits="SMT.FB.Services.Test2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:Button ID="btnTest" runat="server" onclick="btnTest_Click" Text="测试" />
    <asp:Button ID="btnContenList" runat="server" onclick="btnContenList_Click" Text="列出连接" />
    </div>
    <div>
    <asp:Label Text="预算日期 : " style="width: 79px"  runat="server"/>
    <asp:TextBox ID="txtMonth" runat="server" Text="2012-01-01" />
    <asp:Label Text="部门ID : " style="width: 68px" runat="server" />
    
    <asp:TextBox ID="txtDeptID" runat="server" 
            Text="fb0b4d07-edff-4a43-967c-51402379484c" Width="277px" />
    <asp:Button ID="btnDeptDetail" runat="server" Text="列出部门预算" 
            onclick="btnDeptDetail_Click" />
    <asp:Label Text="[查部门明细的操作]" runat="server" />
    </div>
    
    <div>
        <asp:Label ID="listDetail" runat="server" />
    </div>
    <div>
    <asp:GridView ID="GridView1" runat="server" Height="245px" Width="759px" AutoGenerateColumns="true">
    </asp:GridView>    
    </div>
    <div>
    <div>
    <asp:TextBox ID="txtOrderID" runat="server" />
    <asp:TextBox ID="txtCheckState" runat="server" />
    <asp:Button ID="btnUpdateOrder" runat="server" onclick="btnUpdateOrder_Click" />
    </div>
    <div>
    <asp:Label ID="lbMsg" runat="server" />
    </div>
    </div>
    </form>
</body>
</html>
