using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon color selector control - migrated from VB6 pdColorSelector.ctl
///
/// Features:
/// - Displays current color as a clickable swatch
/// - Opens color picker dialog when clicked
/// - Optional secondary color display (main window color)
/// - Theme support
/// - High-DPI aware
///
/// Original VB6: Controls/pdColorSelector.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 60 instances across the application
/// </summary>
public class PdColorSelector : PhotoDemonControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Currently selected color
    /// </summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color),
        typeof(Color),
        typeof(PdColorSelector),
        Colors.Black,
        BindingMode.TwoWay,
        propertyChanged: OnColorChanged);

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    /// Main window color (for secondary swatch)
    /// </summary>
    public static readonly BindableProperty MainWindowColorProperty = BindableProperty.Create(
        nameof(MainWindowColor),
        typeof(Color),
        typeof(PdColorSelector),
        Colors.White,
        propertyChanged: OnVisualPropertyChanged);

    public Color MainWindowColor
    {
        get => (Color)GetValue(MainWindowColorProperty);
        set => SetValue(MainWindowColorProperty, value);
    }

    /// <summary>
    /// Whether to show the main window color swatch
    /// </summary>
    public static readonly BindableProperty ShowMainWindowColorProperty = BindableProperty.Create(
        nameof(ShowMainWindowColor),
        typeof(bool),
        typeof(PdColorSelector),
        true,
        propertyChanged: OnVisualPropertyChanged);

    public bool ShowMainWindowColor
    {
        get => (bool)GetValue(ShowMainWindowColorProperty);
        set => SetValue(ShowMainWindowColorProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when color is changed
    /// </summary>
    public event EventHandler ColorChanged;

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
    private bool _mouseInPrimaryButton;
    private bool _mouseInSecondaryButton;
    private SKRect _primaryColorRect;
    private SKRect _secondaryColorRect;
    private const float SecondaryColorWidth = 30;
    private const float Gap = 4;

    #endregion

    #region Constructor

    public PdColorSelector()
    {
        // Create canvas for rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        // Set up gesture recognizers
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _canvasView.GestureRecognizers.Add(tapGesture);

        // Set up pointer events for hover
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerMoved += OnPointerMoved;
        pointerGesture.PointerExited += (s, e) =>
        {
            _mouseInPrimaryButton = false;
            _mouseInSecondaryButton = false;
            _canvasView.InvalidateSurface();
        };
        _canvasView.GestureRecognizers.Add(pointerGesture);

        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private async void OnTapped(object sender, TappedEventArgs e)
    {
        if (!IsEnabled)
            return;

        var point = e.GetPosition(_canvasView);
        if (!point.HasValue)
            return;

        float x = (float)point.Value.X;
        float y = (float)point.Value.Y;

        // Check if clicked on secondary color (copy to primary)
        if (ShowMainWindowColor && _secondaryColorRect.Contains(x, y))
        {
            Color = MainWindowColor;
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
        // Check if clicked on primary color (show color picker)
        else if (_primaryColorRect.Contains(x, y))
        {
            await ShowColorPickerAsync();
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        var point = e.GetPosition(_canvasView);
        if (!point.HasValue)
            return;

        float x = (float)point.Value.X;
        float y = (float)point.Value.Y;

        bool newPrimaryHover = _primaryColorRect.Contains(x, y);
        bool newSecondaryHover = ShowMainWindowColor && _secondaryColorRect.Contains(x, y);

        if (newPrimaryHover != _mouseInPrimaryButton || newSecondaryHover != _mouseInSecondaryButton)
        {
            _mouseInPrimaryButton = newPrimaryHover;
            _mouseInSecondaryButton = newSecondaryHover;
            _canvasView.InvalidateSurface();
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;

        // Update layout
        UpdateLayout(info.Width, info.Height);

        // Draw primary color swatch
        DrawColorSwatch(canvas, _primaryColorRect, Color, _mouseInPrimaryButton);

        // Draw secondary color swatch if enabled
        if (ShowMainWindowColor)
        {
            DrawColorSwatch(canvas, _secondaryColorRect, MainWindowColor, _mouseInSecondaryButton);
        }
    }

    private static void OnColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdColorSelector selector)
        {
            selector._canvasView?.InvalidateSurface();
            selector.ColorChanged?.Invoke(selector, EventArgs.Empty);
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdColorSelector selector)
        {
            selector._canvasView?.InvalidateSurface();
        }
    }

    #endregion

    #region Layout

    private void UpdateLayout(float width, float height)
    {
        if (ShowMainWindowColor)
        {
            // Primary color takes most of the space, secondary on the right
            float primaryWidth = width - SecondaryColorWidth - Gap;
            _primaryColorRect = new SKRect(0, 0, primaryWidth, height);
            _secondaryColorRect = new SKRect(primaryWidth + Gap, 0, width, height);
        }
        else
        {
            // Primary color takes full width
            _primaryColorRect = new SKRect(0, 0, width, height);
            _secondaryColorRect = SKRect.Empty;
        }
    }

    #endregion

    #region Rendering

    private void DrawColorSwatch(SKCanvas canvas, SKRect rect, Color color, bool isHovered)
    {
        if (rect.IsEmpty)
            return;

        // Draw checkerboard background (for transparency visualization)
        DrawCheckerboard(canvas, rect);

        // Draw color
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = color.ToSKColor();
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRoundRect(rect, 2, 2, paint);
        }

        // Draw border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetBorderColor(isHovered);
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = isHovered ? 2 : 1;
            canvas.DrawRoundRect(rect, 2, 2, paint);
        }
    }

    private void DrawCheckerboard(SKCanvas canvas, SKRect rect)
    {
        const float checkSize = 4;

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = false;

            for (float y = rect.Top; y < rect.Bottom; y += checkSize)
            {
                for (float x = rect.Left; x < rect.Right; x += checkSize)
                {
                    int checkX = (int)((x - rect.Left) / checkSize);
                    int checkY = (int)((y - rect.Top) / checkSize);
                    bool isLight = (checkX + checkY) % 2 == 0;

                    paint.Color = isLight
                        ? new SKColor(255, 255, 255)
                        : new SKColor(204, 204, 204);

                    var checkRect = new SKRect(x, y, Math.Min(x + checkSize, rect.Right), Math.Min(y + checkSize, rect.Bottom));
                    canvas.DrawRect(checkRect, paint);
                }
            }
        }
    }

    private SKColor GetBorderColor(bool isHovered)
    {
        if (!IsEnabled)
            return new SKColor(200, 200, 200);
        else if (isHovered)
            return new SKColor(0, 120, 215);
        else
            return new SKColor(120, 120, 120);
    }

    #endregion

    #region Color Picker

    private async Task ShowColorPickerAsync()
    {
        // Show color picker dialog
        // Note: MAUI doesn't have a built-in color picker, so we'll need to create one
        // or use a community package. For now, this is a placeholder.

        // In a full implementation, you would:
        // 1. Create a custom color picker page/popup
        // 2. Navigate to it or show as modal
        // 3. Get the selected color
        // 4. Update the Color property

        // Placeholder: Just show a simple alert
        var result = await Application.Current.MainPage.DisplayAlert(
            "Color Selector",
            $"Current Color: {Color}\n\nIn a full implementation, this would show a color picker dialog.",
            "OK",
            "Cancel");

        // In real implementation:
        // if (selectedColor.HasValue)
        // {
        //     Color = selectedColor.Value;
        //     ColorChanged?.Invoke(this, EventArgs.Empty);
        // }
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
