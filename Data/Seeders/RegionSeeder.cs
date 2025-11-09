using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds initial region data
    /// </summary>
    public static class RegionSeeder
    {
        /// <summary>
        /// Seeds regions if they don't already exist
        /// </summary>
        public static void SeedRegions(ApplicationDbContext context)
        {
            // Check if regions already exist
            if (context.Regions.Any())
            {
                return; // Data already seeded
            }

            var regions = new List<Region>
            {
                new Region
                {
                    RegionNumber = 1,
                    RegionCode = "REG001",
                    RegionName = "Region 1",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Region
                {
                    RegionNumber = 2,
                    RegionCode = "REG002",
                    RegionName = "Region 2",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Region
                {
                    RegionNumber = 3,
                    RegionCode = "REG003",
                    RegionName = "Region 3",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Region
                {
                    RegionNumber = 4,
                    RegionCode = "REG004",
                    RegionName = "Region 4",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Region
                {
                    RegionNumber = 5,
                    RegionCode = "REG005",
                    RegionName = "Region 5",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Region
                {
                    RegionNumber = 6,
                    RegionCode = "REG006",
                    RegionName = "Region 6",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Region
                {
                    RegionNumber = 7,
                    RegionCode = "REG007",
                    RegionName = "Region 7",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            };

            context.Regions.AddRange(regions);
            context.SaveChanges();
        }
    }
}
