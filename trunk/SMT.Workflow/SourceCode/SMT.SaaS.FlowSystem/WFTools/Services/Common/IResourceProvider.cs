using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services
{
    /// <summary>
    /// Marker interface that indicates the implementation provides
    /// resources for persisting data to durable storage.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", 
        Justification = "Used at compile time to make sure workflow services return a valid resource provider.")]
    public interface IResourceProvider
    {
    }
}