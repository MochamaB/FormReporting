/**
 * Form Builder - Canvas Toolbar
 * Handles toolbar actions: section navigation, zoom, collapse/expand, preview
 */

const FormBuilderToolbar = {
    // Current zoom level (percentage)
    zoomLevel: 100,
    zoomLevels: [75, 90, 100, 110, 125],
    
    // Section observer for tracking visible section
    sectionObserver: null,

    /**
     * Initialize the toolbar
     */
    init: function() {
        console.log('🔧 Initializing Canvas Toolbar');
        
        this.bindEvents();
        this.updateSectionIndicators();
        this.setupSectionObserver();
        this.updateZoomDisplay();
    },

    /**
     * Bind all toolbar button events
     */
    bindEvents: function() {
        // Zoom controls
        document.getElementById('btnZoomIn')?.addEventListener('click', () => this.zoomIn());
        document.getElementById('btnZoomOut')?.addEventListener('click', () => this.zoomOut());
        document.getElementById('btnZoomReset')?.addEventListener('click', () => this.zoomReset());

        // Section actions
        document.getElementById('btnCollapseAll')?.addEventListener('click', () => this.collapseAllSections());
        document.getElementById('btnExpandAll')?.addEventListener('click', () => this.expandAllSections());

        // Quick actions
        document.getElementById('btnAddSection')?.addEventListener('click', () => this.addSection());
        document.getElementById('btnPreviewForm')?.addEventListener('click', () => this.previewForm());

        // Undo/Redo (placeholder for future implementation)
        document.getElementById('btnUndo')?.addEventListener('click', () => this.undo());
        document.getElementById('btnRedo')?.addEventListener('click', () => this.redo());

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => this.handleKeyboardShortcuts(e));

        // Listen for section changes to update indicators
        document.addEventListener('sectionAdded', () => this.updateSectionIndicators());
        document.addEventListener('sectionDeleted', () => this.updateSectionIndicators());
        document.addEventListener('sectionReordered', () => this.updateSectionIndicators());
    },

    // ========================================================================
    // SECTION NAVIGATION
    // ========================================================================

    /**
     * Update section indicators and dropdown based on current sections
     */
    updateSectionIndicators: function() {
        this.updateSectionDots();
        this.updateSectionDropdown();
    },

    /**
     * Update section indicator dots
     */
    updateSectionDots: function() {
        const container = document.getElementById('sectionIndicators');
        const template = document.getElementById('sectionIndicatorTemplate');
        
        if (!container || !template) return;

        const sections = document.querySelectorAll('.builder-section');
        
        // Remove existing indicators
        container.querySelectorAll('.section-indicator').forEach(el => el.remove());

        if (sections.length === 0) {
            return;
        }

        // Create indicator for each section using template
        sections.forEach((section, index) => {
            const sectionId = section.dataset.sectionId;
            const sectionName = section.querySelector('.section-title')?.textContent?.trim() || `Section ${index + 1}`;
            
            // Clone template
            const clone = template.content.cloneNode(true);
            const indicator = clone.querySelector('.section-indicator');
            
            // Set data attributes
            indicator.dataset.sectionId = sectionId;
            indicator.dataset.sectionName = sectionName;
            indicator.title = sectionName;
            
            // Set tooltip text
            const tooltip = indicator.querySelector('.indicator-tooltip');
            if (tooltip) {
                tooltip.textContent = sectionName;
            }
            
            // Add click handler
            indicator.addEventListener('click', () => this.scrollToSection(sectionId));
            
            container.appendChild(indicator);
        });

        // Mark the first one as active initially
        this.updateActiveIndicator();
    },

    /**
     * Update section dropdown menu
     */
    updateSectionDropdown: function() {
        const menu = document.getElementById('sectionDropdownMenu');
        const template = document.getElementById('sectionDropdownItemTemplate');
        const emptyItem = document.getElementById('sectionDropdownEmpty');
        
        if (!menu || !template) return;

        const sections = document.querySelectorAll('.builder-section');
        
        // Remove existing items (keep header, divider, and empty state)
        menu.querySelectorAll('.section-dropdown-item').forEach(el => el.closest('li').remove());

        if (sections.length === 0) {
            // Show empty state
            if (emptyItem) {
                emptyItem.style.display = 'block';
            }
            return;
        }

        // Hide empty state
        if (emptyItem) {
            emptyItem.style.display = 'none';
        }

        // Create dropdown item for each section
        sections.forEach((section, index) => {
            const sectionId = section.dataset.sectionId;
            const sectionName = section.querySelector('.section-title')?.textContent?.trim() || `Section ${index + 1}`;
            
            // Clone template
            const clone = template.content.cloneNode(true);
            const li = clone.querySelector('li');
            const item = clone.querySelector('.section-dropdown-item');
            const numberSpan = clone.querySelector('.section-dropdown-number');
            const nameSpan = clone.querySelector('.section-dropdown-name');
            
            // Set data and content
            item.dataset.sectionId = sectionId;
            numberSpan.textContent = index + 1;
            nameSpan.textContent = sectionName;
            
            // Add click handler
            item.addEventListener('click', (e) => {
                e.preventDefault();
                this.scrollToSection(sectionId);
                
                // Close dropdown
                const dropdown = bootstrap.Dropdown.getInstance(document.getElementById('sectionDropdownBtn'));
                if (dropdown) dropdown.hide();
            });
            
            menu.appendChild(li);
        });
    },

    /**
     * Scroll to a specific section
     * @param {string} sectionId - The section ID to scroll to
     */
    scrollToSection: function(sectionId) {
        const section = document.querySelector(`.builder-section[data-section-id="${sectionId}"]`);
        if (!section) return;

        const canvas = document.getElementById('designCanvas');
        if (canvas) {
            // Smooth scroll within the canvas
            section.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }

        // Highlight the section briefly
        section.classList.add('highlight-pulse');
        setTimeout(() => section.classList.remove('highlight-pulse'), 1000);

        // Update active indicator
        this.setActiveIndicator(sectionId);
    },

    /**
     * Set active indicator for a section
     * @param {string} sectionId - The section ID
     */
    setActiveIndicator: function(sectionId) {
        // Remove active from all dots
        document.querySelectorAll('.section-indicator').forEach(ind => {
            ind.classList.remove('active');
        });

        // Add active to current dot
        const indicator = document.querySelector(`.section-indicator[data-section-id="${sectionId}"]`);
        if (indicator) {
            indicator.classList.add('active');
        }

        // Update dropdown items
        document.querySelectorAll('.section-dropdown-item').forEach(item => {
            item.classList.remove('active');
        });
        const dropdownItem = document.querySelector(`.section-dropdown-item[data-section-id="${sectionId}"]`);
        if (dropdownItem) {
            dropdownItem.classList.add('active');
        }
    },

    /**
     * Update active indicator based on scroll position
     */
    updateActiveIndicator: function() {
        const canvas = document.getElementById('designCanvas');
        if (!canvas) return;

        const sections = document.querySelectorAll('.builder-section');
        if (sections.length === 0) return;

        // Find the section most in view
        let mostVisibleSection = null;
        let maxVisibility = 0;

        sections.forEach(section => {
            const rect = section.getBoundingClientRect();
            const canvasRect = canvas.getBoundingClientRect();
            
            // Calculate how much of the section is visible
            const visibleTop = Math.max(rect.top, canvasRect.top);
            const visibleBottom = Math.min(rect.bottom, canvasRect.bottom);
            const visibleHeight = Math.max(0, visibleBottom - visibleTop);
            
            if (visibleHeight > maxVisibility) {
                maxVisibility = visibleHeight;
                mostVisibleSection = section;
            }
        });

        if (mostVisibleSection) {
            this.setActiveIndicator(mostVisibleSection.dataset.sectionId);
        }
    },

    /**
     * Setup intersection observer for section visibility
     */
    setupSectionObserver: function() {
        const canvas = document.getElementById('designCanvas');
        if (!canvas) return;

        // Debounce scroll handler
        let scrollTimeout;
        canvas.addEventListener('scroll', () => {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => this.updateActiveIndicator(), 100);
        });
    },

    // ========================================================================
    // ZOOM CONTROLS
    // ========================================================================

    /**
     * Zoom in the canvas
     */
    zoomIn: function() {
        const currentIndex = this.zoomLevels.indexOf(this.zoomLevel);
        if (currentIndex < this.zoomLevels.length - 1) {
            this.zoomLevel = this.zoomLevels[currentIndex + 1];
            this.applyZoom();
        }
    },

    /**
     * Zoom out the canvas
     */
    zoomOut: function() {
        const currentIndex = this.zoomLevels.indexOf(this.zoomLevel);
        if (currentIndex > 0) {
            this.zoomLevel = this.zoomLevels[currentIndex - 1];
            this.applyZoom();
        }
    },

    /**
     * Reset zoom to 100%
     */
    zoomReset: function() {
        this.zoomLevel = 100;
        this.applyZoom();
    },

    /**
     * Apply current zoom level to canvas
     */
    applyZoom: function() {
        const canvas = document.getElementById('designCanvas');
        if (!canvas) return;

        // Apply zoom via CSS transform
        const wrapper = canvas.querySelector('.canvas-wrapper');
        if (wrapper) {
            wrapper.style.transform = `scale(${this.zoomLevel / 100})`;
            wrapper.style.transformOrigin = 'top center';
        }

        this.updateZoomDisplay();
        console.log(`🔍 Zoom: ${this.zoomLevel}%`);
    },

    /**
     * Update zoom level display
     */
    updateZoomDisplay: function() {
        const display = document.getElementById('zoomLevelDisplay');
        if (display) {
            display.textContent = `${this.zoomLevel}%`;
        }
    },

    // ========================================================================
    // SECTION COLLAPSE/EXPAND
    // ========================================================================

    /**
     * Collapse all sections
     */
    collapseAllSections: function() {
        const sections = document.querySelectorAll('.builder-section');
        sections.forEach(section => {
            this.collapseSection(section);
        });
        console.log('📁 All sections collapsed');
    },

    /**
     * Expand all sections
     */
    expandAllSections: function() {
        const sections = document.querySelectorAll('.builder-section');
        sections.forEach(section => {
            this.expandSection(section);
        });
        console.log('📂 All sections expanded');
    },

    /**
     * Collapse a single section
     * @param {HTMLElement} section - The section element
     */
    collapseSection: function(section) {
        section.classList.add('collapsed');
        
        // Find and update collapse icon
        const icon = section.querySelector('.section-collapse-icon');
        if (icon) {
            icon.classList.remove('ri-arrow-down-s-line');
            icon.classList.add('ri-arrow-right-s-line');
        }

        // Hide fields container
        const fieldsContainer = section.querySelector('.fields-container, .section-content, .card-body');
        if (fieldsContainer) {
            fieldsContainer.style.display = 'none';
        }
    },

    /**
     * Expand a single section
     * @param {HTMLElement} section - The section element
     */
    expandSection: function(section) {
        section.classList.remove('collapsed');
        
        // Find and update collapse icon
        const icon = section.querySelector('.section-collapse-icon');
        if (icon) {
            icon.classList.remove('ri-arrow-right-s-line');
            icon.classList.add('ri-arrow-down-s-line');
        }

        // Show fields container
        const fieldsContainer = section.querySelector('.fields-container, .section-content, .card-body');
        if (fieldsContainer) {
            fieldsContainer.style.display = '';
        }
    },

    /**
     * Toggle section collapse state
     * @param {string} sectionId - The section ID
     */
    toggleSection: function(sectionId) {
        const section = document.querySelector(`.builder-section[data-section-id="${sectionId}"]`);
        if (!section) return;

        if (section.classList.contains('collapsed')) {
            this.expandSection(section);
        } else {
            this.collapseSection(section);
        }
    },

    // ========================================================================
    // QUICK ACTIONS
    // ========================================================================

    /**
     * Add a new section (triggers existing modal)
     */
    addSection: function() {
        // Use existing section modal if available
        if (typeof FormBuilderSections !== 'undefined' && FormBuilderSections.showAddSectionModal) {
            FormBuilderSections.showAddSectionModal();
        } else {
            // Fallback: click the add section button if it exists
            const addBtn = document.querySelector('[onclick*="showAddSectionModal"]');
            if (addBtn) {
                addBtn.click();
            } else {
                console.warn('Add section functionality not available');
            }
        }
    },

    /**
     * Preview the form
     */
    previewForm: function() {
        // Check if preview tab exists
        const previewTab = document.querySelector('a[href="#builder-preview"]');
        if (previewTab) {
            previewTab.click();
            return;
        }

        // Alternative: Open preview modal
        this.showPreviewModal();
    },

    /**
     * Show preview modal
     */
    showPreviewModal: function() {
        // Create preview overlay if it doesn't exist
        let overlay = document.getElementById('previewOverlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.id = 'previewOverlay';
            overlay.className = 'preview-overlay';
            overlay.innerHTML = `
                <div class="preview-container">
                    <div class="preview-header">
                        <h5><i class="ri-eye-line me-2"></i>Form Preview</h5>
                        <button type="button" class="preview-close" onclick="FormBuilderToolbar.closePreview()">
                            <i class="ri-close-line"></i>
                        </button>
                    </div>
                    <div class="preview-body" id="previewBody">
                        <!-- Preview content will be loaded here -->
                    </div>
                </div>
            `;
            document.body.appendChild(overlay);

            // Close on overlay click
            overlay.addEventListener('click', (e) => {
                if (e.target === overlay) {
                    this.closePreview();
                }
            });
        }

        // Load preview content
        this.loadPreviewContent();

        // Show overlay
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    },

    /**
     * Load preview content
     */
    loadPreviewContent: function() {
        const previewBody = document.getElementById('previewBody');
        if (!previewBody) return;

        // Clone the canvas content for preview
        const canvas = document.querySelector('.canvas-wrapper');
        if (canvas) {
            const clone = canvas.cloneNode(true);
            
            // Remove builder-specific elements
            clone.querySelectorAll('.field-actions, .section-actions, .drag-handle').forEach(el => el.remove());
            clone.querySelectorAll('.builder-section').forEach(el => {
                el.classList.remove('selected', 'hover');
                el.style.cursor = 'default';
            });
            clone.querySelectorAll('.builder-field').forEach(el => {
                el.classList.remove('selected', 'hover');
                el.style.cursor = 'default';
            });

            previewBody.innerHTML = '';
            previewBody.appendChild(clone);
        } else {
            previewBody.innerHTML = '<p class="text-muted text-center py-4">No content to preview</p>';
        }
    },

    /**
     * Close preview modal
     */
    closePreview: function() {
        const overlay = document.getElementById('previewOverlay');
        if (overlay) {
            overlay.classList.remove('active');
            document.body.style.overflow = '';
        }
    },

    // ========================================================================
    // UNDO/REDO (Placeholder)
    // ========================================================================

    // Action history for undo/redo
    actionHistory: [],
    historyIndex: -1,

    /**
     * Undo last action
     */
    undo: function() {
        // TODO: Implement undo functionality
        console.log('⏪ Undo - Not yet implemented');
        this.showToast('Undo functionality coming soon', 'info');
    },

    /**
     * Redo last undone action
     */
    redo: function() {
        // TODO: Implement redo functionality
        console.log('⏩ Redo - Not yet implemented');
        this.showToast('Redo functionality coming soon', 'info');
    },

    // ========================================================================
    // KEYBOARD SHORTCUTS
    // ========================================================================

    /**
     * Handle keyboard shortcuts
     * @param {KeyboardEvent} e - Keyboard event
     */
    handleKeyboardShortcuts: function(e) {
        // Don't trigger if typing in an input
        if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.isContentEditable) {
            return;
        }

        // Ctrl/Cmd + key combinations
        if (e.ctrlKey || e.metaKey) {
            switch (e.key) {
                case '+':
                case '=':
                    e.preventDefault();
                    this.zoomIn();
                    break;
                case '-':
                    e.preventDefault();
                    this.zoomOut();
                    break;
                case '0':
                    e.preventDefault();
                    this.zoomReset();
                    break;
                case 'z':
                    if (!e.shiftKey) {
                        e.preventDefault();
                        this.undo();
                    }
                    break;
                case 'y':
                    e.preventDefault();
                    this.redo();
                    break;
                case 'Z':
                    if (e.shiftKey) {
                        e.preventDefault();
                        this.redo();
                    }
                    break;
            }
        }

        // Alt + key combinations
        if (e.altKey) {
            switch (e.key) {
                case 'c':
                    e.preventDefault();
                    this.collapseAllSections();
                    break;
                case 'e':
                    e.preventDefault();
                    this.expandAllSections();
                    break;
                case 'n':
                    e.preventDefault();
                    this.addSection();
                    break;
                case 'p':
                    e.preventDefault();
                    this.previewForm();
                    break;
            }
        }

        // Escape to close preview
        if (e.key === 'Escape') {
            this.closePreview();
        }
    },

    // ========================================================================
    // UTILITIES
    // ========================================================================

    /**
     * Show a toast notification
     * @param {string} message - Message to display
     * @param {string} type - Toast type (success, error, info, warning)
     */
    showToast: function(message, type = 'info') {
        // Use existing toast system if available
        if (typeof Toastify !== 'undefined') {
            Toastify({
                text: message,
                duration: 3000,
                gravity: 'top',
                position: 'right',
                className: `bg-${type}`,
            }).showToast();
        } else {
            console.log(`[${type.toUpperCase()}] ${message}`);
        }
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Small delay to ensure other components are loaded
    setTimeout(() => {
        FormBuilderToolbar.init();
    }, 100);
});
