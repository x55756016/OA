<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="bj.aspx.cs" Inherits="Asd.Award.AwardPrintBJ" %>

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
            font-size: 16px;
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
    <form id="Form1" runat="server">
    <ul id="tab_menu">
       <%-- <li class="act">三等奖（第一轮）</li>
        <li>三等奖（第二轮）</li>
        <li>三等奖（第三轮）</li>--%>
        <li class="act">三等奖</li>
        <li>二等奖</li>
        <li>一等奖</li>
        <li>特等奖</li>
    </ul>
    <ul id="tab_content">
        <li class="act">        
            <table id="tableSZ31" runat="server">
                <tr>
                    <th colspan="2">
                        <%--深圳 三等奖（第一轮）--%>
                        深圳 三等奖
                    </th>
                </tr>
            </table>
            <table id="tableBJ31" runat="server">
                <tr>
                    <th colspan="2">
                        <%--北京 三等奖（第一轮）--%>
                        北京 三等奖
                    </th>
                </tr>
            </table>
        </li>
       <%-- <li>
            <table id="tableSZ32" runat="server">
                <tr>
                    <th colspan="2">
                        深圳 三等奖（第二轮）
                    </th>
                </tr>
            </table>
            <table id="tableBJ32" runat="server">
                <tr>
                    <th colspan="2">
                        北京 三等奖（第二轮）
                    </th>
                </tr>
            </table>
        </li>
        <li>
            <table id="tableSZ33" runat="server">
                <tr>
                    <th colspan="2">
                        深圳 三等奖（第三轮）
                    </th>
                </tr>
            </table>
            <table id="tableBJ33" runat="server">
                <tr>
                    <th colspan="2">
                        北京 三等奖（第三轮）
                    </th>
                </tr>
            </table>
        </li>--%>
        <li>
            <table id="tableSZ2" runat="server">
                <tr>
                    <th colspan="2">
                        深圳 二等奖
                    </th>
                </tr>
            </table>
            <table id="tableBJ2" runat="server">
                <tr>
                    <th colspan="2">
                        北京 二等奖
                    </th>
                </tr>
            </table>
        </li>
        <li>
            <table id="tableSZ1" runat="server">
                <tr>
                    <th colspan="2">
                        深圳 一等奖
                    </th>
                </tr>
            </table>
            <table id="tableBJ1" runat="server">
                <tr>
                    <th colspan="2">
                        北京 一等奖
                    </th>
                </tr>
            </table>
        </li>
        <li>
            <table id="tableSZ0" runat="server">
                <tr>
                    <th colspan="2">
                        深圳 特等奖
                    </th>
                </tr>
            </table>
            <table id="tableBJ0" runat="server">
                <tr>
                    <th colspan="2">
                        北京 特等奖
                    </th>
                </tr>
            </table>
        </li>
    </ul>
  </form>
</body>
</html>
