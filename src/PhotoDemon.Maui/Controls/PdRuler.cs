using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon viewport ruler UI element - migrated from VB6 pdRuler.ctl
///
/// Features:
/// - Horizontal or vertical ruler orientation
/// - Multiple measurement units (pixels, inches, centimeters)
/// - High-DPI aware rendering
/// - Automatic notch and label rendering
/// - Mouse position tracking on ruler
/// - Integration with canvas viewport
/// - Theme support
///
/// This control is designed for use on the primary canvas to show measurement guides.
/// It displays tick marks and labels at appropriate intervals based on zoom level.
///
/// Original VB6: Controls/pdRuler.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdRuler : PhotoDemonControlBase
{
    #region Enums

    public enum Orientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    public enum MeasurementUnit
    {
        Pixels = 0,
        Inches = 1,
        Centimeters = 2
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Ruler orientation (Horizontal or Vertical)
    /// </summary>
    public static readonly BindableProperty RulerOrientationProperty = BindableProperty.Create(
        nameof(RulerOrientation),
        typeof(Orientation),
        typeof(PdRuler),
        Orientation.Horizontal,
        propertyChanged: OnVisualPropertyChanged);

    public Orientation RulerOrientation
    {
        get => (Orientation)GetValue(RulerOrientationProperty);
        set => SetValue(RulerOrientationProperty, value);
    }

    /// <summary>
    /// Measurement unit for ruler (Pixels, Inches, Centimeters)
    /// </summary>
    public static readonly BindableProperty UnitProperty = BindableProperty.Create(
        nameof(Unit),
        typeof(MeasurementUnit),
        typeof(PdRuler),
        MeasurementUnit.Pixels,
        propertyChanged: OnVisualPropertyChanged);

    public MeasurementUnit Unit
    {
        get => (MeasurementUnit)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    /// <summary>
    /// Canvas offset X for ruler calculations
    /// </summary>
    public static readonly BindableProperty CanvasOffsetXProperty = BindableProperty.Create(
        nameof(CanvasOffsetX),
        typeof(double),
        typeof(PdRuler),
        0.0,
        propertyChanged: OnVisualPropertyChanged);

    public double CanvasOffsetX
    {
        get => (double)GetValue(CanvasOffsetXProperty);
        set => SetValue(CanvasOffsetXProperty, value);
    }

    /// <summary>
    /// Canvas offset Y for ruler calculations
    /// </summary>
    public static readonly BindableProperty CanvasOffsetYProperty = BindableProperty.Create(
        nameof(CanvasOffsetY),
        typeof(double),
        typeof(PdRuler),
        0.0,
        propertyChanged: OnVisualPropertyChanged);

    public double CanvasOffsetY
    {
        get => (double)GetValue(CanvasOffsetYProperty);
        set => SetValue(CanvasOffsetYProperty, value);
    }

    /// <summary>
    /// Zoom level for ruler scaling (1.0 = 100%)
    /// </summary>
    public static readonly BindableProperty ZoomLevelProperty = BindableProperty.Create(
        nameof(ZoomLevel),
        typeof(double),
        typeof(PdRuler),
        1.0,
        propertyChanged: OnVisualPropertyChanged);

    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    /// <summary>
    /// Image DPI for unit conversion
    /// </summary>
    public static readonly BindableProperty ImageDPIProperty = BindableProperty.Create(
        nameof(ImageDPI),
        typeof(double),
        typeof(PdRuler),
        96.0,
        propertyChanged: OnVisualPropertyChanged);

    public double ImageDPI
    {
        get => (double)GetValue(ImageDPIProperty);
        set => SetValue(ImageDPIProperty, value);
    }

    #endregion

    #region Private Fields

    private double _mouseX = -1;
    private double _mouseY = -1;

    #endregion

    #region Constructor

    public PdRuler()
    {
        // Set up mouse tracking
        PointerGestureRecognizer pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerExited += OnPointerExited;
        GestureRecognizers.Add(pointerGesture);

        SizeChanged += (s, e) => InvalidateSurface();
    }

    #endregion

    #region Event Handlers

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetPosition(this) is Point point)
        {
            _mouseX = point.X;
            _mouseY = point.Y;
            InvalidateSurface();
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _mouseX = -1;
        _mouseY = -1;
        InvalidateSurface();
    }

    #endregion

    #region Protected Override Methods

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        canvas.Clear();

        // Fill background
        var backgroundColor = ConvertToSKColor(BackgroundColor ?? Colors.LightGray);
        canvas.Clear(backgroundColor);

        // Draw ruler based on orientation
        if (RulerOrientation == Orientation.Horizontal)
        {
            DrawHorizontalRuler(canvas, info);
        }
        else
        {
            DrawVerticalRuler(canvas, info);
        }

        // Draw mouse position indicator if mouse is over ruler
        if (_mouseX >= 0 || _mouseY >= 0)
        {
            DrawMouseIndicator(canvas, info);
        }
    }

    #endregion

    #region Private Methods

    private void DrawHorizontalRuler(SKCanvas canvas, SKImageInfo info)
    {
        float width = info.Width;
        float height = info.Height;

        // Calculate step size based on zoom level
        int step = CalculateStepSize();
        if (step <= 0) return;

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 10f * GetDisplayDensity(),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        using var linePaint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1f,
            IsAntialias = true
        };

        // Draw tick marks and labels
        float offsetX = (float)CanvasOffsetX;
        int start = (int)Math.Floor(-offsetX / step) * step;
        int end = (int)Math.Ceiling((width - offsetX) / step) * step;

        for (int i = start; i <= end; i += step)
        {
            float x = offsetX + i;
            if (x < 0 || x > width) continue;

            // Draw major tick
            float tickHeight = height * 0.4f;
            canvas.DrawLine(x, height - tickHeight, x, height, linePaint);

            // Draw label
            string label = FormatLabel(i);
            canvas.DrawText(label, x, height - tickHeight - 2, textPaint);

            // Draw minor ticks
            for (int j = 1; j < 10; j++)
            {
                float minorX = x + (j * step / 10f);
                if (minorX >= 0 && minorX <= width)
                {
                    float minorHeight = j == 5 ? height * 0.25f : height * 0.15f;
                    canvas.DrawLine(minorX, height - minorHeight, minorX, height, linePaint);
                }
            }
        }
    }

    private void DrawVerticalRuler(SKCanvas canvas, SKImageInfo info)
    {
        float width = info.Width;
        float height = info.Height;

        // Calculate step size based on zoom level
        int step = CalculateStepSize();
        if (step <= 0) return;

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 10f * GetDisplayDensity(),
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };

        using var linePaint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1f,
            IsAntialias = true
        };

        // Draw tick marks and labels
        float offsetY = (float)CanvasOffsetY;
        int start = (int)Math.Floor(-offsetY / step) * step;
        int end = (int)Math.Ceiling((height - offsetY) / step) * step;

        for (int i = start; i <= end; i += step)
        {
            float y = offsetY + i;
            if (y < 0 || y > height) continue;

            // Draw major tick
            float tickWidth = width * 0.4f;
            canvas.DrawLine(width - tickWidth, y, width, y, linePaint);

            // Draw label (rotated for vertical ruler)
            string label = FormatLabel(i);
            canvas.Save();
            canvas.Translate(width - tickWidth - 4, y);
            canvas.RotateDegrees(-90);
            canvas.DrawText(label, 0, 0, textPaint);
            canvas.Restore();

            // Draw minor ticks
            for (int j = 1; j < 10; j++)
            {
                float minorY = y + (j * step / 10f);
                if (minorY >= 0 && minorY <= height)
                {
                    float minorWidth = j == 5 ? width * 0.25f : width * 0.15f;
                    canvas.DrawLine(width - minorWidth, minorY, width, minorY, linePaint);
                }
            }
        }
    }

    private void DrawMouseIndicator(SKCanvas canvas, SKImageInfo info)
    {
        using var indicatorPaint = new SKPaint
        {
            Color = new SKColor(255, 0, 0, 128),
            StrokeWidth = 2f,
            IsAntialias = true
        };

        if (RulerOrientation == Orientation.Horizontal && _mouseX >= 0)
        {
            canvas.DrawLine((float)_mouseX, 0, (float)_mouseX, info.Height, indicatorPaint);
        }
        else if (RulerOrientation == Orientation.Vertical && _mouseY >= 0)
        {
            canvas.DrawLine(0, (float)_mouseY, info.Width, (float)_mouseY, indicatorPaint);
        }
    }

    private int CalculateStepSize()
    {
        // Calculate appropriate step size based on zoom level and unit
        double zoomFactor = ZoomLevel;

        switch (Unit)
        {
            case MeasurementUnit.Pixels:
                if (zoomFactor >= 8) return 10;
                if (zoomFactor >= 4) return 25;
                if (zoomFactor >= 2) return 50;
                if (zoomFactor >= 1) return 100;
                return 200;

            case MeasurementUnit.Inches:
                // Convert based on DPI
                return (int)(ImageDPI / zoomFactor);

            case MeasurementUnit.Centimeters:
                // Convert based on DPI (1 inch = 2.54 cm)
                return (int)(ImageDPI / 2.54 / zoomFactor);

            default:
                return 100;
        }
    }

    private string FormatLabel(int value)
    {
        switch (Unit)
        {
            case MeasurementUnit.Pixels:
                return value.ToString();

            case MeasurementUnit.Inches:
                double inches = value / ImageDPI;
                return inches.ToString("0.0\"");

            case MeasurementUnit.Centimeters:
                double cm = value / ImageDPI * 2.54;
                return cm.ToString("0.0cm");

            default:
                return value.ToString();
        }
    }

    private float GetDisplayDensity()
    {
        return (float)(DeviceDisplay.Current?.MainDisplayInfo.Density ?? 1.0);
    }

    #endregion
}
