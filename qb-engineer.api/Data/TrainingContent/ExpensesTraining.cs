using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

using Serilog;

namespace QBEngineer.Api.Data.TrainingContent;

public class ExpensesTraining : TrainingContentBase
{
    public ExpensesTraining(AppDbContext db, Dictionary<string, int> slugMap) : base(db, slugMap) { }

    public override async Task SeedAsync()
    {
        // ── Overview (Article) ───────────────────────────────────────────
        await GetOrCreateModule(new TrainingModule
        {
            Title = "Expenses Overview",
            Slug = "expenses-overview",
            Summary = "What the Expenses module does: submission flow, approval workflow, recurring expenses, and upcoming projections.",
            ContentType = TrainingContentType.Article,
            EstimatedMinutes = 5,
            IsPublished = true,
            SortOrder = 1,
            AppRoutes = """["/expenses"]""",
            Tags = """["expenses","approval","recurring"]""",
            ContentJson = """
{
  "body": "## Expenses Overview\n\nThe Expenses module is your central hub for submitting, approving, and tracking all company expenses. It covers one-time charges, recurring subscriptions, and upcoming cost projections — giving you a complete picture of operational spending.\n\n### Three Sub-Components\n\nThe Expenses page is divided into three sub-components, each accessible from the page tabs:\n\n1. **Main Expenses** — Submit and browse all one-time and ad-hoc expenses.\n2. **Approval Queue** — Managers review, approve, or reject submitted expenses.\n3. **Upcoming Expenses** — View projected costs from recurring expense schedules.\n\n### Submission Flow\n\nTo submit an expense:\n1. Navigate to /expenses.\n2. Click the Add Expense button.\n3. Fill in Amount (required), Date (required), Category (required from reference data), and an optional Description.\n4. Click Save. The expense enters **Pending** status.\n\nOnce submitted, the expense appears in your list and in the Approval Queue for managers.\n\n### Approval Workflow\n\nManagers and Admins see the Approval Queue tab. For each pending expense they can:\n- **Approve** — marks the expense as Approved (green chip).\n- **Reject** — marks it as Rejected (red chip) with optional notes.\n- Open a **Review Dialog** to see full details and add Approval Notes before deciding.\n\nThe Approval Queue header shows a summary: the count of pending expenses and their total dollar amount.\n\nSelf-approved expenses (submitted by managers who auto-approve) show a **SelfApproved** status with a green chip.\n\n### Recurring Expenses\n\nRecurring expenses automate regular charges like subscriptions, leases, and maintenance contracts. Each recurring expense has:\n- A **Frequency** (Weekly, Biweekly, Monthly, Quarterly, or Annually)\n- A **Classification** that categorizes the type of recurring cost\n- Optional **Start Date** and **End Date** to define the active window\n- An **Auto Approve** toggle — when enabled, generated expenses skip the approval queue\n\nRecurring expenses can be paused (toggle play/pause) or deleted when no longer needed.\n\n### Upcoming Projections\n\nThe Upcoming tab shows a 90-day lookahead of expenses generated from recurring schedules. It displays:\n- A **90-day total** of upcoming costs\n- **Monthly breakdown chips** showing per-month subtotals\n- A filterable table with due dates, classifications, categories, vendors, and amounts\n\n### Classifications\n\nRecurring and upcoming expenses use classifications to group spending:\n- **Subscription** — SaaS tools, software licenses\n- **Lease** — equipment or facility leases\n- **Insurance** — liability, property, workers' comp\n- **Utility** — electric, water, internet, phone\n- **Maintenance Contract** — service agreements for equipment\n- **License** — business licenses, permits\n- **Membership** — trade associations, professional memberships\n- **Other** — anything that doesn't fit above\n\nEach classification has a distinct color chip for visual identification in tables.",
  "sections": []
}
"""
        });

        // ── Walkthrough ──────────────────────────────────────────────────
        await GetOrCreateModule(new TrainingModule
        {
            Title = "Expenses — Guided Tour",
            Slug = "expenses-walkthrough",
            Summary = "A guided tour covering all three expenses sub-components: main list, approval queue, and upcoming/recurring.",
            ContentType = TrainingContentType.Walkthrough,
            EstimatedMinutes = 7,
            IsPublished = true,
            SortOrder = 2,
            AppRoutes = """["/expenses"]""",
            Tags = """["expenses","walkthrough"]""",
            ContentJson = """
{
  "appRoute": "/expenses",
  "startButtonLabel": "Tour Expenses",
  "steps": [
    {
      "element": ".filters-bar app-input",
      "popover": {
        "title": "Search Expenses",
        "description": "Search across all expenses by description, category, or submitter name. Results filter in real time as you type.",
        "side": "bottom"
      }
    },
    {
      "element": "[data-testid='status-filter']",
      "popover": {
        "title": "Status Filter",
        "description": "Filter expenses by status: All, Pending (awaiting approval), Approved, Rejected, or SelfApproved. Each status has a color-coded chip in the table.",
        "side": "bottom"
      }
    },
    {
      "element": "app-data-table",
      "popover": {
        "title": "Expenses Table",
        "description": "All submitted expenses appear here. Columns include Date (MM/dd/yyyy), Category (colored chip), Description, Job (if linked), Submitted By, Amount ($X.XX), and Status (color chip). Click column headers to sort. Click a row for details.",
        "side": "top"
      }
    },
    {
      "element": "[data-testid='expense-save-btn'], [data-testid='new-expense-btn'], .action-btn--primary",
      "popover": {
        "title": "Add Expense",
        "description": "Click here to submit a new expense. You'll fill in Amount (required), Date (required), Category (required from reference data), and an optional Description. The expense starts in Pending status.",
        "side": "bottom"
      }
    }
  ]
}
"""
        });

        // ── Field Reference (QuickRef) ───────────────────────────────────
        await GetOrCreateModule(new TrainingModule
        {
            Title = "Expenses Field Reference",
            Slug = "expenses-field-reference",
            Summary = "Complete reference for every field, button, status, validation rule, table column, and classification in the Expenses module.",
            ContentType = TrainingContentType.QuickRef,
            EstimatedMinutes = 5,
            IsPublished = true,
            SortOrder = 3,
            AppRoutes = """["/expenses"]""",
            Tags = """["expenses","reference"]""",
            ContentJson = """
{
  "title": "Expenses Field Reference",
  "groups": [
    {
      "heading": "Create Expense Dialog Fields",
      "items": [
        {"label": "Amount (required)", "value": "Number input, min 0.01, prefix '$'. The expense dollar amount. data-testid: expense-amount"},
        {"label": "Date (required)", "value": "Datepicker. The date the expense was incurred. Displayed as MM/dd/yyyy. data-testid: expense-date"},
        {"label": "Category (required)", "value": "Select dropdown. Options loaded from reference data (expense categories configured in Admin). data-testid: expense-category"},
        {"label": "Description (optional)", "value": "Textarea. Free-form description of the expense. data-testid: expense-description"},
        {"label": "Save button", "value": "Disabled when form is invalid or save is in progress. Hover shows validation popover listing violations. data-testid: expense-save-btn"}
      ]
    },
    {
      "heading": "Expense Statuses",
      "items": [
        {"label": "Pending (warning/yellow chip)", "value": "Expense has been submitted and is awaiting manager approval. Default status for new expenses."},
        {"label": "Approved (success/green chip)", "value": "Expense has been reviewed and approved by a manager."},
        {"label": "Rejected (error/red chip)", "value": "Expense has been reviewed and rejected. Manager may include rejection notes."},
        {"label": "SelfApproved (success/green chip)", "value": "Expense was submitted by a manager who auto-approved it. Same green chip as Approved."}
      ]
    },
    {
      "heading": "Main Expenses Table Columns",
      "items": [
        {"label": "Date", "value": "Sortable. Expense date formatted as MM/dd/yyyy."},
        {"label": "Category", "value": "Sortable. Displayed as a colored chip matching the category."},
        {"label": "Description", "value": "Sortable. Free-form text description."},
        {"label": "Job", "value": "Sortable. Linked job number or '—' if not linked to a job."},
        {"label": "Submitted By", "value": "Sortable. Name of the user who submitted the expense."},
        {"label": "Amount", "value": "Sortable. Formatted as $X.XX, right-aligned."},
        {"label": "Status", "value": "Sortable, filterable (enum). Color-coded chip: Pending (yellow), Approved (green), Rejected (red), SelfApproved (green)."},
        {"label": "Actions", "value": "Row-level action buttons for edit/delete operations."}
      ]
    },
    {
      "heading": "Approval Queue",
      "items": [
        {"label": "Search", "value": "Text input. Filters pending expenses by description, submitter, or category."},
        {"label": "Summary bar", "value": "Displays count of pending expenses and their total dollar amount (e.g., '12 pending — $4,250.00')."},
        {"label": "Date column", "value": "Sortable. Expense date."},
        {"label": "Submitted By column", "value": "Sortable. Name of submitter."},
        {"label": "Category column", "value": "Sortable. Colored chip."},
        {"label": "Description column", "value": "Sortable. Expense description text."},
        {"label": "Job column", "value": "Sortable. Linked job or '—'."},
        {"label": "Amount column", "value": "Sortable. Dollar amount."},
        {"label": "Approve action (check icon)", "value": "Approves the expense immediately. Icon button with checkmark."},
        {"label": "Reject action (close icon)", "value": "Rejects the expense immediately. Icon button with X/close."},
        {"label": "Review dialog", "value": "Read-only display of all expense fields plus an Approval Notes textarea. Footer has Approve and Reject buttons."}
      ]
    },
    {
      "heading": "Upcoming Expenses — Upcoming Tab",
      "items": [
        {"label": "Classification filter", "value": "Select dropdown. Filters by classification type."},
        {"label": "90-day total", "value": "Displays the sum of all upcoming expenses in the next 90 days."},
        {"label": "Monthly breakdown chips", "value": "Chip badges showing per-month subtotals for the upcoming period."},
        {"label": "Due Date column", "value": "Sortable. When the expense is due."},
        {"label": "Classification column", "value": "Sortable. Color-coded chip (see Classifications section)."},
        {"label": "Category column", "value": "Sortable. Expense category."},
        {"label": "Description column", "value": "Sortable. Description text."},
        {"label": "Vendor column", "value": "Sortable. Vendor name."},
        {"label": "Amount column", "value": "Sortable. Dollar amount."},
        {"label": "Frequency column", "value": "Sortable. How often this expense recurs."}
      ]
    },
    {
      "heading": "Upcoming Expenses — Recurring Tab",
      "items": [
        {"label": "Classification column", "value": "Sortable. Color-coded chip."},
        {"label": "Category column", "value": "Sortable. Expense category."},
        {"label": "Description column", "value": "Sortable. Description text."},
        {"label": "Vendor column", "value": "Sortable. Vendor name."},
        {"label": "Amount column", "value": "Sortable. Dollar amount."},
        {"label": "Frequency column", "value": "Sortable. Weekly/Biweekly/Monthly/Quarterly/Annually."},
        {"label": "Next Due column", "value": "Sortable. Next scheduled date."},
        {"label": "Active column", "value": "Sortable. Whether the recurring expense is active or paused."},
        {"label": "Pause/Play toggle (action)", "value": "Icon button to pause or resume the recurring expense schedule."},
        {"label": "Delete action", "value": "Icon button to delete the recurring expense."}
      ]
    },
    {
      "heading": "Create Recurring Expense Dialog Fields",
      "items": [
        {"label": "Amount (required)", "value": "Number input, min 0.01. The recurring dollar amount. data-testid: recurring-amount"},
        {"label": "Frequency (required)", "value": "Select dropdown: Weekly, Biweekly, Monthly, Quarterly, Annually. data-testid: recurring-frequency"},
        {"label": "Category (required)", "value": "Select dropdown. Options from reference data. data-testid: recurring-category"},
        {"label": "Classification (required)", "value": "Select dropdown: Subscription, Lease, Insurance, Utility, Maintenance Contract, License, Membership, Other. data-testid: recurring-classification"},
        {"label": "Description (required)", "value": "Text input. What the recurring expense is for. data-testid: recurring-description"},
        {"label": "Vendor (optional)", "value": "Text input. Placeholder: 'e.g., Microsoft, AWS'. data-testid: recurring-vendor"},
        {"label": "Start Date (required)", "value": "Datepicker. When the recurring schedule begins. data-testid: recurring-start"},
        {"label": "End Date (optional)", "value": "Datepicker. When the recurring schedule ends. Leave empty for indefinite. data-testid: recurring-end"},
        {"label": "Auto Approve (toggle)", "value": "Slide toggle, default OFF. When ON, generated expenses skip the approval queue. data-testid: recurring-auto-approve"},
        {"label": "Save button", "value": "Disabled when form invalid or saving. data-testid: recurring-save-btn"}
      ]
    },
    {
      "heading": "Classification Colors",
      "items": [
        {"label": "Subscription", "value": "Error/red chip. SaaS tools and software subscriptions."},
        {"label": "Lease", "value": "Warning/yellow chip. Equipment or facility leases."},
        {"label": "Insurance", "value": "Info/blue chip. Liability, property, workers' comp insurance."},
        {"label": "Utility", "value": "Muted/gray chip. Electric, water, internet, phone."},
        {"label": "Maintenance Contract", "value": "Primary/indigo chip. Service agreements for equipment."},
        {"label": "License", "value": "Warning/yellow chip. Business licenses and permits."},
        {"label": "Membership", "value": "Success/green chip. Trade associations and professional memberships."},
        {"label": "Other", "value": "Default chip (no special color). Uncategorized recurring expenses."}
      ]
    },
    {
      "heading": "Validation Rules",
      "items": [
        {"label": "Amount (expense)", "value": "Required. Must be a number >= 0.01."},
        {"label": "Date (expense)", "value": "Required. Must be a valid date."},
        {"label": "Category (expense)", "value": "Required. Must select from reference data options."},
        {"label": "Description (expense)", "value": "Optional. No length restriction."},
        {"label": "Amount (recurring)", "value": "Required. Must be a number >= 0.01."},
        {"label": "Frequency (recurring)", "value": "Required. Must select one of the five frequency options."},
        {"label": "Category (recurring)", "value": "Required. Must select from reference data options."},
        {"label": "Classification (recurring)", "value": "Required. Must select one of the eight classification types."},
        {"label": "Description (recurring)", "value": "Required. Must not be empty."},
        {"label": "Start Date (recurring)", "value": "Required. Must be a valid date."},
        {"label": "End Date (recurring)", "value": "Optional. If provided, must be after Start Date."},
        {"label": "Submit button popover", "value": "When form is invalid, hovering over Save shows a validation popover listing which fields need attention."}
      ]
    }
  ]
}
"""
        });

        // ── Knowledge Check (Quiz) ───────────────────────────────────────
        await GetOrCreateModule(new TrainingModule
        {
            Title = "Expenses Knowledge Check",
            Slug = "expenses-quiz",
            Summary = "Test your knowledge of the Expenses module: submission, approval workflow, recurring expenses, and classifications.",
            ContentType = TrainingContentType.Quiz,
            EstimatedMinutes = 6,
            IsPublished = true,
            SortOrder = 4,
            AppRoutes = """["/training"]""",
            Tags = """["expenses","quiz"]""",
            ContentJson = """
{
  "passingScore": 80,
  "questionsPerQuiz": 8,
  "shuffleOptions": true,
  "showExplanationsAfterSubmit": true,
  "questions": [
    {
      "id": "ex1",
      "text": "You need to submit a $150 expense for office supplies. What fields are required in the Create Expense dialog?",
      "options": [
        {"id": "a", "text": "Amount, Date, Category, and Description are all required"},
        {"id": "b", "text": "Amount, Date, and Category are required; Description is optional", "isCorrect": true},
        {"id": "c", "text": "Only Amount is required — all other fields are optional"},
        {"id": "d", "text": "Amount, Date, Category, Description, and Job are all required"}
      ],
      "explanation": "The Create Expense dialog requires three fields: Amount (min $0.01), Date (datepicker), and Category (from reference data). Description is optional."
    },
    {
      "id": "ex2",
      "text": "What status does a newly submitted expense start with?",
      "options": [
        {"id": "a", "text": "Draft — it needs to be finalized before submission"},
        {"id": "b", "text": "Approved — expenses are auto-approved by default"},
        {"id": "c", "text": "Pending — it awaits manager approval", "isCorrect": true},
        {"id": "d", "text": "SelfApproved — the submitter's own expenses are auto-approved"}
      ],
      "explanation": "All newly submitted expenses start in Pending status (yellow chip) and appear in the Approval Queue for manager review."
    },
    {
      "id": "ex3",
      "text": "A manager sees 12 pending expenses totaling $4,250 in the Approval Queue. How can they approve a single expense?",
      "options": [
        {"id": "a", "text": "Click the checkmark icon on the expense row to approve it immediately", "isCorrect": true},
        {"id": "b", "text": "Select the expense and click a bulk Approve All button"},
        {"id": "c", "text": "Open the expense detail and change the status dropdown to Approved"},
        {"id": "d", "text": "Right-click the row and select Approve from the context menu"}
      ],
      "explanation": "Each row in the Approval Queue has two action icons: a checkmark (approve) and an X (reject). Clicking the checkmark immediately approves that expense."
    },
    {
      "id": "ex4",
      "text": "What is the difference between the Approve icon action and the Review Dialog?",
      "options": [
        {"id": "a", "text": "They are identical — the Review Dialog is just a larger view of the same action"},
        {"id": "b", "text": "The Review Dialog shows read-only details and lets the manager add Approval Notes before approving or rejecting", "isCorrect": true},
        {"id": "c", "text": "The Review Dialog is only for rejections — approvals must use the icon"},
        {"id": "d", "text": "The icon approves without confirmation; the Review Dialog requires a second manager to co-approve"}
      ],
      "explanation": "The Review Dialog opens a read-only view of the expense with all fields visible, plus an Approval Notes textarea. The manager can then click Approve or Reject with context."
    },
    {
      "id": "ex5",
      "text": "What color chip does the 'Subscription' classification use in the Upcoming Expenses view?",
      "options": [
        {"id": "a", "text": "Success/green"},
        {"id": "b", "text": "Warning/yellow"},
        {"id": "c", "text": "Error/red", "isCorrect": true},
        {"id": "d", "text": "Info/blue"}
      ],
      "explanation": "Subscription uses an error/red chip. The full color mapping is: Subscription (red), Lease (yellow), Insurance (blue), Utility (gray), Maintenance Contract (indigo/primary), License (yellow), Membership (green), Other (default)."
    },
    {
      "id": "ex6",
      "text": "You want to set up a monthly AWS bill as a recurring expense. Which fields are required?",
      "options": [
        {"id": "a", "text": "Amount, Frequency, Category, Classification, Description, and Start Date", "isCorrect": true},
        {"id": "b", "text": "Amount, Frequency, and Vendor only"},
        {"id": "c", "text": "Amount, Category, and End Date"},
        {"id": "d", "text": "All fields including Vendor and End Date are required"}
      ],
      "explanation": "Recurring expenses require: Amount (min $0.01), Frequency (e.g., Monthly), Category (from reference data), Classification (e.g., Subscription), Description, and Start Date. Vendor and End Date are optional."
    },
    {
      "id": "ex7",
      "text": "A recurring expense has Auto Approve toggled ON. What happens when the system generates an expense from this schedule?",
      "options": [
        {"id": "a", "text": "The generated expense is created in Draft status for manual submission"},
        {"id": "b", "text": "The generated expense skips the approval queue and is automatically approved", "isCorrect": true},
        {"id": "c", "text": "The generated expense is sent to the submitter for review before approval"},
        {"id": "d", "text": "Auto Approve only works for expenses under $100"}
      ],
      "explanation": "When Auto Approve is ON, expenses generated from the recurring schedule bypass the Approval Queue entirely and are created in an approved state."
    },
    {
      "id": "ex8",
      "text": "You need to temporarily stop a recurring lease payment. How do you do this without deleting the schedule?",
      "options": [
        {"id": "a", "text": "Set the End Date to today to stop future generations"},
        {"id": "b", "text": "Click the pause icon in the Actions column to pause the recurring expense", "isCorrect": true},
        {"id": "c", "text": "Change the Frequency to 'None' to halt generation"},
        {"id": "d", "text": "Toggle Auto Approve off — this pauses generation until re-enabled"}
      ],
      "explanation": "The Recurring tab has a pause/play toggle icon in the Actions column. Clicking pause stops the schedule from generating new expenses. Click play to resume later."
    },
    {
      "id": "ex9",
      "text": "The Upcoming tab shows a '90-day total' and 'monthly breakdown chips'. What do the monthly breakdown chips display?",
      "options": [
        {"id": "a", "text": "The number of recurring expenses due each month"},
        {"id": "b", "text": "Per-month dollar subtotals of upcoming expenses", "isCorrect": true},
        {"id": "c", "text": "The categories of expenses due each month"},
        {"id": "d", "text": "Links to detailed monthly expense reports"}
      ],
      "explanation": "Monthly breakdown chips show the dollar subtotal per month within the 90-day window, giving a quick visual summary of when money will be spent."
    },
    {
      "id": "ex10",
      "text": "What does the 'SelfApproved' status mean on an expense?",
      "options": [
        {"id": "a", "text": "The expense was approved by the system automatically based on rules"},
        {"id": "b", "text": "The expense was submitted by a manager who has auto-approve privileges", "isCorrect": true},
        {"id": "c", "text": "The submitter clicked an Approve button on their own expense"},
        {"id": "d", "text": "The expense was under a threshold amount and was auto-approved"}
      ],
      "explanation": "SelfApproved indicates the expense was submitted by a manager with auto-approve capability. It displays with a green chip, same as Approved."
    },
    {
      "id": "ex11",
      "text": "What is the minimum amount you can enter when creating an expense?",
      "options": [
        {"id": "a", "text": "$0.00 — zero-dollar expenses are allowed for tracking purposes"},
        {"id": "b", "text": "$0.01 — the minimum is one cent", "isCorrect": true},
        {"id": "c", "text": "$1.00 — expenses under a dollar are not supported"},
        {"id": "d", "text": "There is no minimum — any positive number works"}
      ],
      "explanation": "The Amount field has a minimum validator of 0.01. You cannot submit an expense for $0.00 or a negative amount."
    },
    {
      "id": "ex12",
      "text": "Which classification uses an info/blue chip in the Upcoming Expenses view?",
      "options": [
        {"id": "a", "text": "Utility"},
        {"id": "b", "text": "Insurance", "isCorrect": true},
        {"id": "c", "text": "Lease"},
        {"id": "d", "text": "Maintenance Contract"}
      ],
      "explanation": "Insurance uses the info/blue chip. Utility uses muted/gray, Lease uses warning/yellow, and Maintenance Contract uses primary/indigo."
    },
    {
      "id": "ex13",
      "text": "You want to reject an expense and leave a note explaining why. What is the best approach?",
      "options": [
        {"id": "a", "text": "Click the X icon to reject, then find the expense in the main list to add a note"},
        {"id": "b", "text": "Open the Review Dialog, type your reason in the Approval Notes textarea, then click Reject", "isCorrect": true},
        {"id": "c", "text": "Reject the expense and send a separate email to the submitter"},
        {"id": "d", "text": "You cannot add notes when rejecting — only approvals support notes"}
      ],
      "explanation": "The Review Dialog shows all expense details in read-only mode and includes an Approval Notes textarea. Type your reason, then click Reject. The notes are preserved with the rejection."
    },
    {
      "id": "ex14",
      "text": "What frequency options are available when creating a recurring expense?",
      "options": [
        {"id": "a", "text": "Daily, Weekly, Monthly, Yearly"},
        {"id": "b", "text": "Weekly, Biweekly, Monthly, Quarterly, Annually", "isCorrect": true},
        {"id": "c", "text": "Monthly, Quarterly, Semi-Annually, Annually"},
        {"id": "d", "text": "Weekly, Monthly, Annually only"}
      ],
      "explanation": "The Frequency dropdown offers five options: Weekly, Biweekly, Monthly, Quarterly, and Annually. There is no Daily option."
    },
    {
      "id": "ex15",
      "text": "A recurring expense has a Start Date of 01/01/2026 and an End Date of 06/30/2026 with Monthly frequency. When will the last expense be generated?",
      "options": [
        {"id": "a", "text": "05/01/2026 — the last expense is generated one month before the end date"},
        {"id": "b", "text": "06/01/2026 or 06/30/2026 — on or before the end date", "isCorrect": true},
        {"id": "c", "text": "07/01/2026 — the end date is when the next generation would have occurred"},
        {"id": "d", "text": "12/31/2026 — end dates are rounded to the end of the year"}
      ],
      "explanation": "The End Date defines when the recurring schedule stops generating expenses. The last expense will be generated on or before the end date, so June 2026 is the final month."
    }
  ]
}
"""
        });

        Log.Information("Seeded Expenses training modules");
    }
}
