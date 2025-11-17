using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon collapsible title control - migrated from VB6 pdTitle.ctl
///
/// Features:
/// - Title label with expand/collapse arrow
/// - Toggle state (true = expanded, false = collapsed)
/// - Used above collapsible UI panels
/// - Optional drag-to-resize support
/// - Theme support
/// - High-DPI aware
///
/// Original VB6: Controls/pdTitle.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 55 instances across the application
/// </summary>
public class PdTitle : PhotoDemonControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Title state (true = expanded/down arrow, false = collapsed/right arrow)
    /// </summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value),
        typeof(bool),
        typeof(PdTitle),
        true,
        BindingMode.TwoWay,
        propertyChanged: OnValueChanged);

    public bool Value
    {
        get => (bool)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Whether the title bar supports drag-to-resize
    /// </summary>
    public static readonly BindableProperty DraggableProperty = BindableProperty.Create(
        nameof(Draggable),
        typeof(bool),
        typeof(PdTitle),
        false);

    public bool Draggable
    {
        get => (bool)GetValue(DraggableProperty);
        set => SetValue(DraggableProperty, value);
    }

    /// <summary>
    /// Font size for title
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(PdTitle),
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
    /// Raised when title is clicked (passes new state)
    /// </summary>
    public event EventHandler<bool> Click;

    /// <summary>
    /// Raised when dragging (for resize operations)
    /// </summary>
    public event EventHandler<Point> MouseDrag;

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
    private bool _isMouseOver;
    private bool _isDragging;
    private Point _dragStartPoint;
    private const float ArrowSize = 8;
    private const float ArrowPadding = 8;
    private const float GripperPadding = 12;

    #endregion

    #region Constructor

    public PdTitle()
    {
        // Create canvas for rendering
        _canvasView = new SKCanvasView();
        _canvasView.PaintSurface += OnPaintSurface;

        Content = _canvasView;

        // Set up gesture recognizers
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += OnTapped;
        _canvasView.GestureRecognizers.Add(tapGesture);

        // Set up pointer events for hover and drag
        var pointerGesture = new PointerGestureRecognizer();
        pointerGesture.PointerEntered += (s, e) =>
        {
            _isMouseOver = true;
            _canvasView.InvalidateSurface();
        };
        pointerGesture.PointerExited += (s, e) =>
        {
            _isMouseOver = false;
            _canvasView.InvalidateSurface();
        };
        _canvasView.GestureRecognizers.Add(pointerGesture);

        // Set up pan gesture for dragging
        if (Draggable)
        {
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            _canvasView.GestureRecognizers.Add(panGesture);
        }

        ApplyTheme();
    }

    #endregion

    #region Event Handlers

    private void OnTapped(object sender, EventArgs e)
    {
        if (!IsEnabled)
            return;

        // Toggle state
        Value = !Value;

        // Raise click event with new state
        Click?.Invoke(this, Value);
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!IsEnabled || !Draggable)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isDragging = true;
                _dragStartPoint = new Point(e.TotalX, e.TotalY);
                break;

            case GestureStatus.Running:
                if (_isDragging)
                {
                    var deltaX = e.TotalX - _dragStartPoint.X;
                    var deltaY = e.TotalY - _dragStartPoint.Y;
                    MouseDrag?.Invoke(this, new Point(deltaX, deltaY));
                    _dragStartPoint = new Point(e.TotalX, e.TotalY);
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isDragging = false;
                break;
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        var info = e.Info;

        // Draw background
        DrawBackground(canvas, info);

        // Draw expand/collapse arrow
        DrawArrow(canvas, info);

        // Draw caption
        DrawCaption(canvas, info);

        // Draw gripper if draggable
        if (Draggable)
        {
            DrawGripper(canvas, info);
        }
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTitle title)
        {
            title._canvasView?.InvalidateSurface();
        }
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTitle title)
        {
            title._canvasView?.InvalidateSurface();
        }
    }

    #endregion

    #region Rendering

    private void DrawBackground(SKCanvas canvas, SKImageInfo info)
    {
        var rect = new SKRect(0, 0, info.Width, info.Height);

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetBackgroundColor();
            paint.Style = SKPaintStyle.Fill;
            canvas.DrawRect(rect, paint);
        }

        // Draw bottom border
        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetBorderColor();
            paint.Style = SKPaintStyle.Stroke;
            paint.StrokeWidth = 1;
            canvas.DrawLine(0, info.Height - 1, info.Width, info.Height - 1, paint);
        }
    }

    private void DrawArrow(SKCanvas canvas, SKImageInfo info)
    {
        float centerY = info.Height / 2;
        float arrowX = ArrowPadding + ArrowSize / 2;

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetTextColor();
            paint.Style = SKPaintStyle.Fill;

            using (var path = new SKPath())
            {
                if (Value)
                {
                    // Down arrow (expanded state)
                    path.MoveTo(arrowX - ArrowSize / 2, centerY - ArrowSize / 4);
                    path.LineTo(arrowX + ArrowSize / 2, centerY - ArrowSize / 4);
                    path.LineTo(arrowX, centerY + ArrowSize / 4);
                }
                else
                {
                    // Right arrow (collapsed state)
                    path.MoveTo(arrowX - ArrowSize / 4, centerY - ArrowSize / 2);
                    path.LineTo(arrowX + ArrowSize / 4, centerY);
                    path.LineTo(arrowX - ArrowSize / 4, centerY + ArrowSize / 2);
                }

                path.Close();
                canvas.DrawPath(path, paint);
            }
        }
    }

    private void DrawCaption(SKCanvas canvas, SKImageInfo info)
    {
        if (string.IsNullOrEmpty(Caption))
            return;

        float captionX = ArrowPadding + ArrowSize + ArrowPadding;
        float captionWidth = info.Width - captionX - (Draggable ? GripperPadding + 20 : 10);

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetTextColor();
            paint.TextSize = (float)FontSize * 1.33f;
            paint.Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

            // Calculate vertical center
            var textBounds = new SKRect();
            paint.MeasureText(Caption, ref textBounds);
            float textY = (info.Height - textBounds.Height) / 2 - textBounds.Top;

            canvas.DrawText(Caption, captionX, textY, paint);
        }
    }

    private void DrawGripper(SKCanvas canvas, SKImageInfo info)
    {
        // Draw vertical gripper dots on the right side
        float gripperX = info.Width - GripperPadding;
        float centerY = info.Height / 2;
        float dotRadius = 1.5f;
        float dotSpacing = 4;

        using (var paint = new SKPaint())
        {
            paint.IsAntialias = true;
            paint.Color = GetTextColor();
            paint.Style = SKPaintStyle.Fill;

            // Draw 3 rows of 2 dots
            for (int row = -1; row <= 1; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    float x = gripperX - (col * dotSpacing);
                    float y = centerY + (row * dotSpacing);
                    canvas.DrawCircle(x, y, dotRadius, paint);
                }
            }
        }
    }

    private SKColor GetBackgroundColor()
    {
        if (!IsEnabled)
            return new SKColor(245, 245, 245);
        else if (_isMouseOver)
            return new SKColor(230, 230, 230);
        else
            return new SKColor(240, 240, 240);
    }

    private SKColor GetBorderColor()
    {
        return new SKColor(200, 200, 200);
    }

    private SKColor GetTextColor()
    {
        return IsEnabled
            ? new SKColor(60, 60, 60)
            : new SKColor(150, 150, 150);
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
