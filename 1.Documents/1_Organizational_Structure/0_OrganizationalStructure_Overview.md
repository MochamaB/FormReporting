# KTDA ICT REPORTING SYSTEM
## Organizational Structure & Multi-Tenancy Overview

**Document Version:** 1.1
**Last Updated:** 2025-10-30
**Target Audience:** Technical & Non-Technical Stakeholders
**Purpose:** Explain the organizational structure and multi-tenant architecture

---

## TABLE OF CONTENTS

1. [Executive Summary](#1-executive-summary)
2. [KTDA Organizational Hierarchy](#2-ktda-organizational-hierarchy)
3. [What is Multi-Tenancy?](#3-what-is-multi-tenancy)
4. [The Three-Tier Structure](#4-the-three-tier-structure)
5. [Roles & Permissions](#5-roles--permissions)
6. [Data Isolation & Security](#6-data-isolation--security)
7. [User Management](#7-user-management)
8. [Access Control Examples](#8-access-control-examples)
9. [Real-World Scenarios](#9-real-world-scenarios)
10. [Benefits of Multi-Tenancy](#10-benefits-of-multi-tenancy)
11. [System Architecture](#11-system-architecture)
12. [Frequently Asked Questions](#12-frequently-asked-questions)

---

## 1. EXECUTIVE SUMMARY

### 1.1 What This Document Covers

This document explains how the KTDA ICT Reporting System organizes users, factories, and data using a **multi-tenant architecture**. Whether you're a Field Systems Administrator, Regional ICT Manager, Head Office staff, or system administrator, this guide will help you understand:

- How KTDA's organizational structure is reflected in the system
- What "multi-tenancy" means in simple terms
- Who can see and do what in the system
- How data is kept secure and separate between factories
- How users are managed across different levels

### 1.2 Key Concepts in Simple Terms

| Concept | Simple Explanation | Example |
|---------|-------------------|---------|
| **Multi-Tenancy** | Multiple factories (tenants) share one system, but each sees only their own data | Kangaita Factory cannot see Ragati Factory's data |
| **Hierarchy** | Three levels: Head Office → Region → Factory | Head Office oversees all 7 regions, Regions oversee factories |
| **Tenant** | A factory or subsidiary that uses the system | Kangaita Factory is one tenant |
| **Role** | What job you do in the organization | Field Systems Administrator, Regional ICT Manager, Head Office ICT staff |
| **Permissions** | What actions you're allowed to perform | Field Admins can submit reports, Regional Managers can approve them |
| **Data Isolation** | Each factory's data is separate and private | Factories cannot access each other's reports |

### 1.3 Who Should Read This Document?

- **Field Systems Administrators** - Understand your access level and what data you can see
- **Regional ICT Managers** - Understand how you oversee multiple factories
- **Head Office ICT Management** - Understand the full system architecture
- **System Administrators** - Understand how to manage users and tenants
- **IT Developers** - Understand the technical implementation of multi-tenancy
- **Auditors & Compliance** - Understand data security and access controls

---

## 2. KTDA ORGANIZATIONAL HIERARCHY

### 2.1 KTDA's Real-World Structure

The Kenya Tea Development Agency (KTDA) operates across Kenya with a hierarchical structure managing **71 tea factories** and **9 subsidiaries**:

```
┌─────────────────────────────────────────────────────────────────┐
│                        HEAD OFFICE                              │
│                     (Nairobi - Central HQ)                      │
│                                                                 │
│  ICT Departments:                                               │
│  • ICT-O (Office/General Management)                            │
│  • ICT-S (Services Coordination)                                │
│  • ICT-I (Infrastructure & Security)                            │
│  • ICT-B (Business Systems Development)                         │
│                                                                 │
│  Responsibilities:                                              │
│  • Overall ICT strategy and governance                          │
│  • System administration and support                            │
│  • Data consolidation and national reporting                    │
│  • Budget allocation and procurement                            │
└────────────────────────────┬────────────────────────────────────┘
                             │
        ┌────────────────────┼──────────────────────┐
        │                    │                      │
        ▼                    ▼                      ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│ REGION 1      │    │ REGION 2      │    │ REGION 3      │ ...
│ Kiambu &      │    │ Murang'a &    │    │ Kirinyaga &   │
│ Murang'a      │    │ Nyeri         │    │ Embu          │
│               │    │               │    │               │
│ 12 Factories  │    │ 9 Factories   │    │ 8 Factories   │
└───────┬───────┘    └───────┬───────┘    └───────┬───────┘
        │                    │                    │
  ┌─────┼─────┐        ┌─────┼─────┐        ┌─────┼─────┐
  │     │     │        │     │     │        │     │     │
  ▼     ▼     ▼        ▼     ▼     ▼        ▼     ▼     ▼
┌────┐┌────┐┌────┐  ┌────┐┌────┐┌────┐  ┌────┐┌────┐┌────┐
│Gach││Gach││Ikum│  │Chin││Gath││Gatu│  │Kang││Kath││Kimu│
│ara-││ege ││bi  │  │ga  ││uthi││ng- │  │ai- ││ang-││nye │
│ge  ││    ││    │  │    ││    ││uru │  │ta  ││ari-││    │
│    ││    ││    │  │    ││    ││    │  │    ││ri  ││    │
└────┘└────┘└────┘  └────┘└────┘└────┘  └────┘└────┘└────┘
 Factory  Factory    Factory  Factory    Factory  Factory
  (Tenant) (Tenant)   (Tenant) (Tenant)   (Tenant) (Tenant)
```

### 2.2 Complete Regional Breakdown

#### Region 1 - Kiambu & Murang'a Counties
**12 Factories:**
1. Gacharage Tea Factory
2. Gachege Tea Factory
3. Ikumbi Tea Factory
4. Kambaa Tea Factory
5. Kagwe Tea Factory
6. Mataara Tea Factory
7. Ndarugu Tea Factory
8. Nduti Tea Factory
9. Ngere Tea Factory
10. Njunu Tea Factory
11. Theta Tea Factory
12. Makomboki Tea Factory

**Regional ICT Manager:** Peter Kibe

---

#### Region 2 - Murang'a & Nyeri Counties
**9 Factories:**
1. Chinga Tea Factory
2. Gathuthi Tea Factory
3. Gatunguru Tea Factory
4. Githambo Tea Factory
5. Gitugi Tea Factory
6. Iriaini Tea Factory
7. Kanyenyaini Tea Factory
8. Kiru Tea Factory
9. Ragati Tea Factory

**Regional ICT Manager:** Benjamin K. Ndungu

---

#### Region 3 - Kirinyaga & Embu Counties
**8 Factories:**
1. Kangaita Tea Factory
2. Kathangariri Tea Factory
3. Kimunye Tea Factory
4. Mununga Tea Factory
5. Mungania Tea Factory
6. Ndima Tea Factory
7. Rukuriri Tea Factory
8. Thumaita Tea Factory

**Regional ICT Manager:** Eric Kinyeki

---

#### Region 4 - Meru & Tharaka Nithi Counties
**8 Factories:**
1. Githongo Tea Factory
2. Igembe Tea Factory
3. Imenti Tea Factory
4. Kiegoi Tea Factory
5. Kinoro Tea Factory
6. Kionyo Tea Factory
7. Michimikuru Tea Factory
8. Weru Tea Factory

**Regional ICT Manager:** Jackson Gachuki

---

#### Region 5 - Kericho & Bomet Counties
**16 Factories:**
1. Boito Tea Factory
2. Kapkatet Tea Factory
3. Kapkoros Tea Factory
4. Kapset Tea Factory
5. Kobel Tea Factory
6. Litein Tea Factory
7. Mogogosiek Tea Factory
8. Momul Tea Factory
9. Motigo Tea Factory
10. Olenguruone Tea Factory
11. Rorok Tea Factory
12. Tebesonik Tea Factory
13. Tegat Tea Factory
14. Tirgaga Tea Factory
15. Toror Tea Factory
16. Chelal Tea Factory

**Regional ICT Manager:** Enock O. Ogara

---

#### Region 6 - Kisii & Nyamira Counties
**14 Factories:**
1. Eberege Tea Factory
2. Gianchore Tea Factory
3. Itumbe Tea Factory
4. Kebirigo Tea Factory
5. Kiamokama Tea Factory
6. Matunwa Tea Factory
7. Nyamache Tea Factory
8. Nyankoba Tea Factory
9. Nyansiongo Tea Factory
10. Ogembo Tea Factory
11. Rianyamwamu Tea Factory
12. Sanganyi Tea Factory
13. Sombogo Tea Factory
14. Tombe Tea Factory

**Regional ICT Manager:** Richard S. Olume

---

#### Region 7 - Nandi, Trans Nzoia & Vihiga Counties
**4 Factories:**
1. Chebut Tea Factory
2. Kapsara Tea Factory
3. Kaptumo Tea Factory
4. Mudete Tea Factory

**Regional ICT Manager:** Eric Ngetich

---

### 2.3 KTDA Subsidiaries

In addition to the 71 tea factories, KTDA manages 9 subsidiary companies:

1. **KTDA Management Services (KTDA MS)** - Manages the 71 tea factories on behalf of factory companies and farmer-shareholders
2. **Kenya Tea Packers (KETEPA) Limited** - Blending, packaging, and marketing tea products (brands: Fahari ya Kenya, Safari Pure, Maisha water)
3. **Chai Trading Company Limited** - Warehousing, blending, trading, and export of tea from Mombasa
4. **Greenland Fedha Limited** - Provides affordable credit facilities to smallholder tea farmers
5. **Majani Insurance Brokers Limited** - Insurance brokerage services for tea factories
6. **KTDA Power Company Limited** - Energy projects including small-scale hydropower plants
7. **Tea Machinery and Engineering Company (TEMEC) Limited** - Engineering services and tea processing machinery fabrication
8. **KTDA Foundation** - Corporate social responsibility initiatives (community empowerment, education, conservation)
9. **Chai Logistics Centre** - Inland container terminal providing logistics and warehousing services

### 2.4 Hierarchy Levels Explained

#### Level 1: Head Office (Top Level)
- **Location:** Nairobi (Central HQ)
- **Scope:** National (all 7 regions, all 71 factories, all subsidiaries)
- **Key Departments & Personnel:**
  - **ICT-O (Office/General Management):** Group General Manager, Admin Assistant
  - **ICT-S (Services):** 7 ICT Services Coordinators
  - **ICT-I (Infrastructure):** Head of Infrastructure, Hardware Engineers (4), Network Support Coordinators (3), Security Coordinators (4)
  - **ICT-B (Business Systems):** Head of Business Systems, Systems Developers (9)

**Responsibilities:**
- Set ICT policies and standards
- Allocate budgets to regions and factories
- Consolidate national reports
- Manage system infrastructure
- Approve major IT expenditures
- Monitor overall performance across all factories
- Develop and maintain business systems

#### Level 2: Region (Middle Level)
- **Scope:** Regional (4-16 factories within a geographic area)
- **Number of Regions:** 7 (covering different counties)
- **Key Personnel:**
  - Regional ICT Manager (RICTM) - 1 per region
  - Field Systems Administrators - 1 per factory

**Responsibilities:**
- Oversee 4-16 factories within the region
- Approve factory ICT reports and requests
- Coordinate regional ICT initiatives
- Provide technical support to factories
- Monitor factory performance and compliance
- Escalate critical issues to Head Office

#### Level 3: Factory/Subsidiary (Base Level)
- **Scope:** Individual factory or subsidiary
- **Number:** 71 factories + 9 subsidiaries = 80 total tenants
- **Key Personnel:**
  - Factory Manager (Business Operations)
  - Field Systems Administrator (ICT Support)

**Responsibilities:**
- Manage day-to-day ICT operations at the factory
- Maintain hardware and software inventory
- Submit monthly/quarterly ICT reports
- Log and resolve support tickets
- Request budget for equipment purchases
- Ensure compliance with KTDA ICT policies

### 2.5 Organizational Hierarchy Table

| Level | Name | Reports To | Manages | Example | Count |
|-------|------|------------|---------|---------|-------|
| **1** | Head Office | Board of Directors | All Regions | Nairobi HQ | 1 |
| **2** | Region | Head Office | 4-16 Factories | Region 3 (Kirinyaga & Embu) | 7 regions |
| **3** | Factory (Tenant) | Regional ICT Manager | Factory ICT Operations | Kangaita Factory | 71 factories |
| **3** | Subsidiary (Tenant) | Head Office | Subsidiary Operations | KETEPA Limited | 9 subsidiaries |

---

## 3. WHAT IS MULTI-TENANCY?

### 3.1 The Apartment Building Analogy

Think of the KTDA ICT Reporting System like a large apartment building:

```
┌─────────────────────────────────────────────────────────────────┐
│                  KTDA ICT REPORTING SYSTEM                      │
│                    (The Apartment Building)                     │
│                                                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │ Apartment 1 │  │ Apartment 2 │  │ Apartment 3 │            │
│  │ (Kangaita)  │  │ (Ragati)    │  │ (Gathuthi)  │   ...      │
│  │             │  │             │  │             │            │
│  │ • Own data  │  │ • Own data  │  │ • Own data  │            │
│  │ • Own users │  │ • Own users │  │ • Own users │            │
│  │ • Own locks │  │ • Own locks │  │ • Own locks │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
│                                                                 │
│  80 Apartments Total (71 factories + 9 subsidiaries)           │
│                                                                 │
│  Shared Infrastructure:                                         │
│  • Database (foundation)                                        │
│  • Web server (plumbing & electricity)                          │
│  • Authentication (security system)                             │
│  • Reporting engine (shared amenities)                          │
└─────────────────────────────────────────────────────────────────┘
```

**Key Points:**
- **One Building, Many Apartments** - All 80 tenants use the same system (building), but each has its own private space (apartment)
- **Shared Infrastructure** - Everyone benefits from shared facilities (database, web server, security)
- **Private Data** - Kangaita Factory (Apartment 1) cannot see Ragati Factory's data (Apartment 2)
- **Same Features** - All apartments have the same rooms (features), but different furniture (data)
- **Lower Cost** - Sharing infrastructure is cheaper than building 80 separate systems

### 3.2 Multi-Tenancy in the KTDA Context

**What It Means:**
- All 71 factories and 9 subsidiaries use **one single system**
- Each tenant sees **only its own data**
- Data is **logically separated** but **physically stored together** in one database
- Each tenant has its **own users, reports, hardware inventory, and tickets**
- The system **looks and works the same** for everyone, but shows different data

**What It Does NOT Mean:**
- Tenants do NOT share data with each other
- Kangaita Factory CANNOT see Ragati Factory's reports
- Field Systems Administrators at one factory CANNOT log in to another factory's account
- There is NOT a separate system installation for each factory

### 3.3 Why Multi-Tenancy?

#### Traditional Approach (Separate Systems)
```
Kangaita System        Ragati System          Gathuthi System    ... (80 systems)
┌────────────────┐    ┌────────────────┐     ┌────────────────┐
│ Database       │    │ Database       │     │ Database       │
│ Web Server     │    │ Web Server     │     │ Web Server     │
│ Users          │    │ Users          │     │ Users          │
└────────────────┘    └────────────────┘     └────────────────┘

Cost: KES 500,000 × 80 tenants = KES 40,000,000
Maintenance: 80 separate systems to update
Support: 80 separate databases to backup
```

#### Multi-Tenant Approach (One Shared System)
```
                    KTDA ICT Reporting System
┌──────────────────────────────────────────────────────────┐
│ Database (with TenantId to separate data)                │
│ Web Server (one installation)                            │
│ Users (all tenants, filtered by TenantId)                │
│                                                           │
│ Tenants: Kangaita | Ragati | Gathuthi | ... (80 total)  │
└──────────────────────────────────────────────────────────┘

Cost: KES 1,500,000 (one-time) + KES 300,000/year hosting
Maintenance: 1 system to update (benefits all tenants)
Support: 1 database to backup
```

**Savings:** KES 38,500,000+ (96% cost reduction)

### 3.4 How Data Separation Works

Every piece of data in the system has a **TenantId** that identifies which factory it belongs to:

```
ChecklistSubmissions Table (Simplified View)

┌──────────────┬──────────┬──────────────┬────────────────────┐
│ SubmissionId │ TenantId │ TemplateName │ SubmittedDate      │
├──────────────┼──────────┼──────────────┼────────────────────┤
│ 1001         │ 38       │ Monthly ICT  │ 2025-10-15         │
│              │ (Kangaita)│ Report       │                    │
├──────────────┼──────────┼──────────────┼────────────────────┤
│ 1002         │ 34       │ Monthly ICT  │ 2025-10-16         │
│              │ (Ragati) │ Report       │                    │
├──────────────┼──────────┼──────────────┼────────────────────┤
│ 1003         │ 38       │ Quarterly    │ 2025-10-17         │
│              │ (Kangaita)│ Review       │                    │
└──────────────┴──────────┴──────────────┴────────────────────┘

When a Kangaita Field Systems Administrator logs in:
System automatically filters: WHERE TenantId = 38
Administrator sees: Submissions 1001, 1003 only

When a Ragati Field Systems Administrator logs in:
System automatically filters: WHERE TenantId = 34
Administrator sees: Submission 1002 only
```

**Automatic Filtering:**
- Users NEVER manually select their factory
- The system knows which factory you belong to (from your user profile)
- Every database query automatically adds: `WHERE TenantId = [Your Factory]`
- This filtering is enforced at the application level (cannot be bypassed)

---

## 4. THE THREE-TIER STRUCTURE

### 4.1 Visual Representation

```
┌─────────────────────────────────────────────────────────────────┐
│                        TIER 1: HEAD OFFICE                      │
│                                                                 │
│  Access Level: NATIONAL (All 7 Regions, All 80 Tenants)        │
│  Users: 30+ staff (ICT-O, ICT-S, ICT-I, ICT-B departments)     │
│  Can See: Everything (all 71 factories + 9 subsidiaries)       │
│  Can Do: Approve budgets, generate national reports, manage     │
│          system, create users, override regional decisions      │
│                                                                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ Manages & Oversees
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      TIER 2: REGIONAL OFFICES                   │
│                                                                 │
│  Access Level: REGIONAL (4-16 factories in their region)       │
│  Number of Regions: 7                                           │
│  Users per Region: 1 Regional ICT Manager + Field Admins       │
│  Can See: All factories in their assigned region               │
│  Can Do: Approve factory reports, monitor performance,          │
│          generate regional reports, support factories           │
│                                                                 │
│  Region 1: 12 factories | Region 2: 9 factories                │
│  Region 3: 8 factories  | Region 4: 8 factories                │
│  Region 5: 16 factories | Region 6: 14 factories               │
│  Region 7: 4 factories                                          │
│                                                                 │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             │ Manages & Supports
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                     TIER 3: FACTORY LEVEL                       │
│                                                                 │
│  Access Level: FACTORY-SPECIFIC (Own factory only)             │
│  Number of Factories: 71 tea factories                          │
│  Users per Factory: 1 Field Systems Administrator              │
│  Can See: Only their own factory's data                         │
│  Can Do: Submit reports, manage inventory, log tickets,         │
│          view own performance                                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Tier Comparison Table

| Aspect | Tier 1 (Head Office) | Tier 2 (Region) | Tier 3 (Factory) |
|--------|---------------------|-----------------|------------------|
| **Scope** | National (All) | Regional (4-16 factories) | Single Factory |
| **Users** | 30+ | 1 RICTM + Field Admins | 1 Field Admin |
| **Data Access** | All 80 tenants | Factories in region | Own factory only |
| **Can Approve** | Everything | Regional submissions | Nothing (submitter only) |
| **Can Override** | Regional decisions | Factory submissions | N/A |
| **Reports** | National dashboard | Regional comparison | Factory-specific |
| **User Management** | Create all users | Request user creation | Request user creation |

### 4.3 Data Flow Across Tiers

```
MONTHLY REPORTING FLOW:

Factory Level (Tier 3)
│
│  1. Field Systems Administrator at Kangaita Factory completes monthly checklist
│  2. System auto-fills 74% of fields from inventory
│  3. Administrator fills remaining 26% manually
│  4. Administrator submits report
│
▼
Regional Level (Tier 2)
│
│  5. Regional ICT Manager (Eric Kinyeki, Region 3) receives notification
│  6. Manager reviews submission (compared with previous month)
│  7. Manager approves or rejects with comments
│  8. If rejected: Factory corrects and resubmits (back to step 4)
│
▼
Head Office Level (Tier 1)
│
│  9. Head Office receives approved report
│  10. Data included in national dashboard
│  11. Trends analyzed across all regions
│  12. Executive reports generated for management
│
▼
Decision Making
│
│  13. Budget allocation based on reports
│  14. Hardware procurement planning
│  15. Regional performance evaluation
│  16. Strategic ICT planning
```

---

## 5. ROLES & PERMISSIONS

### 5.1 Role Hierarchy

```
┌─────────────────────────────────────────────────────────────────┐
│                    ROLE HIERARCHY                               │
│                                                                 │
│                  System Administrator (Highest)                 │
│                           │                                     │
│                           ├─── Can do everything                │
│                           │    (system configuration, user      │
│                           │     management, override controls)  │
│                           ▼                                     │
│              Head Office ICT Manager                            │
│                           │                                     │
│                           ├─── National access                  │
│                           │    (all factories, all reports,     │
│                           │     budget approval)                │
│                           ▼                                     │
│             Regional ICT Manager (RICTM)                        │
│                           │                                     │
│                           ├─── Regional access                  │
│                           │    (region factories, approval      │
│                           │     authority, regional reports)    │
│                           ▼                                     │
│            Field Systems Administrator                          │
│                           │                                     │
│                           └─── Factory-only access              │
│                                (own factory, submit reports,    │
│                                 manage inventory)               │
└─────────────────────────────────────────────────────────────────┘
```

### 5.2 Detailed Role Permissions

#### 5.2.1 System Administrator

**Who:** ICT staff at Head Office (typically ICT-I department - Infrastructure team)

**Access Level:** System-wide (all factories, all data, all settings)

**Permissions:**

| Category | Actions Allowed |
|----------|----------------|
| **Users** | Create, edit, delete all users across all factories |
| **Tenants** | Create new factories/subsidiaries, edit factory details |
| **Reports** | View all reports from all factories, generate system-wide analytics |
| **Configuration** | Change system settings, manage templates, configure workflows |
| **Data** | Export all data, run database maintenance, access audit logs |
| **Security** | Reset passwords, unlock accounts, view login history |
| **Overrides** | Can override any restriction or approval (emergency use only) |

**Key Responsibilities:**
- Add new factories when KTDA expands
- Create user accounts for new employees
- Reset forgotten passwords
- Configure checklist templates
- Monitor system health and performance
- Handle technical support escalations

**Example Actions:**
- Create a new factory: "Kionyo Tea Factory" in Region 4
- Add a new Regional ICT Manager: "Jackson Gachuki" for Region 4
- Reset password for: "Elizabeth Ndegwa" at Kangaita Factory
- Configure system backup schedule
- Generate audit report of all logins in the last month

---

#### 5.2.2 Head Office ICT Manager

**Who:** Senior ICT management at Nairobi HQ (ICT-B Head, ICT-S Head, ICT-I Head)

**Access Level:** National (all factories, all regions, read/write access)

**Permissions:**

| Category | Actions Allowed |
|----------|----------------|
| **Users** | View all users, request user creation (through System Admins) |
| **Reports** | View all reports, approve/reject any report, generate national reports |
| **Dashboard** | National dashboard with all 71 factories, regional comparisons |
| **Budget** | Approve factory hardware/software requests, allocate regional budgets |
| **Approvals** | Can override regional manager decisions |
| **Templates** | Create and publish new checklist templates for all factories |
| **Data** | Export reports to Excel/PDF, email reports to stakeholders |

**Key Responsibilities:**
- Monitor national ICT performance
- Approve large budget requests (>KES 500,000)
- Generate executive reports for senior management
- Ensure compliance with ICT policies
- Coordinate regional managers
- Make strategic ICT decisions

**Example Actions:**
- View dashboard showing all 71 factories' operational status
- Approve Kangaita Factory's request for KES 800,000 hardware budget
- Generate quarterly report comparing all 7 regions
- Publish new "Annual ICT Audit" checklist template
- Override a regional rejection if it was unjustified

---

#### 5.2.3 Regional ICT Manager (RICTM)

**Who:** ICT managers at regional offices (7 people, one per region)

**Current Regional ICT Managers:**
- **Region 1:** Peter Kibe (12 factories: Gacharage, Gachege, Ikumbi, etc.)
- **Region 2:** Benjamin K. Ndungu (9 factories: Chinga, Gathuthi, Ragati, etc.)
- **Region 3:** Eric Kinyeki (8 factories: Kangaita, Ndima, Mununga, etc.)
- **Region 4:** Jackson Gachuki (8 factories: Githongo, Weru, Imenti, etc.)
- **Region 5:** Enock O. Ogara (16 factories: Kapkatet, Litein, Toror, etc.)
- **Region 6:** Richard S. Olume (14 factories: Ogembo, Kiamokama, Nyamache, etc.)
- **Region 7:** Eric Ngetich (4 factories: Mudete, Kaptumo, Kapsara, Chebut)

**Access Level:** Regional (4-16 factories in assigned region only)

**Permissions:**

| Category | Actions Allowed |
|----------|----------------|
| **Users** | View users in their region, request new user creation |
| **Reports** | View reports from factories in their region, approve/reject submissions |
| **Dashboard** | Regional dashboard with factories in their region |
| **Budget** | Approve small factory requests (<KES 500,000), recommend larger requests to HQ |
| **Approvals** | Primary approval authority for factory monthly/quarterly reports |
| **Support** | Provide technical guidance to factories, escalate complex issues to HQ |
| **Data** | Export regional reports, compare factory performance |

**Cannot Do:**
- See data from other regions (e.g., Eric Kinyeki in Region 3 cannot see Region 5 data)
- Approve own factory's reports (if they have a home factory)
- Delete or modify system-wide settings
- Create or delete factories

**Key Responsibilities:**
- Review and approve monthly reports from factories in their region
- Monitor regional ICT performance trends
- Support Field Systems Administrators with technical issues
- Coordinate regional ICT initiatives
- Escalate critical issues to Head Office

**Example Actions (Eric Kinyeki, Region 3 ICT Manager):**
- View dashboard with 8 Region 3 factories
- Approve monthly report from Elizabeth Ndegwa at Kangaita Factory
- Reject report from Ndima Factory (data inconsistency), provide correction comments
- Compare performance of Kangaita vs Mununga factories
- Recommend Rukuriri Factory for hardware upgrade (to Head Office)

**Cannot Do:**
- View Region 5 factories (Enock O. Ogara's region)
- Approve Head Office requests
- Create new checklist templates (only Head Office)

---

#### 5.2.4 Field Systems Administrator

**Who:** Primary ICT staff at each factory (71 people, one per factory)

**Examples of Current Field Systems Administrators:**
- **Kangaita Factory (Region 3):** Elizabeth Ndegwa
- **Ragati Factory (Region 2):** Daniel M. Muigai
- **Ndima Factory (Region 3):** Gideon Rotich
- **Gathuthi Factory (Region 2):** Vincent Koech
- **Kapkatet Factory (Region 5):** Vincent Onsongo
- **Ogembo Factory (Region 6):** John Sang
- **Mudete Factory (Region 7):** Anthony Getugi

**Access Level:** Factory-specific (own factory only, read/write access)

**Permissions:**

| Category | Actions Allowed |
|----------|----------------|
| **Reports** | Create, edit, submit monthly/quarterly checklists for own factory |
| **Inventory** | Manage hardware and software inventory for own factory |
| **Tickets** | Log support tickets, track ticket resolution |
| **Dashboard** | View own factory's dashboard and historical trends |
| **Approvals** | Cannot approve (submitter role only) |
| **Data** | Export own factory's reports to Excel/PDF |
| **Users** | View users at their factory, request new user accounts |

**Cannot Do:**
- See other factories' data (even in same region)
- Approve own submissions (must be approved by Regional ICT Manager)
- Create checklist templates
- Modify system settings
- Create user accounts

**Key Responsibilities:**
- Complete and submit monthly ICT status reports
- Maintain accurate hardware/software inventory
- Log and track support tickets
- Respond to regional manager's questions/comments
- Ensure timely submission of all required reports

**Example Actions (Elizabeth Ndegwa, Kangaita Factory):**
- Open new monthly checklist (system auto-fills 74% from inventory)
- Fill remaining fields manually (downtime reasons, ticket summaries)
- Submit checklist to Regional ICT Manager (Eric Kinyeki) for approval
- Update inventory when new computers arrive
- Log ticket: "Printer not working in Accounts department"
- View own factory's 6-month trend (operational percentage declining)
- Export October report as PDF for factory manager review

**Cannot Do:**
- View Ragati Factory's reports (different factory, even though same region)
- Approve own submissions
- Change checklist questions

---

### 5.3 Permission Matrix

| Action | System Admin | HO ICT Manager | Regional ICT Manager | Field Systems Admin |
|--------|--------------|----------------|----------------------|---------------------|
| **View own factory data** | ✅ All | ✅ All | ✅ Region | ✅ Own |
| **View other factories** | ✅ | ✅ | ✅ Region only | ❌ |
| **Submit checklist** | ✅ | ✅ | ✅ | ✅ |
| **Approve checklist** | ✅ | ✅ | ✅ Region only | ❌ |
| **Create users** | ✅ | ❌ (request) | ❌ (request) | ❌ (request) |
| **Manage inventory** | ✅ | ✅ View | ✅ View | ✅ Own |
| **Create templates** | ✅ | ✅ | ❌ | ❌ |
| **Export reports** | ✅ | ✅ | ✅ Region | ✅ Own |
| **System configuration** | ✅ | ❌ | ❌ | ❌ |
| **Override approvals** | ✅ | ✅ | ❌ | ❌ |

**Legend:**
- ✅ = Allowed
- ❌ = Not allowed
- Region = Limited to their assigned region
- Own = Limited to their own factory

---

## 6. DATA ISOLATION & SECURITY

### 6.1 How Data is Kept Separate

Every table in the database has a **TenantId** column that identifies which factory the data belongs to:

```
DATABASE STRUCTURE (Simplified)

Tenants Table (Master List of Factories)
┌──────────┬──────────────────────┬──────────┬──────────────┐
│ TenantId │ TenantName           │ RegionId │ TenantType   │
├──────────┼──────────────────────┼──────────┼──────────────┤
│ 1        │ Head Office          │ NULL     │ HeadOffice   │
│ 11       │ Gacharage Factory    │ 1        │ Factory      │
│ 12       │ Gachege Factory      │ 1        │ Factory      │
│ 26       │ Chinga Factory       │ 2        │ Factory      │
│ 34       │ Ragati Factory       │ 2        │ Factory      │
│ 38       │ Kangaita Factory     │ 3        │ Factory      │
│ 43       │ Ndima Factory        │ 3        │ Factory      │
│ ...      │ ...                  │ ...      │ ...          │
│ 81       │ KETEPA Limited       │ NULL     │ Subsidiary   │
└──────────┴──────────────────────┴──────────┴──────────────┘

TenantHardware Table (Factory Equipment)
┌────────────┬──────────┬──────────────┬────────────┬────────┐
│ HardwareId │ TenantId │ ItemName     │ SerialNo   │ Status │
├────────────┼──────────┼──────────────┼────────────┼────────┤
│ 1001       │ 38       │ Desktop PC   │ DL-345     │ Oper.  │
│ 1002       │ 38       │ Laptop       │ LP-123     │ Oper.  │
│ 1003       │ 34       │ Desktop PC   │ DL-456     │ Faulty │
│ 1004       │ 34       │ Server       │ SV-001     │ Oper.  │
└────────────┴──────────┴──────────────┴────────────┴────────┘

Users Table (System Users)
┌────────┬───────────────────┬──────────┬────────────┬──────────────────┐
│ UserId │ FullName          │ TenantId │ RoleId     │ Email            │
├────────┼───────────────────┼──────────┼────────────┼──────────────────┤
│ 301    │ Elizabeth Ndegwa  │ 38       │ Field_Adm  │ elizabeth@ktda...│
│ 302    │ Daniel M. Muigai  │ 34       │ Field_Adm  │ daniel@ktda...   │
│ 203    │ Eric Kinyeki      │ NULL     │ RICTM_R3   │ eric.k@ktda...   │
└────────┴───────────────────┴──────────┴────────────┴──────────────────┘
```

### 6.2 Automatic Data Filtering

When a user logs in, the system automatically:

1. **Identifies the user's TenantId and Role**
   - Elizabeth Ndegwa (UserId 301) → TenantId 38 (Kangaita), Role: Field Systems Administrator

2. **Applies filters to ALL database queries**
   - For Field Systems Administrators: `WHERE TenantId = 38`
   - For Regional ICT Managers: `WHERE TenantId IN (38, 39, 40, 41, 42, 43, 44, 45)` (factories in Region 3)
   - For Head Office: No filter (see all)

3. **Enforces filters at the application layer**
   - Cannot be bypassed by users
   - Even direct URL access is filtered

**Example:**

```
User: Elizabeth Ndegwa (Kangaita Factory, TenantId 38)
Action: View Hardware Inventory

SQL Query Generated by System:
SELECT * FROM TenantHardware
WHERE TenantId = 38
  AND Status != 'Retired';

Results Elizabeth Sees:
┌────────────┬──────────────┬────────────┬────────┐
│ HardwareId │ ItemName     │ SerialNo   │ Status │
├────────────┼──────────────┼────────────┼────────┤
│ 1001       │ Desktop PC   │ DL-345     │ Oper.  │
│ 1002       │ Laptop       │ LP-123     │ Oper.  │
└────────────┴──────────────┴────────────┴────────┘

Elizabeth CANNOT see HardwareId 1003 or 1004 (belong to TenantId 34 - Ragati)
```

### 6.3 Security Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                      SECURITY LAYERS                            │
└─────────────────────────────────────────────────────────────────┘

Layer 1: AUTHENTICATION (Who are you?)
│
├─ Username and password required
├─ Passwords encrypted with bcrypt (industry standard)
├─ Failed login attempts tracked (lockout after 5 attempts)
├─ Session timeout after 30 minutes of inactivity
│
▼

Layer 2: AUTHORIZATION (What can you do?)
│
├─ Role-based permissions checked on every action
├─ Cannot access pages/features not allowed for your role
├─ API endpoints validate role before processing requests
│
▼

Layer 3: DATA FILTERING (What can you see?)
│
├─ TenantId filter applied automatically to all queries
├─ Users cannot see other factories' data
├─ Regional managers see only their region
├─ Enforced at database query level (cannot bypass)
│
▼

Layer 4: AUDIT LOGGING (Who did what, when?)
│
├─ All create/update/delete actions logged
├─ Includes: User, Action, TenantId, Timestamp, IP Address
├─ Audit trail cannot be deleted (immutable log)
├─ Available to System Administrators for investigation
│
▼

Layer 5: DATA ENCRYPTION (Protect data at rest and in transit)
│
├─ HTTPS encryption for all web traffic (SSL/TLS)
├─ Database connections encrypted
├─ Sensitive fields encrypted (passwords, API keys)
├─ Regular backups encrypted
```

### 6.4 Data Privacy Compliance

The system ensures compliance with data privacy regulations:

| Requirement | Implementation |
|-------------|----------------|
| **Data Minimization** | Only collect necessary data (no personal data beyond name, email, role) |
| **Access Control** | Users can only access data relevant to their role and factory |
| **Audit Trail** | All data access logged with timestamp and user ID |
| **Data Retention** | Old submissions archived after 5 years (configurable) |
| **Right to Access** | Users can export their own factory's data anytime |
| **Data Portability** | Export to Excel/PDF for offline use |
| **Secure Deletion** | When a user is deactivated, their data is retained but account is disabled |

---

## 7. USER MANAGEMENT

### 7.1 User Lifecycle

```
┌─────────────────────────────────────────────────────────────────┐
│                      USER LIFECYCLE                             │
└─────────────────────────────────────────────────────────────────┘

1. USER CREATION
│
├─ System Administrator creates user account
├─ Assigns: Full Name, Email, Role, TenantId (factory)
├─ Generates temporary password
├─ Sends welcome email with login instructions
│
▼

2. FIRST LOGIN
│
├─ User logs in with temporary password
├─ System forces password change (security requirement)
├─ User creates strong password (8+ chars, uppercase, lowercase, number)
├─ System creates user session (valid for 30 minutes)
│
▼

3. ACTIVE USE
│
├─ User performs daily tasks (submit reports, manage inventory, etc.)
├─ All actions logged in audit trail
├─ Session renewed with each action (up to 8 hours max)
│
▼

4. PASSWORD RESET (if forgotten)
│
├─ User clicks "Forgot Password" on login page
├─ System sends reset link to registered email
├─ User clicks link, sets new password
├─ Old password immediately invalidated
│
▼

5. ACCOUNT DEACTIVATION (employee leaves)
│
├─ System Administrator marks account as "Inactive"
├─ User can no longer log in
├─ Data created by user is retained (for audit trail)
├─ User's name shows as "[Deactivated]" in old records
│
▼

6. ACCOUNT REACTIVATION (employee returns)
│
├─ System Administrator marks account as "Active"
├─ User can log in again
├─ All previous data accessible
```

### 7.2 User Creation Process

**Step-by-Step Example:**

**Scenario:** Kangaita Factory hires a new Field Systems Administrator, Elizabeth Ndegwa

**Step 1: Factory Manager sends request to System Administrator**
- Email: "Please create user account for Elizabeth Ndegwa, new Field Systems Administrator at Kangaita Factory. Email: elizabeth.ndegwa@ktda.co.ke"

**Step 2: System Administrator creates user**
- Logs into system as Administrator
- Navigates to: Users → Create New User
- Fills form:
  - Full Name: Elizabeth Ndegwa
  - Email: elizabeth.ndegwa@ktda.co.ke
  - Role: Field Systems Administrator
  - Factory (Tenant): Kangaita Factory (Region 3)
  - Regional ICT Manager: Eric Kinyeki (RICTM_R3)
  - Status: Active
- Clicks "Create User"

**Step 3: System generates credentials**
- Username: elizabeth.ndegwa@ktda.co.ke (email address)
- Temporary Password: Auto-generated (e.g., "Ktda2025!Temp456")
- System sends welcome email to elizabeth.ndegwa@ktda.co.ke

**Step 4: Elizabeth receives welcome email**
```
Subject: Welcome to KTDA ICT Reporting System

Dear Elizabeth Ndegwa,

Your user account has been created for the KTDA ICT Reporting System.

Username: elizabeth.ndegwa@ktda.co.ke
Temporary Password: Ktda2025!Temp456
Factory: Kangaita Tea Factory (Region 3)
Role: Field Systems Administrator
Regional ICT Manager: Eric Kinyeki

Please log in at: https://reporting.ktda.co.ke

You will be required to change your password on first login.

For support, contact: ictsupport@ktda.co.ke

Best regards,
KTDA ICT Department
```

**Step 5: Elizabeth logs in for first time**
- Goes to https://reporting.ktda.co.ke
- Enters username and temporary password
- System prompts: "You must change your password"
- Elizabeth creates new password: "MySecure@Pass2025"
- System validates (8+ chars, uppercase, lowercase, number) ✓
- Elizabeth logs in successfully

**Step 6: System applies automatic filters**
- Elizabeth's TenantId: 38 (Kangaita Factory)
- Elizabeth's Role: Field Systems Administrator
- All database queries filtered: `WHERE TenantId = 38`
- Elizabeth sees only Kangaita Factory data

### 7.3 User Profile Management

**Users can update their own:**
- Profile photo
- Contact phone number
- Notification preferences (email notifications on/off)

**Users CANNOT change:**
- Their role (only System Administrator can)
- Their assigned factory (only System Administrator can)
- Their email address (username) - requires Admin approval

**System Administrator can update:**
- User role (promote/demote)
- Assigned factory (if user transfers)
- Account status (active/inactive)
- Password reset (if user forgot)

---

## 8. ACCESS CONTROL EXAMPLES

### 8.1 Example 1: Field Admin Tries to View Another Factory

**Scenario:** Elizabeth Ndegwa (Kangaita Factory) tries to access Ragati Factory's data

**What Happens:**

```
Elizabeth logs in → System sets TenantId = 38 (Kangaita)

Elizabeth tries to access: https://reporting.ktda.co.ke/Hardware/View/1003

System checks: Does HardwareId 1003 belong to TenantId 38?
Database: SELECT * FROM TenantHardware WHERE HardwareId = 1003 AND TenantId = 38;
Result: 0 rows (HardwareId 1003 belongs to TenantId 34 - Ragati)

System Response: 404 Not Found (or "Access Denied")
```

**Elizabeth sees:** "The hardware item you are trying to access does not exist or you do not have permission to view it."

**Why this is secure:**
- Even if Elizabeth guesses the correct URL, she cannot see the data
- System enforces TenantId filter at the database level
- No way to bypass (even with developer tools or URL manipulation)

---

### 8.2 Example 2: Regional ICT Manager Approves Submission

**Scenario:** Eric Kinyeki (Region 3 ICT Manager) approves Kangaita Factory's monthly report

**What Happens:**

```
Eric logs in → System sets Role = Regional ICT Manager, RegionId = 3

Eric navigates to: Approvals → Pending Approvals

System query:
SELECT * FROM ChecklistSubmissions cs
JOIN Tenants t ON cs.TenantId = t.TenantId
WHERE t.RegionId = 3  -- Eric's region
  AND cs.Status = 'Submitted';

Results: Shows pending submissions from Region 3 factories
- Kangaita Factory (TenantId 38) - submitted by Elizabeth Ndegwa
- Ndima Factory (TenantId 43) - submitted by Gideon Rotich
- Mununga Factory (TenantId 41) - submitted by Zipporah Karimi

Eric clicks "Approve" on Kangaita Factory submission

System checks:
1. Is Eric's role authorized to approve? ✓ (Regional ICT Manager can approve)
2. Is Kangaita in Eric's region? ✓ (TenantId 38 → RegionId 3 → Eric's region)
3. Is submission status "Submitted"? ✓ (not already approved)

System updates:
UPDATE ChecklistSubmissions
SET Status = 'Approved',
    ApprovedById = 203,  -- Eric's UserId
    ApprovedDate = GETDATE(),
    ApproverComments = 'Good work. All data looks accurate.'
WHERE SubmissionId = 1001
  AND TenantId = 38;

System logs audit trail:
INSERT INTO AuditLog (UserId, Action, TenantId, Details, Timestamp)
VALUES (203, 'Approved Submission', 38, 'SubmissionId: 1001', GETDATE());

System sends notification:
- Email to Elizabeth Ndegwa (submitter)
- SignalR push notification to Elizabeth's browser
```

**Result:** Submission approved, Elizabeth notified, audit trail logged

---

### 8.3 Example 3: Head Office Overrides Regional Decision

**Scenario:** Head Office ICT Manager overrides a regional rejection

**Background:**
- Kangaita Factory submitted monthly report
- Eric Kinyeki (Regional ICT Manager) rejected it with comment: "Hardware count seems too low"
- Factory Manager appeals to Head Office: "Count is accurate, old equipment was retired"

**What Happens:**

```
Head Office Manager logs in → System sets Role = Head Office ICT Manager (national access)

Manager navigates to: Approvals → All Submissions → Filter: "Rejected"

System query:
SELECT * FROM ChecklistSubmissions
WHERE Status = 'Rejected'
ORDER BY RejectedDate DESC;

Results: Shows all rejected submissions from all 71 factories

Manager clicks on Kangaita Factory rejected submission

Manager reviews:
- Original submission data
- Regional Manager's rejection comment: "Hardware count seems too low"
- Factory Manager's appeal: "Count is accurate, old equipment was retired"
- Checks inventory history: Confirms 5 computers retired last month

Manager decides to override rejection

Manager clicks "Override and Approve"

System confirms: "You are overriding a regional manager's decision. Provide justification."

Manager enters comment:
"Rejection overridden. Count verified against inventory. 5 computers were retired last month per approved retirement form #RT-2025-045. Approval granted."

System updates:
UPDATE ChecklistSubmissions
SET Status = 'Approved',
    ApprovedById = 101,  -- Head Office Manager's UserId
    ApprovedDate = GETDATE(),
    ApproverComments = 'Rejection overridden...',
    OverriddenRejection = 1,
    OriginalRejecterId = 203  -- Eric's UserId
WHERE SubmissionId = 1001;

System sends notifications:
- Email to Factory Manager: "Your appeal was approved by Head Office"
- Email to Eric Kinyeki: "Your rejection was overridden by Head Office"
- Email to Elizabeth Ndegwa: "Your submission has been approved"
```

**Result:** Rejection overridden, all parties notified, audit trail shows override

---

## 9. REAL-WORLD SCENARIOS

### 9.1 Scenario 1: New Factory Onboarding

**Context:** KTDA adds a new tea factory - already exists in Region 4

**Steps:**

1. **System Administrator creates new tenant**
   - Navigates to: Tenants → Create New Tenant
   - TenantId assigned automatically (e.g., 72)

2. **Administrator creates user account**
   - Field Systems Administrator for new factory
   - Regional ICT Manager: Jackson Gachuki (RICTM_R4)

3. **New Field Admin starts using system**
   - Adds hardware inventory
   - System automatically sets TenantId = 72

4. **Regional ICT Manager can now see new factory**
   - Jackson Gachuki's dashboard shows 9 factories (was 8)

5. **Head Office sees new factory**
   - National dashboard shows 72 factories (was 71)

**Result:** New factory integrated in 30 minutes

---

### 9.2 Scenario 2: User Transfers Between Factories

**Context:** Daniel M. Muigai transfers from Ragati Factory (Region 2) to Gathuthi Factory (Region 2)

**Steps:**

1. **HR notifies System Administrator**
   - Transfer effective Nov 1, 2025

2. **Administrator updates account**
   - OLD: Ragati Factory (TenantId 34)
   - NEW: Gathuthi Factory (TenantId 27)
   - Regional ICT Manager: Benjamin K. Ndungu (no change - same region)

3. **System updates access**
   - Daniel now sees TenantId = 27 (Gathuthi)
   - Cannot see Ragati data anymore

4. **Historical data preserved**
   - Old submissions at Ragati still show "Created by: Daniel M. Muigai"

**Result:** Transfer completed instantly, audit trail intact

---

### 9.3 Scenario 3: Regional Manager Reviews Performance

**Context:** Benjamin K. Ndungu (Region 2 Manager) compares all 9 factories

**Steps:**

1. **Benjamin logs in**
   - System sets RegionId = 2

2. **Navigates to Factory Comparison**
   - Sees 9 Region 2 factories only

3. **Reviews metrics:**
   ```
   Factory          | Total | Oper | Faulty | OP %
   -----------------|-------|------|--------|------
   Chinga           | 50    | 48   | 2      | 96%
   Gathuthi         | 45    | 43   | 2      | 96%
   Ragati           | 50    | 46   | 4      | 92%
   Gatunguru        | 40    | 36   | 4      | 90%
   Gitugi           | 42    | 37   | 5      | 88%
   ```

4. **Identifies Gitugi as underperformer** (88%)

5. **Exports report and emails factory**

**Result:** Performance gaps identified, corrective action initiated

---

### 9.4 Scenario 4: Head Office Budget Planning

**Context:** Head Office prepares 2026 budget

**Steps:**

1. **Head Office Manager reviews national dashboard**
   - All 71 factories
   - Overall: 88% operational

2. **Drills down by region:**
   ```
   Region 1: 91% (12 factories)
   Region 2: 90% (9 factories)
   Region 3: 89% (8 factories)
   Region 4: 87% (8 factories)
   Region 5: 86% (16 factories)
   Region 6: 85% (14 factories)
   Region 7: 92% (4 factories)
   ```

3. **Identifies Region 6 as priority** (85%, 14 factories)

4. **Calculates budget:**
   - Region 6: KES 3,500,000
   - Region 5: KES 2,800,000
   - Total: KES 6,300,000

5. **Prepares presentation with data evidence**

6. **Management approves budget**

**Result:** Data-driven decision making

---

## 10. BENEFITS OF MULTI-TENANCY

### 10.1 Cost Savings

**Traditional Approach (80 Separate Systems):**
```
Per Tenant Cost: KES 500,000
Total (80 tenants): KES 40,000,000 initial
Annual maintenance: KES 8,000,000/year
5-Year Cost: KES 72,000,000
```

**Multi-Tenant Approach:**
```
Initial: KES 4,000,000
Annual: KES 1,400,000/year
5-Year Cost: KES 11,000,000

SAVINGS: KES 61,000,000 (85% reduction)
```




---

## 12. FREQUENTLY ASKED QUESTIONS

**Q: Can Kangaita Factory see Ragati Factory's data?**
A: No. Each factory sees only its own data via automatic TenantId filtering.

**Q: How many factories are in the system?**
A: 71 tea factories across 7 regions, plus 9 subsidiaries (80 total tenants).

**Q: Who are the Regional ICT Managers?**
A:
- Region 1: Peter Kibe (12 factories)
- Region 2: Benjamin K. Ndungu (9 factories)
- Region 3: Eric Kinyeki (8 factories)
- Region 4: Jackson Gachuki (8 factories)
- Region 5: Enock O. Ogara (16 factories)
- Region 6: Richard S. Olume (14 factories)
- Region 7: Eric Ngetich (4 factories)

**Q: Can Field Systems Administrators approve their own reports?**
A: No. Reports must be approved by their Regional ICT Manager.

**Q: What if the system goes down?**
A: 99.9% uptime SLA, automated backups, disaster recovery plan, redundant servers.

**Q: How much does multi-tenancy save?**
A: KES 61,000,000 over 5 years (85% cost reduction vs separate systems).

**Q: Which region has the most factories?**
A: Region 5 (Kericho & Bomet) with 16 factories.

**Q: Can users be transferred between factories?**
A: Yes. System Administrator updates TenantId. Historical data preserved.

---

## DOCUMENT END

**Summary:**

This document explained the KTDA ICT Reporting System's organizational structure and multi-tenancy architecture based on actual KTDA data:

- **71 tea factories** across **7 regions** + **9 subsidiaries** = **80 total tenants**
- **Three-tier hierarchy:** Head Office → 7 Regions → 71 Factories
- **4 ICT departments** at Head Office: ICT-O, ICT-S, ICT-I, ICT-B
- **7 Regional ICT Managers** with real names and factory assignments
- **Multi-tenancy:** 85% cost savings (KES 61M over 5 years)
- **Automatic TenantId filtering** ensures complete data isolation
- **Role-based access:** System Admin, HO Manager, RICTM, Field Admin

**Key Personnel Referenced:**
- Regional ICT Managers: Peter Kibe, Benjamin K. Ndungu, Eric Kinyeki, Jackson Gachuki, Enock O. Ogara, Richard S. Olume, Eric Ngetich
- Field Systems Administrators: Elizabeth Ndegwa (Kangaita), Daniel M. Muigai (Ragati), Gideon Rotich (Ndima), Vincent Koech (Gathuthi), and others

**Next Steps:**
1. Review document with stakeholders
2. Conduct user training on roles and permissions
3. Proceed to implementation documentation
4. Set up user accounts for all 71 factories

**Related Documents:**
- [5_Checklist_Templates_Submissions/0_ChecklistSystem_Overview.md](../5_Checklist_Templates_Submissions/0_ChecklistSystem_Overview.md)
- [KTDA_ICT_Reporting_System_Analysis.md](../KTDA_ICT_Reporting_System_Analysis.md)
- [KTDA_Enhanced_Database_Schema.sql](../KTDA_Enhanced_Database_Schema.sql)

**Contact:**
ICT Development Team Lead
Email: ictsupport@ktda.co.ke