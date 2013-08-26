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

namespace SMT.Workflow.Platform.Designer.DesignerControl
{
    public class ActivityProperty
    {
        public ActivityProperty() { }

        #region Model
        private int _rec_no;
        private int _tmp_no;
        private int _order;
        private string _name;
        private string _desc;
        private bool _can_processing;
        private bool _can_return;
        private bool _can_return_any;
        private bool _can_stop;
        private bool _can_change;
        private bool _can_collect;
        private bool _allow_repeat = true;
        private bool _allow_mail = true;
        private bool _allow_note;
        private bool _allow_track;
        private int _track_no;
        private int _approve_type = 1;
        private string _approver = string.Empty;
        private int _parallel_rule;

        /// <summary>
        /// 
        /// </summary>
        public int rec_no
        {
            set { _rec_no = value; }
            get { return _rec_no; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool can_change
        {
            set { _can_change = value; }
            get { return _can_change; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool can_collect
        {
            set { _can_collect = value; }
            get { return _can_collect; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool allow_repeat
        {
            set { _allow_repeat = value; }
            get { return _allow_repeat; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool allow_mail
        {
            set { _allow_mail = value; }
            get { return _allow_mail; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool allow_note
        {
            set { _allow_note = value; }
            get { return _allow_note; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool allow_track
        {
            set { _allow_track = value; }
            get { return _allow_track; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int track_no
        {
            set { _track_no = value; }
            get { return _track_no; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int approve_type
        {
            set { _approve_type = value; }
            get { return _approve_type; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int parallel_rule
        {
            set { _parallel_rule = value; }
            get { return _parallel_rule; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int tmp_no
        {
            set { _tmp_no = value; }
            get { return _tmp_no; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int order
        {
            set { _order = value; }
            get { return _order; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string name
        {
            set { _name = value; }
            get { return _name; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string approver
        {
            set { _approver = value; }
            get { return _approver; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string desc
        {
            set { _desc = value; }
            get { return _desc; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool can_processing
        {
            set { _can_processing = value; }
            get { return _can_processing; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool can_return
        {
            set { _can_return = value; }
            get { return _can_return; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool can_return_any
        {
            set { _can_return_any = value; }
            get { return _can_return_any; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool can_stop
        {
            set { _can_stop = value; }
            get { return _can_stop; }
        }
        #endregion Model
    }
}
