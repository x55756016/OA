using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Xml;

namespace WFTools.Utilities.Workflow
{
    /// <summary>
    /// Helper class used for retrieving useful information
    /// about a workflow.
    /// </summary>
    public static class WorkflowInstanceHelper
    {
        /// <summary>
        /// Indicates whether a workflow instance is XOML-based or
        /// compiled from a DLL.
        /// </summary>
        /// <param name="rootActivity">
        /// Root <see cref="Activity" /> of the workflow instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the workflow is XOML-based, <c>false</c> otherwise.
        /// </returns>
        public static bool IsXomlWorkflow(Activity rootActivity)
        {
            //HACK
            // retrieve the 'WorkflowXamlMarkupProperty' dependency property
            // from the activity using reflection, must be a better way of doing this?!
            // this method of doing things is stolen from System.Workflow using
            // reflector...
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
            FieldInfo fieldInfo = typeof (Activity).GetField("WorkflowXamlMarkupProperty", bindingFlags);
            DependencyProperty dependencyProperty = (DependencyProperty)fieldInfo.GetValue(null);
            string workflowXaml = rootActivity.GetValue(dependencyProperty) as string;
            if (!string.IsNullOrEmpty(workflowXaml))
                return true;

            return false;
        }

        /// <summary>
        /// Create a XOML document representing the specified object.
        /// </summary>
        /// <param name="objectToSerialise">
        /// Object to generate a XOML document from.
        /// </param>
        /// <returns>
        /// String containing the XOML document for the specified object.
        /// </returns>
        public static string GetXomlDocument(object objectToSerialise)
        {
            using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (XmlWriter xmlWriter = createXmlWriter(stringWriter))
                {
                    new WorkflowMarkupSerializer().Serialize(xmlWriter, objectToSerialise);
                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Helper method used to manually execute an workflow instance whilst handling
        /// any exceptions that are thrown during execution.
        /// </summary>
        /// <param name="instanceId">
        /// <see cref="Guid" /> representing the workflow instance.
        /// </param>
        /// <param name="workflowRuntime">
        /// A <see cref="WorkflowRuntimeWrapperBase" /> which wraps the real <see cref="WorkflowRuntime" />.
        /// </param>
        /// <remarks>
        /// This method relies on an instance of the <see cref="ManualWorkflowSchedulerService" />
        /// as the thrown exception is handled synchrously.
        /// </remarks>
        public static void ManualRunWithErrorHandling(Guid instanceId, WorkflowRuntime workflowRuntime)
        {
            ManualWorkflowSchedulerService scheduler = workflowRuntime.GetService<ManualWorkflowSchedulerService>();
            if (scheduler == null)
                throw new ArgumentException("No ManualWorkflowSchedulerService associated with this runtime.", "workflowRuntime");

            Exception workflowException = null;
            EventHandler<WorkflowTerminatedEventArgs> terminatedHandler = null;
            terminatedHandler =
                delegate(object sender, WorkflowTerminatedEventArgs e)
                {
                    if (e.WorkflowInstance.InstanceId == instanceId && e.Exception != null)
                    {
                        workflowRuntime.WorkflowTerminated -= terminatedHandler;

                        workflowException = e.Exception;
                    }
                };
            workflowRuntime.WorkflowTerminated += terminatedHandler;

            ErrorHandlingService errorHandlingService = workflowRuntime.GetService<ErrorHandlingService>();
            if (errorHandlingService != null)
            {
                EventHandler<ErrorHandlingEventArgs> errorHandler = null;
                errorHandler =
                    delegate(object sender, ErrorHandlingEventArgs e)
                        {
                            if (e.InstanceId == instanceId && e.Exception != null)
                            {
                                errorHandlingService.Error -= errorHandler;

                                workflowException = e.Exception;
                            }
                        };
                errorHandlingService.Error += errorHandler;
            }

            scheduler.RunWorkflow(instanceId);

            if (workflowException != null)
                throw workflowException;
        }

        private static XmlWriterSettings createXmlWriterSettings()
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "\t";
            xmlWriterSettings.OmitXmlDeclaration = true;
            xmlWriterSettings.CloseOutput = true;

            return xmlWriterSettings;
        }

        private static XmlWriter createXmlWriter(TextWriter textWriter)
        {
            return XmlWriter.Create(textWriter, createXmlWriterSettings());
        }
    }
}