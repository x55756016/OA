<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test.aspx.cs" Inherits="SMT.FB.Services.test" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="height: 29px">
        <asp:TextBox ID="TextBox1" runat="server" Width="337px"></asp:TextBox>        
        <asp:Button ID="btnShow" runat="server" Text="查看" onclick="btnShow_Click"  />
       
        <asp:Button ID="btnShow0" runat="server" Text="月度结算" onclick="btnShow0_Click" />
       
    </div>
    <div style="height: 21px">
        <asp:TextBox ID="TextBox2" runat="server" Width="337px"></asp:TextBox>        
    </div>
    <div>
    <asp:Panel ID="Panel1" runat="server" Height="49px" Width="957px">
        <asp:DataList ID="DataList1" runat="server">
        </asp:DataList>
    </asp:Panel>
    </div>
    

    <div>
    <asp:TextBox ID="TextBox3" runat="server" Width="852px" Height="116px"></asp:TextBox>     
     <asp:Button ID="Button1" runat="server" Text="更新" onclick="Button1_Click"  />   
    </div>

    <div>
        EXTENSIONALORDERID：
        <asp:TextBox ID="txtExtensionalOrderID" runat="server" />
        <asp:Button ID="btnGetTravelExpApplyMasterCode" runat="server" Text="获取单据编号" 
            onclick="btnGetTravelExpApplyMasterCode_Click" />
        <asp:Label ID="lblExpApplyMasterCode" runat="server" Text=""></asp:Label>
    </div>
    <div> 
        实体类型：
        <asp:DropDownList ID="ddlOrderType" runat="server" />
        <br />
        单据ID：
        <asp:TextBox ID="txtOrderID" runat="server" Height="54px" Width="777px" />
        <br />
        <br />
        <asp:Button ID="Button2" runat="server" Text="提交" 　 onclick="Button2_Click"/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Button ID="Button3" runat="server" Text="审核通过" onclick="Button3_Click"/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Button ID="Button4" runat="server" Text="审核不通过"　 onclick="Button4_Click"/>&nbsp;&nbsp;&nbsp;&nbsp;
    &nbsp;</div>
    <div>
    <asp:Button ID="Button5" runat="server" Text="测试" onclick="Button5_Click" />
    </div>
    <div>
    <p style="height:30px; text-align:center;">---------------------------分割行：行下为专用手机测试---------------------------
    </p>
     实体类型：
        <asp:DropDownList ID="dpdMobileEntityList" runat="server" />
        <br />
        单据ID：
        <asp:TextBox ID="txtMobileEntityID" runat="server" Height="54px" Width="777px" />
        <br />
        <br />
        <asp:Button ID="Button6" runat="server" Text="提交" 　 onclick="btnMobileApproving_Click"/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Button ID="Button7" runat="server" Text="审核通过" onclick="btnMobileApproved_Click"/>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Button ID="Button8" runat="server" Text="审核不通过"　 onclick="btnMobileUnApproved_Click"/>&nbsp;&nbsp;&nbsp;&nbsp;
    </div>
    </form>
</body>
</html>
