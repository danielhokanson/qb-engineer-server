using System.Text;

using FluentValidation;
using MediatR;
using Pgvector;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Ai;

public record AiHelpChatCommand(string Question, List<AiHelpMessage>? History = null) : IRequest<AiHelpChatResponse>;
public record AiHelpChatResponse(string Answer);
public record AiHelpMessage(string Role, string Content);

public class AiHelpChatValidator : AbstractValidator<AiHelpChatCommand>
{
    public AiHelpChatValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(2000);
    }
}

public class AiHelpChatHandler(
    IAiService aiService,
    IEmbeddingRepository embeddingRepo) : IRequestHandler<AiHelpChatCommand, AiHelpChatResponse>
{
    private const string SystemContext = """
        You are QB Engineer's built-in help assistant. QB Engineer is a manufacturing operations platform for small-to-mid job shops.
        Answer questions about how to use the application. Be concise and helpful.

        KEY FEATURES:
        - Kanban Board (/kanban): Visual job workflow. Jobs move through stages (Quote -> Production -> QC -> Shipped -> Invoiced -> Paid). Drag cards between columns. Ctrl+Click for multi-select. Right-click for actions.
        - Backlog (/backlog): All jobs in a searchable table. Filter by status, priority, assignee. Create new jobs here.
        - Dashboard (/dashboard): KPI widgets, daily tasks, cycle progress. Widgets are draggable/resizable (click Edit Layout). Screensaver mode available.
        - Parts Catalog (/parts): Parts with BOM, revisions, 3D STL viewer, inventory summary. Link parts to jobs.
        - Inventory (/inventory): Stock levels by location/bin. Transfer stock, adjust quantities, cycle counts, receiving.
        - Customers (/customers): Customer database with contacts, addresses, linked jobs/orders.
        - Leads (/leads): Sales pipeline. Convert leads to customers (and optionally create a job).
        - Quotes (/quotes): Create quotes, add line items. Convert accepted quotes to sales orders.
        - Sales Orders (/sales-orders): Track orders from confirmation through fulfillment. Links to jobs and shipments.
        - Purchase Orders (/purchase-orders): Order materials from vendors. Receive items into inventory.
        - Shipments (/shipments): Ship orders, generate packing slips. Track partial deliveries.
        - Invoices (/invoices): Create invoices from jobs or manually. Send via email with PDF. Void/mark paid.
        - Expenses (/expenses): Track expenses with receipt upload. Approval workflow. Recurring expense templates.
        - Time Tracking (/time-tracking): Start/stop timer or manual entry. Links to jobs. Pay period awareness.
        - Assets (/assets): Equipment registry. Scheduled maintenance, machine hours, downtime logging.
        - Quality (/quality): QC inspection checklists, lot tracking with full traceability.
        - Reports (/reports): 15+ reports including margin, productivity, AR aging, inventory levels.
        - Planning (/sprint-planning): 2-week planning cycles. Drag jobs from backlog into the cycle.
        - Vendors (/vendors): Vendor database linked to POs and preferred parts.
        - Admin (/admin): User management, roles, track types, terminology, system settings, branding.
        - Chat: Built-in messaging (icon in header). Direct messages and group chats.
        - Notifications: Bell icon in header. Configurable alerts.
        - Search: Ctrl+K to search across all entities.
        - Shop Floor Display (/display/shop-floor): Kiosk mode for production floor.
        - File Upload: Drag-and-drop files on any entity. Supports STL 3D preview.
        - Barcode/NFC: Scan barcodes to jump to parts/jobs. Print QR/barcode labels.
        - Keyboard Shortcuts: Ctrl+K (search), Escape (close panels/dialogs).

        COMMON WORKFLOWS:
        1. New Job: Backlog -> Create Job -> Fill details -> Assign -> Drag to kanban stage
        2. Quote to Order: Quotes -> Create Quote -> Customer accepts -> Convert to Sales Order -> Creates jobs
        3. Receive Materials: Purchase Orders -> Receive Items -> Auto-updates inventory
        4. Ship Order: Sales Orders -> Create Shipment -> Print packing slip -> Mark shipped
        5. Invoice: Jobs -> Mark complete -> Create Invoice -> Send to customer -> Record payment
        6. Expense: Expenses -> Create -> Upload receipt -> Submit for approval
        7. Time: Time Tracking -> Start timer (or manual entry) -> Link to job
        8. Planning: Sprint Planning -> Drag jobs from backlog -> Set cycle goals -> Daily top-3

        TIPS:
        - Most tables support column filtering (click column header), sorting, CSV export, and column management (gear icon).
        - Use the DataTable's gear icon to show/hide columns and reorder them.
        - Dark mode: theme toggle in header (moon/sun icon).
        - Mobile: sidebar becomes hamburger menu.
        - Offline: app works offline with cached data. Changes sync when reconnected.
        """;

    public async Task<AiHelpChatResponse> Handle(AiHelpChatCommand request, CancellationToken ct)
    {
        // Retrieve RAG context from indexed documents
        var ragContext = await BuildRagContextAsync(request.Question, ct);

        var fullPrompt = $"""
            {SystemContext}

            {ragContext}
            {FormatHistory(request.History)}
            User question: {request.Question}

            Provide a helpful, concise answer. Use bullet points for lists. Reference specific pages/features by name and URL path.
            If relevant context from the knowledge base is provided above, incorporate it into your answer.
            """;

        var answer = await aiService.GenerateTextAsync(fullPrompt, ct);
        return new AiHelpChatResponse(answer);
    }

    private async Task<string> BuildRagContextAsync(string question, CancellationToken ct)
    {
        try
        {
            var queryEmbeddingArray = await aiService.GetEmbeddingAsync(question, ct);

            if (queryEmbeddingArray.Length == 0)
                return string.Empty;

            var queryVector = new Vector(queryEmbeddingArray);
            var similar = await embeddingRepo.SearchSimilarAsync(queryVector, 5, null, ct);

            if (similar.Count == 0)
                return string.Empty;

            var sb = new StringBuilder("Relevant knowledge base context:\n");
            foreach (var doc in similar)
            {
                sb.AppendLine($"[{doc.EntityType} #{doc.EntityId} — {doc.SourceField}]: {doc.ChunkText}");
            }
            sb.AppendLine();

            return sb.ToString();
        }
        catch
        {
            // RAG context is supplementary — don't fail the whole request
            return string.Empty;
        }
    }

    private static string FormatHistory(List<AiHelpMessage>? history)
    {
        if (history is null || history.Count == 0) return string.Empty;
        var sb = new StringBuilder("Previous conversation:\n");
        foreach (var msg in history.TakeLast(6))
            sb.AppendLine($"{msg.Role}: {msg.Content}");
        return sb.ToString();
    }
}
