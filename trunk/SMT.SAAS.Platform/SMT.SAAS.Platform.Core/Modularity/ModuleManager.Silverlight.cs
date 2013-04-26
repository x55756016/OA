using System.Collections.Generic;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: ���ڷ���һ������ģ������
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    public partial class ModuleManager
    {
        /// <summary>
        /// ����ע���<see cref="IModuleTypeLoader"/> ʵ�������ڼ��س�ʼ��ģ�顣
        /// </summary>
        /// <value>
        /// ģ����ض���<see cref="IModuleTypeLoader"/> 
        /// </value>
        public virtual IEnumerable<IModuleTypeLoader> ModuleTypeLoaders
        {
            get
            {
                if (this.typeLoaders == null)
                {
                    this.typeLoaders = new List<IModuleTypeLoader>()
                                          {
                                              new XapModuleTypeLoader()
                                          };
                }

                return this.typeLoaders;
            }

            set
            {
                this.typeLoaders = value;
            }
        }
    }
}
