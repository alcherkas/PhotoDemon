using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon color wheel control - migrated from VB6 pdColorWheel.ctl
///
/// Features:
/// - Circular hue wheel for hue selection
/// - Central saturation/value square for saturation and value selection
/// - Mouse interaction for color picking
/// - HSV color space manipulation
/// - ColorChanged event when user selects a color
/// - High-DPI aware rendering
/// - Custom wheel width
///
/// This control provides a quick, on-canvas-friendly mechanism for rapidly switching colors.
/// The design is similar to MyPaint and other photo editors.
///
/// Original VB6: Controls/pdColorWheel.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdColorWheel : PhotoDemonControlBase
{
    #region Events

    /// <summary>
    /// Raised when the color changes (either from user interaction or external set)
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    #endregion

    #region Event Args

    public class ColorChangedEventArgs : EventArgs
    {
        public Color NewColor { get; }
        public bool IsInternal { get; }

        public ColorChangedEventArgs(Color newColor, bool isInternal)
        {
            NewColor = newColor;
            IsInternal = isInternal;
        }
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Current color in RGB
    /// </summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(PdColorWheel),
        Colors.Red,
        BindingMode.TwoWay,
        propertyChanged: OnColorChanged);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Width of the hue wheel ring (in pixels)
    /// </summary>
    public static readonly BindableProperty WheelWidthProperty = BindableProperty.Create(
        nameof(WheelWidth),
        typeof(float),
        typeof(PdColorWheel),
        15f,
        propertyChanged: OnVisualPropertyChanged);

    public float WheelWidth
    {
        get => (float)GetValue(WheelWidthProperty);
        set => SetValue(WheelWidthProperty, value);
    }

    #endregion

    #region Private Fields

    private double _hue = 0;           // 0-360
    private double _saturation = 1;    // 0-1
    private double _value = 1;         // 0-1

    private bool _mouseDownWheel = false;
    private bool _mouseDownBox = false;

    private const float WheelPadding = 2f;

    #endregion

    #region Constructor

    public PdColorWheel()
    {
        // Set up mouse interaction
        PointerGestureRecognizer pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerPressed += OnPointerPressed;
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerReleased += OnPointerReleased;
        GestureRecognizers.Add(pointerGesture);

        SizeChanged += (s, e) => InvalidateSurface();
    }

    #endregion

    #region Event Handlers

    private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdColorWheel wheel && newValue is Color color)
        {
            // Convert RGB to HSV
            RgbToHsv(color, out double h, out double s, out double v);

            if (s != 0) wheel._hue = h;
            if (v != 0) wheel._saturation = s;
            wheel._value = v;

            wheel.InvalidateSurface();
        }
    }

    private void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        if (!IsEnabled) return;

        if (e.GetPosition(this) is Point point)
        {
            float x = (float)point.X;
            float y = (float)point.Y;

            // Check if in wheel or center box
            if (IsInWheelRegion(x, y))
            {
                _mouseDownWheel = true;
                UpdateHueFromPoint(x, y);
            }
            else if (IsInBoxRegion(x, y))
            {
                _mouseDownBox = true;
                UpdateSVFromPoint(x, y);
            }
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!IsEnabled) return;

        if (e.GetPosition(this) is Point point)
        {
            float x = (float)point.X;
            float y = (float)point.Y;

            if (_mouseDownWheel)
            {
                UpdateHueFromPoint(x, y);
            }
            else if (_mouseDownBox)
            {
                UpdateSVFromPoint(x, y);
            }
        }
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        _mouseDownWheel = false;
        _mouseDownBox = false;
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

        float width = info.Width;
        float height = info.Height;
        float size = Math.Min(width, height);
        float centerX = width / 2f;
        float centerY = height / 2f;

        // Calculate wheel dimensions
        float outerRadius = (size / 2f) - WheelPadding;
        float innerRadius = outerRadius - (WheelWidth * GetDisplayDensity());

        // Draw hue wheel
        DrawHueWheel(canvas, centerX, centerY, innerRadius, outerRadius);

        // Draw saturation/value square
        DrawSVSquare(canvas, centerX, centerY, innerRadius);

        // Draw current selection indicators
        DrawSelectionIndicators(canvas, centerX, centerY, innerRadius, outerRadius);
    }

    #endregion

    #region Private Methods

    private void DrawHueWheel(SKCanvas canvas, float centerX, float centerY, float innerRadius, float outerRadius)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = outerRadius - innerRadius
        };

        // Draw hue wheel with color segments
        int segments = 360;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * (360f / segments);
            var color = HsvToRgb(angle, 1, 1);
            paint.Color = new SKColor(
                (byte)(color.Red * 255),
                (byte)(color.Green * 255),
                (byte)(color.Blue * 255));

            float angleRad = angle * (float)Math.PI / 180f;
            float startAngle = angle - 90; // Offset by 90 degrees

            using var path = new SKPath();
            path.AddArc(new SKRect(
                centerX - outerRadius,
                centerY - outerRadius,
                centerX + outerRadius,
                centerY + outerRadius),
                startAngle, 1);
            canvas.DrawPath(path, paint);
        }
    }

    private void DrawSVSquare(SKCanvas canvas, float centerX, float centerY, float maxSize)
    {
        float squareSize = maxSize * 1.4f; // Diagonal fits in circle
        float halfSize = squareSize / 2f;

        // Create gradient for saturation (left to right) and value (top to bottom)
        var currentHueColor = HsvToRgb(_hue, 1, 1);

        // Draw saturation/value gradient
        using (var paint = new SKPaint
        {
            IsAntialias = true
        })
        {
            // This is a simplified version - full implementation would use shader
            float steps = 20;
            for (int y = 0; y < steps; y++)
            {
                for (int x = 0; x < steps; x++)
                {
                    float s = x / steps;
                    float v = 1 - (y / steps);

                    var color = HsvToRgb(_hue, s, v);
                    paint.Color = new SKColor(
                        (byte)(color.Red * 255),
                        (byte)(color.Green * 255),
                        (byte)(color.Blue * 255));

                    float rectX = centerX - halfSize + (x * squareSize / steps);
                    float rectY = centerY - halfSize + (y * squareSize / steps);
                    canvas.DrawRect(new SKRect(
                        rectX, rectY,
                        rectX + squareSize / steps + 1,
                        rectY + squareSize / steps + 1), paint);
                }
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
            canvas.DrawRect(new SKRect(
                centerX - halfSize,
                centerY - halfSize,
                centerX + halfSize,
                centerY + halfSize), borderPaint);
        }
    }

    private void DrawSelectionIndicators(SKCanvas canvas, float centerX, float centerY, float innerRadius, float outerRadius)
    {
        // Draw hue indicator on wheel
        float hueAngle = (float)(_hue - 90) * (float)Math.PI / 180f;
        float hueRadius = (innerRadius + outerRadius) / 2f;
        float hueX = centerX + (float)Math.Cos(hueAngle) * hueRadius;
        float hueY = centerY + (float)Math.Sin(hueAngle) * hueRadius;

        using (var paint = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(hueX, hueY, 4f, paint);
        }

        // Draw S/V indicator in square
        float squareSize = innerRadius * 1.4f;
        float halfSize = squareSize / 2f;
        float svX = centerX - halfSize + (float)(_saturation * squareSize);
        float svY = centerY - halfSize + (float)((1 - _value) * squareSize);

        using (var paint = new SKPaint
        {
            Color = _value > 0.5 ? SKColors.Black : SKColors.White,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(svX, svY, 4f, paint);
        }
    }

    private bool IsInWheelRegion(float x, float y)
    {
        float width = (float)Width;
        float height = (float)Height;
        float size = Math.Min(width, height);
        float centerX = width / 2f;
        float centerY = height / 2f;

        float outerRadius = (size / 2f) - WheelPadding;
        float innerRadius = outerRadius - (WheelWidth * GetDisplayDensity());

        float dx = x - centerX;
        float dy = y - centerY;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy);

        return distance >= innerRadius && distance <= outerRadius;
    }

    private bool IsInBoxRegion(float x, float y)
    {
        float width = (float)Width;
        float height = (float)Height;
        float size = Math.Min(width, height);
        float centerX = width / 2f;
        float centerY = height / 2f;

        float outerRadius = (size / 2f) - WheelPadding;
        float innerRadius = outerRadius - (WheelWidth * GetDisplayDensity());
        float squareSize = innerRadius * 1.4f;
        float halfSize = squareSize / 2f;

        return x >= centerX - halfSize && x <= centerX + halfSize &&
               y >= centerY - halfSize && y <= centerY + halfSize;
    }

    private void UpdateHueFromPoint(float x, float y)
    {
        float width = (float)Width;
        float height = (float)Height;
        float centerX = width / 2f;
        float centerY = height / 2f;

        float dx = x - centerX;
        float dy = y - centerY;
        float angle = (float)Math.Atan2(dy, dx) * 180f / (float)Math.PI;
        angle += 90; // Offset
        if (angle < 0) angle += 360;

        _hue = angle;
        UpdateColorAndRaise(true);
    }

    private void UpdateSVFromPoint(float x, float y)
    {
        float width = (float)Width;
        float height = (float)Height;
        float size = Math.Min(width, height);
        float centerX = width / 2f;
        float centerY = height / 2f;

        float outerRadius = (size / 2f) - WheelPadding;
        float innerRadius = outerRadius - (WheelWidth * GetDisplayDensity());
        float squareSize = innerRadius * 1.4f;
        float halfSize = squareSize / 2f;

        _saturation = Math.Clamp((x - (centerX - halfSize)) / squareSize, 0, 1);
        _value = Math.Clamp(1 - ((y - (centerY - halfSize)) / squareSize), 0, 1);

        UpdateColorAndRaise(true);
    }

    private void UpdateColorAndRaise(bool isInternal)
    {
        var newColor = HsvToRgb(_hue, _saturation, _value);
        Color = newColor;
        ColorChanged?.Invoke(this, new ColorChangedEventArgs(newColor, isInternal));
        InvalidateSurface();
    }

    // HSV to RGB conversion
    private static Color HsvToRgb(double h, double s, double v)
    {
        double r, g, b;

        if (s == 0)
        {
            r = g = b = v;
        }
        else
        {
            h = h / 60.0;
            int i = (int)Math.Floor(h);
            double f = h - i;
            double p = v * (1 - s);
            double q = v * (1 - s * f);
            double t = v * (1 - s * (1 - f));

            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }
        }

        return Color.FromRgb(r, g, b);
    }

    // RGB to HSV conversion
    private static void RgbToHsv(Color color, out double h, out double s, out double v)
    {
        double r = color.Red;
        double g = color.Green;
        double b = color.Blue;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        v = max;

        if (max != 0)
            s = delta / max;
        else
            s = 0;

        if (s == 0)
        {
            h = 0;
        }
        else
        {
            if (r == max)
                h = (g - b) / delta;
            else if (g == max)
                h = 2 + (b - r) / delta;
            else
                h = 4 + (r - g) / delta;

            h *= 60;
            if (h < 0)
                h += 360;
        }
    }

    private float GetDisplayDensity()
    {
        return (float)(DeviceDisplay.Current?.MainDisplayInfo.Density ?? 1.0);
    }

    #endregion
}
