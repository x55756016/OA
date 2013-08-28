using System;
using System.Workflow.Activities;
using System.Workflow.Runtime;

namespace WFTools.Utilities.Workflow
{
    /// <summary>
    /// Event arguments used for error handling performed by a <see cref="ErrorHandlingService" />.
    /// </summary>
    [Serializable]
    public class ErrorHandlingEventArgs : ExternalDataEventArgs
    {
        public ErrorHandlingEventArgs(Guid instanceId, Exception exception) : base(instanceId)
        {
            this.exception = exception;
            this.WaitForIdle = true;
        }

        private readonly Exception exception;
        /// <summary>
        /// The exception raised.
        /// </summary>
        public Exception Exception
        {
            get { return exception; }
        }
    }

    /// <summary>
    /// Abstract implementation of <see cref="IErrorHandlingService" /> that can serve
    /// as a base class for other more functional services.
    /// </summary>
    public abstract class ErrorHandlingService : IErrorHandlingService
    {
        /// <summary>
        /// Event raised to the hosting application when an error occurs.
        /// </summary>
        public event EventHandler<ErrorHandlingEventArgs> Error;

        /// <summary>
        /// Raises an exception to the hosting application.
        /// </summary>
        /// <param name="exception">
        /// The <see cref="Exception" /> to raise.
        /// </param>
        public void RaiseError(Exception exception)
        {
            if (Error != null)
                Error(this, new ErrorHandlingEventArgs(WorkflowEnvironment.WorkflowInstanceId, exception));
        }
    }
}
