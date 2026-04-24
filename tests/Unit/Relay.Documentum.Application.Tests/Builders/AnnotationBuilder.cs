using Relay.Documentum.Domain.Aggregates;

namespace Relay.Documentum.Application.Tests.Builders;

internal static class AnnotationBuilder
{
    public static Annotation Build(
        int id = 1,
        string path = "/documentum/store/test.pdf",
        string createdBy = "testuser@adticorp.com") =>
        Annotation.Reconstitute(id, path, createdBy);
}
