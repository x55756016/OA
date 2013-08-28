using System;
using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services.Tracking.Ado.Sql
{
    /// <summary>
    /// SQL Server specific implementation of <see cref="ITrackingNameResolver" />.
    /// </summary>
    public class SqlTrackingNameResolver : ITrackingNameResolver
    {
        /// <summary>
        /// Resolve a <see cref="TrackingCommandName" /> to its database-specific command text.
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
                case TrackingCommandName.DeleteInstanceTrackingProfile :
                    return "DeleteInstanceTrackingProfile";
                case TrackingCommandName.DeleteTrackingProfile :
                    return "DeleteTrackingProfile";
                case TrackingCommandName.GetCurrentDefaultTrackingProfile :
                    return "GetCurrentDefaultTrackingProfile";
                case TrackingCommandName.GetDefaultTrackingProfile :
                    return "GetDefaultTrackingProfile";
                case TrackingCommandName.GetInstanceTrackingProfile :
                    return "GetInstanceTrackingProfile";
                case TrackingCommandName.GetTrackingProfile :
                    return "GetTrackingProfile";
                case TrackingCommandName.GetTrackingProfileChanges :
                    return "GetUpdatedTrackingProfiles";
                case TrackingCommandName.InsertActivities :
                    return "InsertActivities";
                case TrackingCommandName.InsertActivityAddedActions :
                    return "InsertAddedActivity";
                case TrackingCommandName.InsertActivityRemovedActions :
                    return "InsertRemovedActivity";
                case TrackingCommandName.InsertActivityTrackingRecords :
                    return "InsertActivityExecutionStatusEventMultiple";
                case TrackingCommandName.InsertEventAnnotations :
                    return "InsertEventAnnotationMultiple";
                case TrackingCommandName.InsertTrackingDataAnnotations :
                    return "InsertTrackingDataItemAnnotationMultiple";
                case TrackingCommandName.InsertTrackingDataItems :
                    return "InsertTrackingDataItemMultiple";
                case TrackingCommandName.InsertUserTrackingRecords :
                    return "InsertUserEvent";
                case TrackingCommandName.InsertWorkflow :
                    return "InsertWorkflow";
                case TrackingCommandName.InsertWorkflowInstance :
                    return "InsertWorkflowInstance";
                case TrackingCommandName.InsertWorkflowTrackingRecords :
                    return "InsertWorkflowInstanceEvent";
                case TrackingCommandName.UpdateDefaultTrackingProfile :
                    return "UpdateDefaultTrackingProfile";
                case TrackingCommandName.UpdateInstanceTrackingProfile :
                    return "SetInstanceTrackingProfile";
                case TrackingCommandName.UpdateTrackingProfile :
                    return "UpdateTrackingProfile";
            }

            throw new NotSupportedException(RM.Get_Error_CommandNamesNotSupported());
        }

        /// <summary>
        /// Resolve a <see cref="TrackingParameterName" /> to its database-specific parameter name.
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
                case TrackingParameterName.ActivityInstanceId :
                    return "ActivityInstanceId";

                case TrackingParameterName.ActivityStatusEventId :
                    return "ActivityExecutionStatusEventId";

                case TrackingParameterName.ActivityStatusId :
                    return "ExecutionStatusId";

                case TrackingParameterName.ActivityXml :
                    return "Activities";

                case TrackingParameterName.ActivityXoml :

                    if (commandName == TrackingCommandName.InsertActivityAddedActions)
                        return "AddedActivityAction";
                    else if (commandName == TrackingCommandName.InsertActivityRemovedActions)
                        return "RemovedActivityAction";

                    break;

                case TrackingParameterName.Annotation :
                    return "Annotation";

                case TrackingParameterName.AssemblyFullName:
                    return "AssemblyFullName";

                case TrackingParameterName.CallerContextGuid:
                    return "CallerContextGuid";

                case TrackingParameterName.CallerInstanceId:
                    return "CallerInstanceId";

                case TrackingParameterName.CallerParentContextGuid:
                    return "CallerParentContextGuid";

                case TrackingParameterName.CallPath:
                    return "CallPath";

                case TrackingParameterName.ContextGuid:
                    return "ContextGuid";

                case TrackingParameterName.CreateDefault:
                    return "CreateDefault";

                case TrackingParameterName.DataBlob :
                    return "Data_Blob";

                case TrackingParameterName.DataNonSerialisable :
                    return "DataNonSerializable";

                case TrackingParameterName.DataString :
                    return "Data_Str";

                case TrackingParameterName.EventArgs :
                    return "EventArg";

                case TrackingParameterName.EventArgsAssemblyFullName :
                    return "EventArgAssemblyFullName";

                case TrackingParameterName.EventArgsTypeFullName :
                    return "EventArgTypeFullName";

                case TrackingParameterName.EventDateTime:
                    return "EventDateTime";

                case TrackingParameterName.EventId :
                    return "EventId";

                case TrackingParameterName.EventOrder :
                    return "EventOrder";

                case TrackingParameterName.EventType :
                    return "EventTypeId";

                case TrackingParameterName.Exists:
                    return "Exists";

                case TrackingParameterName.FieldName :
                    return "FieldName";

                case TrackingParameterName.InitialisedDateTime :
                    return "EventDateTime";

                case TrackingParameterName.InstanceId:
                    switch (commandName)
                    {
                        case TrackingCommandName.DeleteInstanceTrackingProfile:
                        case TrackingCommandName.GetInstanceTrackingProfile:
                        case TrackingCommandName.UpdateInstanceTrackingProfile:
                            return "InstanceId";
                        case TrackingCommandName.InsertWorkflowInstance:
                            return "WorkflowInstanceId";
                    }
                    break;

                case TrackingParameterName.IsInstanceType:
                    return "IsInstanceType";

                case TrackingParameterName.LastCheck :
                    return "LastCheckDateTime";

                case TrackingParameterName.NextCheck :
                    return "MaxCheckDateTime";

                case TrackingParameterName.Order :
                    return "Order";

                case TrackingParameterName.ParentContextGuid :
                    return "ParentContextGuid";

                case TrackingParameterName.ParentQualifiedName :
                    return "ParentQualifiedName";

                case TrackingParameterName.QualifiedName :
                    return "QualifiedName";

                case TrackingParameterName.TrackingDataItemId :
                    return "TrackingDataItemId";

                case TrackingParameterName.TrackingProfile:
                    return "TrackingProfileXml";

                case TrackingParameterName.TypeFullName:
                    return "TypeFullName";

                case TrackingParameterName.Version:
                    return "Version";

                case TrackingParameterName.UserDataAssemblyFullName :
                    return "UserDataAssemblyFullName";

                case TrackingParameterName.UserDataBlob :
                    return "UserData_Blob";

                case TrackingParameterName.UserDataKey :
                    return "UserDataKey";

                case TrackingParameterName.UserDataNonSerialisable :
                    return "UserDataNonSerializable";

                case TrackingParameterName.UserDataString :
                    return "UserData_Str";

                case TrackingParameterName.UserDataTypeFullName :
                    return "UserDataTypeFullName";

                case TrackingParameterName.UserEventId :
                    return "UserEventId";

                case TrackingParameterName.WorkflowDefinition :
                    return "WorkflowDefinition";

                case TrackingParameterName.WorkflowInstanceEventId :
                    return "WorkflowInstanceEventId";

                case TrackingParameterName.WorkflowInstanceId :
                    return "WorkflowInstanceInternalId";

                case TrackingParameterName.WorkflowInstanceStatusId :
                    return "TrackingWorkflowEventId";

                case TrackingParameterName.WorkflowTypeId:
                    switch (commandName)
                    {
                        case TrackingCommandName.InsertActivities:
                            return "WorkflowTypeId";
                        case TrackingCommandName.InsertWorkflow:
                            return "WorkflowId";
                    }
                    break;
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
            String resolvedParameterName = ResolveParameterName(commandName, parameterName);
            switch (commandName)
            {
                // special cases that only support 1 parameter batch
                case TrackingCommandName.InsertUserTrackingRecords :
                case TrackingCommandName.InsertActivityAddedActions :
                case TrackingCommandName.InsertActivityRemovedActions :
                    return resolvedParameterName;
                default:
                    return String.Format("{0}{1}", resolvedParameterName, parameterBatch + 1);
            }
        }
    }
}