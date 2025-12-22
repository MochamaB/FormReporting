/**
 * Form Builder - Section Management
 * Handles section CRUD operations (Create, Read, Update, Delete)
 */

const FormBuilderSections = {

    /**
     * Show add section modal
     */
    showAddSectionModal: function() {
        const modal = document.getElementById('addSectionModal');
        const form = document.getElementById('addSectionForm');

        if (!modal || !form) {
            console.error('Add section modal or form not found');
            return;
        }

        // Reset form
        form.reset();
        document.getElementById('isCollapsible').checked = true;
        document.getElementById('isCollapsedByDefault').checked = false;

        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    },

    /**
     * Save section from modal
     * Validates form and calls API to create section
     */
    saveSectionFromModal: async function(event, templateId) {
        const form = document.getElementById('addSectionForm');

        // Validate form
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
            return;
        }

        // Get form values
        const name = document.getElementById('sectionName').value.trim();
        const description = document.getElementById('sectionDescription').value.trim();
        const icon = document.getElementById('sectionIcon').value.trim();
        const columnLayout = parseInt(document.getElementById('sectionColumnLayout').value) || 1;
        const isCollapsible = document.getElementById('isCollapsible').checked;
        const isCollapsedByDefault = document.getElementById('isCollapsedByDefault').checked;

        // Get save button
        const saveBtn = event.target;
        const originalHtml = saveBtn.innerHTML;

        // Disable save button with loading state
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';

        try {
            const response = await fetch('/api/formbuilder/sections/add', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    templateId: templateId,
                    section: {
                        sectionName: name,
                        sectionDescription: description || null,
                        iconClass: icon || null,
                        columnLayout: columnLayout,
                        isCollapsible: isCollapsible,
                        isCollapsedByDefault: isCollapsedByDefault
                    }
                })
            });

            const result = await response.json();

            if (result.success) {
                console.log('Section added:', result.section);
                const newSectionId = result.section?.sectionId;

                // Reset button state before closing modal
                saveBtn.disabled = false;
                saveBtn.innerHTML = originalHtml;

                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('addSectionModal'));
                modal.hide();

                // Dynamically add section to canvas without page reload
                if (newSectionId) {
                    try {
                        // Hide empty state placeholder if it exists
                        const sectionsContainer = document.getElementById('sectionsContainer');
                        const emptyState = sectionsContainer?.querySelector('.text-center.py-5');
                        if (emptyState) {
                            emptyState.remove();
                            console.log('[AddSection] Empty state placeholder removed');
                        }

                        // Render and insert the new section
                        const newSectionCard = await FormBuilder.renderAndInsertSection(newSectionId, 'append');
                        console.log(`[AddSection] ✅ Section ${newSectionId} added to DOM`);

                        // Select the new section
                        if (typeof selectSection === 'function') {
                            selectSection(newSectionId);
                            console.log(`[AddSection] ✅ Section ${newSectionId} selected`);
                        }

                        // Show success notification
                        if (typeof FormBuilderDragDrop !== 'undefined' && FormBuilderDragDrop.showNotification) {
                            FormBuilderDragDrop.showNotification('Section added successfully', 'success');
                        }
                    } catch (error) {
                        console.error('[AddSection] Error rendering section:', error);
                        // Fallback to reload on error
                        FormBuilder.reload();
                    }
                } else {
                    // Fallback if no section ID returned
                    FormBuilder.reload();
                }
            } else {
                alert('Error: ' + (result.message || 'Failed to add section'));
                saveBtn.disabled = false;
                saveBtn.innerHTML = originalHtml;
            }
        } catch (error) {
            console.error('Error adding section:', error);
            alert('An error occurred while adding the section');
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalHtml;
        }
    },

    /**
     * Edit section
     * @param {number} sectionId - Section ID to edit
     */
    editSection: function(sectionId) {
        // TODO: Implement edit functionality
        alert('Edit Section functionality will be implemented next');
        console.log('Editing section:', sectionId);
    },

    /**
     * Delete section - Show confirmation modal
     * @param {number} sectionId - Section ID to delete
     */
    deleteSection: function(sectionId) {
        // Get section data from canvas
        const sectionCard = document.getElementById(`section-${sectionId}`);
        if (!sectionCard) {
            console.error('Section not found:', sectionId);
            return;
        }

        const sectionName = sectionCard.querySelector('.card-title')?.textContent || 'Unknown Section';
        const fieldCount = sectionCard.querySelectorAll('.fields-container .field-item')?.length || 0;

        // Populate modal with section info
        document.getElementById('deleteSectionName').textContent = sectionName;
        document.getElementById('deleteFieldCount').textContent = fieldCount;

        // Show/hide field warning
        const fieldWarning = document.getElementById('deleteWarningFields');
        if (fieldCount > 0) {
            fieldWarning.classList.remove('d-none');
        } else {
            fieldWarning.classList.add('d-none');
        }

        // Store section ID for confirmation
        this.pendingDeleteSectionId = sectionId;

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('deleteSectionModal'));
        modal.show();
    },

    /**
     * Confirm delete section - Called from modal
     */
    confirmDeleteSection: async function() {
        if (!this.pendingDeleteSectionId) {
            console.error('No section ID to delete');
            return;
        }

        const sectionId = this.pendingDeleteSectionId;
        const confirmBtn = document.getElementById('confirmDeleteBtn');
        const originalHtml = confirmBtn.innerHTML;

        // Show loading state
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Deleting...';

        try {
            const response = await fetch(`/api/formbuilder/sections/${sectionId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const result = await response.json();

            if (result.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('deleteSectionModal'));
                modal.hide();

                // Reload page to reflect changes
                // NOTE: Full page reload reinitializes all drop zones automatically
                // Future optimization: Remove section from DOM and call FormBuilderDragDrop.reinitializeDropZones()
                FormBuilder.reload();
            } else {
                alert('Error: ' + (result.message || 'Failed to delete section'));
                confirmBtn.disabled = false;
                confirmBtn.innerHTML = originalHtml;
            }
        } catch (error) {
            console.error('Error deleting section:', error);
            alert('An error occurred while deleting the section');
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = originalHtml;
        }
    },

    /**
     * Duplicate section - Show confirmation modal
     * @param {number} sectionId - Section ID to duplicate
     */
    duplicateSection: function(sectionId) {
        // Get section data from canvas
        const sectionCard = document.getElementById(`section-${sectionId}`);
        if (!sectionCard) {
            console.error('Section not found:', sectionId);
            return;
        }

        const sectionName = sectionCard.querySelector('.card-title')?.textContent || 'Unknown Section';
        const fieldCount = sectionCard.querySelectorAll('.fields-container .field-item')?.length || 0;

        // Populate modal with section info
        document.getElementById('duplicateSectionName').textContent = sectionName;
        document.getElementById('duplicateFieldCount').textContent = fieldCount;

        // Show/hide field info
        const fieldInfo = document.getElementById('duplicateInfoFields');
        if (fieldCount > 0) {
            fieldInfo.classList.remove('d-none');
        } else {
            fieldInfo.classList.add('d-none');
        }

        // Store section ID for confirmation
        this.pendingDuplicateSectionId = sectionId;

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('duplicateSectionModal'));
        modal.show();
    },

    /**
     * Confirm duplicate section - Called from modal
     */
    confirmDuplicateSection: async function() {
        if (!this.pendingDuplicateSectionId) {
            console.error('No section ID to duplicate');
            return;
        }

        const sectionId = this.pendingDuplicateSectionId;
        const confirmBtn = document.getElementById('confirmDuplicateBtn');
        const originalHtml = confirmBtn.innerHTML;

        // Show loading state
        confirmBtn.disabled = true;
        confirmBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Duplicating...';

        try {
            const response = await fetch(`/api/formbuilder/sections/${sectionId}/duplicate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const result = await response.json();

            if (result.success) {
                // Close modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('duplicateSectionModal'));
                modal.hide();

                // Reload page to show duplicated section
                // NOTE: Full page reload reinitializes all drop zones automatically
                // Future optimization: Add section to DOM and call FormBuilderDragDrop.reinitializeDropZones()
                FormBuilder.reload();
            } else {
                alert('Error: ' + (result.message || 'Failed to duplicate section'));
                confirmBtn.disabled = false;
                confirmBtn.innerHTML = originalHtml;
            }
        } catch (error) {
            console.error('Error duplicating section:', error);
            alert('An error occurred while duplicating the section');
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = originalHtml;
        }
    },

    // NOTE: Field-related functions have been moved to form-builder-fields.js
};

// Expose functions globally for inline onclick handlers
window.addSection = function() {
    FormBuilderSections.showAddSectionModal();
};

window.showAddSectionModal = function() {
    FormBuilderSections.showAddSectionModal();
};

window.saveSectionFromModal = function(event) {
    if (typeof TEMPLATE_ID !== 'undefined') {
        FormBuilderSections.saveSectionFromModal(event, TEMPLATE_ID);
    } else {
        console.error('TEMPLATE_ID not defined');
    }
};

window.editSection = function(sectionId) {
    FormBuilderSections.editSection(sectionId);
};

window.deleteSection = function(sectionId) {
    FormBuilderSections.deleteSection(sectionId);
};

window.confirmDeleteSection = function() {
    FormBuilderSections.confirmDeleteSection();
};

window.duplicateSection = function(sectionId) {
    FormBuilderSections.duplicateSection(sectionId);
};

window.confirmDuplicateSection = function() {
    FormBuilderSections.confirmDuplicateSection();
};

// NOTE: Field-related global functions are now in form-builder-fields.js
