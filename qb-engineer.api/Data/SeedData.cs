using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;
using Serilog;

namespace QBEngineer.Api.Data;

public static partial class SeedData
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

        var pmorris = await EnsureUserAsync(userManager, "pmorris@qbengineer.local",
            "P.", "Morris", "PM", "#0369a1", "Engineer123!", "PM");

        var lwilson = await EnsureUserAsync(userManager, "lwilson@qbengineer.local",
            "L.", "Wilson", "LW", "#7c3aed", "Engineer123!", "Manager");

        var cthompson = await EnsureUserAsync(userManager, "cthompson@qbengineer.local",
            "C.", "Thompson", "CT", "#b45309", "Engineer123!", "OfficeManager");

        var bkelly = await EnsureUserAsync(userManager, "bkelly@qbengineer.local",
            "B.", "Kelly", "BK", "#0f766e", "Engineer123!", "ProductionWorker");

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

            var today = DateTimeOffset.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // Helper
            int pos = 0;
            Job MakeJob(string number, string title, int trackTypeId, int stageId,
                int? assigneeId = null, int? customerId = null, DateTimeOffset? dueDate = null,
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
            var now = DateTimeOffset.UtcNow;

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

        // ── Sales Tax Rates ────────────────────────────────────────────────
        await SeedSalesTaxRatesAsync(db);

        // ── Historical Data ────────────────────────────────────────────────
        await SeedHistoricalDataAsync(db, admin.Id, akim.Id, dhart.Id, jsilva.Id, mreyes.Id,
            pmorris.Id, lwilson.Id, cthompson.Id, bkelly.Id);

        Log.Information("Database seeding complete");
    }

    private static async Task SeedSalesTaxRatesAsync(AppDbContext db)
    {
        if (await db.SalesTaxRates.AnyAsync()) return;

        var epoch = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // US state-level base rates (2024/2025). Rates are the STATE base rate only.
        // Local (county/city) rates add on top — admins should adjust to their effective
        // combined rate for each nexus jurisdiction.
        // Sources: Tax Foundation 2024 State Individual Income Tax Rates and Sales Tax Handbook.
        // Zero-rate states (OR, MT, NH, DE, AK) are included for completeness so the system
        // returns 0% automatically rather than falling back to the default.
        var states = new (string Code, string Name, decimal Rate)[]
        {
            ("AL", "Alabama",        0.0400m),
            ("AK", "Alaska",         0.0000m), // No state tax; local rates apply
            ("AZ", "Arizona",        0.0560m),
            ("AR", "Arkansas",       0.0650m),
            ("CA", "California",     0.0725m),
            ("CO", "Colorado",       0.0290m),
            ("CT", "Connecticut",    0.0635m),
            ("DE", "Delaware",       0.0000m), // No sales tax
            ("FL", "Florida",        0.0600m),
            ("GA", "Georgia",        0.0400m),
            ("HI", "Hawaii",         0.0400m), // General Excise Tax
            ("ID", "Idaho",          0.0600m),
            ("IL", "Illinois",       0.0625m),
            ("IN", "Indiana",        0.0700m),
            ("IA", "Iowa",           0.0600m),
            ("KS", "Kansas",         0.0650m),
            ("KY", "Kentucky",       0.0600m),
            ("LA", "Louisiana",      0.0445m),
            ("ME", "Maine",          0.0550m),
            ("MD", "Maryland",       0.0600m),
            ("MA", "Massachusetts",  0.0625m),
            ("MI", "Michigan",       0.0600m),
            ("MN", "Minnesota",      0.0688m),
            ("MS", "Mississippi",    0.0700m),
            ("MO", "Missouri",       0.0423m),
            ("MT", "Montana",        0.0000m), // No sales tax
            ("NE", "Nebraska",       0.0550m),
            ("NV", "Nevada",         0.0685m),
            ("NH", "New Hampshire",  0.0000m), // No sales tax
            ("NJ", "New Jersey",     0.0663m),
            ("NM", "New Mexico",     0.0500m), // Gross Receipts Tax
            ("NY", "New York",       0.0400m),
            ("NC", "North Carolina", 0.0475m),
            ("ND", "North Dakota",   0.0500m),
            ("OH", "Ohio",           0.0575m),
            ("OK", "Oklahoma",       0.0450m),
            ("OR", "Oregon",         0.0000m), // No sales tax
            ("PA", "Pennsylvania",   0.0600m),
            ("RI", "Rhode Island",   0.0700m),
            ("SC", "South Carolina", 0.0600m),
            ("SD", "South Dakota",   0.0420m),
            ("TN", "Tennessee",      0.0700m),
            ("TX", "Texas",          0.0625m),
            ("UT", "Utah",           0.0610m),
            ("VT", "Vermont",        0.0600m),
            ("VA", "Virginia",       0.0530m),
            ("WA", "Washington",     0.0650m),
            ("WV", "West Virginia",  0.0600m),
            ("WI", "Wisconsin",      0.0500m),
            ("WY", "Wyoming",        0.0400m),
        };

        var rates = states.Select(s => new SalesTaxRate
        {
            Name = $"{s.Name} Sales Tax",
            Code = s.Code,
            StateCode = s.Code,
            Rate = s.Rate,
            EffectiveFrom = epoch,
            EffectiveTo = null,
            IsDefault = false,
            IsActive = true,
            Description = s.Rate == 0
                ? $"{s.Name}: no state sales tax. Verify if local rates apply."
                : $"{s.Name} state base rate. Add local rates for your nexus jurisdictions.",
        }).ToList();

        // Mark a general default (0%) — overridden once the admin configures their state
        rates.Add(new SalesTaxRate
        {
            Name = "Default (No Tax)",
            Code = "DEFAULT",
            StateCode = null,
            Rate = 0.0000m,
            EffectiveFrom = epoch,
            EffectiveTo = null,
            IsDefault = true,
            IsActive = true,
            Description = "Fallback rate when no state-specific rate is found. Update to your default jurisdiction rate.",
        });

        db.SalesTaxRates.AddRange(rates);
        await db.SaveChangesAsync();
        Log.Information("Seeded {Count} sales tax rates (50 states + default)", rates.Count);
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
                new JobStage { TrackTypeId = production.Id, Name = "Materials Ordered", Code = "materials_ordered", SortOrder = 4, Color = "#8b5cf6", AccountingDocumentType = AccountingDocumentType.PurchaseOrder, IsShopFloor = true },
                new JobStage { TrackTypeId = production.Id, Name = "Materials Received", Code = "materials_received", SortOrder = 5, Color = "#a855f7", IsShopFloor = true },
                new JobStage { TrackTypeId = production.Id, Name = "In Production", Code = "in_production", SortOrder = 6, Color = "#f59e0b", IsShopFloor = true },
                new JobStage { TrackTypeId = production.Id, Name = "QC/Review", Code = "qc_review", SortOrder = 7, Color = "#ec4899", IsShopFloor = true },
                new JobStage { TrackTypeId = production.Id, Name = "Shipped", Code = "shipped", SortOrder = 8, Color = "#c2410c", AccountingDocumentType = AccountingDocumentType.Invoice, IsShopFloor = true },
                new JobStage { TrackTypeId = production.Id, Name = "Invoiced/Sent", Code = "invoiced_sent", SortOrder = 9, Color = "#dc2626", AccountingDocumentType = AccountingDocumentType.Invoice, IsIrreversible = true },
                new JobStage { TrackTypeId = production.Id, Name = "Payment Received", Code = "payment_received", SortOrder = 10, Color = "#15803d", AccountingDocumentType = AccountingDocumentType.Payment, IsIrreversible = true }
            );
            await db.SaveChangesAsync();

            var rnd = new TrackType { Name = "R&D/Tooling", Code = "rnd", SortOrder = 2, IsShopFloor = false };
            db.TrackTypes.Add(rnd);
            await db.SaveChangesAsync();

            db.JobStages.AddRange(
                new JobStage { TrackTypeId = rnd.Id, Name = "Concept", Code = "concept", SortOrder = 1, Color = "#94a3b8" },
                new JobStage { TrackTypeId = rnd.Id, Name = "Design", Code = "design", SortOrder = 2, Color = "#0d9488" },
                new JobStage { TrackTypeId = rnd.Id, Name = "Prototype", Code = "prototype", SortOrder = 3, Color = "#0ea5e9", IsShopFloor = true },
                new JobStage { TrackTypeId = rnd.Id, Name = "Test", Code = "test", SortOrder = 4, Color = "#f59e0b", IsShopFloor = true },
                new JobStage { TrackTypeId = rnd.Id, Name = "Iterate", Code = "iterate", SortOrder = 5, Color = "#ec4899", IsShopFloor = true },
                new JobStage { TrackTypeId = rnd.Id, Name = "Production Ready", Code = "production_ready", SortOrder = 6, Color = "#15803d" }
            );
            await db.SaveChangesAsync();

            var maintenance = new TrackType { Name = "Maintenance", Code = "maintenance", SortOrder = 3 };
            db.TrackTypes.Add(maintenance);
            await db.SaveChangesAsync();

            db.JobStages.AddRange(
                new JobStage { TrackTypeId = maintenance.Id, Name = "Requested", Code = "requested", SortOrder = 1, Color = "#94a3b8" },
                new JobStage { TrackTypeId = maintenance.Id, Name = "Scheduled", Code = "scheduled", SortOrder = 2, Color = "#0ea5e9", IsShopFloor = true },
                new JobStage { TrackTypeId = maintenance.Id, Name = "In Progress", Code = "in_progress", SortOrder = 3, Color = "#f59e0b", IsShopFloor = true },
                new JobStage { TrackTypeId = maintenance.Id, Name = "Complete", Code = "complete", SortOrder = 4, Color = "#15803d", IsShopFloor = true }
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
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_declined", Label = "Declined", SortOrder = 4 },
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_expired", Label = "Expired", SortOrder = 5 },
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_converted_to_quote", Label = "Converted to Quote", SortOrder = 6 },
                new ReferenceData { GroupCode = "quote_workflow_status", Code = "quote_status_converted_to_order", Label = "Converted to Order", SortOrder = 7 },

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
        await SeedAdditionalTrainingPathsAsync(db);
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

    private static async Task SeedAdditionalTrainingPathsAsync(AppDbContext db)
    {
        // Look up existing modules by slug for cross-path reuse
        var existingSlugs = await db.TrainingModules
            .AsNoTracking()
            .Select(m => new { m.Id, m.Slug })
            .ToListAsync();
        var bySlug = existingSlugs.ToDictionary(m => m.Slug, m => m.Id);

        // Helper: get or create a module (idempotent by slug)
        async Task<int> GetOrCreateModule(TrainingModule m)
        {
            if (bySlug.TryGetValue(m.Slug, out var existingId)) return existingId;
            db.TrainingModules.Add(m);
            await db.SaveChangesAsync();
            bySlug[m.Slug] = m.Id;
            return m.Id;
        }

        // ── Path 3: Shop Floor Worker ──────────────────────────────────────
        if (!await db.TrainingPaths.Where(p => p.Title == "Shop Floor Worker").AnyAsync())
        {
            var sfClockIn = new TrainingModule
            {
                Title = "Shop Floor Clock-In Walkthrough",
                Slug = "shop-floor-clock-in",
                Summary = "A step-by-step tour of the kiosk display, clocking in with your badge and PIN, and starting a job timer.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Walkthrough,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 1,
                AppRoutes = """["/shop-floor"]""",
                Tags = """["shop-floor","kiosk","time-tracking"]""",
                ContentJson = """{"appRoute":"/shop-floor","startButtonLabel":"Show Me the Kiosk","steps":[{"element":"[data-tour='kiosk-display']","popover":{"title":"Shop Floor Kiosk","description":"The kiosk display is your primary work interface on the shop floor. It shows the jobs queued for this workstation and lets you clock in, start timers, and move jobs — all without a full computer login.","side":"bottom"}},{"element":"[data-tour='kiosk-scan-input']","popover":{"title":"Scan Your Badge","description":"Hold your badge up to the scanner or barcode reader. If your badge isn't set up yet, ask your admin to assign a barcode to your profile under Admin → Users.","side":"bottom"}},{"element":"[data-tour='kiosk-pin-pad']","popover":{"title":"Enter Your PIN","description":"After scanning your badge, enter your 4–6 digit PIN on the number pad. This PIN is different from your password — it's set by your admin or you can change it under Account → Security.","side":"bottom"}},{"element":"[data-tour='quick-actions']","popover":{"title":"Quick Actions","description":"Once you're authenticated, the quick action panel appears. Large buttons let you Clock In, Clock Out, Start a job timer, Stop your current timer, or move a job to the next stage — all with a single tap.","side":"top"}},{"element":"[data-tour='active-jobs']","popover":{"title":"Your Active Jobs","description":"Your currently assigned jobs are shown below the quick actions. Tap any job to see its details, add a note, or start a timer directly from this screen.","side":"top"}}]}"""
            };

            var sfKiosk = new TrainingModule
            {
                Title = "Kiosk Authentication and Badge Setup",
                Slug = "kiosk-authentication",
                Summary = "How kiosk authentication works, what to do if your badge is not recognized, and how to reset your PIN.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 3,
                IsPublished = true,
                SortOrder = 2,
                AppRoutes = """["/shop-floor","/account/security"]""",
                Tags = """["shop-floor","kiosk","auth"]""",
                ContentJson = """{"body":"## Kiosk Authentication\n\nThe shop floor kiosk uses a two-factor badge+PIN system so workers can authenticate quickly without a keyboard.\n\n### How It Works\n\n1. **Scan your badge** — Hold your RFID card, NFC tag, or barcode badge up to the reader.\n2. **Enter your PIN** — Type your 4–6 digit PIN on the touchscreen keypad.\n3. **You're in** — The quick action panel appears. You stay logged in until you clock out or the session timeout.\n\n### If Your Badge Is Not Recognized\n\nIf you see 'Badge not found', your badge identifier hasn't been linked to your account yet. Ask your admin to open Admin → Users → your profile and add your badge under Scan Identifiers.\n\n### Changing Your PIN\n\nYour PIN is separate from your login password. To change it, log into the web app normally and go to Account → Security. Look for the PIN section. Enter your current PIN and set a new one. PINs must be 4–6 digits.\n\n### Forgot Your PIN\n\nIf you've forgotten your PIN, you cannot reset it yourself — it requires your admin to generate a new temporary PIN from your user profile. Contact your supervisor or admin.\n\n### Multiple Kiosks\n\nYour badge and PIN work on any kiosk terminal in the system. You don't need separate credentials per station.","sections":[]}"""
            };

            var sfScanning = new TrainingModule
            {
                Title = "Scanning Jobs and Moving Cards from the Floor",
                Slug = "shop-floor-scanning",
                Summary = "How to scan a job barcode to pull it up on the kiosk, start a timer, and advance it to the next stage.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 4,
                IsPublished = true,
                SortOrder = 3,
                AppRoutes = """["/shop-floor"]""",
                Tags = """["shop-floor","scanning","jobs"]""",
                ContentJson = """{"body":"## Scanning Jobs from the Shop Floor\n\nEvery job in QB Engineer has a QR code and barcode on its printed work order. Use the kiosk scanner to pull up a job instantly without searching.\n\n### Scanning a Work Order\n\n1. Authenticate on the kiosk with your badge and PIN.\n2. Point the scanner at the job's barcode (on the printed work order or label on the part traveler).\n3. The job appears on screen with its current stage, assignee, and subtasks.\n4. Tap **Start Timer** to begin tracking your time, or **Move to Next Stage** to advance the job.\n\n### Starting a Timer via Scan\n\nWhen you scan a job and tap Start Timer, the timer starts immediately and is logged against that job under your user account. You can only have one active timer at a time — scanning a second job and starting a timer will prompt you to stop the first one.\n\n### Moving a Job Forward\n\nIf you've completed your work on a job (e.g., finished machining), scan the job and tap **Advance Stage**. The job moves to the next stage on the kanban board. Some stage moves require confirmation if they're irreversible (Invoice, Payment).\n\n### Adding a Note via Scan\n\nScan the job and tap **Add Note**. A simple text input appears. Type your note and tap Submit. The note is added to the job's activity log and any @mentioned users receive a notification.\n\n### What If the Scanner Doesn't Work\n\nIf the barcode doesn't scan, type the job number manually in the search bar on the kiosk display. Job numbers always follow the format shown at the top of the work order.","sections":[]}"""
            };

            var sfQuickRef = new TrainingModule
            {
                Title = "Shop Floor Quick Reference",
                Slug = "shop-floor-quick-reference",
                Summary = "Quick reference for kiosk actions, barcode scanning, job statuses, and common troubleshooting.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.QuickRef,
                EstimatedMinutes = 2,
                IsPublished = true,
                SortOrder = 4,
                AppRoutes = """["/shop-floor"]""",
                Tags = """["shop-floor","reference"]""",
                ContentJson = """{"title":"Shop Floor Quick Reference","groups":[{"heading":"Kiosk Actions","items":[{"label":"Clock In","value":"Scan badge → enter PIN → tap Clock In"},{"label":"Start Timer","value":"Scan badge → scan job barcode → tap Start Timer"},{"label":"Stop Timer","value":"Tap active timer → Stop, or scan new job to auto-switch"},{"label":"Move Job Forward","value":"Scan job → Advance Stage (requires authentication)"},{"label":"Add Note","value":"Scan job → Add Note → type message → Submit"}]},{"heading":"Badge Troubleshooting","items":[{"label":"Badge not found","value":"Ask admin to add your badge under Admin → Users → Scan Identifiers"},{"label":"Wrong PIN","value":"3 wrong attempts locks session — wait 5 min or ask admin"},{"label":"Forgot PIN","value":"Ask admin to reset from your user profile"},{"label":"PIN change","value":"Web login → Account → Security → PIN section"}]},{"heading":"Job Status Indicators","items":[{"label":"Green border","value":"Job is on schedule — due date in the future"},{"label":"Yellow border","value":"Job is due today"},{"label":"Red border","value":"Job is overdue — escalate to supervisor"},{"label":"Timer icon","value":"Active time entry running against this job"}]}]}"""
            };

            int sfClockInId = await GetOrCreateModule(sfClockIn);
            int sfKioskId = await GetOrCreateModule(sfKiosk);
            int sfScanningId = await GetOrCreateModule(sfScanning);
            int sfQuickRefId = await GetOrCreateModule(sfQuickRef);
            bySlug.TryGetValue("logging-your-time", out var logTimeId);
            bySlug.TryGetValue("submitting-expenses", out var expensesId);

            var shopFloorPath = new TrainingPath
            {
                Title = "Shop Floor Worker",
                Slug = "shop-floor-worker",
                Description = "Everything a shop floor worker needs to clock in, scan jobs, track time, and submit expenses from the kiosk.",
                Icon = "factory",
                IsAutoAssigned = false,
                IsActive = true,
                SortOrder = 3,
                AllowedRoles = """["Admin","Manager","Engineer","ProductionWorker"]""",
            };
            db.TrainingPaths.Add(shopFloorPath);
            await db.SaveChangesAsync();

            var sfModules = new List<(int ModuleId, int Position)>
            {
                (sfClockInId, 1), (sfKioskId, 2), (sfScanningId, 3), (sfQuickRefId, 4),
            };
            if (logTimeId > 0) sfModules.Add((logTimeId, 5));
            if (expensesId > 0) sfModules.Add((expensesId, 6));

            foreach (var (moduleId, position) in sfModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = shopFloorPath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }
            await db.SaveChangesAsync();
            Log.Information("Seeded Shop Floor Worker training path");
        }

        // ── Path 4: Production Manager ─────────────────────────────────────
        if (!await db.TrainingPaths.Where(p => p.Title == "Production Manager").AnyAsync())
        {
            var approvingExpenses = new TrainingModule
            {
                Title = "Approving Expenses and Reviewing Submissions",
                Slug = "approving-expenses",
                Summary = "How managers review employee expense submissions, approve or reject them, and handle reimbursement workflows.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 1,
                AppRoutes = """["/expenses"]""",
                Tags = """["expenses","manager","approval"]""",
                ContentJson = """{"body":"## Approving Expenses as a Manager\n\nManagers can see all expense submissions from their team and are responsible for reviewing and approving or rejecting them.\n\n### Approval Queue\n\nNavigate to **Expenses → Approval Queue**. This view shows all submissions pending your review, sorted by submission date. You can filter by employee, date range, or category.\n\n### Reviewing a Submission\n\nClick any expense row to expand its details. You'll see:\n- The employee's description and category\n- The amount and date\n- A thumbnail of the receipt (click to enlarge)\n- The linked job (if applicable)\n\n### Approving\n\nIf everything looks correct, click **Approve**. The expense is marked Approved and will be included in the next payroll or reimbursement run depending on your company's process.\n\n### Rejecting with Feedback\n\nIf you need to reject a submission (wrong category, missing receipt, over policy limit), click **Reject** and enter a brief reason. The employee receives a notification with your reason and can resubmit with corrections.\n\n### Expense Policies\n\nExpenses over your company's policy limit may be flagged automatically. You'll see a warning indicator. These still need your manual review — the system flags them but doesn't auto-reject.\n\n### Reporting\n\nUse Reports → Expense Report to see all approved expenses by employee, job, date range, or category. This is the primary input for expense reimbursement and job costing.","sections":[]}"""
            };

            var capacityMonitor = new TrainingModule
            {
                Title = "Capacity and Workload Monitoring",
                Slug = "capacity-workload-monitoring",
                Summary = "How to use reports and the kanban board to monitor team workload, identify bottlenecks, and rebalance work.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 6,
                IsPublished = true,
                SortOrder = 2,
                AppRoutes = """["/reports","/kanban"]""",
                Tags = """["manager","capacity","planning"]""",
                ContentJson = """{"body":"## Capacity and Workload Monitoring\n\nAs a manager, you're responsible for making sure your team's workload is sustainable and that no single person is overloaded.\n\n### Board-Level Visibility\n\nThe Kanban Board's Team Swimlane view (toggle from the board filters) organizes cards by assignee. You can immediately see who has too much in their queue and who has capacity to take more.\n\nWIP limits per column are your first guardrail — when a column turns red, it's a signal to stop pulling work into that stage and finish what's there.\n\n### Workload Report\n\nNavigate to **Reports** and open the **Workload by Assignee** report. This shows each team member's active job count, total estimated hours, and remaining hours for the current planning cycle. Use this to identify imbalances before they become problems.\n\n### Time Distribution Report\n\nThe **Time by Employee** report shows actual hours logged per person over any date range. Compare this against planned hours from the planning cycle to spot who's running hot or cold.\n\n### Reassigning Jobs\n\nFrom the kanban board, Ctrl+Click to select one or more cards and use the bulk Assign action to reassign them. You can also open a job's detail panel and change the assignee directly.\n\n### Planning Cycle Adjustments\n\nIf a mid-cycle review shows the team is overloaded, open Planning and move uncommitted work back to the backlog. Protect the team's ability to finish what they started.","sections":[]}"""
            };

            var managerQuiz = new TrainingModule
            {
                Title = "Production Manager Assessment",
                Slug = "production-manager-quiz",
                Summary = "An assessment covering manager tools: expense approval, capacity monitoring, planning cycles, and reporting.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Quiz,
                EstimatedMinutes = 7,
                IsPublished = true,
                SortOrder = 3,
                AppRoutes = """["/training"]""",
                Tags = """["manager","quiz","assessment"]""",
                ContentJson = """{"passingScore":75,"questionsPerQuiz":8,"shuffleOptions":true,"showExplanationsAfterSubmit":true,"questions":[{"id":"pm1","text":"An employee submitted an expense with a missing receipt. What is the correct action?","options":[{"id":"a","text":"Approve it anyway — receipts are optional for small amounts"},{"id":"b","text":"Reject it with a reason so the employee can resubmit with the receipt attached","isCorrect":true},{"id":"c","text":"Delete the submission and ask them to resubmit from scratch"},{"id":"d","text":"Hold it in the queue until payday and decide then"}],"explanation":"Reject with a clear reason so the employee knows what's missing. They'll receive a notification with your feedback and can correct and resubmit. Don't hold submissions indefinitely."},{"id":"pm2","text":"The kanban board column 'In Production' has a red header. What should you do as a manager?","options":[{"id":"a","text":"Archive some jobs in that column to bring the count down"},{"id":"b","text":"Stop pulling new jobs into that stage and focus the team on completing the current ones","isCorrect":true},{"id":"c","text":"Remove the WIP limit to allow more work in that stage"},{"id":"d","text":"Reassign all jobs in that column to different team members"}],"explanation":"A red column header means the WIP limit has been exceeded. The right response is to stop adding work and focus on clearing the current load first. WIP limits protect quality and throughput."},{"id":"pm3","text":"You want to see which employees are working on what during the current planning cycle. Where do you look?","options":[{"id":"a","text":"Dashboard → Open Orders widget — it shows all active jobs"},{"id":"b","text":"Kanban Board → Team Swimlane view — shows cards grouped by assignee","isCorrect":true},{"id":"c","text":"Reports → Time Tracking → export the week"},{"id":"d","text":"Admin → Teams → member activity"}],"explanation":"The Team Swimlane view on the Kanban Board reorganizes cards by assignee, giving you an immediate visual of who is working on what and whether anyone is overloaded."},{"id":"pm4","text":"A team member is logging significantly fewer hours than expected. What is the best first step?","options":[{"id":"a","text":"Run the Time by Employee report to see their actual logged hours and compare to planned","isCorrect":true},{"id":"b","text":"Assume they are not working and issue a warning"},{"id":"c","text":"Delete their timer entries for the week and have them re-enter"},{"id":"d","text":"Move their jobs to someone else and wait"}],"explanation":"Start with data. Run the Time by Employee report to see what they've actually logged. There could be legitimate reasons (they forgot to start timers) or it could signal an issue that needs a conversation."},{"id":"pm5","text":"Mid-cycle, the team is clearly overloaded — too much committed work for the time remaining. What should you do?","options":[{"id":"a","text":"Leave it and expect overtime to absorb the difference"},{"id":"b","text":"Archive jobs in the current cycle to reduce the count"},{"id":"c","text":"Move lower-priority committed jobs back to the backlog to protect the team's ability to finish priority work","isCorrect":true},{"id":"d","text":"Extend the cycle length to accommodate all work"}],"explanation":"Moving work back to the backlog is the right answer. Committing to more than you can finish damages morale and quality. Protect the cycle by cutting scope, not extending time or expecting heroics."},{"id":"pm6","text":"Which report shows you each team member's active job count and estimated hours for the current cycle?","options":[{"id":"a","text":"Job Summary Report — filter by current cycle"},{"id":"b","text":"Workload by Assignee Report","isCorrect":true},{"id":"c","text":"Time by Employee Report — shows logged vs. planned"},{"id":"d","text":"AR Aging Report — includes job hours in the aging buckets"}],"explanation":"The Workload by Assignee report is purpose-built for capacity visibility. It shows active job counts, estimated hours, and remaining hours per person for the current planning cycle."},{"id":"pm7","text":"You need to move 5 jobs from one assignee to another because someone called in sick. What is the fastest way?","options":[{"id":"a","text":"Open each job individually and change the assignee one by one"},{"id":"b","text":"Ctrl+Click all 5 cards on the board and use the bulk Assign action","isCorrect":true},{"id":"c","text":"Archive all 5 jobs and reassign them from the backlog"},{"id":"d","text":"Update the team's schedule in Admin → Teams"}],"explanation":"Ctrl+Click selects multiple cards. With all 5 selected, the bulk Assign action lets you change the assignee for all of them in a single operation — no need to open each card individually."},{"id":"pm8","text":"A planning cycle is ending and several committed jobs are not complete. What happens to them?","options":[{"id":"a","text":"They are automatically archived as missed"},{"id":"b","text":"They are deleted and need to be re-entered next cycle"},{"id":"c","text":"They roll over to the next cycle or you can return them to the backlog","isCorrect":true},{"id":"d","text":"They automatically move to the first stage of the next cycle's board"}],"explanation":"Incomplete committed jobs don't disappear. You choose what to do with them: roll them over to the next cycle automatically, or move them back to the backlog if priorities have changed."},{"id":"pm9","text":"You want to understand job costing for the last month — how many hours were spent per job and by whom. Which report do you use?","options":[{"id":"a","text":"Expense Report — includes time as a cost line"},{"id":"b","text":"Time by Job report filtered to last 30 days","isCorrect":true},{"id":"c","text":"Job Summary report — it includes hour totals automatically"},{"id":"d","text":"AR Aging report — it calculates cost vs. billable"}],"explanation":"The Time by Job report shows hours logged per job, with breakdowns by employee. Filter to your date range for a clean job costing view. This is the primary input for understanding where your team's time is going."},{"id":"pm10","text":"An employee's expense submission has been sitting in the approval queue for 2 weeks. What should you check?","options":[{"id":"a","text":"Nothing — employees can wait indefinitely in the queue"},{"id":"b","text":"Whether it was accidentally rejected and the employee wasn't notified"},{"id":"c","text":"Check if another manager already approved it or if it needs your action","isCorrect":true},{"id":"d","text":"Automatically approve all expenses over 14 days old"}],"explanation":"Check the Approval Queue to see the submission's current status. It may need your action, may have been approved by another manager, or may have been rejected and the employee doesn't know. Old pending items deserve attention."},{"id":"pm11","text":"What does the WIP limit on a kanban column represent?","options":[{"id":"a","text":"The maximum number of files that can be attached to jobs in that column"},{"id":"b","text":"The recommended maximum number of jobs that should be in that stage simultaneously","isCorrect":true},{"id":"c","text":"The number of employees allowed to work on jobs in that stage"},{"id":"d","text":"The maximum budget allocated to jobs at that stage"}],"explanation":"WIP limits set a cap on how many jobs should be in a stage at once. They prevent overloading any single stage and encourage flow — finish work in progress before pulling more in."},{"id":"pm12","text":"You want to review all expenses charged to a specific job. How do you do this?","options":[{"id":"a","text":"Open the job detail panel → Expenses tab","isCorrect":true},{"id":"b","text":"Expenses page → filter by job number"},{"id":"c","text":"Reports → Job Summary → click the job row to see expenses"},{"id":"d","text":"Admin → Audit Log → filter by job"}],"explanation":"The job detail panel has an Expenses tab showing all expense submissions linked to that job. This gives you quick access to all job-related costs without leaving the job context."}]}"""
            };

            int approveExpId = await GetOrCreateModule(approvingExpenses);
            int capacityId = await GetOrCreateModule(capacityMonitor);
            int managerQuizId = await GetOrCreateModule(managerQuiz);
            bySlug.TryGetValue("kanban-board-basics", out var kanbanId);
            bySlug.TryGetValue("backlog-and-planning", out var backlogId);
            bySlug.TryGetValue("reports-and-analytics", out var reportsId);

            var managerPath = new TrainingPath
            {
                Title = "Production Manager",
                Slug = "production-manager",
                Description = "Training for production managers: team oversight, expense approval, capacity monitoring, reporting, and planning.",
                Icon = "manage_accounts",
                IsAutoAssigned = false,
                IsActive = true,
                SortOrder = 4,
                AllowedRoles = """["Admin","Manager"]""",
            };
            db.TrainingPaths.Add(managerPath);
            await db.SaveChangesAsync();

            var pmModules = new List<(int ModuleId, int Position)>
            {
                (approveExpId, 1), (capacityId, 2),
            };
            if (kanbanId > 0) pmModules.Add((kanbanId, 3));
            if (backlogId > 0) pmModules.Add((backlogId, 4));
            if (reportsId > 0) pmModules.Add((reportsId, 5));
            pmModules.Add((managerQuizId, 6));

            foreach (var (moduleId, position) in pmModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = managerPath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }
            await db.SaveChangesAsync();
            Log.Information("Seeded Production Manager training path");
        }

        // ── Path 5: Office & Finance ───────────────────────────────────────
        if (!await db.TrainingPaths.Where(p => p.Title == "Office and Finance").AnyAsync())
        {
            var customersModule = new TrainingModule
            {
                Title = "Customers and Contacts",
                Slug = "customers-and-contacts",
                Summary = "How to manage the customer list, add contacts, and track customer addresses for order fulfillment.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 1,
                AppRoutes = """["/customers"]""",
                Tags = """["customers","contacts","office"]""",
                ContentJson = """{"body":"## Customers and Contacts\n\nThe Customers module is your CRM. It tracks who you do business with, their contact information, billing and shipping addresses, and their order history.\n\n### Customer List\n\nNavigate to **Customers** in the sidebar. The table shows all active customers with their primary contact, phone, and recent order activity. Use the search box to find a customer by name, city, or email.\n\n### Adding a Customer\n\nClick **New Customer** and fill in the company name, billing address, payment terms, and credit limit. At least one contact is required — add the primary contact's name, title, email, and phone.\n\n### Multiple Addresses\n\nCustomers can have multiple shipping addresses (e.g., different warehouse locations). Open a customer record and go to the **Addresses** tab to add, edit, or set a default shipping address. When creating a shipment, you'll select from these addresses.\n\n### Contacts\n\nThe **Contacts** tab shows all people at that company. Each contact can have a direct phone number, email, and role (e.g., AP Contact, Engineering, Purchasing). Having accurate contacts helps route questions to the right person.\n\n### Customer Orders and History\n\nThe **Orders** tab shows all sales orders and quotes for that customer. Click any order to open it directly. This is the fastest way to look up 'what's the status of Joe's order from last month.'\n\n### Notes and Activities\n\nUse the activity log on each customer record to log calls, emails, and follow-up notes. Keep the record current so the whole team has context.","sections":[]}"""
            };

            var quotesModule = new TrainingModule
            {
                Title = "Quotes and Estimates",
                Slug = "quotes-and-estimates",
                Summary = "How to create price quotes, add line items, apply discount and tax, and convert a quote to a sales order.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 6,
                IsPublished = true,
                SortOrder = 2,
                AppRoutes = """["/quotes"]""",
                Tags = """["quotes","sales","office"]""",
                ContentJson = """{"body":"## Quotes and Estimates\n\nA quote is a formal price proposal sent to a customer before an order is placed. In QB Engineer, quotes link directly to jobs — when a customer accepts, the quote converts to a Sales Order and a job is created on the kanban board.\n\n### Creating a Quote\n\nNavigate to **Quotes** and click **New Quote**. Select the customer, set the expiration date, and add line items. Each line item can reference a part from your Parts Catalog (which pulls in the standard price) or be entered manually.\n\n### Pricing and Discounts\n\nFor each line item, set the unit price, quantity, and optionally a per-line discount percentage. A running total at the bottom of the quote updates as you add items.\n\n### Tax and Shipping\n\nAdd applicable sales tax and shipping charges as separate line items or use the Tax and Shipping fields at the bottom of the quote. Tax rates can be set per-customer from their record.\n\n### Sending to the Customer\n\nClick **Send** to generate a PDF quote. The PDF is automatically emailed to the customer's primary billing contact (or any contact you specify) and a copy is stored in the quote's Files section.\n\n### Quote Statuses\n\n- **Draft** — being built, not sent\n- **Sent** — delivered to customer, awaiting response\n- **Accepted** — customer approved\n- **Rejected** — customer declined\n- **Expired** — past the expiration date\n\n### Converting to a Sales Order\n\nWhen a customer accepts, click **Convert to Sales Order**. A Sales Order is created with the same line items, and if the job doesn't exist yet, you'll be prompted to create one on the kanban board.","sections":[]}"""
            };

            var invoicingModule = new TrainingModule
            {
                Title = "Invoicing and Billing",
                Slug = "invoicing-and-billing",
                Summary = "How to create invoices from shipped jobs or sales orders, send them to customers, and manage invoice statuses.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 6,
                IsPublished = true,
                SortOrder = 3,
                AppRoutes = """["/invoices"]""",
                Tags = """["invoices","billing","office"]""",
                ContentJson = """{"body":"## Invoicing and Billing\n\nInvoices are generated after work is shipped or services are delivered. In QB Engineer (standalone mode), you manage the full invoice lifecycle locally. If QuickBooks is connected, invoices sync there automatically.\n\n### Creating an Invoice\n\nNavigate to **Invoices** and click **New Invoice**. The most common flow is to start from a shipped Sales Order — click **Create Invoice from SO** and select the order. Line items are pre-populated from the SO.\n\nAlternatively, create a blank invoice and add line items manually.\n\n### Invoice Statuses\n\n- **Draft** — being prepared\n- **Sent** — emailed or mailed to customer\n- **Partial** — customer has made a partial payment\n- **Paid** — fully paid\n- **Void** — cancelled (preserved for audit trail)\n\n### Sending the Invoice\n\nClick **Send** to email the invoice as a PDF to the customer's billing contact. A copy is attached to the invoice record. You can also print and mail it.\n\n### Applying Payments\n\nWhen a customer pays, go to **Payments** and create a new payment. Select the customer and the invoices to apply it to. QB Engineer tracks the remaining balance and updates invoice status automatically.\n\n### Late Invoices\n\nThe **AR Aging** report in Reports shows all outstanding invoices bucketed by age (0–30, 31–60, 61–90, 90+ days). This is the key tool for collections follow-up.","sections":[]}"""
            };

            var paymentsModule = new TrainingModule
            {
                Title = "Payments and Accounts Receivable",
                Slug = "payments-and-ar",
                Summary = "How to record customer payments, apply them to invoices, and use AR aging to track outstanding balances.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 4,
                AppRoutes = """["/payments"]""",
                Tags = """["payments","ar","office"]""",
                ContentJson = """{"body":"## Payments and Accounts Receivable\n\n### Recording a Payment\n\nNavigate to **Payments** and click **New Payment**. Select the customer, enter the amount received, payment date, and method (Check, ACH, Credit Card, Wire). Then select which invoices this payment applies to.\n\n### Partial Payments\n\nIf the payment doesn't cover the full invoice, apply it as a partial. The invoice status updates to 'Partial' and the remaining balance is tracked. The next payment can be applied to the same invoice.\n\n### Overpayments\n\nIf a customer overpays, the excess amount becomes a credit on their account. You can apply it to future invoices.\n\n### AR Aging Report\n\nNavigate to **Reports** and run **AR Aging**. This shows all open balances grouped by how old they are:\n- 0–30 days: Current — just sent, normal\n- 31–60 days: Follow up if no response\n- 61–90 days: Escalate — this needs attention\n- 90+ days: Consider collections or write-off\n\nSort by customer or by age bucket to prioritize your collection calls.\n\n### Payment Terms\n\nPayment terms are set per customer (Net 15, Net 30, Net 45, etc.). They appear on every invoice and drive the AR aging calculation. Update a customer's terms from their record → Billing section.","sections":[]}"""
            };

            var vendorsModule = new TrainingModule
            {
                Title = "Vendors and Vendor Management",
                Slug = "vendors-management",
                Summary = "How to manage vendors, track contact information, and link vendors to purchase orders and parts.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 4,
                IsPublished = true,
                SortOrder = 5,
                AppRoutes = """["/vendors"]""",
                Tags = """["vendors","purchasing","office"]""",
                ContentJson = """{"body":"## Vendors and Vendor Management\n\nVendors are the companies you purchase materials, services, and tooling from. The Vendors module keeps their contact info, payment terms, and order history in one place.\n\n### Vendor List\n\nNavigate to **Vendors**. The table shows all active vendors with their primary contact and recent PO activity. Search by name or filter by category.\n\n### Adding a Vendor\n\nClick **New Vendor** and fill in the company name, address, payment terms, and primary contact. If this vendor handles materials for specific parts, you can link them from the Parts Catalog BOM.\n\n### Vendor Contacts\n\nAdd multiple contacts under the **Contacts** tab — Account Manager, Sales Rep, AP Contact, etc. Keep emails and direct phone numbers current so you can reach the right person when a delivery is late or there's a quality issue.\n\n### Purchase History\n\nThe **Purchase Orders** tab on each vendor record shows all POs you've ever sent to that vendor. Click any PO to view its status and received quantities. This is useful for verifying pricing history before creating a new order.\n\n### Vendor Performance Notes\n\nUse the activity log on the vendor record to note delivery issues, quality concerns, or price changes. This institutional knowledge is shared across your team.","sections":[]}"""
            };

            var officeQuickRef = new TrainingModule
            {
                Title = "Office and Finance Quick Reference",
                Slug = "office-finance-quick-reference",
                Summary = "Quick reference for order-to-cash workflow, invoice statuses, payment terms, and common office actions.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.QuickRef,
                EstimatedMinutes = 2,
                IsPublished = true,
                SortOrder = 6,
                AppRoutes = """["/invoices","/payments","/quotes","/sales-orders"]""",
                Tags = """["office","reference","billing"]""",
                ContentJson = """{"title":"Office and Finance Quick Reference","groups":[{"heading":"Order-to-Cash Workflow","items":[{"label":"Step 1","value":"Quote → customer accepts → Convert to Sales Order"},{"label":"Step 2","value":"Sales Order → production begins → job moves through kanban stages"},{"label":"Step 3","value":"Job shipped → Create Invoice from Sales Order"},{"label":"Step 4","value":"Invoice sent → customer pays → Record Payment → apply to invoice"}]},{"heading":"Invoice Statuses","items":[{"label":"Draft","value":"Being prepared — not sent to customer yet"},{"label":"Sent","value":"Emailed or delivered — awaiting payment"},{"label":"Partial","value":"Customer has paid some but not all"},{"label":"Paid","value":"Fully paid — balance is zero"},{"label":"Void","value":"Cancelled — preserved for audit"}]},{"heading":"Payment Terms","items":[{"label":"Net 15","value":"Payment due 15 days from invoice date"},{"label":"Net 30","value":"Payment due 30 days from invoice date"},{"label":"Net 45 / Net 60","value":"Extended terms for large customers"},{"label":"Due on Receipt","value":"Payment expected immediately upon delivery"}]},{"heading":"AR Aging Buckets","items":[{"label":"0–30 days","value":"Current — normal, no action needed"},{"label":"31–60 days","value":"Follow up with a courtesy reminder"},{"label":"61–90 days","value":"Escalate — call the customer directly"},{"label":"90+ days","value":"Consider collections or payment plan"}]}]}"""
            };

            int custId = await GetOrCreateModule(customersModule);
            int quotesId = await GetOrCreateModule(quotesModule);
            int invoiceId = await GetOrCreateModule(invoicingModule);
            int paymentsId = await GetOrCreateModule(paymentsModule);
            int vendorsId = await GetOrCreateModule(vendorsModule);
            int officeQRId = await GetOrCreateModule(officeQuickRef);
            bySlug.TryGetValue("purchase-orders-and-receiving", out var poId);

            var officePath = new TrainingPath
            {
                Title = "Office and Finance",
                Slug = "office-and-finance",
                Description = "Training for office and finance staff: quotes, sales orders, invoicing, payments, AR, and vendor management.",
                Icon = "account_balance",
                IsAutoAssigned = false,
                IsActive = true,
                SortOrder = 5,
                AllowedRoles = """["Admin","Manager","OfficeManager"]""",
            };
            db.TrainingPaths.Add(officePath);
            await db.SaveChangesAsync();

            var officeModules = new List<(int ModuleId, int Position)>
            {
                (custId, 1), (quotesId, 2), (invoiceId, 3), (paymentsId, 4), (vendorsId, 5), (officeQRId, 6),
            };
            if (poId > 0) officeModules.Add((poId, 7));

            foreach (var (moduleId, position) in officeModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = officePath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }
            await db.SaveChangesAsync();
            Log.Information("Seeded Office and Finance training path");
        }

        // ── Path 6: Parts, Inventory & Quality ────────────────────────────
        if (!await db.TrainingPaths.Where(p => p.Title == "Parts, Inventory and Quality").AnyAsync())
        {
            var inventoryModule = new TrainingModule
            {
                Title = "Inventory Management",
                Slug = "inventory-management",
                Summary = "How to view stock levels, manage bin locations, process receipts, and track material movements.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 6,
                IsPublished = true,
                SortOrder = 1,
                AppRoutes = """["/inventory"]""",
                Tags = """["inventory","parts","stock"]""",
                ContentJson = """{"body":"## Inventory Management\n\nThe Inventory module tracks your physical stock — where it is, how much you have, and where it's been. Every receipt, issue, and transfer creates a permanent movement record.\n\n### Stock Overview\n\nNavigate to **Inventory**. The Stock tab shows all stocked parts with current quantities across all bin locations. Filter by location, part type, or search by part number. Parts with zero stock or below minimum level are highlighted.\n\n### Bin Locations\n\nInventory is organized into storage locations (e.g., Warehouse A, Shelf B3, Rack C) and bins within those locations. A single part can be in multiple bins. The location path is shown for each bin (e.g., Warehouse A → Shelf B3 → Bin 12).\n\n### Receiving Stock\n\nWhen a PO delivery arrives, go to **Purchase Orders**, find the PO, and click **Receive**. Enter the actual quantity received per line and assign a bin location. Stock levels update immediately and the movement is logged.\n\n### Issuing Stock\n\nWhen stock is consumed by production (pulled for a job), record it as an issue from the inventory movement screen. Select the part, quantity, source bin, and the job it's being consumed by. This creates a consumption record.\n\n### Adjustments\n\nFor cycle counts or corrections, use **Inventory → Adjust**. Enter the part, bin, and new quantity. Provide a reason (Cycle Count, Write-Off, Correction). All adjustments are logged with who made them and why.\n\n### Minimum Stock Levels\n\nSet minimum stock levels per part to trigger low-stock alerts. When quantity drops below the minimum, the part appears highlighted in red in the inventory list and shows on the Dashboard's low-stock widget.","sections":[]}"""
            };

            var binTransfers = new TrainingModule
            {
                Title = "Bin Locations and Stock Transfers",
                Slug = "bin-locations-stock-transfers",
                Summary = "A guided walkthrough of navigating bin locations, moving stock between bins, and performing a cycle count.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Walkthrough,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 2,
                AppRoutes = """["/inventory"]""",
                Tags = """["inventory","bins","transfers"]""",
                ContentJson = """{"appRoute":"/inventory","startButtonLabel":"Tour Inventory","steps":[{"element":"[data-tour='inventory-tabs']","popover":{"title":"Inventory Tabs","description":"Inventory has three tabs: Stock (current quantities per part), Locations (bin location tree), and Movements (full audit log of all receipts, issues, transfers, and adjustments).","side":"bottom"}},{"element":"[data-tour='stock-table']","popover":{"title":"Stock Table","description":"Each row is a part+bin combination. The quantity shown is the current on-hand count. Parts with quantities below the minimum level are highlighted.","side":"bottom"}},{"element":"[data-tour='transfer-btn']","popover":{"title":"Transfer Stock Between Bins","description":"Click Transfer on any stock row to move some or all of that quantity to a different bin. Both the source and destination movement records are created automatically.","side":"left"}},{"element":"[data-tour='adjust-btn']","popover":{"title":"Adjust for Cycle Count","description":"Use Adjust to correct quantities from a physical count. Select the bin, enter the counted quantity, and provide a reason. The adjustment delta is recorded.","side":"left"}},{"element":"[data-tour='movements-tab']","popover":{"title":"Movement History","description":"The Movements tab shows every stock transaction — receipts, issues, transfers, and adjustments — with the date, user, and quantity. This is your complete audit trail.","side":"bottom"}}]}"""
            };

            var qualityModule = new TrainingModule
            {
                Title = "Quality Inspections and QC Templates",
                Slug = "quality-inspections-qc",
                Summary = "How to create quality inspection templates, run inspections against jobs or lots, and review QC results.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 6,
                IsPublished = true,
                SortOrder = 3,
                AppRoutes = """["/quality"]""",
                Tags = """["quality","qc","inspections"]""",
                ContentJson = """{"body":"## Quality Inspections and QC Templates\n\n### QC Templates\n\nNavigate to **Quality → Templates**. Templates define what gets checked during an inspection — visual checks, dimensional measurements, functional tests, and pass/fail criteria. Each template is reusable across multiple jobs or lots.\n\nTo create a template, click **New Template**. Add check items with their type (Pass/Fail, Measurement, Count), required values or tolerances, and whether each item is critical (a critical fail blocks the job from advancing).\n\n### Running an Inspection\n\nWhen a job reaches the QC/Review stage on the kanban board, an inspection can be triggered automatically (if configured) or manually. Navigate to **Quality → Inspections** and click **New Inspection**. Select the job, the template to use, and the inspector.\n\nWork through the checklist: enter measurements, mark pass/fail for each item, and add notes or photos for any failures.\n\n### Inspection Results\n\nAfter completing all items, submit the inspection. The system calculates an overall pass/fail based on your template rules. A passed inspection can advance the job. A failed inspection triggers a hold — the job can't move forward until the failure is addressed and a re-inspection passes.\n\n### Production Lots\n\nFor repetitive production (same part number, multiple units in a run), create a Lot from the Quality module. Inspections against a lot track pass/fail rates across the whole run, giving you yield data.\n\n### Reports\n\nThe QC Rejection Report in Reports shows failure rates by part, template check, or date range. Use this to identify chronic quality issues and drive process improvements.","sections":[]}"""
            };

            var inventoryQR = new TrainingModule
            {
                Title = "Inventory Quick Reference",
                Slug = "inventory-quick-reference",
                Summary = "Quick reference for inventory actions, stock movement types, and bin location concepts.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.QuickRef,
                EstimatedMinutes = 2,
                IsPublished = true,
                SortOrder = 4,
                AppRoutes = """["/inventory"]""",
                Tags = """["inventory","reference"]""",
                ContentJson = """{"title":"Inventory Quick Reference","groups":[{"heading":"Movement Types","items":[{"label":"Receipt","value":"Stock received from a vendor PO — increases quantity"},{"label":"Issue","value":"Stock consumed by a job — decreases quantity"},{"label":"Transfer","value":"Stock moved from one bin to another — no net change"},{"label":"Adjustment","value":"Manual correction (cycle count, write-off) — changes quantity with logged reason"}]},{"heading":"Common Actions","items":[{"label":"View current stock","value":"Inventory → Stock tab"},{"label":"Receive from PO","value":"Purchase Orders → open PO → Receive"},{"label":"Transfer between bins","value":"Inventory → Stock → Transfer button on any row"},{"label":"Cycle count adjustment","value":"Inventory → Stock → Adjust button"},{"label":"View movement history","value":"Inventory → Movements tab"}]},{"heading":"Stock Status Indicators","items":[{"label":"Green","value":"Stock above minimum level — healthy"},{"label":"Yellow","value":"Stock at or near minimum level — reorder soon"},{"label":"Red","value":"Stock below minimum or zero — action required"},{"label":"Reserved","value":"Quantity reserved for a job — do not use for other orders"}]}]}"""
            };

            var partsInventoryQuiz = new TrainingModule
            {
                Title = "Parts and Inventory Assessment",
                Slug = "parts-inventory-quiz",
                Summary = "An assessment covering parts catalog, BOM management, inventory movements, and quality inspections.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Quiz,
                EstimatedMinutes = 7,
                IsPublished = true,
                SortOrder = 5,
                AppRoutes = """["/training"]""",
                Tags = """["parts","inventory","quality","quiz"]""",
                ContentJson = """{"passingScore":75,"questionsPerQuiz":8,"shuffleOptions":true,"showExplanationsAfterSubmit":true,"questions":[{"id":"pi1","text":"You received 50 units of a raw material from a vendor. Where do you record this in QB Engineer?","options":[{"id":"a","text":"Inventory → Stock → Adjust (enter new total quantity)"},{"id":"b","text":"Purchase Orders → open the PO → Receive (enter actual qty received)","isCorrect":true},{"id":"c","text":"Parts Catalog → part record → Inventory tab → Add Stock"},{"id":"d","text":"Create a new inventory movement manually in the Movements tab"}],"explanation":"Always receive stock through the Purchase Order. This links the receipt to the vendor, creates the movement record, and closes the PO line — all in one step. Using Adjust would work but bypasses the PO linkage."},{"id":"pi2","text":"A QC inspection finds a critical defect on a job. What happens next?","options":[{"id":"a","text":"The job is automatically archived as failed"},{"id":"b","text":"The job is placed on hold and cannot advance until the failure is addressed and a re-inspection passes","isCorrect":true},{"id":"c","text":"The job moves to the next stage with a defect flag attached"},{"id":"d","text":"An email is sent to the customer explaining the delay"}],"explanation":"Critical failures create a hold on the job — it cannot move to the next stage until the issue is corrected and a follow-up inspection passes. This is a quality gate built into the workflow."},{"id":"pi3","text":"A part has BOM source type 'Make'. You just created a job for the parent part. What else should happen?","options":[{"id":"a","text":"Nothing — 'Make' is just a label, no automatic actions"},{"id":"b","text":"A child sub-job is created for the Make component linked to the parent job","isCorrect":true},{"id":"c","text":"A purchase order is automatically created for the component"},{"id":"d","text":"The component is reserved from inventory immediately"}],"explanation":"'Make' source means manufacturing in-house. When a job is created for the parent part, the system creates a linked child sub-job for any Make component. Both jobs then appear on the board."},{"id":"pi4","text":"You want to move 20 units of aluminum bar from Shelf A to Shelf B. How do you record this?","options":[{"id":"a","text":"Create two Adjust transactions — decrease Shelf A, increase Shelf B"},{"id":"b","text":"Inventory → Stock → Transfer on the Shelf A row, entering 20 units and selecting Shelf B as destination","isCorrect":true},{"id":"c","text":"Delete the Shelf A stock record and add a new record for Shelf B"},{"id":"d","text":"You cannot move stock — location is set when received and is permanent"}],"explanation":"The Transfer action on a stock row moves quantities between bins in a single transaction. It creates both the outbound and inbound movement records automatically, keeping the audit trail clean."},{"id":"pi5","text":"What does a 'Reserved' quantity indicator on a stock row mean?","options":[{"id":"a","text":"The stock has been quarantined due to a quality hold"},{"id":"b","text":"The stock is set aside for a specific job and should not be used for other orders","isCorrect":true},{"id":"c","text":"The vendor reserved this quantity in their warehouse for a future PO"},{"id":"d","text":"The stock record is locked by another user and cannot be edited"}],"explanation":"Reserved quantities are allocated to a specific job or order. They're still physically in the bin, but the system treats them as committed. Unreserved (available) quantity is what you can use for new orders."},{"id":"pi6","text":"You need to understand how many units of PN-1042 you used last quarter. Where do you find this?","options":[{"id":"a","text":"Parts Catalog → PN-1042 → Inventory tab → movement history","isCorrect":true},{"id":"b","text":"Inventory → Stock → click the part row to see history"},{"id":"c","text":"Reports → Parts Usage → filter by part number"},{"id":"d","text":"Both A and B are correct — either path works"}],"explanation":"The Part detail Inventory tab shows all movements (receipts, issues, transfers, adjustments) for that specific part. The Inventory Movements tab also works but you'd need to filter it by part number."},{"id":"pi7","text":"A job is at the QC/Review stage. The quality inspection passes. What typically happens next?","options":[{"id":"a","text":"The job is automatically archived as complete"},{"id":"b","text":"The inspector must manually re-open the kanban board and drag the job to Shipped"},{"id":"c","text":"The passed inspection removes the QC hold and the job can advance to the next stage (Shipped)","isCorrect":true},{"id":"d","text":"A customer notification is sent automatically and the job is invoiced"}],"explanation":"A passed inspection clears the QC gate. The job is now eligible to move to the next stage (Shipped). The stage move is still manual — someone needs to drag or move the card — but the quality block is removed."},{"id":"pi8","text":"You want to know the current stock quantity of a specific part across ALL bin locations. What is the best way?","options":[{"id":"a","text":"Check each bin location individually and add up the numbers"},{"id":"b","text":"Parts Catalog → part detail → Inventory tab — shows total and per-bin breakdown","isCorrect":true},{"id":"c","text":"Inventory → Stock tab — the part appears once per bin, add manually"},{"id":"d","text":"Reports → Stock Summary — only that report shows totals"}],"explanation":"The part detail Inventory tab shows the total on-hand quantity at the top, then a breakdown by bin location below. This is the fastest single view for total stock across all locations."},{"id":"pi9","text":"A new design revision has been approved for part PN-1042. How should this be handled in QB Engineer?","options":[{"id":"a","text":"Edit the existing part record to reflect the new design"},{"id":"b","text":"Archive PN-1042 and create PN-1042A as a new part"},{"id":"c","text":"Create a new revision (e.g., Rev D) in the part's Revisions tab","isCorrect":true},{"id":"d","text":"Duplicate the part with a new part number"}],"explanation":"Always use the Revisions system. Creating a new revision preserves the full history of the previous revision — BOM, process steps, inspection records — while recording what changed and when the change was approved."},{"id":"pi10","text":"You're doing a cycle count and find 18 units in a bin, but the system shows 22. How do you correct this?","options":[{"id":"a","text":"Transfer 4 units out of the bin to balance the count"},{"id":"b","text":"Delete the stock record and recreate it with 18 units"},{"id":"c","text":"Inventory → Stock → Adjust → enter 18 as the new quantity with reason 'Cycle Count'","isCorrect":true},{"id":"d","text":"Only managers can make adjustments — submit a request to your supervisor"}],"explanation":"The Adjust action is designed exactly for cycle count discrepancies. Enter the actual counted quantity, select 'Cycle Count' as the reason, and the system records the adjustment delta (+/-4 in this case) in the movement history."}]}"""
            };

            int invId = await GetOrCreateModule(inventoryModule);
            int binId = await GetOrCreateModule(binTransfers);
            int qualId = await GetOrCreateModule(qualityModule);
            int invQRId = await GetOrCreateModule(inventoryQR);
            int piqId = await GetOrCreateModule(partsInventoryQuiz);
            bySlug.TryGetValue("parts-catalog-basics", out var partsCatalogId);
            bySlug.TryGetValue("parts-quick-reference", out var partsQRId);
            bySlug.TryGetValue("purchase-orders-and-receiving", out var poId2);

            var piqPath = new TrainingPath
            {
                Title = "Parts, Inventory and Quality",
                Slug = "parts-inventory-quality",
                Description = "Deep training on the parts catalog, BOM management, inventory movements, bin transfers, and quality inspections.",
                Icon = "inventory_2",
                IsAutoAssigned = false,
                IsActive = true,
                SortOrder = 6,
                AllowedRoles = """["Admin","Manager","Engineer"]""",
            };
            db.TrainingPaths.Add(piqPath);
            await db.SaveChangesAsync();

            var piqModules = new List<(int ModuleId, int Position)>
            {
                (invId, 1), (binId, 2), (qualId, 3), (invQRId, 4),
            };
            if (partsCatalogId > 0) piqModules.Add((partsCatalogId, 5));
            if (partsQRId > 0) piqModules.Add((partsQRId, 6));
            if (poId2 > 0) piqModules.Add((poId2, 7));
            piqModules.Add((piqId, 8));

            foreach (var (moduleId, position) in piqModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = piqPath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }
            await db.SaveChangesAsync();
            Log.Information("Seeded Parts, Inventory and Quality training path");
        }

        // ── Path 7: Admin Setup & Configuration ───────────────────────────
        if (!await db.TrainingPaths.Where(p => p.Title == "Admin Setup and Configuration").AnyAsync())
        {
            var systemSettingsModule = new TrainingModule
            {
                Title = "System Settings and Branding",
                Slug = "system-settings-branding",
                Summary = "How to configure application settings, company profile, brand colors, logo, and company locations.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 1,
                AppRoutes = """["/admin/settings"]""",
                Tags = """["admin","settings","branding"]""",
                ContentJson = """{"body":"## System Settings and Branding\n\n### Company Profile\n\nNavigate to **Admin → Settings → Company Profile**. Enter your company's legal name, phone number, email, EIN, and website. This information appears on invoices, packing slips, and other customer-facing documents.\n\n### Company Locations\n\nUnder **Admin → Settings → Locations**, add all your physical locations (main office, warehouse, remote sites). Each location has a name, address, and state. Mark one as the Default — this is the default work location for employees who haven't specified one, and it drives state withholding form requirements.\n\n### Brand Colors and Logo\n\nIn **Admin → Settings**, scroll to the Brand section. Enter hex color codes for your primary and accent brand colors. These update the entire app's color theme in real time — all users see the change immediately.\n\nTo upload your company logo, click the Logo section and drag your image file (PNG or SVG recommended). The logo appears in the header bar and on printed documents.\n\n### Application Settings\n\nThe Settings panel also has configuration for:\n- **Application name** — shown in the browser tab and header\n- **Planning cycle duration** — default length for new planning cycles\n- **Auto-archive days** — how long after completion before jobs auto-archive\n- **Max upload size** — maximum file attachment size\n- **Email notifications** — enable/disable system emails","sections":[]}"""
            };

            var complianceAdminModule = new TrainingModule
            {
                Title = "Compliance Templates Administration",
                Slug = "compliance-templates-admin",
                Summary = "How to manage compliance form templates: uploading PDFs, configuring fields, and monitoring employee completion.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 6,
                IsPublished = true,
                SortOrder = 2,
                AppRoutes = """["/admin/compliance"]""",
                Tags = """["admin","compliance","hr"]""",
                ContentJson = """{"body":"## Compliance Templates Administration\n\n### What Are Compliance Templates?\n\nCompliance templates are the master definitions of each form employees must complete (W-4, I-9, state withholding, employee handbook, etc.). Admins manage the templates; employees fill them out from their Account page.\n\n### Creating a Template\n\nNavigate to **Admin → Compliance** and click **New Template**. Give it a title (e.g., 'Federal W-4 2024'), a form type (Tax Withholding, Identity Verification, Acknowledgment), and an effective date. Then configure which form fields employees will fill out.\n\n### Uploading a PDF Form\n\nFor government forms like the W-4, upload the official PDF. The system uses AI-powered PDF extraction to automatically identify form fields and generate a renderable form definition. Review the extracted fields and adjust any that weren't parsed correctly.\n\n### Field Configuration\n\nEach form field has a type (text, number, dropdown, signature, checkbox), validation rules, and optional help text. Some fields (name, SSN, address) are auto-populated from the employee's profile.\n\n### Monitoring Completion\n\nNavigate to **Admin → Compliance → User Status** to see completion status per employee. The table shows which forms each employee has completed, which are pending, and which are overdue. Click any employee row to see their individual form submissions.\n\n### Approval Workflow\n\nWhen an employee submits a form, it appears in the approval queue. Review the submission, and either Approve (locks the form) or Request Changes (sends it back with your notes). Approved forms generate a PDF record stored in the employee's documents.","sections":[]}"""
            };

            var integrationModule = new TrainingModule
            {
                Title = "Integration Configuration",
                Slug = "integration-configuration",
                Summary = "How to connect QuickBooks Online, configure SMTP email, set up MinIO storage, and manage integration settings.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 7,
                IsPublished = true,
                SortOrder = 3,
                AppRoutes = """["/admin/integrations"]""",
                Tags = """["admin","integrations","quickbooks"]""",
                ContentJson = """{"body":"## Integration Configuration\n\nNavigate to **Admin → Integrations** to configure external service connections.\n\n### QuickBooks Online\n\nQB Engineer syncs with QuickBooks Online for accounting. To connect:\n1. Click **Connect QuickBooks** — you'll be redirected to the Intuit OAuth consent screen.\n2. Sign in to your QBO account and authorize QB Engineer.\n3. You're returned to the app with the connection active.\n\nOnce connected, the following sync automatically:\n- Customers (bidirectional)\n- Items/Products (from QB)\n- Invoices (QB Engineer → QB)\n- Payments (QB Engineer → QB)\n- Vendor Bills from POs (QB Engineer → QB)\n- Time Activities (QB Engineer → QB for payroll)\n\nNote: When QuickBooks is connected, some features (invoicing, payments, AR aging) are read-only in QB Engineer — manage them in QBO directly.\n\n### SMTP Email\n\nFor system emails (setup tokens, notifications, invoice delivery), configure your SMTP server under **Integrations → Email**. Enter the SMTP host, port, username, and password. Use the **Test Connection** button to send a test email before saving.\n\n### Storage (MinIO)\n\nFile attachments are stored in MinIO (S3-compatible). In a Docker Compose deployment, this is already configured. If you're using an external storage provider (AWS S3, Cloudflare R2), enter the endpoint, access key, and secret key here.\n\n### Mock Mode\n\nDuring development or testing, enable **Mock Integrations** to bypass all external APIs. All services return simulated responses. This is controlled by the `MOCK_INTEGRATIONS` environment variable in docker-compose.yml.","sections":[]}"""
            };

            var refDataModule = new TrainingModule
            {
                Title = "Reference Data and Terminology",
                Slug = "reference-data-terminology",
                Summary = "How to manage lookup values (expense categories, lead sources, priorities) and customize application labels.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 4,
                IsPublished = true,
                SortOrder = 4,
                AppRoutes = """["/admin/reference-data","/admin/terminology"]""",
                Tags = """["admin","reference-data","terminology"]""",
                ContentJson = """{"body":"## Reference Data and Terminology\n\n### Reference Data\n\nNavigate to **Admin → Reference Data** to manage all lookup values used throughout the application. Reference data groups include:\n\n- **Expense Categories** — the dropdown in the expense form\n- **Lead Sources** — where leads came from (Trade Show, Referral, Website, etc.)\n- **Asset Types** — categories for the asset register\n- **Job Priorities** — the priority options on job cards\n\nEach group is expandable. Click a group to see its current values. You can add new values, edit labels, and reorder entries. The `code` field is immutable once set — it's used internally. Only the `label` can be changed.\n\n### Adding New Values\n\nClick **Add Item** within a group. Enter the label (what users see) and the code (internal identifier, lowercase-snake-case). Click Save. The new option immediately appears in the relevant dropdowns.\n\n### Terminology\n\nNavigate to **Admin → Terminology** to customize application labels. If your company calls jobs 'Work Orders' instead of 'Jobs', enter 'Work Orders' next to the `entity_job` key. Click **Save Terminology**.\n\nTerminology changes apply across the entire app — all users see the updated labels immediately. The change is cached in each browser for fast rendering.\n\n### Resetting Labels\n\nDelete the custom text for any terminology key to revert to the system default. The default is derived from the key name (e.g., `entity_job` → 'Job').","sections":[]}"""
            };

            var auditLogModule = new TrainingModule
            {
                Title = "Audit Log and System Monitoring",
                Slug = "audit-log-monitoring",
                Summary = "Quick reference for using the audit log to investigate changes, track user actions, and support compliance.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.QuickRef,
                EstimatedMinutes = 3,
                IsPublished = true,
                SortOrder = 5,
                AppRoutes = """["/admin/audit-log"]""",
                Tags = """["admin","audit","monitoring"]""",
                ContentJson = """{"title":"Audit Log and System Monitoring","groups":[{"heading":"What the Audit Log Records","items":[{"label":"Entity changes","value":"Every create, update, and delete across all entities with before/after field values"},{"label":"Auth events","value":"Logins, failed attempts, token generation, password changes"},{"label":"Admin actions","value":"Role changes, user creation/deactivation, setting changes"},{"label":"Integration events","value":"QB sync operations, file uploads, email sends"}]},{"heading":"Filtering the Log","items":[{"label":"By entity type","value":"Filter to just Jobs, Users, Invoices, etc."},{"label":"By user","value":"See everything a specific user changed"},{"label":"By date range","value":"Narrow to an incident's timeframe"},{"label":"By action","value":"Create / Update / Delete filter"}]},{"heading":"Common Use Cases","items":[{"label":"Who changed a job?","value":"Filter by entity=Job, entity ID, action=Update"},{"label":"Who logged in when?","value":"Filter by action=Login, filter by user"},{"label":"What changed in a setting?","value":"Filter by entity=SystemSetting"},{"label":"Who approved a compliance form?","value":"Filter by entity=ComplianceFormSubmission, action=Update"}]},{"heading":"Retention","items":[{"label":"Default retention","value":"90 days (configurable in system settings)"},{"label":"Compliance export","value":"Export to CSV for external audit systems"},{"label":"Immutability","value":"Audit records cannot be edited or deleted by any user"}]}]}"""
            };

            var trainingAdminModule = new TrainingModule
            {
                Title = "Training Module Administration",
                Slug = "training-module-admin",
                Summary = "How to create training modules, manage learning paths, generate AI walkthroughs, and monitor employee progress.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 6,
                AppRoutes = """["/admin/training"]""",
                Tags = """["admin","training","lms"]""",
                ContentJson = """{"body":"## Training Module Administration\n\nNavigate to **Admin → Training** to manage the entire learning management system.\n\n### Content Tab\n\nThe Content tab lists all training modules. Each module has a type (Article, Video, Walkthrough, QuickRef, Quiz), published status, and estimated time. Use the search box to find modules.\n\n### Creating a Module\n\nClick **New Module** to open the editor. Fill in the title, summary, content type, and estimated time. Set the content:\n- **Article/QuickRef** — write the content in the JSON content editor or use the rich text interface\n- **Walkthrough** — define the driver.js tour steps, or use **AI Generate** to automatically create steps from the live page\n- **Quiz** — add questions with 4 options each, mark the correct answer, and optionally add explanations\n\n### Publishing\n\nModules are Draft until you toggle **Published**. Only published modules are visible to employees.\n\n### AI Walkthrough Generation\n\nFor Walkthrough modules, click the sparkle (✨) icon to trigger AI generation. The system opens the target page in a headless browser, analyzes the DOM, and sends it to the local AI model to generate relevant tour steps. Review the suggested steps and edit as needed before saving.\n\n### Paths Tab\n\nLearning Paths group modules into ordered sequences. Create a path, add modules to it in order, and mark required vs. optional. Set allowed roles and whether the path is auto-assigned to new users.\n\n### Progress Tab\n\nThe Progress tab shows completion percentages for all users. Click the detail icon (↗) on any user row to open their per-module breakdown in a side panel.","sections":[]}"""
            };

            int sysSettId = await GetOrCreateModule(systemSettingsModule);
            int compAdminId = await GetOrCreateModule(complianceAdminModule);
            int integId = await GetOrCreateModule(integrationModule);
            int refDataId = await GetOrCreateModule(refDataModule);
            int auditId = await GetOrCreateModule(auditLogModule);
            int trainingAdminId = await GetOrCreateModule(trainingAdminModule);
            bySlug.TryGetValue("admin-users-and-roles", out var adminUsersId);

            var adminPath = new TrainingPath
            {
                Title = "Admin Setup and Configuration",
                Slug = "admin-setup-configuration",
                Description = "Complete admin onboarding: users, roles, settings, branding, integrations, compliance templates, and training administration.",
                Icon = "admin_panel_settings",
                IsAutoAssigned = false,
                IsActive = true,
                SortOrder = 7,
                AllowedRoles = """["Admin"]""",
            };
            db.TrainingPaths.Add(adminPath);
            await db.SaveChangesAsync();

            var adminModules = new List<(int ModuleId, int Position)>
            {
                (sysSettId, 1), (compAdminId, 2), (integId, 3), (refDataId, 4), (auditId, 5), (trainingAdminId, 6),
            };
            if (adminUsersId > 0) adminModules.Add((adminUsersId, 7));

            foreach (var (moduleId, position) in adminModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = adminPath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }
            await db.SaveChangesAsync();
            Log.Information("Seeded Admin Setup and Configuration training path");
        }

        // ── Path 8: Sales and Customer Management ─────────────────────────
        if (!await db.TrainingPaths.Where(p => p.Title == "Sales and Customer Management").AnyAsync())
        {
            var leadsModule = new TrainingModule
            {
                Title = "Leads and Sales Pipeline",
                Slug = "leads-pipeline",
                Summary = "How to capture leads, track them through the sales pipeline, and convert them to customers and quotes.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 1,
                AppRoutes = """["/leads"]""",
                Tags = """["sales","leads","crm"]""",
                ContentJson = """{"body":"## Leads and Sales Pipeline\n\nLeads represent potential customers or new business opportunities that haven't yet converted to a formal order. Track them through QB Engineer's Leads module from first contact to closed deal.\n\n### Lead List\n\nNavigate to **Leads**. The table shows all open leads with their source, status, assigned salesperson, and value estimate. Leads are sorted by status (New → Contacted → Qualified → Proposal → Closed).\n\n### Adding a Lead\n\nClick **New Lead**. Enter:\n- **Company name and contact** — who this opportunity is with\n- **Source** — how the lead was generated (Trade Show, Referral, Cold Call, Website, etc.)\n- **Estimated value** — your rough estimate of deal size\n- **Assignee** — who owns this lead\n- **Notes** — any context from the initial conversation\n\n### Lead Statuses\n\n- **New** — just entered, no contact made\n- **Contacted** — first contact made, awaiting response\n- **Qualified** — confirmed there's a real need and budget\n- **Proposal Sent** — a quote has been sent\n- **Won** — converted to customer/order\n- **Lost** — didn't win the business\n\n### Converting to a Customer\n\nWhen a lead converts to real business, click **Convert to Customer**. This creates a Customer record pre-populated with the lead's contact information. You can then create a Quote directly from the converted lead.\n\n### Activity Log\n\nLog every touchpoint in the lead's activity feed: calls, emails, meetings, demos. Keep the record current so you (and your team) always know the last interaction and next step.","sections":[]}"""
            };

            var shipmentsModule = new TrainingModule
            {
                Title = "Shipments and Carrier Tracking",
                Slug = "shipments-tracking",
                Summary = "How to create shipments, enter tracking numbers, ship from multiple carriers, and view delivery status.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.Article,
                EstimatedMinutes = 5,
                IsPublished = true,
                SortOrder = 2,
                AppRoutes = """["/shipments"]""",
                Tags = """["shipments","shipping","tracking"]""",
                ContentJson = """{"body":"## Shipments and Carrier Tracking\n\nShipments track physical delivery of goods to customers. A shipment can fulfill one or more Sales Order lines, and is required before an invoice can be created for shipped goods.\n\n### Creating a Shipment\n\nNavigate to **Shipments** and click **New Shipment**. Select:\n- The customer and shipping address (from their saved addresses)\n- The Sales Order lines being shipped (partial fulfillment is supported)\n- The carrier (UPS, FedEx, USPS, DHL, or manual entry)\n- Package dimensions and weight (for carrier rate shopping if integration is configured)\n\n### Getting Shipping Rates\n\nIf a carrier API is configured, click **Get Rates** to fetch live rates from multiple carriers. Select a rate and the label is generated automatically. The tracking number is attached to the shipment record.\n\n### Manual Tracking Numbers\n\nIf you're using a carrier not integrated with the system, select Manual and enter the tracking number. You can still set the carrier name and expected delivery date.\n\n### Shipment Status\n\nShipment statuses follow the delivery lifecycle:\n- **Draft** — being prepared\n- **Packed** — packed and ready to ship\n- **Shipped** — handed off to carrier\n- **In Transit** — out for delivery\n- **Delivered** — confirmed received\n\n### Tracking Updates\n\nThe tracking timeline on each shipment shows the carrier's status events as they update. QB Engineer polls the carrier API to refresh tracking status.","sections":[]}"""
            };

            var salesQuickRef = new TrainingModule
            {
                Title = "Sales Quick Reference",
                Slug = "sales-quick-reference",
                Summary = "Quick reference for the quote-to-cash workflow, lead statuses, shipment statuses, and common sales actions.",
                ContentType = QBEngineer.Core.Enums.TrainingContentType.QuickRef,
                EstimatedMinutes = 2,
                IsPublished = true,
                SortOrder = 3,
                AppRoutes = """["/leads","/quotes","/sales-orders","/shipments"]""",
                Tags = """["sales","reference","shipping"]""",
                ContentJson = """{"title":"Sales Quick Reference","groups":[{"heading":"Lead Pipeline Stages","items":[{"label":"New","value":"First entry — no contact made yet"},{"label":"Contacted","value":"Initial outreach made — awaiting response"},{"label":"Qualified","value":"Confirmed need and budget — moving to proposal"},{"label":"Proposal Sent","value":"Quote delivered to prospect"},{"label":"Won","value":"Deal closed — convert to Customer and Quote"},{"label":"Lost","value":"Didn't win — log reason for pipeline analysis"}]},{"heading":"Quote-to-Cash Flow","items":[{"label":"Step 1","value":"Leads → Create Quote"},{"label":"Step 2","value":"Quote Accepted → Convert to Sales Order"},{"label":"Step 3","value":"Sales Order → Production (kanban job created)"},{"label":"Step 4","value":"Job Shipped → Create Shipment → Create Invoice"},{"label":"Step 5","value":"Invoice Sent → Payment Received"}]},{"heading":"Shipment Statuses","items":[{"label":"Draft","value":"Being prepared — items not yet packed"},{"label":"Packed","value":"Ready to hand off to carrier"},{"label":"Shipped","value":"With carrier — tracking number assigned"},{"label":"In Transit","value":"Out for delivery"},{"label":"Delivered","value":"Confirmed received by customer"}]},{"heading":"Common Actions","items":[{"label":"New lead","value":"Leads → New Lead"},{"label":"Convert lead","value":"Lead detail → Convert to Customer"},{"label":"New quote","value":"Quotes → New Quote (or from lead detail)"},{"label":"Ship an order","value":"Shipments → New Shipment → select SO lines"},{"label":"Invoice after ship","value":"Invoices → New Invoice → Create from SO"}]}]}"""
            };

            int leadsId = await GetOrCreateModule(leadsModule);
            int shipmentsId = await GetOrCreateModule(shipmentsModule);
            int salesQRId = await GetOrCreateModule(salesQuickRef);
            bySlug.TryGetValue("customers-and-contacts", out var custId2);
            bySlug.TryGetValue("quotes-and-estimates", out var quotesId2);
            bySlug.TryGetValue("sales-orders-overview", out var soId);

            var salesPath = new TrainingPath
            {
                Title = "Sales and Customer Management",
                Slug = "sales-customer-management",
                Description = "Training for sales staff: leads pipeline, quoting, sales orders, shipments, and customer relationship management.",
                Icon = "storefront",
                IsAutoAssigned = false,
                IsActive = true,
                SortOrder = 8,
                AllowedRoles = """["Admin","Manager","OfficeManager"]""",
            };
            db.TrainingPaths.Add(salesPath);
            await db.SaveChangesAsync();

            var salesModules = new List<(int ModuleId, int Position)>
            {
                (leadsId, 1), (shipmentsId, 2), (salesQRId, 3),
            };
            if (custId2 > 0) salesModules.Add((custId2, 4));
            if (quotesId2 > 0) salesModules.Add((quotesId2, 5));

            foreach (var (moduleId, position) in salesModules)
            {
                db.TrainingPathModules.Add(new TrainingPathModule
                {
                    PathId = salesPath.Id,
                    ModuleId = moduleId,
                    Position = position,
                    IsRequired = true,
                });
            }
            await db.SaveChangesAsync();
            Log.Information("Seeded Sales and Customer Management training path");
        }
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

