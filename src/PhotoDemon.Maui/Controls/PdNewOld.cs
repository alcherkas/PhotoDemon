using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon New/Old comparison control - migrated from VB6 pdNewOld.ctl
///
/// Features:
/// - Side-by-side display of "new" vs "old" values
/// - Click on "old" item to restore it as the "new" value
/// - Owner-drawn rendering via DrawNewItem and DrawOldItem events
/// - Hover state on old item (shows it's clickable)
/// - Theme support
/// - Used primarily in color selection dialogs
///
/// Original VB6: Controls/pdNewOld.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdNewOld : PhotoDemonControlBase
{
    #region Events

    /// <summary>
    /// Raised when the user clicks the "old" item to restore it
    /// </summary>
    public event EventHandler? OldItemClicked;

    /// <summary>
    /// Raised to allow custom rendering of the "new" item area
    /// </summary>
    public event EventHandler<PaintSurfaceEventArgs>? DrawNewItem;

    /// <summary>
    /// Raised to allow custom rendering of the "old" item area
    /// </summary>
    public event EventHandler<PaintSurfaceEventArgs>? DrawOldItem;

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Font size for the "new" and "old" captions
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(float),
        typeof(PdNewOld),
        12f,
        propertyChanged: OnVisualPropertyChanged);

    public new float FontSize
    {
        get => (float)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Border color for item rectangles
    /// </summary>
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(PdNewOld),
        Colors.Gray,
        propertyChanged: OnVisualPropertyChanged);

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    #endregion

    #region Private Fields

    private SKRect _newItemRect;
    private SKRect _oldItemRect;
    private bool _oldItemIsHovered = false;

    private const string NewCaptionText = "new:";
    private const string OldCaptionText = "original:";
    private const float TextPadding = 4f;

    #endregion

    #region Constructor

    public PdNewOld()
    {
        // Set up hover tracking
        PointerGestureRecognizer pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += OnPointerEntered;
        pointerGesture.PointerExited += OnPointerExited;
        pointerGesture.PointerMoved += OnPointerMoved;
        GestureRecognizers.Add(pointerGesture);

        // Set up tap gesture for clicking the old item
        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        GestureRecognizers.Add(tapGesture);

        SizeChanged += OnSizeChanged;
    }

    #endregion

    #region Event Handlers

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        UpdateControlLayout();
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
#if WINDOWS
        Microsoft.UI.Input.InputCursor.Current = Microsoft.UI.Input.InputCursor.CreateFromCoreCursor(
            new Microsoft.UI.Xaml.CoreCursor(Microsoft.UI.Xaml.CoreCursorType.Hand, 0));
#endif
        InvalidateSurface();
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _oldItemIsHovered = false;
#if WINDOWS
        Microsoft.UI.Input.InputCursor.Current = Microsoft.UI.Input.InputCursor.CreateFromCoreCursor(
            new Microsoft.UI.Xaml.CoreCursor(Microsoft.UI.Xaml.CoreCursorType.Arrow, 0));
#endif
        InvalidateSurface();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetPosition(this) is Point point)
        {
            bool wasHovered = _oldItemIsHovered;
            _oldItemIsHovered = _oldItemRect.Contains((float)point.X, (float)point.Y);

#if WINDOWS
            if (_oldItemIsHovered)
            {
                Microsoft.UI.Input.InputCursor.Current = Microsoft.UI.Input.InputCursor.CreateFromCoreCursor(
                    new Microsoft.UI.Xaml.CoreCursor(Microsoft.UI.Xaml.CoreCursorType.Hand, 0));
            }
            else
            {
                Microsoft.UI.Input.InputCursor.Current = Microsoft.UI.Input.InputCursor.CreateFromCoreCursor(
                    new Microsoft.UI.Xaml.CoreCursor(Microsoft.UI.Xaml.CoreCursorType.Arrow, 0));
            }
#endif

            if (wasHovered != _oldItemIsHovered)
            {
                InvalidateSurface();
            }
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (!IsEnabled) return;

        if (e.GetPosition(this) is Point point)
        {
            // Only raise event if clicking the old item
            if (_oldItemRect.Contains((float)point.X, (float)point.Y))
            {
                OldItemClicked?.Invoke(this, EventArgs.Empty);
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

        // Update layout if needed
        if (_newItemRect.Width == 0)
        {
            UpdateControlLayout();
        }

        // Draw the new item section
        PaintNewItemSection(canvas, info);

        // Draw the old item section
        PaintOldItemSection(canvas, info);
    }

    #endregion

    #region Private Methods

    private void UpdateControlLayout()
    {
        float width = (float)Width;
        float height = (float)Height;

        if (width <= 0 || height <= 0) return;

        // Measure the caption text to determine layout
        using var paint = new SKPaint
        {
            TextSize = FontSize * GetDisplayDensity(),
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Tahoma")
        };

        float newCaptionWidth = paint.MeasureText(NewCaptionText);
        float oldCaptionWidth = paint.MeasureText(OldCaptionText);
        float maxCaptionWidth = Math.Max(newCaptionWidth, oldCaptionWidth) + (TextPadding * 2);

        // Split the control vertically in half
        float halfHeight = height / 2f;

        // New item rect (top half)
        _newItemRect = new SKRect(
            maxCaptionWidth,
            1,
            width - 1,
            halfHeight - 1);

        // Old item rect (bottom half)
        _oldItemRect = new SKRect(
            maxCaptionWidth,
            halfHeight,
            width - 1,
            height - 2);

        InvalidateSurface();
    }

    private void PaintNewItemSection(SKCanvas canvas, SKImageInfo info)
    {
        float density = GetDisplayDensity();

        // Draw "new:" caption
        using (var textPaint = new SKPaint
        {
            Color = ConvertToSKColor(IsEnabled ? Colors.Black : Colors.Gray),
            TextSize = FontSize * density,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Tahoma"),
            TextAlign = SKTextAlign.Left
        })
        {
            var textBounds = new SKRect();
            textPaint.MeasureText(NewCaptionText, ref textBounds);
            float textY = _newItemRect.Top + (_newItemRect.Height / 2f) - textBounds.MidY;
            canvas.DrawText(NewCaptionText, TextPadding, textY, textPaint);
        }

        // Draw the new item rectangle border
        using (var borderPaint = new SKPaint
        {
            Color = ConvertToSKColor(BorderColor),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            IsAntialias = true
        })
        {
            canvas.DrawRect(_newItemRect, borderPaint);
        }

        // Allow owner to draw custom content
        if (DrawNewItem != null)
        {
            // Create a clipped region for the owner to draw in
            canvas.Save();
            canvas.ClipRect(_newItemRect);

            var args = new SKPaintSurfaceEventArgs(
                canvas.Surface,
                new SKImageInfo((int)_newItemRect.Width, (int)_newItemRect.Height));
            DrawNewItem.Invoke(this, args);

            canvas.Restore();
        }
    }

    private void PaintOldItemSection(SKCanvas canvas, SKImageInfo info)
    {
        float density = GetDisplayDensity();

        // Draw "original:" caption
        using (var textPaint = new SKPaint
        {
            Color = ConvertToSKColor(IsEnabled ? Colors.Black : Colors.Gray),
            TextSize = FontSize * density,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Tahoma"),
            TextAlign = SKTextAlign.Left
        })
        {
            var textBounds = new SKRect();
            textPaint.MeasureText(OldCaptionText, ref textBounds);
            float textY = _oldItemRect.Top + (_oldItemRect.Height / 2f) - textBounds.MidY;
            canvas.DrawText(OldCaptionText, TextPadding, textY, textPaint);
        }

        // Draw the old item rectangle border (highlight if hovered)
        using (var borderPaint = new SKPaint
        {
            Color = _oldItemIsHovered
                ? ConvertToSKColor(Colors.Blue)
                : ConvertToSKColor(BorderColor),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = _oldItemIsHovered ? 2f : 1f,
            IsAntialias = true
        })
        {
            canvas.DrawRect(_oldItemRect, borderPaint);
        }

        // Allow owner to draw custom content
        if (DrawOldItem != null)
        {
            // Create a clipped region for the owner to draw in
            canvas.Save();
            canvas.ClipRect(_oldItemRect);

            var args = new SKPaintSurfaceEventArgs(
                canvas.Surface,
                new SKImageInfo((int)_oldItemRect.Width, (int)_oldItemRect.Height));
            DrawOldItem.Invoke(this, args);

            canvas.Restore();
        }
    }

    private float GetDisplayDensity()
    {
        return (float)(DeviceDisplay.Current?.MainDisplayInfo.Density ?? 1.0);
    }

    #endregion
}
