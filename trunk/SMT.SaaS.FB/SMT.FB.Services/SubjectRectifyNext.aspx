<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SubjectRectifyNext.aspx.cs" Inherits="SMT.FB.Services.SubjectRectifyNext" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        年结后月度预算没有生效，没有预算科目，解决办法<br />
        <br />
    
    </div>
    <asp:Label ID="Label1" runat="server" Text="公司"></asp:Label>
    <asp:TextBox ID="tbCom" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="Label2" runat="server" Text="部门"></asp:Label>
    <asp:TextBox ID="tbDep" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="Label4" runat="server" Text="科目"></asp:Label>
    <asp:TextBox ID="tbSub" runat="server"></asp:TextBox>
    <asp:Label ID="Label12" runat="server" Text="可为空，为空查询所有，为增加版使用"></asp:Label>
    <br />
    <asp:Button ID="btnCheck" runat="server" Text="查询accountd显示信息" OnClick="btnCheck_Click"/>
    <br />
    <asp:GridView ID="accountd" runat="server">
    </asp:GridView>
    <br />
    公司所属id<asp:TextBox ID="tbComID" runat="server" Width="286px"></asp:TextBox>
    <br />
    部门所属id<asp:TextBox ID="tbDepID" runat="server" 
        Width="285px"></asp:TextBox>
    <br />
    科目所属id<asp:TextBox ID="tbSubID" runat="server" 
        Width="284px"></asp:TextBox>
    <br />
    <asp:Label ID="Label6" runat="server" Text="预算总账id"></asp:Label>
    <asp:TextBox ID="tbBG" runat="server" Width="285px"></asp:TextBox>
    <br />
    <br />
    <asp:Button ID="btnBudget" runat="server" Text="查询t_Fb_Budgetaccount总账表" 
        onclick="btnBudget_Click" />
    <br />
    <br />
    <br />
    <asp:GridView ID="t_Fb_Budgetaccount" runat="server">
    </asp:GridView>
    <br />
    <asp:Label ID="Label5" runat="server" Text="输入月份"></asp:Label>
    <asp:TextBox ID="tbmonth" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="Label7" runat="server" Text="预算费用"></asp:Label>
    <asp:TextBox ID="tbbm" runat="server" ></asp:TextBox>
    <br />
    <asp:Label ID="Label8" runat="server" Text="可用结余"></asp:Label>
    <asp:TextBox ID="tbuser" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="Label9" runat="server" Text="实际结余"></asp:Label>
    <asp:TextBox ID="tbact" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="Label10" runat="server" Text="创建时间"></asp:Label>
    <asp:TextBox ID="tbcreate" runat="server" ></asp:TextBox>
    <br />
    <asp:Label ID="Label11" runat="server" Text="更新时间"></asp:Label>
    <asp:TextBox ID="tbupdate" runat="server"></asp:TextBox>
    <br />
    <asp:Button ID="btnInsert" runat="server" Text="插入数据" 
        onclick="btnInsert_Click" />
    <br />
    ——————————————————我是分割线—————————————<br />
    <asp:Button ID="btCheckzb_dept" runat="server" Text="查询部门流水" 
        onclick="btCheckzb_dept_Click" />
    <br />
    <asp:GridView ID="zb_dept" runat="server">
    </asp:GridView>
    <asp:Button ID="btnClear" runat="server" 
        Text="预算清零（警告，该功能一般不要用，仅仅用于跨年年结以后预算科目结余没清零的情况）" onclick="btnClear_Click" />
    <br />
    <br />
    <br />
    <br />
    <br />
    <br />
    <br />
    <br />
    <br />
    <br />
    ——————————————还是分割线————————<br />
    <asp:Button ID="btCheckNew" runat="server" Text="查询部门流水增强（只根据公司部门没有预算科目）" 
        onclick="btCheckNew_Click" />
    <asp:Label ID="lbflag" runat="server" Text="流水是否为空标示"></asp:Label>
    <br />
    <asp:GridView ID="zb_deptNew" runat="server">
    </asp:GridView>
    <asp:Button ID="btnClearNew" runat="server" 
        Text="预算清零增强版" onclick="btnClearNew_Click" />
    <br />
    <asp:Button ID="btdelete" runat="server" Text="删除" onclick="btdelete_Click" />
    <asp:TextBox ID="tbdelete" runat="server"></asp:TextBox>
    </form>
</body>
</html>
