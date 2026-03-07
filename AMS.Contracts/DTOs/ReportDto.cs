namespace AMS.Contracts.DTOs
{
    public class AttendanceReportDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LateDays { get; set; }
        public decimal AttendancePercentage { get; set; }
    }

    public class DepartmentAttendanceReportDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalEmployees { get; set; }
        public int AverageAttendancePercentage { get; set; }
        public List<AttendanceReportDto> EmployeeReports { get; set; }
    }

    public class LeaveBalanceDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public int TotalDaysAllowed { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
    }
}
