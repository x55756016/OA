using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Text;
using System.Xml.Linq;
using System.Windows.Printing;
using System.Threading;
using System;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
using System.Windows.Resources;
using System.Collections.Generic;
using System.Windows.Shapes;
using SMT.SaaS.FrameworkUI.RichNotepad.InteractiveText;


namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public partial class RichMainPage : UserControl
    {
        private bool isShow = false;
        private int type = 0;
        public byte[] RichTextBoxContext  //设置获取值
        {
            get { return Utility.GetRichTextBoxContext(rtb); }
            set { 
                 Utility.SetRichTextBoxData(rtb,value);
            }
        }
   
        public RichMainPage()
        {
            InitializeComponent();
            this.DataContext = this;
            IsRTL = Thread.CurrentThread.CurrentUICulture.Parent.Name.ToLower() == "he" || Thread.CurrentThread.CurrentUICulture.Parent.Name.ToLower() == "ar";
            Loaded += new RoutedEventHandler(MainPage_Loaded);
           
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //   ************   TODO    Add sample Xaml file here    ************
            //StreamResourceInfo sr = App.GetResourceStream(new
            //    Uri("/RichNotepad;component/sampleNew.sav", UriKind.Relative));
            //StreamReader sread = new StreamReader(sr.Stream);
            //string xaml = sread.ReadToEnd();
            //rtb.Xaml = xaml;
            //sread.Close();

           
        }

        #region RichContent
        public static readonly DependencyProperty RichContentProperty = DependencyProperty.Register("RichContent", typeof(byte[]), typeof(RichMainPage), null);
        public byte[] RichContent
        {
            get
            {
                return SMT.SaaS.FrameworkUI.Common.Utility.GetRichTextBoxContext(this.rtb);
                //object obj = box.Xaml;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string StrBlocks = string.Empty;
                //    for (int i = 0; i < this.rtb.Blocks.Count; i++)
                //    {
                //        Paragraph aPara = (Paragraph)this.rtb.Blocks[i];
                //        for (int j = 0; j < aPara.Inlines.Count; j++)
                //        {
                //            Run aRun = (Run)aPara.Inlines[j];
                //            StrBlocks = StrBlocks + aRun.Text + "\n";
                //        }
                //    }
                //    byte[] bytes = Encoding.UTF8.GetBytes(StrBlocks);
                //    return bytes;
                //}

            }
            set
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetRichTextBoxData(this.rtb,value);

                //if (value == null || value.Length == 0)
                //{
                //    return;
                //}
                //using (MemoryStream stream = new MemoryStream(value))
                //{
                //    //DataContractSerializer serializer = new DataContractSerializer(typeof(string));
                //    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                //    {
                //        //string Xaml = (string)serializer.ReadObject(stream);
                //        string Xaml = reader.ReadToEnd();
                //        //txtContent.GetRichTextbox().Xaml = Xaml;
                //        this.rtb.Blocks.Clear();
                //        Run myRun = new Run();
                //        myRun.Text = Xaml;
                //        Paragraph myPara = new Paragraph();
                //        myPara.Inlines.Add(myRun);
                //        this.rtb.Blocks.Add(myPara);
                //    }
                //}
            }
        }

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRTL.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirtyProperty = DependencyProperty.Register("IsDirty", typeof(bool), typeof(RichMainPage), null);

        public bool IsRTL
        {
            get { return (bool)GetValue(IsRTLProperty); }
            set { SetValue(IsRTLProperty, value); }
        }

        public static readonly DependencyProperty IsRTLProperty = DependencyProperty.Register("IsRTL", typeof(bool), typeof(RichMainPage), null);

        public RichTextBox GetRichTextbox()
        {
            return this.rtb;
        }
        #endregion

        #region Callback for returning focus to RichTextBox
        //  Callback for returning focus to RichTextBox
        private void ReturnFocus()
        {
            if (rtb != null)
                rtb.Focus();
        }
        #endregion

        #region Position TextPointer in Text
  
        //   Position TextPointer in Text
        private void PositionHand()
        {
            Rect r;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                TextPointer tp = rtb.GetPositionFromPoint(lastRTAMouseMove.GetPosition(rtb));
                r = tp.GetCharacterRect(LogicalDirection.Forward);
            }
            else
            {
                r = rtb.Selection.End.GetCharacterRect(LogicalDirection.Forward);
            }

            if (r != Rect.Empty)
            {
                Canvas.SetLeft(caretHand, r.Left);
                Canvas.SetTop(caretHand, r.Bottom);
            }

        }


        private void rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            PositionHand();
        }
        #endregion

        #region Mouse Events BEGIN
        //Mouse Events BEGIN
        private void mainPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // prevent Silverlight menu from showing
            e.Handled = true;
        }

        private void mainPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MPContextMenu menu = new MPContextMenu(this);
            menu.Show(e.GetPosition(LayoutRoot));
        }

        private void rtb_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void rtb_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //App currentApp = (App)Application.Current;
            //// 修改当前显示页面内容. 
            
            // if (!rtb.IsReadOnly)
            //{
            //   RTBContextMenu menu = new RTBContextMenu(rtb);
            //   Point a = e.GetPosition(null);
            //   menu.Show(a);
            //}
        }
        Rectangle highlightRect;
        MouseEventArgs lastRTAMouseMove;
        private void rtb_MouseMove(object sender, MouseEventArgs e)
        {
            lastRTAMouseMove = e;
            if (showHighlight)
            {
                foreach (Rect r in m_selectionRect)
                {
                    if (r.Contains(e.GetPosition(highlightCanvas)))
                    {
                        if (highlightRect == null)
                        {
                            highlightRect = CreateHighlightRectangle(r);
                        }
                        else
                        {
                            highlightRect.Visibility = System.Windows.Visibility.Visible;
                            highlightRect.Width = r.Width;
                            highlightRect.Height = r.Height;
                            Canvas.SetLeft(highlightRect, r.Left);
                            Canvas.SetTop(highlightRect, r.Top);
                        }
                    }
                }
            }
            PositionHand();
        }
        #endregion

        #region Right Button Events

       
        private void rtb_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            VisualStateManager.GoToState(this, "DragOver", true);
        }

        private void rtb_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

      
/// 转全角的函数(SBC case) 
/// </summary> 
/// <param name="input">任意字符串</param> 
/// <returns>全角字符串</returns> 
///<remarks> 
///全角空格为12288，半角空格为32 
///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248 
///</remarks>         
public string ToSBC(string input) 
{ 
    //半角转全角： 
    char[] c = input.ToCharArray(); 
    for (int i = 0; i < c.Length; i++) 
    { 
        if (c[i] == 32) 
        { 
            c[i] = (char) 12288; 
            continue; 
        }
        //if (c[i] < 127)
        //    c[i] = (char)(c[i] + 65248); 
    } 
    return new string(c); 
} 



        private void rtb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    //执行Ctrl+E事件后的业务
                   // MessageBox.Show("粘贴事件：" + rtb.Xaml);
                  //  rtb.Xaml
                    char s='\r';
                    BlockCollection bco = rtb.Blocks;
                    for (int i = 0; i < rtb.Blocks.Count; i++)
                    {
                        Paragraph aPara = (Paragraph)rtb.Blocks[i];
                        for (int j = 0; j < aPara.Inlines.Count; j++)
                        {
                            TextElement element = (TextElement)aPara.Inlines[j];

                            if (element.GetType().Equals(typeof(Run)))
                            {
                                StringBuilder buder = new StringBuilder();
                                Run aRun = (Run)element;
                              //  MessageBox.Show("粘贴事件：" + aRun.Text.Split(s)[0].ToString());
                                
                                string[] strtr = aRun.Text.Split(s);
                                if (strtr.Length > 1)
                                {
                                  
                                    for(int m=0;m<strtr.Length-1;m++)
                                    {
                                      //  bco = new BlockCollection();

                                       // aRun.Text = str;
                                        Paragraph para = new Paragraph();
                                      //  para = (Paragraph)rtb.Blocks[0];

                                       // InlineCollection l = new InlineCollection();
                                        para.Inlines.Add("aa");
                                         TextElement elementdd = (TextElement)para.Inlines[0];
                                         Run aRundd =  (Run)elementdd;
                                         aRundd.Text = strtr[m];
                                        rtb.Selection.Insert(para);
                                      //  MessageBox.Show("粘贴事件：" + aRun.Text.Split(s)[m].ToString()+"/n"+rtb.Xaml);
                                    }
                                    rtb.Blocks.Remove(rtb.Blocks[i]);
                                }
                                

                            }
                        }
                    }

                }
            }

            if (rtb.Blocks.Count > 1 || (rtb.Blocks.Count == 1 && (rtb.Blocks[0] as Paragraph).Inlines.Count > 0))
                IsDirty = true;
            else
                IsDirty = false;
        }
        #endregion

        #region Highlight Feature

        private Rectangle CreateHighlightRectangle(Rect bounds)
        {
            Rectangle r = new Rectangle();
            r.Fill = new SolidColorBrush(Color.FromArgb(75, 0, 0, 200));
            r.Stroke = new SolidColorBrush(Color.FromArgb(230, 0, 0, 254));
            r.StrokeThickness = 1;
            r.Width = bounds.Width;
            r.Height = bounds.Height;
            Canvas.SetLeft(r, bounds.Left);
            Canvas.SetTop(r, bounds.Top);

            highlightCanvas.Children.Add(r);

            return r;

        }

        // Highlight Feature
        private bool showHighlight = false;
        private List<Rect> m_selectionRect = new List<Rect>();
        public void btnHighlight_Checked(object sender, RoutedEventArgs e)
        {
            if (!showHighlight)
            {
                showHighlight = true;

                TextPointer tp = rtb.ContentStart;
                TextPointer nextTp = null;
                Rect nextRect = Rect.Empty;
                Rect tpRect = tp.GetCharacterRect(LogicalDirection.Forward);
                Rect lineRect = Rect.Empty;

                int lineCount = 1;

                while (tp != null)
                {
                    nextTp = tp.GetNextInsertionPosition(LogicalDirection.Forward);
                    if (nextTp != null && nextTp.IsAtInsertionPosition)
                    {
                        nextRect = nextTp.GetCharacterRect(LogicalDirection.Forward);
                        // this occurs for more than one line
                        if (nextRect.Top > tpRect.Top)
                        {
                            if (m_selectionRect.Count < lineCount)
                                m_selectionRect.Add(lineRect);
                            else
                                m_selectionRect[lineCount - 1] = lineRect;

                            lineCount++;

                            if (m_selectionRect.Count < lineCount)
                                m_selectionRect.Add(nextRect);

                            lineRect = nextRect;
                        }
                        else if (nextRect != Rect.Empty)
                        {
                            if (tpRect != Rect.Empty)
                                lineRect.Union(nextRect);
                            else
                                lineRect = nextRect;
                        }
                    }
                    tp = nextTp;
                    tpRect = nextRect;
                }
                if (lineRect != Rect.Empty)
                {
                    if (m_selectionRect.Count < lineCount)
                        m_selectionRect.Add(lineRect);
                    else
                        m_selectionRect[lineCount - 1] = lineRect;
                }
                while (m_selectionRect.Count > lineCount)
                {
                    m_selectionRect.RemoveAt(m_selectionRect.Count - 1);
                    //DeleteRect();
                }
            }
            else
            {
                showHighlight = false;
                if (highlightRect != null)
                {
                    highlightRect.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

        }
        #endregion
      

        // Convert to XAML Button
        public void btnMarkUp_Checked(object sender, RoutedEventArgs e)
        {
           
        }
        //  TODO   Add btnRTL_Checked code here
        public void btnRTL_Checked(object sender, RoutedEventArgs e)
        {
           
        }

        #region Font
        private void btnBold_Click(object sender, RoutedEventArgs e)
        {
            if (rtb != null && rtb.Selection.Text.Length > 0)
            {
                if (rtb.Selection.GetPropertyValue(Run.FontWeightProperty) is FontWeight &&
                    ((FontWeight)rtb.Selection.GetPropertyValue(Run.FontWeightProperty)) == FontWeights.Normal)
                    rtb.Selection.ApplyPropertyValue(Run.FontWeightProperty, FontWeights.Bold);
                else
                    rtb.Selection.ApplyPropertyValue(Run.FontWeightProperty, FontWeights.Normal);

            }
            ReturnFocus();
        }

        private void btnItalic_Click(object sender, RoutedEventArgs e)
        {
            if (rtb != null && rtb.Selection.Text.Length > 0)
            {
                if (rtb.Selection.GetPropertyValue(Run.FontStyleProperty) is FontStyle &&
                    ((FontStyle)rtb.Selection.GetPropertyValue(Run.FontStyleProperty)) == FontStyles.Normal)
                    rtb.Selection.ApplyPropertyValue(Run.FontStyleProperty, FontStyles.Italic);
                else
                    rtb.Selection.ApplyPropertyValue(Run.FontStyleProperty, FontStyles.Normal);
            }
            ReturnFocus();

        }

        private void btnUnderline_Click(object sender, RoutedEventArgs e)
        {
            if (rtb != null && rtb.Selection.Text.Length > 0)
            {
                if (rtb.Selection.GetPropertyValue(Run.TextDecorationsProperty) == null)
                    rtb.Selection.ApplyPropertyValue(Run.TextDecorationsProperty, TextDecorations.Underline);
                else
                    rtb.Selection.ApplyPropertyValue(Run.TextDecorationsProperty, null);
            }
            ReturnFocus();

        }

        private void cmbFontSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rtb != null && rtb.Selection.Text.Length > 0)
            {
                rtb.Selection.ApplyPropertyValue(Run.FontSizeProperty,
                    double.Parse((cmbFontSizes.SelectedItem as ComboBoxItem).Tag.ToString()));
            }
            ReturnFocus();


        }

        private void cmbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rtb != null && rtb.Selection.Text.Length > 0)
            {
                rtb.Selection.ApplyPropertyValue(Run.FontFamilyProperty,
                    new FontFamily((cmbFonts.SelectedItem as ComboBoxItem).Tag.ToString()));
            }
            ReturnFocus();


        }

        private void cmbFontColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rtb != null && rtb.Selection.Text.Length > 0)
            {
                string color = (cmbFontColors.SelectedItem as ComboBoxItem).Tag.ToString();

                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(
                    byte.Parse(color.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(color.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(color.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
                    byte.Parse(color.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)));

                rtb.Selection.ApplyPropertyValue(Run.ForegroundProperty, brush);
            }

        }
        #endregion

        #region Image
        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            //Uri imageUri = new Uri("http://t3.baidu.com/it/u=2499248362,4035544451&fm=0&gp=0.jpg", UriKind.RelativeOrAbsolute);
            //InlineUIContainer container = new InlineUIContainer();
            ////  System.Windows.Documents.TextElement.
            //container.Child = CreateImageFromUri(imageUri, 200, 150);
            //rtb.Selection.Insert(container);

            //ReturnFocus();


            InsertURL cw = new InsertURL(rtb.Selection.Text, true);
            cw.HasCloseButton = false;
            cw.Closed += (s, args) =>
            {
                if (cw.DialogResult.Value)
                {
                    InlineUIContainer container = new InlineUIContainer();
                    int w = 200;
                    int.TryParse(cw.txtWidth.Text, out w); //图片宽度
                    int h = 150;
                    int.TryParse(cw.txtHeight.Text, out h); //图片高度

                    //输入图片路径 
                    if (cw.fs != null)
                    {
                        BitmapImage bmp = null;
                        try
                        {
                            bmp = cw.bmp;
                            Image img = new Image();
                            img.Stretch = Stretch.Uniform;
                            img.Width = w;
                            img.Height = h; //图片高度
                            img.Source = bmp;
                            img.Tag = bmp.UriSource.ToString();
                          //  bmp.DownloadProgress += new EventHandler<DownloadProgressEventArgs>(bmp_DownloadProgress);
                            container.Child = img;
                            rtb.Selection.Insert(container);
                            ReturnFocus();

                        }
                        catch
                        {
                            bmp = null;
                        }
                    }
                    else
                    {
                        string url = cw.txtURL.Text;
                        //添加图片插入到InlineUIContainer之中 
                        Uri imageUri = new Uri(url, UriKind.RelativeOrAbsolute);
                        container.Child =Utility.CreateImageFromUri(imageUri, w, h);
                        rtb.Selection.Insert(container);
                        ReturnFocus();

                    }

                }
            };
            cw.Show();
        }
        private Image getImage()
        {
            return Utility.CreateImageFromUri(new Uri("desert.jpg", UriKind.RelativeOrAbsolute), 200, 150);
        }
        #endregion

        #region Hyperlink
        private void btnHyperlink_Click(object sender, RoutedEventArgs e)
        {
            //rtb.DisplayActionDefinition();
            InsertURL cw = new InsertURL(rtb.Selection.Text, false);
            cw.HasCloseButton = false;
            cw.Closed += (s, args) =>
            {
                if (cw.DialogResult.Value)
                {
                    Hyperlink hyperlink = new Hyperlink();
                    hyperlink.TargetName = "_blank";
                    hyperlink.NavigateUri = new Uri(cw.txtURL.Text);

                    //if (cw.txtURLDesc.Text.Length > 0)
                    //    hyperlink.Inlines.Add(cw.txtURLDesc.Text);
                    //else
                        hyperlink.Inlines.Add(cw.txtURL.Text);

                    rtb.Selection.Insert(hyperlink);
                }
            };

            cw.Show();
        }
        #endregion

        #region Datagrid
        private void btnDatagrid_Click(object sender, RoutedEventArgs e)
        {
            InlineUIContainer container = new InlineUIContainer();

            container.Child = getDataGrid();

            rtb.Selection.Insert(container);
            ReturnFocus();
        }

        private DataGrid getDataGrid()
        {
            DataGrid dg = new DataGrid();
            dg.AutoGenerateColumns = true;
            dg.Width = 500;
            dg.Height = 150;
            dg.ItemsSource = Customer.GetSampleCustomerList();
            dg.Style = (Style)Application.Current.Resources["DataGridStyle"];

            return dg;
        }
        #endregion

        #region Calendar
        private void btnCalendar_Click(object sender, RoutedEventArgs e)
        {
            InlineUIContainer container = new InlineUIContainer();
            container.Child = getCalendar();
            rtb.Selection.Insert(container);
            ReturnFocus();
        }

        private Calendar getCalendar()
        {
            Calendar cal = new Calendar();
            cal.Width = 179;
            cal.Height = 169;
            cal.FontFamily = new FontFamily("Portable User Interface");
            cal.Style = (Style)this.Resources["CalendarStyle1"];

            return cal;
        }
        #endregion      

        #region 管理标头控件是否显示
        public void HideControls()
        {
            mainPanel.Visibility = Visibility.Collapsed;
            rtb.IsReadOnly = true;
            
            RichTextBox2.Background = new SolidColorBrush(Colors.Transparent);
            RichTextBox3.Fill = new SolidColorBrush(Colors.Transparent);
            rtb.BorderThickness = new Thickness(0);
            rtb.Background = new SolidColorBrush(Colors.Transparent);
            xamlTb.Background = new SolidColorBrush(Colors.Transparent);
            LayoutRoot.Background = new SolidColorBrush(Colors.Transparent);
            RichTextBox4.Background = new SolidColorBrush(Colors.Transparent);
            highlightCanvas.Background = new SolidColorBrush(Colors.Transparent);
            
        }

        //2011-03-15
        public void HideRichBoxBorder()
        {
            RichTextBoxBorder1.BorderThickness = new Thickness(0);
        }

        public void ShowControls()
        {
            mainPanel.Visibility = Visibility.Visible;
            rtb.IsReadOnly = false;
        }
        #endregion

        #region New;Save;Open
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            bool clear = true;
            if (IsDirty)
            {
                if (MessageBox.Show("All changes will be lost. Continue?",
                    "Continue", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    clear = false;
                }
            }

            if (clear)
            {
                rtb.Blocks.Clear();
                IsDirty = false;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string xaml = rtb.Xaml;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".sav";
            sfd.Filter = "Saved Files|*.sav|All Files|*.*";
            if (sfd.ShowDialog().Value)
            {
                using (FileStream fs = (FileStream)sfd.OpenFile())
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    byte[] buffer = enc.GetBytes(xaml);
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Close();
                    IsDirty = false;
                }
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Saved Files|*.sav|All Files|*.*";

            if (ofd.ShowDialog().Value)
            {
                FileInfo fi = ofd.File;
                using (StreamReader reader = fi.OpenText())
                {
                    rtb.Xaml = reader.ReadToEnd();
                }
            }
        }
        #endregion

        #region Paste;Cut;Copy
        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            rtb.Selection.Text = Clipboard.GetText();
            ReturnFocus();
        }

        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(rtb.Selection.Text);
            rtb.Selection.Text = "";
            ReturnFocus();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(rtb.Selection.Text);
            ReturnFocus();
        }
        #endregion

        #region Print
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDocument theDoc = new PrintDocument();
            string DocumentName = "Silverlight 4 Text Editor - Opened Document";
            theDoc.PrintPage += (s, args) =>
            {
                args.PageVisual = rtb;
                args.HasMorePages = false;
            };
            theDoc.EndPrint += (s, args) =>
            {
                MessageBox.Show("The document printed successfully", "Text Editor", MessageBoxButton.OK);
            };


            theDoc.Print(DocumentName);
        }

        #endregion
       
        #region rtb_Drop
        //   Drag & Drop of Word or txt files
        private void rtb_Drop(object sender, System.Windows.DragEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            if (e.Data != null)
            {
                IDataObject f = e.Data as IDataObject;

                if (f != null)
                {
                    object data = f.GetData(DataFormats.FileDrop);
                    FileInfo[] files = data as FileInfo[];

                    if (files != null)
                    {
                        foreach (FileInfo file in files)
                        {
                            if (file != null)
                            {
                                if (file.Extension.Equals(".txt"))
                                {
                                    try
                                    {
                                        Stream sr = file.OpenRead();
                                        string contents;
                                        using (StreamReader reader = new StreamReader(sr))
                                        {
                                            contents = reader.ReadToEnd();
                                        }
                                        rtb.Selection.Text = contents;
                                    }
                                    catch (IOException ex)
                                    {
                                        //check if message is for a File IO
                                        docOpenedError();
                                    }
                                }
                                else if (file.Extension.Equals(".docx"))
                                {
                                    try
                                    {
                                        Stream sr = file.OpenRead();
                                        string contents;

                                        StreamResourceInfo zipInfo = new StreamResourceInfo(sr, null);
                                        StreamResourceInfo wordInfo = Application.GetResourceStream(zipInfo, new Uri("word/document.xml", UriKind.Relative));

                                        using (StreamReader reader = new StreamReader(wordInfo.Stream))
                                        {
                                            contents = reader.ReadToEnd();
                                        }
                                        XDocument xmlFile = XDocument.Parse(contents);
                                        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

                                        var query = from xp in xmlFile.Descendants(w + "p")
                                                    select xp;
                                        Paragraph p = null;
                                        Run r = null;
                                        foreach (XElement xp in query)
                                        {
                                            p = new Paragraph();
                                            var query2 = from xr in xp.Descendants(w + "r")
                                                         select xr;
                                            foreach (XElement xr in query2)
                                            {
                                                r = new Run();
                                                var query3 = from xt in xr.Descendants()
                                                             select xt;
                                                foreach (XElement xt in query3)
                                                {
                                                    if (xt.Name == (w + "t"))
                                                        r.Text = xt.Value.ToString();
                                                    else if (xt.Name == (w + "br"))
                                                        p.Inlines.Add(new LineBreak());
                                                }
                                                p.Inlines.Add(r);
                                            }
                                            p.Inlines.Add(new LineBreak());
                                            rtb.Blocks.Add(p);
                                        }
                                    }
                                    catch (IOException ex)
                                    {
                                        //check if message is for a File IO
                                        docOpenedError();
                                    }

                                }
                            }
                        }
                    }
                }
            }
            ReturnFocus();
        }
        #endregion
    
        private void docOpenedError()
        {
            //check if message is for a File IO
            MessageBox.Show("The document may already be open.  Please ensure no other application has the document opened or locked and try again", "Drag & Drop Error", MessageBoxButton.OK);
        }

        #region 对齐方式
        private void btnRleft_Click(object sender, RoutedEventArgs e)
        {
            rtb.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
            ReturnFocus();
        }

        private void btnRCenter_Click(object sender, RoutedEventArgs e)
        {
              rtb.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);
            ReturnFocus();
        }
        private void btnRRight_Click(object sender, RoutedEventArgs e)
        {
            rtb.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Right);
            ReturnFocus();
        }

        #endregion
      
        void rtb_LayoutUpdated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #region Table
        private void btnAddTable_Click(object sender, RoutedEventArgs e)
        {
            InlineUIContainer container = new InlineUIContainer();

            container.Child = this.getDataGridD();

            rtb.Selection.Insert(container);
            ReturnFocus();
        }
      
        private DataGrid getDataGridD()
        {
            DataGrid dg = new DataGrid();
            for (int i = 0; i < 4; i++)
            {
                DataGridTextColumn item = new DataGridTextColumn();
                item.Header = "  ";
                item.MinWidth = 50;
               // item.

                dg.Columns.Add(item);

            }
            DataGridRow dr = new DataGridRow();
            //Formatting
           

           // dg.LoadingRow+=new EventHandler<DataGridRowEventArgs>(dg_LoadingRow);
            // DataGridColumn
           // dg.Columns.Add(new  DataGridColumn
            dg.AutoGenerateColumns = true;
            dg.Width = 500;
            dg.Height = 150;
           // dg.ItemsSource = Customer.GetSampleCustomerList();
            dg.Style = (Style)Application.Current.Resources["DataGridStyle"];

            return dg;
        }
        #endregion

        #region 数据编号方式

     
        private void btnbullets_Click(object sender, RoutedEventArgs e)
        {
          //  rtb.Selection.ApplyPropertyValue(Paragraph., TextAlignment.Left);
            //foreach (CellReference reference in this.rtb.Selection)
            //{
            //    if (reference.Element is RichTextBlock)
            //    {
            //        RichTextBox element = (RichTextBox)reference.Element;
            //        element.SelectAll();
            //        element.ApplyFormattingToSelection(format, param, updateHistory);
            //        element.ClearSelection();
            //    }
            //}
            try
            {
                TextPointer tp = rtb.Selection.Start;
                Run run = (Run)tp.Parent;
                string context = run.Text;
                if (context.Length > 7)
                {
                    context = context.Substring(0, 7);
                }
                string[] qie = new string[] { "   ● " };
                string[] ch = context.Split(qie, StringSplitOptions.None);
                if (ch.Length == 1)
                {
                    string strname = "   ● " + run.Text;
                    run.Text = strname;
                    type = 1;
                    this.KeyDown += new KeyEventHandler(RichMainPage_KeyDown);
                    this.KeyUp += new KeyEventHandler(RichMainPage_KeyUp);

                }
                if (ch.Length == 2)
                {
                    run.Text = ch[1].ToString();
                }
                ReturnFocus();
            }
            catch (Exception ex)
            { }
 
        }
        string strname = "";
        void RichMainPage_KeyUp(object sender, KeyEventArgs e)
        {
            
                if (e.Key == Key.Enter)
                {
                    try
                    {
                        if (!isShow && type == 1)
                        {
                            TextPointer tp = rtb.Selection.Start;
                            Run run = (Run)tp.Parent;
                            run.Text = "   ● ";
                            //  rtb.Selection.Insert(run);

                        }
                        if (!isShow && type == 2)
                        {
                            TextPointer tp = rtb.Selection.Start;
                            Run runt = (Run)tp.Parent;
                            runt.Text = strname;
                            //  rtb.Selection.Insert(runt);
                        }
                        isShow = true;

                    }
                    catch (Exception ex)
                    { }
            }
          
           
        }

        void RichMainPage_KeyDown(object sender, KeyEventArgs e)
        {
           

                if (e.Key == Key.Enter)
                {
                    try
                    {
                        if (type == 1)
                        {

                            TextPointer tp = rtb.Selection.Start;
                            Run runs = (Run)tp.Parent;
                            string context = runs.Text;
                            if (context == "   ● ")
                            {
                                runs.Text = "";
                                isShow = true;

                            }
                            else
                            {
                                string[] qie = new string[] { "   ● " };
                                string[] ch = context.Split(qie, StringSplitOptions.None);
                                if (ch.Length == 2)
                                {
                                    isShow = false;
                                }
                                else
                                {
                                    isShow = true;
                                }
                            }
                        }

                        if (type == 2)
                        {
                            TextPointer tp = rtb.Selection.Start;
                            Run run = (Run)tp.Parent;
                            int value = 0;
                            string context = run.Text;
                            string[] qie = new string[] { "   " };
                            string[] ch = context.Split(qie, StringSplitOptions.None);
                            // isShow = true;
                            if (ch.Length > 1)
                            {
                                string[] li = ch[1].Split(':');
                                if (li.Length > 1)
                                {
                                    int.TryParse(li[0].ToString(), out value);
                                    if (li[1].ToString().Trim() != "")
                                    {
                                        isShow = false;
                                    }
                                    else
                                    {
                                        run.Text = "";
                                        isShow = true;
                                    }
                                }
                                

                            }
                            value += 1;
                            strname = "   " + value + ": ";
                            //run.Text = strname;
                        }
                    }
                    catch (Exception ex)
                    { }

                }

        }

        private void btnnumbering_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextPointer tp = rtb.Selection.Start;
                Run run = (Run)tp.Parent;
               // string context = run.Text.Substring(0,7);
                string context = run.Text;
                if (context.Length > 7)
                {
                    context = context.Substring(0, 7);
                }
                int value = 0;
                string[] qie = new string[] { "   " };
                string[] ch = context.Split(qie, StringSplitOptions.None);

                if (ch.Length ==2)
                {
                    string[] li = ch[1].Split(':');
                    if (li.Length > 1)
                    {
                        int.TryParse(li[0].ToString(), out value);
                    }
                }
                if (ch.Length == 2)
                {
                    strname = "";
                }
                else
                {
                    value += 1;
                    strname = "   " + value + ": " + run.Text;
                }

                run.Text = strname;
                type = 2;
                this.KeyDown += new KeyEventHandler(RichMainPage_KeyDown);
                this.KeyUp += new KeyEventHandler(RichMainPage_KeyUp);
                ReturnFocus();
            }
            catch (Exception ex)
            { }
        }

        private void btnmultilevellist_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 编辑与只读
        private void btnRO_Checked(object sender, RoutedEventArgs e)
        {
            rtb.IsReadOnly = !rtb.IsReadOnly;

            //Set the button image based on the state of the toggle button.
            if (rtb.IsReadOnly)
                btnRO.Content = RichMainPage.createImageFromUri(new Uri("/SMT.SAAS.Images;Component/Images/Office/view.png", UriKind.RelativeOrAbsolute), 20, 20);
            else
                btnRO.Content = RichMainPage.createImageFromUri(new Uri("/SMT.SAAS.Images;Component/Images/Office/edit.png", UriKind.RelativeOrAbsolute), 20, 20);
            ReturnFocus();
        }
        private static Image createImageFromUri(Uri URI, double width, double height)
        {
            Image img = new Image();
            img.Stretch = Stretch.Uniform;
            img.Width = width;
            img.Height = height;
            BitmapImage bi = new BitmapImage(URI);
            img.Source = bi;
            img.Tag = bi.UriSource.ToString();
            return img;
        }
        #endregion

        #region 撤销或重复
 
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        private void rtb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                
            }
            //if (e.Key == Key.V)
            //{
            //    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            //    {
            //        //执行Ctrl+E事件后的业务
            //        MessageBox.Show("粘贴事件：" + rtb.Xaml);
            //    }
            //}

//if(e.Control && e.KeyCode==Keys.V)  e.Handled = true;  }   
            //if (e.Key == Key.Ctrl && e.PlatformKeyCode == 86)
            //{
            //    MessageBox.Show("复制");
            //}
            //else if (e.Key == Key.Ctrl && e.Key == Key.V)
            //{
            //    MessageBox.Show("粘贴");
            //}
        }
        bool aaa = true;
        byte[] richddd = null;
        private void aaaaaaaaa_Click(object sender, RoutedEventArgs e)
        {
           
            if (aaa)
            {
                richddd = this.RichTextBoxContext;
                MessageBox.Show("赋值中。。。请稍等");
                aaa = false;
            }
            else
            {
                this.RichTextBoxContext = richddd;
                aaa = true;
            }
            
           
        }
        //void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    int index = e.Row.GetIndex();
        //    var cell = dg.Columns[0].GetCellContent(e.Row) as TextBlock;
        //    cell.Text = (index + 1).ToString();

        //}

    }

    #region Customer
    public class Customer
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Address { get; set; }

        public Customer() { }

        public Customer(String firstName, String lastName,
            String address)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Address = address;
        }

        public static List<Customer> GetSampleCustomerList()
        {
            return new List<Customer>(new Customer[4] {
                new Customer("A", "", 
                    " "), 
                new Customer("B", "", 
                    ""),
                new Customer("C", "", 
                    ""),
                new Customer("D", "", 
                    "")
            });
        }
    }
    #endregion
}
