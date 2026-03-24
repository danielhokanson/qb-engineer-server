using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;
using Serilog;

namespace QBEngineer.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        // ── 1. Roles ──────────────────────────────────────────────────────
        string[] roles = ["Admin", "Manager", "Engineer", "PM", "ProductionWorker", "OfficeManager"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
        }

        // ── 2. Admin user ─────────────────────────────────────────────────
        var admin = await EnsureUserAsync(userManager, "admin@qbengineer.local",
            "Admin", "User", "AU", "#0d9488", "Admin123!", "Admin");

        // ── 3. Team members ───────────────────────────────────────────────
        var akim = await EnsureUserAsync(userManager, "akim@qbengineer.local",
            "A.", "Kim", "AK", "#0d9488", "Engineer123!", "Engineer");

        var dhart = await EnsureUserAsync(userManager, "dhart@qbengineer.local",
            "D.", "Hart", "DH", "#7c3aed", "Engineer123!", "Engineer");

        var jsilva = await EnsureUserAsync(userManager, "jsilva@qbengineer.local",
            "J.", "Silva", "JS", "#c2410c", "Engineer123!", "Engineer");

        var mreyes = await EnsureUserAsync(userManager, "mreyes@qbengineer.local",
            "M.", "Reyes", "MR", "#15803d", "Engineer123!", "Engineer");

        await db.SaveChangesAsync();

        // ── 4. Track Types, Stages & Reference Data ────────────────────
        await SeedEssentialDataAsync(db);

        // ── 5. Customers ──────────────────────────────────────────────────
        if (!await db.Customers.AnyAsync())
        {
            db.Customers.AddRange(
                new Customer { Name = "Acme Corp" },
                new Customer { Name = "Quantum Dynamics" },
                new Customer { Name = "Apex Manufacturing" },
                new Customer { Name = "Meridian Systems" }
            );
            await db.SaveChangesAsync();
            Log.Information("Seeded customers");
        }

        // ── 6. Jobs ───────────────────────────────────────────────────────
        if (!await db.Jobs.AnyAsync())
        {
            // Look up production track and stages
            var prodTrack = await db.TrackTypes
                .FirstAsync(t => t.Code == "production");

            var stages = await db.JobStages
                .Where(s => s.TrackTypeId == prodTrack.Id)
                .ToDictionaryAsync(s => s.Code);

            var maintTrack = await db.TrackTypes
                .FirstAsync(t => t.Code == "maintenance");

            var maintStages = await db.JobStages
                .Where(s => s.TrackTypeId == maintTrack.Id)
                .ToDictionaryAsync(s => s.Code);

            var customers = await db.Customers.ToListAsync();
            var acme = customers.First(c => c.Name == "Acme Corp");
            var quantum = customers.First(c => c.Name == "Quantum Dynamics");
            var apex = customers.First(c => c.Name == "Apex Manufacturing");
            var meridian = customers.First(c => c.Name == "Meridian Systems");

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // Helper
            int pos = 0;
            Job MakeJob(string number, string title, int trackTypeId, int stageId,
                int? assigneeId = null, int? customerId = null, DateTime? dueDate = null,
                JobPriority priority = JobPriority.Normal)
            {
                return new Job
                {
                    JobNumber = number,
                    Title = title,
                    TrackTypeId = trackTypeId,
                    CurrentStageId = stageId,
                    AssigneeId = assigneeId,
                    CustomerId = customerId,
                    DueDate = dueDate,
                    Priority = priority,
                    BoardPosition = ++pos,
                };
            }

            var jobs = new List<Job>
            {
                // ── Dashboard task list jobs (12 from mock) ───────────────
                // Active tasks
                MakeJob("J-1042", "CNC setup — Bracket Assy Rev C", prodTrack.Id, stages["in_production"].Id,
                    akim.Id, apex.Id, today.AddDays(1)),
                MakeJob("J-1038", "Material prep — Shaft Housing", prodTrack.Id, stages["materials_received"].Id,
                    dhart.Id, quantum.Id, today.AddDays(4)),
                MakeJob("J-1035", "QC inspection — Motor Mount v2", prodTrack.Id, stages["qc_review"].Id,
                    jsilva.Id, acme.Id, today),

                // Next tasks
                MakeJob("J-1041", "Weld fixture alignment check", prodTrack.Id, stages["in_production"].Id,
                    mreyes.Id, meridian.Id),
                MakeJob("J-1039", "Program verify — Plate Adapter", prodTrack.Id, stages["in_production"].Id,
                    akim.Id, apex.Id),
                MakeJob("J-1033", "Deburr & finish — Gear Blank", prodTrack.Id, stages["in_production"].Id,
                    dhart.Id, quantum.Id, yesterday, JobPriority.High),
                MakeJob("J-1040", "First article inspection — Flange", prodTrack.Id, stages["qc_review"].Id,
                    mreyes.Id, acme.Id),
                MakeJob("J-1036", "Assembly — Pneumatic Manifold", prodTrack.Id, stages["in_production"].Id,
                    akim.Id, meridian.Id, today.AddDays(6)),
                MakeJob("J-1037", "Surface grind — Dowel Plate", prodTrack.Id, stages["in_production"].Id,
                    dhart.Id, apex.Id),
                MakeJob("J-1044", "Anodize prep — Heat Sink Array", prodTrack.Id, stages["materials_ordered"].Id,
                    jsilva.Id, quantum.Id),
                MakeJob("J-1045", "Drill & tap — Mounting Block", prodTrack.Id, stages["in_production"].Id,
                    mreyes.Id, meridian.Id),

                // Afternoon tasks from mock
                MakeJob("J-1046", "EDM programming — Die Insert", prodTrack.Id, stages["in_production"].Id,
                    akim.Id, acme.Id),
                MakeJob("J-1034", "Laser mark — Serial plates (50pc)", prodTrack.Id, stages["in_production"].Id,
                    dhart.Id, quantum.Id, today, JobPriority.High),
                MakeJob("J-1047", "Wire EDM — Punch Tool blank", prodTrack.Id, stages["in_production"].Id,
                    akim.Id, apex.Id),
                MakeJob("J-1048", "Tumble finish — Small parts lot", prodTrack.Id, stages["in_production"].Id,
                    dhart.Id, meridian.Id),

                // ── Deadline-specific jobs ────────────────────────────────
                MakeJob("J-1031", "Acme Order — Pack & ship", prodTrack.Id, stages["shipped"].Id,
                    mreyes.Id, acme.Id, today, JobPriority.High),

                // ── Quoting stage jobs (Quoting=3) ────────────────────────
                MakeJob("J-1050", "Custom bracket quote — Meridian", prodTrack.Id, stages["quote_requested"].Id,
                    null, meridian.Id),
                MakeJob("J-1051", "Prototype enclosure estimate", prodTrack.Id, stages["quoted"].Id,
                    akim.Id, quantum.Id),
                MakeJob("J-1052", "Gear assembly RFQ", prodTrack.Id, stages["quoted"].Id,
                    null, apex.Id),

                // ── Planning stage jobs (Planning=5 total: Order Confirmed) ─
                // (J-1038 Materials Received counts separately)
                MakeJob("J-1053", "Motor housing — Order confirmed", prodTrack.Id, stages["order_confirmed"].Id,
                    dhart.Id, acme.Id),
                MakeJob("J-1054", "Connector plate — Planning", prodTrack.Id, stages["order_confirmed"].Id,
                    jsilva.Id, quantum.Id),
                MakeJob("J-1055", "Valve body — Order confirmed", prodTrack.Id, stages["order_confirmed"].Id,
                    akim.Id, meridian.Id),
                MakeJob("J-1056", "Actuator arm — Order confirmed", prodTrack.Id, stages["order_confirmed"].Id,
                    mreyes.Id, apex.Id),
                MakeJob("J-1057", "Bearing cap — Order confirmed", prodTrack.Id, stages["order_confirmed"].Id,
                    dhart.Id, acme.Id),

                // ── Materials stage jobs (Materials=4 total: Ordered + Received) ─
                // J-1044 is already in materials_ordered, J-1038 in materials_received
                MakeJob("J-1058", "Aluminum stock — Heat Sink", prodTrack.Id, stages["materials_ordered"].Id,
                    jsilva.Id, quantum.Id),
                MakeJob("J-1059", "Steel rod — Shaft blanks", prodTrack.Id, stages["materials_received"].Id,
                    dhart.Id, apex.Id),

                // ── QC stage (QC=3 total) ─────────────────────────────────
                // J-1035 and J-1040 already in qc_review
                MakeJob("J-1030", "QC final — Precision bushing", prodTrack.Id, stages["qc_review"].Id,
                    jsilva.Id, acme.Id),

                // ── Shipping stage (Shipping=2) ───────────────────────────
                // J-1031 already in shipped
                MakeJob("J-1060", "Ship — Quantum Dynamics order", prodTrack.Id, stages["shipped"].Id,
                    mreyes.Id, quantum.Id),

                // ── Complete stage (Complete=8: Invoiced/Sent + Payment Received) ─
                MakeJob("J-1020", "Acme bracket order — Complete", prodTrack.Id, stages["invoiced_sent"].Id,
                    akim.Id, acme.Id),
                MakeJob("J-1021", "Shaft assembly — Paid", prodTrack.Id, stages["payment_received"].Id,
                    dhart.Id, quantum.Id),
                MakeJob("J-1022", "Custom flange — Invoiced", prodTrack.Id, stages["invoiced_sent"].Id,
                    jsilva.Id, apex.Id),
                MakeJob("J-1023", "Dowel pin set — Paid", prodTrack.Id, stages["payment_received"].Id,
                    mreyes.Id, meridian.Id),
                MakeJob("J-1024", "Motor mount batch — Invoiced", prodTrack.Id, stages["invoiced_sent"].Id,
                    akim.Id, acme.Id),
                MakeJob("J-1025", "Precision sleeve — Paid", prodTrack.Id, stages["payment_received"].Id,
                    dhart.Id, quantum.Id),
                MakeJob("J-1026", "Manifold adapter — Invoiced", prodTrack.Id, stages["invoiced_sent"].Id,
                    jsilva.Id, apex.Id),
                MakeJob("J-1027", "Coupling insert — Paid", prodTrack.Id, stages["payment_received"].Id,
                    mreyes.Id, meridian.Id),
            };

            // Maintenance job from mock
            jobs.Add(MakeJob("M-0012", "Machine clean & PM — Haas VF-2", maintTrack.Id, maintStages["scheduled"].Id,
                mreyes.Id));

            db.Jobs.AddRange(jobs);
            await db.SaveChangesAsync();

            // ── Subtask: Tool change on J-1042 ───────────────────────────
            var j1042 = jobs.First(j => j.JobNumber == "J-1042");
            db.JobSubtasks.Add(new JobSubtask
            {
                JobId = j1042.Id,
                Text = "Tool change — End Mill #4 replace",
                AssigneeId = jsilva.Id,
                SortOrder = 1,
            });
            await db.SaveChangesAsync();

            Log.Information("Seeded {Count} jobs", jobs.Count);
        }

        // Reference data is now seeded by SeedEssentialDataAsync above

        // ── 8. Activity Log ──────────────────────────────────────────────
        if (!await db.JobActivityLogs.AnyAsync())
        {
            var now = DateTime.UtcNow;

            // Look up job IDs for activity entries
            var j1042 = await db.Jobs.FirstAsync(j => j.JobNumber == "J-1042");
            var j1044 = await db.Jobs.FirstAsync(j => j.JobNumber == "J-1044");
            var j1038 = await db.Jobs.FirstAsync(j => j.JobNumber == "J-1038");
            var j1030 = await db.Jobs.FirstAsync(j => j.JobNumber == "J-1030");
            var j1034 = await db.Jobs.FirstAsync(j => j.JobNumber == "J-1034");

            db.JobActivityLogs.AddRange(
                new JobActivityLog
                {
                    JobId = j1042.Id,
                    UserId = admin.Id,
                    Action = ActivityAction.StageMoved,
                    FieldName = "CurrentStageId",
                    OldValue = "Materials Received",
                    NewValue = "In Production",
                    Description = "J-1042 moved to Production",
                    CreatedAt = now.AddMinutes(-10),
                },
                new JobActivityLog
                {
                    JobId = j1044.Id,
                    UserId = admin.Id,
                    Action = ActivityAction.Assigned,
                    FieldName = "AssigneeId",
                    NewValue = akim.Id.ToString(),
                    Description = "A. Kim assigned to J-1044",
                    CreatedAt = now.AddMinutes(-25),
                },
                new JobActivityLog
                {
                    JobId = j1038.Id,
                    UserId = dhart.Id,
                    Action = ActivityAction.FieldChanged,
                    FieldName = "Attachment",
                    NewValue = "drawing_v2.pdf",
                    Description = "Drawing uploaded to J-1038",
                    CreatedAt = now.AddHours(-1),
                },
                new JobActivityLog
                {
                    JobId = j1030.Id,
                    UserId = jsilva.Id,
                    Action = ActivityAction.StageMoved,
                    FieldName = "CurrentStageId",
                    OldValue = "In Production",
                    NewValue = "QC/Review",
                    Description = "J-1030 passed QC inspection",
                    CreatedAt = now.AddHours(-2),
                },
                new JobActivityLog
                {
                    JobId = j1034.Id,
                    UserId = null,
                    Action = ActivityAction.FieldChanged,
                    FieldName = "DueDate",
                    Description = "J-1034 overdue — was due yesterday",
                    CreatedAt = now.AddHours(-3),
                }
            );
            await db.SaveChangesAsync();
            Log.Information("Seeded activity log entries");
        }

        // ── 9. Pre-packaged Reports ─────────────────────────────────────
        if (!await db.SavedReports.AnyAsync())
        {
            var adminUser = await userManager.FindByEmailAsync("admin@qbengineer.local");
            var adminId = adminUser!.Id;

            db.SavedReports.AddRange(
                // ── My Reports (user-scoped at runtime, seeded as shared templates) ──
                new SavedReport
                {
                    Name = "My Work History",
                    Description = "Your assigned jobs with stage, customer, and dates",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","Customer.Name","CurrentStage.Name","Priority","DueDate","CompletedDate","CreatedAt"]""",
                    SortField = "CreatedAt", SortDirection = "desc",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "My Time Log",
                    Description = "Your time entries with job and duration details",
                    EntitySource = "TimeEntries",
                    ColumnsJson = """["Date","Job.JobNumber","Job.Title","Category","DurationMinutes","Notes","IsManual"]""",
                    SortField = "Date", SortDirection = "desc",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "My Expense History",
                    Description = "Your expense submissions with status and amounts",
                    EntitySource = "Expenses",
                    ColumnsJson = """["ExpenseDate","Category","Description","Amount","Status","Job.JobNumber"]""",
                    SortField = "ExpenseDate", SortDirection = "desc",
                    IsShared = true, UserId = adminId,
                },
                // ── Job Reports ──
                new SavedReport
                {
                    Name = "Jobs by Stage",
                    Description = "All jobs grouped by their current stage",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","Customer.Name","CurrentStage.Name","Priority","DueDate","TrackType.Name"]""",
                    GroupByField = "CurrentStage.Name",
                    SortField = "CurrentStage.Name", SortDirection = "asc",
                    ChartType = "bar", ChartLabelField = "CurrentStage.Name", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Overdue Jobs",
                    Description = "Jobs past their due date",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","Customer.Name","CurrentStage.Name","Priority","DueDate","TrackType.Name"]""",
                    FiltersJson = """[{"Field":"DueDate","Operator":"LessThan","Value":"now"},{"Field":"CompletedDate","Operator":"IsNull"}]""",
                    SortField = "DueDate", SortDirection = "asc",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Job Completion Trend",
                    Description = "Jobs created vs completed over time",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","CreatedAt","CompletedDate","Customer.Name","TrackType.Name"]""",
                    SortField = "CreatedAt", SortDirection = "desc",
                    ChartType = "line", ChartLabelField = "CreatedAt", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "On-Time Delivery Rate",
                    Description = "Completed jobs analyzed by on-time vs late delivery",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","DueDate","CompletedDate","Customer.Name","CurrentStage.Name"]""",
                    FiltersJson = """[{"Field":"CompletedDate","Operator":"IsNotNull"}]""",
                    SortField = "CompletedDate", SortDirection = "desc",
                    ChartType = "pie", ChartLabelField = "TrackType.Name", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Average Lead Time",
                    Description = "Average time jobs spend in each stage",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","CurrentStage.Name","CreatedAt","CompletedDate","TrackType.Name"]""",
                    GroupByField = "CurrentStage.Name",
                    ChartType = "bar", ChartLabelField = "CurrentStage.Name", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Time in Stage (Bottleneck)",
                    Description = "Identify bottleneck stages where jobs spend the most time",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","CurrentStage.Name","DueDate","CreatedAt","Priority"]""",
                    GroupByField = "CurrentStage.Name",
                    SortField = "CurrentStage.Name", SortDirection = "asc",
                    ChartType = "bar", ChartLabelField = "CurrentStage.Name", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "R&D Reports",
                    Description = "R&D jobs with iterations, hours, and current stage",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","IterationCount","CurrentStage.Name","IsInternal","CreatedAt","CompletedDate"]""",
                    FiltersJson = """[{"Field":"IsInternal","Operator":"Equals","Value":"true"}]""",
                    SortField = "CreatedAt", SortDirection = "desc",
                    IsShared = true, UserId = adminId,
                },
                // ── Team & Labor Reports ──
                new SavedReport
                {
                    Name = "Team Workload",
                    Description = "Active and overdue jobs per team member",
                    EntitySource = "Jobs",
                    ColumnsJson = """["JobNumber","Title","Priority","DueDate","CurrentStage.Name","IsArchived","Customer.Name"]""",
                    FiltersJson = """[{"Field":"IsArchived","Operator":"Equals","Value":"false"}]""",
                    SortField = "Priority", SortDirection = "asc",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Employee Productivity",
                    Description = "Time entries and job completions per employee",
                    EntitySource = "TimeEntries",
                    ColumnsJson = """["Date","Job.JobNumber","Job.Title","Category","DurationMinutes","IsManual"]""",
                    GroupByField = "Category",
                    SortField = "Date", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Category", ChartValueField = "DurationMinutes",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Labor Hours by Job",
                    Description = "Total time entries grouped by job",
                    EntitySource = "TimeEntries",
                    ColumnsJson = """["Job.JobNumber","Job.Title","Date","DurationMinutes","Category","Notes"]""",
                    GroupByField = "Job.JobNumber",
                    SortField = "DurationMinutes", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Job.JobNumber", ChartValueField = "DurationMinutes",
                    IsShared = true, UserId = adminId,
                },
                // ── Financial Reports ──
                new SavedReport
                {
                    Name = "Expense Summary",
                    Description = "Expenses grouped by category with totals",
                    EntitySource = "Expenses",
                    ColumnsJson = """["ExpenseDate","Category","Description","Amount","Status","Job.JobNumber"]""",
                    GroupByField = "Category",
                    SortField = "ExpenseDate", SortDirection = "desc",
                    ChartType = "pie", ChartLabelField = "Category", ChartValueField = "Amount",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Invoice Summary",
                    Description = "All invoices with status, dates, and customer",
                    EntitySource = "Invoices",
                    ColumnsJson = """["InvoiceNumber","Customer.Name","Status","InvoiceDate","DueDate","TaxRate","CreatedAt"]""",
                    GroupByField = "Status",
                    SortField = "InvoiceDate", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Status", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                // ── Customer & Sales Reports ──
                new SavedReport
                {
                    Name = "Customer Activity",
                    Description = "Job activity per customer (active, completed, total)",
                    EntitySource = "Jobs",
                    ColumnsJson = """["Customer.Name","JobNumber","Title","CurrentStage.Name","Priority","DueDate","CompletedDate"]""",
                    GroupByField = "Customer.Name",
                    SortField = "Customer.Name", SortDirection = "asc",
                    ChartType = "bar", ChartLabelField = "Customer.Name", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Quote-to-Close Rate",
                    Description = "Quote conversion analysis — sent vs accepted vs expired",
                    EntitySource = "Quotes",
                    ColumnsJson = """["QuoteNumber","Customer.Name","Status","SentDate","AcceptedDate","ExpirationDate","CreatedAt"]""",
                    GroupByField = "Status",
                    SortField = "CreatedAt", SortDirection = "desc",
                    ChartType = "pie", ChartLabelField = "Status", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Lead Pipeline",
                    Description = "Leads grouped by status through the pipeline",
                    EntitySource = "Leads",
                    ColumnsJson = """["CompanyName","ContactName","Source","Status","FollowUpDate","CreatedAt"]""",
                    GroupByField = "Status",
                    SortField = "CreatedAt", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Status", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Lead & Sales Summary",
                    Description = "Lead sources and conversion metrics",
                    EntitySource = "Leads",
                    ColumnsJson = """["CompanyName","ContactName","Source","Status","Email","Phone","CreatedAt"]""",
                    GroupByField = "Source",
                    SortField = "Source", SortDirection = "asc",
                    ChartType = "bar", ChartLabelField = "Source", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                // ── Inventory & Supply Chain ──
                new SavedReport
                {
                    Name = "Inventory Levels",
                    Description = "Bin content levels by location",
                    EntitySource = "Inventory",
                    ColumnsJson = """["Location.Name","EntityType","Quantity","ReservedQuantity","LotNumber","Status","PlacedAt"]""",
                    GroupByField = "Location.Name",
                    SortField = "Location.Name", SortDirection = "asc",
                    ChartType = "bar", ChartLabelField = "Location.Name", ChartValueField = "Quantity",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Shipping Summary",
                    Description = "Shipments by carrier and status",
                    EntitySource = "Shipments",
                    ColumnsJson = """["ShipmentNumber","SalesOrder.OrderNumber","Carrier","Status","ShippedDate","DeliveredDate","ShippingCost","TrackingNumber"]""",
                    GroupByField = "Carrier",
                    SortField = "ShippedDate", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Carrier", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Purchase Order Summary",
                    Description = "Purchase orders by status and vendor",
                    EntitySource = "PurchaseOrders",
                    ColumnsJson = """["PONumber","Vendor.Name","Status","SubmittedDate","ExpectedDeliveryDate","ReceivedDate"]""",
                    GroupByField = "Status",
                    SortField = "CreatedAt", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Status", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Sales Order Summary",
                    Description = "Sales orders by status and customer",
                    EntitySource = "SalesOrders",
                    ColumnsJson = """["OrderNumber","Customer.Name","Status","ConfirmedDate","RequestedDeliveryDate","CustomerPO"]""",
                    GroupByField = "Status",
                    SortField = "CreatedAt", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Status", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                // ── Operations ──
                new SavedReport
                {
                    Name = "Maintenance Reports",
                    Description = "Asset maintenance, downtime hours, and reliability",
                    EntitySource = "Assets",
                    ColumnsJson = """["Name","AssetType","Status","Location","Manufacturer","CurrentHours","CavityCount","CurrentShotCount","CreatedAt"]""",
                    GroupByField = "AssetType",
                    SortField = "CurrentHours", SortDirection = "desc",
                    ChartType = "bar", ChartLabelField = "Name", ChartValueField = "CurrentHours",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Quality / Scrap Rate",
                    Description = "Parts and jobs with quality inspection outcomes",
                    EntitySource = "Parts",
                    ColumnsJson = """["PartNumber","Description","Status","PartType","Material","Revision","CreatedAt"]""",
                    GroupByField = "Status",
                    SortField = "PartNumber", SortDirection = "asc",
                    ChartType = "pie", ChartLabelField = "Status", ChartValueField = "Id",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Customer List",
                    Description = "All customers with contact information and activity status",
                    EntitySource = "Customers",
                    ColumnsJson = """["Name","CompanyName","Email","Phone","IsActive","CreatedAt"]""",
                    SortField = "Name", SortDirection = "asc",
                    IsShared = true, UserId = adminId,
                },
                new SavedReport
                {
                    Name = "Parts Catalog",
                    Description = "Complete parts listing with revision and stock info",
                    EntitySource = "Parts",
                    ColumnsJson = """["PartNumber","Description","Revision","Status","PartType","Material","MinStockThreshold","ReorderPoint"]""",
                    SortField = "PartNumber", SortDirection = "asc",
                    IsShared = true, UserId = adminId,
                }
            );

            await db.SaveChangesAsync();
            Log.Information("Seeded {Count} pre-packaged reports", 27);
        }

        Log.Information("Database seeding complete");
    }

    public static async Task SeedEssentialDataAsync(AppDbContext db)
    {
        // Track Types & Stages
        if (!await db.TrackTypes.AnyAsync())
        {
            var production = new TrackType
            {
                Name = "Production",
                Code = "production",
                IsDefault = true,
                SortOrder = 1,
            };
            db.TrackTypes.Add(production);
            await db.SaveChangesAsync();

            db.JobStages.AddRange(
                new JobStage { TrackTypeId = production.Id, Name = "Quote Requested", Code = "quote_requested", SortOrder = 1, Color = "#94a3b8" },
                new JobStage { TrackTypeId = production.Id, Name = "Quoted", Code = "quoted", SortOrder = 2, Color = "#0d9488", AccountingDocumentType = AccountingDocumentType.Estimate },
                new JobStage { TrackTypeId = production.Id, Name = "Order Confirmed", Code = "order_confirmed", SortOrder = 3, Color = "#0ea5e9", AccountingDocumentType = AccountingDocumentType.SalesOrder },
                new JobStage { TrackTypeId = production.Id, Name = "Materials Ordered", Code = "materials_ordered", SortOrder = 4, Color = "#8b5cf6", AccountingDocumentType = AccountingDocumentType.PurchaseOrder },
                new JobStage { TrackTypeId = production.Id, Name = "Materials Received", Code = "materials_received", SortOrder = 5, Color = "#a855f7" },
                new JobStage { TrackTypeId = production.Id, Name = "In Production", Code = "in_production", SortOrder = 6, Color = "#f59e0b" },
                new JobStage { TrackTypeId = production.Id, Name = "QC/Review", Code = "qc_review", SortOrder = 7, Color = "#ec4899" },
                new JobStage { TrackTypeId = production.Id, Name = "Shipped", Code = "shipped", SortOrder = 8, Color = "#c2410c", AccountingDocumentType = AccountingDocumentType.Invoice },
                new JobStage { TrackTypeId = production.Id, Name = "Invoiced/Sent", Code = "invoiced_sent", SortOrder = 9, Color = "#dc2626", AccountingDocumentType = AccountingDocumentType.Invoice, IsIrreversible = true },
                new JobStage { TrackTypeId = production.Id, Name = "Payment Received", Code = "payment_received", SortOrder = 10, Color = "#15803d", AccountingDocumentType = AccountingDocumentType.Payment, IsIrreversible = true }
            );
            await db.SaveChangesAsync();

            var rnd = new TrackType { Name = "R&D/Tooling", Code = "rnd", SortOrder = 2 };
            db.TrackTypes.Add(rnd);
            await db.SaveChangesAsync();

            db.JobStages.AddRange(
                new JobStage { TrackTypeId = rnd.Id, Name = "Concept", Code = "concept", SortOrder = 1, Color = "#94a3b8" },
                new JobStage { TrackTypeId = rnd.Id, Name = "Design", Code = "design", SortOrder = 2, Color = "#0d9488" },
                new JobStage { TrackTypeId = rnd.Id, Name = "Prototype", Code = "prototype", SortOrder = 3, Color = "#0ea5e9" },
                new JobStage { TrackTypeId = rnd.Id, Name = "Test", Code = "test", SortOrder = 4, Color = "#f59e0b" },
                new JobStage { TrackTypeId = rnd.Id, Name = "Iterate", Code = "iterate", SortOrder = 5, Color = "#ec4899" },
                new JobStage { TrackTypeId = rnd.Id, Name = "Production Ready", Code = "production_ready", SortOrder = 6, Color = "#15803d" }
            );
            await db.SaveChangesAsync();

            var maintenance = new TrackType { Name = "Maintenance", Code = "maintenance", SortOrder = 3 };
            db.TrackTypes.Add(maintenance);
            await db.SaveChangesAsync();

            db.JobStages.AddRange(
                new JobStage { TrackTypeId = maintenance.Id, Name = "Requested", Code = "requested", SortOrder = 1, Color = "#94a3b8" },
                new JobStage { TrackTypeId = maintenance.Id, Name = "Scheduled", Code = "scheduled", SortOrder = 2, Color = "#0ea5e9" },
                new JobStage { TrackTypeId = maintenance.Id, Name = "In Progress", Code = "in_progress", SortOrder = 3, Color = "#f59e0b" },
                new JobStage { TrackTypeId = maintenance.Id, Name = "Complete", Code = "complete", SortOrder = 4, Color = "#15803d" }
            );
            await db.SaveChangesAsync();

            Log.Information("Seeded track types and stages");
        }

        // Reference Data
        if (!await db.ReferenceData.AnyAsync())
        {
            db.ReferenceData.AddRange(
                new ReferenceData { GroupCode = "job_priority", Code = "low", Label = "Low", SortOrder = 1, Metadata = """{"color":"#94a3b8"}""" },
                new ReferenceData { GroupCode = "job_priority", Code = "normal", Label = "Normal", SortOrder = 2, Metadata = """{"color":"#0d9488"}""" },
                new ReferenceData { GroupCode = "job_priority", Code = "high", Label = "High", SortOrder = 3, Metadata = """{"color":"#f59e0b"}""" },
                new ReferenceData { GroupCode = "job_priority", Code = "urgent", Label = "Urgent", SortOrder = 4, Metadata = """{"color":"#dc2626"}""" },

                new ReferenceData { GroupCode = "contact_role", Code = "primary", Label = "Primary", SortOrder = 1 },
                new ReferenceData { GroupCode = "contact_role", Code = "billing", Label = "Billing", SortOrder = 2 },
                new ReferenceData { GroupCode = "contact_role", Code = "technical", Label = "Technical", SortOrder = 3 },
                new ReferenceData { GroupCode = "contact_role", Code = "shipping", Label = "Shipping", SortOrder = 4 },

                new ReferenceData { GroupCode = "expense_category", Code = "material", Label = "Material", SortOrder = 1 },
                new ReferenceData { GroupCode = "expense_category", Code = "tooling", Label = "Tooling", SortOrder = 2 },
                new ReferenceData { GroupCode = "expense_category", Code = "shipping", Label = "Shipping", SortOrder = 3 },
                new ReferenceData { GroupCode = "expense_category", Code = "travel", Label = "Travel", SortOrder = 4 },
                new ReferenceData { GroupCode = "expense_category", Code = "office", Label = "Office", SortOrder = 5 },
                new ReferenceData { GroupCode = "expense_category", Code = "other", Label = "Other", SortOrder = 6 },

                new ReferenceData { GroupCode = "return_reason", Code = "defective", Label = "Defective", SortOrder = 1 },
                new ReferenceData { GroupCode = "return_reason", Code = "wrong_part", Label = "Wrong Part", SortOrder = 2 },
                new ReferenceData { GroupCode = "return_reason", Code = "damaged_in_shipping", Label = "Damaged in Shipping", SortOrder = 3 },
                new ReferenceData { GroupCode = "return_reason", Code = "customer_error", Label = "Customer Error", SortOrder = 4 },

                new ReferenceData { GroupCode = "lead_source", Code = "referral", Label = "Referral", SortOrder = 1 },
                new ReferenceData { GroupCode = "lead_source", Code = "website", Label = "Website", SortOrder = 2 },
                new ReferenceData { GroupCode = "lead_source", Code = "trade_show", Label = "Trade Show", SortOrder = 3 },
                new ReferenceData { GroupCode = "lead_source", Code = "cold_call", Label = "Cold Call", SortOrder = 4 },
                new ReferenceData { GroupCode = "lead_source", Code = "other", Label = "Other", SortOrder = 5 },

                // Job Workflow Statuses
                new ReferenceData { GroupCode = "job_workflow_status", Code = "job_status_created", Label = "Created", SortOrder = 1 },
                new ReferenceData { GroupCode = "job_workflow_status", Code = "job_status_in_progress", Label = "In Progress", SortOrder = 2 },
                new ReferenceData { GroupCode = "job_workflow_status", Code = "job_status_on_hold", Label = "On Hold", SortOrder = 3 },
                new ReferenceData { GroupCode = "job_workflow_status", Code = "job_status_completed", Label = "Completed", SortOrder = 4 },
                new ReferenceData { GroupCode = "job_workflow_status", Code = "job_status_archived", Label = "Archived", SortOrder = 5 },

                // Job Hold Types
                new ReferenceData { GroupCode = "job_hold_type", Code = "job_hold_material", Label = "Material Hold", SortOrder = 1 },
                new ReferenceData { GroupCode = "job_hold_type", Code = "job_hold_quality", Label = "Quality Hold", SortOrder = 2 },
                new ReferenceData { GroupCode = "job_hold_type", Code = "job_hold_customer", Label = "Customer Hold", SortOrder = 3 },
                new ReferenceData { GroupCode = "job_hold_type", Code = "job_hold_engineering", Label = "Engineering Hold", SortOrder = 4 },

                // Quote Workflow Statuses
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_draft", Label = "Draft", SortOrder = 1 },
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_sent", Label = "Sent", SortOrder = 2 },
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_accepted", Label = "Accepted", SortOrder = 3 },
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_rejected", Label = "Rejected", SortOrder = 4 },
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_expired", Label = "Expired", SortOrder = 5 },

                // Sales Order Workflow Statuses
                new ReferenceData { GroupCode = "so_workflow_status", Code = "so_status_draft", Label = "Draft", SortOrder = 1 },
                new ReferenceData { GroupCode = "so_workflow_status", Code = "so_status_confirmed", Label = "Confirmed", SortOrder = 2 },
                new ReferenceData { GroupCode = "so_workflow_status", Code = "so_status_in_progress", Label = "In Progress", SortOrder = 3 },
                new ReferenceData { GroupCode = "so_workflow_status", Code = "so_status_fulfilled", Label = "Fulfilled", SortOrder = 4 },
                new ReferenceData { GroupCode = "so_workflow_status", Code = "so_status_closed", Label = "Closed", SortOrder = 5 },

                // Purchase Order Workflow Statuses
                new ReferenceData { GroupCode = "po_workflow_status", Code = "po_status_draft", Label = "Draft", SortOrder = 1 },
                new ReferenceData { GroupCode = "po_workflow_status", Code = "po_status_submitted", Label = "Submitted", SortOrder = 2 },
                new ReferenceData { GroupCode = "po_workflow_status", Code = "po_status_partial_received", Label = "Partially Received", SortOrder = 3 },
                new ReferenceData { GroupCode = "po_workflow_status", Code = "po_status_received", Label = "Received", SortOrder = 4 },
                new ReferenceData { GroupCode = "po_workflow_status", Code = "po_status_closed", Label = "Closed", SortOrder = 5 },

                // Asset Hold Types
                new ReferenceData { GroupCode = "asset_hold_type", Code = "asset_hold_maintenance", Label = "Maintenance Due", SortOrder = 1 },
                new ReferenceData { GroupCode = "asset_hold_type", Code = "asset_hold_calibration", Label = "Calibration Expired", SortOrder = 2 },
                new ReferenceData { GroupCode = "asset_hold_type", Code = "asset_hold_repair", Label = "Under Repair", SortOrder = 3 }
            );
            await db.SaveChangesAsync();
            Log.Information("Seeded reference data");
        }

        // State Withholding Forms — all US states with form info + DocuSeal template IDs where pre-loaded
        if (!await db.ReferenceData.AnyAsync(r => r.GroupCode == "state_withholding"))
        {
            db.ReferenceData.AddRange(
                // States with NO income tax — marked as "none" category
                new ReferenceData { GroupCode = "state_withholding", Code = "AK", Label = "Alaska", SortOrder = 1, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "FL", Label = "Florida", SortOrder = 2, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "NV", Label = "Nevada", SortOrder = 3, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "NH", Label = "New Hampshire", SortOrder = 4, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "SD", Label = "South Dakota", SortOrder = 5, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "TN", Label = "Tennessee", SortOrder = 6, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "TX", Label = "Texas", SortOrder = 7, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "WA", Label = "Washington", SortOrder = 8, Metadata = """{"category":"no_tax"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "WY", Label = "Wyoming", SortOrder = 9, Metadata = """{"category":"no_tax"}""" },

                // States that accept federal W-4 only — marked as "federal" category
                new ReferenceData { GroupCode = "state_withholding", Code = "CO", Label = "Colorado", SortOrder = 10, Metadata = """{"category":"federal","formName":"Uses Federal W-4"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "MT", Label = "Montana", SortOrder = 11, Metadata = """{"category":"federal","formName":"Uses Federal W-4"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "NM", Label = "New Mexico", SortOrder = 12, Metadata = """{"category":"federal","formName":"Uses Federal W-4"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "ND", Label = "North Dakota", SortOrder = 13, Metadata = """{"category":"federal","formName":"Uses Federal W-4"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "UT", Label = "Utah", SortOrder = 14, Metadata = """{"category":"federal","formName":"Uses Federal W-4"}""" },

                // States with own forms — pre-loaded in DocuSeal (docuSealTemplateId set)
                new ReferenceData { GroupCode = "state_withholding", Code = "AR", Label = "Arkansas", SortOrder = 15, Metadata = """{"category":"state_form","formName":"AR4EC","sourceUrl":"https://www.dfa.arkansas.gov/images/uploads/incomeTaxOffice/AR4EC.pdf","docuSealTemplateId":3}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "CA", Label = "California", SortOrder = 16, Metadata = """{"category":"state_form","formName":"DE 4","sourceUrl":"https://edd.ca.gov/siteassets/files/pdf_pub_ctr/de4.pdf","docuSealTemplateId":4}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "KS", Label = "Kansas", SortOrder = 17, Metadata = """{"category":"state_form","formName":"K-4","sourceUrl":"https://www.ksrevenue.gov/pdf/k-4.pdf","docuSealTemplateId":5}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "MA", Label = "Massachusetts", SortOrder = 18, Metadata = """{"category":"state_form","formName":"M-4","sourceUrl":"https://www.mass.gov/doc/form-m-4-massachusetts-employees-withholding-exemption-certificate/download","docuSealTemplateId":6}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "NJ", Label = "New Jersey", SortOrder = 19, Metadata = """{"category":"state_form","formName":"NJ-W4","sourceUrl":"https://www.nj.gov/treasury/taxation/pdf/current/njw4.pdf","docuSealTemplateId":7}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "NY", Label = "New York", SortOrder = 20, Metadata = """{"category":"state_form","formName":"IT-2104","sourceUrl":"https://www.tax.ny.gov/pdf/current_forms/it/it2104_fill_in.pdf","docuSealTemplateId":8}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "OR", Label = "Oregon", SortOrder = 21, Metadata = """{"category":"state_form","formName":"OR-W-4","sourceUrl":"https://www.oregon.gov/dor/forms/FormsPubs/form-or-w-4_101-402_2024.pdf","docuSealTemplateId":9}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "PA", Label = "Pennsylvania", SortOrder = 22, Metadata = """{"category":"state_form","formName":"REV-419","sourceUrl":"https://www.revenue.pa.gov/FormsandPublications/FormsforIndividuals/PIT/Documents/rev-419.pdf","docuSealTemplateId":10}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "VA", Label = "Virginia", SortOrder = 23, Metadata = """{"category":"state_form","formName":"VA-4","sourceUrl":"https://www.tax.virginia.gov/sites/default/files/taxforms/withholding/any/va-4-any.pdf","docuSealTemplateId":11}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "VT", Label = "Vermont", SortOrder = 24, Metadata = """{"category":"state_form","formName":"W-4VT","sourceUrl":"https://tax.vermont.gov/sites/tax/files/documents/W-4VT.pdf","docuSealTemplateId":12}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "WI", Label = "Wisconsin", SortOrder = 25, Metadata = """{"category":"state_form","formName":"WT-4","sourceUrl":"https://www.revenue.wi.gov/DOR%20Publications/pb166.pdf","docuSealTemplateId":13}""" },

                // States with own forms — source URLs for PDF extraction pipeline
                new ReferenceData { GroupCode = "state_withholding", Code = "AL", Label = "Alabama", SortOrder = 26, Metadata = """{"category":"state_form","formName":"A-4","sourceUrl":"https://www.revenue.alabama.gov/wp-content/uploads/2017/05/A-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "AZ", Label = "Arizona", SortOrder = 27, Metadata = """{"category":"state_form","formName":"A-4","sourceUrl":"https://azdor.gov/sites/default/files/media/FORM_A-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "CT", Label = "Connecticut", SortOrder = 28, Metadata = """{"category":"state_form","formName":"CT-W4","sourceUrl":"https://portal.ct.gov/-/media/drs/forms/2024/withholdingforms/ct-w4_1224.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "DC", Label = "District of Columbia", SortOrder = 29, Metadata = """{"category":"state_form","formName":"D-4","sourceUrl":"https://otr.cfo.dc.gov/sites/default/files/dc/sites/otr/publication/attachments/2024_D-4_Fill_In.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "DE", Label = "Delaware", SortOrder = 30, Metadata = """{"category":"state_form","formName":"W-4 (DE)","sourceUrl":"https://revenue.delaware.gov/wp-content/uploads/sites/tax/2020/02/Delaware_W4_Employee_Withholding.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "GA", Label = "Georgia", SortOrder = 31, Metadata = """{"category":"state_form","formName":"G-4","sourceUrl":"https://dor.georgia.gov/sites/dor.georgia.gov/files/related_files/document/TSD/Form/2024_G-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "HI", Label = "Hawaii", SortOrder = 32, Metadata = """{"category":"state_form","formName":"HW-4","sourceUrl":"https://files.hawaii.gov/tax/forms/2023/hw4_i.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "IA", Label = "Iowa", SortOrder = 33, Metadata = """{"category":"state_form","formName":"IA W-4","sourceUrl":"https://tax.iowa.gov/sites/default/files/2023-01/IAW4%2844-019%29.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "ID", Label = "Idaho", SortOrder = 34, Metadata = """{"category":"state_form","formName":"ID W-4","sourceUrl":"https://tax.idaho.gov/wp-content/uploads/forms/EFO00307/EFO00307_04-28-2025.pdf","docuSealTemplateId":14}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "IL", Label = "Illinois", SortOrder = 35, Metadata = """{"category":"state_form","formName":"IL-W-4","sourceUrl":"https://tax.illinois.gov/content/dam/soi/en/web/tax/forms/withholding/documents/il-w-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "IN", Label = "Indiana", SortOrder = 36, Metadata = """{"category":"state_form","formName":"WH-4","sourceUrl":"https://www.in.gov/dor/files/WH-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "KY", Label = "Kentucky", SortOrder = 37, Metadata = """{"category":"state_form","formName":"K-4 (KY)","sourceUrl":"https://revenue.ky.gov/Forms/Form%20K-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "LA", Label = "Louisiana", SortOrder = 38, Metadata = """{"category":"state_form","formName":"L-4","sourceUrl":"https://revenue.louisiana.gov/Forms/ForIndividuals/R-1300(L4).pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "MD", Label = "Maryland", SortOrder = 39, Metadata = """{"category":"state_form","formName":"MW507","sourceUrl":"https://www.marylandtaxes.gov/forms/current_forms/MW507.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "ME", Label = "Maine", SortOrder = 40, Metadata = """{"category":"state_form","formName":"W-4ME","sourceUrl":"https://www.maine.gov/revenue/sites/maine.gov.revenue/files/inline-files/w-4me_2024.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "MI", Label = "Michigan", SortOrder = 41, Metadata = """{"category":"state_form","formName":"MI-W4","sourceUrl":"https://www.michigan.gov/taxes/-/media/Project/Websites/taxes/Forms/2024/Withholding/MI-W4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "MN", Label = "Minnesota", SortOrder = 42, Metadata = """{"category":"state_form","formName":"W-4MN","sourceUrl":"https://www.revenue.state.mn.us/sites/default/files/2024-01/w-4mn_24.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "MO", Label = "Missouri", SortOrder = 43, Metadata = """{"category":"state_form","formName":"MO W-4","sourceUrl":"https://dor.mo.gov/forms/MO%20W-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "MS", Label = "Mississippi", SortOrder = 44, Metadata = """{"category":"state_form","formName":"89-350","sourceUrl":"https://www.dor.ms.gov/sites/default/files/Forms/Individual/Withholding/89-350-23-1.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "NC", Label = "North Carolina", SortOrder = 45, Metadata = """{"category":"state_form","formName":"NC-4","sourceUrl":"https://www.ncdor.gov/documents/nc-4-employee-withholding-allowance-certificate"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "NE", Label = "Nebraska", SortOrder = 46, Metadata = """{"category":"state_form","formName":"W-4N","sourceUrl":"https://revenue.nebraska.gov/sites/revenue.nebraska.gov/files/doc/tax-forms/f_w4n.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "OH", Label = "Ohio", SortOrder = 47, Metadata = """{"category":"state_form","formName":"IT-4","sourceUrl":"https://tax.ohio.gov/static/forms/ohio_individual/individual/2024/it-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "OK", Label = "Oklahoma", SortOrder = 48, Metadata = """{"category":"state_form","formName":"OK-W-4","sourceUrl":"https://oklahoma.gov/content/dam/ok/en/tax/documents/forms/withholding/OK-W-4.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "RI", Label = "Rhode Island", SortOrder = 49, Metadata = """{"category":"state_form","formName":"RI W-4","sourceUrl":"https://tax.ri.gov/sites/g/files/xkgbur541/files/forms/W-4_2024.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "SC", Label = "South Carolina", SortOrder = 50, Metadata = """{"category":"state_form","formName":"SC W-4","sourceUrl":"https://dor.sc.gov/forms-site/Forms/SC_W4_2024.pdf"}""" },
                new ReferenceData { GroupCode = "state_withholding", Code = "WV", Label = "West Virginia", SortOrder = 51, Metadata = """{"category":"state_form","formName":"WV/IT-104","sourceUrl":"https://tax.wv.gov/Documents/TaxForms/it104.pdf"}""" }
            );
            await db.SaveChangesAsync();
            Log.Information("Seeded state withholding reference data (51 entries)");
        }

        // Compliance Form Templates
        if (!await db.ComplianceFormTemplates.AnyAsync())
        {
            db.ComplianceFormTemplates.AddRange(
                new ComplianceFormTemplate
                {
                    Name = "W-4 Federal Tax Withholding",
                    FormType = ComplianceFormType.W4,
                    Description = "Employee's Withholding Certificate — determines federal income tax withholding from your paycheck.",
                    Icon = "request_quote",
                    SourceUrl = "https://www.irs.gov/pub/irs-pdf/fw4.pdf",
                    IsAutoSync = true,
                    IsActive = true,
                    SortOrder = 1,
                    RequiresIdentityDocs = false,
                    BlocksJobAssignment = true,
                    ProfileCompletionKey = "w4",
                },
                new ComplianceFormTemplate
                {
                    Name = "I-9 Employment Eligibility",
                    FormType = ComplianceFormType.I9,
                    Description = "Employment Eligibility Verification — verifies identity and authorization to work in the United States.",
                    Icon = "verified_user",
                    SourceUrl = "https://www.uscis.gov/sites/default/files/document/forms/i-9.pdf",
                    IsAutoSync = true,
                    IsActive = true,
                    SortOrder = 2,
                    RequiresIdentityDocs = true,
                    BlocksJobAssignment = true,
                    ProfileCompletionKey = "i9",
                },
                new ComplianceFormTemplate
                {
                    Name = "State Tax Withholding",
                    FormType = ComplianceFormType.StateWithholding,
                    Description = "State-specific income tax withholding form — varies by state. Upload the form for your state.",
                    Icon = "account_balance",
                    IsAutoSync = false,
                    IsActive = true,
                    SortOrder = 3,
                    RequiresIdentityDocs = false,
                    BlocksJobAssignment = true,
                    ProfileCompletionKey = "stateWithholding",
                },
                new ComplianceFormTemplate
                {
                    Name = "Direct Deposit Authorization",
                    FormType = ComplianceFormType.DirectDeposit,
                    Description = "Authorize electronic deposit of your paycheck to your bank account.",
                    Icon = "account_balance_wallet",
                    IsAutoSync = false,
                    IsActive = true,
                    SortOrder = 4,
                    RequiresIdentityDocs = false,
                    BlocksJobAssignment = false,
                    ProfileCompletionKey = "directDeposit",
                },
                new ComplianceFormTemplate
                {
                    Name = "Workers' Comp Acknowledgment",
                    FormType = ComplianceFormType.WorkersComp,
                    Description = "Acknowledge receipt and understanding of workers' compensation coverage and procedures.",
                    Icon = "health_and_safety",
                    IsAutoSync = false,
                    IsActive = true,
                    SortOrder = 5,
                    RequiresIdentityDocs = false,
                    BlocksJobAssignment = false,
                    ProfileCompletionKey = "workersComp",
                },
                new ComplianceFormTemplate
                {
                    Name = "Employee Handbook Acknowledgment",
                    FormType = ComplianceFormType.Handbook,
                    Description = "Acknowledge receipt and understanding of the employee handbook and company policies.",
                    Icon = "menu_book",
                    IsAutoSync = false,
                    IsActive = true,
                    SortOrder = 6,
                    RequiresIdentityDocs = false,
                    BlocksJobAssignment = false,
                    ProfileCompletionKey = "handbook",
                }
            );
            await db.SaveChangesAsync();
            Log.Information("Seeded compliance form templates");
        }

        // Form definitions are NOT seeded — they are dynamically extracted from PDFs
        // when an admin uploads a document or triggers extraction via the extract-definition endpoint.
        // Templates with SourceUrl will have their definitions extracted on first sync.

        // Backfill state withholding source URLs for existing installs
        // Load all entries in memory — Metadata is jsonb, so string.Contains() doesn't translate to SQL
        var stateEntries = (await db.ReferenceData
            .Where(r => r.GroupCode == "state_withholding")
            .ToListAsync())
            .Where(r => r.Metadata != null && !r.Metadata.Contains("sourceUrl"))
            .ToList();
        if (stateEntries.Count > 0)
        {
            var stateUrls = StateWithholdingUrls.GetAll();
            foreach (var entry in stateEntries)
            {
                if (stateUrls.TryGetValue(entry.Code, out var url))
                {
                    // Inject sourceUrl into existing metadata JSON
                    entry.Metadata = entry.Metadata!.TrimEnd('}') + $@",""sourceUrl"":""{url}""}}";
                }
            }
            await db.SaveChangesAsync();
            Log.Information("Backfilled {Count} state withholding entries with source URLs", stateEntries.Count);
        }

        // Company Profile Settings
        if (!await db.SystemSettings.AnyAsync(s => s.Key == "company.name"))
        {
            db.SystemSettings.AddRange(
                new SystemSetting { Key = "company.name", Value = "", Description = "Legal business name" },
                new SystemSetting { Key = "company.phone", Value = "", Description = "Main company phone" },
                new SystemSetting { Key = "company.email", Value = "", Description = "Main company email" },
                new SystemSetting { Key = "company.ein", Value = "", Description = "Federal tax identification number (EIN)" },
                new SystemSetting { Key = "company.website", Value = "", Description = "Company website URL" },
                new SystemSetting { Key = "company_state", Value = "ID", Description = "Company home state (determines default state withholding form)" }
            );
            await db.SaveChangesAsync();
            Log.Information("Seeded company profile settings");
        }

        // ── Training Modules & Paths ──────────────────────────────────────
        await SeedTrainingAsync(db);
    }

    private static async Task SeedTrainingAsync(AppDbContext db)
    {
        if (await db.TrainingModules.AnyAsync()) return;

        // ── Training Modules ──────────────────────────────────────────────

        // Path 1: New Employee Onboarding
        var welcome = new TrainingModule
        {
            Title = "Welcome to QB Engineer",
            Slug = "welcome-to-qb-engineer",
            Summary = "Get oriented with QB Engineer — what it does, how it's organized, and what you'll use every day.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 3,
            IsPublished = true,
            IsOnboardingRequired = true,
            SortOrder = 1,
            AppRoutes = """["/dashboard"]""",
            Tags = """["onboarding","welcome"]""",
            ContentJson = """{"body":"## Welcome to QB Engineer\n\nQB Engineer is a manufacturing operations platform built for small engineering shops. It connects every part of your workflow — from quoting a job to shipping it — in one unified system. You'll use it every day for tracking work, logging time, submitting expenses, and staying compliant.\n\n### What It Does\n\nAt its core, QB Engineer manages jobs on a kanban board. Every job moves through stages from Quote Requested all the way to Payment Received, matching how work actually flows through a machine shop. Along the way it integrates with QuickBooks Online to keep your financials in sync automatically.\n\nBeyond jobs, QB Engineer handles parts catalogs (including BOMs), inventory, purchase orders, sales orders, shipments, and quality inspections. Everything is connected — a job links to a part, which links to its BOM, which links to the vendor POs.\n\n### How It's Organized\n\nThe left sidebar is your primary navigation. Icons expand to labeled sections when you hover. The main areas you'll use:\n\n- **Dashboard** — your daily summary: open jobs, tasks, timers, cycle progress\n- **Board** — the kanban board for all active jobs\n- **Time Tracking** — start/stop timers and review your logged hours\n- **Account** — your profile, compliance forms, pay stubs, and security settings\n\n### What to Do Next\n\nStart by completing the remaining modules in this onboarding path. They'll walk you through setting up your profile, completing required compliance forms, logging time, and submitting expenses. The whole path takes about 25 minutes.\n\nAfter onboarding, your manager will assign additional training paths based on your role.","sections":[]}"""
        };

        var navigating = new TrainingModule
        {
            Title = "Navigating the App",
            Slug = "navigating-the-app",
            Summary = "A guided tour of the sidebar, header, notifications, and main navigation areas.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Walkthrough,
            EstimatedMinutes = 5,
            IsPublished = true,
            IsOnboardingRequired = true,
            SortOrder = 2,
            AppRoutes = """["/dashboard"]""",
            Tags = """["onboarding","navigation"]""",
            ContentJson = """{"appRoute":"/dashboard","startButtonLabel":"Take the Tour","steps":[{"element":"[data-tour='sidebar']","popover":{"title":"Sidebar Navigation","description":"The sidebar holds all your navigation links. Icons on the left expand to labeled menus when you hover over them. Click any icon to navigate to that section.","side":"right"}},{"element":"[data-tour='header']","popover":{"title":"App Header","description":"The header stays visible on every page. Use the search icon to do a global search across jobs, parts, customers, and more.","side":"bottom"}},{"element":"[data-tour='notifications-bell']","popover":{"title":"Notifications","description":"The bell icon shows your unread notification count. Click it to open the notification panel — you'll see job updates, assignments, and system alerts here.","side":"bottom"}},{"element":"[data-tour='user-menu']","popover":{"title":"User Menu","description":"Your avatar in the top-right opens the user menu. From here you can switch themes, access your account settings, or log out.","side":"bottom"}},{"element":"[data-tour='dashboard-widgets']","popover":{"title":"Dashboard Widgets","description":"Your dashboard shows open jobs, today's tasks, active timers, and cycle progress. It updates in real time as work moves through the board.","side":"top"}}]}"""
        };

        var profileSetup = new TrainingModule
        {
            Title = "Setting Up Your Profile",
            Slug = "setting-up-your-profile",
            Summary = "How to complete your contact info, emergency contact, and account security settings.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 4,
            IsPublished = true,
            IsOnboardingRequired = true,
            SortOrder = 3,
            AppRoutes = """["/account","/account/profile"]""",
            Tags = """["onboarding","account"]""",
            ContentJson = """{"body":"## Setting Up Your Profile\n\nYour employee profile is how the system knows who you are, where to reach you, and how to pay you. Completing it accurately is important — your work location drives which state withholding forms you'll need to fill out.\n\n### Profile Section\n\nNavigate to **Account → Profile**. Fill in your first and last name, then confirm your work location. If you work at a location other than the company's default, update this now — it affects your state tax withholding.\n\n### Contact Information\n\nGo to **Account → Contact**. Enter your personal phone number and home address. This is kept confidential and is only used for HR and payroll purposes.\n\n### Emergency Contact\n\nUnder **Account → Emergency**, add the name, phone number, and relationship of someone to contact in an emergency. This is required as part of your onboarding.\n\n### Security Settings\n\nVisit **Account → Security** to review your login settings. If your employer uses badge/PIN kiosk login, you'll use the 4–6 digit PIN you were assigned. You can change this PIN here at any time.\n\n### Why It Matters\n\nYour profile drives your compliance form requirements. An incomplete work location means the system can't determine which state withholding form to show you. An incomplete address or emergency contact will show a yellow warning on your profile completeness tracker.","sections":[]}"""
        };

        var compliance = new TrainingModule
        {
            Title = "Completing Your Compliance Forms",
            Slug = "completing-compliance-forms",
            Summary = "Walk through required onboarding forms: W-4, state withholding, I-9, direct deposit, and handbook acknowledgment.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 6,
            IsPublished = true,
            IsOnboardingRequired = true,
            SortOrder = 4,
            AppRoutes = """["/account/tax-forms"]""",
            Tags = """["onboarding","compliance","w4","i9"]""",
            ContentJson = """{"body":"## Completing Your Compliance Forms\n\nCompliance forms are legal documents required for employment. You must complete them before you can be assigned to jobs. Navigate to **Account → Tax Forms** to see all required forms.\n\n### Required Forms\n\n**Federal W-4** — The Employee's Withholding Certificate. This tells your employer how much federal income tax to withhold from each paycheck. You'll fill this out directly in the app. The form is pre-populated with your name and address from your profile.\n\n**State Withholding** — Depending on your work location, you may need to fill out a state-specific withholding form. The app automatically shows the correct form for your state. Some states (like Texas and Florida) have no income tax and no form is required.\n\n**I-9 Employment Eligibility** — Federal law requires you to verify your right to work in the United States. You'll complete Part 1 of the I-9 in the app. Your manager will complete Part 2 after reviewing your identity documents in person.\n\n**Direct Deposit Authorization** — Authorize electronic deposit of your paycheck. You'll need your bank's routing number and your account number.\n\n**Workers' Comp Acknowledgment** — A simple acknowledgment that you've been informed of your workers' compensation coverage. One click to complete.\n\n**Employee Handbook** — Acknowledge that you've received and read the employee handbook.\n\n### What Happens After Submission\n\nEach form shows a status badge: Not Started, In Progress, Submitted, or Approved. Once your manager reviews and approves a form, it's locked and a PDF copy is available for your records. The Profile Completeness widget on your dashboard tracks your overall completion percentage.","sections":[]}"""
        };

        var timeTracking = new TrainingModule
        {
            Title = "Logging Your Time",
            Slug = "logging-your-time",
            Summary = "How to start and stop timers, log manual time entries, and review your hours.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 5,
            IsPublished = true,
            IsOnboardingRequired = true,
            SortOrder = 5,
            AppRoutes = """["/time-tracking"]""",
            Tags = """["onboarding","time-tracking"]""",
            ContentJson = """{"body":"## Logging Your Time\n\nAccurate time tracking is essential — it drives payroll, job costing, and billing. QB Engineer makes it easy with live timers that you can start and stop directly from a job card or from the Time Tracking page.\n\n### Starting a Timer\n\nThere are two ways to start a timer:\n\n1. **From the Kanban Board** — Find your job card and click the play button (▶) on the card. A timer starts immediately and shows as active on the Dashboard.\n2. **From Time Tracking** — Navigate to **Time Tracking** and click **Start Timer**. Select the job from the dropdown and click Start.\n\nYou can only have one active timer at a time. Starting a new timer automatically stops the previous one.\n\n### Stopping a Timer\n\nClick the stop button (■) on the active timer card in the Dashboard, or navigate to **Time Tracking** and click Stop on the running entry.\n\n### Manual Time Entry\n\nIf you forgot to start a timer or worked offline, you can add time manually. In **Time Tracking**, click **Add Entry**, select the date, job, duration, and optionally add notes. Manual entries are flagged differently from timer-based entries.\n\n### Reviewing Your Hours\n\nThe Time Tracking page shows all your entries for the current week by default. Use the date range picker to see any time range. Your total hours are shown at the top. If your entries look wrong, contact your manager — entries can be corrected up until payroll is processed.","sections":[]}"""
        };

        var expenses = new TrainingModule
        {
            Title = "Submitting Expenses",
            Slug = "submitting-expenses",
            Summary = "A step-by-step walkthrough of creating an expense report and uploading a receipt.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Walkthrough,
            EstimatedMinutes = 4,
            IsPublished = true,
            IsOnboardingRequired = true,
            SortOrder = 6,
            AppRoutes = """["/expenses"]""",
            Tags = """["onboarding","expenses"]""",
            ContentJson = """{"appRoute":"/expenses","startButtonLabel":"Walk Me Through It","steps":[{"element":"[data-tour='expenses-list']","popover":{"title":"Your Expense List","description":"This page shows all your submitted expenses. Each row shows the date, description, amount, and current approval status.","side":"bottom"}},{"element":"[data-tour='new-expense-btn']","popover":{"title":"New Expense Button","description":"Click 'New Expense' to open the expense form. You can also submit expenses on behalf of others if you have a Manager role.","side":"bottom"}},{"element":"[data-tour='expense-form-fields']","popover":{"title":"Expense Details","description":"Fill in the date, category, amount, and a brief description. Link the expense to a job if it's a project cost — this helps with job costing and QB sync.","side":"right"}},{"element":"[data-tour='receipt-upload']","popover":{"title":"Upload Your Receipt","description":"Drag and drop your receipt image or PDF here, or click to browse. Supported formats: JPG, PNG, PDF. Maximum 10MB.","side":"top"}},{"element":"[data-tour='submit-expense-btn']","popover":{"title":"Submit for Approval","description":"Click Submit to send your expense to your manager for approval. You'll get a notification when it's approved or if questions arise.","side":"top"}}]}"""
        };

        var onboardingQuiz = new TrainingModule
        {
            Title = "Onboarding Knowledge Check",
            Slug = "onboarding-quiz",
            Summary = "A short quiz to confirm you're ready to work in QB Engineer.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Quiz,
            EstimatedMinutes = 5,
            IsPublished = true,
            IsOnboardingRequired = true,
            SortOrder = 7,
            AppRoutes = """["/training"]""",
            Tags = """["onboarding","quiz"]""",
            ContentJson = """{"passingScore":80,"questionsPerQuiz":10,"shuffleOptions":true,"showExplanationsAfterSubmit":true,"questions":[{"id":"ob1","text":"Where do you go to start a work timer for a specific job?","options":[{"id":"a","text":"The Reports page — it tracks all job activity"},{"id":"b","text":"The job card on the Kanban Board or the Time Tracking page","isCorrect":true},{"id":"c","text":"The Admin panel under Time Settings"},{"id":"d","text":"The Backlog — select the job and click Start"}],"explanation":"Timers can be started from the job card on the Kanban Board or from the Time Tracking page. Both give you a running clock tied to that specific job."},{"id":"ob2","text":"You forgot to start your timer this morning. How do you record those hours?","options":[{"id":"a","text":"Ask your manager to add them to your timesheet"},{"id":"b","text":"You can't — only real-time timers are allowed"},{"id":"c","text":"Add a manual time entry in the Time Tracking page","isCorrect":true},{"id":"d","text":"Edit the job description to note the time worked"}],"explanation":"The Time Tracking page supports manual entries for cases when you forgot to start a timer. Enter the date, job, and duration directly."},{"id":"ob3","text":"Which section shows all your required compliance forms (W-4, I-9, etc.)?","options":[{"id":"a","text":"Account → Tax Forms","isCorrect":true},{"id":"b","text":"Admin → Compliance Templates"},{"id":"c","text":"Dashboard → Getting Started"},{"id":"d","text":"Reports → Employee Documents"}],"explanation":"All personal tax and compliance forms live under Account → Tax Forms. You'll see your submission status and any forms that still need to be completed."},{"id":"ob4","text":"You need to submit a $45 fuel expense for a job site visit. What should you do?","options":[{"id":"a","text":"Email your manager the receipt and they'll enter it"},{"id":"b","text":"Go to Expenses, click New Expense, fill in the details, upload your receipt, and submit","isCorrect":true},{"id":"c","text":"Add a note to the job card with the dollar amount"},{"id":"d","text":"Log it as billable time since you were traveling"}],"explanation":"All expense reimbursements go through the Expenses page. Upload your receipt, select the category, link it to a job if applicable, and submit for approval."},{"id":"ob5","text":"Where do you update your emergency contact information?","options":[{"id":"a","text":"Account → Emergency","isCorrect":true},{"id":"b","text":"Admin → Users → your profile"},{"id":"c","text":"Dashboard → Profile card"},{"id":"d","text":"Account → Security"}],"explanation":"Emergency contacts are managed under Account → Emergency. Keep this up to date — it's required for onboarding completion."},{"id":"ob6","text":"You want to see how complete your employee profile is. Where do you look?","options":[{"id":"a","text":"Dashboard → Profile Completeness widget","isCorrect":true},{"id":"b","text":"Admin → Users → your account"},{"id":"c","text":"Reports → Employee Status"},{"id":"d","text":"Account → Security → Profile Score"}],"explanation":"The Dashboard shows a Profile Completeness widget that lists exactly which items are still missing. It disappears once you've finished all required sections."},{"id":"ob7","text":"You want to change your login password. Where do you go?","options":[{"id":"a","text":"Admin → Users — your admin can reset it"},{"id":"b","text":"Account → Security","isCorrect":true},{"id":"c","text":"Dashboard → Settings → Password"},{"id":"d","text":"Account → Profile → Edit"}],"explanation":"Your password and authentication settings are under Account → Security. You can update your password there at any time without contacting an admin."},{"id":"ob8","text":"A notification badge appears on the bell icon in the header. What does that indicate?","options":[{"id":"a","text":"You have a new message in the Chat feature"},{"id":"b","text":"Your session is about to expire"},{"id":"c","text":"You have unread notifications from the system or your teammates","isCorrect":true},{"id":"d","text":"A compliance form is overdue — the badge always means that"}],"explanation":"The bell badge shows the count of unread notifications. Click it to open the notification panel where you can see, dismiss, and pin individual notifications."},{"id":"ob9","text":"You need to attach a PDF drawing to a job. Where in the job is the best place to do this?","options":[{"id":"a","text":"Job detail panel → Files section (drag and drop or click to browse)","isCorrect":true},{"id":"b","text":"Parts Catalog → attach it to the related part instead"},{"id":"c","text":"Reports → upload to a saved report"},{"id":"d","text":"Admin → Document Storage"}],"explanation":"The job detail panel has a Files section where you can attach PDFs, images, CAD files, and other documents directly to the job record."},{"id":"ob10","text":"What happens to a job when it is archived?","options":[{"id":"a","text":"It is permanently deleted from the system"},{"id":"b","text":"It moves to a separate archive database that admins can access"},{"id":"c","text":"It is removed from the board but preserved in the system for reporting and traceability","isCorrect":true},{"id":"d","text":"It is locked and no further changes can be made"}],"explanation":"Jobs are never deleted in QB Engineer — they're archived. An archived job disappears from the board but remains fully searchable and visible in the Backlog with the Archived filter."},{"id":"ob11","text":"Where would you find jobs assigned to you that are due today?","options":[{"id":"a","text":"Reports → My Jobs Due Today"},{"id":"b","text":"Dashboard → Today's Tasks widget","isCorrect":true},{"id":"c","text":"Backlog → filter by assignee and due date"},{"id":"d","text":"Kanban Board → scroll to find your cards"}],"explanation":"The Today's Tasks widget on the Dashboard shows all active jobs assigned to you, sorted by due date, with overdue jobs highlighted."},{"id":"ob12","text":"You started a timer on the wrong job. What should you do?","options":[{"id":"a","text":"Let it run — you can't stop a timer once started"},{"id":"b","text":"Stop the timer, then manually edit the time entry to correct the job","isCorrect":true},{"id":"c","text":"Ask your manager to delete it"},{"id":"d","text":"Archive the job and start again"}],"explanation":"Stop the running timer, then go to Time Tracking to edit the entry — you can correct the linked job, adjust the duration, or delete it entirely."},{"id":"ob13","text":"Which page shows all open purchase orders for materials your team is waiting on?","options":[{"id":"a","text":"Inventory → Pending Receipts"},{"id":"b","text":"Backlog → filter by status Materials Ordered"},{"id":"c","text":"Purchase Orders page","isCorrect":true},{"id":"d","text":"Dashboard → Open Orders widget shows all POs"}],"explanation":"The Purchase Orders page lists all POs with their status (Draft, Sent, Partially Received, Received). You can see what's outstanding and when deliveries are expected."},{"id":"ob14","text":"You want to send a message to a coworker about a specific job without leaving the app. What do you use?","options":[{"id":"a","text":"Email them from your regular email client"},{"id":"b","text":"Add a note to the job card and @mention them","isCorrect":true},{"id":"c","text":"Update the job description and hope they notice"},{"id":"d","text":"Use the Reports page to flag the job for review"}],"explanation":"Typing a comment in the job's activity feed and @mentioning a teammate sends them an in-app notification. They can reply directly on the job card."},{"id":"ob15","text":"Your W-4 was recently updated and you need to re-submit it. Where do you go?","options":[{"id":"a","text":"Admin → Compliance → re-upload"},{"id":"b","text":"Account → Tax Forms — find the W-4 and click to complete it again","isCorrect":true},{"id":"c","text":"Reports → Employee → Tax Documents"},{"id":"d","text":"Contact payroll — you cannot self-submit"}],"explanation":"Account → Tax Forms lets you view and re-submit compliance forms like the W-4 whenever they need to be updated. Your submission history is also visible there."},{"id":"ob16","text":"The Kanban board shows a column with a red header. What does this indicate?","options":[{"id":"a","text":"The column contains at least one overdue job"},{"id":"b","text":"The column has been locked by an admin"},{"id":"c","text":"The job count in that column has exceeded the WIP limit","isCorrect":true},{"id":"d","text":"The column contains jobs with Critical priority"}],"explanation":"Column headers turn red when the number of jobs exceeds the configured WIP (Work In Progress) limit. It's a visual signal to finish work before adding more to that stage."},{"id":"ob17","text":"You want to see everything that happened to a specific job — who moved it, what changed, what notes were added. Where is this?","options":[{"id":"a","text":"Reports → Job History"},{"id":"b","text":"Job detail panel → Activity tab","isCorrect":true},{"id":"c","text":"Admin → Audit Log — filter by job"},{"id":"d","text":"Backlog → job row → expand"}],"explanation":"Every job has a full activity timeline in its detail panel. It shows stage moves, field edits, file uploads, comments, @mentions, and timer starts/stops in chronological order."},{"id":"ob18","text":"Where do you go to view your recent pay stubs?","options":[{"id":"a","text":"Dashboard → Payroll widget"},{"id":"b","text":"Admin → Payroll → your records"},{"id":"c","text":"Account → Pay Stubs","isCorrect":true},{"id":"d","text":"Reports → Payroll → filter by employee"}],"explanation":"Your personal pay stubs are available under Account → Pay Stubs. Admins upload pay stubs there and you receive a notification when a new one is available."},{"id":"ob19","text":"What is the correct way to log time on a job you finished yesterday but forgot to track?","options":[{"id":"a","text":"You cannot log time for a previous date — only today"},{"id":"b","text":"Ask your manager to enter it in the system for you"},{"id":"c","text":"Open Time Tracking, click New Entry, set yesterday's date, and enter the duration","isCorrect":true},{"id":"d","text":"Add a comment to the job explaining the hours worked"}],"explanation":"Manual time entries support any past date. Open Time Tracking → New Entry, select the correct date and job, enter the hours, and save."},{"id":"ob20","text":"You want to print a barcode label for a job. Where do you do this?","options":[{"id":"a","text":"Reports → Labels → generate from job list"},{"id":"b","text":"Job detail panel → print a label from the job's action menu","isCorrect":true},{"id":"c","text":"Admin → Label Templates → select and print"},{"id":"d","text":"Inventory → Bin Labels — all labels are printed from there"}],"explanation":"Job labels are printed directly from the job detail panel. The label includes the job number, QR code, and barcode for use with shop floor scanners."}]}"""
        };

        // Path 2: Production Engineer Training
        var kanbanBasics = new TrainingModule
        {
            Title = "Understanding the Kanban Board",
            Slug = "kanban-board-basics",
            Summary = "A guided tour of the production board, columns, cards, WIP limits, and filters.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Walkthrough,
            EstimatedMinutes = 7,
            IsPublished = true,
            IsOnboardingRequired = false,
            SortOrder = 1,
            AppRoutes = """["/kanban","/board"]""",
            Tags = """["kanban","jobs","engineering"]""",
            ContentJson = """{"appRoute":"/kanban","startButtonLabel":"Tour the Board","steps":[{"element":"[data-tour='board-columns']","popover":{"title":"Board Columns","description":"Each column is a production stage. Jobs flow left to right as work progresses. The column header shows the stage name, the count of jobs, and the WIP limit if one is set.","side":"bottom"}},{"element":"[data-tour='board-filters']","popover":{"title":"Board Filters","description":"Use the filter bar to narrow the board by assignee, priority, customer, or date range. Filters are reflected in the URL so you can share or bookmark specific views.","side":"bottom"}},{"element":"[data-tour='job-card']","popover":{"title":"Job Cards","description":"Each card shows the job number, title, assignee avatar, priority indicator, and due date. A red due date means the job is overdue. A timer icon means someone is actively working on it.","side":"right"}},{"element":"[data-tour='wip-limit']","popover":{"title":"WIP Limits","description":"Column headers turn red when the job count exceeds the configured WIP limit. This is a visual signal to finish work before pulling more in.","side":"bottom"}},{"element":"[data-tour='track-type-switcher']","popover":{"title":"Track Type Switcher","description":"The board supports multiple track types: Production, R&D/Tooling, Maintenance, and custom types. Use the switcher to move between them.","side":"bottom"}}]}"""
        };

        var jobManagement = new TrainingModule
        {
            Title = "Creating and Managing Jobs",
            Slug = "creating-managing-jobs",
            Summary = "How to create jobs, move them through stages, assign them, and use bulk actions.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 8,
            IsPublished = true,
            SortOrder = 2,
            AppRoutes = """["/kanban","/board","/backlog"]""",
            Tags = """["kanban","jobs","engineering"]""",
            ContentJson = """{"body":"## Creating and Managing Jobs\n\nJobs are the core unit of work in QB Engineer. Each job represents a discrete piece of work — a machining order, a tooling build, a maintenance task — and moves through stages from Quote to Payment.\n\n### Creating a Job\n\nClick **New Job** from the Kanban Board or Backlog. Fill in the job number (auto-generated if left blank), title, track type, starting stage, customer, assignee, due date, and priority. A job can also be linked to a part from your Parts Catalog.\n\n### Moving Jobs Between Stages\n\nDrag a job card from one column to the next. Some stage transitions are irreversible — once a job has an Invoice or Payment attached, it cannot move backward. The system will warn you before allowing a backward move on non-irreversible stages.\n\n### Assigning Jobs\n\nClick the assignee avatar on a job card and select a team member. Multiple people can work on a job, but only one is the primary assignee.\n\n### Bulk Actions\n\nCtrl+Click to select multiple job cards. A bulk action bar appears at the bottom of the board with options to Move, Assign, Set Priority, or Archive all selected jobs at once. This is useful for moving a batch of materials-received jobs into production.\n\n### Archiving vs. Deleting\n\nJobs are never deleted — they're archived. An archived job is removed from the board but remains in the system for reporting, billing, and traceability. Access archived jobs from the Backlog with the Archived filter enabled.\n\n### Priority System\n\nJobs can be marked Low, Normal, High, or Critical. High and Critical jobs get visual emphasis on the board — red/orange labels and sorting priority. Set priority from the job card menu or bulk action bar.","sections":[]}"""
        };

        var jobDetail = new TrainingModule
        {
            Title = "Job Detail: Notes, Files, and Subtasks",
            Slug = "job-detail-panel",
            Summary = "How to use the job detail panel to add notes, attach files, and create subtasks.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 5,
            IsPublished = true,
            SortOrder = 3,
            AppRoutes = """["/kanban","/board"]""",
            Tags = """["kanban","jobs"]""",
            ContentJson = """{"body":"## Job Detail Panel\n\nClick any job card to open the detail panel on the right side of the screen. This panel gives you full visibility into the job without leaving the board.\n\n### Notes and Activity Log\n\nThe activity feed shows a chronological history of everything that has happened to the job: stage moves, field changes, file uploads, and comments. To add a note, type in the comment box at the bottom and press Enter. You can @mention a team member to send them a notification.\n\n### Attaching Files\n\nDrag and drop files directly onto the job detail panel, or click the attachment area to browse. Supported file types: PDF, images (JPG, PNG, TIFF), CAD files (STEP, IGES, DXF), and any other document type. Files are stored securely in MinIO object storage and accessible from the job detail at any time.\n\n### Subtasks\n\nSubtasks are smaller to-dos within a job. Click **Add Subtask** to create one. Each subtask can be assigned to a specific team member. Subtasks show on the job card as a progress indicator (e.g., 2/5 complete).\n\n### Linking Jobs\n\nA job can be linked to related jobs — for example, a tooling build linked to the production job it supports. Use the Links section in the detail panel to create Parent/Child or Blocked-By relationships.\n\n### Key Dates\n\nThe detail panel shows Start Date, Due Date, and Completed Date. Overdue jobs display their due date in red. The system records the completion date automatically when a job reaches a terminal stage like Payment Received.","sections":[]}"""
        };

        var partsCatalog = new TrainingModule
        {
            Title = "Parts Catalog Basics",
            Slug = "parts-catalog-basics",
            Summary = "How to navigate the parts catalog, read part records, and understand BOMs and process steps.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 6,
            IsPublished = true,
            SortOrder = 4,
            AppRoutes = """["/parts"]""",
            Tags = """["parts","bom","engineering"]""",
            ContentJson = """{"body":"## Parts Catalog Basics\n\nThe Parts Catalog is the master reference for everything you manufacture or purchase. Each part record contains specifications, bill of materials (BOM), process steps, inventory levels, and revision history.\n\n### Navigating the Catalog\n\nGo to **Parts** in the sidebar. Use the search box to find parts by number, description, or material. Filter by status (Draft, Prototype, Active, Obsolete) or part type (Machined, Assembly, Purchased, Raw Material).\n\n### Part Record Overview\n\nA part record shows:\n- **Part Number and Revision** — e.g., PN-1042 Rev C\n- **Description and Material** — what it is and what it's made from\n- **Status** — Draft through Obsolete\n- **Estimated hours and cost** — used for job costing and quoting\n- **Linked Jobs** — jobs currently being run against this part\n\n### Bill of Materials (BOM)\n\nThe BOM tab shows all child components needed to make this part. Each BOM line has a quantity, source type (Make/Buy/Stock), and links to vendor or sub-part records. BOM changes are tracked with revisions — you can see what the BOM looked like at any revision.\n\n### Process Steps\n\nThe Process Steps tab shows the manufacturing routing — the ordered list of operations needed to produce the part (e.g., Saw → CNC Mill → Deburr → Anodize → Inspect). Each step has an estimated time. This drives job planning and scheduling.\n\n### Inventory Connection\n\nThe Inventory tab on each part shows current stock across all bin locations. When a job is created for a part, the system can suggest reserving stock from inventory if available.","sections":[]}"""
        };

        var partsQuickRef = new TrainingModule
        {
            Title = "Parts Quick Reference",
            Slug = "parts-quick-reference",
            Summary = "Quick reference card for part statuses, BOM source types, and common part catalog actions.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.QuickRef,
            EstimatedMinutes = 2,
            IsPublished = true,
            SortOrder = 5,
            AppRoutes = """["/parts"]""",
            Tags = """["parts","reference"]""",
            ContentJson = """{"title":"Parts Catalog Quick Reference","groups":[{"heading":"Part Statuses","items":[{"label":"Draft","value":"Part is being designed — not yet released to production"},{"label":"Prototype","value":"First article or sample stage — limited runs only"},{"label":"Active","value":"Released for regular production use"},{"label":"Obsolete","value":"No longer in production — preserved for reference and traceability"}]},{"heading":"BOM Source Types","items":[{"label":"Make","value":"Manufactured in-house — will generate a sub-job"},{"label":"Buy","value":"Purchased from vendor — will generate a PO line"},{"label":"Stock","value":"Pulled from internal inventory — will create a reservation"}]},{"heading":"Common Actions","items":[{"label":"New Part","value":"Parts page → New Part button (top-right)"},{"label":"New Revision","value":"Part detail → Revisions tab → New Revision"},{"label":"Add BOM Line","value":"Part detail → BOM tab → Add Component"},{"label":"Check Inventory","value":"Part detail → Inventory tab"},{"label":"Link to Job","value":"Job detail → Links section → Link Part"}]}]}"""
        };

        var backlogPlanning = new TrainingModule
        {
            Title = "Backlog and Planning",
            Slug = "backlog-and-planning",
            Summary = "How to use the backlog to manage unscheduled work and the planning cycle to commit jobs to sprints.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 6,
            IsPublished = true,
            SortOrder = 6,
            AppRoutes = """["/backlog","/planning"]""",
            Tags = """["backlog","planning","engineering"]""",
            ContentJson = """{"body":"## Backlog and Planning\n\nThe Backlog holds all jobs that exist in the system but aren't currently active on the board. The Planning section lets you group jobs into two-week cycles — what gets committed to this cycle and what stays in the backlog.\n\n### The Backlog\n\nNavigate to **Backlog** to see all open jobs that aren't on the board, plus all archived and completed jobs. Filter by status, priority, customer, assignee, or track type. The backlog is where PMs and managers do triage: deciding what gets worked on next.\n\nFrom the backlog you can:\n- Create new jobs without placing them on the board immediately\n- Edit job details without moving them to the board\n- Move jobs to the board (drag to the Board section or use the Move action)\n- Archive jobs that are no longer relevant\n\n### Planning Cycles\n\nNavigate to **Planning** to see the current and upcoming cycles. A planning cycle is typically two weeks. The planning view has a split panel: the backlog on the left, the current cycle on the right.\n\nDrag jobs from the backlog into the cycle to commit them. Committed jobs appear on the board. At the end of a cycle, incomplete jobs roll over to the next cycle or return to the backlog.\n\n### Daily Prompts\n\nEach evening, the system may prompt you with 'What are your top 3 priorities for tomorrow?' These are lightweight reminders to plan your next day. They show on the Dashboard.","sections":[]}"""
        };

        var dashboard = new TrainingModule
        {
            Title = "Reading the Dashboard",
            Slug = "reading-the-dashboard",
            Summary = "What each dashboard widget shows and how to use it to stay on top of your work.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 4,
            IsPublished = true,
            SortOrder = 7,
            AppRoutes = """["/dashboard"]""",
            Tags = """["dashboard","engineering"]""",
            ContentJson = """{"body":"## Reading the Dashboard\n\nThe Dashboard is your daily home base in QB Engineer. It aggregates the most important information from across the system into a single view.\n\n### Today's Tasks Widget\n\nShows all jobs assigned to you that are currently active on the board, sorted by due date. Jobs due today or overdue are highlighted. Click any task to open the job detail panel.\n\n### Active Timers\n\nIf you have a timer running, it shows prominently at the top with an elapsed time counter. Click Stop to stop the timer, or click the job name to open the detail panel.\n\n### Open Orders Widget\n\nShows a summary of all open jobs by stage: how many are in Quote, Production, QC, Shipped, and so on. This gives you a quick snapshot of shop load.\n\n### Cycle Progress Widget\n\nShows the current planning cycle's completion percentage. Completed jobs vs. total committed jobs. If you're mid-cycle and behind, this widget turns yellow.\n\n### Getting Started Banner\n\nNew users see a Getting Started banner that tracks onboarding progress. It disappears once you've completed your profile, compliance forms, and this training path.\n\n### Customizing Your Dashboard\n\nYou can rearrange dashboard widgets by dragging them. Your layout is saved to your user preferences and restored every time you log in, even from a different browser or device.","sections":[]}"""
        };

        var engineerQuiz = new TrainingModule
        {
            Title = "Production Engineer Assessment",
            Slug = "production-engineer-quiz",
            Summary = "An assessment covering kanban, job management, and parts catalog knowledge.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Quiz,
            EstimatedMinutes = 8,
            IsPublished = true,
            SortOrder = 8,
            AppRoutes = """["/training"]""",
            Tags = """["engineering","quiz","assessment"]""",
            ContentJson = """{"passingScore":75,"questionsPerQuiz":10,"shuffleOptions":true,"showExplanationsAfterSubmit":true,"questions":[{"id":"eq1","text":"A job card shows a red due date. What does this indicate?","options":[{"id":"a","text":"The job has been flagged as high priority by a manager"},{"id":"b","text":"The job is overdue — the due date has passed","isCorrect":true},{"id":"c","text":"The job requires a quality inspection before moving forward"},{"id":"d","text":"The job is blocked by a missing component"}],"explanation":"Red due dates mean the scheduled completion date has passed. The job needs attention — either re-prioritize it or update the due date if the timeline changed."},{"id":"eq2","text":"What does the WIP limit on a kanban column control?","options":[{"id":"a","text":"The maximum number of subtasks allowed inside each job card"},{"id":"b","text":"How many users can be simultaneously assigned to jobs in that stage"},{"id":"c","text":"The recommended maximum number of jobs in that stage at one time","isCorrect":true},{"id":"d","text":"The size limit for file attachments on jobs in that column"}],"explanation":"WIP (Work In Progress) limits cap how many jobs should be in a given stage simultaneously. Exceeding the limit turns the column header red as a visual alert to finish work before pulling more in."},{"id":"eq3","text":"You need to move 8 jobs from Materials Received to In Production at once. What is the fastest way?","options":[{"id":"a","text":"Drag each card one by one to the new column"},{"id":"b","text":"Ctrl+Click each card to select all 8, then use the bulk Move action","isCorrect":true},{"id":"c","text":"Archive all 8 and recreate them in the new stage"},{"id":"d","text":"Use the Backlog filter, select them, and push to board"}],"explanation":"Ctrl+Click selects multiple cards. Once selected, a bulk action bar appears at the bottom of the board where you can Move, Assign, Set Priority, or Archive all selected jobs at once."},{"id":"eq4","text":"A part has BOM source type 'Buy'. What does that mean for production planning?","options":[{"id":"a","text":"The part will be manufactured in a sub-job linked to the main job"},{"id":"b","text":"The part will be pulled from your internal stock bin locations"},{"id":"c","text":"The part must be purchased from a vendor and will generate a purchase order line","isCorrect":true},{"id":"d","text":"The part is currently being evaluated for vendor pricing"}],"explanation":"BOM source type 'Buy' means the component is sourced externally. When a job is created for this part, the system will prompt you to create a PO for those components."},{"id":"eq5","text":"A part is marked 'Obsolete'. What does this mean?","options":[{"id":"a","text":"The part record was permanently deleted but the name is retained for reference"},{"id":"b","text":"The part is no longer manufactured but is preserved for historical and traceability purposes","isCorrect":true},{"id":"c","text":"The part has been transferred to a sister company's catalog"},{"id":"d","text":"The part needs a design revision before it can be used again"}],"explanation":"Obsolete status means the part is retired from active production. The record stays fully intact for traceability — past jobs, BOMs, and inspection records all still reference it correctly."},{"id":"eq6","text":"Where would you find the manufacturing routing (ordered list of operations) for a part?","options":[{"id":"a","text":"Part detail → BOM tab — BOM includes all process steps"},{"id":"b","text":"Part detail → Process Steps tab","isCorrect":true},{"id":"c","text":"Part detail → Inventory tab — stock and routing are combined"},{"id":"d","text":"Job detail → Subtasks — each subtask maps to an operation"}],"explanation":"Process Steps define the routing: the ordered operations a part goes through (Saw → Mill → Deburr → Inspect). These drive time estimates and job planning. BOMs handle material components separately."},{"id":"eq7","text":"You want to commit which jobs your team will work on for the next two weeks. Which feature handles this?","options":[{"id":"a","text":"Backlog — apply a priority filter to mark jobs for this sprint"},{"id":"b","text":"Kanban swimlanes — group jobs by timeline to visualize the two weeks"},{"id":"c","text":"Planning Cycles — drag jobs from the backlog into the active cycle","isCorrect":true},{"id":"d","text":"Reports → Capacity Planning — assign jobs from the report"}],"explanation":"Planning Cycles are the sprint-planning tool. The split-panel view shows your backlog on the left and the active cycle on the right. Drag jobs in to commit them, and they appear on the board."},{"id":"eq8","text":"After a planning cycle ends, what happens to jobs that were committed but not completed?","options":[{"id":"a","text":"They are automatically archived to keep the board clean"},{"id":"b","text":"They are deleted and must be re-entered in the next cycle"},{"id":"c","text":"They roll over to the next cycle or can be returned to the backlog","isCorrect":true},{"id":"d","text":"They are automatically reassigned to available team members"}],"explanation":"Incomplete committed jobs don't disappear — they roll over to the next cycle or you can return them to the backlog. No work is lost between cycles."},{"id":"eq9","text":"A BOM line has source type 'Make'. What does this mean when a job is created?","options":[{"id":"a","text":"The component will be purchased via a purchase order"},{"id":"b","text":"A child sub-job will be created linked to the parent job","isCorrect":true},{"id":"c","text":"The component will be pulled from the nearest storage bin"},{"id":"d","text":"The component is flagged as requiring a make-or-buy decision"}],"explanation":"BOM source 'Make' means the component is manufactured in-house. Creating a job for the parent part will also spawn a linked child sub-job for that component, so both are tracked in parallel."},{"id":"eq10","text":"You opened the Process Steps tab on a part and the list is empty. What does that likely mean?","options":[{"id":"a","text":"The part has been marked Obsolete and process steps were cleared"},{"id":"b","text":"Process steps for this part have not been defined yet","isCorrect":true},{"id":"c","text":"Process steps are only visible to managers and admins"},{"id":"d","text":"The part is a purchased item and doesn't need a manufacturing routing"}],"explanation":"An empty Process Steps tab just means no routing has been set up yet. For purchased parts this is expected, but for machined or assembled parts it should be filled in to support accurate time estimates."},{"id":"eq11","text":"What is the difference between a job's due date and its start date?","options":[{"id":"a","text":"There is no start date — only due dates exist on jobs"},{"id":"b","text":"Start date is when materials were received; due date is the customer ship date"},{"id":"c","text":"Start date is when work is planned to begin; due date is the target completion date","isCorrect":true},{"id":"d","text":"Both dates are set automatically from the planning cycle boundaries"}],"explanation":"The start date marks when work should begin on the job. The due date is when the job needs to be complete. Both appear on the card and in reports, and due dates control the overdue highlighting."},{"id":"eq12","text":"You are reviewing part PN-1042 Rev C. A new engineering change is approved. What is the correct next step in QB Engineer?","options":[{"id":"a","text":"Edit the current part record directly to reflect the change"},{"id":"b","text":"Duplicate the part with a new part number for the revised version"},{"id":"c","text":"Create a new revision (Rev D) in the part's Revisions tab","isCorrect":true},{"id":"d","text":"Mark the part Obsolete and create a new part from scratch"}],"explanation":"Revisions are tracked under the part's Revisions tab. Creating Rev D preserves the complete history of Rev C — the BOM, process steps, and inspection data — for full traceability without losing the old design."},{"id":"eq13","text":"A job is in the 'Invoiced/Sent' stage. Can it be moved backward to 'In Production'?","options":[{"id":"a","text":"Yes — any job can be moved to any stage at any time"},{"id":"b","text":"Yes — but only an admin can override the forward-only rule"},{"id":"c","text":"No — once a job reaches an invoiced stage, it cannot move backward because the invoice is an irreversible accounting document","isCorrect":true},{"id":"d","text":"No — jobs are locked once they leave the QC/Review stage"}],"explanation":"Certain stages are marked irreversible because they have accounting documents attached (Invoiced, Payment Received). Once a job is there, moving it backward would create accounting inconsistencies and is blocked by the system."},{"id":"eq14","text":"What is the purpose of linking jobs together in QB Engineer?","options":[{"id":"a","text":"Linked jobs are merged into a single card on the board"},{"id":"b","text":"To show relationships like Parent/Child or Blocked-By between related jobs","isCorrect":true},{"id":"c","text":"Linking jobs shares their time tracking and expenses automatically"},{"id":"d","text":"Links are used to assign multiple track types to a single job"}],"explanation":"Job links document relationships between work. A tooling build linked as Parent to the production job it supports, or a Blocked-By link showing one job can't start until another finishes."},{"id":"eq15","text":"You need to check whether a specific raw material is in stock before creating a job. Where do you look?","options":[{"id":"a","text":"Purchase Orders → filter by part number to see recent receipts"},{"id":"b","text":"Backlog → filter by part — the backlog shows current stock"},{"id":"c","text":"Parts Catalog → open the part → Inventory tab","isCorrect":true},{"id":"d","text":"Dashboard → Open Orders widget shows all current inventory"}],"explanation":"The Inventory tab on a part record shows current stock quantities across all bin locations. You can see exactly what's available, where it's stored, and reserve it for an upcoming job."},{"id":"eq16","text":"What is a planning cycle's 'Day 1' typically used for?","options":[{"id":"a","text":"Running end-of-cycle reports and archiving completed jobs"},{"id":"b","text":"A guided planning session — reviewing the backlog and committing jobs to the cycle","isCorrect":true},{"id":"c","text":"Automatically generating purchase orders for the cycle's material needs"},{"id":"d","text":"Assigning all uncommitted jobs equally across team members"}],"explanation":"Day 1 of a planning cycle is a structured planning day. The split-panel view helps the team review the backlog, discuss priorities, and drag jobs into the cycle to commit them for the upcoming two weeks."},{"id":"eq17","text":"A job card has a timer icon animated on it. What does this mean?","options":[{"id":"a","text":"The job is overdue and the icon is a countdown timer"},{"id":"b","text":"An automated reminder timer was set by a manager"},{"id":"c","text":"A team member is actively running a time entry against that job right now","isCorrect":true},{"id":"d","text":"The job's estimated time has been exceeded"}],"explanation":"An animated timer icon on a job card means someone has an active timer running against it. Hover over or click the card to see who is clocked in."},{"id":"eq18","text":"How would you see the complete history of stock movements for a specific part across all bin locations?","options":[{"id":"a","text":"Reports → Inventory → filter by part number"},{"id":"b","text":"Parts Catalog → part detail → Inventory tab → movement history","isCorrect":true},{"id":"c","text":"Purchase Orders → filter by part → received quantities show history"},{"id":"d","text":"Admin → Audit Log → filter by entity type Inventory"}],"explanation":"The Inventory tab on a part record shows both current bin quantities and the full movement history — receipts, issues, transfers, and adjustments — all in one place."},{"id":"eq19","text":"You want to generate a report showing hours worked per job for the last 30 days. Where do you start?","options":[{"id":"a","text":"Time Tracking page → export the table to CSV"},{"id":"b","text":"Reports → pre-built 'Time by Job' report, or build a custom report with Time Entries as the data source","isCorrect":true},{"id":"c","text":"Kanban Board → export jobs with elapsed time"},{"id":"d","text":"Admin → Analytics → Time Distribution"}],"explanation":"The Reports module has a pre-built 'Time by Job' report. You can also build a custom report using Time Entries as the source and group by job. Both support date range filters."},{"id":"eq20","text":"What is the correct way to handle a job that has been cancelled by the customer after materials were already ordered?","options":[{"id":"a","text":"Delete the job — deleting removes it cleanly from all records"},{"id":"b","text":"Leave the job in its current stage but add a note saying it was cancelled"},{"id":"c","text":"Archive the job and cancel or return the outstanding purchase orders separately","isCorrect":true},{"id":"d","text":"Move it to the last stage (Payment Received) to close it out"}],"explanation":"Archive the job to remove it from the active board without losing the record. Handle outstanding POs separately — you may need to cancel them with the vendor or process a return. Both actions are independent in QB Engineer."},{"id":"eq21","text":"A part's BOM has a component with source type 'Stock'. What does this mean for fulfillment?","options":[{"id":"a","text":"The component needs to be ordered — 'Stock' is a placeholder status"},{"id":"b","text":"The component will be pulled from an internal inventory bin location and a reservation will be created","isCorrect":true},{"id":"c","text":"The component is managed by a third-party stockist and auto-ordered"},{"id":"d","text":"The component is already attached to the job and no action is needed"}],"explanation":"BOM source 'Stock' means the material is pulled from your own inventory. When a job is created, the system can create a reservation against available bin stock so that quantity isn't double-allocated to another job."},{"id":"eq22","text":"You want the kanban board to show only jobs assigned to the machining team. How do you do this?","options":[{"id":"a","text":"Go to Admin → Teams, set the machining team as default, and refresh the board"},{"id":"b","text":"Use the Assignee filter in the board filter bar to filter by team members","isCorrect":true},{"id":"c","text":"The board automatically groups by team — there's no filter needed"},{"id":"d","text":"Create a separate track type for each team to keep their boards isolated"}],"explanation":"The board filter bar supports filtering by assignee, customer, priority, track type, and more. Selecting all members of the machining team by name (or using team swimlane view if enabled) scopes the board to just their work."},{"id":"eq23","text":"What does the 'Prototype' part status mean?","options":[{"id":"a","text":"The part design is complete and approved for full production runs"},{"id":"b","text":"The part is being designed and the record is a placeholder only"},{"id":"c","text":"The part is in a first-article or sample stage — limited production runs only","isCorrect":true},{"id":"d","text":"The part has failed inspection and is being redesigned"}],"explanation":"Prototype status means the part has passed the design phase but is still in validation. It's allowed in limited production (first articles, samples) but not yet released for regular production runs. That requires Active status."},{"id":"eq24","text":"A job has subtasks listed in its detail panel. What do the subtasks represent?","options":[{"id":"a","text":"Individual time entries — each clock-in creates a subtask automatically"},{"id":"b","text":"Child jobs that were automatically created from BOM 'Make' components"},{"id":"c","text":"Discrete to-do items within the job that can be assigned and checked off individually","isCorrect":true},{"id":"d","text":"Quality inspection checkpoints that must be completed before the job can advance"}],"explanation":"Subtasks are lightweight to-do items inside a job. Each can be assigned to a specific person. The job card shows a progress indicator (e.g., 3/5 complete) so you can see task completion at a glance on the board."},{"id":"eq25","text":"You want to see which jobs across the entire shop are currently In Production and when they're due. What is the best way?","options":[{"id":"a","text":"Scroll through the board and mentally note all In Production cards"},{"id":"b","text":"Reports → Job Summary — filter by stage 'In Production' and sort by due date","isCorrect":true},{"id":"c","text":"Backlog → filter by stage In Production and sort by due date"},{"id":"d","text":"Time Tracking → filter by current jobs to see active ones"}],"explanation":"The Job Summary report in Reports lets you filter by any stage and sort by due date, giving you a clean list. The Backlog filter can also work, but Reports gives a better formatted, exportable view."}]}"""
        };

        // Standalone modules
        var reportsModule = new TrainingModule
        {
            Title = "Reports and Analytics",
            Slug = "reports-and-analytics",
            Summary = "How to use the report builder, run pre-built reports, and save custom report templates.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 5,
            IsPublished = true,
            SortOrder = 1,
            AppRoutes = """["/reports"]""",
            Tags = """["reports","analytics"]""",
            ContentJson = """{"body":"## Reports and Analytics\n\nThe Reports module gives you access to 27 pre-built report templates covering jobs, time, expenses, parts, inventory, financials, and more. You can also build fully custom reports using the drag-and-drop report builder.\n\n### Pre-Built Reports\n\nNavigate to **Reports** to see all available reports grouped by category. Click any report name to run it immediately with default filters. Common reports include:\n\n- **Job Summary** — all jobs with status, stage, customer, assignee, and elapsed time\n- **Time by Job** — total hours per job for a date range\n- **Expense Report** — all expenses by employee or category\n- **Parts Inventory** — current stock levels across all locations\n- **AR Aging** — outstanding invoice balances by age bucket\n\n### Custom Report Builder\n\nClick **New Report** to open the builder. Select a data source (Jobs, Parts, Time Entries, Expenses, etc.) and choose which fields to include as columns. Add filters, sorting, and grouping. Run the report to preview results.\n\n### Saving Reports\n\nOnce you've configured a report you want to reuse, click **Save**. Give it a name and optionally set a description. Saved reports appear in your list and can be shared with your team.\n\n### Exporting\n\nAny report can be exported to CSV or printed directly from the browser. PDF export is available for formatted reports like expense summaries and job summaries.","sections":[]}"""
        };

        var purchaseOrdersModule = new TrainingModule
        {
            Title = "Purchase Orders and Receiving",
            Slug = "purchase-orders-and-receiving",
            Summary = "How to create purchase orders, track vendor deliveries, and receive items into inventory.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 6,
            IsPublished = true,
            SortOrder = 2,
            AppRoutes = """["/purchase-orders"]""",
            Tags = """["purchasing","inventory"]""",
            ContentJson = """{"body":"## Purchase Orders and Receiving\n\nPurchase Orders (POs) track what you've ordered from vendors, what's been received, and what's still outstanding. QB Engineer creates POs manually or automatically when a job's BOM has 'Buy' components.\n\n### Creating a Purchase Order\n\nNavigate to **Purchase Orders** and click **New PO**. Select the vendor, add line items (part number, description, quantity, unit price), set the expected delivery date, and submit. POs sync to QuickBooks as vendor bills when received.\n\n### PO Statuses\n\n- **Draft** — being built, not yet sent to vendor\n- **Sent** — transmitted to vendor, awaiting delivery\n- **Partially Received** — some items have arrived\n- **Received** — all items have been received\n- **Cancelled** — order was cancelled\n\n### Receiving Items\n\nWhen a delivery arrives, open the PO and click **Receive**. The receiving dialog shows each line with an expected quantity. Enter the actual quantity received (which may be partial). The system records the receipt, updates inventory, and marks the PO line as received or partially received.\n\n### Linking POs to Jobs\n\nWhen you create a PO from a job's BOM, the PO lines are linked to the job. The job's materials stage automatically updates when all linked POs are fully received. This drives the Materials Ordered → Materials Received stage transition on the kanban board.","sections":[]}"""
        };

        var adminUsersModule = new TrainingModule
        {
            Title = "Admin: Managing Users and Roles",
            Slug = "admin-users-and-roles",
            Summary = "For admins and managers: how to create users, assign roles, configure kiosk access, and manage teams.",
            ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
            EstimatedMinutes = 7,
            IsPublished = true,
            SortOrder = 3,
            AppRoutes = """["/admin"]""",
            Tags = """["admin"]""",
            ContentJson = """{"body":"## Admin: Managing Users and Roles\n\nUser management in QB Engineer is handled through the Admin panel. Only users with the Admin or Manager role can access this area.\n\n### Creating a New User\n\nNavigate to **Admin → Users** and click **New User**. Fill in the user's name, email address, and role. Do NOT set a password — the system generates a secure setup token and emails it to the new user. The employee follows the link to set their own password and complete their account.\n\n### Roles\n\nQB Engineer uses additive roles:\n\n- **Engineer** — kanban board, assigned work, time tracking, expenses, files\n- **Production Worker** — simplified task list, start/stop timer, move cards, notes\n- **PM** — backlog, planning, leads, reporting (read-only board)\n- **Manager** — everything PM + assign work, approve expenses, set priorities\n- **Office Manager** — customer/vendor, invoice queue, employee documents\n- **Admin** — everything + user management, roles, system settings, track types\n\n### Kiosk Auth (Badge + PIN)\n\nFor shop floor workers who don't use a computer, enable Tier 2 kiosk authentication. In the user's profile, assign an employee barcode (printed on their badge). The worker scans their badge at a kiosk terminal and enters a 4–6 digit PIN to clock in, start timers, and move jobs.\n\n### Teams\n\nOrganize users into teams under **Admin → Teams**. Teams are used for filtered board views (swimlanes by team), workload reporting, and shift scheduling.\n\n### Deactivating Users\n\nWhen an employee leaves, deactivate their account from Admin → Users. Deactivated users cannot log in but all their historical data (jobs, time entries, expenses) is preserved.","sections":[]}"""
        };

        db.TrainingModules.AddRange(
            welcome, navigating, profileSetup, compliance, timeTracking, expenses, onboardingQuiz,
            kanbanBasics, jobManagement, jobDetail, partsCatalog, partsQuickRef, backlogPlanning, dashboard, engineerQuiz,
            reportsModule, purchaseOrdersModule, adminUsersModule
        );
        await db.SaveChangesAsync();

        // ── Training Paths ────────────────────────────────────────────────
        if (!await db.TrainingPaths.AnyAsync())
        {
            var onboardingPath = new TrainingPath
            {
                Title = "New Employee Onboarding",
                Slug = "new-employee-onboarding",
                Description = "Everything a new employee needs to get started: profile setup, compliance forms, time tracking, and expenses.",
                Icon = "waving_hand",
                IsAutoAssigned = true,
                IsActive = true,
                SortOrder = 1,
            };

            var engineerPath = new TrainingPath
            {
                Title = "Production Engineer Training",
                Slug = "production-engineer-training",
                Description = "Core training for production engineers: kanban board, job management, parts catalog, backlog, and planning.",
                Icon = "engineering",
                IsAutoAssigned = true,
                IsActive = true,
                SortOrder = 2,
                AllowedRoles = """["Admin","Manager","Engineer"]""",
            };

            db.TrainingPaths.AddRange(onboardingPath, engineerPath);
            await db.SaveChangesAsync();

            // ── Path-Module Associations ──────────────────────────────────
            var onboardingModules = new[]
            {
                (welcome.Id, 1), (navigating.Id, 2), (profileSetup.Id, 3),
                (compliance.Id, 4), (timeTracking.Id, 5), (expenses.Id, 6), (onboardingQuiz.Id, 7)
            };

            foreach (var (moduleId, position) in onboardingModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = onboardingPath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }

            var engineerModules = new[]
            {
                (kanbanBasics.Id, 1), (jobManagement.Id, 2), (jobDetail.Id, 3),
                (partsCatalog.Id, 4), (partsQuickRef.Id, 5), (backlogPlanning.Id, 6),
                (dashboard.Id, 7), (engineerQuiz.Id, 8)
            };

            foreach (var (moduleId, position) in engineerModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = engineerPath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }

            await db.SaveChangesAsync();
            Log.Information("Seeded training paths and {Count} path-module associations", onboardingModules.Length + engineerModules.Length);
        }

        // Back-fill enrollments for any existing users not yet enrolled in auto-assigned paths
        var autoPaths = await db.TrainingPaths
            .Where(p => p.IsAutoAssigned && p.IsActive && p.DeletedAt == null)
            .ToListAsync();

        var existingEnrollments = await db.TrainingPathEnrollments
            .Select(e => new { e.UserId, e.PathId })
            .ToListAsync();

        var users = await db.Users
            .Select(u => new { u.Id })
            .ToListAsync();

        var userRoles = await db.UserRoles
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name ?? "" })
            .ToListAsync();

        var rolesByUser = userRoles.GroupBy(r => r.UserId).ToDictionary(g => g.Key, g => g.Select(r => r.RoleName).ToArray());

        var newEnrollments = 0;
        foreach (var path in autoPaths)
        {
            var allowedRoles = string.IsNullOrEmpty(path.AllowedRoles)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<string[]>(path.AllowedRoles);

            foreach (var user in users)
            {
                if (existingEnrollments.Any(e => e.UserId == user.Id && e.PathId == path.Id))
                    continue;

                var userRoleNames = rolesByUser.TryGetValue(user.Id, out var roles) ? roles : [];
                if (allowedRoles != null && !userRoleNames.Any(r => allowedRoles.Contains(r)))
                    continue;

                db.TrainingPathEnrollments.Add(new TrainingPathEnrollment
                {
                    UserId = user.Id,
                    PathId = path.Id,
                    IsAutoAssigned = true,
                });
                newEnrollments++;
            }
        }

        if (newEnrollments > 0)
        {
            await db.SaveChangesAsync();
            Log.Information("Back-filled {Count} training enrollments for existing users", newEnrollments);
        }

        Log.Information("Seeded {Count} training modules", 18);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string firstName, string lastName,
        string initials, string avatarColor,
        string password, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return existing;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Initials = initials,
            AvatarColor = avatarColor,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            Log.Information("Created user: {Email} ({Role})", email, role);
        }
        else
        {
            Log.Error("Failed to create user {Email}: {Errors}", email,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return user;
    }
}
