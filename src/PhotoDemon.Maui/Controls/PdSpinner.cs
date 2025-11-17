using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Data;
using System.Globalization;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon spinner control - migrated from VB6 pdSpinner.ctl
///
/// Features:
/// - Text entry with numeric validation
/// - Up/Down spin buttons
/// - Optional reset button (to default value)
/// - Formula evaluation (e.g., "(1+2)*3" = 9)
/// - Min/Max range validation
/// - Floating-point support with significant digits
/// - Locale-aware input (comma/decimal)
/// - Change and FinalChange events
///
/// Original VB6: Controls/pdSpinner.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 72 instances + embedded in pdSlider
/// </summary>
public class PdSpinner : PhotoDemonControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Current numeric value
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(PdSpinner),
        0.0,
        BindingMode.TwoWay,
        propertyChanged: OnValueChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Minimum allowed value
    /// </summary>
    public static readonly BindableProperty MinProperty = BindableProperty.Create(
        nameof(Min),
        typeof(double),
        typeof(PdSpinner),
        double.MinValue);

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    /// <summary>
    /// Maximum allowed value
    /// </summary>
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max),
        typeof(double),
        typeof(PdSpinner),
        double.MaxValue);

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>
    /// Default value (for reset button)
    /// </summary>
    public static readonly BindableProperty DefaultValueProperty = BindableProperty.Create(
        nameof(DefaultValue),
        typeof(double),
        typeof(PdSpinner),
        0.0);

    public double DefaultValue
    {
        get => (double)GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// Number of significant digits (0 = integer)
    /// </summary>
    public static readonly BindableProperty SigDigitsProperty = BindableProperty.Create(
        nameof(SigDigits),
        typeof(int),
        typeof(PdSpinner),
        0,
        propertyChanged: OnValueChanged);

    public int SigDigits
    {
        get => (int)GetValue(SigDigitsProperty);
        set => SetValue(SigDigitsProperty, value);
    }

    /// <summary>
    /// Increment/decrement step for spin buttons
    /// </summary>
    public static readonly BindableProperty IncrementProperty = BindableProperty.Create(
        nameof(Increment),
        typeof(double),
        typeof(PdSpinner),
        1.0);

    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    /// <summary>
    /// Whether to show the reset button
    /// </summary>
    public static readonly BindableProperty ShowResetButtonProperty = BindableProperty.Create(
        nameof(ShowResetButton),
        typeof(bool),
        typeof(PdSpinner),
        true,
        propertyChanged: OnLayoutPropertyChanged);

    public bool ShowResetButton
    {
        get => (bool)GetValue(ShowResetButtonProperty);
        set => SetValue(ShowResetButtonProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when value changes by any means
    /// </summary>
    public event EventHandler Change;

    /// <summary>
    /// Raised when value changes and mouse is released (for expensive operations)
    /// </summary>
    public event EventHandler FinalChange;

    /// <summary>
    /// Raised before reset button is clicked
    /// </summary>
    public event EventHandler BeforeResetClick;

    /// <summary>
    /// Raised when reset button is clicked
    /// </summary>
    public event EventHandler ResetClick;

    /// <summary>
    /// Raised when control gets focus
    /// </summary>
    public event EventHandler GotFocusAPI;

    /// <summary>
    /// Raised when control loses focus
    /// </summary>
    public event EventHandler LostFocusAPI;

    #endregion

    #region Private Fields

    private readonly Entry _textBox;
    private readonly SKCanvasView _canvasView;
    private readonly Grid _containerGrid;

    private bool _mouseDownUp;
    private bool _mouseDownDown;
    private bool _mouseDownReset;
    private bool _mouseOverUp;
    private bool _mouseOverDown;
    private bool _mouseOverReset;
    private bool _textBoxInitiated;
    private bool _isSpinning;

    private const float ButtonWidth = 20;
    private const float ButtonHeight = 14;

    #endregion

    #region Constructor

    public PdSpinner()
    {
        // Create container grid
        _containerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // Text box
                new ColumnDefinition { Width = GridLength.Auto }, // Reset button
                new ColumnDefinition { Width = GridLength.Auto }  // Spin buttons
            }
        };

        // Create text entry
        _textBox = new Entry
        {
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Fill
        };
        _textBox.TextChanged += OnTextChanged;
        _textBox.Unfocused += OnTextBoxUnfocused;
        _textBox.Focused += (s, e) => GotFocusAPI?.Invoke(this, EventArgs.Empty);
        Grid.SetColumn(_textBox, 0);
        _containerGrid.Children.Add(_textBox);

        // Create canvas for custom buttons (reset, up, down)
        _canvasView = new SKCanvasView
        {
            WidthRequest = ButtonWidth * 2, // Reset + Spin buttons
            HeightRequest = ButtonHeight * 2,
            VerticalOptions = LayoutOptions.Center
        };
        _canvasView.PaintSurface += OnPaintSurface;

        // Set up gesture recognizers
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnCanvasTapped;
        _canvasView.GestureRecognizers.Add(tapGesture);

        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerPressed += OnPointerPressed;
        pointerGesture.PointerReleased += OnPointerReleased;
        pointerGesture.PointerExited += (s, e) =>
        {
            _mouseOverUp = _mouseOverDown = _mouseOverReset = false;
            _canvasView.InvalidateSurface();
        };
        _canvasView.GestureRecognizers.Add(pointerGesture);

        Grid.SetColumn(_canvasView, 2);
        _containerGrid.Children.Add(_canvasView);

        Content = _containerGrid;

        // Initialize value display
        UpdateTextBox();
        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_textBoxInitiated) return;

        _textBoxInitiated = true;
        try
        {
            if (TryParseValue(e.NewTextValue, out double result))
            {
                if (result >= Min && result <= Max)
                {
                    Value = result;
                    Change?.Invoke(this, EventArgs.Empty);
                    FinalChange?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        finally
        {
            _textBoxInitiated = false;
        }
    }

    private void OnTextBoxUnfocused(object sender, FocusEventArgs e)
    {
        // Reformat text box when focus lost
        UpdateTextBox();
        LostFocusAPI?.Invoke(this, EventArgs.Empty);
    }

    private void OnCanvasTapped(object sender, TappedEventArgs e)
    {
        if (!IsEnabled) return;

        var point = e.GetPosition(_canvasView);
        if (!point.HasValue) return;

        float x = (float)point.Value.X;
        float y = (float)point.Value.Y;

        if (IsInResetButton(x, y) && ShowResetButton)
        {
            HandleResetClick();
        }
        else if (IsInUpButton(x, y))
        {
            IncrementValue();
        }
        else if (IsInDownButton(x, y))
        {
            DecrementValue();
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        var point = e.GetPosition(_canvasView);
        if (!point.HasValue) return;

        float x = (float)point.Value.X;
        float y = (float)point.Value.Y;

        bool newOverUp = IsInUpButton(x, y);
        bool newOverDown = IsInDownButton(x, y);
        bool newOverReset = IsInResetButton(x, y) && ShowResetButton;

        if (newOverUp != _mouseOverUp || newOverDown != _mouseOverDown || newOverReset != _mouseOverReset)
        {
            _mouseOverUp = newOverUp;
            _mouseOverDown = newOverDown;
            _mouseOverReset = newOverReset;
            _canvasView.InvalidateSurface();
        }
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        var point = e.GetPosition(_canvasView);
        if (!point.HasValue) return;

        float x = (float)point.Value.X;
        float y = (float)point.Value.Y;

        _mouseDownUp = IsInUpButton(x, y);
        _mouseDownDown = IsInDownButton(x, y);
        _mouseDownReset = IsInResetButton(x, y) && ShowResetButton;
        _isSpinning = _mouseDownUp || _mouseDownDown;

        _canvasView.InvalidateSurface();
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        if (_isSpinning)
        {
            FinalChange?.Invoke(this, EventArgs.Empty);
        }

        _mouseDownUp = _mouseDownDown = _mouseDownReset = false;
        _isSpinning = false;
        _canvasView.InvalidateSurface();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;

        float x = 0;

        // Draw reset button if visible
        if (ShowResetButton)
        {
            DrawResetButton(canvas, new SKRect(x, 0, x + ButtonWidth, info.Height));
            x += ButtonWidth;
        }

        // Draw up/down spin buttons
        float halfHeight = info.Height / 2;
        DrawUpButton(canvas, new SKRect(x, 0, x + ButtonWidth, halfHeight));
        DrawDownButton(canvas, new SKRect(x, halfHeight, x + ButtonWidth, info.Height));
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdSpinner spinner)
        {
            // Clamp value to range
            double value = (double)newValue;
            if (value < spinner.Min) value = spinner.Min;
            if (value > spinner.Max) value = spinner.Max;

            if (!spinner._textBoxInitiated)
            {
                spinner.UpdateTextBox();
            }

            spinner.Change?.Invoke(spinner, EventArgs.Empty);
        }
    }

    private static void OnLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdSpinner spinner)
        {
            spinner._canvasView.WidthRequest = spinner.ShowResetButton
                ? ButtonWidth * 2
                : ButtonWidth;
            spinner._canvasView.InvalidateSurface();
        }
    }

    #endregion

    #region Value Handling

    private void IncrementValue()
    {
        double newValue = Value + Increment;
        if (newValue <= Max)
        {
            Value = newValue;
            if (!_isSpinning)
            {
                FinalChange?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void DecrementValue()
    {
        double newValue = Value - Increment;
        if (newValue >= Min)
        {
            Value = newValue;
            if (!_isSpinning)
            {
                FinalChange?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void HandleResetClick()
    {
        BeforeResetClick?.Invoke(this, EventArgs.Empty);
        Value = DefaultValue;
        ResetClick?.Invoke(this, EventArgs.Empty);
        FinalChange?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateTextBox()
    {
        _textBoxInitiated = true;
        try
        {
            string format = SigDigits > 0 ? $"F{SigDigits}" : "F0";
            _textBox.Text = Value.ToString(format, CultureInfo.CurrentCulture);
        }
        finally
        {
            _textBoxInitiated = false;
        }
    }

    private bool TryParseValue(string text, out double result)
    {
        result = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // Try simple numeric parse
        if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            return true;

        // Try formula evaluation
        return TryEvaluateFormula(text, out result);
    }

    private bool TryEvaluateFormula(string formula, out double result)
    {
        result = 0;

        try
        {
            // Use DataTable.Compute for simple formula evaluation
            var dataTable = new DataTable();
            var value = dataTable.Compute(formula, null);
            result = Convert.ToDouble(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Hit Testing

    private bool IsInResetButton(float x, float y)
    {
        if (!ShowResetButton) return false;
        return x >= 0 && x < ButtonWidth;
    }

    private bool IsInUpButton(float x, float y)
    {
        float startX = ShowResetButton ? ButtonWidth : 0;
        float halfHeight = _canvasView.Height / 2;
        return x >= startX && x < startX + ButtonWidth && y < halfHeight;
    }

    private bool IsInDownButton(float x, float y)
    {
        float startX = ShowResetButton ? ButtonWidth : 0;
        float halfHeight = _canvasView.Height / 2;
        return x >= startX && x < startX + ButtonWidth && y >= halfHeight;
    }

    #endregion

    #region Rendering

    private void DrawResetButton(SKCanvas canvas, SKRect rect)
    {
        var fillColor = GetButtonFillColor(_mouseDownReset, _mouseOverReset);
        var borderColor = GetButtonBorderColor();

        // Draw button background
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = fillColor;
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(rect, paint);
        }

        // Draw border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = borderColor;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            canvas.DrawRect(rect, paint);
        }

        // Draw reset icon (circular arrow)
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = IsEnabled ? new SKColor(60, 60, 60) : new SKColor(150, 150, 150);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1.5f;

            float centerX = rect.MidX;
            float centerY = rect.MidY;
            float radius = 5;

            using (var path = new SKPath())
            {
                path.AddArc(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius), -90, 270);
                canvas.DrawPath(path, paint);

                // Arrow head
                path.Reset();
                path.MoveTo(centerX + radius, centerY);
                path.LineTo(centerX + radius - 3, centerY - 3);
                path.MoveTo(centerX + radius, centerY);
                path.LineTo(centerX + radius - 3, centerY + 3);
                canvas.DrawPath(path, paint);
            }
        }
    }

    private void DrawUpButton(SKCanvas canvas, SKRect rect)
    {
        var fillColor = GetButtonFillColor(_mouseDownUp, _mouseOverUp);
        var borderColor = GetButtonBorderColor();

        // Draw button background
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = fillColor;
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(rect, paint);
        }

        // Draw border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = borderColor;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            canvas.DrawRect(rect, paint);
        }

        // Draw up arrow
        DrawArrow(canvas, rect, true);
    }

    private void DrawDownButton(SKCanvas canvas, SKRect rect)
    {
        var fillColor = GetButtonFillColor(_mouseDownDown, _mouseOverDown);
        var borderColor = GetButtonBorderColor();

        // Draw button background
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = fillColor;
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(rect, paint);
        }

        // Draw border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = borderColor;
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            canvas.DrawRect(rect, paint);
        }

        // Draw down arrow
        DrawArrow(canvas, rect, false);
    }

    private void DrawArrow(SKCanvas canvas, SKRect rect, bool pointsUp)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = IsEnabled ? new SKColor(60, 60, 60) : new SKColor(150, 150, 150);
            paint.Style = SKPaintStyle.Fill;

            float centerX = rect.MidX;
            float centerY = rect.MidY;
            float size = 4;

            using (var path = new SKPath())
            {
                if (pointsUp)
                {
                    path.MoveTo(centerX, centerY - size);
                    path.LineTo(centerX - size, centerY + size);
                    path.LineTo(centerX + size, centerY + size);
                }
                else
                {
                    path.MoveTo(centerX, centerY + size);
                    path.LineTo(centerX - size, centerY - size);
                    path.LineTo(centerX + size, centerY - size);
                }
                path.Close();

                canvas.DrawPath(path, paint);
            }
        }
    }

    private SKColor GetButtonFillColor(bool isPressed, bool isHovered)
    {
        if (!IsEnabled)
            return new SKColor(240, 240, 240);
        else if (isPressed)
            return new SKColor(180, 180, 180);
        else if (isHovered)
            return new SKColor(220, 220, 220);
        else
            return new SKColor(245, 245, 245);
    }

    private SKColor GetButtonBorderColor()
    {
        return IsEnabled
            ? new SKColor(170, 170, 170)
            : new SKColor(200, 200, 200);
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
