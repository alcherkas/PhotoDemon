# PhotoDemon VB6 to .NET 10 MAUI - UI Control Mapping

**Document Version:** 1.0
**Date:** 2025-11-17
**Author:** Agent 4 - UI Component Mapping Agent
**Status:** Complete

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Control Inventory](#control-inventory)
3. [Standard VB6 Controls](#standard-vb6-controls)
4. [PhotoDemon Custom Controls](#photodemon-custom-controls)
5. [MAUI Control Mappings](#maui-control-mappings)
6. [Event Mapping](#event-mapping)
7. [Property Mapping](#property-mapping)
8. [Complex Controls Requiring Custom Implementation](#complex-controls-requiring-custom-implementation)
9. [Migration Strategy](#migration-strategy)
10. [Implementation Priorities](#implementation-priorities)

---

## Executive Summary

### Inventory Summary

- **Total Custom Controls:** 56 (.ctl files)
- **Most Used Control:** `pdSlider` (454 instances)
- **Standard VB6 Controls:** Minimal usage (primarily in support tools)
- **PhotoDemon Custom Controls in Forms:** 2,100+ instances across 200+ forms
- **Migration Complexity:** High - Most controls require custom implementation

### Key Findings

1. **Heavy Customization**: PhotoDemon uses almost exclusively custom-drawn controls
2. **Owner-Drawn Architecture**: All custom controls are owner-drawn with GDI+ rendering
3. **Unicode Support**: Custom controls built to overcome VB6's Unicode limitations
4. **High DPI Aware**: Custom controls implement high DPI scaling
5. **Theme System**: Integrated theming engine (pdThemeColors) throughout all controls
6. **Minimal Standard Controls**: Standard VB6 controls only appear in support utilities

### Migration Impact

- **High**: 90% of controls need full reimplementation
- **Medium**: 8% can use MAUI controls with heavy customization
- **Low**: 2% can use standard MAUI controls directly

---

## Control Inventory

### Custom Control Files (56 total)

Located in `/home/user/PhotoDemon/Controls/`

```
pdAccelerator.ctl          pdListBox.ctl               pdResize.ctl
pdBrushSelector.ctl        pdListBoxOD.ctl             pdRuler.ctl
pdButton.ctl               pdListBoxView.ctl           pdScrollBar.ctl
pdButtonStrip.ctl          pdListBoxViewOD.ctl         pdSearchBar.ctl
pdButtonStripVertical.ctl  pdMetadataExport.ctl        pdSlider.ctl
pdButtonToolbox.ctl        pdNavigator.ctl             pdSliderStandalone.ctl
pdCanvas.ctl               pdNavigatorInner.ctl        pdSpinner.ctl
pdCanvasView.ctl           pdNewOld.ctl                pdStatusBar.ctl
pdCheckBox.ctl             pdPaletteUI.ctl             pdStrip.ctl
pdColorDepth.ctl           pdPenSelector.ctl           pdTextBox.ctl
pdColorSelector.ctl        pdPictureBox.ctl            pdTitle.ctl
pdColorVariants.ctl        pdPictureBoxInteractive.ctl pdTreeviewOD.ctl
pdColorWheel.ctl           pdPreview.ctl               pdTreeviewViewOD.ctl
pdCommandBar.ctl           pdProgressBar.ctl
pdCommandBarMini.ctl       pdRadioButton.ctl
pdContainer.ctl            pdRandomizeUI.ctl
pdDownload.ctl
pdDropDown.ctl
pdDropDownFont.ctl
pdFxPreview.ctl
pdGradientSelector.ctl
pdHistory.ctl
pdHyperlink.ctl
pdImageStrip.ctl
pdLabel.ctl
pdLayerList.ctl
pdLayerListInner.ctl
```

### Usage Frequency (Top 20)

| Control | Usage Count | Primary Purpose |
|---------|-------------|-----------------|
| `pdSlider` | 454 | Value adjustment with text box |
| `pdLabel` | 330 | Unicode text display |
| `pdCheckBox` | 201 | Boolean options |
| `pdButtonStrip` | 169 | Multi-option selection |
| `pdCommandBar` | 161 | OK/Cancel/Preset/Reset toolbar |
| `pdContainer` | 137 | Layout container |
| `pdFxPreviewCtl` | 134 | Effect preview rendering |
| `pdDropDown` | 132 | Dropdown selection |
| `pdButtonToolbox` | 112 | Toolbar buttons |
| `pdButton` | 104 | Standard buttons |
| `pdSpinner` | 72 | Numeric input |
| `pdColorSelector` | 60 | Color picking |
| `pdTitle` | 55 | Section headers |
| `pdTextBox` | 41 | Unicode text input |
| `pdPictureBox` | 35 | Image display |
| `pdHyperlink` | 21 | Clickable links |
| `pdCommandBarMini` | 21 | Compact command bar |
| `pdListBox` | 20 | List display |
| `pdRadioButton` | 19 | Mutually exclusive options |
| `pdListBoxOD` | 10 | Owner-drawn list box |

---

## Standard VB6 Controls

### Minimal Usage

Standard VB6 controls are **rarely used** in the main PhotoDemon application. They appear primarily in support tools:

#### Found in Support Tools Only

**File:** `/home/user/PhotoDemon/Support/Update Patcher 2.0/frmPatch.frm`
- `VB.Timer` - Timer control
- `VB.TextBox` - Standard text box

**File:** `/home/user/PhotoDemon/Support/Project-wide search and replace/frmMain.frm`
- `VB.CommandButton` - Standard buttons
- `VB.CheckBox` - Standard checkboxes
- `VB.TextBox` - Standard text input
- `VB.Label` - Standard labels
- `VB.Line` - Line separator

**File:** `/home/user/PhotoDemon/Support/i18n-manager/frmGenerateI18N.frm`
- `VB.ListBox` - Standard list box (1 instance)
- `VB.CommandButton` - Standard command buttons
- `VB.CheckBox` - Standard checkboxes
- `VB.Label` - Standard labels

**File:** `/home/user/PhotoDemon/Forms/Toolbar_Debug.frm`
- `VB.Timer` - Debug timer (1 instance)

### Why Standard Controls Are Avoided

Per control source code comments (e.g., `/home/user/PhotoDemon/Controls/pdButton.ctl:38-43`):

> "PhotoDemon has some unique needs when it comes to user controls - needs that the intrinsic VB controls can't handle. These range from the obnoxious (lack of an 'autosize' property for anything but labels) to the critical (no Unicode support)."

**Limitations of VB6 Standard Controls:**
1. No Unicode support
2. No autosizing capabilities
3. No high DPI support
4. Limited theming capabilities
5. Poor visual consistency
6. No owner-draw capabilities

---

## PhotoDemon Custom Controls

### Architecture Overview

All PhotoDemon custom controls share common architectural patterns:

#### Common Features
1. **Owner-Drawn**: All controls use custom GDI+ rendering
2. **Unicode Support**: Full Unicode text support via API text boxes
3. **High DPI Aware**: Automatic DPI scaling
4. **Theming**: Integrated with `pdThemeColors` class
5. **Focus Management**: Custom focus events (`GotFocusAPI`, `LostFocusAPI`)
6. **Tab Support**: Custom tab targeting (`SetCustomTabTarget`)
7. **Support Class**: All use `pdUCSupport` class for common functionality

#### Base Control Structure

Every custom control follows this pattern:

```vb
' Common events
Public Event Click()
Public Event GotFocusAPI()
Public Event LostFocusAPI()
Public Event SetCustomTabTarget(ByVal shiftTabWasPressed As Boolean, ByRef newTargetHwnd As Long)

' Common support
Private WithEvents ucSupport As pdUCSupport

' Common theming
Private m_Colors As pdThemeColors
```

**Reference:** `/home/user/PhotoDemon/Controls/pdButton.ctl:61-68`

### Control Categories

#### 1. Input Controls

##### pdSlider
**Purpose:** Combined slider and numeric input
**File:** `/home/user/PhotoDemon/Controls/pdSlider.ctl`
**Usage:** 454 instances
**Components:**
- `pdSliderStandalone` (slider track)
- `pdSpinner` (numeric input)

**Key Features:**
- Dual input methods (slide + text)
- Integer or floating-point values (`SigDigits` property)
- Gradient rendering modes
- Self-captioning
- Auto-validation
- Locale-aware (decimal/comma handling)

**Events:**
- `Change()` - Fires on any value change
- `FinalChange()` - Fires only on MouseUp/KeyUp
- `ResetClick()` - Reset to default value

**Properties:**
- `Value`, `Min`, `Max`
- `SigDigits` (decimal places)
- `Caption`, `CaptionPadding`

**Reference:** `/home/user/PhotoDemon/Controls/pdSlider.ctl:59-70`

---

##### pdSpinner
**Purpose:** Numeric input with up/down buttons
**File:** `/home/user/PhotoDemon/Controls/pdSpinner.ctl`
**Usage:** 72 instances (plus 454 embedded in pdSlider)
**Key Features:**
- Numeric validation
- Min/Max bounds
- Significant digits support
- Locale-aware number formatting

---

##### pdTextBox
**Purpose:** Unicode text input
**File:** `/home/user/PhotoDemon/Controls/pdTextBox.ctl`
**Usage:** 41 instances
**Implementation:** Wrapper around Windows API Edit control (`pdEditBoxW`)
**Key Features:**
- Full Unicode support
- Custom border rendering
- Keyboard hooking for control keys
- Warning: Accelerators don't work while focused (due to keyboard hook)

**Events:**
- `Change()`
- `KeyPress()`, `KeyDown()`, `KeyUp()`
- `Resize()`

**Reference:** `/home/user/PhotoDemon/Controls/pdTextBox.ctl:45-58`

---

##### pdCheckBox
**Purpose:** Boolean checkbox
**File:** `/home/user/PhotoDemon/Controls/pdCheckBox.ctl`
**Usage:** 201 instances
**Key Features:**
- Owner-drawn
- `Value` property (True/False)
- `Caption` property
- Theme-aware rendering

---

##### pdRadioButton
**Purpose:** Radio button (mutually exclusive options)
**File:** `/home/user/PhotoDemon/Controls/pdRadioButton.ctl`
**Usage:** 19 instances
**Key Features:**
- Owner-drawn
- `Value` property
- Auto-grouping by container

---

##### pdColorSelector
**Purpose:** Color selection button
**File:** `/home/user/PhotoDemon/Controls/pdColorSelector.ctl`
**Usage:** 60 instances
**Key Features:**
- Displays selected color
- Opens color picker dialog on click
- Shows main window color for quick selection
- Supports color reset/randomize/presets

**Events:**
- `ColorChanged()`
- `NeedParentForm()` - For proper window ordering

**Reference:** `/home/user/PhotoDemon/Controls/pdColorSelector.ctl:56-64`

---

#### 2. Selection Controls

##### pdDropDown
**Purpose:** Dropdown list (no edit)
**File:** `/home/user/PhotoDemon/Controls/pdDropDown.ctl`
**Usage:** 132 instances
**Implementation:** Uses embedded `pdListBox` for popup
**Key Features:**
- Owner-drawn combo box
- No edit capability (by design)
- Dynamic popup window
- Subclasses parent to auto-close on window move

**Events:**
- `Click()` - Selection changed

**Reference:** `/home/user/PhotoDemon/Controls/pdDropDown.ctl:48-54`

---

##### pdDropDownFont
**Purpose:** Font selection dropdown
**File:** `/home/user/PhotoDemon/Controls/pdDropDownFont.ctl`
**Usage:** 2 instances
**Key Features:**
- Font enumeration
- Font preview rendering
- System font list

---

##### pdListBox
**Purpose:** Standard list box
**File:** `/home/user/PhotoDemon/Controls/pdListBox.ctl`
**Usage:** 20 instances
**Key Features:**
- Unicode support
- Owner-drawn
- Uses `pdListSupport` for data management

---

##### pdListBoxOD
**Purpose:** Owner-drawn list box
**File:** `/home/user/PhotoDemon/Controls/pdListBoxOD.ctl`
**Usage:** 10 instances
**Key Features:**
- Full custom rendering via events
- Allows arbitrary item heights

---

##### pdButtonStrip
**Purpose:** Horizontal button strip (like tab control)
**File:** `/home/user/PhotoDemon/Controls/pdButtonStrip.ctl`
**Usage:** 169 instances
**Key Features:**
- Multiple buttons in single control
- Auto-sizing based on captions
- Single selection mode
- Theme-aware

**Events:**
- `Click(ByVal buttonIndex As Long)`

**Common Usage Example:**
```vb
Begin PhotoDemon.pdButtonStrip btsModel
   Caption = "model"
End
' Handler:
Private Sub btsModel_Click(ByVal buttonIndex As Long)
    ' Handle selection
End Sub
```

**Reference:** `/home/user/PhotoDemon/Forms/Adjustments_BrightnessContrast.frm:27-36`

---

##### pdButtonStripVertical
**Purpose:** Vertical button strip
**File:** `/home/user/PhotoDemon/Controls/pdButtonStripVertical.ctl`
**Usage:** 5 instances

---

#### 3. Button Controls

##### pdButton
**Purpose:** Standard push button
**File:** `/home/user/PhotoDemon/Controls/pdButton.ctl`
**Usage:** 104 instances
**Key Features:**
- Owner-drawn
- Auto-captioning with overflow handling
- High DPI support
- Hand cursor on hover
- Optional image support (spritesheet: base/hover/disabled)
- Owner-drawn mode for custom rendering
- Drag/drop support (as of April 2024)

**Events:**
- `Click()`
- `DrawButton()` - Owner-drawn mode
- `CustomDragDrop()`, `CustomDragOver()`

**Render Modes:**
- `BRM_Normal` - Standard text button
- `BRM_OwnerDrawn` - Custom rendering via event

**Reference:** `/home/user/PhotoDemon/Controls/pdButton.ctl:61-78`

---

##### pdButtonToolbox
**Purpose:** Small toolbar buttons
**File:** `/home/user/PhotoDemon/Controls/pdButtonToolbox.ctl`
**Usage:** 112 instances
**Key Features:**
- Compact size
- `AutoToggle` property
- Icon support

---

#### 4. Display Controls

##### pdLabel
**Purpose:** Unicode text label
**File:** `/home/user/PhotoDemon/Controls/pdLabel.ctl`
**Usage:** 330 instances
**Key Features:**
- Unicode text
- Auto-sizing
- `FontSize` property
- `ForeColor` property
- `Caption` property
- Supports custom drag/drop

**Common Properties:**
```vb
Caption = "text"
FontSize = 18
ForeColor = 4210752
```

**Reference:** `/home/user/PhotoDemon/Forms/pdCanvas.ctl:41-56`

---

##### pdTitle
**Purpose:** Section header/title
**File:** `/home/user/PhotoDemon/Controls/pdTitle.ctl`
**Usage:** 55 instances
**Key Features:**
- Larger font
- Visual separator
- Collapsible sections (optional)

---

##### pdHyperlink
**Purpose:** Clickable hyperlink
**File:** `/home/user/PhotoDemon/Controls/pdHyperlink.ctl`
**Usage:** 21 instances
**Key Features:**
- Underlined text
- Hand cursor
- `RaiseClickEvent` property
- Can open URLs or raise events

**Common Usage:**
```vb
Begin PhotoDemon.pdHyperlink hypRecentFiles
   Alignment = 2
   Caption = "clear recent image list"
   RaiseClickEvent = -1  'True
End
```

**Reference:** `/home/user/PhotoDemon/Controls/pdCanvas.ctl:59-70`

---

##### pdPictureBox
**Purpose:** Image display
**File:** `/home/user/PhotoDemon/Controls/pdPictureBox.ctl`
**Usage:** 35 instances
**Key Features:**
- Owner-drawn
- No automatic painting (manual control required)

---

##### pdPictureBoxInteractive
**Purpose:** Interactive image display
**File:** `/home/user/PhotoDemon/Controls/pdPictureBoxInteractive.ctl`
**Usage:** 6 instances
**Key Features:**
- Mouse interaction support
- Point selection
- Used for effect parameter visualization

---

##### pdProgressBar
**Purpose:** Progress indicator
**File:** `/home/user/PhotoDemon/Controls/pdProgressBar.ctl`
**Usage:** 3 instances
**Key Features:**
- Custom rendering
- Percentage display
- Determinate/indeterminate modes

---

#### 5. Container Controls

##### pdContainer
**Purpose:** Layout container
**File:** `/home/user/PhotoDemon/Controls/pdContainer.ctl`
**Usage:** 137 instances
**Key Features:**
- Groups controls
- Supports visibility toggling
- Used for collapsible sections

**Common Usage:**
```vb
Begin PhotoDemon.pdContainer cntrPopOut
   Begin PhotoDemon.pdLabel lblTitle
   End
   Begin PhotoDemon.pdButton cmdAction
   End
End
```

**Reference:** `/home/user/PhotoDemon/Forms/Toolpanel_Measure.frm:33-107`

---

#### 6. Complex Specialized Controls

##### pdCommandBar
**Purpose:** Dialog command bar (OK/Cancel/Presets/Reset)
**File:** `/home/user/PhotoDemon/Controls/pdCommandBar.ctl`
**Usage:** 161 instances
**Key Features:**
- Standard dialog footer
- OK and Cancel buttons
- Preset management (save/load/delete)
- Randomize button
- Reset button
- Handles tool dialog lifecycle

**Events:**
- `OKClick()`
- `CancelClick()`
- `ResetClick()`

**Internal Components:**
- `cmdOK`, `cmdCancel` (pdButton)
- `cboPreset` (pdDropDown)
- `cmdAction()` array (pdButtonToolbox)

**Reference:** `/home/user/PhotoDemon/Controls/pdCommandBar.ctl:135-150`

---

##### pdCommandBarMini
**Purpose:** Compact command bar
**File:** `/home/user/PhotoDemon/Controls/pdCommandBarMini.ctl`
**Usage:** 21 instances
**Key Features:**
- Simplified version of pdCommandBar
- Used in smaller dialogs

---

##### pdFxPreviewCtl
**Purpose:** Effect preview control
**File:** `/home/user/PhotoDemon/Controls/pdFxPreview.ctl`
**Usage:** 134 instances
**Key Features:**
- Displays before/after preview
- Toggle between original and effect
- Fit/100% zoom modes
- Click-to-select color
- Click-to-select coordinates
- Integrated with preview system

**Events:**
- `ViewportChanged()` - When zoom mode changes

**Internal Components:**
- `pdPreviewBox` - Actual preview renderer
- `btsZoom` (pdButtonStrip) - Zoom toggle
- `btsState` (pdButtonStrip) - Before/after toggle

**Reference:** `/home/user/PhotoDemon/Controls/pdFxPreview.ctl:60-88`

---

##### pdCanvas
**Purpose:** Main image canvas (MDI child window)
**File:** `/home/user/PhotoDemon/Controls/pdCanvas.ctl`
**Usage:** 1 instance (but critical)
**Key Features:**
- Main image editing surface
- Supports drag/drop
- Displays "no images" welcome screen
- Recent files list
- Quick start buttons

**Complex Nested Structure:**
```vb
Begin PhotoDemon.pdContainer pnlNoImages
   Begin PhotoDemon.pdLabel lblTitle()
   Begin PhotoDemon.pdHyperlink hypRecentFiles
   Begin PhotoDemon.pdCheckBox chkRecentFiles
   Begin PhotoDemon.pdButton cmdStart()
End
```

**Reference:** `/home/user/PhotoDemon/Controls/pdCanvas.ctl:27-100`

---

##### pdCanvasView
**Purpose:** Canvas viewport manager
**File:** `/home/user/PhotoDemon/Controls/pdCanvasView.ctl`
**Usage:** Used internally by pdCanvas

---

##### pdNavigator
**Purpose:** Image navigator (minimap)
**File:** `/home/user/PhotoDemon/Controls/pdNavigator.ctl`
**Usage:** 1 instance
**Key Features:**
- Thumbnail overview of image
- Viewport indicator
- Click-to-navigate

---

##### pdNavigatorInner
**Purpose:** Inner navigator rendering
**File:** `/home/user/PhotoDemon/Controls/pdNavigatorInner.ctl`
**Usage:** Used internally by pdNavigator

---

##### pdLayerList
**Purpose:** Layer list panel
**File:** `/home/user/PhotoDemon/Controls/pdLayerList.ctl`
**Usage:** 1 instance (but critical)
**Key Features:**
- Displays image layers
- Drag-to-reorder
- Layer thumbnails
- Visibility toggles
- Blend mode selection

**Internal Components:**
- `vScroll` (pdScrollBar)
- `lbView` (pdLayerListInner)

**Reference:** `/home/user/PhotoDemon/Controls/pdLayerList.ctl:49-64`

---

##### pdLayerListInner
**Purpose:** Layer list rendering and interaction
**File:** `/home/user/PhotoDemon/Controls/pdLayerListInner.ctl`
**Usage:** Used internally by pdLayerList

---

##### pdHistory
**Purpose:** Undo/redo history panel
**File:** `/home/user/PhotoDemon/Controls/pdHistory.ctl`
**Usage:** 2 instances

---

##### pdColorWheel
**Purpose:** Color wheel selector
**File:** `/home/user/PhotoDemon/Controls/pdColorWheel.ctl`
**Usage:** 2 instances
**Key Features:**
- HSV/HSL color selection
- Interactive dragging
- Custom rendering

---

##### pdColorVariants
**Purpose:** Color variant selector
**File:** `/home/user/PhotoDemon/Controls/pdColorVariants.ctl`
**Usage:** 1 instance

---

##### pdGradientSelector
**Purpose:** Gradient editor
**File:** `/home/user/PhotoDemon/Controls/pdGradientSelector.ctl`
**Usage:** 5 instances
**Key Features:**
- Gradient node editing
- Multi-stop gradients
- Preview rendering

---

##### pdBrushSelector
**Purpose:** Brush selection and configuration
**File:** `/home/user/PhotoDemon/Controls/pdBrushSelector.ctl`
**Usage:** 4 instances

---

##### pdPenSelector
**Purpose:** Pen/stroke selection and configuration
**File:** `/home/user/PhotoDemon/Controls/pdPenSelector.ctl`
**Usage:** 4 instances

---

##### pdStatusBar
**Purpose:** Application status bar
**File:** `/home/user/PhotoDemon/Controls/pdStatusBar.ctl`
**Usage:** Part of main window
**Key Features:**
- Progress display
- Status messages
- Multi-section layout

---

##### pdScrollBar
**Purpose:** Custom scrollbar
**File:** `/home/user/PhotoDemon/Controls/pdScrollBar.ctl`
**Usage:** Used throughout as needed
**Key Features:**
- Owner-drawn
- Theme-aware
- Replaces standard scrollbars

---

##### pdRuler
**Purpose:** Canvas rulers
**File:** `/home/user/PhotoDemon/Controls/pdRuler.ctl`
**Usage:** Part of canvas system
**Key Features:**
- Measurement display
- Unit conversion
- Guide creation

---

##### pdTreeviewOD
**Purpose:** Owner-drawn treeview
**File:** `/home/user/PhotoDemon/Controls/pdTreeviewOD.ctl`
**Usage:** 1 instance

---

##### pdTreeviewViewOD
**Purpose:** Treeview view component
**File:** `/home/user/PhotoDemon/Controls/pdTreeviewViewOD.ctl`
**Usage:** Used internally

---

##### pdMetadataExport
**Purpose:** Metadata export options
**File:** `/home/user/PhotoDemon/Controls/pdMetadataExport.ctl`
**Usage:** 9 instances

---

##### pdRandomizeUI
**Purpose:** Randomization seed control
**File:** `/home/user/PhotoDemon/Controls/pdRandomizeUI.ctl`
**Usage:** 8 instances
**Key Features:**
- Random seed input
- Randomize button
- Used in procedural effects

**Events:**
- `Change()` - Seed changed

---

##### pdResize
**Purpose:** Image resize options panel
**File:** `/home/user/PhotoDemon/Controls/pdResize.ctl`
**Usage:** 7 instances

---

##### pdColorDepth
**Purpose:** Color depth selection
**File:** `/home/user/PhotoDemon/Controls/pdColorDepth.ctl`
**Usage:** 2 instances

---

##### pdPaletteUI
**Purpose:** Palette management UI
**File:** `/home/user/PhotoDemon/Controls/pdPaletteUI.ctl`
**Usage:** 2 instances

---

##### pdNewOld
**Purpose:** New/Old color comparison
**File:** `/home/user/PhotoDemon/Controls/pdNewOld.ctl`
**Usage:** 1 instance

---

##### pdDownload
**Purpose:** Download progress/control
**File:** `/home/user/PhotoDemon/Controls/pdDownload.ctl`
**Usage:** 1 instance

---

##### pdSearchBar
**Purpose:** Search input bar
**File:** `/home/user/PhotoDemon/Controls/pdSearchBar.ctl`
**Usage:** 1 instance

---

##### pdStrip
**Purpose:** Generic strip container
**File:** `/home/user/PhotoDemon/Controls/pdStrip.ctl`
**Usage:** 1 instance

---

##### pdImageStrip
**Purpose:** Image thumbnail strip
**File:** `/home/user/PhotoDemon/Controls/pdImageStrip.ctl`
**Usage:** Used in file dialogs

---

##### pdAccelerator
**Purpose:** Keyboard accelerator manager
**File:** `/home/user/PhotoDemon/Controls/pdAccelerator.ctl`
**Usage:** 1 instance

---

##### pdPreview
**Purpose:** Preview renderer (used by pdFxPreviewCtl)
**File:** `/home/user/PhotoDemon/Controls/pdPreview.ctl`
**Usage:** Used internally

---

##### pdSliderStandalone
**Purpose:** Standalone slider (no text box)
**File:** `/home/user/PhotoDemon/Controls/pdSliderStandalone.ctl`
**Usage:** 7 instances + embedded in pdSlider

---

##### pdListBoxView
**Purpose:** List box view component
**File:** `/home/user/PhotoDemon/Controls/pdListBoxView.ctl`
**Usage:** Used internally

---

##### pdListBoxViewOD
**Purpose:** Owner-drawn list box view
**File:** `/home/user/PhotoDemon/Controls/pdListBoxViewOD.ctl`
**Usage:** Used internally

---

## MAUI Control Mappings

### General Mapping Strategy

**PhotoDemon Pattern:** Owner-drawn controls with custom rendering
**MAUI Approach:** Custom controls using `GraphicsView` (Skia backend) or `SkiaSharp` directly

### Core Mapping Table

| PhotoDemon Control | MAUI Control | Migration Complexity | Notes |
|--------------------|--------------|---------------------|-------|
| **Input Controls** |
| `pdButton` | `Button` + Custom Template | **Medium-High** | Use ControlTemplate for owner-draw; implement image support |
| `pdSlider` | Custom `Slider` + `Entry` | **High** | Need composite control; MAUI Slider + numeric Entry |
| `pdSpinner` | `Stepper` + `Entry` | **Medium-High** | Combine Stepper with Entry validation |
| `pdTextBox` | `Entry` or `Editor` | **Low-Medium** | MAUI has native Unicode; styling needed |
| `pdCheckBox` | `CheckBox` | **Low** | Use ControlTemplate for custom rendering |
| `pdRadioButton` | `RadioButton` | **Low** | Use ControlTemplate for custom rendering |
| `pdColorSelector` | Custom Control | **High** | Build custom color picker with modal dialog |
| **Selection Controls** |
| `pdDropDown` | `Picker` | **Medium** | MAUI Picker is similar; custom template for appearance |
| `pdDropDownFont` | `Picker` + Custom | **Medium-High** | Enumerate fonts; custom item template |
| `pdListBox` | `ListView` or `CollectionView` | **Medium** | `CollectionView` preferred; custom item template |
| `pdListBoxOD` | `CollectionView` + `DataTemplate` | **High** | Full custom item rendering |
| `pdButtonStrip` | Custom `SegmentedControl`-like | **High** | No direct equivalent; build custom |
| `pdButtonStripVertical` | Custom Control | **High** | Build vertical variant |
| **Button Controls** |
| `pdButtonToolbox` | `ImageButton` | **Medium** | Small icon buttons; custom template |
| **Display Controls** |
| `pdLabel` | `Label` | **Low** | Direct mapping; styling needed |
| `pdTitle` | `Label` + `BoxView` | **Low-Medium** | Styled Label with separator |
| `pdHyperlink` | `Label` + `TapGestureRecognizer` | **Low** | Or use custom HyperlinkButton |
| `pdPictureBox` | `Image` or `GraphicsView` | **Medium** | For custom rendering use GraphicsView |
| `pdPictureBoxInteractive` | `GraphicsView` + Touch | **High** | Custom rendering + touch handling |
| `pdProgressBar` | `ProgressBar` | **Low-Medium** | Custom template for appearance |
| **Container Controls** |
| `pdContainer` | `Frame` or `Border` | **Low** | Layout containers; `ContentView` also viable |
| **Complex Controls** |
| `pdCommandBar` | Custom Composite | **High** | Build custom toolbar with Button + Picker |
| `pdCommandBarMini` | Custom Composite | **High** | Simplified version of above |
| `pdFxPreviewCtl` | Custom GraphicsView | **Very High** | Complex rendering; before/after logic |
| `pdCanvas` | Custom GraphicsView | **Very High** | Core image editor; massive custom work |
| `pdCanvasView` | Part of pdCanvas | **Very High** | Viewport management |
| `pdNavigator` | Custom GraphicsView | **High** | Minimap with interaction |
| `pdLayerList` | `CollectionView` + Custom | **Very High** | Drag-drop reorder; thumbnails; complex |
| `pdHistory` | `ListView` | **Medium-High** | Undo/redo list with selection |
| `pdColorWheel` | Custom GraphicsView | **High** | HSV/HSL wheel rendering + interaction |
| `pdColorVariants` | Custom Control | **High** | Color variant grid |
| `pdGradientSelector` | Custom GraphicsView | **Very High** | Node-based gradient editor |
| `pdBrushSelector` | Custom Composite | **High** | Brush preview + settings |
| `pdPenSelector` | Custom Composite | **High** | Pen/stroke preview + settings |
| `pdStatusBar` | Custom Layout | **Medium-High** | Multi-section status display |
| `pdScrollBar` | `ScrollView` scrollbars | **Low** | MAUI has styled scrollbars |
| `pdRuler` | Custom GraphicsView | **High** | Measurement overlay |
| `pdTreeviewOD` | `TreeView` (CommunityToolkit) | **High** | Use toolkit + custom template |
| `pdMetadataExport` | Custom Composite | **Medium** | Metadata options panel |
| `pdRandomizeUI` | Custom Composite | **Medium** | Seed input + button |
| `pdResize` | Custom Composite | **Medium-High** | Resize options panel |
| `pdColorDepth` | Custom Composite | **Medium** | Color depth selector |
| `pdPaletteUI` | Custom Composite | **Medium-High** | Palette editor |
| `pdPreview` | GraphicsView | **High** | Preview rendering |
| `pdSliderStandalone` | `Slider` | **Medium** | Custom template |
| `pdScrollBar` | Part of `ScrollView` | **Low** | Built-in |
| `pdAccelerator` | MAUI Keyboard Accelerators | **Medium** | Use built-in accelerator system |

### VB6 Standard Controls (Rare Usage)

| VB6 Control | MAUI Control | Notes |
|-------------|--------------|-------|
| `Timer` | `IDispatcherTimer` | Use `Dispatcher.CreateTimer()` |
| `TextBox` | `Entry` | Direct mapping |
| `CommandButton` | `Button` | Direct mapping |
| `CheckBox` | `CheckBox` | Direct mapping |
| `Label` | `Label` | Direct mapping |
| `ListBox` | `ListView` or `Picker` | Depends on usage |
| `Line` | `BoxView` | Thin rectangle |
| `Shape` | `BoxView` or custom | Depends on shape |

---

## Event Mapping

### Common Event Patterns

#### VB6 PhotoDemon Custom Control Events → MAUI

| VB6 Event | MAUI Event | Notes |
|-----------|------------|-------|
| `Click()` | `Clicked` | Standard button click |
| `Change()` | `ValueChanged` or `TextChanged` | Depends on control |
| `FinalChange()` | Custom event | Trigger on pointer release |
| `GotFocusAPI()` | `Focused` | Custom focus handling |
| `LostFocusAPI()` | `Unfocused` | Custom focus handling |
| `SetCustomTabTarget()` | MAUI Tab Navigation | Use `TabIndex` and focus navigation |
| `ViewportChanged()` | Custom event | Preview zoom changes |
| `ColorChanged()` | Custom event | Color selection |
| `DrawButton()` | Render in `Drawable` | Owner-draw pattern |
| `CustomDragDrop()` | `Drop` event | Drag/drop support |
| `CustomDragOver()` | `DragOver` event | Drag/drop support |
| `OKClick()` | `Clicked` | CommandBar OK |
| `CancelClick()` | `Clicked` | CommandBar Cancel |
| `ResetClick()` | `Clicked` | CommandBar Reset |
| `RenderTrackImage()` | Custom render | Slider track rendering |

### Event Pattern Examples

#### VB6 Pattern: Button Click
```vb
Private Sub cmdOK_Click()
    ' Handle click
End Sub
```

**MAUI Equivalent:**
```csharp
cmdOK.Clicked += (sender, e) => {
    // Handle click
};
```

---

#### VB6 Pattern: Slider Change
```vb
Private Sub sltBright_Change()
    UpdatePreview
End Sub
```

**MAUI Equivalent:**
```csharp
sltBright.ValueChanged += (sender, e) => {
    UpdatePreview();
};
```

---

#### VB6 Pattern: Button Strip Click
```vb
Private Sub btsModel_Click(ByVal buttonIndex As Long)
    SetLegacyVisibility
    UpdatePreview
End Sub
```

**MAUI Equivalent (Custom Control):**
```csharp
btsModel.SelectionChanged += (sender, e) => {
    var buttonIndex = btsModel.SelectedIndex;
    SetLegacyVisibility();
    UpdatePreview();
};
```

---

#### VB6 Pattern: Command Bar Events
```vb
Private Sub cmdBar_OKClick()
    ' Validate and apply
End Sub

Private Sub cmdBar_CancelClick()
    ' Close without changes
End Sub

Private Sub cmdBar_ResetClick()
    ' Reset to defaults
End Sub
```

**MAUI Equivalent (Custom CommandBar Control):**
```csharp
cmdBar.OKClicked += OnOKClicked;
cmdBar.CancelClicked += OnCancelClicked;
cmdBar.ResetClicked += OnResetClicked;
```

---

#### VB6 Pattern: Preview Viewport Change
```vb
Private Sub pdFxPreview_ViewportChanged()
    UpdatePreview
End Sub
```

**MAUI Equivalent:**
```csharp
pdFxPreview.ViewportChanged += (sender, e) => {
    UpdatePreview();
};
```

---

### Focus Events

PhotoDemon uses custom focus events because "VB focus events are wonky, especially when we use CreateWindow within a UC" (per source comments).

**VB6 Pattern:**
```vb
Public Event GotFocusAPI()
Public Event LostFocusAPI()
Public Event SetCustomTabTarget(ByVal shiftTabWasPressed As Boolean, ByRef newTargetHwnd As Long)
```

**MAUI Equivalent:**
- Use standard `Focused` and `Unfocused` events
- MAUI handles focus properly out-of-the-box
- Use `TabIndex` for tab order
- Consider `FocusNavigationDirection` for advanced scenarios

---

### Keyboard Events

**VB6 Pattern:**
```vb
Public Event KeyPress(ByVal Shift As ShiftConstants, ByVal vKey As Long, ByRef preventFurtherHandling As Boolean)
Public Event KeyDown(ByVal Shift As ShiftConstants, ByVal vKey As Long, ByRef preventFurtherHandling As Boolean)
Public Event KeyUp(ByVal Shift As ShiftConstants, ByVal vKey As Long, ByRef preventFurtherHandling As Boolean)
```

**MAUI Equivalent:**
- Use keyboard accelerators: `<KeyboardAccelerator>`
- Handle at `Page` or `Window` level
- For text input: Use `Entry.TextChanged` and validation

**Example:**
```xml
<Button Text="Save">
    <Button.KeyboardAccelerators>
        <KeyboardAccelerator Modifiers="Ctrl" Key="S" />
    </Button.KeyboardAccelerators>
</Button>
```

---

## Property Mapping

### Common Properties

#### Visual Properties

| VB6 Property | MAUI Property | Notes |
|--------------|---------------|-------|
| `Caption` | `Text` | Control text |
| `Value` | `Value`, `IsChecked`, `Text` | Depends on control |
| `Min` | `Minimum` | Slider/Stepper |
| `Max` | `Maximum` | Slider/Stepper |
| `Enabled` | `IsEnabled` | Enabled state |
| `Visible` | `IsVisible` | Visibility |
| `FontSize` | `FontSize` | Text size |
| `BackColor` | `BackgroundColor` | Background |
| `ForeColor` | `TextColor` | Text color |
| `ToolTipText` | `ToolTipProperties.Text` | Tooltip |
| `Left`, `Top`, `Width`, `Height` | Layout properties | Use layouts instead |

#### PhotoDemon-Specific Properties

| VB6 Property | MAUI Equivalent | Notes |
|--------------|-----------------|-------|
| `SigDigits` | Custom property | Decimal precision for numeric controls |
| `DefaultValue` | Custom property | Reset target |
| `CaptionPadding` | `Padding` or `Margin` | Spacing |
| `UseCustomBackColor` | `BackgroundColor` | Direct color assignment |
| `UseCustomBackgroundColor` | `BackgroundColor` | Direct color assignment |
| `RaiseClickEvent` | Wire up `Clicked` event | Event behavior toggle |
| `CustomDragDropEnabled` | `AllowDrop` + handlers | Drag/drop support |
| `AutoToggle` | `IsCheckable` (custom) | Toggle button behavior |
| `RenderMode` | Custom enum | Owner-draw vs standard |

### Property Pattern Examples

#### VB6: Setting Properties
```vb
Begin PhotoDemon.pdSlider sltBright
   Caption = "brightness"
   Min = -255
   Max = 255
   Value = 0
   DefaultValue = 0
End
```

**MAUI Equivalent (XAML):**
```xml
<controls:PhotoDemonSlider
    x:Name="sltBright"
    Text="brightness"
    Minimum="-255"
    Maximum="255"
    Value="0"
    DefaultValue="0" />
```

**MAUI Equivalent (C#):**
```csharp
var sltBright = new PhotoDemonSlider
{
    Text = "brightness",
    Minimum = -255,
    Maximum = 255,
    Value = 0,
    DefaultValue = 0
};
```

---

#### VB6: Color Properties
```vb
Begin PhotoDemon.pdLabel lblTitle
   Caption = "center position (x, y)"
   FontSize = 12
   ForeColor = 4210752
End
```

**MAUI Equivalent:**
```xml
<Label
    x:Name="lblTitle"
    Text="center position (x, y)"
    FontSize="12"
    TextColor="#404040" />
```

Note: VB6 color `4210752` = RGB(64, 64, 64) = `#404040`

---

#### VB6: Button with Image
```vb
Begin PhotoDemon.pdButton cmdStart
   Caption = "New image..."
   FontSize = 12
   CustomDragDropEnabled = -1  'True
End
```

**MAUI Equivalent:**
```xml
<Button
    x:Name="cmdStart"
    Text="New image..."
    FontSize="12"
    AllowDrop="True"
    Drop="OnDrop"
    DragOver="OnDragOver" />
```

---

### Layout Properties

**VB6 Pattern:**
```vb
Begin PhotoDemon.pdButton cmdOK
   Height = 510
   Left = 6600
   Top = 120
   Width = 1365
End
```

**MAUI Approach:**
Use layouts (Grid, StackLayout, AbsoluteLayout) instead of absolute positioning.

**MAUI Equivalent (Grid):**
```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>

    <Button
        Grid.Column="1"
        Text="OK"
        WidthRequest="91"
        HeightRequest="34" />
</Grid>
```

**Note:** MAUI uses device-independent units. VB6 pixel values need conversion.

---

## Complex Controls Requiring Custom Implementation

### Tier 1: Critical Custom Controls (Highest Priority)

#### 1. pdCanvas
**Complexity:** Very High
**File:** `/home/user/PhotoDemon/Controls/pdCanvas.ctl`
**Purpose:** Main image editing canvas

**Migration Requirements:**
- Custom `GraphicsView` with Skia rendering
- Multi-layer rendering
- Selection overlay
- Tool interaction (paint, select, transform)
- Zoom/pan with high performance
- Drag/drop support (files, layers)
- Ruler integration
- Grid/guide overlay

**MAUI Implementation Strategy:**
1. Use `SKCanvasView` (SkiaSharp.Views.Maui)
2. Implement custom `IDrawable` for rendering
3. Use touch handlers for interaction
4. Layer management with `ObservableCollection<Layer>`
5. Viewport management class
6. Consider hardware acceleration

**Estimated Complexity:** 4-6 weeks (single developer)

---

#### 2. pdFxPreviewCtl
**Complexity:** Very High
**File:** `/home/user/PhotoDemon/Controls/pdFxPreview.ctl`
**Purpose:** Effect preview with before/after toggle

**Migration Requirements:**
- Preview rendering (Skia)
- Before/after state management
- Fit vs 100% zoom
- Pan/scroll in 100% mode
- Click-to-select-color
- Click-to-select-coordinate
- Viewport change events

**MAUI Implementation Strategy:**
1. Custom control with `SKCanvasView`
2. Toggle buttons for state/zoom
3. Touch gesture handling
4. Preview buffer management
5. Async preview generation

**Estimated Complexity:** 2-3 weeks

---

#### 3. pdSlider
**Complexity:** High
**File:** `/home/user/PhotoDemon/Controls/pdSlider.ctl`
**Purpose:** Combined slider + numeric input

**Migration Requirements:**
- Slider track rendering
- Numeric text box with validation
- Synchronization between slider and text
- Min/Max/SigDigits support
- Caption with optional padding
- Gradient rendering modes (optional)
- Change vs FinalChange events
- Locale-aware number formatting

**MAUI Implementation Strategy:**
1. Composite control: `Grid` with `Slider` + `Entry`
2. Custom `Slider` with styled track
3. Two-way binding with validation
4. Custom property for `SigDigits`
5. Optional caption `Label`

**Example Structure:**
```xml
<Grid ColumnDefinitions="Auto,*,Auto">
    <Label Grid.Column="0" Text="{Binding Caption}" />
    <Slider Grid.Column="1"
            Minimum="{Binding Min}"
            Maximum="{Binding Max}"
            Value="{Binding Value}" />
    <Entry Grid.Column="2"
           Text="{Binding Value, StringFormat={Binding FormatString}}"
           WidthRequest="80" />
</Grid>
```

**Estimated Complexity:** 1-2 weeks

---

#### 4. pdCommandBar
**Complexity:** High
**File:** `/home/user/PhotoDemon/Controls/pdCommandBar.ctl`
**Purpose:** Universal tool dialog footer

**Migration Requirements:**
- OK/Cancel buttons
- Preset dropdown (save/load/delete)
- Randomize button
- Reset button
- Last-used preset tracking
- XML preset serialization
- Integration with parent form lifecycle

**MAUI Implementation Strategy:**
1. Custom composite control
2. Use `Grid` or `HorizontalStackLayout`
3. Preset management service
4. Events: OKClicked, CancelClicked, ResetClicked
5. Preset serialization (JSON or XML)

**Layout:**
```
[Preset ▼] [?] [Randomize] [Reset]          [Cancel] [OK]
```

**Estimated Complexity:** 2-3 weeks

---

#### 5. pdLayerList
**Complexity:** Very High
**File:** `/home/user/PhotoDemon/Controls/pdLayerList.ctl`
**Purpose:** Layer panel with thumbnails and drag-reorder

**Migration Requirements:**
- Layer list display with thumbnails
- Visibility toggles per layer
- Blend mode dropdown per layer
- Opacity slider per layer
- Drag-to-reorder
- Scrollable list
- Selection management
- Custom item rendering

**MAUI Implementation Strategy:**
1. Use `CollectionView` with custom `DataTemplate`
2. Drag/drop reordering (MAUI Community Toolkit)
3. Layer view model: `ObservableCollection<LayerViewModel>`
4. Thumbnail generation service
5. Custom item template with controls

**Item Template Structure:**
```xml
<DataTemplate>
    <Grid>
        <Image Source="{Binding Thumbnail}" />
        <Label Text="{Binding Name}" />
        <CheckBox IsChecked="{Binding IsVisible}" />
        <Picker ItemsSource="{Binding BlendModes}"
                SelectedItem="{Binding BlendMode}" />
    </Grid>
</DataTemplate>
```

**Estimated Complexity:** 3-4 weeks

---

#### 6. pdButtonStrip
**Complexity:** High
**File:** `/home/user/PhotoDemon/Controls/pdButtonStrip.ctl`
**Purpose:** Horizontal segmented control

**Migration Requirements:**
- Multiple button segments
- Single selection
- Auto-sizing based on text
- Theme-aware rendering
- `Click(index)` event

**MAUI Implementation Strategy:**
1. No direct MAUI equivalent
2. Option A: Use `RadioButton` group with custom template
3. Option B: Build custom control with `HorizontalStackLayout` of `Button`
4. Option C: Wait for MAUI Community Toolkit `SegmentedControl` (if available)

**Example (Option B):**
```csharp
public class ButtonStrip : ContentView
{
    public event EventHandler<int> SelectionChanged;
    private List<Button> buttons = new List<Button>();

    public void AddButton(string text) {
        var btn = new Button { Text = text };
        btn.Clicked += (s, e) => OnButtonClicked(buttons.IndexOf(btn));
        buttons.Add(btn);
        // Add to layout
    }
}
```

**Estimated Complexity:** 1-2 weeks

---

#### 7. pdColorSelector
**Complexity:** High
**File:** `/home/user/PhotoDemon/Controls/pdColorSelector.ctl`
**Purpose:** Color picker button

**Migration Requirements:**
- Display selected color
- Open color picker modal on click
- Main window color quick-select
- Color change event
- Integration with undo/redo

**MAUI Implementation Strategy:**
1. Custom button with color preview
2. Modal color picker dialog
3. Use community color picker or build custom
4. Consider platform-specific pickers

**Example:**
```csharp
public class ColorSelectorButton : Button
{
    public static readonly BindableProperty SelectedColorProperty = ...;

    public Color SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    protected override void OnClicked() {
        var picker = new ColorPickerDialog();
        picker.SelectedColor = SelectedColor;
        if (await picker.ShowAsync()) {
            SelectedColor = picker.SelectedColor;
            ColorChanged?.Invoke(this, SelectedColor);
        }
    }
}
```

**Estimated Complexity:** 2-3 weeks (including color picker dialog)

---

### Tier 2: Important Custom Controls (Medium Priority)

#### 8. pdGradientSelector
**Complexity:** Very High
**File:** `/home/user/PhotoDemon/Controls/pdGradientSelector.ctl`
**Purpose:** Multi-stop gradient editor

**Migration Requirements:**
- Gradient preview bar
- Draggable color stops
- Add/remove stops
- Color picker per stop
- Position adjustment
- Gradient type (linear/radial/etc.)

**MAUI Implementation Strategy:**
1. Custom `GraphicsView` for gradient preview
2. Touch handlers for stop manipulation
3. Stop collection: `ObservableCollection<GradientStop>`
4. Modal stop editor
5. Skia gradient rendering

**Estimated Complexity:** 3-4 weeks

---

#### 9. pdColorWheel
**Complexity:** High
**File:** `/home/user/PhotoDemon/Controls/pdColorWheel.ctl`
**Purpose:** HSV/HSL color wheel

**Migration Requirements:**
- Circular HSV wheel rendering
- Brightness/Saturation triangle or bar
- Interactive dragging
- Real-time color updates

**MAUI Implementation Strategy:**
1. `SKCanvasView` for rendering
2. Touch gesture handling
3. HSV/RGB conversion utilities
4. Custom `IDrawable` implementation

**Estimated Complexity:** 2-3 weeks

---

#### 10. pdNavigator
**Complexity:** High
**File:** `/home/user/PhotoDemon/Controls/pdNavigator.ctl`
**Purpose:** Image minimap navigator

**Migration Requirements:**
- Thumbnail rendering of full image
- Viewport rectangle overlay
- Click/drag to navigate
- Auto-update on image changes

**MAUI Implementation Strategy:**
1. `SKCanvasView` for thumbnail
2. Overlay rectangle for viewport
3. Pan gesture recognizer
4. Subscribe to canvas viewport changes

**Estimated Complexity:** 1-2 weeks

---

#### 11. pdBrushSelector / pdPenSelector
**Complexity:** High
**Purpose:** Brush/pen configuration panels

**Migration Requirements:**
- Preview rendering
- Settings panel (size, hardness, opacity, etc.)
- Preset system

**MAUI Implementation Strategy:**
1. Composite control with preview + settings
2. Preview using `SKCanvasView`
3. Settings using standard MAUI controls (Slider, Picker)

**Estimated Complexity:** 2 weeks each

---

#### 12. pdMetadataExport / pdResize / pdColorDepth / pdRandomizeUI
**Complexity:** Medium
**Purpose:** Specialized option panels

**Migration Requirements:**
- Various settings controls
- Validation
- Presets

**MAUI Implementation Strategy:**
- Use standard MAUI controls (CheckBox, Entry, Slider, Picker)
- Group in `Frame` or custom `ContentView`
- Data binding to settings models

**Estimated Complexity:** 3-5 days each

---

### Tier 3: Support Controls (Lower Priority)

#### 13. pdHistory
**Complexity:** Medium-High
**Purpose:** Undo/redo history list

**MAUI Implementation:**
- `ListView` with custom item template
- Undo/redo command bindings
- History stack management

**Estimated Complexity:** 1 week

---

#### 14. pdTreeviewOD
**Complexity:** High
**Purpose:** Owner-drawn treeview

**MAUI Implementation:**
- Use MAUI Community Toolkit `TreeView` (if available)
- Custom `DataTemplate` for nodes
- Expand/collapse logic

**Estimated Complexity:** 2-3 weeks

---

#### 15. pdRuler
**Complexity:** Medium-High
**Purpose:** Canvas measurement rulers

**MAUI Implementation:**
- `GraphicsView` overlay on canvas
- Tick mark rendering
- Unit conversion (pixels, inches, cm)
- Guide creation on click

**Estimated Complexity:** 1-2 weeks

---

#### 16. pdStatusBar
**Complexity:** Medium
**Purpose:** Multi-section status bar

**MAUI Implementation:**
- `Grid` with multiple `Label` sections
- Progress indicator area
- Message queue system

**Estimated Complexity:** 3-5 days

---

### Tier 4: Simple Controls (Quick Wins)

#### 17. pdCheckBox, pdRadioButton, pdLabel, pdButton (basic), pdTextBox
**Complexity:** Low-Medium

**MAUI Implementation:**
- Use standard controls with custom templates
- Apply consistent styling
- Wire up events

**Estimated Complexity:** 1-3 days each

---

## Migration Strategy

### Phase 1: Foundation (Weeks 1-4)

**Goal:** Establish base control library

1. **Create Control Library Project**
   - `PhotoDemon.Maui.Controls` class library
   - Reference SkiaSharp.Views.Maui
   - Set up theming system

2. **Theme System Migration**
   - Port `pdThemeColors` → MAUI `ResourceDictionary` + theme manager
   - Define color schemes (Dark, Light, custom)
   - Implement dynamic theme switching

3. **Base Control Classes**
   - Create base classes for common patterns
   - `PhotoDemonControl` base class
   - `PhotoDemonDrawableControl` base (with Skia)
   - Event pattern standardization

4. **Simple Controls (Quick Wins)**
   - `pdLabel` → Styled `Label`
   - `pdCheckBox` → Styled `CheckBox`
   - `pdRadioButton` → Styled `RadioButton`
   - `pdButton` → Styled `Button`
   - `pdTextBox` → Styled `Entry`
   - `pdProgressBar` → Styled `ProgressBar`
   - `pdContainer` → `Frame` or `Border`
   - `pdTitle` → Custom `Label`

**Deliverable:** 8-10 basic controls working

---

### Phase 2: Input Controls (Weeks 5-8)

**Goal:** Implement core input controls

1. **pdSlider**
   - Composite control (Slider + Entry)
   - Value synchronization
   - Caption support
   - SigDigits property

2. **pdSpinner**
   - Stepper + Entry composite
   - Validation logic

3. **pdDropDown**
   - Styled `Picker`
   - Custom item templates

4. **pdButtonStrip**
   - Custom segmented control
   - Selection management

5. **pdColorSelector**
   - Color button with preview
   - Color picker dialog integration

**Deliverable:** 5 input controls complete

---

### Phase 3: Complex Display Controls (Weeks 9-14)

**Goal:** Build advanced rendering controls

1. **pdFxPreviewCtl**
   - Skia-based preview control
   - Before/after toggle
   - Zoom modes
   - Click interactions

2. **pdPictureBox / pdPictureBoxInteractive**
   - Image display with Skia
   - Touch interaction support

3. **pdColorWheel**
   - HSV wheel rendering
   - Interactive color selection

4. **pdGradientSelector**
   - Gradient preview bar
   - Stop manipulation
   - Color editing

**Deliverable:** 4 complex display controls

---

### Phase 4: Command & Layout Controls (Weeks 15-18)

**Goal:** Implement dialog and layout infrastructure

1. **pdCommandBar**
   - OK/Cancel/Reset buttons
   - Preset management system
   - Serialization integration

2. **pdCommandBarMini**
   - Simplified command bar

3. **pdContainer** (enhanced)
   - Collapsible sections
   - Advanced layouts

4. **pdStatusBar**
   - Multi-section status display
   - Progress integration

**Deliverable:** Dialog framework complete

---

### Phase 5: Core Canvas System (Weeks 19-26)

**Goal:** Build main editing canvas

1. **pdCanvas (Basic)**
   - Image display
   - Zoom/pan
   - Basic rendering

2. **pdCanvas (Advanced)**
   - Multi-layer rendering
   - Selection overlay
   - Tool integration
   - Drag/drop support

3. **pdCanvasView**
   - Viewport management
   - Coordinate systems

4. **pdRuler**
   - Measurement overlays
   - Unit conversion

5. **pdNavigator**
   - Minimap thumbnail
   - Viewport indicator
   - Click-to-navigate

**Deliverable:** Core canvas system functional

---

### Phase 6: Layer & History (Weeks 27-32)

**Goal:** Implement layer and history panels

1. **pdLayerList**
   - Layer display with thumbnails
   - Drag-to-reorder
   - Visibility toggles
   - Blend modes

2. **pdLayerListInner**
   - Rendering logic
   - Interaction handling

3. **pdHistory**
   - Undo/redo list
   - Stack management
   - Command integration

**Deliverable:** Layer and history systems complete

---

### Phase 7: Specialized Controls (Weeks 33-40)

**Goal:** Implement remaining specialized controls

1. **Tool Selectors**
   - `pdBrushSelector`
   - `pdPenSelector`

2. **Option Panels**
   - `pdMetadataExport`
   - `pdResize`
   - `pdColorDepth`
   - `pdRandomizeUI`
   - `pdPaletteUI`

3. **Misc Controls**
   - `pdDownload`
   - `pdSearchBar`
   - `pdTreeviewOD`
   - `pdImageStrip`

**Deliverable:** All controls implemented

---

### Phase 8: Polish & Optimization (Weeks 41-44)

**Goal:** Refinement and performance

1. **Performance Optimization**
   - Rendering performance profiling
   - Memory optimization
   - Lazy loading
   - Control pooling where appropriate

2. **Accessibility**
   - Screen reader support
   - Keyboard navigation
   - High contrast themes

3. **Documentation**
   - Control API documentation
   - Usage examples
   - Migration guide

4. **Testing**
   - Unit tests for controls
   - Visual regression tests
   - Touch interaction tests

**Deliverable:** Production-ready control library

---

## Implementation Priorities

### Critical Path Controls

These controls are required for basic application functionality:

1. **pdCanvas** - Without this, no image editing is possible
2. **pdCommandBar** - Required for all tool dialogs
3. **pdFxPreviewCtl** - Required for effect previews
4. **pdSlider** - Most common input control (454 uses)
5. **pdButton, pdLabel, pdCheckBox** - Basic UI building blocks

### Recommended Development Order

**Month 1:**
- Theme system
- Basic controls (Label, CheckBox, RadioButton, Button, TextBox)
- Container controls

**Month 2:**
- pdSlider (most used control)
- pdDropDown
- pdSpinner
- pdButtonStrip

**Month 3:**
- pdFxPreviewCtl
- pdColorSelector
- pdCommandBar

**Month 4-5:**
- pdCanvas (basic)
- pdNavigator
- pdRuler

**Month 6-7:**
- pdCanvas (advanced)
- pdLayerList
- pdHistory

**Month 8-9:**
- Specialized controls
- pdGradientSelector
- pdColorWheel
- Tool selectors

**Month 10:**
- Remaining controls
- Option panels
- Utilities

**Month 11:**
- Polish, optimization, accessibility

### Parallel Development Strategy

Multiple developers can work in parallel:

- **Developer 1:** Theme system + basic controls
- **Developer 2:** Input controls (Slider, Spinner, etc.)
- **Developer 3:** Canvas system
- **Developer 4:** Layer/History panels
- **Developer 5:** Specialized controls

### Testing Strategy

1. **Unit Tests**
   - Control property getters/setters
   - Value validation
   - Event raising

2. **Integration Tests**
   - Control interaction
   - Parent-child communication
   - Theme switching

3. **Visual Tests**
   - Rendering correctness
   - High DPI handling
   - Cross-platform appearance

4. **Performance Tests**
   - Rendering performance (60 FPS target)
   - Memory usage
   - Large image handling

---

## Technical Considerations

### Rendering

**VB6 Approach:**
- GDI+ for all custom rendering
- Direct DC (Device Context) manipulation
- `HasDC = 0 'False` to disable automatic DC creation

**MAUI Approach:**
- Use SkiaSharp for custom rendering
- `SKCanvasView` for interactive controls
- `GraphicsView` with `IDrawable` for simpler cases
- Consider hardware acceleration via Metal/DirectX/Vulkan backends

### Unicode Support

**VB6 Challenge:**
- VB6 has poor Unicode support
- PhotoDemon uses API edit controls (`CreateWindowEx` with `EDIT` class)

**MAUI Solution:**
- Native Unicode support throughout
- No special handling needed

### High DPI

**VB6 Approach:**
- Manual DPI scaling
- Pixel-based measurements
- DPI-aware manifests

**MAUI Approach:**
- Device-independent units
- Automatic scaling on all platforms
- No manual DPI handling required

### Theming

**VB6 Approach:**
- Custom `pdThemeColors` class
- Color lists per control
- Manual color application

**MAUI Approach:**
- Use `ResourceDictionary` with theme resources
- Dynamic resource binding: `{DynamicResource BackgroundColor}`
- Theme switching via resource dictionary swap

**Example:**
```xml
<ResourceDictionary x:Key="DarkTheme">
    <Color x:Key="BackgroundColor">#1E1E1E</Color>
    <Color x:Key="ForegroundColor">#FFFFFF</Color>
    <Color x:Key="AccentColor">#0078D4</Color>
</ResourceDictionary>
```

### Focus Management

**VB6 Challenge:**
- Complex focus management due to API-created windows
- Custom `GotFocusAPI` / `LostFocusAPI` events
- Custom tab targeting

**MAUI Solution:**
- Use standard `Focused` / `Unfocused` events
- `TabIndex` for tab order
- `IsFocused` property for focus state
- Focus scope management

### Custom Events

**VB6 Pattern:**
```vb
Public Event ColorChanged()
' ...
RaiseEvent ColorChanged
```

**MAUI Pattern:**
```csharp
public event EventHandler ColorChanged;
// ...
ColorChanged?.Invoke(this, EventArgs.Empty);
```

**With Arguments:**
```csharp
public event EventHandler<int> SelectionChanged;
// ...
SelectionChanged?.Invoke(this, buttonIndex);
```

### Control Properties

**VB6 Pattern:**
```vb
Public Property Get Caption() As String
    Caption = m_Caption
End Property

Public Property Let Caption(ByRef newCaption As String)
    m_Caption = newCaption
    RedrawControl
End Property
```

**MAUI Pattern (Bindable Property):**
```csharp
public static readonly BindableProperty CaptionProperty =
    BindableProperty.Create(
        nameof(Caption),
        typeof(string),
        typeof(PhotoDemonControl),
        string.Empty,
        propertyChanged: OnCaptionChanged);

public string Caption {
    get => (string)GetValue(CaptionProperty);
    set => SetValue(CaptionProperty, value);
}

static void OnCaptionChanged(BindableObject bindable, object oldValue, object newValue) {
    var control = (PhotoDemonControl)bindable;
    control.InvalidateSurface(); // Trigger redraw
}
```

---

## File References

### Control Files Analyzed

All controls located in: `/home/user/PhotoDemon/Controls/`

**Key Files Referenced:**
- `/home/user/PhotoDemon/Controls/pdButton.ctl:38-78` - Button architecture
- `/home/user/PhotoDemon/Controls/pdSlider.ctl:59-95` - Slider design
- `/home/user/PhotoDemon/Controls/pdCanvas.ctl:27-100` - Canvas structure
- `/home/user/PhotoDemon/Controls/pdTextBox.ctl:45-75` - Text box implementation
- `/home/user/PhotoDemon/Controls/pdColorSelector.ctl:56-64` - Color selector
- `/home/user/PhotoDemon/Controls/pdDropDown.ctl:48-95` - Dropdown design
- `/home/user/PhotoDemon/Controls/pdCommandBar.ctl:135-150` - Command bar
- `/home/user/PhotoDemon/Controls/pdFxPreview.ctl:60-98` - Preview control
- `/home/user/PhotoDemon/Controls/pdLayerList.ctl:49-99` - Layer list

### Form Files Analyzed

Forms located in: `/home/user/PhotoDemon/Forms/`

**Sample Forms:**
- `/home/user/PhotoDemon/Forms/Adjustments_BrightnessContrast.frm:27-200` - Control usage patterns
- `/home/user/PhotoDemon/Forms/Effects_Artistic_Kaleidoscope.frm:73-355` - Property patterns
- `/home/user/PhotoDemon/Forms/Toolpanel_Measure.frm:33-305` - Container usage

### Support Tool Files

- `/home/user/PhotoDemon/Support/Update Patcher 2.0/frmPatch.frm` - Standard VB6 controls
- `/home/user/PhotoDemon/Support/Project-wide search and replace/frmMain.frm` - Standard VB6 controls
- `/home/user/PhotoDemon/Support/i18n-manager/frmGenerateI18N.frm` - Standard VB6 controls

---

## Summary Statistics

### Control Counts

- **Total Custom Controls:** 56 files
- **Total Custom Control Instances:** 2,100+
- **Forms Using Custom Controls:** 200+
- **Standard VB6 Controls:** ~30 instances (support tools only)

### Migration Effort Estimate

**Total Estimated Time:** 44 weeks (11 months) with single developer

**Team Estimate (5 developers):** 10-11 months with parallel development

**Control Complexity Breakdown:**
- **Very High:** 6 controls (Canvas, FxPreview, LayerList, GradientSelector, etc.)
- **High:** 15 controls (Slider, CommandBar, ColorWheel, ButtonStrip, etc.)
- **Medium:** 20 controls (Spinner, DropDown, various option panels)
- **Low:** 15 controls (Label, CheckBox, Button, TextBox, etc.)

### Risk Assessment

**High Risk:**
- Canvas performance with large images
- Layer list drag-drop on mobile
- Gradient editor touch precision
- Cross-platform rendering consistency

**Medium Risk:**
- Theme system migration
- Preset serialization compatibility
- Color picker platform differences
- Font enumeration differences

**Low Risk:**
- Basic controls (Label, Button, etc.)
- Event wiring
- Property binding
- Layout conversion

---

## Recommendations

### 1. Start with Foundation

Begin with theme system and basic controls to establish patterns.

### 2. Build Incrementally

Implement controls in priority order, testing each thoroughly before moving on.

### 3. Consider Third-Party Libraries

Evaluate:
- **MAUI Community Toolkit** - Additional controls
- **Syncfusion MAUI** - Commercial control suite (includes gradient editor, color picker, etc.)
- **Telerik MAUI** - Commercial control suite
- **DevExpress MAUI** - Commercial control suite

**Trade-off:** Commercial libraries reduce development time but add licensing costs and dependencies.

### 4. Prototype Critical Controls Early

Build proof-of-concept for Canvas and FxPreview early to validate approach.

### 5. Maintain VB6 Compatibility Layer

Consider building a compatibility layer that mirrors VB6 control names and properties to ease form migration.

**Example:**
```csharp
// Instead of learning new names:
public class pdSlider : PhotoDemonSlider { }
public class pdButton : PhotoDemonButton { }
```

### 6. Automate Form Migration

Build tools to convert VB6 .frm files to MAUI XAML:
- Parse VB6 form structure
- Map controls
- Generate XAML
- Manual cleanup still required but saves significant time

### 7. Parallel Development

Once patterns are established, multiple developers can work on different control categories simultaneously.

### 8. Focus on Performance

Canvas and preview controls must render at 60 FPS for smooth interaction. Profile early and often.

### 9. Document As You Go

Create usage examples and API docs for each control as it's completed.

### 10. Plan for Accessibility

Build in screen reader support, keyboard navigation, and high contrast from the start.

---

## Conclusion

The PhotoDemon VB6 to .NET 10 MAUI UI migration is a substantial undertaking due to the extensive custom control library. The application relies on 56 custom owner-drawn controls with sophisticated rendering and interaction patterns.

**Key Success Factors:**
1. Establishing solid foundation with theme system and base classes
2. Building controls incrementally in priority order
3. Leveraging SkiaSharp for custom rendering
4. Thorough testing at each phase
5. Sufficient team size for parallel development

**Timeline:**
- **Single Developer:** 44 weeks (11 months)
- **5-Person Team:** 10-11 months

**Biggest Challenges:**
- pdCanvas (main editing surface)
- pdFxPreviewCtl (effect previews)
- pdLayerList (layer management)
- pdGradientSelector (node-based editing)

**Quick Wins:**
- Basic controls (Label, CheckBox, Button, etc.)
- Theme system
- Simple container controls

With careful planning and execution, the migration is achievable and will result in a modern, cross-platform control library that maintains PhotoDemon's distinctive user experience.

---

**End of Document**
