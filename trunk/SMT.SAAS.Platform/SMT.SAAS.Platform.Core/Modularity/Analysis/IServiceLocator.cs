using System;
using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 定义获取对象实例的一组方法
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core
{
    /// <summary>
    /// 定义获取对象实例的一组方法
    /// </summary>
    public interface IServiceLocator
    {
        IEnumerable<TService> GetAllInstances<TService>();

        IEnumerable<object> GetAllInstances(Type serviceType);

        TService GetInstance<TService>();

        TService GetInstance<TService>(string key);

        object GetInstance(Type serviceType);

        object GetInstance(Type serviceType, string key);
    }
}
