using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.ComponentModel;

namespace PhotoDemon.Maui.Controls.Base;

/// <summary>
/// Base class for all PhotoDemon custom controls.
/// Provides common functionality like theming, DPI awareness, and accessibility support.
/// </summary>
public abstract class PhotoDemonControlBase : ContentView, IPhotoDemonControl
{
    #region Bindable Properties

    /// <summary>
    /// Caption/Text displayed on the control
    /// </summary>
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption),
        typeof(string),
        typeof(PhotoDemonControlBase),
        string.Empty,
        propertyChanged: OnCaptionChanged);

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>
    /// Whether the control is enabled
    /// </summary>
    public new static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled),
        typeof(bool),
        typeof(PhotoDemonControlBase),
        true,
        propertyChanged: OnIsEnabledChanged);

    public new bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    #endregion

    #region Constructor

    protected PhotoDemonControlBase()
    {
        // Apply default theme on creation
        ApplyTheme();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnCaptionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PhotoDemonControlBase control)
        {
            control.OnCaptionChangedCore((string)oldValue, (string)newValue);
        }
    }

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PhotoDemonControlBase control)
        {
            control.OnIsEnabledChangedCore((bool)oldValue, (bool)newValue);
        }
    }

    protected virtual void OnCaptionChangedCore(string oldValue, string newValue)
    {
        UpdateVisualAppearance();
    }

    protected virtual void OnIsEnabledChangedCore(bool oldValue, bool newValue)
    {
        UpdateVisualAppearance();
    }

    #endregion

    #region IPhotoDemonControl Implementation

    public virtual void UpdateVisualAppearance()
    {
        // Override in derived classes to update visual appearance
    }

    public virtual void ApplyTheme()
    {
        // Override in derived classes to apply theming
        // This will integrate with PhotoDemon's theme engine
    }

    #endregion
}
