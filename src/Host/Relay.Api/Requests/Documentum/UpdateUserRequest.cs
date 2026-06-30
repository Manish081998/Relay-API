using System.Text.Json.Serialization;
using Relay.Api.Converters;

namespace Relay.Api.Requests.Documentum;

public sealed record UpdateUserRequest(
    string GlobalId,
    int? BrandId,
    [property: JsonConverter(typeof(NumericStringJsonConverter))] string? QueueId,   // comma-separated e.g. "5,11"
    int? RoleId,
    string UpdatedBy);