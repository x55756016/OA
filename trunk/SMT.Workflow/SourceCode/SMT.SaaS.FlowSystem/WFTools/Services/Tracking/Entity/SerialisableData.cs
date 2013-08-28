using System;
using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services.Tracking.Entity
{
    /// <summary>
    /// Representation of a serialised version of some data.
    /// </summary>
    public class SerialisableData
    {
        private object unserialisedData;
        /// <summary>
        /// The unserialised data.
        /// </summary>
        public object UnserialisedData
        {
            get { return unserialisedData; }
            set { unserialisedData = value; }
        }

        private byte[] serialisedData;
        /// <summary>
        /// Serialised representation of the <see cref="UnserialisedData" /> property.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", 
            Justification = "byte [] contains serialised data, cannot be represented as a collection")]
        public byte[] SerialisedData
        {
            get { return serialisedData; }
            set { serialisedData = value; }
        }

        private string stringData;
        /// <summary>
        /// String representation of the <see cref="UnserialisedData" /> property.
        /// </summary>
        public string StringData
        {
            get { return stringData; }
            set { stringData = value; }
        }

        private Type type;
        /// <summary>
        /// Indicates the <see cref="Type" /> of the <see cref="UnserialisedData" /> property.
        /// </summary>
        public Type Type
        {
            get { return type; }
            set { type = value; }
        }

        private bool nonSerialisable;
        /// <summary>
        /// Indicates whether the <see cref="UnserialisedData" /> property
        /// is serialisable.
        /// </summary>
        public bool NonSerialisable
        {
            get { return nonSerialisable; }
            set { nonSerialisable = value; }
        }
    }
}
