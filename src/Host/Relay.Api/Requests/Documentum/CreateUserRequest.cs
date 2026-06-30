using System.Text.Json.Serialization;
using Relay.Api.Converters;

namespace Relay.Api.Requests.Documentum;

public sealed record CreateUserRequest(
    string GlobalId,
    string FirstName,
    string LastName,
    string? EmailId,
    int? BrandId,
    [property: JsonConverter(typeof(NumericStringJsonConverter))] string? QueueId,   // comma-separated e.g. "5,11"
    int? RoleId,
    string CreatedBy);
