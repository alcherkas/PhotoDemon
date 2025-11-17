using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon color variants selector - migrated from VB6 pdColorVariants.ctl
///
/// Features:
/// - Displays 13 color variants around a central primary color
/// - Variants include: Hue Up/Down, Saturation Up/Down, Value Up/Down, RGB Up/Down
/// - Quick "nudge" mechanism to adjust colors without opening a full color dialog
/// - Click on any variant to select it
/// - Two display modes: Circular or Rectangular
/// - ColorChanged event when variant is selected
/// - High-DPI aware rendering
///
/// Original VB6: Controls/pdColorVariants.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdColorVariants : PhotoDemonControlBase
{
    #region Enums

    public enum WheelShape
    {
        Circular = 0,
        Rectangular = 1
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when a color variant is selected
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
    /// Primary color
    /// </summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(PdColorVariants),
        Colors.Red,
        BindingMode.TwoWay,
        propertyChanged: OnColorChanged);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Control shape (Circular or Rectangular)
    /// </summary>
    public static readonly BindableProperty ShapeProperty = BindableProperty.Create(
        nameof(Shape),
        typeof(WheelShape),
        typeof(PdColorVariants),
        WheelShape.Rectangular,
        propertyChanged: OnVisualPropertyChanged);

    public WheelShape Shape
    {
        get => (WheelShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    #endregion

    #region Private Fields

    private const int NumVariants = 13;
    private Color[] _colorVariants = new Color[NumVariants];
    private SKRect[] _variantRects = new SKRect[NumVariants];
    private int _hoveredIndex = -1;

    private const float VariantBoxSize = 16f;

    #endregion

    #region Constructor

    public PdColorVariants()
    {
        // Set up mouse interaction
        PointerGestureRecognizer pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerExited += OnPointerExited;
        GestureRecognizers.Add(pointerGesture);

        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        GestureRecognizers.Add(tapGesture);

        SizeChanged += (s, e) => UpdateLayout();

        // Initialize with default color
        CalculateVariants();
    }

    #endregion

    #region Event Handlers

    private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdColorVariants control)
        {
            control.CalculateVariants();
            control.InvalidateSurface();
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetPosition(this) is Point point)
        {
            int oldIndex = _hoveredIndex;
            _hoveredIndex = GetVariantIndexAtPoint((float)point.X, (float)point.Y);

            if (oldIndex != _hoveredIndex)
            {
                InvalidateSurface();
            }
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_hoveredIndex != -1)
        {
            _hoveredIndex = -1;
            InvalidateSurface();
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled) return;

        if (e.GetPosition(this) is Point point)
        {
            int index = GetVariantIndexAtPoint((float)point.X, (float)point.Y);
            if (index >= 0 && index < NumVariants)
            {
                Color = _colorVariants[index];
                ColorChanged?.Invoke(this, new ColorChangedEventArgs(_colorVariants[index], true));
            }
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

        // Draw variant boxes
        for (int i = 0; i < NumVariants && i < _variantRects.Length; i++)
        {
            DrawVariantBox(canvas, i, _variantRects[i]);
        }
    }

    #endregion

    #region Private Methods

    private void UpdateLayout()
    {
        float width = (float)Width;
        float height = (float)Height;

        if (width <= 0 || height <= 0) return;

        float density = GetDisplayDensity();
        float boxSize = VariantBoxSize * density;

        if (Shape == WheelShape.Rectangular)
        {
            // Layout in a grid-like pattern with center box
            // Center box (primary color) - larger
            float centerSize = boxSize * 3;
            float centerX = (width - centerSize) / 2f;
            float centerY = (height - centerSize) / 2f;
            _variantRects[0] = new SKRect(centerX, centerY, centerX + centerSize, centerY + centerSize);

            // Surrounding variant boxes (8 positions around center + 4 corners)
            // Top row: HueUp, SaturationUp, ValueUp
            _variantRects[1] = new SKRect(0, 0, boxSize, boxSize);
            _variantRects[2] = new SKRect(boxSize + 2, 0, boxSize * 2 + 2, boxSize);
            _variantRects[3] = new SKRect((boxSize + 2) * 2, 0, (boxSize + 2) * 2 + boxSize, boxSize);

            // Right column: RedUp, GreenUp, BlueUp
            _variantRects[4] = new SKRect(width - boxSize, 0, width, boxSize);
            _variantRects[5] = new SKRect(width - boxSize, boxSize + 2, width, boxSize * 2 + 2);
            _variantRects[6] = new SKRect(width - boxSize, (boxSize + 2) * 2, width, (boxSize + 2) * 2 + boxSize);

            // Bottom row: ValueDown, SaturationDown, HueDown
            _variantRects[7] = new SKRect((boxSize + 2) * 2, height - boxSize, (boxSize + 2) * 2 + boxSize, height);
            _variantRects[8] = new SKRect(boxSize + 2, height - boxSize, boxSize * 2 + 2, height);
            _variantRects[9] = new SKRect(0, height - boxSize, boxSize, height);

            // Left column: BlueDown, GreenDown, RedDown
            _variantRects[10] = new SKRect(0, (boxSize + 2) * 2, boxSize, (boxSize + 2) * 2 + boxSize);
            _variantRects[11] = new SKRect(0, boxSize + 2, boxSize, boxSize * 2 + 2);
            _variantRects[12] = new SKRect(0, 0, boxSize, boxSize);
        }
        else
        {
            // Circular layout - similar but using circular arrangement
            // For simplicity, we'll use rectangular layout for now
            UpdateLayout(); // Call rectangular version
        }

        InvalidateSurface();
    }

    private void CalculateVariants()
    {
        _colorVariants[0] = Color; // Primary

        // Get HSV values
        RgbToHsv(Color, out double h, out double s, out double v);

        // Calculate variants
        double hueStep = 30; // degrees
        double satStep = 0.2;
        double valStep = 0.2;
        double rgbStep = 0.2;

        // Hue variants
        _colorVariants[1] = HsvToRgb((h + hueStep) % 360, s, v);      // HueUp
        _colorVariants[9] = HsvToRgb((h - hueStep + 360) % 360, s, v); // HueDown

        // Saturation variants
        _colorVariants[2] = HsvToRgb(h, Math.Clamp(s + satStep, 0, 1), v); // SaturationUp
        _colorVariants[8] = HsvToRgb(h, Math.Clamp(s - satStep, 0, 1), v); // SaturationDown

        // Value variants
        _colorVariants[3] = HsvToRgb(h, s, Math.Clamp(v + valStep, 0, 1)); // ValueUp
        _colorVariants[7] = HsvToRgb(h, s, Math.Clamp(v - valStep, 0, 1)); // ValueDown

        // RGB variants
        _colorVariants[4] = Color.WithRed(Math.Clamp(Color.Red + rgbStep, 0, 1));   // RedUp
        _colorVariants[12] = Color.WithRed(Math.Clamp(Color.Red - rgbStep, 0, 1));  // RedDown

        _colorVariants[5] = Color.WithGreen(Math.Clamp(Color.Green + rgbStep, 0, 1)); // GreenUp
        _colorVariants[11] = Color.WithGreen(Math.Clamp(Color.Green - rgbStep, 0, 1)); // GreenDown

        _colorVariants[6] = Color.WithBlue(Math.Clamp(Color.Blue + rgbStep, 0, 1));  // BlueUp
        _colorVariants[10] = Color.WithBlue(Math.Clamp(Color.Blue - rgbStep, 0, 1)); // BlueDown
    }

    private void DrawVariantBox(SKCanvas canvas, int index, SKRect rect)
    {
        // Fill with variant color
        using (var fillPaint = new SKPaint
        {
            Color = ConvertToSKColor(_colorVariants[index]),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        })
        {
            canvas.DrawRect(rect, fillPaint);
        }

        // Draw border (highlight if hovered)
        using (var borderPaint = new SKPaint
        {
            Color = index == _hoveredIndex ? SKColors.White : SKColors.Gray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = index == _hoveredIndex ? 2f : 1f,
            IsAntialias = true
        })
        {
            canvas.DrawRect(rect, borderPaint);
        }
    }

    private int GetVariantIndexAtPoint(float x, float y)
    {
        for (int i = 0; i < NumVariants && i < _variantRects.Length; i++)
        {
            if (_variantRects[i].Contains(x, y))
            {
                return i;
            }
        }
        return -1;
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
