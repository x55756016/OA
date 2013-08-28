using System.Diagnostics;

namespace WFTools.Utilities.Diagnostics
{
    /// <summary>
    /// Helper class to output trace information.
    /// </summary>
    public static class TraceHelper
    {
        /// <summary>
        /// Output trace data with the method and class that it was called from.
        /// </summary>
        [Conditional("DEBUG")]
        [Conditional("TRACE")]
        public static void Trace(string format, params object [] parameters)
        {
            Trace(2, format, parameters);
        }

        /// <summary>
        /// Output trace data with the method and class that it was called from.
        /// </summary>
        [Conditional("DEBUG")]
        [Conditional("TRACE")]
        public static void Trace()
        {
            Trace(2, string.Empty);
        }

        /// <summary>
        /// Output trace data with the method and class that it was called from.
        /// Skips the specified number of frames to reach the 'real' method that
        /// asked for the trace.
        /// </summary>
        [Conditional("DEBUG")]
        [Conditional("TRACE")]
        public static void Trace(int skipFrames)
        {
            Trace(skipFrames, string.Empty);
        }

        /// <summary>
        /// Output trace data with the method and class that it was called from.
        /// Skips the specified number of frames to reach the 'real' method that
        /// asked for the trace.
        /// </summary>
        [Conditional("DEBUG")]
        [Conditional("TRACE")]
        public static void Trace(int skipFrames, string format, params object [] parameters)
        {
            StackFrame stackFrame = new StackFrame(skipFrames, true);
            // save the method name
            string className = stackFrame.GetMethod().DeclaringType.Name;
            string methodName = stackFrame.GetMethod().Name;

            if (string.IsNullOrEmpty(format))
            {
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "{0}::{1}", className, methodName));
            }
            else
            {
                string formattedMessage = string.Format(format, parameters);
                System.Diagnostics.Trace.WriteLine(string.Format(
                    "{0}::{1} - {2}", className, methodName, formattedMessage));
            }
        }
    }
}
