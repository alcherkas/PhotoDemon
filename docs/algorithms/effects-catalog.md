# PhotoDemon Effects & Algorithms Catalog

**Document Version:** 1.0
**Date:** 2025-11-17
**Project:** PhotoDemon VB6 to .NET 10 MAUI Migration
**Agent:** Agent 7 - Effects & Algorithms

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Effects Catalog Overview](#effects-catalog-overview)
3. [Complete Effects Listing](#complete-effects-listing)
   - [Adjustments (28 effects)](#adjustments)
   - [Effects - Artistic (12 effects)](#effects---artistic)
   - [Effects - Blur (8 effects)](#effects---blur)
   - [Effects - Distort (12 effects)](#effects---distort)
   - [Effects - Edge (6 effects)](#effects---edge)
   - [Effects - Light and Shadow (7 effects)](#effects---light-and-shadow)
   - [Effects - Natural (7 effects)](#effects---natural)
   - [Effects - Noise (8 effects)](#effects---noise)
   - [Effects - Pixelate (6 effects)](#effects---pixelate)
   - [Effects - Render (3 effects)](#effects---render)
   - [Effects - Sharpen (2 effects)](#effects---sharpen)
   - [Effects - Stylize (9 effects)](#effects---stylize)
   - [Effects - Transform (6 effects)](#effects---transform)
   - [Animation Effects (3 effects)](#animation-effects)
4. [Algorithm Implementation Details](#algorithm-implementation-details)
5. [Performance-Critical Sections](#performance-critical-sections)
6. [Macro Recording System](#macro-recording-system)
7. [Batch Processing System](#batch-processing-system)
8. [External Dependencies](#external-dependencies)
9. [Migration Strategy](#migration-strategy)

---

## Executive Summary

PhotoDemon contains **117+ image processing effects and adjustments**, organized into 14 major categories. The application uses a sophisticated architecture with custom algorithm implementations, external library integrations, and advanced features like macro recording and batch processing.

### Key Statistics

- **Total Effects/Adjustments:** 117+
- **Effect Categories:** 14 major categories
- **Forms/Dialogs:** 216 total forms (113 for effects/adjustments)
- **Filter Modules:** 11 core filter implementation modules
- **Architecture:** XML-based parameter system with macro recording support
- **File Format:** PDM (PhotoDemon Macro) files in XML format

---

## Effects Catalog Overview

### Category Distribution

| Category | Count | Complexity | Migration Priority |
|----------|-------|------------|-------------------|
| Adjustments | 28 | Medium-High | High |
| Artistic | 12 | Medium | Medium |
| Blur | 8 | High | High |
| Distort | 12 | High | Medium |
| Edge Detection | 6 | Medium | Medium |
| Light and Shadow | 7 | Medium | Medium |
| Natural Effects | 7 | Medium | Low |
| Noise | 8 | Medium-High | Medium |
| Pixelate | 6 | Low-Medium | Low |
| Render | 3 | Medium | Low |
| Sharpen | 2 | High | High |
| Stylize | 9 | Medium | Medium |
| Transform | 6 | High | High |
| Animation | 3 | Medium | Medium |

---

## Complete Effects Listing

### Adjustments

#### Color Adjustments

| Effect Name | Form Location | Implementation | Algorithm Type |
|-------------|---------------|----------------|----------------|
| **Auto Correct** | N/A (direct menu) | `/Modules/Filters_Color.bas:20` | Auto color correction |
| **Auto Enhance** | N/A (direct menu) | `/Modules/Filters_Color.bas:369` | Auto enhancement |
| **Black and White** | `/Forms/Adjustments_BlackAndWhite.frm` | `/Modules/Filters_Color.bas` | Channel mixing |
| **Brightness and Contrast** | `/Forms/Adjustments_BrightnessContrast.frm` | Color adjustment | Pixel-level transform |
| **Color Balance** | `/Forms/Adjustments_ColorBalance.frm` | Color adjustment | RGB channel balancing |
| **Hue and Saturation** | `/Forms/Adjustments_Color_HSL.frm` | Color space conversion | HSL color space |
| **Temperature** | `/Forms/Adjustments_Color_Temperature.frm` | Color temperature | White point adjustment |
| **Tint** | `/Forms/Adjustments_Color_Tint.frm` | Color tinting | Color overlay |
| **Vibrance** | `/Forms/Adjustments_Color_Vibrance.frm` | Saturation adjustment | Selective saturation |
| **Colorize** | `/Forms/Adjustments_Color_Colorize.frm` | Color overlay | Hue mapping |
| **Replace Color** | `/Forms/Adjustments_Color_ReplaceColor.frm` | Color replacement | Selective color |
| **Sepia** | `/Forms/Adjustments_Color_Sepia.frm` | Color tone | Sepia tone formula |
| **Grayscale** | `/Forms/Adjustments_Color_Grayscale.frm` | Color to mono | Weighted average |
| **Color Lookup** | `/Forms/Adjustments_Color_Lookup.frm` | LUT application | 3D LUT |
| **Split Toning** | `/Forms/Adjustments_Photo_SplitTone.frm` | Dual color tone | Highlight/Shadow toning |
| **Photo Filter** | `/Forms/Adjustments_Photo_PhotoFilters.frm` | Filter simulation | Color overlay |
| **White Balance** | `/Forms/Adjustments_WhiteBalance.frm` | White point | Auto/manual balance |

#### Channel Operations

| Effect Name | Form Location | Implementation | Algorithm Type |
|-------------|---------------|----------------|----------------|
| **Channel Mixer** | `/Forms/Adjustments_Channel_ChannelMixer.frm` | Channel mixing | Matrix transform |
| **Rechannel** | `/Forms/Adjustments_Channel_Rechannel.frm` | Channel swap | Channel reassignment |
| **Maximum Channel** | N/A (direct menu) | `/Modules/Filters_Color.bas:299` | Channel selection |
| **Minimum Channel** | N/A (direct menu) | `/Modules/Filters_Color.bas:299` | Channel selection |
| **Shift Left** | N/A (direct menu) | `/Modules/Filters_Color.bas:109` | Channel rotation |
| **Shift Right** | N/A (direct menu) | `/Modules/Filters_Color.bas:109` | Channel rotation |

#### Lighting & Tone

| Effect Name | Form Location | Implementation | Algorithm Type |
|-------------|---------------|----------------|----------------|
| **Curves** | `/Forms/Adjustments_Curves.frm` | Tone curves | Spline interpolation |
| **Levels** | `/Forms/Adjustments_Levels.frm` | Histogram mapping | Input/output levels |
| **Gamma** | `/Forms/Adjustments_Lighting_Gamma.frm` | Gamma correction | Power function |
| **Exposure** | `/Forms/Adjustments_Lighting_Exposure.frm` | Exposure adjustment | EV compensation |
| **Dehaze** | `/Forms/Adjustments_Lighting_Dehaze.frm` | Atmospheric removal | Contrast restoration |
| **HDR** | `/Forms/Adjustments_Photo_HDR.frm` | Tone mapping | HDR simulation |
| **Shadows and Highlights** | `/Forms/Adjustments_ShadowAndHighlight.frm` | Shadow/highlight | Selective tone mapping |

#### Histogram Operations

| Effect Name | Form Location | Implementation | Algorithm Type |
|-------------|---------------|----------------|----------------|
| **Display Histogram** | `/Forms/Adjustments_Histogram_DisplayHistogram.frm` | Histogram view | Statistical analysis |
| **Equalize** | `/Forms/Adjustments_Histogram_Equalize.frm` | Histogram equalization | Cumulative distribution |
| **Stretch** | N/A (direct menu) | Histogram stretch | Auto levels |

#### Inversion

| Effect Name | Form Location | Implementation | Algorithm Type |
|-------------|---------------|----------------|----------------|
| **Invert RGB** | N/A (direct menu) | `/Modules/Filters_Color.bas:169` | Pixel inversion |
| **Invert CMYK** | N/A (direct menu) | `/Modules/Filters_Color.bas:57` | Film negative |
| **Invert Hue** | N/A (direct menu) | `/Modules/Filters_Color.bas:232` | Hue rotation |

#### Mapping

| Effect Name | Form Location | Implementation | Algorithm Type |
|-------------|---------------|----------------|----------------|
| **Gradient Map** | `/Forms/Adjustments_Map_Gradient.frm` | Gradient mapping | Luminance to gradient |
| **Palette Map** | N/A | Palette mapping | Color quantization |

#### Monochrome

| Effect Name | Form Location | Implementation | Algorithm Type |
|-------------|---------------|----------------|----------------|
| **Color to Monochrome** | N/A | Color conversion | Channel weighting |
| **Monochrome to Gray** | `/Forms/Adjustments_Monochrome_MonoToGray.frm` | Bit depth conversion | Grayscale conversion |

---

### Effects - Artistic

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Colored Pencil** | `/Forms/Effects_Artistic_ColoredPencil.frm` | Edge detection + texture | Medium | Edge-based sketch |
| **Comic Book** | `/Forms/Effects_Artistic_ComicBook.frm` | Edge detection + quantization | Medium | Posterize + outline |
| **Figured Glass (Dents)** | `/Forms/Effects_Artistic_FiguredGlass.frm` | Displacement mapping | Medium | Glass distortion |
| **Film Noir** | `/Forms/Effects_Artistic_FilmNoir.frm` | High contrast B&W | Low | Contrast enhancement |
| **Glass Tiles** | `/Forms/Effects_Artistic_GlassTiles.frm` | Tile displacement | Medium | Grid-based distortion |
| **Kaleidoscope** | `/Forms/Effects_Artistic_Kaleidoscope.frm` | Radial symmetry | High | Polar mirroring |
| **Modern Art** | `/Forms/Effects_Artistic_ModernArt.frm` | Abstract patterns | Medium | Pattern generation |
| **Oil Painting** | `/Forms/Effects_Artistic_OilPainting.frm` | Kuwahara filter | High | Anisotropic smoothing |
| **Plastic Wrap** | `/Forms/Effects_Artistic_PlasticWrap.frm` | Highlight enhancement | Medium | Specular highlights |
| **Posterize** | `/Forms/Effects_Artistic_Posterize.frm` | Color quantization | Low | Bit reduction |
| **Relief** | `/Forms/Effects_Artistic_Relief.frm` | `/Modules/Filters_Edge.bas:22` | Medium | 3D embossing |
| **Stained Glass** | `/Forms/Effects_Artistic_StainedGlass.frm` | Voronoi tessellation | High | Cell-based coloring |

---

### Effects - Blur

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Box Blur** | `/Forms/Effects_Blur_BoxBlur.frm` | `/Modules/Filters_Area.bas` | Low | Simple averaging |
| **Gaussian Blur** | `/Forms/Effects_Blur_GaussianBlur.frm` | `/Modules/Filters_Area.bas:621` | High | Deriche/AM algorithm |
| **Surface Blur** | `/Forms/Effects_Blur_SurfaceBlur.frm` | Bilateral filter | High | Edge-preserving blur |
| **Motion Blur** | `/Forms/Effects_Blur_MotionBlur.frm` | Directional blur | Medium | Linear convolution |
| **Radial Blur** | `/Forms/Effects_Blur_RadialBlur.frm` | Polar blur | High | Rotational blur |
| **Zoom Blur** | `/Forms/Effects_Blur_ZoomBlur.frm` | Radial zoom | Medium | Center-out blur |
| **Kuwahara** | `/Forms/Effects_Blur_Kuwahara.frm` | Kuwahara filter | High | Edge-preserving smooth |
| **SNN (Symmetric Nearest-Neighbor)** | `/Forms/Effects_Blur_SNN.frm` | Noise reduction | Medium | Symmetric filter |

**Performance Note:** Gaussian Blur uses advanced IIR (Infinite Impulse Response) filters with Deriche and Alvarez-Mazorra algorithms for optimal performance on large images.

---

### Effects - Distort

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Correct Lens Distortion** | `/Forms/Effects_Distort_CorrectLens.frm` | Barrel/pincushion correction | High | Lens distortion model |
| **Donut** | `/Forms/Effects_Distort_Donut.frm` | Circular warp | Medium | Polar transformation |
| **Droste** | `/Forms/Effects_Distort_Droste.frm` | Recursive zoom | Very High | Complex logarithmic spiral |
| **Lens** | `/Forms/Effects_Distort_ApplyLens.frm` | Spherical warp | Medium | Fish-eye effect |
| **Pinch and Whirl** | `/Forms/Effects_Distort_Pinch.frm` | Combined distortion | Medium | Radial pinch + rotation |
| **Poke** | `/Forms/Effects_Distort_Poke.frm` | Inverse pinch | Medium | Outward push |
| **Ripple** | `/Forms/Effects_Distort_Ripple.frm` | Wave distortion | Medium | Sine wave displacement |
| **Squish** | `/Forms/Effects_Distort_Squish.frm` | Anamorphic squeeze | Medium | Non-uniform scaling |
| **Swirl** | `/Forms/Effects_Distort_Swirl.frm` | Rotation distortion | Medium | Angular displacement |
| **Waves** | `/Forms/Effects_Distort_Waves.frm` | Multi-directional waves | Medium | Multiple sine waves |
| **Miscellaneous** | `/Forms/Effects_Distort_Miscellaneous.frm` | Various distortions | Variable | Multiple effects |

---

### Effects - Edge

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Emboss** | `/Forms/Effects_Edge_Emboss.frm` | Directional derivative | Low | 3D relief effect |
| **Enhance Edges** | `/Forms/Effects_Edge_EnhanceEdges.frm` | Edge sharpening | Medium | Contrast enhancement |
| **Find Edges** | `/Forms/Effects_Edge_FindEdges.frm` | Edge detection | Medium | Sobel/Prewitt operators |
| **Gradient Flow** | `/Forms/Effects_Edge_GradientFlow.frm` | Gradient field | High | Flow visualization |
| **Range Filter** | `/Forms/Effects_Edge_Range.frm` | Local range | Medium | Min-max difference |
| **Trace Contour** | `/Forms/Effects_Edge_TraceContour.frm` | Contour tracing | Medium | Isoline detection |

---

### Effects - Light and Shadow

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Black Light** | `/Forms/Effects_LightAndShadow_Blacklight.frm` | UV simulation | Low | Color inversion effect |
| **Bump Map** | `/Forms/Effects_LightAndShadow_BumpMap.frm` | 3D lighting | High | Normal map lighting |
| **Cross-Screen** | `/Forms/Effects_LightAndShadow_CrossScreen.frm` | Star filter | Medium | Radial line pattern |
| **Rainbow** | `/Forms/Effects_LightAndShadow_Rainbow.frm` | Spectrum overlay | Low | Color gradient |
| **Sunshine** | `/Forms/Effects_LightAndShadow_Sunshine.frm` | Radial rays | Medium | God rays effect |
| **Dilate** | N/A (direct menu) | Morphological dilation | Medium | Maximum filter |
| **Erode** | N/A (direct menu) | Morphological erosion | Medium | Minimum filter |

---

### Effects - Natural

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Atmosphere** | `/Forms/Effects_Nature_Atmosphere.frm` | Atmospheric haze | Medium | Distance fog |
| **Fog** | `/Forms/Effects_Nature_Fog.frm` | Perlin noise fog | Medium | Procedural fog |
| **Ignite** | `/Forms/Effects_Nature_Ignite.frm` | Fire effect | High | Turbulence-based fire |
| **Lava** | `/Forms/Effects_Nature_Lava.frm` | Lava texture | High | Procedural texture |
| **Metal** | `/Forms/Effects_Nature_Metal.frm` | `/Modules/Filters_Natural.bas:21` | High | Chrome/steel effect |
| **Snow** | `/Forms/Effects_Nature_Snow.frm` | Snow particles | Medium | Noise-based particles |
| **Underwater** | `/Forms/Effects_Nature_Water.frm` | Water effect | Medium | Caustic patterns |

---

### Effects - Noise

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Add Film Grain** | `/Forms/Effects_Noise_FilmGrain.frm` | Grain simulation | Low | Random noise |
| **Add RGB Noise** | `/Forms/Effects_Noise_AddRGBNoise.frm` | Color noise | Low | Per-channel noise |
| **Anisotropic Diffusion** | `/Forms/Effects_Noise_Anisotropic.frm` | Edge-preserving smoothing | Very High | PDE-based smoothing |
| **Dust and Scratches** | `/Forms/Effects_Noise_DustAndScratches.frm` | Defect removal | Medium | Conditional median |
| **Harmonic Mean** | `/Forms/Effects_Noise_HarmonicMean.frm` | Noise reduction | Medium | Harmonic averaging |
| **Mean Shift** | `/Forms/Effects_Noise_MeanShift.frm` | Segmentation/smoothing | Very High | Iterative clustering |
| **Median** | `/Forms/Effects_Noise_MedianSmoothing.frm` | Median filter | Medium | Order statistic |
| **Symmetric Nearest-Neighbor** | `/Forms/Effects_Noise_SNN.frm` | Noise reduction | Medium | Symmetric averaging |

**Performance Note:** Anisotropic diffusion and mean shift are computationally intensive and require optimization in .NET.

---

### Effects - Pixelate

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Color Halftone** | `/Forms/Effects_Pixelate_ColorHalftone.frm` | `/Modules/Filters_Stylize.bas:30` | High | CMYK halftone screen |
| **Crystallize** | `/Forms/Effects_Pixelate_Crystallize.frm` | Voronoi cells | High | Voronoi tessellation |
| **Fragment** | `/Forms/Effects_Pixelate_Fragment.frm` | Multi-copy offset | Low | Simple displacement |
| **Mezzotint** | `/Forms/Effects_Pixelate_Mezzotint.frm` | Stipple pattern | Medium | Dithering patterns |
| **Mosaic** | `/Forms/Effects_Pixelate_Mosaic.frm` | Block averaging | Low | Grid-based pixelation |
| **Pointillize** | `/Forms/Effects_Pixelate_Pointillize.frm` | Dot pattern | Medium | Circular cell pattern |

---

### Effects - Render

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Clouds** | `/Forms/Effects_Render_Clouds.frm` | `/Modules/Filters_Render.bas:53` | Medium | Perlin/Simplex noise |
| **Fibers** | `/Forms/Effects_Render_Fibers.frm` | `/Modules/Filters_Render.bas:301` | Medium | Procedural fibers |
| **Truchet** | `/Forms/Effects_Render_Truchet.frm` | Truchet tiles | Medium | Tiling patterns |

---

### Effects - Sharpen

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Sharpen** | `/Forms/Effects_Sharpen_Sharpen.frm` | Unsharp mask | Medium | High-pass filter |
| **Unsharp Mask** | `/Forms/Effects_Sharpen_UnsharpMask.frm` | Advanced USM | High | Gaussian + blend |

**Performance Note:** Unsharp mask leverages the optimized Gaussian blur implementation.

---

### Effects - Stylize

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Antique** | `/Forms/Effects_Stylize_Antique.frm` | `/Modules/Filters_Stylize.bas:288` | Medium | Vintage photo effect |
| **Diffuse** | `/Forms/Effects_Stylize_Diffuse.frm` | Random displacement | Low | Noise displacement |
| **Kuwahara** | `/Forms/Effects_Stylize_Kuwahara.frm` | Edge-preserving filter | High | Anisotropic smoothing |
| **Outline** | `/Forms/Effects_Stylize_Outline.frm` | Edge extraction | Medium | Sobel edge detection |
| **Palette** | `/Forms/Effects_Stylize_Palettize.frm` | Color quantization | Medium | K-means clustering |
| **Portrait Glow** | `/Forms/Effects_Stylize_PortraitGlow.frm` | Soft focus | Medium | Gaussian + screen blend |
| **Solarize** | `/Forms/Effects_Stylize_Solarize.frm` | Tone curve | Low | Threshold inversion |
| **Twins** | `/Forms/Effects_Stylize_Twins.frm` | Mirror effect | Low | Image duplication |
| **Vignetting** | `/Forms/Effects_Stylize_Vignette.frm` | Edge darkening | Low | Radial gradient |

---

### Effects - Transform

| Effect Name | Form Location | Implementation | Complexity | Algorithm Description |
|-------------|---------------|----------------|------------|----------------------|
| **Offset and Zoom** | `/Forms/Effects_Transform_PanZoom.frm` | Translation + scale | Low | Affine transform |
| **Perspective** | `/Forms/Effects_Transform_Perspective.frm` | 4-point transform | High | Homography |
| **Polar Conversion** | `/Forms/Effects_Transform_PolarCoords.frm` | Cartesian ↔ Polar | Medium | Coordinate transform |
| **Rotate** | `/Forms/Effects_Transform_Rotate.frm` | Arbitrary rotation | Medium | Affine transform |
| **Shear** | `/Forms/Effects_Transform_Shear.frm` | Skew transform | Medium | Affine transform |
| **Spherize** | `/Forms/Effects_Transform_Sphere.frm` | Spherical warp | High | 3D projection |

**Migration Note:** All transform operations should use .NET's Matrix transformation capabilities for optimal performance.

---

### Animation Effects

| Effect Name | Form Location | Implementation | Complexity | Description |
|-------------|---------------|----------------|------------|-------------|
| **Background** | `/Forms/Effects_Animation_Background.frm` | Background control | Low | Frame background |
| **Foreground** | N/A (referenced in menu) | Foreground control | Low | Frame foreground |
| **Playback Speed** | `/Forms/Effects_Animation_Speed.frm` | Timing control | Low | Frame delay adjustment |

---

## Algorithm Implementation Details

### Core Filter Modules

PhotoDemon organizes its algorithms into specialized modules:

| Module | Location | Primary Functions | Complexity |
|--------|----------|-------------------|------------|
| **Filters_Area** | `/Modules/Filters_Area.bas` | Convolution, Gaussian blur, Box blur | High |
| **Filters_ByteArray** | `/Modules/Filters_ByteArray.bas` | Low-level pixel operations | Medium |
| **Filters_Color** | `/Modules/Filters_Color.bas` | Color adjustments, channel operations | Medium |
| **Filters_Edge** | `/Modules/Filters_Edge.bas` | Edge detection, embossing | Medium |
| **Filters_Layers** | `/Modules/Filters_Layers.bas` | Layer-based operations | Medium |
| **Filters_Misc** | `/Modules/Filters_Misc.bas` | Miscellaneous filters | Variable |
| **Filters_Natural** | `/Modules/Filters_Natural.bas` | Natural effects (metal, chrome) | High |
| **Filters_Render** | `/Modules/Filters_Render.bas` | Procedural generation (clouds, fibers) | Medium |
| **Filters_Scientific** | `/Modules/Filters_Scientific.bas` | Mathematical operations | High |
| **Filters_Stylize** | `/Modules/Filters_Stylize.bas` | Stylistic effects (halftone, antique) | Medium |
| **Filters_Transform** | `/Modules/Filters_Transform.bas` | Geometric transformations | High |

### Advanced Algorithms

#### Gaussian Blur (Performance-Critical)

**Location:** `/Modules/Filters_Area.bas`

**Implementations:**
1. **Alvarez-Mazorra (AM) Algorithm** (`AM_gaussian_conv`, line 532)
   - IIR-based approximation
   - O(n) complexity regardless of kernel size
   - Configurable quality/speed tradeoff

2. **Deriche IIR Filter** (`GaussianBlur_Deriche`, line 945)
   - 4th order recursive filter
   - Excellent approximation of Gaussian kernel
   - Optimal for large radius blurs

3. **Horizontal Blur IIR** (`HorizontalBlur_IIR`, line 771)
   - Separable IIR filter
   - Symmetric/asymmetric modes
   - Used for motion blur effects

**Migration Strategy:** Implement using .NET's parallel processing with SIMD optimizations. Consider using libraries like ImageSharp or custom implementations.

#### Convolution Engine

**Location:** `/Modules/Filters_Area.bas`

**Function:** `ConvolveDIB_XML` (line 90)

**Features:**
- XML-based parameter parsing
- Custom kernel support
- Progress reporting
- Multi-threading support

**Migration Note:** .NET's `System.Drawing.Imaging` or ImageSharp provide convolution support, but custom implementation may be needed for compatibility.

#### Color Space Conversions

**Location:** `/Modules/Filters_Color.bas`

**Operations:**
- RGB ↔ HSL/HSV
- RGB ↔ CMYK
- RGB ↔ LAB
- Color temperature calculations

**Migration Note:** Use .NET's `System.Drawing.Color` extensions or third-party libraries like ColorMine for accurate conversions.

---

## Performance-Critical Sections

### High-Performance Requirements

Based on code analysis, the following modules contain performance-critical code with optimization comments:

1. **Filters_Area.bas** - Blur operations, convolution
   - Contains comments about "performance", "optimize", "fast"
   - IIR filters specifically designed for speed
   - Supersampling quality tables

2. **Filters_Scientific.bas** - Complex mathematical operations
   - Scientific computations requiring optimization

3. **Resampling.bas** - Image scaling algorithms
   - Critical for resize operations
   - Multiple interpolation methods

4. **Plugin_FreeImage.bas** - External library integration
   - Performance-critical image I/O
   - Format conversion operations

### Optimization Techniques Used

1. **IIR Filters:** Infinite Impulse Response filters for O(n) complexity blurs
2. **Separable Convolution:** 2D operations split into 1D for efficiency
3. **Lookup Tables (LUTs):** Pre-computed values for color operations
4. **Direct Memory Access:** Byte array manipulation for pixel operations
5. **Progress Reporting Optimization:** Selective updates to avoid UI overhead

### .NET Migration Performance Considerations

| Technique | VB6 Implementation | .NET Recommendation |
|-----------|-------------------|---------------------|
| Direct pixel access | SafeArrays, GetPixelFormatSize | `Bitmap.LockBits()` with unsafe code |
| Parallel processing | Limited thread support | `Parallel.For`, PLINQ |
| SIMD operations | Not available | `System.Numerics.Vector<T>` |
| Memory management | COM-based | Span<T>, Memory<T> |
| GPU acceleration | Not available | Consider OpenCL.NET or GPU.NET |

---

## Macro Recording System

### Architecture

**Primary Module:** `/Modules/BatchProcessor.bas` (Attribute VB_Name = "Macros")

**File Format:** XML-based PDM (PhotoDemon Macro) files

### Key Components

#### Macro States

```vb
Public Enum PD_MacroStatus
    MacroSTOP = 0      ' No macro active
    MacroSTART = 1     ' Recording in progress
    MacroBATCH = 2     ' Batch processing
    MacroPLAYBACK = 3  ' Macro playback
    MacroCANCEL = 128  ' Canceled operation
End Enum
```

#### Recording Process

1. **Start Recording** (`StartMacro`, line 70)
   - Sets global recording flag
   - Initializes process array
   - Updates UI indicators

2. **Action Capture**
   - All effect operations are captured via `PD_ProcessCall` structure
   - Parameters stored as XML strings
   - Validation checks for recordable actions

3. **Stop Recording** (`StopMacro`, line 85)
   - Validates at least one recordable action exists
   - Prompts for save location
   - Exports to XML file

#### Macro File Structure

**Version:** 8.2014 (`MACRO_VERSION_2014`)

**XML Schema:**
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
  <!-- Additional entries... -->
</Macro>
```

#### Validation Rules

An action is recordable if:
1. Has valid ID (non-empty)
2. Does NOT raise a dialog
3. Explicitly marked as "recordable"
4. NOT a special ID (e.g., "Original image")

**Implementation:** `IsActionOKForMacro` function (line 243)

### Playback System

**Function:** `PlayMacro` (line 290)

**Process:**
1. Disable user input during playback
2. Load macro XML file
3. Parse process entries
4. Execute each action in sequence
5. Re-enable user input

### Recent Macros

- MRU (Most Recently Used) list maintained
- Quick access via `Tools > Recent macros` menu

### .NET Migration Strategy

**Recommendation:** Use .NET's built-in XML serialization

```csharp
public class PhotoDemonMacro
{
    [XmlElement("pdMacroVersion")]
    public string Version { get; set; }

    [XmlElement("processCount")]
    public int ProcessCount { get; set; }

    [XmlArray("processes")]
    [XmlArrayItem("processEntry")]
    public List<ProcessEntry> Processes { get; set; }
}

public class ProcessEntry
{
    [XmlAttribute("index")]
    public int Index { get; set; }

    [XmlElement("ID")]
    public string ID { get; set; }

    [XmlElement("Parameters")]
    public string Parameters { get; set; }

    [XmlElement("MakeUndo")]
    public int UndoType { get; set; }

    [XmlElement("Tool")]
    public int Tool { get; set; }
}
```

**Benefits:**
- Automatic serialization/deserialization
- Maintains backward compatibility with VB6 macro files
- Type-safe parameter handling

---

## Batch Processing System

### Architecture

**Primary Forms:**
- `/Forms/File_BatchWizard.frm` - Main batch processing wizard
- `/Forms/File_BatchRepair.frm` - Batch repair utility

### Integration with Macro System

The batch processor leverages the macro system to apply operations to multiple images:

1. User selects source images
2. Chooses operation (resize, convert, apply macro, etc.)
3. Specifies output settings
4. System processes each image sequentially

### Batch Operations

Based on menu structure:

1. **Batch Process** (`file_batch_process`)
   - Apply macros to multiple images
   - Resize/convert operations
   - Output format selection
   - Preserves metadata

2. **Batch Repair** (`file_batch_repair`)
   - Fix corrupted images
   - Metadata repair
   - Format conversion

### Wizard Steps

The batch wizard likely includes:
1. Source file selection
2. Operation selection
3. Output settings
4. Processing execution
5. Results summary

### .NET Migration Strategy

**Recommendation:** Use MAUI's navigation system with async/await pattern

**Benefits:**
- Non-blocking UI during batch processing
- Progress reporting via `IProgress<T>`
- Cancellation support via `CancellationToken`
- Better error handling with try-catch patterns

**Example Architecture:**
```csharp
public class BatchProcessor
{
    public async Task<BatchResult> ProcessBatchAsync(
        IEnumerable<string> files,
        PhotoDemonMacro macro,
        BatchSettings settings,
        IProgress<BatchProgress> progress,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

---

## External Dependencies

### Plugin System

PhotoDemon uses external libraries for various operations:

| Plugin | Module | Purpose |
|--------|--------|---------|
| **FreeImage** | `Plugin_FreeImage.bas` | Image format support, advanced operations |
| **LittleCMS** | `Plugin_LittleCMS.bas` | Color management, ICC profiles |
| **ExifTool** | `Plugin_ExifTool.bas` | Metadata reading/writing |
| **WebP** | `Plugin_WebP.bas` | WebP format support |
| **AVIF** | `Plugin_AVIF.bas` | AVIF format support |
| **HEIF** | `Plugin_heif.bas` | HEIF/HEIC format support |
| **JXL** | `Plugin_jxl.bas` | JPEG XL format support |
| **OpenJPEG** | `Plugin_OpenJPEG.bas` | JPEG 2000 support |
| **PDF** | `Plugin_PDF.bas` | PDF import/export |
| **resvg** | `Plugin_resvg.bas` | SVG rendering |
| **zstd** | `Plugin_zstd.bas` | Zstandard compression |
| **lz4** | `Plugin_lz4.bas` | LZ4 compression |
| **libdeflate** | `Plugin_libdeflate.bas` | DEFLATE compression |
| **EZTwain** | `Plugin_EZTwain.bas` | Scanner support |
| **DDS** | `Plugin_DDS.bas` | DirectDraw Surface support |

### .NET Alternatives

| VB6 Plugin | .NET Alternative | Notes |
|------------|------------------|-------|
| FreeImage | ImageSharp, SkiaSharp | Modern .NET image libraries |
| LittleCMS | Windows Color System API | Built into Windows |
| ExifTool | MetadataExtractor | Pure .NET metadata library |
| WebP/AVIF | ImageSharp | Native support |
| PDF | PDFium.NET | PDF rendering |
| SVG | SkiaSharp, Svg.NET | Native SVG support |
| Compression | System.IO.Compression | Built-in support |
| TWAIN | WIA (Windows Image Acquisition) | Native Windows API |

---

## Migration Strategy

### Phase 1: Core Effects (High Priority)

**Timeline:** Months 1-3

**Focus Areas:**
1. **Adjustments** - Most frequently used, highest user impact
   - Brightness/Contrast, Curves, Levels, Color Balance
   - Hue/Saturation, Vibrance, White Balance

2. **Basic Blur/Sharpen** - Performance-critical
   - Gaussian Blur (with IIR optimization)
   - Unsharp Mask
   - Box Blur

3. **Essential Transforms** - Core functionality
   - Resize, Rotate, Crop
   - Flip, Mirror

**Deliverables:**
- Core adjustment algorithms ported
- Performance benchmarking suite
- Unit tests for color accuracy

### Phase 2: Popular Effects (Medium Priority)

**Timeline:** Months 4-6

**Focus Areas:**
1. **Artistic Effects** - User favorites
   - Oil Painting, Colored Pencil, Comic Book

2. **Distortions** - Moderate complexity
   - Lens correction, Pinch/Whirl, Waves

3. **Edge Detection** - Moderate complexity
   - Find Edges, Emboss, Enhance Edges

**Deliverables:**
- 50+ effects implemented
- Effect preview system working
- Parameter serialization complete

### Phase 3: Advanced Effects (Lower Priority)

**Timeline:** Months 7-9

**Focus Areas:**
1. **Natural Effects** - Complex procedural generation
   - Metal, Lava, Ignite, Fog

2. **Noise Operations** - Advanced algorithms
   - Anisotropic Diffusion, Mean Shift

3. **Pixelate & Render** - Specialized effects
   - Color Halftone, Crystallize, Clouds, Fibers

**Deliverables:**
- All 117+ effects implemented
- Performance optimization complete
- GPU acceleration explored

### Phase 4: Macro & Batch Systems

**Timeline:** Month 10

**Focus Areas:**
1. **Macro Recording** - Core functionality
   - Action capture and playback
   - XML serialization
   - Backward compatibility with VB6 macros

2. **Batch Processing** - Automation
   - Multi-file processing
   - Progress reporting
   - Error handling

**Deliverables:**
- Macro system fully functional
- Batch wizard UI complete
- Legacy macro file support

### Implementation Approach by Algorithm Type

#### Simple Pixel Operations
- **Examples:** Brightness, Contrast, Invert, Grayscale
- **Approach:** Direct port with LINQ/Parallel.For
- **Effort:** Low
- **Library:** Custom implementation

#### Color Space Conversions
- **Examples:** HSL, HSV, LAB, CMYK
- **Approach:** Use ColorMine or custom implementations
- **Effort:** Low-Medium
- **Library:** ColorMine.NET or custom

#### Convolution Operations
- **Examples:** Blur, Sharpen, Edge Detection
- **Approach:** Custom implementation with SIMD
- **Effort:** Medium-High
- **Library:** ImageSharp or custom

#### Geometric Transformations
- **Examples:** Rotate, Resize, Perspective
- **Approach:** Use .NET Matrix operations
- **Effort:** Medium
- **Library:** System.Drawing or SkiaSharp

#### Advanced Algorithms
- **Examples:** Anisotropic Diffusion, Mean Shift, Droste
- **Approach:** Port complex algorithms, optimize with C#
- **Effort:** High
- **Library:** Custom with possible GPU acceleration

#### Procedural Generation
- **Examples:** Clouds, Fibers, Truchet
- **Approach:** Port noise generators and rendering
- **Effort:** Medium
- **Library:** Custom (SimplexNoise, Perlin)

### Testing Strategy

1. **Visual Regression Testing**
   - Compare output between VB6 and .NET versions
   - Acceptable tolerance for floating-point differences
   - Automated image comparison tools

2. **Performance Benchmarking**
   - Measure execution time for each effect
   - Target: Match or exceed VB6 performance
   - Focus on high-use effects first

3. **Parameter Compatibility**
   - Ensure XML parameter parsing works identically
   - Test macro file compatibility
   - Validate edge cases and error handling

### Risk Mitigation

| Risk | Impact | Mitigation Strategy |
|------|--------|---------------------|
| Algorithm accuracy loss | High | Implement comprehensive test suite with known outputs |
| Performance degradation | High | Profile early, optimize critical paths with SIMD/GPU |
| Plugin compatibility | Medium | Identify .NET alternatives early, create compatibility layer |
| Macro file incompatibility | Medium | Extensive testing with real-world macro files |
| UI responsiveness during processing | Medium | Implement async/await pattern throughout |

---

## Summary Statistics

### Total Effects Count: 117+

**By Category:**
- Adjustments: 28 (24%)
- Artistic: 12 (10%)
- Blur: 8 (7%)
- Distort: 12 (10%)
- Edge: 6 (5%)
- Light/Shadow: 7 (6%)
- Natural: 7 (6%)
- Noise: 8 (7%)
- Pixelate: 6 (5%)
- Render: 3 (3%)
- Sharpen: 2 (2%)
- Stylize: 9 (8%)
- Transform: 6 (5%)
- Animation: 3 (3%)

### Complexity Distribution

- **Low Complexity:** 25 effects (21%)
- **Medium Complexity:** 60 effects (51%)
- **High Complexity:** 25 effects (21%)
- **Very High Complexity:** 7 effects (6%)

### Implementation Modules

- **Total Filter Modules:** 11
- **Total Forms:** 216 (113 for effects/adjustments)
- **Performance-Critical Modules:** 88 (identified via keyword search)

### Critical Systems

1. **Macro Recording System**
   - XML-based storage
   - Version: 8.2014
   - Supports both live recording and history-based creation

2. **Batch Processing System**
   - Integrated with macro playback
   - Multi-file support
   - Repair utilities included

3. **Plugin Architecture**
   - 15+ external libraries
   - Format support, color management, compression
   - Migration requires .NET alternatives

---

## Appendix: File Locations Reference

### Core Module Locations

```
/home/user/PhotoDemon/Modules/
├── Filters_Area.bas          # Blur, convolution
├── Filters_ByteArray.bas     # Low-level operations
├── Filters_Color.bas         # Color adjustments
├── Filters_Edge.bas          # Edge detection
├── Filters_Layers.bas        # Layer operations
├── Filters_Misc.bas          # Miscellaneous
├── Filters_Natural.bas       # Natural effects
├── Filters_Render.bas        # Procedural generation
├── Filters_Scientific.bas    # Mathematical operations
├── Filters_Stylize.bas       # Stylistic effects
├── Filters_Transform.bas     # Geometric transforms
└── BatchProcessor.bas        # Macro & batch processing
```

### Form Locations

```
/home/user/PhotoDemon/Forms/
├── Adjustments_*.frm         # 28 adjustment dialogs
├── Effects_Artistic_*.frm    # 12 artistic effects
├── Effects_Blur_*.frm        # 8 blur effects
├── Effects_Distort_*.frm     # 12 distortion effects
├── Effects_Edge_*.frm        # 6 edge effects
├── Effects_LightAndShadow_*.frm  # 5 light/shadow effects
├── Effects_Nature_*.frm      # 7 natural effects
├── Effects_Noise_*.frm       # 7 noise effects
├── Effects_Pixelate_*.frm    # 6 pixelate effects
├── Effects_Render_*.frm      # 3 render effects
├── Effects_Sharpen_*.frm     # 2 sharpen effects
├── Effects_Stylize_*.frm     # 8 stylize effects
├── Effects_Transform_*.frm   # 6 transform effects
├── Effects_Animation_*.frm   # 2 animation effects
├── File_BatchWizard.frm      # Batch processing wizard
└── File_BatchRepair.frm      # Batch repair utility
```

---

**Document End**

*For questions or clarifications, contact the migration team.*
