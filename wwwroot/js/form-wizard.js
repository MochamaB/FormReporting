/**
 * Universal Wizard Navigation Script
 * Works for both Vertical and Horizontal wizards
 * Handles multi-step navigation with automatic state management
 */

(function () {
    'use strict';

    // Wait for DOM to be ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeWizards);
    } else {
        initializeWizards();
    }

    function initializeWizards() {
        // Find all wizard containers (both vertical and horizontal)
        var verticalWizards = document.querySelectorAll('.wizard-stepper');
        var horizontalWizards = document.querySelectorAll('.horizontal-wizard-stepper');

        // Initialize vertical wizards
        verticalWizards.forEach(function (wizardContainer) {
            var wizardRow = wizardContainer.closest('.row');
            if (!wizardRow) return;

            // Initialize state based on config (respects Done/Active/Pending from server)
            updateWizardStates(wizardRow);

            // Attach event listeners for this wizard instance
            attachNavigationListeners(wizardRow);
            attachStepClickListeners(wizardRow, '.wizard-step');
        });

        // Initialize horizontal wizards
        horizontalWizards.forEach(function (wizardContainer) {
            var wizardRow = wizardContainer.closest('.row').parentElement;
            if (!wizardRow) return;

            // Initialize state based on config
            updateHorizontalWizardStates(wizardRow);

            // Attach event listeners
            attachNavigationListeners(wizardRow);
            attachStepClickListeners(wizardRow, '.horizontal-wizard-step');
        });
    }

    // Update step states based on active tab (Vertical Wizard)
    function updateWizardStates(wizardRow) {
        var steps = wizardRow.querySelectorAll('.wizard-step');
        var activeFound = false;

        steps.forEach(function (step) {
            var tabTarget = step.getAttribute('data-bs-target');
            var tabPane = wizardRow.querySelector(tabTarget);

            if (tabPane && tabPane.classList.contains('active')) {
                // Current active step
                step.classList.remove('done');
                step.classList.add('active');
                activeFound = true;
            } else if (!activeFound) {
                // Steps before active (completed/done)
                step.classList.remove('active');
                step.classList.add('done');
            } else {
                // Steps after active (pending)
                step.classList.remove('active', 'done');
            }
        });
    }

    // Update step states for horizontal wizard
    function updateHorizontalWizardStates(wizardContainer) {
        var steps = wizardContainer.querySelectorAll('.horizontal-wizard-step');
        var activeFound = false;

        steps.forEach(function (step) {
            var tabTarget = step.getAttribute('data-bs-target');
            var tabPane = wizardContainer.querySelector(tabTarget);

            if (tabPane && tabPane.classList.contains('active')) {
                // Current active step
                step.classList.remove('done');
                step.classList.add('active');
                activeFound = true;
            } else if (!activeFound) {
                // Steps before active (completed/done)
                step.classList.remove('active');
                step.classList.add('done');
            } else {
                // Steps after active (pending)
                step.classList.remove('active', 'done');
            }
        });
    }

    // Attach navigation button listeners
    function attachNavigationListeners(wizardContainer) {
        // Next buttons
        var nextButtons = wizardContainer.querySelectorAll('.nexttab');
        nextButtons.forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var nextTabId = this.getAttribute('data-nexttab');
                var nextTab = document.getElementById(nextTabId);

                if (nextTab) {
                    var tab = new bootstrap.Tab(nextTab);
                    tab.show();

                    // Update states after transition
                    setTimeout(function () {
                        var wizardRow = nextTab.closest('.row');
                        if (wizardRow) {
                            // Check if vertical or horizontal
                            if (wizardRow.querySelector('.wizard-stepper')) {
                                updateWizardStates(wizardRow);
                            } else {
                                updateHorizontalWizardStates(wizardRow.parentElement);
                            }
                        }
                    }, 50);
                }
            });
        });

        // Previous buttons
        var prevButtons = wizardContainer.querySelectorAll('.previestab');
        prevButtons.forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var prevTabId = this.getAttribute('data-previous');
                var prevTab = document.getElementById(prevTabId);

                if (prevTab) {
                    var tab = new bootstrap.Tab(prevTab);
                    tab.show();

                    // Update states after transition
                    setTimeout(function () {
                        var wizardRow = prevTab.closest('.row');
                        if (wizardRow) {
                            // Check if vertical or horizontal
                            if (wizardRow.querySelector('.wizard-stepper')) {
                                updateWizardStates(wizardRow);
                            } else {
                                updateHorizontalWizardStates(wizardRow.parentElement);
                            }
                        }
                    }, 50);
                }
            });
        });
    }

    // Attach step click listeners
    function attachStepClickListeners(wizardContainer, stepSelector) {
        var steps = wizardContainer.querySelectorAll(stepSelector);

        steps.forEach(function (step) {
            step.addEventListener('click', function (e) {
                e.preventDefault();
                var tabTarget = this.getAttribute('data-bs-target');

                if (tabTarget) {
                    // Trigger the corresponding tab
                    var tab = new bootstrap.Tab(this);
                    tab.show();

                    // Update states after transition
                    setTimeout(function () {
                        // Check if vertical or horizontal
                        if (stepSelector === '.wizard-step') {
                            var wizardRow = step.closest('.row');
                            if (wizardRow) {
                                updateWizardStates(wizardRow);
                            }
                        } else {
                            var wizardRow = step.closest('.row').parentElement;
                            if (wizardRow) {
                                updateHorizontalWizardStates(wizardRow);
                            }
                        }
                    }, 50);
                }
            });
        });
    }
})();
