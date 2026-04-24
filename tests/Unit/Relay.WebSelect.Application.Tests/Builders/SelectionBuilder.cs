using System;
using Relay.WebTool.Domain.Aggregates;

namespace Relay.WebTool.Application.Tests.Builders;

internal static class SelectionBuilder
{
    public static Selection Build(
        Guid? id = null,
        string title = "Test Selection",
        Guid? submittedBy = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? submittedAt = null)
    {
        var selection = Selection.Reconstitute(
            id ?? Guid.NewGuid(),
            title,
            submittedBy ?? Guid.NewGuid(),
            createdAt ?? DateTimeOffset.UtcNow,
            submittedAt);

        selection.RehydrateOption(Guid.NewGuid(), "Option A", "A", 1);
        selection.RehydrateOption(Guid.NewGuid(), "Option B", "B", 2);

        return selection;
    }
}
