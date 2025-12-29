/**
 * Workflow Wizard Step-by-Step Validation
 * Provides real-time validation for workflow wizard steps
 * Integrates with existing WorkflowService validation methods
 */

const WorkflowStepValidation = {
    
    /**
     * Validate current step configuration
     * @param {string} stepId - The step identifier (target, action, assignee, settings)
     * @param {number} templateId - The template ID
     * @returns {Promise<boolean>} - True if step is valid
     */
    async validateCurrentStep(stepId, templateId) {
        const stepData = this.collectStepData(stepId, templateId);
        
        try {
            const response = await fetch('/api/workflows/validate-step', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(stepData)
            });
            
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            const result = await response.json();
            this.displayValidationResult(stepId, result);
            return result.IsValid;
            
        } catch (error) {
            console.error('Validation error:', error);
            this.displayValidationError(stepId, 'Unable to validate step. Please try again.');
            return false;
        }
    },
    
    /**
     * Collect form data for current step
     * @param {string} stepId - The step identifier
     * @param {number} templateId - The template ID
     * @returns {object} - Step validation data
     */
    collectStepData(stepId, templateId) {
        const formData = new FormData(document.getElementById('create-workflow-form'));
        
        // Handle checkbox values properly (checkboxes send 'on' when checked, nothing when unchecked)
        const isMandatoryCheckbox = document.getElementById('is-mandatory');
        const isParallelCheckbox = document.getElementById('is-parallel');
        
        return {
            TemplateId: templateId,
            StepId: stepId,
            TargetType: formData.get('TargetType'),
            TargetId: formData.get('TargetId') ? parseInt(formData.get('TargetId')) : null,
            ActionId: formData.get('ActionId') ? parseInt(formData.get('ActionId')) : null,
            StepName: formData.get('StepName'),
            AssigneeType: formData.get('AssigneeType'),
            ApproverRoleId: formData.get('ApproverRoleId') ? parseInt(formData.get('ApproverRoleId')) : null,
            ApproverUserId: formData.get('ApproverUserId') ? parseInt(formData.get('ApproverUserId')) : null,
            AssigneeDepartmentId: formData.get('AssigneeDepartmentId') ? parseInt(formData.get('AssigneeDepartmentId')) : null,
            AssigneeFieldId: formData.get('AssigneeFieldId') ? parseInt(formData.get('AssigneeFieldId')) : null,
            IsMandatory: isMandatoryCheckbox ? isMandatoryCheckbox.checked : true,
            IsParallel: isParallelCheckbox ? isParallelCheckbox.checked : false,
            DueDays: formData.get('DueDays') ? parseInt(formData.get('DueDays')) : null,
            EscalationRoleId: formData.get('EscalationRoleId') ? parseInt(formData.get('EscalationRoleId')) : null,
            ConditionLogic: formData.get('ConditionLogic'),
            AutoApproveCondition: formData.get('AutoApproveCondition'),
            DependsOnStepIds: formData.get('DependsOnStepIds')
        };
    },
    
    /**
     * Display validation result in UI
     * @param {string} stepId - The step identifier
     * @param {object} result - Validation result from API
     */
    displayValidationResult(stepId, result) {
        const container = document.getElementById(`validation-${stepId}`);
        if (!container) return;
        
        const errorDiv = container.querySelector('.validation-errors');
        const successDiv = container.querySelector('.validation-success');
        const errorList = container.querySelector('.validation-error-list');
        
        // Handle warnings (show but don't block)
        const warningDiv = container.querySelector('.validation-warnings');
        const warningList = container.querySelector('.validation-warning-list');
        
        if (result.Warnings && result.Warnings.length > 0 && warningDiv && warningList) {
            warningDiv.classList.remove('d-none');
            warningList.innerHTML = result.Warnings.map(warning => `<li>${warning}</li>`).join('');
        } else if (warningDiv) {
            warningDiv.classList.add('d-none');
        }
        
        if (result.IsValid) {
            errorDiv?.classList.add('d-none');
            successDiv?.classList.remove('d-none');
            this.enableNextButton(stepId);
            this.updateStepStatus(stepId, 'valid');
        } else {
            successDiv?.classList.add('d-none');
            errorDiv?.classList.remove('d-none');
            
            if (errorList && result.Errors && result.Errors.length > 0) {
                errorList.innerHTML = result.Errors.map(error => `<li>${error}</li>`).join('');
            }
            
            this.disableNextButton(stepId);
            this.updateStepStatus(stepId, 'invalid');
        }
    },
    
    /**
     * Display validation error message
     * @param {string} stepId - The step identifier
     * @param {string} message - Error message to display
     */
    displayValidationError(stepId, message) {
        const container = document.getElementById(`validation-${stepId}`);
        if (!container) return;
        
        const errorDiv = container.querySelector('.validation-errors');
        const successDiv = container.querySelector('.validation-success');
        const errorList = container.querySelector('.validation-error-list');
        
        successDiv?.classList.add('d-none');
        errorDiv?.classList.remove('d-none');
        
        if (errorList) {
            errorList.innerHTML = `<li>${message}</li>`;
        }
        
        this.disableNextButton(stepId);
        this.updateStepStatus(stepId, 'error');
    },
    
    /**
     * Enable next button for step
     * @param {string} stepId - The step identifier
     */
    enableNextButton(stepId) {
        const nextBtn = document.querySelector(`[data-step="${stepId}"] .btn-next, .wizard-step[data-step="${stepId}"] + .wizard-navigation .btn-next`);
        if (nextBtn) {
            nextBtn.disabled = false;
            nextBtn.classList.remove('btn-secondary');
            nextBtn.classList.add('btn-primary');
        }
    },
    
    /**
     * Disable next button for step
     * @param {string} stepId - The step identifier
     */
    disableNextButton(stepId) {
        const nextBtn = document.querySelector(`[data-step="${stepId}"] .btn-next, .wizard-step[data-step="${stepId}"] + .wizard-navigation .btn-next`);
        if (nextBtn) {
            nextBtn.disabled = true;
            nextBtn.classList.remove('btn-primary');
            nextBtn.classList.add('btn-secondary');
        }
    },
    
    /**
     * Update step status indicator
     * @param {string} stepId - The step identifier
     * @param {string} status - Status: 'valid', 'invalid', 'error', 'pending'
     */
    updateStepStatus(stepId, status) {
        const stepElement = document.querySelector(`[data-step="${stepId}"]`);
        if (!stepElement) return;
        
        const statusIcon = stepElement.querySelector('.step-status-icon');
        if (!statusIcon) return;
        
        // Remove existing status classes
        statusIcon.classList.remove('text-success', 'text-danger', 'text-warning', 'text-muted');
        statusIcon.innerHTML = '';
        
        switch (status) {
            case 'valid':
                statusIcon.classList.add('text-success');
                statusIcon.innerHTML = '<i class="ri-check-line"></i>';
                break;
            case 'invalid':
                statusIcon.classList.add('text-danger');
                statusIcon.innerHTML = '<i class="ri-close-line"></i>';
                break;
            case 'error':
                statusIcon.classList.add('text-warning');
                statusIcon.innerHTML = '<i class="ri-error-warning-line"></i>';
                break;
            default:
                statusIcon.classList.add('text-muted');
                statusIcon.innerHTML = '<i class="ri-more-line"></i>';
                break;
        }
    },
    
    /**
     * Clear validation feedback for step
     * @param {string} stepId - The step identifier
     */
    clearValidationFeedback(stepId) {
        const container = document.getElementById(`validation-${stepId}`);
        if (!container) return;
        
        const errorDiv = container.querySelector('.validation-errors');
        const successDiv = container.querySelector('.validation-success');
        
        errorDiv?.classList.add('d-none');
        successDiv?.classList.add('d-none');
        
        this.updateStepStatus(stepId, 'pending');
    },
    
    /**
     * Initialize validation for all steps
     * @param {number} templateId - The template ID
     */
    initializeValidation(templateId) {
        const steps = ['target', 'action', 'assignee', 'settings'];
        
        steps.forEach(stepId => {
            // Add change listeners for step inputs
            const stepContainer = document.querySelector(`[data-step="${stepId}"]`);
            if (stepContainer) {
                const inputs = stepContainer.querySelectorAll('input, select, textarea');
                inputs.forEach(input => {
                    input.addEventListener('change', () => {
                        // Debounce validation calls
                        clearTimeout(this.validationTimeout);
                        this.validationTimeout = setTimeout(() => {
                            this.validateCurrentStep(stepId, templateId);
                        }, 500);
                    });
                });
            }
        });
        
        console.log('[WorkflowStepValidation] Initialized validation for template:', templateId);
    },
    
    /**
     * Validate before allowing step navigation
     * @param {string} currentStepId - Current step identifier
     * @param {number} templateId - Template ID
     * @returns {Promise<boolean>} - True if navigation is allowed
     */
    async validateBeforeNavigation(currentStepId, templateId) {
        const isValid = await this.validateCurrentStep(currentStepId, templateId);
        
        if (!isValid) {
            // Scroll to validation feedback
            const validationContainer = document.getElementById(`validation-${currentStepId}`);
            if (validationContainer) {
                validationContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
        
        return isValid;
    }
};

// Export for use in other scripts
window.WorkflowStepValidation = WorkflowStepValidation;
