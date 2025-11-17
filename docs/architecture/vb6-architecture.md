# PhotoDemon VB6 Architecture Documentation

**Document Version:** 1.0
**Generated:** 2025-11-17
**PhotoDemon Version:** 10.0 (Alpha)
**Migration Target:** .NET 10 MAUI

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Architecture Overview](#architecture-overview)
3. [Codebase Statistics](#codebase-statistics)
4. [Core Subsystems](#core-subsystems)
5. [Modules (114 files)](#modules-114-files)
6. [Classes (141 files)](#classes-141-files)
7. [Forms (216 files)](#forms-216-files)
8. [Controls (56 files)](#controls-56-files)
9. [Dependency Graph](#dependency-graph)
10. [Key Architectural Patterns](#key-architectural-patterns)
11. [Migration Considerations](#migration-considerations)

---

## Executive Summary

PhotoDemon is a sophisticated image editing application built in VB6, consisting of:
- **114 Modules** (.bas files) - Business logic and utilities
- **141 Classes** (.cls files) - Object-oriented components
- **216 Forms** (.frm files) - User interface dialogs and windows
- **56 Controls** (.ctl files) - Custom UI components

The architecture is organized into distinct subsystems for rendering, file I/O, effects, UI, tools, and plugin management. The codebase demonstrates mature software engineering practices including separation of concerns, comprehensive error handling, and extensive plugin architecture.

---

## Architecture Overview

### High-Level Architecture

PhotoDemon follows a modular, layered architecture:

```
┌─────────────────────────────────────────────────────┐
│              User Interface Layer                    │
│  (Forms, Controls, Toolbars, Layer Panels)          │
└─────────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────────┐
│           Application Services Layer                 │
│  (Processor, Actions, Menus, Interface)             │
└─────────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────────┐
│              Core Business Logic Layer               │
│  ┌─────────────┬──────────────┬──────────────┐     │
│  │  Rendering  │   File I/O   │   Effects    │     │
│  │   Engine    │   Engine     │   Engine     │     │
│  └─────────────┴──────────────┴──────────────┘     │
└─────────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────────┐
│              Data Model Layer                        │
│  (pdImage, pdLayer, pdDIB, pdSelection)             │
└─────────────────────────────────────────────────────┘
                        │
┌─────────────────────────────────────────────────────┐
│         Platform & Plugin Layer                      │
│  (GDI/GDI+, WIC, Plugins, External Libraries)       │
└─────────────────────────────────────────────────────┘
```

### Entry Point

**File:** `/Modules/Main.bas`
**Function:** `Main()`
**Line:** 64

The application starts in `PDMain.Main()`, which:
1. Initializes common controls (shell32.dll, comctl32.dll)
2. Loads the splash screen
3. Initializes subsystems (19 discrete loading steps)
4. Loads the main window (FormMain)
5. Processes command-line arguments

### Global State Management

PhotoDemon uses three key modules for global state:

1. **PublicConstants.bas** - Application constants and build configuration
   - Build quality flags (ALPHA, BETA, PRODUCTION)
   - Magic numbers for file formats
   - Virtual key codes
   - Maximum image dimensions (100,000 pixels)

2. **PublicVars.bas** - Global variables and shared state
   - Core service instances (g_Language, g_Clipboard, g_Resources)
   - Current tool state (g_CurrentTool, g_PreviousTool)
   - Display management (g_Displays)
   - Window management (g_WindowManager)
   - Theme engine (g_Themer)
   - Performance settings (g_ViewportPerformance, g_InterfacePerformance)

3. **PublicEnumsAndTypes.bas** - Type definitions and enumerations
   - Common data structures
   - API type definitions
   - Application-wide enumerations

---

## Codebase Statistics

### File Counts

| Category | Count | Purpose |
|----------|-------|---------|
| Modules (.bas) | 114 | Business logic, utilities, API wrappers |
| Classes (.cls) | 141 | Object-oriented components |
| Forms (.frm) | 216 | User interface dialogs |
| Controls (.ctl) | 56 | Custom UI components |
| **Total** | **527** | **VB6 source files** |

### Lines of Code (Estimated)

Based on sampling, PhotoDemon contains approximately:
- **250,000+ lines** of VB6 source code
- Extensive inline documentation and comments
- Comprehensive error handling throughout

---

## Core Subsystems

PhotoDemon is organized into 11 major subsystems:

### 1. Rendering Engine

**Purpose:** High-performance 2D rendering with multiple backend support

**Key Components:**
- **PD2D.bas** (/Modules/PD2D.bas:1) - Unified 2D painting interface
- **pd2DSurface.cls** (/Classes/pd2DSurface.cls:1) - Surface abstraction
- **pd2DBrush.cls** (/Classes/pd2DBrush.cls:1) - Brush objects
- **pd2DPen.cls** (/Classes/pd2DPen.cls:1) - Pen objects
- **pd2DPath.cls** (/Classes/pd2DPath.cls:1) - Vector paths
- **pd2DGradient.cls** (/Classes/pd2DGradient.cls:1) - Gradient fills
- **pd2DTransform.cls** (/Classes/pd2DTransform.cls:1) - Affine transformations
- **pdCompositor.cls** (/Classes/pdCompositor.cls:1) - Layer compositing engine
- **pdPixelBlender.cls** (/Classes/pdPixelBlender.cls:1) - Blend mode implementation

**Backend Support:**
- **GDI.bas** - Windows GDI rendering
- **GDIPlus.bas** - Windows GDI+ rendering
- **Drawing.bas** - High-level drawing functions
- **Drawing2D.bas** - 2D drawing utilities

**Architecture Notes:**
- Abstraction layer over GDI/GDI+ for performance optimization
- Falls back to bare AlphaBlend calls when possible for speed
- Supports both integer (I) and floating-point (F) coordinate systems
- Implements custom blend modes and compositing operations

### 2. Image Data Model

**Purpose:** Core data structures for image representation

**Key Components:**
- **pdImage.cls** (/Classes/pdImage.cls:1) - Top-level image container
  - Manages image-level properties (width, height, DPI)
  - Contains layer collection
  - Handles metadata (pdMetadata)
  - Manages undo/redo (pdUndo)
  - Viewport management (pdViewport)
  - Composite buffer caching

- **pdLayer.cls** (/Classes/pdLayer.cls:1) - Individual layer representation
  - Layer types: raster, text, vector, adjustment
  - Layer properties: opacity, blend mode, transforms
  - Non-destructive transformations (rotation, shear)
  - Layer masks support

- **pdDIB.cls** (/Classes/pdDIB.cls:1) - Device Independent Bitmap wrapper
  - Core pixel storage (24-bit and 32-bit)
  - GDI-compatible DC management
  - Memory/disk suspension for large images
  - Automatic decompression on access

**Data Flow:**
```
pdImage (Container)
  ├─> Layer Collection (pdLayer[])
  │     └─> LayerDIB (pdDIB)
  ├─> CompositeBuffer (pdDIB)
  ├─> CanvasBuffer (pdDIB)
  ├─> ScratchLayer (pdLayer)
  ├─> MainSelection (pdSelection)
  ├─> ImgMetadata (pdMetadata)
  └─> UndoManager (pdUndo)
```

### 3. File I/O Subsystem

**Purpose:** Comprehensive image format support

**Modules:**
- **ImageLoader.bas** (/Modules/ImageLoader.bas:1) - Low-level import
- **ImageExporter.bas** (/Modules/ImageExporter.bas:1) - Low-level export
- **Loading.bas** (/Modules/Loading.bas:1) - High-level load coordinator
- **Saving.bas** (/Modules/Saving.bas:1) - High-level save coordinator
- **ImageFormats.bas** (/Modules/ImageFormats.bas:1) - Format detection/support

**Native Format Parsers:**
PhotoDemon includes pure VB6 parsers for:
- **pdPNG.cls** - PNG (Portable Network Graphics)
- **pdPSD.cls** - Photoshop PSD/PSB files
- **pdPSP.cls** - PaintShop Pro PSP files
- **pdGIF.cls** - GIF (Graphics Interchange Format)
- **pdICO.cls** - Windows Icons
- **pdPCX.cls** - PC Paintbrush
- **pdQOI.cls** - Quite OK Image format
- **pdWBMP.cls** - Wireless Bitmap
- **pdXBM.cls** - X Bitmap
- **pdXCF.cls** - GIMP XCF format
- **pdOpenRaster.cls** - OpenRaster (.ora)
- **pdCBZ.cls** - Comic Book Archive
- **pdHGT.cls** - SRTM Height Map
- **pdMBM.cls** - Symbian MBM format

**PDI (PhotoDemon Image) Format:**
- Native format for full layer/metadata preservation
- **pdPackageChunky.cls** - Modern chunked format (PNG-like)
- **pdPackager2.cls** - Version 2 packaging
- **pdPackagerLegacy.cls** - Legacy format support
- **PDPackaging.bas** - High-level packaging functions

**External Library Support:**
- See Plugin Subsystem for FreeImage, libwebp, etc.

### 4. Effects Engine

**Purpose:** Image manipulation and effects processing

**Filter Modules (11 categories):**
- **Filters_Area.bas** - Area-based filters (blur, sharpen)
- **Filters_ByteArray.bas** - Direct byte array manipulation
- **Filters_Color.bas** - Color adjustments
- **Filters_Edge.bas** - Edge detection and enhancement
- **Filters_Layers.bas** - Layer-specific filters
- **Filters_Misc.bas** - Miscellaneous effects
- **Filters_Natural.bas** - Natural media effects
- **Filters_Render.bas** - Procedural rendering
- **Filters_Scientific.bas** - Scientific/mathematical filters
- **Filters_Stylize.bas** - Stylization effects
- **Filters_Transform.bas** - Geometric transformations

**Effect Support Classes:**
- **pdFilterSupport.cls** (/Classes/pdFilterSupport.cls:1) - Effect pipeline support
- **pdFilterLUT.cls** (/Classes/pdFilterLUT.cls:1) - Lookup table filters
- **pdLUT3D.cls** (/Classes/pdLUT3D.cls:1) - 3D LUT support
- **pdFFT.cls** (/Classes/pdFFT.cls:1) - Fast Fourier Transform
- **pdFxBilateral.cls** (/Classes/pdFxBilateral.cls:1) - Bilateral filter

**Forms (140+ effect dialogs):**
Effects are organized into categories:
- **Adjustments_*** (28 forms) - Color/tone adjustments
- **Effects_Artistic_*** (13 forms) - Artistic effects
- **Effects_Blur_*** (8 forms) - Blur effects
- **Effects_Distort_*** (11 forms) - Distortion effects
- **Effects_Edge_*** (6 forms) - Edge effects
- **Effects_LightAndShadow_*** (5 forms) - Lighting effects
- **Effects_Nature_*** (7 forms) - Natural media
- **Effects_Noise_*** (7 forms) - Noise filters
- **Effects_Pixelate_*** (6 forms) - Pixelation effects
- **Effects_Render_*** (3 forms) - Render effects
- **Effects_Sharpen_*** (2 forms) - Sharpening
- **Effects_Stylize_*** (8 forms) - Stylization
- **Effects_Transform_*** (6 forms) - Transformations

**Effect Processing:**
- **Processor.bas** (/Modules/Processor.bas:1) - Central effect dispatcher
  - Macro recording
  - Undo/Redo creation
  - Progress reporting
  - Error handling
- **EffectPrep.bas** (/Modules/EffectPrep.bas:1) - Effect preparation

### 5. Selection Subsystem

**Purpose:** Complex selection tools and operations

**Key Components:**
- **pdSelection.cls** (/Classes/pdSelection.cls:1) - Selection management
  - Selection shapes: rectangle, ellipse, polygon, lasso, wand
  - Composite selections (add, subtract, intersect)
  - Feathering and border modes
  - Selection masks and rendering

**Modules:**
- **Selections.bas** (/Modules/Selections.bas:1) - Selection operations
- **SelectionUI.bas** (/Modules/SelectionUI.bas:1) - Selection UI rendering
- **SelectionFiles.bas** (/Modules/SelectionFiles.bas:1) - Save/load selections
- **SelectionFilters.bas** (/Modules/SelectionFilters.bas:1) - Selection modifications

**Supporting Classes:**
- **pdFloodFill.cls** (/Classes/pdFloodFill.cls:1) - Magic wand flood fill
- **pdEdgeDetector.cls** (/Classes/pdEdgeDetector.cls:1) - Edge-based selections

### 6. Tool Subsystem

**Purpose:** Interactive editing tools

**Tool Modules:**
- **Tools.bas** (/Modules/Tools.bas:1) - Tool management
- **Paintbrush.bas** (/Modules/Paintbrush.bas:1) - Paintbrush tool
- **PencilTool.bas** (/Modules/PencilTool.bas:1) - Pencil tool
- **FillTool.bas** (/Modules/FillTool.bas:1) - Fill/bucket tool
- **GradientTool.bas** (/Modules/GradientTool.bas:1) - Gradient tool
- **Clonestamp.bas** (/Modules/Clonestamp.bas:1) - Clone stamp tool
- **MoveTool.bas** (/Modules/MoveTool.bas:1) - Move/transform tool
- **MeasureTool.bas** (/Modules/MeasureTool.bas:1) - Measurement tool
- **ZoomTool.bas** (/Modules/ZoomTool.bas:1) - Zoom tool
- **Tools_Crop.bas** (/Modules/Tools_Crop.bas:1) - Crop tool

**Tool Panels (Forms):**
- **Toolpanel_Paintbrush.frm** - Paintbrush options
- **Toolpanel_Pencil.frm** - Pencil options
- **Toolpanel_Fill.frm** - Fill options
- **Toolpanel_Gradient.frm** - Gradient options
- **Toolpanel_Clone.frm** - Clone stamp options
- **Toolpanel_MoveSize.frm** - Move/size options
- **Toolpanel_Crop.frm** - Crop options
- **Toolpanel_Selections.frm** - Selection tool options
- **Toolpanel_TextBasic.frm** - Basic text options
- **Toolpanel_Typography.frm** - Advanced typography
- **Toolpanel_Measure.frm** - Measurement options
- **Toolpanel_ColorPicker.frm** - Color picker options
- **Toolpanel_Eraser.frm** - Eraser options

**Supporting Classes:**
- **pdPaintbrush.cls** (/Classes/pdPaintbrush.cls:1) - Paintbrush engine
- **pdInputMouse.cls** (/Classes/pdInputMouse.cls:1) - Mouse input
- **pdInputKeyboard.cls** (/Classes/pdInputKeyboard.cls:1) - Keyboard input

### 7. Undo/Redo System

**Purpose:** Unlimited undo/redo with diff-based storage

**Key Components:**
- **pdUndo.cls** (/Classes/pdUndo.cls:1) - Undo/redo manager
  - Diff-based storage (like Git)
  - HDD-backed (unlimited history)
  - Saves only changed data
  - Supports multiple undo types

**Architecture:**
- Each pdImage contains its own pdUndo instance
- Undo data stored as PDI files in temp directory
- Tracks both flat and layered save states
- Integrates with macro recording

### 8. Plugin Subsystem

**Purpose:** External library integration for extended functionality

**Plugin Manager:**
- **Plugin_Management.bas** (/Modules/Plugin_Management.bas:1)
  - 18 core plugins supported
  - Version checking and validation
  - Graceful degradation if plugins unavailable

**Plugin Modules (18 plugins):**

**Image Format Plugins:**
- **Plugin_FreeImage.bas** - FreeImage library (3.19.0)
  - 100+ image format support
  - Advanced loading/saving

- **Plugin_WebP.bas** - WebP format (1.5.0)
- **Plugin_AVIF.bas** - AVIF format (1.2.0)
- **Plugin_heif.bas** - HEIF/HEIC format (1.17.6)
- **Plugin_jxl.bas** - JPEG XL format (0.11.1)
- **Plugin_OpenJPEG.bas** - JPEG 2000 format (2.5)
- **Plugin_CharLS.bas** - JPEG-LS lossless (2.4.2)
- **Plugin_DDS.bas** - DirectX texture format (DirectXTex 2024.10.29)
- **Plugin_PDF.bas** - PDF rendering (PDFium 136.0.7073)
- **Plugin_resvg.bas** - SVG rendering (0.45.0)

**Compression Plugins:**
- **Plugin_lz4.bas** - LZ4 compression (10904)
- **Plugin_zstd.bas** - Zstandard compression (10507)
- **Plugin_libdeflate.bas** - DEFLATE compression (1.23)

**Other Plugins:**
- **Plugin_LittleCMS.bas** - Color management (2.16.0)
- **Plugin_ExifTool.bas** - Metadata handling (12.70)
- **Plugin_EZTwain.bas** - Scanner support (1.18.0)
- **Plugin_8bf.bas** - Photoshop plugin support (pspiHost 0.9)

**Metadata:**
- **pdMetadata.cls** (/Classes/pdMetadata.cls:1) - Metadata container
- Requires ExifTool plugin for full functionality

### 9. User Interface Subsystem

**Purpose:** Custom-drawn, theme-able UI framework

**UI Management:**
- **Interface.bas** (/Modules/Interface.bas:1) - UI coordination
- **Menus.bas** (/Modules/Menus.bas:1) - Menu management
- **DialogManager.bas** (/Modules/DialogManager.bas:1) - Dialog handling
- **Toolboxes.bas** (/Modules/Toolboxes.bas:1) - Toolbox management
- **CanvasManager.bas** (/Modules/CanvasManager.bas:1) - Canvas rendering

**Window Management:**
- **pdWindowManager.cls** (/Classes/pdWindowManager.cls:1) - Window layout
- **pdWindowPainter.cls** (/Classes/pdWindowPainter.cls:1) - Window rendering
- **pdWindowSize.cls** (/Classes/pdWindowSize.cls:1) - Window sizing
- **pdWindowSync.cls** (/Classes/pdWindowSync.cls:1) - Window synchronization

**Theme System:**
- **pdTheme.cls** (referenced in PublicVars.bas:72) - Theme engine
- **pdThemeColors.cls** (/Classes/pdThemeColors.cls:1) - Color schemes
- **pdVisualThemes.cls** (/Classes/pdVisualThemes.cls:1) - Visual theming

**Custom Controls (56 total):**

**Input Controls:**
- **pdButton.ctl** - Custom button
- **pdCheckBox.ctl** - Custom checkbox
- **pdRadioButton.ctl** - Custom radio button
- **pdTextBox.ctl** - Custom text box
- **pdSlider.ctl** - Slider control
- **pdSliderStandalone.ctl** - Standalone slider
- **pdSpinner.ctl** - Numeric spinner
- **pdDropDown.ctl** - Dropdown list
- **pdDropDownFont.ctl** - Font picker dropdown
- **pdSearchBar.ctl** - Search input

**Display Controls:**
- **pdLabel.ctl** - Custom label
- **pdTitle.ctl** - Title bar
- **pdProgressBar.ctl** - Progress indicator
- **pdStatusBar.ctl** - Status bar
- **pdHyperlink.ctl** - Clickable link

**Container Controls:**
- **pdContainer.ctl** - Generic container
- **pdButtonStrip.ctl** - Horizontal button strip
- **pdButtonStripVertical.ctl** - Vertical button strip
- **pdStrip.ctl** - Generic strip container

**Specialized Controls:**
- **pdCanvas.ctl** - Main image canvas
- **pdCanvasView.ctl** - Canvas viewport
- **pdCommandBar.ctl** - OK/Cancel/Reset bar
- **pdCommandBarMini.ctl** - Mini command bar
- **pdColorSelector.ctl** - Color picker
- **pdColorWheel.ctl** - HSV color wheel
- **pdColorVariants.ctl** - Color variations
- **pdBrushSelector.ctl** - Brush picker
- **pdPenSelector.ctl** - Pen picker
- **pdGradientSelector.ctl** - Gradient picker
- **pdFxPreview.ctl** - Effect preview panel
- **pdPreview.ctl** - Generic preview
- **pdLayerList.ctl** - Layer list
- **pdLayerListInner.ctl** - Layer list inner
- **pdHistory.ctl** - Undo history list
- **pdNavigator.ctl** - Image navigator
- **pdNavigatorInner.ctl** - Navigator inner
- **pdPaletteUI.ctl** - Palette editor
- **pdResize.ctl** - Resize control
- **pdColorDepth.ctl** - Color depth selector
- **pdMetadataExport.ctl** - Metadata export UI
- **pdNewOld.ctl** - Before/after preview
- **pdRandomizeUI.ctl** - Randomize parameters
- **pdImageStrip.ctl** - Image thumbnail strip
- **pdPictureBox.ctl** - Picture box
- **pdPictureBoxInteractive.ctl** - Interactive picture box
- **pdListBox.ctl** - List box
- **pdListBoxOD.ctl** - Owner-draw list box
- **pdListBoxView.ctl** - List box view
- **pdListBoxViewOD.ctl** - Owner-draw list view
- **pdTreeviewOD.ctl** - Owner-draw tree view
- **pdTreeviewViewOD.ctl** - Tree view control
- **pdScrollBar.ctl** - Custom scrollbar
- **pdRuler.ctl** - Ruler control
- **pdAccelerator.ctl** - Keyboard accelerator
- **pdButtonToolbox.ctl** - Toolbox button
- **pdDownload.ctl** - Download progress

**Main Forms:**
- **MainWindow.frm** (/Forms/MainWindow.frm:1) - MDI-style main window
- **Startup_Splash.frm** - Splash screen

**Layer Panels:**
- **Layerpanel_Layers.frm** - Layer management
- **Layerpanel_Colors.frm** - Color panel
- **Layerpanel_Navigator.frm** - Navigator panel
- **Layerpanel_Search.frm** - Search panel

**Toolbars:**
- **Toolbar_ToolSelect.frm** - Tool selection toolbar
- **Toolbar_Layers.frm** - Layer operations toolbar
- **Toolbar_ToolOptionsContainer.frm** - Tool options container
- **Toolbar_Debug.frm** - Debug toolbar

### 10. Utilities and Support

**String Handling:**
- **Strings.bas** (/Modules/Strings.bas:1) - String utilities
- **pdString.cls** (/Classes/pdString.cls:1) - String builder
- **pdStringStack.cls** (/Classes/pdStringStack.cls:1) - String stack
- **pdStringHash.cls** (/Classes/pdStringHash.cls:1) - String hash table

**Math and Algorithms:**
- **PDMath.bas** (/Modules/PDMath.bas:1) - Math utilities
- **ComplexNumbers.bas** (/Modules/ComplexNumbers.bas:1) - Complex math
- **Evaluator.bas** (/Modules/Evaluator.bas:1) - Expression evaluator
- **Resampling.bas** (/Modules/Resampling.bas:1) - Image resampling

**Collections:**
- **pdDictionary.cls** (/Classes/pdDictionary.cls:1) - Key-value store
- **pdStack.cls** (/Classes/pdStack.cls:1) - Stack structure
- **pdObjectList.cls** (/Classes/pdObjectList.cls:1) - Object list
- **pdListSupport.cls** (/Classes/pdListSupport.cls:1) - List utilities
- **pdTreeSupport.cls** (/Classes/pdTreeSupport.cls:1) - Tree utilities

**Data Structures:**
- **pdKDTree.cls** (/Classes/pdKDTree.cls:1) - K-D tree for spatial queries
- **pdKDTreeNode.cls** (/Classes/pdKDTreeNode.cls:1) - K-D tree node
- **pdKDTreeArray.cls** (/Classes/pdKDTreeArray.cls:1) - K-D tree array
- **pdHashCoord.cls** (/Classes/pdHashCoord.cls:1) - Coordinate hash
- **pdHistogramHash.cls** (/Classes/pdHistogramHash.cls:1) - Histogram hash

**Color Processing:**
- **Colors.bas** (/Modules/Colors.bas:1) - Color utilities
- **ColorManagement_ICM.bas** (/Modules/ColorManagement_ICM.bas:1) - ICC color management
- **ColorPicker.bas** (/Modules/ColorPicker.bas:1) - Color picking
- **pdColorSearch.cls** (/Classes/pdColorSearch.cls:1) - Color space search
- **pdColorCount.cls** (/Classes/pdColorCount.cls:1) - Color counting
- **pdColorSearchNode.cls** (/Classes/pdColorSearchNode.cls:1) - Search tree node

**Color Management:**
- **pdICCProfile.cls** (/Classes/pdICCProfile.cls:1) - ICC profile
- **pdLCMSProfile.cls** (/Classes/pdLCMSProfile.cls:1) - LittleCMS profile
- **pdLCMSTransform.cls** (/Classes/pdLCMSTransform.cls:1) - Color transform

**Palettes and Quantization:**
- **Palettes.bas** (/Modules/Palettes.bas:1) - Palette management
- **pdPalette.cls** (/Classes/pdPalette.cls:1) - Palette class
- **pdPaletteChild.cls** (/Classes/pdPaletteChild.cls:1) - Palette entry
- **pdMedianCut.cls** (/Classes/pdMedianCut.cls:1) - Median cut quantization
- **pdNeuquant.cls** (/Classes/pdNeuquant.cls:1) - Neural quantization

**Text Rendering:**
- **TextSupport.bas** (/Modules/TextSupport.bas:1) - Text utilities
- **TextTools.bas** (/Modules/TextTools.bas:1) - Text tools
- **Fonts.bas** (/Modules/Fonts.bas:1) - Font management
- **Uniscribe.bas** (/Modules/Uniscribe.bas:1) - Complex text layout
- **pdFont.cls** (/Classes/pdFont.cls:1) - Font object
- **pdFontCollection.cls** (/Classes/pdFontCollection.cls:1) - Font collection
- **pdGlyphCollection.cls** (/Classes/pdGlyphCollection.cls:1) - Glyph cache
- **pdTextRenderer.cls** (/Classes/pdTextRenderer.cls:1) - Text rendering
- **pdUniscribe.cls** (/Classes/pdUniscribe.cls:1) - Uniscribe wrapper
- **pdUniscribeItem.cls** (/Classes/pdUniscribeItem.cls:1) - Uniscribe item

**File System:**
- **Files.bas** (/Modules/Files.bas:1) - File utilities
- **pdFSO.cls** (/Classes/pdFSO.cls:1) - File system object
- **pdFileMM.cls** (/Classes/pdFileMM.cls:1) - Memory-mapped files

**Serialization:**
- **pdSerialize.cls** (/Classes/pdSerialize.cls:1) - Object serialization
- **pdXML.cls** (/Classes/pdXML.cls:1) - XML parsing/writing
- **pdStream.cls** (/Classes/pdStream.cls:1) - Byte stream

**Compression:**
- **Compression.bas** (/Modules/Compression.bas:1) - Compression utilities
- **cZipArchive.cls** (/Classes/cZipArchive.cls:1) - ZIP archive support

**Cryptography:**
- **pdCrypto.cls** (/Classes/pdCrypto.cls:1) - Encryption/hashing

**Operating System:**
- **OS.bas** (/Modules/OS.bas:1) - OS utilities
- **VB_Hacks.bas** (/Modules/VB_Hacks.bas:1) - VB6 workarounds
- **Mutex.bas** (/Modules/Mutex.bas:1) - Mutex handling
- **pdMutex.cls** (/Classes/pdMutex.cls:1) - Mutex object

**Display Management:**
- **pdDisplay.cls** (/Classes/pdDisplay.cls:1) - Single display
- **pdDisplays.cls** (/Classes/pdDisplays.cls:1) - Multi-monitor support

**Miscellaneous:**
- **pdTimer.cls** (/Classes/pdTimer.cls:1) - Timer
- **pdTimerAnimation.cls** (/Classes/pdTimerAnimation.cls:1) - Animation timer
- **pdTimerCountdown.cls** (/Classes/pdTimerCountdown.cls:1) - Countdown timer
- **pdProfiler.cls** (/Classes/pdProfiler.cls:1) - Performance profiling
- **pdRandomize.cls** (/Classes/pdRandomize.cls:1) - Random number generation
- **pdNoise.cls** (/Classes/pdNoise.cls:1) - Noise generation
- **pdPoissonDisc.cls** (/Classes/pdPoissonDisc.cls:1) - Poisson disc sampling
- **pdVoronoi.cls** (/Classes/pdVoronoi.cls:1) - Voronoi diagrams

### 11. Specialized Algorithms

**Advanced Image Processing:**
- **pdInpaint.cls** (/Classes/pdInpaint.cls:1) - Content-aware inpainting
- **pdSeamCarving.cls** (/Classes/pdSeamCarving.cls:1) - Content-aware resize
- **pdFocusDetector.cls** (/Classes/pdFocusDetector.cls:1) - Focus detection
- **pdMakeTexture.cls** (/Classes/pdMakeTexture.cls:1) - Texture synthesis
- **pdSpriteSheet.cls** (/Classes/pdSpriteSheet.cls:1) - Sprite sheet generation

**Histograms:**
- **Histograms.bas** (/Modules/Histograms.bas:1) - Histogram utilities

**Pixel Iteration:**
- **pdPixelIterator.cls** (/Classes/pdPixelIterator.cls:1) - Fast pixel access

**Surfaces:**
- **pdSurfaceF.cls** (/Classes/pdSurfaceF.cls:1) - Floating-point surface

**Animation:**
- **Animation.bas** (/Modules/Animation.bas:1) - Animation support

---

## Modules (114 files)

Modules (.bas files) contain procedural code, global functions, and API declarations. They are organized by subsystem:

### Core Application (8 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| Main.bas | Application entry point | Main() - Program startup |
| Processor.bas | Central command dispatcher | Process() - All actions route through here |
| Actions.bas | Action coordination | Macro recording, batch operations |
| PublicConstants.bas | Application constants | Build flags, magic numbers, VK codes |
| PublicVars.bas | Global variables | Shared application state |
| PublicEnumsAndTypes.bas | Type definitions | Enums, structures, API types |
| Legacy.bas | Backward compatibility | Legacy file format support |
| Debug.bas | Debugging utilities | Log output, debug reports |

### Rendering (7 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| PD2D.bas | 2D painting interface | CopySurface, DrawCircle, FillRect |
| Drawing.bas | High-level drawing | Drawing primitives |
| Drawing2D.bas | 2D drawing utilities | 2D shape rendering |
| GDI.bas | GDI wrapper | BitBlt, StretchBlt, AlphaBlend |
| GDIPlus.bas | GDI+ wrapper | GDI+ graphics operations |
| DibSupport.bas | DIB utilities | DIB creation, manipulation |
| ViewportEngine.bas | Viewport rendering | Canvas rendering pipeline |

### File I/O (8 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| ImageLoader.bas | Image import | LoadPDI, LoadFromFile |
| ImageExporter.bas | Image export | ExportToFile, SavePDI |
| Loading.bas | Load coordination | High-level load orchestration |
| Saving.bas | Save coordination | High-level save orchestration |
| ImageFormats.bas | Format detection | IsFileSupported, GetFormatFromExtension |
| ImageFormats_GIF.bas | GIF format | GIF loading/saving |
| ImageFormats_GIF_LZW.bas | GIF compression | LZW compression for GIF |
| ImageFormats_PSP.bas | PSP format | PaintShop Pro format |
| PDPackaging.bas | PDI packaging | Package/unpackage PDI files |

### Effects and Filters (12 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| Filters_Area.bas | Area filters | Box blur, Gaussian blur |
| Filters_ByteArray.bas | Byte array filters | Direct pixel manipulation |
| Filters_Color.bas | Color filters | Hue, saturation, brightness |
| Filters_Edge.bas | Edge filters | Sobel, Prewitt, edge enhance |
| Filters_Layers.bas | Layer filters | Flatten, merge |
| Filters_Misc.bas | Miscellaneous | Various effects |
| Filters_Natural.bas | Natural media | Oil paint, watercolor |
| Filters_Render.bas | Render filters | Clouds, fibers, noise |
| Filters_Scientific.bas | Scientific | FFT, convolution |
| Filters_Stylize.bas | Stylize | Emboss, posterize |
| Filters_Transform.bas | Transforms | Rotate, scale, distort |
| EffectPrep.bas | Effect preparation | Pre-process images for effects |

### UI and Interface (12 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| Interface.bas | UI coordination | ShowPDDialog, UI state management |
| Menus.bas | Menu management | BuildMenu, UpdateMenuIcons |
| DialogManager.bas | Dialog handling | ShowDialog, PromptUser |
| CanvasManager.bas | Canvas management | Render canvas, scroll, zoom |
| Toolboxes.bas | Toolbox management | Show/hide toolboxes |
| ProgressBars.bas | Progress indication | SetProgBarVal, ResetProgressBar |
| UIImages.bas | UI image resources | Load UI icons, images |
| IconsAndCursors.bas | Icon/cursor mgmt | Load cursors, create icons |
| Hotkeys.bas | Keyboard shortcuts | RegisterHotkey, ProcessHotkey |
| NavKey.bas | Navigation keys | Arrow key navigation |
| UserControl_Support.bas | Control helpers | UC initialization helpers |
| Zoom.bas | Zoom management | ZoomIn, ZoomOut, FitToScreen |

### Tools (11 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| Tools.bas | Tool management | SelectTool, GetCurrentTool |
| Paintbrush.bas | Paintbrush tool | Brush stroke rendering |
| PencilTool.bas | Pencil tool | Pencil drawing |
| FillTool.bas | Fill/bucket tool | Flood fill operations |
| GradientTool.bas | Gradient tool | Gradient fills |
| Clonestamp.bas | Clone stamp | Clone source to target |
| MoveTool.bas | Move tool | Layer movement |
| MeasureTool.bas | Measure tool | Distance/angle measurement |
| ZoomTool.bas | Zoom tool | Interactive zoom |
| Tools_Crop.bas | Crop tool | Crop image/layer |
| ColorPicker.bas | Color picker | Pick colors from image |

### Selection (4 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| Selections.bas | Selection ops | CreateSelection, ModifySelection |
| SelectionUI.bas | Selection rendering | Draw selection outline |
| SelectionFiles.bas | Selection I/O | Save/load selections |
| SelectionFilters.bas | Selection filters | Feather, border, grow, shrink |

### Layer Management (2 modules)

| Module | Purpose | Key Functions |
|--------|---------|---------------|
| Layers.bas | Layer operations | AddLayer, DeleteLayer, MergeLayer |
| PDImages.bas | Image collection | Manage open images |

### Plugins (18 modules)

| Module | Purpose | Version |
|--------|---------|---------|
| Plugin_Management.bas | Plugin coordination | - |
| Plugin_FreeImage.bas | FreeImage library | 3.19.0 |
| Plugin_WebP.bas | WebP format | 1.5.0 |
| Plugin_AVIF.bas | AVIF format | 1.2.0 |
| Plugin_heif.bas | HEIF/HEIC | 1.17.6 |
| Plugin_jxl.bas | JPEG XL | 0.11.1 |
| Plugin_OpenJPEG.bas | JPEG 2000 | 2.5 |
| Plugin_CharLS.bas | JPEG-LS | 2.4.2 |
| Plugin_DDS.bas | DirectX textures | 2024.10.29 |
| Plugin_PDF.bas | PDF rendering | 136.0.7073 |
| Plugin_resvg.bas | SVG rendering | 0.45.0 |
| Plugin_LittleCMS.bas | Color management | 2.16.0 |
| Plugin_ExifTool.bas | Metadata | 12.70 |
| Plugin_EZTwain.bas | Scanner support | 1.18.0 |
| Plugin_8bf.bas | Photoshop plugins | 0.9 |
| Plugin_lz4.bas | LZ4 compression | 10904 |
| Plugin_zstd.bas | Zstd compression | 10507 |
| Plugin_libdeflate.bas | DEFLATE | 1.23 |

### Utilities (20 modules)

| Module | Purpose |
|--------|---------|
| Strings.bas | String manipulation |
| PDMath.bas | Math utilities |
| ComplexNumbers.bas | Complex number math |
| Evaluator.bas | Expression evaluator |
| Resampling.bas | Image resampling |
| Colors.bas | Color utilities |
| ColorManagement_ICM.bas | ICC color mgmt |
| Palettes.bas | Palette management |
| TextSupport.bas | Text utilities |
| TextTools.bas | Text tools |
| Fonts.bas | Font management |
| Uniscribe.bas | Complex text |
| Files.bas | File utilities |
| Compression.bas | Compression |
| OS.bas | OS utilities |
| VB_Hacks.bas | VB6 workarounds |
| Mutex.bas | Mutex handling |
| Units.bas | Unit conversion |
| Histograms.bas | Histogram ops |
| Snap.bas | Snap-to-grid |

### Application Services (12 modules)

| Module | Purpose |
|--------|---------|
| AutosaveEngine.bas | Auto-save |
| BatchProcessor.bas | Batch processing |
| FileMenu.bas | File menu ops |
| Printing.bas | Print support |
| ScreenCapture.bas | Screen capture |
| UpdateEngine.bas | Auto-update |
| UserPrefs.bas | User preferences |
| FreeImageWrapper.bas | FreeImage wrapper |
| WIC.bas | Windows Imaging |
| Web.bas | Web utilities |
| Animation.bas | Animation support |

---

## Classes (141 files)

Classes (.cls files) provide object-oriented components. They are organized by subsystem:

### Core Data Model (8 classes)

| Class | Purpose | Key Properties |
|-------|---------|----------------|
| pdImage.cls | Image container | Width, Height, Layers[], UndoManager |
| pdLayer.cls | Layer representation | LayerDIB, Opacity, BlendMode, Transforms |
| pdDIB.cls | Bitmap wrapper | Width, Height, BitsPerPixel, DIBits |
| pdSelection.cls | Selection data | SelectionShape, Bounds, Mask |
| pdLayerMask.cls | Layer masks | Mask data, invert flag |
| pdMetadata.cls | Image metadata | EXIF, IPTC, XMP data |
| pdUndo.cls | Undo/Redo | Stack, UndoNum, RedoState |
| pdViewport.cls | Viewport mgmt | Zoom, Scroll, Visible rect |

### Rendering (13 classes)

| Class | Purpose |
|-------|---------|
| pd2DSurface.cls | Surface abstraction |
| pd2DBrush.cls | Brush objects |
| pd2DPen.cls | Pen objects |
| pd2DPath.cls | Vector paths |
| pd2DGradient.cls | Gradient fills |
| pd2DRegion.cls | Clipping regions |
| pd2DTransform.cls | Affine transforms |
| pdCompositor.cls | Layer compositor |
| pdPixelBlender.cls | Blend modes |
| pdPixelIterator.cls | Fast pixel access |
| pdSurfaceF.cls | Float surface |
| pdWindowPainter.cls | Window painting |
| pdDisplay.cls | Display info |
| pdDisplays.cls | Multi-monitor |

### File Formats (22 classes)

**Native Parsers:**
| Class | Format |
|-------|--------|
| pdPNG.cls | PNG |
| pdPSD.cls | Photoshop PSD/PSB |
| pdPSP.cls | PaintShop Pro |
| pdGIF.cls | GIF |
| pdICO.cls | Windows Icon |
| pdPCX.cls | PC Paintbrush |
| pdQOI.cls | Quite OK Image |
| pdWBMP.cls | Wireless Bitmap |
| pdXBM.cls | X Bitmap |
| pdXCF.cls | GIMP XCF |
| pdOpenRaster.cls | OpenRaster |
| pdCBZ.cls | Comic Book Archive |
| pdHGT.cls | SRTM Height Map |
| pdMBM.cls | Symbian MBM |
| pdWebP.cls | WebP (with plugin) |
| pdPDF.cls | PDF (with plugin) |

**Format Support:**
| Class | Purpose |
|-------|---------|
| pdPNGChunk.cls | PNG chunk |
| pdPSDLayer.cls | PSD layer |
| pdPSDLayerInfo.cls | PSD layer info |
| pdPSPBlock.cls | PSP block |
| pdPSPLayer.cls | PSP layer |
| pdPSPChannel.cls | PSP channel |
| pdPSPShape.cls | PSP shape |

**Packaging:**
| Class | Purpose |
|-------|---------|
| pdPackageChunky.cls | Modern PDI format |
| pdPackager2.cls | PDI v2 format |
| pdPackagerLegacy.cls | Legacy PDI |

### Color Management (12 classes)

| Class | Purpose |
|-------|---------|
| pdColorSearch.cls | Color space search |
| pdColorCount.cls | Color counting |
| pdColorSearchNode.cls | Search tree node |
| pdICCProfile.cls | ICC profile |
| pdLCMSProfile.cls | LittleCMS profile |
| pdLCMSTransform.cls | Color transform |
| pdPalette.cls | Palette object |
| pdPaletteChild.cls | Palette entry |
| pdMedianCut.cls | Median cut |
| pdNeuquant.cls | Neural quantization |
| pdLUT3D.cls | 3D LUT |
| pdFilterLUT.cls | Filter LUT |

### Effects and Filters (7 classes)

| Class | Purpose |
|-------|---------|
| pdFilterSupport.cls | Filter helpers |
| pdFFT.cls | Fast Fourier Transform |
| pdFxBilateral.cls | Bilateral filter |
| pdInpaint.cls | Inpainting |
| pdSeamCarving.cls | Content-aware resize |
| pdFocusDetector.cls | Focus detection |
| pdEdgeDetector.cls | Edge detection |

### Tools (3 classes)

| Class | Purpose |
|-------|---------|
| pdPaintbrush.cls | Paintbrush engine |
| pdInputMouse.cls | Mouse input |
| pdInputKeyboard.cls | Keyboard input |

### Utilities (35 classes)

**Strings:**
| Class | Purpose |
|-------|---------|
| pdString.cls | String builder |
| pdStringStack.cls | String stack |
| pdStringHash.cls | String hash |

**Collections:**
| Class | Purpose |
|-------|---------|
| pdDictionary.cls | Key-value store |
| pdStack.cls | Stack |
| pdObjectList.cls | Object list |
| pdListSupport.cls | List helpers |
| pdTreeSupport.cls | Tree helpers |
| pdVariantHash.cls | Variant hash |

**Data Structures:**
| Class | Purpose |
|-------|---------|
| pdKDTree.cls | K-D tree |
| pdKDTreeNode.cls | K-D tree node |
| pdKDTreeArray.cls | K-D tree array |
| pdHashCoord.cls | Coordinate hash |
| pdHistogramHash.cls | Histogram hash |

**Text Rendering:**
| Class | Purpose |
|-------|---------|
| pdFont.cls | Font object |
| pdFontCollection.cls | Font collection |
| pdGlyphCollection.cls | Glyph cache |
| pdTextRenderer.cls | Text renderer |
| pdUniscribe.cls | Uniscribe wrapper |
| pdUniscribeItem.cls | Uniscribe item |

**File System:**
| Class | Purpose |
|-------|---------|
| pdFSO.cls | File system object |
| pdFileMM.cls | Memory-mapped files |
| pdStream.cls | Byte stream |

**Serialization:**
| Class | Purpose |
|-------|---------|
| pdSerialize.cls | Object serialization |
| pdXML.cls | XML parser |

**Compression:**
| Class | Purpose |
|-------|---------|
| cZipArchive.cls | ZIP archive |

**Cryptography:**
| Class | Purpose |
|-------|---------|
| pdCrypto.cls | Encryption/hashing |

**OS Integration:**
| Class | Purpose |
|-------|---------|
| pdMutex.cls | Mutex object |
| pdPipe.cls | Named pipe |
| pdPipeSync.cls | Sync pipe |
| pdAsyncPipe.cls | Async pipe |

**Miscellaneous:**
| Class | Purpose |
|-------|---------|
| pdTimer.cls | Timer |
| pdTimerAnimation.cls | Animation timer |
| pdTimerCountdown.cls | Countdown timer |
| pdProfiler.cls | Performance profiling |
| pdRandomize.cls | RNG |
| pdNoise.cls | Noise generation |
| pdPoissonDisc.cls | Poisson disc |
| pdVoronoi.cls | Voronoi diagrams |
| pdMakeTexture.cls | Texture synthesis |
| pdSpriteSheet.cls | Sprite sheets |

### UI Support (17 classes)

**Window Management:**
| Class | Purpose |
|-------|---------|
| pdWindowManager.cls | Window layout |
| pdWindowSize.cls | Window sizing |
| pdWindowSync.cls | Window sync |

**Theme:**
| Class | Purpose |
|-------|---------|
| pdThemeColors.cls | Color schemes |
| pdVisualThemes.cls | Visual themes |

**UI Components:**
| Class | Purpose |
|-------|---------|
| pdCaption.cls | Window caption |
| pdFlyout.cls | Flyout menu |
| pdPopupMenu.cls | Popup menu |
| clsMenuImage.cls | Menu images |

**Dialogs:**
| Class | Purpose |
|-------|---------|
| pdOpenSaveDialog.cls | Open/Save dialog |
| cFileDialogVista.cls | Vista file dialog |
| cUnicodeBrowseFolders.cls | Folder browser |

**Clipboard:**
| Class | Purpose |
|-------|---------|
| pdClipboard.cls | Clipboard wrapper |
| pdClipboardMain.cls | Main clipboard |

**User Control:**
| Class | Purpose |
|-------|---------|
| pdUCSupport.cls | UC helpers |
| pdUCEventSink.cls | UC event sink |
| pdEditBoxW.cls | Unicode edit box |

### Application Services (12 classes)

| Class | Purpose |
|-------|---------|
| pdThunderMain.cls | Main form listener |
| pdResources.cls | Resource handler |
| pdTranslate.cls | Localization |
| pdAutoLocalize.cls | Auto-localize |
| cGoogleTranslate.cls | Google Translate |
| pdRecentFiles.cls | Recent files MRU |
| pdMRUManager.cls | MRU manager |
| pdMRURecentMacros.cls | Recent macros |
| pdLastUsedSettings.cls | Last settings |
| pdToolPreset.cls | Tool presets |
| pdZoom.cls | Zoom state |
| ISubclass.cls | Subclassing interface |

---

## Forms (216 files)

Forms (.frm files) define UI dialogs and windows. They are organized by menu structure:

### Main Application (5 forms)

| Form | Purpose |
|------|---------|
| MainWindow.frm | MDI-style main window |
| Startup_Splash.frm | Splash screen during load |
| Misc_Tooltip.frm | Custom tooltip rendering |
| Help_About.frm | About dialog |
| Toolbar_Debug.frm | Debug toolbar |

### Layer Panels (4 forms)

| Form | Purpose |
|------|---------|
| Layerpanel_Layers.frm | Layer list and management |
| Layerpanel_Colors.frm | Color picker panel |
| Layerpanel_Navigator.frm | Image navigator panel |
| Layerpanel_Search.frm | Search functionality |

### Toolbars (3 forms)

| Form | Purpose |
|------|---------|
| Toolbar_ToolSelect.frm | Tool selection toolbar |
| Toolbar_Layers.frm | Layer operations toolbar |
| Toolbar_ToolOptionsContainer.frm | Container for tool options |

### Tool Panels (13 forms)

| Form | Tool |
|------|------|
| Toolpanel_Paintbrush.frm | Paintbrush options |
| Toolpanel_Pencil.frm | Pencil options |
| Toolpanel_Fill.frm | Fill/bucket options |
| Toolpanel_Gradient.frm | Gradient options |
| Toolpanel_Clone.frm | Clone stamp options |
| Toolpanel_MoveSize.frm | Move/size options |
| Toolpanel_Crop.frm | Crop options |
| Toolpanel_Selections.frm | Selection tool options |
| Toolpanel_TextBasic.frm | Basic text options |
| Toolpanel_Typography.frm | Advanced typography |
| Toolpanel_Measure.frm | Measurement options |
| Toolpanel_ColorPicker.frm | Color picker options |
| Toolpanel_Eraser.frm | Eraser options |

### Dialogs (16 forms)

| Form | Purpose |
|------|---------|
| Dialog_ColorPanel.frm | Color selection panel |
| Dialog_ColorSelector.frm | Color selector dialog |
| Dialog_AutosaveFound.frm | Autosave recovery |
| Dialog_GenericRemember.frm | Generic remember choice |
| Dialog_GenericWait.frm | Generic wait dialog |
| Dialog_GradientEditor.frm | Gradient editor |
| Dialog_FillSettings.frm | Fill settings |
| Dialog_OutlineSettings.frm | Outline settings |
| Dialog_MsgBox.frm | Custom message box |
| Dialog_IDEWarning.frm | IDE warning |
| Dialog_ImportPDF.frm | PDF import options |
| Dialog_SVGImport.frm | SVG import options |
| Dialog_ToneMapping.frm | Tone mapping |
| Dialog_UITheme.frm | Theme selector |
| Dialog_UnsavedChanges.frm | Unsaved changes |
| Dialog_UpdateAvailable.frm | Update notification |
| Dialog_EditPreset.frm | Edit preset |

### File Menu (33 forms)

**File Operations:**
| Form | Purpose |
|------|---------|
| File_New.frm | New image dialog |
| File_PrintXP.frm | Print dialog (XP+) |

**Batch Operations:**
| Form | Purpose |
|------|---------|
| File_BatchWizard.frm | Batch process wizard |
| File_BatchRepair.frm | Batch repair damaged images |

**Import:**
| Form | Purpose |
|------|---------|
| File_Import_FromInternet.frm | Download from URL |
| File_Import_ScreenCapture.frm | Screen capture |

**Export:**
| Form | Purpose |
|------|---------|
| File_Export_AnimatedGIF.frm | Export animated GIF |
| File_Export_AnimatedPNG.frm | Export animated PNG (APNG) |
| File_Export_AnimatedWebP.frm | Export animated WebP |
| File_Export_AnimatedJXL.frm | Export animated JPEG XL |
| File_Export_Palette.frm | Export color palette |
| File_Export_Layers.frm | Export layers |
| File_Export_LUT.frm | Export LUT |

**Save Format Dialogs (15 forms):**
| Form | Format |
|------|--------|
| File_Save_AVIF.frm | AVIF |
| File_Save_BMP.frm | BMP |
| File_Save_DDS.frm | DirectDraw Surface |
| File_Save_GIF.frm | GIF |
| File_Save_HEIF.frm | HEIF/HEIC |
| File_Save_ICO.frm | Windows Icon |
| File_Save_JP2.frm | JPEG 2000 |
| File_Save_JPG.frm | JPEG |
| File_Save_JXL.frm | JPEG XL |
| File_Save_JXR.frm | JPEG XR |
| File_Save_PNG.frm | PNG |
| File_Save_PSD.frm | Photoshop PSD |
| File_Save_PSP.frm | PaintShop Pro |
| File_Save_Pixmap.frm | PBM/PGM/PPM |
| File_Save_TIFF.frm | TIFF |
| File_Save_WebP.frm | WebP |

### Edit Menu (6 forms)

| Form | Purpose |
|------|---------|
| Edit_Clipboard.frm | Clipboard management |
| Edit_ContentAwareFill.frm | Content-aware fill |
| Edit_Fade.frm | Fade last effect |
| Edit_Fill.frm | Fill selection |
| Edit_Stroke.frm | Stroke selection |
| Edit_UndoHistory.frm | Undo history browser |

### Image Menu (8 forms)

| Form | Purpose |
|------|---------|
| Image_Animation.frm | Animation settings |
| Image_CanvasSize.frm | Canvas size |
| Image_Compare.frm | Compare images |
| Image_ContentAwareResize.frm | Seam carving |
| Image_CreateLUT.frm | Create LUT from image |
| Image_Metadata.frm | View/edit metadata |
| Image_Resize.frm | Resize image |
| Image_Rotate.frm | Rotate image |
| Image_Straighten.frm | Straighten image |

### Layer Menu (6 forms)

| Form | Purpose |
|------|---------|
| Layer_Add_RasterLayer.frm | Add raster layer |
| Layer_Flatten.frm | Flatten image |
| Layer_Split_ImagesToLayers.frm | Split images to layers |
| Layer_Transparency_GreenScreen.frm | Chroma key transparency |
| Layer_Transparency_Luma.frm | Luminance transparency |
| Layer_Transparency_RemoveTransparency.frm | Remove transparency |
| Layer_Transparency_Threshold.frm | Threshold transparency |

### Select Menu (1 form)

| Form | Purpose |
|------|---------|
| Select_GenericModification.frm | Generic selection mods |

### Adjustments Menu (28 forms)

**Basic Adjustments:**
| Form | Adjustment |
|------|------------|
| Adjustments_BrightnessContrast.frm | Brightness/Contrast |
| Adjustments_ColorBalance.frm | Color Balance |
| Adjustments_Curves.frm | Curves |
| Adjustments_Levels.frm | Levels |
| Adjustments_ShadowAndHighlight.frm | Shadows/Highlights |
| Adjustments_WhiteBalance.frm | White Balance |

**Color Adjustments (8 forms):**
| Form | Adjustment |
|------|------------|
| Adjustments_Color_Colorize.frm | Colorize |
| Adjustments_Color_Grayscale.frm | Convert to grayscale |
| Adjustments_Color_HSL.frm | Hue/Saturation/Lightness |
| Adjustments_Color_Lookup.frm | Color lookup |
| Adjustments_Color_ReplaceColor.frm | Replace color |
| Adjustments_Color_Sepia.frm | Sepia tone |
| Adjustments_Color_Temperature.frm | Color temperature |
| Adjustments_Color_Tint.frm | Tint |
| Adjustments_Color_Vibrance.frm | Vibrance |

**Lighting (3 forms):**
| Form | Adjustment |
|------|------------|
| Adjustments_Lighting_Dehaze.frm | Dehaze |
| Adjustments_Lighting_Exposure.frm | Exposure |
| Adjustments_Lighting_Gamma.frm | Gamma correction |

**Photo (3 forms):**
| Form | Adjustment |
|------|------------|
| Adjustments_Photo_HDR.frm | HDR |
| Adjustments_Photo_PhotoFilters.frm | Photo filters |
| Adjustments_Photo_SplitTone.frm | Split toning |

**Other:**
| Form | Adjustment |
|------|------------|
| Adjustments_BlackAndWhite.frm | Black & white |
| Adjustments_Channel_ChannelMixer.frm | Channel mixer |
| Adjustments_Channel_Rechannel.frm | Rechannel |
| Adjustments_Histogram_DisplayHistogram.frm | Display histogram |
| Adjustments_Histogram_Equalize.frm | Equalize histogram |
| Adjustments_Map_Gradient.frm | Gradient map |
| Adjustments_Monochrome_MonoToGray.frm | Monochrome to gray |

### Effects Menu (90 forms)

**Artistic (13 forms):**
| Form | Effect |
|------|--------|
| Effects_Artistic_ColoredPencil.frm | Colored pencil |
| Effects_Artistic_ComicBook.frm | Comic book |
| Effects_Artistic_FiguredGlass.frm | Figured glass |
| Effects_Artistic_FilmNoir.frm | Film noir |
| Effects_Artistic_GlassTiles.frm | Glass tiles |
| Effects_Artistic_Kaleidoscope.frm | Kaleidoscope |
| Effects_Artistic_ModernArt.frm | Modern art |
| Effects_Artistic_OilPainting.frm | Oil painting |
| Effects_Artistic_PlasticWrap.frm | Plastic wrap |
| Effects_Artistic_Posterize.frm | Posterize |
| Effects_Artistic_Relief.frm | Relief |
| Effects_Artistic_StainedGlass.frm | Stained glass |

**Blur (8 forms):**
| Form | Effect |
|------|--------|
| Effects_Blur_BoxBlur.frm | Box blur |
| Effects_Blur_GaussianBlur.frm | Gaussian blur |
| Effects_Blur_Kuwahara.frm | Kuwahara filter |
| Effects_Blur_MotionBlur.frm | Motion blur |
| Effects_Blur_RadialBlur.frm | Radial blur |
| Effects_Blur_SNN.frm | Symmetric nearest neighbor |
| Effects_Blur_SurfaceBlur.frm | Surface blur |
| Effects_Blur_ZoomBlur.frm | Zoom blur |

**Distort (11 forms):**
| Form | Effect |
|------|--------|
| Effects_Distort_ApplyLens.frm | Apply lens |
| Effects_Distort_CorrectLens.frm | Correct lens |
| Effects_Distort_Donut.frm | Donut |
| Effects_Distort_Droste.frm | Droste effect |
| Effects_Distort_Miscellaneous.frm | Misc distortions |
| Effects_Distort_Pinch.frm | Pinch |
| Effects_Distort_Poke.frm | Poke |
| Effects_Distort_Ripple.frm | Ripple |
| Effects_Distort_Squish.frm | Squish |
| Effects_Distort_Swirl.frm | Swirl |
| Effects_Distort_Waves.frm | Waves |

**Edge (6 forms):**
| Form | Effect |
|------|--------|
| Effects_Edge_Emboss.frm | Emboss |
| Effects_Edge_EnhanceEdges.frm | Enhance edges |
| Effects_Edge_FindEdges.frm | Find edges |
| Effects_Edge_GradientFlow.frm | Gradient flow |
| Effects_Edge_Range.frm | Range filter |
| Effects_Edge_TraceContour.frm | Trace contour |

**Light and Shadow (5 forms):**
| Form | Effect |
|------|--------|
| Effects_LightAndShadow_Blacklight.frm | Blacklight |
| Effects_LightAndShadow_BumpMap.frm | Bump map |
| Effects_LightAndShadow_CrossScreen.frm | Cross-screen |
| Effects_LightAndShadow_Rainbow.frm | Rainbow |
| Effects_LightAndShadow_Sunshine.frm | Sunshine |

**Nature (7 forms):**
| Form | Effect |
|------|--------|
| Effects_Nature_Atmosphere.frm | Atmosphere |
| Effects_Nature_Fog.frm | Fog |
| Effects_Nature_Ignite.frm | Ignite |
| Effects_Nature_Lava.frm | Lava |
| Effects_Nature_Metal.frm | Metal |
| Effects_Nature_Snow.frm | Snow |
| Effects_Nature_Water.frm | Water |

**Noise (7 forms):**
| Form | Effect |
|------|--------|
| Effects_Noise_AddRGBNoise.frm | Add RGB noise |
| Effects_Noise_Anisotropic.frm | Anisotropic diffusion |
| Effects_Noise_DustAndScratches.frm | Dust and scratches |
| Effects_Noise_FilmGrain.frm | Film grain |
| Effects_Noise_HarmonicMean.frm | Harmonic mean |
| Effects_Noise_MeanShift.frm | Mean shift |
| Effects_Noise_MedianSmoothing.frm | Median filter |

**Pixelate (6 forms):**
| Form | Effect |
|------|--------|
| Effects_Pixelate_ColorHalftone.frm | Color halftone |
| Effects_Pixelate_Crystallize.frm | Crystallize |
| Effects_Pixelate_Fragment.frm | Fragment |
| Effects_Pixelate_Mezzotint.frm | Mezzotint |
| Effects_Pixelate_Mosaic.frm | Mosaic |
| Effects_Pixelate_Pointillize.frm | Pointillize |

**Render (3 forms):**
| Form | Effect |
|------|--------|
| Effects_Render_Clouds.frm | Clouds |
| Effects_Render_Fibers.frm | Fibers |
| Effects_Render_Truchet.frm | Truchet tiles |

**Sharpen (2 forms):**
| Form | Effect |
|------|--------|
| Effects_Sharpen_Sharpen.frm | Sharpen |
| Effects_Sharpen_UnsharpMask.frm | Unsharp mask |

**Stylize (8 forms):**
| Form | Effect |
|------|--------|
| Effects_Stylize_Antique.frm | Antique |
| Effects_Stylize_Diffuse.frm | Diffuse |
| Effects_Stylize_Outline.frm | Outline |
| Effects_Stylize_Palettize.frm | Palettize |
| Effects_Stylize_PortraitGlow.frm | Portrait glow |
| Effects_Stylize_Solarize.frm | Solarize |
| Effects_Stylize_Twins.frm | Twins |
| Effects_Stylize_Vignette.frm | Vignette |

**Transform (6 forms):**
| Form | Effect |
|------|--------|
| Effects_Transform_PanZoom.frm | Pan and zoom |
| Effects_Transform_Perspective.frm | Perspective |
| Effects_Transform_PolarCoords.frm | Polar coordinates |
| Effects_Transform_Rotate.frm | Rotate |
| Effects_Transform_Shear.frm | Shear |
| Effects_Transform_Sphere.frm | Sphere |

**Other:**
| Form | Effect |
|------|--------|
| Effects_8bf.frm | 8bf plugin host |
| Effects_CustomFilter.frm | Custom convolution |
| Effects_Animation_Background.frm | Animation background |
| Effects_Animation_Speed.frm | Animation speed |

### Tools Menu (9 forms)

| Form | Purpose |
|------|---------|
| Tools_BuildPackage.frm | Build PD package |
| Tools_Hotkeys.frm | Customize hotkeys |
| Tools_LanguageEditor.frm | Language editor |
| Tools_MacroSession.frm | Macro recorder |
| Tools_Options.frm | Preferences/Options |
| Tools_PluginManager.frm | Plugin management |
| Tools_ScreenVideo.frm | Screen recording |
| Tools_ScreenVideoPrefs.frm | Screen rec preferences |
| Tools_ThemeEditor.frm | Theme editor |

---

## Controls (56 files)

Custom user controls (.ctl files) provide consistent, theme-able UI elements:

### Input Controls (14 controls)

| Control | Purpose |
|---------|---------|
| pdButton.ctl | Custom-drawn button |
| pdCheckBox.ctl | Custom checkbox |
| pdRadioButton.ctl | Custom radio button |
| pdTextBox.ctl | Custom text input |
| pdSlider.ctl | Slider with label |
| pdSliderStandalone.ctl | Standalone slider |
| pdSpinner.ctl | Numeric up/down |
| pdDropDown.ctl | Dropdown list |
| pdDropDownFont.ctl | Font picker |
| pdSearchBar.ctl | Search input |
| pdColorSelector.ctl | Color picker |
| pdBrushSelector.ctl | Brush picker |
| pdPenSelector.ctl | Pen picker |
| pdGradientSelector.ctl | Gradient picker |

### Display Controls (11 controls)

| Control | Purpose |
|---------|---------|
| pdLabel.ctl | Custom label |
| pdTitle.ctl | Title bar |
| pdProgressBar.ctl | Progress bar |
| pdStatusBar.ctl | Status bar |
| pdHyperlink.ctl | Clickable link |
| pdFxPreview.ctl | Effect preview |
| pdPreview.ctl | Generic preview |
| pdNewOld.ctl | Before/after |
| pdPictureBox.ctl | Picture display |
| pdPictureBoxInteractive.ctl | Interactive picture |
| pdColorWheel.ctl | HSV color wheel |
| pdColorVariants.ctl | Color variations |

### Container Controls (5 controls)

| Control | Purpose |
|---------|---------|
| pdContainer.ctl | Generic container |
| pdButtonStrip.ctl | Horizontal buttons |
| pdButtonStripVertical.ctl | Vertical buttons |
| pdStrip.ctl | Generic strip |
| pdCommandBar.ctl | OK/Cancel/Reset |
| pdCommandBarMini.ctl | Mini command bar |

### Specialized Controls (26 controls)

**Canvas:**
| Control | Purpose |
|---------|---------|
| pdCanvas.ctl | Main canvas |
| pdCanvasView.ctl | Canvas viewport |

**Panels:**
| Control | Purpose |
|---------|---------|
| pdLayerList.ctl | Layer list |
| pdLayerListInner.ctl | Layer list inner |
| pdHistory.ctl | Undo history |
| pdNavigator.ctl | Navigator panel |
| pdNavigatorInner.ctl | Navigator inner |
| pdPaletteUI.ctl | Palette UI |

**Lists and Trees:**
| Control | Purpose |
|---------|---------|
| pdListBox.ctl | List box |
| pdListBoxOD.ctl | Owner-draw list |
| pdListBoxView.ctl | List view |
| pdListBoxViewOD.ctl | Owner-draw view |
| pdTreeviewOD.ctl | Tree view |
| pdTreeviewViewOD.ctl | Tree view control |
| pdImageStrip.ctl | Image thumbnails |

**Editors:**
| Control | Purpose |
|---------|---------|
| pdResize.ctl | Resize dialog |
| pdColorDepth.ctl | Color depth picker |
| pdMetadataExport.ctl | Metadata export |
| pdRandomizeUI.ctl | Randomize params |

**UI Helpers:**
| Control | Purpose |
|---------|---------|
| pdScrollBar.ctl | Custom scrollbar |
| pdRuler.ctl | Measurement ruler |
| pdAccelerator.ctl | Keyboard shortcuts |
| pdButtonToolbox.ctl | Toolbox button |
| pdDownload.ctl | Download progress |

---

## Dependency Graph

### Major Component Dependencies

```
Main.bas
  └─> LoadTheProgram()
        ├─> PluginManager (initialize plugins)
        ├─> g_Language (pdTranslate)
        ├─> g_Resources (pdResources)
        ├─> g_Clipboard (pdClipboardMain)
        ├─> g_WindowManager (pdWindowManager)
        ├─> g_Themer (pdTheme)
        └─> FormMain (MainWindow.frm)
              └─> pdCanvas controls
                    └─> ViewportEngine
                          └─> CanvasManager
                                └─> pdCompositor
                                      ├─> pdPixelBlender
                                      └─> pdImage
                                            ├─> pdLayer[]
                                            │     └─> pdDIB
                                            ├─> pdSelection
                                            ├─> pdMetadata
                                            ├─> pdUndo
                                            └─> pdViewport
```

### Action Processing Flow

```
User Action
  └─> Processor.Process()
        ├─> Validate action
        ├─> Show dialog (if showDialog = True)
        │     └─> FormXXX.frm
        │           └─> pdCommandBar
        │                 └─> Return parameters
        ├─> Execute action
        │     ├─> Filters_XXX or Effects_XXX
        │     ├─> Tools_XXX
        │     └─> Layer operations
        ├─> Create Undo (if createUndo <> UNDO_Nothing)
        │     └─> pdUndo.CreateUndoData()
        │           └─> Save diff to PDI file
        └─> Record Macro (if recordAction = True)
              └─> Save action to macro file
```

### File Loading Flow

```
File > Open
  └─> Loading.LoadFileAsNewImage()
        ├─> ImageFormats.IsFileSupported() - Detect format
        ├─> ImageLoader.LoadFromFile() - Import
        │     ├─> Internal parser (pdPNG, pdPSD, etc.)
        │     ├─> Plugin (FreeImage, libwebp, etc.)
        │     └─> Windows codec (WIC, GDI+)
        ├─> pdImage.CreateNewImageFromDIB()
        │     ├─> Create pdImage instance
        │     ├─> Create base layer (pdLayer)
        │     ├─> Initialize pdUndo
        │     └─> Load metadata (pdMetadata)
        └─> Add to PDImages collection
              └─> Display in canvas
```

### Rendering Pipeline

```
Canvas Redraw
  └─> ViewportEngine.Stage_RenderCanvas()
        ├─> pdImage.GetCompositedImage()
        │     └─> pdCompositor.Composite()
        │           ├─> For each visible layer:
        │           │     ├─> Apply non-destructive transforms
        │           │     └─> pdPixelBlender.BlendDIBs()
        │           └─> Return composite pdDIB
        ├─> Apply zoom/pan (pdViewport)
        ├─> Render selection outline (pdSelection)
        └─> PD2D.CopySurface() - Blit to canvas
```

### Module Interdependencies

**Core Dependencies:**
- Almost all modules depend on `PublicConstants`, `PublicVars`, `PublicEnumsAndTypes`
- Most modules use `Strings.bas`, `PDMath.bas`, `Colors.bas`
- UI modules depend on `Interface.bas`, `g_Themer`

**Rendering Stack:**
```
PD2D.bas
  ├─> pd2DSurface, pd2DBrush, pd2DPen, pd2DPath
  ├─> GDI.bas (for BitBlt, AlphaBlend)
  └─> GDIPlus.bas (for advanced features)
```

**Image Data Stack:**
```
pdImage
  ├─> pdLayer (0..n)
  │     ├─> pdDIB (raster data)
  │     └─> pdLayerMask
  ├─> pdCompositor
  ├─> pdSelection
  ├─> pdMetadata
  ├─> pdUndo
  └─> pdViewport
```

---

## Key Architectural Patterns

### 1. Central Dispatcher Pattern

**Pattern:** All user actions route through `Processor.Process()`

**Benefits:**
- Centralized error handling
- Automatic macro recording
- Consistent undo/redo creation
- Progress reporting
- Action queuing

**Implementation:**
```vb6
Public Sub Process(ByVal processID As String, _
                   Optional raiseDialog As Boolean = False, _
                   Optional processParameters As String = vbNullString, _
                   Optional createUndo As PD_UndoType = UNDO_Nothing, _
                   Optional relevantTool As Long = -1, _
                   Optional recordAction As Boolean = True)
```

### 2. Plugin Architecture

**Pattern:** Graceful degradation with version checking

**Benefits:**
- Extended functionality without bloat
- Portable application (plugins in /App/PhotoDemon/Plugins)
- Version compatibility checking
- Fallback to alternative methods

**Implementation:**
- 18 core plugins managed by `Plugin_Management.bas`
- Each plugin has its own module (Plugin_XXX.bas)
- Plugins initialized at startup
- Application continues if plugin unavailable

### 3. Diff-Based Undo/Redo

**Pattern:** Git-like diff storage instead of full copies

**Benefits:**
- Unlimited undo history
- Minimal disk usage
- Fast undo/redo operations
- Supports all image modifications

**Implementation:**
- Each undo stores only changed layers/properties
- Undo data saved as PDI files in temp directory
- `pdUndo.cls` manages stack and diff creation

### 4. Abstraction Layers

**Pattern:** Multiple abstraction layers for cross-platform support

**Rendering Abstraction:**
```
PD2D (high-level API)
  └─> GDI/GDI+ (implementation)
```

**File I/O Abstraction:**
```
Loading/Saving (high-level)
  └─> ImageLoader/ImageExporter (routing)
        ├─> Internal parsers (pdPNG, pdPSD, etc.)
        ├─> External plugins (FreeImage, etc.)
        └─> OS codecs (WIC, GDI+)
```

### 5. Viewport Caching

**Pattern:** Multi-level caching for performance

**Cache Levels:**
1. **CompositeBuffer** (pdDIB) - Full composited image
2. **CanvasBuffer** (pdDIB) - Viewport-sized, cropped composite
3. **Layer DIBs** - Individual layer pixel data

**Invalidation:**
- Layer changes → invalidate CompositeBuffer
- Viewport changes → invalidate CanvasBuffer
- Minimize recomposition

### 6. Custom Control Framework

**Pattern:** Owner-drawn controls for consistent theming

**Benefits:**
- Full visual control
- Consistent look across Windows versions
- Theme support
- DPI-aware rendering

**Implementation:**
- 56 custom controls (.ctl files)
- All controls support theming via `g_Themer`
- `pdUCSupport.cls` provides common UC functionality

### 7. Modular Effect System

**Pattern:** Standardized effect dialog and execution

**Structure:**
```
Effect Dialog (Form)
  ├─> Effect parameters (sliders, checkboxes)
  ├─> pdFxPreview (live preview)
  └─> pdCommandBar (OK/Cancel/Reset)
        └─> On OK: Call Processor.Process()
              └─> Execute filter in Filters_XXX.bas
```

**Benefits:**
- Consistent UI across 140+ effects
- Live preview
- Preset support
- Macro recording

### 8. Non-Destructive Transformations

**Pattern:** Layer transforms stored as properties, applied at render time

**Supported Transforms:**
- Rotation (angle, center point)
- Scaling (X/Y modifiers)
- Shearing (X/Y shear)
- Position offset

**Implementation:**
- Transforms stored in `pdLayer.myLayerData`
- Applied during `pdCompositor.Composite()`
- Original raster data preserved

---

## Migration Considerations

### Key Challenges

1. **VB6 Language Features:**
   - Control arrays (used extensively)
   - Default properties (e.g., `textBox = "value"`)
   - Late binding
   - ByRef default parameters
   - Variant type usage
   - Optional parameters with default values
   - On Error Resume Next error handling

2. **Windows API Dependencies:**
   - Extensive P/Invoke declarations (100+ API functions)
   - GDI/GDI+ rendering
   - COM interop
   - Subclassing and window procedures

3. **Form Designer:**
   - VB6 forms → XAML/MAUI
   - 216 forms to convert
   - 56 custom controls to recreate
   - Control arrays to reimplement

4. **Module Organization:**
   - 114 modules → static classes or namespaces
   - Global state management
   - Public variables → dependency injection

5. **Plugin System:**
   - 18 native DLLs (C/C++)
   - P/Invoke declarations
   - Function pointer callbacks
   - 32-bit → 64-bit considerations

6. **Binary Compatibility:**
   - PDI file format (must remain compatible)
   - Selection files
   - Macro files
   - Preset files

### Migration Priorities

**Phase 1: Core Infrastructure**
1. Data model (pdImage, pdLayer, pdDIB)
2. Basic rendering (PD2D, GDI wrapper)
3. File I/O (PDI format, PNG, JPEG)

**Phase 2: Essential Features**
1. Selection subsystem
2. Basic effects (20-30 most-used)
3. Basic tools (move, zoom, crop)
4. Undo/redo system

**Phase 3: Advanced Features**
1. Remaining effects (100+)
2. Advanced tools
3. Plugin integration
4. Macro system

**Phase 4: Polish**
1. All UI refinements
2. Performance optimization
3. Accessibility
4. Testing and bug fixes

### Recommended Architecture for .NET 10 MAUI

**Namespace Organization:**
```
PhotoDemon.Core
  ├─> DataModel (pdImage, pdLayer, pdDIB)
  ├─> Rendering (PD2D, Compositing)
  ├─> FileIO (ImageLoader, ImageExporter)
  └─> Utilities (Math, Strings, Colors)

PhotoDemon.Effects
  ├─> Adjustments
  ├─> Filters
  └─> Transforms

PhotoDemon.UI
  ├─> Controls (custom MAUI controls)
  ├─> Dialogs
  └─> ViewModels (MVVM pattern)

PhotoDemon.Plugins
  ├─> PluginManager
  └─> PluginInterop (P/Invoke)

PhotoDemon.Services
  ├─> UndoRedo
  ├─> Selections
  ├─> Tools
  └─> Theming
```

**Design Patterns:**
- **MVVM** for all UI
- **Dependency Injection** for services
- **Factory Pattern** for plugin loading
- **Command Pattern** for actions (replaces Processor)
- **Observer Pattern** for UI updates

**Technology Stack:**
- **.NET 10** (target framework)
- **MAUI** (cross-platform UI)
- **SkiaSharp** (2D rendering, replaces GDI+)
- **ImageSharp** (image processing)
- **SQLite** (preferences, MRU, presets)

---

## Appendix: File Listings

### Complete Module List (114 files)

```
Actions.bas                 FillTool.bas               Plugin_Management.bas
Animation.bas               Filters_Area.bas           Plugin_OpenJPEG.bas
AutosaveEngine.bas          Filters_ByteArray.bas      Plugin_PDF.bas
BatchProcessor.bas          Filters_Color.bas          Plugin_WebP.bas
CanvasManager.bas           Filters_Edge.bas           Plugin_heif.bas
Clonestamp.bas              Filters_Layers.bas         Plugin_jxl.bas
ColorManagement_ICM.bas     Filters_Misc.bas           Plugin_libdeflate.bas
ColorPicker.bas             Filters_Natural.bas        Plugin_lz4.bas
Colors.bas                  Filters_Render.bas         Plugin_resvg.bas
ComplexNumbers.bas          Filters_Scientific.bas     Plugin_zstd.bas
Compression.bas             Filters_Stylize.bas        Printing.bas
Debug.bas                   Filters_Transform.bas      Processor.bas
DialogManager.bas           Fonts.bas                  ProgressBars.bas
DibSupport.bas              FreeImageWrapper.bas       PublicConstants.bas
Drawing.bas                 GDI.bas                    PublicEnumsAndTypes.bas
Drawing2D.bas               GDIPlus.bas                PublicVars.bas
EffectPrep.bas              GradientTool.bas           Resampling.bas
Evaluator.bas               Histograms.bas             Saving.bas
FileMenu.bas                Hotkeys.bas                ScreenCapture.bas
Files.bas                   IconsAndCursors.bas        SelectionFiles.bas
ImageExporter.bas           ImageFormats.bas           SelectionFilters.bas
ImageFormats_GIF.bas        ImageFormats_GIF_LZW.bas   SelectionUI.bas
ImageFormats_PSP.bas        ImageLoader.bas            Selections.bas
Interface.bas               Layers.bas                 Snap.bas
Legacy.bas                  Loading.bas                Strings.bas
Main.bas                    MeasureTool.bas            TextSupport.bas
Menus.bas                   MoveTool.bas               TextTools.bas
Mutex.bas                   NavKey.bas                 Toolboxes.bas
OS.bas                      PD2D.bas                   Tools.bas
PDImages.bas                PDMath.bas                 Tools_Crop.bas
PDPackaging.bas             Paintbrush.bas             UIImages.bas
Palettes.bas                PencilTool.bas             Uniscribe.bas
Plugin_8bf.bas              Plugin_AVIF.bas            Units.bas
Plugin_CharLS.bas           Plugin_DDS.bas             UpdateEngine.bas
Plugin_EZTwain.bas          Plugin_ExifTool.bas        UserControl_Support.bas
Plugin_FreeImage.bas        Plugin_LittleCMS.bas       UserPrefs.bas
                                                       VB_Hacks.bas
                                                       ViewportEngine.bas
                                                       WIC.bas
                                                       Web.bas
                                                       Zoom.bas
                                                       ZoomTool.bas
```

### Complete Class List (141 files)

```
ISubclass.cls               pdICO.cls                  pdQOI.cls
cFileDialogVista.cls        pdImage.cls                pdRandomize.cls
cGoogleTranslate.cls        pdInpaint.cls              pdRecentFiles.cls
cUnicodeBrowseFolders.cls   pdInputKeyboard.cls        pdResources.cls
cZipArchive.cls             pdInputMouse.cls           pdSeamCarving.cls
clsMenuImage.cls            pdKDTree.cls               pdSelection.cls
pd2DBrush.cls               pdKDTreeArray.cls          pdSerialize.cls
pd2DGradient.cls            pdKDTreeNode.cls           pdSpriteSheet.cls
pd2DPath.cls                pdLCMSProfile.cls          pdStack.cls
pd2DPen.cls                 pdLCMSTransform.cls        pdStream.cls
pd2DRegion.cls              pdLUT3D.cls                pdString.cls
pd2DSurface.cls             pdLastUsedSettings.cls     pdStringHash.cls
pd2DTransform.cls           pdLayer.cls                pdStringStack.cls
pdAsyncPipe.cls             pdLayerMask.cls            pdSurfaceF.cls
pdAutoLocalize.cls          pdListSupport.cls          pdTextRenderer.cls
pdCBZ.cls                   pdMBM.cls                  pdThemeColors.cls
pdCaption.cls               pdMRUManager.cls           pdThunderMain.cls
pdClipboard.cls             pdMRURecentMacros.cls      pdTimer.cls
pdClipboardMain.cls         pdMakeTexture.cls          pdTimerAnimation.cls
pdColorCount.cls            pdMedianCut.cls            pdTimerCountdown.cls
pdColorSearch.cls           pdMetadata.cls             pdToolPreset.cls
pdColorSearchNode.cls       pdMutex.cls                pdTranslate.cls
pdCompositor.cls            pdNeuquant.cls             pdTreeSupport.cls
pdCrypto.cls                pdNoise.cls                pdUCEventSink.cls
pdDIB.cls                   pdObjectList.cls           pdUCSupport.cls
pdDictionary.cls            pdOpenRaster.cls           pdUndo.cls
pdDisplay.cls               pdOpenSaveDialog.cls       pdUniscribe.cls
pdDisplays.cls              pdPCX.cls                  pdUniscribeItem.cls
pdEdgeDetector.cls          pdPDF.cls                  pdVariantHash.cls
pdEditBoxW.cls              pdPNG.cls                  pdViewport.cls
pdFFT.cls                   pdPNGChunk.cls             pdVisualThemes.cls
pdFSO.cls                   pdPSD.cls                  pdVoronoi.cls
pdFileMM.cls                pdPSDLayer.cls             pdWBMP.cls
pdFilterLUT.cls             pdPSDLayerInfo.cls         pdWebP.cls
pdFilterSupport.cls         pdPSP.cls                  pdWindowManager.cls
pdFloodFill.cls             pdPSPBlock.cls             pdWindowPainter.cls
pdFlyout.cls                pdPSPChannel.cls           pdWindowSize.cls
pdFocusDetector.cls         pdPSPLayer.cls             pdWindowSync.cls
pdFont.cls                  pdPSPShape.cls             pdXBM.cls
pdFontCollection.cls        pdPackageChunky.cls        pdXCF.cls
pdFxBilateral.cls           pdPackager2.cls            pdXML.cls
pdGIF.cls                   pdPackagerLegacy.cls       pdZoom.cls
pdGlyphCollection.cls       pdPaintbrush.cls
pdHGT.cls                   pdPalette.cls
pdHashCoord.cls             pdPaletteChild.cls
pdHistogramHash.cls         pdPipe.cls
pdICCProfile.cls            pdPipeSync.cls
                            pdPixelBlender.cls
                            pdPixelIterator.cls
                            pdPoissonDisc.cls
                            pdPopupMenu.cls
                            pdProfiler.cls
```

---

**End of Document**

*This architecture documentation was generated through systematic analysis of the PhotoDemon VB6 codebase. For questions or corrections, please refer to the PhotoDemon development team.*
