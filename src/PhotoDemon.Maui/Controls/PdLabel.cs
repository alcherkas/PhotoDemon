using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon custom label control - migrated from VB6 pdLabel.ctl
///
/// Features:
/// - Unicode support (native in .NET)
/// - Auto-sizing based on caption length
/// - Auto font sizing to fit within control
/// - Word wrap support
/// - Alignment control
/// - Custom colors
/// - Theme support
/// - High-DPI aware
///
/// Original VB6: Controls/pdLabel.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 330 instances across the application
/// </summary>
public class PdLabel : PhotoDemonControlBase
{
    #region Enums

    public enum LabelLayout
    {
        /// <summary>
        /// Shrink font to fit caption within control bounds
        /// </summary>
        AutoFitCaption = 0,

        /// <summary>
        /// Shrink font + word wrap to fit within control
        /// </summary>
        AutoFitCaptionPlusWordWrap = 1,

        /// <summary>
        /// Grow control to fit caption
        /// </summary>
        AutoSizeControl = 2,

        /// <summary>
        /// Grow control + word wrap
        /// </summary>
        AutoSizeControlPlusWordWrap = 3
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Text alignment (Left, Center, Right)
    /// </summary>
    public static readonly BindableProperty AlignmentProperty = BindableProperty.Create(
        nameof(Alignment),
        typeof(TextAlignment),
        typeof(PdLabel),
        TextAlignment.Start,
        propertyChanged: OnVisualPropertyChanged);

    public TextAlignment Alignment
    {
        get => (TextAlignment)GetValue(AlignmentProperty);
        set => SetValue(AlignmentProperty, value);
    }

    /// <summary>
    /// Label layout mode
    /// </summary>
    public static readonly BindableProperty LayoutProperty = BindableProperty.Create(
        nameof(Layout),
        typeof(LabelLayout),
        typeof(PdLabel),
        LabelLayout.AutoFitCaption,
        propertyChanged: OnLayoutPropertyChanged);

    public LabelLayout Layout
    {
        get => (LabelLayout)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>
    /// Custom foreground color
    /// </summary>
    public static readonly BindableProperty ForeColorProperty = BindableProperty.Create(
        nameof(ForeColor),
        typeof(Color),
        typeof(PdLabel),
        Colors.Black,
        propertyChanged: OnVisualPropertyChanged);

    public Color ForeColor
    {
        get => (Color)GetValue(ForeColorProperty);
        set => SetValue(ForeColorProperty, value);
    }

    /// <summary>
    /// Use custom foreground color instead of theme
    /// </summary>
    public static readonly BindableProperty UseCustomForeColorProperty = BindableProperty.Create(
        nameof(UseCustomForeColor),
        typeof(bool),
        typeof(PdLabel),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool UseCustomForeColor
    {
        get => (bool)GetValue(UseCustomForeColorProperty);
        set => SetValue(UseCustomForeColorProperty, value);
    }

    /// <summary>
    /// Custom background color
    /// </summary>
    public static readonly BindableProperty BackColorProperty = BindableProperty.Create(
        nameof(BackColor),
        typeof(Color),
        typeof(PdLabel),
        Colors.Transparent,
        propertyChanged: OnVisualPropertyChanged);

    public Color BackColor
    {
        get => (Color)GetValue(BackColorProperty);
        set => SetValue(BackColorProperty, value);
    }

    /// <summary>
    /// Use custom background color instead of theme
    /// </summary>
    public static readonly BindableProperty UseCustomBackColorProperty = BindableProperty.Create(
        nameof(UseCustomBackColor),
        typeof(bool),
        typeof(PdLabel),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool UseCustomBackColor
    {
        get => (bool)GetValue(UseCustomBackColorProperty);
        set => SetValue(UseCustomBackColorProperty, value);
    }

    /// <summary>
    /// Font size
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(PdLabel),
        14.0,
        propertyChanged: OnVisualPropertyChanged);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Whether word wrap is enabled
    /// </summary>
    public static readonly BindableProperty WordWrapProperty = BindableProperty.Create(
        nameof(WordWrap),
        typeof(bool),
        typeof(PdLabel),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool WordWrap
    {
        get => (bool)GetValue(WordWrapProperty);
        set => SetValue(WordWrapProperty, value);
    }

    #endregion

    #region Private Fields

    private readonly SKCanvasView _canvasView;
    private bool _fitFailure;
    private double _calculatedFontSize;

    #endregion

    #region Constructor

    public PdLabel()
    {
        // Create canvas for rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        // Labels don't accept focus by design
        IsTabStop = false;

        // Initialize
        _calculatedFontSize = FontSize;
        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;

        // Draw background if custom color is set
        if (UseCustomBackColor)
        {
            using (var paint = new SKPaint())
            {
                paint.Color = BackColor.ToSKColor();
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), paint);
            }
        }

        // Draw caption text
        if (!string.IsNullOrEmpty(Caption))
        {
            DrawCaption(canvas, info);
        }
    }

    #endregion

    #region Rendering

    private void DrawCaption(SKCanvas canvas, SKImageInfo info)
    {
        var textColor = UseCustomForeColor
            ? ForeColor.ToSKColor()
            : GetThemeForeColor();

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = textColor;
            paint.Typeface = SKTypeface.FromFamilyName("Segoe UI");

            // Determine font size based on layout mode
            DetermineFontSize(paint, info.Width, info.Height);
            paint.TextSize = (float)_calculatedFontSize * 1.33f; // Convert to pixels

            // Set text alignment
            paint.TextAlign = GetSKTextAlign();

            // Draw text based on layout
            if (Layout == LabelLayout.AutoFitCaptionPlusWordWrap ||
                Layout == LabelLayout.AutoSizeControlPlusWordWrap)
            {
                DrawWrappedText(canvas, paint, info);
            }
            else
            {
                DrawSingleLineText(canvas, paint, info);
            }
        }
    }

    private void DetermineFontSize(SKPaint paint, float width, float height)
    {
        if (Layout == LabelLayout.AutoFitCaption ||
            Layout == LabelLayout.AutoFitCaptionPlusWordWrap)
        {
            // Shrink font to fit within control bounds
            _calculatedFontSize = FontSize;
            paint.TextSize = (float)_calculatedFontSize * 1.33f;

            var textBounds = new SKRect();
            paint.MeasureText(Caption, ref textBounds);

            // Keep shrinking until it fits or we hit minimum size
            const double minFontSize = 6.0;
            while (textBounds.Width > width && _calculatedFontSize > minFontSize)
            {
                _calculatedFontSize -= 0.5;
                paint.TextSize = (float)_calculatedFontSize * 1.33f;
                paint.MeasureText(Caption, ref textBounds);
            }

            // Check for fit failure
            _fitFailure = textBounds.Width > width;
        }
        else
        {
            // Use specified font size
            _calculatedFontSize = FontSize;
            _fitFailure = false;
        }
    }

    private void DrawSingleLineText(SKCanvas canvas, SKPaint paint, SKImageInfo info)
    {
        var displayText = Caption;

        // Add ellipsis if fit failed
        if (_fitFailure)
        {
            displayText = TruncateWithEllipsis(displayText, paint, info.Width);
        }

        // Calculate position
        float x = GetTextX(info.Width, paint.TextAlign);

        var textBounds = new SKRect();
        paint.MeasureText(displayText, ref textBounds);
        float y = (info.Height - textBounds.Height) / 2 - textBounds.Top;

        canvas.DrawText(displayText, x, y, paint);
    }

    private void DrawWrappedText(SKCanvas canvas, SKPaint paint, SKImageInfo info)
    {
        // Simple word wrapping implementation
        var words = Caption.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine)
                ? word
                : $"{currentLine} {word}";

            var textBounds = new SKRect();
            paint.MeasureText(testLine, ref textBounds);

            if (textBounds.Width > info.Width && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        // Draw each line
        var textHeight = paint.FontMetrics.Descent - paint.FontMetrics.Ascent;
        var lineSpacing = textHeight * 1.2f;
        var totalHeight = lines.Count * lineSpacing;
        var startY = (info.Height - totalHeight) / 2 - paint.FontMetrics.Ascent;

        for (int i = 0; i < lines.Count; i++)
        {
            float x = GetTextX(info.Width, paint.TextAlign);
            float y = startY + (i * lineSpacing);
            canvas.DrawText(lines[i], x, y, paint);
        }
    }

    private string TruncateWithEllipsis(string text, SKPaint paint, float maxWidth)
    {
        const string ellipsis = "...";

        var textBounds = new SKRect();
        paint.MeasureText(text, ref textBounds);

        if (textBounds.Width <= maxWidth)
            return text;

        // Binary search for the right length
        int left = 0;
        int right = text.Length;
        string result = text;

        while (left < right)
        {
            int mid = (left + right + 1) / 2;
            var truncated = text.Substring(0, mid) + ellipsis;
            paint.MeasureText(truncated, ref textBounds);

            if (textBounds.Width <= maxWidth)
            {
                result = truncated;
                left = mid;
            }
            else
            {
                right = mid - 1;
            }
        }

        return result;
    }

    private float GetTextX(float width, SKTextAlign align)
    {
        return align switch
        {
            SKTextAlign.Left => 4, // Small padding
            SKTextAlign.Center => width / 2,
            SKTextAlign.Right => width - 4, // Small padding
            _ => 4
        };
    }

    private SKTextAlign GetSKTextAlign()
    {
        return Alignment switch
        {
            TextAlignment.Start => SKTextAlign.Left,
            TextAlignment.Center => SKTextAlign.Center,
            TextAlignment.End => SKTextAlign.Right,
            _ => SKTextAlign.Left
        };
    }

    private SKColor GetThemeForeColor()
    {
        // This will be integrated with PhotoDemon's theme engine
        return new SKColor(0, 0, 0); // Default black
    }

    #endregion

    #region Property Changed Handlers

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdLabel label)
        {
            label._canvasView?.InvalidateSurface();
        }
    }

    private static void OnLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdLabel label)
        {
            // Update word wrap based on layout
            var layout = (LabelLayout)newValue;
            label.WordWrap = layout == LabelLayout.AutoFitCaptionPlusWordWrap ||
                            layout == LabelLayout.AutoSizeControlPlusWordWrap;

            label._canvasView?.InvalidateSurface();
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
        // Theme application will be integrated with PhotoDemon's theme engine
        UpdateVisualAppearance();
    }

    #endregion

    #region Auto-sizing

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Caption) || propertyName == nameof(FontSize))
        {
            if (Layout == LabelLayout.AutoSizeControl ||
                Layout == LabelLayout.AutoSizeControlPlusWordWrap)
            {
                // Request size measurement
                InvalidateMeasure();
            }
        }
    }

    protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
    {
        if (Layout == LabelLayout.AutoSizeControl ||
            Layout == LabelLayout.AutoSizeControlPlusWordWrap)
        {
            // Measure text to determine required size
            using (var paint = new SKPaint())
            {
                paint.TextSize = (float)FontSize * 1.33f;
                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI");

                var textBounds = new SKRect();
                paint.MeasureText(Caption ?? "", ref textBounds);

                var width = textBounds.Width + 8; // Add padding
                var height = textBounds.Height + 8; // Add padding

                return new SizeRequest(new Size(width, height));
            }
        }

        return base.OnMeasure(widthConstraint, heightConstraint);
    }

    #endregion
}
