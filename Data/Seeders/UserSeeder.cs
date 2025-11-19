using FormReporting.Models.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds initial user accounts including SuperAdmin, System Admin, and ICT personnel
    /// </summary>
    public static class UserSeeder
    {
        private const string DEFAULT_PASSWORD = "ktda.123";
        private const string SYSADMIN_PASSWORD = "ktda@2026!";
        private static int _employeeCounter = 1; // Counter for sequential employee numbers (SAP00001, SAP00002, etc.)

        /// <summary>
        /// Seeds users if they don't already exist
        /// Requires Roles, Regions, and Tenants to be seeded first
        /// </summary>
        public static void SeedUsers(ApplicationDbContext context)
        {
            // Check if users already exist
            if (context.Users.Any())
            {
                return; // Data already seeded
            }

            // Initialize password hasher
            var hasher = new PasswordHasher<User>();

            // Seed users in order
            SeedSuperAdmin(context, hasher);
            SeedSystemAdmin(context, hasher);
            SeedHeadOfficeICT(context, hasher);
            SeedRegionalICTManagers(context, hasher);
            SeedFactoryICTOfficers(context, hasher);

            context.SaveChanges();
        }

        #region SuperAdmin and System Admin

        /// <summary>
        /// Creates the SuperAdmin user (system owner with unrestricted access)
        /// </summary>
        private static void SeedSuperAdmin(ApplicationDbContext context, PasswordHasher<User> hasher)
        {
            // Get Head Office tenant ID
            var headOfficeTenantId = context.Tenants
                .First(t => t.TenantCode == "HO001").TenantId;

            var superAdmin = new User
            {
                UserName = "superadmin",
                NormalizedUserName = "SUPERADMIN",
                Email = "superadmin@ktda.com",
                NormalizedEmail = "SUPERADMIN@KTDA.COM",
                EmailConfirmed = true,
                FirstName = "Super",
                LastName = "Administrator",
                EmployeeNumber = "SA-001",
                TenantId = headOfficeTenantId,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.UtcNow
            };

            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, SYSADMIN_PASSWORD);
            context.Users.Add(superAdmin);
            context.SaveChanges();

            // Note: SuperAdmin doesn't need a role - access is hardcoded in authorization logic
        }

        /// <summary>
        /// Creates the System Administrator user with SYSTEM_ADMIN role
        /// </summary>
        private static void SeedSystemAdmin(ApplicationDbContext context, PasswordHasher<User> hasher)
        {
            // Get Head Office tenant ID
            var headOfficeTenantId = context.Tenants
                .First(t => t.TenantCode == "HO001").TenantId;

            var sysAdmin = new User
            {
                UserName = "sysadmin",
                NormalizedUserName = "SYSADMIN",
                Email = "sysadmin@ktda.com",
                NormalizedEmail = "SYSADMIN@KTDA.COM",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                EmployeeNumber = "SA-002",
                TenantId = headOfficeTenantId,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.UtcNow
            };

            sysAdmin.PasswordHash = hasher.HashPassword(sysAdmin, SYSADMIN_PASSWORD);
            context.Users.Add(sysAdmin);
            context.SaveChanges();

            // Assign SYSTEM_ADMIN role
            var systemAdminRole = context.Roles.First(r => r.RoleCode == "SYSTEM_ADMIN");
            var userRole = new UserRole
            {
                UserId = sysAdmin.UserId,
                RoleId = systemAdminRole.RoleId,
                AssignedDate = DateTime.UtcNow
            };
            context.UserRoles.Add(userRole);
            context.SaveChanges();
        }

        #endregion

        #region Head Office ICT Staff

        /// <summary>
        /// Seeds Head Office ICT personnel from all departments
        /// </summary>
        private static void SeedHeadOfficeICT(ApplicationDbContext context, PasswordHasher<User> hasher)
        {
            // Get Head Office tenant ID
            var headOfficeTenantId = context.Tenants
                .First(t => t.TenantCode == "HO001").TenantId;

            var hoIctRole = context.Roles.First(r => r.RoleCode == "HO_ICT_MGR");
            var employeeRole = context.Roles.First(r => r.RoleCode == "EMPLOYEE");
            var systemAdminRole = context.Roles.First(r => r.RoleCode == "SYSTEM_ADMIN");
            var helpdeskRole = context.Roles.First(r => r.RoleCode == "HELPDESK");

            var headOfficeStaff = new List<(string Name, string Department, string Designation, string RoleCode)>
            {
                // ICT-O (Office)
                ("Martin Mwarangu", "ICT-O", "Group General Manager", "HO_ICT_MGR"),
                ("Sheila Maina", "ICT-O", "Admin Assistant", "HO_ICT_MGR"),

                // ICT-S (Services)
                ("Rogers Nyamweya", "ICT-S", "ICT Services Coordinator", "HELPDESK"),
                ("Agnes W. Mwangi", "ICT-S", "ICT Services Coordinator", "HELPDESK"),
                ("Beatrice Kairo", "ICT-S", "ICT Services Coordinator", "HELPDESK"),
                ("Rhoda Sainah", "ICT-S", "ICT Services Coordinator", "HELPDESK"),
                ("Denis Nganga", "ICT-S", "ICT Services Coordinator", "HELPDESK"),
                ("Kelvin Munene", "ICT-S", "ICT Services Coordinator", "HELPDESK"),
                ("Philemon Langat", "ICT-S", "ICT Services Coordinator", "HELPDESK"),

                // ICT-I (Infrastructure)
                ("Edwin Mwosa", "ICT-I", "Head of Infrastructure and Services", "HO_ICT_MGR"),
                ("Samuel Kinyua", "ICT-I", "ICT Hardware Engineer", "HELPDESK"),
                ("Joseph Ndungu", "ICT-I", "ICT Hardware Engineer", "HELPDESK"),
                ("Antony Wanjau", "ICT-I", "ICT Hardware Engineer", "HELPDESK"),
                ("John Maroko", "ICT-I", "ICT Hardware Engineer", "HELPDESK"),
                ("Winfred Kabuuri", "ICT-I", "Network Support Coordinator", "HELPDESK"),
                ("Stephen Michino", "ICT-I", "Network Support Coordinator", "HELPDESK"),
                ("Charles Bett", "ICT-I", "Network Support Coordinator", "HELPDESK"),
                ("Brenda Mucheru", "ICT-I", "ICT Security Coordinator", "HELPDESK"),
                ("Veronica Nderitu", "ICT-I", "ICT Security Coordinator", "HELPDESK"),
                ("Ben Thuo", "ICT-I", "ICT Security Coordinator", "HELPDESK"),
                ("Florence Barasa", "ICT-I", "ICT Security Coordinator", "HELPDESK"),

                // ICT-B (Business Systems)
                ("Ken Nyaribo", "ICT-B", "Head of Business systems", "HO_ICT_MGR"),
                ("Felix Mugambi", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Antony Kuria", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Martin Kariuki", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Samuel Ngambi", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Simon Kiragu", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Paul Gathogo", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Brian Mochama", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Ezra Kungu", "ICT-B", "Systems Developer", "SYSTEM_ADMIN"),
                ("Elias Mugo", "ICT-B", "Systems Developer", "SYSTEM_ADMIN")
            };

            foreach (var staff in headOfficeStaff)
            {
                var user = CreateUser(staff.Name, headOfficeTenantId, hasher);
                context.Users.Add(user);
                context.SaveChanges();

                // Assign role
                var role = context.Roles.First(r => r.RoleCode == staff.RoleCode);
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId,
                    AssignedDate = DateTime.UtcNow
                };
                context.UserRoles.Add(userRole);
            }

            context.SaveChanges();
        }

        #endregion

        #region Regional ICT Managers

        /// <summary>
        /// Seeds Regional ICT Managers (one per region)
        /// </summary>
        private static void SeedRegionalICTManagers(ApplicationDbContext context, PasswordHasher<User> hasher)
        {
            // Get Head Office tenant ID - Regional managers are HO staff managing regions
            var headOfficeTenantId = context.Tenants
                .First(t => t.TenantCode == "HO001").TenantId;

            var regionalIctRole = context.Roles.First(r => r.RoleCode == "REGIONAL_ICT");

            var regionalManagers = new List<(string Name, int RegionNumber)>
            {
                ("Peter Kibe", 1),
                ("Benjamin K. Ndungu", 2),
                ("Eric Kinyeki", 3),
                ("Jackson Gachuki", 4),
                ("Enock O. Ogara", 5),
                ("Richard S. Olume", 6),
                ("Eric Ngetich", 7)
            };

            foreach (var manager in regionalManagers)
            {
                var user = CreateUser(manager.Name, headOfficeTenantId, hasher);
                context.Users.Add(user);
                context.SaveChanges();

                // Assign REGIONAL_ICT role
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = regionalIctRole.RoleId,
                    AssignedDate = DateTime.UtcNow
                };
                context.UserRoles.Add(userRole);
            }

            context.SaveChanges();
        }

        #endregion

        #region Factory ICT Officers

        /// <summary>
        /// Seeds Factory ICT Officers across all regions
        /// </summary>
        private static void SeedFactoryICTOfficers(ApplicationDbContext context, PasswordHasher<User> hasher)
        {
            var factoryIctRole = context.Roles.First(r => r.RoleCode == "FACTORY_ICT");

            var factoryOfficers = new List<(string Name, string Factory)>
            {
                // REGION 1
                ("Mercy N. Gitari", "Kambaa"),
                ("Andrew Kariuki", "Kagwe"),
                ("Beth N. Muiruri", "Theta"),
                ("Jackline Kerubo", "Ndarugu"),
                ("Samuel Muchiri", "Gachege"),
                ("Cecilia Maina", "Mataara"),
                ("Anne Muriithi", "Ngere"),
                ("George Mau", "Makomboki"),
                ("Diana Opondo", "Nduti"),
                ("Mary Ndichu", "Ikumbi"),
                ("John F Mathenge", "Gacharage"),

                // REGION 2
                ("Paul Makori", "Kanyenyaini"),
                ("Ann Maina", "Chinga"),
                ("Nicholas Nyamu Muraguri", "Kiru"),
                ("David Njoroge Githinji", "Githambo"),
                ("Mikenathan Ngugi", "Gatunguru"),
                ("Daniel M. Muigai", "Ragati"),
                ("Dedan Kariuki Kamuto", "Gitugi"),
                ("Philip Murage Kinyua", "Iriaini"),
                ("Vincent Koech", "Gathuthi"),

                // REGION 3
                ("Gideon Rotich", "Ndima"),
                ("Zipporah Karimi", "Mununga"),
                ("Elizabeth Ndegwa", "Kangaita"),
                ("Millicent Kagunda", "Kimunye"),
                ("Michael Thoronjo", "Thumaita"),
                ("Dickson Kariuki", "Kathangariri"),
                ("Gilbert Mwaniki", "Mungania"),
                ("Joseph Njogu", "Rukuriri"),

                // REGION 4
                ("David Mwangi", "Weru"),
                ("George Kinyua", "Kinoro"),
                ("Simon Thuku", "Imenti"),
                ("Ezekiel Leting", "Githongo"),
                ("Timothy T Kajuki", "Michimikuru"),
                ("John Muthamia", "Kiegoi"),

                // REGION 5
                ("Vincent Onsongo", "Kapkatet"),
                ("Wilbon Yegon", "Tegat"),
                ("Vincent Ruto", "Litein"),
                ("Andrew Tonui", "Toror"),
                ("Lonah Jerono", "Momul"),
                ("Eric Machogu", "Kapset"),
                ("Fred Kegoro", "Mogogosiek"),
                ("Beatrice Bii", "Kobel"),
                ("Nicholas Eddy", "Tirgaga"),
                ("Alexander Kerich", "Motigo"),
                ("Philemon Langat", "Olenguruone"),

                // REGION 6
                ("Cyrus Vogg", "Rianyamwamu"),
                ("Preston Nyaundi", "Kiamokama"),
                ("John Sang", "Ogembo Group"),
                ("Nicodemus Siro", "Kebirigo"),
                ("James Ogamba", "Nyankoba"),
                ("Loice Mecheo", "Itumbe"),
                ("Cecilia Chemutai", "Gianchore"),
                ("Steve Kendagor", "Nyamache"),
                ("Peter Kibet", "Sanganyi"),
                ("Polycarp Okora", "Nyansiongo Group"),
                ("Peter Langat", "Tombe Group"),

                // REGION 7
                ("Anthony Getugi", "Mudete"),
                ("Enock Kosgey", "Kaptumo"),
                ("Linet Barus", "Kapsara")
            };

            foreach (var officer in factoryOfficers)
            {
                // Lookup factory tenant by name (extract factory name from full name if it includes " Tea Factory")
                var factorySearchName = officer.Factory.Replace(" Tea Factory", "").Replace(" Group", "").Trim();
                var tenant = context.Tenants
                    .FirstOrDefault(t => t.TenantName.Contains(factorySearchName) && t.TenantType == "Factory");

                // If factory not found, skip this user (prevents seeding errors)
                if (tenant == null)
                {
                    Console.WriteLine($"WARNING: Factory tenant not found for {officer.Factory}. Skipping user {officer.Name}");
                    continue;
                }

                var user = CreateUser(officer.Name, tenant.TenantId, hasher);
                context.Users.Add(user);
                context.SaveChanges();

                // Assign FACTORY_ICT role
                var userRole = new UserRole
                {
                    UserId = user.UserId,
                    RoleId = factoryIctRole.RoleId,
                    AssignedDate = DateTime.UtcNow
                };
                context.UserRoles.Add(userRole);
            }

            context.SaveChanges();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a user object from name and tenant ID
        /// </summary>
        private static User CreateUser(string fullName, int tenantId, PasswordHasher<User> hasher)
        {
            var (firstName, lastName) = ParseName(fullName);
            var username = GenerateUsername(firstName, lastName);
            var email = GenerateEmail(username);

            var user = new User
            {
                UserName = username,
                NormalizedUserName = username.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                EmailConfirmed = false, // Users must confirm email on first login
                FirstName = firstName,
                LastName = lastName,
                EmployeeNumber = GenerateEmployeeNumber(), // Sequential: SAP00001, SAP00002, etc.
                TenantId = tenantId,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.UtcNow
            };

            user.PasswordHash = hasher.HashPassword(user, DEFAULT_PASSWORD);
            return user;
        }

        /// <summary>
        /// Parses full name into first and last name
        /// </summary>
        private static (string FirstName, string LastName) ParseName(string fullName)
        {
            var parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 1)
            {
                return (parts[0], parts[0]);
            }
            else if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }
            else
            {
                // For names with middle names/initials (e.g., "Mercy N. Gitari")
                var firstName = parts[0];
                var lastName = parts[parts.Length - 1];
                return (firstName, lastName);
            }
        }

        /// <summary>
        /// Generates username from first and last name
        /// Pattern: firstname.lastname (lowercase)
        /// </summary>
        private static string GenerateUsername(string firstName, string lastName)
        {
            var cleanFirstName = firstName.ToLower().Replace(".", "").Trim();
            var cleanLastName = lastName.ToLower().Replace(".", "").Trim();
            return $"{cleanFirstName}.{cleanLastName}";
        }

        /// <summary>
        /// Generates email from username
        /// Pattern: username@ktda.com
        /// </summary>
        private static string GenerateEmail(string username)
        {
            return $"{username}@ktda.com";
        }

        /// <summary>
        /// Generates sequential employee number
        /// Pattern: SAP00001, SAP00002, SAP00003, etc.
        /// SuperAdmin (SA-001) and SysAdmin (SA-002) are hardcoded separately
        /// </summary>
        private static string GenerateEmployeeNumber()
        {
            var employeeNumber = $"SAP{_employeeCounter:D5}";
            _employeeCounter++;
            return employeeNumber;
        }

        #endregion
    }
}
