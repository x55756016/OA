<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Award.aspx.cs" Inherits="Asd.Award.Award" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" src="Scripts/jquery-1.7.min.js"></script>
<style>
    body{ margin:0; padding:0;}
    table,td{border-spacing:0; border-collapse:collapse;}
    .main{}
    .chart{  border:2px solid #999; border-top:0;border-right:0;  width:100%; overflow:auto; }
    .chart td{height:300px; width:50px; padding:0; margin:0;}
    .chart td:first-child{ border:none;}
    .c2{ height:20px; border:none;}
    .c2 td{ height:20px !important;border-left:1px dotted #ccc; width:100px !important;}
    .c2 td:first-child{ }
    .chart td,.chart th{ text-align:center; vertical-align:bottom;}
    .chart td em{ display:inline-block;color:#000; text-align:center; width:30px; margin:0; padding:0; font-style:normal;}
    .chart td em span{ position: absolute; margin-top:-18px; margin-left:8px;}
    .chart td em.sz{ background-color:Red;margin-left:10px;background: -webkit-gradient(linear, left top, left bottom, from(red), to(#ffd14a));}
    .chart td em.bj{ background-color:blue; margin-left:-23px; background: -webkit-gradient(linear, left top, left bottom, from(blue), to(#00ccff));}
   
    .clearf{clear: both;display: block;overflow: hidden;visibility: hidden;width: 0;height: 0;}
</style>
 <div style="float:left; width:330px;margin-bottom:80px;">
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

    <asp:GridView ID="GridView1" runat="server" AllowSorting="True" 
        AutoGenerateColumns="False" DataKeyNames="TicketNO" 
        DataSourceID="ObjectDataSource1" Width="328px">
        <Columns>
            <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
            <asp:BoundField DataField="TicketNO" HeaderText="TicketNO" ReadOnly="True" 
                SortExpression="TicketNO" />
            <asp:BoundField DataField="UpdateTime" HeaderText="UpdateTime" 
                SortExpression="UpdateTime" />
            <asp:BoundField DataField="Remark" HeaderText="Remark" 
                SortExpression="Remark" />
        </Columns>
    </asp:GridView>
    <div class="clearf"></div>
</div>
<div style="float:left;">
    <div style="margin:0 10px; display:inline-block;" id="s2">
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
        TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.ViewOnGoingGroupByLevelBelongToTableAdapter">
    </asp:ObjectDataSource>
</div>
    <div style="display:inline-block; margin:0 10px; vertical-align:top;" id="s3">
    <asp:GridView ID="GridView3" runat="server" AutoGenerateColumns="False" 
        DataSourceID="ObjectDataSource3">
        <Columns>
            <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
            <asp:BoundField DataField="Total" HeaderText="Total" SortExpression="Total" />
        </Columns>
    </asp:GridView>

    <asp:ObjectDataSource ID="ObjectDataSource3" runat="server" 
        OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" 
        TypeName="Asd.Award.Domain.AwardDataSetTableAdapters.ViewOnGoingGroupByLevelTableAdapter">
    </asp:ObjectDataSource>
</div>
    <script type="text/javascript">
        $(function () {
           
            var $em = $(".chart em");

            var sz_all = 0, bj_all = 0;
            $("#s2").find("tr").each(function () {
                var $td1 = $(this).children().eq(0), $td2 = $(this).children().eq(1), $td3 = $(this).children().eq(2);
                var txt = $td1.text();
                if (txt == 3) {
                    if ($td2.text() == 'SZ') {
                        $em.eq(2).children().text($td3.text());
                        sz_all += parseInt($td3.text());
                    } else {
                        $em.eq(3).children().text($td3.text());
                        bj_all += parseInt($td3.text());
                    }
                } else if (txt == 2) {
                    if ($td2.text() == 'SZ') {
                        $em.eq(4).children().text($td3.text());
                        sz_all += parseInt($td3.text());
                    } else {
                        $em.eq(5).children().text($td3.text());
                        bj_all += parseInt($td3.text());
                    }
                } else if (txt == 1) {
                    if ($td2.text() == 'SZ') {
                        $em.eq(6).children().text($td3.text());
                        sz_all += parseInt($td3.text());
                    } else {
                        $em.eq(7).children().text($td3.text());
                        bj_all += parseInt($td3.text());
                    }
                } else if (txt == 0) {
                    if ($td2.text() == 'SZ') {
                        $em.eq(8).children().text($td3.text());
                        sz_all += parseInt($td3.text());
                    } else {
                        $em.eq(9).children().text($td3.text());
                        bj_all += parseInt($td3.text());
                    }
                }
                $em.eq(0).children().text(sz_all);
                $em.eq(1).children().text(bj_all);

            })

            $("em").each(function () {
                var h = ($(this).children().text() / 1600) * 4000;
                $(this).height(h);
            });
        });
</script>
    <div style="margin:10px; width:500px; height:300px;">
    <table class='chart' cellpadding='0' cellspacing='0' height="300">
        <tbody>
            <tr>
                <td>
                    <em class="sz"><span></span></em></td><td>
                    <em class="bj"><span></span></em>
                </td>
                <td style="border-left:1px dotted #ccc">
                    <em class="sz"><span></span></em></td><td>
                    <em class="bj"><span></span></em>
                </td>
                <td style="border-left:1px dotted #ccc">
                    <em class="sz"><span></span></em></td><td>
                    <em class="bj"><span></span></em>
                </td>
                <td style="border-left:1px dotted #ccc">
                    <em class="sz"><span></span></em></td><td>
                    <em class="bj"><span></span></em>
                </td>
                <td style="border-left:1px dotted #ccc">
                    <em class="sz"><span></span></em></td><td>
                    <em class="bj"><span></span></em>
                </td>
            </tr>
        </tbody>        
    </table>
    <table class='chart c2' cellpadding='0' cellspacing='0' height="20">
    <tbody>
        <tr>
                <td><span>获奖比例</span></td>                
                <td><span>三等奖</span></td>
                <td><span>二等奖</span></td>
                <td><span>一等奖</span></td>   
                <td><span>特等奖</span></td>  
        </tr>
        <tr>
         <td colspan="5"><span style="margin-top:30px;"><strong style=" color:red;">红色：深圳</strong>　<strong style=" color:blue;">蓝色：北京</strong></span></td>
       </tr>
    </tbody>
    </table>
    <div class="clearf"></div>
</div>
    <div class="clearf"></div>
</div>

</asp:Content>
