<%@ Page Language="C#" AutoEventWireup="true"%>
<script language="C#" runat="server">
	protected void Page_Load(object sender, EventArgs e)
	{
		//Please note that the configuration set in the source file is more important than the one set in tags on the .aspx page.
		CKEditor1.config.uiColor = "#BFEE62";
		CKEditor1.config.language = "de";
		CKEditor1.config.enterMode = EnterMode.BR;
	}
</script>
<%@ Register Assembly="CKEditor.NET" Namespace="CKEditor.NET" TagPrefix="CKEditor" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!--
Copyright (c) 2003-2012, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
-->
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>CKEditor Configuration &mdash; CKEditor for ASP.NET Sample</title>
	<link href="sample.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<form id="form1" runat="server">
		<h1 class="samples">
			CKEditor for ASP.NET Sample &mdash; Configuration
		</h1>
		<div class="description">
		<p>
			CKEditor is a highly customizable solution. Editor settings can be configured on
			<code>.aspx</code> page in the following way:
		</p>
<pre class="samples"><span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:<span style="color: #a52a2a">CKEditorControl </span></span><span style="color: #ff0000">ID</span><span style="color: #0000ff">=&quot;CKEditor1&quot; </span><span style="color: #ff0000">BasePath</span><span style="color: #0000ff">=&quot;~/ckeditor&quot; </span><span style="color: #ff0000">runat</span><span style="color: #0000ff">=&quot;server&quot; </span><span style="color: #ff0000">UIColor</span><span style="color: #0000ff">=&quot;#BFEE62&quot; </span><span style="color: #ff0000">Language</span><span style="color: #0000ff">=&quot;de&quot; </span><span style="color: #ff0000">EnterMode</span><span style="color: #0000ff">=&quot;BR&quot;&gt;</span>
<span style="color: #0000ff">&lt;<span>/</span><span style="color: #a52a2a">CKEditor</span>:<span style="color: #a52a2a">CKEditorControl</span>&gt;</span></pre>
		<p>
			The configuration can also be placed in the source file:
		</p>
<pre class="samples"><span style="color: #006400">//Please note that the configuration set in the source file is more important than the one set in tags on the .aspx page.</span>
CKEditor1.config.uiColor = <span style="color: #a52a2a">&quot;#BFEE62&quot;</span>;
CKEditor1.config.language = <span style="color: #a52a2a">&quot;de&quot;</span>;
CKEditor1.config.enterMode = <span style="color: #2b91af">EnterMode</span>.BR;
</pre>
	</div>
	<CKEditor:CKEditorControl ID="CKEditor1" BasePath="~/ckeditor" runat="server" UIColor="#BFEE62" Language="de" EnterMode="BR"></CKEditor:CKEditorControl>
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
