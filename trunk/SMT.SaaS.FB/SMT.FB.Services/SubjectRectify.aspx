<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SubjectRectify.aspx.cs" Inherits="SMT.FB.Services.SubjectRectify" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .style1
        {
            width: 100%;
        }
        .style2
        {
            height: 19px;
        }
        .style3
        {
            text-align: right;
        }
        .style4
        {
            width: 260px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <br />
        科目费用结余查询与维护:<asp:TextBox ID="txtResult" runat="server" Height="89px" TextMode="MultiLine" 
            Width="738px"></asp:TextBox>
        <br />
        <table cellspacing="1" class="style1" cellpadding="0" frame="border">
            <tr>
                <td colspan="3">
                    <table class="style1">
                        <tr>
                            <td height="25px" width="100px">
                                预算类型：</td>
                            <td class="style4">
                                <asp:DropDownList ID="DropDownList1" runat="server" Height="25px" Width="100px">
                                    <asp:ListItem Value="1">部门年度</asp:ListItem>
                                    <asp:ListItem Selected="True" Value="2">部门月度</asp:ListItem>
                                    <asp:ListItem Value="3">个人月度</asp:ListItem>
                                </asp:DropDownList>
                            &nbsp; 年份：<asp:DropDownList ID="DropDownList2" runat="server" Height="25px" 
                                    Width="76px">
                                    <asp:ListItem Value="0">全部</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td style="text-align: left">
                                <asp:Button ID="btnCloseBudget" runat="server" onclick="btnCloseBudget_Click" 
                                    Text="手动月结" />
                                <asp:Button ID="btnHuodong" runat="server" onclick="btnHuodong_Click" 
                                    Text="批量合并重复的活动经费" />
                            </td>
                            <td style="text-align: right">
                                <asp:Button ID="Button5" runat="server" Height="25px" onclick="Button5_Click" 
                                    Text="批处理可用额度" Visible="False" />
                            </td>
                        </tr>
                        <tr>
                            <td Height="25px" Width="100px">
                                科目：</td>
                            <td style="color: #FF0000" class="style4">
                                <asp:TextBox ID="TextBox1" runat="server" Height="25px" Width="250px" ></asp:TextBox>
                                <asp:Button ID="btnsubject" runat="server" onclick="btnsubject_Click" 
                                    Text="检查科目" />
                                </td>
                            <td colspan="2">
                                科目如果是跨年度使用，请把年份选择为“全部”，必填。</td>
                        </tr>
                        <tr>
                           <td Height="25px" Width="100px">
                                公司名称：</td>
                             <td style="color: #FF0000" class="style4">
                                <asp:TextBox ID="TextBox2" runat="server" Height="25px" Width="250px" ></asp:TextBox>
                                 <asp:Button ID="btnCompany" runat="server" onclick="btnCompany_Click" 
                                     Text="检查公司" />
                                </td>
                            <td colspan="2">
                                请使用全称等值查询，必填。</td>
                        </tr>
                        <tr>
                           <td Height="25px" Width="100px">
                                部门名称：</td>
                            <td class="style4">
                                <asp:TextBox ID="TextBox3" runat="server" Height="25px" Width="250px" ></asp:TextBox>
                                <asp:Button ID="btnDepartment" runat="server" onclick="btnDepartment_Click" 
                                    style="height: 21px" Text="检查部门" />
                                <asp:Button ID="btnCheckDep" runat="server" onclick="btnCheckDep_Click" 
                                    Text="检查部门信息完整性" />
                            </td>
                            <td colspan="2">
                                请使用全称等值查询，预算类型为部门月度、部门年度时必填。</td>
                        </tr>
                        <tr>
                           <td Height="25px" Width="100px">
                                个人姓名：</td>
                            <td class="style4">
                                <asp:TextBox ID="TextBox4" runat="server" Height="25px" Width="250px" ></asp:TextBox>
                            </td>
                            <td colspan="2">
                                预算类型为个人月度时，则此项必填。</td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="button1" runat="server" onclick="Button1_Click" Text="查询" 
                        Width="61px" />
                </td>
                <td>
                    <asp:Button ID="BtnBrowrry" runat="server" onclick="BtnBrowrry_Click" 
                        Text="查询借还款" />
                </td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    总账记录:</td>
                <td colspan="2">
                    <asp:GridView ID="total" runat="server" BackColor="White" BorderColor="#3366CC" 
                        BorderStyle="None" BorderWidth="1px" CellPadding="4">
                        <FooterStyle BackColor="#99CCCC" ForeColor="#003399" />
                        <HeaderStyle BackColor="#003399" Font-Bold="True" ForeColor="#CCCCFF" />
                        <PagerStyle BackColor="#99CCCC" ForeColor="#003399" HorizontalAlign="Left" />
                        <RowStyle BackColor="White" ForeColor="#003399" />
                        <SelectedRowStyle BackColor="#009999" Font-Bold="True" ForeColor="#CCFF99" />
                        <SortedAscendingCellStyle BackColor="#EDF6F6" />
                        <SortedAscendingHeaderStyle BackColor="#0D4AC4" />
                        <SortedDescendingCellStyle BackColor="#D6DFDF" />
                        <SortedDescendingHeaderStyle BackColor="#002876" />
                    </asp:GridView>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    可用结余：<asp:TextBox ID="TextBox6" runat="server"></asp:TextBox>
                    <asp:Button ID="Button3" runat="server" onclick="Button3_Click" Text="修改可用结余" 
                        Height="25px" />
                    <asp:Label ID="Label3" runat="server" ForeColor="Red"></asp:Label>
                </td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    实际结余：<asp:TextBox ID="TextBox7" runat="server"></asp:TextBox>
                    <asp:Button ID="Button4" runat="server" Height="25px" onclick="Button4_Click" 
                        Text="修改实际结余" />
                    <asp:Label ID="Label4" runat="server" ForeColor="Red"></asp:Label>
                </td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    借还款结余：<asp:TextBox ID="TxtBowrryMoney" runat="server"></asp:TextBox>
                    <asp:Button ID="btnBowrryMoney" runat="server" Text="修改借款余额" 
                        onclick="btnBowrryMoney_Click" />
                    <asp:Label ID="lbBrowrryMoney" runat="server" Text="Label" ForeColor="Red"></asp:Label>
                </td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td colspan="2" class="style3">
                    SQL语句查询：<asp:TextBox ID="TextBox5" runat="server" 
                        Height="25px"></asp:TextBox>
                    <asp:Button ID="Button2" runat="server" onclick="Button2_Click" Text="执行查询" 
                        Width="85px" />
                </td>
            </tr>
            <tr>
                <td class="style2">
                    单据明细：</td>
                <td class="style2" colspan="2">
                    可用金额：<asp:Label 
                        ID="Label1" runat="server" Text="Label"></asp:Label>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 实际金额：<asp:Label ID="Label2" runat="server" Text="Label"></asp:Label>
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:GridView ID="order" runat="server" BackColor="White" BorderColor="#CCCCCC" 
                        BorderStyle="None" BorderWidth="1px" CellPadding="3">
                        <FooterStyle BackColor="White" ForeColor="#000066" />
                        <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                        <PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
                        <RowStyle ForeColor="#000066" />
                        <SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                        <SortedAscendingCellStyle BackColor="#F1F1F1" />
                        <SortedAscendingHeaderStyle BackColor="#007DBB" />
                        <SortedDescendingCellStyle BackColor="#CAC9C9" />
                        <SortedDescendingHeaderStyle BackColor="#00547E" />
                    </asp:GridView>
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
            </tr>
            </table>
        <br />
        <br />
    
    </div>
    </form>
</body>
</html>
