<%@ Page Title="主页" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Init.aspx.cs" Inherits="Asd.Award._Init" %>
  
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
  <style>
  .btn{border-radius: 100px; border:none; width:600px; height:150px; cursor:pointer; font-family:'微软雅黑';background:#115899;
       background: -webkit-gradient(linear, left top, left bottom, from(#5ec3f3), to(#115899));
       box-shadow: 0px 3px 4px #999;
       }
  .btn:hover{ background-color:Green;color:#fff;
              background: -webkit-gradient(linear, left top, left bottom, from(#00ccff), to(#0095fe));
              }
  </style>
    <h2>
        初始化
    </h2>
    <div style="padding-bottom:500px">
         <asp:Button ID="ButtonClearAward" runat="server"   CssClass="btn"
                Text="清空中奖记录" OnClick="ButtonClearAward_Click" Font-Size="30px"  />
    </div>
    <div style="display:none">
    <hr />
        <asp:RadioButtonList ID="RadioButtonList1" runat="server">
            <asp:ListItem Selected="True" Value="BJ">北京</asp:ListItem>
            <asp:ListItem Value="SZ">深圳</asp:ListItem>
        </asp:RadioButtonList>
        <div>
            <asp:TextBox ID="TextBoxStart" runat="server"></asp:TextBox>
            <asp:TextBox ID="TextBoxEnd" runat="server"></asp:TextBox>
            <asp:Button ID="ButtonBatch" runat="server" Text="批添加入场卷" OnClick="ButtonBatch_Click" />
        </div>
        <div>
            <asp:TextBox ID="TextBoxSingle" runat="server"></asp:TextBox>
            <asp:Button ID="ButtonSingle" runat="server" Text="单个添加入场卷" OnClick="ButtonSingle_Click" />
            <br />
            <asp:Button ID="ButtonClearTicket" runat="server" Text="清空入场卷" OnClick="ButtonClearTicket_Click" />
        </div>
    </div>
</asp:Content>
