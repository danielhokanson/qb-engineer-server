using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Reports;

public record ExportReportQuery(int ReportId, ReportExportFormat Format) : IRequest<ExportReportResult>;

public record ExportReportResult(byte[] Content, string ContentType, string FileName);

public class ExportReportHandler(
    AppDbContext db,
    IMediator mediator) : IRequestHandler<ExportReportQuery, ExportReportResult>
{
    public async Task<ExportReportResult> Handle(ExportReportQuery request, CancellationToken cancellationToken)
    {
        var report = await db.SavedReports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Saved report {request.ReportId} not found.");

        // Run the report to get the data
        var runResult = await mediator.Send(new RunReportCommand(
            report.EntitySource,
            System.Text.Json.JsonSerializer.Deserialize<string[]>(report.ColumnsJson) ?? [],
            null,
            report.GroupByField,
            report.SortField,
            report.SortDirection,
            1,
            10000), cancellationToken);

        return request.Format switch
        {
            ReportExportFormat.Csv => ExportToCsv(runResult, report.Name),
            ReportExportFormat.Xlsx => ExportToXlsx(runResult, report.Name),
            ReportExportFormat.Pdf => ExportToPdf(runResult, report.Name),
            _ => throw new InvalidOperationException($"Unsupported export format: {request.Format}"),
        };
    }

    private static ExportReportResult ExportToCsv(RunReportResponseModel result, string reportName)
    {
        var sb = new System.Text.StringBuilder();

        if (result.Rows.Count > 0)
        {
            var headers = result.Rows[0].Keys;
            sb.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

            foreach (var row in result.Rows)
            {
                var values = headers.Select(h => row.TryGetValue(h, out var v) ? $"\"{v?.ToString()?.Replace("\"", "\"\"")}\"" : "\"\"");
                sb.AppendLine(string.Join(",", values));
            }
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return new ExportReportResult(bytes, "text/csv", $"{reportName}.csv");
    }

    private static ExportReportResult ExportToXlsx(RunReportResponseModel result, string reportName)
    {
        // TODO: Implement using ClosedXML once package is added
        // using var workbook = new ClosedXML.Excel.XLWorkbook();
        // var worksheet = workbook.Worksheets.Add(reportName);
        // ... populate worksheet rows ...
        // using var stream = new System.IO.MemoryStream();
        // workbook.SaveAs(stream);
        // return new ExportReportResult(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}.xlsx");

        throw new NotImplementedException("XLSX export requires ClosedXML NuGet package. Add <PackageReference Include=\"ClosedXML\" Version=\"0.104.1\" /> to qb-engineer.api.csproj.");
    }

    private static ExportReportResult ExportToPdf(RunReportResponseModel result, string reportName)
    {
        // TODO: Implement using QuestPDF
        throw new NotImplementedException("PDF export not yet implemented for report builder. Use QuestPDF to generate a table-based PDF.");
    }
}
