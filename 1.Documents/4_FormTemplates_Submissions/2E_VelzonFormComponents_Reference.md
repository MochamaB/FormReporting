# Velzon Form Components HTML/CSS Reference

**Document:** 2E_VelzonFormComponents_Reference.md
**Location:** 1.Documents/4_FormTemplates_Submissions/
**Purpose:** Reference guide for Velzon admin template form HTML structures and Bootstrap classes
**Source Files:** 1.Documents/velzon/Form/*.html
**Date:** 2025-11-07

---

## Table of Contents

1. [Basic Text Input](#1-basic-text-input)
2. [Input with Required Indicator](#2-input-with-required-indicator)
3. [Input with Placeholder](#3-input-with-placeholder)
4. [Input with Help Text](#4-input-with-help-text)
5. [Input with Prefix/Suffix](#5-input-with-prefixsuffix-input-groups)
6. [Textarea](#6-textarea)
7. [Number Input](#7-number-input)
8. [Email/Phone/URL Inputs](#8-emailphoneurl-inputs)
9. [Dropdown/Select](#9-dropdownselect)
10. [Radio Button Group](#10-radio-button-group)
11. [Checkbox](#11-checkbox)
12. [Date Picker](#12-date-picker)
13. [File Upload](#13-file-upload)
14. [Validation Error State](#14-validation-error-state)
15. [Read-only/Disabled Input](#15-read-only--disabled-input)
16. [Form Section/Card](#16-form-sectioncard)
17. [Summary: Key Classes](#summary-key-velzon-classes--patterns)

---

## 1. BASIC TEXT INPUT

**HTML Structure:**
```html
<div>
    <label for="basicInput" class="form-label">Basic Input</label>
    <input type="text" class="form-control" id="basicInput">
</div>
```

**Required Classes:**
- Wrapper: `<div>` (no class needed)
- Label: `form-label`
- Input: `form-control`

---

## 2. INPUT WITH REQUIRED INDICATOR

**HTML Structure:**
```html
<div>
    <label for="requiredInput" class="form-label">
        Field Name <span class="text-danger">*</span>
    </label>
    <input type="text" class="form-control" id="requiredInput" required>
</div>
```

**Alternative (with bold asterisk):**
```html
<div>
    <label for="requiredInput" class="form-label">
        Field Name
        <span class="fw-bold text-danger">*</span>
    </label>
    <input type="text" class="form-control" id="requiredInput" required>
</div>
```

**Required Classes:**
- Text danger class: `text-danger` (for red asterisk)
- Optional weight: `fw-bold`

---

## 3. INPUT WITH PLACEHOLDER

**HTML Structure:**
```html
<div>
    <label for="placeholderInput" class="form-label">Input with Placeholder</label>
    <input type="text" class="form-control" id="placeholderInput" placeholder="Enter your name">
</div>
```

**Required Classes:**
- Input: `form-control`
- Attribute: `placeholder="text"`

---

## 4. INPUT WITH HELP TEXT

**HTML Structure:**
```html
<div>
    <label for="formtextInput" class="form-label">Password</label>
    <input type="password" class="form-control" id="formtextInput">
    <div id="passwordHelpBlock" class="form-text">
        Must be 8-20 characters long.
    </div>
</div>
```

**Required Classes:**
- Help text wrapper: `form-text`
- Note: `form-text` appears muted gray by default

---

## 5. INPUT WITH PREFIX/SUFFIX (Input Groups)

### Prefix Example (@ symbol):
```html
<div class="input-group">
    <span class="input-group-text" id="basic-addon1">@</span>
    <input type="text" class="form-control" placeholder="Username"
           aria-label="Username" aria-describedby="basic-addon1">
</div>
```

### Suffix Example (@example.com):
```html
<div class="input-group">
    <input type="text" class="form-control" placeholder="Recipient's username"
           aria-label="Recipient's username" aria-describedby="basic-addon2">
    <span class="input-group-text" id="basic-addon2">@example.com</span>
</div>
```

### Currency Example (Both Prefix and Suffix):
```html
<div class="input-group">
    <span class="input-group-text">KES</span>
    <input type="text" class="form-control" aria-label="Amount">
    <span class="input-group-text">.00</span>
</div>
```

### With Label Wrapper:
```html
<div>
    <label for="basic-url" class="form-label">Your vanity URL</label>
    <div class="input-group">
        <span class="input-group-text" id="basic-addon3">https://example.com/users/</span>
        <input type="text" class="form-control" id="basic-url" aria-describedby="basic-addon3">
    </div>
</div>
```

**Required Classes:**
- Container: `input-group`
- Text element: `input-group-text`
- Input: `form-control`
- Sizing variants: `input-group-sm`, `input-group-lg`

---

## 6. TEXTAREA

**HTML Structure:**
```html
<div>
    <label for="exampleFormControlTextarea5" class="form-label">Example Textarea</label>
    <textarea class="form-control" id="exampleFormControlTextarea5" rows="3"></textarea>
</div>
```

**Required Classes:**
- Textarea: `form-control`
- Attribute: `rows="number"`

---

## 7. NUMBER INPUT

**HTML Structure:**
```html
<div>
    <label for="numberInput" class="form-label">Number Input</label>
    <input type="number" class="form-control" id="numberInput">
</div>
```

**Required Classes:**
- Input: `form-control`
- Type: `type="number"`

---

## 8. EMAIL/PHONE/URL INPUTS

### Email Input:
```html
<div>
    <label for="emailInput" class="form-label">Email Address</label>
    <input type="email" class="form-control" id="emailInput" placeholder="example@gmail.com">
</div>
```

### Phone Input:
```html
<div>
    <label for="phoneInput" class="form-label">Phone Number</label>
    <input type="tel" class="form-control" id="phoneInput">
</div>
```

### URL Input:
```html
<div>
    <label for="urlInput" class="form-label">Website URL</label>
    <input type="url" class="form-control" id="urlInput">
</div>
```

### Email with Icon (Form Icon Pattern):
```html
<div class="form-icon">
    <input type="email" class="form-control form-control-icon"
           id="iconInput" placeholder="example@gmail.com">
    <i class="ri-mail-unread-line"></i>
</div>
```

**Required Classes:**
- Input: `form-control`
- With icon: `form-control-icon` + wrapper `form-icon`
- Icon container for right alignment: `form-icon right`

---

## 9. DROPDOWN/SELECT

### Basic Select:
```html
<div>
    <label for="basicSelect" class="form-label">Select Option</label>
    <select class="form-select" id="basicSelect">
        <option selected>Choose...</option>
        <option value="1">Option 1</option>
        <option value="2">Option 2</option>
        <option value="3">Option 3</option>
    </select>
</div>
```

### Multiple Select:
```html
<div>
    <label for="multipleSelect" class="form-label">Select Multiple</label>
    <select class="form-select" id="multipleSelect" multiple>
        <option value="1">Option 1</option>
        <option value="2">Option 2</option>
        <option value="3">Option 3</option>
    </select>
</div>
```

### Size Variants:
```html
<!-- Small -->
<select class="form-select form-select-sm">...</select>

<!-- Large -->
<select class="form-select form-select-lg">...</select>

<!-- Rounded -->
<select class="form-select rounded-pill">...</select>
```

### Disabled Select:
```html
<select class="form-select" disabled>
    <option selected>Choose...</option>
</select>
```

**Required Classes:**
- Select: `form-select`
- Small size: `form-select-sm`
- Large size: `form-select-lg`
- Rounded: `rounded-pill`

---

## 10. RADIO BUTTON GROUP

### Standard Radio Group:
```html
<div>
    <div class="form-check">
        <input class="form-check-input" type="radio" name="radioGroup" id="radio1">
        <label class="form-check-label" for="radio1">
            Option 1
        </label>
    </div>
    <div class="form-check">
        <input class="form-check-input" type="radio" name="radioGroup" id="radio2">
        <label class="form-check-label" for="radio2">
            Option 2
        </label>
    </div>
    <div class="form-check">
        <input class="form-check-input" type="radio" name="radioGroup" id="radio3">
        <label class="form-check-label" for="radio3">
            Option 3
        </label>
    </div>
</div>
```

### Radio Right Alignment:
```html
<div class="form-check form-check-right">
    <input class="form-check-input" type="radio" name="radioGroup" id="radio1">
    <label class="form-check-label" for="radio1">
        Option Right
    </label>
</div>
```

### Colored Radio:
```html
<div class="form-check form-check-primary">
    <input class="form-check-input" type="radio" id="radioPrimary">
    <label class="form-check-label" for="radioPrimary">
        Primary Radio
    </label>
</div>
```

### Color Variants Available:
- `form-check-primary`
- `form-check-secondary`
- `form-check-success`
- `form-check-warning`
- `form-check-danger`
- `form-check-info`
- `form-check-dark`

**Required Classes:**
- Container: `form-check`
- Input: `form-check-input`
- Label: `form-check-label`
- Right align: `form-check-right`
- Color variant: `form-check-[color]`

---

## 11. CHECKBOX

### Single Checkbox:
```html
<div class="form-check">
    <input class="form-check-input" type="checkbox" id="checkbox1">
    <label class="form-check-label" for="checkbox1">
        Default checkbox
    </label>
</div>
```

### Checked Checkbox:
```html
<div class="form-check">
    <input class="form-check-input" type="checkbox" id="checkbox2" checked>
    <label class="form-check-label" for="checkbox2">
        Checked checkbox
    </label>
</div>
```

### Disabled Checkbox:
```html
<div class="form-check">
    <input class="form-check-input" type="checkbox" id="checkboxDisabled" disabled>
    <label class="form-check-label" for="checkboxDisabled">
        Disabled checkbox
    </label>
</div>
```

### Multiple Checkboxes:
```html
<div>
    <div class="form-check mb-2">
        <input class="form-check-input" type="checkbox" id="checkbox1">
        <label class="form-check-label" for="checkbox1">Checkbox 1</label>
    </div>
    <div class="form-check mb-2">
        <input class="form-check-input" type="checkbox" id="checkbox2">
        <label class="form-check-label" for="checkbox2">Checkbox 2</label>
    </div>
    <div class="form-check">
        <input class="form-check-input" type="checkbox" id="checkbox3">
        <label class="form-check-label" for="checkbox3">Checkbox 3</label>
    </div>
</div>
```

### Colored Checkbox:
```html
<div class="form-check form-check-success mb-3">
    <input class="form-check-input" type="checkbox" id="checkSuccess" checked>
    <label class="form-check-label" for="checkSuccess">
        Checkbox Success
    </label>
</div>
```

### Outline Checkbox:
```html
<div class="form-check form-check-outline form-check-primary mb-3">
    <input class="form-check-input" type="checkbox" id="checkOutline" checked>
    <label class="form-check-label" for="checkOutline">
        Checkbox Outline Primary
    </label>
</div>
```

**Required Classes:**
- Container: `form-check`
- Input: `form-check-input`
- Label: `form-check-label`
- Right align: `form-check-right`
- Color variant: `form-check-[color]`
- Outline style: `form-check-outline`
- Spacing: `mb-2`, `mb-3`

---

## 12. DATE PICKER

### Date Input:
```html
<div>
    <label for="exampleInputdate" class="form-label">Input Date</label>
    <input type="date" class="form-control" id="exampleInputdate">
</div>
```

### Time Input:
```html
<div>
    <label for="exampleInputtime" class="form-label">Input Time</label>
    <input type="time" class="form-control" id="exampleInputtime">
</div>
```

### DateTime Input:
```html
<div>
    <label for="exampleInputdatetime" class="form-label">Input DateTime</label>
    <input type="datetime-local" class="form-control" id="exampleInputdatetime">
</div>
```

### Date Range:
```html
<div class="row">
    <div class="col-md-6">
        <label for="startDate" class="form-label">Start Date</label>
        <input type="date" class="form-control" id="startDate">
    </div>
    <div class="col-md-6">
        <label for="endDate" class="form-label">End Date</label>
        <input type="date" class="form-control" id="endDate">
    </div>
</div>
```

**Required Classes:**
- Input: `form-control`
- Type: `type="date"`, `type="time"`, or `type="datetime-local"`

---

## 13. FILE UPLOAD

### Basic File Input:
```html
<div>
    <label for="formFile" class="form-label">Choose file</label>
    <input class="form-control" type="file" id="formFile">
</div>
```

### File Input in Input Group:
```html
<div class="input-group">
    <label class="input-group-text" for="inputGroupFile01">Upload</label>
    <input type="file" class="form-control" id="inputGroupFile01">
</div>
```

### File Input with Button:
```html
<div class="input-group">
    <button class="btn btn-outline-primary material-shadow-none"
            type="button" id="fileBtn">Button</button>
    <input type="file" class="form-control" id="inputGroupFile"
           aria-describedby="fileBtn" aria-label="Upload">
</div>
```

### Multiple Files:
```html
<div>
    <label for="multipleFiles" class="form-label">Choose files</label>
    <input class="form-control" type="file" id="multipleFiles" multiple>
</div>
```

**Required Classes:**
- Input: `form-control`
- In group: Use `input-group` and `input-group-text`
- Type: `type="file"`
- Multiple: `multiple` attribute

---

## 14. VALIDATION ERROR STATE

### Invalid Input (With Error Message):
```html
<div class="col-md-6">
    <label for="validationCustom03" class="form-label">City</label>
    <input type="text" class="form-control is-invalid"
           id="validationCustom03" required>
    <div class="invalid-feedback">
        Please provide a valid city.
    </div>
</div>
```

### Valid Input (With Success Message):
```html
<div class="col-md-4">
    <label for="validationCustom01" class="form-label">First name</label>
    <input type="text" class="form-control is-valid"
           id="validationCustom01" value="Mark" required>
    <div class="valid-feedback">
        Looks good!
    </div>
</div>
```

### Select with Invalid Feedback:
```html
<div class="col-md-3">
    <label for="validationCustom04" class="form-label">State</label>
    <select class="form-select is-invalid" id="validationCustom04" required>
        <option selected disabled value="">Choose...</option>
        <option>...</option>
    </select>
    <div class="invalid-feedback">
        Please select a valid state.
    </div>
</div>
```

### Checkbox with Invalid Feedback:
```html
<div class="col-12">
    <div class="form-check">
        <input class="form-check-input is-invalid" type="checkbox"
               value="" id="invalidCheck" required>
        <label class="form-check-label" for="invalidCheck">
            Agree to terms and conditions
        </label>
        <div class="invalid-feedback">
            You must agree before submitting.
        </div>
    </div>
</div>
```

### Textarea with Invalid Feedback:
```html
<div class="mb-3">
    <label for="validationTextarea" class="form-label">Textarea</label>
    <textarea class="form-control is-invalid" id="validationTextarea"
              placeholder="Required example textarea" required></textarea>
    <div class="invalid-feedback">
        Please enter a message in the textarea.
    </div>
</div>
```

### Input Group with Validation:
```html
<div class="col-md-4">
    <label for="validationCustomUsername" class="form-label">Username</label>
    <div class="input-group has-validation">
        <span class="input-group-text" id="inputGroupPrepend">@</span>
        <input type="text" class="form-control is-invalid"
               id="validationCustomUsername"
               aria-describedby="inputGroupPrepend" required>
        <div class="invalid-feedback">
            Please choose a username.
        </div>
    </div>
</div>
```

**Required Classes:**
- Invalid state: `is-invalid`
- Valid state: `is-valid`
- Error message: `invalid-feedback`
- Success message: `valid-feedback`
- Input group validation: `has-validation` (parent class)
- Form needs validation: `needs-validation` (form attribute)

**Form Attribute:**
```html
<form class="row g-3 needs-validation" novalidate>
    <!-- form content -->
</form>
```

---

## 15. READ-ONLY / DISABLED INPUT

### Read-only Input:
```html
<div>
    <label for="readonlyInput" class="form-label">Readonly Input</label>
    <input type="text" class="form-control" id="readonlyInput"
           value="Readonly input" readonly>
</div>
```

### Read-only Plaintext:
```html
<div>
    <label for="readonlyPlaintext" class="form-label">Readonly Plain Text Input</label>
    <input type="text" class="form-control-plaintext" id="readonlyPlaintext"
           value="Readonly input" readonly>
</div>
```

### Disabled Input:
```html
<div>
    <label for="disabledInput" class="form-label">Disabled Input</label>
    <input type="text" class="form-control" id="disabledInput"
           value="Disabled input" disabled>
</div>
```

### Disabled Select:
```html
<select class="form-select" disabled>
    <option selected>Choose...</option>
</select>
```

### Disabled Checkbox:
```html
<div class="form-check">
    <input class="form-check-input" type="checkbox" id="disabledCheck" disabled>
    <label class="form-check-label" for="disabledCheck">
        Disabled checkbox
    </label>
</div>
```

**Required Classes:**
- Readonly attribute: `readonly`
- Disabled attribute: `disabled`
- Plaintext style: `form-control-plaintext`

---

## 16. FORM SECTION/CARD

### Full Form Card Wrapper:
```html
<div class="row">
    <div class="col-lg-12">
        <div class="card">
            <div class="card-header align-items-center d-flex">
                <h4 class="card-title mb-0 flex-grow-1">Form Title</h4>
                <div class="flex-shrink-0">
                    <div class="form-check form-switch form-switch-right form-switch-md">
                        <label for="showcode" class="form-label text-muted">Show Code</label>
                        <input class="form-check-input code-switcher"
                               type="checkbox" id="showcode">
                    </div>
                </div>
            </div><!-- end card header -->
            <div class="card-body">
                <!-- Form content here -->
                <form>
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label for="firstName" class="form-label">First name</label>
                            <input type="text" class="form-control" id="firstName">
                        </div>
                        <div class="col-md-6">
                            <label for="lastName" class="form-label">Last name</label>
                            <input type="text" class="form-control" id="lastName">
                        </div>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>
```

### Minimal Form Card:
```html
<div class="card">
    <div class="card-header">
        <h5 class="card-title mb-0">Form Section</h5>
    </div>
    <div class="card-body">
        <!-- Form fields -->
    </div>
</div>
```

### Collapsible Form Section:
```html
<div class="card">
    <div class="card-header" id="headingOne">
        <h5 class="mb-0">
            <button class="btn btn-link w-100 text-start d-flex justify-content-between align-items-center"
                    type="button"
                    data-bs-toggle="collapse"
                    data-bs-target="#collapseOne">
                <span>
                    <i class="ri-user-line me-2"></i>
                    Personal Information
                </span>
                <i class="ri-arrow-down-s-line"></i>
            </button>
        </h5>
    </div>
    <div id="collapseOne" class="collapse show" aria-labelledby="headingOne">
        <div class="card-body">
            <!-- Form fields -->
        </div>
    </div>
</div>
```

**Required Classes:**
- Card container: `card`
- Card header: `card-header`
- Card title: `card-title`
- Card body: `card-body`
- Header alignment: `align-items-center d-flex`
- Title grow: `flex-grow-1`
- Responsive grid: `row g-3`
- Column sizes: `col-md-6`, `col-lg-12`, etc.
- Collapse: `collapse`, `show` (for default expanded state)

---

## SUMMARY: Key Velzon Classes & Patterns

### Core Classes Used Across All Components:

| Component | Primary Classes |
|-----------|-----------------|
| Text Input | `form-control` |
| Textarea | `form-control` |
| Select | `form-select` |
| Checkbox | `form-check`, `form-check-input`, `form-check-label` |
| Radio | `form-check`, `form-check-input`, `form-check-label` |
| Label | `form-label` |
| Input Group | `input-group`, `input-group-text` |
| Help Text | `form-text` |
| Validation | `is-valid`, `is-invalid`, `valid-feedback`, `invalid-feedback` |
| Card | `card`, `card-header`, `card-body`, `card-title` |

### Sizing Classes:

```css
input-group-sm          /* Small input group */
input-group-lg          /* Large input group */
form-select-sm          /* Small select */
form-select-lg          /* Large select */
```

### Color Variants (for Checkboxes & Radios):

```css
form-check-primary
form-check-secondary
form-check-success
form-check-warning
form-check-danger
form-check-info
form-check-dark
form-check-outline      /* With outline style */
```

### Border Styles:

```css
rounded-pill            /* Fully rounded inputs */
border-dashed           /* Dashed borders */
```

### Spacing:

```css
mb-2, mb-3              /* Margin bottom for spacing */
g-3                     /* Gap between grid columns */
```

### Validation States:

```css
is-valid                /* Apply to input/select for valid state */
is-invalid              /* Apply to input/select for invalid state */
valid-feedback          /* Success message container */
invalid-feedback        /* Error message container */
has-validation          /* Apply to input-group for validation support */
needs-validation        /* Apply to form element */
```

### Read-only/Disabled:

```css
readonly                /* Attribute for read-only inputs */
disabled                /* Attribute for disabled inputs */
form-control-plaintext  /* Class for plaintext readonly style */
```

---

## Usage in Razor Partial Views

### Example: Text Field Partial

```cshtml
@model FormFieldViewModel

<div class="mb-3">
    <label for="@Model.InputId" class="form-label">
        @Model.FieldName
        @if (Model.IsRequired)
        {
            <span class="text-danger">*</span>
        }
    </label>

    @if (!string.IsNullOrEmpty(Model.FieldDescription))
    {
        <small class="text-muted d-block mb-1">@Model.FieldDescription</small>
    }

    <input type="text"
           class="form-control @(ViewData.ModelState["field_" + Model.FieldId]?.Errors.Any() == true ? "is-invalid" : "")"
           id="@Model.InputId"
           name="@Model.InputName"
           placeholder="@Model.PlaceholderText"
           value="@Model.CurrentValue"
           @(Model.IsRequired ? "required" : "")
           @(Model.IsReadOnly ? "readonly" : "")
           @(Model.IsDisabled ? "disabled" : "")
           data-validations='@Model.ValidationDataAttribute' />

    @if (!string.IsNullOrEmpty(Model.HelpText))
    {
        <div class="form-text">@Model.HelpText</div>
    }

    <div class="invalid-feedback"></div>
</div>
```

---

**Related Documentation:**
- `2D_FormBuilder_UILogic.md` - Form builder and rendering architecture
- `2B_FormBuilder_Structure.md` - Database schema

**Last Updated:** 2025-11-07
