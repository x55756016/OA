<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="CKEditor.NET" Namespace="CKEditor.NET" TagPrefix="CKEditor" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<!--
Copyright (c) 2003-2012, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
-->
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Shared Toolbars &mdash; CKEditor for ASP.NET Sample</title>
	<link href="sample.css" rel="stylesheet" type="text/css" />
</head>
<body>
<form id="form1" style="height:600px;" runat="server">
	<h1 class="samples">
		CKEditor for ASP.NET Sample &mdash; Shared Toolbars 
	</h1>
	<div class="description">
	<p>
		This sample shows how to configure multiple CKEditor for ASP.NET instances to share some parts of the interface. You can choose to share the toolbar, the elements path, or both.
	</p>
<pre class="samples"><span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl </span><span style="color: #ff0000">ID</span><span style="color: #0000ff">=&quot;CKEditor1&quot; </span><span style="color: #ff0000">SharedSpacesTop</span><span style="color: #0000ff">=&quot;divTopShared&quot; </span><span style="color: #ff0000">SharedSpacesBottom</span><span style="color: #0000ff">=&quot;divBottomShared&quot; </span><span style="color: #ff0000">runat</span><span style="color: #0000ff">=&quot;server&quot;&gt;&lt;/</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl</span><span style="color: #0000ff">&gt;</span>
	
<span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl </span><span style="color: #ff0000">ID</span><span style="color: #0000ff">=&quot;CKEditor2&quot; </span><span style="color: #ff0000">SharedSpacesTop</span><span style="color: #0000ff">=&quot;divTopShared&quot; </span><span style="color: #ff0000">SharedSpacesBottom</span><span style="color: #0000ff">=&quot;divBottomShared&quot; </span><span style="color: #ff0000">runat</span><span style="color: #0000ff">=&quot;server&quot;&gt;&lt;/</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl</span><span style="color: #0000ff">&gt;</span>

<span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl </span><span style="color: #ff0000">ID</span><span style="color: #0000ff">=&quot;CKEditor3&quot; </span><span style="color: #ff0000">SharedSpacesTop</span><span style="color: #0000ff">=&quot;divTopShared&quot; </span><span style="color: #ff0000">runat</span><span style="color: #0000ff">=&quot;server&quot;&gt;&lt;/</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl</span><span style="color: #0000ff">&gt;</span>

<span style="color: #0000ff">&lt;</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl </span><span style="color: #ff0000">ID</span><span style="color: #0000ff">=&quot;CKEditor4&quot; </span><span style="color: #ff0000">runat</span><span style="color: #0000ff">=&quot;server&quot;&gt;&lt;/</span><span style="color: #a52a2a">CKEditor</span><span style="color: #0000ff">:</span><span style="color: #a52a2a">CKEditorControl</span><span style="color: #0000ff">&gt;</span>
</pre>
	</div>
	<div id="divTopShared" style="margin:20 0 0 20;">
	</div>
	<br />
	<div style="height:500px; overflow: scroll; margin:20 0 0 20; border: thin solid black; padding:15px 10px 15px 10px;">
	Editor 1 (uses shared toolbar and elements path):
	<CKEditor:CKEditorControl ID="CKEditor1" SharedSpacesTop="divTopShared" SharedSpacesBottom="divBottomShared" runat="server" Height="200">
		&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
	</CKEditor:CKEditorControl>
	<br />
	Editor 2 (uses shared toolbar and elements path):
	<CKEditor:CKEditorControl ID="CKEditor2" SharedSpacesTop="divTopShared" SharedSpacesBottom="divBottomShared" runat="server" Height="200">
		&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
	</CKEditor:CKEditorControl>
	<br />
	Editor 3 (uses shared toolbar only):
	<CKEditor:CKEditorControl ID="CKEditor3" SharedSpacesTop="divTopShared" runat="server" Height="200">
		&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
	</CKEditor:CKEditorControl>
	<br />
	Editor 4 (no shared spaces):
	<CKEditor:CKEditorControl ID="CKEditor4" runat="server" Height="200">
		&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
	</CKEditor:CKEditorControl>
	</div>
	<br />
	<div id="divBottomShared">
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
