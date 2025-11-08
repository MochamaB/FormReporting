/**
 * Form Wizard Navigation
 * Handles multi-step form navigation with validation
 */

(function () {
    'use strict';

    // Initialize all wizards on page load
    document.addEventListener('DOMContentLoaded', function () {
        const wizards = document.querySelectorAll('[data-wizard-form="true"]');
        wizards.forEach(function (wizardForm) {
            initializeWizard(wizardForm);
        });
    });

    /**
     * Initialize a wizard form
     */
    function initializeWizard(wizardForm) {
        const formId = wizardForm.id.replace('form-', '');
        const wizardData = {
            formId: formId,
            currentStep: 1,
            totalSteps: wizardForm.querySelectorAll('.wizard-step-content').length,
            validateOnStepChange: wizardForm.dataset.validateOnStepChange === 'true'
        };

        // Store wizard data
        wizardForm.wizardData = wizardData;

        // Setup event listeners
        setupNavigationButtons(wizardForm, wizardData);
        setupStepperClicks(wizardForm, wizardData);

        // Initial update
        updateWizardUI(wizardForm, wizardData);
    }

    /**
     * Setup navigation button event listeners
     */
    function setupNavigationButtons(wizardForm, wizardData) {
        const nextBtn = document.getElementById(`wizard-next-${wizardData.formId}`);
        const prevBtn = document.getElementById(`wizard-prev-${wizardData.formId}`);
        const submitBtn = document.getElementById(`wizard-submit-${wizardData.formId}`);

        if (nextBtn) {
            nextBtn.addEventListener('click', function () {
                navigateToStep(wizardForm, wizardData, wizardData.currentStep + 1);
            });
        }

        if (prevBtn) {
            prevBtn.addEventListener('click', function () {
                navigateToStep(wizardForm, wizardData, wizardData.currentStep - 1);
            });
        }

        if (submitBtn) {
            // Submit button will use default form submission
            submitBtn.addEventListener('click', function () {
                // Optionally validate all steps before final submission
                if (wizardData.validateOnStepChange) {
                    const isValid = validateCurrentStep(wizardForm, wizardData);
                    if (!isValid) {
                        alert('Please fix validation errors before submitting.');
                        return false;
                    }
                }
            });
        }
    }

    /**
     * Setup stepper step click events (if step skipping is allowed)
     */
    function setupStepperClicks(wizardForm, wizardData) {
        const stepperSteps = wizardForm.closest('.form-wizard-wrapper').querySelectorAll('.wizard-step.clickable');

        stepperSteps.forEach(function (stepElement) {
            stepElement.addEventListener('click', function (e) {
                e.preventDefault();
                const targetStep = parseInt(stepElement.dataset.step);
                navigateToStep(wizardForm, wizardData, targetStep);
            });
        });
    }

    /**
     * Navigate to a specific step
     */
    function navigateToStep(wizardForm, wizardData, targetStep) {
        // Validate bounds
        if (targetStep < 1 || targetStep > wizardData.totalSteps) {
            return;
        }

        // Validate current step before leaving (if moving forward)
        if (targetStep > wizardData.currentStep && wizardData.validateOnStepChange) {
            const isValid = validateCurrentStep(wizardForm, wizardData);
            if (!isValid) {
                return;
            }
        }

        // Hide current step
        const currentStepElement = wizardForm.querySelector(`[data-step="${wizardData.currentStep}"]`);
        if (currentStepElement) {
            currentStepElement.style.display = 'none';
        }

        // Update current step
        const previousStep = wizardData.currentStep;
        wizardData.currentStep = targetStep;

        // Show new step
        const newStepElement = wizardForm.querySelector(`[data-step="${targetStep}"]`);
        if (newStepElement) {
            newStepElement.style.display = 'block';
        }

        // Mark previous step as completed if moving forward
        if (targetStep > previousStep) {
            markStepAsCompleted(wizardForm, wizardData, previousStep);
        }

        // Update UI
        updateWizardUI(wizardForm, wizardData);

        // Scroll to top
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    /**
     * Validate fields in current step
     */
    function validateCurrentStep(wizardForm, wizardData) {
        const currentStepElement = wizardForm.querySelector(`.wizard-step-content[data-step="${wizardData.currentStep}"]`);
        if (!currentStepElement) {
            return true;
        }

        let isValid = true;

        // Get all required fields in current step
        const requiredFields = currentStepElement.querySelectorAll('[required]');

        requiredFields.forEach(function (field) {
            // Remove existing validation feedback
            field.classList.remove('is-invalid');
            const feedback = field.parentElement.querySelector('.invalid-feedback');
            if (feedback) {
                feedback.remove();
            }

            // Validate field
            if (!field.value || field.value.trim() === '') {
                isValid = false;
                field.classList.add('is-invalid');

                // Add validation feedback
                const feedbackDiv = document.createElement('div');
                feedbackDiv.className = 'invalid-feedback';
                feedbackDiv.textContent = 'This field is required.';
                field.parentElement.appendChild(feedbackDiv);
            }
        });

        return isValid;
    }

    /**
     * Mark a step as completed in the stepper UI
     */
    function markStepAsCompleted(wizardForm, wizardData, stepNumber) {
        const wrapper = wizardForm.closest('.form-wizard-wrapper');
        const stepElement = wrapper.querySelector(`.wizard-step[data-step="${stepNumber}"]`);

        if (stepElement) {
            stepElement.classList.add('completed');
        }
    }

    /**
     * Update wizard UI elements
     */
    function updateWizardUI(wizardForm, wizardData) {
        const wrapper = wizardForm.closest('.form-wizard-wrapper');

        // Update step counter
        const stepCounter = document.getElementById(`current-step-${wizardData.formId}`);
        if (stepCounter) {
            stepCounter.textContent = wizardData.currentStep;
        }

        // Update progress bar
        const progressPercentage = ((wizardData.currentStep - 1) / (wizardData.totalSteps - 1)) * 100;
        const progressBar = document.getElementById(`wizard-progress-${wizardData.formId}`);
        if (progressBar) {
            progressBar.style.width = progressPercentage + '%';
            progressBar.setAttribute('aria-valuenow', progressPercentage);
        }

        // Update stepper active state
        const stepperSteps = wrapper.querySelectorAll('.wizard-step');
        stepperSteps.forEach(function (step) {
            const stepNum = parseInt(step.dataset.step);
            if (stepNum === wizardData.currentStep) {
                step.classList.add('active');
            } else {
                step.classList.remove('active');
            }
        });

        // Update button visibility
        const prevBtn = document.getElementById(`wizard-prev-${wizardData.formId}`);
        const nextBtn = document.getElementById(`wizard-next-${wizardData.formId}`);
        const submitBtn = document.getElementById(`wizard-submit-${wizardData.formId}`);

        if (prevBtn) {
            prevBtn.style.display = wizardData.currentStep === 1 ? 'none' : 'inline-block';
        }

        const isLastStep = wizardData.currentStep === wizardData.totalSteps;

        if (nextBtn) {
            nextBtn.style.display = isLastStep ? 'none' : 'inline-block';
        }

        if (submitBtn) {
            submitBtn.style.display = isLastStep ? 'inline-block' : 'none';
        }

        // Update hidden field for tracking current step
        const stepIndexField = document.getElementById(`current-step-index-${wizardData.formId}`);
        if (stepIndexField) {
            stepIndexField.value = wizardData.currentStep - 1;
        }
    }

    // Export functions for external use
    window.FormWizard = {
        navigateToStep: function (formId, stepNumber) {
            const wizardForm = document.getElementById(`form-${formId}`);
            if (wizardForm && wizardForm.wizardData) {
                navigateToStep(wizardForm, wizardForm.wizardData, stepNumber);
            }
        },
        getCurrentStep: function (formId) {
            const wizardForm = document.getElementById(`form-${formId}`);
            return wizardForm && wizardForm.wizardData ? wizardForm.wizardData.currentStep : null;
        }
    };
})();
