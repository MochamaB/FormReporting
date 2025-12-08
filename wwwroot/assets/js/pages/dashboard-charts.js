/**
 * Dashboard Charts - ApexCharts initialization
 * Handles chart rendering from data attributes
 */

class DashboardCharts {
    constructor() {
        this.charts = new Map();
        this.defaultColors = ['#405189', '#0ab39c', '#f06548', '#f7b84b', '#299cdb'];
    }

    /**
     * Initialize all charts on the page
     */
    initializeAll() {
        const chartContainers = document.querySelectorAll('[id^="chart-"]');
        chartContainers.forEach(container => {
            this.initializeChart(container);
        });
    }

    /**
     * Initialize a single chart from data attributes
     * @param {HTMLElement} container - Chart container element
     */
    initializeChart(container) {
        const chartId = container.id;

        // Skip if already initialized
        if (this.charts.has(chartId)) {
            console.warn(`Chart ${chartId} already initialized`);
            return;
        }

        try {
            // Read configuration from data attributes
            const config = this.readChartConfig(container);

            // Build ApexCharts options
            const options = this.buildChartOptions(config);

            // Create and render chart
            const chart = new ApexCharts(container, options);
            chart.render();

            // Store chart instance
            this.charts.set(chartId, chart);

            console.log(`Chart ${chartId} initialized successfully`);
        } catch (error) {
            console.error(`Error initializing chart ${chartId}:`, error);
            this.showChartError(container, error.message);
        }
    }

    /**
     * Read chart configuration from data attributes
     * @param {HTMLElement} container - Chart container element
     * @returns {Object} Chart configuration
     */
    readChartConfig(container) {
        return {
            type: container.dataset.chartType || 'area',
            height: parseInt(container.dataset.chartHeight) || 350,
            series: JSON.parse(container.dataset.chartSeries || '[]'),
            categories: JSON.parse(container.dataset.chartCategories || '[]'),
            colors: JSON.parse(container.dataset.chartColors || '[]'),
            showToolbar: container.dataset.chartToolbar === 'true',
            customOptions: container.dataset.chartOptions
                ? JSON.parse(container.dataset.chartOptions)
                : null
        };
    }

    /**
     * Build ApexCharts options from configuration
     * @param {Object} config - Chart configuration
     * @returns {Object} ApexCharts options
     */
    buildChartOptions(config) {
        // Start with base options
        let options = {
            chart: {
                type: config.type,
                height: config.height,
                toolbar: {
                    show: config.showToolbar
                },
                fontFamily: 'Poppins, sans-serif'
            },
            series: config.series,
            colors: config.colors.length > 0 ? config.colors : this.defaultColors,
            dataLabels: {
                enabled: false
            },
            stroke: {
                curve: 'smooth',
                width: 2
            },
            grid: {
                borderColor: '#f1f1f1',
                padding: {
                    top: 0,
                    right: 10,
                    bottom: 0,
                    left: 10
                }
            },
            legend: {
                position: 'top',
                horizontalAlign: 'right'
            }
        };

        // Add categories for x-axis if provided
        if (config.categories.length > 0) {
            options.xaxis = {
                categories: config.categories,
                axisBorder: {
                    show: false
                },
                axisTicks: {
                    show: false
                }
            };
        }

        // Chart type specific options
        options = this.applyTypeSpecificOptions(options, config.type);

        // Merge with custom options if provided
        if (config.customOptions) {
            options = this.deepMerge(options, config.customOptions);
        }

        return options;
    }

    /**
     * Apply type-specific chart options
     * @param {Object} options - Base options
     * @param {string} type - Chart type
     * @returns {Object} Updated options
     */
    applyTypeSpecificOptions(options, type) {
        switch (type) {
            case 'area':
                options.fill = {
                    type: 'gradient',
                    gradient: {
                        shadeIntensity: 1,
                        opacityFrom: 0.4,
                        opacityTo: 0.1,
                        stops: [0, 90, 100]
                    }
                };
                break;

            case 'bar':
            case 'column':
                options.plotOptions = {
                    bar: {
                        borderRadius: 4,
                        columnWidth: '45%',
                        dataLabels: {
                            position: 'top'
                        }
                    }
                };
                break;

            case 'donut':
            case 'pie':
                options.plotOptions = {
                    pie: {
                        donut: {
                            size: type === 'donut' ? '70%' : '0%'
                        }
                    }
                };
                options.legend = {
                    position: 'bottom'
                };
                break;

            case 'radialBar':
                options.plotOptions = {
                    radialBar: {
                        hollow: {
                            size: '70%'
                        }
                    }
                };
                break;
        }

        return options;
    }

    /**
     * Deep merge two objects
     * @param {Object} target - Target object
     * @param {Object} source - Source object
     * @returns {Object} Merged object
     */
    deepMerge(target, source) {
        const output = Object.assign({}, target);
        if (this.isObject(target) && this.isObject(source)) {
            Object.keys(source).forEach(key => {
                if (this.isObject(source[key])) {
                    if (!(key in target))
                        Object.assign(output, { [key]: source[key] });
                    else
                        output[key] = this.deepMerge(target[key], source[key]);
                } else {
                    Object.assign(output, { [key]: source[key] });
                }
            });
        }
        return output;
    }

    /**
     * Check if value is object
     * @param {*} item - Value to check
     * @returns {boolean}
     */
    isObject(item) {
        return item && typeof item === 'object' && !Array.isArray(item);
    }

    /**
     * Update chart data dynamically
     * @param {string} chartId - Chart container ID
     * @param {Array} newSeries - New series data
     * @param {Array} newCategories - New categories (optional)
     */
    updateChart(chartId, newSeries, newCategories = null) {
        const chart = this.charts.get(chartId);
        if (chart) {
            chart.updateSeries(newSeries);
            if (newCategories) {
                chart.updateOptions({
                    xaxis: { categories: newCategories }
                });
            }
        } else {
            console.warn(`Chart ${chartId} not found`);
        }
    }

    /**
     * Destroy a chart
     * @param {string} chartId - Chart container ID
     */
    destroyChart(chartId) {
        const chart = this.charts.get(chartId);
        if (chart) {
            chart.destroy();
            this.charts.delete(chartId);
        }
    }

    /**
     * Destroy all charts
     */
    destroyAll() {
        this.charts.forEach(chart => chart.destroy());
        this.charts.clear();
    }

    /**
     * Show chart error message
     * @param {HTMLElement} container - Chart container
     * @param {string} message - Error message
     */
    showChartError(container, message) {
        container.innerHTML = `
            <div class="component-error">
                <div class="error-icon">
                    <i class="ri-error-warning-line"></i>
                </div>
                <div class="error-message">
                    Failed to load chart: ${message}
                </div>
                <button class="btn btn-sm btn-soft-primary retry-btn" onclick="location.reload()">
                    Retry
                </button>
            </div>
        `;
    }
}

// Create global instance
const dashboardCharts = new DashboardCharts();

// Auto-initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    dashboardCharts.initializeAll();
});

// Export for use in other modules
window.DashboardCharts = dashboardCharts;
