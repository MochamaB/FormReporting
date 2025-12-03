using System.ComponentModel.DataAnnotations;

namespace FormReporting.Models.ViewModels.Metrics
{
    /// <summary>
    /// DTO for building calculated metric formulas
    /// </summary>
    public class FormulaBuilderDto
    {
        [Required]
        public string Formula { get; set; } = string.Empty;

        [Required]
        public List<int> SourceItemIds { get; set; } = new();

        /// <summary>
        /// Maps variable names in formula to ItemIds
        /// Example: {"operational": 21, "total": 20}
        /// Used in formula: "(operational / total) * 100"
        /// </summary>
        [Required]
        public Dictionary<string, int> ItemAliases { get; set; } = new();

        public int? RoundTo { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public bool ValidateDivisionByZero { get; set; } = true;

        /// <summary>
        /// Convert to TransformationLogic JSON
        /// </summary>
        public string ToJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                formula = Formula,
                sourceItems = SourceItemIds,
                itemAliases = ItemAliases,
                roundTo = RoundTo,
                minValue = MinValue,
                maxValue = MaxValue,
                validateDivisionByZero = ValidateDivisionByZero
            });
        }

        /// <summary>
        /// Parse from TransformationLogic JSON
        /// </summary>
        public static FormulaBuilderDto? FromJson(string json)
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new FormulaBuilderDto
                {
                    Formula = root.GetProperty("formula").GetString() ?? string.Empty,
                    SourceItemIds = root.GetProperty("sourceItems")
                        .EnumerateArray()
                        .Select(e => e.GetInt32())
                        .ToList(),
                    ItemAliases = root.GetProperty("itemAliases")
                        .EnumerateObject()
                        .ToDictionary(p => p.Name, p => p.Value.GetInt32()),
                    RoundTo = root.TryGetProperty("roundTo", out var roundTo) 
                        ? roundTo.GetInt32() : null,
                    MinValue = root.TryGetProperty("minValue", out var minVal) 
                        ? minVal.GetDecimal() : null,
                    MaxValue = root.TryGetProperty("maxValue", out var maxVal) 
                        ? maxVal.GetDecimal() : null,
                    ValidateDivisionByZero = root.TryGetProperty("validateDivisionByZero", out var validate) 
                        ? validate.GetBoolean() : true
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
