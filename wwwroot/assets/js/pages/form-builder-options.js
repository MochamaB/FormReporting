/**
 * FormBuilderTemplateOptions
 * Handles field options management and template application
 */
var FormBuilderTemplateOptions = (function() {
    console.log('✅ FormBuilderTemplateOptions module loading...');

    let currentFieldId = null;
    let availableTemplates = [];
    let selectedTemplate = null;

    /**
     * Initialize options manager for a field
     */
    async function init(fieldId, fieldType, existingOptions) {
        console.log(`[FormBuilderTemplateOptions] Initializing for field ${fieldId}, type ${fieldType}`);
        currentFieldId = fieldId;

        // Reset selected template
        selectedTemplate = null;
        const selector = document.getElementById('template-selector');
        if (selector) {
            selector.value = '';
        }

        // Load templates for this field type
        loadTemplates(fieldType);

        // Render options table from server (preferred) or fallback to client-side
        try {
            await renderOptionsTableFromServer();
        } catch (error) {
            console.warn('[FormBuilderTemplateOptions] Server render failed, using client-side fallback');
            renderOptionsTable(existingOptions || []);
        }

        // Attach event listeners (only once)
        if (!selector || !selector.hasAttribute('data-initialized')) {
            attachEventListeners();
            if (selector) {
                selector.setAttribute('data-initialized', 'true');
            }
        }

        console.log(`[FormBuilderTemplateOptions] Initialization complete`);
    }

    /**
     * Load available templates (all active templates work for any option-based field)
     */
    async function loadTemplates(fieldType) {
        try {
            console.log(`[FormBuilderTemplateOptions] Loading templates for field type: ${fieldType}`);
            const url = `/api/formbuilder/option-templates`;
            console.log(`[FormBuilderTemplateOptions] Fetching from: ${url}`);

            const response = await fetch(url);
            console.log(`[FormBuilderTemplateOptions] Response status: ${response.status}`);

            if (!response.ok) {
                const errorText = await response.text();
                console.error(`[FormBuilderTemplateOptions] Error response:`, errorText);
                throw new Error(`Failed to load templates: ${response.status}`);
            }

            const result = await response.json();
            console.log(`[FormBuilderTemplateOptions] API Result:`, result);

            availableTemplates = result.templates || [];
            console.log(`[FormBuilderTemplateOptions] Loaded ${availableTemplates.length} templates:`, availableTemplates);

            populateTemplateSelector();
        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error loading templates:', error);
            showNotification('Failed to load option templates', 'error');
        }
    }

    /**
     * Populate template selector dropdown
     */
    function populateTemplateSelector() {
        const selector = document.getElementById('template-selector');
        console.log(`[FormBuilderTemplateOptions] Populating template selector, element found:`, !!selector);

        if (!selector) {
            console.error('[FormBuilderTemplateOptions] template-selector element not found in DOM!');
            return;
        }

        // Clear existing options
        selector.innerHTML = '<option value="">-- Select Template --</option>';
        console.log(`[FormBuilderTemplateOptions] Cleared selector, adding ${availableTemplates.length} templates`);

        if (availableTemplates.length === 0) {
            console.warn('[FormBuilderTemplateOptions] No templates available to populate');
            const option = document.createElement('option');
            option.textContent = '-- No templates available --';
            option.disabled = true;
            selector.appendChild(option);
            return;
        }

        // Group by category
        const grouped = availableTemplates.reduce((acc, template) => {
            const category = template.category || 'Other';
            if (!acc[category]) acc[category] = [];
            acc[category].push(template);
            return acc;
        }, {});

        console.log(`[FormBuilderTemplateOptions] Grouped templates by category:`, grouped);

        // Add optgroups
        Object.keys(grouped).sort().forEach(category => {
            const optgroup = document.createElement('optgroup');
            optgroup.label = category;

            grouped[category].forEach(template => {
                const option = document.createElement('option');
                option.value = template.templateId;
                option.textContent = `${template.templateName} (${template.itemCount} options)`;
                option.dataset.template = JSON.stringify(template);
                optgroup.appendChild(option);
            });

            selector.appendChild(optgroup);
        });

        console.log(`[FormBuilderTemplateOptions] Template selector populated with ${selector.options.length} options`);
    }

    /**
     * Attach event listeners - Auto-apply template on selection
     */
    function attachEventListeners() {
        const selector = document.getElementById('template-selector');
        if (selector && !selector.hasAttribute('data-initialized')) {
            selector.addEventListener('change', async function() {
                const templateId = this.value;

                // If empty selection (user chose "-- Select Template --"), just return
                if (!templateId) {
                    selectedTemplate = null;
                    return;
                }

                // Find selected template
                selectedTemplate = availableTemplates.find(t => t.templateId == templateId);
                if (!selectedTemplate) {
                    console.error('[FormBuilderTemplateOptions] Selected template not found');
                    return;
                }

                // Auto-apply immediately
                await applyTemplateImmediately(selectedTemplate);
            });

            selector.setAttribute('data-initialized', 'true');
        }
    }

    /**
     * Auto-apply template immediately when selected (no confirmation)
     */
    async function applyTemplateImmediately(template) {
        if (!template || !currentFieldId) return;

        // Get UI elements
        const selector = document.getElementById('template-selector');
        const loadingSpinner = document.getElementById('template-loading');

        // Show loading state
        if (selector) selector.disabled = true;
        if (loadingSpinner) loadingSpinner.style.display = 'inline-block';

        try {
            console.log(`[FormBuilderTemplateOptions] Auto-applying template ${template.templateId} to field ${currentFieldId}`);

            // Call API to apply template
            const response = await fetch(
                `/api/formbuilder/fields/${currentFieldId}/apply-template/${template.templateId}`,
                {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                }
            );

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to apply template');
            }

            const result = await response.json();
            console.log('[FormBuilderTemplateOptions] Template applied successfully:', result);

            // Re-render options table from server (partial view)
            await renderOptionsTableFromServer();

            // Update canvas field preview without page reload
            await updateCanvasFieldPreview();

            // Show success notification
            showNotification(
                `Applied "${template.templateName}" with ${result.field.options.length} options`,
                'success'
            );

        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error applying template:', error);
            showNotification(error.message || 'Failed to apply template', 'error');

            // Reset selector on error
            if (selector) selector.value = '';

        } finally {
            // Hide loading state
            if (selector) selector.disabled = false;
            if (loadingSpinner) loadingSpinner.style.display = 'none';
        }
    }

    /**
     * Render options table from server (fetches partial view HTML)
     * This is the preferred method - uses server-rendered Razor partial
     */
    async function renderOptionsTableFromServer() {
        if (!currentFieldId) {
            console.error('[FormBuilderTemplateOptions] No field ID set for rendering');
            return;
        }

        try {
            console.log(`[FormBuilderTemplateOptions] Fetching rendered options table for field ${currentFieldId}`);

            const response = await fetch(`/api/formbuilder/fields/${currentFieldId}/options/render`);

            if (!response.ok) {
                throw new Error(`Failed to fetch options table: ${response.status}`);
            }

            const result = await response.json();

            if (result.success && result.html) {
                // Find the wrapper container and replace its contents
                const wrapper = document.getElementById('options-table-wrapper');

                if (wrapper) {
                    wrapper.innerHTML = result.html;
                    
                    // Re-initialize sortable after rendering
                    initializeSortable();
                    
                    console.log(`[FormBuilderTemplateOptions] ✅ Rendered ${result.optionCount} options from server`);
                } else {
                    console.error('[FormBuilderTemplateOptions] options-table-wrapper not found');
                }
            }
        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error rendering options table from server:', error);
            // Fallback to client-side rendering if server fails
            showNotification('Failed to load options table', 'error');
        }
    }

    /**
     * Render options table (client-side fallback)
     * Used when server rendering is not available
     */
    function renderOptionsTable(options) {
        const tbody = document.getElementById('options-list');
        if (!tbody) {
            console.error('[FormBuilderTemplateOptions] options-list tbody not found!');
            return;
        }

        console.log(`[FormBuilderTemplateOptions] Rendering ${options.length} options (client-side)`);

        if (options.length === 0) {
            tbody.innerHTML = `
                <tr id="options-empty-state" class="border-bottom">
                    <td colspan="7" class="text-center text-muted py-4">
                        <i class="ri-inbox-line d-block mb-2" style="font-size: 2rem; opacity: 0.5;"></i>
                        <p class="mb-0">No options available</p>
                        <small>Add an option or apply a template above</small>
                    </td>
                </tr>
            `;
        } else {
            let html = '';
            options.forEach((option, index) => {
                html += `
                <tr data-option-id="${option.optionId}" data-index="${index}" class="border-bottom">
                    <td class="text-center">
                        <i class="ri-draggable" style="cursor: move; color: #6c757d;"></i>
                    </td>
                    <td class="text-center fw-medium row-number">
                        ${index + 1}
                    </td>
                    <td data-column="OptionValue">
                        <input type="text"
                               class="form-control form-control-sm"
                               value="${escapeHtml(option.optionValue)}"
                               placeholder="e.g., yes"
                               required
                               onchange="FormBuilderTemplateOptions.updateOption(${option.optionId}, 'value', this.value)">
                    </td>
                    <td data-column="OptionLabel">
                        <input type="text"
                               class="form-control form-control-sm"
                               value="${escapeHtml(option.optionLabel)}"
                               placeholder="e.g., Yes"
                               required
                               onchange="FormBuilderTemplateOptions.updateOption(${option.optionId}, 'label', this.value)">
                    </td>
                    <td class="text-center" data-column="ScoreValue">
                        <input type="number"
                               class="form-control form-control-sm"
                               value="${option.scoreValue !== null && option.scoreValue !== undefined ? option.scoreValue : ''}"
                               step="0.01"
                               placeholder="0"
                               style="max-width: 80px; margin: 0 auto;"
                               onchange="FormBuilderTemplateOptions.updateOption(${option.optionId}, 'score', this.value)">
                    </td>
                    <td class="text-center" data-column="IsDefault">
                        <input type="checkbox"
                               class="form-check-input"
                               ${option.isDefault ? 'checked' : ''}
                               onchange="FormBuilderTemplateOptions.setDefaultOption(${option.optionId}, this.checked)">
                    </td>
                    <td class="text-center">
                        <button type="button"
                                class="btn btn-sm btn-soft-danger"
                                onclick="FormBuilderTemplateOptions.deleteOption(${option.optionId})"
                                ${options.length <= 2 ? 'disabled title="Minimum 2 options required"' : ''}>
                            <i class="ri-delete-bin-line"></i>
                        </button>
                    </td>
                </tr>
                `;
            });
            tbody.innerHTML = html;
        }

        // Initialize drag-drop if Sortable is available
        initializeSortable();
        console.log(`[FormBuilderTemplateOptions] Table rendered successfully`);
    }

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Initialize drag-drop for options reordering
     */
    function initializeSortable() {
        const tbody = document.getElementById('options-list');
        if (!tbody || !window.Sortable) {
            console.log('Sortable not available');
            return;
        }

        // Destroy existing sortable if any
        if (tbody.sortableInstance) {
            tbody.sortableInstance.destroy();
        }

        tbody.sortableInstance = new Sortable(tbody, {
            animation: 150,
            handle: '.ri-draggable',
            onEnd: function() {
                console.log('Options reordered');
                reorderOptions();
            }
        });
    }

    /**
     * Reorder options after drag-drop
     */
    async function reorderOptions() {
        const tbody = document.getElementById('options-list');
        const rows = tbody.querySelectorAll('tr[data-option-id]');

        // Update row numbers visually
        rows.forEach((row, index) => {
            const rowNumberCell = row.querySelector('td:nth-child(2)');
            if (rowNumberCell) {
                rowNumberCell.textContent = index + 1;
            }
            row.dataset.index = index;
        });

        const optionIds = Array.from(rows).map((row, index) => {
            return {
                optionId: parseInt(row.dataset.optionId),
                displayOrder: index + 1
            };
        });

        try {
            const response = await fetch(`/api/formbuilder/fields/${currentFieldId}/options/reorder`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(optionIds)
            });

            if (!response.ok) throw new Error('Failed to reorder options');

            console.log('[FormBuilderTemplateOptions] Options reordered successfully');
        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error reordering options:', error);
            showNotification('Failed to reorder options', 'error');
        }
    }

    /**
     * Add new custom option
     * After adding, re-renders options table from server and updates canvas preview
     */
    async function addOption() {
        if (!currentFieldId) {
            console.error('[FormBuilderTemplateOptions] No field selected');
            showNotification('Please select a field first', 'error');
            return;
        }

        try {
            const optionCount = document.querySelectorAll('#options-list tr[data-option-id]').length;
            console.log(`[FormBuilderTemplateOptions] Adding option to field ${currentFieldId}`);

            const response = await fetch(`/api/formbuilder/fields/${currentFieldId}/options`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    optionLabel: `Option ${optionCount + 1}`,
                    optionValue: `option_${optionCount + 1}`
                })
            });

            if (!response.ok) throw new Error('Failed to add option');

            const result = await response.json();
            console.log('[FormBuilderTemplateOptions] Option added:', result);

            // Re-render options table from server (partial view)
            await renderOptionsTableFromServer();

            // Update canvas field preview without page reload
            await updateCanvasFieldPreview();

            showNotification('Option added successfully', 'success');
        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error adding option:', error);
            showNotification('Failed to add option', 'error');
        }
    }

    /**
     * Update canvas field preview without page reload
     * Fetches rendered field card HTML and replaces it in the DOM
     */
    async function updateCanvasFieldPreview() {
        if (!currentFieldId) {
            console.warn('[FormBuilderTemplateOptions] updateCanvasFieldPreview: No currentFieldId set');
            return;
        }

        try {
            console.log(`[FormBuilderTemplateOptions] Updating canvas preview for field ${currentFieldId}`);

            const apiUrl = `/api/formbuilder/fields/${currentFieldId}/render`;
            console.log(`[FormBuilderTemplateOptions] Fetching from: ${apiUrl}`);

            const response = await fetch(apiUrl);

            if (!response.ok) {
                const errorText = await response.text();
                console.error(`[FormBuilderTemplateOptions] Failed to render field card: ${response.status}`, errorText);
                return;
            }

            const result = await response.json();
            console.log('[FormBuilderTemplateOptions] Render API response:', result);

            if (result.success && result.html) {
                // Try multiple selectors to find the field card
                let fieldCard = document.getElementById(`field-${currentFieldId}`);
                
                if (!fieldCard) {
                    // Try alternative selector
                    fieldCard = document.querySelector(`[data-field-id="${currentFieldId}"]`);
                }
                
                if (!fieldCard) {
                    // Try finding by class and data attribute
                    fieldCard = document.querySelector(`.builder-field-card[data-field-id="${currentFieldId}"]`);
                }

                console.log(`[FormBuilderTemplateOptions] Field card found:`, !!fieldCard, fieldCard?.id);

                if (fieldCard) {
                    // Store selection state
                    const wasSelected = fieldCard.classList.contains('selected-element');
                    const fieldBody = document.getElementById(`field-body-${currentFieldId}`);
                    const wasExpanded = fieldBody ? fieldBody.style.display !== 'none' : true;

                    console.log(`[FormBuilderTemplateOptions] State - selected: ${wasSelected}, expanded: ${wasExpanded}`);

                    // Create temporary container to parse HTML
                    const tempDiv = document.createElement('div');
                    tempDiv.innerHTML = result.html.trim();
                    const newFieldCard = tempDiv.firstElementChild;

                    if (!newFieldCard) {
                        console.error('[FormBuilderTemplateOptions] Failed to parse new field card HTML');
                        return;
                    }

                    // Restore selection state
                    if (wasSelected) {
                        newFieldCard.classList.add('selected-element');

                        // Restore parent section selection indicator
                        const parentSection = fieldCard.closest('.builder-section');
                        if (parentSection) {
                            parentSection.classList.add('section-has-selected-field');
                        }
                    }

                    // Restore expansion state
                    if (wasExpanded) {
                        const newFieldBody = newFieldCard.querySelector(`#field-body-${currentFieldId}`);
                        if (newFieldBody) {
                            newFieldBody.style.display = 'block';
                        }
                    }

                    // Replace the field card in DOM
                    fieldCard.replaceWith(newFieldCard);

                    console.log('[FormBuilderTemplateOptions] ✅ Canvas preview updated successfully');
                } else {
                    console.warn(`[FormBuilderTemplateOptions] Field card not found in DOM for field ${currentFieldId}`);
                    console.warn('[FormBuilderTemplateOptions] Available field cards:', 
                        Array.from(document.querySelectorAll('.builder-field-card')).map(el => el.id || el.dataset.fieldId)
                    );
                }
            } else {
                console.error('[FormBuilderTemplateOptions] Invalid response from render API:', result);
            }
        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error updating canvas preview:', error);
        }
    }

    /**
     * Update option property
     * Sends complete option data to the API (required by FieldOptionDto)
     */
    async function updateOption(optionId, property, value) {
        // Get the table row for this option
        const row = document.querySelector(`tr[data-option-id="${optionId}"]`);
        if (!row) {
            console.error(`[FormBuilderTemplateOptions] Option row not found: ${optionId}`);
            showNotification('Option not found', 'error');
            return;
        }

        // Get all current values from the row inputs
        const labelInput = row.querySelector('.option-label-input');
        const valueInput = row.querySelector('.option-value-input');
        const scoreInput = row.querySelector('input[type="number"]');
        const defaultCheckbox = row.querySelector('.option-default-checkbox');

        // Build complete payload with all current values
        const payload = {
            optionId: optionId,
            optionLabel: labelInput?.value || '',
            optionValue: valueInput?.value || '',
            displayOrder: parseInt(row.dataset.displayOrder) || parseInt(row.dataset.index) || 1,
            isDefault: defaultCheckbox?.checked || false,
            isActive: true
        };

        // Override with the new value for the changed property
        if (property === 'value') payload.optionValue = value;
        if (property === 'label') payload.optionLabel = value;
        // Note: score is handled separately as it's not part of FieldOptionDto

        // Validate required fields
        if (!payload.optionLabel.trim()) {
            showNotification('Option label is required', 'error');
            return;
        }
        if (!payload.optionValue.trim()) {
            showNotification('Option value is required', 'error');
            return;
        }

        try {
            console.log(`[FormBuilderTemplateOptions] Updating option ${optionId}:`, payload);

            const response = await fetch(`/api/formbuilder/options/${optionId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to update option');
            }

            console.log(`[FormBuilderTemplateOptions] ✅ Option ${optionId} updated: ${property} = ${value}`);

            // Update canvas preview after successful update
            await updateCanvasFieldPreview();

        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error updating option:', error);
            showNotification(error.message || 'Failed to update option', 'error');
        }
    }

    /**
     * Set default option
     * After setting, re-renders options table from server and updates canvas preview
     */
    async function setDefaultOption(optionId, isDefault) {
        try {
            console.log(`[FormBuilderTemplateOptions] Setting default option ${optionId}`);

            const response = await fetch(`/api/formbuilder/options/${optionId}/default?fieldId=${currentFieldId}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) throw new Error('Failed to set default option');

            // Re-render options table from server (handles single-select clearing other defaults)
            await renderOptionsTableFromServer();

            // Update canvas field preview
            await updateCanvasFieldPreview();

            console.log(`[FormBuilderTemplateOptions] Option ${optionId} set as default: ${isDefault}`);
        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error setting default option:', error);
            showNotification('Failed to set default option', 'error');
        }
    }

    // Store pending delete option ID for modal confirmation
    let pendingDeleteOptionId = null;

    /**
     * Show delete option confirmation modal
     * @param {number} optionId - Option ID to delete
     */
    function showDeleteOptionModal(optionId) {
        pendingDeleteOptionId = optionId;

        // Get option details from the table row
        const row = document.querySelector(`tr[data-option-id="${optionId}"]`);
        if (row) {
            const labelInput = row.querySelector('.option-label-input');
            const valueInput = row.querySelector('.option-value-input');
            
            document.getElementById('deleteOptionLabel').textContent = labelInput?.value || '-';
            document.getElementById('deleteOptionValue').textContent = valueInput?.value || '-';
        }

        // Show modal
        const modalElement = document.getElementById('deleteOptionModal');
        if (modalElement) {
            const modal = new bootstrap.Modal(modalElement);
            modal.show();
        } else {
            // Fallback to confirm if modal not found
            console.warn('[FormBuilderTemplateOptions] Delete modal not found, using confirm()');
            if (confirm('Delete this option?')) {
                executeDeleteOption(optionId);
            }
        }
    }

    /**
     * Execute the actual delete operation (called after modal confirmation)
     * @param {number} optionId - Option ID to delete (optional, uses pendingDeleteOptionId if not provided)
     */
    async function executeDeleteOption(optionId = null) {
        const deleteId = optionId || pendingDeleteOptionId;
        if (!deleteId) {
            console.error('[FormBuilderTemplateOptions] No option ID to delete');
            return;
        }

        try {
            console.log(`[FormBuilderTemplateOptions] Deleting option ${deleteId}`);

            const response = await fetch(`/api/formbuilder/options/${deleteId}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to delete option');
            }

            // Hide modal if it's open
            const modalElement = document.getElementById('deleteOptionModal');
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
            }

            // Re-render options table from server
            await renderOptionsTableFromServer();

            // Update canvas field preview
            await updateCanvasFieldPreview();

            showNotification('Option deleted successfully', 'success');
        } catch (error) {
            console.error('[FormBuilderTemplateOptions] Error deleting option:', error);
            showNotification(error.message || 'Failed to delete option', 'error');
        } finally {
            pendingDeleteOptionId = null;
        }
    }

    /**
     * Delete option - shows confirmation modal
     * After deleting, re-renders options table from server and updates canvas preview
     */
    function deleteOption(optionId) {
        showDeleteOptionModal(optionId);
    }

    /**
     * Initialize delete modal confirm button handler
     */
    function initializeDeleteModal() {
        const confirmBtn = document.getElementById('confirmDeleteOptionBtn');
        if (confirmBtn && !confirmBtn.hasAttribute('data-initialized')) {
            confirmBtn.addEventListener('click', () => {
                executeDeleteOption();
            });
            confirmBtn.setAttribute('data-initialized', 'true');
            console.log('[FormBuilderTemplateOptions] Delete modal confirm button initialized');
        }
    }

    // Initialize delete modal when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeDeleteModal);
    } else {
        initializeDeleteModal();
    }

    /**
     * Show notification
     */
    function showNotification(message, type = 'info') {
        // Try to use existing notification system
        if (window.Toastify) {
            Toastify({
                text: message,
                duration: 3000,
                gravity: "top",
                position: "right",
                backgroundColor: type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8'
            }).showToast();
        } else {
            // Fallback to alert
            alert(message);
        }
    }

    // Public API - template is now auto-applied on selection
    const api = {
        init,
        addOption,
        updateOption,
        setDefaultOption,
        deleteOption,
        reorderOptions,
        renderOptionsTableFromServer,
        updateCanvasFieldPreview
    };

    console.log('✅ FormBuilderTemplateOptions module loaded successfully!', api);
    return api;
})();
