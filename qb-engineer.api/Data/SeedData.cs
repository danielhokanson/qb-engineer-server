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
