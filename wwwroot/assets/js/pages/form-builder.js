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
 * Continue to Next Stage (Metric Mapping)
 * Validates form completeness using database state and navigates to next step
 */
async function continueToNextStage() {
    // Show loading state
    const btn = document.getElementById('continueToMetricsBtn');
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
            confirmButtonText: 'Continue to Metric Mapping',
            cancelButtonText: 'Stay Here'
        });

        if (result.isConfirmed) {
            // Navigate to metric mapping
            window.location.href = `/Forms/FormTemplates/MetricMapping/${FormBuilder.templateId}`;
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
