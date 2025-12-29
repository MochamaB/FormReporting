/**
 * Template Details Page JavaScript
 * Handles tab interactions, AJAX operations, and UI behaviors
 */

(function () {
    'use strict';

    // ============================================================================
    // CONFIGURATION
    // ============================================================================

    const CONFIG = {
        apiBaseUrl: '/api',
        selectors: {
            tabsContainer: '#templateDetailsTabs',
            assignmentPanel: '#assignments-panel',
            workflowPanel: '#workflow-panel',
            assignmentTable: '#assignmentsTable',
            assignmentTableBody: '#assignmentsTableBody',
            workflowStepsList: '#workflowStepsList'
        },
        endpoints: {
            assignments: '/api/assignments',
            workflows: '/api/workflows',
            workflowEngine: '/api/workflow-engine'
        },
        pageSize: 10
    };

    // ============================================================================
    // STATE
    // ============================================================================

    let state = {
        templateId: null,
        activeTab: 'overview',
        // Workflow
        workflow: null,
        workflowLoaded: false
    };

    // ============================================================================
    // INITIALIZATION
    // ============================================================================

    function init() {
        // Get template ID from page
        const templateIdElement = document.getElementById('templateId');
        if (templateIdElement) {
            state.templateId = parseInt(templateIdElement.value, 10);
        }

        // Initialize tab events
        initTabEvents();

        // Initialize panel-specific handlers
        initAssignmentPanel();
        initWorkflowPanel();
        initStructurePanel();

        console.log('[TemplateDetails] Initialized for template:', state.templateId);
    }

    // ============================================================================
    // TAB EVENTS
    // ============================================================================

    function initTabEvents() {
        const tabsContainer = document.querySelector(CONFIG.selectors.tabsContainer);
        if (!tabsContainer) return;

        // Listen for tab changes
        tabsContainer.addEventListener('shown.bs.tab', function (event) {
            const tabId = event.target.getAttribute('href')?.replace('#', '') || 
                          event.target.getAttribute('data-bs-target')?.replace('#', '');
            
            if (tabId) {
                state.activeTab = tabId;
                onTabChanged(tabId);
            }
        });
    }

    function onTabChanged(tabId) {
        console.log('[TemplateDetails] Tab changed to:', tabId);

        switch (tabId) {
            case 'assignments':
                loadAssignments();
                break;
            case 'workflow':
                // Workflow panel is now server-side rendered, no client-side loading needed
                console.log('[TemplateDetails] Workflow panel loaded via server-side rendering');
                break;
            case 'submissions':
                loadSubmissions();
                break;
            case 'metrics':
                loadMetrics();
                break;
        }
    }

    // ============================================================================
    // ASSIGNMENT PANEL
    // ============================================================================
    // Note: The DataTable component with AJAX mode handles all the loading,
    // filtering, pagination, and rendering. This section only contains
    // action handlers that are called from the row action dropdown.

    function initAssignmentPanel() {
        const panel = document.querySelector(CONFIG.selectors.assignmentPanel);
        if (!panel) return;

        // DataTable component handles everything automatically via AJAX mode
        // We just need to ensure the template ID is available for actions
        console.log('[TemplateDetails] Assignment panel initialized - DataTable handles AJAX loading');
    }

    function loadAssignments() {
        // DataTable component handles this automatically
        // This function is kept for backward compatibility
        // To reload, use: window.reloadTable_assignmentsTable()
        if (typeof window.reloadTable_assignmentsTable === 'function') {
            window.reloadTable_assignmentsTable();
        }
    }

    // Placeholder to prevent errors - these were removed since DataTable handles them
    function updateBulkActionsState() {
        // Handled by DataTable component
    }

    function renderAssignmentsTable() {
        // Handled by DataTable component
    }

    function renderPagination() {
        // Handled by DataTable component
    }

    // ============================================================================
    // WORKFLOW PANEL
    // ============================================================================

    function initWorkflowPanel() {
        // Initialize sortable for workflow steps (if Sortable.js is available)
        const stepsList = document.querySelector(CONFIG.selectors.workflowStepsList);
        if (stepsList && typeof Sortable !== 'undefined') {
            new Sortable(stepsList, {
                handle: '.drag-handle',
                animation: 150,
                onEnd: function (evt) {
                    reorderWorkflowSteps();
                }
            });
        }

        // Add step button
        const addStepBtn = document.getElementById('btnAddWorkflowStep');
        if (addStepBtn) {
            addStepBtn.addEventListener('click', function () {
                openAddStepModal();
            });
        }
    }

    function loadWorkflow() {
        if (!state.templateId) return;

        const panel = document.querySelector(CONFIG.selectors.workflowPanel);
        if (!panel) return;

        // Get workflow ID from page data
        const workflowIdElement = document.getElementById('workflowId');
        const workflowId = workflowIdElement ? parseInt(workflowIdElement.value, 10) : null;

        if (!workflowId) {
            // No workflow assigned - show empty state
            renderWorkflowEmptyState();
            return;
        }

        showPanelLoading(panel);

        fetch(`${CONFIG.endpoints.workflows}/${workflowId}`)
            .then(response => response.json())
            .then(data => {
                state.workflow = data;
                renderWorkflow(data);
            })
            .catch(error => {
                console.error('[TemplateDetails] Error loading workflow:', error);
                showPanelError(panel, 'Failed to load workflow');
            });
    }

    function renderWorkflow(workflow) {
        // Render timeline
        renderWorkflowTimeline(workflow.steps || []);

        // Render steps list
        renderWorkflowStepsList(workflow.steps || []);
    }

    function renderWorkflowTimeline(steps) {
        const timeline = document.getElementById('workflowTimeline');
        if (!timeline) return;

        if (!steps || steps.length === 0) {
            timeline.innerHTML = '<p class="text-muted text-center">No steps defined</p>';
            return;
        }

        timeline.innerHTML = steps.map((step, index) => `
            <div class="workflow-step-node">
                <div class="workflow-step-icon">
                    <i class="${step.actionIcon || 'ri-checkbox-circle-line'}"></i>
                </div>
                <div class="workflow-step-label">${escapeHtml(step.stepName)}</div>
                <div class="workflow-step-assignee">${escapeHtml(step.assigneeDisplay || '')}</div>
            </div>
        `).join('');
    }

    function renderWorkflowStepsList(steps) {
        const stepsList = document.querySelector(CONFIG.selectors.workflowStepsList);
        if (!stepsList) return;

        if (!steps || steps.length === 0) {
            stepsList.innerHTML = '<p class="text-muted text-center py-3">No steps defined. Add steps to create a workflow.</p>';
            return;
        }

        stepsList.innerHTML = steps.map((step, index) => `
            <li class="workflow-step-card" data-step-id="${step.stepId}">
                <div class="drag-handle">
                    <i class="ri-draggable"></i>
                </div>
                <div class="step-number">${index + 1}</div>
                <div class="step-content">
                    <div class="step-title">${escapeHtml(step.stepName)}</div>
                    <div class="step-meta">
                        <span class="step-meta-item"><i class="ri-flashlight-line"></i> ${escapeHtml(step.actionName)}</span>
                        <span class="step-meta-item"><i class="ri-user-line"></i> ${escapeHtml(step.assigneeDisplay)}</span>
                        ${step.dueDays ? `<span class="step-meta-item"><i class="ri-time-line"></i> ${step.dueDays} days</span>` : ''}
                        ${step.isMandatory ? '<span class="step-meta-item text-danger"><i class="ri-asterisk"></i> Required</span>' : ''}
                    </div>
                </div>
                <div class="step-actions">
                    <button class="btn btn-soft-primary btn-sm" onclick="TemplateDetails.editStep(${step.stepId})" title="Edit">
                        <i class="ri-pencil-line"></i>
                    </button>
                    <button class="btn btn-soft-danger btn-sm" onclick="TemplateDetails.deleteStep(${step.stepId})" title="Delete">
                        <i class="ri-delete-bin-line"></i>
                    </button>
                </div>
            </li>
        `).join('');
    }

    function renderWorkflowEmptyState() {
        const panel = document.querySelector(CONFIG.selectors.workflowPanel);
        if (!panel) return;

        const emptyStateHtml = `
            <div class="empty-state">
                <div class="empty-icon"><i class="ri-flow-chart"></i></div>
                <div class="empty-title">No workflow configured</div>
                <div class="empty-description">This template has no approval workflow. Submissions will be saved directly without approval.</div>
                <div class="d-flex gap-2 justify-content-center">
                    <button class="btn btn-outline-primary" onclick="TemplateDetails.selectExistingWorkflow()">
                        <i class="ri-list-check me-1"></i> Select Existing Workflow
                    </button>
                    <button class="btn btn-primary" onclick="TemplateDetails.createNewWorkflow()">
                        <i class="ri-add-line me-1"></i> Create New Workflowss
                    </button>
                </div>
            </div>
        `;

        // Find the workflow content area and set empty state
        const workflowContent = panel.querySelector('.workflow-content') || panel;
        workflowContent.innerHTML = emptyStateHtml;
    }

    function reorderWorkflowSteps() {
        const stepsList = document.querySelector(CONFIG.selectors.workflowStepsList);
        if (!stepsList || !state.workflow) return;

        const stepIds = Array.from(stepsList.querySelectorAll('.workflow-step-card'))
            .map(card => parseInt(card.dataset.stepId, 10));

        fetch(`${CONFIG.endpoints.workflows}/${state.workflow.workflowId}/steps/reorder`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ stepIds })
        })
            .then(response => {
                if (!response.ok) throw new Error('Failed to reorder steps');
                console.log('[TemplateDetails] Steps reordered successfully');
            })
            .catch(error => {
                console.error('[TemplateDetails] Error reordering steps:', error);
                // Reload to restore original order
                loadWorkflow();
            });
    }

    // ============================================================================
    // STRUCTURE PANEL
    // ============================================================================

    function initStructurePanel() {
        // Section collapse/expand
        document.addEventListener('click', function (event) {
            if (event.target.closest('.section-header')) {
                const sectionCard = event.target.closest('.section-card');
                if (sectionCard) {
                    sectionCard.classList.toggle('collapsed');
                }
            }
        });
    }

    // ============================================================================
    // SUBMISSIONS & METRICS (Placeholder for future implementation)
    // ============================================================================

    function loadSubmissions() {
        console.log('[TemplateDetails] Loading submissions...');
        // TODO: Implement submissions loading
    }

    function loadMetrics() {
        console.log('[TemplateDetails] Loading metrics...');
        // TODO: Implement metrics loading
    }

    // ============================================================================
    // UTILITY FUNCTIONS
    // ============================================================================

    function showPanelLoading(panel) {
        const loadingHtml = `
            <div class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `;
        const contentArea = panel.querySelector('.panel-content') || panel;
        contentArea.innerHTML = loadingHtml;
    }

    function showPanelError(panel, message) {
        const errorHtml = `
            <div class="alert alert-danger" role="alert">
                <i class="ri-error-warning-line me-2"></i>${escapeHtml(message)}
            </div>
        `;
        const contentArea = panel.querySelector('.panel-content') || panel;
        contentArea.innerHTML = errorHtml;
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function formatDate(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }

    function formatPeriod(year, month) {
        if (!year) return '-';
        if (!month) return year.toString();
        const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        return `${monthNames[month - 1]} ${year}`;
    }

    function getAssignmentTypeIcon(type) {
        const icons = {
            'All': 'ri-global-line',
            'TenantType': 'ri-building-2-line',
            'TenantGroup': 'ri-group-line',
            'SpecificTenant': 'ri-building-line',
            'Role': 'ri-shield-user-line',
            'Department': 'ri-organization-chart',
            'UserGroup': 'ri-team-line',
            'SpecificUser': 'ri-user-line'
        };
        return icons[type] || 'ri-user-line';
    }

    function getAssignmentTypeColor(type) {
        const colors = {
            'All': 'primary',
            'TenantType': 'info',
            'TenantGroup': 'success',
            'SpecificTenant': 'warning',
            'Role': 'danger',
            'Department': 'secondary',
            'UserGroup': 'dark',
            'SpecificUser': 'primary'
        };
        return colors[type] || 'secondary';
    }

    function getStatusBadgeClass(status) {
        const classes = {
            'Active': 'bg-success',
            'Pending': 'bg-warning',
            'Completed': 'bg-info',
            'Overdue': 'bg-danger',
            'Cancelled': 'bg-secondary'
        };
        return classes[status] || 'bg-secondary';
    }

    // ============================================================================
    // PUBLIC API
    // ============================================================================

    window.TemplateDetails = {
        init: init,
        loadWorkflow: loadWorkflow,
        
        // Assignment actions (DataTable handles loading via AJAX)
        reloadAssignments: function() {
            if (typeof window.reloadTable_assignmentsTable === 'function') {
                window.reloadTable_assignmentsTable();
            }
        },
        editAssignment: function (id) {
            window.location.href = `/FormTemplates/Assignments/${id}/Edit`;
        },
        extendAssignment: function (id) {
            const newDate = prompt('Enter new due date (YYYY-MM-DD):');
            if (!newDate) return;
            
            fetch(`/api/assignments/${id}/extend`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ newDueDate: newDate })
            })
                .then(r => r.json())
                .then(result => {
                    if (result.success) {
                        alert('Due date extended successfully.');
                        if (typeof window.reloadTable_assignmentsTable === 'function') {
                            window.reloadTable_assignmentsTable();
                        }
                    } else {
                        alert(result.message || 'Failed to extend due date.');
                    }
                })
                .catch(err => {
                    console.error(err);
                    alert('An error occurred.');
                });
        },
        remindAssignment: function (id) {
            if (!confirm('Send reminder for this assignment?')) return;
            
            fetch(`/api/assignments/${id}/remind`, {
                method: 'POST'
            })
                .then(r => r.json())
                .then(result => {
                    alert(result.success ? 'Reminder sent.' : (result.message || 'Failed to send reminder.'));
                })
                .catch(err => {
                    console.error(err);
                    alert('An error occurred.');
                });
        },
        cancelAssignment: function (id) {
            const reason = prompt('Enter cancellation reason:');
            if (reason === null) return;
            
            fetch(`/api/assignments/${id}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ reason: reason })
            })
                .then(r => r.json())
                .then(result => {
                    if (result.success) {
                        alert('Assignment cancelled.');
                        if (typeof window.reloadTable_assignmentsTable === 'function') {
                            window.reloadTable_assignmentsTable();
                        }
                    } else {
                        alert(result.message || 'Failed to cancel assignment.');
                    }
                })
                .catch(err => {
                    console.error(err);
                    alert('An error occurred.');
                });
        },
        
        // Workflow actions
        editStep: function (id) { console.log('Edit step:', id); },
        deleteStep: function (id) { console.log('Delete step:', id); },
        selectExistingWorkflow: function () { console.log('Select existing workflow'); },
        createNewWorkflow: function () { console.log('Create new workflow'); }
    };

    // ============================================================================
    // AUTO-INITIALIZE
    // ============================================================================

    document.addEventListener('DOMContentLoaded', init);

})();
