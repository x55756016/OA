using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.SAAS.Platform.BLL;

namespace SMT.SAAS.Platform.Services
{
    public partial class PlatformServices : IPlatformServices
    {
        private readonly ShortCutBLL _bll = new ShortCutBLL();
        public List<Model.ShortCut> GetShortCutByUser(string userSysID)
        {
            return _bll.GetShortCutByUser(userSysID); 
        }

        public bool AddShortCutByUser(List<Model.ShortCut> models, string userID)
        {
            return _bll.AddByListAndUser(models, userID); 
        }

        public bool RemoveShortCutByUser(string shortCutID, string userID)
        {
            return _bll.DeleteShortCutByUser(shortCutID, userID); 
        }
    }
}