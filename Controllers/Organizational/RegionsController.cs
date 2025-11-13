using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FormReporting.Data;
using FormReporting.Models.ViewModels.Organizational;
using FormReporting.Models.Entities.Organizational;

namespace FormReporting.Controllers.Organizational
{
    public class RegionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Index - List all regions with statistics, filters, and pagination
        /// </summary>
        public async Task<IActionResult> Index(string? search, string? status, int page = 1)
        {
            const int pageSize = 15; // Items per page

            // Start with base query
            var query = _context.Regions
                .Include(r => r.Tenants)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    r.RegionNumber.ToString().Contains(search) ||
                    r.RegionCode.ToLower().Contains(search) ||
                    r.RegionName.ToLower().Contains(search));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.ToLower() == "active")
                    query = query.Where(r => r.IsActive);
                else if (status.ToLower() == "inactive")
                    query = query.Where(r => !r.IsActive);
            }

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Calculate statistics (on filtered data)
            var allRegions = await query.ToListAsync();
            
            ViewBag.TotalRegions = allRegions.Count(r => r.IsActive);
            ViewBag.TotalTenants = allRegions.Sum(r => r.Tenants.Count(t => t.IsActive));
            ViewBag.ActiveRegions = allRegions.Count(r => r.IsActive && r.Tenants.Any(t => t.IsActive));
            ViewBag.InactiveRegions = allRegions.Count(r => !r.IsActive);

            // Calculate trend (regions created in last 30 days vs previous 30 days)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var sixtyDaysAgo = DateTime.Now.AddDays(-60);
            var recentRegions = await _context.Regions.CountAsync(r => r.CreatedDate >= thirtyDaysAgo);
            var previousRegions = await _context.Regions.CountAsync(r => r.CreatedDate >= sixtyDaysAgo && r.CreatedDate < thirtyDaysAgo);
            ViewBag.RegionGrowth = previousRegions > 0 
                ? ((recentRegions - previousRegions) / (double)previousRegions * 100) 
                : 0;

            // Execute query with pagination
            var regions = await query
                .OrderBy(r => r.RegionNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RegionViewModel
                {
                    RegionId = r.RegionId,
                    RegionNumber = r.RegionNumber.ToString(),
                    RegionCode = r.RegionCode,
                    RegionName = r.RegionName,
                    TenantCount = r.Tenants.Count(t => t.IsActive),
                    IsActive = r.IsActive,
                    CreatedDate = r.CreatedDate,
                    ModifiedDate = r.ModifiedDate
                })
                .ToListAsync();

            // Pass pagination info to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            // Pass filter values to view for maintaining state
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;

            return View("~/Views/Organizational/Regions/Index.cshtml", regions);
        }

        /// <summary>
        /// Create - Display form for creating a new region
        /// </summary>
        public IActionResult Create()
        {
            return View("~/Views/Organizational/Regions/Create.cshtml");
        }

        /// <summary>
        /// Create - Handle form submission for creating a new region
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Region region)
        {
            if (ModelState.IsValid)
            {
                _context.Add(region);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Region '{region.RegionName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Organizational/Regions/Create.cshtml", region);
        }

        /// <summary>
        /// Edit - Display form for editing an existing region
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions.FindAsync(id);
            if (region == null)
            {
                return NotFound();
            }

            return View("~/Views/Organizational/Regions/Edit.cshtml", region);
        }

        /// <summary>
        /// Edit - Handle form submission for updating an existing region
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Region region)
        {
            if (id != region.RegionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(region);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Region '{region.RegionName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await RegionExistsAsync(region.RegionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Organizational/Regions/Edit.cshtml", region);
        }

        /// <summary>
        /// Details - Display detailed information about a specific region
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .Include(r => r.Tenants)
                .FirstOrDefaultAsync(r => r.RegionId == id);

            if (region == null)
            {
                return NotFound();
            }

            return View("~/Views/Organizational/Regions/Details.cshtml", region);
        }

        private async Task<bool> RegionExistsAsync(int id)
        {
            return await _context.Regions.AnyAsync(e => e.RegionId == id);
        }
    }
}
