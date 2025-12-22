namespace FormReporting.Models.ViewModels.Forms
{
    /// <summary>
    /// Section data transfer object for Form Builder
    /// Represents a section (page) in the form template
    /// </summary>
    public class SectionDto
    {
        /// <summary>
        /// Section ID (database primary key)
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// Section name/title
        /// </summary>
        public string SectionName { get; set; } = string.Empty;

        /// <summary>
        /// Optional section description
        /// </summary>
        public string? SectionDescription { get; set; }

        /// <summary>
        /// Display order (1, 2, 3...)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Can this section be collapsed?
        /// </summary>
        public bool IsCollapsible { get; set; } = true;

        /// <summary>
        /// Should section start collapsed?
        /// </summary>
        public bool IsCollapsedByDefault { get; set; } = false;

        /// <summary>
        /// Icon class (e.g., "ri-information-line")
        /// </summary>
        public string? IconClass { get; set; }

        /// <summary>
        /// Column layout (1 = Single, 2 = Two Columns, 3 = Three Columns)
        /// </summary>
        public int ColumnLayout { get; set; } = 1;

        /// <summary>
        /// Number of fields in this section
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// List of fields in this section
        /// </summary>
        public List<FieldDto> Fields { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a new section
    /// </summary>
    public class CreateSectionDto
    {
        public string SectionName { get; set; } = string.Empty;
        public string? SectionDescription { get; set; }
        public string? IconClass { get; set; }
        public int ColumnLayout { get; set; } = 1;
        public bool IsCollapsible { get; set; } = true;
        public bool IsCollapsedByDefault { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating an existing section
    /// </summary>
    public class UpdateSectionDto
    {
        public string SectionName { get; set; } = string.Empty;
        public string? SectionDescription { get; set; }
        public string? IconClass { get; set; }
        public bool IsCollapsible { get; set; }
        public bool IsCollapsedByDefault { get; set; }
    }

    /// <summary>
    /// DTO for section configuration (layout, styling, etc.)
    /// </summary>
    public class SectionConfigDto
    {
        public int ColumnLayout { get; set; } = 1;
        public string? SectionWidth { get; set; }
        public string? BackgroundStyle { get; set; }
        public bool? ShowSectionNumber { get; set; }
        public int? TopPadding { get; set; }
        public int? BottomPadding { get; set; }
    }
}
