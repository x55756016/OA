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

namespace SMT.SAAS.Platform.WebParts.ViewModels
{
    public class DataGridPageViewModel : BasicViewModel
    {
        public event EventHandler OnPageIndexChanged;
        private int _pageCount = 0;
        private int _pageIndex = 1;
        private int _pageSize = 14;

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
                    PageIndexChanged();
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
                    PageIndexChanged();
                }
            }
        }

        private void PageIndexChanged()
        {
            if (OnPageIndexChanged != null)
                OnPageIndexChanged(this, EventArgs.Empty);
         
        }
    }
}
