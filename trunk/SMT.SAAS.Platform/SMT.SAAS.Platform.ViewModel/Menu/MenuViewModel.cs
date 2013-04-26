
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SMT.SAAS.Platform.Core.Modularity;

namespace SMT.SAAS.Platform.ViewModel.Menu
{
    public class MenuViewModel : Foundation.BasicViewModel
    {
        public MenuViewModel()
        {
        }
        #region 成员列表

        public string MenuID { get; set; }

        public string MenuName { get; set; }

        public string MenuIconPath { get; set; }

        public ModuleInfo Content { get; set; }

        public ObservableCollection<MenuViewModel> Item { get; set; }

        #endregion
    }
}
