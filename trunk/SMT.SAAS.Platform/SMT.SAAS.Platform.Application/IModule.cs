
namespace SMT.SAAS.Platform
{
    public interface IModule:ICleanup
    {
        /// <summary>
        /// 初始化。在系统加载成功后将进行系统初始化动作，用于为系统正常运行提供资源。
        /// </summary>
        void Initialize();
    }
}
