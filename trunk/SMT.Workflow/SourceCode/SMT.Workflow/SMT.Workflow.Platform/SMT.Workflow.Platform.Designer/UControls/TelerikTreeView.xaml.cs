using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SMT.Workflow.Platform.Designer.Views.FlowDesign;
using Telerik.Windows.Controls;

namespace SMT.Workflow.Platform.Designer.UControls
{
    public partial class TelerikTreeView : UserControl
    {
        public static ObservableCollection<FlowDesigns> _itemFlow = new ObservableCollection<FlowDesigns>();

        public TelerikTreeView()
        {
            InitializeComponent();
            this.treeView.ItemsSource = new TreeDataSource();
            this.treeView.PreviewSelected += new EventHandler<Telerik.Windows.RadRoutedEventArgs>(treeView_PreviewSelected);

        }

        void treeView_PreviewSelected(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var item = treeView.SelectedItem as FloWapiece;
            if (item != null)
            {

                //ParentIDs = item.ID;
                //Pname = item.Name;
                //if (item.ParentID == 1)
                //{//一级节点（顶点）
                //    hortal.Visibility = System.Windows.Visibility.Collapsed;
                //    Tflow.Visibility = System.Windows.Visibility.Collapsed;
                //    TFlowlist.Visibility = System.Windows.Visibility.Visible;
                //    TFlowlist.AddFlow.Visibility = System.Windows.Visibility.Collapsed;
                //    TFlowlist.LoadData(_itemFlow);
                //}
                //else if (item.ParentID == 2)
                //{//二级节点

                //    hortal.Visibility = System.Windows.Visibility.Collapsed;
                //    Tflow.Visibility = System.Windows.Visibility.Collapsed;
                //    TFlowlist.Visibility = System.Windows.Visibility.Visible;
                //    TFlowlist.AddFlow.Visibility = System.Windows.Visibility.Visible;
                //    TFlowlist.OnSubmitComplated += new EventHandler(form_OnSubmitComplated);
                //    ObservableCollection<FlowDesigns> _itemFlows = new ObservableCollection<FlowDesigns>();
                //    var items = _itemFlow.Where(p => p.ParentID == item.ID);
                //    foreach (var flow in items)
                //    {
                //        _itemFlows.Add(flow);
                //    }
                //    TFlowlist.LoadData(_itemFlows);
                //}
                //else
                //{//三级节点
                //    hortal.Visibility = System.Windows.Visibility.Visible;
                //    Tflow.Visibility = System.Windows.Visibility.Visible;
                //    TFlowlist.Visibility = System.Windows.Visibility.Collapsed;
                //    ToXml(item.Name);
                //}
            }
            //RadTreeViewItem item = e.OriginalSource as RadTreeViewItem;
        }

       


    }
    //一级菜单
    public class FlowAll : INotifyPropertyChanged
    {
        private static int uid = 0;
        public string _ID; //ID
        public string _Name;//名称
        public int _ParentID; // 菜单级别
        public string _PARENTID;//上级菜单ID	
        public string _PARENTNAME; //上级菜单名称
        public List<ItemAll> batteryList { get; set; }
        public FlowAll(string ID, string name, int parentID, string pARENTID, string pARENTNAME)
        {
            ID = (++uid).ToString();
            Name = name;
            ParentID = parentID;
            PARENTID = pARENTID;
            PARENTNAME = pARENTNAME;
            batteryList = new List<ItemAll>();
        }
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (value != this._Name)
                {
                    this._Name = value;
                    this.NotifyPropertyChanged("Name");
                }
            }
        }
        public string ID
        {
            get { return _ID; }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    this.NotifyPropertyChanged("ID");
                }
            }
        }
        public int ParentID
        {
            get { return this._ParentID; }
            set
            {
                if (value != this._ParentID)
                {
                    this._ParentID = value;
                    this.NotifyPropertyChanged("ParentID");
                }
            }
        }
        public string PARENTID
        {
            get { return this._PARENTID; }
            set
            {
                if (value != this._PARENTID)
                {
                    this._PARENTID = value;
                    this.NotifyPropertyChanged("PARENTID");
                }
            }
        }
        public string PARENTNAME
        {
            get { return this._PARENTNAME; }
            set
            {
                if (value != this._PARENTNAME)
                {
                    this._PARENTNAME = value;
                    this.NotifyPropertyChanged("PARENTNAME");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }
    //二级菜单
    public class ItemAll : INotifyPropertyChanged
    {
        private static int uid = 0;
        public string _ID; //ID
        public string _Name;//名称
        public int _ParentID; // 菜单级别
        public string _PARENTID;//上级菜单ID	
        public string _PARENTNAME; //上级菜单名称

        public ItemAll(string ID, string name, int parentID, string pARENTID, string pARENTNAME)
        {
            ID = (++uid).ToString();
            Name = name;
            ParentID = parentID;
            PARENTID = pARENTID;
            PARENTNAME = pARENTNAME;
            wellList = new List<FloWapiece>();
        }
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (value != this._Name)
                {
                    this._Name = value;
                    this.NotifyPropertyChanged("Name");
                }
            }
        }
        public string ID
        {
            get { return _ID; }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    this.NotifyPropertyChanged("ID");
                }
            }
        }
        public int ParentID
        {
            get { return this._ParentID; }
            set
            {
                if (value != this._ParentID)
                {
                    this._ParentID = value;
                    this.NotifyPropertyChanged("ParentID");
                }
            }
        }
        public string PARENTID
        {
            get { return this._PARENTID; }
            set
            {
                if (value != this._PARENTID)
                {
                    this._PARENTID = value;
                    this.NotifyPropertyChanged("PARENTID");
                }
            }
        }
        public string PARENTNAME
        {
            get { return this._PARENTNAME; }
            set
            {
                if (value != this._PARENTNAME)
                {
                    this._PARENTNAME = value;
                    this.NotifyPropertyChanged("PARENTNAME");
                }
            }
        }
        public List<FloWapiece> wellList { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion


    }
    //三级菜单
    public class FloWapiece : INotifyPropertyChanged
    {
        private static int uid = 0;
        public string _ID; //ID
        public string _Name;//名称
        public int _ParentID; // 菜单级别
        public string _PARENTID;//上级菜单ID	
        public string _PARENTNAME; //上级菜单名称
        public FloWapiece(string ID, string name, int parentID, string pARENTID, string pARENTNAME)
        {
            ID = (++uid).ToString();
            Name = name;
            ParentID = parentID;
            PARENTID = pARENTID;
            PARENTNAME = pARENTNAME;

        }
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (value != this._Name)
                {
                    this._Name = value;
                    this.NotifyPropertyChanged("Name");
                }
            }
        }
        public string ID
        {
            get { return _ID; }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    this.NotifyPropertyChanged("ID");
                }
            }
        }
        public int ParentID
        {
            get { return this._ParentID; }
            set
            {
                if (value != this._ParentID)
                {
                    this._ParentID = value;
                    this.NotifyPropertyChanged("ParentID");
                }
            }
        }
        public string PARENTID
        {
            get { return this._PARENTID; }
            set
            {
                if (value != this._PARENTID)
                {
                    this._PARENTID = value;
                    this.NotifyPropertyChanged("PARENTID");
                }
            }
        }
        public string PARENTNAME
        {
            get { return this._PARENTNAME; }
            set
            {
                if (value != this._PARENTNAME)
                {
                    this._PARENTNAME = value;
                    this.NotifyPropertyChanged("PARENTNAME");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


    }
    public class TreeDataSource : List<FlowAll>
    {
        //public ObservableCollection<Tree> _itemTree = new ObservableCollection<Tree>();
        //public TreeDataSource()
        //{
        //    DateGrid();
        //    FlowAll treeFlow;
        //    ItemAll item;
        //    var itemFlow = _itemTree.Where(p => p.ParentID == 1);
        //    if (itemFlow.Count() > 0)
        //    {
        //        foreach (var items in itemFlow)
        //        {
        //            Add(treeFlow = new FlowAll(items.ID, items.Name, items.ParentID, items.PARENTID, items.PARENTNAME));
        //            var menu = _itemTree.Where(p => p.ParentID == 2 && p.PARENTID == items.ID);
        //            foreach (var menuitem in menu)
        //            {
        //                treeFlow.batteryList.Add((item = new ItemAll(menuitem.ID, menuitem.Name, menuitem.ParentID, menuitem.PARENTID, menuitem.PARENTNAME)));
        //                var itemWapiece = _itemTree.Where(p => p.ParentID == 3 && p.PARENTID == menuitem.ID);
        //                foreach (var itemess in itemWapiece)
        //                {
        //                    item.wellList.Add(new FloWapiece(itemess.ID, itemess.Name, itemess.ParentID, itemess.PARENTID, itemess.PARENTNAME));
        //                }
        //            }
        //        }
        //     }
        //}
        //public void DateGrid()
        //{
        //    _itemTree.Add(new Tree()
        //    {
        //        ID = "1", //ID
        //        Name = "审核流程",  //名称
        //        ParentID = 1, // 菜单级别
        //        PARENTID = "0",//上级菜单ID	
        //        PARENTNAME = "没有" //上级菜单名称
        //    });
        //    _itemTree.Add(new Tree()
        //    {
        //        ID = "2-1", //ID
        //        Name = "进销存流程",  //名称
        //        ParentID = 2, // 菜单级别
        //        PARENTID = "1",//上级菜单ID	
        //        PARENTNAME = "审核流程" //上级菜单名称
        //    });
        //    _itemTree.Add(new Tree()
        //    {
        //        ID = "2-1-1", //ID
        //        Name = "销售订单",  //名称
        //        ParentID = 3, // 菜单级别s
        //        PARENTID = "2-1",//上级菜单ID	
        //        PARENTNAME = "进销存" //上级菜单名称
        //    });
        //    _itemTree.Add(new Tree()
        //    {
        //        ID = "2-2", //ID
        //        Name = "培训流程",  //名称
        //        ParentID = 2, // 菜单级别
        //        PARENTID = "1",//上级菜单ID	
        //        PARENTNAME = "审核流程" //上级菜单名称
        //    });
        //    _itemTree.Add(new Tree()
        //    {
        //        ID = "2-2-1", //ID
        //        Name = "培训报名",  //名称
        //        ParentID = 3, // 菜单级别
        //        PARENTID = "2-2",//上级菜单ID	
        //        PARENTNAME = "培训流程" //上级菜单名称
        //    });

        //}
    }
}
