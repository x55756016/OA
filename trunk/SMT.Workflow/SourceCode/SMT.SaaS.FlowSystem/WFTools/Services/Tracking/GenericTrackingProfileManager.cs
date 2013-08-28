using System;
using System.Workflow.Runtime.Tracking;
using WFTools.Utilities.Diagnostics;

namespace WFTools.Services.Tracking
{
    /// <summary>
    /// Abstract class used for managing tracking profiles within
    /// some form of durable storage in a very generic fashion. 
    /// 
    /// A resource provider and accessor work hand-in-hand to actually
    /// persist and retrieve tracking profiles to the tracking store.
    /// </summary>
    public abstract class GenericTrackingProfileManager
    {
        private IResourceProvider _resourceProvider;
        /// <summary>
        /// The active <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying tracking store.
        /// </summary>
        private IResourceProvider resourceProvider
        {
            get
            {
                if (_resourceProvider == null)
                    _resourceProvider = CreateResourceProvider();

                return _resourceProvider;
            }
        }

        /// <summary>
        /// Create an <see cref="IResourceProvider" /> that provides 
        /// resources necessary for manipulating the underlying tracking store.
        /// </summary>
        /// <remarks>
        /// The resource provider is created once upon Start of the 
        /// tracking service.
        /// </remarks>
        protected abstract IResourceProvider CreateResourceProvider();

        /// <summary>
        /// Create an <see cref="ITrackingServiceResourceAccessor" /> that is responsible
        /// for manipulating the underlying tracking store.
        /// </summary>
        /// <param name="resourceProvider">
        /// The active <see cref="IResourceProvider" />.
        /// </param>
        protected abstract ITrackingProfileResourceAccessor CreateAccessor(IResourceProvider resourceProvider);

        /// <summary>
        /// Deletes the tracking profile for the specified workflow <see cref="Type" />.
        /// </summary>
        /// <param name="workflowType">The <see cref="Type"></see> of the workflow.</param>
        public void DeleteTrackingProfile(Type workflowType)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                resourceAccessor.DeleteTrackingProfile(workflowType);
            }
        }

        /// <summary>
        /// Deletes the tracking profile for the workflow instance with the 
        /// specified identifier.
        /// </summary>
        /// <param name="instanceId">The <see cref="Guid"></see> of the workflow instance.</param>
        public void DeleteTrackingProfile(Guid instanceId)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                resourceAccessor.DeleteTrackingProfile(instanceId);
            }
        }

        ///<summary>
        /// Returns the tracking profile, qualified by version, for the 
        /// specified workflow <see cref="T:System.Type"></see>. 
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///<param name="workflowType">The <see cref="T:System.Type"></see> of the workflow.</param>
        ///<param name="profileVersion">The <see cref="T:System.Version"></see> of the tracking profile.</param>
        public TrackingProfile GetTrackingProfile(Type workflowType, Version profileVersion)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                return resourceAccessor.GetTrackingProfile(workflowType, profileVersion);
            }
        }

        ///<summary>
        /// Returns the latest tracking profile for the specified workflow 
        /// <see cref="T:System.Type" />.
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///<param name="workflowType">The <see cref="T:System.Type"></see> of the workflow.</param>
        public TrackingProfile GetTrackingProfile(Type workflowType)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                return resourceAccessor.GetTrackingProfile(workflowType);
            }
        }

        /// <summary>
        /// Returns the tracking profile for the workflow instance with the 
        /// specified identifier.
        /// </summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        /// <param name="instanceId">The <see cref="Guid"></see> of the workflow instance.</param>
        public TrackingProfile GetTrackingProfile(Guid instanceId)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                return resourceAccessor.GetTrackingProfile(instanceId);
            }
        }

        ///<summary>
        /// Returns the latest default tracking profile.
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        public TrackingProfile GetDefaultTrackingProfile()
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                return resourceAccessor.GetDefaultTrackingProfile();
            }
        }

        ///<summary>
        /// Returns the default tracking profile, qualified by version.
        ///</summary>
        ///<returns>
        ///A <see cref="T:System.Workflow.Runtime.Tracking.TrackingProfile"></see>.
        ///</returns>
        ///<param name="profileVersion">The <see cref="T:System.Version"></see> of the tracking profile.</param>
        public TrackingProfile GetDefaultTrackingProfile(Version profileVersion)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                return resourceAccessor.GetDefaultTrackingProfile(profileVersion);
            }
        }

        /// <summary>
        /// Updates the tracking profile for the specified workflow <see cref="Type" />.
        /// </summary>
        /// <param name="workflowType">The <see cref="Type"></see> of the workflow.</param>
        /// <param name="updatedProfile">The updated <see cref="TrackingProfile" />.</param>
        public void UpdateTrackingProfile(Type workflowType, TrackingProfile updatedProfile)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                resourceAccessor.UpdateTrackingProfile(workflowType, updatedProfile);
            }
        }

        /// <summary>
        /// Updates the tracking profile for the specified workflow instance.
        /// </summary>
        /// <param name="instanceId">The <see cref="Guid"></see> of the workflow instance.</param>
        /// <param name="updatedProfile">The updated <see cref="TrackingProfile" />.</param>
        public void UpdateTrackingProfile(Guid instanceId, TrackingProfile updatedProfile)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                resourceAccessor.UpdateTrackingProfile(instanceId, updatedProfile);
            }
        }

        /// <summary>
        /// Updates the default tracking profile.
        /// </summary>
        /// <param name="trackingProfile">The updated default <see cref="TrackingProfile" />.</param>
        public void UpdateDefaultTrackingProfile(TrackingProfile trackingProfile)
        {
            TraceHelper.Trace();

            using (ITrackingProfileResourceAccessor resourceAccessor = CreateAccessor(resourceProvider))
            {
                resourceAccessor.UpdateDefaultTrackingProfile(trackingProfile);
            }
        }
    }
}