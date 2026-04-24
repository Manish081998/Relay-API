using Relay.SharedKernel.Common;
using Relay.SharedKernel.Domain;

namespace Relay.WebTool.Domain.Entities;

/// <summary>
/// Child entity of the Selection aggregate.
/// </summary>
public sealed class SelectionOption : Entity<Guid>
{
    public string Label { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public int DisplayOrder { get; private set; }

    private SelectionOption() { }

    internal SelectionOption(Guid id, string label, string value, int displayOrder) : base(id)
    {
        Label = Guard.NotNullOrWhiteSpace(label);
        Value = Guard.NotNullOrWhiteSpace(value);
        DisplayOrder = Guard.NonNegative(displayOrder);
    }
}
