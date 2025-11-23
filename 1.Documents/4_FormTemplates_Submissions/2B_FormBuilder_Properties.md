# FormBuilder Properties Panel - Complete Specification

## 📋 Overview

The Properties Panel is a **context-aware 4-tab interface** on the right side of the FormBuilder that dynamically changes content based on what's selected (Section or Field).

---

## 🎯 Architecture

### Three-Panel Layout
```
┌──────────────┬────────────────────┬─────────────────┐
│   TOOLBOX    │      CANVAS        │   PROPERTIES    │
│   (250px)    │    (Flexible)      │    (320px)      │
│              │                    │                 │
│  Add items   │  Visual builder    │  Edit selected  │
│              │                    │                 │
└──────────────┴────────────────────┴─────────────────┘
```

### Properties Panel States

1. **Empty State** - Nothing selected
2. **Section Mode** - Section selected
3. **Field Mode** - Field selected (content varies by field type)

---

## 📑 Four Tabs Structure

### Tabs Always Visible
```
[General] [Config] [Validation] [Advanced]
```

**Tab Visibility Rules:**
- **Section selected**: General, Config, Advanced (Validation hidden)
- **Field selected**: All 4 tabs visible
- **Nothing selected**: Empty state message

---

## 1️⃣ SECTION PROPERTIES

### Tab 1: GENERAL (Section Basics)

**Purpose:** Core section identification and display settings

**Fields:**
```
┌─────────────────────────────────────┐
│  📄 Section Properties              │
├─────────────────────────────────────┤
│  Section Name *                     │
│  [Infrastructure and Systems      ] │
│                                     │
│  Description                        │
│  [Fill information about...       ] │
│  [                                ] │
│                                     │
│  Icon (Optional)                    │
│  [ri-building-line                ] │
│  → Browse Remix Icons               │
│                                     │
│  Display Order                      │
│  [1                               ] │
│  (Auto-managed, read-only)          │
│                                     │
│  ☑ Collapsible                     │
│  ☐ Start collapsed by default      │
│                                     │
│  [💾 Save Changes]                 │
└─────────────────────────────────────┘
```

**Database Mapping:**
- `SectionName` → FormTemplateSection.SectionName (required)
- `SectionDescription` → FormTemplateSection.SectionDescription (nullable)
- `IconClass` → FormTemplateSection.IconClass (nullable)
- `DisplayOrder` → FormTemplateSection.DisplayOrder (int)
- `IsCollapsible` → FormTemplateSection.IsCollapsible (bool)
- `IsCollapsedByDefault` → FormTemplateSection.IsCollapsedByDefault (bool)

**Validation:**
- Section Name: Required, Max 100 chars
- Description: Optional, Max 500 chars
- Icon: Optional, Max 50 chars (Remix icon class)

**API Endpoint:**
```
PUT /api/formbuilder/sections/{sectionId}
Body: {
    sectionName: string,
    sectionDescription: string?,
    iconClass: string?,
    isCollapsible: bool,
    isCollapsedByDefault: bool
}
```

---

### Tab 2: CONFIGURATION (Section Layout)

**Purpose:** Visual layout and display settings

**Fields:**
```
┌─────────────────────────────────────┐
│  ⚙️ Section Layout                  │
├─────────────────────────────────────┤
│  Column Layout                      │
│  ○ Single Column                    │
│  ● Two Columns                      │
│  ○ Three Columns                    │
│                                     │
│  Section Width                      │
│  ● Full Width (100%)               │
│  ○ Centered (80%)                   │
│  ○ Narrow (60%)                     │
│                                     │
│  Background Style                   │
│  ● Transparent (default)            │
│  ○ Light Gray                       │
│  ○ White Card                       │
│                                     │
│  Show Section Number                │
│  ☑ Display "Section 1, Section 2"  │
│                                     │
│  Spacing                            │
│  Top Padding:    [Medium ▾]        │
│  Bottom Padding: [Medium ▾]        │
│                                     │
└─────────────────────────────────────┘
```

**Future Enhancement:** Store in FormItemConfiguration table as key-value pairs
- Key: "columnLayout", Value: "2"
- Key: "sectionWidth", Value: "100"
- Key: "backgroundStyle", Value: "transparent"

---

### Tab 3: VALIDATION (Hidden for Sections)

**Status:** Not applicable for sections, tab hidden when section selected

---

### Tab 4: ADVANCED (Section Logic)

**Purpose:** Conditional display, routing, and permissions

**Fields:**
```
┌─────────────────────────────────────┐
│  🔧 Advanced Settings               │
├─────────────────────────────────────┤
│                                     │
│  Conditional Display                │
│  ● Always visible                   │
│  ○ Show only if conditions met     │
│                                     │
│    ┌─ Condition 1 ────────[✕]      │
│    │  Field: [Select ▾]            │
│    │  Operator: [Equals ▾]         │
│    │  Value: [______]              │
│    └───────────────────────────     │
│                                     │
│    [+ Add Condition]                │
│    Logic: [AND ▾] [OR]             │
│                                     │
│  Section Routing                    │
│  After completion, navigate to:     │
│  ● Next section (default)           │
│  ○ Specific section [Select ▾]     │
│  ○ Skip to summary                  │
│                                     │
│  Permissions (Future)               │
│  Who can edit this section?         │
│  ☑ Form creator                     │
│  ☐ Specific role [Select ▾]        │
│                                     │
└─────────────────────────────────────┘
```

**Database Mapping:**
- Store conditional logic as JSON in `ConditionalLogic` column
- Store routing in `SectionRouting` table (future)

---

## 2️⃣ FIELD PROPERTIES

### Tab 1: GENERAL (Common to All Fields)

**Purpose:** Basic field identification and display

**Fields (All Types):**
```
┌─────────────────────────────────────┐
│  📝 Field Properties                │
│  Type: [Text Input]                 │
├─────────────────────────────────────┤
│  Field Name *                       │
│  [Employee Name                   ] │
│                                     │
│  Field Code (auto)                  │
│  [SEC1_001                        ] │
│  ✓ Auto-generated                   │
│                                     │
│  Description                        │
│  [Enter employee full name        ] │
│                                     │
│  Placeholder Text                   │
│  [e.g., John Doe                  ] │
│                                     │
│  Help Text                          │
│  [First and last name required    ] │
│                                     │
│  ☑ Required Field                  │
│  ☐ Read Only                        │
│  ☐ Disabled                         │
│                                     │
│  Default Value                      │
│  [________________________]         │
│                                     │
│  Display Order                      │
│  [1                               ] │
│                                     │
│  [💾 Save Changes]                 │
└─────────────────────────────────────┘
```

**Database Mapping:**
- `ItemName` → FormTemplateItem.ItemName (required)
- `ItemCode` → FormTemplateItem.ItemCode (auto-generated)
- `ItemDescription` → FormTemplateItem.ItemDescription (nullable)
- `PlaceholderText` → FormTemplateItem.PlaceholderText (nullable)
- `HelpText` → FormTemplateItem.HelpText (nullable)
- `IsRequired` → FormTemplateItem.IsRequired (bool)
- `DefaultValue` → FormTemplateItem.DefaultValue (nullable)
- `DisplayOrder` → FormTemplateItem.DisplayOrder (int)

---

### Tab 2: CONFIGURATION (Type-Specific)

**Purpose:** Settings unique to each field type

#### TEXT Field
```
┌─────────────────────────────────────┐
│  ⚙️ Text Input Settings             │
├─────────────────────────────────────┤
│  Input Mask                         │
│  ○ None                             │
│  ○ Phone: (999) 999-9999           │
│  ○ Custom: [_____________]         │
│                                     │
│  Character Limits                   │
│  Min Length: [0  ]                 │
│  Max Length: [100]                 │
│                                     │
│  Prefix / Suffix                    │
│  Prefix:  [$     ]                 │
│  Suffix:  [  USD ]                 │
│                                     │
│  Text Transform                     │
│  ○ None  ● Uppercase               │
│  ○ Lowercase  ○ Capitalize         │
└─────────────────────────────────────┘
```

**Database:** FormItemConfiguration (key-value pairs)

#### NUMBER Field
```
┌─────────────────────────────────────┐
│  ⚙️ Number Input Settings           │
├─────────────────────────────────────┤
│  Number Range                       │
│  Min Value: [0     ]               │
│  Max Value: [10000 ]               │
│                                     │
│  Step Increment                     │
│  Step: [1  ] (e.g., 0.01, 5, 10)   │
│                                     │
│  Display Format                     │
│  ● Plain number                     │
│  ○ Currency ($1,234.56)            │
│  ○ Percentage (50%)                │
│  ○ Decimal (2 places)              │
│                                     │
│  Decimal Places                     │
│  Precision: [2 ▾]                  │
│                                     │
│  Prefix / Suffix                    │
│  Prefix:  [$     ]                 │
│  Suffix:  [  USD ]                 │
└─────────────────────────────────────┘
```

#### DATE Field
```
┌─────────────────────────────────────┐
│  ⚙️ Date Picker Settings            │
├─────────────────────────────────────┤
│  Date Format                        │
│  ● MM/DD/YYYY  ○ DD/MM/YYYY        │
│  ○ YYYY-MM-DD  ○ Custom            │
│                                     │
│  Date Range                         │
│  Min Date: [01/01/2000]            │
│  Max Date: [12/31/2030]            │
│                                     │
│  Default Date                       │
│  ○ None  ● Today  ○ Specific       │
│                                     │
│  ☐ Disable weekends                │
│  ☐ Disable past dates              │
│  ☐ Disable future dates            │
└─────────────────────────────────────┘
```

#### DROPDOWN/RADIO/CHECKBOX Field
```
┌─────────────────────────────────────┐
│  ⚙️ Selection Settings              │
├─────────────────────────────────────┤
│  ☐ Allow multiple selections       │
│  ☐ Allow search/filter             │
│  ☐ Allow custom values             │
│                                     │
│  Placeholder                        │
│  [Select an option...             ] │
│                                     │
│  Maximum Selections                 │
│  [Unlimited ▾]                     │
│                                     │
│  → Options managed in ADVANCED tab  │
└─────────────────────────────────────┘
```

#### FILE UPLOAD Field
```
┌─────────────────────────────────────┐
│  ⚙️ File Upload Settings            │
├─────────────────────────────────────┤
│  Allowed File Types                 │
│  ☑ Images (.jpg, .png, .gif)       │
│  ☑ Documents (.pdf, .docx)         │
│  ☐ Spreadsheets (.xlsx, .csv)      │
│  ☐ All files                        │
│                                     │
│  Custom Types                       │
│  [.zip, .rar                      ] │
│                                     │
│  File Size Limit                    │
│  Max: [5   ▾] MB                   │
│                                     │
│  Multiple Files                     │
│  ☑ Allow multiple uploads          │
│  Max files: [3  ]                  │
│                                     │
│  Upload Method                      │
│  ● Immediate  ○ On submit          │
└─────────────────────────────────────┘
```

#### RATING Field
```
┌─────────────────────────────────────┐
│  ⚙️ Rating Settings                 │
├─────────────────────────────────────┤
│  Rating Type                        │
│  ● Stars ⭐  ○ Hearts ❤️           │
│  ○ Thumbs 👍  ○ Custom             │
│                                     │
│  Number of Options                  │
│  [5  ▾] (1-10)                     │
│                                     │
│  Labels                             │
│  Min: [Poor      ]                 │
│  Max: [Excellent ]                 │
│                                     │
│  ☐ Allow half ratings (4.5)        │
│  ☐ Show numeric value              │
└─────────────────────────────────────┘
```

---

### Tab 3: VALIDATION (All Fields)

**Purpose:** Add multiple validation rules per field

**Interface:**
```
┌─────────────────────────────────────┐
│  ✓ Validation Rules                 │
│  [+ Add Validation Rule]            │
├─────────────────────────────────────┤
│  ┌─ Rule 1 ─────────────────[✕]    │
│  │  Type: [Required ▾]             │
│  │  Error: [This field is required]│
│  └─────────────────────────────────│
│                                     │
│  ┌─ Rule 2 ─────────────────[✕]    │
│  │  Type: [Min Length ▾]           │
│  │  Value: [3  ]                   │
│  │  Error: [Min 3 characters     ] │
│  └─────────────────────────────────│
│                                     │
│  ┌─ Rule 3 ─────────────────[✕]    │
│  │  Type: [Regex Pattern ▾]        │
│  │  Pattern: [^[A-Z][a-z]+$      ] │
│  │  Error: [Must start with cap  ] │
│  └─────────────────────────────────│
└─────────────────────────────────────┘
```

**Available Validation Types:**
- Required
- Min Length / Max Length
- Min Value / Max Value
- Email Format
- Phone Format
- URL Format
- Regex Pattern
- Date Range
- Custom JavaScript

**Database:** FormItemValidation table
- ValidationType (string)
- ValidationValue (string - stores min/max/pattern)
- ErrorMessage (string)
- ValidationOrder (int)
- IsActive (bool)

---

### Tab 4: ADVANCED (Context-Dependent)

**Purpose:** Options, conditional logic, calculations

#### For DROPDOWN/RADIO/CHECKBOX (Options Manager)
```
┌─────────────────────────────────────┐
│  🔧 Options Management              │
│  [+ Add] [Import CSV] [Reorder]     │
├─────────────────────────────────────┤
│  ┌─ Option 1 ──────────────[↑↓][✕] │
│  │  Label:  [Small     ]           │
│  │  Value:  [S         ]           │
│  │  ☐ Default                      │
│  └─────────────────────────────────│
│                                     │
│  ┌─ Option 2 ──────────────[↑↓][✕] │
│  │  Label:  [Medium    ]           │
│  │  Value:  [M         ]           │
│  │  ☑ Default                      │
│  └─────────────────────────────────│
│                                     │
│  ☐ Allow "Other" with text input   │
└─────────────────────────────────────┘
```

**Database:** FormItemOptions table
- OptionLabel (string)
- OptionValue (string)
- DisplayOrder (int)
- IsDefault (bool)
- ParentOptionId (int, for cascading)

#### For ALL FIELDS (Conditional Logic)
```
┌─────────────────────────────────────┐
│  🔧 Conditional Logic               │
├─────────────────────────────────────┤
│  Field Visibility                   │
│  ● Always visible                   │
│  ○ Show only if:                    │
│                                     │
│    ┌─ Condition 1 ────────[✕]      │
│    │  Field: [Country ▾]           │
│    │  Operator: [Equals ▾]         │
│    │  Value: [USA]                 │
│    └───────────────────────────     │
│                                     │
│    Logic: [AND ▾] [OR]             │
│                                     │
│    [+ Add Condition]                │
│                                     │
│  Actions when met:                  │
│  ☑ Show field                       │
│  ☐ Make required                    │
│  ☐ Set default value               │
└─────────────────────────────────────┘
```

**Database:** Store as JSON in FormTemplateItem.ConditionalLogic
```json
{
  "action": "show",
  "logicType": "AND",
  "rules": [
    {
      "itemId": 45,
      "operator": "equals",
      "value": "Yes"
    }
  ]
}
```

#### For NUMBER FIELDS (Calculations)
```
┌─────────────────────────────────────┐
│  🔧 Calculations                    │
├─────────────────────────────────────┤
│  Calculate from:                    │
│  ○ Manual entry                     │
│  ● Formula:                         │
│                                     │
│    [Field1] [+▾] [Field2]          │
│                                     │
│  Example: Price * Quantity          │
│                                     │
│  Available fields:                  │
│  • SEC1_001 (Price)                │
│  • SEC1_002 (Quantity)             │
│                                     │
│  ☑ Auto-update on change           │
│  ☑ Allow manual override           │
└─────────────────────────────────────┘
```

**Database:** FormItemCalculation table (future)

---

## 🔄 Dynamic Behavior

### Content Switching Logic

```javascript
function loadProperties(elementType, elementId) {
    if (elementType === 'section') {
        showSectionProperties(elementId);
    } else if (elementType === 'field') {
        const fieldType = getFieldType(elementId);
        showFieldProperties(elementId, fieldType);
    } else {
        showEmptyState();
    }
}

function showSectionProperties(sectionId) {
    // Show: General, Config, Advanced
    // Hide: Validation
    document.querySelector('[href="#prop-validation"]').parentElement.style.display = 'none';

    loadSectionGeneral(sectionId);
    loadSectionConfig(sectionId);
    loadSectionAdvanced(sectionId);
}

function showFieldProperties(fieldId, fieldType) {
    // Show: All tabs
    document.querySelector('[href="#prop-validation"]').parentElement.style.display = 'block';

    loadFieldGeneral(fieldId);
    loadFieldConfig(fieldId, fieldType);  // Dynamic based on type
    loadFieldValidation(fieldId);
    loadFieldAdvanced(fieldId, fieldType); // Dynamic based on type
}
```

---

## 📊 API Endpoints Summary

### Section Endpoints
```
GET    /api/formbuilder/sections/{id}           - Get section details
PUT    /api/formbuilder/sections/{id}           - Update section
DELETE /api/formbuilder/sections/{id}           - Delete section
POST   /api/formbuilder/sections/{id}/duplicate - Duplicate section
```

### Field Endpoints (Future Phase 2)
```
GET    /api/formbuilder/fields/{id}             - Get field details
PUT    /api/formbuilder/fields/{id}             - Update field
DELETE /api/formbuilder/fields/{id}             - Delete field
POST   /api/formbuilder/fields/{id}/duplicate   - Duplicate field

GET    /api/formbuilder/fields/{id}/validations - Get validations
POST   /api/formbuilder/fields/{id}/validations - Add validation
DELETE /api/formbuilder/validations/{id}        - Delete validation

GET    /api/formbuilder/fields/{id}/options     - Get options
POST   /api/formbuilder/fields/{id}/options     - Add option
PUT    /api/formbuilder/options/{id}            - Update option
DELETE /api/formbuilder/options/{id}            - Delete option
```

---

## 🎨 UX Patterns

### Save Strategies

**Option 1: Auto-Save (Recommended)**
- Save each field on blur
- Show "Saving..." indicator
- Update canvas in real-time
- No page reload needed

**Option 2: Manual Save**
- Save button at bottom of each tab
- Batch update all changes
- Show success toast
- Update canvas after save

**Option 3: Hybrid**
- Auto-save General tab (frequent changes)
- Manual save for Advanced tab (complex changes)

### Visual Feedback

**Saving State:**
```
[Field Name: ___________] (typing)
[Field Name: ___________] 💾 Saving...
[Field Name: ___________] ✓ Saved
```

**Validation Errors:**
```
[Field Name: ]
⚠️ Field name is required
```

**Success Messages:**
```
✓ Section updated successfully
```

---

## 📁 File Structure

```
Views/Forms/FormTemplates/Partials/FormBuilder/
├── _PropertiesPanel.cshtml                    (Main container + tabs)
├── Properties/
│   ├── _PropertiesGeneral.cshtml             (Tab 1)
│   ├── _PropertiesConfiguration.cshtml       (Tab 2)
│   ├── _PropertiesValidation.cshtml          (Tab 3)
│   └── _PropertiesAdvanced.cshtml            (Tab 4)

wwwroot/assets/js/pages/
├── form-builder-properties.js                 (Main logic)
└── form-builder-properties-config.js          (Type-specific configs)
```

---

## ✅ Implementation Priority

### Phase 1 (Current)
- ✅ Section General tab
- ⬜ Section Advanced tab (basic)

### Phase 2 (Next)
- ⬜ Field General tab
- ⬜ Field Configuration tab (Text, Number, Date)
- ⬜ Field Validation tab

### Phase 3 (Advanced)
- ⬜ Field Configuration (Dropdown, File, Rating)
- ⬜ Field Advanced tab (Options manager)
- ⬜ Conditional Logic builder
- ⬜ Calculations builder

---

## 🔗 Related Documents

- `2B_FormBuilder_Plan.md` - Overall implementation plan
- `2B_FormBuilder_Structure.md` - Database schema
- `2B_FormBuilder_AddField.md` - Field drag-drop implementation

---

**Document Version:** 1.0
**Last Updated:** 2024-01-23
**Author:** FormBuilder Team
