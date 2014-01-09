<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm2.aspx.cs" Inherits="Asd.Award.WebForm2" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:GridView ID="GridView1" runat="server" AllowPaging="True" 
            AllowSorting="True" AutoGenerateColumns="False" 
            DataSourceID="EntityDataSource1" DataKeyNames="TicketNO">
            <Columns>
                <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" 
                    ShowSelectButton="True" />
                <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
                <asp:BoundField DataField="TicketNO" HeaderText="TicketNO" ReadOnly="True" 
                    SortExpression="TicketNO" />
                <asp:BoundField DataField="Remark" HeaderText="Remark" 
                    SortExpression="Remark" />
                <asp:BoundField DataField="UpdateTime" HeaderText="UpdateTime" 
                    SortExpression="UpdateTime" />
            </Columns>
        </asp:GridView>
    
        <asp:EntityDataSource ID="EntityDataSource1" runat="server" 
            ConnectionString="name=AsdLyncEntities" DefaultContainerName="AsdLyncEntities" 
            EnableDelete="True" EnableFlattening="False" EnableInsert="True" 
            EnableUpdate="True" EntitySetName="TmpAward">
        </asp:EntityDataSource>
    
    </div>
    </form>
</body>
</html>
