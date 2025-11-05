# Form Builder - Implementation Guide

**Component:** Template Builder (Phase 2.1)
**Duration:** Weeks 5-6 (2 weeks)
**Priority:** CRITICAL - Foundation for entire checklist system
**Dependencies:** Database schema must be created first

---

## Table of Contents

1. [Overview](#overview)
2. [UI Layout & Mockups](#ui-layout--mockups)
3. [User Journey](#user-journey)
4. [Component Breakdown](#component-breakdown)
5. [Data Flow](#data-flow)
6. [Step-by-Step Implementation Guide](#step-by-step-implementation-guide)
7. [Field Type Configurations](#field-type-configurations)
8. [Validation Rules Builder](#validation-rules-builder)
9. [Conditional Logic Builder](#conditional-logic-builder)
10. [Testing Checklist](#testing-checklist)

---

## Overview

### What is the Form Builder?

The Form Builder is an **admin-only tool** that enables Head Office ICT managers to create dynamic checklist templates without writing code. Think of it as a "survey builder" like Google Forms or SurveyMonkey, but specifically designed for KTDA's operational checklists.

### Key Features

✅ **Drag-and-drop interface** - Visual form designer
✅ **Section organization** - Group related questions
✅ **7 field types** - Text, Number, Date, Boolean, Dropdown, TextArea, FileUpload
✅ **Validation rules** - Required, min/max, regex patterns
✅ **Conditional logic** - Show/hide fields based on answers
✅ **Pre-fill mapping** - Link fields to hardware/software inventory
✅ **Live preview** - See form as users will see it
✅ **Version control** - Track template changes over time

### User Persona

**Primary User:** Sarah Wambui (System Administrator at Head Office)

**Goals:**
- Create "Factory Monthly Report" template with 33 questions
- Organize questions into 5 logical sections
- Configure validation to prevent bad data
- Link hardware fields to inventory for auto-fill
- Preview and publish template for 100+ factories

**Pain Points:**
- Current Excel templates are hard to modify
- Adding questions requires updating formulas
- No validation (factories submit incorrect data)
- No version control (confusion about latest template)

---

## UI Layout & Mockups

### Overall Layout: Three-Panel Design

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ KTDA Form Builder                                    [Save] [Preview] [Publish]        │
│ Template: Factory Monthly Report (v1.0)             Last saved: 2 mins ago             │
├──────────────────┬──────────────────────────────────────────────┬──────────────────────┤
│                  │                                              │                      │
│  LEFT PANEL      │         CENTER PANEL                         │    RIGHT PANEL       │
│  (Toolbox)       │         (Canvas)                             │    (Properties)      │
│  Width: 250px    │         Width: flex                          │    Width: 350px      │
│                  │                                              │                      │
├──────────────────┼──────────────────────────────────────────────┼──────────────────────┤
│                  │                                              │                      │
│ [+ Add Section]  │  ┌────────────────────────────────────────┐ │ SECTION PROPERTIES   │
│                  │  │ 📦 Section: Hardware Status        [⋮⋮]│ │ ──────────────────── │
│ Field Types:     │  │ Computer and device inventory      [▼]│ │                      │
│ ──────────────   │  ├────────────────────────────────────────┤ │ (Click section or    │
│                  │  │ [⋮⋮] Q1: Total computers      [⚙️][🗑️]│ │  item to edit)       │
│ 📝 Text Input    │  │      Type: Number | Required       │ │                      │
│ (Drag me)        │  │                                        │ │                      │
│                  │  │ [⋮⋮] Q2: Operational         [⚙️][🗑️]│ │                      │
│ 🔢 Number        │  │      Type: Number | Required       │ │                      │
│ (Drag me)        │  │                                        │ │                      │
│                  │  │ [⋮⋮] Q3: Under repair        [⚙️][🗑️]│ │                      │
│ 📅 Date Picker   │  │      Type: Number | Required       │ │                      │
│ (Drag me)        │  │                                        │ │                      │
│                  │  │ [+ Add Question]                       │ │                      │
│ ✓ Yes/No         │  └────────────────────────────────────────┘ │                      │
│ (Drag me)        │                                              │                      │
│                  │  ┌────────────────────────────────────────┐ │                      │
│ 📋 Dropdown      │  │ 📦 Section: Software Licenses      [⋮⋮]│ │                      │
│ (Drag me)        │  │ Software installations & licensing [▼]│ │                      │
│                  │  ├────────────────────────────────────────┤ │                      │
│ 📄 Text Area     │  │ [⋮⋮] Q4: EWS Version          [⚙️][🗑️]│ │                      │
│ (Drag me)        │  │      Type: Text | Required         │ │                      │
│                  │  │                                        │ │                      │
│ 📎 File Upload   │  │ [+ Add Question]                       │ │                      │
│ (Drag me)        │  └────────────────────────────────────────┘ │                      │
│                  │                                              │                      │
│                  │  [+ Add Section Button]                      │                      │
│                  │                                              │                      │
│                  │  (Scroll for more sections...)               │                      │
│                  │                                              │                      │
└──────────────────┴──────────────────────────────────────────────┴──────────────────────┘
```

### Panel Descriptions

#### LEFT PANEL: Toolbox

**Purpose:** Source of all draggable components

**Contains:**
1. **Add Section Button** - Creates new section
2. **Field Types** - 7 draggable field types with icons
3. **Help Text** - Brief description of each field type

**Behavior:**
- Always visible (fixed position)
- Scroll if too many field types
- Visual feedback on drag (ghost element)

---

#### CENTER PANEL: Canvas

**Purpose:** Visual representation of the form being built

**Contains:**
1. **Section Cards** - Collapsible containers
2. **Question Items** - Individual fields within sections
3. **Add Question Buttons** - Add field to specific section
4. **Drag Handles** - Reorder sections and questions

**Behavior:**
- Main scrollable area
- Sections can be collapsed/expanded
- Drag-drop zones highlighted on hover
- Empty state: "Drag a section here to start"

---

#### RIGHT PANEL: Properties

**Purpose:** Edit properties of selected section or question

**Contains:**
1. **Section Properties Form** - When section selected
2. **Question Properties Form** - When question selected
3. **Validation Rules** - When question selected
4. **Conditional Logic** - When question selected
5. **Pre-fill Source** - When question selected

**Behavior:**
- Empty state: "Select a section or question to edit"
- Live updates as user types
- Validation errors shown inline
- Context-sensitive (changes based on selection)

---

## User Journey

### Journey 1: Creating a New Template

```
┌─────────────────────────────────────────────────┐
│ Step 1: Navigate to Template Builder           │
│ Dashboard → Templates → [+ New Template]        │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 2: Create Template Modal Opens            │
│                                                 │
│ Template Name: [Factory Monthly Report_____]   │
│ Template Code: [FACTORY_MONTHLY_________]      │
│ Description:   [Comprehensive monthly...____]  │
│ Frequency:     [Dropdown: Monthly ▼]           │
│ Requires Approval: [✓]                         │
│ Applicable To: [☑ Factories  ☐ Subsidiaries]  │
│                                                 │
│           [Cancel]  [Create Template]          │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 3: Builder Opens with Empty Canvas        │
│ Three panels displayed                          │
│ Canvas shows: "Add your first section"         │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 4: User Clicks [+ Add Section]            │
│ Section modal opens                             │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 5: Configure Section                      │
│                                                 │
│ Section Name: [Hardware Status__________]      │
│ Description:  [Computer and device...___]      │
│ Icon:         [Icon picker: fa-desktop ▼]      │
│ Collapsible:  [✓]                              │
│ Collapsed by default: [☐]                      │
│                                                 │
│           [Cancel]  [Add Section]              │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 6: Section Appears on Canvas              │
│ Empty section card with [+ Add Question]       │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 7: User Drags "Number" Field Type         │
│ Drags from left panel into section             │
│ Drop zone highlighted in section               │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 8: Field Configuration Modal Opens        │
│ (Context: Number field type)                   │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 9: Configure Question                     │
│                                                 │
│ Question Text: [Total number of computers___]  │
│ Description:   [Count all desktop and...____]  │
│ Required:      [✓]                             │
│ Default Value: [___] (optional)                │
│                                                 │
│ Number Settings:                                │
│ Min Value:     [1________]                     │
│ Max Value:     [500______]                     │
│ Decimals:      [0] (whole numbers)             │
│ Step:          [1]                             │
│                                                 │
│ Pre-fill Source: [Hardware Inventory ▼]        │
│                                                 │
│           [Cancel]  [Add Question]             │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 10: Question Appears in Section           │
│ Shows question summary with type badge          │
│ User can edit [⚙️] or delete [🗑️]              │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 11: Repeat for All Questions              │
│ Add 32 more questions across 5 sections        │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 12: Reorder Sections & Questions          │
│ Drag sections up/down                           │
│ Drag questions within sections                  │
│ Changes saved automatically                     │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 13: Click [Preview] Button                │
│ Modal opens showing full form as users see it  │
│ Test validation (try invalid inputs)           │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 14: Click [Publish] Button                │
│ Confirmation: "Publish template? Factories      │
│ will be able to use this template."             │
│           [Cancel]  [Publish]                  │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│ Step 15: Template Active                       │
│ Template.IsActive = 1                           │
│ Available in factory dropdown menus             │
│ Success message: "Template published!"          │
└─────────────────────────────────────────────────┘
```

---

## Component Breakdown

### Component 1: Template List Page

**URL:** `/TemplateBuilder/Index`

**Layout:**
```
┌────────────────────────────────────────────────────────────────────┐
│ Checklist Templates                          [+ New Template]      │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│ Search: [_________________] 🔍   Filter: [All ▼] [Active ▼]      │
│                                                                    │
│ ┌──────────────────────────────────────────────────────────────┐ │
│ │ Factory Monthly Report                          [Active] [▼] │ │
│ │ Monthly • 33 questions • 5 sections                          │ │
│ │ Created: Oct 1, 2025 • Version 1.0                          │ │
│ │ Used by: 100 factories • Last month: 95 submissions         │ │
│ │                                                              │ │
│ │ [Edit] [Clone] [Versions] [Deactivate]                      │ │
│ └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│ ┌──────────────────────────────────────────────────────────────┐ │
│ │ Factory Daily Checklist                    [Active] [▼]      │ │
│ │ Daily • 12 questions • 2 sections                           │ │
│ │ Created: Sep 15, 2025 • Version 1.0                        │ │
│ │ Used by: 100 factories • Yesterday: 98 submissions         │ │
│ │                                                              │ │
│ │ [Edit] [Clone] [Versions] [Deactivate]                      │ │
│ └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│ ┌──────────────────────────────────────────────────────────────┐ │
│ │ Regional Weekly Summary                   [Inactive] [▼]     │ │
│ │ Weekly • 18 questions • 3 sections                          │ │
│ │ Created: Aug 20, 2025 • Version 2.0                        │ │
│ │ Used by: 7 regional offices • Last week: 7 submissions     │ │
│ │                                                              │ │
│ │ [Edit] [Clone] [Versions] [Activate]                        │ │
│ └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│                       [1] 2 3 ... Next →                           │
└────────────────────────────────────────────────────────────────────┘
```

**Elements:**
- **Header** with "New Template" button
- **Search bar** for filtering templates
- **Template cards** showing metadata
- **Action buttons** for each template
- **Pagination** if more than 10 templates

---

### Component 2: Section Card (on Canvas)

**Appearance: Expanded State**
```
┌────────────────────────────────────────────────────────┐
│ [⋮⋮] 📦 Section: Hardware Status           [⚙️] [🗑️] [▼]│
│ Computer and device inventory                          │
├────────────────────────────────────────────────────────┤
│                                                        │
│ [⋮⋮] Q1: Total number of computers          [⚙️] [🗑️] │
│      Type: Number | Required | Min: 1, Max: 500       │
│      Pre-fill: Hardware Inventory                     │
│                                                        │
│ [⋮⋮] Q2: Operational computers               [⚙️] [🗑️] │
│      Type: Number | Required                          │
│      Pre-fill: Hardware Inventory (Status=Operational)│
│                                                        │
│ [⋮⋮] Q3: Computers under repair              [⚙️] [🗑️] │
│      Type: Number | Required                          │
│      Pre-fill: Hardware Inventory (Status=Repair)     │
│                                                        │
│ [+ Add Question to This Section]                      │
│                                                        │
└────────────────────────────────────────────────────────┘
```

**Appearance: Collapsed State**
```
┌────────────────────────────────────────────────────────┐
│ [⋮⋮] 📦 Section: Hardware Status           [⚙️] [🗑️] [▶]│
│ Computer and device inventory • 3 questions            │
└────────────────────────────────────────────────────────┘
```

**Elements:**
- **Drag handle [⋮⋮]** - Reorder section
- **Icon** - Visual identifier (fa-desktop, fa-cube, etc.)
- **Section name** - Editable
- **Description** - Brief explanation
- **Edit button [⚙️]** - Opens properties panel
- **Delete button [🗑️]** - Removes section (with confirmation)
- **Collapse/Expand [▼/▶]** - Toggle visibility
- **Question list** - Child items
- **Add Question button** - Add field to this section

---

### Component 3: Question Item (within Section)

**Appearance:**
```
┌────────────────────────────────────────────────────────┐
│ [⋮⋮] Q5: WAN Connection Type                [⚙️] [🗑️] │
│      Type: Dropdown | Required                        │
│      Options: Reference Data (WAN_TYPE)               │
│      Pre-fill: Last recorded value                    │
└────────────────────────────────────────────────────────┘
```

**Elements:**
- **Drag handle [⋮⋮]** - Reorder within section
- **Question number** - Auto-assigned (Q1, Q2, Q3...)
- **Question text** - Displayed to users
- **Type badge** - Field type with color coding
- **Metadata** - Required status, validation summary
- **Pre-fill indicator** - Shows if auto-filled
- **Edit button [⚙️]** - Opens properties panel
- **Delete button [🗑️]** - Removes question (with confirmation)

---

### Component 4: Add Section Modal

**Layout:**
```
┌──────────────────────────────────────────────────────┐
│ Add New Section                              [✕]     │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Section Name: *                                      │
│ [Hardware Status_____________________________]       │
│                                                      │
│ Description:                                         │
│ [Computer and device inventory_______________]       │
│ [________________________________________]       │
│                                                      │
│ Icon: *                                              │
│ [fa-desktop ▼] 📦 Preview                           │
│                                                      │
│ Common icons:                                        │
│ [📦 fa-desktop] [💻 fa-laptop] [🖨️ fa-print]        │
│ [🌐 fa-network-wired] [📊 fa-chart-bar]              │
│ [🔧 fa-tools] [🔐 fa-lock]                          │
│                                                      │
│ Display Settings:                                    │
│ [✓] Collapsible                                     │
│ [☐] Collapsed by default                            │
│                                                      │
│ Display Order: [Auto] (will be added at end)        │
│                                                      │
│                      [Cancel]  [Add Section]        │
└──────────────────────────────────────────────────────┘
```

**Validation:**
- Section name required (max 100 characters)
- Icon required
- Section name must be unique within template

---

### Component 5: Add Question Modal (Context: Number Field)

**Layout:**
```
┌──────────────────────────────────────────────────────┐
│ Add Number Field                             [✕]     │
├──────────────────────────────────────────────────────┤
│                                                      │
│ BASIC SETTINGS                                       │
│ ─────────────────────────────────────────────────── │
│                                                      │
│ Question Text: *                                     │
│ [Total number of computers___________________]       │
│                                                      │
│ Description (Help Text):                             │
│ [Count all desktop and laptop computers______]       │
│ [________________________________________]       │
│                                                      │
│ [✓] Required field                                  │
│ [☐] Read-only (display only)                        │
│                                                      │
│ Default Value:                                       │
│ [______] (leave empty for no default)               │
│                                                      │
│ VALIDATION RULES                                     │
│ ─────────────────────────────────────────────────── │
│                                                      │
│ Number Type:                                         │
│ ● Integer  ○ Decimal                                │
│                                                      │
│ Range:                                               │
│ Min Value: [1______]                                │
│ Max Value: [500____]                                │
│                                                      │
│ Step: [1] (increment value)                          │
│                                                      │
│ PRE-FILL SETTINGS                                    │
│ ─────────────────────────────────────────────────── │
│                                                      │
│ Auto-fill from:                                      │
│ [Hardware Inventory ▼]                              │
│                                                      │
│ Options:                                             │
│ • None (manual entry)                                │
│ • Hardware Inventory - Computer Count                │
│ • Hardware Inventory - By Status                     │
│ • Software Licenses - Count                          │
│ • Ticket Statistics - Count                          │
│ • Custom Query                                       │
│                                                      │
│ [✓] Allow user to override pre-filled value         │
│                                                      │
│                      [Cancel]  [Add Question]       │
└──────────────────────────────────────────────────────┘
```

---

### Component 6: Properties Panel (Section Selected)

**Layout:**
```
┌──────────────────────────────────────────────────────┐
│ SECTION PROPERTIES                                   │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Section: Hardware Status                             │
│                                                      │
│ Section Name: *                                      │
│ [Hardware Status_____________________________]       │
│                                                      │
│ Description:                                         │
│ [Computer and device inventory_______________]       │
│                                                      │
│ Icon:                                                │
│ [fa-desktop ▼] 📦                                   │
│                                                      │
│ Display Settings:                                    │
│ [✓] Collapsible                                     │
│ [☐] Collapsed by default                            │
│                                                      │
│ Display Order:                                       │
│ [1__] (1 = first, higher = later)                   │
│                                                      │
│ Questions in This Section: 3                         │
│                                                      │
│ ────────────────────────────────────────────────    │
│                                                      │
│ [Save Changes]  [Revert]                            │
│                                                      │
└──────────────────────────────────────────────────────┘
```

**Behavior:**
- Updates live as user types
- Save button enabled only if changes made
- Validation errors shown inline
- Revert button discards unsaved changes

---

### Component 7: Properties Panel (Question Selected - Number Field)

**Layout:**
```
┌──────────────────────────────────────────────────────┐
│ QUESTION PROPERTIES                                  │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Field Type: Number                     [Change ▼]   │
│                                                      │
│ Question Text: *                                     │
│ [Total number of computers___________________]       │
│                                                      │
│ Description:                                         │
│ [Count all desktop and laptop computers______]       │
│                                                      │
│ [✓] Required    [☐] Read-only                       │
│                                                      │
│ Default Value: [______]                             │
│                                                      │
│ ────────────────────────────────────────────────    │
│ VALIDATION RULES                                     │
│ ────────────────────────────────────────────────    │
│                                                      │
│ Number Type: ● Integer  ○ Decimal                   │
│                                                      │
│ Min Value: [1______]                                │
│ Max Value: [500____]                                │
│ Step:      [1]                                       │
│                                                      │
│ ────────────────────────────────────────────────    │
│ PRE-FILL SETTINGS                                    │
│ ────────────────────────────────────────────────    │
│                                                      │
│ Auto-fill from:                                      │
│ [Hardware Inventory ▼]                              │
│                                                      │
│ [✓] Allow user override                             │
│                                                      │
│ ────────────────────────────────────────────────    │
│ CONDITIONAL LOGIC                                    │
│ ────────────────────────────────────────────────    │
│                                                      │
│ [☐] Show this field conditionally                   │
│                                                      │
│ [+ Add Condition]                                    │
│                                                      │
│ ────────────────────────────────────────────────    │
│                                                      │
│ [Save Changes]  [Revert]                            │
│                                                      │
└──────────────────────────────────────────────────────┘
```

---

### Component 8: Preview Modal

**Layout:**
```
┌────────────────────────────────────────────────────────────────────┐
│ Preview: Factory Monthly Report                         [✕] Close │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│ This is how users will see the form.                              │
│ Try filling it out to test validation.                            │
│                                                                    │
│ ┌────────────────────────────────────────────────────────────┐   │
│ │ [Accordion Header]                                         │   │
│ │ 📦 Hardware Status                                    [▼]  │   │
│ └────────────────────────────────────────────────────────────┘   │
│ ┌────────────────────────────────────────────────────────────┐   │
│ │                                                            │   │
│ │ Total number of computers: *                               │   │
│ │ [_____] (1-500)                                           │   │
│ │ Count all desktop and laptop computers                     │   │
│ │                                                            │   │
│ │ Operational computers: *                                   │   │
│ │ [_____]                                                   │   │
│ │                                                            │   │
│ │ Computers under repair: *                                  │   │
│ │ [_____]                                                   │   │
│ │                                                            │   │
│ └────────────────────────────────────────────────────────────┘   │
│                                                                    │
│ ┌────────────────────────────────────────────────────────────┐   │
│ │ [Accordion Header]                                         │   │
│ │ 📦 Software Licenses                              [▶]  │   │
│ └────────────────────────────────────────────────────────────┘   │
│                                                                    │
│ (5 more sections...)                                              │
│                                                                    │
│ [Submit] button disabled (preview only)                           │
│                                                                    │
│                              [Close Preview]                      │
└────────────────────────────────────────────────────────────────────┘
```

**Purpose:**
- Test form flow
- Verify validation works
- Check field ordering
- See actual user experience
- Cannot actually submit (preview only)

---

## Data Flow

### Flow 1: Creating a Template

```
User Action                    Frontend Event              Backend Action                Database Update
────────────────────────────────────────────────────────────────────────────────────────────────────────────
Click [+ New Template]    →   Modal opens             →   (No backend call yet)

Fill form & click Create  →   AJAX POST               →   TemplateController          →  INSERT ChecklistTemplates
                              /TemplateBuilder/Create      .Create(dto)                   (TemplateId=1, IsActive=0)

                              Success response        ←   Return templateId

                              Redirect to Builder     →   Load Builder.cshtml         →  (No DB change)
                              /TemplateBuilder            with templateId=1
                              /Builder/1
```

### Flow 2: Adding a Section

```
User Action                    Frontend Event              Backend Action                Database Update
────────────────────────────────────────────────────────────────────────────────────────────────────────────
Click [+ Add Section]     →   Modal opens             →   (No backend call yet)

Fill form & click Add     →   AJAX POST               →   TemplateBuilderController   →  INSERT ChecklistSections
                              /TemplateBuilder/           .AddSection(templateId, dto)   (SectionId=1, TemplateId=1,
                              AddSection                                                  DisplayOrder=1)

                              Success response        ←   Return section object

Canvas updates            ←   Append section card     →   (No backend call)
                              to canvas
```

### Flow 3: Dragging a Field Type

```
User Action                    Frontend Event              Backend Action                Database Update
────────────────────────────────────────────────────────────────────────────────────────────────────────────
Start drag from toolbox   →   dragstart event         →   (No backend call)
                              Store field type

Drag over section         →   dragover event          →   (No backend call)
                              Highlight drop zone

Drop in section           →   drop event              →   (No backend call)
                              Modal opens with
                              pre-selected field type

Fill modal & click Add    →   AJAX POST               →   TemplateBuilderController   →  INSERT ChecklistItems
                              /TemplateBuilder/           .AddItem(sectionId, dto)       (ItemId=1, SectionId=1,
                              AddItem                                                     DataType="Number",
                                                                                          ValidationRules=JSON)

                              Success response        ←   Return item object

Canvas updates            ←   Append question item    →   (No backend call)
                              to section card
```

### Flow 4: Reordering Sections

```
User Action                    Frontend Event              Backend Action                Database Update
────────────────────────────────────────────────────────────────────────────────────────────────────────────
Drag section up/down      →   SortableJS handles      →   (No backend call yet)
                              DOM reordering

Drop section              →   onEnd callback          →   AJAX POST                   →  UPDATE ChecklistSections
                              Extract new order           /TemplateBuilder/              SET DisplayOrder=1 WHERE...
                              [sectionId: 3,2,1,4,5]      ReorderSections                SET DisplayOrder=2 WHERE...
                                                                                          SET DisplayOrder=3 WHERE...

                              Success response        ←   Return success
```

### Flow 5: Editing Properties (Live Update)

```
User Action                    Frontend Event              Backend Action                Database Update
────────────────────────────────────────────────────────────────────────────────────────────────────────────
Click question [⚙️]       →   Load properties panel   →   (No backend call)
                              Populate current values

Type in field             →   input event (debounced) →   AJAX PUT (after 1s pause)   →  UPDATE ChecklistItems
                              Wait 1 second               /TemplateBuilder/               SET ItemName='New text'
                                                          UpdateItem/{itemId}             WHERE ItemId=1

                              Success response        ←   Return updated item

                              Show "Saved" indicator  ←   (No backend call)
```

### Flow 6: Publishing Template

```
User Action                    Frontend Event              Backend Action                Database Update
────────────────────────────────────────────────────────────────────────────────────────────────────────────
Click [Publish]           →   Confirmation modal      →   (No backend call yet)
                              "Publish template?"

Confirm                   →   AJAX POST               →   TemplateBuilderController   →  UPDATE ChecklistTemplates
                              /TemplateBuilder/           .Publish(templateId)           SET IsActive=1
                              Publish/{templateId}                                        WHERE TemplateId=1

                                                          Run validations:
                                                          - At least 1 section?
                                                          - At least 3 questions?
                                                          - All questions have types?

                              Success response        ←   Return success

                              Show success message    ←   (No backend call)
                              "Template published!"

                              Redirect to list        →   Navigate to /TemplateBuilder
```

---

## Step-by-Step Implementation Guide

### Phase 1: Setup & Database (Day 1)

**Step 1.1: Create Database Migration**
- Create migration file for 5 tables
- Define all columns, constraints, indexes
- Include seed data for field types
- Test migration on dev database

**Step 1.2: Create Domain Entities**
- ChecklistTemplate.cs entity
- ChecklistSection.cs entity
- ChecklistItem.cs entity
- ChecklistSubmission.cs entity
- ChecklistResponse.cs entity
- Configure relationships in DbContext

**Step 1.3: Create Repositories**
- ITemplateRepository interface
- TemplateRepository implementation
- ISectionRepository interface
- SectionRepository implementation
- IItemRepository interface
- ItemRepository implementation
- Register in dependency injection

---

### Phase 2: Template List Page (Day 2)

**Step 2.1: Create Controller & Actions**
- TemplateBuilderController.cs
- Index() action - list all templates
- Create() GET action - show modal
- Create() POST action - save template
- Edit() action - redirect to builder
- Delete() action - soft delete
- Clone() action - duplicate template

**Step 2.2: Create Index View**
- Layout with header and action button
- Template cards with metadata
- Search and filter controls
- Pagination
- Action buttons per template

**Step 2.3: Create Template Modal**
- Bootstrap modal component
- Form with validation
- Template name input
- Template code input (auto-generated from name)
- Description textarea
- Frequency dropdown (Daily, Weekly, Monthly, Quarterly, Annual)
- Requires approval checkbox
- Applicable tenant types checkboxes

**Step 2.4: Wire Up JavaScript**
- Open modal on button click
- Form validation (client-side)
- AJAX POST on form submit
- Success: redirect to builder
- Error: show validation messages

---

### Phase 3: Builder Layout (Days 3-4)

**Step 3.1: Create Builder View**
- Three-column layout (Bootstrap grid)
- Fixed left panel (250px)
- Flexible center panel (remaining space)
- Fixed right panel (350px)
- Header with template name and action buttons
- Auto-save indicator in header

**Step 3.2: Left Panel - Toolbox**
- "Add Section" button at top
- Field types list below
- Each field type as draggable element
- Icons for each type (Font Awesome)
- Tooltips on hover

**Step 3.3: Center Panel - Canvas**
- Empty state message
- Section cards container
- Each section card collapsible (Bootstrap accordion)
- Question items within sections
- Add question buttons
- Drag-drop zones

**Step 3.4: Right Panel - Properties**
- Empty state message
- Section properties form (hidden initially)
- Question properties form (hidden initially)
- Show/hide based on selection
- Save/revert buttons

**Step 3.5: Install SortableJS**
- Download SortableJS library
- Add to wwwroot/lib/sortablejs/
- Reference in Builder.cshtml
- Initialize on page load

---

### Phase 4: Section Management (Day 5)

**Step 4.1: Add Section Modal**
- Bootstrap modal component
- Section name input (required)
- Description textarea
- Icon picker dropdown (with preview)
- Collapsible checkbox
- Collapsed by default checkbox
- Validation

**Step 4.2: Add Section Backend**
- AddSection() POST endpoint
- Validate section data
- Calculate DisplayOrder (max + 1)
- Insert into ChecklistSections table
- Return section object as JSON

**Step 4.3: Add Section Frontend**
- Open modal on button click
- Icon picker functionality
- Form submission (AJAX)
- Append section card to canvas on success
- Show error messages on failure

**Step 4.4: Section Card Component**
- Collapsible card (Bootstrap accordion)
- Drag handle (SortableJS)
- Section header with icon
- Edit button → loads properties panel
- Delete button → confirmation then AJAX DELETE
- Collapse/expand toggle
- Question items list
- Add question button

**Step 4.5: Drag-Drop Reordering (Sections)**
- Initialize SortableJS on sections container
- Configure drag handle
- Configure animation (150ms)
- onEnd callback → extract new order
- AJAX POST new order to backend
- Backend updates DisplayOrder for all sections

---

### Phase 5: Field Type Implementation (Days 6-8)

**Step 5.1: Add Question Modal Structure**
- Bootstrap modal component
- Tabbed interface (Basic, Validation, Pre-fill, Advanced)
- Common fields (all field types):
  - Question text input
  - Description textarea
  - Required checkbox
  - Read-only checkbox
  - Default value input
- Type-specific sections (shown conditionally)
- Save button
- Cancel button

**Step 5.2: Text Field Configuration**
- Max length input (default 255)
- Placeholder text input
- Regex pattern input
- Pattern description (help text)
- Examples of valid inputs

**Step 5.3: Number Field Configuration**
- Number type radio (Integer / Decimal)
- Min value input
- Max value input
- Step input (increment)
- Unit input (optional: %, KSh, GB, etc.)

**Step 5.4: Date Field Configuration**
- Date type dropdown (Date only / DateTime)
- Min date input
- Max date input
- Default to today checkbox
- Date format dropdown (dd/MM/yyyy, MM/dd/yyyy)

**Step 5.5: Boolean Field Configuration**
- Display style dropdown:
  - Checkbox
  - Yes/No radio buttons
  - Toggle switch
- Default value dropdown (Yes, No, Unset)
- Label for Yes option
- Label for No option

**Step 5.6: Dropdown Field Configuration**
- Options source radio:
  - Reference Data
  - Custom options
  - Database query
- If Reference Data selected:
  - Reference type dropdown (loads from ReferenceDataTypes)
- If Custom options selected:
  - Textarea for options (one per line)
- If Database query:
  - Table dropdown
  - Display column input
  - Value column input
- Allow multiple selections checkbox

**Step 5.7: TextArea Field Configuration**
- Rows input (default 4)
- Max length input
- Rich text editor checkbox (enable/disable)
- Placeholder text input

**Step 5.8: FileUpload Field Configuration**
- Allowed file types input (e.g., .pdf,.jpg,.png)
- Max file size input (MB)
- Multiple files checkbox
- Upload location dropdown (server path)

**Step 5.9: Add Question Backend**
- AddItem() POST endpoint
- Accept field type and all configuration
- Validate data (required fields, ranges, etc.)
- Build ValidationRules JSON
- Build ConditionalLogic JSON (if applicable)
- Calculate DisplayOrder within section
- Insert into ChecklistItems table
- Return item object as JSON

**Step 5.10: Add Question Frontend**
- Show modal on drag-drop or button click
- Pre-populate field type if dragged
- Show/hide type-specific sections
- Form validation (client-side)
- AJAX POST on save
- Append question item to section on success
- Show error messages on failure

**Step 5.11: Question Item Component**
- Card or list item style
- Drag handle (for reordering within section)
- Question number (Q1, Q2, etc.)
- Question text
- Type badge with color coding
- Metadata (required, validation summary)
- Pre-fill indicator (if configured)
- Edit button → loads properties panel
- Delete button → confirmation then AJAX DELETE

**Step 5.12: Drag-Drop Reordering (Questions)**
- Initialize SortableJS on each section's question list
- Configure group name (allows moving between sections)
- Configure drag handle
- onEnd callback → extract new order and section
- AJAX POST new order to backend
- Backend updates DisplayOrder and SectionId

---

### Phase 6: Properties Panel (Day 9)

**Step 6.1: Selection Handling**
- Click section → highlight section
- Click question → highlight question
- Show appropriate properties form
- Hide other forms

**Step 6.2: Section Properties Form**
- Bind to selected section data
- Section name input (editable)
- Description textarea (editable)
- Icon picker (editable)
- Collapsible checkbox (editable)
- Collapsed by default checkbox (editable)
- Display order input (editable)
- Question count (read-only)
- Save button → AJAX PUT
- Revert button → reload original values

**Step 6.3: Question Properties Form**
- Bind to selected question data
- Field type dropdown (can change type)
- Question text input (editable)
- Description textarea (editable)
- Required checkbox (editable)
- Read-only checkbox (editable)
- Default value input (editable)
- Type-specific configuration (editable)
- Save button → AJAX PUT
- Revert button → reload original values

**Step 6.4: Live Updates**
- Debounce input events (wait 1 second after typing stops)
- Auto-save changes via AJAX PUT
- Show "Saving..." indicator
- Show "Saved ✓" indicator on success
- Update canvas immediately on successful save
- Show error messages on failure

---

### Phase 7: Validation Rules Builder (Day 10)

**Step 7.1: Validation Rules UI**
- Collapsible section in properties panel
- Add rule button
- Rule list (can have multiple rules)
- Each rule shows:
  - Rule type dropdown
  - Rule parameters (depends on type)
  - Delete rule button

**Step 7.2: Rule Types**
- **Required** - no parameters
- **Min Length** - length input
- **Max Length** - length input
- **Min Value** - value input (numbers/dates)
- **Max Value** - value input (numbers/dates)
- **Regex Pattern** - pattern input + description
- **Email Format** - no parameters
- **Phone Format** - no parameters
- **Custom Function** - JavaScript function input (advanced)

**Step 7.3: Rule Builder Logic**
- Add rule → append to list
- Delete rule → remove from list
- Save → serialize rules to JSON
- Store JSON in ChecklistItems.ValidationRules
- Load → deserialize JSON to populate UI

---

### Phase 8: Conditional Logic Builder (Day 11)

**Step 8.1: Conditional Logic UI**
- Collapsible section in properties panel
- "Show this field conditionally" checkbox
- If checked, show condition builder
- Add condition button
- Condition list (can have multiple conditions, AND logic)

**Step 8.2: Condition Builder**
- Each condition has:
  - If [Question dropdown]
  - Operator dropdown (equals, not equals, greater than, less than, contains)
  - Value input (depends on question type)
  - Delete condition button

**Step 8.3: Operator Options by Field Type**
- **Text:** equals, not equals, contains, starts with, ends with
- **Number:** equals, not equals, greater than, less than, between
- **Date:** equals, not equals, before, after, between
- **Boolean:** equals (Yes/No)
- **Dropdown:** equals, not equals, one of

**Step 8.4: Condition Logic**
- Add condition → append to list
- Delete condition → remove from list
- Save → serialize conditions to JSON
- Store JSON in ChecklistItems.ConditionalLogic
- Load → deserialize JSON to populate UI

---

### Phase 9: Preview Functionality (Day 12)

**Step 9.1: Preview Modal**
- Large modal (fullscreen or 90% width)
- Load full template with sections and items
- Render form exactly as users will see it
- Use FieldRendererService (same as actual forms)
- Enable validation (test it works)
- Disable submission (preview only)

**Step 9.2: Preview Button**
- Top-right corner of builder
- Click → AJAX GET template data
- Pass to preview modal
- Modal renders form
- User can test filling it out
- Validation triggers on invalid input
- Close button returns to builder

---

### Phase 10: Publish & Validation (Day 13)

**Step 10.1: Publish Validation**
- Check at least 1 section exists
- Check at least 3 questions exist
- Check all questions have valid types
- Check all questions have question text
- Check all required validation rules are set
- Check no circular conditional logic
- Show detailed error messages if validation fails

**Step 10.2: Publish Flow**
- Click Publish button
- Run validations (frontend)
- If pass, show confirmation modal
- Confirm → AJAX POST to Publish endpoint
- Backend runs additional validations
- If pass, set IsActive = 1
- Return success
- Show success message
- Redirect to template list or stay in builder

**Step 10.3: Unpublish Flow**
- Only available for already-published templates
- Click Unpublish button
- Confirmation modal (warn about impact)
- Confirm → AJAX POST to Unpublish endpoint
- Backend sets IsActive = 0
- Return success
- Show success message

---

## Field Type Configurations

### Field Type Matrix

| Field Type | Configuration Options | Validation Rules | Pre-fill Options | Complexity |
|------------|----------------------|------------------|------------------|------------|
| Text | Max length, Placeholder, Regex | Required, MinLength, MaxLength, Pattern | Last value, Custom query | ⭐⭐ |
| Number | Min, Max, Decimals, Step, Unit | Required, Min, Max, Range | Hardware count, Software count, Ticket count | ⭐⭐ |
| Date | Min, Max, Default to today | Required, MinDate, MaxDate, DateRange | Last backup, Last update | ⭐⭐ |
| Boolean | Display style, Default value | Required | Status checks (Y/N) | ⭐ |
| Dropdown | Source (Ref/Custom/Query), Options | Required, AllowMultiple | Reference data, Last value | ⭐⭐⭐ |
| TextArea | Rows, Max length, Rich text | Required, MaxLength | Last comments, Common responses | ⭐⭐ |
| FileUpload | Allowed types, Max size, Multiple | Required, FileType, FileSize | Previous attachments | ⭐⭐⭐ |

### Detailed Configuration: Dropdown Field

**Option 1: Reference Data Source**
```
┌────────────────────────────────────────────────────┐
│ Options Source: ● Reference Data                  │
│                                                    │
│ Reference Type: [WAN_TYPE ▼]                      │
│                                                    │
│ Preview Options:                                   │
│ • Fiber Optic                                      │
│ • Microwave                                        │
│ • Hybrid Connection                                │
│ • Microwave-Viable on last mile Fibre             │
│                                                    │
│ [✓] Load options dynamically (allows updates)     │
│ [☐] Allow multiple selections                     │
└────────────────────────────────────────────────────┘
```

**Option 2: Custom Options**
```
┌────────────────────────────────────────────────────┐
│ Options Source: ● Custom Options                  │
│                                                    │
│ Options (one per line):                            │
│ [Excellent______________________________]          │
│ [Good___________________________________]          │
│ [Fair___________________________________]          │
│ [Poor___________________________________]          │
│                                                    │
│ [☐] Allow multiple selections                     │
└────────────────────────────────────────────────────┘
```

**Option 3: Database Query (Advanced)**
```
┌────────────────────────────────────────────────────┐
│ Options Source: ● Database Query                  │
│                                                    │
│ Table:         [SoftwareProducts ▼]               │
│ Display Field: [ProductName ▼]                    │
│ Value Field:   [ProductCode ▼]                    │
│                                                    │
│ Filter (WHERE clause):                             │
│ [IsActive = 1_____________________________]        │
│                                                    │
│ Order By:      [ProductName ▼] [ASC ▼]           │
│                                                    │
│ Preview: (shows first 5 options)                   │
│                                                    │
│ [☐] Allow multiple selections                     │
└────────────────────────────────────────────────────┘
```

---

## Validation Rules Builder

### Visual Builder

```
┌────────────────────────────────────────────────────────────────┐
│ VALIDATION RULES                                               │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ [+ Add Validation Rule]                                        │
│                                                                │
│ ┌──────────────────────────────────────────────────────────┐ │
│ │ Rule 1: Required                                   [🗑️]  │ │
│ │ This field must be filled before submission               │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                                │
│ ┌──────────────────────────────────────────────────────────┐ │
│ │ Rule 2: Range (Number)                             [🗑️]  │ │
│ │ Min Value: [1______]  Max Value: [500____]                │ │
│ │ Error message: "Value must be between 1 and 500"          │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                                │
│ ┌──────────────────────────────────────────────────────────┐ │
│ │ Rule 3: Custom (Advanced)                          [🗑️]  │ │
│ │ Validation Function:                                       │ │
│ │ [function(value) {                                ]        │ │
│ │ [  return value > 0 && value <= 1000;            ]        │ │
│ │ [}                                               ]        │ │
│ │ Error message: [Must be positive and ≤1000_____]          │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### Add Rule Modal

```
┌──────────────────────────────────────────────────────┐
│ Add Validation Rule                          [✕]     │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Rule Type: *                                         │
│ [Range (Min/Max) ▼]                                 │
│                                                      │
│ Options:                                             │
│ • Required                                           │
│ • Min Length / Max Length                            │
│ • Min Value / Max Value / Range                      │
│ • Regex Pattern                                      │
│ • Email Format                                       │
│ • Phone Format                                       │
│ • Custom Function                                    │
│                                                      │
│ ──────────────────────────────────────────────────  │
│ Rule Configuration (shown based on type):            │
│ ──────────────────────────────────────────────────  │
│                                                      │
│ Min Value: [1______]                                │
│ Max Value: [500____]                                │
│                                                      │
│ Error Message:                                       │
│ [Value must be between 1 and 500_____________]       │
│                                                      │
│                      [Cancel]  [Add Rule]           │
└──────────────────────────────────────────────────────┘
```

---

## Conditional Logic Builder

### Visual Builder

```
┌────────────────────────────────────────────────────────────────┐
│ CONDITIONAL LOGIC                                              │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ [☐] Show this field conditionally                             │
│                                                                │
│ When checked, this section appears:                            │
│                                                                │
│ ┌──────────────────────────────────────────────────────────┐ │
│ │ Show this field when ALL of the following are true:     │ │
│ │                                                          │ │
│ │ [+ Add Condition]                                        │ │
│ │                                                          │ │
│ │ ┌────────────────────────────────────────────────────┐ │ │
│ │ │ Condition 1:                               [🗑️]    │ │ │
│ │ │                                                    │ │ │
│ │ │ If  [Any hardware failures? ▼]                    │ │ │
│ │ │ is  [equals ▼]                                    │ │ │
│ │ │     [Yes ▼]                                        │ │ │
│ │ └────────────────────────────────────────────────────┘ │ │
│ │                                                          │ │
│ │ ┌────────────────────────────────────────────────────┐ │ │
│ │ │ Condition 2:                               [🗑️]    │ │ │
│ │ │                                                    │ │ │
│ │ │ If  [Total computers ▼]                           │ │ │
│ │ │ is  [greater than ▼]                              │ │ │
│ │ │     [10________]                                   │ │ │
│ │ └────────────────────────────────────────────────────┘ │ │
│ │                                                          │ │
│ └──────────────────────────────────────────────────────────┘ │
│                                                                │
│ Logic: If (Condition 1 AND Condition 2), show this field      │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### Add Condition Modal

```
┌──────────────────────────────────────────────────────┐
│ Add Condition                                [✕]     │
├──────────────────────────────────────────────────────┤
│                                                      │
│ Show this field when:                                │
│                                                      │
│ If  [Question dropdown_______________▼]             │
│                                                      │
│ Available questions:                                 │
│ • Q1: Total number of computers                      │
│ • Q2: Operational computers                          │
│ • Q3: Any hardware failures?                         │
│ • Q4: EWS Version                                    │
│ • (shows all previous questions)                     │
│                                                      │
│ is  [Operator dropdown_______________▼]             │
│                                                      │
│ Available operators (depends on question type):      │
│ • equals                                             │
│ • not equals                                         │
│ • greater than                                       │
│ • less than                                          │
│ • between                                            │
│ • contains                                           │
│ • is empty                                           │
│ • is not empty                                       │
│                                                      │
│ Value: [___________________________]                │
│ (Input type depends on question type)                │
│                                                      │
│                      [Cancel]  [Add Condition]      │
└──────────────────────────────────────────────────────┘
```

---

## Testing Checklist

### Unit Testing

- [ ] Template CRUD operations
- [ ] Section CRUD operations
- [ ] Item CRUD operations
- [ ] Validation rules serialization/deserialization
- [ ] Conditional logic serialization/deserialization
- [ ] DisplayOrder calculation
- [ ] Template validation before publish

### Integration Testing

- [ ] Create template → Add sections → Add items → Preview → Publish
- [ ] Drag-drop section reordering persists to database
- [ ] Drag-drop question reordering persists to database
- [ ] Drag question between sections updates SectionId
- [ ] Delete section cascades to items
- [ ] Clone template duplicates all sections and items
- [ ] Version increment on significant changes

### UI Testing

- [ ] Three-panel layout responsive on different screen sizes
- [ ] Drag-drop works smoothly (SortableJS)
- [ ] Modals open and close correctly
- [ ] Form validation prevents invalid data
- [ ] Properties panel updates live
- [ ] Preview modal shows accurate form rendering
- [ ] Icon picker displays correctly
- [ ] Tooltips show on hover

### Browser Compatibility

- [ ] Chrome (latest)
- [ ] Edge (latest)
- [ ] Firefox (latest)
- [ ] Safari (if Mac available)

### Performance Testing

- [ ] Builder loads in < 2 seconds with 50 questions
- [ ] Drag-drop is smooth (no lag)
- [ ] Auto-save completes in < 500ms
- [ ] Preview modal loads in < 1 second

---

**Document Version:** 1.0
**Last Updated:** October 30, 2025
**Maintained By:** KTDA ICT Development Team
**Next Steps:** Proceed to [FormRendering_Implementation.md](FormRendering_Implementation.md)
