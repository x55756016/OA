using System;
using System.Runtime.Serialization;

namespace WFTools.Services.Tracking
{
    /// <summary>
    /// Exception thrown when something goes wrong in a tracking service.
    /// </summary>
    [Serializable]
    public class TrackingException : Exception
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public TrackingException() : base()
        {
        }

        /// <summary>
        /// Construct with a message.
        /// </summary>
        public TrackingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Construct with a message and inner exception.
        /// </summary>
        public TrackingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Used during serialisation.
        /// </summary>
        protected TrackingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}