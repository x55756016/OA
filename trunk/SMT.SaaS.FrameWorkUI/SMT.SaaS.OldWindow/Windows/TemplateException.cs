namespace System.Windows
{
	public class TemplateException : Exception
	{
		public TemplateException() : base() { }
		public TemplateException(string message) : base(message) { }
		public TemplateException(string message, Exception innerException) : base(message, innerException) { }
	}
}
