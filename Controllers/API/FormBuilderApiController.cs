using Microsoft.AspNetCore.Mvc;
using FormReporting.Data;
using FormReporting.Services.Forms;
using FormReporting.Models.ViewModels.Forms;

namespace FormReporting.Controllers.API
{
    /// <summary>
    /// API Controller for Form Builder operations
    /// Handles AJAX requests from the form builder UI
    /// </summary>
    [ApiController]
    [Route("api/formbuilder")]
    public class FormBuilderApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFormBuilderService _formBuilderService;

        public FormBuilderApiController(
            ApplicationDbContext context,
            IFormBuilderService formBuilderService)
        {
            _context = context;
            _formBuilderService = formBuilderService;
        }

        /// <summary>
        /// Get template data for builder
        /// </summary>
        [HttpGet("{templateId}")]
        public async Task<IActionResult> GetTemplateData(int templateId)
        {
            var data = await _formBuilderService.LoadForBuilderAsync(templateId);
            
            if (data == null)
                return NotFound(new { success = false, message = "Template not found" });

            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Get available field types for palette
        /// </summary>
        [HttpGet("field-types")]
        public IActionResult GetFieldTypes()
        {
            var fieldTypes = _formBuilderService.GetAvailableFieldTypes();
            return Ok(new { success = true, fieldTypes });
        }

        /// <summary>
        /// Validate template structure
        /// </summary>
        [HttpGet("{templateId}/validate")]
        public async Task<IActionResult> ValidateTemplate(int templateId)
        {
            var (isValid, errors) = await _formBuilderService.ValidateTemplateStructureAsync(templateId);
            
            return Ok(new 
            { 
                success = isValid, 
                isValid, 
                errors 
            });
        }

        /// <summary>
        /// Add a new section to a template
        /// </summary>
        [HttpPost("sections/add")]
        public async Task<IActionResult> AddSection([FromBody] AddSectionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data" });

            var section = await _formBuilderService.AddSectionAsync(request.TemplateId, request.Section);
            
            if (section == null)
                return NotFound(new { success = false, message = "Template not found" });

            return Ok(new { success = true, section });
        }

        /// <summary>
        /// Reorder sections after drag-drop
        /// </summary>
        [HttpPut("sections/reorder")]
        public async Task<IActionResult> ReorderSections([FromBody] ReorderSectionsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data" });

            var success = await _formBuilderService.ReorderSectionsAsync(request.TemplateId, request.Sections);
            
            if (!success)
                return BadRequest(new { success = false, message = "Failed to reorder sections" });

            return Ok(new { success = true, message = "Section order updated successfully" });
        }
    }

    /// <summary>
    /// Request model for adding a section
    /// </summary>
    public class AddSectionRequest
    {
        public int TemplateId { get; set; }
        public CreateSectionDto Section { get; set; } = new();
    }

    /// <summary>
    /// Request model for reordering sections
    /// </summary>
    public class ReorderSectionsRequest
    {
        public int TemplateId { get; set; }
        public List<SectionOrderDto> Sections { get; set; } = new();
    }
}
