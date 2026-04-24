using System;
using Relay.Intranet.Domain.Aggregates;

namespace Relay.Intranet.Application.Tests.Builders;

internal static class UserBuilder
{
    public static User Build(
        Guid? id = null,
        string displayName = "Test User",
        string email = "testuser@adticorp.com",
        bool isActive = true,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? deactivatedAt = null) =>
        User.Reconstitute(
            id ?? Guid.NewGuid(),
            displayName,
            email,
            isActive,
            createdAt ?? DateTimeOffset.UtcNow,
            deactivatedAt);
}
