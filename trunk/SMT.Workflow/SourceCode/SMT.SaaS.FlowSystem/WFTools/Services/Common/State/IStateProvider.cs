using System.Diagnostics.CodeAnalysis;

namespace WFTools.Services.Common.State
{
    /// <summary>
    /// Interface allowing access to add/remove/get state from some
    /// form of storage.
    /// </summary>
    public interface IStateProvider
    {
        /// <summary>
        /// Retrieve an item from storage.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of an item in storage.
        /// </param>
        /// <returns>
        /// An <see cref="object" /> representing the item in storage, 
        /// or <c>null</c> if the item was not found.
        /// </returns>
        object Get(string key);

        /// <summary>
        /// Retrieve an item from storage, casting it to the specified type.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of an item in storage.
        /// </param>
        /// <typeparam name="T">
        /// Type to cast the returned item to.
        /// </typeparam>
        /// <returns>
        /// The item from storage, or <c>null</c> if the item was not found.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "Code analysis mistakenly marks this method as failing this rule.")]
        T Get<T>(string key);

        /// <summary>
        /// Add an item to storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        /// <param name="value">
        /// The item to add.
        /// </param>
        void Add(string key, object value);

        /// <summary>
        /// Add an item to storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        /// <param name="value">
        /// The item to add.
        /// </param>
        void Add<T>(string key, T value);

        /// <summary>
        /// Remove an item to storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        void Remove(string key);

        /// <summary>
        /// Update an item in storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item to update.
        /// </param>
        /// <param name="value">
        /// The item to update.
        /// </param>
        void Update(string key, object value);

        /// <summary>
        /// Update an item in storage using the specified unique key.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item to update.
        /// </param>
        /// <param name="value">
        /// The item to update.
        /// </param>
        void Update<T>(string key, T value);

        /// <summary>
        /// Indicates whether an item exists in storage.
        /// </summary>
        /// <param name="key">
        /// Unique identifier of the item.
        /// </param>
        bool Contains(string key);
    }
}