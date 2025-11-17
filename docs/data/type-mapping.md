# PhotoDemon VB6 to .NET 10 MAUI Type Mapping

## Document Overview

**Purpose**: Comprehensive mapping of VB6 data types and structures to C# equivalents for PhotoDemon migration
**Last Updated**: 2025-11-17
**Scope**: All Type declarations, Class structures, and primitive type usage
**Status**: Analysis Complete

---

## Executive Summary

### Statistics
- **Type Declarations in .bas files**: ~176 types across 52 files
- **Type Declarations in .frm files**: ~19 types across 16 files
- **Class Files**: 163+ classes
- **API Declarations**: 813+ Windows API calls
- **Total Type References**: 9500+ occurrences

### Critical Migration Concerns

1. **32-bit to 64-bit Pointer Migration**
   - VB6 uses `Long` (32-bit) for all pointers
   - .NET requires `IntPtr` (platform-dependent)
   - **Impact**: High - affects 800+ API declarations and memory manipulation

2. **SafeArray Direct Memory Access**
   - VB6 uses undocumented `VarPtr`/`VarPtrArray` for bitmap manipulation
   - .NET requires `unsafe` code or `Span<T>`/`Memory<T>`
   - **Impact**: High - core image processing functionality

3. **Structure Packing/Alignment**
   - VB6 has implicit structure packing rules
   - C# requires explicit `[StructLayout]` attributes
   - **Impact**: High - affects file I/O and API interop

4. **Currency Type as 64-bit Hack**
   - VB6 uses `Currency` to represent 64-bit integers
   - C# should use `long` or `ulong`
   - **Impact**: Medium - affects OS memory reporting

---

## Part 1: VB6 to C# Primitive Type Mapping

### Standard Type Conversions

| VB6 Type | Size | Signed | C# Type | .NET Type | Notes |
|----------|------|--------|---------|-----------|-------|
| `Byte` | 1 byte | Unsigned | `byte` | `System.Byte` | Direct equivalent |
| `Boolean` | 2 bytes | N/A | `bool` (use `short` for interop) | `System.Boolean` | **WARNING**: VB6 Boolean is 2 bytes, C# is 1 byte. Use `[MarshalAs(UnmanagedType.VariantBool)]` for interop |
| `Integer` | 2 bytes | Signed | `short` | `System.Int16` | **WARNING**: NOT `int` in C# |
| `Long` | 4 bytes | Signed | `int` | `System.Int32` | **WARNING**: C# `long` is 64-bit! |
| `Single` | 4 bytes | Signed | `float` | `System.Single` | Direct equivalent |
| `Double` | 8 bytes | Signed | `double` | `System.Double` | Direct equivalent |
| `Currency` | 8 bytes | Fixed-point | `decimal` or `long` | Context-dependent | VB6 uses this for 64-bit integers in some cases |
| `String` | Variable | N/A | `string` | `System.String` | VB6 uses BSTR (COM strings), C# uses managed strings |

### Pointer and Handle Types

| VB6 Usage | VB6 Type | C# Type | .NET Attributes | Notes |
|-----------|----------|---------|-----------------|-------|
| Pointer to memory | `Long` | `IntPtr` | N/A | **CRITICAL**: All pointer usage must migrate |
| Handle (HWND, HDC, etc.) | `Long` | `IntPtr` | N/A | Windows handles |
| Pointer in structure | `Long` | `IntPtr` | `[MarshalAs(UnmanagedType.SysInt)]` | For P/Invoke structures |
| Function pointer | `Long` | `delegate` or `IntPtr` | N/A | Callback functions |

### Array Types

| VB6 Array | VB6 Declaration | C# Equivalent | Notes |
|-----------|-----------------|---------------|-------|
| Fixed array | `Dim arr(0 To 9) As Long` | `int[]` or `int[10]` | C# uses 0-based indexing by default |
| Dynamic array | `ReDim arr(n) As Long` | `List<int>` or `int[]` + `Array.Resize` | Prefer `List<T>` for dynamic sizing |
| Array in Type | `Data(0 To 255) As Byte` | `[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] byte[]` | For fixed-size arrays in structures |
| Multidimensional | `Dim arr(0 To 9, 0 To 9)` | `int[,]` or `int[][]` | Jagged arrays may perform better |

---

## Part 2: Structure (Type) Mappings

### 2.1 Core Graphics Structures

#### RGBQuad (Pixel Data)
**File**: Modules/Drawing2D.bas:312

**VB6 Definition**:
```vb6
Public Type RGBQuad
    Blue As Byte
    Green As Byte
    Red As Byte
    Alpha As Byte
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RGBQuad
{
    public byte Blue;
    public byte Green;
    public byte Red;
    public byte Alpha;
}
```

**Migration Notes**:
- Used extensively for pixel-by-pixel operations
- Memory layout is critical (BGRA order matches Windows DIBs)
- Pack = 1 ensures no padding
- Consider `readonly struct` for performance
- Alternative: Use `System.Drawing.Color` or custom `Rgba32` from ImageSharp

---

#### SafeArray2D (Direct Memory Access)
**File**: Modules/Drawing2D.bas:362

**VB6 Definition**:
```vb6
Public Type SafeArray2D
    cDims      As Integer
    fFeatures  As Integer
    cbElements As Long
    cLocks     As Long
    pvData     As Long
    Bounds(1)  As SafeArrayBound
End Type

Public Type SafeArrayBound
    cElements As Long
    lBound    As Long
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct SAFEARRAY2D
{
    public short cDims;
    public short fFeatures;
    public int cbElements;
    public int cLocks;
    public IntPtr pvData;           // *** 32→64-bit concern
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public SAFEARRAYBOUND[] Bounds;
}

[StructLayout(LayoutKind.Sequential)]
public struct SAFEARRAYBOUND
{
    public int cElements;
    public int lBound;
}
```

**Migration Notes**:
- **CRITICAL**: VB6 uses this to manipulate array pointers directly
- PhotoDemon uses `VarPtrArray` to overlay SafeArray on bitmap data
- .NET equivalent: Use `Span<T>`, `Memory<T>`, or `unsafe` pointers
- **Recommended approach**: Migrate to `Span<byte>` for bitmap data
- See: Classes/pdDIB.cls for extensive usage
- **64-bit concern**: `pvData` must be `IntPtr`, not `int`

---

#### GP_BitmapData (GDI+ Bitmap Lock)
**File**: Modules/GDIPlus.bas:772

**VB6 Definition**:
```vb6
Public Type GP_BitmapData
    BD_Width As Long
    BD_Height As Long
    BD_Stride As Long
    BD_PixelFormat As GP_PixelFormat
    BD_Scan0 As Long
    BD_Reserved As Long
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BitmapData
{
    public int Width;
    public int Height;
    public int Stride;
    public PixelFormat PixelFormat;
    public IntPtr Scan0;            // *** 32→64-bit concern
    public IntPtr Reserved;         // *** 32→64-bit concern
}
```

**Migration Notes**:
- Maps to `System.Drawing.Imaging.BitmapData`
- `Scan0` is pointer to pixel data - MUST be `IntPtr`
- Consider using `SkiaSharp.SKBitmap` or `ImageSharp` instead of GDI+

---

#### BITMAPINFOHEADER (Windows DIB)
**File**: Modules/GDI.bas:33

**VB6 Definition**:
```vb6
Private Type GDI_BitmapInfoHeader
    biSize As Long
    biWidth As Long
    biHeight As Long
    biPlanes As Integer
    biBitCount As Integer
    biCompression As Long
    biSizeImage As Long
    biXPelsPerMeter As Long
    biYPelsPerMeter As Long
    biClrUsed As Long
    biClrImportant As Long
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct BITMAPINFOHEADER
{
    public uint biSize;
    public int biWidth;
    public int biHeight;
    public ushort biPlanes;
    public ushort biBitCount;
    public uint biCompression;
    public uint biSizeImage;
    public int biXPelsPerMeter;
    public int biYPelsPerMeter;
    public uint biClrUsed;
    public uint biClrImportant;
}
```

**Migration Notes**:
- Used for BMP file I/O and DIB creation
- Memory layout MUST match Windows specification exactly
- **Serialization concern**: Used in file format reading/writing
- Size must be 40 bytes (verify with `sizeof()`)

---

### 2.2 Geometry Structures

#### PointFloat, PointLong, RectF, RectL
**File**: Modules/Drawing2D.bas:319-349

**VB6 Definitions**:
```vb6
Public Type PointFloat
    x As Single
    y As Single
End Type

Public Type PointLong
    x As Long
    y As Long
End Type

Public Type RectF
    Left As Single
    Top As Single
    Width As Single
    Height As Single
End Type

Public Type RectL
    Left As Long
    Top As Long
    Right As Long
    Bottom As Long
End Type
```

**C# Equivalents**:
```csharp
// Use built-in .NET types where possible
using PointF = System.Drawing.PointF;
using Point = System.Drawing.Point;
using RectangleF = System.Drawing.RectangleF;
using Rectangle = System.Drawing.Rectangle;

// Or for MAUI:
using PointF = Microsoft.Maui.Graphics.PointF;
using Point = Microsoft.Maui.Graphics.Point;
using RectangleF = Microsoft.Maui.Graphics.RectangleF;
using Rectangle = Microsoft.Maui.Graphics.Rect;

// Or define custom if needed:
[StructLayout(LayoutKind.Sequential)]
public struct PointF
{
    public float X;
    public float Y;
}
```

**Migration Notes**:
- **Prefer built-in .NET types** where possible
- Watch for coordinate system differences (top-down vs bottom-up)
- MAUI uses `Rect` (not `Rectangle`)
- Consider `System.Numerics.Vector2` for calculations

---

### 2.3 Font Structures

#### LOGFONTW (Windows Font Definition)
**File**: Modules/Fonts.bas:32

**VB6 Definition**:
```vb6
Public Type LOGFONTW
    lfHeight As Long
    lfWidth As Long
    lfEscapement As Long
    lfOrientation As Long
    lfWeight As Long
    lfItalic As Byte
    lfUnderline As Byte
    lfStrikeOut As Byte
    lfCharSet As Byte
    lfOutPrecision As Byte
    lfClipPrecision As Byte
    lfQuality As Byte
    lfPitchAndFamily As Byte
    lfFaceName(0 To LF_FACESIZEW - 1) As Byte
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct LOGFONTW
{
    public int lfHeight;
    public int lfWidth;
    public int lfEscapement;
    public int lfOrientation;
    public int lfWeight;
    public byte lfItalic;
    public byte lfUnderline;
    public byte lfStrikeOut;
    public byte lfCharSet;
    public byte lfOutPrecision;
    public byte lfClipPrecision;
    public byte lfQuality;
    public byte lfPitchAndFamily;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string lfFaceName;
}
```

**Migration Notes**:
- Used for Windows GDI font creation
- Face name is 32 wide characters (64 bytes)
- **MAUI alternative**: Use `Microsoft.Maui.Graphics.Font`
- Consider migrating away from GDI fonts entirely

---

#### TEXTMETRIC (Font Metrics)
**File**: Modules/Fonts.bas:90

**VB6 Definition**:
```vb6
Public Type TEXTMETRIC
    tmHeight As Long
    tmAscent As Long
    tmDescent As Long
    tmInternalLeading As Long
    tmExternalLeading As Long
    tmAveCharWidth As Long
    tmMaxCharWidth As Long
    tmWeight As Long
    tmOverhang As Long
    tmDigitizedAspectX As Long
    tmDigitizedAspectY As Long
    tmFirstChar As Integer
    tmLastChar As Integer
    tmDefaultChar As Integer
    tmBreakChar As Integer
    tmItalic As Byte
    tmUnderlined As Byte
    tmStruckOut As Byte
    tmPitchAndFamily As Byte
    tmCharSet As Byte
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct TEXTMETRIC
{
    public int tmHeight;
    public int tmAscent;
    public int tmDescent;
    public int tmInternalLeading;
    public int tmExternalLeading;
    public int tmAveCharWidth;
    public int tmMaxCharWidth;
    public int tmWeight;
    public int tmOverhang;
    public int tmDigitizedAspectX;
    public int tmDigitizedAspectY;
    public short tmFirstChar;
    public short tmLastChar;
    public short tmDefaultChar;
    public short tmBreakChar;
    public byte tmItalic;
    public byte tmUnderlined;
    public byte tmStruckOut;
    public byte tmPitchAndFamily;
    public byte tmCharSet;
}
```

**Migration Notes**:
- **Padding concern**: VB6 may add padding differently than C#
- Size should be 57 bytes (verify!)
- File also includes `TEXTMETRIC_PADDED_W` variant with explicit padding
- Consider `SkiaSharp.SKFontMetrics` for cross-platform fonts

---

### 2.4 File Format Structures

#### PSP_ChannelHeader (PaintShop Pro Format)
**File**: Modules/ImageFormats_PSP.bas:244

**VB6 Definition**:
```vb6
Public Type PSP_ChannelHeader
    ch_ParentVersionMajor As Long
    ch_ParentWidth As Long
    ch_ParentHeight As Long
    ch_ParentBitDepth As Long
    ch_MaskWidth As Long
    ch_MaskHeight As Long
    ch_Compression As PSPCompression
    ch_CompressedSize As Long
    ch_UncompressedSize As Long
    ch_dstBitmapType As PSPDIBType
    ch_ChannelType As PSPChannelType
    ch_ChannelOK As Boolean
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct PSP_ChannelHeader
{
    public int ch_ParentVersionMajor;
    public int ch_ParentWidth;
    public int ch_ParentHeight;
    public int ch_ParentBitDepth;
    public int ch_MaskWidth;
    public int ch_MaskHeight;
    public PSPCompression ch_Compression;
    public int ch_CompressedSize;
    public int ch_UncompressedSize;
    public PSPDIBType ch_dstBitmapType;
    public PSPChannelType ch_ChannelType;
    [MarshalAs(UnmanagedType.VariantBool)]
    public bool ch_ChannelOK;
}
```

**Migration Notes**:
- **Serialization concern**: Used for PSP file I/O
- Enum sizes matter - VB6 enums are 32-bit `Long`
- Boolean at end may cause alignment issues
- **File I/O**: Must match on-disk format exactly
- See: Classes/pdPSP.cls for usage

---

#### PSPImageHeader
**File**: Modules/ImageFormats_PSP.bas:261

**VB6 Definition**:
```vb6
Public Type PSPImageHeader
    psph_VersionMajor As Long
    psph_VersionMinor As Long
    psph_HeaderSize As Long
    psph_Width As Long
    psph_Height As Long
    psph_Resolution As Double
    psph_ResolutionUnit As PSP_METRIC
    psph_Compression As PSPCompression
    psph_BitDepth As Long
    psph_PlaneCount As Long
    psph_ColorCount As Long
    psph_IsGrayscale As Boolean
    psph_TotalSize As Long
    psph_ActiveLayer As Long
    psph_LayerCount As Long
    psph_ContentFlags As PSPGraphicContents
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct PSPImageHeader
{
    public int psph_VersionMajor;
    public int psph_VersionMinor;
    public int psph_HeaderSize;
    public int psph_Width;
    public int psph_Height;
    public double psph_Resolution;
    public PSP_METRIC psph_ResolutionUnit;
    public PSPCompression psph_Compression;
    public int psph_BitDepth;
    public int psph_PlaneCount;
    public int psph_ColorCount;
    [MarshalAs(UnmanagedType.VariantBool)]
    public bool psph_IsGrayscale;
    public int psph_TotalSize;
    public int psph_ActiveLayer;
    public int psph_LayerCount;
    public PSPGraphicContents psph_ContentFlags;
}
```

**Migration Notes**:
- **Critical serialization structure**
- Boolean embedded in structure - use `VariantBool` marshaling
- Header size is variable - always read from file
- See Classes/pdPSPBlock.cls for block-based I/O

---

#### PD_PNGHeader (PNG Format)
**File**: Modules/PublicEnumsAndTypes.bas:782

**VB6 Definition**:
```vb6
Public Type PD_PNGHeader
    Width As Long
    Height As Long
    ColorType As PD_PNGColorType
    Interlaced As Boolean
    BitDepth As Byte
    BitsPerPixel As Byte
End Type
```

**C# Equivalent**:
```csharp
public struct PD_PNGHeader
{
    public int Width;
    public int Height;
    public PD_PNGColorType ColorType;
    [MarshalAs(UnmanagedType.VariantBool)]
    public bool Interlaced;
    public byte BitDepth;
    public byte BitsPerPixel;
}
```

**Migration Notes**:
- Internal use only (not for file I/O)
- See Classes/pdPNG.cls for actual PNG parsing
- Consider using `ImageSharp` or `SkiaSharp` PNG libraries

---

### 2.5 Windows API Structures

#### OS_MemoryStatusEx (64-bit Memory Info)
**File**: Modules/OS.bas:210

**VB6 Definition**:
```vb6
Private Type OS_MemoryStatusEx
    dwLength As Long
    dwMemoryLoad As Long
    ullTotalPhys As Currency        ' *** 64-bit hack!
    ullAvailPhys As Currency
    ullTotalPageFile As Currency
    ullAvailPageFile As Currency
    ullTotalVirtual As Currency
    ullAvailVirtual As Currency
    ullAvailExtendedVirtual As Currency
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct MEMORYSTATUSEX
{
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;
    public ulong ullAvailPhys;
    public ulong ullTotalPageFile;
    public ulong ullAvailPageFile;
    public ulong ullTotalVirtual;
    public ulong ullAvailVirtual;
    public ulong ullAvailExtendedVirtual;
}
```

**Migration Notes**:
- **VB6 HACK**: Uses `Currency` to represent 64-bit integers!
- Currency is 8 bytes, same as `ulong`
- C# should use proper `ulong` type
- This is a common VB6 workaround for lack of 64-bit integers

---

#### OS_ProcessEntry32 (Process Information)
**File**: Modules/OS.bas:222

**VB6 Definition**:
```vb6
Private Type OS_ProcessEntry32
    dwSize As Long
    cntUsage As Long
    th32ProcessID As Long
    th32DefaultHeapID As Long
    th32ModuleID As Long
    cntThreads As Long
    th32ParentProcessID As Long
    pcPriClassBase As Long
    dwFlags As Long
    szExeFile(0 To MAX_PATH * 2 - 1) As Byte
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct PROCESSENTRY32
{
    public uint dwSize;
    public uint cntUsage;
    public uint th32ProcessID;
    public UIntPtr th32DefaultHeapID;    // *** Pointer in 32-bit, can be 64-bit
    public uint th32ModuleID;
    public uint cntThreads;
    public uint th32ParentProcessID;
    public int pcPriClassBase;
    public uint dwFlags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szExeFile;
}
```

**Migration Notes**:
- `szExeFile` is MAX_PATH * 2 bytes = 260 wide chars
- Consider using `Process` class instead of P/Invoke

---

#### GUID (Globally Unique Identifier)
**File**: Modules/PublicEnumsAndTypes.bas:31

**VB6 Definition**:
```vb6
Public Type Guid
    Data1 As Long
    Data2 As Integer
    Data3 As Integer
    Data4(0 To 7) As Byte
End Type
```

**C# Equivalent**:
```csharp
// Use built-in System.Guid
using Guid = System.Guid;

// Or for P/Invoke:
[StructLayout(LayoutKind.Sequential)]
public struct GUID
{
    public uint Data1;
    public ushort Data2;
    public ushort Data3;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Data4;
}
```

**Migration Notes**:
- **Prefer `System.Guid`** for all managed code
- Only define struct for P/Invoke scenarios
- Size is 16 bytes

---

### 2.6 Application-Specific Structures

#### AutosaveXML (Autosave Metadata)
**File**: Modules/AutosaveEngine.bas:33

**VB6 Definition**:
```vb6
Public Type AutosaveXML
    xmlPath As String
    parentImageID As String
    friendlyName As String
    currentFormat As PD_IMAGE_FORMAT
    originalFormat As PD_IMAGE_FORMAT
    originalPath As String
    originalSessionID As String
    undoStackHeight As Long
    undoStackAbsoluteMaximum As Long
    undoStackPointer As Long
    undoNumAtLastSave As Long
End Type
```

**C# Equivalent**:
```csharp
public class AutosaveXML
{
    public string XmlPath { get; set; }
    public string ParentImageID { get; set; }
    public string FriendlyName { get; set; }
    public PD_IMAGE_FORMAT CurrentFormat { get; set; }
    public PD_IMAGE_FORMAT OriginalFormat { get; set; }
    public string OriginalPath { get; set; }
    public string OriginalSessionID { get; set; }
    public int UndoStackHeight { get; set; }
    public int UndoStackAbsoluteMaximum { get; set; }
    public int UndoStackPointer { get; set; }
    public int UndoNumAtLastSave { get; set; }
}
```

**Migration Notes**:
- **Use class, not struct** (contains strings)
- Serialized to XML (not binary)
- Consider using records for immutability

---

#### PDLanguageFile (Localization Metadata)
**File**: Modules/PublicEnumsAndTypes.bas:110

**VB6 Definition**:
```vb6
Public Type PDLanguageFile
    Author As String
    FileName As String
    langID As String
    LangName As String
    LangType As String
    langVersion As String
    LangStatus As String
    InternalDisplayName As String
    UpdateChecksum As Long
    IsOfficial As Boolean
End Type
```

**C# Equivalent**:
```csharp
public class PDLanguageFile
{
    public string Author { get; set; }
    public string FileName { get; set; }
    public string LangID { get; set; }
    public string LangName { get; set; }
    public string LangType { get; set; }
    public string LangVersion { get; set; }
    public string LangStatus { get; set; }
    public string InternalDisplayName { get; set; }
    public int UpdateChecksum { get; set; }
    public bool IsOfficial { get; set; }
}
```

**Migration Notes**:
- Use class (contains strings)
- Loaded from XML language files
- See Classes/pdTranslate.cls

---

#### PD_ProcessCall (Macro Recording)
**File**: Modules/PublicEnumsAndTypes.bas:712

**VB6 Definition**:
```vb6
Public Type PD_ProcessCall
    pcID As String
    pcParameters As String
    pcUndoType As PD_UndoType
    pcTool As PDTools
    pcRaiseDialog As Boolean
    pcRecorded As Boolean
End Type
```

**C# Equivalent**:
```csharp
public class PD_ProcessCall
{
    public string ID { get; set; }
    public string Parameters { get; set; }
    public PD_UndoType UndoType { get; set; }
    public PDTools Tool { get; set; }
    public bool RaiseDialog { get; set; }
    public bool Recorded { get; set; }
}
```

**Migration Notes**:
- Central to macro recording system
- Parameters stored as serialized string
- Consider using JSON for parameter serialization

---

#### PD_UndoEntry (Undo/Redo Stack)
**File**: Modules/PublicEnumsAndTypes.bas:725

**VB6 Definition**:
```vb6
Public Type PD_UndoEntry
    srcProcCall As PD_ProcessCall
    undoLayerID As Long
    undoFileSize As Long
    thumbnailLarge As pdDIB
End Type
```

**C# Equivalent**:
```csharp
public class PD_UndoEntry
{
    public PD_ProcessCall SrcProcCall { get; set; }
    public int UndoLayerID { get; set; }
    public long UndoFileSize { get; set; }
    public pdDIB ThumbnailLarge { get; set; }
}
```

**Migration Notes**:
- Contains object reference (`pdDIB`)
- Must be class, not struct
- Undo data stored externally in temp files

---

#### PDPackage_ChunkData (Binary Packaging)
**File**: Modules/PDPackaging.bas:16

**VB6 Definition**:
```vb6
Public Type PDPackage_ChunkData
    chunkID As String
    ptrChunkData As Long
    chunkDataLength As Long
    chunkDataFormat As PDPackage_DataType
    chunkCompressionFormat As PD_CompressionFormat
    chunkCompressionLevel As Long
End Type
```

**C# Equivalent**:
```csharp
public struct PDPackage_ChunkData
{
    public string ChunkID;              // 4 chars max
    public IntPtr PtrChunkData;         // *** 32→64-bit concern
    public int ChunkDataLength;
    public PDPackage_DataType ChunkDataFormat;
    public PD_CompressionFormat ChunkCompressionFormat;
    public int ChunkCompressionLevel;
}
```

**Migration Notes**:
- Used for PDI (PhotoDemon Image) file format
- `ptrChunkData` is memory pointer - use `IntPtr`
- See Classes/pdPackageChunky.cls for implementation

---

### 2.7 Plugin Interface Structures

#### OpenJPEG Structures
**File**: Modules/Plugin_OpenJPEG.bas

**opj_image_comp** (VB6):
```vb6
Private Type opj_image_comp
    dx As Long
    dy As Long
    w As Long
    h As Long
    x0 As Long
    y0 As Long
    prec As Long
    opj_bpp As Long
    sgnd As Long
    resno_decoded As Long
    factor As Long
    p_data As Long              ' *** Pointer
    Alpha As Integer
End Type
```

**C# Equivalent**:
```csharp
[StructLayout(LayoutKind.Sequential)]
struct opj_image_comp_t
{
    public uint dx;
    public uint dy;
    public uint w;
    public uint h;
    public uint x0;
    public uint y0;
    public uint prec;
    public uint bpp;
    public uint sgnd;
    public uint resno_decoded;
    public uint factor;
    public IntPtr data;         // *** Pointer to int32 array
    public ushort alpha;
}
```

**Migration Notes**:
- External library struct - must match exactly
- `p_data` points to external memory - use `IntPtr`
- Consider using wrapper library instead of direct P/Invoke

---

#### FreeImage Structures
**File**: Modules/Plugin_FreeImage.bas:24

**BITMAPINFOHEADER** (used by FreeImage):
```vb6
Private Type BITMAPINFOHEADER
    Size As Long
    Width As Long
    Height As Long
    Planes As Integer
    BitCount As Integer
    Compression As Long
    ImageSize As Long
    xPelsPerMeter As Long
    yPelsPerMeter As Long
    ColorUsed As Long
    ColorImportant As Long
End Type
```

**Migration Notes**:
- Same as Windows DIB header (see section 2.1)
- FreeImage is being phased out in favor of specialized libraries
- Consider migrating to ImageSharp/SkiaSharp instead

---

### 2.8 Uniscribe (Text Rendering) Structures

**File**: Modules/Uniscribe.bas:122+

**VB6 Definitions**:
```vb6
Public Type SCRIPT_ANALYSIS
    eScript As Integer
    fRTL As Integer
    fLayoutRTL As Integer
    fLinkBefore As Integer
    fLinkAfter As Integer
    fLogicalOrder As Integer
    fNoGlyphIndex As Integer
    s As SCRIPT_STATE
End Type

Public Type SCRIPT_ITEM
    iCharPos As Long
    a As SCRIPT_ANALYSIS
End Type
```

**C# Equivalents**:
```csharp
[StructLayout(LayoutKind.Sequential)]
public struct SCRIPT_ANALYSIS
{
    public ushort eScript;
    public ushort fFlags;           // Bitfield - be careful!
    public SCRIPT_STATE s;
}

[StructLayout(LayoutKind.Sequential)]
public struct SCRIPT_ITEM
{
    public int iCharPos;
    public SCRIPT_ANALYSIS a;
}
```

**Migration Notes**:
- Complex bitfield structures
- Uniscribe is legacy - consider using DirectWrite or SkiaSharp.HarfBuzz
- See Classes/pdUniscribe.cls for usage

---

## Part 3: Class Structure Overview

### Total Classes: 163+

#### Core Image Classes
| Class | Purpose | Migration Target |
|-------|---------|------------------|
| `pdDIB` | Device Independent Bitmap | Custom class or `ImageSharp.Image<Rgba32>` |
| `pdImage` | Image document (layers + metadata) | Custom class |
| `pdLayer` | Single layer in image | Custom class |
| `pdSelection` | Selection region | Custom class or `SKPath` |
| `pd2DSurface` | Drawing surface abstraction | `SKCanvas` or `Microsoft.Maui.Graphics.ICanvas` |

#### File Format Classes
| Class | Purpose | Migration Notes |
|-------|---------|------------------|
| `pdPNG` | PNG file I/O | Replace with `ImageSharp` or `SkiaSharp` |
| `pdPSD` | Photoshop file I/O | Port or use library |
| `pdPSP` | PaintShop Pro file I/O | Port custom code |
| `pdGIF` | GIF file I/O | Replace with `ImageSharp` |
| `pdWebP` | WebP file I/O | Use `libwebp` wrapper |
| `pdPDF` | PDF import | Use `PDFium` or similar |

#### Graphics Backend Classes
| Class | Purpose | Migration Target |
|-------|---------|------------------|
| `pd2DBrush` | Brush abstraction | `SKPaint` or `Microsoft.Maui.Graphics.SolidPaint` |
| `pd2DPen` | Pen abstraction | `SKPaint` or `Microsoft.Maui.Graphics.Paint` |
| `pd2DPath` | Path abstraction | `SKPath` or `Microsoft.Maui.Graphics.PathF` |
| `pd2DGradient` | Gradient fills | `SKShader` or MAUI gradients |
| `pd2DTransform` | Transforms | `SKMatrix` or `System.Numerics.Matrix3x2` |

#### Utility Classes
| Class | Purpose | Migration Notes |
|-------|---------|------------------|
| `pdString` | String builder | Use `StringBuilder` |
| `pdStringStack` | String stack | Use `Stack<string>` |
| `pdStream` | Binary I/O | Use `BinaryReader/Writer` |
| `pdXML` | XML parsing | Use `System.Xml.Linq` |
| `pdFSO` | File system operations | Use `System.IO` |
| `pdCrypto` | Cryptography | Use `System.Security.Cryptography` |

---

## Part 4: 32-bit to 64-bit Migration Concerns

### Critical Pointer Conversions

#### All Pointer Types MUST Migrate
```vb6
' VB6 (32-bit only)
Dim ptr As Long
ptr = VarPtr(someVariable)

' C# (32/64-bit)
IntPtr ptr = Marshal.GetFunctionPointerForDelegate(someDelegate);
// Or for managed objects:
GCHandle handle = GCHandle.Alloc(someObject);
IntPtr ptr = GCHandle.ToIntPtr(handle);
```

#### Windows Handles
All window handles, device contexts, etc.:
- VB6: `Long`
- C#: `IntPtr`

Affected APIs:
- All GDI functions (HDC, HBITMAP, HFONT, etc.)
- All window functions (HWND)
- All kernel handles (HANDLE)

#### Memory Pointers in Structures

**Before (VB6)**:
```vb6
Type MyStruct
    dataPtr As Long
    dataSize As Long
End Type
```

**After (C#)**:
```csharp
[StructLayout(LayoutKind.Sequential)]
struct MyStruct
{
    public IntPtr dataPtr;      // Changed!
    public int dataSize;
}
```

### Structure Size Changes

Many structures will change size when migrating to 64-bit:

| Structure | VB6 32-bit Size | C# 32-bit Size | C# 64-bit Size | Concern |
|-----------|-----------------|----------------|----------------|---------|
| `SafeArray2D` | 24 bytes | 24 bytes | 32 bytes | **High** - affects memory layout |
| `GP_BitmapData` | 24 bytes | 24 bytes | 32 bytes | **High** - affects GDI+ interop |
| `PROCESSENTRY32` | ~300 bytes | Varies | Varies | Medium - heap ID pointer |
| `PDPackage_ChunkData` | Variable | Variable | Variable | **High** - affects file format |

### Alignment Concerns

VB6 uses different alignment rules than C#:

```csharp
// Explicit packing may be needed:
[StructLayout(LayoutKind.Sequential, Pack = 1)]   // No padding
[StructLayout(LayoutKind.Sequential, Pack = 4)]   // 4-byte alignment
[StructLayout(LayoutKind.Sequential)]             // Natural alignment
```

**Rule of thumb**:
- File I/O structures: Use `Pack = 1` (byte-aligned)
- Windows API structures: Use natural packing (default)
- Internal structures: Use default

---

## Part 5: Serialization Requirements

### 5.1 File Format Headers (Binary)

These structures MUST maintain exact byte layout:

#### PDI Format (PhotoDemon Image)
- **File**: Classes/pdPackager2.cls
- **Concern**: Custom binary format with chunked data
- **Migration**: Ensure chunk headers match exactly
- **Compression**: Uses zlib, lz4, zstd - maintain compatibility

#### PNG Format
- **File**: Classes/pdPNG.cls
- **Concern**: Custom PNG chunk parsing
- **Migration**: Use ImageSharp which handles PNG chunks
- **Note**: Custom chunks (tEXt, zTXt, etc.) for metadata

#### PSP Format (PaintShop Pro)
- **File**: Classes/pdPSP.cls, Modules/ImageFormats_PSP.bas
- **Concern**: Complex block-based format
- **Migration**: Port structure definitions exactly
- **Alignment**: Uses 4-byte alignment for blocks

#### PSD Format (Photoshop)
- **File**: Classes/pdPSD.cls
- **Concern**: Adobe's proprietary format
- **Migration**: Consider using library (e.g., Ntreev.Library.Psd)
- **Complexity**: Very complex, thousands of lines of parsing code

### 5.2 Settings/Preferences (XML)

#### AutosaveXML
- Format: XML
- No binary compatibility concerns
- Use `XmlSerializer` or `System.Text.Json`

#### User Preferences
- File: Data/Preferences.xml
- Format: Custom XML schema
- See: Classes/pdXML.cls for parser

#### Tool Presets
- Multiple XML files in Data/Presets/
- JSON may be better alternative for .NET

### 5.3 Undo/Redo Data

**Format**: Binary files in temp directory
- Compressed layer data
- Serialized image headers
- **Migration concern**: May need converter for old undo files
- **Recommendation**: Version the format, detect old versions

---

## Part 6: Recommended Migration Strategy

### Phase 1: Core Type Infrastructure

1. **Create type mapping library**
   ```
   PhotoDemon.Interop.Types
   - VB6Types.cs (all VB6-compatible structures)
   - WinAPI.cs (Windows API structures)
   - FileFormats.cs (file format structures)
   ```

2. **Define base types**
   - All geometry types (Point, Rect, etc.)
   - All color types (RGBQuad, etc.)
   - All common structures

3. **Add marshaling attributes**
   - `[StructLayout]` on all structures
   - `[MarshalAs]` on all non-standard fields
   - Size verification unit tests

### Phase 2: Memory Management

1. **Replace SafeArray direct access**
   ```csharp
   // Instead of VB6 VarPtr hacks:
   unsafe
   {
       fixed (byte* ptr = imageData)
       {
           // Work with ptr
       }
   }

   // Or safer:
   Span<byte> imageData = stackalloc byte[width * height * 4];
   ```

2. **Create bitmap abstraction layer**
   - Wraps unsafe memory access
   - Provides safe API
   - Handles pixel format conversions

### Phase 3: API Wrappers

1. **Wrap all P/Invoke calls**
   ```csharp
   // Don't use P/Invoke directly in business logic
   // Create wrapper classes:
   internal static class GDI32
   {
       [DllImport("gdi32.dll")]
       private static extern IntPtr CreateDIBSection(...);

       public static Bitmap CreateDIB(...)
       {
           IntPtr hBitmap = CreateDIBSection(...);
           return new Bitmap(hBitmap);
       }
   }
   ```

2. **Validate all structure sizes**
   ```csharp
   [TestMethod]
   public void VerifyStructureSizes()
   {
       Assert.AreEqual(40, Marshal.SizeOf<BITMAPINFOHEADER>());
       Assert.AreEqual(4, Marshal.SizeOf<RGBQuad>());
   }
   ```

### Phase 4: File Format Migration

1. **For each format, decide**:
   - Use library? (PNG → ImageSharp)
   - Port existing code? (PSP, PDI)
   - Rewrite from spec? (If code is messy)

2. **Create format abstraction**:
   ```csharp
   interface IImageFormat
   {
       bool CanLoad(string extension);
       Task<Image> LoadAsync(Stream stream);
       Task SaveAsync(Image image, Stream stream);
   }
   ```

3. **Maintain backward compatibility**
   - Old PDI files must still load
   - Consider format versioning
   - Write upgrade path

### Phase 5: Testing

1. **Unit tests for all structures**
   - Size verification
   - Marshaling verification
   - Round-trip serialization

2. **Integration tests**
   - Load old VB6-created files
   - Verify pixel-perfect rendering
   - Compare with reference images

---

## Part 7: High-Risk Migration Areas

### 7.1 Direct Memory Manipulation

**Risk Level**: **CRITICAL**

**Files Affected**:
- Classes/pdDIB.cls (500+ lines of pointer arithmetic)
- Modules/DibSupport.bas
- All filter modules (Filters_*.bas)

**Issue**: VB6 uses `VarPtr`, `VarPtrArray`, `CopyMemoryStrict` extensively

**Migration Path**:
```csharp
// VB6 approach:
VarPtrArray(pixels) = GetSafeArrayPtr(dibPixels)

// C# equivalent:
unsafe
{
    fixed (byte* pixelPtr = dibPixels)
    {
        Span<RGBQuad> pixels = new Span<RGBQuad>(
            pixelPtr,
            width * height
        );
    }
}

// Or use ImageSharp:
using var image = Image.LoadPixelData<Rgba32>(dibPixels, width, height);
```

### 7.2 Variant Type Hacks

**Risk Level**: HIGH

**Issue**: VB6 uses `Currency` for 64-bit integers

**Files**:
- Modules/OS.bas (OS_MemoryStatusEx)
- Any code dealing with large file sizes

**Migration**:
- Replace `Currency` with `long` or `ulong`
- Check all bitwise operations
- Verify calculations

### 7.3 String Handling

**Risk Level**: MEDIUM

**Issue**: VB6 strings are BSTR (COM strings), C# strings are managed

**Concerns**:
- Fixed-size string arrays in structures
- Embedded null terminators
- ANSI vs Unicode

**Migration**:
```csharp
// For P/Invoke with fixed-size strings:
[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
public string fileName;

// For byte arrays that are actually strings:
[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
public byte[] faceNameBytes;

// Convert when needed:
string faceName = Encoding.Unicode.GetString(faceNameBytes)
    .TrimEnd('\0');
```

### 7.4 File Format Compatibility

**Risk Level**: HIGH

**Issue**: Binary file formats MUST match exactly

**Affected Formats**:
- PDI (PhotoDemon Image) - **CRITICAL**
- Undo files - HIGH
- Settings (XML) - LOW (text-based)
- PSP/PSD reading - MEDIUM (read-only)

**Testing Requirements**:
- Load 100+ real-world PDI files
- Verify pixel-perfect match
- Test all compression formats
- Test all layer types

---

## Part 8: Type Mapping Quick Reference

### Primitive Types
```
VB6           →  C# (Managed)    →  C# (P/Invoke)
-------------------------------------------------------
Byte          →  byte             →  byte
Boolean       →  bool             →  [MarshalAs(UnmanagedType.VariantBool)] bool
Integer       →  short            →  short
Long          →  int              →  int
Single        →  float            →  float
Double        →  double           →  double
Currency      →  decimal or long  →  long
String        →  string           →  string (with [MarshalAs])
Pointer       →  IntPtr           →  IntPtr
```

### Special Cases
```
VB6 Usage              →  C# Equivalent
-------------------------------------------------------
Long (for pointer)     →  IntPtr
Long (for handle)      →  IntPtr
Currency (for Int64)   →  long or ulong
Boolean (in struct)    →  [MarshalAs(UnmanagedType.VariantBool)] bool
Fixed string array     →  [MarshalAs(UnmanagedType.ByValTStr, SizeConst = N)]
Fixed byte array       →  [MarshalAs(UnmanagedType.ByValArray, SizeConst = N)]
```

### Common Windows Types
```
VB6 Type    →  Windows Type  →  C# Type
-------------------------------------------------------
Long        →  HWND          →  IntPtr
Long        →  HDC           →  IntPtr
Long        →  HBITMAP       →  IntPtr
Long        →  HFONT         →  IntPtr
Long        →  HANDLE        →  IntPtr
Long        →  LPVOID        →  IntPtr
Long        →  DWORD         →  uint
Integer     →  WORD          →  ushort
Byte        →  BYTE          →  byte
Boolean     →  BOOL          →  [MarshalAs(UnmanagedType.Bool)] bool
```

---

## Part 9: Code References

### Key Files with Type Declarations

#### Core Types (52 .bas files with Types)
```
Modules/Drawing2D.bas:312-386         - Core geometry types
Modules/PublicEnumsAndTypes.bas:1-805 - Application types
Modules/GDIPlus.bas:772-922          - GDI+ structures
Modules/GDI.bas:23-72                - GDI structures
Modules/Fonts.bas:32-288             - Font structures
Modules/OS.bas:203-274               - Windows API structures
Modules/VB_Hacks.bas:33-82           - Clipboard/COM structures
Modules/ImageFormats_PSP.bas:244-288 - PSP file format
Modules/Plugin_OpenJPEG.bas:117-355  - JPEG2000 plugin
Modules/AutosaveEngine.bas:33-45     - Autosave metadata
Modules/PDPackaging.bas:16-23        - PDI file format
```

#### Image Processing Classes
```
Classes/pdDIB.cls            - Device Independent Bitmap
Classes/pdImage.cls          - Image document
Classes/pdLayer.cls          - Image layer
Classes/pd2DSurface.cls      - Drawing surface
Classes/pd2DBrush.cls        - Brush abstraction
Classes/pd2DPen.cls          - Pen abstraction
Classes/pd2DPath.cls         - Path abstraction
```

#### File Format Classes
```
Classes/pdPNG.cls           - PNG format
Classes/pdPSD.cls           - Photoshop format
Classes/pdPSP.cls           - PaintShop Pro format
Classes/pdGIF.cls           - GIF format
Classes/pdWebP.cls          - WebP format
Classes/pdPackager2.cls     - PDI packaging
```

---

## Part 10: Summary and Recommendations

### Critical Path Items

1. **Immediate**: Define all core structures with proper marshaling
2. **High Priority**: Migrate `pdDIB` class (core image data)
3. **High Priority**: Migrate file I/O for PDI format
4. **Medium Priority**: Migrate all file format loaders
5. **Lower Priority**: Migrate UI-specific types

### Recommended Technology Stack

#### Image Processing
- **Primary**: `SixLabors.ImageSharp` (pure .NET, cross-platform)
- **Secondary**: `SkiaSharp` (for rendering)
- **Fallback**: Custom unsafe code with `Span<T>`

#### Graphics Backend
- **Primary**: `Microsoft.Maui.Graphics` (MAUI apps)
- **Secondary**: `SkiaSharp` (more features)
- **Windows-specific**: Keep some GDI+ for compatibility

#### File I/O
- **PNG**: ImageSharp
- **JPEG**: ImageSharp or SkiaSharp
- **WebP**: libwebp P/Invoke wrapper
- **PSP/PDI**: Port existing code
- **PSD**: Consider 3rd-party library

#### Text Rendering
- **Primary**: `SkiaSharp.HarfBuzz` (cross-platform)
- **Secondary**: Platform-specific (CoreText, DirectWrite, Pango)
- **Fallback**: MAUI built-in text

### Testing Strategy

1. **Structure Size Tests**
   - Verify all structure sizes match expected values
   - Test on both 32-bit and 64-bit builds

2. **Marshaling Tests**
   - Round-trip all structures through P/Invoke
   - Verify byte-level compatibility

3. **File Format Tests**
   - Load 1000+ real-world PDI files
   - Verify pixel-perfect match with VB6 version
   - Test all supported formats

4. **Performance Tests**
   - Image loading/saving performance
   - Filter performance
   - Memory usage

### Risk Assessment

| Risk Area | Severity | Mitigation |
|-----------|----------|------------|
| Pointer migration | **CRITICAL** | Extensive testing, use `Span<T>` |
| File format compatibility | **HIGH** | Binary-level validation tests |
| Performance regression | **MEDIUM** | Benchmark against VB6 version |
| Memory leaks | **MEDIUM** | Use `IDisposable`, memory profiling |
| Platform differences | **LOW** | MAUI abstracts most differences |

### Next Steps

1. Review this document with team
2. Create prototype for `pdDIB` migration
3. Build structure size validation suite
4. Create file format compatibility tests
5. Begin incremental migration

---

## Appendices

### Appendix A: VB6 Type Size Reference

| VB6 Type | Size | Notes |
|----------|------|-------|
| Byte | 1 byte | Unsigned |
| Boolean | 2 bytes | -1 = True, 0 = False |
| Integer | 2 bytes | Signed |
| Long | 4 bytes | Signed |
| Single | 4 bytes | IEEE 754 |
| Double | 8 bytes | IEEE 754 |
| Currency | 8 bytes | Fixed-point decimal |
| String | Variable | BSTR (4-byte length prefix + Unicode data + null) |

### Appendix B: Structure Packing Rules

**VB6 Packing**:
- Aligns to natural boundaries (1, 2, 4 bytes)
- No explicit control over packing
- Arrays inline (no padding between elements)

**C# Packing**:
```csharp
[StructLayout(LayoutKind.Sequential)]        // Default: natural alignment
[StructLayout(LayoutKind.Sequential, Pack = 1)] // No padding
[StructLayout(LayoutKind.Sequential, Pack = 2)] // 2-byte alignment
[StructLayout(LayoutKind.Sequential, Pack = 4)] // 4-byte alignment
[StructLayout(LayoutKind.Sequential, Pack = 8)] // 8-byte alignment
[StructLayout(LayoutKind.Explicit)]           // Manual field offsets
```

### Appendix C: Common Pitfalls

1. **Boolean size**: VB6 = 2 bytes, C# = 1 byte
2. **Integer ≠ int**: VB6 Integer = C# short
3. **Long ≠ long**: VB6 Long = C# int
4. **Pointer type**: VB6 Long → C# IntPtr
5. **String in struct**: Requires explicit marshaling
6. **Array in struct**: Requires `ByValArray` attribute
7. **Enum size**: VB6 = 4 bytes (Long), C# = 4 bytes (int) by default

---

**End of Document**

**Document Version**: 1.0
**Total Structures Documented**: 40+ detailed, 195+ total
**Total Classes Identified**: 163+
**Migration Complexity**: High
**Estimated Effort**: 6-12 months for complete type migration
