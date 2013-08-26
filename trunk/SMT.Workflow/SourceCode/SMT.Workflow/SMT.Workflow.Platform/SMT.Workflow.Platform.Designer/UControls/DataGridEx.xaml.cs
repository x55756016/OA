/*---------------------------------------------------------------------  
	 * 版　权：Copyright ©  SmtOnline  2011    
	 * 文件名：文件注释说明.cs  
	 * 创建者：wanglin   
	 * 创建日期：2011/9/20 9:25:36   
	 * CLR版本： 4.0.30319.1  
	 * 命名空间：SMT.Workflow.Platform.Designer.Views.FlowDesign 
	 * 描　　述： 扩展DataGrid控件
	 * 模块名称：工作流设计器
* ---------------------------------------------------------------------*/

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

namespace SMT.Workflow.Platform.Designer.UControls
{
    /// <summary>
    /// DoubleClickDataGrid
    /// </summary>
    public partial class DataGridEx : DataGrid
    {
        /// <summary>
        /// 行单击事件
        /// </summary>
        public event RowClickedHandler RowClicked;

        /// <summary>
        /// 行双击事件
        /// </summary>
        public event RowDoubleClickedHandler RowDoubleClicked;

        /// <summary>
        /// 单击事件委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RowClickedHandler(object sender, DataGridRowClickedArgs e);

        /// <summary>
        /// 双击事件委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void RowDoubleClickedHandler (object sender, DataGridRowClickedArgs e);

        /// <summary>
        /// 最后单击的行
        /// </summary>
        private DataGridRow _LastDataGridRow = null;

        /// <summary>
        /// 最后单击的列
        /// </summary>
        private DataGridColumn _LastDataGridColumn = null;

        /// <summary>
        /// 最后单击的单元格
        /// </summary>
        private DataGridCell _LastDataGridCell = null;

        /// <summary>
        /// 最后单击的对象
        /// </summary>
        private object _LastObject = null;

        /// <summary>
        /// 最后单击的时间
        /// </summary>
        private DateTime _LastClick = DateTime.MinValue;

        /// <summary>
        /// 双击间隔时间，缺省0.3秒，不能太大
        /// </summary>
        private double _DoubleClickTime = 300;

        /// <summary>
        /// 双击事件间隔
        /// </summary>
        public double DoubleClickTime
        {
            get
            {
                return _DoubleClickTime;
            }
            set
            {
                _DoubleClickTime = value;
            }
        }

        /// <summary>
        /// 构造函数，注册鼠标左键事件
        /// </summary>
        public DataGridEx()
        {
            InitializeComponent();

            this.MouseLeftButtonUp += new MouseButtonEventHandler(DoubleClickDataGridClick);
        }

        /// <summary>
        /// 行单击
        /// </summary>
        protected void OnRowClicked()
        {
            if (RowClicked != null)
            {
                RowClicked(this, new DataGridRowClickedArgs
                            (_LastDataGridRow, _LastDataGridColumn, _LastDataGridCell, _LastObject));
            }
        }

        /// <summary>
        /// 行双击
        /// </summary>
        protected void OnRowDoubleClicked()
        {
            if (RowDoubleClicked != null)
            {
                RowDoubleClicked(this, new DataGridRowClickedArgs
                    (_LastDataGridRow, _LastDataGridColumn, _LastDataGridCell, _LastObject));
            }
        }

        /// <summary>
        /// DtaGrid双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleClickDataGridClick(object sender, MouseButtonEventArgs e)
        {
            DateTime clickTime = DateTime.Now;
            DataGridRow currentRowClicked;
            DataGridColumn currentColumnClicked;
            DataGridCell currentCellClicked;
            object currentObject;

            //找到最少的行
            if (GetDataGridCellByPosition(e.GetPosition(null), out currentRowClicked,
                out currentColumnClicked, out currentCellClicked, out currentObject))
            {
                //判断当前行就是选择的最新一行
                //在这个时间段内，确认为双击事件
                bool isDoubleClick = (currentRowClicked == _LastDataGridRow &&
                    clickTime.Subtract(_LastClick) <= TimeSpan.FromMilliseconds
                    (_DoubleClickTime));

                        _LastDataGridRow = currentRowClicked;
                        _LastDataGridColumn = currentColumnClicked;
                        _LastDataGridCell = currentCellClicked;
                        _LastObject = currentObject;

                        if (isDoubleClick)
                        {
                            OnRowDoubleClicked();
                        }
                        else
                        {
                            OnRowClicked();
                        }
            }
            else
            {
                _LastDataGridRow = null;
                _LastDataGridCell = null;
                _LastDataGridColumn = null;
                _LastObject = null;
            }

            _LastClick = clickTime;
        }

        /// <summary>
        /// 根据所在位置找到选择单元格
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="dataGridRow"></param>
        /// <param name="dataGridColumn"></param>
        /// <param name="dataGridCell"></param>
        /// <param name="dataGridObject"></param>
        /// <returns></returns>
        private bool GetDataGridCellByPosition(Point pt, out DataGridRow dataGridRow,
                out DataGridColumn dataGridColumn, out DataGridCell dataGridCell, out object dataGridObject)
        {
            var elements = VisualTreeHelper.FindElementsInHostCoordinates(pt, this);
            dataGridRow = null;
            dataGridCell = null;
            dataGridColumn = null;
            dataGridObject = null;

            if (null == elements || elements.Count() == 0)
            {
                return false;
            }

            var rowQuery = from gridRow in elements
                           where gridRow
                               is DataGridRow
                           select gridRow as DataGridRow;
            dataGridRow = rowQuery.FirstOrDefault();
            if (dataGridRow == null)
            {
                return false;
            }

            dataGridObject = dataGridRow.DataContext;

            var cellQuery = from gridCell in elements
                            where gridCell is DataGridCell
                            select gridCell as DataGridCell;
            dataGridCell = cellQuery.FirstOrDefault();

            if (dataGridCell != null)
            {
                dataGridColumn = DataGridColumn.GetColumnContainingElement(dataGridCell);
            }

            //如果得到当前的行，即返回；很多次 列，与内容会为空
            return dataGridRow != null;
        }
    }

    /// <summary>
    /// DataGrid行点击事件参数
    /// </summary>
    public class DataGridRowClickedArgs
    {
        public DataGridRow DataGridRow { get; set; }
        public DataGridColumn DataGridColumn { get; set; }
        public DataGridCell DataGridCell { get; set; }
        public object DataGridRowItem { get; set; }

        public DataGridRowClickedArgs(DataGridRow dataGridRow, 
            DataGridColumn dataGridColumn, DataGridCell dataGridCell, object dataGridRowItem)
        {
            DataGridRow = dataGridRow;
            DataGridColumn = dataGridColumn;
            DataGridCell = dataGridCell;
            DataGridRowItem = dataGridRowItem;
        }
    }
}
