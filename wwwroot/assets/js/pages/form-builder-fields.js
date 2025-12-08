/**
 * Form Builder - Field Management
 * Handles field CRUD operations (Create, Read, Update, Delete)
 * Manages 21 different field types with type-specific logic
 */

const FormBuilderFields = {
    pendingFieldType: null,
    pendingSectionId: null,
    pendingDeleteFieldId: null,
    pendingDeleteFieldData: null,
    
    // Cache for option templates (loaded once per page session)
    _optionTemplatesCache: null,

    /**
     * Initialize field management
     */
    init: function() {
        // Pre-load option templates in background for faster modal opening
        this.preloadOptionTemplates();
        console.log('FormBuilderFields initialized');
    },
    
    /**
     * Pre-load option templates in background
     */
    preloadOptionTemplates: async function() {
        if (this._optionTemplatesCache) return; // Already loaded
        
        try {
            const response = await fetch('/api/formbuilder/option-templates/list');
            if (response.ok) {
                const result = await response.json();
                if (result.success && result.templates) {
                    this._optionTemplatesCache = result.templates;
                    console.log(`[FormBuilderFields] Pre-loaded ${result.templates.length} option templates`);
                }
            }
        } catch (error) {
            console.warn('[FormBuilderFields] Failed to pre-load option templates:', error);
        }
    },

    // ========================================================================
    // UTILITY FUNCTIONS
    // ========================================================================

    /**
     * Show error modal with custom message
     * @param {string} title - Error title
     * @param {string} message - User-friendly error message
     * @param {string} technicalDetails - Technical error details (optional)
     */
    showErrorModal: function(title, message, technicalDetails = null) {
        // Set modal content
        document.getElementById('errorModalTitle').textContent = title;
        document.getElementById('errorModalHeading').textContent = title;
        document.getElementById('errorModalMessage').textContent = message;

        // Show/hide technical details
        const detailsContainer = document.getElementById('errorDetailsContainer');
        if (technicalDetails) {
            document.getElementById('errorTechnicalDetails').textContent = technicalDetails;
            detailsContainer.style.display = 'block';
        } else {
            detailsContainer.style.display = 'none';
        }

        // Show modal
        const errorModal = new bootstrap.Modal(document.getElementById('errorModal'));
        errorModal.show();
    },

    // ========================================================================
    // MODAL OPERATIONS
    // ========================================================================

    /**
     * Show Add Field Modal
     * @param {Event} event - Click event
     * @param {number} sectionId - Section ID to add field to
     * @param {string} fieldType - Type of field to add (from FormFieldType enum)
     */
    showAddFieldModal: function(event, sectionId, fieldType) {
        // Stop event propagation
        if (event) {
            event.stopPropagation();
            event.preventDefault();
        }

        // Get modal elements
        const modal = document.getElementById('addFieldModal');
        const fieldTypeName = document.getElementById('fieldTypeName');
        const fieldTypeDisplay = document.getElementById('fieldTypeDisplay');
        const fieldTypeInput = document.getElementById('fieldType');
        const fieldSectionIdInput = document.getElementById('fieldSectionId');

        if (!modal) {
            console.error('Add Field modal not found');
            return;
        }

        // Update modal header with field type name
        if (fieldTypeName) {
            fieldTypeName.textContent = fieldType || 'Field';
        }

        // Update field type display in alert
        if (fieldTypeDisplay) {
            fieldTypeDisplay.textContent = fieldType || 'Not Selected';
        }

        // Set hidden form fields
        if (fieldTypeInput) {
            fieldTypeInput.value = fieldType || '';
        }

        if (fieldSectionIdInput) {
            fieldSectionIdInput.value = sectionId || '';
        }

        // Store field type and section ID for future use
        this.pendingFieldType = fieldType;
        this.pendingSectionId = sectionId;

        // Show/hide option template selector based on field type
        this.toggleOptionTemplateSelector(fieldType);

        console.log('Opening Add Field modal for section:', sectionId, 'type:', fieldType);

        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    },

    /**
     * Check if field type requires options (selection fields)
     * @param {string} fieldType - Field type to check
     * @returns {boolean} True if field type requires options
     */
    requiresOptions: function(fieldType) {
        const selectionTypes = ['Dropdown', 'Radio', 'Checkbox', 'MultiSelect'];
        return selectionTypes.includes(fieldType);
    },

    /**
     * Toggle option template selector visibility and load templates
     * @param {string} fieldType - Current field type
     */
    toggleOptionTemplateSelector: async function(fieldType) {
        const container = document.getElementById('optionTemplateContainer');
        const selector = document.getElementById('optionTemplateId');
        
        if (!container || !selector) return;

        if (this.requiresOptions(fieldType)) {
            // Show container and load templates
            container.style.display = 'block';
            await this.loadOptionTemplates(fieldType);
        } else {
            // Hide container for non-selection fields
            container.style.display = 'none';
            // Reset to default
            selector.innerHTML = '<option value="0" selected>Default Options (Option 1, 2, 3)</option>';
        }
    },

    /**
     * Load option templates for the selector (uses cache for instant loading)
     * @param {string} fieldType - Field type to filter templates (optional)
     */
    loadOptionTemplates: async function(fieldType) {
        const selector = document.getElementById('optionTemplateId');
        if (!selector) return;

        // Clear and set default option
        selector.innerHTML = '<option value="0" selected>Default Options (Option 1, 2, 3)</option>';

        // Use cached templates if available
        let templates = this._optionTemplatesCache;
        
        if (!templates) {
            // Fetch from lightweight endpoint if not cached
            try {
                const response = await fetch('/api/formbuilder/option-templates/list');
                if (!response.ok) {
                    console.error('Failed to load option templates');
                    return;
                }

                const result = await response.json();
                if (!result.success || !result.templates) {
                    console.error('Invalid response from option templates API');
                    return;
                }
                
                templates = result.templates;
                this._optionTemplatesCache = templates; // Cache for next time
            } catch (error) {
                console.error('Error loading option templates:', error);
                return;
            }
        }

        // Group templates by category
        const grouped = {};
        templates.forEach(template => {
            const category = template.category || 'Custom';
            if (!grouped[category]) {
                grouped[category] = [];
            }
            grouped[category].push(template);
        });

        // Add grouped options
        Object.keys(grouped).sort().forEach(category => {
            const optgroup = document.createElement('optgroup');
            optgroup.label = category;
            
            grouped[category].forEach(template => {
                const option = document.createElement('option');
                option.value = template.templateId;
                option.textContent = `${template.templateName} (${template.optionCount} options)`;
                optgroup.appendChild(option);
            });
            
            selector.appendChild(optgroup);
        });

        console.log(`[AddField] Loaded ${templates.length} option templates from ${this._optionTemplatesCache ? 'cache' : 'API'}`);
    },

    /**
     * Populate modal with field type-specific form
     * @param {string} fieldType - Field type to display form for
     */
    populateModalForFieldType: function(fieldType) {
        // TODO: Implement type-specific form rendering
        // This will show different fields based on field type:
        // - Text/TextArea: Placeholder, Max Length
        // - Dropdown/Radio/Checkbox: Options manager
        // - Number/Decimal: Min/Max values
        // - Date/DateTime: Date range
        // - FileUpload: File types, Max size
        // etc.
        console.log('Populating form for field type:', fieldType);
    },

    /**
     * Save field from modal
     */
    saveFieldFromModal: async function() {
        try {
            // Get form elements
            const form = document.getElementById('addFieldForm');
            const btnSave = document.getElementById('btnAddField');

            if (!form) {
                console.error('Form not found');
                return;
            }

            // Validate form
            if (!form.checkValidity()) {
                form.classList.add('was-validated');
                return;
            }

            // Get form values
            const fieldType = document.getElementById('fieldType')?.value;
            const sectionId = document.getElementById('fieldSectionId')?.value;
            const itemName = document.getElementById('itemName')?.value?.trim();
            const itemDescription = document.getElementById('itemDescription')?.value?.trim();
            const isRequired = document.getElementById('isRequired')?.checked || false;
            const optionTemplateId = parseInt(document.getElementById('optionTemplateId')?.value) || 0;

            // Validate required fields
            if (!fieldType || !sectionId || !itemName) {
                this.showErrorModal(
                    'Missing Information',
                    'Please fill in all required fields. Field label is required.',
                    `Missing: ${!fieldType ? 'Field Type, ' : ''}${!sectionId ? 'Section ID, ' : ''}${!itemName ? 'Field Label' : ''}`
                );
                return;
            }

            // Show loading state
            if (btnSave) {
                btnSave.disabled = true;
                btnSave.innerHTML = '<i class="ri-loader-4-line spinner-border spinner-border-sm me-1"></i>Adding...';
            }

            // Prepare data for API - property names must match C# DTO exactly (PascalCase)
            const fieldData = {
                SectionId: parseInt(sectionId),
                ItemName: itemName,
                ItemDescription: itemDescription || null,
                DataType: fieldType,  // Must match C# property: FormFieldType DataType
                IsRequired: isRequired,
                DisplayOrder: 0, // Will be auto-calculated by backend
                OptionTemplateId: optionTemplateId > 0 ? optionTemplateId : null // Option template to use (0 = default options)
            };

            console.log('Saving field:', fieldData);

            // Get anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // Call API to create field (new RESTful endpoint)
            const response = await fetch('/api/formbuilder/fields', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token || ''
                },
                body: JSON.stringify(fieldData)
            });

            // Check if response is OK
            if (!response.ok) {
                console.error('Server responded with error:', response.status, response.statusText);
                const errorText = await response.text();
                console.error('Response body:', errorText);
                throw new Error(`Server error: ${response.status} ${response.statusText}`);
            }

            // Try to parse JSON with better error handling
            const responseText = await response.text();
            console.log('Response text:', responseText);

            let result;
            try {
                result = JSON.parse(responseText);
            } catch (e) {
                console.error('Failed to parse JSON:', e);
                console.error('Response was:', responseText);
                throw new Error('Invalid JSON response from server');
            }

            if (result.success) {
                // Reset button state immediately before closing modal
                if (btnSave) {
                    btnSave.disabled = false;
                    btnSave.innerHTML = '<i class="ri-add-circle-line me-1"></i>Add Field';
                }

                // Success! Close modal and add field dynamically
                const modal = bootstrap.Modal.getInstance(document.getElementById('addFieldModal'));
                if (modal) {
                    modal.hide();
                }

                // Add field to canvas without page reload
                const newFieldId = result.field?.itemId;
                console.log(`[AddField] Adding new field ${newFieldId} to section ${sectionId}`);

                // Find the section's fields container
                const sectionContainer = document.querySelector(`.fields-container[data-section-id="${sectionId}"]`);

                if (!sectionContainer) {
                    console.error(`[AddField] Section container not found for section ${sectionId}`);
                    // Fallback to reload if container not found
                    await this.reloadFormBuilder(sectionId, newFieldId);
                    return;
                }

                try {
                    // Render and insert the new field card
                    const newFieldCard = await FormBuilder.renderAndInsertField(newFieldId, 'append', sectionContainer);
                    console.log(`[AddField] ✅ Field ${newFieldId} added to DOM`);

                    // Select the new field immediately
                    if (typeof selectField === 'function') {
                        selectField(newFieldId);
                        console.log(`[AddField] ✅ Field ${newFieldId} selected`);
                    } else {
                        console.warn(`[AddField] selectField function not available`);
                    }
                } catch (error) {
                    console.error(`[AddField] Error rendering field:`, error);
                    // Fallback to reload on error
                    await this.reloadFormBuilder(sectionId, newFieldId);
                }
            } else {
                // Show error modal with server message
                this.showErrorModal(
                    'Failed to Add Field',
                    result.message || 'An error occurred while adding the field. Please try again.',
                    result.details || null
                );

                // Re-enable save button
                if (btnSave) {
                    btnSave.disabled = false;
                    btnSave.innerHTML = '<i class="ri-add-circle-line me-1"></i>Add Field';
                }
            }
        } catch (error) {
            console.error('Error saving field:', error);

            // Show error modal instead of alert
            this.showErrorModal(
                'Network Error',
                'Unable to communicate with the server. Please check your connection and try again.',
                error.message + (error.stack ? '\n\n' + error.stack : '')
            );

            // Re-enable save button
            const btnSave = document.getElementById('btnAddField');
            if (btnSave) {
                btnSave.disabled = false;
                btnSave.innerHTML = '<i class="ri-add-circle-line me-1"></i>Add Field';
            }
        }
    },

    /**
     * Reload form builder section to show new field
     * @param {number} sectionId - Section ID (optional, not used currently)
     * @param {number} newFieldId - New field ID to auto-select after reload
     */
    reloadFormBuilder: async function(sectionId, newFieldId) {
        try {
            // Get template ID from page
            const templateId = document.querySelector('[data-template-id]')?.dataset.templateId;

            if (!templateId) {
                console.error('Template ID not found');
                // Fallback: reload entire page
                window.location.reload();
                return;
            }

            // Reload with field ID in URL to auto-select after page loads
            if (newFieldId) {
                console.log(`Reloading form builder and will auto-select field ${newFieldId}`);
                window.location.href = `${window.location.pathname}?selectField=${newFieldId}`;
            } else {
                console.log('Reloading form builder');
                window.location.reload();
            }
        } catch (error) {
            console.error('Error reloading form builder:', error);
            // Fallback: reload entire page
            window.location.reload();
        }
    },

    /**
     * Toggle field collapse/expand
     * @param {number} fieldId - Field ID to toggle
     */
    toggleFieldCollapse: function(fieldId) {
        const fieldCard = document.getElementById(`field-${fieldId}`);
        const fieldBody = document.getElementById(`field-body-${fieldId}`);
        const fieldFooter = document.getElementById(`field-footer-${fieldId}`);
        const collapseIcon = document.getElementById(`field-collapse-icon-${fieldId}`);

        if (fieldBody && collapseIcon) {
            // Check if field is currently selected
            const isSelected = fieldCard && fieldCard.classList.contains('selected-element');
            
            if (fieldBody.style.display === 'none') {
                // Expanding - show body, hide footer
                fieldBody.style.display = 'block';
                if (fieldFooter) fieldFooter.style.display = 'none';
                collapseIcon.classList.remove('ri-add-line');
                collapseIcon.classList.add('ri-subtract-line');
            } else {
                // Only allow collapsing if field is NOT selected
                if (!isSelected) {
                    // Collapsing - hide body, show footer
                    fieldBody.style.display = 'none';
                    if (fieldFooter) fieldFooter.style.display = 'block';
                    collapseIcon.classList.remove('ri-subtract-line');
                    collapseIcon.classList.add('ri-add-line');
                } else {
                    // Field is selected - don't collapse, maybe show visual feedback
                    console.log('Cannot collapse selected field');
                }
            }
        }
    },

    /**
     * Change field type inline (quick edit)
     * @param {number} fieldId - Field ID
     * @param {string} newType - New field type
     */
    changeFieldType: async function(fieldId, newType) {
        try {
            console.log(`Changing field ${fieldId} type to ${newType}`);

            // Prepare update data
            const updateData = {
                DataType: newType
            };

            // Call API to update field type (new RESTful endpoint with PATCH verb)
            const response = await fetch(`/api/formbuilder/fields/${fieldId}/type`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updateData)
            });

            if (!response.ok) {
                throw new Error(`Server error: ${response.status}`);
            }

            const result = await response.json();

            if (result.success) {
                // Success - replace field card in DOM with updated version
                console.log(`[ChangeFieldType] Field ${fieldId} type changed to ${newType}`);

                try {
                    // Render and replace the field card
                    const newFieldCard = await FormBuilder.renderAndInsertField(fieldId, 'replace');
                    console.log(`[ChangeFieldType] ✅ Field ${fieldId} updated in DOM`);

                    // Keep field selected (renderAndInsertField preserves selection)
                    // Reload properties panel to show updated configuration options
                    if (typeof selectField === 'function') {
                        selectField(fieldId);
                        console.log(`[ChangeFieldType] ✅ Field ${fieldId} re-selected with new properties`);
                    }
                } catch (error) {
                    console.error(`[ChangeFieldType] Error rendering field:`, error);
                    // Fallback to reload on error
                    window.location.reload();
                }
            } else {
                this.showErrorModal(
                    'Failed to Change Field Type',
                    result.message || 'Unable to change field type. Please try again.',
                    null
                );
            }
        } catch (error) {
            console.error('Error changing field type:', error);
            this.showErrorModal(
                'Error',
                'An error occurred while changing the field type. Please try again.',
                error.message
            );
        }
    },

    // ========================================================================
    // FIELD CRUD OPERATIONS
    // ========================================================================

    /**
     * Edit field (removed - inline editing via dropdown instead)
     */

    /**
     * Delete field
     * @param {number} fieldId - Field ID to delete
     */
    /**
     * Show delete field confirmation modal
     * @param {number} fieldId - Field ID to delete
     */
    showDeleteFieldModal: function(fieldId) {
        // Get field card to extract field info
        const fieldCard = document.getElementById(`field-${fieldId}`);
        if (!fieldCard) {
            console.error('Field card not found');
            return;
        }

        // Get field name from the card (if available)
        const fieldName = fieldCard.querySelector('.field-preview-content label')?.textContent || `Field #${fieldId}`;

        // Store field ID for confirmation
        this.pendingDeleteFieldId = fieldId;
        this.pendingDeleteFieldData = { fieldName };

        // Update modal content
        document.getElementById('deleteFieldName').textContent = fieldName;

        // Check if field has options
        const optionCount = fieldCard.querySelector('[data-field-type]')?.dataset.optionCount || 0;
        const warningOptions = document.getElementById('deleteWarningOptions');
        const optionCountSpan = document.getElementById('deleteOptionCount');

        if (optionCount > 0) {
            optionCountSpan.textContent = optionCount;
            warningOptions.classList.remove('d-none');
        } else {
            warningOptions.classList.add('d-none');
        }

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('deleteFieldModal'));
        modal.show();

        console.log(`Delete field modal shown for field ${fieldId}`);
    },

    /**
     * Confirm and execute field deletion
     */
    confirmDeleteField: async function() {
        if (!this.pendingDeleteFieldId) {
            console.error('No field ID to delete');
            return;
        }

        const fieldId = this.pendingDeleteFieldId;

        try {
            // Hide modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('deleteFieldModal'));
            if (modal) {
                modal.hide();
            }

            console.log('Deleting field:', fieldId);

            // Call API to delete field (new RESTful endpoint with DELETE verb)
            const response = await fetch(`/api/formbuilder/fields/${fieldId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                throw new Error(`Server error: ${response.status}`);
            }

            const result = await response.json();

            if (result.success) {
                // Success - remove field from DOM without page reload
                console.log('Field deleted successfully');

                // Find and remove the field card from canvas
                const fieldCard = document.getElementById(`field-${fieldId}`);
                if (fieldCard) {
                    fieldCard.remove();
                    console.log(`Field card ${fieldId} removed from DOM`);
                }

                // Clear properties panel if this field was selected
                if (window.FormBuilderProperties) {
                    // Check if the deleted field is currently selected (use == for type coercion)
                    const isFieldSelected = FormBuilderProperties.currentElementType === 'field' && 
                                           FormBuilderProperties.currentElementId == fieldId;
                    
                    console.log('[FormBuilderFields] Checking if field was selected:', {
                        currentType: FormBuilderProperties.currentElementType,
                        currentId: FormBuilderProperties.currentElementId,
                        deletedId: fieldId,
                        isSelected: isFieldSelected
                    });
                    
                    if (isFieldSelected) {
                        // Use the proper showEmptyState method to clear the panel
                        FormBuilderProperties.showEmptyState();
                        console.log('[FormBuilderFields] Properties panel cleared after field deletion');
                    }
                }

                // Remove selection from all elements
                document.querySelectorAll('.selected-element').forEach(el => {
                    el.classList.remove('selected-element');
                });

                // Reset pending delete data
                this.pendingDeleteFieldId = null;
                this.pendingDeleteFieldData = null;
            } else {
                this.showErrorModal(
                    'Failed to Delete Field',
                    result.message || 'Unable to delete field. Please try again.',
                    null
                );
            }
        } catch (error) {
            console.error('Error deleting field:', error);
            this.showErrorModal(
                'Error',
                'An error occurred while deleting the field. Please try again.',
                error.message
            );
        }
    },

    /**
     * Duplicate field
     * @param {number} fieldId - Field ID to duplicate
     */
    duplicateField: async function(fieldId) {
        try {
            console.log('Duplicating field:', fieldId);

            // Call API to duplicate field (new RESTful endpoint)
            const response = await fetch(`/api/formbuilder/fields/${fieldId}/duplicate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if (!response.ok) {
                throw new Error(`Server error: ${response.status}`);
            }

            const result = await response.json();

            if (result.success) {
                // Success - insert duplicated field after original
                const newFieldId = result.field?.itemId;
                console.log(`[DuplicateField] Field ${fieldId} duplicated as ${newFieldId}`);

                // Find the original field card
                const originalFieldCard = document.getElementById(`field-${fieldId}`);

                if (!originalFieldCard) {
                    console.error(`[DuplicateField] Original field card not found, falling back to reload`);
                    window.location.reload();
                    return;
                }

                try {
                    // Render and insert the duplicated field after the original
                    const newFieldCard = await FormBuilder.renderAndInsertField(newFieldId, 'insertAfter', originalFieldCard);
                    console.log(`[DuplicateField] ✅ Field ${newFieldId} inserted after ${fieldId}`);

                    // Select the duplicated field immediately
                    if (typeof selectField === 'function') {
                        selectField(newFieldId);
                        console.log(`[DuplicateField] ✅ Field ${newFieldId} selected`);
                    }
                } catch (error) {
                    console.error(`[DuplicateField] Error rendering field:`, error);
                    // Fallback to reload on error
                    window.location.reload();
                }
            } else {
                this.showErrorModal(
                    'Failed to Duplicate Field',
                    result.message || 'Unable to duplicate field. Please try again.',
                    null
                );
            }
        } catch (error) {
            console.error('Error duplicating field:', error);
            this.showErrorModal(
                'Error',
                'An error occurred while duplicating the field. Please try again.',
                error.message
            );
        }
    },

    // ========================================================================
    // TYPE-SPECIFIC LOGIC (To be implemented per field type)
    // ========================================================================

    /**
     * Handle Dropdown field specifics
     */
    handleDropdownField: function() {
        // TODO: Options manager for dropdown
    },

    /**
     * Handle Radio field specifics
     */
    handleRadioField: function() {
        // TODO: Options manager for radio buttons
    },

    /**
     * Handle Checkbox field specifics
     */
    handleCheckboxField: function() {
        // TODO: Options manager for checkboxes
    },

    /**
     * Handle MultiSelect field specifics
     */
    handleMultiSelectField: function() {
        // TODO: Options manager for multi-select
    },

    /**
     * Handle Number field specifics
     */
    handleNumberField: function() {
        // TODO: Min/Max value settings
    },

    /**
     * Handle FileUpload field specifics
     */
    handleFileUploadField: function() {
        // TODO: File type restrictions, max size
    },

    // ========================================================================
    // OPTIONS MANAGEMENT (For Dropdown, Radio, Checkbox, MultiSelect)
    // ========================================================================

    /**
     * Add option to selection field
     */
    addOption: function() {
        // TODO: Add new option row
        console.log('Add option');
    },

    /**
     * Remove option from selection field
     * @param {number} optionIndex - Index of option to remove
     */
    removeOption: function(optionIndex) {
        // TODO: Remove option row
        console.log('Remove option:', optionIndex);
    },

    /**
     * Reorder options
     */
    reorderOptions: function() {
        // TODO: Drag-drop reordering of options
        console.log('Reorder options');
    },

    // ========================================================================
    // VALIDATION MANAGEMENT
    // ========================================================================

    /**
     * Add validation rule to field
     */
    addValidationRule: function() {
        // TODO: Add validation rule
        console.log('Add validation rule');
    },

    /**
     * Remove validation rule
     * @param {number} ruleIndex - Index of rule to remove
     */
    removeValidationRule: function(ruleIndex) {
        // TODO: Remove validation rule
        console.log('Remove validation rule:', ruleIndex);
    }
};

// ============================================================================
// EXPOSE GLOBAL FUNCTIONS
// ============================================================================

/**
 * Global function to show add field modal (called from dropdown)
 */
window.showAddFieldModal = function(event, sectionId, fieldType) {
    FormBuilderFields.showAddFieldModal(event, sectionId, fieldType);
};

/**
 * Global function to edit field
 */
window.editField = function(fieldId) {
    FormBuilderFields.editField(fieldId);
};

/**
 * Global function to delete field (shows confirmation modal)
 */
window.deleteField = function(fieldId) {
    FormBuilderFields.showDeleteFieldModal(fieldId);
};

/**
 * Global function to confirm field deletion (called from modal)
 */
window.confirmDeleteField = function() {
    FormBuilderFields.confirmDeleteField();
};

/**
 * Global function to duplicate field
 */
window.duplicateField = function(fieldId) {
    FormBuilderFields.duplicateField(fieldId);
};

/**
 * Global function to save field from modal
 */
window.saveFieldFromModal = function() {
    FormBuilderFields.saveFieldFromModal();
};

/**
 * Global function to toggle field collapse
 */
window.toggleFieldCollapse = function(fieldId) {
    FormBuilderFields.toggleFieldCollapse(fieldId);
};

/**
 * Global function to change field type
 */
window.changeFieldType = function(fieldId, newType) {
    FormBuilderFields.changeFieldType(fieldId, newType);
};
