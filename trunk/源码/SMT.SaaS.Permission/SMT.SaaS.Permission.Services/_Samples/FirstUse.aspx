<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="CKEditor.NET" Namespace="CKEditor.NET" TagPrefix="CKEditor" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!--
Copyright (c) 2003-2012, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
-->
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>First Use &mdash; CKEditor for ASP.NET Sample</title>
	<link href="sample.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<form id="form1" runat="server">
	<h1 class="samples">
		CKEditor for ASP.NET Sample &mdash; Adding the CKEditor for ASP.NET Control to a Page
	</h1>
	<div class="description">
	<p>
		If you want use the CKEditor for ASP.NET Control, you must add references to the project and
		register the control.<br />
		If you only intend to use the control on a single page, you can add a <code>Register</code>
		section to its source code:
	</p>
	<pre class="samples"><span style="background-color: #ffff00">&lt;<font>%</font></span><span style="color: #0000ff">@</span><span style="color: #a52a2a">Register </span><span style="color: #ff0000">Assembly</span><span style="color: #0000ff">=&quot;CKEditor.NET&quot; </span><span style="color: #ff0000">Namespace</span><span style="color: #0000ff">=&quot;CKEditor.NET&quot; </span><span style="color: #ff0000">TagPrefix</span><span style="color: #0000ff">=&quot;CKEditor&quot;</span><span style="background-color: #ffff00">%&gt;</span></pre>
	<p>
		If you intend to use the control on multiple pages, you can add the <code>Register</code>
		section in <code>web.config</code>. Insert the following code into the <code><span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">system.web</span><span style="color: #0000ff">&gt;&lt;</span><span style="color: #a52a2a">pages</span><span style="color: #0000ff">&gt;&lt;</span><span style="color: #a52a2a">controls</span><span style="color: #0000ff">&gt;</span></code> section:
	</p>
	<pre class="samples"><span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">add </span><span style="color: #ff0000">tagPrefix</span><span style="color: #0000ff">=&quot;CKEditor&quot;</span><span style="color: #ff0000"> assembly</span><span style="color: #0000ff">=&quot;CKEditor.NET&quot;</span><span style="color: #ff0000"> namespace</span><span style="color: #0000ff">=&quot;CKEditor.NET&quot;/&gt;</span></pre>
	<p>
		To insert the CKEditor for ASP.NET Control into a web page, use the following code:
	</p>
	<pre class="samples"><span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl </span><span style="color: #ff0000">ID</span><span style="color: #0000ff">=&quot;CKEditor1&quot; </span><span style="color: #ff0000">BasePath</span><span style="color: #0000ff">=&quot;~/ckeditor&quot; </span><span style="color: #ff0000">runat</span><span style="color: #0000ff">=&quot;server&quot;&gt;&lt;/</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl</span><span style="color: #0000ff">&gt;</span></pre>
	<p>The CKEditor instance below was inserted using the second method.</p>
	</div>
	<div>
		<CKEditor:CKEditorControl ID="CKEditor1" runat="server" Height="200" BasePath="~/ckeditor">
		&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
		</CKEditor:CKEditorControl>
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
