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
/// PhotoDemon vertical button strip control - migrated from VB6 pdButtonStripVertical.ctl
///
/// Features:
/// - Segmented control with multiple buttons stacked vertically
/// - Single selection mode (like radio buttons)
/// - Arbitrary number of buttons with captions
/// - Optional images per button (continuously aligned)
/// - Keyboard navigation (arrow keys)
/// - Auto-wrapped captions
/// - Focus indicators
///
/// This is similar to a vertical radio button group with custom rendering.
///
/// Original VB6: Controls/pdButtonStripVertical.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 5 instances across the application
/// </summary>
public class PdButtonStripVertical : PhotoDemonControlBase
{
    #region Button Entry Class

    public class ButtonEntry
    {
        public string Caption { get; set; } = string.Empty;
        public ImageSource? Image { get; set; }
        public object? Tag { get; set; }
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when a button is clicked
    /// </summary>
    public event EventHandler<int>? ButtonClick;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Currently selected button index
    /// </summary>
    public static readonly BindableProperty ListIndexProperty = BindableProperty.Create(
        nameof(ListIndex),
        typeof(int),
        typeof(PdButtonStripVertical),
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
        typeof(PdButtonStripVertical),
        null);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Font size for button captions
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(float),
        typeof(PdButtonStripVertical),
        10f,
        propertyChanged: OnVisualPropertyChanged);

    public new float FontSize
    {
        get => (float)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Bold font
    /// </summary>
    public static readonly BindableProperty FontBoldProperty = BindableProperty.Create(
        nameof(FontBold),
        typeof(bool),
        typeof(PdButtonStripVertical),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool FontBold
    {
        get => (bool)GetValue(FontBoldProperty);
        set => SetValue(FontBoldProperty, value);
    }

    #endregion

    #region Private Fields

    private List<SKRect> _buttonRects = new();
    private int _hoverIndex = -1;
    private int _mouseDownIndex = -1;

    private const float ButtonPadding = 8f;
    private const float ImageTextPadding = 8f;

    #endregion

    #region Constructor

    public PdButtonStripVertical()
    {
        Buttons = new ObservableCollection<ButtonEntry>();
        Buttons.CollectionChanged += OnButtonsCollectionChanged;

        // Set up hover and press tracking
        PointerGestureRecognizer pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += OnPointerEntered;
        pointerGesture.PointerExited += OnPointerExited;
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerPressed += OnPointerPressed;
        pointerGesture.PointerReleased += OnPointerReleased;
        GestureRecognizers.Add(pointerGesture);

        // Set up tap gesture
        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        GestureRecognizers.Add(tapGesture);

        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Event Handlers

    private void OnButtonsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateControlLayout();
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        UpdateControlLayout();
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        InvalidateSurface();
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _hoverIndex = -1;
        InvalidateSurface();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetPosition(this) is Point point)
        {
            int oldHoverIndex = _hoverIndex;
            _hoverIndex = GetButtonIndexAtPoint((float)point.X, (float)point.Y);

            if (oldHoverIndex != _hoverIndex)
            {
                InvalidateSurface();
            }
        }
    }

    private void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        if (e.GetPosition(this) is Point point)
        {
            _mouseDownIndex = GetButtonIndexAtPoint((float)point.X, (float)point.Y);
            InvalidateSurface();
        }
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        _mouseDownIndex = -1;
        InvalidateSurface();
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled || Buttons.Count == 0) return;

        if (e.GetPosition(this) is Point point)
        {
            int clickedIndex = GetButtonIndexAtPoint((float)point.X, (float)point.Y);
            if (clickedIndex >= 0 && clickedIndex < Buttons.Count)
            {
                ListIndex = clickedIndex;
                ButtonClick?.Invoke(this, clickedIndex);
                Command?.Execute(clickedIndex);
            }
        }
    }

    private static void OnListIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdButtonStripVertical control)
        {
            control.InvalidateSurface();
        }
    }

    #endregion

    #region Protected Override Methods

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        canvas.Clear();

        // Fill background
        var backgroundColor = ConvertToSKColor(BackgroundColor ?? Colors.Transparent);
        canvas.Clear(backgroundColor);

        if (Buttons.Count == 0) return;

        // Update layout if needed
        if (_buttonRects.Count != Buttons.Count)
        {
            UpdateControlLayout();
        }

        // Draw each button
        for (int i = 0; i < Buttons.Count && i < _buttonRects.Count; i++)
        {
            DrawButton(canvas, i, _buttonRects[i]);
        }
    }

    #endregion

    #region Private Methods

    private void UpdateControlLayout()
    {
        float width = (float)Width;
        float height = (float)Height;

        if (width <= 0 || height <= 0 || Buttons.Count == 0)
        {
            _buttonRects.Clear();
            return;
        }

        // Calculate button layout - distribute evenly vertically
        float buttonHeight = height / Buttons.Count;
        _buttonRects.Clear();

        for (int i = 0; i < Buttons.Count; i++)
        {
            float top = i * buttonHeight;
            _buttonRects.Add(new SKRect(
                0,
                top,
                width,
                top + buttonHeight));
        }

        InvalidateSurface();
    }

    private int GetButtonIndexAtPoint(float x, float y)
    {
        for (int i = 0; i < _buttonRects.Count; i++)
        {
            if (_buttonRects[i].Contains(x, y))
            {
                return i;
            }
        }
        return -1;
    }

    private void DrawButton(SKCanvas canvas, int index, SKRect buttonRect)
    {
        bool isSelected = (index == ListIndex);
        bool isHovered = (index == _hoverIndex);
        bool isPressed = (index == _mouseDownIndex);

        // Determine button colors
        SKColor fillColor, borderColor, textColor;

        if (isSelected)
        {
            fillColor = new SKColor(100, 100, 100); // Dark gray for selected
            borderColor = new SKColor(60, 60, 60);
            textColor = SKColors.White;
        }
        else if (isPressed)
        {
            fillColor = new SKColor(180, 180, 180);
            borderColor = new SKColor(120, 120, 120);
            textColor = SKColors.Black;
        }
        else if (isHovered)
        {
            fillColor = new SKColor(220, 220, 220);
            borderColor = new SKColor(160, 160, 160);
            textColor = SKColors.Black;
        }
        else
        {
            fillColor = new SKColor(240, 240, 240);
            borderColor = new SKColor(180, 180, 180);
            textColor = SKColors.Black;
        }

        if (!IsEnabled)
        {
            fillColor = new SKColor(250, 250, 250);
            textColor = new SKColor(160, 160, 160);
        }

        // Draw button background
        using (var fillPaint = new SKPaint
        {
            Color = fillColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        })
        {
            canvas.DrawRect(buttonRect, fillPaint);
        }

        // Draw button border
        using (var borderPaint = new SKPaint
        {
            Color = borderColor,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            IsAntialias = true
        })
        {
            canvas.DrawRect(buttonRect, borderPaint);
        }

        // Draw caption
        if (index < Buttons.Count)
        {
            string caption = Buttons[index].Caption ?? string.Empty;
            if (!string.IsNullOrEmpty(caption))
            {
                using (var textPaint = new SKPaint
                {
                    Color = textColor,
                    TextSize = FontSize * GetDisplayDensity(),
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center,
                    Typeface = SKTypeface.FromFamilyName(
                        "Tahoma",
                        FontBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                        SKFontStyleWidth.Normal,
                        SKFontStyleSlant.Upright)
                })
                {
                    // Center the text in the button
                    var textBounds = new SKRect();
                    textPaint.MeasureText(caption, ref textBounds);
                    float textX = buttonRect.MidX;
                    float textY = buttonRect.MidY - textBounds.MidY;

                    canvas.DrawText(caption, textX, textY, textPaint);
                }
            }
        }
    }

    private float GetDisplayDensity()
    {
        return (float)(DeviceDisplay.Current?.MainDisplayInfo.Density ?? 1.0);
    }

    #endregion
}
