# PHASE 1: FOUNDATION - DATABASE & MODEL UPDATES

## Overview
This phase establishes the foundational database structure and model updates needed to support both Traditional and Collaborative submission modes.

---

## Step 1.1: Add SubmissionMode to FormTemplate

### Database Changes

#### Add SubmissionMode Column
- Add column `SubmissionMode` to FormTemplates table
- Type: NVARCHAR(20)
- Required: Yes
- Default value: 'Traditional'
- Add check constraint to allow only 'Traditional' or 'Collaborative'

#### Create System Configuration (Optional)
- Create SystemConfiguration table for system-wide defaults
- Store default submission mode preference
- Allow admin to configure default for new templates

### Model Changes

#### Update FormTemplate Entity
- Add SubmissionMode property (string, required, max 20 chars)
- Add validation attribute for allowed values
- Add computed property IsSubmissionModeChangeable
  - Returns false if template has any submissions
  - Returns true if no submissions exist

### DTO Updates

#### FormTemplateCreateDto
- Add SubmissionMode property
- Default to "Traditional"
- Add validation to ensure value is valid

#### FormTemplateUpdateDto
- Add SubmissionMode property (optional)
- Add validation: cannot change if submissions exist
- Throw business exception if change attempted with existing submissions

### Migration
- Create migration file: AddSubmissionModeToFormTemplate
- Add column with default
- Add check constraint
- Update all existing templates to 'Traditional'
- Make column non-nullable

---

## Step 1.2: Update FormTemplateSubmission Status Values

### Database Changes

#### Expand Status Column Values
- Drop existing status check constraint
- Add new constraint with expanded values:
  - Draft (Traditional only)
  - InProgress (Collaborative only)
  - Submitted (Both modes)
  - InApproval (Both modes)
  - Approved (Both modes)
  - Rejected (Both modes)
  - Returned (Both modes)

### Model Changes

#### Update FormTemplateSubmission Entity
- Update Status property validation
- Add computed property: IsInProgress
- Add computed property: IsReturned
- Document status meanings in XML comments

### Business Rules Documentation

#### Define Status Transition Rules
**Traditional Mode Flow:**
- Draft → Submitted → InApproval → Approved
- Can return to Draft if Returned

**Collaborative Mode Flow:**
- InProgress → Submitted → InApproval → Approved
- Can return to InProgress if Returned

### Migration
- Create migration: UpdateSubmissionStatusValues
- Drop old constraint
- Add new constraint
- Verify no existing data conflicts

---

## Step 1.3: Add Metadata to Track Submission Type

### Database Changes

#### Add New Columns to FormTemplateSubmissions
- InitiatedBy (INT, nullable, FK to Users)
- InitiationSource (NVARCHAR(20), required, default 'UserCreated')
- CompletionPercentage (DECIMAL(5,2), required, default 0.00)

#### Add Constraints
- Check constraint for InitiationSource (UserCreated, SystemCreated, AdminCreated, ScheduledJob)
- Check constraint for CompletionPercentage (0-100 range)
- Foreign key for InitiatedBy to Users table

### Model Changes

#### Update FormTemplateSubmission Entity
- Add InitiatedBy property (nullable int)
- Add InitiationSource property (string, required)
- Add CompletionPercentage property (decimal)
- Add navigation property: InitiatingUser

#### Add Computed Properties
- IsUserInitiated (returns true if InitiationSource is UserCreated)
- IsSystemInitiated (returns true if System or ScheduledJob)
- IsAdminInitiated (returns true if AdminCreated)

### Purpose Documentation
- InitiatedBy: Who/what created the submission (can differ from SubmittedBy in Collaborative)
- InitiationSource: How it was created (manual, scheduled, admin)
- CompletionPercentage: Track progress in Collaborative mode (sections completed)

### Migration
- Create migration: AddSubmissionMetadata
- Add columns with defaults
- Add constraints
- Update existing records with default values

---

## Step 1.4: Update SubmissionWorkflowProgress

### Database Changes

#### Add Tracking Columns
- IsCollaborativeFillStep (BIT, required, default 0)
- CompletedSectionCount (INT, required, default 0)
- TotalSectionCount (INT, required, default 0)

### Model Changes

#### Update SubmissionWorkflowProgress Entity
- Add IsCollaborativeFillStep property (bool)
- Add CompletedSectionCount property (int)
- Add TotalSectionCount property (int)

#### Add Computed Property
- SectionCompletionPercentage
  - Calculates percentage based on CompletedSectionCount / TotalSectionCount
  - Returns 0 if TotalSectionCount is 0

### Purpose
- Track whether step is a collaborative fill step
- Monitor progress of section completion
- Used only for Fill steps in Collaborative mode

### Migration
- Create migration: UpdateWorkflowProgressForCollaborative
- Add columns with defaults
- Update existing records (all to 0/false)

---

## Step 1.5: Create Mode Configuration Enums

### Create SubmissionMode Enum
- File: Models/Common/SubmissionMode.cs
- Values:
  - Traditional = 0
  - Collaborative = 1
- Add XML documentation for each value

### Create InitiationSource Enum
- File: Models/Common/InitiationSource.cs
- Values:
  - UserCreated = 0
  - SystemCreated = 1
  - AdminCreated = 2
  - ScheduledJob = 3
- Add XML documentation explaining each source

### Purpose
- Type-safe representation of submission modes
- Prevents magic strings in code
- Better IntelliSense support

---

## Step 1.6: Update ViewModels

### FormTemplateViewModel
- Add SubmissionMode property
- Add SubmissionModeDisplay property (returns friendly name)
- Add IsCollaborative computed property
- Add IsTraditional computed property

### TemplateDetailsViewModel
- Add SubmissionMode property
- Add SubmissionModeDescription property (detailed explanation)
- Add CanChangeSubmissionMode property (checks if submissions exist)

### SubmissionViewModel
- Add InitiationSource property
- Add InitiationSourceDisplay property (friendly name)
- Add InitiatedByName property
- Add CompletionPercentage property
- Add IsInProgress computed property

### Purpose
- Provide UI-friendly representations of data
- Encapsulate business logic for display
- Support both modes in views

---

## Step 1.7: Database Indexes

### Create Performance Indexes

#### Index for Submission Mode Filtering
- Table: FormTemplates
- Columns: SubmissionMode
- Include: TemplateId, TemplateName, IsActive
- Purpose: Fast filtering of templates by mode

#### Index for Submission Status
- Table: FormTemplateSubmissions
- Columns: Status, InitiationSource
- Include: SubmissionId, TemplateId, SubmittedBy, SubmittedDate
- Purpose: Fast filtering for dashboards

#### Filtered Index for In-Progress
- Table: FormTemplateSubmissions
- Columns: Status, InitiationSource
- Filter: WHERE Status = 'InProgress'
- Purpose: Quick lookup of collaborative in-progress submissions

---

## Step 1.8: Data Migration Script

### Migrate Existing Data

#### Set Default Values
- Update all FormTemplates to SubmissionMode = 'Traditional'
- Update all FormTemplateSubmissions:
  - InitiationSource = 'UserCreated'
  - InitiatedBy = SubmittedBy
  - CompletionPercentage = 100 if Approved/Submitted, 0 otherwise

#### Verification Queries
- Count templates by SubmissionMode
- Count submissions by InitiationSource
- Verify no null values in required fields

### Rollback Capability
- Document rollback steps for each migration
- Test rollback procedures
- Keep backup of pre-migration data

---

## Step 1.9: Validation Attributes

### Create Custom Validators

#### SubmissionModeAttribute
- Custom validation attribute
- Validates value is "Traditional" or "Collaborative"
- Returns meaningful error message if invalid
- Apply to DTOs and ViewModels

#### InitiationSourceAttribute (if needed)
- Validates InitiationSource values
- Ensures consistency across application

### Apply Validation
- Add attributes to DTOs
- Add attributes to ViewModels
- Configure validation in service layer

---

## Step 1.10: Unit Tests

### Test Coverage Required

#### FormTemplate Tests
- Test default SubmissionMode is Traditional
- Test SubmissionMode can be set to Collaborative
- Test SubmissionMode cannot be changed when submissions exist
- Test SubmissionMode can be changed when no submissions exist
- Test validation rejects invalid mode values

#### FormTemplateSubmission Tests
- Test InitiationSource defaults to UserCreated
- Test CompletionPercentage defaults to 0
- Test Status InProgress only allowed for Collaborative
- Test Status Draft only allowed for Traditional
- Test InitiatedBy can differ from SubmittedBy

#### Validation Tests
- Test SubmissionModeAttribute with valid values
- Test SubmissionModeAttribute rejects invalid values
- Test constraint violations are caught

### Integration Tests
- Test migration runs successfully
- Test data migration preserves existing data
- Test rollback procedures work correctly

---

## Implementation Checklist

### Database Layer
- [ ] Create SubmissionMode migration
- [ ] Create submission metadata migration
- [ ] Create workflow progress migration
- [ ] Add all check constraints
- [ ] Add foreign keys
- [ ] Create performance indexes
- [ ] Write and run data migration script
- [ ] Document rollback procedures

### Model Layer
- [ ] Update FormTemplate entity
- [ ] Update FormTemplateSubmission entity
- [ ] Update SubmissionWorkflowProgress entity
- [ ] Create SubmissionMode enum
- [ ] Create InitiationSource enum
- [ ] Add all computed properties
- [ ] Update XML documentation

### DTO Layer
- [ ] Update FormTemplateCreateDto
- [ ] Update FormTemplateUpdateDto
- [ ] Update SubmissionCreateDto
- [ ] Add validation attributes
- [ ] Test DTO validation

### ViewModel Layer
- [ ] Update FormTemplateViewModel
- [ ] Update TemplateDetailsViewModel
- [ ] Update SubmissionViewModel
- [ ] Add display properties
- [ ] Add computed properties

### Validation Layer
- [ ] Create SubmissionModeAttribute
- [ ] Create other custom validators as needed
- [ ] Apply to DTOs
- [ ] Apply to ViewModels
- [ ] Test validation logic

### Testing Layer
- [ ] Write FormTemplate unit tests
- [ ] Write FormTemplateSubmission unit tests
- [ ] Write validation tests
- [ ] Write integration tests
- [ ] Test migration scripts
- [ ] Test rollback procedures
- [ ] Verify test coverage (>80%)

### Documentation
- [ ] Document status transition flows
- [ ] Document submission mode meanings
- [ ] Update database schema documentation
- [ ] Document migration procedures
- [ ] Document rollback procedures
- [ ] Update API documentation

---

## Verification Steps

### Post-Implementation Checks
1. All migrations run without errors
2. All existing data migrated correctly
3. All unit tests pass
4. All integration tests pass
5. Database constraints enforced
6. No performance degradation
7. Rollback procedures tested and verified

### Quality Gates
- Code review completed
- All tests passing
- Documentation updated
- Migration scripts peer-reviewed
- Performance benchmarks met

---

## Estimated Effort
- Database migrations: 2 hours
- Entity model updates: 3 hours
- DTO/ViewModel updates: 2 hours
- Custom validators: 2 hours
- Unit tests: 4 hours
- Integration tests: 2 hours
- Testing & verification: 3 hours
- Documentation: 2 hours

**Total: ~20 hours (2.5 days)**

---

## Dependencies
- None (this is the foundation phase)

## Blocks
- Phase 2 (Template Creation)
- Phase 3 (Assignment Logic)
- Phase 4 (Submission Initiation)

## Risks
- Data migration may take longer than expected for large datasets
- Existing code may reference status values directly (find and update)
- Index creation may impact performance on large tables (schedule during maintenance)

---

## Success Criteria
- [ ] All database schema changes deployed
- [ ] All models support both modes
- [ ] All tests passing
- [ ] No breaking changes to existing functionality
- [ ] Existing templates work as before (backward compatible)
- [ ] Performance benchmarks met or exceeded
