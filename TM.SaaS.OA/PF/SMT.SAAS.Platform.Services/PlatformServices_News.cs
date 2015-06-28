using System;
using System.Collections.Generic;
using TM_SaaS_OA_EFModel;
using System.Runtime.Serialization;
using SMT.SAAS.Platform.Model;
using System.ServiceModel;

namespace SMT.SAAS.Platform.Services
{
    //[KnownType(typeof(T_PF_NEWS))]
    //[KnownType(typeof(T_PF_DISTRIBUTEUSER))]
    public partial class PlatformServices
    {
        private SMT.SAAS.Platform.BLL.NewsBLL _newsbll = new SMT.SAAS.Platform.BLL.NewsBLL();

        #region IPlatformServices 成员
       [OperationContract]
        public bool AddNews(TM_SaaS_OA_EFModel.T_PF_NEWS model)
        {
            try
            {
                bool result = false;
                result = _newsbll.AddEntity(model);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("添加新闻异常:" + ex.Message);
            }
        }
        /// <summary>
        /// 新闻表与关联表添加功能       
        /// </summary>
        /// <param name="model">新闻实体</param>
        /// <param name="_viewer">新闻关联表实体</param>
        /// <returns></returns>
        [OperationContract]
        public bool AddNewsByViewer(TM_SaaS_OA_EFModel.T_PF_NEWS model, List<TM_SaaS_OA_EFModel.T_PF_DISTRIBUTEUSER> _viewer)
        {
            bool result = false;
            try
            {
                result = _newsbll.AddEntity(model, _viewer);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("添加新闻异常:" + ex.Message);
            }

        }
          [OperationContract]
        public bool UpdateNews(TM_SaaS_OA_EFModel.T_PF_NEWS model, List<TM_SaaS_OA_EFModel.T_PF_DISTRIBUTEUSER> _viewer)
        {
            try
            {
                bool result = false;
                result = _newsbll.UpdateEntity(model, _viewer);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("添加新闻异常:" + ex.Message);
            }
        }
          [OperationContract]
        public bool DeleteNews(string _newsID)
        {
            return _newsbll.DeleteEntity(_newsID);
        }
          [OperationContract]
        public T_PF_NEWS GetNewsModelByID(string _newsID)
        {
            return _newsbll.GetEntityByID(_newsID);
        }
          [OperationContract]
        public T_PF_NEWSView GetNewsDetailsByID(string _newsID)
        {
            return _newsbll.GetEntityViewByID(_newsID);
        }
          [OperationContract]
        public List<TM_SaaS_OA_EFModel.T_PF_NEWS> GetNewsList()
        {
            return _newsbll.GetNewsList();
        }
          [OperationContract]
        public Dictionary<T_PF_NEWS, List<T_PF_DISTRIBUTEUSER>> GetNewsLists()
        {
            return _newsbll.GetNewsLists();
        }
          [OperationContract]
        public List<TM_SaaS_OA_EFModel.T_PF_NEWS> GetNewsListByParams(string type, int topCount, string state)
        {
            return _newsbll.GetNewsList(type, topCount, state);
        }
          [OperationContract]
        public System.Collections.Generic.List<T_PF_NEWS> GetNewsListByState(string state)
        {
            return _newsbll.GetNewsListByState(state);
        }
          [OperationContract]
        public List<T_PF_NEWS> GetImageNewsList(int topCount, string state)
        {
            return _newsbll.GetImageNews(topCount, state);
        }
          [OperationContract]
        public List<T_PF_NEWSListView> GetNewsListByPage(int pageIndex, int pageSize, string sort, string filterString, ref int pageCount)
        {
            return _newsbll.GetNewsListByPage(pageIndex, pageSize, sort, filterString, ref pageCount);
        }

        #endregion

          [OperationContract]
        public System.Collections.Generic.List<T_PF_NEWSListView> GetPopupNewsList()
        {
            try
            {
                return _newsbll.GetPopupNewsList();
            }
            catch (Exception ex)
            {
                throw new Exception("添加新闻异常:" + ex.Message);
            }
        }
          [OperationContract]
        public List<T_PF_NEWSListView> GetNewsListByMobile(int pageIndex, int pageSize, string sort, string filterString, ref int pageCount, ref int rowCount)
        {
            return _newsbll.GetNewsListByMobile(pageIndex, pageSize, sort, filterString, ref pageCount, ref rowCount);
        }

    }
}
