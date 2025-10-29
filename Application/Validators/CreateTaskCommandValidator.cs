// RealEstate.Application/Validators/CreateTaskCommandValidator.cs
using FluentValidation;
using RealEstate.Application.Commands.Tasks;

namespace RealEstate.Application.Validators;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.TaskType)
            .IsInEnum().WithMessage("Invalid task type.");

        RuleFor(x => x.AssignedTo)
            .NotEmpty().WithMessage("Assigned user is required.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.");
    }
}