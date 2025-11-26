/**
 * Form Builder - Drag & Drop Manager
 * Handles SortableJS initialization and section reordering
 * Uses pure SortableJS groups to avoid conflicts
 */

const FormBuilderDragDrop = {
    // SortableJS instances
    toolboxSortable: null,
    sectionsSortable: null,
    fieldPaletteSortable: null,  // NEW: For field palette in toolbox
    fieldDropZones: [],           // NEW: Array of sortable instances for each section

    // Current template ID
    templateId: null,

    /**
     * Initialize drag-drop functionality
     * @param {number} templateId - Template ID for API calls
     */
    init: function(templateId) {
        this.templateId = templateId;
        this.initializeToolboxDragDrop();       // Section drag from toolbox
        this.initializeSectionDragDrop();        // Section drop and reorder
        this.initializeFieldPaletteDragDrop();   // NEW: Field drag from toolbox
        this.initializeFieldDropZones();         // NEW: Field drop into sections
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
                pull: false,           // Don't allow pulling sections out
                put: ['sections']      // ONLY accept items from 'sections' group (reject 'fields')
            },
            animation: 150,
            handle: '.drag-handle',           // Only drag by handle on existing sections
            draggable: '.builder-section',    // Only .builder-section elements can be dragged/reordered
            ghostClass: 'sortable-ghost',
            dragClass: 'sortable-drag',
            fallbackTolerance: 3,              // Better handling for nested sortables

            // CRITICAL: Filter out nested elements that should NOT be drop zones
            filter: '.fields-container, .card-body, .btn, .form-check-input, input, textarea, select',
            preventOnFilter: false,

            // Prevent fields from being dropped in sectionsContainer
            onMove: (evt) => {
                // Reject any item that's not a section (e.g., field items)
                return evt.dragged.classList.contains('builder-section') ||
                       evt.dragged.classList.contains('draggable-section');
            },

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
    },

    /**
     * Initialize field palette drag-drop in toolbox
     * Fields can be cloned and dragged to sections
     * Initializes SortableJS on each field-category individually
     */
    initializeFieldPaletteDragDrop: function() {
        // Find all field-category containers
        const fieldCategories = document.querySelectorAll('.field-palette-container .field-category');

        if (fieldCategories.length === 0) {
            console.warn('No field categories found - field palette drag-drop not initialized');
            return;
        }

        // Initialize SortableJS on each field category
        // This allows individual field items to be dragged from their categories
        fieldCategories.forEach((category) => {
            new Sortable(category, {
                group: {
                    name: 'fields',
                    pull: 'clone',    // Clone items from toolbox
                    put: false        // Don't allow items to be put back
                },
                sort: false,           // Disable sorting in toolbox
                draggable: '.draggable-field',  // Only field items are draggable
                animation: 150,
                ghostClass: 'sortable-ghost-field',

                // Filter out category headers
                filter: 'h6',

                onEnd: (evt) => {
                    // Remove the cloned element after drag ends
                    if (evt.item && evt.pullMode === 'clone') {
                        evt.item.remove();
                    }
                }
            });
        });

        console.log(`Field palette drag-drop initialized for ${fieldCategories.length} categories`);
    },

    /**
     * Initialize field drop zones for all sections
     * Creates sortable instances for each fields-container
     */
    initializeFieldDropZones: function() {
        const fieldsContainers = document.querySelectorAll('.fields-container');

        if (fieldsContainers.length === 0) {
            console.warn('No fields containers found - field drop zones not initialized');
            return;
        }

        fieldsContainers.forEach(container => {
            const sectionId = container.dataset.sectionId;

            const sortableInstance = new Sortable(container, {
                group: {
                    name: 'fields',
                    pull: false,        // Don't allow pulling fields out (for now)
                    put: ['fields']     // ONLY accept items from 'fields' group
                },
                animation: 150,
                ghostClass: 'sortable-ghost-field',
                draggable: '.builder-field-card',  // For dragging field cards (new and existing)
                fallbackTolerance: 3,          // Better handling for nested sortables
                handle: '.field-drag-handle',  // Only drag by handle

                // Only accept field items (from palette or reordering)
                onMove: (evt) => {
                    // Highlight drop zone when hovering
                    if (evt.to && evt.to.classList.contains('fields-container')) {
                        evt.to.classList.add('sortable-drag-over');
                    }

                    // Remove highlight from previous container
                    if (evt.from && evt.from !== evt.to && evt.from.classList.contains('fields-container')) {
                        evt.from.classList.remove('sortable-drag-over');
                    }

                    // Accept if it's a draggable field from palette or existing builder field card
                    return evt.dragged.classList.contains('draggable-field') ||
                           evt.dragged.classList.contains('builder-field-card');
                },

                // Called when field is dropped from palette into section
                onAdd: (evt) => {
                    // Remove drag-over class
                    container.classList.remove('sortable-drag-over');
                    const fieldType = evt.item.dataset.fieldType;

                    // Remove the dropped clone element
                    evt.item.remove();

                    // Remove empty state message if it exists
                    const emptyMessage = container.querySelector('.empty-fields-message');
                    if (emptyMessage) {
                        emptyMessage.remove();
                    }

                    // Open modal to configure the field
                    console.log(`Field ${fieldType} dropped into section ${sectionId}`);
                    FormBuilderFields.showAddFieldModal(null, sectionId, fieldType);
                },

                // Called when existing field is reordered within section (future)
                onUpdate: (_evt) => {
                    // Remove drag-over class
                    container.classList.remove('sortable-drag-over');

                    console.log(`Field reordered in section ${sectionId}`);
                    // TODO: Implement field reorder API call
                    // this.updateFieldOrder(sectionId);
                },

                // Clean up drag-over classes when drag ends (success or cancel)
                onEnd: (_evt) => {
                    // Remove drag-over class from all fields-containers
                    document.querySelectorAll('.fields-container').forEach(fc => {
                        fc.classList.remove('sortable-drag-over');
                    });
                }
            });

            this.fieldDropZones.push({
                sectionId: sectionId,
                instance: sortableInstance
            });
        });

        console.log(`Field drop zones initialized for ${fieldsContainers.length} sections`);
    },

    /**
     * Reinitialize field drop zones
     * Called when sections are added, deleted, or duplicated
     */
    reinitializeDropZones: function() {
        console.log('Reinitializing field drop zones...');

        // Destroy existing drop zone instances
        this.fieldDropZones.forEach(dropZone => {
            if (dropZone.instance) {
                dropZone.instance.destroy();
            }
        });
        this.fieldDropZones = [];

        // Reinitialize all drop zones
        this.initializeFieldDropZones();
    }
};
