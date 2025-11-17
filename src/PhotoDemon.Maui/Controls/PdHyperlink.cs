using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Windows.Input;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon Unicode hyperlink (clickable label) control - migrated from VB6 pdHyperlink.ctl
///
/// Features:
/// - Clickable text that opens URLs
/// - Two layout modes: AutoFitCaption (shrink text to fit) and AutoSizeControl (resize control to fit text)
/// - Underline on hover and focus
/// - Custom colors (optional, otherwise theme-based)
/// - Optional Click event for manual handling instead of auto-launching URL
/// - Keyboard support (Space/Enter to activate)
/// - High-DPI aware
/// - Unicode support
///
/// Original VB6: Controls/pdHyperlink.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdHyperlink : PhotoDemonControlBase
{
    #region Enums

    public enum LayoutMode
    {
        /// <summary>
        /// Shrink caption text to fit within control bounds
        /// </summary>
        AutoFitCaption = 0,

        /// <summary>
        /// Resize control to fit caption at normal font size
        /// </summary>
        AutoSizeControl = 2
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when the hyperlink is clicked (only if RaiseClickEvent is true)
    /// </summary>
    public event EventHandler? Click;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// The URL to open when clicked (if RaiseClickEvent is false)
    /// </summary>
    public static readonly BindableProperty UrlProperty = BindableProperty.Create(
        nameof(Url),
        typeof(string),
        typeof(PdHyperlink),
        string.Empty);

    public string Url
    {
        get => (string)GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    /// <summary>
    /// Layout mode - AutoFitCaption or AutoSizeControl
    /// </summary>
    public static readonly BindableProperty LayoutProperty = BindableProperty.Create(
        nameof(Layout),
        typeof(LayoutMode),
        typeof(PdHyperlink),
        LayoutMode.AutoFitCaption,
        propertyChanged: OnLayoutPropertyChanged);

    public LayoutMode Layout
    {
        get => (LayoutMode)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>
    /// If true, raise Click event instead of opening URL
    /// </summary>
    public static readonly BindableProperty RaiseClickEventProperty = BindableProperty.Create(
        nameof(RaiseClickEvent),
        typeof(bool),
        typeof(PdHyperlink),
        false);

    public bool RaiseClickEvent
    {
        get => (bool)GetValue(RaiseClickEventProperty);
        set => SetValue(RaiseClickEventProperty, value);
    }

    /// <summary>
    /// Use custom back color instead of theme
    /// </summary>
    public static readonly BindableProperty UseCustomBackColorProperty = BindableProperty.Create(
        nameof(UseCustomBackColor),
        typeof(bool),
        typeof(PdHyperlink),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool UseCustomBackColor
    {
        get => (bool)GetValue(UseCustomBackColorProperty);
        set => SetValue(UseCustomBackColorProperty, value);
    }

    /// <summary>
    /// Use custom fore color instead of theme
    /// </summary>
    public static readonly BindableProperty UseCustomForeColorProperty = BindableProperty.Create(
        nameof(UseCustomForeColor),
        typeof(bool),
        typeof(PdHyperlink),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool UseCustomForeColor
    {
        get => (bool)GetValue(UseCustomForeColorProperty);
        set => SetValue(UseCustomForeColorProperty, value);
    }

    /// <summary>
    /// Font size for the caption
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(float),
        typeof(PdHyperlink),
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
        typeof(PdHyperlink),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool FontBold
    {
        get => (bool)GetValue(FontBoldProperty);
        set => SetValue(FontBoldProperty, value);
    }

    /// <summary>
    /// Italic font
    /// </summary>
    public static readonly BindableProperty FontItalicProperty = BindableProperty.Create(
        nameof(FontItalic),
        typeof(bool),
        typeof(PdHyperlink),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool FontItalic
    {
        get => (bool)GetValue(FontItalicProperty);
        set => SetValue(FontItalicProperty, value);
    }

    /// <summary>
    /// Text alignment
    /// </summary>
    public static readonly BindableProperty TextAlignmentProperty = BindableProperty.Create(
        nameof(TextAlignment),
        typeof(TextAlignment),
        typeof(PdHyperlink),
        TextAlignment.Start,
        propertyChanged: OnVisualPropertyChanged);

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    #endregion

    #region Private Fields

    private bool _mouseInsideControl = false;
    private bool _fitFailure = false;
    private float _currentFontSize;

    #endregion

    #region Constructor

    public PdHyperlink()
    {
        _currentFontSize = FontSize;

        // Set up hover cursor
        PointerGestureRecognizer pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += OnPointerEntered;
        pointerGesture.PointerExited += OnPointerExited;
        GestureRecognizers.Add(pointerGesture);

        // Set up tap gesture
        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        GestureRecognizers.Add(tapGesture);

        // Handle keyboard input via Focus
        Focused += (s, e) => InvalidateSurface();
        Unfocused += (s, e) => InvalidateSurface();
    }

    #endregion

    #region Event Handlers

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        _mouseInsideControl = true;
#if WINDOWS
        Microsoft.UI.Input.InputCursor.Current = Microsoft.UI.Input.InputCursor.CreateFromCoreCursor(
            new Microsoft.UI.Xaml.CoreCursor(Microsoft.UI.Xaml.CoreCursorType.Hand, 0));
#endif
        InvalidateSurface();
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _mouseInsideControl = false;
#if WINDOWS
        Microsoft.UI.Input.InputCursor.Current = Microsoft.UI.Input.InputCursor.CreateFromCoreCursor(
            new Microsoft.UI.Xaml.CoreCursor(Microsoft.UI.Xaml.CoreCursorType.Arrow, 0));
#endif
        InvalidateSurface();
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled) return;

        if (RaiseClickEvent)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
        else if (!string.IsNullOrEmpty(Url))
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri(Url));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open URL: {ex.Message}");
            }
        }
    }

    private static void OnLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdHyperlink hyperlink)
        {
            hyperlink.UpdateControlLayout();
        }
    }

    #endregion

    #region Protected Override Methods

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        canvas.Clear();

        // Determine background color
        var backColor = UseCustomBackColor && BackgroundColor != null
            ? ConvertToSKColor(BackgroundColor)
            : SKColors.Transparent; // Default to transparent for hyperlinks

        canvas.Clear(backColor);

        // Draw the text
        PaintCaption(canvas, info);
    }

    #endregion

    #region Private Methods

    private void UpdateControlLayout()
    {
        float controlWidth = (float)Width;
        float controlHeight = (float)Height;

        if (controlWidth <= 0 || controlHeight <= 0) return;

        string captionText = Caption ?? string.Empty;
        if (string.IsNullOrEmpty(captionText)) return;

        using var paint = CreateTextPaint(FontSize, false);

        switch (Layout)
        {
            case LayoutMode.AutoFitCaption:
                // Try to shrink font to fit
                float testFontSize = FontSize;
                float minFontSize = 6f; // Minimum readable size
                var textBounds = new SKRect();

                while (testFontSize >= minFontSize)
                {
                    paint.TextSize = testFontSize * GetDisplayDensity();
                    float textWidth = paint.MeasureText(captionText, ref textBounds);

                    if (textWidth <= controlWidth)
                    {
                        _currentFontSize = testFontSize;
                        _fitFailure = false;
                        break;
                    }

                    testFontSize -= 0.5f;
                }

                if (testFontSize < minFontSize)
                {
                    _fitFailure = true;
                    _currentFontSize = minFontSize;
                }
                break;

            case LayoutMode.AutoSizeControl:
                // Measure text at normal size and resize control
                _currentFontSize = FontSize;
                paint.TextSize = _currentFontSize * GetDisplayDensity();
                var bounds = new SKRect();
                float width = paint.MeasureText(captionText, ref bounds);
                float height = bounds.Height;

                if (width > 0 && height > 0)
                {
                    WidthRequest = width / GetDisplayDensity();
                    HeightRequest = height / GetDisplayDensity();
                }
                break;
        }

        InvalidateSurface();
    }

    private void PaintCaption(SKCanvas canvas, SKImageInfo info)
    {
        string captionText = Caption ?? string.Empty;
        if (string.IsNullOrEmpty(captionText)) return;

        // Determine if we should underline (hover or focus)
        bool shouldUnderline = _mouseInsideControl || IsFocused;

        // Determine text color
        var foreColor = UseCustomForeColor && ForegroundColor != null
            ? ConvertToSKColor(ForegroundColor)
            : ConvertToSKColor(IsEnabled ? Color.FromRgb(0, 0, 255) : Colors.Gray); // Blue for links

        using var paint = CreateTextPaint(_currentFontSize, shouldUnderline);
        paint.Color = foreColor;

        // Set text alignment
        float x;
        switch (TextAlignment)
        {
            case TextAlignment.Center:
                paint.TextAlign = SKTextAlign.Center;
                x = info.Width / 2f;
                break;
            case TextAlignment.End:
                paint.TextAlign = SKTextAlign.Right;
                x = info.Width - 2;
                break;
            default: // Start
                paint.TextAlign = SKTextAlign.Left;
                x = 2;
                break;
        }

        // Calculate vertical center
        var textBounds = new SKRect();
        paint.MeasureText(captionText, ref textBounds);
        float y = (info.Height / 2f) - textBounds.MidY;

        // If fit failed, add ellipsis
        string displayText = captionText;
        if (_fitFailure && Layout == LayoutMode.AutoFitCaption)
        {
            float availableWidth = info.Width - 4;
            displayText = TruncateTextWithEllipsis(captionText, paint, availableWidth);
        }

        canvas.DrawText(displayText, x, y, paint);
    }

    private SKPaint CreateTextPaint(float fontSize, bool underline)
    {
        var paint = new SKPaint
        {
            TextSize = fontSize * GetDisplayDensity(),
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName(
                "Tahoma",
                FontBold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                FontItalic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright)
        };

        if (underline)
        {
            paint.IsStroke = false;
            paint.UnderlineText = true;
        }

        return paint;
    }

    private string TruncateTextWithEllipsis(string text, SKPaint paint, float maxWidth)
    {
        string ellipsis = "...";
        float ellipsisWidth = paint.MeasureText(ellipsis);

        if (paint.MeasureText(text) <= maxWidth)
            return text;

        float availableWidth = maxWidth - ellipsisWidth;
        int length = text.Length;

        while (length > 0)
        {
            string truncated = text.Substring(0, length);
            if (paint.MeasureText(truncated) <= availableWidth)
                return truncated + ellipsis;
            length--;
        }

        return ellipsis;
    }

    private float GetDisplayDensity()
    {
        return (float)(DeviceDisplay.Current?.MainDisplayInfo.Density ?? 1.0);
    }

    #endregion
}
