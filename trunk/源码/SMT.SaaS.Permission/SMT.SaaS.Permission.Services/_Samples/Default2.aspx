<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default2.aspx.cs" Inherits="Default2" %>
<%@ Register Assembly="CKEditor.NET" Namespace="CKEditor.NET" TagPrefix="CKEditor" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <CKEditor:CKEditorControl ID="CKEditor1" BasePath="~/ckeditor" runat="server" Height="500"/>
    <br />
    <asp:Button ID="Button1" runat="server" Text="保存文件" onclick="Button1_Click"></asp:Button>
    </div>
    <asp:Button ID="Button2" runat="server" onclick="Button2_Click" Text="打开文件" />
    </form>
</body>
</html>
