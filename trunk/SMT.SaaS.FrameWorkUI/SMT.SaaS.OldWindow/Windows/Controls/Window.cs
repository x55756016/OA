
# region Using Directives

using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

# endregion

namespace System.Windows.Controls
{
    /// <summary>
    /// ChildWindow,用于弹出可操作的对话框
    /// </summary>
    [TemplatePart(Name = Window.RootGridName, Type = typeof(Grid))]
    [TemplatePart(Name = Window.GripName, Type = typeof(UIElement))]
    [TemplatePart(Name = Window.DragName, Type = typeof(UIElement))]
    [TemplatePart(Name = Window.CommandButtonsContainerName, Type = typeof(UIElement))]
    public class Window : ContentControl
    {
        # region Declarations

        //private IDisposable _ContentSizeChangedDisposable;
        private Action _UnsubscribeContainerSizeChanged;
        private LayoutChange _UserLayoutChange;
        private LayoutChange _PreviewLayoutChange;
        private LayoutChange _CurrentLayoutChange;
        private Point _PreviousMousePosition;
        private double _CursorOffsetX;
        private double _CursorOffsetY;

        private Action _RemoveFromParent;

        public event EventHandler Closed;
        private Delegate _Closed;

        private const string RootGridName = "RootGrid";

        private const string GripName = "Grip";
        private UIElement _Grip;
        private bool _HasGrip;
        private bool _IsMouseOverGrip;

        private const string DragName = "Drag";
        private bool _HasDrag;
        private bool _IsMouseOverDrag;

        private const string CommandButtonsContainerName = "CommandButtonsContainer";
        private bool _IsMouseOverButtonsContainer;

        # endregion 

        # region Constructor

        public Window()
        {
            this.DefaultStyleKey = typeof(Window);
            this.DataContext = this;

            this.CloseCommand = new ActionCommand((result) => this.Close(result ?? this.Result), () => this.CanClose);
            this.ToggleMaximizeCommand = new ActionCommand(this.ToggleMaximize, () => this.CanMaximize);

            this.ChangeHorizontalMargins = this.ChangeVerticalMargins = true;

            this.Loaded += this.OnLoaded;
        }

        # endregion

        # region Properties

        # region OverlayTemplate

        public static readonly DependencyProperty OverlayTemplateProperty =
            DependencyProperty.Register("OverlayTemplate", typeof(DataTemplate),
            typeof(Window), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate OverlayTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(Window.OverlayTemplateProperty);
            }
            set
            {
                this.SetValue(Window.OverlayTemplateProperty, value);
            }
        }

        # endregion

        # region Popup

        private Popup _Popup;

        private Popup Popup
        {
            get
            {
                if (this._Popup.IsNull())
                {
                    this._Popup = new Popup();

                    this._Popup.Child = this.InternalContainerGrid;
                }
                return this._Popup;
            }
            set
            {
                this._Popup = value;
            }
        }

        # endregion

        # region Container

        private FrameworkElement _Container;

        public FrameworkElement Container
        {
            get
            {
                return this._Container;
            }
            set
            {
                if (this._Container != value)
                {
                    if (this.IsOpened)
                    {
                        throw new InvalidOperationException(
                            "Cannot change \"Container\" property when Window is opened.");
                    }

                    this._Container = value;
                }
            }
        }

        # endregion

        # region Overlay

        private UIElement _Overlay;

        private bool _IsOverlayLoaded;

        public UIElement Overlay
        {
            get
            {
                if (!this._IsOverlayLoaded)
                {
                    if (this.OverlayTemplate.IsNotNull())
                    {
                        var overlay = this.OverlayTemplate.LoadContent();

                        if (overlay.IsNotNull())
                        {
                            if (!(overlay is UIElement))
                            {
                                throw new TemplateException(
                                    "The OverlayTemplate must be of type UIElement.");
                            }
                            this._Overlay = (UIElement)overlay;
                        }
                    }
                    this._IsOverlayLoaded = true;
                }
                return this._Overlay;
            }
        }

        # endregion

        # region InternalContainerGrid

        private Grid _InternalContainerGrid;

        internal Grid InternalContainerGrid
        {
            get
            {
                if (this._InternalContainerGrid.IsNull())
                {
                    var grid = this._InternalContainerGrid = new Grid();

                    grid.Children.Add(this);
                }
                return this._InternalContainerGrid;
            }
        }

        # endregion

        # region DialogMode

        private DialogMode _DialogMode;

        public DialogMode DialogMode
        {
            get
            {
                return this._DialogMode;
            }
            set
            {
                if (this.IsOpened)
                {
                    throw new InvalidOperationException(
                        "Cannot change the type of Window when it's opened.");
                }
                this._DialogMode = value;
            }
        }

        # endregion

        # region IsModal

        public bool IsModal
        {
            get
            {
                return this.DialogMode != DialogMode.Default;
            }
            set
            {
                this.DialogMode = this.IsModalToDialogMode(value);
            }
        }

        # endregion

        # region IsOpened

        private bool _IsOpened;

        public bool IsOpened
        {
            get
            {
                return this._IsOpened;
            }
            set
            {
                if (this._IsOpened != value)
                {
                    this._IsOpened = value;

                    if (value)
                    {
                        this.Show();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
        }

        # endregion

        # region TitleContent

        public static readonly DependencyProperty TitleContentProperty =
            DependencyProperty.Register("TitleContent", typeof(object),
            typeof(Window), new PropertyMetadata("Title"));

        public object TitleContent
        {
            get
            {
                return (object)this.GetValue(Window.TitleContentProperty);
            }
            set
            {
                this.SetValue(Window.TitleContentProperty, value);
            }
        }

        # endregion

        # region IconContent

        public static readonly DependencyProperty IconContentProperty =
            DependencyProperty.Register("IconContent", typeof(object),
            typeof(Window), null);

        public object IconContent
        {
            get
            {
                return (object)this.GetValue(Window.IconContentProperty);
            }
            set
            {
                this.SetValue(Window.IconContentProperty, value);
            }
        }

        # endregion

        # region CloseCommand

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ActionCommand),
            typeof(Window), null);

        public ActionCommand CloseCommand
        {
            get
            {
                return (ActionCommand)this.GetValue(Window.CloseCommandProperty);
            }
            set
            {
                this.SetValue(Window.CloseCommandProperty, value);
            }
        }

        # endregion

        # region ToggleMaximizeCommand

        public static readonly DependencyProperty ToggleMaximizeCommandProperty =
            DependencyProperty.Register("ToggleMaximizeCommand", typeof(ActionCommand),
            typeof(Window), null);

        public ActionCommand ToggleMaximizeCommand
        {
            get
            {
                return (ActionCommand)this.GetValue(Window.ToggleMaximizeCommandProperty);
            }
            set
            {
                this.SetValue(Window.ToggleMaximizeCommandProperty, value);
            }
        }

        # endregion

        # region IsMaximized

        private bool _IsMaximized;

        private double _Width;
        private double _Heigth;
        private HorizontalAlignment _HorizontalAlignment;
        private VerticalAlignment _VerticalAlignment;
        private Thickness _Margin;

        public bool IsMaximized
        {
            get
            {
                return this._IsMaximized;
            }
            set
            {
                if (this._IsMaximized != value)
                {
                    this._IsMaximized = value;

                    if (value)
                    {
                        this._Width = this.Width;
                        this._Heigth = this.Height;
                        this._Margin = this.Margin;
                        this._HorizontalAlignment = this.HorizontalAlignment;
                        this._VerticalAlignment = this.VerticalAlignment;

                        this.Width = this.Height = double.NaN;
                        this.Margin = new Thickness();

                        this.HorizontalAlignment = HorizontalAlignment.Stretch;
                        this.VerticalAlignment = VerticalAlignment.Stretch;
                    }
                    else
                    {
                        this.Width = this._Width;
                        this.Height = this._Heigth;
                        this.HorizontalAlignment = this._HorizontalAlignment;
                        this.VerticalAlignment = this._VerticalAlignment;
                        this.Margin = this._Margin;
                    }
                }
            }
        }

        # endregion

        # region CanMaximize

        public static readonly DependencyProperty CanMaximizeProperty =
            DependencyProperty.Register("CanMaximize", typeof(bool),
            typeof(Window), new PropertyMetadata(true));

        public bool CanMaximize
        {
            get
            {
                return (bool)this.GetValue(Window.CanMaximizeProperty);
            }
            set
            {
                this.SetValue(Window.CanMaximizeProperty, value);
            }
        }

        # endregion

        # region MaximizeButtonVisibility

        public static readonly DependencyProperty MaximizeButtonVisibilityProperty =
            DependencyProperty.Register("MaximizeButtonVisibility", typeof(Visibility),
            typeof(Window), new PropertyMetadata(Visibility.Visible));

        public Visibility MaximizeButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(Window.MaximizeButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(Window.MaximizeButtonVisibilityProperty, value);
            }
        }

        # endregion

        # region CanClose

        public static readonly DependencyProperty CanCloseProperty =
            DependencyProperty.Register("CanClose", typeof(bool),
            typeof(Window), new PropertyMetadata(true));

        public bool CanClose
        {
            get
            {
                return (bool)this.GetValue(Window.CanCloseProperty);
            }
            set
            {
                this.SetValue(Window.CanCloseProperty, value);
            }
        }

        # endregion

        # region CloseButtonVisibility

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility),
            typeof(Window), new PropertyMetadata(Visibility.Visible));

        public Visibility CloseButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(Window.CloseButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(Window.CloseButtonVisibilityProperty, value);
            }
        }

        # endregion

        # region ResizeGripVisibility

        public static readonly DependencyProperty ResizeGripVisibilityProperty =
            DependencyProperty.Register("ResizeGripVisibility", typeof(Visibility),
            typeof(Window), new PropertyMetadata(Visibility.Visible));

        public Visibility ResizeGripVisibility
        {
            get
            {
                return (Visibility)this.GetValue(Window.ResizeGripVisibilityProperty);
            }
            set
            {
                this.SetValue(Window.ResizeGripVisibilityProperty, value);
            }
        }

        # endregion

        # region CanResize

        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register("CanResize", typeof(bool),
            typeof(Window), new PropertyMetadata(true));

        public bool CanResize
        {
            get
            {
                return (bool)this.GetValue(Window.CanResizeProperty);
            }
            set
            {
                this.SetValue(Window.CanResizeProperty, value);
            }
        }

        # endregion

        # region CanDrag

        public static readonly DependencyProperty CanDragProperty =
            DependencyProperty.Register("CanDrag", typeof(bool),
            typeof(Window), new PropertyMetadata(true));

        public bool CanDrag
        {
            get
            {
                return (bool)this.GetValue(Window.CanDragProperty);
            }
            set
            {
                this.SetValue(Window.CanDragProperty, value);
            }
        }

        # endregion

        # region ResizeSize

        public static readonly DependencyProperty ResizeSizeProperty =
            DependencyProperty.Register("ResizeSize", typeof(int),
            typeof(Window), new PropertyMetadata(6));

        public int ResizeSize
        {
            get
            {
                return (int)this.GetValue(Window.ResizeSizeProperty);
            }
            set
            {
                this.SetValue(Window.ResizeSizeProperty, value);
            }
        }

        # endregion

        # region HorizontalResizeCorner

        public static readonly DependencyProperty HorizontalResizeCornerProperty =
            DependencyProperty.Register("HorizontalResizeCorner", typeof(int),
            typeof(Window), new PropertyMetadata(20));

        public int HorizontalResizeCorner
        {
            get
            {
                return (int)this.GetValue(Window.HorizontalResizeCornerProperty);
            }
            set
            {
                this.SetValue(Window.HorizontalResizeCornerProperty, value);
            }
        }

        # endregion

        # region VerticalResizeCorner

        public static readonly DependencyProperty VerticalResizeCornerProperty =
            DependencyProperty.Register("VerticalResizeCorner", typeof(int),
            typeof(Window), new PropertyMetadata(6));

        public int VerticalResizeCorner
        {
            get
            {
                return (int)this.GetValue(Window.VerticalResizeCornerProperty);
            }
            set
            {
                this.SetValue(Window.VerticalResizeCornerProperty, value);
            }
        }

        # endregion

        # region RemainInsideParent

        public static readonly DependencyProperty RemainInsideParentProperty =
            DependencyProperty.Register("RemainInsideParent", typeof(bool),
            typeof(Window), new PropertyMetadata(false));

        public bool RemainInsideParent
        {
            get
            {
                return (bool)this.GetValue(Window.RemainInsideParentProperty);
            }
            set
            {
                this.SetValue(Window.RemainInsideParentProperty, value);
            }
        }

        # endregion

        # region More

        public bool ChangeHorizontalMargins { get; set; }
        public bool ChangeVerticalMargins { get; set; }

        # endregion

        # region ApplicationModal

        private static Window _ApplicationModal;

        public static Window ApplicationModal
        {
            get
            {
                return Window._ApplicationModal;
            }
            private set
            {
                Window._ApplicationModal = value;
            }
        }

        # endregion

        # region Result

        private object _Result;

        public object Result
        {
            get
            {
                return this._Result;
            }
            set
            {
                this._Result = value;
            }
        }

        public TResult GetDialogResult<TResult>()
        {
            return (TResult)this._Result;
        }

        # endregion

        # region GripRect

        private Rect _GripRect;

        private Rect GripRect
        {
            get
            {
                if (this._GripRect.Width == 0)
                {
                    this._GripRect = new Rect(new Point(), this._Grip.RenderSize);
                }
                return this._GripRect;
            }
        }

        private void SetIsOverGrip(MouseEventArgs e)
        {
            this._IsMouseOverGrip = this._HasGrip &&
                this.GripRect.Contains(e.GetPosition(this._Grip));
        }

        # endregion

        # endregion

        # region Custom Cursors

        private FrameworkElement _CustomCursor;

        # region NESWCursor

        private FrameworkElement _NESWCursor;

        private bool _IsNESWCursorLoaded;

        private FrameworkElement NESWCursor
        {
            get
            {
                if (!this._IsNESWCursorLoaded)
                {
                    this._NESWCursor = this.GetCursorTemplate(this.NESWCursorTemplate);

                    this._IsNESWCursorLoaded = true;
                }
                return this._NESWCursor;
            }
        }

        # endregion

        # region NWSECursor

        private FrameworkElement _NWSECursor;

        private bool _IsNWSECursorLoaded;

        private FrameworkElement NWSECursor
        {
            get
            {
                if (!this._IsNWSECursorLoaded)
                {
                    this._NWSECursor = this.GetCursorTemplate(this.NWSECursorTemplate);

                    this._IsNWSECursorLoaded = true;
                }
                return this._NWSECursor;
            }
        }

        # endregion

        # region NESWCursorTemplate

        public static readonly DependencyProperty NESWCursorTemplateProperty =
            DependencyProperty.Register("NESWCursorTemplate", typeof(DataTemplate),
            typeof(Window), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate NESWCursorTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(Window.NESWCursorTemplateProperty);
            }
            set
            {
                this.SetValue(Window.NESWCursorTemplateProperty, value);
            }
        }

        # endregion

        # region NWSECursorTemplate

        public static readonly DependencyProperty NWSECursorTemplateProperty =
            DependencyProperty.Register("NWSECursorTemplate", typeof(DataTemplate),
            typeof(Window), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate NWSECursorTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(Window.NWSECursorTemplateProperty);
            }
            set
            {
                this.SetValue(Window.NWSECursorTemplateProperty, value);
            }
        }

        # endregion

        # region LoadCursorTemplate

        private FrameworkElement GetCursorTemplate(DataTemplate template)
        {
            FrameworkElement element = null;
            var cursor = template.LoadContent();

            if (cursor.IsNotNull())
            {
                if (!(cursor is FrameworkElement))
                {
                    throw new TemplateException("The CursorTemplate must be of type FrameworkElement.");
                }

                element = (FrameworkElement)cursor;
                element.Visibility = Visibility.Collapsed;
                element.HorizontalAlignment = HorizontalAlignment.Center;
                element.VerticalAlignment = VerticalAlignment.Center;
                //element.MouseEnter += (sender, e) => this.InternalMouseMove(e);
            }
            return element;
        }

        # endregion

        # region HideCustomCursor

        private void HideCustomCursor()
        {
            if (this._CustomCursor.IsNotNull())
            {
                this._CustomCursor.Visibility = Visibility.Collapsed;
                this._CustomCursor = null;
            }
        }

        # endregion

        # region SetCustomCursor

        private void SetCustomCursor(FrameworkElement cursor, Point position)
        {
            if (cursor.IsNull())
            {
                this.HideCustomCursor();
                this.Cursor = Cursors.SizeWE;
            }
            else
            {
                this.Cursor = Cursors.None;

                this.ArrangeCustomCursorPosition(cursor, position);

                cursor.Visibility = Visibility.Visible;

                this._CustomCursor = cursor;
            }
        }

        # endregion

        # region ArrangeCustomCursorPosition

        private void ArrangeCustomCursorPosition(FrameworkElement cursor, Point position)
        {
            var outerMargin = -10000D;

            var left = outerMargin;
            var top = outerMargin;
            var right = outerMargin;
            var bottom = outerMargin;

            this.ArrangeCustomCursorPosition(this._PreviewLayoutChange.HasResizeRight, ref left, ref right, this.ActualWidth, position.X);
            this.ArrangeCustomCursorPosition(this._PreviewLayoutChange.HasResizeBottom, ref top, ref bottom, this.ActualHeight, position.Y);

            cursor.Margin = new Thickness(left, top, right, bottom);
        }

        private void ArrangeCustomCursorPosition(bool isOuter, ref double offset, ref double outerOffset, double length, double position)
        {
            if (isOuter)
            {
                offset += length - (length - position) * 2;
            }
            else
            {
                outerOffset += length - position * 2;
            }
        }

        private void ArrangeCustomCursorPositionByChange(FrameworkElement cursor, Point change)
        {
            var margin = cursor.Margin;
            var left = margin.Left;
            var top = margin.Top;
            var right = margin.Right;
            var bottom = margin.Bottom;

            this.ArrangeCustomCursorPositionByChange(this._CurrentLayoutChange.HasResizeRight, ref left, ref right, change.X);
            this.ArrangeCustomCursorPositionByChange(this._CurrentLayoutChange.HasResizeBottom, ref top, ref bottom, change.Y);

            cursor.Margin = new Thickness(left, top, right, bottom);
        }

        private void ArrangeCustomCursorPositionByChange(bool isOuter, ref double offset, ref double outerOffSet, double change)
        {
            if (isOuter)
            {
                offset += change;
            }
            else
            {
                outerOffSet -= change;
            }
        }

        # endregion

        # endregion

        # region IsModalToDialogMode

        private DialogMode IsModalToDialogMode(bool isModal)
        {
            return isModal ? (this.Container.IsNull() ? DialogMode.ApplicationModal : DialogMode.Modal) : DialogMode.Default;
        }

        # endregion

        # region 

        private void ArrangeOverlayVisibility()
        {
            var overlay = this.Overlay;

            if (overlay.IsNotNull())
            {
                overlay.Visibility = this.IsModal.ToVisibility();
            }
        }

        # endregion

        # region OnContainerSizeChanged

        private void Container_SizeChanged(object sender, EventArgs e)
        {
        }
        private void OnContainerSizeChanged()
        {
            if (this.DialogMode == DialogMode.ApplicationModal)
            {
                var container = this.InternalContainerGrid;
                var content = AppUseful.Content;
                container.Width = content.ActualWidth;
                container.Height = content.ActualHeight;
            }

            var containerWidth = this.InternalContainerGrid.ActualWidth;
            var containerHeight = this.InternalContainerGrid.ActualHeight;
            var isHorizontalCentered = this.HorizontalAlignment == HorizontalAlignment.Center;
            var isVerticalCentered = this.VerticalAlignment == VerticalAlignment.Center;

            var margin = this.Margin;
            var left = margin.Left;
            var top = margin.Top;
            var right = margin.Right;
            var bottom = margin.Bottom;

            double aux = 0;

            this.ArrangeMove(containerWidth, this.ActualWidth, ref left,
                ref right, 0, true, isHorizontalCentered, ref aux);

            this.ArrangeMove(containerHeight, this.ActualHeight, ref top,
                ref bottom, 0, true, isVerticalCentered, ref aux);

            this.Margin = new Thickness(left, top, right, bottom);
        }

        # endregion

        # region SetUserLayoutChange

        private void SetUserLayoutChange()
        {
            this._CurrentLayoutChange = LayoutChange.None;

            var result = LayoutChange.None;

            var horizontalAlignment = this.HorizontalAlignment;
            var verticalAlignment = this.VerticalAlignment;

            if (this.CanDrag)
            {
                if (horizontalAlignment == HorizontalAlignment.Center)
                {
                    result |= LayoutChange.MoveHorizontal;
                }
                if (verticalAlignment == VerticalAlignment.Center)
                {
                    result |= LayoutChange.MoveVertical;
                }
            }

            if (this.CanResize)
            {
                if (horizontalAlignment.In(HorizontalAlignment.Center, HorizontalAlignment.Right))
                {
                    result |= LayoutChange.ResizeLeft;
                }
                if (verticalAlignment.In(VerticalAlignment.Center, VerticalAlignment.Bottom))
                {
                    result |= LayoutChange.ResizeTop;
                }
                if (horizontalAlignment.In(HorizontalAlignment.Left, HorizontalAlignment.Center))
                {
                    result |= LayoutChange.ResizeRight;
                }
                if (verticalAlignment.In(VerticalAlignment.Top, VerticalAlignment.Center))
                {
                    result |= LayoutChange.ResizeBottom;
                }
            }

            this._UserLayoutChange = result;
        }

        # endregion

        # region GetLayoutChangePreview

        private LayoutChange GetLayoutChangePreview(Point position)
        {
            var current = LayoutChange.None;

            if (!this.IsMaximized && !this._IsMouseOverButtonsContainer)
            {
                var resizeSideThichness = this.ResizeSize;
                var horizontalCornerSize = this.HorizontalResizeCorner;
                var verticalCornerSize = this.VerticalResizeCorner;

                var width = this.ActualWidth;
                var height = this.ActualHeight;

                if (this._IsMouseOverGrip)
                {
                    current |= LayoutChange.ResizeRight | LayoutChange.ResizeBottom;
                }
                else if (this._IsMouseOverDrag || (!this._HasDrag &&
                    position.X >= resizeSideThichness && position.X < width - resizeSideThichness &&
                    position.Y >= resizeSideThichness && position.Y < height - resizeSideThichness))
                {
                    current = LayoutChange.MoveAll;
                }
                else
                {
                    if (position.X > -3 && position.X < width + 2 && position.Y > -3 && position.Y < height + 2)
                    {
                        if (position.X < horizontalCornerSize)
                        {
                            current |= LayoutChange.ResizeLeft;
                        }
                        else if (position.X >= width - horizontalCornerSize)
                        {
                            current |= LayoutChange.ResizeRight;
                        }

                        if (position.Y < verticalCornerSize)
                        {
                            current |= LayoutChange.ResizeTop;
                        }
                        else if (position.Y >= height - verticalCornerSize)
                        {
                            current |= LayoutChange.ResizeBottom;
                        }
                    }
                }
                current &= this._UserLayoutChange; // get property changes (alignment and can x can y
            }
            return current;
        }

        # endregion

        # region LayoutChange Struct

        private struct LayoutChange
        {
            private LayoutChange(byte value)
            {
                this._Value = value;
            }

            private byte _Value;

            public static LayoutChange None = new LayoutChange(0);
            public static LayoutChange MoveHorizontal = new LayoutChange(1);
            public static LayoutChange MoveVertical = new LayoutChange(2);
            public static LayoutChange MoveAll = MoveHorizontal | MoveVertical;
            public static LayoutChange ResizeLeft = new LayoutChange(4);
            public static LayoutChange ResizeTop = new LayoutChange(8);
            public static LayoutChange ResizeTopAndLeft = ResizeTop | ResizeLeft;
            public static LayoutChange ResizeRight = new LayoutChange(16);
            public static LayoutChange ResizeTopAndRight = ResizeTop | ResizeRight;
            public static LayoutChange ResizeBottom = new LayoutChange(32);
            public static LayoutChange ResizeBottomAndLeft = ResizeBottom | ResizeLeft;
            public static LayoutChange ResizeBottomAndRight = ResizeBottom | ResizeRight;

            public bool HasMove { get { return (this._Value & 3) > 0; } }
            public bool HasResize { get { return (this._Value >> 2) > 0; } }
            public bool HasMoveHorizontal { get { return this.Has(MoveHorizontal); } }
            public bool HasMoveVertical { get { return this.Has(MoveVertical); } }
            public bool HasResizeLeft { get { return this.Has(ResizeLeft); } }
            public bool HasResizeTop { get { return this.Has(ResizeTop); } }
            public bool HasResizeRight { get { return this.Has(ResizeRight); } }
            public bool HasResizeBottom { get { return this.Has(ResizeBottom); } }
            public bool IsResizeNWOrSE { get { return this == ResizeTopAndLeft || this == ResizeBottomAndRight; } }
            public bool IsResizeNEOrSW { get { return this == ResizeTopAndRight || this == ResizeBottomAndLeft; } }
            public bool IsResizeNOrS { get { return this == ResizeTop || this == ResizeBottom; } }
            public bool IsResizeWOrE { get { return this == ResizeLeft || this == ResizeRight; } }
            public bool HasHorizontalResize { get { return HasResizeLeft || HasResizeRight; } }
            public bool HasVerticalResize { get { return HasResizeTop || HasResizeBottom; } }

            public static LayoutChange operator |(LayoutChange a, LayoutChange b)
            {
                return new LayoutChange((byte)(a._Value | b._Value));
            }
            public static LayoutChange operator &(LayoutChange a, LayoutChange b)
            {
                return new LayoutChange((byte)(a._Value & b._Value));
            }

            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }

                return ((LayoutChange)obj)._Value == this._Value;
            }

            public bool Has(LayoutChange layoutChange)
            {
                return (this | layoutChange) == this;
            }

            public override int GetHashCode()
            {
                return this._Value.GetHashCode();
            }

            public static bool operator ==(LayoutChange x, LayoutChange y)
            {
                return x.Equals(y);
            }

            public static bool operator !=(LayoutChange x, LayoutChange y)
            {
                return !x.Equals(y);
            }
        }

        # endregion

        # region SetCursor

        private void SetCursor(Point position)
        {
            var layoutChange = this._PreviewLayoutChange = this.GetLayoutChangePreview(position);

            if (layoutChange.IsResizeNWOrSE)
            {
                this.SetCustomCursor(this.NWSECursor, position);
            }
            else if (layoutChange.IsResizeNEOrSW)
            {
                this.SetCustomCursor(this.NESWCursor, position);
            }
            else if (layoutChange.IsResizeNOrS)
            {
                this.HideCustomCursor();
                this.Cursor = Cursors.SizeNS;
            }
            else if (layoutChange.IsResizeWOrE)
            {
                this.HideCustomCursor();
                this.Cursor = Cursors.SizeWE;
            }
            else
            {
                this.HideCustomCursor();
                this.Cursor = Cursors.Arrow;
            }
        }

        # endregion

        # region ArrangeBounds

        private void ArrangeBounds(Point currentPosition)
        {
            var change = currentPosition.Subtract(this._PreviousMousePosition);

            var changeX = change.X;
            var changeY = change.Y;

            var cursorChange = change;

            if (this.ArrangeCursorOffset(ref changeX, ref this._CursorOffsetX) |
                this.ArrangeCursorOffset(ref changeY, ref this._CursorOffsetY))
            {
                var margin = this.Margin;
                var left = margin.Left;
                var top = margin.Top;
                var right = margin.Right;
                var bottom = margin.Bottom;
                double width, height;
                var containerWidth = this.InternalContainerGrid.ActualWidth;
                var containerHeight = this.InternalContainerGrid.ActualHeight;
                var isHorizontalCentered = this.HorizontalAlignment == HorizontalAlignment.Center;
                var isVerticalCentered = this.VerticalAlignment == VerticalAlignment.Center;
                var current = this._CurrentLayoutChange;

                if (current.HasMove)
                {
                    width = this.ActualWidth;
                    height = this.ActualHeight;

                    this.ArrangeMove(containerWidth, width, ref left, ref right, changeX,
                        current.HasMoveHorizontal && changeX != 0, isHorizontalCentered, ref this._CursorOffsetX);

                    this.ArrangeMove(containerHeight, height, ref top, ref bottom, changeY,
                        current.HasMoveVertical && changeY != 0, isVerticalCentered, ref this._CursorOffsetY);
                }
                else
                {
                    width = this.Width;
                    height = this.Height;

                    this.ArrangeResize(containerWidth, ref width, ref left, ref right, ref changeX, current.HasHorizontalResize,
                        current.HasResizeRight, isHorizontalCentered, this.MinWidth, this.MaxWidth, ref this._CursorOffsetX);

                    this.ArrangeResize(containerHeight, ref height, ref top, ref bottom, ref changeY, current.HasVerticalResize,
                        current.HasResizeBottom, isVerticalCentered, this.MinHeight, this.MaxHeight, ref this._CursorOffsetY);

                    this.Width = width;
                    this.Height = height;

                    this.ArrangeMargins(this.ChangeHorizontalMargins, ref left, ref right, this.Margin.Left, this.Margin.Right);
                    this.ArrangeMargins(this.ChangeVerticalMargins, ref top, ref bottom, this.Margin.Top, this.Margin.Bottom);

                }
                this.Margin = new Thickness(left, top, right, bottom);
            }

            if (this._CustomCursor.IsNotNull())
            {
                cursorChange.X += (cursorChange.X - changeX);
                cursorChange.Y += (cursorChange.Y - changeY);

                this.ArrangeCustomCursorPositionByChange(this._CustomCursor, cursorChange);
            }
        }

        private void ArrangeMargins(bool changeMargins, ref double offset, ref double outerOffSet, double offsetValue, double outerOffsetValue)
        {
            if (!changeMargins)
            {
                offset = offsetValue;
                outerOffSet = outerOffsetValue;
            }
        }

        private bool ArrangeCursorOffset(ref double change, ref double offset)
        {
            if (Math.Abs(change) > Math.Abs(offset))
            {
                change += offset;
                offset = 0;
                return true;
            }
            else
            {
                offset += change;
                change = 0;
                return false;
            }
        }

        private void ArrangeMove(
            double containerLength, double length, ref double offset, ref double outerOffset,
            double change, bool condition, bool isCentered, ref double cursorOffset)
        {
            if (condition)
            {
                change += this.CheckBoundsConstraints(containerLength, length,
                    offset + change, outerOffset - change, isCentered, ref cursorOffset);

                this.ComplementChange(ref offset, ref outerOffset, change);
            }
        }

        private void ArrangeResize(
            double containerLength, ref double length, ref double offset, ref double outerOffset,
            ref double change, bool hasResize, bool isOuter, bool isCentered, double min, double max, ref double cursorOffset)
        {
            if (change != 0 && hasResize)
            {
                if (isOuter)
                {
                    change += this.CheckBoundsConstraints(containerLength, length + change,
                        offset, outerOffset - change, isCentered, ref cursorOffset);

                    change += this.CheckMinMaxConstraints(length + change, min, max, ref cursorOffset, isOuter);

                    this.ComplementChange(ref length, ref outerOffset, change);
                }
                else
                {
                    change += this.CheckBoundsConstraints(containerLength, length - change,
                        offset + change, outerOffset, isCentered, ref cursorOffset);

                    change -= this.CheckMinMaxConstraints(length - change, min, max, ref cursorOffset, isOuter);

                    this.ComplementChange(ref offset, ref length, change);
                }
            }
        }

        private double CheckBoundsConstraints(double containerLength, double length,
            double offset, double outerOffset, bool isCentered, ref double cursorOffset)
        {
            double diff = 0;
            if (this.RemainInsideParent)
            {
                var space = containerLength - length;
                if (isCentered)
                {
                    diff -= Math.Min(0, (space + offset - outerOffset) / 2);
                    diff += Math.Min(0, (space - offset + outerOffset) / 2);

                    if (diff % 1 < 0) // todo: discover from which elements UseLayoutRounding property is causing relocation
                    {
                        diff -= 0.5;
                    }
                }
                else
                {
                    diff -= Math.Min(0, space - outerOffset);
                    diff += Math.Min(0, space - offset);
                }
                cursorOffset -= diff;
            }
            return diff;
        }

        private double CheckMinMaxConstraints(double length, double min, double max, ref double cursorOffset, bool isOuter)
        {
            double diff = 0;

            diff += Math.Min(0, max - length);
            diff -= Math.Min(0, length - min);

            cursorOffset += isOuter ? -diff : diff;

            return diff;
        }

        private void ComplementChange(ref double complementA, ref double complementB, double change)
        {
            complementA += change;
            complementB -= change;
        }

        # endregion

        # region OnMouseEnter

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.InternalMouseMove(e);
        }

        # endregion

        # region MouseMove

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.InternalMouseMove(e);
        }

        private void InternalMouseMove(MouseEventArgs e)
        {
            if (this.CanDrag || this.CanResize)
            {
                var position = e.GetPosition(this);
                if (this._CurrentLayoutChange == LayoutChange.None)
                {
                    this.SetIsOverGrip(e);

                    this.SetCursor(position);
                }
                else
                {
                    position = e.GetPosition(AppUseful.RootVisual);

                    if (this._PreviousMousePosition != position)
                    {
                        this.ArrangeBounds(position);

                        this._PreviousMousePosition = position;
                    }
                }
            }
        }

        # endregion

        # region MouseDown

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (this._PreviewLayoutChange != LayoutChange.None)
            {
                var preview = this._PreviewLayoutChange;

                if (preview.HasHorizontalResize && double.IsNaN(this.Width))
                {
                    this.Width = this.ActualWidth;
                }
                if (preview.HasVerticalResize && double.IsNaN(this.Height))
                {
                    this.Height = this.ActualHeight;
                }

                this._CurrentLayoutChange = preview;
                this._PreviousMousePosition = e.GetPosition(AppUseful.RootVisual);

                this.CaptureMouse();
            }

            this.Focus();
        }

        # endregion

        # region OnMouseLeftButtonUp

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (this._CurrentLayoutChange != LayoutChange.None)
            {
                e.Handled = true;

                this._CursorOffsetX = this._CursorOffsetY = 0;

                this.ReleaseMouseCapture();
                this._CurrentLayoutChange = LayoutChange.None;

                this.InternalMouseMove(e);
            }
        }

        # endregion

        # region OnMouseLeave

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (this._CurrentLayoutChange == LayoutChange.None)
            {
                this.Cursor = Cursors.Arrow;
                this.HideCustomCursor();
            }
        }

        # endregion

        # region OnGotFocus

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            this.TryBringToFront();
        }

        # endregion

        # region TryBringToFront

        private void TryBringToFront()
        {
            this.InternalContainerGrid.TryBringToFront();
        }

        # endregion

        # region SetParent

        private void SetParent()
        {
            var container = this.Container;
            var content = this.InternalContainerGrid;

            var property = container.GetContentProperty();

            var value = property.GetValue(container, null);

            if (typeof(IList).IsInstanceOfType(value))
            {
                var list = (IList)value;
                list.Add(content);

                this._RemoveFromParent = () => list.Remove(content);
            }
            else
            {
                try
                {
                    property.SetValue(container, content, null);

                    this._RemoveFromParent = () => property.SetValue(container, null, null);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        "无法设置内容属性‘窗体’属性的对象。", exception);
                }
            }
        }
        private void SetParent(bool IsPanel)
        {
            var container = this.Container;
            var content = this.InternalContainerGrid;

            //var property = container.GetContentProperty();

            //var value = property.GetValue(container, null);
            if(typeof(Grid).IsInstanceOfType(container))
            {
                Grid parent=this.Container as Grid;
                Canvas.SetZIndex(content, 10);
                parent.Children.Add(content);
                this._RemoveFromParent = () => parent.Children.Remove(content);
            }
            //Grid parent=this.Container as Grid
            //if (typeof(IList).IsInstanceOfType(value))
            //{
            //    var list = (IList)value;
            //    list.Add(content);

            //    this._RemoveFromParent = () => list.Remove(content);
            //}
            //else
            //{
            //    try
            //    {
            //        property.SetValue(container, content, null);

            //        this._RemoveFromParent = () => property.SetValue(container, null, null);
            //    }
            //    catch (Exception exception)
            //    {
            //        throw new InvalidOperationException(
            //            "无法设置内容属性‘窗体’属性的对象。", exception);
            //    }
            //}
        }
        # endregion

        # region Show

        # region Overloads

        public void Show()
        {
            this.Show(this.Container.IsNull());
        }
        public void Show(Action closed)
        {
            this.Show(this.Container.IsNull(), closed);
        }
        public void Show<TResult>(TResult startValue, Action<TResult> closed)
        {
            this.Show(this.Container.IsNull(), startValue, closed);
        }
        public void Show(FrameworkElement container)
        {
            this.Show(this.IsModal, container);
        }
        public void Show(FrameworkElement container, Action closed)
        {
            this.Show(this.IsModal, container, closed);
        }
        public void Show<TResult>(FrameworkElement container, TResult startValue, Action<TResult> closed)
        {
            this.Show(this.IsModal, container, startValue, closed);
        }
        public void ShowDialog()
        {
            this.Show(true);
        }
        public void ShowDialog(Action closed)
        {
            this.Show(true, closed);
        }
        public void ShowDialog<TResult>(TResult startValue, Action<TResult> closed)
        {
            this.Show(true, startValue, closed);
        }
        public void ShowDialog(WindowsContainer container)
        {
            this.Show(true, container);
        }
        public void ShowDialog(WindowsContainer container, Action closed)
        {
            this.Show(true, container, closed);
        }
        public void ShowDialog<TResult>(WindowsContainer container, TResult startValue, Action<TResult> closed)
        {
            this.Show(true, container, startValue, closed);
        }
        public void Show(bool isModal)
        {
            this.Show(this.IsModalToDialogMode(isModal));
        }
        public void Show(bool isModal, Action closed)
        {
            this.Show(this.IsModalToDialogMode(isModal), closed);
        }
        public void Show<TResult>(bool isModal, TResult startValue, Action<TResult> closed)
        {
            this.Show(this.IsModalToDialogMode(isModal), startValue, closed);
        }
        public void Show(bool isModal, FrameworkElement container)
        {
            this.Show(isModal ? DialogMode.Modal : DialogMode.Default, container);
        }
        public void Show(bool isModal, FrameworkElement container, Action closed)
        {
            this.Show(isModal ? DialogMode.Modal : DialogMode.Default, container, closed);
        }
        public void Show<TResult>(bool isModal, FrameworkElement container, TResult startValue, Action<TResult> closed)
        {
            this.Show(isModal ? DialogMode.Modal : DialogMode.Default, container, startValue, closed);
        }
        public void Show(DialogMode dialogMode)
        {
            this.Show(dialogMode, this.Container);
        }
        public void Show(DialogMode dialogMode, Action closed)
        {
            this.Show(dialogMode, this.Container, closed);
        }
        public void Show<TResult>(DialogMode dialogMode, TResult startValue, Action<TResult> closed)
        {
            this.Show(dialogMode, this.Container, startValue, closed);
        }
        public void Show(DialogMode dialogMode, FrameworkElement container)
        {
            this.InternalShow(dialogMode, container, null);
        }
        public void Show(DialogMode dialogMode, FrameworkElement container, Action closed)
        {
            this.InternalShow(dialogMode, container, closed);
        }
        public void Show<TResult>(DialogMode dialogMode, FrameworkElement container, TResult startValue, Action<TResult> closed)
        {
            this.Result = startValue;

            this.InternalShow(dialogMode, container, closed);
        }

        # endregion

        private void InternalShow(DialogMode dialogMode, FrameworkElement container, Delegate closed)
        {
            //获取容器
            this.Container = container;
            //获取窗口模式
            this.DialogMode = dialogMode;

            this.ArrangeOverlayVisibility();
            this.SetUserLayoutChange();

            if (dialogMode == DialogMode.ApplicationModal)
            {
                if (Window.ApplicationModal.IsNotNull())
                {
                   throw new InvalidOperationException(
                        "已经有一个应用模式窗口打开。");
               }
                Window.ApplicationModal = this;

               this.OnContainerSizeChanged();
            //    //this._ContentSizeChangedDisposable = AppUseful.Content.GetEvent("Resized").Subscribe(this.OnContainerSizeChanged);
                AppUseful.Content.Resized += this.Container_SizeChanged;
                this._UnsubscribeContainerSizeChanged = () => AppUseful.Content.Resized -= this.Container_SizeChanged;

                this.Popup.IsOpen = true;

                AppUseful.GetRootVisual<Control>().IsEnabled = false;
            }
            else
            {
             if (container.IsNull())
               {
                   throw new NotImplementedException(
                      "要显示一个非模态窗口的应用程序，您必须设置‘窗体’属性或参数。");
            }

                //this._ContentSizeChangedDisposable = container.GetSizeChanged().Subscribe(this.OnContainerSizeChanged);
                container.SizeChanged += this.Container_SizeChanged;
                this._UnsubscribeContainerSizeChanged = () => container.SizeChanged -= this.Container_SizeChanged;

                var isModal = dialogMode == DialogMode.Modal;

                var windowsContainer = container as WindowsContainer;

                if (windowsContainer == null)
                {
                    if (isModal)
                    {
                        throw new InvalidOperationException(
                           "要显示一个模式窗口中，您必须设置‘窗体’属性或参数为WindowsContainer对象。");
                   }
                    //this.SetParent();
                   SetParent(true);
               }
                else
                {
                   windowsContainer.ShowWindow(this);

                  this._RemoveFromParent = () =>
                    {
                       windowsContainer.CloseWindow(this);
                    };
                }
            }

            this._IsOpened = true;
            this.Focus();
            this.TryBringToFront();

            this._Closed = closed;
                
            //Popup p = new Popup();
            //p.Child = this.InternalContainerGrid;
            //p.IsOpen = true;
            //this._RemoveFromParent = () => p.IsOpen=false;
        }

        # endregion

        # region Close

        public void Close<TResult>(TResult result)
        {
            this.Close((object)result);
        }
        public void Close(object result)
        {
            this._Result = result;
            this.Close();
        }
        public void Close()
        {
            //this._ContentSizeChangedDisposable.Dispose();
            this._UnsubscribeContainerSizeChanged();

            if (this.DialogMode == DialogMode.ApplicationModal)
            {
                this.Popup.IsOpen = false;

                AppUseful.GetRootVisual<Control>().IsEnabled = true;
                Window.ApplicationModal = null;

            }
            else
            {
                if (this._RemoveFromParent.IsNotNull())
                {
                    this._RemoveFromParent();
                }
            }

            this._IsOpened = false;
            this.OnClosed();
        }

        # endregion

        # region ToggleMaximize

        public void ToggleMaximize()
        {
            this.IsMaximized = !this.IsMaximized;
        }

        # endregion

        # region OnClosed

        private void OnClosed()
        {
            if (this._Closed.IsNotNull())
            {
                var result = this._Result;
                var length = this._Closed.Method.GetParameters().Length;

                if (length == 0)
                {
                    this._Closed.DynamicInvoke();
                }
                else if (result.IsNull())
                {
                    throw new InvalidOperationException("The \"Result\" was not setted.");
                }
                else
                {
                    this._Closed.DynamicInvoke(result);
                }
            }

            var handler = this.Closed;

            if (handler.IsNotNull())
            {
                handler(this, EventArgs.Empty);
            }
        }

        # endregion

        # region OnApplyTemplate

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var rootGrid = this.GetTemplateChild(Window.RootGridName) as Grid;

            if (rootGrid.IsNotNull())
            {
                this.TryInsertCursor(rootGrid, this.NESWCursor);
                this.TryInsertCursor(rootGrid, this.NWSECursor);
            }

            this._HasGrip = (this._Grip = this.GetTemplateChild(Window.GripName) as UIElement).IsNotNull();

            this._HasDrag = this.TryRegisterMouseContent(Window.DragName, b => this._IsMouseOverDrag = b);
            this.TryRegisterMouseContent(Window.CommandButtonsContainerName, b => this._IsMouseOverButtonsContainer = b);

            this.SetUserLayoutChange();
        }

        private void TryInsertCursor(Grid grid, FrameworkElement cursor)
        {
            if (cursor.IsNotNull())
            {
                Grid.SetColumnSpan(cursor, Math.Max(1, grid.ColumnDefinitions.Count));
                Grid.SetRowSpan(cursor, Math.Max(1, grid.RowDefinitions.Count));
                grid.Children.Add(cursor);
            }
        }

        private bool TryRegisterMouseContent(string templatePartName, Action<bool> action)
        {
            var element = this.GetTemplateChild(templatePartName) as UIElement;

            if (element.IsNotNull())
            {
                //element.GetMouseEnter().Subscribe(() => action(true));
                element.MouseEnter += (sender, e) => action(true);
                //element.GetMouseLeave().Subscribe(() => action(false));
                element.MouseLeave += (sender, e) => action(false);
                return true;
            }

            return false;
        }

        # endregion

        # region OnLoaded

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.InternalContainerGrid.Children.Count == 1)
            {
                this._IsOverlayLoaded = false;
                this.ArrangeOverlayVisibility();
                var overlay = this.Overlay;

                if (overlay.IsNotNull())
                {
                    this.InternalContainerGrid.Children.Insert(0, overlay);
                }
            }
        }

        # endregion
    }

    # region DialogMode Enum

    public enum DialogMode
    {
        Default,
        Modal,
        ApplicationModal
    }

    # endregion
}
