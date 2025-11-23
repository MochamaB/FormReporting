/**
 * Form Builder - Layout Manager
 * Manages UI layout components (properties panel, section collapse, etc.)
 */

const FormBuilderLayout = {
    // Properties panel collapsed state
    propertiesCollapsed: false,

    /**
     * Initialize layout components
     */
    init: function() {
        console.log('FormBuilder Layout initialized');
    },

    /**
     * Toggle properties panel (right sidebar)
     */
    toggleProperties: function() {
        const panel = document.getElementById('propertiesPanel');
        const grid = document.getElementById('designGrid');
        const icon = document.getElementById('propertiesToggleIcon');

        if (!panel || !grid || !icon) {
            console.error('Properties panel elements not found');
            return;
        }

        this.propertiesCollapsed = !this.propertiesCollapsed;

        if (this.propertiesCollapsed) {
            panel.classList.add('collapsed');
            grid.classList.add('properties-collapsed');
            icon.classList.remove('ri-arrow-left-s-line');
            icon.classList.add('ri-arrow-right-s-line');
        } else {
            panel.classList.remove('collapsed');
            grid.classList.remove('properties-collapsed');
            icon.classList.remove('ri-arrow-right-s-line');
            icon.classList.add('ri-arrow-left-s-line');
        }
    },

    /**
     * Select a section (highlight and show properties)
     * @param {number} sectionId - Section ID to select
     */
    selectSection: function(sectionId) {
        // Remove previous selection
        document.querySelectorAll('.builder-section').forEach(section => {
            section.classList.remove('selected');
        });

        // Add selection to clicked section
        const section = document.getElementById('section-' + sectionId);
        if (section) {
            section.classList.add('selected');
            FormBuilder.selectedSectionId = sectionId;
            console.log('Selected section:', sectionId);

            // TODO: Load section properties in right panel
        }
    },

    /**
     * Toggle section collapse/expand
     * @param {number} sectionId - Section ID to toggle
     */
    toggleSectionCollapse: function(sectionId) {
        const body = document.getElementById('section-body-' + sectionId);
        const icon = document.getElementById('collapse-icon-' + sectionId);

        if (!body || !icon) {
            console.error('Section elements not found for ID:', sectionId);
            return;
        }

        if (body.style.display === 'none') {
            // Expand
            body.style.display = 'block';
            icon.classList.remove('ri-add-line');
            icon.classList.add('ri-subtract-line');
        } else {
            // Collapse
            body.style.display = 'none';
            icon.classList.remove('ri-subtract-line');
            icon.classList.add('ri-add-line');
        }
    }
};

// Expose functions globally for inline onclick handlers
window.toggleProperties = function() {
    FormBuilderLayout.toggleProperties();
};

window.selectSection = function(sectionId) {
    FormBuilderLayout.selectSection(sectionId);
};

window.toggleSectionCollapse = function(sectionId) {
    FormBuilderLayout.toggleSectionCollapse(sectionId);
};
