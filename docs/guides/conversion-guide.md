# VB6 to C# .NET MAUI Conversion Guide

**Document Version:** 1.0
**Date:** 2025-11-17
**Target:** PhotoDemon Migration to .NET 10 MAUI
**Author:** Agent 11 - Conversion Guide Agent

---

## Table of Contents

1. [VB6 → C# Syntax Guide](#vb6--c-syntax-guide)
2. [Common Pattern Translations](#common-pattern-translations)
3. [Error Handling Migration](#error-handling-migration)
4. [Resource Management](#resource-management)
5. [Threading Model Changes](#threading-model-changes)
6. [Quick Reference Table](#quick-reference-table)

---

## VB6 → C# Syntax Guide

### Variable Declarations

**VB6:**
```vb
' Variable declarations in VB6
Dim m_Processing As Boolean
Dim m_FocusHWnd As Long
Dim procStartTime As Currency
Private Const PD_PROCESS_EXIT_NOW As String = "EXIT_NOW"

' Multiple variables on one line
Dim m_dibWidth As Long, m_dibHeight As Long

' Arrays
Private imgLayers() As pdLayer
Dim prevGenericSetting() As Variant
```

**C#:**
```csharp
// Variable declarations in C#
private bool m_Processing;
private IntPtr m_FocusHWnd;
private double procStartTime;
private const string PD_PROCESS_EXIT_NOW = "EXIT_NOW";

// Multiple variables (explicit typing required)
private long m_dibWidth;
private long m_dibHeight;

// Arrays (generic lists preferred)
private pdLayer[] imgLayers;
private object[] prevGenericSetting;

// Better: Use List<T> instead of arrays
private List<Layer> imgLayers = new();
private List<object> prevGenericSetting = new();
```

**Key Differences:**
- VB6 uses `Dim`, `Private`, `Public`; C# uses `var`, `private`, `public`
- VB6 has implicit `Variant` type; C# requires explicit types (or `var` with initialization)
- VB6 uses `As Type`; C# uses `Type variableName`
- VB6 `Long` (32-bit) → C# `int` or `long` (int is 32-bit, long is 64-bit)
- VB6 `Currency` → C# `decimal` or `double`
- VB6 `String` → C# `string`
- VB6 `Boolean` → C# `bool`

### Type Mapping Reference

| VB6 Type | C# Type | Notes |
|----------|---------|-------|
| `Boolean` | `bool` | |
| `Byte` | `byte` | |
| `Integer` (16-bit) | `short` | VB6 Integer is 16-bit |
| `Long` (32-bit) | `int` | VB6 Long is 32-bit |
| `Single` | `float` | |
| `Double` | `double` | |
| `Currency` | `decimal` | Use `decimal` for financial precision |
| `String` | `string` | |
| `Variant` | `object` or `dynamic` | Prefer explicit types |
| `Date` | `DateTime` | |
| `Object` | `object` | |
| VB6 Arrays | `T[]` or `List<T>` | Prefer `List<T>` |

**Reference:** See `/home/user/PhotoDemon/docs/data/type-mapping.md` for comprehensive type mappings

### Loops

**VB6:**
```vb
' For loop
For i = 0 To 255
    lut(i) = CalculateValue(i)
Next i

' For Each loop
Dim layer As pdLayer
For Each layer In imgLayers
    layer.Visible = True
Next layer

' While loop
Do While condition
    ' process
Loop

' Do Until
Do Until finished
    ' process
Loop
```

**C#:**
```csharp
// For loop
for (int i = 0; i <= 255; i++)
{
    lut[i] = CalculateValue(i);
}

// Foreach loop
foreach (var layer in imgLayers)
{
    layer.Visible = true;
}

// While loop
while (condition)
{
    // process
}

// Do-while (runs at least once)
do
{
    // process
} while (!finished);
```

**Key Differences:**
- VB6 `For i = 0 To 10` is *inclusive* (0-10); C# `for (int i = 0; i < 10; i++)` is *exclusive* (0-9)
- VB6 uses `Next`; C# uses closing brace `}`
- VB6 `For Each` → C# `foreach`
- VB6 `Do While`/`Do Until` → C# `while`/`do-while`

### Conditionals

**VB6:**
```vb
' If-Then-Else
If raiseDialog Then
    ShowDialog processID
ElseIf showPreview Then
    UpdatePreview
Else
    ApplyEffect
End If

' Single-line If
If (Not raiseDialog) Then VBHacks.GetHighResTime procStartTime

' Select Case
Select Case processID
    Case "blur"
        ApplyBlur
    Case "sharpen"
        ApplySharpen
    Case Else
        ShowError
End Select

' IIf (ternary)
Dim result As String
result = IIf(success, "OK", "FAIL")
```

**C#:**
```csharp
// If-else
if (raiseDialog)
{
    ShowDialog(processID);
}
else if (showPreview)
{
    UpdatePreview();
}
else
{
    ApplyEffect();
}

// Single-line if (still needs braces for clarity)
if (!raiseDialog)
    VBHacks.GetHighResTime(ref procStartTime);

// Switch statement
switch (processID)
{
    case "blur":
        ApplyBlur();
        break;
    case "sharpen":
        ApplySharpen();
        break;
    default:
        ShowError();
        break;
}

// C# 8+ switch expression (preferred)
var action = processID switch
{
    "blur" => ApplyBlur(),
    "sharpen" => ApplySharpen(),
    _ => ShowError()
};

// Ternary operator
string result = success ? "OK" : "FAIL";
```

**Key Differences:**
- VB6 uses `Then`/`End If`; C# uses braces `{}`
- VB6 `Select Case` → C# `switch` (requires `break` statements)
- VB6 `IIf()` → C# ternary operator `? :`
- C# switch requires `break`, `return`, or `throw` (no fall-through)

### String Handling

**VB6:**
```vb
' String concatenation
Dim msg As String
msg = "Hello " & userName & "!"

' String comparison (case-insensitive by default with Option Compare Text)
If Strings.StringsEqual(processID, "repeat last action", True) Then
    ' ...
End If

' String functions
Dim length As Long
length = Len(str)                ' Length
Dim upper As String
upper = UCase(str)               ' Uppercase
Dim lower As String
lower = LCase(str)               ' Lowercase
Dim trimmed As String
trimmed = Trim(str)              ' Trim spaces

' Substring
Dim sub As String
sub = Mid$(str, 5, 10)           ' Start at position 5, length 10

' String replacement
str = Replace$(str, vbCrLf, vbNullString)

' Check for empty string
If LenB(processParameters) <> 0 Then
    ' String is not empty
End If
```

**C#:**
```csharp
// String concatenation (use interpolation)
string msg = $"Hello {userName}!";

// String comparison
if (string.Equals(processID, "repeat last action",
    StringComparison.OrdinalIgnoreCase))
{
    // ...
}

// String methods
int length = str.Length;                              // Length
string upper = str.ToUpper();                         // Uppercase
string lower = str.ToLower();                         // Lowercase
string trimmed = str.Trim();                          // Trim spaces

// Substring
string sub = str.Substring(4, 10);                    // Start at index 4, length 10

// String replacement
str = str.Replace("\r\n", string.Empty);

// Check for empty string
if (!string.IsNullOrEmpty(processParameters))
{
    // String is not empty
}

// Modern null check
if (processParameters?.Length > 0)
{
    // String is not null or empty
}
```

**Key Differences:**
- VB6 uses `&` for concatenation; C# uses `+` or string interpolation `$"..."`
- VB6 string positions are 1-based; C# is 0-based
- VB6 `Len()` → C# `.Length` property
- VB6 `LenB()` checks byte length; C# use `string.IsNullOrEmpty()` or `?.Length`
- VB6 `Replace$()` → C# `.Replace()` method
- VB6 comparison can be case-insensitive with `Option Compare Text`; C# requires explicit `StringComparison`

### Collections and Arrays

**VB6:**
```vb
' Dynamic array
Dim layers() As pdLayer
ReDim layers(0 To 10)
ReDim Preserve layers(0 To 20)  ' Preserve existing data

' Collection
Dim coll As Collection
Set coll = New Collection
coll.Add item, key:="myKey"
Dim value As Variant
value = coll("myKey")

' Dictionary (using Scripting.Dictionary)
Dim dict As Scripting.Dictionary
Set dict = New Scripting.Dictionary
dict.Add "key1", "value1"
If dict.Exists("key1") Then
    dict("key1") = "newValue"
End If

' Array bounds
Dim lbound As Long, ubound As Long
lbound = LBound(layers)
ubound = UBound(layers)
```

**C#:**
```csharp
// Arrays (fixed size, prefer List<T>)
Layer[] layers = new Layer[11];  // 0-10 (11 elements)

// List (dynamic, preferred)
List<Layer> layers = new();
layers.Add(newLayer);
layers.Insert(0, newLayer);
layers.RemoveAt(5);

// Dictionary
Dictionary<string, string> dict = new();
dict.Add("key1", "value1");
dict["key1"] = "newValue";  // Add or update

if (dict.ContainsKey("key1"))
{
    string value = dict["key1"];
}

// TryGetValue (safer, no exception if key missing)
if (dict.TryGetValue("key1", out string value))
{
    // Use value
}

// ObservableCollection (for data binding)
ObservableCollection<Layer> layers = new();
// UI automatically updates when items added/removed
```

**Key Differences:**
- VB6 arrays require `ReDim`; C# arrays are fixed-size (use `List<T>` for dynamic)
- VB6 `Collection` → C# `List<T>` or `ObservableCollection<T>`
- VB6 `Dictionary` → C# `Dictionary<TKey, TValue>`
- VB6 arrays can be 1-based; C# arrays are always 0-based
- VB6 `Preserve` keyword; C# use `List<T>.Add()` instead

### Property Procedures

**VB6 (from pdDIB.cls):**
```vb
' Property Get
Friend Function GetAlphaPremultiplication() As Boolean
    GetAlphaPremultiplication = m_IsAlphaPremultiplied
End Function

' Property Let/Set
Friend Sub SetInitialAlphaPremultiplicationState(ByVal newState As Boolean)
    m_IsAlphaPremultiplied = newState
End Sub

' Property with complex logic
Friend Sub SetDPI(ByVal xRes As Double, ByVal yRes As Double)
    If (xRes = 0#) Then xRes = 96#
    If (yRes = 0#) Then yRes = 96#
    m_XResolution = xRes
    m_YResolution = yRes
    m_dibDPI = (xRes + yRes) * 0.5
End Sub

Friend Function GetDPI() As Double
    GetDPI = m_dibDPI
End Function
```

**C#:**
```csharp
// Auto-property (simple)
public bool IsAlphaPremultiplied { get; set; }

// Property with backing field
private bool m_IsAlphaPremultiplied;
public bool IsAlphaPremultiplied
{
    get => m_IsAlphaPremultiplied;
    set => m_IsAlphaPremultiplied = value;
}

// Read-only property
public bool IsAlphaPremultiplied => m_IsAlphaPremultiplied;

// Property with complex logic
private double m_XResolution;
private double m_YResolution;
private double m_dibDPI;

public void SetDPI(double xRes, double yRes)
{
    if (xRes == 0.0) xRes = 96.0;
    if (yRes == 0.0) yRes = 96.0;
    m_XResolution = xRes;
    m_YResolution = yRes;
    m_dibDPI = (xRes + yRes) * 0.5;
}

public double GetDPI() => m_dibDPI;

// Better: Use property with setter
public double DPI
{
    get => m_dibDPI;
    private set
    {
        double xRes = value;
        double yRes = value;
        if (xRes == 0.0) xRes = 96.0;
        if (yRes == 0.0) yRes = 96.0;
        m_XResolution = xRes;
        m_YResolution = yRes;
        m_dibDPI = (xRes + yRes) * 0.5;
    }
}
```

**Key Differences:**
- VB6 uses `Property Get`/`Property Let`/`Property Set`; C# uses `get`/`set` accessors
- VB6 requires function return via assignment; C# uses `return` or expression body
- C# has auto-properties for simple getters/setters
- C# properties can have different access levels (e.g., `public get; private set;`)

---

## Common Pattern Translations

### Form/Control Initialization

**VB6 (from MainWindow.frm):**
```vb
Private Sub Form_Load()
    ' Form initialization
    Me.Caption = "PhotoDemon by Tanner Helland"
    Me.BackColor = &H80000005&

    ' Load controls
    Set HotkeyManager = New pdAccelerator
    Set MainCanvas = New pdCanvas

    ' Initialize state
    InitializeMenus
    LoadUserSettings

    ' Continue with program initialization
    ContinueLoadingProgram
End Sub

Private Sub Form_Resize()
    ' Handle resize
    If (Me.WindowState <> vbMinimized) Then
        LayoutControls
    End If
End Sub

Private Sub Form_Unload(Cancel As Integer)
    ' Cleanup
    SaveUserSettings
    UnloadAllImages
End Sub
```

**C# MAUI:**
```csharp
/// <summary>
/// MainPage - replaces VB6 FormMain
/// Reference: /home/user/PhotoDemon/Forms/MainWindow.frm
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly ISettingsService _settings;

    public MainPage(MainViewModel viewModel, ISettingsService settings)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _settings = settings;

        BindingContext = _viewModel;

        // Subscribe to lifecycle events
        Loaded += OnPageLoaded;
        Unloaded += OnPageUnloaded;
        SizeChanged += OnPageSizeChanged;
    }

    private void OnPageLoaded(object sender, EventArgs e)
    {
        // Form_Load equivalent
        Title = "PhotoDemon by Tanner Helland";

        // Initialize
        InitializeMenus();
        _settings.LoadUserSettings();

        // Continue initialization
        _ = ContinueProgramInitializationAsync();
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        // Form_Resize equivalent
        if (Window?.Page?.Width > 0)
        {
            LayoutControls();
        }
    }

    private void OnPageUnloaded(object sender, EventArgs e)
    {
        // Form_Unload equivalent
        _settings.SaveUserSettings();
        _viewModel.UnloadAllImages();
    }

    private async Task ContinueProgramInitializationAsync()
    {
        await _viewModel.InitializeAsync();
    }
}
```

**XAML (MainPage.xaml):**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:PhotoDemon.ViewModels"
             x:Class="PhotoDemon.Views.MainPage"
             x:DataType="vm:MainViewModel"
             Title="{Binding Title}">

    <Shell.TitleView>
        <Grid>
            <Label Text="PhotoDemon by Tanner Helland"
                   FontSize="20"
                   VerticalOptions="Center"/>
        </Grid>
    </Shell.TitleView>

    <Grid>
        <!-- Main canvas area -->
        <views:ImageCanvasView x:Name="MainCanvas"
                               Grid.Row="0"
                               ActiveDocument="{Binding ActiveImage}"/>

        <!-- Toolbars, panels, etc. -->
    </Grid>
</ContentPage>
```

**Key Patterns:**
- VB6 `Form_Load` → C# `Loaded` event or constructor
- VB6 `Form_Resize` → C# `SizeChanged` event
- VB6 `Form_Unload` → C# `Unloaded` event or `IDisposable`
- VB6 controls created at design time; MAUI uses XAML + data binding
- VB6 `Set obj = New Class` → C# dependency injection via constructor

### Event Handling

**VB6:**
```vb
' Menu click event
Private Sub MnuFile_Click(Index As Integer)
    Select Case Index
        Case 0  ' New
            Process "New Image", True
        Case 1  ' Open
            Process "Open", True
        Case 8  ' Save
            Process "Save", False
    End Select
End Sub

' Control events
Private Sub MainCanvas_MouseDown(Button As Integer, Shift As Integer, x As Single, y As Single)
    If (Button = vbLeftButton) Then
        StartDrawing x, y
    End If
End Sub

Private Sub sltBrightness_Change()
    UpdatePreview
End Sub
```

**C# MAUI (XAML):**
```xml
<!-- Menu item binding to command -->
<MenuItem Text="New..."
          Command="{Binding NewImageCommand}"/>

<MenuItem Text="Open..."
          Command="{Binding OpenImageCommand}"/>

<MenuItem Text="Save"
          Command="{Binding SaveImageCommand}"/>

<!-- Control event handlers -->
<Slider x:Name="sltBrightness"
        Minimum="-255"
        Maximum="255"
        Value="{Binding Brightness}"
        ValueChanged="OnBrightnessChanged"/>
```

**C# MAUI (Code-behind):**
```csharp
// ViewModel with commands
public partial class MainViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task NewImageAsync()
    {
        await _navigation.NavigateToAsync("newimage");
    }

    [RelayCommand]
    private async Task OpenImageAsync()
    {
        var files = await FilePicker.PickMultipleAsync();
        // Process files
    }

    [RelayCommand(CanExecute = nameof(CanSaveImage))]
    private async Task SaveImageAsync()
    {
        await _imageService.SaveImageAsync(ActiveImage);
    }

    private bool CanSaveImage() => ActiveImage != null;
}

// View code-behind (minimal)
private void OnBrightnessChanged(object sender, ValueChangedEventArgs e)
{
    // Optional: Handle in code-behind if needed
    // Prefer binding to ViewModel property instead
}
```

**Better: Pure XAML Binding (Preferred):**
```xml
<!-- Slider bound directly to ViewModel property -->
<Slider Value="{Binding Brightness, Mode=TwoWay}"/>
```

```csharp
// ViewModel automatically updates preview on property change
public partial class BrightnessContrastViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _brightness;

    partial void OnBrightnessChanged(int value)
    {
        // Automatically called when Brightness changes
        _ = UpdatePreviewAsync();
    }
}
```

**Key Differences:**
- VB6 uses event procedures like `Control_EventName`; C# uses event handlers or commands
- VB6 control arrays with `Index`; C# each control has unique name
- MAUI prefers MVVM pattern: View → ViewModel → Command
- Use `ICommand` (via `RelayCommand`) instead of event handlers
- Data binding replaces manual property updates

### Module-Level Code → Static Classes / Dependency Injection

**VB6 (Processor.bas):**
```vb
' Module with global functions
Option Explicit

Private m_Processing As Boolean
Private m_LastProcess As PD_ProcessCall

Public Sub Process(ByVal processID As String, _
                   Optional raiseDialog As Boolean = False, _
                   Optional processParameters As String = vbNullString)

    On Error GoTo MainErrHandler

    m_Processing = True

    ' ... processing logic

    Exit Sub

MainErrHandler:
    HandleError Err.Number, Err.Description
End Sub
```

**C# (Two Approaches):**

**Approach 1: Static Class (for utility functions only):**
```csharp
/// <summary>
/// String utilities - pure functions with no state
/// Reference: /home/user/PhotoDemon/Modules/Strings.bas
/// </summary>
public static class StringHelpers
{
    public static bool StringsEqual(string str1, string str2, bool ignoreCase)
    {
        return string.Equals(str1, str2,
            ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    public static string RemoveLineBreaks(string str)
    {
        return str.Replace("\r\n", string.Empty)
                  .Replace("\n", string.Empty);
    }
}
```

**Approach 2: Service with Dependency Injection (preferred for stateful modules):**
```csharp
/// <summary>
/// Effect processor service
/// Reference: /home/user/PhotoDemon/Modules/Processor.bas
/// </summary>
public interface IEffectService
{
    Task ProcessAsync(
        string processID,
        bool raiseDialog = false,
        string processParameters = "");
}

public class EffectService : IEffectService
{
    private readonly IImageDocumentService _imageService;
    private readonly IUndoService _undoService;
    private readonly ILogger<EffectService> _logger;

    // State (previously module-level variables)
    private bool m_Processing;
    private PD_ProcessCall m_LastProcess;

    public EffectService(
        IImageDocumentService imageService,
        IUndoService undoService,
        ILogger<EffectService> logger)
    {
        _imageService = imageService;
        _undoService = undoService;
        _logger = logger;
    }

    public async Task ProcessAsync(
        string processID,
        bool raiseDialog = false,
        string processParameters = "")
    {
        try
        {
            m_Processing = true;

            // ... processing logic

            await ApplyEffectAsync(processID, processParameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {ProcessID}", processID);
            throw;
        }
        finally
        {
            m_Processing = false;
        }
    }

    private async Task ApplyEffectAsync(string processID, string parameters)
    {
        // Implementation
        await Task.CompletedTask;
    }
}
```

**Registration in MauiProgram.cs:**
```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Register services
        builder.Services.AddTransient<IEffectService, EffectService>();
        builder.Services.AddSingleton<IImageDocumentService, ImageDocumentService>();
        builder.Services.AddTransient<IUndoService, UndoService>();

        return builder.Build();
    }
}
```

**Key Decisions:**
- **VB6 Module with only pure functions** → C# `static class`
- **VB6 Module with state** → C# service class with DI
- Module-level variables → instance fields in service class
- Global functions → methods on injected service

**Reference:** `/home/user/PhotoDemon/docs/architecture/maui-architecture.md` (lines 755-941)

### Optional Parameters and Default Values

**VB6:**
```vb
' VB6 supports optional parameters with defaults
Public Sub Process(ByVal processID As String, _
                   Optional raiseDialog As Boolean = False, _
                   Optional processParameters As String = vbNullString, _
                   Optional createUndo As PD_UndoType = UNDO_Nothing)
    ' ...
End Sub

' Can be called various ways:
Process "Blur"
Process "Blur", True
Process "Blur", False, "radius|5.0"
Process "Blur", createUndo:=UNDO_Image  ' Named parameters
```

**C#:**
```csharp
// C# also supports optional parameters
public async Task ProcessAsync(
    string processID,
    bool raiseDialog = false,
    string processParameters = "",
    UndoType createUndo = UndoType.Nothing)
{
    // ...
}

// Called the same way:
await ProcessAsync("Blur");
await ProcessAsync("Blur", true);
await ProcessAsync("Blur", false, "radius|5.0");
await ProcessAsync("Blur", createUndo: UndoType.Image);  // Named arguments
```

**Key Differences:**
- Both support optional parameters with defaults
- VB6 uses `:=` for named parameters; C# uses `:`
- VB6 `vbNullString` → C# `string.Empty` or `""`
- C# requires default values to be compile-time constants

### ByRef vs ByVal → ref/out Parameters

**VB6:**
```vb
' ByVal - pass by value (copy)
Private Sub ModifyValue(ByVal x As Long)
    x = x + 1  ' Does NOT modify caller's variable
End Sub

' ByRef - pass by reference (default in VB6!)
Private Sub ModifyReference(ByRef x As Long)
    x = x + 1  ' DOES modify caller's variable
End Sub

' Example usage
Dim value As Long
value = 10
ModifyValue value        ' value is still 10
ModifyReference value    ' value is now 11
```

**C#:**
```csharp
// By value (default in C#)
private void ModifyValue(int x)
{
    x = x + 1;  // Does NOT modify caller's variable
}

// ref - pass by reference
private void ModifyReference(ref int x)
{
    x = x + 1;  // DOES modify caller's variable
}

// out - must be assigned before returning
private void GetValue(out int x)
{
    x = 42;  // MUST assign a value
}

// Example usage
int value = 10;
ModifyValue(value);           // value is still 10
ModifyReference(ref value);   // value is now 11

GetValue(out int result);     // result is 42
// Can also use:
int result2;
GetValue(out result2);
```

**Key Differences:**
- **VB6 default is `ByRef`; C# default is by-value**
- VB6 `ByRef` → C# `ref` (must use `ref` at call site)
- C# has `out` for return values (must be assigned)
- **CRITICAL:** When migrating, check all VB6 functions - they default to `ByRef`!

**Example from PhotoDemon:**
```vb
' VB6 - implicit ByRef
Private Declare Function GlobalMemoryStatusEx Lib "kernel32" (lpBuffer As MEMORYSTATUSEX) As Long

' This modifies lpBuffer because ByRef is default
```

```csharp
// C# - explicit ref required
[DllImport("kernel32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

// Must use ref at call site
var memStatus = new MEMORYSTATUSEX();
memStatus.dwLength = (uint)Marshal.SizeOf(memStatus);
if (GlobalMemoryStatusEx(ref memStatus))
{
    // Use memStatus
}
```

---

## Error Handling Migration

### On Error GoTo → try/catch/finally

**VB6 (from Processor.bas:92):**
```vb
Public Sub Process(ByVal processID As String, Optional raiseDialog As Boolean = False)
    'Main error handler for the software processor
    On Error GoTo MainErrHandler

    'Processing logic here
    m_Processing = True

    If raiseDialog Then
        ShowDialog processID
    Else
        ApplyEffect processID
    End If

    Exit Sub

MainErrHandler:
    PDDebug.LogAction "Error in Processor.Process: " & Err.Description, PDM_Error

    'Display friendly error to user
    Message "An error occurred: " & Err.Description, vbCritical

    'Clean up
    m_Processing = False
End Sub
```

**C#:**
```csharp
public async Task ProcessAsync(string processID, bool raiseDialog = false)
{
    try
    {
        // Processing logic here
        m_Processing = true;

        if (raiseDialog)
        {
            await ShowDialogAsync(processID);
        }
        else
        {
            await ApplyEffectAsync(processID);
        }
    }
    catch (OutOfMemoryException ex)
    {
        // Handle specific exception
        _logger.LogError(ex, "Out of memory in Process");
        await _dialogs.ShowErrorAsync("Out of Memory",
            "Not enough memory to complete this operation.");
    }
    catch (OperationCanceledException)
    {
        // User cancelled - this is not an error
        _logger.LogInformation("Operation cancelled by user");
    }
    catch (Exception ex)
    {
        // Handle general exception
        _logger.LogError(ex, "Error in ProcessAsync: {ProcessID}", processID);

        // Display friendly error to user
        await _dialogs.ShowErrorAsync("Error",
            $"An error occurred: {ex.Message}");
    }
    finally
    {
        // Clean up (always executes)
        m_Processing = false;
    }
}
```

**Key Differences:**
- VB6 `On Error GoTo Label` → C# `try-catch-finally`
- VB6 `Err.Description` → C# `ex.Message`
- VB6 `Err.Number` → C# exception type
- VB6 has single error handler; C# can catch specific exception types
- C# `finally` block always executes (for cleanup)
- **IMPORTANT:** VB6 `On Error Resume Next` → C# handle exceptions explicitly (don't ignore!)

### On Error Resume Next (Dangerous!)

**VB6:**
```vb
' VB6 - ignores errors (BAD PRACTICE)
On Error Resume Next
Set obj = Nothing
DeleteFile filename
On Error GoTo 0  ' Reset error handling
```

**C# (Don't do this!):**
```csharp
// C# - DON'T silently swallow exceptions!
try
{
    obj?.Dispose();
    File.Delete(filename);
}
catch
{
    // Empty catch - BAD! Never do this
}
```

**C# (Correct approach):**
```csharp
// C# - Handle expected exceptions explicitly
try
{
    obj?.Dispose();
}
catch (ObjectDisposedException)
{
    // Expected - object already disposed
}

// File operations - check first
if (File.Exists(filename))
{
    try
    {
        File.Delete(filename);
    }
    catch (IOException ex)
    {
        _logger.LogWarning(ex, "Could not delete file {FileName}", filename);
        // Continue execution
    }
}
```

### Err.Raise → Throwing Exceptions

**VB6:**
```vb
Public Function LoadImage(ByVal filePath As String) As pdDIB
    If Not FileExists(filePath) Then
        Err.Raise vbObjectError + 1000, "LoadImage", "File not found: " & filePath
    End If

    If Not IsValidImageFile(filePath) Then
        Err.Raise vbObjectError + 1001, "LoadImage", "Invalid image file format"
    End If

    ' Load image...
End Function
```

**C#:**
```csharp
public async Task<BitmapBuffer> LoadImageAsync(string filePath)
{
    if (!File.Exists(filePath))
    {
        throw new FileNotFoundException(
            $"File not found: {filePath}",
            filePath);
    }

    if (!IsValidImageFile(filePath))
    {
        throw new InvalidImageFormatException(
            "Invalid image file format",
            filePath);
    }

    // Load image...
}
```

**Custom Exception Types:**
```csharp
/// <summary>
/// Exception for invalid image formats
/// </summary>
public class InvalidImageFormatException : Exception
{
    public string FilePath { get; }

    public InvalidImageFormatException(string message, string filePath)
        : base(message)
    {
        FilePath = filePath;
    }

    public InvalidImageFormatException(string message, string filePath, Exception innerException)
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}
```

**Usage:**
```csharp
try
{
    var image = await LoadImageAsync(filePath);
}
catch (FileNotFoundException ex)
{
    await _dialogs.ShowErrorAsync("File Not Found",
        $"The file '{ex.FileName}' could not be found.");
}
catch (InvalidImageFormatException ex)
{
    await _dialogs.ShowErrorAsync("Invalid Format",
        $"The file '{ex.FilePath}' is not a valid image format.");
}
catch (Exception ex)
{
    await _dialogs.ShowErrorAsync("Error",
        $"An unexpected error occurred: {ex.Message}");
}
```

### Error Handling Best Practices

**❌ VB6 Bad Pattern:**
```vb
On Error Resume Next
' Do everything
On Error GoTo 0
```

**✅ C# Good Pattern:**
```csharp
// Handle specific exceptions, let unexpected ones bubble up
try
{
    await PerformOperationAsync();
}
catch (OperationCanceledException)
{
    // User cancelled - not an error
}
catch (OutOfMemoryException ex)
{
    // Critical error - log and inform user
    _logger.LogError(ex, "Out of memory");
    throw; // Re-throw to let app handle
}
// Let other exceptions propagate
```

**✅ C# Pattern with Cleanup:**
```csharp
// Use 'using' for automatic disposal
using (var stream = File.OpenRead(path))
{
    await ProcessStreamAsync(stream);
}
// Stream automatically closed/disposed even if exception occurs

// For objects not in 'using', use try-finally
BitmapBuffer buffer = null;
try
{
    buffer = new BitmapBuffer(width, height);
    await ProcessBufferAsync(buffer);
}
finally
{
    buffer?.Dispose();
}
```

---

## Resource Management

### GDI/GDI+ → MAUI Graphics/SkiaSharp

**VB6 (from pdDIB.cls):**
```vb
Private Declare Function CreateDIBSection Lib "gdi32" _
    (ByVal hDC As Long, lpBitsInfo As BITMAPINFOHEADER, _
     ByVal wUsage As Long, ByRef lpBits As Long, _
     ByVal hSection As Long, ByVal dwOffset As Long) As Long

Private Declare Function SelectObject Lib "gdi32" _
    (ByVal hDC As Long, ByVal hObject As Long) As Long

Private Declare Function DeleteObject Lib "gdi32" _
    (ByVal hObject As Long) As Long

' Create DIB
Private Function CreateDIB(ByVal dibWidth As Long, ByVal dibHeight As Long) As Boolean
    ' Create DIB section
    m_dibHandle = CreateDIBSection(m_dibDC, m_dibHeader, 0, m_dibBits, 0, 0)

    If (m_dibHandle <> 0) Then
        ' Select into DC
        m_dibHandleOriginal = SelectObject(m_dibDC, m_dibHandle)
        CreateDIB = True
    End If
End Function

' Cleanup
Private Sub FreeDIB()
    If (m_dibHandle <> 0) Then
        SelectObject m_dibDC, m_dibHandleOriginal
        DeleteObject m_dibHandle
        m_dibHandle = 0
    End If
End Sub
```

**C# MAUI with SkiaSharp:**
```csharp
/// <summary>
/// Bitmap buffer using SkiaSharp
/// Reference: /home/user/PhotoDemon/Classes/pdDIB.cls
/// </summary>
public class BitmapBuffer : IDisposable
{
    private SKBitmap _bitmap;
    private bool _disposed;

    public int Width => _bitmap?.Width ?? 0;
    public int Height => _bitmap?.Height ?? 0;
    public int Stride => _bitmap?.RowBytes ?? 0;

    public BitmapBuffer(int width, int height)
    {
        // SkiaSharp creates bitmap in managed memory
        _bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
    }

    /// <summary>
    /// Get direct pixel access
    /// Replaces: VB6 m_dibBits pointer + SafeArray manipulation
    /// </summary>
    public unsafe Span<byte> GetPixelData()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        IntPtr ptr = _bitmap.GetPixels();
        int length = Height * Stride;
        return new Span<byte>(ptr.ToPointer(), length);
    }

    /// <summary>
    /// Get typed pixel access
    /// </summary>
    public unsafe Span<Rgba32> GetPixels()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        IntPtr ptr = _bitmap.GetPixels();
        int pixelCount = Width * Height;
        return new Span<Rgba32>(ptr.ToPointer(), pixelCount);
    }

    /// <summary>
    /// Create SKImage for rendering
    /// </summary>
    public SKImage ToImage()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return SKImage.FromBitmap(_bitmap);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _bitmap?.Dispose();
            _bitmap = null;
        }

        _disposed = true;
    }

    ~BitmapBuffer()
    {
        Dispose(false);
    }
}
```

**Usage Comparison:**

**VB6:**
```vb
Dim srcDIB As pdDIB
Set srcDIB = New pdDIB
srcDIB.CreateBlank 1920, 1080, 32

' Access pixels
Dim srcPixels() As Byte
srcDIB.WrapArrayAroundDIB srcPixels

' Modify pixels
For y = 0 To srcDIB.GetDIBHeight - 1
    For x = 0 To srcDIB.GetDIBWidth - 1
        Dim pixelPos As Long
        pixelPos = y * srcDIB.GetDIBStride + x * 4
        srcPixels(pixelPos + 2) = 255  ' Red
    Next x
Next y

srcDIB.UnwrapArrayFromDIB srcPixels

' Cleanup
Set srcDIB = Nothing
```

**C#:**
```csharp
using var srcBuffer = new BitmapBuffer(1920, 1080);

// Access pixels
Span<Rgba32> pixels = srcBuffer.GetPixels();

// Modify pixels (much simpler!)
for (int y = 0; y < srcBuffer.Height; y++)
{
    for (int x = 0; x < srcBuffer.Width; x++)
    {
        int pixelPos = y * srcBuffer.Width + x;
        pixels[pixelPos].R = 255;  // Red
    }
}

// Automatic cleanup via 'using'
```

**Key Differences:**
- VB6 uses raw GDI handles; C# uses managed SkiaSharp objects
- VB6 requires manual cleanup (`DeleteObject`); C# uses `IDisposable` pattern
- VB6 `SafeArray` manipulation; C# uses `Span<T>` for safe, efficient access
- C# `using` statement ensures automatic cleanup

### Object Lifetime: Set obj = Nothing → using/IDisposable

**VB6:**
```vb
' Manual cleanup required
Dim srcDIB As pdDIB
Set srcDIB = New pdDIB
srcDIB.CreateBlank 1920, 1080, 32

' Use srcDIB...

' Manual cleanup
Set srcDIB = Nothing  ' Calls Class_Terminate
```

**C# (IDisposable Pattern):**
```csharp
// Manual disposal
BitmapBuffer srcBuffer = new BitmapBuffer(1920, 1080);
try
{
    // Use srcBuffer...
}
finally
{
    srcBuffer?.Dispose();  // Ensure cleanup
}

// Better: using statement (automatic disposal)
using (var srcBuffer = new BitmapBuffer(1920, 1080))
{
    // Use srcBuffer...
}
// Automatically disposed here

// Even better: using declaration (C# 8+)
using var srcBuffer = new BitmapBuffer(1920, 1080);
// Use srcBuffer...
// Automatically disposed at end of scope
```

**Implementing IDisposable:**
```csharp
public class ImageDocument : IDisposable
{
    private BitmapBuffer _compositeBuffer;
    private ObservableCollection<Layer> _layers = new();
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Dispose managed resources
            _compositeBuffer?.Dispose();

            foreach (var layer in _layers)
            {
                layer?.Dispose();
            }
            _layers.Clear();
        }

        // Free unmanaged resources (if any)
        // None in this example

        _disposed = true;
    }

    ~ImageDocument()
    {
        Dispose(false);
    }
}
```

**Reference:** `/home/user/PhotoDemon/docs/architecture/maui-architecture.md` (lines 2236-2274)

### Memory Management Differences

**VB6 (Reference Counting):**
```vb
' VB6 uses reference counting
Dim obj1 As pdDIB
Set obj1 = New pdDIB  ' RefCount = 1

Dim obj2 As pdDIB
Set obj2 = obj1       ' RefCount = 2

Set obj1 = Nothing    ' RefCount = 1
Set obj2 = Nothing    ' RefCount = 0, object destroyed immediately
```

**C# (Garbage Collection):**
```csharp
// C# uses garbage collection
BitmapBuffer obj1 = new BitmapBuffer(100, 100);  // Object created

BitmapBuffer obj2 = obj1;  // Both reference same object

obj1 = null;  // Reference removed
obj2 = null;  // Last reference removed

// Object is NOT immediately destroyed!
// GC will collect it at some point in the future

// For resources, use IDisposable:
using var obj = new BitmapBuffer(100, 100);
// Object disposed deterministically at end of using block
```

**Key Differences:**
- VB6: Objects destroyed when last reference removed
- C#: Objects collected by GC at unpredictable times
- C#: Use `IDisposable` for deterministic cleanup of resources
- C#: `using` statement ensures cleanup even if exception occurs

### Handle Management

**VB6:**
```vb
' Manual handle management
Private m_dibDC As Long
Private m_dibHandle As Long
Private m_dibHandleOriginal As Long

Private Sub CreateDIB()
    m_dibDC = CreateCompatibleDC(0)
    m_dibHandle = CreateDIBSection(...)
    m_dibHandleOriginal = SelectObject(m_dibDC, m_dibHandle)
End Sub

Private Sub FreeDIB()
    If (m_dibHandle <> 0) Then
        SelectObject m_dibDC, m_dibHandleOriginal
        DeleteObject m_dibHandle
        m_dibHandle = 0
    End If

    If (m_dibDC <> 0) Then
        DeleteDC m_dibDC
        m_dibDC = 0
    End If
End Sub
```

**C#:**
```csharp
// SafeHandle for native resources
public sealed class SafeBitmapHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public SafeBitmapHandle() : base(true)
    {
    }

    protected override bool ReleaseHandle()
    {
        return DeleteObject(handle);
    }

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
}

// Usage
public class NativeBitmap : IDisposable
{
    private SafeBitmapHandle _handle;

    public NativeBitmap(int width, int height)
    {
        // Create native bitmap
        IntPtr hBitmap = CreateDIBSection(...);
        _handle = new SafeBitmapHandle();
        _handle.SetHandle(hBitmap);
    }

    public void Dispose()
    {
        _handle?.Dispose();  // Automatically calls ReleaseHandle
    }
}
```

**Best Practice: Avoid Native Handles**
```csharp
// Better: Use SkiaSharp (managed wrapper)
using var bitmap = new SKBitmap(width, height);
// No manual handle management needed!
```

---

## Threading Model Changes

### DoEvents → async/await Patterns

**VB6:**
```vb
Public Sub ApplyBlur(ByVal radius As Double)
    Dim i As Long

    For i = 0 To 100
        ' Long-running operation
        ProcessRow i

        ' Allow UI to update
        DoEvents

        ' Update progress
        UpdateProgressBar i
    Next i
End Sub
```

**C# (WRONG - blocks UI thread):**
```csharp
// DON'T DO THIS - blocks UI thread
public void ApplyBlur(double radius)
{
    for (int i = 0; i <= 100; i++)
    {
        ProcessRow(i);

        // No equivalent to DoEvents!
        // UI will freeze!

        UpdateProgressBar(i);
    }
}
```

**C# (CORRECT - async/await):**
```csharp
public async Task ApplyBlurAsync(double radius, IProgress<int> progress = null)
{
    // Move to background thread
    await Task.Run(() =>
    {
        for (int i = 0; i <= 100; i++)
        {
            ProcessRow(i);

            // Report progress (marshals to UI thread)
            progress?.Report(i);
        }
    });
}

// ViewModel
public partial class EffectViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task ApplyAsync()
    {
        IsBusy = true;

        try
        {
            var progress = new Progress<int>(value =>
            {
                // This runs on UI thread
                ProgressValue = value;
            });

            await ApplyBlurAsync(Radius, progress);

            await _navigation.GoBackAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

**Key Patterns:**
- **VB6 `DoEvents`** → **C# `async`/`await` + background thread**
- Long operations MUST be async in MAUI
- Use `Task.Run()` for CPU-bound work
- Use `IProgress<T>` for progress reporting
- UI updates automatically marshaled to UI thread via `IProgress<T>`

### Background Operations

**VB6:**
```vb
' VB6 - limited threading, uses DoEvents
Public Sub ProcessImage()
    Dim totalSteps As Long
    totalSteps = 1000

    For i = 1 To totalSteps
        ' Process
        DoSomething i

        ' Update progress
        If (i Mod 10 = 0) Then
            UpdateProgress CDbl(i) / totalSteps
            DoEvents
        End If
    Next i
End Sub
```

**C# MAUI:**
```csharp
public async Task ProcessImageAsync(
    CancellationToken cancellationToken = default)
{
    int totalSteps = 1000;

    await Task.Run(() =>
    {
        for (int i = 1; i <= totalSteps; i++)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Process
            DoSomething(i);

            // Report progress every 10 steps
            if (i % 10 == 0)
            {
                double progressPercent = (double)i / totalSteps;
                // Progress automatically marshaled to UI thread
                OnProgressChanged(progressPercent);
            }
        }
    }, cancellationToken);
}
```

**With Progress Reporting:**
```csharp
public async Task ProcessImageAsync(
    IProgress<ProgressInfo> progress = null,
    CancellationToken cancellationToken = default)
{
    int totalSteps = 1000;

    await Task.Run(() =>
    {
        for (int i = 1; i <= totalSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DoSomething(i);

            if (i % 10 == 0)
            {
                progress?.Report(new ProgressInfo
                {
                    CurrentStep = i,
                    TotalSteps = totalSteps,
                    Message = $"Processing step {i} of {totalSteps}",
                    PercentComplete = (double)i / totalSteps * 100
                });
            }
        }
    }, cancellationToken);
}

public class ProgressInfo
{
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string Message { get; set; }
    public double PercentComplete { get; set; }
}
```

### UI Thread Synchronization

**VB6:**
```vb
' VB6 - single-threaded, no synchronization needed
Private Sub WorkerComplete()
    ' Always on UI thread
    txtStatus.Text = "Complete!"
    cmdProcess.Enabled = True
End Sub
```

**C# MAUI:**
```csharp
// C# - must marshal to UI thread
private async Task WorkerCompleteAsync()
{
    // If on background thread, marshal to UI thread
    await MainThread.InvokeOnMainThreadAsync(() =>
    {
        // This code runs on UI thread
        txtStatus.Text = "Complete!";
        cmdProcess.IsEnabled = true;
    });
}

// Better: Use IProgress<T> to automatically marshal
private async Task DoWorkAsync()
{
    var progress = new Progress<string>(message =>
    {
        // Automatically on UI thread
        txtStatus.Text = message;
    });

    await Task.Run(() =>
    {
        // Background thread
        ProcessData();

        // Report completion (automatically marshals to UI thread)
        progress.Report("Complete!");
    });
}
```

### Thread Safety Considerations

**VB6 (not thread-safe, but single-threaded):**
```vb
Private m_Counter As Long

Public Sub IncrementCounter()
    m_Counter = m_Counter + 1  ' Safe in VB6 (single-threaded)
End Sub
```

**C# (requires synchronization):**
```csharp
private int m_Counter;
private readonly object m_Lock = new();

public void IncrementCounter()
{
    // Thread-safe with lock
    lock (m_Lock)
    {
        m_Counter = m_Counter + 1;
    }
}

// Better: Use Interlocked for simple operations
public void IncrementCounter()
{
    Interlocked.Increment(ref m_Counter);
}

// Best: Avoid shared state; use immutable objects
```

**Async-Safe Collections:**
```csharp
// NOT thread-safe
private List<ImageDocument> _images = new();

// Thread-safe alternatives
private ConcurrentBag<ImageDocument> _images = new();
private ConcurrentQueue<ImageDocument> _imageQueue = new();

// Or use locks
private readonly object _imagesLock = new();
private List<ImageDocument> _images = new();

public void AddImage(ImageDocument image)
{
    lock (_imagesLock)
    {
        _images.Add(image);
    }
}
```

### Cancellation Support

**VB6:**
```vb
' VB6 - manual flag checking
Private m_CancelRequested As Boolean

Public Sub ProcessImages()
    m_CancelRequested = False

    For i = 1 To 1000
        If m_CancelRequested Then Exit For

        ProcessImage i
        DoEvents  ' Allow cancel button to be clicked
    Next i
End Sub

Private Sub cmdCancel_Click()
    m_CancelRequested = True
End Sub
```

**C# MAUI:**
```csharp
// ViewModel
public partial class ProcessingViewModel : ViewModelBase
{
    private CancellationTokenSource _cts;

    [RelayCommand]
    private async Task ProcessImagesAsync()
    {
        _cts = new CancellationTokenSource();
        IsBusy = true;

        try
        {
            await ProcessImagesInternalAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // User cancelled - not an error
            await _dialogs.ShowMessageAsync("Cancelled",
                "Operation cancelled by user.");
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    private async Task ProcessImagesInternalAsync(
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            for (int i = 1; i <= 1000; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                ProcessImage(i);
            }
        }, cancellationToken);
    }
}
```

**XAML:**
```xml
<StackLayout>
    <Button Text="Process Images"
            Command="{Binding ProcessImagesCommand}"
            IsEnabled="{Binding IsBusy, Converter={StaticResource InverseBoolConverter}}"/>

    <Button Text="Cancel"
            Command="{Binding CancelCommand}"
            IsEnabled="{Binding IsBusy}"/>

    <ActivityIndicator IsRunning="{Binding IsBusy}"/>
</StackLayout>
```

---

## Quick Reference Table

### Language Syntax

| Feature | VB6 | C# |
|---------|-----|-----|
| **Variable Declaration** | `Dim x As Long` | `int x;` |
| **Constant** | `Const MAX As Long = 100` | `const int MAX = 100;` |
| **String Concatenation** | `"Hello " & name` | `$"Hello {name}"` |
| **Comparison** | `If x = 5 Then` | `if (x == 5)` |
| **Not Equal** | `If x <> 5 Then` | `if (x != 5)` |
| **Logical AND** | `If x And y Then` | `if (x && y)` |
| **Logical OR** | `If x Or y Then` | `if (x \|\| y)` |
| **Logical NOT** | `If Not x Then` | `if (!x)` |
| **String Length** | `Len(str)` | `str.Length` |
| **Substring** | `Mid$(str, 5, 10)` | `str.Substring(4, 10)` |
| **String Replace** | `Replace$(str, "a", "b")` | `str.Replace("a", "b")` |
| **Array Index** | `arr(5)` (1-based) | `arr[4]` (0-based) |
| **Line Continuation** | `_` (underscore) | (automatic) |
| **Comments** | `'` or `Rem` | `//` or `/* */` |

### Object-Oriented

| Feature | VB6 | C# |
|---------|-----|-----|
| **Create Object** | `Set obj = New Class` | `var obj = new Class();` |
| **Destroy Object** | `Set obj = Nothing` | `obj?.Dispose();` (if IDisposable) |
| **Property Get** | `Property Get Name() As String` | `public string Name { get; }` |
| **Property Let** | `Property Let Name(v As String)` | `public string Name { get; set; }` |
| **Constructor** | `Private Sub Class_Initialize()` | `public Class() { }` |
| **Destructor** | `Private Sub Class_Terminate()` | `~Class() { }` or `Dispose()` |
| **Interface** | `Implements IInterface` | `class X : IInterface { }` |
| **Inheritance** | (Not supported) | `class Child : Parent { }` |

### Error Handling

| Feature | VB6 | C# |
|---------|-----|-----|
| **Try-Catch** | `On Error GoTo ErrHandler` | `try { } catch { }` |
| **Finally** | (Manual cleanup) | `finally { }` |
| **Resume Next** | `On Error Resume Next` | (Don't use - handle explicitly) |
| **Throw Error** | `Err.Raise number, source, desc` | `throw new Exception(msg);` |
| **Error Number** | `Err.Number` | Exception type |
| **Error Message** | `Err.Description` | `ex.Message` |

### PhotoDemon-Specific Mappings

| VB6 Component | File | C# Component | Notes |
|---------------|------|--------------|-------|
| `pdDIB` | `Classes/pdDIB.cls` | `BitmapBuffer` | Use `Span<T>` for pixels |
| `pdImage` | `Classes/pdImage.cls` | `ImageDocument` | Model with `INotifyPropertyChanged` |
| `pdLayer` | `Classes/pdLayer.cls` | `Layer` | Layer model |
| `Processor.Process` | `Modules/Processor.bas:89` | `IEffectService.ProcessAsync` | Async with `CancellationToken` |
| `FormMain` | `Forms/MainWindow.frm` | `MainPage.xaml` + `MainViewModel` | MVVM pattern |
| `DoEvents` | (throughout) | `await Task.Yield()` | Or better: proper async |
| Module variables | `Modules/*.bas` | Service instance fields | Use DI |
| Global functions | `Modules/*.bas` | Static class or service | Prefer services |

---

## Common Pitfalls and Gotchas

### 1. Array Indexing

**⚠️ VB6 arrays can be 1-based; C# arrays are ALWAYS 0-based**

```vb
' VB6
Dim arr(1 To 10) As Long  ' 1-based array
arr(1) = 5                ' First element
```

```csharp
// C#
int[] arr = new int[10];  // 0-based array (indices 0-9)
arr[0] = 5;               // First element
```

### 2. ByRef Default in VB6

**⚠️ VB6 parameters are ByRef by default; C# parameters are by-value by default**

```vb
' VB6 - implicit ByRef!
Sub Modify(x As Long)
    x = x + 1  ' DOES modify caller's variable!
End Sub
```

```csharp
// C# - by value
void Modify(int x)
{
    x = x + 1;  // Does NOT modify caller's variable
}

// Must explicitly use ref
void Modify(ref int x)
{
    x = x + 1;  // DOES modify caller's variable
}
```

### 3. String Comparison

**⚠️ VB6 can be case-insensitive by default; C# is case-sensitive**

```vb
' VB6 with Option Compare Text
Option Compare Text
If str1 = str2 Then  ' Case-insensitive!
```

```csharp
// C# - case-sensitive
if (str1 == str2)  // Case-sensitive!

// For case-insensitive:
if (string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase))
```

### 4. Nothing vs null

**⚠️ VB6 Nothing can mean different things; C# null is consistent**

```vb
' VB6
If obj Is Nothing Then        ' Check if object is Nothing
Set obj = Nothing             ' Set to Nothing
If str = vbNullString Then    ' Empty string
```

```csharp
// C#
if (obj == null)              // Check if null
obj = null;                   // Set to null
if (string.IsNullOrEmpty(str)) // Empty string
```

### 5. Integer Types

**⚠️ VB6 Integer is 16-bit, Long is 32-bit; C# int is 32-bit, long is 64-bit**

```vb
' VB6
Dim i As Integer  ' 16-bit
Dim l As Long     ' 32-bit
```

```csharp
// C#
short i;  // 16-bit (same as VB6 Integer)
int l;    // 32-bit (same as VB6 Long)
long ll;  // 64-bit
```

### 6. Boolean Values

**⚠️ VB6 True = -1, False = 0; C# True = 1 (or any non-zero), False = 0**

```vb
' VB6
Dim b As Boolean
b = True           ' b = -1
If b = -1 Then     ' True
```

```csharp
// C#
bool b;
b = true;          // b = true (displayed as "True")
if (b == true)     // Or just: if (b)
```

### 7. Optional Parameters

**⚠️ Both support optional parameters, but syntax differs**

```vb
' VB6
Sub DoSomething(x As Long, Optional y As Long = 10)
End Sub
```

```csharp
// C#
void DoSomething(int x, int y = 10)
{
}
```

### 8. Variant Type

**⚠️ VB6 Variant can hold anything; C# requires explicit types**

```vb
' VB6
Dim v As Variant
v = 5
v = "Hello"
v = obj
```

```csharp
// C# - use object or dynamic (prefer explicit types)
object v;
v = 5;
v = "Hello";
v = obj;

// Better: Use explicit types
int i = 5;
string s = "Hello";
MyClass obj = new MyClass();
```

### 9. Collection Access

**⚠️ VB6 Collection uses 1-based indexing and parentheses; C# uses 0-based and brackets**

```vb
' VB6
Dim coll As Collection
Set coll = New Collection
coll.Add "item1"
MsgBox coll(1)  ' First item (1-based)
```

```csharp
// C#
var list = new List<string>();
list.Add("item1");
Console.WriteLine(list[0]);  // First item (0-based)
```

### 10. For Loop Bounds

**⚠️ VB6 `To` is inclusive; C# `<` is exclusive**

```vb
' VB6
For i = 0 To 10    ' 0, 1, 2, ..., 10 (11 iterations)
Next i
```

```csharp
// C#
for (int i = 0; i < 10; i++)  // 0, 1, 2, ..., 9 (10 iterations)

// For inclusive (like VB6):
for (int i = 0; i <= 10; i++)  // 0, 1, 2, ..., 10 (11 iterations)
```

---

## Additional Resources

### Related Documentation

1. **Architecture Documentation**
   - `/home/user/PhotoDemon/docs/architecture/vb6-architecture.md` - VB6 codebase structure
   - `/home/user/PhotoDemon/docs/architecture/maui-architecture.md` - Target MAUI architecture

2. **Data Mapping**
   - `/home/user/PhotoDemon/docs/data/type-mapping.md` - Comprehensive type mappings

3. **Migration Strategy**
   - `/home/user/PhotoDemon/docs/migration/roadmap.md` - Overall migration roadmap
   - `/home/user/PhotoDemon/docs/migration/phase1-plan.md` - Phase 1 details
   - `/home/user/PhotoDemon/docs/migration/phase2-plan.md` - Phase 2 details

### Example Conversions

**Complete Class Conversion Example:**

See `/home/user/PhotoDemon/docs/architecture/maui-architecture.md` lines 505-684 for complete `pdImage` → `ImageDocument` conversion

**Service Pattern Example:**

See `/home/user/PhotoDemon/docs/architecture/maui-architecture.md` lines 993-1095 for `Processor.bas` → `EffectService` conversion

**Control Conversion Example:**

See `/home/user/PhotoDemon/docs/architecture/maui-architecture.md` lines 186-210 for `pdCanvas.ctl` → `ImageCanvasView.xaml` conversion

---

## Document Summary

This conversion guide provides:

1. **Syntax Translations**: Direct VB6 → C# syntax mappings with PhotoDemon examples
2. **Pattern Translations**: Common PhotoDemon patterns (forms, events, modules) → MAUI equivalents
3. **Error Handling**: VB6 error handling → C# exception patterns
4. **Resource Management**: GDI/GDI+ → SkiaSharp, manual memory → IDisposable
5. **Threading**: DoEvents → async/await, single-threaded → multi-threaded patterns
6. **Quick Reference**: Fast lookup tables for common conversions
7. **Pitfalls**: Common mistakes when migrating from VB6 to C#

**Key Takeaways:**

- ✅ Use async/await instead of DoEvents
- ✅ Use IDisposable for resource cleanup
- ✅ Use dependency injection instead of global modules
- ✅ Use MVVM pattern for UI separation
- ✅ Use SkiaSharp for graphics instead of GDI
- ✅ Check array indexing (0-based in C#)
- ✅ Check parameter passing (by-value default in C#)
- ✅ Use explicit string comparison modes
- ✅ Handle exceptions explicitly (avoid empty catch blocks)
- ✅ Use cancellation tokens for long operations

---

**Document Version:** 1.0
**Last Updated:** 2025-11-17
**Next Review:** With Phase 3 (File Formats) implementation
