using System;

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
    public interface IWebpart : IModule
    {
        /// <summary>
        /// 获取数据的条数。受用户个性化控制。
        /// </summary>
        int Top { get; set; }

        /// <summary>
        /// 获取的时间范围。受用户个性化控制。
        /// 1小时以内、2小时以内、1天、2天等...
        /// </summary>
        int RefDate { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        string Titel { get; set; }

        /// <summary>
        /// 加载数据。
        /// </summary>
        void LoadDate();

        /// <summary>
        /// 暂停WebPart刷新
        /// </summary>
        void Stop();

        /// <summary>
        /// 开始
        /// </summary>
        void Star();
    }
}

