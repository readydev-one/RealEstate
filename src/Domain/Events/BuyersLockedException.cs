// RealEstate.Domain/Exceptions/BuyersLockedException.cs
namespace RealEstate.Domain.Exceptions;

public class BuyersLockedException : DomainException
{
    public BuyersLockedException(string propertyId)
        : base($"Buyers are locked for property '{propertyId}' because down payment has been accepted.") { }
}