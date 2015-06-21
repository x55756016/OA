
namespace SMT.SAAS.Controls
{
    /// <summary>
    /// 定义一种释放分配的资源的方法
    /// 为类定义通用的接口,用于释放资源，
    /// 此接口并不会影响IDisposable.当实例实现了此接口，也可以不进行资源垃圾处理
    /// </summary>
    public interface ICleanup
    {
        /// <summary>
        /// 清理实例资源，例如保存实例、移除资源等
        /// </summary>
        void Cleanup();
    }
}
