using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon lightweight picture box control - migrated from VB6 pdPictureBox.ctl
///
/// Features:
/// - Non-interactive image display (cannot receive focus by design)
/// - Custom rendering via DrawMe event
/// - Image display with automatic centering and aspect ratio preservation
/// - Text rendering support for simple messages
/// - High-DPI aware
/// - Theme support
/// - Flicker-free rendering
/// - Low resource consumption
///
/// Original VB6: Controls/pdPictureBox.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdPictureBox : PhotoDemonControlBase
{
    #region Events

    /// <summary>
    /// Raised when the control needs custom rendering.
    /// The owner can draw directly to the provided canvas.
    /// </summary>
    public event EventHandler<PaintSurfaceEventArgs>? DrawMe;

    /// <summary>
    /// Raised after the control is resized
    /// </summary>
    public event EventHandler<SizeChangedEventArgs>? Resized;

    /// <summary>
    /// Raised when a window resize is detected (for special redraw needs)
    /// </summary>
    public event EventHandler? WindowResizeDetected;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Border color for drawing around image or control
    /// </summary>
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(PdPictureBox),
        Colors.Gray,
        propertyChanged: OnVisualPropertyChanged);

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// Draw border around the displayed image
    /// </summary>
    public static readonly BindableProperty DrawBorderAroundImageProperty = BindableProperty.Create(
        nameof(DrawBorderAroundImage),
        typeof(bool),
        typeof(PdPictureBox),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool DrawBorderAroundImage
    {
        get => (bool)GetValue(DrawBorderAroundImageProperty);
        set => SetValue(DrawBorderAroundImageProperty, value);
    }

    /// <summary>
    /// Draw border around the entire control
    /// </summary>
    public static readonly BindableProperty DrawBorderAroundControlProperty = BindableProperty.Create(
        nameof(DrawBorderAroundControl),
        typeof(bool),
        typeof(PdPictureBox),
        true,
        propertyChanged: OnVisualPropertyChanged);

    public bool DrawBorderAroundControl
    {
        get => (bool)GetValue(DrawBorderAroundControlProperty);
        set => SetValue(DrawBorderAroundControlProperty, value);
    }

    #endregion

    #region Private Fields

    private SKBitmap? _currentBitmap;
    private string? _currentText;
    private float _textSize = 12f;
    private bool _textBold = false;
    private bool _useNeutralBackground = false;

    #endregion

    #region Constructor

    public PdPictureBox()
    {
        // Non-interactive by design - cannot receive focus
        InputTransparent = false; // Still need to receive events for hover states

        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Request immediate redraw of the control
    /// </summary>
    public void RequestRedraw(bool paintImmediately = false)
    {
        InvalidateSurface();
    }

    /// <summary>
    /// Paint simple text onto the picture box (typically for warnings/errors)
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="fontSize">Font size (default 12)</param>
    /// <param name="isBold">Use bold font</param>
    /// <param name="refreshImmediately">Force immediate refresh</param>
    public void PaintText(string text, float fontSize = 12f, bool isBold = false, bool refreshImmediately = false)
    {
        _currentText = text;
        _textSize = fontSize;
        _textBold = isBold;
        _currentBitmap = null; // Clear any existing bitmap

        if (refreshImmediately)
        {
            InvalidateSurface();
        }
    }

    /// <summary>
    /// Copy a bitmap to the picture box with automatic centering and aspect ratio preservation
    /// </summary>
    /// <param name="bitmap">Source bitmap to display</param>
    /// <param name="doNotStretchIfSmaller">If true, don't enlarge images smaller than the control</param>
    /// <param name="useNeutralBackground">Use neutral gray background instead of theme</param>
    /// <param name="drawBorderAroundImage">Draw border around the image</param>
    /// <param name="drawBorderAroundControl">Draw border around the control</param>
    public void CopyBitmap(
        SKBitmap bitmap,
        bool doNotStretchIfSmaller = false,
        bool useNeutralBackground = false,
        bool? drawBorderAroundImage = null,
        bool? drawBorderAroundControl = null)
    {
        _currentBitmap = bitmap;
        _currentText = null; // Clear any existing text
        _useNeutralBackground = useNeutralBackground;

        // Update border properties if provided
        if (drawBorderAroundImage.HasValue)
            DrawBorderAroundImage = drawBorderAroundImage.Value;
        if (drawBorderAroundControl.HasValue)
            DrawBorderAroundControl = drawBorderAroundControl.Value;

        InvalidateSurface();
    }

    /// <summary>
    /// Clear the picture box content
    /// </summary>
    public void Clear()
    {
        _currentBitmap = null;
        _currentText = null;
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
        var backgroundColor = _useNeutralBackground
            ? new SKColor(127, 127, 127)
            : ConvertToSKColor(BackgroundColor ?? Colors.White);
        canvas.Clear(backgroundColor);

        // If DrawMe event has handlers, let the owner do custom rendering
        if (DrawMe != null)
        {
            DrawMe.Invoke(this, e);
        }
        // If we have text to display, render it
        else if (!string.IsNullOrEmpty(_currentText))
        {
            PaintTextInternal(canvas, info, _currentText, _textSize, _textBold);
        }
        // If we have a bitmap to display, render it
        else if (_currentBitmap != null)
        {
            PaintBitmapInternal(canvas, info, _currentBitmap);
        }
    }

    #endregion

    #region Private Methods

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (sender is View view)
        {
            Resized?.Invoke(this, new SizeChangedEventArgs(view.Width, view.Height));
        }
    }

    private void PaintTextInternal(SKCanvas canvas, SKImageInfo info, string text, float fontSize, bool isBold)
    {
        using var paint = new SKPaint
        {
            Color = ConvertToSKColor(IsEnabled ? Colors.Black : Colors.Gray),
            TextSize = fontSize * GetDisplayDensity(),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName(
                "Tahoma",
                isBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        // Measure text bounds
        var textBounds = new SKRect();
        paint.MeasureText(text, ref textBounds);

        // Center the text
        float x = info.Width / 2f;
        float y = (info.Height / 2f) - (textBounds.MidY);

        canvas.DrawText(text, x, y, paint);
    }

    private void PaintBitmapInternal(SKCanvas canvas, SKImageInfo info, SKBitmap bitmap)
    {
        float dstWidth = info.Width;
        float dstHeight = info.Height;
        float srcWidth = bitmap.Width;
        float srcHeight = bitmap.Height;

        if (srcWidth <= 0 || srcHeight <= 0) return;

        // Calculate aspect ratios
        float srcAspect = srcWidth / srcHeight;
        float dstAspect = dstWidth / dstHeight;

        // Calculate destination rectangle preserving aspect ratio
        float drawWidth, drawHeight;
        float previewX, previewY;

        if (srcAspect > dstAspect)
        {
            // Image is wider - fit to width
            drawWidth = dstWidth;
            drawHeight = dstWidth / srcAspect;
            previewX = 0;
            previewY = (dstHeight - drawHeight) / 2f;
        }
        else
        {
            // Image is taller - fit to height
            drawHeight = dstHeight;
            drawWidth = dstHeight * srcAspect;
            previewX = (dstWidth - drawWidth) / 2f;
            previewY = 0;
        }

        var destRect = new SKRect(previewX, previewY, previewX + drawWidth, previewY + drawHeight);

        // Draw the bitmap with high-quality scaling
        using (var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        })
        {
            canvas.DrawBitmap(bitmap, destRect, paint);
        }

        // Draw borders if requested
        if (DrawBorderAroundImage)
        {
            using var borderPaint = new SKPaint
            {
                Color = ConvertToSKColor(BorderColor),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true
            };
            canvas.DrawRect(destRect, borderPaint);
        }

        if (DrawBorderAroundControl)
        {
            using var borderPaint = new SKPaint
            {
                Color = ConvertToSKColor(BorderColor),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                IsAntialias = true
            };
            canvas.DrawRect(new SKRect(0, 0, info.Width - 1, info.Height - 1), borderPaint);
        }
    }

    private float GetDisplayDensity()
    {
        return (float)(DeviceDisplay.Current?.MainDisplayInfo.Density ?? 1.0);
    }

    #endregion
}
