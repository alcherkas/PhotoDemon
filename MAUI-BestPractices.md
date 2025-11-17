# .NET MAUI Best Practices for PhotoDemon Migration

## Overview
This document outlines industry best practices for building .NET MAUI applications, specifically tailored for the PhotoDemon migration from VB6 to .NET 10 MAUI.

**Version Context:** .NET 10 was released November 11, 2025 as a Long-Term Support (LTS) release. This guide references .NET MAUI 10 features and best practices.

---

## Architecture Patterns

### MVVM (Model-View-ViewModel)
**Recommended as the primary pattern for PhotoDemon**

**Benefits:**
- Separation of concerns: UI logic stays decoupled from business and data logic
- ViewModels can be unit tested in isolation from the UI
- Logic can be reused across multiple views and platforms
- Clean, maintainable codebase for large applications

**Implementation:**
- Use **Community Toolkit.MVVM** to reduce boilerplate code
- Simplifies MVVM implementation and improves state management
- Provides source generators for commands and properties

### Clean Architecture Principles
- **Feature separation**: Organize code by feature rather than technical layer
- **Interface segregation**: Use interfaces for loose coupling
- **Platform abstraction layers**: Clean extension points for platform-specific code
- **Modular design**: Each module should be independently testable

---

## Project Structure

### Core Files

**MauiProgram.cs**
- Entry point for dependency injection registration
- Register all views, view models, and services
- Configure app-level settings and handlers

**App.xaml / App.xaml.cs**
- Define application-level resources
- Configure host window
- Handle application lifecycle events

**AppShell.xaml / AppShell.xaml.cs**
- Main navigation host
- Define menu structure and routes
- Handle global navigation patterns

### Recommended Folder Structure
```
PhotoDemon.MAUI/
├── Models/
├── ViewModels/
├── Views/
│   ├── Pages/
│   ├── Controls/
│   └── Dialogs/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
├── Resources/
│   ├── Images/
│   ├── Fonts/
│   └── Styles/
└── Helpers/
```

---

## Dependency Injection

### Built-in DI Support
.NET MAUI has built-in support for **Microsoft.Extensions.DependencyInjection**

### Registration in MauiProgram.cs
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Services
        builder.Services.AddSingleton<IImageProcessingService, ImageProcessingService>();
        builder.Services.AddTransient<IFileService, FileService>();

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<EditorViewModel>();

        // Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EditorPage>();

        return builder.Build();
    }
}
```

### Service Lifetimes
- **Singleton**: One instance for app lifetime (e.g., settings, caching)
- **Transient**: New instance each time (e.g., ViewModels, Views)
- **Scoped**: Not commonly used in MAUI (mobile app pattern differs from web)

### Constructor Injection
```csharp
public class EditorViewModel
{
    private readonly IImageProcessingService _imageService;
    private readonly IFileService _fileService;

    public EditorViewModel(
        IImageProcessingService imageService,
        IFileService fileService)
    {
        _imageService = imageService;
        _fileService = fileService;
    }
}
```

---

## Performance Optimization

### UI Rendering

**Avoid Deeply Nested Layouts**
- Each layout requires calculation and rendering resources
- Use flat layout hierarchies where possible
- Consider `Grid` over nested `StackLayout`s

**Use CollectionView (ListView is Deprecated)**
- Built-in virtualization for large data sets
- More efficient than ListView
- Better performance for PhotoDemon's layer lists and tool palettes
- **Note:** ListView is marked obsolete in .NET 10 (though still functional). CollectionView is the recommended replacement with better performance, flexible layouts, and ongoing maintenance

**Handler Architecture**
- MAUI's handler architecture improves performance over Xamarin.Forms renderers
- Direct mapping to native controls with less overhead

### Asynchronous Programming

**Keep UI Responsive**
```csharp
// Bad
public void ApplyFilter()
{
    var result = ProcessImage(); // Blocks UI
    UpdateDisplay(result);
}

// Good
public async Task ApplyFilterAsync()
{
    var result = await ProcessImageAsync(); // UI remains responsive
    UpdateDisplay(result);
}
```

**Benefits:**
- Applications using async calls improve user experience by up to 70%
- Prevents UI freezing during resource-intensive operations
- Critical for PhotoDemon's 200+ image processing tools

### Startup Performance

**Native AOT Compilation**
- **Supported platforms:** iOS and Mac Catalyst (full Native AOT)
- Android uses profiled AOT (different from Native AOT)
- Windows/Desktop support is limited
- Reduces app size by ~35-50% on iOS
- Improves startup time by ~28-50% on iOS/Mac
- **Important:** Many libraries don't support AOT yet; test thoroughly
- Enable with `<PublishAot>true</PublishAot>` in .csproj
- Requires use of trim-safe patterns (e.g., IQueryAttributable instead of QueryProperty)

**Lazy Loading**
- Load heavy components only when needed
- Defer initialization of non-critical services
- Particularly important for PhotoDemon's extensive plugin system

### Memory Management

**Image Processing Considerations**
- Dispose of image resources properly
- Use `IDisposable` pattern for large objects
- Monitor memory usage during batch operations
- Consider streaming for very large files

**Resource Cleanup**
```csharp
public async Task ProcessImageAsync(string path)
{
    using var stream = await FileSystem.OpenAppPackageFileAsync(path);
    using var image = await Image.LoadAsync(stream);
    // Process image
} // Automatically disposed
```

---

## Cross-Platform Development

### Platform Abstraction

**Use Partial Classes for Platform-Specific Code**
```csharp
// Shared code
public partial class FilePickerService : IFilePickerService
{
    public partial Task<string> PickFileAsync();
}

// Windows/FilePickerService.cs
public partial class FilePickerService
{
    public partial async Task<string> PickFileAsync()
    {
        // Windows-specific implementation
    }
}
```

**Conditional Compilation**
```csharp
#if WINDOWS
    // Windows-specific code
#elif ANDROID
    // Android-specific code
#elif IOS || MACCATALYST
    // iOS/Mac-specific code
#endif
```

### Platform Features

**Target Broadest Shared Feature Set**
- Design for common denominator first
- Add platform-specific enhancements where appropriate
- Use `DeviceInfo` API to detect capabilities

**Platform-Specific UI**
```xml
<ContentPage>
    <OnPlatform x:TypeArguments="View">
        <On Platform="Windows">
            <!-- Windows-specific UI -->
        </On>
        <On Platform="Android,iOS">
            <!-- Mobile-specific UI -->
        </On>
    </OnPlatform>
</ContentPage>
```

---

## Navigation Patterns

### Shell Navigation (Recommended)
- Consistent navigation experience
- Built-in flyout and tab support
- URI-based routing

### Clean Navigation Implementation
```csharp
// Register routes
Routing.RegisterRoute("editor", typeof(EditorPage));

// Navigate with parameters
await Shell.Current.GoToAsync($"editor?imageId={imageId}");

// Receive parameters in ViewModel using IQueryAttributable (Trim-Safe)
public partial class EditorViewModel : ObservableObject, IQueryAttributable
{
    private string _imageId;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("imageId", out var value))
        {
            _imageId = value?.ToString();
            LoadImage(_imageId);
        }
    }
}
```

**⚠️ Important:** Use `IQueryAttributable` instead of `[QueryProperty]` attribute. The QueryProperty attribute is **not trim-safe** and should not be used with full trimming or NativeAOT deployments. IQueryAttributable is the recommended approach for production applications.

---

## Data Binding

### Community Toolkit.MVVM
```csharp
public partial class EditorViewModel : ObservableObject
{
    [ObservableProperty]
    private string _imageTitle;

    [ObservableProperty]
    private bool _isProcessing;

    [RelayCommand]
    private async Task ApplyFilterAsync()
    {
        IsProcessing = true;
        await ProcessImageAsync();
        IsProcessing = false;
    }
}
```

**Benefits:**
- Reduces boilerplate code by 60-70%
- Source generators create INotifyPropertyChanged implementation
- Automatic command creation with CanExecute support

---

## Testing Strategy

### Unit Testing ViewModels
- Test business logic independently of UI
- Mock services using interfaces
- Use xUnit, NUnit, or MSTest

### Integration Testing
- Test service interactions
- Validate data flow between layers

### UI Testing
- Use Appium for cross-platform UI testing
- Test critical user flows
- Platform-specific testing for native features

---

## App Size Optimization

### Trimming
- Enable trimming to remove unused code
- Reduces app package size
- Test thoroughly after enabling

### Resource Optimization
- Use vector graphics (SVG) where possible
- Optimize image assets for each platform
- Consider asset catalogs for iOS/Mac

---

## Security & Configuration

### Secure Storage
```csharp
// Store sensitive data
await SecureStorage.SetAsync("api_key", apiKey);

// Retrieve sensitive data
var apiKey = await SecureStorage.GetAsync("api_key");
```

### Configuration Management
- Use `appsettings.json` for configuration
- Environment-specific settings
- Keep secrets out of source control

---

## Accessibility

### Screen Reader Support
- Set `AutomationId` on interactive elements
- Use `SemanticProperties` for descriptions
- Test with platform screen readers (Narrator, VoiceOver, TalkBack)

### Keyboard Navigation
- Ensure tab order is logical
- Support keyboard shortcuts for common actions
- Critical for desktop platforms (Windows, macOS)

---

## PhotoDemon-Specific Considerations

### Image Processing Pipeline
- Use hardware acceleration where available
- Consider SkiaSharp for cross-platform graphics
- Implement background processing for heavy operations

### Plugin Architecture
- Design extensible plugin system using DI
- Support dynamic plugin loading
- Maintain backward compatibility where possible

### File Format Support
- **Recommended libraries:**
  - **SkiaSharp** (MIT license) - Best for rendering and GPU-accelerated operations
  - **Magick.NET** (Apache 2.0 license) - Comprehensive format support
  - **ImageSharp** - ⚠️ **Commercial license required** for production use (not free for commercial projects)
- Implement format handlers as services
- Support streaming for large files
- Consider SkiaSharp for cross-platform graphics due to:
  - Hardware acceleration support
  - Native integration with MAUI
  - MIT licensing compatible with PhotoDemon's BSD license

### Macro Recording System
- Design command pattern for recordable actions
- Serialize commands as data (JSON/XML)
- Support batch execution

### 200+ Tools & Effects
- Lazy load tool implementations
- Cache frequently used tools
- Provide progress reporting for long operations

---

## Additional Resources

- [Microsoft .NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Enterprise Application Patterns Using .NET MAUI](https://learn.microsoft.com/en-us/dotnet/architecture/maui/)
- [Community Toolkit.MVVM](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [.NET MAUI Samples](https://github.com/dotnet/maui-samples)

---

## Summary

For PhotoDemon's migration to .NET MAUI:

1. **Use MVVM** with Community Toolkit for maintainability
2. **Implement DI** for loose coupling and testability
3. **Optimize performance** with async/await and efficient rendering
4. **Design for cross-platform** with clean abstraction layers
5. **Test thoroughly** across all target platforms
6. **Leverage modern tooling** for improved developer experience

These practices will ensure PhotoDemon's .NET MAUI version is performant, maintainable, and provides an excellent user experience across all platforms.
