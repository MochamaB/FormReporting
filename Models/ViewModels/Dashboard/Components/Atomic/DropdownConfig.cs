namespace FormReporting.Models.ViewModels.Dashboard.Components.Atomic
{
    /// <summary>
    /// Configuration for dropdown atomic component
    /// Used for filters, actions, and option menus in dashboard components
    /// </summary>
    public class DropdownConfig
    {
        /// <summary>
        /// Button text/label
        /// </summary>
        public string ButtonText { get; set; } = string.Empty;

        /// <summary>
        /// Button icon class (optional)
        /// </summary>
        public string? ButtonIcon { get; set; }

        /// <summary>
        /// Button variant: primary, secondary, soft-primary, soft-secondary, link, etc.
        /// </summary>
        public string ButtonVariant { get; set; } = "soft-secondary";

        /// <summary>
        /// Button size: sm, md (default), lg
        /// </summary>
        public string ButtonSize { get; set; } = "sm";

        /// <summary>
        /// Dropdown items
        /// </summary>
        public List<DropdownItem> Items { get; set; } = new List<DropdownItem>();

        /// <summary>
        /// Dropdown alignment: start (left), end (right)
        /// </summary>
        public string Alignment { get; set; } = "end";

        /// <summary>
        /// Direction: down, up, start, end
        /// </summary>
        public string Direction { get; set; } = "down";

        /// <summary>
        /// Show caret/arrow icon
        /// </summary>
        public bool ShowCaret { get; set; } = true;

        /// <summary>
        /// Additional CSS classes for button
        /// </summary>
        public string? ButtonClass { get; set; }

        /// <summary>
        /// Additional CSS classes for dropdown menu
        /// </summary>
        public string? MenuClass { get; set; }

        /// <summary>
        /// Get button CSS classes
        /// </summary>
        public string GetButtonClasses()
        {
            var classes = new List<string> { "btn" };

            var variant = ButtonVariant.StartsWith("btn-")
                ? ButtonVariant
                : $"btn-{ButtonVariant}";
            classes.Add(variant);

            if (ButtonSize != "md")
            {
                classes.Add($"btn-{ButtonSize}");
            }

            classes.Add("dropdown-toggle");

            if (!string.IsNullOrEmpty(ButtonClass))
            {
                classes.Add(ButtonClass);
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Get dropdown menu CSS classes
        /// </summary>
        public string GetMenuClasses()
        {
            var classes = new List<string> { "dropdown-menu" };

            if (Alignment == "end")
            {
                classes.Add("dropdown-menu-end");
            }

            if (!string.IsNullOrEmpty(MenuClass))
            {
                classes.Add(MenuClass);
            }

            return string.Join(" ", classes);
        }

        /// <summary>
        /// Helper: Create filter dropdown
        /// </summary>
        public static DropdownConfig Filter(string label = "Sort by: ", params string[] options)
        {
            var config = new DropdownConfig
            {
                ButtonText = label,
                ButtonVariant = "link",
                ShowCaret = true,
                ButtonClass = "text-reset dropdown-btn"
            };

            foreach (var option in options)
            {
                config.Items.Add(new DropdownItem { Text = option });
            }

            return config;
        }

        /// <summary>
        /// Helper: Create action dropdown (3-dot menu)
        /// </summary>
        public static DropdownConfig Actions(params DropdownItem[] items)
        {
            return new DropdownConfig
            {
                ButtonIcon = "ri-more-fill",
                ButtonVariant = "soft-secondary",
                ButtonSize = "sm",
                Items = items.ToList(),
                ShowCaret = false
            };
        }

        /// <summary>
        /// Helper: Create period selector dropdown
        /// </summary>
        public static DropdownConfig PeriodSelector()
        {
            return new DropdownConfig
            {
                ButtonText = "Current Month",
                ShowCaret = true,
                Items = new List<DropdownItem>
                {
                    new DropdownItem { Text = "Today", Value = "today" },
                    new DropdownItem { Text = "Last Week", Value = "lastweek" },
                    new DropdownItem { Text = "Last Month", Value = "lastmonth" },
                    new DropdownItem { Text = "Current Year", Value = "currentyear" }
                }
            };
        }
    }

    /// <summary>
    /// Dropdown item configuration
    /// </summary>
    public class DropdownItem
    {
        /// <summary>
        /// Item display text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Item value (for data binding)
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Item URL/action
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Item icon class
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Is this item a divider?
        /// </summary>
        public bool IsDivider { get; set; } = false;

        /// <summary>
        /// Is this item a header?
        /// </summary>
        public bool IsHeader { get; set; } = false;

        /// <summary>
        /// Is this item disabled?
        /// </summary>
        public bool Disabled { get; set; } = false;

        /// <summary>
        /// JavaScript click handler
        /// </summary>
        public string? OnClick { get; set; }

        /// <summary>
        /// Helper: Create divider item
        /// </summary>
        public static DropdownItem Divider() => new DropdownItem { IsDivider = true };

        /// <summary>
        /// Helper: Create header item
        /// </summary>
        public static DropdownItem Header(string text) => new DropdownItem { Text = text, IsHeader = true };
    }
}
