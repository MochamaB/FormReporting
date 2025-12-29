/**
 * Workflow Wizard Step Initialization
 * Contains all step-specific logic extracted from partials
 */

const WorkflowWizardSteps = {
    
    initializeTargetStep() {
        console.log('[WorkflowWizard] Initializing Target Step');
        
        const sectionPicker = document.getElementById('section-picker');
        const fieldPicker = document.getElementById('field-picker');
        const targetInfo = document.getElementById('target-info');
        const targetInfoText = document.getElementById('target-info-text');
        const targetTypeInput = document.getElementById('target-type');
        const targetIdInput = document.getElementById('target-id');

        if (!targetTypeInput) return; // Step not active

        const targetDescriptions = {
            'Submission': 'This step will apply to the entire form submission. The assignee will have access to all sections and fields.',
            'Section': 'This step will apply only to the selected section. The assignee will only be able to act on fields within this section.',
            'Field': 'This step will apply only to the selected field. The assignee will only be able to act on this specific field.'
        };

        // Auto-select "Entire Submission" by default
        setTimeout(() => {
            const submissionCard = document.querySelector('.target-type-card[data-target-type="Submission"]');
            if (submissionCard && !submissionCard.classList.contains('active')) {
                submissionCard.click();
            }
        }, 100);

        // Handle target type card clicks
        document.querySelectorAll('.target-type-card').forEach(card => {
            card.addEventListener('click', function() {
                const targetType = this.dataset.targetType;

                // Remove active class from all
                document.querySelectorAll('.target-type-card').forEach(c => c.classList.remove('active'));
                this.classList.add('active');
                targetTypeInput.value = targetType;

                // Hide all pickers
                sectionPicker?.classList.add('d-none');
                fieldPicker?.classList.add('d-none');

                // Clear required and values
                document.getElementById('target-section')?.removeAttribute('required');
                document.getElementById('target-field')?.removeAttribute('required');
                targetIdInput.value = '';

                // Show appropriate picker
                if (targetType === 'Section') {
                    sectionPicker?.classList.remove('d-none');
                    document.getElementById('target-section')?.setAttribute('required', 'required');
                    WorkflowWizardSteps.loadSectionsForTarget();
                } else if (targetType === 'Field') {
                    fieldPicker?.classList.remove('d-none');
                    document.getElementById('target-field')?.setAttribute('required', 'required');
                    WorkflowWizardSteps.loadSectionsForFieldFilter();
                    WorkflowWizardSteps.loadFieldsForTarget();
                } else if (targetType === 'Submission') {
                    WorkflowWizard.setSelectedTarget('Submission', null, 'Entire Submission');
                }

                // Show info
                if (targetDescriptions[targetType]) {
                    targetInfoText.textContent = targetDescriptions[targetType];
                    targetInfo?.classList.remove('d-none');
                }
            });
        });

        // Handle section selection for target
        document.getElementById('target-section')?.addEventListener('change', function() {
            targetIdInput.value = this.value;
            if (this.value) {
                const selectedOption = this.options[this.selectedIndex];
                const sectionName = selectedOption.text.split(' (')[0];
                WorkflowWizard.setSelectedTarget('Section', this.value, sectionName);
            }
        });

        // Handle field selection for target
        document.getElementById('target-field')?.addEventListener('change', function() {
            targetIdInput.value = this.value;
            if (this.value) {
                const selectedOption = this.options[this.selectedIndex];
                const fieldFullName = selectedOption.text;
                const fieldName = fieldFullName.split(' - ')[1] || fieldFullName;
                WorkflowWizard.setSelectedTarget('Field', this.value, fieldName);
            }
        });

        // Handle section filter for fields
        document.getElementById('target-section-for-field')?.addEventListener('change', function() {
            const sectionId = this.value;
            WorkflowWizardSteps.loadFieldsForTarget(sectionId || null);
        });
    },

    async loadSectionsForTarget() {
        try {
            const sections = await WorkflowWizard.loadSections();
            const select = document.getElementById('target-section');
            if (select && sections && sections.length > 0) {
                select.innerHTML = '<option value="">-- Select a section --</option>' +
                    sections.map(s => `<option value="${s.sectionId}">${s.sectionName} (${s.fieldCount} fields)</option>`).join('');
            } else if (select) {
                select.innerHTML = '<option value="">No sections available</option>';
            }
        } catch (error) {
            console.error('Error loading sections for target:', error);
        }
    },

    async loadSectionsForFieldFilter() {
        try {
            const sections = await WorkflowWizard.loadSections();
            const select = document.getElementById('target-section-for-field');
            if (select && sections && sections.length > 0) {
                select.innerHTML = '<option value="">-- All sections --</option>' +
                    sections.map(s => `<option value="${s.sectionId}">${s.sectionName}</option>`).join('');
            }
        } catch (error) {
            console.error('Error loading sections for field filter:', error);
        }
    },

    async loadFieldsForTarget(sectionId = null) {
        try {
            const fields = await WorkflowWizard.loadFields(sectionId);
            const select = document.getElementById('target-field');
            if (select && fields && fields.length > 0) {
                select.innerHTML = '<option value="">-- Select a field --</option>' +
                    fields.map(f => `<option value="${f.itemId}">${f.sectionName} - ${f.itemName}</option>`).join('');
            } else if (select) {
                select.innerHTML = '<option value="">No fields available</option>';
            }
        } catch (error) {
            console.error('Error loading fields for target:', error);
        }
    },

    initializeActionStep() {
        console.log('[WorkflowWizard] Initializing Action Step');
        
        const container = document.getElementById('action-types-container');
        const detailsDiv = document.getElementById('action-details');
        const actionIdInput = document.getElementById('selected-action-id');
        
        if (!container) return; // Step not active

        WorkflowWizardSteps.loadActionsForStep();
    },

    async loadActionsForStep() {
        const container = document.getElementById('action-types-container');
        try {
            const allActions = await WorkflowWizard.loadAvailableActions();
            const actions = WorkflowWizard.filterActionsByMode(allActions);

            if (actions && actions.length > 0) {
                WorkflowWizardSteps.renderActions(actions);
            } else {
                throw new Error('No actions available');
            }
        } catch (error) {
            console.error('Error loading actions:', error);
            if (container) {
                container.innerHTML = '<div class="col-12 text-center text-danger">Failed to load actions</div>';
            }
        }
    },

    renderActions(actions) {
        const container = document.getElementById('action-types-container');
        const detailsDiv = document.getElementById('action-details');
        const actionIdInput = document.getElementById('selected-action-id');
        
        const actionIcons = {
            'Fill': 'ri-pencil-line',
            'Sign': 'ri-pen-nib-line',
            'Approve': 'ri-check-line',
            'Reject': 'ri-close-line',
            'Review': 'ri-eye-line',
            'Verify': 'ri-shield-check-line'
        };
        
        const actionColors = {
            'Fill': '#0d6efd',
            'Sign': '#6f42c1',
            'Approve': '#198754',
            'Reject': '#dc3545',
            'Review': '#ffc107',
            'Verify': '#0dcaf0'
        };
        
        container.innerHTML = actions.map(action => `
            <div class="col-md-4">
                <div class="action-card" data-action-id="${action.actionId}" 
                     data-action-code="${action.actionCode}"
                     data-description="${action.description || ''}"
                     data-requires-signature="${action.requiresSignature}"
                     data-requires-comment="${action.requiresComment}">
                    <div class="action-card-icon" style="background-color: ${actionColors[action.actionCode]}20; color: ${actionColors[action.actionCode]};">
                        <i class="${actionIcons[action.actionCode] || 'ri-flashlight-line'}"></i>
                    </div>
                    <h6 class="action-card-title">${action.actionName}</h6>
                    <p class="action-card-description">${action.description || 'No description'}</p>
                </div>
            </div>
        `).join('');
        
        // Add click handlers
        document.querySelectorAll('.action-card').forEach(card => {
            card.addEventListener('click', function() {
                document.querySelectorAll('.action-card').forEach(c => c.classList.remove('active'));
                this.classList.add('active');
                actionIdInput.value = this.dataset.actionId;

                // Show details
                document.getElementById('action-description').textContent = this.dataset.description || 'No description';

                const sigInfo = document.getElementById('requires-signature-info');
                const commentInfo = document.getElementById('requires-comment-info');

                if (this.dataset.requiresSignature === 'true') {
                    sigInfo?.classList.remove('d-none');
                } else {
                    sigInfo?.classList.add('d-none');
                }

                if (this.dataset.requiresComment === 'true') {
                    commentInfo?.classList.remove('d-none');
                } else {
                    commentInfo?.classList.add('d-none');
                }

                detailsDiv?.classList.remove('d-none');

                // Auto-generate step name
                const actionCode = this.dataset.actionCode;
                const generatedName = WorkflowWizard.generateStepName(actionCode);
                const stepNameInput = document.getElementById('step-name');

                if (stepNameInput && generatedName) {
                    stepNameInput.value = generatedName;
                    stepNameInput.setAttribute('placeholder', generatedName);
                    console.log('[Action Step] Auto-generated step name:', generatedName);
                }
            });
        });
    },

    initializeAssigneeStep() {
        console.log('[WorkflowWizard] Initializing Assignee Step');
        
        const assigneeTypeInput = document.getElementById('assignee-type');
        if (!assigneeTypeInput) return; // Step not active

        const rolePicker = document.getElementById('role-picker');
        const userPicker = document.getElementById('user-picker');
        const departmentPicker = document.getElementById('department-picker');
        const fieldPicker = document.getElementById('field-picker');
        const assigneeInfo = document.getElementById('assignee-info');
        const assigneeInfoText = document.getElementById('assignee-info-text');

        const assigneeDescriptions = {
            'Submitter': 'The person who created this submission will be assigned to complete this step.',
            'PreviousActor': 'The person who completed the previous workflow step will be assigned to this step.',
            'Role': 'Any user with the selected role will be able to complete this step.',
            'User': 'Only the specific selected user will be able to complete this step.',
            'Department': 'Any user in the selected department will be able to complete this step.',
            'FieldValue': 'The user ID stored in the selected field will be assigned to this step.'
        };

        // Apply mode restrictions
        WorkflowWizardSteps.applyAssigneeModeRestrictions();

        // Handle assignee card clicks
        document.querySelectorAll('.assignee-card').forEach(card => {
            card.addEventListener('click', function() {
                if (this.classList.contains('disabled')) {
                    console.log('[Assignee Step] Card is disabled, ignoring click');
                    return;
                }

                const selectedType = this.dataset.assigneeType;
                const requiresPicker = this.dataset.requiresPicker === 'true';
                const pickerType = this.dataset.pickerType;

                document.querySelectorAll('.assignee-card').forEach(c => c.classList.remove('active'));
                this.classList.add('active');
                assigneeTypeInput.value = selectedType;

                // Hide all pickers
                rolePicker?.classList.add('d-none');
                userPicker?.classList.add('d-none');
                departmentPicker?.classList.add('d-none');
                fieldPicker?.classList.add('d-none');

                // Clear required attributes
                document.getElementById('approver-role')?.removeAttribute('required');
                document.getElementById('approver-user')?.removeAttribute('required');
                document.getElementById('assignee-department')?.removeAttribute('required');
                document.getElementById('assignee-field')?.removeAttribute('required');

                // Show appropriate picker
                if (requiresPicker) {
                    if (pickerType === 'role') {
                        rolePicker?.classList.remove('d-none');
                        document.getElementById('approver-role')?.setAttribute('required', 'required');
                        WorkflowWizardSteps.loadRolesForAssignee();
                    } else if (pickerType === 'user') {
                        userPicker?.classList.remove('d-none');
                        document.getElementById('approver-user')?.setAttribute('required', 'required');
                        WorkflowWizardSteps.loadUsersForAssignee();
                    } else if (pickerType === 'department') {
                        departmentPicker?.classList.remove('d-none');
                        document.getElementById('assignee-department')?.setAttribute('required', 'required');
                        WorkflowWizardSteps.loadDepartmentsForAssignee();
                    } else if (pickerType === 'field') {
                        fieldPicker?.classList.remove('d-none');
                        document.getElementById('assignee-field')?.setAttribute('required', 'required');
                        WorkflowWizardSteps.loadFieldsForAssignee();
                    }
                }

                // Show info
                if (assigneeDescriptions[selectedType]) {
                    assigneeInfoText.textContent = assigneeDescriptions[selectedType];
                    assigneeInfo?.classList.remove('d-none');
                } else {
                    assigneeInfo?.classList.add('d-none');
                }
            });
        });
    },

    async loadRolesForAssignee() {
        try {
            const response = await fetch('/api/workflows/roles');
            const result = await response.json();
            const select = document.getElementById('approver-role');
            if (result.success && select) {
                select.innerHTML = '<option value="">-- Select a role --</option>' +
                    result.data.map(r => `<option value="${r.roleId}">${r.roleName}${r.scopeLevel ? ' (' + r.scopeLevel + ')' : ''}</option>`).join('');
            }
        } catch (error) {
            console.error('Error loading roles:', error);
            const select = document.getElementById('approver-role');
            if (select) select.innerHTML = '<option value="">Error loading roles</option>';
        }
    },

    async loadUsersForAssignee() {
        try {
            const response = await fetch('/api/workflows/users');
            const result = await response.json();
            const select = document.getElementById('approver-user');
            if (result.success && select) {
                select.innerHTML = '<option value="">-- Select a user --</option>' +
                    result.data.map(u => `<option value="${u.userId}">${u.fullName}${u.departmentName ? ' - ' + u.departmentName : ''}</option>`).join('');
            }
        } catch (error) {
            console.error('Error loading users:', error);
            const select = document.getElementById('approver-user');
            if (select) select.innerHTML = '<option value="">Error loading users</option>';
        }
    },

    async loadDepartmentsForAssignee() {
        try {
            const response = await fetch('/api/workflows/departments');
            const result = await response.json();
            const select = document.getElementById('assignee-department');
            if (result.success && select) {
                select.innerHTML = '<option value="">-- Select a department --</option>' +
                    result.data.map(d => `<option value="${d.departmentId}">${d.departmentName}${d.userCount ? ' (' + d.userCount + ' users)' : ''}</option>`).join('');
            }
        } catch (error) {
            console.error('Error loading departments:', error);
            const select = document.getElementById('assignee-department');
            if (select) select.innerHTML = '<option value="">Error loading departments</option>';
        }
    },

    async loadFieldsForAssignee() {
        try {
            const fields = await WorkflowWizard.loadFields();
            const select = document.getElementById('assignee-field');
            if (fields && fields.length > 0 && select) {
                select.innerHTML = '<option value="">-- Select a field --</option>' +
                    fields.map(f => `<option value="${f.itemId}">${f.sectionName} - ${f.itemName}</option>`).join('');
            } else if (select) {
                select.innerHTML = '<option value="">No fields available</option>';
            }
        } catch (error) {
            console.error('Error loading fields for assignee:', error);
            const select = document.getElementById('assignee-field');
            if (select) select.innerHTML = '<option value="">Error loading fields</option>';
        }
    },

    applyAssigneeModeRestrictions() {
        if (typeof WorkflowWizard === 'undefined' || !WorkflowWizard.isIndividualMode) {
            console.log('[Assignee Step] WorkflowWizard not yet initialized, will apply restrictions later');
            return;
        }

        const isIndividual = WorkflowWizard.isIndividualMode();
        console.log('[Assignee Step] Applying mode restrictions. Individual mode:', isIndividual);

        document.querySelectorAll('.assignee-card[data-mode-restriction]').forEach(card => {
            const restriction = card.dataset.modeRestriction;
            if (restriction === 'individual' && isIndividual) {
                card.classList.add('disabled');
                console.log('[Assignee Step] Disabled card:', card.dataset.assigneeType);
            } else {
                card.classList.remove('disabled');
            }
        });
    },

    initializeSettingsStep() {
        console.log('[WorkflowWizard] Initializing Settings Step');
        
        const stepOrderInput = document.getElementById('step-order');
        if (!stepOrderInput) return; // Step not active

        // Load escalation roles
        WorkflowWizardSteps.loadEscalationRoles();
        
        // Update summary on input changes
        document.getElementById('step-order')?.addEventListener('input', WorkflowWizardSteps.updateSummary);
        document.getElementById('due-days')?.addEventListener('input', WorkflowWizardSteps.updateSummary);
        document.getElementById('is-mandatory')?.addEventListener('change', WorkflowWizardSteps.updateSummary);
        document.getElementById('is-parallel')?.addEventListener('change', WorkflowWizardSteps.updateSummary);
        
        // Initial summary update
        WorkflowWizardSteps.updateSummary();
    },

    async loadEscalationRoles() {
        try {
            const response = await fetch('/api/workflows/roles');
            if (!response.ok) {
                console.warn('Failed to load escalation roles:', response.status);
                return;
            }
            const result = await response.json();
            const select = document.getElementById('escalation-role');
            
            if (result.success && select) {
                select.innerHTML = '<option value="">-- No escalation --</option>' +
                    result.data.map(r => `<option value="${r.roleId}">${r.roleName}</option>`).join('');
            }
        } catch (error) {
            console.error('Error loading escalation roles:', error);
        }
    },

    updateSummary() {
        const order = document.getElementById('step-order')?.value || '1';
        const dueDays = document.getElementById('due-days')?.value;
        const isMandatory = document.getElementById('is-mandatory')?.checked;
        const isParallel = document.getElementById('is-parallel')?.checked;
        
        document.getElementById('summary-order').textContent = order;
        document.getElementById('summary-due').textContent = dueDays ? `${dueDays} days` : 'No deadline';
        document.getElementById('summary-mandatory').textContent = isMandatory ? 'Yes' : 'No';
        document.getElementById('summary-parallel').textContent = isParallel ? 'Yes' : 'No';
    },

    initializeAllSteps() {
        console.log('[WorkflowWizard] Initializing all steps');
        this.initializeTargetStep();
        this.initializeActionStep();
        this.initializeAssigneeStep();
        this.initializeSettingsStep();
    }
};

// Expose globally for external calls
window.applyAssigneeModeRestrictions = WorkflowWizardSteps.applyAssigneeModeRestrictions;
window.WorkflowWizardSteps = WorkflowWizardSteps;
