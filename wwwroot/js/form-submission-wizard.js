/**
 * Form Submission Wizard Navigation
 * Handles step navigation, validation, auto-save, and stepper synchronization
 */

(function () {
    'use strict';

    // ========================================================================
    // CONFIGURATION
    // ========================================================================
    
    const CONFIG = {
        autoSaveInterval: 30000, // 30 seconds
        animationDuration: 300,
        validationDelay: 100
    };

    // ========================================================================
    // STATE MANAGEMENT
    // ========================================================================
    
    const wizardInstances = new Map();

    class FormWizard {
        constructor(wizardElement) {
            this.wrapper = wizardElement;
            this.formId = wizardElement.dataset.wizardId;
            this.form = document.getElementById(`form-${this.formId}`);
            
            if (!this.form) {
                console.error(`Form not found for wizard: ${this.formId}`);
                return;
            }

            // Elements
            this.stepContents = this.wrapper.querySelectorAll('.wizard-step-content');
            this.prevBtn = document.getElementById(`wizard-prev-${this.formId}`);
            this.nextBtn = document.getElementById(`wizard-next-${this.formId}`);
            this.submitBtn = document.getElementById(`wizard-submit-${this.formId}`);
            this.currentStepInput = document.getElementById(`current-step-index-${this.formId}`);
            
            // Find stepper (in _FormProgress.cshtml)
            this.stepper = document.getElementById(`wizard-stepper-${this.formId}`);
            this.stepperItems = this.stepper ? this.stepper.querySelectorAll('.submission-wizard-step') : [];
            
            // State
            this.currentStep = 0;
            this.totalSteps = this.stepContents.length;
            this.isNavigating = false;
            this.isDirty = false;
            this.autoSaveTimer = null;

            // Initialize
            this.init();
        }

        init() {
            this.bindEvents();
            this.updateNavigationButtons();
            this.updateStepperState();
            this.startAutoSave();
            
            // Restore to saved step if resuming
            const savedStep = parseInt(this.currentStepInput?.value) || 0;
            if (savedStep > 0 && savedStep < this.totalSteps) {
                this.goToStep(savedStep, false);
            }

            console.log(`Wizard initialized: ${this.formId}, Steps: ${this.totalSteps}`);
        }

        // ====================================================================
        // EVENT BINDING
        // ====================================================================

        bindEvents() {
            // Next button
            if (this.nextBtn) {
                this.nextBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.nextStep();
                });
            }

            // Previous button
            if (this.prevBtn) {
                this.prevBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.prevStep();
                });
            }

            // Submit button
            if (this.submitBtn) {
                this.submitBtn.addEventListener('click', (e) => {
                    // Validate first
                    if (!this.validateCurrentStep()) {
                        e.preventDefault();
                        return;
                    }
                    
                    // Clear dirty flag to prevent "leave site" warning on form submit
                    this.isDirty = false;
                    
                    // Let form submit naturally
                });
            }

            // Stepper click navigation
            this.stepperItems.forEach((item, index) => {
                item.addEventListener('click', (e) => {
                    e.preventDefault();
                    // Only allow clicking on completed steps or current step
                    if (index <= this.currentStep) {
                        this.goToStep(index, true);
                    }
                });
            });

            // Track form changes for dirty state
            this.form.addEventListener('input', () => {
                this.isDirty = true;
            });

            this.form.addEventListener('change', () => {
                this.isDirty = true;
            });

            // Warn before leaving with unsaved changes
            window.addEventListener('beforeunload', (e) => {
                if (this.isDirty) {
                    e.preventDefault();
                    e.returnValue = '';
                }
            });

            // Keyboard navigation
            document.addEventListener('keydown', (e) => {
                // Only if this wizard's form is focused
                if (!this.form.contains(document.activeElement)) return;
                
                if (e.key === 'Enter' && e.ctrlKey) {
                    e.preventDefault();
                    this.nextStep();
                }
            });
        }

        // ====================================================================
        // NAVIGATION
        // ====================================================================

        nextStep() {
            if (this.isNavigating) return;
            
            // Validate current step before proceeding
            if (!this.validateCurrentStep()) {
                this.showValidationErrors();
                return;
            }

            if (this.currentStep < this.totalSteps - 1) {
                this.goToStep(this.currentStep + 1, true);
            }
        }

        prevStep() {
            if (this.isNavigating) return;
            
            if (this.currentStep > 0) {
                this.goToStep(this.currentStep - 1, true);
            }
        }

        goToStep(stepIndex, animate = true) {
            if (stepIndex < 0 || stepIndex >= this.totalSteps) return;
            if (stepIndex === this.currentStep) return;

            this.isNavigating = true;

            const currentContent = this.stepContents[this.currentStep];
            const nextContent = this.stepContents[stepIndex];
            const direction = stepIndex > this.currentStep ? 'forward' : 'backward';

            if (animate) {
                // Animate transition
                currentContent.style.opacity = '0';
                currentContent.style.transform = direction === 'forward' ? 'translateX(-20px)' : 'translateX(20px)';
                
                setTimeout(() => {
                    currentContent.style.display = 'none';
                    nextContent.style.display = 'block';
                    nextContent.style.opacity = '0';
                    nextContent.style.transform = direction === 'forward' ? 'translateX(20px)' : 'translateX(-20px)';
                    
                    // Force reflow
                    nextContent.offsetHeight;
                    
                    nextContent.style.transition = `opacity ${CONFIG.animationDuration}ms ease, transform ${CONFIG.animationDuration}ms ease`;
                    nextContent.style.opacity = '1';
                    nextContent.style.transform = 'translateX(0)';
                    
                    setTimeout(() => {
                        this.isNavigating = false;
                        nextContent.style.transition = '';
                    }, CONFIG.animationDuration);
                }, CONFIG.animationDuration);
            } else {
                // No animation
                currentContent.style.display = 'none';
                nextContent.style.display = 'block';
                nextContent.style.opacity = '1';
                nextContent.style.transform = 'translateX(0)';
                this.isNavigating = false;
            }

            // Update state
            this.currentStep = stepIndex;
            if (this.currentStepInput) {
                this.currentStepInput.value = stepIndex;
            }

            // Update UI
            this.updateNavigationButtons();
            this.updateStepperState();

            // Scroll to top of form
            this.wrapper.scrollIntoView({ behavior: 'smooth', block: 'start' });

            // Auto-save on step change
            this.triggerAutoSave();
        }

        // ====================================================================
        // UI UPDATES
        // ====================================================================

        updateNavigationButtons() {
            // Previous button visibility
            if (this.prevBtn) {
                this.prevBtn.style.display = this.currentStep > 0 ? 'inline-flex' : 'none';
            }

            // Next vs Submit button
            const isLastStep = this.currentStep === this.totalSteps - 1;
            
            if (this.nextBtn) {
                this.nextBtn.style.display = isLastStep ? 'none' : 'inline-flex';
            }
            
            if (this.submitBtn) {
                this.submitBtn.style.display = isLastStep ? 'inline-flex' : 'none';
            }
        }

        updateStepperState() {
            if (!this.stepperItems.length) return;

            this.stepperItems.forEach((item, index) => {
                const circle = item.querySelector('.submission-step-circle');
                const stepNumber = index + 1;
                
                // Remove all state classes
                item.classList.remove('done', 'active');
                
                if (index < this.currentStep) {
                    // Completed step
                    item.classList.add('done');
                    if (circle) {
                        circle.innerHTML = '<i class="ri-check-line"></i>';
                    }
                    item.style.cursor = 'pointer';
                } else if (index === this.currentStep) {
                    // Current step
                    item.classList.add('active');
                    if (circle) {
                        // Restore original content (number or icon)
                        const originalIcon = item.dataset.icon;
                        if (originalIcon) {
                            circle.innerHTML = `<i class="${originalIcon}"></i>`;
                        } else {
                            circle.innerHTML = `<span>${stepNumber}</span>`;
                        }
                    }
                    item.style.cursor = 'default';
                } else {
                    // Future step
                    if (circle) {
                        circle.innerHTML = `<span>${stepNumber}</span>`;
                    }
                    item.style.cursor = 'not-allowed';
                }
            });
        }

        // ====================================================================
        // VALIDATION
        // ====================================================================

        validateCurrentStep() {
            const currentContent = this.stepContents[this.currentStep];
            if (!currentContent) return true;

            const fields = currentContent.querySelectorAll('input, select, textarea');
            let isValid = true;
            let firstInvalidField = null;

            fields.forEach(field => {
                // Clear previous validation state
                field.classList.remove('is-invalid', 'is-valid');
                const feedback = field.closest('.mb-3')?.querySelector('.invalid-feedback');
                if (feedback) feedback.textContent = '';

                // Skip disabled/readonly fields
                if (field.disabled || field.readOnly) return;

                // Check HTML5 validation
                if (!field.checkValidity()) {
                    isValid = false;
                    field.classList.add('is-invalid');
                    
                    if (feedback) {
                        feedback.textContent = field.validationMessage || 'This field is required';
                    }
                    
                    if (!firstInvalidField) {
                        firstInvalidField = field;
                    }
                } else if (field.value) {
                    field.classList.add('is-valid');
                }

                // Custom validation from data attribute
                const validationRules = field.dataset.validation;
                if (validationRules && field.value) {
                    try {
                        const rules = JSON.parse(validationRules);
                        const customError = this.runCustomValidation(field, rules);
                        if (customError) {
                            isValid = false;
                            field.classList.remove('is-valid');
                            field.classList.add('is-invalid');
                            if (feedback) {
                                feedback.textContent = customError;
                            }
                            if (!firstInvalidField) {
                                firstInvalidField = field;
                            }
                        }
                    } catch (e) {
                        console.warn('Invalid validation rules:', e);
                    }
                }
            });

            // Focus first invalid field
            if (firstInvalidField) {
                firstInvalidField.focus();
                firstInvalidField.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }

            return isValid;
        }

        runCustomValidation(field, rules) {
            for (const rule of rules) {
                const value = field.value;
                
                switch (rule.ValidationType?.toLowerCase()) {
                    case 'minlength':
                        if (value.length < (rule.MinLength || 0)) {
                            return rule.ErrorMessage || `Minimum ${rule.MinLength} characters required`;
                        }
                        break;
                    case 'maxlength':
                        if (value.length > (rule.MaxLength || Infinity)) {
                            return rule.ErrorMessage || `Maximum ${rule.MaxLength} characters allowed`;
                        }
                        break;
                    case 'minvalue':
                        if (parseFloat(value) < (rule.MinValue || -Infinity)) {
                            return rule.ErrorMessage || `Minimum value is ${rule.MinValue}`;
                        }
                        break;
                    case 'maxvalue':
                        if (parseFloat(value) > (rule.MaxValue || Infinity)) {
                            return rule.ErrorMessage || `Maximum value is ${rule.MaxValue}`;
                        }
                        break;
                    case 'regex':
                    case 'pattern':
                        if (rule.RegexPattern) {
                            const regex = new RegExp(rule.RegexPattern);
                            if (!regex.test(value)) {
                                return rule.ErrorMessage || 'Invalid format';
                            }
                        }
                        break;
                    case 'email':
                        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                        if (!emailRegex.test(value)) {
                            return rule.ErrorMessage || 'Invalid email address';
                        }
                        break;
                }
            }
            return null;
        }

        showValidationErrors() {
            // Show a toast or alert
            const toast = document.createElement('div');
            toast.className = 'position-fixed top-0 end-0 p-3';
            toast.style.zIndex = '1100';
            toast.innerHTML = `
                <div class="toast show align-items-center text-white bg-danger border-0" role="alert">
                    <div class="d-flex">
                        <div class="toast-body">
                            <i class="ri-error-warning-line me-2"></i>
                            Please fix the errors before continuing
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `;
            document.body.appendChild(toast);
            
            setTimeout(() => {
                toast.remove();
            }, 3000);
        }

        // ====================================================================
        // AUTO-SAVE
        // ====================================================================

        startAutoSave() {
            const autoSaveUrl = this.form.dataset.saveDraftUrl || '/api/submissions/auto-save';
            const autoSaveEnabled = this.form.dataset.autoSave === 'true';
            
            if (!autoSaveEnabled) return;

            this.autoSaveTimer = setInterval(() => {
                if (this.isDirty) {
                    this.triggerAutoSave();
                }
            }, CONFIG.autoSaveInterval);
        }

        async triggerAutoSave() {
            const autoSaveUrl = this.form.dataset.saveDraftUrl || '/api/submissions/auto-save';
            
            // Get submission context
            const submissionIdInput = document.getElementById(`submission-id-${this.formId}`);
            const templateIdInput = document.getElementById(`template-id-${this.formId}`);
            const tenantIdInput = document.getElementById(`tenant-id-${this.formId}`);
            const reportingPeriodInput = document.getElementById(`reporting-period-${this.formId}`);

            const submissionId = parseInt(submissionIdInput?.value) || 0;
            const templateId = parseInt(templateIdInput?.value) || 0;
            const tenantId = tenantIdInput?.value ? parseInt(tenantIdInput.value) : null;
            const reportingPeriod = reportingPeriodInput?.value || '';

            // Collect responses - handle all field types properly
            const responses = {};
            
            // First, collect all form elements by their field ID
            // This handles checkboxes, radios, and regular inputs correctly
            
            // 1. Collect checkboxes (can have multiple values per field)
            const checkboxGroups = {};
            this.form.querySelectorAll('input[type="checkbox"]').forEach(el => {
                const fieldId = this.extractFieldId(el);
                if (fieldId) {
                    if (!checkboxGroups[fieldId]) {
                        checkboxGroups[fieldId] = [];
                    }
                    if (el.checked) {
                        checkboxGroups[fieldId].push(el.value);
                    }
                }
            });
            
            // Add checkbox values (comma-separated for multi-select, or single value)
            Object.keys(checkboxGroups).forEach(fieldId => {
                const values = checkboxGroups[fieldId];
                if (values.length > 0) {
                    responses[fieldId] = values.join(',');
                } else {
                    // No checkboxes checked - store empty or false for single checkbox
                    const checkboxes = this.form.querySelectorAll(`input[type="checkbox"][name="responses[${fieldId}]"], input[type="checkbox"][data-field-id="${fieldId}"]`);
                    if (checkboxes.length === 1) {
                        // Single checkbox - store false
                        responses[fieldId] = 'false';
                    }
                    // Multi-checkbox with none selected - don't store anything (or store empty)
                }
            });
            
            // 2. Collect radio buttons (only one value per field)
            this.form.querySelectorAll('input[type="radio"]:checked').forEach(el => {
                const fieldId = this.extractFieldId(el);
                if (fieldId) {
                    responses[fieldId] = el.value;
                }
            });
            
            // 3. Collect text inputs, textareas, selects, and other input types
            this.form.querySelectorAll('input:not([type="checkbox"]):not([type="radio"]):not([type="hidden"]), textarea, select').forEach(el => {
                const fieldId = this.extractFieldId(el);
                if (fieldId && !responses.hasOwnProperty(fieldId)) {
                    // Handle multi-select elements
                    if (el.tagName === 'SELECT' && el.multiple) {
                        const selectedValues = Array.from(el.selectedOptions).map(opt => opt.value);
                        responses[fieldId] = selectedValues.join(',');
                    } else {
                        responses[fieldId] = el.value;
                    }
                }
            });
            
            // 4. Also check for elements with data-field-id that might have been missed
            this.form.querySelectorAll('[data-field-id]').forEach(el => {
                const fieldId = el.dataset.fieldId;
                if (fieldId && !responses.hasOwnProperty(fieldId)) {
                    if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.tagName === 'SELECT') {
                        if (el.type !== 'checkbox' && el.type !== 'radio') {
                            responses[fieldId] = el.value;
                        }
                    }
                }
            });

            const payload = {
                submissionId: submissionId,
                templateId: templateId,
                tenantId: tenantId,
                reportingPeriod: reportingPeriod,
                currentSection: this.currentStep,
                responses: responses
            };

            // Update status
            this.updateAutoSaveStatus('saving');

            try {
                const response = await fetch(autoSaveUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify(payload)
                });

                const result = await response.json();

                if (result.success) {
                    this.isDirty = false;
                    
                    // Update submission ID if this was lazy creation
                    if (submissionId === 0 && result.submissionId > 0) {
                        submissionIdInput.value = result.submissionId;
                        console.log('Submission created:', result.submissionId);
                    }

                    this.updateAutoSaveStatus('saved', result.savedAt);
                } else {
                    console.error('Auto-save failed:', result.errors);
                    this.updateAutoSaveStatus('error');
                }
            } catch (error) {
                console.error('Auto-save error:', error);
                this.updateAutoSaveStatus('error');
            }
        }

        updateAutoSaveStatus(status, timestamp) {
            // Update global status if function exists
            if (typeof window.updateAutoSaveStatus === 'function') {
                const message = status === 'saved' ? `Saved at ${new Date().toLocaleTimeString()}` : null;
                window.updateAutoSaveStatus(status, message);
            }

            // Show toast for saved status
            if (status === 'saved') {
                const toast = document.getElementById(`auto-save-toast-${this.formId}`);
                if (toast && typeof bootstrap !== 'undefined') {
                    const bsToast = new bootstrap.Toast(toast, { delay: 2000 });
                    bsToast.show();
                }
            }
        }

        /**
         * Extract field ID from an element
         * Checks data-field-id attribute first, then parses from name attribute
         */
        extractFieldId(element) {
            // First check data-field-id attribute
            if (element.dataset.fieldId) {
                return element.dataset.fieldId;
            }
            
            // Then try to parse from name attribute (responses[123] pattern)
            const name = element.name;
            if (name) {
                const match = name.match(/responses\[(\d+)\]/);
                if (match) {
                    return match[1];
                }
                
                // Also check for field_123 pattern
                const fieldMatch = name.match(/^field_(\d+)$/);
                if (fieldMatch) {
                    return fieldMatch[1];
                }
            }
            
            // Check id attribute (field_123 pattern)
            const id = element.id;
            if (id) {
                const idMatch = id.match(/^field_(\d+)/);
                if (idMatch) {
                    return idMatch[1];
                }
            }
            
            return null;
        }

        // ====================================================================
        // CLEANUP
        // ====================================================================

        destroy() {
            if (this.autoSaveTimer) {
                clearInterval(this.autoSaveTimer);
            }
        }
    }

    // ========================================================================
    // INITIALIZATION
    // ========================================================================

    function initializeWizards() {
        const wizards = document.querySelectorAll('.form-wizard-wrapper');
        
        wizards.forEach(wizard => {
            const wizardId = wizard.dataset.wizardId;
            if (wizardId && !wizardInstances.has(wizardId)) {
                const instance = new FormWizard(wizard);
                wizardInstances.set(wizardId, instance);
            }
        });
    }

    // ========================================================================
    // GLOBAL FUNCTIONS (for onclick handlers in HTML)
    // ========================================================================

    window.saveDraft = function(formId) {
        const instance = wizardInstances.get(formId);
        if (instance) {
            instance.triggerAutoSave();
        }
    };

    window.cancelForm = function(formId) {
        const instance = wizardInstances.get(formId);
        if (instance && instance.isDirty) {
            // Modal will handle confirmation
            return;
        }
        // If not dirty, allow navigation
        const cancelUrl = document.querySelector(`#form-${formId}`)?.closest('.form-wizard-wrapper')?.dataset.cancelUrl;
        if (cancelUrl) {
            window.location.href = cancelUrl;
        }
    };

    // ========================================================================
    // DOM READY
    // ========================================================================

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeWizards);
    } else {
        initializeWizards();
    }

    // Re-initialize on dynamic content (for SPA-like behavior)
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            if (mutation.addedNodes.length) {
                mutation.addedNodes.forEach((node) => {
                    if (node.nodeType === 1 && node.classList?.contains('form-wizard-wrapper')) {
                        initializeWizards();
                    }
                });
            }
        });
    });

    observer.observe(document.body, { childList: true, subtree: true });

})();
