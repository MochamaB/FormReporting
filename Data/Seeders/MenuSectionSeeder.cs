// Data/Seeders/MenuSectionSeeder.cs
using FormReporting.Models.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    public static class MenuSectionSeeder
    {
         public static void SeedMenuSections(ApplicationDbContext context)
        {
            if (context.MenuSections.Any())
                return;

            var sections = new List<MenuSection>
            {
                new() { SectionName = "MAIN MENU", SectionCode = "MAIN", DisplayOrder = 1 },
                new() { SectionName = "ASSET MANAGEMENT", SectionCode = "ASSETS", DisplayOrder = 2 },
                new() { SectionName = "ORGANIZATIONAL MANAGEMENT", SectionCode = "ORG", DisplayOrder = 3 },
                new() { SectionName = "PERFORMANCE & METRICS", SectionCode = "METRICS", DisplayOrder = 4 },
                new() { SectionName = "FINANCIAL MANAGEMENT", SectionCode = "FINANCE", DisplayOrder = 5 },
                new() { SectionName = "SYSTEM ADMINISTRATION", SectionCode = "ADMIN", DisplayOrder = 6 }
            };

            context.MenuSections.AddRange(sections);
            context.SaveChanges();
        }
    }
}