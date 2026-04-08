using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Data.Context;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>, IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    private readonly IClock _clock;

    public AppDbContext(DbContextOptions<AppDbContext> options, IClock clock) : base(options)
    {
        _clock = clock;
    }

    public DbSet<TrackType> TrackTypes => Set<TrackType>();
    public DbSet<JobStage> JobStages => Set<JobStage>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobSubtask> JobSubtasks => Set<JobSubtask>();
    public DbSet<JobLink> JobLinks => Set<JobLink>();
    public DbSet<JobActivityLog> JobActivityLogs => Set<JobActivityLog>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PartPrice> PartPrices => Set<PartPrice>();
    public DbSet<BOMEntry> BOMEntries => Set<BOMEntry>();
    public DbSet<ReferenceData> ReferenceData => Set<ReferenceData>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<SyncQueueEntry> SyncQueueEntries => Set<SyncQueueEntry>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<BinContent> BinContents => Set<BinContent>();
    public DbSet<BinMovement> BinMovements => Set<BinMovement>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<RecurringExpense> RecurringExpenses => Set<RecurringExpense>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<ClockEvent> ClockEvents => Set<ClockEvent>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TerminologyEntry> TerminologyEntries => Set<TerminologyEntry>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<PlanningCycle> PlanningCycles => Set<PlanningCycle>();
    public DbSet<PlanningCycleEntry> PlanningCycleEntries => Set<PlanningCycleEntry>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<ReceivingRecord> ReceivingRecords => Set<ReceivingRecord>();
    public DbSet<JobPart> JobParts => Set<JobPart>();
    public DbSet<JobNote> JobNotes => Set<JobNote>();

    // Order Management
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteLine> QuoteLines => Set<QuoteLine>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();

    // Standalone Financial (⚡ Accounting Boundary)
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentApplication> PaymentApplications => Set<PaymentApplication>();

    // Pricing
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListEntry> PriceListEntries => Set<PriceListEntry>();
    public DbSet<RecurringOrder> RecurringOrders => Set<RecurringOrder>();
    public DbSet<RecurringOrderLine> RecurringOrderLines => Set<RecurringOrderLine>();
    public DbSet<CustomerReturn> CustomerReturns => Set<CustomerReturn>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();

    // Asset Maintenance
    public DbSet<MaintenanceSchedule> MaintenanceSchedules => Set<MaintenanceSchedule>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();

    // Production
    public DbSet<ProductionRun> ProductionRuns => Set<ProductionRun>();

    // Part Revisions
    public DbSet<PartRevision> PartRevisions => Set<PartRevision>();

    // Downtime
    public DbSet<DowntimeLog> DowntimeLogs => Set<DowntimeLog>();

    // Sales Tax
    public DbSet<SalesTaxRate> SalesTaxRates => Set<SalesTaxRate>();

    // Quality & Traceability
    public DbSet<QcChecklistTemplate> QcChecklistTemplates => Set<QcChecklistTemplate>();
    public DbSet<QcChecklistItem> QcChecklistItems => Set<QcChecklistItem>();
    public DbSet<QcInspection> QcInspections => Set<QcInspection>();
    public DbSet<QcInspectionResult> QcInspectionResults => Set<QcInspectionResult>();
    public DbSet<LotRecord> LotRecords => Set<LotRecord>();

    // Operations
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OperationMaterial> OperationMaterials => Set<OperationMaterial>();

    // Cycle Counts
    public DbSet<CycleCount> CycleCounts => Set<CycleCount>();
    public DbSet<CycleCountLine> CycleCountLines => Set<CycleCountLine>();

    // Reservations
    public DbSet<Reservation> Reservations => Set<Reservation>();

    // AI / RAG
    public DbSet<DocumentEmbedding> DocumentEmbeddings => Set<DocumentEmbedding>();
    public DbSet<AiAssistant> AiAssistants => Set<AiAssistant>();

    // Status Tracking
    public DbSet<StatusEntry> StatusEntries => Set<StatusEntry>();

    // Audit
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

    // Shipment Packages
    public DbSet<ShipmentPackage> ShipmentPackages => Set<ShipmentPackage>();

    // Scheduled Tasks
    public DbSet<ScheduledTask> ScheduledTasks => Set<ScheduledTask>();

    // Scan Identifiers (NFC/RFID/Barcode)
    public DbSet<UserScanIdentifier> UserScanIdentifiers => Set<UserScanIdentifier>();

    // Report Builder
    public DbSet<SavedReport> SavedReports => Set<SavedReport>();

    // Teams & Kiosk Terminals
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<KioskTerminal> KioskTerminals => Set<KioskTerminal>();

    // Central Barcode Registry
    public DbSet<Barcode> Barcodes => Set<Barcode>();

    // Employee Profiles
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();

    // Company Locations
    public DbSet<CompanyLocation> CompanyLocations => Set<CompanyLocation>();

    // Compliance & Document Signing
    public DbSet<ComplianceFormTemplate> ComplianceFormTemplates => Set<ComplianceFormTemplate>();
    public DbSet<ComplianceFormSubmission> ComplianceFormSubmissions => Set<ComplianceFormSubmission>();
    public DbSet<IdentityDocument> IdentityDocuments => Set<IdentityDocument>();
    public DbSet<FormDefinitionVersion> FormDefinitionVersions => Set<FormDefinitionVersion>();

    // Payroll
    public DbSet<PayStub> PayStubs => Set<PayStub>();
    public DbSet<PayStubDeduction> PayStubDeductions => Set<PayStubDeduction>();
    public DbSet<TaxDocument> TaxDocuments => Set<TaxDocument>();

    // Replenishment
    public DbSet<ReorderSuggestion> ReorderSuggestions => Set<ReorderSuggestion>();

    // Training
    public DbSet<TrainingModule> TrainingModules => Set<TrainingModule>();
    public DbSet<TrainingPath> TrainingPaths => Set<TrainingPath>();
    public DbSet<TrainingPathModule> TrainingPathModules => Set<TrainingPathModule>();
    public DbSet<TrainingPathEnrollment> TrainingPathEnrollments => Set<TrainingPathEnrollment>();
    public DbSet<TrainingProgress> TrainingProgress => Set<TrainingProgress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("vector");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply snake_case naming convention for all tables and columns
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.GetColumnName()));

            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName()!));

            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()!));

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
        }

        // Global query filter for soft delete on all BaseAuditableEntity types
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var deletedAtProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseAuditableEntity.DeletedAt));
            var nullConstant = System.Linq.Expressions.Expression.Constant(null, typeof(DateTimeOffset?));
            var filter = System.Linq.Expressions.Expression.Equal(deletedAtProperty, nullConstant);
            var lambda = System.Linq.Expressions.Expression.Lambda(filter, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();
        var now = _clock.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default) entry.Entity.CreatedAt = now;
                    if (entry.Entity.UpdatedAt == default) entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])
                ? "_" + c
                : c.ToString()))
            .ToLowerInvariant();
    }
}
