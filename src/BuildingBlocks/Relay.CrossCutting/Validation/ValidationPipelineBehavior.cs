using FluentValidation;

namespace Relay.CrossCutting.Validation;

/// <summary>
/// Convenience wrapper that runs a FluentValidation validator before invoking a handler.
/// Throws <see cref="ValidationException"/> when validation fails; the global exception
/// handler converts that to HTTP 400.
/// </summary>
public sealed class ValidationPipeline<TRequest>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipeline(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    public async Task EnsureValidAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        if (!_validators.Any())
        {
            return;
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }
}
