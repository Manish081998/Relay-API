using Microsoft.Extensions.Options;
using Relay.Api.Settings;

namespace Relay.Api.Services;

public interface IDocumentPathBuilder
{
    string BuildRelativeFolder(string brandName, DateTime orderDate, int orderSeq);
    string BuildAbsoluteFolder(string brandName, DateTime orderDate, int orderSeq);
}

internal sealed class DocumentPathBuilder : IDocumentPathBuilder
{
    private readonly string _basePath;

    public DocumentPathBuilder(IOptions<FileStorageSettings> settings)
    {
        _basePath = settings?.Value?.BasePath
            ?? throw new ArgumentNullException(nameof(settings));
    }

    public string BuildRelativeFolder(string brandName, DateTime orderDate, int orderSeq)
    {
        var safeBrand = DocumentFileHelper.SanitizeFileName(brandName);
        return Path.Combine(safeBrand, orderDate.ToString("yyyy"), orderDate.ToString("MM"), orderDate.ToString("dd"), orderSeq.ToString());
    }

    public string BuildAbsoluteFolder(string brandName, DateTime orderDate, int orderSeq)
    {
        return Path.Combine(_basePath, BuildRelativeFolder(brandName, orderDate, orderSeq));
    }
}
