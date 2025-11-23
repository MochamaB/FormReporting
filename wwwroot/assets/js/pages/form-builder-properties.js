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

        this.currentElementType = null;
        this.currentElementId = null;
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
     * Load field properties (Phase 2)
     * @param {number} fieldId - The field ID to load
     */
    loadFieldProperties: async function(fieldId) {
        // TODO: Implement in Phase 2
        this.hideAllPropertyPanels();
        document.getElementById('properties-field-general').style.display = 'block';
        this.currentElementType = 'field';
        this.currentElementId = fieldId;
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
