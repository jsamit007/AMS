using AMS.Contracts.DTOs;
using FluentValidation;

namespace AMS.Contracts.Validators
{
    public class CreateLeaveDtoValidator : AbstractValidator<CreateLeaveDto>
    {
        public CreateLeaveDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0)
                .WithMessage("Employee ID must be greater than 0");

            RuleFor(x => x.LeaveType)
                .NotEmpty()
                .WithMessage("Leave type is required")
                .Must(x => new[] { "Sick", "Casual", "Earned", "Unpaid" }.Contains(x))
                .WithMessage("Leave type must be one of: Sick, Casual, Earned, Unpaid");

            RuleFor(x => x.FromDate)
                .NotEmpty()
                .WithMessage("From date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("From date must be today or in the future");

            RuleFor(x => x.ToDate)
                .NotEmpty()
                .WithMessage("To date is required")
                .GreaterThanOrEqualTo(x => x.FromDate)
                .WithMessage("To date must be greater than or equal to from date");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Reason is required")
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters");
        }
    }

    public class UpdateLeaveDtoValidator : AbstractValidator<UpdateLeaveDto>
    {
        public UpdateLeaveDtoValidator()
        {
            RuleFor(x => x.Status)
                .Must(x => new[] { "Pending", "Approved", "Rejected" }.Contains(x))
                .WithMessage("Status must be one of: Pending, Approved, Rejected")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.ApprovedBy)
                .GreaterThan(0)
                .WithMessage("Approver ID must be greater than 0")
                .When(x => x.ApprovedBy.HasValue);
        }
    }
}
