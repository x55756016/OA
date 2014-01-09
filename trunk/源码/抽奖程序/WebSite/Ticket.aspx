<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Ticket.aspx.cs" Inherits="Asd.Award.Ticket" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" 
        AllowSorting="True" AutoGenerateColumns="False" DataKeyNames="TicketNO" 
        DataSourceID="SqlDataSource1" EmptyDataText="没有可显示的数据记录。" Width="98px">
        <Columns>
            <asp:BoundField DataField="TicketNO" HeaderText="TicketNO" ReadOnly="True" 
                SortExpression="TicketNO" />
            <asp:BoundField DataField="TicketCount" HeaderText="TicketCount" 
                SortExpression="TicketCount" />
        </Columns>
    </asp:GridView>
    <asp:Button ID="btnsz" runat="server" onclick="btnsz_Click" Text="修改深圳总人数" />
    <asp:TextBox ID="tbsz" runat="server"></asp:TextBox>
    <br />
    <asp:Button ID="btnbj" runat="server" Text="修改北京总人数" onclick="btnbj_Click" />
    <asp:TextBox ID="tbbj" runat="server"></asp:TextBox>
    <asp:SqlDataSource ID="SqlDataSource1" runat="server" 
        ConnectionString="<%$ ConnectionStrings:AsdLyncConnectionString %>" 
        ProviderName="<%$ ConnectionStrings:AsdLyncConnectionString.ProviderName %>" 
        SelectCommand="SELECT * FROM [TmpTicket]">
    </asp:SqlDataSource>
  
</asp:Content>
