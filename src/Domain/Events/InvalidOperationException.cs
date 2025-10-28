// RealEstate.Domain/Exceptions/InvalidOperationException.cs
namespace RealEstate.Domain.Exceptions;

public class InvalidOperationException : DomainException
{
    public InvalidOperationException(string message) : base(message) { }
}