<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AwardPrint.aspx.cs" Inherits="Asd.Award.AwardPrint" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>AISIDI AWARD 2012</title>
    <script src="Scripts/jquery-1.7.min.js" type="text/javascript"></script>
    <style type="text/css">
        body
        {
            margin: 0;
            padding: 0;
            font-size: 14px;
            font-family: '宋体' ,Tahoma;
        }
        .main
        {
            overflow: auto;
        }
        .chart
        {
            border: 2px solid #999;
            border-top: 0;
            border-right: 0;
            width: 100%;
            overflow: auto;
        }
        .chart td
        {
            height: 300px;
        }
        .chart td, .chart th
        {
            text-align: center;
            position: relative;
        }
        .chart td em
        {
            position: absolute;
            bottom: 0;
            left: 0;
            display: inline-block;
            color: #000;
            text-align: center;
            padding: -30px 0 0 0;
            width: 40px;
            line-height: -300px;
            font-style: normal;
        }
        .chart td em span
        {
            position: absolute;
            margin-top: -15px;
        }
        .chart td em.sz
        {
            background-color: Red;
            margin-left: 10px;
        }
        .chart td em.bj
        {
            background-color: blue;
            margin-left: 60px;
        }
        .chart th span
        {
            position: absolute;
            margin: 10px 0 0 10px;
        }
        ul, li
        {
            list-style-type: none;
            margin: 0;
            padding: 0;
        }
        #tab_menu
        {
            margin: 5px auto;
            border-bottom: 1px solid #000;
            height: 20px;
            padding-left: 20px;
        }
        #tab_menu li
        {
            float: left;
            margin-right: 2px;
            height: 20px;
            padding: 0 5px;
            line-height: 20px;
            text-align: center;
            cursor: pointer;
        }
        #tab_menu li.act
        {
            cursor: default;
            border: 1px solid #000;
            border-bottom: none;
            font-weight: bold;
        }
        #tab_content li
        {
            display: none;
        }
        #tab_content li.act
        {
            display: block;
        }
        table
        {
            width: 280px;
            border-collapse: collapse;
            float: left;
            margin-right: 40px;
        }
        td
        {
            border: 1px solid #999;
            padding: 3px 5px;
            font-size: 18px;
            font-family: Tahoma;
        }
    </style>
    <script type="text/javascript">
        $(function () {
            $("#tab_menu li").click(function () {
                var index = $(this).index();
                $(this).addClass("act").siblings().removeClass("act");
                $("#tab_content").children().eq(index).addClass("act").siblings().removeClass("act");
            })
        })
    </script>
</head>
<body>
    <form runat="server">
    <ul id="tab_menu">
        <li class="act">三等奖（第一轮）</li>
        <li>三等奖（第二轮）</li>
        <li>三等奖（第三轮）</li>
        <li>二等奖</li>
        <li>一等奖</li>
        <li>特等奖</li>
    </ul>
    <ul id="tab_content">
        <li class="act">
            <table id="tableSZ31" runat="server">
                <tr>
                    <th colspan="2">
                        深圳
                    </th>
                </tr>
            </table>
            <table id="tableBJ31" runat="server">
                <tr>
                    <th colspan="2">
                        北京
                    </th>
                </tr>
            </table>
        </li>
        <li>三等奖（第二轮）
            <table id="tableSZ32" runat="server">
                <tr>
                    <th colspan="2">
                        深圳
                    </th>
                </tr>
            </table>
            <table id="tableBJ32" runat="server">
                <tr>
                    <th colspan="2">
                        北京
                    </th>
                </tr>
            </table>
        </li>
        <li>三等奖（第三轮）
            <table id="tableSZ33" runat="server">
                <tr>
                    <th colspan="2">
                        深圳
                    </th>
                </tr>
            </table>
            <table id="tableBJ33" runat="server">
                <tr>
                    <th colspan="2">
                        北京
                    </th>
                </tr>
            </table>
        </li>
        <li>二等奖
            <table id="tableSZ2" runat="server">
                <tr>
                    <th colspan="2">
                        深圳
                    </th>
                </tr>
            </table>
            <table id="tableBJ2" runat="server">
                <tr>
                    <th colspan="2">
                        北京
                    </th>
                </tr>
            </table>
        </li>
        <li>一等奖
            <table id="tableSZ1" runat="server">
                <tr>
                    <th colspan="2">
                        深圳
                    </th>
                </tr>
            </table>
            <table id="tableBJ1" runat="server">
                <tr>
                    <th colspan="2">
                        北京
                    </th>
                </tr>
            </table>
        </li>
        <li>特等奖
            <table id="tableSZ0" runat="server">
                <tr>
                    <th colspan="2">
                        深圳
                    </th>
                </tr>
            </table>
            <table id="tableBJ0" runat="server">
                <tr>
                    <th colspan="2">
                        北京
                    </th>
                </tr>
            </table>
        </li>
    </ul>
    <%--
    <div style="float: left; width: 330px;">
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" DeleteMethod="Delete"
            InsertMethod="Insert" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData"
            TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.TmpAwardTableAdapter" UpdateMethod="Update">
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
        <asp:GridView ID="GridView1" runat="server" AllowSorting="True" AutoGenerateColumns="False"
            DataKeyNames="TicketNO" DataSourceID="ObjectDataSource1" Width="328px">
            <Columns>
                <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
                <asp:BoundField DataField="TicketNO" HeaderText="TicketNO" ReadOnly="True" SortExpression="TicketNO" />
                <asp:BoundField DataField="UpdateTime" HeaderText="UpdateTime" 
                    SortExpression="UpdateTime" Visible="False" />
                <asp:BoundField DataField="Remark" HeaderText="Remark" SortExpression="Remark" />
            </Columns>
        </asp:GridView>
    </div>
    <div style="float: left;">
        <div style="margin: 0 10px; display: inline-block;" id="s2">
            <asp:GridView ID="GridView2" runat="server" AutoGenerateColumns="False" DataSourceID="ObjectDataSource2">
                <Columns>
                    <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
                    <asp:BoundField DataField="BelongTo" HeaderText="BelongTo" SortExpression="BelongTo" />
                    <asp:BoundField DataField="Total" HeaderText="Total" SortExpression="Total" />
                </Columns>
            </asp:GridView>
            <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" OldValuesParameterFormatString="original_{0}"
                SelectMethod="GetData" TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.ViewOnGoingGroupByLevelBelongToTableAdapter">
            </asp:ObjectDataSource>
        </div>
        <div style="display: inline-block; margin: 0 10px; vertical-align: top;" id="s3">
            <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" DataSourceID="ObjectDataSource3">
                <Columns>
                    <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
                    <asp:BoundField DataField="Total" HeaderText="Total" SortExpression="Total" />
                </Columns>
            </asp:GridView>
            <asp:ObjectDataSource ID="ObjectDataSource3" runat="server" OldValuesParameterFormatString="original_{0}"
                SelectMethod="GetData" TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.ViewOnGoingGroupByLevelTableAdapter">
            </asp:ObjectDataSource>
        </div>
    </div>--%>
    </form>
</body>
</html>
