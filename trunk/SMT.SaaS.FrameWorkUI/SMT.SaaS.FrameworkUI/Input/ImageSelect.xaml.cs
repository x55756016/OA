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

using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace SMT.SaaS.FrameworkUI
{
    public partial class ImageSelect : UserControl
    {
        
        public string ButtonToolTip
        {
            get
            {
                return GetValue(ButtonTooltipProperty) as string;
            }
            set
            {
                SetValue(ButtonTooltipProperty, value);
            }
        }

        public  DependencyProperty ImageFieldProperty;
        public  DependencyProperty ButtonTooltipProperty;

        public byte[] ImageField
        {
            get
            {
                return GetValue(ImageFieldProperty) as byte[];
            }
            set 
            {
                SetValue(ImageFieldProperty, value);
            }
        }

        public ImageSelect()
        {
            ImageFieldProperty = DependencyProperty.Register("ImageField", typeof(byte[]), typeof(ImageSelect)
                , new PropertyMetadata(null, new PropertyChangedCallback(ImageSelect.OnImagePropertyChanged)));

            ButtonTooltipProperty = DependencyProperty.Register("ButtonToolTip", typeof(string), typeof(ImageSelect)
    , new PropertyMetadata("Select Image", new PropertyChangedCallback(ImageSelect.OnButtonTooltipPropertyChanged)));

            InitializeComponent();
        }

        public static void OnButtonTooltipPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ImageSelect obj = sender as ImageSelect;
            if (obj != null)
            {
                obj.OnButtonTooltipPropertyChanged(e);
            }
        }
        public static void OnImagePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ImageSelect obj = sender as ImageSelect;
            if (obj != null)
            {
                obj.OnImagePropertyChanged(e);
            }
        }

        private void OnButtonTooltipPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ToolTipService.SetToolTip(imgSelect, ButtonToolTip);
        }

        private void OnImagePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            //Image = e.NewValue as byte[];
            BindImageField();
        }
        private void BindImageField()
        {
            if (ImageField != null && ImageField.Length > 0)
            {
                try
                {
                    BitmapImage img = new BitmapImage();
                    
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(ImageField);
                    
                    img.SetSource(stream);
                    stream.Close();

                    imgPhoto.Source = img;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void imgSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            // Set filter options and filter index.
            ofd.Filter = "Image Files (*.bmp, *.jpg,*.png)|*.bmp;*.jpg;*.png|All Files (*.*)|*.*";
            ofd.FilterIndex = 1;

            ofd.Multiselect = false;

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    System.IO.FileStream stream = ofd.File.OpenRead();

                    byte[] tmpByte = new byte[stream.Length];
                    stream.Read(tmpByte, 0, tmpByte.Length);
                    //BitmapImage img = new BitmapImage();
                    //img.SetSource(stream);
                    //imgPhoto.Source = img;

                    stream.Close();
                
                    ImageField = tmpByte;
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

    }
}
