# Authentication & Identity Management Implementation Plan

**Module:** User Authentication, Authorization & RBAC  
**Version:** 1.0  
**Last Updated:** November 18, 2025

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Identity Configuration](#identity-configuration)
4. [Authentication Service](#authentication-service)
5. [Claims Service](#claims-service)
6. [Scope Service](#scope-service)
7. [Multi-Channel Notification Service](#notification-service)
8. [Account Controller](#account-controller)
9. [Password Controller](#password-controller)
10. [Authorization Attributes](#authorization-attributes)
11. [Audit Logging](#audit-logging)
12. [ViewModels](#viewmodels)
13. [Implementation Order](#implementation-order)
14. [Testing Checklist](#testing-checklist)

---

## 1. Overview

### Purpose

This document outlines the complete implementation plan for the Authentication and Identity Management system in the KTDA ICT Reporting System.

### Key Features

- ✅ Login/Logout with lockout protection (5 attempts = 30 min lockout)
- ✅ Password Management (Forgot, Reset, Change)
- ✅ Claims-Based Authorization (Identity, Roles, Permissions, Scope)
- ✅ Hierarchical Scope System (Global → Regional → Tenant → Department → Team → Individual)
- ✅ Multi-Channel Notifications (Email, SMS, Push, In-App)
- ✅ Audit Logging for all authentication events
- ❌ No User Registration (users created by admin or synced)

### Model Analysis Summary

✅ **User Model:** ASP.NET Identity compatible (PasswordHash, SecurityStamp, LockoutEnd, AccessFailedCount)  
✅ **ScopeLevel Model:** Hierarchical scopes (Level 1-6)  
✅ **Notification System:** Multi-channel with templates, preferences, delivery tracking  
❌ **No Registration:** Users created by admin only

---

## 2. Architecture

### Component Structure

```
Authentication System
├── Identity Configuration (Program.cs)
├── Services
│   ├── IAuthenticationService
│   ├── IClaimsService
│   ├── IScopeService
│   └── INotificationService
├── Controllers
│   ├── AccountController (Login, Logout)
│   └── PasswordController (Forgot, Reset, Change)
├── ViewModels
│   ├── LoginViewModel
│   ├── ForgotPasswordViewModel
│   ├── ResetPasswordViewModel
│   └── ChangePasswordViewModel
└── Views
    ├── Account/Login.cshtml
    ├── Password/ForgotPassword.cshtml
    ├── Password/ResetPassword.cshtml
    └── Password/ChangePassword.cshtml
```

---

## 3. Identity Configuration

### File: `Program.cs`

Configure ASP.NET Core Identity with custom settings:

**Password Requirements:**
- Minimum 8 characters
- Require uppercase, lowercase, digit, special character

**Lockout Settings:**
- Max failed attempts: 5
- Lockout duration: 30 minutes

**Cookie Settings:**
- HttpOnly, Secure, SameSite=Strict
- Sliding expiration: 30 minutes

**Token Lifetime:**
- Password reset: 24 hours

---

## 4. Authentication Service

### Interface: `Services/Identity/IAuthenticationService.cs`

```csharp
Task<SignInResult> LoginAsync(string usernameOrEmail, string password, bool rememberMe);
Task LogoutAsync();
Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
Task<bool> ValidateUserIsActiveAsync(User user);
Task<bool> IsAccountLockedAsync(User user);
Task UpdateLastLoginAsync(int userId);
Task IncrementAccessFailedCountAsync(User user);
Task ResetAccessFailedCountAsync(User user);
```

### Responsibilities

- Validate credentials using UserManager
- Check IsActive flag
- Check lockout status (LockoutEnd)
- Track failed attempts (AccessFailedCount)
- Lock account after 5 failures for 30 minutes
- Update LastLoginDate on success

---

## 5. Claims Service

### Interface: `Services/Identity/IClaimsService.cs`

```csharp
Task<List<Claim>> BuildUserClaimsAsync(User user);
Task<List<Claim>> GetIdentityClaimsAsync(User user);
Task<List<Claim>> GetRoleClaimsAsync(User user);
Task<List<Claim>> GetPermissionClaimsAsync(User user);
Task<List<Claim>> GetTenantAccessClaimsAsync(User user);
Task<List<Claim>> GetScopeClaimsAsync(User user);
```

### Claims to Build (WF-2.7)

**1. Identity Claims:**
- NameIdentifier, Name, Email, FullName, EmployeeNumber, DepartmentId, DepartmentName

**2. Role Claims:**
- ClaimTypes.Role for each user role

**3. Permission Claims:**
- "Permission" claim for each permission (e.g., "Forms.Submit")

**4. Scope Claims (Using ScopeLevel):**
- ScopeLevel, ScopeName, ScopeCode
- TenantAccess based on level (*, Region:X, Tenant:Y, Department:Z)

**5. Tenant Access Exceptions:**
- TenantAccessException for each UserTenantAccess record

**6. Primary Context:**
- PrimaryTenantId, TenantName, RegionId

---

## 6. Scope Service

### Interface: `Services/Identity/IScopeService.cs`

```csharp
Task<UserScope> GetUserScopeAsync(ClaimsPrincipal user);
Task<List<int>> GetAccessibleTenantIdsAsync(ClaimsPrincipal user);
Task<bool> HasAccessToTenantAsync(ClaimsPrincipal user, int tenantId);
Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionCode);
```

### UserScope Class

```csharp
public class UserScope
{
    public int UserId { get; set; }
    public string ScopeName { get; set; }
    public int Level { get; set; }
    public List<int> AccessibleTenantIds { get; set; }
    public List<string> Permissions { get; set; }
}
```

### Tenant Access Logic

- **Level 1 (Global):** All tenants
- **Level 2 (Regional):** Tenants in user's region
- **Level 3 (Tenant):** User's primary tenant only
- **Level 4+ (Department/Team/Individual):** User's primary tenant only
- **Plus:** UserTenantAccess exceptions

---

## 7. Multi-Channel Notification Service

### Interface: `Services/Notifications/INotificationService.cs`

```csharp
Task SendNotificationAsync(NotificationRequest request);
Task SendPasswordResetNotificationAsync(User user, string resetToken);
Task SendPasswordChangedNotificationAsync(User user);
Task SendAccountLockedNotificationAsync(User user);
Task SendFromTemplateAsync(string templateCode, Dictionary<string, string> placeholders, List<int> recipientUserIds);
```

### Implementation Flow

1. Create Notification record
2. Create NotificationRecipient records
3. Check user preferences per channel
4. Create NotificationDelivery records (status: Pending)
5. Queue background job to process deliveries
6. Respect user preferences and quiet hours

### Channels Supported

- **Email:** SMTP configuration
- **SMS:** Twilio/Africa's Talking
- **Push:** Firebase Cloud Messaging
- **InApp:** Database notifications

---

## 8. Account Controller

### File: `Controllers/Identity/AccountController.cs`

**Login (GET):**
- Display login form
- Redirect if already authenticated

**Login (POST) - WF-2.6:**
1. Get user by username/email
2. Check IsActive flag
3. Check lockout status
4. Validate password
5. On success: Update LastLoginDate, reset failed count, log event
6. On failure: Increment failed count, lock after 5 attempts, send notification

**Logout:**
- Sign out user
- Log event
- Redirect to login

---

## 9. Password Controller

### File: `Controllers/Identity/PasswordController.cs`

**Forgot Password (WF-2.8):**
1. Validate email exists
2. Generate reset token (24-hour expiry)
3. Send multi-channel notification with reset link
4. Log event

**Reset Password:**
1. Validate token
2. Update password
3. Send confirmation notification
4. Log event

**Change Password (Authenticated):**
1. Validate current password
2. Update to new password
3. Refresh sign-in
4. Send confirmation notification
5. Log event

---

## 10. Authorization Attributes

### PermissionAuthorizeAttribute

```csharp
[PermissionAuthorize("Forms.Submit")]
public IActionResult SubmitForm() { }
```

Checks if user has specific permission claim.

### ScopeAuthorizeAttribute

```csharp
[ScopeAuthorize]
public IActionResult ViewTenant(int tenantId) { }
```

Checks if user has access to specified tenant.

---

## 11. Audit Logging

### Events to Log

- Login (Success/Failure)
- Logout
- Password Reset Requested
- Password Reset Completed
- Password Changed
- Account Locked
- Failed Login Attempt

### AuditLog Fields

- UserId, Action, IpAddress, UserAgent, Timestamp, Success, FailureReason

---

## 12. ViewModels

### LoginViewModel
- UserName (required)
- Password (required)
- RememberMe (bool)

### ForgotPasswordViewModel
- Email (required, EmailAddress)

### ResetPasswordViewModel
- Email, Token, Password, ConfirmPassword

### ChangePasswordViewModel
- CurrentPassword, NewPassword, ConfirmPassword

---

## 13. Implementation Order

1. ✅ Configure Identity in Program.cs
2. ✅ Create ViewModels
3. ✅ Create Authentication Service
4. ✅ Create Claims Service
5. ✅ Create Scope Service
6. ✅ Create Notification Service
7. ✅ Create Account Controller
8. ✅ Create Password Controller
9. ✅ Create Views
10. ✅ Create Authorization Attributes
11. ✅ Add Audit Logging
12. ✅ Test all flows

---

## 14. Testing Checklist

- [ ] Login with valid credentials
- [ ] Login with invalid credentials
- [ ] Account locks after 5 failed attempts
- [ ] Login with inactive account fails
- [ ] Login with locked account fails
- [ ] Logout works correctly
- [ ] Forgot password sends notification
- [ ] Reset password with valid token
- [ ] Reset password with expired token fails
- [ ] Change password (authenticated)
- [ ] Claims are built correctly
- [ ] Permissions are checked correctly
- [ ] Tenant scope is enforced
- [ ] Multi-channel notifications work
- [ ] Audit logs are created
- [ ] User preferences are respected

---

**END OF DOCUMENT**
