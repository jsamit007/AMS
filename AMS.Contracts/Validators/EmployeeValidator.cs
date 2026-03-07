using AMS.Contracts.DTOs;
using FluentValidation;

namespace AMS.Contracts.Validators
{
    public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeDtoValidator()
        {
            RuleFor(x => x.EmployeeCode)
                .NotEmpty()
                .WithMessage("Employee code is required")
                .MaximumLength(20)
                .WithMessage("Employee code cannot exceed 20 characters")
                .Matches(@"^[A-Z0-9]+$")
                .WithMessage("Employee code must contain only uppercase letters and numbers");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Email must be a valid email address")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Matches(@"^\+?[\d\s\-\(\)]{7,}$")
                .WithMessage("Phone number must be valid");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Department ID must be greater than 0");

            RuleFor(x => x.Designation)
                .NotEmpty()
                .WithMessage("Designation is required")
                .MaximumLength(100)
                .WithMessage("Designation cannot exceed 100 characters");

            RuleFor(x => x.JoiningDate)
                .NotEmpty()
                .WithMessage("Joining date is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Joining date cannot be in the future");
        }
    }

    public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
    {
        public UpdateEmployeeDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[\d\s\-\(\)]{7,}$")
                .WithMessage("Phone number must be valid")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Department ID must be greater than 0")
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x.Designation)
                .MaximumLength(100)
                .WithMessage("Designation cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Designation));
        }
    }
}
