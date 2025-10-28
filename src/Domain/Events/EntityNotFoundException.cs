// RealEstate.Domain/Exceptions/EntityNotFoundException.cs
namespace RealEstate.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, string entityId)
        : base($"{entityName} with ID '{entityId}' was not found.") { }
}
