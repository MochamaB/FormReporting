# Form Responses & Submissions Viewing - Implementation Plan

## Document Information
- **Created**: December 17, 2025
- **Status**: Planning
- **Priority**: High
- **Related Documents**: 
  - `deepdive.md` - Project architecture reference
  - `Section4_Workflows_Actions.md` - Form workflows

---

## 1. Executive Summary

### Purpose
Enable users to view form submissions and responses for templates they have access to, with scope-based filtering that respects the organizational hierarchy. Users access submissions from the AvailableForms page, view a list of responses in a table format, and drill down into individual responses via an offcanvas panel.

### Key Goals
1. Centralize submission access through the AvailableForms page
2. Implement scope-based access control for submissions
3. Provide intuitive UI for browsing and viewing responses
4. Support quick response preview without page navigation

---

## 2. User Flow

### Navigation Path
```
AvailableForms.cshtml (My Forms)
    │
    ├── [Forms Tab] → Template Cards Grid
    │       │
    │       └── [View Submissions] button on card
    │               │
    │               └── TemplateSubmissions.cshtml (NEW)
    │                       │
    │                       ├── [Responses Tab] - Table with inline previews
    │                       │       │
    │                       │       └── [Click Row] → Offcanvas Panel
    │                       │               │
    │                       │               └── Full response details
    │                       │
    │                       └── [Summary Tab] - Aggregated statistics
```

### User Stories

**US-1**: As a Regional Manager, I want to view all submissions from factories in my region so I can monitor compliance.

**US-2**: As a Factory Manager, I want to see only my factory's submissions so I can review my team's work.

**US-3**: As a user, I want to quickly preview responses without leaving the submissions list.

**US-4**: As an administrator, I want to export submissions data for reporting purposes.

---

## 3. Scope-Based Access Control

### Scope Levels and Data Access

| Scope Code | Access Level | Submissions Visible |
|------------|--------------|---------------------|
| **GLOBAL** | System-wide | All submissions for the template |
| **REGIONAL** | Region-based | Submissions from tenants in user's region |
| **TENANT** | Single tenant | Submissions from user's primary tenant only |
| **DEPARTMENT** | Department-based | Submissions from user's department's tenant |
| **TEAM** | Team-based | Submissions from user's tenant |
| **INDIVIDUAL** | Personal only | Only user's own submissions |

### Scope Indicator
Display a visual indicator showing what data the user is viewing:
- Badge/info bar at top of submissions list
- Examples:
  - "Viewing: All Submissions" (GLOBAL)
  - "Viewing: Central Region Submissions" (REGIONAL)
  - "Viewing: Kericho Factory Submissions" (TENANT)
  - "Viewing: Your Submissions Only" (INDIVIDUAL)

### Special Cases
- **Non-tenant forms** (e.g., appraisals, training feedback): Filter by submitter's tenant association
- **Anonymous submissions**: Visible based on template ownership/assignment

---

## 4. UI Design Specification

### 4.1 Template Submissions Page Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ TEMPLATE HEADER                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ [Category Icon] Template Name                                           │ │
│ │                                                                         │ │
│ │ Category: Operations  |  Version: 1.0  |  Published: Dec 15, 2025      │ │
│ │                                                                         │ │
│ │                                            [Actions ▼]  [View Form]    │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ TAB NAVIGATION                                                              │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ [Responses (45)]  [Summary]                                             │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ SCOPE INDICATOR                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ 🔒 Viewing: Central Region Submissions (45 total)                       │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ FILTERS BAR                                                                 │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ [Period ▼]  [Tenant ▼]  [Status ▼]  [🔍 Search...]       [Export ▼]    │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ RESPONSES TABLE                                                             │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ □  ☆  #   Date      Submitter         Tenant      Q1...   Q2...   ⋮    │ │
│ ├─────────────────────────────────────────────────────────────────────────┤ │
│ │ □  ☆  45  10:46am   gregory@mail.com  Kericho     Answer  ████    ⋮    │ │
│ │ □  ☆  44  08:33am   mckenzie@mail.com Nandi       Answer  ███     ⋮    │ │
│ │ □  ★  43  Nov 15    hermann@mail.com  Sotik       Answer  ██      ⋮    │ │
│ │ □  ☆  42  Nov 15    sven@mail.com     Kericho     Answer  █       ⋮    │ │
│ │ ...                                                                     │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ PAGINATION                                                                  │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Showing 1-10 of 45 submissions      [◀ Prev]  1  2  3  ...  [Next ▶]   │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Table Columns

| Column | Width | Description |
|--------|-------|-------------|
| Checkbox | 40px | Bulk selection for export/actions |
| Star | 30px | Flag/favorite submission for follow-up |
| # | 60px | Submission number (sequential per template) |
| Date | 100px | Submission date (relative for recent, absolute for older) |
| Submitter | 180px | User email or name |
| Tenant | 120px | Tenant/factory name (if applicable) |
| Status | 100px | Badge showing Draft/Submitted/Approved/Rejected |
| Q1, Q2, Q3 | Dynamic | First 3-5 form fields with truncated/visual answers |
| Progress | 80px | Visual bar showing answered/total fields |
| Actions | 60px | Dropdown menu |

### 4.3 Inline Response Previews

Different field types display differently in table columns:

| Field Type | Table Display |
|------------|---------------|
| Text/TextArea | Truncated text (max 30 chars) with ellipsis |
| Number | Formatted number with suffix if applicable |
| Rating | Progress bar with score (e.g., "7/10" with bar) |
| Dropdown | Selected option label |
| Checkbox | ✓ or ✗ icon |
| Date | Formatted date |
| File Upload | "Download" link or file count |
| MultiSelect | Comma-separated or "+N more" |

### 4.4 Response Detail Offcanvas

```
┌────────────────────────────────────────────┐
│ Response #43, Nov 15, 2025            [X]  │
├────────────────────────────────────────────┤
│ [Answers]  [Comments (3)]  [History]       │
├────────────────────────────────────────────┤
│                                            │
│ SUBMISSION INFO CARD                       │
│ ┌────────────────────────────────────────┐ │
│ │ Submitter:  John Doe                   │ │
│ │ Email:      john.doe@company.com       │ │
│ │ Tenant:     Kericho Factory            │ │
│ │ Period:     November 2025              │ │
│ │ Status:     ✓ Approved                 │ │
│ │ Submitted:  Nov 15, 2025 at 10:46 AM   │ │
│ │ Reviewed:   Nov 16, 2025 by Jane Smith │ │
│ └────────────────────────────────────────┘ │
│                                            │
│ SECTION: Personal Information              │
│ ┌────────────────────────────────────────┐ │
│ │ Q1. What is your email address?        │ │
│ │     gregory.muryn@gmail.com            │ │
│ ├────────────────────────────────────────┤ │
│ │ Q2. Rate our service please            │ │
│ │     ★★★★☆ (4 out of 5)                 │ │
│ ├────────────────────────────────────────┤ │
│ │ Q3. Please tell us why you chose       │ │
│ │     that rating                        │ │
│ │                                        │ │
│ │     I love working here. Great people, │ │
│ │     projects and of course our office. │ │
│ └────────────────────────────────────────┘ │
│                                            │
│ SECTION: Attachments                       │
│ ┌────────────────────────────────────────┐ │
│ │ Q5. Attach your full-size photos       │ │
│ │                                        │ │
│ │     ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐   │ │
│ │     │ img │ │ img │ │ img │ │ +4  │   │ │
│ │     └─────┘ └─────┘ └─────┘ └─────┘   │ │
│ └────────────────────────────────────────┘ │
│                                            │
│ FOOTER ACTIONS                             │
│ ┌────────────────────────────────────────┐ │
│ │ [🖨️ Print]  [📄 Export PDF]  [↗ Full] │ │
│ └────────────────────────────────────────┘ │
└────────────────────────────────────────────┘
```

### 4.5 Offcanvas Tabs

| Tab | Content |
|-----|---------|
| **Answers** | All responses grouped by section with proper formatting |
| **Comments** | Discussion thread for the submission (if enabled) |
| **History** | Audit trail of status changes, edits, approvals |

### 4.6 Summary Tab

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ SUMMARY TAB                                                                 │
│                                                                             │
│ STATS CARDS ROW                                                             │
│ ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐  │
│ │    45     │  │    38     │  │     5     │  │     2     │  │     0     │  │
│ │   Total   │  │ Approved  │  │  Pending  │  │ Rejected  │  │  Drafts   │  │
│ │           │  │   (84%)   │  │   (11%)   │  │   (5%)    │  │   (0%)    │  │
│ └───────────┘  └───────────┘  └───────────┘  └───────────┘  └───────────┘  │
│                                                                             │
│ COMPLETION METRICS                                                          │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Average Completion Rate:  87%  ████████████████████░░░░                 │ │
│ │ Average Time to Complete: 12 minutes                                    │ │
│ │ On-Time Submission Rate:  92%                                           │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ SUBMISSION TIMELINE (Phase 2)                                               │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ [Line chart showing submissions over time by week/month]                │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ FIELD ANALYTICS (Phase 2)                                                   │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ Q2. Rate our service (Rating)                                           │ │
│ │ Average: 4.2/5                                                          │ │
│ │ Distribution: ★★★★★ 60% | ★★★★ 25% | ★★★ 10% | ★★ 3% | ★ 2%           │ │
│ ├─────────────────────────────────────────────────────────────────────────┤ │
│ │ Q4. Select your department (Dropdown)                                   │ │
│ │ [Horizontal bar chart showing option distribution]                      │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Filter Specifications

### 5.1 Available Filters

| Filter | Type | Options |
|--------|------|---------|
| **Period** | Dropdown | All Time, This Month, Last Month, This Quarter, This Year, Custom Range |
| **Tenant** | Dropdown | All Tenants (within scope), List of accessible tenants |
| **Status** | Dropdown | All, Draft, Submitted, In Approval, Approved, Rejected |
| **Search** | Text Input | Searches submitter name/email, response text |

### 5.2 Filter Behavior
- Filters are applied via query parameters for bookmarkable URLs
- Tenant filter only shows tenants within user's scope
- "Clear Filters" button resets all filters
- Filter state persists during pagination

---

## 6. Data Models

### 6.1 New ViewModels Required

| ViewModel | Purpose |
|-----------|---------|
| `TemplateSubmissionsViewModel` | Main page model with template info, scope, filters, submissions list |
| `SubmissionRowViewModel` | Single row in the submissions table with inline previews |
| `ResponsePreview` | Truncated/formatted response for table column display |
| `SubmissionDetailViewModel` | Full submission details for offcanvas |
| `SectionResponseGroup` | Responses grouped by form section |
| `ResponseDetailViewModel` | Single response with full formatting |
| `SubmissionSummaryStats` | Aggregated statistics for Summary tab |
| `SubmissionFilters` | Current filter state |

### 6.2 Key Properties

**TemplateSubmissionsViewModel**
- Template metadata (ID, name, code, category, version, publish date)
- Scope information (code, display text, total count)
- Field columns configuration (which fields to show in table)
- List of submission rows
- Summary statistics
- Filter state
- Pagination state

**SubmissionRowViewModel**
- Submission ID and sequential number
- Submission date
- Submitter name and email
- Tenant name
- Status with badge class
- Completion percentage
- Flag/star state
- List of response previews for inline display

**SubmissionDetailViewModel**
- Full submission metadata
- Approval/review information
- Responses grouped by section
- Comments count
- History entries

---

## 7. Service Layer Changes

### 7.1 IFormSubmissionService - New Methods

| Method | Purpose |
|--------|---------|
| `GetTemplateSubmissionsPageAsync` | Get paginated, scope-filtered submissions for a template |
| `GetSubmissionDetailAsync` | Get full submission details for offcanvas |
| `GetScopedTemplateStatsAsync` | Get scope-filtered stats for form cards |
| `GetFieldColumnsForTemplateAsync` | Get first N fields for table columns |

### 7.2 FormSubmissionService Changes

1. **Inject IScopeService** - Required for scope-based filtering
2. **Add scope filtering logic** - Apply tenant filtering based on user scope
3. **Build dynamic field columns** - Select appropriate fields for table display
4. **Format response values** - Convert raw values to display format based on data type

### 7.3 Scope Filtering Logic

For each query:
1. Get user's scope via `IScopeService.GetUserScopeAsync()`
2. Get accessible tenant IDs via `IScopeService.GetAccessibleTenantIdsAsync()`
3. Apply filter:
   - INDIVIDUAL: `WHERE SubmittedBy = userId`
   - GLOBAL: No tenant filter
   - Others: `WHERE TenantId IN (accessibleTenantIds) OR TenantId IS NULL`

---

## 8. Controller Actions

### 8.1 New Actions in SubmissionsController

| Action | Route | Method | Purpose |
|--------|-------|--------|---------|
| `TemplateSubmissions` | `/Submissions/Template/{templateId}` | GET | Main submissions list page |
| `GetSubmissionDetail` | `/Submissions/Detail/{submissionId}` | GET (AJAX) | Offcanvas content |
| `ExportSubmissions` | `/Submissions/Export/{templateId}` | GET | Export to CSV/Excel |
| `ToggleFlag` | `/Submissions/ToggleFlag/{submissionId}` | POST (AJAX) | Star/unstar submission |

### 8.2 Action Parameters

**TemplateSubmissions**
- `templateId` (required): Template to show submissions for
- `status` (optional): Filter by status
- `tenant` (optional): Filter by tenant ID
- `period` (optional): Filter by period (e.g., "thisMonth", "lastQuarter")
- `search` (optional): Search text
- `page` (optional, default 1): Page number
- `tab` (optional, default "responses"): Active tab

---

## 9. View Files

### 9.1 New Files to Create

| File | Purpose |
|------|---------|
| `Views/Submissions/TemplateSubmissions.cshtml` | Main page view |
| `Views/Submissions/Partials/_TemplateSubmissionsHeader.cshtml` | Template info header |
| `Views/Submissions/Partials/_SubmissionsResponsesTab.cshtml` | Responses table tab content |
| `Views/Submissions/Partials/_SubmissionsSummaryTab.cshtml` | Summary statistics tab content |
| `Views/Submissions/Partials/_SubmissionDetailOffcanvas.cshtml` | Response detail offcanvas |
| `Views/Submissions/Partials/_ResponseDisplay.cshtml` | Single response formatting partial |
| `wwwroot/js/pages/template-submissions.js` | Page JavaScript |
| `wwwroot/css/template-submissions.css` | Page-specific styles |

### 9.2 Files to Modify

| File | Change |
|------|--------|
| `Views/Submissions/Partials/_AvailableFormCard.cshtml` | Update "View Submissions" link URL |
| `Views/Submissions/AvailableForms.cshtml` | Ensure stats use scope-filtered data |

---

## 10. JavaScript Functionality

### 10.1 Page Behaviors

| Feature | Description |
|---------|-------------|
| **Row Click** | Opens offcanvas with submission details |
| **Offcanvas Loading** | AJAX load of submission detail content |
| **Tab Switching** | Handles Responses/Summary tab switching |
| **Filter Changes** | Submits filter form on dropdown change |
| **Bulk Selection** | Checkbox select all / individual selection |
| **Export** | Triggers export with current filters |
| **Flag Toggle** | AJAX toggle of star/flag status |
| **Keyboard Navigation** | Arrow keys to navigate rows, Enter to open |

### 10.2 Offcanvas Behaviors

| Feature | Description |
|---------|-------------|
| **Tab Navigation** | Switch between Answers/Comments/History |
| **Image Gallery** | Lightbox for file upload previews |
| **Print** | Opens print-friendly view |
| **Full View** | Opens submission in new page |
| **Previous/Next** | Navigate between submissions |

---

## 11. Implementation Phases

### Phase 1: Core Functionality (Priority: High)
1. Create ViewModels for submissions page
2. Add scope-aware methods to IFormSubmissionService
3. Implement scope filtering in FormSubmissionService
4. Create TemplateSubmissions controller action
5. Create main TemplateSubmissions.cshtml view
6. Create Responses tab with table
7. Create offcanvas for response details
8. Update _AvailableFormCard.cshtml link
9. Basic JavaScript for offcanvas loading

### Phase 2: Enhanced Features (Priority: Medium)
1. Add Summary tab with statistics
2. Implement export functionality (CSV/Excel)
3. Add bulk selection and actions
4. Add flag/star functionality
5. Add Comments tab in offcanvas
6. Add History tab in offcanvas
7. Keyboard navigation

### Phase 3: Advanced Analytics (Priority: Low)
1. Field-level analytics in Summary
2. Charts and visualizations (Chart.js)
3. Submission timeline chart
4. Response distribution charts
5. Comparison between submissions
6. Advanced export options (PDF)

---

## 12. Testing Considerations

### 12.1 Scope Testing Scenarios

| Scenario | Expected Result |
|----------|-----------------|
| GLOBAL user views submissions | Sees all submissions |
| REGIONAL user views submissions | Sees only their region's submissions |
| TENANT user views submissions | Sees only their tenant's submissions |
| INDIVIDUAL user views submissions | Sees only their own submissions |
| User tries to access submission outside scope | Access denied or filtered out |

### 12.2 UI Testing

| Scenario | Expected Result |
|----------|-----------------|
| Click row in table | Offcanvas opens with correct submission |
| Apply status filter | Table updates with filtered results |
| Search for submitter | Table shows matching submissions |
| Paginate through results | Correct page loads, filters preserved |
| Export with filters | Export includes only filtered data |

---

## 13. Security Considerations

1. **Scope Enforcement**: All queries MUST filter by user scope
2. **Submission Access**: Verify user can access submission before returning details
3. **Template Access**: Verify user has access to template before showing submissions
4. **Export Limits**: Consider limiting export size or requiring confirmation
5. **Audit Logging**: Log access to sensitive submissions

---

## 14. Performance Considerations

1. **Pagination**: Always paginate submissions (default 10-20 per page)
2. **Lazy Loading**: Load offcanvas content via AJAX, not on page load
3. **Field Columns**: Limit inline preview fields to 3-5 to reduce query complexity
4. **Caching**: Consider caching template metadata and field configurations
5. **Indexes**: Ensure database indexes on TemplateId, TenantId, Status, SubmittedDate

---

## 15. Open Questions

1. **Tenant Filter Visibility**: Should users with REGIONAL/GLOBAL scope see a tenant filter dropdown?
   - Recommendation: Yes, for easier filtering

2. **Field Column Selection**: How many form fields should show as columns? Which ones?
   - Recommendation: First 3-5 fields, or fields marked as "key fields" in template

3. **Flagging Feature**: Should the star/flag feature be implemented in Phase 1?
   - Recommendation: Defer to Phase 2

4. **Comments Feature**: Do submissions have a comments/discussion feature?
   - Recommendation: Defer to Phase 2, design now

5. **History Tab**: What events should appear in submission history?
   - Recommendation: Status changes, edits, approvals, comments

6. **Mobile Responsiveness**: How should the table display on mobile?
   - Recommendation: Card view on mobile, table on desktop

---

## 16. Dependencies

### External Dependencies
- None new required

### Internal Dependencies
- `IScopeService` / `ScopeService` - For scope-based filtering
- `IFormSubmissionService` / `FormSubmissionService` - Core submission operations
- Existing partial views and components (DataTable, Tabs, etc.)

---

## 17. Success Metrics

1. Users can access submissions from AvailableForms page
2. Submissions are correctly filtered by user scope
3. Offcanvas displays complete response details
4. Page loads in under 2 seconds
5. Export functionality works correctly

---

## Appendix A: URL Structure

```
/Submissions/AvailableForms                    - List of available form templates
/Submissions/Template/{templateId}             - Submissions for a template
/Submissions/Template/{templateId}?status=Approved&page=2
/Submissions/Detail/{submissionId}             - AJAX: Submission detail
/Submissions/Export/{templateId}?format=csv    - Export submissions
/Submissions/View/{submissionId}               - Full page view (existing)
```

## Appendix B: Related Existing Files

### Models
- `Models/Entities/Forms/FormTemplateSubmission.cs`
- `Models/Entities/Forms/FormTemplateResponse.cs`
- `Models/Entities/Forms/FormTemplate.cs`
- `Models/Entities/Forms/FormTemplateItem.cs`

### Services
- `Services/Forms/IFormSubmissionService.cs`
- `Services/Forms/FormSubmissionService.cs`
- `Services/Forms/IFormResponseService.cs`
- `Services/Identity/IScopeService.cs`
- `Services/Identity/ScopeService.cs`

### Controllers
- `Controllers/Submissions/SubmissionsController.cs`

### Views
- `Views/Submissions/AvailableForms.cshtml`
- `Views/Submissions/Partials/_AvailableFormCard.cshtml`
- `Views/Submissions/Partials/_SubmissionsContent.cshtml`
