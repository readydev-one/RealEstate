// RealEstate.Application/Validators/CreatePropertyCommandValidator.cs
using FluentValidation;
using RealEstate.Application.Commands.Properties;

namespace RealEstate.Application.Validators;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.Request.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(x => x.Request.PurchasePrice)
            .GreaterThan(0).WithMessage("Purchase price must be greater than 0.");

        RuleFor(x => x.Request.ClosingDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Closing date must be in the future.");

        RuleFor(x => x.Request.AnnualPropertyTax)
            .GreaterThanOrEqualTo(0).WithMessage("Annual property tax must be non-negative.");

        RuleFor(x => x.Request.MonthlyInsurance)
            .GreaterThanOrEqualTo(0).WithMessage("Monthly insurance must be non-negative.");

        RuleFor(x => x.Request.EscrowMonths)
            .InclusiveBetween(1, 24).WithMessage("Escrow months must be between 1 and 24.");
    }
}