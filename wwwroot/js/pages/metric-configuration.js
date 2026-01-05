/**
 * Field Mapping Wizard - Clean Implementation
 * Uses Razor partials for HTML structure, JavaScript for behavior only
 */

// ===================================================================
// INITIALIZATION
// ===================================================================

document.addEventListener('DOMContentLoaded', function() {
    initializeModalHandlers();
});

/**
 * Initialize modal event handlers
 */
function initializeModalHandlers() {
    const fieldMappingModal = document.getElementById('fieldMappingModal');
    if (fieldMappingModal) {
        fieldMappingModal.addEventListener('show.bs.modal', function(event) {
            const button = event.relatedTarget;
            const fieldId = button.getAttribute('data-field-id');
            
            if (fieldId) {
                loadFieldMappingWizard(fieldId);
            }
        });
    }
}

// ===================================================================
// WIZARD LOADING & INITIALIZATION
// ===================================================================

/**
 * Load field mapping wizard for a specific field
 * Calls the MVC controller to render the wizard partial
 */
function loadFieldMappingWizard(fieldId) {
    const modal = document.getElementById('fieldMappingModal');
    const modalLabel = document.getElementById('fieldMappingModalLabel');
    const contentDiv = document.getElementById('fieldMappingContent');
    
    if (!modal || !contentDiv) return;
    
    // Show loading state
    contentDiv.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading wizard...</span>
            </div>
            <div class="mt-2 text-muted">Preparing field mapping wizard...</div>
        </div>
    `;
    
    // Load wizard partial from MVC controller (renders proper Razor partial with HorizontalWizard)
    fetch(`/Metrics/Mapping/RenderFieldMappingWizard/${fieldId}`)
        .then(response => {
            // Always get the text first to see what the server returned
            return response.text().then(text => {
                if (!response.ok) {
                    console.error('Server response:', text);
                    throw new Error(`Server error (${response.status}): ${text.substring(0, 200)}`);
                }
                return text;
            });
        })
        .then(html => {
            // Insert the rendered wizard HTML
            contentDiv.innerHTML = html;
            
            // Update modal title from field data
            const fieldDataInput = document.getElementById('fieldDataJson');
            if (fieldDataInput) {
                try {
                    const fieldData = JSON.parse(fieldDataInput.value);
                    modalLabel.innerHTML = `<i class="ri-focus-3-line me-2"></i>Configure Field Mapping: ${fieldData.itemName}`;
                } catch (e) {
                    console.error('Error parsing field data:', e);
                }
            }
        })
        .catch(error => {
            console.error('Error loading wizard:', error);
            contentDiv.innerHTML = `<div class="alert alert-danger"><strong>Error:</strong> ${error.message}</div>`;
        });
}

// ===================================================================
// FIELD MAPPING WIZARD OBJECT
// ===================================================================

const FieldMappingWizard = {
    fieldData: null,
    step1Data: null,
    step2Data: null,
    currentStep: 1,
    
    /**
     * Initialize wizard with field data
     * Called from the wizard partial after it's loaded
     */
    initialize: function(fieldData) {
        this.fieldData = fieldData;
        this.currentStep = 1;
        this.step1Data = null;
        this.step2Data = null;
        
        // Initialize Step 1 form with suggestions if no existing data
        this.initializeStep1();
        
        // Trigger initial metric option display
        this.handleMetricOptionChange($('#metricOptionChooser').val() || 'standalone');
        
        console.log('FieldMappingWizard initialized with field:', fieldData.itemName);
    },
    
    /**
     * Initialize Step 1 form with suggestions
     */
    initializeStep1: function() {
        // Pre-populate mapping name if empty
        const mappingNameInput = $('#mappingName');
        if (!mappingNameInput.val()) {
            const suggestedName = `${this.fieldData.itemName} Performance`;
            mappingNameInput.attr('placeholder', suggestedName);
        }
        
        // Update summary with current values
        $('#summaryMappingName').text($('#mappingName').val() || '-');
        $('#summaryMappingType').text($('#mappingType option:selected').text() || '-');
        $('#summaryAggregationType').text($('#aggregationType option:selected').text().split(' - ')[0] || '-');
    },
    
    /**
     * Initialize create new metric section with suggestions
     */
    initializeCreateNewSection: function() {
        // Generate metric name suggestions
        const suggestedName = `${this.fieldData.itemName} Performance Index`;
        $('#newMetricName').val(suggestedName);
        
        // Auto-generate metric code
        this.generateMetricCode(suggestedName);
        
        // Generate description
        const description = `Measures the performance and quality of ${this.fieldData.itemName.toLowerCase()} based on field responses.`;
        $('#newMetricDescription').val(description);
        
        // Setup real-time code generation
        $('#newMetricName').off('input').on('input', (e) => {
            this.generateMetricCode(e.target.value);
        });
        
        // Generate threshold suggestions if field has options
        if (this.fieldData.hasOptions && this.fieldData.options && this.fieldData.options.length > 0) {
            this.generateThresholdSuggestions();
        }
    },
    
    /**
     * Show selected metric details when linking
     */
    showSelectedMetricDetails: function(metricId) {
        const selectedOption = $(`#metricSelect option[value="${metricId}"]`);
        if (selectedOption.length) {
            $('#selectedMetricName').text(selectedOption.text());
            $('#selectedMetricCategory').text(selectedOption.data('category') || '-');
            $('#selectedMetricDescription').text(selectedOption.data('description') || '-');
            $('#selectedMetricDetails').removeClass('d-none');
        }
    },
    
    /**
     * Validate Step 1 form
     */
    validateStep1: function() {
        const mappingName = $('#mappingName').val()?.trim();
        const mappingType = $('#mappingType').val();
        const aggregationType = $('#aggregationType').val();
        
        let isValid = true;
        
        // Clear previous validation states
        $('#mappingName, #mappingType, #aggregationType').removeClass('is-invalid');
        
        if (!mappingName) {
            $('#mappingName').addClass('is-invalid').focus();
            isValid = false;
        }
        
        if (!mappingType) {
            $('#mappingType').addClass('is-invalid');
            if (isValid) $('#mappingType').focus();
            isValid = false;
        }
        
        if (!aggregationType) {
            $('#aggregationType').addClass('is-invalid');
            if (isValid) $('#aggregationType').focus();
            isValid = false;
        }
        
        if (!isValid) {
            // Show validation message
            console.log('Step 1 validation failed - please fill all required fields');
        }
        
        return isValid;
    },
    
    /**
     * Store Step 1 data in memory (no API call)
     */
    storeStep1Data: function() {
        this.step1Data = {
            itemId: this.fieldData.itemId,
            mappingName: $('#mappingName').val().trim(),
            mappingType: $('#mappingType').val(),
            aggregationType: $('#aggregationType').val(),
            expectedValue: $('#expectedValue').val()?.trim() || null
        };
        
        this.currentStep = 2;
        console.log('Step 1 data stored:', this.step1Data);
    },
    
    /**
     * Handle metric option change in Step 2
     * Uses server-rendered content sections instead of generating HTML
     */
    handleMetricOptionChange: function(selectedOption) {
        // Hide all option content sections
        $('.option-content').addClass('d-none');
        
        // Show selected content section
        $(`#${selectedOption}-content`).removeClass('d-none');
        
        // Initialize specific functionality based on selection
        switch(selectedOption) {
            case 'create-new':
                this.initializeCreateNewSection();
                break;
            case 'link-existing':
                // Metrics are already pre-loaded from server, just show details if selected
                const selectedMetric = $('#metricSelect').val();
                if (selectedMetric) {
                    this.showSelectedMetricDetails(selectedMetric);
                }
                break;
        }
    },
    
    // NOTE: showCreateNewContent and showLinkExistingContent removed - using server-rendered content
    
    /**
     * Initialize metric creation with suggestions (for create-new option)
     */
    initializeMetricCreation: function() {
        // Generate metric name suggestions
        const suggestedName = `${this.fieldData.itemName} Performance Index`;
        $('#newMetricName').val(suggestedName);
        
        // Auto-generate metric code
        this.generateMetricCode(suggestedName);
        
        // Generate description
        const description = `Measures the performance and quality of ${this.fieldData.itemName.toLowerCase()} based on field responses.`;
        $('#newMetricDescription').val(description);
        
        // Setup real-time code generation
        $('#newMetricName').off('input').on('input', (e) => {
            this.generateMetricCode(e.target.value);
        });
        
        // Generate threshold suggestions if field has options
        if (this.fieldData.hasOptions && this.fieldData.options.length > 0) {
            this.generateThresholdSuggestions();
        }
    },
    
    /**
     * Generate metric code with global context
     */
    generateMetricCode: function(metricName) {
        if (!metricName) return;
        
        const fieldConcept = metricName
            .replace(/[^a-zA-Z0-9\s]/g, '')
            .trim()
            .split(/\s+/)
            .map(word => word.toUpperCase())
            .join('_');
        
        const templateId = this.fieldData.templateId || 'UNKNOWN';
        const code = `${fieldConcept}_TEMPLATE_${templateId}`;
        
        $('#newMetricCode').val(code);
    },
    
    /**
     * Generate threshold suggestions based on field options
     */
    generateThresholdSuggestions: function() {
        const scores = this.fieldData.options.map(o => parseFloat(o.scoreValue) || 0).filter(s => s > 0);
        if (scores.length === 0) return;
        
        const maxScore = Math.max(...scores);
        
        // Generate smart thresholds
        const greenThreshold = Math.round(maxScore * 0.8);
        const yellowThreshold = Math.round(maxScore * 0.6);
        const redThreshold = Math.round(maxScore * 0.4);
        
        $('#newMetricGreen').val(greenThreshold);
        $('#newMetricYellow').val(yellowThreshold);
        $('#newMetricRed').val(redThreshold);
    },
    
    // NOTE: loadCompatibleMetrics and populateCompatibleMetrics removed - metrics are pre-loaded from server
    
    /**
     * Save Step 2 - Handle final save based on metric choice
     */
    saveStep2: async function() {
        const metricOption = $('#metricOptionChooser').val();
        
        try {
            let result;
            
            switch (metricOption) {
                case 'standalone':
                    result = await this.saveStandaloneChoice();
                    break;
                case 'create-new':
                    result = await this.saveCreateNewMetric();
                    break;
                case 'link-existing':
                    result = await this.saveLinkExistingMetric();
                    break;
                default:
                    throw new Error('Please select a metric option');
            }
            
            if (result.success) {
                showAlert('Field mapping saved successfully!', 'success');
                
                // Close modal and refresh
                const modal = bootstrap.Modal.getInstance(document.getElementById('fieldMappingModal'));
                if (modal) {
                    modal.hide();
                }
                
                // Refresh page to show updated mapping
                setTimeout(() => window.location.reload(), 1000);
            } else {
                showAlert(result.message || 'Failed to save mapping', 'danger');
            }
        } catch (error) {
            showAlert('Error: ' + error.message, 'danger');
        }
    },
    
    /**
     * Save standalone choice - save Step 1 data without metric link
     */
    saveStandaloneChoice: async function() {
        // Save the mapping with Step 1 data only (no metric)
        const mappingData = {
            itemId: this.fieldData.itemId,
            mappingName: this.step1Data.mappingName,
            mappingType: this.step1Data.mappingType,
            aggregationType: this.step1Data.aggregationType,
            expectedValue: this.step1Data.expectedValue,
            metricId: null // Standalone - no metric link
        };
        
        const response = await fetch('/api/metric-mapping/field/save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(mappingData)
        });
        
        return await response.json();
    },
    
    /**
     * Save create new metric and link
     */
    saveCreateNewMetric: async function() {
        const metricName = $('#newMetricName').val()?.trim();
        if (!metricName) {
            throw new Error('Metric name is required');
        }
        
        // Create MetricDefinition
        const metricData = {
            MetricName: metricName,
            MetricCode: $('#newMetricCode').val()?.trim(),
            Description: $('#newMetricDescription').val()?.trim(),
            DataType: $('#newMetricDataType').val() || 'Decimal',
            Unit: $('#newMetricUnit').val()?.trim() || 'Score',
            Category: $('#newMetricCategory').val() || 'Performance',
            ThresholdGreen: parseFloat($('#newMetricGreen').val()) || null,
            ThresholdYellow: parseFloat($('#newMetricYellow').val()) || null,
            ThresholdRed: parseFloat($('#newMetricRed').val()) || null,
            TemplateId: this.fieldData.templateId
        };
        
        const metricResponse = await fetch('/api/metric-mapping/create-metric', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(metricData)
        });
        
        const metricResult = await metricResponse.json();
        if (!metricResult.success) {
            throw new Error(metricResult.message || 'Failed to create metric');
        }
        
        // Update FormItemMapping with MetricId
        return await this.updateMappingWithMetric(metricResult.data.metricId);
    },
    
    /**
     * Save link to existing metric
     */
    saveLinkExistingMetric: async function() {
        const metricId = $('#metricSelect').val();
        if (!metricId) {
            throw new Error('Please select a metric to link to');
        }
        
        return await this.updateMappingWithMetric(parseInt(metricId));
    },
    
    /**
     * Update mapping with metric ID
     */
    updateMappingWithMetric: async function(metricId) {
        const updateData = {
            itemId: this.fieldData.itemId,
            metricId: metricId,
            mappingName: this.step1Data.mappingName,
            mappingType: this.step1Data.mappingType,
            aggregationType: this.step1Data.aggregationType,
            expectedValue: this.step1Data.expectedValue
        };
        
        const response = await fetch('/api/metric-mapping/field/save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updateData)
        });
        
        return await response.json();
    }
};

// ===================================================================
// EVENT HANDLERS
// ===================================================================

/**
 * Setup wizard event handlers
 */
function setupWizardEventHandlers() {
    // Step 1 to Step 2 navigation
    $('#nextToStep2').off('click').on('click', function() {
        FieldMappingWizard.saveStep1().then(() => {
            // Activate Step 2 tab
            const step2Tab = new bootstrap.Tab(document.getElementById('step2-tab'));
            step2Tab.show();
        }).catch(error => {
            showAlert('Error saving step 1: ' + error.message, 'danger');
        });
    });
    
    // Step 2 back to Step 1
    $('#backToStep1').off('click').on('click', function() {
        const step1Tab = new bootstrap.Tab(document.getElementById('step1-tab'));
        step1Tab.show();
    });
    
    // Metric option chooser change handler
    $('#metricOptionChooser').off('change').on('change', function() {
        const selectedOption = $(this).val();
        FieldMappingWizard.handleMetricOptionChange(selectedOption);
    });
    
    // Save button handler
    $('#saveFieldMapping').off('click').on('click', function() {
        FieldMappingWizard.saveStep2();
    });
    
    // Expected value container toggle
    $('#mappingType').off('change').on('change', function() {
        if ($(this).val() === 'BinaryCompliance') {
            $('#expectedValueContainer').removeClass('d-none');
        } else {
            $('#expectedValueContainer').addClass('d-none');
        }
    });
}

// ===================================================================
// UTILITY FUNCTIONS
// ===================================================================

/**
 * Show error message in content div
 */
function showError(contentDiv, message) {
    contentDiv.innerHTML = `
        <div class="alert alert-danger">
            <h6>Error Loading Wizard</h6>
            <p>${message}</p>
            <button class="btn btn-outline-danger btn-sm" onclick="window.location.reload()">
                <i class="ri-refresh-line me-1"></i>Retry
            </button>
        </div>
    `;
}

/**
 * Show alert message to user
 */
function showAlert(message, type = 'info') {
    // Create alert element
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Add to page
    document.body.appendChild(alertDiv);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 5000);
}
