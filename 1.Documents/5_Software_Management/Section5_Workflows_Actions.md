# SECTION 5: SOFTWARE MANAGEMENT - Workflows & Actions

**Module:** Software Asset Management & License Tracking
**Tables:** 5 (SoftwareProducts, SoftwareVersions, SoftwareLicenses, TenantSoftwareInstallations, SoftwareInstallationHistory)

---

## 1. SOFTWARE PRODUCTS

### **CRUD Operations:**
- **CREATE** Software Product
- **READ** Product (Single)
- **READ** All Products (List with filters by Category/Vendor)
- **READ** Products by License Model
- **UPDATE** Product Details
- **DELETE** Product (Check for dependencies first)

### **Business Rules:**
- ProductCode must be unique
- ProductCategory must be: 'System', 'Application', 'Utility', 'Security'
- LicenseModel must be: 'PerUser', 'PerDevice', 'Enterprise', 'Subscription', 'OpenSource', 'Concurrent'
- RequiresLicense flag determines if license tracking is mandatory
- IsKTDAProduct flag for internally developed software
- Cannot delete Product if versions, licenses, or installations exist

### **Workflows:**

#### **WF-5.1: Create Software Product**
```
Trigger: Admin registers new software in catalog
Steps:
  1. Navigate to "Software Management → Products → Add New"
  2. Enter product details:
     - ProductCode: WIN_SERVER_2022
     - ProductName: Windows Server 2022
     - Vendor: Microsoft Corporation
     - ProductCategory: [System ▼]

  3. License Configuration:
     - LicenseModel: [PerDevice ▼]
     - [✓] Requires License
     - [ ] Is KTDA Product

  4. Enter Description:
     "Microsoft Windows Server 2022 Standard Edition - Network Operating System"

  5. Validate ProductCode uniqueness
  6. Save Product with IsActive = 1
  7. Show success: "Product created. Add versions now?"
  8. Redirect to "Add Version" screen
```

#### **WF-5.2: Product Categorization**
```
Standard Categories:
  System:
    - Operating Systems (Windows, Linux, macOS)
    - Database Systems (SQL Server, MySQL, PostgreSQL)
    - Virtualization Platforms (VMware, Hyper-V)

  Application:
    - Office Suites (Microsoft 365, LibreOffice)
    - ERP Systems (SAP, Oracle, Custom KTDA System)
    - Development Tools (Visual Studio, Eclipse)
    - Design Software (Adobe Creative Suite, AutoCAD)

  Utility:
    - Backup Solutions (Veeam, Acronis)
    - Compression Tools (WinRAR, 7-Zip)
    - File Transfer (WinSCP, FileZilla)
    - Remote Access (TeamViewer, AnyDesk)

  Security:
    - Antivirus (Kaspersky, Bitdefender, Windows Defender)
    - Firewall Software
    - VPN Clients
    - Encryption Tools
```

#### **WF-5.3: License Model Selection Guide**
```
PerUser:
  Definition: License tied to individual user
  Example: Microsoft 365 E3 (john.doe@ktda.co.ke)
  Tracking: Assign to UserId
  Count: Number of active users with access

PerDevice:
  Definition: License tied to specific machine
  Example: Windows Server 2022 on SERVER-01
  Tracking: Link to MachineName or HardwareItemId
  Count: Number of installations/devices

Enterprise:
  Definition: Site license for unlimited use within organization
  Example: KTDA ERP System (all 80 tenants)
  Tracking: License at organization level
  Count: N/A (unlimited)

Subscription:
  Definition: Time-limited license (monthly/yearly)
  Example: Adobe Creative Cloud (Annual)
  Tracking: ExpiryDate monitoring
  Count: Purchased quantity

OpenSource:
  Definition: Free to use, no license key required
  Example: PostgreSQL, Linux Ubuntu
  Tracking: Version compliance only
  Count: N/A

Concurrent:
  Definition: Floating license pool (e.g., 50 concurrent users)
  Example: AutoCAD (20 concurrent seats)
  Tracking: Active sessions count
  Count: Maximum simultaneous users
```

#### **WF-5.4: Update Product Details**
```
Trigger: Admin needs to modify product information
Steps:
  1. Navigate to "Products → Edit {ProductName}"
  2. Editable fields:
     - ProductName
     - Vendor
     - ProductCategory
     - Description
     - IsActive (for soft delete)

  3. Non-editable fields (data integrity):
     - ProductCode (unique identifier)
     - RequiresLicense (affects license tracking)
     - LicenseModel (affects installations)

  4. If changing critical fields, show warning:
     "X installations exist. Changes may affect license compliance reporting."

  5. Save changes
  6. Log in AuditLogs
  7. Trigger recalculation of affected reports
```

#### **WF-5.5: Delete Product with Dependencies Check**
```
Trigger: Admin attempts to delete product
Steps:
  1. Navigate to "Products → Delete {ProductName}"
  2. System checks dependencies:
     a. SoftwareVersions: 5 versions found
     b. SoftwareLicenses: 3 active licenses
     c. TenantSoftwareInstallations: 25 active installations

  3. Show error:
     "Cannot delete. Product is in use:
      - 5 versions registered
      - 3 active licenses (Total cost: KES 2,500,000)
      - 25 active installations across 12 tenants

     Options:
     [ ] Deactivate instead (sets IsActive = 0)
     [ ] Force delete (removes ALL data - REQUIRES SYSADMIN)"

  4. If user selects "Deactivate":
     - Set IsActive = 0
     - Hide from product catalog
     - Preserve all versions, licenses, installations (audit trail)
     - Product still appears in historical reports

  5. If user selects "Force delete" (SYSADMIN only):
     - Confirm with password
     - Show final warning: "This will DELETE all versions, licenses, and installation records"
     - CASCADE DELETE SoftwareVersions
     - CASCADE DELETE SoftwareLicenses
     - CASCADE DELETE TenantSoftwareInstallations
     - CASCADE DELETE SoftwareInstallationHistory
     - DELETE SoftwareProducts
     - Log in AuditLogs with Reason
```

---

## 2. SOFTWARE VERSIONS

### **CRUD Operations:**
- **CREATE** Software Version
- **READ** Version (Single)
- **READ** Versions for Product (ordered by MajorVersion DESC)
- **READ** Supported Versions Only (IsSupported = 1)
- **READ** Vulnerable Versions (SecurityLevel = 'Vulnerable')
- **UPDATE** Version Status (support/security level)
- **DELETE** Version (if no installations exist)

### **Business Rules:**
- VersionNumber must be unique per Product (compound unique: ProductId + VersionNumber)
- MajorVersion, MinorVersion, PatchVersion for version comparison (e.g., 10.2.5)
- SecurityLevel must be: 'Critical', 'Stable', 'Vulnerable', 'Unsupported'
- Only one version can have IsCurrentVersion = 1 per Product
- EndOfLifeDate triggers version support warnings
- ChecksumSHA256 for file integrity verification

### **Workflows:**

#### **WF-5.6: Register Software Version**
```
Trigger: New software version released
Steps:
  1. Navigate to "Products → {ProductName} → Versions → Add New"
  2. Enter version details:
     - VersionNumber: 2022
     - Or semantic: Major: 10, Minor: 2, Patch: 5 (displays as "10.2.5")

  3. Release information:
     - ReleaseDate: [2024-11-01 📅]
     - EndOfLifeDate: [2029-10-31 📅] (5-year support)

  4. Version status:
     - [✓] Is Current Version (auto-unchecks previous current version)
     - [✓] Is Supported
     - SecurityLevel: [Stable ▼]
     - [ ] Minimum Supported Version (for compliance)

  5. Distribution details (optional):
     - DownloadUrl: https://download.microsoft.com/...
     - FileSize: 5368709120 (5 GB)
     - ChecksumSHA256: a3f2... (for integrity verification)

  6. Release Notes (optional):
     "- New Active Directory features
      - Enhanced security for hybrid cloud
      - Performance improvements for containers"

  7. Validate version uniqueness for product
  8. If IsCurrentVersion = 1:
     - Query existing current version for this product
     - Update old version: SET IsCurrentVersion = 0

  9. Save version
  10. Trigger "Version Available" notification to admins
  11. Generate "Installations Needing Upgrade" report
```

#### **WF-5.7: Mark Version as Vulnerable**
```
Trigger: Security bulletin released about software vulnerability
Example: CVE-2025-12345 affects Windows Server 2019 versions < 17763.5678
Steps:
  1. Navigate to "Versions → Windows Server 2019 → Version 17763.5000"
  2. Current Status: SecurityLevel = 'Stable', IsSupported = 1

  3. Update Security Level:
     SecurityLevel: [Vulnerable ▼]
     Reason: "CVE-2025-12345: Remote Code Execution vulnerability"

  4. Optional: Set End of Life:
     [ ] Force End of Support
     EndOfLifeDate: [2025-12-01 📅]

  5. Save changes

  6. System actions:
     a. Query all installations using this version:
        SELECT ti.InstallationId, t.TenantName, ti.MachineName
        FROM TenantSoftwareInstallations ti
        INNER JOIN Tenants t ON ti.TenantId = t.TenantId
        WHERE ti.VersionId = X AND ti.Status = 'Active'

     b. Auto-update Installation.Status:
        UPDATE TenantSoftwareInstallations
        SET Status = 'NeedsUpgrade'
        WHERE VersionId = X AND Status = 'Active'

     c. Generate high-priority alert for each tenant:
        Subject: "SECURITY ALERT: Vulnerable Software Detected"
        Message: "Windows Server 2019 version 17763.5000 on SERVER-01 has critical vulnerability CVE-2025-12345.
                  Immediate upgrade required."
        Priority: High

     d. Create dashboard widget: "Vulnerable Installations"

     e. Notify:
        - SYSADMIN role
        - HO_ICT_MGR role
        - Tenant ICT contacts

  7. Log in AuditLogs and SystemMetricLogs
```

#### **WF-5.8: Version Comparison Logic**
```
Use Case: Determine if installation is outdated
Logic:
  1. Parse version numbers:
     CurrentVersion: 10.2.5 → Major=10, Minor=2, Patch=5
     InstalledVersion: 10.1.3 → Major=10, Minor=1, Patch=3

  2. Compare:
     IF InstalledMajor < CurrentMajor → "Major Upgrade Needed"
     ELSE IF InstalledMinor < CurrentMinor → "Minor Upgrade Available"
     ELSE IF InstalledPatch < CurrentPatch → "Patch Available"
     ELSE → "Up to Date"

  3. Check End of Life:
     IF InstalledVersion.EndOfLifeDate < TODAY() → "End of Life - Urgent Upgrade"

  4. Check Security:
     IF InstalledVersion.SecurityLevel = 'Vulnerable' → "Security Risk - Urgent Upgrade"
     IF InstalledVersion.SecurityLevel = 'Unsupported' → "Unsupported Version"

  5. Result status:
     - 🟢 Up to Date (current version, security stable)
     - 🟡 Update Available (newer minor/patch version)
     - 🔴 Urgent (EOL or vulnerable)

Example Output:
  Installation: Windows Server 2019 on FACTORY-SERVER-01
  Installed: 17763.5000 (released 2023-05-01)
  Current: 17763.6543 (released 2025-10-01)
  Status: 🔴 Urgent
  Reason: Version marked as Vulnerable (CVE-2025-12345), EOL in 30 days
  Action: Upgrade to version 17763.6543 immediately
```

#### **WF-5.9: End of Life Management**
```
Trigger: Daily Hangfire job at 3 AM
Job: CheckVersionEndOfLife()
Steps:
  1. Query versions approaching EOL:
     SELECT * FROM SoftwareVersions
     WHERE IsSupported = 1
       AND EndOfLifeDate IS NOT NULL
       AND EndOfLifeDate BETWEEN GETDATE() AND DATEADD(DAY, 90, GETDATE())

  2. For each version:
     a. Calculate days until EOL
     b. Query installations using this version
     c. Group by tenant

  3. Generate notifications based on timeline:
     - 90 days before EOL: Info notification to tenant ICT
     - 60 days before EOL: Warning notification + email
     - 30 days before EOL: High priority alert + dashboard banner
     - 7 days before EOL: Critical alert + escalation to SYSADMIN
     - On EOL date:
       * Set IsSupported = 0
       * Update installations: Status = 'EndOfLife'
       * Generate compliance report
       * Escalate to management

  4. Log in SystemMetricLogs:
     JobName: 'CheckVersionEndOfLife'
     Status: 'Success'
     Details: '{"versionsChecked": 45, "alertsGenerated": 12, "installationsAffected": 78}'
```

---

## 3. SOFTWARE LICENSES

### **CRUD Operations:**
- **CREATE** License
- **READ** License (Single)
- **READ** Licenses by Product
- **READ** Expiring Licenses (ExpiryDate within 90 days)
- **READ** Available Licenses (QuantityPurchased > QuantityUsed)
- **READ** Over-Allocated Licenses (QuantityUsed > QuantityPurchased)
- **UPDATE** License Details
- **UPDATE** License Allocation (QuantityUsed)
- **DELETE** License (Check if in use first)

### **Business Rules:**
- LicenseKey required (encrypted/hashed for security)
- LicenseType must be: 'Perpetual', 'Subscription', 'Trial', 'Volume', 'Academic', 'OEM'
- QuantityUsed cannot exceed QuantityPurchased (enforced by CHECK constraint)
- ExpiryDate monitoring for subscription licenses
- Cost tracking in specified Currency (default: KES)
- PurchaseOrderNumber links to procurement system

### **Workflows:**

#### **WF-5.10: Purchase License**
```
Trigger: Organization purchases software licenses
Steps:
  1. Navigate to "Licenses → Add New License"
  2. Select Product:
     Product: [Microsoft Office 365 E3 ▼]

  3. License Details:
     - LicenseKey: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX (encrypted in DB)
     - LicenseType: [Subscription ▼]

  4. Purchase Information:
     - Purchase Date: [2025-11-01 📅]
     - Expiry Date: [2026-11-01 📅] (1-year subscription)
     - Quantity Purchased: 100 (user licenses)
     - Quantity Used: 0 (initial)

  5. Procurement Details:
     - Purchase Order Number: PO-2025-ICT-045
     - Vendor: Microsoft Kenya Ltd.
     - Cost: 1,250,000.00
     - Currency: [KES ▼]

  6. Support Contact:
     - Support Contact: Microsoft Kenya Support
     - Support Phone: +254-20-XXXXXX
     - Support Email: support@microsoft.co.ke

  7. Notes:
     "Annual subscription for 100 users across HeadOffice and Regional offices.
      Renew before 2026-11-01. Contact procurement 60 days in advance."

  8. Validate:
     - LicenseKey not already in system
     - QuantityPurchased > 0

  9. Save license with IsActive = 1
  10. Add to "License Renewal Calendar" (if ExpiryDate exists)
  11. Notify procurement team
  12. Create reminder: 90 days before expiry
```

#### **WF-5.11: Allocate License to Installation**
```
Trigger: Software installed on tenant machine, needs license assignment
Steps:
  1. Installation created (via WF-5.16)
     InstallationId: 456
     ProductId: 5 (Microsoft Office 365)
     TenantId: 50 (Kiambu Factory)

  2. Check Product.RequiresLicense:
     IF RequiresLicense = 0 → Skip license allocation (e.g., Open Source)
     IF RequiresLicense = 1 → Proceed

  3. Find available licenses:
     SELECT * FROM SoftwareLicenses
     WHERE ProductId = 5
       AND IsActive = 1
       AND (ExpiryDate IS NULL OR ExpiryDate > GETDATE())
       AND (QuantityPurchased - QuantityUsed) > 0
     ORDER BY ExpiryDate ASC

  4. If available licenses found:
     - Show list to admin/user
     - Admin selects license: LicenseId = 12

  5. If no available licenses:
     - Show error: "No available licenses for Microsoft Office 365"
     - Options:
       [ ] Purchase more licenses
       [ ] Reclaim unused license from another installation
       [ ] Proceed without license (compliance risk - flag installation)

  6. If license selected:
     UPDATE TenantSoftwareInstallations
     SET LicenseId = 12
     WHERE InstallationId = 456

     UPDATE SoftwareLicenses
     SET QuantityUsed = QuantityUsed + 1,
         ModifiedDate = GETDATE()
     WHERE LicenseId = 12

  7. Validate constraint:
     CHECK (QuantityUsed <= QuantityPurchased)
     If violation → Rollback, show error: "License allocation exceeds purchased quantity"

  8. Log in AuditLogs:
     Action: 'LicenseAllocated'
     Details: 'License 12 assigned to Installation 456 (Kiambu Factory)'

  9. Refresh license dashboard widgets
```

#### **WF-5.12: Reclaim License (Deallocation)**
```
Trigger: Software uninstalled or license needs to be freed
Steps:
  1. Navigate to "Installations → Uninstall" (or automated via WF-5.20)
  2. Installation details:
     InstallationId: 456
     ProductId: 5 (Microsoft Office 365)
     LicenseId: 12

  3. Confirm uninstall: "Remove Microsoft Office 365 from Kiambu Factory SERVER-01?"

  4. Process:
     a. Create history record:
        INSERT INTO SoftwareInstallationHistory
        (InstallationId, FromVersionId, ToVersionId, ChangeType, ChangedBy, Reason)
        VALUES (456, 8, NULL, 'Uninstall', @UserId, 'Machine decommissioned')

     b. Update installation status:
        UPDATE TenantSoftwareInstallations
        SET Status = 'Uninstalled',
            ModifiedDate = GETDATE(),
            ModifiedBy = @UserId
        WHERE InstallationId = 456

     c. Reclaim license:
        IF LicenseId IS NOT NULL:
          UPDATE SoftwareLicenses
          SET QuantityUsed = QuantityUsed - 1,
              ModifiedDate = GETDATE()
          WHERE LicenseId = 12

     d. Validate:
        CHECK (QuantityUsed >= 0)

  5. Notify:
     - Tenant ICT: "License reclaimed from SERVER-01. Available for reuse."
     - License pool updated: "Microsoft Office 365: 65/100 licenses in use"

  6. Log in AuditLogs
```

#### **WF-5.13: License Expiry Monitoring (Scheduled Job)**
```
Trigger: Daily Hangfire job at 6 AM
Job: CheckLicenseExpiry()
Steps:
  1. Query expiring licenses:
     SELECT * FROM SoftwareLicenses
     WHERE IsActive = 1
       AND ExpiryDate IS NOT NULL
       AND ExpiryDate BETWEEN GETDATE() AND DATEADD(DAY, 90, GETDATE())
     ORDER BY ExpiryDate ASC

  2. For each license:
     a. Calculate days until expiry
     b. Query affected installations:
        SELECT COUNT(*) FROM TenantSoftwareInstallations
        WHERE LicenseId = X AND Status = 'Active'

     c. Calculate renewal cost (if Cost field populated)

  3. Generate alerts based on timeline:
     - 90 days: Info notification to procurement
     - 60 days: Email to procurement + HO_ICT_MGR
       Subject: "License Renewal Required: Microsoft Office 365 (60 days)"
       Message: "100 licenses expire on 2026-11-01. Current usage: 85/100.
                 Estimated renewal cost: KES 1,250,000. Contact vendor for quote."

     - 30 days: High priority alert + dashboard banner
       Create to-do item for procurement team

     - 7 days: Critical alert + escalation to management
       Daily email reminder

  4. On Expiry Date:
     - Set IsActive = 0
     - Query affected installations:
       SELECT ti.*, t.TenantName
       FROM TenantSoftwareInstallations ti
       INNER JOIN Tenants t ON ti.TenantId = t.TenantId
       WHERE ti.LicenseId = X AND ti.Status = 'Active'

     - Update installation status:
       UPDATE TenantSoftwareInstallations
       SET Status = 'NeedsUpgrade',  -- or custom status 'ExpiredLicense'
           Notes = 'License expired on ' + CAST(ExpiryDate AS NVARCHAR)
       WHERE LicenseId = X

     - Generate compliance report: "Expired License Installations"
     - Notify all affected tenants
     - Escalate to SYSADMIN and management

  5. Log in SystemMetricLogs:
     JobName: 'CheckLicenseExpiry'
     Status: 'Success'
     Details: '{"licensesChecked": 125, "expiringSoon": 8, "expired": 2, "installationsAffected": 45}'
```

#### **WF-5.14: License Compliance Report**
```
Trigger: Admin runs "License Compliance" report (monthly audit)
Steps:
  1. Navigate to "Reports → License Compliance"
  2. Select date range: [2025-01-01] to [2025-11-30]

  3. Query logic:
     For each Product WHERE RequiresLicense = 1:
       a. Total Licenses Purchased:
          SUM(QuantityPurchased) FROM SoftwareLicenses WHERE ProductId = X AND IsActive = 1

       b. Total Licenses Used:
          COUNT(*) FROM TenantSoftwareInstallations WHERE ProductId = X AND Status = 'Active' AND LicenseId IS NOT NULL

       c. Unlicensed Installations:
          COUNT(*) FROM TenantSoftwareInstallations WHERE ProductId = X AND Status = 'Active' AND LicenseId IS NULL

       d. Available Licenses:
          Purchased - Used

       e. Over-Allocated:
          IF Used > Purchased → Flag as CRITICAL

       f. Expiring Soon (next 90 days):
          COUNT(*) FROM SoftwareLicenses WHERE ProductId = X AND ExpiryDate BETWEEN NOW() AND NOW() + 90 DAYS

  4. Generate report:
     | Product          | Purchased | Used | Available | Unlicensed | Over-Allocated | Expiring (90d) | Status |
     |------------------|-----------|------|-----------|------------|----------------|----------------|--------|
     | Office 365 E3    | 100       | 85   | 15        | 0          | No             | 1              | ✅ OK   |
     | Windows Server   | 50        | 48   | 2         | 0          | No             | 0              | ✅ OK   |
     | AutoCAD 2024     | 20        | 25   | -5        | 3          | YES            | 0              | ⚠️ RISK |
     | Kaspersky AV     | 200       | 180  | 20        | 15         | No             | 1              | ⚠️ CHECK|

  5. Highlight issues:
     - AutoCAD: 5 over-allocated + 3 unlicensed = Compliance violation
     - Kaspersky: 15 unlicensed installations = Security risk

  6. Actions:
     [ ] Generate procurement request for additional licenses
     [ ] Identify and remove unlicensed installations
     [ ] Reclaim unused licenses

  7. Export options:
     - Excel (with drill-down to installation details)
     - PDF (executive summary)
     - Email to stakeholders

  8. Auto-create alerts for critical issues
```

---

## 4. TENANT SOFTWARE INSTALLATIONS

### **CRUD Operations:**
- **CREATE** Installation Record
- **READ** Installation (Single)
- **READ** Installations by Tenant
- **READ** Installations by Product
- **READ** Installations by Status
- **READ** Installations Needing Upgrade
- **UPDATE** Installation Status
- **UPDATE** Verification Date
- **DELETE** Installation (Soft delete via Status='Uninstalled')

### **Business Rules:**
- Unique constraint: (TenantId, ProductId, InstallationType, MachineName) - prevents duplicate records
- Status must be: 'Active', 'Deprecated', 'NeedsUpgrade', 'EndOfLife', 'Uninstalled'
- InstallationType must be: 'Server', 'Workstation', 'Cloud', 'Virtual', 'Container'
- If Product.RequiresLicense = 1, LicenseId should be populated (soft rule, can flag violations)
- LastVerifiedDate tracking for audit compliance
- VersionId links to SoftwareVersions for upgrade tracking

### **Workflows:**

#### **WF-5.15: Record Software Installation (Manual)**
```
Trigger: ICT staff installs software and records it in system
Steps:
  1. Navigate to "Installations → Add Installation"
  2. Select Tenant:
     Tenant: [Kiambu Factory ▼]

  3. Select Product:
     Product: [Windows Server 2022 ▼]
     Auto-load: RequiresLicense = Yes, LicenseModel = PerDevice

  4. Select Version:
     Version: [2022 (Current, Stable) ▼]
     Show warning if not current: "⚠️ Version 2019 selected. Current version is 2022."

  5. Installation Details:
     - Installation Type: [Server ▼]
     - Machine Name: KIAMBU-SERVER-01
     - IP Address: 192.168.10.50
     - Installation Path: C:\Windows
     - Installation Date: [2025-11-01 📅]

  6. License Assignment:
     Auto-query available licenses:
     "2 licenses available for Windows Server 2022"

     Select License: [PO-2025-045 (50 total, 35 used, expires 2029-10-31) ▼]
     Or: [ ] No license assigned (flag for compliance review)

  7. Verification:
     - Verified By: [Current User]
     - Last Verified: [TODAY]

  8. Notes:
     "Primary domain controller for Kiambu Factory network. Migrated from 2019 on 2025-11-01."

  9. Validate:
     - Check unique constraint (Tenant + Product + Type + Machine)
     - If duplicate: "Installation already exists. Update existing record instead?"

  10. Save:
      INSERT INTO TenantSoftwareInstallations
      (TenantId, ProductId, VersionId, LicenseId, InstallationDate, Status, ...)
      VALUES (50, 12, 45, 18, '2025-11-01', 'Active', ...)

  11. If license assigned:
      UPDATE SoftwareLicenses
      SET QuantityUsed = QuantityUsed + 1
      WHERE LicenseId = 18

  12. Create history record:
      INSERT INTO SoftwareInstallationHistory
      (InstallationId, ToVersionId, ChangeType, ChangedBy, Reason)
      VALUES (@NewInstallationId, 45, 'Install', @UserId, 'New server setup')

  13. Notify tenant ICT contact
  14. Update installation count metrics
```

#### **WF-5.16: Record Installation (Automated Discovery)**
```
Trigger: Automated inventory script detects software on machine
Example: PowerShell/WMI scan discovers installed software
Steps:
  1. Automated agent runs on KIAMBU-SERVER-01
  2. Agent queries installed software:
     Get-WmiObject -Class Win32_Product | Select Name, Version, InstallDate

  3. Agent sends data to API endpoint:
     POST /api/software/installations/bulk
     {
       "tenantCode": "KIAMBU_FACTORY",
       "machineName": "KIAMBU-SERVER-01",
       "ipAddress": "192.168.10.50",
       "discoveries": [
         {
           "productName": "Microsoft SQL Server 2019",
           "version": "15.0.2095.3",
           "installPath": "C:\\Program Files\\Microsoft SQL Server",
           "installDate": "2024-03-15"
         },
         {
           "productName": "WinRAR",
           "version": "6.24",
           "installPath": "C:\\Program Files\\WinRAR",
           "installDate": "2024-05-20"
         }
       ]
     }

  4. Backend processor:
     For each discovery:
       a. Match product by name:
          SELECT ProductId FROM SoftwareProducts
          WHERE ProductName LIKE '%{productName}%'
          OR ProductCode = '{parsedCode}'

       b. If no match found:
          - Create pending approval record
          - Notify admin: "Unknown software detected: WinRAR on KIAMBU-SERVER-01"
          - Admin reviews and creates product or maps to existing

       c. Match/Create version:
          SELECT VersionId FROM SoftwareVersions
          WHERE ProductId = X AND VersionNumber = '{version}'

          If not found:
            INSERT INTO SoftwareVersions (ProductId, VersionNumber, ...)
            VALUES (X, '15.0.2095.3', ...)

       d. Check if installation already exists:
          SELECT * FROM TenantSoftwareInstallations
          WHERE TenantId = X
            AND ProductId = Y
            AND MachineName = 'KIAMBU-SERVER-01'

       e. If exists → UPDATE LastVerifiedDate = GETDATE()
       f. If new → INSERT INTO TenantSoftwareInstallations (auto-discovery flag)

       g. License validation:
          IF Product.RequiresLicense = 1 AND Installation.LicenseId IS NULL:
            - Flag installation: Status = 'Active' but add note "Unlicensed - requires review"
            - Create alert for compliance team

  5. Generate summary report:
     "Automated Discovery Results for KIAMBU-SERVER-01:
      - 15 software products found
      - 12 matched to catalog
      - 3 new products require review
      - 2 unlicensed installations flagged"

  6. Create compliance tasks for unlicensed installations
```

#### **WF-5.17: Verify Installation**
```
Trigger: Periodic verification (quarterly audit or scheduled job)
Steps:
  1. Query installations needing verification:
     SELECT * FROM TenantSoftwareInstallations
     WHERE Status = 'Active'
       AND (LastVerifiedDate IS NULL OR LastVerifiedDate < DATEADD(MONTH, -3, GETDATE()))
     ORDER BY TenantId, LastVerifiedDate ASC

  2. For each installation:
     - Send verification request to tenant ICT:
       "Please verify Microsoft Office 365 is still installed on WORKSTATION-05"
       [ ] Confirmed - Still Installed
       [ ] Uninstalled
       [ ] Needs Upgrade

  3. ICT Staff response options:
     a. Confirmed:
        UPDATE TenantSoftwareInstallations
        SET LastVerifiedDate = GETDATE(),
            VerifiedBy = @UserId
        WHERE InstallationId = X

     b. Uninstalled:
        Trigger WF-5.20 (Software Uninstallation)

     c. Needs Upgrade:
        UPDATE Status = 'NeedsUpgrade'
        Create task for upgrade planning

  4. Generate verification report:
     - Verified: 450 installations
     - Uninstalled: 25 (licenses reclaimed)
     - Needs Upgrade: 12
     - Not Verified: 8 (escalate for forced check)
```

#### **WF-5.18: Software Upgrade**
```
Trigger: Admin upgrades software version on machine
Steps:
  1. Navigate to "Installations → Upgrade"
  2. Select installation:
     Tenant: Kiambu Factory
     Product: Windows Server
     Current Version: 2019 (Build 17763.5000)
     Machine: KIAMBU-SERVER-01

  3. Available upgrade paths:
     [ ] Windows Server 2022 (Current, Stable)
     [ ] Windows Server 2019 (Build 17763.6543) - Latest patch

  4. Select target version:
     Target: Windows Server 2022

  5. License check:
     Current License: PO-2023-012 (Windows Server 2019 - Perpetual)
     Compatible with 2022: ✅ Yes (upgrade rights included)

     Or:
     Compatible with 2022: ❌ No - New license required
     Available licenses: 2 licenses available
     Select License: [PO-2025-045 ▼]

  6. Upgrade details:
     - Scheduled Date: [2025-11-15 📅]
     - Reason: "Upgrade to supported version, 2019 approaching EOL"
     - Backup verified: [✓]
     - Downtime approved: [✓]

  7. Execute upgrade:
     a. Create history record BEFORE upgrade:
        INSERT INTO SoftwareInstallationHistory
        (InstallationId, FromVersionId, ToVersionId, ChangeType, ChangedBy, Reason, SuccessStatus)
        VALUES (@InstallId, 30, 45, 'Upgrade', @UserId, 'EOL mitigation', NULL)

     b. Update installation:
        UPDATE TenantSoftwareInstallations
        SET VersionId = 45,
            LicenseId = @NewLicenseId (if changed),
            ModifiedDate = GETDATE(),
            ModifiedBy = @UserId,
            Notes = CONCAT(Notes, '\nUpgraded from 2019 to 2022 on ', GETDATE())
        WHERE InstallationId = @InstallId

     c. If license changed:
        -- Reclaim old license
        UPDATE SoftwareLicenses SET QuantityUsed = QuantityUsed - 1 WHERE LicenseId = @OldLicenseId
        -- Allocate new license
        UPDATE SoftwareLicenses SET QuantityUsed = QuantityUsed + 1 WHERE LicenseId = @NewLicenseId

  8. Post-upgrade verification:
     - Update history record SuccessStatus = 1
     - Set installation Status = 'Active' (if was 'NeedsUpgrade')
     - Update LastVerifiedDate = GETDATE()

  9. If upgrade fails:
     - Update history record:
       SuccessStatus = 0,
       ErrorMessage = 'Rollback due to compatibility issue with KTDA ERP'
     - Rollback installation to previous version
     - Create support ticket

  10. Notify:
      - Tenant ICT: "Upgrade completed successfully"
      - Update "Outdated Installations" dashboard
```

#### **WF-5.19: Mark Installation as Deprecated**
```
Trigger: Software version no longer supported, but still in use
Steps:
  1. System detects:
     - Version.IsSupported = 0
     - Version.EndOfLifeDate < GETDATE()
     - Installation.Status = 'Active'

  2. Auto-update installations:
     UPDATE TenantSoftwareInstallations
     SET Status = 'Deprecated'
     WHERE VersionId IN (SELECT VersionId FROM SoftwareVersions WHERE IsSupported = 0)
       AND Status = 'Active'

  3. Generate "Deprecated Software Report":
     | Tenant           | Product          | Version | Machine        | EOL Date   | Action Needed |
     |------------------|------------------|---------|----------------|------------|---------------|
     | Kiambu Factory   | Windows 7        | 6.1     | WORKSTATION-12 | 2020-01-14 | Urgent Upgrade|
     | Thika Factory    | Office 2010      | 14.0    | WORKSTATION-08 | 2020-10-13 | Replace       |

  4. Create high-priority tasks for each tenant
  5. Dashboard widget: "Deprecated Installations" (red alert)
```

#### **WF-5.20: Software Uninstallation**
```
Trigger: Software removed from machine
Steps:
  1. Navigate to "Installations → Uninstall"
  2. Select installation:
     Tenant: Kiambu Factory
     Product: Adobe Photoshop CC
     Machine: WORKSTATION-05
     License: PO-2024-018 (1 of 10 used)

  3. Uninstall details:
     - Reason: [User left organization ▼]
     - Uninstall Date: [2025-11-15 📅]
     - Notes: "User John Doe transferred to Thika. License available for reallocation."

  4. Confirm: "Remove Adobe Photoshop CC from WORKSTATION-05 and reclaim license?"

  5. Execute:
     a. Create history:
        INSERT INTO SoftwareInstallationHistory
        (InstallationId, FromVersionId, ToVersionId, ChangeType, ChangedBy, Reason, SuccessStatus)
        VALUES (@InstallId, 67, NULL, 'Uninstall', @UserId, 'User left organization', 1)

     b. Update installation:
        UPDATE TenantSoftwareInstallations
        SET Status = 'Uninstalled',
            ModifiedDate = GETDATE(),
            ModifiedBy = @UserId,
            Notes = CONCAT(Notes, '\nUninstalled on ', GETDATE(), ' - ', @Reason)
        WHERE InstallationId = @InstallId

     c. Reclaim license:
        IF LicenseId IS NOT NULL:
          UPDATE SoftwareLicenses
          SET QuantityUsed = QuantityUsed - 1
          WHERE LicenseId = @LicenseId

  6. Notify:
     - Tenant ICT: "Software uninstalled, license reclaimed"
     - License manager: "Adobe Photoshop license available (9/10 in use)"

  7. Update dashboards and compliance reports
```

---

## 5. SOFTWARE INSTALLATION HISTORY

### **CRUD Operations:**
- **CREATE** History Record (via installation workflows)
- **READ** History for Installation
- **READ** History for Product (all installations)
- **READ** History by Change Type
- **READ** Failed Changes (SuccessStatus = 0)
- **READ** User Activity (ChangedBy filter)

### **Business Rules:**
- Automatically created by installation workflows (user cannot manually create)
- ChangeType must be: 'Install', 'Upgrade', 'Downgrade', 'Uninstall', 'Reinstall', 'Patch'
- FromVersionId can be NULL (for new installations)
- ToVersionId can be NULL (for uninstalls)
- SuccessStatus tracks if change succeeded (for rollback tracking)
- Immutable records (audit trail - no updates, no deletes)

### **Workflows:**

#### **WF-5.21: Query Installation History**
```
Trigger: Admin reviews installation change log
Steps:
  1. Navigate to "Installations → {Installation} → History"
  2. Display timeline:

     📅 2025-11-15 10:30 AM - Upgrade ✅ Success
        From: Windows Server 2019 (Build 17763.5000)
        To: Windows Server 2022 (Build 20348.1234)
        By: John Doe (ICT Admin)
        Reason: "Upgrade to supported version, 2019 approaching EOL"

     📅 2024-06-10 02:45 PM - Patch ✅ Success
        From: Windows Server 2019 (Build 17763.4500)
        To: Windows Server 2019 (Build 17763.5000)
        By: System (Automated Patch)
        Reason: "Security update KB5028168"

     📅 2023-03-22 09:00 AM - Install ✅ Success
        To: Windows Server 2019 (Build 17763.4500)
        By: Jane Smith (Setup)
        Reason: "New server deployment"

  3. Filter options:
     - By Change Type: [All ▼] Install, Upgrade, Uninstall
     - By Date Range: [Last 12 months ▼]
     - By User: [All Users ▼]
     - Success Only: [ ]

  4. Export options:
     - Excel (audit trail)
     - PDF (compliance report)
```

#### **WF-5.22: Failed Change Analysis**
```
Trigger: Admin reviews failed installations/upgrades
Steps:
  1. Query failed changes:
     SELECT h.*, sp.ProductName, sv.VersionNumber, u.UserName
     FROM SoftwareInstallationHistory h
     INNER JOIN TenantSoftwareInstallations i ON h.InstallationId = i.InstallationId
     INNER JOIN SoftwareProducts sp ON i.ProductId = sp.ProductId
     INNER JOIN SoftwareVersions sv ON h.ToVersionId = sv.VersionId
     INNER JOIN Users u ON h.ChangedBy = u.UserId
     WHERE h.SuccessStatus = 0
       AND h.ChangeDate >= DATEADD(MONTH, -3, GETDATE())
     ORDER BY h.ChangeDate DESC

  2. Display report:
     | Date       | Tenant   | Product           | Change    | Error                          | User      |
     |------------|----------|-------------------|-----------|--------------------------------|-----------|
     | 2025-11-10 | Kiambu   | SQL Server 2022   | Upgrade   | Insufficient disk space (50GB) | John Doe  |
     | 2025-11-05 | Thika    | Office 365        | Install   | Network timeout during download| Jane Smith|
     | 2025-10-28 | Meru     | Windows Server    | Patch     | Rollback: compatibility issue  | System    |

  3. Analysis:
     - Common failure reasons
     - Products with high failure rate
     - Users needing training

  4. Actions:
     - Create remediation tasks
     - Update installation procedures
     - Escalate recurring issues
```

#### **WF-5.23: Product Upgrade Trend Analysis**
```
Trigger: Management wants to see software lifecycle patterns
Steps:
  1. Query: Upgrades per product over time
     SELECT
       sp.ProductName,
       YEAR(h.ChangeDate) AS Year,
       MONTH(h.ChangeDate) AS Month,
       COUNT(*) AS UpgradeCount
     FROM SoftwareInstallationHistory h
     INNER JOIN TenantSoftwareInstallations i ON h.InstallationId = i.InstallationId
     INNER JOIN SoftwareProducts sp ON i.ProductId = sp.ProductId
     WHERE h.ChangeType = 'Upgrade'
       AND h.ChangeDate >= DATEADD(YEAR, -2, GETDATE())
     GROUP BY sp.ProductName, YEAR(h.ChangeDate), MONTH(h.ChangeDate)
     ORDER BY Year DESC, Month DESC

  2. Visualize trends:
     - Line chart: Upgrades per month by product
     - Identify: Peak upgrade periods (e.g., Q1 budget cycle)
     - Insights: Products requiring frequent updates (security-critical)

  3. Use for planning:
     - Budget forecasting
     - Resource allocation (ICT staff)
     - License renewal timing
```

---

## CROSS-TABLE WORKFLOWS

### **WF-5.24: Complete Software Lifecycle (New Product)**
```
Trigger: Organization adopts new software product
Example: Purchase and deploy Adobe Creative Cloud
Steps:
  1. Step 1: Register Product (WF-5.1)
     ProductCode: ADOBE_CC_ALL
     ProductName: Adobe Creative Cloud All Apps
     Vendor: Adobe Systems Inc.
     Category: Application
     LicenseModel: Subscription
     RequiresLicense: Yes

  2. Step 2: Add Initial Version (WF-5.6)
     VersionNumber: 2025
     ReleaseDate: 2025-01-01
     IsCurrentVersion: Yes
     SecurityLevel: Stable

  3. Step 3: Purchase License (WF-5.10)
     LicenseType: Subscription
     QuantityPurchased: 20 (user licenses)
     ExpiryDate: 2026-01-01 (1-year)
     Cost: KES 500,000
     PurchaseOrderNumber: PO-2025-ICT-089

  4. Step 4: Deploy to Tenants (WF-5.15)
     Install on 15 workstations across HeadOffice Design Department
     Assign licenses to each installation (20 purchased, 15 used, 5 available)

  5. Step 5: Ongoing Management
     - Monthly: Verify installations (WF-5.17)
     - Quarterly: Review license usage
     - Annually: Renew subscription (90-day reminder via WF-5.13)
     - As needed: Upgrade versions (WF-5.18)

  6. Step 6: Eventual Decommission (if product replaced)
     - Uninstall from all machines (WF-5.20)
     - Reclaim all licenses
     - Deactivate product (retain history)
```

### **WF-5.25: Vulnerability Response Workflow**
```
Trigger: CVE published for software in use
Example: Critical vulnerability in Windows Server 2019
Steps:
  1. Security bulletin received:
     CVE-2025-XXXXX: Remote Code Execution in Windows Server 2019 versions < 17763.6543
     Severity: Critical (CVSS 9.8)
     Patch available: Build 17763.6543

  2. Identify affected installations (WF-5.7):
     Query:
     SELECT ti.*, t.TenantName, t.ContactEmail
     FROM TenantSoftwareInstallations ti
     INNER JOIN Tenants t ON ti.TenantId = t.TenantId
     INNER JOIN SoftwareVersions sv ON ti.VersionId = sv.VersionId
     WHERE ti.ProductId = 12 (Windows Server 2019)
       AND sv.MajorVersion = 17763
       AND sv.PatchVersion < 6543
       AND ti.Status = 'Active'

     Result: 12 servers across 8 factories

  3. Update version security status:
     UPDATE SoftwareVersions
     SET SecurityLevel = 'Vulnerable',
         IsSupported = 0
     WHERE ProductId = 12
       AND MajorVersion = 17763
       AND PatchVersion < 6543

  4. Auto-flag installations:
     UPDATE TenantSoftwareInstallations
     SET Status = 'NeedsUpgrade'
     WHERE VersionId IN (SELECT VersionId FROM SoftwareVersions WHERE SecurityLevel = 'Vulnerable')

  5. Create urgent alerts:
     For each affected tenant:
       INSERT INTO Notifications
       (NotificationType, Priority, Subject, Message, RecipientUserId)
       VALUES
       ('Security', 'High',
        'CRITICAL: Vulnerable Software Detected',
        'Your Windows Server 2019 installation has a critical security vulnerability (CVE-2025-XXXXX).
         Immediate patching required. Patch to Build 17763.6543.',
        @TenantICTContactId)

  6. Create remediation tasks:
     For each installation:
       - Assign to tenant ICT staff
       - Due date: 48 hours (critical)
       - Task: Apply Windows Update KB XXXXXXX

  7. Track remediation:
     Dashboard widget: "Vulnerability Remediation Status"
     - 12 servers affected
     - 8 patched ✅
     - 3 in progress 🟡
     - 1 delayed (approved exception) 🔴

  8. Verify completion:
     After patching, ICT staff updates installation (WF-5.18 Patch):
     - New VersionId (Build 17763.6543)
     - Status changes from 'NeedsUpgrade' to 'Active'
     - LastVerifiedDate = today

  9. Generate compliance report:
     "CVE-2025-XXXXX Remediation Summary:
      - Vulnerabilities identified: 12
      - Remediated within 48h: 11 (92%)
      - Exceptions granted: 1 (documented)
      - Total remediation time: 36 hours average"
```

### **WF-5.26: Software Audit (Annual Compliance)**
```
Trigger: Annual ICT audit or regulatory compliance requirement
Steps:
  1. Scope definition:
     - Audit Period: 2025-01-01 to 2025-12-31
     - Include: All active tenants
     - Focus: License compliance, version currency, security posture

  2. Data collection:
     a. Installation Inventory:
        - Total installations: 2,450
        - By tenant
        - By product category
        - By installation type (Server, Workstation, Cloud)

     b. License Compliance (WF-5.14):
        - Total licenses purchased: 850
        - Total licenses in use: 782
        - Unlicensed installations: 15
        - Over-allocated products: 2
        - Expiring licenses (next 90 days): 8

     c. Version Currency:
        - Installations on current version: 1,850 (76%)
        - Installations needing upgrade: 450 (18%)
        - Deprecated installations: 150 (6%)

     d. Security Posture:
        - Vulnerable installations: 5 (flagged for immediate action)
        - End-of-life installations: 35
        - Unsupported versions: 120

  3. Cost analysis:
     - Total software expenditure: KES 15,250,000
     - License renewal forecast (next 12 months): KES 8,500,000
     - Unused license value: KES 1,200,000 (candidates for deallocation)

  4. Verification sampling:
     - Random sample: 10% of installations
     - Physical verification at selected sites
     - Compare DB records vs actual installations

  5. Issues identified:
     - 15 unlicensed AutoCAD installations (compliance risk)
     - 35 Windows 7 machines still in use (EOL since 2020)
     - 5 servers with vulnerable software versions
     - 3 tenants with no software inventory recorded

  6. Remediation plan:
     Action Items:
     1. Purchase 20 additional AutoCAD licenses (Budget: KES 800,000)
     2. Upgrade Windows 7 machines to Windows 10/11 (45-day deadline)
     3. Apply security patches to 5 vulnerable servers (7-day deadline)
     4. Conduct inventory audit at 3 non-compliant tenants

  7. Audit report:
     Executive Summary:
     - Overall Compliance: 94%
     - License Compliance: 98%
     - Version Currency: 76%
     - Security Posture: 98%

     Recommendations:
     - Implement automated discovery (WF-5.16) for continuous monitoring
     - Quarterly verification cycles (WF-5.17)
     - Proactive upgrade planning for EOL software
     - License optimization: Reclaim 68 unused licenses (KES 1.2M savings)

  8. Board presentation:
     - PDF report with executive summary
     - Financial impact analysis
     - Risk assessment
     - Compliance certification
```

### **WF-5.27: License Optimization Project**
```
Trigger: Cost reduction initiative or budget constraints
Objective: Maximize license utilization, reduce waste
Steps:
  1. Identify underutilized licenses:
     Query:
     SELECT
       sp.ProductName,
       sl.QuantityPurchased,
       sl.QuantityUsed,
       (sl.QuantityPurchased - sl.QuantityUsed) AS Unused,
       sl.Cost,
       (sl.Cost / sl.QuantityPurchased) * (sl.QuantityPurchased - sl.QuantityUsed) AS WastedValue
     FROM SoftwareLicenses sl
     INNER JOIN SoftwareProducts sp ON sl.ProductId = sp.ProductId
     WHERE sl.IsActive = 1
       AND (sl.QuantityPurchased - sl.QuantityUsed) > 0
     ORDER BY WastedValue DESC

  2. Analysis results:
     | Product          | Purchased | Used | Unused | Total Cost    | Wasted Value |
     |------------------|-----------|------|--------|---------------|--------------|
     | Adobe CC         | 50        | 28   | 22     | KES 1,250,000 | KES 550,000  |
     | AutoCAD          | 30        | 25   | 5      | KES 900,000   | KES 150,000  |
     | MS Project       | 20        | 8    | 12     | KES 400,000   | KES 240,000  |
     **Total Waste:** KES 940,000

  3. Optimization strategies:
     a. Reallocate unused licenses:
        - Query installations without licenses (same product)
        - Assign available licenses to unlicensed installations
        - Target: Reduce unlicensed count from 15 to 0

     b. Downgrade subscriptions:
        - Adobe CC: 50 licenses → 30 licenses (reduce by 20)
        - Action: Don't renew 20 licenses next cycle
        - Savings: KES 500,000/year

     c. Convert to enterprise licensing:
        - MS Office: Currently 150 PerUser licenses @ KES 5,000 = KES 750,000
        - Switch to: Enterprise (unlimited) @ KES 900,000
        - Savings: KES 150,000 immediate + scalability

     d. Identify alternatives:
        - MS Project (12 unused): High cost, low usage
        - Alternative: Migrate to Microsoft Planner (included in M365)
        - Savings: KES 240,000/year

  4. Implementation:
     - Phase 1: Reallocate existing licenses (immediate, no cost)
     - Phase 2: Subscription adjustments (at next renewal)
     - Phase 3: Enterprise migration (next budget cycle)
     - Phase 4: Product replacement (requires change management)

  5. Projected savings:
     - Year 1: KES 1,140,000 (reallocation + downgrades)
     - Year 2: KES 1,390,000 (additional optimizations)
     - 3-year total: KES 3,900,000

  6. Monitoring:
     - Monthly license utilization dashboard
     - Quarterly optimization reviews
     - Continuous discovery for accurate counts
```

### **WF-5.28: Software Standardization Initiative**
```
Trigger: Too many similar products, hard to support
Objective: Standardize on fewer products for easier management
Steps:
  1. Inventory analysis:
     Find products with overlapping functionality:
     - PDF Readers: Adobe Acrobat (20), Foxit Reader (15), PDF-XChange (8)
     - Compression: WinRAR (50), 7-Zip (30), WinZip (10)
     - Remote Desktop: TeamViewer (25), AnyDesk (18), Chrome RD (12)

  2. Standardization decision:
     Category: PDF Readers
     Decision: Standardize on Adobe Acrobat Reader (free) + Adobe Acrobat Pro (paid for 10 power users)
     Action: Uninstall Foxit (15 machines), PDF-XChange (8 machines)

     Category: Compression
     Decision: Standardize on 7-Zip (open source, free)
     Action: Uninstall WinRAR (50 licenses → KES 150,000 savings), WinZip (10 licenses)

  3. Migration plan:
     For each non-standard installation:
       1. Create task: "Uninstall WinRAR from WORKSTATION-05"
       2. Assign to tenant ICT
       3. Execute WF-5.20 (Uninstallation)
       4. Reclaim licenses where applicable
       5. Install standard alternative
       6. Verify user can perform same tasks

  4. Policy enforcement:
     - Update "Approved Software List"
     - Restrict new installations to approved products only
     - Automated discovery flags non-standard software

  5. Benefits:
     - Reduced support complexity
     - License cost savings: KES 300,000/year
     - Easier training (one tool per category)
     - Better negotiation leverage with vendors (volume)
```

---

## SUMMARY

### **Total Operations:**
- **CRUD Actions:** 45+ operations across 5 tables
- **Workflows:** 28 defined workflows
- **Business Rules:** 25+ validation rules

### **Key Integration Points:**
1. **SoftwareProducts ↔ SoftwareVersions** → Version lifecycle tracking
2. **SoftwareVersions ↔ TenantSoftwareInstallations** → Upgrade/vulnerability management
3. **SoftwareLicenses ↔ TenantSoftwareInstallations** → License compliance
4. **Tenants ↔ TenantSoftwareInstallations** → Multi-tenant asset tracking
5. **Users ↔ SoftwareInstallationHistory** → Audit trail and user activity
6. **FormTemplateSubmissions** → Automated software inventory forms
7. **MetricDefinitions** → Software compliance KPIs
8. **Notifications** → License expiry, vulnerability alerts, compliance warnings
9. **AuditLogs** → Complete change tracking

### **Permissions Required:**
```
Software Catalog Management:
  - Software.ManageProducts → SYSADMIN, HO_ICT_MGR
  - Software.ManageVersions → SYSADMIN, HO_ICT_MGR

License Management:
  - Software.ManageLicenses → SYSADMIN, HO_ICT_MGR, PROCUREMENT
  - Software.AllocateLicenses → SYSADMIN, HO_ICT_MGR, REGIONAL_ICT
  - Software.ViewLicenseCosts → SYSADMIN, HO_ICT_MGR, CFO

Installation Management:
  - Software.RecordInstallations → All ICT staff (tenant-scoped)
  - Software.ViewInstallations → All ICT staff (tenant-scoped), AUDITOR (all)
  - Software.ManageInstallations → SYSADMIN, HO_ICT_MGR, REGIONAL_ICT (region), FACTORY_ICT (own tenant)

Compliance & Reporting:
  - Software.RunComplianceReports → SYSADMIN, HO_ICT_MGR, AUDITOR
  - Software.ViewAuditHistory → SYSADMIN, AUDITOR
```

### **Key Metrics & KPIs:**
```
License Compliance:
  - License utilization rate (used / purchased)
  - Unlicensed installations count
  - Over-allocated products count
  - License cost per user/device
  - Expiring licenses (next 90 days)

Version Currency:
  - % installations on current version
  - % installations on supported versions
  - % installations with vulnerabilities
  - % deprecated installations

Security Posture:
  - Vulnerable installations count (SecurityLevel='Vulnerable')
  - End-of-life installations count
  - Average time to patch vulnerabilities
  - Verification compliance rate

Cost Management:
  - Total software asset value
  - Unused license value (waste)
  - Cost per tenant
  - Savings from optimization initiatives
```

### **Scheduled Jobs:**
```
Daily (3:00 AM):
  - CheckVersionEndOfLife() → Flag EOL versions, alert tenants

Daily (6:00 AM):
  - CheckLicenseExpiry() → Alert on expiring licenses (90/60/30/7 day thresholds)

Weekly (Sunday 2:00 AM):
  - GenerateComplianceReport() → Weekly license compliance summary

Monthly (1st, 4:00 AM):
  - VerificationReminders() → Send verification requests for installations not verified in 90 days
  - LicenseUtilizationReport() → Monthly license usage and cost report

Quarterly (1st of Q1/Q2/Q3/Q4, 5:00 AM):
  - SoftwareAuditReport() → Comprehensive audit for management
```

---

**End of Section 5 Workflows**
