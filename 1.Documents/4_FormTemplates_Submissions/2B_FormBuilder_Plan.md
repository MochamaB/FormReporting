# Form Builder Implementation Plan

**Project:** FormReporting - SurveyJS-Style Form Builder  
**Date:** November 20, 2025  
**Status:** Planning Phase  

---

## 📋 Executive Summary

### Purpose
Build a drag-and-drop form builder interface (similar to SurveyJS) for creating form templates with sections, fields, validation rules, and conditional logic.

### Key Features
- Three-panel layout (Toolbox, Canvas, Properties)
- Drag & drop field creation
- Real-time validation & conditional logic
- Auto-save every 30 seconds
- Undo/redo functionality

### Related Documents
- 📘 **[Add Field Implementation Guide](./2B_FormBuilder_AddField.md)** - Detailed drag-drop and modal implementation
- 📗 **[Form Builder Structure](./2B_FormBuilder_Structure.md)** - Business logic and requirements

---

## 📊 Data Model Overview

### Entity Hierarchy
```
FormTemplate (TemplateId, TemplateName, TemplateCode, Version, PublishStatus)
├── FormTemplateSection (SectionId, SectionName, DisplayOrder, IsCollapsible)
│   └── FormTemplateItem (ItemId, ItemCode, ItemName, DataType, IsRequired, DisplayOrder)
│       ├── FormItemValidation (ValidationType, MinValue, MaxValue, ErrorMessage)
│       ├── FormItemOption (OptionValue, OptionLabel, DisplayOrder)
│       └── FormItemConfiguration (ConfigKey, ConfigValue)
```

### Supported Field Types
1. **Text** - Single-line input
2. **TextArea** - Multi-line text
3. **Number** - Numeric with min/max
4. **Date** - Date picker
5. **Boolean** - Checkbox/toggle
6. **Dropdown** - Single-select
7. **RadioButton** - Radio group
8. **MultiSelect** - Multi-select
9. **Rating** - Star rating
10. **FileUpload** - File attachment

---

## 🏗️ Architecture

### Three-Panel Layout
```
┌────────────────────────────────────────────────────────┐
│  LEFT (250px)  │  CENTER (Fluid)  │  RIGHT (350px)    │
│  TOOLBOX       │  CANVAS          │  PROPERTIES       │
│  [Sections]    │  ┌─Section 1──┐  │  [Basic Tab]      │
│  [Fields]      │  │ Field 1    │  │  [Validation]     │
│                │  │ Field 2    │  │  [Logic]          │
│  Drag →        │  └────────────┘  │  [Advanced]       │
└────────────────────────────────────────────────────────┘
```

### File Structure
```
Backend:
  Services/Forms/
    ├── FormBuilderService.cs
    ├── SectionService.cs
    ├── FieldService.cs
    ├── ValidationService.cs
    └── AutosaveService.cs
    
  Controllers/API/
    └── FormBuilderApiController.cs
    
  ViewModels/Forms/
    ├── FormBuilderViewModel.cs
    ├── SectionDto.cs
    └── FieldDto.cs

Frontend:
  Views/Forms/FormTemplates/
    ├── FormBuilder.cshtml
    └── Partials/FormBuilder/
        ├── _ToolboxPanel.cshtml
        ├── _CanvasPanel.cshtml
        └── _PropertiesPanel.cshtml
        
  wwwroot/js/form-builder/
    ├── core/
    │   ├── form-builder.js
    │   ├── state-manager.js
    │   └── autosave.js
    ├── panels/
    │   ├── toolbox-manager.js
    │   ├── canvas-manager.js
    │   └── properties-manager.js
    └── utils/
        ├── api-client.js
        └── drag-drop-handler.js
        
  wwwroot/css/form-builder/
    ├── form-builder.css
    └── panels/
        ├── toolbox.css
        ├── canvas.css
        └── properties.css
```

---

## 🚀 Implementation Phases

### PHASE 1: Foundation (Days 1-5)
**Goal:** Create infrastructure & basic UI

#### Day 1: Backend Setup
- [ ] Create `IFormBuilderService` interface
- [ ] Implement `FormBuilderService.LoadForBuilderAsync()`
- [ ] Create ViewModels: `FormBuilderViewModel`, `SectionDto`, `FieldDto`
- [ ] Register service in `Program.cs`

#### Day 2: API Endpoints - Sections
- [ ] Create `FormBuilderApiController`
- [ ] `POST /api/formbuilder/{templateId}/sections` - Add section
- [ ] `PUT /api/formbuilder/{templateId}/sections/{id}` - Edit section
- [ ] `DELETE /api/formbuilder/{templateId}/sections/{id}` - Delete section
- [ ] `PUT /api/formbuilder/{templateId}/sections/reorder` - Reorder sections

#### Day 3: Three-Panel Layout
- [ ] Create `FormBuilder.cshtml` main view
- [ ] Create `_ToolboxPanel.cshtml` (left)
- [ ] Create `_CanvasPanel.cshtml` (center)
- [ ] Create `_PropertiesPanel.cshtml` (right)
- [ ] Create `form-builder.css` with grid layout

#### Day 4: JavaScript Foundation
- [ ] Create `form-builder.js` (main orchestrator)
- [ ] Create `state-manager.js` (Redux-style state)
- [ ] Create `event-bus.js` (pub/sub events)
- [ ] Create `api-client.js` (AJAX wrapper)

#### Day 5: Section CRUD UI
- [ ] Render sections in canvas
- [ ] Add section modal
- [ ] Edit section modal
- [ ] Delete section with confirmation
- [ ] Test all section operations

**Milestone 1:** Can add, edit, delete, and view sections ✅

---

### PHASE 2: Core Builder (Days 6-10)
**Goal:** Field management & drag-drop

**📖 Implementation Guide:** See [2B_FormBuilder_AddField.md](./2B_FormBuilder_AddField.md) for detailed drag-drop and modal implementation

#### Day 6: API Endpoints - Fields
- [ ] `POST /api/formbuilder/{templateId}/fields` - Add field
- [ ] `PUT /api/formbuilder/{templateId}/fields/{id}` - Edit field
- [ ] `DELETE /api/formbuilder/{templateId}/fields/{id}` - Delete field
- [ ] `PUT /api/formbuilder/{templateId}/fields/reorder` - Reorder fields

#### Day 7: Field Palette
- [ ] Create `_FieldPalette.cshtml` with field types
- [ ] Style field type cards with icons
- [ ] Implement search/filter
- [ ] Make fields draggable (Sortable.js)

#### Day 8: Canvas Field Rendering
- [ ] Create `_FieldCard.cshtml` component
- [ ] Render fields in sections
- [ ] Implement drop zones
- [ ] Visual indicators (required, validated)

#### Day 9: Drag & Drop
- [ ] Initialize Sortable.js for field palette
- [ ] Handle drag from palette to canvas
- [ ] Handle drag within section (reorder)
- [ ] Handle drag between sections
- [ ] Update display orders via API

#### Day 10: Properties Panel - Basic Tab
- [ ] Show/hide panel on selection
- [ ] Create `_FieldPropertiesBasic.cshtml`
- [ ] Form inputs: Name, Code, Type, Required
- [ ] Save button → API call
- [ ] Update canvas on save

**Milestone 2:** Can drag fields, edit properties ✅

---

### PHASE 3: Advanced Features (Days 11-15)
**Goal:** Validation & conditional logic

#### Day 11: Validation Rules UI
- [ ] Create `_FieldPropertiesValidation.cshtml` tab
- [ ] [+ Add Rule] button
- [ ] Validation type dropdown
- [ ] Min/Max inputs (conditional on type)
- [ ] Error message input
- [ ] List existing rules

#### Day 12: Validation API
- [ ] `POST /api/formbuilder/fields/{itemId}/validations` - Add rule
- [ ] `PUT /api/formbuilder/fields/{itemId}/validations/{id}` - Edit rule
- [ ] `DELETE /api/formbuilder/fields/{itemId}/validations/{id}` - Delete rule
- [ ] Load/save validation rules

#### Day 13: Conditional Logic UI
- [ ] Create `_FieldPropertiesLogic.cshtml` tab
- [ ] Action selector (Show/Hide/Enable/Disable)
- [ ] Logic operator (AND/OR)
- [ ] [+ Add Rule] button
- [ ] Rule builder: Field, Operator, Value
- [ ] Preview/test logic button

#### Day 14: Conditional Logic API
- [ ] `PUT /api/formbuilder/fields/{itemId}/conditional-logic` - Save logic
- [ ] Parse JSON structure
- [ ] Validate source field references
- [ ] Save to `ConditionalLogic` column

#### Day 15: Field Options (Dropdowns)
- [ ] Create options manager UI
- [ ] Add/edit/delete options
- [ ] Reorder options (drag-drop)
- [ ] Mark default option
- [ ] `POST/PUT/DELETE /api/formbuilder/fields/{itemId}/options`

**Milestone 3:** Full validation & logic working ✅

---

### PHASE 4: Polish & Optimization (Days 16-20)
**Goal:** UX enhancements & performance

#### Day 16: Autosave
- [ ] Create `autosave.js` module
- [ ] Track dirty state
- [ ] Auto-save every 30 seconds
- [ ] Save indicator in UI
- [ ] Handle save failures gracefully

#### Day 17: Undo/Redo
- [ ] Create `undo-redo.js` module
- [ ] Implement history stack
- [ ] Undo button (Ctrl+Z)
- [ ] Redo button (Ctrl+Y)
- [ ] Show current history position

#### Day 18: Copy/Duplicate
- [ ] Copy section button
- [ ] Duplicate field button
- [ ] Copy includes validations & options
- [ ] Auto-increment codes

#### Day 19: Keyboard Shortcuts
- [ ] Ctrl+S - Save
- [ ] Ctrl+Z - Undo
- [ ] Ctrl+Y - Redo
- [ ] Del - Delete selected
- [ ] Esc - Close properties panel
- [ ] Document shortcuts

#### Day 20: Performance & Testing
- [ ] Optimize rendering for 50+ fields
- [ ] Debounce API calls
- [ ] Add loading indicators
- [ ] Browser testing (Chrome, Edge, Firefox)
- [ ] Mobile responsive adjustments

**Milestone 4:** Production-ready ✅

---

## 📡 API Endpoints Reference

### Sections
```
POST   /api/formbuilder/{templateId}/sections
PUT    /api/formbuilder/{templateId}/sections/{id}
DELETE /api/formbuilder/{templateId}/sections/{id}
PUT    /api/formbuilder/{templateId}/sections/reorder
```

### Fields
```
POST   /api/formbuilder/{templateId}/fields
PUT    /api/formbuilder/{templateId}/fields/{id}
DELETE /api/formbuilder/{templateId}/fields/{id}
PUT    /api/formbuilder/{templateId}/fields/reorder
```

### Validations
```
POST   /api/formbuilder/fields/{itemId}/validations
PUT    /api/formbuilder/fields/{itemId}/validations/{id}
DELETE /api/formbuilder/fields/{itemId}/validations/{id}
```

### Options
```
POST   /api/formbuilder/fields/{itemId}/options
PUT    /api/formbuilder/fields/{itemId}/options/{id}
DELETE /api/formbuilder/fields/{itemId}/options/{id}
PUT    /api/formbuilder/fields/{itemId}/options/reorder
```

### Conditional Logic
```
PUT    /api/formbuilder/fields/{itemId}/conditional-logic
DELETE /api/formbuilder/fields/{itemId}/conditional-logic
```

### Autosave
```
POST   /api/formbuilder/{templateId}/autosave
```

---

## 💾 Autosave Implementation

### JavaScript (autosave.js)
```javascript
class AutosaveManager {
    constructor(templateId) {
        this.templateId = templateId;
        this.isDirty = false;
        this.saveTimer = null;
        this.INTERVAL = 30000; // 30 seconds
    }
    
    markDirty() {
        this.isDirty = true;
        this.scheduleSave();
    }
    
    scheduleSave() {
        clearTimeout(this.saveTimer);
        this.saveTimer = setTimeout(() => {
            if (this.isDirty) this.save();
        }, this.INTERVAL);
    }
    
    async save() {
        const state = StateManager.getState();
        const response = await ApiClient.post(
            `/api/formbuilder/${this.templateId}/autosave`,
            state
        );
        if (response.ok) {
            this.isDirty = false;
            this.showIndicator('Saved');
        }
    }
}
```

---

## ✅ Testing Checklist

### Section Operations
- [ ] Add section with valid name
- [ ] Edit section name and description
- [ ] Delete empty section
- [ ] Delete section with fields (confirmation)
- [ ] Reorder sections by dragging
- [ ] Duplicate section

### Field Operations
- [ ] Drag Text field from palette to section
- [ ] Drag Number field
- [ ] Drag Dropdown field
- [ ] Edit field properties (name, code, required)
- [ ] Delete field
- [ ] Reorder fields by dragging
- [ ] Move field between sections
- [ ] Duplicate field

### Validation Rules
- [ ] Add Required validation
- [ ] Add Min/Max Length validation
- [ ] Add Email/Phone validation
- [ ] Add Regex pattern validation
- [ ] Edit validation rule
- [ ] Delete validation rule
- [ ] Multiple rules on one field

### Conditional Logic
- [ ] Show field if another = "Yes"
- [ ] Hide field if number > 10
- [ ] Enable field with AND logic
- [ ] Disable field with OR logic
- [ ] Test logic preview

### Field Options (Dropdown)
- [ ] Add option to dropdown
- [ ] Edit option label
- [ ] Delete option
- [ ] Reorder options
- [ ] Mark default option
- [ ] Cascading dropdown (parent-child)

### Autosave
- [ ] Auto-saves after 30 seconds of inactivity
- [ ] Save indicator shows "Saving..."
- [ ] Save indicator shows "Saved"
- [ ] Handles save failure gracefully
- [ ] Manual save button works

### Performance
- [ ] Loads 50 fields without lag
- [ ] Drag-drop is smooth
- [ ] API calls are debounced
- [ ] No memory leaks
- [ ] Works in Chrome, Edge, Firefox

---

## 📚 Third-Party Libraries

### Required
1. **Sortable.js** - Drag & drop functionality
2. **SweetAlert2** - Modals and alerts
3. **Flatpickr** - Date picker (for date fields)

### Optional
4. **Monaco Editor** - For custom validation expressions
5. **JSONEditor** - For conditional logic JSON editing

---

## 🎯 Success Metrics

### Functional
- All CRUD operations work correctly
- Drag-drop is intuitive
- Auto-save prevents data loss
- Validation rules execute properly
- Conditional logic works as expected

### Performance
- Page load < 2 seconds
- Field rendering < 100ms
- API response < 500ms
- Supports 100+ fields

### UX
- No more than 2 clicks to add field
- Properties panel updates instantly
- Clear visual feedback for all actions
- Error messages are helpful

---

## 📝 Notes

### Design Decisions
- Use Redux-style state management for consistency
- Single API controller for all builder operations
- JSON storage for conditional logic (flexible schema)
- Autosave to prevent data loss

### Future Enhancements
- Field library (reusable fields)
- Template import/export
- Section routing (skip logic)
- Calculated fields
- Matrix/Grid layout
- Mobile app support

---

**Document Version:** 1.0  
**Last Updated:** November 20, 2025  
**Next Review:** After Phase 1 completion
