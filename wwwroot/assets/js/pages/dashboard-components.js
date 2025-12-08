/**
 * Dashboard Components - Counter animations and utilities
 * Used for animating counters and initializing UI components
 */

class DashboardComponents {
    /**
     * Initialize all dashboard components
     */
    static init() {
        this.initCounters();
        this.initFeatherIcons();
    }

    /**
     * Initialize counter animations
     * Animates from 0 to target value when element is in viewport
     */
    static initCounters() {
        const counters = document.querySelectorAll('.counter-value');

        counters.forEach(counter => {
            const target = parseFloat(counter.dataset.target);
            const decimals = parseInt(counter.dataset.decimals) || 0;
            const useSeparator = counter.dataset.separator === 'true';
            const duration = parseInt(counter.dataset.duration) || 2000;

            // Create intersection observer to trigger animation when in viewport
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        this.animateCounter(counter, target, decimals, useSeparator, duration);
                        observer.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.1 });

            observer.observe(counter);
        });
    }

    /**
     * Animate a single counter
     * @param {HTMLElement} element - Counter element
     * @param {number} target - Target value
     * @param {number} decimals - Number of decimal places
     * @param {boolean} useSeparator - Use thousands separator
     * @param {number} duration - Animation duration in ms
     */
    static animateCounter(element, target, decimals, useSeparator, duration) {
        const startTime = performance.now();
        const startValue = 0;

        const animate = (currentTime) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function (ease-out)
            const easeOut = 1 - Math.pow(1 - progress, 3);

            const current = startValue + (target - startValue) * easeOut;

            // Format the number
            let formattedValue;
            if (useSeparator) {
                formattedValue = current.toLocaleString('en-US', {
                    minimumFractionDigits: decimals,
                    maximumFractionDigits: decimals
                });
            } else {
                formattedValue = current.toFixed(decimals);
            }

            element.textContent = formattedValue;

            if (progress < 1) {
                requestAnimationFrame(animate);
            } else {
                // Ensure we end with exact target value
                if (useSeparator) {
                    element.textContent = target.toLocaleString('en-US', {
                        minimumFractionDigits: decimals,
                        maximumFractionDigits: decimals
                    });
                } else {
                    element.textContent = target.toFixed(decimals);
                }
            }
        };

        requestAnimationFrame(animate);
    }

    /**
     * Initialize Feather icons
     */
    static initFeatherIcons() {
        if (typeof feather !== 'undefined') {
            feather.replace();
        }
    }

    /**
     * Reinitialize components after AJAX content load
     * Call this after dynamically loading content
     */
    static reinit() {
        this.initCounters();
        this.initFeatherIcons();
    }
}

// Auto-initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    DashboardComponents.init();
});

// Export for use in other modules
window.DashboardComponents = DashboardComponents;
