using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 用于返回一个加载模块对象的
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    public partial class ModuleManager
    {
        /// <summary>
        /// 返回注册的<see cref="IModuleTypeLoader"/> 实例，用于加载初始化模块。
        /// </summary>
        /// <value>
        /// 模块加载对象。<see cref="IModuleTypeLoader"/> 
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
