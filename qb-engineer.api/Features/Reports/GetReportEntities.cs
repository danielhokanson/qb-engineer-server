using MediatR;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetReportEntitiesQuery : IRequest<List<ReportEntityDefinitionModel>>;

public class GetReportEntitiesHandler : IRequestHandler<GetReportEntitiesQuery, List<ReportEntityDefinitionModel>>
{
    public Task<List<ReportEntityDefinitionModel>> Handle(GetReportEntitiesQuery request, CancellationToken cancellationToken)
    {
        var entities = new List<ReportEntityDefinitionModel>
        {
            new("Jobs", "Jobs", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("JobNumber", "Job Number", "string", true, true, false),
                new("Title", "Title", "string", true, true, false),
                new("Description", "Description", "string", true, false, false),
                new("Priority", "Priority", "enum", true, true, true),
                new("IsArchived", "Archived", "boolean", true, true, true),
                new("DueDate", "Due Date", "date", true, true, true),
                new("StartDate", "Start Date", "date", true, true, false),
                new("CompletedDate", "Completed Date", "date", true, true, false),
                new("BoardPosition", "Board Position", "number", true, true, false),
                new("IterationCount", "Iteration Count", "number", true, true, false),
                new("IsInternal", "Internal", "boolean", true, true, true),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("UpdatedAt", "Updated At", "date", true, true, false),
                new("Customer.Name", "Customer Name", "string", true, true, true),
                new("TrackType.Name", "Track Type", "string", true, true, true),
                new("CurrentStage.Name", "Current Stage", "string", true, true, true),
            }),
            new("Parts", "Parts", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("PartNumber", "Part Number", "string", true, true, false),
                new("Description", "Description", "string", true, true, false),
                new("Revision", "Revision", "string", true, true, true),
                new("Status", "Status", "enum", true, true, true),
                new("PartType", "Part Type", "enum", true, true, true),
                new("Material", "Material", "string", true, true, true),
                new("MoldToolRef", "Mold/Tool Ref", "string", true, false, false),
                new("ExternalPartNumber", "External Part #", "string", true, false, false),
                new("MinStockThreshold", "Min Stock", "number", true, true, false),
                new("ReorderPoint", "Reorder Point", "number", true, true, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("UpdatedAt", "Updated At", "date", true, true, false),
            }),
            new("Customers", "Customers", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("Name", "Name", "string", true, true, false),
                new("CompanyName", "Company Name", "string", true, true, true),
                new("Email", "Email", "string", true, true, false),
                new("Phone", "Phone", "string", true, false, false),
                new("IsActive", "Active", "boolean", true, true, true),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("UpdatedAt", "Updated At", "date", true, true, false),
            }),
            new("Expenses", "Expenses", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("Amount", "Amount", "number", true, true, false),
                new("Category", "Category", "string", true, true, true),
                new("Description", "Description", "string", true, false, false),
                new("Status", "Status", "enum", true, true, true),
                new("ExpenseDate", "Expense Date", "date", true, true, true),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("UpdatedAt", "Updated At", "date", true, true, false),
                new("Job.JobNumber", "Job Number", "string", true, true, false),
                new("Job.Title", "Job Title", "string", true, false, false),
            }),
            new("TimeEntries", "Time Entries", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("Date", "Date", "date", true, true, true),
                new("DurationMinutes", "Duration (min)", "number", true, true, false),
                new("Category", "Category", "string", true, true, true),
                new("Notes", "Notes", "string", true, false, false),
                new("IsManual", "Manual Entry", "boolean", true, true, true),
                new("IsLocked", "Locked", "boolean", true, true, true),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("Job.JobNumber", "Job Number", "string", true, true, false),
                new("Job.Title", "Job Title", "string", true, false, false),
            }),
            new("Invoices", "Invoices", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("InvoiceNumber", "Invoice Number", "string", true, true, false),
                new("Status", "Status", "enum", true, true, true),
                new("InvoiceDate", "Invoice Date", "date", true, true, true),
                new("DueDate", "Due Date", "date", true, true, true),
                new("TaxRate", "Tax Rate", "number", true, true, false),
                new("Notes", "Notes", "string", true, false, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("Customer.Name", "Customer Name", "string", true, true, true),
            }),
            new("Leads", "Leads", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("CompanyName", "Company Name", "string", true, true, false),
                new("ContactName", "Contact Name", "string", true, true, false),
                new("Email", "Email", "string", true, false, false),
                new("Phone", "Phone", "string", true, false, false),
                new("Source", "Source", "string", true, true, true),
                new("Status", "Status", "enum", true, true, true),
                new("FollowUpDate", "Follow-Up Date", "date", true, true, false),
                new("LostReason", "Lost Reason", "string", true, false, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("UpdatedAt", "Updated At", "date", true, true, false),
            }),
            new("Assets", "Assets", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("Name", "Name", "string", true, true, false),
                new("AssetType", "Type", "enum", true, true, true),
                new("Location", "Location", "string", true, true, true),
                new("Manufacturer", "Manufacturer", "string", true, true, true),
                new("Model", "Model", "string", true, true, false),
                new("SerialNumber", "Serial Number", "string", true, false, false),
                new("Status", "Status", "enum", true, true, true),
                new("CurrentHours", "Current Hours", "number", true, true, false),
                new("IsCustomerOwned", "Customer Owned", "boolean", true, true, true),
                new("CavityCount", "Cavity Count", "number", true, true, false),
                new("CurrentShotCount", "Shot Count", "number", true, true, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("UpdatedAt", "Updated At", "date", true, true, false),
            }),
            new("PurchaseOrders", "Purchase Orders", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("PONumber", "PO Number", "string", true, true, false),
                new("Status", "Status", "enum", true, true, true),
                new("SubmittedDate", "Submitted Date", "date", true, true, true),
                new("AcknowledgedDate", "Acknowledged Date", "date", true, true, false),
                new("ExpectedDeliveryDate", "Expected Delivery", "date", true, true, true),
                new("ReceivedDate", "Received Date", "date", true, true, false),
                new("Notes", "Notes", "string", true, false, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("Vendor.Name", "Vendor Name", "string", true, true, true),
                new("Job.JobNumber", "Job Number", "string", true, true, false),
            }),
            new("SalesOrders", "Sales Orders", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("OrderNumber", "Order Number", "string", true, true, false),
                new("Status", "Status", "enum", true, true, true),
                new("ConfirmedDate", "Confirmed Date", "date", true, true, true),
                new("RequestedDeliveryDate", "Requested Delivery", "date", true, true, true),
                new("CustomerPO", "Customer PO", "string", true, true, false),
                new("TaxRate", "Tax Rate", "number", true, true, false),
                new("Notes", "Notes", "string", true, false, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("Customer.Name", "Customer Name", "string", true, true, true),
            }),
            new("Quotes", "Quotes", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("QuoteNumber", "Quote Number", "string", true, true, false),
                new("Status", "Status", "enum", true, true, true),
                new("SentDate", "Sent Date", "date", true, true, true),
                new("ExpirationDate", "Expiration Date", "date", true, true, true),
                new("AcceptedDate", "Accepted Date", "date", true, true, false),
                new("TaxRate", "Tax Rate", "number", true, true, false),
                new("Notes", "Notes", "string", true, false, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("Customer.Name", "Customer Name", "string", true, true, true),
            }),
            new("Shipments", "Shipments", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("ShipmentNumber", "Shipment Number", "string", true, true, false),
                new("Status", "Status", "enum", true, true, true),
                new("Carrier", "Carrier", "string", true, true, true),
                new("TrackingNumber", "Tracking Number", "string", true, false, false),
                new("ShippedDate", "Shipped Date", "date", true, true, true),
                new("DeliveredDate", "Delivered Date", "date", true, true, false),
                new("ShippingCost", "Shipping Cost", "number", true, true, false),
                new("Weight", "Weight", "number", true, true, false),
                new("Notes", "Notes", "string", true, false, false),
                new("CreatedAt", "Created At", "date", true, true, true),
                new("SalesOrder.OrderNumber", "Sales Order #", "string", true, true, false),
            }),
            new("Inventory", "Inventory (Bin Contents)", new List<ReportFieldDefinitionModel>
            {
                new("Id", "ID", "number", true, true, false),
                new("EntityType", "Entity Type", "string", true, true, true),
                new("EntityId", "Entity ID", "number", true, true, false),
                new("Quantity", "Quantity", "number", true, true, false),
                new("ReservedQuantity", "Reserved Qty", "number", true, true, false),
                new("LotNumber", "Lot Number", "string", true, true, true),
                new("Status", "Status", "enum", true, true, true),
                new("PlacedAt", "Placed At", "date", true, true, true),
                new("RemovedAt", "Removed At", "date", true, true, false),
                new("Notes", "Notes", "string", true, false, false),
                new("Location.Name", "Location Name", "string", true, true, true),
            }),
        };

        return Task.FromResult(entities);
    }
}
