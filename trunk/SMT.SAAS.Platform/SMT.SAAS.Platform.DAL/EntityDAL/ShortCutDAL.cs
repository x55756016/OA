using SMT.SAAS.Platform.DAL.Interface;
using SMT.SAAS.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMT_Platform_EFModel;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供基于Oracle数据库的快捷方式信息访问实现
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.OracleDAL
{
    /// <summary>
    /// 提供基于Oracle数据库的快捷方式信息访问实现
    /// </summary>
    public class ShortCutDAL : IShortCutDAL
    {
        private CommonDAL<T_PF_SHORTCUT> _commonDAL;
        private CommonDAL<T_PF_USER_SHORTCUT> _userShortCutDAL;
        public ShortCutDAL()
        {
            _commonDAL = new CommonDAL<T_PF_SHORTCUT>();
            _userShortCutDAL = new CommonDAL<T_PF_USER_SHORTCUT>();
        }

        public List<ShortCut> GetShortCutListByUser(string userID)
        {
            var shortcutids = from us in _commonDAL.GetTable<T_PF_USER_SHORTCUT>()
                              where us.OWNERID == userID
                              select new
                              {
                                  us.SHORTCUTID
                              };

            List<string> ids = new List<string>();

            foreach (var item in shortcutids)
            {
                ids.Add(item.SHORTCUTID);
            }

            var ents = from ent in _commonDAL.GetObjects()
                       where ids.Contains(ent.SHORTCUTID) || ent.ISSYSNEED == "1"
                       select ent;

            List<ShortCut> result = new List<ShortCut>();
            if (ents.Count() > 0)
            {
                foreach (var item in ents)
                {
                    result.Add(item.CloneObject<ShortCut>(new ShortCut()));
                }
            }
            return result;
        }

        public bool Add(ShortCut entity)
        {
            T_PF_SHORTCUT model = entity.CloneObject<T_PF_SHORTCUT>(new T_PF_SHORTCUT());
            model = Utility.CreateCommonProperty().CloneObject<T_PF_SHORTCUT>(model);
            return _commonDAL.Add(model);
        }

        private bool ExistsUserShortCut(string shortCutID, string userID)
        {
            var ents = from ent in _userShortCutDAL.GetObjects()
                       where ent.SHORTCUTID == shortCutID && ent.OWNERID == userID
                       select ent;
            return ents.Count() > 0 ? true : false;
        }

        private bool AddUserShortCut(string shortCutID, string userID)
        {

            if (ExistsUserShortCut(shortCutID, userID))
                return true;

            T_PF_USER_SHORTCUT model = Utility.CreateCommonProperty().CloneObject<T_PF_USER_SHORTCUT>(new T_PF_USER_SHORTCUT());
            model.SHORTCUTID = shortCutID;
            model.OWNERID = userID;
            model.USERSHORTCUTID = Guid.NewGuid().ToString();

            return _userShortCutDAL.Add(model);
        }

        public bool DeleteShortCutByUser(string shortCutID, string userID)
        {
            var ents = from ent in _userShortCutDAL.GetObjects()
                       where ent.SHORTCUTID == shortCutID && ent.OWNERID == userID
                       select ent;
            if (ents.Count() > 0)
            {
                T_PF_USER_SHORTCUT currentItem = ents.FirstOrDefault() as T_PF_USER_SHORTCUT;
                return  _userShortCutDAL.Delete(currentItem);
            }
            return false;
        }

        public bool AddByUser(ShortCut entity, string userid)
        {
            bool result = true;
            if (!IsExists(entity.ShortCutID))
            {
               result=this.Add(entity);
            }

            if (result)
                result = AddUserShortCut(entity.ShortCutID, userid);

            return result;
        }

        public bool Update(ShortCut entity)
        {
            T_PF_SHORTCUT model = entity.CloneObject<T_PF_SHORTCUT>(new T_PF_SHORTCUT());
            model = Utility.CreateCommonProperty().CloneObject<T_PF_SHORTCUT>(model);
            return _commonDAL.Update(model);
        }

        public bool Delete(ShortCut entity)
        {
            throw new NotImplementedException();
        }

        public ShortCut GetEntityByKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool IsExists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            var ents = from ent in _commonDAL.GetObjects()
                       where ent.SHORTCUTID == key
                       select ent;
            return ents.Count() > 0 ? true : false;
        }

        public List<ShortCut> GetListByPager(int index, int size, ref int count, string sort)
        {
            throw new NotImplementedException();
        }
    }
}

