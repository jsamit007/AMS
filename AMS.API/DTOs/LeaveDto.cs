namespace AMS.API.DTOs
{
    public class LeaveDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } // Sick, Casual, Earned, Unpaid
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int NumberOfDays { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class CreateLeaveDto
    {
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; }
    }

    public class UpdateLeaveDto
    {
        public string? Status { get; set; }
        public int? ApprovedBy { get; set; }
    }
}
