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
     * Update field display order after drag-drop reordering
     * Sends new order to backend API
     * @param {number} sectionId - Section ID containing the fields
     */
    updateFieldOrder: async function(sectionId) {
        const container = document.querySelector(`.fields-container[data-section-id="${sectionId}"]`);

        if (!container) {
            console.error(`Fields container not found for section ${sectionId}`);
            return;
        }

        const fieldCards = container.querySelectorAll('.builder-field-card');
        const updates = [];

        fieldCards.forEach((card, index) => {
            const itemId = parseInt(card.dataset.fieldId);  // data-field-id contains ItemId
            if (itemId) {
                updates.push({
                    itemId: itemId,  // API expects ItemId property
                    displayOrder: index + 1
                });
            }
        });

        if (updates.length === 0) {
            console.warn('No fields to reorder');
            return;
        }

        try {
            const response = await fetch(`/api/formbuilder/sections/${sectionId}/fields/reorder`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updates)
            });

            const result = await response.json();

            if (result.success) {
                console.log('Field order updated successfully');
                // Optional: Show toast notification
                // You can add SweetAlert2 toast here if desired
            } else {
                console.error('Failed to update field order:', result.message);
                // Reload to restore correct order
                window.location.reload();
            }
        } catch (error) {
            console.error('Error updating field order:', error);
            // Reload to restore correct order on error
            window.location.reload();
        }
    },

    /**
     * Move a field from one section to another
     * Updates the field's section and reorders fields in both sections
     * @param {number} fieldId - Field ID being moved
     * @param {number} targetSectionId - New section ID
     * @param {HTMLElement} sourceContainer - Source fields-container
     * @param {HTMLElement} targetContainer - Target fields-container
     */
    moveFieldToSection: async function(fieldId, targetSectionId, sourceContainer, targetContainer) {
        const sourceSectionId = parseInt(sourceContainer.dataset.sectionId);

        console.log(`Moving field ${fieldId} from section ${sourceSectionId} to section ${targetSectionId}`);

        try {
            // First, update the field's section and get new orders for target section
            const targetFieldCards = targetContainer.querySelectorAll('.builder-field-card');
            const targetUpdates = [];

            targetFieldCards.forEach((card, index) => {
                const itemId = parseInt(card.dataset.fieldId);
                if (itemId) {
                    targetUpdates.push({
                        itemId: itemId,
                        displayOrder: index + 1
                    });
                }
            });

            // Update target section field orders (includes the moved field)
            const targetResponse = await fetch(`/api/formbuilder/sections/${targetSectionId}/fields/reorder`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(targetUpdates)
            });

            const targetResult = await targetResponse.json();

            if (!targetResult.success) {
                console.error('Failed to update target section:', targetResult.message);
                window.location.reload();
                return;
            }

            // Now update field's section ID in the database
            const moveResponse = await fetch(`/api/formbuilder/fields/${fieldId}/section`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    sectionId: targetSectionId
                })
            });

            if (!moveResponse.ok) {
                console.error('Failed to update field section');
                window.location.reload();
                return;
            }

            // Update source section field orders (remaining fields)
            const sourceFieldCards = sourceContainer.querySelectorAll('.builder-field-card');

            if (sourceFieldCards.length > 0) {
                const sourceUpdates = [];
                sourceFieldCards.forEach((card, index) => {
                    const itemId = parseInt(card.dataset.fieldId);
                    if (itemId) {
                        sourceUpdates.push({
                            itemId: itemId,
                            displayOrder: index + 1
                        });
                    }
                });

                const sourceResponse = await fetch(`/api/formbuilder/sections/${sourceSectionId}/fields/reorder`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(sourceUpdates)
                });

                const sourceResult = await sourceResponse.json();

                if (!sourceResult.success) {
                    console.error('Failed to update source section:', sourceResult.message);
                    window.location.reload();
                    return;
                }
            }

            console.log('Field moved successfully');
            // Reload to refresh field counts and empty states
            window.location.reload();

        } catch (error) {
            console.error('Error moving field:', error);
            window.location.reload();
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
                    pull: true,         // Allow pulling fields out for cross-section dragging
                    put: ['fields']     // ONLY accept items from 'fields' group
                },
                animation: 150,
                ghostClass: 'sortable-ghost-field',
                draggable: '.builder-field-card',  // For dragging field cards (new and existing)
                fallbackTolerance: 3,          // Better handling for nested sortables
                handle: '.field-drag-handle',  // Only drag by handle

                // Only accept field items (from palette or reordering)
                onMove: (evt) => {
                    // Highlight drop zone when hovering with different styles based on source
                    if (evt.to && evt.to.classList.contains('fields-container')) {
                        // Differentiate between new field (from palette) vs reordering existing field
                        if (evt.dragged.classList.contains('draggable-field')) {
                            // Dragging NEW field from palette - full container highlight
                            evt.to.classList.add('drag-over-new-field');
                            evt.to.classList.remove('drag-over-reorder');
                        } else if (evt.dragged.classList.contains('builder-field-card')) {
                            // Reordering EXISTING field - subtle highlight
                            evt.to.classList.add('drag-over-reorder');
                            evt.to.classList.remove('drag-over-new-field');
                        }
                    }

                    // Remove highlight from previous container
                    if (evt.from && evt.from !== evt.to && evt.from.classList.contains('fields-container')) {
                        evt.from.classList.remove('drag-over-new-field');
                        evt.from.classList.remove('drag-over-reorder');
                    }

                    // Accept if it's a draggable field from palette or existing builder field card
                    return evt.dragged.classList.contains('draggable-field') ||
                           evt.dragged.classList.contains('builder-field-card');
                },

                // Called when field is dropped into this section from another location
                onAdd: (evt) => {
                    // Remove drag-over classes
                    container.classList.remove('drag-over-new-field');
                    container.classList.remove('drag-over-reorder');

                    // Check if this is a NEW field from palette or EXISTING field from another section
                    const isExistingField = evt.item.classList.contains('builder-field-card');
                    const fieldId = evt.item.dataset.fieldId;

                    if (isExistingField && fieldId) {
                        // EXISTING field being moved from another section
                        console.log(`Field ${fieldId} moved to section ${sectionId}`);

                        // Remove empty state message if it exists
                        const emptyMessage = container.querySelector('.empty-fields-message');
                        if (emptyMessage) {
                            emptyMessage.remove();
                        }

                        // Update the field's section in database and reorder both sections
                        this.moveFieldToSection(parseInt(fieldId), parseInt(sectionId), evt.from, evt.to);
                    } else {
                        // NEW field from palette
                        const fieldType = evt.item.dataset.fieldType;

                        // Remove the dropped clone element
                        evt.item.remove();

                        // Remove empty state message if it exists
                        const emptyMessage = container.querySelector('.empty-fields-message');
                        if (emptyMessage) {
                            emptyMessage.remove();
                        }

                        // Open modal to configure the field
                        console.log(`New field ${fieldType} dropped into section ${sectionId}`);
                        FormBuilderFields.showAddFieldModal(null, sectionId, fieldType);
                    }
                },

                // Called when existing field is reordered within same section
                onUpdate: (_evt) => {
                    // Remove drag-over classes
                    container.classList.remove('drag-over-new-field');
                    container.classList.remove('drag-over-reorder');

                    console.log(`Field reordered in section ${sectionId}`);
                    // Save the new order to database
                    this.updateFieldOrder(sectionId);
                },

                // Clean up drag-over classes when drag ends (success or cancel)
                onEnd: (_evt) => {
                    // Remove all drag-over classes from all fields-containers
                    document.querySelectorAll('.fields-container').forEach(fc => {
                        fc.classList.remove('drag-over-new-field');
                        fc.classList.remove('drag-over-reorder');
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
