using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SMT.SAAS.Platform.BLL;

namespace SMT.SAAS.Platform.Services
{
    public partial class PlatformSiServices : IPlatformSiServices
    {
        private readonly ShortCutBLL _bll = new ShortCutBLL();
        public bool AddShortCut(Model.ShortCut model)
        {
            return _bll.Add(model);
        }

        public bool AddShortCutByList(List<Model.ShortCut> models)
        {
            return _bll.AddByList(models);
        }

        public bool AddShortCutByUser(List<Model.ShortCut> models, string userID)
        {
            return _bll.AddByListAndUser(models,userID);
        }

        public List<Model.ShortCut> GetShortCutByUser(string userSysID)
        {
            return _bll.GetShortCutByUser(userSysID);
        }
    }
}