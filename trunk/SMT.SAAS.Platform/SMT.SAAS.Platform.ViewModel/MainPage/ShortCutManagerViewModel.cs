using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;

namespace SMT.SAAS.Platform.ViewModel.MainPage
{
    public class ShortCutManagerViewModel : Foundation.BasicViewModel
    {

        Model.Services.ShortCutServices _services = new Model.Services.ShortCutServices();

        private ObservableCollection<ShortCutViewModel> _item = new ObservableCollection<ShortCutViewModel>();

        public ObservableCollection<ShortCutViewModel> Item
        {
            get { return _item; }
            set
            {
                SetValue(ref _item, value, "Item");
            }
        }

        public ShortCutManagerViewModel()
        {
            InitItems();
        }

        private void InitItems()
        {
            _services = new Model.Services.ShortCutServices();
            _services.OnGetShortCutCompleted += new EventHandler<Model.GetEntityListEventArgs<Model.ShortCut>>(_services_OnGetShortCutCompleted);
            _services.OnRemoveShortCutCompleted += new EventHandler<Model.ExecuteNoQueryEventArgs>(_services_OnRemoveShortCutCompleted);
            
            if(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo==null) return;
            _services.GetShortCutByUser(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
           
        }

       

        public void Submit()
        {
            ObservableCollection<Model.ShortCut> items = new ObservableCollection<Model.ShortCut>();
            foreach (var item in Item)
            {
                Model.ShortCut tempItem = item.CloneObject<Model.ShortCut>(new Model.ShortCut());
                items.Add(tempItem);
            }

            _services.AddShortCutByUser(items, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
        }

        public void RemoveItem(string shortCutID)
        {
            var existsitem = this.Item.FirstOrDefault(i => i.ModuleID == shortCutID);
            if (existsitem != null)
            {
                _services.RemoveShortCutByUser(shortCutID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID);
                this.Item.Remove(existsitem);
            }
        }

        void _services_OnRemoveShortCutCompleted(object sender, Model.ExecuteNoQueryEventArgs e)
        {
            if (e.Result)
                Debug.WriteLine("ShortCut Removed!");
        }

        void _services_OnGetShortCutCompleted(object sender, Model.GetEntityListEventArgs<Model.ShortCut> e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    ObservableCollection<ShortCutViewModel> items = new ObservableCollection<ShortCutViewModel>();
                    foreach (var item in e.Result)
                    {
                        ShortCutViewModel tempItem = item.CloneObject<ShortCutViewModel>(new ShortCutViewModel());
                        items.Add(tempItem);
                    }
                    this.Item = items;
                }
            }
        }
    }
}
