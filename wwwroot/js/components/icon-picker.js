/**
 * Icon Picker Component
 * Provides live preview and modal selection for Remix Icons
 */
const IconPicker = (function() {
    'use strict';

    // Curated list of commonly used Remix Icons for forms/categories
    const iconCategories = {
        'Documents & Files': [
            'ri-file-line', 'ri-file-list-line', 'ri-file-list-2-line', 'ri-file-list-3-line',
            'ri-file-text-line', 'ri-file-copy-line', 'ri-file-edit-line', 'ri-file-chart-line',
            'ri-folder-line', 'ri-folder-open-line', 'ri-folder-add-line', 'ri-folder-chart-line',
            'ri-draft-line', 'ri-article-line', 'ri-newspaper-line', 'ri-book-line',
            'ri-book-open-line', 'ri-clipboard-line', 'ri-survey-line', 'ri-task-line'
        ],
        'Business & Finance': [
            'ri-briefcase-line', 'ri-building-line', 'ri-building-2-line', 'ri-bank-line',
            'ri-money-dollar-circle-line', 'ri-wallet-line', 'ri-coin-line', 'ri-funds-line',
            'ri-stock-line', 'ri-bar-chart-line', 'ri-bar-chart-box-line', 'ri-pie-chart-line',
            'ri-line-chart-line', 'ri-bubble-chart-line', 'ri-calculator-line', 'ri-percent-line'
        ],
        'Communication': [
            'ri-mail-line', 'ri-mail-send-line', 'ri-inbox-line', 'ri-chat-1-line',
            'ri-message-line', 'ri-discuss-line', 'ri-question-answer-line', 'ri-feedback-line',
            'ri-phone-line', 'ri-customer-service-line', 'ri-notification-line', 'ri-alarm-line'
        ],
        'Users & Teams': [
            'ri-user-line', 'ri-user-add-line', 'ri-user-settings-line', 'ri-user-star-line',
            'ri-group-line', 'ri-team-line', 'ri-admin-line', 'ri-user-2-line',
            'ri-contacts-line', 'ri-account-circle-line', 'ri-account-box-line', 'ri-profile-line'
        ],
        'Security & Safety': [
            'ri-shield-line', 'ri-shield-check-line', 'ri-shield-star-line', 'ri-shield-user-line',
            'ri-lock-line', 'ri-lock-password-line', 'ri-key-line', 'ri-key-2-line',
            'ri-safe-line', 'ri-alarm-warning-line', 'ri-error-warning-line', 'ri-fire-line',
            'ri-first-aid-kit-line', 'ri-heart-pulse-line', 'ri-hospital-line', 'ri-nurse-line'
        ],
        'Tools & Settings': [
            'ri-settings-line', 'ri-settings-2-line', 'ri-settings-3-line', 'ri-tools-line',
            'ri-hammer-line', 'ri-screwdriver-line', 'ri-wrench-line', 'ri-paint-brush-line',
            'ri-magic-line', 'ri-equalizer-line', 'ri-filter-line', 'ri-filter-2-line',
            'ri-dashboard-line', 'ri-apps-line', 'ri-grid-line', 'ri-layout-line'
        ],
        'Status & Actions': [
            'ri-check-line', 'ri-check-double-line', 'ri-checkbox-circle-line', 'ri-close-line',
            'ri-add-line', 'ri-subtract-line', 'ri-edit-line', 'ri-delete-bin-line',
            'ri-refresh-line', 'ri-loop-left-line', 'ri-time-line', 'ri-calendar-line',
            'ri-calendar-check-line', 'ri-calendar-todo-line', 'ri-flag-line', 'ri-bookmark-line'
        ],
        'Navigation & UI': [
            'ri-home-line', 'ri-menu-line', 'ri-more-line', 'ri-arrow-right-line',
            'ri-arrow-left-line', 'ri-arrow-up-line', 'ri-arrow-down-line', 'ri-search-line',
            'ri-zoom-in-line', 'ri-zoom-out-line', 'ri-eye-line', 'ri-eye-off-line',
            'ri-fullscreen-line', 'ri-external-link-line', 'ri-link', 'ri-unlink'
        ],
        'Media': [
            'ri-image-line', 'ri-image-2-line', 'ri-gallery-line', 'ri-camera-line',
            'ri-video-line', 'ri-film-line', 'ri-music-line', 'ri-headphone-line',
            'ri-mic-line', 'ri-volume-up-line', 'ri-download-line', 'ri-upload-line',
            'ri-attachment-line', 'ri-print-line', 'ri-qr-code-line', 'ri-barcode-line'
        ],
        'Logistics & Location': [
            'ri-truck-line', 'ri-car-line', 'ri-bus-line', 'ri-ship-line',
            'ri-plane-line', 'ri-road-map-line', 'ri-map-pin-line', 'ri-compass-line',
            'ri-route-line', 'ri-parking-line', 'ri-gas-station-line', 'ri-store-line',
            'ri-shopping-cart-line', 'ri-shopping-bag-line', 'ri-box-1-line', 'ri-archive-line'
        ],
        'Nature & Weather': [
            'ri-plant-line', 'ri-leaf-line', 'ri-seedling-line', 'ri-tree-line',
            'ri-sun-line', 'ri-moon-line', 'ri-cloud-line', 'ri-rainy-line',
            'ri-thunderstorms-line', 'ri-snowy-line', 'ri-water-flash-line', 'ri-drop-line',
            'ri-temp-hot-line', 'ri-temp-cold-line', 'ri-fire-line', 'ri-earth-line'
        ],
        'Miscellaneous': [
            'ri-lightbulb-line', 'ri-flashlight-line', 'ri-trophy-line', 'ri-medal-line',
            'ri-gift-line', 'ri-cake-line', 'ri-heart-line', 'ri-star-line',
            'ri-thumb-up-line', 'ri-thumb-down-line', 'ri-emotion-happy-line', 'ri-emotion-sad-line',
            'ri-question-line', 'ri-information-line', 'ri-spam-line', 'ri-bug-line'
        ]
    };

    let currentTargetInput = null;
    let currentTargetPreview = null;

    /**
     * Initialize the icon picker
     */
    function init() {
        setupLivePreview();
        setupModalTriggers();
        createModal();
    }

    /**
     * Setup live preview - update icon as user types
     */
    function setupLivePreview() {
        document.addEventListener('input', function(e) {
            if (e.target.classList.contains('icon-picker-input')) {
                const previewId = e.target.dataset.iconPreview;
                const previewEl = document.getElementById(previewId);
                if (previewEl) {
                    updatePreview(previewEl, e.target.value);
                }
            }
        });
    }

    /**
     * Update icon preview element
     */
    function updatePreview(previewEl, iconClass) {
        if (iconClass && iconClass.startsWith('ri-')) {
            previewEl.className = iconClass;
        } else {
            previewEl.className = 'ri-image-line text-muted';
        }
    }

    /**
     * Setup modal trigger buttons
     */
    function setupModalTriggers() {
        document.addEventListener('click', function(e) {
            const btn = e.target.closest('.icon-picker-btn');
            if (btn) {
                currentTargetInput = document.getElementById(btn.dataset.targetInput);
                currentTargetPreview = document.getElementById(btn.dataset.targetPreview);

                // Highlight current selection in modal
                highlightCurrentSelection();
            }
        });
    }

    /**
     * Create the icon picker modal
     */
    function createModal() {
        // Check if modal already exists
        if (document.getElementById('iconPickerModal')) return;

        const modalHtml = `
            <div class="modal fade" id="iconPickerModal" tabindex="-1" aria-labelledby="iconPickerModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-scrollable">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="iconPickerModalLabel">
                                <i class="ri-palette-line me-2"></i>Select Icon
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <!-- Search -->
                            <div class="mb-3">
                                <div class="input-group">
                                    <span class="input-group-text"><i class="ri-search-line"></i></span>
                                    <input type="text" class="form-control" id="iconSearchInput"
                                           placeholder="Search icons..." autocomplete="off">
                                </div>
                            </div>

                            <!-- Selected Preview -->
                            <div class="mb-3 p-3 bg-light rounded d-flex align-items-center justify-content-between" id="iconPreviewSection" style="display: none !important;">
                                <div class="d-flex align-items-center">
                                    <div class="avatar-sm me-3">
                                        <div class="avatar-title bg-primary text-white rounded fs-4" id="modalIconPreview">
                                            <i class="ri-image-line"></i>
                                        </div>
                                    </div>
                                    <div>
                                        <strong>Selected:</strong>
                                        <code id="modalIconClass">None</code>
                                    </div>
                                </div>
                                <button type="button" class="btn btn-sm btn-outline-danger" id="clearIconSelection">
                                    <i class="ri-close-line"></i> Clear
                                </button>
                            </div>

                            <!-- Icon Grid -->
                            <div id="iconGridContainer">
                                ${generateIconGrid()}
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary" id="selectIconBtn">
                                <i class="ri-check-line me-1"></i>Select Icon
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', modalHtml);
        setupModalEvents();
    }

    /**
     * Generate the icon grid HTML
     */
    function generateIconGrid() {
        let html = '';

        for (const [category, icons] of Object.entries(iconCategories)) {
            html += `
                <div class="icon-category mb-3">
                    <h6 class="text-muted mb-2">${category}</h6>
                    <div class="row g-2">
            `;

            for (const icon of icons) {
                html += `
                    <div class="col-auto icon-item" data-icon="${icon}">
                        <button type="button" class="btn btn-outline-secondary icon-select-btn p-2"
                                data-icon="${icon}" title="${icon}">
                            <i class="${icon}" style="font-size: 1.25rem;"></i>
                        </button>
                    </div>
                `;
            }

            html += `
                    </div>
                </div>
            `;
        }

        return html;
    }

    /**
     * Setup modal event handlers
     */
    function setupModalEvents() {
        const modal = document.getElementById('iconPickerModal');
        const searchInput = document.getElementById('iconSearchInput');
        const selectBtn = document.getElementById('selectIconBtn');
        const clearBtn = document.getElementById('clearIconSelection');

        // Search functionality
        searchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            const iconItems = modal.querySelectorAll('.icon-item');
            const categories = modal.querySelectorAll('.icon-category');

            iconItems.forEach(item => {
                const iconName = item.dataset.icon.toLowerCase();
                item.style.display = iconName.includes(searchTerm) ? '' : 'none';
            });

            // Hide empty categories
            categories.forEach(cat => {
                const visibleIcons = cat.querySelectorAll('.icon-item:not([style*="display: none"])');
                cat.style.display = visibleIcons.length > 0 ? '' : 'none';
            });
        });

        // Icon selection
        modal.addEventListener('click', function(e) {
            const btn = e.target.closest('.icon-select-btn');
            if (btn) {
                // Remove previous selection
                modal.querySelectorAll('.icon-select-btn.active').forEach(b => b.classList.remove('active'));

                // Add selection to clicked button
                btn.classList.add('active');

                // Update preview
                const iconClass = btn.dataset.icon;
                document.getElementById('modalIconPreview').innerHTML = `<i class="${iconClass}"></i>`;
                document.getElementById('modalIconClass').textContent = iconClass;
                document.getElementById('iconPreviewSection').style.display = 'flex';
            }
        });

        // Select button
        selectBtn.addEventListener('click', function() {
            const selectedBtn = modal.querySelector('.icon-select-btn.active');
            if (selectedBtn && currentTargetInput) {
                const iconClass = selectedBtn.dataset.icon;
                currentTargetInput.value = iconClass;

                if (currentTargetPreview) {
                    updatePreview(currentTargetPreview, iconClass);
                }

                // Trigger input event for any listeners
                currentTargetInput.dispatchEvent(new Event('input', { bubbles: true }));
            }

            // Close modal
            bootstrap.Modal.getInstance(modal).hide();
        });

        // Clear button
        clearBtn.addEventListener('click', function() {
            modal.querySelectorAll('.icon-select-btn.active').forEach(b => b.classList.remove('active'));
            document.getElementById('modalIconPreview').innerHTML = '<i class="ri-image-line"></i>';
            document.getElementById('modalIconClass').textContent = 'None';
            document.getElementById('iconPreviewSection').style.display = 'none';
        });

        // Reset on modal close
        modal.addEventListener('hidden.bs.modal', function() {
            searchInput.value = '';
            searchInput.dispatchEvent(new Event('input'));
            modal.querySelectorAll('.icon-select-btn.active').forEach(b => b.classList.remove('active'));
            document.getElementById('iconPreviewSection').style.display = 'none';
        });
    }

    /**
     * Highlight current selection when modal opens
     */
    function highlightCurrentSelection() {
        const modal = document.getElementById('iconPickerModal');
        if (!modal || !currentTargetInput) return;

        const currentValue = currentTargetInput.value;

        // Clear previous selections
        modal.querySelectorAll('.icon-select-btn.active').forEach(b => b.classList.remove('active'));

        if (currentValue) {
            const matchingBtn = modal.querySelector(`.icon-select-btn[data-icon="${currentValue}"]`);
            if (matchingBtn) {
                matchingBtn.classList.add('active');
                document.getElementById('modalIconPreview').innerHTML = `<i class="${currentValue}"></i>`;
                document.getElementById('modalIconClass').textContent = currentValue;
                document.getElementById('iconPreviewSection').style.display = 'flex';
            }
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Public API
    return {
        init: init,
        updatePreview: updatePreview
    };
})();
