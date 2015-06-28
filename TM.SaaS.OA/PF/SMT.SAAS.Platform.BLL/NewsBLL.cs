using System;
using System.Collections.Generic;
using System.Linq;
using TM_SaaS_OA_EFModel;
using System.Linq.Dynamic;
using SMT.Foundation.Log;
using System.Text;
using System.Diagnostics;
using SMT.SAAS.Platform.Model;

namespace SMT.SAAS.Platform.BLL
{

    public class NewsBLL : BaseBll<T_PF_NEWS>
    {
        public T_PF_NEWS GetEntityByID(string _newsID)
        {
            try
            {
                if (_newsID.Length == 0)
                    throw new Exception("查询参数 NewsID 不能为空!!");

                var ents = from o in dal.GetObjects()
                           where o.NEWSID == _newsID
                           select o;

                return ents.Count() > 0 ? ents.FirstOrDefault() : null;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }
        #region 基础操作
        public bool AddEntity(T_PF_NEWS _entityModel)
        {
            try
            {
                var temp = dal.GetObjects().FirstOrDefault(s => (s.NEWSID == _entityModel.NEWSID));
                if (temp != null)
                {
                    throw new Exception("Repetition");
                }

                bool result = dal.Add(_entityModel);

                return result;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }
        /// <summary>
        /// 新闻表与关联表添加功能
        /// </summary>
        /// <param name="_entityModel">新闻实体</param>
        /// <param name="viewer">新闻关联表实体</param>
        /// <returns></returns>
        public bool AddEntity(T_PF_NEWS _entityModel, List<T_PF_DISTRIBUTEUSER> viewer)
        {
            try
            {
                if (viewer.Count > 0)
                {
                    bool result = AddEntity(_entityModel);
                    int results = 0;
                    if (result)  //判断新闻表是否添加成功
                    {
                        foreach (var item in viewer)
                        {
                            results = dal.Add(item);
                        }

                    }
                    return results > 0 ? true : false;
                }
                return false;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }
        public bool UpdateEntity(T_PF_NEWS _entityModel, List<T_PF_DISTRIBUTEUSER> viewer)
        {
            try
            {
                var ents = from ent in dal.GetObjects()
                           where ent.NEWSID == _entityModel.NEWSID
                           select ent;
                if (ents.Count() > 0)
                {
                    var ent = ents.FirstOrDefault();

                    Utility.CloneEntity(_entityModel, ent);

                    bool flag = dal.Update(ent);
                    int result = 0;
                    if (flag)
                    {
                        result = Entity(ent.NEWSID, viewer);
                    }
                    return result > 0 ? true : false;
                }
                return false;

            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }
        public int Entity(string _newsid, List<T_PF_DISTRIBUTEUSER> viewer)
        {
            try
            {
                var delete = from o in dal.GetObjects<T_PF_DISTRIBUTEUSER>()
                             where o.FORMID == _newsid
                             select o;
                //获取实体是否存在
                int result = 0;
                if (delete.Count() > 0)
                {
                    foreach (var item in delete) //遍历实体
                    {
                        dal.DeleteFromContext(item);
                    }

                    result = dal.SaveContextChanges();
                }

                if (result > 0)
                {
                    foreach (var item in viewer)
                    {
                        result = dal.Add(item);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return 0;
            }
        }
        public bool DeleteEntity(string _newsID)
        {
            try
            {
                var ents = from e in dal.GetObjects()
                           where e.NEWSID == _newsID
                           select e;
                var delete = from o in dal.GetObjects<T_PF_DISTRIBUTEUSER>()
                             where o.FORMID == _newsID
                             select o;

                //获取实体是否存在
                if (delete.Count() > 0)
                {
                    foreach (var item in delete) //遍历实体
                    {
                        dal.DeleteFromContext(item);
                    }

                    dal.SaveContextChanges();
                }

                //获取实体是否存在
                var ent = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                if (ent != null)
                {
                    dal.DeleteFromContext(ent);
                }
                int result = dal.SaveContextChanges();
                return result > 0 ? true : false;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return false;
            }
        }
        #endregion
        #region 自定义操作

        public T_PF_NEWSView GetEntityViewByID(string _newsID)
        {
            try
            {
                var ents = from o in dal.GetObjects()
                           where o.NEWSID == _newsID
                           select o;

                T_PF_NEWS newsEntity = ents.Count() > 0 ? ents.FirstOrDefault() : null;

                var distrlist = from distr in dal.GetObjects<T_PF_DISTRIBUTEUSER>()
                                where distr.FORMID == newsEntity.NEWSID
                                select distr;
                T_PF_NEWSView result = new T_PF_NEWSView();

                result.T_PF_DISTRIBUTEUSERS = distrlist.ToList();
                result.T_PF_NEWS = newsEntity;
                return result;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }

        /// <summary>
        /// 获取所有结果
        /// </summary>
        /// <returns></returns>
        public List<T_PF_NEWS> GetNewsList()
        {
            try
            {
                var ents = from ent in dal.GetObjects()
                           select ent;

                return ents.Count() > 0 ? ents.ToList() : new List<T_PF_NEWS>();
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }

        public Dictionary<T_PF_NEWS, List<T_PF_DISTRIBUTEUSER>> GetNewsLists()
        {
            try
            {
                Dictionary<T_PF_NEWS, List<T_PF_DISTRIBUTEUSER>> dic = new Dictionary<T_PF_NEWS, List<T_PF_DISTRIBUTEUSER>>();
                List<T_PF_DISTRIBUTEUSER> dic2 = new List<T_PF_DISTRIBUTEUSER>();
                var newsList = from news in dal.GetObjects()
                               select news;
                foreach (var item in newsList.ToList())
                {
                    var distrlist = from distr in dal.GetObjects<T_PF_DISTRIBUTEUSER>()
                                    where distr.FORMID == item.NEWSID
                                    select distr;

                    dic.Add(item, distrlist.ToList());
                }
                return dic;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }

        /// <summary>
        /// 根据参数获取结果
        /// </summary>
        /// <param name="type">新闻类型</param>
        /// <param name="topCount">前topCount条</param>
        /// <param name="state">新闻状态</param>
        /// <returns>结果集</returns>
        public List<T_PF_NEWSListView> GetPopupNewsList()
        {
            try
            {
                var newsList = from news in dal.GetObjects()
                               where news.NEWSSTATE == "1" && news.ENDDATE >= DateTime.Now && news.ISPOPUP == "1"
                               orderby news.UPDATEDATE descending
                               select new T_PF_NEWSListView
                               {
                                   NEWSID = news.NEWSID,
                                   NEWSTITEL = news.NEWSTITEL,
                                   UPDATEDATE = news.UPDATEDATE,
                                   NEWSTYPEID = news.NEWSTYPEID,
                                   NEWSTATE = news.NEWSSTATE
                               };
                return newsList.ToList();
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }

        }

        /// <summary>
        /// 根据参数获取结果
        /// </summary>
        /// <param name="type">新闻类型</param>
        /// <param name="topCount">前topCount条</param>
        /// <param name="state">新闻状态</param>
        /// <returns>结果集</returns>
        public List<T_PF_NEWS> GetNewsList(string type, int topCount, string state)
        {
            try
            {
                string[] types = type.Split('|');
                string types1 = types[0];
                string types2 = types[1];

                var ents = from ent in dal.GetObjects()
                           where ent.NEWSSTATE == state
                           //&& (ent.NEWSTYPEID.Contains(type))
                           orderby ent.UPDATEDATE descending
                           select new
                           {
                               NEWSID = ent.NEWSID,
                               NEWSTITEL = ent.NEWSTITEL,
                               UPDATEDATE = ent.UPDATEDATE,
                               NEWSTYPEID = ent.NEWSTYPEID,
                               PUTDEPTNAME= ent.PUTDEPTNAME
                           };

                var topList = ents.ToList().Take(topCount);

                List<T_PF_NEWS> result = new List<T_PF_NEWS>();
                if (topList.Count() > 0)
                {
                    foreach (var item in topList)
                    {
                        T_PF_NEWS news = new T_PF_NEWS()
                        {
                            NEWSID = item.NEWSID,
                            NEWSTITEL = item.NEWSTITEL,
                            UPDATEDATE = item.UPDATEDATE,
                            NEWSTYPEID = item.NEWSTYPEID,
                            PUTDEPTNAME = item.PUTDEPTNAME
                        };
                        result.Add(news);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }

        }

        /// <summary>
        /// 获取指定状态的新闻
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns>结果列表</returns>
        public List<T_PF_NEWS> GetNewsListByState(string state)
        {
            try
            {
                var ents = from ent in dal.GetObjects()
                           where ent.NEWSSTATE == state
                           orderby ent.UPDATEDATE descending
                           select new
                           {
                               NEWSID = ent.NEWSID,
                               NEWSTITEL = ent.NEWSTITEL,
                               UPDATEDATE = ent.UPDATEDATE,
                               NEWSTYPEID = ent.NEWSTYPEID
                           };
                List<T_PF_NEWS> result = new List<T_PF_NEWS>();
                if (ents.Count() > 0)
                {
                    foreach (var item in ents)
                    {
                        T_PF_NEWS news = new T_PF_NEWS()
                        {
                            NEWSID = item.NEWSID,
                            NEWSTITEL = item.NEWSTITEL,
                            UPDATEDATE = item.UPDATEDATE,
                            NEWSTYPEID = item.NEWSTYPEID
                        };
                        result.Add(news);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }

        /// <summary>
        /// 获取新闻图片
        /// </summary>
        /// <param name="topCount">获取多少条</param>
        /// <param name="state">新闻状态</param>
        /// <returns></returns>
        public List<T_PF_NEWS> GetImageNews(int topCount, string state)
        {
            try
            {
                var ents = from ent in dal.GetObjects()
                           where ent.NEWSSTATE == state && ent.ISIMAGE == "1"
                           orderby ent.UPDATEDATE descending
                           select new
                          {
                              NEWSID = ent.NEWSID,
                              NEWSTITEL = ent.NEWSTITEL,
                              UPDATEDATE = ent.UPDATEDATE,
                              NEWSTYPEID = ent.NEWSTYPEID
                          };

                //悲剧的Top
                var topList = ents.ToList().Take(topCount);

                List<T_PF_NEWS> result = new List<T_PF_NEWS>();
                if (topList.Count() > 0)
                {
                    foreach (var item in topList)
                    {
                        T_PF_NEWS news = new T_PF_NEWS()
                        {
                            NEWSID = item.NEWSID,
                            NEWSTITEL = item.NEWSTITEL,
                            UPDATEDATE = item.UPDATEDATE,
                            NEWSTYPEID = item.NEWSTYPEID
                        };
                        result.Add(news);
                    }
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }

        public List<T_PF_NEWSListView> GetNewsListByPage(int pageIndex, int pageSize, string sort, string filterString, ref int pageCount)
        {
            try
            {
                var newsList = from news in dal.GetObjects()
                               orderby news.UPDATEDATE descending
                               select new T_PF_NEWSListView
                               {
                                   NEWSID = news.NEWSID,
                                   NEWSTITEL = news.NEWSTITEL,
                                   UPDATEDATE = news.UPDATEDATE,
                                   NEWSTYPEID = news.NEWSTYPEID,
                                   NEWSTATE = news.NEWSSTATE
                               };

                var list = newsList.ToList();

                List<T_PF_NEWSListView> newsList2 = Utility.Pager<T_PF_NEWSListView>(list, pageIndex, pageSize, ref pageCount);
                return newsList2;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }

        public Dictionary<T_PF_NEWS, List<T_PF_DISTRIBUTEUSER>> GetNewsListsByPage(int pageIndex, int pageSize, string sort, string filterString, ref int pageCount)
        {
            try
            {
                Dictionary<T_PF_NEWS, List<T_PF_DISTRIBUTEUSER>> dic = new Dictionary<T_PF_NEWS, List<T_PF_DISTRIBUTEUSER>>();
                List<T_PF_DISTRIBUTEUSER> dic2 = new List<T_PF_DISTRIBUTEUSER>();

                var newsList = from news in dal.GetObjects()
                               orderby news.UPDATEDATE descending
                               select news;

                List<T_PF_NEWS> newsList2 = Utility.Pager<T_PF_NEWS>(newsList.ToList(), pageIndex, pageSize, ref pageCount);

                foreach (var item in newsList2.ToList())
                {
                    var distrlist = from distr in dal.GetObjects<T_PF_DISTRIBUTEUSER>()
                                    where distr.FORMID == item.NEWSID
                                    select distr;

                    dic.Add(item, distrlist.ToList());
                }
                return dic;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }
        public List<T_PF_NEWSListView> GetNewsListByMobile(int pageIndex, int pageSize, string sort, string filterString, ref int pageCount, ref int rowCount)
        {
            try
            {
                var newsList = from news in dal.GetObjects()
                               orderby news.UPDATEDATE descending
                               select new T_PF_NEWSListView
                               {
                                   NEWSID = news.NEWSID,
                                   NEWSTITEL = news.NEWSTITEL,
                                   UPDATEDATE = news.UPDATEDATE,
                                   NEWSTYPEID = news.NEWSTYPEID,
                                   NEWSTATE = news.NEWSSTATE
                               };

                var list = newsList.ToList();

                List<T_PF_NEWSListView> newsList2 = Utility.Pager<T_PF_NEWSListView>(list, pageIndex, pageSize, ref pageCount);
                rowCount = list.Count;
                return newsList2;
            }
            catch (Exception ex)
            {
                #region Write Error Log
                StackFrame frame = (new StackTrace(true)).GetFrame(1);
                Utility.Log(this.GetType().FullName, frame.GetMethod().Name, ex);
                #endregion

                return null;
            }
        }
        #endregion
        #region 执行处理方法
        public void BeginTransaction()
        {
            dal.BeginTransaction();
        }
        public void CommitTransaction()
        {
            dal.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            dal.RollbackTransaction();
        }
        #endregion
    }
}
