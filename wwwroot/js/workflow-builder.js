/**
 * Workflow Builder JavaScript
 * Handles workflow panel interactions, AJAX calls, and drag-drop reordering
 */

const WorkflowBuilder = (function() {
    'use strict';

    // State
    let templateId = null;
    let currentWorkflow = null;
    let availableActions = [];
    let sections = [];
    let fields = [];
    let sortableInstance = null;

    // DOM Elements
    const elements = {
        panel: null,
        loading: null,
        emptyState: null,
        content: null,
        timeline: null,
        stepsList: null,
        noStepsMessage: null,
        stepModal: null,
        selectWorkflowModal: null
    };

    /**
     * Initialize the workflow builder
     */
    function init(tid) {
        templateId = tid;
        
        // Cache DOM elements
        cacheElements();
        
        // Bind events
        bindEvents();
        
        // Load initial data
        loadWorkflow();
        loadAvailableActions();
        loadSections();
    }

    /**
     * Cache DOM elements
     */
    function cacheElements() {
        elements.panel = document.getElementById('workflow-builder-panel');
        elements.loading = document.getElementById('workflow-loading');
        elements.emptyState = document.getElementById('workflow-empty-state');
        elements.content = document.getElementById('workflow-content');
        elements.timeline = document.getElementById('workflow-timeline');
        elements.stepsList = document.getElementById('workflow-steps-list');
        elements.noStepsMessage = document.getElementById('no-steps-message');
        elements.stepModal = new bootstrap.Modal(document.getElementById('workflow-step-modal'));
        elements.selectWorkflowModal = new bootstrap.Modal(document.getElementById('select-workflow-modal'));
    }

    /**
     * Bind event listeners
     */
    function bindEvents() {
        // Empty state buttons
        document.getElementById('select-workflow-btn')?.addEventListener('click', showSelectWorkflowModal);
        document.getElementById('create-workflow-btn')?.addEventListener('click', createNewWorkflow);
        
        // Header buttons
        document.getElementById('change-workflow-btn')?.addEventListener('click', showSelectWorkflowModal);
        document.getElementById('create-new-workflow-btn')?.addEventListener('click', createNewWorkflow);
        
        // Add step button
        document.getElementById('add-step-btn')?.addEventListener('click', () => showStepModal());
        
        // Select workflow modal
        document.getElementById('workflow-select')?.addEventListener('change', onWorkflowSelectChange);
        document.getElementById('assign-workflow-btn')?.addEventListener('click', assignWorkflow);
    }

    /**
     * Load workflow for template
     */
    async function loadWorkflow() {
        try {
            showLoading();
            
            const response = await fetch(`/api/workflows/template/${templateId}`);
            const result = await response.json();
            
            if (result.success && result.data) {
                currentWorkflow = result.data;
                renderWorkflow();
            } else {
                showEmptyState();
            }
        } catch (error) {
            console.error('Error loading workflow:', error);
            showError('Failed to load workflow');
            showEmptyState();
        }
    }

    /**
     * Load available workflow actions
     */
    async function loadAvailableActions() {
        try {
            const response = await fetch('/api/workflows/actions');
            const result = await response.json();
            
            if (result.success) {
                availableActions = result.data;
            }
        } catch (error) {
            console.error('Error loading actions:', error);
        }
    }

    /**
     * Load template sections
     */
    async function loadSections() {
        try {
            const response = await fetch(`/api/workflows/templates/${templateId}/sections`);
            const result = await response.json();
            
            if (result.success) {
                sections = result.data;
            }
        } catch (error) {
            console.error('Error loading sections:', error);
        }
    }

    /**
     * Load template fields
     */
    async function loadFields(sectionId = null) {
        try {
            let url = `/api/workflows/templates/${templateId}/fields`;
            if (sectionId) {
                url += `?sectionId=${sectionId}`;
            }
            
            const response = await fetch(url);
            const result = await response.json();
            
            if (result.success) {
                fields = result.data;
                return fields;
            }
        } catch (error) {
            console.error('Error loading fields:', error);
        }
        return [];
    }

    /**
     * Render workflow
     */
    function renderWorkflow() {
        if (!currentWorkflow) {
            showEmptyState();
            return;
        }

        // Update header
        document.getElementById('workflow-name').textContent = currentWorkflow.workflowName;
        document.getElementById('workflow-step-count').textContent = currentWorkflow.stepCount || 0;
        document.getElementById('workflow-modified-date').textContent = formatDate(currentWorkflow.modifiedDate);

        // Render timeline
        renderTimeline();

        // Render steps list
        renderStepsList();

        // Show content
        showContent();

        // Initialize sortable
        initializeSortable();
    }

    /**
     * Render visual timeline
     */
    function renderTimeline() {
        if (!currentWorkflow.steps || currentWorkflow.steps.length === 0) {
            elements.timeline.innerHTML = '<div class="workflow-timeline-empty"><i class="ri-information-line fs-20"></i><p class="mt-2">No steps defined</p></div>';
            return;
        }

        const timelineHtml = currentWorkflow.steps.map(step => {
            const actionIcon = getActionIcon(step.actionCode);
            const assigneeLabel = getAssigneeLabel(step);
            
            return `
                <div class="workflow-timeline-step">
                    <div class="timeline-step-icon" style="background-color: ${getActionColor(step.actionCode)}; border-color: ${getActionColor(step.actionCode)};">
                        <div class="timeline-step-number">${step.stepOrder}</div>
                        <div class="timeline-step-action">${step.actionCode || 'Action'}</div>
                    </div>
                    <div class="timeline-step-label">
                        ${step.stepName || 'Unnamed Step'}
                        <div class="timeline-step-assignee">${assigneeLabel}</div>
                    </div>
                </div>
            `;
        }).join('');

        elements.timeline.innerHTML = timelineHtml;
    }

    /**
     * Render steps list
     */
    function renderStepsList() {
        if (!currentWorkflow.steps || currentWorkflow.steps.length === 0) {
            elements.stepsList.innerHTML = '';
            elements.noStepsMessage.classList.remove('d-none');
            return;
        }

        elements.noStepsMessage.classList.add('d-none');

        const stepsHtml = currentWorkflow.steps.map(step => {
            const actionIcon = getActionIcon(step.actionCode);
            const actionColor = getActionColor(step.actionCode);
            const assigneeLabel = getAssigneeLabel(step);
            const targetLabel = getTargetLabel(step);
            
            return `
                <div class="workflow-step-card" data-step-id="${step.stepId}" data-step-order="${step.stepOrder}">
                    <div class="step-card-header">
                        <i class="ri-draggable step-drag-handle"></i>
                        <div class="step-order-badge" style="background-color: ${actionColor};">${step.stepOrder}</div>
                        <h6 class="step-card-title">${step.stepName || 'Unnamed Step'}</h6>
                        <div class="step-card-actions">
                            <button type="button" class="btn btn-sm btn-soft-primary" onclick="WorkflowBuilder.editStep(${step.stepId})">
                                <i class="ri-edit-line"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-soft-danger" onclick="WorkflowBuilder.deleteStep(${step.stepId})">
                                <i class="ri-delete-bin-line"></i>
                            </button>
                        </div>
                    </div>
                    <div class="step-card-body">
                        <div class="step-detail-row">
                            <div class="step-detail-item">
                                <span class="action-icon action-${step.actionCode?.toLowerCase() || 'fill'}">
                                    <i class="${actionIcon}"></i>
                                </span>
                                <span><span class="step-detail-label">Action:</span> <span class="step-detail-value">${step.actionName || 'Unknown'}</span></span>
                            </div>
                            <div class="step-detail-item">
                                <i class="ri-user-star-line"></i>
                                <span><span class="step-detail-label">Assignee:</span> <span class="step-detail-value">${assigneeLabel}</span></span>
                            </div>
                            <div class="step-detail-item">
                                <i class="ri-focus-3-line"></i>
                                <span><span class="step-detail-label">Target:</span> ${targetLabel}</span>
                            </div>
                            ${step.dueDays ? `
                            <div class="step-detail-item">
                                <i class="ri-time-line"></i>
                                <span><span class="step-detail-label">Due:</span> <span class="step-detail-value">${step.dueDays} days</span></span>
                            </div>
                            ` : ''}
                            ${step.isMandatory ? `
                            <div class="step-detail-item">
                                <i class="ri-checkbox-circle-line"></i>
                                <span class="step-detail-value">Mandatory</span>
                            </div>
                            ` : ''}
                            ${step.requiresSignature ? `
                            <div class="step-detail-item">
                                <i class="ri-pen-nib-line"></i>
                                <span class="step-detail-value">Requires Signature</span>
                            </div>
                            ` : ''}
                        </div>
                    </div>
                </div>
            `;
        }).join('');

        elements.stepsList.innerHTML = stepsHtml;
    }

    /**
     * Initialize sortable for drag-drop reordering
     */
    function initializeSortable() {
        if (sortableInstance) {
            sortableInstance.destroy();
        }

        if (elements.stepsList && currentWorkflow?.steps?.length > 0) {
            sortableInstance = Sortable.create(elements.stepsList, {
                animation: 150,
                handle: '.step-drag-handle',
                ghostClass: 'sortable-ghost',
                dragClass: 'sortable-drag',
                onEnd: handleStepReorder
            });
        }
    }

    /**
     * Handle step reorder
     */
    async function handleStepReorder(evt) {
        const stepCards = elements.stepsList.querySelectorAll('.workflow-step-card');
        const newOrders = Array.from(stepCards).map((card, index) => ({
            stepId: parseInt(card.dataset.stepId),
            newOrder: index + 1
        }));

        try {
            const response = await fetch(`/api/workflows/${currentWorkflow.workflowId}/steps/reorder`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newOrders)
            });

            const result = await response.json();

            if (result.success) {
                showSuccess('Steps reordered successfully');
                await loadWorkflow();
            } else {
                showError('Failed to reorder steps');
                await loadWorkflow(); // Reload to reset order
            }
        } catch (error) {
            console.error('Error reordering steps:', error);
            showError('Failed to reorder steps');
            await loadWorkflow();
        }
    }

    /**
     * Show select workflow modal
     */
    async function showSelectWorkflowModal() {
        try {
            const response = await fetch('/api/workflows/lookup');
            const result = await response.json();

            if (result.success) {
                const select = document.getElementById('workflow-select');
                select.innerHTML = '<option value="">-- Select a workflow --</option>' +
                    result.data.map(w => `<option value="${w.value}">${w.text} (${w.stepCount} steps)</option>`).join('');
                
                elements.selectWorkflowModal.show();
            }
        } catch (error) {
            console.error('Error loading workflows:', error);
            showError('Failed to load workflows');
        }
    }

    /**
     * Handle workflow selection change
     */
    async function onWorkflowSelectChange(e) {
        const workflowId = e.target.value;
        const assignBtn = document.getElementById('assign-workflow-btn');
        const preview = document.getElementById('selected-workflow-preview');

        if (!workflowId) {
            assignBtn.disabled = true;
            preview.classList.add('d-none');
            return;
        }

        try {
            const response = await fetch(`/api/workflows/${workflowId}`);
            const result = await response.json();

            if (result.success) {
                document.getElementById('preview-workflow-name').textContent = result.data.workflowName;
                document.getElementById('preview-workflow-steps').textContent = result.data.stepCount;
                document.getElementById('preview-workflow-description').textContent = result.data.description || 'No description';
                preview.classList.remove('d-none');
                assignBtn.disabled = false;
            }
        } catch (error) {
            console.error('Error loading workflow details:', error);
        }
    }

    /**
     * Assign workflow to template
     */
    async function assignWorkflow() {
        const workflowId = document.getElementById('workflow-select').value;
        
        if (!workflowId) return;

        try {
            // Update template with workflow (you'll need to create this endpoint)
            const response = await fetch(`/api/form-templates/${templateId}/workflow`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ workflowId: parseInt(workflowId) })
            });

            const result = await response.json();

            if (result.success) {
                showSuccess('Workflow assigned successfully');
                elements.selectWorkflowModal.hide();
                await loadWorkflow();
            } else {
                showError('Failed to assign workflow');
            }
        } catch (error) {
            console.error('Error assigning workflow:', error);
            showError('Failed to assign workflow');
        }
    }

    /**
     * Create new workflow
     */
    function createNewWorkflow() {
        // TODO: Implement create workflow wizard
        showInfo('Create workflow feature coming soon');
    }

    /**
     * Show step modal for add/edit
     */
    function showStepModal(stepId = null) {
        // TODO: Load step form wizard
        showInfo('Step form wizard coming soon');
        elements.stepModal.show();
    }

    /**
     * Edit step
     */
    function editStep(stepId) {
        showStepModal(stepId);
    }

    /**
     * Delete step
     */
    async function deleteStep(stepId) {
        if (!confirm('Are you sure you want to delete this step?')) {
            return;
        }

        try {
            const response = await fetch(`/api/workflows/steps/${stepId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (result.success) {
                showSuccess('Step deleted successfully');
                await loadWorkflow();
            } else {
                showError(result.message || 'Failed to delete step');
            }
        } catch (error) {
            console.error('Error deleting step:', error);
            showError('Failed to delete step');
        }
    }

    /**
     * Helper: Get action icon
     */
    function getActionIcon(actionCode) {
        const icons = {
            'Fill': 'ri-pencil-line',
            'Sign': 'ri-pen-nib-line',
            'Approve': 'ri-check-line',
            'Reject': 'ri-close-line',
            'Review': 'ri-eye-line',
            'Verify': 'ri-shield-check-line'
        };
        return icons[actionCode] || 'ri-flashlight-line';
    }

    /**
     * Helper: Get action color
     */
    function getActionColor(actionCode) {
        const colors = {
            'Fill': '#0d6efd',
            'Sign': '#6f42c1',
            'Approve': '#198754',
            'Reject': '#dc3545',
            'Review': '#ffc107',
            'Verify': '#0dcaf0'
        };
        return colors[actionCode] || '#6c757d';
    }

    /**
     * Helper: Get assignee label
     */
    function getAssigneeLabel(step) {
        if (step.assigneeType === 'Submitter') return 'Submitter';
        if (step.assigneeType === 'PreviousActor') return 'Previous Actor';
        if (step.assigneeType === 'Role') return `Role: ${step.approverRoleName || 'Unknown'}`;
        if (step.assigneeType === 'User') return `User: ${step.approverUserName || 'Unknown'}`;
        if (step.assigneeType === 'Department') return `Dept: ${step.assigneeDepartmentName || 'Unknown'}`;
        if (step.assigneeType === 'FieldValue') return 'Field Value';
        return 'Unknown';
    }

    /**
     * Helper: Get target label
     */
    function getTargetLabel(step) {
        const targetType = step.targetType || 'Submission';
        const badgeClass = `target-badge target-${targetType.toLowerCase()}`;
        
        if (targetType === 'Submission') {
            return `<span class="${badgeClass}"><i class="ri-file-list-line"></i> Entire Form</span>`;
        } else if (targetType === 'Section') {
            return `<span class="${badgeClass}"><i class="ri-layout-line"></i> Section</span>`;
        } else if (targetType === 'Field') {
            return `<span class="${badgeClass}"><i class="ri-input-field"></i> Field</span>`;
        }
        return `<span class="${badgeClass}">All</span>`;
    }

    /**
     * Helper: Format date
     */
    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
    }

    /**
     * UI State functions
     */
    function showLoading() {
        elements.loading?.classList.remove('d-none');
        elements.emptyState?.classList.add('d-none');
        elements.content?.classList.add('d-none');
    }

    function showEmptyState() {
        elements.loading?.classList.add('d-none');
        elements.emptyState?.classList.remove('d-none');
        elements.content?.classList.add('d-none');
    }

    function showContent() {
        elements.loading?.classList.add('d-none');
        elements.emptyState?.classList.add('d-none');
        elements.content?.classList.remove('d-none');
    }

    /**
     * Notification functions
     */
    function showSuccess(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ icon: 'success', title: 'Success', text: message, timer: 2000, showConfirmButton: false });
        } else {
            alert(message);
        }
    }

    function showError(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ icon: 'error', title: 'Error', text: message });
        } else {
            alert(message);
        }
    }

    function showInfo(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ icon: 'info', title: 'Info', text: message });
        } else {
            alert(message);
        }
    }

    // Public API
    return {
        init,
        editStep,
        deleteStep,
        loadWorkflow,
        getAvailableActions: () => availableActions,
        getSections: () => sections,
        getFields: () => fields,
        loadFields
    };
})();

// Make it globally accessible
window.WorkflowBuilder = WorkflowBuilder;
