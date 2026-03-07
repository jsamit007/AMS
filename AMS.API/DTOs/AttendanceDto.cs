namespace AMS.API.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string Status { get; set; } // Present, Absent, Late, LeaveApproved
        public string? Remarks { get; set; }
    }

    public class CreateAttendanceDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public TimeSpan? CheckOutTime { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }
}
