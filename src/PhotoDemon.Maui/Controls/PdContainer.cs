using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon generic control container - migrated from VB6 pdContainer.ctl
///
/// Features:
/// - Lightweight container for grouping controls
/// - Themable background color
/// - Optional custom background color
/// - Drag/drop support
/// - Size change notifications
/// - No focus handling (container only)
///
/// This is a simpler alternative to heavy containers like Frame.
/// It provides theming and layout support without the overhead.
///
/// Original VB6: Controls/pdContainer.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 137 instances across the application
/// </summary>
public class PdContainer : PhotoDemonControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Custom background color (overrides theme if UseUniqueBackColor is true)
    /// </summary>
    public static readonly BindableProperty UniqueBackColorProperty = BindableProperty.Create(
        nameof(UniqueBackColor),
        typeof(Color),
        typeof(PdContainer),
        null,
        propertyChanged: OnVisualPropertyChanged);

    public Color UniqueBackColor
    {
        get => (Color)GetValue(UniqueBackColorProperty);
        set => SetValue(UniqueBackColorProperty, value);
    }

    /// <summary>
    /// Whether to use the unique back color instead of theme
    /// </summary>
    public static readonly BindableProperty UseUniqueBackColorProperty = BindableProperty.Create(
        nameof(UseUniqueBackColor),
        typeof(bool),
        typeof(PdContainer),
        false,
        propertyChanged: OnVisualPropertyChanged);

    public bool UseUniqueBackColor
    {
        get => (bool)GetValue(UseUniqueBackColorProperty);
        set => SetValue(UseUniqueBackColorProperty, value);
    }

    /// <summary>
    /// Padding around child content
    /// </summary>
    public static readonly BindableProperty PaddingProperty = BindableProperty.Create(
        nameof(Padding),
        typeof(Thickness),
        typeof(PdContainer),
        new Thickness(0),
        propertyChanged: OnVisualPropertyChanged);

    public new Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// Border thickness (0 for no border)
    /// </summary>
    public static readonly BindableProperty BorderThicknessProperty = BindableProperty.Create(
        nameof(BorderThickness),
        typeof(double),
        typeof(PdContainer),
        0.0,
        propertyChanged: OnVisualPropertyChanged);

    public double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Border color
    /// </summary>
    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(PdContainer),
        Colors.LightGray,
        propertyChanged: OnVisualPropertyChanged);

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    /// <summary>
    /// Corner radius for rounded corners
    /// </summary>
    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(double),
        typeof(PdContainer),
        0.0,
        propertyChanged: OnVisualPropertyChanged);

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when container size changes
    /// </summary>
    public event EventHandler SizeChanged;

    /// <summary>
    /// Raised when container receives focus
    /// </summary>
    public event EventHandler GotFocusAPI;

    /// <summary>
    /// Raised when container loses focus
    /// </summary>
    public event EventHandler LostFocusAPI;

    #endregion

    #region Private Fields

    private readonly Border _border;
    private readonly Grid _contentGrid;
    private Size _previousSize;

    #endregion

    #region Constructor

    public PdContainer()
    {
        // Create border for visual appearance
        _border = new Border
        {
            StrokeThickness = 0,
            Stroke = Brushes.Transparent,
            BackgroundColor = Colors.Transparent
        };

        // Create grid to hold child content
        _contentGrid = new Grid
        {
            Padding = new Thickness(0)
        };

        _border.Content = _contentGrid;
        Content = _border;

        // Containers don't accept focus by design
        IsTabStop = false;

        // Track size changes
        SizeChanged += OnSizeChangedInternal;

        ApplyTheme();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the child content of the container
    /// </summary>
    public void SetContent(View child)
    {
        _contentGrid.Children.Clear();
        if (child != null)
        {
            _contentGrid.Children.Add(child);
        }
    }

    /// <summary>
    /// Adds a child to the container (supports multiple children via Grid)
    /// </summary>
    public void AddChild(View child, int row = 0, int column = 0)
    {
        if (child == null) return;

        // Ensure grid has enough rows/columns
        while (_contentGrid.RowDefinitions.Count <= row)
        {
            _contentGrid.RowDefinitions.Add(new RowDefinition());
        }
        while (_contentGrid.ColumnDefinitions.Count <= column)
        {
            _contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }

        Grid.SetRow(child, row);
        Grid.SetColumn(child, column);
        _contentGrid.Children.Add(child);
    }

    /// <summary>
    /// Clears all children from the container
    /// </summary>
    public void ClearChildren()
    {
        _contentGrid.Children.Clear();
        _contentGrid.RowDefinitions.Clear();
        _contentGrid.ColumnDefinitions.Clear();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdContainer container)
        {
            container.UpdateVisualAppearance();
        }
    }

    #endregion

    #region Size Tracking

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        var newSize = new Size(width, height);
        if (newSize != _previousSize)
        {
            _previousSize = newSize;
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnSizeChangedInternal(object sender, EventArgs e)
    {
        // Internal size change tracking
        // Can be used for layout recalculations if needed
    }

    #endregion

    #region IPhotoDemonControl Implementation

    public override void UpdateVisualAppearance()
    {
        base.UpdateVisualAppearance();

        // Update background color
        if (UseUniqueBackColor && UniqueBackColor != null)
        {
            _border.BackgroundColor = UniqueBackColor;
        }
        else
        {
            _border.BackgroundColor = GetThemeBackgroundColor();
        }

        // Update border
        _border.StrokeThickness = BorderThickness;
        _border.Stroke = BorderThickness > 0 ? new SolidColorBrush(BorderColor) : Brushes.Transparent;

        // Update corner radius
        _border.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(CornerRadius)
        };

        // Update padding
        _contentGrid.Padding = Padding;
    }

    public override void ApplyTheme()
    {
        base.ApplyTheme();
        UpdateVisualAppearance();
    }

    #endregion

    #region Theme Support

    private Color GetThemeBackgroundColor()
    {
        // This will be integrated with PhotoDemon's theme engine
        // For now, return a neutral background
        return Colors.Transparent;
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
        }
    }

    #endregion
}
