using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon standalone slider control - migrated from VB6 pdSliderStandalone.ctl
///
/// Features:
/// - Horizontal slider with draggable thumb
/// - Min/Max range with floating-point support
/// - Multiple track styles (default, gradient, hue spectrum, custom)
/// - Change and FinalChange events
/// - High-DPI aware
/// - Significant digits support
///
/// This is typically used as a component within pdSlider (which adds a spinner).
/// Can also be used standalone for simple slider needs.
///
/// Original VB6: Controls/pdSliderStandalone.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 7 instances + embedded in pdSlider (454 uses)
/// </summary>
public class PdSliderStandalone : PhotoDemonControlBase
{
    #region Enums

    public enum TrackStyle
    {
        Default = 0,
        NoFrills = 1,
        GradientTwoPoint = 2,
        GradientThreePoint = 3,
        HueSpectrum360 = 4,
        CustomOwnerDrawn = 5
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Current slider value
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(PdSliderStandalone),
        0.0,
        BindingMode.TwoWay,
        propertyChanged: OnValueChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Minimum value
    /// </summary>
    public static readonly BindableProperty MinProperty = BindableProperty.Create(
        nameof(Min),
        typeof(double),
        typeof(PdSliderStandalone),
        0.0);

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    /// <summary>
    /// Maximum value
    /// </summary>
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max),
        typeof(double),
        typeof(PdSliderStandalone),
        100.0);

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>
    /// Number of significant digits (0 = integer)
    /// </summary>
    public static readonly BindableProperty SignificantDigitsProperty = BindableProperty.Create(
        nameof(SignificantDigits),
        typeof(int),
        typeof(PdSliderStandalone),
        0);

    public int SignificantDigits
    {
        get => (int)GetValue(SignificantDigitsProperty);
        set => SetValue(SignificantDigitsProperty, value);
    }

    /// <summary>
    /// Track rendering style
    /// </summary>
    public static readonly BindableProperty TrackStyleProperty = BindableProperty.Create(
        nameof(SliderTrackStyle),
        typeof(TrackStyle),
        typeof(PdSliderStandalone),
        TrackStyle.Default,
        propertyChanged: OnVisualPropertyChanged);

    public TrackStyle SliderTrackStyle
    {
        get => (TrackStyle)GetValue(TrackStyleProperty);
        set => SetValue(TrackStyleProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when value changes
    /// </summary>
    public event EventHandler Change;

    /// <summary>
    /// Raised when dragging completes (mouse released)
    /// </summary>
    public event EventHandler FinalChange;

    /// <summary>
    /// Raised for custom track rendering (when TrackStyle = CustomOwnerDrawn)
    /// </summary>
    public event EventHandler<SKPaintSurfaceEventArgs> RenderTrackImage;

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

    private readonly SKCanvasView _canvasView;
    private bool _isDragging;
    private float _trackDiameter = 6;
    private float _sliderDiameter = 16;
    private SKRect _trackRect;
    private SKRect _sliderRect;

    #endregion

    #region Constructor

    public PdSliderStandalone()
    {
        // Create canvas for rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        // Set up gesture recognizers
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        _canvasView.GestureRecognizers.Add(panGesture);

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _canvasView.GestureRecognizers.Add(tapGesture);

        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!IsEnabled) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isDragging = true;
                break;

            case GestureStatus.Running:
                if (_isDragging)
                {
                    UpdateValueFromPosition((float)e.TotalX);
                    Change?.Invoke(this, EventArgs.Empty);
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (_isDragging)
                {
                    _isDragging = false;
                    FinalChange?.Invoke(this, EventArgs.Empty);
                }
                break;
        }
    }

    private void OnTapped(object sender, TappedEventArgs e)
    {
        if (!IsEnabled) return;

        var point = e.GetPosition(_canvasView);
        if (point.HasValue)
        {
            float relativeX = (float)point.Value.X - _trackRect.Left;
            UpdateValueFromTrackPosition(relativeX);
            Change?.Invoke(this, EventArgs.Empty);
            FinalChange?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;

        // Update layout
        UpdateLayout(info.Width, info.Height);

        // Draw track
        DrawTrack(canvas, e);

        // Draw slider thumb
        DrawSlider(canvas);
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdSliderStandalone slider)
        {
            // Clamp value to range
            double value = (double)newValue;
            if (value < slider.Min) value = slider.Min;
            if (value > slider.Max) value = slider.Max;

            if (Math.Abs(value - (double)newValue) > 0.0001)
            {
                slider.Value = value;
            }

            slider._canvasView?.InvalidateSurface();
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdSliderStandalone slider)
        {
            slider._canvasView?.InvalidateSurface();
        }
    }

    #endregion

    #region Layout

    private void UpdateLayout(float width, float height)
    {
        // Track rect (horizontal bar in center)
        float trackHeight = _trackDiameter;
        float trackY = (height - trackHeight) / 2;
        float trackMargin = _sliderDiameter / 2;
        _trackRect = new SKRect(trackMargin, trackY, width - trackMargin, trackY + trackHeight);

        // Slider thumb position based on value
        float valuePercent = (float)((Value - Min) / (Max - Min));
        float sliderX = _trackRect.Left + (valuePercent * _trackRect.Width);
        float sliderY = height / 2;
        float sliderRadius = _sliderDiameter / 2;
        _sliderRect = new SKRect(
            sliderX - sliderRadius,
            sliderY - sliderRadius,
            sliderX + sliderRadius,
            sliderY + sliderRadius);
    }

    private void UpdateValueFromPosition(float deltaX)
    {
        // Calculate new slider position
        float currentX = _sliderRect.MidX + deltaX;
        UpdateValueFromTrackPosition(currentX - _trackRect.Left);
    }

    private void UpdateValueFromTrackPosition(float relativeX)
    {
        // Clamp to track bounds
        relativeX = Math.Max(0, Math.Min(_trackRect.Width, relativeX));

        // Calculate value
        double percent = relativeX / _trackRect.Width;
        double newValue = Min + (percent * (Max - Min));

        // Round to significant digits
        if (SignificantDigits > 0)
        {
            newValue = Math.Round(newValue, SignificantDigits);
        }
        else
        {
            newValue = Math.Round(newValue);
        }

        Value = newValue;
    }

    #endregion

    #region Rendering

    private void DrawTrack(SKCanvas canvas, SKPaintSurfaceEventArgs e)
    {
        switch (SliderTrackStyle)
        {
            case TrackStyle.Default:
                DrawDefaultTrack(canvas);
                break;

            case TrackStyle.NoFrills:
                DrawNoFrillsTrack(canvas);
                break;

            case TrackStyle.GradientTwoPoint:
                DrawGradientTwoPointTrack(canvas);
                break;

            case TrackStyle.GradientThreePoint:
                DrawGradientThreePointTrack(canvas);
                break;

            case TrackStyle.HueSpectrum360:
                DrawHueSpectrumTrack(canvas);
                break;

            case TrackStyle.CustomOwnerDrawn:
                RenderTrackImage?.Invoke(this, e);
                break;
        }
    }

    private void DrawDefaultTrack(SKCanvas canvas)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = IsEnabled
                ? new SKColor(200, 200, 200)
                : new SKColor(230, 230, 230);
            paint.Style = SKPaintStyle.Fill;

            canvas.DrawRoundRect(_trackRect, _trackDiameter / 2, _trackDiameter / 2, paint);
        }

        // Draw filled portion
        if (Value > Min)
        {
            float fillWidth = _sliderRect.MidX - _trackRect.Left;
            var fillRect = new SKRect(_trackRect.Left, _trackRect.Top, _trackRect.Left + fillWidth, _trackRect.Bottom);

            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Color = IsEnabled
                    ? new SKColor(0, 120, 215)
                    : new SKColor(150, 150, 150);
                paint.Style = SKPaintStyle.Fill;

                canvas.DrawRoundRect(fillRect, _trackDiameter / 2, _trackDiameter / 2, paint);
            }
        }
    }

    private void DrawNoFrillsTrack(SKCanvas canvas)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = new SKColor(180, 180, 180);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;

            canvas.DrawRoundRect(_trackRect, _trackDiameter / 2, _trackDiameter / 2, paint);
        }
    }

    private void DrawGradientTwoPointTrack(SKCanvas canvas)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(_trackRect.Left, _trackRect.MidY),
                new SKPoint(_trackRect.Right, _trackRect.MidY),
                new[] { new SKColor(255, 255, 255), new SKColor(0, 0, 0) },
                SKShaderTileMode.Clamp);

            canvas.DrawRoundRect(_trackRect, _trackDiameter / 2, _trackDiameter / 2, paint);
        }
    }

    private void DrawGradientThreePointTrack(SKCanvas canvas)
    {
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(_trackRect.Left, _trackRect.MidY),
                new SKPoint(_trackRect.Right, _trackRect.MidY),
                new[] {
                    new SKColor(0, 0, 0),
                    new SKColor(128, 128, 128),
                    new SKColor(255, 255, 255)
                },
                new[] { 0.0f, 0.5f, 1.0f },
                SKShaderTileMode.Clamp);

            canvas.DrawRoundRect(_trackRect, _trackDiameter / 2, _trackDiameter / 2, paint);
        }
    }

    private void DrawHueSpectrumTrack(SKCanvas canvas)
    {
        // Create hue spectrum (red -> yellow -> green -> cyan -> blue -> magenta -> red)
        var colors = new[]
        {
            new SKColor(255, 0, 0),     // Red
            new SKColor(255, 255, 0),   // Yellow
            new SKColor(0, 255, 0),     // Green
            new SKColor(0, 255, 255),   // Cyan
            new SKColor(0, 0, 255),     // Blue
            new SKColor(255, 0, 255),   // Magenta
            new SKColor(255, 0, 0)      // Red
        };

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(_trackRect.Left, _trackRect.MidY),
                new SKPoint(_trackRect.Right, _trackRect.MidY),
                colors,
                SKShaderTileMode.Clamp);

            canvas.DrawRoundRect(_trackRect, _trackDiameter / 2, _trackDiameter / 2, paint);
        }
    }

    private void DrawSlider(SKCanvas canvas)
    {
        // Draw slider shadow
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = new SKColor(0, 0, 0, 40);
            paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2);

            var shadowRect = _sliderRect;
            shadowRect.Offset(1, 1);
            canvas.DrawOval(shadowRect, paint);
        }

        // Draw slider fill
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = IsEnabled
                ? (_isDragging ? new SKColor(230, 230, 230) : new SKColor(255, 255, 255))
                : new SKColor(245, 245, 245);
            paint.Style = SKPaintStyle.Fill;

            canvas.DrawOval(_sliderRect, paint);
        }

        // Draw slider border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = IsEnabled
                ? new SKColor(120, 120, 120)
                : new SKColor(200, 200, 200);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;

            canvas.DrawOval(_sliderRect, paint);
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
                GotFocusAPI?.Invoke(this, EventArgs.Empty);
            }
            else
            {
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
