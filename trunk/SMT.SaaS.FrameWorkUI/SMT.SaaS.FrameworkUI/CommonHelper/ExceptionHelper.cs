// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

# region Using Directives

using System.IO;
using System.Linq;

# if !PocketPC && !SILVERLIGHT
using System.Web.Services.Protocols;
using System.ServiceModel;
# endif

# endregion

namespace System
{
	public static class ExceptionHelper
	{
		# region SerializeToString

		public static string SerializeToString(Exception exception)
		{
			string message = string.Empty;
			var tabs = string.Empty;

			var prepare = new Func<string, string>(s =>
				s.Replace(Environment.NewLine, Environment.NewLine + tabs));

			while (exception != null)
			{
				message = string.Format("{0}{1}Message: {2}\r\n", message, tabs, prepare(exception.Message));

                # if !PocketPC && !SILVERLIGHT

				if (exception is SoapException)
				{
					var soapException = (SoapException)exception;

					message = string.Format("InnerLevel: {0}\r\n\tSoapException.Detail.InnerText: {1}\r\n\r\n\t{2}",
						index, soapException.Detail.InnerText, message);
				}
                else if (exception is FaultException<ExceptionDetail>)
                {
                    var faultException = (FaultException<ExceptionDetail>)exception;
                    message = string.Format("InnerLevel: {0}\r\n\tFaultException<ExceptionDetail>.Detail.InnerException.Message: {1}\r\n\r\n\t{2}",
                        index, faultException.Detail.InnerException.Message, message);

                    message = string.Format("InnerLevel: {0}\r\n\tFaultException<ExceptionDetail>.Detail.InnerException.StackTrace: {1}\r\n\r\n\t{2}",
                        index, faultException.Detail.InnerException.StackTrace, message);
                }
				
				# endif

                if (exception.StackTrace != null)
				{
					message = string.Format("{0}{1}StackTrace:\r\n{1}{2}\r\n", message, tabs, prepare(exception.StackTrace));
				}

				exception = exception.InnerException;

				if (exception != null)
				{
					message = string.Format("{0}{1}Inner Exception:\r\n", message, tabs);
				}

				tabs += '\t';
			}

			return message;
		} 

		# endregion

		# region Serialize

		public static byte[] Serialize(Exception exception)
		{
			var sException = ExceptionHelper.SerializeToString(exception);

			var stream = new MemoryStream();
			var writer = new BinaryWriter(stream);

			writer.Write(sException);
			
			return stream.ToArray();
		}

		# endregion

		# region DeserializeToString

		public static string DeserializeToString(byte[] data)
		{
			return ExceptionHelper.DeserializeToString(data, 0);
		}
		public static string DeserializeToString(byte[] data, int offset)
		{
			var stream = new MemoryStream(data);
			var reader = new BinaryReader(stream);

			stream.Position = offset;

			var result = reader.ReadString();
			
			return result;
		}

		# endregion
	}
}
