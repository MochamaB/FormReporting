/**
 * Dashboard Loader - AJAX Lazy Loading with Skeleton Placeholders
 * Progressively loads dashboard components with priority queueing
 */

class DashboardLoader {
    constructor() {
        this.queue = [];
        this.loading = new Set();
        this.loaded = new Set();
        this.maxConcurrent = 3; // Load max 3 components simultaneously
        this.retryAttempts = 3;
        this.retryDelay = 1000; // 1 second

        // Priority levels
        this.priorities = {
            critical: 1,
            high: 2,
            normal: 3,
            low: 4
        };
    }

    /**
     * Initialize the loader and start loading components
     */
    init() {
        // Find all components to load
        const components = document.querySelectorAll('[data-component]');

        components.forEach(element => {
            const componentData = {
                element: element,
                type: element.dataset.component,
                url: element.dataset.url,
                priority: this.priorities[element.dataset.priority] || this.priorities.normal,
                retries: 0
            };

            this.queue.push(componentData);
        });

        // Sort queue by priority
        this.queue.sort((a, b) => a.priority - b.priority);

        console.log(`DashboardLoader initialized with ${this.queue.length} components`);

        // Start loading
        this.processQueue();

        // Setup intersection observer for lazy loading
        this.setupIntersectionObserver();
    }

    /**
     * Process the loading queue
     */
    processQueue() {
        // Load up to maxConcurrent components
        while (this.loading.size < this.maxConcurrent && this.queue.length > 0) {
            const component = this.queue.shift();
            this.loadComponent(component);
        }
    }

    /**
     * Load a single component
     * @param {Object} component - Component data
     */
    async loadComponent(component) {
        const { element, type, url, retries } = component;
        const componentId = element.id || `component-${Date.now()}`;

        if (!element.id) {
            element.id = componentId;
        }

        console.log(`Loading ${type} from ${url}...`);

        this.loading.add(componentId);
        element.classList.add('component-loading');

        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const html = await response.text();

            // Replace skeleton with actual content
            element.innerHTML = html;
            element.classList.remove('component-loading');
            element.classList.add('component-loaded');

            this.loading.delete(componentId);
            this.loaded.add(componentId);

            console.log(`${type} loaded successfully`);

            // Reinitialize components (counters, charts, etc.)
            this.reinitializeComponents(element);

            // Process next in queue
            this.processQueue();

        } catch (error) {
            console.error(`Error loading ${type}:`, error);

            // Retry logic
            if (retries < this.retryAttempts) {
                console.log(`Retrying ${type}... (${retries + 1}/${this.retryAttempts})`);

                this.loading.delete(componentId);
                component.retries++;

                // Re-add to queue with delay
                setTimeout(() => {
                    this.queue.unshift(component); // Add to front (high priority)
                    this.processQueue();
                }, this.retryDelay);

            } else {
                // Show error
                this.showComponentError(element, type, url, error.message);
                this.loading.delete(componentId);

                // Process next in queue
                this.processQueue();
            }
        }
    }

    /**
     * Reinitialize components after AJAX load
     * @param {HTMLElement} container - Container element
     */
    reinitializeComponents(container) {
        // Reinitialize counters
        if (window.DashboardComponents) {
            window.DashboardComponents.reinit();
        }

        // Reinitialize charts
        const charts = container.querySelectorAll('[id^="chart-"]');
        if (charts.length > 0 && window.DashboardCharts) {
            charts.forEach(chart => {
                window.DashboardCharts.initializeChart(chart);
            });
        }

        // Reinitialize any other components
        // Add more reinitializations as needed
    }

    /**
     * Setup intersection observer for viewport-based lazy loading
     * Only loads components when they become visible
     */
    setupIntersectionObserver() {
        // Find components marked for viewport loading
        const lazyComponents = document.querySelectorAll('[data-lazy-load="viewport"]');

        if (lazyComponents.length === 0) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const element = entry.target;
                    const componentId = element.id;

                    // Check if not already loaded/loading
                    if (!this.loaded.has(componentId) && !this.loading.has(componentId)) {
                        // Find component in queue and prioritize it
                        const index = this.queue.findIndex(c => c.element === element);
                        if (index !== -1) {
                            const component = this.queue.splice(index, 1)[0];
                            this.loadComponent(component);
                        }
                    }

                    observer.unobserve(element);
                }
            });
        }, {
            root: null,
            rootMargin: '50px', // Start loading 50px before entering viewport
            threshold: 0.1
        });

        lazyComponents.forEach(element => {
            observer.observe(element);
        });
    }

    /**
     * Show component error message
     * @param {HTMLElement} element - Component container
     * @param {string} type - Component type
     * @param {string} url - Component URL
     * @param {string} message - Error message
     */
    showComponentError(element, type, url, message) {
        element.classList.remove('component-loading');
        element.innerHTML = `
            <div class="component-error">
                <div class="error-icon">
                    <i class="ri-error-warning-line"></i>
                </div>
                <div class="error-message">
                    <strong>Failed to load ${type}</strong><br>
                    ${message}
                </div>
                <button class="btn btn-sm btn-soft-primary retry-btn"
                        onclick="DashboardLoader.retryComponent('${element.id}', '${type}', '${url}')">
                    <i class="ri-refresh-line"></i> Retry
                </button>
            </div>
        `;
    }

    /**
     * Retry loading a component (called from error UI)
     * @param {string} elementId - Element ID
     * @param {string} type - Component type
     * @param {string} url - Component URL
     */
    static retryComponent(elementId, type, url) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const component = {
            element: element,
            type: type,
            url: url,
            priority: dashboardLoader.priorities.critical,
            retries: 0
        };

        dashboardLoader.loadComponent(component);
    }

    /**
     * Manually load a component by ID
     * @param {string} elementId - Element ID
     */
    loadById(elementId) {
        const element = document.getElementById(elementId);
        if (!element || !element.dataset.component) {
            console.error(`Component ${elementId} not found or invalid`);
            return;
        }

        const component = {
            element: element,
            type: element.dataset.component,
            url: element.dataset.url,
            priority: this.priorities.critical,
            retries: 0
        };

        this.loadComponent(component);
    }

    /**
     * Get loading stats
     * @returns {Object} Loading statistics
     */
    getStats() {
        return {
            total: this.queue.length + this.loading.size + this.loaded.size,
            queued: this.queue.length,
            loading: this.loading.size,
            loaded: this.loaded.size
        };
    }
}

// Create global instance
const dashboardLoader = new DashboardLoader();

// Auto-initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    dashboardLoader.init();

    // Log stats after 5 seconds
    setTimeout(() => {
        const stats = dashboardLoader.getStats();
        console.log('Dashboard loading stats:', stats);
    }, 5000);
});

// Export for use in other modules
window.DashboardLoader = dashboardLoader;
