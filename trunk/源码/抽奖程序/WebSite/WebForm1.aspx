<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="Asd.Award.WebForm1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
            DeleteMethod="Delete" InsertMethod="Insert" 
            OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" 
            TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.TmpAwardTableAdapter" 
            UpdateMethod="Update">
            <DeleteParameters>
                <asp:Parameter Name="Original_TicketNO" Type="String" />
            </DeleteParameters>
            <InsertParameters>
                <asp:Parameter Name="Level" Type="String" />
                <asp:Parameter Name="TicketNO" Type="String" />
                <asp:Parameter Name="Remark" Type="String" />
                <asp:Parameter Name="UpdateTime" Type="DateTime" />
            </InsertParameters>
            <UpdateParameters>
                <asp:Parameter Name="Level" Type="String" />
                <asp:Parameter Name="Remark" Type="String" />
                <asp:Parameter Name="UpdateTime" Type="DateTime" />
                <asp:Parameter Name="Original_TicketNO" Type="String" />
            </UpdateParameters>
        </asp:ObjectDataSource>
        <asp:GridView ID="GridView1" runat="server" AllowPaging="True" 
            AutoGenerateColumns="False" DataKeyNames="TicketNO" 
            DataSourceID="ObjectDataSource1">
            <Columns>
                <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />
                <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
                <asp:BoundField DataField="TicketNO" HeaderText="TicketNO" ReadOnly="True" 
                    SortExpression="TicketNO" />
                <asp:BoundField DataField="Remark" HeaderText="Remark" 
                    SortExpression="Remark" />
                <asp:BoundField DataField="UpdateTime" HeaderText="UpdateTime" 
                    SortExpression="UpdateTime" />
            </Columns>
        </asp:GridView>
        <asp:DetailsView ID="DetailsView1" runat="server" AutoGenerateRows="False" 
            DataKeyNames="TicketNO" DataSourceID="ObjectDataSource1" Height="50px" 
            Width="125px">
            <Fields>
                <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
                <asp:BoundField DataField="TicketNO" HeaderText="TicketNO" ReadOnly="True" 
                    SortExpression="TicketNO" />
                <asp:BoundField DataField="Remark" HeaderText="Remark" 
                    SortExpression="Remark" />
                <asp:BoundField DataField="UpdateTime" HeaderText="UpdateTime" 
                    SortExpression="UpdateTime" />
                <asp:CommandField ShowInsertButton="True" />
            </Fields>
        </asp:DetailsView>
    
    </div>
    </form>
</body>
</html>
