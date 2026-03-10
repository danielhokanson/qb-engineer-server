namespace QBEngineer.Core.Interfaces;

public interface ICsvExportService
{
    byte[] Export<T>(IEnumerable<T> records);
    Stream ExportToStream<T>(IEnumerable<T> records);
}
