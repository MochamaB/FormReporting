# 📊 Field Options Management - Implementation Guide

**Document:** 2b_FormBuilder_FieldOptions
**Date:** November 27, 2025
**Phase:** Phase 2 - Field Options Management
**Status:** Ready for Implementation

---

## 1. Overview

### What Are Options?
Options are the selectable choices for dropdown menus, radio buttons, checkboxes, and multi-select fields. Each option has a label (user-facing) and a value (system identifier).

### Field Types Requiring Options

| Field Type | Selection Mode | Example Use |
|------------|----------------|-------------|
| Dropdown | Single | Country selector |
| Radio | Single | Gender choice |
| Checkbox | Multiple | Interests/hobbies |
| MultiSelect | Multiple | Skills selection |

### Current State
- ✅ Auto-creates 3 default options when field is added
- ✅ Preserves options when field type changes
- ✅ Displays options in canvas preview
- ❌ **No UI to manage options manually**

---

## 2. Data Structure

### FormItemOption Entity
**Table:** `FormItemOptions`

| Field | Purpose | Example |
|-------|---------|---------|
| OptionId | Primary key | 1, 2, 3... |
| ItemId | Parent field ID | 45 |
| OptionLabel | User-facing text | "Red", "Blue" |
| OptionValue | System identifier | "red", "blue" |
| DisplayOrder | Sort position | 1, 2, 3... |
| IsDefault | Pre-selected? | true/false |
| IsActive | Enabled? | true/false |
| ParentOptionId | For cascading (Phase 3) | null |

### Key Features
- **Value vs Label:** Separate display text from system value
- **Auto-generation:** Values auto-created from labels ("Red Color" → "red_color")
- **Display Order:** Drag-drop reordering supported
- **Default Selection:** Single-select allows 1 default, multi-select allows multiple
- **Data Preservation:** Options kept when field type changes

---

## 3. What's Missing (What We're Building)

### Current Limitations
1. Cannot add new options manually
2. Cannot edit option labels/values
3. Cannot delete options
4. Cannot reorder options
5. Cannot set default selections
6. No UI in properties panel

### Required Features
1. **Add Option:** Click "+ Add Option" to create new choice
2. **Edit Option:** Inline editing of label and value
3. **Delete Option:** Remove option (minimum 2 required)
4. **Reorder Options:** Drag-drop to change order
5. **Set Default:** Checkbox to mark pre-selected options
6. **Smart Validation:** Unique values, minimum options, auto-generation

---

## 4. Architecture

### API Controller
**Location:** `Controllers/API/FormBuilderApiController.cs`
**Note:** Uses RESTful architecture (consolidated from FormTemplatesController)

### Service Layer
**Interface:** `Services/Forms/IFormBuilderService.cs`
**Implementation:** `Services/Forms/FormBuilderService.cs`

### Frontend
**UI:** `Views/Forms/FormTemplates/Partials/FormBuilder/Properties/_PropertiesConfiguration.cshtml` (Configuration Tab)
**JavaScript:** `wwwroot/assets/js/pages/form-builder-properties.js`
**Note:** Options belong in Configuration tab. Advanced tab is for conditional logic.

---

## 5. Implementation Steps

### Phase 1: Backend API (2 hours)

#### Step 1.1: Add DTO Models
**File:** `Models/ViewModels/Forms/FieldDto.cs`

Add new DTO class after existing `FieldOptionDto`:
- `ReorderOptionDto` with properties: OptionId, DisplayOrder
- Consider adding `IsActive` to `FieldOptionDto` if not present

#### Step 1.2: Add Service Interface Methods
**File:** `Services/Forms/IFormBuilderService.cs`

Add method signatures:
1. `Task<FieldOptionDto?> AddOptionAsync(int fieldId, FieldOptionDto dto)`
2. `Task<bool> UpdateOptionAsync(int optionId, FieldOptionDto dto)`
3. `Task<bool> DeleteOptionAsync(int optionId)`
4. `Task<bool> ReorderOptionsAsync(int fieldId, List<ReorderOptionDto> updates)`
5. `Task<bool> SetDefaultOptionAsync(int optionId, int fieldId)`

#### Step 1.3: Implement Service Methods
**File:** `Services/Forms/FormBuilderService.cs`

**AddOptionAsync:**
- Verify field exists and requires options
- Auto-generate value from label if empty (lowercase, underscores)
- Ensure unique value (append counter if duplicate)
- Set DisplayOrder to max + 1
- Create option in database
- Return DTO with new OptionId

**UpdateOptionAsync:**
- Find option by OptionId
- Validate unique value within field
- Update OptionLabel, OptionValue
- Save changes

**DeleteOptionAsync:**
- Count options for parent field
- Enforce minimum 2 options rule (throw exception if deleting would leave <2)
- Remove option from database

**ReorderOptionsAsync:**
- Loop through updates array
- Update DisplayOrder for each option
- Save all changes in one transaction

**SetDefaultOptionAsync:**
- Load field with all options
- Check if single-select (Dropdown/Radio) or multi-select (Checkbox/MultiSelect)
- **Single-select:** Unset all other defaults, set only this one
- **Multi-select:** Toggle this option's IsDefault
- Save changes

#### Step 1.4: Add API Endpoints
**File:** `Controllers/API/FormBuilderApiController.cs`

Add RESTful endpoints:

| HTTP Method | Route | Action | Service Call |
|-------------|-------|--------|--------------|
| POST | `/api/formbuilder/fields/{fieldId}/options` | Add option | AddOptionAsync |
| PUT | `/api/formbuilder/options/{optionId}` | Update option | UpdateOptionAsync |
| DELETE | `/api/formbuilder/options/{optionId}` | Delete option | DeleteOptionAsync |
| PUT | `/api/formbuilder/fields/{fieldId}/options/reorder` | Reorder options | ReorderOptionsAsync |
| PATCH | `/api/formbuilder/options/{optionId}/default` | Set default | SetDefaultOptionAsync |

Each endpoint should:
- Accept proper parameters
- Call service method
- Return JSON result: `{ success: true/false, message: "...", data: {...} }`
- Handle exceptions with 500 status

---

### Phase 2: Frontend UI (1 hour)

#### Step 2.1: Update Configuration Tab View
**File:** `Views/Forms/FormTemplates/Partials/FormBuilder/Properties/_PropertiesConfiguration.cshtml`

Add options manager section for **fields only** (hidden by default):

**Structure:**
1. Container div `#field-options-manager` (display: none initially)
2. Header with label "Field Options"
3. Sortable list container `#options-list` with class `sortable-options`
4. Option item template (repeating):
   - Drag handle icon
   - Label input (text, onblur="updateOption()")
   - Value input (text, onblur="updateOption()")
   - Default checkbox (onchange="setDefaultOption()")
   - Delete button (onclick="deleteOption()")
5. "+ Add Option" button (btn-soft-success)
6. Helper text: "Drag to reorder. Min 2 options required."

**Visibility Logic:**
- Show only when field type is Dropdown/Radio/Checkbox/MultiSelect
- Hide for all other field types
- Hide when section is selected

---

### Phase 3: Frontend JavaScript (2 hours)

#### Step 3.1: Load Options
**File:** `form-builder-properties.js`

Add method `loadFieldOptions(fieldId)`:
1. Options already loaded with field data (no separate API call needed)
2. Get field data from `this.currentFieldData.options`
3. Clear existing options list
4. Loop through options, create option item HTML for each
5. Append to `#options-list`
6. Initialize SortableJS on `#options-list`
7. Show/hide options manager based on field type

#### Step 3.2: Add Option
Add method `addOption()`:
1. Get current field ID
2. Create new option row in UI with empty inputs
3. Focus on label input
4. On blur, call API: `POST /api/formbuilder/fields/{fieldId}/options`
5. Update row with returned OptionId
6. Refresh canvas preview

#### Step 3.3: Update Option
Add method `updateOption(optionId)`:
1. Get label and value from input fields
2. Validate not empty
3. Call API: `PUT /api/formbuilder/options/{optionId}`
4. Show success/error feedback
5. Refresh canvas preview

#### Step 3.4: Delete Option
Add method `deleteOption(optionId)`:
1. Show confirmation dialog
2. Call API: `DELETE /api/formbuilder/options/{optionId}`
3. If success, remove row from UI
4. If error (minimum 2 options), show error message
5. Refresh canvas preview

#### Step 3.5: Reorder Options
Add method `reorderOptions()`:
1. Triggered by SortableJS onUpdate event
2. Collect all option IDs in new display order
3. Build array: `[{optionId: 1, displayOrder: 1}, ...]`
4. Call API: `PUT /api/formbuilder/fields/{fieldId}/options/reorder`
5. Show success feedback

#### Step 3.6: Set Default Option
Add method `setDefaultOption(optionId)`:
1. Get current field type
2. If single-select (Dropdown/Radio):
   - Uncheck all other default checkboxes
   - Check only this one
3. If multi-select (Checkbox/MultiSelect):
   - Toggle this checkbox independently
4. Call API: `PATCH /api/formbuilder/options/{optionId}/default`

#### Step 3.7: Value Auto-Generation
Add real-time value generation:
1. On label input keyup/blur
2. If value is empty or auto-generated:
   - Convert label to lowercase
   - Replace spaces and special chars with underscores
   - Remove leading/trailing underscores
   - Set as value
   - Mark as auto-generated (data attribute)
3. If user manually edits value, stop auto-generation

#### Step 3.8: Update Field Selection Logic
Modify `selectField()` to show/hide options manager:
1. Check field type (DataType property)
2. If Dropdown/Radio/Checkbox/MultiSelect: show options manager
3. Otherwise: hide options manager
4. Update tab visibility if needed

---

### Phase 4: Integration & Polish (1 hour)

#### Step 4.1: Canvas Preview Updates
Ensure canvas updates when options change:
1. After add/update/delete/reorder, reload page OR
2. Update field preview partial with new options
3. Trigger re-render of dropdown/radio/checkbox elements

#### Step 4.2: SortableJS Configuration
Initialize drag-drop for options list:
- Group: 'field-options'
- Handle: '.option-drag-handle'
- Animation: 150ms
- GhostClass: 'sortable-ghost'
- OnUpdate: call `reorderOptions()`

#### Step 4.3: Error Handling
Add user-friendly error messages:
- "Option label is required"
- "Option value must be unique"
- "Cannot delete. Minimum 2 options required"
- "Failed to save option. Please try again."

#### Step 4.4: Loading States
Add visual feedback:
- Spinner on save button during API call
- Success checkmark on success
- Error icon on failure
- Disable inputs during save

---

## 6. Business Rules

### Validation Rules

**Option Label:**
- Required (cannot be empty)
- Max 200 characters
- Trimmed (leading/trailing spaces removed)
- Should be unique within field (warning, not error)

**Option Value:**
- Required (cannot be empty)
- Max 200 characters
- Must be unique within field (hard error)
- URL-safe format (lowercase, numbers, underscores only)
- Auto-generated from label if empty

**Minimum Options:**
- Selection fields require at least 2 options
- Cannot delete if only 2 remain
- Show error: "Selection fields require at least 2 options"

**Default Selection:**
- **Single-select (Dropdown/Radio):** Only 1 default allowed, setting new default unsets others
- **Multi-select (Checkbox/MultiSelect):** Multiple defaults allowed, each independent
- Can have no defaults (nothing pre-selected)

**Display Order:**
- Sequential integers (1, 2, 3...)
- Auto-assigned on create (max + 1)
- Updated by drag-drop

---

## 7. User Workflow

### Adding a Dropdown Field
1. User drags "Dropdown" from toolbox
2. Modal opens → enters "Color" → saves
3. Backend auto-creates 3 defaults: Option 1, Option 2, Option 3
4. Field appears in canvas
5. User selects field → Properties panel opens
6. User clicks Configuration tab
7. Options manager appears with 3 defaults
8. User edits, adds, deletes, reorders as needed

### Customizing Options
1. User clicks first option label input
2. Changes "Option 1" to "Red"
3. Value auto-updates to "red" on blur
4. User clicks "+ Add Option"
5. New row appears → enters "Blue" → blurs
6. Backend creates option with value "blue"
7. User drags "Blue" above "Red" to reorder
8. Backend updates DisplayOrder
9. User checks "Red" as default
10. Backend sets IsDefault=true for Red
11. Canvas preview updates immediately

### Switching Field Types
1. Field is Dropdown with options: Red, Blue, Green
2. User changes type to Radio (options preserved)
3. User changes type to Text (options hidden but kept)
4. User changes back to Dropdown (options restored!)

---

## 8. Technical Challenges & Solutions

### Challenge 1: Unique Value Generation
**Problem:** User enters "Red" but "red" already exists
**Solution:** Append counter ("red_1", "red_2") until unique

### Challenge 2: Default Option Logic
**Problem:** Single-select should allow only 1 default
**Solution:** Check field type in backend, unset others for Dropdown/Radio, allow multiple for Checkbox/MultiSelect

### Challenge 3: Minimum Options Enforcement
**Problem:** User tries to delete when only 2 options remain
**Solution:** Count options before delete, throw exception if ≤2, show friendly error in frontend

### Challenge 4: Drag-Drop Reordering
**Problem:** Need to update all display orders after reorder
**Solution:** Frontend sends array of {optionId, displayOrder}, backend loops and updates all

### Challenge 5: Real-Time Value Auto-Generation
**Problem:** Value should update as user types label
**Solution:** Use input event listener, track if auto-generated, stop if user manually edits value

---

## 9. Testing Checklist

### Functional Tests
- [ ] Add dropdown field → 3 defaults created
- [ ] Edit option label → value auto-updates
- [ ] Manually edit value → auto-generation stops
- [ ] Add 4th option → appears in list
- [ ] Delete 4th option → removed successfully
- [ ] Try deleting when 2 options → error shown
- [ ] Drag option to reorder → order persists
- [ ] Set default on dropdown → only 1 checked
- [ ] Set default on checkbox → multiple allowed
- [ ] Change field type to Text → options hidden
- [ ] Change back to Dropdown → options restored
- [ ] Duplicate field → options copied
- [ ] Delete field → options cascade deleted
- [ ] Canvas preview → reflects option changes

### Edge Cases
- [ ] Empty label → validation error
- [ ] Duplicate value → error shown
- [ ] Very long label (>200 chars) → truncated
- [ ] Special characters in label → converted in value
- [ ] Reorder with 1 option → no error
- [ ] Set default on already-default → works
- [ ] Concurrent edits → last save wins

---

## 10. Summary

### Current State
- ✅ 3 default options auto-created
- ✅ Options preserved on type changes
- ✅ Options displayed in preview
- ❌ No manual management UI

### What We're Building
- ✅ Full CRUD for options
- ✅ Drag-drop reordering
- ✅ Default selection management
- ✅ Smart validation & error handling
- ✅ Real-time value auto-generation

### Estimated Time
- Backend API: 2 hours
- Frontend UI: 1 hour
- Frontend JavaScript: 2 hours
- Integration & Polish: 1 hour
- **Total: 6 hours**

### Priority Order
1. Backend service methods + API endpoints
2. Frontend UI in Configuration tab
3. JavaScript CRUD operations
4. Drag-drop reordering
5. Default selection logic
6. Validation & error handling
7. Canvas preview integration

---

## 11. Future Enhancements (Phase 3)

- **Cascading Dropdowns:** Parent-child option relationships using ParentOptionId
- **Bulk Import/Export:** CSV import/export for bulk option management
- **Option Groups:** Categorize options with visual separators
- **Conditional Options:** Show/hide options based on other field values
- **API-Driven Options:** Load options dynamically from external API

---

**Document Status:** ✅ Ready for Implementation
**Next Step:** Start with backend service layer
