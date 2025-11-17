using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Timers;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon progress bar control - migrated from VB6 pdProgressBar.ctl
///
/// Features:
/// - Standard progress mode (0 to Max)
/// - Marquee mode for indeterminate progress
/// - Theme support
/// - High-DPI aware
/// - Smooth animations
///
/// Original VB6: Controls/pdProgressBar.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 3 instances (used in status bars and long operations)
/// </summary>
public class PdProgressBar : PhotoDemonControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Current progress value
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(double),
        typeof(PdProgressBar),
        0.0,
        propertyChanged: OnValueChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Maximum progress value
    /// </summary>
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max),
        typeof(double),
        typeof(PdProgressBar),
        100.0,
        propertyChanged: OnValueChanged);

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>
    /// Whether to use marquee mode (indeterminate progress)
    /// </summary>
    public static readonly BindableProperty MarqueeModeProperty = BindableProperty.Create(
        nameof(MarqueeMode),
        typeof(bool),
        typeof(PdProgressBar),
        false,
        propertyChanged: OnMarqueeModeChanged);

    public bool MarqueeMode
    {
        get => (bool)GetValue(MarqueeModeProperty);
        set => SetValue(MarqueeModeProperty, value);
    }

    /// <summary>
    /// Progress bar color
    /// </summary>
    public static readonly BindableProperty ProgressColorProperty = BindableProperty.Create(
        nameof(ProgressColor),
        typeof(Color),
        typeof(PdProgressBar),
        null,
        propertyChanged: OnVisualPropertyChanged);

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    #endregion

    #region Private Fields

    private readonly SKCanvasView _canvasView;
    private readonly System.Timers.Timer _marqueeTimer;
    private double _marqueeOffset;
    private const double MarqueeSpeed = 2.0; // pixels per frame
    private const float MarqueeBlockWidth = 30;
    private const float MarqueeGap = 10;

    #endregion

    #region Constructor

    public PdProgressBar()
    {
        // Create canvas for rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        // Set up marquee timer
        _marqueeTimer = new System.Timers.Timer(16); // ~60 FPS
        _marqueeTimer.Elapsed += OnMarqueeTimerTick;

        // Progress bars don't accept focus
        IsTabStop = false;

        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnMarqueeTimerTick(object sender, ElapsedEventArgs e)
    {
        _marqueeOffset += MarqueeSpeed;

        // Wrap around
        if (_marqueeOffset > MarqueeBlockWidth + MarqueeGap)
        {
            _marqueeOffset = 0;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _canvasView?.InvalidateSurface();
        });
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;

        // Draw background and border
        DrawBackground(canvas, info);

        // Draw progress (standard or marquee)
        if (MarqueeMode)
        {
            DrawMarqueeProgress(canvas, info);
        }
        else
        {
            DrawStandardProgress(canvas, info);
        }
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdProgressBar progressBar)
        {
            progressBar._canvasView?.InvalidateSurface();
        }
    }

    private static void OnMarqueeModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdProgressBar progressBar)
        {
            bool isMarquee = (bool)newValue;
            if (isMarquee)
            {
                progressBar._marqueeTimer.Start();
            }
            else
            {
                progressBar._marqueeTimer.Stop();
                progressBar._marqueeOffset = 0;
            }

            progressBar._canvasView?.InvalidateSurface();
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdProgressBar progressBar)
        {
            progressBar._canvasView?.InvalidateSurface();
        }
    }

    #endregion

    #region Rendering

    private void DrawBackground(SKCanvas canvas, SKImageInfo info)
    {
        var rect = new SKRect(0, 0, info.Width, info.Height);

        // Draw background
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetBackgroundColor();
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRoundRect(rect, 2, 2, paint);
        }

        // Draw border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetBorderColor();
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            canvas.DrawRoundRect(rect, 2, 2, paint);
        }
    }

    private void DrawStandardProgress(SKCanvas canvas, SKImageInfo info)
    {
        if (Max <= 0 || Value <= 0)
            return;

        // Calculate progress width
        double percent = Math.Min(1.0, Value / Max);
        float progressWidth = (float)(info.Width * percent);

        if (progressWidth < 2)
            return;

        var progressRect = new SKRect(1, 1, progressWidth - 1, info.Height - 1);

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetProgressColor();
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRoundRect(progressRect, 2, 2, paint);
        }
    }

    private void DrawMarqueeProgress(SKCanvas canvas, SKImageInfo info)
    {
        var progressColor = GetProgressColor();
        var highlightColor = GetMarqueeHighlightColor();

        // Draw animated marquee blocks
        float blockWidth = MarqueeBlockWidth;
        float gap = MarqueeGap;
        float totalWidth = blockWidth + gap;
        float startX = (float)(-totalWidth + _marqueeOffset);

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Fill;

            // Draw blocks across the entire width
            for (float x = startX; x < info.Width; x += totalWidth)
            {
                var blockRect = new SKRect(x, 2, x + blockWidth, info.Height - 2);

                // Clip to progress bar bounds
                if (blockRect.Right < 0 || blockRect.Left > info.Width)
                    continue;

                // Create gradient for each block
                paint.Shader = SKShader.CreateLinearGradient(
                    new SKPoint(blockRect.Left, blockRect.MidY),
                    new SKPoint(blockRect.Right, blockRect.MidY),
                    new[] { progressColor.ToSKColor(), highlightColor.ToSKColor(), progressColor.ToSKColor() },
                    new[] { 0.0f, 0.5f, 1.0f },
                    SKShaderTileMode.Clamp);

                canvas.DrawRoundRect(blockRect, 2, 2, paint);
            }
        }
    }

    private SKColor GetBackgroundColor()
    {
        // This will be integrated with PhotoDemon's theme engine
        return new SKColor(240, 240, 240);
    }

    private SKColor GetBorderColor()
    {
        // This will be integrated with PhotoDemon's theme engine
        return new SKColor(180, 180, 180);
    }

    private Color GetProgressColor()
    {
        if (ProgressColor != null)
            return ProgressColor;

        // Default progress color (blue)
        return Color.FromRgb(0, 120, 215);
    }

    private Color GetMarqueeHighlightColor()
    {
        // Lighter version of progress color for marquee effect
        var baseColor = GetProgressColor();
        return Color.FromRgba(
            baseColor.Red,
            baseColor.Green,
            baseColor.Blue,
            0.5);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Reset progress to zero
    /// </summary>
    public void Reset()
    {
        Value = 0;
    }

    /// <summary>
    /// Increment progress by specified amount
    /// </summary>
    public void Increment(double amount)
    {
        Value = Math.Min(Max, Value + amount);
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

    #region Cleanup

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        // Stop timer when control is removed
        if (Handler == null)
        {
            _marqueeTimer?.Stop();
        }
    }

    #endregion
}
