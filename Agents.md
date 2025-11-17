# PhotoDemon Migration Documentation Plan

## Project Overview
**Migration Target**: VB6 → .NET 10 with .NET MAUI
**Objective**: Modernize PhotoDemon photo editor while maintaining feature parity and cross-platform support

### Scope
This migration is a substantial undertaking involving:
- **634+ files** (143 classes, 261 forms, 114 controls, 116 modules)
- **200+ image editing tools** and effects
- Complex file format support (PSD, RAW, XCF, PSP, etc.)
- Win32 API dependencies requiring migration
- 32-bit → 64-bit architecture changes

### Migration Goals
- Modern C# codebase with .NET 10
- Cross-platform support (Windows, macOS, Linux) via MAUI
- Maintain all 200+ features from VB6 version
- Improve performance with 64-bit architecture
- Preserve portable, no-install philosophy

### Target Architecture
- MVVM pattern
- Dependency injection
- Service-oriented design
- Plugin architecture for extensibility

---

## Working with Claude Code

### Running Agent Tasks
Each agent task can be executed using Claude Code on the web:

1. Reference the specific agent task (e.g., "Agent 1: Architecture Mapping")
2. Claude will analyze the relevant codebase sections
3. Documentation will be generated in the appropriate `docs/` subdirectory

### Best Practices
- Run agents sequentially within each phase
- Review and validate output before proceeding to next agent
- Update documentation as implementation progresses
- Use code references (`file:line`) for traceability
- Each agent runs independently but builds on previous work

---

## Phase 1: Codebase Analysis & Documentation

### Agent Tasks

#### 1. Architecture Mapping Agent
**Objective**: Document current VB6 architecture

- Analyze and document all modules in `/Modules` (116+ files)
- Document all classes in `/Classes` (143+ files)
- Document all forms in `/Forms` (261+ files)
- Document all controls in `/Controls` (114+ files)
- Create dependency graph showing module relationships
- Identify core subsystems (rendering, file I/O, effects, UI, etc.)

**Deliverable**: `docs/architecture/vb6-architecture.md`

---

#### 2. API Inventory Agent
**Objective**: Catalog all Win32 API calls and external dependencies

- List all Declare statements (Win32 API calls)
- Document all ActiveX/COM dependencies
- Identify all external DLLs in `/App/PhotoDemon/Plugins`
- Map to .NET equivalents or required P/Invoke signatures
- Flag APIs with no direct .NET equivalent

**Deliverable**: `docs/api/api-inventory.md`

---

#### 3. Dependency Analysis & .NET Alternatives Agent
**Objective**: Analyze all dependencies and research suitable .NET alternatives

- Analyze all third-party libraries and plugins
- Document current dependency purposes and functionality
- Research .NET 10 compatible alternatives:
  - Image processing libraries (ImageSharp, SkiaSharp, Magick.NET)
  - Graphics libraries for MAUI
  - Compression libraries (SharpZipLib, etc.)
  - Codec libraries for PSD/RAW/XCF formats
- Evaluate NuGet packages for feature parity
- Document licensing compatibility
- Assess performance characteristics of alternatives
- Create dependency migration matrix (VB6 lib → .NET package)
- Identify gaps requiring custom implementation

**Deliverable**: `docs/dependencies/dotnet-alternatives.md`

---

#### 4. UI Component Mapping Agent
**Objective**: Map VB6 controls to MAUI controls

- Document all custom controls (114+ files in `/Controls`)
- Map VB6 controls → MAUI equivalents
- Identify controls requiring custom implementation
- Document event handling patterns
- Catalog property/method translations

**Deliverable**: `docs/ui/control-mapping.md`

---

#### 5. Data Structure Agent
**Objective**: Document data types and structures

- Document all Type declarations
- Document all Class structures
- Map VB6 types → C# types
- Identify memory layout concerns (32-bit → 64-bit)
- Document serialization requirements (file formats, settings)

**Deliverable**: `docs/data/type-mapping.md`

---

#### 6. File Format Agent
**Objective**: Document file I/O and format support

- Document PSD support implementation
- Document RAW format handling
- Document XCF, PSP format support
- Identify all codec dependencies
- Map to .NET libraries (ImageSharp, SkiaSharp, etc.)

**Deliverable**: `docs/formats/file-format-support.md`

---

#### 7. Effects & Algorithms Agent
**Objective**: Document image processing algorithms

- Catalog all 200+ tools and effects
- Document algorithm implementations
- Identify performance-critical sections
- Document macro recording system
- Document batch processing system

**Deliverable**: `docs/algorithms/effects-catalog.md`

---

## Phase 2: Migration Strategy

### Agent Tasks

#### 8. Migration Roadmap Agent
**Objective**: Create phased migration plan

- Define migration phases by subsystem
- Identify critical path components
- Create milestone definitions
- Estimate complexity per component
- Define testing strategy

**Deliverable**: `docs/migration/roadmap.md`

---

#### 9. Risk Assessment Agent
**Objective**: Identify migration risks

- Identify high-risk components
- Document breaking changes (32→64 bit, portable nature)
- Assess performance implications
- Identify UI/UX changes needed for cross-platform
- Document backward compatibility requirements

**Deliverable**: `docs/migration/risk-assessment.md`

---

#### 10. MAUI Architecture Agent
**Objective**: Design .NET 10 MAUI architecture

- Design MVVM architecture
- Design dependency injection structure
- Design service layer
- Design plugin architecture
- Design cross-platform abstraction layer

**Deliverable**: `docs/architecture/maui-architecture.md`

---

## Phase 3: Implementation Guides

### Agent Tasks

#### 11. Conversion Guide Agent
**Objective**: Create conversion reference guides

- VB6 → C# syntax guide
- Common pattern translations
- Error handling migration
- Resource management (GDI → MAUI graphics)
- Threading model changes

**Deliverable**: `docs/guides/conversion-guide.md`

---

#### 12. Testing Strategy Agent
**Objective**: Define testing approach

- Unit test strategy
- Integration test strategy
- UI test approach
- Performance benchmarking plan
- Regression test requirements

**Deliverable**: `docs/testing/testing-strategy.md`

---

## Documentation Structure

```
docs/
├── architecture/
│   ├── vb6-architecture.md
│   └── maui-architecture.md
├── api/
│   └── api-inventory.md
├── dependencies/
│   └── dotnet-alternatives.md
├── ui/
│   └── control-mapping.md
├── data/
│   └── type-mapping.md
├── formats/
│   └── file-format-support.md
├── algorithms/
│   └── effects-catalog.md
├── migration/
│   ├── roadmap.md
│   └── risk-assessment.md
├── guides/
│   └── conversion-guide.md
└── testing/
    └── testing-strategy.md
```

---

## Execution Notes

- Each agent task can be run independently using Claude Code
- Agents should be run sequentially within each phase
- Documentation should be updated as implementation progresses
- All markdown files should include code references with `file:line` format
- Cross-reference related documentation sections

---

## Success Criteria

- Complete architectural understanding documented
- All dependencies mapped to .NET equivalents
- Clear migration roadmap with milestones
- Risk mitigation strategies defined
- Implementation guides ready for development team
