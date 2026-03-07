namespace AMS.Repository.Entities
{
    public class Leave
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = null!; // Sick, Casual, Earned, Unpaid
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int NumberOfDays { get; set; }
        public string Reason { get; set; } = null!;
        public string Status { get; set; } = null!; // Pending, Approved, Rejected
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Employee? Employee { get; set; }
        public virtual Employee? ApprovedByEmployee { get; set; }
    }
}
