/**
 * Form Builder Properties Panel
 * Handles loading and saving properties for sections and fields
 */

const FormBuilderProperties = {
    currentElementType: null,  // 'section' or 'field'
    currentElementId: null,
    currentSectionData: null,

    /**
     * Initialize properties panel
     */
    init: function() {
        this.setupEventListeners();
        this.showEmptyState();
    },

    /**
     * Setup event listeners for property form elements
     */
    setupEventListeners: function() {
        // Icon input - update preview
        const iconInput = document.getElementById('prop_iconClass');
        if (iconInput) {
            iconInput.addEventListener('input', (e) => {
                this.updateIconPreview(e.target.value);
            });
        }

        // Collapsible checkbox - show/hide collapsed by default option
        const collapsibleCheckbox = document.getElementById('prop_isCollapsible');
        if (collapsibleCheckbox) {
            collapsibleCheckbox.addEventListener('change', (e) => {
                this.toggleCollapsedByDefaultVisibility(e.target.checked);
            });
        }
    },

    /**
     * Show empty state (no element selected)
     */
    showEmptyState: function() {
        this.hideAllPropertyPanels();
        // Show empty states for all tabs
        document.getElementById('properties-empty').style.display = 'block';
        document.getElementById('config-empty').style.display = 'block';

        // Reset tab visibility (show all tabs in empty state)
        this.updateTabVisibility(null);

        // Reset field form visibility (show all conditional fields)
        const placeholderContainer = document.getElementById('prop_placeholderText_container');
        const defaultValueContainer = document.getElementById('prop_defaultValue_container');
        if (placeholderContainer) placeholderContainer.style.display = 'block';
        if (defaultValueContainer) defaultValueContainer.style.display = 'block';

        // Clear current element tracking
        this.currentElementType = null;
        this.currentElementId = null;
        this.currentFieldData = null;
        
        console.log('[FormBuilderProperties] Empty state shown, panel cleared');
    },

    /**
     * Hide all property panels
     */
    hideAllPropertyPanels: function() {
        // General tab panels
        document.getElementById('properties-empty').style.display = 'none';
        document.getElementById('properties-section-general').style.display = 'none';
        document.getElementById('properties-field-general').style.display = 'none';

        // Configuration tab panels
        document.getElementById('config-empty').style.display = 'none';
        if (document.getElementById('config-section')) {
            document.getElementById('config-section').style.display = 'none';
        }
        if (document.getElementById('config-field')) {
            document.getElementById('config-field').style.display = 'none';
        }
    },

    /**
     * Show/hide tabs based on element type
     * Sections: Show General, Config, Advanced (hide Validation)
     * Fields: Show all 4 tabs
     */
    updateTabVisibility: function(elementType) {
        const validationTab = document.getElementById('tab-validation');

        if (elementType === 'section') {
            // Hide Validation tab for sections
            validationTab.style.display = 'none';
        } else if (elementType === 'field') {
            // Show all tabs for fields
            validationTab.style.display = 'block';
        } else {
            // Empty state - show all tabs
            validationTab.style.display = 'block';
        }
    },

    /**
     * Load section properties when a section is selected
     * @param {number} sectionId - The section ID to load
     */
    loadSectionProperties: async function(sectionId) {
        try {
            // Show loading state
            this.hideAllPropertyPanels();

            // Fetch section data from API
            const response = await fetch(`/api/formbuilder/sections/${sectionId}`);

            if (!response.ok) {
                throw new Error('Failed to load section data');
            }

            const result = await response.json();

            if (result.success && result.data) {
                this.currentElementType = 'section';
                this.currentElementId = sectionId;
                this.currentSectionData = result.data;

                // Update tab visibility (hide Validation for sections)
                this.updateTabVisibility('section');

                // Populate the form
                this.populateSectionForm(result.data);

                // Show section properties panel
                document.getElementById('properties-section-general').style.display = 'block';
            } else {
                console.error('Failed to load section:', result.message);
                alert('Failed to load section properties');
                this.showEmptyState();
            }

        } catch (error) {
            console.error('Error loading section properties:', error);
            alert('An error occurred while loading section properties');
            this.showEmptyState();
        }
    },

    /**
     * Populate section form with data
     * @param {object} data - Section data
     */
    populateSectionForm: function(data) {
        // Section Name
        document.getElementById('prop_sectionName').value = data.sectionName || '';

        // Description
        document.getElementById('prop_sectionDescription').value = data.sectionDescription || '';

        // Icon
        const iconClass = data.iconClass || '';
        document.getElementById('prop_iconClass').value = iconClass;
        this.updateIconPreview(iconClass);

        // Display Order (read-only)
        document.getElementById('prop_displayOrder').value = data.displayOrder || 0;

        // Collapsible
        const isCollapsible = data.isCollapsible || false;
        document.getElementById('prop_isCollapsible').checked = isCollapsible;

        // Collapsed by Default
        document.getElementById('prop_isCollapsedByDefault').checked = data.isCollapsedByDefault || false;

        // Show/hide collapsed by default option
        this.toggleCollapsedByDefaultVisibility(isCollapsible);

        // Clear validation state
        document.getElementById('sectionGeneralForm').classList.remove('was-validated');
    },

    /**
     * Update icon preview
     * @param {string} iconClass - Icon class name
     */
    updateIconPreview: function(iconClass) {
        const preview = document.getElementById('prop_iconPreview');
        if (preview) {
            // Remove all existing icon classes
            preview.className = '';

            // Add new icon class or default
            if (iconClass && iconClass.trim()) {
                preview.className = iconClass.trim();
            } else {
                preview.className = 'ri-layout-line';
            }
        }
    },

    /**
     * Toggle visibility of "Collapsed by Default" option
     * @param {boolean} isCollapsible - Whether collapsible is enabled
     */
    toggleCollapsedByDefaultVisibility: function(isCollapsible) {
        const container = document.getElementById('prop_collapsedContainer');
        if (container) {
            container.style.display = isCollapsible ? 'block' : 'none';

            // If not collapsible, uncheck collapsed by default
            if (!isCollapsible) {
                document.getElementById('prop_isCollapsedByDefault').checked = false;
            }
        }
    },

    /**
     * Save section properties
     */
    saveSectionProperties: async function() {
        const form = document.getElementById('sectionGeneralForm');

        // Validate form
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
            return;
        }

        // Get form values
        const sectionName = document.getElementById('prop_sectionName').value.trim();
        const sectionDescription = document.getElementById('prop_sectionDescription').value.trim();
        const iconClass = document.getElementById('prop_iconClass').value.trim();
        const isCollapsible = document.getElementById('prop_isCollapsible').checked;
        const isCollapsedByDefault = document.getElementById('prop_isCollapsedByDefault').checked;

        // Prepare update data
        const updateData = {
            sectionId: this.currentElementId,
            sectionName: sectionName,
            sectionDescription: sectionDescription || null,
            iconClass: iconClass || null,
            isCollapsible: isCollapsible,
            isCollapsedByDefault: isCollapsible ? isCollapsedByDefault : false
        };

        // Show saving state
        const saveBtn = event.target;
        const originalBtnHtml = saveBtn.innerHTML;
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';

        try {
            const response = await fetch(`/api/formbuilder/sections/${this.currentElementId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updateData)
            });

            const result = await response.json();

            if (result.success) {
                // Update canvas without page reload
                this.updateSectionInCanvas(updateData);

                // Update cached data
                this.currentSectionData = { ...this.currentSectionData, ...updateData };

                // Show success feedback
                this.showSaveSuccess(saveBtn, originalBtnHtml);
            } else {
                console.error('Save failed:', result.message);
                alert('Failed to save section properties: ' + (result.message || 'Unknown error'));

                // Restore button
                saveBtn.disabled = false;
                saveBtn.innerHTML = originalBtnHtml;
            }

        } catch (error) {
            console.error('Error saving section properties:', error);
            alert('An error occurred while saving section properties');

            // Restore button
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalBtnHtml;
        }
    },

    /**
     * Show save success feedback
     * @param {HTMLElement} btn - Save button element
     * @param {string} originalHtml - Original button HTML
     */
    showSaveSuccess: function(btn, originalHtml) {
        btn.innerHTML = '<i class="ri-check-line me-1"></i>Saved!';
        btn.classList.remove('btn-primary');
        btn.classList.add('btn-success');

        setTimeout(() => {
            btn.disabled = false;
            btn.innerHTML = originalHtml;
            btn.classList.remove('btn-success');
            btn.classList.add('btn-primary');
        }, 2000);
    },

    /**
     * Update section in canvas (without page reload)
     * @param {object} data - Updated section data
     */
    updateSectionInCanvas: function(data) {
        const sectionCard = document.getElementById(`section-${data.sectionId}`);
        if (!sectionCard) return;

        // Update section name
        const titleElement = sectionCard.querySelector('.card-title');
        if (titleElement) {
            titleElement.textContent = data.sectionName;
        }

        // Update description
        const descElement = sectionCard.querySelector('.text-muted.small');
        if (data.sectionDescription) {
            if (descElement) {
                descElement.textContent = data.sectionDescription;
            } else {
                // Create description element if it doesn't exist
                const titleContainer = sectionCard.querySelector('.flex-grow-1');
                const newDesc = document.createElement('p');
                newDesc.className = 'text-muted mb-0 small';
                newDesc.textContent = data.sectionDescription;
                titleContainer.appendChild(newDesc);
            }
        } else {
            // Remove description if empty
            if (descElement) {
                descElement.remove();
            }
        }

        // Update icon in card header if icon element exists
        const iconElement = sectionCard.querySelector('.card-header i:not(.ri-draggable)');
        if (iconElement && data.iconClass) {
            iconElement.className = data.iconClass;
        }

        // Update collapsible data attributes
        sectionCard.dataset.isCollapsible = data.isCollapsible;
        sectionCard.dataset.isCollapsedByDefault = data.isCollapsedByDefault;
    },

    /**
     * Load field properties
     * @param {number} fieldId - The field ID to load
     */
    loadFieldProperties: async function(fieldId) {
        try {
            // Show loading state
            this.hideAllPropertyPanels();

            // Fetch field data from API (new RESTful endpoint)
            const response = await fetch(`/api/formbuilder/fields/${fieldId}`);

            if (!response.ok) {
                throw new Error('Failed to load field data');
            }

            const result = await response.json();

            if (result.success && result.field) {
                this.currentElementType = 'field';
                this.currentElementId = fieldId;
                this.currentFieldData = result.field;

                // Update tab visibility (show Validation for fields)
                this.updateTabVisibility('field');

                // Populate the form
                this.populateFieldForm(result.field);

                // Show field properties panel
                document.getElementById('properties-field-general').style.display = 'block';

                // Load options if field type requires them
                FormBuilderOptions.loadOptions(result.field);

                // Load validations for the field
                FormBuilderValidation.loadValidations(result.field);

                // Load field-specific configurations
                FormBuilderConfiguration.loadConfiguration(result.field);

                console.log('Field properties loaded:', result.field);
            } else {
                console.error('Failed to load field:', result.message);
                alert('Failed to load field properties');
                this.showEmptyState();
            }

        } catch (error) {
            console.error('Error loading field properties:', error);
            alert('An error occurred while loading field properties');
            this.showEmptyState();
        }
    },

    /**
     * Populate field form with data
     * @param {object} data - Field data
     */
    populateFieldForm: function(data) {
        // Field Type (hidden, used for conditional logic)
        document.getElementById('prop_fieldType').value = data.dataTypeName || '';

        // Field Name
        document.getElementById('prop_fieldName').value = data.itemName || '';

        // Placeholder Text
        document.getElementById('prop_placeholderText').value = data.placeholderText || '';

        // Help Text
        document.getElementById('prop_helpText').value = data.helpText || '';

        // Default Value
        document.getElementById('prop_defaultValue').value = data.defaultValue || '';

        // Required checkbox
        document.getElementById('prop_isRequired').checked = data.isRequired || false;

        // Conditionally show/hide fields based on field type
        this.updateFieldVisibility(data.dataTypeName);
    },

    /**
     * Update field visibility based on field type
     * @param {string} fieldType - The field type name
     */
    updateFieldVisibility: function(fieldType) {
        const placeholderContainer = document.getElementById('prop_placeholderText_container');
        const defaultValueContainer = document.getElementById('prop_defaultValue_container');

        // Field types that DON'T support placeholder (option-based selection fields)
        const noPlaceholderTypes = ['Radio', 'Checkbox'];
        
        // Field types that DON'T support default value (option-based - use "Set as Default" in options)
        const noDefaultValueTypes = ['Radio', 'Checkbox', 'Dropdown', 'MultiSelect'];

        // Show/hide Placeholder field
        if (placeholderContainer) {
            if (noPlaceholderTypes.includes(fieldType)) {
                placeholderContainer.style.display = 'none';
            } else {
                placeholderContainer.style.display = 'block';
            }
        }

        // Show/hide Default Value field
        if (defaultValueContainer) {
            if (noDefaultValueTypes.includes(fieldType)) {
                defaultValueContainer.style.display = 'none';
            } else {
                defaultValueContainer.style.display = 'block';
            }
        }
    },

    /**
     * Save field properties
     */
    saveFieldProperties: async function() {
        const form = document.getElementById('fieldGeneralForm');

        // Validate form
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
            return;
        }

        // Get form values
        const fieldName = document.getElementById('prop_fieldName').value.trim();
        const placeholderText = document.getElementById('prop_placeholderText').value.trim();
        const helpText = document.getElementById('prop_helpText').value.trim();
        const defaultValue = document.getElementById('prop_defaultValue').value.trim();
        const isRequired = document.getElementById('prop_isRequired').checked;

        // Prepare update data
        const updateData = {
            itemName: fieldName,
            placeholderText: placeholderText || null,
            helpText: helpText || null,
            defaultValue: defaultValue || null,
            isRequired: isRequired
        };

        // Show saving state
        const saveBtn = event.target;
        const originalBtnHtml = saveBtn.innerHTML;
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';

        try {
            // Call API to update field (new RESTful endpoint with PUT verb)
            const response = await fetch(`/api/formbuilder/fields/${this.currentElementId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updateData)
            });

            const result = await response.json();

            if (result.success) {
                // Success feedback
                saveBtn.innerHTML = '<i class="ri-check-line me-1"></i>Saved!';
                saveBtn.classList.remove('btn-secondary');
                saveBtn.classList.add('btn-success');

                // Update the canvas preview dynamically (no page reload)
                await this.updateCanvasPreview();

                // Update current field data cache
                if (this.currentFieldData) {
                    this.currentFieldData.itemName = fieldName;
                    this.currentFieldData.placeholderText = placeholderText;
                    this.currentFieldData.helpText = helpText;
                    this.currentFieldData.defaultValue = defaultValue;
                    this.currentFieldData.isRequired = isRequired;
                }

                // Reset button after a short delay
                setTimeout(() => {
                    saveBtn.disabled = false;
                    saveBtn.innerHTML = originalBtnHtml;
                    saveBtn.classList.remove('btn-success');
                    saveBtn.classList.add('btn-secondary');
                }, 1500);

                console.log('[FormBuilderProperties] Field properties saved successfully');
            } else {
                throw new Error(result.message || 'Failed to save field');
            }

        } catch (error) {
            console.error('Error saving field:', error);
            alert('Failed to save field properties: ' + error.message);

            // Reset button
            saveBtn.disabled = false;
            saveBtn.innerHTML = originalBtnHtml;
        }
    },

    // ========================================================================
    // CONFIGURATION TAB METHODS
    // ========================================================================

    /**
     * Load section configuration settings
     * @param {number} sectionId - The section ID to load configuration for
     */
    loadSectionConfiguration: async function(sectionId) {
        try {
            // Hide empty state and show section config form
            document.getElementById('config-empty').style.display = 'none';
            document.getElementById('config-section').style.display = 'block';
            document.getElementById('config-field').style.display = 'none';

            // For now, use the existing section data
            // In the future, this would fetch from FormItemConfiguration table
            const response = await fetch(`/api/formbuilder/sections/${sectionId}`);

            if (!response.ok) {
                throw new Error('Failed to load section configuration');
            }

            const result = await response.json();

            if (result.success && result.data) {
                this.populateSectionConfigurationForm(result.data);
            } else {
                console.error('Failed to load section configuration:', result.message);
                // Show defaults
                this.populateSectionConfigurationForm({});
            }

        } catch (error) {
            console.error('Error loading section configuration:', error);
            // Show defaults
            this.populateSectionConfigurationForm({});
        }
    },

    /**
     * Populate section configuration form with data (or defaults)
     * @param {object} data - Section data with configuration
     */
    populateSectionConfigurationForm: function(data) {
        // Column Layout (default: single column)
        const columnLayout = data.columnLayout || '1';
        document.querySelectorAll('input[name="config_columnLayout"]').forEach(radio => {
            radio.checked = (radio.value === columnLayout);
        });

        // Section Width (default: 100%)
        const sectionWidth = data.sectionWidth || '100';
        document.querySelectorAll('input[name="config_sectionWidth"]').forEach(radio => {
            radio.checked = (radio.value === sectionWidth);
        });

        // Background Style (default: transparent)
        const backgroundStyle = data.backgroundStyle || 'transparent';
        document.querySelectorAll('input[name="config_backgroundStyle"]').forEach(radio => {
            radio.checked = (radio.value === backgroundStyle);
        });

        // Show Section Number (default: true)
        const showSectionNumber = data.showSectionNumber !== undefined ? data.showSectionNumber : true;
        document.getElementById('config_showSectionNumber').checked = showSectionNumber;

        // Spacing (defaults: medium for both)
        const topPadding = data.topPadding || 'medium';
        const bottomPadding = data.bottomPadding || 'medium';
        document.getElementById('config_topPadding').value = topPadding;
        document.getElementById('config_bottomPadding').value = bottomPadding;
    },

    /**
     * Save section configuration
     */
    saveSectionConfiguration: async function() {
        // Get selected values
        const columnLayout = document.querySelector('input[name="config_columnLayout"]:checked')?.value || '1';
        const sectionWidth = document.querySelector('input[name="config_sectionWidth"]:checked')?.value || '100';
        const backgroundStyle = document.querySelector('input[name="config_backgroundStyle"]:checked')?.value || 'transparent';
        const showSectionNumber = document.getElementById('config_showSectionNumber').checked;
        const topPadding = document.getElementById('config_topPadding').value;
        const bottomPadding = document.getElementById('config_bottomPadding').value;

        // Prepare configuration data
        const configData = {
            sectionId: this.currentElementId,
            configuration: {
                columnLayout,
                sectionWidth,
                backgroundStyle,
                showSectionNumber,
                topPadding,
                bottomPadding
            }
        };

        // Show saving state
        const saveBtn = event.target;
        const originalBtnHtml = saveBtn.innerHTML;
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Saving...';

        try {
            // For now, we'll just store this in the session/localStorage
            // In the future, this would be saved to FormItemConfiguration table
            const response = await fetch(`/api/formbuilder/sections/${this.currentElementId}/configuration`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(configData)
            });

            const result = await response.json();

            if (result.success) {
                // Apply visual changes to the canvas (future enhancement)
                this.applySectionConfigurationToCanvas(configData);

                // Show success feedback
                this.showSaveSuccess(saveBtn, originalBtnHtml);
            } else {
                console.error('Save failed:', result.message);
                alert('Failed to save configuration: ' + (result.message || 'Unknown error'));

                // Restore button
                saveBtn.disabled = false;
                saveBtn.innerHTML = originalBtnHtml;
            }

        } catch (error) {
            console.error('Error saving section configuration:', error);

            // For now, just show success since backend endpoint doesn't exist yet
            console.log('Configuration would be saved:', configData);
            this.showSaveSuccess(saveBtn, originalBtnHtml);
        }
    },

    /**
     * Apply configuration visually to the section in canvas
     * @param {object} configData - Configuration data to apply
     */
    applySectionConfigurationToCanvas: function(configData) {
        const sectionCard = document.getElementById(`section-${configData.sectionId}`);
        if (!sectionCard) return;

        // Store configuration in data attributes for future use
        sectionCard.dataset.columnLayout = configData.configuration.columnLayout;
        sectionCard.dataset.sectionWidth = configData.configuration.sectionWidth;
        sectionCard.dataset.backgroundStyle = configData.configuration.backgroundStyle;
        sectionCard.dataset.showSectionNumber = configData.configuration.showSectionNumber;
        sectionCard.dataset.topPadding = configData.configuration.topPadding;
        sectionCard.dataset.bottomPadding = configData.configuration.bottomPadding;

        // Future enhancement: Apply visual changes here
        // - Update section width class
        // - Update background color
        // - Update padding classes
        // - Show/hide section number badge
    },

    /**
     * Update canvas preview without full page reload
     * Fetches rendered field card HTML and replaces it in the DOM
     */
    updateCanvasPreview: async function() {
        // Use currentElementId (the field ID we're editing)
        const fieldId = this.currentElementId;
        if (!fieldId || this.currentElementType !== 'field') return;

        try {
            console.log(`[FormBuilderProperties] Updating canvas preview for field ${fieldId}`);

            // Fetch rendered field card HTML from server
            const response = await fetch(`/api/formbuilder/fields/${fieldId}/render`);

            if (!response.ok) {
                console.error('Failed to render field card');
                return;
            }

            const result = await response.json();

            if (result.success && result.html) {
                const fieldCard = document.getElementById(`field-${fieldId}`);

                if (fieldCard) {
                    // Store selection state
                    const wasSelected = fieldCard.classList.contains('selected-element');
                    const wasExpanded = document.getElementById(`field-body-${fieldId}`)?.style.display !== 'none';

                    // Create temporary container to parse HTML
                    const tempDiv = document.createElement('div');
                    tempDiv.innerHTML = result.html;
                    const newFieldCard = tempDiv.firstElementChild;

                    // Restore selection state
                    if (wasSelected) {
                        newFieldCard.classList.add('selected-element');

                        // Restore parent section selection indicator
                        const parentSection = fieldCard.closest('.builder-section');
                        if (parentSection) {
                            parentSection.classList.add('section-has-selected-field');
                        }
                    }

                    // Restore expansion state if field was expanded
                    if (wasExpanded) {
                        const newFieldBody = newFieldCard.querySelector(`#field-body-${fieldId}`);
                        if (newFieldBody) {
                            newFieldBody.style.display = 'block';
                        }
                    }

                    // Replace the field card in DOM
                    fieldCard.replaceWith(newFieldCard);

                    console.log('[FormBuilderProperties] Canvas preview updated successfully');
                } else {
                    console.warn(`Field card not found in canvas for field ${fieldId}`);
                }
            }
        } catch (error) {
            console.error('Error updating canvas preview:', error);
            // Don't show error to user - this is a background operation
        }
    }
};

// ============================================================================
// FORM BUILDER OPTIONS MANAGER
// Handles field options (Dropdown, Radio, Checkbox, MultiSelect)
// ============================================================================

const FormBuilderOptions = {
    // State management
    currentFieldId: null,
    currentOptions: [],
    sortableInstance: null,
    pendingDeleteOptionId: null,

    /**
     * Initialize options manager
     */
    init: function() {
        console.log('FormBuilderOptions initialized');

        // Initialize delete option modal confirm button
        this.initializeDeleteModal();
    },

    /**
     * Initialize delete option modal event listeners
     */
    initializeDeleteModal: function() {
        const confirmBtn = document.getElementById('confirmDeleteOptionBtn');
        if (confirmBtn) {
            confirmBtn.addEventListener('click', () => {
                if (this.pendingDeleteOptionId) {
                    this.deleteOption(this.pendingDeleteOptionId);

                    // Hide modal
                    const modal = bootstrap.Modal.getInstance(document.getElementById('deleteOptionModal'));
                    if (modal) modal.hide();

                    // Clear pending delete
                    this.pendingDeleteOptionId = null;
                }
            });
        }
    },

    /**
     * Load options for a field
     * @param {object} fieldData - Field data including options array
     */
    loadOptions: function(fieldData) {
        // Store current field ID
        this.currentFieldId = fieldData.itemId;
        
        // Check if field type requires options
        const requiresOptions = ['Dropdown', 'Radio', 'Checkbox', 'MultiSelect'].includes(fieldData.dataTypeName);
        
        if (!requiresOptions) {
            this.hideOptionsManager();
            return;
        }
        
        // Store options
        this.currentOptions = fieldData.options || [];

        // Initialize FormBuilderTemplateOptions module (new template-based options manager)
        console.log('[FormBuilderProperties] Checking for FormBuilderTemplateOptions module...', !!window.FormBuilderTemplateOptions);

        if (window.FormBuilderTemplateOptions && typeof window.FormBuilderTemplateOptions.init === 'function') {
            console.log('[FormBuilderProperties] Using NEW FormBuilderTemplateOptions module');
            window.FormBuilderTemplateOptions.init(fieldData.itemId, fieldData.dataTypeName, this.currentOptions);
        } else {
            console.error('[FormBuilderProperties] FormBuilderTemplateOptions module NOT FOUND! Using old rendering.');
            console.error('[FormBuilderProperties] Make sure form-builder-options.js is loaded.');
            // Fallback to old options rendering if FormBuilderTemplateOptions not available
            this.renderOptionsList(this.currentOptions);
            this.initializeSortable();
        }

        // Show options manager
        this.showOptionsManager();

        console.log(`[FormBuilderProperties] Loaded ${this.currentOptions.length} options for field ${this.currentFieldId}`);
    },

    /**
     * Render options table by fetching HTML from server
     * Uses server-side Razor partial view for consistent styling
     * Delegates to FormBuilderTemplateOptions if available
     */
    renderOptionsTable: async function() {
        // Prefer FormBuilderTemplateOptions module if available
        if (window.FormBuilderTemplateOptions && typeof window.FormBuilderTemplateOptions.renderOptionsTableFromServer === 'function') {
            console.log('[FormBuilderOptions] Delegating to FormBuilderTemplateOptions.renderOptionsTableFromServer');
            await window.FormBuilderTemplateOptions.renderOptionsTableFromServer();
            return;
        }

        // Fallback implementation
        if (!this.currentFieldId) {
            console.error('[FormBuilderOptions] No field ID set');
            return;
        }

        try {
            console.log(`[FormBuilderOptions] Fetching rendered options table for field ${this.currentFieldId}`);

            // Fetch rendered HTML from server
            const response = await fetch(`/api/formbuilder/fields/${this.currentFieldId}/options/render`);

            if (!response.ok) {
                throw new Error('Failed to fetch options table');
            }

            const result = await response.json();

            if (result.success && result.html) {
                // Find the wrapper container and replace its contents
                const wrapper = document.getElementById('options-table-wrapper');

                if (wrapper) {
                    wrapper.innerHTML = result.html;

                    // Re-initialize sortable after rendering
                    this.initializeSortable();

                    console.log(`[FormBuilderOptions] ✅ Rendered ${result.optionCount} options from server`);
                } else {
                    console.error('[FormBuilderOptions] options-table-wrapper not found');
                }
            }
        } catch (error) {
            console.error('[FormBuilderOptions] Error rendering options table:', error);
        }
    },

    /**
     * Legacy function - kept for backwards compatibility
     * Now calls renderOptionsTable instead
     * @param {Array} options - Array of option objects (ignored - we fetch from server)
     */
    renderOptionsList: function(options) {
        this.renderOptionsTable();
    },

    /**
     * Escape HTML special characters
     * @param {string} text - Text to escape
     * @returns {string} Escaped text
     */
    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    /**
     * Add a new option
     */
    addOption: async function() {
        if (!this.currentFieldId) {
            alert('No field selected');
            return;
        }
        
        // Create temporary option with placeholder data
        const tempOption = {
            optionId: 0,
            optionLabel: `Option ${this.currentOptions.length + 1}`,
            optionValue: `option_${this.currentOptions.length + 1}`,
            displayOrder: this.currentOptions.length + 1,
            isDefault: false,
            isActive: true
        };
        
        try {
            // Call API
            const response = await fetch(`/api/formbuilder/fields/${this.currentFieldId}/options`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(tempOption)
            });
            
            if (!response.ok) {
                throw new Error('Failed to add option');
            }
            
            const result = await response.json();

            if (result.success && result.option) {
                // Add to local array
                this.currentOptions.push(result.option);

                // Re-render list
                this.renderOptionsList(this.currentOptions);

                // Re-initialize sortable
                this.initializeSortable();

                // Focus on new option's value input for immediate editing
                setTimeout(() => {
                    const newValueInput = document.querySelector(`input.option-value-input[data-option-id="${result.option.optionId}"]`);
                    if (newValueInput) {
                        newValueInput.select();
                    }
                }, 100);

                // Update canvas preview without full page reload
                await FormBuilderProperties.updateCanvasPreview();

                console.log('Option added successfully:', result.option);
            } else {
                throw new Error(result.message || 'Failed to add option');
            }
            
        } catch (error) {
            console.error('Error adding option:', error);
            alert('Failed to add option: ' + error.message);
        }
    },

    /**
     * Update an option
     * @param {number} optionId - Option ID to update
     */
    updateOption: async function(optionId) {
        const labelInput = document.querySelector(`input.option-label-input[data-option-id="${optionId}"]`);
        const valueInput = document.querySelector(`input.option-value-input[data-option-id="${optionId}"]`);
        
        if (!labelInput || !valueInput) {
            console.error('Option inputs not found');
            return;
        }
        
        const label = labelInput.value.trim();
        const value = valueInput.value.trim();
        
        // Validate
        if (!label) {
            alert('Option label is required');
            labelInput.focus();
            return;
        }
        
        if (!value) {
            alert('Option value is required');
            valueInput.focus();
            return;
        }
        
        try {
            // Find option in local array
            const option = this.currentOptions.find(o => o.optionId === optionId);
            if (!option) {
                throw new Error('Option not found');
            }
            
            // Call API
            const response = await fetch(`/api/formbuilder/options/${optionId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    optionLabel: label,
                    optionValue: value,
                    isDefault: option.isDefault,
                    isActive: option.isActive,
                    displayOrder: option.displayOrder
                })
            });
            
            if (!response.ok) {
                throw new Error('Failed to update option');
            }
            
            const result = await response.json();
            
            if (result.success) {
                // Update local array
                option.optionLabel = label;
                option.optionValue = value;
                
                // Show success feedback (green border briefly)
                labelInput.style.borderColor = '#0ab39c';
                valueInput.style.borderColor = '#0ab39c';
                setTimeout(() => {
                    labelInput.style.borderColor = '';
                    valueInput.style.borderColor = '';
                }, 1000);
                
                // Update canvas preview without full page reload
                FormBuilderProperties.updateCanvasPreview();
                
                console.log('Option updated successfully:', option);
            } else {
                throw new Error(result.message || 'Failed to update option');
            }
            
        } catch (error) {
            console.error('Error updating option:', error);
            alert('Failed to update option: ' + error.message);
        }
    },

    /**
     * Delete an option
     * @param {number} optionId - Option ID to delete
     */
    deleteOption: async function(optionId) {
        // Check minimum 2 options
        if (this.currentOptions.length <= 2) {
            alert('Cannot delete option. Selection fields require at least 2 options.');
            return;
        }
        
        try {
            // Call API
            const response = await fetch(`/api/formbuilder/options/${optionId}`, {
                method: 'DELETE'
            });
            
            if (!response.ok) {
                throw new Error('Failed to delete option');
            }
            
            const result = await response.json();
            
            if (result.success) {
                // Remove from local array
                this.currentOptions = this.currentOptions.filter(o => o.optionId !== optionId);
                
                // Re-render list
                this.renderOptionsList(this.currentOptions);
                
                // Re-initialize sortable
                this.initializeSortable();
                
                // Update canvas preview without full page reload
                FormBuilderProperties.updateCanvasPreview();
                
                console.log('Option deleted successfully');
            } else {
                throw new Error(result.message || 'Failed to delete option');
            }
            
        } catch (error) {
            console.error('Error deleting option:', error);
            alert('Failed to delete option: ' + error.message);
        }
    },

    /**
     * Set/unset default option
     * @param {number} optionId - Option ID to toggle
     */
    setDefaultOption: async function(optionId) {
        try {
            // Get field type to determine single vs multi-select
            const fieldData = FormBuilderProperties.currentFieldData;
            const isSingleSelect = ['Dropdown', 'Radio'].includes(fieldData.dataTypeName);
            
            // Call API
            const response = await fetch(`/api/formbuilder/options/${optionId}/default?fieldId=${this.currentFieldId}`, {
                method: 'PATCH'
            });
            
            if (!response.ok) {
                throw new Error('Failed to set default option');
            }
            
            const result = await response.json();
            
            if (result.success) {
                if (isSingleSelect) {
                    // Uncheck all others
                    this.currentOptions.forEach(o => {
                        o.isDefault = (o.optionId === optionId);
                    });
                    
                    // Update UI
                    document.querySelectorAll('.option-default-checkbox').forEach(cb => {
                        const cbOptionId = parseInt(cb.id.replace('default_', ''));
                        cb.checked = (cbOptionId === optionId);
                    });
                } else {
                    // Toggle this option
                    const option = this.currentOptions.find(o => o.optionId === optionId);
                    if (option) {
                        option.isDefault = !option.isDefault;
                    }
                }
                
                // Update canvas preview without full page reload
                FormBuilderProperties.updateCanvasPreview();
                
                console.log('Default option updated');
            } else {
                throw new Error(result.message || 'Failed to set default');
            }
            
        } catch (error) {
            console.error('Error setting default option:', error);
            alert('Failed to set default option: ' + error.message);
        }
    },

    /**
     * Initialize SortableJS for drag-drop reordering
     */
    initializeSortable: function() {
        // Destroy existing instance
        this.destroySortable();
        
        const container = document.getElementById('options-list');
        if (!container || this.currentOptions.length === 0) {
            return;
        }
        
        this.sortableInstance = new Sortable(container, {
            handle: '.option-drag-handle',
            animation: 150,
            ghostClass: 'sortable-ghost',
            dragClass: 'sortable-drag',
            
            onUpdate: (evt) => {
                console.log('Options reordered');
                this.reorderOptions();
            }
        });
        
        console.log('Sortable initialized for options');
    },

    /**
     * Destroy SortableJS instance
     */
    destroySortable: function() {
        if (this.sortableInstance) {
            this.sortableInstance.destroy();
            this.sortableInstance = null;
        }
    },

    /**
     * Update option display orders after drag-drop
     */
    reorderOptions: async function() {
        try {
            // Get current DOM order
            const optionElements = document.querySelectorAll('.option-item');
            const updates = [];
            
            optionElements.forEach((el, index) => {
                const optionId = parseInt(el.dataset.optionId);
                updates.push({
                    optionId: optionId,
                    displayOrder: index + 1
                });
            });
            
            // Call API
            const response = await fetch(`/api/formbuilder/fields/${this.currentFieldId}/options/reorder`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updates)
            });
            
            if (!response.ok) {
                throw new Error('Failed to reorder options');
            }
            
            const result = await response.json();
            
            if (result.success) {
                // Update local array
                updates.forEach(update => {
                    const option = this.currentOptions.find(o => o.optionId === update.optionId);
                    if (option) {
                        option.displayOrder = update.displayOrder;
                    }
                });
                
                // Update canvas preview without full page reload
                FormBuilderProperties.updateCanvasPreview();
                
                console.log('Options reordered successfully');
            } else {
                throw new Error(result.message || 'Failed to reorder');
            }
            
        } catch (error) {
            console.error('Error reordering options:', error);
            alert('Failed to reorder options: ' + error.message);
        }
    },

    /**
     * Generate value from label (auto-generation)
     * @param {string} label - Option label
     * @returns {string} Generated value
     */
    generateValueFromLabel: function(label) {
        return label
            .toLowerCase()
            .trim()
            .replace(/[^a-z0-9]+/g, '_')
            .replace(/^_+|_+$/g, '');
    },

    /**
     * Auto-generate value from label as user types
     * @param {number} optionId - Option ID
     */
    autoGenerateValueFromLabel: function(optionId) {
        const labelInput = document.querySelector(`input.option-label-input[data-option-id="${optionId}"]`);
        const valueInput = document.querySelector(`input.option-value-input[data-option-id="${optionId}"]`);
        
        if (!labelInput || !valueInput) return;
        
        // Only auto-generate if value is empty or was auto-generated
        if (valueInput.dataset.autoGenerated === 'true' || !valueInput.value) {
            const generatedValue = this.generateValueFromLabel(labelInput.value);
            valueInput.value = generatedValue;
            valueInput.dataset.autoGenerated = 'true';
        }
    },

    /**
     * Show options manager
     */
    showOptionsManager: function() {
        const manager = document.getElementById('field-options-manager');
        if (manager) {
            manager.style.display = 'block';
            
            // Show config-field container
            const configField = document.getElementById('config-field');
            if (configField) {
                configField.style.display = 'block';
            }
        }
    },

    /**
     * Hide options manager
     */
    hideOptionsManager: function() {
        const manager = document.getElementById('field-options-manager');
        if (manager) {
            manager.style.display = 'none';
        }
        
        // Destroy sortable
        this.destroySortable();
    },

    /**
     * Focus on option for editing
     * @param {number} optionId - Option ID to focus
     */
    focusOptionForEdit: function(optionId) {
        const valueInput = document.querySelector(`input.option-value-input[data-option-id="${optionId}"]`);
        if (valueInput) {
            valueInput.focus();
            valueInput.select();
        }
    }
};

/**
 * Global function to save section properties (called from HTML)
 */
function saveSectionProperties() {
    FormBuilderProperties.saveSectionProperties();
}

/**
 * Global function to save section configuration (called from HTML)
 */
function saveSectionConfiguration() {
    FormBuilderProperties.saveSectionConfiguration();
}

/**
 * Global function to select and load section properties (called from HTML)
 * @param {number} sectionId - Section ID to select
 */
function selectSection(sectionId) {
    // Update visual selection
    FormBuilderLayout.selectSection(sectionId);

    // Load properties for General tab (default active tab)
    FormBuilderProperties.loadSectionProperties(sectionId);

    // Load configuration for Configuration tab
    FormBuilderProperties.loadSectionConfiguration(sectionId);
}

/**
 * Global function to select a field (called from HTML)
 * @param {number} fieldId - Field ID to select
 */
function selectField(fieldId) {
    // Remove all existing selections (sections and fields)
    document.querySelectorAll('.builder-section.selected, .builder-section.selected-element').forEach(el => {
        el.classList.remove('selected', 'selected-element');
    });
    
    document.querySelectorAll('.builder-field-card.selected-element').forEach(el => {
        el.classList.remove('selected-element');
    });

    // Remove section-has-selected-field class from all sections
    document.querySelectorAll('.section-has-selected-field').forEach(el => {
        el.classList.remove('section-has-selected-field');
    });

    // Add selected class to clicked field
    const fieldCard = document.getElementById(`field-${fieldId}`);
    if (fieldCard) {
        fieldCard.classList.add('selected-element');
        
        // Ensure field is expanded when selected
        const fieldBody = document.getElementById(`field-body-${fieldId}`);
        const fieldFooter = document.getElementById(`field-footer-${fieldId}`);
        const collapseIcon = document.getElementById(`field-collapse-icon-${fieldId}`);
        
        if (fieldBody && fieldBody.style.display === 'none') {
            // Expand the field - show body, hide footer
            fieldBody.style.display = 'block';
            if (fieldFooter) fieldFooter.style.display = 'none';
            if (collapseIcon) {
                collapseIcon.classList.remove('ri-add-line');
                collapseIcon.classList.add('ri-subtract-line');
            }
        }
        
        // Add class to parent section for :has() fallback
        const parentSection = fieldCard.closest('.builder-section');
        if (parentSection) {
            parentSection.classList.add('section-has-selected-field');
        }
        
        // Scroll into view if needed
        fieldCard.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }

    console.log(`Field ${fieldId} selected`);
    
    // Load field properties
    FormBuilderProperties.loadFieldProperties(fieldId);
}

/**
 * Global function to save field properties (called from HTML)
 */
function saveFieldProperties() {
    FormBuilderProperties.saveFieldProperties();
}

// ============================================================================
// GLOBAL FUNCTIONS FOR OPTIONS MANAGEMENT (called from HTML onclick handlers)
// ============================================================================

/**
 * Update option value (called on blur from _OptionsTable.cshtml)
 * @param {number} optionId - Option ID
 */
function updateOptionValue(optionId) {
    // Get the input values
    const valueInput = document.querySelector(`input.option-value-input[data-option-id="${optionId}"]`);
    const labelInput = document.querySelector(`input.option-label-input[data-option-id="${optionId}"]`);
    
    if (valueInput && window.FormBuilderTemplateOptions) {
        FormBuilderTemplateOptions.updateOption(optionId, 'value', valueInput.value);
    } else if (window.FormBuilderOptions) {
        FormBuilderOptions.updateOption(optionId);
    }
}

/**
 * Update option label (called on blur from _OptionsTable.cshtml)
 * @param {number} optionId - Option ID
 */
function updateOptionLabel(optionId) {
    const labelInput = document.querySelector(`input.option-label-input[data-option-id="${optionId}"]`);
    
    if (labelInput && window.FormBuilderTemplateOptions) {
        FormBuilderTemplateOptions.updateOption(optionId, 'label', labelInput.value);
    } else if (window.FormBuilderOptions) {
        FormBuilderOptions.updateOption(optionId);
    }
}

/**
 * Update option score (called on blur from _OptionsTable.cshtml)
 * @param {number} optionId - Option ID
 */
function updateOptionScore(optionId) {
    const scoreInput = document.querySelector(`input[data-option-id="${optionId}"][type="number"]`);
    
    if (scoreInput && window.FormBuilderTemplateOptions) {
        FormBuilderTemplateOptions.updateOption(optionId, 'score', scoreInput.value);
    }
}

/**
 * Auto-generate value from label (called on input)
 * @param {number} optionId - Option ID
 */
function autoGenerateValue(optionId) {
    const labelInput = document.querySelector(`input.option-label-input[data-option-id="${optionId}"]`);
    const valueInput = document.querySelector(`input.option-value-input[data-option-id="${optionId}"]`);
    
    // Only auto-generate if value input has data-auto-generated="true"
    if (labelInput && valueInput && valueInput.dataset.autoGenerated === 'true') {
        // Convert label to snake_case value
        const generatedValue = labelInput.value
            .toLowerCase()
            .trim()
            .replace(/[^a-z0-9\s]/g, '')
            .replace(/\s+/g, '_');
        
        valueInput.value = generatedValue;
    }
}

/**
 * Toggle option default (called on checkbox change from _OptionsTable.cshtml)
 * @param {number} optionId - Option ID
 */
function toggleOptionDefault(optionId) {
    if (window.FormBuilderTemplateOptions && typeof window.FormBuilderTemplateOptions.setDefaultOption === 'function') {
        const checkbox = document.querySelector(`#default_${optionId}`);
        FormBuilderTemplateOptions.setDefaultOption(optionId, checkbox ? checkbox.checked : false);
    } else if (window.FormBuilderOptions) {
        FormBuilderOptions.setDefaultOption(optionId);
    }
}

/**
 * Delete option with confirmation (called on button click from _OptionsTable.cshtml)
 * @param {number} optionId - Option ID
 */
function deleteOptionConfirm(optionId) {
    // Use FormBuilderTemplateOptions if available (has its own confirm)
    if (window.FormBuilderTemplateOptions && typeof window.FormBuilderTemplateOptions.deleteOption === 'function') {
        FormBuilderTemplateOptions.deleteOption(optionId);
        return;
    }
    
    // Fallback to FormBuilderOptions with modal
    if (window.FormBuilderOptions) {
        const option = FormBuilderOptions.currentOptions.find(o => o.optionId === optionId);

        if (!option) {
            console.error('Option not found:', optionId);
            return;
        }

        // Store pending delete option ID
        FormBuilderOptions.pendingDeleteOptionId = optionId;

        // Populate modal with option details
        document.getElementById('deleteOptionLabel').textContent = option.optionLabel || '-';
        document.getElementById('deleteOptionValue').textContent = option.optionValue || '-';

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('deleteOptionModal'));
        modal.show();
    }
}

/**
 * Focus on option for editing (called on edit button click)
 * @param {number} optionId - Option ID
 */
function focusOptionForEdit(optionId) {
    // Focus on the label input for editing
    const labelInput = document.querySelector(`input.option-label-input[data-option-id="${optionId}"]`);
    if (labelInput) {
        labelInput.focus();
        labelInput.select();
    } else if (window.FormBuilderOptions) {
        FormBuilderOptions.focusOptionForEdit(optionId);
    }
}

/**
 * Add a new option (called on Add Option button click)
 * Prefers FormBuilderTemplateOptions module, falls back to FormBuilderOptions
 */
function addOption() {
    if (window.FormBuilderTemplateOptions && typeof window.FormBuilderTemplateOptions.addOption === 'function') {
        FormBuilderTemplateOptions.addOption();
    } else if (window.FormBuilderOptions && typeof FormBuilderOptions.addOption === 'function') {
        FormBuilderOptions.addOption();
    } else {
        console.error('[addOption] No options module available');
        alert('Unable to add option. Please refresh the page.');
    }
}

// ============================================================================
// VALIDATION MANAGEMENT MODULE
// ============================================================================

/**
 * Validation Management Module
 * Handles CRUD operations for field validation rules
 */
const FormBuilderValidation = {
    // State management
    currentFieldId: null,
    currentFieldType: null,
    currentValidations: [],
    sortableInstance: null,
    selectedValidations: new Set(), // Track selected validation types

    // Enum mapping: FormFieldType enum values to string names
    fieldTypeEnum: {
        1: 'Text',
        2: 'TextArea',
        3: 'Number',
        4: 'Decimal',
        5: 'Date',
        6: 'Time',
        7: 'DateTime',
        8: 'Dropdown',
        9: 'Radio',
        10: 'Checkbox',
        11: 'MultiSelect',
        12: 'FileUpload',
        13: 'Image',
        14: 'Signature',
        15: 'Rating',
        16: 'Slider',
        17: 'Email',
        18: 'Phone',
        19: 'URL',
        20: 'Currency',
        21: 'Percentage'
    },

    /**
     * Load validations for a field
     * @param {object} fieldData - Field data containing validations
     */
    loadValidations: async function(fieldData) {
        console.log('🔍 Loading validations for field:', fieldData);

        this.currentFieldId = fieldData.itemId;

        // Get field type - use dataTypeName (string) or dataType (enum number)
        // Priority: dataTypeName (string) > dataType (enum number)
        let rawFieldType = fieldData.dataTypeName || fieldData.dataType;

        if (typeof rawFieldType === 'number') {
            // Convert enum number to string name
            this.currentFieldType = this.fieldTypeEnum[rawFieldType] || 'Text';
            console.log(`✅ Converted field type: ${rawFieldType} (enum) → "${this.currentFieldType}" (string)`);
        } else if (typeof rawFieldType === 'string') {
            this.currentFieldType = rawFieldType;
            console.log(`✅ Field type detected: "${this.currentFieldType}"`);
        } else {
            this.currentFieldType = null;
            console.error('❌ Field type is invalid or missing. fieldData:', fieldData);
        }

        // Show field type prominently
        this.displayFieldTypeInfo();
        
        try {
            // Fetch validations from API
            const response = await fetch(`/api/formbuilder/fields/${fieldData.itemId}/validations`);
            const result = await response.json();
            
            if (result.success) {
                this.currentValidations = result.data || [];
                this.renderValidationsList(this.currentValidations);
                this.initializeSortable();
                
                // Update validation checkboxes based on field type
                this.updateValidationCheckboxes();
            } else {
                console.error('Failed to load validations:', result.message);
                this.currentValidations = [];
                this.renderValidationsList([]);
            }
        } catch (error) {
            console.error('Error loading validations:', error);
            this.currentValidations = [];
            this.renderValidationsList([]);
        }
    },

    /**
     * Get applicable validation types for a field type
     * @param {string} fieldType - Field type
     * @returns {Array} Array of applicable validation types
     */
    getApplicableValidationTypes: function(fieldType) {
        const validationsByFieldType = {
            // Text-based fields
            'Text': ['Required', 'MinLength', 'MaxLength', 'Email', 'Phone', 'URL', 'Pattern'],
            'TextArea': ['Required', 'MinLength', 'MaxLength', 'Pattern'],
            'Email': ['Required', 'Email', 'MinLength', 'MaxLength'],
            'Phone': ['Required', 'Phone', 'MinLength', 'MaxLength'],
            'URL': ['Required', 'URL', 'MinLength', 'MaxLength'],
            
            // Numeric fields
            'Number': ['Required', 'MinValue', 'MaxValue', 'Range', 'Integer', 'Decimal', 'Number'],
            'Decimal': ['Required', 'MinValue', 'MaxValue', 'Range', 'Decimal'],
            'Currency': ['Required', 'MinValue', 'MaxValue', 'Range', 'Decimal'],
            'Percentage': ['Required', 'MinValue', 'MaxValue', 'Range', 'Decimal'],
            
            // Date/Time fields
            'Date': ['Required', 'Date', 'MinValue', 'MaxValue', 'Range'],
            'DateTime': ['Required', 'Date', 'MinValue', 'MaxValue', 'Range'],
            'Time': ['Required'],
            
            // Selection fields
            'Dropdown': ['Required'],
            'Radio': ['Required'],
            'Checkbox': ['Required'],
            'MultiSelect': ['Required'],
            
            // File upload
            'FileUpload': ['Required', 'Pattern'],
            'Image': ['Required'],
            'Signature': ['Required'],
            
            // Rating fields
            'Rating': ['Required', 'MinValue', 'MaxValue'],
            'Slider': ['Required', 'MinValue', 'MaxValue'],
            
            // Default (if field type not found)
            'Default': ['Required', 'MinLength', 'MaxLength', 'Pattern', 'Custom']
        };
        
        return validationsByFieldType[fieldType] || validationsByFieldType['Default'];
    },

    /**
     * Display field type information prominently at the top
     */
    displayFieldTypeInfo: function() {
        const infoBox = document.getElementById('field-type-info');
        const displaySpan = document.getElementById('current-field-type-display');
        
        if (!infoBox || !displaySpan) return;
        
        if (this.currentFieldType) {
            displaySpan.textContent = this.currentFieldType;
            displaySpan.className = 'badge bg-success ms-2';
            displaySpan.style.fontSize = '0.9rem';
            infoBox.style.display = 'block';
        } else {
            displaySpan.textContent = 'Not Detected!';
            displaySpan.className = 'badge bg-danger ms-2';
            displaySpan.style.fontSize = '0.9rem';
            infoBox.style.display = 'block';
        }
        
        console.log('📊 Field Type Display Updated:', this.currentFieldType);
    },

    /**
     * Update validation checkboxes visibility based on current field type
     */
    updateValidationCheckboxes: function() {
        // First, ensure all checkboxes have initial display (for first load)
        const allCheckboxes = document.querySelectorAll('.validation-checkbox');
        allCheckboxes.forEach(checkbox => {
            if (!checkbox.style.display) {
                checkbox.style.display = 'inline-flex';
            }
        });
        
        if (!this.currentFieldType) {
            console.warn('⚠️ No field type set for validation filtering');
            // Show all if no field type
            allCheckboxes.forEach(checkbox => {
                checkbox.style.display = 'inline-flex';
            });
            return;
        }
        
        const applicableTypes = this.getApplicableValidationTypes(this.currentFieldType);
        console.log('🔍 VALIDATION FILTER DEBUG:', { 
            fieldType: this.currentFieldType, 
            applicableTypes: applicableTypes,
            allCheckboxCount: allCheckboxes.length,
            checkboxTypes: Array.from(allCheckboxes).map(cb => cb.dataset.validationType)
        });
        
        // Show/hide checkboxes based on applicable types
        let visibleCount = 0;
        let hiddenCount = 0;
        
        allCheckboxes.forEach(checkbox => {
            const validationType = checkbox.dataset.validationType;
            
            if (applicableTypes.includes(validationType)) {
                checkbox.style.display = 'inline-flex';
                checkbox.style.visibility = 'visible';
                checkbox.style.opacity = '1';
                visibleCount++;
            } else {
                checkbox.style.display = 'none';
                checkbox.style.visibility = 'hidden';
                checkbox.style.opacity = '0';
                hiddenCount++;
                
                // Uncheck if it was checked
                const input = checkbox.querySelector('input[type="checkbox"]');
                if (input && input.checked) {
                    input.checked = false;
                    this.selectedValidations.delete(validationType);
                }
            }
        });
        
        console.log('✅ Filtered checkboxes:', { 
            visible: visibleCount, 
            hidden: hiddenCount,
            visibleTypes: Array.from(allCheckboxes)
                .filter(cb => cb.style.display !== 'none')
                .map(cb => cb.dataset.validationType)
        });
        
        // Hide groups that have no visible checkboxes
        const groups = document.querySelectorAll('.validation-group');
        groups.forEach(group => {
            const checkboxes = group.querySelectorAll('.validation-checkbox');
            let hasVisible = false;
            
            checkboxes.forEach(cb => {
                if (cb.style.display !== 'none') {
                    hasVisible = true;
                }
            });
            
            if (hasVisible) {
                group.style.display = 'block';
            } else {
                group.style.display = 'none';
            }
            
            console.log(`Group ${group.dataset.group}: ${hasVisible ? 'VISIBLE' : 'HIDDEN'}`);
        });
        
        // Clear any previous selections
        this.clearSelection();
    },

    /**
     * Handle validation checkbox change
     * @param {string} validationType - The validation type that was checked/unchecked
     */
    onValidationCheckChange: function(validationType) {
        const checkbox = document.getElementById(`val-${validationType.toLowerCase()}`);
        
        if (checkbox && checkbox.checked) {
            this.selectedValidations.add(validationType);
        } else {
            this.selectedValidations.delete(validationType);
        }
        
        // Update the configuration area
        this.updateSelectedValidationsUI();
    },

    /**
     * Update the UI to show configuration forms for selected validations
     */
    updateSelectedValidationsUI: function() {
        const container = document.getElementById('selected-validations-container');
        const listContainer = document.getElementById('selected-validations-list');
        
        if (!container || !listContainer) return;
        
        if (this.selectedValidations.size === 0) {
            container.style.display = 'none';
            return;
        }
        
        container.style.display = 'block';
        
        // Build configuration forms for each selected validation
        let html = '';
        this.selectedValidations.forEach(validationType => {
            html += this.createValidationConfigForm(validationType);
        });
        
        listContainer.innerHTML = html;
    },

    /**
     * Create configuration form for a validation type
     * @param {string} validationType - The validation type
     * @returns {string} HTML string
     */
    createValidationConfigForm: function(validationType) {
        const parametersHtml = this.getParametersHtmlForType(validationType);
        const defaultMessage = this.getDefaultErrorMessage(validationType);
        
        return `
            <div class="card mb-3" data-validation-config="${validationType}">
                <div class="card-body">
                    <h6 class="mb-3"><i class="ri-checkbox-circle-line text-success"></i> ${validationType}</h6>
                    
                    ${parametersHtml ? `
                        <div class="row g-2 mb-3">
                            ${parametersHtml}
                        </div>
                    ` : ''}
                    
                    <div class="row g-2 mb-2">
                        <div class="col-md-6">
                            <label class="form-label">Severity</label>
                            <select class="form-select form-select-sm validation-severity" data-validation-type="${validationType}">
                                <option value="Error" selected>Error (Block submission)</option>
                                <option value="Warning">Warning (Allow submission)</option>
                                <option value="Info">Info (Just notify)</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">Error Message</label>
                            <input type="text" class="form-control form-control-sm validation-message" 
                                   data-validation-type="${validationType}"
                                   placeholder="${defaultMessage}">
                        </div>
                    </div>
                </div>
            </div>
        `;
    },

    /**
     * Clear all validation selections
     */
    clearSelection: function() {
        // Uncheck all checkboxes
        const checkboxes = document.querySelectorAll('.validation-checkbox input[type="checkbox"]');
        checkboxes.forEach(cb => cb.checked = false);
        
        // Clear set
        this.selectedValidations.clear();
        
        // Hide configuration area
        const container = document.getElementById('selected-validations-container');
        if (container) container.style.display = 'none';
    },

    /**
     * Add all selected validations
     */
    addSelectedValidations: async function() {
        if (!this.currentFieldId) {
            alert('No field selected');
            return;
        }
        
        if (this.selectedValidations.size === 0) {
            alert('No validations selected');
            return;
        }
        
        const validationsToAdd = [];
        
        // Collect data for each selected validation
        for (const validationType of this.selectedValidations) {
            const dto = await this.collectValidationData(validationType);
            if (dto) {
                validationsToAdd.push(dto);
            } else {
                return; // Stop if validation failed
            }
        }
        
        // Add all validations
        for (const dto of validationsToAdd) {
            try {
                const response = await fetch(`/api/formbuilder/fields/${this.currentFieldId}/validations`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(dto)
                });
                
                const result = await response.json();
                
                if (!result.success) {
                    alert('Failed to add validation: ' + result.message);
                    return;
                }
            } catch (error) {
                console.error('Error adding validation:', error);
                alert('Error adding validation');
                return;
            }
        }
        
        // Reload validations
        await this.loadValidations({ itemId: this.currentFieldId });
        
        // Clear selection
        this.clearSelection();
        
        // Update validation count
        this.updateValidationCount();
        
        // Update canvas preview
        this.updateCanvasPreview();
        
        console.log('All validations added successfully');
    },

    /**
     * Collect validation data from form
     * @param {string} validationType - The validation type
     * @returns {object|null} DTO object or null if validation failed
     */
    collectValidationData: async function(validationType) {
        const severity = document.querySelector(`.validation-severity[data-validation-type="${validationType}"]`)?.value || 'Error';
        const message = document.querySelector(`.validation-message[data-validation-type="${validationType}"]`)?.value || '';
        
        const dto = {
            validationType: validationType,
            severity: severity,
            errorMessage: message
        };
        
        // Add type-specific parameters
        switch (validationType) {
            case 'MinLength':
                const minLengthInput = document.getElementById('param-minlength');
                if (!minLengthInput?.value) {
                    alert('Please enter minimum length for MinLength validation');
                    return null;
                }
                dto.minLength = parseInt(minLengthInput.value);
                break;
            
            case 'MaxLength':
                const maxLengthInput = document.getElementById('param-maxlength');
                if (!maxLengthInput?.value) {
                    alert('Please enter maximum length for MaxLength validation');
                    return null;
                }
                dto.maxLength = parseInt(maxLengthInput.value);
                break;
            
            case 'MinValue':
                const minValueInput = document.getElementById('param-minvalue');
                if (!minValueInput?.value) {
                    alert('Please enter minimum value for MinValue validation');
                    return null;
                }
                dto.minValue = parseFloat(minValueInput.value);
                break;
            
            case 'MaxValue':
                const maxValueInput = document.getElementById('param-maxvalue');
                if (!maxValueInput?.value) {
                    alert('Please enter maximum value for MaxValue validation');
                    return null;
                }
                dto.maxValue = parseFloat(maxValueInput.value);
                break;
            
            case 'Range':
                const rangeMinInput = document.getElementById('param-minvalue');
                const rangeMaxInput = document.getElementById('param-maxvalue');
                if (!rangeMinInput?.value || !rangeMaxInput?.value) {
                    alert('Please enter both minimum and maximum values for Range validation');
                    return null;
                }
                dto.minValue = parseFloat(rangeMinInput.value);
                dto.maxValue = parseFloat(rangeMaxInput.value);
                break;
            
            case 'Pattern':
                const patternInput = document.getElementById('param-pattern');
                if (!patternInput?.value) {
                    alert('Please enter a regex pattern for Pattern validation');
                    return null;
                }
                dto.regexPattern = patternInput.value;
                break;
            
            case 'Custom':
                const customInput = document.getElementById('param-custom');
                if (!customInput?.value) {
                    alert('Please enter custom expression for Custom validation');
                    return null;
                }
                dto.customExpression = customInput.value;
                break;
        }
        
        return dto;
    },

    /**
     * Render the list of validations
     * @param {Array} validations - Array of validation objects
     */
    renderValidationsList: function(validations) {
        const listContainer = document.getElementById('validations-list');
        const emptyState = document.getElementById('validations-empty');
        const countBadge = document.getElementById('validation-count-badge');
        
        if (!listContainer) return;
        
        // Update count badge
        if (countBadge) {
            countBadge.textContent = validations.length;
        }
        
        // Show/hide empty state
        if (validations.length === 0) {
            listContainer.innerHTML = '';
            if (emptyState) emptyState.style.display = 'block';
            return;
        }
        
        if (emptyState) emptyState.style.display = 'none';
        
        // Render validation rows
        listContainer.innerHTML = validations.map(validation => 
            this.createValidationRowHtml(validation)
        ).join('');
    },

    /**
     * Create HTML for a single validation row
     * @param {object} validation - Validation object
     * @returns {string} HTML string
     */
    createValidationRowHtml: function(validation) {
        const ruleDetails = this.getValidationRuleDetails(validation);
        const severityBadge = this.getSeverityBadgeHtml(validation.severity);
        
        return `
            <tr class="validation-item" data-validation-id="${validation.validationId}">
                <!-- Drag Handle -->
                <td>
                    <i class="ri-draggable validation-drag-handle"></i>
                </td>
                
                <!-- Type -->
                <td>
                    <strong>${validation.validationType}</strong>
                    ${severityBadge}
                </td>
                
                <!-- Rule Details -->
                <td>
                    <small class="text-muted">${ruleDetails}</small>
                </td>
                
                <!-- Error Message -->
                <td>
                    <small>${validation.errorMessage}</small>
                </td>
                
                <!-- Actions -->
                <td class="text-end">
                    <button type="button" 
                            class="btn btn-sm btn-ghost-secondary p-1" 
                            onclick="editValidation(${validation.validationId})"
                            title="Edit rule">
                        <i class="ri-edit-line"></i>
                    </button>
                    <button type="button" 
                            class="btn btn-sm btn-ghost-danger p-1" 
                            onclick="deleteValidationConfirm(${validation.validationId})"
                            title="Delete rule">
                        <i class="ri-delete-bin-line"></i>
                    </button>
                </td>
            </tr>
        `;
    },

    /**
     * Get formatted rule details based on validation type
     * @param {object} validation - Validation object
     * @returns {string} Formatted details
     */
    getValidationRuleDetails: function(validation) {
        switch (validation.validationType) {
            case 'Required':
                return 'Field must have a value';
            case 'MinLength':
                return `Minimum ${validation.minLength} characters`;
            case 'MaxLength':
                return `Maximum ${validation.maxLength} characters`;
            case 'MinValue':
                return `Minimum value: ${validation.minValue}`;
            case 'MaxValue':
                return `Maximum value: ${validation.maxValue}`;
            case 'Range':
                return `Range: ${validation.minValue} - ${validation.maxValue}`;
            case 'Pattern':
                return `Pattern: ${validation.regexPattern}`;
            case 'Email':
                return 'Must be valid email format';
            case 'Phone':
                return 'Must be valid phone number';
            case 'URL':
                return 'Must be valid URL';
            case 'Integer':
                return 'Must be whole number';
            case 'Decimal':
                return 'Must be decimal number';
            case 'Number':
                return 'Must be numeric';
            case 'Date':
                return 'Must be valid date';
            case 'Custom':
                return validation.customExpression || 'Custom validation';
            default:
                return validation.validationType;
        }
    },

    /**
     * Get severity badge HTML
     * @param {string} severity - Severity level
     * @returns {string} Badge HTML
     */
    getSeverityBadgeHtml: function(severity) {
        const badges = {
            'Error': '<span class="badge badge-soft-danger ms-1">Error</span>',
            'Warning': '<span class="badge badge-soft-warning ms-1">Warning</span>',
            'Info': '<span class="badge badge-soft-info ms-1">Info</span>'
        };
        return badges[severity] || '';
    },

    /**
     * Handle validation type change - show/hide relevant parameter inputs
     */
    onValidationTypeChange: function() {
        const typeSelect = document.getElementById('new-validation-type');
        const parametersContainer = document.getElementById('validation-parameters');
        const messageInput = document.getElementById('new-validation-message');
        
        if (!typeSelect || !parametersContainer) return;
        
        const selectedType = typeSelect.value;
        
        if (!selectedType) {
            parametersContainer.style.display = 'none';
            parametersContainer.innerHTML = '';
            return;
        }
        
        // Generate dynamic inputs based on validation type
        const parametersHtml = this.getParametersHtmlForType(selectedType);
        
        if (parametersHtml) {
            parametersContainer.innerHTML = parametersHtml;
            parametersContainer.style.display = 'block';
        } else {
            parametersContainer.innerHTML = '';
            parametersContainer.style.display = 'none';
        }
        
        // Set default error message
        if (messageInput && !messageInput.value) {
            messageInput.placeholder = this.getDefaultErrorMessage(selectedType);
        }
    },

    /**
     * Get parameter inputs HTML based on validation type
     * @param {string} type - Validation type
     * @returns {string} HTML for parameter inputs
     */
    getParametersHtmlForType: function(type) {
        switch (type) {
            case 'MinLength':
                return `
                    <div class="col-md-6">
                        <label class="form-label">Minimum Length</label>
                        <input type="number" id="param-minlength" class="form-control form-control-sm" 
                               placeholder="e.g., 5" min="1" required>
                    </div>
                `;
            
            case 'MaxLength':
                return `
                    <div class="col-md-6">
                        <label class="form-label">Maximum Length</label>
                        <input type="number" id="param-maxlength" class="form-control form-control-sm" 
                               placeholder="e.g., 100" min="1" required>
                    </div>
                `;
            
            case 'MinValue':
                return `
                    <div class="col-md-6">
                        <label class="form-label">Minimum Value</label>
                        <input type="number" id="param-minvalue" class="form-control form-control-sm" 
                               placeholder="e.g., 0" step="any" required>
                    </div>
                `;
            
            case 'MaxValue':
                return `
                    <div class="col-md-6">
                        <label class="form-label">Maximum Value</label>
                        <input type="number" id="param-maxvalue" class="form-control form-control-sm" 
                               placeholder="e.g., 100" step="any" required>
                    </div>
                `;
            
            case 'Range':
                return `
                    <div class="col-md-6">
                        <label class="form-label">Minimum Value</label>
                        <input type="number" id="param-minvalue" class="form-control form-control-sm" 
                               placeholder="e.g., 0" step="any" required>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Maximum Value</label>
                        <input type="number" id="param-maxvalue" class="form-control form-control-sm" 
                               placeholder="e.g., 100" step="any" required>
                    </div>
                `;
            
            case 'Pattern':
                return `
                    <div class="col-12">
                        <label class="form-label">Regex Pattern</label>
                        <input type="text" id="param-pattern" class="form-control form-control-sm" 
                               placeholder="e.g., ^[A-Z]{3}[0-9]{4}$" required>
                        <small class="text-muted">Enter a valid regular expression pattern</small>
                    </div>
                `;
            
            case 'Custom':
                return `
                    <div class="col-12">
                        <label class="form-label">Custom Expression</label>
                        <textarea id="param-custom" class="form-control form-control-sm" 
                                  placeholder="Enter custom validation logic" rows="3" required></textarea>
                        <small class="text-muted">Advanced: JavaScript expression for validation</small>
                    </div>
                `;
            
            default:
                return ''; // No parameters needed for simple types
        }
    },

    /**
     * Get default error message for validation type
     * @param {string} type - Validation type
     * @returns {string} Default error message
     */
    getDefaultErrorMessage: function(type) {
        const messages = {
            'Required': 'This field is required.',
            'Email': 'Please enter a valid email address.',
            'Phone': 'Please enter a valid phone number.',
            'URL': 'Please enter a valid URL.',
            'MinLength': 'Input is too short.',
            'MaxLength': 'Input is too long.',
            'MinValue': 'Value is too low.',
            'MaxValue': 'Value is too high.',
            'Range': 'Value is out of range.',
            'Pattern': 'Input does not match the required pattern.',
            'Integer': 'Please enter a whole number.',
            'Decimal': 'Please enter a valid decimal number.',
            'Number': 'Please enter a valid number.',
            'Date': 'Please enter a valid date.',
            'Custom': 'Invalid input.'
        };
        return messages[type] || 'Invalid input.';
    },

    /**
     * Add a new validation rule
     */
    addValidation: async function() {
        if (!this.currentFieldId) {
            alert('No field selected');
            return;
        }
        
        const typeSelect = document.getElementById('new-validation-type');
        const severitySelect = document.getElementById('new-validation-severity');
        const messageInput = document.getElementById('new-validation-message');
        
        if (!typeSelect || !typeSelect.value) {
            alert('Please select a validation type');
            return;
        }
        
        const validationType = typeSelect.value;
        
        // Collect parameters based on type
        const dto = {
            validationType: validationType,
            severity: severitySelect?.value || 'Error',
            errorMessage: messageInput?.value || ''
        };
        
        // Add type-specific parameters
        switch (validationType) {
            case 'MinLength':
                const minLengthInput = document.getElementById('param-minlength');
                if (!minLengthInput?.value) {
                    alert('Please enter minimum length');
                    return;
                }
                dto.minLength = parseInt(minLengthInput.value);
                break;
            
            case 'MaxLength':
                const maxLengthInput = document.getElementById('param-maxlength');
                if (!maxLengthInput?.value) {
                    alert('Please enter maximum length');
                    return;
                }
                dto.maxLength = parseInt(maxLengthInput.value);
                break;
            
            case 'MinValue':
                const minValueInput = document.getElementById('param-minvalue');
                if (!minValueInput?.value) {
                    alert('Please enter minimum value');
                    return;
                }
                dto.minValue = parseFloat(minValueInput.value);
                break;
            
            case 'MaxValue':
                const maxValueInput = document.getElementById('param-maxvalue');
                if (!maxValueInput?.value) {
                    alert('Please enter maximum value');
                    return;
                }
                dto.maxValue = parseFloat(maxValueInput.value);
                break;
            
            case 'Range':
                const rangeMinInput = document.getElementById('param-minvalue');
                const rangeMaxInput = document.getElementById('param-maxvalue');
                if (!rangeMinInput?.value || !rangeMaxInput?.value) {
                    alert('Please enter both minimum and maximum values');
                    return;
                }
                dto.minValue = parseFloat(rangeMinInput.value);
                dto.maxValue = parseFloat(rangeMaxInput.value);
                break;
            
            case 'Pattern':
                const patternInput = document.getElementById('param-pattern');
                if (!patternInput?.value) {
                    alert('Please enter a regex pattern');
                    return;
                }
                dto.regexPattern = patternInput.value;
                break;
            
            case 'Custom':
                const customInput = document.getElementById('param-custom');
                if (!customInput?.value) {
                    alert('Please enter custom expression');
                    return;
                }
                dto.customExpression = customInput.value;
                break;
        }
        
        try {
            const response = await fetch(`/api/formbuilder/fields/${this.currentFieldId}/validations`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Reload validations
                await this.loadValidations({ itemId: this.currentFieldId });
                
                // Clear form
                this.clearAddForm();
                
                // Update validation count in properties panel
                this.updateValidationCount();
                
                // Update canvas preview
                FormBuilderProperties.updateCanvasPreview();
                
                console.log('Validation added successfully');
            } else {
                alert('Failed to add validation: ' + result.message);
            }
        } catch (error) {
            console.error('Error adding validation:', error);
            alert('Error adding validation');
        }
    },

    /**
     * Clear the add validation form
     */
    clearAddForm: function() {
        const typeSelect = document.getElementById('new-validation-type');
        const severitySelect = document.getElementById('new-validation-severity');
        const messageInput = document.getElementById('new-validation-message');
        const parametersContainer = document.getElementById('validation-parameters');
        
        if (typeSelect) typeSelect.value = '';
        if (severitySelect) severitySelect.value = 'Error';
        if (messageInput) messageInput.value = '';
        if (parametersContainer) {
            parametersContainer.innerHTML = '';
            parametersContainer.style.display = 'none';
        }
    },

    /**
     * Delete a validation rule
     * @param {number} validationId - Validation ID
     */
    deleteValidation: async function(validationId) {
        try {
            const response = await fetch(`/api/formbuilder/validations/${validationId}`, {
                method: 'DELETE'
            });
            
            const result = await response.json();
            
            if (result.success) {
                // Reload validations
                await this.loadValidations({ itemId: this.currentFieldId });
                
                // Update validation count
                this.updateValidationCount();
                
                // Update canvas preview
                FormBuilderProperties.updateCanvasPreview();
                
                console.log('Validation deleted successfully');
            } else {
                alert('Failed to delete validation: ' + result.message);
            }
        } catch (error) {
            console.error('Error deleting validation:', error);
            alert('Error deleting validation');
        }
    },

    /**
     * Initialize SortableJS for drag-drop reordering
     */
    initializeSortable: function() {
        // Destroy existing instance
        this.destroySortable();
        
        const listContainer = document.getElementById('validations-list');
        if (!listContainer) return;
        
        // Only initialize if there are items
        if (this.currentValidations.length === 0) return;
        
        this.sortableInstance = new Sortable(listContainer, {
            handle: '.validation-drag-handle',
            animation: 150,
            ghostClass: 'sortable-ghost',
            dragClass: 'sortable-drag',
            onEnd: () => {
                this.reorderValidations();
            }
        });
    },

    /**
     * Destroy SortableJS instance
     */
    destroySortable: function() {
        if (this.sortableInstance) {
            this.sortableInstance.destroy();
            this.sortableInstance = null;
        }
    },

    /**
     * Reorder validations after drag-drop
     */
    reorderValidations: async function() {
        const rows = document.querySelectorAll('#validations-list .validation-item');
        const updates = Array.from(rows).map((row, index) => ({
            validationId: parseInt(row.dataset.validationId),
            validationOrder: index + 1
        }));
        
        try {
            const response = await fetch(`/api/formbuilder/fields/${this.currentFieldId}/validations/reorder`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updates)
            });
            
            const result = await response.json();
            
            if (result.success) {
                console.log('Validations reordered successfully');
                // No need to reload or update canvas for reorder
            } else {
                alert('Failed to reorder validations: ' + result.message);
            }
        } catch (error) {
            console.error('Error reordering validations:', error);
        }
    },

    /**
     * Update validation count in properties panel
     */
    updateValidationCount: function() {
        const countElement = document.getElementById('field-validation-count');
        if (countElement && this.currentFieldId) {
            countElement.textContent = this.currentValidations.length;
        }
    },

    /**
     * Update canvas preview without full page reload
     */
    updateCanvasPreview: function() {
        // Update validation count in the canvas field element
        if (this.currentFieldId) {
            const fieldElement = document.querySelector(`[data-field-id="${this.currentFieldId}"]`);
            if (fieldElement) {
                const validationBadge = fieldElement.querySelector('.validation-count-badge');
                if (validationBadge) {
                    validationBadge.textContent = this.currentValidations.length;
                }
            }
        }
    }
};

// ============================================================================
// GLOBAL WRAPPER FUNCTIONS FOR VALIDATION MANAGEMENT
// ============================================================================

/**
 * Edit validation (called on edit button click)
 * @param {number} validationId - Validation ID
 */
function editValidation(validationId) {
    // TODO: Implement edit functionality (future enhancement)
    alert('Edit validation functionality coming soon');
}

/**
 * Delete validation with confirmation (called on delete button click)
 * @param {number} validationId - Validation ID
 */
function deleteValidationConfirm(validationId) {
    if (confirm('Are you sure you want to delete this validation rule?')) {
        FormBuilderValidation.deleteValidation(validationId);
    }
}

// ============================================================================
// FIELD-SPECIFIC CONFIGURATION MODULE
// Handles type-specific settings (min/max values, text formatting, etc.)
// ============================================================================

const FormBuilderConfiguration = {
    // State management
    currentFieldId: null,
    currentFieldType: null,
    saveTimeout: null,

    /**
     * Load configurations for a field
     * @param {object} fieldData - Field data including configuration values
     */
    loadConfiguration: function(fieldData) {
        console.log('📝 Loading field configuration:', fieldData);

        this.currentFieldId = fieldData.itemId;
        this.currentFieldType = fieldData.dataTypeName || 'Text';

        // Hide all config sections first
        this.hideAllConfigSections();

        // Show appropriate config section based on field type
        this.showConfigForFieldType(this.currentFieldType);

        // Populate config values
        this.populateConfigValues(fieldData);

        console.log(`✅ Configuration loaded for ${this.currentFieldType} field`);
    },

    /**
     * Hide all configuration sections
     */
    hideAllConfigSections: function() {
        document.getElementById('config-number-fields')?.style && (document.getElementById('config-number-fields').style.display = 'none');
        document.getElementById('config-text-fields')?.style && (document.getElementById('config-text-fields').style.display = 'none');
        document.getElementById('config-no-settings')?.style && (document.getElementById('config-no-settings').style.display = 'none');
    },

    /**
     * Show configuration section for specific field type
     * @param {string} fieldType - Field type name
     */
    showConfigForFieldType: function(fieldType) {
        // Number-based fields
        const numberTypes = ['Number', 'Decimal', 'Currency', 'Percentage'];
        if (numberTypes.includes(fieldType)) {
            document.getElementById('config-number-fields').style.display = 'block';
            return;
        }

        // Text-based fields
        const textTypes = ['Text', 'TextArea', 'Email', 'Phone', 'URL'];
        if (textTypes.includes(fieldType)) {
            document.getElementById('config-text-fields').style.display = 'block';
            return;
        }

        // No special configuration for other types
        document.getElementById('config-no-settings').style.display = 'block';
    },

    /**
     * Populate configuration input fields with current values
     * @param {object} fieldData - Field data with config properties
     */
    populateConfigValues: function(fieldData) {
        // Number field configurations
        this.setInputValue('config-minValue', fieldData.minValue);
        this.setInputValue('config-maxValue', fieldData.maxValue);
        this.setInputValue('config-step', fieldData.step);
        this.setSelectValue('config-decimalPlaces', fieldData.decimalPlaces);
        this.setCheckboxValue('config-allowNegative', fieldData.allowNegative);

        // Text field configurations
        this.setInputValue('config-minLength', fieldData.minLength);
        this.setInputValue('config-maxLength', fieldData.maxLength);
        this.setInputValue('config-inputMask', fieldData.inputMask);
        this.setSelectValue('config-textTransform', fieldData.textTransform);
        this.setCheckboxValue('config-autoTrim', fieldData.autoTrim ?? true); // Default to true
    },

    /**
     * Set input field value
     */
    setInputValue: function(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value ?? '';
        }
    },

    /**
     * Set select dropdown value
     */
    setSelectValue: function(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value ?? '';
        }
    },

    /**
     * Set checkbox checked state
     */
    setCheckboxValue: function(elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.checked = value === true;
        }
    },

    /**
     * Save configuration value (auto-save with debouncing)
     * @param {string} configKey - Configuration key (e.g., 'minValue', 'maxValue')
     * @param {any} value - Configuration value
     */
    saveConfig: function(configKey, value) {
        // Clear existing timeout
        if (this.saveTimeout) {
            clearTimeout(this.saveTimeout);
        }

        // Debounce: wait 500ms after user stops typing
        this.saveTimeout = setTimeout(() => {
            this.performSave(configKey, value);
        }, 500);
    },

    /**
     * Perform the actual save operation
     * @param {string} configKey - Configuration key
     * @param {any} value - Configuration value
     */
    performSave: async function(configKey, value) {
        if (!this.currentFieldId) {
            console.error('No field selected for configuration save');
            return;
        }

        // Convert value to appropriate type
        let processedValue = value;

        // Handle empty strings (clear the config)
        if (value === '' || value === null || value === undefined) {
            processedValue = null;
        }
        // Handle boolean checkbox values
        else if (typeof value === 'boolean') {
            processedValue = value;
        }
        // Handle numeric inputs
        else if (['minValue', 'maxValue', 'step', 'decimalPlaces', 'minLength', 'maxLength'].includes(configKey)) {
            processedValue = value === '' ? null : parseFloat(value);
        }

        // Client-side validation
        if (!this.validateConfig(configKey, processedValue)) {
            return; // Validation failed, don't save
        }

        console.log(`💾 Saving config: ${configKey} = ${processedValue}`);

        try {
            // Get current field data from FormBuilderProperties
            const currentFieldData = FormBuilderProperties.currentFieldData;
            if (!currentFieldData) {
                console.error('Current field data not available');
                return;
            }

            // Build update DTO with all required fields + the changed value
            const dto = {
                itemName: currentFieldData.itemName,
                itemDescription: currentFieldData.itemDescription,
                isRequired: currentFieldData.isRequired,
                placeholderText: currentFieldData.placeholderText,
                helpText: currentFieldData.helpText,
                prefixText: currentFieldData.prefixText,
                suffixText: currentFieldData.suffixText,
                defaultValue: currentFieldData.defaultValue
            };

            // Add the changed configuration value
            dto[configKey] = processedValue;

            const response = await fetch(`/api/formbuilder/fields/${this.currentFieldId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(dto)
            });

            const result = await response.json();

            if (result.success) {
                console.log(`✅ Configuration saved: ${configKey}`);
                // Update local field data with new value
                currentFieldData[configKey] = processedValue;
                // TODO: Update canvas preview if needed
            } else {
                console.error('Failed to save configuration:', result.message);
                alert('Failed to save: ' + result.message);
            }
        } catch (error) {
            console.error('Error saving configuration:', error);
            alert('Error saving configuration');
        }
    },

    /**
     * Validate configuration value before saving
     * @param {string} configKey - Configuration key
     * @param {any} value - Configuration value to validate
     * @returns {boolean} - True if valid, false otherwise
     */
    validateConfig: function(configKey, value) {
        // Skip validation for null values (clearing config)
        if (value === null || value === undefined) {
            return true;
        }

        // Validate min/max relationship
        if (configKey === 'minValue') {
            const maxValue = parseFloat(document.getElementById('config-maxValue')?.value);
            if (!isNaN(maxValue) && value > maxValue) {
                alert('Minimum value cannot be greater than maximum value');
                // Reset to previous value
                document.getElementById('config-minValue').value = '';
                return false;
            }
        }

        if (configKey === 'maxValue') {
            const minValue = parseFloat(document.getElementById('config-minValue')?.value);
            if (!isNaN(minValue) && value < minValue) {
                alert('Maximum value cannot be less than minimum value');
                // Reset to previous value
                document.getElementById('config-maxValue').value = '';
                return false;
            }
        }

        // Validate min/max length relationship
        if (configKey === 'minLength') {
            const maxLength = parseInt(document.getElementById('config-maxLength')?.value);
            if (!isNaN(maxLength) && value > maxLength) {
                alert('Minimum length cannot be greater than maximum length');
                document.getElementById('config-minLength').value = '';
                return false;
            }
        }

        if (configKey === 'maxLength') {
            const minLength = parseInt(document.getElementById('config-minLength')?.value);
            if (!isNaN(minLength) && value < minLength) {
                alert('Maximum length cannot be less than minimum length');
                document.getElementById('config-maxLength').value = '';
                return false;
            }
        }

        // Validate step is positive
        if (configKey === 'step' && value <= 0) {
            alert('Step value must be greater than 0');
            document.getElementById('config-step').value = '';
            return false;
        }

        // Validate decimal places is non-negative
        if (configKey === 'decimalPlaces' && value < 0) {
            alert('Decimal places cannot be negative');
            document.getElementById('config-decimalPlaces').value = '';
            return false;
        }

        return true;
    }
};

// ============================================================================
// INTEGRATION: Call configuration loader when field properties are loaded
// ============================================================================

// This will be called from FormBuilderProperties.loadFieldProperties()
// after the field data is fetched
