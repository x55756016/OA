using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WFTools.Utilities
{
    /// <summary>
    /// Utility class for cloning and serialising objects.
    /// </summary>
    public static class SerialisationUtilities
    {
        /// <summary>
        /// Clone the passed object providing an exact replica.
        /// </summary>
        /// <param name="objectToClone">The object to clone</param>
        /// <returns>A clone of the original object</returns>
        public static T Clone<T>(T objectToClone)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Context = new StreamingContext(StreamingContextStates.Clone);

            formatter.Serialize(stream, objectToClone);
            stream.Position = 0;
            return (T) formatter.Deserialize(stream);
        }

        /// <summary>
        /// Clone the passed object providing an exact replica.
        /// </summary>
        /// <param name="objectToSerialise">The object to serialise.</param>
        /// <returns>
        /// A <see cref="byte" /> array containing a binary serialised 
        /// representation of the original object.
        /// </returns>
        public static byte [] Serialise(object objectToSerialise)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Context = new StreamingContext(StreamingContextStates.Clone);
            formatter.Serialize(stream, objectToSerialise);

            byte[] serialisedData = new byte[stream.Length];

            int read = 0;
            int offset = 0;
            while (read > 0)
            {
                offset += read;
                read = stream.Read(serialisedData, offset, 
                    (int)stream.Length - offset);
            }

            return serialisedData;
        }
    }
}