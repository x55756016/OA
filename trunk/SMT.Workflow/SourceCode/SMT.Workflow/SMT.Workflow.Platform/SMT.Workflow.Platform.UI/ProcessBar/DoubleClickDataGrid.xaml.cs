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

namespace SMT.Workflow.Platform.UI.ProcessBar
{
    /// <summary>
    /// DoubleClickDataGrid
    /// </summary>
    public partial class DoubleClickDataGrid : DataGrid
    {
        public event RowClickedHandler RowClicked;
        public event RowDoubleClickedHandler RowDoubleClicked;

        public delegate void RowClickedHandler(object sender, DataGridRowClickedArgs e);
        public delegate void RowDoubleClickedHandler (object sender, DataGridRowClickedArgs e);

        private DataGridRow _LastDataGridRow = null;
        private DataGridColumn _LastDataGridColumn = null;
        private DataGridCell _LastDataGridCell = null;
        private object _LastObject = null;
        private DateTime _LastClick = DateTime.MinValue;

        private double _DoubleClickTime = 1500;
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

        public DoubleClickDataGrid()
        {
            InitializeComponent();

            this.MouseLeftButtonUp += new MouseButtonEventHandler
                    (DoubleClickDataGridClick);
        }

        protected void OnRowClicked()
        {
            if (RowClicked != null)
            {
                RowClicked(this, new DataGridRowClickedArgs
          (_LastDataGridRow, _LastDataGridColumn, _LastDataGridCell, _LastObject));
            }
        }

        protected void OnRowDoubleClicked()
        {
            if (RowDoubleClicked != null)
            {
                RowDoubleClicked(this, new DataGridRowClickedArgs
          (_LastDataGridRow, _LastDataGridColumn, _LastDataGridCell, _LastObject));
            }
        }

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

        #region 根据所在位置找到选择单元格
        private bool GetDataGridCellByPosition(Point pt, out DataGridRow dataGridRow,
    out DataGridColumn dataGridColumn, out DataGridCell dataGridCell,
        out object dataGridObject)
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
        #endregion
    }

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
