/**
 * Form Builder Progress - Exit and Navigation Logic
 * Handles exit confirmation with modals and unsaved changes detection
 */

// Track if user is intentionally navigating (to prevent double confirmation)
let isNavigating = false;

document.addEventListener('DOMContentLoaded', function() {
    // Initialize Bootstrap modals
    const exitConfirmModal = document.getElementById('exitConfirmModal');
    const exitModalInstance = exitConfirmModal ? new bootstrap.Modal(exitConfirmModal) : null;

    // ============================================================================
    // GLOBAL NAVIGATION INTERCEPTION
    // Intercept all clicks on links (sidebar menu, navbar, etc.)
    // ============================================================================
    document.addEventListener('click', function(e) {
        // Skip if already navigating or no unsaved changes
        if (isNavigating) return;
        
        // Find if clicked element or its parent is a link
        const link = e.target.closest('a');
        if (!link || !link.href) return;
        
        // Skip if it's the exit button (handled separately)
        if (link.id === 'exitBtn') return;
        
        // Skip if it's a same-page anchor link (#)
        if (link.getAttribute('href')?.startsWith('#')) return;
        
        // Skip if it's a modal trigger or dropdown
        if (link.hasAttribute('data-bs-toggle') || link.hasAttribute('data-bs-target')) return;
        
        // Check if navigation is within the same page (form builder steps)
        const currentPath = window.location.pathname;
        const linkPath = new URL(link.href, window.location.origin).pathname;
        
        // Skip if it's a step navigation (handled by navigateToStep)
        if (link.hasAttribute('onclick') && link.getAttribute('onclick').includes('navigateToStep')) return;
        
        // Check for unsaved changes
        if (typeof hasUnsavedChanges === 'function' && hasUnsavedChanges()) {
            e.preventDefault();
            
            // Show confirmation modal
            const modalMessage = document.getElementById('exitModalMessage');
            if (modalMessage) {
                modalMessage.textContent = 'You have unsaved changes. Are you sure you want to leave this page? All unsaved changes will be lost.';
            }
            
            if (exitModalInstance) {
                // Update confirm button to navigate to clicked link
                const confirmBtn = document.getElementById('confirmExitBtn');
                if (confirmBtn) {
                    confirmBtn.onclick = function() {
                        isNavigating = true;
                        exitModalInstance.hide();
                        window.location.href = link.href;
                    };
                }
                exitModalInstance.show();
            }
        }
    }, true); // Use capture phase to catch all clicks early

    // Exit button handler
    const exitBtn = document.getElementById('exitBtn');
    if (exitBtn && exitModalInstance) {
        exitBtn.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Check if there are unsaved changes
            const hasChanges = typeof hasUnsavedChanges === 'function' && hasUnsavedChanges();
            
            // Update modal message based on unsaved changes
            const modalMessage = document.getElementById('exitModalMessage');
            if (modalMessage) {
                modalMessage.textContent = hasChanges 
                    ? 'You have unsaved changes. Are you sure you want to exit the Form Builder? All unsaved changes will be lost.'
                    : 'Are you sure you want to exit the Form Builder?';
            }
            
            // Show modal
            exitModalInstance.show();
        });
    }

    // Confirm exit button in modal
    const confirmExitBtn = document.getElementById('confirmExitBtn');
    if (confirmExitBtn && exitBtn) {
        confirmExitBtn.addEventListener('click', function() {
            // Get exit URL from the link
            const exitUrl = exitBtn.getAttribute('href');
            if (exitUrl) {
                window.location.href = exitUrl;
            }
        });
    }

    // Step navigation with unsaved changes check
    window.navigateToStep = function(url) {
        if (typeof hasUnsavedChanges === 'function' && hasUnsavedChanges()) {
            // Show confirmation modal for step navigation
            const modalMessage = document.getElementById('exitModalMessage');
            if (modalMessage) {
                modalMessage.textContent = 'You have unsaved changes. Are you sure you want to leave this step?';
            }
            
            if (exitModalInstance) {
                // Update confirm button to navigate to step instead
                const confirmBtn = document.getElementById('confirmExitBtn');
                if (confirmBtn) {
                    confirmBtn.onclick = function() {
                        window.location.href = url;
                    };
                }
                exitModalInstance.show();
            }
        } else {
            window.location.href = url;
        }
    };

    // Save draft button handler (if exists)
    const saveDraftBtn = document.getElementById('saveDraftBtn');
    if (saveDraftBtn) {
        saveDraftBtn.addEventListener('click', function() {
            if (typeof saveDraft === 'function') {
                saveDraft();
            } else {
                // Show info modal
                showInfoModal('Save draft functionality not implemented for this step');
            }
        });
    }

    // Helper function to show info modal
    function showInfoModal(message) {
        const infoModal = document.getElementById('infoModal');
        const infoMessage = document.getElementById('infoModalMessage');
        
        if (infoModal && infoMessage) {
            infoMessage.textContent = message;
            const modalInstance = new bootstrap.Modal(infoModal);
            modalInstance.show();
        } else {
            // Fallback to alert if modal not found
            alert(message);
        }
    }

    // ============================================================================
    // BROWSER NAVIGATION INTERCEPTION
    // Intercept browser back/forward buttons and page close
    // ============================================================================
    window.addEventListener('beforeunload', function(e) {
        // Skip if already navigating
        if (isNavigating) return;
        
        // Check for unsaved changes
        if (typeof hasUnsavedChanges === 'function' && hasUnsavedChanges()) {
            // Standard way to show browser confirmation dialog
            e.preventDefault();
            e.returnValue = ''; // Chrome requires returnValue to be set
            return ''; // Some browsers show this message
        }
    });
});
