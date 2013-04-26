using SMT.SAAS.Platform.DAL.Interface;
using SMT.SAAS.Platform.Model;
using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供基于Oracle数据库的WEBPART信息访问实现
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.OracleDAL
{
	/// <summary>
	/// 提供基于Oracle数据库的WEBPART信息访问实现
	/// </summary>
	public class WebPartDAL : IWebPartDAL
	{

        public bool Add(WebPart entity)
        {
            throw new System.NotImplementedException();
        }

        public bool Update(WebPart entity)
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(WebPart entity)
        {
            throw new System.NotImplementedException();
        }

        public WebPart GetEntityByKey(string key)
        {
            throw new System.NotImplementedException();
        }

        public bool IsExists(string key)
        {
            throw new System.NotImplementedException();
        }

        public List<WebPart> GetListByPager(int index, int size, ref int count, string sort)
        {
            throw new System.NotImplementedException();
        }
    }
}

