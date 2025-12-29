/**
 * Workflow Wizard JavaScript
 * Helper functions for the workflow creation wizard
 * Handles API calls for sections, fields, actions, roles, users, and departments
 */

const WorkflowWizard = (function() {
    'use strict';

    // State
    let templateId = null;
    let submissionMode = 1; // 1=Individual, 2=Collaborative
    let availableActions = [];
    let sections = [];
    let fields = [];

    // Wizard step state (for auto-generating step names)
    let selectedTarget = {
        type: null,        // "Submission", "Section", "Field"
        id: null,          // null for Submission, sectionId or fieldId
        name: null         // Display name (e.g., "Entire Submission", "Section One", "Email Address")
    };

    /**
     * Initialize the workflow wizard
     * @param {number} tid - Template ID
     * @param {number} mode - Submission mode (1=Individual, 2=Collaborative)
     */
    function init(tid, mode = 1) {
        templateId = tid;
        submissionMode = mode;
        console.log('[WorkflowWizard] Initialized with templateId:', templateId, 'mode:', submissionMode);
    }

    /**
     * Get template ID
     * @returns {number|null}
     */
    function getTemplateId() {
        return templateId;
    }

    /**
     * Get submission mode
     * @returns {number} 1=Individual, 2=Collaborative
     */
    function getSubmissionMode() {
        return submissionMode;
    }

    /**
     * Check if template is in Individual mode
     * @returns {boolean}
     */
    function isIndividualMode() {
        return submissionMode === 1;
    }

    /**
     * Check if template is in Collaborative mode
     * @returns {boolean}
     */
    function isCollaborativeMode() {
        return submissionMode === 2;
    }

    // ============================================================================
    // TARGET SELECTION STATE
    // ============================================================================

    /**
     * Set selected target information
     * @param {string} type - Target type: "Submission", "Section", "Field"
     * @param {number|null} id - Target ID (null for Submission)
     * @param {string} name - Display name
     */
    function setSelectedTarget(type, id, name) {
        selectedTarget.type = type;
        selectedTarget.id = id;
        selectedTarget.name = name;
        console.log('[WorkflowWizard] Target selected:', selectedTarget);
    }

    /**
     * Get selected target information
     * @returns {Object} Selected target object
     */
    function getSelectedTarget() {
        return selectedTarget;
    }

    /**
     * Generate step name based on action and target
     * @param {string} actionName - Action name (e.g., "Fill", "Approve", "Sign")
     * @returns {string} Generated step name
     */
    function generateStepName(actionName) {
        if (!actionName) {
            return '';
        }

        if (!selectedTarget.name) {
            return actionName;
        }

        return `${actionName} ${selectedTarget.name}`;
    }

    // ============================================================================
    // API HELPER FUNCTIONS
    // ============================================================================

    /**
     * Load available workflow actions
     * @returns {Promise<Array>}
     */
    async function loadAvailableActions() {
        try {
            const response = await fetch('/api/workflows/actions');
            const result = await response.json();
            
            if (result.success) {
                availableActions = result.data;
                return availableActions;
            }
            return [];
        } catch (error) {
            console.error('[WorkflowWizard] Error loading actions:', error);
            return [];
        }
    }

    /**
     * Get cached available actions
     * @returns {Array}
     */
    function getAvailableActions() {
        return availableActions;
    }

    /**
     * Load template sections
     * @returns {Promise<Array>}
     */
    async function loadSections() {
        if (!templateId) {
            console.error('[WorkflowWizard] Template ID not set');
            return [];
        }

        try {
            const response = await fetch(`/api/workflows/templates/${templateId}/sections`);
            const result = await response.json();
            
            if (result.success) {
                sections = result.data;
                return sections;
            }
            return [];
        } catch (error) {
            console.error('[WorkflowWizard] Error loading sections:', error);
            return [];
        }
    }

    /**
     * Get cached sections
     * @returns {Array}
     */
    function getSections() {
        return sections;
    }

    /**
     * Load template fields
     * @param {number|null} sectionId - Optional section ID to filter by
     * @returns {Promise<Array>}
     */
    async function loadFields(sectionId = null) {
        if (!templateId) {
            console.error('[WorkflowWizard] Template ID not set');
            return [];
        }

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
            return [];
        } catch (error) {
            console.error('[WorkflowWizard] Error loading fields:', error);
            return [];
        }
    }

    /**
     * Get cached fields
     * @returns {Array}
     */
    function getFields() {
        return fields;
    }

    /**
     * Load available roles
     * @returns {Promise<Array>}
     */
    async function loadRoles() {
        try {
            const response = await fetch('/api/workflows/roles');
            const result = await response.json();
            
            if (result.success) {
                return result.data;
            }
            return [];
        } catch (error) {
            console.error('[WorkflowWizard] Error loading roles:', error);
            return [];
        }
    }

    /**
     * Load available users
     * @returns {Promise<Array>}
     */
    async function loadUsers() {
        try {
            const response = await fetch('/api/workflows/users');
            const result = await response.json();
            
            if (result.success) {
                return result.data;
            }
            return [];
        } catch (error) {
            console.error('[WorkflowWizard] Error loading users:', error);
            return [];
        }
    }

    /**
     * Load available departments
     * @returns {Promise<Array>}
     */
    async function loadDepartments() {
        try {
            const response = await fetch('/api/workflows/departments');
            const result = await response.json();
            
            if (result.success) {
                return result.data;
            }
            return [];
        } catch (error) {
            console.error('[WorkflowWizard] Error loading departments:', error);
            return [];
        }
    }

    // ============================================================================
    // MODE RESTRICTIONS
    // ============================================================================

    /**
     * Apply mode-specific restrictions to the workflow wizard UI
     * For Individual mode:
     * - Hide/disable Section and Field target options (only allow Submission)
     * - Filter out Fill action (only allow Approve, Reject, Sign, Review, Verify)
     */
    function applyModeRestrictions() {
        console.log('[WorkflowWizard] Applying mode restrictions for mode:', submissionMode);

        if (isIndividualMode()) {
            applyIndividualModeRestrictions();
        }
        // Collaborative mode has no restrictions - all options available
    }

    /**
     * Apply Individual mode restrictions
     * - Only "Entire Submission" target allowed
     * - No "Fill" action allowed
     */
    function applyIndividualModeRestrictions() {
        // STEP 1: Restrict target types to "Submission" only
        const sectionCard = document.querySelector('.target-type-card[data-target-type="Section"]');
        const fieldCard = document.querySelector('.target-type-card[data-target-type="Field"]');

        if (sectionCard) {
            sectionCard.style.opacity = '0.5';
            sectionCard.style.pointerEvents = 'none';
            sectionCard.style.position = 'relative';

            // Add disabled overlay with explanation
            if (!sectionCard.querySelector('.mode-restriction-overlay')) {
                const overlay = document.createElement('div');
                overlay.className = 'mode-restriction-overlay';
                overlay.innerHTML = '<i class="ri-lock-line"></i> Not available in Individual mode';
                sectionCard.appendChild(overlay);
            }
        }

        if (fieldCard) {
            fieldCard.style.opacity = '0.5';
            fieldCard.style.pointerEvents = 'none';
            fieldCard.style.position = 'relative';

            // Add disabled overlay with explanation
            if (!fieldCard.querySelector('.mode-restriction-overlay')) {
                const overlay = document.createElement('div');
                overlay.className = 'mode-restriction-overlay';
                overlay.innerHTML = '<i class="ri-lock-line"></i> Not available in Individual mode';
                fieldCard.appendChild(overlay);
            }
        }

        console.log('[WorkflowWizard] Individual mode: Section and Field targets disabled');
    }

    /**
     * Filter actions based on submission mode
     * For Individual mode, exclude "Fill" action
     * @param {Array} actions - Original actions array
     * @returns {Array} Filtered actions array
     */
    function filterActionsByMode(actions) {
        if (!actions || !Array.isArray(actions)) {
            return [];
        }

        if (isIndividualMode()) {
            // Individual mode: Exclude "Fill" action
            const filtered = actions.filter(action => action.actionCode !== 'Fill');
            console.log('[WorkflowWizard] Individual mode: Filtered out Fill action. Available actions:',
                        filtered.map(a => a.actionCode).join(', '));
            return filtered;
        }

        // Collaborative mode: All actions available
        return actions;
    }

    /**
     * Filter assignee types based on submission mode
     * For Individual mode, exclude "Submitter" and "FieldValue" types
     * @param {Array} assigneeTypes - Original assignee types array
     * @returns {Array} Filtered assignee types array
     */
    function filterAssigneesByMode(assigneeTypes) {
        if (!assigneeTypes || !Array.isArray(assigneeTypes)) {
            return [];
        }

        if (isIndividualMode()) {
            // Individual mode: Exclude "Submitter" and "FieldValue" (conflict of interest)
            const filtered = assigneeTypes.filter(type =>
                type !== 'Submitter' && type !== 'FieldValue'
            );
            console.log('[WorkflowWizard] Individual mode: Filtered assignee types. Available:',
                        filtered.join(', '));
            return filtered;
        }

        // Collaborative mode: All assignee types available
        return assigneeTypes;
    }

    // ============================================================================
    // UTILITY FUNCTIONS
    // ============================================================================

    /**
     * Get action icon class
     * @param {string} actionCode - Action code
     * @returns {string}
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
     * Get action color
     * @param {string} actionCode - Action code
     * @returns {string}
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
     * Show success notification
     * @param {string} message
     */
    function showSuccess(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ 
                icon: 'success', 
                title: 'Success', 
                text: message, 
                timer: 2000, 
                showConfirmButton: false 
            });
        } else {
            alert(message);
        }
    }

    /**
     * Show error notification
     * @param {string} message
     */
    function showError(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ 
                icon: 'error', 
                title: 'Error', 
                text: message 
            });
        } else {
            alert(message);
        }
    }

    /**
     * Show info notification
     * @param {string} message
     */
    function showInfo(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({ 
                icon: 'info', 
                title: 'Info', 
                text: message 
            });
        } else {
            alert(message);
        }
    }

    // Public API
    return {
        init,
        getTemplateId,
        getSubmissionMode,
        isIndividualMode,
        isCollaborativeMode,
        loadAvailableActions,
        filterActionsByMode,
        loadSections,
        getSections,
        loadFields,
        getFields,
        loadRoles,
        loadUsers,
        loadDepartments,
        setSelectedTarget,
        generateStepName,
        applyModeRestrictions,
        showSuccess,
        showError,
        showInfo
    };
})();

// Expose WorkflowWizard globally
window.WorkflowWizard = WorkflowWizard;
