using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Windows.Input;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon custom checkbox control - migrated from VB6 pdCheckBox.ctl
///
/// Features:
/// - Unicode caption support (native in .NET)
/// - Custom rendering with theme support
/// - High-DPI aware
/// - Caption auto-shrink to fit
/// - Clickable checkbox and caption
/// - Focus indicators
///
/// Original VB6: Controls/pdCheckBox.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 201 instances across the application
/// </summary>
public class PdCheckBox : PhotoDemonControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Whether the checkbox is checked
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(bool),
        typeof(PdCheckBox),
        false,
        BindingMode.TwoWay,
        propertyChanged: OnValueChanged);

    public bool Value
    {
        get => (bool)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Command executed when checkbox is toggled
    /// </summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(PdCheckBox),
        null);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Command parameter
    /// </summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(PdCheckBox),
        null);

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Font size for caption
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(PdCheckBox),
        12.0,
        propertyChanged: OnVisualPropertyChanged);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when checkbox value changes
    /// </summary>
    public event EventHandler<bool> Click;

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
    private bool _isMouseOver;
    private bool _showFocusRect;
    private SKRect _checkboxRect;
    private SKRect _captionRect;
    private SKRect _clickableRect;
    private double _calculatedFontSize;

    private const float CheckboxSize = 16;
    private const float CheckboxCaptionGap = 8;

    #endregion

    #region Constructor

    public PdCheckBox()
    {
        // Create canvas for rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        _calculatedFontSize = FontSize;

        // Set up gesture recognizers
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _canvasView.GestureRecognizers.Add(tapGesture);

        // Set up pointer events for hover
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, e) =>
        {
            _isMouseOver = true;
            _canvasView.InvalidateSurface();
        };
        pointerGesture.PointerExited += (s, e) =>
        {
            _isMouseOver = false;
            _canvasView.InvalidateSurface();
        };
        _canvasView.GestureRecognizers.Add(pointerGesture);

        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnTapped(object sender, EventArgs e)
    {
        if (!IsEnabled)
            return;

        // Toggle value
        Value = !Value;

        // Raise events
        Click?.Invoke(this, Value);

        // Execute command if bound
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;

        // Update layout rects
        UpdateLayout(info.Width, info.Height);

        // Draw checkbox
        DrawCheckbox(canvas);

        // Draw caption
        DrawCaption(canvas);

        // Draw focus rect if needed
        if (_showFocusRect)
        {
            DrawFocusRect(canvas);
        }
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdCheckBox checkbox)
        {
            checkbox._canvasView?.InvalidateSurface();
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdCheckBox checkbox)
        {
            checkbox._calculatedFontSize = checkbox.FontSize;
            checkbox._canvasView?.InvalidateSurface();
        }
    }

    #endregion

    #region Rendering

    private void UpdateLayout(float width, float height)
    {
        // Checkbox position (left side, vertically centered)
        float checkboxY = (height - CheckboxSize) / 2;
        _checkboxRect = new SKRect(0, checkboxY, CheckboxSize, checkboxY + CheckboxSize);

        // Caption position (to the right of checkbox)
        float captionX = CheckboxSize + CheckboxCaptionGap;
        float captionWidth = width - captionX;
        _captionRect = new SKRect(captionX, 0, width, height);

        // Clickable rect encompasses both checkbox and caption
        _clickableRect = new SKRect(0, 0, width, height);
    }

    private void DrawCheckbox(SKCanvas canvas)
    {
        var fillColor = GetCheckboxFillColor();
        var borderColor = GetCheckboxBorderColor();

        // Draw checkbox background
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Fill;
            paint.Color = fillColor;
            canvas.DrawRoundRect(_checkboxRect, 2, 2, paint);
        }

        // Draw checkbox border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = borderColor;
            paint.StrokeWidth = 1;
            canvas.DrawRoundRect(_checkboxRect, 2, 2, paint);
        }

        // Draw checkmark if checked
        if (Value)
        {
            DrawCheckmark(canvas);
        }
    }

    private void DrawCheckmark(SKCanvas canvas)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = IsEnabled
                ? new SKColor(0, 120, 215) // Blue checkmark
                : new SKColor(150, 150, 150); // Gray when disabled
            paint.StrokeWidth = 2;
            paint.StrokeCap = SKStrokeCap.Round;

            // Create checkmark path
            using (var path = new SKPath())
            {
                float padding = 4;
                float left = _checkboxRect.Left + padding;
                float top = _checkboxRect.Top + padding;
                float size = CheckboxSize - (padding * 2);

                // Checkmark shape
                path.MoveTo(left, top + size * 0.5f);
                path.LineTo(left + size * 0.4f, top + size * 0.8f);
                path.LineTo(left + size, top + size * 0.2f);

                canvas.DrawPath(path, paint);
            }
        }
    }

    private void DrawCaption(SKCanvas canvas)
    {
        if (string.IsNullOrEmpty(Caption))
            return;

        var textColor = GetCaptionColor();

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = textColor;
            paint.Typeface = SKTypeface.FromFamilyName("Segoe UI");

            // Determine font size (shrink to fit if necessary)
            DetermineFontSize(paint, _captionRect.Width);
            paint.TextSize = (float)_calculatedFontSize * 1.33f;

            // Calculate vertical center
            var textBounds = new SKRect();
            paint.MeasureText(Caption, ref textBounds);
            float textY = (_captionRect.Top + _captionRect.Bottom - textBounds.Height) / 2 - textBounds.Top;

            canvas.DrawText(Caption, _captionRect.Left, textY, paint);
        }
    }

    private void DetermineFontSize(SKPaint paint, float maxWidth)
    {
        _calculatedFontSize = FontSize;
        paint.TextSize = (float)_calculatedFontSize * 1.33f;

        var textBounds = new SKRect();
        paint.MeasureText(Caption, ref textBounds);

        // Shrink font until it fits or we hit minimum
        const double minFontSize = 7.0;
        while (textBounds.Width > maxWidth && _calculatedFontSize > minFontSize)
        {
            _calculatedFontSize -= 0.5;
            paint.TextSize = (float)_calculatedFontSize * 1.33f;
            paint.MeasureText(Caption, ref textBounds);
        }
    }

    private void DrawFocusRect(SKCanvas canvas)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = new SKColor(100, 100, 255, 180);
            paint.StrokeWidth = 1;
            paint.PathEffect = SKPathEffect.CreateDash(new[] { 4f, 4f }, 0);

            var focusRect = _clickableRect;
            focusRect.Inflate(-2, -2);
            canvas.DrawRect(focusRect, paint);
        }
    }

    private SKColor GetCheckboxFillColor()
    {
        if (!IsEnabled)
            return new SKColor(245, 245, 245);
        else if (Value)
            return new SKColor(255, 255, 255);
        else if (_isMouseOver)
            return new SKColor(250, 250, 250);
        else
            return new SKColor(255, 255, 255);
    }

    private SKColor GetCheckboxBorderColor()
    {
        if (!IsEnabled)
            return new SKColor(200, 200, 200);
        else if (Value)
            return new SKColor(0, 120, 215);
        else if (_isMouseOver)
            return new SKColor(150, 150, 150);
        else
            return new SKColor(170, 170, 170);
    }

    private SKColor GetCaptionColor()
    {
        return IsEnabled
            ? new SKColor(0, 0, 0)
            : new SKColor(150, 150, 150);
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
