<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Option.aspx.cs" Inherits="Asd.Award.Option" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
        DeleteMethod="Delete" InsertMethod="Insert" 
        OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" 
        TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.TmpOptionTableAdapter" 
        UpdateMethod="Update">
        <DeleteParameters>
            <asp:Parameter Name="Original_BelongTo" Type="String" />
            <asp:Parameter Name="Original_Level" Type="String" />
            <asp:Parameter Name="Original_HitSequence" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:Parameter Name="BelongTo" Type="String" />
            <asp:Parameter Name="Level" Type="String" />
            <asp:Parameter Name="HitSequence" Type="Int32" />
            <asp:Parameter Name="AwardQty" Type="Int32" />
            <asp:Parameter Name="Remark" Type="String" />
        </InsertParameters>
        <UpdateParameters>
            <asp:Parameter Name="AwardQty" Type="Int32" />
            <asp:Parameter Name="Remark" Type="String" />
            <asp:Parameter Name="Original_BelongTo" Type="String" />
            <asp:Parameter Name="Original_Level" Type="String" />
            <asp:Parameter Name="Original_HitSequence" Type="Int32" />
        </UpdateParameters>
    </asp:ObjectDataSource>
    <asp:GridView ID="GridView1" runat="server" AllowSorting="True" 
        AutoGenerateColumns="False" DataKeyNames="BelongTo,Level,HitSequence" 
        DataSourceID="ObjectDataSource1">
        <Columns>
            <asp:BoundField DataField="BelongTo" HeaderText="BelongTo" ReadOnly="True" 
                SortExpression="BelongTo" />
            <asp:BoundField DataField="Level" HeaderText="Level" ReadOnly="True" 
                SortExpression="Level" />
            <asp:BoundField DataField="HitSequence" HeaderText="HitSequence" 
                ReadOnly="True" SortExpression="HitSequence" />
            <asp:BoundField DataField="AwardQty" HeaderText="AwardQty" 
                SortExpression="AwardQty" />
            <asp:BoundField DataField="Remark" HeaderText="Remark" 
                SortExpression="Remark" />
        </Columns>
    </asp:GridView>
    <asp:GridView ID="GridView2" runat="server" AutoGenerateColumns="False" 
        DataSourceID="ObjectDataSource2">
        <Columns>
            <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
            <asp:BoundField DataField="BelongTo" HeaderText="BelongTo" 
                SortExpression="BelongTo" />
            <asp:BoundField DataField="Total" HeaderText="Total" SortExpression="Total" />
        </Columns>
    </asp:GridView>
    <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" 
        OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" 
        TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.ViewAwardGroupByLevelBelongToTableAdapter">
    </asp:ObjectDataSource>
    <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" 
        DataSourceID="ObjectDataSource3">
        <Columns>
            <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
            <asp:BoundField DataField="Total" HeaderText="Total" SortExpression="Total" />
        </Columns>
    </asp:GridView>
    <asp:ObjectDataSource ID="ObjectDataSource3" runat="server" 
        OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" 
        TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.ViewAwardGroupByLevelTableAdapter">
    </asp:ObjectDataSource>
    <p>
    </p>
</asp:Content>
