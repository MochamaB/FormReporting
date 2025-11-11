using FormReporting.Models.Entities.Organizational;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds initial tenant data (Head Office, Factories, Subsidiaries)
    /// </summary>
    public static class TenantSeeder
    {
        /// <summary>
        /// Seeds tenants if they don't already exist
        /// </summary>
        public static void SeedTenants(ApplicationDbContext context)
        {
            // Check if tenants already exist
            if (context.Tenants.Any())
            {
                return; // Data already seeded
            }

            var tenants = new List<Tenant>();

            // 1. HEAD OFFICE
            tenants.Add(new Tenant
            {
                TenantType = "HeadOffice",
                TenantCode = "HO001",
                TenantName = "KTDA Head Office",
                RegionId = null,
                IsActive = true,
                CreatedDate = DateTime.Now
            });

            // 2. FACTORIES - Get region IDs from database
            var regions = context.Regions.ToDictionary(r => r.RegionNumber, r => r.RegionId);

            // Region 1 Factories (Kiambu & Murang'a)
            var region1Factories = new[]
            {
                "Gacharage Tea Factory", "Gachege Tea Factory", "Ikumbi Tea Factory",
                "Kambaa Tea Factory", "Kagwe Tea Factory", "Mataara Tea Factory",
                "Ndarugu Tea Factory", "Nduti Tea Factory", "Ngere Tea Factory",
                "Njunu Tea Factory", "Theta Tea Factory", "Makomboki Tea Factory"
            };
            tenants.AddRange(CreateFactories(region1Factories, 1, regions[1]));

            // Region 2 Factories (Murang'a & Nyeri)
            var region2Factories = new[]
            {
                "Chinga Tea Factory", "Gathuthi Tea Factory", "Gatunguru Tea Factory",
                "Githambo Tea Factory", "Gitugi Tea Factory", "Iriaini Tea Factory",
                "Kanyenyaini Tea Factory", "Kiru Tea Factory", "Ragati Tea Factory"
            };
            tenants.AddRange(CreateFactories(region2Factories, 2, regions[2]));

            // Region 3 Factories (Kirinyaga & Embu)
            var region3Factories = new[]
            {
                "Kangaita Tea Factory", "Kathangariri Tea Factory", "Kimunye Tea Factory",
                "Mununga Tea Factory", "Mungania Tea Factory", "Ndima Tea Factory",
                "Rukuriri Tea Factory", "Thumaita Tea Factory"
            };
            tenants.AddRange(CreateFactories(region3Factories, 3, regions[3]));

            // Region 4 Factories (Meru & Tharaka Nithi)
            var region4Factories = new[]
            {
                "Githongo Tea Factory", "Igembe Tea Factory", "Imenti Tea Factory",
                "Kiegoi Tea Factory", "Kinoro Tea Factory", "Kionyo Tea Factory",
                "Michimikuru Tea Factory", "Weru Tea Factory"
            };
            tenants.AddRange(CreateFactories(region4Factories, 4, regions[4]));

            // Region 5 Factories (Kericho & Bomet)
            var region5Factories = new[]
            {
                "Boito Tea Factory", "Kapkatet Tea Factory", "Kapkoros Tea Factory",
                "Kapset Tea Factory", "Kobel Tea Factory", "Litein Tea Factory",
                "Mogogosiek Tea Factory", "Momul Tea Factory", "Motigo Tea Factory",
                "Olenguruone Tea Factory", "Rorok Tea Factory", "Tebesonik Tea Factory",
                "Tegat Tea Factory", "Tirgaga Tea Factory", "Toror Tea Factory",
                "Chelal Tea Factory"
            };
            tenants.AddRange(CreateFactories(region5Factories, 5, regions[5]));

            // Region 6 Factories (Kisii & Nyamira)
            var region6Factories = new[]
            {
                "Eberege Tea Factory", "Gianchore Tea Factory", "Itumbe Tea Factory",
                "Kebirigo Tea Factory", "Kiamokama Tea Factory", "Matunwa Tea Factory",
                "Nyamache Tea Factory", "Nyankoba Tea Factory", "Nyansiongo Tea Factory",
                "Ogembo Tea Factory", "Rianyamwamu Tea Factory", "Sanganyi Tea Factory",
                "Sombogo Tea Factory", "Tombe Tea Factory"
            };
            tenants.AddRange(CreateFactories(region6Factories, 6, regions[6]));

            // Region 7 Factories (Nandi, Trans Nzoia & Vihiga)
            var region7Factories = new[]
            {
                "Chebut Tea Factory", "Kapsara Tea Factory", "Kaptumo Tea Factory",
                "Mudete Tea Factory"
            };
            tenants.AddRange(CreateFactories(region7Factories, 7, regions[7]));

            // 3. SUBSIDIARIES
            var subsidiaries = new Dictionary<string, string>
            {
                { "SUB001", "KTDA Management Services (KTDA MS)" },
                { "SUB002", "Kenya Tea Packers (KETEPA) Limited" },
                { "SUB003", "Chai Trading Company Limited" },
                { "SUB004", "Greenland Fedha Limited" },
                { "SUB005", "Majani Insurance Brokers Limited" },
                { "SUB006", "KTDA Power Company Limited" },
                { "SUB007", "Tea Machinery and Engineering Company (TEMEC) Limited" },
                { "SUB008", "KTDA Foundation" },
                { "SUB009", "Chai Logistics Centre" }
            };

            foreach (var subsidiary in subsidiaries)
            {
                tenants.Add(new Tenant
                {
                    TenantType = "Subsidiary",
                    TenantCode = subsidiary.Key,
                    TenantName = subsidiary.Value,
                    RegionId = null,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });
            }

            context.Tenants.AddRange(tenants);
            context.SaveChanges();
        }

        /// <summary>
        /// Helper method to create factory tenants
        /// </summary>
        private static List<Tenant> CreateFactories(string[] factoryNames, int regionNumber, int regionId)
        {
            var factories = new List<Tenant>();
            int counter = 1;

            foreach (var factoryName in factoryNames)
            {
                // Generate factory code: FAC-R1-001, FAC-R1-002, etc.
                string factoryCode = $"FAC-R{regionNumber}-{counter:D3}";

                factories.Add(new Tenant
                {
                    TenantType = "Factory",
                    TenantCode = factoryCode,
                    TenantName = factoryName,
                    RegionId = regionId,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                });

                counter++;
            }

            return factories;
        }
    }
}
