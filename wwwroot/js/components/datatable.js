/**
 * DataTable Component JavaScript
 * Handles sorting, bulk actions, view toggle, and AJAX loading
 */

(function($) {
    'use strict';

    // ============================================================================
    // DATATABLE INITIALIZATION
    // ============================================================================

    function initDataTable(tableId) {
        var table = $('#' + tableId);
        if (!table.length) return;

        var tbody = table.find('tbody');
        var headers = table.find('thead th.sortable');
        var config = getTableConfig(table);

        // Initialize features based on config
        initSorting(table, tbody, headers, config);
        
        if (config.enableBulkActions) {
            initBulkActions(table, tbody, tableId);
        }

        if (config.enableViewToggle) {
            initViewToggle(tableId);
        }

        if (config.enableAjaxLoading) {
            initAjaxLoading(table, tbody, tableId, config);
        }
    }

    function getTableConfig(table) {
        // Helper to parse boolean from data attribute (handles "true"/"false" strings)
        function parseBool(val) {
            return val === true || val === 'true' || val === 'True';
        }

        // Helper to parse JSON from data attribute
        function parseJson(val, defaultVal) {
            if (!val) return defaultVal;
            if (typeof val === 'object') return val; // Already parsed by jQuery
            try {
                return JSON.parse(val);
            } catch (e) {
                return defaultVal;
            }
        }

        return {
            enableBulkActions: parseBool(table.data('bulk-actions')),
            enableViewToggle: parseBool(table.data('view-toggle')),
            enableAjaxLoading: parseBool(table.data('ajax-enabled')),
            ajaxUrl: table.data('ajax-url') || '',
            ajaxParams: parseJson(table.data('ajax-params'), {}),
            ajaxColumns: parseJson(table.data('ajax-columns'), []),
            pageSize: parseInt(table.data('page-size'), 10) || 10,
            skeletonRowCount: parseInt(table.data('skeleton-rows'), 10) || 5
        };
    }

    // ============================================================================
    // SORTING
    // ============================================================================

    function initSorting(table, tbody, headers, config) {
        headers.on('click', function() {
            var th = $(this);
            var columnIndex = th.index();

            // Adjust for checkbox column
            if (config.enableBulkActions) {
                columnIndex = columnIndex - 1;
            }

            var rows = tbody.find('tr:not(.skeleton-row)').toArray();
            var isAscending = th.hasClass('sort-asc');

            // Remove sorting classes and reset icons for all headers
            headers.removeClass('sort-asc sort-desc');
            headers.find('i').attr('class', 'ri-arrow-up-down-line ms-1 text-muted fs-14');

            // Sort rows
            rows.sort(function(a, b) {
                var aValue = $(a).find('td').eq(columnIndex).text().trim();
                var bValue = $(b).find('td').eq(columnIndex).text().trim();

                // Remove common formatting
                aValue = aValue.replace(/[$,]/g, '');
                bValue = bValue.replace(/[$,]/g, '');

                // Try numbers
                var aNum = parseFloat(aValue);
                var bNum = parseFloat(bValue);

                if (!isNaN(aNum) && !isNaN(bNum)) {
                    return isAscending ? (bNum - aNum) : (aNum - bNum);
                }

                // Try dates
                var aDate = new Date(aValue);
                var bDate = new Date(bValue);

                if (!isNaN(aDate.getTime()) && !isNaN(bDate.getTime())) {
                    return isAscending ? (bDate - aDate) : (aDate - bDate);
                }

                // String comparison
                return isAscending
                    ? bValue.localeCompare(aValue, undefined, { numeric: true, sensitivity: 'base' })
                    : aValue.localeCompare(bValue, undefined, { numeric: true, sensitivity: 'base' });
            });

            // Update table
            tbody.find('tr:not(.skeleton-row)').remove();
            tbody.append(rows);

            // Update icon
            if (isAscending) {
                th.addClass('sort-desc');
                th.find('i').attr('class', 'ri-arrow-down-line ms-1 text-primary fs-14');
            } else {
                th.addClass('sort-asc');
                th.find('i').attr('class', 'ri-arrow-up-line ms-1 text-primary fs-14');
            }
        });
    }

    // ============================================================================
    // BULK ACTIONS
    // ============================================================================

    function initBulkActions(table, tbody, tableId) {
        var selectAllCheckbox = table.find('.select-all-checkbox');
        var bulkActionsDropdown = $('.bulk-actions-dropdown[data-table-id="' + tableId + '"]');
        var selectedCountSpan = bulkActionsDropdown.find('.selected-count');
        var minSelectionsForBulk = 2;

        function getRowCheckboxes() {
            return tbody.find('.row-select-checkbox');
        }

        function updateBulkActionsVisibility() {
            var rowCheckboxes = getRowCheckboxes();
            var checkedCount = rowCheckboxes.filter(':checked').length;
            selectedCountSpan.text(checkedCount);
            
            if (checkedCount >= minSelectionsForBulk) {
                bulkActionsDropdown.show();
            } else {
                bulkActionsDropdown.hide();
            }
            
            // Update select-all checkbox state
            var allChecked = rowCheckboxes.length > 0 && rowCheckboxes.length === checkedCount;
            selectAllCheckbox.prop('checked', allChecked);
            selectAllCheckbox.prop('indeterminate', checkedCount > 0 && !allChecked);
        }

        // Select all checkbox
        selectAllCheckbox.on('change', function() {
            var isChecked = $(this).prop('checked');
            getRowCheckboxes().prop('checked', isChecked);
            updateBulkActionsVisibility();
        });

        // Row checkbox changes (delegated for dynamic content)
        tbody.on('change', '.row-select-checkbox', function() {
            updateBulkActionsVisibility();
        });

        // Bulk action click
        bulkActionsDropdown.on('click', '.bulk-action-btn', function(e) {
            e.preventDefault();
            var btn = $(this);
            var url = btn.data('url');
            var needsConfirm = btn.data('confirm') === true || btn.data('confirm') === 'true';
            var message = btn.data('message') || 'Are you sure you want to perform this action?';

            var selectedIds = getRowCheckboxes().filter(':checked').map(function() {
                return $(this).val();
            }).get();

            if (selectedIds.length < minSelectionsForBulk) {
                alert('Please select at least ' + minSelectionsForBulk + ' items.');
                return;
            }

            if (needsConfirm && !confirm(message)) {
                return;
            }

            // Submit bulk action
            $.post(url, { ids: selectedIds }, function(response) {
                if (response.success) {
                    // Reload table if AJAX mode, otherwise reload page
                    if (typeof window['reloadTable_' + tableId] === 'function') {
                        window['reloadTable_' + tableId]();
                    } else {
                        location.reload();
                    }
                } else {
                    alert(response.message || 'Action failed');
                }
            }).fail(function(xhr) {
                alert('Error: ' + (xhr.responseJSON?.message || 'Action failed'));
            });
        });

        // Initial state
        updateBulkActionsVisibility();
    }

    // ============================================================================
    // VIEW TOGGLE
    // ============================================================================

    function initViewToggle(tableId) {
        var viewToggleBtns = $('.view-toggle-btn[data-table-id="' + tableId + '"]');
        var tableView = $('#' + tableId + '_tableView');
        var cardView = $('#' + tableId + '_cardView');
        var storageKey = 'dataTableViewPreference_' + tableId;

        // Restore saved preference
        var savedView = localStorage.getItem(storageKey);
        if (savedView) {
            switchView(savedView);
        }

        // Toggle click handler
        viewToggleBtns.on('click', function() {
            var view = $(this).data('view');
            switchView(view);
            localStorage.setItem(storageKey, view);
        });

        function switchView(view) {
            viewToggleBtns.removeClass('active');
            viewToggleBtns.filter('[data-view="' + view + '"]').addClass('active');

            if (view === 'table') {
                tableView.show();
                cardView.hide();
            } else {
                tableView.hide();
                cardView.show();
            }
        }
    }

    // ============================================================================
    // AJAX LOADING
    // ============================================================================

    function initAjaxLoading(table, tbody, tableId, config) {
        var ajaxUrl = config.ajaxUrl;
        var ajaxParams = config.ajaxParams;
        var ajaxColumns = config.ajaxColumns;
        var currentPage = 1;
        var pageSize = config.pageSize;
        var totalPages = 1;
        var totalRecords = 0;
        var currentFilters = {};

        // Load data on init
        loadAjaxData();

        // Expose reload function globally
        window['reloadTable_' + tableId] = loadAjaxData;

        function loadAjaxData() {
            // Show skeleton
            showSkeleton();

            // Build query params
            var params = $.extend({}, ajaxParams, currentFilters, {
                page: currentPage,
                pageSize: pageSize
            });

            $.get(ajaxUrl, params)
                .done(function(response) {
                    if (response.success) {
                        renderRows(response.data || []);
                        updatePagination(response.pagination || {});
                    } else {
                        showError(response.message || 'Failed to load data');
                    }
                })
                .fail(function(xhr) {
                    showError('Failed to load data. Please try again.');
                });
        }

        function showSkeleton() {
            var skeletonHtml = '';
            var colCount = ajaxColumns.length + (config.enableBulkActions ? 1 : 0);
            
            for (var i = 0; i < config.skeletonRowCount; i++) {
                skeletonHtml += '<tr class="skeleton-row">';
                if (config.enableBulkActions) {
                    skeletonHtml += '<td><div class="skeleton" style="width:16px;height:16px;border-radius:3px;"></div></td>';
                }
                ajaxColumns.forEach(function(col) {
                    if (col.displayType === 'avatar') {
                        skeletonHtml += '<td><div class="d-flex align-items-center gap-2"><div class="skeleton" style="width:32px;height:32px;border-radius:50%;"></div><div class="flex-grow-1"><div class="skeleton" style="height:14px;width:70%;margin-bottom:4px;border-radius:4px;"></div><div class="skeleton" style="height:10px;width:50%;border-radius:4px;"></div></div></div></td>';
                    } else if (col.displayType === 'badge') {
                        skeletonHtml += '<td><div class="skeleton" style="height:20px;width:70px;border-radius:10px;"></div></td>';
                    } else if (col.displayType === 'actions') {
                        skeletonHtml += '<td><div class="skeleton" style="height:28px;width:28px;border-radius:4px;"></div></td>';
                    } else {
                        skeletonHtml += '<td><div class="skeleton" style="height:14px;width:80%;border-radius:4px;"></div></td>';
                    }
                });
                skeletonHtml += '</tr>';
            }
            tbody.html(skeletonHtml);
        }

        function renderRows(data) {
            var colCount = ajaxColumns.length + (config.enableBulkActions ? 1 : 0);
            
            if (!data || data.length === 0) {
                tbody.html('<tr><td colspan="' + colCount + '" class="text-center py-5"><div class="empty-state"><div class="bg-light rounded-circle d-inline-flex align-items-center justify-content-center" style="width:80px;height:80px;"><i class="ri-inbox-line fs-1 text-muted"></i></div><h5 class="mt-3">No Data Found</h5><p class="text-muted">No records match your criteria.</p></div></td></tr>');
                return;
            }

            var html = '';
            data.forEach(function(row) {
                // Get row ID - check common ID field names
                var rowId = row.id || row.assignmentId || row.templateId || row.roleId || row.userId || '';
                html += '<tr data-id="' + rowId + '">';
                if (config.enableBulkActions) {
                    html += '<td><input type="checkbox" class="form-check-input row-select-checkbox" value="' + rowId + '" /></td>';
                }
                ajaxColumns.forEach(function(col) {
                    html += renderCell(row, col, rowId);
                });
                html += '</tr>';
            });
            tbody.html(html);

            // Reset bulk actions
            table.find('.select-all-checkbox').prop('checked', false);
        }

        function renderCell(row, col, rowId) {
            var value = row[col.fieldName] || '';
            var secondary = col.secondaryField ? (row[col.secondaryField] || '') : '';
            var color = col.badgeColorField ? (row[col.badgeColorField] || 'secondary') : 'secondary';
            var icon = col.iconField ? (row[col.iconField] || '') : '';

            switch (col.displayType) {
                case 'checkbox':
                    return '<td><input type="checkbox" class="form-check-input row-select-checkbox" value="' + value + '" /></td>';
                case 'avatar':
                    return '<td><div class="d-flex align-items-center gap-2"><div class="avatar-xs bg-' + color + '-subtle text-' + color + ' rounded-circle d-flex align-items-center justify-content-center" style="width:32px;height:32px;"><i class="' + icon + '"></i></div><div><div class="fw-medium">' + escapeHtml(value) + '</div><small class="text-muted">' + escapeHtml(secondary) + '</small></div></div></td>';
                case 'badge':
                    return '<td><span class="badge bg-' + color + '-subtle text-' + color + '">' + escapeHtml(value) + '</span></td>';
                case 'date':
                    return '<td>' + formatDate(value) + '</td>';
                case 'actions':
                    return renderActionsCell(row, col.actions || [], rowId);
                default:
                    if (secondary) {
                        return '<td><div>' + escapeHtml(value) + '</div><small class="text-muted">' + escapeHtml(secondary) + '</small></td>';
                    }
                    return '<td>' + escapeHtml(value) + '</td>';
            }
        }

        function renderActionsCell(row, actions, rowId) {
            var html = '<td><div class="dropdown"><button class="btn btn-soft-secondary btn-sm dropdown-toggle" type="button" data-bs-toggle="dropdown"><i class="ri-more-2-fill"></i></button><ul class="dropdown-menu dropdown-menu-end">';
            
            actions.forEach(function(action) {
                if (action.isDivider) {
                    html += '<li><hr class="dropdown-divider"></li>';
                } else {
                    var url = action.url ? action.url.replace(/\{id\}/g, rowId) : '#';
                    var onclick = action.onClick ? action.onClick.replace(/\{id\}/g, rowId) : '';
                    var colorClass = action.colorClass === 'danger' ? ' text-danger' : '';
                    html += '<li><a class="dropdown-item' + colorClass + '" href="' + url + '"' + (onclick ? ' onclick="' + onclick + '; return false;"' : '') + '><i class="' + action.iconClass + ' me-2"></i>' + action.text + '</a></li>';
                }
            });
            
            html += '</ul></div></td>';
            return html;
        }

        function updatePagination(pagination) {
            currentPage = pagination.page || pagination.currentPage || 1;
            totalPages = pagination.totalPages || 1;
            totalRecords = pagination.totalCount || pagination.totalRecords || 0;

            // Update info text
            var paginationInfo = $('#' + tableId + '_paginationInfo');
            paginationInfo.find('.ajax-current-page').text(currentPage);
            paginationInfo.find('.ajax-total-pages').text(totalPages);
            paginationInfo.find('.ajax-total-records').text(totalRecords);

            // Build pagination controls
            var controls = $('#' + tableId + '_paginationControls');
            var html = '';

            // Previous
            html += '<li class="page-item ' + (currentPage === 1 ? 'disabled' : '') + '"><a class="page-link ajax-page-link" href="#" data-page="' + (currentPage - 1) + '"><i class="ri-arrow-left-s-line"></i></a></li>';

            // Page numbers
            var startPage = Math.max(1, currentPage - 2);
            var endPage = Math.min(totalPages, currentPage + 2);

            if (startPage > 1) {
                html += '<li class="page-item"><a class="page-link ajax-page-link" href="#" data-page="1">1</a></li>';
                if (startPage > 2) html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }

            for (var i = startPage; i <= endPage; i++) {
                html += '<li class="page-item ' + (i === currentPage ? 'active' : '') + '"><a class="page-link ajax-page-link" href="#" data-page="' + i + '">' + i + '</a></li>';
            }

            if (endPage < totalPages) {
                if (endPage < totalPages - 1) html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
                html += '<li class="page-item"><a class="page-link ajax-page-link" href="#" data-page="' + totalPages + '">' + totalPages + '</a></li>';
            }

            // Next
            html += '<li class="page-item ' + (currentPage >= totalPages ? 'disabled' : '') + '"><a class="page-link ajax-page-link" href="#" data-page="' + (currentPage + 1) + '"><i class="ri-arrow-right-s-line"></i></a></li>';

            controls.html(html);

            // Bind page click events
            controls.find('.ajax-page-link').on('click', function(e) {
                e.preventDefault();
                var page = parseInt($(this).data('page'), 10);
                if (page >= 1 && page <= totalPages && page !== currentPage) {
                    currentPage = page;
                    loadAjaxData();
                }
            });
        }

        function showError(message) {
            var colCount = ajaxColumns.length + (config.enableBulkActions ? 1 : 0);
            tbody.html('<tr><td colspan="' + colCount + '" class="text-center py-4"><div class="text-danger"><i class="ri-error-warning-line fs-2"></i><p class="mt-2">' + message + '</p><button class="btn btn-sm btn-outline-primary" onclick="window[\'reloadTable_' + tableId + '\']()"><i class="ri-refresh-line me-1"></i>Retry</button></div></td></tr>');
        }

        // Filter change handlers for AJAX mode
        $('[data-ajax-filter-for="' + tableId + '"]').on('change', function() {
            var filterName = $(this).data('filter-name');
            var filterValue = $(this).val();
            if (filterValue) {
                currentFilters[filterName] = filterValue;
            } else {
                delete currentFilters[filterName];
            }
            currentPage = 1;
            loadAjaxData();
        });

        // Search handler for AJAX mode
        $('[data-ajax-search-for="' + tableId + '"]').on('keypress', function(e) {
            if (e.which === 13) {
                currentFilters.search = $(this).val();
                currentPage = 1;
                loadAjaxData();
            }
        });
    }

    // ============================================================================
    // UTILITY FUNCTIONS
    // ============================================================================

    function escapeHtml(text) {
        if (!text) return '';
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function formatDate(dateStr) {
        if (!dateStr) return '-';
        var date = new Date(dateStr);
        if (isNaN(date.getTime())) return dateStr;
        var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        return months[date.getMonth()] + ' ' + date.getDate().toString().padStart(2, '0') + ', ' + date.getFullYear();
    }

    // ============================================================================
    // AUTO-INITIALIZE
    // ============================================================================

    $(document).ready(function() {
        // Initialize all data tables on the page
        $('table[data-datatable="true"]').each(function() {
            initDataTable($(this).attr('id'));
        });
    });

    // Expose for manual initialization
    window.DataTableComponent = {
        init: initDataTable
    };

})(jQuery);
