using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using PhotoDemon.Maui.Controls.Base;

namespace PhotoDemon.Maui.Controls;

/// <summary>
/// PhotoDemon text box control - migrated from VB6 pdTextBox.ctl
///
/// Features:
/// - Unicode support (native in .NET)
/// - Custom themed appearance
/// - Multiline support
/// - Password mode
/// - Max length
/// - Focus handling
/// - Keyboard events
///
/// This is a wrapper around MAUI Entry/Editor with PhotoDemon theming.
///
/// Original VB6: Controls/pdTextBox.ctl
/// Documentation: docs/ui/control-mapping.md
/// Usage: 41 instances across the application
/// </summary>
public class PdTextBox : PhotoDemonControlBase
{
    #region Bindable Properties

    /// <summary>
    /// Text content
    /// </summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(PdTextBox),
        string.Empty,
        BindingMode.TwoWay,
        propertyChanged: OnTextChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Maximum text length (0 = unlimited)
    /// </summary>
    public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(
        nameof(MaxLength),
        typeof(int),
        typeof(PdTextBox),
        0,
        propertyChanged: OnMaxLengthChanged);

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>
    /// Whether text box supports multiple lines
    /// </summary>
    public static readonly BindableProperty MultilineProperty = BindableProperty.Create(
        nameof(Multiline),
        typeof(bool),
        typeof(PdTextBox),
        false,
        propertyChanged: OnMultilineChanged);

    public bool Multiline
    {
        get => (bool)GetValue(MultilineProperty);
        set => SetValue(MultilineProperty, value);
    }

    /// <summary>
    /// Password character mode
    /// </summary>
    public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
        nameof(IsPassword),
        typeof(bool),
        typeof(PdTextBox),
        false,
        propertyChanged: OnIsPasswordChanged);

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    /// <summary>
    /// Placeholder text
    /// </summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(PdTextBox),
        string.Empty,
        propertyChanged: OnPlaceholderChanged);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Text alignment
    /// </summary>
    public static readonly BindableProperty TextAlignmentProperty = BindableProperty.Create(
        nameof(TextAlignment),
        typeof(TextAlignment),
        typeof(PdTextBox),
        TextAlignment.Start,
        propertyChanged: OnTextAlignmentChanged);

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    /// <summary>
    /// Font size
    /// </summary>
    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize),
        typeof(double),
        typeof(PdTextBox),
        14.0,
        propertyChanged: OnFontSizeChanged);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// Raised when text changes
    /// </summary>
    public event EventHandler Change;

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

    private Entry _entry;
    private Editor _editor;
    private Border _border;
    private bool _internalUpdate;

    #endregion

    #region Constructor

    public PdTextBox()
    {
        CreateControls();
        ApplyTheme();
    }

    #endregion

    #region Control Creation

    private void CreateControls()
    {
        if (Multiline)
        {
            CreateEditor();
        }
        else
        {
            CreateEntry();
        }
    }

    private void CreateEntry()
    {
        _entry = new Entry
        {
            FontSize = FontSize,
            HorizontalTextAlignment = TextAlignment,
            Placeholder = Placeholder,
            MaxLength = MaxLength > 0 ? MaxLength : int.MaxValue,
            IsPassword = IsPassword,
            Text = Text
        };

        _entry.TextChanged += OnEntryTextChanged;
        _entry.Focused += (s, e) => GotFocusAPI?.Invoke(this, EventArgs.Empty);
        _entry.Unfocused += (s, e) => LostFocusAPI?.Invoke(this, EventArgs.Empty);

        _border = new Border
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            Content = _entry
        };

        Content = _border;
    }

    private void CreateEditor()
    {
        _editor = new Editor
        {
            FontSize = FontSize,
            Placeholder = Placeholder,
            MaxLength = MaxLength > 0 ? MaxLength : int.MaxValue,
            Text = Text,
            AutoSize = EditorAutoSizeOption.TextChanges
        };

        _editor.TextChanged += OnEditorTextChanged;
        _editor.Focused += (s, e) => GotFocusAPI?.Invoke(this, EventArgs.Empty);
        _editor.Unfocused += (s, e) => LostFocusAPI?.Invoke(this, EventArgs.Empty);

        _border = new Border
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            Content = _editor
        };

        Content = _border;
    }

    #endregion

    #region Event Handlers

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_internalUpdate) return;

        _internalUpdate = true;
        try
        {
            Text = e.NewTextValue;
            Change?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _internalUpdate = false;
        }
    }

    private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_internalUpdate) return;

        _internalUpdate = true;
        try
        {
            Text = e.NewTextValue;
            Change?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _internalUpdate = false;
        }
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTextBox textBox && !textBox._internalUpdate)
        {
            textBox._internalUpdate = true;
            try
            {
                if (textBox._entry != null)
                    textBox._entry.Text = (string)newValue;
                if (textBox._editor != null)
                    textBox._editor.Text = (string)newValue;
            }
            finally
            {
                textBox._internalUpdate = false;
            }
        }
    }

    private static void OnMaxLengthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTextBox textBox)
        {
            int maxLength = (int)newValue;
            if (textBox._entry != null)
                textBox._entry.MaxLength = maxLength > 0 ? maxLength : int.MaxValue;
            if (textBox._editor != null)
                textBox._editor.MaxLength = maxLength > 0 ? maxLength : int.MaxValue;
        }
    }

    private static void OnMultilineChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTextBox textBox)
        {
            // Recreate controls when multiline changes
            textBox.CreateControls();
        }
    }

    private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTextBox textBox && textBox._entry != null)
        {
            textBox._entry.IsPassword = (bool)newValue;
        }
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTextBox textBox)
        {
            string placeholder = (string)newValue;
            if (textBox._entry != null)
                textBox._entry.Placeholder = placeholder;
            if (textBox._editor != null)
                textBox._editor.Placeholder = placeholder;
        }
    }

    private static void OnTextAlignmentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTextBox textBox && textBox._entry != null)
        {
            textBox._entry.HorizontalTextAlignment = (TextAlignment)newValue;
        }
    }

    private static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PdTextBox textBox)
        {
            double fontSize = (double)newValue;
            if (textBox._entry != null)
                textBox._entry.FontSize = fontSize;
            if (textBox._editor != null)
                textBox._editor.FontSize = fontSize;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Select all text
    /// </summary>
    public void SelectAll()
    {
        _entry?.Focus();
        // MAUI doesn't have direct SelectAll API, but focusing will select in many cases
    }

    /// <summary>
    /// Clear text
    /// </summary>
    public void Clear()
    {
        Text = string.Empty;
    }

    #endregion

    #region IPhotoDemonControl Implementation

    public override void UpdateVisualAppearance()
    {
        base.UpdateVisualAppearance();

        // Update border colors based on theme
        if (_border != null)
        {
            _border.Stroke = IsEnabled
                ? new SolidColorBrush(Colors.Gray)
                : new SolidColorBrush(Colors.LightGray);
        }

        // Update text color
        if (_entry != null)
        {
            _entry.TextColor = IsEnabled ? Colors.Black : Colors.Gray;
        }
        if (_editor != null)
        {
            _editor.TextColor = IsEnabled ? Colors.Black : Colors.Gray;
        }
    }

    public override void ApplyTheme()
    {
        base.ApplyTheme();
        UpdateVisualAppearance();
    }

    #endregion
}
