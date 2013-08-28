using System.Workflow.Runtime.Tracking;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Serialisable version of <see cref="TrackingDataItem" /> and its data.
    /// </summary>
    public class SerialisableTrackingDataItem : TrackingDataItem
    {
        private SerialisableData data;
        /// <summary>
        /// Serialised representation of the <see cref="TrackingDataItem.Data" /> property.
        /// </summary>
        public new SerialisableData Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}
