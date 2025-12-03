/**
 * Metric Mapping - Form Fields to KPI Metrics
 * Handles the two-column interface for mapping form fields to metrics
 */

const MetricMapping = {
    // State
    templateId: null,
    templateName: null,
    currentFieldId: null,
    fieldsData: null,
    metricsData: null,
    
    /**
     * Initialize the metric mapping interface
     */
    init() {
        this.templateId = $('#templateId').val();
        this.templateName = $('#templateName').val();
        
        // Load initial data
        this.loadFieldTree();
        this.loadMetrics();
        
        // Attach event handlers
        this.attachEventHandlers();
        
        console.log('Metric Mapping initialized for template:', this.templateId);
    },
    
    /**
     * Attach all event handlers
     */
    attachEventHandlers() {
        const self = this;
        
        // Expand/Collapse all sections
        $('#btnExpandAll').on('click', () => this.expandAllSections());
        $('#btnCollapseAll').on('click', () => this.collapseAllSections());
        $('#btnRefreshFields').on('click', () => this.loadFieldTree());
        
        // Cancel configuration
        $('#btnCancelConfiguration').on('click', () => this.cancelConfiguration());
        
        // Save scoring configuration
        $('#btnSaveScoring').on('click', () => this.saveScoring());
        
        // Back to scoring tab
        $('#btnBackToScoring').on('click', () => this.showScoringTab());
        
        // Save mapping
        $('#btnSaveMapping').on('click', () => this.saveMapping());
        
        // Test mapping
        $('#btnTestMapping').on('click', () => this.testMapping());
        
        // Tab change handlers
        $('a[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
            const targetTab = $(e.target).attr('href');
            self.onTabChange(targetTab);
        });
        
        // Continue to next step
        $('#btnContinueToWorkflow').on('click', () => {
            window.location.href = `/Forms/FormTemplates/ApprovalWorkflow?id=${this.templateId}`;
        });
    },
    
    /**
     * Load field tree from API
     */
    loadFieldTree() {
        const self = this;
        
        $('#fieldsLoading').show();
        $('#fieldTreeContainer').hide();
        $('#fieldsEmptyState').hide();
        
        $.ajax({
            url: `/api/metric-mapping/template/${this.templateId}/fields`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    self.fieldsData = response.data;
                    self.renderFieldTree(response.data);
                    self.updateStatistics(response.data);
                } else {
                    toastr.error(response.message || 'Failed to load fields');
                    $('#fieldsEmptyState').show();
                }
                $('#fieldsLoading').hide();
            },
            error: function(xhr) {
                console.error('Error loading fields:', xhr);
                toastr.error('Failed to load form fields');
                $('#fieldsLoading').hide();
                $('#fieldsEmptyState').show();
            }
        });
    },
    
    /**
     * Render field tree in left panel
     */
    renderFieldTree(data) {
        const $container = $('#fieldTreeContainer');
        $container.empty();
        
        if (!data.sections || data.sections.length === 0) {
            $('#fieldsEmptyState').show();
            return;
        }
        
        // Render each section
        data.sections.forEach(section => {
            const sectionHtml = this.renderSection(section);
            $container.append(sectionHtml);
        });
        
        $container.show();
        
        // Attach section click handlers
        this.attachSectionHandlers();
        
        // Attach field click handlers
        this.attachFieldHandlers();
    },
    
    /**
     * Render a section with its fields
     */
    renderSection(section) {
        const progressPercent = section.totalFields > 0 
            ? Math.round((section.mappedCount / section.totalFields) * 100) 
            : 0;
        
        const progressClass = progressPercent === 100 ? 'success' : progressPercent > 0 ? 'warning' : 'secondary';
        
        let html = `
            <div class="list-group-item nested-section" data-section-id="${section.sectionId}">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <i class="ri-arrow-down-s-line section-icon me-2"></i>
                        <strong>${section.sectionName}</strong>
                        <span class="badge bg-${progressClass}-subtle text-${progressClass} ms-2">
                            ${section.mappedCount}/${section.totalFields} mapped
                        </span>
                    </div>
                </div>
            </div>
            <div class="section-fields show" data-section-id="${section.sectionId}">`;
        
        // Render fields in this section
        section.fields.forEach(field => {
            html += this.renderField(field);
        });
        
        html += '</div>';
        
        return html;
    },
    
    /**
     * Render a single field item
     */
    renderField(field) {
        const statusClass = field.isMapped ? 'mapped' : 'unmapped';
        const mappingInfo = field.isMapped 
            ? `<small class="text-muted ms-2">→ ${field.metricName}</small>` 
            : '<small class="text-warning ms-2">(Not mapped)</small>';
        
        const mappingTypeDisplay = field.mappingType ? this.getMappingTypeDisplay(field.mappingType) : '';
        
        return `
            <div class="list-group-item field-item ${statusClass}" data-field-id="${field.itemId}">
                <i class="${field.statusIcon} ${field.statusClass} status-icon"></i>
                <span class="field-name">${field.itemName}</span>
                <span class="field-datatype">(${field.dataType})</span>
                ${mappingInfo}
                ${mappingTypeDisplay}
            </div>`;
    },
    
    /**
     * Get mapping type display badge
     */
    getMappingTypeDisplay(mappingType) {
        const badges = {
            'Direct': '<span class="mapping-type-badge bg-success-subtle text-success">Direct</span>',
            'SystemCalculated': '<span class="mapping-type-badge bg-primary-subtle text-primary">Formula</span>',
            'BinaryCompliance': '<span class="mapping-type-badge bg-info-subtle text-info">Compliance</span>',
            'Derived': '<span class="mapping-type-badge bg-secondary-subtle text-secondary">Derived</span>'
        };
        return badges[mappingType] || '';
    },
    
    /**
     * Attach section toggle handlers
     */
    attachSectionHandlers() {
        const self = this;
        
        $('.nested-section').off('click').on('click', function() {
            const sectionId = $(this).data('section-id');
            const $fields = $(`.section-fields[data-section-id="${sectionId}"]`);
            
            // Toggle visibility
            $fields.toggleClass('show');
            
            // Toggle icon
            $(this).toggleClass('collapsed');
        });
    },
    
    /**
     * Attach field click handlers
     */
    attachFieldHandlers() {
        const self = this;
        
        $('.field-item').off('click').on('click', function() {
            const fieldId = $(this).data('field-id');
            self.loadFieldMapping(fieldId);
        });
    },
    
    /**
     * Load mapping configuration for a field
     */
    loadFieldMapping(fieldId) {
        const self = this;
        this.currentFieldId = fieldId;
        
        // Highlight selected field
        $('.field-item').removeClass('selected');
        $(`.field-item[data-field-id="${fieldId}"]`).addClass('selected');
        
        // Show loading state
        $('#noSelectionMessage').hide();
        $('#tabbedConfigContainer').hide();
        $('#mappingLoading').show();
        
        $.ajax({
            url: `/api/metric-mapping/field/${fieldId}`,
            type: 'GET',
            success: function(response) {
                $('#mappingLoading').hide();
                
                if (response.success) {
                    // Show tabbed interface
                    $('#tabbedConfigContainer').show();
                    
                    // Load Tab 1: Scoring Configuration
                    self.renderScoringTab(response);
                    
                    // Load Tab 2: Metric Mapping
                    self.renderMappingTab(response);
                    
                    // Start on scoring tab
                    $('#scoring-tab-link').tab('show');
                    $('#scoringActions').show();
                    $('#mappingActions').hide();
                } else {
                    toastr.error(response.message || 'Failed to load field configuration');
                    $('#noSelectionMessage').show();
                }
            },
            error: function(xhr) {
                console.error('Error loading field configuration:', xhr);
                toastr.error('Failed to load field configuration');
                $('#mappingLoading').hide();
                $('#noSelectionMessage').show();
            }
        });
    },
    
    /**
     * Render Tab 1: Scoring Configuration
     */
    renderScoringTab(data) {
        const field = data.field;
        const $container = $('#scoringConfigContent');
        
        let html = `
            <div class="info-card mb-3">
                <h6 class="mb-2">
                    <i class="ri-file-list-3-line me-2"></i>${field.itemName}
                </h6>
                <div class="row small">
                    <div class="col-6"><strong>Type:</strong> ${field.dataType}</div>
                    <div class="col-6"><strong>Required:</strong> ${field.isRequired ? 'Yes' : 'No'}</div>
                    <div class="col-12 mt-2"><strong>Section:</strong> ${field.sectionName}</div>
                </div>
            </div>`;
        
        // Different configuration based on field type
        if (field.hasOptions) {
            // Fields with options (Checkbox, Dropdown, Radio)
            html += `
                <h6 class="mb-3">Scoring Method</h6>
                <div class="mb-3">
                    <div class="form-check mb-2">
                        <input class="form-check-input" type="radio" name="scoringMethod" id="optionScoring" value="option" checked>
                        <label class="form-check-label" for="optionScoring">
                            <strong>Option Scores</strong> - Assign numeric values to each option
                        </label>
                    </div>
                    <div class="form-check">
                        <input class="form-check-input" type="radio" name="scoringMethod" id="noScoring" value="none">
                        <label class="form-check-label" for="noScoring">
                            <strong>No Scoring</strong> - Text only (cannot be used for numeric metrics)
                        </label>
                    </div>
                </div>
                
                <div id="optionScoresTable" class="mt-3">
                    <h6 class="mb-2">Option Scores</h6>
                    <small class="text-muted d-block mb-3">Assign numeric values (0-100) to each option for metric calculation</small>
                    <table class="table table-sm option-score-table">
                        <thead class="table-light">
                            <tr>
                                <th>Option</th>
                                <th style="width: 150px;">Score Value</th>
                            </tr>
                        </thead>
                        <tbody>`;
            
            field.options.forEach(option => {
                html += `
                    <tr>
                        <td>${option.optionLabel}</td>
                        <td>
                            <input type="number" class="form-control form-control-sm" 
                                   data-option-id="${option.optionId}"
                                   value="${option.scoreValue || ''}" 
                                   min="0" max="100" step="1"
                                   placeholder="0-100">
                        </td>
                    </tr>`;
            });
            
            html += `
                        </tbody>
                    </table>
                </div>`;
        } else if (field.dataType === 'Number' || field.dataType === 'Integer') {
            // Numeric fields
            html += `
                <h6 class="mb-3">Scoring Method</h6>
                <div class="mb-3">
                    <div class="form-check mb-2">
                        <input class="form-check-input" type="radio" name="scoringMethod" id="directValue" value="direct" checked>
                        <label class="form-check-label" for="directValue">
                            <strong>Use Direct Value</strong> - Use the entered number as-is
                        </label>
                    </div>
                </div>
                <div class="info-card">
                    <i class="ri-information-line me-2"></i>
                    <small>This field accepts numeric input which will be used directly for counting/summing metrics.</small>
                </div>`;
        } else {
            // Text fields
            html += `
                <div class="info-card warning">
                    <i class="ri-alert-line me-2"></i>
                    <h6>Text-Only Field</h6>
                    <p class="mb-0">This field type cannot produce numeric values. It can only be mapped to categorical/status metrics.</p>
                </div>`;
        }
        
        $container.html(html);
    },
    
    /**
     * Render Tab 2: Metric Mapping
     */
    renderMappingTab(data) {
        const field = data.field;
        const mapping = data.mapping;
        const $container = $('#metricMappingContent');
        
        if (data.isMapped) {
            // Show existing mapping
            $container.html(this.renderExistingMapping(mapping));
        } else {
            // Show new mapping form
            $container.html(this.renderNewMappingForm(field, data.availableMetrics));
        }
        
        // Attach form handlers
        this.attachMappingFormHandlers();
    },
    
    /**
     * Render mapping configuration form (LEGACY - kept for compatibility)
     */
    renderMappingForm(data) {
        const field = data.field;
        const mapping = data.mapping;
        const availableMetrics = data.availableMetrics;
        
        let html = `
            <div class="alert alert-light border">
                <h5 class="mb-3">
                    <i class="ri-file-list-3-line text-primary me-2"></i>
                    ${field.itemName}
                </h5>
                <div class="row">
                    <div class="col-6">
                        <small class="text-muted">Field Code:</small>
                        <div class="fw-semibold">${field.itemCode}</div>
                    </div>
                    <div class="col-6">
                        <small class="text-muted">Data Type:</small>
                        <div class="fw-semibold">${field.dataType}</div>
                    </div>
                    <div class="col-6 mt-2">
                        <small class="text-muted">Section:</small>
                        <div class="fw-semibold">${field.sectionName}</div>
                    </div>
                    <div class="col-6 mt-2">
                        <small class="text-muted">Required:</small>
                        <div class="fw-semibold">${field.isRequired ? 'Yes' : 'No'}</div>
                    </div>
                </div>
            </div>`;
        
        if (data.isMapped) {
            // Show existing mapping
            html += this.renderExistingMapping(mapping);
        } else {
            // Show new mapping form
            html += this.renderNewMappingForm(field, availableMetrics);
        }
        
        $('#mappingFormContainer').html(html);
        
        // Attach form handlers
        this.attachMappingFormHandlers();
    },
    
    /**
     * Render existing mapping details
     */
    renderExistingMapping(mapping) {
        return `
            <div class="card border-success mt-3">
                <div class="card-header bg-success-subtle">
                    <h6 class="mb-0 text-success">
                        <i class="ri-check-line me-2"></i>
                        Currently Mapped
                    </h6>
                </div>
                <div class="card-body">
                    <div class="row mb-3">
                        <div class="col-6">
                            <small class="text-muted">Metric:</small>
                            <div class="fw-semibold">${mapping.metricName}</div>
                            <small class="text-muted">${mapping.metricCode}</small>
                        </div>
                        <div class="col-6">
                            <small class="text-muted">Mapping Type:</small>
                            <div class="fw-semibold">${this.getMappingTypeLabel(mapping.mappingType)}</div>
                        </div>
                    </div>
                    
                    ${mapping.transformationLogic ? `
                        <div class="mb-3">
                            <small class="text-muted">Transformation Logic:</small>
                            <pre class="bg-light p-2 rounded"><code>${mapping.transformationSummary || 'N/A'}</code></pre>
                        </div>
                    ` : ''}
                    
                    ${mapping.expectedValue ? `
                        <div class="mb-3">
                            <small class="text-muted">Expected Value:</small>
                            <div class="fw-semibold">${mapping.expectedValue}</div>
                        </div>
                    ` : ''}
                    
                    <div class="alert alert-warning mt-3">
                        <i class="ri-information-line me-2"></i>
                        <small>To change this mapping, please delete it first and create a new one.</small>
                    </div>
                    
                    <button type="button" class="btn btn-danger w-100" id="btnDeleteMapping" data-mapping-id="${mapping.mappingId}">
                        <i class="ri-delete-bin-line me-2"></i>Delete Mapping
                    </button>
                </div>
            </div>`;
    },
    
    /**
     * Render new mapping form
     */
    renderNewMappingForm(field, availableMetrics) {
        return `
            <div class="card mt-3">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="ri-add-line me-2"></i>
                        Create New Mapping
                    </h6>
                </div>
                <div class="card-body">
                    <input type="hidden" id="mappingItemId" value="${field.itemId}">
                    
                    <!-- Mapping Type Selection -->
                    <div class="mb-3">
                        <label class="form-label">Mapping Type <span class="text-danger">*</span></label>
                        <select class="form-select" id="mappingType" required>
                            <option value="">Select mapping type...</option>
                            <option value="Direct">Direct - Copy field value as-is</option>
                            <option value="SystemCalculated">Calculated - Use formula</option>
                            <option value="BinaryCompliance">Binary Compliance - Yes/No check</option>
                        </select>
                        <small class="form-text text-muted">Choose how this field's value should be transformed into a metric.</small>
                    </div>
                    
                    <!-- Metric Selection -->
                    <div class="mb-3">
                        <label class="form-label">Select Metric <span class="text-danger">*</span></label>
                        <select class="form-select" id="mappingMetricId" required>
                            <option value="">Select a metric...</option>
                            ${this.renderMetricOptions(availableMetrics)}
                        </select>
                        <small class="form-text text-muted">Choose the KPI metric to populate with this field's data.</small>
                    </div>
                    
                    <!-- Conditional Fields (shown based on mapping type) -->
                    <div id="binaryComplianceFields" style="display: none;">
                        <div class="mb-3">
                            <label class="form-label">Expected Value <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="expectedValue" placeholder="e.g., Yes, TRUE, 100%">
                            <small class="form-text text-muted">Value that represents 100% compliance (other values = 0%).</small>
                        </div>
                    </div>
                    
                    <div id="calculatedFields" style="display: none;">
                        <div class="alert alert-info">
                            <i class="ri-information-line me-2"></i>
                            <strong>Formula Builder</strong>
                            <p class="mb-0 mt-2">Advanced formula configuration will be available after saving this mapping.</p>
                        </div>
                    </div>
                </div>
            </div>`;
    },
    
    /**
     * Render metric options for dropdown
     */
    renderMetricOptions(metrics) {
        if (!metrics || metrics.length === 0) {
            return '<option value="" disabled>No compatible metrics available</option>';
        }
        
        let html = '';
        let currentCategory = '';
        
        metrics.forEach(metric => {
            // Add category optgroup
            if (metric.metricCategory !== currentCategory) {
                if (currentCategory !== '') {
                    html += '</optgroup>';
                }
                currentCategory = metric.metricCategory || 'Uncategorized';
                html += `<optgroup label="${currentCategory}">`;
            }
            
            html += `<option value="${metric.metricId}">${metric.metricName} (${metric.metricCode})</option>`;
        });
        
        if (currentCategory !== '') {
            html += '</optgroup>';
        }
        
        return html;
    },
    
    /**
     * Attach mapping form event handlers
     */
    attachMappingFormHandlers() {
        const self = this;
        
        // Mapping type change
        $('#mappingType').on('change', function() {
            const mappingType = $(this).val();
            
            // Show/hide conditional fields
            $('#binaryComplianceFields').toggle(mappingType === 'BinaryCompliance');
            $('#calculatedFields').toggle(mappingType === 'SystemCalculated');
        });
        
        // Delete mapping
        $('#btnDeleteMapping').on('click', function() {
            const mappingId = $(this).data('mapping-id');
            self.deleteMapping(mappingId);
        });
    },
    
    /**
     * Save mapping
     */
    saveMapping() {
        const self = this;
        
        // Validate form
        const itemId = $('#mappingItemId').val();
        const mappingType = $('#mappingType').val();
        const metricId = $('#mappingMetricId').val();
        
        if (!mappingType) {
            toastr.error('Please select a mapping type');
            return;
        }
        
        if (!metricId) {
            toastr.error('Please select a metric');
            return;
        }
        
        // Build DTO
        const dto = {
            itemId: parseInt(itemId),
            metricId: parseInt(metricId),
            mappingType: mappingType
        };
        
        // Add conditional fields
        if (mappingType === 'BinaryCompliance') {
            dto.expectedValue = $('#expectedValue').val();
            if (!dto.expectedValue) {
                toastr.error('Expected value is required for Binary Compliance mapping');
                return;
            }
        }
        
        // Show loading
        $('#btnSaveMapping').prop('disabled', true).html('<i class="ri-loader-4-line spinner-border spinner-border-sm me-1"></i>Saving...');
        
        $.ajax({
            url: '/api/metric-mapping/create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dto),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    toastr.success('Mapping created successfully');
                    
                    // Refresh field tree
                    self.loadFieldTree();
                    
                    // Reload current field
                    setTimeout(() => {
                        self.loadFieldMapping(self.currentFieldId);
                    }, 500);
                } else {
                    toastr.error(response.message || 'Failed to create mapping');
                    if (response.errors && response.errors.length > 0) {
                        response.errors.forEach(error => toastr.error(error));
                    }
                }
            },
            error: function(xhr) {
                console.error('Error saving mapping:', xhr);
                toastr.error('Failed to save mapping');
            },
            complete: function() {
                $('#btnSaveMapping').prop('disabled', false).html('<i class="ri-save-line me-1"></i>Save Mapping');
            }
        });
    },
    
    /**
     * Delete mapping
     */
    deleteMapping(mappingId) {
        const self = this;
        
        if (!confirm('Are you sure you want to delete this mapping? This action cannot be undone.')) {
            return;
        }
        
        $.ajax({
            url: `/api/metric-mapping/delete/${mappingId}`,
            type: 'DELETE',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    toastr.success('Mapping deleted successfully');
                    
                    // Refresh field tree
                    self.loadFieldTree();
                    
                    // Reload current field
                    setTimeout(() => {
                        self.loadFieldMapping(self.currentFieldId);
                    }, 500);
                } else {
                    toastr.error(response.message || 'Failed to delete mapping');
                }
            },
            error: function(xhr) {
                console.error('Error deleting mapping:', xhr);
                toastr.error('Failed to delete mapping');
            }
        });
    },
    
    /**
     * Cancel mapping configuration
     */
    cancelMapping() {
        // Clear selection
        $('.field-item').removeClass('selected');
        this.currentFieldId = null;
        
        // Hide form, show default message
        $('#mappingFormContainer').hide();
        $('#mappingActions').hide();
        $('#noSelectionMessage').show();
    },
    
    /**
     * Test mapping (placeholder for future implementation)
     */
    testMapping() {
        toastr.info('Mapping test functionality coming soon');
    },
    
    /**
     * Load all metrics for dropdown
     */
    loadMetrics() {
        const self = this;
        
        $.ajax({
            url: '/api/metric-mapping/metrics',
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    self.metricsData = response.data;
                }
            },
            error: function(xhr) {
                console.error('Error loading metrics:', xhr);
            }
        });
    },
    
    /**
     * Update statistics card
     */
    updateStatistics(data) {
        $('#statTotalFields').text(data.totalFields || 0);
        $('#statMappedFields').text(data.mappedFields || 0);
        $('#statUnmappedFields').text((data.totalFields - data.mappedFields) || 0);
        
        const progressPercent = data.totalFields > 0 
            ? Math.round((data.mappedFields / data.totalFields) * 100) 
            : 0;
        
        $('#mappingProgress')
            .css('width', progressPercent + '%')
            .attr('aria-valuenow', progressPercent)
            .text(progressPercent + '%');
    },
    
    /**
     * Expand all sections
     */
    expandAllSections() {
        $('.section-fields').addClass('show');
        $('.nested-section').removeClass('collapsed');
    },
    
    /**
     * Collapse all sections
     */
    collapseAllSections() {
        $('.section-fields').removeClass('show');
        $('.nested-section').addClass('collapsed');
    },
    
    /**
     * Get mapping type display label
     */
    getMappingTypeLabel(type) {
        const labels = {
            'Direct': 'Direct (1:1)',
            'SystemCalculated': 'Calculated (Formula)',
            'BinaryCompliance': 'Binary Compliance (Yes/No)',
            'Derived': 'Derived (Complex)'
        };
        return labels[type] || type;
    },
    
    /**
     * Handle tab changes
     */
    onTabChange(tabId) {
        // Show/hide appropriate action buttons
        if (tabId === '#scoring-tab') {
            $('#scoringActions').show();
            $('#mappingActions').hide();
        } else if (tabId === '#mapping-tab') {
            $('#scoringActions').hide();
            $('#mappingActions').show();
        }
    },
    
    /**
     * Show scoring tab
     */
    showScoringTab() {
        $('#scoring-tab-link').tab('show');
    },
    
    /**
     * Show mapping tab
     */
    showMappingTab() {
        $('#mapping-tab-link').tab('show');
        $('#mapping-tab-badge').show();
    },
    
    /**
     * Save scoring configuration
     */
    saveScoring() {
        // Placeholder - will be implemented with specific field type logic
        toastr.success('Scoring configuration saved');
        
        // Enable mapping tab
        $('#mapping-tab-link').removeClass('disabled');
        $('#mapping-tab-badge').show();
        
        // Auto-switch to mapping tab
        this.showMappingTab();
    },
    
    /**
     * Cancel configuration
     */
    cancelConfiguration() {
        // Clear selection
        $('.field-item').removeClass('selected');
        this.currentFieldId = null;
        
        // Hide tabbed container, show default message
        $('#tabbedConfigContainer').hide();
        $('#noSelectionMessage').show();
        
        // Reset tabs
        $('#scoring-tab-link').tab('show');
        $('#mapping-tab-badge').hide();
    }
};

// Initialize on document ready
$(document).ready(function() {
    if ($('#templateId').length) {
        MetricMapping.init();
    }
});
