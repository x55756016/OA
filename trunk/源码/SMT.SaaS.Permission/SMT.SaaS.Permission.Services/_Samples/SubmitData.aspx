<%@ Page Language="C#" AutoEventWireup="true" %>
<script language="C#" runat="server">
protected void Page_Load(object sender, EventArgs e)
{
	//Add CKEditor output HTML to the <pre> tag.
	preCKEditorData.InnerText = CKEditor1.Text; 
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
	<title>Data Submission &mdash; CKEditor for ASP.NET Sample</title>
	<link href="sample.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<form id="form1" runat="server">
	<h1 class="samples">
		CKEditor for ASP.NET Sample &mdash; Data Submission
	</h1>
	<div class="description">
	<p>
		If you want see the HTML output from CKEditor, use the <code>Text</code> property:
	</p>
	<pre class="samples"><span style="color: #0000ff">protected void</span> Page_Load(<span style="color: #0000ff">object</span> sender, <span style="color: #2b91af">EventArgs</span> e)
{
    <span style="color: #008000">//Add CKEditor output HTML to the &lt;pre&gt; tag.</span>
    preCKEditorData.InnerText = CKEditor1.Text; 
}</pre>
	</div>
	<div>
		<CKEditor:CKEditorControl ID="CKEditor1" BasePath="~/ckeditor" runat="server" Height="200">
&lt;p&gt;
	This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;</CKEditor:CKEditorControl>
	</div>
	<input type="submit" value="Submit" />
	<div>
		<pre runat="server" id="preCKEditorData" class="samples"></pre>
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
