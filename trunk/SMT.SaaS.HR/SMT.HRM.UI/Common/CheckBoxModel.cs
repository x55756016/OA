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

namespace SMT.HRM.UI
{
    public class CheckBoxModel
    {
        private int _id;
        private string _cbName;
        private bool _isChecked;

        public CheckBoxModel() { }

        public CheckBoxModel(int id, string strCBName, bool bIsChecked)
        {
            this._id = id;
            this._cbName = strCBName;
            this._isChecked = bIsChecked;
        }

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        public string CBNAME
        {
            get
            {
                return _cbName;
            }
            set
            {
                _cbName = value;
            }
        }
        public bool ISCHECKED  
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
            }
        }
    }
}
