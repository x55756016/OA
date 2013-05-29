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
using System.Collections.Generic;
using System.Windows.Markup;


namespace SMT.FB.UI.Common.Controls.DataTemplates
{
    public class DataTemplateHelper
    {

        public static XElementString GetEmptyDataTemplate()
        {

            XElementString xDataTemplate =
           new XElementString("DataTemplate",
               new XAttributeString("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"),
               new XAttributeString("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml"),
               new XAttributeString("xmlns:data", "clr-namespace:System.Windows.Controls;assembly=System.Windows.Controls.Data")
               );

            return xDataTemplate;
        }

        public static DataTemplate GetDataTemplate(XElementString xDataTemplate)
        {
            XElementString xRoot = GetEmptyDataTemplate();
            xRoot.Add(xDataTemplate);

            DataTemplate dataTemplate = XamlReader.Load(xRoot.ToString()) as DataTemplate;
            return dataTemplate;
        }

        public static XElementString GetDataGridElement(List<GridItem> items)
        {
            XElementString gridXCs = new XElementString("data:DataGrid.Columns");
           
            items.ForEach(item =>
            {
                XElementString gridXColumn = new XElementString("data:DataGridTextColumn",
                                     new XAttributeString("Header", item.PropertyDisplayName),
                                     new XAttributeString("Width", item.Width.ToString())
                                     );
                if (! string.IsNullOrEmpty(item.PropertyName))
                {
                   gridXColumn.Add( new XAttributeString("Binding", "{Binding Mode=TwoWay, Path=" + item.PropertyName + "}"));
                }

                //XElement gridXColumn = new XElement(data + "DataGridTextColumn", 
                //    new XAttribute("Header", item.PropertyDisplayName),
                //    new XAttribute("Width", item.Width.ToString()),
                //    new XAttribute("Binding", "{Binding Mode=TwoWay, Path=" + item.PropertyName +"}")
                //    );
                gridXCs.Add(gridXColumn);
            });
             //  
            XElementString gridX = new XElementString("data:DataGrid", new XAttributeString("AutoGenerateColumns", "false"));
            gridX.Add(gridXCs);

            return gridX;
        }

        public static DataTemplate GetDataGrid(List<GridItem> items)
        {

            XElementString gridX = GetDataGridElement(items);
            
            return GetDataTemplate(gridX);
        }
        public static DataTemplate GetEmptyGrid()
        {

            XElementString xGrid = new XElementString("Grid");
            return GetDataTemplate(xGrid);
        }
    }

}
