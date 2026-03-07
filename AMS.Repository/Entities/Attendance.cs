namespace AMS.Repository.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string Status { get; set; } = null!; // Present, Absent, Late, LeaveApproved
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Employee? Employee { get; set; }
    }
}
