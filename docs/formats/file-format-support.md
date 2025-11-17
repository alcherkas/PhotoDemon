# PhotoDemon File Format Support Documentation

**Project:** VB6 to .NET 10 MAUI Migration
**Document Version:** 1.0
**Last Updated:** 2025-11-17
**Author:** Agent 6 - File Format Agent

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Format Support Matrix](#format-support-matrix)
3. [Internal Format Implementations](#internal-format-implementations)
4. [External Plugin Dependencies](#external-plugin-dependencies)
5. [Import/Export Capabilities](#importexport-capabilities)
6. [.NET Library Mapping](#net-library-mapping)
7. [Migration Complexity Assessment](#migration-complexity-assessment)
8. [Code References](#code-references)

---

## Executive Summary

PhotoDemon supports an extensive array of image formats through a combination of:
- **Custom native parsers** (PSD, PSP, XCF, GIF, PNG, ICO, etc.)
- **External plugin libraries** (FreeImage, libwebp, libheif, libavif, libjxl, OpenJPEG, etc.)
- **System libraries** (GDI+ for EMF/WMF)

The application demonstrates exceptional format coverage with **40+ import formats** and **25+ export formats**, including advanced features like:
- Multi-layer support (PSD, PSP, XCF, ORA, PDI)
- Animation support (GIF, PNG/APNG, WebP, JXL)
- HDR/RAW image support (DNG, EXR, HDR, RAW variants)
- Modern codecs (AVIF, HEIF, JXL, WebP)
- Legacy format support (PCX, XBM, WBMP, MBM, etc.)

### Key Statistics

| Metric | Count |
|--------|-------|
| Total Import Formats | 40+ |
| Total Export Formats | 25+ |
| Native Parsers | 15 |
| External Plugins | 15+ |
| Multi-layer Formats | 5 |
| Animated Formats | 4 |

---

## Format Support Matrix

### Modern High-Efficiency Formats

| Format | Extension(s) | Import | Export | Implementation | Color Depth | Animation |
|--------|-------------|--------|--------|----------------|-------------|-----------|
| **AVIF** | .avif, .avifs, .heif, .heic | ✓ | ✓ | libavif (exe) | 8/10-bit | Planned |
| **HEIF** | .heif, .heic, .heifs, .heics, .hif | ✓ | ✓ | libheif | 8/10/12-bit | ✓ |
| **JXL** | .jxl | ✓ | ✓ | libjxl (exe) | 8/16/32-bit | ✓ |
| **WebP** | .webp | ✓ | ✓ | libwebp | 8-bit | ✓ |

**Code References:**
- AVIF: `/home/user/PhotoDemon/Modules/Plugin_AVIF.bas:1-100`
- HEIF: `/home/user/PhotoDemon/Modules/Plugin_heif.bas:1-100`
- JXL: `/home/user/PhotoDemon/Modules/Plugin_jxl.bas:1-100`
- WebP: `/home/user/PhotoDemon/Modules/Plugin_WebP.bas:1-100`

### Professional/Multi-Layer Formats

| Format | Extension(s) | Import | Export | Implementation | Layers | Color Depth |
|--------|-------------|--------|--------|----------------|--------|-------------|
| **PSD** | .psd, .psb | ✓ | ✓ | Native parser (pdPSD) | ✓ | 1-32 bpc |
| **PSP** | .psp, .pspimage, .tub, .msk | ✓ | ✓ | Native parser (pdPSP) | ✓ | 1-48-bit |
| **XCF** | .xcf, .xcfgz, .xcf.gz | ✓ | - | Native parser (pdXCF) | ✓ | 8/16/32-bit int/float |
| **ORA** | .ora | ✓ | ✓ | Native parser (pdOpenRaster) | ✓ | 8-bit |
| **PDI** | .pdi | ✓ | ✓ | Native parser (pdPackageChunky) | ✓ | 32-bit |

**Key Features:**
- **PSD Parser**: Full Apple color management test suite compliance, supports all 9 color modes, 1-32 bits per channel
- **PSP Parser**: Handles versions 5-current, supports groups, vector layers, and composite images
- **XCF Parser**: Comprehensive GIMP format support including all precisions (int/float, 8/16/32/64-bit)
- **ORA Parser**: OpenRaster specification compliance with ZIP-based layer storage

**Code References:**
- PSD: `/home/user/PhotoDemon/Classes/pdPSD.cls:1-200`
- PSP: `/home/user/PhotoDemon/Classes/pdPSP.cls:1-200`
- XCF: `/home/user/PhotoDemon/Classes/pdXCF.cls:1-200`
- ORA: `/home/user/PhotoDemon/Classes/pdOpenRaster.cls`
- PDI: `/home/user/PhotoDemon/Modules/ImageLoader.bas:59-200`

### Standard Web/Raster Formats

| Format | Extension(s) | Import | Export | Implementation | Animation | Notes |
|--------|-------------|--------|--------|----------------|-----------|-------|
| **PNG** | .png, .apng | ✓ | ✓ | Native parser (pdPNG) | ✓ (APNG) | Preferred decoder |
| **JPEG** | .jpg, .jpeg, .jpe, .jif, .jfif | ✓ | ✓ | GDI+ / FreeImage | - | EXIF auto-rotate |
| **GIF** | .gif, .agif | ✓ | ✓ | Native parser (pdGIF) | ✓ | Full animation support |
| **BMP** | .bmp | ✓ | ✓ | Native / GDI+ | - | Windows native |
| **TIFF** | .tif, .tiff | ✓ | ✓ | FreeImage / GDI+ | Multi-page | 1-48 bpc support |
| **ICO** | .ico | ✓ | ✓ | Native parser (pdICO) | - | Multiple sizes |

**Code References:**
- PNG: `/home/user/PhotoDemon/Classes/pdPNG.cls`
- GIF: `/home/user/PhotoDemon/Classes/pdGIF.cls`
- ICO: `/home/user/PhotoDemon/Classes/pdICO.cls`

### JPEG 2000 Family

| Format | Extension(s) | Import | Export | Implementation | Notes |
|--------|-------------|--------|--------|----------------|-------|
| **JPEG-2000** | .jp2, .j2k, .jpt, .j2c, .jpc, .jpx, .jpf, .jph | ✓ | ✓ | OpenJPEG | Multiple container formats |
| **JPEG-LS** | .jls | ✓ | - | CharLS plugin | Lossless JPEG |
| **JPEG XR** | .jxr, .hdp, .wdp | ✓ | ✓ | FreeImage | Microsoft HD Photo |

**Code References:**
- JP2: `/home/user/PhotoDemon/Modules/Plugin_OpenJPEG.bas:1-100`
- JLS: `/home/user/PhotoDemon/Modules/Plugin_CharLS.bas`

### RAW Camera Formats

| Category | Extensions | Import | Export | Implementation |
|----------|-----------|--------|--------|----------------|
| **RAW Images** | .3fr, .arw, .bay, .bmq, .cap, .cine, .cr2, .crw, .cs1, .dc2, .dcr, .dng, .drf, .dsc, .erf, .fff, .ia, .iiq, .k25, .kc2, .kdc, .mdc, .mef, .mos, .mrw, .nef, .nrw, .orf, .pef, .ptx, .pxn, .qtk, .raf, .raw, .rdc, .rw2, .rwz, .sr2, .srf, .sti | ✓ | - (as HDR) | FreeImage |
| **DNG** | .dng | ✓ | - | FreeImage | Adobe Digital Negative |

**Notes:** RAW formats are imported via FreeImage and can be exported as HDR format, which supports similar features. Extensive manufacturer support for Canon, Nikon, Sony, Fuji, Olympus, Pentax, Panasonic, etc.

**Code Reference:** `/home/user/PhotoDemon/Modules/ImageFormats.bas:314`

### HDR/Scientific Formats

| Format | Extension(s) | Import | Export | Implementation | Color Depth |
|--------|-------------|--------|--------|----------------|-------------|
| **HDR/RGBE** | .hdr | ✓ | ✓ | FreeImage | 32-bit float (Radiance) |
| **EXR** | .exr | ✓ | - | FreeImage | 16/32-bit float (ILM) |
| **PFM** | .pfm | ✓ | - | FreeImage | 32-bit float Portable FloatMap |

### Gaming/Texture Formats

| Format | Extension(s) | Import | Export | Implementation | Notes |
|--------|-------------|--------|--------|----------------|-------|
| **DDS** | .dds | ✓ | ✓ | DirectXTex (texconv.exe) | DirectDraw Surface, BC1-7 compression |

**Code Reference:** `/home/user/PhotoDemon/Modules/Plugin_DDS.bas:1-100`

### Legacy/Specialized Formats

| Format | Extension(s) | Import | Export | Implementation | Notes |
|--------|-------------|--------|--------|----------------|-------|
| **PCX/DCX** | .pcx, .pcc, .dcx | ✓ | ✓ | Native parser (pdPCX) | Zsoft Paintbrush |
| **XBM** | .xbm | ✓ | - | Native parser | X Bitmap |
| **WBMP** | .wbmp, .wbm, .wap | ✓ | ✓ | Native parser | Wireless Bitmap |
| **QOI** | .qoi | ✓ | ✓ | Native parser (pdQOI) | Quite OK Image |
| **MBM** | .mbm, .mbw, .mcl, .aif, .abw, .acl | ✓ | - | Native parser (pdMBM) | Symbian Bitmap |
| **TGA** | .tga | ✓ | ✓ | FreeImage | Truevision TARGA |

**Code References:**
- PCX: `/home/user/PhotoDemon/Classes/pdPCX.cls`
- QOI: `/home/user/PhotoDemon/Classes/pdQOI.cls`
- MBM: `/home/user/PhotoDemon/Classes/pdMBM.cls`

### Vector/Document Formats

| Format | Extension(s) | Import | Export | Implementation | Notes |
|--------|-------------|--------|--------|----------------|-------|
| **SVG** | .svg, .svgz | ✓ | - | resvg plugin | Rasterized on import |
| **PDF** | .pdf | ✓ | - | pdfium plugin | Rasterized on import |
| **EMF** | .emf | ✓ | - | GDI+ | Enhanced Metafile |
| **WMF** | .wmf | ✓ | - | GDI+ | Windows Metafile |

**Code References:**
- SVG: `/home/user/PhotoDemon/Modules/Plugin_resvg.bas`
- PDF: `/home/user/PhotoDemon/Modules/Plugin_PDF.bas`

### Archive/Multi-Image Formats

| Format | Extension(s) | Import | Export | Implementation | Notes |
|--------|-------------|--------|--------|----------------|-------|
| **CBZ** | .cbz | ✓ | - | Native parser (pdCBZ) | Comic Book Archive (ZIP) |
| **HGT** | .hgt | ✓ | - | Native parser (pdHGT) | SRTM elevation data |

**Code References:**
- CBZ: `/home/user/PhotoDemon/Classes/pdCBZ.cls`
- HGT: `/home/user/PhotoDemon/Classes/pdHGT.cls`

### Netpbm Family

| Format | Extension(s) | Import | Export | Implementation | Notes |
|--------|-------------|--------|--------|----------------|-------|
| **PNM/PPM/PGM/PBM** | .pnm, .ppm, .pgm, .pbm, .pfm | ✓ | ✓ | FreeImage | Portable anymap/pixmap/graymap/bitmap |

---

## Internal Format Implementations

PhotoDemon includes **15 custom-built format parsers** written in VB6. These parsers provide exceptional performance and feature coverage:

### 1. PSD (Adobe Photoshop) - `/home/user/PhotoDemon/Classes/pdPSD.cls`

**Implementation Highlights:**
- Passes full Apple color management test suite
- Supports all 9 PSD color modes: Bitmap, Grayscale, Indexed, RGB, CMYK, Multichannel, Duotone, Lab
- Handles 1-32 bits per channel
- RLE (PackBits) compression native support
- DEFLATE compression via libdeflate
- Layer groups, masks, adjustment layers
- ICC profile support with full color-managed pipeline
- PSB (Large Document Format) support

**Key Classes:**
- `pdPSD` - Main parser (line 9-200+)
- `pdPSDLayer` - Layer handling
- `pdPSDLayerInfo` - Layer metadata

**Dependencies:**
- `libdeflate` for DEFLATE-compressed data (optional, HDR images)
- `zlib` for ZIP compression (optional)

**Export Capabilities:**
- 32-bpp RGBA output
- Layer preservation
- ICC profile embedding
- Metadata preservation

### 2. PSP (PaintShop Pro) - `/home/user/PhotoDemon/Classes/pdPSP.cls`

**Implementation Highlights:**
- Supports PSP versions 5 through current
- Custom block-based parser with extensive format coverage
- Handles composite images and thumbnails
- Layer groups support (with dummy layer workaround)
- Vector layer data preservation
- ICC profile support
- Compression: RLE, JPEG (for composites)

**Key Classes:**
- `pdPSP` - Main parser (line 9-200+)
- `pdPSPLayer` - Layer objects
- `pdPSPBlock` - Block structure handling
- `pdPSPChannel` - Channel data
- `pdPSPShape` - Vector shapes

**Special Features:**
- Composite image loading (merged preview)
- Tube, frame, and mask file variants
- Version-specific parsing (PSPv5 uses different headers)

### 3. XCF (GIMP) - `/home/user/PhotoDemon/Classes/pdXCF.cls`

**Implementation Highlights:**
- Comprehensive precision support: 8/16/32/64-bit integer and floating-point
- Color modes: RGB, Grayscale, Indexed (with/without alpha)
- Compression: RLE, ZLib
- GZip file variants (.xcfgz, .xcf.gz)
- Layer properties and parasites
- Full tile-based loading system
- Color space handling (linear/gamma)

**Supported Precisions:**
- 8-bit int (linear/gamma)
- 16-bit int (linear/gamma)
- 32-bit int (linear/gamma)
- 16-bit float (linear/gamma)
- 32-bit float (linear/gamma)
- 64-bit float (linear/gamma)

**Code Reference:** Lines 14-200 in `/home/user/PhotoDemon/Classes/pdXCF.cls`

### 4. PNG/APNG - `/home/user/PhotoDemon/Classes/pdPNG.cls`

**Implementation Highlights:**
- Full PNG specification compliance
- APNG (Animated PNG) support
- Multiple decoders available (internal preferred, FreeImage fallback, GDI+ last resort)
- Chunk-based architecture
- Supports all PNG color types and bit depths
- Interlacing support (Adam7)
- Ancillary chunks (tEXt, zTXt, iTXt, etc.)

**Key Class:**
- `pdPNGChunk` - Chunk handling

### 5. GIF - `/home/user/PhotoDemon/Classes/pdGIF.cls`

**Implementation Highlights:**
- Full animation support
- LZW decompression in dedicated module
- Frame timing and disposal methods
- Transparency support
- Comment and metadata extraction

**Related Modules:**
- `/home/user/PhotoDemon/Modules/ImageFormats_GIF.bas`
- `/home/user/PhotoDemon/Modules/ImageFormats_GIF_LZW.bas`

### 6. Other Native Parsers

| Parser | Class File | Key Features |
|--------|-----------|--------------|
| **ICO** | `pdICO.cls` | Multiple icon sizes, transparency, PNG-compressed icons |
| **PCX/DCX** | `pdPCX.cls` | Multi-page DCX, RLE compression, 1-24 bpp |
| **QOI** | `pdQOI.cls` | Fast modern lossless format |
| **WBMP** | Custom code | 1-bit wireless format |
| **XBM** | Custom code | X Window System bitmap |
| **MBM** | `pdMBM.cls` | Symbian multi-bitmap |
| **ORA** | `pdOpenRaster.cls` | ZIP-based layered format |
| **CBZ** | `pdCBZ.cls` | Comic book archive (ZIP with images) |
| **HGT** | `pdHGT.cls` | SRTM elevation data (16-bit signed integer) |
| **PDF** | `pdPDF.cls` | PDF page rasterization via pdfium |

---

## External Plugin Dependencies

PhotoDemon relies on multiple external libraries for advanced format support:

### 1. FreeImage (Legacy Fallback)

**Library:** `FreeImage.dll`
**License:** GPLv3 / FreeImage Public License
**Status:** Legacy fallback (being phased out)
**Code:** `/home/user/PhotoDemon/Modules/Plugin_FreeImage.bas:1-150`

**Supported Formats (when primary parsers unavailable):**
- DNG (Digital Negative)
- EXR (OpenEXR)
- G3 (Fax format)
- HDR (Radiance RGBE)
- IFF (Amiga Interchange)
- JNG (JPEG Network Graphics)
- JXR (JPEG XR / HD Photo)
- KOALA (Commodore 64)
- LBM (Deluxe Paint)
- PBM, PGM, PPM (Netpbm)
- PCD (Kodak PhotoCD)
- PFM (Portable FloatMap)
- PICT (Macintosh Picture)
- RAS (Sun Raster)
- RAW (various camera formats)
- SGI (Silicon Graphics)
- TGA (TARGA)
- XPM (X Pixmap)

**Note:** PhotoDemon is actively reducing FreeImage dependency by implementing native parsers.

### 2. libwebp (Google WebP)

**Libraries:** `libwebp.dll`, `libwebpdemux.dll`, `libwebpmux.dll`, `libsharpyuv.dll`
**Version:** 1.3.2+
**License:** BSD
**Code:** `/home/user/PhotoDemon/Modules/Plugin_WebP.bas:1-100`

**Features:**
- WebP decode/encode
- Animated WebP support
- Lossy and lossless compression
- Alpha channel support
- YUV color space handling

### 3. libheif (HEIF/HEIC)

**Libraries:** `libheif.dll`, `libde265.dll`, `libx265.dll`
**Version:** Latest compatible
**License:** LGPL
**Code:** `/home/user/PhotoDemon/Modules/Plugin_heif.bas:1-100`

**Features:**
- HEIF/HEIC decode/encode (x265 encoder)
- Multi-image containers
- 8/10/12-bit support
- ICC profile support
- HDR support
- Thumbnail extraction

**Encoding Parameters:**
- Quality (0-100)
- Lossless mode
- Preset (ultrafast to placebo)
- Tune (psnr, ssim, grain, fastdecode)
- Chroma subsampling (420, 422, 444)

### 4. libavif (AVIF/AV1 Images)

**Executables:** `avifdec.exe`, `avifenc.exe`
**Version:** 1.2.0
**License:** BSD
**Platform:** 64-bit only
**Code:** `/home/user/PhotoDemon/Modules/Plugin_AVIF.bas:1-100`

**Features:**
- AV1 image encoding/decoding
- Multi-threaded processing
- 8/10/12-bit depth
- HDR support (PQ, HLG)
- Animated AVIF (planned)

**Note:** Uses command-line executables for cross-process encoding/decoding due to x64 limitations.

### 5. libjxl (JPEG XL)

**Executables:** `djxl.exe`, `cjxl.exe`, `jxlinfo.exe`
**Version:** Latest compatible
**License:** BSD
**Platform:** Windows Vista+ (no XP support)
**Code:** `/home/user/PhotoDemon/Modules/Plugin_jxl.bas:1-100`

**Features:**
- JPEG XL decode/encode
- Lossless/lossy compression
- Animation support
- Progressive decoding
- 1-32 bpc support
- HDR support

### 6. OpenJPEG (JPEG 2000)

**Library:** `openjp2.dll`
**Version:** 2.5.4+
**License:** BSD
**Code:** `/home/user/PhotoDemon/Modules/Plugin_OpenJPEG.bas:1-100`

**Implementation Details:**
- Custom-built interface (2025)
- Passes OpenJPEG conformance suite (except one 32-bit crash bug)
- Extensive color space handling
- Supports J2K, JPT, JP2, JPX containers
- Signed/unsigned data type support
- Precision handling beyond standard RGB

**Color Spaces:**
- sRGB, Grayscale
- YUV (SYCC), e-YCC
- CMYK
- Unspecified (inferred by component count)

**Known Issues:**
- JPEG-2000 spec allows "undefined" color space (terrible design)
- PD defaults to RGB for unknown 3/4-channel images

### 7. CharLS (JPEG-LS)

**Plugin:** CharLS library
**License:** BSD
**Code:** `/home/user/PhotoDemon/Modules/Plugin_CharLS.bas`

**Features:**
- Lossless JPEG compression
- Medical imaging support

### 8. DirectXTex (DDS)

**Executable:** `texconv.exe`, `texdiag.exe`
**Version:** October 2024 (last Win7-compatible release)
**License:** MIT
**Platform:** 64-bit only
**Code:** `/home/user/PhotoDemon/Modules/Plugin_DDS.bas:1-100`

**Features:**
- DirectDraw Surface support
- BC1-BC7 compression
- DXT compression
- Mipmap support
- Cube maps
- Texture arrays

**Workarounds:**
- Uses temporary directories for output (no direct filename control)
- File list workaround for spaces in filenames

### 9. ExifTool (Metadata)

**Executable:** `exiftool.exe`
**Version:** Latest
**License:** Perl Artistic License
**Code:** `/home/user/PhotoDemon/Modules/Plugin_ExifTool.bas:1-100`

**Features:**
- Comprehensive metadata reading/writing
- EXIF, IPTC, XMP support
- Asynchronous processing via stdin/stdout pipes
- Portable Perl runtime (auto-extracted)

**Usage Pattern:**
- Started as persistent process
- Metadata streamed via pipes
- Results parsed from XML output

### 10. resvg (SVG Rasterizer)

**Plugin:** resvg library
**Code:** `/home/user/PhotoDemon/Modules/Plugin_resvg.bas`

**Features:**
- SVG/SVGZ rasterization
- User-configurable DPI
- Size dialogs for import

### 11. pdfium (PDF Rasterizer)

**Plugin:** pdfium library
**Code:** `/home/user/PhotoDemon/Modules/Plugin_PDF.bas`

**Features:**
- PDF page rasterization
- Multi-page support
- Page selection dialog

### 12. Compression Libraries

**Libraries Used:**
- `libdeflate.dll` - DEFLATE compression (faster than zlib)
- `lz4.dll` - LZ4 compression (for PDI and temp files)
- `zstd.dll` - Zstandard compression (for PDI and updates)
- `zlib.dll` - ZIP/DEFLATE fallback

**Code References:**
- `/home/user/PhotoDemon/Modules/Plugin_libdeflate.bas`
- `/home/user/PhotoDemon/Modules/Plugin_lz4.bas`
- `/home/user/PhotoDemon/Modules/Plugin_zstd.bas`

---

## Import/Export Capabilities

### Import-Only Formats (40+ formats)

PhotoDemon can **import** these formats but cannot export them:

**RAW Camera Formats:** 50+ extensions via FreeImage
**Legacy Formats:** XCF (GIMP), XPM, PICT, PCD, G3, JNG, KOALA, LBM, SGI
**Document Formats:** PDF, SVG, EMF, WMF
**Scientific:** EXR (OpenEXR), PFM
**Archive:** CBZ (comic books), HGT (SRTM)
**Specialty:** MBM (Symbian), JLS (JPEG-LS), FAXG3

### Export-Capable Formats (25+ formats)

PhotoDemon can **export** to these formats:

**Web Standard:** PNG, JPEG, GIF, WebP, AVIF
**Modern:** JXL (JPEG XL), HEIF/HEIC
**Professional:** PSD (Photoshop), PSP (PaintShop Pro), PDI (PhotoDemon native)
**Open Standard:** ORA (OpenRaster), TIFF
**Specialized:** DDS (textures), ICO (icons), BMP, PCX, WBMP, QOI
**Advanced:** HDR (Radiance), JXR (JPEG XR), JP2 (JPEG 2000)
**Netpbm:** PNM/PPM/PGM/PBM, TGA (TARGA)

### Animation Support

| Format | Import Animation | Export Animation | Notes |
|--------|-----------------|------------------|-------|
| **GIF** | ✓ | ✓ | Full frame timing, disposal methods |
| **PNG/APNG** | ✓ | ✓ | Animated PNG support |
| **WebP** | ✓ | ✓ | Via libwebp |
| **JXL** | ✓ | ✓ | JPEG XL animation |
| **HEIF** | ✓ | ✓ | Image sequences |

**Code Reference:** `/home/user/PhotoDemon/Modules/ImageFormats.bas:946-953`

### Multi-Page Support

Formats supporting multiple pages/images per file:
- **TIFF** - Multi-page documents
- **ICO** - Multiple icon sizes
- **PDF** - Multiple pages (import only)
- **GIF** - Animation frames
- **PNG/APNG** - Animation frames
- **HEIF** - Image sequences

---

## .NET Library Mapping

For the .NET 10 MAUI migration, here are recommended libraries for each format category:

### Recommended: ImageSharp (Primary Library)

**Library:** SixLabors.ImageSharp
**License:** Apache 2.0 (with commercial licensing for non-OSS projects)
**NuGet:** `SixLabors.ImageSharp`, `SixLabors.ImageSharp.Drawing`

**Format Support:**
- **Built-in:** PNG, JPEG, GIF, BMP, TGA, WEBP (basic)
- **Quality:** High-quality, well-maintained, actively developed
- **Performance:** Excellent, optimized for .NET
- **Features:** Image processing, drawing, transformation

**Migration Path:**
- Use for PNG, JPEG, GIF, BMP, TGA as primary decoder/encoder
- Excellent API design, well-documented
- Cross-platform (MAUI compatible)

### Recommended: Magick.NET (Advanced Formats)

**Library:** Magick.NET (ImageMagick for .NET)
**License:** Apache 2.0
**NuGet:** `Magick.NET-Q16-AnyCPU` (or Q8/Q16-HDRI variants)

**Format Support:**
- **100+ formats:** PSD, XCF, TIFF, SVG, PDF, RAW, EXR, DDS, and more
- **Quality:** Mature, comprehensive
- **Performance:** Good, can be memory-intensive
- **Features:** Extensive image manipulation, format conversion

**Migration Path:**
- Use as fallback for formats not covered by ImageSharp
- Excellent for PSD, TIFF (multi-page), format conversion
- May need platform-specific native libraries

**Limitations:**
- PSD support is read-only for layers (writes flattened)
- RAW format support varies by ImageMagick build

### Specialized Libraries

#### 1. WebP: libwebp-sharp or WebP.NET

**Options:**
- `Imazen.WebP` (NuGet) - Native libwebp wrapper
- `WebPSharp` - Pure .NET implementation

**Recommendation:** Use native libwebp wrapper for performance

#### 2. AVIF/HEIF: libheif-sharp

**Library:** `libheif-sharp` (community wrapper) or P/Invoke to libheif
**NuGet:** Various community packages available

**Alternative:** Use Magick.NET which includes HEIF support

#### 3. JPEG XL: libjxl-net

**Status:** Community wrappers available but immature
**Recommendation:** Shell to cjxl/djxl.exe initially, migrate to native binding when stable

#### 4. JPEG 2000: CSJ2K or Magick.NET

**Options:**
- `CSJ2K` - Pure C# JPEG 2000 codec
- Magick.NET includes OpenJPEG support

**Recommendation:** Magick.NET for compatibility

#### 5. RAW Formats: LibRaw.NET

**Library:** LibRaw.NET or RawSpeed.NET
**NuGet:** Community packages available

**Recommendation:** Magick.NET includes LibRaw support

#### 6. DDS: DirectXTex (native) or custom P/Invoke

**Options:**
- Continue using texconv.exe via Process
- P/Invoke to DirectXTex.dll
- Community .NET wrappers

**Recommendation:** Process wrapper initially (existing code), migrate to native binding

#### 7. PDF: PDFium.NET or Docnet

**Libraries:**
- `PDFiumSharp` or `PdfiumViewer`
- `Docnet.Core` (modern alternative)

**Recommendation:** PDFiumSharp (mature, maintained)

#### 8. SVG: Svg.NET or SkiaSharp

**Libraries:**
- `Svg` (NuGet) - Pure .NET SVG rendering
- `SkiaSharp` with SVG extension

**Recommendation:** SkiaSharp.Svg for MAUI integration

### Custom Format Parsers (Port to .NET)

These should be **ported from VB6 to C#** as they represent significant IP and functionality:

#### High Priority Ports

1. **PSD Parser** (`pdPSD.cls`) ⭐⭐⭐⭐⭐
   - **Reason:** Exceptional quality, passes Apple test suite, better than most libraries
   - **Effort:** High (complex format, ~2000+ lines)
   - **Value:** Very High (competitive advantage)
   - **Alternative:** Magick.NET (but loses layer metadata, color management quality)

2. **PSP Parser** (`pdPSP.cls`) ⭐⭐⭐⭐
   - **Reason:** No good .NET alternatives exist
   - **Effort:** High (~2000+ lines)
   - **Value:** High (unique capability)
   - **Alternative:** None (format barely supported elsewhere)

3. **XCF Parser** (`pdXCF.cls`) ⭐⭐⭐⭐
   - **Reason:** Comprehensive GIMP support, no .NET alternatives
   - **Effort:** High (~2000+ lines)
   - **Value:** High (interoperability with GIMP)
   - **Alternative:** Magick.NET (limited)

4. **PNG Parser** (`pdPNG.cls`) ⭐⭐⭐
   - **Reason:** Excellent APNG support
   - **Effort:** Medium-High
   - **Value:** Medium (ImageSharp covers basics, but APNG support is unique)
   - **Alternative:** ImageSharp + AnimatedGif or custom APNG

5. **GIF Parser** (`pdGIF.cls`) ⭐⭐⭐
   - **Reason:** Full animation support
   - **Effort:** Medium
   - **Value:** Medium (ImageSharp has GIF, but verify animation features)
   - **Alternative:** ImageSharp

#### Medium Priority Ports

6. **ICO Parser** (`pdICO.cls`) ⭐⭐⭐
   - **Effort:** Low-Medium
   - **Alternative:** Built-in .NET or ImageSharp

7. **QOI Parser** (`pdQOI.cls`) ⭐⭐
   - **Effort:** Low (simple format, ~500 lines)
   - **Alternative:** QoiSharp (NuGet)

8. **PCX Parser** (`pdPCX.cls`) ⭐⭐
   - **Effort:** Low-Medium
   - **Alternative:** Magick.NET

#### Low Priority (Use .NET Libraries)

- **ORA** - Zip manipulation is trivial in .NET
- **CBZ** - Zip + image loading
- **HGT** - Simple binary format
- **WBMP, XBM, MBM** - Rare formats, implement if needed

### Metadata Handling: ExifTool.NET or MetadataExtractor

**Libraries:**
- `MetadataExtractor` (NuGet) - Pure .NET, excellent EXIF/IPTC/XMP support
- Continue using ExifTool.exe via Process (existing approach)

**Recommendation:**
- **Primary:** MetadataExtractor for common formats
- **Fallback:** ExifTool.exe for comprehensive coverage

### Drawing/Graphics: SkiaSharp

**Library:** SkiaSharp
**License:** MIT
**NuGet:** `SkiaSharp`, `SkiaSharp.Views.Maui`

**Use Cases:**
- Custom drawing operations
- SVG rendering
- High-performance graphics
- Cross-platform MAUI compatibility

**Migration Path:**
- Replace GDI+ operations with SkiaSharp
- Excellent MAUI integration
- Hardware acceleration support

---

## Migration Complexity Assessment

### Complexity Matrix by Format Category

| Category | VB6 Implementation | .NET Recommendation | Complexity | Effort | Risk |
|----------|-------------------|---------------------|------------|--------|------|
| **PNG/JPEG/GIF/BMP** | Native parsers + GDI+ | ImageSharp | ⭐⭐ | Low | Low |
| **PSD** | Custom parser | Port + Magick.NET fallback | ⭐⭐⭐⭐⭐ | High | Medium |
| **PSP** | Custom parser | Port to C# | ⭐⭐⭐⭐⭐ | High | Medium |
| **XCF** | Custom parser | Port to C# | ⭐⭐⭐⭐⭐ | High | Medium |
| **WebP** | libwebp.dll | libwebp wrapper or ImageSharp | ⭐⭐ | Low | Low |
| **AVIF** | avifdec.exe | libavif wrapper or Process | ⭐⭐⭐ | Medium | Medium |
| **HEIF** | libheif.dll | libheif wrapper or Magick.NET | ⭐⭐⭐ | Medium | Medium |
| **JXL** | djxl.exe | Process wrapper initially | ⭐⭐⭐ | Medium | Medium |
| **JP2** | OpenJPEG | Magick.NET or CSJ2K | ⭐⭐⭐ | Medium | Low |
| **DDS** | texconv.exe | Process wrapper or DirectXTex | ⭐⭐⭐ | Medium | Low |
| **RAW** | FreeImage | Magick.NET or LibRaw | ⭐⭐⭐⭐ | Medium | Low |
| **TIFF** | FreeImage/GDI+ | ImageSharp or Magick.NET | ⭐⭐ | Low | Low |
| **PDF** | pdfium | PDFiumSharp | ⭐⭐ | Low | Low |
| **SVG** | resvg | SkiaSharp.Svg | ⭐⭐ | Low | Low |
| **HDR/EXR** | FreeImage | Magick.NET | ⭐⭐⭐ | Medium | Low |
| **Legacy formats** | Native/FreeImage | Magick.NET | ⭐⭐ | Low | Low |

### Migration Strategy Recommendations

#### Phase 1: Foundation (Months 1-2)
**Goal:** Get basic format support working

1. **Integrate ImageSharp** for PNG, JPEG, GIF, BMP, TGA
2. **Integrate Magick.NET** as comprehensive fallback
3. **Test compatibility** with existing image files
4. **Implement format detection** logic
5. **Create abstraction layer** for format I/O

**Effort:** 2-3 weeks
**Risk:** Low
**Deliverable:** Basic image load/save working for 80% of common formats

#### Phase 2: Modern Formats (Months 2-3)
**Goal:** WebP, AVIF, HEIF, JXL support

1. **Integrate libwebp** wrapper for WebP
2. **Test Magick.NET HEIF** support or integrate libheif
3. **Process wrapper for AVIF** (avifdec/avifenc.exe)
4. **Process wrapper for JXL** (djxl/cjxl.exe)
5. **Implement import dialogs** for vector formats (PDF, SVG)

**Effort:** 3-4 weeks
**Risk:** Medium (dependency on native libraries)
**Deliverable:** Modern format support matching VB6 version

#### Phase 3: Professional Formats (Months 3-5)
**Goal:** PSD, PSP, XCF support (CRITICAL DIFFERENTIATOR)

1. **Port PSD parser to C#** (highest priority)
   - Start with header/layer parsing
   - Port color management logic
   - Test against Apple test suite
   - Implement compression handlers

2. **Port PSP parser to C#**
   - Block structure parsing
   - Layer and composite handling
   - Version-specific logic

3. **Port XCF parser to C#**
   - Tile-based loading
   - Precision handling
   - Compression support

4. **Test extensively** with real-world files

**Effort:** 8-10 weeks (parallelizable)
**Risk:** High (complex formats, extensive testing needed)
**Deliverable:** Feature parity with VB6 for professional formats

#### Phase 4: Specialized Formats (Months 5-6)
**Goal:** Complete format coverage

1. **Port remaining custom parsers** (QOI, ICO, PCX, etc.)
2. **Implement DDS support** via DirectXTex
3. **Implement JP2** via Magick.NET or OpenJPEG wrapper
4. **RAW format support** via Magick.NET
5. **Legacy format fallbacks** via Magick.NET

**Effort:** 4-6 weeks
**Risk:** Low-Medium
**Deliverable:** 100% format coverage parity

#### Phase 5: Optimization & Polish (Month 6+)
**Goal:** Performance, memory optimization, edge cases

1. **Performance profiling** for all formats
2. **Memory optimization** (streaming, large files)
3. **Edge case handling** (malformed files, errors)
4. **Animation support** refinement
5. **Metadata preservation** via MetadataExtractor
6. **Export dialog** implementation

**Effort:** 4-6 weeks
**Risk:** Low
**Deliverable:** Production-ready format support

### Critical Success Factors

1. **Preserve PSD/PSP/XCF parsers** - These are competitive advantages and should be ported, not replaced
2. **Test extensively** - Build comprehensive test suite with real-world files
3. **Maintain color management** - PD's color-managed pipeline is a key differentiator
4. **Animation support** - Ensure GIF, APNG, WebP, JXL animation works correctly
5. **Metadata handling** - Preserve EXIF/IPTC/XMP across formats
6. **Error handling** - Graceful degradation when formats aren't supported

### Risk Mitigation

| Risk | Mitigation Strategy |
|------|-------------------|
| **Custom parser porting errors** | Extensive unit tests, reference file comparison, gradual rollout |
| **Native library dependencies** | Bundle libraries with app, provide fallbacks, test on multiple platforms |
| **Performance regression** | Benchmark against VB6 version, optimize hot paths, use streaming where possible |
| **Format incompatibility** | Comprehensive test suite, user feedback loop, version tracking |
| **License issues** | Verify all library licenses, commercial licensing where needed (ImageSharp) |
| **Platform-specific issues** | Test on Windows, macOS, iOS, Android, handle platform differences |

---

## Code References

### Main Format Management

| Module/Class | Path | Description |
|-------------|------|-------------|
| **ImageFormats** | `/home/user/PhotoDemon/Modules/ImageFormats.bas` | Central format registry and detection |
| **ImageImporter** | `/home/user/PhotoDemon/Modules/ImageLoader.bas` | Import orchestration |
| **ImageExporter** | `/home/user/PhotoDemon/Modules/Saving.bas` | Export orchestration |
| **Loading** | `/home/user/PhotoDemon/Modules/Loading.bas` | High-level load interface |
| **Saving** | `/home/user/PhotoDemon/Modules/Saving.bas` | High-level save interface |

**Key Functions:**
- `ImageFormats.GenerateInputFormats()` - Line 188-373 (format registration)
- `ImageFormats.GenerateOutputFormats()` - Line 402-479 (export formats)
- `ImageFormats.GetPDIFFromExtension()` - Line 772-891 (extension to format mapping)

### Format Implementations

#### Native Parsers (Classes)

| Format | Class File | Lines | Key Methods |
|--------|-----------|-------|-------------|
| PSD | `/home/user/PhotoDemon/Classes/pdPSD.cls` | 1-2000+ | `LoadPSD()`, color mode handlers |
| PSP | `/home/user/PhotoDemon/Classes/pdPSP.cls` | 1-2000+ | `LoadPSP()`, block parsers |
| XCF | `/home/user/PhotoDemon/Classes/pdXCF.cls` | 1-2000+ | `LoadXCF()`, tile loaders |
| PNG | `/home/user/PhotoDemon/Classes/pdPNG.cls` | 1-2000+ | Chunk handlers, APNG support |
| GIF | `/home/user/PhotoDemon/Classes/pdGIF.cls` | 1-2000+ | LZW decoder, animation |
| ICO | `/home/user/PhotoDemon/Classes/pdICO.cls` | 1-1000+ | Multi-size handling |
| PCX | `/home/user/PhotoDemon/Classes/pdPCX.cls` | 1-1000+ | RLE decoder, DCX multi-page |
| QOI | `/home/user/PhotoDemon/Classes/pdQOI.cls` | 1-500+ | Fast encoding/decoding |
| MBM | `/home/user/PhotoDemon/Classes/pdMBM.cls` | 1-1000+ | Symbian format |
| ORA | `/home/user/PhotoDemon/Classes/pdOpenRaster.cls` | 1-1000+ | ZIP-based layers |
| CBZ | `/home/user/PhotoDemon/Classes/pdCBZ.cls` | 1-500+ | Comic archive |
| HGT | `/home/user/PhotoDemon/Classes/pdHGT.cls` | 1-500+ | SRTM elevation |
| PDF | `/home/user/PhotoDemon/Classes/pdPDF.cls` | 1-1000+ | Via pdfium |

#### Plugin Interfaces (Modules)

| Format/Plugin | Module File | Lines | Description |
|--------------|------------|-------|-------------|
| FreeImage | `/home/user/PhotoDemon/Modules/Plugin_FreeImage.bas` | 1-150+ | Legacy format fallback |
| WebP | `/home/user/PhotoDemon/Modules/Plugin_WebP.bas` | 1-100+ | libwebp interface |
| AVIF | `/home/user/PhotoDemon/Modules/Plugin_AVIF.bas` | 1-100+ | avifdec/enc.exe wrapper |
| HEIF | `/home/user/PhotoDemon/Modules/Plugin_heif.bas` | 1-100+ | libheif interface |
| JXL | `/home/user/PhotoDemon/Modules/Plugin_jxl.bas` | 1-100+ | djxl/cjxl.exe wrapper |
| OpenJPEG | `/home/user/PhotoDemon/Modules/Plugin_OpenJPEG.bas` | 1-100+ | JPEG 2000 support |
| CharLS | `/home/user/PhotoDemon/Modules/Plugin_CharLS.bas` | 1-100+ | JPEG-LS support |
| DDS | `/home/user/PhotoDemon/Modules/Plugin_DDS.bas` | 1-100+ | texconv.exe wrapper |
| ExifTool | `/home/user/PhotoDemon/Modules/Plugin_ExifTool.bas` | 1-100+ | Metadata via exiftool.exe |
| PDF | `/home/user/PhotoDemon/Modules/Plugin_PDF.bas` | 1-100+ | pdfium interface |
| resvg | `/home/user/PhotoDemon/Modules/Plugin_resvg.bas` | 1-100+ | SVG rasterization |

### Format Enums

**Location:** `/home/user/PhotoDemon/Modules/PublicEnumsAndTypes.bas:541-621`

**Enum:** `PD_IMAGE_FORMAT`

Contains all format constants (PDIF_*) used throughout the codebase for format identification.

---

## Migration Checklist

### Pre-Migration Preparation
- [ ] Audit current format usage statistics (which formats are actually used by users)
- [ ] Collect test file suite for each supported format
- [ ] Document current color management pipeline
- [ ] Benchmark current performance metrics
- [ ] Identify commercial licensing requirements (e.g., ImageSharp)

### Library Integration
- [ ] Set up NuGet packages (ImageSharp, Magick.NET, SkiaSharp)
- [ ] Test library compatibility on target platforms (Windows, macOS, iOS, Android)
- [ ] Create abstraction layer for format I/O
- [ ] Implement format detection and routing logic
- [ ] Set up error handling and logging

### Format-by-Format Migration
- [ ] Phase 1: Common formats (PNG, JPEG, GIF, BMP) via ImageSharp
- [ ] Phase 2: Modern formats (WebP, AVIF, HEIF, JXL)
- [ ] Phase 3: Professional formats (PSD, PSP, XCF) - **CRITICAL**
- [ ] Phase 4: Specialized formats (DDS, JP2, RAW, etc.)
- [ ] Phase 5: Legacy formats (PCX, XBM, WBMP, etc.)

### Parser Porting (C# Conversion)
- [ ] Port PSD parser (`pdPSD.cls` → C#)
- [ ] Port PSP parser (`pdPSP.cls` → C#)
- [ ] Port XCF parser (`pdXCF.cls` → C#)
- [ ] Port PNG parser (`pdPNG.cls` → C# or use ImageSharp)
- [ ] Port GIF parser (`pdGIF.cls` → C# or use ImageSharp)
- [ ] Port remaining custom parsers as needed

### Testing & Validation
- [ ] Unit tests for each format
- [ ] Integration tests with real-world files
- [ ] Performance benchmarks vs VB6 version
- [ ] Memory usage testing
- [ ] Edge case handling (malformed files, large files, etc.)
- [ ] Animation support verification
- [ ] Metadata preservation testing
- [ ] Color management validation (especially for PSD)

### Documentation & Training
- [ ] Update user documentation for format support
- [ ] Document breaking changes or limitations
- [ ] Create developer guide for adding new formats
- [ ] Document library dependencies and licensing

---

## Appendix: Format Feature Comparison

### Color Depth Support

| Format | 1-8 bpp | 16 bpp | 24 bpp | 32 bpp | 48 bpp | 64 bpp | Float | HDR |
|--------|---------|--------|--------|--------|--------|--------|-------|-----|
| PNG | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | - | - |
| JPEG | - | - | ✓ | - | - | - | - | - |
| GIF | ✓ | - | - | - | - | - | - | - |
| WebP | - | - | ✓ | ✓ | - | - | - | - |
| AVIF | - | ✓ | ✓ | ✓ | ✓ | - | - | ✓ |
| HEIF | - | ✓ | ✓ | ✓ | ✓ | - | - | ✓ |
| JXL | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| PSD | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| PSP | ✓ | ✓ | ✓ | ✓ | ✓ | - | - | - |
| XCF | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | - |
| TIFF | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| EXR | - | - | - | - | - | - | ✓ | ✓ |
| HDR | - | - | - | - | - | - | ✓ | ✓ |

### Metadata Support

| Format | EXIF | IPTC | XMP | ICC Profile | Custom |
|--------|------|------|-----|-------------|--------|
| JPEG | ✓ | ✓ | ✓ | ✓ | - |
| PNG | - | - | ✓ | ✓ | ✓ (tEXt) |
| TIFF | ✓ | ✓ | ✓ | ✓ | ✓ |
| WebP | - | - | ✓ | ✓ | - |
| AVIF | ✓ | - | ✓ | ✓ | - |
| HEIF | ✓ | - | ✓ | ✓ | - |
| JXL | ✓ | - | ✓ | ✓ | - |
| PSD | ✓ | ✓ | ✓ | ✓ | ✓ |
| PSP | ✓ | - | - | ✓ | ✓ |

---

## Summary

PhotoDemon's file format support represents a **significant competitive advantage** through:

1. **Comprehensive Coverage**: 40+ import formats, 25+ export formats
2. **Custom Parsers**: High-quality PSD, PSP, XCF parsers that rival or exceed commercial alternatives
3. **Modern Codec Support**: AVIF, HEIF, JXL, WebP with full feature support
4. **Professional Features**: Multi-layer, animation, HDR, RAW support
5. **Color Management**: Exceptional quality (Apple test suite compliance for PSD)

### .NET Migration Priorities

**Highest Priority (Must Port):**
1. **PSD Parser** - Competitive differentiator, best-in-class quality
2. **PSP Parser** - Unique capability, no good alternatives
3. **XCF Parser** - GIMP interoperability, comprehensive support

**High Priority (Recommended Port):**
4. **PNG/APNG Parser** - APNG support advantage
5. **GIF Parser** - Animation expertise

**Medium Priority (Consider Port or Use Libraries):**
6. ICO, QOI, PCX parsers - Simple formats, library alternatives exist

**Use .NET Libraries:**
- Standard formats: ImageSharp (PNG, JPEG, GIF, BMP)
- Advanced formats: Magick.NET (TIFF, RAW, legacy formats)
- Modern formats: Native library wrappers (WebP, AVIF, HEIF, JXL)
- Metadata: MetadataExtractor or ExifTool wrapper

### Recommended Library Stack

```
Primary: ImageSharp (standard formats)
Secondary: Magick.NET (advanced/fallback)
Specialized: libwebp, libheif, libavif, libjxl (native wrappers)
Custom: Port PSD/PSP/XCF parsers to C#
Metadata: MetadataExtractor
Drawing: SkiaSharp
```

### Estimated Migration Effort

- **Total Effort:** 5-6 months (with 2-3 developers)
- **Critical Path:** Custom parser porting (PSD, PSP, XCF)
- **Risk Level:** Medium (mostly well-understood, extensive testing needed)
- **Success Probability:** High (clear path, good library support, valuable IP to preserve)

---

**End of Document**
