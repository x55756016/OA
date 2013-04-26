﻿
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform
{
	public interface ICleanup 
	{
		/// <summary>
		/// 清理实例资源，例如保存实例、移除资源等
        /// 此接口不等同于IDisposable接口。仅为显式释放提供支持。
		/// </summary>
		void Cleanup();

	}
}

