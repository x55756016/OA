using System.Diagnostics;

namespace WFTools.Utilities.Diagnostics
{
    /// <summary>
    /// TraceListener that calls delegates with a message.
    /// </summary>
    public class DelegateTraceListener : TraceListener
    {
        /// <summary>
        /// Delegate used to write information from the trace listener.
        /// </summary>
        public delegate void WriteHandler(string message);

        /// <summary>
        /// Event fired when a write is called.
        /// </summary>
        public event WriteHandler WriteDelegate;
        /// <summary>
        /// Event fired when a write line is called.
        /// </summary>
        public event WriteHandler WriteLineDelegate;

        ///<summary>
        ///When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        ///</summary>
        ///
        ///<param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void Write(string message)
        {
            if (WriteDelegate != null)
                WriteDelegate(message);
        }

        ///<summary>
        ///When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        ///</summary>
        ///
        ///<param name="message">A message to write. </param><filterpriority>2</filterpriority>
        public override void WriteLine(string message)
        {
            if (WriteLineDelegate != null)
                WriteLineDelegate(message);
        }
    }
}
