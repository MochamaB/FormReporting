/**
 * Form Builder - Drag & Drop Manager
 * Handles SortableJS initialization and section reordering
 * Uses pure SortableJS groups to avoid conflicts
 */

const FormBuilderDragDrop = {
    // SortableJS instances
    toolboxSortable: null,
    sectionsSortable: null,

    // Current template ID
    templateId: null,

    /**
     * Initialize drag-drop functionality
     * @param {number} templateId - Template ID for API calls
     */
    init: function(templateId) {
        this.templateId = templateId;
        this.initializeToolboxDragDrop();
        this.initializeSectionDragDrop();
        console.log('FormBuilder Drag & Drop initialized');
    },

    /**
     * Initialize toolbox drag-drop using SortableJS
     * Toolbox section can be cloned to canvas
     */
    initializeToolboxDragDrop: function() {
        const draggableSection = document.querySelector('.draggable-section');

        if (!draggableSection) {
            console.warn('Toolbox section template not found');
            return;
        }

        // Get parent container of draggable section
        const parentContainer = draggableSection.parentElement;

        // Initialize SortableJS on parent container
        this.toolboxSortable = new Sortable(parentContainer, {
            group: {
                name: 'sections',
                pull: 'clone',  // Clone items from toolbox (don't remove)
                put: false      // Don't allow items to be put back
            },
            sort: false,  // Disable sorting in toolbox
            animation: 150,

            onEnd: (evt) => {
                // When cloned to canvas
                if (evt.to.id === 'sectionsContainer' && evt.pullMode === 'clone') {
                    // Remove the cloned element immediately
                    if (evt.item && evt.item.parentNode) {
                        evt.item.remove();
                    }
                    // Show add section modal
                    FormBuilderSections.showAddSectionModal();
                }
            }
        });

        console.log('Toolbox drag-drop initialized');
    },

    /**
     * Initialize section drag-drop using SortableJS
     * Allows reordering of existing sections
     */
    initializeSectionDragDrop: function() {
        const sectionsContainer = document.getElementById('sectionsContainer');

        if (!sectionsContainer) {
            console.warn('Sections container not found - drag-drop not initialized');
            return;
        }

        // Make sections container sortable (for reordering)
        this.sectionsSortable = new Sortable(sectionsContainer, {
            group: {
                name: 'sections',
                pull: false,    // Don't allow pulling sections out
                put: true       // Allow sections from toolbox
            },
            animation: 150,
            handle: '.drag-handle',           // Only drag by handle on existing sections
            draggable: '.builder-section',    // Only .builder-section elements can be dragged/reordered
            ghostClass: 'sortable-ghost',
            dragClass: 'sortable-drag',

            // CRITICAL: Filter out nested elements that should NOT be drop zones
            filter: '.fields-container, .card-body, .btn, .form-check-input, input, textarea, select',
            preventOnFilter: false,

            onEnd: (evt) => {
                // Only update order if this was a reorder (not a clone from toolbox)
                if (evt.pullMode !== 'clone') {
                    // Check if item is a real section (has data-section-id)
                    if (evt.item && evt.item.dataset.sectionId) {
                        this.updateSectionOrder();
                    }
                }
            }
        });

        console.log('Canvas sections drag-drop initialized');
    },

    /**
     * Update section display order after drag-drop reordering
     * Sends new order to backend API
     */
    updateSectionOrder: async function() {
        const sections = document.querySelectorAll('.builder-section');
        const updates = [];

        sections.forEach((section, index) => {
            const sectionId = parseInt(section.dataset.sectionId);
            updates.push({
                sectionId: sectionId,
                displayOrder: index + 1
            });
        });

        try {
            const response = await fetch('/api/formbuilder/sections/reorder', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    templateId: this.templateId,
                    sections: updates
                })
            });

            const result = await response.json();

            if (result.success) {
                console.log('Section order updated successfully');
                // Optional: Show toast notification
                // You can add SweetAlert2 toast here if desired
            } else {
                console.error('Failed to update section order');
                // Reload to restore correct order
                FormBuilder.reload();
            }
        } catch (error) {
            console.error('Error updating section order:', error);
            // Reload to restore correct order on error
            FormBuilder.reload();
        }
    }
};
