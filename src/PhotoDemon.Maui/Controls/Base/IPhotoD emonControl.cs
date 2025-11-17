using Microsoft.Maui.Controls;

namespace PhotoDemon.Maui.Controls.Base;

/// <summary>
/// Base interface for all PhotoDemon controls.
/// Provides common functionality across all custom controls.
/// </summary>
public interface IPhotoDemonControl
{
    /// <summary>
    /// Gets or sets whether the control is enabled for user interaction.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the caption/text displayed on the control.
    /// </summary>
    string Caption { get; set; }

    /// <summary>
    /// Updates the visual appearance of the control (e.g., for theming).
    /// </summary>
    void UpdateVisualAppearance();

    /// <summary>
    /// Applies the current theme to the control.
    /// </summary>
    void ApplyTheme();
}
