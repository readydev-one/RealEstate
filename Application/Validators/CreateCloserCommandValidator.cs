// RealEstate.Application/Validators/CreateCloserCommandValidator.cs
using FluentValidation;
using RealEstate.Application.Commands.Users;

namespace RealEstate.Application.Validators;

public class CreateCloserCommandValidator : AbstractValidator<CreateCloserCommand>
{
    public CreateCloserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.Agency)
            .NotEmpty().WithMessage("Agency is required.")
            .MaximumLength(200).WithMessage("Agency must not exceed 200 characters.");
    }
}