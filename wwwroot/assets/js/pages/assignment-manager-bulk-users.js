/**
 * AssignmentManager - Bulk User Selection Module
 *
 * Provides reusable functionality for bulk user selection with tenant grouping
 * Used in: Role wizard, Form template wizard, Permission assignments, etc.
 *
 * Features:
 * - AJAX loading of scope-filtered users
 * - Grouping by tenant in accordions
 * - Multi-select with checkboxes
 * - Search filtering
 * - Select All / Deselect All (global and per-tenant)
 * - Real-time selection counters
 *
 * Usage:
 * 1. Include this script in your view
 * 2. Call initBulkUserSelection(options) with configuration
 */

(function (window, $) {
    'use strict';

    /**
     * Initialize bulk user selection functionality
     * @param {Object} options Configuration options
     * @param {string} options.managerId - Unique identifier for this instance
     * @param {string} options.ajaxEndpoint - URL to fetch users (e.g., '/Roles/GetUsersGroupedByTenant')
     * @param {string} options.containerSelector - Selector for tenant groups container (e.g., '#tenantGroupedUsers')
     * @param {string} options.searchInputSelector - Selector for search input (e.g., '#bulkUserSearch')
     * @param {string} options.selectAllSelector - Selector for global select all button
     * @param {string} options.deselectAllSelector - Selector for global deselect all button
     * @param {string} options.selectedCountSelector - Selector for selected count display
     * @param {string} options.loadingSelector - Selector for loading indicator
     * @param {string} options.emptyStateSelector - Selector for empty state message
     * @param {string} options.checkboxName - Name attribute for user checkboxes (for form submission)
     * @param {Function} options.onSelectionChanged - Optional callback when selection changes
     * @returns {Object} Public API for this instance
     */
    window.initBulkUserSelection = function (options) {
        // Validate required options
        if (!options.managerId || !options.ajaxEndpoint || !options.containerSelector) {
            console.error('initBulkUserSelection: managerId, ajaxEndpoint, and containerSelector are required');
            return null;
        }

        // State
        var selectedUserIds = new Set();
        var allUsers = [];
        var currentSearchQuery = '';

        // Selectors (with defaults)
        var $container = $(options.containerSelector);
        var $searchInput = $(options.searchInputSelector || '#bulkUserSearch_' + options.managerId);
        var $searchBtn = $('#searchUsersBtn_' + options.managerId);
        var $clearSearchBtn = $('#clearSearchBtn_' + options.managerId);
        var $selectAllBtn = $(options.selectAllSelector || '#selectAllUsers_' + options.managerId);
        var $deselectAllBtn = $(options.deselectAllSelector || '#deselectAllUsers_' + options.managerId);
        var $selectedCount = $(options.selectedCountSelector || '#selectedCount_' + options.managerId);
        var $loading = $(options.loadingSelector || '#loadingUsers_' + options.managerId);
        var $emptyState = $(options.emptyStateSelector || '#emptyState_' + options.managerId);

        var checkboxName = options.checkboxName || 'SelectedUserIds';

        /**
         * Load users from server via AJAX
         */
        function loadUsers(searchQuery) {
            searchQuery = searchQuery || '';
            currentSearchQuery = searchQuery;

            // Show/hide clear button based on search query
            if (searchQuery) {
                $clearSearchBtn.show();
            } else {
                $clearSearchBtn.hide();
            }

            $loading.show();
            $container.hide();
            $emptyState.hide();

            $.ajax({
                url: options.ajaxEndpoint,
                type: 'GET',
                data: { search: searchQuery },
                success: function (tenantGroups) {
                    if (tenantGroups.error) {
                        alert('Error loading users: ' + tenantGroups.error);
                        $loading.hide();
                        return;
                    }

                    // Flatten all users for easy access
                    allUsers = [];
                    tenantGroups.forEach(function (group) {
                        group.users.forEach(function (user) {
                            allUsers.push({
                                userId: user.userId,
                                fullName: user.fullName,
                                email: user.email,
                                employeeNumber: user.employeeNumber,
                                departmentName: user.departmentName,
                                tenantId: group.tenantId,
                                tenantName: group.tenantName
                            });
                        });
                    });

                    renderTenantGroups(tenantGroups);
                    $loading.hide();

                    if (tenantGroups.length === 0) {
                        $emptyState.show();
                    } else {
                        $container.show();
                    }
                },
                error: function () {
                    alert('Failed to load users. Please try again.');
                    $loading.hide();
                }
            });
        }

        /**
         * Render tenant groups as accordions
         */
        function renderTenantGroups(tenantGroups) {
            $container.empty();

            if (tenantGroups.length === 0) {
                return;
            }

            var accordionId = 'tenantAccordion_' + options.managerId;
            var $accordion = $('<div class="accordion" id="' + accordionId + '"></div>');

            tenantGroups.forEach(function (group, index) {
                var isFirstOpen = index === 0;
                var collapseId = 'collapse_' + options.managerId + '_' + group.tenantId;
                var selectedInTenant = group.users.filter(function(u) {
                    return selectedUserIds.has(u.userId);
                }).length;

                var $accordionItem = $('<div class="accordion-item"></div>');

                // Accordion Header
                var $header = $('<h2 class="accordion-header"></h2>');
                var $button = $('<button class="accordion-button ' + (isFirstOpen ? '' : 'collapsed') + '" type="button" data-bs-toggle="collapse" data-bs-target="#' + collapseId + '"></button>')
                    .html('<div class="d-flex justify-content-between align-items-center w-100 pe-3">' +
                        '<div><i class="ri-community-line me-2"></i><strong>' + group.tenantName + '</strong></div>' +
                        '<div><span class="badge bg-secondary tenant-badge-' + group.tenantId + '">' + selectedInTenant + '/' + group.userCount + '</span></div>' +
                        '</div>');
                $header.append($button);

                // Accordion Body
                var $collapse = $('<div id="' + collapseId + '" class="accordion-collapse collapse ' + (isFirstOpen ? 'show' : '') + '" data-bs-parent="#' + accordionId + '"></div>');
                var $body = $('<div class="accordion-body p-2"></div>');

                // Select All in Tenant button
                var $selectAllBtn = $('<button type="button" class="btn btn-sm btn-outline-primary mb-2 select-all-tenant-btn" data-tenant-id="' + group.tenantId + '">' +
                    '<i class="ri-checkbox-multiple-line me-1"></i>Select All in ' + group.tenantName +
                    '</button>');
                $body.append($selectAllBtn);

                // User Cards
                group.users.forEach(function (user) {
                    var isSelected = selectedUserIds.has(user.userId);
                    var $userCard = $('<div class="user-card ' + (isSelected ? 'selected' : '') + '" data-user-id="' + user.userId + '" data-tenant-id="' + group.tenantId + '"></div>')
                        .html('<div class="d-flex align-items-start gap-3">' +
                            '<input type="checkbox" class="form-check-input user-checkbox mt-1" name="' + checkboxName + '" value="' + user.userId + '" ' + (isSelected ? 'checked' : '') + '>' +
                            '<div class="flex-grow-1">' +
                            '<div class="fw-bold">' + user.fullName + '</div>' +
                            '<small class="text-muted d-block"><i class="ri-user-line me-1"></i>' + user.employeeNumber + '</small>' +
                            '<small class="text-muted"><i class="ri-building-line me-1"></i>' + user.departmentName + '</small>' +
                            '</div>' +
                            '</div>');
                    $body.append($userCard);
                });

                $collapse.append($body);
                $accordionItem.append($header, $collapse);
                $accordion.append($accordionItem);
            });

            $container.append($accordion);
        }

        /**
         * Update UI based on current selections
         */
        function updateUI() {
            // Update count badge
            var countText = selectedUserIds.size + ' user' + (selectedUserIds.size !== 1 ? 's' : '') + ' selected';
            $selectedCount.html('<i class="ri-user-line me-1"></i>' + countText);

            // Update user cards
            $('.user-card').each(function () {
                var userId = parseInt($(this).data('user-id'));
                var isSelected = selectedUserIds.has(userId);

                $(this).toggleClass('selected', isSelected);
                $(this).find('.user-checkbox').prop('checked', isSelected);
            });

            // Update tenant badges
            var tenantGroups = {};
            allUsers.forEach(function (user) {
                if (!tenantGroups[user.tenantId]) {
                    tenantGroups[user.tenantId] = { total: 0, selected: 0 };
                }
                tenantGroups[user.tenantId].total++;
                if (selectedUserIds.has(user.userId)) {
                    tenantGroups[user.tenantId].selected++;
                }
            });

            Object.keys(tenantGroups).forEach(function (tenantId) {
                $('.tenant-badge-' + tenantId).text(tenantGroups[tenantId].selected + '/' + tenantGroups[tenantId].total);
            });

            // Call optional callback
            if (options.onSelectionChanged) {
                options.onSelectionChanged(getSelectedUsers());
            }
        }

        /**
         * Get selected users with full details
         */
        function getSelectedUsers() {
            return Array.from(selectedUserIds).map(function (userId) {
                return allUsers.find(function(u) { return u.userId === userId; });
            }).filter(Boolean);
        }

        // ========================================
        // Event Handlers
        // ========================================

        // Search button click handler
        $searchBtn.on('click', function () {
            var query = $searchInput.val().trim();
            loadUsers(query);
        });

        // Clear search button handler
        $clearSearchBtn.on('click', function () {
            $searchInput.val('');
            loadUsers('');
        });

        // Enter key in search box triggers search
        $searchInput.on('keydown', function (e) {
            if (e.key === 'Enter' || e.keyCode === 13) {
                e.preventDefault();
                var query = $(this).val().trim();
                loadUsers(query);
                return false;
            }
        });

        // Select all users
        $selectAllBtn.on('click', function () {
            allUsers.forEach(function (user) {
                selectedUserIds.add(user.userId);
            });
            updateUI();
        });

        // Deselect all users
        $deselectAllBtn.on('click', function () {
            selectedUserIds.clear();
            updateUI();
        });

        // Select all in tenant (delegated event)
        $container.on('click', '.select-all-tenant-btn', function () {
            var tenantId = $(this).data('tenant-id');
            var tenantUsers = allUsers.filter(function(u) { return u.tenantId == tenantId; });
            tenantUsers.forEach(function (user) {
                selectedUserIds.add(user.userId);
            });
            updateUI();
        });

        // Toggle user selection (delegated event)
        $container.on('click', '.user-card', function () {
            var userId = parseInt($(this).data('user-id'));

            if (selectedUserIds.has(userId)) {
                selectedUserIds.delete(userId);
            } else {
                selectedUserIds.add(userId);
            }

            updateUI();
        });

        // User checkbox change (delegated event)
        $container.on('change', '.user-checkbox', function (e) {
            e.stopPropagation(); // Prevent double toggle from card click
            var userId = parseInt($(this).closest('.user-card').data('user-id'));

            if ($(this).is(':checked')) {
                selectedUserIds.add(userId);
            } else {
                selectedUserIds.delete(userId);
            }

            updateUI();
        });

        // ========================================
        // Initialization
        // ========================================
        loadUsers();

        // ========================================
        // Public API
        // ========================================
        return {
            loadUsers: loadUsers,
            getSelectedUsers: getSelectedUsers,
            getSelectedUserIds: function() { return Array.from(selectedUserIds); },
            clearSelection: function() {
                selectedUserIds.clear();
                updateUI();
            },
            setSelection: function(userIds) {
                selectedUserIds.clear();
                userIds.forEach(function(id) { selectedUserIds.add(id); });
                updateUI();
            }
        };
    };

})(window, jQuery);
