using SMT.SAAS.Platform.DAL.Interface;
using SMT.SAAS.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供基于Oracle数据库的公共组件信息访问实现
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.OracleDAL
{
	/// <summary>
	/// 提供基于Oracle数据库的公共组件信息访问实现
	/// </summary>
	public class PublicPartDAL: IPublicPartDAL
	{
        public bool Add(PublicPart entity)
        {
            throw new NotImplementedException();
        }

        public bool Update(PublicPart entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(PublicPart entity)
        {
            throw new NotImplementedException();
        }

        public PublicPart GetEntityByKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool IsExists(string key)
        {
            throw new NotImplementedException();
        }

        public List<PublicPart> GetListByPager(int index, int size, ref int count, string sort)
        {
            throw new NotImplementedException();
        }
    }
}

