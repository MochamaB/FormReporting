# PHASE 2: TEMPLATE CREATION - MODE SELECTION

## Overview
This phase implements the user interface and logic for administrators to select submission mode when creating or editing form templates, with appropriate validation and guidance.

---

## Step 2.1: Update Template Creation Wizard UI

### Add Mode Selection to Template Setup Step

#### Location
- File: Views/Forms/FormTemplates/Create.cshtml
- Section: Step 1 - Template Setup
- Position: After template basic info, before sections

#### UI Component Requirements
- Radio button group for mode selection
- Two options: Traditional and Collaborative
- Each option has:
  - Clear label
  - Icon/visual indicator
  - Descriptive text explaining the mode
  - Example use case
- Default selection: Traditional
- Required field (cannot proceed without selection)

#### User Experience
- Selection prominently placed
- Tooltip/help icon with detailed explanation
- Visual differentiation between options (color, icons)
- Warning message if mode has implications
- Cannot be skipped or left blank

### Add Mode Selection Help/Info

#### Information Panel
- Expandable section explaining each mode
- "When to use Traditional" guidance
- "When to use Collaborative" guidance
- Examples of each type
- Link to documentation

#### Tooltips
- Hover tooltips on radio buttons
- Quick summary of each mode
- Link to "Learn more"

---

## Step 2.2: Update FormTemplateController

### Modify Create Action

#### GET Create Action
- Initialize model with default SubmissionMode
- Load mode options from enum/configuration
- Pass mode descriptions to view
- Set default selection

#### POST Create Action
- Validate SubmissionMode is provided
- Validate value is valid (Traditional or Collaborative)
- Include in template creation
- Persist to database

### Modify Edit Action

#### Display Mode (Read-Only if Submissions Exist)
- Check if template has submissions
- If yes: Display mode as read-only badge
- If no: Allow mode change
- Show warning about mode change implications

#### Validation on Update
- Check CanChangeSubmissionMode
- If template has submissions AND mode changed:
  - Return validation error
  - Display user-friendly message
  - Prevent save

---

## Step 2.3: Update FormTemplateService

### Add Mode Validation Logic

#### CreateTemplate Method
- Validate SubmissionMode parameter
- Ensure value is valid enum value
- Set default if not provided
- Log mode selection for audit

#### UpdateTemplate Method
- Check if mode change is allowed
- Validate no submissions exist if mode changing
- Throw business exception if not allowed
- Include detailed error message

### Add Helper Methods

#### CanChangeSubmissionMode
- Check if template has any submissions
- Return boolean
- Cache result if appropriate

#### GetSubmissionModeOptions
- Return list of available modes
- Include display name and description
- Used for dropdowns/radio buttons

---

## Step 2.4: Update Workflow Creation Wizard

### Make Workflow Wizard Mode-Aware

#### Load Template Mode
- When creating workflow for template
- Load template's SubmissionMode
- Pass to workflow wizard
- Store in ViewData/session

#### Filter Available Actions

##### Traditional Mode
- Hide "Fill" action from available actions list
- Show only: Approve, Reject, Sign, Review, Verify
- Display info message: "Fill action not available for Traditional templates"

##### Collaborative Mode
- Show ALL actions including Fill
- Highlight "Fill" as required action
- Display info message: "At least one Fill step required"

### Update Workflow Wizard Validation

#### Step Validation

##### Traditional Mode Rules
- Cannot add "Fill" action step
- Validation error if attempted
- Error message: "Traditional templates cannot use Fill actions in workflow"
- Suggest Approve/Review as first step

##### Collaborative Mode Rules
- Must have at least one "Fill" action step
- All Fill steps must come before approval steps
- Each Fill step must have:
  - TargetType specified (Section or Field)
  - TargetId specified
  - AssigneeType specified
- Validation error if rules violated

#### Submit/Save Validation
- Before saving workflow
- Validate mode-specific rules
- Show all validation errors together
- Prevent save if validation fails

---

## Step 2.5: Add Workflow Validation Service

### Create WorkflowValidationService

#### ValidateWorkflowForTemplate Method
- Input: WorkflowDefinition, TemplateId
- Load template to get SubmissionMode
- Apply mode-specific validation rules
- Return validation result with errors

#### Validation Rules by Mode

##### Traditional Mode Validations
- No Fill actions allowed
- First step should be approval-type
- Optional workflow (can be empty)
- All steps must have valid AssigneeType

##### Collaborative Mode Validations
- At least one Fill action required
- Fill actions must be first (before approvals)
- Each Fill must have TargetType
- Each Fill must have TargetId (section/field)
- Each Fill must have AssigneeType
- Warning if not all sections covered

### Integration Points
- Called from WorkflowController before save
- Called from WorkflowService.CreateWorkflow
- Called from WorkflowService.UpdateWorkflow
- Called from workflow wizard client-side

---

## Step 2.6: Update Workflow Creation UI

### Modify Workflow Wizard Steps

#### Action Selection Step
- Filter actions based on template mode
- Show/hide Fill action accordingly
- Display mode-specific guidance
- Visual indicators for available vs unavailable

#### Step Configuration
- When Fill action selected (Collaborative only)
- Show TargetType selector (Submission/Section/Field)
- Show TargetId selector (section/field dropdown)
- Make fields required
- Validate on blur/change

### Add Visual Indicators

#### Mode Badge
- Display template's submission mode
- Show at top of wizard
- Color-coded badge
- "Traditional" or "Collaborative"

#### Action Availability
- Disabled actions shown grayed out
- Tooltip explains why disabled
- Link to learn about mode restrictions

---

## Step 2.7: Add Client-Side Validation

### Workflow Wizard JavaScript

#### Mode Detection
- Detect template mode from ViewData
- Store in JavaScript state
- Use for client-side validation

#### Action Filtering
- Filter action list based on mode
- Hide/disable Fill for Traditional
- Show Fill prominently for Collaborative

#### Real-Time Validation
- Validate as user adds steps
- Check mode-specific rules
- Show validation messages inline
- Prevent submission if invalid

### Validation Messages
- Friendly error messages
- Explain what's wrong
- Suggest how to fix
- Link to documentation

---

## Step 2.8: Update Template Details View

### Display Submission Mode

#### Template Overview Section
- Add "Submission Mode" field
- Display as badge with icon
- Color-coded (blue for Traditional, green for Collaborative)
- Tooltip with explanation

#### Edit Mode Indicator
- If template has submissions: "Cannot change (submissions exist)"
- If no submissions: "Can be changed" with edit link
- Warning icon if mode locked

### Add Mode Change Warning

#### If User Attempts Change
- Modal confirmation dialog
- Explain implications of mode change
- List what will be affected
- Require explicit confirmation
- "Are you sure?" prompt

---

## Step 2.9: Add Mode Selection Help Documentation

### Create Help Content

#### In-App Help
- Dedicated help section for submission modes
- Accessible from template creation page
- Comparison table: Traditional vs Collaborative
- Decision tree to help choose
- Examples of each type

#### Tooltips and Hints
- Contextual help throughout wizard
- "?" icons next to mode selector
- Inline explanations
- Best practices

---

## Step 2.10: Add Validation Error Messages

### Define User-Friendly Messages

#### Mode Selection Errors
- "Submission mode is required"
- "Invalid submission mode selected"
- "Mode cannot be changed - template has submissions"

#### Workflow Validation Errors for Traditional
- "Fill action cannot be used in Traditional mode workflows"
- "Traditional templates require approval-type actions only"

#### Workflow Validation Errors for Collaborative
- "Collaborative templates require at least one Fill action"
- "Fill actions must come before approval actions"
- "Each Fill action must specify a target section or field"
- "Fill action must have an assignee specified"

### Error Display
- Show errors prominently
- Group related errors
- Provide actionable guidance
- Link to help documentation

---

## Implementation Checklist

### UI Layer
- [ ] Add mode selector to template creation wizard
- [ ] Add mode display to template details view
- [ ] Add mode indicator to workflow wizard
- [ ] Create help/documentation content
- [ ] Add tooltips and hints
- [ ] Design mode selection component
- [ ] Implement responsive design

### Controller Layer
- [ ] Update FormTemplatesController.Create (GET)
- [ ] Update FormTemplatesController.Create (POST)
- [ ] Update FormTemplatesController.Edit (GET)
- [ ] Update FormTemplatesController.Edit (POST)
- [ ] Update WorkflowController to pass template mode
- [ ] Add validation error handling

### Service Layer
- [ ] Update FormTemplateService.CreateTemplate
- [ ] Update FormTemplateService.UpdateTemplate
- [ ] Add CanChangeSubmissionMode method
- [ ] Add GetSubmissionModeOptions method
- [ ] Create WorkflowValidationService
- [ ] Add ValidateWorkflowForTemplate method
- [ ] Implement mode-specific validation rules

### Validation Layer
- [ ] Add mode validation to DTOs
- [ ] Add workflow validation rules
- [ ] Create custom validators if needed
- [ ] Add validation error messages
- [ ] Test all validation scenarios

### JavaScript Layer
- [ ] Add mode detection script
- [ ] Filter actions based on mode
- [ ] Implement client-side validation
- [ ] Add real-time validation feedback
- [ ] Handle mode change warnings

### Testing
- [ ] Test template creation with Traditional mode
- [ ] Test template creation with Collaborative mode
- [ ] Test mode change allowed (no submissions)
- [ ] Test mode change blocked (with submissions)
- [ ] Test workflow validation for Traditional
- [ ] Test workflow validation for Collaborative
- [ ] Test error messages display correctly
- [ ] Test UI components render correctly

---

## Verification Steps

### Functional Testing
1. Create template in Traditional mode - workflow accepts only approval actions
2. Create template in Collaborative mode - workflow requires Fill actions
3. Try to change mode with no submissions - should succeed
4. Try to change mode with submissions - should be blocked
5. Try to add Fill action to Traditional workflow - should be blocked
6. Try to save Collaborative workflow without Fill - should be blocked

### UI Testing
1. Mode selector is visible and prominent
2. Help text is clear and helpful
3. Validation errors are user-friendly
4. Mode badge displays correctly
5. Workflow wizard adapts to mode
6. Tooltips and hints are helpful

---

## Estimated Effort
- UI updates: 4 hours
- Controller updates: 2 hours
- Service layer: 3 hours
- Validation logic: 3 hours
- JavaScript: 3 hours
- Help content: 2 hours
- Testing: 4 hours
- Bug fixes: 2 hours

**Total: ~23 hours (3 days)**

---

## Dependencies
- Phase 1 (database and models must be complete)

## Blocks
- Phase 3 (Assignment Logic)
- Phase 4 (Submission Initiation)

## Risks
- Users may not understand mode differences (mitigate with good help content)
- Mode selection may be overlooked (mitigate with required field and prominent placement)
- Workflow validation may be too restrictive (gather feedback and adjust)

---

## Success Criteria
- [ ] Admins can select submission mode when creating templates
- [ ] Mode selection is intuitive and well-explained
- [ ] Workflow wizard adapts correctly to each mode
- [ ] Validation prevents invalid workflow configurations
- [ ] Mode cannot be changed once submissions exist
- [ ] All error messages are clear and actionable
- [ ] Help documentation is accessible and helpful
