/**
 * Universal Wizard Navigation Script
 * Works for both Vertical and Horizontal wizards
 * Handles multi-step navigation with automatic state management
 * Supports custom validation callbacks per wizard instance
 */

(function () {
    'use strict';

    // Global validation callbacks registry (indexed by wizard/form ID)
    window.wizardValidationCallbacks = window.wizardValidationCallbacks || {};

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
            var stepIconDiv = step.querySelector('.wizard-step-icon');
            var stepNumber = step.getAttribute('data-step-number');

            if (tabPane && tabPane.classList.contains('active')) {
                // Current active step
                step.classList.remove('done');
                step.classList.add('active');
                // Show step number (not checkmark)
                stepIconDiv.innerHTML = '<span class="step-number">' + stepNumber + '</span>';
                activeFound = true;
            } else if (!activeFound) {
                // Steps before active (completed/done)
                step.classList.remove('active');
                step.classList.add('done');
                // Show checkmark icon
                stepIconDiv.innerHTML = '<i class="ri-check-line"></i>';
            } else {
                // Steps after active (pending)
                step.classList.remove('active', 'done');
                // Show step number
                stepIconDiv.innerHTML = '<span class="step-number">' + stepNumber + '</span>';
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
                var nextTabTrigger = document.getElementById(nextTabId);

                if (nextTabTrigger) {
                    // Get wizard ID for validation callback lookup
                    var form = this.closest('form');
                    var wizardId = form ? form.id : null;

                    // Run validation if callback exists
                    var isValid = true;
                    if (wizardId && window.wizardValidationCallbacks[wizardId]) {
                        // Find current active tab to determine step number
                        var currentWizardContainer = this.closest('.row');
                        var currentActiveTab = currentWizardContainer ? currentWizardContainer.querySelector('.tab-pane.active') : null;
                        var currentStepId = currentActiveTab ? currentActiveTab.id : null;

                        isValid = window.wizardValidationCallbacks[wizardId](currentStepId);
                    }

                    // Only proceed if validation passed
                    if (isValid) {
                        // Pure JavaScript navigation - no Bootstrap Tab API
                        showTab(nextTabTrigger);

                        // Update states after transition
                        setTimeout(function () {
                            var wizardRow = nextTabTrigger.closest('.row');
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
                }
            });
        });

        // Previous buttons
        var prevButtons = wizardContainer.querySelectorAll('.previestab');
        prevButtons.forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                var prevTabId = this.getAttribute('data-previous');
                var prevTabTrigger = document.getElementById(prevTabId);

                if (prevTabTrigger) {
                    // Pure JavaScript navigation - no Bootstrap Tab API
                    showTab(prevTabTrigger);

                    // Update states after transition
                    setTimeout(function () {
                        var wizardRow = prevTabTrigger.closest('.row');
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

    // Pure JavaScript tab switching (no Bootstrap Tab API)
    function showTab(tabTrigger) {
        // Get the target tab pane ID from data-bs-target
        var targetId = tabTrigger.getAttribute('data-bs-target');
        if (!targetId) return;

        var targetPane = document.querySelector(targetId);
        if (!targetPane) return;

        // Find the wizard container (either .row for vertical or parent for horizontal)
        var container = tabTrigger.closest('.row');
        if (!container) return;

        // 1. Deactivate all wizard step triggers (li elements)
        var allStepTriggers = container.querySelectorAll('.wizard-step, .horizontal-wizard-step');
        allStepTriggers.forEach(function (step) {
            step.classList.remove('active');
            step.setAttribute('aria-selected', 'false');
        });

        // 2. Activate the clicked trigger
        tabTrigger.classList.add('active');
        tabTrigger.setAttribute('aria-selected', 'true');

        // 3. Hide all tab panes
        var allTabPanes = container.querySelectorAll('.tab-pane');
        allTabPanes.forEach(function (pane) {
            pane.classList.remove('show', 'active');
        });

        // 4. Show the target tab pane
        targetPane.classList.add('show', 'active');
    }

    // Attach step click listeners
    function attachStepClickListeners(wizardContainer, stepSelector) {
        var steps = wizardContainer.querySelectorAll(stepSelector);

        steps.forEach(function (step) {
            step.addEventListener('click', function (e) {
                e.preventDefault();
                var tabTarget = this.getAttribute('data-bs-target');

                if (tabTarget) {
                    // Pure JavaScript navigation - no Bootstrap Tab API
                    showTab(this);

                    // Update states after transition
                    var currentStep = this;
                    setTimeout(function () {
                        // Check if vertical or horizontal
                        if (stepSelector === '.wizard-step') {
                            var wizardRow = currentStep.closest('.row');
                            if (wizardRow) {
                                updateWizardStates(wizardRow);
                            }
                        } else {
                            var wizardRow = currentStep.closest('.row').parentElement;
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
