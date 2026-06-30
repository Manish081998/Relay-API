namespace Relay.Api.Services;

internal static class DocumentFileHelper
{
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "document";

        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalid.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "document" : sanitized;
    }

    public static string GetContentTypeCategory(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => "PDF",
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".tiff" or ".tif" or ".svg" => "Image",
            ".doc" or ".docx" => "Word",
            ".xls" or ".xlsx" => "Excel",
            ".txt" => "Text",
            _ => "Other",
        };

    public static string GetMimeType(string filePath) =>
        Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            ".svg" => "image/svg+xml",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream",
        };

    public static string BuildDocumentDisplayName(string originalFileName, string? repPO, bool isSupportDoc)
    {
        if (isSupportDoc || string.IsNullOrWhiteSpace(repPO))
            return originalFileName;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
        var ext = Path.GetExtension(originalFileName);
        var dateSuffix = DateTime.Now.ToString("MMddyyyy");
        return $"PO{repPO}_{nameWithoutExt}_{dateSuffix}{ext}";
    }

    public static string ExtractOriginalName(string versionFileName)
    {
        var underscoreIdx = versionFileName.IndexOf('_');
        return underscoreIdx >= 0 ? versionFileName[(underscoreIdx + 1)..] : versionFileName;
    }
}
