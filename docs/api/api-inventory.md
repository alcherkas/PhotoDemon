# PhotoDemon Win32 API and External Dependencies Inventory

**Migration Project:** VB6 → .NET 10 MAUI
**Document Version:** 1.0
**Generated:** 2025-11-17
**Agent:** API Inventory Agent

---

## Executive Summary

PhotoDemon is a heavily Win32 API-dependent VB6 application with **1,493 Declare statements** across **135 files**. The application makes extensive use of:

- **GDI/GDI+ graphics APIs** (primary rendering pipeline)
- **External plugin DLLs** for image format support
- **Advanced Windows APIs** (memory management, file I/O, cryptography)
- **P/Invoke-heavy architecture** suitable for .NET interop

### Risk Assessment

| Risk Level | Category | Count | Notes |
|------------|----------|-------|-------|
| 🔴 **HIGH** | GDI/GDI+ Graphics | 200+ APIs | Core rendering pipeline; requires complete rewrite for MAUI |
| 🟡 **MEDIUM** | Plugin DLLs | 15+ DLLs | External dependencies; some have .NET wrappers available |
| 🟡 **MEDIUM** | Memory Management | 100+ APIs | VirtualAlloc, GlobalAlloc patterns; needs .NET Marshal equivalents |
| 🟢 **LOW** | File I/O | 80+ APIs | Well-documented .NET equivalents available |
| 🟢 **LOW** | String/Unicode | 50+ APIs | .NET handles natively |

---

## 1. Win32 API Declarations by Category

### 1.1 Graphics APIs (GDI/GDI+)

**Total Occurrences:** 200+ API calls
**Primary Files:** `/Modules/GDIPlus.bas`, `/Modules/GDI.bas`, `/Classes/pd2D*.cls`
**Migration Complexity:** 🔴 **HIGH** - Requires complete rewrite

#### GDI+ APIs (gdiplus.dll)

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `GdipCreateMatrix` | 50+ | Matrix transforms | `System.Drawing.Drawing2D.Matrix` | Direct mapping available |
| `GdipCloneMatrix` | 30+ | Matrix cloning | `Matrix.Clone()` | Direct mapping |
| `GdipCreateHatchBrush` | 20+ | Pattern brushes | `HatchBrush` class | Direct mapping |
| `GdipCreateSolidFill` | 40+ | Solid color brushes | `SolidBrush` class | Direct mapping |
| `GdipCreateTexture` | 15+ | Texture brushes | `TextureBrush` class | Direct mapping |
| `GdipDrawImageRectRectI` | 60+ | Image rendering | `Graphics.DrawImage()` | Direct mapping |
| `GdipCreateFromHDC` | 25+ | Graphics from DC | `Graphics.FromHdc()` | Available but legacy |
| `GdipCreateBitmapFromScan0` | 35+ | Bitmap creation | `Bitmap` constructor | Direct mapping |
| `GdipBitmapLockBits` | 40+ | Direct pixel access | `Bitmap.LockBits()` | Direct mapping |
| `GdipBitmapUnlockBits` | 40+ | Unlock pixel data | `Bitmap.UnlockBits()` | Direct mapping |

**File Locations:**
- `/home/user/PhotoDemon/Modules/GDIPlus.bas` - 112 GDI+ declares
- `/home/user/PhotoDemon/Classes/pd2DTransform.cls` - 11 matrix APIs
- `/home/user/PhotoDemon/Classes/pd2DBrush.cls` - 8 brush APIs
- `/home/user/PhotoDemon/Classes/pd2DSurface.cls` - 15 surface APIs
- `/home/user/PhotoDemon/Classes/pd2DPath.cls` - 38 path APIs
- `/home/user/PhotoDemon/Classes/pd2DPen.cls` - 14 pen APIs
- `/home/user/PhotoDemon/Classes/pd2DRegion.cls` - 20 region APIs
- `/home/user/PhotoDemon/Classes/pd2DGradient.cls` - 1 gradient API

#### Legacy GDI APIs (gdi32.dll)

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `GetDC` | 10+ | Get device context | `Graphics.FromHwnd()` | Use managed alternative |
| `ReleaseDC` | 10+ | Release DC | `Graphics.Dispose()` | Automatic in .NET |
| `BitBlt` | 15+ | Bitmap blitting | `Graphics.DrawImage()` | Use managed alternative |
| `StretchBlt` | 8+ | Scaled blitting | `Graphics.DrawImage()` | Use managed alternative |
| `CreateCompatibleDC` | 12+ | Memory DC creation | Use `Bitmap` directly | Not needed in .NET |
| `SelectObject` | 20+ | Select GDI object | Not needed | .NET manages automatically |
| `DeleteObject` | 20+ | Delete GDI object | `Dispose()` | Automatic garbage collection |
| `GetICMProfile` | 2+ | Color profile | `Image.ColorProfile` | Available in .NET |

**File Location:**
- `/home/user/PhotoDemon/Modules/GDI.bas` - 13 GDI declares

#### WIC (Windows Imaging Component)

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| Various WIC APIs | 13+ | Image codec support | `System.Drawing.Imaging` | Consider ImageSharp instead |

**File Location:**
- `/home/user/PhotoDemon/Modules/WIC.bas` - 13 WIC declares

---

### 1.2 File I/O and File System APIs

**Total Occurrences:** 80+ API calls
**Primary Files:** `/Classes/cZipArchive.cls`, `/Classes/pdFSO.cls`, `/Modules/Files.bas`
**Migration Complexity:** 🟢 **LOW** - Well-documented .NET equivalents

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `CreateFileW` | 10+ | File creation/opening | `FileStream` constructor | Direct mapping |
| `ReadFile` | 8+ | File reading | `FileStream.Read()` | Direct mapping |
| `WriteFile` | 8+ | File writing | `FileStream.Write()` | Direct mapping |
| `CloseHandle` | 15+ | Handle cleanup | `Stream.Dispose()` | Automatic with using |
| `SetFilePointerEx` | 5+ | File seeking | `FileStream.Seek()` | Direct mapping |
| `SetEndOfFile` | 3+ | Truncate file | `FileStream.SetLength()` | Direct mapping |
| `FindFirstFileW` | 5+ | Directory enumeration | `Directory.GetFiles()` | Direct mapping |
| `FindNextFileW` | 3+ | Next file in enum | Built into GetFiles() | Not needed |
| `FindClose` | 5+ | Close find handle | Not needed | Automatic |
| `GetFileAttributesW` | 3+ | File attributes | `File.GetAttributes()` | Direct mapping |
| `CreateDirectoryW` | 3+ | Directory creation | `Directory.CreateDirectory()` | Direct mapping |

**File Locations:**
- `/home/user/PhotoDemon/Classes/cZipArchive.cls` - 48 file I/O declares
- `/home/user/PhotoDemon/Classes/pdFSO.cls` - 33 file system declares
- `/home/user/PhotoDemon/Modules/Files.bas` - 7 file APIs

---

### 1.3 Memory Management APIs

**Total Occurrences:** 100+ API calls
**Primary Files:** `/Modules/VB_Hacks.bas`, `/Modules/OS.bas`, `/Classes/cZipArchive.cls`
**Migration Complexity:** 🟡 **MEDIUM** - Requires Marshal class and unsafe code

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `VirtualAlloc` | 15+ | Virtual memory allocation | `Marshal.AllocHGlobal()` | Use Marshal or stackalloc |
| `GlobalAlloc` | 5+ | Global memory allocation | `Marshal.AllocHGlobal()` | Use Marshal |
| `GlobalLock` | 3+ | Lock global memory | Not needed | .NET memory is pinned differently |
| `GlobalUnlock` | 3+ | Unlock global memory | Not needed | Not needed |
| `CoTaskMemFree` | 5+ | COM memory free | `Marshal.FreeCoTaskMem()` | Direct mapping |
| `CopyMemory` (RtlMoveMemory) | 100+ | Memory copy | `Marshal.Copy()` or `Buffer.BlockCopy()` | Multiple alternatives |
| `FillMemory` | 30+ | Memory fill | `Span<T>.Fill()` | Modern .NET approach |
| `ZeroMemory` | 30+ | Zero memory | `Span<T>.Clear()` | Modern .NET approach |
| `GetMem1/2/4/8` (msvbvm60) | 40+ | VB6 memory peek | `Marshal.ReadByte/Int16/Int32/Int64()` | Direct mapping |
| `PutMem1/2/4/8` (msvbvm60) | 20+ | VB6 memory poke | `Marshal.WriteByte/Int16/Int32/Int64()` | Direct mapping |

**File Locations:**
- `/home/user/PhotoDemon/Modules/VB_Hacks.bas` - 39 memory declares (PUBLIC)
- `/home/user/PhotoDemon/Modules/OS.bas` - 38 OS memory APIs
- `/home/user/PhotoDemon/Classes/cZipArchive.cls` - 13 memory APIs

---

### 1.4 Window Management and UI APIs

**Total Occurrences:** 150+ API calls
**Primary Files:** `/Classes/pdWindow*.cls`, `/Modules/UserControl_Support.bas`, `/Modules/Toolboxes.bas`
**Migration Complexity:** 🔴 **HIGH** - MAUI uses different UI framework

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `SendMessage` | 50+ | Window messaging | P/Invoke or control events | MAUI uses different model |
| `PostMessage` | 10+ | Async messaging | Events/Tasks | Use async/await pattern |
| `GetWindowLong` | 20+ | Window data retrieval | Control properties | MAUI handles differently |
| `SetWindowLong` | 20+ | Window data setting | Control properties | MAUI handles differently |
| `SetWindowPos` | 30+ | Window positioning | Control.Bounds | Direct mapping |
| `ShowWindow` | 15+ | Window visibility | Control.Visible | Direct mapping |
| `GetClientRect` | 10+ | Client rectangle | Control.ClientSize | Direct mapping |
| `GetWindowRect` | 10+ | Window rectangle | Control.Bounds | Direct mapping |
| `SetParent` | 5+ | Parent window change | Control.Parent | Direct mapping |
| `InvalidateRect` | 8+ | Force redraw | Control.Invalidate() | Direct mapping |

**File Locations:**
- `/home/user/PhotoDemon/Classes/pdWindowManager.cls` - 21 window APIs
- `/home/user/PhotoDemon/Classes/pdWindowSync.cls` - 5 window APIs
- `/home/user/PhotoDemon/Classes/pdWindowPainter.cls` - 6 window APIs
- `/home/user/PhotoDemon/Classes/pdWindowSize.cls` - 4 window APIs
- `/home/user/PhotoDemon/Modules/UserControl_Support.bas` - 10 control APIs
- `/home/user/PhotoDemon/Modules/Toolboxes.bas` - 5 window APIs

---

### 1.5 User Input APIs

**Total Occurrences:** 50+ API calls
**Primary Files:** `/Classes/pdInputMouse.cls`, `/Classes/pdInputKeyboard.cls`, `/Controls/pdAccelerator.ctl`
**Migration Complexity:** 🟡 **MEDIUM** - MAUI has input events but may need P/Invoke for advanced features

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `CallNextHookEx` | 5+ | Keyboard hooks | Not recommended | Use MAUI input events |
| `UnhookWindowsHookEx` | 5+ | Unhook keyboard | Not recommended | Use MAUI input events |
| Various mouse APIs | 20+ | Mouse tracking | MAUI pointer events | Use MAUI events |

**File Locations:**
- `/home/user/PhotoDemon/Classes/pdInputMouse.cls` - 14 mouse APIs
- `/home/user/PhotoDemon/Classes/pdInputKeyboard.cls` - 2 keyboard APIs
- `/home/user/PhotoDemon/Controls/pdAccelerator.ctl` - 2 hook APIs

---

### 1.6 Cryptography and Security APIs

**Total Occurrences:** 40+ API calls
**Primary Files:** `/Classes/pdCrypto.cls`, `/Classes/cZipArchive.cls`
**Migration Complexity:** 🟢 **LOW** - .NET has excellent crypto support

#### Legacy Crypto APIs (advapi32.dll)

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `CryptAcquireContextW` | 2+ | Crypto context | Not needed | Use System.Security.Cryptography |
| `CryptCreateHash` | 2+ | Hash creation | `HashAlgorithm.Create()` | Direct mapping |
| `CryptHashData` | 2+ | Hash data | `HashAlgorithm.ComputeHash()` | Direct mapping |
| `CryptGetHashParam` | 2+ | Get hash value | Return from ComputeHash() | Automatic |
| `CryptDestroyHash` | 2+ | Destroy hash | `Dispose()` | Automatic |
| `CryptReleaseContext` | 2+ | Release context | Not needed | Automatic |
| `RtlGenRandom` | 3+ | Random generation | `RandomNumberGenerator` | Direct mapping |

#### Modern Crypto APIs (bcrypt.dll)

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `BCryptOpenAlgorithmProvider` | 5+ | Algorithm provider | Algorithm classes | Use System.Security.Cryptography |
| `BCryptCloseAlgorithmProvider` | 3+ | Close provider | `Dispose()` | Automatic |
| `BCryptGenerateSymmetricKey` | 3+ | Key generation | `SymmetricAlgorithm.GenerateKey()` | Direct mapping |
| `BCryptEncrypt` | 3+ | Encryption | `ICryptoTransform.TransformFinalBlock()` | Direct mapping |
| `BCryptDeriveKeyPBKDF2` | 2+ | Key derivation | `Rfc2898DeriveBytes` | Direct mapping |
| `BCryptCreateHash` | 2+ | Hash creation | `HashAlgorithm.Create()` | Direct mapping |

#### Certificate/String APIs (crypt32.dll)

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `CryptBinaryToString` | 3+ | Base64 encoding | `Convert.ToBase64String()` | Direct mapping |
| `CryptStringToBinary` | 3+ | Base64 decoding | `Convert.FromBase64String()` | Direct mapping |

**File Locations:**
- `/home/user/PhotoDemon/Classes/pdCrypto.cls` - 6 crypto declares
- `/home/user/PhotoDemon/Classes/cZipArchive.cls` - 14 BCrypt APIs

---

### 1.7 String and Unicode APIs

**Total Occurrences:** 60+ API calls
**Primary Files:** `/Classes/pdString.cls`, `/Classes/cZipArchive.cls`
**Migration Complexity:** 🟢 **LOW** - .NET handles Unicode natively

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `MultiByteToWideChar` | 10+ | ANSI to Unicode | `Encoding.Unicode.GetString()` | Direct mapping |
| `WideCharToMultiByte` | 10+ | Unicode to ANSI | `Encoding.GetEncoding().GetBytes()` | Direct mapping |
| `lstrlenW` | 5+ | String length | `String.Length` | Native property |

**File Locations:**
- `/home/user/PhotoDemon/Classes/pdString.cls` - 2 string APIs
- `/home/user/PhotoDemon/Classes/cZipArchive.cls` - 2 string APIs

---

### 1.8 Operating System and Process APIs

**Total Occurrences:** 60+ API calls
**Primary Files:** `/Modules/OS.bas`
**Migration Complexity:** 🟡 **MEDIUM** - Most have .NET equivalents

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `GetCurrentProcessId` | 3+ | Process ID | `Process.GetCurrentProcess().Id` | Direct mapping |
| `GetEnvironmentVariableW` | 5+ | Environment vars | `Environment.GetEnvironmentVariable()` | Direct mapping |
| `SetEnvironmentVariableW` | 3+ | Set environment | `Environment.SetEnvironmentVariable()` | Direct mapping |
| `GetStdHandle` | 3+ | Console handles | `Console` class | Use Console class |
| `Sleep` | 5+ | Thread sleep | `Thread.Sleep()` or `Task.Delay()` | Direct mapping |
| `IsProcessorFeaturePresent` | 10+ | CPU features | `RuntimeInformation` | Use RuntimeInformation |

**File Location:**
- `/home/user/PhotoDemon/Modules/OS.bas` - 38 OS declares

---

### 1.9 COM/ActiveX Interop APIs

**Total Occurrences:** 30+ API calls
**Primary Files:** `/Classes/pdPDF.cls`, `/Modules/Plugin_*.bas`
**Migration Complexity:** 🟡 **MEDIUM** - .NET has COM interop support

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `DispCallFunc` (oleaut32) | 20+ | Late-bound COM calls | Dynamic COM objects | Use COM interop or refactor |
| `CoTaskMemFree` | 5+ | COM memory free | `Marshal.FreeCoTaskMem()` | Direct mapping |

**File Locations:**
- `/home/user/PhotoDemon/Classes/pdPDF.cls` - 1 COM API
- `/home/user/PhotoDemon/Modules/Plugin_lz4.bas` - 2 COM APIs
- `/home/user/PhotoDemon/Modules/Plugin_heif.bas` - 2 COM APIs

---

### 1.10 Color Management APIs

**Total Occurrences:** 10+ API calls
**Primary Files:** `/Modules/ColorManagement_ICM.bas`
**Migration Complexity:** 🟡 **MEDIUM** - Limited .NET support

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `GetColorDirectory` (mscms) | 2+ | Get color profile dir | P/Invoke required | Limited .NET support |
| `MonitorFromWindow` | 2+ | Monitor identification | `Screen.FromHandle()` | Partial mapping |
| `GetICMProfile` | 2+ | Get ICC profile | P/Invoke required | Limited .NET support |

**File Location:**
- `/home/user/PhotoDemon/Modules/ColorManagement_ICM.bas` - 4 ICM declares

---

### 1.11 Miscellaneous System APIs

**Total Occurrences:** 80+ API calls
**Primary Files:** Various
**Migration Complexity:** 🟡 **MEDIUM** - Mixed availability in .NET

| API Function | Usage Count | Purpose | .NET Equivalent | Migration Notes |
|--------------|-------------|---------|-----------------|-----------------|
| `GetModuleHandle` | 10+ | Get module handle | `Process.GetCurrentProcess().MainModule` | Partial mapping |
| `GetProcAddress` | 30+ | Get function pointer | `Marshal.GetDelegateForFunctionPointer()` | Available |
| `LoadLibrary` / `LoadLibraryW` | 10+ | Load DLL | Not needed / P/Invoke | Use DllImport attribute |
| `FreeLibrary` | 5+ | Unload DLL | Not needed | Automatic |
| `CallWindowProc` | 5+ | Call window proc | Not needed | Use MAUI framework |
| `PathMatchSpecW` (shlwapi) | 2+ | Pattern matching | Custom or use regex | No direct equivalent |

---

## 2. External Plugin DLLs

**Location:** `/home/user/PhotoDemon/App/PhotoDemon/Plugins/`
**Migration Complexity:** 🟡 **MEDIUM** - Some have .NET wrappers, others need P/Invoke

### 2.1 Image Format Libraries

| DLL Name | Purpose | Version/Notes | .NET Alternative | Migration Strategy |
|----------|---------|---------------|------------------|-------------------|
| **FreeImage.dll** | Multi-format image library | Open source | ImageSharp / SkiaSharp | Replace with ImageSharp |
| **libheif.dll** | HEIF/HEIC support | HEIF codec | - | P/Invoke wrapper needed |
| **libwebp.dll** | WebP support | Google WebP | System.Drawing.Common | Use SkiaSharp or ImageSharp |
| **libwebpdemux.dll** | WebP demuxing | Google WebP | - | P/Invoke wrapper needed |
| **libwebpmux.dll** | WebP muxing | Google WebP | - | P/Invoke wrapper needed |
| **libsharpyuv.dll** | WebP color conversion | Google WebP | - | Part of WebP suite |
| **openjp2.dll** | JPEG 2000 support | OpenJPEG | - | P/Invoke wrapper needed |
| **charls-2.dll** | JPEG-LS support | CharLS v2 | - | P/Invoke wrapper needed |
| **pdfium.dll** | PDF rendering | Google PDFium | PDFium.NET NuGet | Use PDFium.NET |
| **resvg.dll** | SVG rendering | Rust-based SVG | Svg.Skia | Use SkiaSharp + Svg.Skia |

**Usage Pattern:** All libraries called via P/Invoke through wrapper modules in `/Modules/Plugin_*.bas`

**Files with Plugin Declares:**
- `/home/user/PhotoDemon/Modules/FreeImageWrapper.bas` - 73 FreeImage APIs
- `/home/user/PhotoDemon/Modules/Plugin_heif.bas` - 19 libheif APIs
- `/home/user/PhotoDemon/Modules/Plugin_WebP.bas` - Multiple WebP APIs
- `/home/user/PhotoDemon/Modules/Plugin_OpenJPEG.bas` - 32 OpenJPEG APIs
- `/home/user/PhotoDemon/Modules/Plugin_CharLS.bas` - 11 CharLS APIs
- `/home/user/PhotoDemon/Modules/Plugin_PDF.bas` - 2 PDFium APIs
- `/home/user/PhotoDemon/Modules/Plugin_resvg.bas` - 33 resvg APIs

### 2.2 Compression Libraries

| DLL Name | Purpose | Version/Notes | .NET Alternative | Migration Strategy |
|----------|---------|---------------|------------------|-------------------|
| **liblz4.dll** | LZ4 compression | Fast compression | K4os.Compression.LZ4 NuGet | Use NuGet package |
| **libzstd.dll** | Zstandard compression | Facebook Zstd | ZstdSharp NuGet | Use NuGet package |
| **libdeflate.dll** | Deflate compression | Fast deflate | System.IO.Compression | Use built-in or P/Invoke |

**Usage Pattern:** Called through wrapper modules with GetProcAddress pattern

**Files with Compression Declares:**
- `/home/user/PhotoDemon/Modules/Plugin_lz4.bas` - 2 lz4 APIs
- `/home/user/PhotoDemon/Modules/Plugin_zstd.bas` - 2 zstd APIs
- `/home/user/PhotoDemon/Modules/Plugin_libdeflate.bas` - 2 deflate APIs

### 2.3 Color Management Libraries

| DLL Name | Purpose | Version/Notes | .NET Alternative | Migration Strategy |
|----------|---------|---------------|------------------|-------------------|
| **lcms2.dll** | Little CMS 2 | ICC color management | - | P/Invoke wrapper needed or use System.Drawing |

**Usage Pattern:** Direct P/Invoke through wrapper module

**Files with Color Management Declares:**
- `/home/user/PhotoDemon/Modules/Plugin_LittleCMS.bas` - 27 lcms2 APIs

### 2.4 Special Purpose Libraries

| DLL Name | Purpose | Version/Notes | .NET Alternative | Migration Strategy |
|----------|---------|---------------|------------------|-------------------|
| **PDHelper_win32.dll** | Custom helper DLL | PhotoDemon-specific | - | Rewrite in C# or keep P/Invoke |
| **EZTW32.dll** | TWAIN scanner support | EZTwain | WIA or TWAIN.NET | Consider WIA library |
| **pspiHost.dll** | Photoshop plugin host | 8bf filter support | - | Complex; may need P/Invoke |
| **texconv.exe** | DirectX texture tool | Microsoft DirectXTex | - | Process.Start() or library |
| **texdiag.exe** | DirectX texture diagnostic | Microsoft DirectXTex | - | Process.Start() or library |
| **exiftool.exe** | EXIF metadata | Perl-based | ExifTool.NET | Use .NET wrapper or MetadataExtractor |

**Files with Special Purpose Declares:**
- `/home/user/PhotoDemon/Modules/Plugin_heif.bas` - 19 PDHelper_win32 APIs
- `/home/user/PhotoDemon/Modules/Plugin_EZTwain.bas` - 4 TWAIN APIs
- `/home/user/PhotoDemon/Modules/Plugin_8bf.bas` - 11 Photoshop plugin APIs
- `/home/user/PhotoDemon/Modules/Plugin_ExifTool.bas` - 11 ExifTool APIs

### 2.5 Additional Dependency Files

| File | Type | Purpose |
|------|------|---------|
| **libx265.dll** | Library | H.265/HEVC encoding support |
| **libde265.dll** | Library | H.265/HEVC decoding support |

---

## 3. P/Invoke Migration Strategy

### 3.1 High-Priority APIs for Migration

These APIs are used most frequently and should be migrated first:

1. **CopyMemory/RtlMoveMemory** (100+ uses) → `Marshal.Copy()` or `Buffer.BlockCopy()`
2. **GDI+ Graphics APIs** (200+ uses) → `System.Drawing` (Windows) or `SkiaSharp` (cross-platform)
3. **GetProcAddress** (30+ uses) → `Marshal.GetDelegateForFunctionPointer()`
4. **SendMessage** (50+ uses) → MAUI events or P/Invoke
5. **VirtualAlloc** (15+ uses) → `Marshal.AllocHGlobal()` or stackalloc

### 3.2 Recommended Migration Path by Category

#### ✅ Easy (Low Complexity)

- **File I/O APIs** → Use `System.IO` namespace
- **String/Unicode APIs** → Use .NET string handling
- **Cryptography APIs** → Use `System.Security.Cryptography`
- **Compression APIs** → Use NuGet packages (LZ4, Zstd)

#### ⚠️ Medium (Moderate Complexity)

- **Memory Management** → Use `System.Runtime.InteropServices.Marshal`
- **Operating System APIs** → Use `System.Diagnostics` and `System.Environment`
- **COM Interop** → Use .NET COM interop or refactor
- **Color Management** → Use ImageSharp or P/Invoke lcms2

#### 🚫 Hard (High Complexity)

- **GDI/GDI+ Graphics** → Complete rewrite using SkiaSharp for MAUI
- **Window Management** → Complete rewrite using MAUI framework
- **Custom Controls** → Rewrite as MAUI custom controls
- **Subclassing** → Not applicable in MAUI; use events

---

## 4. Architecture Recommendations for .NET MAUI

### 4.1 Graphics Engine Replacement

**Current:** GDI+ via P/Invoke (200+ API calls)
**Recommended:** **SkiaSharp** for MAUI cross-platform graphics

```csharp
// Instead of GDI+ P/Invoke:
// GdipCreateMatrix(), GdipDrawImage(), etc.

// Use SkiaSharp:
using SkiaSharp;
using SkiaSharp.Views.Maui;

public class ImageRenderer
{
    public void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var paint = new SKPaint();
        // Modern cross-platform rendering
    }
}
```

**Rationale:**
- SkiaSharp is cross-platform (Windows, macOS, iOS, Android, Linux)
- Used by Flutter, Xamarin, and MAUI
- Hardware-accelerated
- Similar API surface to GDI+

### 4.2 Plugin DLL Management

**Current:** Direct P/Invoke with LoadLibrary/GetProcAddress
**Recommended:** Use NuGet packages where available, P/Invoke wrappers for others

**Priority 1: Replace with NuGet**
- FreeImage → **ImageSharp** (cross-platform, pure .NET)
- lcms2 → **ImageSharp** (has color management)
- lz4 → **K4os.Compression.LZ4**
- zstd → **ZstdSharp**
- resvg → **Svg.Skia**

**Priority 2: P/Invoke Wrappers**
- libheif → Create C# wrapper
- libwebp → Use **SkiaSharp.Extended.WebP**
- openjp2 → Create C# wrapper or use ImageSharp
- pdfium → **PDFium.NET** NuGet

**Priority 3: Process Execution**
- exiftool.exe → **MetadataExtractor** NuGet (pure .NET)
- texconv.exe → Keep as Process.Start() or find .NET library

### 4.3 Memory Management Strategy

**Current:** Manual memory management with VirtualAlloc, GlobalAlloc
**Recommended:** Use .NET memory management with Marshal for interop

```csharp
// Instead of VirtualAlloc:
IntPtr ptr = Marshal.AllocHGlobal(size);
try
{
    // Use memory
}
finally
{
    Marshal.FreeHGlobal(ptr);
}

// Or use modern Span<T> for stack allocation:
Span<byte> buffer = stackalloc byte[256];
```

### 4.4 Window and UI Management

**Current:** Win32 window APIs (SendMessage, SetWindowPos, etc.)
**Recommended:** MAUI framework and events

```csharp
// Instead of SendMessage() for control communication:
// Use MAUI events, binding, and MVVM pattern

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnButtonClicked()
    {
        // MAUI event handling
    }
}
```

---

## 5. Risk Assessment and Mitigation

### 5.1 Critical Risks

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|------------|
| GDI+ graphics pipeline replacement | 🔴 HIGH | CERTAIN | Phase 1: Port to SkiaSharp incrementally |
| Performance degradation from managed code | 🟡 MEDIUM | POSSIBLE | Use unsafe code and Span<T> for hot paths |
| Plugin DLL compatibility on non-Windows | 🔴 HIGH | CERTAIN | Use pure .NET alternatives or conditional compilation |
| Loss of VB6 COM interop functionality | 🟡 MEDIUM | LIKELY | Refactor to eliminate COM dependencies |

### 5.2 Testing Strategy

1. **Unit Test Critical APIs**
   - Memory operations (CopyMemory → Buffer.BlockCopy)
   - Cryptography (BCrypt → System.Security.Cryptography)
   - File I/O (CreateFile → FileStream)

2. **Integration Test Plugin DLLs**
   - Ensure P/Invoke wrappers work correctly
   - Test on multiple platforms (Windows, macOS, Linux)

3. **Visual Regression Testing**
   - Compare GDI+ output vs SkiaSharp output
   - Pixel-perfect comparison for rendering

4. **Performance Benchmarking**
   - Compare VB6 vs .NET performance
   - Identify and optimize hot paths

---

## 6. Migration Recommendations by Priority

### Phase 1: Foundation (Weeks 1-4)
- ✅ Migrate file I/O APIs to System.IO
- ✅ Migrate string/Unicode handling to .NET strings
- ✅ Migrate cryptography to System.Security.Cryptography
- ✅ Replace compression DLLs with NuGet packages

### Phase 2: Core Graphics (Weeks 5-12)
- 🔄 Port GDI+ rendering pipeline to SkiaSharp
- 🔄 Reimplement pd2D* classes using SkiaSharp
- 🔄 Create SkiaSharp-based image editing primitives
- 🔄 Test rendering accuracy and performance

### Phase 3: Plugin Integration (Weeks 13-16)
- 🔄 Replace FreeImage with ImageSharp
- 🔄 Create P/Invoke wrappers for libheif, openjp2
- 🔄 Test image format loading/saving
- 🔄 Implement cross-platform plugin loading

### Phase 4: UI and Controls (Weeks 17-24)
- 🔄 Rewrite VB6 custom controls as MAUI controls
- 🔄 Implement MVVM pattern for UI
- 🔄 Migrate window management to MAUI
- 🔄 Implement MAUI-native dialogs

### Phase 5: Testing and Optimization (Weeks 25-28)
- 🔄 Comprehensive testing on all platforms
- 🔄 Performance optimization
- 🔄 Memory profiling and leak detection
- 🔄 Final bug fixes and polish

---

## 7. Summary Statistics

### API Usage Breakdown

| Category | Declare Count | Migration Complexity | Recommended Approach |
|----------|---------------|---------------------|----------------------|
| Graphics (GDI/GDI+) | 200+ | 🔴 HIGH | SkiaSharp complete rewrite |
| Memory Management | 100+ | 🟡 MEDIUM | Marshal + Span<T> |
| File I/O | 80+ | 🟢 LOW | System.IO namespace |
| Window Management | 150+ | 🔴 HIGH | MAUI framework |
| Cryptography | 40+ | 🟢 LOW | System.Security.Cryptography |
| String/Unicode | 60+ | 🟢 LOW | Native .NET strings |
| OS/Process APIs | 60+ | 🟡 MEDIUM | System.Diagnostics |
| COM Interop | 30+ | 🟡 MEDIUM | Refactor or COM interop |
| **TOTAL** | **1,493** | **Mixed** | **See migration plan** |

### DLL Dependencies

| Type | Count | Status |
|------|-------|--------|
| Image Format Libraries | 10 | Most have .NET alternatives |
| Compression Libraries | 3 | NuGet packages available |
| Color Management | 1 | ImageSharp or P/Invoke |
| Special Purpose | 6 | Mixed (rewrite, P/Invoke, or Process.Start) |
| **TOTAL** | **20** | **Good path forward** |

### High-Risk Items Requiring Special Attention

1. **GDI/GDI+ Complete Replacement** (200+ APIs)
   - Rewrite entire rendering pipeline using SkiaSharp
   - Extensive testing required

2. **Custom VB6 Controls** (30+ controls)
   - Rewrite as MAUI custom controls
   - XAML-based UI design

3. **Photoshop Plugin Support** (pspiHost.dll)
   - Complex binary format
   - May require extensive P/Invoke or removal

4. **Platform-Specific Plugin DLLs**
   - Many DLLs are Windows-only (x86/x64)
   - Need cross-platform alternatives for true MAUI support

---

## 8. Conclusion

PhotoDemon's VB6 codebase makes extensive use of Win32 APIs (1,493 Declare statements) but follows patterns that are well-suited for .NET migration:

### Strengths
- ✅ Clear API organization (categorized in modules)
- ✅ Heavy use of external DLLs (easy to replace)
- ✅ Good separation of concerns (graphics, I/O, crypto)
- ✅ Most Win32 APIs have direct .NET equivalents

### Challenges
- ⚠️ GDI+ graphics pipeline requires complete rewrite
- ⚠️ Custom VB6 controls need MAUI equivalents
- ⚠️ Windows-specific DLLs limit cross-platform support
- ⚠️ Memory management patterns need careful porting

### Recommendation
The migration is **FEASIBLE** but **HIGH-EFFORT**, estimated at 6-7 months for a complete port to .NET 10 MAUI with SkiaSharp-based graphics engine. The modular architecture and clear API boundaries make this a good candidate for incremental migration.

---

**Document prepared by:** API Inventory Agent
**For:** PhotoDemon VB6 → .NET 10 MAUI Migration
**Last updated:** 2025-11-17
