using System;
using System.Collections.ObjectModel;
using SMT.SAAS.Platform.Model;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.Generic;
using SMT.SAAS.Platform.WebParts.Models;
using SMT.SAAS.Platform.WebParts.NewsWS;
using SMT.SAAS.Platform.WebParts.NewsCallBackWS;

namespace SMT.SAAS.Platform.WebParts.ClientServices
{
    /// <summary>
    /// 封装平台访问发布新闻的WCF操作。
    /// </summary>
    public class NewsServices
    {

        SMT.Saas.Tools.PublicInterfaceWS.PublicServiceClient publicWS;
        /// <summary>
        /// 获取新闻列表完成事件
        /// </summary>
        public event EventHandler<GetEntityListEventArgs<NewsModel>> OnGetNewsListCompleted;
        /// <summary>
        /// 执行操作完成事件，比如增删改查
        /// </summary>
        public event EventHandler<ExectNoQueryEventArgs> OnExectNoQueryCompleted;

        private static NewsModel _currentAddModel=new NewsModel();
        private static NewsModel _currentUpdateModel=new NewsModel();
        private static NewsModel _currentDeleteModel=new NewsModel();
        ObservableCollection<NewsModel> _currentGetDetailslist = new ObservableCollection<NewsModel>();
        #region 构造函数 Constructed
        public NewsServices()
        {
            servers = new BasicServices();
            client = servers.PlatformClient;
            callBackClient = servers.CallBackClient;
            publicWS = new Saas.Tools.PublicInterfaceWS.PublicServiceClient();

            RegisterServices();
        }
        #endregion

        #region 私有成员 Private Member
        private PlatformServicesClient client = null;
        private NewsCallBackClient callBackClient = null;
        private BasicServices servers = null;
        public string NewsId = string.Empty; //新闻ID
        #endregion

        #region 私有方法 Private Method
        private void RegisterServices()
        {
            //根据ID获取单条新闻详细内容
            client.GetNewsDetailsByIDCompleted += new EventHandler<GetNewsDetailsByIDCompletedEventArgs>(client_GetNewsDetailsByIDCompleted);
            //获取所有新闻列表
            client.GetNewsListsCompleted += new EventHandler<GetNewsListsCompletedEventArgs>(client_GetNewsListsCompleted);
            //分页获取新闻
            //client.GetNewsListByPageCompleted += new EventHandler<GetNewsListByPageCompletedEventArgs>(client_GetNewsListByPageCompleted);
            client.GetNewsListByPageAndEmpIDCompleted += new EventHandler<GetNewsListByPageAndEmpIDCompletedEventArgs>(client_GetNewsListByPageAndEmpIDCompleted);
            
            client.AddNewsCompleted += new EventHandler<AddNewsCompletedEventArgs>(client_AddNewsCompleted);
            client.DeleteNewsCompleted += new EventHandler<DeleteNewsCompletedEventArgs>(client_DeleteNewsCompleted);
            client.GetPopupNewsListCompleted += new EventHandler<GetPopupNewsListCompletedEventArgs>(client_GetPopupNewsListCompleted);
            publicWS.AddContentCompleted += new EventHandler<Saas.Tools.PublicInterfaceWS.AddContentCompletedEventArgs>(publicWS_AddContentCompleted);
            publicWS.UpdateContentCompleted += new EventHandler<Saas.Tools.PublicInterfaceWS.UpdateContentCompletedEventArgs>(publicWS_UpdateContentCompleted);
            publicWS.DeleteContentCompleted += new EventHandler<Saas.Tools.PublicInterfaceWS.DeleteContentCompletedEventArgs>(publicWS_DeleteContentCompleted);
            publicWS.GetContentCompleted += new EventHandler<Saas.Tools.PublicInterfaceWS.GetContentCompletedEventArgs>(publicWS_GetContentCompleted);
        }

        void client_GetNewsListByPageAndEmpIDCompleted(object sender, GetNewsListByPageAndEmpIDCompletedEventArgs e)
        {
            ObservableCollection<NewsModel> list = new ObservableCollection<NewsModel>();
            if (e.Error == null)
            {
                if (e.Result.IsNotNull())
                {
                    if (e.Result.Count > 0)
                    {
                        foreach (var item in e.Result)
                        {
                            list.Add(EntityViewToModel(item));
                        }
                    }
                }
                if (OnGetNewsListCompleted != null)
                    OnGetNewsListCompleted(this, new GetEntityListEventArgs<NewsModel>(list, e.Error, e.pageCount));
            }
        }

        void client_GetPopupNewsListCompleted(object sender, GetPopupNewsListCompletedEventArgs e)
        {
            ObservableCollection<NewsModel> list = new ObservableCollection<NewsModel>();
            if (e.Error == null)
            {
                if (e.Result.IsNotNull())
                {
                    if (e.Result.Count > 0)
                    {
                        foreach (var item in e.Result)
                        {
                            list.Add(EntityViewToModel(item));
                        }
                    }
                }
            }
            if (OnGetNewsListCompleted != null)
                OnGetNewsListCompleted(this, new GetEntityListEventArgs<NewsModel>(list, e.Error));
        }

        void publicWS_GetContentCompleted(object sender, Saas.Tools.PublicInterfaceWS.GetContentCompletedEventArgs e)
        {

            _currentGetDetailslist[0].NEWSCONTENT = e.Result;

            if (OnGetNewsListCompleted != null)
                OnGetNewsListCompleted(this, new GetEntityListEventArgs<NewsModel>(_currentGetDetailslist, e.Error));
        }

        void publicWS_DeleteContentCompleted(object sender, Saas.Tools.PublicInterfaceWS.DeleteContentCompletedEventArgs e)
        {
            client.DeleteNewsAsync(_currentDeleteModel.NEWSID);
        }

        void publicWS_UpdateContentCompleted(object sender, Saas.Tools.PublicInterfaceWS.UpdateContentCompletedEventArgs e)
        {
            SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS _entity = ModelToEntity(_currentUpdateModel);
            ObservableCollection<T_PF_DISTRIBUTEUSER> _buteuser = ModelEntity(_currentUpdateModel);
            client.UpdateNewsAsync(_entity, _buteuser);
            client.UpdateNewsCompleted += (obj, args) =>
            {
                bool result = false;
                if (args.Error.IsNull())
                {
                    result = args.Result;
                }
                if (OnExectNoQueryCompleted != null)
                    OnExectNoQueryCompleted(this, new ExectNoQueryEventArgs(result, args.Error));

                if (result)
                {
                    callBackClient.TalkNewsAsync(new SMT.SAAS.Platform.WebParts.NewsCallBackWS.T_PF_NEWS()
                    {
                        COMMENTCOUNT = _entity.COMMENTCOUNT,
                        CREATECOMPANYID = _entity.CREATECOMPANYID,
                        CREATEDATE = _entity.CREATEDATE,
                        CREATEDEPARTMENTID = _entity.CREATEDEPARTMENTID,
                        CREATEPOSTID = _entity.CREATEPOSTID,
                        CREATEUSERID = _entity.CREATEPOSTID,
                        CREATEUSERNAME = _entity.CREATEUSERNAME,
                        NEWSCONTENT = new byte[0],
                        NEWSID = _entity.NEWSID,
                        NEWSSTATE = _entity.NEWSSTATE,
                        NEWSTITEL = _entity.NEWSTITEL,
                        NEWSTYPEID = _entity.NEWSTYPEID,
                        OWNERCOMPANYID = _entity.OWNERCOMPANYID,
                        OWNERDEPARTMENTID = _entity.OWNERDEPARTMENTID,
                        OWNERID = _entity.OWNERID,
                        OWNERNAME = _entity.OWNERNAME,
                        OWNERPOSTID = _entity.OWNERPOSTID,
                        READCOUNT = _entity.READCOUNT,
                        UPDATEDATE = _entity.UPDATEDATE,
                        UPDATEUSERID = _entity.UPDATEUSERID,
                        UPDATEUSERNAME = _entity.UPDATEUSERNAME

                    });
                }
            };
        }

        void publicWS_AddContentCompleted(object sender, Saas.Tools.PublicInterfaceWS.AddContentCompletedEventArgs e)
        {
            SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS _entity = ModelToEntity(_currentAddModel);
            ObservableCollection<T_PF_DISTRIBUTEUSER> _buteuser = ModelEntity(_currentAddModel);

            _entity.NEWSCONTENT = new byte[0];

            client.AddNewsByViewerAsync(_entity, _buteuser);
            client.AddNewsByViewerCompleted += (obj, args) =>
            {
                bool result = false;
                if (args.Error.IsNull())
                {
                    result = args.Result;
                }
                if (OnExectNoQueryCompleted != null)
                    OnExectNoQueryCompleted(this, new ExectNoQueryEventArgs(result, args.Error));
                if (result)
                {
                    callBackClient.TalkNewsAsync(new SMT.SAAS.Platform.WebParts.NewsCallBackWS.T_PF_NEWS()
                    {
                        COMMENTCOUNT = _entity.COMMENTCOUNT,
                        CREATECOMPANYID = _entity.CREATECOMPANYID,
                        CREATEDATE = _entity.CREATEDATE,
                        CREATEDEPARTMENTID = _entity.CREATEDEPARTMENTID,
                        CREATEPOSTID = _entity.CREATEPOSTID,
                        CREATEUSERID = _entity.CREATEPOSTID,
                        CREATEUSERNAME = _entity.CREATEUSERNAME,
                        NEWSCONTENT = new byte[0],
                        NEWSID = _entity.NEWSID,
                        NEWSSTATE = _entity.NEWSSTATE,
                        NEWSTITEL = _entity.NEWSTITEL,
                        NEWSTYPEID = _entity.NEWSTYPEID,
                        OWNERCOMPANYID = _entity.OWNERCOMPANYID,
                        OWNERDEPARTMENTID = _entity.OWNERDEPARTMENTID,
                        OWNERID = _entity.OWNERID,
                        OWNERNAME = _entity.OWNERNAME,
                        OWNERPOSTID = _entity.OWNERPOSTID,
                        READCOUNT = _entity.READCOUNT,
                        UPDATEDATE = _entity.UPDATEDATE,
                        UPDATEUSERID = _entity.UPDATEUSERID,
                        UPDATEUSERNAME = _entity.UPDATEUSERNAME

                    });
                }
            };
        }

        private NewsModel EntityToModel(SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS entity, ObservableCollection<T_PF_DISTRIBUTEUSER> disentity)
        {
            ObservableCollection<Model.DISTR> _viewer = new ObservableCollection<Model.DISTR>();
            if (disentity != null)
            {
                foreach (var item in disentity)
                {
                    //_viewer.Add(new Model.DISTR() { MODELNAMES = item.MODELNAME, VIEWERS = item.FORMID });//我去，这种事情会发生
                    _viewer.Add(new Model.DISTR() { MODELNAMES = item.MODELNAME, VIEWERS = item.VIEWER });
                }
            }

            NewsModel vm =  new NewsModel()
             {
                 NEWSTITEL = entity.NEWSTITEL,
                 NEWSID = entity.NEWSID,
                 ISRELEASE = true,
                 NEWSCONTENT = entity.NEWSCONTENT,
                 NEWSSTATE = entity.NEWSSTATE,
                 NEWSTYPEID = entity.NEWSTYPEID,
                 UPDATEDATE = Convert.ToDateTime(entity.UPDATEDATE),
                 ISIMAGE = entity.ISIMAGE == "0" ? false : true,
                 ISPOPUP = entity.ISPOPUP==null?false:( entity.ISPOPUP == "0" ? false : true),
                 ENDDATE=entity.ENDDATE==null?DateTime.Now:(DateTime)entity.ENDDATE,
                 PUTDEPTID=entity.PUTDEPTID,
                 PUTDEPTNAME=entity.PUTDEPTNAME,
                 VIEWER = _viewer
             };
            return vm;
        }

        private NewsModel EntityViewToModel(T_PF_NEWSListView entityView)
        {
            return new NewsModel()
            {
                NEWSTITEL = entityView.NEWSTITEL,
                NEWSID = entityView.NEWSID,
                NEWSTYPEID = entityView.NEWSTYPEID,
                UPDATEDATE = Convert.ToDateTime(entityView.UPDATEDATE),
                NEWSSTATE = entityView.NEWSTATE,
                 
            };
        }

        private SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS ModelToEntity(NewsModel clientModel)
        {

            DateTime dt = new DateTime(clientModel.UPDATEDATE.Year, clientModel.UPDATEDATE.Month, clientModel.UPDATEDATE.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS model = new SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS()
            {
                CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID,
                CREATEDATE = DateTime.Now,
                CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,
                CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID,
                CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID,
                CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName,
                OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID,
                OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,
                OWNERID = Common.CurrentLoginUserInfo.EmployeeID,
                OWNERNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName,
                OWNERPOSTID = Common.CurrentLoginUserInfo.EmployeeID,
                UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID,
                UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName,



                UPDATEDATE = dt,

                NEWSID = clientModel.NEWSID.Length > 0 ? clientModel.NEWSID : Guid.NewGuid().ToString(),
                NEWSCONTENT = clientModel.NEWSCONTENT,
                NEWSSTATE = clientModel.NEWSSTATE,
                NEWSTITEL = clientModel.NEWSTITEL,
                COMMENTCOUNT = "0",
                READCOUNT = "0",
                ISIMAGE = clientModel.ISIMAGE ? "1" : "0",
                ISPOPUP = clientModel.ISPOPUP ? "1" : "0",
                ENDDATE=clientModel.ENDDATE,
                NEWSTYPEID = clientModel.NEWSTYPEID,
                PUTDEPTID=clientModel.PUTDEPTID,
                PUTDEPTNAME=clientModel.PUTDEPTNAME
            };
            NewsId = model.NEWSID.ToString();
            return model;
        }

        /// <summary>
        /// 实体类NewsModel值添加到List<T_PF_DISTRIBUTEUSER>
        /// </summary>
        /// <param name="cilientModel"></param>
        /// <returns></returns>
        private ObservableCollection<T_PF_DISTRIBUTEUSER> ModelEntity(NewsModel cilientModel)
        {

            ObservableCollection<T_PF_DISTRIBUTEUSER> ButeUserModel = new ObservableCollection<T_PF_DISTRIBUTEUSER>();
            // NewsId = cilientModel.NEWSID.Length > 0 ? cilientModel.NEWSID : Guid.NewGuid().ToString();

            foreach (var item in cilientModel.VIEWER)
            {
                T_PF_DISTRIBUTEUSER model = new T_PF_DISTRIBUTEUSER()
                {
                    CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID,
                    CREATEDATE = DateTime.Now,
                    CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,
                    CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID,
                    CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID,
                    CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName,
                    OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID,
                    OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,
                    OWNERID = Common.CurrentLoginUserInfo.EmployeeID,
                    OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName,
                    OWNERPOSTID = Common.CurrentLoginUserInfo.EmployeeID,
                    UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID,
                    UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName,
                    UPDATEDATE = DateTime.Now,

                    //新闻关联表ID赋值
                    DISTRIBUTEUSERID = Guid.NewGuid().ToString(),
                    MODELNAME = item.MODELNAMES,
                    //新闻ID赋值
                    FORMID = NewsId,//cilientModel.NEWSID.Length > 0 ? cilientModel.NEWSID : Guid.NewGuid().ToString(),
                    VIEWTYPE = "3",
                    //用户权限赋值
                    VIEWER = item.VIEWERS
                };
                ButeUserModel.Add(model);
            }
            return ButeUserModel;
        }
        #endregion

        #region 公有方法 Public Method
        /// <summary>
        /// 获取新闻列表
        /// </summary>
        public void GetPopupNewsList()
        {
            client.GetPopupNewsListAsync();
        }

        /// <summary>
        /// 获取新闻列表
        /// </summary>
        public void GetNewsList()
        {
            client.GetNewsListsAsync("");
        }

        public void GetNewsListByPage(int pageIndex, int pageSize, string sortString, string filterString, int pageCount)
        {
            //client.GetNewsListByPageAsync(pageIndex, pageSize, sortString, filterString, pageCount);
            client.GetNewsListByPageAndEmpIDAsync(pageIndex, pageSize, sortString, filterString, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        /// <summary>
        /// 根据给定关键字获取详细信息
        /// </summary>
        /// <param name="newsID">新闻ID</param>
        public void GetEntityDetails(string newsID)
        {
            try
            {
                client.GetNewsDetailsByIDAsync(newsID);
            }
            catch (Exception ex)
            {
               
            }
            
        }
        /// <summary>
        /// 新增数据到数据库
        /// </summary>
        /// <param name="clientModel">数据实体</param>
        public void AddEntity(NewsModel clientModel)
        {
            _currentAddModel = clientModel;
            SMT.SAAS.Platform.WebParts.NewsWS.T_PF_NEWS _entity = ModelToEntity(clientModel);
            publicWS.AddContentAsync(_entity.NEWSID, _entity.NEWSCONTENT, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, "Platform", "News", new Saas.Tools.PublicInterfaceWS.UserInfo()
            {
                COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID,
                DEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,
                POSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID,
                USERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID,
                USERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName
            });

            //client.AddNewsAsync(_entity);
        }
        public void AddEntityByViewer(NewsModel clientModel)
        {
            _currentAddModel = clientModel;

            publicWS.AddContentAsync(_currentAddModel.NEWSID, _currentAddModel.NEWSCONTENT, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, "Platform", "News", new Saas.Tools.PublicInterfaceWS.UserInfo()
            {
                COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID,
                DEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,
                POSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID,
                USERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID,
                USERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName
            });
        }
        public void UpdateEntity(NewsModel clientModel)
        {
            _currentUpdateModel = clientModel;
            publicWS.UpdateContentAsync(_currentUpdateModel.NEWSID, _currentUpdateModel.NEWSCONTENT, new Saas.Tools.PublicInterfaceWS.UserInfo()
            {
                COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID,
                DEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID,
                POSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID,
                USERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID,
                USERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName
            });

        }
        public void DeleteEntity(NewsModel clientModel)
        {
            _currentDeleteModel = clientModel;
            publicWS.DeleteContentAsync(_currentDeleteModel.NEWSID);

        }
        #endregion

        #region 事件函数 Event Hanlder
        private void client_DeleteNewsCompleted(object sender, DeleteNewsCompletedEventArgs e)
        {
            bool result = false;
            if (e.Error.IsNull())
            {
                result = e.Result;
            }
            if (OnExectNoQueryCompleted != null)
                OnExectNoQueryCompleted(this, new ExectNoQueryEventArgs(result, e.Error));
        }

        private void client_AddNewsCompleted(object sender, AddNewsCompletedEventArgs e)
        {
            bool result = false;
            if (e.Error.IsNull())
            {
                result = e.Result;
            }
            if (OnExectNoQueryCompleted != null)
                OnExectNoQueryCompleted(this, new ExectNoQueryEventArgs(result, e.Error));
        }

        private void client_GetNewsListsCompleted(object sender, GetNewsListsCompletedEventArgs e)
        {
            ObservableCollection<NewsModel> list = new ObservableCollection<NewsModel>();
            if (e.Error == null)
            {
                if (e.Result.IsNotNull())
                {
                    if (e.Result.Count > 0)
                    {
                        foreach (var item in e.Result)
                        {
                            list.Add(EntityToModel(item.Key, item.Value));
                        }
                    }
                }
            }
            if (OnGetNewsListCompleted != null)
                OnGetNewsListCompleted(this, new GetEntityListEventArgs<NewsModel>(list, e.Error));
        }

        //private void client_GetNewsListByPageCompleted(object sender, GetNewsListByPageCompletedEventArgs e)
        //{
        //    ObservableCollection<NewsModel> list = new ObservableCollection<NewsModel>();
        //    if (e.Error == null)
        //    {
        //        if (e.Result.IsNotNull())
        //        {
        //            if (e.Result.Count > 0)
        //            {
        //                foreach (var item in e.Result)
        //                {
        //                    list.Add(EntityViewToModel(item));
        //                }
        //            }
        //        }
        //        if (OnGetNewsListCompleted != null)
        //            OnGetNewsListCompleted(this, new GetEntityListEventArgs<NewsModel>(list, e.Error, e.pageCount));
        //    }
        //}

        private void client_GetNewsDetailsByIDCompleted(object sender, GetNewsDetailsByIDCompletedEventArgs e)
        {
            ObservableCollection<NewsModel> list = new ObservableCollection<NewsModel>();
            if (e.Error == null)
            {
                if (e.Result.IsNotNull())
                {
                    list.Add(EntityToModel(e.Result.T_PF_NEWS, e.Result.T_PF_DISTRIBUTEUSERS));
                    _currentGetDetailslist = list;
                    publicWS.GetContentAsync(e.Result.T_PF_NEWS.NEWSID);
                }

                //if (OnGetNewsListCompleted != null)
                //    OnGetNewsListCompleted(this, new GetEntityListEventArgs<NewsModel>(list, e.Error));
            }
        }
        #endregion
    }
}
