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
using System.Xml.Linq;
using System.Xml;
using System.Windows.Markup;
using System.Collections.Generic;

namespace SMT.FB.UI.Common
{
    public class DataTempaleHelper
    {
        public static XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        public static XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        public static XNamespace data = "clr-namespace:System.Windows.Controls;assembly=System.Windows.Controls.Data";
        public static XElement GetEmptyDataTemplate()
        {
            XElement xDataTemplate =
            new XElement(ns + "DataTemplate", new XAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));
            return xDataTemplate;
        }

        public static DataTemplate GetDataGridColumnDataTemplate()
        {
            XElement cbbElement = new XElement(ns + "ComboBox");
            cbbElement.SetAttributeValue(x + "Name", "cbb1");

            XElement xStackPanel = new XElement(ns + "StackPanel");

            xStackPanel.Add(cbbElement);
            XElement xRoot = GetEmptyDataTemplate();
            xRoot.Add(xStackPanel);
            return GetDataTemplate(xRoot);


        }

        public static DataTemplate GetDataGridColumnDataTemplate(XElement xElement)
        {

            XElement xStackPanel = new XElement(ns + "StackPanel");

            xStackPanel.Add(xElement);
            XElement xRoot = GetEmptyDataTemplate();
            xRoot.Add(xStackPanel);
            return GetDataTemplate(xRoot);


        }

        public static DataTemplate GetDataTemplate(XElement xDataTemplate)
        {
            XElement xRoot = GetEmptyDataTemplate();
            xRoot.Add(xDataTemplate);
            XmlReader xr = xRoot.CreateReader();
            DataTemplate dataTemplate = XamlReader.Load(xRoot.ToString()) as DataTemplate;
            return dataTemplate;
        }

        public static XElement GetDataGridElement(List<GridItem> items)
        {
            XElement gridXCs = new XElement(data + "DataGrid.Columns");

            items.ForEach ( item =>
                {
                    XElement gridXColumn = new XElement(data + "DataGridTextColumn",
                                         new XAttribute("Header", item.PropertyDisplayName),
                                         new XAttribute("Width", item.Width.ToString())
                                         );

                    //XElement gridXColumn = new XElement(data + "DataGridTextColumn", 
                    //    new XAttribute("Header", item.PropertyDisplayName),
                    //    new XAttribute("Width", item.Width.ToString()),
                    //    new XAttribute("Binding", "{Binding Mode=TwoWay, Path=" + item.PropertyName +"}")
                    //    );
                    gridXCs.Add(gridXColumn);
                });
            //  Mode=TwoWay, Path={RelationCollection[0].FBEntities
            XElement gridX = new XElement(data + "DataGrid");
//            XElement gridX = new XElement(data + "DataGrid", new XAttribute("ItemsSource", "{Binding}"));
            gridX.Add(gridXCs);
            return gridX;
        }

        public static DataTemplate GetDataGrid(List<GridItem> items)
        {
            XElement gridX = GetDataGridElement(items);
            return GetDataTemplate(gridX);
        }
    
        private DataTemplate GetDT(double textboxWidh)
        {
            //内存中动态生成一个XAML，描述了一个DataTemplate
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XElement xDataTemplate =
            new XElement(ns + "DataTemplate", new XAttribute("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"),
            new XElement(ns + "Grid", new XAttribute("Background", "White"),
                new XElement(ns + "Grid.ColumnDefinitions",
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "*")),
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "75")),
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", "25")),
                    new XElement(ns + "ColumnDefinition", new XAttribute("Width", textboxWidh.ToString()))
                    ),
                new XElement(ns + "Grid.RowDefinitions",
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition"),
                    new XElement(ns + "RowDefinition")
                    ),
                new XElement(ns + "TextBlock", new XAttribute("Text", "机票费"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "0")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "车船费"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "1")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "住宿费"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "2")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "出差补助"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "3")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "住宿节约补助"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "4")),
                new XElement(ns + "TextBlock", new XAttribute("Text", "其他"), new XAttribute("Grid.Column", "1"), new XAttribute("Grid.Row", "5")),

                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.AIRFARE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "0")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.CARFARE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "1")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.LODGINGEXPENSES}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "2")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.TRAVELLINGALLOWANCE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "3")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.LODGESAVINGEXPENSES}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "4")),
                new XElement(ns + "TextBox", new XAttribute("Text", "{Binding Mode=TwoWay, Path=Entity.OTHERCHARGE}"), new XAttribute("Grid.Column", "3"), new XAttribute("Grid.Row", "5"))

            )
            );

            //将内存中的XAML实例化成为DataTemplate对象，并赋值给
            //ListBox的ItemTemplate属性，完成数据绑定
            XmlReader xr = xDataTemplate.CreateReader();
            DataTemplate dataTemplate = XamlReader.Load(xDataTemplate.ToString()) as DataTemplate;


            return dataTemplate;
        }
    }
}
