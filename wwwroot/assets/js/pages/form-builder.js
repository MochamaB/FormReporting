/**
 * Form Builder - Main Orchestrator
 * Initializes all form builder components and manages global state
 */

// Global FormBuilder namespace
const FormBuilder = {
    // Template ID (set from view)
    templateId: null,

    // Selected section state
    selectedSectionId: null,

    // SortableJS instance for sections
    sectionsSortable: null,

    /**
     * Initialize the form builder
     * @param {number} templateId - The template ID being edited
     */
    init: function(templateId) {
        this.templateId = templateId;
        console.log('Form Builder initialized for template:', templateId);

        // Initialize all components
        FormBuilderLayout.init();
        FormBuilderDragDrop.init(templateId);
        FormBuilderFields.init();
        FormBuilderProperties.init();

        // Initialize FormBuilderOptions (for delete modal, etc.)
        if (typeof FormBuilderOptions !== 'undefined') {
            FormBuilderOptions.init();
        }

        // Setup event listeners
        this.setupEventListeners();
    },

    /**
     * Setup global event listeners
     */
    setupEventListeners: function() {
        // isCollapsible checkbox change listener
        const isCollapsibleCheckbox = document.getElementById('isCollapsible');
        if (isCollapsibleCheckbox) {
            isCollapsibleCheckbox.addEventListener('change', function() {
                const container = document.getElementById('collapsedByDefaultContainer');
                if (this.checked) {
                    container.style.display = 'block';
                } else {
                    container.style.display = 'none';
                    document.getElementById('isCollapsedByDefault').checked = false;
                }
            });
        }
    },

    /**
     * Reload the current page
     */
    reload: function() {
        location.reload();
    },

    /**
     * Validate stage completion using database state (AJAX call)
     * @param {number} templateId - Template ID to validate
     * @param {number} currentStage - Current stage enum value
     * @returns {Promise} Promise that resolves with validation result
     */
    validateStageCompletion: async function(templateId, currentStage) {
        try {
            // Note: ValidateStageCompletion uses attribute routing, not conventional routing
            const response = await fetch(`/Forms/FormTemplates/ValidateStageCompletion?id=${templateId}&currentStage=${currentStage}`);
            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Validation error:', error);
            return {
                success: false,
                isValid: false,
                message: 'Error validating template. Please try again.'
            };
        }
    },

    /**
     * Reload a specific field's data (for options management)
     * @param {number} fieldId - Field ID to reload
     * @returns {Promise} Promise that resolves when field is reloaded
     */
    reloadField: async function(fieldId) {
        try {
            console.log(`Reloading field ${fieldId}`);
            const response = await fetch(`/api/formbuilder/fields/${fieldId}`);
            if (!response.ok) throw new Error('Failed to load field');

            const result = await response.json();
            const fieldData = result.field;

            // Reload field in properties panel if it's the currently selected field
            if (FormBuilderProperties && FormBuilderProperties.currentFieldId === fieldId) {
                FormBuilderProperties.loadOptions(fieldData);
            }

            // Update field preview in canvas
            if (FormBuilderProperties && FormBuilderProperties.updateCanvasPreview) {
                await FormBuilderProperties.updateCanvasPreview();
            }

            console.log(`Field ${fieldId} reloaded successfully`);
            return fieldData;
        } catch (error) {
            console.error('Error reloading field:', error);
            throw error;
        }
    },

    /**
     * Render field HTML from server and insert into DOM (no page reload)
     * @param {number} fieldId - Field ID to render
     * @param {string} action - Action: 'append', 'replace', 'insertAfter'
     * @param {HTMLElement} targetElement - Target element (required for append/insertAfter)
     * @returns {Promise<HTMLElement>} The inserted field card element
     */
    renderAndInsertField: async function(fieldId, action = 'append', targetElement = null) {
        try {
            console.log(`[FormBuilder] Rendering field ${fieldId} with action: ${action}`);

            // Fetch rendered HTML from server
            const response = await fetch(`/api/formbuilder/fields/${fieldId}/render`);

            if (!response.ok) {
                throw new Error(`Failed to render field: ${response.status}`);
            }

            const result = await response.json();

            if (!result.success || !result.html) {
                throw new Error('Invalid render response from server');
            }

            // Parse HTML string into DOM element
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = result.html;
            const newFieldCard = tempDiv.firstElementChild;

            if (!newFieldCard) {
                throw new Error('No field card in rendered HTML');
            }

            // Insert into DOM based on action
            switch(action) {
                case 'append':
                    // Add new field to end of section
                    if (!targetElement) {
                        throw new Error('targetElement required for append action');
                    }
                    targetElement.appendChild(newFieldCard);
                    console.log(`[FormBuilder] Field ${fieldId} appended to section`);
                    break;

                case 'replace':
                    // Replace existing field card
                    const existingCard = document.getElementById(`field-${fieldId}`);
                    if (existingCard) {
                        // Preserve selection state
                        const wasSelected = existingCard.classList.contains('selected-element');
                        if (wasSelected) {
                            newFieldCard.classList.add('selected-element');
                        }
                        existingCard.replaceWith(newFieldCard);
                        console.log(`[FormBuilder] Field ${fieldId} replaced in DOM`);
                    } else {
                        console.warn(`[FormBuilder] Field ${fieldId} not found for replace, appending instead`);
                        if (targetElement) {
                            targetElement.appendChild(newFieldCard);
                        }
                    }
                    break;

                case 'insertAfter':
                    // Insert after target element (for duplicate)
                    if (!targetElement) {
                        throw new Error('targetElement required for insertAfter action');
                    }
                    targetElement.after(newFieldCard);
                    console.log(`[FormBuilder] Field ${fieldId} inserted after target`);
                    break;

                default:
                    throw new Error(`Unknown action: ${action}`);
            }

            console.log(`[FormBuilder] ✅ Field ${fieldId} successfully rendered and inserted`);
            return newFieldCard;

        } catch (error) {
            console.error(`[FormBuilder] Error rendering field ${fieldId}:`, error);
            throw error;
        }
    }
};

/**
 * Save Form Builder Progress
 * Shows confirmation that changes are auto-saved
 */
function saveFormBuilderProgress() {
    // All changes are auto-saved via API calls
    // Just show success notification
    Swal.fire({
        icon: 'success',
        title: 'Progress Saved',
        text: 'All changes have been saved automatically.',
        timer: 2000,
        showConfirmButton: false
    });
}

/**
 * Continue to Next Stage (Review & Publish)
 * Validates form completeness using database state and navigates to next step
 * Step Order: Setup → Build → Publish (3-step wizard)
 */
async function continueToNextStage() {
    // Show loading state
    const btn = document.getElementById('continueToNextBtn');
    const originalHtml = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Validating...';
    
    try {
        // Validate using database state (Stage 2 = FormBuilder)
        const validation = await FormBuilder.validateStageCompletion(FormBuilder.templateId, 2);
        
        // Restore button state
        btn.disabled = false;
        btn.innerHTML = originalHtml;
        
        if (!validation.isValid) {
            // Build error details HTML
            let errorHtml = `<p>${validation.message}</p>
                           <small class="text-muted">Current state: ${validation.sectionsCount || 0} sections, ${validation.fieldsCount || 0} fields</small>`;

            // Add technical error details if available (for debugging)
            if (validation.error) {
                errorHtml += `<hr><div style="text-align: left; max-height: 200px; overflow-y: auto;">
                             <small><strong>Error Details:</strong><br>${validation.error}</small></div>`;
            }

            Swal.fire({
                icon: 'warning',
                title: 'Cannot Continue',
                html: errorHtml,
                confirmButtonText: 'OK',
                width: '600px'
            });
            return;
        }

        // Validation passed - show success info
        const result = await Swal.fire({
            icon: 'success',
            title: 'Ready to Continue!',
            html: `<p>Form Builder validation passed.</p>
                   <p class="text-muted mb-0">✓ ${validation.sectionsCount} section(s) with ${validation.fieldsCount} field(s)</p>
                   <p class="text-muted">✓ Progress: ${validation.completionPercentage}%</p>`,
            showCancelButton: true,
            confirmButtonText: 'Continue to Review & Publish',
            cancelButtonText: 'Stay Here'
        });

        if (result.isConfirmed) {
            // Navigate to Review & Publish (Step 3)
            window.location.href = `/FormTemplates/ReviewPublish/${FormBuilder.templateId}`;
        }
    } catch (error) {
        // Restore button state on error
        btn.disabled = false;
        btn.innerHTML = originalHtml;
        
        Swal.fire({
            icon: 'error',
            title: 'Validation Error',
            text: 'Unable to validate template. Please try again.',
            confirmButtonText: 'OK'
        });
    }
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function() {
    // Template ID will be set by inline script in view
    if (typeof TEMPLATE_ID !== 'undefined') {
        FormBuilder.init(TEMPLATE_ID);
    }
});
