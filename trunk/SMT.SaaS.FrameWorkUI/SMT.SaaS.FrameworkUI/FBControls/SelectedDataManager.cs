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
using SMT.Saas.Tools.FBServiceWS;

namespace SMT.SaaS.FrameworkUI.FBControls
{
    public class SelectedDataManager
    {
        public SelectedDataManager()
        {
            GetSameItem = (list, entity) =>
                {
                    if (list == null)
                    {
                        return null;
                    }
                    return list.FirstOrDefault(item => item == entity);
                };
        }
        #region 属性
        public List<FBEntity> SelectedItems { get; set; }

        public List<FBEntity> OriginalItems { get; set; }

        public Func<List<FBEntity>, FBEntity, FBEntity> GetSameItem;

        private QueryExpression queryExpression;
        public QueryExpression QueryExpression
        {
            get
            {
                return queryExpression;
            }
            set
            {
                if (!object.Equals(queryExpression, value))
                {
                    queryExpression = value;
                    this.OriginalItems = null;
                }
            }
        }

        #endregion

        private FBServiceClient fbService;
        private FBServiceClient FBService
        {
            get
            {
                if (fbService == null)
                {
                    fbService = new FBServiceClient();
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
            FBService.QueryFBEntitiesAsync(this.QueryExpression);
        }

        private void SetActivedItems()
        {
            this.OriginalItems.ForEach(item =>
                {

                    FBEntity entitySelected = GetSameItem(SelectedItems, item);
                    if (entitySelected != null)
                    {
                        this.OriginalItems.Remove(item);
                        this.OriginalItems.Add(entitySelected);
                    }
                });
        }

        private void OnGetUnSelectedItems()
        {
            if (GetUnSelectedItemsCompleted != null)
            {
                List<FBEntity> listInActive = this.OriginalItems.ToList();
                ActionCompletedEventArgs<List<FBEntity>> args = new ActionCompletedEventArgs<List<FBEntity>>(listInActive);
                GetUnSelectedItemsCompleted(this, args);
            }
        }

        public void GetUnSelectedItems()
        {
            GetOriginalItems();

        }

        public event EventHandler<ActionCompletedEventArgs<List<FBEntity>>> GetUnSelectedItemsCompleted;
    }
}
