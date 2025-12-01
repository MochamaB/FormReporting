/**
 * FORM BUILDER SUMMARY MODULE
 * Manages the Summary tab with hierarchy tree and details panel
 * Handles drag-drop reordering for sections and fields
 */

const FormBuilderSummary = {
    // State
    templateData: null,
    currentSelection: null, // { type: 'section'|'field', id: number }

    /**
     * Initialize the Summary module
     */
    init: function() {
        console.log('[FormBuilderSummary] Initializing...');

        // Load embedded data
        this.loadEmbeddedData();

        // Initialize drag-drop if data exists
        if (this.templateData && this.templateData.sections && this.templateData.sections.length > 0) {
            this.initializeDragDrop();
        }

        // Set initial max-height for fields lists
        this.initializeFieldsLists();

        console.log('[FormBuilderSummary] Initialized successfully');
    },

    /**
     * Load template data from embedded JSON
     */
    loadEmbeddedData: function() {
        const dataElement = document.getElementById('summary-data');
        if (dataElement) {
            try {
                this.templateData = JSON.parse(dataElement.textContent);
                console.log('[FormBuilderSummary] Loaded template data:', this.templateData);
            } catch (error) {
                console.error('[FormBuilderSummary] Failed to parse template data:', error);
            }
        }
    },

    /**
     * Initialize max-height for fields lists (for collapse animation)
     */
    initializeFieldsLists: function() {
        const fieldsLists = document.querySelectorAll('.fields-list');
        fieldsLists.forEach(list => {
            if (!list.closest('.section-item').classList.contains('collapsed')) {
                list.style.maxHeight = list.scrollHeight + 'px';
            }
        });
    },

    /**
     * Initialize drag-drop for sections and fields
     */
    initializeDragDrop: function() {
        this.initializeSectionDragDrop();
        this.initializeFieldDragDrop();
    },

    /**
     * Initialize drag-drop for sections
     */
    initializeSectionDragDrop: function() {
        const sectionsList = document.getElementById('sections-list');
        if (!sectionsList) return;

        Sortable.create(sectionsList, {
            animation: 150,
            handle: '.section-drag-handle',
            ghostClass: 'sortable-ghost',
            chosenClass: 'sortable-chosen',
            dragClass: 'sortable-drag',

            onEnd: (evt) => {
                console.log('[FormBuilderSummary] Section reordered:', evt.oldIndex, '->', evt.newIndex);
                this.saveSectionOrder();
            }
        });
    },

    /**
     * Initialize drag-drop for fields within each section
     */
    initializeFieldDragDrop: function() {
        const fieldsLists = document.querySelectorAll('.fields-list');

        fieldsLists.forEach(list => {
            Sortable.create(list, {
                animation: 150,
                handle: '.field-drag-handle',
                ghostClass: 'sortable-ghost',
                chosenClass: 'sortable-chosen',
                dragClass: 'sortable-drag',
                group: 'fields', // Allow moving between sections

                onEnd: (evt) => {
                    const sectionId = evt.to.dataset.sectionId;
                    console.log('[FormBuilderSummary] Field reordered in section:', sectionId);

                    // Check if moved to different section
                    if (evt.from !== evt.to) {
                        const fieldId = evt.item.dataset.fieldId;
                        console.log('[FormBuilderSummary] Field moved to different section:', fieldId, '->', sectionId);
                        this.moveFieldToSection(fieldId, sectionId);
                    } else {
                        this.saveFieldOrder(sectionId);
                    }
                }
            });
        });
    },

    /**
     * Save section order after drag-drop
     */
    saveSectionOrder: async function() {
        const sectionItems = document.querySelectorAll('#sections-list .section-item');
        const sections = [];

        sectionItems.forEach((item, index) => {
            sections.push({
                sectionId: parseInt(item.dataset.sectionId),
                displayOrder: index + 1
            });
        });

        console.log('[FormBuilderSummary] Saving section order:', sections);

        try {
            const response = await fetch('/api/formbuilder/sections/reorder', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    templateId: this.templateData.templateId,
                    sections: sections
                })
            });

            const result = await response.json();

            if (result.success) {
                console.log('[FormBuilderSummary] Section order saved successfully');
                this.showNotification('Section order updated', 'success');
            } else {
                console.error('[FormBuilderSummary] Failed to save section order:', result.message);
                this.showNotification('Failed to update section order', 'error');
            }
        } catch (error) {
            console.error('[FormBuilderSummary] Error saving section order:', error);
            this.showNotification('Error updating section order', 'error');
        }
    },

    /**
     * Save field order after drag-drop within same section
     */
    saveFieldOrder: async function(sectionId) {
        const fieldsList = document.querySelector(`.fields-list[data-section-id="${sectionId}"]`);
        const fieldItems = fieldsList.querySelectorAll('.field-item');
        const updates = [];

        fieldItems.forEach((item, index) => {
            updates.push({
                itemId: parseInt(item.dataset.fieldId),
                displayOrder: index + 1
            });
        });

        console.log('[FormBuilderSummary] Saving field order for section', sectionId, ':', updates);

        try {
            const response = await fetch(`/api/formbuilder/sections/${sectionId}/fields/reorder`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updates)
            });

            const result = await response.json();

            if (result.success) {
                console.log('[FormBuilderSummary] Field order saved successfully');
                this.showNotification('Field order updated', 'success');
            } else {
                console.error('[FormBuilderSummary] Failed to save field order:', result.message);
                this.showNotification('Failed to update field order', 'error');
            }
        } catch (error) {
            console.error('[FormBuilderSummary] Error saving field order:', error);
            this.showNotification('Error updating field order', 'error');
        }
    },

    /**
     * Move field to different section
     */
    moveFieldToSection: async function(fieldId, targetSectionId) {
        console.log('[FormBuilderSummary] Moving field', fieldId, 'to section', targetSectionId);

        try {
            const response = await fetch(`/api/formbuilder/fields/${fieldId}/section`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ sectionId: parseInt(targetSectionId) })
            });

            const result = await response.json();

            if (result.success) {
                console.log('[FormBuilderSummary] Field moved successfully');
                this.showNotification('Field moved to different section', 'success');

                // Update field order in new section
                this.saveFieldOrder(targetSectionId);
            } else {
                console.error('[FormBuilderSummary] Failed to move field:', result.message);
                this.showNotification('Failed to move field', 'error');

                // Reload page to revert UI
                location.reload();
            }
        } catch (error) {
            console.error('[FormBuilderSummary] Error moving field:', error);
            this.showNotification('Error moving field', 'error');
            location.reload();
        }
    },

    /**
     * Select a section and show its details
     */
    selectSection: function(sectionId) {
        console.log('[FormBuilderSummary] Selecting section:', sectionId);

        // Clear previous selection
        this.clearSelectionHighlight();

        // Set current selection
        this.currentSelection = { type: 'section', id: sectionId };

        // Highlight selected section
        const sectionItem = document.querySelector(`.section-item[data-section-id="${sectionId}"]`);
        if (sectionItem) {
            sectionItem.classList.add('selected');
        }

        // Hide all detail panels
        document.getElementById('details-empty').style.display = 'none';
        document.getElementById('details-section').style.display = 'block';
        document.getElementById('details-field').style.display = 'none';

        // Load section details
        this.showSectionDetails(sectionId);
    },

    /**
     * Select a field and show its details
     */
    selectField: function(fieldId) {
        console.log('[FormBuilderSummary] Selecting field:', fieldId);

        // Clear previous selection
        this.clearSelectionHighlight();

        // Set current selection
        this.currentSelection = { type: 'field', id: fieldId };

        // Highlight selected field
        const fieldItem = document.querySelector(`.field-item[data-field-id="${fieldId}"]`);
        if (fieldItem) {
            fieldItem.classList.add('selected');
        }

        // Hide all detail panels
        document.getElementById('details-empty').style.display = 'none';
        document.getElementById('details-section').style.display = 'none';
        document.getElementById('details-field').style.display = 'block';

        // Load field details
        this.showFieldDetails(fieldId);
    },

    /**
     * Clear selection and show empty state
     */
    clearSelection: function() {
        this.clearSelectionHighlight();
        this.currentSelection = null;

        document.getElementById('details-empty').style.display = 'block';
        document.getElementById('details-section').style.display = 'none';
        document.getElementById('details-field').style.display = 'none';
    },

    /**
     * Clear visual selection highlight
     */
    clearSelectionHighlight: function() {
        document.querySelectorAll('.section-item.selected, .field-item.selected').forEach(item => {
            item.classList.remove('selected');
        });
    },

    /**
     * Toggle section collapse/expand
     */
    toggleSection: function(sectionId) {
        const sectionItem = document.querySelector(`.section-item[data-section-id="${sectionId}"]`);
        const fieldsList = sectionItem.querySelector('.fields-list');

        if (!fieldsList) return;

        if (sectionItem.classList.contains('collapsed')) {
            // Expand
            sectionItem.classList.remove('collapsed');
            fieldsList.style.maxHeight = fieldsList.scrollHeight + 'px';
        } else {
            // Collapse
            sectionItem.classList.add('collapsed');
            fieldsList.style.maxHeight = '0';
        }
    },

    /**
     * Expand all sections
     */
    expandAll: function() {
        document.querySelectorAll('.section-item').forEach(item => {
            const fieldsList = item.querySelector('.fields-list');
            if (fieldsList) {
                item.classList.remove('collapsed');
                fieldsList.style.maxHeight = fieldsList.scrollHeight + 'px';
            }
        });
    },

    /**
     * Collapse all sections
     */
    collapseAll: function() {
        document.querySelectorAll('.section-item').forEach(item => {
            const fieldsList = item.querySelector('.fields-list');
            if (fieldsList) {
                item.classList.add('collapsed');
                fieldsList.style.maxHeight = '0';
            }
        });
    },

    /**
     * Show section details in right panel
     */
    showSectionDetails: function(sectionId) {
        // Find section data
        const section = this.templateData.sections.find(s => s.sectionId === sectionId);
        if (!section) return;

        // Update header
        document.getElementById('section-details-name').textContent = section.sectionName;
        document.getElementById('section-details-code').textContent = section.sectionCode;

        // Render General tab
        this.renderSectionGeneralTab(section);

        // Render Display tab
        this.renderSectionDisplayTab(section);

        // Render Fields tab
        this.renderSectionFieldsTab(section);
    },

    /**
     * Render Section General Tab
     */
    renderSectionGeneralTab: function(section) {
        const content = document.getElementById('section-general-content');
        content.innerHTML = `
            <div class="info-card">
                <div class="info-card-label">Section Name</div>
                <div class="info-card-value">${section.sectionName}</div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Section Code</div>
                <div class="info-card-value">${section.sectionCode}</div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Display Order</div>
                <div class="info-card-value">${section.displayOrder}</div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Total Fields</div>
                <div class="info-card-value">${section.fieldCount}</div>
            </div>
        `;
    },

    /**
     * Render Section Display Tab
     */
    renderSectionDisplayTab: function(section) {
        const content = document.getElementById('section-display-content');
        content.innerHTML = `
            <div class="info-card">
                <div class="info-card-label">Collapsible</div>
                <div class="info-card-value">
                    <span class="badge ${section.isCollapsible ? 'bg-success' : 'bg-secondary'}">
                        ${section.isCollapsible ? 'Yes' : 'No'}
                    </span>
                </div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Collapsed by Default</div>
                <div class="info-card-value">
                    <span class="badge ${section.isCollapsed ? 'bg-success' : 'bg-secondary'}">
                        ${section.isCollapsed ? 'Yes' : 'No'}
                    </span>
                </div>
            </div>
        `;
    },

    /**
     * Render Section Fields Tab
     */
    renderSectionFieldsTab: function(section) {
        const content = document.getElementById('section-fields-content');

        if (section.fields.length === 0) {
            content.innerHTML = `
                <div class="text-center py-4">
                    <i class="ri-inbox-line display-4 text-muted"></i>
                    <p class="text-muted mt-3">No fields in this section</p>
                </div>
            `;
            return;
        }

        let html = '<div class="list-group">';
        section.fields.forEach(field => {
            html += `
                <div class="list-group-item">
                    <div class="d-flex align-items-center">
                        <div class="flex-grow-1">
                            <h6 class="mb-1">${field.itemName}</h6>
                            <small class="text-muted">${field.itemCode} • ${field.dataType}</small>
                        </div>
                        ${field.isRequired ? '<span class="badge bg-danger">Required</span>' : ''}
                    </div>
                </div>
            `;
        });
        html += '</div>';

        content.innerHTML = html;
    },

    /**
     * Show field details in right panel
     */
    showFieldDetails: function(fieldId) {
        // Find field data across all sections
        let field = null;
        for (const section of this.templateData.sections) {
            field = section.fields.find(f => f.itemId === fieldId);
            if (field) break;
        }

        if (!field) return;

        // Update header
        document.getElementById('field-details-name').textContent = field.itemName;
        document.getElementById('field-details-code').textContent = field.itemCode;

        // Render General tab
        this.renderFieldGeneralTab(field);

        // Render Validation tab
        this.renderFieldValidationTab(field);

        // Render Options tab
        this.renderFieldOptionsTab(field);
    },

    /**
     * Render Field General Tab
     */
    renderFieldGeneralTab: function(field) {
        const content = document.getElementById('field-general-content');
        content.innerHTML = `
            <div class="info-card">
                <div class="info-card-label">Field Name</div>
                <div class="info-card-value">${field.itemName}</div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Field Code</div>
                <div class="info-card-value">${field.itemCode}</div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Data Type</div>
                <div class="info-card-value">
                    <span class="badge bg-info">${field.dataType}</span>
                </div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Required</div>
                <div class="info-card-value">
                    <span class="badge ${field.isRequired ? 'bg-danger' : 'bg-secondary'}">
                        ${field.isRequired ? 'Yes' : 'No'}
                    </span>
                </div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Display Order</div>
                <div class="info-card-value">${field.displayOrder}</div>
            </div>
        `;
    },

    /**
     * Render Field Validation Tab
     */
    renderFieldValidationTab: function(field) {
        const content = document.getElementById('field-validation-content');
        content.innerHTML = `
            <div class="info-card">
                <div class="info-card-label">Validation Rules</div>
                <div class="info-card-value">
                    ${field.hasValidation ?
                        '<span class="badge bg-success"><i class="ri-check-line me-1"></i>Active</span>' :
                        '<span class="badge bg-secondary">None</span>'}
                </div>
            </div>

            <div class="info-card">
                <div class="info-card-label">Conditional Logic</div>
                <div class="info-card-value">
                    ${field.hasConditionalLogic ?
                        '<span class="badge bg-warning"><i class="ri-git-branch-line me-1"></i>Active</span>' :
                        '<span class="badge bg-secondary">None</span>'}
                </div>
            </div>
        `;
    },

    /**
     * Render Field Options Tab
     */
    renderFieldOptionsTab: function(field) {
        const content = document.getElementById('field-options-content');

        const hasOptions = ['Dropdown', 'Radio', 'Checkbox', 'MultiSelect'].includes(field.dataType);

        if (!hasOptions) {
            content.innerHTML = `
                <div class="alert alert-info">
                    <i class="ri-information-line me-2"></i>
                    This field type does not support options
                </div>
            `;
            return;
        }

        content.innerHTML = `
            <div class="info-card">
                <div class="info-card-label">Total Options</div>
                <div class="info-card-value">
                    <span class="badge bg-primary">${field.optionCount}</span>
                </div>
            </div>
        `;
    },

    /**
     * Show notification toast
     */
    showNotification: function(message, type = 'info') {
        // Simple console notification for now
        // You can integrate with your existing notification system
        console.log(`[Notification ${type}]:`, message);

        // TODO: Integrate with Velzon toast notifications if available
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Only initialize if on Summary tab
    const summaryTab = document.getElementById('builder-summary');
    if (summaryTab) {
        // Initialize immediately if tab is active, otherwise on tab show
        if (summaryTab.classList.contains('active')) {
            FormBuilderSummary.init();
        } else {
            // Listen for tab activation
            const summaryTabLink = document.querySelector('a[href="#builder-summary"]');
            if (summaryTabLink) {
                summaryTabLink.addEventListener('shown.bs.tab', function() {
                    FormBuilderSummary.init();
                });
            }
        }
    }
});
