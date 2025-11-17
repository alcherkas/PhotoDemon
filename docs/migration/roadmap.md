# PhotoDemon VB6 to .NET 10 MAUI Migration Roadmap

**Document Version:** 1.0
**Date:** 2025-11-17
**Author:** Agent 8 - Migration Roadmap Agent
**Status:** Final Draft

---

## Executive Summary

This roadmap provides a comprehensive, phased approach to migrating PhotoDemon from VB6 to .NET 10 MAUI. Based on extensive Phase 1 analysis, the migration is **feasible but high-effort**, requiring approximately **18-24 months** with a team of 5-7 developers working in parallel.

### Project Scope

| Metric | Count | Complexity |
|--------|-------|------------|
| **Total Source Files** | 634 files | Very High |
| **Lines of Code** | 250,000+ | Very High |
| **Custom Controls** | 56 controls (2,100+ instances) | High |
| **Image Effects** | 117+ effects | High |
| **File Formats** | 40+ import, 25+ export | High |
| **Win32 API Calls** | 1,493 declarations | High |
| **External Plugins** | 18 core plugins | Medium |

### Critical Success Factors

1. **Preserve custom parsers** (PSD, PSP, XCF) - Competitive advantage
2. **Maintain color management quality** - Apple test suite compliance
3. **Achieve performance parity** - Match or exceed VB6 performance
4. **Cross-platform compatibility** - Windows, macOS, Linux via MAUI
5. **Backward compatibility** - Support existing PDI files and macros

### Risk Assessment

| Risk Category | Level | Mitigation Priority |
|---------------|-------|-------------------|
| GDI/GDI+ to SkiaSharp migration | 🔴 HIGH | Highest |
| Custom control reimplementation | 🔴 HIGH | Highest |
| 32-bit to 64-bit pointer migration | 🔴 HIGH | High |
| File format compatibility | 🟡 MEDIUM | High |
| Performance regression | 🟡 MEDIUM | Medium |
| Plugin dependency changes | 🟡 MEDIUM | Medium |

### Recommended Timeline

**Total Duration:** 18-24 months (with 5-7 developers)

- **Phase 0:** Foundation & Planning (Months 1-2)
- **Phase 1:** Core Infrastructure (Months 3-6)
- **Phase 2:** UI Framework (Months 7-12)
- **Phase 3:** Effects & Algorithms (Months 13-16)
- **Phase 4:** Format Support (Months 17-20)
- **Phase 5:** Testing & Optimization (Months 21-24)

---

## Phase 0: Foundation & Planning (Months 1-2)

### Objectives

Establish development infrastructure, validate technology choices, and create proof-of-concept implementations for critical components.

### Deliverables

#### 0.1 Development Environment Setup

**Timeline:** Week 1-2
**Effort:** 2-3 developer-weeks
**Owner:** Tech Lead + DevOps

**Tasks:**
- [ ] Set up .NET 10 MAUI development environment (Visual Studio 2025)
- [ ] Configure source control (Git repository structure)
- [ ] Establish CI/CD pipeline (GitHub Actions or Azure DevOps)
- [ ] Set up package management (NuGet feeds)
- [ ] Configure build configurations (Debug, Release, Platform-specific)
- [ ] Establish code standards and linting rules
- [ ] Set up issue tracking and project management tools

**Exit Criteria:**
- All developers can build and run sample MAUI app
- CI/CD pipeline successfully builds on all target platforms
- Code review process established

#### 0.2 Technology Validation

**Timeline:** Week 2-4
**Effort:** 4-6 developer-weeks
**Owner:** Senior Developers

**Proof-of-Concept Implementations:**

1. **Graphics Engine PoC** (Week 2-3)
   - Implement basic image loading/display with SkiaSharp
   - Test bitmap manipulation performance (LockBits, unsafe code)
   - Validate GDI+ → SkiaSharp rendering pipeline
   - Benchmark pixel-level operations
   - **Reference:** `/docs/api/api-inventory.md:40-89` (GDI+ APIs)

2. **Custom Control PoC** (Week 3)
   - Build prototype pdSlider control (most used control)
   - Test MAUI GraphicsView performance
   - Validate event handling and data binding
   - **Reference:** `/docs/ui/control-mapping.md:196-225` (pdSlider)

3. **Image Format PoC** (Week 3-4)
   - Test ImageSharp + Magick.NET integration
   - Validate PNG/JPEG loading performance
   - Test basic PSD parsing (header reading)
   - **Reference:** `/docs/formats/file-format-support.md:83-89` (PSD)

4. **Memory Management PoC** (Week 4)
   - Test Span<T> and Memory<T> for pixel operations
   - Validate 32→64-bit pointer migration patterns
   - Benchmark unsafe code vs safe alternatives
   - **Reference:** `/docs/data/type-mapping.md:1074-1146` (Pointer migration)

**Exit Criteria:**
- All PoCs demonstrate acceptable performance (within 2x of VB6)
- Technology choices validated or alternatives identified
- No blocking technical issues discovered
- Team confident in technology stack

#### 0.3 Architecture Design

**Timeline:** Week 4-6
**Effort:** 3-4 developer-weeks
**Owner:** Architects + Tech Lead

**Artifacts:**

1. **Solution Structure** (Week 4)
   ```
   PhotoDemon.sln
   ├── PhotoDemon.Core/              # Core data models, algorithms
   ├── PhotoDemon.Effects/            # Effects and filters
   ├── PhotoDemon.Formats/            # Image format parsers
   ├── PhotoDemon.Plugins/            # Plugin interfaces
   ├── PhotoDemon.UI.Controls/        # Custom MAUI controls
   ├── PhotoDemon.UI.Maui/            # MAUI application
   ├── PhotoDemon.Services/           # Services (undo, macro, etc.)
   ├── PhotoDemon.Interop/            # P/Invoke wrappers
   └── PhotoDemon.Tests/              # Unit and integration tests
   ```

2. **Dependency Injection Design** (Week 5)
   - Service registration patterns
   - Scoped vs singleton lifetimes
   - Interface abstractions
   - **Reference:** `/docs/architecture/vb6-architecture.md:88-108` (Global state)

3. **Data Model Architecture** (Week 5)
   - pdImage → Image class design
   - pdLayer → Layer class design
   - pdDIB → Bitmap abstraction
   - **Reference:** `/docs/architecture/vb6-architecture.md:164-199` (Data model)

4. **MVVM Pattern Definition** (Week 6)
   - ViewModels for all major forms
   - Command pattern for actions
   - Data binding strategies
   - **Reference:** `/docs/architecture/vb6-architecture.md:1645-1665` (Processor pattern)

**Exit Criteria:**
- Architecture documented and approved
- Solution structure created
- Coding patterns established
- Team aligned on approach

#### 0.4 Project Planning Finalization

**Timeline:** Week 6-8
**Effort:** 2-3 developer-weeks
**Owner:** Project Manager + Tech Lead

**Tasks:**
- [ ] Define detailed sprint/iteration plan
- [ ] Assign team roles and responsibilities
- [ ] Create risk register with mitigation plans
- [ ] Establish communication protocols
- [ ] Define testing strategy and QA process
- [ ] Set up performance baseline metrics
- [ ] Create migration toolkit (code generators, etc.)

**Exit Criteria:**
- Detailed project plan approved by stakeholders
- All team members onboarded
- Sprint 0 completed successfully

---

## Phase 1: Core Infrastructure (Months 3-6)

### Objectives

Build the foundational layers upon which all other components depend. This includes core data structures, memory management, type system, and basic I/O.

### Critical Path Components

These must be completed before other work can proceed.

### Deliverables

#### 1.1 Type System & Interop Layer (Month 3, Weeks 9-12)

**Timeline:** 4 weeks
**Effort:** 8-10 developer-weeks
**Owner:** Senior Developer (Type Systems)
**Complexity:** 🔴 HIGH

**Tasks:**

1. **Create Type Mapping Library** (Week 9)
   - Define all VB6-compatible structures
   - Implement marshaling attributes
   - Create geometry types (Point, Rect, etc.)
   - Implement color types (RGBQuad, etc.)
   - **Reference:** `/docs/data/type-mapping.md:46-78` (Primitive types)

2. **Windows API Wrappers** (Week 10)
   - GDI32 wrapper functions
   - Kernel32 wrapper functions
   - User32 wrapper functions
   - Handle management utilities
   - **Reference:** `/docs/api/api-inventory.md:64-89` (GDI APIs)

3. **Memory Management Abstraction** (Week 11)
   - SafeArray replacement with Span<T>
   - Bitmap data access wrapper
   - Memory pooling for large allocations
   - IDisposable pattern enforcement
   - **Reference:** `/docs/data/type-mapping.md:119-167` (SafeArray)

4. **Structure Size Validation** (Week 12)
   - Unit tests for all structure sizes
   - 32-bit vs 64-bit validation
   - Packing/alignment verification
   - File I/O structure tests
   - **Reference:** `/docs/data/type-mapping.md:1123-1146` (Structure sizes)

**Exit Criteria:**
- All 176+ Type declarations migrated
- Structure size tests pass (100% coverage)
- No P/Invoke marshaling errors
- Performance overhead < 10% vs native calls

#### 1.2 Core Data Model (Month 3-4, Weeks 10-14)

**Timeline:** 5 weeks
**Effort:** 12-15 developer-weeks
**Owner:** Senior Developer (Core)
**Complexity:** 🔴 HIGH
**Dependencies:** 1.1 Type System

**Tasks:**

1. **pdDIB → Bitmap Class** (Week 10-11)
   - Create Bitmap abstraction over SkiaSharp.SKBitmap
   - Implement LockBits/UnlockBits pattern
   - Add memory suspension support (disk backing)
   - Support 24-bit and 32-bit formats
   - **Reference:** `/docs/architecture/vb6-architecture.md:183-187` (pdDIB)

2. **pdLayer → Layer Class** (Week 12)
   - Layer types: raster, text, vector, adjustment
   - Properties: opacity, blend mode, transforms
   - Non-destructive transformation support
   - Layer mask support
   - **Reference:** `/docs/architecture/vb6-architecture.md:176-181` (pdLayer)

3. **pdImage → Image Class** (Week 13)
   - Image container with layer collection
   - Viewport management
   - Composite buffer caching
   - Metadata container
   - Undo/redo integration (stub)
   - **Reference:** `/docs/architecture/vb6-architecture.md:169-199` (pdImage)

4. **pdSelection → Selection Class** (Week 14)
   - Selection shapes: rectangle, ellipse, polygon, lasso, wand
   - Composite selections (add, subtract, intersect)
   - Feathering and border modes
   - Selection masks
   - **Reference:** `/docs/architecture/vb6-architecture.md:289-308` (Selection)

**Exit Criteria:**
- All core data classes implemented
- Unit tests with 80%+ coverage
- Memory leak tests passing
- Performance benchmarks established

#### 1.3 Rendering Engine Foundation (Month 4, Weeks 14-17)

**Timeline:** 4 weeks
**Effort:** 10-12 developer-weeks
**Owner:** Senior Developer (Graphics)
**Complexity:** 🔴 HIGH
**Dependencies:** 1.2 Core Data Model

**Tasks:**

1. **PD2D → Graphics2D Abstraction** (Week 14-15)
   - Unified 2D painting interface over SkiaSharp
   - Surface abstraction (pd2DSurface → SKSurface)
   - Brush abstraction (pd2DBrush → SKPaint)
   - Pen abstraction (pd2DPen → SKPaint)
   - Path abstraction (pd2DPath → SKPath)
   - **Reference:** `/docs/architecture/vb6-architecture.md:141-163` (Rendering engine)

2. **Compositor Implementation** (Week 16)
   - Layer blending (pdPixelBlender → custom blend modes)
   - Blend mode support (Normal, Multiply, Screen, etc.)
   - Alpha compositing
   - Hardware acceleration support
   - **Reference:** `/docs/architecture/vb6-architecture.md:149-150` (pdCompositor)

3. **Viewport & Canvas Rendering** (Week 17)
   - Viewport rendering pipeline
   - Zoom/pan support
   - Composite buffer management
   - Invalidation/redraw optimization
   - **Reference:** `/docs/architecture/vb6-architecture.md:1599-1613` (Rendering pipeline)

**Exit Criteria:**
- Basic rendering working (display image on canvas)
- Blend modes implemented and tested
- Performance: 60 FPS viewport rendering
- No memory leaks in rendering loop

#### 1.4 File I/O Foundation (Month 5, Weeks 18-21)

**Timeline:** 4 weeks
**Effort:** 10-12 developer-weeks
**Owner:** Senior Developer (File I/O)
**Complexity:** 🟡 MEDIUM
**Dependencies:** 1.2 Core Data Model

**Tasks:**

1. **Format Detection & Registry** (Week 18)
   - Migrate ImageFormats.bas logic
   - Extension to format mapping
   - Magic number detection
   - Format capability flags
   - **Reference:** `/docs/formats/file-format-support.md:885-905` (Format management)

2. **Standard Format Support** (Week 18-19)
   - Integrate ImageSharp for PNG, JPEG, GIF, BMP
   - Integrate Magick.NET as fallback
   - Format conversion utilities
   - **Reference:** `/docs/formats/file-format-support.md:92-105` (Standard formats)

3. **PDI Format (Critical)** (Week 19-20)
   - Migrate pdPackageChunky logic
   - Chunked data structure
   - Compression integration (LZ4, Zstd)
   - Backward compatibility with VB6 PDI files
   - **Reference:** `/docs/formats/file-format-support.md:70-90` (PDI format)

4. **Basic Load/Save Pipeline** (Week 21)
   - High-level load/save interface
   - Progress reporting
   - Error handling
   - Metadata preservation (stub)
   - **Reference:** `/docs/formats/file-format-support.md:885-905` (Load/save)

**Exit Criteria:**
- Can load/save: PNG, JPEG, GIF, BMP via ImageSharp
- Can load/save: PDI files (backward compatible)
- All existing PDI test files load correctly
- No data loss in round-trip tests

#### 1.5 Plugin Infrastructure (Month 5-6, Weeks 20-24)

**Timeline:** 5 weeks
**Effort:** 8-10 developer-weeks
**Owner:** Developer (Plugins)
**Complexity:** 🟡 MEDIUM
**Dependencies:** 1.4 File I/O

**Tasks:**

1. **Plugin Manager** (Week 20)
   - Plugin discovery and loading
   - Version checking
   - Graceful degradation
   - **Reference:** `/docs/architecture/vb6-architecture.md:362-401` (Plugin system)

2. **Compression Plugins** (Week 21)
   - Integrate K4os.Compression.LZ4
   - Integrate ZstdSharp.Port
   - System.IO.Compression for Deflate
   - **Reference:** `/docs/dependencies/dotnet-alternatives.md:280-339` (Compression)

3. **Color Management Plugin** (Week 22)
   - Integrate lcmsNET
   - ICC profile support
   - Color space conversions
   - **Reference:** `/docs/dependencies/dotnet-alternatives.md:343-419` (Color management)

4. **Metadata Plugin** (Week 23)
   - Integrate MetadataExtractor
   - EXIF, IPTC, XMP reading
   - Fallback to ExifTool.exe
   - **Reference:** `/docs/dependencies/dotnet-alternatives.md:531-588` (Metadata)

5. **Plugin Testing** (Week 24)
   - Integration tests for all plugins
   - Fallback testing
   - Performance benchmarks

**Exit Criteria:**
- All compression plugins working
- Color management functional
- Metadata reading working
- Plugin loading robust and tested

#### 1.6 Utility & Support Classes (Month 6, Weeks 22-26)

**Timeline:** 5 weeks (parallel with 1.5)
**Effort:** 6-8 developer-weeks
**Owner:** Junior/Mid Developer
**Complexity:** 🟢 LOW

**Tasks:**

1. **String Utilities** (Week 22)
   - pdString → StringBuilder extensions
   - Unicode handling
   - **Reference:** `/docs/architecture/vb6-architecture.md:510-513` (String utilities)

2. **Collections** (Week 23)
   - pdDictionary → Dictionary<K,V>
   - pdStack → Stack<T>
   - Custom collections as needed
   - **Reference:** `/docs/architecture/vb6-architecture.md:522-527` (Collections)

3. **Math Utilities** (Week 24)
   - PDMath functions
   - Color math utilities
   - Complex numbers (if needed)
   - **Reference:** `/docs/architecture/vb6-architecture.md:516-520` (Math)

4. **File System Utilities** (Week 25)
   - pdFSO → System.IO wrapper
   - Path manipulation
   - Directory operations
   - **Reference:** `/docs/architecture/vb6-architecture.md:569-571` (File system)

5. **Serialization** (Week 26)
   - pdXML → System.Xml.Linq
   - pdSerialize → JSON serialization
   - **Reference:** `/docs/architecture/vb6-architecture.md:574-575` (Serialization)

**Exit Criteria:**
- All utility classes ported
- Unit tests passing
- Performance acceptable

### Phase 1 Milestones & Exit Criteria

**Major Milestone: M1 - Core Infrastructure Complete**

**Timeline:** End of Month 6
**Exit Criteria:**
- ✅ All type mappings defined and tested
- ✅ Core data model (Image, Layer, Bitmap) working
- ✅ Basic rendering engine functional
- ✅ Can load/save PDI and common formats
- ✅ Plugin infrastructure operational
- ✅ All Phase 1 unit tests passing
- ✅ Performance benchmarks established
- ✅ No memory leaks detected
- ✅ Ready for UI development

**Deliverables for Stakeholder Review:**
- Demo: Load image → display → save
- Performance report: Benchmark vs VB6
- Technical documentation: Architecture and APIs
- Test coverage report: 80%+ coverage

---

## Phase 2: UI Framework (Months 7-12)

### Objectives

Build the MAUI UI framework including custom controls, themes, and main application windows. This is the largest and most complex phase.

### Critical Path Components

Custom controls must be built incrementally in dependency order.

### Deliverables

#### 2.1 Theme System (Month 7, Weeks 27-30)

**Timeline:** 4 weeks
**Effort:** 6-8 developer-weeks
**Owner:** UI Developer
**Complexity:** 🟡 MEDIUM

**Tasks:**

1. **pdThemeColors → ResourceDictionary** (Week 27)
   - Define color schemes (Dark, Light, custom)
   - Implement dynamic theme switching
   - Color resource bindings
   - **Reference:** `/docs/ui/control-mapping.md:2056-2073` (Theming)

2. **Style Definitions** (Week 28)
   - Base control styles
   - Typography system
   - Spacing/padding standards
   - Border/shadow styles

3. **Theme Manager Service** (Week 29)
   - Theme loading/switching
   - Persistence (user preferences)
   - Theme preview

4. **Testing** (Week 30)
   - Theme switching tests
   - Color contrast verification
   - Accessibility compliance

**Exit Criteria:**
- Theme system working
- Dark and Light themes defined
- Theme switching instant (< 100ms)
- Accessible color contrast ratios

#### 2.2 Basic Controls (Month 7-8, Weeks 28-32)

**Timeline:** 5 weeks (parallel with 2.1)
**Effort:** 10-12 developer-weeks
**Owner:** 2x UI Developers
**Complexity:** 🟢 LOW-MEDIUM
**Dependencies:** 2.1 Theme System

**Priority Order:**

1. **pdLabel → Styled Label** (Week 28)
   - Custom font size support
   - Auto-sizing
   - Theme integration
   - **Usage:** 330 instances
   - **Reference:** `/docs/ui/control-mapping.md:429-449` (pdLabel)

2. **pdButton → Styled Button** (Week 28)
   - Custom templates
   - Image support
   - Owner-draw mode
   - Drag/drop support
   - **Usage:** 104 instances
   - **Reference:** `/docs/ui/control-mapping.md:390-413` (pdButton)

3. **pdCheckBox → Styled CheckBox** (Week 29)
   - Custom rendering
   - Theme-aware
   - **Usage:** 201 instances
   - **Reference:** `/docs/ui/control-mapping.md:260-269` (pdCheckBox)

4. **pdRadioButton → Styled RadioButton** (Week 29)
   - Auto-grouping
   - Custom rendering
   - **Usage:** 19 instances
   - **Reference:** `/docs/ui/control-mapping.md:271-280` (pdRadioButton)

5. **pdTextBox → Styled Entry** (Week 30)
   - Unicode support (native in MAUI)
   - Custom border rendering
   - **Usage:** 41 instances
   - **Reference:** `/docs/ui/control-mapping.md:240-258` (pdTextBox)

6. **pdContainer → Frame/Border** (Week 30)
   - Layout container
   - Collapsible sections
   - **Usage:** 137 instances
   - **Reference:** `/docs/ui/control-mapping.md:519-540` (pdContainer)

7. **pdTitle → Custom Label** (Week 31)
   - Section headers
   - Visual separator
   - Collapsible support
   - **Usage:** 55 instances
   - **Reference:** `/docs/ui/control-mapping.md:451-460` (pdTitle)

8. **pdProgressBar → Styled ProgressBar** (Week 31)
   - Custom rendering
   - Percentage display
   - Indeterminate mode
   - **Usage:** 3 instances
   - **Reference:** `/docs/ui/control-mapping.md:507-515` (pdProgressBar)

9. **pdHyperlink → Custom Control** (Week 32)
   - Clickable links
   - URL navigation
   - Event raising
   - **Usage:** 21 instances
   - **Reference:** `/docs/ui/control-mapping.md:462-483` (pdHyperlink)

**Exit Criteria:**
- 9 basic controls implemented
- All themed correctly
- Unit tests for each control
- Demo app with all controls

#### 2.3 Input Controls (Month 8-9, Weeks 33-38)

**Timeline:** 6 weeks
**Effort:** 14-18 developer-weeks
**Owner:** 2x UI Developers
**Complexity:** 🔴 HIGH
**Dependencies:** 2.2 Basic Controls

**Priority Order:**

1. **pdSlider (HIGHEST PRIORITY)** (Week 33-34)
   - Composite: Slider + Entry
   - Value synchronization
   - SigDigits property
   - Caption support
   - FinalChange event
   - Locale-aware number formatting
   - **Usage:** 454 instances (MOST USED)
   - **Effort:** 2 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1336-1375` (pdSlider PoC)

2. **pdSpinner** (Week 35)
   - Stepper + Entry composite
   - Validation logic
   - Min/Max bounds
   - **Usage:** 72 instances + embedded in pdSlider
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:227-238` (pdSpinner)

3. **pdDropDown** (Week 35-36)
   - Styled Picker
   - Custom item templates
   - No-edit mode (by design)
   - **Usage:** 132 instances
   - **Effort:** 1.5 weeks
   - **Reference:** `/docs/ui/control-mapping.md:303-318` (pdDropDown)

4. **pdButtonStrip** (Week 36-37)
   - Segmented control (no MAUI equivalent)
   - Single selection mode
   - Auto-sizing
   - **Usage:** 169 instances
   - **Effort:** 2 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1448-1481` (pdButtonStrip PoC)

5. **pdColorSelector** (Week 37-38)
   - Color picker button
   - Modal color picker dialog
   - Main window color quick-select
   - **Usage:** 60 instances
   - **Effort:** 2 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1487-1526` (pdColorSelector PoC)

**Exit Criteria:**
- 5 input controls complete
- pdSlider performance acceptable (< 50ms update)
- Color picker functional
- All controls tested

#### 2.4 Complex Display Controls (Month 9-10, Weeks 38-44)

**Timeline:** 7 weeks
**Effort:** 16-20 developer-weeks
**Owner:** 2x Senior UI Developers
**Complexity:** 🔴 VERY HIGH
**Dependencies:** 2.3 Input Controls

**Priority Order:**

1. **pdFxPreviewCtl (CRITICAL)** (Week 38-40)
   - Effect preview with before/after toggle
   - Fit vs 100% zoom
   - Click interactions
   - Async preview generation
   - **Usage:** 134 instances
   - **Effort:** 3 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1301-1346` (pdFxPreview PoC)

2. **pdPictureBox / pdPictureBoxInteractive** (Week 41)
   - Image display with SkiaSharp
   - Touch interaction support
   - **Usage:** 35+6 instances
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:486-501` (pdPictureBox)

3. **pdColorWheel** (Week 42)
   - HSV wheel rendering
   - Interactive color selection
   - Touch gesture handling
   - **Usage:** 2 instances
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:1556-1575` (pdColorWheel PoC)

4. **pdGradientSelector** (Week 43-44)
   - Gradient preview bar
   - Draggable color stops
   - Add/remove stops
   - Modal stop editor
   - **Usage:** 5 instances
   - **Effort:** 2 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1528-1555` (pdGradientSelector PoC)

**Exit Criteria:**
- pdFxPreviewCtl working with live previews
- Color wheel functional
- Gradient editor usable
- Performance: 60 FPS rendering

#### 2.5 Command & Layout Controls (Month 10-11, Weeks 42-48)

**Timeline:** 7 weeks (parallel with 2.4)
**Effort:** 12-16 developer-weeks
**Owner:** UI Developer
**Complexity:** 🔴 HIGH
**Dependencies:** 2.2 Basic Controls, 2.3 Input Controls

**Priority Order:**

1. **pdCommandBar (CRITICAL)** (Week 42-44)
   - OK/Cancel/Reset buttons
   - Preset management system
   - Randomize button
   - XML serialization
   - **Usage:** 161 instances (ALL effect dialogs)
   - **Effort:** 3 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1376-1410` (pdCommandBar PoC)

2. **pdCommandBarMini** (Week 45)
   - Simplified command bar
   - **Usage:** 21 instances
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:571-578` (pdCommandBarMini)

3. **pdStatusBar** (Week 46)
   - Multi-section status display
   - Progress integration
   - Message queue
   - **Usage:** Part of main window
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:1676-1686` (pdStatusBar)

4. **List Controls** (Week 47)
   - pdListBox → CollectionView
   - pdDropDownFont → Custom Picker
   - **Usage:** 20+2 instances
   - **Effort:** 1 week

5. **Specialized Option Panels** (Week 48)
   - pdMetadataExport
   - pdRandomizeUI
   - pdColorDepth
   - **Usage:** 9+8+2 instances
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:1611-1631` (Option panels)

**Exit Criteria:**
- pdCommandBar fully functional
- All effect dialogs can use command bar
- Status bar working
- Option panels complete

#### 2.6 Core Canvas System (Month 11-12, Weeks 46-54)

**Timeline:** 9 weeks
**Effort:** 20-28 developer-weeks
**Owner:** 2x Senior Developers (Graphics)
**Complexity:** 🔴 VERY HIGH
**Dependencies:** Phase 1 Rendering, 2.2 Basic Controls

**Tasks:**

1. **pdCanvas - Basic (Week 46-48)**
   - Image display
   - Zoom/pan
   - Basic rendering
   - Drag/drop support
   - Welcome screen
   - **Usage:** 1 instance (but CRITICAL)
   - **Effort:** 3 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1286-1323` (pdCanvas PoC)

2. **pdCanvas - Advanced (Week 48-51)**
   - Multi-layer rendering
   - Selection overlay
   - Tool integration
   - Rulers integration
   - Performance optimization
   - **Effort:** 4 weeks
   - **Reference:** `/docs/ui/control-mapping.md:605-628` (pdCanvas)

3. **pdCanvasView** (Week 51-52)
   - Viewport management
   - Coordinate systems
   - Scroll synchronization
   - **Usage:** Used internally by pdCanvas
   - **Effort:** 2 weeks

4. **pdNavigator** (Week 52)
   - Minimap thumbnail
   - Viewport indicator
   - Click-to-navigate
   - **Usage:** 1 instance
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:1576-1595` (pdNavigator PoC)

5. **pdRuler** (Week 53)
   - Measurement overlays
   - Unit conversion
   - Guide creation
   - **Usage:** Part of canvas system
   - **Effort:** 1 week
   - **Reference:** `/docs/ui/control-mapping.md:1660-1673` (pdRuler)

6. **Integration & Testing** (Week 54)
   - Canvas performance profiling
   - Memory leak testing
   - Large image testing
   - Tool interaction testing

**Exit Criteria:**
- Canvas displays images correctly
- Zoom/pan smooth (60 FPS)
- Can handle 100MP+ images
- Selection overlay working
- Navigator functional
- No memory leaks

#### 2.7 Layer & History Panels (Month 12-13, Weeks 50-58)

**Timeline:** 9 weeks (parallel with 2.6)
**Effort:** 16-22 developer-weeks
**Owner:** 2x UI Developers
**Complexity:** 🔴 VERY HIGH
**Dependencies:** Phase 1 Core Data Model, 2.2 Basic Controls

**Tasks:**

1. **pdLayerList (Week 50-53)**
   - Layer display with thumbnails
   - Drag-to-reorder
   - Visibility toggles
   - Blend mode dropdowns
   - Opacity sliders
   - CollectionView + DataTemplate
   - **Usage:** 1 instance (CRITICAL)
   - **Effort:** 4 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1411-1446` (pdLayerList PoC)

2. **pdLayerListInner (Week 53)**
   - Rendering logic
   - Interaction handling
   - Thumbnail generation
   - **Usage:** Used internally
   - **Effort:** 1 week

3. **pdHistory (Week 54-55)**
   - Undo/redo list
   - Stack management
   - Command integration
   - ListView with custom template
   - **Usage:** 2 instances
   - **Effort:** 2 weeks
   - **Reference:** `/docs/ui/control-mapping.md:1646-1659` (pdHistory)

4. **Integration & Testing (Week 56-58)**
   - Layer panel performance
   - Drag/drop testing
   - Thumbnail generation performance
   - Memory management

**Exit Criteria:**
- Layer list functional
- Drag-to-reorder working
- Thumbnails generate correctly
- History panel working
- Performance acceptable

#### 2.8 Main Application Windows (Month 13, Weeks 54-58)

**Timeline:** 5 weeks (parallel with 2.7)
**Effort:** 10-14 developer-weeks
**Owner:** 2x UI Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** All Phase 2 controls

**Tasks:**

1. **Main Window (Week 54-56)**
   - MDI-style layout (MAUI TabbedPage or custom)
   - Menu system
   - Toolbar integration
   - Status bar integration
   - **Reference:** `/docs/architecture/vb6-architecture.md:493-495` (MainWindow)

2. **Layer Panels Container (Week 56)**
   - Layerpanel_Layers
   - Layerpanel_Colors
   - Layerpanel_Navigator
   - Layerpanel_Search
   - **Reference:** `/docs/architecture/vb6-architecture.md:1076-1083` (Layer panels)

3. **Toolbars (Week 57)**
   - Tool selection toolbar
   - Layer operations toolbar
   - Tool options container
   - **Reference:** `/docs/architecture/vb6-architecture.md:1085-1091` (Toolbars)

4. **Dialogs & Testing (Week 58)**
   - About dialog
   - Preferences dialog
   - Generic dialogs (message box, etc.)
   - Navigation testing
   - Theme switching testing

**Exit Criteria:**
- Main window structure complete
- Can open/close images
- Panels dockable/resizable
- Menu system functional
- All dialogs working

### Phase 2 Milestones & Exit Criteria

**Major Milestone: M2 - UI Framework Complete**

**Timeline:** End of Month 13
**Exit Criteria:**
- ✅ All 56 custom controls implemented
- ✅ Theme system working (Dark/Light)
- ✅ Canvas displays images with zoom/pan
- ✅ Layer panel functional
- ✅ Main window structure complete
- ✅ All UI unit tests passing
- ✅ Performance: 60 FPS UI interactions
- ✅ No memory leaks in UI layer
- ✅ Ready for effects development

**Deliverables for Stakeholder Review:**
- Demo: Full UI walkthrough
- Performance report: UI responsiveness
- Accessibility audit results
- User feedback from alpha testers

---

## Phase 3: Effects & Algorithms (Months 14-17)

### Objectives

Implement all 117+ image processing effects and adjustments. Focus on performance-critical algorithms first.

### Critical Path Components

Core adjustments must be complete before effects dialogs.

### Deliverables

#### 3.1 Effect Infrastructure (Month 14, Weeks 59-62)

**Timeline:** 4 weeks
**Effort:** 8-10 developer-weeks
**Owner:** Senior Developer (Effects)
**Complexity:** 🟡 MEDIUM
**Dependencies:** Phase 1 Core, Phase 2 UI (pdCommandBar, pdFxPreviewCtl)

**Tasks:**

1. **Effect Pipeline Architecture (Week 59)**
   - IEffect interface definition
   - Effect parameter system (XML-based)
   - Progress reporting
   - Cancellation support
   - **Reference:** `/docs/algorithms/effects-catalog.md:343-362` (Filter modules)

2. **Effect Dialog Base (Week 60)**
   - Base class for all effect dialogs
   - pdCommandBar integration
   - pdFxPreviewCtl integration
   - Parameter serialization/deserialization
   - Preset management

3. **Effect Registration System (Week 61)**
   - Effect discovery
   - Category organization
   - Menu integration
   - Keyboard shortcut mapping

4. **Performance Infrastructure (Week 62)**
   - Parallel processing support
   - SIMD optimization helpers
   - Progress tracking
   - Memory pooling
   - **Reference:** `/docs/algorithms/effects-catalog.md:415-455` (Performance)

**Exit Criteria:**
- Effect infrastructure ready
- Can add new effects easily
- Preview system working
- Parameter system functional

#### 3.2 Core Adjustments (Month 14-15, Weeks 62-68)

**Timeline:** 7 weeks
**Effort:** 16-20 developer-weeks
**Owner:** 2x Developers (Algorithms)
**Complexity:** 🟡 MEDIUM-HIGH
**Dependencies:** 3.1 Effect Infrastructure
**Priority:** 🔴 HIGHEST (Most used effects)

**Implementation Order:**

1. **Basic Color Adjustments (Week 62-63)**
   - Brightness/Contrast
   - Hue/Saturation/Lightness
   - Vibrance
   - White Balance
   - Temperature/Tint
   - **Reference:** `/docs/algorithms/effects-catalog.md:80-101` (Color adjustments)

2. **Tone Curve Adjustments (Week 64-65)**
   - Levels (histogram mapping)
   - Curves (spline interpolation)
   - Gamma correction
   - Exposure
   - **Reference:** `/docs/algorithms/effects-catalog.md:113-126` (Tone adjustments)

3. **Channel Operations (Week 66)**
   - Channel Mixer
   - Rechannel
   - Shift channels
   - Maximum/Minimum channel
   - **Reference:** `/docs/algorithms/effects-catalog.md:103-111` (Channel ops)

4. **Advanced Color (Week 67)**
   - Color Balance
   - Colorize
   - Replace Color
   - Sepia/Grayscale
   - Split Toning
   - **Reference:** `/docs/algorithms/effects-catalog.md:80-101` (Advanced color)

5. **Histogram Operations (Week 68)**
   - Histogram Equalization
   - Auto Correct
   - Auto Enhance
   - **Reference:** `/docs/algorithms/effects-catalog.md:127-132` (Histogram)

**Exit Criteria:**
- All 28 adjustments implemented
- Color accuracy verified (vs VB6)
- Performance within 2x of VB6
- All adjustment dialogs working

#### 3.3 Performance-Critical Effects (Month 15-16, Weeks 66-72)

**Timeline:** 7 weeks (parallel with 3.2)
**Effort:** 16-22 developer-weeks
**Owner:** 2x Senior Developers (Performance)
**Complexity:** 🔴 VERY HIGH
**Dependencies:** 3.1 Effect Infrastructure
**Priority:** 🔴 HIGHEST (Performance critical)

**Implementation Order:**

1. **Gaussian Blur - IIR Implementation (Week 66-68)**
   - Alvarez-Mazorra algorithm
   - Deriche IIR filter
   - Separable convolution
   - Multi-threading
   - SIMD optimization
   - **Effort:** 3 weeks (CRITICAL)
   - **Reference:** `/docs/algorithms/effects-catalog.md:366-386` (Gaussian blur)

2. **Convolution Engine (Week 68-69)**
   - Generic convolution kernel
   - Box blur
   - Sharpen/Unsharp mask
   - Edge detection (Sobel, Prewitt)
   - **Effort:** 2 weeks
   - **Reference:** `/docs/algorithms/effects-catalog.md:388-400` (Convolution)

3. **Surface Blur / Bilateral Filter (Week 70)**
   - Edge-preserving blur
   - Noise reduction
   - **Effort:** 1 week
   - **Reference:** `/docs/algorithms/effects-catalog.md:179-189` (Blur effects)

4. **Other Blur Effects (Week 71)**
   - Motion Blur
   - Radial Blur
   - Zoom Blur
   - Kuwahara filter
   - **Effort:** 1 week

5. **Performance Testing & Optimization (Week 72)**
   - Benchmark all blur variants
   - Profile and optimize hot paths
   - GPU acceleration exploration
   - Memory optimization

**Exit Criteria:**
- Gaussian blur within 1.5x of VB6 speed
- All blur effects < 2x VB6 speed
- Unsharp mask functional
- Edge detection working

#### 3.4 Geometric Transformations (Month 16, Weeks 70-74)

**Timeline:** 5 weeks (parallel with 3.3)
**Effort:** 10-14 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🔴 HIGH
**Dependencies:** 3.1 Effect Infrastructure

**Implementation Order:**

1. **Basic Transforms (Week 70-71)**
   - Rotate (arbitrary angle)
   - Flip/Mirror
   - Offset and Zoom
   - Shear
   - **Reference:** `/docs/algorithms/effects-catalog.md:320-330` (Transform)

2. **Advanced Transforms (Week 72-73)**
   - Perspective (4-point)
   - Polar conversion
   - Spherize
   - **Reference:** `/docs/algorithms/effects-catalog.md:320-330` (Advanced transform)

3. **Interpolation Methods (Week 74)**
   - Nearest neighbor
   - Bilinear
   - Bicubic
   - Lanczos
   - **Reference:** `/docs/architecture/vb6-architecture.md:520` (Resampling)

**Exit Criteria:**
- All 6 transform effects working
- High-quality interpolation
- Performance acceptable
- No edge artifacts

#### 3.5 Artistic & Stylize Effects (Month 16-17, Weeks 73-78)

**Timeline:** 6 weeks (parallel with 3.4)
**Effort:** 14-18 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** 3.1 Effect Infrastructure, 3.3 Convolution

**Implementation Order:**

1. **Edge-Based Effects (Week 73-74)**
   - Colored Pencil
   - Comic Book
   - Oil Painting (Kuwahara)
   - Outline/Find Edges
   - **Reference:** `/docs/algorithms/effects-catalog.md:159-173` (Artistic)

2. **Pattern Effects (Week 75)**
   - Kaleidoscope
   - Stained Glass (Voronoi)
   - Glass Tiles
   - **Reference:** `/docs/algorithms/effects-catalog.md:159-173` (Artistic)

3. **Tone Effects (Week 76)**
   - Film Noir
   - Posterize
   - Antique
   - Solarize
   - Portrait Glow
   - **Reference:** `/docs/algorithms/effects-catalog.md:305-316` (Stylize)

4. **Other Effects (Week 77)**
   - Plastic Wrap
   - Relief
   - Diffuse
   - Twins
   - Vignetting
   - **Reference:** `/docs/algorithms/effects-catalog.md:305-316` (Stylize)

5. **Modern Art & Testing (Week 78)**
   - Modern Art
   - Figured Glass
   - Integration testing

**Exit Criteria:**
- All 12 artistic effects working
- All 9 stylize effects working
- Visual quality matches VB6
- Performance acceptable

#### 3.6 Distortion Effects (Month 17, Weeks 76-80)

**Timeline:** 5 weeks (parallel with 3.5)
**Effort:** 12-16 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🔴 HIGH
**Dependencies:** 3.1 Effect Infrastructure

**Implementation Order:**

1. **Basic Distortions (Week 76-77)**
   - Lens (fish-eye)
   - Correct Lens Distortion
   - Pinch and Whirl
   - Poke
   - **Reference:** `/docs/algorithms/effects-catalog.md:193-209` (Distort)

2. **Wave Distortions (Week 78)**
   - Ripple
   - Waves
   - Swirl
   - Squish

3. **Advanced Distortions (Week 79-80)**
   - Donut
   - Droste (VERY COMPLEX - logarithmic spiral)
   - Miscellaneous distortions
   - **Reference:** `/docs/algorithms/effects-catalog.md:193-209` (Distort)

**Exit Criteria:**
- All 12 distortion effects working
- Droste effect functional (accept some complexity)
- Performance acceptable
- No crashes on edge cases

#### 3.7 Remaining Effect Categories (Month 17-18, Weeks 78-82)

**Timeline:** 5 weeks (parallel with 3.6)
**Effort:** 12-16 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** 3.1 Effect Infrastructure

**Categories:**

1. **Pixelate Effects (Week 78-79)**
   - Mosaic
   - Crystallize (Voronoi)
   - Color Halftone
   - Mezzotint
   - Pointillize
   - Fragment
   - **Reference:** `/docs/algorithms/effects-catalog.md:268-279` (Pixelate)

2. **Noise Effects (Week 80)**
   - Add RGB Noise
   - Add Film Grain
   - Median filter
   - Dust and Scratches
   - Harmonic Mean
   - SNN
   - **Reference:** `/docs/algorithms/effects-catalog.md:252-264` (Noise)

3. **Render Effects (Week 81)**
   - Clouds (Perlin/Simplex noise)
   - Fibers
   - Truchet tiles
   - **Reference:** `/docs/algorithms/effects-catalog.md:283-289` (Render)

4. **Light/Shadow & Natural (Week 82)**
   - Black Light, Bump Map, Cross-Screen, Rainbow, Sunshine
   - Atmosphere, Fog, Ignite, Lava, Metal, Snow, Underwater
   - **Reference:** `/docs/algorithms/effects-catalog.md:223-249` (Light/Natural)

**Exit Criteria:**
- All effect categories complete
- 100+ effects implemented
- All effect dialogs working
- Performance acceptable

#### 3.8 Advanced Noise Algorithms (Month 18, Week 82)

**Timeline:** 1 week (optional - can defer)
**Effort:** 4-6 developer-weeks
**Owner:** Senior Developer
**Complexity:** 🔴 VERY HIGH
**Dependencies:** 3.7 Noise Effects

**Optional/Deferred:**

1. **Anisotropic Diffusion**
   - PDE-based smoothing
   - Very complex algorithm
   - **Reference:** `/docs/algorithms/effects-catalog.md:258` (Anisotropic)

2. **Mean Shift**
   - Iterative clustering
   - Very complex algorithm
   - **Reference:** `/docs/algorithms/effects-catalog.md:261` (Mean shift)

**Note:** These can be deferred to post-MVP if time constrained.

**Exit Criteria:**
- Anisotropic diffusion working (or deferred)
- Mean shift working (or deferred)
- Documentation of limitations

### Phase 3 Milestones & Exit Criteria

**Major Milestone: M3 - Effects Complete**

**Timeline:** End of Month 18
**Exit Criteria:**
- ✅ All 117+ effects implemented (or 115+ if advanced noise deferred)
- ✅ All effect dialogs functional
- ✅ Performance within 2x of VB6 for most effects
- ✅ Gaussian blur within 1.5x of VB6
- ✅ Visual quality matches VB6
- ✅ Effect parameter system working
- ✅ Preview system responsive
- ✅ All effects tested
- ✅ Ready for format support

**Deliverables for Stakeholder Review:**
- Demo: All effect categories
- Performance report: Effect benchmarks
- Quality comparison: VB6 vs .NET outputs
- User feedback from beta testers

---

## Phase 4: Format Support (Months 19-21)

### Objectives

Complete image format support by porting custom parsers and integrating modern codec libraries.

### Critical Path Components

PDI format and PSD parser are critical path items.

### Deliverables

#### 4.1 Modern Format Support (Month 19, Weeks 83-86)

**Timeline:** 4 weeks
**Effort:** 8-10 developer-weeks
**Owner:** Developer (Formats)
**Complexity:** 🟡 MEDIUM
**Dependencies:** Phase 1 File I/O

**Tasks:**

1. **WebP Integration (Week 83)**
   - Integrate libwebp wrapper
   - Animation support
   - Lossy/lossless encoding
   - **Reference:** `/docs/formats/file-format-support.md:148-165` (WebP)

2. **HEIF/AVIF Integration (Week 84)**
   - Test Magick.NET HEIF support
   - Integrate libheif wrapper if needed
   - AVIF process wrapper (avifdec/avifenc.exe)
   - **Reference:** `/docs/formats/file-format-support.md:51-66` (Modern formats)

3. **JPEG XL Support (Week 85)**
   - Process wrapper (djxl/cjxl.exe)
   - Animation support
   - Test on Windows 11 24H2+ WIC codec
   - **Reference:** `/docs/formats/file-format-support.md:398-415` (JXL)

4. **Testing & Optimization (Week 86)**
   - Multi-format load/save tests
   - Performance benchmarks
   - Quality verification

**Exit Criteria:**
- WebP, HEIF, AVIF, JXL all working
- Animation support functional
- Performance acceptable
- All test files load correctly

#### 4.2 PSD Parser Migration (Month 19-20, Weeks 85-92)

**Timeline:** 8 weeks
**Effort:** 18-24 developer-weeks
**Owner:** 2x Senior Developers
**Complexity:** 🔴 VERY HIGH
**Priority:** 🔴 HIGHEST (Competitive advantage)
**Dependencies:** Phase 1 File I/O

**Tasks:**

1. **PSD File Structure (Week 85-86)**
   - Port header parsing
   - Color mode handling (all 9 modes)
   - Resource section parsing
   - Layer and mask info section
   - Image data section
   - **Reference:** `/docs/formats/file-format-support.md:196-223` (PSD parser)

2. **Layer Parsing (Week 87-88)**
   - Layer records
   - Layer masks
   - Adjustment layers
   - Layer groups
   - **Reference:** `/docs/classes/pdPSD.cls` (source code)

3. **Compression Handlers (Week 89)**
   - RLE (PackBits) - native implementation
   - DEFLATE - via libdeflate
   - ZIP - via .NET
   - **Reference:** `/docs/formats/file-format-support.md:203-206` (Compression)

4. **Color Management (Week 90)**
   - ICC profile support
   - Color mode conversions (RGB, CMYK, Lab, etc.)
   - 1-32 bits per channel support
   - **Reference:** `/docs/formats/file-format-support.md:205-207` (Color management)

5. **PSB Support (Week 91)**
   - Large document format
   - 64-bit file offsets
   - Extended dimensions

6. **Testing & Validation (Week 92)**
   - Apple color management test suite
   - Photoshop compatibility tests
   - Round-trip testing
   - Performance benchmarks

**Exit Criteria:**
- Can load all PSD color modes
- Passes Apple test suite
- Layer support functional
- ICC profile handling correct
- PSB support working
- Round-trip preserves data

#### 4.3 PSP Parser Migration (Month 20-21, Weeks 90-96)

**Timeline:** 7 weeks (parallel with 4.2)
**Effort:** 16-20 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🔴 VERY HIGH
**Priority:** 🔴 HIGH (Unique capability)
**Dependencies:** Phase 1 File I/O

**Tasks:**

1. **PSP Block Structure (Week 90-91)**
   - Block-based parser
   - Version detection (v5-current)
   - General image header
   - Layer block
   - Channel block
   - **Reference:** `/docs/formats/file-format-support.md:224-246` (PSP parser)

2. **Layer Handling (Week 92-93)**
   - Raster layers
   - Vector layers (preserve data)
   - Layer groups (dummy layer workaround)
   - Composite images
   - **Reference:** `/docs/classes/pdPSP.cls` (source code)

3. **Compression & Special Features (Week 94)**
   - RLE compression
   - JPEG composites
   - Tube/frame/mask variants
   - ICC profiles

4. **Testing & Validation (Week 95-96)**
   - PSP v5-current file tests
   - Round-trip testing
   - Compatibility verification

**Exit Criteria:**
- Can load PSP v5-current
- Layer support functional
- Composite images load correctly
- Round-trip preserves data
- Vector layer data preserved

#### 4.4 XCF Parser Migration (Month 21, Weeks 94-98)

**Timeline:** 5 weeks (parallel with 4.3)
**Effort:** 12-16 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🔴 VERY HIGH
**Priority:** 🔴 HIGH (GIMP interop)
**Dependencies:** Phase 1 File I/O

**Tasks:**

1. **XCF Structure & Precision Support (Week 94-95)**
   - File header parsing
   - Precision support (8/16/32/64-bit int/float)
   - Color modes (RGB, Grayscale, Indexed)
   - **Reference:** `/docs/formats/file-format-support.md:247-267` (XCF parser)

2. **Tile-Based Loading (Week 96)**
   - Tile structure
   - RLE compression
   - ZLib compression
   - GZip file variants (.xcfgz)
   - **Reference:** `/docs/classes/pdXCF.cls` (source code)

3. **Layer Properties & Parasites (Week 97)**
   - Layer properties
   - Layer modes
   - Parasites (metadata)
   - Color space handling (linear/gamma)

4. **Testing & Validation (Week 98)**
   - GIMP compatibility tests
   - All precision tests
   - Round-trip testing

**Exit Criteria:**
- All XCF precisions load correctly
- Tile-based loading functional
- GZip variants working
- GIMP compatibility verified

#### 4.5 Other Custom Parsers (Month 21, Weeks 96-100)

**Timeline:** 5 weeks (parallel with 4.4)
**Effort:** 10-14 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** Phase 1 File I/O

**Priority Order:**

1. **PNG/APNG Parser (Week 96-97)**
   - Port pdPNG or use ImageSharp
   - APNG animation support
   - Chunk-based architecture
   - **Decision:** Evaluate if ImageSharp APNG support is sufficient
   - **Reference:** `/docs/formats/file-format-support.md:94` (PNG)

2. **GIF Parser (Week 98)**
   - Port pdGIF or use ImageSharp
   - Animation support
   - **Decision:** Evaluate if ImageSharp GIF animation is sufficient
   - **Reference:** `/docs/formats/file-format-support.md:96` (GIF)

3. **ICO Parser (Week 99)**
   - Port pdICO
   - Multiple icon sizes
   - PNG-compressed icons
   - **Reference:** `/docs/formats/file-format-support.md:99` (ICO)

4. **Other Formats (Week 100)**
   - QOI (pdQOI) - simple format
   - PCX/DCX (pdPCX) - legacy format
   - MBM (pdMBM) - Symbian format
   - **Reference:** `/docs/formats/file-format-support.md:148-160` (Legacy)

**Exit Criteria:**
- APNG support verified
- GIF animation working
- ICO multi-size working
- Legacy formats functional (if ported)

#### 4.6 Specialized Format Support (Month 21-22, Weeks 98-104)

**Timeline:** 7 weeks (parallel with 4.5)
**Effort:** 12-16 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** Phase 1 Plugin Infrastructure

**Tasks:**

1. **JPEG 2000 (Week 98-99)**
   - Integrate Magick.NET OpenJPEG support
   - Or use CSJ2K pure .NET library
   - Test all JP2 containers (JP2, J2K, JPX, etc.)
   - **Reference:** `/docs/formats/file-format-support.md:107-118` (JP2)

2. **DDS Texture Support (Week 100)**
   - Process wrapper for texconv.exe
   - BC1-BC7 compression
   - Mipmap support
   - **Reference:** `/docs/formats/file-format-support.md:138-143` (DDS)

3. **RAW Format Support (Week 101)**
   - Integrate Magick.NET LibRaw support
   - Test common camera RAW formats
   - DNG support
   - **Reference:** `/docs/formats/file-format-support.md:119-128` (RAW)

4. **Vector Format Support (Week 102)**
   - SVG: Integrate SkiaSharp.Svg
   - PDF: Integrate PDFiumSharp
   - EMF/WMF: Evaluate .NET support
   - **Reference:** `/docs/formats/file-format-support.md:162-169` (Vector)

5. **HDR/Scientific Formats (Week 103)**
   - HDR/RGBE: via Magick.NET
   - EXR: via Magick.NET
   - PFM: via Magick.NET
   - **Reference:** `/docs/formats/file-format-support.md:130-136` (HDR)

6. **Testing & Integration (Week 104)**
   - Comprehensive format test suite
   - Performance benchmarks
   - Quality verification

**Exit Criteria:**
- JPEG 2000 working
- DDS support functional
- RAW formats load correctly
- SVG/PDF rasterization working
- HDR formats functional

### Phase 4 Milestones & Exit Criteria

**Major Milestone: M4 - Format Support Complete**

**Timeline:** End of Month 22
**Exit Criteria:**
- ✅ All 40+ import formats supported
- ✅ All 25+ export formats supported
- ✅ PSD parser ported and passing Apple test suite
- ✅ PSP parser ported and functional
- ✅ XCF parser ported and functional
- ✅ PDI backward compatibility verified (100+ test files)
- ✅ Modern formats (WebP, HEIF, AVIF, JXL) working
- ✅ Animation support functional
- ✅ All format tests passing
- ✅ No data loss in round-trip tests

**Deliverables for Stakeholder Review:**
- Demo: Load/save all supported formats
- Quality report: PSD Apple test suite results
- Compatibility report: Legacy file loading
- Performance report: Format I/O benchmarks

---

## Phase 5: Integration & Polish (Months 22-24)

### Objectives

Complete remaining features, integrate all systems, optimize performance, and prepare for release.

### Deliverables

#### 5.1 Undo/Redo System (Month 22, Weeks 99-102)

**Timeline:** 4 weeks
**Effort:** 8-12 developer-weeks
**Owner:** Senior Developer
**Complexity:** 🔴 HIGH
**Dependencies:** Phase 1 Core Data Model

**Tasks:**

1. **pdUndo → Undo Manager (Week 99-100)**
   - Diff-based storage (Git-like)
   - HDD-backed (unlimited history)
   - PDI file format for undo data
   - **Reference:** `/docs/architecture/vb6-architecture.md:346-361` (Undo system)

2. **Integration with Effects (Week 101)**
   - Create undo points for all effects
   - Undo type classification
   - Memory optimization

3. **History Panel Integration (Week 102)**
   - pdHistory control integration
   - Undo/redo navigation
   - Thumbnail generation for history

**Exit Criteria:**
- Undo/redo functional for all operations
- Unlimited history working
- Memory usage reasonable
- History panel displays correctly

#### 5.2 Macro Recording System (Month 22-23, Weeks 101-106)

**Timeline:** 6 weeks
**Effort:** 12-16 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** Phase 3 Effects

**Tasks:**

1. **Macro Infrastructure (Week 101-102)**
   - Macro recording state machine
   - Action capture system
   - Parameter serialization
   - **Reference:** `/docs/algorithms/effects-catalog.md:456-578` (Macro system)

2. **XML Serialization (Week 103)**
   - PDM file format (XML)
   - Version 8.2014 compatibility
   - Backward compatibility with VB6 macros
   - **Reference:** `/docs/algorithms/effects-catalog.md:497-516` (XML schema)

3. **Macro Playback (Week 104)**
   - Sequential execution
   - Error handling
   - Progress reporting

4. **Validation & Testing (Week 105)**
   - Recordable action validation
   - VB6 macro file compatibility tests
   - Edge case testing

5. **UI Integration (Week 106)**
   - Record/Stop buttons
   - Macro playback UI
   - Recent macros MRU

**Exit Criteria:**
- Macro recording functional
- Macro playback functional
- VB6 macros load and play correctly
- All recordable effects captured
- XML serialization correct

#### 5.3 Batch Processing System (Month 23, Weeks 104-108)

**Timeline:** 5 weeks
**Effort:** 10-14 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** 5.2 Macro System

**Tasks:**

1. **Batch Wizard UI (Week 104-105)**
   - Source file selection
   - Operation selection
   - Output settings
   - Progress display
   - **Reference:** `/docs/algorithms/effects-catalog.md:585-651` (Batch processing)

2. **Batch Processor Engine (Week 106)**
   - Multi-file processing
   - Async/await pattern
   - Cancellation support
   - Error handling

3. **Batch Operations (Week 107)**
   - Resize/convert
   - Apply macro
   - Format conversion
   - Metadata operations

4. **Batch Repair Utility (Week 108)**
   - Fix corrupted images
   - Metadata repair
   - Format recovery

**Exit Criteria:**
- Batch wizard functional
- Can process 100+ images
- Progress reporting working
- Error handling robust
- Cancellation working

#### 5.4 Tool System (Month 23-24, Weeks 106-112)

**Timeline:** 7 weeks
**Effort:** 16-20 developer-weeks
**Owner:** 2x Developers
**Complexity:** 🔴 HIGH
**Dependencies:** Phase 2 Canvas

**Priority Order:**

1. **Tool Infrastructure (Week 106)**
   - Tool selection system
   - Tool cursor management
   - Tool panels container
   - **Reference:** `/docs/architecture/vb6-architecture.md:309-344` (Tool subsystem)

2. **Selection Tools (Week 107-108)**
   - Rectangle, Ellipse selection
   - Polygon, Lasso selection
   - Magic Wand (flood fill)
   - **Reference:** `/docs/architecture/vb6-architecture.md:289-308` (Selection)

3. **Basic Edit Tools (Week 109)**
   - Move tool
   - Zoom tool
   - Crop tool
   - **Reference:** `/docs/architecture/vb6-architecture.md:314-324` (Tools)

4. **Paint Tools (Week 110-111)**
   - Paintbrush
   - Pencil
   - Eraser
   - Fill/Bucket
   - Gradient tool
   - **Reference:** `/docs/architecture/vb6-architecture.md:314-324` (Paint tools)

5. **Advanced Tools (Week 112)**
   - Clone stamp
   - Text tool
   - Measure tool
   - Color picker

**Exit Criteria:**
- All major tools implemented
- Selection tools functional
- Paint tools working
- Tool panels integrated
- Cursor feedback correct

#### 5.5 Performance Optimization (Month 24, Weeks 109-114)

**Timeline:** 6 weeks (parallel with 5.4)
**Effort:** 12-18 developer-weeks
**Owner:** 2x Senior Developers
**Complexity:** 🔴 HIGH
**Dependencies:** All previous phases

**Tasks:**

1. **Performance Profiling (Week 109)**
   - Identify bottlenecks
   - Memory leak detection
   - CPU profiling
   - GPU utilization analysis

2. **Optimization - Effects (Week 110-111)**
   - SIMD optimization for critical effects
   - Parallel processing tuning
   - Memory pooling
   - Cache optimization
   - **Target:** All effects within 2x of VB6

3. **Optimization - Rendering (Week 112)**
   - Canvas rendering optimization
   - Layer compositing optimization
   - Viewport caching
   - **Target:** 60 FPS at all times

4. **Optimization - File I/O (Week 113)**
   - Streaming for large files
   - Async I/O optimization
   - Memory-mapped files
   - **Target:** Match or exceed VB6 load times

5. **Optimization - Memory (Week 114)**
   - Memory leak fixes
   - Large image handling
   - Memory suspension optimization
   - **Target:** Handle 100MP+ images

**Exit Criteria:**
- All critical effects within 2x of VB6
- Canvas maintains 60 FPS
- No memory leaks detected
- Can handle 100MP+ images
- File I/O performance acceptable

#### 5.6 Testing & Quality Assurance (Month 24, Weeks 112-116)

**Timeline:** 5 weeks (parallel with 5.5)
**Effort:** 10-15 developer-weeks
**Owner:** QA Team + Developers
**Complexity:** 🟡 MEDIUM
**Dependencies:** All previous phases

**Tasks:**

1. **Automated Testing (Week 112)**
   - Expand unit test coverage (target: 80%+)
   - Integration test suite
   - Visual regression tests
   - Performance benchmarks

2. **Platform Testing (Week 113)**
   - Windows testing (7, 10, 11)
   - macOS testing (10.15+)
   - Linux testing (Ubuntu, Fedora)
   - ARM64 testing (Apple Silicon, Windows ARM)

3. **Compatibility Testing (Week 114)**
   - VB6 file compatibility (1000+ test files)
   - VB6 macro compatibility
   - Format round-trip testing
   - Color accuracy verification

4. **User Acceptance Testing (Week 115)**
   - Beta tester recruitment
   - Feedback collection
   - Bug fixing
   - Documentation review

5. **Final Bug Fixing (Week 116)**
   - Critical bug fixes
   - Edge case handling
   - Polish and refinement

**Exit Criteria:**
- 80%+ unit test coverage
- All platforms tested
- All VB6 files load correctly
- No critical bugs
- Beta tester feedback positive

#### 5.7 Documentation & Release Prep (Month 24, Weeks 114-117)

**Timeline:** 4 weeks (parallel with 5.6)
**Effort:** 6-10 developer-weeks
**Owner:** Tech Writer + Developers
**Complexity:** 🟢 LOW

**Tasks:**

1. **User Documentation (Week 114-115)**
   - User manual
   - Tutorial videos
   - FAQ
   - Migration guide (VB6 to .NET)

2. **Developer Documentation (Week 115)**
   - API documentation
   - Architecture documentation
   - Plugin development guide
   - Contributing guide

3. **Release Notes (Week 116)**
   - Changelog
   - Known issues
   - System requirements
   - Installation guide

4. **Release Packaging (Week 117)**
   - Installer creation (Windows, macOS, Linux)
   - Code signing
   - Release testing
   - Distribution preparation

**Exit Criteria:**
- All documentation complete
- Installers tested on all platforms
- Release notes finalized
- Ready for v1.0 release

### Phase 5 Milestones & Exit Criteria

**Major Milestone: M5 - Release Candidate**

**Timeline:** End of Month 24
**Exit Criteria:**
- ✅ All features implemented
- ✅ Undo/redo functional
- ✅ Macro recording/playback working
- ✅ Batch processing functional
- ✅ All tools implemented
- ✅ Performance targets met
- ✅ All tests passing
- ✅ Documentation complete
- ✅ All platforms tested
- ✅ Ready for v1.0 release

**Deliverables for Stakeholder Review:**
- Release Candidate build
- Complete feature comparison (VB6 vs .NET)
- Performance report (final benchmarks)
- Test results (all platforms)
- User documentation
- Marketing materials

---

## Critical Dependencies & Sequencing

### Dependency Graph

```
Phase 0: Foundation & Planning
    ↓
Phase 1: Core Infrastructure
    ├── 1.1 Type System ────────────────────────┐
    ├── 1.2 Core Data Model (depends on 1.1) ───┼──┐
    ├── 1.3 Rendering (depends on 1.2) ─────────┼──┼──┐
    ├── 1.4 File I/O (depends on 1.2) ──────────┘  │  │
    ├── 1.5 Plugin Infrastructure (depends on 1.4) │  │
    └── 1.6 Utilities (parallel) ───────────────────┘  │
         ↓                                              │
Phase 2: UI Framework                                  │
    ├── 2.1 Theme System ───────────────────────┐      │
    ├── 2.2 Basic Controls (depends on 2.1) ────┼──┐   │
    ├── 2.3 Input Controls (depends on 2.2) ────┼──┼──┐│
    ├── 2.4 Display Controls (depends on 2.3) ──┼──┼──││
    ├── 2.5 Command Controls (depends on 2.2,3) ┼──┼──││
    ├── 2.6 Canvas System (depends on 1.3, 2.2) ┘  │  ││
    ├── 2.7 Layer/History (depends on 1.2, 2.2) ───┘  ││
    └── 2.8 Main Windows (depends on ALL 2.x) ────────┘│
         ↓                                              │
Phase 3: Effects & Algorithms                          │
    ├── 3.1 Effect Infrastructure (depends on 2.4,5) ──┘
    ├── 3.2 Core Adjustments (depends on 3.1) ──┐
    ├── 3.3 Performance Effects (depends on 3.1) ┼──┐
    ├── 3.4 Transforms (depends on 3.1) ─────────┼──┼─┐
    ├── 3.5 Artistic (depends on 3.1, 3.3) ──────┼──┼─┤
    ├── 3.6 Distortions (depends on 3.1) ────────┼──┼─┤
    ├── 3.7 Remaining Categories (depends on 3.1)┘  │ │
    └── 3.8 Advanced Noise (optional) ───────────────┘ │
         ↓                                              │
Phase 4: Format Support                                │
    ├── 4.1 Modern Formats (depends on 1.4) ──────────┐│
    ├── 4.2 PSD Parser (depends on 1.4) ──────────────┼┤
    ├── 4.3 PSP Parser (depends on 1.4) ──────────────┼┤
    ├── 4.4 XCF Parser (depends on 1.4) ──────────────┼┤
    ├── 4.5 Other Parsers (depends on 1.4) ───────────┼┤
    └── 4.6 Specialized Formats (depends on 1.5) ─────┘│
         ↓                                              │
Phase 5: Integration & Polish                          │
    ├── 5.1 Undo/Redo (depends on 1.2) ───────────────┐│
    ├── 5.2 Macro System (depends on 3.x) ────────────┼┤
    ├── 5.3 Batch Processing (depends on 5.2) ────────┼┤
    ├── 5.4 Tool System (depends on 2.6) ─────────────┼┤
    ├── 5.5 Performance Optimization (depends on ALL) ┼┤
    ├── 5.6 Testing (depends on ALL) ─────────────────┼┤
    └── 5.7 Documentation (parallel) ─────────────────┘│
         ↓                                              ↓
    v1.0 Release                                 All Complete
```

### Parallel Work Opportunities

To maximize team efficiency, the following work streams can proceed in parallel:

#### Phase 1 (Months 3-6)
- Stream A: Type System → Core Data Model → Rendering (Senior Dev 1)
- Stream B: File I/O → Plugin Infrastructure (Senior Dev 2)
- Stream C: Utilities (Junior Dev)

#### Phase 2 (Months 7-13)
- Stream A: Basic Controls → Input Controls (UI Dev 1)
- Stream B: Theme System → Display Controls (UI Dev 2)
- Stream C: Canvas System (Graphics Dev 1+2)
- Stream D: Layer/History Panels (UI Dev 3+4)
- Stream E: Command Controls (UI Dev 5)
- Stream F: Main Windows (after dependencies, UI Dev 1+2)

#### Phase 3 (Months 14-18)
- Stream A: Effect Infrastructure → Core Adjustments (Algo Dev 1)
- Stream B: Performance Effects (Gaussian blur, etc.) (Algo Dev 2+3)
- Stream C: Transforms (Algo Dev 4)
- Stream D: Artistic & Stylize (Algo Dev 5+6)
- Stream E: Distortions (Algo Dev 7+8)
- Stream F: Remaining categories (Algo Dev 9+10)

#### Phase 4 (Months 19-22)
- Stream A: Modern Formats (Format Dev 1)
- Stream B: PSD Parser (Format Dev 2+3)
- Stream C: PSP Parser (Format Dev 4+5)
- Stream D: XCF Parser (Format Dev 6+7)
- Stream E: Other Parsers (Format Dev 8)
- Stream F: Specialized Formats (Format Dev 9)

#### Phase 5 (Months 22-24)
- Stream A: Undo/Redo (Dev 1)
- Stream B: Macro System (Dev 2+3)
- Stream C: Batch Processing (Dev 4+5)
- Stream D: Tool System (Dev 6+7)
- Stream E: Performance Optimization (Senior Dev 8+9)
- Stream F: Testing (QA Team)
- Stream G: Documentation (Tech Writer)

---

## Resource Planning

### Team Composition

**Recommended Team Size:** 7-10 developers + 2-3 QA + 1 Tech Writer + 1 PM

#### Core Team (Minimum)

| Role | Count | Phases | Key Responsibilities |
|------|-------|--------|---------------------|
| **Technical Lead** | 1 | 0-5 | Architecture, code review, critical path |
| **Senior Developers** | 3 | 1-5 | Core infrastructure, graphics, algorithms |
| **Mid-Level Developers** | 4 | 2-5 | UI controls, effects, formats |
| **Junior Developers** | 2 | 1-5 | Utilities, testing, support |
| **QA Engineers** | 2 | 3-5 | Testing, automation, regression |
| **Tech Writer** | 1 | 5 | Documentation |
| **Project Manager** | 1 | 0-5 | Planning, coordination, reporting |

#### Expanded Team (Optimal)

Additional resources for faster delivery:

| Role | Additional | Purpose |
|------|-----------|---------|
| UI/UX Developers | +2 | Accelerate Phase 2 |
| Algorithm Developers | +2 | Accelerate Phase 3 |
| Format Specialists | +2 | Accelerate Phase 4 |
| QA Engineers | +1 | More thorough testing |
| DevOps Engineer | +1 | CI/CD, automation |

### Skill Requirements

#### Must-Have Skills
- C# / .NET 10
- MAUI framework
- SkiaSharp or graphics programming
- Image processing algorithms
- Binary file format parsing
- Performance optimization
- Unit testing

#### Nice-to-Have Skills
- VB6 (for understanding legacy code)
- ImageSharp / Magick.NET
- Color science
- SIMD programming
- GPU programming (OpenCL, CUDA)
- Cross-platform development

### Training Needs

**Weeks 1-2 (Phase 0):**
- MAUI framework training (2 days)
- SkiaSharp training (2 days)
- PhotoDemon architecture review (3 days)
- Code review and standards (1 day)

---

## Risk Management

### High-Risk Items

| Risk | Probability | Impact | Mitigation Strategy |
|------|------------|--------|---------------------|
| **GDI+ to SkiaSharp incompatibilities** | Medium | High | Early PoC (Phase 0), extensive testing, fallback options |
| **Performance regression** | High | High | Continuous benchmarking, optimization sprints, SIMD/GPU |
| **Custom parser bugs** | Medium | High | Reference test suites, VB6 comparison, round-trip tests |
| **Control rendering differences** | Medium | Medium | Visual regression tests, pixel-perfect comparisons |
| **File format incompatibility** | Low | High | Extensive test file library, backward compatibility tests |
| **Team skill gaps** | Low | Medium | Training program, knowledge sharing, code reviews |
| **Scope creep** | Medium | Medium | Strict phase gates, feature freeze after Phase 4 |
| **Platform-specific bugs** | Medium | Medium | Test early on all platforms, platform-specific code isolated |
| **Memory leaks** | Medium | High | Memory profiling, automated leak detection, code review |
| **Third-party dependency issues** | Low | Medium | License compatibility checks, fallback libraries, version pinning |

### Mitigation Strategies

#### 1. Performance Risk
- **Continuous Benchmarking:** Automated benchmarks run on every commit
- **Early Optimization:** Profile and optimize critical paths early
- **Fallback Plans:** If .NET can't match VB6, consider native libraries

#### 2. Quality Risk
- **Test-Driven Development:** Write tests before/with code
- **Code Reviews:** All code reviewed by senior developer
- **Automated Testing:** CI/CD runs full test suite on every commit
- **Beta Program:** Early user feedback

#### 3. Schedule Risk
- **Buffer Time:** 20% buffer built into each phase
- **Early Warning:** Weekly progress reviews, monthly milestones
- **Parallel Work:** Maximize parallel streams to absorb delays
- **Scope Flexibility:** Mark features as optional/deferred if needed

#### 4. Technical Risk
- **Proof-of-Concepts:** Validate all high-risk components in Phase 0
- **Architectural Reviews:** Monthly architecture review meetings
- **External Expertise:** Consultants for specialized areas (color management, etc.)

---

## Success Metrics

### Phase Completion Metrics

Each phase has clear exit criteria (listed in phase sections). Summary:

| Phase | Key Metric | Target |
|-------|-----------|--------|
| Phase 0 | All PoCs successful | 100% validation |
| Phase 1 | Core infrastructure tests passing | 80%+ coverage |
| Phase 2 | All 56 controls implemented | 100% complete |
| Phase 3 | All 117+ effects implemented | 100% complete |
| Phase 4 | All formats supported | 40+ import, 25+ export |
| Phase 5 | Performance targets met | Within 2x of VB6 |

### Quality Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| **Code Coverage** | 80%+ | Unit tests |
| **Performance** | Within 2x of VB6 for most operations | Automated benchmarks |
| **Performance (Critical)** | Within 1.5x for Gaussian blur, rendering | Manual benchmarks |
| **Memory Leaks** | Zero | Automated leak detection |
| **File Compatibility** | 100% of VB6 files | Test suite (1000+ files) |
| **Macro Compatibility** | 95%+ of VB6 macros | Test suite |
| **Visual Quality** | Pixel-perfect or better | Visual regression tests |
| **Crash Rate** | < 0.1% (1 crash per 1000 operations) | Telemetry |

### Platform Support Metrics

| Platform | Target | Notes |
|----------|--------|-------|
| **Windows 10/11** | Full support | Primary platform |
| **macOS 11+** | Full support | Apple Silicon + Intel |
| **Linux** | Best-effort | Ubuntu 20.04+, Fedora 35+ |
| **iOS/Android** | Future | Not in v1.0 scope |

---

## Deliverables Summary

### Phase 0 Deliverables
- Development environment setup
- 4 proof-of-concept implementations
- Architecture design document
- Detailed project plan

### Phase 1 Deliverables
- Type mapping library (176+ types)
- Core data model (Image, Layer, Bitmap)
- Rendering engine (SkiaSharp-based)
- File I/O foundation (PDI + common formats)
- Plugin infrastructure
- 80%+ test coverage

### Phase 2 Deliverables
- 56 custom MAUI controls
- Theme system (Dark/Light)
- Canvas system (zoom/pan/render)
- Layer panel
- History panel
- Main application window
- All UI tests passing

### Phase 3 Deliverables
- 117+ effects implemented
- Effect dialog system
- Effect preview system
- Parameter serialization
- Performance within 2x of VB6

### Phase 4 Deliverables
- 40+ import formats
- 25+ export formats
- PSD, PSP, XCF parsers ported
- Modern format support (WebP, HEIF, AVIF, JXL)
- 100% file compatibility with VB6

### Phase 5 Deliverables
- Undo/redo system
- Macro recording/playback
- Batch processing
- All tools implemented
- Performance optimized
- All platforms tested
- Complete documentation
- Release Candidate build

---

## Acceptance Criteria

### v1.0 Release Criteria

The following must be TRUE before v1.0 release:

#### Functional Requirements
- [ ] Can load and save all formats supported in VB6 version
- [ ] All 117+ effects functional
- [ ] All 56 custom controls working
- [ ] Canvas displays images correctly with zoom/pan
- [ ] Layer system functional (add/delete/reorder/blend)
- [ ] Undo/redo unlimited history working
- [ ] Macro recording and playback functional
- [ ] Batch processing working (100+ images)
- [ ] All major tools implemented (selection, paint, transform)
- [ ] Color management working (ICC profiles)
- [ ] Metadata reading/writing functional

#### Performance Requirements
- [ ] Gaussian blur within 1.5x of VB6 speed
- [ ] All effects within 2x of VB6 speed (90%+)
- [ ] Canvas maintains 60 FPS during pan/zoom
- [ ] File load times within 2x of VB6
- [ ] File save times within 2x of VB6
- [ ] Can handle 100MP+ images
- [ ] UI remains responsive during operations

#### Quality Requirements
- [ ] Zero critical bugs
- [ ] < 10 known minor bugs
- [ ] 80%+ unit test coverage
- [ ] All platform tests passing
- [ ] 1000+ VB6 test files load correctly
- [ ] No memory leaks detected
- [ ] No crashes in normal operation
- [ ] Accessibility compliance (WCAG 2.1 Level AA)

#### Documentation Requirements
- [ ] User manual complete
- [ ] API documentation complete
- [ ] Migration guide complete
- [ ] Installation guide complete
- [ ] Release notes complete

#### Platform Requirements
- [ ] Windows 10/11 fully supported
- [ ] macOS 11+ fully supported
- [ ] Linux (Ubuntu/Fedora) best-effort support
- [ ] All installers tested and signed

---

## Contingency Plans

### If Behind Schedule

**Trigger:** > 2 weeks behind at any milestone

**Actions:**
1. Increase team size (+2-3 developers)
2. Reduce scope (defer optional features)
3. Extend timeline (add 1-2 months)
4. Increase parallel work streams

**Optional Features (Can Defer):**
- Advanced noise algorithms (Anisotropic Diffusion, Mean Shift)
- Some legacy format support (XBM, WBMP, MBM)
- Advanced tool features (clone stamp, some paint tools)
- Photoshop plugin hosting
- Linux support (defer to v1.1)

### If Performance Targets Not Met

**Trigger:** Effects > 3x slower than VB6

**Actions:**
1. Dedicated optimization sprint (2-4 weeks)
2. Bring in performance specialist
3. Investigate native library wrappers
4. Consider GPU acceleration
5. Profile and optimize critical paths
6. SIMD optimization
7. If needed: hybrid VB6/C++ DLL approach for critical algorithms

### If Blocking Technical Issue

**Trigger:** No viable path forward on critical component

**Actions:**
1. Escalate to technical lead immediately
2. Research alternative approaches
3. Consult external experts
4. Consider pivoting technology (e.g., different graphics library)
5. If truly blocked: re-scope project

---

## Post-v1.0 Roadmap

### v1.1 (3-6 months after v1.0)

**Focus:** Refinement and optimization

- Performance optimization based on telemetry
- Bug fixes from user feedback
- Linux platform improvements
- Additional format support (if needed)
- Plugin API improvements
- Accessibility improvements

### v1.2 (6-12 months after v1.0)

**Focus:** New features and platform expansion

- Photoshop plugin support
- Advanced RAW processing
- GPU acceleration for effects
- Mobile platform exploration (iOS/Android)
- Cloud integration
- Scripting API (Python/JavaScript)

### v2.0 (12-18 months after v1.0)

**Focus:** Major new capabilities

- AI/ML-powered features
- Vector editing capabilities
- Advanced color grading
- Video frame editing
- Collaboration features
- Professional workflow improvements

---

## Conclusion

This roadmap provides a comprehensive, phased approach to migrating PhotoDemon from VB6 to .NET 10 MAUI. The migration is **ambitious but achievable** with:

1. **Proper team composition** (7-10 developers)
2. **Disciplined execution** (following phase gates)
3. **Risk management** (early PoCs, continuous testing)
4. **Quality focus** (80%+ test coverage, performance benchmarks)
5. **Realistic timeline** (18-24 months)

### Critical Success Factors

✅ **Preserve competitive advantages** (PSD/PSP/XCF parsers, color management)
✅ **Maintain quality** (Apple test suite, color accuracy)
✅ **Achieve performance parity** (within 2x of VB6)
✅ **Cross-platform compatibility** (Windows, macOS, Linux)
✅ **Backward compatibility** (all VB6 files and macros)

### Next Steps

1. **Review and approve this roadmap** with stakeholders
2. **Assemble core team** (Weeks 1-2)
3. **Begin Phase 0** (Foundation & Planning)
4. **Execute PoCs** to validate technology choices
5. **Proceed to Phase 1** upon successful Phase 0 completion

---

**Document Prepared By:** Agent 8 - Migration Roadmap Agent
**Date:** 2025-11-17
**Status:** Final Draft - Ready for Stakeholder Review

**Approvals Required:**
- [ ] Technical Lead
- [ ] Project Manager
- [ ] Product Owner
- [ ] Executive Sponsor
