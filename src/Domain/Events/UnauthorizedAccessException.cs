// RealEstate.Domain/Exceptions/UnauthorizedAccessException.cs
namespace RealEstate.Domain.Exceptions;

public class UnauthorizedAccessException : DomainException
{
    public UnauthorizedAccessException(string message) : base(message) { }
}
