<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="CKEditor.NET" Namespace="CKEditor.NET" TagPrefix="CKEditor" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!--
Copyright (c) 2003-2012, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
-->
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CKEditor Samples</title>
	<link href="sample.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<form id="form1" runat="server">
	<div>
		<h1 class="samples">
			CKEditor for ASP.NET Samples Site
		</h1>
		<h2 class="samples">
			Basic Samples
		</h2>
		<ul class="samples">
			<li>
				<asp:LinkButton ID="LinkButton1" CssClass="samples" runat="server" PostBackUrl="~/FirstUse.aspx">First use</asp:LinkButton><br />
				Using CKEditor for ASP.NET Control on your website.
			</li>
			<li>
				<asp:LinkButton ID="LinkButton2" CssClass="samples" runat="server" PostBackUrl="~/ToolbarDefine.aspx">Defining custom toolbar</asp:LinkButton><br />
				Configuring CKEditor toolbar to suit your needs.
			</li>
			<li>
				<asp:LinkButton ID="LinkButton3" CssClass="samples" runat="server" PostBackUrl="~/AttachEvents.aspx">Attaching events</asp:LinkButton><br />
				Attaching functions to CKEditor events.
			</li>
			<li>
				<asp:LinkButton ID="LinkButton4" CssClass="samples" runat="server" PostBackUrl="~/SampleConfig.aspx">Sample configuration</asp:LinkButton><br />
				Configuring CKEditor on your website.
			</li>
		</ul>
		<h2 class="samples">
			Advanced Samples
		</h2>
		<ul class="samples">
			<li>
			<asp:LinkButton ID="LinkButton5" CssClass="samples" runat="server" PostBackUrl="~/SharedSpaces.aspx">Shared toolbars</asp:LinkButton><br />
				Displaying multiple editor instances that share the toolbar and/or the elements path. 
			</li>
			<li>
			<asp:LinkButton ID="LinkButton6" CssClass="samples" runat="server" PostBackUrl="~/SubmitData.aspx">Data submission</asp:LinkButton><br />
				Sending CKEditor data to the server.
			</li>
		</ul>
	</div>
	<div id="footer">
		<hr />
		<p>
			CKEditor &mdash; The text editor for the Internet &mdash; <a class="samples" href="http://ckeditor.com/">
				http://ckeditor.com</a>
		</p>
		<p id="copy">
			Copyright &copy; 2003&ndash;2011, <a class="samples" href="http://cksource.com/">CKSource</a>
			&mdash; Frederico Knabben. All rights reserved.
		</p>
	</div>
	</form>
</body>
</html>
