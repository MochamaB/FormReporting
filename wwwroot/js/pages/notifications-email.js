/**
 * Email Notifications Page JavaScript
 * Handles email list, filters, and offcanvas for Email Notifications page
 */

let currentFilter = 'all';
let currentCategory = null;
let currentPage = 1;
let pageSize = 20;
let totalEmails = 0;
let currentEmailId = null;

$(document).ready(function () {
    initializeFilters();
    initializeOffcanvas();
    initializeActions();
    loadEmails();
});

// ============================================================================
// FILTERS
// ============================================================================

function initializeFilters() {
    // Status filters (All, Sent, Pending, Failed)
    $('.list-group-item[data-filter]').on('click', function (e) {
        e.preventDefault();

        // Update active state
        $('.list-group-item[data-filter]').removeClass('active');
        $(this).addClass('active');

        // Set filter and reload
        currentFilter = $(this).data('filter');
        currentPage = 1;
        loadEmails();
    });

    // Category filters
    $('.list-group-item[data-category]').on('click', function (e) {
        e.preventDefault();
        currentCategory = $(this).data('category') || null;
        currentPage = 1;
        loadEmails();
    });

    // Refresh button
    $('#refreshEmails').on('click', function () {
        loadEmails();
    });

    // Pagination
    $('#prevPage').on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadEmails();
        }
    });

    $('#nextPage').on('click', function () {
        const totalPages = Math.ceil(totalEmails / pageSize);
        if (currentPage < totalPages) {
            currentPage++;
            loadEmails();
        }
    });
}

// ============================================================================
// LOAD EMAILS
// ============================================================================

function loadEmails() {
    // Show loading
    $('#emailLoading').show();
    $('#emailList').hide();
    $('#emailEmpty').hide();
    $('#emailPagination').hide();

    $.ajax({
        url: '/Notifications/GetEmailList',
        method: 'GET',
        data: {
            filter: currentFilter,
            category: currentCategory,
            page: currentPage,
            pageSize: pageSize
        },
        success: function (response) {
            if (response.data && response.data.length > 0) {
                displayEmails(response.data);
                totalEmails = response.data.length; // Note: This should come from server
                updatePagination();
            } else {
                showEmptyState();
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading emails:', error);
            showToast('Failed to load emails', 'error');
            showEmptyState();
        }
    });
}

function displayEmails(emails) {
    $('#emailLoading').hide();
    $('#emailEmpty').hide();

    let html = '<div class="list-group list-group-flush">';

    emails.forEach(function (email) {
        const unreadClass = !email.isRead ? 'fw-bold' : '';
        const unreadIndicator = !email.isRead ? '<span class="badge bg-warning-subtle text-warning badge-sm">New</span>' : '';

        // Status badge
        let statusBadge = '';
        if (email.deliveryStatus === 'Sent' || email.deliveryStatus === 'Delivered') {
            statusBadge = '<span class="badge bg-success-subtle text-success badge-sm">Sent</span>';
        } else if (email.deliveryStatus === 'Pending') {
            statusBadge = '<span class="badge bg-warning-subtle text-warning badge-sm">Pending</span>';
        } else if (email.deliveryStatus === 'Failed' || email.deliveryStatus === 'Bounced') {
            statusBadge = '<span class="badge bg-danger-subtle text-danger badge-sm">Failed</span>';
        }

        html += `
            <a href="javascript:void(0);" class="list-group-item list-group-item-action email-item" data-id="${email.notificationId}">
                <div class="d-flex align-items-start">
                    <div class="flex-shrink-0 me-3">
                        <div class="avatar-xs">
                            <div class="avatar-title rounded-circle bg-primary-subtle text-primary">
                                <i class="ri-mail-line"></i>
                            </div>
                        </div>
                    </div>
                    <div class="flex-grow-1 overflow-hidden">
                        <div class="d-flex align-items-center mb-1">
                            <h6 class="mb-0 ${unreadClass} text-truncate flex-grow-1">${email.subject}</h6>
                            <div class="flex-shrink-0 ms-2">
                                ${unreadIndicator}
                                ${statusBadge}
                            </div>
                        </div>
                        <p class="text-muted text-truncate mb-1">${email.previewText}</p>
                        <div class="d-flex align-items-center">
                            <small class="text-muted">
                                <i class="ri-time-line me-1"></i>
                                ${formatDate(email.sentDate)}
                            </small>
                            <small class="text-muted ms-3">
                                <i class="ri-mail-line me-1"></i>
                                ${email.recipientAddress}
                            </small>
                        </div>
                    </div>
                </div>
            </a>
        `;
    });

    html += '</div>';

    $('#emailList').html(html).show();

    // Attach click handlers
    $('.email-item').on('click', function () {
        const emailId = $(this).data('id');
        viewEmail(emailId);
    });
}

function showEmptyState() {
    $('#emailLoading').hide();
    $('#emailList').hide();
    $('#emailEmpty').show();
    $('#emailPagination').hide();
}

function updatePagination() {
    const totalPages = Math.ceil(totalEmails / pageSize);
    const start = (currentPage - 1) * pageSize + 1;
    const end = Math.min(currentPage * pageSize, totalEmails);

    $('#emailStart').text(start);
    $('#emailEnd').text(end);
    $('#emailTotal').text(totalEmails);

    $('#prevPage').prop('disabled', currentPage === 1);
    $('#nextPage').prop('disabled', currentPage >= totalPages);

    $('#emailPagination').show();
}

// ============================================================================
// OFFCANVAS (View Email Details)
// ============================================================================

function initializeOffcanvas() {
    const offcanvasElement = document.getElementById('emailOffcanvas');

    offcanvasElement.addEventListener('hidden.bs.offcanvas', function () {
        // Reset offcanvas state when closed
        currentEmailId = null;
        $('#emailDetailContent').hide();
        $('#emailDetailLoading').show();
        $('#emailDetailError').hide();
    });
}

function viewEmail(emailId) {
    currentEmailId = emailId;

    // Show offcanvas
    const offcanvas = new bootstrap.Offcanvas(document.getElementById('emailOffcanvas'));
    offcanvas.show();

    // Show loading state
    $('#emailDetailLoading').show();
    $('#emailDetailContent').hide();
    $('#emailDetailError').hide();

    // Fetch email details
    $.ajax({
        url: '/Notifications/GetDetails',
        method: 'GET',
        data: { id: emailId },
        success: function (response) {
            if (response.success) {
                displayEmailDetails(response.data);
            } else {
                showEmailError(response.error || 'Failed to load email');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading email:', error);
            showEmailError('An error occurred while loading the email');
        }
    });
}

function displayEmailDetails(email) {
    // Hide loading, show content
    $('#emailDetailLoading').hide();
    $('#emailDetailContent').show();

    // Set email details
    $('#emailSubject').text(email.title);
    $('#emailFrom').text(email.from || 'KTDA Form Reporting System');
    $('#emailTo').text(email.recipientAddress);
    $('#emailDate').text(formatDate(email.sentDate || email.createdDate));
    $('#emailBody').html(email.fullMessage);

    // Delivery status
    $('#emailDeliveryStatus').text(email.deliveryStatus || 'Unknown');

    if (email.sentDate) {
        $('#emailSentDate').text(formatDate(email.sentDate));
        $('#emailSentDateRow').show();
    } else {
        $('#emailSentDateRow').hide();
    }

    if (email.errorMessage) {
        $('#emailError').text(email.errorMessage);
        $('#emailErrorRow').show();
    } else {
        $('#emailErrorRow').hide();
    }

    // Status badges
    let statusClass = '';
    if (email.deliveryStatus === 'Sent' || email.deliveryStatus === 'Delivered') {
        statusClass = 'bg-success';
    } else if (email.deliveryStatus === 'Pending') {
        statusClass = 'bg-warning';
    } else if (email.deliveryStatus === 'Failed' || email.deliveryStatus === 'Bounced') {
        statusClass = 'bg-danger';
    }
    $('#emailStatusBadge').attr('class', `badge ${statusClass}`).text(email.deliveryStatus || 'Unknown');

    // Priority badge
    const priorityClasses = {
        'Urgent': 'bg-danger',
        'High': 'bg-warning',
        'Normal': 'bg-info',
        'Low': 'bg-secondary'
    };
    $('#emailPriorityBadge').attr('class', `badge ${priorityClasses[email.priority] || 'bg-info'}`).text(email.priority);

    // Read/Unread badge
    const readClass = email.isRead ? 'bg-success' : 'bg-warning';
    const readText = email.isRead ? 'Read' : 'Unread';
    $('#emailReadBadge').attr('class', `badge ${readClass}`).text(readText);

    // Source information
    if (email.sourceEntityType) {
        $('#emailSource').text(`${email.sourceEntityType} #${email.sourceEntityId}`);
        $('#emailSourceWrapper').show();
    } else {
        $('#emailSourceWrapper').hide();
    }

    // Action button
    if (email.actionUrl) {
        $('#emailActionButton').attr('href', email.actionUrl);
        $('#emailActionText').text('View Related Item');
        $('#emailActionWrapper').show();
    } else {
        $('#emailActionWrapper').hide();
    }

    // Show/hide mark as read/unread buttons
    if (email.isRead) {
        $('#markEmailAsReadBtn').hide();
        $('#markEmailAsUnreadBtn').show();
    } else {
        $('#markEmailAsReadBtn').show();
        $('#markEmailAsUnreadBtn').hide();
    }
}

function showEmailError(message) {
    $('#emailDetailLoading').hide();
    $('#emailDetailErrorMessage').text(message);
    $('#emailDetailError').show();
}

// ============================================================================
// ACTIONS
// ============================================================================

function initializeActions() {
    // Mark as read from offcanvas
    $('#markEmailAsReadBtn').on('click', function () {
        if (currentEmailId) {
            markEmailAsRead(currentEmailId);
        }
    });

    // Mark as unread from offcanvas
    $('#markEmailAsUnreadBtn').on('click', function () {
        if (currentEmailId) {
            markEmailAsUnread(currentEmailId);
        }
    });

    // Delete from offcanvas
    $('#deleteEmailBtn').on('click', function () {
        if (currentEmailId) {
            deleteEmail(currentEmailId);
        }
    });
}

function markEmailAsRead(emailId) {
    $.ajax({
        url: '/Notifications/MarkAsRead',
        method: 'POST',
        data: { id: emailId },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                loadEmails(); // Reload list

                // Update offcanvas if open
                if (currentEmailId === emailId) {
                    $('#markEmailAsReadBtn').hide();
                    $('#markEmailAsUnreadBtn').show();
                    $('#emailReadBadge').attr('class', 'badge bg-success').text('Read');
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

function markEmailAsUnread(emailId) {
    $.ajax({
        url: '/Notifications/MarkAsUnread',
        method: 'POST',
        data: { id: emailId },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                loadEmails(); // Reload list

                // Update offcanvas if open
                if (currentEmailId === emailId) {
                    $('#markEmailAsReadBtn').show();
                    $('#markEmailAsUnreadBtn').hide();
                    $('#emailReadBadge').attr('class', 'badge bg-warning').text('Unread');
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

function deleteEmail(emailId) {
    if (!confirm('Are you sure you want to delete this email?')) return;

    $.ajax({
        url: '/Notifications/Delete',
        method: 'POST',
        data: { id: emailId },
        success: function (response) {
            if (response.success) {
                showToast(response.message, 'success');
                loadEmails(); // Reload list

                // Close offcanvas
                const offcanvas = bootstrap.Offcanvas.getInstance(document.getElementById('emailOffcanvas'));
                offcanvas.hide();
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
