using System;
using System.Workflow.Runtime.Tracking;

namespace WFTools.Services.Tracking
{
    /// <summary>
    /// Interface that exposes data-access functionality to the
    /// <see cref="GenericTrackingProfileManager"/>.
    /// </summary>
    public interface ITrackingProfileResourceAccessor : IDisposable
    {
        /// <summary>
        /// Deletes the tracking profile for the specified workflow <see cref="Type" />.
        /// </summary>
        /// <param name="workflowType">The <see cref="Type"></see> of the workflow.</param>
        void DeleteTrackingProfile(Type workflowType);

        /// <summary>
        /// Deletes the tracking profile for the workflow instance with the 
        /// specified identifier.
        /// </summary>
        /// <param name="instanceId">The <see cref="Guid"></see> of the workflow instance.</param>
        void DeleteTrackingProfile(Guid instanceId);

        ///<summary>
        /// Returns the tracking profile, qualified by version, for the 
        /// specified workflow <see cref="T:System.Type"></see>. 
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///<param name="workflowType">The <see cref="T:System.Type"></see> of the workflow.</param>
        ///<param name="profileVersion">The <see cref="T:System.Version"></see> of the tracking profile.</param>
        TrackingProfile GetTrackingProfile(Type workflowType, Version profileVersion);

        ///<summary>
        /// Returns the latest tracking profile for the specified workflow 
        /// <see cref="T:System.Type" />.
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///<param name="workflowType">The <see cref="T:System.Type"></see> of the workflow.</param>
        TrackingProfile GetTrackingProfile(Type workflowType);

        /// <summary>
        /// Returns the tracking profile for the workflow instance with the 
        /// specified identifier.
        /// </summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        /// <param name="instanceId">The <see cref="Guid"></see> of the workflow instance.</param>
        TrackingProfile GetTrackingProfile(Guid instanceId);

        ///<summary>
        /// Returns the latest default tracking profile.
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        TrackingProfile GetDefaultTrackingProfile();

        ///<summary>
        /// Returns the default tracking profile, qualified by version.
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///<param name="profileVersion">The <see cref="T:System.Version"></see> of the tracking profile.</param>
        TrackingProfile GetDefaultTrackingProfile(Version profileVersion);

        /// <summary>
        /// Updates the tracking profile for the specified workflow <see cref="Type" />.
        /// </summary>
        /// <param name="workflowType">The <see cref="Type"></see> of the workflow.</param>
        /// <param name="updatedProfile">The updated <see cref="TrackingProfile" />.</param>
        void UpdateTrackingProfile(Type workflowType, TrackingProfile updatedProfile);

        /// <summary>
        /// Updates the tracking profile for the specified workflow instance.
        /// </summary>
        /// <param name="instanceId">The <see cref="Guid"></see> of the workflow instance.</param>
        /// <param name="updatedProfile">The updated <see cref="TrackingProfile" />.</param>
        void UpdateTrackingProfile(Guid instanceId, TrackingProfile updatedProfile);

        /// <summary>
        /// Updates the default tracking profile.
        /// </summary>
        /// <param name="updatedProfile">The updated default <see cref="TrackingProfile" />.</param>
        void UpdateDefaultTrackingProfile(TrackingProfile updatedProfile);
    }
}