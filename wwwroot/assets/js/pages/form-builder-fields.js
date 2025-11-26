/**
 * Form Builder - Field Management
 * Handles field CRUD operations (Create, Read, Update, Delete)
 * Manages 21 different field types with type-specific logic
 */

const FormBuilderFields = {
    pendingFieldType: null,
    pendingSectionId: null,

    /**
     * Initialize field management
     */
    init: function() {
        console.log('FormBuilderFields initialized');
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

        console.log('Opening Add Field modal for section:', sectionId, 'type:', fieldType);

        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
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
                DisplayOrder: 0 // Will be auto-calculated by backend
            };

            console.log('Saving field:', fieldData);

            // Get anti-forgery token
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            // Call API to create field
            const response = await fetch('/Forms/FormTemplates/AddField', {
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
                // Success! Close modal and reload
                const modal = bootstrap.Modal.getInstance(document.getElementById('addFieldModal'));
                if (modal) {
                    modal.hide();
                }

                // Reload the form builder to show new field (no success alert)
                await this.reloadFormBuilder(sectionId);
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
     */
    reloadFormBuilder: async function(sectionId) {
        try {
            // Get template ID from page
            const templateId = document.querySelector('[data-template-id]')?.dataset.templateId;

            if (!templateId) {
                console.error('Template ID not found');
                // Fallback: reload entire page
                window.location.reload();
                return;
            }

            // Reload the entire form builder view
            console.log('Reloading form builder for template:', templateId);
            window.location.reload();
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
        const fieldBody = document.getElementById(`field-body-${fieldId}`);
        const collapseIcon = document.getElementById(`field-collapse-icon-${fieldId}`);

        if (fieldBody && collapseIcon) {
            if (fieldBody.style.display === 'none') {
                // Expand
                fieldBody.style.display = 'block';
                collapseIcon.classList.remove('ri-add-line');
                collapseIcon.classList.add('ri-subtract-line');
            } else {
                // Collapse
                fieldBody.style.display = 'none';
                collapseIcon.classList.remove('ri-subtract-line');
                collapseIcon.classList.add('ri-add-line');
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

            // Call API to update field type
            const response = await fetch(`/Forms/FormTemplates/UpdateFieldType/${fieldId}`, {
                method: 'POST',
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
                // Success - reload to show updated field
                console.log('Field type updated successfully');
                window.location.reload();
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
    deleteField: function(fieldId) {
        // TODO: Show confirmation modal and delete field
        console.log('Delete field:', fieldId);
        if (confirm('Delete this field?')) {
            alert('Delete field functionality will be implemented later');
        }
    },

    /**
     * Duplicate field
     * @param {number} fieldId - Field ID to duplicate
     */
    duplicateField: function(fieldId) {
        // TODO: Duplicate field with all settings
        console.log('Duplicate field:', fieldId);
        alert('Duplicate field functionality will be implemented later');
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
 * Global function to delete field
 */
window.deleteField = function(fieldId) {
    FormBuilderFields.deleteField(fieldId);
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
