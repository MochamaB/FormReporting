/**
 * Menu Management - Drag & Drop and Tree Interaction
 * Uses SortableJS for drag-and-drop reordering
 */

(function () {
    'use strict';

    // Store original form data for change tracking
    let originalFormData = {};
    let allModules = [];
    let allMenuItems = [];

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function () {
        console.log('🚀 Menu Management initialized');
        
        initializeSortable();
        initializeEventHandlers();
        loadModulesAndMenuItems();
        testAPIConnection();
    });

    /**
     * Initialize SortableJS on all nested lists
     */
    function initializeSortable() {
        console.log('📋 Initializing SortableJS...');

        // Get all sortable containers
        const sortableContainers = document.querySelectorAll('.nested-sortable');
        
        if (sortableContainers.length === 0) {
            console.warn('⚠️ No sortable containers found');
            return;
        }

        sortableContainers.forEach(function (container) {
            new Sortable(container, {
                group: 'nested-menu',
                animation: 150,
                fallbackOnBody: true,
                swapThreshold: 0.65,
                handle: '.menu-item, .menu-item-module', // Can drag by the whole item
                ghostClass: 'sortable-ghost',
                chosenClass: 'sortable-chosen',
                dragClass: 'sortable-drag',
                
                // Filter out section headers from being draggable
                filter: '.nested-section',
                
                // On drag start
                onStart: function (evt) {
                    console.log('🎯 Drag started:', evt.item.dataset.id);
                    evt.item.classList.add('dragging');
                },
                
                // On drag end
                onEnd: function (evt) {
                    console.log('✅ Drag ended:', evt.item.dataset.id);
                    evt.item.classList.remove('dragging');
                    
                    // Get the new order
                    const itemId = evt.item.dataset.id;
                    const oldIndex = evt.oldIndex;
                    const newIndex = evt.newIndex;
                    
                    console.log(`Item ${itemId} moved from position ${oldIndex} to ${newIndex}`);
                    
                    // Save the new order
                    saveMenuOrder();
                },
                
                // On item added to new list
                onAdd: function (evt) {
                    console.log('➕ Item added to new list:', evt.item.dataset.id);
                },
                
                // On item moved within same list
                onUpdate: function (evt) {
                    console.log('🔄 Item reordered:', evt.item.dataset.id);
                },
                
                // On item removed from list
                onRemove: function (evt) {
                    console.log('➖ Item removed from list:', evt.item.dataset.id);
                }
            });
        });

        console.log(`✅ Initialized SortableJS on ${sortableContainers.length} containers`);
    }

    /**
     * Save the new menu order after drag & drop
     */
    function saveMenuOrder() {
        console.log('💾 Saving menu order...');

        // Collect all menu items with their new positions
        const items = [];
        let displayOrder = 1;

        // Get all menu items in the tree
        document.querySelectorAll('.menu-item').forEach(function (item) {
            const itemData = {
                menuItemId: parseInt(item.dataset.id),
                newDisplayOrder: displayOrder++,
                newParentMenuItemId: getParentMenuItemId(item),
                newModuleId: parseInt(item.dataset.moduleId) || null
            };
            
            items.push(itemData);
        });

        console.log('📦 Items to save:', items);

        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        console.log('🔐 Anti-forgery token:', token ? 'Found' : 'Not found');

        // Send to server
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['RequestVerificationToken'] = token;
        }

        fetch('/Administration/MenuManagement/ReorderItems', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify({ items: items })
        })
        .then(response => {
            console.log('📡 Response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('📦 Response data:', data);
            if (data.success) {
                console.log('✅ Menu order saved successfully');
                showInfoModal('Success', 'Menu order updated successfully', 'success');
                
                // Reload the page to refresh the tree
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            } else {
                console.error('❌ Failed to save menu order:', data.message);
                showInfoModal('Error', 'Failed to save menu order: ' + data.message, 'error');
            }
        })
        .catch(error => {
            console.error('❌ Error saving menu order:', error);
            showInfoModal('Error', 'Error saving menu order: ' + error.message, 'error');
        });
    }

    /**
     * Get the parent menu item ID for a given item
     */
    function getParentMenuItemId(itemElement) {
        // Find the closest parent menu item
        const parentList = itemElement.closest('.nested-list');
        if (!parentList) return null;

        const parentItem = parentList.closest('.menu-item');
        if (!parentItem) return null;

        return parseInt(parentItem.dataset.id) || null;
    }

    /**
     * Initialize event handlers for buttons and interactions
     */
    function initializeEventHandlers() {
        console.log('🎮 Initializing event handlers...');

        // Expand All button
        const btnExpandAll = document.getElementById('btnExpandAll');
        if (btnExpandAll) {
            btnExpandAll.addEventListener('click', expandAllItems);
        }

        // Collapse All button
        const btnCollapseAll = document.getElementById('btnCollapseAll');
        if (btnCollapseAll) {
            btnCollapseAll.addEventListener('click', collapseAllItems);
        }

        // Add New Item button
        const btnAddNewItem = document.getElementById('btnAddNewItem');
        if (btnAddNewItem) {
            btnAddNewItem.addEventListener('click', showNewItemForm);
        }

        // Save Properties button
        const btnSaveProperties = document.getElementById('btnSaveProperties');
        if (btnSaveProperties) {
            btnSaveProperties.addEventListener('click', saveMenuItemProperties);
        }

        // Cancel Edit button
        const btnCancelEdit = document.getElementById('btnCancelEdit');
        if (btnCancelEdit) {
            btnCancelEdit.addEventListener('click', function () {
                console.log('❌ Cancel edit clicked');
                
                // Hide properties form, show no selection message
                document.getElementById('propertiesForm').style.display = 'none';
                document.getElementById('noSelectionMessage').style.display = 'block';
                
                // Remove selected class from all items
                document.querySelectorAll('.menu-item').forEach(item => {
                    item.classList.remove('selected');
                });
                
                showToast('Edit cancelled', 'info');
            });
        }
    }

    /**
     * Show form for creating new menu item
     */
    function showNewItemForm() {
        console.log('➕ Creating new menu item...');

        // Hide no selection message, show properties form
        document.getElementById('noSelectionMessage').style.display = 'none';
        document.getElementById('propertiesForm').style.display = 'block';
        document.getElementById('propertiesLoading').style.display = 'none';
        document.getElementById('propertiesContent').style.display = 'block';

        // Remove selected class from all items
        document.querySelectorAll('.menu-item').forEach(item => {
            item.classList.remove('selected');
        });

        // Set MenuItemId to 0 (indicates new item)
        document.getElementById('inputMenuItemId').value = '0';

        // Update header
        document.getElementById('propMenuTitle').textContent = 'New Menu Item';
        document.getElementById('propMenuCode').textContent = 'NEW_ITEM';

        // Clear all form fields
        document.getElementById('inputMenuTitle').value = '';
        document.getElementById('inputMenuCode').value = '';
        document.getElementById('selectModuleId').value = '';
        document.getElementById('inputSectionName').value = '';
        document.getElementById('selectParentMenuItemId').value = '';
        document.getElementById('inputLevel').value = '1';
        document.getElementById('inputIcon').value = '';
        document.getElementById('inputDisplayOrder').value = '1';
        document.getElementById('checkIsActive').checked = true;
        document.getElementById('checkIsVisible').checked = true;
        document.getElementById('checkRequiresAuth').checked = true;
        document.getElementById('inputController').value = '';
        document.getElementById('inputAction').value = '';
        document.getElementById('inputFullPath').value = 'N/A';
        document.getElementById('inputCreatedDate').value = 'Not yet created';

        // Reset icon preview
        document.getElementById('iconPreview').className = 'ri-menu-line';

        // Populate parent dropdown (no current item to exclude)
        populateParentDropdown(0);

        // Clear original form data
        originalFormData = {};

        // Remove validation errors
        document.querySelectorAll('.is-invalid').forEach(el => {
            el.classList.remove('is-invalid');
        });

        // Add change listeners
        addFormChangeListeners();

        console.log('✅ New item form ready');
        showToast('Fill in the details for the new menu item', 'info');
    }

    /**
     * Load menu item properties when clicked
     */
    window.loadMenuItemProperties = function (menuItemId) {
        console.log('📄 Loading properties for menu item:', menuItemId);

        // Remove selected class from all items
        document.querySelectorAll('.menu-item').forEach(item => {
            item.classList.remove('selected');
        });

        // Add selected class to clicked item
        const clickedItem = document.querySelector(`[data-id="${menuItemId}"]`);
        if (clickedItem) {
            clickedItem.classList.add('selected');
        }

        // Show loading state
        const noSelectionMessage = document.getElementById('noSelectionMessage');
        const propertiesForm = document.getElementById('propertiesForm');
        
        if (noSelectionMessage) noSelectionMessage.style.display = 'none';
        if (propertiesForm) propertiesForm.style.display = 'block';

        // Fetch item data via API
        fetch(`/Administration/MenuManagement/GetMenuItem/${menuItemId}`)
            .then(response => response.json())
            .then(data => {
                console.log('📦 Menu item data:', data);
                if (data.success) {
                    console.log('✅ Successfully loaded menu item properties');
                    // Properties form will be populated in Step 7
                    populatePropertiesForm(data.data);
                } else {
                    console.error('❌ Failed to load menu item:', data.message);
                    showToast('Failed to load menu item: ' + data.message, 'error');
                }
            })
            .catch(error => {
                console.error('❌ Error loading menu item:', error);
                showToast('Error loading menu item', 'error');
            });
    };

    /**
     * Populate properties form with menu item data
     */
    function populatePropertiesForm(menuItem) {
        console.log('📝 Populating properties form:', menuItem);

        // Hide loading, show content
        document.getElementById('propertiesLoading').style.display = 'none';
        document.getElementById('propertiesContent').style.display = 'block';

        // Store menu item ID
        document.getElementById('inputMenuItemId').value = menuItem.menuItemId;

        // Header
        document.getElementById('propMenuTitle').textContent = menuItem.menuTitle || 'N/A';
        document.getElementById('propMenuCode').textContent = menuItem.menuCode || 'N/A';

        // Basic Info Tab
        document.getElementById('inputMenuTitle').value = menuItem.menuTitle || '';
        document.getElementById('inputMenuCode').value = menuItem.menuCode || '';
        document.getElementById('selectModuleId').value = menuItem.moduleId || '';
        document.getElementById('inputSectionName').value = menuItem.sectionName || 'N/A';
        document.getElementById('inputLevel').value = menuItem.level || 1;

        // Populate parent dropdown and set value
        populateParentDropdown(menuItem.menuItemId);
        document.getElementById('selectParentMenuItemId').value = menuItem.parentMenuItemId || '';

        // Display Tab
        document.getElementById('inputIcon').value = menuItem.icon || '';
        document.getElementById('inputDisplayOrder').value = menuItem.displayOrder || 0;
        document.getElementById('checkIsActive').checked = menuItem.isActive || false;
        document.getElementById('checkIsVisible').checked = menuItem.isVisible || false;
        document.getElementById('checkRequiresAuth').checked = menuItem.requiresAuth || false;

        // Update icon preview
        updateIconPreview();

        // Navigation Tab
        document.getElementById('inputController').value = menuItem.controller || '';
        document.getElementById('inputAction').value = menuItem.action || '';
        
        // Update full path
        updateFullPath();

        // Format created date
        let createdDate = 'N/A';
        if (menuItem.createdDate) {
            const date = new Date(menuItem.createdDate);
            createdDate = date.toLocaleString();
        }
        document.getElementById('inputCreatedDate').value = createdDate;

        // Store original data for change tracking
        originalFormData = {
            menuItemId: menuItem.menuItemId,
            menuTitle: menuItem.menuTitle,
            menuCode: menuItem.menuCode,
            moduleId: menuItem.moduleId,
            parentMenuItemId: menuItem.parentMenuItemId,
            level: menuItem.level,
            icon: menuItem.icon,
            displayOrder: menuItem.displayOrder,
            isActive: menuItem.isActive,
            isVisible: menuItem.isVisible,
            requiresAuth: menuItem.requiresAuth,
            controller: menuItem.controller,
            action: menuItem.action
        };

        // Add change listeners
        addFormChangeListeners();

        console.log('✅ Properties form populated successfully');
    }

    /**
     * Update icon preview when icon input changes
     */
    function updateIconPreview() {
        const iconInput = document.getElementById('inputIcon');
        const iconPreview = document.getElementById('iconPreview');
        
        if (iconInput && iconPreview) {
            const iconClass = iconInput.value.trim();
            if (iconClass) {
                iconPreview.className = iconClass;
            } else {
                iconPreview.className = 'ri-menu-line';
            }
        }
    }

    /**
     * Update full path when controller or action changes
     */
    function updateFullPath() {
        const controller = document.getElementById('inputController').value.trim();
        const action = document.getElementById('inputAction').value.trim();
        const fullPathInput = document.getElementById('inputFullPath');
        
        if (fullPathInput) {
            if (controller && action) {
                fullPathInput.value = `/${controller}/${action}`;
            } else {
                fullPathInput.value = 'N/A';
            }
        }
    }

    /**
     * Add change listeners to form fields
     */
    function addFormChangeListeners() {
        // Icon input - update preview
        const iconInput = document.getElementById('inputIcon');
        if (iconInput) {
            iconInput.addEventListener('input', updateIconPreview);
        }

        // Controller/Action - update full path
        const controllerInput = document.getElementById('inputController');
        const actionInput = document.getElementById('inputAction');
        if (controllerInput) {
            controllerInput.addEventListener('input', updateFullPath);
        }
        if (actionInput) {
            actionInput.addEventListener('input', updateFullPath);
        }

        // Icon picker button
        const btnIconPicker = document.getElementById('btnIconPicker');
        if (btnIconPicker) {
            btnIconPicker.addEventListener('click', function() {
                showToast('Icon picker will be implemented in Step 11', 'info');
            });
        }
    }

    /**
     * Save menu item properties
     */
    function saveMenuItemProperties() {
        console.log('💾 Saving menu item properties...');

        // Validate form
        if (!validateForm()) {
            console.error('❌ Form validation failed');
            return;
        }

        // Collect form data
        const formData = {
            menuItemId: parseInt(document.getElementById('inputMenuItemId').value),
            menuTitle: document.getElementById('inputMenuTitle').value.trim(),
            menuCode: document.getElementById('inputMenuCode').value.trim(),
            moduleId: parseInt(document.getElementById('selectModuleId').value) || null,
            parentMenuItemId: parseInt(document.getElementById('selectParentMenuItemId').value) || null,
            level: parseInt(document.getElementById('inputLevel').value),
            icon: document.getElementById('inputIcon').value.trim() || null,
            displayOrder: parseInt(document.getElementById('inputDisplayOrder').value),
            isActive: document.getElementById('checkIsActive').checked,
            isVisible: document.getElementById('checkIsVisible').checked,
            requiresAuth: document.getElementById('checkRequiresAuth').checked,
            controller: document.getElementById('inputController').value.trim() || null,
            action: document.getElementById('inputAction').value.trim() || null
        };

        console.log('📦 Form data to save:', formData);

        // Check if there are changes
        if (!hasFormChanges(formData)) {
            console.log('ℹ️ No changes detected');
            showToast('No changes to save', 'info');
            return;
        }

        // Show loading state
        const btnSave = document.getElementById('btnSaveProperties');
        const originalText = btnSave.innerHTML;
        btnSave.disabled = true;
        btnSave.innerHTML = '<i class="ri-loader-4-line"></i> Saving...';

        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        // Send to server
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['RequestVerificationToken'] = token;
        }

        fetch('/Administration/MenuManagement/SaveMenuItem', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(formData)
        })
        .then(response => {
            console.log('📡 Response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('📦 Response data:', data);
            
            // Restore button state
            btnSave.disabled = false;
            btnSave.innerHTML = originalText;

            if (data.success) {
                console.log('✅ Menu item saved successfully');
                showToast('Menu item saved successfully', 'success');
                
                // Update original form data
                originalFormData = { ...formData };
                
                // Reload the page to refresh the tree
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            } else {
                console.error('❌ Failed to save menu item:', data.message);
                showToast('Failed to save: ' + data.message, 'error');
                
                // Show validation errors if any
                if (data.errors && data.errors.length > 0) {
                    data.errors.forEach(error => {
                        console.error('  - ' + error);
                    });
                }
            }
        })
        .catch(error => {
            console.error('❌ Error saving menu item:', error);
            
            // Restore button state
            btnSave.disabled = false;
            btnSave.innerHTML = originalText;
            
            showToast('Error saving menu item: ' + error.message, 'error');
        });
    }

    /**
     * Validate form before saving
     */
    function validateForm() {
        let isValid = true;
        const errors = [];

        // Menu Title
        const menuTitle = document.getElementById('inputMenuTitle').value.trim();
        if (!menuTitle) {
            errors.push('Menu Title is required');
            document.getElementById('inputMenuTitle').classList.add('is-invalid');
            isValid = false;
        } else {
            document.getElementById('inputMenuTitle').classList.remove('is-invalid');
        }

        // Menu Code
        const menuCode = document.getElementById('inputMenuCode').value.trim();
        if (!menuCode) {
            errors.push('Menu Code is required');
            document.getElementById('inputMenuCode').classList.add('is-invalid');
            isValid = false;
        } else {
            document.getElementById('inputMenuCode').classList.remove('is-invalid');
        }

        // Module
        const moduleId = document.getElementById('selectModuleId').value;
        if (!moduleId) {
            errors.push('Module is required');
            document.getElementById('selectModuleId').classList.add('is-invalid');
            isValid = false;
        } else {
            document.getElementById('selectModuleId').classList.remove('is-invalid');
        }

        // Level
        const level = parseInt(document.getElementById('inputLevel').value);
        if (!level || level < 1 || level > 4) {
            errors.push('Level must be between 1 and 4');
            document.getElementById('inputLevel').classList.add('is-invalid');
            isValid = false;
        } else {
            document.getElementById('inputLevel').classList.remove('is-invalid');
        }

        if (!isValid) {
            console.error('❌ Validation errors:', errors);
            showToast('Please fix validation errors: ' + errors.join(', '), 'error');
        }

        return isValid;
    }

    /**
     * Check if form has changes
     */
    function hasFormChanges(currentData) {
        if (!originalFormData || !originalFormData.menuItemId) {
            return true; // No original data, assume changes
        }

        // Compare each field
        const fields = [
            'menuTitle', 'menuCode', 'moduleId', 'parentMenuItemId', 
            'level', 'icon', 'displayOrder', 'isActive', 'isVisible', 
            'requiresAuth', 'controller', 'action'
        ];

        for (const field of fields) {
            if (currentData[field] !== originalFormData[field]) {
                console.log(`📝 Field changed: ${field}`, {
                    old: originalFormData[field],
                    new: currentData[field]
                });
                return true;
            }
        }

        return false;
    }

    /**
     * Expand all nested menu items
     */
    function expandAllItems() {
        console.log('📂 Expanding all nested items...');
        
        // Find all nested lists
        const nestedLists = document.querySelectorAll('.nested-list');
        let expandedCount = 0;
        
        nestedLists.forEach(list => {
            if (list.style.display === 'none' || !list.style.display) {
                list.style.display = 'block';
                expandedCount++;
            }
        });
        
        console.log(`✅ Expanded ${expandedCount} nested items`);
        showInfoModal('Success', `Expanded ${expandedCount} nested section(s)`, 'success');
    }

    /**
     * Collapse all nested menu items (keep top level visible)
     */
    function collapseAllItems() {
        console.log('📁 Collapsing all nested items...');
        
        // Hide all nested lists except top level
        const nestedLists = document.querySelectorAll('.nested-2 .nested-list, .nested-3 .nested-list, .nested-4 .nested-list');
        let collapsedCount = 0;
        
        nestedLists.forEach(list => {
            if (list.style.display !== 'none') {
                list.style.display = 'none';
                collapsedCount++;
            }
        });
        
        console.log(`✅ Collapsed ${collapsedCount} nested items`);
        showInfoModal('Success', `Collapsed ${collapsedCount} nested section(s)`, 'success');
    }

    /**
     * Delete menu item
     */
    window.deleteMenuItem = function (menuItemId, menuTitle) {
        console.log('🗑️ Delete menu item:', menuItemId, menuTitle);

        // Show confirmation modal
        showConfirmModal(
            'Delete Menu Item',
            `Are you sure you want to delete "${menuTitle}"?\n\nThis action cannot be undone.`,
            function() {
                // User confirmed - proceed with deletion
                console.log('✅ Delete confirmed');
                performDelete(menuItemId, menuTitle);
            }
        );
    };

    /**
     * Perform the actual deletion
     */
    function performDelete(menuItemId, menuTitle) {

        // Get anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        // Send delete request
        const headers = {
            'Content-Type': 'application/json'
        };
        
        if (token) {
            headers['RequestVerificationToken'] = token;
        }

        fetch(`/Administration/MenuManagement/DeleteMenuItem/${menuItemId}`, {
            method: 'POST',
            headers: headers
        })
        .then(response => {
            console.log('📡 Response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('📦 Response data:', data);

            if (data.success) {
                console.log('✅ Menu item deleted successfully');
                showInfoModal('Success', data.message || 'Menu item deleted successfully', 'success');
                
                // Reload the page to refresh the tree
                setTimeout(() => {
                    window.location.reload();
                }, 1500);
            } else {
                console.error('❌ Failed to delete menu item:', data.message);
                showInfoModal('Error', data.message || 'Failed to delete menu item', 'error');
                
                // Show specific errors if any
                if (data.errors && data.errors.length > 0) {
                    data.errors.forEach(error => {
                        console.error('  - ' + error);
                    });
                }
            }
        })
        .catch(error => {
            console.error('❌ Error deleting menu item:', error);
            showInfoModal('Error', 'Error deleting menu item: ' + error.message, 'error');
        });
    }

    /**
     * Show toast notification
     */
    function showToast(message, type = 'info') {
        // Using browser's built-in notification for now
        // Can be replaced with a proper toast library later
        console.log(`🔔 Toast [${type}]:`, message);
        
        // If you have a toast library in Velzon, use it here
        // For now, just use alert as fallback
        if (type === 'error') {
            console.error('❌', message);
        } else if (type === 'success') {
            console.log('✅', message);
        } else {
            console.info('ℹ️', message);
        }
    }

    /**
     * Show confirmation modal
     */
    function showConfirmModal(title, message, onConfirm) {
        const modal = document.getElementById('confirmModal');
        const modalTitle = document.getElementById('confirmModalLabel');
        const modalMessage = document.getElementById('confirmModalMessage');
        const confirmBtn = document.getElementById('confirmModalAction');
        
        // Set content
        modalTitle.textContent = title;
        modalMessage.textContent = message;
        
        // Remove any existing event listeners by cloning the button
        const newConfirmBtn = confirmBtn.cloneNode(true);
        confirmBtn.parentNode.replaceChild(newConfirmBtn, confirmBtn);
        
        // Add new event listener
        newConfirmBtn.addEventListener('click', function() {
            // Hide modal
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
            
            // Execute callback
            if (typeof onConfirm === 'function') {
                onConfirm();
            }
        });
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    /**
     * Show info modal
     */
    function showInfoModal(title, message, type = 'info') {
        const modal = document.getElementById('infoModal');
        const modalTitle = document.getElementById('infoModalLabel');
        const modalMessage = document.getElementById('infoModalMessage');
        const modalIcon = document.getElementById('infoModalIcon');
        
        // Set content
        modalTitle.textContent = title;
        modalMessage.textContent = message;
        
        // Set icon based on type
        if (type === 'success') {
            modalIcon.className = 'ri-checkbox-circle-line text-success';
        } else if (type === 'error') {
            modalIcon.className = 'ri-error-warning-line text-danger';
        } else if (type === 'warning') {
            modalIcon.className = 'ri-alert-line text-warning';
        } else {
            modalIcon.className = 'ri-information-line text-info';
        }
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    /**
     * Load modules and menu items for dropdowns
     */
    function loadModulesAndMenuItems() {
        console.log('📥 Loading modules and menu items...');

        fetch('/Administration/MenuManagement/GetMenuTree')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Clear existing data
                    allModules = [];
                    allMenuItems = [];
                    
                    data.data.sections.forEach(section => {
                        section.modules.forEach(module => {
                            allModules.push({
                                moduleId: module.moduleId,
                                moduleName: module.moduleName,
                                sectionName: section.sectionName
                            });
                            
                            // Collect all menu items
                            module.menuItems.forEach(item => {
                                collectMenuItems(item);
                            });
                        });
                    });
                    
                    console.log('✅ Loaded', allModules.length, 'modules and', allMenuItems.length, 'menu items');
                    populateModuleDropdown();
                } else {
                    console.error('❌ Failed to load modules:', data.message);
                }
            })
            .catch(error => {
                console.error('❌ Error loading modules:', error);
            });
    }

    /**
     * Recursively collect all menu items
     */
    function collectMenuItems(item) {
        allMenuItems.push({
            menuItemId: item.menuItemId,
            menuTitle: item.menuTitle,
            level: item.level
        });
        
        if (item.children && item.children.length > 0) {
            item.children.forEach(child => collectMenuItems(child));
        }
    }

    /**
     * Populate module dropdown
     */
    function populateModuleDropdown() {
        const select = document.getElementById('selectModuleId');
        if (!select) return;
        
        // Clear existing options except first
        select.innerHTML = '<option value="">Select Module...</option>';
        
        // Add modules grouped by section
        let currentSection = '';
        allModules.forEach(module => {
            if (module.sectionName !== currentSection) {
                if (currentSection !== '') {
                    // Add separator
                    const separator = document.createElement('option');
                    separator.disabled = true;
                    separator.textContent = '──────────';
                    select.appendChild(separator);
                }
                currentSection = module.sectionName;
            }
            
            const option = document.createElement('option');
            option.value = module.moduleId;
            option.textContent = `${module.moduleName} (${module.sectionName})`;
            select.appendChild(option);
        });
        
        console.log('✅ Module dropdown populated');
    }

    /**
     * Populate parent menu item dropdown
     */
    function populateParentDropdown(currentItemId) {
        const select = document.getElementById('selectParentMenuItemId');
        if (!select) return;
        
        // Clear existing options
        select.innerHTML = '<option value="">None (Top Level)</option>';
        
        // Add menu items (excluding current item and its children)
        allMenuItems.forEach(item => {
            if (item.menuItemId !== currentItemId) {
                const option = document.createElement('option');
                option.value = item.menuItemId;
                const indent = '  '.repeat(item.level - 1);
                option.textContent = `${indent}${item.menuTitle}`;
                select.appendChild(option);
            }
        });
        
        console.log('✅ Parent dropdown populated');
    }

    /**
     * Test API connection on page load
     */
    function testAPIConnection() {
        console.log('🔌 Testing API connection...');

        fetch('/Administration/MenuManagement/GetMenuTree')
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    console.log('✅ API connection successful');
                    console.log('📊 Menu tree data:', data.data);
                } else {
                    console.error('❌ API returned error:', data.message);
                }
            })
            .catch(error => {
                console.error('❌ Failed to connect to API:', error);
            });
    }

})();
