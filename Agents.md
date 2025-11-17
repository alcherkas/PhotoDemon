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

### Reference Documentation
- **[MAUI Best Practices](./MAUI-BestPractices.md)** - Comprehensive guide for .NET MAUI architecture, performance optimization, and PhotoDemon-specific considerations

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
  - Image processing libraries (SkiaSharp, Magick.NET, ImageSharp)
  - Graphics libraries for MAUI
  - Compression libraries (SharpZipLib, etc.)
  - Codec libraries for PSD/RAW/XCF formats
- Evaluate NuGet packages for feature parity
- **Document licensing compatibility** (critical: PhotoDemon is BSD-licensed, avoid libraries with commercial restrictions like ImageSharp)
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
- Follow best practices from [MAUI-BestPractices.md](./MAUI-BestPractices.md)

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

#### 13. Form/Control Migration Priority Agent
**Objective**: Create prioritized migration sequence for 261 forms and 114 controls

- Analyze form-to-control dependencies
- Analyze control-to-control dependencies
- Create comprehensive dependency tree/graph
- Identify shared/reusable controls (migrate first)
- Identify standalone vs. dependent forms
- Categorize by complexity:
  - Simple forms (basic UI, minimal logic)
  - Medium forms (moderate controls, some business logic)
  - Complex forms (heavy processing, custom controls, complex state)
- Prioritize by business value:
  - Core features (file open/save, basic editing)
  - Secondary features (effects, filters)
  - Advanced features (macros, batch processing)
  - Administrative/settings UI
- Generate migration sequence with phases:
  - Phase 1: Shared controls and simple utility forms
  - Phase 2: Core application forms
  - Phase 3: Feature-specific forms
  - Phase 4: Complex/specialized forms
- Estimate effort per form/control
- Identify blocking dependencies
- Create migration checklist with order

**Deliverable**: `docs/migration/form-control-priority.md`

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
│   ├── risk-assessment.md
│   └── form-control-priority.md
├── guides/
│   └── conversion-guide.md
└── testing/
    └── testing-strategy.md
```

---

## Phase 4: Implementation Execution

After completing Phases 1-3 (analysis and planning), use Claude Code to perform the actual migration work.

### Prerequisites
- All Phase 1-3 agents completed
- Documentation generated in `docs/` folder
- Migration priority order defined (Agent 13)
- .NET MAUI project structure created

### Implementation Workflow

#### Step 1: Project Setup
Create the .NET MAUI project structure following `docs/architecture/maui-architecture.md`:

```
Prompt example:
"Create a new .NET MAUI project structure for PhotoDemon following the
architecture defined in docs/architecture/maui-architecture.md. Set up:
- MVVM folder structure
- Dependency injection in MauiProgram.cs
- Base classes and interfaces
- Follow best practices from MAUI-BestPractices.md"
```

#### Step 2: Migrate Shared Controls (Priority Phase 1)
Start with shared/reusable controls as defined in `docs/migration/form-control-priority.md`:

```
Prompt example:
"Migrate the VB6 control [ControlName] from Controls/[ControlName].ctl to .NET MAUI.

Context:
- VB6 source: Controls/[ControlName].ctl
- Control mapping: docs/ui/control-mapping.md
- MAUI best practices: MAUI-BestPractices.md
- Target architecture: docs/architecture/maui-architecture.md

Create:
1. MAUI custom control implementation
2. Bindable properties
3. Platform-specific renderers if needed
4. Usage example"
```

#### Step 3: Migrate Forms by Priority
Follow the migration sequence from `docs/migration/form-control-priority.md`:

```
Prompt example:
"Migrate VB6 Form [FormName] to .NET MAUI following the priority order.

References:
- VB6 source: Forms/[FormName].frm
- VB6 architecture: docs/architecture/vb6-architecture.md
- Control mappings: docs/ui/control-mapping.md
- Data type mappings: docs/data/type-mapping.md
- API mappings: docs/api/api-inventory.md
- Dependency alternatives: docs/dependencies/dotnet-alternatives.md
- Best practices: MAUI-BestPractices.md

Create:
1. XAML Page (View)
2. ViewModel with Community Toolkit.MVVM
3. Required services (register in DI)
4. Navigation setup
5. Unit tests for ViewModel

Follow MVVM pattern and ensure trim-safe code (use IQueryAttributable, not QueryProperty)."
```

#### Step 4: Migrate Business Logic
Migrate modules and classes referenced by the forms:

```
Prompt example:
"Migrate VB6 Module [ModuleName] to .NET MAUI service.

References:
- VB6 source: Modules/[ModuleName].bas
- Type mappings: docs/data/type-mapping.md
- API equivalents: docs/api/api-inventory.md
- Conversion guide: docs/guides/conversion-guide.md

Create:
1. Service interface
2. Service implementation
3. Register in MauiProgram.cs
4. Unit tests"
```

#### Step 5: Migrate Effects and Algorithms
Migrate the 200+ image processing tools:

```
Prompt example:
"Migrate image effect [EffectName] from VB6 to .NET MAUI.

References:
- VB6 source: [path to effect code]
- Algorithm documentation: docs/algorithms/effects-catalog.md
- Dependency alternatives: docs/dependencies/dotnet-alternatives.md (use SkiaSharp)
- Best practices: MAUI-BestPractices.md

Create:
1. Effect service implementing standard interface
2. Async implementation for UI responsiveness
3. Progress reporting
4. Macro recording support
5. Unit tests with sample images"
```

#### Step 6: Implement File Format Support
Migrate file I/O handlers:

```
Prompt example:
"Implement [Format] file format support for .NET MAUI PhotoDemon.

References:
- VB6 implementation: [path]
- Format documentation: docs/formats/file-format-support.md
- Dependency alternatives: docs/dependencies/dotnet-alternatives.md
- Best practices: MAUI-BestPractices.md

Use SkiaSharp or Magick.NET (check licensing).
Implement streaming for large files.
Create unit tests with sample files."
```

### Incremental Migration Best Practices

**1. One Component at a Time**
- Migrate one form/control per session
- Test thoroughly before moving to next
- Commit each completed component

**2. Reference Documentation Consistently**
- Always reference relevant docs in prompts
- Keep documentation updated as you discover edge cases
- Document deviations from original behavior

**3. Maintain Parallel Testing**
- Keep VB6 version for comparison
- Test equivalent functionality
- Document behavior differences

**4. Use Git Branches**
```bash
git checkout -b migrate/form-main-window
# Migrate component
git commit -m "Migrate main window form to MAUI"
git checkout main
git merge migrate/form-main-window
```

**5. Progressive Integration**
- Start with isolated components
- Integrate incrementally
- Run integration tests after each merge

### Example Session Flow

**Session 1: Migrate Color Picker Control**
1. Read `docs/migration/form-control-priority.md` (identifies this as Phase 1 priority)
2. Read `Controls/ColorPicker.ctl` (VB6 source)
3. Read `docs/ui/control-mapping.md` (MAUI equivalent controls)
4. Prompt Claude Code to create MAUI implementation
5. Review, test, commit

**Session 2: Migrate Main Window Form**
1. Confirm dependencies migrated (check priority doc)
2. Read `Forms/MainWindow.frm` and all referenced files
3. Reference architecture, mappings, and best practices docs
4. Prompt Claude Code to create Page + ViewModel + Services
5. Review, test navigation, commit

**Session 3: Migrate Image Processing Effect**
1. Read effect source code
2. Reference `docs/algorithms/effects-catalog.md`
3. Reference `docs/dependencies/dotnet-alternatives.md` for SkiaSharp usage
4. Prompt Claude Code to create effect service
5. Test with sample images, commit

### Monitoring Progress

Create a tracking file: `MIGRATION-PROGRESS.md`

```markdown
# Migration Progress

## Phase 1: Shared Controls (20 controls)
- [x] ColorPicker (Session 1, Commit abc123)
- [x] LayerList (Session 2, Commit def456)
- [ ] ToolOptions
- [ ] ...

## Phase 2: Core Forms (50 forms)
- [x] MainWindow (Session 10, Commit ghi789)
- [ ] ImageCanvas
- [ ] ...

## Phase 3: Feature Forms (120 forms)
- [ ] ...

## Phase 4: Complex Forms (71 forms)
- [ ] ...
```

### Parallel Execution Strategy

Since Claude Code can run multiple tasks in parallel, you can accelerate migration:

```
"Migrate the following 3 independent controls in parallel:
1. ColorPicker (Controls/ColorPicker.ctl)
2. FontSelector (Controls/FontSelector.ctl)
3. ZoomControl (Controls/ZoomControl.ctl)

Reference docs/ui/control-mapping.md and MAUI-BestPractices.md for all three.
These controls have no dependencies on each other per docs/migration/form-control-priority.md."
```

**Important:** Only parallelize truly independent components.

### Quality Gates

Before marking a component as "migrated":
- [ ] Code compiles without warnings
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)
- [ ] Functionality matches VB6 version (or document intentional changes)
- [ ] Follows MAUI best practices
- [ ] Code is trim-safe (no QueryProperty, proper DI usage)
- [ ] Performance is acceptable
- [ ] Committed to version control

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
