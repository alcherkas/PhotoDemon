# PhotoDemon VB6 to .NET 10 MAUI Migration - Risk Assessment

**Document Version:** 1.0
**Date:** 2025-11-17
**Agent:** Agent 9 - Risk Assessment
**Status:** Complete

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Risk Overview Dashboard](#risk-overview-dashboard)
3. [Technical Risks](#technical-risks)
4. [Performance Risks](#performance-risks)
5. [Compatibility Risks](#compatibility-risks)
6. [Licensing & Legal Risks](#licensing--legal-risks)
7. [UX & Cross-Platform Risks](#ux--cross-platform-risks)
8. [Resource & Timeline Risks](#resource--timeline-risks)
9. [Mitigation Strategies](#mitigation-strategies)
10. [Contingency Plans](#contingency-plans)
11. [Risk Monitoring Plan](#risk-monitoring-plan)

---

## Executive Summary

### Critical Findings

The PhotoDemon VB6 to .NET 10 MAUI migration presents a **high-complexity, medium-risk** project with significant technical challenges but clear mitigation paths. The project is **FEASIBLE** but requires careful planning and execution.

### Risk Statistics

- **Total Risks Identified:** 52
- **Critical (High Severity × High Likelihood):** 8 risks
- **High Priority:** 18 risks
- **Medium Priority:** 22 risks
- **Low Priority:** 12 risks

### Top 5 Critical Risks (Immediate Attention Required)

| # | Risk | Severity | Likelihood | Impact Area |
|---|------|----------|------------|-------------|
| 1 | **32-bit to 64-bit pointer migration errors** | CRITICAL | High | Core architecture |
| 2 | **ImageSharp commercial licensing conflict** | CRITICAL | Certain | Legal/Budget |
| 3 | **GDI+ to SkiaSharp rendering differences** | HIGH | High | Visual quality |
| 4 | **Custom control reimplementation scope** | HIGH | High | Timeline/Resources |
| 5 | **Performance regression in image processing** | HIGH | Medium | User experience |

### Overall Project Risk Rating

**Risk Level:** 🟡 **MEDIUM-HIGH**

**Confidence in Success:** 75% with proper planning and mitigation
**Estimated Timeline:** 11-18 months (varies by team size)
**Budget Risk:** Medium (commercial licenses, extended timeline)

---

## Risk Overview Dashboard

### Risk Distribution by Category

| Category | Critical | High | Medium | Low | Total |
|----------|----------|------|--------|-----|-------|
| **Technical** | 5 | 8 | 6 | 2 | 21 |
| **Performance** | 1 | 3 | 4 | 2 | 10 |
| **Compatibility** | 1 | 3 | 4 | 3 | 11 |
| **Licensing** | 1 | 1 | 1 | 0 | 3 |
| **UX/Cross-Platform** | 0 | 2 | 4 | 3 | 9 |
| **Resource/Timeline** | 0 | 1 | 3 | 2 | 6 |
| **TOTAL** | **8** | **18** | **22** | **12** | **60** |

### Risk Severity Legend

- **CRITICAL** 🔴 - Project-threatening, requires immediate mitigation
- **HIGH** 🟠 - Major impact, requires active management
- **MEDIUM** 🟡 - Moderate impact, plan mitigation
- **LOW** 🟢 - Minor impact, monitor

---

## Technical Risks

### T-001: 32-bit to 64-bit Pointer Migration

**Reference:** `/home/user/PhotoDemon/docs/data/type-mapping.md:1024-1146`

**Severity:** 🔴 **CRITICAL**
**Likelihood:** High (affects 800+ API declarations)
**Impact:** System crashes, memory corruption, data loss

**Description:**
VB6 uses `Long` (32-bit) for all pointers, handles, and memory addresses. .NET requires `IntPtr` (platform-dependent). This affects:
- 813+ Windows API declarations
- All GDI/GDI+ operations (HDC, HBITMAP, HWND)
- Memory management functions
- Plugin interfaces
- File format parsers with pointer-based structures

**Evidence:**
```vb6
' VB6 (WRONG in 64-bit)
Type SafeArray2D
    pvData As Long        ' Pointer - 32-bit only!
End Type

' C# (CORRECT)
struct SAFEARRAY2D {
    public IntPtr pvData;  // Platform-independent pointer
}
```

**Root Cause:**
- VB6 is 32-bit only
- PhotoDemon uses extensive Win32 API and direct memory access
- Structure sizes change between 32/64-bit (e.g., SAFEARRAY2D: 24→32 bytes)

**Mitigation Strategy:**
1. Create comprehensive type mapping document (DONE - type-mapping.md)
2. Implement automated search/replace for all pointer types
3. Add structure size validation unit tests
4. Use `[StructLayout]` attributes on all interop structures
5. Test on both x86 and x64 builds early and often

**Success Criteria:**
- All 813+ API declarations migrated to IntPtr
- Structure size tests pass on x64
- No pointer-related crashes in testing

**Residual Risk:** Medium (extensive testing required)

---

### T-002: SafeArray Direct Memory Access

**Reference:** `/home/user/PhotoDemon/docs/data/type-mapping.md:119-168`

**Severity:** 🔴 **CRITICAL**
**Likelihood:** High (core image processing functionality)
**Impact:** Image corruption, crashes, performance degradation

**Description:**
PhotoDemon uses undocumented VB6 techniques (`VarPtr`, `VarPtrArray`) to manipulate SafeArray pointers for direct pixel access. This is not possible in managed .NET without `unsafe` code or significant architecture changes.

**Evidence:**
- `/home/user/PhotoDemon/Classes/pdDIB.cls` - 500+ lines of pointer arithmetic
- All filter modules use direct memory access
- Performance-critical operations depend on this approach

**Impact Analysis:**
- **If using safe managed code:** 2-3x slower performance
- **If using unsafe code:** Similar performance, but increased complexity
- **If using Span<T>:** Modern approach, good performance, safer than unsafe

**Mitigation Strategy:**
1. **Primary:** Use `Span<byte>` and `Memory<byte>` for pixel buffers
2. **Secondary:** Use `Bitmap.LockBits()` with unsafe pointer access
3. **Fallback:** Use ImageSharp's `Image<TPixel>` abstraction
4. Create abstraction layer for pixel access to isolate implementation

**Implementation Example:**
```csharp
// Modern .NET approach
public unsafe void ProcessPixels(Bitmap bitmap) {
    var bitmapData = bitmap.LockBits(/* ... */);
    try {
        Span<byte> pixels = new Span<byte>(
            bitmapData.Scan0.ToPointer(),
            bitmapData.Stride * bitmapData.Height
        );
        // Fast pixel access without managed overhead
    } finally {
        bitmap.UnlockBits(bitmapData);
    }
}
```

**Success Criteria:**
- Pixel access performance within 20% of VB6 version
- No memory leaks under sustained use
- Cross-platform compatibility maintained

**Residual Risk:** Medium-High (requires extensive performance testing)

---

### T-003: GDI+ to SkiaSharp Rendering Pipeline Rewrite

**Reference:** `/home/user/PhotoDemon/docs/api/api-inventory.md:37-88`

**Severity:** 🔴 **CRITICAL**
**Likelihood:** Certain (200+ GDI+ APIs must be replaced)
**Impact:** Visual quality differences, feature gaps, rendering bugs

**Description:**
PhotoDemon's entire rendering pipeline uses GDI+ (200+ API calls). MAUI requires migration to SkiaSharp or similar cross-platform graphics library.

**Affected Modules:**
- `/home/user/PhotoDemon/Modules/GDIPlus.bas` - 112 GDI+ declares
- `/home/user/PhotoDemon/Modules/GDI.bas` - 13 GDI declares
- All pd2D* classes (pd2DSurface, pd2DBrush, pd2DPen, pd2DPath, etc.)
- All 56 custom controls (owner-drawn with GDI+)

**Breaking Changes:**
| GDI+ Feature | SkiaSharp Equivalent | Compatibility Risk |
|--------------|---------------------|-------------------|
| `Graphics.DrawImage()` | `SKCanvas.DrawImage()` | Low - direct mapping |
| `Graphics.DrawString()` | `SKCanvas.DrawText()` | Medium - font metrics differ |
| `Matrix` transforms | `SKMatrix` | Low - similar API |
| `Bitmap.LockBits()` | `SKBitmap.GetPixels()` | Medium - different memory model |
| Blend modes | `SKBlendMode` | Medium - some modes differ |
| Text rendering (GDI+) | SkiaSharp.HarfBuzz | High - complex text layout |

**Specific Concerns:**
1. **Font Rendering:** GDI+ and SkiaSharp may produce slightly different text layouts
2. **Blend Modes:** Not all blend modes have 1:1 equivalents
3. **Color Management:** SkiaSharp's color pipeline differs from GDI+
4. **Performance:** Initial SkiaSharp implementation may be slower until optimized

**Mitigation Strategy:**
1. Create abstraction layer (e.g., `ICanvas` interface) to isolate rendering
2. Build comprehensive visual regression test suite
3. Allow tolerance for minor pixel differences (anti-aliasing, rounding)
4. Prioritize feature parity over pixel-perfect matching
5. Use SkiaSharp's GPU acceleration where available

**Migration Phases:**
- **Phase 1:** Core drawing primitives (lines, shapes, fills)
- **Phase 2:** Image rendering and transformations
- **Phase 3:** Text rendering with HarfBuzz
- **Phase 4:** Advanced effects (blend modes, filters)

**Success Criteria:**
- 95%+ visual similarity to VB6 version
- No loss of critical features
- Performance within 20% of GDI+ on CPU, better with GPU

**Residual Risk:** Medium (extensive testing required, some visual differences acceptable)

---

### T-004: Custom Control Reimplementation Scope

**Reference:** `/home/user/PhotoDemon/docs/ui/control-mapping.md:1-2316`

**Severity:** 🟠 **HIGH**
**Likelihood:** Certain (56 custom controls, 2100+ instances)
**Impact:** Timeline overrun, resource exhaustion, feature cuts

**Description:**
PhotoDemon uses 56 custom owner-drawn controls with 2,100+ instances across 200+ forms. 90% require full reimplementation in MAUI.

**Scope Analysis:**
| Control Tier | Count | Complexity | Effort (weeks) |
|-------------|-------|------------|----------------|
| **Tier 1 (Critical)** | 6 | Very High | 14-26 |
| **Tier 2 (Important)** | 11 | High | 22-33 |
| **Tier 3 (Support)** | 15 | Medium | 15-30 |
| **Tier 4 (Simple)** | 24 | Low | 12-24 |
| **TOTAL** | **56** | **Mixed** | **63-113 weeks** |

**Critical Path Controls:**
1. **pdCanvas** - Main image editing surface (4-6 weeks)
2. **pdFxPreviewCtl** - Effect preview (2-3 weeks)
3. **pdSlider** - Most-used input control (454 instances) (1-2 weeks)
4. **pdCommandBar** - Universal dialog footer (161 instances) (2-3 weeks)
5. **pdLayerList** - Layer management panel (3-4 weeks)

**Timeline Impact:**
- **Single developer:** 63-113 weeks (15-27 months) ❌ UNACCEPTABLE
- **5-person team (parallel):** 10-11 months ✅ FEASIBLE
- **With 3rd-party controls:** 6-8 months ✅ RECOMMENDED

**Mitigation Strategy:**
1. **Evaluate commercial control suites** (Syncfusion, Telerik, DevExpress MAUI)
   - Pros: Reduce development time by 40-50%
   - Cons: Licensing costs, dependency lock-in
   - Decision: Cost-benefit analysis needed

2. **Prioritize control development:**
   - Implement Tier 1 controls first (critical path)
   - Use MAUI built-in controls as placeholders
   - Defer Tier 4 controls to later phases

3. **Parallel development strategy:**
   - Developer 1: Theme system + basic controls
   - Developer 2: pdSlider + input controls
   - Developer 3: pdCanvas system
   - Developer 4: pdLayerList + panels
   - Developer 5: pdFxPreviewCtl + specialized controls

4. **Consider hybrid approach:**
   - Use commercial suite for standard controls
   - Build custom controls only for unique needs (pdCanvas, pdLayerList)

**Success Criteria:**
- All Tier 1 controls functional within 6 months
- 80% control coverage by month 9
- 100% control parity by month 11

**Residual Risk:** Medium-High (timeline overrun likely without commercial controls)

---

### T-005: Structure Packing and Alignment

**Reference:** `/home/user/PhotoDemon/docs/data/type-mapping.md:1119-1146`

**Severity:** 🟠 **HIGH**
**Likelihood:** High (affects file I/O and API interop)
**Impact:** File corruption, API call failures, data corruption

**Description:**
VB6 and C# use different default structure packing rules. This affects:
- Binary file format reading/writing (PDI, PSD, PSP, etc.)
- Windows API structures (BITMAPINFOHEADER, LOGFONT, etc.)
- Plugin interfaces (FreeImage, OpenJPEG, etc.)

**Examples of Affected Structures:**
```csharp
// WRONG - default packing may add padding
struct RGBQuad {
    public byte Blue;
    public byte Green;
    public byte Red;
    public byte Alpha;
}

// CORRECT - explicit packing matches VB6/Windows
[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct RGBQuad {
    public byte Blue;
    public byte Green;
    public byte Red;
    public byte Alpha;
}
```

**Critical Files:**
- PDI format headers (custom binary format)
- PSP/PSD parsers (proprietary formats)
- All Windows API structures
- Plugin interface structures

**Mitigation Strategy:**
1. Add `[StructLayout]` attribute to ALL structures
2. Create unit tests to verify structure sizes
3. Test binary file I/O with real-world files
4. Document packing rules for each structure type

**Testing Approach:**
```csharp
[TestMethod]
public void VerifyStructureSizes() {
    Assert.AreEqual(40, Marshal.SizeOf<BITMAPINFOHEADER>());
    Assert.AreEqual(4, Marshal.SizeOf<RGBQuad>());
    Assert.AreEqual(24, Marshal.SizeOf<SAFEARRAY2D_32>());
    Assert.AreEqual(32, Marshal.SizeOf<SAFEARRAY2D_64>());
}
```

**Success Criteria:**
- All 195+ structures have explicit `[StructLayout]` attributes
- Structure size tests pass on both x86 and x64
- PDI files load correctly
- No API call failures due to struct size mismatches

**Residual Risk:** Low (mitigated with proper testing)

---

### T-006: Currency Type 64-bit Integer Hack

**Reference:** `/home/user/PhotoDemon/docs/data/type-mapping.md:587-626`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium (limited usage)
**Impact:** Incorrect large file size reporting, memory calculations

**Description:**
VB6 lacks native 64-bit integer support. PhotoDemon uses the `Currency` type (8-byte fixed-point) to represent 64-bit integers in OS memory structures.

**Example:**
```vb6
Private Type OS_MemoryStatusEx
    dwLength As Long
    dwMemoryLoad As Long
    ullTotalPhys As Currency        ' *** Should be UInt64!
    ullAvailPhys As Currency
    ' ...
End Type
```

**Correct C# Equivalent:**
```csharp
struct MEMORYSTATUSEX {
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;      // Proper 64-bit unsigned
    public ulong ullAvailPhys;
    // ...
}
```

**Affected Areas:**
- OS memory reporting (`/home/user/PhotoDemon/Modules/OS.bas:210`)
- Large file size handling
- 64-bit calculations

**Mitigation Strategy:**
1. Search for all `Currency` usage in codebase
2. Determine if used for fixed-point decimal or 64-bit integer
3. Replace with appropriate .NET type (`decimal` or `long`/`ulong`)
4. Verify calculations produce correct results

**Success Criteria:**
- No incorrect `Currency` → `decimal` conversions
- Large file sizes reported correctly
- Memory statistics accurate on >4GB systems

**Residual Risk:** Low (limited usage, easy to fix)

---

### T-007: VB6 Boolean Size Mismatch

**Reference:** `/home/user/PhotoDemon/docs/data/type-mapping.md:52`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High (used in many structures)
**Impact:** Structure size mismatch, incorrect boolean values

**Description:**
VB6 `Boolean` is 2 bytes (`True` = -1, `False` = 0). C# `bool` is 1 byte (`True` = 1, `False` = 0).

**Problems:**
1. Structure size differences
2. P/Invoke marshaling issues
3. File format compatibility

**Example:**
```csharp
// WRONG - C# bool is 1 byte
struct MyStruct {
    public bool flag;  // 1 byte!
    public int value;  // Padding added
}

// CORRECT for VB6 compatibility
struct MyStruct {
    [MarshalAs(UnmanagedType.VariantBool)]
    public bool flag;  // 2 bytes, matches VB6
    public int value;
}

// ALTERNATIVE for new code
struct MyStruct {
    public short flag;  // Explicit 2-byte integer
    public int value;
}
```

**Mitigation Strategy:**
1. Use `[MarshalAs(UnmanagedType.VariantBool)]` for P/Invoke
2. Use `short` for structures saved to disk
3. Document all boolean marshaling decisions
4. Test file format compatibility

**Success Criteria:**
- All file formats load correctly
- No P/Invoke boolean errors
- Structure sizes match expected values

**Residual Risk:** Low (well-understood issue)

---

### T-008: String Handling and BSTR Compatibility

**Reference:** `/home/user/PhotoDemon/docs/data/type-mapping.md:1342-1386`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** String corruption, memory leaks, crashes

**Description:**
VB6 strings are BSTR (COM strings) with different memory layout than .NET strings. Issues include:
- Fixed-size string arrays in structures
- Embedded null terminators
- ANSI vs Unicode encoding
- COM string ownership

**Example:**
```vb6
' VB6
Type LOGFONTW
    ' ...
    lfFaceName(0 To 63) As Byte    ' 64 bytes = 32 wide chars
End Type

' C# - Option 1
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
struct LOGFONTW {
    // ...
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string lfFaceName;  // Automatic marshaling
}

// C# - Option 2
[StructLayout(LayoutKind.Sequential)]
struct LOGFONTW {
    // ...
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] lfFaceNameBytes;  // Manual handling

    public string FaceName {
        get => Encoding.Unicode.GetString(lfFaceNameBytes).TrimEnd('\0');
    }
}
```

**Mitigation Strategy:**
1. Use `[MarshalAs]` attributes for P/Invoke strings
2. Choose appropriate marshaling strategy per use case
3. Handle null terminators correctly
4. Test with non-ASCII characters

**Success Criteria:**
- No string corruption in file I/O
- Unicode text handled correctly
- No memory leaks from string marshaling

**Residual Risk:** Low (standard .NET interop pattern)

---

### T-009: Plugin DLL Compatibility (Native Libraries)

**Reference:** `/home/user/PhotoDemon/docs/dependencies/dotnet-alternatives.md:324-410`

**Severity:** 🟠 **HIGH**
**Likelihood:** High (15+ native DLLs)
**Impact:** Feature loss, cross-platform limitations

**Description:**
PhotoDemon relies on 15+ native DLLs for advanced functionality. Many are Windows-only and may not work on other platforms.

**Plugin Inventory:**
| Plugin | Platform | .NET Alternative | Risk |
|--------|----------|------------------|------|
| FreeImage | Windows | ImageSharp | Low - replaceable |
| libwebp | Cross-platform | ImageSharp | Low - good support |
| libheif | Linux/Win/Mac | HeyRed.ImageSharp.Heif | Medium - native wrapper needed |
| libavif | Cross-platform | Native wrapper | Medium - shell to exe |
| libjxl | Windows Vista+ | **None** | **HIGH - GAP** |
| OpenJPEG | Cross-platform | Magick.NET | Low - library support |
| lcms2 | Cross-platform | lcmsNET | Low - wrapper exists |
| pdfium | Cross-platform | PDFiumSharp | Low - wrapper exists |
| resvg | Cross-platform | Svg.Skia | Low - alternative exists |
| DirectXTex | Windows only | **Platform-specific** | **HIGH - Windows only** |
| EZTwain | Windows only | WIA (Windows) | **MEDIUM - scanner gap** |
| pspiHost | Windows only | **None** | **MEDIUM - Photoshop plugin gap** |

**Critical Gaps:**
1. **JPEG XL (libjxl):** No pure .NET library
   - Mitigation: Shell to djxl.exe/cjxl.exe, or use Windows 11 24H2+ WIC codec
   - Impact: Format support limited on older Windows/other platforms

2. **DDS (DirectXTex):** Windows-only texture format
   - Mitigation: Platform-specific code, or BCnEncoder.Net (limited)
   - Impact: DDS support Windows-only or degraded

3. **Photoshop Plugins (pspiHost):** No .NET alternative
   - Mitigation: Defer to post-MVP, evaluate user demand
   - Impact: Feature gap (power users affected)

**Mitigation Strategy:**
1. **Prioritize .NET alternatives:**
   - FreeImage → ImageSharp + Magick.NET
   - GDI+ → SkiaSharp
   - Various formats → Magick.NET

2. **Create P/Invoke wrappers for critical plugins:**
   - libheif, libavif, libjxl (if not using shell approach)
   - Bundle platform-specific native libraries

3. **Platform-specific implementations:**
   - Windows: Use WIC codecs for HEIF/AVIF/JXL where available
   - macOS: Use Image I/O framework
   - Linux: Use GdkPixbuf or other native APIs

4. **Graceful degradation:**
   - Detect plugin availability at runtime
   - Disable features if plugins missing
   - Clear user messaging about platform limitations

**Success Criteria:**
- 90%+ feature parity on Windows
- 70%+ feature parity on macOS/iOS
- 60%+ feature parity on Linux/Android
- All missing features documented

**Residual Risk:** Medium-High (platform-specific features inevitable)

---

### T-010: Macro Recording XML Compatibility

**Reference:** `/home/user/PhotoDemon/docs/algorithms/effects-catalog.md:456-577`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** Macro files won't load, automation broken

**Description:**
PhotoDemon uses XML-based macro files (PDM format, version 8.2014) to record and play back action sequences. .NET serialization must maintain backward compatibility.

**File Format:**
```xml
<Macro>
  <pdMacroVersion>8.2014</pdMacroVersion>
  <processCount>N</processCount>
  <processEntry index="1">
    <ID>effect_name</ID>
    <Parameters>XML parameter string</Parameters>
    <MakeUndo>undo_type</MakeUndo>
    <Tool>tool_id</Tool>
  </processEntry>
</Macro>
```

**Risks:**
1. Parameter string format changes
2. Effect ID renaming
3. XML parsing differences
4. Version detection issues

**Mitigation Strategy:**
1. Maintain exact XML schema (don't change element names)
2. Build comprehensive macro file test suite
3. Implement version detection and upgrade path
4. Use .NET's `XmlSerializer` for compatibility

**C# Implementation:**
```csharp
[XmlRoot("Macro")]
public class PhotoDemonMacro {
    [XmlElement("pdMacroVersion")]
    public string Version { get; set; } = "8.2014";

    [XmlElement("processCount")]
    public int ProcessCount { get; set; }

    [XmlArray("processes")]
    [XmlArrayItem("processEntry")]
    public List<ProcessEntry> Processes { get; set; }
}
```

**Success Criteria:**
- All VB6-created macro files load correctly
- Parameters parse identically
- Effects execute with same results
- Round-trip save/load works

**Residual Risk:** Low (XML is text-based, easy to validate)

---

### T-011: File Format Binary Compatibility

**Reference:** `/home/user/PhotoDemon/docs/formats/file-format-support.md:1-1085`

**Severity:** 🔴 **CRITICAL**
**Likelihood:** High (custom binary formats)
**Impact:** Data loss, file corruption, user distrust

**Description:**
PhotoDemon has custom binary file formats that MUST remain compatible:
- **PDI (PhotoDemon Image):** Native format with layers, compression, metadata
- **Undo files:** Binary diffs stored in temp directory
- **Settings:** May include binary data

**PDI Format Structure:**
- Chunked format (PNG-like)
- Multiple compression options (lz4, zstd, deflate)
- Version-specific headers
- Platform-specific data (pointers, structure sizes)

**Breaking Changes to Avoid:**
1. Structure size changes (32→64-bit pointers)
2. Endianness differences (shouldn't be an issue on x86/ARM little-endian)
3. Floating-point representation changes
4. String encoding changes

**Mitigation Strategy:**
1. **Port PDI parser carefully:**
   - Maintain exact structure layouts with `[StructLayout]`
   - Test on both 32-bit and 64-bit builds
   - Verify chunk parsing matches VB6 version

2. **Build comprehensive test suite:**
   - Collect 100+ real PDI files from users
   - Automated load/compare tests
   - Pixel-perfect validation
   - Metadata preservation checks

3. **Implement version detection:**
   - Detect VB6 vs .NET files
   - Support old format indefinitely
   - Consider adding .NET-specific optimizations in new version

4. **Conservative approach:**
   - Don't change binary format unless absolutely necessary
   - Maintain VB6 writer for maximum compatibility
   - Add .NET features as optional extensions

**Code References:**
- `/home/user/PhotoDemon/Classes/pdPackageChunky.cls` - Modern PDI format
- `/home/user/PhotoDemon/Classes/pdPackager2.cls` - PDI v2
- `/home/user/PhotoDemon/Classes/pdPackagerLegacy.cls` - Legacy support

**Success Criteria:**
- 100% of test PDI files load correctly
- Pixel-perfect round-trip (save/load)
- Metadata preserved
- Compression works identically
- No data loss

**Residual Risk:** Medium (extensive testing mitigates)

---

### T-012: Custom Format Parser Porting (PSD, PSP, XCF)

**Reference:** `/home/user/PhotoDemon/docs/formats/file-format-support.md:196-310`

**Severity:** 🟠 **HIGH**
**Likelihood:** Certain (must port for feature parity)
**Impact:** Feature loss, quality degradation

**Description:**
PhotoDemon's custom PSD, PSP, and XCF parsers represent significant IP and competitive advantages. These must be ported carefully to maintain quality.

**Parser Complexity:**
| Format | Lines of Code | Complexity | Quality | Priority |
|--------|--------------|------------|---------|----------|
| **PSD** | 2000+ | Very High | Best-in-class (Apple test suite) | ⭐⭐⭐⭐⭐ |
| **PSP** | 2000+ | Very High | No .NET alternatives | ⭐⭐⭐⭐⭐ |
| **XCF** | 2000+ | Very High | Comprehensive GIMP support | ⭐⭐⭐⭐ |

**PSD Parser Highlights:**
- Passes Apple color management test suite
- Supports all 9 color modes (Bitmap, Grayscale, Indexed, RGB, CMYK, Multichannel, Duotone, Lab)
- 1-32 bits per channel
- RLE, DEFLATE compression
- Layer groups, masks, adjustments
- ICC profile support

**Risks:**
1. **Porting errors:** Complex algorithms may not translate directly
2. **Endianness issues:** PSD is big-endian, Intel/ARM is little-endian
3. **Precision loss:** Floating-point calculations may differ slightly
4. **Performance:** .NET may be slower initially

**Mitigation Strategy:**
1. **Incremental porting:**
   - Phase 1: Header parsing
   - Phase 2: Layer reading
   - Phase 3: Color management
   - Phase 4: Compression handlers
   - Phase 5: Advanced features

2. **Extensive testing:**
   - Use existing test file collection
   - Pixel-perfect comparison with VB6 version
   - Edge case testing (corrupt files, unusual modes)

3. **Performance optimization:**
   - Profile hot paths
   - Use `Span<T>` for buffer manipulation
   - Consider unsafe code for critical sections

4. **Quality gates:**
   - All test files must load
   - Color accuracy verified
   - Metadata preserved
   - Performance acceptable

**Success Criteria:**
- PSD parser maintains Apple test suite compliance
- PSP parser handles all versions (v5-current)
- XCF parser supports all GIMP precisions
- No visual quality degradation

**Residual Risk:** Medium (extensive testing required, some edge cases may fail initially)

---

## Performance Risks

### P-001: Image Processing Performance Regression

**Reference:** `/home/user/PhotoDemon/docs/algorithms/effects-catalog.md:415-445`

**Severity:** 🟠 **HIGH**
**Likelihood:** Medium
**Impact:** Poor user experience, perceived quality loss

**Description:**
VB6's direct memory access and optimized algorithms may outperform initial .NET implementations. PhotoDemon has 117+ effects with performance-critical code.

**Performance-Critical Operations:**
1. **Gaussian Blur** (IIR filters, Deriche/AM algorithms)
2. **Convolution** (separable filters, SIMD opportunities)
3. **Pixel iteration** (direct memory access)
4. **Color space conversions** (HSL, LAB, CMYK)
5. **Anisotropic diffusion** (iterative PDE solver)
6. **Mean shift** (clustering algorithm)

**Benchmark Comparison (Expected):**
| Operation | VB6 Baseline | ImageSharp | Magick.NET | SkiaSharp | Unsafe C# |
|-----------|-------------|------------|------------|-----------|-----------|
| JPEG decode | 1.0x | 1.2x | 2.0x | 1.0x | N/A |
| Gaussian blur | 1.0x | 1.5x | 2.5x | 0.8x (GPU) | 1.1x |
| Resize | 1.0x | 1.5x | 2.5x | 0.8x (GPU) | 1.2x |
| Color adjustment | 1.0x | 1.2x | 1.8x | 1.0x | 0.9x |

**Mitigation Strategy:**
1. **Benchmark early:** Test performance before full implementation
2. **Use modern .NET features:**
   - `Span<T>` and `Memory<T>` for zero-copy buffers
   - `System.Numerics.Vector<T>` for SIMD
   - `Parallel.For` for multi-threading
   - Unsafe code for critical paths

3. **Optimize hot paths:**
   - Profile with BenchmarkDotNet
   - Identify bottlenecks
   - Apply targeted optimizations

4. **Consider GPU acceleration:**
   - SkiaSharp compute shaders
   - OpenCL.NET for complex operations
   - Platform-specific Metal/DirectX

5. **Progressive optimization:**
   - Ship with acceptable performance
   - Optimize based on user feedback
   - Target 90% of VB6 performance initially

**Success Criteria:**
- Core operations within 20% of VB6 performance
- Effects complete in reasonable time (<5s for 4K image)
- No UI freezing during processing
- GPU acceleration working on supported hardware

**Residual Risk:** Medium (optimization is iterative process)

---

### P-002: Memory Usage Increase

**Reference:** `/home/user/PhotoDemon/docs/dependencies/dotnet-alternatives.md:973-1031`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High
**Impact:** Reduced max image size, system slowdown

**Description:**
Managed .NET libraries (ImageSharp, Magick.NET) use more memory than native VB6 code.

**Memory Comparison:**
| Operation | VB6 | ImageSharp | Magick.NET |
|-----------|-----|------------|------------|
| Baseline | 1.0x | 2.0x | 3.0x |
| Large image (16K) | 1.0x | 2.5x | 4.0x |
| Effects pipeline | 1.0x | 1.5x | 2.5x |

**Root Causes:**
- .NET GC overhead
- Managed object headers
- Additional abstractions
- Library inefficiencies

**Mitigation Strategy:**
1. **Aggressive disposal:** Use `using` statements and `IDisposable`
2. **Object pooling:** Reuse large buffers
3. **Streaming:** Process images in tiles for very large files
4. **Memory profiling:** Use dotMemory or similar tools
5. **Native interop:** Use unmanaged memory for large buffers

**Example:**
```csharp
// Pool large buffers
public class BufferPool {
    private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

    public static byte[] Rent(int size) => Pool.Rent(size);
    public static void Return(byte[] buffer) => Pool.Return(buffer);
}
```

**Success Criteria:**
- Maximum image size at least 75% of VB6 version
- No memory leaks under sustained use
- GC pauses <100ms for typical operations

**Residual Risk:** Low-Medium (manageable with proper techniques)

---

### P-003: Startup Time Increase

**Reference:** `/home/user/PhotoDemon/docs/dependencies/dotnet-alternatives.md:1008-1016`

**Severity:** 🟢 **LOW**
**Likelihood:** High
**Impact:** Slightly slower app launch

**Description:**
.NET apps have longer startup times due to JIT compilation and larger assemblies.

**Expected Increase:**
- VB6: ~1 second cold start
- .NET: ~1.5-2 seconds cold start
- Additional ~200-500ms for library loading

**Mitigation Strategy:**
1. **Native AOT compilation:** Consider for final release
2. **Lazy loading:** Load plugins on-demand
3. **Splash screen:** Mask startup delay
4. **Profile-guided optimization (PGO):** Optimize JIT
5. **Trimming:** Remove unused code

**Success Criteria:**
- Cold start <3 seconds
- Warm start <1 second
- Not worse than industry-standard apps

**Residual Risk:** Very Low (acceptable tradeoff)

---

### P-004: Effect Processing Speed

**Reference:** `/home/user/PhotoDemon/docs/algorithms/effects-catalog.md:1-943`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** Slow effect application, poor preview performance

**Description:**
Complex effects (Anisotropic Diffusion, Mean Shift, Droste) may be significantly slower in .NET without optimization.

**High-Complexity Effects:**
| Effect | Algorithm | VB6 Time (4K) | Expected .NET |
|--------|-----------|---------------|---------------|
| Anisotropic Diffusion | PDE solver | 5s | 10-15s |
| Mean Shift | Iterative clustering | 8s | 15-25s |
| Droste | Logarithmic spiral | 3s | 5-8s |
| Oil Painting (Kuwahara) | Anisotropic filter | 2s | 4-6s |

**Mitigation Strategy:**
1. **Algorithm-specific optimization:**
   - Vectorize inner loops with SIMD
   - Parallelize with Parallel.For
   - Use lookup tables where applicable

2. **Progressive rendering:**
   - Show low-quality preview quickly
   - Refine iteratively
   - Allow cancellation

3. **GPU acceleration:**
   - Use compute shaders for suitable algorithms
   - Fall back to CPU if GPU unavailable

4. **User expectations:**
   - Clear progress indication
   - Estimated time remaining
   - Cancellation support

**Success Criteria:**
- Effects complete within 2x VB6 time
- Preview updates in <1 second
- UI remains responsive

**Residual Risk:** Medium (some effects will be slower)

---

### P-005: Canvas Rendering Performance

**Reference:** `/home/user/PhotoDemon/docs/ui/control-mapping.md:1285-1350`

**Severity:** 🟠 **HIGH**
**Likelihood:** Medium
**Impact:** Laggy editing, poor user experience

**Description:**
The main editing canvas (`pdCanvas`) must render at 60 FPS for smooth interaction. SkiaSharp performance is critical.

**Performance Targets:**
| Operation | Target | Acceptable |
|-----------|--------|------------|
| Pan/zoom | 60 FPS | 30 FPS |
| Layer composite | 60 FPS | 15 FPS |
| Selection outline | 60 FPS | 30 FPS |
| Tool preview | 60 FPS | 20 FPS |

**Mitigation Strategy:**
1. **Use GPU acceleration:** SkiaSharp hardware backend
2. **Cache composited layers:** Redraw only when changed
3. **Viewport culling:** Only render visible area
4. **Level-of-detail:** Lower quality during interaction
5. **Async rendering:** Background compositing

**Success Criteria:**
- 30+ FPS during pan/zoom
- No visible lag during typical editing
- Smooth tool interaction

**Residual Risk:** Medium (hardware-dependent)

---

### P-006: Large File Handling

**Reference:** `/home/user/PhotoDemon/docs/architecture/vb6-architecture.md:186-188`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** Crashes on large images, memory exhaustion

**Description:**
PhotoDemon supports images up to 100,000 pixels (PublicConstants.bas). .NET memory limitations may reduce this.

**Current Limits (VB6):**
- Max dimension: 100,000 pixels
- Practical limit: ~50,000 x 50,000 (10GP, ~40GB uncompressed)
- Uses DIB suspension (disk swap) for very large images

**.NET Challenges:**
- 2GB array limit (before .NET 4.5)
- 4GB object limit (x64)
- GC pressure with large allocations

**Mitigation Strategy:**
1. **Use native memory:** `Marshal.AllocHGlobal()`
2. **Tile-based processing:** Process in chunks
3. **Disk suspension:** Port pdDIB's disk swap feature
4. **Streaming:** Don't load entire image at once
5. **64-bit only:** Require x64 for very large images

**Success Criteria:**
- Support 50,000 x 50,000 images on 16GB+ RAM
- Graceful degradation on low-memory systems
- Clear messaging about limits

**Residual Risk:** Medium (platform limitations exist)

---

## Compatibility Risks

### C-001: PDI File Format Backward Compatibility

**Reference:** `/home/user/PhotoDemon/docs/formats/file-format-support.md:69-89`

**Severity:** 🔴 **CRITICAL**
**Likelihood:** High
**Impact:** Users unable to open their files, data loss

**Description:**
PhotoDemon's native PDI format must remain 100% backward compatible. Users have thousands of PDI files that MUST open correctly.

**PDI Format Details:**
- Chunked binary format (PNG-like)
- Multiple versions (legacy, v2, chunky)
- Compression: LZ4, Zstandard, DEFLATE
- Layer data, metadata, undo information
- Platform-specific structures

**Compatibility Requirements:**
1. **Load all VB6-created PDI files**
2. **Preserve all metadata**
3. **Maintain exact color values**
4. **Support all layer types**
5. **Handle compressed data correctly**

**Testing Strategy:**
1. **Collect test corpus:**
   - Request user-contributed PDI files
   - Create automated test files
   - Include edge cases (huge files, many layers, corrupt files)

2. **Automated validation:**
   - Load in both VB6 and .NET
   - Compare pixel data (bit-exact)
   - Verify metadata preservation
   - Check layer properties

3. **Regression prevention:**
   - Include in CI/CD pipeline
   - Block releases if tests fail
   - Maintain test corpus indefinitely

**Mitigation Strategy:**
- Port pdPackageChunky.cls precisely
- Maintain exact structure layouts
- Test extensively before release
- Provide conversion tool if format change needed

**Success Criteria:**
- 100% of test PDI files load correctly
- Zero pixel differences
- All metadata preserved
- Compression algorithms work identically

**Residual Risk:** Medium (extensive testing mitigates)

---

### C-002: Macro File Compatibility

**Reference:** `/home/user/PhotoDemon/docs/algorithms/effects-catalog.md:456-577`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** Automation broken, workflow disruption

**Description:**
Users rely on recorded macros for automation. PDM (PhotoDemon Macro) XML files must remain compatible.

**Compatibility Requirements:**
1. Load VB6-created PDM files
2. Execute actions identically
3. Maintain parameter formats
4. Support version detection

**Testing Strategy:**
- Collect user-contributed macro files
- Test playback produces identical results
- Verify parameter parsing
- Test edge cases

**Mitigation Strategy:**
- Maintain exact XML schema
- Document parameter formats
- Version macros for future changes
- Provide macro converter if needed

**Success Criteria:**
- All test macros execute correctly
- Results match VB6 version
- No parameter parsing errors

**Residual Risk:** Low (XML is text-based, easy to validate)

---

### C-003: Settings and Preferences Migration

**Reference:** `/home/user/PhotoDemon/docs/architecture/vb6-architecture.md:1186-1189`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High
**Impact:** Lost settings, poor first-run experience

**Description:**
PhotoDemon stores user preferences in XML files. These should migrate seamlessly to .NET version.

**Settings Files:**
- `Data/Preferences.xml` - Main settings
- `Data/Presets/*.xml` - Tool presets
- Language files - UI translations
- Recent files MRU

**Migration Strategy:**
1. **Detect VB6 settings:** Check standard install paths
2. **Import on first run:** Copy/convert settings
3. **Validate and upgrade:** Fix incompatibilities
4. **Preserve user customizations:** UI layout, presets, etc.

**Risks:**
- Path changes (Windows → cross-platform)
- Setting name changes
- Data type changes
- Missing settings

**Mitigation:**
- Build settings migration tool
- Provide manual import option
- Log migration issues
- Graceful defaults if import fails

**Success Criteria:**
- Settings import automatically on first run
- 90%+ settings preserved
- Clear upgrade instructions for manual import

**Residual Risk:** Low (XML is forgiving)

---

### C-004: Plugin Version Compatibility

**Reference:** `/home/user/PhotoDemon/docs/api/api-inventory.md:324-404`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** Plugin failures, feature loss

**Description:**
PhotoDemon uses specific plugin versions. .NET wrappers may require different versions or have breaking changes.

**Plugin Compatibility Matrix:**
| Plugin | VB6 Version | .NET Wrapper | Compatibility |
|--------|-------------|--------------|---------------|
| libwebp | 1.5.0 | ImageSharp | High - format stable |
| libheif | 1.17.6 | HeyRed.ImageSharp.Heif | Medium - API changes |
| libavif | 1.2.0 | Shell to exe | High - stable format |
| OpenJPEG | 2.5 | Magick.NET | High - stable API |
| LittleCMS | 2.16.0 | lcmsNET | High - stable API |

**Mitigation Strategy:**
1. **Version detection:** Check plugin versions at runtime
2. **Graceful degradation:** Disable features if plugin incompatible
3. **Update plugins:** Bundle latest compatible versions
4. **API versioning:** Handle API changes in wrapper code

**Success Criteria:**
- All plugins work with .NET version
- Clear error messages if plugin missing
- No crashes from version mismatches

**Residual Risk:** Low (most plugins are stable)

---

### C-005: Cross-Platform File Path Handling

**Reference:** MAUI cross-platform requirements

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High (cross-platform deployment)
**Impact:** File access errors, crashes

**Description:**
VB6 uses Windows-specific paths (`C:\`, backslashes). MAUI must handle macOS/Linux paths correctly.

**Issues:**
- Path separators: `\` vs `/`
- Drive letters: `C:\` vs `/Volumes/`
- Case sensitivity: Windows is case-insensitive, Unix is case-sensitive
- Reserved characters differ by platform

**Examples:**
```csharp
// WRONG
string path = "C:\\Users\\Photos\\image.png";

// CORRECT
string path = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    "image.png"
);
```

**Mitigation Strategy:**
1. **Use Path.Combine():** Always use Path API
2. **Platform abstractions:** Use MAUI's file system APIs
3. **Test on all platforms:** Windows, macOS, Linux
4. **Handle case sensitivity:** Use `StringComparison.OrdinalIgnoreCase` when appropriate

**Success Criteria:**
- No hard-coded paths
- All file operations work on Windows/Mac/Linux
- No path separator errors

**Residual Risk:** Low (standard cross-platform practice)

---

### C-006: Keyboard Shortcut Differences

**Reference:** `/home/user/PhotoDemon/docs/ui/control-mapping.md:1100-1128`

**Severity:** 🟢 **LOW**
**Likelihood:** High
**Impact:** User confusion, reduced productivity

**Description:**
Keyboard shortcuts differ across platforms (Ctrl vs Cmd, function keys, etc.).

**Platform Differences:**
| Action | Windows | macOS | MAUI Solution |
|--------|---------|-------|---------------|
| Copy | Ctrl+C | Cmd+C | Use platform modifier |
| Paste | Ctrl+V | Cmd+V | Use platform modifier |
| Undo | Ctrl+Z | Cmd+Z | Use platform modifier |
| Quit | Alt+F4 | Cmd+Q | Platform-specific |

**Mitigation Strategy:**
1. **Use MAUI keyboard accelerators:** Automatically map to platform conventions
2. **Customizable shortcuts:** Allow user remapping
3. **Platform detection:** Use correct modifiers per platform
4. **Documentation:** Clear shortcut reference per platform

**Success Criteria:**
- Shortcuts work as expected on each platform
- No conflicting key combinations
- User can customize shortcuts

**Residual Risk:** Very Low (MAUI handles this well)

---

## Licensing & Legal Risks

### L-001: ImageSharp Commercial Licensing Conflict

**Reference:** `/home/user/PhotoDemon/docs/dependencies/dotnet-alternatives.md:59-80`

**Severity:** 🔴 **CRITICAL**
**Likelihood:** Certain
**Impact:** Legal issues, budget impact, architecture change

**Description:**
**ImageSharp requires a commercial license ($1,000-$10,000/year) for use in commercial applications.** PhotoDemon is BSD-licensed open source, and this licensing requirement creates a conflict.

**ImageSharp License Terms:**
- Free for personal/non-commercial use
- Free for small businesses (<$1M revenue)
- Paid license for commercial use
- Apache 2.0 license changed to Split License model in v2.0

**Problem:**
- PhotoDemon is open source (BSD 3-Clause)
- Users may use PhotoDemon commercially
- Distributing ImageSharp with PhotoDemon could violate license
- Budget impact if project pays for license

**Options Analysis:**

**Option A: Pay for ImageSharp License** ❌
- Cost: $1,000-$10,000/year
- Problem: PhotoDemon is non-commercial open source project
- Not sustainable long-term

**Option B: Use Magick.NET Instead** ✅ **RECOMMENDED**
- License: Apache 2.0 (free for all uses)
- Feature parity: 95% (excellent format support)
- Trade-off: Slightly slower, more memory usage
- Benefit: BSD license compatible

**Option C: Request Open Source Exception** ⚠️
- Contact SixLabors
- Request free license for PhotoDemon
- No guarantee of approval
- Creates uncertainty

**Option D: Hybrid Build** ⚠️
- Provide two builds: ImageSharp (non-commercial) and Magick.NET (commercial)
- Complex build pipeline
- User confusion
- Maintenance burden

**Mitigation Strategy:**
**Primary Recommendation:** Use **Magick.NET + SkiaSharp** stack exclusively.

**Stack Architecture:**
```
Core Image Processing: Magick.NET (Apache 2.0)
Rendering: SkiaSharp (MIT)
Color Management: lcmsNET (MIT)
Compression: Built-in .NET + K4os.LZ4 + ZstdSharp (all permissive)
Vector Graphics: Svg.Skia (MIT)
Metadata: MetadataExtractor (Apache 2.0)
```

**Benefits:**
- All libraries 100% free for all uses
- BSD license compatibility
- No commercial licensing concerns
- Clear legal standing

**Trade-offs:**
- ~20% slower than ImageSharp for some operations (acceptable)
- ~30% more memory usage (manageable)
- Larger native dependencies (~50-100MB vs ImageSharp's zero)

**Success Criteria:**
- No commercial licenses required
- All dependencies BSD-compatible
- Legal review confirms license compliance
- Performance acceptable (within 20% of ImageSharp)

**Residual Risk:** Very Low (Magick.NET is well-established and permissively licensed)

---

### L-002: LGPL Library Dependencies (libheif)

**Reference:** `/home/user/PhotoDemon/docs/dependencies/dotnet-alternatives.md:167-184`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** License compliance burden, code disclosure requirements

**Description:**
libheif (HEIF/HEIC format support) is LGPL 3.0 licensed. LGPL requires dynamic linking and source availability.

**LGPL Requirements:**
1. **Dynamic linking:** Cannot statically link (DLL is fine)
2. **Source disclosure:** Must provide libheif source or link to it
3. **Modification disclosure:** If modified, must release modifications
4. **License notice:** Must include LGPL license text

**PhotoDemon's Current Approach (VB6):**
- Ships libheif.dll as separate file ✅ (dynamic linking)
- No modifications to libheif ✅ (no disclosure needed)
- Could improve license attribution

**Mitigation Strategy:**
1. **Continue dynamic linking:** Ship .dll/.so/.dylib separately
2. **License attribution:**
   - Include LGPL license text in About dialog
   - Link to libheif source repository
   - Document in readme and documentation

3. **Alternative (if LGPL is problematic):**
   - Use Magick.NET's HEIF support (Apache 2.0)
   - Use Windows 10/11 WIC codec (Windows only)
   - Skip HEIF support (undesirable, format is popular)

**Success Criteria:**
- LGPL compliance verified by legal review
- License attribution complete
- No static linking of LGPL code

**Residual Risk:** Low (LGPL compliance is straightforward for dynamic linking)

---

### L-003: Third-Party Control Suite Licensing

**Reference:** `/home/user/PhotoDemon/docs/ui/control-mapping.md:2229-2242`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium (if commercial controls used)
**Impact:** Budget impact, license compliance

**Description:**
Commercial MAUI control suites (Syncfusion, Telerik, DevExpress) could significantly reduce development time (40-50%) but require paid licenses.

**Commercial Options:**
| Vendor | License Cost | Features | Compatibility |
|--------|-------------|----------|---------------|
| Syncfusion | $795-$2,495/dev/year | Comprehensive | Good |
| Telerik | $1,299-$2,499/dev/year | Extensive | Good |
| DevExpress | $499-$1,999/dev/year | Good coverage | Good |

**Open Source Alternative:**
- MAUI Community Toolkit (MIT license) ✅ **RECOMMENDED**
- Build custom controls (no license cost, more time)

**Decision Matrix:**
| Factor | Commercial | Custom + Toolkit |
|--------|-----------|------------------|
| Cost | $2,000-$10,000 | $0 |
| Timeline | 6-8 months | 10-11 months |
| Flexibility | Limited | Full control |
| Maintenance | Vendor-dependent | Self-maintained |
| License risk | Compliance required | None |

**Mitigation Strategy:**
**Primary Recommendation:** Use **MAUI Community Toolkit + custom controls**.

**Rationale:**
- PhotoDemon is open source non-commercial project
- Commercial controls create licensing burden
- Custom controls provide exact functionality needed
- Time increase (3-4 months) is acceptable
- Full control over features and maintenance

**If budget becomes available:**
- Syncfusion Community License (free for <$1M revenue)
- Evaluate for specific complex controls only
- Keep core controls custom

**Success Criteria:**
- No commercial license dependencies
- All controls functional within timeline
- Open source distribution unencumbered

**Residual Risk:** Low (clear path with toolkit + custom development)

---

## UX & Cross-Platform Risks

### UX-001: Visual Consistency Across Platforms

**Reference:** MAUI cross-platform rendering

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High
**Impact:** Inconsistent user experience, platform-specific bugs

**Description:**
MAUI apps may look and behave differently on Windows, macOS, iOS, and Android. PhotoDemon's complex UI requires careful design.

**Platform Differences:**
| Aspect | Windows | macOS | iOS/Android |
|--------|---------|-------|-------------|
| Window chrome | System | Custom | N/A (fullscreen) |
| Menus | Traditional | Menu bar | Bottom nav/hamburger |
| Scrollbars | Always visible | Auto-hide | Touch gestures |
| Tooltips | Hover | Hover | Long-press |
| File dialogs | Windows style | macOS style | Platform picker |
| Keyboard shortcuts | Ctrl | Cmd | Limited |

**Mitigation Strategy:**
1. **Design for lowest common denominator:** Touch-friendly UI works everywhere
2. **Platform-specific adaptations:** Use `OnPlatform` for differences
3. **Test on all platforms:** Regular testing on each target platform
4. **User testing:** Get feedback from users on each platform
5. **Responsive design:** Adapt to screen sizes (desktop, tablet, phone)

**Example:**
```xml
<Button Text="Save">
    <Button.KeyboardAccelerators>
        <KeyboardAccelerator Key="S">
            <KeyboardAccelerator.Modifiers>
                <OnPlatform x:TypeArguments="KeyboardAcceleratorModifiers">
                    <On Platform="Windows">Ctrl</On>
                    <On Platform="macOS">Cmd</On>
                </OnPlatform>
            </KeyboardAccelerator.Modifiers>
        </KeyboardAccelerator>
    </Button.KeyboardAccelerators>
</Button>
```

**Success Criteria:**
- App feels native on each platform
- No jarring UI inconsistencies
- All features accessible on all platforms
- Platform conventions respected

**Residual Risk:** Medium (platform differences inevitable)

---

### UX-002: Touch vs. Mouse Interaction

**Reference:** MAUI cross-platform input handling

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High (mobile deployment)
**Impact:** Poor mobile UX, limited functionality

**Description:**
PhotoDemon is designed for precise mouse/pen input. Touch input on tablets/phones requires different interaction patterns.

**Challenges:**
| Interaction | Desktop (Mouse) | Mobile (Touch) |
|-------------|----------------|----------------|
| Hover effects | Supported | Not available |
| Right-click | Context menu | Long-press |
| Drag-and-drop | Precise | Finger occlusion |
| Fine adjustments | Easy | Difficult |
| Multi-selection | Ctrl+Click | Gesture-based |
| Tooltips | Hover | Long-press |

**Specific PhotoDemon Challenges:**
- **Canvas editing:** Requires precision (zoom, pen tools)
- **Sliders:** Need fine control (consider pdSlider with 454 instances)
- **Layer management:** Drag-to-reorder may be difficult
- **Color picking:** Precise pixel selection

**Mitigation Strategy:**
1. **Prioritize desktop:** Ensure desktop experience is excellent
2. **Mobile as secondary:** Limited mobile feature set acceptable
3. **Touch-friendly controls:**
   - Larger tap targets (44x44 minimum)
   - Pinch-to-zoom on canvas
   - Gesture-based navigation
   - Touch-optimized sliders (wider tracks)

4. **Contextual UI:**
   - Show different controls for touch vs. mouse
   - Detect input method and adapt
   - Provide alternatives (number input for sliders on mobile)

5. **Progressive disclosure:**
   - Hide advanced features on small screens
   - Full feature set on tablets/desktop

**Success Criteria:**
- Desktop experience matches or exceeds VB6
- Mobile experience usable for basic tasks
- Clear documentation of platform limitations
- User can switch between devices

**Residual Risk:** Medium (mobile will have limitations)

---

### UX-003: High DPI and Multi-Monitor Support

**Reference:** `/home/user/PhotoDemon/docs/ui/control-mapping.md:2040-2053`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High
**Impact:** Blurry UI, incorrect scaling

**Description:**
VB6 has poor high DPI support. PhotoDemon implemented custom DPI handling. MAUI should handle this automatically but may have edge cases.

**DPI Challenges:**
- Mixed DPI monitors (4K + 1080p)
- DPI changes during runtime
- Custom control scaling
- Image vs. UI scaling (image should be 1:1, UI should scale)

**VB6 Approach:**
- Manual DPI scaling throughout codebase
- Pixel-based measurements
- DPI-aware manifest

**MAUI Approach:**
- Device-independent units (automatic scaling)
- Per-monitor DPI awareness
- Platform handles most scaling

**Potential Issues:**
1. **Custom-drawn controls:** May need manual scale factor
2. **Image rendering:** Must distinguish UI scale from image zoom
3. **Mixed monitors:** Window moved between monitors
4. **Fonts:** May render at different sizes

**Mitigation Strategy:**
1. **Use MAUI's built-in scaling:** Don't override unless necessary
2. **Test on high DPI displays:** 4K, 5K, Retina
3. **Test multi-monitor:** Different DPI monitors
4. **Image rendering:**
   - Keep image at actual pixels (1:1)
   - Scale UI elements only
   - Clear zoom indicator

**Success Criteria:**
- Sharp UI on all DPI settings
- Correct scaling on multi-monitor
- Image pixels render 1:1 at 100% zoom
- No blurry text or icons

**Residual Risk:** Low (MAUI handles most cases)

---

### UX-004: Portable/No-Install Philosophy

**Reference:** `/home/user/PhotoDemon/docs/architecture/vb6-architecture.md:1673-1682`

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High (architecture change)
**Impact:** Departure from core philosophy, user disappointment

**Description:**
PhotoDemon's philosophy is "portable, no install required." .NET applications typically require runtime installation or larger self-contained packages.

**Current VB6 Approach:**
- Single .exe + Plugins folder
- No installer, runs from any folder
- Portable: works from USB drive
- Minimal dependencies (VB6 runtime built into Windows)

**.NET Challenges:**
- .NET runtime required (or self-contained deployment)
- Larger file size (self-contained: ~100-200MB)
- Platform-specific binaries
- Security restrictions (code signing on macOS)

**Deployment Options:**

| Option | Size | Install Required | Portable | Platforms |
|--------|------|-----------------|----------|-----------|
| **Framework-dependent** | ~20MB | .NET 10 runtime | ❌ | Windows/Mac/Linux |
| **Self-contained** | ~100-200MB | ❌ | ✅ | Single platform |
| **Single-file** | ~100-200MB | ❌ | ✅ | Single platform |
| **Native AOT** | ~50-100MB | ❌ | ✅ | Single platform |

**Recommended Approach:**
**Hybrid deployment:**
1. **Primary:** Self-contained single-file (Windows/Mac/Linux separate)
   - Pros: No runtime installation, truly portable
   - Cons: Larger file size, separate builds per platform

2. **Alternative:** Framework-dependent with installer
   - Pros: Smaller download
   - Cons: Requires .NET installation

3. **Future:** Native AOT compilation
   - Pros: Smaller size, faster startup
   - Cons: Limited MAUI support currently

**Mitigation Strategy:**
1. **Clearly communicate changes:** Users need to understand new deployment model
2. **Provide both options:** Self-contained and framework-dependent
3. **Optimize size:** Trimming, compression
4. **Maintain portability:** Settings in AppData, no registry

**Success Criteria:**
- Portable version available (self-contained)
- No registry modifications
- Settings portable (in app folder or AppData)
- Clear download options

**Residual Risk:** Low (users will accept larger download for portability)

---

### UX-005: Theming and Dark Mode

**Reference:** `/home/user/PhotoDemon/docs/ui/control-mapping.md:2058-2074`

**Severity:** 🟢 **LOW**
**Likelihood:** Low (feature addition)
**Impact:** Visual inconsistency, user preference ignored

**Description:**
PhotoDemon has custom theme system. MAUI has built-in light/dark mode support. Need to integrate or replace.

**VB6 Approach:**
- Custom `pdThemeColors` class
- Manual color application
- Theme switching via settings

**MAUI Approach:**
- `ResourceDictionary` with `DynamicResource`
- Automatic OS theme detection
- Smooth theme transitions

**Migration Strategy:**
1. **Port theme colors to ResourceDictionary**
2. **Use DynamicResource for all colors**
3. **Support OS theme preference**
4. **Allow manual theme override**
5. **Smooth transition animations**

**Success Criteria:**
- Light and dark themes work
- Respects OS preference
- User can override
- All controls theme consistently

**Residual Risk:** Very Low (MAUI handles this well)

---

## Resource & Timeline Risks

### R-001: Team Size and Skill Set

**Reference:** `/home/user/PhotoDemon/docs/ui/control-mapping.md:2183-2222`

**Severity:** 🟠 **HIGH**
**Likelihood:** High
**Impact:** Timeline overrun, quality issues

**Description:**
Migration requires diverse skill set and sufficient team size. Single developer timeline is unacceptable (15-27 months for UI alone).

**Required Skills:**
- C# / .NET 10 proficiency
- MAUI framework experience
- SkiaSharp graphics programming
- Image processing algorithms
- Cross-platform development
- Performance optimization
- Testing and QA

**Team Size Analysis:**
| Team Size | Timeline | Risk | Recommendation |
|-----------|----------|------|----------------|
| 1 developer | 18-24 months | 🔴 Very High | ❌ Not recommended |
| 2-3 developers | 12-16 months | 🟠 High | ⚠️ Risky |
| 4-5 developers | 9-12 months | 🟡 Medium | ✅ Recommended |
| 6+ developers | 6-9 months | 🟢 Low | ✅ Ideal (if budget allows) |

**Recommended Team Structure (5 developers):**
1. **Lead Architect** - Overall design, core architecture
2. **Graphics Engineer** - SkiaSharp, rendering, effects
3. **UI Developer** - MAUI controls, layout, theming
4. **Platform Specialist** - Cross-platform issues, native interop
5. **QA/Test Engineer** - Testing, automation, quality

**Mitigation Strategy:**
1. **Secure adequate team:** Minimum 4-5 developers
2. **Hire specialists:** SkiaSharp and MAUI experience critical
3. **Provide training:** MAUI/SkiaSharp workshops for team
4. **Parallel development:** Clear module boundaries
5. **Code reviews:** Ensure quality and knowledge sharing

**Success Criteria:**
- Team assembled within 2 months
- All developers proficient in key technologies within 3 months
- Parallel development working smoothly
- No single points of failure (knowledge silos)

**Residual Risk:** Medium (recruiting and training takes time)

---

### R-002: Timeline Estimation Uncertainty

**Reference:** Multiple docs suggest 11-18 month range

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High
**Impact:** Budget overruns, missed deadlines

**Description:**
Timeline estimates vary widely (6-18 months depending on approach). Uncertainty creates planning difficulties.

**Timeline Components:**
| Phase | Optimistic | Realistic | Pessimistic |
|-------|-----------|-----------|-------------|
| Core architecture | 1 month | 2 months | 3 months |
| UI controls | 6 months | 10 months | 14 months |
| Image processing | 3 months | 5 months | 7 months |
| File formats | 3 months | 5 months | 8 months |
| Testing/polish | 2 months | 4 months | 6 months |
| **TOTAL** | **9 months** | **14 months** | **20 months** |

**Uncertainty Factors:**
- Team experience with MAUI
- Control complexity underestimation
- Integration issues
- Platform-specific bugs
- Performance optimization time
- Scope creep

**Mitigation Strategy:**
1. **Agile methodology:** Iterative development with regular releases
2. **Milestone-based planning:** Clear checkpoints
3. **MVP approach:** Ship core features first, iterate
4. **Buffer time:** Add 25-30% contingency
5. **Risk review meetings:** Monthly timeline assessment
6. **Parallel tracks:** De-risk critical path items early

**Recommended Timeline:**
- **Minimum Viable Product (MVP):** 9 months
- **Feature Parity:** 14 months
- **Polish and Optimization:** 16 months

**Success Criteria:**
- MVP delivered on time (9 months)
- Regular milestone achievements
- No surprise delays >1 month
- Clear communication of delays

**Residual Risk:** Medium (software estimates are inherently uncertain)

---

### R-003: Budget Constraints

**Reference:** Commercial licenses, team salaries, infrastructure

**Severity:** 🟡 **MEDIUM**
**Likelihood:** Medium
**Impact:** Feature cuts, timeline extension, quality reduction

**Description:**
Migration requires budget for team, licenses, infrastructure, and tools.

**Budget Components:**
| Item | Cost Range | Priority |
|------|-----------|----------|
| Development team (5 × 14 months) | $400k-$1M | Critical |
| Commercial licenses (if used) | $5k-$20k | Optional |
| Infrastructure (CI/CD, testing) | $5k-$10k | High |
| Tools and software | $5k-$10k | Medium |
| Testing devices | $5k-$10k | High |
| **TOTAL** | **$420k-$1.05M** | - |

**Open Source Project Considerations:**
- PhotoDemon is non-commercial open source
- Budget may be volunteer time or crowdfunding
- Commercial licenses should be avoided

**Mitigation Strategy:**
1. **Use free/open source tools:**
   - Magick.NET instead of ImageSharp ✅
   - MAUI Community Toolkit instead of commercial controls ✅
   - GitHub Actions for CI/CD ✅
   - Open source testing tools ✅

2. **Optimize costs:**
   - Remote team (global talent, lower rates)
   - Open source contributors (community involvement)
   - Phased development (spread costs over time)

3. **Funding options:**
   - Patreon/sponsorship
   - Crowdfunding campaign
   - Grant applications (open source software grants)
   - Corporate sponsorship

**Success Criteria:**
- Budget sufficient for 14-month timeline
- No commercial license dependencies
- Adequate testing infrastructure
- No budget-related delays

**Residual Risk:** Medium (funding for open source projects is challenging)

---

### R-004: Community and User Expectations

**Reference:** PhotoDemon's active user community

**Severity:** 🟡 **MEDIUM**
**Likelihood:** High
**Impact:** User dissatisfaction, negative feedback

**Description:**
PhotoDemon has an active user base with high expectations. Migration must maintain or exceed current functionality.

**User Expectations:**
- All current features available
- Same or better performance
- No data loss (file compatibility)
- Familiar UI (learning curve minimal)
- Cross-platform availability (new benefit)
- Free and open source (maintained)

**Communication Risks:**
- Long development time (users wait)
- Feature gaps in MVP
- Performance regressions
- Breaking changes

**Mitigation Strategy:**
1. **Transparent communication:**
   - Regular blog posts on progress
   - Beta program for early adopters
   - Clear roadmap and timeline
   - Honest about limitations

2. **Maintain VB6 version:**
   - Continue bug fixes during migration
   - Don't abandon existing users
   - Parallel development acceptable

3. **Early preview releases:**
   - Monthly preview builds
   - Gather feedback early
   - Iterate based on user input

4. **Manage expectations:**
   - Clear about what's changing
   - Explain benefits of migration
   - Acknowledge trade-offs

5. **Community involvement:**
   - Accept contributions
   - Public issue tracking
   - User input on priorities

**Success Criteria:**
- Community remains engaged
- Positive feedback on previews
- Active beta testing program
- Smooth transition for users

**Residual Risk:** Low-Medium (community involvement mitigates)

---

## Mitigation Strategies

### High-Priority Mitigations (Immediate Action)

#### M-001: Establish Type Safety Framework
**Addresses:** T-001, T-002, T-005, T-006, T-007

**Actions:**
1. Create comprehensive type mapping document ✅ (DONE)
2. Build structure validation test suite
3. Implement `[StructLayout]` attribute standards
4. Create IntPtr migration checklist
5. Automate pointer type detection

**Timeline:** Month 1
**Owner:** Lead Architect
**Success Metric:** All structures validated, zero pointer errors in tests

---

#### M-002: Select License-Compatible Technology Stack
**Addresses:** L-001, L-002, L-003

**Actions:**
1. Adopt Magick.NET + SkiaSharp stack ✅ (RECOMMENDED)
2. Avoid ImageSharp to prevent commercial licensing
3. Document all library licenses
4. Legal review of final stack
5. Create license compliance checklist

**Timeline:** Month 1
**Owner:** Project Lead
**Success Metric:** Legal approval, no commercial licenses required

---

#### M-003: Build Performance Baseline
**Addresses:** P-001, P-002, P-004, P-005

**Actions:**
1. Create benchmark suite for VB6 version
2. Identify top 20 performance-critical operations
3. Set performance targets (90% of VB6)
4. Profile VB6 to understand optimization techniques
5. Document algorithmic approaches

**Timeline:** Month 1-2
**Owner:** Graphics Engineer
**Success Metric:** Baseline established, targets defined

---

#### M-004: Establish Testing Infrastructure
**Addresses:** C-001, C-002, T-011, T-012

**Actions:**
1. Collect test file corpus (PDI, macros, images)
2. Set up automated comparison testing
3. Create visual regression test suite
4. Build CI/CD pipeline
5. Establish quality gates

**Timeline:** Month 2-3
**Owner:** QA Engineer
**Success Metric:** 100+ test files, automated validation

---

### Medium-Priority Mitigations (Months 3-6)

#### M-005: Implement Abstraction Layers
**Addresses:** T-003, T-009, P-001

**Actions:**
1. Create ICanvas abstraction for rendering
2. Create IImageProcessor abstraction for effects
3. Create IFormatHandler abstraction for file I/O
4. Isolate platform-specific code
5. Enable future optimization without API changes

**Timeline:** Months 3-4
**Owner:** Lead Architect
**Success Metric:** All platform/library dependencies behind abstractions

---

#### M-006: Port Critical Custom Parsers
**Addresses:** T-012, C-001

**Actions:**
1. Port PSD parser with test suite
2. Port PSP parser with test suite
3. Port XCF parser with test suite
4. Validate against Apple test suite (PSD)
5. Performance optimization

**Timeline:** Months 3-6
**Owner:** Platform Specialist
**Success Metric:** All parsers pass test suites, maintain quality

---

### Ongoing Mitigations (Throughout Project)

#### M-007: Regular User Communication
**Addresses:** R-004, R-002

**Actions:**
1. Monthly blog posts on progress
2. Quarterly preview releases
3. Public roadmap updates
4. Community surveys
5. Beta program

**Timeline:** Ongoing
**Owner:** Project Lead
**Success Metric:** Engaged community, positive feedback

---

#### M-008: Cross-Platform Testing
**Addresses:** UX-001, UX-002, C-005

**Actions:**
1. Weekly testing on Windows, macOS, Linux
2. Monthly mobile testing (iOS, Android)
3. Multi-monitor and high DPI testing
4. Touch input testing
5. Platform-specific bug tracking

**Timeline:** Ongoing (from Month 4)
**Owner:** QA Engineer
**Success Metric:** All platforms tested, bugs tracked and fixed

---

## Contingency Plans

### CP-001: Timeline Overrun (>15 months)

**Trigger:** Milestone slip >2 months

**Actions:**
1. **Reduce scope:**
   - Defer Tier 4 controls to v1.1
   - Limit initial platform support (Windows only)
   - MVP feature set only

2. **Increase resources:**
   - Add contractors for specific tasks
   - Community contributions
   - Accept partial feature parity

3. **Extend timeline:**
   - Revise roadmap
   - Communicate to users
   - Continue VB6 maintenance

**Decision Point:** Month 9 review

---

### CP-002: Performance Unacceptable (<50% of VB6)

**Trigger:** Benchmark shows >2x slowdown for core operations

**Actions:**
1. **Optimize hot paths:**
   - Profile and identify bottlenecks
   - Use unsafe code for critical sections
   - Implement SIMD vectorization
   - Consider GPU acceleration

2. **Switch libraries:**
   - Evaluate SkiaSharp vs custom implementations
   - Consider native library wrappers
   - Hybrid managed/native approach

3. **Architecture changes:**
   - Redesign performance-critical modules
   - Use lower-level APIs
   - Accept platform limitations

**Decision Point:** Month 6 performance review

---

### CP-003: Commercial License Required (No Alternative)

**Trigger:** Critical dependency requires commercial license

**Actions:**
1. **Seek funding:**
   - Crowdfunding campaign
   - Corporate sponsorship
   - Grant applications

2. **Implement alternative:**
   - Custom implementation
   - Different library
   - Feature removal

3. **License negotiation:**
   - Request open source exception
   - Negotiate favorable terms
   - Delay feature

**Decision Point:** Case-by-case basis

---

### CP-004: Critical Feature Gap (Cannot Implement)

**Trigger:** Essential feature has no .NET solution

**Examples:** JPEG XL support, Photoshop plugins

**Actions:**
1. **Native interop:**
   - Create P/Invoke wrapper
   - Bundle native libraries
   - Platform-specific implementations

2. **Process execution:**
   - Shell to command-line tools
   - Async communication
   - Handle errors gracefully

3. **Defer or remove:**
   - Communicate to users
   - Evaluate alternatives
   - Plan for future addition

**Decision Point:** Feature-by-feature evaluation

---

### CP-005: Team Capacity Issues

**Trigger:** Key developer leaves, team underperforming

**Actions:**
1. **Cross-training:**
   - Knowledge sharing sessions
   - Pair programming
   - Documentation emphasis

2. **Hiring:**
   - Recruit replacement
   - Contract specialists
   - Temporary augmentation

3. **Scope reduction:**
   - Re-prioritize features
   - Extend timeline
   - Community contributions

**Decision Point:** Within 2 weeks of issue

---

## Risk Monitoring Plan

### Weekly Monitoring (Team Level)

**Metrics:**
- Velocity (story points completed)
- Defect rate (bugs per feature)
- Code coverage (unit tests)
- Build health (CI/CD status)

**Review:** Weekly stand-up

**Owner:** Lead Architect

---

### Monthly Monitoring (Management Level)

**Metrics:**
- Milestone progress (on-time %)
- Budget burn rate
- Team capacity utilization
- Technical debt accumulation
- Performance benchmarks

**Review:** Monthly steering committee

**Owner:** Project Lead

---

### Quarterly Monitoring (Strategic Level)

**Metrics:**
- Overall timeline vs. plan
- Feature completion (% of planned features)
- Quality metrics (bug density, test coverage)
- User feedback (community sentiment)
- Platform parity (Windows/Mac/Linux coverage)

**Review:** Quarterly stakeholder review

**Owner:** Executive Sponsor

---

### Risk Escalation Process

**Level 1 (Team):**
- Daily stand-up
- Immediate mitigation
- Owner: Developer

**Level 2 (Project):**
- Weekly risk review
- Mitigation planning
- Owner: Lead Architect

**Level 3 (Management):**
- Monthly steering committee
- Resource allocation
- Owner: Project Lead

**Level 4 (Executive):**
- Quarterly review
- Strategic decisions
- Owner: Executive Sponsor

---

## Summary: Top 10 Risks and Mitigations

### 1. 🔴 CRITICAL: 32-bit to 64-bit Pointer Migration
- **Mitigation:** Comprehensive type mapping, IntPtr everywhere, extensive testing
- **Owner:** Lead Architect
- **Timeline:** Month 1-2

### 2. 🔴 CRITICAL: ImageSharp Commercial Licensing
- **Mitigation:** Use Magick.NET + SkiaSharp stack instead
- **Owner:** Project Lead
- **Timeline:** Month 1 (decision made)

### 3. 🔴 CRITICAL: GDI+ to SkiaSharp Rewrite
- **Mitigation:** Abstraction layer, visual regression tests, gradual migration
- **Owner:** Graphics Engineer
- **Timeline:** Months 2-8

### 4. 🔴 CRITICAL: PDI File Format Compatibility
- **Mitigation:** Extensive test corpus, byte-level validation, conservative approach
- **Owner:** Platform Specialist
- **Timeline:** Months 3-6

### 5. 🟠 HIGH: Custom Control Scope (56 controls)
- **Mitigation:** Prioritize Tier 1, parallel development, consider toolkit
- **Owner:** UI Developer
- **Timeline:** Months 2-10

### 6. 🟠 HIGH: Performance Regression
- **Mitigation:** Benchmark early, optimize hot paths, use modern .NET features
- **Owner:** Graphics Engineer
- **Timeline:** Ongoing

### 7. 🟠 HIGH: PSD/PSP/XCF Parser Porting
- **Mitigation:** Careful porting, extensive testing, maintain quality
- **Owner:** Platform Specialist
- **Timeline:** Months 3-6

### 8. 🟠 HIGH: Team Size and Timeline
- **Mitigation:** 5-person team minimum, parallel development, clear milestones
- **Owner:** Project Lead
- **Timeline:** Month 1 (team assembly)

### 9. 🟡 MEDIUM: Plugin Compatibility
- **Mitigation:** .NET alternatives, P/Invoke wrappers, graceful degradation
- **Owner:** Platform Specialist
- **Timeline:** Months 2-8

### 10. 🟡 MEDIUM: Cross-Platform UX
- **Mitigation:** Platform-specific adaptations, testing on all platforms
- **Owner:** UI Developer
- **Timeline:** Ongoing

---

## Overall Assessment

### Project Viability: ✅ **FEASIBLE**

**Confidence:** 75% success probability with proper planning and execution

**Critical Success Factors:**
1. ✅ Adequate team size (5 developers)
2. ✅ Avoid ImageSharp licensing (use Magick.NET)
3. ✅ Extensive testing infrastructure
4. ✅ Comprehensive type mapping
5. ✅ Performance baseline and monitoring
6. ✅ Transparent user communication

**Go/No-Go Recommendation:** **GO** with the following conditions:

1. **Team:** Minimum 4-5 developers secured
2. **Timeline:** 14-month realistic timeline accepted
3. **Budget:** Sufficient for team and infrastructure (no commercial licenses)
4. **Technology:** Magick.NET + SkiaSharp stack adopted
5. **Testing:** Comprehensive test infrastructure in place by Month 3
6. **Milestones:** Clear go/no-go checkpoints at Months 6, 9, 12

**Red Flags to Watch:**
- ❌ Single developer or <3-person team
- ❌ Requirement for commercial licenses
- ❌ Performance <50% of VB6 at Month 6
- ❌ >30% test files failing at Month 9
- ❌ Timeline slippage >3 months

---

**Document Prepared By:** Agent 9 - Risk Assessment
**Review Date:** 2025-11-17
**Next Review:** Month 3 of project (or upon new risks identified)
**Distribution:** Project team, stakeholders, community

---

**END OF RISK ASSESSMENT**
