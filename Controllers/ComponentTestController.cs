using Microsoft.AspNetCore.Mvc;
using FormReporting.Extensions;
using FormReporting.Models.ViewModels.Components;
using FormReporting.Models.Common;

namespace FormReporting.Controllers
{
    /// <summary>
    /// Controller for testing reusable UI components
    /// Remove or secure this controller in production
    /// </summary>
    public class ComponentTestController : Controller
    {
        /// <summary>
        /// Test all statistic card types
        /// Navigate to: /ComponentTest/StatCards
        /// </summary>
        public IActionResult StatCards()
        {
            return View("~/Views/Test/StatCards.cshtml");
        }

        /// <summary>
        /// Test with real data example
        /// Navigate to: /ComponentTest/RealDataExample
        /// </summary>
        public IActionResult RealDataExample()
        {
            // Simulate real controller data
            ViewBag.TotalTenants = 77;
            ViewBag.ActiveTenants = 69;
            ViewBag.InactiveTenants = 8;
            ViewBag.TotalRegions = 9;

            ViewBag.TotalUsers = 156;
            ViewBag.ActiveUsers = 142;
            ViewBag.TotalRoles = 12;
            ViewBag.TotalPermissions = 45;

            return View("~/Views/Test/RealDataExample.cshtml");
        }

        /// <summary>
        /// Test DataTable component with sample data
        /// Navigate to: /ComponentTest/DataTable
        /// </summary>
        public IActionResult DataTable(string? search, string? status, string? role, int? page)
        {
            // Simulate data from database
            var allUsers = GetSampleUsers();

            // Apply filters
            var filteredUsers = allUsers.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                filteredUsers = filteredUsers.Where(u =>
                    u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status))
            {
                filteredUsers = filteredUsers.Where(u => u.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(role))
            {
                filteredUsers = filteredUsers.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
            }

            // Calculate pagination
            var totalRecords = filteredUsers.Count();
            var (currentPage, totalPages, skip, take) = DataTableExtensions.CalculatePagination(page, totalRecords, 10);

            var pagedUsers = filteredUsers.Skip(skip).Take(take).ToList();

            // Pass data to view
            ViewBag.Users = pagedUsers;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Role = role;

            return View("~/Views/Test/DataTable.cshtml");
        }

        /// <summary>
        /// Test DataTable with bulk actions enabled
        /// Navigate to: /ComponentTest/DataTableBulk
        /// </summary>
        public IActionResult DataTableBulk(int? page)
        {
            var allUsers = GetSampleUsers();
            var totalRecords = allUsers.Count;
            var (currentPage, totalPages, skip, take) = DataTableExtensions.CalculatePagination(page, totalRecords, 10);
            var pagedUsers = allUsers.Skip(skip).Take(take).ToList();

            ViewBag.Users = pagedUsers;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;

            return View("~/Views/Test/DataTableBulk.cshtml");
        }

        /// <summary>
        /// Test Form Rendering with sample ICT reporting form structure
        /// Navigate to: /ComponentTest/FormRendering
        /// </summary>
        public IActionResult FormRendering()
        {
            // Create form configuration - this is how it would be done in real controllers
            var formConfig = new FormConfig
            {
                FormId = "monthly_ict_report",
                Title = "Monthly ICT Performance Report",
                Description = "Submit monthly ICT systems performance and support metrics for your factory",
                ShowProgressBar = true,
                EnableAutoSave = true,
                AutoSaveIntervalSeconds = 60,
                SubmitUrl = "/FormSubmission/Submit",
                SaveDraftUrl = "/FormSubmission/SaveDraft",
                ShowCancelButton = true,
                SubmitButtonText = "Submit Report",
                // Wizard mode settings
                RenderMode = FormRenderMode.Wizard,
                ShowStepNumbers = true,
                AllowStepSkipping = false,
                ValidateOnStepChange = true,
                PreviousButtonText = "Previous",
                NextButtonText = "Next"
            };

            // Section 1: Factory Information
            formConfig.WithSection("Factory Information", "Basic factory and reporting period details", section =>
            {
                section.WithIcon("ri-building-line");

                section.WithTextField("1", "Factory Name", isRequired: true, placeholder: "e.g., Kangaita Factory");

                section.WithDropdownField("2", "Region", new List<FormFieldOption>
                {
                    new FormFieldOption { Value = "", Label = "-- Select Region --" },
                    new FormFieldOption { Value = "central", Label = "Central Region" },
                    new FormFieldOption { Value = "eastern", Label = "Eastern Region" },
                    new FormFieldOption { Value = "mt_kenya", Label = "Mt Kenya Region" },
                    new FormFieldOption { Value = "rift_valley", Label = "Rift Valley Region" },
                    new FormFieldOption { Value = "south_rift", Label = "South Rift Region" }
                }, isRequired: true);

                var reportingPeriodField = new FormFieldConfig
                {
                    FieldId = "3",
                    FieldName = "Reporting Period",
                    FieldType = FormFieldType.Date,
                    IsRequired = true,
                    PlaceholderText = "Select month"
                }.WithHelpText("Select the month being reported");
                section.WithField(reportingPeriodField);
            });

            // Section 2: System Performance
            formConfig.WithSection("System Performance", "Overall system uptime and performance metrics", section =>
            {
                section.WithIcon("ri-dashboard-line");

                var uptimeField = new FormFieldConfig
                {
                    FieldId = "4",
                    FieldName = "System Uptime Percentage",
                    FieldType = FormFieldType.Percentage,
                    IsRequired = true,
                    PlaceholderText = "e.g., 99.5"
                }.WithValidation("Range", "Uptime must be between 0 and 100", minValue: 0, maxValue: 100)
                 .WithHelpText("Overall system availability for the reporting period");
                section.WithField(uptimeField);

                var incidentsField = new FormFieldConfig
                {
                    FieldId = "5",
                    FieldName = "Number of Critical Incidents",
                    FieldType = FormFieldType.Number,
                    IsRequired = true,
                    PlaceholderText = "e.g., 5"
                }.WithValidation("Range", "Must be a positive number", minValue: 0)
                 .WithHelpText("Total critical incidents reported during the month");
                section.WithField(incidentsField);

                var responseTimeField = new FormFieldConfig
                {
                    FieldId = "6",
                    FieldName = "Average Incident Response Time (hours)",
                    FieldType = FormFieldType.Number,
                    IsRequired = false,
                    PlaceholderText = "e.g., 2.5"
                }.WithHelpText("Average time to respond to critical incidents");
                section.WithField(responseTimeField);
            });

            // Section 3: Infrastructure Status
            formConfig.WithSection("Infrastructure Status", "Hardware and network infrastructure health", section =>
            {
                section.WithIcon("ri-server-line");
                section.AsCollapsible(startCollapsed: true);

                section.WithNumberField("7", "Active Servers", isRequired: true, minValue: 0);
                section.WithNumberField("8", "Active Workstations", isRequired: true, minValue: 0);

                section.WithDropdownField("9", "Network Status", new List<FormFieldOption>
                {
                    new FormFieldOption { Value = "", Label = "-- Select Status --" },
                    new FormFieldOption { Value = "excellent", Label = "Excellent - No issues" },
                    new FormFieldOption { Value = "good", Label = "Good - Minor issues" },
                    new FormFieldOption { Value = "fair", Label = "Fair - Some concerns" },
                    new FormFieldOption { Value = "poor", Label = "Poor - Needs attention" }
                }, isRequired: true);

                var infrastructureNotesField = new FormFieldConfig
                {
                    FieldId = "10",
                    FieldName = "Infrastructure Notes",
                    FieldType = FormFieldType.TextArea,
                    IsRequired = false,
                    PlaceholderText = "Any additional notes on infrastructure status..."
                }.WithHelpText("Optional notes about hardware issues, upgrades, or concerns");
                section.WithField(infrastructureNotesField);
            });

            // Section 4: User Support
            formConfig.WithSection("User Support & Training", "Support tickets and user training activities", section =>
            {
                section.WithIcon("ri-customer-service-2-line");
                section.AsCollapsible(startCollapsed: true);

                section.WithNumberField("11", "Support Tickets Resolved", isRequired: true, minValue: 0);
                section.WithNumberField("12", "Support Tickets Pending", isRequired: true, minValue: 0);
                section.WithNumberField("13", "Users Trained", isRequired: false, minValue: 0);

                var trainingTopicsField = new FormFieldConfig
                {
                    FieldId = "14",
                    FieldName = "Training Topics Covered",
                    FieldType = FormFieldType.TextArea,
                    IsRequired = false,
                    PlaceholderText = "List training sessions conducted..."
                }.WithHelpText("Describe any user training sessions conducted during this period");
                section.WithField(trainingTopicsField);
            });

            // Transform configuration to ViewModel
            var formViewModel = formConfig.BuildForm();

            return View("~/Views/Test/FormRendering.cshtml", formViewModel);
        }

        /// <summary>
        /// Test all form field types showcase
        /// Navigate to: /ComponentTest/FieldTypesShowcase
        /// </summary>
        public IActionResult FieldTypesShowcase()
        {
            var formConfig = new FormConfig
            {
                FormId = "field_types_showcase",
                Title = "Form Field Types Showcase",
                Description = "Demonstration of all 21 available form field types with examples",
                ShowProgressBar = false,
                RenderMode = FormRenderMode.SinglePage,
                SubmitButtonText = "Submit Test Form",
                ShowResetButton = true
            };

            // Section 1: Input Fields
            formConfig.WithSection("Input Fields", "Basic text and numeric input fields", section =>
            {
                section.WithIcon("ri-input-method-line");

                var textField = new FormFieldConfig
                {
                    FieldId = "txt1",
                    FieldName = "Text Field",
                    FieldType = FormFieldType.Text,
                    IsRequired = true,
                    PlaceholderText = "Enter text here..."
                }.WithHelpText("Standard single-line text input");
                section.WithField(textField);

                var textAreaField = new FormFieldConfig
                {
                    FieldId = "txt2",
                    FieldName = "Text Area Field",
                    FieldType = FormFieldType.TextArea,
                    IsRequired = false,
                    PlaceholderText = "Enter multiple lines of text..."
                }.WithHelpText("Multi-line text input for longer content");
                section.WithField(textAreaField);

                var numberField = new FormFieldConfig
                {
                    FieldId = "num1",
                    FieldName = "Number Field",
                    FieldType = FormFieldType.Number,
                    IsRequired = true,
                    PlaceholderText = "Enter a number"
                }.WithValidation("Range", "Value must be at least 0", minValue: 0)
                 .WithHelpText("Integer number input");
                section.WithField(numberField);

                var decimalField = new FormFieldConfig
                {
                    FieldId = "dec1",
                    FieldName = "Decimal Field",
                    FieldType = FormFieldType.Decimal,
                    IsRequired = false,
                    PlaceholderText = "0.00"
                }.WithHelpText("Decimal number with precision (step 0.01)");
                section.WithField(decimalField);
            });

            // Section 2: Date/Time Fields
            formConfig.WithSection("Date & Time Fields", "Date and time input fields", section =>
            {
                section.WithIcon("ri-calendar-line");

                var dateField = new FormFieldConfig
                {
                    FieldId = "date1",
                    FieldName = "Date Field",
                    FieldType = FormFieldType.Date,
                    IsRequired = true
                }.WithHelpText("Date picker (calendar)");
                section.WithField(dateField);

                var timeField = new FormFieldConfig
                {
                    FieldId = "time1",
                    FieldName = "Time Field",
                    FieldType = FormFieldType.Time,
                    IsRequired = false
                }.WithHelpText("Time picker (hours and minutes)");
                section.WithField(timeField);

                var dateTimeField = new FormFieldConfig
                {
                    FieldId = "datetime1",
                    FieldName = "DateTime Field",
                    FieldType = FormFieldType.DateTime,
                    IsRequired = false
                }.WithHelpText("Date and time picker combined");
                section.WithField(dateTimeField);
            });

            // Section 3: Selection Fields
            formConfig.WithSection("Selection Fields", "Dropdown, radio, checkbox, and multi-select options", section =>
            {
                section.WithIcon("ri-checkbox-multiple-line");

                var dropdownField = new FormFieldConfig
                {
                    FieldId = "dropdown1",
                    FieldName = "Dropdown Field",
                    FieldType = FormFieldType.Dropdown,
                    IsRequired = true,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Value = "", Label = "-- Select an option --" },
                        new FormFieldOption { Value = "opt1", Label = "Option 1" },
                        new FormFieldOption { Value = "opt2", Label = "Option 2" },
                        new FormFieldOption { Value = "opt3", Label = "Option 3" }
                    }
                }.WithHelpText("Single-select dropdown list");
                section.WithField(dropdownField);

                var radioField = new FormFieldConfig
                {
                    FieldId = "radio1",
                    FieldName = "Radio Button Field",
                    FieldType = FormFieldType.Radio,
                    IsRequired = true,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Value = "yes", Label = "Yes" },
                        new FormFieldOption { Value = "no", Label = "No" },
                        new FormFieldOption { Value = "maybe", Label = "Maybe" }
                    }
                }.WithHelpText("Radio button group (single selection)");
                section.WithField(radioField);

                var checkboxField = new FormFieldConfig
                {
                    FieldId = "check1",
                    FieldName = "Checkbox Field",
                    FieldType = FormFieldType.Checkbox,
                    IsRequired = false,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Value = "feature1", Label = "Feature 1" },
                        new FormFieldOption { Value = "feature2", Label = "Feature 2" },
                        new FormFieldOption { Value = "feature3", Label = "Feature 3" }
                    }
                }.WithHelpText("Checkbox group (multiple selection)");
                section.WithField(checkboxField);

                var multiSelectField = new FormFieldConfig
                {
                    FieldId = "multiselect1",
                    FieldName = "Multi-Select Field",
                    FieldType = FormFieldType.MultiSelect,
                    IsRequired = false,
                    Options = new List<FormFieldOption>
                    {
                        new FormFieldOption { Value = "item1", Label = "Item 1" },
                        new FormFieldOption { Value = "item2", Label = "Item 2" },
                        new FormFieldOption { Value = "item3", Label = "Item 3" },
                        new FormFieldOption { Value = "item4", Label = "Item 4" }
                    }
                }.WithHelpText("Multi-select dropdown (hold Ctrl to select multiple)");
                section.WithField(multiSelectField);
            });

            // Section 4: Media Fields
            formConfig.WithSection("Media Fields", "File upload, image, and signature capture", section =>
            {
                section.WithIcon("ri-image-line");

                var fileField = new FormFieldConfig
                {
                    FieldId = "file1",
                    FieldName = "File Upload Field",
                    FieldType = FormFieldType.FileUpload,
                    IsRequired = false
                }.WithHelpText("Upload any file type");
                section.WithField(fileField);

                var imageField = new FormFieldConfig
                {
                    FieldId = "img1",
                    FieldName = "Image Upload Field",
                    FieldType = FormFieldType.Image,
                    IsRequired = false
                }.WithHelpText("Upload image with preview (accepts image/* only)");
                section.WithField(imageField);

                var signatureField = new FormFieldConfig
                {
                    FieldId = "sig1",
                    FieldName = "Signature Field",
                    FieldType = FormFieldType.Signature,
                    IsRequired = false
                }.WithHelpText("Draw signature with mouse or touch");
                section.WithField(signatureField);
            });

            // Section 5: Rating Fields
            formConfig.WithSection("Rating & Slider Fields", "Star ratings and range sliders", section =>
            {
                section.WithIcon("ri-star-line");

                var ratingField = new FormFieldConfig
                {
                    FieldId = "rating1",
                    FieldName = "Rating Field",
                    FieldType = FormFieldType.Rating,
                    IsRequired = false
                }.WithHelpText("Click stars to rate (1-5 stars)");
                section.WithField(ratingField);

                var sliderField = new FormFieldConfig
                {
                    FieldId = "slider1",
                    FieldName = "Slider Field",
                    FieldType = FormFieldType.Slider,
                    IsRequired = false
                }.WithHelpText("Drag slider to select value (0-100)");
                section.WithField(sliderField);
            });

            // Section 6: Contact Fields
            formConfig.WithSection("Contact Fields", "Email, phone, and URL inputs with validation", section =>
            {
                section.WithIcon("ri-contacts-line");

                var emailField = new FormFieldConfig
                {
                    FieldId = "email1",
                    FieldName = "Email Field",
                    FieldType = FormFieldType.Email,
                    IsRequired = true,
                    PlaceholderText = "user@example.com"
                }.WithValidation("Email", "Please enter a valid email address")
                 .WithHelpText("Email input with built-in validation");
                section.WithField(emailField);

                var phoneField = new FormFieldConfig
                {
                    FieldId = "phone1",
                    FieldName = "Phone Field",
                    FieldType = FormFieldType.Phone,
                    IsRequired = false,
                    PlaceholderText = "+254 712 345678"
                }.WithHelpText("Phone number input");
                section.WithField(phoneField);

                var urlField = new FormFieldConfig
                {
                    FieldId = "url1",
                    FieldName = "URL Field",
                    FieldType = FormFieldType.Url,
                    IsRequired = false,
                    PlaceholderText = "https://example.com"
                }.WithHelpText("URL input with validation");
                section.WithField(urlField);
            });

            // Section 7: Specialized Fields
            formConfig.WithSection("Specialized Fields", "Currency and percentage inputs with formatting", section =>
            {
                section.WithIcon("ri-money-dollar-circle-line");

                var currencyField = new FormFieldConfig
                {
                    FieldId = "curr1",
                    FieldName = "Currency Field",
                    FieldType = FormFieldType.Currency,
                    IsRequired = false,
                    PrefixText = "KES",
                    PlaceholderText = "0.00"
                }.WithHelpText("Currency input with prefix (default KES)");
                section.WithField(currencyField);

                var percentageField = new FormFieldConfig
                {
                    FieldId = "perc1",
                    FieldName = "Percentage Field",
                    FieldType = FormFieldType.Percentage,
                    IsRequired = false,
                    PlaceholderText = "0"
                }.WithValidation("Range", "Percentage must be between 0 and 100", minValue: 0, maxValue: 100)
                .WithHelpText("Percentage input with % suffix (0-100)");
                section.WithField(percentageField);
            });

            var formViewModel = formConfig.BuildForm();
            return View("~/Views/Test/FieldTypesShowcase.cshtml", formViewModel);
        }

        // ========== HELPER METHOD ==========
        private List<SampleUser> GetSampleUsers()
        {
            return new List<SampleUser>
            {
                new SampleUser { Id = 1, Name = "John Kamau", Email = "john.kamau@ktda.co.ke", Role = "HeadOffice Admin", Status = "Active", Tenant = "Head Office" },
                new SampleUser { Id = 2, Name = "Mary Wanjiru", Email = "mary.wanjiru@ktda.co.ke", Role = "Regional Manager", Status = "Active", Tenant = "Central Region" },
                new SampleUser { Id = 3, Name = "Peter Otieno", Email = "peter.otieno@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Kangaita Factory" },
                new SampleUser { Id = 4, Name = "Jane Muthoni", Email = "jane.muthoni@ktda.co.ke", Role = "Factory User", Status = "Inactive", Tenant = "Chinga Factory" },
                new SampleUser { Id = 5, Name = "David Omondi", Email = "david.omondi@ktda.co.ke", Role = "Regional Manager", Status = "Active", Tenant = "Rift Valley Region" },
                new SampleUser { Id = 6, Name = "Sarah Njeri", Email = "sarah.njeri@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Gatundu Factory" },
                new SampleUser { Id = 7, Name = "James Kiprop", Email = "james.kiprop@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Kapkatet Factory" },
                new SampleUser { Id = 8, Name = "Lucy Akinyi", Email = "lucy.akinyi@ktda.co.ke", Role = "HeadOffice Admin", Status = "Active", Tenant = "Head Office" },
                new SampleUser { Id = 9, Name = "Samuel Kariuki", Email = "samuel.kariuki@ktda.co.ke", Role = "Factory User", Status = "Inactive", Tenant = "Kimunye Factory" },
                new SampleUser { Id = 10, Name = "Grace Wambui", Email = "grace.wambui@ktda.co.ke", Role = "Regional Manager", Status = "Active", Tenant = "Eastern Region" },
                new SampleUser { Id = 11, Name = "Daniel Mutua", Email = "daniel.mutua@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Makomboki Factory" },
                new SampleUser { Id = 12, Name = "Ruth Adhiambo", Email = "ruth.adhiambo@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Momul Factory" },
                new SampleUser { Id = 13, Name = "Patrick Mwangi", Email = "patrick.mwangi@ktda.co.ke", Role = "Factory User", Status = "Inactive", Tenant = "Ndima Factory" },
                new SampleUser { Id = 14, Name = "Elizabeth Nyambura", Email = "elizabeth.nyambura@ktda.co.ke", Role = "Regional Manager", Status = "Active", Tenant = "Mt Kenya Region" },
                new SampleUser { Id = 15, Name = "Michael Chelanga", Email = "michael.chelanga@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Toror Factory" },
                new SampleUser { Id = 16, Name = "Ann Wanjiku", Email = "ann.wanjiku@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Gachege Factory" },
                new SampleUser { Id = 17, Name = "Joseph Kibet", Email = "joseph.kibet@ktda.co.ke", Role = "HeadOffice Admin", Status = "Active", Tenant = "Head Office" },
                new SampleUser { Id = 18, Name = "Catherine Wangari", Email = "catherine.wangari@ktda.co.ke", Role = "Factory User", Status = "Inactive", Tenant = "Kanyenyaini Factory" },
                new SampleUser { Id = 19, Name = "Francis Maina", Email = "francis.maina@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Ragati Factory" },
                new SampleUser { Id = 20, Name = "Margaret Chebet", Email = "margaret.chebet@ktda.co.ke", Role = "Regional Manager", Status = "Active", Tenant = "South Rift Region" },
                new SampleUser { Id = 21, Name = "George Karanja", Email = "george.karanja@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Theta Factory" },
                new SampleUser { Id = 22, Name = "Alice Njoki", Email = "alice.njoki@ktda.co.ke", Role = "Factory User", Status = "Active", Tenant = "Imenti Factory" },
                new SampleUser { Id = 23, Name = "Robert Koech", Email = "robert.koech@ktda.co.ke", Role = "Factory User", Status = "Inactive", Tenant = "Kapset Factory" }
            };
        }

        public class SampleUser
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
            public string Status { get; set; } = "";
            public string Tenant { get; set; } = "";
        }
    }
}
