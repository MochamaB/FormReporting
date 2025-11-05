# KTDA ICT Reporting System - Technology Stack

**Version:** 1.0
**Date:** October 29, 2025
**Purpose:** Complete technology stack specification and rationale

---

## Table of Contents

1. [Technology Stack Overview](#overview)
2. [Backend Technologies](#backend)
3. [Frontend Technologies](#frontend)
4. [Database Technologies](#database)
5. [Development Tools](#development-tools)
6. [Deployment & Hosting](#deployment)
7. [Why This Stack?](#rationale)
8. [Technology Comparison](#comparison)
9. [Learning Resources](#resources)

---

## <a name="overview"></a>Technology Stack Overview

### Architecture Type
**Server-Side Rendered (SSR) Web Application** with Progressive Enhancement

### Core Technology
**ASP.NET Core 8.0 MVC + Razor Pages**

### Deployment Target
**On-Premises IIS (Internet Information Services)** on Windows Server

---

## <a name="backend"></a>Backend Technologies

### 1. ASP.NET Core 8.0

**What it is:**
- Open-source, cross-platform framework for building modern web applications
- Built by Microsoft, successor to ASP.NET Framework
- High performance, modular, and cloud-ready

**Why we chose it:**
- ✅ **Mature & Stable**: Production-ready with long-term support (LTS)
- ✅ **High Performance**: 10x faster than traditional ASP.NET Framework
- ✅ **Microsoft Stack Alignment**: Integrates seamlessly with SQL Server, IIS, Windows Server
- ✅ **Built-in Features**: Authentication, authorization, dependency injection, logging
- ✅ **Active Development**: Regular updates and security patches from Microsoft
- ✅ **Enterprise Support**: Microsoft provides commercial support if needed

**Version:** 8.0 LTS (Long-Term Support until November 2026)

**Key Features Used:**
- MVC (Model-View-Controller) pattern for clean separation of concerns
- Razor Pages for form-heavy pages (simpler than full MVC)
- Dependency Injection (built-in)
- Middleware pipeline for request processing
- Model Binding and Validation
- Tag Helpers for cleaner Razor syntax

---

### 2. C# 12

**What it is:**
- Modern, type-safe, object-oriented programming language
- Runs on .NET 8 runtime

**Why we chose it:**
- ✅ **Type Safety**: Catches errors at compile-time
- ✅ **Rich Ecosystem**: Millions of NuGet packages available
- ✅ **LINQ**: Powerful query syntax for data manipulation
- ✅ **Async/Await**: Built-in support for asynchronous programming
- ✅ **Team Expertise**: Most .NET developers know C# well

**Key Language Features Used:**
- Records for DTOs (immutable data transfer objects)
- Nullable reference types for null safety
- Pattern matching for cleaner conditional logic
- LINQ for database queries
- Async/await for I/O operations

---

### 3. Entity Framework Core 8.0

**What it is:**
- Object-Relational Mapper (ORM) that eliminates most data-access code
- Maps database tables to C# classes
- Generates SQL queries from LINQ expressions

**Why we chose it:**
- ✅ **Productivity**: Write less SQL, more C#
- ✅ **Type Safety**: Compile-time checking of database queries
- ✅ **Migrations**: Database schema versioning and updates
- ✅ **Change Tracking**: Automatic detection of entity modifications
- ✅ **LINQ Support**: Write queries in C# instead of SQL

**Example:**
```csharp
// Instead of writing SQL:
// SELECT * FROM Tenants WHERE TenantType = 'Factory' AND IsActive = 1

// Write LINQ:
var factories = await _context.Tenants
    .Where(t => t.TenantType == "Factory" && t.IsActive)
    .ToListAsync();
```

**Key Features Used:**
- Code-First migrations
- DbContext for database operations
- Eager/lazy loading for related data
- Repository pattern implementation
- Stored procedure support
- Raw SQL queries when needed for complex reports

---

### 4. ASP.NET Core Identity

**What it is:**
- Complete membership system for user authentication and authorization
- Handles user registration, login, password management, roles, claims

**Why we chose it:**
- ✅ **Built-in**: No third-party dependencies
- ✅ **Secure**: Industry-standard password hashing (PBKDF2)
- ✅ **Customizable**: Can extend User and Role entities
- ✅ **Two-Factor Authentication**: Built-in 2FA support
- ✅ **Account Lockout**: Automatic brute-force protection

**Features Used:**
- User registration and login
- Password complexity requirements
- Role-based authorization
- Claims-based authorization for fine-grained permissions
- Account lockout after failed attempts
- Password reset functionality

---

### 5. Hangfire

**What it is:**
- Background job processing library for .NET
- Runs tasks outside the request-response cycle
- Persistent storage of job queues

**Why we chose it:**
- ✅ **Reliable**: Jobs persisted in database (survives app restarts)
- ✅ **Dashboard**: Built-in web UI to monitor jobs
- ✅ **Scheduling**: Cron-like scheduling for recurring tasks
- ✅ **Retries**: Automatic retry on failure
- ✅ **No Windows Service Needed**: Runs in-process

**Use Cases in KTDA System:**
- Monthly snapshot aggregation (1st day of each month at 2 AM)
- Daily backup status checks
- License expiry notifications (check daily)
- Email queue processing
- Report generation for large datasets
- Automated alerts based on rules

**Example:**
```csharp
// Schedule monthly snapshot refresh
RecurringJob.AddOrUpdate(
    "refresh-monthly-snapshots",
    () => _snapshotService.RefreshSnapshots(),
    Cron.Monthly(1, 2)); // 1st day of month at 2 AM
```

---

### 6. SignalR

**What it is:**
- Real-time web communication library
- Server can push updates to connected clients
- Uses WebSockets (falls back to Server-Sent Events or Long Polling)

**Why we chose it:**
- ✅ **Real-Time Notifications**: Push alerts to users instantly
- ✅ **Built-in to ASP.NET Core**: No external service needed
- ✅ **Automatic Reconnection**: Handles connection drops gracefully
- ✅ **Scalable**: Can add Redis backplane later if needed

**Use Cases in KTDA System:**
- Real-time notification bell updates
- Ticket assignment alerts
- Submission approval notifications
- System alerts (backup failures, license expiries)
- Dashboard live updates (optional)

**Example:**
```csharp
// Server pushes notification to specific user
await _hubContext.Clients
    .User(userId)
    .SendAsync("ReceiveNotification", notification);
```

**Note:** SignalR is used ONLY for notifications, not for entire UI like Blazor Server.

---

## <a name="frontend"></a>Frontend Technologies

### 7. Razor Pages & Razor Syntax

**What it is:**
- Server-side HTML templating engine
- Mix C# code with HTML using `@` symbol
- Pages compiled to C#, then executed on server to generate HTML

**Why we chose it:**
- ✅ **Type-Safe**: IntelliSense and compile-time checking
- ✅ **Server-Rendered**: No JavaScript required for basic functionality
- ✅ **Model Binding**: Automatic form-to-object mapping
- ✅ **Tag Helpers**: Clean HTML syntax for forms, links, etc.
- ✅ **Simpler than MVC**: Page-focused, less boilerplate

**When to use Razor Pages vs. MVC:**
- **Razor Pages**: Form-heavy CRUD operations (user management, ticket creation, hardware inventory)
- **MVC Controllers**: Complex workflows, APIs, multiple actions per entity

**Example:**
```html
@page
@model CreateTicketModel

<h2>Create Support Ticket</h2>

<form method="post">
    <div class="mb-3">
        <label asp-for="Ticket.Title" class="form-label"></label>
        <input asp-for="Ticket.Title" class="form-control" />
        <span asp-validation-for="Ticket.Title" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Submit Ticket</button>
</form>
```

---

### 8. Bootstrap 5

**What it is:**
- CSS framework for responsive, mobile-first design
- Pre-built components (buttons, forms, modals, navbars)
- Grid system for layouts

**Why we chose it:**
- ✅ **Industry Standard**: Most popular CSS framework
- ✅ **Responsive**: Works on desktop, tablet, mobile automatically
- ✅ **Component Library**: Buttons, forms, tables, modals, alerts
- ✅ **Customizable**: Can override default styles
- ✅ **Well Documented**: Extensive documentation and examples

**Version:** 5.3 (latest stable)

**Features Used:**
- Grid system for page layouts
- Form controls and validation styles
- Navigation components (navbar, breadcrumbs)
- Cards for content containers
- Modals for dialogs
- Tables with sorting and filtering
- Alerts and toasts for messages

---

### 9. jQuery 3.7

**What it is:**
- Fast, small JavaScript library
- Simplifies DOM manipulation, AJAX, event handling

**Why we chose it:**
- ✅ **Simple**: Easier to learn than React/Angular
- ✅ **AJAX Made Easy**: Simple API for server communication
- ✅ **Plugin Ecosystem**: Thousands of plugins (DataTables, Select2, etc.)
- ✅ **Browser Compatibility**: Handles browser differences automatically
- ✅ **Low Learning Curve**: Team can be productive quickly

**Use Cases:**
- Form validation (client-side)
- AJAX calls to load data without page refresh
- Dynamic UI updates (show/hide sections, enable/disable fields)
- Event handling (clicks, changes, submissions)
- Integrating third-party plugins

**Example:**
```javascript
// Load tickets via AJAX
$('#loadTicketsBtn').click(function() {
    $.ajax({
        url: '/api/tickets',
        method: 'GET',
        success: function(tickets) {
            displayTickets(tickets);
        }
    });
});
```

---

### 10. Chart.js 4.0

**What it is:**
- Simple yet flexible JavaScript charting library
- Creates responsive, animated charts
- Canvas-based rendering

**Why we chose it:**
- ✅ **Free & Open Source**: No licensing costs
- ✅ **Easy to Use**: Simple configuration
- ✅ **Responsive**: Charts resize automatically
- ✅ **Chart Types**: Line, bar, pie, doughnut, radar, etc.
- ✅ **Lightweight**: ~200KB minified

**Use Cases in KTDA System:**
- Dashboard KPI visualizations
- Monthly submission trends (line charts)
- Ticket distribution by category (pie charts)
- Regional comparison charts (bar charts)
- Hardware inventory breakdown (doughnut charts)
- Budget vs. actual spending (stacked bar charts)

**Example:**
```javascript
// Create bar chart for regional ticket counts
new Chart(document.getElementById('regionTicketsChart'), {
    type: 'bar',
    data: {
        labels: ['Region 1', 'Region 2', 'Region 3', 'Region 4', 'Region 5', 'Region 6', 'Region 7'],
        datasets: [{
            label: 'Open Tickets',
            data: [12, 19, 8, 15, 22, 10, 14],
            backgroundColor: 'rgba(54, 162, 235, 0.2)',
            borderColor: 'rgba(54, 162, 235, 1)',
            borderWidth: 1
        }]
    }
});
```

---

### 11. DataTables.js 2.0

**What it is:**
- jQuery plugin for advanced table features
- Adds sorting, searching, pagination, export to HTML tables

**Why we chose it:**
- ✅ **Feature-Rich**: Sorting, filtering, pagination out of the box
- ✅ **Server-Side Processing**: Can handle millions of rows
- ✅ **Export**: Excel, PDF, CSV export built-in
- ✅ **Responsive**: Mobile-friendly tables
- ✅ **Customizable**: Extensive API for customization

**Use Cases:**
- Ticket list tables
- Hardware inventory grids
- User management tables
- Software installation lists
- Any data grid with sorting/filtering needs

**Example:**
```javascript
$('#ticketsTable').DataTable({
    pageLength: 25,
    order: [[0, 'desc']], // Sort by first column descending
    buttons: ['copy', 'excel', 'pdf'],
    serverSide: true, // For large datasets
    ajax: '/api/tickets/datatable'
});
```

---

### 12. Additional Frontend Libraries

**Select2** (Dropdown enhancement)
- Multi-select with search
- AJAX data loading
- Tag input
- Use case: Tenant selection, user assignment, category selection

**Toastr** (Toast notifications)
- Non-blocking notifications
- Success/error/warning/info messages
- Auto-dismiss
- Use case: Form submission feedback, AJAX operation results

**jQuery Validation** (Client-side validation)
- Integrates with ASP.NET Core validation
- Real-time validation feedback
- Custom validation rules
- Use case: All forms require client-side validation

---

## <a name="database"></a>Database Technologies

### 13. Microsoft SQL Server 2022

**What it is:**
- Enterprise relational database management system (RDBMS)
- Highly performant, scalable, secure

**Why we chose it:**
- ✅ **KTDA Standard**: Already used in organization
- ✅ **Enterprise Features**: Backup, replication, high availability
- ✅ **Performance**: Query optimizer, indexing, partitioning
- ✅ **Security**: Row-level security, encryption, auditing
- ✅ **Integration**: Tight integration with .NET and IIS
- ✅ **Tooling**: SQL Server Management Studio (SSMS)

**Edition:** Standard Edition (sufficient for this system)

**Key Features Used:**
- Tables, views, stored procedures
- Indexes for query optimization
- Foreign keys for referential integrity
- CHECK constraints for data validation
- Triggers for audit logging (optional)
- Full-text search for ticket descriptions
- JSON support for flexible fields

---

## <a name="development-tools"></a>Development Tools

### IDE
- **Visual Studio 2022 Community/Professional**
  - Full-featured IDE for .NET development
  - IntelliSense, debugging, refactoring tools
  - Integrated database tools
  - NuGet package management

### Database Management
- **SQL Server Management Studio (SSMS) 19**
  - Database design and administration
  - Query development and optimization
  - Backup and restore operations

### Version Control
- **Git** for source code management
- **GitHub/GitLab/Azure DevOps** for remote repository

### API Testing
- **Postman** or **Swagger UI** (built into ASP.NET Core)
  - Test API endpoints
  - Generate API documentation

---

## <a name="deployment"></a>Deployment & Hosting

### Web Server
**Internet Information Services (IIS) 10.0**
- Built into Windows Server 2022
- ASP.NET Core Hosting Bundle required
- Application pools for isolation
- HTTPS/SSL support
- URL rewriting
- Request filtering

### Hosting Configuration
```
Windows Server 2022
├── IIS 10.0
│   ├── Application Pool (KTDAReporting)
│   │   ├── .NET CLR Version: No Managed Code
│   │   ├── Managed Pipeline: Integrated
│   │   └── Identity: ApplicationPoolIdentity
│   └── Website (KTDA ICT Reporting)
│       ├── Port: 443 (HTTPS)
│       ├── SSL Certificate: Company SSL cert
│       └── Application: /
└── SQL Server 2022
    └── Database: KTDA_ICT_Reporting
```

### Application Deployment
- **Publish Method**: Folder publish
- **Target Framework**: net8.0
- **Deployment Mode**: Framework-dependent
- **Target Runtime**: win-x64

### Background Services
- **Hangfire Dashboard**: /hangfire (admin only)
- **SignalR Hub**: /notificationHub

---

## <a name="rationale"></a>Why This Stack?

### Decision Criteria

**1. Team Skillset**
- ✅ C# developers are common
- ✅ JavaScript basics are easier to learn than React/Angular
- ✅ SQL Server is KTDA standard

**2. Project Requirements**
- ✅ Form-heavy application (Razor Pages excel here)
- ✅ Internal system (SEO not needed)
- ✅ ~100-200 concurrent users (server-side rendering is fine)
- ✅ Factory networks (stateless pages work better than WebSocket-dependent Blazor)

**3. Deployment Constraints**
- ✅ On-premises IIS hosting (no cloud dependencies)
- ✅ Windows Server environment
- ✅ Single-server deployment initially (can scale later)

**4. Maintainability**
- ✅ Single codebase (no separate frontend repo)
- ✅ Mature, stable technologies (less churn)
- ✅ Extensive documentation and community support

**5. Cost**
- ✅ All tools are free or already licensed
- ✅ No cloud service costs
- ✅ Low server resource requirements

---

## <a name="comparison"></a>Technology Comparison

### Why NOT Blazor Server?

| Aspect | Razor Pages + jQuery | Blazor Server |
|--------|---------------------|---------------|
| Connection | Stateless HTTP | Persistent WebSocket |
| Server Load | Low (per request) | High (per user session) |
| Network Issues | Graceful (page reload) | App breaks until reconnect |
| Latency | One roundtrip per action | Every UI interaction roundtrips |
| Learning Curve | Low | Medium-High |
| Best For | Forms, CRUD, reports | Rich, interactive SPAs |

**Verdict:** Blazor Server is overkill for KTDA's form-heavy CRUD application.

---

### Why NOT React/Angular SPA?

| Aspect | Razor Pages + jQuery | React/Angular SPA |
|--------|---------------------|-------------------|
| Codebase | Single (C#) | Dual (C# + TypeScript) |
| Learning Curve | Low | High |
| SEO | Good | Poor (unless SSR) |
| Initial Load | Fast | Slow (large JS bundle) |
| API Needed | No | Yes (separate API project) |
| Complexity | Low | High |

**Verdict:** SPAs are for public-facing, highly interactive applications. KTDA doesn't need this complexity.

---

## <a name="resources"></a>Learning Resources

### Official Documentation
- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core
- **Entity Framework Core**: https://docs.microsoft.com/ef/core
- **Bootstrap 5**: https://getbootstrap.com/docs/5.3
- **Chart.js**: https://www.chartjs.org/docs
- **Hangfire**: https://docs.hangfire.io
- **SignalR**: https://docs.microsoft.com/aspnet/core/signalr

### Video Tutorials
- **ASP.NET Core MVC**: Microsoft Learn (free courses)
- **Razor Pages**: Pluralsight, Udemy
- **jQuery**: YouTube (Traversy Media, freeCodeCamp)

### Sample Projects
- **Microsoft eShopOnWeb**: https://github.com/dotnet-architecture/eShopOnWeb
  - Reference architecture for ASP.NET Core applications

---

## Summary

**Technology Stack:**
```
Backend:
├── ASP.NET Core 8.0 MVC + Razor Pages
├── C# 12
├── Entity Framework Core 8.0
├── ASP.NET Core Identity
├── Hangfire (background jobs)
└── SignalR (real-time notifications)

Frontend:
├── Razor Syntax (server-rendered)
├── Bootstrap 5 (CSS framework)
├── jQuery 3.7 (JavaScript library)
├── Chart.js 4.0 (charting)
├── DataTables.js 2.0 (data grids)
├── Select2 (dropdowns)
├── Toastr (notifications)
└── jQuery Validation (client validation)

Database:
└── Microsoft SQL Server 2022 Standard

Hosting:
└── IIS 10.0 on Windows Server 2022
```

**This stack is:**
- ✅ **Proven**: Used by thousands of enterprise applications
- ✅ **Stable**: Mature technologies with long-term support
- ✅ **Maintainable**: Single language, clear patterns
- ✅ **Scalable**: Can handle KTDA's growth for years
- ✅ **Cost-Effective**: Minimal licensing and infrastructure costs
- ✅ **Team-Friendly**: Aligns with existing skills

---

**Document Version:** 1.0
**Last Updated:** October 29, 2025
**Related Documents:**
- Backend Structure: See `BackendStructure.md`
- Database Schema: See `KTDA_Enhanced_Database_Schema.sql`
- Data Dictionary: See `KTDA_Data_Dictionary.md`
