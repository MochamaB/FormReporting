/**
 * Tab Persistence Utility
 * 
 * Automatically saves and restores active tabs across page reloads and filter changes
 * Uses localStorage + URL parameters for maximum reliability
 * 
 * Usage: No configuration needed - works automatically with Bootstrap tabs
 * 
 * Features:
 * - Saves tab selection to localStorage
 * - Updates URL parameters for bookmarking
 * - Handles multiple tab groups on same page
 * - Works with filter changes and page reloads
 * - Automatic initialization
 */

window.TabPersistence = {
    // Configuration
    config: {
        storageKey: 'activeTabs',
        urlParam: 'tab',
        autoInitialize: true
    },

    /**
     * Initialize all tab groups on the page
     */
    initialize: function() {
        const self = this;
        
        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function() {
                self.initTabGroups();
            });
        } else {
            self.initTabGroups();
        }
    },

    /**
     * Find and initialize all tab groups
     */
    initTabGroups: function() {
        const self = this;
        
        // Find all tab navigation elements
        const tabGroups = document.querySelectorAll('[data-tab-group]');
        
        tabGroups.forEach(function(tabGroup) {
            const groupId = tabGroup.getAttribute('data-tab-group');
            self.initTabGroup(groupId, tabGroup);
        });

        // Also handle tab groups without data-tab-group (auto-detect)
        const autoTabGroups = document.querySelectorAll('.nav-tabs');
        autoTabGroups.forEach(function(tabGroup, index) {
            if (!tabGroup.hasAttribute('data-tab-group')) {
                const groupId = 'tab-group-' + index;
                tabGroup.setAttribute('data-tab-group', groupId);
                self.initTabGroup(groupId, tabGroup);
            }
        });
    },

    /**
     * Initialize a specific tab group
     */
    initTabGroup: function(groupId, tabGroup) {
        const self = this;
        
        // Find all tab links in this group
        const tabLinks = tabGroup.querySelectorAll('[data-bs-toggle="tab"]');
        
        if (tabLinks.length === 0) return;

        // Restore active tab
        const activeTabId = self.getActiveTab(groupId);
        if (activeTabId) {
            self.showTab(groupId, activeTabId, false);
        }

        // Add click handlers to save tab selection
        tabLinks.forEach(function(tabLink) {
            tabLink.addEventListener('shown.bs.tab', function(e) {
                const targetTab = e.target.getAttribute('href');
                const tabId = targetTab.replace('#', '');
                self.saveTab(groupId, tabId);
            });
        });
    },

    /**
     * Get the active tab for a group (URL > localStorage > default)
     */
    getActiveTab: function(groupId) {
        // First check URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        const urlTab = urlParams.get(this.config.urlParam);
        
        // If URL has tab parameter and this is the main tab group, use it
        if (urlTab && groupId === 'tab-group-0') {
            return urlTab;
        }

        // Then check localStorage
        const savedTabs = this.getSavedTabs();
        return savedTabs[groupId] || null;
    },

    /**
     * Save active tab to localStorage and update URL
     */
    saveTab: function(groupId, tabId) {
        const self = this;
        
        // Save to localStorage
        const savedTabs = this.getSavedTabs();
        savedTabs[groupId] = tabId;
        localStorage.setItem(this.config.storageKey, JSON.stringify(savedTabs));

        // Update URL if this is the main tab group
        if (groupId === 'tab-group-0') {
            self.updateUrl(tabId);
        }
    },

    /**
     * Show a specific tab
     */
    showTab: function(groupId, tabId, save = true) {
        const tabGroup = document.querySelector(`[data-tab-group="${groupId}"]`);
        if (!tabGroup) return;

        // Find the tab link and trigger click
        const tabLink = tabGroup.querySelector(`[href="#${tabId}"]`);
        if (tabLink) {
            const tab = new bootstrap.Tab(tabLink);
            tab.show();
            
            if (save) {
                this.saveTab(groupId, tabId);
            }
        }
    },

    /**
     * Update URL without page reload
     */
    updateUrl: function(tabId) {
        const url = new URL(window.location);
        
        if (tabId) {
            url.searchParams.set(this.config.urlParam, tabId);
        } else {
            url.searchParams.delete(this.config.urlParam);
        }

        // Update URL without triggering page reload
        window.history.replaceState({}, '', url);
    },

    /**
     * Get saved tabs from localStorage
     */
    getSavedTabs: function() {
        try {
            const saved = localStorage.getItem(this.config.storageKey);
            return saved ? JSON.parse(saved) : {};
        } catch (e) {
            return {};
        }
    },

    /**
     * Clear all saved tabs
     */
    clearSavedTabs: function() {
        localStorage.removeItem(this.config.storageKey);
    },

    /**
     * Get current active tab for a group
     */
    getCurrentTab: function(groupId) {
        const tabGroup = document.querySelector(`[data-tab-group="${groupId}"]`);
        if (!tabGroup) return null;

        const activeTab = tabGroup.querySelector('.tab-pane.active');
        return activeTab ? activeTab.id : null;
    },

    /**
     * Manually trigger tab change (useful for programmatic navigation)
     */
    switchToTab: function(groupId, tabId) {
        this.showTab(groupId, tabId, true);
    }
};

// Auto-initialize if enabled
if (window.TabPersistence.config.autoInitialize) {
    window.TabPersistence.initialize();
}
