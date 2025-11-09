-- ============================================================================
-- Migration: Fix MenuItems Controller and Action columns
-- Description: Move Action values to Controller and set Action to 'Index'
-- ============================================================================

-- Step 1: Backup current data (view before changes)
SELECT
    MenuItemId,
    MenuTitle,
    Controller AS OldController,
    Action AS OldAction,
    Action AS NewController,  -- What will become the new Controller
    'Index' AS NewAction      -- What will become the new Action
FROM MenuItems
ORDER BY ModuleId, DisplayOrder;

-- Step 2: Update MenuItems - Move Action to Controller, set Action to 'Index'
UPDATE MenuItems
SET
    Controller = Action,      -- Move current Action value to Controller
    Action = 'Index'          -- Set Action to Index
WHERE Action IS NOT NULL;

-- Step 3: Verify the changes
SELECT
    mi.MenuItemId,
    m.ModuleName,
    mi.MenuTitle,
    mi.Controller AS NewController,
    mi.Action AS NewAction,
    mi.DisplayOrder
FROM MenuItems mi
INNER JOIN Modules m ON mi.ModuleId = m.ModuleId
ORDER BY m.DisplayOrder, mi.DisplayOrder;

-- ============================================================================
-- Rollback Script (if needed)
-- ============================================================================
/*
-- This requires you to have saved the backup data from Step 1
-- Manually restore old values if needed
*/

-- ============================================================================
-- Optional: Singularize Controller Names
-- ============================================================================
-- Uncomment and modify if you want to change plural names to singular
-- (e.g., "Regions" -> "Region", "Factories" -> "Factory")

/*
UPDATE MenuItems SET Controller = 'Region' WHERE Controller = 'Regions';
UPDATE MenuItems SET Controller = 'Factory' WHERE Controller = 'Factories';
UPDATE MenuItems SET Controller = 'Subsidiary' WHERE Controller = 'Subsidiaries';
UPDATE MenuItems SET Controller = 'Tenant' WHERE Controller = 'Tenants';
UPDATE MenuItems SET Controller = 'Department' WHERE Controller = 'Departments';
UPDATE MenuItems SET Controller = 'Group' WHERE Controller = 'Groups';
UPDATE MenuItems SET Controller = 'User' WHERE Controller = 'Users';
UPDATE MenuItems SET Controller = 'Role' WHERE Controller = 'Roles';
UPDATE MenuItems SET Controller = 'Permission' WHERE Controller = 'Permissions';
UPDATE MenuItems SET Controller = 'Budget' WHERE Controller = 'Budgets';
UPDATE MenuItems SET Controller = 'Expenditure' WHERE Controller = 'Expenditures';
UPDATE MenuItems SET Controller = 'CostCenter' WHERE Controller = 'CostCenters';
UPDATE MenuItems SET Controller = 'Vendor' WHERE Controller = 'Vendors';
UPDATE MenuItems SET Controller = 'Category' WHERE Controller = 'Categories';
UPDATE MenuItems SET Controller = 'Assignment' WHERE Controller = 'Assignments';
UPDATE MenuItems SET Controller = 'Installation' WHERE Controller = 'Installations';
UPDATE MenuItems SET Controller = 'Allocation' WHERE Controller = 'Allocations';
UPDATE MenuItems SET Controller = 'Preference' WHERE Controller = 'Preferences';
UPDATE MenuItems SET Controller = 'Alert' WHERE Controller = 'Alerts';
UPDATE MenuItems SET Controller = 'File' WHERE Controller = 'Files';
UPDATE MenuItems SET Controller = 'Folder' WHERE Controller = 'Folders';
UPDATE MenuItems SET Controller = 'License' WHERE Controller = 'Licenses';
UPDATE MenuItems SET Controller = 'Version' WHERE Controller = 'Versions';
UPDATE MenuItems SET Controller = 'Widget' WHERE Controller = 'Widgets';
UPDATE MenuItems SET Controller = 'Target' WHERE Controller = 'Targets';
UPDATE MenuItems SET Controller = 'Value' WHERE Controller = 'Values';
UPDATE MenuItems SET Controller = 'Definition' WHERE Controller = 'Definitions';
UPDATE MenuItems SET Controller = 'Template' WHERE Controller = 'Templates';
UPDATE MenuItems SET Controller = 'Ticket' WHERE Controller = 'All' WHERE MenuTitle = 'All Tickets';
UPDATE MenuItems SET Controller = 'Report' WHERE Controller = 'All' WHERE MenuTitle = 'All Reports';
*/
