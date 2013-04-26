
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 标记接口，允许<see cref="ModuleInfoGroup"/>和<see cref="ModuleInfo"/>
    /// 分别可以使用代码和XAML添加到 <see cref="IModuleCatalog"/>。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "This is a marker interface")]
    public interface IModuleCatalogItem
    {

    }
}
