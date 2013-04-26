using System;
using System.Collections.Generic;
using System.Globalization;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 计算模块依赖关系
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 根据模块间的依赖关系，使用 <see cref="ModuleInitializer"/> 来获取它们的加载序列。
    /// </summary>
    public class ModuleDependencySolver
    {
        private readonly ListDictionary<string, string> dependencyMatrix = new ListDictionary<string, string>();
        private readonly List<string> knownModules = new List<string>();

        /// <summary>
        /// 添加一个模块到计算器中。
        /// </summary>
        /// <param name="name">
        /// 标识模块的唯一名称。
        /// </param>
        public void AddModule(string name)
        {

            if (String.IsNullOrEmpty(name))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.StringCannotBeNullOrEmpty, "name"));
          
                AddToDependencyMatrix(name);

                AddToKnownModules(name);
            

        }

        /// <summary>
        /// 根据跟定的模块添加两个模块的依赖关系。
        /// </summary>
        /// <param name="dependingModule">
        /// 依赖的模块名称。
        /// </param>
        /// <param name="dependentModule">
        /// The name of the module dependingModule depends on.
        /// </param>
        public void AddDependency(string dependingModule, string dependentModule)
        {
            if (String.IsNullOrEmpty(dependingModule))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.StringCannotBeNullOrEmpty, "dependingModule"));

            if (String.IsNullOrEmpty(dependentModule))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.StringCannotBeNullOrEmpty, "dependentModule"));

            if (!knownModules.Contains(dependingModule))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.DependencyForUnknownModule, dependingModule));

            AddToDependencyMatrix(dependentModule);
            dependencyMatrix.Add(dependentModule, dependingModule);
        }

        private void AddToDependencyMatrix(string module)
        {
            if (!dependencyMatrix.ContainsKey(module))
            {
                dependencyMatrix.Add(module);
            }
        }

        private void AddToKnownModules(string module)
        {
            if (!knownModules.Contains(module))
            {
                knownModules.Add(module);
            }
        }

        /// <summary>
        /// Calculates an ordered vector according to the defined dependencies.
        /// Non-dependant modules appears at the beginning of the resulting array.
        /// </summary>
        /// <returns>The resulting ordered list of modules.</returns>
        /// <exception cref="CyclicDependencyFoundException">This exception is thrown
        /// when a cycle is found in the defined depedency graph.</exception>
        public string[] Solve()
        {
            List<string> skip = new List<string>();
            while (skip.Count < dependencyMatrix.Count)
            {
                List<string> leaves = this.FindLeaves(skip);
                if (leaves.Count == 0 && skip.Count < dependencyMatrix.Count)
                {
                    throw new CyclicDependencyFoundException(Resources.CyclicDependencyFound);
                }
                skip.AddRange(leaves);
            }
            skip.Reverse();

            if (skip.Count > knownModules.Count)
            {
                string moduleNames = this.FindMissingModules(skip);
                throw new ModularityException(moduleNames, String.Format(CultureInfo.CurrentCulture,
                                                            Resources.DependencyOnMissingModule,
                                                            moduleNames));
            }

            return skip.ToArray();
        }

        private string FindMissingModules(List<string> skip)
        {
            string missingModules = "";

            foreach (string module in skip)
            {
                if (!knownModules.Contains(module))
                {
                    missingModules += ", ";
                    missingModules += module;
                }
            }

            return missingModules.Substring(2);
        }

        /// <summary>
        /// Gets the number of modules added to the solver.
        /// </summary>
        /// <value>The number of modules.</value>
        public int ModuleCount
        {
            get { return dependencyMatrix.Count; }
        }

        private List<string> FindLeaves(List<string> skip)
        {
            List<string> result = new List<string>();

            foreach (string precedent in dependencyMatrix.Keys)
            {
                if (skip.Contains(precedent))
                {
                    continue;
                }

                int count = 0;
                foreach (string dependent in dependencyMatrix[precedent])
                {
                    if (skip.Contains(dependent))
                    {
                        continue;
                    }
                    count++;
                }
                if (count == 0)
                {
                    result.Add(precedent);
                }
            }
            return result;
        }
    }
}