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
using System.ComponentModel;
using System.Collections.Generic;

namespace SMT.SAAS.Controls.Toolkit
{
    public class DragDockPanelHost : Canvas
    {
        private int columns = 1;
        private DragDockPanel draggingPanel;
        private int maxColumns;
        private DragDockPanel maximizedPanel;
        private int maxRows;
        private double minimizedColumnWidth = 250.0;
        private MinimizedPositions minimizedPosition = MinimizedPositions.Right;
        private double minimizedRowHeight = 75.0;
        private bool panelsInitialised;
        private int rows = 1;
        public DragDockPanelHost()
        {
            base.Loaded += new RoutedEventHandler(this.DragDockPanelHost_Loaded);
            base.SizeChanged += new SizeChangedEventHandler(this.DragDockPanelHost_SizeChanged);
            base.LayoutUpdated += new EventHandler(this.DragDockPanelHost_LayoutUpdated);
        }

        public void AddPanel(DragDockPanel panel)
        {
            Dictionary<int, DragDockPanel> orderedPanels = this.GetOrderedPanels();
            orderedPanels.Add(base.Children.Count, panel);
            base.Children.Add(panel);
            this.PreparePanel(panel);
            this.SetRowsAndColumns(orderedPanels);
            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        private void AnimatePanelLayout()
        {
            if ((!double.IsInfinity(base.ActualWidth) && !double.IsNaN(base.ActualWidth)) && (base.ActualWidth != 0.0))
            {
                if (this.maximizedPanel == null)
                {
                    foreach (UIElement element in base.Children)
                    {
                        DragDockPanel panel = (DragDockPanel)element;
                        if (panel != this.draggingPanel)
                        {
                            panel.AnimatePosition(Grid.GetColumn(panel) * (base.ActualWidth / ((double)this.columns)), Grid.GetRow(panel) * (base.ActualHeight / ((double)this.rows)));
                        }
                    }
                }
                else
                {
                    Dictionary<int, DragDockPanel> dictionary = new Dictionary<int, DragDockPanel>();
                    foreach (UIElement element2 in base.Children)
                    {
                        DragDockPanel panel2 = (DragDockPanel)element2;
                        dictionary.Add((Grid.GetRow(panel2) * this.columns) + Grid.GetColumn(panel2), panel2);
                    }
                    double num = 0.0;
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        if (dictionary[i] != this.maximizedPanel)
                        {
                            double x = 0.0;
                            double y = num;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                x = base.ActualWidth - this.minimizedColumnWidth;
                                y = num;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                            {
                                x = 0.0;
                                y = num;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                            {
                                x = num;
                                y = base.ActualHeight - this.minimizedRowHeight;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                x = num;
                                y = 0.0;
                            }
                            dictionary[i].AnimatePosition(x, y);
                            if (this.minimizedPosition.Equals(MinimizedPositions.Left) || this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                num += base.ActualHeight / ((double)(base.Children.Count - 1));
                            }
                            else
                            {
                                num += base.ActualWidth / ((double)(base.Children.Count - 1));
                            }
                        }
                        else
                        {
                            double minimizedColumnWidth = 0.0;
                            double minimizedRowHeight = 0.0;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                minimizedColumnWidth = 0.0;
                                minimizedRowHeight = 0.0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                            {
                                minimizedColumnWidth = this.minimizedColumnWidth;
                                minimizedRowHeight = 0.0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                            {
                                minimizedColumnWidth = 0.0;
                                minimizedRowHeight = 0.0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                minimizedColumnWidth = 0.0;
                                minimizedRowHeight = this.minimizedRowHeight;
                            }
                            dictionary[i].AnimatePosition(minimizedColumnWidth, minimizedRowHeight);
                        }
                    }
                }
            }
        }

        private void AnimatePanelSizes()
        {
            if ((!double.IsInfinity(base.ActualWidth) && !double.IsNaN(base.ActualWidth)) && (base.ActualWidth != 0.0))
            {
                if (this.maximizedPanel == null)
                {
                    foreach (UIElement element in base.Children)
                    {
                        DragDockPanel panel = (DragDockPanel)element;
                        double width = ((base.ActualWidth / ((double)this.columns)) - panel.Margin.Left) - panel.Margin.Right;
                        double height = ((base.ActualHeight / ((double)this.rows)) - panel.Margin.Top) - panel.Margin.Bottom;
                        if (width < 0.0)
                        {
                            width = 0.0;
                        }
                        if (height < 0.0)
                        {
                            height = 0.0;
                        }
                        panel.AnimateSize(width, height);
                    }
                }
                else
                {
                    foreach (UIElement element2 in base.Children)
                    {
                        DragDockPanel panel2 = (DragDockPanel)element2;
                        if (panel2 != this.maximizedPanel)
                        {
                            double num3 = (this.minimizedColumnWidth - panel2.Margin.Left) - panel2.Margin.Right;
                            double num4 = ((base.ActualHeight / ((double)(base.Children.Count - 1))) - panel2.Margin.Top) - panel2.Margin.Bottom;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                num3 = ((base.ActualWidth / ((double)(base.Children.Count - 1))) - panel2.Margin.Left) - panel2.Margin.Right;
                                num4 = (this.minimizedRowHeight - panel2.Margin.Top) - panel2.Margin.Bottom;
                            }
                            if (num4 < 0.0)
                            {
                                num4 = 0.0;
                            }
                            if (num3 < 0.0)
                            {
                                num3 = 0.0;
                            }
                            panel2.AnimateSize(num3, num4);
                            continue;
                        }
                        double num5 = ((base.ActualWidth - this.minimizedColumnWidth) - panel2.Margin.Left) - panel2.Margin.Right;
                        double num6 = (base.ActualHeight - panel2.Margin.Top) - panel2.Margin.Bottom;
                        if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                        {
                            num5 = (base.ActualWidth - panel2.Margin.Left) - panel2.Margin.Right;
                            num6 = ((base.ActualHeight - this.minimizedRowHeight) - panel2.Margin.Top) - panel2.Margin.Bottom;
                        }
                        if (num6 < 0.0)
                        {
                            num6 = 0.0;
                        }
                        if (num5 < 0.0)
                        {
                            num5 = 0.0;
                        }
                        panel2.AnimateSize(num5, num6);
                    }
                }
            }
        }

        private void DragDockPanel_DragFinished(object sender, DragEventArgs args)
        {
            this.draggingPanel = null;
            this.UpdatePanelLayout();
        }

        private void DragDockPanel_DragMoved(object sender, DragEventArgs args)
        {
            Point position = args.MouseEventArgs.GetPosition(this);
            int num = (int)Math.Floor((double)(position.Y / (base.ActualHeight / ((double)this.rows))));
            int num2 = (int)Math.Floor((double)(position.X / (base.ActualWidth / ((double)this.columns))));
            DragDockPanel panel = null;
            foreach (UIElement element in base.Children)
            {
                DragDockPanel panel2 = element as DragDockPanel;
                if (((panel2 != this.draggingPanel) && (Grid.GetColumn(panel2) == num2)) && (Grid.GetRow(panel2) == num))
                {
                    panel = panel2;
                    break;
                }
            }
            if (panel != null)
            {
                int column = Grid.GetColumn(panel);
                int row = Grid.GetRow(panel);
                Grid.SetColumn(panel, Grid.GetColumn(this.draggingPanel));
                Grid.SetRow(panel, Grid.GetRow(this.draggingPanel));
                Grid.SetColumn(this.draggingPanel, column);
                Grid.SetRow(this.draggingPanel, row);
                this.AnimatePanelLayout();
            }
        }

        private void DragDockPanel_DragStarted(object sender, DragEventArgs args)
        {
            DragDockPanel panel = sender as DragDockPanel;
            this.draggingPanel = panel;
        }

        private void DragDockPanel_Maximized(object sender, EventArgs e)
        {
            DragDockPanel panel = sender as DragDockPanel;
            this.maximizedPanel = panel;
            foreach (UIElement element in base.Children)
            {
                DragDockPanel panel2 = element as DragDockPanel;
                panel2.DraggingEnabled = false;
                if (panel2 != this.maximizedPanel)
                {
                    panel2.PanelState = PanelState.Minimized;
                }
            }
            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        private void DragDockPanel_Restored(object sender, EventArgs e)
        {
            this.maximizedPanel = null;
            foreach (UIElement element in base.Children)
            {
                DragDockPanel panel = element as DragDockPanel;
                panel.PanelState = PanelState.Restored;
                panel.DraggingEnabled = true;
            }
            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        private void DragDockPanelHost_LayoutUpdated(object sender, EventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();
                for (int i = 0; i < base.Children.Count; i++)
                {
                    if (base.Children[i].GetType() == typeof(DragDockPanel))
                    {
                        DragDockPanel panel = (DragDockPanel)base.Children[i];
                        orderedPanels.Add(i, panel);
                    }
                }
                this.SetRowsAndColumns(orderedPanels);
                this.UpdatePanelLayout();
            }
        }

        private void DragDockPanelHost_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();
            for (int i = 0; i < base.Children.Count; i++)
            {
                if (base.Children[i].GetType() == typeof(DragDockPanel))
                {
                    DragDockPanel panel = (DragDockPanel)base.Children[i];
                    this.PreparePanel(panel);
                    orderedPanels.Add(i, panel);
                }
            }
            this.SetRowsAndColumns(orderedPanels);
            this.panelsInitialised = true;
            this.UpdatePanelLayout();
        }

        private void DragDockPanelHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdatePanelLayout();
        }

        private Dictionary<int, DragDockPanel> GetOrderedPanels()
        {
            Dictionary<int, DragDockPanel> dictionary = new Dictionary<int, DragDockPanel>();
            foreach (UIElement element in base.Children)
            {
                DragDockPanel panel = (DragDockPanel)element;
                panel.PanelIndex = (Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel);
                dictionary.Add((Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel), panel);
            }
            return dictionary;
        }

        protected virtual void PreparePanel(DragDockPanel panel)
        {
            panel.DragStarted += new DragEventHander(this.DragDockPanel_DragStarted);
            panel.DragFinished += new DragEventHander(this.DragDockPanel_DragFinished);
            panel.DragMoved += new DragEventHander(this.DragDockPanel_DragMoved);
            panel.Maximized += new EventHandler(this.DragDockPanel_Maximized);
            panel.Restored += new EventHandler(this.DragDockPanel_Restored);
            if (panel.PanelState == PanelState.Maximized)
            {
                this.maximizedPanel = panel;
            }
        }

        public void RemovePanel(DragDockPanel panel)
        {
            Dictionary<int, DragDockPanel> dictionary = new Dictionary<int, DragDockPanel>();
            List<int> list = new List<int>();
            foreach (UIElement element in base.Children)
            {
                DragDockPanel panel2 = (DragDockPanel)element;
                dictionary.Add((Grid.GetRow(panel2) * this.columns) + Grid.GetColumn(panel2), panel2);
                list.Add((Grid.GetRow(panel2) * this.columns) + Grid.GetColumn(panel2));
            }
            dictionary.Remove((Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel));
            list.Remove((Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel));
            base.Children.Remove(panel);
            Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();
            for (int i = 0; i < list.Count; i++)
            {
                orderedPanels.Add(i, dictionary[list[i]]);
            }
            this.SetRowsAndColumns(orderedPanels);
            if ((this.maximizedPanel == panel) || (orderedPanels.Count == 1))
            {
                orderedPanels[0].PanelState = PanelState.Restored;
                this.maximizedPanel = null;
                foreach (UIElement element2 in base.Children)
                {
                    DragDockPanel panel3 = (DragDockPanel)element2;
                    panel3.DraggingEnabled = true;
                }
            }
            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        public void RemovePanelAt(int index)
        {
            if ((index >= 0) && (index < base.Children.Count))
            {
                Dictionary<int, DragDockPanel> dictionary = new Dictionary<int, DragDockPanel>();
                foreach (UIElement element in base.Children)
                {
                    DragDockPanel panel = (DragDockPanel)element;
                    dictionary.Add((Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel), panel);
                }
                this.RemovePanel(dictionary[index]);
            }
        }

        private void SetRowsAndColumns(Dictionary<int, DragDockPanel> orderedPanels)
        {
            if (orderedPanels.Count != 0)
            {
                this.rows = (int)Math.Floor(Math.Sqrt((double)base.Children.Count));
                if (this.maxRows > 0)
                {
                    if (this.rows > this.maxRows)
                    {
                        this.rows = this.maxRows;
                    }
                    this.columns = (int)Math.Ceiling((double)(((double)base.Children.Count) / ((double)this.rows)));
                }
                else if (this.maxColumns > 0)
                {
                    this.columns = (int)Math.Ceiling((double)(((double)base.Children.Count) / ((double)this.rows)));
                    if (this.columns > this.maxColumns)
                    {
                        this.columns = this.maxColumns;
                        this.rows = (int)Math.Ceiling((double)(((double)base.Children.Count) / ((double)this.columns)));
                    }
                }
                else
                {
                    this.columns = (int)Math.Ceiling((double)(((double)base.Children.Count) / ((double)this.rows)));
                }
                int num = 0;
                for (int i = 0; i < this.rows; i++)
                {
                    for (int j = 0; j < this.columns; j++)
                    {
                        Grid.SetRow(orderedPanels[num], i);
                        Grid.SetColumn(orderedPanels[num], j);
                        num++;
                        if (num == base.Children.Count)
                        {
                            break;
                        }
                    }
                    if (num == base.Children.Count)
                    {
                        return;
                    }
                }
            }
        }

        private void UpdatePanelLayout()
        {
            if ((!double.IsInfinity(base.ActualWidth) && !double.IsNaN(base.ActualWidth)) && (base.ActualWidth != 0.0))
            {
                if (this.maximizedPanel == null)
                {
                    foreach (UIElement element in base.Children)
                    {
                        DragDockPanel panel = (DragDockPanel)element;
                        Canvas.SetLeft(panel, Grid.GetColumn(panel) * (base.ActualWidth / ((double)this.columns)));
                        Canvas.SetTop(panel, Grid.GetRow(panel) * (base.ActualHeight / ((double)this.rows)));
                        double num = ((base.ActualWidth / ((double)this.columns)) - panel.Margin.Left) - panel.Margin.Right;
                        double num2 = ((base.ActualHeight / ((double)this.rows)) - panel.Margin.Top) - panel.Margin.Bottom;
                        if (num < 0.0)
                        {
                            num = 0.0;
                        }
                        if (num2 < 0.0)
                        {
                            num2 = 0.0;
                        }
                        panel.Width = num;
                        panel.Height = num2;
                    }
                }
                else
                {
                    Dictionary<int, DragDockPanel> dictionary = new Dictionary<int, DragDockPanel>();
                    foreach (UIElement element2 in base.Children)
                    {
                        DragDockPanel panel2 = (DragDockPanel)element2;
                        dictionary.Add((Grid.GetRow(panel2) * this.columns) + Grid.GetColumn(panel2), panel2);
                    }
                    double num3 = 0.0;
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        if (dictionary[i] != this.maximizedPanel)
                        {
                            double num5 = (this.minimizedColumnWidth - dictionary[i].Margin.Left) - dictionary[i].Margin.Right;
                            double num6 = ((base.ActualHeight / ((double)(base.Children.Count - 1))) - dictionary[i].Margin.Top) - dictionary[i].Margin.Bottom;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                num5 = ((base.ActualWidth / ((double)(base.Children.Count - 1))) - dictionary[i].Margin.Left) - dictionary[i].Margin.Right;
                                num6 = (this.minimizedRowHeight - dictionary[i].Margin.Top) - dictionary[i].Margin.Bottom;
                            }
                            if (num6 < 0.0)
                            {
                                num6 = 0.0;
                            }
                            if (num5 < 0.0)
                            {
                                num5 = 0.0;
                            }
                            dictionary[i].Width = num5;
                            dictionary[i].Height = num6;
                            double length = 0.0;
                            double num8 = num3;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                length = base.ActualWidth - this.minimizedColumnWidth;
                                num8 = num3;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                            {
                                length = 0.0;
                                num8 = num3;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                            {
                                length = num3;
                                num8 = base.ActualHeight - this.minimizedRowHeight;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                length = num3;
                                num8 = 0.0;
                            }
                            Canvas.SetLeft(dictionary[i], length);
                            Canvas.SetTop(dictionary[i], num8);
                            if (this.minimizedPosition.Equals(MinimizedPositions.Left) || this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                num3 += base.ActualHeight / ((double)(base.Children.Count - 1));
                            }
                            else
                            {
                                num3 += base.ActualWidth / ((double)(base.Children.Count - 1));
                            }
                        }
                        else
                        {
                            double num9 = ((base.ActualWidth - this.minimizedColumnWidth) - dictionary[i].Margin.Left) - dictionary[i].Margin.Right;
                            double num10 = (base.ActualHeight - dictionary[i].Margin.Top) - dictionary[i].Margin.Bottom;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                num9 = (base.ActualWidth - dictionary[i].Margin.Left) - dictionary[i].Margin.Right;
                                num10 = ((base.ActualHeight - this.minimizedRowHeight) - dictionary[i].Margin.Top) - dictionary[i].Margin.Bottom;
                            }
                            if (num10 < 0.0)
                            {
                                num10 = 0.0;
                            }
                            if (num9 < 0.0)
                            {
                                num9 = 0.0;
                            }
                            dictionary[i].Width = num9;
                            dictionary[i].Height = num10;
                            double minimizedColumnWidth = 0.0;
                            double minimizedRowHeight = 0.0;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                minimizedColumnWidth = 0.0;
                                minimizedRowHeight = 0.0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                            {
                                minimizedColumnWidth = this.minimizedColumnWidth;
                                minimizedRowHeight = 0.0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                            {
                                minimizedColumnWidth = 0.0;
                                minimizedRowHeight = 0.0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                minimizedColumnWidth = 0.0;
                                minimizedRowHeight = this.minimizedRowHeight;
                            }
                            Canvas.SetLeft(dictionary[i], minimizedColumnWidth);
                            Canvas.SetTop(dictionary[i], minimizedRowHeight);
                        }
                    }
                }
            }
        }

        [Category("布局属性"), Description("设置日期允许的最大列数. 最大行数的优先级在最大列数之前.")]
        public int MaxColumns
        {
            get
            {
                return this.maxColumns;
            }
            set
            {
                this.maxColumns = value;
                if (this.panelsInitialised)
                {
                    this.SetRowsAndColumns(this.GetOrderedPanels());
                    this.AnimatePanelSizes();
                    this.AnimatePanelLayout();
                }
            }
        }

        [Category("布局属性"), Description("设置容器最大行数. 最大行数的优先级在最大列数之前.")]
        public int MaxRows
        {
            get
            {
                return this.maxRows;
            }
            set
            {
                this.maxRows = value;
                if (this.panelsInitialised)
                {
                    this.SetRowsAndColumns(this.GetOrderedPanels());
                    this.AnimatePanelSizes();
                    this.AnimatePanelLayout();
                }
            }
        }

        [Description("设置列的最小宽度."), Category("布局属性")]
        public double MinimizedColumnWidth
        {
            get
            {
                return this.minimizedColumnWidth;
            }
            set
            {
                this.minimizedColumnWidth = value;
                this.UpdatePanelLayout();
            }
        }

        [Category("布局属性"), Description("设置面板的最小位置.")]
        public MinimizedPositions MinimizedPosition
        {
            get
            {
                return this.minimizedPosition;
            }
            set
            {
                this.minimizedPosition = value;
                this.AnimatePanelSizes();
                this.AnimatePanelLayout();
            }
        }

        [Description("设置行的最小高度."), Category("布局属性")]
        public double MinimizedRowHeight
        {
            get
            {
                return this.minimizedRowHeight;
            }
            set
            {
                this.minimizedRowHeight = value;
                this.UpdatePanelLayout();
            }
        }
    }
}
