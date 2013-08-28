using System;
using System.Runtime.Serialization;

namespace WFTools.Services.Persistence
{
    /// <summary>
    /// Exception thrown when something goes wrong in a persistence service.
    /// </summary>
    [Serializable]
    public class PersistenceException : Exception
    {
		/// <summary>
		/// Default Constructor.
		/// </summary>
		public PersistenceException() : base()
		{
		}

		/// <summary>
		/// Construct with a message.
		/// </summary>
		public PersistenceException(string message) : base(message)
		{
		}

		/// <summary>
		/// Construct with a message and inner exception.
		/// </summary>
		public PersistenceException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Used during serialisation.
		/// </summary>
        protected PersistenceException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
    }
}
