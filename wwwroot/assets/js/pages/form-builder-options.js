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
    function init(fieldId, fieldType, existingOptions) {
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

        // Render options table
        renderOptionsTable(existingOptions || []);

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

            // Update options table with new data
            renderOptionsTable(result.field.options);

            // Update canvas preview without page reload
            if (window.FormBuilder && window.FormBuilder.reloadField) {
                await window.FormBuilder.reloadField(currentFieldId);
            }

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
     * Render options table
     */
    function renderOptionsTable(options) {
        const tbody = document.getElementById('options-list');
        if (!tbody) {
            console.error('[FormBuilderTemplateOptions] options-list tbody not found!');
            return;
        }

        console.log(`[FormBuilderTemplateOptions] Rendering ${options.length} options`);

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
     */
    async function addOption() {
        if (!currentFieldId) return;

        try {
            const optionCount = document.querySelectorAll('#options-list tr[data-option-id]').length;
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
            console.log('Option added:', result);

            // Reload field to get updated options
            if (window.FormBuilder && window.FormBuilder.reloadField) {
                await window.FormBuilder.reloadField(currentFieldId);
            }

            showNotification('Option added successfully', 'success');
        } catch (error) {
            console.error('Error adding option:', error);
            showNotification('Failed to add option', 'error');
        }
    }

    /**
     * Update option property
     */
    async function updateOption(optionId, property, value) {
        const payload = {};
        if (property === 'value') payload.optionValue = value;
        if (property === 'label') payload.optionLabel = value;
        if (property === 'score') payload.scoreValue = value ? parseFloat(value) : null;

        try {
            const response = await fetch(`/api/formbuilder/options/${optionId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!response.ok) throw new Error('Failed to update option');

            console.log(`Option ${optionId} updated: ${property} = ${value}`);
        } catch (error) {
            console.error('Error updating option:', error);
            showNotification('Failed to update option', 'error');
        }
    }

    /**
     * Set default option
     */
    async function setDefaultOption(optionId, isDefault) {
        try {
            const response = await fetch(`/api/formbuilder/options/${optionId}/default?fieldId=${currentFieldId}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' }
            });

            if (!response.ok) throw new Error('Failed to set default option');

            // Reload field to update UI (in case other defaults were cleared for single-select)
            if (window.FormBuilder && window.FormBuilder.reloadField) {
                await window.FormBuilder.reloadField(currentFieldId);
            }

            console.log(`Option ${optionId} set as default: ${isDefault}`);
        } catch (error) {
            console.error('Error setting default option:', error);
            showNotification('Failed to set default option', 'error');
        }
    }

    /**
     * Delete option
     */
    async function deleteOption(optionId) {
        if (!confirm('Delete this option?')) return;

        try {
            const response = await fetch(`/api/formbuilder/options/${optionId}`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to delete option');
            }

            // Reload field to update options list
            if (window.FormBuilder && window.FormBuilder.reloadField) {
                await window.FormBuilder.reloadField(currentFieldId);
            }

            showNotification('Option deleted successfully', 'success');
        } catch (error) {
            console.error('Error deleting option:', error);
            showNotification(error.message || 'Failed to delete option', 'error');
        }
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
        reorderOptions
    };

    console.log('✅ FormBuilderTemplateOptions module loaded successfully!', api);
    return api;
})();
