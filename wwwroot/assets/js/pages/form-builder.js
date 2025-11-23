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
    }
};

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function() {
    // Template ID will be set by inline script in view
    if (typeof TEMPLATE_ID !== 'undefined') {
        FormBuilder.init(TEMPLATE_ID);
    }
});
