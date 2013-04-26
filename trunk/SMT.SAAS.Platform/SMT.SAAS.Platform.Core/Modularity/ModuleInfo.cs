using System;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ��������ģ����Ϣ��Ԫ����
    /// </summary>
    public partial class ModuleInfo : IModuleCatalogItem
    {
        /// <summary>
        /// ��ʼ��һ���յ�<see cref="ModuleInfo"/>ʵ����
        /// </summary>
        public ModuleInfo()
            : this(null, null, new string[0])
        {
        }

        /// <summary>
        /// ���ݸ�����������ʼ��һ��<see cref="ModuleInfo"/>��ʵ����
        /// </summary>
        /// <param name="name">ģ������.</param>
        /// <param name="type">ģ���<see cref="Type"/>',�����޶���AssemblyQualifiedName.</param>
        /// <param name="dependsOn">��ģ������������ģ��.</param>
        /// <exception cref="ArgumentNullException">
        /// ��<paramref name="dependsOn"/>Ϊ<see langword="null"/>���׳�<see cref="ArgumentNullException"/> �쳣��
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
        ///  ���ݸ�����������ʼ��һ��<see cref="ModuleInfo"/>��ʵ����
        /// </summary>
        /// <param name="name">ģ������.</param>
        /// <param name="type">ģ������.</param>
        public ModuleInfo(string name, string type)
            : this(name, type, new string[0])
        {
        }

        /// <summary>
        /// ��ȡ������ģ�����ơ�
        /// </summary>
        /// <value>ģ������.</value>
        public string ModuleName { get; set; }

        /// <summary>
        /// ��ȡ������ģ��<see cref="Type"/> AssemblyQualifiedName��
        /// </summary>
        /// <value>ģ�����͡�</value>
        public string ModuleType { get; set; }

        /// <summary>
        /// ��ȡ�����õ�ǰģ������������ģ���б�
        /// </summary>
        /// <value>��ģ������������ģ���б�.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The setter is here to work around a Silverlight issue with setting properties from within Xaml.")]
        public Collection<string> DependsOn { get; set; }

        /// <summary>
        /// ָ��ģ��ĳ�ʼ����ʽ
        /// </summary>
        public InitializationMode InitializationMode { get; set; }

        /// <summary>
        /// ģ����򼯵�λ��
        /// <example>
        /// ����ʾ��˵��<see cref="ModuleInfo.Ref"/>��ֵ��
        ///   http://myDomain/ClientBin/MyModules.xap  ��Silverlight�ϣ�����λ�á�
        ///   file://c:/MyProject/Modules/MyModule.dll  ��WPF�ϣ�����λ�á�
        /// </example>
        /// </summary>
        public string Ref { get; set; }

        /// <summary>
        /// ��ȡ������<see cref="ModuleInfo"/>״̬����ģ����غͳ�ʼ����ʱ����ʡ�
        /// </summary>
        public ModuleState State { get; set; }
    }
}
