namespace System.Windows
{
	public class TemplatePartMissingException : TemplateException
	{
		//public TemplatePartMissingException() : base() { }
		//public TemplatePartMissingException(string message) : base(message) { }
		public TemplatePartMissingException(string templatePartname, Type controlType)
			: base(string.Format("There is missing an \"{0}\" element named in template of \"{1}\" control",
			templatePartname, controlType))
		{
		}
		//public TemplatePartMissingException(string message, System.Exception inner) : base(message, inner) { }
	}
}
