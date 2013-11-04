<%@ Page Language="C#" AutoEventWireup="true" %>

<script language="C#" runat="server">
	protected void Page_Load(object sender, EventArgs e)
	{
		CKEditor1.CKEditorInstanceEventHandler = new System.Collections.Generic.List<object>();
		CKEditor1.CKEditorInstanceEventHandler.Add(new object[] { "instanceReady", "function (evt) { alert('Event Handler attached on CKEditorInstanceEventHandler to editor: ' + evt.editor.name);}" });
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
	<title>Attaching Events &mdash; CKEditor for ASP.NET Sample</title>
	<link href="sample.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<form id="form1" runat="server">
		<h1 class="samples">
			CKEditor for ASP.NET Sample &mdash; Attaching Events
		</h1>
		<div class="description">
		<p>
			In order to attach a function to an event in a single editor instance, use the following method:
		</p>
<pre class="samples">CKEditor1.CKEditorInstanceEventHandler = <span style="color: #0000ff">new</span> System.Collections.Generic.<span style="color: #2b91af">List</span>&lt;<span style="color: #0000ff">object</span>&gt;();
CKEditor1.CKEditorInstanceEventHandler.Add(<span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;instanceReady&quot;</span>
	, <span style="color: #a52a2a">&quot;function (evt) { alert(&#39;Event Handler attached on CKEditorInstanceEventHandlerto editor: &#39; + evt.editor.name);}&quot; </span>});
</pre>
		<p>
			Attaching a function to an event in all editor instances (use e.g. in <code>Global.asax</code>)
			can be achieved in the following way:
		</p>
<pre class="samples">CKEditor.NET.<span style="color: #2b91af">CKEditorConfig</span>.GlobalConfig.CKEditorInstanceEventHandler.Add(<span style="color: #0000ff">new object</span>[] 
	{<span style="color: #a52a2a"> &quot;mode&quot;</span>,<span style="color: #a52a2a"> function (evt) { alert(&#39;Events attached to all instances. (Events: \&quot;mode\&quot;)&#39;);}&quot; </span>});</pre>
		<p>
			To attach a function to CKEditor events, use the following code:
		</p>
<pre class="samples">CKEditor1.CKEditorEventHandler.Add(<span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;instanceReady&quot;</span>
	, <span style="color: #a52a2a">&quot;function (evt) { alert(&#39;Events attached to CKEditor. (Events: \&quot;instanceReady\&quot;)&#39;);}&quot; </span>});</pre>
		<p>
			The sample editor below uses the first method to attach a function to an event in this instance only.
		</p>
		</div>
		<CKEditor:CKEditorControl ID="CKEditor1" runat="server" Height="200">
		&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
		</CKEditor:CKEditorControl>
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
