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
/// PhotoDemon owner-drawn strip control - migrated from VB6 pdStrip.ctl
///
/// Features:
/// - Similar to pdButtonStrip but with owner-drawn button content
/// - Single selection mode (like radio buttons)
/// - Arbitrary number of buttons with custom rendering
/// - DrawButton event for owner-drawn content
/// - Keyboard navigation (arrow keys)
/// - Hover and selection states
/// - Used primarily for theme accent color selection
///
/// This control is similar to pdButtonStrip, except button entries are owner-drawn.
/// The rendering techniques have been tweaked to work better with unpredictable strip contents.
///
/// Original VB6: Controls/pdStrip.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdStrip : PhotoDemonControlBase
{
    #region Button Entry Class

    public class StripButton
    {
        public string Data { get; set; } = string.Empty;
        public object? Tag { get; set; }
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when a button is clicked
    /// </summary>
    public event EventHandler<int>? ButtonClick;

    /// <summary>
    /// Raised when a button needs to be drawn (owner-drawn)
    /// </summary>
    public event EventHandler<DrawButtonEventArgs>? DrawButton;

    #endregion

    #region Event Args

    public class DrawButtonEventArgs : EventArgs
    {
        public int ButtonIndex { get; }
        public string ButtonValue { get; }
        public SKCanvas Canvas { get; }
        public SKRect ButtonRect { get; }

        public DrawButtonEventArgs(int buttonIndex, string buttonValue, SKCanvas canvas, SKRect buttonRect)
        {
            ButtonIndex = buttonIndex;
            ButtonValue = buttonValue;
            Canvas = canvas;
            ButtonRect = buttonRect;
        }
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Currently selected button index
    /// </summary>
    public static readonly BindableProperty ListIndexProperty = BindableProperty.Create(
        nameof(ListIndex),
        typeof(int),
        typeof(PdStrip),
        0,
        BindingMode.TwoWay,
        propertyChanged: OnListIndexChanged);

    public int ListIndex
    {
        get => (int)GetValue(ListIndexProperty);
        set => SetValue(ListIndexProperty, value);
    }

    /// <summary>
    /// Collection of strip buttons
    /// </summary>
    public ObservableCollection<StripButton> Buttons { get; }

    /// <summary>
    /// Command executed when button is clicked
    /// </summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(PdStrip),
        null);

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    #endregion

    #region Private Fields

    private List<SKRect> _buttonRects = new();
    private int _hoverIndex = -1;
    private int _mouseDownIndex = -1;

    #endregion

    #region Constructor

    public PdStrip()
    {
        Buttons = new ObservableCollection<StripButton>();
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
        if (bindable is PdStrip control)
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
        var backgroundColor = ConvertToSKColor(BackgroundColor ?? Colors.White);
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
            DrawButtonInternal(canvas, i, _buttonRects[i]);
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

        // Calculate button layout - distribute evenly horizontally
        float buttonWidth = width / Buttons.Count;
        _buttonRects.Clear();

        for (int i = 0; i < Buttons.Count; i++)
        {
            float left = i * buttonWidth;
            _buttonRects.Add(new SKRect(
                left,
                0,
                left + buttonWidth,
                height));
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

    private void DrawButtonInternal(SKCanvas canvas, int index, SKRect buttonRect)
    {
        bool isSelected = (index == ListIndex);
        bool isHovered = (index == _hoverIndex);
        bool isPressed = (index == _mouseDownIndex);

        // Draw button background with subtle hover/selection state
        if (isSelected || isHovered || isPressed)
        {
            SKColor overlayColor;
            if (isSelected)
            {
                overlayColor = new SKColor(0, 0, 0, 30); // Subtle dark overlay for selection
            }
            else if (isPressed)
            {
                overlayColor = new SKColor(0, 0, 0, 50); // Darker for press
            }
            else
            {
                overlayColor = new SKColor(255, 255, 255, 20); // Light overlay for hover
            }

            using (var overlayPaint = new SKPaint
            {
                Color = overlayColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawRect(buttonRect, overlayPaint);
            }
        }

        // Draw border
        using (var borderPaint = new SKPaint
        {
            Color = SKColors.Gray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            IsAntialias = true
        })
        {
            canvas.DrawRect(buttonRect, borderPaint);
        }

        // Raise DrawButton event for owner-drawn content
        if (DrawButton != null && index < Buttons.Count)
        {
            var args = new DrawButtonEventArgs(index, Buttons[index].Data, canvas, buttonRect);
            DrawButton.Invoke(this, args);
        }
    }

    #endregion
}
