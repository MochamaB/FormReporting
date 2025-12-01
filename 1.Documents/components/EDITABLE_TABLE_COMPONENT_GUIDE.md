# Editable Table Component Guide

## Overview

The **Editable Table Component** is a reusable data grid for bulk data entry and editing across the application. It follows the three-layer architecture pattern and provides a clean, borderless design with advanced features like multi-select, sorting, and dynamic row management.

---

## Architecture

### Three-Layer Pattern

```
Configuration Layer (What to display)
    ↓
Extension Layer (How to transform)
    ↓
View Layer (How to render)
```

### File Structure

```
Models/ViewModels/Components/
└── EditableTableComponents.cs          # Config & ViewModel classes

Extensions/
└── EditableTableExtensions.cs          # Transformation logic

Views/Shared/Components/DataTable/
├── _EditableTable.cshtml               # Main wrapper
└── _EditableTableContent.cshtml        # Table content & JavaScript
```

---

## Features

✅ **Add/Remove Rows** - Dynamic row management  
✅ **Multi-Select** - Bulk row selection with delete  
✅ **Column Sorting** - Click headers to sort  
✅ **Auto-Reindexing** - Proper model binding after changes  
✅ **Row Numbers** - Auto-updating row numbers  
✅ **Multiple Input Types** - Text, number, select, color, date, etc.  
✅ **Validation** - Required fields, patterns, min/max  
✅ **Borderless Design** - Clean row-separated layout  
✅ **Conditional Columns** - Show/hide based on conditions  
✅ **Empty State** - Custom message when no data  

---

## Usage Example

### Step 1: Controller Setup

```csharp
using FormReporting.Models.ViewModels.Components;
using FormReporting.Extensions;

public async Task<IActionResult> Create()
{
    var model = new OptionTemplateEditViewModel
    {
        // ... your model properties ...
        Items = new List<OptionTemplateItemEditViewModel>()
    };
    
    // Configure editable table
    var tableConfig = new EditableTableConfig
    {
        TableId = "option-items-table",
        ItemIndexPropertyName = "Items",
        AllowMultiSelect = true,
        AllowSorting = true,
        ShowRowNumbers = true,
        EmptyMessage = "No options added yet. Click 'Add Option' to start.",
        InitialRowCount = model.Items.Count,
        Buttons = new EditableTableButtons
        {
            AddButtonText = "Add Option",
            AddButtonIcon = "ri-add-line",
            ShowClearAllButton = true,
            ShowDeleteSelectedButton = true
        },
        Columns = new List<EditableTableColumn>
        {
            new EditableTableColumn
            {
                PropertyName = "OptionLabel",
                Header = "Option Label",
                ColumnType = EditableColumnType.Text,
                IsRequired = true,
                Placeholder = "e.g., Very Satisfied",
                Width = "250px",
                DisplayOrder = 1
            },
            new EditableTableColumn
            {
                PropertyName = "OptionValue",
                Header = "Option Value",
                ColumnType = EditableColumnType.Text,
                IsRequired = true,
                Placeholder = "e.g., very_satisfied",
                Width = "200px",
                DisplayOrder = 2
            },
            new EditableTableColumn
            {
                PropertyName = "DisplayOrder",
                Header = "Display Order",
                ColumnType = EditableColumnType.Number,
                Width = "100px",
                Min = 0,
                DisplayOrder = 3
            },
            new EditableTableColumn
            {
                PropertyName = "ScoreValue",
                Header = "Score",
                ColumnType = EditableColumnType.Decimal,
                Width = "120px",
                Step = 0.01m,
                Placeholder = "0.00",
                ConditionalDisplay = "HasScoring",
                DisplayOrder = 4
            },
            new EditableTableColumn
            {
                PropertyName = "ColorHint",
                Header = "Color",
                ColumnType = EditableColumnType.Color,
                Width = "150px",
                DefaultValue = "#6c757d",
                DisplayOrder = 5
            },
            new EditableTableColumn
            {
                PropertyName = "IsDefault",
                Header = "Default",
                ColumnType = EditableColumnType.Checkbox,
                Width = "80px",
                DisplayOrder = 6
            }
        }
    }.BuildEditableTable();
    
    ViewData["ItemsTable"] = tableConfig;
    
    return View(model);
}
```

### Step 2: View Implementation

```cshtml
@model YourViewModel

@{
    var itemsTable = ViewData["ItemsTable"] as EditableTableViewModel;
}

<div class="row">
    <div class="col-12">
        <partial name="~/Views/Shared/Components/DataTable/_EditableTable.cshtml" 
                 model="itemsTable" />
    </div>
</div>
```

---

## Column Types

### Supported Input Types

| Type | Description | Additional Properties |
|------|-------------|----------------------|
| `Text` | Standard text input | MaxLength, Pattern |
| `Number` | Integer input | Min, Max, Step |
| `Decimal` | Decimal input | Min, Max, Step |
| `Date` | Date picker | Min, Max |
| `Email` | Email validation | Pattern |
| `Url` | URL validation | Pattern |
| `Checkbox` | Boolean checkbox | DefaultValue |
| `Select` | Dropdown list | SelectOptions |
| `Color` | Color picker | DefaultValue |
| `TextArea` | Multi-line text | MaxLength, Rows |
| `Hidden` | Hidden field | DefaultValue |

---

## Configuration Options

### EditableTableConfig

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TableId` | string | Auto-generated | Unique table identifier |
| `ItemIndexPropertyName` | string | "Items" | Property name for model binding |
| `Columns` | List\<Column\> | Required | Column definitions |
| `Buttons` | Buttons | Default | Button configuration |
| `AllowReorder` | bool | false | Enable drag-and-drop |
| `ShowRowNumbers` | bool | true | Show row numbers |
| `AllowMultiSelect` | bool | false | Enable multi-select |
| `AllowSorting` | bool | false | Enable column sorting |
| `EmptyMessage` | string | "No items" | Empty state message |
| `InitialRowCount` | int | 0 | Existing rows count |
| `WrapInCard` | bool | false | Wrap in card component |
| `CardTitle` | string | null | Card header title |

### EditableTableColumn

| Property | Type | Description |
|----------|------|-------------|
| `PropertyName` | string | Property for model binding |
| `Header` | string | Column header text |
| `ColumnType` | Enum | Input type |
| `Width` | string | Column width (e.g., "200px") |
| `IsRequired` | bool | Required validation |
| `Placeholder` | string | Input placeholder |
| `DefaultValue` | object | Default value for new rows |
| `SelectOptions` | List\<Option\> | Options for dropdowns |
| `Min` | decimal? | Minimum value |
| `Max` | decimal? | Maximum value |
| `Step` | decimal? | Increment step |
| `MaxLength` | int? | Maximum character length |
| `Pattern` | string | HTML5 pattern validation |
| `Sortable` | bool | Allow sorting on column |
| `ConditionalDisplay` | string | Show/hide condition |
| `HelpText` | string | Helper text |

---

## Fluent API

Chain methods for cleaner configuration:

```csharp
var tableConfig = new EditableTableConfig
{
    // ... columns ...
}
.WithMultiSelect()           // Enable multi-select
.WithSorting()               // Enable sorting
.WithReordering()            // Enable drag-and-drop
.InCard("Option Items")      // Wrap in card with title
.WithEmptyMessage("No items")
.BuildEditableTable();
```

---

## Design Features

### Borderless Table Design

The table uses a clean, borderless design with:
- **No internal borders** - Only row separators
- **Row hover effect** - Subtle background change
- **Selected row highlight** - Blue background for selected rows
- **Clean spacing** - Proper padding for readability

### Styling Classes

```css
/* Automatically applied */
.table-borderless      /* Remove all borders */
.border-bottom         /* Row separator */
.table-sm              /* Compact sizing */
.sortable              /* Sortable column cursor */
.sort-asc / .sort-desc /* Sort direction indicators */
```

---

## JavaScript API

The component automatically initializes on page load. No manual initialization required.

### Events

```javascript
// Access the table instance (auto-generated ID)
const tableId = 'your-table-id';
const tbody = document.getElementById(tableId + '-tbody');

// Listen for row changes
tbody.addEventListener('DOMSubtreeModified', function() {
    console.log('Table rows changed');
});
```

---

## Model Binding

The component automatically handles ASP.NET Core model binding:

```csharp
[HttpPost]
public async Task<IActionResult> Create(OptionTemplateEditViewModel model)
{
    // model.Items will be populated with all rows
    // Indices are automatically handled: Items[0], Items[1], etc.
    
    foreach (var item in model.Items)
    {
        // Process each row
    }
}
```

---

## Validation

### Server-Side

Use Data Annotations in your ViewModel:

```csharp
public class ItemViewModel
{
    [Required(ErrorMessage = "Label is required")]
    [StringLength(200)]
    public string OptionLabel { get; set; }
    
    [Required]
    [RegularExpression(@"^[a-z_]+$")]
    public string OptionValue { get; set; }
}
```

### Client-Side

Add validation attributes to columns:

```csharp
new EditableTableColumn
{
    PropertyName = "OptionValue",
    IsRequired = true,
    Pattern = "^[a-z_]+$",  // HTML5 pattern
    MaxLength = 200
}
```

---

## Advanced Examples

### Conditional Columns

Show columns based on other field values:

```csharp
new EditableTableColumn
{
    PropertyName = "ScoreValue",
    Header = "Score",
    ConditionalDisplay = "HasScoring",  // JavaScript expression
    // Column shows only when HasScoring checkbox is checked
}
```

### Dropdown Columns

```csharp
new EditableTableColumn
{
    PropertyName = "Category",
    Header = "Category",
    ColumnType = EditableColumnType.Select,
    SelectOptions = new List<EditableTableSelectOption>
    {
        new() { Value = "rating", Text = "Rating" },
        new() { Value = "agreement", Text = "Agreement" },
        new() { Value = "binary", Text = "Binary" }
    }
}
```

### With Card Wrapper

```csharp
var tableConfig = new EditableTableConfig
{
    WrapInCard = true,
    CardTitle = "Template Options",
    // ... rest of config
}.BuildEditableTable();
```

---

## Troubleshooting

### Issue: Rows not binding to model

**Solution:** Ensure `ItemIndexPropertyName` matches your ViewModel property name exactly.

```csharp
// ViewModel
public List<ItemViewModel> Items { get; set; }

// Config
ItemIndexPropertyName = "Items"  // Must match!
```

### Issue: Validation not working

**Solution:** 
1. Check Data Annotations on ViewModel
2. Verify `IsRequired` and `Pattern` on column config
3. Ensure form has `method="POST"` and proper action

### Issue: JavaScript not initializing

**Solution:**
1. Check browser console for errors
2. Verify `data-editable-table` attribute exists
3. Ensure jQuery/Bootstrap scripts loaded (if using)

---

## Best Practices

1. **Always initialize Items collection** in controller (avoid null)
2. **Use meaningful PropertyName** values for clarity
3. **Set appropriate widths** to prevent layout issues
4. **Provide helpful placeholders** for better UX
5. **Use validation** for data integrity
6. **Test with empty state** to ensure proper messaging
7. **Consider mobile responsiveness** with table-responsive wrapper

---

## Future Enhancements

- ⏳ Drag-and-drop row reordering
- ⏳ Inline editing mode (edit existing rows)
- ⏳ Export to CSV/Excel
- ⏳ Import from CSV
- ⏳ Column visibility toggle
- ⏳ Advanced filtering
- ⏳ Pagination for large datasets

---

## Support

For issues or feature requests related to this component, please refer to the main project documentation or contact the development team.

**Component Version:** 1.0  
**Last Updated:** December 2024
