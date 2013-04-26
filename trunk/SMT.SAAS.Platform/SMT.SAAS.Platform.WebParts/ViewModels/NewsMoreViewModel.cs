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
using SMT.SAAS.Platform.WebParts.ViewModels.Foundation;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.WebParts.ClientServices;
using SMT.SAAS.Platform.Model;

namespace SMT.SAAS.Platform.WebParts.ViewModels
{
    public class NewsMoreViewModel : BasicViewModel
    {
        private ObservableCollection<NewsViewModel> _items = null;
        private NewsServices _Services = null;

        private NewsViewModel _currentEntity = new NewsViewModel();
        public ObservableCollection<NewsViewModel> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    this._items = value;
                    if (this._items.Count > 0)
                        CurrentEntity = _items[0];

                    base.RaisePropertyChanged("Items");
                }
            }
        }
        public NewsViewModel CurrentEntity
        {
            get { return _currentEntity; }
            set
            {
                if (this._currentEntity != value)
                {
                    this._currentEntity = value;
                    //if (_currentEntity.IsNotNull())
                    //{
                    //    _currentEntity.OnDataChangedCompleted += (args, e) =>
                    //    {
                    //        LoadDate();
                    //    };
                    //}
                    base.RaisePropertyChanged("CurrentEntity");
                }
            }
        }

        private int _pageCount = 0;
        private int _pageIndex = 1;
        private int _pageSize = 20;

        public int PageCount
        {
            get { return _pageCount; }
            set
            {
                if (this._pageCount != value)
                {
                    this._pageCount = value;
                    base.RaisePropertyChanged("PageCount");
                }
            }
        }

        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                if (this._pageIndex != value)
                {
                    this._pageIndex = value;
                    base.RaisePropertyChanged("PageCount");
                    OnPageIndexChanged();
                }
            }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (this._pageSize != value)
                {
                    this._pageSize = value;
                    base.RaisePropertyChanged("PageSize");
                    OnPageIndexChanged();
                }
            }
        }

        public NewsMoreViewModel()
        {
            _items = new ObservableCollection<NewsViewModel>();
            _Services = new NewsServices();
            _Services.OnGetNewsListCompleted += new EventHandler<GetEntityListEventArgs<NewsModel>>(_Services_OnGetNewsListCompleted);
            LoadDate();
        }

        public void Refresh()
        {
            LoadDate();
        }

        private EventHandler<GetEntityListEventArgs<NewsModel>> hanlder;

        private void LoadDate()
        {
            _Services.GetNewsListByPage(PageIndex, PageSize, "", "", PageCount);
        }

        void _Services_OnGetNewsListCompleted(object sender, GetEntityListEventArgs<NewsModel> e)
        {
            if (_items == null)
                _items = new ObservableCollection<NewsViewModel>();

            ObservableCollection<NewsViewModel> list = new ObservableCollection<NewsViewModel>();
            foreach (var item in e.Result)
            {
                list.Add(ConventToViewModel(item));
            }
            this.Items = list;
            this.PageCount = e.PageCount;
            if (this._currentEntity.IsNull() && list.Count > 0)
                this._currentEntity = list[0];
        }
        private NewsViewModel ConventToViewModel(NewsModel model)
        {
            int state = model.NEWSSTATE.IsNotNull() ? int.Parse(model.NEWSSTATE) : 0;
            int type = model.NEWSTYPEID.IsNotNull() ? int.Parse(model.NEWSTYPEID) : 0;

            var vmodel = new NewsViewModel()
            {
                NEWSTITEL = model.NEWSTITEL,
                NEWSID = model.NEWSID,
                UPDATEDATE = model.UPDATEDATE
            };
            vmodel.NEWSSTATE = vmodel.NEWSSTATELIST[state];
            vmodel.NEWSTYPE = vmodel.NEWSTYPELIST[type];

            return vmodel;
        }

        public ICommand PageIndexChanged
        {
            get { return new Foundation.RelayCommand(OnPageIndexChanged); }

        }

        private void OnPageIndexChanged()
        {
            LoadDate();
        }

    }
}
