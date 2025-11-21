# Form Builder: Add Field Implementation Guide

**Purpose:** Detailed implementation for adding fields using drag-and-drop and modal methods  
**Date:** November 20, 2025

---

## 📋 Overview

### Two Methods to Add Fields

**Method 1: Drag & Drop** (Primary UX)
- Drag field type from palette → drop into section
- Shows quick config modal on drop
- Best for: Fast field creation, visual workflow

**Method 2: Button + Modal** (Alternative UX)  
- Click [+ Add Field] button in section
- Shows full field configuration modal
- Best for: Precise configuration, keyboard users

---

## 🎨 Method 1: Drag & Drop Implementation

### Step 1: Field Palette HTML
**File:** `Views/Forms/FormTemplates/Partials/FormBuilder/_FieldPalette.cshtml`

```html
<div class="field-palette-container">
    <div class="palette-header">
        <h6>Field Types</h6>
        <input type="search" id="palette-search" class="form-control form-control-sm" 
               placeholder="Search fields..." />
    </div>
    
    <div class="palette-body">
        <!-- Basic Fields Category -->
        <div class="palette-category">
            <div class="category-title">Basic Fields</div>
            
            <div id="field-palette" class="palette-items">
                <!-- Text Field -->
                <div class="palette-field-item" data-field-type="Text" data-category="Basic">
                    <i class="ri-text text-primary"></i>
                    <div class="field-type-info">
                        <strong>Text Input</strong>
                        <small>Single-line text</small>
                    </div>
                </div>
                
                <!-- Number Field -->
                <div class="palette-field-item" data-field-type="Number" data-category="Basic">
                    <i class="ri-hashtag text-success"></i>
                    <div class="field-type-info">
                        <strong>Number</strong>
                        <small>Numeric input</small>
                    </div>
                </div>
                
                <!-- Date Field -->
                <div class="palette-field-item" data-field-type="Date" data-category="Basic">
                    <i class="ri-calendar-line text-info"></i>
                    <div class="field-type-info">
                        <strong>Date</strong>
                        <small>Date picker</small>
                    </div>
                </div>
                
                <!-- Add other field types... -->
            </div>
        </div>
        
        <!-- Choice Fields Category -->
        <div class="palette-category">
            <div class="category-title">Choice Fields</div>
            
            <div class="palette-items">
                <div class="palette-field-item" data-field-type="Dropdown" data-category="Choice">
                    <i class="ri-arrow-down-s-line text-warning"></i>
                    <div class="field-type-info">
                        <strong>Dropdown</strong>
                        <small>Single selection</small>
                    </div>
                </div>
                <!-- Add other choice fields... -->
            </div>
        </div>
    </div>
</div>
```

### Step 2: Section Field Container
**File:** `Views/Forms/FormTemplates/Partials/FormBuilder/_SectionCard.cshtml`

```html
@model SectionDto

<div class="section-card" data-section-id="@Model.SectionId">
    <div class="section-header">
        <h5>@Model.SectionName</h5>
        <div class="section-actions">
            <button class="btn btn-sm btn-ghost-secondary">Edit</button>
            <button class="btn btn-sm btn-ghost-danger">Delete</button>
        </div>
    </div>
    
    <!-- FIELD DROP ZONE -->
    <div class="section-fields-container" 
         data-section-id="@Model.SectionId">
         
        @if (Model.Fields.Any())
        {
            foreach (var field in Model.Fields)
            {
                <partial name="_FieldCard" model="field" />
            }
        }
        else
        {
            <div class="field-drop-zone empty">
                <i class="ri-drag-drop-line"></i>
                <p>Drag field types here or click Add Field</p>
            </div>
        }
    </div>
    
    <!-- ADD FIELD BUTTON (Method 2) -->
    <button class="btn btn-soft-primary btn-sm w-100 mt-2" 
            onclick="FormBuilder.showAddFieldModal(@Model.SectionId)">
        <i class="ri-add-line"></i> Add Field
    </button>
</div>
```

### Step 3: Initialize SortableJS
**File:** `wwwroot/js/form-builder/panels/canvas-manager.js`

```javascript
class CanvasManager {
    initDragDrop() {
        // 1. Palette (clone mode)
        const palette = document.getElementById('field-palette');
        Sortable.create(palette, {
            group: {
                name: 'fields',
                pull: 'clone',
                put: false
            },
            sort: false,
            animation: 150,
            onEnd: this.handlePaletteDragEnd.bind(this)
        });
        
        // 2. Each section (drop zone)
        document.querySelectorAll('.section-fields-container').forEach(container => {
            Sortable.create(container, {
                group: 'fields',
                animation: 150,
                handle: '.drag-handle',
                onAdd: this.handleFieldDrop.bind(this),
                onUpdate: this.handleFieldReorder.bind(this)
            });
        });
    }
    
    async handleFieldDrop(evt) {
        const fieldType = evt.item.dataset.fieldType;
        const sectionId = evt.to.dataset.sectionId;
        const displayOrder = evt.newIndex + 1;
        
        if (fieldType) {
            // NEW field from palette
            evt.item.remove(); // Remove clone
            await this.createFieldFromPalette(sectionId, fieldType, displayOrder);
        }
    }
    
    async createFieldFromPalette(sectionId, fieldType, displayOrder) {
        // Show quick config modal
        const config = await this.showQuickFieldModal(fieldType);
        if (!config) return;
        
        // Create field via API
        const result = await ApiClient.post(
            `/api/formbuilder/${StateManager.templateId}/fields`,
            {
                sectionId: parseInt(sectionId),
                itemName: config.name,
                dataType: fieldType,
                isRequired: config.isRequired,
                displayOrder: displayOrder
            }
        );
        
        if (result.success) {
            await this.reloadSection(sectionId);
            AutosaveManager.markDirty();
            Toast.success('Field added');
        }
    }
    
    async showQuickFieldModal(fieldType) {
        const result = await Swal.fire({
            title: `Add ${fieldType} Field`,
            html: `
                <div class="mb-3 text-start">
                    <label class="form-label">Field Name *</label>
                    <input type="text" id="field-name" class="form-control" 
                           placeholder="e.g., Employee Count" autofocus>
                </div>
                <div class="form-check text-start">
                    <input type="checkbox" id="field-required" class="form-check-input">
                    <label class="form-check-label">Required field</label>
                </div>
            `,
            showCancelButton: true,
            confirmButtonText: 'Create Field',
            preConfirm: () => {
                const name = document.getElementById('field-name').value;
                if (!name) {
                    Swal.showValidationMessage('Field name is required');
                    return false;
                }
                return {
                    name: name,
                    isRequired: document.getElementById('field-required').checked
                };
            }
        });
        
        return result.isConfirmed ? result.value : null;
    }
}
```

---

## 🔘 Method 2: Button + Modal Implementation

### Step 1: Full Field Configuration Modal
**File:** `Views/Forms/FormTemplates/Partials/FormBuilder/_AddFieldModal.cshtml`

```html
<div class="modal fade" id="addFieldModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Add New Field</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            
            <div class="modal-body">
                <form id="addFieldForm">
                    <!-- Hidden section ID -->
                    <input type="hidden" id="modal-section-id" />
                    
                    <!-- Field Type Selection -->
                    <div class="mb-3">
                        <label class="form-label">Field Type *</label>
                        <select id="modal-field-type" class="form-select" required>
                            <option value="">Select field type...</option>
                            <optgroup label="Basic Fields">
                                <option value="Text">Text Input</option>
                                <option value="TextArea">Text Area</option>
                                <option value="Number">Number</option>
                                <option value="Date">Date</option>
                                <option value="Boolean">Yes/No</option>
                            </optgroup>
                            <optgroup label="Choice Fields">
                                <option value="Dropdown">Dropdown</option>
                                <option value="RadioButton">Radio Buttons</option>
                                <option value="MultiSelect">Multi-Select</option>
                            </optgroup>
                        </select>
                    </div>
                    
                    <!-- Field Name -->
                    <div class="mb-3">
                        <label class="form-label">Field Name *</label>
                        <input type="text" id="modal-field-name" class="form-control" 
                               placeholder="e.g., Employee Count" required />
                        <div class="form-text">The question shown to users</div>
                    </div>
                    
                    <!-- Field Code (auto-generated) -->
                    <div class="mb-3">
                        <label class="form-label">Field Code</label>
                        <div class="input-group">
                            <input type="text" id="modal-field-code" class="form-control" 
                                   readonly placeholder="Auto-generated" />
                            <button type="button" class="btn btn-outline-secondary" 
                                    onclick="FormBuilder.generateFieldCode()">
                                <i class="ri-refresh-line"></i> Generate
                            </button>
                        </div>
                    </div>
                    
                    <!-- Required -->
                    <div class="mb-3">
                        <div class="form-check">
                            <input type="checkbox" id="modal-field-required" class="form-check-input" />
                            <label class="form-check-label">Required field</label>
                        </div>
                    </div>
                    
                    <!-- Placeholder -->
                    <div class="mb-3">
                        <label class="form-label">Placeholder Text</label>
                        <input type="text" id="modal-placeholder" class="form-control" 
                               placeholder="Hint text in empty field" />
                    </div>
                    
                    <!-- Help Text -->
                    <div class="mb-3">
                        <label class="form-label">Help Text</label>
                        <textarea id="modal-help-text" class="form-control" rows="2" 
                                  placeholder="Additional guidance for users"></textarea>
                    </div>
                </form>
            </div>
            
            <div class="modal-footer">
                <button type="button" class="btn btn-light" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" onclick="FormBuilder.createFieldFromModal()">
                    <i class="ri-add-line"></i> Create Field
                </button>
            </div>
        </div>
    </div>
</div>
```

### Step 2: Modal Logic
**File:** `wwwroot/js/form-builder/components/field-builder.js`

```javascript
class FieldBuilder {
    /**
     * Show add field modal
     */
    showAddFieldModal(sectionId) {
        // Store section ID
        document.getElementById('modal-section-id').value = sectionId;
        
        // Reset form
        document.getElementById('addFieldForm').reset();
        document.getElementById('modal-field-code').value = '';
        
        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('addFieldModal'));
        modal.show();
    }
    
    /**
     * Generate field code based on section and sequence
     */
    async generateFieldCode() {
        const sectionId = document.getElementById('modal-section-id').value;
        const fieldName = document.getElementById('modal-field-name').value;
        
        if (!fieldName) {
            Toast.warning('Enter field name first');
            return;
        }
        
        // Call API to generate unique code
        const result = await ApiClient.post(
            `/api/formbuilder/fields/generate-code`,
            { sectionId, fieldName }
        );
        
        if (result.success) {
            document.getElementById('modal-field-code').value = result.code;
        }
    }
    
    /**
     * Create field from modal form
     */
    async createFieldFromModal() {
        const form = document.getElementById('addFieldForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }
        
        const sectionId = document.getElementById('modal-section-id').value;
        const fieldData = {
            sectionId: parseInt(sectionId),
            itemName: document.getElementById('modal-field-name').value,
            itemCode: document.getElementById('modal-field-code').value,
            dataType: document.getElementById('modal-field-type').value,
            isRequired: document.getElementById('modal-field-required').checked,
            placeholderText: document.getElementById('modal-placeholder').value,
            helpText: document.getElementById('modal-help-text').value
        };
        
        // Create field
        const result = await ApiClient.post(
            `/api/formbuilder/${StateManager.templateId}/fields`,
            fieldData
        );
        
        if (result.success) {
            // Close modal
            bootstrap.Modal.getInstance(document.getElementById('addFieldModal')).hide();
            
            // Reload section
            await CanvasManager.reloadSection(sectionId);
            
            // Mark dirty
            AutosaveManager.markDirty();
            
            // Success message
            Toast.success('Field created successfully');
        } else {
            Toast.error(result.message || 'Failed to create field');
        }
    }
}
```

---

## 🔄 Field Rendering Logic

### Backend: API Endpoint
**File:** `Controllers/API/FormBuilderApiController.cs`

```csharp
[HttpPost("{templateId}/fields")]
public async Task<IActionResult> CreateField(int templateId, [FromBody] CreateFieldDto dto)
{
    // 1. Validate template exists and is draft
    var template = await _context.FormTemplates.FindAsync(templateId);
    if (template == null || template.PublishStatus != "Draft")
        return BadRequest(new { success = false, message = "Invalid template" });
    
    // 2. Generate item code if not provided
    if (string.IsNullOrEmpty(dto.ItemCode))
    {
        dto.ItemCode = await GenerateItemCodeAsync(dto.SectionId);
    }
    
    // 3. Calculate display order if not provided
    if (dto.DisplayOrder == 0)
    {
        var maxOrder = await _context.FormTemplateItems
            .Where(i => i.SectionId == dto.SectionId)
            .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;
        dto.DisplayOrder = maxOrder + 1;
    }
    
    // 4. Create field entity
    var field = new FormTemplateItem
    {
        TemplateId = templateId,
        SectionId = dto.SectionId,
        ItemCode = dto.ItemCode,
        ItemName = dto.ItemName,
        DataType = dto.DataType,
        IsRequired = dto.IsRequired,
        DisplayOrder = dto.DisplayOrder,
        PlaceholderText = dto.PlaceholderText,
        HelpText = dto.HelpText,
        IsActive = true,
        CreatedDate = DateTime.UtcNow
    };
    
    _context.FormTemplateItems.Add(field);
    await _context.SaveChangesAsync();
    
    // 5. Build ViewModel for response
    var fieldViewModel = new FormFieldViewModel
    {
        FieldId = field.ItemId.ToString(),
        FieldName = field.ItemName,
        FieldType = Enum.Parse<FormFieldType>(field.DataType),
        IsRequired = field.IsRequired,
        PlaceholderText = field.PlaceholderText,
        HelpText = field.HelpText,
        // ... other properties
    };
    
    // 6. Render HTML using partial
    var html = await _razorViewEngine.RenderPartialToStringAsync(
        "~/Views/Forms/FormTemplates/Partials/FormBuilder/_FieldCard.cshtml",
        fieldViewModel
    );
    
    return Ok(new
    {
        success = true,
        itemId = field.ItemId,
        itemCode = field.ItemCode,
        html = html // Send rendered HTML back
    });
}
```

### Frontend: Reload Section
**File:** `wwwroot/js/form-builder/panels/canvas-manager.js`

```javascript
async reloadSection(sectionId) {
    // Option A: Reload individual section fields
    const response = await ApiClient.get(
        `/api/formbuilder/${StateManager.templateId}/sections/${sectionId}/fields`
    );
    
    if (response.success) {
        const container = document.querySelector(
            `.section-fields-container[data-section-id="${sectionId}"]`
        );
        container.innerHTML = response.html;
        
        // Reinitialize sortable
        this.initSectionSortable(container);
    }
}

// Alternative: Insert single field
async insertFieldCard(sectionId, fieldHtml) {
    const container = document.querySelector(
        `.section-fields-container[data-section-id="${sectionId}"]`
    );
    
    // Remove empty state if exists
    const emptyZone = container.querySelector('.field-drop-zone.empty');
    if (emptyZone) emptyZone.remove();
    
    // Append new field card
    container.insertAdjacentHTML('beforeend', fieldHtml);
    
    // Reinitialize sortable
    this.initSectionSortable(container);
}
```

---

## 📊 Complete Data Flow Diagram

```
USER INITIATES ADD FIELD
├─ Method 1: Drag from Palette
│   └─ User drags "Text Input" to Section 2
│       ↓
│   SortableJS onAdd event fires
│       ↓
│   Show Quick Modal (name + required)
│       ↓
│   User enters "Employee Count" + checks Required
│       ↓
│   POST /api/formbuilder/3/fields
│       ↓
│   Server creates FormTemplateItem
│       ↓
│   Server renders _FieldCard.cshtml
│       ↓
│   Returns { success: true, html: "..." }
│       ↓
│   JavaScript inserts HTML into section
│       ↓
│   Reinitialize Sortable
│       ↓
│   Mark dirty → autosave countdown
│
└─ Method 2: Button + Modal
    └─ User clicks [+ Add Field] in Section 2
        ↓
    Show Full Modal
        ↓
    User selects Type, enters Name, configures options
        ↓
    Click [Create Field]
        ↓
    POST /api/formbuilder/3/fields
        ↓
    (Same flow as Method 1 from here)
```

---

## ✅ Implementation Checklist

### Drag & Drop
- [ ] Install Sortable.js library
- [ ] Create field palette HTML with all types
- [ ] Style palette items with icons
- [ ] Initialize Sortable on palette (clone mode)
- [ ] Initialize Sortable on sections (move mode)
- [ ] Handle onAdd event → show quick modal
- [ ] Create field via API
- [ ] Insert rendered HTML
- [ ] Reinitialize Sortable

### Button + Modal
- [ ] Create full field configuration modal HTML
- [ ] Show modal on [+ Add Field] click
- [ ] Populate section ID in hidden field
- [ ] Implement field code generator
- [ ] Validate form before submit
- [ ] Create field via same API
- [ ] Close modal on success
- [ ] Insert rendered HTML

### Field Rendering
- [ ] Create `_FieldCard.cshtml` wrapper
- [ ] Reuse existing field partials
- [ ] Add edit/delete buttons
- [ ] Add drag handle
- [ ] Show validation/logic indicators
- [ ] Style for builder context

---

**Document Version:** 1.0  
**Last Updated:** November 20, 2025
