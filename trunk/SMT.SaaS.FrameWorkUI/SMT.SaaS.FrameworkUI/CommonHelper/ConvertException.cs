// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace System
{
	# if !SILVERLIGHT
	[Serializable]
	# endif
	public class ConvertException : Exception
	{
		public ConvertException()
		{
		}
		
		public ConvertException(string message)
			: base(message)
		{
		}

		public ConvertException(string message, Exception innerException)
			: base(message, innerException)
		{
		}		
	}
}
