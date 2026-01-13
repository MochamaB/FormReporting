using FormReporting.Models.Common;

namespace FormReporting.Services.Metrics
{
    /// <summary>
    /// Service to validate mapping types based on field types and provide mapping recommendations
    /// </summary>
    public interface IFieldMappingValidationService
    {
        /// <summary>
        /// Get valid mapping types for a given field type
        /// </summary>
        List<string> GetValidMappingTypes(FormFieldType fieldType);
        
        /// <summary>
        /// Check if a mapping type is valid for a field type
        /// </summary>
        bool IsValidMappingType(FormFieldType fieldType, string mappingType);
        
        /// <summary>
        /// Get recommended mapping type for a field type
        /// </summary>
        string GetRecommendedMappingType(FormFieldType fieldType);
        
        /// <summary>
        /// Get field type categories for UI grouping
        /// </summary>
        Dictionary<string, List<FormFieldType>> GetFieldTypeCategories();

        /// <summary>
        /// Get valid aggregation types for a given field type and mapping type combination
        /// </summary>
        List<string> GetValidAggregationTypes(FormFieldType fieldType, string mappingType);

        /// <summary>
        /// Check if an aggregation type is valid for a field type and mapping type combination
        /// </summary>
        bool IsValidAggregationType(FormFieldType fieldType, string mappingType, string aggregationType);

        /// <summary>
        /// Get recommended aggregation type for a field type and mapping type combination
        /// </summary>
        string GetRecommendedAggregationType(FormFieldType fieldType, string mappingType);

        /// <summary>
        /// Get all available aggregation types
        /// </summary>
        List<string> GetAllAggregationTypes();

        /// <summary>
        /// Suggest thresholds based on data type and aggregation type
        /// </summary>
        (decimal? Green, decimal? Yellow, decimal? Red) SuggestThresholds(string dataType, string aggregationType);
    }

    public class FieldMappingValidationService : IFieldMappingValidationService
    {
        // Mapping type constants
        private const string DIRECT = "Direct";
        private const string CALCULATED = "Calculated";
        private const string DERIVED = "Derived";
        private const string EXPECTED = "Expected";

        // Aggregation type constants
        private const string AGG_SUM = "Sum";
        private const string AGG_AVERAGE = "Average";
        private const string AGG_COUNT = "Count";
        private const string AGG_MIN = "Min";
        private const string AGG_MAX = "Max";
        private const string AGG_LATEST = "Latest";
        private const string AGG_PERCENTAGE = "Percentage";

        // All available aggregation types
        private static readonly List<string> _allAggregationTypes = new()
        {
            AGG_SUM, AGG_AVERAGE, AGG_COUNT, AGG_MIN, AGG_MAX, AGG_LATEST, AGG_PERCENTAGE
        };

        // Field type to mapping type validation matrix
        private readonly Dictionary<FormFieldType, List<string>> _validMappingTypes = new()
        {
            // Numeric fields - can be direct, calculated, or derived
            [FormFieldType.Number] = new() { DIRECT, CALCULATED, DERIVED },
            [FormFieldType.Decimal] = new() { DIRECT, CALCULATED, DERIVED },
            [FormFieldType.Currency] = new() { DIRECT, CALCULATED, DERIVED },
            [FormFieldType.Percentage] = new() { DIRECT, CALCULATED, DERIVED },
            [FormFieldType.Rating] = new() { DIRECT, CALCULATED, DERIVED },
            [FormFieldType.Slider] = new() { DIRECT, CALCULATED, DERIVED },

            // Selection fields - can be direct (with scores), expected, or derived
            [FormFieldType.Dropdown] = new() { DIRECT, EXPECTED, DERIVED },
            [FormFieldType.Radio] = new() { DIRECT, EXPECTED, DERIVED },
            [FormFieldType.Checkbox] = new() { DIRECT, EXPECTED, DERIVED },

            // Text fields - only expected compliance or derived
            [FormFieldType.Text] = new() { EXPECTED, DERIVED },

            // Date fields - calculated (for durations) or derived
            [FormFieldType.Date] = new() { CALCULATED, DERIVED },
            [FormFieldType.DateTime] = new() { CALCULATED, DERIVED },
            [FormFieldType.Time] = new() { CALCULATED, DERIVED },

            // Multi-select - calculated (count-based) or derived
            [FormFieldType.MultiSelect] = new() { CALCULATED, DERIVED },

            // Media and contact fields - only derived (not directly measurable)
            [FormFieldType.FileUpload] = new() { DERIVED },
            [FormFieldType.Image] = new() { DERIVED },
            [FormFieldType.Signature] = new() { DERIVED },
            [FormFieldType.Email] = new() { DERIVED },
            [FormFieldType.Phone] = new() { DERIVED },
            [FormFieldType.Url] = new() { DERIVED },
            [FormFieldType.TextArea] = new() { DERIVED }
        };

        // Recommended mapping types (first choice for each field type)
        private readonly Dictionary<FormFieldType, string> _recommendedMappingTypes = new()
        {
            // Numeric fields - direct mapping is most common
            [FormFieldType.Number] = DIRECT,
            [FormFieldType.Decimal] = DIRECT,
            [FormFieldType.Currency] = DIRECT,
            [FormFieldType.Percentage] = DIRECT,
            [FormFieldType.Rating] = DIRECT,
            [FormFieldType.Slider] = DIRECT,

            // Selection fields - direct if scored, expected if compliance-based
            [FormFieldType.Dropdown] = DIRECT,
            [FormFieldType.Radio] = DIRECT,
            [FormFieldType.Checkbox] = EXPECTED,

            // Text fields - expected compliance
            [FormFieldType.Text] = EXPECTED,

            // Date fields - calculated for durations
            [FormFieldType.Date] = CALCULATED,
            [FormFieldType.DateTime] = CALCULATED,
            [FormFieldType.Time] = CALCULATED,

            // Multi-select - calculated for counts
            [FormFieldType.MultiSelect] = CALCULATED,

            // Media and contact - derived only
            [FormFieldType.FileUpload] = DERIVED,
            [FormFieldType.Image] = DERIVED,
            [FormFieldType.Signature] = DERIVED,
            [FormFieldType.Email] = DERIVED,
            [FormFieldType.Phone] = DERIVED,
            [FormFieldType.Url] = DERIVED,
            [FormFieldType.TextArea] = DERIVED
        };

        public List<string> GetValidMappingTypes(FormFieldType fieldType)
        {
            return _validMappingTypes.TryGetValue(fieldType, out var mappingTypes) 
                ? mappingTypes 
                : new List<string> { DERIVED }; // Default to derived if unknown
        }

        public bool IsValidMappingType(FormFieldType fieldType, string mappingType)
        {
            var validTypes = GetValidMappingTypes(fieldType);
            return validTypes.Contains(mappingType);
        }

        public string GetRecommendedMappingType(FormFieldType fieldType)
        {
            return _recommendedMappingTypes.TryGetValue(fieldType, out var recommendedType) 
                ? recommendedType 
                : DERIVED; // Default to derived if unknown
        }

        public Dictionary<string, List<FormFieldType>> GetFieldTypeCategories()
        {
            return new Dictionary<string, List<FormFieldType>>
            {
                ["Numeric"] = new()
                {
                    FormFieldType.Number, FormFieldType.Decimal, FormFieldType.Currency,
                    FormFieldType.Percentage, FormFieldType.Rating, FormFieldType.Slider
                },
                ["Selection"] = new()
                {
                    FormFieldType.Dropdown, FormFieldType.Radio, FormFieldType.Checkbox,
                    FormFieldType.MultiSelect
                },
                ["Text"] = new()
                {
                    FormFieldType.Text, FormFieldType.TextArea
                },
                ["Date/Time"] = new()
                {
                    FormFieldType.Date, FormFieldType.DateTime, FormFieldType.Time
                },
                ["Media"] = new()
                {
                    FormFieldType.FileUpload, FormFieldType.Image, FormFieldType.Signature
                },
                ["Contact"] = new()
                {
                    FormFieldType.Email, FormFieldType.Phone, FormFieldType.Url
                }
            };
        }

        // ===================================================================
        // AGGREGATION TYPE VALIDATION
        // ===================================================================

        // Valid aggregation types per field type + mapping type combination
        private readonly Dictionary<(FormFieldType, string), List<string>> _validAggregationTypes = new()
        {
            // Numeric + Direct → all numeric aggregations
            [(FormFieldType.Number, DIRECT)] = new() { AGG_SUM, AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Decimal, DIRECT)] = new() { AGG_SUM, AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Currency, DIRECT)] = new() { AGG_SUM, AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_LATEST },
            [(FormFieldType.Percentage, DIRECT)] = new() { AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_LATEST },
            [(FormFieldType.Rating, DIRECT)] = new() { AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_COUNT },
            [(FormFieldType.Slider, DIRECT)] = new() { AGG_AVERAGE, AGG_MIN, AGG_MAX },

            // Numeric + Calculated → same as direct
            [(FormFieldType.Number, CALCULATED)] = new() { AGG_SUM, AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Decimal, CALCULATED)] = new() { AGG_SUM, AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Currency, CALCULATED)] = new() { AGG_SUM, AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_LATEST },
            [(FormFieldType.Percentage, CALCULATED)] = new() { AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_LATEST },
            [(FormFieldType.Rating, CALCULATED)] = new() { AGG_AVERAGE, AGG_MIN, AGG_MAX, AGG_COUNT },
            [(FormFieldType.Slider, CALCULATED)] = new() { AGG_AVERAGE, AGG_MIN, AGG_MAX },

            // Selection + Direct (scored options)
            [(FormFieldType.Dropdown, DIRECT)] = new() { AGG_AVERAGE, AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Radio, DIRECT)] = new() { AGG_AVERAGE, AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Checkbox, DIRECT)] = new() { AGG_SUM, AGG_COUNT, AGG_AVERAGE },

            // Selection + Expected → percentage-friendly for compliance tracking
            [(FormFieldType.Dropdown, EXPECTED)] = new() { AGG_PERCENTAGE, AGG_COUNT, AGG_SUM },
            [(FormFieldType.Radio, EXPECTED)] = new() { AGG_PERCENTAGE, AGG_COUNT, AGG_SUM },
            [(FormFieldType.Checkbox, EXPECTED)] = new() { AGG_PERCENTAGE, AGG_COUNT, AGG_SUM },

            // Text + Expected
            [(FormFieldType.Text, EXPECTED)] = new() { AGG_PERCENTAGE, AGG_COUNT },

            // Date/Time + Calculated
            [(FormFieldType.Date, CALCULATED)] = new() { AGG_COUNT, AGG_LATEST, AGG_MIN, AGG_MAX },
            [(FormFieldType.DateTime, CALCULATED)] = new() { AGG_COUNT, AGG_LATEST, AGG_MIN, AGG_MAX },
            [(FormFieldType.Time, CALCULATED)] = new() { AGG_COUNT, AGG_AVERAGE },

            // MultiSelect + Calculated
            [(FormFieldType.MultiSelect, CALCULATED)] = new() { AGG_COUNT, AGG_SUM, AGG_AVERAGE },

            // Derived mappings - typically count or latest
            [(FormFieldType.FileUpload, DERIVED)] = new() { AGG_COUNT, AGG_SUM },
            [(FormFieldType.Image, DERIVED)] = new() { AGG_COUNT },
            [(FormFieldType.Signature, DERIVED)] = new() { AGG_COUNT },
            [(FormFieldType.Email, DERIVED)] = new() { AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Phone, DERIVED)] = new() { AGG_COUNT, AGG_LATEST },
            [(FormFieldType.Url, DERIVED)] = new() { AGG_COUNT, AGG_LATEST },
            [(FormFieldType.TextArea, DERIVED)] = new() { AGG_COUNT, AGG_LATEST }
        };

        // Recommended aggregation types per field type + mapping type
        private readonly Dictionary<(FormFieldType, string), string> _recommendedAggregationTypes = new()
        {
            // Numeric fields
            [(FormFieldType.Number, DIRECT)] = AGG_SUM,
            [(FormFieldType.Decimal, DIRECT)] = AGG_SUM,
            [(FormFieldType.Currency, DIRECT)] = AGG_SUM,
            [(FormFieldType.Percentage, DIRECT)] = AGG_AVERAGE,
            [(FormFieldType.Rating, DIRECT)] = AGG_AVERAGE,
            [(FormFieldType.Slider, DIRECT)] = AGG_AVERAGE,

            // Selection + Direct
            [(FormFieldType.Dropdown, DIRECT)] = AGG_AVERAGE,
            [(FormFieldType.Radio, DIRECT)] = AGG_AVERAGE,
            [(FormFieldType.Checkbox, DIRECT)] = AGG_COUNT,

            // Selection + Expected → Percentage for compliance tracking
            [(FormFieldType.Dropdown, EXPECTED)] = AGG_PERCENTAGE,
            [(FormFieldType.Radio, EXPECTED)] = AGG_PERCENTAGE,
            [(FormFieldType.Checkbox, EXPECTED)] = AGG_PERCENTAGE,

            // Text + Expected
            [(FormFieldType.Text, EXPECTED)] = AGG_PERCENTAGE,

            // Date/Time
            [(FormFieldType.Date, CALCULATED)] = AGG_COUNT,
            [(FormFieldType.DateTime, CALCULATED)] = AGG_COUNT,
            [(FormFieldType.Time, CALCULATED)] = AGG_AVERAGE,

            // MultiSelect
            [(FormFieldType.MultiSelect, CALCULATED)] = AGG_COUNT,

            // Derived
            [(FormFieldType.FileUpload, DERIVED)] = AGG_COUNT,
            [(FormFieldType.Image, DERIVED)] = AGG_COUNT,
            [(FormFieldType.Signature, DERIVED)] = AGG_COUNT,
            [(FormFieldType.Email, DERIVED)] = AGG_COUNT,
            [(FormFieldType.Phone, DERIVED)] = AGG_COUNT,
            [(FormFieldType.Url, DERIVED)] = AGG_COUNT,
            [(FormFieldType.TextArea, DERIVED)] = AGG_COUNT
        };

        public List<string> GetValidAggregationTypes(FormFieldType fieldType, string mappingType)
        {
            var key = (fieldType, mappingType);
            if (_validAggregationTypes.TryGetValue(key, out var aggregationTypes))
            {
                return aggregationTypes;
            }

            // Fallback: return count and latest for unknown combinations
            return new List<string> { AGG_COUNT, AGG_LATEST };
        }

        public bool IsValidAggregationType(FormFieldType fieldType, string mappingType, string aggregationType)
        {
            var validTypes = GetValidAggregationTypes(fieldType, mappingType);
            return validTypes.Contains(aggregationType);
        }

        public string GetRecommendedAggregationType(FormFieldType fieldType, string mappingType)
        {
            var key = (fieldType, mappingType);
            if (_recommendedAggregationTypes.TryGetValue(key, out var recommendedType))
            {
                return recommendedType;
            }

            // Fallback
            return AGG_COUNT;
        }

        public List<string> GetAllAggregationTypes()
        {
            return _allAggregationTypes;
        }

        // ===================================================================
        // THRESHOLD SUGGESTIONS
        // ===================================================================

        public (decimal? Green, decimal? Yellow, decimal? Red) SuggestThresholds(string dataType, string aggregationType)
        {
            // Percentage-based metrics (including Percentage aggregation)
            if (dataType == "Percentage" || aggregationType == AGG_PERCENTAGE)
            {
                return (90m, 60m, 30m);
            }

            // Rating scales
            if (dataType == "Rating")
            {
                // Assume 1-5 scale by default
                return (4m, 3m, 2m);
            }

            // Boolean/Binary (average gives 0-1, multiply by 100 for percentage)
            if (dataType == "Boolean")
            {
                return (0.9m, 0.6m, 0.3m);
            }

            // For absolute numbers (Sum, Count, etc.) - no sensible default
            // Return null to indicate user must define
            return (null, null, null);
        }
    }
}
