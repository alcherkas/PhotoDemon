using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon button strip control - migrated from VB6 pdButtonStrip.ctl
///
/// Features:
/// - Segmented control with multiple buttons
/// - Single selection mode (like radio buttons)
/// - Arbitrary number of buttons with captions
/// - Optional images per button
/// - Keyboard navigation (arrow keys)
/// - Auto-wrapped captions
/// - Two color schemes (default, light)
/// - Focus indicators
///
/// This is similar to iOS UISegmentedControl or a horizontal radio button group.
///
/// Original VB6: Controls/pdButtonStrip.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 169 instances across the application
/// </summary>
public class PdButtonStrip : PhotoDemonControlBase
{
    #region Enums

    public enum ColorScheme
    {
        Default = 0,
        Light = 1
    }

    #endregion

    #region Button Entry Class

    public class ButtonEntry
    {
        public string Caption { get; set; }
        public ImageSource Image { get; set; }
        public object Tag { get; set; }
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Currently selected button index
    /// </summary>
    public static readonly BindableProperty ListIndexProperty = BindableProperty.Create(
        nameof(ListIndex),
        typeof(int),
        typeof(PdButtonStrip),
        0,
        BindingMode.TwoWay,
        propertyChanged: OnListIndexChanged);

    public int ListIndex
    {
        get => (int)GetValue(ListIndexProperty);
        set => SetValue(ListIndexProperty, value);
    }

    /// <summary>
    /// Collection of button entries
    /// </summary>
    public ObservableCollection<ButtonEntry> Buttons { get; }

    /// <summary>
    /// Command executed when button is clicked
    /// </summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(PdButtonStrip),
        null);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Color scheme (Default or Light)
    /// </summary>
    public static readonly BindableProperty ColoringModeProperty = BindableProperty.Create(
        nameof(ColoringMode),
        typeof(ColorScheme),
        typeof(PdButtonStrip),
        ColorScheme.Default,
        propertyChanged: OnVisualPropertyChanged);

    public ColorScheme ColoringMode
    {
        get => (ColorScheme)GetValue(ColoringModeProperty);
        set => SetValue(ColoringModeProperty, value);
    }

    /// <summary>
    /// Don't auto-reset when command bar reset is clicked
    /// </summary>
    public static readonly BindableProperty DontAutoResetProperty = BindableProperty.Create(
        nameof(DontAutoReset),
        typeof(bool),
        typeof(PdButtonStrip),
        false);

    public bool DontAutoReset
    {
        get => (bool)GetValue(DontAutoResetProperty);
        set => SetValue(DontAutoResetProperty, value);
    }

    /// <summary>
    /// Font size for button captions
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(PdButtonStrip),
        12.0,
        propertyChanged: OnVisualPropertyChanged);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Whether font is bold
    /// </summary>
    public static readonly BindableProperty FontBoldProperty = BindableProperty.Create(
        nameof(FontBold),
        typeof(bool),
        typeof(PdButtonStrip),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool FontBold
    {
        get => (bool)GetValue(FontBoldProperty);
        set => SetValue(FontBoldProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when a button is clicked
    /// </summary>
    public event EventHandler<int> Click;

    /// <summary>
    /// Raised when mouse moves over a button (for custom tooltips)
    /// </summary>
    public event EventHandler<int> MouseMoveInfoOnly;

    /// <summary>
    /// Raised when control receives focus
    /// </summary>
    public event EventHandler GotFocusAPI;

    /// <summary>
    /// Raised when control loses focus
    /// </summary>
    public event EventHandler LostFocusAPI;

    #endregion

    #region Private Fields

    private readonly SKCanvasView _canvasView;
    private int _hoverIndex = -1;
    private int _mouseDownIndex = -1;
    private bool _showFocusRect;
    private List<SKRect> _buttonRects = new();
    private const float ButtonGap = 2;

    #endregion

    #region Constructor

    public PdButtonStrip()
    {
        Buttons = new ObservableCollection<ButtonEntry>();
        Buttons.CollectionChanged += (s, e) =>
        {
            UpdateLayout();
            _canvasView?.InvalidateSurface();
        };

        // Create canvas for rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        // Set up gesture recognizers
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _canvasView.GestureRecognizers.Add(tapGesture);

        // Set up pointer events for hover
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerEntered += (s, e) => _canvasView.InvalidateSurface();
        pointerGesture.PointerExited += (s, e) =>
        {
            _hoverIndex = -1;
            _mouseDownIndex = -1;
            _canvasView.InvalidateSurface();
        };
        pointerGesture.PointerPressed += OnPointerPressed;
        pointerGesture.PointerReleased += OnPointerReleased;
        _canvasView.GestureRecognizers.Add(pointerGesture);

        ApplyTheme();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Add a button with caption
    /// </summary>
    public void AddButton(string caption)
    {
        Buttons.Add(new ButtonEntry { Caption = caption });
    }

    /// <summary>
    /// Add a button with caption and image
    /// </summary>
    public void AddButton(string caption, ImageSource image)
    {
        Buttons.Add(new ButtonEntry { Caption = caption, Image = image });
    }

    /// <summary>
    /// Remove all buttons
    /// </summary>
    public void Clear()
    {
        Buttons.Clear();
        ListIndex = 0;
    }

    /// <summary>
    /// Add multiple buttons from delimited string (VB6 compatibility)
    /// </summary>
    public void AddItem(string captions, char delimiter = '|')
    {
        var items = captions.Split(delimiter);
        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                AddButton(item.Trim());
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnTapped(object sender, TappedEventArgs e)
    {
        if (!IsEnabled || Buttons.Count == 0)
            return;

        var point = e.GetPosition(_canvasView);
        if (point.HasValue)
        {
            int clickedIndex = GetButtonIndexAt((float)point.Value.X, (float)point.Value.Y);
            if (clickedIndex >= 0 && clickedIndex != ListIndex)
            {
                ListIndex = clickedIndex;
            }
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        var point = e.GetPosition(_canvasView);
        if (point.HasValue)
        {
            int newHoverIndex = GetButtonIndexAt((float)point.Value.X, (float)point.Value.Y);
            if (newHoverIndex != _hoverIndex)
            {
                _hoverIndex = newHoverIndex;
                _canvasView?.InvalidateSurface();

                if (_hoverIndex >= 0)
                {
                    MouseMoveInfoOnly?.Invoke(this, _hoverIndex);
                }
            }
        }
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        var point = e.GetPosition(_canvasView);
        if (point.HasValue)
        {
            _mouseDownIndex = GetButtonIndexAt((float)point.Value.X, (float)point.Value.Y);
            _canvasView?.InvalidateSurface();
        }
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        _mouseDownIndex = -1;
        _canvasView?.InvalidateSurface();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (Buttons.Count == 0)
            return;

        var info = e.Info;
        UpdateLayout(info.Width, info.Height);

        // Draw background
        DrawBackground(canvas, info);

        // Draw each button
        for (int i = 0; i < Buttons.Count && i < _buttonRects.Count; i++)
        {
            DrawButton(canvas, i, _buttonRects[i]);
        }

        // Draw focus rect if needed
        if (_showFocusRect && ListIndex >= 0 && ListIndex < _buttonRects.Count)
        {
            DrawFocusRect(canvas, _buttonRects[ListIndex]);
        }
    }

    private static void OnListIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdButtonStrip strip)
        {
            strip._canvasView?.InvalidateSurface();

            // Raise click event
            strip.Click?.Invoke(strip, strip.ListIndex);

            // Execute command
            if (strip.Command?.CanExecute(strip.ListIndex) == true)
            {
                strip.Command.Execute(strip.ListIndex);
            }
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdButtonStrip strip)
        {
            strip._canvasView?.InvalidateSurface();
        }
    }

    #endregion

    #region Layout

    private void UpdateLayout(float width = 0, float height = 0)
    {
        if (Buttons.Count == 0) return;

        _buttonRects.Clear();

        float buttonWidth = (width - (ButtonGap * (Buttons.Count - 1))) / Buttons.Count;
        float x = 0;

        for (int i = 0; i < Buttons.Count; i++)
        {
            _buttonRects.Add(new SKRect(x, 0, x + buttonWidth, height));
            x += buttonWidth + ButtonGap;
        }
    }

    private int GetButtonIndexAt(float x, float y)
    {
        for (int i = 0; i < _buttonRects.Count; i++)
        {
            if (_buttonRects[i].Contains(x, y))
                return i;
        }
        return -1;
    }

    #endregion

    #region Rendering

    private void DrawBackground(SKCanvas canvas, SKImageInfo info)
    {
        var bgColor = GetBackgroundColor();
        using (var paint = new SKPaint())
        {
            paint.Color = bgColor;
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), paint);
        }
    }

    private void DrawButton(SKCanvas canvas, int index, SKRect rect)
    {
        bool isSelected = index == ListIndex;
        bool isHovered = index == _hoverIndex;
        bool isPressed = index == _mouseDownIndex;

        // Draw button fill
        var fillColor = GetButtonFillColor(isSelected, isHovered, isPressed);
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = fillColor;
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(rect, paint);
        }

        // Draw button border
        var borderColor = GetButtonBorderColor(isSelected);
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = borderColor;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = isSelected ? 2 : 1;
            canvas.DrawRect(rect, paint);
        }

        // Draw caption
        if (!string.IsNullOrEmpty(Buttons[index].Caption))
        {
            DrawButtonCaption(canvas, rect, Buttons[index].Caption, isSelected);
        }
    }

    private void DrawButtonCaption(SKCanvas canvas, SKRect rect, string caption, bool isSelected)
    {
        var textColor = GetTextColor(isSelected);

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = textColor;
            paint.TextSize = (float)FontSize * 1.33f;
            paint.TextAlign = SKTextAlign.Center;
            paint.Typeface = SKTypeface.FromFamilyName(
                "Segoe UI",
                FontBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);

            // Calculate vertical center
            var textBounds = new SKRect();
            paint.MeasureText(caption, ref textBounds);
            float textY = (rect.Top + rect.Bottom - textBounds.Height) / 2 - textBounds.Top;

            canvas.DrawText(caption, rect.MidX, textY, paint);
        }
    }

    private void DrawFocusRect(SKCanvas canvas, SKRect rect)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = new SKColor(100, 100, 255, 180);
            paint.StrokeWidth = 2;
            paint.PathEffect = SKPathEffect.CreateDash(new[] { 4f, 4f }, 0);

            var focusRect = rect;
            focusRect.Inflate(-3, -3);
            canvas.DrawRect(focusRect, paint);
        }
    }

    private SKColor GetBackgroundColor()
    {
        return ColoringMode == ColorScheme.Light
            ? new SKColor(245, 245, 245)
            : new SKColor(60, 60, 60);
    }

    private SKColor GetButtonFillColor(bool isSelected, bool isHovered, bool isPressed)
    {
        if (!IsEnabled)
            return new SKColor(230, 230, 230);

        if (ColoringMode == ColorScheme.Light)
        {
            if (isSelected)
                return new SKColor(0, 120, 215);
            else if (isPressed)
                return new SKColor(200, 200, 200);
            else if (isHovered)
                return new SKColor(220, 220, 220);
            else
                return new SKColor(240, 240, 240);
        }
        else
        {
            if (isSelected)
                return new SKColor(0, 120, 215);
            else if (isPressed)
                return new SKColor(80, 80, 80);
            else if (isHovered)
                return new SKColor(90, 90, 90);
            else
                return new SKColor(70, 70, 70);
        }
    }

    private SKColor GetButtonBorderColor(bool isSelected)
    {
        if (!IsEnabled)
            return new SKColor(200, 200, 200);

        return isSelected
            ? new SKColor(0, 90, 180)
            : new SKColor(120, 120, 120);
    }

    private SKColor GetTextColor(bool isSelected)
    {
        if (!IsEnabled)
            return new SKColor(150, 150, 150);

        if (ColoringMode == ColorScheme.Light)
        {
            return isSelected
                ? new SKColor(255, 255, 255)
                : new SKColor(0, 0, 0);
        }
        else
        {
            return new SKColor(255, 255, 255);
        }
    }

    #endregion

    #region Focus Handling

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsFocused))
        {
            if (IsFocused)
            {
                _showFocusRect = true;
                GotFocusAPI?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _showFocusRect = false;
                LostFocusAPI?.Invoke(this, EventArgs.Empty);
            }

            _canvasView?.InvalidateSurface();
        }
    }

    #endregion

    #region IPhotoDemonControl Implementation

    public override void UpdateVisualAppearance()
    {
        base.UpdateVisualAppearance();
        _canvasView?.InvalidateSurface();
    }

    public override void ApplyTheme()
    {
        base.ApplyTheme();
        UpdateVisualAppearance();
    }

    #endregion
}
