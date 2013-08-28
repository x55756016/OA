using System;
using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services.Tracking.Ado.Oracle
{
    /// <summary>
    /// Oracle specific implementation of <see cref="ITrackingNameResolver" />.
    /// </summary>
    public class OracleTrackingNameResolver : ITrackingNameResolver
    {
        /// <summary>
        /// Resolve <see cref="TrackingCommandName" /> to their database-specific command text.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="TrackingCommandName" /> indicating which command needs to be resolved.
        /// </param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "Uses a large switch statement.")]
        public string ResolveCommandName(TrackingCommandName commandName)
        {
            switch (commandName)
            {
                case TrackingCommandName.DeleteInstanceTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.DeleteTrackingProfile";
                case TrackingCommandName.DeleteTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.DeleteTrackingProfile";
                case TrackingCommandName.GetCurrentDefaultTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.GetDefaultTrackingProfile";
                case TrackingCommandName.GetDefaultTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.GetDefaultTrackingProfile";
                case TrackingCommandName.GetInstanceTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.GetTrackingProfile";
                case TrackingCommandName.GetTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.GetTrackingProfile";
                case TrackingCommandName.GetTrackingProfileChanges:
                    return "WORKFLOW_TRACKING_PKG.GetTrackingProfileChanges";
                case TrackingCommandName.InsertActivities:
                    return "WORKFLOW_TRACKING_PKG.InsertActivities";
                case TrackingCommandName.InsertActivityAddedActions:
                    return "WORKFLOW_TRACKING_PKG.InsertActivityAddedActions";
                case TrackingCommandName.InsertActivityRemovedActions:
                    return "WORKFLOW_TRACKING_PKG.InsertActivityRemovedActions";
                case TrackingCommandName.InsertActivityTrackingRecords:
                    return "WORKFLOW_TRACKING_PKG.InsertActivityTrackingRecords";
                case TrackingCommandName.InsertEventAnnotations:
                    return "WORKFLOW_TRACKING_PKG.InsertEventAnnotations";
                case TrackingCommandName.InsertTrackingDataItems:
                    return "WORKFLOW_TRACKING_PKG.InsertTrackingDataItems";
                case TrackingCommandName.InsertTrackingDataAnnotations:
                    return "WORKFLOW_TRACKING_PKG.InsertTrackingDataAnnotations";
                case TrackingCommandName.InsertUserTrackingRecords:
                    return "WORKFLOW_TRACKING_PKG.InsertUserTrackingRecords";
                case TrackingCommandName.InsertWorkflow:
                    return "WORKFLOW_TRACKING_PKG.InsertWorkflow";
                case TrackingCommandName.InsertWorkflowInstance:
                    return "WORKFLOW_TRACKING_PKG.InsertWorkflowInstance";
                case TrackingCommandName.InsertWorkflowTrackingRecords:
                    return "WORKFLOW_TRACKING_PKG.InsertWorkflowTrackingRecords";
                case TrackingCommandName.UpdateDefaultTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.UpdateDefaultTrackingProfile";
                case TrackingCommandName.UpdateInstanceTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.UpdateTrackingProfile";
                case TrackingCommandName.UpdateTrackingProfile:
                    return "WORKFLOW_TRACKING_PKG.UpdateTrackingProfile";
            }

            throw new NotSupportedException(RM.Get_Error_CommandNamesNotSupported());
        }

        /// <summary>
        /// Resolve <see cref="TrackingParameterName" /> to their database-specific parameter name.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="TrackingCommandName" /> indicating which command the parameter
        /// name needs to be resolved for.
        /// </param>
        /// <param name="parameterName">
        /// A <see cref="TrackingParameterName" /> indicating which parameter needs to be resolved.
        /// </param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "Uses a large switch statement.")]
        public string ResolveParameterName(TrackingCommandName commandName, TrackingParameterName parameterName)
        {
            switch (parameterName)
            {
                case TrackingParameterName.ActivityXml:
                    return "p_ACTIVITY_XML";

                case TrackingParameterName.ActivityXoml:

                    if (commandName == TrackingCommandName.InsertActivityAddedActions)
                        return "p_ADDED_ACTIVITY_ACTION";
                    else if (commandName == TrackingCommandName.InsertActivityRemovedActions)
                        return "p_REMOVED_ACTIVITY_ACTION";

                    break;

                case TrackingParameterName.ActivityInstanceId:
                    return "p_ACTIVITY_INSTANCE_ID";

                case TrackingParameterName.ActivityStatusId:
                    return "p_ACTIVITY_STATUS_ID";

                case TrackingParameterName.ActivityStatusEventId:
                    return "p_ACTIVITY_STATUS_EVENT_ID";

                case TrackingParameterName.Annotation:
                    return "p_ANNOTATION";

                case TrackingParameterName.AssemblyFullName:
                    return "p_ASSEMBLY_FULL_NAME";

                case TrackingParameterName.CallerContextGuid:
                    return "p_CALLER_CONTEXT_GUID";

                case TrackingParameterName.CallerInstanceId:
                    return "p_CALLER_INSTANCE_ID";

                case TrackingParameterName.CallerParentContextGuid:
                    return "p_CALLER_PARENT_CONTEXT_GUID";

                case TrackingParameterName.CallPath:
                    return "p_CALL_PATH";

                case TrackingParameterName.ContextGuid:
                    return "p_CONTEXT_GUID";

                case TrackingParameterName.CreateDefault:
                    return "p_CREATE_DEFAULT";

                case TrackingParameterName.DataBlob:
                    return "p_DATA_BLOB";

                case TrackingParameterName.DataString:
                    return "p_DATA_STR";

                case TrackingParameterName.DataNonSerialisable:
                    return "p_DATA_NON_SERIALISABLE";

                case TrackingParameterName.EventArgs:
                    return "p_EVENT_ARG";

                case TrackingParameterName.EventArgsAssemblyFullName:
                    return "p_EVENT_ARG_ASSEMBLY_NAME";

                case TrackingParameterName.EventArgsTypeFullName:
                    return "p_EVENT_ARG_TYPE_NAME";

                case TrackingParameterName.EventDateTime:
                    return "p_EVENT_DATE_TIME";

                case TrackingParameterName.EventId:
                    return "p_EVENT_ID";

                case TrackingParameterName.EventOrder:
                    return "p_EVENT_ORDER";

                case TrackingParameterName.EventType:
                    return "p_EVENT_TYPE";

                case TrackingParameterName.Exists:
                    return "p_EXISTS";

                case TrackingParameterName.FieldName:
                    return "p_FIELD_NAME";

                case TrackingParameterName.InitialisedDateTime:
                    return "p_INITIALISED_DATE_TIME";

                case TrackingParameterName.InstanceId:
                    return "p_INSTANCE_ID";

                case TrackingParameterName.IsInstanceType:
                    return "p_IS_INSTANCE_TYPE";

                case TrackingParameterName.LastCheck:
                    return "p_LAST_CHECK";

                case TrackingParameterName.NextCheck:
                    return "p_NEXT_CHECK";

                case TrackingParameterName.Order:
                    return "p_ORDER";

                case TrackingParameterName.ParentContextGuid:
                    return "p_PARENT_CONTEXT_GUID";

                case TrackingParameterName.ParentQualifiedName:
                    return "p_PARENT_QUALIFIED_NAME";

                case TrackingParameterName.QualifiedName:
                    return "p_QUALIFIED_NAME";

                case TrackingParameterName.TrackingDataItemId:
                    return "p_TRACKING_DATA_ITEM_ID";

                case TrackingParameterName.TrackingProfile:
                    return "p_TRACKING_PROFILE_XML";

                case TrackingParameterName.TypeFullName:
                    return "p_TYPE_FULL_NAME";

                case TrackingParameterName.Version:
                    return "p_VERSION";

                case TrackingParameterName.UserDataAssemblyFullName:
                    return "p_USER_DATA_ASSEMBLY_NAME";

                case TrackingParameterName.UserDataBlob:
                    return "p_USER_DATA_BLOB";

                case TrackingParameterName.UserDataKey:
                    return "p_USER_DATA_KEY";

                case TrackingParameterName.UserDataNonSerialisable:
                    return "p_USER_DATA_NON_SERIALISABLE";

                case TrackingParameterName.UserDataString:
                    return "p_USER_DATA_STR";

                case TrackingParameterName.UserDataTypeFullName:
                    return "p_USER_DATA_TYPE_NAME";

                case TrackingParameterName.UserEventId:
                    return "p_USER_EVENT_ID";

                case TrackingParameterName.WorkflowDefinition:
                    return "p_WORKFLOW_DEFINITION";

                case TrackingParameterName.WorkflowTypeId:
                    return "p_WORKFLOW_TYPE_ID";

                case TrackingParameterName.WorkflowInstanceEventId:
                    return "p_WORKFLOW_INSTANCE_EVENT_ID";

                case TrackingParameterName.WorkflowInstanceId:
                    return "p_WORKFLOW_INSTANCE_ID";

                case TrackingParameterName.WorkflowInstanceStatusId:
                    return "p_WORKFLOW_INSTANCE_STATUS";
            }

            throw new NotSupportedException(RM.Get_Error_CommandNameParameterNameNotSupported());
        }

        /// <summary>
        /// Resolve a <see cref="TrackingParameterName" /> to its database-specific parameter name
        /// including any additional batching information.
        /// </summary>
        /// <param name="commandName">
        /// A <see cref="TrackingCommandName" /> indicating which command the parameter
        /// name needs to be resolved for.
        /// </param>
        /// <param name="parameterName">
        /// A <see cref="TrackingParameterName" /> indicating which parameter needs to be resolved.
        /// </param>
        /// <param name="parameterBatch">
        /// <see cref="Int32" /> representing which parameter batch we're dealing with.
        /// </param>
        /// <returns>
        /// </returns>
        public string ResolveParameterName(TrackingCommandName commandName, TrackingParameterName parameterName, Int32 parameterBatch)
        {
            return String.Format("{0}_{1}", ResolveParameterName(
                commandName, parameterName), parameterBatch + 1);
        }
    }
}