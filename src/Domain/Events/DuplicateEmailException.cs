// RealEstate.Domain/Exceptions/DuplicateEmailException.cs
namespace RealEstate.Domain.Exceptions;

public class DuplicateEmailException : DomainException
{
    public DuplicateEmailException(string email)
        : base($"A user with email '{email}' already exists.") { }
}