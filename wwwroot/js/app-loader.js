/**
 * App Loader - Universal Progress Bar
 * Provides visual loading feedback for page loads and AJAX requests
 * 
 * Usage:
 *   AppLoader.start()    - Start the progress bar
 *   AppLoader.done()     - Complete and hide the progress bar
 *   AppLoader.set(50)    - Set to specific percentage (0-100)
 *   AppLoader.inc()      - Increment by a small random amount
 * 
 * Auto-integration:
 *   - Automatically hooks into fetch() requests
 *   - Shows on page navigation (beforeunload)
 */

const AppLoader = (function() {
    'use strict';

    // Configuration
    const config = {
        minimum: 8,           // Starting percentage
        trickleSpeed: 200,    // How often to trickle (ms)
        trickleAmount: [1, 3], // Random increment range
        speed: 200,           // Animation speed (ms)
        easing: 'ease-out'
    };

    // State
    let progress = 0;
    let isActive = false;
    let trickleInterval = null;
    let pendingRequests = 0;

    // DOM elements (lazy initialized)
    let loaderEl = null;
    let barEl = null;

    /**
     * Initialize DOM elements
     */
    function init() {
        loaderEl = document.getElementById('app-loader');
        if (loaderEl) {
            barEl = loaderEl.querySelector('.app-loader-bar');
        }

        // Auto-hook into fetch if available
        if (typeof window.fetch === 'function') {
            hookFetch();
        }

        // Show loader on page navigation
        window.addEventListener('beforeunload', function() {
            start();
        });
    }

    /**
     * Hook into fetch() to auto-show loader
     */
    function hookFetch() {
        const originalFetch = window.fetch;
        window.fetch = function(...args) {
            // Check if this fetch should show loader (can be disabled with custom header)
            const options = args[1] || {};
            const showLoader = options.showLoader !== false;

            if (showLoader) {
                pendingRequests++;
                if (pendingRequests === 1) {
                    start();
                }
            }

            return originalFetch.apply(this, args)
                .then(response => {
                    if (showLoader) {
                        pendingRequests--;
                        if (pendingRequests <= 0) {
                            pendingRequests = 0;
                            done();
                        }
                    }
                    return response;
                })
                .catch(error => {
                    if (showLoader) {
                        pendingRequests--;
                        if (pendingRequests <= 0) {
                            pendingRequests = 0;
                            done();
                        }
                    }
                    throw error;
                });
        };
    }

    /**
     * Start the progress bar
     */
    function start() {
        if (!barEl) init();
        if (!barEl) return; // Element not found

        isActive = true;
        progress = config.minimum;
        
        // Show and set initial progress
        loaderEl.classList.add('active');
        loaderEl.classList.remove('completing');
        setBarWidth(progress);

        // Start trickling
        startTrickle();
    }

    /**
     * Complete the progress bar
     */
    function done() {
        if (!isActive) return;

        // Stop trickling
        stopTrickle();

        // Complete to 100%
        progress = 100;
        loaderEl.classList.add('completing');
        setBarWidth(100);

        // Hide after animation
        setTimeout(function() {
            if (progress >= 100) {
                loaderEl.classList.remove('active', 'completing');
                setBarWidth(0);
                isActive = false;
                progress = 0;
            }
        }, 300);
    }

    /**
     * Set progress to specific percentage
     */
    function set(value) {
        if (!barEl) init();
        if (!barEl) return;

        value = clamp(value, config.minimum, 100);
        progress = value;

        if (!isActive && value < 100) {
            loaderEl.classList.add('active');
            isActive = true;
            startTrickle();
        }

        setBarWidth(value);

        if (value >= 100) {
            done();
        }
    }

    /**
     * Increment progress by a small random amount
     */
    function inc() {
        if (!isActive) {
            start();
            return;
        }

        // Slow down as we approach 100%
        let amount;
        if (progress < 25) {
            amount = random(3, 6);
        } else if (progress < 50) {
            amount = random(2, 4);
        } else if (progress < 75) {
            amount = random(1, 3);
        } else if (progress < 90) {
            amount = random(0.5, 1.5);
        } else if (progress < 99) {
            amount = random(0.1, 0.5);
        } else {
            amount = 0; // Don't go past 99% until done() is called
        }

        progress = clamp(progress + amount, 0, 99.5);
        setBarWidth(progress);
    }

    /**
     * Start the trickle animation
     */
    function startTrickle() {
        stopTrickle();
        trickleInterval = setInterval(function() {
            inc();
        }, config.trickleSpeed);
    }

    /**
     * Stop the trickle animation
     */
    function stopTrickle() {
        if (trickleInterval) {
            clearInterval(trickleInterval);
            trickleInterval = null;
        }
    }

    /**
     * Set the bar width
     */
    function setBarWidth(percent) {
        if (barEl) {
            barEl.style.width = percent + '%';
        }
    }

    /**
     * Clamp a value between min and max
     */
    function clamp(value, min, max) {
        return Math.min(Math.max(value, min), max);
    }

    /**
     * Get random number between min and max
     */
    function random(min, max) {
        return Math.random() * (max - min) + min;
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Public API
    return {
        start: start,
        done: done,
        set: set,
        inc: inc,
        
        // Check if currently active
        isActive: function() { return isActive; },
        
        // Get current progress
        getProgress: function() { return progress; }
    };
})();
