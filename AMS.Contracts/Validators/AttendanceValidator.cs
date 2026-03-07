using AMS.Contracts.DTOs;
using FluentValidation;

namespace AMS.Contracts.Validators
{
    public class CreateAttendanceDtoValidator : AbstractValidator<CreateAttendanceDto>
    {
        public CreateAttendanceDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0)
                .WithMessage("Employee ID must be greater than 0");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date is required")
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Date cannot be in the future");

            RuleFor(x => x.CheckInTime)
                .NotEmpty()
                .WithMessage("Check-in time is required");

            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status is required")
                .NotNull()
                .Must(s => new[] { "Present", "Absent", "Late", "LeaveApproved" }.Contains(s))
                .WithMessage("Status must be one of: Present, Absent, Late, LeaveApproved");

            RuleFor(x => x.Remarks)
                .MaximumLength(500)
                .WithMessage("Remarks cannot exceed 500 characters");

            // CheckOutTime is optional but if provided, must be after CheckInTime
            RuleFor(x => x.CheckOutTime)
                .GreaterThan(x => x.CheckInTime)
                .WithMessage("Check-out time must be after check-in time")
                .When(x => x.CheckOutTime.HasValue);
        }
    }

    public class UpdateAttendanceDtoValidator : AbstractValidator<UpdateAttendanceDto>
    {
        public UpdateAttendanceDtoValidator()
        {
            RuleFor(x => x.Status)
                .Must(s => new[] { "Present", "Absent", "Late", "LeaveApproved" }.Contains(s))
                .WithMessage("Status must be one of: Present, Absent, Late, LeaveApproved")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.Remarks)
                .MaximumLength(500)
                .WithMessage("Remarks cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Remarks));
        }
    }
}
