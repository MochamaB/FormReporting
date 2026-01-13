/**
 * Form Statistics Dashboard - Progressive AJAX Loading
 * Handles dynamic loading of charts and tables via AJAX
 * Uses ApexCharts for data visualization
 */
const FormStatisticsDashboard = {
    config: {
        templateId: null,
        startDate: null,
        endDate: null,
        tenantId: null,
        groupBy: 'Daily'
    },

    /**
     * Initialize dashboard with progressive loading
     */
    init: function(options) {
        this.config = { ...this.config, ...options };
        
        // Initialize filter values from URL params or options
        this.initializeFilterValues();
        
        // Setup filter form handlers
        this.setupFilterHandlers();
        
        // Auto-load AJAX sections based on data attributes
        this.loadAjaxSections();
    },
    
    /**
     * Initialize filter values from URL params or options
     */
    initializeFilterValues: function() {
        // Get URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        
        // Update config with URL params or options
        this.config.regionId = urlParams.get('regionId') || this.config.regionId;
        this.config.tenantId = urlParams.get('tenantId') || this.config.tenantId;
        this.config.status = urlParams.get('status') || this.config.status;
        this.config.startDate = urlParams.get('startDate') || this.config.startDate;
        this.config.endDate = urlParams.get('endDate') || this.config.endDate;
        this.config.groupBy = urlParams.get('groupBy') || this.config.groupBy;
        
        // Update dropdown buttons to show initial values
        this.updateDropdownButtons();
    },
    
    /**
     * Update dropdown button appearance based on current filter values
     */
    updateDropdownButtons: function() {
        const filters = ['regionId', 'tenantId', 'status'];
        
        filters.forEach(filterName => {
            const button = document.querySelector(`[data-filter-name="${filterName}"]`);
            if (button) {
                const filterValue = this.config[filterName];
                const isActive = filterValue && filterValue !== '';
                
                // Update button class
                button.className = `${isActive ? 'btn btn-primary' : 'btn btn-outline-secondary'} dropdown-toggle`;
                
                // Update button text
                const dropdown = button.closest('.dropdown');
                if (dropdown) {
                    const optionText = isActive 
                        ? dropdown.querySelector(`[data-value="${filterValue}"]`)?.textContent?.trim()
                        : filterName.charAt(0).toUpperCase() + filterName.slice(1).replace('Id', '');
                    button.innerHTML = `<i class="ri-filter-line me-1"></i>${optionText || filterName}`;
                    
                    // Update active state in dropdown menu
                    dropdown.querySelectorAll('.dropdown-item').forEach(item => {
                        const itemValue = item.getAttribute('data-value');
                        const itemActive = itemValue === filterValue;
                        item.classList.toggle('active', itemActive);
                        
                        // Update check icon
                        const icon = item.querySelector('i');
                        if (itemActive && !icon) {
                            item.innerHTML = `<i class="ri-check-line me-1"></i>${item.textContent.trim()}`;
                        } else if (!itemActive && icon) {
                            icon.remove();
                        }
                    });
                }
                
                // Create/update hidden input
                const form = document.querySelector('#dashboard-filters form');
                if (form) {
                    let hiddenInput = form.querySelector(`input[name="${filterName}"]`);
                    if (!hiddenInput) {
                        hiddenInput = document.createElement('input');
                        hiddenInput.type = 'hidden';
                        hiddenInput.name = filterName;
                        form.appendChild(hiddenInput);
                    }
                    hiddenInput.value = filterValue || '';
                }
            }
        });
    },
    
    /**
     * Load all AJAX sections automatically based on data attributes
     */
    loadAjaxSections: function() {
        const self = this;
        
        // Find all AJAX sections in the dashboard
        $('.dashboard-section-ajax').each(function() {
            const $section = $(this);
            const sectionId = $section.data('section-id');
            const ajaxUrl = $section.data('ajax-url');
            const componentType = $section.data('component-type');
            
            if (ajaxUrl) {
                self.loadAjaxSection($section, sectionId, ajaxUrl, componentType);
            }
        });
    },
    
    /**
     * Load a single AJAX section
     */
    loadAjaxSection: function($section, sectionId, ajaxUrl, componentType) {
        const self = this;
        const data = this.buildRequestData();
        
        $.ajax({
            url: ajaxUrl,
            type: 'GET',
            data: data,
            success: function(response) {
                // Replace skeleton with actual content
                if (componentType === 'Chart') {
                    // For charts, response is JSON config
                    self.renderChartSection($section, response);
                } else if (componentType === 'Table') {
                    // For tables, response is HTML
                    $section.html(response);
                } else {
                    // Generic HTML response
                    $section.html(response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading section ' + sectionId + ':', error);
                $section.html('<div class="alert alert-danger">Failed to load section</div>');
            }
        });
    },
    
    /**
     * Render chart section from JSON config
     */
    renderChartSection: function($section, chartConfig) {
        // Create chart container
        const chartHtml = '<div class="card"><div class="card-header"><h5 class="card-title mb-0">' + 
            chartConfig.title + '</h5></div><div class="card-body">' +
            '<div id="' + chartConfig.id + '"></div></div></div>';
        
        $section.html(chartHtml);
        
        // Render chart based on type
        if (chartConfig.chartType === 'line' || chartConfig.chartType === 'area') {
            this.renderLineChart(chartConfig);
        } else if (chartConfig.chartType === 'pie' || chartConfig.chartType === 'donut') {
            this.renderPieChart(chartConfig);
        }
    },

    /**
     * Setup filter form event handlers
     */
    setupFilterHandlers: function() {
        const self = this;
        
        // Handle filter form submission (FilterPanelConfig generates form inside the panel)
        $(document).on('submit', '#dashboard-filters form', function(e) {
            e.preventDefault();
            self.applyFilters();
        });
        
        // Handle reset/clear button (FilterPanelConfig uses type="reset")
        $(document).on('click', '#dashboard-filters button[type="reset"]', function(e) {
            e.preventDefault();
            self.resetFilters();
        });
    },

    /**
     * Apply filters and refresh dashboard
     */
    applyFilters: function(filterString) {
        // If filterString is provided, parse it (for main filter changes)
        if (filterString) {
            const params = new URLSearchParams(filterString);
            
            // Update config with main filter values only
            this.config.regionId = params.get('regionId') || this.config.regionId;
            this.config.tenantId = params.get('tenantId') || this.config.tenantId;
            this.config.status = params.get('status') || this.config.status;
        } else {
            // Get form data from the FilterPanelConfig form (for advanced filter changes)
            const form = document.querySelector('#dashboard-filters form');
            const formData = new FormData(form);
            
            // Update config with all filter values
            this.config.templateId = formData.get('templateId') || this.config.templateId;
            this.config.startDate = formData.get('startDate') || null;
            this.config.endDate = formData.get('endDate') || null;
            this.config.tenantId = formData.get('tenantId') || null;
            this.config.regionId = formData.get('regionId') || null;
            this.config.submitterId = formData.get('submitterId') || null;
            this.config.status = formData.get('status') || null;
            this.config.groupBy = formData.get('groupBy') || 'Daily';
        }
        
        // Show loading states
        this.showLoadingStates();
        
        // Refresh all dashboard sections
        this.refreshDashboard();
    },

    /**
     * Reset filters to default values
     */
    resetFilters: function() {
        // Reset form (FilterPanelConfig form)
        const form = document.querySelector('#dashboard-filters form');
        if (form) {
            form.reset();
        }
        
        // Reset config to defaults (keep templateId)
        const templateId = this.config.templateId;
        this.config = {
            templateId: templateId,
            startDate: null,
            endDate: null,
            tenantId: null,
            regionId: null,
            submitterId: null,
            status: null,
            groupBy: 'Daily'
        };
        
        // Refresh dashboard
        this.showLoadingStates();
        this.refreshDashboard();
    },

    /**
     * Show loading states for all sections
     */
    showLoadingStates: function() {
        $('#quick-stats-section').html('<div class="text-center p-4"><div class="spinner-border" role="status"></div></div>');
        $('#trend-chart-section').html('<div class="text-center p-4"><div class="spinner-border" role="status"></div></div>');
        $('#status-chart-section').html('<div class="text-center p-4"><div class="spinner-border" role="status"></div></div>');
        $('#recent-submissions-section').html('<div class="text-center p-4"><div class="spinner-border" role="status"></div></div>');
        
        if ($('#tenant-comparison-section').length > 0) {
            $('#tenant-comparison-section').html('<div class="text-center p-4"><div class="spinner-border" role="status"></div></div>');
        }
    },

    /**
     * Refresh all dashboard sections with current filters
     */
    refreshDashboard: function() {
        // Refresh quick stats
        this.loadQuickStats();
        
        // Refresh charts
        this.loadTrendChart();
        this.loadStatusChart();
        
        // Refresh tables
        this.loadRecentSubmissions();
        
        if ($('#tenant-comparison-section').length > 0) {
            this.loadTenantComparison();
        }
    },

    /**
     * Load quick stats with current filters
     */
    loadQuickStats: function() {
        const data = this.buildRequestData();
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetQuickStats',
            type: 'GET',
            data: data,
            success: function(html) {
                $('#quick-stats-section').html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading quick stats:', error);
                $('#quick-stats-section').html('<div class="alert alert-danger">Failed to load statistics</div>');
            }
        });
    },

    /**
     * Build request data object with current filters
     */
    buildRequestData: function() {
        const data = { templateId: this.config.templateId };
        
        if (this.config.startDate) data.startDate = this.config.startDate;
        if (this.config.endDate) data.endDate = this.config.endDate;
        if (this.config.tenantId) data.tenantId = this.config.tenantId;
        if (this.config.regionId) data.regionId = this.config.regionId;
        if (this.config.submitterId) data.submitterId = this.config.submitterId;
        if (this.config.status) data.status = this.config.status;
        if (this.config.groupBy) data.groupBy = this.config.groupBy;
        
        return data;
    },

    /**
     * Initialize widget (embedded version with custom container IDs)
     */
    initWidget: function(options) {
        // Merge config with defaults
        this.config = { 
            templateId: options.templateId,
            startDate: options.startDate || null,
            endDate: options.endDate || null,
            tenantId: options.tenantId || null,
            groupBy: options.groupBy || 'Daily',
            ...options 
        };
        
        const containerId = options.containerId || 'form-statistics-widget';
        
        console.log('Initializing FormStatistics widget:', this.config);
        
        // Use unified AJAX section loading
        this.loadAjaxSections();
        
        if (options.showTenantComparison) {
            this.loadWidgetTenantComparison(containerId);
        }
    },

    /**
     * Load quick stats for widget
     */
    loadWidgetQuickStats: function(containerId) {
        const $container = $('#' + containerId + '-quick-stats');
        
        // Build data object, only include non-empty values
        const data = { templateId: this.config.templateId };
        if (this.config.startDate) data.startDate = this.config.startDate;
        if (this.config.endDate) data.endDate = this.config.endDate;
        if (this.config.tenantId) data.tenantId = this.config.tenantId;
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetQuickStats',
            type: 'GET',
            data: data,
            success: function(html) {
                $container.html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading quick stats:', error);
                console.error('Response:', xhr.responseText);
                $container.html('<div class="alert alert-danger">Failed to load statistics</div>');
            }
        });
    },

    /**
     * Load trend chart for widget
     */
    loadWidgetTrendChart: function(containerId) {
        const $container = $('#' + containerId + '-trend-chart');
        
        const data = { 
            templateId: this.config.templateId,
            groupBy: this.config.groupBy || 'Daily'
        };
        if (this.config.startDate) data.startDate = this.config.startDate;
        if (this.config.endDate) data.endDate = this.config.endDate;
        if (this.config.tenantId) data.tenantId = this.config.tenantId;
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetTrendChart',
            type: 'GET',
            data: data,
            success: function(chartConfig) {
                const chartElementId = containerId + '-trend-chart-canvas';
                $container.html('<div class="card"><div class="card-header"><h5 class="card-title mb-0">' + 
                    chartConfig.title + '</h5></div><div class="card-body">' +
                    '<div id="' + chartElementId + '"></div></div></div>');
                
                // Set the chart ID for rendering
                chartConfig.id = chartElementId;
                FormStatisticsDashboard.renderLineChart(chartConfig);
            },
            error: function(xhr, status, error) {
                console.error('Error loading trend chart:', error);
                $container.html('<div class="alert alert-danger">Failed to load chart</div>');
            }
        });
    },

    /**
     * Load status chart for widget
     */
    loadWidgetStatusChart: function(containerId) {
        const $container = $('#' + containerId + '-status-chart');
        
        const data = { templateId: this.config.templateId };
        if (this.config.startDate) data.startDate = this.config.startDate;
        if (this.config.endDate) data.endDate = this.config.endDate;
        if (this.config.tenantId) data.tenantId = this.config.tenantId;
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetStatusChart',
            type: 'GET',
            data: data,
            success: function(chartConfig) {
                const chartElementId = containerId + '-status-chart-canvas';
                $container.html('<div class="card"><div class="card-header"><h5 class="card-title mb-0">' + 
                    chartConfig.title + '</h5></div><div class="card-body">' +
                    '<div id="' + chartElementId + '"></div></div></div>');
                
                // Set the chart ID for rendering
                chartConfig.id = chartElementId;
                FormStatisticsDashboard.renderPieChart(chartConfig);
            },
            error: function(xhr, status, error) {
                console.error('Error loading status chart:', error);
                $container.html('<div class="alert alert-danger">Failed to load chart</div>');
            }
        });
    },

    /**
     * Load recent submissions for widget
     */
    loadWidgetRecentSubmissions: function(containerId) {
        const $container = $('#' + containerId + '-recent-submissions');
        
        const data = { 
            templateId: this.config.templateId,
            count: 10
        };
        if (this.config.tenantId) data.tenantId = this.config.tenantId;
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetRecentSubmissions',
            type: 'GET',
            data: data,
            success: function(html) {
                $container.html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading recent submissions:', error);
                $container.html('<div class="alert alert-danger">Failed to load submissions</div>');
            }
        });
    },

    /**
     * Load tenant comparison for widget
     */
    loadWidgetTenantComparison: function(containerId) {
        const $container = $('#' + containerId + '-tenant-comparison');
        
        const data = { templateId: this.config.templateId };
        if (this.config.startDate) data.startDate = this.config.startDate;
        if (this.config.endDate) data.endDate = this.config.endDate;
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetTenantComparison',
            type: 'GET',
            data: data,
            success: function(html) {
                $container.html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading tenant comparison:', error);
                $container.html('<div class="alert alert-danger">Failed to load comparison</div>');
            }
        });
    },

    /**
     * Load trend chart via AJAX
     */
    loadTrendChart: function() {
        const $container = $('#trend-chart-section');
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetTrendChart',
            type: 'GET',
            data: this.buildRequestData(),
            success: function(chartConfig) {
                // Replace skeleton with chart container
                $container.html('<div class="card"><div class="card-header"><h5 class="card-title mb-0">' + 
                    chartConfig.title + '</h5></div><div class="card-body">' +
                    '<div id="' + chartConfig.id + '"></div></div></div>');
                
                // Render ApexChart
                FormStatisticsDashboard.renderLineChart(chartConfig);
            },
            error: function(xhr, status, error) {
                console.error('Error loading trend chart:', error);
                $container.html('<div class="alert alert-danger">Failed to load chart</div>');
            }
        });
    },

    /**
     * Load status chart via AJAX
     */
    loadStatusChart: function() {
        const $container = $('#status-chart-section');
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetStatusChart',
            type: 'GET',
            data: this.buildRequestData(),
            success: function(chartConfig) {
                // Replace skeleton with chart container
                $container.html('<div class="card"><div class="card-header"><h5 class="card-title mb-0">' + 
                    chartConfig.title + '</h5></div><div class="card-body">' +
                    '<div id="' + chartConfig.id + '"></div></div></div>');
                
                // Render ApexChart
                FormStatisticsDashboard.renderPieChart(chartConfig);
            },
            error: function(xhr, status, error) {
                console.error('Error loading status chart:', error);
                $container.html('<div class="alert alert-danger">Failed to load chart</div>');
            }
        });
    },

    /**
     * Load recent submissions table via AJAX
     */
    loadRecentSubmissions: function() {
        const $container = $('#recent-submissions-section');
        
        const data = this.buildRequestData();
        data.count = 10; // Add count parameter for recent submissions
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetRecentSubmissions',
            type: 'GET',
            data: data,
            success: function(html) {
                $container.html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading recent submissions:', error);
                $container.html('<div class="alert alert-danger">Failed to load submissions</div>');
            }
        });
    },

    /**
     * Load tenant comparison table via AJAX
     */
    loadTenantComparison: function() {
        const $container = $('#tenant-comparison-section');
        
        $.ajax({
            url: '/Dashboard/FormStatistics/GetTenantComparison',
            type: 'GET',
            data: this.buildRequestData(),
            success: function(html) {
                $container.html(html);
            },
            error: function(xhr, status, error) {
                console.error('Error loading tenant comparison:', error);
                $container.html('<div class="alert alert-danger">Failed to load comparison</div>');
            }
        });
    },

    /**
     * Render line chart using ApexCharts
     */
    renderLineChart: function(chartConfig) {
        // Parse JSON strings from ChartCardConfig
        const series = JSON.parse(chartConfig.seriesJson);
        const categories = JSON.parse(chartConfig.categoriesJson);
        
        const options = {
            chart: {
                type: 'line',
                height: chartConfig.height,
                toolbar: {
                    show: chartConfig.showToolbar
                }
            },
            series: series,
            xaxis: {
                categories: categories
            },
            colors: chartConfig.colors,
            stroke: {
                curve: 'smooth',
                width: 2
            },
            legend: {
                show: true
            }
        };

        const chart = new ApexCharts(document.querySelector('#' + chartConfig.id), options);
        chart.render();
    },

    /**
     * Render pie chart using ApexCharts
     */
    renderPieChart: function(chartConfig) {
        // Parse JSON strings from ChartCardConfig
        const series = JSON.parse(chartConfig.seriesJson);
        const labels = JSON.parse(chartConfig.categoriesJson);
        
        const options = {
            chart: {
                type: 'pie',
                height: chartConfig.height
            },
            series: series,
            labels: labels,
            colors: chartConfig.colors,
            legend: {
                show: true,
                position: 'bottom'
            },
            responsive: [{
                breakpoint: 480,
                options: {
                    chart: {
                        width: 200
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }]
        };

        const chart = new ApexCharts(document.querySelector('#' + chartConfig.id), options);
        chart.render();
    }
};
