using System;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 定义描述模块信息的元数据
    /// </summary>
    public partial class ModuleInfo : IModuleCatalogItem
    {
        /// <summary>
        /// 初始化一个空的<see cref="ModuleInfo"/>实例。
        /// </summary>
        public ModuleInfo()
            : this(null, null, new string[0])
        {
        }

        /// <summary>
        /// 根据给定参数，初始化一个<see cref="ModuleInfo"/>新实例。
        /// </summary>
        /// <param name="name">模块名称.</param>
        /// <param name="type">模块的<see cref="Type"/>',程序集限定名AssemblyQualifiedName.</param>
        /// <param name="dependsOn">该模块依赖的其它模块.</param>
        /// <exception cref="ArgumentNullException">
        /// 若<paramref name="dependsOn"/>为<see langword="null"/>将抛出<see cref="ArgumentNullException"/> 异常。
        /// </exception>
        public ModuleInfo(string name, string type, params string[] dependsOn)
        {
            if (dependsOn == null) throw new System.ArgumentNullException("dependsOn");

            this.ModuleName = name;
            this.ModuleType = type;
            this.DependsOn = new Collection<string>();
            foreach (string dependency in dependsOn)
            {
                this.DependsOn.Add(dependency);
            }
        }

        /// <summary>
        ///  根据给定参数，初始化一个<see cref="ModuleInfo"/>新实例。
        /// </summary>
        /// <param name="name">模块名称.</param>
        /// <param name="type">模块类型.</param>
        public ModuleInfo(string name, string type)
            : this(name, type, new string[0])
        {
        }

        /// <summary>
        /// 获取或设置模块名称。
        /// </summary>
        /// <value>模块名称.</value>
        public string ModuleName { get; set; }

        /// <summary>
        /// 获取或设置模块<see cref="Type"/> AssemblyQualifiedName。
        /// </summary>
        /// <value>模块类型。</value>
        public string ModuleType { get; set; }

        /// <summary>
        /// 获取或设置当前模块依赖的其他模块列表
        /// </summary>
        /// <value>该模块依赖的其他模块列表.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The setter is here to work around a Silverlight issue with setting properties from within Xaml.")]
        public Collection<string> DependsOn { get; set; }

        /// <summary>
        /// 指定模块的初始化方式
        /// </summary>
        public InitializationMode InitializationMode { get; set; }

        /// <summary>
        /// 模块程序集的位置
        /// <example>
        /// 下面示例说明<see cref="ModuleInfo.Ref"/>的值：
        ///   http://myDomain/ClientBin/MyModules.xap  在Silverlight上，网络位置。
        ///   file://c:/MyProject/Modules/MyModule.dll  在WPF上，本地位置。
        /// </example>
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// 获取或设置<see cref="ModuleInfo"/>状态，在模块加载和初始化的时候访问。
        /// </summary>
        public ModuleState State { get; set; }
    }
}
