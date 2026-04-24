namespace Relay.Documentum.Domain.Exceptions;

public class DocumentumDomainException : Exception
{
    public DocumentumDomainException(string message) : base(message) { }
    public DocumentumDomainException(string message, Exception inner) : base(message, inner) { }
}
