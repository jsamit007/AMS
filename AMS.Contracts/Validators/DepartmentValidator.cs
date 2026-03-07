using AMS.Contracts.DTOs;
using FluentValidation;

namespace AMS.Contracts.Validators
{
    public class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
    {
        public CreateDepartmentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Department name is required")
                .MaximumLength(100)
                .WithMessage("Department name cannot exceed 100 characters");

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Department code is required")
                .MaximumLength(20)
                .WithMessage("Department code cannot exceed 20 characters")
                .Matches(@"^[A-Z0-9]+$")
                .WithMessage("Department code must contain only uppercase letters and numbers");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ManagerId)
                .GreaterThan(0)
                .WithMessage("Manager ID must be greater than 0")
                .When(x => x.ManagerId.HasValue);
        }
    }

    public class UpdateDepartmentDtoValidator : AbstractValidator<UpdateDepartmentDto>
    {
        public UpdateDepartmentDtoValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                .WithMessage("Department name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ManagerId)
                .GreaterThan(0)
                .WithMessage("Manager ID must be greater than 0")
                .When(x => x.ManagerId.HasValue);
        }
    }
}
