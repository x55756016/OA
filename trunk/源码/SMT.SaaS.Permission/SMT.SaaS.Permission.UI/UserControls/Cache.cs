
namespace SMT.SaaS.Permission.UI.UserControls
{
    public class Cache
    {
        Cache() 
        {
        }

        /// <summary>
        /// Nested class for lazy initialization.
        /// </summary>
        class NestedCache
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedCache()
            {
            }
            internal static readonly Cache Instance = new Cache();
        }

        public static Cache Instance
        {
            get { return NestedCache.Instance; }
        }

        public object this[string key]
        {
            get { return ClientStorage.Instance[key]; }
            set { ClientStorage.Instance[key] = value; }
        }
        public void Add(string key,object obj)
        {
            ClientStorage.Instance.Add(key,obj);
        }
        public void Remove(string key)
        {
            ClientStorage.Instance.Remove(key);
        }

    }
}
