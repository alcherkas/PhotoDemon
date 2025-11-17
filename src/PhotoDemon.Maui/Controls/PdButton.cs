using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Windows.Input;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon custom button control - migrated from VB6 pdButton.ctl
///
/// Features:
/// - Unicode caption support with access keys
/// - Custom rendering modes (Normal, OwnerDrawn)
/// - Theme support
/// - High-DPI aware
/// - Custom colors
/// - Image support
/// - Drag/drop support
/// - Focus indicators
///
/// Original VB6: Controls/pdButton.ctl
/// Documentation: docs/ui/control-mapping.md
/// </summary>
public class PdButton : PhotoDemonControlBase
{
    #region Enums

    public enum RenderMode
    {
        Normal = 0,
        OwnerDrawn = 1
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Command executed when button is clicked
    /// </summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(PdButton),
        null);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Command parameter
    /// </summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(PdButton),
        null);

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Button background color
    /// </summary>
    public static readonly BindableProperty ButtonBackColorProperty = BindableProperty.Create(
        nameof(ButtonBackColor),
        typeof(Color),
        typeof(PdButton),
        null,
        propertyChanged: OnVisualPropertyChanged);

    public Color ButtonBackColor
    {
        get => (Color)GetValue(ButtonBackColorProperty);
        set => SetValue(ButtonBackColorProperty, value);
    }

    /// <summary>
    /// Use custom back color instead of theme
    /// </summary>
    public static readonly BindableProperty UseCustomBackColorProperty = BindableProperty.Create(
        nameof(UseCustomBackColor),
        typeof(bool),
        typeof(PdButton),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool UseCustomBackColor
    {
        get => (bool)GetValue(UseCustomBackColorProperty);
        set => SetValue(UseCustomBackColorProperty, value);
    }

    /// <summary>
    /// Button render mode
    /// </summary>
    public static readonly BindableProperty ButtonRenderModeProperty = BindableProperty.Create(
        nameof(ButtonRenderMode),
        typeof(RenderMode),
        typeof(PdButton),
        RenderMode.Normal,
        propertyChanged: OnVisualPropertyChanged);

    public RenderMode ButtonRenderMode
    {
        get => (RenderMode)GetValue(ButtonRenderModeProperty);
        set => SetValue(ButtonRenderModeProperty, value);
    }

    /// <summary>
    /// Font size for the caption
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(PdButton),
        14.0,
        propertyChanged: OnVisualPropertyChanged);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when button is clicked
    /// </summary>
    public event EventHandler Clicked;

    /// <summary>
    /// Raised when button receives focus
    /// </summary>
    public event EventHandler GotFocusAPI;

    /// <summary>
    /// Raised when button loses focus
    /// </summary>
    public event EventHandler LostFocusAPI;

    /// <summary>
    /// Raised for owner-drawn rendering
    /// </summary>
    public event EventHandler<SKPaintSurfaceEventArgs> DrawButton;

    #endregion

    #region Private Fields

    private readonly SKCanvasView _canvasView;
    private bool _isMouseOver;
    private bool _isPressed;
    private bool _showFocusRect;

    #endregion

    #region Constructor

    public PdButton()
    {
        // Create canvas for custom rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        // Set up gesture recognizers
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _canvasView.GestureRecognizers.Add(tapGesture);

        // Set up pointer events for hover effects
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, e) =>
        {
            _isMouseOver = true;
            _canvasView.InvalidateSurface();
        };
        pointerGesture.PointerExited += (s, e) =>
        {
            _isMouseOver = false;
            _isPressed = false;
            _canvasView.InvalidateSurface();
        };
        pointerGesture.PointerPressed += (s, e) =>
        {
            _isPressed = true;
            _canvasView.InvalidateSurface();
        };
        pointerGesture.PointerReleased += (s, e) =>
        {
            _isPressed = false;
            _canvasView.InvalidateSurface();
        };
        _canvasView.GestureRecognizers.Add(pointerGesture);

        // Apply default styling
        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnTapped(object sender, EventArgs e)
    {
        if (!IsEnabled)
            return;

        // Raise click event
        Clicked?.Invoke(this, EventArgs.Empty);

        // Execute command if bound
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (ButtonRenderMode == RenderMode.OwnerDrawn)
        {
            // Allow custom drawing via event
            DrawButton?.Invoke(this, e);
        }
        else
        {
            // Standard button rendering
            DrawStandardButton(canvas, e.Info);
        }
    }

    #endregion

    #region Rendering

    private void DrawStandardButton(SKCanvas canvas, SKImageInfo info)
    {
        var rect = new SKRect(0, 0, info.Width, info.Height);

        // Determine button state colors
        var fillColor = GetButtonFillColor();
        var borderColor = GetButtonBorderColor();
        var textColor = GetCaptionColor();

        // Draw button background
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Fill;
            paint.Color = fillColor;

            // Slightly rounded corners for modern look
            canvas.DrawRoundRect(rect, 4, 4, paint);
        }

        // Draw border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = borderColor;
            paint.StrokeWidth = _isPressed ? 2 : 1;

            canvas.DrawRoundRect(rect, 4, 4, paint);
        }

        // Draw focus rect if needed
        if (_showFocusRect && !_isPressed)
        {
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Stroke;
                paint.Color = new SKColor(100, 100, 255, 180);
                paint.StrokeWidth = 2;
                paint.PathEffect = SKPathEffect.CreateDash(new[] { 4f, 4f }, 0);

                var focusRect = rect;
                focusRect.Inflate(-3, -3);
                canvas.DrawRoundRect(focusRect, 4, 4, paint);
            }
        }

        // Draw caption text
        if (!string.IsNullOrEmpty(Caption))
        {
            // Remove access key ampersand for display
            var displayText = Caption.Replace("&", "");

            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Color = textColor;
                paint.TextSize = (float)FontSize * 1.33f; // Convert to pixels
                paint.TextAlign = SKTextAlign.Center;
                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI");

                // Calculate vertical center
                var textBounds = new SKRect();
                paint.MeasureText(displayText, ref textBounds);
                var textY = (info.Height - textBounds.Height) / 2 - textBounds.Top;

                canvas.DrawText(displayText, info.Width / 2, textY, paint);
            }
        }
    }

    private SKColor GetButtonFillColor()
    {
        if (UseCustomBackColor && ButtonBackColor != null)
        {
            return ButtonBackColor.ToSKColor();
        }

        // Theme-based colors
        if (!IsEnabled)
            return new SKColor(240, 240, 240); // Disabled
        else if (_isPressed)
            return new SKColor(180, 180, 180); // Pressed
        else if (_isMouseOver)
            return new SKColor(220, 220, 220); // Hovered
        else
            return new SKColor(245, 245, 245); // Normal
    }

    private SKColor GetButtonBorderColor()
    {
        if (!IsEnabled)
            return new SKColor(200, 200, 200);
        else if (_isPressed)
            return new SKColor(100, 100, 100);
        else if (_isMouseOver)
            return new SKColor(150, 150, 150);
        else
            return new SKColor(170, 170, 170);
    }

    private SKColor GetCaptionColor()
    {
        return IsEnabled
            ? new SKColor(0, 0, 0)
            : new SKColor(150, 150, 150);
    }

    #endregion

    #region Property Changed Handlers

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdButton button)
        {
            button._canvasView?.InvalidateSurface();
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

    #region Focus Handling

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(IsFocused))
        {
            if (IsFocused)
            {
                _showFocusRect = true;
                GotFocusAPI?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _showFocusRect = false;
                _isPressed = false;
                _isMouseOver = false;
                LostFocusAPI?.Invoke(this, EventArgs.Empty);
            }

            _canvasView?.InvalidateSurface();
        }
    }

    #endregion
}

/// <summary>
/// Extension methods for color conversion
/// </summary>
internal static class ColorExtensions
{
    public static SKColor ToSKColor(this Color color)
    {
        return new SKColor(
            (byte)(color.Red * 255),
            (byte)(color.Green * 255),
            (byte)(color.Blue * 255),
            (byte)(color.Alpha * 255));
    }
}
