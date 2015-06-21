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

namespace SMT.SAAS.Controls.Toolkit.InfoPanel
{
    public partial class ItemAdvanceControl : UserControl
    {
        public event EventHandler<SelectedIndexChangedArgs> OnSelectedIndexChanged;
        public event EventHandler<SelectedIndexChangedArgs> OnItemClicked;
        // Methods
        public ItemAdvanceControl()
        {
            this.InitializeComponent();
            this.LeftArrowActive.MouseEnter+=new MouseEventHandler(LeftArrowActive_MouseEnter);
            this.LeftArrowActive.MouseLeave+=new MouseEventHandler(LeftArrowActive_MouseLeave);
            this.LeftArrow.MouseLeftButtonDown += new MouseButtonEventHandler(LeftArrowActive_MouseLeftButtonDown);
            this.RightArrowActive.MouseEnter+=new MouseEventHandler(RightArrowActive_MouseEnter);
            this.RightArrowActive.MouseLeave+=new MouseEventHandler(RightArrowActive_MouseLeave);
            this.RightArrow.MouseLeftButtonDown += new MouseButtonEventHandler(RightArrowActive_MouseLeftButtonDown);
        }
        private void HighlightSelectedItems(int previous, int targetIndex)
        {
            if (this.ActiveItemList.SelectedIndex > -1)
            {

                ((InfoIndexItem)this.ActiveItemList.Items[previous]).DeSelectItem();
            }
            this.ActiveItemList.SelectedIndex=targetIndex;
            ((InfoIndexItem)this.ActiveItemList.Items[this.ActiveItemList.SelectedIndex]).SelectItem();
            if (targetIndex == 0)
            {
                this.LeftArrow.Visibility = Visibility.Collapsed;
                this.RightArrow.Visibility = Visibility.Visible;
            }
            else if (targetIndex == (this.ActiveItemList.Items.Count- 1))
            {
                this.LeftArrow.Visibility = Visibility.Visible;
                this.RightArrow.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.LeftArrow.Visibility = Visibility.Visible;
                this.RightArrow.Visibility = Visibility.Visible;
            }
        }


        private void LeftArrowActive_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOverLeft", true);
        }

        private void LeftArrowActive_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseExitLeft", true);
        }

        private void LeftArrowActive_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int targetIndex = ((this.ActiveItemList.SelectedIndex - 1) >= 0) ? (this.ActiveItemList.SelectedIndex - 1) : 0;
            this.OnSelectedIndexChanged(this, new SelectedIndexChangedArgs { PreviousIndex = this.ActiveItemList.SelectedIndex, NewIndex = targetIndex });
            this.HighlightSelectedItems(this.ActiveItemList.SelectedIndex, targetIndex);
        }

        private void NavItemClick(object sender, RoutedEventArgs e)
        {
            InfoIndexItem item = sender as InfoIndexItem;
            this.OnSelectedIndexChanged(this, new SelectedIndexChangedArgs { PreviousIndex = this.ActiveItemList.SelectedIndex, NewIndex = item.Id });
            this.HighlightSelectedItems(this.ActiveItemList.SelectedIndex, item.Id);
        }

        private void RightArrowActive_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOverRight", true);
        }

        private void RightArrowActive_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseExitRight", true);
        }

        private void RightArrowActive_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int targetIndex = ((this.ActiveItemList.SelectedIndex + 1) <= (this.ActiveItemList.Items.Count - 1)) ? (this.ActiveItemList.SelectedIndex + 1) : (this.ActiveItemList.Items.Count - 1);
            this.OnSelectedIndexChanged(this, new SelectedIndexChangedArgs { PreviousIndex = this.ActiveItemList.SelectedIndex, NewIndex = targetIndex });
            this.HighlightSelectedItems(this.ActiveItemList.SelectedIndex, targetIndex);
        }

        private void SetupNavItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                InfoIndexItem item = new InfoIndexItem
                {
                    Id = i
                };
                item.Click = (RoutedEventHandler)Delegate.Combine(item.Click, new RoutedEventHandler(NavItemClick));
               this.ActiveItemList.Items.Add(item);
               
               // Button btn = new Button() { Content = i.ToString(), Height = 10, Width = 10 };
              // this.ActiveItemList.Items.Add(btn);
            }
            VisualStateManager.GoToState(this, "Reveal", false);
        }

       
        public int Index
        {
            get
            {
                return this.ActiveItemList.SelectedIndex;
            }
            set
            {
                this.HighlightSelectedItems(this.ActiveItemList.SelectedIndex, value);
            }
        }

        public int TotalItems
        {
            get
            {
                return this.ActiveItemList.Items.Count;
            }
            set
            {
                this.SetupNavItems(value);
            }
        }

    }
    public class SelectedIndexChangedArgs : EventArgs
    {
        // Methods
        public SelectedIndexChangedArgs()
        { }

        // Properties
        public int NewIndex { get; set; }
        public int PreviousIndex { get; set; }
    }
}
