/**
 * Notifications Index Page JavaScript
 * Handles notification actions and offcanvas for All Notifications page
 * DataTable component handles AJAX loading automatically
 */

let currentNotificationId = null;

$(document).ready(function () {
    initializeOffcanvas();
});

// ============================================================================
// OFFCANVAS (View Notification Details)
// ============================================================================

function initializeOffcanvas() {
    const offcanvasElement = document.getElementById('notificationOffcanvas');

    if (offcanvasElement) {
        offcanvasElement.addEventListener('hidden.bs.offcanvas', function () {
            // Reset offcanvas state when closed
            currentNotificationId = null;
            $('#notificationContent').hide();
            $('#notificationDetailLoading').show();
            $('#notificationDetailError').hide();
        });
    }

    // Wire up offcanvas action buttons
    $('#markAsReadBtn').off('click').on('click', function () {
        if (currentNotificationId) {
            markAsRead(currentNotificationId);
        }
    });

    $('#markAsUnreadBtn').off('click').on('click', function () {
        if (currentNotificationId) {
            markAsUnread(currentNotificationId);
        }
    });

    $('#deleteNotificationBtn').off('click').on('click', function () {
        if (currentNotificationId) {
            deleteNotificationFromOffcanvas(currentNotificationId);
        }
    });
}

/**
 * View notification details in offcanvas
 * Called from DataTable row action
 */
function viewNotification(notificationId) {
    currentNotificationId = notificationId;

    // Show offcanvas
    const offcanvas = new bootstrap.Offcanvas(document.getElementById('notificationOffcanvas'));
    offcanvas.show();

    // Show loading state
    $('#notificationDetailLoading').show();
    $('#notificationContent').hide();
    $('#notificationDetailError').hide();

    // Fetch notification details
    $.ajax({
        url: '/Notifications/GetDetails',
        method: 'GET',
        data: { id: notificationId },
        success: function (response) {
            if (response.success) {
                displayNotificationDetails(response.data);

                // Auto-mark as read when viewed
                if (!response.data.isRead) {
                    markAsReadSilent(notificationId);
                }
            } else {
                showNotificationError(response.error || 'Failed to load notification');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading notification:', error);
            showNotificationError('An error occurred while loading the notification');
        }
    });
}

function displayNotificationDetails(notification) {
    // Hide loading, show content
    $('#notificationDetailLoading').hide();
    $('#notificationContent').show();

    // Set title and metadata
    $('#notificationTitle').text(notification.title);

    // Category
    if (notification.category) {
        $('#notificationCategory').text(notification.category);
        $('#notificationCategoryWrapper').show();
    } else {
        $('#notificationCategoryWrapper').hide();
    }

    $('#notificationDate').text(formatDate(notification.createdDate));

    // Set message content (HTML)
    $('#notificationMessage').html(notification.fullMessage);

    // Priority badge
    const priorityClasses = {
        'Urgent': 'bg-danger',
        'High': 'bg-warning',
        'Normal': 'bg-info',
        'Low': 'bg-secondary'
    };
    $('#notificationPriorityBadge').attr('class', `badge ${priorityClasses[notification.priority] || 'bg-info'}`).text(notification.priority);

    // Channel badge
    $('#notificationChannelBadge').text(notification.channelType);

    // Read/Unread badge
    const readClass = notification.isRead ? 'bg-success' : 'bg-warning';
    const readText = notification.isRead ? 'Read' : 'Unread';
    $('#notificationReadBadge').attr('class', `badge ${readClass}`).text(readText);

    // Source information
    if (notification.sourceEntityType) {
        $('#notificationSource').text(`${notification.sourceEntityType} #${notification.sourceEntityId}`);
        $('#notificationSourceWrapper').show();
    } else {
        $('#notificationSourceWrapper').hide();
    }

    // Action button
    if (notification.actionUrl) {
        $('#notificationActionButton').attr('href', notification.actionUrl);
        $('#notificationActionText').text('View Related Item');
        $('#notificationActionWrapper').show();
    } else {
        $('#notificationActionWrapper').hide();
    }

    // Show/hide mark as read/unread buttons
    if (notification.isRead) {
        $('#markAsReadBtn').hide();
        $('#markAsUnreadBtn').show();
    } else {
        $('#markAsReadBtn').show();
        $('#markAsUnreadBtn').hide();
    }
}

function showNotificationError(message) {
    $('#notificationDetailLoading').hide();
    $('#notificationDetailErrorMessage').text(message);
    $('#notificationDetailError').show();
}

// ============================================================================
// ACTIONS (Called from DataTable)
// ============================================================================

/**
 * Mark notification as read
 * Called from DataTable row action
 */
function markAsRead(notificationId) {
    $.ajax({
        url: '/Notifications/MarkAsRead',
        method: 'POST',
        data: { id: notificationId },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');

                // Reload the DataTable
                if (typeof window['reloadTable_notificationsTable'] === 'function') {
                    window['reloadTable_notificationsTable']();
                }

                refreshStats();
            } else {
                showToast(response.error, 'error');
            }
        },
        error: function () {
            showToast('An error occurred', 'error');
        }
    });
}

/**
 * Mark as read silently (no toast notification)
 * Used when viewing a notification
 */
function markAsReadSilent(notificationId) {
    $.ajax({
        url: '/Notifications/MarkAsRead',
        method: 'POST',
        data: { id: notificationId },
        success: function (response) {
            if (response.success) {
                // Reload the DataTable silently
                if (typeof window['reloadTable_notificationsTable'] === 'function') {
                    window['reloadTable_notificationsTable']();
                }
                refreshStats();
            }
        }
    });
}

/**
 * Mark notification as unread
 * Called from offcanvas action
 */
function markAsUnread(notificationId) {
    $.ajax({
        url: '/Notifications/MarkAsUnread',
        method: 'POST',
        data: { id: notificationId },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');

                // Reload the DataTable
                if (typeof window['reloadTable_notificationsTable'] === 'function') {
                    window['reloadTable_notificationsTable']();
                }

                refreshStats();

                // Close the offcanvas
                const offcanvas = bootstrap.Offcanvas.getInstance(document.getElementById('notificationOffcanvas'));
                if (offcanvas) {
                    offcanvas.hide();
                }
            } else {
                showToast(response.error, 'error');
            }
        },
        error: function () {
            showToast('An error occurred', 'error');
        }
    });
}

/**
 * Delete notification
 * Called from DataTable row action
 */
function deleteNotification(notificationId) {
    if (!confirm('Are you sure you want to delete this notification?')) return;

    $.ajax({
        url: '/Notifications/Delete',
        method: 'POST',
        data: { id: notificationId },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');

                // Reload the DataTable
                if (typeof window['reloadTable_notificationsTable'] === 'function') {
                    window['reloadTable_notificationsTable']();
                }

                refreshStats();

                // Close offcanvas if it's open for this notification
                if (currentNotificationId === notificationId) {
                    const offcanvas = bootstrap.Offcanvas.getInstance(document.getElementById('notificationOffcanvas'));
                    if (offcanvas) {
                        offcanvas.hide();
                    }
                }
            } else {
                showToast(response.error, 'error');
            }
        },
        error: function () {
            showToast('An error occurred', 'error');
        }
    });
}

/**
 * Delete notification from offcanvas
 * Called from offcanvas delete button
 */
function deleteNotificationFromOffcanvas(notificationId) {
    if (!confirm('Are you sure you want to delete this notification?')) return;

    $.ajax({
        url: '/Notifications/Delete',
        method: 'POST',
        data: { id: notificationId },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');

                // Reload the DataTable
                if (typeof window['reloadTable_notificationsTable'] === 'function') {
                    window['reloadTable_notificationsTable']();
                }

                refreshStats();

                // Close the offcanvas
                const offcanvas = bootstrap.Offcanvas.getInstance(document.getElementById('notificationOffcanvas'));
                if (offcanvas) {
                    offcanvas.hide();
                }
            } else {
                showToast(response.error, 'error');
            }
        },
        error: function () {
            showToast('An error occurred', 'error');
        }
    });
}

// ============================================================================
// UTILITIES
// ============================================================================

function refreshStats() {
    $.ajax({
        url: '/Notifications/GetStats',
        method: 'GET',
        success: function (response) {
            if (response.success && response.data) {
                // Update stat card numbers
                $('#totalCount').text(response.data.totalCount);
                $('#unreadCount').text(response.data.unreadCount);
                $('#emailCount').text(response.data.emailCount);
                $('#inAppCount').text(response.data.inAppCount);
            }
        }
    });
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';

    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) {
        const hours = Math.floor(diff / (1000 * 60 * 60));
        if (hours === 0) {
            const minutes = Math.floor(diff / (1000 * 60));
            return `${minutes} minute${minutes !== 1 ? 's' : ''} ago`;
        }
        return `${hours} hour${hours !== 1 ? 's' : ''} ago`;
    } else if (days === 1) {
        return 'Yesterday';
    } else if (days < 7) {
        return `${days} days ago`;
    } else {
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    }
}

function showToast(message, type = 'info') {
    // Using Toastify or similar library (adjust based on your setup)
    if (typeof Toastify !== 'undefined') {
        Toastify({
            text: message,
            duration: 3000,
            gravity: 'top',
            position: 'right',
            backgroundColor: type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#17a2b8'
        }).showToast();
    } else {
        console.log(`[${type.toUpperCase()}] ${message}`);
    }
}
