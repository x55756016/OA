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
using SMT.SAAS.Platform.Model;
using SMT.SAAS.Platform.WebParts.ClientServices;
using System.ComponentModel.DataAnnotations;

namespace SMT.SAAS.Platform.WebParts.ViewModels
{
    /// <summary>
    /// ViewModel类:NEWSViewModel 的摘要说明。
    /// </summary>
    public class NewsViewModel : BasicViewModel
    {
        #region 私有成员 Private Member
        private NewsServices _Services = null;
        private ObservableCollection<NewsType> _newstypelist = new ObservableCollection<NewsType>() 
        {
           new NewsType(){ID=0,Name="新闻"},
           new NewsType(){ID=1,Name="动态"}
        };
        private ObservableCollection<NewsState> _newsstatelist = new ObservableCollection<NewsState>() 
        {
           new NewsState(){ID=0,Name="未发布"},
           new NewsState(){ID=1,Name="发  布"},
           new NewsState(){ID=2,Name="关  闭"}
        };
        private string _newsid = Guid.NewGuid().ToString();
        private NewsType _newstype;
        private string _newstitel = string.Empty;
        private byte[] _newscontent = new byte[0];
        private string _readcount = string.Empty;
        private string _commentcount = string.Empty;
        public DateTime _updatedate = DateTime.Now;
        private NewsState _newsstate;
        private bool _isrelease = true;
        private bool _isimage = false;
        private bool _ispopup = false;
        private DateTime _enddate = DateTime.Now;
        private ObservableCollection<string> _viewer = new ObservableCollection<string>();
        private ObservableCollection<DISTR> _distr = new ObservableCollection<DISTR>();
        private ObservableCollection<string> _modelname = new ObservableCollection<string>();
        private bool _isAdd = true;
        public NewsManagerViewModel ParentVM;
        private string _putdeptname = string.Empty;
        private string _putdeptid = string.Empty;
        #endregion

        #region 构造函数 Constructed
        public NewsViewModel()
        {
            _Services = new NewsServices();
            if (_newsstate.IsNull())
                _newsstate = _newsstatelist[0];
            if (_newstype.IsNull())
                _newstype = _newstypelist[0];
        }
        #endregion

        #region 公有属性 Public Properties

        /// <summary>
        /// 标识实体状态
        /// </summary>
        public bool IsAdd
        {
            get { return _isAdd; }
            set
            { _isAdd = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string NEWSID
        {
            get { return _newsid; }
            set
            {
                if (_newsid != value)
                {
                    SetValue(ref _newsid, value, "NEWSID");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CompanyID
        {
            get { return _newsid; }
            set
            {
                if (_newsid != value)
                {
                    SetValue(ref _newsid, value, "CompanyID");

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public NewsType NEWSTYPE
        {
            get
            {
                return _newstype;
            }
            set
            {
                if (_newstype != value)
                {
                    SetValue(ref _newstype, value, "NEWSTYPE");
                }
            }
        }
        public ObservableCollection<NewsType> NEWSTYPELIST
        {
            get
            {
                return _newstypelist;
            }
            set
            {
                if (_newstypelist != value)
                {
                    SetValue(ref _newstypelist, value, "NEWSTYPELIST");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string NEWSTITEL
        {
            get { return _newstitel; }
            set
            {
                if (_newstitel != value)
                {
                    SetValue(ref _newstitel, value, "NEWSTITEL");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public byte[] NEWSCONTENT
        {
            get { return _newscontent; }
            set
            {
                if (_newscontent != value)
                {
                    SetValue(ref _newscontent, value, "NEWSCONTENT");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string READCOUNT
        {
            get { return _readcount; }
            set
            {
                if (_readcount != value)
                {
                    Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = "READCOUNT" });
                    _readcount = value;
                    base.RaisePropertyChanged("READCOUNT");
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string COMMENTCOUNT
        {
            get { return _commentcount; }
            set
            {
                Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = "COMMENTCOUNT" });
                if (_commentcount != value)
                {
                    _commentcount = value;
                    base.RaisePropertyChanged("COMMENTCOUNT");
                }
            }
        }
        /// <summary>
        /// 0:未发布，1:已发布,2:已关闭
        /// </summary>
        public NewsState NEWSSTATE
        {
            get
            {
                return _newsstate;
            }
            set
            {
                if (_newsstate != value)
                {
                    SetValue(ref _newsstate, value, "NEWSSTATE");
                }
            }
        }
        public bool ISRELEASE
        {
            get { return _isrelease; }
            set
            {
                if (_isrelease != value)
                {
                    SetValue(ref _isrelease, value, "ISRELEASE");
                }
            }
        }
        public bool ISIMAGE
        {
            get { return _isimage; }
            set
            {
                if (_isimage != value)
                {
                    SetValue(ref _isimage, value, "ISIMAGE");
                }
            }
        }

        public bool ISPOPUP
        {
            get { return _ispopup; }
            set
            {
                if (_ispopup != value)
                {
                    SetValue(ref _ispopup, value, "ISPOPUP");
                }
            }
        }

        public DateTime ENDDATE
        {
            get { return _enddate; }
            set
            {
                if (_enddate != value)
                {
                    SetValue(ref _enddate, value, "ENDDATE");
                }
            }
        }

        public string PUTDEPTNAME
        {
            get { return _putdeptname; }
            set
            {
                if (_putdeptname != value)
                {
                    SetValue(ref _putdeptname, value, "PUTDEPTNAME");
                }
            }
        }
        public string PUTDEPTID
        {
            get { return _putdeptid; }
            set
            {
                if (_putdeptid != value)
                {
                    SetValue(ref _putdeptid, value, "PUTDEPTID");
                }
            }
        }

        public DateTime UPDATEDATE
        {
            get { return _updatedate; }
            set
            {
                if (_updatedate != value)
                {
                    SetValue(ref _updatedate, value, "UPDATEDATE");
                }
            }
        }
        public ObservableCollection<string> VIEWER
        {
            get
            {
                return _viewer;
            }
            set
            {
                if (_viewer != value)
                {
                    SetValue(ref _viewer, value, "VIEWER");
                }
            }
        }
        public ObservableCollection<string> MODELNAME
        {
            get
            {
                return _viewer;
            }
            set
            {
                if (_viewer != value)
                {
                    SetValue(ref _viewer, value, "VIEWER");
                    Validator.ValidateProperty(value, new ValidationContext(this, null, null) { MemberName = "MODELNAME" });
                    _viewer = value;
                    base.RaisePropertyChanged("MODELNAME");
                }
            }
        }
        public ObservableCollection<DISTR> DISTRS
        {
            get
            {
                return _distr;
            }
            set
            {
                if (_distr != value)
                {
                    SetValue(ref _distr, value, "DISTRS");
                }
            }
        }
        public ObservableCollection<NewsState> NEWSSTATELIST
        {
            get
            {
                return _newsstatelist;
            }
            set
            {
                if (_newsstatelist != value)
                {
                    SetValue(ref _newsstatelist, value, "NEWSSTATELIST");
                }
            }
        }
        #endregion

        #region 私有方法 Private Method
        private void SaveNews()
        {
            if (IsAdd)
            {
                AddNews();
            }
            else
            {
                UpdateNews();
            }
        }
        private void AddNews()
        {
            _Services.AddEntityByViewer(ConventToModel(this));
            _Services.OnExectNoQueryCompleted += (obj, args) =>
            {
                if (args.Result)
                {
                    if (OnDataChangedCompleted != null)
                        OnDataChangedCompleted(this, EventArgs.Empty);
                    MessageWindow.Show("", "新增新闻成功！", MessageIcon.Information, MessageWindowType.Flow);
                }
                else
                    MessageWindow.Show("", "新增新闻失败！", MessageIcon.Error, MessageWindowType.Flow);

                if (ParentVM != null)
                    ParentVM.Refresh();
            };
        }
        private void UpdateNews()
        {
            _Services.UpdateEntity(ConventToModel(this));
            _Services.OnExectNoQueryCompleted += (obj, args) =>
            {
                if (args.Result)
                {
                    if (OnDataChangedCompleted != null)
                        OnDataChangedCompleted(this, EventArgs.Empty);
                    MessageWindow.Show("", "修改新闻成功！", MessageIcon.Information, MessageWindowType.Flow);
                }
                else
                    MessageWindow.Show("", "修改新闻失败！", MessageIcon.Error, MessageWindowType.Flow);

                if (ParentVM != null)
                    ParentVM.Refresh();
            };
        }
        private void DeleteNews()
        {
            _Services.DeleteEntity(ConventToModel(this));
            _Services.OnExectNoQueryCompleted += (obj, args) =>
            {
                if (args.Result)
                {
                    if (OnDataChangedCompleted != null)
                        OnDataChangedCompleted(this, EventArgs.Empty);
                    MessageWindow.Show("", "删除新闻成功！", MessageIcon.Information, MessageWindowType.Flow);
                 
                }
                else
                    MessageWindow.Show("", "删除新闻失败！", MessageIcon.Error, MessageWindowType.Flow);

                if (ParentVM != null)
                    ParentVM.Refresh();
            };
        }
        public void LoadDetails()
        {
            _Services.OnGetNewsListCompleted += (obj, args) =>
            {
                if (args.Result.Count > 0)
                {
                    NewsModel model = args.Result[0];

                    ObservableCollection<DISTR> distrs = new ObservableCollection<DISTR>();
                    foreach (var item in model.VIEWER)
                    {
                        distrs.Add(new DISTR() { VIEWERS = item.VIEWERS, MODELNAMES = item.MODELNAMES });
                    }
                    ISRELEASE = model.ISRELEASE;
                    NEWSCONTENT = model.NEWSCONTENT;
                    ISIMAGE = model.ISIMAGE;
                    ISPOPUP = model.ISPOPUP;
                    ENDDATE = model.ENDDATE;
                    PUTDEPTID = model.PUTDEPTID;
                    PUTDEPTNAME = model.PUTDEPTNAME;
                    DISTRS = distrs;
                    if (OnLoadDetailsCompleted != null)
                        OnLoadDetailsCompleted(this, EventArgs.Empty);
                }
            };
            _Services.GetEntityDetails(this.NEWSID);
        }
        private NewsModel ConventToModel(NewsViewModel viewModel)
        {

            ObservableCollection<Model.DISTR> viewer = new ObservableCollection<Model.DISTR>();
            foreach (var item in viewModel.DISTRS)
            {
                viewer.Add(new Model.DISTR() { VIEWERS = item.VIEWERS, MODELNAMES = item.MODELNAMES });
            }

            return new NewsModel()
            {
                NEWSID = viewModel.NEWSID,
                NEWSTITEL = viewModel.NEWSTITEL.Trim(),
                ISRELEASE = viewModel.ISRELEASE,
                NEWSCONTENT = viewModel.NEWSCONTENT,
                NEWSSTATE = viewModel.NEWSSTATE.ID.ToString(),
                NEWSTYPEID = viewModel.NEWSTYPE.ID.ToString(),
                ISIMAGE = viewModel.ISIMAGE,
                VIEWER = viewer,
                ISPOPUP = viewModel.ISPOPUP,
                ENDDATE = viewModel.ENDDATE,
                UPDATEDATE = viewModel.UPDATEDATE,
                PUTDEPTID=viewModel.PUTDEPTID,
                PUTDEPTNAME=viewModel.PUTDEPTNAME

            };
        }
        private NewsViewModel ConventToViewModel(NewsModel model)
        {
            int state = model.NEWSSTATE.IsNotNull() ? int.Parse(model.NEWSSTATE) : 0;
            int type = model.NEWSTYPEID.IsNotNull() ? int.Parse(model.NEWSTYPEID) : 0;
            ObservableCollection<DISTR> distrs = new ObservableCollection<DISTR>();
            foreach (var item in model.VIEWER)
            {
                distrs.Add(new DISTR() { VIEWERS = item.VIEWERS, MODELNAMES = item.MODELNAMES });
            }
            var vmodel = new NewsViewModel()
            {
                NEWSTITEL = model.NEWSTITEL,
                NEWSID = model.NEWSID,
                ISRELEASE = model.ISRELEASE,
                NEWSCONTENT = model.NEWSCONTENT,
                ISIMAGE = model.ISIMAGE,
                ISPOPUP=model.ISPOPUP,
                ENDDATE=model.ENDDATE,
                UPDATEDATE = model.UPDATEDATE,
                DISTRS = distrs
            };
            vmodel.NEWSSTATE = vmodel.NEWSSTATELIST[state];
            vmodel.NEWSTYPE = vmodel.NEWSTYPELIST[type];

            return vmodel;
        }
        #endregion

        #region 公有方法 Public Method
        public ICommand SaveEntity
        {
            get { return new Foundation.RelayCommand(AddNews); }
        }
        public ICommand UpdateEntity
        {
            get { return new Foundation.RelayCommand(UpdateNews); }
        }
        public ICommand DeleteEntity
        {
            get { return new Foundation.RelayCommand(DeleteNews); }
        }
        #endregion

        #region 事件函数 Event Hanlder
        public event EventHandler OnDataChangedCompleted;
        public event EventHandler<AddEventArgs> OnAddCompleted;
        public event EventHandler OnLoadDetailsCompleted;

        #endregion
    }
    public class NewsState : BasicViewModel
    {
        private int _id;
        private string _name;

        public int ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    SetValue(ref _id, value, "ID");
                }
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    SetValue(ref _name, value, "Name");
                }
            }
        }

    }
    public class DISTR : BasicViewModel
    {
        private string _viewers = string.Empty;
        private string _modelnames = string.Empty;

        public string VIEWERS
        {
            get
            {
                return _viewers;
            }
            set
            {
                if (_viewers != value)
                {
                    SetValue(ref _viewers, value, "VIEWERS");
                }
            }
        }
        public string MODELNAMES
        {
            get
            {
                return _modelnames;
            }
            set
            {
                if (_modelnames != value)
                {
                    SetValue(ref _modelnames, value, "MODELNAMES");
                }
            }
        }
    }
    public class NewsType : BasicViewModel
    {
        private int _id;
        private string _name;

        public int ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    SetValue(ref _id, value, "ID");
                }
            }
        }
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    SetValue(ref _name, value, "Name");
                }
            }
        }

    }
    public class AddEventArgs : EventArgs
    {
        public string NewsID { get; set; }
        public AddEventArgs(string newsid)
        {
            this.NewsID = newsid;
        }
    }
}
