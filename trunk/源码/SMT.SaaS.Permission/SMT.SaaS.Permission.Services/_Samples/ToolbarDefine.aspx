<%@ Page Language="C#" AutoEventWireup="true" %>
<script language="C#" runat="server">
	protected void Page_Load(object sender, EventArgs e)
	{
		CKEditor1.config.toolbar = new object[]
			{
				new object[] { "Source", "-", "Save", "NewPage", "Preview", "-", "Templates" },
				new object[] { "Cut", "Copy", "Paste", "PasteText", "PasteFromWord", "-", "Print", "SpellChecker", "Scayt" },
				new object[] { "Undo", "Redo", "-", "Find", "Replace", "-", "SelectAll", "RemoveFormat" },
				new object[] { "Form", "Checkbox", "Radio", "TextField", "Textarea", "Select", "Button", "ImageButton", "HiddenField" },
				"/",
				new object[] { "Bold", "Italic", "Underline", "Strike", "-", "Subscript", "Superscript" },
				new object[] { "NumberedList", "BulletedList", "-", "Outdent", "Indent", "Blockquote", "CreateDiv" },
				new object[] { "JustifyLeft", "JustifyCenter", "JustifyRight", "JustifyBlock" },
				new object[] { "BidiLtr", "BidiRtl" },
				new object[] { "Link", "Unlink", "Anchor" },
				new object[] { "Image", "Flash", "Table", "HorizontalRule", "Smiley", "SpecialChar", "PageBreak", "Iframe" },
				"/",
				new object[] { "Styles", "Format", "Font", "FontSize" },
				new object[] { "TextColor", "BGColor" },
				new object[] { "Maximize", "ShowBlocks", "-", "About" }
			};
		CKEditor2.config.toolbar = new object[]
			{
				new object[] { "Bold", "Italic", "-", "NumberedList", "BulletedList", "-", "Link", "Unlink", "-", "About" },
				new object[] { "Cut", "Copy", "Paste", "PasteText", "PasteFromWord", "-", "Print", "SpellChecker", "Scayt" },
			};
		CKEditor3.config.toolbar = "Basic";
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
	<title>Custom Toolbar Definition &mdash; CKEditor for ASP.NET Sample</title>
	<link href="sample.css" rel="stylesheet" type="text/css" />
</head>
<body>
	<form id="form1" runat="server">
	<h1 class="samples">
		CKEditor for ASP.NET Sample &mdash; Defining a Custom CKEditor Toolbar
	</h1>
	<div class="description">
	<p>
		CKEditor toolbar can be adjusted to your needs. You can define a toolbar that contains
		all the buttons available in the <strong>Full</strong> toolbar definition using the
		following code:
	</p>
<pre class="samples">CKEditor1.config.toolbar = <span style="color: #0000ff">new object[]</span>
{
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;Source&quot;, &quot;-&quot;, &quot;Save&quot;, &quot;NewPage&quot;, &quot;Preview&quot;, &quot;-&quot;, &quot;Templates&quot;</span> },
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;Cut&quot;, &quot;Copy&quot;, &quot;Paste&quot;, &quot;PasteText&quot;, &quot;PasteFromWord&quot;, &quot;-&quot;, &quot;Print&quot;, &quot;SpellChecker&quot;, &quot;Scayt&quot;</span> },
    <span style="color: #0000ff">new object</span>[] {<span style="color: #a52a2a"> &quot;Undo&quot;, &quot;Redo&quot;, &quot;-&quot;, &quot;Find&quot;, &quot;Replace&quot;, &quot;-&quot;, &quot;SelectAll&quot;, &quot;RemoveFormat&quot;</span> },
    <span style="color: #0000ff">new object</span>[] {<span style="color: #a52a2a"> &quot;Form&quot;, &quot;Checkbox&quot;, &quot;Radio&quot;, &quot;TextField&quot;, &quot;Textarea&quot;, &quot;Select&quot;, &quot;Button&quot;, &quot;ImageButton&quot;, &quot;HiddenField&quot;</span> },
    <span style="color: #a52a2a">&quot;/&quot;</span>,
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;Bold&quot;, &quot;Italic&quot;, &quot;Underline&quot;, &quot;Strike&quot;, &quot;-&quot;, &quot;Subscript&quot;, &quot;Superscript&quot;</span> },
    <span style="color: #0000ff">new object</span>[] {<span style="color: #a52a2a"> &quot;NumberedList&quot;, &quot;BulletedList&quot;, &quot;-&quot;, &quot;Outdent&quot;, &quot;Indent&quot;, &quot;Blockquote&quot;, &quot;CreateDiv&quot;</span> },
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;JustifyLeft&quot;, &quot;JustifyCenter&quot;, &quot;JustifyRight&quot;, &quot;JustifyBlock&quot;</span> },
    <span style="color: #0000ff">new object</span>[] {<span style="color: #a52a2a"> &quot;BidiLtr&quot;, &quot;BidiRtl&quot;</span> },
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;Link&quot;, &quot;Unlink&quot;, &quot;Anchor&quot;</span> },
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;Image&quot;, &quot;Flash&quot;, &quot;Table&quot;, &quot;HorizontalRule&quot;, &quot;Smiley&quot;, &quot;SpecialChar&quot;, &quot;PageBreak&quot;, &quot;Iframe&quot;</span> },
    <span style="color: #a52a2a">&quot;/&quot;</span>,
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;Styles&quot;, &quot;Format&quot;, &quot;Font&quot;, &quot;FontSize&quot;</span> },
    <span style="color: #0000ff">new object</span>[] {<span style="color: #a52a2a"> &quot;TextColor&quot;, &quot;BGColor&quot; </span>},
    <span style="color: #0000ff">new object</span>[] {<span style="color: #a52a2a"> &quot;Maximize&quot;, &quot;ShowBlocks&quot;, &quot;-&quot;, &quot;About&quot;</span> }
};</pre>
	</div>
	<CKEditor:CKEditorControl ID="CKEditor1" runat="server" Height="200">
	&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
	</CKEditor:CKEditorControl>
	<br />
	<div class="description">
	<p>
		If you want to strip CKEditor toolbar to a bare minimum that suits your needs,
		limit your definition to what is needed only.
	</p>
	<pre class="samples">CKEditor2.config.toolbar = <span style="color: #0000ff">new object</span>[]
{
    <span style="color: #0000ff">new object</span>[] { <span style="color: #a52a2a">&quot;Bold&quot;, &quot;Italic&quot;, &quot;-&quot;, &quot;NumberedList&quot;, &quot;BulletedList&quot;, &quot;-&quot;, &quot;Link&quot;, &quot;Unlink&quot;, &quot;-&quot;, &quot;About&quot; </span>},
    <span style="color: #0000ff">new object</span>[] {<span style="color: #a52a2a"> &quot;Cut&quot;, &quot;Copy&quot;, &quot;Paste&quot;, &quot;PasteText&quot;, &quot;PasteFromWord&quot;, &quot;-&quot;, &quot;Print&quot;, &quot;SpellChecker&quot;, &quot;Scayt&quot;</span> },
};</pre>
	</div>
	<CKEditor:CKEditorControl ID="CKEditor2" runat="server" Height="200">
	&lt;p&gt;This is some &lt;strong&gt;sample text&lt;/strong&gt;. You are using &lt;a href="http://ckeditor.com/"&gt;CKEditor&lt;/a&gt;.&lt;/p&gt;
	</CKEditor:CKEditorControl>
	<br />
	<div class="description">
	<p>
		You can also use one of the pre-defined CKEditor toolbar configurations (<strong>Full</strong> or
		<strong>Basic</strong>) by setting the <code>config.toolbar</code> property.
	</p>
<pre class="samples">CKEditor3.config.toolbar =<span style="color: #a52a2a"> &quot;Basic&quot;</span>;</pre>
	<CKEditor:CKEditorControl ID="CKEditor3" runat="server" BasePath="~/ckeditor">
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
