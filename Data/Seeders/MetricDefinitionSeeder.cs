using FormReporting.Models.Entities.Metrics;
using Microsoft.EntityFrameworkCore;

namespace FormReporting.Data.Seeders
{
    /// <summary>
    /// Seeds predefined KTDA metric definitions
    /// </summary>
    public static class MetricDefinitionSeeder
    {
        public static void SeedMetricDefinitions(ApplicationDbContext context)
        {
            // Skip if metrics already exist
            if (context.MetricDefinitions.Any())
                return;

            var metrics = new List<MetricDefinition>
            {
                // ===================================================================
                // HARDWARE METRICS
                // ===================================================================
                new MetricDefinition
                {
                    
                    MetricCode = "TOTAL_COMPUTERS",
                    MetricName = "Total Computers per Factory",
                    Category = "Hardware",
                    Description = "Total number of computers in factory inventory",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                        
                    MetricCode = "OPERATIONAL_COMPUTERS",
                    MetricName = "Operational Computers",
                    Category = "Hardware",
                    Description = "Number of computers that are currently operational",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                        
                    MetricCode = "COMPUTER_AVAILABILITY_PCT",
                    MetricName = "Computer Availability Percentage",
                    Category = "Hardware",
                    Description = "Percentage of computers operational vs total inventory",
                    SourceType = "SystemCalculated",
                    DataType = "Percentage",
                    Unit = "Percentage",
                    AggregationType = "AVG",
                    IsKPI = true,
                    ThresholdGreen = 95.0m,
                    ThresholdYellow = 85.0m,
                    ThresholdRed = 85.0m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "TOTAL_PRINTERS",
                    MetricName = "Total Printers",
                    Category = "Hardware",
                    Description = "Total number of printers in factory",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "OPERATIONAL_PRINTERS",
                    MetricName = "Operational Printers",
                    Category = "Hardware",
                    Description = "Number of printers currently operational",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "PRINTER_AVAILABILITY_PCT",
                    MetricName = "Printer Availability Percentage",
                    Category = "Hardware",
                    Description = "Percentage of printers operational vs total",
                    SourceType = "SystemCalculated",
                    DataType = "Percentage",
                    Unit = "Percentage",
                    AggregationType = "AVG",
                    IsKPI = true,
                    ThresholdGreen = 90.0m,
                    ThresholdYellow = 75.0m,
                    ThresholdRed = 75.0m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // NETWORK METRICS
                // ===================================================================
                new MetricDefinition
                {
                    
                    MetricCode = "LAN_STATUS",
                    MetricName = "LAN Network Operational Status",
                    Category = "Network",
                    Description = "Is the LAN network operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "WAN_STATUS",
                    MetricName = "WAN Network Operational Status",
                    Category = "Network",
                    Description = "Is the WAN network operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "WAN_TYPE",
                    MetricName = "WAN Connection Type",
                    Category = "Network",
                    Description = "Type of WAN connection (Fiber/Microwave/Hybrid)",
                    SourceType = "UserInput",
                    DataType = "Text",
                    Unit = "Text",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "NETWORK_UPTIME_PCT",
                    MetricName = "Network Uptime Percentage",
                    Category = "Network",
                    Description = "Percentage of time network was available",
                    SourceType = "ExternalSystem",
                    DataType = "Percentage",
                    Unit = "Percentage",
                    AggregationType = "AVG",
                    IsKPI = true,
                    ThresholdGreen = 99.0m,
                    ThresholdYellow = 95.0m,
                    ThresholdRed = 95.0m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // SOFTWARE & COMPLIANCE METRICS
                // ===================================================================
                new MetricDefinition
                {
                     
                    MetricCode = "CHAIPRO_VERSION",
                    MetricName = "Chaipro Financials Version",
                    Category = "Software",
                    Description = "Current version of Chaipro Financials installed",
                    SourceType = "UserInput",
                    DataType = "Text",
                    Unit = "Version",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "BACKUP_STATUS",
                    MetricName = "Backup Status",
                    Category = "Compliance",
                    Description = "Is backup system operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "BACKUP_COMPLIANCE",
                    MetricName = "Backup Compliance",
                    Category = "Compliance",
                    Description = "Is backup policy being followed?",
                    SourceType = "BinaryCompliance",
                    DataType = "Percentage",
                    Unit = "Percentage",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 100.0m,
                    ThresholdYellow = 50.0m,
                    ThresholdRed = 50.0m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "FIREWALL_STATUS",
                    MetricName = "Firewall Status",
                    Category = "Security",
                    Description = "Is firewall operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "ANTIVIRUS_STATUS",
                    MetricName = "Antivirus Status",
                    Category = "Security",
                    Description = "Is antivirus up-to-date and operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // REPORT COMPLIANCE METRICS
                // ===================================================================
                new MetricDefinition
                {
                  
                    MetricCode = "REPORT_SUBMISSION_TIMELINESS",
                    MetricName = "Report Submission Timeliness",
                    Category = "Compliance",
                    Description = "Was report submitted by deadline (5th of month)?",
                    SourceType = "ComplianceTracking",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "TRUE",
                    ComplianceRule = "{\"type\": \"deadline\", \"daysAfterPeriodEnd\": 5}",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // LICENSE METRICS
                // ===================================================================
                new MetricDefinition
                {
                    
                    MetricCode = "LICENSE_COMPLIANCE",
                    MetricName = "License Compliance Status",
                    Category = "Compliance",
                    Description = "Are all software licenses valid?",
                    SourceType = "AutomatedCheck",
                    DataType = "Percentage",
                    Unit = "Percentage",
                    IsKPI = true,
                    ThresholdGreen = 100.0m,
                    ThresholdYellow = 90.0m,
                    ThresholdRed = 90.0m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // EMAIL & COMMUNICATION METRICS
                // ===================================================================
                new MetricDefinition
                {
                    
                    MetricCode = "EMAIL_SERVER_STATUS",
                    MetricName = "Email Server Status",
                    Category = "Infrastructure",
                    Description = "Is the email server operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                   
                    MetricCode = "PBX_STATUS",
                    MetricName = "PBX Connection Status",
                    Category = "Infrastructure",
                    Description = "Is PBX connection (PBD, PSM) operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // BUSINESS SYSTEMS METRICS (EWS, CHAIPRO)
                // ===================================================================
                new MetricDefinition
                {
                    
                    MetricCode = "EWS_STATUS",
                    MetricName = "Electronic Weighment Solution Status",
                    Category = "Software",
                    Description = "Is EWS operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "EWS_VERSION",
                    MetricName = "Electronic Weighment Solution Version",
                    Category = "Software",
                    Description = "Current EWS version installed",
                    SourceType = "UserInput",
                    DataType = "Text",
                    Unit = "Version",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                     
                    MetricCode = "CHAIPRO_STATUS",
                    MetricName = "Chaipro System Status",
                    Category = "Software",
                    Description = "Overall Chaipro system operational status",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "CHAIPRO_FACTORY_TVAP_STATUS",
                    MetricName = "Chaipro Factory TVAP Status",
                    Category = "Software",
                    Description = "Is Chaipro Factory TVAP functional?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                   
                    MetricCode = "CHAIPRO_CAMERAS_STATUS",
                    MetricName = "Chaipro Cameras Status",
                    Category = "Software",
                    Description = "Are Chaipro cameras functional?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "NFC_CARD_USAGE_PCT",
                    MetricName = "NFC Card Usage Percentage",
                    Category = "Software",
                    Description = "Percentage of NFC card usage at factory",
                    SourceType = "UserInput",
                    DataType = "Percentage",
                    Unit = "Percentage",
                    AggregationType = "AVG",
                    IsKPI = true,
                    ThresholdGreen = 80.0m,
                    ThresholdYellow = 60.0m,
                    ThresholdRed = 60.0m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // SURVEILLANCE & MONITORING METRICS
                // ===================================================================
                new MetricDefinition
                {
                    
                    MetricCode = "CCTV_STATUS",
                    MetricName = "CCTV System Status",
                    Category = "Security",
                    Description = "Is CCTV system operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "WEIGHBRIDGE_STATUS",
                    MetricName = "Weighbridge Status",
                    Category = "Hardware",
                    Description = "Is weighbridge operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "AFMS_STATUS",
                    MetricName = "AFMS System Status",
                    Category = "Software",
                    Description = "Is AFMS (Advanced Factory Management System) operational?",
                    SourceType = "UserInput",
                    DataType = "Boolean",
                    Unit = "Status",
                    ExpectedValue = "Yes",
                    IsKPI = true,
                    ThresholdGreen = 1.0m,
                    ThresholdYellow = 0.5m,
                    ThresholdRed = 0.5m,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // ===================================================================
                // HARDWARE INVENTORY METRICS
                // ===================================================================
                new MetricDefinition
                {
                    
                    MetricCode = "TOTAL_LAPTOPS",
                    MetricName = "Total Laptops",
                    Category = "Hardware",
                    Description = "Total number of laptops",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "TOTAL_TABLETS",
                    MetricName = "Total Tablets",
                    Category = "Hardware",
                    Description = "Total number of tablets",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "TOTAL_UPS",
                    MetricName = "Total UPS Units",
                    Category = "Hardware",
                    Description = "Total number of UPS units",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "TOTAL_SERVERS",
                    MetricName = "Total Servers",
                    Category = "Hardware",
                    Description = "Total number of servers (physical and virtual)",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "TOTAL_NETWORK_SWITCHES",
                    MetricName = "Total Network Switches",
                    Category = "Hardware",
                    Description = "Total number of network switches",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                new MetricDefinition
                {
                    
                    MetricCode = "TOTAL_ROUTERS",
                    MetricName = "Total Routers",
                    Category = "Hardware",
                    Description = "Total number of routers",
                    SourceType = "UserInput",
                    DataType = "Integer",
                    Unit = "Count",
                    AggregationType = "SUM",
                    IsKPI = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };

            context.MetricDefinitions.AddRange(metrics);
            context.SaveChanges();
        }
    }
}
