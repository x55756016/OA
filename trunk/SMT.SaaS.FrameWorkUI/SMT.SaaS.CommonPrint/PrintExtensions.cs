using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Printing;
using System.Windows.Data;

namespace SMT.SaaS.CommonPrint
{
    public static class PrintExtensions
    {
        public static DataGrid ToPrintFriendlyGrid(this DataGrid source,PagedCollectionView pcv)
        {
            DataGrid dg = new DataGrid();
            dg.ItemsSource = pcv;
            dg.AutoGenerateColumns = false;

            for (int i = 0; i < source.Columns.Count; i++)
            {
                DataGridTextColumn newColumn = new DataGridTextColumn();
                DataGridTextColumn column = (DataGridTextColumn)source.Columns[i];
                newColumn.Header = column.Header;
                System.Windows.Data.Binding bind;
                if (column.Binding != null)
                {
                    bind = new System.Windows.Data.Binding();
                    bind.Path = column.Binding.Path;
                    //bind.Converter = column.Binding.Converter;
                }
                else
                    bind = new System.Windows.Data.Binding();
                newColumn.Binding = bind;
                dg.Columns.Add(newColumn);
            }

            return dg;
        }

        public static void PrintForm(this UIElement source)
        {
            var doc = new PrintDocument();
            doc.PrintPage += (s, e) =>
            {
                e.PageVisual = source;
            };

            doc.Print(null);
        }

        public static void PrintGrid(this DataGrid source,PagedCollectionView pcv)
        {
            var dg = source.ToPrintFriendlyGrid(pcv);
            var doc = new PrintDocument();

            var offsetY = 0d;
            var totalHeight = 0d;
            var canvas = new Canvas();
            canvas.Children.Add(dg);
            doc.PrintPage += (s, e) =>
            {
                e.PageVisual = canvas;
                canvas.Margin = new Thickness(50);
                if (totalHeight == 0)
                {
                    totalHeight = dg.DesiredSize.Height;
                }

                Canvas.SetTop(dg, -offsetY);

                offsetY += e.PrintableArea.Height;

                e.HasMorePages = offsetY <= totalHeight;
            };


            doc.Print(null);
        }
    }
}
