using System;
using System.Workflow.Activities;

namespace WFTools.Utilities.Workflow
{
    /// <summary>
    /// Interface used for raising exceptions to a hosting application.
    /// </summary>
    [ExternalDataExchange]
    public interface IErrorHandlingService
    {
        /// <summary>
        /// Raises an exception to the hosting application.
        /// </summary>
        /// <param name="exception">
        /// The <see cref="Exception" /> to raise.
        /// </param>
        void RaiseError(Exception exception);
    }
}