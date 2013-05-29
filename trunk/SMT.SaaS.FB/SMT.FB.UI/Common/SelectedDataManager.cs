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
using System.Collections.Generic;
using System.Linq;
using SMT.FB.UI.FBCommonWS;

namespace SMT.FB.UI.Common
{
    public class SelectedDataManager
    {
        public SelectedDataManager()
        {
            
        }
        #region 属性
        public List<FBEntity> SelectedItems { get; set; }

        public List<FBEntity> OriginalItems { get; set; }

        private Func<List<FBEntity>, FBEntity, FBEntity> _GetSameItem = null;
        public Func<List<FBEntity>, FBEntity, FBEntity> GetSameItem
        {
            set
            {
               _GetSameItem = value;
            }
            get
            {
                if ( _GetSameItem == null)
                {
                    _GetSameItem = (list, entity) =>
                    {
                        if (list == null)
                        {
                            return null;
                        }
                        return list.FirstOrDefault(item => item == entity);
                    };
                }
                return _GetSameItem;
            }
        }

        public QueryExpression QueryExpression { get; set; }

        #endregion

        private FBEntityService fbService;
        private FBEntityService FBService
        {
            get
            {
                if (fbService == null)
                {
                    fbService = new FBEntityService();
                    fbService.QueryFBEntitiesCompleted += new EventHandler<QueryFBEntitiesCompletedEventArgs>(fbService_QueryFBEntitiesCompleted);
                }
                return fbService;
            }
        }

        private void fbService_QueryFBEntitiesCompleted(object sender, QueryFBEntitiesCompletedEventArgs e)
        {
            OriginalItems = e.Result.ToList();
            OnGetUnSelectedItems();
        }
        
        private void GetOriginalItems()
        {
            FBService.QueryFBEntities(this.QueryExpression);
        }

       

        private void OnGetUnSelectedItems()
        {
            if (GetUnSelectedItemsCompleted != null)
            {
                List<FBEntity> listInActive = this.OriginalItems.Where(item =>
                {
                    FBEntity sameEntity = GetSameItem(this.SelectedItems, item);
                    return sameEntity == null;
                }).ToList();

                ActionCompletedEventArgs<List<FBEntity>> args = new ActionCompletedEventArgs<List<FBEntity>>(listInActive);
                GetUnSelectedItemsCompleted(this, args);
            }
        }

        public void GetUnSelectedItems()
        {
            //if (this.OriginalItems == null)
            //{
            //    GetOriginalItems();
            //}
            //else
            //{
            //    OnGetUnSelectedItems();
            //}
            GetOriginalItems();
        }

        public event EventHandler<ActionCompletedEventArgs<List<FBEntity>>> GetUnSelectedItemsCompleted;
    }
}
